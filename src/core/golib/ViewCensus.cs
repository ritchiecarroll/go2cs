// Copyright 2026 The go2cs Authors. All rights reserved.
//
// THE VIEW-CACHE MISS CENSUS (board oo; COORD 20261120T155751Z step 1; C1's scout 20261120T152425Z). A SCRATCH
// INSTRUMENT: it rides a scratch ref for one measurement and is not proposed for master.
//
// The question. A field view (`&p.f`, and every pointer-receiver call on a field) is cached per (box, accessor): on
// SlottedStandardBox, FieldRefBox and ElemRefBox in a slot the inlined hit path reads, on every other kind in a
// ConditionalWeakTable (ж<T>.viewsHead). A weak-table box is cheap only if it LIVES. The scout's claim is that
// modernc's sqlite reaches struct fields through `(*T)(unsafe.Pointer(p))`, which mints a fresh NativeBox per
// conversion (GoTypeOps.NewNativeBox), so its first `of()` is a weak-table CREATE every time; and that the native
// field hop (FieldRefBox.NativeFieldBox) mints another NativeBox on every call, cached root or not. This counts:
//
//   MINT          every NativeBox constructed, by T; nil (address 0) apart. Exact.
//   DISTINCT      the distinct non-zero addresses minted per T, up to a cap: mints over distinct is how often one
//                 address is re-converted, which is what interning per (T, address) would save. Exact below the cap.
//   SITE          every Nth mint (GO2CS_VIEW_CENSUS_SAMPLE, default 256, 0 off) names its golib door and its first
//                 caller outside golib, from a stack trace. SAMPLED: multiply by N for an estimate.
//   SLOW          every call that reaches the cache's slow path (viewOfSlow, and the untyped viewOf, which has no
//                 inlined hit path), by the box's kind, where its list lives, and what the lookup found:
//                     where=slot       a slotted kind (its slot was read and missed, or an older site's delegate)
//                     where=weak-new   a weak-table kind with NO entry yet: the lookup CREATES one (G's L3)
//                     where=weak-live  a weak-table kind whose entry exists: the box lived to be asked again
//                     where=uncached   the nil box, or the cache disabled: no lookup at all
//                 and outcome=found (an existing view returned) or outcome=minted (a FieldRefBox was minted).
//   NATIVE-FIELD  every of() whose answer was replaced by a native box at the field's Go offset, by the root's kind.
//   DOOR          (re-run, census-2) every mint through one of golib's 15 construction sites, by that site, EXACT: each
//                 site wraps its `new NativeBox<T>(...)` in ViewCensus.Door, so an inlined frame cannot hide it (the
//                 first run's sampled door read `?` for 55% of rows). MINT minus the DOOR rows is the mints made
//                 outside golib (go.unsafe's unsafe.Add, unsafe.cs:1608), reported as DOOR-UNLABELED.
//   BARE          (re-run) per T, the mints that carry no provenance: no pin, no retained source, not an order
//                 token's carrier. Only these could be interned per (T, address) without breaking what a box keeps
//                 (Change B's population; P3's MUST 1 on the pointer-boundary design).
//   DISTINCT is now capped PER TYPE (GO2CS_VIEW_CENSUS_DISTINCT_CAP, default 16,000,000), not over all types: the first
//   run's global 1,000,000 cap left byte's and uintptr's ratios undecided.
//
// The per-access signature the scout predicts: for NativeBox, SLOW where=weak-new outcome=minted ~= SLOW calls, and
// NATIVE-FIELD ~= the of() calls on native roots; for the slotted kinds, minted ~= distinct (box, field) pairs.
//
// OFF BY DEFAULT AND FREE WHEN OFF: every hook tests `ViewCensus.Enabled`, a static readonly bool read once from
// GO2CS_VIEW_CENSUS, which the JIT folds at tier 1.
//
// THE FILE. GO2CS_VIEW_CENSUS_FILE (a {pid} in it is replaced; without one the process id is added before the
// extension), else view-census-<pid>.txt in the temporary directory. One file PER PROCESS. The file holds ONE block,
// the latest snapshot, written to a temporary sibling and moved over the file, so it is never half-written. Written
// at module init (START: a zero is a reading, and no file means the census never armed), when a MINT or SLOW row is
// new, every 4,000,000 events, and at exit (FINAL). A run's count is the sum over FILES of each file's block. Only a
// FINAL block asserts that the rows reconcile with the counters.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

