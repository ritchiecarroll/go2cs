using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// A func stored through an <c>unsafe.Pointer</c> whose target the store does not name: Go's
/// <c>*(*F)(e) = fn</c>, emitted as <c>unsafe.Pointer.StoreFunc(e, fn)</c>
/// (docs/phase4/DESIGN-interior-pointer-root.md 4.8 B1 and 10.4). What <c>e</c> names decides what is stored: a
/// word takes the func's word, a func slot takes the func, and nothing writes a word over a reference.
/// </summary>
[TestClass]
public unsafe class FuncStoreThroughPointerTests
{
    private delegate long Combine(long a, long b);

    private static long Add(long a, long b) => a + b;

    private static long Mul(long a, long b) => a * b;

    // struct{ name string; f func(int64, int64) int64; w uintptr; n int64 }: holds references, so its pointer
    // answers an order token. Go offsets: name 0, f 16, w 24, n 32.
    private struct FuncTable
    {
        internal @string name;
        internal Func<long, long, long> f;
        internal uintptr w;
        internal long n;
    }

    private static nuint WordOf(Delegate fn) => GoFuncCookie.Of(fn);

    // var h func(int64, int64) int64; p := unsafe.Pointer(&h); *(*func(int64, int64) int64)(p) = mul
    [TestMethod]
    public void AFuncStoredThroughAPointerToAFuncVariableIsTheFuncItself()
    {
        ref Func<long, long, long> h = ref heap<Func<long, long, long>>(out ж<Func<long, long, long>> Ꮡh);

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡh), (Func<long, long, long>)Mul);
        Assert.IsNotNull(h, "the func variable a pointer names takes the func");
        Assert.AreEqual(30L, h(5, 6));

        // The same through a pointer VALUE carried across (FromBox), and at offset 0 of a moved pointer.
        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromBox(Ꮡh), (Func<long, long, long>)Add);
        Assert.AreEqual(11L, h(5, 6));

        @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), 0), (Func<long, long, long>)Mul);
        Assert.AreEqual(30L, h(5, 6));

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡh), null);
        Assert.IsNull(h, "a nil func stored through the pointer is nil in the variable");
    }

    // type binop func(int64, int64) int64; var nb binop; *(*func(int64, int64) int64)(unsafe.Pointer(&nb)) = add
    [TestMethod]
    public void AFuncVariableOfANamedTypeTakesAFuncStoredAsAnotherTypeWithItsSignature()
    {
        ref Combine nb = ref heap<Combine>(out ж<Combine> Ꮡnb);

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡnb), (Func<long, long, long>)Add);
        Assert.AreEqual(4L, nb(2, 2), "the slot's own delegate type holds the func");

        // A func whose signature the slot cannot hold is refused by name, and the slot is not written.
        PanicException refused = Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡnb), (Action)(() => { })));
        StringAssert.Contains(refused.Message, "cannot be read as");
        Assert.AreEqual(4L, nb(2, 2));
    }

    // var words [3]uintptr; *(*func(...))(unsafe.Add(unsafe.Pointer(&words), 8)) = add
    [TestMethod]
    public void AFuncStoredIntoAWordOfGoMemoryIsItsWord()
    {
        ref array<uintptr> words = ref heap(new array<uintptr>(3), out ж<array<uintptr>> Ꮡwords);

        @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡwords), 8), (Func<long, long, long>)Add);

        Assert.AreEqual(WordOf((Func<long, long, long>)Add), (nuint)words[1], "a word slot takes the func's word");
        Assert.AreEqual((uintptr)0, words[0]);
        Assert.AreEqual((uintptr)0, words[2]);
        Assert.AreEqual(7L, GoFuncCookie.Func<Func<long, long, long>>((nuint)words[1])(3, 4), "and the word reads back as the func");

        // var slot uintptr; p := unsafe.Pointer(&slot); *(*func(...))(p) = mul
        ref uintptr slot = ref heap(new uintptr(), out ж<uintptr> Ꮡslot);

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡslot), (Func<long, long, long>)Mul);
        Assert.AreEqual(WordOf((Func<long, long, long>)Mul), (nuint)slot);

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(Ꮡslot), null);
        Assert.AreEqual((uintptr)0, slot, "a nil func's word is 0");
    }

    // ccgo's method table in C memory: *(*func(...))(unsafe.Pointer(table + 8)) = add
    [TestMethod]
    public void AFuncStoredIntoCMemoryIsItsWord()
    {
        IntPtr buffer = Marshal.AllocHGlobal(24);

        try
        {
            for (int i = 0; i < 24; i++)
                Marshal.WriteByte(buffer, i, 0);

            nuint address = (nuint)(nint)buffer;

            @unsafe.Pointer.StoreFunc(@unsafe.Add((@unsafe.Pointer)(uintptr)address, 8), (Func<long, long, long>)Add);

            Assert.AreEqual((long)WordOf((Func<long, long, long>)Add), Marshal.ReadInt64(buffer, 8), "C memory takes the func's word");
            Assert.AreEqual(0L, Marshal.ReadInt64(buffer, 0));
            Assert.AreEqual(0L, Marshal.ReadInt64(buffer, 16));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // var t table; *(*func(...))(unsafe.Add(unsafe.Pointer(&t), unsafe.Offsetof(t.f))) = add, and the same at t.w.
    [TestMethod]
    public void InsideAValueThatHoldsReferencesAFuncFieldTakesTheFuncAndAWordFieldTheWord()
    {
        ref FuncTable t = ref heap(new FuncTable { name = "t", n = 9 }, out ж<FuncTable> Ꮡt);

        @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡt), 16), (Func<long, long, long>)Add);
        Assert.IsNotNull(t.f, "a func field takes the func");
        Assert.AreEqual(3L, t.f(1, 2));

        @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡt), 24), (Func<long, long, long>)Mul);
        Assert.AreEqual(WordOf((Func<long, long, long>)Mul), (nuint)t.w, "a uintptr field takes the word");

        Assert.AreEqual((@string)"t", t.name, "and nothing else in the value is written");
        Assert.AreEqual(9L, t.n);
        Assert.AreEqual(3L, t.f(1, 2));
    }

    // A func stored where Go's layout has neither a func nor a word is refused by name. No arm writes 8 bytes over
    // a reference: offset 0 is the string, 32 is an int64, 20 is inside the func field.
    [TestMethod]
    public void AFuncStoredOverAnythingElseInsideAValueThatHoldsReferencesIsRefusedByName()
    {
        ref FuncTable t = ref heap(new FuncTable { name = "t", n = 9 }, out ж<FuncTable> Ꮡt);
        ж<FuncTable> box = Ꮡt;

        foreach (uintptr offset in new uintptr[] { 0, 32, 20 })
        {
            PanicException refused = Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(box), offset), (Func<long, long, long>)Add), $"offset {offset}");
            StringAssert.Contains(refused.Message, "names neither a func nor a word", $"offset {offset}");
        }

        Assert.AreEqual((@string)"t", t.name);
        Assert.AreEqual(9L, t.n);
        Assert.IsNull(t.f);
    }

    // A word does not keep its func alive, as a uintptr does not in Go. A closure stored only as a word, then
    // dropped, leaves a word that names no live func: a call through it is refused by name.
    [TestMethod]
    public void AClosuresWordAfterItsClosureIsDroppedIsRefusedByNameAtTheCall()
    {
        ref uintptr slot = ref heap(new uintptr(), out ж<uintptr> Ꮡslot);

        StoreAClosureNobodyHolds(Ꮡslot);
        Assert.AreNotEqual((uintptr)0, slot, "the closure's word was stored");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (GoFuncCookie.Resolve((nuint)slot) is not null)
            Assert.Inconclusive("the JIT kept the closure alive past its frame; the refusal arm cannot be reached on this run");

        nuint word = (nuint)slot;
        PanicException refused = Assert.ThrowsException<PanicException>(() => GoFuncCookie.Func<Func<long, long, long>>(word));
        StringAssert.Contains(refused.Message, "names no live func");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void StoreAClosureNobodyHolds(ж<uintptr> slot)
    {
        long captured = 10;
        Func<long, long, long> closure = (a, b) => a + b + captured;

        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(slot), closure);
    }

    // A static function's word is good for the life of the process: no delegate over it need be held.
    [TestMethod]
    public void AStaticFunctionsWordStoredThroughAPointerOutlivesItsDelegate()
    {
        ref uintptr slot = ref heap(new uintptr(), out ж<uintptr> Ꮡslot);

        StoreAStaticFunctionThroughAFreshDelegate(Ꮡslot);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.AreEqual(12L, GoFuncCookie.Func<Func<long, long, long>>((nuint)slot)(3, 4));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void StoreAStaticFunctionThroughAFreshDelegate(ж<uintptr> slot)
    {
        @unsafe.Pointer.StoreFunc(@unsafe.Pointer.FromPinnedBox(slot), new Func<long, long, long>(Mul));
    }

    [TestMethod]
    public void AFuncStoredThroughANilPointerIsANilDereference()
    {
        Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.StoreFunc(null, (Func<long, long, long>)Add));
        Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.StoreFunc(@unsafe.Pointer.Nil, (Func<long, long, long>)Add));
    }
}
