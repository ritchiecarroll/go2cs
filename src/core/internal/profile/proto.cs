// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file is a simple protocol buffer encoder and decoder.
//
// A protocol message must implement the message interface:
//   decoder() []decoder
//   encode(*buffer)
//
// The decode method returns a slice indexed by field number that gives the
// function to decode that field.
// The encode method encodes its receiver into the given buffer.
//
// The two methods are simple enough to be implemented by hand rather than
// by using a protocol compiler.
//
// See profile.go for examples of messages implementing this interface.
//
// There is no support for groups, message sets, or "has" bits.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;

partial class profile_package {

[GoType] partial struct buffer {
    internal nint field;
    internal nint typ;
    internal uint64 u64;
    internal slice<byte> data;
    internal array<byte> tmp = new(16);
}

// type Δdecoder is a methodless func type — rendered inline as its base delegate

[GoType] partial interface message {
    slice<Func<ж<buffer>, message, error>> decoder();
    void encode(ж<buffer> _);
}

internal static slice<byte> marshal(message m) {
    ref var b = ref heap(new buffer(), out var Ꮡb);
    m.encode(Ꮡb);
    return b.data;
}

internal static void encodeVarint(ref buffer b, uint64 x) {
    while (x >= 128) {
        b.data = append(b.data, (byte)((byte)x | 0x80));
        x >>= (int)(7);
    }
    b.data = append(b.data, (byte)x);
}

internal static void encodeLength(ref buffer b, nint tag, nint len) {
    encodeVarint(ref b, (uint64)(((uint64)tag << (int)(3)) | 2));
    encodeVarint(ref b, (uint64)len);
}

internal static void encodeUint64(ref buffer b, nint tag, uint64 x) {
    // append varint to b.data
    encodeVarint(ref b, (uint64)(((uint64)tag << (int)(3)) | 0));
    encodeVarint(ref b, x);
}

internal static void encodeUint64s(ref buffer b, nint tag, slice<uint64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
            encodeVarint(ref b, u);
        }
        nint n2 = len(b.data);
        encodeLength(ref b, tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data.slice(n2, n3));
        copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
        copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
        return;
    }
    foreach (var (_, u) in x) {
        encodeUint64(ref b, tag, u);
    }
}

internal static void encodeUint64Opt(ref buffer b, nint tag, uint64 x) {
    if (x == 0) {
        return;
    }
    encodeUint64(ref b, tag, x);
}

internal static void encodeInt64(ref buffer b, nint tag, int64 x) {
    var u = (uint64)x;
    encodeUint64(ref b, tag, u);
}

internal static void encodeInt64Opt(ref buffer b, nint tag, int64 x) {
    if (x == 0) {
        return;
    }
    encodeInt64(ref b, tag, x);
}

internal static void encodeInt64s(ref buffer b, nint tag, slice<int64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
            encodeVarint(ref b, (uint64)u);
        }
        nint n2 = len(b.data);
        encodeLength(ref b, tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data.slice(n2, n3));
        copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
        copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
        return;
    }
    foreach (var (_, u) in x) {
        encodeInt64(ref b, tag, u);
    }
}

internal static void encodeString(ref buffer b, nint tag, @string x) {
    encodeLength(ref b, tag, len(x));
    b.data = append(b.data, x.ꓸꓸꓸ);
}

internal static void encodeStrings(ref buffer b, nint tag, slice<@string> x) {
    foreach (var (_, s) in x) {
        encodeString(ref b, tag, s);
    }
}

internal static void encodeBool(ref buffer b, nint tag, bool x) {
    if (x){
        encodeUint64(ref b, tag, 1);
    } else {
        encodeUint64(ref b, tag, 0);
    }
}

internal static void encodeBoolOpt(ref buffer b, nint tag, bool x) {
    if (!x) {
        return;
    }
    encodeBool(ref b, tag, x);
}

internal static void encodeMessage(ж<buffer> Ꮡb, nint tag, message m) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n1 = len(b.data);
    m.encode(Ꮡb);
    nint n2 = len(b.data);
    encodeLength(ref (Ꮡb).DerefOrNull(), tag, n2 - n1);
    nint n3 = len(b.data);
    copy(b.tmp[..], b.data.slice(n2, n3));
    copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
    copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
}

internal static error /*err*/ unmarshal(slice<byte> data, message m) {
    ref var b = ref heap<buffer>(out var Ꮡb);
    b = new buffer(data: data, typ: 2);
    return decodeMessage(Ꮡb, m);
}

internal static uint64 le64(slice<byte> p) {
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)p[0] | ((uint64)p[1] << (int)(8))) | ((uint64)p[2] << (int)(16))) | ((uint64)p[3] << (int)(24))) | ((uint64)p[4] << (int)(32))) | ((uint64)p[5] << (int)(40))) | ((uint64)p[6] << (int)(48))) | ((uint64)p[7] << (int)(56)));
}

