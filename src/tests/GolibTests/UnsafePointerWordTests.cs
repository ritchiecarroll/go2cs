using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using atomic = go.sync.atomic_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// An <c>unsafe.Pointer</c> stored into Go memory that holds a WORD (docs/phase4/DESIGN-cgo-phase3-marshaling.md, 18.3 to
/// 18.7, seat S-B1): the word is the referent's number, taken through the pinning projection, and a per-slot entry keeps
/// the referent alive and pinned while the word is published.
/// </summary>
/// <remarks>
/// <para>
/// The arms drive the shapes the converter emits for <c>*p</c> and <c>*p = v</c> with <c>p</c> a <c>*unsafe.Pointer</c>
/// (<see cref="Load"/>, <see cref="Store"/>), and the hand-owned atomic doors. Before the cure the converter emitted
/// <c>p.Value</c>, which the reference floor's exemption let write a CLR REFERENCE into the word: the collector does not
/// trace the word, so the referent could move or die under a pointer still in use (the .5 BOARD row of 2026-11-16).
/// </para>
/// <para>
/// SAFETY ON THE RED TREE. Every arm asserts first that the stored word is the referent's number (its pinned address, or
/// its order token). Before the cure the word is a CLR reference, so each arm fails at that assert, before any collection
/// and before any read that could follow a dangling reference inside the test host.
/// </para>
/// </remarks>
[TestClass]
public unsafe class UnsafePointerWordTests
{
    // ---- the emitted shapes (the converter's W1/W2 door: before the cure, `p.Value`) ----

    private static @unsafe.Pointer Load(ж<@unsafe.Pointer> p) => @unsafe.Pointer.LoadWord(p);

    private static void Store(ж<@unsafe.Pointer> p, @unsafe.Pointer v) => @unsafe.Pointer.StoreWord(p, v);

    // ---- fixtures ----

    private struct Pair
    {
        internal long a;
        internal long b;
    }

    private struct Holder
    {
        internal string s;
        internal object? root;
    }

    private struct Named
    {
        internal string name;
        internal nint scalar;

        internal static ref nint Ꮡscalar(ref Named instance) => ref instance.scalar;
    }

    // A word of Go memory and the *unsafe.Pointer over it, minted as the converter mints `(*unsafe.Pointer)(unsafe.Pointer(&w))`.
    private static (ж<uintptr> word, ж<@unsafe.Pointer> slot) WordSlot()
    {
        ж<uintptr> word = new StandardBox<uintptr>(0);
        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromBox(word));

