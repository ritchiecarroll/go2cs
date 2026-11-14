using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// unsafe.Add keeps what its pointer carried (docs/phase4/DESIGN-unsafe-pointer-call-result.md). Go's unsafe.Pointer
// keeps its referent alive, and unsafe.Add returns an unsafe.Pointer. Here a pointer CARRIES the box it was minted
// from, and the box holds the pin that keeps its storage where the pointer's number says it is. Add returned the new
// number alone, so the pin could be released under a pointer still in use, and a write through it missed the storage.
//
// Every arm takes its pointer as the converter emits it: unsafe.Pointer(&b[0]) is the pinning mint of an element box.
[TestClass]
public unsafe class UnsafeAddCarryTests
{
    private struct holder
    {
        public @string Name;
        public int N;
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (@unsafe.Pointer moved, WeakReference element, nuint number) AddOverADroppedElement(array<byte> bytes)
    {
        ж<byte> element = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer carrying = @unsafe.Pointer.FromPinnedBox(element);

        return (@unsafe.Add(carrying, 4), new WeakReference(element), carrying.Value.Value);
    }

    [TestMethod]
    public void AddOverACarryingPointerKeepsWhatItCarried()
    {
        array<byte> bytes = new array<byte>(32);
        (@unsafe.Pointer moved, WeakReference element, nuint number) = AddOverADroppedElement(bytes);

        Assert.AreEqual(number + 4, moved.Value.Value, "the moved pointer's number is the element's plus the offset");

        Collect();

        Assert.IsTrue(element.IsAlive, "the element box, and the pin it holds, are collected while the moved pointer is alive");
        Assert.IsTrue(moved.RetainedSource is NativeBox<byte> { RetainedSource: not null } derived && derived.NativeAddress == number + 4,
            "the moved pointer carries a native box AT its own value, which retains what the pointer carried");

        // The write Go makes through it: *(*byte)(p) = 7.
        ((ж<byte>)(uintptr)moved).Value = 7;
        Assert.AreEqual((byte)7, bytes[4], "a write through the moved pointer reaches the slice after a collection");
    }

    [TestMethod]
    public void AddOverAPointerThatCarriesNothingStaysANumber()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            // A bare number.
            @unsafe.Pointer bare = @unsafe.Add(new @unsafe.Pointer((uintptr)(nuint)memory), 4);
            Assert.AreEqual((nuint)memory + 4, bare.Value.Value);
            Assert.IsNull(bare.RetainedSource, "a number carries nothing, before and after");

            // C memory alone: the native box is carried by the pinning mint, and there is nothing to keep of it.
            ж<byte> native = new NativeBox<byte>((nuint)memory);
            @unsafe.Pointer overC = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(native), 4);
            Assert.AreEqual((nuint)memory + 4, overC.Value.Value);
            Assert.IsNull(overC.RetainedSource, "C memory alone keeps the plain number");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    [TestMethod]
    public void AddOverATokenKeepsTheTokensOwnRoute()
    {
        // A pointee that holds references has no address: its number is a token, and token arithmetic has its own route.
        ж<holder> box = new StandardBox<holder>(default(holder));
        @unsafe.Pointer carrying = @unsafe.Pointer.FromPinnedBox(box);
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(carrying.Value.Value), "the control: this pointer's number is a token");

        @unsafe.Pointer moved = @unsafe.Add(carrying, 8);

        Assert.AreEqual(carrying.Value.Value + 8, moved.Value.Value);
        Assert.IsNull(moved.RetainedSource, "a token plus an offset stays the number it was");
        GC.KeepAlive(box);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<int32> moved, WeakReference view) TypedAddOverADroppedView(array<byte> bytes)
    {
        // (*int32)(unsafe.Pointer(&b[0])): a view pinned over the element.
        ж<int32> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, int32>();

        return (@unsafe.Add(view, 4), new WeakReference(view));
    }

    [TestMethod]
    public void TypedAddOverAViewOfGoMemoryKeepsTheView()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[4] = 9;
        (ж<int32> moved, WeakReference view) = TypedAddOverADroppedView(bytes);

        Collect();

        Assert.IsTrue(view.IsAlive, "the view, and the pin it holds, are collected while the pointer moved from it is alive");
        Assert.AreEqual(9, moved.Value, "the moved pointer still reads the slice");
    }

    [TestMethod]
    public void TypedAddOverCMemoryKeepsNothing()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            ж<int32> moved = @unsafe.Add(new NativeBox<int32>((nuint)memory), 4);

            Assert.IsTrue(moved is NativeBox<int32> { RetainedSource: null } native && native.NativeAddress == (nuint)memory + 4,
                "a pointer moved within C memory is C memory alone");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
}
