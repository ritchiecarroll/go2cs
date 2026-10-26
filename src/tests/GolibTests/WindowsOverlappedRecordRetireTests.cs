using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

#nullable enable

namespace GolibTests;

// Guards the retirement of syscall's overlapped operation record (OverlappedOp, the windows flavour's
// zsyscall_windows_wsa_impl.cs), the defect behind "Handle is not initialized" out of net's Close.
//
// Go 1.26 pools internal/poll's operations, so a record is retired by TWO paths that can run at once:
// a submit whose pooled key now names another (descriptor, mode), through
// GoAsyncIO.GetOrBindOperationState, and its socket's pollClose, through SocketBinding.Dispose. The
// record is private to syscall, so these arms reach it by reflection; nothing in the converted surface
// can hold two retirements open at once.
//
// What each arm reads on the tree before the fix:
//   ARearmOnARetiredRecordPanicsByName       RED, deterministically: the submit allocates on a disposed
//                                            PreAllocatedOverlapped and throws ObjectDisposedException.
//   TwoRetirementsAtOnceFreeTheBlockOnce     red only when the two threads interleave inside Dispose: a
//                                            race, so a green run on that tree does not clear it.
//   APollCloseSnapshotWaitsForTheRetirement  RED by name: the tree has no per-record gate to hold.
//
// Windows only by nature (ThreadPoolBoundHandle binds a real socket to the CLR's completion port), and
// gated on the HOST: elsewhere the arms read NOT MEASURED.
[TestClass]
[DoNotParallelize]
public class WindowsOverlappedRecordRetireTests
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [TestMethod]
    public void ARearmOnARetiredRecordPanicsByName()
    {
        using Binding binding = Binding.Open();
        object record = binding.NewRecord('r');

        binding.Rearm(record);
        ((IDisposable)record).Dispose();

        Exception? thrown = null;

        try
        {
            binding.Rearm(record);
        }
        catch (TargetInvocationException ex)
        {
            thrown = ex.InnerException;
        }

        Assert.IsInstanceOfType(thrown, typeof(PanicException), $"a submit on a retired record threw {thrown?.GetType().Name ?? "nothing"}: {thrown?.Message}");
        StringAssert.Contains(thrown!.Message, "retired operation record");
        StringAssert.Contains(thrown.Message, "mode 'r'");
    }

    [TestMethod]
    public void TwoRetirementsAtOnceFreeTheBlockOnce()
    {
        const int records = 2000;

        using Binding binding = Binding.Open();
        int failures = 0;
        string? first = null;

        for (int i = 0; i < records; i++)
        {
            object record = binding.NewRecord('w');
            binding.Rearm(record);

            using Barrier barrier = new(2);

            void retire()
            {
                barrier.SignalAndWait();

                try
                {
                    ((IDisposable)record).Dispose();
                }
                catch (Exception ex)
                {
                    if (Interlocked.Increment(ref failures) == 1)
                        first = $"{ex.GetType().Name}: {ex.Message}";
                }
            }

            Thread other = new(retire);
            other.Start();
            retire();
            other.Join();
        }

        Assert.AreEqual(0, failures, $"{failures} of {records} double retirements threw; first: {first}");
    }

    [TestMethod]
    public void APollCloseSnapshotWaitsForTheRetirement()
    {
        Binding binding = Binding.Open();
        object record = binding.NewRecord('r');
        binding.Rearm(record);

        object gate = record.GetType().GetField("m_gate", Members)?.GetValue(record)
            ?? throw new AssertFailedException("OverlappedOp has no per-record gate: two retirements can both free its block");

        Exception? retireFault = null, closeFault = null;
        Thread retire, close;

        Monitor.Enter(gate);

        try
        {
            // The submit-side retirement enters first and holds at the gate, with the record still on its
            // socket's list; pollClose then snapshots that list and must wait for the same gate.
            retire = new Thread(() => retireFault = capture(() => ((IDisposable)record).Dispose()));
            retire.Start();

            close = new Thread(() => closeFault = capture(binding.Dispose));
            close.Start();

            Assert.IsFalse(retire.Join(200), "the retirement did not wait at the record's gate");
            Assert.IsFalse(close.Join(200), "pollClose disposed its bound handle while a record it tracks was still being retired");
        }
        finally
        {
            Monitor.Exit(gate);
        }

        Assert.IsTrue(retire.Join(5000) && close.Join(5000), "a retirement did not finish once the gate was free");
        Assert.IsNull(retireFault, $"the retirement threw: {retireFault}");
        Assert.IsNull(closeFault, $"pollClose threw: {closeFault}");
        Assert.IsTrue(binding.HandleDisposed, "pollClose returned without disposing its bound handle");
    }

    private static Exception? capture(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // One socket's SocketBinding, built by reflection over a socket the CLR has never bound, and the
    // records made on it.
    private sealed class Binding : IDisposable
    {
        private static readonly Type s_package = typeof(syscall_package);

        private readonly nint m_socket;
        private readonly object m_binding;
        private readonly Type m_recordType;
        private bool m_closed;

        private Binding(nint socket, object binding, Type recordType)
        {
            m_socket = socket;
            m_binding = binding;
            m_recordType = recordType;
        }

        internal static Binding Open()
        {
            if (!OperatingSystem.IsWindows())
                Assert.Inconclusive("the overlapped records bind a socket to the CLR's completion port, a windows facility");

            Type bindingType = nested("SocketBinding");
            Type recordType = nested("OverlappedOp");

            nint socket = openSocket();
            object binding = Activator.CreateInstance(bindingType, Members, null, [(nuint)socket], null)!;

            return new Binding(socket, binding, recordType);
        }

        internal object NewRecord(char mode)
        {
            object record = Activator.CreateInstance(m_recordType, Members, null, [m_binding, (nint)mode, newKey()], null)!;
            m_binding.GetType().GetMethod("Track", Members)!.Invoke(m_binding, [record]);

            return record;
        }

        internal void Rearm(object record) => m_recordType.GetMethod("Rearm", Members)!.Invoke(record, null);

        internal bool HandleDisposed
        {
            get
            {
                ThreadPoolBoundHandle bound = (ThreadPoolBoundHandle)m_binding.GetType().GetField("Bound", Members)!.GetValue(m_binding)!;

                // An allocation is refused by a disposed handle, and freed again by a live one.
                try
                {
                    // Reached only past Open's host gate, so only on windows.
#pragma warning disable CA1416
                    unsafe
                    {
                        NativeOverlapped* probe = bound.AllocateNativeOverlapped(static (_, _, _) => { }, null, null);
                        bound.FreeNativeOverlapped(probe);
                    }
#pragma warning restore CA1416

                    return false;
                }
                catch (ObjectDisposedException)
                {
                    return true;
                }
            }
        }

        public void Dispose()
        {
            if (m_closed)
                return;

            m_closed = true;
            ((IDisposable)m_binding).Dispose();
            closesocket(m_socket);
        }

        // A record's key is the caller's *syscall.Overlapped, as every wrapper passes it: Rearm reads
        // the key's HEvent to route the submit, so a stand-in object fails the cast before the arm
        // under test is reached. A zero Overlapped has no event, so the submit routes through the
        // port, as a pollable socket's operation does.
        private static object newKey()
        {
            Type overlapped = s_package.GetNestedType("Overlapped", BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new AssertFailedException("syscall_package.Overlapped is not in this build (the windows flavour carries it)");

            foreach (MethodInfo method in typeof(builtin).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name == "Ꮡ" && method.IsGenericMethodDefinition && method.GetParameters() is [{ ParameterType.IsByRef: true }])
                    return method.MakeGenericMethod(overlapped).Invoke(null, [Activator.CreateInstance(overlapped)])!;
            }

            throw new AssertFailedException("builtin.Ꮡ<T>(in T) is not in this build");
        }

        private static Type nested(string name) =>
            s_package.GetNestedType(name, BindingFlags.NonPublic)
            ?? throw new AssertFailedException($"syscall_package.{name} is not in this build (the windows flavour carries it)");

        private static unsafe nint openSocket()
        {
            byte* data = stackalloc byte[512];

            Assert.AreEqual(0, WSAStartup(0x0202, data), "WSAStartup");

            nint socket = WSASocketW(2 /* AF_INET */, 1 /* SOCK_STREAM */, 6 /* IPPROTO_TCP */, 0, 0, 0x01 /* WSA_FLAG_OVERLAPPED */);

            Assert.AreNotEqual((nint)(-1), socket, $"WSASocketW: {Marshal.GetLastPInvokeError()}");
            return socket;
        }

        [DllImport("ws2_32.dll")]
        private static extern unsafe int WSAStartup(ushort version, byte* data);

        [DllImport("ws2_32.dll", SetLastError = true)]
        private static extern nint WSASocketW(int af, int type, int protocol, nint protocolInfo, uint group, uint flags);

        [DllImport("ws2_32.dll")]
        private static extern int closesocket(nint socket);
    }
}
