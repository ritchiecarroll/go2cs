using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;

// Candidate golib mechanisms. Each returns a @string whose bytes equal the input; the cache never
// decides correctness (every hit is CONTENT-verified), only whether a copy is made.
static class Lit
{
    const int MaxLen = 32;

    // V2: direct-mapped, content-hashed, byte[] slots (one reference per slot: publication is atomic
    // and a racing writer can only cause a verified miss). The miss path allocates exactly what the
    // uncached path allocates -- the byte[] -- and nothing else.
    static readonly byte[]?[] s_dm = new byte[]?[4096];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint Hash(ReadOnlySpan<byte> s)
    {
        // FNV-1a over <= 32 bytes: deterministic across runs (no address, no ASLR), so a collision
        // between two hot literals is a property of their CONTENT and reproduces identically.
        uint h = 2166136261;
        foreach (byte b in s) h = (h ^ b) * 16777619;
        return h ^ (uint)s.Length;
    }

    public static @string ContentDM(ReadOnlySpan<byte> s)
    {
        if (s.Length == 0) return new @string(Array.Empty<byte>());
        if (s.Length > MaxLen) return new @string(s);
        ref byte[]? slot = ref s_dm[Hash(s) & (s_dm.Length - 1)];
        byte[]? hit = Volatile.Read(ref slot);
        if (hit is not null && hit.Length == s.Length && s.SequenceEqual(hit)) return new @string(hit);
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        Volatile.Write(ref slot, copy);
        return new @string(copy);
    }

    // V5: direct-mapped keyed by the span's ADDRESS (RVA data for a u8 literal), content-verified.
    static readonly byte[]?[] s_addr = new byte[]?[4096];

    public static unsafe @string AddressDM(ReadOnlySpan<byte> s)
    {
        if (s.Length == 0) return new @string(Array.Empty<byte>());
        if (s.Length > MaxLen) return new @string(s);
        nuint p = (nuint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));
        ref byte[]? slot = ref s_addr[(int)(((p >> 3) ^ (p >> 15) ^ (nuint)s.Length) & (nuint)(s_addr.Length - 1))];
        byte[]? hit = Volatile.Read(ref slot);
        if (hit is not null && hit.Length == s.Length && s.SequenceEqual(hit)) return new @string(hit);
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        Volatile.Write(ref slot, copy);
        return new @string(copy);
    }

    // V3: insert-only ConcurrentDictionary with a span alternate lookup (no key allocation on a hit).
    sealed class BytesComparer : IEqualityComparer<byte[]>, IAlternateEqualityComparer<ReadOnlySpan<byte>, byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y) => x.AsSpan().SequenceEqual(y);
        public int GetHashCode(byte[] obj) => (int)Hash(obj);
        public bool Equals(ReadOnlySpan<byte> alternate, byte[] other) => alternate.SequenceEqual(other);
        public int GetHashCode(ReadOnlySpan<byte> alternate) => (int)Hash(alternate);
        public byte[] Create(ReadOnlySpan<byte> alternate) => alternate.ToArray();
    }

    static readonly ConcurrentDictionary<byte[], byte[]> s_dict = new(new BytesComparer());
    static readonly ConcurrentDictionary<byte[], byte[]>.AlternateLookup<ReadOnlySpan<byte>> s_alt = s_dict.GetAlternateLookup<ReadOnlySpan<byte>>();

    static int s_dictCount;

    public static @string Dict(ReadOnlySpan<byte> s)
    {
        if (s.Length == 0) return new @string(Array.Empty<byte>());
        if (s.Length > MaxLen) return new @string(s);
        if (s_alt.TryGetValue(s, out byte[]? hit)) return new @string(hit);
        if (Volatile.Read(ref s_dictCount) >= 65536) return new @string(s);
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        if (s_alt.TryAdd(s, copy)) Interlocked.Increment(ref s_dictCount);
        return new @string(copy);
    }

    // V4: the UTF-16 literal's REFERENCE is the key (C# literals are interned). Needs a pair per slot,
    // so a slot holds an immutable entry object -- which the miss path must ALSO allocate.
    sealed class Entry(string key, byte[] value) { public readonly string Key = key; public readonly byte[] Value = value; }
    static readonly Entry?[] s_ref = new Entry?[4096];

    public static @string Utf16Ref(string s)
    {
        ref Entry? slot = ref s_ref[RuntimeHelpers.GetHashCode(s) & (s_ref.Length - 1)];
        Entry? e = Volatile.Read(ref slot);
        if (e is not null && ReferenceEquals(e.Key, s)) return new @string(e.Value);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        AllocationCounter.Count(2);
        Volatile.Write(ref slot, new Entry(s, bytes));
        return new @string(bytes);
    }

    // V6: <= 8-byte literals keyed by (length, packed bytes) in one ulong; direct-mapped, byte[] slot.
    static readonly byte[]?[] s_pk = new byte[]?[4096];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong Pack(ReadOnlySpan<byte> s)
    {
        ulong v = 0;
        s.CopyTo(MemoryMarshal.AsBytes(new Span<ulong>(ref v)));
        return v;
    }

    public static @string Packed(ReadOnlySpan<byte> s)
    {
        if ((uint)(s.Length - 1) >= 8) return s.Length == 0 ? new @string(Array.Empty<byte>()) : new @string(s);
        ulong key = Pack(s);
        int idx = (int)(((key * 0x9E3779B97F4A7C15UL) ^ (ulong)s.Length * 0xC2B2AE3D27D4EB4FUL) >> 52);
        ref byte[]? slot = ref s_pk[idx];
        byte[]? hit = Volatile.Read(ref slot);
        if (hit is not null && hit.Length == s.Length && Pack(hit) == key) return new @string(hit);
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        Volatile.Write(ref slot, copy);
        return new @string(copy);
    }

    // V7: INSERT-ONLY open-addressed table (never evicts): a literal, once seen, hits for the life of
    // the process whatever else is converted -- deterministic under composition. Lock-free reads; an
    // insert takes a lock; capped so non-literal content cannot grow it without bound.
    static byte[]?[] s_io = new byte[]?[1 << 14];
    static int s_ioCount;
    static readonly object s_ioLock = new();

    public static @string InsertOnly(ReadOnlySpan<byte> s)
    {
        if ((uint)(s.Length - 1) >= 16) return s.Length == 0 ? new @string(Array.Empty<byte>()) : new @string(s);
        byte[]?[] table = Volatile.Read(ref s_io);
        int mask = table.Length - 1;
        for (int i = (int)Hash(s) & mask, probes = 0; probes < 8; i = (i + 1) & mask, probes++)
        {
            byte[]? e = Volatile.Read(ref table[i]);
            if (e is null) break;
            if (e.Length == s.Length && s.SequenceEqual(e)) return new @string(e);
        }
        byte[] copy = s.ToArray();
        AllocationCounter.Count();
        if (Volatile.Read(ref s_ioCount) < (1 << 13))
        {
            lock (s_ioLock)
            {
                for (int i = (int)Hash(s) & mask, probes = 0; probes < 8; i = (i + 1) & mask, probes++)
                {
                    byte[]? e = table[i];
                    if (e is null) { Volatile.Write(ref table[i], copy); s_ioCount++; break; }
                    if (e.Length == s.Length && s.SequenceEqual(e)) return new @string(e);
                }
            }
        }
        return new @string(copy);
    }
}
