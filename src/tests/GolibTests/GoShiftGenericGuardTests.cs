// GoShiftGenericGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards golib's generic <c>GoShift</c> guards for a type parameter target and a map element target. .NET's
/// IShiftOperators masks a type argument's count to its width (<c>(byte)(x &lt;&lt; (n &amp; 7))</c>), so a
/// converted <c>x &lt;&lt;= 9</c> on a uint8 type argument read as <c>x &lt;&lt; 1</c>. The generic guards take
/// the width from the type argument's size. They are additive: a basic receiver must still bind its
/// non-generic overload, which the binding test reads from the compiler's own choice in an expression tree.
/// </summary>
[TestClass]
public class GoShiftGenericGuardTests
{
    // Read through fields so nothing folds at compile time.
    private static uint64 s_three = 3;
    private static uint64 s_eight = 8;
    private static uint64 s_nine = 9;
    private static uint64 s_forty = 40;
    private static int64 s_minusOne = -1;
    private static int64 s_seventy = 70;

    private static T Shl<T>(T x, uint64 n) where T : IShiftOperators<T, int, T> => x.Lsh(n);

    private static T Shr<T>(T x, uint64 n) where T : IShiftOperators<T, int, T> => x.Rsh(n);

    private static T ShrSigned<T>(T x, int64 n) where T : IShiftOperators<T, int, T> => x.Rsh(n);

    private static MethodInfo Bound(LambdaExpression shift) => ((MethodCallExpression)shift.Body).Method;

    private static void AssertNonGeneric(LambdaExpression shift, Type receiver, Type count)
    {
        MethodInfo method = Bound(shift);
        ParameterInfo[] parameters = method.GetParameters();

        Assert.IsFalse(method.IsGenericMethod, $"{method} on {receiver.Name} bound a generic overload");
        Assert.AreEqual(typeof(GoShift), method.DeclaringType);
        Assert.AreEqual(receiver, parameters[0].ParameterType, $"{method} receiver");
        Assert.AreEqual(count, parameters[1].ParameterType, $"{method} count");
    }

