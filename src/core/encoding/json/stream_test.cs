// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using path = path_package;
using reflect = reflect_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.net;
using go.net.http;
using go.runtime;
using static go.encoding.json_package;

partial class json_internal_test_package {

// TODO(https://go.dev/issue/52751): Replace with native testing support.

// CaseName is a case name annotated with a file and line.
public partial struct CaseName {
    public @string Name;
    public CasePos Where;
}

// Name annotates a case name with the file and line of the caller.
public static partial CaseName /*c*/ Name(@string s) {
    CaseName c = new();

    c.Name = s;
    runtime.Callers(2, c.Where.pc[..]);
    return c.ΔClone();
}

// CasePos represents a file and line number.
public partial struct CasePos {
    internal array<uintptr> pc = new(1);
}

public static @string String(this CasePos pos) {
    pos = pos.ΔClone();

    var frames = runtime.CallersFrames(pos.pc[..]);
    var (frame, _) = frames.Next();
    return fmt.Sprintf("%s:%d"u8, path.Base(frame.File), frame.Line);
}

// Test values for the stream test.
// One of each JSON kind.
internal static slice<any> streamTest = [
    0.1D,
    (@string)"hello",
    default,
    true,
    false,
    slice<any>([(@string)"a", (@string)"b", (@string)"c"]),
    new map<@string, any>{["K"] = (@string)"Kelvin", ["ß"] = (@string)"long s"},
    3.14D // another value to make sure something can follow map
];

internal static @string streamEncoded = """
0.1
"hello"
null
true
false
["a","b","c"]
{"ß":"long s","K":"Kelvin"}
3.14

"""u8.Raw();

public static void TestEncoder(ж<testing.T> t) {
    for (nint i = 0; i <= len(streamTest); i++) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
        // Check that enc.SetIndent("", "") turns off indentation.
        enc.SetIndent(">", ".");
        enc.SetIndent(""u8, ""u8);
        foreach (var (j, v) in range(streamTest.slice(0, i))) {
            var err = enc.Encode(v); if (err != nil) {
                t.Fatalf("#%d.%d Encode error: %v", i, j, err);
            }
        }
        @string got = buf.String();
        @string want = nlines(streamEncoded, i); if (got != want) {
            t.Errorf("encoding %d items: mismatch:", i);
            diff(t, slice<byte>(got), slice<byte>(want));
            break;
        }
    }
}

// Trigger an error in Marshal with cyclic data.
internal partial struct TestEncoderErrorAndReuseEncodeState_Dummy /*dyn*/ {
    public @string Name;
    public ж<TestEncoderErrorAndReuseEncodeState_Dummy> Next;
}

internal partial struct TestEncoderErrorAndReuseEncodeState_Data /*dyn*/ {
    public @string A;
    public nint I;
}

