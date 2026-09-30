// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha3 implements the SHA-3 fixed-output-length hash functions and
// the SHAKE variable-output-length functions defined by [FIPS 202], as well as
// the cSHAKE extendable-output-length functions defined by [SP 800-185].
//
// [FIPS 202]: https://doi.org/10.6028/NIST.FIPS.202
// [SP 800-185]: https://doi.org/10.6028/NIST.SP.800-185
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class sha3_package {

[GoType("num:nint")] partial struct spongeDirection;

internal static spongeDirection spongeAbsorbing => /* iota */ 0;
internal static spongeDirection spongeSqueezing => 1;

[GoType] partial struct Digest {
    internal array<byte> a = new(1600 / 8); // main state of the hash
    // a[n:rate] is the buffer. If absorbing, it's the remaining space to XOR
    // into before running the permutation. If squeezing, it's the remaining
    // output to produce before running the permutation.
    internal nint n, rate;
    // dsbyte contains the "domain separation" bits and the first bit of
    // the padding. Sections 6.1 and 6.2 of [1] separate the outputs of the
    // SHA-3 and SHAKE functions by appending bitstrings to the message.
    // Using a little-endian bit-ordering convention, these are "01" for SHA-3
    // and "1111" for SHAKE, or 00000010b and 00001111b, respectively. Then the
    // padding rule from section 5.1 is applied to pad the message to a multiple
    // of the rate, which involves adding a "1" bit, zero or more "0" bits, and
    // a final "1" bit. We merge the first "1" bit from the padding into dsbyte,
    // giving 00000110b (0x06) and 00011111b (0x1f).
    // [1] http://csrc.nist.gov/publications/drafts/fips-202/fips_202_draft.pdf
    //     "Draft FIPS 202: SHA-3 Standard: Permutation-Based Hash and
    //      Extendable-Output Functions (May 2014)"
    internal byte dsbyte;
    internal nint outputLen;            // the default output size in bytes
    internal spongeDirection state; // whether the sponge is absorbing or squeezing
}

// BlockSize returns the rate of sponge underlying this hash function.
[GoRecv] public static nint BlockSize(this ref Digest d) {
    return d.rate;
}

// Size returns the output size of the hash function in bytes.
[GoRecv] public static nint Size(this ref Digest d) {
    return d.outputLen;
}

// Reset resets the Digest to its initial state.
[GoRecv] public static void Reset(this ref Digest d) {
    // Zero the permutation's state.
    foreach (var (i, _) in d.a) {
        d.a[i] = 0;
    }
    d.state = spongeAbsorbing;
    d.n = 0;
}

[GoRecv] public static ж<Digest> Clone(this ref Digest d) {
    ref var ret = ref heap<Digest>(out var Ꮡret);
    ret = d.ΔClone();
    return Ꮡret;
}

// permute applies the KeccakF-1600 permutation.
internal static void permute(this ж<Digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    keccakF1600(Ꮡd.of(Digest.Ꮡa));
    d.n = 0;
}

// padAndPermute appends the domain separation bits in dsbyte, applies
// the multi-bitrate 10..1 padding rule, and permutes the state.
internal static void padAndPermute(this ж<Digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Pad with this instance's domain-separator bits. We know that there's
    // at least one byte of space in the sponge because, if it were full,
    // permute would have been called to empty it. dsbyte also contains the
    // first one bit for the padding. See the comment in the state struct.
    d.a[d.n] ^= (byte)(d.dsbyte);
    // This adds the final one bit for the padding. Because of the way that
    // bits are numbered from the LSB upwards, the final bit is the MSB of
    // the last byte.
    d.a[d.rate - 1] ^= (byte)(0x80);
    // Apply the permutation
    Ꮡd.permute();
    d.state = spongeSqueezing;
}

// Write absorbs more data into the hash's state.
public static (nint n, error err) Write(this ж<Digest> Ꮡd, slice<byte> p) {
    return Ꮡd.write(p);
}

internal static (nint n, error err) writeGeneric(this ж<Digest> Ꮡd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    if (d.state != spongeAbsorbing) {
        throw panic("sha3: Write after Read");
    }
    n = len(p);
    while (len(p) > 0) {
        nint x = subtle.XORBytes(d.a.slice(d.n, d.rate), d.a.slice(d.n, d.rate), p);
        d.n += x;
        p = p.slice(x);
        // If the sponge is full, apply the permutation.
        if (d.n == d.rate) {
            Ꮡd.permute();
        }
    }
    return (n, err);
}

// read squeezes an arbitrary number of bytes from the sponge.
internal static (nint n, error err) readGeneric(this ж<Digest> Ꮡd, slice<byte> @out) {
    nint n = default!;
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    // If we're still absorbing, pad and apply the permutation.
    if (d.state == spongeAbsorbing) {
        Ꮡd.padAndPermute();
    }
    n = len(@out);
    // Now, do the squeezing.
    while (len(@out) > 0) {
        // Apply the permutation if we've squeezed the sponge dry.
        if (d.n == d.rate) {
            Ꮡd.permute();
        }
        nint x = copy(@out, d.a.slice(d.n, d.rate));
        d.n += x;
        @out = @out.slice(x);
    }
    return (n, err);
}

// Sum appends the current hash to b and returns the resulting slice.
// It does not change the underlying hash state.
[GoRecv] public static slice<byte> Sum(this ref Digest d, slice<byte> b) {
    fips140.RecordApproved();
    return d.sum(b);
}

[GoRecv] internal static slice<byte> sumGeneric(this ref Digest d, slice<byte> b) {
    if (d.state != spongeAbsorbing) {
        throw panic("sha3: Sum after Read");
    }
    // Make a copy of the original hash so that caller can keep writing
    // and summing.
    var dup = d.Clone();
    var hash = new slice<byte>((~dup).outputLen, 64); // explicit cap to allow stack allocation
    dup.read(hash);
    return appendꓸꓸꓸ(b, hash);
}

internal static readonly @string magicSHA3 = "sha\x08"u8;
internal static readonly @string magicShake = "sha\x09"u8;
internal static readonly @string magicCShake = "sha\x0a"u8;
internal static readonly @string magicKeccak = "sha\x0b"u8;
internal const nint marshaledSize = /* len(magicSHA3) + 1 + 200 + 1 + 1 */ 207;

[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref Digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

[GoRecv] public static (slice<byte>, error) AppendBinary(this ref Digest d, slice<byte> b) {
    var exprᴛ1 = d.dsbyte;
    if (exprᴛ1 == dsbyteSHA3) {
        b = append(b, magicSHA3.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == dsbyteShake) {
        b = append(b, magicShake.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == dsbyteCShake) {
        b = append(b, magicCShake.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == dsbyteKeccak) {
        b = append(b, magicKeccak.ꓸꓸꓸ);
    }
    else { /* default: */
        throw panic("unknown dsbyte");
    }

    // rate is at most 168, and n is at most rate.
    b = append(b, (byte)d.rate);
    b = appendꓸꓸꓸ(b, d.a[..]);
    b = append(b, (byte)d.n, (byte)(nint)d.state);
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sha3InvalidHashStateˢ = "sha3: invalid hash state"u8;
private static readonly @string sha3InvalidHashStateˢ2 = "sha3: invalid hash state identifier"u8;
private static readonly @string sha3InvalidHashStateˢ3 = "sha3: invalid hash state function"u8;

[GoRecv] public static error UnmarshalBinary(this ref Digest d, slice<byte> b) {
    if (len(b) != marshaledSize) {
        return errors.New(sha3InvalidHashStateˢ);
    }
    @string magic = ((@string)(b[..(int)(len(magicSHA3))]));
    b = b[(int)(len(magicSHA3))..];
    switch (ᐧ) {
    case {} when magic == magicSHA3 && d.dsbyte == dsbyteSHA3: {
        break;
    }
    case {} when magic == magicShake && d.dsbyte == dsbyteShake: {
        break;
    }
    case {} when magic == magicCShake && d.dsbyte == dsbyteCShake: {
        break;
    }
    case {} when magic == magicKeccak && d.dsbyte == dsbyteKeccak: {
        break;
    }
    default: {
        return errors.New(sha3InvalidHashStateˢ2);
    }}

    nint rate = (nint)b[0];
    b = b[1..];
    if (rate != d.rate) {
        return errors.New(sha3InvalidHashStateˢ3);
    }
    copy(d.a[..], b);
    b = b[(int)(len(d.a))..];
    nint n = (nint)b[0];
    spongeDirection state = ((spongeDirection)(nint)b[1]);
    if (n > d.rate) {
        return errors.New(sha3InvalidHashStateˢ);
    }
    d.n = n;
    if (state != spongeAbsorbing && state != spongeSqueezing) {
        return errors.New(sha3InvalidHashStateˢ);
    }
    d.state = state;
    return default!;
}

} // end sha3_package
