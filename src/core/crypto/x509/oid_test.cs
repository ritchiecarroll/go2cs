// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using encoding = encoding_package;
using asn1 = go.encoding.asn1_package;
using math = math_package;
using testing = testing_package;
using go.encoding;
using static go.crypto.x509_package;

partial class x509_internal_test_package {


partial struct oidTestsᴛ1 /*dyn*/ {
    internal slice<byte> raw;
    internal bool valid;
    internal @string str;
    internal slice<uint64> ints;
}
internal static slice<oidTestsᴛ1> oidTests = new oidTestsᴛ1[]{
    new(new byte[]{}.slice(), false, ""u8, default!),
    new(new byte[]{0x80, 0x01}.slice(), false, ""u8, default!),
    new(new byte[]{0x01, 0x80, 0x01}.slice(), false, ""u8, default!),
    new(new byte[]{1, 2, 3}.slice(), true, "0.1.2.3"u8, new uint64[]{0, 1, 2, 3}.slice()),
    new(new byte[]{41, 2, 3}.slice(), true, "1.1.2.3"u8, new uint64[]{1, 1, 2, 3}.slice()),
    new(new byte[]{86, 2, 3}.slice(), true, "2.6.2.3"u8, new uint64[]{2, 6, 2, 3}.slice()),
    new(new byte[]{41, 255, 255, 255, 127}.slice(), true, "1.1.268435455"u8, new uint64[]{1, 1, 268435455}.slice()),
    new(new byte[]{41, 0x87, 255, 255, 255, 127}.slice(), true, "1.1.2147483647"u8, new uint64[]{1, 1, 2147483647}.slice()),
    new(new byte[]{41, 255, 255, 255, 255, 127}.slice(), true, "1.1.34359738367"u8, new uint64[]{1, 1, 34359738367UL}.slice()),
    new(new byte[]{42, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "1.2.9223372036854775807"u8, new uint64[]{1, 2, 9223372036854775807UL}.slice()),
    new(new byte[]{43, 0x81, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "1.3.18446744073709551615"u8, new uint64[]{1, 3, 18446744073709551615UL}.slice()),
    new(new byte[]{44, 0x83, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "1.4.36893488147419103231"u8, default!),
    new(new byte[]{85, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.5.1180591620717411303423"u8, default!),
    new(new byte[]{85, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.5.19342813113834066795298815"u8, default!),
    new(new byte[]{255, 255, 255, 127}.slice(), true, "2.268435375"u8, new uint64[]{2, 268435375}.slice()),
    new(new byte[]{0x87, 255, 255, 255, 127}.slice(), true, "2.2147483567"u8, new uint64[]{2, 2147483567}.slice()),
    new(new byte[]{255, 127}.slice(), true, "2.16303"u8, new uint64[]{2, 16303}.slice()),
    new(new byte[]{255, 255, 255, 255, 127}.slice(), true, "2.34359738287"u8, new uint64[]{2, 34359738287UL}.slice()),
    new(new byte[]{255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.9223372036854775727"u8, new uint64[]{2, 9223372036854775727UL}.slice()),
    new(new byte[]{0x81, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.18446744073709551535"u8, new uint64[]{2, 18446744073709551535UL}.slice()),
    new(new byte[]{0x83, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.36893488147419103151"u8, default!),
    new(new byte[]{255, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.1180591620717411303343"u8, default!),
    new(new byte[]{255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127}.slice(), true, "2.19342813113834066795298735"u8, default!),
    new(new byte[]{41, (byte)(0x80 | 66), (byte)(0x80 | 44), (byte)(0x80 | 11), 33}.slice(), true, "1.1.139134369"u8, new uint64[]{1, 1, 139134369}.slice()),
    new(new byte[]{(byte)(0x80 | 66), (byte)(0x80 | 44), (byte)(0x80 | 11), 33}.slice(), true, "2.139134289"u8, new uint64[]{2, 139134289}.slice())
}.slice();

public static void TestOID(ж<testing.T> Ꮡt) {
    foreach (var (_, v) in oidTests) {
        var (oid, ok) = newOIDFromDER(v.raw);
        if (ok != v.valid) {
            Ꮡt.Errorf("newOIDFromDER(%v) = (%v, %v); want = (OID, %v)"u8, v.raw, oid, ok, v.valid);
            continue;
        }
        if (!ok) {
            continue;
        }
        {
            @string str = oid.String(); if (str != v.str) {
                Ꮡt.Errorf("(%#v).String() = %v, want; %v"u8, oid, str, v.str);
            }
        }
        asn1.ObjectIdentifier asn1OID = default!;
        foreach (var (_, vΔ1) in v.ints) {
            if (vΔ1 > math.MaxInt32) {
                asn1OID = default!;
                break;
            }
            asn1OID = append(asn1OID, (nint)vΔ1);
        }
        (var o, ok) = oid.toASN1OID();
        {
            var shouldOk = asn1OID != default!; if (shouldOk != ok) {
                Ꮡt.Errorf("(%#v).toASN1OID() = (%v, %v); want = (%v, %v)"u8, oid, o, ok, asn1OID, shouldOk);
                continue;
            }
        }
        if (asn1OID != default! && !o.Equal(asn1OID)) {
            Ꮡt.Errorf("(%#v).toASN1OID() = (%v, true); want = (%v, true)"u8, oid, o, asn1OID);
        }
        if (v.ints != default!) {
            var (oid2, err) = OIDFromInts(v.ints);
            if (err != default!) {
                Ꮡt.Errorf("OIDFromInts(%v) = (%v, %v); want = (%v, nil)"u8, v.ints, oid2, err, oid);
            }
            if (!oid2.Equal(oid)) {
                Ꮡt.Errorf("OIDFromInts(%v) = (%v, nil); want = (%v, nil)"u8, v.ints, oid2, oid);
            }
        }
    }
}

internal partial struct TestInvalidOID_cases /*dyn*/ {
    internal @string str;
    internal slice<uint64> ints;
}

public static void TestInvalidOID(ж<testing.T> Ꮡt) {
    var cases = new TestInvalidOID_cases[]{
        new(str: ""u8, ints: new uint64[]{}.slice()),
        new(str: "1"u8, ints: new uint64[]{1}.slice()),
        new(str: "3"u8, ints: new uint64[]{3}.slice()),
        new(str: "3.100.200"u8, ints: new uint64[]{3, 100, 200}.slice()),
        new(str: "1.81"u8, ints: new uint64[]{1, 81}.slice()),
        new(str: "1.81.200"u8, ints: new uint64[]{1, 81, 200}.slice())
    }.slice();
    foreach (var (_, tt) in cases) {
        var (oid, err) = OIDFromInts(tt.ints);
        if (err == default!) {
            Ꮡt.Errorf("OIDFromInts(%v) = (%v, %v); want = (OID{}, %v)"u8, tt.ints, oid, err, errInvalidOID);
        }
        (var oid2, err) = ParseOID(tt.str);
        if (err == default!) {
            Ꮡt.Errorf("ParseOID(%v) = (%v, %v); want = (OID{}, %v)"u8, tt.str, oid2, err, errInvalidOID);
        }
        global::go.crypto.x509_package.OID oid3 = default!;
        err = oid3.UnmarshalText(slice<byte>(tt.str));
        if (err == default!) {
            Ꮡt.Errorf("(*OID).UnmarshalText(%v) = (%v, %v); want = (OID{}, %v)"u8, tt.str, oid3, err, errInvalidOID);
        }
    }
}

internal partial struct TestOIDEqual_type /*dyn*/ {
    internal global::go.crypto.x509_package.OID oid;
    internal global::go.crypto.x509_package.OID oid2;
    internal bool eq;
}

public static void TestOIDEqual(ж<testing.T> Ꮡt) {
    slice<TestOIDEqual_type> cases = new TestOIDEqual_type[]{
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: mustNewOIDFromInts(new uint64[]{1, 2, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: mustNewOIDFromInts(new uint64[]{1, 2, 3, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{2, 33, 22}.slice()), oid2: mustNewOIDFromInts(new uint64[]{2, 33, 23}.slice()), eq: false),
        new(oid: new OID(nil), oid2: new OID(nil), eq: true),
        new(oid: new OID(nil), oid2: mustNewOIDFromInts(new uint64[]{2, 33, 23}.slice()), eq: false)
    }.slice();
    foreach (var (_, tt) in cases) {
        {
            var eq = tt.oid.Equal(tt.oid2); if (eq != tt.eq) {
                Ꮡt.Errorf("(%v).Equal(%v) = %v, want %v"u8, tt.oid, tt.oid2, eq, tt.eq);
            }
        }
    }
}

internal static encoding.BinaryMarshaler _ᴛ1ʗ = new x509_test_package.x509_OIDᴠBinaryMarshaler(new OID(nil));
internal static encoding.BinaryUnmarshaler _ᴛ2ʗ = new x509_test_package.x509_OIDжBinaryUnmarshaler(@new<global::go.crypto.x509_package.OID>());
internal static encoding.TextMarshaler _ᴛ3ʗ = new x509_test_package.x509_OIDᴠTextMarshaler(new OID(nil));
internal static encoding.TextUnmarshaler _ᴛ4ʗ = new x509_test_package.x509_OIDжTextUnmarshaler(@new<global::go.crypto.x509_package.OID>());

internal partial struct TestOIDMarshal_cases /*dyn*/ {
    internal @string @in;
    internal global::go.crypto.x509_package.OID @out;
    internal error err;
}

public static void TestOIDMarshal(ж<testing.T> Ꮡt) {
    var cases = new TestOIDMarshal_cases[]{
        new(@in: ""u8, err: errInvalidOID),
        new(@in: "0"u8, err: errInvalidOID),
        new(@in: "1"u8, err: errInvalidOID),
        new(@in: ".1"u8, err: errInvalidOID),
        new(@in: ".1."u8, err: errInvalidOID),
        new(@in: "1."u8, err: errInvalidOID),
        new(@in: "1.."u8, err: errInvalidOID),
        new(@in: "1.2."u8, err: errInvalidOID),
        new(@in: "1.2.333."u8, err: errInvalidOID),
        new(@in: "1.2.333.."u8, err: errInvalidOID),
        new(@in: "1.2.."u8, err: errInvalidOID),
        new(@in: "+1.2"u8, err: errInvalidOID),
        new(@in: "-1.2"u8, err: errInvalidOID),
        new(@in: "1.-2"u8, err: errInvalidOID),
        new(@in: "1.2.+333"u8, err: errInvalidOID)
    }.slice();
    foreach (var (_, v) in oidTests) {
        var (oid, ok) = newOIDFromDER(v.raw);
        if (!ok) {
            continue;
        }
        cases = append(cases, new TestOIDMarshal_cases(
            @in: v.str,
            @out: oid,
            err: default!
        ));
    }
    foreach (var (_, tt) in cases) {
        var (o, err) = ParseOID(tt.@in);
        if (!AreEqual(err, tt.err)) {
            Ꮡt.Errorf("ParseOID(%q) = %v; want = %v"u8, tt.@in, err, tt.err);
            continue;
        }
        global::go.crypto.x509_package.OID o2 = default!;
        err = o2.UnmarshalText(slice<byte>(tt.@in));
        if (!AreEqual(err, tt.err)) {
            Ꮡt.Errorf("(*OID).UnmarshalText(%q) = %v; want = %v"u8, tt.@in, err, tt.err);
            continue;
        }
        if (err != default!) {
            continue;
        }
        if (!o.Equal(tt.@out)) {
            Ꮡt.Errorf("(*OID).UnmarshalText(%q) = %v; want = %v"u8, tt.@in, o, tt.@out);
            continue;
        }
        if (!o2.Equal(tt.@out)) {
            Ꮡt.Errorf("ParseOID(%q) = %v; want = %v"u8, tt.@in, o2, tt.@out);
            continue;
        }
        (var marshalled, err) = o.MarshalText();
        if (((sstring)marshalled) != tt.@in || err != default!) {
            Ꮡt.Errorf("(%#v).MarshalText() = (%v, %v); want = (%v, nil)"u8, o, ((@string)marshalled), err, tt.@in);
            continue;
        }
        var textAppend = new slice<byte>(4);
        (textAppend, err) = o.AppendText(textAppend);
        textAppend = textAppend[4..];
        if (((sstring)textAppend) != tt.@in || err != default!) {
            Ꮡt.Errorf("(%#v).AppendText() = (%v, %v); want = (%v, nil)"u8, o, ((@string)textAppend), err, tt.@in);
            continue;
        }
        (var binary, err) = o.MarshalBinary();
        if (err != default!) {
            Ꮡt.Errorf("(%#v).MarshalBinary() = %v; want = nil"u8, o, err);
        }
        global::go.crypto.x509_package.OID o3 = default!;
        {
            var errΔ1 = o3.UnmarshalBinary(binary); if (errΔ1 != default!) {
                Ꮡt.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = nil"u8, binary, errΔ1);
            }
        }
        if (!o3.Equal(tt.@out)) {
            Ꮡt.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = %v"u8, binary, o3, tt.@out);
            continue;
        }
        var binaryAppend = new slice<byte>(4);
        (binaryAppend, err) = o.AppendBinary(binaryAppend);
        binaryAppend = binaryAppend[4..];
        if (err != default!) {
            Ꮡt.Errorf("(%#v).AppendBinary() = %v; want = nil"u8, o, err);
        }
        global::go.crypto.x509_package.OID o4 = default!;
        {
            var errΔ2 = o4.UnmarshalBinary(binaryAppend); if (errΔ2 != default!) {
                Ꮡt.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = nil"u8, binaryAppend, errΔ2);
            }
        }
        if (!o4.Equal(tt.@out)) {
            Ꮡt.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = %v"u8, binaryAppend, o4, tt.@out);
            continue;
        }
    }
}

internal partial struct TestOIDEqualASN1OID_type /*dyn*/ {
    internal global::go.crypto.x509_package.OID oid;
    internal asn1.ObjectIdentifier oid2;
    internal bool eq;
}

public static void TestOIDEqualASN1OID(ж<testing.T> Ꮡt) {
    var maxInt32PlusOne = 2147483648L;
/*convert to int, so that it compiles on 32bit*/
    slice<TestOIDEqualASN1OID_type> cases = new TestOIDEqualASN1OID_type[]{
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 3}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 2, 3}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 3, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 22}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 23}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 23}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 22}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 127}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 127}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 128}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 127}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 128}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 128}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 129}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 129}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 128}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 129}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 129}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 128}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 255}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 255}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 256}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 256}.slice()), eq: true),
        new(oid: mustNewOIDFromInts(new uint64[]{2, 33, 257}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{2, 33, 256}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{2, 33, 256}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{2, 33, 257}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, math.MaxInt32}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, math.MaxInt32}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33}.slice()), eq: false),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, math.MaxInt32}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, math.MaxInt32}.slice()), eq: true),
        new(
            oid: mustNewOIDFromInts(new uint64[]{1, 33, math.MaxInt32 + 1}.slice()),
            oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, (nint)maxInt32PlusOne}.slice()),
            eq: false
        ),
        new(oid: mustNewOIDFromInts(new uint64[]{1, 33, 256}.slice()), oid2: new asn1.ObjectIdentifier(new nint[]{}.slice()), eq: false),
        new(oid: new OID(nil), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 256}.slice()), eq: false),
        new(oid: new OID(nil), oid2: new asn1.ObjectIdentifier(new nint[]{}.slice()), eq: false)
    }.slice();
    foreach (var (_, tt) in cases) {
        {
            var eq = tt.oid.EqualASN1OID(tt.oid2); if (eq != tt.eq) {
                Ꮡt.Errorf("(%v).EqualASN1OID(%v) = %v, want %v"u8, tt.oid, tt.oid2, eq, tt.eq);
            }
        }
    }
}

public static void TestOIDUnmarshalBinary(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in oidTests) {
        global::go.crypto.x509_package.OID o = default!;
        var err = o.UnmarshalBinary(tt.raw);
        var expectErr = errInvalidOID;
        if (tt.valid) {
            expectErr = default!;
        }
        if (!AreEqual(err, expectErr)) {
            Ꮡt.Errorf("(o *OID).UnmarshalBinary(%v) = %v; want = %v; (o = %v)"u8, tt.raw, err, expectErr, o);
        }
    }
}

public static void BenchmarkOIDMarshalUnmarshalText(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var oid = mustNewOIDFromInts(new uint64[]{1, 2, 3, 9999, 1024}.slice());
    foreach (var _ᴛ1 in range(b.N)) {
        var (text, err) = oid.MarshalText();
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        global::go.crypto.x509_package.OID o = default!;
        {
            var errΔ1 = o.UnmarshalText(text); if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
}

} // end x509_internal_test_package
