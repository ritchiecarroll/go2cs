// Copyright 2026 The go2cs Authors. All rights reserved.
//
// THE NATIVE-BOX DEREFERENCE CENSUS (docs/phase4/DESIGN-interior-pointer-root.md 4.8, 10.1 and 10.5: "B0").
//
// A NativeBox<T> reads and writes a T at an address. Where T holds a managed reference, the read makes a reference out
// of whatever bytes are there and the write puts a reference where the collector does not look. A struct T whose CLR
// layout is PROVABLY not Go's is already refused by name (NativeBox<T>.ValueAddress, GoReflect.ClrLayoutMismatch).
// What is left, and what a later refusal ("B2") would close, is counted here and nowhere refused:
//
//   B2a   T holds a reference and is not a Go struct value. That is every T that is itself a reference (a func, a
//         pointer, a string, a slice, an interface, an unsafe.Pointer, a map, a channel), and an ARRAY value, whose
//         CLR form holds its elements by reference where Go has them inline. Each row names T's Go kind.
//   B2b   T is a Go struct value that holds a reference and that ClrLayoutMismatch does not refuse: its layout is
//         unproven, or every reference in it sits in a field that is zero bytes of Go memory.
//
// and beside them, as its own class and whatever T is:
//
//   UNPINNABLE   the box was derived from a managed box whose storage cannot be held still (ж<T>.StorageKind is
//                Unpinnable), so its address was read from storage nothing pins (design 10.1, third table row).
//
// WHERE. One call, in NativeBox<T>.ValueAddress, after the layout refusal and after the address is known to name
// memory: a dereference that is refused is not counted, so every count is a whole-value read or write that happened.
// A field view (NativeBox<T>.FieldRef) of a field that holds a reference is refused before it reads, and the native
// word operations (atomics on a pointer slot) read a word and not a T: neither is in the population and neither is
// counted.
//
// EVERY ROW says whether the box keeps something: `source=retained` (a box it was derived from), `source=pinned` (a pin
// over managed storage), `source=none` (an address and nothing else: C memory, or Go memory nothing holds).
//
// OFF BY DEFAULT AND FREE WHEN OFF. NativeBox<T> holds one static readonly reference per T that is null unless the
// variable below is set, and tests it once per dereference. Nothing else runs.
//
// THE FILE, AND HOW TO READ IT. The shape is Q44RegistryCensus's, for the reasons recorded there, each paid for:
// a file and not stderr (a test host swallows stderr, and stderr belongs to the program under test, whose children
// inherit the variable); one file PER PROCESS (two processes on one path destroy each other's blocks silently); a START
// block at module init (so a zero is a reading: no file at all means the census never armed); a block at the first
// dereference, at every new row and every 250,000 dereferences (so a host that dies before its exit hook still leaves
// every row). Blocks are CUMULATIVE SNAPSHOTS: the LAST block in a file is that process's count, and a run's count is
// the sum of those across FILES, never across blocks.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

/// <summary>
/// Counters for the native-box dereference census. Off unless <c>GO2CS_NATIVEBOX_CENSUS</c> is set; the file is
/// <c>GO2CS_NATIVEBOX_CENSUS_FILE</c> (a <c>{pid}</c> in it is replaced; without one the process id is added), or
/// <c>nativebox-census-&lt;pid&gt;.txt</c> in the temporary directory.
/// </summary>
internal static class NativeBoxDereferenceCensus
{
    // Read once. A census that can be switched on mid-run cannot be reconciled.
    internal static readonly bool Enabled =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GO2CS_NATIVEBOX_CENSUS"));

    internal const string ClassB2a = "B2a";
    internal const string ClassB2b = "B2b";
    internal const string ClassUnpinnable = "UNPINNABLE";

    private static long s_dereferences;
    private static long s_b2a;
    private static long s_b2b;
    private static long s_unpinnable;
    private static int s_flushing;

