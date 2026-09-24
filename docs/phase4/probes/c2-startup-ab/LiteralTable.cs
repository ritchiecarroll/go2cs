// LiteralTable.cs - the start-up A/B probe's golib table (DESIGN-string-literal-allocation §8.1R2, §8.1R3).
// Copied into a scratch src/core/golib for the probe's arms; it is not a proposed golib file.
//
// Round 2 (revision 4) adds the HYBRID LAZY form: a module's initializer registers only its image range
// and a registrar delegate (RegisterLazyModule); the first table MISS whose address falls inside that
// range runs the module's registrar once, then retries. GO2CS_LITTABLE_HELPER=1 runs a registrar on a
// helper thread instead of the missing thread, so its allocations land on another thread's byte counter.
// Every arm carries this file; the A arm neither wires the operator nor generates registrations, so its
// only cost is the report hook's one environment read at golib load.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace go;

public static unsafe class LiteralTable
{
    private sealed class Table
    {
        public readonly nint[] Keys;
        public readonly int[] Lengths;
        public readonly byte[]?[] Values;
        public int Count;
        public Table(int size) { Keys = new nint[size]; Lengths = new int[size]; Values = new byte[]?[size]; }
    }

    private sealed class LazyModule
    {
        public nint Start, End;
        public Action? Registrar;
        public string Name = "";
        public string Via = "none";
        public string Path = "";
        public long CallerBytes;
    }

    private static Table s_table = new(1 << 12);
    private static readonly object s_lock = new();
    private static readonly object s_lazyLock = new();
    private static LazyModule? s_currentLazy; // the module whose registrar is running, for the range check
    private static LazyModule[] s_lazy = [];
    private static bool s_pending;
    private static readonly bool s_off = Environment.GetEnvironmentVariable("GO2CS_LITTABLE_OFF") == "1";
    private static readonly bool s_helper = Environment.GetEnvironmentVariable("GO2CS_LITTABLE_HELPER") == "1";
    public static int Registered;
    private static long s_registerTicks, s_hits, s_misses, s_lazyRuns, s_lazyCallerBytes, s_rangeMisses, s_mapsReads;

