// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asn1 implements parsing of DER-encoded ASN.1 data structures,
// as defined in ITU-T Rec X.690.
//
// See also “A Layman's Guide to a Subset of ASN.1, BER, and DER,”
// http://luca.ntop.org/Teaching/Appunti/asn1.html.
namespace go.encoding;

// ASN.1 is a syntax for specifying abstract objects and BER, DER, PER, XER etc
// are different encoding formats for those objects. Here, we'll be dealing
// with DER, the Distinguished Encoding Rules. DER is used in X.509 because
// it's fast to parse and, unlike BER, has a unique encoding for every object.
// When calculating hashes over objects, it's important that the resulting
// bytes be the same at both ends and DER removes this margin of error.
//
// ASN.1 is very complex and this package doesn't attempt to implement
// everything by any means.
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using math = math_package;
using big = go.math.big_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @internal;
using go.math;
using unicode;

partial class asn1_package {

// A StructuralError suggests that the ASN.1 data is valid, but the Go type
// which is receiving it doesn't match.
[GoType] partial struct StructuralError {
    public @string Msg;
}

public static @string Error(this StructuralError e) {
    return "asn1: structure error: "u8 + e.Msg;
}

// A SyntaxError suggests that the ASN.1 data is invalid.
[GoType] partial struct SyntaxError {
    public @string Msg;
}

public static @string Error(this SyntaxError e) {
    return "asn1: syntax error: "u8 + e.Msg;
}

// We start by dealing with each of the primitive types in turn.
// BOOLEAN
internal static (bool ret, error err) parseBool(slice<byte> bytes) {
    bool ret = default!;
    error err = default!;

    if (len(bytes) != 1) {
        err = new SyntaxError("invalid boolean"u8);
        return (ret, err);
    }
    // DER demands that "If the encoding represents the boolean value TRUE,
    // its single contents octet shall have all eight bits set to one."
    // Thus only 0 and 255 are valid encoded values.
    switch (bytes[0]) {
    case 0: {
        ret = false;
        break;
    }
    case 0xff: {
        ret = true;
        break;
    }
    default: {
        err = new SyntaxError("invalid boolean"u8);
        break;
    }}

    return (ret, err);
}

// INTEGER

// checkInteger returns nil if the given bytes are a valid DER-encoded
// INTEGER and an error otherwise.
internal static error checkInteger(slice<byte> bytes) {
    if (len(bytes) == 0) {
        return new StructuralError("empty integer"u8);
    }
    if (len(bytes) == 1) {
        return default!;
    }
    if ((bytes[0] == 0 && (byte)(bytes[1] & 0x80) == 0) || (bytes[0] == 0xff && (byte)(bytes[1] & 0x80) == 0x80)) {
        return new StructuralError("integer not minimally-encoded"u8);
    }
    return default!;
}

// parseInt64 treats the given bytes as a big-endian, signed integer and
// returns the result.
internal static (int64 ret, error err) parseInt64(slice<byte> bytes) {
    int64 ret = default!;
    error err = default!;

    err = checkInteger(bytes);
    if (err != default!) {
        return (ret, err);
    }
    if (len(bytes) > 8) {
        // We'll overflow an int64 in this case.
        err = new StructuralError("integer too large"u8);
        return (ret, err);
    }
    for (nint bytesRead = 0; bytesRead < len(bytes); bytesRead++) {
        ret <<= (int)(8);
        ret |= (int64)((int64)bytes[bytesRead]);
    }
    // Shift up and down in order to sign extend the result.
    ret.LshAssign((uint64)((uint8)(64 - (uint8)len(bytes) * 8)));
    ret.RshAssign((uint64)((uint8)(64 - (uint8)len(bytes) * 8)));
    return (ret, err);
}

// parseInt32 treats the given bytes as a big-endian, signed integer and returns
// the result.
internal static (int32, error) parseInt32(slice<byte> bytes) {
    {
        var errΔ1 = checkInteger(bytes); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    var (ret64, err) = parseInt64(bytes);
    if (err != default!) {
        return (0, err);
    }
    if (ret64 != (int64)(int32)ret64) {
        return (0, new StructuralError("integer too large"u8));
    }
    return ((int32)ret64, default!);
}

internal static ж<bigꓸInt> bigOne = big.NewInt(1);

// parseBigInt treats the given bytes as a big-endian, signed integer and returns
// the result.
internal static (ж<bigꓸInt>, error) parseBigInt(slice<byte> bytes) {
    {
        var err = checkInteger(bytes); if (err != default!) {
            return (default!, err);
        }
    }
    var ret = @new<bigꓸInt>();
    if (len(bytes) > 0 && (byte)(bytes[0] & 0x80) == 0x80) {
        // This is a negative number.
        var notBytes = new slice<byte>(len(bytes));
        foreach (var (i, _) in notBytes) {
            notBytes[i] = (byte)(((byte)(~bytes[i])));
        }
        ret.SetBytes(notBytes);
        ret.Add(ret, bigOne);
        ret.Neg(ret);
        return (ret, default!);
    }
    ret.SetBytes(bytes);
    return (ret, default!);
}

// BIT STRING

// BitString is the structure to use when you want an ASN.1 BIT STRING type. A
// bit string is padded up to the nearest byte in memory and the number of
// valid bits is recorded. Padding bits will be zero.
[GoType] partial struct BitString {
    public slice<byte> Bytes; // bits packed into bytes.
    public nint BitLength;   // length in bits.
}

// At returns the bit at the given index. If the index is out of range it
// returns 0.
public static nint At(this BitString b, nint i) {
    if (i < 0 || i >= b.BitLength) {
        return 0;
    }
    nint x = i / 8;
    nuint y = 7 - (nuint)(i % 8);
    return (nint)((nint)(b.Bytes[x].Rsh(y)) & 1);
}

// RightAlign returns a slice where the padding bits are at the beginning. The
// slice may share memory with the BitString.
public static slice<byte> RightAlign(this BitString b) {
    nuint shift = (nuint)(8 - (b.BitLength % 8));
    if (shift == 8 || len(b.Bytes) == 0) {
        return b.Bytes;
    }
    var a = new slice<byte>(len(b.Bytes));
    a[0] = (byte)(b.Bytes[0].Rsh(shift));
    for (nint i = 1; i < len(b.Bytes); i++) {
        a[i] = (byte)(b.Bytes[i - 1].Lsh((8 - shift)));
        a[i] |= (byte)(b.Bytes[i].Rsh(shift));
    }
    return a;
}

// parseBitString parses an ASN.1 bit string from the given byte slice and returns it.
internal static (BitString ret, error err) parseBitString(slice<byte> bytes) {
    BitString ret = default!;
    error err = default!;

    if (len(bytes) == 0) {
        err = new SyntaxError("zero length BIT STRING"u8);
        return (ret, err);
    }
    nint paddingBits = (nint)bytes[0];
    if (paddingBits > 7 || len(bytes) == 1 && paddingBits > 0 || (byte)(bytes[len(bytes) - 1] & ((((byte)1).Lsh((uint64)(bytes[0]))) - 1)) != 0) {
        err = new SyntaxError("invalid padding bits in BIT STRING"u8);
        return (ret, err);
    }
    ret.BitLength = (len(bytes) - 1) * 8 - paddingBits;
    ret.Bytes = bytes[1..];
    return (ret, err);
}

// NULL

// NullRawValue is a [RawValue] with its Tag set to the ASN.1 NULL type tag (5).
public static RawValue NullRawValue = new RawValue(Tag: TagNull);

// NullBytes contains bytes representing the DER-encoded ASN.1 NULL type.
public static slice<byte> NullBytes = new byte[]{TagNull, 0}.slice();

[GoType("[]nint")] partial struct ObjectIdentifier;

// OBJECT IDENTIFIER

// Equal reports whether oi and other represent the same identifier.
public static bool Equal(this ObjectIdentifier oi, ObjectIdentifier other) {
    return slices.Equal<ObjectIdentifier, nint>(oi, other);
}

public static @string String(this ObjectIdentifier oi) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    Ꮡs.Grow(32);
    var buf = new slice<byte>(0, 19);
    foreach (var (i, v) in oi) {
        if (i > 0) {
            Ꮡs.WriteByte((rune)'.');
        }
        Ꮡs.Write(strconv.AppendInt(buf, (int64)v, 10));
    }
    return s.String();
}

// parseObjectIdentifier parses an OBJECT IDENTIFIER from the given bytes and
// returns it. An object identifier is a sequence of variable length integers
// that are assigned in a hierarchy.
internal static (ObjectIdentifier s, error err) parseObjectIdentifier(slice<byte> bytes) {
    ObjectIdentifier s = default!;
    error err = default!;

    if (len(bytes) == 0) {
        err = new SyntaxError("zero length OBJECT IDENTIFIER"u8);
        return (s, err);
    }
    // In the worst case, we get two elements from the first byte (which is
    // encoded differently) and then every varint is a single byte long.
    s = new slice<nint>(len(bytes) + 1);
    // The first varint is 40*value1 + value2:
    // According to this packing, value1 can take the values 0, 1 and 2 only.
    // When value1 = 0 or value1 = 1, then value2 is <= 39. When value1 = 2,
    // then there are no restrictions on value2.
    (var v, var offset, err) = parseBase128Int(bytes, 0);
    if (err != default!) {
        return (s, err);
    }
    if (v < 80){
        s[0] = v / 40;
        s[1] = v % 40;
    } else {
        s[0] = 2;
        s[1] = v - 80;
    }
    nint i = 2;
    for (; offset < len(bytes); i++) {
        (v, offset, err) = parseBase128Int(bytes, offset);
        if (err != default!) {
            return (s, err);
        }
        s[i] = v;
    }
    s = s.slice(0, i);
    return (s, err);
}

[GoType("num:nint")] partial struct Enumerated;

[GoType("bool")] partial struct Flag;

// ENUMERATED
// FLAG

// parseBase128Int parses a base-128 encoded int from the given offset in the
// given byte slice. It returns the value and the new offset.
internal static (nint ret, nint offset, error err) parseBase128Int(slice<byte> bytes, nint initOffset) {
    nint ret = default!;
    nint offset = default!;
    error err = default!;

    offset = initOffset;
    int64 ret64 = default!;
    for (nint shifted = 0; offset < len(bytes); shifted++) {
        // 5 * 7 bits per byte == 35 bits of data
        // Thus the representation is either non-minimal or too large for an int32
        if (shifted == 5) {
            err = new StructuralError("base 128 integer too large"u8);
            return (ret, offset, err);
        }
        ret64 <<= (int)(7);
        var b = bytes[offset];
        // integers should be minimally encoded, so the leading octet should
        // never be 0x80
        if (shifted == 0 && b == 0x80) {
            err = new SyntaxError("integer is not minimally encoded"u8);
            return (ret, offset, err);
        }
        ret64 |= (int64)((int64)((byte)(b & 0x7f)));
        offset++;
        if ((byte)(b & 0x80) == 0) {
            ret = (nint)ret64;
            // Ensure that the returned value fits in an int on all platforms
            if (ret64 > math.MaxInt32) {
                err = new StructuralError("base 128 integer too large"u8);
            }
            return (ret, offset, err);
        }
    }
    err = new SyntaxError("truncated base 128 integer"u8);
    return (ret, offset, err);
}

// UTCTime
internal static (time.Time ret, error err) parseUTCTime(slice<byte> bytes) {
    time.Time ret = default!;
    error err = default!;

    @string s = ((@string)bytes);
    @string formatStr = "0601021504Z0700"u8;
    (ret, err) = time.Parse(formatStr, s);
    if (err != default!) {
        formatStr = "060102150405Z0700"u8;
        (ret, err) = time.Parse(formatStr, s);
    }
    if (err != default!) {
        return (ret, err);
    }
    {
        @string serialized = ret.Format(formatStr); if (serialized != s) {
            err = fmt.Errorf("asn1: time did not serialize back to the original value and may be invalid: given %q, but serialized as %q"u8, s, serialized);
            return (ret, err);
        }
    }
    if (ret.Year() >= 2050) {
        // UTCTime only encodes times prior to 2050. See https://tools.ietf.org/html/rfc5280#section-4.1.2.5.1
        ret = ret.AddDate(-100, 0, 0);
    }
    return (ret, err);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string formatStrᶜ = "20060102150405.999999999Z0700"u8;

// parseGeneralizedTime parses the GeneralizedTime from the given byte slice
// and returns the resulting time.
internal static (time.Time ret, error err) parseGeneralizedTime(slice<byte> bytes) {
    time.Time ret = default!;
    error err = default!;

    @string formatStr = formatStrᶜ;
    @string s = ((@string)bytes);
    {
        (ret, err) = time.Parse(formatStr, s); if (err != default!) {
            return (ret, err);
        }
    }
    {
        @string serialized = ret.Format(formatStr); if (serialized != s) {
            err = fmt.Errorf("asn1: time did not serialize back to the original value and may be invalid: given %q, but serialized as %q"u8, s, serialized);
        }
    }
    return (ret, err);
}

// NumericString

// parseNumericString parses an ASN.1 NumericString from the given byte array
// and returns it.
internal static (@string ret, error err) parseNumericString(slice<byte> bytes) {
    foreach (var (_, b) in bytes) {
        if (!isNumeric(b)) {
            return ("", new SyntaxError("NumericString contains invalid character"u8));
        }
    }
    return (((@string)bytes), default!);
}

// isNumeric reports whether the given b is in the ASN.1 NumericString set.
internal static bool isNumeric(byte b) {
    return (rune)'0' <= b && b <= (rune)'9' || b == (rune)' ';
}

// PrintableString

// parsePrintableString parses an ASN.1 PrintableString from the given byte
// array and returns it.
internal static (@string ret, error err) parsePrintableString(slice<byte> bytes) {
    @string ret = default!;
    error err = default!;

    foreach (var (_, b) in bytes) {
        if (!isPrintable(b, allowAsterisk, allowAmpersand)) {
            err = new SyntaxError("PrintableString contains invalid character"u8);
            return (ret, err);
        }
    }
    ret = ((@string)bytes);
    return (ret, err);
}

[GoType("bool")] partial struct asteriskFlag;

[GoType("bool")] partial struct ampersandFlag;

internal static asteriskFlag allowAsterisk => true;
internal static asteriskFlag rejectAsterisk => false;
internal static ampersandFlag allowAmpersand => true;
internal static ampersandFlag rejectAmpersand => false;

// isPrintable reports whether the given b is in the ASN.1 PrintableString set.
// If asterisk is allowAsterisk then '*' is also allowed, reflecting existing
// practice. If ampersand is allowAmpersand then '&' is allowed as well.
internal static bool isPrintable(byte b, asteriskFlag asterisk, ampersandFlag ampersand) {
    return (rune)'a' <= b && b <= (rune)'z' || (rune)'A' <= b && b <= (rune)'Z' || (rune)'0' <= b && b <= (rune)'9' || (rune)'\'' <= b && b <= (rune)')' || (rune)'+' <= b && b <= (rune)'/' || b == (rune)' ' || b == (rune)':' || b == (rune)'=' || b == (rune)'?' || ((bool)asterisk && b == (rune)'*') || ((bool)ampersand && b == (rune)'&');
}

// This is technically not allowed in a PrintableString.
// However, x509 certificates with wildcard strings don't
// always use the correct string type so we permit it.
// This is not technically allowed either. However, not
// only is it relatively common, but there are also a
// handful of CA certificates that contain it. At least
// one of which will not expire until 2027.
// IA5String

// parseIA5String parses an ASN.1 IA5String (ASCII string) from the given
// byte slice and returns it.
internal static (@string ret, error err) parseIA5String(slice<byte> bytes) {
    @string ret = default!;
    error err = default!;

    foreach (var (_, b) in bytes) {
        if (b >= utf8.RuneSelf) {
            err = new SyntaxError("IA5String contains invalid character"u8);
            return (ret, err);
        }
    }
    ret = ((@string)bytes);
    return (ret, err);
}

// T61String

// parseT61String parses an ASN.1 T61String (8-bit clean string) from the given
// byte slice and returns it.
internal static (@string ret, error err) parseT61String(slice<byte> bytes) {
    return (((@string)bytes), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asn1InvalidUtf8Stringˢ = "asn1: invalid UTF-8 string"u8;

// UTF8String

// parseUTF8String parses an ASN.1 UTF8String (raw UTF-8) from the given byte
// array and returns it.
internal static (@string ret, error err) parseUTF8String(slice<byte> bytes) {
    if (!utf8.Valid(bytes)) {
        return ("", errors.New(asn1InvalidUtf8Stringˢ));
    }
    return (((@string)bytes), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pkcs12OddLengthBmpStringˢ = "pkcs12: odd-length BMP string"u8;

// BMPString

// parseBMPString parses an ASN.1 BMPString (Basic Multilingual Plane of
// ISO/IEC/ITU 10646-1) from the given byte slice and returns it.
internal static (@string, error) parseBMPString(slice<byte> bmpString) {
    if (len(bmpString) % 2 != 0) {
        return ("", errors.New(pkcs12OddLengthBmpStringˢ));
    }
    // Strip terminator if present.
    {
        nint l = len(bmpString); if (l >= 2 && bmpString[l - 1] == 0 && bmpString[l - 2] == 0) {
            bmpString = bmpString.slice(0, l - 2);
        }
    }
    var s = new slice<uint16>(0, len(bmpString) / 2);
    while (len(bmpString) > 0) {
        s = append(s, (uint16)((uint16)((uint16)bmpString[0] << (int)(8)) + (uint16)bmpString[1]));
        bmpString = bmpString[2..];
    }
    return (((@string)utf16.Decode(s)), default!);
}

// A RawValue represents an undecoded ASN.1 object.
[GoType] partial struct RawValue {
    public nint Class, Tag;
    public bool IsCompound;
    public slice<byte> Bytes;
    public slice<byte> FullBytes; // includes the tag and length
}

[GoType("[]byte")] partial struct RawContent;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asn1InternalErrorInˢ = "asn1: internal error in parseTagAndLength"u8;

// Tagging

// parseTagAndLength parses an ASN.1 tag and length pair from the given offset
// into a byte slice. It returns the parsed data and the new offset. SET and
// SET OF (tag 17) are mapped to SEQUENCE and SEQUENCE OF (tag 16) since we
// don't distinguish between ordered and unordered objects in this code.
internal static (tagAndLength ret, nint offset, error err) parseTagAndLength(slice<byte> bytes, nint initOffset) {
    tagAndLength ret = default!;
    nint offset = default!;
    error err = default!;

    offset = initOffset;
    // parseTagAndLength should not be called without at least a single
    // byte to read. Thus this check is for robustness:
    if (offset >= len(bytes)) {
        err = errors.New(asn1InternalErrorInˢ);
        return (ret, offset, err);
    }
    var b = bytes[offset];
    offset++;
    ret.@class = (nint)((b >> (int)(6)));
    ret.isCompound = (byte)(b & 0x20) == 0x20;
    ret.tag = (nint)((byte)(b & 0x1f));
    // If the bottom five bits are set, then the tag number is actually base 128
    // encoded afterwards
    if (ret.tag == 0x1f) {
        (ret.tag, offset, err) = parseBase128Int(bytes, offset);
        if (err != default!) {
            return (ret, offset, err);
        }
        // Tags should be encoded in minimal form.
        if (ret.tag < 0x1f) {
            err = new SyntaxError("non-minimal tag"u8);
            return (ret, offset, err);
        }
    }
    if (offset >= len(bytes)) {
        err = new SyntaxError("truncated tag or length"u8);
        return (ret, offset, err);
    }
    b = bytes[offset];
    offset++;
    if ((byte)(b & 0x80) == 0){
        // The length is encoded in the bottom 7 bits.
        ret.length = (nint)((byte)(b & 0x7f));
    } else {
        // Bottom 7 bits give the number of length bytes to follow.
        nint numBytes = (nint)((byte)(b & 0x7f));
        if (numBytes == 0) {
            err = new SyntaxError("indefinite length found (not DER)"u8);
            return (ret, offset, err);
        }
        ret.length = 0;
        for (nint i = 0; i < numBytes; i++) {
            if (offset >= len(bytes)) {
                err = new SyntaxError("truncated tag or length"u8);
                return (ret, offset, err);
            }
            b = bytes[offset];
            offset++;
            if (ret.length >= (1 << (int)(23))) {
                // We can't shift ret.length up without
                // overflowing.
                err = new StructuralError("length too large"u8);
                return (ret, offset, err);
            }
            ret.length <<= (int)(8);
            ret.length |= (nint)((nint)b);
            if (ret.length == 0) {
                // DER requires that lengths be minimal.
                err = new StructuralError("superfluous leading zeros in length"u8);
                return (ret, offset, err);
            }
        }
        // Short lengths must be encoded in short form.
        if (ret.length < 0x80) {
            err = new StructuralError("non-minimal length"u8);
            return (ret, offset, err);
        }
    }
    return (ret, offset, err);
}

// parseSequenceOf is used for SEQUENCE OF and SET OF values. It tries to parse
// a number of ASN.1 values from the given byte slice and returns them as a
// slice of Go values of the given type.
internal static (reflectꓸValue ret, error err) parseSequenceOf(slice<byte> bytes, reflectꓸType sliceType, reflectꓸType elemType) {
    reflectꓸValue ret = new(nil);
    error err = default!;

    var (matchAny, expectedTag, compoundType, ok) = getUniversalType(elemType);
    if (!ok) {
        err = new StructuralError("unknown Go type for slice"u8);
        return (ret, err);
    }
    // First we iterate over the input and count the number of elements,
    // checking that the types are correct in each case.
    nint numElements = 0;
    for (nint offsetΔ1 = 0; offsetΔ1 < len(bytes); ) {
        tagAndLength t = default!;
        (t, offsetΔ1, err) = parseTagAndLength(bytes, offsetΔ1);
        if (err != default!) {
            return (ret, err);
        }
        var exprᴛ1 = t.tag;
        if (exprᴛ1 == TagIA5String || exprᴛ1 == TagGeneralString || exprᴛ1 == TagT61String || exprᴛ1 == TagUTF8String || exprᴛ1 == TagNumericString || exprᴛ1 == TagBMPString) {
            t.tag = TagPrintableString;
        }
        else if (exprᴛ1 == TagGeneralizedTime || exprᴛ1 == TagUTCTime) {
            t.tag = TagUTCTime;
        }

        // We pretend that various other string types are
        // PRINTABLE STRINGs so that a sequence of them can be
        // parsed into a []string.
        // Likewise, both time types are treated the same.
        if (!matchAny && (t.@class != ClassUniversal || t.isCompound != compoundType || t.tag != expectedTag)) {
            err = new StructuralError("sequence tag mismatch"u8);
            return (ret, err);
        }
        if (invalidLength(offsetΔ1, t.length, len(bytes))) {
            err = new SyntaxError("truncated sequence"u8);
            return (ret, err);
        }
        offsetΔ1 += t.length;
        numElements++;
    }
    var elemSize = (uint64)elemType.Size();
    nint safeCap = saferio.SliceCapWithSize(elemSize, (uint64)numElements);
    if (safeCap < 0) {
        err = new SyntaxError(fmt.Sprintf("%s slice too big: %d elements of %d bytes"u8, elemType.Kind(), numElements, elemSize));
        return (ret, err);
    }
    ret = reflect.MakeSlice(sliceType, 0, safeCap);
    var @params = new fieldParameters(nil);
    nint offset = 0;
    for (nint i = 0; i < numElements; i++) {
        ret = reflect.Append(ret, reflect.Zero(elemType));
        (offset, err) = parseField(ret.Index(i), bytes, offset, @params);
        if (err != default!) {
            return (ret, err);
        }
    }
    return (ret, err);
}

internal static reflectꓸType bitStringType = reflect.TypeFor<BitString>();
internal static reflectꓸType objectIdentifierType = reflect.TypeFor<ObjectIdentifier>();
internal static reflectꓸType enumeratedType = reflect.TypeFor<Enumerated>();
internal static reflectꓸType flagType = reflect.TypeFor<Flag>();
internal static reflectꓸType timeType = reflect.TypeFor<time.Time>();
internal static reflectꓸType rawValueType = reflect.TypeFor<RawValue>();
internal static reflectꓸType rawContentsType = reflect.TypeFor<RawContent>();
internal static reflectꓸType bigIntType = reflect.TypeFor<ж<bigꓸInt>>();

// invalidLength reports whether offset + length > sliceLength, or if the
// addition would overflow.
internal static bool invalidLength(nint offset, nint length, nint sliceLength) {
    return offset + length < offset || offset + length > sliceLength;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asn1ˢ = "asn1"u8;

// parseField is the main parsing function. Given a byte slice and an offset
// into the array, it will try to parse a suitable ASN.1 value out and store it
// in the given Value.
internal static (nint offset, error err) parseField(reflectꓸValue v, slice<byte> bytes, nint initOffset, fieldParameters @params) {
    nint offset = default!;
    error err = default!;

    offset = initOffset;
    var fieldType = v.Type();
    // If we have run out of data, it may be that there are optional elements at the end.
    if (offset == len(bytes)) {
        if (!setDefaultValue(v, @params)) {
            err = new SyntaxError("sequence truncated"u8);
        }
        return (offset, err);
    }
    // Deal with the ANY type.
    {
        var ifaceType = fieldType; if (ifaceType.Kind() == reflect.ΔInterface && ifaceType.NumMethod() == 0) {
            tagAndLength tΔ1 = default!;
            (tΔ1, offset, err) = parseTagAndLength(bytes, offset);
            if (err != default!) {
                return (offset, err);
            }
            if (invalidLength(offset, tΔ1.length, len(bytes))) {
                err = new SyntaxError("data truncated"u8);
                return (offset, err);
            }
            any result = default!;
            if (!tΔ1.isCompound && tΔ1.@class == ClassUniversal) {
                var innerBytesΔ1 = bytes.slice(offset, offset + tΔ1.length);
                var exprᴛ1 = tΔ1.tag;
                if (exprᴛ1 == TagBoolean) {
                    (result, err) = parseBool(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagPrintableString) {
                    (result, err) = parsePrintableString(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagNumericString) {
                    (result, err) = parseNumericString(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagIA5String) {
                    (result, err) = parseIA5String(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagT61String) {
                    (result, err) = parseT61String(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagUTF8String) {
                    (result, err) = parseUTF8String(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagInteger) {
                    (result, err) = parseInt64(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagBitString) {
                    (result, err) = parseBitString(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagOID) {
                    (result, err) = parseObjectIdentifier(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagUTCTime) {
                    (result, err) = parseUTCTime(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagGeneralizedTime) {
                    (result, err) = parseGeneralizedTime(innerBytesΔ1);
                }
                else if (exprᴛ1 == TagOctetString) {
                    result = innerBytesΔ1;
                }
                else if (exprᴛ1 == TagBMPString) {
                    (result, err) = parseBMPString(innerBytesΔ1);
                }
                else { /* default: */
                }

            }
            // If we don't know how to handle the type, we just leave Value as nil.
            offset += tΔ1.length;
            if (err != default!) {
                return (offset, err);
            }
            if (result != default!) {
                v.Set(reflect.ValueOf(result));
            }
            return (offset, err);
        }
    }
    (var t, offset, err) = parseTagAndLength(bytes, offset);
    if (err != default!) {
        return (offset, err);
    }
    if (@params.@explicit) {
        nint expectedClassΔ1 = ClassContextSpecific;
        if (@params.application) {
            expectedClassΔ1 = ClassApplication;
        }
        if (offset == len(bytes)) {
            err = new StructuralError("explicit tag has no child"u8);
            return (offset, err);
        }
        if (t.@class == expectedClassΔ1 && t.tag == @params.tag.Value && (t.length == 0 || t.isCompound)){
            if (AreEqual(fieldType, rawValueType)){
            } else 
            if (t.length > 0){
                // The inner element should not be parsed for RawValues.
                (t, offset, err) = parseTagAndLength(bytes, offset);
                if (err != default!) {
                    return (offset, err);
                }
            } else {
                if (!AreEqual(fieldType, flagType)) {
                    err = new StructuralError("zero length explicit tag was not an asn1.Flag"u8);
                    return (offset, err);
                }
                v.SetBool(true);
                return (offset, err);
            }
        } else {
            // The tags didn't match, it might be an optional element.
            var ok = setDefaultValue(v, @params);
            if (ok){
                offset = initOffset;
            } else {
                err = new StructuralError("explicitly tagged member didn't match"u8);
            }
            return (offset, err);
        }
    }
    var (matchAny, universalTag, compoundType, ok1) = getUniversalType(fieldType);
    if (!ok1) {
        err = new StructuralError(fmt.Sprintf("unknown Go type: %v"u8, fieldType));
        return (offset, err);
    }
    // Special case for strings: all the ASN.1 string types map to the Go
    // type string. getUniversalType returns the tag for PrintableString
    // when it sees a string, so if we see a different string type on the
    // wire, we change the universal type to match.
    if (universalTag == TagPrintableString) {
        if (t.@class == ClassUniversal){
            var exprᴛ2 = t.tag;
            if (exprᴛ2 == TagIA5String || exprᴛ2 == TagGeneralString || exprᴛ2 == TagT61String || exprᴛ2 == TagUTF8String || exprᴛ2 == TagNumericString || exprᴛ2 == TagBMPString) {
                universalTag = t.tag;
            }

        } else 
        if (@params.stringType != 0) {
            universalTag = @params.stringType;
        }
    }
    // Special case for time: UTCTime and GeneralizedTime both map to the
    // Go type time.Time.
    if (universalTag == TagUTCTime && t.tag == TagGeneralizedTime && t.@class == ClassUniversal) {
        universalTag = TagGeneralizedTime;
    }
    if (@params.set) {
        universalTag = TagSet;
    }
    var matchAnyClassAndTag = matchAny;
    nint expectedClass = ClassUniversal;
    nint expectedTag = universalTag;
    if (!@params.@explicit && @params.tag != nil) {
        expectedClass = ClassContextSpecific;
        expectedTag = @params.tag.Value;
        matchAnyClassAndTag = false;
    }
    if (!@params.@explicit && @params.application && @params.tag != nil) {
        expectedClass = ClassApplication;
        expectedTag = @params.tag.Value;
        matchAnyClassAndTag = false;
    }
    if (!@params.@explicit && @params.@private && @params.tag != nil) {
        expectedClass = ClassPrivate;
        expectedTag = @params.tag.Value;
        matchAnyClassAndTag = false;
    }
    // We have unwrapped any explicit tagging at this point.
    if (!matchAnyClassAndTag && (t.@class != expectedClass || t.tag != expectedTag) || (!matchAny && t.isCompound != compoundType)) {
        // Tags don't match. Again, it could be an optional element.
        var ok = setDefaultValue(v, @params);
        if (ok){
            offset = initOffset;
        } else {
            err = new StructuralError(fmt.Sprintf("tags don't match (%d vs %+v) %+v %s @%d"u8, expectedTag, t, @params, fieldType.Name(), offset));
        }
        return (offset, err);
    }
    if (invalidLength(offset, t.length, len(bytes))) {
        err = new SyntaxError("data truncated"u8);
        return (offset, err);
    }
    var innerBytes = bytes.slice(offset, offset + t.length);
    offset += t.length;
    // We deal with the structures defined in this package first.
    switch (v.Addr().Interface().type()) {
    case ж<RawValue> vΔ1: {
        vΔ1.Value = new RawValue(t.@class, t.tag, t.isCompound, innerBytes, bytes.slice(initOffset, offset));
        return (offset, err);
    }
    case ж<ObjectIdentifier> vΔ1: {
        (vΔ1.ValueSlot, err) = parseObjectIdentifier(innerBytes);
        return (offset, err);
    }
    case ж<BitString> vΔ1: {
        (vΔ1.Value, err) = parseBitString(innerBytes);
        return (offset, err);
    }
    case ж<time.Time> vΔ1: {
        if (universalTag == TagUTCTime) {
            (vΔ1.Value, err) = parseUTCTime(innerBytes);
            return (offset, err);
        }
        (vΔ1.Value, err) = parseGeneralizedTime(innerBytes);
        return (offset, err);
    }
    case ж<Enumerated> vΔ1: {
        var (parsedInt, err1) = parseInt32(innerBytes);
        if (err1 == default!) {
            vΔ1.Value = ((Enumerated)(nint)parsedInt);
        }
        err = err1;
        return (offset, err);
    }
    case ж<Flag> vΔ1: {
        vΔ1.Value = true;
        return (offset, err);
    }
    case ж<ж<bigꓸInt>> vΔ1: {
        var (parsedInt, err1) = parseBigInt(innerBytes);
        if (err1 == default!) {
            vΔ1.ValueSlot = parsedInt;
        }
        err = err1;
        return (offset, err);
    }}
    {
        var val = v;
        var exprᴛ3 = val.Kind();
        if (exprᴛ3 == reflect.ΔBool) {
            var (parsedBool, err1) = parseBool(innerBytes);
            if (err1 == default!) {
                val.SetBool(parsedBool);
            }
            err = err1;
            return (offset, err);
        }
        if (exprᴛ3 == reflect.ΔInt || exprᴛ3 == reflect.Int32 || exprᴛ3 == reflect.Int64) {
            if (val.Type().Size() == 4){
                var (parsedInt, err1) = parseInt32(innerBytes);
                if (err1 == default!) {
                    val.SetInt((int64)parsedInt);
                }
                err = err1;
            } else {
                var (parsedInt, err1) = parseInt64(innerBytes);
                if (err1 == default!) {
                    val.SetInt(parsedInt);
                }
                err = err1;
            }
            return (offset, err);
        }
        if (exprᴛ3 == reflect.Struct) {
            var structType = fieldType;
            for (nint i = 0; i < structType.NumField(); i++) {
                // TODO(dfc) Add support for the remaining integer types
                if (!structType.Field(i).IsExported()) {
                    err = new StructuralError("struct contains unexported fields"u8);
                    return (offset, err);
                }
            }
            if (structType.NumField() > 0 && AreEqual(structType.Field(0).Type, rawContentsType)) {
                var bytesΔ2 = bytes.slice(initOffset, offset);
                val.Field(0).Set(reflect.ValueOf(((RawContent)bytesΔ2)));
            }
            nint innerOffset = 0;
            for (nint i = 0; i < structType.NumField(); i++) {
                var field = structType.Field(i);
                if (i == 0 && AreEqual(field.Type, rawContentsType)) {
                    continue;
                }
                (innerOffset, err) = parseField(val.Field(i), innerBytes, innerOffset, parseFieldParameters(field.Tag.Get(asn1ˢ)));
                if (err != default!) {
                    return (offset, err);
                }
            }
            return (offset, err);
        }
        if (exprᴛ3 == reflect.ΔSlice) {
            var sliceType = fieldType;
            if (sliceType.Elem().Kind() == reflect.Uint8) {
                // We allow extra bytes at the end of the SEQUENCE because
                // adding elements to the end has been used in X.509 as the
                // version numbers have increased.
                val.Set(reflect.MakeSlice(sliceType, len(innerBytes), len(innerBytes)));
                reflect.Copy(val, reflect.ValueOf(innerBytes));
                return (offset, err);
            }
            var (newSlice, err1) = parseSequenceOf(innerBytes, sliceType, sliceType.Elem());
            if (err1 == default!) {
                val.Set(newSlice);
            }
            err = err1;
            return (offset, err);
        }
        if (exprᴛ3 == reflect.ΔString) {
            @string vΔ3 = default!;
            var exprᴛ4 = universalTag;
            if (exprᴛ4 == TagPrintableString) {
                (vΔ3, err) = parsePrintableString(innerBytes);
            }
            else if (exprᴛ4 == TagNumericString) {
                (vΔ3, err) = parseNumericString(innerBytes);
            }
            else if (exprᴛ4 == TagIA5String) {
                (vΔ3, err) = parseIA5String(innerBytes);
            }
            else if (exprᴛ4 == TagT61String) {
                (vΔ3, err) = parseT61String(innerBytes);
            }
            else if (exprᴛ4 == TagUTF8String) {
                (vΔ3, err) = parseUTF8String(innerBytes);
            }
            else if (exprᴛ4 == TagGeneralString) {
                (vΔ3, err) = parseT61String(innerBytes);
            }
            else if (exprᴛ4 == TagBMPString) {
                (vΔ3, err) = parseBMPString(innerBytes);
            }
            else { /* default: */
                err = new SyntaxError( // GeneralString is specified in ISO-2022/ECMA-35,
 // A brief review suggests that it includes structures
 // that allow the encoding to change midstring and
 // such. We give up and pass it as an 8-bit string.
fmt.Sprintf("internal error: unknown string type %d"u8, universalTag));
            }

            if (err == default!) {
                val.SetString(vΔ3);
            }
            return (offset, err);
        }
    }

    err = new StructuralError("unsupported: "u8 + v.Type().String());
    return (offset, err);
}

// canHaveDefaultValue reports whether k is a Kind that we will set a default
// value for. (A signed integer, essentially.)
internal static bool canHaveDefaultValue(reflectꓸKind k) {
    var exprᴛ1 = k;
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return true;
    }

    return false;
}

// setDefaultValue is used to install a default value, from a tag string, into
// a Value. It is successful if the field was optional, even if a default value
// wasn't provided or it failed to install it into the Value.
internal static bool /*ok*/ setDefaultValue(reflectꓸValue v, fieldParameters @params) {
    bool ok = default!;

    if (!@params.optional) {
        return ok;
    }
    ok = true;
    if (@params.defaultValue == nil) {
        return ok;
    }
    if (canHaveDefaultValue(v.Kind())) {
        v.SetInt(@params.defaultValue.Value);
    }
    return ok;
}

// Unmarshal parses the DER-encoded ASN.1 data structure b
// and uses the reflect package to fill in an arbitrary value pointed at by val.
// Because Unmarshal uses the reflect package, the structs
// being written to must use upper case field names. If val
// is nil or not a pointer, Unmarshal returns an error.
//
// After parsing b, any bytes that were leftover and not used to fill
// val will be returned in rest. When parsing a SEQUENCE into a struct,
// any trailing elements of the SEQUENCE that do not have matching
// fields in val will not be included in rest, as these are considered
// valid elements of the SEQUENCE and not trailing data.
//
//   - An ASN.1 INTEGER can be written to an int, int32, int64,
//     or *[big.Int].
//     If the encoded value does not fit in the Go type,
//     Unmarshal returns a parse error.
//
//   - An ASN.1 BIT STRING can be written to a [BitString].
//
//   - An ASN.1 OCTET STRING can be written to a []byte.
//
//   - An ASN.1 OBJECT IDENTIFIER can be written to an [ObjectIdentifier].
//
//   - An ASN.1 ENUMERATED can be written to an [Enumerated].
//
//   - An ASN.1 UTCTIME or GENERALIZEDTIME can be written to a [time.Time].
//
//   - An ASN.1 PrintableString, IA5String, or NumericString can be written to a string.
//
//   - Any of the above ASN.1 values can be written to an interface{}.
//     The value stored in the interface has the corresponding Go type.
//     For integers, that type is int64.
//
//   - An ASN.1 SEQUENCE OF x or SET OF x can be written
//     to a slice if an x can be written to the slice's element type.
//
//   - An ASN.1 SEQUENCE or SET can be written to a struct
//     if each of the elements in the sequence can be
//     written to the corresponding element in the struct.
//
// The following tags on struct fields have special meaning to Unmarshal:
//
//	application specifies that an APPLICATION tag is used
//	private     specifies that a PRIVATE tag is used
//	default:x   sets the default value for optional integer fields (only used if optional is also present)
//	explicit    specifies that an additional, explicit tag wraps the implicit one
//	optional    marks the field as ASN.1 OPTIONAL
//	set         causes a SET, rather than a SEQUENCE type to be expected
//	tag:x       specifies the ASN.1 tag number; implies ASN.1 CONTEXT SPECIFIC
//
// When decoding an ASN.1 value with an IMPLICIT tag into a string field,
// Unmarshal will default to a PrintableString, which doesn't support
// characters such as '@' and '&'. To force other encodings, use the following
// tags:
//
//	ia5     causes strings to be unmarshaled as ASN.1 IA5String values
//	numeric causes strings to be unmarshaled as ASN.1 NumericString values
//	utf8    causes strings to be unmarshaled as ASN.1 UTF8String values
//
// If the type of the first field of a structure is RawContent then the raw
// ASN1 contents of the struct will be stored in it.
//
// If the name of a slice type ends with "SET" then it's treated as if
// the "set" tag was set on it. This results in interpreting the type as a
// SET OF x rather than a SEQUENCE OF x. This can be used with nested slices
// where a struct tag cannot be given.
//
// Other ASN.1 types are not supported; if it encounters them,
// Unmarshal returns a parse error.
public static (slice<byte> rest, error err) Unmarshal(slice<byte> b, any val) {
    return UnmarshalWithParams(b, val, ""u8);
}

// An invalidUnmarshalError describes an invalid argument passed to Unmarshal.
// (The argument to Unmarshal must be a non-nil pointer.)
[GoType] partial struct invalidUnmarshalError {
    public reflectꓸType Type;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asn1UnmarshalRecipientˢ = "asn1: Unmarshal recipient value is nil"u8;

[GoRecv] internal static @string Error(this ref invalidUnmarshalError e) {
    if (e.Type == default!) {
        return asn1UnmarshalRecipientˢ;
    }
    if (e.Type.Kind() != reflect.ΔPointer) {
        return "asn1: Unmarshal recipient value is non-pointer "u8 + e.Type.String();
    }
    return "asn1: Unmarshal recipient value is nil "u8 + e.Type.String();
}

// UnmarshalWithParams allows field parameters to be specified for the
// top-level element. The form of the params is the same as the field tags.
public static (slice<byte> rest, error err) UnmarshalWithParams(slice<byte> b, any val, @string @params) {
    error err = default!;

    var v = reflect.ValueOf(val);
    if (v.Kind() != reflect.ΔPointer || v.IsNil()) {
        return (default!, new invalidUnmarshalErrorжerror(Ꮡ(new invalidUnmarshalError(reflect.TypeOf(val)))));
    }
    (var offset, err) = parseField(v.Elem(), b, 0, parseFieldParameters(@params));
    if (err != default!) {
        return (default!, err);
    }
    return (b.slice(offset), default!);
}

} // end asn1_package
