using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

[TestClass]
public class PinnedBufferTests
{
    [TestMethod]
    public void CloneOwnsItsOwnPin()
    {
        // Failing-first record (2026-08-28): Clone copied the GCHandle STRUCT, so the original and
        // the clone shared one handle slot — disposing either released the other's pin (observable
        // below), and both finalizers freeing the shared slot lands the second Free on whatever
        // the runtime re-issued it to: GC handle-table corruption, the same table WeakReference,
        // ConditionalWeakTable and every other pin live in.
        var bytes = new byte[] { 1, 2, 3, 4 };
        var original = PinnedBuffer.Of(bytes);
        var clone = (PinnedBuffer)original.Clone();

        // Same pinned storage: a write through one view reads back through the other.
        Span<byte> a = original;
        Span<byte> b = clone;
        a[0] = 42;
        Assert.AreEqual(42, b[0], "the clone must view the same pinned storage");
        Assert.AreSame(bytes, clone.PinnedTarget, "the clone must pin the same target");

        // But each owns its own pin: disposing the original must not release the clone's.
        original.Dispose();
        Assert.AreSame(bytes, clone.PinnedTarget, "disposing the original must not release the clone's pin");
        Assert.AreEqual(42, ((Span<byte>)clone)[0], "the clone's pin must survive the original's dispose");

        clone.Dispose();
    }

    // Two releases of one buffer racing on two threads (COORD's ruling on P3's re-read of the pin-extent seat): exactly
    // one of them frees the handle and the other returns, as a second Dispose on one thread always did. Before the
    // release's state CAS gated it, both could pass the disposed check and reach the free. Probabilistic: it races many
    // buffers, and its message names what the losing release did.
    [TestMethod]
    public void TwoReleasesRacingOnOneBufferFreeItsHandleOnceAndNeitherThrows()
    {
        const int trials = 20_000;
        PinnedBuffer[] buffers = new PinnedBuffer[trials];
        int[] arrived = new int[trials];
        Exception? firstFailure = null;
        int failures = 0;

        for (int i = 0; i < trials; i++)
            buffers[i] = PinnedBuffer.Of(new byte[16]);

        void Release()
        {
            for (int i = 0; i < trials; i++)
            {
                // Both threads meet at each buffer before either releases it.
                Interlocked.Increment(ref arrived[i]);

                SpinWait spin = default;

                while (Volatile.Read(ref arrived[i]) < 2)
                    spin.SpinOnce(-1);

                try
                {
                    buffers[i].Dispose();
                }
                catch (Exception failure)
                {
                    Interlocked.CompareExchange(ref firstFailure, failure, null);
                    Interlocked.Increment(ref failures);
                }
            }
        }

        var first = new Thread(Release);
        var second = new Thread(Release);
        first.Start();
        second.Start();
        first.Join();
        second.Join();

        Assert.AreEqual(0, failures, $"{failures} of {trials} racing releases threw; the first: {firstFailure?.GetType().Name}: {firstFailure?.Message}");

        foreach (PinnedBuffer buffer in buffers)
            Assert.IsNull(buffer.PinnedTarget, "a released buffer still holds its pin");
    }
}
