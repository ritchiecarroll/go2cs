// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements encoding/decoding of Ints.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;

partial class big_package {

// Gob codec version. Permits backward-compatible changes to the encoding.
internal const byte intGobVersion = 1;

// GobEncode implements the [encoding/gob.GobEncoder] interface.
public static (slice<byte>, error) GobEncode(this ж<ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return (default!, default!);
    }
    var buf = new slice<byte>(1 + len(x.abs) * (nint)_S); // extra byte for version and sign bit
    nint i = x.abs.bytes(buf) - 1; // i >= 0
    var b = (byte)((intGobVersion << (int)(1))); // make space for sign bit
    if (x.neg) {
        b |= (byte)(1);
    }
    buf[i] = b;
    return (buf.slice(i), default!);
}

// GobDecode implements the [encoding/gob.GobDecoder] interface.
[GoRecv] public static error GobDecode(this ref ΔInt z, slice<byte> buf) {
    if (len(buf) == 0) {
        // Other side sent a nil or default value.
        z = new ΔInt(nil);
        return default!;
    }
    var b = buf[0];
    if ((byte)((b >> (int)(1))) != intGobVersion) {
        return fmt.Errorf("Int.GobDecode: encoding version %d not supported"u8, (byte)((b >> (int)(1))));
    }
    z.neg = (byte)(b & 1) != 0;
    z.abs = z.abs.setBytes(buf[1..]);
    return default!;
}

// AppendText implements the [encoding.TextAppender] interface.
public static (slice<byte> text, error err) AppendText(this ж<ΔInt> Ꮡx, slice<byte> b) {
    return (Ꮡx.Append(b, 10), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface.
public static (slice<byte> text, error err) MarshalText(this ж<ΔInt> Ꮡx) {
    return Ꮡx.AppendText(default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
public static error UnmarshalText(this ж<ΔInt> Ꮡz, slice<byte> text) {
    {
        var (_, ok) = Ꮡz.setFromScanner(new bytes_ReaderжByteScanner(bytes_package.NewReader(text)), 0); if (!ok) {
            return fmt.Errorf("math/big: cannot unmarshal %q into a *big.Int"u8, text);
        }
    }
    return default!;
}

// The JSON marshalers are only here for API backward compatibility
// (programs that explicitly look for these two methods). JSON works
// fine with the TextMarshaler only.

// MarshalJSON implements the [encoding/json.Marshaler] interface.
public static (slice<byte>, error) MarshalJSON(this ж<ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return (slice<byte>("null"u8), default!);
    }
    return (x.abs.itoa(x.neg, 10), default!);
}

// UnmarshalJSON implements the [encoding/json.Unmarshaler] interface.
public static error UnmarshalJSON(this ж<ΔInt> Ꮡz, slice<byte> text) {
    // Ignore null, like in the main JSON package.
    if (((sstring)text) == "null"u8) {
        return default!;
    }
    return Ꮡz.UnmarshalText(text);
}

} // end big_package
