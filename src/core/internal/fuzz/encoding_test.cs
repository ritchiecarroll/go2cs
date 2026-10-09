// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using math = math_package;
using strconv = strconv_package;
using testing = testing_package;
using unicode = unicode_package;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goTestFuzzV1Int1Uintˢ = """
go test fuzz v1
int(-1)
uint(4294967295)
"""u8;
internal static readonly @string goTestFuzzV1Intˢ = """
go test fuzz v1
int(9223372036854775807)
uint(18446744073709551615)
"""u8;
internal static readonly object didnTWriteFinalNewlineToˢ = (@string)"didn't write final newline to corpus file"u8;

internal partial struct TestUnmarshalMarshal_type /*dyn*/ {
    internal @string desc;
    internal @string @in;
    internal bool reject;
    internal @string want; // if different from in
}

public static void TestUnmarshalMarshal(ж<testing.T> Ꮡt) {
// The two IEEE 754 bit patterns used for the math.Float{64,32}frombits
// encodings are non-math.NAN quiet-NaN values. Since they are not equal
// to math.NaN(), they should be re-encoded to their bit patterns. They
// are, respectively:
//   * math.Float64bits(math.NaN())+1
//   * math.Float32bits(float32(math.NaN()))+1
// Although we arbitrarily choose default integer bases (0 or 16), we may
// want to change those arbitrary choices in the future and should not
// break the parser. Verify that integers in the opposite bases still
// parse correctly.
    slice<TestUnmarshalMarshal_type> tests = new TestUnmarshalMarshal_type[]{
        new(
            desc: "missing version"u8,
            @in: "int(1234)"u8,
            reject: true
        ),
        new(
            desc: "malformed string"u8,
            @in: """
go test fuzz v1
string("a"bcad")
"""u8,
            reject: true
        ),
        new(
            desc: "empty value"u8,
            @in: """
go test fuzz v1
int()
"""u8,
            reject: true
        ),
        new(
            desc: "negative uint"u8,
            @in: """
go test fuzz v1
uint(-32)
"""u8,
            reject: true
        ),
        new(
            desc: "int8 too large"u8,
            @in: """
go test fuzz v1
int8(1234456)
"""u8,
            reject: true
        ),
        new(
            desc: "multiplication in int value"u8,
            @in: """
go test fuzz v1
int(20*5)
"""u8,
            reject: true
        ),
        new(
            desc: "double negation"u8,
            @in: """
go test fuzz v1
int(--5)
"""u8,
            reject: true
        ),
        new(
            desc: "malformed bool"u8,
            @in: """
go test fuzz v1
bool(0)
"""u8,
            reject: true
        ),
        new(
            desc: "malformed byte"u8,
            @in: """
go test fuzz v1
byte('aa)
"""u8,
            reject: true
        ),
        new(
            desc: "byte out of range"u8,
            @in: """
go test fuzz v1
byte('☃')
"""u8,
            reject: true
        ),
        new(
            desc: "extra newline"u8,
            @in: """
go test fuzz v1
string("has extra newline")

"""u8,
            want: """
go test fuzz v1
string("has extra newline")
"""u8
        ),
        new(
            desc: "trailing spaces"u8,
            @in: """
go test fuzz v1
string("extra")
[]byte("spacing")  
    
"""u8,
            want: """
go test fuzz v1
string("extra")
[]byte("spacing")
"""u8
        ),
        new(
            desc: "float types"u8,
            @in: """
go test fuzz v1
float64(0)
float32(0)
"""u8
        ),
        new(
            desc: "various types"u8,
            @in: """
go test fuzz v1
int(-23)
int8(-2)
int64(2342425)
uint(1)
uint16(234)
uint32(352342)
uint64(123)
rune('œ')
byte('K')
byte('ÿ')
[]byte("hello¿")
[]byte("a")
bool(true)
string("hello\\xbd\\xb2=\\xbc ⌘")
float64(-12.5)
float32(2.5)
"""u8
        ),
        new(
            desc: "float edge cases"u8,
            @in: """
go test fuzz v1
float32(-0)
float64(-0)
float32(+Inf)
float32(-Inf)
float32(NaN)
float64(+Inf)
float64(-Inf)
float64(NaN)
math.Float64frombits(0x7ff8000000000002)
math.Float32frombits(0x7fc00001)
"""u8
        ),
        new(
            desc: "int variations"u8,
            @in: """
go test fuzz v1
int(0x0)
int32(0x41)
int64(0xfffffffff)
uint32(0xcafef00d)
uint64(0xffffffffffffffff)
uint8(0b0000000)
byte(0x0)
byte('\000')
byte('\u0000')
byte('\'')
math.Float64frombits(9221120237041090562)
math.Float32frombits(2143289345)
"""u8,
            want: """
go test fuzz v1
int(0)
rune('A')
int64(68719476735)
uint32(3405705229)
uint64(18446744073709551615)
byte('\x00')
byte('\x00')
byte('\x00')
byte('\x00')
byte('\'')
math.Float64frombits(0x7ff8000000000002)
math.Float32frombits(0x7fc00001)
"""u8
        ),
        new(
            desc: "rune validation"u8,
            @in: """
go test fuzz v1
rune(0)
rune(0x41)
rune(-1)
rune(0xfffd)
rune(0xd800)
rune(0x10ffff)
rune(0x110000)

"""u8,
            want: """
go test fuzz v1
rune('\x00')
rune('A')
int32(-1)
rune('�')
int32(55296)
rune('\U0010ffff')
int32(1114112)
"""u8
        ),
        new(
            desc: "int overflow"u8,
            @in: """
go test fuzz v1
int(0x7fffffffffffffff)
uint(0xffffffffffffffff)
"""u8,
            want: ((Func<@string>)(() => {
                var exprᴛ1 = strconv.IntSize;
                if (exprᴛ1 == 32) {
                    return goTestFuzzV1Int1Uintˢ;
                }
                if (exprᴛ1 == 64) {
                    return goTestFuzzV1Intˢ;
                }
                { /* default: */
                    throw panic("unreachable");
                }

            }))()
        ),
        new(
            desc: "windows new line"u8,
            @in: "go test fuzz v1\r\nint(0)\r\n"u8,
            want: "go test fuzz v1\nint(0)"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestUnmarshalMarshal_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.desc, (ж<testing.T> tΔ1) => {
            var (vals, err) = unmarshalCorpusFile(slice<byte>(testʗ1.@in));
            if (testʗ1.reject) {
                if (err == default!) {
                    tΔ1.Fatalf("unmarshal unexpected success"u8);
                }
                return;
            }
            if (err != default!) {
                tΔ1.Fatalf("unmarshal unexpected error: %v"u8, err);
            }
            var newB = marshalCorpusFile(vals.ꓸꓸꓸ);
            if (newB[len(newB) - 1] != (rune)'\n') {
                tΔ1.Error(didnTWriteFinalNewlineToˢ);
            }
            @string want = testʗ1.want;
            if (want == ""u8) {
                want = testʗ1.@in;
            }
            want += "\n"u8;
            @string got = ((@string)newB);
            if (got != want) {
                tΔ1.Errorf("unexpected marshaled value\ngot:\n%s\nwant:\n%s"u8, got, want);
            }
        });
    }
}

