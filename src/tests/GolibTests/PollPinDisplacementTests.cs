using System;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// cgo S6, the internal/poll displacement (docs/phase4/DESIGN-cgo-phase3-marshaling.md 21.8). Go pins every overlapped
// buffer with runtime.Pinner for the flight. In go2cs the managed poller already holds it (the submit retains the
// buffer's own pinning box until the next Rearm), and since S6 a Pinner pin takes a handle of its own, so (*FD).pin is
// displaced to nothing on Windows. COORD required both rows because 21.8 was a code reading: the displacement takes no
// handle per I/O, and the property it relies on holds under a compaction while a read is in flight.
[TestClass]
public class PollPinDisplacementTests
{
    private static (net_package.Conn server, net_package.Conn client, net_package.Listener listener) Pair()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Inconclusive("the displacement is Windows' (internal/poll/windows): NOT MEASURED on this host");

        (net_package.Listener listener, error err) = net_package.Listen("tcp", "127.0.0.1:0");
        Assert.IsNull(err, $"listen: {err}");

        net_package.Conn server = default!;
        error acceptErr = default!;
        using ManualResetEventSlim accepted = new();

        goǃ(() =>
        {
            (server, acceptErr) = listener.Accept();
            accepted.Set();
        });

        (net_package.Conn client, error dialErr) = net_package.Dial("tcp", listener.Addr().String());
        Assert.IsNull(dialErr, $"dial: {dialErr}");
        Assert.IsTrue(accepted.Wait(TimeSpan.FromSeconds(30)), "accept did not finish");
        Assert.IsNull(acceptErr, $"accept: {acceptErr}");

        return (server, client, listener);
    }

    private static T OnAGoroutine<T>(Func<T> call)
    {
        T result = default!;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                result = call();
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(60)), "the goroutine did not finish");

        if (thrown is not null)
            throw thrown;

        return result;
    }

    // Zero handles per I/O: fifty overlapped reads and writes over loopback TCP, and the pin table takes no handle.
    [TestMethod]
    public void AnOverlappedReadAndWriteTakeNoPinHandle()
    {
        (net_package.Conn server, net_package.Conn client, net_package.Listener listener) = Pair();

        try
        {
            long before = GoPinTable.HandlesTaken;

            OnAGoroutine(() =>
            {
                slice<byte> message = new slice<byte>("go2cs-poll"u8.ToArray());
                slice<byte> buffer = new slice<byte>(new byte[64]);

                for (int i = 0; i < 50; i++)
                {
                    (nint written, error werr) = client.Write(message);
                    Assert.IsNull(werr, $"write {i}: {werr}");
                    Assert.AreEqual((nint)message.Length, written);

                    (nint read, error rerr) = server.Read(buffer);
                    Assert.IsNull(rerr, $"read {i}: {rerr}");
                    Assert.AreEqual((nint)message.Length, read, $"read {i}");
                }

                return 0;
            });

            Assert.AreEqual(before, GoPinTable.HandlesTaken, "the displaced (*FD).pin takes no handle per I/O");
        }
        finally
        {
            client.Close();
            server.Close();
            listener.Close();
        }
    }

    // A compaction while a read is in flight: the read is submitted and parked, the heap is compacted with garbage around
    // the buffer, then the peer writes. The bytes land in the buffer the read was given, because the poller holds it.
    [TestMethod]
    public void AnOverlappedReadSurvivesACompactionMidFlight()
    {
        (net_package.Conn server, net_package.Conn client, net_package.Listener listener) = Pair();

        try
        {
            byte[] payload = new byte[256];

            for (int i = 0; i < payload.Length; i++)
                payload[i] = (byte)(i * 7 + 3);

            slice<byte> buffer = new slice<byte>(new byte[payload.Length]);
            nint got = 0;
            error readErr = default!;
            using ManualResetEventSlim done = new();

            goǃ(() =>
            {
                while (got < payload.Length && readErr == default)
                {
                    (nint n, error err) = server.Read(buffer[(int)got..]);
                    got += n;
                    readErr = err;
                }

                done.Set();
            });

            Thread.Sleep(300); // the read is submitted and parked on the poller

            for (int round = 0; round < 3; round++)
            {
                object[] churn = new object[8192];

                for (int i = 0; i < churn.Length; i++)
                    churn[i] = new byte[64];

                churn = null!;
                System.GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            }

            Assert.IsFalse(done.IsSet, "the read must still be in flight when the heap is compacted");

            OnAGoroutine(() =>
            {
                (nint written, error werr) = client.Write(new slice<byte>(payload));
                Assert.IsNull(werr, $"write: {werr}");
                return written;
            });

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the read did not finish");
            Assert.IsNull(readErr, $"read: {readErr}");

            for (int i = 0; i < payload.Length; i++)
                Assert.AreEqual(payload[i], buffer[i], $"byte {i} landed where the read's buffer is");
        }
        finally
        {
            client.Close();
            server.Close();
            listener.Close();
        }
    }
}