/// <summary>The view-cache miss census (scratch instrument; see the file header).</summary>
internal static class ViewCensus
{
    // Read once. A census that can be switched on mid-run cannot be reconciled.
    internal static readonly bool Enabled =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GO2CS_VIEW_CENSUS"));

    /// <summary>The tally the hooks feed. A control builds its own (<see cref="ViewCensusTally"/>) and never this one.</summary>
    internal static readonly ViewCensusTally Live = new(SampleFromEnvironment(), DistinctCapFromEnvironment());

    internal const string WhereSlot = "slot";
    internal const string WhereWeakNew = "weak-new";
    internal const string WhereWeakLive = "weak-live";
    internal const string WhereUncached = "uncached";

    private static int SampleFromEnvironment() =>
        int.TryParse(Environment.GetEnvironmentVariable("GO2CS_VIEW_CENSUS_SAMPLE"), out int n) && n >= 0 ? n : 256;

    private static int DistinctCapFromEnvironment() =>
        int.TryParse(Environment.GetEnvironmentVariable("GO2CS_VIEW_CENSUS_DISTINCT_CAP"), out int n) && n > 0 ? n : 16_000_000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Mint(Type pointee, nuint address, bool pinned, bool retained, bool aliasesToken) =>
        Live.Mint(pointee, address, sampleStack: true, pinned, retained, aliasesToken);

    /// <summary>
    /// Counts one mint at a named golib construction site and returns the box unchanged. A site wraps its
    /// <c>new NativeBox&lt;T&gt;(...)</c> in it; with the census off it is the box and a folded test.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TBox Door<TBox>(TBox box, string door) where TBox : class
    {
        if (Enabled)
            Live.Door(door);

        return box;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Slow(Type box, string where, bool found) => Live.Slow(box, where, found);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void NativeField(Type root) => Live.NativeField(root);

    /// <summary>The path the START block was written to, or null when none was (the census is off).</summary>
    internal static string? StartBlockPath { get; private set; }

    internal static string OutputPath()
    {
        string pid = Environment.ProcessId.ToString();

        if (Environment.GetEnvironmentVariable("GO2CS_VIEW_CENSUS_FILE") is not { Length: > 0 } named)
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"view-census-{pid}.txt");

        if (named.Contains("{pid}", StringComparison.Ordinal))
            return named.Replace("{pid}", pid);

        string? dir = System.IO.Path.GetDirectoryName(named);
        string perProcess = $"{System.IO.Path.GetFileNameWithoutExtension(named)}-{pid}{System.IO.Path.GetExtension(named)}";

        return string.IsNullOrEmpty(dir) ? perProcess : System.IO.Path.Combine(dir, perProcess);
    }

    [ModuleInitializer]
    internal static void Arm()
    {
        if (!Enabled)
            return;

        string path = OutputPath();
        Live.Path = path;
        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Live.Write(final: true);
        Live.Write(final: false, start: true);
        StartBlockPath = path;
    }
}

/// <summary>One census's counters and rows. <see cref="ViewCensus.Live"/> is the one the hooks feed.</summary>
internal sealed class ViewCensusTally
{
    private const long FlushEvery = 4_000_000;

    private readonly int m_sample;
    private readonly int m_distinctCap;

    private long m_events;
    private long m_mints;
    private long m_nilMints;
    private long m_slow;
    private long m_nativeFields;
    private long m_sampled;
    private long m_distinct;
    private long m_bare;
    private long m_doors;
    private int m_flushing;

    private readonly ConcurrentDictionary<Type, StrongBox<long>> m_mintRows = new();
    // Per T: the distinct non-zero addresses minted, up to m_distinctCap each (a HashSet under its own lock; 28 bytes or
    // so an entry), and whether the cap was reached.
    private sealed class Addresses
    {
        internal readonly HashSet<nuint> Seen = [];
        internal bool Capped;
    }

