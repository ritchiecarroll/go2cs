using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// runtime.memmove / runtime.memclrNoHeapPointers (src/core/runtime/memmove_impl.cs). Both were
// PartialStubGenerator stubs throwing NotImplementedException, and the runtime row's test host died
// on the first one at TestMemmoveAtomicity (10,233 of 10,891 results in). The arms follow the Go
// test's own shapes: OVERLAPPING copies in both directions (memmove, not memcpy), over real bytes and
// over a `[N]*int` window -- whose element pointers are ORDER TOKENS, since the CLR cannot pin an
// array of references -- plus a non-overlapping control and the refusal for a token whose managed
// element has no byte image.
[TestClass]
public class RuntimeMemmoveTests
{
    private static unsafe_package.Pointer At(GCHandle pinned, int offset) =>
        new(new uintptr((nuint)(pinned.AddrOfPinnedObject() + offset)));

    private static byte[] Sequence(int length)
    {
        byte[] bytes = new byte[length];

        for (int i = 0; i < length; i++)
            bytes[i] = (byte)(i + 1);

        return bytes;
    }

    private static void WithPinned(byte[] bytes, Action<GCHandle> body)
    {
        GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try { body(pinned); }
        finally { pinned.Free(); }
    }

    [TestMethod]
    public void BytesOverlappingForwardCopyAsMemmove()
    {
        // dst above src: a naive front-to-back copy would smear the first bytes forward.
        byte[] bytes = Sequence(10);

        WithPinned(bytes, p => runtime_package.GoMemmove(At(p, 2), At(p, 0), new uintptr(6)));

        CollectionAssert.AreEqual(new byte[] { 1, 2, 1, 2, 3, 4, 5, 6, 9, 10 }, bytes);
    }

