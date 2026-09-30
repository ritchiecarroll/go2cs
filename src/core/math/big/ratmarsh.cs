// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Rats.
namespace go.math;

using errors = errors_package;
using fmt = fmt_package;
using byteorder = @internal.byteorder_package;
using math = math_package;
using @internal;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte ratGobVersion = 1;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ratGobEncodeNumeratorTooˢ = "Rat.GobEncode: numerator too large"u8;

// GobEncode implements the [encoding/gob.GobEncoder] interface.
public static (slice<byte>, error) GobEncode(this ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return (default!, default!);
    }
    var buf = new slice<byte>(1 + 4 + (len(x.a.abs) + len(x.b.abs)) * (nint)_S); // extra bytes for version and sign bit (1), and numerator length (4)
    nint i = x.b.abs.bytes(buf);
    nint j = x.a.abs.bytes(buf.slice(0, i));
    nint n = i - j;
    if ((nint)(uint32)n != n) {
        // this should never happen
        return (default!, errors.New(ratGobEncodeNumeratorTooˢ));
    }
    byteorder.BEPutUint32(buf.slice(j - 4, j), (uint32)n);
    j -= 1 + 4;
    var b = (byte)((ratGobVersion << (int)(1))); // make space for sign bit
    if (x.a.neg) {
        b |= (byte)(1);
    }
    buf[j] = b;
    return (buf.slice(j), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ratGobDecodeBufferTooˢ = "Rat.GobDecode: buffer too small"u8;
internal static readonly @string ratGobDecodeInvalidˢ = "Rat.GobDecode: invalid length"u8;

// GobDecode implements the [encoding/gob.GobDecoder] interface.
[GoRecv] public static error GobDecode(this ref ΔRat z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new ΔRat(nil);
        return default!;
    }
    if (len(buf) < 5) {
        return errors.New(ratGobDecodeBufferTooˢ);
    }
    var b = buf[0];
    if ((byte)((b >> (int)(1))) != ratGobVersion) {
        return fmt.Errorf("Rat.GobDecode: encoding version %d not supported"u8, (byte)((b >> (int)(1))));
    }
    UntypedInt j = /* 1 + 4 */ 5;
    var ln = byteorder.BEUint32(buf[(int)(j - 4)..(int)(j)]);
    if ((uint64)ln > (uint64)(math.MaxInt - j)) {
        return errors.New(ratGobDecodeInvalidˢ);
    }
    nint i = (nint)j + (nint)ln;
    if (len(buf) < i) {
        return errors.New(ratGobDecodeBufferTooˢ);
    }
    z.a.neg = (byte)(b & 1) != 0;
    z.a.abs = z.a.abs.setBytes(buf.slice(j, i));
    z.b.abs = z.b.abs.setBytes(buf.slice(i));
    return default!;
}

// AppendText implements the [encoding.TextAppender] interface.
public static (slice<byte>, error) AppendText(this ж<ΔRat> Ꮡx, slice<byte> b) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.IsInt()) {
        return Ꮡx.of(big_package.ΔRat.Ꮡa).AppendText(b);
    }
    return (Ꮡx.marshal(b), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface.
public static (slice<byte> text, error err) MarshalText(this ж<ΔRat> Ꮡx) {
    return Ꮡx.AppendText(default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
public static error UnmarshalText(this ж<ΔRat> Ꮡz, slice<byte> text) {
    // TODO(gri): get rid of the []byte/string conversion
    {
        var (_, ok) = Ꮡz.SetString(((@string)text)); if (!ok) {
            return fmt.Errorf("math/big: cannot unmarshal %q into a *big.Rat"u8, text);
        }
    }
    return default!;
}

} // end big_package
