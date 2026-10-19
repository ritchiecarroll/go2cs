// NativeArrayOpenerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using reflect = go.reflect_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The native array openers (G's ruling, 2026-10-19: NARROWER): a foreign <c>*[N]*E</c>, and the window over a native
/// struct's <c>[N]X</c> field, are built only for an element some compilation spells, through the opener go2cs-gen
/// registers in that compilation's module initializer. A miss is a Go panic naming the element, never a wrong read.
/// </summary>
/// <remarks>
/// <para>
/// This assembly has no generator, so <see cref="NativeArrayOpenerRegistrations"/> registers what its tests spell, as
/// go2cs-gen would. The arms: reflect over foreign words reads them for a registered E and refuses an unregistered E
/// by name (the zero box read nil for both); unsafe.Slice over a foreign <c>**E</c> reads the words (COORD's ruling,
/// 2026-10-19: crypto/x509's windows chain reads, which refused at the native slice); a spelled door over an
/// unregistered E refuses by name; and the window over an <c>[N]X</c> field with no registered X refuses by name.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NativeArrayOpenerTests
{
    // A struct no registration names: reached through an array field, its window has no opener.
    public struct Unregistered
    {
        public ushort Tag;
    }

    // Go layout: Count uint16 at 0, Cells [2]Unregistered at 2.
    public struct Record
    {
        public ushort Count;
        public array<Unregistered> Cells;

        public Record() => Cells = new array<Unregistered>(2);

        public static ref array<Unregistered> ᏑCells(ref Record s) => ref s.Cells;
    }

    private static nint foreignWords(int first, int second)
    {
        nint block = (nint)NativeMemory.AllocZeroed(64);

        *(int*)(block + 32) = first;
        *(int*)(block + 40) = second;
        *(nint*)block = block + 32;
        *(nint*)(block + 8) = block + 40;

        return block;
    }

    private static void release(nint block)
    {
        try
        {
            GoForeignMemory.Release((nuint)block, 64);
        }
        finally
        {
            NativeMemory.Free((void*)block);
        }
    }

    [TestMethod]
    public void ReflectOverForeignWordsReadsThemForARegisteredElement()
    {
        nint block = foreignWords(3, 5);

        try
        {
            reflect.ΔType type = reflect.ArrayOf(2, reflect.PointerTo(reflect.TypeFor<int>()));
            reflect.ΔValue words = reflect.NewAt(type, (uintptr)(nuint)block).Elem();

            Assert.AreEqual(3L, words.Index(0).Elem().Int(), "reflect.NewAt(ArrayOf(2, PointerTo(int32)), p) reads word 0's int32");
            Assert.AreEqual(5L, words.Index(1).Elem().Int(), "and word 1's");
        }
        finally
        {
            release(block);
        }
    }

    [TestMethod]
    public void ReflectOverForeignWordsRefusesAnUnregisteredElementByName()
    {
        nint block = foreignWords(3, 5);

        try
        {
            reflect.ΔType type = reflect.ArrayOf(2, reflect.PointerTo(reflect.TypeFor<double>()));
            PanicException refusal = Assert.ThrowsException<PanicException>(() => reflect.NewAt(type, (uintptr)(nuint)block));

            StringAssert.StartsWith(refusal.Message, "go2cs: native pointer array: ", "a refusal Go never raises carries go2cs:");
            StringAssert.Contains(refusal.Message, "*[2]*float64", "the refusal names the array and its element");
            StringAssert.Contains(refusal.Message, "no reader for it was compiled");
        }
        finally
        {
            release(block);
        }
    }

    // crypto/x509's root_windows shape: unsafe.Slice(chainCtx.Chains, chainCtx.ChainCount), whose first argument is a
    // **E read out of memory the system allocated.
    [TestMethod]
    public void UnsafeSliceOverForeignPointerWordsReadsThem()
    {
        nint block = foreignWords(3, 5);

        try
        {
            slice<ж<int>> words = @unsafe.Slice((ж<ж<int>>)(uintptr)(nuint)block, 2);

            Assert.AreEqual(2, len(words), "unsafe.Slice(p, 2) has length 2");
            Assert.AreEqual((nuint)(block + 32), words[0].NativeAddress, "element 0 is the address word 0 holds");
            Assert.AreEqual(3, words[0].Value, "and reads the int32 there");
            Assert.AreEqual(5, words[1].Value, "element 1 reads word 1's");
        }
        finally
        {
            release(block);
        }
    }

    [TestMethod]
    public void ASpelledDoorOverAnUnregisteredElementRefusesByName()
    {
        nint block = foreignWords(3, 5);

        try
        {
            PanicException refusal = Assert.ThrowsException<PanicException>(() => NativeArrayPointer<ж<Unregistered>>((nuint)block, 2));

            StringAssert.StartsWith(refusal.Message, "go2cs: native pointer array: *[2]*");
            StringAssert.Contains(refusal.Message, "no reader for it was compiled");
        }
        finally
        {
            release(block);
        }
    }

    [TestMethod]
    public void TheWindowOverAnUnregisteredArrayFieldRefusesByName()
    {
        // os's createMountPoint shape (ByteViewArrayFieldTests): a struct viewed over bytes, then &record.Cells[0].
        slice<byte> blob = new(16);
        ж<Record> record = Ꮡ(blob, 0).Reinterpret<byte, Record>();

        PanicException refusal = Assert.ThrowsException<PanicException>(() => record.at<Unregistered>(Record.ᏑCells, 0));

        StringAssert.StartsWith(refusal.Message, "go2cs: native array field: a [2]");
        StringAssert.Contains(refusal.Message, "no window for it was compiled");
    }
}

/// <summary>What go2cs-gen would register for the GolibTests that spell a native array door.</summary>
/// <remarks>
/// The pointer arrays NativePointerArrayTests and these arms read (<c>*[N]*int32</c>, <c>*[N]*uint16</c>), and the
/// array fields the byte-view tests window (<c>[N]uint16</c>, <c>[N]byte</c>, <c>[N]int8</c>). A referenced converted
/// package registers some of these too, but only once its module is loaded, so a test must not lean on that order.
/// </remarks>
internal static class NativeArrayOpenerRegistrations
{
    [ModuleInitializer]
    internal static void Register()
    {
        NativeArrayOpeners.RegisterPointerArray<int>();
        NativeArrayOpeners.RegisterPointerArray<ushort>();
        NativeArrayOpeners.RegisterArray<ushort>();
        NativeArrayOpeners.RegisterArray<byte>();
        NativeArrayOpeners.RegisterArray<sbyte>();
    }
}
