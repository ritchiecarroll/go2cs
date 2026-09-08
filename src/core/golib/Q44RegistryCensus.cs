// Copyright 2026 The go2cs Authors. All rights reserved.
//
// THE Q44 §10.5 REGISTRY CENSUS -- dynamic, at the registry, because §10.5 rules out the alternatives
// by construction: the operators are reached through IMPLICIT conversions (every `unsafe.Pointer(&x)`
// in the corpus), so a call-site grep cannot find them, and frames inline, so a stack walk cannot
// attribute them. Attribution therefore rides on the CALL SITE classifying its own values -- the
// caller-supplied-tag discipline -- and this file is only the counters.
//
// WHAT IT COUNTS. The four arms of DESIGN-managed-pointer-token.md §10.3, decided at ж.cs's
// `uintptr -> ж<T>` operator from values the operator already has:
//
//   Resolve(n) is ж<T>                        ARM 1  same pointee type            (today's arm; pprof's case)
//   Resolve(n) non-null, NOT ж<T>             ARM 2  different pointee type       (the write's case)
//        ... and n == box.PointerOrderToken   ARM 2a offset 0 -- a prefix pun, expressible as an alias
//        ... and n != box.PointerOrderToken   ARM 2b offset != 0 via the pinned-provenance route
//   Resolve(n) null, IsTokenArithmetic(n)     ARM 3  inside a live block, not the token -- refuses today
//   Resolve(n) null, not token-arithmetic     ARM 4  a real address
//
// Mutually exclusive and exhaustive: every conversion lands in exactly one, which is what makes the
// totals reconcilable against the resolve count rather than merely suggestive.
//
// ⚠ ARM 2 IS NOT MERELY "NEW WORK". `IsTokenArithmetic` masks the low 32 bits and requires
// `allocationBase != number`, so it is FALSE when n IS the base. Arm 2 therefore reaches neither
// arm 3's refusal nor arm 1's alias -- it falls through to `new NativeBox<T>(n)`, a native box over a
// number that is not an address. Counting arm 2 is counting how often that happens today.
//
// OFF BY DEFAULT AND FREE WHEN OFF: one static bool read per conversion, and the counters are only
// touched when it is set. It is enabled by the environment variable named below so a census run needs
// no rebuild of anything but this file's consumers, and so no gate ever pays for it.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace go;

/// <summary>
/// Counters for the Q44 §10.5 registry census. Off unless <c>GO2CS_Q44_CENSUS</c> is set.
/// </summary>
internal static class Q44RegistryCensus
{
    // Read once. A census that can be switched on mid-run would make its own totals unreconcilable.
    internal static readonly bool Enabled =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GO2CS_Q44_CENSUS"));

    private static long s_mints;
    private static long s_conversions;
    private static long s_arm1;
    private static long s_arm2a;
    private static long s_arm2b;
    private static long s_arm3;
    private static long s_arm4;
    private static int s_flushing;

    // Per-arm TYPE PAIRS, because §10.5 asks "whether the pointee type matched" and a bare count
    // cannot answer falsifier (b) -- which needs to know WHICH types met at offset 0.
    private static readonly ConcurrentDictionary<string, long> s_arm2Pairs = new();

    // Conservative and CLOSED over fields: a struct counts as reference-bearing if it, or anything it
    // contains, is a managed reference. `RuntimeHelpers.IsReferenceOrContainsReferences<T>` answers
    // this exactly but needs a generic parameter, and here the type is only known as a `Type` -- so
    // the walk is explicit, and it fails REFERENCE-WARDS on anything it cannot decide, because the
    // consequence of a wrong "blittable" is corruption rather than a wrong answer.
    private static bool PointeeContainsReferences(Type t)
    {
        if (!t.IsValueType)
            return true;

        if (t.IsPrimitive || t.IsEnum || t.IsPointer)
            return false;

        foreach (var f in t.GetFields(System.Reflection.BindingFlags.Instance |
                                     System.Reflection.BindingFlags.Public |
                                     System.Reflection.BindingFlags.NonPublic))
        {
            if (f.FieldType == t)
                continue;

            if (PointeeContainsReferences(f.FieldType))
                return true;
        }

        return false;
    }

    internal static void Mint() => Interlocked.Increment(ref s_mints);

