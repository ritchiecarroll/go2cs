using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using runtime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// A converted process started with a signal INHERITED as SIG_IGN, on the linux and darwin signal bridges
/// (runtime/linux/signal_posix_impl.cs, runtime/darwin/signal_posix_darwin_impl.cs).
/// </summary>
/// <remarks>
/// <para>
/// FOUND 2026-11-08 by the 1.26.10.3 linux re-bank: os/signal and os DIVERGED because the kit's host
/// inherited SIGINT ignored (an async `cmd &amp;` in a non-interactive bash starts with SIGINT and SIGQUIT
/// ignored). Go's signal.Notify overrides an inherited SIG_IGN; the converted host died by the very SIGINT
/// it had registered for. strace of the pair: with SIGINT inherited ignored, .NET read the disposition
/// (SIG_IGN) and installed nothing, the bridge's signal(SIGINT, SIG_DFL) cleared it, and no rt_sigaction
/// followed -- "killed by SIGINT"; with SIGINT at its default, .NET installed its SA_SIGINFO handler and
/// the kill was delivered.
/// </para>
/// <para>
/// THE GUARD, dotnet/dotnet 0427b9abbead (the 10.0.14 source; src/runtime/src/native/libs/System.Native/
/// pal_signal.c): InitializeSignalHandlingCore installs SIGINT, SIGQUIT and SIGCONT at START-UP under
/// HAS_CONSOLE_SIGNALS, and InstallSignalHandler, finding SIG_IGN, marks the signal installed and returns
/// without installing -- so every later PosixSignalRegistration.Create of that signal installs nothing.
/// Every other signal is installed lazily at its first Create, after the bridge's clear, which is why
/// SIGHUP worked and the start-up three did not.
/// </para>
/// <para>
/// Each arm is a CHILD re-run of this assembly (GodebugUpdateHookTests' pattern), started through
/// `/bin/sh -c 'trap "" SIG; exec dotnet test ...'`: the child inherits the ignore through the muxer,
/// vstest and the test host (.NET's Process.Start restores a signal it never installed to its inherited
/// disposition). The child records its own PRECONDITION -- runtime.signal_ignored at start, which the
/// bridge seeds from the inherited disposition -- before it does anything that can kill it, so an arm
/// whose harness did not pass the ignore down fails as a broken instrument, never as a pass.
/// </para>
/// </remarks>
[TestClass]
public class InheritedIgnoredSignalTests
{
    private const string RecordVariable = "G2CS_INHERITED_SIG_RECORD";
    private const string SignalVariable = "G2CS_INHERITED_SIG_NUMBER";
    private const string ModeVariable = "G2CS_INHERITED_SIG_MODE";

