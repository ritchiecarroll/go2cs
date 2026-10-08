// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using big = go.math.big_package;
using reflect = reflect_package;
using slices = slices_package;
using time = time_package;
using utf8 = unicode.utf8_package;
using go.math;
using unicode;

partial class asn1_package {

internal static encoder byte00Encoder = ((byteEncoder)0x00);
internal static encoder byteFFEncoder = ((byteEncoder)0xff);

// encoder represents an ASN.1 element that is waiting to be marshaled.
partial interface encoder {
    // Len returns the number of bytes needed to marshal this element.
    nint Len();
    // Encode encodes this element by writing Len() bytes to dst.
    void Encode(slice<byte> dst);
}

partial struct byteEncoder /*num:byte*/;

internal static nint Len(this byteEncoder c) {
    return 1;
}

internal static void Encode(this byteEncoder c, slice<byte> dst) {
    dst[0] = (byte)c;
}

partial struct bytesEncoder /*[]byte*/;

internal static nint Len(this bytesEncoder b) {
    return len(b);
}

internal static void Encode(this bytesEncoder b, slice<byte> dst) {
    if (copy(dst, b) != len(b)) {
        throw panic("internal error");
    }
}

partial struct stringEncoder /*@string*/;

internal static nint Len(this stringEncoder s) {
    return len(s);
}

internal static void Encode(this stringEncoder s, slice<byte> dst) {
    if (copy(dst, s) != len(s)) {
        throw panic("internal error");
    }
}

partial struct multiEncoder /*[]encoder*/;

internal static nint Len(this multiEncoder m) {
    nint size = default!;
    foreach (var (_, e) in m) {
        size += e.Len();
    }
    return size;
}

internal static void Encode(this multiEncoder m, slice<byte> dst) {
    nint off = default!;
    foreach (var (_, e) in m) {
        e.Encode(dst.slice(off));
        off += e.Len();
    }
}

partial struct setEncoder /*[]encoder*/;

internal static nint Len(this setEncoder s) {
    nint size = default!;
    foreach (var (_, e) in s) {
        size += e.Len();
    }
    return size;
}

internal static void Encode(this setEncoder s, slice<byte> dst) {
    // Per X690 Section 11.6: The encodings of the component values of a
    // set-of value shall appear in ascending order, the encodings being
    // compared as octet strings with the shorter components being padded
    // at their trailing end with 0-octets.
    //
    // First we encode each element to its TLV encoding and then use
    // octetSort to get the ordering expected by X690 DER rules before
    // writing the sorted encodings out to dst.
    var l = new slice<slice<byte>>(len(s));
    foreach (var (i, e) in s) {
        l[i] = new slice<byte>(e.Len());
        e.Encode(l[i]);
    }
    // Since we are using bytes.Compare to compare TLV encodings we
    // don't need to right pad s[i] and s[j] to the same length as
    // suggested in X690. If len(s[i]) < len(s[j]) the length octet of
    // s[i], which is the first determining byte, will inherently be
    // smaller than the length octet of s[j]. This lets us skip the
    // padding step.
    slices.SortFunc<slice<slice<byte>>, slice<byte>>(l, bytes.Compare);
    nint off = default!;
    foreach (var (_, b) in l) {
        copy(dst.slice(off), b);
        off += len(b);
    }
}

partial struct taggedEncoder {
    // scratch contains temporary space for encoding the tag and length of
    // an element in order to avoid extra allocations.
    internal array<byte> scratch = new(8);
    internal encoder tag;
    internal encoder body;
}

internal static nint Len(this ref taggedEncoder t) {
    return t.tag.Len() + t.body.Len();
}

internal static void Encode(this ref taggedEncoder t, slice<byte> dst) {
    t.tag.Encode(dst);
    t.body.Encode(dst.slice(t.tag.Len()));
}

partial struct int64Encoder /*num:int64*/;

internal static nint Len(this int64Encoder i) {
    nint n = 1;
    while (i > 127) {
        n++;
        i >>= (int)(8);
    }
    while (i < -128) {
        n++;
        i >>= (int)(8);
    }
    return n;
}

internal static void Encode(this int64Encoder i, slice<byte> dst) {
    nint n = i.Len();
    for (nint j = 0; j < n; j++) {
        dst[j] = (byte)(int64)((i >> (int)((nuint)((n - 1 - j) * 8))));
    }
}

internal static nint base128IntLength(int64 n) {
    if (n == 0) {
        return 1;
    }
    nint l = 0;
    for (var i = n; i > 0; i >>= (int)(7)) {
        l++;
    }
    return l;
}

internal static slice<byte> appendBase128Int(slice<byte> dst, int64 n) {
    nint l = base128IntLength(n);
    for (nint i = l - 1; i >= 0; i--) {
        var o = (byte)(n.Rsh((nuint)(i * 7)));
        o &= (byte)(0x7f);
        if (i != 0) {
            o |= (byte)(0x80);
        }
        dst = append(dst, o);
    }
    return dst;
}

internal static (encoder, error) makeBigInt(ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return (default!, new StructuralError("empty integer"u8));
    }
    if (n.Sign() < 0){
        // A negative number has to be converted to two's-complement
        // form. So we'll invert and subtract 1. If the
        // most-significant-bit isn't set then we'll need to pad the
        // beginning with 0xff in order to keep the number negative.
        var nMinus1 = @new<bigꓸInt>().Neg(Ꮡn);
        nMinus1.Sub(nMinus1, bigOne);
        var bytes = nMinus1.Bytes();
        foreach (var (i, _) in bytes) {
            bytes[i] ^= (byte)(0xff);
        }
        if (len(bytes) == 0 || (byte)(bytes[0] & 0x80) == 0) {
            return (((multiEncoder)new encoder[]{byteFFEncoder, ((bytesEncoder)bytes)}.slice()), default!);
        }
        return (((bytesEncoder)bytes), default!);
    } else 
    if (n.Sign() == 0){
        // Zero is written as a single 0 zero rather than no bytes.
        return (byte00Encoder, default!);
    } else {
        var bytes = n.Bytes();
        if (len(bytes) > 0 && (byte)(bytes[0] & 0x80) != 0) {
            // We'll have to pad this with 0x00 in order to stop it
            // looking like a negative number.
            return (((multiEncoder)new encoder[]{byte00Encoder, ((bytesEncoder)bytes)}.slice()), default!);
        }
        return (((bytesEncoder)bytes), default!);
    }
}