    [TestMethod]
    public void EveryBasicReceiverStillBindsItsNonGenericOverload()
    {
        AssertNonGeneric((Expression<Func<uint64, uint64, uint64>>)((x, n) => x.Rsh(n)), typeof(uint64), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint64, uint64, uint64>>)((x, n) => x.Lsh(n)), typeof(uint64), typeof(uint64));
        AssertNonGeneric((Expression<Func<int64, uint64, int64>>)((x, n) => x.Rsh(n)), typeof(int64), typeof(uint64));
        AssertNonGeneric((Expression<Func<int64, uint64, int64>>)((x, n) => x.Lsh(n)), typeof(int64), typeof(uint64));
        AssertNonGeneric((Expression<Func<nuint, uint64, nuint>>)((x, n) => x.Rsh(n)), typeof(nuint), typeof(uint64));
        AssertNonGeneric((Expression<Func<nuint, uint64, nuint>>)((x, n) => x.Lsh(n)), typeof(nuint), typeof(uint64));
        AssertNonGeneric((Expression<Func<nint, uint64, nint>>)((x, n) => x.Rsh(n)), typeof(nint), typeof(uint64));
        AssertNonGeneric((Expression<Func<nint, uint64, nint>>)((x, n) => x.Lsh(n)), typeof(nint), typeof(uint64));
        AssertNonGeneric((Expression<Func<uintptr, uint64, uintptr>>)((x, n) => x.Rsh(n)), typeof(uintptr), typeof(uint64));
        AssertNonGeneric((Expression<Func<uintptr, uint64, uintptr>>)((x, n) => x.Lsh(n)), typeof(uintptr), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint32, uint64, uint32>>)((x, n) => x.Rsh(n)), typeof(uint32), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint32, uint64, uint32>>)((x, n) => x.Lsh(n)), typeof(uint32), typeof(uint64));
        AssertNonGeneric((Expression<Func<int32, uint64, int32>>)((x, n) => x.Rsh(n)), typeof(int32), typeof(uint64));
        AssertNonGeneric((Expression<Func<int32, uint64, int32>>)((x, n) => x.Lsh(n)), typeof(int32), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint16, uint64, uint16>>)((x, n) => x.Rsh(n)), typeof(uint16), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint16, uint64, uint16>>)((x, n) => x.Lsh(n)), typeof(uint16), typeof(uint64));
        AssertNonGeneric((Expression<Func<int16, uint64, int16>>)((x, n) => x.Rsh(n)), typeof(int16), typeof(uint64));
        AssertNonGeneric((Expression<Func<int16, uint64, int16>>)((x, n) => x.Lsh(n)), typeof(int16), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint8, uint64, uint8>>)((x, n) => x.Rsh(n)), typeof(uint8), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint8, uint64, uint8>>)((x, n) => x.Lsh(n)), typeof(uint8), typeof(uint64));
        AssertNonGeneric((Expression<Func<int8, uint64, int8>>)((x, n) => x.Rsh(n)), typeof(int8), typeof(uint64));
        AssertNonGeneric((Expression<Func<int8, uint64, int8>>)((x, n) => x.Lsh(n)), typeof(int8), typeof(uint64));

        // The signed-count overloads, and counts of other types. A nuint count converts only to uint64; a uint32
        // count converts to both, and C# prefers the signed int64, as it did before the generic overloads existed
        // (the converter widens every count explicitly, so it never relies on either).
        AssertNonGeneric((Expression<Func<uint64, int64, uint64>>)((x, n) => x.Rsh(n)), typeof(uint64), typeof(int64));
        AssertNonGeneric((Expression<Func<nint, int64, nint>>)((x, n) => x.Lsh(n)), typeof(nint), typeof(int64));
        AssertNonGeneric((Expression<Func<uint32, int64, uint32>>)((x, n) => x.Rsh(n)), typeof(uint32), typeof(int64));
        AssertNonGeneric((Expression<Func<uint8, int64, uint8>>)((x, n) => x.Lsh(n)), typeof(uint8), typeof(int64));
        AssertNonGeneric((Expression<Func<int8, int64, int8>>)((x, n) => x.Rsh(n)), typeof(int8), typeof(int64));
        AssertNonGeneric((Expression<Func<uint8, nuint, uint8>>)((x, n) => x.Rsh(n)), typeof(uint8), typeof(uint64));
        AssertNonGeneric((Expression<Func<uint32, uint32, uint32>>)((x, n) => x.Lsh(n)), typeof(uint32), typeof(int64));
    }

    [TestMethod]
    public void ATypeArgumentShiftsByGosRule()
    {
        Assert.AreEqual((uint8)0, Shl((uint8)0xFF, s_nine), "uint8 << 9 is 0, not << 1");
        Assert.AreEqual((uint8)0xF8, Shl((uint8)0xFF, s_three));
        Assert.AreEqual((uint8)0, Shr((uint8)0xFF, s_eight));
        Assert.AreEqual((uint32)0, Shl((uint32)0xFF, s_forty), "uint32 << 40 is 0, not << 8");
        Assert.AreEqual((uint32)0xFF00, Shl((uint32)0xFF, s_eight));
        Assert.AreEqual((int8)(-1), Shr((int8)(-8), s_nine), "a signed right shift past the width sign-fills");
        Assert.AreEqual((int8)0, Shr((int8)64, s_nine));
        Assert.AreEqual((int64)(-1), ShrSigned((int64)(-8), s_seventy));
        Assert.AreEqual((uint64)0, Shl(ulong.MaxValue, 64UL));

        PanicException panic = Assert.ThrowsException<PanicException>(() => ShrSigned((int8)1, s_minusOne));
        StringAssert.Contains(panic.Message, "runtime error: negative shift amount");
    }

    [TestMethod]
    public void TheRefAndMapElementTwinsEvaluateTheirTargetOnce()
    {
        uint8[] slots = [0xFF, 0xFF];
        int reads = 0;

        GoShift.LshAssign(ref slots[Index()], s_nine);
        Assert.AreEqual((uint8)0, slots[1]);
        Assert.AreEqual(1, reads);

        map<nint, uint32> m = new() { [1] = 0xFFFFFFFF };
        reads = 0;

        m.RshAssign(Key(), s_forty);
        Assert.AreEqual((uint32)0, m[1], "uint32 >> 40 is 0, not >> 8");
        Assert.AreEqual(1, reads);

        m.LshAssign(7, s_eight);
        Assert.IsTrue(m.ContainsKey(7), "a missing key is inserted at its zero value, as Go's m[k] <<= n does");

        Assert.ThrowsException<PanicException>(() => m.LshAssign(1, s_minusOne));

        int Index() { reads++; return 1; }
        nint Key() { reads++; return 1; }
    }
}