    [TestMethod]
    public void BytesOverlappingBackwardCopyAsMemmove()
    {
        byte[] bytes = Sequence(10);

        WithPinned(bytes, p => runtime_package.GoMemmove(At(p, 0), At(p, 2), new uintptr(6)));

        CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 6, 7, 8, 7, 8, 9, 10 }, bytes);
    }

    [TestMethod]
    public void BytesNonOverlappingCopyControl()
    {
        byte[] bytes = Sequence(10);

        WithPinned(bytes, p => runtime_package.GoMemmove(At(p, 6), At(p, 0), new uintptr(3)));

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 1, 2, 3, 10 }, bytes);
    }

    [TestMethod]
    public void ZeroLengthIsANoOpEvenForNil()
    {
        // Go's memmove(nil, nil, 0) returns without touching either operand.
        runtime_package.GoMemmove(new unsafe_package.Pointer(nil), new unsafe_package.Pointer(nil), new uintptr(0));
    }

    // TestMemmoveAtomicity's own fixture: s [100]*int, src := s[n-1:2n-1], dst := s[:n].
    private static (slice<ж<nint>> all, slice<ж<nint>> src, slice<ж<nint>> dst, ж<nint> x) PointerWindow(int n, bool backward)
    {
        slice<ж<nint>> all = new slice<ж<nint>>(100);
        slice<ж<nint>> src = all[(n - 1)..(2 * n - 1)];
        slice<ж<nint>> dst = all[..n];

        if (backward)
            (src, dst) = (dst, src);

        ж<nint> x = new StandardBox<nint>(7);

        for (int i = 0; i < n; i++)
            src[i] = x;

        return (all, src, dst, x);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void PointerElementsOverlappingCopyWholeElements(bool backward)
    {
        const int n = 5;
        var (_, src, dst, x) = PointerWindow(n, backward);

        for (int i = 0; i < n; i++)
            dst[i] = null!;

        // dst and src OVERLAP, so clearing dst cleared part of src too (Go's test does the same and
        // so asserts only nil-or-&x). memmove's contract is "as if src were first copied aside": the
        // expected dst is src as it stands NOW.
        ж<nint>[] expected = new ж<nint>[n];

        for (int i = 0; i < n; i++)
            expected[i] = src[i];

        Assert.IsTrue(Array.Exists(expected, e => e is null) && Array.Exists(expected, e => e is not null),
            "the fixture must hold both nil and &x slots, or the copy's direction is not being tested");

        var sp = new unsafe_package.Pointer(Ꮡ(src, 0));
        var dp = new unsafe_package.Pointer(Ꮡ(dst, 0));

        // The fixture must reach the TOKEN arm, not the byte arm: a pointer array cannot be pinned.
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(sp.Value.Value), "src element pointer is expected to be an order token");
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(dp.Value.Value), "dst element pointer is expected to be an order token");

        runtime_package.GoMemmove(dp, sp, new uintptr((nuint)(n * IntPtr.Size)));

        for (int i = 0; i < n; i++)
            Assert.AreSame(expected[i], dst[i], $"dst[{i}] after a {(backward ? "backward" : "forward")} overlapping copy");

        GC.KeepAlive(x);
    }

    [TestMethod]
    public void MemclrOnAPointerWindowClearsWholeElements()
    {
        const int n = 4;
        var (_, src, _, _) = PointerWindow(n, backward: false);

        runtime_package.GoMemclrNoHeapPointers(new unsafe_package.Pointer(Ꮡ(src, 0)), new uintptr((nuint)(n * IntPtr.Size)));

        for (int i = 0; i < n; i++)
            Assert.IsNull(src[i], $"src[{i}] after memclr");
    }

    [TestMethod]
    public void MemclrOnBytesClears()
    {
        byte[] bytes = Sequence(6);

        WithPinned(bytes, p => runtime_package.GoMemclrNoHeapPointers(At(p, 1), new uintptr(3)));

        CollectionAssert.AreEqual(new byte[] { 1, 0, 0, 0, 5, 6 }, bytes);
    }

    [TestMethod]
    public void APointerIntoAReferenceArrayIsATokenWithItsReferent()
    {
        // Before the 2026-09-22 ruling this was an UNPINNED raw address (8-byte stride, unregistered,
        // no referent): GCHandle cannot pin an array of references.
        slice<ж<nint>> all = new slice<ж<nint>>(4);
        var b0 = Ꮡ(all, 0);
        var p0 = new unsafe_package.Pointer(b0);
        var p1 = new unsafe_package.Pointer(Ꮡ(all, 1));

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(p0.Value.Value), "&s[0] over []*int is an order token");
        Assert.AreSame(b0, p0.RetainedSource, "a token mint carries its box");
        Assert.IsNotNull(ManagedPointerTokens.Resolve(p0.Value.Value), "the token resolves through the registry");
        Assert.AreEqual((nuint)1, p1.Value.Value - p0.Value.Value,
            "same-array element tokens order by ELEMENT index (identity + ordering), not by a byte stride");
    }

    [TestMethod]
    public void APointerIntoAReferenceFreeArrayIsStillAPinnedAddress()
    {
        // The control: a reference-free element keeps its real, pinned address and retains nothing.
        slice<nint> all = new slice<nint>(4);
        var b0 = Ꮡ(all, 0);
        var p0 = new unsafe_package.Pointer(b0);
        var p1 = new unsafe_package.Pointer(Ꮡ(all, 1));

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(p0.Value.Value), "&s[0] over []int is a real address");
        Assert.IsTrue(b0.IsPinnedAt(p0.Value.Value), "and it is pinned where it points");
        Assert.IsNull(p0.RetainedSource, "an address mint retains nothing (PointerMintRetentionTests' contract)");
        Assert.AreEqual((nuint)IntPtr.Size, p1.Value.Value - p0.Value.Value, "adjacent elements are one nint apart");
    }

    [TestMethod]
    public void ATokenWhoseElementHasNoByteImageIsRefused()
    {
        // @string is a reference-bearing managed struct: no Go size this runtime can state, so no copy.
        slice<@string> strings = new slice<@string>(4);
        var p = new unsafe_package.Pointer(Ꮡ(strings, 0));
        var q = new unsafe_package.Pointer(Ꮡ(strings, 1));

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(p.Value.Value), "a string element pointer is expected to be an order token");

        var panic = Assert.ThrowsException<PanicException>(() => runtime_package.GoMemmove(q, p, new uintptr(16)));

        StringAssert.Contains(panic.Message, "arm 2a");
    }

    // runtime's own slice header, as export_test.go's MemclrBytes reads it: (*slice)(unsafe.Pointer(&b)).
    private struct SliceHeaderShape
    {
        public unsafe_package.Pointer array;
        public nint len;
        public nint cap;
    }

    [TestMethod]
    public void MemclrThroughASliceHeaderClearsTheSliceAfterTheCollectorMovesIt()
    {
        // runtime's TestMemclr: MemclrBytes(mem[x:x+n]) hands memclrNoHeapPointers the slice
        // header's array word. That word is a TRANSIENT address (FromBox: no pin) that RETAINS the
        // element box. A compacting collection between the header read and the clear moves the
        // backing, and a clear through the stale number misses the slice ("failed clear mem[200]").
        byte[] backing = new byte[64];
        backing.AsSpan().Fill(0xee);
        slice<byte> mem = new(backing);
        ref slice<byte> b = ref heap(mem[8..16], out ж<slice<byte>> Ꮡb);

        unsafe_package.Pointer array = Ꮡb.Reinterpret<slice<byte>, SliceHeaderShape>().Value.array;
        nuint minted = array.Value.Value;
        bool moved = false;

        for (int attempt = 0; attempt < 20 && !moved; attempt++)
        {
            GC.KeepAlive(new byte[4096]);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            moved = unsafe_package.Pointer.FromBox(new ElemRefBox<byte>(new slice<byte>(backing), 8)).Value.Value != minted;
        }

        if (!moved)
            Assert.Inconclusive("the collector did not move the backing, so this run cannot tell a stale address from a live one");

        runtime_package.GoMemclrNoHeapPointers(array, new uintptr(8));

        for (int i = 0; i < 64; i++)
            Assert.AreEqual(i >= 8 && i < 16 ? (byte)0 : (byte)0xee, backing[i], $"backing[{i}]");
    }
}
