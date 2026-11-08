// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using gob = encoding.gob_package;
using json = encoding.json_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using static go.math.big_package;

partial class big_internal_test_package {

internal static slice<@string> floatVals = [
    "0",
    "1",
    "0.1",
    "2.71828",
    "1234567890",
    "3.14e1234",
    "3.14e-1234",
    "0.738957395793475734757349579759957975985497e100",
    "0.73895739579347546656564656573475734957975995797598589749859834759476745986795497e100",
    "inf",
    "Inf"
];

public static void TestFloatGobEncoding(ж<testing.T> t) {
    ref var medium = ref heap(new bytes.Buffer(), out var Ꮡmedium);
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡmedium));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(Ꮡmedium));
    foreach (var (_, test) in range(floatVals)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            foreach (var (_, prec) in range(slice<nuint>([0, 1, 2, 10, 53, 64, 100, 1000]))) {
                foreach (var (_, mode) in range(slice<global::go.math.big_package.RoundingMode>([ToNearestEven, ToNearestAway, ToZero, AwayFromZero, ToNegativeInf, ToPositiveInf]))) {
                    medium.Reset(); // empty buffer for each test case (in case of failures)
                    @string x = sign + test;

                    ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                    var (_, _, err) = Ꮡtx.SetPrec(prec).SetMode(mode).Parse(x, 0);
                    if (err != nil) {
                        t.Errorf("parsing of %s (%dbits, %v) failed (invalid test case): %v", x, prec, mode, err);
                        continue;
                    }

                    // If tx was set to prec == 0, tx.Parse(x, 0) assumes precision 64. Correct it.
                    if (prec == 0) {
                        Ꮡtx.SetPrec(0);
                    }

                    var errΔ1 = enc.Encode(Ꮡtx); if (errΔ1 != nil) {
                        t.Errorf("encoding of %v (%dbits, %v) failed: %v", Ꮡtx, prec, mode, errΔ1);
                        continue;
                    }

                    ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                    var errΔ2 = dec.Decode(Ꮡrx); if (errΔ2 != nil) {
                        t.Errorf("decoding of %v (%dbits, %v) failed: %v", Ꮡtx, prec, mode, errΔ2);
                        continue;
                    }

                    if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                        t.Errorf("transmission of %s failed: got %s want %s", x, Ꮡrx.String(), Ꮡtx.String());
                        continue;
                    }

                    if (rx.Prec() != prec) {
                        t.Errorf("transmission of %s's prec failed: got %d want %d", x, rx.Prec(), prec);
                    }

                    if (rx.Mode() != mode) {
                        t.Errorf("transmission of %s's mode failed: got %s want %s", x, rx.Mode(), mode);
                    }

                    if (rx.Acc() != tx.Acc()) {
                        t.Errorf("transmission of %s's accuracy failed: got %s want %s", x, rx.Acc(), tx.Acc());
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilWantVersionErrorˢ = (@string)"got nil want version error"u8;

public static void TestFloatCorruptGob(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tx = NewFloat(4 / 3).SetPrec(1000).SetMode(ToPositiveInf);
    {
        var err = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡbuf)).Encode(tx.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    var b = buf.Bytes();

    ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b))).Decode(Ꮡrx); if (err != nil) {
            t.Fatal(err);
        }
    }

    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b[..10]))).Decode(Ꮡrx); if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Errorf("got %v want EOF", err);
        }
    }

    b[1] = 0;
    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b))).Decode(Ꮡrx); if (err == nil) {
            t.Fatal(gotNilWantVersionErrorˢ);
        }
    }
}