public static void TestEncoderErrorAndReuseEncodeState(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Disable the GC temporarily to prevent encodeState's in Pool being cleaned away during the test.
        nint percent = debug.SetGCPercent(-1);
        defer(debug.SetGCPercent, percent, ref ᒐ);
        ref var dummy = ref heap<TestEncoderErrorAndReuseEncodeState_Dummy>(new TestEncoderErrorAndReuseEncodeState_Dummy(Name: "Dummy"), out var Ꮡdummy);
        dummy.Next = Ꮡdummy;

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var enc = NewEncoder(new json_test_package.bytes_BufferжWriter(Ꮡbuf));
        {
            var err = enc.Encode(dummy); if (err == nil) {
                t.Errorf("Encode(dummy) error: got nil, want non-nil");
            }
        }
        var want = new TestEncoderErrorAndReuseEncodeState_Data(A: "a", I: 1);
        {
            var err = enc.Encode(want); if (err != nil) {
                t.Errorf("Marshal error: %v", err);
            }
        }

        ref var got = ref heap(new TestEncoderErrorAndReuseEncodeState_Data(), out var Ꮡgot);
        {
            var err = Unmarshal(buf.Bytes(), Ꮡgot); if (err != nil) {
                t.Errorf("Unmarshal error: %v", err);
            }
        }
        if (got != want) {
            t.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %v\n\twant: %v", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string streamEncodedIndent = """
0.1
"hello"
null
true
false
[
>."a",
>."b",
>."c"
>]
{
>."ß": "long s",
>."K": "Kelvin"
>}
3.14

"""u8.Raw();

public static void TestEncoderIndent(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
    enc.SetIndent(">", ".");
    foreach (var (_, v) in range(streamTest)) {
        enc.Encode(v);
    }
    @string got = buf.String();
    @string want = streamEncodedIndent; if (got != want) {
        t.Errorf("Encode mismatch:\ngot:\n%s\n\nwant:\n%s", got, want);
        diff(t, slice<byte>(got), slice<byte>(want));
    }
}

internal partial struct strMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this strMarshaler s) {
    return (slice<byte>((@string)s), default);
}

internal partial struct strPtrMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this ref strPtrMarshaler s) {
    return (slice<byte>((@string)s), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tagStructˢ => ref tagStructˢᵇ;
private static readonly @string tagStructˢᵇ = "tagStruct"u8;
internal static ref readonly @string strˢ => ref strˢᵇ;
private static readonly @string strˢᵇ = @"""<str>"""u8;
internal static ref readonly @string stringOptionˢ => ref stringOptionˢᵇ;
private static readonly @string stringOptionˢᵇ = "stringOption"u8;

internal partial struct TestEncoderSetEscapeHTML_tagStruct /*dyn*/ {
    public nint Valid; /*`json:"<>&#! "`*/
    public nint Invalid; /*`json:"\\"`*/
}

internal partial struct TestEncoderSetEscapeHTML_marshalerStruct /*dyn*/ {
    public strMarshaler NonPtr;
    public strPtrMarshaler Ptr;
}

internal partial struct TestEncoderSetEscapeHTML_stringOption /*dyn*/ {
    public @string Bar; /*`json:"bar,string"`*/
}

internal partial struct TestEncoderSetEscapeHTML_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any v;
    internal @string wantEscape;
    internal @string want;
}

public static void TestEncoderSetEscapeHTML(ж<testing.T> t) {
    C c = default;
    CText ct = default;
    TestEncoderSetEscapeHTML_tagStruct tagStruct = default;

    // This case is particularly interesting, as we force the encoder to
    // take the address of the Ptr field to use its MarshalJSON method. This
    // is why the '&' is important.
    var marshalerStruct = Ꮡ(new TestEncoderSetEscapeHTML_marshalerStruct((@string)@"""<str>""", (@string)@"""<str>"""));

    // https://golang.org/issue/34154
    var stringOption = new TestEncoderSetEscapeHTML_stringOption(@"<html>foobar</html>");

    var tests = slice<TestEncoderSetEscapeHTML_tests>([
        new(Name("c"), c, @"""\u003c\u0026\u003e""", @"""<&>"""),
        new(Name("ct"), ct, @"""\""\u003c\u0026\u003e\""""", @"""\""<&>\"""""),
        new(Name(@"""<&>"""), (@string)"<&>", @"""\u003c\u0026\u003e""", @"""<&>"""),
        new(
            Name(tagStructˢ), tagStruct,
            @"{""\u003c\u003e\u0026#! "":0,""Invalid"":0}",
            @"{""<>&#! "":0,""Invalid"":0}"
        ),
        new(
            Name(strˢ), marshalerStruct.OrTypedNil(),
            @"{""NonPtr"":""\u003cstr\u003e"",""Ptr"":""\u003cstr\u003e""}",
            @"{""NonPtr"":""<str>"",""Ptr"":""<str>""}"
        ),
        new(
            Name(stringOptionˢ), stringOption,
            @"{""bar"":""\""\\u003chtml\\u003efoobar\\u003c/html\\u003e\""""}",
            @"{""bar"":""\""<html>foobar</html>\""""}"
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestEncoderSetEscapeHTML_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
            {
                var err = enc.Encode(ttʗ1.v); if (err != nil) {
                    tΔ1.Fatalf("%s: Encode(%s) error: %s", ttʗ1.Where, ttʗ1.Name, err);
                }
            }
            {
                @string got = strings.TrimSpace(buf.String()); if (got != ttʗ1.wantEscape) {
                    tΔ1.Errorf("%s: Encode(%s):\n\tgot:  %s\n\twant: %s", ttʗ1.Where, ttʗ1.Name, got, ttʗ1.wantEscape);
                }
            }
            buf.Reset();
            enc.SetEscapeHTML(false);
            {
                var err = enc.Encode(ttʗ1.v); if (err != nil) {
                    tΔ1.Fatalf("%s: SetEscapeHTML(false) Encode(%s) error: %s", ttʗ1.Where, ttʗ1.Name, err);
                }
            }
            {
                @string got = strings.TrimSpace(buf.String()); if (got != ttʗ1.want) {
                    tΔ1.Errorf("%s: SetEscapeHTML(false) Encode(%s):\n\tgot:  %s\n\twant: %s",
                        ttʗ1.Where, ttʗ1.Name, got, ttʗ1.want);
                }
            }
        });
    }
}

public static void TestDecoder(ж<testing.T> t) {
    for (nint i = 0; i <= len(streamTest); i++) {
        // Use stream without newlines as input,
        // just to stress the decoder even more.
        // Our test input does not include back-to-back numbers.
        // Otherwise stripping the newlines would
        // merge two adjacent JSON values.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        foreach (var (_, c) in nlines(streamEncoded, i)) {
            if (c != '\n') {
                buf.WriteRune(c);
            }
        }
        var @out = new slice<any>(i);
        var dec = NewDecoder(new json_test_package.bytes_BufferжReader(Ꮡbuf));
        foreach (var j in range(len(@out))) {
            var err = dec.Decode(Ꮡ(@out, j)); if (err != nil) {
                t.Fatalf("decode #%d/%d error: %v", j, i, err);
            }
        }
        if (!reflect.DeepEqual(@out, streamTest.slice(0, i))) {
            t.Errorf("decoding %d items: mismatch:", i);
            foreach (var j in range(len(@out))) {
                if (!reflect.DeepEqual(@out[j], streamTest[j])) {
                    t.Errorf("#%d:\n\tgot:  %v\n\twant: %v", j, @out[j], streamTest[j]);
                }
            }
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameGopherExtraˢ => ref nameGopherExtraˢᵇ;
private static readonly @string nameGopherExtraˢᵇ = @"{""Name"": ""Gopher""} extra "u8;
internal static ref readonly @string extraˢ => ref extraˢᵇ;
private static readonly @string extraˢᵇ = " extra "u8;

internal partial struct TestDecoderBuffered_m /*dyn*/ {
    public @string Name;
}

public static void TestDecoderBuffered(ж<testing.T> t) {
    var r = strings.NewReader(nameGopherExtraˢ);
    ref var m = ref heap(new TestDecoderBuffered_m(), out var Ꮡm);
    var d = NewDecoder(new json_test_package.strings_ReaderжReader(r));
    var err = d.Decode(Ꮡm);
    if (err != nil) {
        t.Fatal(err);
    }
    if (m.Name != "Gopher"u8) {
        t.Errorf("Name = %s, want Gopher", m.Name);
    }
    (var rest, err) = io.ReadAll(d.Buffered());
    if (err != nil) {
        t.Fatal(err);
    }
    @string got = (@string)rest;
    @string want = extraˢ; if (got != want) {
        t.Errorf("Remaining = %s, want %s", got, want);
    }
}

internal static @string nlines(@string s, nint n) {
    if (n <= 0) {
        return ""u8;
    }
    foreach (var (i, c) in s) {
        if (c == '\n') {
            n--; if (n == 0) {
                return s.slice(0, i + 1);
            }
        }
    }
    return s;
}

internal partial struct TestRawMessage_data /*dyn*/ {
    public float64 X;
    public global::go.encoding.json_package.RawMessage Id;
    public float32 Y;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string rawᶜ => ref rawᶜᵇ;
private static readonly @string rawᶜᵇ = @"[""\u0056"",null]"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ8 => ref wantᶜ8ᵇ;
private static readonly @string wantᶜ8ᵇ = @"{""X"":0.1,""Id"":[""\u0056"",null],""Y"":0.2}"u8;

public static void TestRawMessage(ж<testing.T> t) {
    ref var data = ref heap(new TestRawMessage_data(), out var Ꮡdata);
    @string raw = rawᶜ;
    @string want = wantᶜ8;
    var err = Unmarshal(slice<byte>(want), Ꮡdata);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (((sstring)((slice<byte>)data.Id)) != raw) {
        t.Fatalf("Unmarshal:\n\tgot:  %s\n\twant: %s", (slice<byte>)data.Id, raw);
    }
    (var got, err) = Marshal(Ꮡdata);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)got) != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

internal partial struct TestNullRawMessage_data /*dyn*/ {
    public float64 X;
    public global::go.encoding.json_package.RawMessage Id;
    public ж<global::go.encoding.json_package.RawMessage> IdPtr;
    public float32 Y;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ9 => ref wantᶜ9ᵇ;
private static readonly @string wantᶜ9ᵇ = @"{""X"":0.1,""Id"":null,""IdPtr"":null,""Y"":0.2}"u8;

public static void TestNullRawMessage(ж<testing.T> t) {
    ref var data = ref heap(new TestNullRawMessage_data(), out var Ꮡdata);
    @string want = wantᶜ9;
    var err = Unmarshal(slice<byte>(want), Ꮡdata);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    @string wantΔ1 = nullˢ;
    @string gotΔ1 = (@string)(slice<byte>)data.Id; if (wantΔ1 != gotΔ1) {
        t.Fatalf("Unmarshal:\n\tgot:  %s\n\twant: %s", gotΔ1, wantΔ1);
    }
    if (data.IdPtr != nil) {
        t.Fatalf("pointer mismatch: got non-nil, want nil");
    }
    (var got, err) = Marshal(Ꮡdata);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)got) != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

internal partial struct TestBlocking_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
}

public static void TestBlocking(ж<testing.T> t) {
    var tests = slice<TestBlocking_tests>([
        new(Name(""u8), @"{""x"": 1}"),
        new(Name(""u8), @"[1, 2, 3]")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestBlocking_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var (r, w) = net.Pipe();
            var wʗ1 = w;
            goǃ(ᴛ1 => wʗ1.Write(ᴛ1), slice<byte>(ttʗ1.@in));
            ref var val = ref heap<any>(out var Ꮡval);

            // If Decode reads beyond what w.Write writes above,
            // it will block, and the test will deadlock.
            var err = NewDecoder(new json_test_package.net_ConnᴠReader(r)).Decode(Ꮡval); if (err != nil) {
                tΔ1.Errorf("%s: NewDecoder(%s).Decode error: %v", ttʗ1.Where, ttʗ1.@in, err);
            }
            r.Close();
            w.Close();
        });
    }
}

internal partial struct decodeThis {
    internal any v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object objˢ = (@string)"obj"u8;

internal partial struct TestDecodeInStream_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string json;
    internal slice<any> expTokens;
}

public static void TestDecodeInStream(ж<testing.T> t) {
    var tests = slice<TestDecodeInStream_tests>([
        // streaming token cases
        new(CaseName: Name(""u8), json: @"10", expTokens: [(float64)10D]),
        new(CaseName: Name(""u8), json: @" [10] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[', (float64)10D, (global::go.encoding.json_package.Delim)(rune)']']),
        new(CaseName: Name(""u8), json: @" [false,10,""b""] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[', false, (float64)10D, (@string)"b", (global::go.encoding.json_package.Delim)(rune)']']),
        new(CaseName: Name(""u8), json: @"{ ""a"": 1 }", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)1D, (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @"{""a"": 1, ""b"":""3""}", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)1D, (@string)"b", (@string)"3", (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @" [{""a"": 1},{""a"": 2}] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[',
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)1D, (global::go.encoding.json_package.Delim)(rune)'}',
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)2D, (global::go.encoding.json_package.Delim)(rune)'}',
            (global::go.encoding.json_package.Delim)(rune)']']),
        new(CaseName: Name(""u8), json: @"{""obj"": {""a"": 1}}", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', objˢ, (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)1D, (global::go.encoding.json_package.Delim)(rune)'}',
            (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @"{""obj"": [{""a"": 1}]}", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', objˢ, (global::go.encoding.json_package.Delim)(rune)'[',
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a", (float64)1D, (global::go.encoding.json_package.Delim)(rune)'}',
            (global::go.encoding.json_package.Delim)(rune)']', (global::go.encoding.json_package.Delim)(rune)'}']),

        // streaming tokens with intermittent Decode()
        new(CaseName: Name(""u8), json: @"{ ""a"": 1 }", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', (@string)"a",
            new decodeThis((float64)1D),
            (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @" [ { ""a"" : 1 } ] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[',
            new decodeThis(new map<@string, any>{["a"] = (float64)1D}),
            (global::go.encoding.json_package.Delim)(rune)']']),
        new(CaseName: Name(""u8), json: @" [{""a"": 1},{""a"": 2}] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[',
            new decodeThis(new map<@string, any>{["a"] = (float64)1D}),
            new decodeThis(new map<@string, any>{["a"] = (float64)2D}),
            (global::go.encoding.json_package.Delim)(rune)']']),
        new(CaseName: Name(""u8), json: @"{ ""obj"" : [ { ""a"" : 1 } ] }", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', objˢ, (global::go.encoding.json_package.Delim)(rune)'[',
            new decodeThis(new map<@string, any>{["a"] = (float64)1D}),
            (global::go.encoding.json_package.Delim)(rune)']', (global::go.encoding.json_package.Delim)(rune)'}']),

        new(CaseName: Name(""u8), json: @"{""obj"": {""a"": 1}}", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', objˢ,
            new decodeThis(new map<@string, any>{["a"] = (float64)1D}),
            (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @"{""obj"": [{""a"": 1}]}", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{', objˢ,
            new decodeThis(slice<any>([
                new map<@string, any>{["a"] = (float64)1D}
            ])
            ),
            (global::go.encoding.json_package.Delim)(rune)'}']),
        new(CaseName: Name(""u8), json: @" [{""a"": 1} {""a"": 2}] ", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'[',
            new decodeThis(new map<@string, any>{["a"] = (float64)1D}),
            new decodeThis(Ꮡ(new SyntaxError("expected comma after array element", 11)))
        ]),
        new(CaseName: Name(""u8), json: @"{ """u8 + strings.Repeat("a", 513) + @""" 1 }"u8, expTokens: slice<any>([
            (global::go.encoding.json_package.Delim)(rune)'{', strings.Repeat("a", 513),
            new decodeThis(Ꮡ(new SyntaxError("expected colon after object key", 518)))
        ])),
        new(CaseName: Name(""u8), json: @"{ ""\a"" }", expTokens: [
            (global::go.encoding.json_package.Delim)(rune)'{',
            Ꮡ(new SyntaxError("invalid character 'a' in string escape code", 3))
        ]),
        new(CaseName: Name(""u8), json: @" \a", expTokens: [
            Ꮡ(new SyntaxError("invalid character '\\\\' looking for beginning of value", 1))
        ]),
        new(CaseName: Name(""u8), json: @",", expTokens: [
            Ꮡ(new SyntaxError("invalid character ',' looking for beginning of value", 0))
        ])
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestDecodeInStream_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.json)));
            foreach (var (i, vᴛ2) in range(ttʗ1.expTokens)) {
                var want = vᴛ2;

                ref var got = ref heap<any>(out var Ꮡgot);
                error err = default;

                var wantMore = true;
                switch (want) {
                case var ᴛ when AreEqual(ᴛ, ((global::go.encoding.json_package.Delim)(rune)']')) || AreEqual(ᴛ, ((global::go.encoding.json_package.Delim)(rune)'}')): {
                    wantMore = false;
                    break;
                }}
                var gotΔ1 = dec.More(); if (gotΔ1 != wantMore) {
                    tΔ1.Fatalf("%s:\n\tinput: %s\n\tdec.More() = %v, want %v (next token: %T(%v))", ttʗ1.Where, ttʗ1.json, gotΔ1, wantMore, want, want);
                }

                if (want._<decodeThis>(out var dt)) {
                    want = dt.v;
                    err = dec.Decode(Ꮡgot);
                } else {
                    (got, err) = dec.Token();
                }
                if (want._<error>(out var errWant)) {
                    if (err == nil || !reflect.DeepEqual(err, errWant)) {
                        tΔ1.Fatalf("%s:\n\tinput: %s\n\tgot error:  %v\n\twant error: %v", ttʗ1.Where, ttʗ1.json, err, errWant);
                    }
                    break;
                } else if (err != nil) {
                    tΔ1.Fatalf("%s:\n\tinput: %s\n\tgot error:  %v\n\twant error: nil", ttʗ1.Where, ttʗ1.json, err);
                }
                if (!reflect.DeepEqual(got, want)) {
                    tΔ1.Fatalf("%s: token %d:\n\tinput: %s\n\tgot:  %T(%v)\n\twant: %T(%v)", ttʗ1.Where, i, ttʗ1.json, got, got, want, want);
                }
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string rawᶜ1 => ref rawᶜ1ᵇ;
private static readonly @string rawᶜ1ᵇ = @"{ ""foo"": ""bar"" }"u8;

internal partial struct TestHTTPDecoding_foo /*dyn*/ {
    public @string Foo;
}

// Test from golang.org/issue/11893
public static void TestHTTPDecoding(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string raw = rawᶜ1;

        var ts = httptest.NewServer(new json_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(raw));
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = http.Get(ts.URL);
        if (err != nil) {
            log.Fatalf("http.Get error: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        ref var foo = ref heap<TestHTTPDecoding_foo>(new TestHTTPDecoding_foo(), out var Ꮡfoo);

        var d = NewDecoder(new json_test_package.io_ReadCloserᴠReader(res.Body));
        err = d.Decode(Ꮡfoo);
        if (err != nil) {
            t.Fatalf("Decode error: %v", err);
        }
        if (foo.Foo != "bar"u8) {
            t.Errorf(@"Decode: got %q, want ""bar""", foo.Foo);
        }

        // make sure we get the EOF the second time
        err = d.Decode(Ꮡfoo);
        if (!AreEqual(err, io.EOF)) {
            t.Errorf("Decode error:\n\tgot:  %v\n\twant: io.EOF", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestTokenTruncation_tests /*dyn*/ {
    internal @string @in;
    internal error err;
}

public static void TestTokenTruncation(ж<testing.T> t) {
    var tests = slice<TestTokenTruncation_tests>([
        new(@in: @"", err: io.EOF),
        new(@in: @"{", err: io.EOF),
        new(@in: @"{""", err: io.ErrUnexpectedEOF),
        new(@in: @"{""k""", err: io.EOF),
        new(@in: @"{""k"":", err: io.EOF),
        new(@in: @"{""k"",", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ',' after object key", (int64)len(@"{""k"""))))),
        new(@in: @"{""k""}", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' after object key", (int64)len(@"{""k"""))))),
        new(@in: @" [0", err: io.EOF),
        new(@in: @"[0.", err: io.ErrUnexpectedEOF),
        new(@in: @"[0. ", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ' ' after decimal point in numeric literal", (int64)len(@"[0."))))),
        new(@in: @"[0,", err: io.EOF),
        new(@in: @"[0:", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ':' after array element", (int64)len(@"[0"))))),
        new(@in: @"n", err: io.ErrUnexpectedEOF),
        new(@in: @"nul", err: io.ErrUnexpectedEOF),
        new(@in: @"fal ", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ' ' in literal false (expecting 's')", (int64)len(@"fal "))))),
        new(@in: @"false", err: io.EOF)
    ]);
    foreach (var (_, tt) in range(tests)) {
        var d = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        for (nint i = 0; true; i++) {
            var (_, err) = d.Token(); if (err != nil) {
                if (!reflect.DeepEqual(err, tt.err)) {
                    t.Errorf("`%s`: %d.Token error = %#v, want %v", tt.@in, i, err, tt.err);
                }
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unconsumedTestdataˢ = (@string)"unconsumed testdata"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ1 => ref inputᶜ1ᵇ;
private static readonly @string inputᶜ1ᵇ = """
 [
		[ ] , [ "one" ] , [ "one" , "two" ] ,
		{ } , { "alpha" : "bravo" } , { "alpha" : "bravo" , "fizz" : "buzz" }
	] 
"""u8.Raw();

public static void TestDecoderInputOffset(ж<testing.T> t) {
    @string input = inputᶜ1;
    ref var wantOffsets = ref heap<slice<int64>>(slice<int64>([
        0, 1, 2, 5, 6, 7, 8, 9, 12, 13, 18, 19, 20, 21, 24, 25, 30, 31,
        38, 39, 40, 41, 46, 47, 48, 49, 52, 53, 60, 61, 70, 71, 72, 73,
        76, 77, 84, 85, 94, 95, 103, 104, 112, 113, 114, 116, 117, 117,
        117, 117
    ]), out var ᏑwantOffsets);
    ref var wantMores = ref heap<slice<bool>>(slice<bool>([
        true, true, false, true, true, false, true, true, true, false,
        true, false, true, true, true, false, true, true, true, true,
        true, false, false, false, false
    ]), out var ᏑwantMores);

    var d = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(input)));
    var dʗ1 = d;
    void checkOffset() {
        t.Helper();
        var got = dʗ1.InputOffset();
        if (len(ᏑwantOffsets.ValueSlot) == 0) {
            t.Fatalf("InputOffset = %d, want nil", got);
        }
        var want = ᏑwantOffsets.ValueSlot[0];
        if (got != want) {
            t.Fatalf("InputOffset = %d, want %d", got, want);
        }
        ᏑwantOffsets.ValueSlot = ᏑwantOffsets.ValueSlot[1..];
    }
    var dʗ2 = d;
    void checkMore() {
        t.Helper();
        var got = dʗ2.More();
        if (len(ᏑwantMores.ValueSlot) == 0) {
            t.Fatalf("More = %v, want nil", got);
        }
        var want = ᏑwantMores.ValueSlot[0];
        if (got != want) {
            t.Fatalf("More = %v, want %v", got, want);
        }
        ᏑwantMores.ValueSlot = ᏑwantMores.ValueSlot[1..];
    }
    checkOffset();
    checkMore();
    checkOffset();
    for (;;) {
        var (_, err) = d.Token(); if (AreEqual(err, io.EOF)) {
            break;
        } else if (err != nil) {
            t.Fatalf("Token error: %v", err);
        }
        checkOffset();
        checkMore();
        checkOffset();
    }
    checkOffset();
    checkMore();
    checkOffset();

    if (len(wantOffsets) + len(wantMores) > 0) {
        t.Fatal(unconsumedTestdataˢ);
    }
}

} // end json_internal_test_package
