// TrimStage3c1Tests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// Trim stage 3c-1 (docs/PLAN-golib-full-trim.md, section 9): four reflect entry points held a value whose own generic
// type was the instantiation they needed, and still closed a helper over it with MakeGenericMethod. They now dispatch
// through the value's face: a channel's re-stamp, a slice's array alias and byte view (ISlice<T>'s default members), and
// a box's element door (ж<T>, the same view and publish at<E> uses). These pin each entry point's answers, the
// aliasing that makes them Go's, and the refusals of a value that does not match the type the caller names.
// TryByteSliceView's answers are pinned by GoReflectBridgeClosureTests.
[TestClass]
public class TrimStage3c1Tests
{
    private struct Elem
    {
        public ulong A;
        public ulong B;
    }

    [TestMethod]
    public void AChannelReStampSharesItsCore()
    {
        channel<int> both = new(1);
        ChanCargo? send = ChanCargo.Of(GoChanDir.Send);

        channel<int> stamped = (channel<int>)GoReflect.WithChanCargo(both, send);

        Assert.AreSame(send, stamped.Cargo, "the value describes itself with the destination's direction");
        Assert.IsNull(both.Cargo, "the source keeps its own");

        stamped.Send(5);
        Assert.AreEqual(5, both.Receive(), "same core: a send through one is received through the other");

        object notAChannel = new slice<int>(new int[] { 1 });
        Assert.AreSame(notAChannel, GoReflect.WithChanCargo(notAChannel, send), "a value that is not a channel<T> is returned as it is");
    }

    [TestMethod]
    public void ASliceAliasesAsAnArrayPointer()
    {
        slice<int> source = new int[] { 1, 2, 3, 4 }.slice();

        ж<array<int>> pointer = (ж<array<int>>)GoReflect.AliasSliceAsArrayPointer(source, typeof(ж<array<int>>), 3);

        Assert.AreEqual((nint)3, pointer.Value.Length, "the array takes the length the conversion names");
        pointer.Value[0] = 10;
        Assert.AreEqual(10, source[0], "Go's (*[N]T)(s) aliases the slice's backing store");

        slice<long> wrongElement = new long[] { 1, 2, 3 }.slice();
        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.AliasSliceAsArrayPointer(wrongElement, typeof(ж<array<int>>), 3), "a slice of another element is refused by name");
    }

    [TestMethod]
    public void ABoxsElementDoorAliasesItsElement()
    {
        ж<array<int>> box = new StandardBox<array<int>>(new array<int>(new int[] { 1, 2, 3 }));

        ж<int> element = (ж<int>)GoReflect.ElementAliasBoxOfBox(box, typeof(int), 1);

        Assert.AreEqual(2, element.Value);
        element.Value = 20;
        Assert.AreEqual(20, box.Value[1], "the element alias writes through to the box's storage");

        Assert.ThrowsException<PanicException>(() => GoReflect.ElementAliasBoxOfBox(box, typeof(int), 3), "an index past the length panics as Go's &p[i] does");
        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.ElementAliasBoxOfBox(box, typeof(long), 0), "an element type the array does not hold is refused");
    }

    [TestMethod]
    public void ANativeArrayBoxsElementDoorReachesTheBlock()
    {
        // The helper this replaced read the CONTAINER type off the box's first generic argument, which for a native box
        // (NativeArrayBox<E> : ж<array<E>>) is the ELEMENT: the door now asks the box itself, as at<E> does.
        const int n = 4;
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(n * Marshal.SizeOf<Elem>());

        try
        {
            unsafe
            {
                Elem* raw = (Elem*)addr;

                for (int i = 0; i < n; i++)
                    raw[i] = new Elem { A = (ulong)(0x1000 + i), B = 0 };
            }

            ж<array<Elem>> pointer = NativeArrayBox<Elem>.Over(addr, n);
            ж<Elem> element = (ж<Elem>)GoReflect.ElementAliasBoxOfBox(pointer, typeof(Elem), 2);

            Assert.AreEqual((ulong)0x1002, element.Value.A, "the door reads the real block at the element's stride");

            element.Value = new Elem { A = 7, B = 8 };

            unsafe
            {
                Assert.AreEqual((ulong)8, ((Elem*)addr)[2].B, "a write through the alias lands in native memory");
            }

            // A native box answers only its own element type; any other falls through to Value, which a native box refuses
            // by name (it has no array header to read), exactly as at<ulong> does.
            Assert.ThrowsException<PanicException>(() => GoReflect.ElementAliasBoxOfBox(pointer, typeof(ulong), 0), "a native box answers only its own element type");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }
}
