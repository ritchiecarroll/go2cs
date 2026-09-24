using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;

// Second round (§8.1 revision, 2026-09-24): the RECOMMENDED cache form as specified, a word-at-a-time
// hash for it, and the module-init literal REGISTRATION table the review asked to be sized.
static class Lit2
{
    // ---- V8/V9: 2-way set-associative content cache, <= Gate bytes, byte[] per way ----------------
    // Replacement: NO write on a hit; a miss fills way 0 if it is empty, otherwise overwrites way 1 --
    // one store either way. A literal that lands in way 0 is therefore never evicted; one that lands
    // in way 1 can be, by any later miss in its set.
    public const int Gate = 16;
    const int Sets = 2048;
    static readonly byte[]?[] s_ways = new byte[]?[Sets * 2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Fnv(ReadOnlySpan<byte> s)
    {
        uint h = 2166136261;
        foreach (byte b in s) h = (h ^ b) * 16777619;
        return h ^ (uint)s.Length;
    }

    // Two 64-bit reads cover every length up to 16: the bytes past the length are masked off, so equal
    // contents hash equal whatever follows them in memory.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint WordHash(ReadOnlySpan<byte> s)
    {
        ulong a = 0, b = 0;
        ref byte r = ref MemoryMarshal.GetReference(s);
        int n = s.Length;
        if (n >= 8)
        {
            a = Unsafe.ReadUnaligned<ulong>(ref r);
            b = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref r, n - 8)); // overlapping tail read, 8..16
        }
        else if (n >= 4)
        {
            a = Unsafe.ReadUnaligned<uint>(ref r);
            b = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref r, n - 4));
        }
        else if (n > 0)
        {
            a = (ulong)r | ((ulong)Unsafe.Add(ref r, n >> 1) << 8) | ((ulong)Unsafe.Add(ref r, n - 1) << 16);
        }
        ulong h = (a * 0x9E3779B97F4A7C15UL) ^ (b * 0xC2B2AE3D27D4EB4FUL) ^ (ulong)n;
        return (uint)(h ^ (h >> 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static @string TwoWay(ReadOnlySpan<byte> s, uint hash)
    {
        int set = (int)(hash & (Sets - 1)) * 2;
        byte[]? w0 = Volatile.Read(ref s_ways[set]);
        if (w0 is not null && w0.Length == s.Length && s.SequenceEqual(w0)) return new @string(w0);
        byte[]? w1 = Volatile.Read(ref s_ways[set + 1]);
        if (w1 is not null && w1.Length == s.Length && s.SequenceEqual(w1)) return new @string(w1);
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        Volatile.Write(ref s_ways[w0 is null ? set : set + 1], copy);
        return new @string(copy);
    }

    public static @string TwoWayFnv(ReadOnlySpan<byte> s)
    {
        if ((uint)(s.Length - 1) >= Gate) return s.Length == 0 ? new @string(Array.Empty<byte>()) : new @string(s);
        return TwoWay(s, Fnv(s));
    }

    public static @string TwoWayWord(ReadOnlySpan<byte> s)
    {
        if ((uint)(s.Length - 1) >= Gate) return s.Length == 0 ? new @string(Array.Empty<byte>()) : new @string(s);
        return TwoWay(s, WordHash(s));
    }

    public static int SetOf(ReadOnlySpan<byte> s, bool word) => (int)((word ? WordHash(s) : Fnv(s)) & (Sets - 1));

    // Which way of its (FNV) set holds s: 0, 1, or -1 for neither. Diagnostics for the eviction rows.
    public static int WayOf(ReadOnlySpan<byte> s)
    {
        int set = SetOf(s, word: false) * 2;
        if (s_ways[set] is { } a && s.SequenceEqual(a)) return 0;
        if (s_ways[set + 1] is { } b && s.SequenceEqual(b)) return 1;
        return -1;
    }

    // ---- V10: the literal REGISTRATION table ----------------------------------------------------
    // A module initializer registers every u8 literal of its module (the generator would emit the
    // calls; this probe writes them by hand). Roslyn stores identical u8 data ONCE per module (see
    // rvaprobe in the README), so the address a call site hands the operator IS the registered one.
    // Keyed by (address, length): exact, never evicted, and a span that is not a literal -- heap or
    // stack memory, never inside the module image -- can never match. Entries are built at
    // registration and never change, so reads take no lock; a registration from a later module
    // publishes a new table, the one write.
    // Round 3 (§8.1R2): the table GROWS. It starts small, doubles past a quarter full (C2_TABLE_LOAD=2: half), and publishes the grown
    // table with one volatile store; a reader holding the old table still sees a consistent (older)
    // index, because a replaced table is never written again. Inserts into the live table write the
    // length and value before the key (release), and readers read the key first (acquire).
    sealed class Table(int size)
    {
        public readonly nint[] Keys = new nint[size];
        public readonly int[] Lengths = new int[size];
        public readonly byte[]?[] Values = new byte[]?[size];
        public int Count;
    }

    static Table s_table = new(1 << 4);
    static readonly object s_registerLock = new();
    public static int Registered;
    public static int Grown;
    // The load factor, one axis: C2_TABLE_LOAD=2 grows past half full; the default grows past a quarter.
    public static readonly int LoadDenominator = Environment.GetEnvironmentVariable("C2_TABLE_LOAD") == "2" ? 2 : 4;

    static unsafe nint Addr(ReadOnlySpan<byte> s) => (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Slot(nint address, int mask)
    {
        ulong h = (ulong)address * 0x9E3779B97F4A7C15UL;
        return (int)(h >> 40) & mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Find(Table t, nint key, int length)
    {
        int mask = t.Keys.Length - 1;
        for (int i = Slot(key, mask); ; i = (i + 1) & mask)
        {
            nint k = Volatile.Read(ref t.Keys[i]);
            if (k == key && t.Lengths[i] == length) return i;
            if (k == 0) return -1;
        }
    }

    static void Insert(Table t, nint key, int length, byte[] value)
    {
        int mask = t.Keys.Length - 1;
        for (int i = Slot(key, mask); ; i = (i + 1) & mask)
        {
            if (t.Keys[i] != 0) continue;
            t.Lengths[i] = length;
            t.Values[i] = value;
            Volatile.Write(ref t.Keys[i], key);
            t.Count++;
            return;
        }
    }

    // Eager: the @string backing is built at registration, so a hit is a lookup and nothing else.
    public static void Register(ReadOnlySpan<byte> literal)
    {
        if (literal.Length == 0) return;
        lock (s_registerLock)
        {
            Table t = s_table;
            nint key = Addr(literal);
            if (Find(t, key, literal.Length) >= 0) return; // deduplicated by Roslyn: already registered
            if ((t.Count + 1) * LoadDenominator > t.Keys.Length) // grow past 1/LoadDenominator full
            {
                var grown = new Table(t.Keys.Length * 2);
                for (int i = 0; i < t.Keys.Length; i++)
                    if (t.Keys[i] != 0) Insert(grown, t.Keys[i], t.Lengths[i], t.Values[i]!);
                Insert(grown, key, literal.Length, literal.ToArray());
                Volatile.Write(ref s_table, grown);
                Grown++;
            }
            else
            {
                Insert(t, key, literal.Length, literal.ToArray());
            }
            Registered++;
        }
    }

    public static @string FromTable(ReadOnlySpan<byte> s)
    {
        if (s.Length == 0) return new @string(Array.Empty<byte>());
        Table t = Volatile.Read(ref s_table);
        int i = Find(t, Addr(s), s.Length);
        return i >= 0 ? new @string(t.Values[i]) : new @string(s); // a miss is today's copy, exactly
    }
}