internal static slice<byte> appendLength(slice<byte> dst, nint i) {
    nint n = lengthLength(i);
    for (; n > 0; n--) {
        dst = append(dst, (byte)(i.Rsh((nuint)((n - 1) * 8))));
    }
    return dst;
}

internal static nint /*numBytes*/ lengthLength(nint i) {
    nint numBytes = default!;

    numBytes = 1;
    while (i > 255) {
        numBytes++;
        i >>= (int)(8);
    }
    return numBytes;
}

internal static slice<byte> appendTagAndLength(slice<byte> dst, tagAndLength t) {
    var b = (uint8)((uint8)t.@class << (int)(6));
    if (t.isCompound) {
        b |= (uint8)(0x20);
    }
    if (t.tag >= 31){
        b |= (uint8)(0x1f);
        dst = append(dst, b);
        dst = appendBase128Int(dst, (int64)t.tag);
    } else {
        b |= (uint8)((uint8)t.tag);
        dst = append(dst, b);
    }
    if (t.length >= 128){
        nint l = lengthLength(t.length);
        dst = append(dst, (byte)(0x80 | (byte)l));
        dst = appendLength(dst, t.length);
    } else {
        dst = append(dst, (byte)t.length);
    }
    return dst;
}

partial struct bitStringEncoder /*BitString*/;

internal static nint Len(this bitStringEncoder b) {
    return len(b.Bytes) + 1;
}

internal static void Encode(this bitStringEncoder b, slice<byte> dst) {
    dst[0] = (byte)((8 - b.BitLength % 8) % 8);
    if (copy(dst[1..], b.Bytes) != len(b.Bytes)) {
        throw panic("internal error");
    }
}

partial struct oidEncoder /*[]nint*/;

