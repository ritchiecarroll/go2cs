// GoDirectIfaceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards <see cref="GoReflect.GoIsDirectIface"/> at Go 1.26's rule, <c>Size_ == PtrBytes == PtrSize</c>
/// (cmd/compile types.IsDirectIface), and <see cref="GoReflect.GoDirectIfaceWordIsNil"/>'s reduction to the
/// one pointer such a type holds.
/// </summary>
/// <remarks>
/// Through Go 1.25 the rule was structural: a struct needed exactly one field. The two rules part on a
/// struct whose other fields are all zero-size, which is sync/atomic's <c>Pointer[T]</c> shape
/// (<c>_ [0]*T; _ noCopy; v unsafe.Pointer</c>): 1.26 stamps it <c>TFlagDirectIface</c>. A trailing
/// zero-size field is the case that stays indirect under both, because Go pads it to 16 bytes.
/// </remarks>
[TestClass]
public class GoDirectIfaceTests
{
#pragma warning disable CS0649
    private struct Empty { }
    private struct OnePointer { public ж<byte> p; }
    private struct ZeroArrayThenPointer { public array<ж<byte>> z = new(0); public ж<byte> p; public ZeroArrayThenPointer() { } }
    private struct EmptyThenPointer { public Empty z; public ж<byte> p; }
    private struct PointerThenEmpty { public ж<byte> p; public Empty z; }
    private struct TwoPointers { public ж<byte> p; public ж<byte> q; }
    private struct PointerSizedScalar { public nuint x; }
#pragma warning restore CS0649

    [TestMethod]
    public void APointerAndAOneFieldStructOfOneArePointerShaped()
    {
        Assert.IsTrue(GoReflect.GoIsDirectIface(typeof(ж<byte>)));
        Assert.IsTrue(GoReflect.GoIsDirectIface(typeof(OnePointer)));
        Assert.IsTrue(GoReflect.GoIsDirectIface(typeof(array<ж<byte>>), [1]), "[1]*byte");
        Assert.IsFalse(GoReflect.GoIsDirectIface(typeof(array<ж<byte>>), [0]), "[0]*byte");
    }

    [TestMethod]
    public void ZeroSizeFieldsBeforeThePointerArePointerShapedAtGo126()
    {
        // Size 8, PtrBytes 8: direct at 1.26, where the 1.25 one-field rule said indirect.
        Assert.IsTrue(GoReflect.GoIsDirectIface(typeof(ZeroArrayThenPointer)), "struct{ z [0]*byte; p *byte }");
        Assert.IsTrue(GoReflect.GoIsDirectIface(typeof(EmptyThenPointer)), "struct{ z struct{}; p *byte }");
    }

    [TestMethod]
    public void EverythingElseIsNot()
    {
        // A trailing zero-size field is padded: size 16.
        Assert.AreEqual((nint)16, GoReflect.GoSizeOf(typeof(PointerThenEmpty)));
        Assert.IsFalse(GoReflect.GoIsDirectIface(typeof(PointerThenEmpty)), "struct{ p *byte; z struct{} }");
        Assert.IsFalse(GoReflect.GoIsDirectIface(typeof(TwoPointers)));
        Assert.IsFalse(GoReflect.GoIsDirectIface(typeof(PointerSizedScalar)), "one word, no pointer");
        Assert.IsFalse(GoReflect.GoIsDirectIface(typeof(@string)));
    }

    [TestMethod]
    public void TheWordIsTheNonZeroSizeField()
    {
        ZeroArrayThenPointer zero = new();
        Assert.IsTrue(GoReflect.GoDirectIfaceWordIsNil(zero, typeof(ZeroArrayThenPointer)), "the zero value's word is nil");

        ZeroArrayThenPointer live = new() { p = new StandardBox<byte>(7) };
        Assert.IsFalse(GoReflect.GoDirectIfaceWordIsNil(live, typeof(ZeroArrayThenPointer)), "a live pointer's word is not nil");
    }
}