        Assert.IsTrue(slot.IsNative, "the premise: the reinterpret is a native box over the Go word");
        return (word, slot);
    }

    // The stored word names its referent: an address the referent's box is pinned at, or the referent's order token.
    private static void AssertWordNames(nuint word, object box, string what)
    {
        Assert.AreNotEqual((nuint)0, word, $"{what}: the word is 0");

        if (ManagedPointerTokens.IsTaggedToken(word))
        {
            Assert.AreEqual(((INilPointer)box).PointerOrderToken, word, $"{what}: the word is a token, but not the referent's");
            return;
        }

        Assert.IsTrue(((INilPointer)box).IsPinnedAt(word), $"{what}: the word 0x{word:X} is not an address the referent is pinned at (a CLR reference, or a transient address)");
    }

    private static nuint WordOf(ж<uintptr> word) => word.Value.Value;

    // One round of the cycle procedure (design 18.7 finding 3).
    private static void CollectRound()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }

    private const int Rounds = 10;

    // Runs the stated cap, every round, and answers the round in which each reference first read dead (0: alive after the cap).
    private static int[] RoundsToDeath(params WeakReference[] weak)
    {
        int[] died = new int[weak.Length];

        for (int round = 1; round <= Rounds; round++)
        {
            CollectRound();

            for (int i = 0; i < weak.Length; i++)
            {
                if (died[i] == 0 && !weak[i].IsAlive)
                    died[i] = round;
            }
        }

        return died;
    }

    // ---- B-12: the number a store writes is the PINNING projection's ----

    [TestMethod]
    public void B12_AValueTypeReferentIsStoredAsItsPinnedAddressAndSurvivesCompaction()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Pair> x = new StandardBox<Pair>(new Pair { a = 41, b = 42 });

        Store(slot, @unsafe.Pointer.FromBox(x));
        AssertWordNames(WordOf(word), x, "a value-type referent");

        WeakReference xWeak = StoreAndForget(slot);
        GC.KeepAlive(x);
        x = null!;

        for (int round = 0; round < 3; round++)
            CollectRound();

        @unsafe.Pointer loaded = Load(slot);
        Assert.IsInstanceOfType(loaded.Referent, typeof(ж<Pair>), "the load answers the referent's box");
        Assert.IsTrue(xWeak.IsAlive, "the published word keeps its referent alive");
        Assert.AreEqual(141L, @unsafe.Pointer.To<Pair>(loaded).Value.a, "and the referent's bytes are intact");

        GC.KeepAlive(word);
    }

    // Stores a fresh value-type referent whose only reference is the slot's, and answers a weak reference to it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference StoreAndForget(ж<@unsafe.Pointer> slot)
    {
        ж<Pair> fresh = new StandardBox<Pair>(new Pair { a = 141, b = 142 });
        Store(slot, @unsafe.Pointer.FromBox(fresh));
        return new WeakReference(fresh);
    }

    [TestMethod]
    public void B12_CompareAndSwapComparesTheProjectionNotTheCarriedNumber()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Pair> x = new StandardBox<Pair>(new Pair { a = 1 });
        ж<Pair> y = new StandardBox<Pair>(new Pair { a = 2 });
        ж<Pair> z = new StandardBox<Pair>(new Pair { a = 3 });

        Store(slot, @unsafe.Pointer.FromBox(x));
        AssertWordNames(WordOf(word), x, "the stored referent");
        nuint stored = WordOf(word);

        CollectRound();

        Assert.IsFalse(atomic.CompareAndSwapPointer(slot, @unsafe.Pointer.FromBox(y), @unsafe.Pointer.FromBox(z)), "a CAS whose old is another box fails");
        Assert.AreEqual(stored, WordOf(word), "and leaves the word unchanged");

        // Item (i), the positive control: a fresh FromBox mint of the box the load returned.
        object? referent = Load(slot).Referent;
        Assert.AreSame(x, referent, "the load answers the stored box");
        Assert.IsTrue(atomic.CompareAndSwapPointer(slot, @unsafe.Pointer.FromBox((ж<Pair>)referent!), @unsafe.Pointer.FromBox(z)), "a CAS whose old is a fresh mint of the stored box succeeds");
        AssertWordNames(WordOf(word), z, "the swapped-in referent");
        Assert.AreSame(z, Load(slot).Referent, "and the load answers the new box");
    }

    [TestMethod]
    public void B12_AReferenceBearingReferentIsStoredAsItsOrderToken()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Holder> r = new StandardBox<Holder>(new Holder { s = "kept" });

        Store(slot, @unsafe.Pointer.FromBox(r));

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(WordOf(word)), "a referent that holds a reference has no address: the word is its token");
        AssertWordNames(WordOf(word), r, "a reference-bearing referent");
        Assert.AreSame(r, Load(slot).Referent, "and the load answers its box");
    }

    [TestMethod]
    public void B12_AnUnpinnableReferentIsRefusedByName()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Named> container = new StandardBox<Named>(new Named { name = "tcp", scalar = 5 });
        ж<nint> field = container.of(Named.Ꮡscalar);
        Assert.AreEqual(PointerStorage.Unpinnable, field.StorageKind, "the premise: a field of a struct that holds a reference");

        PanicException? refused = null;

        try
        {
            Store(slot, @unsafe.Pointer.FromBox(field));
        }
        catch (PanicException panic)
        {
            refused = panic;
        }

        Assert.IsNotNull(refused, "a store of storage that can be neither pinned nor tokened is refused");
        StringAssert.Contains(refused!.Message, "can be neither pinned nor tokened");
        Assert.AreEqual((nuint)0, WordOf(word), "and stores nothing");
    }

    // ---- B-8: the counted bag ----

    [TestMethod]
    public void B8_StoringTheSameWordTwiceAndACasOfItselfKeepTheReferent()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        WeakReference weak = StoreTwiceAndCasItself(slot);
        AssertWordNames(WordOf(word), weak.Target!, "the twice-stored referent");

        for (int round = 0; round < 3; round++)
            CollectRound();

        Assert.IsTrue(weak.IsAlive, "the entry for the current word never reaches count 0");
        object? first = Load(slot).Referent;
        object? second = Load(slot).Referent;
        Assert.IsNotNull(first);
        Assert.AreSame(first, second, "both loads answer the same box");
        Assert.AreSame(weak.Target, first);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference StoreTwiceAndCasItself(ж<@unsafe.Pointer> slot)
    {
        ж<Pair> a = new StandardBox<Pair>(new Pair { a = 7 });
        @unsafe.Pointer pa = @unsafe.Pointer.FromBox(a);
        Store(slot, pa);
        Store(slot, pa);
        Assert.IsTrue(atomic.CompareAndSwapPointer(slot, pa, pa), "CAS(a, a) succeeds");
        return new WeakReference(a);
    }

    // ---- B-6: racing stores and a loader ----

    [TestMethod]
    public void B6_RacingStoresAndALoaderOnlyEverSeeAWholeReferent()
    {
        (ж<uintptr> word, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Pair> a = new StandardBox<Pair>(new Pair { a = 1 });
        ж<Pair> b = new StandardBox<Pair>(new Pair { a = 2 });
        @unsafe.Pointer pa = @unsafe.Pointer.FromBox(a);
        @unsafe.Pointer pb = @unsafe.Pointer.FromBox(b);
        atomic.StorePointer(slot, pa);
        AssertWordNames(WordOf(word), a, "the first store");

        const int n = 2000;
        int stray = 0;
        using ManualResetEventSlim go = new(false);
        bool done = false;

        Thread wa = new(() => { go.Wait(); for (int i = 0; i < n; i++) atomic.StorePointer(slot, pa); });
        Thread wb = new(() => { go.Wait(); for (int i = 0; i < n; i++) atomic.StorePointer(slot, pb); });
        Thread reader = new(() =>
        {
            go.Wait();
            while (!Volatile.Read(ref done))
            {
                object? seen = atomic.LoadPointer(slot).Referent;
                if (!ReferenceEquals(seen, a) && !ReferenceEquals(seen, b))
                    Interlocked.Increment(ref stray);
            }
        });

        wa.Start();
        wb.Start();
        reader.Start();
        go.Set();
        wa.Join();
        wb.Join();
        Volatile.Write(ref done, true);
        reader.Join();

        Assert.AreEqual(0, stray, "every load answered box a or box b, never a number");

        CollectRound();
        object? last = atomic.LoadPointer(slot).Referent;
        Assert.IsTrue(ReferenceEquals(last, a) || ReferenceEquals(last, b), "the final word names one of the two boxes");
        AssertWordNames(WordOf(word), last!, "the final word");
    }

    // ---- B-7: memmove copies the entry ----

    [TestMethod]
    public void B7_AMemmoveCopiesTheReferentAndTheSourceKeepsIt()
    {
        slice<uintptr> words = new(2);
        ж<@unsafe.Pointer> src = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 0)));
        ж<@unsafe.Pointer> dst = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 1)));
        ж<Pair> x = new StandardBox<Pair>(new Pair { a = 9 });

        Store(src, @unsafe.Pointer.FromBox(x));
        AssertWordNames(words[0].Value, x, "the source word");

        runtime_package.GoMemmove(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 1)), @unsafe.Pointer.FromPinnedBox(Ꮡ(words, 0)), new uintptr(8));

        Assert.AreEqual(words[0].Value, words[1].Value, "the word was copied");
        Assert.AreSame(x, Load(dst).Referent, "the destination reads the box");
        Assert.AreSame(x, Load(src).Referent, "the source still reads it");

        // A copy over []uintptr is a copy of numbers: Go keeps nothing alive by a uintptr.
        slice<uintptr> numbers = new(2);
        copy(numbers, words);
        ж<@unsafe.Pointer> copied = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromPinnedBox(Ꮡ(numbers, 0)));
        Assert.IsNull(Load(copied).RetainedSource, "a word copied by `copy` over []uintptr reads as a number");

        GC.KeepAlive(x);
    }

    // ---- B-9: a clear drops the entry ----

    [TestMethod]
    public void B9_AClearDropsTheReferent()
    {
        slice<uintptr> words = new(1);
        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 0)));
        WeakReference key = StoreKey(slot, words);

        runtime_package.GoMemclrNoHeapPointers(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 0)), new uintptr(8));
        Assert.AreEqual((nuint)0, words[0].Value, "the clear zeroed the word");

        Assert.AreNotEqual(0, RoundsToDeath(key)[0], "the cleared word no longer keeps the key");
        GC.KeepAlive(words);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference StoreKey(ж<@unsafe.Pointer> slot, slice<uintptr> words)
    {
        ж<Pair> k = new StandardBox<Pair>(new Pair { a = 5 });
        Store(slot, @unsafe.Pointer.FromBox(k));
        AssertWordNames(words[0].Value, k, "the stored key");
        return new WeakReference(k);
    }

    // ---- B-4: the cycle arms, one test, one loop (design 18.7 finding 3) ----
    //
    // c2 (no cycle) is the CONTROL that the loop collects. c1 (a reference-bearing referent that reaches the destination's
    // box), c3 (self) and c4 (mutual) are the named leak: each reads ALIVE after the drop and the cap, and DEAD after one
    // word of the cycle is overwritten through W2 and the cap runs again.

    [TestMethod]
    public void B4_CyclesThroughPublishedWordsLeakUntilAWordIsOverwrittenAndTheControlDies()
    {
        WeakReference c2 = MakeControl();
        WeakReference c1 = MakeReferenceCycle();
        WeakReference c3 = MakeSelf();
        (WeakReference c4a, WeakReference c4b) = MakeMutual();

        int[] first = RoundsToDeath(c2, c1, c3, c4a, c4b);
        Console.WriteLine($"step one, round of death (0 = alive after {Rounds}): c2 {first[0]}, c1 {first[1]}, c3 {first[2]}, c4 {first[3]}/{first[4]}");

        Assert.AreNotEqual(0, first[0], "INSTRUMENT: the control c2 must die within the cap, or no reading below means anything");
        Assert.AreEqual(0, first[1], "c1 (a referent that reaches the destination's own box) leaks while published");
        Assert.AreEqual(0, first[2], "c3 (self) leaks while published");
        Assert.AreEqual(0, first[3], "c4 (mutual) leaks while published: A");
        Assert.AreEqual(0, first[4], "c4 (mutual) leaks while published: B");

        OverwriteFirstWord(c1);
        OverwriteFirstWord(c3);
        OverwriteFirstWord(c4a);

        int[] second = RoundsToDeath(c1, c3, c4a, c4b);
        Console.WriteLine($"step two, round of death: c1 {second[0]}, c3 {second[1]}, c4 {second[2]}/{second[3]}");

        Assert.AreNotEqual(0, second[0], "c1 dies after its word is overwritten");
        Assert.AreNotEqual(0, second[1], "c3 dies after its word is overwritten");
        Assert.AreNotEqual(0, second[2], "c4's A dies after one word of the cycle is overwritten");
        Assert.AreNotEqual(0, second[3], "c4's B dies after one word of the cycle is overwritten");
    }

    // c2: a word backing D stores a fresh value-type referent; nothing reaches D. Answers D.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeControl()
    {
        (ж<uintptr> d, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Pair> x = new StandardBox<Pair>(new Pair { a = 2 });
        Store(slot, @unsafe.Pointer.FromBox(x));
        AssertWordNames(WordOf(d), x, "c2's word");
        return new WeakReference(d);
    }

    // c1: D stores a reference-bearing referent that holds D's own box. Answers D.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeReferenceCycle()
    {
        (ж<uintptr> d, ж<@unsafe.Pointer> slot) = WordSlot();
        ж<Holder> r = new StandardBox<Holder>(new Holder { s = "c1", root = d });
        Store(slot, @unsafe.Pointer.FromBox(r));
        AssertWordNames(WordOf(d), r, "c1's word");
        return new WeakReference(d);
    }

    // c3: D stores its own address. Answers D.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeSelf()
    {
        (ж<uintptr> d, ж<@unsafe.Pointer> slot) = WordSlot();
        Store(slot, @unsafe.Pointer.FromBox(d));
        AssertWordNames(WordOf(d), d, "c3's word");
        return new WeakReference(d);
    }

    // c4: A stores B's address and B stores A's. Answers A and B.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, WeakReference) MakeMutual()
    {
        (ж<uintptr> a, ж<@unsafe.Pointer> slotA) = WordSlot();
        (ж<uintptr> b, ж<@unsafe.Pointer> slotB) = WordSlot();
        Store(slotA, @unsafe.Pointer.FromBox(b));
        Store(slotB, @unsafe.Pointer.FromBox(a));
        AssertWordNames(WordOf(a), b, "c4's word in A");
        AssertWordNames(WordOf(b), a, "c4's word in B");
        return (new WeakReference(a), new WeakReference(b));
    }

    // Step two reaches the slot through the weak reference's target, which is alive BECAUSE of the leak.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void OverwriteFirstWord(WeakReference weak)
    {
        if (weak.Target is not ж<uintptr> backing)
        {
            Assert.Fail("NEWS: a leaked backing read dead before its word was overwritten");
            return;
        }

        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromBox(backing));
        Store(slot, @unsafe.Pointer.Nil);
    }

    // ---- W2(a) and W1 over C memory ----

    private static (nuint memory, ж<@unsafe.Pointer> slot) CWordSlot()
    {
        nuint memory = (nuint)NativeMemory.AllocZeroed(16);
        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(new @unsafe.Pointer((uintptr)memory));
        return (memory, slot);
    }

    [TestMethod]
    public void W2a_AGoPointerStoredIntoCMemoryIsRefusedByNameAndANumberIsWritten()
    {
        (nuint memory, ж<@unsafe.Pointer> slot) = CWordSlot();

        try
        {
            ж<Pair> x = new StandardBox<Pair>(new Pair { a = 1 });
            PanicException? refused = null;

            try
            {
                Store(slot, @unsafe.Pointer.FromBox(x));
            }
            catch (PanicException panic)
            {
                refused = panic;
            }

            Assert.IsNotNull(refused, "a Go pointer stored into C memory is refused");
            StringAssert.Contains(refused!.Message, "a Go pointer into C memory");
            Assert.AreEqual(0L, Marshal.ReadInt64((nint)memory), "and stores nothing");

            Store(slot, new @unsafe.Pointer((uintptr)0x1238));
            Assert.AreEqual(0x1238L, Marshal.ReadInt64((nint)memory), "a number is written as the word");
            Assert.AreEqual((nuint)0x1238, (nuint)(uintptr)Load(slot), "and loads back as the number");
            Assert.IsNull(Load(slot).RetainedSource, "which carries nothing");

            Store(slot, @unsafe.Pointer.Nil);
            Assert.AreEqual(0L, Marshal.ReadInt64((nint)memory), "nil writes 0");
            Assert.IsTrue(Load(slot) == nil, "and 0 loads as nil");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void W1_AGoAddressReadOutOfCMemoryIsRefusedByName()
    {
        (nuint memory, ж<@unsafe.Pointer> slot) = CWordSlot();

        try
        {
            ж<Pair> x = new StandardBox<Pair>(new Pair { a = 1 });
            uintptr pinned = x; // the box's own number conversion pins it and registers the address
            Marshal.WriteInt64((nint)memory, (long)pinned.Value);

            PanicException? refused = null;

            try
            {
                _ = Load(slot);
            }
            catch (PanicException panic)
            {
                refused = panic;
            }

            Assert.IsNotNull(refused, "a word in C memory that names Go memory is refused");
            StringAssert.Contains(refused!.Message, "a Go pointer read out of C memory");
            GC.KeepAlive(x);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }
}
