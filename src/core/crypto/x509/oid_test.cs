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
internal static slice<oidTestsᴛ1> oidTests = [
    new([], false, "", default),
    new([0x80, 0x01], false, "", default),
    new([0x01, 0x80, 0x01], false, "", default),

    new([1, 2, 3], true, "0.1.2.3", [0, 1, 2, 3]),
    new([41, 2, 3], true, "1.1.2.3", [1, 1, 2, 3]),
    new([86, 2, 3], true, "2.6.2.3", [2, 6, 2, 3]),

    new([41, 255, 255, 255, 127], true, "1.1.268435455", [1, 1, 268435455]),
    new([41, 0x87, 255, 255, 255, 127], true, "1.1.2147483647", [1, 1, 2147483647]),
    new([41, 255, 255, 255, 255, 127], true, "1.1.34359738367", [1, 1, 34359738367UL]),
    new([42, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "1.2.9223372036854775807", [1, 2, 9223372036854775807UL]),
    new([43, 0x81, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "1.3.18446744073709551615", [1, 3, 18446744073709551615UL]),
    new([44, 0x83, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "1.4.36893488147419103231", default),
    new([85, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.5.1180591620717411303423", default),
    new([85, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.5.19342813113834066795298815", default),

    new([255, 255, 255, 127], true, "2.268435375", [2, 268435375]),
    new([0x87, 255, 255, 255, 127], true, "2.2147483567", [2, 2147483567]),
    new([255, 127], true, "2.16303", [2, 16303]),
    new([255, 255, 255, 255, 127], true, "2.34359738287", [2, 34359738287UL]),
    new([255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.9223372036854775727", [2, 9223372036854775727UL]),
    new([0x81, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.18446744073709551535", [2, 18446744073709551535UL]),
    new([0x83, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.36893488147419103151", default),
    new([255, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.1180591620717411303343", default),
    new([255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 127], true, "2.19342813113834066795298735", default),

    new([41, (byte)(0x80 | 66), (byte)(0x80 | 44), (byte)(0x80 | 11), 33], true, "1.1.139134369", [1, 1, 139134369]),
    new([(byte)(0x80 | 66), (byte)(0x80 | 44), (byte)(0x80 | 11), 33], true, "2.139134289", [2, 139134289])
];

public static void TestOID(ж<testing.T> t) {
    foreach (var (_, v) in range(oidTests)) {
        var (oid, ok) = newOIDFromDER(v.raw);
        if (ok != v.valid) {
            t.Errorf("newOIDFromDER(%v) = (%v, %v); want = (OID, %v)", v.raw, oid, ok, v.valid);
            continue;
        }

        if (!ok) {
            continue;
        }

        @string str = oid.String(); if (str != v.str) {
            t.Errorf("(%#v).String() = %v, want; %v", oid, str, v.str);
        }

        asn1.ObjectIdentifier asn1OID = default;
        foreach (var (_, vΔ1) in range(v.ints)) {
            if (vΔ1 > math.MaxInt32) {
                asn1OID = default;
                break;
            }
            asn1OID = append(asn1OID, (nint)vΔ1);
        }

        (var o, ok) = oid.toASN1OID();
        var shouldOk = asn1OID != nil; if (shouldOk != ok) {
            t.Errorf("(%#v).toASN1OID() = (%v, %v); want = (%v, %v)", oid, o, ok, asn1OID, shouldOk);
            continue;
        }

        if (asn1OID != nil && !o.Equal(asn1OID)) {
            t.Errorf("(%#v).toASN1OID() = (%v, true); want = (%v, true)", oid, o, asn1OID);
        }

        if (v.ints != nil) {
            var (oid2, err) = OIDFromInts(v.ints);
            if (err != nil) {
                t.Errorf("OIDFromInts(%v) = (%v, %v); want = (%v, nil)", v.ints, oid2, err, oid);
            }
            if (!oid2.Equal(oid)) {
                t.Errorf("OIDFromInts(%v) = (%v, nil); want = (%v, nil)", v.ints, oid2, oid);
            }
        }
    }
}

internal partial struct TestInvalidOID_cases /*dyn*/ {
    internal @string str;
    internal slice<uint64> ints;
}

public static void TestInvalidOID(ж<testing.T> t) {
    var cases = slice<TestInvalidOID_cases>([
        new(str: "", ints: []),
        new(str: "1", ints: [1]),
        new(str: "3", ints: [3]),
        new(str: "3.100.200", ints: [3, 100, 200]),
        new(str: "1.81", ints: [1, 81]),
        new(str: "1.81.200", ints: [1, 81, 200])
    ]);

    foreach (var (_, tt) in range(cases)) {
        var (oid, err) = OIDFromInts(tt.ints);
        if (err == nil) {
            t.Errorf("OIDFromInts(%v) = (%v, %v); want = (OID{}, %v)", tt.ints, oid, err, errInvalidOID);
        }

        (var oid2, err) = ParseOID(tt.str);
        if (err == nil) {
            t.Errorf("ParseOID(%v) = (%v, %v); want = (OID{}, %v)", tt.str, oid2, err, errInvalidOID);
        }

        global::go.crypto.x509_package.ΔOID oid3 = default;
        err = oid3.UnmarshalText(slice<byte>(tt.str));
        if (err == nil) {
            t.Errorf("(*OID).UnmarshalText(%v) = (%v, %v); want = (OID{}, %v)", tt.str, oid3, err, errInvalidOID);
        }
    }
}

internal partial struct TestOIDEqual_type /*dyn*/ {
    internal global::go.crypto.x509_package.ΔOID oid;
    internal global::go.crypto.x509_package.ΔOID oid2;
    internal bool eq;
}

public static void TestOIDEqual(ж<testing.T> t) {
    slice<TestOIDEqual_type> cases = slice<TestOIDEqual_type>([
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: mustNewOIDFromInts([1, 2, 3]), eq: true),
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: mustNewOIDFromInts([1, 2, 4]), eq: false),
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: mustNewOIDFromInts([1, 2, 3, 4]), eq: false),
        new(oid: mustNewOIDFromInts([2, 33, 22]), oid2: mustNewOIDFromInts([2, 33, 23]), eq: false),
        new(oid: new ΔOID(), oid2: new ΔOID(), eq: true),
        new(oid: new ΔOID(), oid2: mustNewOIDFromInts([2, 33, 23]), eq: false)
    ]);

    foreach (var (_, tt) in range(cases)) {
        var eq = tt.oid.Equal(tt.oid2); if (eq != tt.eq) {
            t.Errorf("(%v).Equal(%v) = %v, want %v", tt.oid, tt.oid2, eq, tt.eq);
        }
    }
}

internal static encoding.BinaryMarshaler _ᴛ1ʗ = new x509_test_package.x509_ΔOIDᴠBinaryMarshaler(new ΔOID());
internal static encoding.BinaryUnmarshaler _ᴛ2ʗ = new x509_test_package.x509_ΔOIDжBinaryUnmarshaler(@new<global::go.crypto.x509_package.ΔOID>());
internal static encoding.TextMarshaler _ᴛ3ʗ = new x509_test_package.x509_ΔOIDᴠTextMarshaler(new ΔOID());
internal static encoding.TextUnmarshaler _ᴛ4ʗ = new x509_test_package.x509_ΔOIDжTextUnmarshaler(@new<global::go.crypto.x509_package.ΔOID>());

internal partial struct TestOIDMarshal_cases /*dyn*/ {
    internal @string @in;
    internal global::go.crypto.x509_package.ΔOID @out;
    internal error err;
}

public static void TestOIDMarshal(ж<testing.T> t) {
    var cases = slice<TestOIDMarshal_cases>([
        new(@in: "", err: errInvalidOID),
        new(@in: "0", err: errInvalidOID),
        new(@in: "1", err: errInvalidOID),
        new(@in: ".1", err: errInvalidOID),
        new(@in: ".1.", err: errInvalidOID),
        new(@in: "1.", err: errInvalidOID),
        new(@in: "1..", err: errInvalidOID),
        new(@in: "1.2.", err: errInvalidOID),
        new(@in: "1.2.333.", err: errInvalidOID),
        new(@in: "1.2.333..", err: errInvalidOID),
        new(@in: "1.2..", err: errInvalidOID),
        new(@in: "+1.2", err: errInvalidOID),
        new(@in: "-1.2", err: errInvalidOID),
        new(@in: "1.-2", err: errInvalidOID),
        new(@in: "1.2.+333", err: errInvalidOID)
    ]);

    foreach (var (_, v) in range(oidTests)) {
        var (oid, ok) = newOIDFromDER(v.raw);
        if (!ok) {
            continue;
        }
        cases = append(cases, new TestOIDMarshal_cases(
            @in: v.str,
            @out: oid,
            err: default
        ));
    }

    foreach (var (_, tt) in range(cases)) {
        var (o, err) = ParseOID(tt.@in);
        if (!AreEqual(err, tt.err)) {
            t.Errorf("ParseOID(%q) = %v; want = %v", tt.@in, err, tt.err);
            continue;
        }

        global::go.crypto.x509_package.ΔOID o2 = default;
        err = o2.UnmarshalText(slice<byte>(tt.@in));
        if (!AreEqual(err, tt.err)) {
            t.Errorf("(*OID).UnmarshalText(%q) = %v; want = %v", tt.@in, err, tt.err);
            continue;
        }

        if (err != nil) {
            continue;
        }

        if (!o.Equal(tt.@out)) {
            t.Errorf("(*OID).UnmarshalText(%q) = %v; want = %v", tt.@in, o, tt.@out);
            continue;
        }

        if (!o2.Equal(tt.@out)) {
            t.Errorf("ParseOID(%q) = %v; want = %v", tt.@in, o2, tt.@out);
            continue;
        }

        (var marshalled, err) = o.MarshalText();
        if (((sstring)marshalled) != tt.@in || err != nil) {
            t.Errorf("(%#v).MarshalText() = (%v, %v); want = (%v, nil)", o, (@string)marshalled, err, tt.@in);
            continue;
        }

        var textAppend = new slice<byte>(4);
        (textAppend, err) = o.AppendText(textAppend);
        textAppend = textAppend[4..];
        if (((sstring)textAppend) != tt.@in || err != nil) {
            t.Errorf("(%#v).AppendText() = (%v, %v); want = (%v, nil)", o, (@string)textAppend, err, tt.@in);
            continue;
        }

        (var binary, err) = o.MarshalBinary();
        if (err != nil) {
            t.Errorf("(%#v).MarshalBinary() = %v; want = nil", o, err);
        }

        global::go.crypto.x509_package.ΔOID o3 = default;
        var errΔ1 = o3.UnmarshalBinary(binary); if (errΔ1 != nil) {
            t.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = nil", binary, errΔ1);
        }

        if (!o3.Equal(tt.@out)) {
            t.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = %v", binary, o3, tt.@out);
            continue;
        }

        var binaryAppend = new slice<byte>(4);
        (binaryAppend, err) = o.AppendBinary(binaryAppend);
        binaryAppend = binaryAppend[4..];
        if (err != nil) {
            t.Errorf("(%#v).AppendBinary() = %v; want = nil", o, err);
        }

        global::go.crypto.x509_package.ΔOID o4 = default;
        var errΔ2 = o4.UnmarshalBinary(binaryAppend); if (errΔ2 != nil) {
            t.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = nil", binaryAppend, errΔ2);
        }

        if (!o4.Equal(tt.@out)) {
            t.Errorf("(*OID).UnmarshalBinary(%v) = %v; want = %v", binaryAppend, o4, tt.@out);
            continue;
        }
    }
}

internal partial struct TestOIDEqualASN1OID_type /*dyn*/ {
    internal global::go.crypto.x509_package.ΔOID oid;
    internal asn1.ObjectIdentifier oid2;
    internal bool eq;
}

public static void TestOIDEqualASN1OID(ж<testing.T> t) {
    var maxInt32PlusOne = 2147483648L;
    slice<TestOIDEqualASN1OID_type> cases = slice<TestOIDEqualASN1OID_type>([
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 3}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 2, 3]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 2, 3, 4}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 22]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 23}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 23]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 22}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 127]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 127}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([1, 33, 128]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 127}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 128]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 128}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([1, 33, 129]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 129}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([1, 33, 128]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 129}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 129]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 128}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, 255]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 255}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([1, 33, 256]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 256}.slice()), eq: true),
        new(oid: mustNewOIDFromInts([2, 33, 257]), oid2: new asn1.ObjectIdentifier(new nint[]{2, 33, 256}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([2, 33, 256]), oid2: new asn1.ObjectIdentifier(new nint[]{2, 33, 257}.slice()), eq: false),

        new(oid: mustNewOIDFromInts([1, 33]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, math.MaxInt32}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, math.MaxInt32]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33}.slice()), eq: false),
        new(oid: mustNewOIDFromInts([1, 33, math.MaxInt32]), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, math.MaxInt32}.slice()), eq: true),
        new(
            oid: mustNewOIDFromInts([1, 33, math.MaxInt32 + 1]),
            oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, /*convert to int, so that it compiles on 32bit*/ (nint)maxInt32PlusOne}.slice()),
            eq: false
        ),

        new(oid: mustNewOIDFromInts([1, 33, 256]), oid2: new asn1.ObjectIdentifier(new nint[]{}.slice()), eq: false),
        new(oid: new ΔOID(), oid2: new asn1.ObjectIdentifier(new nint[]{1, 33, 256}.slice()), eq: false),
        new(oid: new ΔOID(), oid2: new asn1.ObjectIdentifier(new nint[]{}.slice()), eq: false)
    ]);

    foreach (var (_, tt) in range(cases)) {
        var eq = tt.oid.EqualASN1OID(tt.oid2); if (eq != tt.eq) {
            t.Errorf("(%v).EqualASN1OID(%v) = %v, want %v", tt.oid, tt.oid2, eq, tt.eq);
        }
    }
}

