// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using math = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using go.math;
using static go.encoding.json_package;

partial class json_internal_test_package {

internal static @string indentNewlines(@string s) {
    return strings.Join(strings.Split(s, "\n"), "\n\t");
}

internal static @string stripWhitespace(@string s) {
    return strings.Map((rune r) => {
        if (r == ' ' || r == '\n' || r == '\r' || r == '\t') {
            return -1;
        }
        return r;
    }, s);
}

internal partial struct TestValid_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string data;
    internal bool ok;
}

public static void TestValid(ж<testing.T> t) {
    var tests = slice<TestValid_tests>([
        new(Name(""u8), @"foo", false),
        new(Name(""u8), @"}{", false),
        new(Name(""u8), @"{]", false),
        new(Name(""u8), @"{}", true),
        new(Name(""u8), @"{""foo"":""bar""}", true),
        new(Name(""u8), @"{""foo"":""bar"",""bar"":{""baz"":[""qux""]}}", true)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestValid_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var ok = Valid(slice<byte>(ttʗ1.data)); if (ok != ttʗ1.ok) {
                tΔ1.Errorf("%s: Valid(`%s`) = %v, want %v", ttʗ1.Where, ttʗ1.data, ok, ttʗ1.ok);
            }
        });
    }
}

internal partial struct TestCompactAndIndent_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string compact;
    internal @string indent;
}