internal static nint Len(this oidEncoder oid) {
    nint l = base128IntLength((int64)(oid[0] * 40 + oid[1]));
    for (nint i = 2; i < len(oid); i++) {
        l += base128IntLength((int64)oid[i]);
    }
    return l;
}

internal static void Encode(this oidEncoder oid, slice<byte> dst) {
    dst = appendBase128Int(dst[..0], (int64)(oid[0] * 40 + oid[1]));
    for (nint i = 2; i < len(oid); i++) {
        dst = appendBase128Int(dst, (int64)oid[i]);
    }
}

internal static (encoder e, error err) makeObjectIdentifier(slice<nint> oid) {
    if (len(oid) < 2 || oid[0] > 2 || (oid[0] < 2 && oid[1] >= 40)) {
        return (default!, new StructuralError("invalid object identifier"u8));
    }
    return (((oidEncoder)oid), default!);
}

internal static (encoder e, error err) makePrintableString(@string s) {
    for (nint i = 0; i < len(s); i++) {
        // The asterisk is often used in PrintableString, even though
        // it is invalid. If a PrintableString was specifically
        // requested then the asterisk is permitted by this code.
        // Ampersand is allowed in parsing due a handful of CA
        // certificates, however when making new certificates
        // it is rejected.
        if (!isPrintable(s[i], allowAsterisk, rejectAmpersand)) {
            return (default!, new StructuralError("PrintableString contains invalid character"u8));
        }
    }
    return (((stringEncoder)s), default!);
}

internal static (encoder e, error err) makeIA5String(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] > 127) {
            return (default!, new StructuralError("IA5String contains invalid character"u8));
        }
    }
    return (((stringEncoder)s), default!);
}

internal static (encoder e, error err) makeNumericString(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (!isNumeric(s[i])) {
            return (default!, new StructuralError("NumericString contains invalid character"u8));
        }
    }
    return (((stringEncoder)s), default!);
}

internal static encoder makeUTF8String(@string s) {
    return ((stringEncoder)s);
}

internal static slice<byte> appendTwoDigits(slice<byte> dst, nint v) {
    return append(dst, (byte)((rune)'0' + (v / 10) % 10), (byte)((rune)'0' + v % 10));
}

internal static slice<byte> appendFourDigits(slice<byte> dst, nint v) {
    return append(dst,
        (byte)((rune)'0' + (v / 1000) % 10),
        (byte)((rune)'0' + (v / 100) % 10),
        (byte)((rune)'0' + (v / 10) % 10),
        (byte)((rune)'0' + v % 10));
}

internal static bool outsideUTCRange(time.Time t) {
    nint year = t.Year();
    return year < 1950 || year >= 2050;
}

internal static (encoder e, error err) makeUTCTime(time.Time t) {
    error err = default!;

    var dst = new slice<byte>(0, 18);
    (dst, err) = appendUTCTime(dst, t);
    if (err != default!) {
        return (default!, err);
    }
    return (((bytesEncoder)dst), default!);
}

internal static (encoder e, error err) makeGeneralizedTime(time.Time t) {
    error err = default!;

    var dst = new slice<byte>(0, 20);
    (dst, err) = appendGeneralizedTime(dst, t);
    if (err != default!) {
        return (default!, err);
    }
    return (((bytesEncoder)dst), default!);
}

internal static (slice<byte> ret, error err) appendUTCTime(slice<byte> dst, time.Time t) {
    nint year = t.Year();
    switch (ᐧ) {
    case {} when 1950 <= year && year < 2000: {
        dst = appendTwoDigits(dst, year - 1900);
        break;
    }
    case {} when 2000 <= year && year < 2050: {
        dst = appendTwoDigits(dst, year - 2000);
        break;
    }
    default: {
        return (default!, new StructuralError("cannot represent time as UTCTime"u8));
    }}

    return (appendTimeCommon(dst, t), default!);
}

internal static (slice<byte> ret, error err) appendGeneralizedTime(slice<byte> dst, time.Time t) {
    nint year = t.Year();
    if (year < 0 || year > 9999) {
        return (default!, new StructuralError("cannot represent time as GeneralizedTime"u8));
    }
    dst = appendFourDigits(dst, year);
    return (appendTimeCommon(dst, t), default!);
}

