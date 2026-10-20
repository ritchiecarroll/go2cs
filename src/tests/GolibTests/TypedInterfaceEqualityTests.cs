// TypedInterfaceEqualityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Numerics;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using fs = go.io.fs_package;
using syscall = go.syscall_package;
using unique = go.unique_package;

namespace GolibTests;

/// <summary>
/// Go's <c>==</c> between two interface values of one dynamic type (<c>err == syscall.EAGAIN</c>, <c>err == io.EOF</c>)
/// reaches <c>builtin.AreEqual(object?, object?)</c>, which found the type's operator by reflection and invoked it: 85-156 ns
/// and 144 bytes a compare (P2, linux). It now calls the type's own <c>==</c> in compiled code (IGoEquatable, which
/// go2cs-gen's templates and golib's comparable types implement), or a boxed primitive's own operator. Each arm uses a
/// REAL generated type from a converted package, so the generator's emission is what is tested: a named basic
/// (syscall.Errno), a struct whose interface-typed field recurses through AreEqual (fs.PathError), and a generic
/// struct instance (unique.Handle). The reflective path stays the fallback for anything else and counts what reaches it
/// (EqualityFallbacks): every arm here must leave that count unmoved and allocate nothing.
/// </summary>
[TestClass]
public class TypedInterfaceEqualityTests
{
    private static void AssertTyped(object left, object right, bool want, string what)
    {
        long before = EqualityFallbacks.ThreadCount;
        Assert.AreEqual(want, AreEqual(left, right), $"{what}: Go's == answer");
        Assert.AreEqual(before, EqualityFallbacks.ThreadCount, $"{what}: reached the reflective fallback");
    }

    [TestMethod]
    public void ANamedBasicComparesThroughItsOwnOperator()
    {
        AssertTyped(syscall.EAGAIN, syscall.EAGAIN, true, "Errno == Errno");
        AssertTyped(syscall.EAGAIN, syscall.EINTR, false, "Errno != Errno");
    }

    [TestMethod]
    public void AStructWithAnInterfaceFieldStaysTypedThroughTheRecursion()
    {
        error boom = errors_package.New("boom");
        object a = new fs.PathError(Op: "read", Path: "p", Err: boom);
        object b = new fs.PathError(Op: "read", Path: "p", Err: boom);
        object c = new fs.PathError(Op: "read", Path: "p", Err: errors_package.New("boom"));

        AssertTyped(a, b, true, "PathError, one error value");
        // Two errors.New values are two pointers: Go compares the field by identity, so the structs differ.
        AssertTyped(a, c, false, "PathError, two error values");
    }

    [TestMethod]
    public void AGenericStructInstanceComparesThroughItsOwnOperator()
    {
        object a = unique.Make<@string>("x");
        object b = unique.Make<@string>("x");
        object c = unique.Make<@string>("y");

        AssertTyped(a, b, true, "unique.Handle[string], one value");
        AssertTyped(a, c, false, "unique.Handle[string], two values");
    }

    [TestMethod]
    public void GolibTypesAndPrimitivesKeepGoSemantics()
    {
        ж<nint> p = new StandardBox<nint>(1), q = new StandardBox<nint>(1);
        AssertTyped(p, p, true, "*int, one pointer");
        AssertTyped(p, q, false, "*int, two pointers to equal values");

        AssertTyped((@string)"go", (@string)"go", true, "string");
        AssertTyped((nint)7, (nint)7, true, "int");
        AssertTyped(true, false, false, "bool");

        // IEEE: NaN is unequal to itself through an interface, as in Go.
        AssertTyped(double.NaN, double.NaN, false, "float64 NaN");
        AssertTyped(float.NaN, float.NaN, false, "float32 NaN");
        AssertTyped(new Complex(double.NaN, 0), new Complex(double.NaN, 0), false, "complex128 NaN");
        AssertTyped(new complex64(float.NaN, 0), new complex64(float.NaN, 0), false, "complex64 NaN");
    }

    [TestMethod]
    public void AnUncomparableDynamicTypeStillPanicsWithGosText()
    {
        object s = new nint[] { 1 }.slice();

        PanicException panic = Assert.ThrowsException<PanicException>(() => AreEqual(s, s));
        StringAssert.Contains(panic.Message, "comparing uncomparable type []int");
    }

    [TestMethod]
    public void AFuncDynamicTypeIsUncomparableWhicheverSideIsNil()
    {
        // A nil func keeps its type inside an interface (the NilFuncValue carrier), so it is no exemption: Go panics
        // comparing two interface values that hold one func type, nil or not, and answers against the nil interface or
        // another dynamic type.
        Action none = null;
        Action some = () => { };
        object nilFunc = none.OrTypedNilFunc(), func = some.OrTypedNilFunc();

        foreach ((object left, object right, string what) in new[] { (nilFunc, nilFunc, "nil == nil"), (nilFunc, func, "nil == func"), (func, nilFunc, "func == nil") })
        {
            PanicException panic = Assert.ThrowsException<PanicException>(() => AreEqual(left, right), what);
            StringAssert.Contains(panic.Message, "comparing uncomparable type func()", what);
        }

        Assert.IsFalse(AreEqual(nilFunc, null), "nil func == nil interface");
        Assert.IsFalse(AreEqual(nilFunc, (nint)1), "nil func == int");
    }

    [TestMethod]
    public void ATypedCompareAllocatesNothing()
    {
        object a = syscall.EAGAIN, b = syscall.EAGAIN;
        error boom = errors_package.New("boom");
        object x = new fs.PathError(Op: "read", Path: "p", Err: boom), y = new fs.PathError(Op: "read", Path: "p", Err: boom);

        for (int i = 0; i < 100; i++)
        {
            _ = AreEqual(a, b);
            _ = AreEqual(x, y);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 1000; i++)
        {
            _ = AreEqual(a, b);
            _ = AreEqual(x, y);
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(0L, allocated, $"2,000 typed compares allocated {allocated} bytes");
    }
}