public static void TestCompactAndIndent(ж<testing.T> t) {
    var tests = slice<TestCompactAndIndent_tests>([
        new(Name(""u8), @"1", @"1"),
        new(Name(""u8), @"{}", @"{}"),
        new(Name(""u8), @"[]", @"[]"),
        new(Name(""u8), @"{"""":2}", "{\n\t\"\": 2\n}"),
        new(Name(""u8), @"[3]", "[\n\t3\n]"),
        new(Name(""u8), @"[1,2,3]", "[\n\t1,\n\t2,\n\t3\n]"),
        new(Name(""u8), @"{""x"":1}", "{\n\t\"x\": 1\n}"),
        new(Name(""u8), @"[true,false,null,""x"",1,1.5,0,-5e+2]", """
[
	true,
	false,
	null,
	"x",
	1,
	1.5,
	0,
	-5e+2
]
"""u8.Raw()),
        new(Name(""u8), "{\"\":\"<>&\u2028\u2029\"}", "{\n\t\"\": \"<>&\u2028\u2029\"\n}"), // See golang.org/issue/34070
        new(Name(""u8), @"null", "null \n\r\t")                                             // See golang.org/issue/13520 and golang.org/issue/74806
    ]);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestCompactAndIndent_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            Ꮡbuf.Value.Reset();
            {
                var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.compact)); if (err != nil) {
                    tΔ1.Errorf("%s: Compact error: %v", ttʗ1.Where, err);
                } else {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                    }
                }
            }

            Ꮡbuf.Value.Reset();
            {
                var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.indent)); if (err != nil) {
                    tΔ1.Errorf("%s: Compact error: %v", ttʗ1.Where, err);
                } else {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                    }
                }
            }

            Ꮡbuf.Value.Reset();
            {
                var err = Indent(Ꮡbuf, slice<byte>(ttʗ1.indent), ""u8, "\t"); if (err != nil) {
                    tΔ1.Errorf("%s: Indent error: %v", ttʗ1.Where, err);
                } else {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.indent) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.indent));
                    }
                }
            }

            Ꮡbuf.Value.Reset();
            {
                var err = Indent(Ꮡbuf, slice<byte>(ttʗ1.compact), ""u8, "\t"); if (err != nil) {
                    tΔ1.Errorf("%s: Indent error: %v", ttʗ1.Where, err);
                } else {
                    @string got = Ꮡbuf.String(); if (got != strings.TrimRight(ttʗ1.indent, " \n\r\t")) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.indent));
                    }
                }
            }
        });
    }
}

internal partial struct TestCompactSeparators_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in, compact;
}

public static void TestCompactSeparators(ж<testing.T> t) {
    // U+2028 and U+2029 should be escaped inside strings.
    // They should not appear outside strings.
    var tests = slice<TestCompactSeparators_tests>([
        new(Name(""u8), "{\"\u2028\": 1}", "{\"\u2028\":1}"),
        new(Name(""u8), "{\"\u2029\" :2}", "{\"\u2029\":2}")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestCompactSeparators_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.@in)); if (err != nil) {
                tΔ1.Errorf("%s: Compact error: %v", ttʗ1.Where, err);
            } else {
                @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                    tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object compactˢ = (@string)"Compact:"u8;

// Tests of a large random structure.
public static void TestCompactBig(ж<testing.T> t) {
    initBig();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var err = Compact(Ꮡbuf, jsonBig); if (err != nil) {
        t.Fatalf("Compact error: %v", err);
    }
    var b = buf.Bytes();
    if (!bytes.Equal(b, jsonBig)) {
        t.Error(compactˢ);
        diff(t, b, jsonBig);
        return;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object indentIndentJsonBigˢ = (@string)"Indent(Indent(jsonBig)) != Indent(jsonBig):"u8;
internal static readonly object compactIndentJsonBigˢ = (@string)"Compact(Indent(jsonBig)) != jsonBig:"u8;

public static void TestIndentBig(ж<testing.T> t) {
    t.Parallel();
    initBig();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Indent(Ꮡbuf, jsonBig, ""u8, "\t"); if (err != nil) {
            t.Fatalf("Indent error: %v", err);
        }
    }
    var b = buf.Bytes();
    if (len(b) == len(jsonBig)) {
        // jsonBig is compact (no unnecessary spaces);
        // indenting should make it bigger
        t.Fatalf("Indent did not expand the input");
    }

    // should be idempotent
    ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
    {
        var err = Indent(Ꮡbuf1, b, ""u8, "\t"); if (err != nil) {
            t.Fatalf("Indent error: %v", err);
        }
    }
    var b1 = buf1.Bytes();
    if (!bytes.Equal(b1, b)) {
        t.Error(indentIndentJsonBigˢ);
        diff(t, b1, b);
        return;
    }

    // should get back to original
    buf1.Reset();
    {
        var err = Compact(Ꮡbuf1, b); if (err != nil) {
            t.Fatalf("Compact error: %v", err);
        }
    }
    b1 = buf1.Bytes();
    if (!bytes.Equal(b1, jsonBig)) {
        t.Error(compactIndentJsonBigˢ);
        diff(t, b1, jsonBig);
        return;
    }
}

internal partial struct TestIndentErrors_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal error err;
}

public static void TestIndentErrors(ж<testing.T> t) {
    var tests = slice<TestIndentErrors_tests>([
        new(Name(""u8), @"{""X"": ""foo"", ""Y""}", new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' after object key", 17)))),
        new(Name(""u8), @"{""X"": ""foo"" ""Y"": ""bar""}", new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\"' after object key:value pair", 13))))
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestIndentErrors_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var Δslice = new slice<uint8>(0);
            var buf = bytes.NewBuffer(Δslice);
            var err = Indent(buf, slice<uint8>(ttʗ1.@in), ""u8, ""u8); if (err != nil) {
                if (!reflect.DeepEqual(err, ttʗ1.err)) {
                    tΔ1.Fatalf("%s: Indent error:\n\tgot:  %v\n\twant: %v", ttʗ1.Where, err, ttʗ1.err);
                }
            }
        });
    }
}

internal static void diff(ж<testing.T> t, slice<byte> a, slice<byte> b) {
    t.Helper();
    for (nint i = 0; ; i++) {
        if (i >= len(a) || i >= len(b) || a[i] != b[i]) {
            nint j = i - 10;
            if (j < 0) {
                j = 0;
            }
            t.Errorf("diverge at %d: «%s» vs «%s»", i, trim(a.slice(j)), trim(b.slice(j)));
            return;
        }
    }
}

internal static slice<byte> trim(slice<byte> b) {
    return b.slice(0, min(len(b), 20));
}

// Generate a random JSON object.
internal static slice<byte> jsonBig;

internal static void initBig() {
    nint n = 10000;
    if (testing.Short()) {
        n = 100;
    }
    var (b, err) = Marshal(genValue(n));
    if (err != nil) {
        throw panic(err);
    }
    jsonBig = b;
}

internal static any genValue(nint n) {
    if (n > 1) {
        switch (rand.Intn(2)) {
        case 0: {
            return genArray(n);
        }
        case 1: {
            return genMap(n);
        }}
    }
    switch (rand.Intn(3)) {
    case 0: {
        return rand.Intn(2) == 0;
    }
    case 1: {
        return rand.NormFloat64();
    }
    case 2: {
        return genString(30D);
    }}
    throw panic("unreachable");
}

internal static @string genString(float64 stddev) {
    nint n = (nint)math.Abs(rand.NormFloat64() * stddev + stddev / 2D);
    var c = new slice<rune>(n);
    foreach (var i in range(len(c))) {
        var f = math.Abs(rand.NormFloat64() * 64D + 32D);
        if (f > 1114111D) {
            f = 1114111D;
        }
        c[i] = (rune)f;
    }
    return (@string)c;
}

internal static slice<any> genArray(nint n) {
    nint f = (nint)(math.Abs(rand.NormFloat64()) * math.Min(10D, (float64)(n / 2)));
    if (f > n) {
        f = n;
    }
    if (f < 1) {
        f = 1;
    }
    var x = new slice<any>(f);
    foreach (var i in range(len(x))) {
        x[i] = genValue(quo(((i + 1) * n), f) - quo((i * n), f));
    }
    return x;
}

internal static map<@string, any> genMap(nint n) {
    nint f = (nint)(math.Abs(rand.NormFloat64()) * math.Min(10D, (float64)(n / 2)));
    if (f > n) {
        f = n;
    }
    if (n > 0 && f == 0) {
        f = 1;
    }
    var x = new map<@string, any>();
    for (nint i = 0; i < f; i++) {
        x[genString(10D)] = genValue(quo(((i + 1) * n), f) - quo((i * n), f));
    }
    return x;
}

} // end json_internal_test_package