// BenchmarkMarshalCorpusFile measures the time it takes to serialize byte
// slices of various sizes to a corpus file. The slice contains a repeating
// sequence of bytes 0-255 to mix escaped and non-escaped characters.
public static void BenchmarkMarshalCorpusFile(ж<testing.B> Ꮡb) {
    var buf = new slice<byte>(1024 * 1024);
    for (nint i = 0; i < len(buf); i++) {
        buf[i] = (byte)i;
    }
    for (nint sz = 1; sz <= len(buf); sz <<= (int)(1)) {
        nint szΔ1 = sz;
        var bufʗ1 = buf;
        Ꮡb.Run(strconv.Itoa(szΔ1), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bΔ1.SetBytes((int64)szΔ1);
                marshalCorpusFile(bufʗ1.slice(0, szΔ1));
            }
        });
    }
}

// BenchmarkUnmarshalCorpusfile measures the time it takes to deserialize
// files encoding byte slices of various sizes. The slice contains a repeating
// sequence of bytes 0-255 to mix escaped and non-escaped characters.
public static void BenchmarkUnmarshalCorpusFile(ж<testing.B> Ꮡb) {
    var buf = new slice<byte>(1024 * 1024);
    for (nint i = 0; i < len(buf); i++) {
        buf[i] = (byte)i;
    }
    for (nint sz = 1; sz <= len(buf); sz <<= (int)(1)) {
        nint szΔ1 = sz;
        var data = marshalCorpusFile(buf.slice(0, szΔ1));
        var dataʗ1 = data;
        Ꮡb.Run(strconv.Itoa(szΔ1), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bΔ1.SetBytes((int64)szΔ1);
                unmarshalCorpusFile(dataʗ1);
            }
        });
    }
}

