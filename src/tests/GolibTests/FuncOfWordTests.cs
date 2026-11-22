// FuncOfWordTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Go's <c>*(*unsafe.Pointer)(unsafe.Pointer(&amp;f)) = p</c> for a func variable f (funcWordOperations.go, w4; the
/// BOARD's 2026-11-20 LOST WRITE, TC-13): <see cref="@unsafe.Pointer.FuncOfWord{F}"/>.
/// </summary>
/// <remarks>
/// Before the fix the store wrote p into the reinterpret's fresh <c>Ꮡ(OfFunc(f))</c> box, which nothing reads, so f
/// stayed nil and the next call through it was a nil func call. A func value's word is its GoFuncCookie, so a word that
/// IS one is that func; 0 is the nil func; any other word (Go's funcval pointer, whose fn names code by a PC, or C
/// memory) is refused by name, until cgo callbacks bind a func to a PC.
/// </remarks>
[TestClass]
public unsafe class FuncOfWordTests
{
    private const string Refused = "func word store: the pointer stored as a func value's word is not a func value's word";

    private static nint Inc(nint a) => a + 1;

    private static void AssertRefused(Action action)
    {
        PanicException panic = Assert.ThrowsException<PanicException>(action);
        StringAssert.Contains(panic.Message, Refused);
    }

    // The copy shape: one func value's word stored into another func variable, then called.
    [TestMethod]
    public void AFuncValuesWordCopiedIntoAnotherFuncIsThatFunc()
    {
        Func<nint, nint> src = Inc;
        Func<nint, nint> dst = @unsafe.Pointer.FuncOfWord<Func<nint, nint>>(@unsafe.Pointer.OfFunc(src));

        Assert.AreEqual((nint)3, dst(2));
        Assert.IsTrue(@unsafe.Pointer.OfFunc(dst) == @unsafe.Pointer.OfFunc(src), "the copy round-trips: its word is the source's word");
    }

    // A closure's word names that closure value, captured state included.
    [TestMethod]
    public void AClosuresWordCopiedIsThatClosure()
    {
        nint basis = 10;
        Func<nint, nint> src = a => a + basis;
        Func<nint, nint> dst = @unsafe.Pointer.FuncOfWord<Func<nint, nint>>(@unsafe.Pointer.OfFunc(src));

        basis = 20;
        Assert.AreEqual((nint)21, dst(1));
        GC.KeepAlive(src);
    }

    [TestMethod]
    public void ANilWordIsTheNilFunc()
    {
        Assert.IsNull(@unsafe.Pointer.FuncOfWord<Func<nint, nint>>(@unsafe.Pointer.Nil));
        Assert.IsNull(@unsafe.Pointer.FuncOfWord<Func<nint, nint>>(null));
        Assert.IsNull(@unsafe.Pointer.FuncOfWord<Func<nint, nint>>(@unsafe.Pointer.OfFunc(null)));
    }

    // The runtime's shape (debugcall.go, cgocall.go): a funcval built by hand, its fn word a code PC, stored as the
    // func's word. go2cs binds no func to a PC before cgo callbacks, so the store is refused by name, never left nil.
    [TestMethod]
    public void AFuncvalWhoseWordIsACodePCIsRefusedByName()
    {
        slice<uintptr> funcval = new(1);
        funcval[0] = (uintptr)GoSyntheticPC.Of((Func<nint, nint>)Inc);

        AssertRefused(() => @unsafe.Pointer.FuncOfWord<Func<nint, nint>>(@unsafe.Pointer.FromPinnedBox(Ꮡ(funcval, 0))));
        GC.KeepAlive(funcval);
    }

    // C memory, or any number that is not a func value's word.
    [TestMethod]
    public void ACMemoryWordIsRefusedByName()
    {
        nuint memory = (nuint)NativeMemory.AllocZeroed(16);

        try
        {
            AssertRefused(() => @unsafe.Pointer.FuncOfWord<Func<nint, nint>>(new @unsafe.Pointer((uintptr)memory)));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }
}
