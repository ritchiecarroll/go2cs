using System;
using System.Threading;
using go;

// Round 3 (§8.1R2, review fix 19): the registration table under CONCURRENT growth. Four writers register
// 50,000 distinct (address, length) keys over one pinned buffer -- the table starts at 16 slots, so it
// grows about twelve times mid-run -- while four readers look up keys a writer has already published
// (every one must HIT: equal bytes and zero counted objects) and keys nobody registered (every one must
// MISS: equal bytes, one counted copy).
static class Round3
{
    public static void Stress()
    {
        AllocationCounter.Enable();
        const int Writers = 4, PerWriter = 12_500, Readers = 4;
        byte[] buf = GC.AllocateArray<byte>(PerWriter * Writers + 64, pinned: true);
        new Random(7).NextBytes(buf);
        int Len(int off) => 1 + off % 24;
        var progress = new int[Writers];
        int grownBefore = Lit2.Grown, done = 0;
        long hits = 0, misses = 0, badHit = 0, badMiss = 0, hitCounted = 0;

        var threads = new Thread[Writers + Readers];
        for (int w = 0; w < Writers; w++)
        {
            int ww = w;
            threads[w] = new Thread(() =>
            {
                for (int j = 0; j < PerWriter; j++)
                {
                    int off = j * Writers + ww;
                    Lit2.Register(buf.AsSpan(off, Len(off)));
                    Volatile.Write(ref progress[ww], j + 1);
                }
                Interlocked.Increment(ref done);
            });
        }
        for (int r = 0; r < Readers; r++)
        {
            int seed = r;
            threads[Writers + r] = new Thread(() =>
            {
                var rnd = new Random(100 + seed);
                long h = 0, m = 0, bh = 0, bm = 0, hc = 0;
                while (Volatile.Read(ref done) < Writers)
                {
                    int w = rnd.Next(Writers), n = Volatile.Read(ref progress[w]);
                    if (n == 0) continue;
                    int off = rnd.Next(n) * Writers + w;
                    ReadOnlySpan<byte> key = buf.AsSpan(off, Len(off));
                    long c0 = AllocationCounter.CurrentThreadCount;
                    @string s = Lit2.FromTable(key);
                    hc += AllocationCounter.CurrentThreadCount - c0;
                    if (!((ReadOnlySpan<byte>)s.ToSpan()).SequenceEqual(key)) bh++;
                    h++;
                    // an unregistered key: same address, a length no writer uses (25..40)
                    ReadOnlySpan<byte> other = buf.AsSpan(off, 25 + off % 16);
                    @string o = Lit2.FromTable(other);
                    if (!((ReadOnlySpan<byte>)o.ToSpan()).SequenceEqual(other)) bm++;
                    m++;
                }
                Interlocked.Add(ref hits, h); Interlocked.Add(ref misses, m); Interlocked.Add(ref badHit, bh);
                Interlocked.Add(ref badMiss, bm); Interlocked.Add(ref hitCounted, hc);
            });
        }
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
        Console.WriteLine($"-- V10 concurrent growth ({System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}): {Writers * PerWriter} registrations, {Lit2.Grown - grownBefore} growths; " +
                          $"{hits} hit lookups ({badHit} wrong bytes, {hitCounted} counted objects); {misses} miss lookups ({badMiss} wrong bytes)");
    }
}
