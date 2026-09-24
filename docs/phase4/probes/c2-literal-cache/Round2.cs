using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using go;

// Round 2 rows (§8.1 revision): the recommended 2-way form measured instead of extrapolated, hits
// near the gate, the registration table, a noise-floor duplicate of the baseline, the gate's own
// cost, a three-literal thrash in one set, a cross-thread eviction attack, and the 8-thread stress.
static class Round2
{
    static int s_sink;

    [MethodImpl(MethodImplOptions.NoInlining)] static void Use(@string s) => s_sink += s.Length;

    // The literals every call site below spells, registered the way a generated module initializer
    // would register them. Roslyn stores each distinct u8 content once per module, so these are the
    // very addresses the sites hand the operator.
    [ModuleInitializer]
    internal static void RegisterLiterals()
    {
        Lit2.Register("n"u8);
        Lit2.Register("%s: %v"u8);
        Lit2.Register("key=%q value"u8);
        Lit2.Register("0123456789abcdef"u8);
        Lit2.Register("0123456789abcdef0123456789abcdef"u8);
        Lit2.Register("// "u8);
    }

    static (double ns, double counted) Measure(Action<int> body, int n)
    {
        body(1000);
        double best = double.MaxValue, counted = 0;
        for (int round = 0; round < 7; round++)
        {
            long c0 = AllocationCounter.CurrentThreadCount;
            var sw = Stopwatch.StartNew();
            body(n);
            sw.Stop();
            best = Math.Min(best, sw.Elapsed.TotalNanoseconds / n);
            counted = (double)(AllocationCounter.CurrentThreadCount - c0) / n;
        }
        return (best, counted);
    }

    static void Row(string name, Action<int> body, int n = 20_000_000)
    {
        var (ns, c) = Measure(body, n);
        Console.WriteLine($"{name,-62} {ns,7:F2} ns  {c,5:F2} counted");
    }