    [ModuleInitializer]
    internal static void ReportHook()
    {
        if (Environment.GetEnvironmentVariable("GO2CS_LITTABLE_REPORT") != "1")
            return;

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            int inRange = 0, outOfRange = 0;
            foreach (LazyModule m in s_lazy)
                if (m.End > m.Start) inRange++; else outOfRange++;

            Console.Error.WriteLine(
                $"LITTABLE off={s_off} helper={s_helper} registered={Registered} slots={s_table.Keys.Length} hits={s_hits} misses={s_misses} " +
                $"lazyModules={s_lazy.Length} mapsReads={s_mapsReads} lazyRuns={s_lazyRuns} lazyCallerBytes={s_lazyCallerBytes} rangeless={outOfRange} literalOutsideRange={s_rangeMisses} " +
                $"registerMs={System.Diagnostics.Stopwatch.GetElapsedTime(0, s_registerTicks).TotalMilliseconds:F2} assemblies={AppDomain.CurrentDomain.GetAssemblies().Length} " +
                $"jitMethods={System.Runtime.JitInfo.GetCompiledMethodCount()} jitMs={System.Runtime.JitInfo.GetCompilationTime().TotalMilliseconds:F1} jitILBytes={System.Runtime.JitInfo.GetCompiledILBytes()}");

            if (Environment.GetEnvironmentVariable("GO2CS_LITTABLE_REPORT_MODULES") == "1")
                foreach (LazyModule m in s_lazy)
                    Console.Error.WriteLine($"LITTABLE-MODULE {m.Name} via={m.Via} ran={m.Registrar is null} callerBytes={m.CallerBytes} size={(long)(m.End - m.Start)}");
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint Addr(ReadOnlySpan<byte> s) => (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Slot(nint key, int mask) => (int)(((ulong)key * 0x9E3779B97F4A7C15UL) >> 40) & mask;

    // ---- eager registration (arm B), and the body every lazy registrar runs ----
    public static void Register(ReadOnlySpan<byte> literal)
    {
        if (literal.Length == 0 || s_off)
            return;

        long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        try { RegisterCore(literal); } finally { s_registerTicks += System.Diagnostics.Stopwatch.GetTimestamp() - t0; }
    }

    private static void RegisterCore(ReadOnlySpan<byte> literal)
    {
        lock (s_lock)
        {
            Table t = s_table;
            nint key = Addr(literal);

            // Range discovery check: a literal a lazy registrar registers must lie inside its module's range.
            if (s_currentLazy is { } current && (key < current.Start || key >= current.End))
                s_rangeMisses++;

            if (Find(t, key, literal.Length) >= 0)
                return;

            if ((t.Count + 1) * 4 > t.Keys.Length)
            {
                Table grown = new(t.Keys.Length * 2);

                for (int i = 0; i < t.Keys.Length; i++)
                    if (t.Keys[i] != 0)
                        Insert(grown, t.Keys[i], t.Lengths[i], t.Values[i]!);

                t = grown;
            }

            Insert(t, key, literal.Length, literal.ToArray());
            Volatile.Write(ref s_table, t);
            Registered++;
        }
    }

    // ---- lazy registration (arm L): the module's image range and its registrar ----
    public static void RegisterLazyModule(Module module, Action registrar)
    {
        if (s_off)
            return;

        var m = new LazyModule { Registrar = registrar, Name = module.Name };
        nint start = Marshal.GetHINSTANCE(module);

        if (start == 0 || start == -1)
        {
            // Not an OS-loaded image (linux, macOS): resolved from /proc/self/maps on the first miss that
            // needs it, once for every module still pending -- never here, where it would cost a file
            // read per module at start-up.
            m.Path = module.FullyQualifiedName;
            m.Via = "pending";
            s_pending = true;
        }
        else
        {
            m.Via = "hinstance";
            // PE header: e_lfanew at 0x3C; SizeOfImage at optional-header offset 56 (PE32 and PE32+ alike).
            int peOffset = *(int*)(start + 0x3C);
            uint sizeOfImage = *(uint*)(start + peOffset + 24 + 56);
            long fileLength = 0;

            try { if (System.IO.File.Exists(module.FullyQualifiedName)) fileLength = new System.IO.FileInfo(module.FullyQualifiedName).Length; }
            catch { }

            m.Start = start;
            m.End = start + (nint)Math.Max(sizeOfImage, fileLength);
        }

        lock (s_lock)
        {
            var next = new LazyModule[s_lazy.Length + 1];
            s_lazy.CopyTo(next, 0);
            next[^1] = m;
            Array.Sort(next, (a, b) => a.Start.CompareTo(b.Start));
            Volatile.Write(ref s_lazy, next);
        }
    }

    // /proc/self/maps lines: "start-end perms offset dev inode path". Every mapping of the file counts.
    private static bool TryMappedRange(string path, string[] maps, out nint lo, out nint hi)
    {
        lo = hi = 0;

        try
        {
            if (string.IsNullOrEmpty(path))
                return false;

            foreach (string line in maps)
            {
                if (!line.EndsWith(path, StringComparison.Ordinal))
                    continue;

                int dash = line.IndexOf('-'), space = line.IndexOf(' ');
                nint a = (nint)Convert.ToInt64(line[..dash], 16), b = (nint)Convert.ToInt64(line[(dash + 1)..space], 16);

                if (lo == 0 || a < lo) lo = a;
                if (b > hi) hi = b;
            }
        }
        catch
        {
            return false;
        }

        return lo != 0;
    }

    // The helper form: one registrar thread created at golib load, handed work through pre-allocated
    // events, so a lazy registration allocates nothing on the thread whose miss triggered it.
    private static readonly AutoResetEvent? s_workReady = s_helper ? new(false) : null;
    private static readonly AutoResetEvent? s_workDone = s_helper ? new(false) : null;
    private static Action? s_work;

    [ModuleInitializer]
    internal static void StartHelper()
    {
        if (!s_helper)
            return;

        var worker = new Thread(() =>
        {
            while (true)
            {
                s_workReady!.WaitOne();
                s_work!();
                s_work = null;
                s_workDone!.Set();
            }
        }) { IsBackground = true, Name = "littable-registrar" };

        worker.Start();
    }

    private static void Insert(Table t, nint key, int length, byte[] value)
    {
        int mask = t.Keys.Length - 1;

        for (int i = Slot(key, mask); ; i = (i + 1) & mask)
        {
            if (t.Keys[i] != 0)
                continue;

            t.Lengths[i] = length;
            t.Values[i] = value;
            Volatile.Write(ref t.Keys[i], key);
            t.Count++;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Find(Table t, nint key, int length)
    {
        int mask = t.Keys.Length - 1;

        for (int i = Slot(key, mask); ; i = (i + 1) & mask)
        {
            nint k = Volatile.Read(ref t.Keys[i]);

            if (k == key && t.Lengths[i] == length)
                return i;

            if (k == 0)
                return -1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? Lookup(ReadOnlySpan<byte> s)
    {
        if (s.Length == 0)
            return null;

        Table t = Volatile.Read(ref s_table);
        nint key = Addr(s);
        int i = Find(t, key, s.Length);

        if (i >= 0)
        {
            s_hits++;
            return t.Values[i];
        }

        return s_lazy.Length == 0 ? Miss() : LazyMiss(key, s.Length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[]? Miss()
    {
        s_misses++;
        return null;
    }

    // A miss: if the span lies inside a lazy module whose registrar has not run, run it once and retry.
    private static void ResolvePending()
    {
        lock (s_lock)
        {
            if (!s_pending)
                return;

            string[] maps = [];

            try { maps = System.IO.File.ReadAllLines("/proc/self/maps"); }
            catch { }

            foreach (LazyModule m in s_lazy)
            {
                if (m.Via != "pending")
                    continue;

                if (TryMappedRange(m.Path, maps, out nint lo, out nint hi))
                {
                    m.Start = lo;
                    m.End = hi;
                    m.Via = "proc-maps";
                }
                else
                {
                    m.Via = "none";
                }
            }

            var next = (LazyModule[])s_lazy.Clone();
            Array.Sort(next, (a, b) => a.Start.CompareTo(b.Start));
            Volatile.Write(ref s_lazy, next);
            s_pending = false;
            s_mapsReads++;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[]? LazyMiss(nint key, int length)
    {
        if (s_pending)
            ResolvePending();

        LazyModule[] lazy = Volatile.Read(ref s_lazy);
        int lo = 0, hi = lazy.Length - 1, found = -1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >>> 1;

            if (lazy[mid].Start <= key) { found = mid; lo = mid + 1; }
            else hi = mid - 1;
        }

        if (found < 0 || key >= lazy[found].End)
        {
            s_misses++;
            return null;
        }

        LazyModule m = lazy[found];

        lock (s_lazyLock)
        {
        Action? registrar = Interlocked.Exchange(ref m.Registrar, null);

        if (registrar is not null)
        {
            s_currentLazy = m;
            long bytes0 = GC.GetAllocatedBytesForCurrentThread();

            if (s_helper)
            {
                s_work = registrar;
                s_workReady!.Set();
                s_workDone!.WaitOne();
            }
            else
            {
                registrar();
            }

            m.CallerBytes = GC.GetAllocatedBytesForCurrentThread() - bytes0;
            s_lazyCallerBytes += m.CallerBytes;
            s_lazyRuns++;
            s_currentLazy = null;
        }
        }

        Table t = Volatile.Read(ref s_table);
        int i = Find(t, key, length);

        if (i >= 0)
        {
            s_hits++;
            return t.Values[i];
        }

        s_misses++;
        return null;
    }
}