    private readonly ConcurrentDictionary<Type, Addresses> m_addresses = new();
    private readonly ConcurrentDictionary<Type, StrongBox<long>> m_bareRows = new();
    private readonly ConcurrentDictionary<string, StrongBox<long>> m_doorRows = new();
    private readonly ConcurrentDictionary<string, StrongBox<long>> m_siteRows = new();
    private readonly ConcurrentDictionary<(Type, string, bool), StrongBox<long>> m_slowRows = new();
    private readonly ConcurrentDictionary<Type, StrongBox<long>> m_nativeFieldRows = new();

    internal ViewCensusTally(int sample, int distinctCap = 16_000_000)
    {
        m_sample = sample;
        m_distinctCap = distinctCap;
    }

    /// <summary>Where this tally's block is written; null writes nothing.</summary>
    internal string? Path { get; set; }

    // Never inlined: the sampled stack trace skips exactly this frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void Mint(Type pointee, nuint address, bool sampleStack, bool pinned = false, bool retained = false, bool aliasesToken = false)
    {
        long n = Interlocked.Increment(ref m_mints);
        bool newRow = Bump(m_mintRows, pointee);

        if (!pinned && !retained && !aliasesToken)
        {
            Interlocked.Increment(ref m_bare);
            Bump(m_bareRows, pointee);
        }

        if (address == 0)
        {
            Interlocked.Increment(ref m_nilMints);
        }
        else
        {
            Addresses seen = m_addresses.GetOrAdd(pointee, static _ => new Addresses());

            lock (seen)
            {
                if (seen.Seen.Count >= m_distinctCap)
                    seen.Capped = true;
                else if (seen.Seen.Add(address))
                    Interlocked.Increment(ref m_distinct);
            }
        }

        if (sampleStack && m_sample > 0 && n % m_sample == 0)
        {
            Interlocked.Increment(ref m_sampled);
            Bump(m_siteRows, SiteOf(new StackTrace(1, false)));
        }

        Event(newRow);
    }

    internal void Slow(Type box, string where, bool found)
    {
        Interlocked.Increment(ref m_slow);
        Event(Bump(m_slowRows, (box, where, found)));
    }

    internal void NativeField(Type root)
    {
        Interlocked.Increment(ref m_nativeFields);
        Event(Bump(m_nativeFieldRows, root));
    }

    // After the mint it names (the constructor counts first), so at every instant DOOR <= MINT.
    internal void Door(string door)
    {
        Interlocked.Increment(ref m_doors);
        Event(Bump(m_doorRows, door));
    }

    /// <summary>The DOOR count for one construction site, and the BARE count for one pointee type.</summary>
    internal long DoorRow(string door) =>
        m_doorRows.TryGetValue(door, out StrongBox<long>? n) ? Interlocked.Read(ref n.Value) : 0;

    internal long BareRow(Type pointee) =>
        m_bareRows.TryGetValue(pointee, out StrongBox<long>? n) ? Interlocked.Read(ref n.Value) : 0;

    /// <summary>The distinct addresses seen for one pointee type, and whether its cap was reached.</summary>
    internal (long distinct, bool capped) DistinctRow(Type pointee)
    {
        if (!m_addresses.TryGetValue(pointee, out Addresses? seen))
            return (0, false);

        lock (seen)
            return (seen.Seen.Count, seen.Capped);
    }

    /// <summary>The counters, for a control: an assertion is that a counter MOVED across a call.</summary>
    internal (long mints, long nilMints, long slow, long nativeFields, long distinct) Snapshot() =>
        (Interlocked.Read(ref m_mints), Interlocked.Read(ref m_nilMints), Interlocked.Read(ref m_slow),
         Interlocked.Read(ref m_nativeFields), Interlocked.Read(ref m_distinct));

    /// <summary>The SLOW count for one (box kind, where, found) row.</summary>
    internal long SlowRow(Type box, string where, bool found) =>
        m_slowRows.TryGetValue((box, where, found), out StrongBox<long>? n) ? Interlocked.Read(ref n.Value) : 0;

    /// <summary>The MINT count for one pointee type.</summary>
    internal long MintRow(Type pointee) =>
        m_mintRows.TryGetValue(pointee, out StrongBox<long>? n) ? Interlocked.Read(ref n.Value) : 0;

