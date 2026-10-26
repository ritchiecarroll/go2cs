// FieldAccessTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// <see cref="ж{T}.field{TF}"/>, the member the generated field properties read through: a field of a struct
/// reached through a pointer reinterpreted from <c>unsafe.Pointer</c>, os's createMountPoint shape,
/// <c>buf.SubstituteNameOffset = x</c>.
/// </summary>
/// <remarks>
/// The struct holds an <see cref="array{T}"/>, a managed reference, so the reinterpret mints a native box over the
/// pinned bytes and the struct's CLR layout is not Go's: the CLR places the reference first. <c>field</c> answers
/// a field at its GO offset there, refuses by name a field whose CLR form is not its Go form, and answers the
/// struct's own slot in managed memory. It allocates nothing either way.
/// </remarks>
[TestClass]
public class FieldAccessTests
{
    public struct Inner
    {
        public ushort X;
        public ushort Y;
    }

    // Go layout: Kind uint32 at 0; Length uint16 at 4; In {X, Y uint16} at 6; Path [2]uint16 at 10; size 16.
    public struct Buffer
    {
        public uint Kind;
        public ushort Length;
        public Inner In;
        public array<ushort> Path;

        public Buffer() => Path = new array<ushort>(2);

        // Converter-named, so the field's Go name is read off the accessor.
        public static ref uint ᏑKind(ref Buffer s) => ref s.Kind;
        public static ref ushort ᏑLength(ref Buffer s) => ref s.Length;
        public static ref Inner ᏑIn(ref Buffer s) => ref s.In;
        public static ref array<ushort> ᏑPath(ref Buffer s) => ref s.Path;
    }

    [TestMethod]
    public void AFieldOfAByteViewIsWrittenAtItsGoOffset()
    {
        slice<byte> blob = new(16);
        ж<Buffer> buf = Ꮡ(blob, 0).Reinterpret<byte, Buffer>();

        buf.field(Buffer.ᏑKind) = 0x01020304;
        buf.field(Buffer.ᏑLength) = 0x0506;
        buf.field(Buffer.ᏑLength) += 1;
        buf.field(Buffer.ᏑIn).Y = 0x0809;

        byte[] want = [4, 3, 2, 1, 7, 5, 0, 0, 9, 8, 0, 0, 0, 0, 0, 0];

        for (int i = 0; i < want.Length; i++)
            Assert.AreEqual(want[i], blob[i], $"byte {i}");

        Assert.AreEqual(0x01020304u, buf.field(Buffer.ᏑKind));
        Assert.AreEqual((ushort)0x0507, buf.field(Buffer.ᏑLength));
        Assert.AreEqual((ushort)0x0809, buf.field(Buffer.ᏑIn).Y);
    }

    [TestMethod]
    public void AFieldOfAByteViewReadsBytesWrittenThroughTheBuffer()
    {
        slice<byte> blob = new(16);
        ж<Buffer> buf = Ꮡ(blob, 0).Reinterpret<byte, Buffer>();

        blob[4] = 0x34;
        blob[5] = 0x12;
        blob[6] = 0x78;
        blob[7] = 0x56;

        Assert.AreEqual((ushort)0x1234, buf.field(Buffer.ᏑLength));
        Assert.AreEqual((ushort)0x5678, buf.field(Buffer.ᏑIn).X);
    }

    [TestMethod]
    public void AReferenceFieldOverNativeMemoryRefusesByName()
    {
        slice<byte> blob = new(16);
        ж<Buffer> buf = Ꮡ(blob, 0).Reinterpret<byte, Buffer>();

        PanicException panic = Assert.ThrowsException<PanicException>(() => buf.field(Buffer.ᏑPath));
        string text = panic.Message;

        StringAssert.StartsWith(text, "runtime error: cannot read native memory as ");
        StringAssert.EndsWith(text, ": its CLR layout is not its Go layout (field Path is a managed reference where Go stores the value inline)");
    }

    [TestMethod]
    public void AManagedRootAnswersTheStructsOwnSlot()
    {
        ж<Buffer> box = Ꮡ(new Buffer());

        box.field(Buffer.ᏑLength) = 9;
        box.field(Buffer.ᏑIn).X = 3;
        box.field(Buffer.ᏑPath)[1] = 5;

        Assert.AreEqual((ushort)9, box.Value.Length);
        Assert.AreEqual((ushort)3, box.Value.In.X);
        Assert.AreEqual((ushort)5, box.Value.Path[1], "a managed root reaches a reference field too");
    }

    // The reading COORD asked for: bytes allocated per call, native and managed, after a warm-up that memoizes the
    // field's Go offset. A field read through the property is a call to field, so this is the property's cost.
    [TestMethod]
    public void FieldAllocatesNothing()
    {
        const int Calls = 10_000;

        slice<byte> blob = new(16);
        ж<Buffer> native = Ꮡ(blob, 0).Reinterpret<byte, Buffer>();
        ж<Buffer> managed = Ꮡ(new Buffer());

        native.field(Buffer.ᏑLength) = 0;
        managed.field(Buffer.ᏑLength) = 0;

        long allocated = BytesOverFieldCalls(native, managed, Calls);

        Assert.AreEqual(0L, allocated, $"{allocated} bytes over {Calls} calls on each root");
        Assert.AreEqual((ushort)Calls, native.field(Buffer.ᏑLength));
        Assert.AreEqual((ushort)Calls, managed.field(Buffer.ᏑLength));
    }

    // The measuring loop is compiled fully optimized from its first call. Under tiered compilation it starts at tier 0
    // and moves to an on-stack-replacement method partway through the window, and on Windows (.NET 10.0.11) that
    // transition allocated 24 B on the test thread: the JIT's bytes, not field's (R, 2026-10-21; each of
    // TC_OnStackReplacement=0, TieredCompilation=0, TieredPGO=0 and TC_QuickJitForLoops=0 removed it).
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
    private static long BytesOverFieldCalls(ж<Buffer> native, ж<Buffer> managed, int calls)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < calls; i++)
        {
            native.field(Buffer.ᏑLength)++;
            managed.field(Buffer.ᏑLength)++;
        }

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
