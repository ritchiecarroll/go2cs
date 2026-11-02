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

public static void TestRatGobEncoding(ж<testing.T> t) {
    ref var medium = ref heap(new bytes.Buffer(), out var Ꮡmedium);
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡmedium));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(Ꮡmedium));
    foreach (var (_, test) in encodingTests) {
        medium.Reset(); // empty buffer for each test case (in case of failures)
        ref var tx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡtx);
        Ꮡtx.SetString(test + ".14159265"u8);
        {
            var err = enc.Encode(Ꮡtx); if (err != nil) {
                t.Errorf("encoding of %s failed: %s", Ꮡtx, err);
                continue;
            }
        }
        ref var rx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡrx);
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

// Sending a nil Rat pointer (inside a slice) on a round trip through gob should yield a zero.
// TODO: top-level nils.
public static void TestGobEncodingNilRatInSlice(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(buf));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(buf));

    ref var @in = ref heap<slice<ж<global::go.math.big_package.ΔRat>>>(out var Ꮡin);

    @in = new slice<ж<global::go.math.big_package.ΔRat>>(1);
    var err = enc.Encode(Ꮡin);
    if (err != nil) {
        t.Errorf("gob encode failed: %q", err);
    }
    ref var @out = ref heap<slice<ж<global::go.math.big_package.ΔRat>>>(out var Ꮡout);
    err = dec.Decode(Ꮡout);
    if (err != nil) {
        t.Fatalf("gob decode failed: %q", err);
    }
    if (len(@out) != 1) {
        t.Fatalf("wrong len; want 1 got %d", len(@out));
    }
    ref var zero = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡzero);
    if (@out[0].Cmp(Ꮡzero) != 0) {
        t.Fatalf("transmission of (*Int)(nil) failed: got %s want 0", @out);
    }
}

internal static slice<@string> ratNums = slice<@string>([
    "-141592653589793238462643383279502884197169399375105820974944592307816406286",
    "-1415926535897932384626433832795028841971",
    "-141592653589793",
    "-1",
    "0",
    "1",
    "141592653589793",
    "1415926535897932384626433832795028841971",
    "141592653589793238462643383279502884197169399375105820974944592307816406286"
]);

internal static slice<@string> ratDenoms = slice<@string>([
    "1",
    "718281828459045",
    "7182818284590452353602874713526624977572",
    "718281828459045235360287471352662497757247093699959574966967627724076630353"
]);

public static void TestRatJSONEncoding(ж<testing.T> t) {
    foreach (var (_, num) in ratNums) {
        foreach (var (_, denom) in ratDenoms) {
            ref var tx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡtx);
            Ꮡtx.SetString(num + "/"u8 + denom);
            var (b, err) = json.Marshal(Ꮡtx);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡrx);
            {
                var errΔ1 = json.Unmarshal(b, Ꮡrx); if (errΔ1 != nil) {
                    t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("JSON encoding of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestRatXMLEncoding(ж<testing.T> t) {
    foreach (var (_, num) in ratNums) {
        foreach (var (_, denom) in ratDenoms) {
            ref var tx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡtx);
            Ꮡtx.SetString(num + "/"u8 + denom);
            var (b, err) = xml.Marshal(Ꮡtx);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡrx);
            {
                var errΔ1 = xml.Unmarshal(b, Ꮡrx); if (errΔ1 != nil) {
                    t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("XML encoding of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

public static void TestRatGobDecodeShortBuffer(ж<testing.T> t) {
    foreach (var (_, tc) in slice<slice<byte>>([
        slice<byte>([0x2]),
        slice<byte>([0x2, 0x0, 0x0, 0x0, 0xff]),
        slice<byte>([0x2, 0xff, 0xff, 0xff, 0xff])
    ])) {
        var err = NewRat(1, 2).GobDecode(tc);
        if (err == nil) {
            t.Error(expectedGobDecodeToˢ);
        }
    }
}

public static void TestRatAppendText(ж<testing.T> t) {
    foreach (var (_, num) in ratNums) {
        foreach (var (_, denom) in ratDenoms) {
            ref var tx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡtx);
            Ꮡtx.SetString(num + "/"u8 + denom);
            var buf = new slice<byte>(4, 32);
            var (b, err) = Ꮡtx.AppendText(buf);
            if (err != nil) {
                t.Errorf("marshaling of %s failed: %s", Ꮡtx, err);
                continue;
            }
            ref var rx = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡrx);
            {
                var errΔ1 = Ꮡrx.UnmarshalText(b[4..]); if (errΔ1 != nil) {
                    t.Errorf("unmarshaling of %s failed: %s", Ꮡtx, errΔ1);
                    continue;
                }
            }
            if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                t.Errorf("AppendText of %s failed: got %s want %s", Ꮡtx, Ꮡrx, Ꮡtx);
            }
        }
    }
}

} // end big_internal_test_package