    /// <summary>The NATIVE-FIELD count for one root kind.</summary>
    internal long NativeFieldRow(Type root) =>
        m_nativeFieldRows.TryGetValue(root, out StrongBox<long>? n) ? Interlocked.Read(ref n.Value) : 0;

    private static bool Bump<TKey>(ConcurrentDictionary<TKey, StrongBox<long>> rows, TKey key) where TKey : notnull
    {
        if (rows.TryGetValue(key, out StrongBox<long>? n))
        {
            Interlocked.Increment(ref n.Value);
            return false;
        }

        StrongBox<long> created = new(0);
        StrongBox<long> row = rows.GetOrAdd(key, created);
        Interlocked.Increment(ref row.Value);

        return ReferenceEquals(row, created);
    }

    private void Event(bool newRow)
    {
        long n = Interlocked.Increment(ref m_events);

        if (Path is not null && (newRow || n % FlushEvery == 0))
            Write(final: false);
    }

    // The golib door (the first golib frame that is not this census and not NativeBox's constructor) and the first
    // frame outside golib. Names only, through DiagnosticMethodInfo, as RuntimePanicCheck reads frames: a trimmed or
    // Native AOT program keeps a frame's method, type and assembly names, where a MethodBase needs metadata (IL2026).
    private static string SiteOf(StackTrace trace)
    {
        string? golib = typeof(ViewCensus).Assembly.FullName;
        string door = "?";
        string caller = "?";

        for (int i = 0; i < trace.FrameCount; i++)
        {
            if (trace.GetFrame(i) is not { } frame || DiagnosticMethodInfo.Create(frame) is not { DeclaringTypeName: { } typeName } method)
                continue;

            if (typeName.StartsWith("go.ViewCensus", StringComparison.Ordinal))
                continue;

            if (method.DeclaringAssemblyName == golib)
            {
                if (door == "?" && !(method.Name == ".ctor" && typeName.StartsWith("go.NativeBox`1", StringComparison.Ordinal)))
                    door = $"{ShortTypeName(typeName)}.{method.Name}";

                continue;
            }

            caller = $"{ShortTypeName(typeName)}.{method.Name}";
            break;
        }

        return $"door={door}  caller={caller}";
    }

    // A generic type name without its bracketed argument list: the door is about the method, not its instantiation.
    private static string ShortTypeName(string typeName)
    {
        int bracket = typeName.IndexOf('[');

        return bracket > 0 ? typeName[..bracket] : typeName;
    }

    internal static string NameOf(Type t)
    {
        if (!t.IsGenericType)
            return t.FullName ?? t.Name;

        string name = t.Name;
        int tick = name.IndexOf('`');

        if (tick >= 0)
            name = name[..tick];

        return (t.Namespace is { Length: > 0 } ns ? ns + "." : "") + name + "<" + string.Join(",", Array.ConvertAll(t.GetGenericArguments(), NameOf)) + ">";
    }

    // A generic box kind by its definition alone (NativeBox, not NativeBox<sqlite3.Mem>): the SLOW and NATIVE-FIELD
    // rows are about the KIND. The pointee is in the KIND-T rows beside them.
    internal static string KindOf(Type t)
    {
        string name = t.Name;
        int tick = name.IndexOf('`');

        return tick >= 0 ? name[..tick] : name;
    }

