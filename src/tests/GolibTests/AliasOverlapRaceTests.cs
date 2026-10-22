using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using alias = go.crypto.@internal.fips140.alias_package;
using valias = go.vendor.golang.org.x.crypto.@internal.alias_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;

namespace GolibTests;

// The crypto/internal/alias address-ordering RACE — its mechanism as a live test, the structural
// contract that replaces it, and two stress guards over the REAL converted code.
//
// Go writes AnyOverlap (and slices.overlaps) by ORDERING element addresses, and the converter emitted
// that literally as four `(uintptr)Ꮡ(…)` takes. Each take pins its backing through a FINALIZABLE holder
// on a box that is garbage the instant the take returns, so the pin is released by the finalizer, not by
// the next take; a collection landing between two takes relocates an operand whose earlier pin has
// already been finalized, and the ordering then compares two heap layouts. Measured 2026-09-03 on a
// 4-core host, Release, tiering off, 16 threads + 2 allocation-churn threads (the probe is the C2 lane's
// `c2-overlap-probe`; the numbers are in the commit that landed this file):
//
//     the mirrored four-take predicate     RED at 17 s / 913,047 calls — FIVE threads torn on ONE collection,
//                                          every quadruple four real heap addresses, exactly one array's pair
//                                          inconsistent (x relocated between take 1 and take 4 by 3.6–43.9 MB,
//                                          or y between take 2 and take 3), every re-take 15/15 and FALSE
//     the converted alias.AnyOverlap       RED at 9 s / 280,239 calls (TRUE for two distinct fresh arrays)
//     the converted GCM Open, TLS shape    PANIC `crypto/aes: invalid buffer overlap` at 27 s / 1,405 records,
//                                          through crypto/aes Encrypt ← counterCrypt ← Open — the chain that
//                                          killed the banked net/http row on two host classes
//
// ⚠ THE CONFIGURATION IS PART OF THE MEASUREMENT. The SAME probe read six million clean calls in 120 s at
// Debug and went red in nine seconds at Release with tiering off: a non-optimizing frame roots its
// temporaries for the method's life, so all four pins hold until the predicate returns and the takes are
// atomic by accident. A pin-lifetime or GC-liveness probe runs at Release with tiering off, or it is not
// a measurement — and which way the configuration cuts is decided per case by running it (the
// internal/poll finalizer hypothesis was measured FALSE the same way, and both readings stand). The two
// stress guards below therefore report INCONCLUSIVE, never green, when the assembly under test carries
// a JIT-optimizer-disabled DebuggableAttribute; run `dotnet test -c Release` for the live guard.
//
// The remedy is structural: golib slice<T>.Overlaps answers by canonical backing identity + absolute
// index-range intersection (native-address and zero-size arms), and AnyOverlap / slices.overlaps are
// displaced onto it through manualConversionFuncs (crypto/internal/alias/alias_impl.cs,
// slices/slices_impl.cs). AliasOverlapTests.cs keeps the seven contract assertions; this file adds the
// mechanism, the structural contract, and the stress guards that are RED on the address form.
[TestClass]
public class AliasOverlapRaceTests
{
    private static int StressSeconds =>
        int.TryParse(Environment.GetEnvironmentVariable("GO2CS_OVERLAP_STRESS_SECONDS"), out int s) && s > 0 ? s : 20;

    private static int StressThreads => Math.Max(8, Environment.ProcessorCount * 4);

    private static bool JitOptimizerDisabled(Assembly assembly) =>
        assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true;

