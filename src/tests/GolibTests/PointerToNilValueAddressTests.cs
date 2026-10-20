// PointerToNilValueAddressTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// <c>uintptr(unsafe.Pointer(&amp;p))</c> where <c>p</c> holds nil: the address of the VARIABLE, which Go
/// reports as non-zero and which does not change when <c>p</c> is assigned.
/// </summary>
/// <remarks>
/// The <c>ж&lt;T&gt;</c> to <c>uintptr</c> and <c>void*</c> operators asked the value-peeking <c>IsNull</c>,
/// which a standard heap box answers true for when it holds a null reference or a null
/// <c>Nullable&lt;T&gt;</c>. So <c>&amp;p</c> converted to 0 while <c>p</c> was nil and to the box's
/// number once <c>p</c> was assigned: Windows read a 0 out-parameter as "no output wanted". The nil
/// question there is whether the box IS the nil pointer (<c>IsNilPointer</c>).
/// </remarks>
[TestClass]
public class PointerToNilValueAddressTests
{
    [TestMethod]
    public unsafe void AddressOfANilReferenceVariableIsNonZeroAndStable()
    {
        ж<ж<nint>> Ꮡp = @new<ж<nint>>();

        uintptr before = (uintptr)Ꮡp;
        nuint beforeVoid = (nuint)(void*)Ꮡp;

        Assert.AreNotEqual((uintptr)0, before, "&p is not nil while p is");
        Assert.AreNotEqual((nuint)0, beforeVoid, "the void* operator agrees");

        // ValueSlot, as the converter's `ref var p = ref heap(...)` writes: Value panics on its own peek.
        Ꮡp.ValueSlot = @new<nint>();

        Assert.AreEqual(before, (uintptr)Ꮡp, "assigning p does not move &p");
        Assert.AreEqual(beforeVoid, (nuint)(void*)Ꮡp, "nor through the void* operator");
        Assert.AreEqual((nuint)before.Value, beforeVoid, "both operators name one address");
    }

    [TestMethod]
    public unsafe void AddressOfANullNullableVariableIsNonZeroAndStable()
    {
        ж<int?> Ꮡn = @new<int?>();

        uintptr before = (uintptr)Ꮡn;
        nuint beforeVoid = (nuint)(void*)Ꮡn;

        Assert.AreNotEqual((uintptr)0, before);
        Assert.AreNotEqual((nuint)0, beforeVoid);

        Ꮡn.ValueSlot = 7;

        Assert.AreEqual(before, (uintptr)Ꮡn);
        Assert.AreEqual(beforeVoid, (nuint)(void*)Ꮡn);
    }

    [TestMethod]
    public unsafe void TheNilPointerItselfIsStillZero()
    {
        ж<ж<nint>> nilPointer = nil;
        ж<ж<nint>>? nullReference = null;

        Assert.AreEqual((uintptr)0, (uintptr)nilPointer);
        Assert.AreEqual((nuint)0, (nuint)(void*)nilPointer);
        Assert.AreEqual((uintptr)0, (uintptr)nullReference!);
        Assert.AreEqual((nuint)0, (nuint)(void*)nullReference!);
    }
}