    /// <summary>Writes this tally's one block to <see cref="Path"/>, replacing the last.</summary>
    internal void Write(bool final, bool start = false)
    {
        if (Path is not { } path)
            return;

        // One writer at a time, and a second arrival skips -- except the FINAL write, which waits its turn.
        if (!final && Interlocked.Exchange(ref m_flushing, 1) == 1)
            return;

        if (final)
        {
            SpinWait spin = default;

            while (Interlocked.Exchange(ref m_flushing, 1) == 1)
                spin.SpinOnce();
        }

        try
        {
            string temp = path + ".tmp";
            System.IO.File.WriteAllLines(temp, Lines(final, start));
            System.IO.File.Move(temp, path, overwrite: true);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"VCENSUS-UNREPORTED could not write {path}: {e.GetType().Name}");
        }
        finally
        {
            Interlocked.Exchange(ref m_flushing, 0);
        }
    }

    internal List<string> Lines(bool final, bool start = false)
    {
        (long mints, long nilMints, long slow, long nativeFields, long distinct) = Snapshot();
        long sampled = Interlocked.Read(ref m_sampled);
        long bare = Interlocked.Read(ref m_bare), doors = Interlocked.Read(ref m_doors);

        List<string> lines =
        [
            $"{(final ? "VCENSUS" : "VCENSUS-PARTIAL")} mints={mints} nil-mints={nilMints} distinct={distinct} bare={bare} doors={doors} door-unlabeled={mints - doors} slow={slow} native-fields={nativeFields} sampled={sampled} sample-every={m_sample} distinct-cap-per-type={m_distinctCap}",
            $"VCENSUS-BLOCK pid={Environment.ProcessId} entry={System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "?"} utc={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}",
            "VCENSUS-FOLD the file holds ONE block, the latest; a run's count is the sum of the blocks across FILES"
        ];

        if (start)
            lines.Add("VCENSUS-START armed at module init: the zeros above are a MEASURED zero; NO FILE means the census never armed");

        List<string> rows = [];
        long sumMints = 0, sumSlow = 0, sumNative = 0, sumSites = 0, sumDistinct = 0, sumBare = 0, sumDoors = 0;

        foreach (KeyValuePair<Type, StrongBox<long>> row in m_mintRows)
        {
            long n = Interlocked.Read(ref row.Value.Value);
            (long d, bool capped) = DistinctRow(row.Key);
            long b = BareRow(row.Key);
            sumMints += n;
            sumDistinct += d;
            sumBare += b;
            rows.Add($"VCENSUS-MINT {n,12}  distinct={d,10}{(capped ? "(CAPPED)" : "")}  bare={b,12}  T={NameOf(row.Key)}");
        }

        foreach (KeyValuePair<string, StrongBox<long>> row in m_doorRows)
        {
            long n = Interlocked.Read(ref row.Value.Value);
            sumDoors += n;
            rows.Add($"VCENSUS-DOOR {n,12}  {row.Key}");
        }

        rows.Add($"VCENSUS-DOOR-UNLABELED {mints - doors,12}  mints made outside golib's 15 sites (go.unsafe's unsafe.Add, unsafe.cs:1608)");

        foreach (KeyValuePair<(Type box, string where, bool found), StrongBox<long>> row in m_slowRows)
        {
            long n = Interlocked.Read(ref row.Value.Value);
            sumSlow += n;
            rows.Add($"VCENSUS-SLOW {n,12}  kind={KindOf(row.Key.box)}  where={row.Key.where}  outcome={(row.Key.found ? "found" : "minted")}  box={NameOf(row.Key.box)}");
        }

        foreach (KeyValuePair<Type, StrongBox<long>> row in m_nativeFieldRows)
        {
            long n = Interlocked.Read(ref row.Value.Value);
            sumNative += n;
            rows.Add($"VCENSUS-NATIVE-FIELD {n,12}  root-kind={KindOf(row.Key)}  root={NameOf(row.Key)}");
        }

        foreach (KeyValuePair<string, StrongBox<long>> row in m_siteRows)
        {
            long n = Interlocked.Read(ref row.Value.Value);
            sumSites += n;
            rows.Add($"VCENSUS-SITE {n,12}  {row.Key}");
        }

        bool reconciles = sumMints == mints && sumSlow == slow && sumNative == nativeFields && sumSites == sampled &&
                          sumDistinct == distinct && sumBare == bare && sumDoors == doors && doors <= mints;
        string sums = $"mints={sumMints} slow={sumSlow} native-fields={sumNative} sampled={sumSites} distinct={sumDistinct} bare={sumBare} doors={sumDoors}";

        lines.Add(reconciles
            ? $"VCENSUS-RECONCILES rows sum to {sums}"
            : final
                ? $"VCENSUS-BROKEN rows sum to {sums} against the counters above"
                : $"VCENSUS-PARTIAL-SKEW rows sum to {sums}: threads between a counter and its row; a FINAL block must reconcile");

        rows.Sort(StringComparer.Ordinal);
        lines.AddRange(rows);

        return lines;
    }
}
