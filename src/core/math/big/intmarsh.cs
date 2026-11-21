// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Ints.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using static big_package;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte intGobVersion = 1;

} partial class big_package {

// GobEncode implements the [encoding/gob.GobEncoder] interface.
[GoMethodBody("GobEncode")] internal static (slice<byte>, error) GobEncodeᴍ(ж<Int> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return (default, default);
    }
    var buf = new slice<byte>(1 + len(x.abs) * (nint)_S); // extra byte for version and sign bit
    nint i = x.abs.bytes(buf) - 1;                        // i >= 0
    var b = (byte)(intGobVersion << 1);                   // make space for sign bit
    if (x.neg) {
        b |= (byte)(1);
    }
    buf[i] = b;
    return (buf.slice(i), default);
}

// GobDecode implements the [encoding/gob.GobDecoder] interface.
[GoMethodBody("GobDecode")] internal static error GobDecodeᴍ(ref Int z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new Int();
        return default;
    }
    var b = buf[0];
    if ((byte)((b >> 1)) != intGobVersion) {
        return fmt.Errorf("Int.GobDecode: encoding version %d not supported"u8, (byte)((b >> 1)));
    }
    z.neg = (byte)(b & 1) != 0;
    z.abs = z.abs.setBytes(buf[1..]);
    return default;
}

// AppendText implements the [encoding.TextAppender] interface.
[GoMethodBody("AppendText")] internal static (slice<byte> text, error err) AppendTextᴍ(ж<Int> Ꮡx, slice<byte> b) {
    return (Ꮡx.Append(b, 10), default);
}

// MarshalText implements the [encoding.TextMarshaler] interface.
[GoMethodBody("MarshalText")] internal static (slice<byte> text, error err) MarshalTextᴍ(ж<Int> Ꮡx) {
    return Ꮡx.AppendText(default);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
[GoMethodBody("UnmarshalText")] internal static error UnmarshalTextᴍ(ж<Int> Ꮡz, slice<byte> text) {
    var (_, ok) = Ꮡz.setFromScanner(new bytes_ReaderжByteScanner(bytes.NewReader(text)), 0); if (!ok) {
        return fmt.Errorf("math/big: cannot unmarshal %q into a *big.Int"u8, text);
    }
    return default;
}

// The JSON marshalers are only here for API backward compatibility
// (programs that explicitly look for these two methods). JSON works
// fine with the TextMarshaler only.

// MarshalJSON implements the [encoding/json.Marshaler] interface.
[GoMethodBody("MarshalJSON")] internal static (slice<byte>, error) MarshalJSONᴍ(ж<Int> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return (slice<byte>("null"u8), default);
    }
    return (x.abs.itoa(x.neg, 10), default);
}

// UnmarshalJSON implements the [encoding/json.Unmarshaler] interface.
[GoMethodBody("UnmarshalJSON")] internal static error UnmarshalJSONᴍ(ж<Int> Ꮡz, slice<byte> text) {
    // Ignore null, like in the main JSON package.
    if (((sstring)text) == "null"u8) {
        return default;
    }
    return Ꮡz.UnmarshalText(text);
}

}