    // ⚠ A ResolveCalls COUNTER STOOD HERE AND IS GONE (2026-09-08), with its measurement kept so
    // nobody rebuilds it. It was added to make neutrality provable in a unit test, after the two
    // obvious formulations were shown not to discriminate: "a conversion must not change the
    // registered count" FAILS ON CORRECT CODE, because the one resolve the operator legitimately
    // performs evicts a dead weak entry whether the census is on or off; and counting evictions
    // cannot see a second call either, since two resolves of one token cannot evict twice. Counting
    // Resolve ENTRIES did discriminate -- and cost an Interlocked increment on a path taken 264,167
    // times in a single roster row, which is precisely the kind of work that makes an instrument
    // non-neutral. The lesson is the general one: an instrument built to prove a property of the
    // hot path, ON the hot path, is a perturbation wearing a proof's clothes. The gate is the
    // banked `os` row.

    // ⚠ THE FLUSH COMES AFTER THE ARM COUNTER, and the control is what taught it. Flushing on the
    // conversion increment alone made every partial block report Q44CENSUS-BROKEN -- "arms sum to
    // 249999 but conversions is 250000" -- because the flushing thread had counted its conversion and
    // not yet its arm. The census's own not-exhaustive alarm, fired by the instrument on itself.
    internal static void Arm1() { long n = Interlocked.Increment(ref s_conversions); Interlocked.Increment(ref s_arm1); MaybeFlush(n); }
    internal static void Arm3() { long n = Interlocked.Increment(ref s_conversions); Interlocked.Increment(ref s_arm3); MaybeFlush(n); }
    internal static void Arm4() { long n = Interlocked.Increment(ref s_conversions); Interlocked.Increment(ref s_arm4); MaybeFlush(n); }

    // How often a partial block is written. Chosen large because the flush is I/O on the census-ON
    // path and this instrument has already been non-neutral twice; at the corpus's biggest measured
    // row (3.9 M conversions) it is sixteen writes, and at a small row it is one -- the FIRST.
    private const long FlushEvery = 250_000;

    /// <summary>
    /// Writes a PARTIAL block at the first conversion and every <see cref="FlushEvery"/> after it, so
    /// a host that DIES before its exit hook still leaves a reading.
    /// </summary>
    /// <remarks>
    /// ⚠ COORD ruling 3 (82c60cec4), from `reflect` reading "0 files, no census output": the census
    /// reported only from a ProcessExit hook, so a host that dies before exit writes NOTHING, which is
    /// INDISTINGUISHABLE from a row that performed no conversions. That is this tree's own
    /// unrun-instrument falsifier wearing a result's clothes, and `reflect` had to be recorded
    /// UNMEASURED rather than 0 because of it.
    ///
    /// ⚠ THE NEUTRALITY CONSTRAINT SHAPED THE DESIGN, because this instrument broke neutrality twice
    /// already -- once through a second Resolve, once through a counter added to prove it did not.
    /// So the hot path gains NO ATOMIC OPERATION: the conversion counter was ALREADY an
    /// Interlocked.Increment, and its return value is now read instead of discarded. What is added
    /// per conversion is one comparison against a constant. The I/O itself is off the per-conversion
    /// path by a factor of 250,000, and one thread flushes at a time -- a second thread arriving
    /// mid-flush skips rather than queues, because a census must never become a lock the program
    /// under test waits on.
    /// </remarks>
    private static void MaybeFlush(long conversions)
    {
        if (conversions != 1 && conversions % FlushEvery != 0)
            return;

        // One flusher at a time; a concurrent arrival skips. Losing a partial is free -- the next
        // one is 250,000 conversions away and the exit hook writes the final block regardless.
        if (Interlocked.Exchange(ref s_flushing, 1) == 1)
            return;

        try
        {
            DumpTo(OutputPath, partial: true);
        }
        catch
        {
            // A census must not take the program down. DumpTo already reports its own write failures.
        }
        finally
        {
            Interlocked.Exchange(ref s_flushing, 0);
        }
    }

