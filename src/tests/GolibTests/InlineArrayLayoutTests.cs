// InlineArrayLayoutTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using go.arrays;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// golib's reflection over the inline-array family, and the layout gate (docs/phase4/DESIGN-array-value-storage.md, 7.6.3
/// and 7.7.4): a family member is a Go array to reflection, a holder's Go layout derives, the gate proves S22 and NAMES
/// the controls, and value-level reflection over an inline field panics naming the field.
/// </summary>
[TestClass]
public class InlineArrayLayoutTests
{
    // sqlite's S22 as I0 emits it: struct{ FforceAlignment [0]int16; Fa [201]int8; F__ccgo_pad2 [1]byte }. Go: size 202,
    // alignment 2. The [0]int16 is a readonly short carrier at its Go offset, sharing bytes with Fa.
    [StructLayout(LayoutKind.Explicit)]
    private struct S22 : IGoManagedLayout
    {
        [FieldOffset(0), GoAlignmentCarrier(typeof(array<short>))] public readonly short FforceAlignment;
        [FieldOffset(0)] public array201<sbyte> Fa;
        [FieldOffset(201)] public array1<byte> F__ccgo_pad2;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                S22 probe = default;
                return new(["FforceAlignment", "Fa", "F__ccgo_pad2"],
                    [GoManagedLayout.OffsetOf(in probe, in probe.FforceAlignment), GoManagedLayout.OffsetOf(in probe, in probe.Fa), GoManagedLayout.OffsetOf(in probe, in probe.F__ccgo_pad2)],
                    Unsafe.SizeOf<S22>(), GoManagedLayout.AlignmentOf<S22>());
            }
        }
    }

    // C1's stop line S2: S22 with its carrier DELETED from the declaration. Without the converter's stamp the gate derives
    // Go's alignment from this declaration and passes it; the stamp (go/types: size 202, alignment 2) still says 2.
    [GoInlineLayout(Size = 202, Align = 2)]
    [StructLayout(LayoutKind.Explicit, Size = 202)]
    private struct S22Deleted : IGoManagedLayout
    {
        [FieldOffset(0)] public array201<sbyte> Fa;
        [FieldOffset(201)] public array1<byte> F__ccgo_pad2;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                S22Deleted probe = default;
                return new(["Fa", "F__ccgo_pad2"], [GoManagedLayout.OffsetOf(in probe, in probe.Fa), GoManagedLayout.OffsetOf(in probe, in probe.F__ccgo_pad2)],
                    Unsafe.SizeOf<S22Deleted>(), GoManagedLayout.AlignmentOf<S22Deleted>());
            }
        }
    }

    // ...held by value, as the converter lays it out: S at Go's offset 2, size 204, alignment 2.
    [GoInlineLayout(Size = 204, Align = 2)]
    [StructLayout(LayoutKind.Explicit, Size = 204)]
    private struct HoldsS22Deleted : IGoManagedLayout
    {
        [FieldOffset(0)] public byte B;
        [FieldOffset(2)] public S22Deleted S;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                HoldsS22Deleted probe = default;
                return new(["B", "S"], [GoManagedLayout.OffsetOf(in probe, in probe.B), GoManagedLayout.OffsetOf(in probe, in probe.S)],
                    Unsafe.SizeOf<HoldsS22Deleted>(), GoManagedLayout.AlignmentOf<HoldsS22Deleted>());
            }
        }
    }

    // struct{ B byte; S S22 }: Go puts S at 2, size 204.
    [StructLayout(LayoutKind.Explicit)]
    private struct HoldsS22 : IGoManagedLayout
    {
        [FieldOffset(0)] public byte B;
        [FieldOffset(2)] public S22 S;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                HoldsS22 probe = default;
                return new(["B", "S"], [GoManagedLayout.OffsetOf(in probe, in probe.B), GoManagedLayout.OffsetOf(in probe, in probe.S)],
                    Unsafe.SizeOf<HoldsS22>(), GoManagedLayout.AlignmentOf<HoldsS22>());
            }
        }
    }

    // THE BROKEN CONTROL (7.6.3): S22 whose carrier does not carry int16's alignment. Go's size and Go's offsets, and
    // the CLR alignment of 1 where Go's is 2.
    [StructLayout(LayoutKind.Explicit)]
    private struct S22NoCarrier : IGoManagedLayout
    {
        [FieldOffset(0), GoAlignmentCarrier(typeof(array<short>))] public readonly byte FforceAlignment;
        [FieldOffset(0)] public array201<sbyte> Fa;
        [FieldOffset(201)] public array1<byte> F__ccgo_pad2;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                S22NoCarrier probe = default;
                return new(["FforceAlignment", "Fa", "F__ccgo_pad2"],
                    [GoManagedLayout.OffsetOf(in probe, in probe.FforceAlignment), GoManagedLayout.OffsetOf(in probe, in probe.Fa), GoManagedLayout.OffsetOf(in probe, in probe.F__ccgo_pad2)],
                    Unsafe.SizeOf<S22NoCarrier>(), GoManagedLayout.AlignmentOf<S22NoCarrier>());
            }
        }
    }

    // Where the missing alignment does its damage: a CLR container places the uncarried S22 at 1, Go at 2.
    private struct HoldsS22NoCarrier : IGoManagedLayout
    {
        public byte B;
        public S22NoCarrier S;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                HoldsS22NoCarrier probe = default;
                return new(["B", "S"], [GoManagedLayout.OffsetOf(in probe, in probe.B), GoManagedLayout.OffsetOf(in probe, in probe.S)],
                    Unsafe.SizeOf<HoldsS22NoCarrier>(), GoManagedLayout.AlignmentOf<HoldsS22NoCarrier>());
            }
        }
    }

    // THE SECOND CONTROL: Go's offsets are unknown (an array of arrays whose inner length nothing carries).
    private struct NullOffsets : IGoManagedLayout
    {
        public array<array<int>> A;

        GoManagedLayout IGoManagedLayout.ManagedLayout => new(["A"], [0], 8, 8);
    }

    // struct{ A int32 } with a CLR size of 8.
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private struct WrongSize : IGoManagedLayout
    {
        [FieldOffset(0)] public int A;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                WrongSize probe = default;
                return new(["A"], [GoManagedLayout.OffsetOf(in probe, in probe.A)], Unsafe.SizeOf<WrongSize>(), GoManagedLayout.AlignmentOf<WrongSize>());
            }
        }
    }

    // struct{ B byte; A int32 } with A at CLR offset 8.
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    private struct WrongOffset : IGoManagedLayout
    {
        [FieldOffset(0)] public byte B;
        [FieldOffset(8)] public int A;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                WrongOffset probe = default;
                return new(["B", "A"], [GoManagedLayout.OffsetOf(in probe, in probe.B), GoManagedLayout.OffsetOf(in probe, in probe.A)],
                    Unsafe.SizeOf<WrongOffset>(), GoManagedLayout.AlignmentOf<WrongOffset>());
            }
        }
    }

    // A struct that measures nothing, held in an array by a holder that does.
    private struct Unmeasured
    {
        public int X;
    }

    private struct HoldsUnmeasured : IGoManagedLayout
    {
        public array2<Unmeasured> P;

        GoManagedLayout IGoManagedLayout.ManagedLayout
        {
            get
            {
                HoldsUnmeasured probe = default;
                return new(["P"], [GoManagedLayout.OffsetOf(in probe, in probe.P)], Unsafe.SizeOf<HoldsUnmeasured>(), GoManagedLayout.AlignmentOf<HoldsUnmeasured>());
            }
        }
    }

    [TestMethod]
    public void AMemberIsAGoArrayToReflection()
    {
        Assert.AreEqual(GoReflect.Array, GoReflect.KindOf(typeof(array201<sbyte>)));
        Assert.AreEqual(typeof(sbyte), GoReflect.ElementType(typeof(array201<sbyte>)));
        Assert.AreEqual(201, GoReflect.InlineArrayLength(typeof(array201<sbyte>)));
        Assert.AreEqual(0, GoReflect.InlineArrayLength(typeof(array<sbyte>)));
        Assert.AreEqual(201, GoReflect.GoSizeOf(typeof(array201<sbyte>)));
        Assert.AreEqual(8, GoReflect.GoAlignOf(typeof(array4<long>)));

        // [4][3]int32: the dims are the types', outer first.
        Assert.AreEqual(48, GoReflect.GoSizeOf(typeof(array4<array3<int>>)));
        CollectionAssert.AreEqual(new nint[] { 4, 3 }, GoReflect.ArrayDimsOfValue(default(array4<array3<int>>)));

        Assert.IsInstanceOfType(GoReflect.ZeroValueOf(typeof(array4<int>), [4]), typeof(array4<int>));
    }

    [TestMethod]
    public void AHoldersGoLayoutDerivesFromItsInlineFields()
    {
        CollectionAssert.AreEqual(new nint[] { 0, 0, 201 }, GoReflect.GoFieldOffsets(typeof(S22)));
        Assert.AreEqual(202, GoReflect.GoSizeOf(typeof(S22)));
        Assert.AreEqual(2, GoReflect.GoAlignOf(typeof(S22)));

        CollectionAssert.AreEqual(new nint[] { 0, 2 }, GoReflect.GoFieldOffsets(typeof(HoldsS22)));
        Assert.AreEqual(204, GoReflect.GoSizeOf(typeof(HoldsS22)));

        // The carrier is projected as the Go field it stands for: [0]int16.
        GoReflect.GoFieldInfo carrier = GoReflect.GoFields(typeof(S22))[0];
        Assert.AreEqual("FforceAlignment", carrier.Name);
        Assert.AreEqual(typeof(array<short>), carrier.Type);
        CollectionAssert.AreEqual(new nint[] { 0 }, carrier.ArrayDims);
    }

    [TestMethod]
    public void TheGateProvesS22AtEveryLevel()
    {
        Assert.IsNull(GoReflect.InlineLayoutMismatch(typeof(S22)));
        Assert.IsNull(GoReflect.InlineLayoutMismatch(typeof(HoldsS22)));

        // ClrLayoutMismatch, the native box's guard, answers through the gate for a struct that measures itself.
        Assert.IsNull(GoReflect.ClrLayoutMismatch(typeof(S22)));
        Assert.IsNotNull(GoReflect.GoFieldOffsets(typeof(S22)));
    }

    [TestMethod]
    public void TheGateNamesS22WithoutItsCarrier()
    {
        Assert.AreEqual(Unsafe.SizeOf<S22>(), Unsafe.SizeOf<S22NoCarrier>(), "the control has Go's size");
        CollectionAssert.AreEqual(GoReflect.GoFieldOffsets(typeof(S22)), GoReflect.GoFieldOffsets(typeof(S22NoCarrier)), "and Go's offsets");

        Assert.AreEqual("S22NoCarrier aligns to 1 in the CLR where Go aligns it to 2", GoReflect.InlineLayoutMismatch(typeof(S22NoCarrier)));
        Assert.AreEqual("HoldsS22NoCarrier.S is at CLR offset 1 where Go lays it at 2", GoReflect.InlineLayoutMismatch(typeof(HoldsS22NoCarrier)));
        Assert.AreEqual("S22NoCarrier aligns to 1 in the CLR where Go aligns it to 2", GoReflect.ClrLayoutMismatch(typeof(S22NoCarrier)));
    }

    [TestMethod]
    public void TheGateNamesS22WithItsCarrierDeletedAgainstTheStamp()
    {
        Assert.AreEqual("S22Deleted aligns to 1 in the CLR where Go aligns it to 2", GoReflect.InlineLayoutMismatch(typeof(S22Deleted)));
        Assert.AreEqual("S22Deleted aligns to 1 in the CLR where Go aligns it to 2", GoReflect.ClrLayoutMismatch(typeof(S22Deleted)));
        StringAssert.Contains(GoReflect.InlineLayoutMismatch(typeof(HoldsS22Deleted)), "S22Deleted aligns to 1 in the CLR where Go aligns it to 2");
    }

    [TestMethod]
    public unsafe void AHeldS22WithItsCarrierDeletedFailsClosedOnANativeRead()
    {
        byte* memory = (byte*)NativeMemory.AllocZeroed(204);

        try
        {
            ж<HoldsS22Deleted> held = (ж<HoldsS22Deleted>)(uintptr)(nuint)memory;

            PanicException panic = Assert.ThrowsException<PanicException>(() => held.Value.B,
                "a native read of a struct holding an uncarried S22 answered instead of refusing");

            StringAssert.StartsWith(panic.Message, "runtime error: cannot read native memory as ");
            StringAssert.Contains(panic.Message, "S22Deleted aligns to 1 in the CLR where Go aligns it to 2");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    [TestMethod]
    public void TheGateFailsWhereGoOffsetsAreUnknown()
    {
        Assert.IsNull(GoReflect.GoFieldOffsets(typeof(NullOffsets)));
        Assert.AreEqual("NullOffsets: Go's field offsets are unknown", GoReflect.InlineLayoutMismatch(typeof(NullOffsets)));
        Assert.AreEqual("NullOffsets: Go's field offsets are unknown", GoReflect.ClrLayoutMismatch(typeof(NullOffsets)));
    }

    [TestMethod]
    public void TheGateNamesASizeAnOffsetAndAMissingTable()
    {
        Assert.AreEqual("WrongSize is 8 bytes in the CLR where Go's size is 4", GoReflect.InlineLayoutMismatch(typeof(WrongSize)));
        Assert.AreEqual("WrongOffset.A is at CLR offset 8 where Go lays it at 4", GoReflect.InlineLayoutMismatch(typeof(WrongOffset)));
        Assert.AreEqual("HoldsUnmeasured.P: Unmeasured carries no managed layout table", GoReflect.InlineLayoutMismatch(typeof(HoldsUnmeasured)));
    }

    [TestMethod]
    public void ReflectionOverAnInlineFieldPanicsNamingIt()
    {
        S22 value = default;
        value.Fa[3] = 7;
        GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(typeof(S22));

        PanicException read = Assert.ThrowsException<PanicException>(() => fields[1].Read(value));
        StringAssert.Contains(read.Message, "S22.Fa is a Go array held inline");

        PanicException alias = Assert.ThrowsException<PanicException>(() => GoReflect.FieldAliasBox(new object(), fields[2]));
        StringAssert.Contains(alias.Message, "S22.F__ccgo_pad2 is a Go array held inline");

        // A [0]T reads as its zero-length array, whatever the carrier's shared bytes hold.
        Assert.AreEqual(0, ((IArray)fields[0].Read(value)!).Length);
    }
}
