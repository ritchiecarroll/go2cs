// Copyright 2018 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using bytes = bytes_package;
using rand = crypto.rand_package;
using hex = encoding.hex_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using sync = sync_package;
using crypto;
using encoding;

partial class uuid_package {

partial struct UUID /*[16]byte*/;

partial struct ΔVersion /*num:byte*/;

partial struct ΔVariant /*num:byte*/;

public static ΔVariant Invalid => /* Variant(iota) */ 0;
public static ΔVariant RFC4122 => 1;
public static ΔVariant Reserved => 2;
public static ΔVariant Microsoft => 3;
public static ΔVariant Future => 4;

internal static UntypedInt randPoolSize => /* 16 * 16 */ 256;

internal static io.Reader rander = rand.Reader;
internal static bool poolEnabled = false;
internal static ж<sync.Mutex> ᏑpoolMu = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex poolMu => ref ᏑpoolMu.Value;
internal static nint poolPos = randPoolSize;
internal static array<byte> pool = new(256);

partial struct invalidLengthError {
    internal nint len;
}

internal static @string Error(this invalidLengthError err) {
    return fmt.Sprintf("invalid UUID length: %d"u8, err.len);
}

public static bool IsInvalidLengthError(error err) {
    var (_, ok) = err._<invalidLengthError>(ᐧ);
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string urnUuidˢ = "urn:uuid:"u8;
internal static readonly @string invalidUuidFormatˢ = "invalid UUID format"u8;

public static (UUID, error) Parse(@string s) {
    UUID uuid = default!;
    var exprᴛ1 = len(s);
    if (exprᴛ1 is 36) {
    }
    else if (exprᴛ1 == 36 + 9) {
        if (!strings.EqualFold(s[..9], urnUuidˢ)) {
            return (uuid.Clone(), fmt.Errorf("invalid urn prefix: %q"u8, s[..9]));
        }
        s = s[9..];
    }
    else if (exprᴛ1 == 36 + 2) {
        s = s[1..];
    }
    else if (exprᴛ1 is 32) {
        bool ok = default!;
        foreach (var (i, _) in uuid) {
            (uuid[i], ok) = xtob(s[i * 2], s[i * 2 + 1]);
            if (!ok) {
                return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
            }
        }
        return (uuid.Clone(), default!);
    }
    else { /* default: */
        return (uuid.Clone(), new invalidLengthError(len(s)));
    }

    if (s[8] != (rune)'-' || s[13] != (rune)'-' || s[18] != (rune)'-' || s[23] != (rune)'-') {
        return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
    }
    foreach (var (i, x) in new nint[]{
        0, 2, 4, 6,
        9, 11,
        14, 16,
        19, 21,
        24, 26, 28, 30, 32, 34
    }.array()) {
        var (v, ok) = xtob(s[x], s[x + 1]);
        if (!ok) {
            return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
        }
        uuid[i] = v;
    }
    return (uuid.Clone(), default!);
}

public static (UUID, error) ParseBytes(slice<byte> b) {
    UUID uuid = default!;
    var exprᴛ1 = len(b);
    if (exprᴛ1 is 36) {
    }
    else if (exprᴛ1 == 36 + 9) {
        if (!bytes.EqualFold(b[..9], slice<byte>("urn:uuid:"u8))) {
            return (uuid.Clone(), fmt.Errorf("invalid urn prefix: %q"u8, b[..9]));
        }
        b = b[9..];
    }
    else if (exprᴛ1 == 36 + 2) {
        b = b[1..];
    }
    else if (exprᴛ1 is 32) {
        bool ok = default!;
        for (nint i = 0; i < 32; i += 2) {
            (uuid[i / 2], ok) = xtob(b[i], b[i + 1]);
            if (!ok) {
                return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
            }
        }
        return (uuid.Clone(), default!);
    }
    else { /* default: */
        return (uuid.Clone(), new invalidLengthError(len(b)));
    }

    if (b[8] != (rune)'-' || b[13] != (rune)'-' || b[18] != (rune)'-' || b[23] != (rune)'-') {
        return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
    }
    foreach (var (i, x) in new nint[]{
        0, 2, 4, 6,
        9, 11,
        14, 16,
        19, 21,
        24, 26, 28, 30, 32, 34
    }.array()) {
        var (v, ok) = xtob(b[x], b[x + 1]);
        if (!ok) {
            return (uuid.Clone(), errors.New(invalidUuidFormatˢ));
        }
        uuid[i] = v;
    }
    return (uuid.Clone(), default!);
}

public static UUID MustParse(@string s) {
    var (uuid, err) = Parse(s);
    if (err != default!) {
        throw panic(@"uuid: Parse(" + s + @"): " + err.Error());
    }
    return uuid.Clone();
}

public static (UUID uuid, error err) FromBytes(slice<byte> b) {
    UUID uuid = default!;
    error err = default!;

    err = uuid.UnmarshalBinary(b);
    return (uuid.Clone(), err);
}

public static UUID Must(UUID uuid, error err) {
    uuid = uuid.Clone();

    if (err != default!) {
        throw panic(err);
    }
    return uuid.Clone();
}

public static error Validate(@string s) {
    var exprᴛ1 = len(s);
    if (exprᴛ1 is 36) {
    }
    else if (exprᴛ1 == 36 + 9) {
        if (!strings.EqualFold(s[..9], urnUuidˢ)) {
            return fmt.Errorf("invalid urn prefix: %q"u8, s[..9]);
        }
        s = s[9..];
    }
    else if (exprᴛ1 == 36 + 2) {
        if (s[0] != (rune)'{' || s[len(s) - 1] != (rune)'}') {
            return fmt.Errorf("invalid bracketed UUID format"u8);
        }
        s = s.slice(1, len(s) - 1);
    }
    else if (exprᴛ1 is 32) {
        for (nint i = 0; i < len(s); i += 2) {
            var (_, ok) = xtob(s[i], s[i + 1]);
            if (!ok) {
                return errors.New(invalidUuidFormatˢ);
            }
        }
    }
    else { /* default: */
        return new invalidLengthError(len(s));
    }

    if (len(s) == 36) {
        if (s[8] != (rune)'-' || s[13] != (rune)'-' || s[18] != (rune)'-' || s[23] != (rune)'-') {
            return errors.New(invalidUuidFormatˢ);
        }
        foreach (var (_, x) in new nint[]{0, 2, 4, 6, 9, 11, 14, 16, 19, 21, 24, 26, 28, 30, 32, 34}.slice()) {
            {
                var (_, ok) = xtob(s[x], s[x + 1]); if (!ok) {
                    return errors.New(invalidUuidFormatˢ);
                }
            }
        }
    }
    return default!;
}

public static @string String(this UUID uuid) {
    uuid = uuid.Clone();

    array<byte> buf = new(36);
    encodeHex(buf[..], uuid);
    return ((@string)(buf[..]));
}

public static @string URN(this UUID uuid) {
    uuid = uuid.Clone();

    array<byte> buf = new(45); /* 36 + 9 */
    copy(buf[..], "urn:uuid:"u8);
    encodeHex(buf[9..], uuid);
    return ((@string)(buf[..]));
}

internal static void encodeHex(slice<byte> dst, UUID uuid) {
    uuid = uuid.Clone();

    hex.Encode(dst, uuid[..4]);
    dst[8] = (rune)'-';
    hex.Encode(dst[9..13], uuid[4..6]);
    dst[13] = (rune)'-';
    hex.Encode(dst[14..18], uuid[6..8]);
    dst[18] = (rune)'-';
    hex.Encode(dst[19..23], uuid[8..10]);
    dst[23] = (rune)'-';
    hex.Encode(dst[24..], uuid[10..]);
}

public static ΔVariant Variant(this UUID uuid) {
    uuid = uuid.Clone();

    switch (ᐧ) {
    case {} when ((byte)(uuid[8] & 0xc0)) == 0x80: {
        return RFC4122;
    }
    case {} when ((byte)(uuid[8] & 0xe0)) == 0xc0: {
        return Microsoft;
    }
    case {} when ((byte)(uuid[8] & 0xe0)) == 0xe0: {
        return Future;
    }
    default: {
        return Reserved;
    }}

}

public static ΔVersion Version(this UUID uuid) {
    uuid = uuid.Clone();

    return ((ΔVersion)((uuid[6] >> (int)(4))));
}

public static @string String(this ΔVersion v) {
    if (v > 15) {
        return fmt.Sprintf("BAD_VERSION_%d"u8, v);
    }
    return fmt.Sprintf("VERSION_%d"u8, v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rfc4122ˢ = "RFC4122"u8;
internal static readonly @string reservedˢ = "Reserved"u8;
internal static readonly @string microsoftˢ = "Microsoft"u8;
internal static readonly @string futureˢ = "Future"u8;
internal static readonly @string invalidˢ = "Invalid"u8;

public static @string String(this ΔVariant v) {
    var exprᴛ1 = v;
    if (exprᴛ1 == RFC4122) {
        return rfc4122ˢ;
    }
    if (exprᴛ1 == Reserved) {
        return reservedˢ;
    }
    if (exprᴛ1 == Microsoft) {
        return microsoftˢ;
    }
    if (exprᴛ1 == Future) {
        return futureˢ;
    }
    if (exprᴛ1 == Invalid) {
        return invalidˢ;
    }

    return fmt.Sprintf("BadVariant%d"u8, (nint)(byte)v);
}

public static void SetRand(io.Reader r) {
    if (r == default!) {
        rander = rand.Reader;
        return;
    }
    rander = r;
}

public static void EnableRandPool() {
    poolEnabled = true;
}

public static void DisableRandPool() {
    GoFrame ᒐ = default;
    try {
        poolEnabled = false;
        defer(ᏑpoolMu.Unlock, ref ᒐ);
        ᏑpoolMu.Lock();
        poolPos = randPoolSize;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct UUIDs /*[]UUID*/;

public static slice<@string> Strings(this UUIDs uuids) {
    slice<@string> uuidStrs = new slice<@string>(len(uuids));
    foreach (var (i, vᴛ1) in uuids) {
        var uuid = vᴛ1.Clone();

        uuidStrs[i] = uuid.String();
    }
    return uuidStrs;
}

} // end uuid_package
