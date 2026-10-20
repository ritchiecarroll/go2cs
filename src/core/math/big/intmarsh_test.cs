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

internal static slice<@string> encodingTests = new @string[]{
    "0"u8,
    "1"u8,
    "2"u8,
    "10"u8,
    "1000"u8,
    "1234567890"u8,
    "298472983472983471903246121093472394872319615612417471234712061"u8
}.slice();

public static void TestIntGobEncoding(ж<testing.T> Ꮡt) {
    ref var medium = ref heap(new bytes.Buffer(), out var Ꮡmedium);
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡmedium));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(Ꮡmedium));
    foreach (var (_, test) in encodingTests) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            @string x = sign + test;
            medium.Reset(); // empty buffer for each test case (in case of failures)
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            {
                var err = enc.Encode(Ꮡtx); if (err != default!) {
                    Ꮡt.Errorf("encoding of %s failed: %s"u8, Ꮡtx, err);
                    continue;
                }
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            {
                var err = dec.Decode(Ꮡrx); if (err != default!) {
                    Ꮡt.Errorf("decoding of %s failed: %s"u8, Ꮡtx, err);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                Ꮡt.Errorf("transmission of %s failed: got %s want %s"u8, Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

// Sending a nil Int pointer (inside a slice) on a round trip through gob should yield a zero.
// TODO: top-level nils.
public static void TestGobEncodingNilIntInSlice(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(buf));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(buf));

    ref var @in = ref heap<slice<ж<global::go.math.big_package.ΔInt>>>(out var Ꮡin);

    @in = new slice<ж<global::go.math.big_package.ΔInt>>(1);
    var err = enc.Encode(Ꮡin);
    if (err != default!) {
        Ꮡt.Errorf("gob encode failed: %q"u8, err);
    }
    ref var @out = ref heap<slice<ж<global::go.math.big_package.ΔInt>>>(out var Ꮡout);
    err = dec.Decode(Ꮡout);
    if (err != default!) {
        Ꮡt.Fatalf("gob decode failed: %q"u8, err);
    }
    if (len(@out) != 1) {
        Ꮡt.Fatalf("wrong len; want 1 got %d"u8, len(@out));
    }
    ref var zero = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡzero);
    if (@out[0].Cmp(Ꮡzero) != 0) {
        Ꮡt.Fatalf("transmission of (*Int)(nil) failed: got %s want 0"u8, @out);
    }
}

public static void TestIntJSONEncoding(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in encodingTests) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            var (b, err) = json.Marshal(Ꮡtx);
            if (err != default!) {
                Ꮡt.Errorf("marshaling of %s failed: %s"u8, Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            {
                var errΔ1 = json.Unmarshal(b, Ꮡrx); if (errΔ1 != default!) {
                    Ꮡt.Errorf("unmarshaling of %s failed: %s"u8, Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                Ꮡt.Errorf("JSON encoding of %s failed: got %s want %s"u8, Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nullˢ = "null"u8;

public static void TestIntJSONEncodingNil(ж<testing.T> Ꮡt) {
    ж<global::go.math.big_package.ΔInt> x = default!;
    var (b, err) = x.MarshalJSON();
    if (err != default!) {
        Ꮡt.Fatalf("marshaling of nil failed: %s"u8, err);
    }
    @string got = (@string)b;
    @string want = nullˢ;
    if (got != want) {
        Ꮡt.Fatalf("marshaling of nil failed: got %s want %s"u8, got, want);
    }
}

public static void TestIntXMLEncoding(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in encodingTests) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 0);
            var (b, err) = xml.Marshal(Ꮡtx);
            if (err != default!) {
                Ꮡt.Errorf("marshaling of %s failed: %s"u8, Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            {
                var errΔ1 = xml.Unmarshal(b, Ꮡrx); if (errΔ1 != default!) {
                    Ꮡt.Errorf("unmarshaling of %s failed: %s"u8, Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                Ꮡt.Errorf("XML encoding of %s failed: got %s want %s"u8, Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestIntAppendText(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in encodingTests) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            @string x = sign + test;
            ref var tx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtx);
            Ꮡtx.SetString(x, 10);
            var buf = new slice<byte>(4, 32);
            var (b, err) = Ꮡtx.AppendText(buf);
            if (err != default!) {
                Ꮡt.Errorf("marshaling of %s failed: %s"u8, Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡrx);
            {
                var errΔ1 = Ꮡrx.UnmarshalText(b[4..]); if (errΔ1 != default!) {
                    Ꮡt.Errorf("unmarshaling of %s failed: %s"u8, Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                Ꮡt.Errorf("AppendText of %s failed: got %s want %s"u8, Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestIntAppendTextNil(ж<testing.T> Ꮡt) {
    ж<global::go.math.big_package.ΔInt> x = default!;
    var buf = new slice<byte>(4, 16);
    var (data, _) = x.AppendText(buf);
    if (((sstring)(data[4..])) != "<nil>"u8) {
        Ꮡt.Errorf("got %q, want <nil>"u8, data[4..]);
    }
}

} // end big_internal_test_package
