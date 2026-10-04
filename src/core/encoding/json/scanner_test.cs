// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
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
    return strings.Join(strings.Split(s, "\n"u8), "\n\t"u8);
}

internal static @string stripWhitespace(@string s) {
    return strings.Map((rune r) => {
        if (r == (rune)' ' || r == (rune)'\n' || r == (rune)'\r' || r == (rune)'\t') {
            return -1;
        }
        return r;
    }, s);
}

[GoType("dyn")] internal partial struct TestValid_tests {
    public partial ref CaseName CaseName { get; }
    internal @string data;
    internal bool ok;
}

public static void TestValid(ж<testing.T> Ꮡt) {
    var tests = new TestValid_tests[]{
        new(Name(""u8), @"foo"u8, false),
        new(Name(""u8), @"}{"u8, false),
        new(Name(""u8), @"{]"u8, false),
        new(Name(""u8), @"{}"u8, true),
        new(Name(""u8), @"{""foo"":""bar""}"u8, true),
        new(Name(""u8), @"{""foo"":""bar"",""bar"":{""baz"":[""qux""]}}"u8, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestValid_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            {
                var ok = Valid(slice<byte>(ttʗ1.data)); if (ok != ttʗ1.ok) {
                    tΔ1.Errorf("%s: Valid(`%s`) = %v, want %v"u8, ttʗ1.Where, ttʗ1.data, ok, ttʗ1.ok);
                }
            }
        });
    }
}

[GoType("dyn")] internal partial struct TestCompactAndIndent_tests {
    public partial ref CaseName CaseName { get; }
    internal @string compact;
    internal @string indent;
}

