using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using runtime = go.runtime_package;
using syscall = go.syscall_package;

namespace GolibTests;

/// <summary>
/// Go's SIGPIPE model on the linux flavour, under a CLR that ignores SIGPIPE itself.
/// </summary>
/// <remarks>
/// <para>
/// MEASURED 2026-09-26 on the WSL arm, before any of this: .NET sets SIGPIPE to SIG_IGN at startup
/// (rt_sigaction, old handler SIG_DFL) and its own socket sends carry no MSG_NOSIGNAL, so the process
/// must KEEP that disposition -- with SIG_DFL a .NET Socket.Send to a closed peer kills the process.
/// Go's model has to be given back without ever changing it for a living process.
/// </para>
/// <para>
/// Three defects, three arms. (1) The signal bridge seeded Go's "inherited ignored" set from
/// dispositions read at module init -- after the CLR's SIG_IGN -- so SIGPIPE read as ignored in every
/// converted process, and runtime.sigpipe returned instead of killing the program on EPIPE to
/// stdout or stderr. Go installs its own handler for SIGPIPE whatever it inherited
/// (sigInstallGoHandler respects an inherited SIG_IGN only for SIGHUP and SIGINT), so SIGPIPE is
/// never seeded. (2) dieFromSignal is exercised out of process -- it ends the process -- by os's
/// TestStdPipe. (3) The posix_spawn seam relied on exec resetting CAUGHT handlers, but the CLR's
/// SIGPIPE is IGNORED and exec preserves that, so every child inherited SIGPIPE ignored where a Go
/// parent passes the default; after signal.Ignore(SIGPIPE) Go's child does inherit SIG_IGN.
/// </para>
/// </remarks>
[TestClass]
public class LinuxSigpipeTests
{
    private const int SIGPIPE = 13;

    // The kernel's view, never the runtime's: bit (sig - 1) of /proc/<pid>/status's SigIgn mask.
    private static bool KernelIgnores(int sig)
    {
        string line = File.ReadAllLines("/proc/self/status").First(l => l.StartsWith("SigIgn:", StringComparison.Ordinal));
        return (Convert.ToUInt64(line.Split('\t')[1].Trim(), 16) & (1UL << (sig - 1))) != 0;
    }

    private static void EnsureRuntimeInitialized() =>
        RuntimeHelpers.RunModuleConstructor(typeof(runtime).Module.ModuleHandle);

    [TestMethod]
    public void SigpipeIsNotSeededAsInheritedIgnored()
    {
        EnsureRuntimeInitialized();

        // The control: the scenario is real -- the kernel disposition IS ignored, by the CLR.
        Assert.IsTrue(KernelIgnores(SIGPIPE), "the CLR is expected to hold SIGPIPE at SIG_IGN; without that this arm tests nothing");

        Assert.IsFalse(runtime.signal_ignored(SIGPIPE),
            "SIGPIPE must not read as ignored in Go's model: Go installs its own handler for SIGPIPE whatever it inherited, so an EPIPE on stdout or stderr kills the program; the SIG_IGN here is the CLR's, not the parent's");
    }

    [TestMethod]
    public void ADotNetSocketSendToAClosedPeerStillSurvives()
    {
        // MUST-NOT-REGRESS (green before and after the cut): nothing may take the CLR's SIG_IGN away
        // from a living process, because .NET's socket sends rely on it.
        EnsureRuntimeInitialized();
        Assert.IsTrue(KernelIgnores(SIGPIPE), "a living converted process must keep the CLR's SIGPIPE disposition");

        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        using TcpClient client = new();
        client.Connect((IPEndPoint)listener.LocalEndpoint);
        listener.AcceptSocket().Close();
        listener.Stop();

        SocketException? refused = null;

        try
        {
            for (int i = 0; i < 20 && refused is null; i++)
            {
                client.Client.Send(new byte[65536]);
                System.Threading.Thread.Sleep(20);
            }
        }
        catch (SocketException ex)
        {
            refused = ex;
        }

        Assert.IsNotNull(refused, "a send to a closed peer must fail with EPIPE/ECONNRESET as a SocketException -- and reaching this line is the survival");
    }

    [TestMethod]
    public void ASpawnedChildStartsWithSigpipeAtItsDefault()
    {
        EnsureRuntimeInitialized();
        Assert.IsFalse(runtime.signal_ignored(SIGPIPE), "precondition: the program has not ignored SIGPIPE");
        Assert.AreEqual(0, ChildIgnoresSigpipe(), "a child of a program that has not ignored SIGPIPE starts with SIGPIPE at SIG_DFL, as under Go (whose handler exec resets); the CLR's SIG_IGN must not leak into it");
    }

    [TestMethod]
    public void AfterIgnoreTheChildInheritsSigpipeIgnored()
    {
        EnsureRuntimeInitialized();
        bool was = runtime.signal_ignored(SIGPIPE);

        try
        {
            runtime.signal_ignore(SIGPIPE);
            Assert.AreEqual(1, ChildIgnoresSigpipe(), "after signal.Ignore(SIGPIPE) Go's child inherits SIG_IGN (exec keeps ignored dispositions)");
        }
        finally
        {
            // Restored BY VALUE through Go's own API: signal_enable is what clears an ignore (Reset does
            // not), and for SIGPIPE the bridge's sigenable installs nothing, so enable + disable leaves
            // exactly the prior state.
            if (!was)
            {
                runtime.signal_enable(SIGPIPE);
                runtime.signal_disable(SIGPIPE);
            }
        }

        Assert.AreEqual(was, runtime.signal_ignored(SIGPIPE), "the arm must leave the process's signal state as it found it");
    }

    // Spawns /bin/sh through the converted posix_spawn seam and returns the child's SIGPIPE
    // SigIgn bit as its exit status.
    private static int ChildIgnoresSigpipe()
    {
        syscall.ProcAttr attr = new() { Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }) };

        var (pid, _, err) = syscall.StartProcess(
            "/bin/sh"u8,
            new slice<@string>(new @string[] { "/bin/sh"u8, "-c"u8, "set -- $(grep SigIgn /proc/self/status); exit $(( (0x$2 >> 12) & 1 ))"u8 }),
            new StandardBox<syscall.ProcAttr>(attr));

        Assert.IsNull(err, $"spawning /bin/sh failed: {err}");

        ref var status = ref heap(new syscall.WaitStatus(), out var Ꮡstatus);
        var (waited, werr) = syscall.Wait4(pid, Ꮡstatus, 0, nil);

        Assert.IsNull(werr, $"Wait4 failed: {werr}");
        Assert.AreEqual(pid, waited);
        Assert.IsTrue(status.Exited(), "the probe child must exit normally");

        return (int)status.ExitStatus();
    }
}
