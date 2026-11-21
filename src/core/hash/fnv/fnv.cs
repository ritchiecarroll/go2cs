// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fnv implements FNV-1 and FNV-1a, non-cryptographic hash functions
// created by Glenn Fowler, Landon Curt Noll, and Phong Vo.
// See
// https://en.wikipedia.org/wiki/Fowler-Noll-Vo_hash_function.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using bits = math.bits_package;
using @internal;
using math;
using static fnv_package;

partial class fnv_package {

partial struct sum32 /*num:uint32*/;

partial struct sum32a /*num:uint32*/;

partial struct sum64 /*num:uint64*/;

partial struct sum64a /*num:uint64*/;

partial struct sum128 /*[2]uint64*/;

partial struct sum128a /*[2]uint64*/;

internal static UntypedInt offset32 => 2166136261;
internal static UntypedInt offset64 => 14695981039346656037;
internal static UntypedInt offset128Lower => 0x62b821756295c58d;
internal static UntypedInt offset128Higher => 0x6c62272e07bb0142;
internal static UntypedInt prime32 => 16777619;
internal static UntypedInt prime64 => 1099511628211;
internal static UntypedInt prime128Lower => 0x13b;
internal static UntypedInt prime128Shift => 24;

// New32 returns a new 32-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32() {
    ref var s = ref heap(new sum32(), out var Ꮡs);
    s = offset32;
    return new sum32жHash32(Ꮡs);
}

// New32a returns a new 32-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32a() {
    ref var s = ref heap(new sum32a(), out var Ꮡs);
    s = offset32;
    return new sum32aжHash32(Ꮡs);
}

// New64 returns a new 64-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64() {
    ref var s = ref heap(new sum64(), out var Ꮡs);
    s = offset64;
    return new sum64жHash64(Ꮡs);
}

// New64a returns a new 64-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64a() {
    ref var s = ref heap(new sum64a(), out var Ꮡs);
    s = offset64;
    return new sum64aжHash64(Ꮡs);
}

// New128 returns a new 128-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128() {
    ref var s = ref heap(new sum128(), out var Ꮡs);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return new sum128жHash(Ꮡs);
}

