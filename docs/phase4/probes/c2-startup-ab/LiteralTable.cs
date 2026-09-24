// LiteralTable.cs - the start-up A/B probe's golib table (DESIGN-string-literal-allocation §8.1R2). Copied into
// a scratch src/core/golib for arm B only; it is not a proposed golib file.
using System;
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

    private static Table s_table = new(1 << 12);
    private static readonly object s_lock = new();
    public static int Registered;
    private static readonly bool s_off = Environment.GetEnvironmentVariable("GO2CS_LITTABLE_OFF") == "1";
    private static long s_registerTicks;

    static LiteralTable()
    {
        if (Environment.GetEnvironmentVariable("GO2CS_LITTABLE_REPORT") == "1")
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                Console.Error.WriteLine($"LITTABLE off={s_off} registered={Registered} slots={s_table.Keys.Length} assemblies={AppDomain.CurrentDomain.GetAssemblies().Length} registerMs={System.Diagnostics.Stopwatch.GetElapsedTime(0, s_registerTicks).TotalMilliseconds:F2} jitMethods={System.Runtime.JitInfo.GetCompiledMethodCount()} jitMs={System.Runtime.JitInfo.GetCompilationTime().TotalMilliseconds:F1} jitILBytes={System.Runtime.JitInfo.GetCompiledILBytes()}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint Addr(ReadOnlySpan<byte> s) => (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Slot(nint key, int mask) => (int)(((ulong)key * 0x9E3779B97F4A7C15UL) >> 40) & mask;

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

            if (Find(t, key, literal.Length) >= 0)
                return;

            if ((t.Count + 1) * 2 > t.Keys.Length)
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

    private static void Insert(Table t, nint key, int length, byte[] value)
    {
        int mask = t.Keys.Length - 1;

        for (int i = Slot(key, mask); ; i = (i + 1) & mask)
        {
            if (t.Keys[i] == 0)
            {
                t.Lengths[i] = length;
                t.Values[i] = value;
                Volatile.Write(ref t.Keys[i], key);
                t.Count++;
                return;
            }
        }
    }

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
        int i = Find(t, Addr(s), s.Length);
        return i < 0 ? null : t.Values[i];
    }
}