internal static slice<byte> appendTimeCommon(slice<byte> dst, time.Time t) {
    var (_, month, day) = t.Date();
    dst = appendTwoDigits(dst, (nint)month);
    dst = appendTwoDigits(dst, day);
    var (hour, min, sec) = t.Clock();
    dst = appendTwoDigits(dst, hour);
    dst = appendTwoDigits(dst, min);
    dst = appendTwoDigits(dst, sec);
    var (_, offset) = t.Zone();
    switch (ᐧ) {
    case {} when offset / 60 is 0: {
        return append(dst, (byte)((rune)'Z'));
    }
    case {} when offset is > 0: {
        dst = append(dst, (byte)((rune)'+'));
        break;
    }
    case {} when offset is < 0: {
        dst = append(dst, (byte)((rune)'-'));
        break;
    }}

    nint offsetMinutes = offset / 60;
    if (offsetMinutes < 0) {
        offsetMinutes = -offsetMinutes;
    }
    dst = appendTwoDigits(dst, offsetMinutes / 60);
    dst = appendTwoDigits(dst, offsetMinutes % 60);
    return dst;
}

internal static slice<byte> stripTagAndLength(slice<byte> @in) {
    var (_, offset, err) = parseTagAndLength(@in, 0);
    if (err != default!) {
        return @in;
    }
    return @in.slice(offset);
}