// New128a returns a new 128-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128a() {
    ref var s = ref heap(new sum128a(), out var Ꮡs);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return new sum128aжHash(Ꮡs);
}

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum32 s) { s = offset32; }

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum32a s) { s = offset32; }

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum64 s) { s = offset64; }

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum64a s) { s = offset64; }

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum128 s) {
    s.Value[0] = offset128Higher;
    s.Value[1] = offset128Lower;
}

} partial class fnv_package {

[GoMethodBody("Reset")] internal static void Resetᴍ(ref sum128a s) {
    s.Value[0] = offset128Higher;
    s.Value[1] = offset128Lower;
}

} partial class fnv_package {

[GoMethodBody("Sum32")] internal static uint32 Sum32ᴍ(ref sum32 s) { return (uint32)(s); }

} partial class fnv_package {

[GoMethodBody("Sum32")] internal static uint32 Sum32ᴍ(ref sum32a s) { return (uint32)(s); }

} partial class fnv_package {

[GoMethodBody("Sum64")] internal static uint64 Sum64ᴍ(ref sum64 s) { return (uint64)(s); }

} partial class fnv_package {

[GoMethodBody("Sum64")] internal static uint64 Sum64ᴍ(ref sum64a s) { return (uint64)(s); }

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum32 s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in range(data)) {
        hash *= prime32;
        hash ^= (sum32)(((sum32)(uint32)c));
    }
    s = hash;
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum32a s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in range(data)) {
        hash ^= (sum32a)(((sum32a)(uint32)c));
        hash *= prime32;
    }
    s = hash;
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum64 s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in range(data)) {
        hash *= prime64;
        hash ^= (sum64)(((sum64)(uint64)c));
    }
    s = hash;
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum64a s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in range(data)) {
        hash ^= (sum64a)(((sum64a)(uint64)c));
        hash *= prime64;
    }
    s = hash;
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum128 s, slice<byte> data) {
    foreach (var (_, c) in range(data)) {
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s.Value[1]);
        s0 += (s.Value[1] << (int)(prime128Shift)) + (uint64)prime128Lower * s.Value[0];
        // Update the values
        s.Value[1] = s1;
        s.Value[0] = s0;
        s.Value[1] ^= (uint64)((uint64)c);
    }
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Write")] internal static (nint, error) Writeᴍ(ref sum128a s, slice<byte> data) {
    foreach (var (_, c) in range(data)) {
        s.Value[1] ^= (uint64)((uint64)c);
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s.Value[1]);
        s0 += (s.Value[1] << (int)(prime128Shift)) + (uint64)prime128Lower * s.Value[0];
        // Update the values
        s.Value[1] = s1;
        s.Value[0] = s0;
    }
    return (len(data), default);
}

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum32 s) { return 4; }

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum32a s) { return 4; }

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum64 s) { return 8; }

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum64a s) { return 8; }

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum128 s) { return 16; }

} partial class fnv_package {

[GoMethodBody("Size")] internal static nint Sizeᴍ(ref sum128a s) { return 16; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum32 s) { return 1; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum32a s) { return 1; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum64 s) { return 1; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum64a s) { return 1; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum128 s) { return 1; }

} partial class fnv_package {

[GoMethodBody("BlockSize")] internal static nint BlockSizeᴍ(ref sum128a s) { return 1; }

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum32 s, slice<byte> @in) {
    var v = (uint32)(s);
    return byteorder.BEAppendUint32(@in, v);
}

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum32a s, slice<byte> @in) {
    var v = (uint32)(s);
    return byteorder.BEAppendUint32(@in, v);
}

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum64 s, slice<byte> @in) {
    var v = (uint64)(s);
    return byteorder.BEAppendUint64(@in, v);
}

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum64a s, slice<byte> @in) {
    var v = (uint64)(s);
    return byteorder.BEAppendUint64(@in, v);
}

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum128 s, slice<byte> @in) {
    var ret = byteorder.BEAppendUint64(@in, s.Value[0]);
    return byteorder.BEAppendUint64(ret, s.Value[1]);
}

} partial class fnv_package {

[GoMethodBody("Sum")] internal static slice<byte> Sumᴍ(ref sum128a s, slice<byte> @in) {
    var ret = byteorder.BEAppendUint64(@in, s.Value[0]);
    return byteorder.BEAppendUint64(ret, s.Value[1]);
}

} partial class fnv_package {

internal static ref readonly @string magic32 => ref magic32ᵇ;
private static readonly @string magic32ᵇ = "fnv\x01"u8;
internal static ref readonly @string magic32a => ref magic32aᵇ;
private static readonly @string magic32aᵇ = "fnv\x02"u8;
internal static ref readonly @string magic64 => ref magic64ᵇ;
private static readonly @string magic64ᵇ = "fnv\x03"u8;
internal static ref readonly @string magic64a => ref magic64aᵇ;
private static readonly @string magic64aᵇ = "fnv\x04"u8;
internal static ref readonly @string magic128 => ref magic128ᵇ;
private static readonly @string magic128ᵇ = "fnv\x05"u8;
internal static ref readonly @string magic128a => ref magic128aᵇ;
private static readonly @string magic128aᵇ = "fnv\x06"u8;
internal const nint marshaledSize32 = /* len(magic32) + 4 */ 8;
internal const nint marshaledSize64 = /* len(magic64) + 8 */ 12;
internal const nint marshaledSize128 = /* len(magic128) + 8*2 */ 20;

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum32 s, slice<byte> b) {
    b = append(b, magic32.ꓸꓸꓸ);
    b = byteorder.BEAppendUint32(b, (uint32)(s));
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum32 s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize32));
}

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum32a s, slice<byte> b) {
    b = append(b, magic32a.ꓸꓸꓸ);
    b = byteorder.BEAppendUint32(b, (uint32)(s));
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum32a s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize32));
}

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum64 s, slice<byte> b) {
    b = append(b, magic64.ꓸꓸꓸ);
    b = byteorder.BEAppendUint64(b, (uint64)(s));
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum64 s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize64));
}

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum64a s, slice<byte> b) {
    b = append(b, magic64a.ꓸꓸꓸ);
    b = byteorder.BEAppendUint64(b, (uint64)(s));
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum64a s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize64));
}

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum128 s, slice<byte> b) {
    b = append(b, magic128.ꓸꓸꓸ);
    b = byteorder.BEAppendUint64(b, s.Value[0]);
    b = byteorder.BEAppendUint64(b, s.Value[1]);
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum128 s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize128));
}

} partial class fnv_package {

[GoMethodBody("AppendBinary")] internal static (slice<byte>, error) AppendBinaryᴍ(ref sum128a s, slice<byte> b) {
    b = append(b, magic128a.ꓸꓸꓸ);
    b = byteorder.BEAppendUint64(b, s.Value[0]);
    b = byteorder.BEAppendUint64(b, s.Value[1]);
    return (b, default);
}

[GoMethodBody("MarshalBinary")] internal static (slice<byte>, error) MarshalBinaryᴍ(ref sum128a s) {
    return s.AppendBinary(new slice<byte>(0, marshaledSize128));
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum32 => ref hashFnvInvalidHashStateˢᵇᴍsum32;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum32 = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum32 => ref hashFnvInvalidHashStateˢ2ᵇᴍsum32;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum32 = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum32 s, slice<byte> b) {
    if (len(b) < len(magic32) || ((sstring)(b[..(int)(len(magic32))])) != magic32) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum32);
    }
    if (len(b) != marshaledSize32) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum32);
    }
    s = (sum32)byteorder.BEUint32(b[4..]);
    return default;
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum32a => ref hashFnvInvalidHashStateˢᵇᴍsum32a;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum32a = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum32a => ref hashFnvInvalidHashStateˢ2ᵇᴍsum32a;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum32a = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum32a s, slice<byte> b) {
    if (len(b) < len(magic32a) || ((sstring)(b[..(int)(len(magic32a))])) != magic32a) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum32a);
    }
    if (len(b) != marshaledSize32) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum32a);
    }
    s = (sum32a)byteorder.BEUint32(b[4..]);
    return default;
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum64 => ref hashFnvInvalidHashStateˢᵇᴍsum64;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum64 = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum64 => ref hashFnvInvalidHashStateˢ2ᵇᴍsum64;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum64 = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum64 s, slice<byte> b) {
    if (len(b) < len(magic64) || ((sstring)(b[..(int)(len(magic64))])) != magic64) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum64);
    }
    if (len(b) != marshaledSize64) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum64);
    }
    s = (sum64)byteorder.BEUint64(b[4..]);
    return default;
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum64a => ref hashFnvInvalidHashStateˢᵇᴍsum64a;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum64a = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum64a => ref hashFnvInvalidHashStateˢ2ᵇᴍsum64a;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum64a = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum64a s, slice<byte> b) {
    if (len(b) < len(magic64a) || ((sstring)(b[..(int)(len(magic64a))])) != magic64a) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum64a);
    }
    if (len(b) != marshaledSize64) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum64a);
    }
    s = (sum64a)byteorder.BEUint64(b[4..]);
    return default;
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum128 => ref hashFnvInvalidHashStateˢᵇᴍsum128;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum128 = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum128 => ref hashFnvInvalidHashStateˢ2ᵇᴍsum128;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum128 = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum128 s, slice<byte> b) {
    if (len(b) < len(magic128) || ((sstring)(b[..(int)(len(magic128))])) != magic128) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum128);
    }
    if (len(b) != marshaledSize128) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum128);
    }
    s.Value[0] = byteorder.BEUint64(b[4..]);
    s.Value[1] = byteorder.BEUint64(b[12..]);
    return default;
}

} partial class fnv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashFnvInvalidHashStateˢᴍsum128a => ref hashFnvInvalidHashStateˢᵇᴍsum128a;
private static readonly @string hashFnvInvalidHashStateˢᵇᴍsum128a = "hash/fnv: invalid hash state identifier"u8;
internal static ref readonly @string hashFnvInvalidHashStateˢ2ᴍsum128a => ref hashFnvInvalidHashStateˢ2ᵇᴍsum128a;
private static readonly @string hashFnvInvalidHashStateˢ2ᵇᴍsum128a = "hash/fnv: invalid hash state size"u8;

