// NumericWrapperConversionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// golib's type-parameter conversions reach a NAMED numeric type through IGoNumericWrapper<T>, the members go2cs-gen
// gives every [GoType("num:…")] wrapper, and not through reflection. GenTests' NumericWrapperConversionTests runs the
// generated members; these rows hold golib's half: the dispatch, the refusal, and the trim contract.
[TestClass]
public class NumericWrapperConversionTests
{
    // Written as go2cs-gen writes a wrapper over uint8 (`type small uint8`).
    private readonly struct Small : IGoNumericWrapper<Small>
    {
        public readonly byte Value;

        public Small(byte value) => Value = value;

        Small IGoNumericWrapper<Small>.FromUInt64(ulong value) => new(builtin.ConvertToType<byte>(value));

        Small IGoNumericWrapper<Small>.FromInt64(long value) => new(builtin.ConvertToType<byte>(value));

        Small IGoNumericWrapper<Small>.FromFloat64(double value) => new(builtin.ConvertToType<byte>(value));

        ulong IGoNumericWrapper<Small>.ToUInt64(Small value) => builtin.ConvertToUInt64(value.Value);

        double IGoNumericWrapper<Small>.ToFloat64(Small value) => builtin.ConvertToFloat64(value.Value);

        float IGoNumericWrapper<Small>.ToFloat32(Small value) => builtin.ConvertToFloat32(value.Value);
    }

    // A struct with the Value-and-constructor shape the reflective probe used to read, and no interface.
    private readonly struct Bare
    {
        public readonly int Value;

        public Bare(int value) => Value = value;
    }

    [TestMethod]
    public void EachConversionReachesTheWrapper()
    {
        // Go truncates: small(300) is 44, and small(int64(-1)) is 255.
        Assert.AreEqual((byte)44, builtin.ConvertToType<Small>(300UL).Value);
        Assert.AreEqual((byte)255, builtin.ConvertToType<Small>(-1L).Value);
        Assert.AreEqual((byte)200, builtin.ConvertToType<Small>(200.7).Value);
        Assert.AreEqual(200UL, builtin.ConvertToUInt64(new Small(200)));
        Assert.AreEqual(7.0, builtin.ConvertToFloat64(new Small(7)));
        Assert.AreEqual(9f, builtin.ConvertToFloat32(new Small(9)));
    }

    // A float kind converts an integer by the SOURCE's signedness, as Go's float64(x) does: float64(uint64(1<<64-1))
    // is 1.8446744073709552e+19 and float64(int64(-2)) is -2. Before the signed overload kept its sign, both forwarded
    // through one ulong, so the second could only read as 1.8446744073709552e+19 too. uint64(f) of a float is gc's
    // float-to-integer conversion, the ConvertToType(double) overload's.
    [TestMethod]
    public void AFloatKindConvertsAnIntegerByItsSignedness()
    {
        Assert.AreEqual(18446744073709551615.0, builtin.ConvertToType<double>(ulong.MaxValue));
        Assert.AreEqual(-2.0, builtin.ConvertToType<double>(-2L));
        Assert.AreEqual(1.8446744E+19f, builtin.ConvertToType<float>(ulong.MaxValue));
        Assert.AreEqual(-2f, builtin.ConvertToType<float>(-2L));
        Assert.AreEqual(2UL, builtin.ConvertToUInt64(2.9));
        Assert.AreEqual(builtin.ConvertToType<ulong>(1e300), builtin.ConvertToUInt64(1e300));
        Assert.AreEqual(7UL, builtin.ConvertToUInt64(7.5f));

        // The named byte takes the same split: small(int64(-1)) wraps to 255, small(uint64(1<<64-1)) to 255.
        Assert.AreEqual((byte)255, builtin.ConvertToType<Small>(ulong.MaxValue).Value);
    }

    [TestMethod]
    public void ATypeThatIsNotAWrapperIsRefusedByName()
    {
        NotSupportedException from = Assert.ThrowsException<NotSupportedException>(() => builtin.ConvertToType<Bare>(1UL));
        Assert.AreEqual($"ConvertToType: no numeric wrapper surface on {typeof(Bare)}", from.Message);

        NotSupportedException to = Assert.ThrowsException<NotSupportedException>(() => builtin.ConvertToUInt64(new Bare(1)));
        Assert.AreEqual($"ConvertToUInt64: no numeric wrapper surface on {typeof(Bare)}", to.Message);

        Assert.AreEqual($"ConvertToFloat64: no numeric wrapper surface on {typeof(Bare)}",
            Assert.ThrowsException<NotSupportedException>(() => builtin.ConvertToFloat64(new Bare(1))).Message);
        Assert.AreEqual($"ConvertToFloat32: no numeric wrapper surface on {typeof(Bare)}",
            Assert.ThrowsException<NotSupportedException>(() => builtin.ConvertToFloat32(new Bare(1))).Message);
    }

    // The trim contract: a converted generic function's type parameter flows into these methods, and the converter
    // writes no annotation on its own type parameters, so an annotation here is an IL2091 warning at every such call
    // site (37 of the 42 IL2091 across 10 behavioral projects at 4a56f17a3a) and a promise the trimmer then keeps for nothing.
    [TestMethod]
    public void NoConversionAsksTheTrimmerForMembers()
    {
        MethodInfo[] conversions = typeof(builtin).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name is "ConvertToType" or "ConvertToUInt64" or "ConvertToFloat64" or "ConvertToFloat32")
            .ToArray();

        Assert.AreEqual(6, conversions.Length, "the conversion set changed: " + string.Join(", ", conversions.Select(method => method.ToString())));

        string[] annotated = conversions
            .Where(method => method.GetGenericArguments().Any(parameter => parameter.IsDefined(typeof(DynamicallyAccessedMembersAttribute))))
            .Select(method => method.ToString()!)
            .ToArray();

        Assert.AreEqual(0, annotated.Length, "annotated type parameters: " + string.Join(", ", annotated));
    }
}
