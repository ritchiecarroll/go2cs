// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using errors = errors_package;
using byteorder = @internal.byteorder_package;
using chacha8rand = @internal.chacha8rand_package;
using @internal;

partial class rand_package {

// A ChaCha8 is a ChaCha8-based cryptographically strong
// random number generator.
partial struct ChaCha8 {
    internal chacha8rand.State state;
    // The last readLen bytes of readBuf are still to be consumed by Read.
    internal array<byte> readBuf = new(8);
    internal nint readLen; // 0 <= readLen <= 8
}

// NewChaCha8 returns a new ChaCha8 seeded with the given seed.
public static ж<ChaCha8> NewChaCha8(/*[32]*/ array<byte> seed) {
    seed = seed.Clone();

    var c = @new<ChaCha8>();
    c.of(ChaCha8.Ꮡstate).Init(seed);
    return c;
}

// Seed resets the ChaCha8 to behave the same way as NewChaCha8(seed).
public static void Seed(this ж<ChaCha8> Ꮡc, /*[32]*/ array<byte> seed) {
    seed = seed.Clone();

    ref var c = ref Ꮡc.DerefOrNull();
    Ꮡc.of(ChaCha8.Ꮡstate).Init(seed);
    c.readLen = 0;
    c.readBuf = new byte[]{}.array(8);
}

// Uint64 returns a uniformly distributed random uint64 value.
public static uint64 Uint64(this ж<ChaCha8> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    while (ᐧ) {
        var (x, ok) = c.state.Next();
        if (ok) {
            return x;
        }
        Ꮡc.of(ChaCha8.Ꮡstate).Refill();
    }
}

// Read reads exactly len(p) bytes into p.
// It always returns len(p) and a nil error.
//
// If calls to Read and Uint64 are interleaved, the order in which bits are
// returned by the two is undefined, and Read may return bits generated before
// the last call to Uint64.
public static (nint n, error err) Read(this ж<ChaCha8> Ꮡc, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    if (c.readLen > 0) {
        n = copy(p, c.readBuf.slice(len(c.readBuf) - c.readLen));
        c.readLen -= n;
        p = p.slice(n);
    }
    while (len(p) >= 8) {
        byteorder.LEPutUint64(p, Ꮡc.Uint64());
        p = p[8..];
        n += 8;
    }
    if (len(p) > 0) {
        byteorder.LEPutUint64(c.readBuf[..], Ꮡc.Uint64());
        n += copy(p, c.readBuf[..]);
        c.readLen = 8 - len(p);
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidChaCha8ReadBufferˢ = "invalid ChaCha8 Read buffer encoding"u8;

// UnmarshalBinary implements the [encoding.BinaryUnmarshaler] interface.
public static error UnmarshalBinary(this ж<ChaCha8> Ꮡc, slice<byte> data) {
    ref var c = ref Ꮡc.DerefOrNull();

    (data, var ok) = cutPrefix(data, slice<byte>("readbuf:"u8));
    if (ok) {
        slice<byte> buf = default!;
        (buf, data, ok) = readUint8LengthPrefixed(data);
        if (!ok) {
            return errors.New(invalidChaCha8ReadBufferˢ);
        }
        c.readLen = copy(c.readBuf.slice(len(c.readBuf) - len(buf)), buf);
    }
    return chacha8rand.Unmarshal(Ꮡc.of(ChaCha8.Ꮡstate), data);
}

internal static (slice<byte> after, bool found) cutPrefix(slice<byte> s, slice<byte> prefix) {
    if (len(s) < len(prefix) || ((sstring)(s.slice(0, len(prefix)))) != ((sstring)prefix)) {
        return (s, false);
    }
    return (s.slice(len(prefix)), true);
}

internal static (slice<byte> buf, slice<byte> rest, bool ok) readUint8LengthPrefixed(slice<byte> b) {
    if (len(b) == 0 || len(b) < (nint)((byte)(1 + b[0]))) {
        return (default!, default!, false);
    }
    return (b.slice(1, (byte)(1 + b[0])), b.slice((byte)(1 + b[0])), true);
}

// AppendBinary implements the [encoding.BinaryAppender] interface.
public static (slice<byte>, error) AppendBinary(this ж<ChaCha8> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.readLen > 0) {
        b = append(b, ((@string)"readbuf:"u8).ꓸꓸꓸ);
        b = append(b, (uint8)c.readLen);
        b = appendꓸꓸꓸ(b, c.readBuf.slice(len(c.readBuf) - c.readLen));
    }
    return (appendꓸꓸꓸ(b, chacha8rand.Marshal(Ꮡc.of(ChaCha8.Ꮡstate))), default!);
}

// MarshalBinary implements the [encoding.BinaryMarshaler] interface.
public static (slice<byte>, error) MarshalBinary(this ж<ChaCha8> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    // the maximum length of (chacha8rand.Marshal + c.readBuf + "readbuf:") is 64
    return Ꮡc.AppendBinary(new slice<byte>(0, 64));
}

} // end rand_package
