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

internal static slice<@string> floatVals = new @string[]{
    "0"u8,
    "1"u8,
    "0.1"u8,
    "2.71828"u8,
    "1234567890"u8,
    "3.14e1234"u8,
    "3.14e-1234"u8,
    "0.738957395793475734757349579759957975985497e100"u8,
    "0.73895739579347546656564656573475734957975995797598589749859834759476745986795497e100"u8,
    "inf"u8,
    "Inf"u8
}.slice();

public static void TestFloatGobEncoding(ж<testing.T> Ꮡt) {
    ref var medium = ref heap(new bytes.Buffer(), out var Ꮡmedium);
    var enc = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡmedium));
    var dec = gob.NewDecoder(new big_test_package.bytes_BufferжReader(Ꮡmedium));
    foreach (var (_, test) in floatVals) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            foreach (var (_, prec) in new nuint[]{0, 1, 2, 10, 53, 64, 100, 1000}.slice()) {
                foreach (var (_, mode) in new global::go.math.big_package.RoundingMode[]{ToNearestEven, ToNearestAway, ToZero, AwayFromZero, ToNegativeInf, ToPositiveInf}.slice()) {
                    medium.Reset(); // empty buffer for each test case (in case of failures)
                    @string x = sign + test;
                    ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                    var (_, _, err) = Ꮡtx.SetPrec(prec).SetMode(mode).Parse(x, 0);
                    if (err != default!) {
                        Ꮡt.Errorf("parsing of %s (%dbits, %v) failed (invalid test case): %v"u8, x, prec, mode, err);
                        continue;
                    }
                    // If tx was set to prec == 0, tx.Parse(x, 0) assumes precision 64. Correct it.
                    if (prec == 0) {
                        Ꮡtx.SetPrec(0);
                    }
                    {
                        var errΔ1 = enc.Encode(Ꮡtx); if (errΔ1 != default!) {
                            Ꮡt.Errorf("encoding of %v (%dbits, %v) failed: %v"u8, Ꮡtx, prec, mode, errΔ1);
                            continue;
                        }
                    }
                    ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                    {
                        var errΔ2 = dec.Decode(Ꮡrx); if (errΔ2 != default!) {
                            Ꮡt.Errorf("decoding of %v (%dbits, %v) failed: %v"u8, Ꮡtx, prec, mode, errΔ2);
                            continue;
                        }
                    }
                    if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                        Ꮡt.Errorf("transmission of %s failed: got %s want %s"u8, x, Ꮡrx.String(), Ꮡtx.String());
                        continue;
                    }
                    if (rx.Prec() != prec) {
                        Ꮡt.Errorf("transmission of %s's prec failed: got %d want %d"u8, x, rx.Prec(), prec);
                    }
                    if (rx.Mode() != mode) {
                        Ꮡt.Errorf("transmission of %s's mode failed: got %s want %s"u8, x, rx.Mode(), mode);
                    }
                    if (rx.Acc() != tx.Acc()) {
                        Ꮡt.Errorf("transmission of %s's accuracy failed: got %s want %s"u8, x, rx.Acc(), tx.Acc());
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilWantVersionErrorˢ = (@string)"got nil want version error"u8;

public static void TestFloatCorruptGob(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tx = NewFloat(4 / 3).SetPrec(1000).SetMode(ToPositiveInf);
    {
        var err = gob.NewEncoder(new big_test_package.bytes_BufferжWriter(Ꮡbuf)).Encode(tx.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var b = buf.Bytes();
    ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b))).Decode(Ꮡrx); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b[..10]))).Decode(Ꮡrx); if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            Ꮡt.Errorf("got %v want EOF"u8, err);
        }
    }
    b[1] = 0;
    {
        var err = gob.NewDecoder(new big_test_package.bytes_ReaderжReader(bytes_package.NewReader(b))).Decode(Ꮡrx); if (err == default!) {
            Ꮡt.Fatal(gotNilWantVersionErrorˢ);
        }
    }
}