    public static void Run()
    {
        Console.WriteLine($".NET {Environment.Version}  {System.Runtime.InteropServices.RuntimeInformation.OSDescription}  {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}  cores={Environment.ProcessorCount}  tiered={(Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "(runtimeconfig)")}");
        Console.WriteLine($"registered literals: {Lit2.Registered}");

        Console.WriteLine("-- noise floor: the baseline twice");
        Row("V0  today, 1 B", n => { for (int i = 0; i < n; i++) Use("n"u8); });
        Row("V0' today, 1 B (identical body)", n => { for (int i = 0; i < n; i++) Use("n"u8); });

        Console.WriteLine("-- hit cost by length (1 / 8 / 12 / 16 / 32 B)");
        foreach (var (label, run0, run2, runW, runT) in Shapes())
        {
            Row($"V0  today      {label}", run0);
            Row($"V8  2-way FNV  {label}", run2);
            Row($"V9  2-way word {label}", runW);
            Row($"V10 table      {label}", runT);
        }
        Row("V1  hoisted field (for scale)", n => { for (int i = 0; i < n; i++) Use(s_hoisted); });

        Console.WriteLine("-- non-literal spans through the same entry (the tax elsewhere)");
        byte[] m8 = new byte[8], m24 = new byte[24];
        Row("miss V0  today, 8 B", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(m8, (long)i); Use(new @string((ReadOnlySpan<byte>)m8)); } }, 5_000_000);
        Row("miss V8  2-way, 8 B (inserts)", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(m8, (long)i); Use(Lit2.TwoWayFnv(m8)); } }, 5_000_000);
        Row("miss V10 table, 8 B (never inserts)", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(m8, (long)i); Use(Lit2.FromTable(m8)); } }, 5_000_000);
        Row("gate V0  today, 24 B", n => { for (int i = 0; i < n; i++) { m24[0] = (byte)i; Use(new @string((ReadOnlySpan<byte>)m24)); } }, 5_000_000);
        Row("gate V8  over the 16 B gate, 24 B", n => { for (int i = 0; i < n; i++) { m24[0] = (byte)i; Use(Lit2.TwoWayFnv(m24)); } }, 5_000_000);
        Row("gate V10 table, 24 B non-literal", n => { for (int i = 0; i < n; i++) { m24[0] = (byte)i; Use(Lit2.FromTable(m24)); } }, 5_000_000);

        Console.WriteLine("-- three literals thrashing ONE 2-way set (FNV; found by search, deterministic by content)");
        var (x, y, z) = FindTriple();
        Row($"V8  alternating 3 contents in one set, per round of 3", n => { for (int i = 0; i < n; i++) { Use(Lit2.TwoWayFnv(x)); Use(Lit2.TwoWayFnv(y)); Use(Lit2.TwoWayFnv(z)); } }, 2_000_000);
        Console.WriteLine("   (one op = 3 conversions: divide ns and counted by 3)");

        Console.WriteLine("-- PerfStringMatch's `\"// \"u8` shape, 16M evaluations");
        foreach (var (label, body) in new (string, Action<int>)[]
        {
            ("today", n => { for (int i = 0; i < n; i++) Use("// "u8); }),
            ("V8 2-way", n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("// "u8)); }),
            ("V10 table", n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("// "u8)); }),
            ("hoisted", n => { for (int i = 0; i < n; i++) Use(s_slashes); }),
        })
        {
            var (ns, _) = Measure(body, 16_000_000);
            Console.WriteLine($"   {label,-10} {ns * 16_000_000 / 1e6,8:F1} ms");
        }

        CrossThreadEviction();
        Stress();
    }

    public static void RunEvictionOnly() => CrossThreadEviction();

    static readonly @string s_hoisted = "n"u8;
    static readonly @string s_slashes = "// "u8;

    static IEnumerable<(string, Action<int>, Action<int>, Action<int>, Action<int>)> Shapes()
    {
        yield return ("1 B", n => { for (int i = 0; i < n; i++) Use("n"u8); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("n"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayWord("n"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("n"u8)); });
        yield return ("8 B", n => { for (int i = 0; i < n; i++) Use("%s: %v"u8); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("%s: %v"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayWord("%s: %v"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("%s: %v"u8)); });
        yield return ("12 B", n => { for (int i = 0; i < n; i++) Use("key=%q value"u8); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("key=%q value"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayWord("key=%q value"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("key=%q value"u8)); });
        yield return ("16 B", n => { for (int i = 0; i < n; i++) Use("0123456789abcdef"u8); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("0123456789abcdef"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayWord("0123456789abcdef"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("0123456789abcdef"u8)); });
        yield return ("32 B", n => { for (int i = 0; i < n; i++) Use("0123456789abcdef0123456789abcdef"u8); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayFnv("0123456789abcdef0123456789abcdef"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.TwoWayWord("0123456789abcdef0123456789abcdef"u8)); }, n => { for (int i = 0; i < n; i++) Use(Lit2.FromTable("0123456789abcdef0123456789abcdef"u8)); });
    }

    static (byte[], byte[], byte[]) FindTriple()
    {
        var bySet = new Dictionary<int, List<byte[]>>();
        for (int i = 0; ; i++)
        {
            byte[] k = System.Text.Encoding.ASCII.GetBytes("t" + i);
            int set = Lit2.SetOf(k, word: false);
            if (!bySet.TryGetValue(set, out var list)) bySet[set] = list = [];
            list.Add(k);
            if (list.Count == 3) return (list[0], list[1], list[2]);
        }
    }

    // A literal is measured on one thread while another thread converts non-literal spans that land in
    // the literal's own set -- the per-thread counter sees only the measuring thread, the table is
    // process-wide. Reported as the measured thread's counted objects per 1M conversions.
    static void CrossThreadEviction()
    {
        Console.WriteLine("-- cross-thread eviction: thread A measures a literal, thread B converts colliding non-literals");
        byte[] lit = System.Text.Encoding.ASCII.GetBytes("%d: %s");
        int set = Lit2.SetOf(lit, word: false);
        var colliders = new List<byte[]>();
        for (int i = 0; colliders.Count < 64; i++)
        {
            byte[] k = System.Text.Encoding.ASCII.GetBytes("c" + i);
            if (Lit2.SetOf(k, word: false) == set) colliders.Add(k);
        }
        foreach (bool literalInWay0 in new[] { true, false })
        {
            // Seed the set: either the literal arrives first (way 0, sticky) or a non-literal does.
            // (A fresh set is not available again, so the second arm uses a second literal and set.)
            byte[] l = literalInWay0 ? lit : System.Text.Encoding.ASCII.GetBytes("%x %x");
            int s2 = Lit2.SetOf(l, word: false);
            var col = literalInWay0 ? colliders : FindColliders(s2);
            if (!literalInWay0) Lit2.TwoWayFnv(col[0]);  // a non-literal takes way 0 first
            Lit2.TwoWayFnv(l);
            int wayBefore = Lit2.WayOf(l);
            using var stop = new CancellationTokenSource();
            var attacker = new Thread(() => { int i = 1; while (!stop.IsCancelled()) Lit2.TwoWayFnv(col[i++ % col.Count]); });
            attacker.Start();
            Use(Lit2.FromTable("n"u8)); // runs Round2's static constructor (two hoisted fields, 2 counted) outside the window
            long c0 = AllocationCounter.CurrentThreadCount;
            const int N = 2_000_000;
            for (int i = 0; i < N; i++) Use(Lit2.TwoWayFnv(l));
            long misses = AllocationCounter.CurrentThreadCount - c0;
            stop.Cancel();
            attacker.Join();
            long t0 = AllocationCounter.CurrentThreadCount;
            for (int i = 0; i < N; i++) Use(Lit2.FromTable("n"u8));
            Console.WriteLine($"   literal seeded into way {wayBefore} (arm {(literalInWay0 ? "literal-first" : "non-literal-first")}): V8 counted misses {misses * 1_000_000.0 / N:F1} per 1M under attack; V10 table {(AllocationCounter.CurrentThreadCount - t0) * 1_000_000.0 / N:F1} per 1M");
        }
    }

    static List<byte[]> FindColliders(int set)
    {
        var list = new List<byte[]>();
        for (int i = 0; list.Count < 64; i++)
        {
            byte[] k = System.Text.Encoding.ASCII.GetBytes("q" + i);
            if (Lit2.SetOf(k, word: false) == set) list.Add(k);
        }
        return list;
    }

    static bool IsCancelled(this CancellationTokenSource s) => s.IsCancellationRequested;

    // 8 threads of mixed hits and misses over 10K contents in 2,048 sets. The content check CAN fail
    // here in a way the first round's could not: a torn read of a way, or a way published before its
    // bytes, would hand back an array whose bytes differ from the span that was asked for.
    static void Stress()
    {
        int bad = 0;
        long results = 0;
        var threads = new Thread[8];
        for (int t = 0; t < threads.Length; t++)
        {
            int seed = t;
            threads[t] = new Thread(() =>
            {
                var rnd = new Random(seed);
                Span<byte> tmp = stackalloc byte[6];
                for (int i = 0; i < 2_000_000; i++)
                {
                    int k = rnd.Next(10_000);
                    System.Text.Encoding.ASCII.TryGetBytes("x" + k.ToString("D5"), tmp, out _);
                    @string s = Lit2.TwoWayFnv(tmp);
                    if (!((ReadOnlySpan<byte>)s.ToSpan()).SequenceEqual(tmp)) Interlocked.Increment(ref bad);
                    Interlocked.Increment(ref results);
                }
            });
        }
        foreach (var th in threads) th.Start();
        foreach (var th in threads) th.Join();
        Console.WriteLine($"-- 8-thread stress, V8: {bad} content mismatches in {results} results ({System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture})");
    }
}