    /// <summary>
    /// Arm 2: the resolve hit a box of a DIFFERENT pointee type. <paramref name="atOffsetZero"/>
    /// separates 2a (n IS the box's order token -- Go's prefix pun) from 2b (n is not, so the entry
    /// resolved through the pinned-provenance route instead).
    /// </summary>
    internal static void Arm2(Type requested, object box, bool atOffsetZero)
    {
        long conversion = Interlocked.Increment(ref s_conversions);

        if (atOffsetZero)
            Interlocked.Increment(ref s_arm2a);
        else
            Interlocked.Increment(ref s_arm2b);

        MaybeFlush(conversion);

        // ⚠ THE POINTEE TYPE, not just the box's class. `box.GetType().Name` answers `StandardBox`1`
        // for every box in the corpus -- a name that cannot distinguish one pointee from another, and
        // the 2a remedy's soundness predicate is a question ABOUT THE POINTEE (is the storage
        // reference-bearing? does T fit inside it?). Recording the class alone would have produced a
        // corpus table that looks complete and cannot answer the question it was collected for.
        string resolvedName = box.GetType() is { IsGenericType: true } g
            ? $"{g.Name[..g.Name.IndexOf('`')]}<{string.Join(',', Array.ConvertAll(g.GetGenericArguments(), static t => t.Name))}>"
            : box.GetType().Name;

        // Whether the POINTEE storage carries a managed reference decides whether an offset-0 alias is
        // even expressible: Unsafe.As over mismatched GC layout is memory corruption, not a wrong
        // value. Recorded per site so the remedy can be sized against the sound and unsound halves
        // separately rather than against arm 2a as a lump.
        Type pointee = box.GetType() is { IsGenericType: true } gp ? gp.GetGenericArguments()[0] : box.GetType();
        bool pointeeHasRefs = !pointee.IsValueType || PointeeContainsReferences(pointee);
        bool requestedHasRefs = !requested.IsValueType || PointeeContainsReferences(requested);

        string pair = $"{(atOffsetZero ? "2a" : "2b")}  requested={requested.Name}" +
                      $"{(requestedHasRefs ? "(refs)" : "(blittable)")}  resolved={resolvedName}" +
                      $"{(pointeeHasRefs ? "(refs)" : "(blittable)")}" +
                      $"  alias-expressible={(atOffsetZero && !requestedHasRefs && !pointeeHasRefs ? "YES" : "NO")}";
        s_arm2Pairs.AddOrUpdate(pair, 1, static (_, n) => n + 1);
    }

    /// <summary>
    /// A readable snapshot, so the census's own positive control can assert that each counter MOVED
    /// rather than merely that the run produced a number. Without this the control could only compare
    /// the final totals against an expectation, which is exactly the shape that cannot tell a wired
    /// counter from an unwired one.
    /// </summary>
    internal static (long mints, long conversions, long arm1, long arm2a, long arm2b, long arm3, long arm4) Snapshot()
    {
        return (Interlocked.Read(ref s_mints), Interlocked.Read(ref s_conversions),
                Interlocked.Read(ref s_arm1), Interlocked.Read(ref s_arm2a),
                Interlocked.Read(ref s_arm2b), Interlocked.Read(ref s_arm3),
                Interlocked.Read(ref s_arm4));
    }

    /// <summary>
    /// The file the census reports into. ⚠ A FILE and not stderr, and that is measured rather than
    /// preferred: the first version wrote to stderr from a ProcessExit hook, every counter fired
    /// under the control, and the dump reached NO log -- the MSTest host swallows it. The counters
    /// were wired and the instrument could not report, which is the same defect as a counter that
    /// never moves and is harder to see, because the arms all looked healthy.
    /// </summary>
    internal static string OutputPath => ResolveOutputPath();

    // Set when a caller-supplied path carried no {pid} and this process therefore writes a
    // per-process file instead. Reported in the BLOCK rather than on stderr, because stderr belongs
    // to the program under test and every child inherits the census -- the reason arm 2 removed all
    // routine stderr writes in the first place. The file is the census's own channel.
    private static string s_rewrittenFrom;