public static void TestFloatJSONEncoding(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in floatVals) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            foreach (var (_, prec) in new nuint[]{0, 1, 2, 10, 53, 64, 100, 1000}.slice()) {
                if (prec > 53 && testing.Short()) {
                    continue;
                }
                @string x = sign + test;
                ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                var (_, _, err) = Ꮡtx.SetPrec(prec).Parse(x, 0);
                if (err != default!) {
                    Ꮡt.Errorf("parsing of %s (prec = %d) failed (invalid test case): %v"u8, x, prec, err);
                    continue;
                }
                (var b, err) = json.Marshal(Ꮡtx);
                if (err != default!) {
                    Ꮡt.Errorf("marshaling of %v (prec = %d) failed: %v"u8, Ꮡtx, prec, err);
                    continue;
                }
                ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                Ꮡrx.SetPrec(prec);
                {
                    var errΔ1 = json.Unmarshal(b, Ꮡrx); if (errΔ1 != default!) {
                        Ꮡt.Errorf("unmarshaling of %v (prec = %d) failed: %v"u8, Ꮡtx, prec, errΔ1);
                        continue;
                    }
                }
                if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                    Ꮡt.Errorf("JSON encoding of %v (prec = %d) failed: got %v want %v"u8, Ꮡtx, prec, Ꮡrx, Ꮡtx);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedGobDecodeToˢ = (@string)"expected GobDecode to return error for malformed input"u8;

public static void TestFloatGobDecodeShortBuffer(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in new slice<byte>[]{
        new byte[]{0x1, 0x0, 0x0, 0x0}.slice(),
        new byte[]{0x1, 0xfa, 0x0, 0x0, 0x0, 0x0}.slice()
    }.slice()) {
        var err = NewFloat(0D).GobDecode(tc);
        if (err == default!) {
            Ꮡt.Error(expectedGobDecodeToˢ);
        }
    }
}

internal partial struct TestFloatGobDecodeInvalid_type /*dyn*/ {
    internal slice<byte> buf;
    internal @string msg;
}

public static void TestFloatGobDecodeInvalid(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in new TestFloatGobDecodeInvalid_type[]{
        new(
            new byte[]{0x1, 0x2a, 0x20, 0x20, 0x20, 0x20, 0x0, 0x20, 0x20, 0x20, 0x0, 0x20, 0x20, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0xc}.slice(),
            "Float.GobDecode: msb not set in last word"u8
        ),
        new(
            new byte[]{1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice(),
            "Float.GobDecode: nonzero finite number with empty mantissa"u8
        )
    }.slice()) {
        var err = NewFloat(0D).GobDecode(tc.buf);
        if (err == default! || !strings.HasPrefix(err.Error(), tc.msg)) {
            Ꮡt.Errorf("expected GobDecode error prefix: %s, got: %v"u8, tc.msg, err);
        }
    }
}

public static void TestFloatAppendText(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in floatVals) {
        foreach (var (_, sign) in new @string[]{""u8, "+"u8, "-"u8}.slice()) {
            foreach (var (_, prec) in new nuint[]{0, 1, 2, 10, 53, 64, 100, 1000}.slice()) {
                if (prec > 53 && testing.Short()) {
                    continue;
                }
                @string x = sign + test;
                ref var tx = ref heap(new global::go.math.big_package.Float(), out var Ꮡtx);
                var (_, _, err) = Ꮡtx.SetPrec(prec).Parse(x, 0);
                if (err != default!) {
                    Ꮡt.Errorf("parsing of %s (prec = %d) failed (invalid test case): %v"u8, x, prec, err);
                    continue;
                }
                var buf = new slice<byte>(4, 32);
                (var b, err) = Ꮡtx.AppendText(buf);
                if (err != default!) {
                    Ꮡt.Errorf("marshaling of %v (prec = %d) failed: %v"u8, Ꮡtx, prec, err);
                    continue;
                }
                ref var rx = ref heap(new global::go.math.big_package.Float(), out var Ꮡrx);
                Ꮡrx.SetPrec(prec);
                {
                    var errΔ1 = Ꮡrx.UnmarshalText(b[4..]); if (errΔ1 != default!) {
                        Ꮡt.Errorf("unmarshaling of %v (prec = %d) failed: %v"u8, Ꮡtx, prec, errΔ1);
                        continue;
                    }
                }
                if (Ꮡrx.Cmp(Ꮡtx) != 0) {
                    Ꮡt.Errorf("AppendText of %v (prec = %d) failed: got %v want %v"u8, Ꮡtx, prec, Ꮡrx, Ꮡtx);
                }
            }
        }
    }
}

public static void TestFloatAppendTextNil(ж<testing.T> Ꮡt) {
    ж<global::go.math.big_package.Float> x = default!;
    var buf = new slice<byte>(4, 16);
    var (data, _) = x.AppendText(buf);
    if (((sstring)(data[4..])) != "<nil>"u8) {
        Ꮡt.Errorf("got %q, want <nil>"u8, data[4..]);
    }
}

} // end big_internal_test_package