public static void TestOIDUnmarshalBinary(ж<testing.T> t) {
    foreach (var (_, tt) in range(oidTests)) {
        global::go.crypto.x509_package.ΔOID o = default;
        var err = o.UnmarshalBinary(tt.raw);

        var expectErr = errInvalidOID;
        if (tt.valid) {
            expectErr = default;
        }

        if (!AreEqual(err, expectErr)) {
            t.Errorf("(o *OID).UnmarshalBinary(%v) = %v; want = %v; (o = %v)", tt.raw, err, expectErr, o);
        }
    }
}

public static void BenchmarkOIDMarshalUnmarshalText(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var oid = mustNewOIDFromInts([1, 2, 3, 9999, 1024]);
    foreach (var _ᴛ1 in range(b.N)) {
        var (text, err) = oid.MarshalText();
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        global::go.crypto.x509_package.ΔOID o = default;
        var errΔ1 = o.UnmarshalText(text); if (errΔ1 != nil) {
            Ꮡb.Fatal(errΔ1);
        }
    }
}

public static void TestOIDFromASN1OID(ж<testing.T> t) {
    var negativeComponentOID = new asn1.ObjectIdentifier(new nint[]{-1}.slice());
    var (_, err) = OIDFromASN1OID(negativeComponentOID);
    if (err == nil || err.Error() != "x509: OID components must be non-negative"u8) {
        t.Fatalf("OIDFromASN1OID() = %v; want = \"x509: OID components must be non-negative\"", err);
    }

    var shortOID = new asn1.ObjectIdentifier(new nint[]{1}.slice());
    (_, err) = OIDFromASN1OID(shortOID);
    if (err == nil || !AreEqual(err, errInvalidOID)) {
        t.Fatalf("OIDFromASN1OID() = %v; want = %q", err, errInvalidOID);
    }
    var invalidOIDFirstComponent = new asn1.ObjectIdentifier(new nint[]{255, 1}.slice());
    (_, err) = OIDFromASN1OID(invalidOIDFirstComponent);
    if (err == nil || !AreEqual(err, errInvalidOID)) {
        t.Fatalf("OIDFromASN1OID() = %v; want = %q", err, errInvalidOID);
    }
    var invalidOIDSecondComponent = new asn1.ObjectIdentifier(new nint[]{1, 255}.slice());
    (_, err) = OIDFromASN1OID(invalidOIDSecondComponent);
    if (err == nil || !AreEqual(err, errInvalidOID)) {
        t.Fatalf("OIDFromASN1OID() = %v; want = %q", err, errInvalidOID);
    }
}

} // end x509_internal_test_package
