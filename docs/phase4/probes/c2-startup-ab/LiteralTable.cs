// LiteralTable.cs - the start-up A/B probe's golib table (DESIGN-string-literal-allocation §8.1R2, §8.1R3).
// Copied into a scratch src/core/golib for the probe's arms; it is not a proposed golib file.
//
// Round 2 (revision 4) adds the HYBRID LAZY form: a module's initializer registers only its image range
// and a registrar delegate (RegisterLazyModule); the first table MISS whose address falls inside that
// range runs the module's registrar once, then retries. GO2CS_LITTABLE_HELPER=1 runs a registrar on a
// helper thread instead of the missing thread, so its allocations land on another thread's byte counter;
// it deadlocks once the registration order is fixed (see the design block, §8.1R3.2).
// Round 3 adds anchor-based range discovery (no file read), and the exit diagnostics behind
// GO2CS_LITTABLE_MISSLOG=1: each miss's address and length, classified against the module images and the
// registered ranges, looked up again after every registrar has run, and the bytes of any that still miss.
// Every arm carries this file; the A arm neither wires the operator nor generates registrations, so its
// only cost is the report hook's environment reads at golib load.
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
    private static long s_registerTicks, s_hits, s_misses, s_lazyRuns, s_lazyCallerBytes, s_rangeMisses, s_mapsReads, s_unalignedImages;
    // GO2CS_LITTABLE_MISSLOG=1 (the diagnostic arm only; added after the timed arms were built): record the
    // first 8,192 miss addresses and, at exit, classify each as inside a module image or outside every one.
    private static readonly nint[]? s_missLog = Environment.GetEnvironmentVariable("GO2CS_LITTABLE_MISSLOG") == "1" ? new nint[8192] : null;
    private static readonly int[]? s_missLen = s_missLog is null ? null : new int[8192];
    private static int s_missLogged;

    private static void LogMiss(nint key, int length)
    {
        if (s_missLog is { } log && s_missLogged < log.Length)
        {
            s_missLen![s_missLogged] = length;
            log[s_missLogged++] = key;
        }
    }

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
                $"lazyModules={s_lazy.Length} mapsReads={s_mapsReads} unalignedImages={s_unalignedImages} lazyRuns={s_lazyRuns} lazyCallerBytes={s_lazyCallerBytes} rangeless={outOfRange} literalOutsideRange={s_rangeMisses} " +
                $"registerMs={System.Diagnostics.Stopwatch.GetElapsedTime(0, s_registerTicks).TotalMilliseconds:F2} assemblies={AppDomain.CurrentDomain.GetAssemblies().Length} " +
                $"jitMethods={System.Runtime.JitInfo.GetCompiledMethodCount()} jitMs={System.Runtime.JitInfo.GetCompilationTime().TotalMilliseconds:F1} jitILBytes={System.Runtime.JitInfo.GetCompiledILBytes()}");

            if (s_missLog is { } missLog)
            {
                // every loaded module's image, from /proc/self/maps, by file name
                var images = new List<(nint lo, nint hi, string name)>();
                try
                {
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        string path = asm.IsDynamic ? "" : asm.Location;
                        if (path.Length > 0 && TryMappedRange(path, System.IO.File.ReadAllLines("/proc/self/maps"), out nint lo, out nint hi))
                            images.Add((lo, hi, asm.GetName().Name ?? "?"));
                    }
                }
                catch { }

                var byImage = new Dictionary<string, int>();
                int outside = 0;
                for (int i = 0; i < s_missLogged; i++)
                {
                    string? owner = null;
                    foreach (var (lo, hi, name) in images)
                        if (missLog[i] >= lo && missLog[i] < hi) { owner = name; break; }
                    if (owner is null) outside++;
                    else byImage[owner] = byImage.GetValueOrDefault(owner) + 1;
                }

                // the same misses against the lazy modules' ranges as registered (anchor or maps discovery)
                int inLazy = 0;
                for (int i = 0; i < s_missLogged; i++)
                    foreach (LazyModule m in s_lazy)
                        if (missLog[i] >= m.Start && missLog[i] < m.End) { inLazy++; break; }

                Console.Error.WriteLine($"LITTABLE-MISSES logged={s_missLogged} outsideEveryImage={outside} insideAnImage={s_missLogged - outside} insideALazyRange={inLazy} images={images.Count}");

                // Were the misses literals that were simply looked up BEFORE their module registered? Run every
                // registrar that has not run (diagnostic only, at exit, after every measured window), then look
                // each logged miss up again: found now means the literal preceded its own registration.
                foreach (LazyModule m in s_lazy)
                    Interlocked.Exchange(ref m.Registrar, null)?.Invoke();

                int foundNow = 0;
                var foundByImage = new Dictionary<string, int>();
                for (int i = 0; i < s_missLogged; i++)
                {
                    if (Find(Volatile.Read(ref s_table), missLog[i], s_missLen![i]) < 0)
                    {
                        // still unmatched: show what the span holds (at most 48 bytes, escaped)
                        var head = new ReadOnlySpan<byte>((void*)missLog[i], Math.Min(s_missLen[i], 48));
                        var sb = new System.Text.StringBuilder();
                        foreach (byte c in head) sb.Append(c is >= 0x20 and < 0x7F and not (byte)'\\' ? ((char)c).ToString() : $"\\x{c:x2}");
                        Console.Error.WriteLine($"LITTABLE-UNMATCHED len={s_missLen[i]} bytes=\"{sb}\"");
                        continue;
                    }
                    foundNow++;
                    foreach (var (lo, hi, name) in images)
                        if (missLog[i] >= lo && missLog[i] < hi) { foundByImage[name] = foundByImage.GetValueOrDefault(name) + 1; break; }
                }

                Console.Error.WriteLine($"LITTABLE-MISSES-FOUND-AFTER-ALL-REGISTER {foundNow} of {s_missLogged} (registered now {Registered})");
                foreach (var kv in foundByImage)
                    Console.Error.WriteLine($"LITTABLE-MISSES-FOUND-IN {kv.Key} {kv.Value} of {byImage.GetValueOrDefault(kv.Key)}");

                // how many separate mappings each image file has (a flat and a mapped layout would be two runs)
                try
                {
                    string[] maps = System.IO.File.ReadAllLines("/proc/self/maps");
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        string path = asm.IsDynamic ? "" : asm.Location;
                        if (path.Length == 0) continue;
                        string name = asm.GetName().Name ?? "?";
                        if (!byImage.ContainsKey(name)) continue;
                        LazyModule? lm = null;
                        foreach (LazyModule m in s_lazy) if (m.Name == name + ".dll") lm = m;
                        var runs = new List<string>();
                        foreach (string line in maps)
                            if (line.EndsWith(path, StringComparison.Ordinal))
                            {
                                int dash = line.IndexOf('-'), space = line.IndexOf(' ');
                                long a = Convert.ToInt64(line[..dash], 16), b = Convert.ToInt64(line[(dash + 1)..space], 16);
                                runs.Add($"{(lm is null ? a : a - lm.Start):+#;-#;0}+{b - a}:{line.Substring(space + 1, 4)}");
                            }
                        int missIn = 0;
                        if (lm is not null)
                            for (int i = 0; i < s_missLogged; i++)
                                if (missLog[i] >= lm.Start && missLog[i] < lm.End) missIn++;
                        Console.Error.WriteLine($"LITTABLE-MAPS {name} lazySize={(lm is null ? -1 : (long)(lm.End - lm.Start))} missesInLazyRange={missIn} mappingsRelativeToLazyStart={string.Join(' ', runs)}");
                    }
                }
                catch { }
                foreach (var kv in byImage)
                    Console.Error.WriteLine($"LITTABLE-MISSES-IN {kv.Key} {kv.Value}");
            }

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
    // Anchor discovery (round 3): the module passes the address of one of its own literals. Its image
    // is mapped contiguously from an `MZ` header (flat or mapped layout, and inside a single-file bundle
    // alike), so a page-by-page scan down from the anchor finds the header, confirmed by `PE\0\0` at
    // e_lfanew. The extent covers both layouts: max over sections of raw end and virtual end. No file read,
    // no /proc/self/maps, and nothing per literal.
    public static void RegisterLazyModule(Module module, Action registrar, ReadOnlySpan<byte> anchor)
    {
        if (s_off)
            return;

        var m = new LazyModule { Registrar = registrar, Name = module.Name, Via = "anchor-scan-failed" };

        if (TryImageFromAnchor(Addr(anchor), out nint lo, out nint hi))
        {
            m.Start = lo;
            m.End = hi;
            m.Via = "anchor";
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

    // Round 3's first scan stepped page by page with no bound and faulted (AccessViolation) in a single-file
    // bundle, whose images need not start on a page and whose neighbouring pages need not be readable. The
    // scan is now bounded to the readable mapping run that holds the anchor (/proc/self/maps, re-read only
    // when the anchor lies outside every cached run) and steps 16 bytes. Every timed L arm was built with the page-step scan, which found 43 of 43
    // and 153 of 153 ranges under the JIT build's flat layout.
    private static (long lo, long hi)[]? s_readable;

    private static bool ReadableRun(nint address, out nint lo, out nint hi)
    {
        lo = hi = 0;

        // read on first use, and again when the anchor lies outside every cached run (a module mapped since)
        for (int attempt = 0; attempt < 2; attempt++)
        {
            if (s_readable is not null)
                foreach (var (a, b) in s_readable)
                    if (address >= (nint)a && address < (nint)b) { lo = (nint)a; hi = (nint)b; return true; }

            if (attempt == 1)
                return false;

            var runs = new List<(long lo, long hi)>();

            try
            {
                foreach (string line in System.IO.File.ReadAllLines("/proc/self/maps"))
                {
                    int dash = line.IndexOf('-'), space = line.IndexOf(' ');
                    if (dash < 0 || space < 0 || line[space + 1] != 'r')
                        continue;
                    long a = Convert.ToInt64(line[..dash], 16), b = Convert.ToInt64(line[(dash + 1)..space], 16);
                    if (runs.Count > 0 && runs[^1].hi == a) runs[^1] = (runs[^1].lo, b); // merge adjacent
                    else runs.Add((a, b));
                }
            }
            catch { }

            s_readable = runs.ToArray();
            s_mapsReads++;
        }

        return false;
    }

    private static bool TryImageFromAnchor(nint anchor, out nint lo, out nint hi)
    {
        lo = hi = 0;

        if (!ReadableRun(anchor, out nint floor, out nint ceiling))
            return false;

        for (nint p = anchor & ~(nint)15; p >= floor; p -= 16)
        {
            if (*(ushort*)p != 0x5A4D) // "MZ"
                continue;

            if (p + 0x40 > ceiling)
                continue;

            int lfanew = *(int*)(p + 0x3C);

            if (lfanew <= 0 || lfanew > 4096 - 256 || p + lfanew + 24 > ceiling || *(uint*)(p + lfanew) != 0x00004550) // "PE\0\0"
                continue;

            nint coff = p + lfanew + 4;
            int sections = *(ushort*)(coff + 2), optionalSize = *(ushort*)(coff + 16);
            nint table = coff + 20 + optionalSize;

            if (table + sections * 40 > ceiling)
                continue;

            long extent = *(uint*)(coff + 20 + 56); // SizeOfImage

            for (int k = 0; k < sections; k++)
            {
                nint sh = table + k * 40;
                long virtualEnd = *(uint*)(sh + 12) + (long)*(uint*)(sh + 8);
                long rawEnd = *(uint*)(sh + 20) + (long)*(uint*)(sh + 16);
                extent = Math.Max(extent, Math.Max(virtualEnd, rawEnd));
            }

            if (anchor >= p + (nint)extent)
                return false; // the nearest header below the anchor does not cover it

            lo = p;
            hi = p + (nint)extent;
            if ((lo & 4095) != 0) s_unalignedImages++;
            return true;
        }

        return false;
    }

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

        return s_lazy.Length == 0 ? Miss(key, s.Length) : LazyMiss(key, s.Length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[]? Miss(nint key, int length)
    {
        LogMiss(key, length);
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
            LogMiss(key, length);
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

        LogMiss(key, length);
        s_misses++;
        return null;
    }
}
