// ElementIndexProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ⚠ PROBE. NEVER TO MERGE. It exists to answer ONE question that
// docs/phase4/DESIGN-token-value-tag-refusal.md §4.1 leaves owed, and the prediction for it is on
// the mailbox BEFORE this file was written so it cannot be tuned to what the probe measures.
//
// THE QUESTION. A value tag on minted order tokens costs bits, and the budget is
// tag + hash + displacement = 64 with the displacement currently 32. ElemRefBox stores
// `m_index = slice.Low + index` -- the ABSOLUTE index into the BACKING array, not the index within
// the slice -- so the displacement must hold whatever absolute index a real workload reaches. The
// fixed-size arm is already answered by a static census (production max 32,768, zero at or above
// 65,536, and the one 131,072 array has no address-take). This measures the SLICE arm, which
// nothing static bounds.
//
// TWO COUNTERS, BECAUSE THE PREDICATE READS TWO SITES AND ONLY ONE DECIDES THE TAG:
//
//   max_ctor    every ElemRefBox constructed, whether or not its token is ever read. The ceiling
//               if the token arm is ever widened.
//   max_token   only indices whose token is actually MINTED (PointerOrderToken's managed branch).
//               This is the tag's safety TODAY -- an index whose token is never minted cannot
//               carry a tag and cannot overflow one.
//
// `ctor_calls` and `token_reads` ride along so a zero MAXIMUM can be told apart from a path never
// entered -- the difference between "nothing was that big" and "nothing ran".
//
// THE POSITIVE CONTROL IS WIRED IN, NOT REMEMBERED. A module initializer drives all THREE
// constructors and one token read at known large indices, checks that the counters moved to exactly
// those values, prints what it found, and then ZEROES the counters so the control cannot
// contaminate the measurement. If the hooks are missing the control prints FAILED and says so
// loudly: a run whose control did not fire has no numbers, only an absence.
//
// ⚠ ONE COST THE CONTROL IMPOSES, STATED RATHER THAN DISCOVERED. Driving the three real
// constructors means three real ElemRefBox allocations before Main, so golib's own
// AllocationCounter reads THREE higher on a probe run than on a clean one, and the three
// control backings are about 840 KB of transient int[]. A row that ASSERTS an allocation
// count must not be read from a probe run. The alternative -- poking the counters directly --
// would cost nothing and prove nothing, because what can actually be missing is the HOOKS.
//
// READING IT. One line on stderr at process exit:
//
//   ELEMPROBE max_ctor=<A> max_token=<B> ctor_calls=<C> token_reads=<D>
//
// preceded, once, by:
//
//   ELEMPROBE-CONTROL fired ctor=<n> token=<n>            (or ... FAILED ...)

using System;
using System.Threading;

namespace go;

internal static class ElementIndexProbe
{
    private static long s_maxCtor;
    private static long s_maxToken;
    private static long s_ctorCalls;
    private static long s_tokenReads;

    // The index the control drives every arm to. Large enough to be unmistakable and to exceed the
    // 16-bit block the measurement exists to price; small enough that three backing arrays of that
    // length cost under a megabyte.
    private const int ControlIndex = 70000;

    internal static void Ctor(nint index)
    {
        Interlocked.Increment(ref s_ctorCalls);
        RecordMax(ref s_maxCtor, index);
    }

    internal static void Token(nint index)
    {
        Interlocked.Increment(ref s_tokenReads);
        RecordMax(ref s_maxToken, index);
    }

    // A racing max, done properly: the parallel test phase drives this from many threads at once,
    // and a plain compare-and-store would silently drop the very outlier the probe exists to catch.
    private static void RecordMax(ref long slot, nint index)
    {
        long value = index;
        long seen = Volatile.Read(ref slot);

        while (value > seen)
        {
            long prior = Interlocked.CompareExchange(ref slot, value, seen);

            if (prior == seen)
                return;

            seen = prior;
        }
    }

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Arm()
    {
        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Report();
        Control();
    }

    // Drives the REAL path -- three constructors and one token read -- rather than calling the
    // counters directly. A control that pokes the counter proves the counter works and says nothing
    // about whether the hooks are in place, which is the only thing that can actually be missing.
    private static void Control()
    {
        int[] backing = new int[ControlIndex + 1];

        // Arm 1: the slice constructor. Low is 0 here, so the absolute index IS ControlIndex.
        var fromSlice = new ElemRefBox<int>(new slice<int>(backing), ControlIndex);

        // Arm 2: the array<T> constructor.
        var fromArray = new ElemRefBox<int>(new array<int>(ControlIndex + 1), ControlIndex - 1);

        // Arm 3: the IArray constructor, reached by boxing the header the concrete overloads exist
        // to avoid boxing.
        var fromForeign = new ElemRefBox<int>(
            (IArray)new array<int>(ControlIndex + 1), ControlIndex - 2);

        // One token read, on the managed branch (a managed slice's nativeElementIdentity is 0, so
        // this takes the CanonicalPair path the tag would apply to).
        _ = fromSlice.PointerOrderToken;

        long ctorCalls = Volatile.Read(ref s_ctorCalls);
        long tokenReads = Volatile.Read(ref s_tokenReads);
        long maxCtor = Volatile.Read(ref s_maxCtor);
        long maxToken = Volatile.Read(ref s_maxToken);

        GC.KeepAlive(fromArray);
        GC.KeepAlive(fromForeign);

        bool ok = ctorCalls >= 3 && tokenReads >= 1 &&
                  maxCtor == ControlIndex && maxToken == ControlIndex;

        if (ok)
        {
            Console.Error.WriteLine(
                $"ELEMPROBE-CONTROL fired ctor={maxCtor} token={maxToken} " +
                $"(ctor_calls={ctorCalls}, token_reads={tokenReads})");
        }
        else
        {
            Console.Error.WriteLine(
                $"ELEMPROBE-CONTROL FAILED ctor={maxCtor} token={maxToken} " +
                $"ctor_calls={ctorCalls} token_reads={tokenReads} expected={ControlIndex} " +
                "-- the hooks in ElemRefBox are missing or partial, so THIS RUN HAS NO NUMBERS.");
        }

        // Zero AFTER the control, so the measurement starts from nothing the control put there.
        Volatile.Write(ref s_maxCtor, 0);
        Volatile.Write(ref s_maxToken, 0);
        Volatile.Write(ref s_ctorCalls, 0);
        Volatile.Write(ref s_tokenReads, 0);
    }

    // Public so a host can emit the line explicitly when it does not exit normally.
    public static void Report()
    {
        Console.Error.WriteLine(
            $"ELEMPROBE max_ctor={Volatile.Read(ref s_maxCtor)} " +
            $"max_token={Volatile.Read(ref s_maxToken)} " +
            $"ctor_calls={Volatile.Read(ref s_ctorCalls)} " +
            $"token_reads={Volatile.Read(ref s_tokenReads)}");
    }
}
