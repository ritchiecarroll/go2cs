// NativeArrayReflectStoreTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using reflect = go.reflect_package;

namespace GolibTests;

/// <summary>
/// reflect over a pointer to an array in native memory (COORD's ruling, 2026-10-19, follow-up (b)): Go's
/// <c>v.Elem().Set(arr)</c> is <c>*p = arr</c>, N element stores, and <c>v.Elem().Interface()</c> is <c>*p</c>, a copy
/// of every element. Both went through the box's slot, which a native array does not have, so the store threw a
/// message about READING and the read refused. Each now goes element by element through <c>p[i]</c>'s door:
/// into the block for a value array, and, for a foreign pointer array, refused at the next access exactly as
/// <c>p[i] = x</c> is.
/// </summary>
[TestClass]
public unsafe class NativeArrayReflectStoreTests
{
    private static nint block(nuint bytes) => (nint)NativeMemory.AllocZeroed(bytes);

    private static array<ushort> values(params ushort[] elements)
    {
        array<ushort> result = new(elements.Length);

        for (int i = 0; i < elements.Length; i++)
            result[i] = elements[i];

        return result;
    }

    [TestMethod]
    public void ReflectSetOnANativeValueArrayStoresEveryElement()
    {
        nint memory = block(8);

        try
        {
            reflect.ΔValue elem = reflect.ValueOf(NativeArrayPointer<ushort>((nuint)memory, 4)).Elem();

            elem.Set(reflect.ValueOf(values(1, 2, 3, 4)));

            for (int i = 0; i < 4; i++)
                Assert.AreEqual((ushort)(i + 1), *(ushort*)(memory + 2 * i), $"*p = arr stores element {i} into the block");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ReflectReadOfANativeValueArrayCopiesIt()
    {
        nint memory = block(8);

        try
        {
            for (int i = 0; i < 4; i++)
                *(ushort*)(memory + 2 * i) = (ushort)(10 + i);

            array<ushort> copy = (array<ushort>)reflect.ValueOf(NativeArrayPointer<ushort>((nuint)memory, 4)).Elem().Interface()!;

            *(ushort*)memory = 99;

            for (int i = 0; i < 4; i++)
                Assert.AreEqual((ushort)(10 + i), copy[i], $"*p reads element {i} by value, as Go copies the array");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ReflectIndexSetOnANativeValueArrayLandsInTheBlock()
    {
        nint memory = block(8);

        try
        {
            reflect.ΔValue elem = reflect.ValueOf(NativeArrayPointer<ushort>((nuint)memory, 4)).Elem();

            elem.Index(2).Set(reflect.ValueOf((ushort)9));

            Assert.AreEqual((ushort)9, *(ushort*)(memory + 4), "p[2] = 9 through reflect lands in the block");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // Two foreign pointer words at 0 and 8, naming the ints at 32 and 40.
    private static nint foreignWords()
    {
        nint memory = block(64);

        *(int*)(memory + 32) = 3;
        *(int*)(memory + 40) = 5;
        *(nint*)memory = memory + 32;
        *(nint*)(memory + 8) = memory + 40;

        return memory;
    }

    private static void release(nint memory)
    {
        try
        {
            GoForeignMemory.Release((nuint)memory, 64);
        }
        catch (PanicException)
        {
            // Refused already, at the access the test asserted.
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ReflectSetOnAForeignPointerArrayIsRefusedAtTheNextAccess()
    {
        nint memory = foreignWords();

        try
        {
            ж<array<ж<int>>> words = NativeArrayPointer<ж<int>>((nuint)memory, 2);
            array<ж<int>> source = new(2);
            source[0] = Ꮡ(7);
            source[1] = words.ElementRef(1);

            reflect.ValueOf(words).Elem().Set(reflect.ValueOf(source));

            PanicException refusal = Assert.ThrowsException<PanicException>(() => words.ElementRef(0));
            StringAssert.StartsWith(refusal.Message, "go2cs: native pointer array: the word at 0x");
            StringAssert.Contains(refusal.Message, "was stored into through");
            Assert.AreEqual((nint)(memory + 32), *(nint*)memory, "the refused store never reached the word");
            Assert.AreEqual(5, words.ElementRef(1).Value, "an element stored as the word it already held is no change");
        }
        finally
        {
            release(memory);
        }
    }

    [TestMethod]
    public void ReflectIndexSetOnAForeignPointerArrayIsRefusedAsAnElementStoreIs()
    {
        nint memory = foreignWords();

        try
        {
            ж<array<ж<int>>> words = NativeArrayPointer<ж<int>>((nuint)memory, 2);

            reflect.ValueOf(words).Elem().Index(0).Set(reflect.ValueOf(Ꮡ(7)));

            PanicException refusal = Assert.ThrowsException<PanicException>(() => words.ElementRef(0));
            StringAssert.StartsWith(refusal.Message, "go2cs: native pointer array: the word at 0x");
            StringAssert.Contains(refusal.Message, "was stored into through");
            Assert.AreEqual((nint)(memory + 32), *(nint*)memory, "the refused store never reached the word");
        }
        finally
        {
            release(memory);
        }
    }
}
