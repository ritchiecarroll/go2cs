// InlineArrayFamilyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using go;
using go.arrays;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// The inline-array family of go.arrays.dll (docs/phase4/DESIGN-array-value-storage.md, 7.7): a Go array value held
/// inline, laid out as Go lays it out, with Go's index panic and Go's equality.
/// </summary>
[TestClass]
public class InlineArrayFamilyTests
{
    // The CLR alignment of T, read as T's offset after a byte (7.6.3's measure).
    private struct AfterByte<T> where T : unmanaged
    {
        public byte B;
        public T Value;
    }

    private static int AlignmentOf<T>() where T : unmanaged
    {
        AfterByte<T> probe = default;
        return (int)Unsafe.ByteOffset(ref Unsafe.As<AfterByte<T>, byte>(ref probe), ref Unsafe.As<T, byte>(ref probe.Value));
    }

    [TestMethod]
    public void AMemberIsItsElementsLaidOutInline()
    {
        Assert.AreEqual(201, Unsafe.SizeOf<array201<sbyte>>());
        Assert.AreEqual(32, Unsafe.SizeOf<array4<long>>());
        Assert.AreEqual(6, Unsafe.SizeOf<array3<short>>());

        Assert.AreEqual(1, AlignmentOf<array201<sbyte>>(), "the element's alignment");
        Assert.AreEqual(2, AlignmentOf<array3<short>>());
        Assert.AreEqual(8, AlignmentOf<array4<long>>());

        Assert.AreEqual(201, array201<sbyte>.Length);
    }

    [TestMethod]
    public void ElementsAreReadAndWrittenInPlace()
    {
        array4<int> a = default;

        a[0] = 7;
        a[(nint)3] = 9;
        a[0] += 1;
        a[2u] = 5;
        a[(ulong)1] = 4;

        Assert.AreEqual(8, a[0]);
        Assert.AreEqual(4, a[1]);
        Assert.AreEqual(5, a[2]);
        Assert.AreEqual(9, a[(nint)3]);
    }

    [TestMethod]
    public void AnIndexOutOfRangeIsGosPanic()
    {
        array201<sbyte> a = default;

        string Message(Action access)
        {
            try
            {
                access();
            }
            catch (PanicException panic)
            {
                return panic.Message;
            }

            Assert.Fail("an out-of-range index panics");
            return "";
        }

        StringAssert.Contains(Message(() => a[201] = 1), "index out of range [201] with length 201");
        StringAssert.Contains(Message(() => a[-1] = 1), "index out of range [-1]");
        StringAssert.Contains(Message(() => a[(nint)int.MaxValue + 1] = 1), "index out of range [2147483648] with length 201");
        // An unsigned index is checked before any narrowing: Go reports the unsigned value, never a negative one.
        StringAssert.Contains(Message(() => a[ulong.MaxValue] = 1), "index out of range [18446744073709551615] with length 201");
    }