    private static bool Darwin => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private const int SIGHUP = 1, SIGINT = 2, SIGQUIT = 3;
    private static int SIGCONT => Darwin ? 19 : 18;

    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int sig);

    [DllImport("libc")]
    private static extern int getpid();

    private static void EnsureRuntimeInitialized() =>
        RuntimeHelpers.RunModuleConstructor(typeof(runtime).Module.ModuleHandle);

    private static void Record(string path, string line) => File.AppendAllText(path, line + "\n");

    // THE CHILD. A no-op in an ordinary run (no record variable); in a child it notes its precondition,
    // optionally Notifies, sends the signal to itself and records what happened. Modes:
    //   notify   signal_enable, send, wait up to 5 s for signal_recv to return it
    //   ignore   send without any Notify, then live 1 s (Go keeps an inherited SIG_IGN until Notify)
    //   stop     notify + deliver, then signal_disable (Go's Stop restores the inherited SIG_IGN) and
    //            send again, then live 1 s
    [TestMethod]
    public void ChildSignalProbe()
    {
        string? record = Environment.GetEnvironmentVariable(RecordVariable);

        if (string.IsNullOrEmpty(record))
            return;

        uint sig = uint.Parse(Environment.GetEnvironmentVariable(SignalVariable)!);
        string mode = Environment.GetEnvironmentVariable(ModeVariable)!;

        EnsureRuntimeInitialized();
        Record(record, $"inherited_ignored={runtime.signal_ignored(sig)}");

        uint received = 0;
        using ManualResetEventSlim got = new();

        if (mode is "notify" or "stop")
        {
            runtime.signal_enable(sig);
            Thread receiver = new(() =>
            {
                received = runtime.signal_recv();
                got.Set();
            }) { IsBackground = true };
            receiver.Start();
        }

        Record(record, "sending=1");
        kill(getpid(), (int)sig);

        if (mode is "notify" or "stop")
            Record(record, got.Wait(5000) ? $"delivered={received}" : "delivered=none");
        else
            Thread.Sleep(1000);

        if (mode == "stop")
        {
            runtime.signal_disable(sig);
            Record(record, "resending=1");
            kill(getpid(), (int)sig);
            Thread.Sleep(1000);
        }

        Record(record, "survived=1");
    }

    private sealed record ChildResult(Dictionary<string, string> Lines, int ExitCode, string Output)
    {
        public string Get(string key) => Lines.TryGetValue(key, out string? value) ? value : "(absent)";
    }

    // Runs ChildSignalProbe in a child test host whose `sig` is inherited IGNORED (ignore true) or as the
    // parent passes it (ignore false), and returns what it recorded.
    private static ChildResult RunChild(int sig, string trapName, bool ignore, string mode)
    {
        string record = Path.Combine(Path.GetTempPath(), "g2cs-sigign-" + Guid.NewGuid().ToString("N") + ".txt");
        string filter = $"FullyQualifiedName={typeof(InheritedIgnoredSignalTests).FullName}.{nameof(ChildSignalProbe)}";
        string script = (ignore ? $"trap '' {trapName}; " : "") + "exec \"$@\"";

        ProcessStartInfo start = new("/bin/sh", ["-c", script, "sh", "dotnet", "test", typeof(InheritedIgnoredSignalTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[RecordVariable] = record;
        start.Environment[SignalVariable] = sig.ToString();
        start.Environment[ModeVariable] = mode;

        try
        {
            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(180_000))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail("the signal child did not finish in 180 s");
            }

            Dictionary<string, string> lines = new(StringComparer.Ordinal);

            if (File.Exists(record))
            {
                foreach (string line in File.ReadAllLines(record))
                {
                    int eq = line.IndexOf('=');
                    if (eq > 0)
                        lines[line[..eq]] = line[(eq + 1)..];
                }
            }

            return new ChildResult(lines, process.ExitCode, stdout.Result + stderr.Result);
        }
        finally
        {
            File.Delete(record);
        }
    }

    // The instrument's own check, before any verdict: the child must have STARTED and seen the inherited
    // disposition the arm asked for, or the arm measures nothing.
    private static void AssertPrecondition(ChildResult child, bool inheritedIgnored, string arm)
    {
        Assert.AreNotEqual("(absent)", child.Get("inherited_ignored"),
            $"{arm}: the child recorded no precondition -- the harness never reached the probe (exit {child.ExitCode}):\n{child.Output}");

        if (!inheritedIgnored && child.Get("inherited_ignored") == "True")
            Assert.Inconclusive($"{arm}: this test host itself inherited the signal ignored, so the default-disposition control cannot be established");

        Assert.AreEqual(inheritedIgnored.ToString(), child.Get("inherited_ignored"),
            $"{arm}: the child must start with the signal {(inheritedIgnored ? "inherited ignored" : "at its default")}; anything else is a broken instrument");
    }

    private static void SkipOffUnix()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Darwin)
            Assert.Inconclusive("the inherited-disposition bridge exists on the linux and darwin flavours only");
    }

    [TestMethod]
    public void SigintInheritedIgnoredIsDeliveredAfterNotify()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGINT, "INT", ignore: true, mode: "notify");
        AssertPrecondition(child, inheritedIgnored: true, "SIGINT notify");

        Assert.AreEqual(SIGINT.ToString(), child.Get("delivered"),
            $"Go's signal.Notify overrides an inherited SIG_IGN and delivers SIGINT; the converted child must too, not die by it (exit {child.ExitCode}):\n{child.Output}");
        Assert.AreEqual("1", child.Get("survived"), "the child must survive a SIGINT it Notified for");
    }

    [TestMethod]
    public void SigintInheritedIgnoredWithoutNotifyIsSurvived()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGINT, "INT", ignore: true, mode: "ignore");
        AssertPrecondition(child, inheritedIgnored: true, "SIGINT no-notify");

        Assert.AreEqual("1", child.Get("survived"),
            $"Go's initsig keeps an inherited SIG_IGN on SIGINT until Notify, so a Go program survives this SIGINT; the converted child must too (exit {child.ExitCode}):\n{child.Output}");
    }

    [TestMethod]
    public void SigintInheritedIgnoredIsIgnoredAgainAfterStop()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGINT, "INT", ignore: true, mode: "stop");
        AssertPrecondition(child, inheritedIgnored: true, "SIGINT stop");

        Assert.AreEqual(SIGINT.ToString(), child.Get("delivered"), $"the Notified SIGINT must be delivered first (exit {child.ExitCode}):\n{child.Output}");
        Assert.AreEqual("1", child.Get("survived"),
            $"after Stop, Go restores the inherited SIG_IGN (sigdisable's fwdSig arm), so the second SIGINT is ignored; the converted child must survive it (exit {child.ExitCode}):\n{child.Output}");
    }

    [TestMethod]
    public void SigquitInheritedIgnoredIsDeliveredAfterNotify()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGQUIT, "QUIT", ignore: true, mode: "notify");
        AssertPrecondition(child, inheritedIgnored: true, "SIGQUIT notify");

        Assert.AreEqual(SIGQUIT.ToString(), child.Get("delivered"),
            $"SIGQUIT is in .NET's start-up three; Notify must deliver it over an inherited SIG_IGN (exit {child.ExitCode}):\n{child.Output}");
        Assert.AreEqual("1", child.Get("survived"), "the child must survive a SIGQUIT it Notified for");
    }

    [TestMethod]
    public void SigcontInheritedIgnoredIsDeliveredAfterNotify()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGCONT, "CONT", ignore: true, mode: "notify");
        AssertPrecondition(child, inheritedIgnored: true, "SIGCONT notify");

        Assert.AreEqual(SIGCONT.ToString(), child.Get("delivered"),
            $"SIGCONT is in .NET's start-up three; Notify must deliver it over an inherited SIG_IGN (its default action continues, so the defect reads as a lost delivery, not a death) (exit {child.ExitCode}):\n{child.Output}");
    }

    // CONTROLS, green before and after the fix: the default disposition, and SIGHUP -- inherited ignored
    // like the arms above, but installed LAZILY by .NET, so the bridge's clear-then-Create already serves it.
    [TestMethod]
    public void SigintAtItsDefaultIsDeliveredAfterNotify()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGINT, "INT", ignore: false, mode: "notify");
        AssertPrecondition(child, inheritedIgnored: false, "SIGINT default control");

        Assert.AreEqual(SIGINT.ToString(), child.Get("delivered"), $"control: a default-disposition SIGINT is delivered after Notify (exit {child.ExitCode}):\n{child.Output}");
    }

    [TestMethod]
    public void SighupInheritedIgnoredIsDeliveredAfterNotify()
    {
        SkipOffUnix();
        ChildResult child = RunChild(SIGHUP, "HUP", ignore: true, mode: "notify");
        AssertPrecondition(child, inheritedIgnored: true, "SIGHUP control");

        Assert.AreEqual(SIGHUP.ToString(), child.Get("delivered"), $"control: SIGHUP is installed lazily, so its inherited ignore was always overridden by Notify (exit {child.ExitCode}):\n{child.Output}");
    }
}
