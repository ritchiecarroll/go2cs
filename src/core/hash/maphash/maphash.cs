// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maphash provides hash functions on byte sequences and comparable values.
// These hash functions are intended to be used to implement hash tables or
// other data structures that need to map arbitrary strings or byte
// sequences to a uniform distribution on unsigned 64-bit integers.
// Each different instance of a hash table or data structure should use its own [Seed].
//
// The hash functions are not cryptographically secure.
// (See crypto/sha256 and crypto/sha512 for cryptographic use.)
namespace go.hash;

using byteorder = @internal.byteorder_package;
using math = math_package;
using @internal;

partial class maphash_package {

// A Seed is a random value that selects the specific hash function
// computed by a [Hash]. If two Hashes use the same Seeds, they
// will compute the same hash values for any given input.
// If two Hashes use different Seeds, they are very likely to compute
// distinct hash values for any given input.
//
// A Seed must be initialized by calling [MakeSeed].
// The zero seed is uninitialized and not valid for use with [Hash]'s SetSeed method.
//
// Each Seed value is local to a single process and cannot be serialized
// or otherwise recreated in a different process.
[GoType] partial struct ΔSeed {
    internal uint64 s;
}

// Bytes returns the hash of b with the given seed.
//
// Bytes is equivalent to, but more convenient and efficient than:
//
//	var h Hash
//	h.SetSeed(seed)
//	h.Write(b)
//	return h.Sum64()
public static uint64 Bytes(ΔSeed seed, slice<byte> b) {
    var state = seed.s;
    if (state == 0) {
        throw panic("maphash: use of uninitialized Seed");
    }
    if (len(b) > bufSize) {
        b = b.slice(0, len(b), len(b)); // merge len and cap calculations when reslicing
        while (len(b) > bufSize) {
            state = rthash(b[..(int)(bufSize)], state);
            b = b[(int)(bufSize)..];
        }
    }
    return rthash(b, state);
}

// String returns the hash of s with the given seed.
//
// String is equivalent to, but more convenient and efficient than:
//
//	var h Hash
//	h.SetSeed(seed)
//	h.WriteString(s)
//	return h.Sum64()
public static uint64 String(ΔSeed seed, @string s) {
    var state = seed.s;
    if (state == 0) {
        throw panic("maphash: use of uninitialized Seed");
    }
    while (len(s) > bufSize) {
        state = rthashString(s[..(int)(bufSize)], state);
        s = s[(int)(bufSize)..];
    }
    return rthashString(s, state);
}

// A Hash computes a seeded hash of a byte sequence.
//
// The zero Hash is a valid Hash ready to use.
// A zero Hash chooses a random seed for itself during
// the first call to a Reset, Write, Seed, or Sum64 method.
// For control over the seed, use SetSeed.
//
// The computed hash values depend only on the initial seed and
// the sequence of bytes provided to the Hash object, not on the way
// in which the bytes are provided. For example, the three sequences
//
//	h.Write([]byte{'f','o','o'})
//	h.WriteByte('f'); h.WriteByte('o'); h.WriteByte('o')
//	h.WriteString("foo")
//
// all have the same effect.
//
// Hashes are intended to be collision-resistant, even for situations
// where an adversary controls the byte sequences being hashed.
//
// A Hash is not safe for concurrent use by multiple goroutines, but a Seed is.
// If multiple goroutines must compute the same seeded hash,
// each can declare its own Hash and call SetSeed with a common Seed.
[GoType] partial struct Hash {
    internal array<Action> _ = new(0); // not comparable
    internal ΔSeed seed;          // initial seed used for this hash
    internal ΔSeed state;          // current hash of all flushed bytes
    internal array<byte> buf = new(bufSize); // unflushed byte buffer
    internal nint n;          // number of unflushed bytes
}

// bufSize is the size of the Hash write buffer.
// The buffer ensures that writes depend only on the sequence of bytes,
// not the sequence of WriteByte/Write/WriteString calls,
// by always calling rthash with a full buffer (except for the tail).
internal static UntypedInt bufSize => 128;

// initSeed seeds the hash if necessary.
// initSeed is called lazily before any operation that actually uses h.seed/h.state.
// Note that this does not include Write/WriteByte/WriteString in the case
// where they only add to h.buf. (If they write too much, they call h.flush,
// which does call h.initSeed.)
[GoRecv] internal static void initSeed(this ref Hash h) {
    if (h.seed.s == 0) {
        var seed = MakeSeed();
        h.seed = seed;
        h.state = seed;
    }
}

// WriteByte adds b to the sequence of bytes hashed by h.
// It never fails; the error result is for implementing [io.ByteWriter].
[GoRecv] public static error WriteByte(this ref Hash h, byte b) {
    if (h.n == len(h.buf)) {
        h.flush();
    }
    h.buf[h.n] = b;
    h.n++;
    return default!;
}

// Write adds b to the sequence of bytes hashed by h.
// It always writes all of b and never fails; the count and error result are for implementing [io.Writer].
[GoRecv] public static (nint, error) Write(this ref Hash h, slice<byte> b) {
    nint size = len(b);
    // Deal with bytes left over in h.buf.
    // h.n <= bufSize is always true.
    // Checking it is ~free and it lets the compiler eliminate a bounds check.
    if (h.n > 0 && h.n <= bufSize) {
        nint k = copy(h.buf.slice(h.n), b);
        h.n += k;
        if (h.n < bufSize) {
            // Copied the entirety of b to h.buf.
            return (size, default!);
        }
        b = b.slice(k);
        h.flush();
    }
    // No need to set h.n = 0 here; it happens just before exit.
    // Process as many full buffers as possible, without copying, and calling initSeed only once.
    if (len(b) > bufSize) {
        h.initSeed();
        while (len(b) > bufSize) {
            h.state.s = rthash(b[..(int)(bufSize)], h.state.s);
            b = b[(int)(bufSize)..];
        }
    }
    // Copy the tail.
    copy(h.buf[..], b);
    h.n = len(b);
    return (size, default!);
}

// WriteString adds the bytes of s to the sequence of bytes hashed by h.
// It always writes all of s and never fails; the count and error result are for implementing [io.StringWriter].
[GoRecv] public static (nint, error) WriteString(this ref Hash h, @string s) {
    // WriteString mirrors Write. See Write for comments.
    nint size = len(s);
    if (h.n > 0 && h.n <= bufSize) {
        nint k = copy(h.buf.slice(h.n), s);
        h.n += k;
        if (h.n < bufSize) {
            return (size, default!);
        }
        s = s.slice(k);
        h.flush();
    }
    if (len(s) > bufSize) {
        h.initSeed();
        while (len(s) > bufSize) {
            h.state.s = rthashString(s[..(int)(bufSize)], h.state.s);
            s = s[(int)(bufSize)..];
        }
    }
    copy(h.buf[..], s);
    h.n = len(s);
    return (size, default!);
}

// Seed returns h's seed value.
[GoRecv] public static ΔSeed Seed(this ref Hash h) {
    h.initSeed();
    return h.seed;
}

// SetSeed sets h to use seed, which must have been returned by [MakeSeed]
// or by another [Hash.Seed] method.
// Two [Hash] objects with the same seed behave identically.
// Two [Hash] objects with different seeds will very likely behave differently.
// Any bytes added to h before this call will be discarded.
[GoRecv] public static void SetSeed(this ref Hash h, ΔSeed seed) {
    if (seed.s == 0) {
        throw panic("maphash: use of uninitialized Seed");
    }
    h.seed = seed;
    h.state = seed;
    h.n = 0;
}

// Reset discards all bytes added to h.
// (The seed remains the same.)
[GoRecv] public static void Reset(this ref Hash h) {
    h.initSeed();
    h.state = h.seed;
    h.n = 0;
}

// precondition: buffer is full.
[GoRecv] internal static void flush(this ref Hash h) {
    if (h.n != len(h.buf)) {
        throw panic("maphash: flush of partially full buffer");
    }
    h.initSeed();
    h.state.s = rthash(h.buf.slice(0, h.n), h.state.s);
    h.n = 0;
}

// Sum64 returns h's current 64-bit value, which depends on
// h's seed and the sequence of bytes added to h since the
// last call to [Hash.Reset] or [Hash.SetSeed].
//
// All bits of the Sum64 result are close to uniformly and
// independently distributed, so it can be safely reduced
// by using bit masking, shifting, or modular arithmetic.
[GoRecv] public static uint64 Sum64(this ref Hash h) {
    h.initSeed();
    return rthash(h.buf.slice(0, h.n), h.state.s);
}

// MakeSeed returns a new random seed.
public static ΔSeed MakeSeed() {
    uint64 s = default!;
    while (ᐧ) {
        s = randUint64();
        // We use seed 0 to indicate an uninitialized seed/hash,
        // so keep trying until we get a non-zero seed.
        if (s != 0) {
            break;
        }
    }
    return new ΔSeed(s: s);
}

// Sum appends the hash's current 64-bit value to b.
// It exists for implementing [hash.Hash].
// For direct calls, it is more efficient to use [Hash.Sum64].
[GoRecv] public static slice<byte> Sum(this ref Hash h, slice<byte> b) {
    var x = h.Sum64();
    return append(b,
        (byte)((x >> (int)(0))),
        (byte)((x >> (int)(8))),
        (byte)((x >> (int)(16))),
        (byte)((x >> (int)(24))),
        (byte)((x >> (int)(32))),
        (byte)((x >> (int)(40))),
        (byte)((x >> (int)(48))),
        (byte)((x >> (int)(56))));
}

// Size returns h's hash value size, 8 bytes.
[GoRecv] public static nint Size(this ref Hash h) {
    return 8;
}

// BlockSize returns h's block size.
[GoRecv] public static nint BlockSize(this ref Hash h) {
    return len(h.buf);
}

// Comparable returns the hash of comparable value v with the given seed
// such that Comparable(s, v1) == Comparable(s, v2) if v1 == v2.
// If v != v, then the resulting hash is randomly distributed.
public static uint64 Comparable<T>(ΔSeed seed, T v) {
    escapeForHash(v);
    return comparableHash(v, seed);
}

// escapeForHash forces v to be on the heap, if v contains a
// non-string pointer. We cannot hash pointers to local variables,
// as the address of the local variable might change on stack growth.
// Strings are okay as the hash depends on only the content, not
// the pointer.
//
// This is essentially
//
//	if hasNonStringPointers(T) { abi.Escape(v) }
//
// Implemented as a compiler intrinsic.
internal static void escapeForHash<T>(T v) {
    // Go COMPILER INTRINSIC: the gc compiler replaces every call, so the written `panic("intrinsic")` body is unreachable by construction and has no CLR counterpart.
}

// WriteComparable adds x to the data hashed by h.
public static void WriteComparable<T>(ж<Hash> Ꮡh, T x) {
    ref var h = ref Ꮡh.DerefOrNull();

    escapeForHash(x);
    // writeComparable (not in purego mode) directly operates on h.state
    // without using h.buf. Mix in the buffer length so it won't
    // commute with a buffered write, which either changes h.n or changes
    // h.state.
    if (h.n != 0) {
        writeComparable(Ꮡh, h.n);
    }
    writeComparable(Ꮡh, x);
}

[GoRecv] internal static void float64(this ref Hash h, float64 f) {
    if (f == 0D) {
        h.WriteByte(0);
        return;
    }
    array<byte> buf = new(8);
    if (f != f) {
        byteorder.LEPutUint64(buf[..], randUint64());
        h.Write(buf[..]);
        return;
    }
    byteorder.LEPutUint64(buf[..], math.Float64bits(f));
    h.Write(buf[..]);
}

internal static byte btoi(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

} // end maphash_package