    /// <summary>
    /// The census path, PER PROCESS BY CONSTRUCTION.
    /// </summary>
    /// <remarks>
    /// ⚠ A SHARED PATH SILENTLY DESTROYS BLOCKS, MEASURED 2026-09-08. Two processes writing one path:
    /// the second's first write truncated the first's entire census -- 19 blocks gone, no error, no
    /// report, and a row that reads like a small measured one when it is a destroyed one. That is this
    /// instrument's own falsifier turned on itself, and it is what `reflect` was recorded UNMEASURED
    /// against rather than as conversions=1.
    ///
    /// The fix is STRUCTURAL rather than a rule about who may truncate. A timestamp heuristic was
    /// built and DISCARDED with its measurements: comparing the file's last-write time to this
    /// process's start behaved correctly for two sequential rows AND for two concurrent continuous
    /// writers, but its verdict depends on how often the OTHER process happens to write -- a first
    /// writer that flushes once and goes quiet is still truncated by a later starter. Three
    /// instruments in a row failed to exercise that ordering, which is the signal that the property
    /// depended on write CADENCE; a correctness rule that does is not one to ship in an instrument
    /// whose whole job is not to lose data quietly.
    ///
    /// So a path without {pid} gets the pid inserted, and no ordering or cadence can lose a block.
    /// The reader (docs/phase4/probes/c2-census-read) already folds LAST BLOCK PER FILE summed across
    /// FILES, which is exactly this shape.
    /// </remarks>
    private static string ResolveOutputPath()
    {
        string pid = Environment.ProcessId.ToString();

        if (Environment.GetEnvironmentVariable("GO2CS_Q44_CENSUS_FILE") is not { Length: > 0 } named)
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"q44-census-{pid}.txt");

        if (named.Contains("{pid}", StringComparison.Ordinal))
            return named.Replace("{pid}", pid);

