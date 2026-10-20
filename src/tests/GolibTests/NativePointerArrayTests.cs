// NativePointerArrayTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A Go <c>*[N]*E</c> over native memory, keyed by OWNERSHIP (G's ruling, 2026-10-19): memory the Go runtime's
/// own sysAllocOS / sysReserveOS returned holds Go pointers, kept in a managed shadow; any other native memory
/// holds foreign addresses, read from its words.
/// </summary>
/// <remarks>
/// <para>
/// The arms G named, red-first: Go's own spelling over foreign pointer words reads them; a runtime-owned block
/// keeps the Go pointer stored through it across a fresh pointer to the same block (the arena index's shape,
/// <c>l2[i] = r</c>); a store into foreign memory, through an element or a slice, is refused by name at the next
/// access or at the producer's release; a foreign array of other reference-bearing values refuses; a release
/// splits its range; and a range sysFreeOS released reads as foreign again.
/// </para>
/// <para>
/// Every flavour: the runtime's sysAllocOS / sysFreeOS seams (GoSysAllocOS / GoSysFreeOS) exist on windows,
/// linux and darwin. A box over foreign memory is minted without reading it, so the release arm never touches
/// the pages it freed.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NativePointerArrayTests
{
    private const nuint BlockBytes = 64 * 1024;

    private static void writeUtf16(nint target, string text)
    {
        ushort* chars = (ushort*)target;

        for (int i = 0; i < text.Length; i++)
            chars[i] = text[i];

        chars[text.Length] = 0;
    }

    // The NUL-terminated UTF-16 string a native *uint16 points at.
    private static string readUtf16(ж<uint16> pointer)
    {
        Assert.IsNotNull(pointer, "a foreign pointer word read as nil");
        Assert.AreNotEqual((nuint)0, pointer.NativeAddress, "a foreign pointer word did not read as a native pointer");

        return Marshal.PtrToStringUni((nint)pointer.NativeAddress)!;
    }

    // DNS_TXT_DATAW's shape: pStringArray holds two pointers. Go reads it as
    // `(*[1 << 10]*uint16)(unsafe.Pointer(&d.StringArray[0]))[:d.StringCount:d.StringCount]`, where &d.StringArray[0]
    // is element 0 of the declared `[1]*uint16` field over native memory.
    [TestMethod]
    public void GoSpellingOverForeignPointerWordsReadsEachWord()
    {
        nint block = (nint)NativeMemory.AllocZeroed(256);

        try
        {
            writeUtf16(block + 128, "first");
            writeUtf16(block + 192, "second");
            *(nint*)(block + 0) = block + 128;
            *(nint*)(block + 8) = block + 192;

            ж<array<ж<uint16>>> declared = builtin.NativeArrayPointer<ж<uint16>>((nuint)block, 1);
            slice<ж<uint16>> strings = array<ж<uint16>>.AliasPointer(declared.at<ж<uint16>>(0), 1 << 10).slice(0, 2, 2);

            Assert.AreEqual((nint)2, strings.Length);
            Assert.AreEqual("first", readUtf16(strings[0]));
            Assert.AreEqual("second", readUtf16(strings[1]));

            // The element door reads a word in place too.
            Assert.AreEqual("first", readUtf16(declared.ElementRef(0)));
        }
        finally
        {
            release(block, 256);
        }
    }

    // malloc.go's arena index: `l2 := (*[1 << arenaL2Bits]*heapArena)(sysAllocOS(...))`, then `l2[ri.l2()] = r` with r a
    // Go pointer, read back later through a fresh view of the same block.
    [TestMethod]
    public void ARuntimeOwnedBlockKeepsTheGoPointerStoredThroughIt()
    {
        nuint block = runtime_package.GoSysAllocOS(BlockBytes);
        Assert.AreNotEqual((nuint)0, block, "sysAllocOS returned nil");

        try
        {
            nint words = (nint)(BlockBytes / (nuint)sizeof(nuint));
            ж<int> stored = builtin.Ꮡ(42);

            ж<array<ж<int>>> first = builtin.NativeArrayPointer<ж<int>>(block, words);
            first.Value[3] = stored;

            ж<array<ж<int>>> fresh = builtin.NativeArrayPointer<ж<int>>(block, words);

            Assert.AreSame(stored, fresh.Value[3], "a fresh pointer to the runtime's block lost the Go pointer stored through the first");
            Assert.IsNull(fresh.Value[4], "an element nothing stored reads nil, as fresh OS memory does");
        }
        finally
        {
            runtime_package.GoSysFreeOS(block, BlockBytes);
        }
    }

    // A store into foreign memory is refused by name (G's ruling, 2026-10-19): the memory is its producer's, immutable
    // until released, and Go forbids a Go pointer there. A C# ref cannot see the store, so it is refused at the next
    // access through a foreign array over the word, and it never reaches the word.
    [TestMethod]
    public void AStoreThroughAnElementIsRefusedAtTheNextAccess()
    {
        nint block = (nint)NativeMemory.AllocZeroed(64);

        try
        {
            ж<array<ж<int>>> words = builtin.NativeArrayPointer<ж<int>>((nuint)block, 4);

            // A native pointer is refused as a managed one is: the producer owns the memory.
            words.ElementRef(1) = (ж<int>)(uintptr)(nuint)(block + 32);
            StringAssert.Contains(goRefusal(() => words.ElementRef(1)).Message, "stored into through an element");
            Assert.AreEqual((nint)0, *(nint*)(block + 8), "the refused store reached the word");

            words.ElementRef(0) = builtin.Ꮡ(7);
            StringAssert.Contains(goRefusal(() => words.ElementRef(0)).Message, "Go forbids a Go pointer there");
            Assert.AreEqual((nint)0, *(nint*)block, "the refused store reached the word");

            // The whole array cannot be served without reading every word.
            StringAssert.Contains(goRefusal(() => words.Value).Message, "did not allocate");
        }
        finally
        {
            release(block, 64);
        }
    }

    // A slice of the array is a managed copy of the words, so a store into it is refused at the next access through
    // the array over the same word, and at the latest when the producer releases the memory.
    [TestMethod]
    public void AStoreIntoASliceIsRefusedAtTheNextAccessThroughTheArray()
    {
        nint block = (nint)NativeMemory.AllocZeroed(64);

        try
        {
            ж<array<ж<int>>> words = builtin.NativeArrayPointer<ж<int>>((nuint)block, 4);
            slice<ж<int>> view = words.slice(1, 3, 3);

            view[1] = builtin.Ꮡ(9);

            StringAssert.Contains(goRefusal(() => words.ElementRef(2)).Message, "through a slice of the array");
            Assert.AreEqual((nint)0, *(nint*)(block + 16), "the refused store reached the word");

            // A word the slice does not cover still reads.
            Assert.IsNull(words.ElementRef(0));
        }
        finally
        {
            release(block, 64);
        }
    }

    [TestMethod]
    public void AStoreIntoASliceIsRefusedWhenTheProducerReleasesTheMemory()
    {
        nint block = (nint)NativeMemory.AllocZeroed(64);

        try
        {
            slice<ж<int>> view = builtin.NativeArrayPointer<ж<int>>((nuint)block, 4).slice(0, 2, 2);
            view[0] = builtin.Ꮡ(3);

            PanicException refused = Assert.ThrowsException<PanicException>(() => GoForeignMemory.Release((nuint)block, 64));
            StringAssert.StartsWith(refused.Message, "go2cs: ");
            StringAssert.Contains(refused.Message, "through a slice of the array");
            Assert.AreEqual((nint)0, *(nint*)block, "the refused store reached the word");

            // Released means forgotten: the next release of the same range has nothing to refuse.
            GoForeignMemory.Release((nuint)block, 64);
        }
        finally
        {
            NativeMemory.Free((void*)block);
        }
    }

    // A non-pointer element that carries managed references cannot be read from foreign bytes, and the zeroed store it
    // used to get answered zero for memory that holds data. It refuses by name.
    [TestMethod]
    public void AForeignArrayOfReferenceBearingValuesRefusesByName()
    {
        nint block = (nint)NativeMemory.AllocZeroed(64);

        try
        {
            ж<array<@string>> strings = builtin.NativeArrayPointer<@string>((nuint)block, 2);

            Assert.AreEqual((nuint)block, strings.NativeAddress, "the address stays the block's");
            StringAssert.Contains(goRefusal(() => strings.Value).Message, "did not allocate");
        }
        finally
        {
            NativeMemory.Free((void*)block);
        }
    }

    // Go frees subranges, so a release splits the range it lands in, and the rest stays the runtime's.
    [TestMethod]
    public void AReleaseSplitsTheRangeItLandsIn()
    {
        const nuint page = 4096;
        nuint @base = unchecked((nuint)0xffff_f000_0000_0000); // an address no OS hands a user mapping

        GoOsMemory.Register(@base, 3 * page);

        try
        {
            GoOsMemory.Unregister(@base + page, page);

            Assert.IsTrue(GoOsMemory.Owns(@base), "the range below the release stays registered");
            Assert.IsFalse(GoOsMemory.Owns(@base + page), "the released page reads as foreign");
            Assert.IsTrue(GoOsMemory.Owns(@base + 2 * page), "the range above the release stays registered");
        }
        finally
        {
            GoOsMemory.Unregister(@base, 3 * page);
        }

        Assert.IsFalse(GoOsMemory.Owns(@base + 2 * page));
    }

    // A refusal Go never raises is go2cs's, and says so (the C1 ruling: every go2cs refusal carries go2cs:).
    private static PanicException goRefusal(Func<object?> action)
    {
        PanicException refusal = Assert.ThrowsException<PanicException>(action);
        StringAssert.StartsWith(refusal.Message, "go2cs: ");
        return refusal;
    }

    // The tests' own producer release: what a seam does before it frees.
    private static void release(nint block, nuint length)
    {
        try
        {
            GoForeignMemory.Release((nuint)block, length);
        }
        catch (PanicException)
        {
            // Refused already, at the access the test asserted.
        }
        finally
        {
            NativeMemory.Free((void*)block);
        }
    }

    // sysFreeOS releases the range: the same address is foreign afterward, so a pointer array over it is the foreign
    // kind, whose whole-array read refuses (no word is read: the pages are gone).
    [TestMethod]
    public void ARangeSysFreeOSReleasedReadsAsForeign()
    {
        nuint block = runtime_package.GoSysAllocOS(BlockBytes);
        Assert.AreNotEqual((nuint)0, block, "sysAllocOS returned nil");

        bool freed = false;

        try
        {
            ж<array<ж<int>>> owned = builtin.NativeArrayPointer<ж<int>>(block, 4);
            Assert.IsNull(owned.Value[0], "the runtime's own block reads through its shadow");

            runtime_package.GoSysFreeOS(block, BlockBytes);
            freed = true;

            ж<array<ж<int>>> released = builtin.NativeArrayPointer<ж<int>>(block, 4);
            PanicException foreign = Assert.ThrowsException<PanicException>(() => released.Value);
            StringAssert.StartsWith(foreign.Message, "go2cs: ");
            StringAssert.Contains(foreign.Message, "did not allocate");
        }
        finally
        {
            if (!freed)
                runtime_package.GoSysFreeOS(block, BlockBytes);
        }
    }
}