    private static readonly ConcurrentDictionary<string, long> s_rows = new();
    private static readonly ConcurrentDictionary<(Type pointee, Type root), string> s_unpinnableKeys = new();

    private const long FlushEvery = 250_000;

    /// <summary>What the census knows of one pointee type, decided once per <c>T</c> by <see cref="NativeBox{T}"/>.</summary>
    internal sealed class Pointee
    {
        internal readonly Type Type;

        // B2a, B2b, or null for a T that holds no reference (counted in the total and in no class).
        internal readonly string? ReferenceClass;

        private readonly string? m_retained;
        private readonly string? m_pinned;
        private readonly string? m_none;

        private Pointee(Type type, string? referenceClass)
        {
            Type = type;
            ReferenceClass = referenceClass;

            if (referenceClass is null)
                return;

            string row = $"{referenceClass}  kind={KindNameOf(type)}  T={NameOf(type)}";

            m_retained = row + "  source=retained";
            m_pinned = row + "  source=pinned";
            m_none = row + "  source=none";
        }

        internal string? RowFor(object? retainedSource, bool pinned) =>
            retainedSource is not null ? m_retained : pinned ? m_pinned : m_none;

        // Null when the census is off: the one test NativeBox<T> makes per dereference.
        internal static Pointee? Of<T>()
        {
            if (!Enabled)
                return null;

            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                return new Pointee(typeof(T), null);

            // The split is ClrLayoutMismatch's own first test: it reads a layout for a Go struct VALUE and answers
            // null for everything else. A struct that reaches here holds a reference and was not refused, so its
            // layout is the unproven kind.
            bool goStructValue = typeof(T).IsValueType && GoReflect.KindOf(typeof(T)) == GoReflect.Struct;

            return new Pointee(typeof(T), goStructValue ? ClassB2b : ClassB2a);
        }
    }

    /// <summary>One whole-value read or write through a native box, already known to name memory.</summary>
    internal static void Dereference(Pointee pointee, object? retainedSource, bool pinned)
    {
        long n = Interlocked.Increment(ref s_dereferences);
        bool newRow = false;

        if (pointee.ReferenceClass is { } referenceClass)
        {
            Interlocked.Increment(ref ReferenceEquals(referenceClass, ClassB2a) ? ref s_b2a : ref s_b2b);
            newRow = Count(pointee.RowFor(retainedSource, pinned)!);
        }

        if (retainedSource is not null && UnpinnableRootOf(retainedSource) is { } root)
        {
            Interlocked.Increment(ref s_unpinnable);

            string key = s_unpinnableKeys.GetOrAdd((pointee.Type, root.GetType()),
                static pair => $"{ClassUnpinnable}  T={NameOf(pair.pointee)}  root={NameOf(pair.root)}");

            newRow |= Count(key);
        }

        // AFTER the class counters, so a block written here reconciles (Q44RegistryCensus.MaybeFlush's lesson).
        if (newRow || n == 1 || n % FlushEvery == 0)
            Flush();
    }

    private static bool Count(string row)
    {
        if (s_rows.TryAdd(row, 1))
            return true;

        s_rows.AddOrUpdate(row, 1, static (_, n) => n + 1);

        return false;
    }

    // The first MANAGED box a native box was derived from, when that box's storage cannot be pinned; else null. A chain
    // of derivations (a native box that retains a native box) is walked to its first managed source, as
    // NativeDerivation.Token walks it. The depth bound is a guard against a cycle nothing should be able to build.
    private static object? UnpinnableRootOf(object? source)
    {
        for (int hop = 0; source is not null && hop < 64; hop++)
        {
            if (source is INativeRooted { NativeRootAddress: not 0 } native)
            {
                source = native.NativeDerivedFrom;
                continue;
            }

            return source is IPointerStorageKind { StorageIsUnpinnable: true } ? source : null;
        }

        return null;
    }