public static void TestCompactAndIndent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestCompactAndIndent_tests[]{
        new(Name(""u8), @"1"u8, @"1"u8),
        new(Name(""u8), @"{}"u8, @"{}"u8),
        new(Name(""u8), @"[]"u8, @"[]"u8),
        new(Name(""u8), @"{"""":2}"u8, "{\n\t\"\": 2\n}"u8),
        new(Name(""u8), @"[3]"u8, "[\n\t3\n]"u8),
        new(Name(""u8), @"[1,2,3]"u8, "[\n\t1,\n\t2,\n\t3\n]"u8),
        new(Name(""u8), @"{""x"":1}"u8, "{\n\t\"x\": 1\n}"u8),
        new(Name(""u8), @"[true,false,null,""x"",1,1.5,0,-5e+2]"u8, """
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
"""u8),
        new(Name(""u8), "{\"\":\"<>&\u2028\u2029\"}"u8, "{\n\t\"\": \"<>&\u2028\u2029\"\n}"u8)
    }.slice();
    // See golang.org/issue/34070
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCompactAndIndent_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            Ꮡbuf.Value.Reset();
            {
                var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.compact)); if (err != default!){
                    tΔ1.Errorf("%s: Compact error: %v"u8, ttʗ1.Where, err);
                } else 
                {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                    }
                }
            }
            Ꮡbuf.Value.Reset();
            {
                var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.indent)); if (err != default!){
                    tΔ1.Errorf("%s: Compact error: %v"u8, ttʗ1.Where, err);
                } else 
                {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                    }
                }
            }
            Ꮡbuf.Value.Reset();
            {
                var err = Indent(Ꮡbuf, slice<byte>(ttʗ1.indent), ""u8, "\t"u8); if (err != default!){
                    tΔ1.Errorf("%s: Indent error: %v"u8, ttʗ1.Where, err);
                } else 
                {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.indent) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.indent));
                    }
                }
            }
            Ꮡbuf.Value.Reset();
            {
                var err = Indent(Ꮡbuf, slice<byte>(ttʗ1.compact), ""u8, "\t"u8); if (err != default!){
                    tΔ1.Errorf("%s: Indent error: %v"u8, ttʗ1.Where, err);
                } else 
                {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.indent) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.indent));
                    }
                }
            }
        });
    }
}

[GoType("dyn")] internal partial struct TestCompactSeparators_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in, compact;
}

public static void TestCompactSeparators(ж<testing.T> Ꮡt) {
    // U+2028 and U+2029 should be escaped inside strings.
    // They should not appear outside strings.
    var tests = new TestCompactSeparators_tests[]{
        new(Name(""u8), "{\"\u2028\": 1}"u8, "{\"\u2028\":1}"u8),
        new(Name(""u8), "{\"\u2029\" :2}"u8, "{\"\u2029\":2}"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCompactSeparators_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var err = Compact(Ꮡbuf, slice<byte>(ttʗ1.@in)); if (err != default!){
                    tΔ1.Errorf("%s: Compact error: %v"u8, ttʗ1.Where, err);
                } else 
                {
                    @string got = Ꮡbuf.String(); if (got != ttʗ1.compact) {
                        tΔ1.Errorf("%s: Compact:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, indentNewlines(got), indentNewlines(ttʗ1.compact));
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object compactˢ = (@string)"Compact:"u8;

// Tests of a large random structure.
public static void TestCompactBig(ж<testing.T> Ꮡt) {
    initBig();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Compact(Ꮡbuf, jsonBig); if (err != default!) {
            Ꮡt.Fatalf("Compact error: %v"u8, err);
        }
    }
    var b = buf.Bytes();
    if (!bytes.Equal(b, jsonBig)) {
        Ꮡt.Error(compactˢ);
        diff(Ꮡt, b, jsonBig);
        return;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object indentIndentJsonBigˢ = (@string)"Indent(Indent(jsonBig)) != Indent(jsonBig):"u8;
internal static readonly object compactIndentJsonBigˢ = (@string)"Compact(Indent(jsonBig)) != jsonBig:"u8;

public static void TestIndentBig(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    initBig();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Indent(Ꮡbuf, jsonBig, ""u8, "\t"u8); if (err != default!) {
            Ꮡt.Fatalf("Indent error: %v"u8, err);
        }
    }
    var b = buf.Bytes();
    if (len(b) == len(jsonBig)) {
        // jsonBig is compact (no unnecessary spaces);
        // indenting should make it bigger
        Ꮡt.Fatalf("Indent did not expand the input"u8);
    }
    // should be idempotent
    ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
    {
        var err = Indent(Ꮡbuf1, b, ""u8, "\t"u8); if (err != default!) {
            Ꮡt.Fatalf("Indent error: %v"u8, err);
        }
    }
    var b1 = buf1.Bytes();
    if (!bytes.Equal(b1, b)) {
        Ꮡt.Error(indentIndentJsonBigˢ);
        diff(Ꮡt, b1, b);
        return;
    }
    // should get back to original
    buf1.Reset();
    {
        var err = Compact(Ꮡbuf1, b); if (err != default!) {
            Ꮡt.Fatalf("Compact error: %v"u8, err);
        }
    }
    b1 = buf1.Bytes();
    if (!bytes.Equal(b1, jsonBig)) {
        Ꮡt.Error(compactIndentJsonBigˢ);
        diff(Ꮡt, b1, jsonBig);
        return;
    }
}

[GoType("dyn")] internal partial struct TestIndentErrors_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal error err;
}

public static void TestIndentErrors(ж<testing.T> Ꮡt) {
    var tests = new TestIndentErrors_tests[]{
        new(Name(""u8), @"{""X"": ""foo"", ""Y""}"u8, new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' after object key"u8, 17)))),
        new(Name(""u8), @"{""X"": ""foo"" ""Y"": ""bar""}"u8, new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\"' after object key:value pair"u8, 13))))
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestIndentErrors_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var Δslice = new slice<uint8>(0);
            var buf = bytes.NewBuffer(Δslice);
            {
                var err = Indent(buf, slice<uint8>(ttʗ1.@in), ""u8, ""u8); if (err != default!) {
                    if (!reflect.DeepEqual(err, ttʗ1.err)) {
                        tΔ1.Fatalf("%s: Indent error:\n\tgot:  %v\n\twant: %v"u8, ttʗ1.Where, err, ttʗ1.err);
                    }
                }
            }
        });
    }
}

internal static void diff(ж<testing.T> Ꮡt, slice<byte> a, slice<byte> b) {
    Ꮡt.Helper();
    for (nint i = 0; ᐧ ; i++) {
        if (i >= len(a) || i >= len(b) || a[i] != b[i]) {
            nint j = i - 10;
            if (j < 0) {
                j = 0;
            }
            Ꮡt.Errorf("diverge at %d: «%s» vs «%s»"u8, i, trim(a.slice(j)), trim(b.slice(j)));
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
    if (err != default!) {
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
    foreach (var (i, _) in c) {
        var f = math.Abs(rand.NormFloat64() * 64D + 32D);
        if (f > 1114111D) {
            f = 1114111D;
        }
        c[i] = (rune)f;
    }
    return ((@string)c);
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
    foreach (var (i, _) in x) {
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