public static void TestByteRoundTrip(ж<testing.T> Ꮡt) {
    for (nint x = 0; x < 256; x++) {
        var b1 = (byte)x;
        var buf = marshalCorpusFile(b1);
        var (vs, err) = unmarshalCorpusFile(buf);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var b2 = vs[0]._<byte>();
        if (b2 != b1) {
            Ꮡt.Fatalf("unmarshaled %v, want %v:\n%s"u8, b2, b1, buf);
        }
    }
}

public static void TestInt8RoundTrip(ж<testing.T> Ꮡt) {
    for (nint x = -128; x < 128; x++) {
        var i1 = (int8)x;
        var buf = marshalCorpusFile(i1);
        var (vs, err) = unmarshalCorpusFile(buf);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var i2 = vs[0]._<int8>();
        if (i2 != i1) {
            Ꮡt.Fatalf("unmarshaled %v, want %v:\n%s"u8, i2, i1, buf);
        }
    }
}

public static void FuzzFloat64RoundTrip(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add(math.Float64bits(0D));
    f.Add(math.Float64bits(math.Copysign(0D, -1D)));
    f.Add(math.Float64bits(math.MaxFloat64));
    f.Add(math.Float64bits(math.SmallestNonzeroFloat64));
    f.Add(math.Float64bits(math.NaN()));
    f.Add((uint64)0x7FF0000000000001UL); // signaling NaN
    f.Add(math.Float64bits(math.Inf(1)));
    f.Add(math.Float64bits(math.Inf(-1)));
    Ꮡf.Fuzz((ж<testing.T> t, uint64 u1) => {
        var x1 = math.Float64frombits(u1);
        var b = marshalCorpusFile(x1);
        t.Logf("marshaled math.Float64frombits(0x%x):\n%s"u8, u1, b);
        var (xs, err) = unmarshalCorpusFile(b);
        if (err != default!) {
            t.Fatal(err);
        }
        if (len(xs) != 1) {
            t.Fatalf("unmarshaled %d values"u8, len(xs));
        }
        var x2 = xs[0]._<float64>();
        var u2 = math.Float64bits(x2);
        if (u2 != u1) {
            t.Errorf("unmarshaled %v (bits 0x%x)"u8, x2, u2);
        }
    });
}

public static void FuzzRuneRoundTrip(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((rune)(-1));
    f.Add((rune)0xd800);
    f.Add((rune)0xdfff);
    f.Add((rune)unicode.ReplacementChar);
    f.Add((rune)unicode.MaxASCII);
    f.Add((rune)unicode.MaxLatin1);
    f.Add((rune)unicode.MaxRune);
    f.Add((rune)(unicode.MaxRune + 1));
    f.Add((rune)(-2147483648));
    f.Add((rune)0x7fffffff);
    Ꮡf.Fuzz((ж<testing.T> t, rune r1) => {
        var b = marshalCorpusFile(r1);
        t.Logf("marshaled rune(0x%x):\n%s"u8, r1, b);
        var (rs, err) = unmarshalCorpusFile(b);
        if (err != default!) {
            t.Fatal(err);
        }
        if (len(rs) != 1) {
            t.Fatalf("unmarshaled %d values"u8, len(rs));
        }
        var r2 = rs[0]._<rune>();
        if (r2 != r1) {
            t.Errorf("unmarshaled rune(0x%x)"u8, r2);
        }
    });
}

public static void FuzzStringRoundTrip(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((@string)""u8);
    f.Add((@string)"\x00"u8);
    f.Add(((@string)new rune[]{unicode.ReplacementChar}.slice()));
    Ꮡf.Fuzz((ж<testing.T> t, @string s1) => {
        var b = marshalCorpusFile(s1);
        t.Logf("marshaled %q:\n%s"u8, s1, b);
        var (rs, err) = unmarshalCorpusFile(b);
        if (err != default!) {
            t.Fatal(err);
        }
        if (len(rs) != 1) {
            t.Fatalf("unmarshaled %d values"u8, len(rs));
        }
        @string s2 = rs[0]._<@string>();
        if (s2 != s1) {
            t.Errorf("unmarshaled %q"u8, s2);
        }
    });
}

} // end fuzz_internal_test_package