[GoMethodBody("UnmarshalBinary")] internal static error UnmarshalBinaryᴍ(ref sum128a s, slice<byte> b) {
    if (len(b) < len(magic128a) || ((sstring)(b[..(int)(len(magic128a))])) != magic128a) {
        return errors.New(hashFnvInvalidHashStateˢᴍsum128a);
    }
    if (len(b) != marshaledSize128) {
        return errors.New(hashFnvInvalidHashStateˢ2ᴍsum128a);
    }
    s.Value[0] = byteorder.BEUint64(b[4..]);
    s.Value[1] = byteorder.BEUint64(b[12..]);
    return default;
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum32 d) {
    ref var r = ref heap<sum32>(d, out var Ꮡr);
    return (new sum32жCloner(Ꮡr), default);
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum32a d) {
    ref var r = ref heap<sum32a>(d, out var Ꮡr);
    return (new sum32aжCloner(Ꮡr), default);
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum64 d) {
    ref var r = ref heap<sum64>(d, out var Ꮡr);
    return (new sum64жCloner(Ꮡr), default);
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum64a d) {
    ref var r = ref heap<sum64a>(d, out var Ꮡr);
    return (new sum64aжCloner(Ꮡr), default);
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum128 d) {
    ref var r = ref heap<sum128>(d.ΔClone(), out var Ꮡr);
    return (new sum128жCloner(Ꮡr), default);
}

} partial class fnv_package {

[GoMethodBody("Clone")] internal static (hash.Cloner, error) Cloneᴍ(ref sum128a d) {
    ref var r = ref heap<sum128a>(d.ΔClone(), out var Ꮡr);
    return (new sum128aжCloner(Ꮡr), default);
}

}