        string dir = System.IO.Path.GetDirectoryName(named);
        string stem = System.IO.Path.GetFileNameWithoutExtension(named);
        string ext = System.IO.Path.GetExtension(named);
        string perProcess = $"{stem}-{pid}{ext}";
        s_rewrittenFrom = named;
        return string.IsNullOrEmpty(dir) ? perProcess : System.IO.Path.Combine(dir, perProcess);
    }

    /// <summary>
    /// Writes the census to <see cref="OutputPath"/>. Called from a process-exit hook AND callable
    /// directly, because whether that hook runs under a given test host is not a safe assumption.
    /// </summary>
    /// <remarks>
    /// ⚠ THIS BLOCK USED TO SAY the census also went to stderr "as a secondary", and that a path
    /// without <c>{pid}</c> left "the last row's block, cleanly, never two summed". BOTH ARE NOW
    /// FALSE and are corrected here rather than left to read as the design. Arm 2 removed every
    /// routine stderr write (stderr belongs to the program under test, and children inherit the
    /// census); and a path without <c>{pid}</c> no longer resolves to a shared file at all -- see
    /// <see cref="ResolveOutputPath"/>, which makes it per-process, because a shared path was
    /// MEASURED to destroy a whole process's census silently rather than to leave the last row's
    /// block cleanly.
    ///
    /// What holds now: the path is per-process by construction, so the first write in a process
    /// truncates its OWN file and later writes append to it. Rows stay apart because processes do.
    /// The header line names the process and the entry assembly, so a block is attributable, and a
    /// reader folds LAST BLOCK PER FILE then sums across FILES.
    /// </remarks>
    internal static void Dump() => DumpTo(OutputPath, partial: false);

    // Which paths this process has already written, so the FIRST write truncates and the rest append.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> s_written = new();

    /// <summary>
    /// Dumps to an explicit path, so a caller that must not disturb a live census run (the control)
    /// can report into its own file.
    /// </summary>
    internal static void DumpTo(string path) => DumpTo(path, partial: false);

    private static void DumpTo(string path, bool partial)
    {
        long c = Interlocked.Read(ref s_conversions);
        long a1 = Interlocked.Read(ref s_arm1), a2a = Interlocked.Read(ref s_arm2a);
        long a2b = Interlocked.Read(ref s_arm2b), a3 = Interlocked.Read(ref s_arm3), a4 = Interlocked.Read(ref s_arm4);

        // The reconciliation is computed rather than assumed: the arms must sum to the conversions,
        // or the classification is not exhaustive and no count below it means anything.
        long sum = a1 + a2a + a2b + a3 + a4;

        var lines = new System.Collections.Generic.List<string>
        {
            $"{(partial ? "Q44CENSUS-PARTIAL" : "Q44CENSUS")} mints={Interlocked.Read(ref s_mints)} conversions={c} " +
            $"arm1={a1} arm2a={a2a} arm2b={a2b} arm3={a3} arm4={a4}",

            // Attribution, and it goes AFTER the totals rather than before: an existing control
            // requires the totals line to be FIRST and greppable, and it caught this line in the
            // wrong place the first time it ran.
            $"Q44CENSUS-BLOCK pid={Environment.ProcessId} " +
            $"entry={System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "?"} " +
            $"utc={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}",

            // ⚠ THE FOLD RULE, STATED IN THE OUTPUT ITSELF. One process now writes SEVERAL blocks
            // (the partial flush) and they are CUMULATIVE SNAPSHOTS of one running total, not
            // increments -- so summing the blocks within a file DOUBLE-COUNTS it. i9 read a row 1.96x
            // high that way (c62ca28686) and was right that nothing in the output said so: the method
            // had been correct until the flush existed, and the flush changed the shape underneath it
            // silently. Keying on the FINAL block is also wrong, because a process killed before it
            // finishes leaves only a PARTIAL -- which is the case the flush exists for. The rule that
            // handles both is LAST BLOCK PER FILE, then sum across FILES, and it is printed in every
            // block so that a reader cannot arrive at the naive sum honestly.
            "Q44CENSUS-FOLD cumulative-snapshot -- the LAST block in THIS file is authoritative; " +
            "sum across FILES, never across blocks",
        };

        // Say so IN THE ARTIFACT when the configured path was made per-process, so a reader looking
        // for the name they set finds out why there are several files instead of wondering.
        if (s_rewrittenFrom is { Length: > 0 } from)
            lines.Add($"Q44CENSUS-PATH per-process: configured '{from}' carried no {{pid}}, so this " +
                      $"process wrote '{path}'; a shared path loses blocks silently");

        lines.Add(
            // ⚠ ONLY A FINAL BLOCK ASSERTS EXACT RECONCILIATION. A partial is taken while other
            // threads are mid-arm -- each has counted its conversion and not yet its arm -- so a
            // small shortfall there is the instrument being honest about a live count, not a broken
            // classification. Reporting it as BROKEN would cry wolf on every partial and teach a
            // reader to ignore the one alarm that matters. The skew is PRINTED rather than hidden,
            // because an unexplained gap in a partial is exactly what a reader must be able to check.
            sum == c
                ? $"Q44CENSUS-RECONCILES arms sum to {sum} == conversions {c}"
                : partial
                    ? $"Q44CENSUS-PARTIAL-SKEW arms sum to {sum} against conversions {c}, delta {c - sum} -- threads mid-arm at flush; a FINAL block must reconcile exactly"
                    : $"Q44CENSUS-BROKEN arms sum to {sum} but conversions is {c} -- the classification is NOT exhaustive");

        foreach (var kv in s_arm2Pairs)
            lines.Add($"Q44CENSUS-ARM2 {kv.Value,8}  {kv.Key}");

        // ⚠ NOTHING ROUTINE GOES TO stderr, and that is the whole of arm 2 (2026-09-08). These lines
        // used to be written here as a "secondary" channel. stderr is not a spare channel: it is a
        // stream the PROGRAM UNDER TEST owns, and the census arms in EVERY process that loads golib,
        // including the helper CHILDREN a package's own tests spawn and whose output they compare.
        // The environment carries the gate to those children unchanged -- childEnvWithGo2CSPath
        // copies the whole parent environment and scrubs only go2csPath -- so a child inherits the
        // census whether or not it was meant to be measured. MEASURED with a two-arm probe whose
        // child does ZERO census work: census OFF, child stderr 0 bytes; census ON, child stderr 222
        // bytes carrying "Q44CENSUS armed" at start and the whole block at exit. That is why `os`
        // flips while go/types and encoding/json do not -- os is the row whose tests spawn helpers.
        //
        // The file is the channel that survives a host which swallows stderr. A failure to write is
        // reported rather than swallowed: an instrument that cannot report must say so.
        try
        {
            if (s_written.TryAdd(path, true))
                System.IO.File.WriteAllLines(path, lines);
            else
                System.IO.File.AppendAllLines(path, lines);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Q44CENSUS-UNREPORTED could not write {path}: {e.GetType().Name}");
        }
    }

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Arm()
    {
        if (!Enabled)
            return;

        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Dump();

        // ⚠ NO "armed" LINE ON stderr. It fired in every process that loaded golib with the gate
        // set -- including a spawned helper child that does no census work at all -- and it is half
        // of the 222 bytes the probe measured. Whether the census armed is answerable from its
        // output file, which is where an instrument's report belongs.
    }
}
