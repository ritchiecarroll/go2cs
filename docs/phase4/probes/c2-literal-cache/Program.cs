using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using go;

static class Program
{
    // Hoisted forms (Tier C today; arms A/B/C would emit these).
    static readonly @string nˢ = "n"u8;
    static readonly @string fmtSˢ = "%s"u8;
    static readonly @string fnAtoiˢ = "Atoi"u8;
    static readonly object aBoxˢ = (@string)"a"u8, bBoxˢ = (@string)"b"u8, cBoxˢ = (@string)"c"u8;

    static int s_sink;
    static readonly object v0 = 42;

    [MethodImpl(MethodImplOptions.NoInlining)] static void Use(@string s) => s_sink += s.Length;
    [MethodImpl(MethodImplOptions.NoInlining)] static void UseAny(params object[] kv) => s_sink += kv.Length;
    [MethodImpl(MethodImplOptions.NoInlining)] static void Printf(@string format, object arg) => s_sink += format.Length;

    static (double ns, double bytes, double counted) Measure(System.Action<int> body, int n)
    {
        body(1000); // warm (and first-use of every literal)
        double best = double.MaxValue, bytes = 0, counted = 0;
        for (int round = 0; round < 7; round++)
        {
            long b0 = GC.GetAllocatedBytesForCurrentThread();
            long c0 = AllocationCounter.CurrentThreadCount;
            var sw = Stopwatch.StartNew();
            body(n);
            sw.Stop();
            best = Math.Min(best, sw.Elapsed.TotalNanoseconds / n);
            bytes = (double)(GC.GetAllocatedBytesForCurrentThread() - b0) / n;
            counted = (double)(AllocationCounter.CurrentThreadCount - c0) / n;
        }
        return (best, bytes, counted);
    }

    static void Row(string name, System.Action<int> body, int n = 20_000_000)
    {
        var (ns, b, c) = Measure(body, n);
        Console.WriteLine($"{name,-58} {ns,7:F2} ns  {b,6:F1} B  {c,5:F2} counted");
    }