    private static string KindNameOf(Type t) => GoReflect.KindOf(t) switch
    {
        GoReflect.Array => "Array",
        GoReflect.Chan => "Chan",
        GoReflect.Func => "Func",
        GoReflect.Interface => "Interface",
        GoReflect.Map => "Map",
        GoReflect.Pointer => "Pointer",
        GoReflect.Slice => "Slice",
        GoReflect.String => "String",
        GoReflect.Struct => "Struct",
        GoReflect.UnsafePointer => "UnsafePointer",
        int other => other.ToString()
    };

    // A generic type by its arguments: `StandardBox`1` names every pointee alike.
    private static string NameOf(Type t)
    {
        if (!t.IsGenericType)
            return t.FullName ?? t.Name;

        string name = t.Name;
        int tick = name.IndexOf('`');

        if (tick >= 0)
            name = name[..tick];

        return (t.Namespace is { Length: > 0 } ns ? ns + "." : "") + name + "<" + string.Join(",", Array.ConvertAll(t.GetGenericArguments(), NameOf)) + ">";
    }

    /// <summary>The counters, for the control: an assertion is that a counter MOVED across a call.</summary>
    internal static (long dereferences, long b2a, long b2b, long unpinnable) Snapshot() =>
        (Interlocked.Read(ref s_dereferences), Interlocked.Read(ref s_b2a), Interlocked.Read(ref s_b2b), Interlocked.Read(ref s_unpinnable));

    /// <summary>The count of the rows whose text contains every one of <paramref name="parts"/>.</summary>
    internal static long Rows(params string[] parts)
    {
        long total = 0;

        foreach (KeyValuePair<string, long> row in s_rows)
        {
            if (Array.TrueForAll(parts, part => row.Key.Contains(part, StringComparison.Ordinal)))
                total += row.Value;
        }

        return total;
    }

    // Set when a named path carried no {pid} and this process writes a per-process file instead; said in the block.
    private static string? s_rewrittenFrom;

    /// <summary>The path the START block was written to, or null when none was (the census is off).</summary>
    internal static string? StartBlockPath { get; private set; }

    internal static string OutputPath => ResolveOutputPath();

    private static string ResolveOutputPath()
    {
        string pid = Environment.ProcessId.ToString();

        if (Environment.GetEnvironmentVariable("GO2CS_NATIVEBOX_CENSUS_FILE") is not { Length: > 0 } named)
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nativebox-census-{pid}.txt");

        if (named.Contains("{pid}", StringComparison.Ordinal))
            return named.Replace("{pid}", pid);

        string? dir = System.IO.Path.GetDirectoryName(named);
        string perProcess = $"{System.IO.Path.GetFileNameWithoutExtension(named)}-{pid}{System.IO.Path.GetExtension(named)}";
        s_rewrittenFrom = named;

        return string.IsNullOrEmpty(dir) ? perProcess : System.IO.Path.Combine(dir, perProcess);
    }

    private static void Flush()
    {
        // One flusher at a time, and a second arrival skips: a census is never a lock the program waits on. A skipped
        // block is not a lost row, because every block is a snapshot of everything and the exit hook writes the last.
        if (Interlocked.Exchange(ref s_flushing, 1) == 1)
            return;

        try
        {
            DumpTo(OutputPath, partial: true);
        }
        catch
        {
            // A census does not take the program down. DumpTo reports its own write failures.
        }
        finally
        {
            Interlocked.Exchange(ref s_flushing, 0);
        }
    }

    // Which paths this process has written, so its first write truncates its own file and the rest append.
    private static readonly ConcurrentDictionary<string, bool> s_written = new();

    /// <summary>Writes a final block to an explicit path, so the control can report without touching a live run's file.</summary>
    internal static void DumpTo(string path) => DumpTo(path, partial: false);

