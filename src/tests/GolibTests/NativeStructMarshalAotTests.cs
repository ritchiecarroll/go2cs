// NativeStructMarshalAotTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// 1.26.10.2 seat #18: NativeStructMarshal answers its type questions without closing a generic method at run time,
/// which a Native AOT image has no code for (a converted TLS program exited 2 in syscall.WSAIoctl). The new answers must
/// equal the generic ones the JIT computes, for every type the marshal can see.
/// </summary>
/// <remarks>
/// The census is every non-generic value type golib and this test assembly declare (the native struct shapes the
/// syscall funnels marshal are among this assembly's), the primitives, golib's scalars and some BCL shapes (generic,
/// nested, pointer- and function-pointer-bearing). The old answers are computed here the way the marshal used to, with
/// MakeGenericMethod, which the JIT can do. The Native AOT half of the seat is GolibAotProbe (src/tests/PackageTests),
/// which runs the marshal's three shapes in a published image.
/// </remarks>
[TestClass]
public unsafe class NativeStructMarshalAotTests
{
    // Shapes only: their types are classified, no value of them is made.
#pragma warning disable CS0649
    private struct WithPointer
    {
        public int A;
        public int* P;
    }

    private struct WithFunctionPointer
    {
        public delegate*<int, int> F;
        public long B;
    }

    private struct Nested
    {
        public WithPointer Inner;
        public (int, double) Pair;
    }

    private struct NestedReference
    {
        public Nested Inner;
        public KeyValuePair<int, string> Entry;
    }
#pragma warning restore CS0649

    private enum Small : byte { A, B }

    private static readonly Type[] s_extra =
    [
        typeof(bool), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
        typeof(ulong), typeof(nint), typeof(nuint), typeof(float), typeof(double), typeof(char), typeof(decimal),
        typeof(Complex), typeof(complex64), typeof(uintptr), typeof(@string), typeof(Small), typeof(DayOfWeek),
        typeof(int?), typeof(Complex?), typeof((int, string)), typeof((byte, ushort, uint)), typeof(KeyValuePair<int, int>),
        typeof(Guid), typeof(DateTime), typeof(TimeSpan), typeof(object), typeof(string), typeof(int[]), typeof(slice<byte>),
        typeof(array<ushort>), typeof(ж<int>), typeof(WithPointer), typeof(WithFunctionPointer), typeof(Nested),
        typeof(NestedReference)
    ];

    private static List<Type> census()
    {
        IEnumerable<Type> declared = new[] { typeof(ж<>).Assembly, typeof(NativeStructMarshalAotTests).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsValueType && !type.ContainsGenericParameters && !type.IsByRefLike);

        return s_extra.Concat(declared).Distinct().ToList();
    }

    private static bool oldContainsReferences(Type type) =>
        (bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!.MakeGenericMethod(type).Invoke(null, null)!;

    private static int oldSizeOf(Type type) =>
        (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!.MakeGenericMethod(type).Invoke(null, null)!;

    [TestMethod]
    public void ContainsReferencesAndSizeOfAnswerAsTheGenericFormsDidForEveryTypeTheMarshalCanSee()
    {
        List<Type> types = census();
        List<string> differences = [];
        int references = 0, scalars = 0;

        foreach (Type type in types)
        {
            bool expected = oldContainsReferences(type);
            bool actual = NativeStructMarshal.ContainsReferences(type);

            if (actual != expected)
                differences.Add($"{type.FullName}: ContainsReferences {actual}, the generic form {expected}");

            int expectedSize = oldSizeOf(type);
            int actualSize = NativeStructMarshal.ClrSizeOf(type);

            if (actualSize != expectedSize)
                differences.Add($"{type.FullName}: ClrSizeOf {actualSize}, the generic form {expectedSize}");

            if (expected)
                references++;
            else
                scalars++;
        }

        Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));

        // The census reaches both answers, and is broad: golib alone declares hundreds of value types.
        Assert.IsTrue(types.Count > 300, $"census of {types.Count} types");
        Assert.IsTrue(references > 50 && scalars > 50, $"{references} reference-bearing, {scalars} reference-free");
        Assert.IsFalse(NativeStructMarshal.ContainsReferences(typeof(WithPointer)), "a pointer field is not a reference");
        Assert.IsFalse(NativeStructMarshal.ContainsReferences(typeof(WithFunctionPointer)), "nor is a function pointer");
        Assert.IsTrue(NativeStructMarshal.ContainsReferences(typeof(NestedReference)), "a nested reference is found");
    }

    private static void oldWrite<T>(object value, nint destination) where T : unmanaged =>
        Unsafe.WriteUnaligned((void*)destination, (T)value);

    private static object oldRead<T>(nint source) where T : unmanaged => Unsafe.ReadUnaligned<T>((void*)source);

    private static void leafCopiesAsTheGenericFormDid<T>(T value, List<string> differences) where T : unmanaged
    {
        NativeStructMarshal.Leaf leaf = NativeStructMarshal.Leaf.Of(typeof(T));

        if (leaf is null)
        {
            differences.Add($"{typeof(T).Name}: no leaf");
            return;
        }

        byte* expected = stackalloc byte[16];
        byte* actual = stackalloc byte[16];
        new Span<byte>(expected, 16).Fill(0xA5);
        new Span<byte>(actual, 16).Fill(0xA5);

        oldWrite<T>(value, (nint)expected);
        leaf.Write(value, actual);

        if (!new Span<byte>(expected, 16).SequenceEqual(new Span<byte>(actual, 16)))
            differences.Add($"{typeof(T).Name}: written bytes differ");

        object read = leaf.Read(expected);

        if (read.GetType() != typeof(T) || !read.Equals(oldRead<T>((nint)expected)))
            differences.Add($"{typeof(T).Name}: read {read} ({read.GetType().Name})");

        if (leaf.Size != sizeof(T))
            differences.Add($"{typeof(T).Name}: size {leaf.Size}");
    }

    [TestMethod]
    public void LeafCopiesTheBytesTheGenericCopiersDidAndRefusesAForeignBox()
    {
        List<string> differences = [];

        leafCopiesAsTheGenericFormDid(true, differences);
        leafCopiesAsTheGenericFormDid((sbyte)-5, differences);
        leafCopiesAsTheGenericFormDid((byte)0xFE, differences);
        leafCopiesAsTheGenericFormDid((short)-1234, differences);
        leafCopiesAsTheGenericFormDid((ushort)0xBEEF, differences);
        leafCopiesAsTheGenericFormDid(-123456, differences);
        leafCopiesAsTheGenericFormDid(0xDEADBEEFu, differences);
        leafCopiesAsTheGenericFormDid(-1234567890123L, differences);
        leafCopiesAsTheGenericFormDid(0xFEDCBA9876543210UL, differences);
        leafCopiesAsTheGenericFormDid((nint)(-77), differences);
        leafCopiesAsTheGenericFormDid((nuint)99, differences);
        leafCopiesAsTheGenericFormDid(1.5f, differences);
        leafCopiesAsTheGenericFormDid(-2.25, differences);
        leafCopiesAsTheGenericFormDid(new Complex(1.5, -2.5), differences);

        Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));

        NativeStructMarshal.Leaf leaf = NativeStructMarshal.Leaf.Of(typeof(ushort))!;
        long buffer = 0;
        nint address = (nint)(&buffer);
        Assert.ThrowsException<InvalidCastException>(() => leaf.WriteRaw(5, address), "a boxed int is not a ushort, as (ushort)value said");
        Assert.AreEqual(0L, buffer, "and nothing was written");
    }
}
