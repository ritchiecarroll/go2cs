// PointerNilCheckTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Go's <c>&amp;*p</c> is <c>p</c> after a nil check: the address of the variable a pointer indirection denotes
/// is the pointer itself, and Go still evaluates <c>*p</c>, so a nil <c>p</c> panics there. The converter emits
/// <c>p.nilcheck()</c> for the form (sqlite-cs W9b, the third form), where it used to emit <c>Ꮡ(~p)</c>, a new
/// box over a COPY of the pointee, which lost every write through the result.
/// </summary>
/// <remarks>
/// One arm per spelling of a Go pointer: a managed box, a native address, and the three nil pointers (a null
/// reference, the canonical nil box, the zero address). The nil arms require Go's text exactly.
/// </remarks>
[TestClass]
public class PointerNilCheckTests
{
    private const string Fault = "runtime error: invalid memory address or nil pointer dereference";

    [TestMethod]
    public void ANilCheckOfAManagedPointerIsThatPointer()
    {
        ж<nint> p = Ꮡ((nint)7);
        ж<nint> q = p.nilcheck();

        Assert.AreSame(p, q, "the result is the pointer, not a box over a copy of what it points to");

        q.Value = 9;
        Assert.AreEqual((nint)9, ~p, "a write through the result is a write through the pointer");
    }

    [TestMethod]
    public unsafe void ANilCheckOfANativePointerIsThatPointer()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            ж<ulong> p = (ж<ulong>)(uintptr)(nuint)memory;
            ж<ulong> q = p.nilcheck();

            Assert.AreSame(p, q);

            q.Value = 0xBEEF;
            Assert.AreEqual(0xBEEFUL, *(ulong*)memory, "a write through the result reaches the native word");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    [TestMethod]
    public void ANilCheckOfANullReferencePanicsWithGosText()
    {
        ж<nint>? p = null;

        Assert.AreEqual(Fault, Assert.ThrowsException<PanicException>(() => p.nilcheck()).Message);
    }

    [TestMethod]
    public void ANilCheckOfTheNilBoxPanicsWithGosText()
    {
        ж<nint> p = nil;

        Assert.AreEqual(Fault, Assert.ThrowsException<PanicException>(() => p.nilcheck()).Message);
    }

    [TestMethod]
    public void ANilCheckOfTheZeroAddressPanicsWithGosText()
    {
        ж<ulong> p = (ж<ulong>)(uintptr)0;

        Assert.AreEqual(Fault, Assert.ThrowsException<PanicException>(() => p.nilcheck()).Message);
    }
}