internal static uint32 le32(slice<byte> p) {
    return (uint32)((uint32)((uint32)((uint32)p[0] | ((uint32)p[1] << (int)(8))) | ((uint32)p[2] << (int)(16))) | ((uint32)p[3] << (int)(24)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badVarintˢ = "bad varint"u8;

internal static (uint64, slice<byte>, error) decodeVarint(slice<byte> data) {
    nint i = default!;
    uint64 u = default!;
    for (i = 0; ᐧ ; i++) {
        if (i >= 10 || i >= len(data)) {
            return (0, default!, errors.New(badVarintˢ));
        }
        u |= (uint64)(((uint64)((byte)(data[i] & 0x7F))).Lsh((nuint)(7 * i)));
        if ((byte)(data[i] & 0x80) == 0) {
            return (u, data.slice(i + 1), default!);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notEnoughDataˢ = "not enough data"u8;
internal static readonly @string tooMuchDataˢ = "too much data"u8;

internal static (slice<byte>, error) decodeField(ref buffer b, slice<byte> data) {
    (var x, data, var err) = decodeVarint(data);
    if (err != default!) {
        return (default!, err);
    }
    b.field = (nint)((x >> (int)(3)));
    b.typ = (nint)((uint64)(x & 7));
    b.data = default!;
    b.u64 = 0;
    switch (b.typ) {
    case 0: {
        (b.u64, data, err) = decodeVarint(data);
        if (err != default!) {
            return (default!, err);
        }
        break;
    }
    case 1: {
        if (len(data) < 8) {
            return (default!, errors.New(notEnoughDataˢ));
        }
        b.u64 = le64(data[..8]);
        data = data[8..];
        break;
    }
    case 2: {
        uint64 n = default!;
        (n, data, err) = decodeVarint(data);
        if (err != default!) {
            return (default!, err);
        }
        if (n > (uint64)len(data)) {
            return (default!, errors.New(tooMuchDataˢ));
        }
        b.data = data.slice(0, (nint)(n));
        data = data.slice((nint)(n));
        break;
    }
    case 5: {
        if (len(data) < 4) {
            return (default!, errors.New(notEnoughDataˢ));
        }
        b.u64 = (uint64)le32(data[..4]);
        data = data[4..];
        break;
    }
    default: {
        return (default!, fmt.Errorf("unknown wire type: %d"u8, b.typ));
    }}

    return (data, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeMismatchˢ = "type mismatch"u8;

internal static error checkType(ref buffer b, nint typ) {
    if (b.typ != typ) {
        return errors.New(typeMismatchˢ);
    }
    return default!;
}

internal static error decodeMessage(ж<buffer> Ꮡb, message m) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        var err = checkType(ref (Ꮡb).DerefOrNull(), 2); if (err != default!) {
            return err;
        }
    }
    var dec = m.decoder();
    var data = b.data;
    while (len(data) > 0) {
        // pull varint field# + type
        error err = default!;
        (data, err) = decodeField(ref (Ꮡb).DerefOrNull(), data);
        if (err != default!) {
            return err;
        }
        if (b.field >= len(dec) || dec[b.field] == default!) {
            continue;
        }
        {
            var errΔ1 = dec[b.field](Ꮡb, m); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    }
    return default!;
}

internal static error decodeInt64(ref buffer b, ж<int64> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    {
        var err = checkType(ref b, 0); if (err != default!) {
            return err;
        }
    }
    x = (int64)b.u64;
    return default!;
}

internal static error decodeInt64s(ref buffer b, ж<slice<int64>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (b.typ == 2) {
        // Packed encoding
        var data = b.data;
        while (len(data) > 0) {
            uint64 u = default!;
            error err = default!;
            {
                (u, data, err) = decodeVarint(data); if (err != default!) {
                    return err;
                }
            }
            x = append(x, (int64)u);
        }
        return default!;
    }
    ref var i = ref heap(new int64(), out var Ꮡi);
    {
        var err = decodeInt64(ref b, Ꮡi); if (err != default!) {
            return err;
        }
    }
    x = append(x, i);
    return default!;
}

internal static error decodeUint64(ref buffer b, ж<uint64> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    {
        var err = checkType(ref b, 0); if (err != default!) {
            return err;
        }
    }
    x = b.u64;
    return default!;
}

internal static error decodeUint64s(ref buffer b, ж<slice<uint64>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (b.typ == 2) {
        var data = b.data;
        // Packed encoding
        while (len(data) > 0) {
            uint64 uΔ1 = default!;
            error err = default!;
            {
                (uΔ1, data, err) = decodeVarint(data); if (err != default!) {
                    return err;
                }
            }
            x = append(x, uΔ1);
        }
        return default!;
    }
    ref var u = ref heap(new uint64(), out var Ꮡu);
    {
        var err = decodeUint64(ref b, Ꮡu); if (err != default!) {
            return err;
        }
    }
    x = append(x, u);
    return default!;
}

internal static error decodeString(ref buffer b, ref @string x) {
    {
        var err = checkType(ref b, 2); if (err != default!) {
            return err;
        }
    }
    x = ((@string)b.data);
    return default!;
}

internal static error decodeStrings(ref buffer b, ж<slice<@string>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    @string s = default!;
    {
        var err = decodeString(ref b, ref s); if (err != default!) {
            return err;
        }
    }
    x = append(x, s);
    return default!;
}

internal static error decodeBool(ref buffer b, ж<bool> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    {
        var err = checkType(ref b, 0); if (err != default!) {
            return err;
        }
    }
    if ((int64)b.u64 == 0){
        x = false;
    } else {
        x = true;
    }
    return default!;
}

} // end profile_package