    [TestMethod]
    public void EqualityIsGosElementByElement()
    {
        array4<int> a = default, b = default;
        a[2] = 7;
        b[2] = 7;

        Assert.IsTrue(a == b);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a.Equals((object)b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

        b[3] = 1;

        Assert.IsTrue(a != b);
        Assert.IsFalse(a.Equals(b));

        // Go's ==: a NaN element makes the arrays unequal, and 0 equals -0.
        array2<double> x = default, y = default;
        x[0] = double.NaN;
        y[0] = double.NaN;
        Assert.IsFalse(x == y, "NaN is unequal to NaN in Go");

        x[0] = 0.0;
        y[0] = -0.0;
        Assert.IsTrue(x == y, "0 equals -0 in Go");
        Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
    }

    // 7.6.5: the default equality of an inline array, and of a holder, was order-dependent and WRONG (an unequal map key
    // was found). The family's own equality answers the same in both call orders.
    [TestMethod]
    public void AMapKeyIsFoundOnlyWhenEqualInEitherOrder()
    {
        array8<int> one = default, two = default;
        two[2] = 7;

        // Equality first, then the map.
        Assert.IsFalse(one.Equals(two));
        Dictionary<array8<int>, int> map = new() { [one] = 1 };
        Assert.IsFalse(map.ContainsKey(two));
        Assert.IsTrue(map.ContainsKey(one));

        // The map first, then equality, on a length nothing above has used.
        array16<int> three = default, four = default;
        four[15] = 1;
        Dictionary<array16<int>, int> other = new() { [three] = 1 };
        Assert.IsFalse(other.ContainsKey(four));
        Assert.IsFalse(three.Equals(four));
        Assert.IsFalse(three == four);
    }

    [TestMethod]
    public void GosEqualityReachesTheFamilyWithoutReflection()
    {
        array4<int> a = default, b = default;
        b[1] = 1;

        long before = EqualityFallbacks.ThreadCount;

        Assert.IsTrue(AreEqual((object)a, (object)a));
        Assert.IsFalse(AreEqual((object)a, (object)b));
        Assert.IsTrue(((IGoEquatable)a).GoEquals(a));
        Assert.AreEqual(before, EqualityFallbacks.ThreadCount, "reached the reflective fallback");
    }

    [TestMethod]
    public void EveryMemberCarriesItsLengthForGolib()
    {
        foreach ((Type type, int length) in new[] { (typeof(array1<>), 1), (typeof(array201<>), 201), (typeof(array4096<>), 4096) })
        {
            Assert.AreEqual(length, type.GetCustomAttribute<GoInlineArrayAttribute>()?.Length, type.Name);
            Assert.AreEqual(EditorBrowsableState.Never, type.GetCustomAttribute<EditorBrowsableAttribute>()?.State, type.Name);
        }
    }

    // 7.6.1: the member exposes an element and its length, and nothing that reaches its storage as a whole. Part D3 adds
    // members that COPY rather than reach: the element-list constructor a Go array literal converts to (elements in),
    // ΔRangeSnapshot, the copy a Go range iterates, and ΔCopy and ΔSelect, the copy an interface holds (elements out; C1's
    // review, M1). The reflection face (IGoInlineArray) is explicit.
    [TestMethod]
    public void AMemberExposesOnlyItsElementsAndLength()
    {
        string[] members = typeof(array201<>).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(member => member.Name).Distinct().OrderBy(name => name, StringComparer.Ordinal).ToArray();

        CollectionAssert.AreEqual(new[] { ".ctor", "Equals", "GetHashCode", "Item", "Length", "get_Item", "op_Equality", "op_Inequality", "ΔCopy", "ΔRangeSnapshot", "ΔSelect" }, members, string.Join(", ", members));
    }

    // A Go array literal [5]int32{1, 2} is its elements from index 0 and Go's zero after them.
    [TestMethod]
    public void AnArrayLiteralFillsFromTheStartAndZeroesTheRest()
    {
        array5<int> value = new(1, 2);

        Assert.AreEqual(1, value[0]);
        Assert.AreEqual(2, value[1]);
        Assert.AreEqual(0, value[2]);
        Assert.AreEqual(0, value[4]);
        Assert.AreEqual(default, new array5<int>());
    }

    // C1's review, n1: Go refuses an array literal longer than the array at compile time, so a constructor handed more
    // than N elements is a converter defect, and it is Go's index panic rather than a silent truncation.
    [TestMethod]
    public void ALiteralLongerThanTheArrayPanics()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => new array3<int>(1, 2, 3, 4));
        StringAssert.Contains(panic.Message, "index out of range [3] with length 3");
        Assert.AreEqual(3, new array3<int>(1, 2, 3)[2]);
    }

    // Go ranges over a COPY of an array value: a write the loop body makes is not seen by the loop.
    [TestMethod]
    public void ARangeIteratesACopy()
    {
        array4<int> value = new(1, 2, 3, 4);
        List<int> seen = [];

        foreach (var (i, element) in range(value.ΔRangeSnapshot()))
        {
            if (i == 0)
                value[3] = 99;

            seen.Add(element);
        }

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, seen);
        Assert.AreEqual(99, value[3]);
    }

    // C1's review, M1: an inline array boxed into an interface crosses as its array<T> copy, deep for a nested array, so
    // interface space holds the one form every Go [N]T has there, and the copy aliases nothing the member holds.
    [TestMethod]
    public void TheInterfaceCopyIsADeepGolibArrayThatAliasesNothing()
    {
        array3<array2<short>> nested = new(new array2<short>(1, 2), new array2<short>(3, 4), new array2<short>(5, 6));
        array<array<short>> copy = nested.ΔSelect(static e => e.ΔCopy());

        nested[1][0] = 99;
        Assert.AreEqual((short)3, copy[1][0]);
        Assert.AreEqual((nint)3, copy.Length);
        Assert.AreEqual((nint)2, copy[2].Length);

        array<array<short>> plain = new(new[] { new array<short>(new short[] { 1, 2 }), new array<short>(new short[] { 3, 4 }), new array<short>(new short[] { 5, 6 }) });
        Assert.IsTrue(((object)copy).Equals(plain), "interface == against a plain [3][2]int16");
        Assert.AreEqual(plain.GetHashCode(), copy.GetHashCode(), "the same map key");
        Assert.AreEqual("[[1 2] [3 4] [5 6]]", (string)go.fmt_package.Sprintf("%v", copy));
    }

    // reflect (and fmt through it) walks a boxed MEMBER as a Go array value: its length and a copy of each element. After
    // M1 the automatic rule boxes a member only as an interface argument, which crosses as array<T>; every other boxing
    // (an assignment to an interface, a composite element, a return, a send) keeps the type out. Forced mode does not
    // consult the faces, and there a member boxed those ways reaches interface space as itself: IGoInlineArray and its
    // two reflect arms are kept for that path. (A whole holding struct is not one: reflection over its inline field
    // panics by name, GoReflect.InlineArrays.)
    [TestMethod]
    public void ABoxedMemberIsReadByReflectionThroughItsInlineArrayFace()
    {
        object boxed = new array3<short>(5, 6, 7);

        Assert.IsInstanceOfType(boxed, typeof(IGoInlineArray));
        Assert.IsNotInstanceOfType(boxed, typeof(IArray));
        IGoInlineArray inline = (IGoInlineArray)boxed;
        Assert.AreEqual((nint)3, inline.Length);
        Assert.AreEqual((short)6, inline[1]);

        // The converted reflect reads it through that face (value_impl.cs, Len and Index), and fmt through reflect.
        go.reflect_package.ΔValue value = go.reflect_package.ValueOf(boxed);
        Assert.AreEqual((nint)3, value.Len());
        Assert.AreEqual(6L, (long)value.Index(1).Int());
        Assert.AreEqual("[5 6 7]", (string)go.fmt_package.Sprintf("%v", boxed));
    }

    // P7 by construction: golib names no family type, so a program that does not opt in never loads go.arrays.dll.
    [TestMethod]
    public void GolibDoesNotReferenceTheFamily()
    {
        string[] referenced = typeof(IGoEquatable).Assembly.GetReferencedAssemblies().Select(name => name.Name!).ToArray();

        CollectionAssert.DoesNotContain(referenced, typeof(array1<>).Assembly.GetName().Name, string.Join(", ", referenced));
        Assert.AreEqual("go.arrays", typeof(array1<>).Assembly.GetName().Name);
    }
}