    private static void DumpTo(string path, bool partial, bool start = false)
    {
        long total = Interlocked.Read(ref s_dereferences);
        long b2a = Interlocked.Read(ref s_b2a), b2b = Interlocked.Read(ref s_b2b), unpinnable = Interlocked.Read(ref s_unpinnable);

        List<string> lines =
        [
            // The totals line is FIRST and greppable in every block.
            $"{(partial ? "NBCENSUS-PARTIAL" : "NBCENSUS")} dereferences={total} b2a={b2a} b2b={b2b} unpinnable={unpinnable}",
            $"NBCENSUS-BLOCK pid={Environment.ProcessId} entry={System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "?"} utc={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}",
            "NBCENSUS-FOLD cumulative snapshot: the LAST block in THIS file is authoritative; sum across FILES, never across blocks"
        ];

        if (start)
            lines.Add("NBCENSUS-START armed at module init, before any dereference: the zeros above are a MEASURED zero. A file whose LAST block is this one armed and dereferenced no native box; NO FILE at all means the census never armed here");

        if (s_rewrittenFrom is { Length: > 0 } from)
            lines.Add($"NBCENSUS-PATH per-process: the configured '{from}' carried no {{pid}}, so this process wrote '{path}'");

        long rowsB2a = 0, rowsB2b = 0, rowsUnpinnable = 0;
        List<string> rows = [];

        foreach (KeyValuePair<string, long> row in s_rows)
        {
            if (row.Key.StartsWith(ClassB2a, StringComparison.Ordinal))
                rowsB2a += row.Value;
            else if (row.Key.StartsWith(ClassB2b, StringComparison.Ordinal))
                rowsB2b += row.Value;
            else
                rowsUnpinnable += row.Value;

            rows.Add($"NBCENSUS-ROW {row.Value,10}  {row.Key}");
        }

        // Only a FINAL block asserts that the rows sum to the class counters: a partial is taken while other threads are
        // between a counter and its row.
        bool reconciles = rowsB2a == b2a && rowsB2b == b2b && rowsUnpinnable == unpinnable;

        lines.Add(reconciles
            ? $"NBCENSUS-RECONCILES rows sum to b2a={rowsB2a} b2b={rowsB2b} unpinnable={rowsUnpinnable}"
            : partial
                ? $"NBCENSUS-PARTIAL-SKEW rows sum to b2a={rowsB2a} b2b={rowsB2b} unpinnable={rowsUnpinnable}: threads between a counter and its row at the flush; a FINAL block must reconcile exactly"
                : $"NBCENSUS-BROKEN rows sum to b2a={rowsB2a} b2b={rowsB2b} unpinnable={rowsUnpinnable} against the counters above");

        rows.Sort(StringComparer.Ordinal);
        lines.AddRange(rows);

        try
        {
            if (s_written.TryAdd(path, true))
                System.IO.File.WriteAllLines(path, lines);
            else
                System.IO.File.AppendAllLines(path, lines);
        }
        catch (Exception e)
        {
            // The one stderr line: a census that cannot report says so.
            Console.Error.WriteLine($"NBCENSUS-UNREPORTED could not write {path}: {e.GetType().Name}");
        }
    }

    [ModuleInitializer]
    internal static void Arm()
    {
        if (!Enabled)
            return;

        AppDomain.CurrentDomain.ProcessExit += static (_, _) => DumpTo(OutputPath, partial: false);

        try
        {
            string startPath = OutputPath;
            DumpTo(startPath, partial: true, start: true);
            StartBlockPath = startPath;
        }
        catch
        {
            // Never the program's problem.
        }
    }
}

// Whether a box names managed storage that has an address and cannot be held still, asked without its pointee type.
// Read by the census above and by nothing else.
internal interface IPointerStorageKind
{
    bool StorageIsUnpinnable { get; }
}

public abstract partial class ж<T> : IPointerStorageKind
{
    bool IPointerStorageKind.StorageIsUnpinnable => StorageKind is PointerStorage.Unpinnable;
}