internal static (encoder e, error err) makeBody(reflectꓸValue value, fieldParameters @params) {
    error err = default!;

    var exprᴛ1 = value.Type();
    if (AreEqual(exprᴛ1, flagType)) {
        return (((bytesEncoder)default!), default!);
    }
    if (AreEqual(exprᴛ1, timeType)) {
        var t = value.Interface()._<time.Time>();
        if (@params.timeType == TagGeneralizedTime || outsideUTCRange(t)) {
            return makeGeneralizedTime(t);
        }
        return makeUTCTime(t);
    }
    if (AreEqual(exprᴛ1, bitStringType)) {
        return (((bitStringEncoder)(value.Interface()._<BitString>())), default!);
    }
    if (AreEqual(exprᴛ1, objectIdentifierType)) {
        return makeObjectIdentifier(value.Interface()._<ObjectIdentifier>());
    }
    if (AreEqual(exprᴛ1, bigIntType)) {
        return makeBigInt(value.Interface()._<ж<bigꓸInt>>());
    }

    {
        var v = value;
        var exprᴛ2 = v.Kind();
        if (exprᴛ2 == reflect.ΔBool) {
            if (v.Bool()) {
                return (byteFFEncoder, default!);
            }
            return (byte00Encoder, default!);
        }
        if (exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64) {
            return (((int64Encoder)v.Int()), default!);
        }
        if (exprᴛ2 == reflect.Struct) {
            var t = v.Type();
            for (nint i = 0; i < t.NumField(); i++) {
                if (!t.Field(i).IsExported()) {
                    return (default!, new StructuralError("struct contains unexported fields"u8));
                }
            }
            nint startingField = 0;
            nint n = t.NumField();
            if (n == 0) {
                return (((bytesEncoder)default!), default!);
            }
            if (AreEqual(t.Field(0).Type, rawContentsType)) {
                // If the first element of the structure is a non-empty
                // RawContents, then we don't bother serializing the rest.
                var s = v.Field(0);
                if (s.Len() > 0) {
                    var bytes = s.Bytes();
                    /* The RawContents will contain the tag and
				 * length fields but we'll also be writing
				 * those ourselves, so we strip them out of
				 * bytes */
                    return (((bytesEncoder)stripTagAndLength(bytes)), default!);
                }
                startingField = 1;
            }
            {
                nint n1 = n - startingField;
                switch (n1) {
                case 0: {
                    return (((bytesEncoder)default!), default!);
                }
                case 1: {
                    return makeField(v.Field(startingField), parseFieldParameters(t.Field(startingField).Tag.Get(asn1ˢ)));
                }
                default: {
                    var m = new slice<encoder>(n1);
                    for (nint i = 0; i < n1; i++) {
                        (m[i], err) = makeField(v.Field(i + startingField), parseFieldParameters(t.Field(i + startingField).Tag.Get(asn1ˢ)));
                        if (err != default!) {
                            return (default!, err);
                        }
                    }
                    return (((multiEncoder)m), default!);
                }}
            }

        }
        else if (exprᴛ2 == reflect.ΔSlice) {
            var sliceType = v.Type();
            if (sliceType.Elem().Kind() == reflect.Uint8) {
                return (((bytesEncoder)v.Bytes()), default!);
            }
            fieldParameters fp = default!;
            {
                nint l = v.Len();
                switch (l) {
                case 0: {
                    return (((bytesEncoder)default!), default!);
                }
                case 1: {
                    return makeField(v.Index(0), fp);
                }
                default: {
                    var m = new slice<encoder>(l);
                    for (nint i = 0; i < l; i++) {
                        (m[i], err) = makeField(v.Index(i), fp);
                        if (err != default!) {
                            return (default!, err);
                        }
                    }
                    if (@params.set) {
                        return (((setEncoder)m), default!);
                    }
                    return (((multiEncoder)m), default!);
                }}
            }

        }
        else if (exprᴛ2 == reflect.ΔString) {
            var exprᴛ3 = @params.stringType;
            if (exprᴛ3 == TagIA5String) {
                return makeIA5String(v.String());
            }
            if (exprᴛ3 == TagPrintableString) {
                return makePrintableString(v.String());
            }
            if (exprᴛ3 == TagNumericString) {
                return makeNumericString(v.String());
            }
            { /* default: */
                return (makeUTF8String(v.String()), default!);
            }

        }
    }

    return (default!, new StructuralError("unknown Go type"u8));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asn1StringNotValidUtf8ˢ = "asn1: string not valid UTF-8"u8;

internal static (encoder e, error err) makeField(reflectꓸValue v, fieldParameters @params) {
    error err = default!;

    if (!v.IsValid()) {
        return (default!, fmt.Errorf("asn1: cannot marshal nil value"u8));
    }
    // If the field is an interface{} then recurse into it.
    if (v.Kind() == reflect.ΔInterface && v.Type().NumMethod() == 0) {
        return makeField(v.Elem(), @params);
    }
    if (v.Kind() == reflect.ΔSlice && v.Len() == 0 && @params.omitEmpty) {
        return (((bytesEncoder)default!), default!);
    }
    if (@params.optional && @params.defaultValue != nil && canHaveDefaultValue(v.Kind())) {
        var defaultValue = reflect.New(v.Type()).Elem();
        defaultValue.SetInt(@params.defaultValue.Value);
        if (reflect.DeepEqual(v.Interface(), defaultValue.Interface())) {
            return (((bytesEncoder)default!), default!);
        }
    }
    // If no default value is given then the zero value for the type is
    // assumed to be the default value. This isn't obviously the correct
    // behavior, but it's what Go has traditionally done.
    if (@params.optional && @params.defaultValue == nil) {
        if (reflect.DeepEqual(v.Interface(), reflect.Zero(v.Type()).Interface())) {
            return (((bytesEncoder)default!), default!);
        }
    }
    if (AreEqual(v.Type(), rawValueType)) {
        var rv = v.Interface()._<RawValue>();
        if (len(rv.FullBytes) != 0) {
            return (((bytesEncoder)rv.FullBytes), default!);
        }
        var tΔ1 = @new<taggedEncoder>();
        tΔ1.Value.tag = ((bytesEncoder)appendTagAndLength((~tΔ1).scratch[..0], new tagAndLength(rv.Class, rv.Tag, len(rv.Bytes), rv.IsCompound)));
        tΔ1.Value.body = ((bytesEncoder)rv.Bytes);
        return (new taggedEncoderжencoder(tΔ1), default!);
    }
    var (matchAny, tag, isCompound, ok) = getUniversalType(v.Type());
    if (!ok || matchAny) {
        return (default!, new StructuralError(fmt.Sprintf("unknown Go type: %v"u8, v.Type())));
    }
    if (@params.timeType != 0 && tag != TagUTCTime) {
        return (default!, new StructuralError("explicit time type given to non-time member"u8));
    }
    if (@params.stringType != 0 && tag != TagPrintableString) {
        return (default!, new StructuralError("explicit string type given to non-string member"u8));
    }
    var exprᴛ1 = tag;
    if (exprᴛ1 == TagPrintableString) {
        if (@params.stringType == 0){
            // This is a string without an explicit string type. We'll use
            // a PrintableString if the character set in the string is
            // sufficiently limited, otherwise we'll use a UTF8String.
            foreach (var (_, r) in v.String()) {
                if (r >= utf8.RuneSelf || !isPrintable((byte)r, rejectAsterisk, rejectAmpersand)) {
                    if (!utf8.ValidString(v.String())) {
                        return (default!, errors.New(asn1StringNotValidUtf8ˢ));
                    }
                    tag = TagUTF8String;
                    break;
                }
            }
        } else {
            tag = @params.stringType;
        }
    }
    else if (exprᴛ1 == TagUTCTime) {
        if (@params.timeType == TagGeneralizedTime || outsideUTCRange(v.Interface()._<time.Time>())) {
            tag = TagGeneralizedTime;
        }
    }

    if (@params.set) {
        if (tag != TagSequence) {
            return (default!, new StructuralError("non sequence tagged as set"u8));
        }
        tag = TagSet;
    }
    // makeField can be called for a slice that should be treated as a SET
    // but doesn't have params.set set, for instance when using a slice
    // with the SET type name suffix. In this case getUniversalType returns
    // TagSet, but makeBody doesn't know about that so will treat the slice
    // as a sequence. To work around this we set params.set.
    if (tag == TagSet && !@params.set) {
        @params.set = true;
    }
    var t = @new<taggedEncoder>();
    (t.Value.body, err) = makeBody(v, @params);
    if (err != default!) {
        return (default!, err);
    }
    nint bodyLen = (~t).body.Len();
    nint @class = ClassUniversal;
    if (@params.tag != nil) {
        if (@params.application){
            @class = ClassApplication;
        } else 
        if (@params.@private){
            @class = ClassPrivate;
        } else {
            @class = ClassContextSpecific;
        }
        if (@params.@explicit) {
            t.Value.tag = ((bytesEncoder)appendTagAndLength((~t).scratch[..0], new tagAndLength(ClassUniversal, tag, bodyLen, isCompound)));
            var tt = @new<taggedEncoder>();
            tt.Value.body = new taggedEncoderжencoder(t);
            tt.Value.tag = ((bytesEncoder)appendTagAndLength((~tt).scratch[..0], new tagAndLength(
                @class: @class,
                tag: @params.tag.Value,
                length: bodyLen + (~t).tag.Len(),
                isCompound: true
            )));
            return (new taggedEncoderжencoder(tt), default!);
        }
        // implicit tag.
        tag = @params.tag.Value;
    }
    t.Value.tag = ((bytesEncoder)appendTagAndLength((~t).scratch[..0], new tagAndLength(@class, tag, bodyLen, isCompound)));
    return (new taggedEncoderжencoder(t), default!);
}

// Marshal returns the ASN.1 encoding of val.
//
// In addition to the struct tags recognized by Unmarshal, the following can be
// used:
//
//	ia5:         causes strings to be marshaled as ASN.1, IA5String values
//	omitempty:   causes empty slices to be skipped
//	printable:   causes strings to be marshaled as ASN.1, PrintableString values
//	utf8:        causes strings to be marshaled as ASN.1, UTF8String values
//	utc:         causes time.Time to be marshaled as ASN.1, UTCTime values
//	generalized: causes time.Time to be marshaled as ASN.1, GeneralizedTime values
public static (slice<byte>, error) Marshal(any val) {
    return MarshalWithParams(val, ""u8);
}

// MarshalWithParams allows field parameters to be specified for the
// top-level element. The form of the params is the same as the field tags.
public static (slice<byte>, error) MarshalWithParams(any val, @string @params) {
    var (e, err) = makeField(reflect.ValueOf(val), parseFieldParameters(@params));
    if (err != default!) {
        return (default!, err);
    }
    var b = new slice<byte>(e.Len());
    e.Encode(b);
    return (b, default!);
}

} // end asn1_package
