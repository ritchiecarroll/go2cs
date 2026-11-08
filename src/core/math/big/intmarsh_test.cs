// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using gob = encoding.gob_package;
using json = encoding.json_package;
using xml = encoding.xml_package;
using testing = testing_package;
using encoding;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static slice<@string> encodingTests = [
    "0",
    "1",
    "2",
    "10",
    "1000",
    "1234567890",
    "298472983472983471903246121093472394872319615612417471234712061"
];

public static void TestIntGobEncoding(ж<testing.T> t) {
    ref var medium = ref heap(new bytes.Buffer(), out var Ꮡmedium);
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡmedium));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(Ꮡmedium));
    foreach (var (_, test) in range(encodingTests)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            @string x = sign + test;
            medium.Reset(); // empty buffer for each test case (in case of failures)
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            {
                var err = enc.Encode(Ꮡtx); if (err != nil) {
                    t.Errorf("encoding of %s failed: %s", Ꮡtx, err);
                    continue;
                }
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            {
                var err = dec.Decode(Ꮡrx); if (err != nil) {
                    t.Errorf("decoding of %s failed: %s", Ꮡtx, err);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("transmission of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

// Sending a nil Int pointer (inside a slice) on a round trip through gob should yield a zero.
// TODO: top-level nils.
public static void TestGobEncodingNilIntInSlice(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(buf));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(buf));

    ref var @in = ref heap<slice<ж<global::go.math.big_package.ΔInt>>>(out var Ꮡin);

    @in = new slice<ж<global::go.math.big_package.ΔInt>>(1);
    var err = enc.Encode(Ꮡin);
    if (err != nil) {
        t.Errorf("gob encode failed: %q", err);
    }
    ref var @out = ref heap<slice<ж<global::go.math.big_package.ΔInt>>>(out var Ꮡout);
    err = dec.Decode(Ꮡout);
    if (err != nil) {
        t.Fatalf("gob decode failed: %q", err);
    }
    if (len(@out) != 1) {
        t.Fatalf("wrong len; want 1 got %d", len(@out));
    }
    ref var zero = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡzero);
    if (@out[0].Cmp(Ꮡzero) != 0) {
        t.Fatalf("transmission of (*Int)(nil) failed: got %s want 0", @out);
    }
}

public static void TestIntJSONEncoding(ж<testing.T> t) {
    foreach (var (_, test) in range(encodingTests)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            var (b, err) = json.Marshal(Ꮡtx);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            var errΔ1 = json.Unmarshal(b, Ꮡrx); if (errΔ1 != nil) {
                t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                continue;
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("JSON encoding of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nullˢ => ref nullˢᵇ;
private static readonly @string nullˢᵇ = "null"u8;

public static void TestIntJSONEncodingNil(ж<testing.T> t) {
    ж<global::go.math.big_package.ΔInt> x = default;
    var (b, err) = x.MarshalJSON();
    if (err != nil) {
        t.Fatalf("marshaling of nil failed: %s", err);
    }
    @string got = (@string)b;
    @string want = nullˢ;
    if (got != want) {
        t.Fatalf("marshaling of nil failed: got %s want %s", got, want);
    }
}

public static void TestIntXMLEncoding(ж<testing.T> t) {
    foreach (var (_, test) in range(encodingTests)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 0);
            var (b, err) = xml.Marshal(Ꮡtx);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            var errΔ1 = xml.Unmarshal(b, Ꮡrx); if (errΔ1 != nil) {
                t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                continue;
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("XML encoding of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestIntAppendText(ж<testing.T> t) {
    foreach (var (_, test) in range(encodingTests)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            var buf = new slice<byte>(4, 32);
            var (b, err) = Ꮡtx.AppendText(buf);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            var errΔ1 = Ꮡrx.UnmarshalText(b[4..]); if (errΔ1 != nil) {
                t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                continue;
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("AppendText of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestIntAppendTextNil(ж<testing.T> t) {
    ж<global::go.math.big_package.ΔInt> x = default;
    var buf = new slice<byte>(4, 16);
    var (data, _) = x.AppendText(buf);
    if (((sstring)(data[4..])) != "<nil>"u8) {
        t.Errorf("got %q, want <nil>", data[4..]);
    }
}

} // end big_internal_test_package