    // The pin an address-take holds (ж<T>.m_pin), read so the arm can watch it die without holding it.
    private static readonly FieldInfo s_pinField =
        typeof(ж<byte>).GetField("m_pin", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // The take lives in its own NON-INLINED frame so no slot of the calling frame can keep its box alive —
    // the first cut of this probe took the address inline and read NOT MOVED at Debug for that reason
    // alone. It returns the address and a resurrection-tracking weak reference to the box's pin, which
    // dies only once the pin has been finalized (its handle freed) and then collected.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (nuint Address, WeakReference Pin) Take(slice<byte> x, int i)
    {
        ж<byte> box = Ꮡ(x, i);
        nuint address = (nuint)(uintptr)box;
        object? pin = s_pinField.GetValue(box);
        Assert.IsNotNull(pin, "the take pinned the array, which is what the arm is about");
        return (address, new WeakReference(pin, trackResurrection: true));
    }

    // The array's address now, under a pin that ends with the statement.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe nuint Raw(byte[] array) { fixed (byte* p = array) return (nuint)p; }

    // Finalize-and-compact rounds the arm offers the runtime before it calls the relocation unobserved.
    private const int RelocationRounds = 8;

    [TestMethod]
    public void TakenElementAddressIsNotStableOnceItsPinIsFinalized()
    {
        // THE MECHANISM, kept live: an address taken through the uintptr operator is the array's address
        // only while the take's own pin holds, and that pin is released by its FINALIZER. Two halves:
        //
        //   - The pin's lifetime is golib's, and it is asserted: through the first compacting collection the
        //     array stays where take 1 read it, and once the take's box is gone its pin is finalized and
        //     collected.
        //   - Relocation afterwards is the CLR's choice, never a guarantee: a compacting collection moves the
        //     array only if free space opens below it. Under a full GolibTests run (heap layout, pinned
        //     neighbours) it can stay put, which failed this arm once in a batch battery while it passed in
        //     isolation. So the arm offers RelocationRounds finalize-and-compact rounds and reports
        //     Inconclusive, not failure, when the array never moves.
        //
        // A never-pinned control array shows the construction can relocate at all.
        byte[][] below = new byte[40000][];
        for (int i = 0; i < below.Length; i++) below[i] = new byte[16];
        byte[] control = new byte[16];
        byte[] backing = new byte[16];
        byte[][] above = new byte[40000][];
        for (int i = 0; i < above.Length; i++) above[i] = new byte[16];
        slice<byte> x = backing.slice();

        nuint control0 = Raw(control);
        (nuint take1, WeakReference pin1) = Take(x, 0);
        below = null!; above = null!;                   // the gaps a compacting collection closes

        GC.Collect(2, GCCollectionMode.Forced, true, true);
        nuint control1 = Raw(control);
        nuint whilePinned = Raw(backing);

        nuint current = whilePinned;
        int rounds = 0;

        while (rounds < RelocationRounds && (pin1.IsAlive || current == take1))
        {
            rounds++;
            GC.WaitForPendingFinalizers();              // an unreachable take's pin finalizes: its handle is freed
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            current = Raw(backing);
        }

        nuint control2 = Raw(control);
        GC.KeepAlive(backing);
        GC.KeepAlive(control);

        if (control2 == control0 && control1 == control0)
        {
            // Not a silent pass: a runtime that did not relocate even the unpinned control cannot stage
            // the demonstration (no gap closed), and the assertions below would be vacuous.
            Assert.Inconclusive(
                $"the never-pinned control array did not relocate (0x{control0:X}) — this runtime closed no " +
                "gap below it, so the pin-lifetime demonstration cannot be staged here");
            return;
        }

        Assert.AreEqual(take1, whilePinned,
            "through the first compacting collection the array is still pinned by take 1's unfinalized " +
            "box, so it is still where take 1 read it");
        Assert.IsFalse(pin1.IsAlive,
            $"take 1's pin must be finalized and collected once its box is unreachable; it was still alive after " +
            $"{rounds} finalize-and-compact rounds");

        if (current == take1)
        {
            Assert.Inconclusive(
                $"take 1's pin was released, but the array stayed at 0x{take1:X} through {rounds} compacting " +
                $"collections (control 0x{control0:X} -> 0x{control1:X} -> 0x{control2:X}): relocation is the " +
                "runtime's choice, so this run did not observe the address going stale");
            return;
        }

        Assert.AreNotEqual(take1, current,
            "once take 1's pin has been finalized, a compacting collection relocated the array: the address from " +
            "take 1 is not the array's address any more — two takes are only comparable while the first one's " +
            "pin holds, which is exactly what the four-take predicate assumed and never had");
    }

    [TestMethod]
    public void SliceOverlapsAnswersByStorageAndIndexRange()
    {
        // The structural contract AnyOverlap and slices.overlaps are displaced onto: no address is read,
        // so nothing can tear. Every arm the address form used to answer by ordering.
        byte[] a = new byte[32];
        byte[] b = new byte[32];

        Assert.IsFalse(a.slice().Overlaps(b.slice()), "two distinct backing arrays share no memory");
        Assert.IsFalse(a.slice(0, 16).Overlaps(a.slice(16, 32)), "adjacent-but-disjoint windows of one array touch but share no element");
        Assert.IsTrue(a.slice(0, 20).Overlaps(a.slice(10, 32)), "windows [0,20) and [10,32) of one array share elements 10..19");
        Assert.IsTrue(a.slice(10, 32).Overlaps(a.slice(0, 20)), "Overlaps is symmetric");
        Assert.IsTrue(a.slice(4, 20).Overlaps(a.slice(4, 20)), "a window overlaps itself");
        Assert.IsTrue(a.slice().Overlaps(a.slice(0, 8)), "the whole window overlaps its own prefix");
        Assert.IsTrue(a.slice(8, 9).Overlaps(a.slice(0, 32)), "a one-element window inside the whole overlaps it");
        Assert.IsFalse(a.slice(4, 4).Overlaps(a.slice()), "a zero-length window names no memory");
        Assert.IsFalse(a.slice().Overlaps(a.slice(4, 4)), "…in either argument position");
        Assert.IsFalse(default(slice<byte>).Overlaps(a.slice()), "the nil slice names no memory");
        Assert.IsFalse(a.slice().Overlaps(default(slice<byte>)), "…in either argument position");

        // Every zero-size slice shares ONE static backing in golib, so an index-range answer alone would
        // report any two of them as overlapping; Go's `elemSize == 0` early-out is load-bearing here.
        slice<EmptyStruct> z1 = new slice<EmptyStruct>(8);
        slice<EmptyStruct> z2 = new slice<EmptyStruct>(8);
        Assert.IsFalse(z1.Overlaps(z2), "zero-size elements never overlap (Go: elemSize == 0)");
        Assert.IsFalse(z1.Overlaps(z1), "…not even a zero-size window with itself");

        GC.KeepAlive(a);
        GC.KeepAlive(b);
    }

    [TestMethod]
    public unsafe void SliceOverlapsAnswersNativeWindowsByAddressRange()
    {
        // The native arm: a slice over unmanaged memory has no backing array, so overlap is the exact
        // address-range intersection — and a managed window never overlaps a native one (two spaces).
        // OverNativeMemory is golib's single creation door for native-backed slices (internal, visible to
        // this project).
        nint block = System.Runtime.InteropServices.Marshal.AllocHGlobal(128);
        nint other = System.Runtime.InteropServices.Marshal.AllocHGlobal(64);

        try
        {
            slice<byte> whole = go.slice<byte>.OverNativeMemory((nuint)block, 64);
            slice<byte> shifted = go.slice<byte>.OverNativeMemory((nuint)(block + 16), 64);   // a different base over the same bytes
            slice<byte> elsewhere = go.slice<byte>.OverNativeMemory((nuint)other, 64);
            byte[] managed = new byte[64];

            Assert.IsFalse(whole[..16].Overlaps(whole[16..32]), "adjacent native windows share no byte");
            Assert.IsTrue(whole[..20].Overlaps(whole[10..32]), "native windows [0,20) and [10,32) share bytes 10..19");
            Assert.IsTrue(whole.Overlaps(shifted), "two bases over the same unmanaged bytes overlap by address");
            Assert.IsTrue(shifted.Overlaps(whole), "…symmetrically");
            Assert.IsFalse(whole[..16].Overlaps(shifted[16..]), "block[0,16) and block[32,80) do not overlap even across bases");
            Assert.IsFalse(whole.Overlaps(elsewhere), "two unrelated unmanaged blocks share no byte");
            Assert.IsFalse(whole.Overlaps(managed.slice()), "a native window and a managed one live in different spaces");
            Assert.IsFalse(managed.slice().Overlaps(whole), "…in either argument position");
            Assert.IsFalse(whole[..0].Overlaps(whole), "a zero-length native window names no memory");

            GC.KeepAlive(managed);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(block);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(other);
        }
    }

    [TestMethod]
    public void ConvertedAnyOverlapNeverReportsDistinctArraysUnderStress()
    {
        // The REAL converted predicate, under the stress that turned its address form red in nine
        // seconds on a 4-core host: oversubscribed threads (preemption INSIDE the four-take window is the
        // ingredient), two allocation-churn threads, a fresh pair of arrays per "record" and 1,024 calls per
        // pair (one 16 KB record of block encryptions). RED on the address-ordering form, GREEN on the
        // structural one; INCONCLUSIVE at Debug, where a non-optimizing frame masks the race.
        if (JitOptimizerDisabled(typeof(alias).Assembly))
        {
            Assert.Inconclusive("crypto/internal/alias is a JIT-optimizer-disabled (Debug) build: the four-take " +
                "tear is masked by frame liveness there, so this guard measures nothing — run GolibTests -c Release");
            return;
        }

        int failures = RunStress(() =>
        {
            byte[] mask = new byte[16];
            byte[] counter = new byte[16];
            slice<byte> x = mask.slice(), y = counter.slice();

            for (int k = 0; k < 1024; k++)
            {
                if (alias.InexactOverlap(x, y) || alias.AnyOverlap(x, y))
                    return false;
            }

            GC.KeepAlive(mask);
            GC.KeepAlive(counter);
            return true;
        });

        Assert.AreEqual(0, failures,
            "alias.AnyOverlap/InexactOverlap reported two DISTINCT fresh arrays as overlapping — the " +
            "address-ordering tear is back (or a new one): overlap must be answered by storage identity and " +
            "index range, never by comparing addresses taken one pin at a time");
    }

    [TestMethod]
    public void ConvertedGcmOpenNeverPanicsWithOverlapUnderStress()
    {
        // The shape that killed net/http: cipher.NewGCM(aes.NewCipher(key)).Open(payload[:0], nonce,
        // payload, ad) over 16 KB records — crypto/tls conn.decrypt's exact call — with a fresh cipher +
        // GCM every 64 records (a new connection). counterCrypt calls Encrypt(mask[:], counter[:]) once per
        // block, and crypto/aes guards that with alias.InexactOverlap. PANIC on the address form at 27 s
        // here; nothing on the structural one. INCONCLUSIVE at Debug for the reason in the header.
        if (JitOptimizerDisabled(typeof(alias).Assembly))
        {
            Assert.Inconclusive("crypto/internal/alias is a JIT-optimizer-disabled (Debug) build: the four-take " +
                "tear is masked by frame liveness there, so this guard measures nothing — run GolibTests -c Release");
            return;
        }

        byte[] key = new byte[16];
        for (int i = 0; i < key.Length; i++) key[i] = (byte)(i * 7 + 3);
        byte[] nonce = new byte[12];
        byte[] additionalData = new byte[13];
        byte[] plaintext = new byte[16384];
        for (int i = 0; i < plaintext.Length; i++) plaintext[i] = (byte)i;

        string? firstPanic = null;

        int failures = RunStress(() =>
        {
            var (block, err) = aes.NewCipher(key.slice());
            Assert.IsNull(err, "aes.NewCipher");
            var (aead, err2) = cipher.NewGCM(block);
            Assert.IsNull(err2, "cipher.NewGCM");
            slice<byte> sealedRecord = aead.Seal(default, nonce.slice(), plaintext.slice(), additionalData.slice());

            for (int record = 0; record < 64; record++)
            {
                slice<byte> payload = sealedRecord.Source.slice();   // a fresh read buffer per record

                try
                {
                    var (plain, oerr) = aead.Open(payload[..0], nonce.slice(), payload, additionalData.slice());

                    if (oerr is not null || len(plain) != plaintext.Length)
                    {
                        Interlocked.CompareExchange(ref firstPanic, "Open returned " + (oerr is null ? "a wrong length" : oerr.Error()), null);
                        return false;
                    }
                }
                catch (PanicException ex)
                {
                    Interlocked.CompareExchange(ref firstPanic, ex.Message, null);
                    return false;
                }
            }

            return true;
        });

        Assert.AreEqual(0, failures,
            $"the converted GCM Open raised '{firstPanic}' on records whose mask and counter are two distinct " +
            "fresh arrays — crypto/aes's alias.InexactOverlap guard tore between its address takes");
    }

    [TestMethod]
    public void VendoredAnyOverlapAnswersByStorageAndIndexRange()
    {
        // The vendored purego twin — vendor/golang.org/x/crypto/internal/alias, the guard chacha20 and
        // chacha20poly1305 call through InexactOverlap on every XORKeyStream / Seal / Open — carries the
        // crypto/internal twin's contract: storage identity and index range decide, no address is read.
        byte[] a = new byte[32];
        byte[] b = new byte[32];
        Assert.IsFalse(valias.AnyOverlap(a.slice(), b.slice()), "two distinct backing arrays share no memory");
        Assert.IsFalse(valias.AnyOverlap(a.slice(0, 16), a.slice(16, 32)), "adjacent-but-disjoint windows of one array share no element");
        Assert.IsTrue(valias.AnyOverlap(a.slice(0, 20), a.slice(10, 32)), "windows [0,20) and [10,32) of one array share elements 10..19");
        Assert.IsTrue(valias.AnyOverlap(a.slice(10, 32), a.slice(0, 20)), "AnyOverlap is symmetric");
        Assert.IsTrue(valias.AnyOverlap(a.slice(), a.slice(0, 8)), "the whole window overlaps its own prefix");
        Assert.IsFalse(valias.AnyOverlap(a.slice(4, 4), a.slice()), "a zero-length window names no memory");
        Assert.IsFalse(valias.AnyOverlap(default(slice<byte>), a.slice()), "the nil slice names no memory");
        Assert.IsFalse(valias.InexactOverlap(a.slice(), a.slice()), "exact aliasing is not INEXACT overlap — chacha20's in-place XORKeyStream(dst, dst)");
        Assert.IsTrue(valias.InexactOverlap(a.slice(0, 20), a.slice(1, 21)), "a one-element shift is inexact overlap");
        Assert.IsFalse(valias.InexactOverlap(a.slice(), b.slice()), "distinct arrays: no inexact overlap");
    }

    [TestMethod]
    public void VendoredAnyOverlapDoesNotConfuseArraysWithCollidingIdentityHashes()
    {
        // The vendored purego twin orders `reflect.ValueOf(Ꮡ(x, 0)).Pointer()` values, and the reflect bridge
        // answers Pointer() for an element reference with ElemRefBox.PointerOrderToken — the backing's
        // RuntimeHelpers.GetHashCode shifted 32 bits plus the element index. That token is GC-stable, so the
        // four-take TEAR the crypto/internal twin measured cannot happen here (the 16-thread stress guard read
        // GREEN against this body for 30 s, measured 2026-09-03), but an identity hash is not an identity: two
        // DISTINCT live arrays can share one, and then their token ranges coincide and the order form reads
        // them as overlapping. chacha20poly1305's Seal/Open and chacha20's XORKeyStream guard with
        // InexactOverlap, so such a pair panics `invalid buffer overlap` on buffers that share no byte. The
        // search below allocates until two arrays collide (a 32-bit identity hash makes that a birthday
        // problem, not a rarity), keeping every candidate alive so no hash is recycled. RED on the token form,
        // GREEN on alias_purego_impl.cs over slice<T>.Overlaps, INCONCLUSIVE only if no collision surfaces
        // within the allocation budget.
        var keepAlive = new List<byte[]>(1 << 16);
        var byHash = new Dictionary<int, byte[]>(1 << 16);
        byte[]? first = null, second = null;
        int allocations = 0;

        while (first is null && allocations < (1 << 23))
        {
            byte[] candidate = new byte[64];
            keepAlive.Add(candidate);
            allocations++;
            int hash = RuntimeHelpers.GetHashCode(candidate);

            if (byHash.TryGetValue(hash, out byte[]? earlier))
            {
                first = earlier;
                second = candidate;
            }
            else
            {
                byHash[hash] = candidate;
            }
        }

        if (first is null || second is null)
        {
            Assert.Inconclusive($"no two of {allocations:N0} live arrays shared an identity hash within the budget; the collision arm is unmeasured here");
            return;
        }

        Assert.AreNotSame(first, second, "the search returned one array twice");
        Assert.AreEqual(RuntimeHelpers.GetHashCode(first), RuntimeHelpers.GetHashCode(second), "the pair does not actually collide");

        slice<byte> x = first.slice(), y = second.slice();

        Assert.IsFalse(valias.AnyOverlap(x, y),
            $"the vendored alias.AnyOverlap reported two DISTINCT arrays as overlapping because they share identity hash " +
            $"{RuntimeHelpers.GetHashCode(first):X} (found after {allocations:N0} allocations) — a hash-derived order token is not " +
            "storage identity; overlap must be answered by storage identity and index range");
        Assert.IsFalse(valias.InexactOverlap(x, y), "…and InexactOverlap, chacha20poly1305's Seal/Open guard, must agree");
        Assert.IsFalse(valias.AnyOverlap(y, x), "…in either argument position");

        GC.KeepAlive(keepAlive);
    }
    // A planted worker exception is REPORTED as this test's failure, and the host lives to say so.
    //
    // This is RunStress's own negative control, and it is a test rather than a comment because the
    // property it asserts is one nothing else in the suite can reach: the code path is "a raw worker
    // thread throws", whose UNHARDENED behaviour is not a failing test but a dead process — and a
    // dead process cannot report that it was supposed to fail. The arm is cheap (the plant stops the
    // run on the first poll, so it costs no stress seconds) and it is the only thing standing between
    // this file's hardening and a silent regression back to an abort.
    //
    // ⚠ IF THIS TEST EVER KILLS THE HOST INSTEAD OF FAILING, the hardening is gone. That is the
    // signature to look for: not a red here, but this class reporting fewer tests than it declares.
    [TestMethod]
    public void AStressWorkerExceptionFailsTheTestAndNotTheHost()
    {
        AssertFailedException? reported = null;

        try
        {
            RunStress(() => throw new InvalidOperationException("planted worker exception"));
        }
        catch (AssertFailedException ex)
        {
            reported = ex;
        }

        Assert.IsNotNull(reported,
            "RunStress swallowed a worker exception instead of reporting it. An exception that escapes a raw " +
            "worker thread is UNHANDLED and .NET ends the process — so the unhardened form of this defect is " +
            "not a failing test, it is a test host that dies with every test it had not run yet");
        StringAssert.Contains(reported!.Message, "planted worker exception",
            "the report must carry the original exception's MESSAGE: an abort with no message is what this " +
            "hardening exists to replace, and a report that loses the message is only a quieter abort");
        StringAssert.Contains(reported!.Message, nameof(InvalidOperationException),
            "…and its TYPE, which is what says whether the thrower was the code under test or this harness");
    }

    // Runs `body` on StressThreads oversubscribed workers plus two allocation-churn threads for
    // StressSeconds, stopping at the first false; returns the number of workers that reported false.
    //
    // ⚠ EVERY THREAD BODY IS WRAPPED, and that is the shape rather than a decoration on it. These are
    // raw `new Thread` workers: an exception that escapes one is an UNHANDLED exception, and .NET
    // terminates the PROCESS — the test host, with every test that had not run yet. Measured, four
    // times out of four: the GCM arm's first `Seal` threw NotImplementedException from
    // crypto/internal/fips140's generated `setIndicator` stub (RED 7's class) and took the host with
    // it, leaving two tests in this class UNRUN and the leg reporting an ABORT (i9, c0eecf8850).
    //
    // An abort says nothing about which test failed or why, and it costs the tests that never ran.
    // A FAIL says both and costs nothing. The bodies here also call MSTest's Assert — whose failures
    // are exceptions too, and were just as fatal on a worker as any other.
    //
    // So: the FIRST exception from any thread — worker or churn — is captured, stops the run, and is
    // re-reported on the TEST's own thread after the join, where MSTest attributes it to this test.
    // A thrower also counts as a failure, so no path can report success if that reporting is ever
    // changed. The negative control above plants one deliberately.
    private static int RunStress(Func<bool> body)
    {
        int seconds = StressSeconds;
        int workers = StressThreads;
        bool stop = false;
        int failures = 0;
        Exception? firstError = null;
        string? firstErrorThread = null;
        var threads = new Thread[workers + 2];

        // First writer wins the report; every thread stops either way.
        void Capture(string which, Exception error)
        {
            if (Interlocked.CompareExchange(ref firstError, error, null) is null)
                Volatile.Write(ref firstErrorThread, which);

            Interlocked.Increment(ref failures);
            Volatile.Write(ref stop, true);
        }

        for (int i = 0; i < workers; i++)
        {
            int index = i;

            threads[i] = new Thread(() =>
            {
                try
                {
                    while (!Volatile.Read(ref stop))
                    {
                        if (!body())
                        {
                            Interlocked.Increment(ref failures);
                            Volatile.Write(ref stop, true);
                            return;
                        }
                    }
                }
                catch (Exception error)
                {
                    Capture($"worker {index}", error);
                }
            }) { IsBackground = true };
        }

        for (int i = workers; i < workers + 2; i++)
        {
            int index = i - workers;

            threads[i] = new Thread(() =>
            {
                try
                {
                    long n = 0;
                    while (!Volatile.Read(ref stop))
                    {
                        byte[] garbage = new byte[256];
                        garbage[0] = (byte)n;
                        if ((++n & 0x3FFF) == 0)
                            Thread.Yield();
                    }
                }
                catch (Exception error)
                {
                    // The churn threads only allocate, so nothing is EXPECTED here — which is exactly
                    // why it is wrapped. An OutOfMemoryException on one of these would have ended the
                    // host as surely as the GCM stub did, and been harder to attribute.
                    Capture($"allocation-churn thread {index}", error);
                }
            }) { IsBackground = true };
        }

        foreach (Thread t in threads)
            t.Start();

        var sw = Stopwatch.StartNew();

        while (!Volatile.Read(ref stop) && sw.Elapsed.TotalSeconds < seconds)
            Thread.Sleep(100);

        Volatile.Write(ref stop, true);

        int unjoined = 0;

        foreach (Thread t in threads)
        {
            if (!t.Join(5000))
                unjoined++;
        }

        if (firstError is not null)
        {
            Assert.Fail($"a stress {firstErrorThread} threw {firstError.GetType().FullName}: {firstError.Message}" +
                Environment.NewLine + Environment.NewLine + firstError + Environment.NewLine + Environment.NewLine +
                "Reported here rather than left to end the process: an exception escaping a raw worker thread is " +
                "unhandled, and the host dies with every test it had not run yet. If the thrower is a generated " +
                "NotImplementedException, this is RED 7's class reaching a test rather than a defect in what the " +
                "test measures — the declared-not-implemented census names the member.");
        }

        if (unjoined > 0)
        {
            Assert.Fail($"{unjoined} of {threads.Length} stress threads did not stop within 5 s of being asked to. " +
                "They are background threads, so the process will not wait for them and the NEXT test inherits " +
                "them — which is a finding about this guard's own machinery, not about the code under test.");
        }

        return failures;
    }
}
