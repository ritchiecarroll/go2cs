using System;
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
}