public static void TestFloatJSONEncoding(ж<testing.T> t) {
    foreach (var (_, test) in range(floatVals)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            foreach (var (_, prec) in range(slice<nuint>([0, 1, 2, 10, 53, 64, 100, 1000]))) {
                if (prec > 53 && testing.Short()) {
                    continue;
                }
                @string x = sign + test;
                ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                var (_, _, err) = Ꮡtx.SetPrec(prec).Parse(x, 0);
                if (err != nil) {
                    t.Errorf("parsing of %s (prec = %d) failed (invalid test case): %v", x, prec, err);
                    continue;
                }
                (var b, err) = json.Marshal(Ꮡtx);
                if (err != nil) {
                    t.Errorf("marshaling of %v (prec = %d) failed: %v", Ꮡtx, prec, err);
                    continue;
                }
                ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                Ꮡrx.SetPrec(prec);
                var errΔ1 = json.Unmarshal(b, Ꮡrx); if (errΔ1 != nil) {
                    t.Errorf("unmarshaling of %v (prec = %d) failed: %v", Ꮡtx, prec, errΔ1);
                    continue;
                }
                if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                    t.Errorf("JSON encoding of %v (prec = %d) failed: got %v want %v", Ꮡtx, prec, Ꮡrx, Ꮡtx);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedGobDecodeToˢ = (@string)"expected GobDecode to return error for malformed input"u8;

public static void TestFloatGobDecodeShortBuffer(ж<testing.T> t) {
    foreach (var (_, tc) in range(slice<slice<byte>>([
        [0x1, 0x0, 0x0, 0x0],
        [0x1, 0xfa, 0x0, 0x0, 0x0, 0x0]
    ]))) {
        var err = NewFloat(0D).GobDecode(tc);
        if (err == nil) {
            t.Error(expectedGobDecodeToˢ);
        }
    }
}

internal partial struct TestFloatGobDecodeInvalid_type /*dyn*/ {
    internal slice<byte> buf;
    internal @string msg;
}

public static void TestFloatGobDecodeInvalid(ж<testing.T> t) {
    foreach (var (_, tc) in range(slice<TestFloatGobDecodeInvalid_type>([
        new(
            [0x1, 0x2a, 0x20, 0x20, 0x20, 0x20, 0x0, 0x20, 0x20, 0x20, 0x0, 0x20, 0x20, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0xc],
            "Float.GobDecode: msb not set in last word"
        ),
        new(
            [1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            "Float.GobDecode: nonzero finite number with empty mantissa"
        )
    ]))) {
        var err = NewFloat(0D).GobDecode(tc.buf);
        if (err == nil || !strings.HasPrefix(err.Error(), tc.msg)) {
            t.Errorf("expected GobDecode error prefix: %s, got: %v", tc.msg, err);
        }
    }
}

public static void TestFloatAppendText(ж<testing.T> t) {
    foreach (var (_, test) in range(floatVals)) {
        foreach (var (_, sign) in range(slice<@string>(["", "+", "-"]))) {
            foreach (var (_, prec) in range(slice<nuint>([0, 1, 2, 10, 53, 64, 100, 1000]))) {
                if (prec > 53 && testing.Short()) {
                    continue;
                }
                @string x = sign + test;
                ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                var (_, _, err) = Ꮡtx.SetPrec(prec).Parse(x, 0);
                if (err != nil) {
                    t.Errorf("parsing of %s (prec = %d) failed (invalid test case): %v", x, prec, err);
                    continue;
                }
                var buf = new slice<byte>(4, 32);
                (var b, err) = Ꮡtx.AppendText(buf);
                if (err != nil) {
                    t.Errorf("marshaling of %v (prec = %d) failed: %v", Ꮡtx, prec, err);
                    continue;
                }
                ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                Ꮡrx.SetPrec(prec);
                var errΔ1 = Ꮡrx.UnmarshalText(b[4..]); if (errΔ1 != nil) {
                    t.Errorf("unmarshaling of %v (prec = %d) failed: %v", Ꮡtx, prec, errΔ1);
                    continue;
                }
                if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                    t.Errorf("AppendText of %v (prec = %d) failed: got %v want %v", Ꮡtx, prec, Ꮡrx, Ꮡtx);
                }
            }
        }
    }
}

public static void TestFloatAppendTextNil(ж<testing.T> t) {
    ж<global::go.math.big_package.Float> x = default;
    var buf = new slice<byte>(4, 16);
    var (data, _) = x.AppendText(buf);
    if (((sstring)(data[4..])) != "<nil>"u8) {
        t.Errorf("got %q, want <nil>", data[4..]);
    }
}

} // end big_internal_test_package
