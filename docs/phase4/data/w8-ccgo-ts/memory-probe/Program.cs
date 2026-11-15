// W8 memory and race probe (kept as data beside DESIGN-sqlite-ccgo-ts.md): a copy of each heap string's backing, kept in a ConditionalWeakTable keyed by
// the backing, under churn. Arms: POH (a pinned managed array), NATIVE (NativeMemory, finalizer frees), NATIVE+P
// (the same with GC.AddMemoryPressure / RemoveMemoryPressure). Reports the live copies' bytes at their peak during the
// churn and after forced collections, plus the collection count.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

unsafe sealed class NativeCopy
{
    public static long Live;
    public readonly byte* Base;
    readonly int m_len;
    readonly bool m_pressure;
    public NativeCopy(byte[] src, bool pressure)
    {
        m_len = src.Length; m_pressure = pressure;
        Base = (byte*)NativeMemory.Alloc((nuint)m_len);
        src.AsSpan().CopyTo(new Span<byte>(Base, m_len));
        if (pressure) GC.AddMemoryPressure(m_len);
        Interlocked.Add(ref Live, m_len);
    }
    ~NativeCopy()
    {
        NativeMemory.Free(Base);
        if (m_pressure) GC.RemoveMemoryPressure(m_len);
        Interlocked.Add(ref Live, -m_len);
    }
}

static class Program
{
    static int N = 100_000, Size = 1024;

    static void Main(string[] args)
    {
        string arm = args.Length > 0 ? args[0] : "poh";
        if (arm == "race") { Race(); return; } if (args.Length > 2) { N = int.Parse(args[1]); Size = int.Parse(args[2]); }
        var table = new ConditionalWeakTable<byte[], object>();
        int g0 = GC.CollectionCount(0), g2 = GC.CollectionCount(2);
        (long peak, long sum) = Churn(arm, table);

        long before = arm == "poh" ? LivePoh(table) : Interlocked.Read(ref NativeCopy.Live);
        for (int k = 0; k < 3; k++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
        long after = arm == "poh" ? LivePoh(table) : Interlocked.Read(ref NativeCopy.Live);
        Console.WriteLine($"{arm,-9} total {(long)N * Size / 1024 / 1024} MiB copied | live copies at peak (sampled every {(N >= 10_000 ? 1024 : 1)} reads) {peak / 1024 / 1024.0:F1} MiB | at loop end {before / 1024 / 1024.0:F1} MiB | after 3 forced collections {after / 1024.0:F1} KiB | gen0 {GC.CollectionCount(0) - g0} gen2 {GC.CollectionCount(2) - g2} | {sum % 7}");
    }

    // Out of Main, so no backing stays rooted in Main's frame once the churn returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static (long, long) Churn(string arm, ConditionalWeakTable<byte[], object> table)
    {
        long peak = 0, sum = 0;
        var rng = new Random(1);
        int mask = N >= 10_000 ? 1023 : 0;
        for (int i = 0; i < N; i++)
        {
            byte[] backing = new byte[Size];      // a new heap string's backing, dropped after the read
            rng.NextBytes(backing);
            object copy = table.GetValue(backing, b => arm == "poh"
                ? Copy(b)
                : new NativeCopy(b, arm == "native+p"));
            sum += copy is byte[] a ? a[0] : 0;
            if ((i & mask) == 0)
            {
                long live = arm == "poh" ? LivePoh(table) : Interlocked.Read(ref NativeCopy.Live);
                if (live > peak) peak = live;
            }
        }
        return (peak, sum);
    }

    // THE FIRST-READ RACE: 4 threads call GetValue on the SAME new key at once, 20,000 keys. Counts keys whose
    // callback ran more than once (a race that made a loser), and keys where any two threads received DIFFERENT
    // objects (a caller observing a loser). Every created object is recorded, so a loser that some caller received
    // would show as a mismatch.
    static void Race()
    {
        const int Keys = 20_000, Threads = 4;
        var table = new ConditionalWeakTable<byte[], object>();
        byte[][] keys = new byte[Keys][];
        for (int i = 0; i < Keys; i++) keys[i] = new byte[16];
        object?[,] got = new object?[Keys, Threads];
        int[] created = new int[Keys];
        int[] arrived = new int[Keys];
        var workers = new Thread[Threads];
        for (int t = 0; t < Threads; t++)
        {
            int me = t;
            workers[t] = new Thread(() =>
            {
                for (int i = 0; i < Keys; i++)
                {
                    Interlocked.Increment(ref arrived[i]);
                    SpinWait spin = default;
                    while (Volatile.Read(ref arrived[i]) < Threads) spin.SpinOnce(-1);
                    got[i, me] = table.GetValue(keys[i], k => { Interlocked.Increment(ref created[i]); return new object(); });
                }
            });
            workers[t].Start();
        }
        foreach (var w in workers) w.Join();
        int raced = 0, mismatched = 0, maxCreated = 0;
        for (int i = 0; i < Keys; i++)
        {
            if (created[i] > 1) raced++;
            if (created[i] > maxCreated) maxCreated = created[i];
            for (int t = 1; t < Threads; t++)
                if (!ReferenceEquals(got[i, t], got[i, 0])) { mismatched++; break; }
        }
        Console.WriteLine($"race: {Keys} keys x {Threads} threads | keys whose callback ran more than once {raced} (max {maxCreated}) | keys where two callers got different objects {mismatched}");
    }

    static byte[] Copy(byte[] b)
    {
        byte[] c = GC.AllocateUninitializedArray<byte>(b.Length, pinned: true);
        b.AsSpan().CopyTo(c);
        return c;
    }

    // The live POH copies: the table's entries (keys still alive). Enumeration reads only live entries.
    static long LivePoh(ConditionalWeakTable<byte[], object> table)
    {
        long bytes = 0;
        foreach (var kv in table) bytes += ((byte[])kv.Value).Length;
        return bytes;
    }
}