    static void Main()
    {
        AllocationCounter.Enable();
        Console.WriteLine($".NET {Environment.Version}  {System.Runtime.InteropServices.RuntimeInformation.OSDescription}  cores={Environment.ProcessorCount}  csproj TieredCompilation=false (NativeAOT: no JIT)");
        if (Environment.GetCommandLineArgs().Contains("--round2")) { Round2.Run(); return; }
        if (Environment.GetCommandLineArgs().Contains("--eviction")) { Round2.RunEvictionOnly(); return; }
        if (Environment.GetCommandLineArgs().Contains("--regcost")) { RegCost(); return; }
        if (Environment.GetCommandLineArgs().Contains("--stress10")) { Round3.Stress(); return; }
        if (Environment.GetCommandLineArgs().Contains("--tail")) goto tail;
        if (Environment.GetCommandLineArgs().Contains("--refined"))
        {
            Console.WriteLine("-- refined variants, same shapes");
            Row("V0 today: \"n\" to @string param", n => { for (int i = 0; i < n; i++) Use("n"u8); });
            Row("V1 hoisted field", n => { for (int i = 0; i < n; i++) Use(nˢ); });
            Row("V2 content DM", n => { for (int i = 0; i < n; i++) Use(Lit.ContentDM("n"u8)); });
            Row("V6 packed-key DM (<= 8 B)", n => { for (int i = 0; i < n; i++) Use(Lit.Packed("n"u8)); });
            Row("V7 insert-only (<= 16 B)", n => { for (int i = 0; i < n; i++) Use(Lit.InsertOnly("n"u8)); });
            Row("V0 \"%s\"", n => { for (int i = 0; i < n; i++) Printf("%s"u8, v0); });
            Row("V6 \"%s\"", n => { for (int i = 0; i < n; i++) Printf(Lit.Packed("%s"u8), v0); });
            Row("V7 \"%s\"", n => { for (int i = 0; i < n; i++) Printf(Lit.InsertOnly("%s"u8), v0); });
            byte[] mb = new byte[8];
            Row("miss V0 (non-literal 8 B)", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(mb, (long)i); Use(new @string((ReadOnlySpan<byte>)mb)); } }, 5_000_000);
            Row("miss V6", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(mb, (long)i); Use(Lit.Packed(mb)); } }, 5_000_000);
            Row("miss V7 (fills to cap, then length+probe only)", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(mb, (long)i); Use(Lit.InsertOnly(mb)); } }, 5_000_000);
            byte[] big = new byte[24];
            Row("gated-out V6 (non-literal 24 B: length check only)", n => { for (int i = 0; i < n; i++) { big[0] = (byte)i; Use(Lit.Packed(big)); } }, 5_000_000);
            Row("gated-out V0 (non-literal 24 B)", n => { for (int i = 0; i < n; i++) { big[0] = (byte)i; Use(new @string((ReadOnlySpan<byte>)big)); } }, 5_000_000);
            return;
        }
        Console.WriteLine("-- degenerate literal \"n\" to an @string parameter (arm A's shape)");
        Row("V0 today: (@string)\"n\"u8 per call", n => { for (int i = 0; i < n; i++) Use("n"u8); });
        Row("V1 hoisted field nˢ (arm A)", n => { for (int i = 0; i < n; i++) Use(nˢ); });
        Row("V2 golib cache, content-hashed direct-mapped", n => { for (int i = 0; i < n; i++) Use(Lit.ContentDM("n"u8)); });
        Row("V5 golib cache, address-keyed direct-mapped", n => { for (int i = 0; i < n; i++) Use(Lit.AddressDM("n"u8)); });
        Row("V3 golib cache, ConcurrentDictionary alt-lookup", n => { for (int i = 0; i < n; i++) Use(Lit.Dict("n"u8)); });
        Row("V4 UTF-16 literal, reference-keyed", n => { for (int i = 0; i < n; i++) Use(Lit.Utf16Ref("n")); });

        Console.WriteLine("-- format-position literal: Printf(\"%s\", v) (arm B's shape)");
        object v = 42;
        Row("V0 today", n => { for (int i = 0; i < n; i++) Printf("%s"u8, v); });
        Row("V1 hoisted field (arm B)", n => { for (int i = 0; i < n; i++) Printf(fmtSˢ, v); });
        Row("V2 golib cache (content)", n => { for (int i = 0; i < n; i++) Printf(Lit.ContentDM("%s"u8), v); });

        Console.WriteLine("-- function-local const: @string fnAtoi = \"Atoi\"u8 (arm C's shape)");
        Row("V0 today", n => { for (int i = 0; i < n; i++) { @string fnAtoi = "Atoi"u8; Use(fnAtoi); } });
        Row("V1 hoisted under its own name (arm C)", n => { for (int i = 0; i < n; i++) Use(fnAtoiˢ); });
        Row("V2 golib cache (content)", n => { for (int i = 0; i < n; i++) { @string fnAtoi = Lit.ContentDM("Atoi"u8); Use(fnAtoi); } });

        Console.WriteLine("-- slog-style ...any pack, 3 degenerate keys: f(\"a\", 1, \"b\", 2, \"c\", 3)");
        object one = 1, two = 2, three = 3;
        Row("V0 today (key copy + box per key)", n => { for (int i = 0; i < n; i++) UseAny((@string)"a"u8, one, (@string)"b"u8, two, (@string)"c"u8, three); }, 5_000_000);
        Row("V1 pre-boxed hoisted fields (arm A, any-target)", n => { for (int i = 0; i < n; i++) UseAny(aBoxˢ, one, bBoxˢ, two, cBoxˢ, three); }, 5_000_000);
        Row("V2 golib cache (copy gone, box stays)", n => { for (int i = 0; i < n; i++) UseAny(Lit.ContentDM("a"u8), one, Lit.ContentDM("b"u8), two, Lit.ContentDM("c"u8), three); }, 5_000_000);

        tail:
        Console.WriteLine("-- miss path: a NON-literal 8-byte span that differs every call (the cost the cache adds elsewhere)");
        byte[] buf = new byte[8];
        Row("V0 today", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(buf, (long)i); Use(new @string((ReadOnlySpan<byte>)buf)); } }, 5_000_000);
        Row("V2 content cache, always missing", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(buf, (long)i); Use(Lit.ContentDM(buf)); } }, 5_000_000);
        Row("V3 dictionary cache, always missing (capped at 64K)", n => { for (int i = 0; i < n; i++) { BitConverter.TryWriteBytes(buf, (long)i); Use(Lit.Dict(buf)); } }, 5_000_000);

        Console.WriteLine("-- thrash: two hot literals forced into ONE direct-mapped slot (worst case, deterministic by content)");
        // Find a real colliding pair for the content hash rather than assuming one exists.
        (byte[] x, byte[] y) = FindCollision();
        Row($"V2 alternating \"{System.Text.Encoding.ASCII.GetString(x)}\"/\"{System.Text.Encoding.ASCII.GetString(y)}\"", n => { for (int i = 0; i < n; i++) { Use(Lit.ContentDM(x)); Use(Lit.ContentDM(y)); } }, 5_000_000);

        Console.WriteLine("-- concurrency: 8 threads x 2M mixed hits/misses, every result content-checked");
        Stress();
    }

    static (byte[], byte[]) FindCollision()
    {
        var seen = new Dictionary<uint, byte[]>();
        for (int i = 0; ; i++)
        {
            byte[] k = System.Text.Encoding.ASCII.GetBytes("k" + i);
            uint h = 2166136261; foreach (byte b in k) h = (h ^ b) * 16777619; h ^= (uint)k.Length;
            uint slot = h & 4095;
            if (seen.TryGetValue(slot, out byte[]? other)) return (other, k);
            seen[slot] = k;
        }
    }

    static void Stress()
    {
        int bad = 0;
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
                    int k = rnd.Next(10_000); // 10K distinct contents over 4K slots: constant collisions and races
                    System.Text.Encoding.ASCII.TryGetBytes("x" + k.ToString("D5"), tmp, out _);
                    @string s = Lit.ContentDM(tmp);
                    @string a = Lit.AddressDM(tmp);
                    if (!((ReadOnlySpan<byte>)s.ToSpan()).SequenceEqual(tmp) || !((ReadOnlySpan<byte>)a.ToSpan()).SequenceEqual(tmp)) Interlocked.Increment(ref bad);
                }
            });
        }
        foreach (var th in threads) th.Start();
        foreach (var th in threads) th.Join();
        Console.WriteLine($"   content mismatches: {bad} of {8 * 2_000_000 * 2} results");
    }

    // §8.1.x: the module-initializer cost at the corpus's largest module. The first call includes the
    // JIT of one 3,805-call method (none under NativeAOT); the second call is the dedup path alone.
    static void RegCost()
    {
#if REGBULK
        int before = Lit2.Registered;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        RegBulk.Run();
        double first = sw.Elapsed.TotalMilliseconds;
        sw.Restart();
        RegBulk.Run();
        double again = sw.Elapsed.TotalMilliseconds;
        Console.WriteLine($"registered {Lit2.Registered - before} of {RegBulk.Count} (Roslyn dedup would show fewer); first call {first:F2} ms; second call (all present) {again:F2} ms");
#else
        Console.WriteLine("RegBulk.cs is absent: run `bash gen-regbulk.sh 3805 > RegBulk.cs` and rebuild");
#endif
    }
}
