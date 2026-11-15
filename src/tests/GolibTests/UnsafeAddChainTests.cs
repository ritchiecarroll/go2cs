using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// A pointer advanced step by step keeps ONE hop to what it was derived from
// (docs/phase4/DESIGN-array-pointer-doors.md 15). Go's `for ... { p = unsafe.Add(p, 1) }` makes one pointer per step
// and each replaces the last. Here each step minted a native box that retained the box of the step before, so a walk
// of N steps held N boxes (55 bytes a step), compared in N frames (a stack overflow at 1,000,000 steps), and lost the
// view doors' bound past 63 steps (the walk to the Go value is capped). A derivation from a native box that owns no
// pin now retains what that box retained, so every step is one hop from the Go value.
//
// Every arm takes its pointer as the converter emits it: unsafe.Pointer(&b[0]) is the pinning mint of an element box.
[TestClass]
public unsafe class UnsafeAddChainTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Quad
    {
        internal uint a;
        internal uint b;
        internal ulong c;
    }

    private const long Big = 1L << 30;

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    // `steps` single steps, +1 then -1 in turn: an even count ends where it began, an odd one a byte inside.
    private static @unsafe.Pointer WalkInPlace(@unsafe.Pointer p, int steps)
    {
        for (int i = 0; i < steps; i++)
            p = @unsafe.Add(p, (i & 1) == 0 ? 1 : -1);

        return p;
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(1000)]
    public void AStepwiseWalkStaysOneHopFromWhatItWasDerivedFrom(int steps)
    {
        array<byte> bytes = new array<byte>(2048);
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(element);
        nuint number = p.Value.Value;

        for (int i = 0; i < steps; i++)
            p = @unsafe.Add(p, 1);

        Assert.AreEqual(number + (nuint)steps, p.Value.Value, "the walked pointer's number");
        Assert.IsTrue(p.RetainedSource is NativeBox<byte> carried && carried.NativeAddress == number + (nuint)steps &&
                      ReferenceEquals(carried.RetainedSource, element),
            $"after {steps} steps the pointer carries a native box at its own value that retains the element box itself, not the box of the step before");

        // The write Go makes through it: *(*byte)(p) = 7.
        ((ж<byte>)(uintptr)p).Value = 7;
        Assert.AreEqual((byte)7, bytes[steps], "a write through the walked pointer reaches the array");
        GC.KeepAlive(element);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (@unsafe.Pointer walked, WeakReference firstStep) WalkOverADroppedFirstStep(ж<byte> element, int steps)
    {
        @unsafe.Pointer p = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(element), 1);
        WeakReference firstStep = new WeakReference(p.RetainedSource);

        for (int i = 1; i < steps; i++)
            p = @unsafe.Add(p, 1);

        return (p, firstStep);
    }

    [TestMethod]
    public void AStepwiseWalkKeepsNoEarlierStepAlive()
    {
        array<byte> bytes = new array<byte>(256);
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        (@unsafe.Pointer walked, WeakReference firstStep) = WalkOverADroppedFirstStep(element, 100);

        Collect();

        Assert.IsFalse(firstStep.IsAlive, "the box of step 1 is collected while the pointer walked 100 steps from it is alive");

        ((ж<byte>)(uintptr)walked).Value = 9;
        Assert.AreEqual((byte)9, bytes[100], "and the walked pointer still writes the array");
        GC.KeepAlive(element);
    }

    // RUN ALONE before the fix: the comparison overflowed the stack and ended the test host.
    [TestMethod]
    public void AMillionStepWalkComparesEqualToWhereItBegan()
    {
        array<byte> bytes = new array<byte>(32);
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer start = @unsafe.Pointer.FromPinnedBox(element);
        @unsafe.Pointer walked = WalkInPlace(start, 1_000_000);

        Assert.IsTrue(walked == start, "p walked 1,000,000 steps back to its own address equals p (Go's ==)");
        Assert.AreEqual(start.GetHashCode(), walked.GetHashCode(), "and hashes with it");
        Assert.IsTrue(walked != @unsafe.Add(start, 1), "and is unequal to the next address");
        GC.KeepAlive(element);
    }

    // The order token is the same at every step, however the address was reached: the walked pointer, the same
    // address minted in one step, and Go's own pointer to that element are one identity.
    [TestMethod]
    public void AWalkedPointerEqualsTheSameAddressMintedDirectlyAtEveryStep()
    {
        array<byte> bytes = new array<byte>(1024);
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer start = @unsafe.Pointer.FromPinnedBox(element);
        @unsafe.Pointer walked = start;

        for (int k = 1; k <= 1000; k++)
        {
            walked = @unsafe.Add(walked, 1);

            ж<byte> elementK = new ElemRefBox<byte>(bytes, k);
            @unsafe.Pointer direct = @unsafe.Add(start, k);
            @unsafe.Pointer managed = @unsafe.Pointer.FromPinnedBox(elementK);

            Assert.AreEqual(elementK.PointerOrderToken, NativeDerivation.Token((INativeRooted)walked.RetainedSource!),
                $"step {k}: the walked pointer's order token is the token of &b[{k}]");
            Assert.AreEqual(NativeDerivation.Token((INativeRooted)direct.RetainedSource!), NativeDerivation.Token((INativeRooted)walked.RetainedSource!),
                $"step {k}: and the token of the same address minted in one step");
            Assert.IsTrue(walked == direct, $"step {k}: walked == unsafe.Add(p, {k})");
            Assert.IsTrue(walked == managed, $"step {k}: walked == unsafe.Pointer(&b[{k}])");
            Assert.AreEqual(direct.GetHashCode(), walked.GetHashCode(), $"step {k}: one hash with the direct mint");
            Assert.AreEqual(managed.GetHashCode(), walked.GetHashCode(), $"step {k}: one hash with the managed pointer");
            Assert.IsTrue(walked != start, $"step {k}: walked != p");
        }

        GC.KeepAlive(element);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(61)]
    [DataRow(63)]
    [DataRow(65)]
    [DataRow(2001)]
    [DataRow(10001)]
    public void AViewAtAWalkedInteriorAddressIsBoundedByTheGoValue(int steps)
    {
        heap(new Quad { a = 1, b = 2, c = 3 }, out ж<Quad> box);
        @unsafe.Pointer walked = WalkInPlace(@unsafe.Pointer.FromPinnedBox(box), steps);

        // (*[1<<30]byte)(p)[:1] one byte inside a 16-byte Go value: 15 bytes are left of it.
        Assert.AreEqual((nint)15, @unsafe.Pointer.View<byte>(walked, 0, 1, Big).Capacity,
            $"after {steps} steps the view's capacity ends where the Go value ends");
        GC.KeepAlive(box);
    }

    // A native box that OWNS a pin is what a derivation keeps: it is never passed over, or its pin could be released
    // under the pointer derived from it. Two typed steps from it are one hop from it.
    [TestMethod]
    public void ATypedWalkStaysOneHopFromTheViewThatOwnsThePin()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[8] = 5;

        // (*int32)(unsafe.Pointer(&b[0])): a view pinned over the element.
        ж<int32> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, int32>();
        Assert.IsTrue(view is NativeBox<int32> { PinsManagedStorage: true }, "the control: the view owns a pin");

        ж<int32> once = @unsafe.Add(view, 4);
        ж<int32> twice = @unsafe.Add(once, 4);

        Assert.IsTrue(once is NativeBox<int32> first && ReferenceEquals(first.RetainedSource, view), "one step retains the view");
        Assert.IsTrue(twice is NativeBox<int32> second && ReferenceEquals(second.RetainedSource, view),
            "two steps retain the view too, not the box of the first step");
        Assert.AreEqual(5, twice.Value, "and read the array");
    }

    // The walk itself is a loop, whatever built the chain: a chain made link by link (no mint makes one now) is
    // compared without a frame per link, and a derivation from its last link keeps the Go value at its head.
    // RUN ALONE before the fix: the comparison overflowed the stack and ended the test host.
    [TestMethod]
    public void AChainBuiltLinkByLinkIsWalkedInALoop()
    {
        array<byte> bytes = new array<byte>(32);
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer start = @unsafe.Pointer.FromPinnedBox(element);
        nuint number = start.Value.Value;
        object link = element;

        for (int i = 0; i < 1_000_000; i++)
            link = new NativeBox<byte>(number + (nuint)(i & 1) + 1, retainedSource: link);

        // The last link sits at number + 2.
        ж<byte> last = (ж<byte>)link;

        Assert.AreEqual(new ElemRefBox<byte>(bytes, 2).PointerOrderToken, NativeDerivation.Token((INativeRooted)last),
            "the token of a 1,000,000-link chain is the token of the place it names");
        Assert.IsTrue(@unsafe.Pointer.FromBox(last) == @unsafe.Add(start, 2), "and it equals that address minted in one step");
        Assert.AreSame(element, NativeDerivation.Kept(last), "a derivation from the last link keeps the Go value at the head");
        GC.KeepAlive(element);
    }

    [TestMethod]
    public void AWalkOverCMemoryCarriesNothing()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            @unsafe.Pointer start = new @unsafe.Pointer((uintptr)(nuint)memory);
            @unsafe.Pointer walked = WalkInPlace(start, 1000);

            Assert.IsNull(walked.RetainedSource, "a number carries nothing after 1,000 steps");
            Assert.IsTrue(walked == start, "and equals where it began");

            // C memory through a native box: the typed walk is C memory alone at every step.
            ж<byte> typed = new NativeBox<byte>((nuint)memory);

            for (int i = 0; i < 8; i++)
                typed = @unsafe.Add(typed, 1);

            Assert.IsTrue(typed is NativeBox<byte> { RetainedSource: null } native && native.NativeAddress == (nuint)memory + 8,
                "a pointer walked within C memory is C memory alone");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
}
