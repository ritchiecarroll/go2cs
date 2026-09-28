// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
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
using System.Runtime.CompilerServices;
using go.net;
using go.net.http;
using go.runtime;
using static go.encoding.json_package;

partial class json_internal_test_package {

// TODO(https://go.dev/issue/52751): Replace with native testing support.

// CaseName is a case name annotated with a file and line.
[GoType] [GoValueClone("Where")] public partial struct CaseName {
    public @string Name;
    public CasePos Where;
}

// Name annotates a case name with the file and line of the caller.
[MethodImpl(MethodImplOptions.NoInlining)] public static CaseName /*c*/ Name(@string s) {
    CaseName c = new();

    c.Name = s;
    runtime.Callers(2, c.Where.pc[..]);
    return c.ΔClone();
}

// CasePos represents a file and line number.
[GoType] [GoValueClone("pc")] public partial struct CasePos {
    internal array<uintptr> pc = new(1);
}

public static @string String(this CasePos pos) {
    pos = pos.ΔClone();

    var frames = runtime.CallersFrames(pos.pc[..]);
    var (frame, _) = frames.Next();
    return fmt.Sprintf("%s:%d"u8, path.Base(frame.File), frame.Line);
}

// another value to make sure something can follow map
// Test values for the stream test.
// One of each JSON kind.
internal static slice<any> streamTest = new any[]{
    0.1D,
    (@string)"hello"u8,
    default!,
    true,
    false,
    new any[]{(@string)"a"u8, (@string)"b"u8, (@string)"c"u8}.slice(),
    new map<@string, any>{["K"u8] = (@string)"Kelvin"u8, ["ß"u8] = (@string)"long s"u8},
    3.14D
}.slice();

internal static @string streamEncoded = """
0.1
"hello"
null
true
false
["a","b","c"]
{"ß":"long s","K":"Kelvin"}
3.14

"""u8;

public static void TestEncoder(ж<testing.T> Ꮡt) {
    for (nint i = 0; i <= len(streamTest); i++) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
        // Check that enc.SetIndent("", "") turns off indentation.
        enc.SetIndent(">"u8, "."u8);
        enc.SetIndent(""u8, ""u8);
        foreach (var (j, v) in streamTest[0..(int)(i)]) {
            {
                var err = enc.Encode(v); if (err != default!) {
                    Ꮡt.Fatalf("#%d.%d Encode error: %v"u8, i, j, err);
                }
            }
        }
        {
            @string got = buf.String();
            @string want = nlines(streamEncoded, i); if (got != want) {
                Ꮡt.Errorf("encoding %d items: mismatch:"u8, i);
                diff(Ꮡt, slice<byte>(got), slice<byte>(want));
                break;
            }
        }
    }
}

// Trigger an error in Marshal with cyclic data.
[GoType("dyn")] [GoLocalName("Dummy")] internal partial struct TestEncoderErrorAndReuseEncodeState_Dummy {
    public @string Name;
    public ж<TestEncoderErrorAndReuseEncodeState_Dummy> Next;
}

[GoType("dyn")] [GoLocalName("Data")] internal partial struct TestEncoderErrorAndReuseEncodeState_Data {
    public @string A;
    public nint I;
}

public static void TestEncoderErrorAndReuseEncodeState(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Disable the GC temporarily to prevent encodeState's in Pool being cleaned away during the test.
        nint percent = debug.SetGCPercent(-1);
        defer(debug.SetGCPercent, percent, ref ᒐ);
        ref var dummy = ref heap<TestEncoderErrorAndReuseEncodeState_Dummy>(out var Ꮡdummy);
        dummy = new TestEncoderErrorAndReuseEncodeState_Dummy(Name: "Dummy"u8);
        dummy.Next = Ꮡdummy;
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var enc = NewEncoder(new json_test_package.bytes_BufferжWriter(Ꮡbuf));
        {
            var err = enc.Encode(dummy); if (err == default!) {
                Ꮡt.Errorf("Encode(dummy) error: got nil, want non-nil"u8);
            }
        }
        var want = new TestEncoderErrorAndReuseEncodeState_Data(A: "a"u8, I: 1);
        {
            var err = enc.Encode(want); if (err != default!) {
                Ꮡt.Errorf("Marshal error: %v"u8, err);
            }
        }
        ref var got = ref heap(new TestEncoderErrorAndReuseEncodeState_Data(), out var Ꮡgot);
        {
            var err = Unmarshal(buf.Bytes(), Ꮡgot); if (err != default!) {
                Ꮡt.Errorf("Unmarshal error: %v"u8, err);
            }
        }
        if (got != want) {
            Ꮡt.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %v\n\twant: %v"u8, got, want);
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

"""u8;

public static void TestEncoderIndent(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
    enc.SetIndent(">"u8, "."u8);
    foreach (var (_, v) in streamTest) {
        enc.Encode(v);
    }
    {
        @string got = buf.String();
        @string want = streamEncodedIndent; if (got != want) {
            Ꮡt.Errorf("Encode mismatch:\ngot:\n%s\n\nwant:\n%s"u8, got, want);
            diff(Ꮡt, slice<byte>(got), slice<byte>(want));
        }
    }
}

[GoType("@string")] internal partial struct strMarshaler;

internal static (slice<byte>, error) MarshalJSON(this strMarshaler s) {
    return (slice<byte>((@string)s), default!);
}

[GoType("@string")] internal partial struct strPtrMarshaler;

[GoRecv] internal static (slice<byte>, error) MarshalJSON(this ref strPtrMarshaler s) {
    return (slice<byte>((@string)s), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tagStructˢ = "tagStruct"u8;
internal static readonly @string strˢ = @"""<str>"""u8;
internal static readonly @string stringOptionˢ = "stringOption"u8;

[GoType("dyn")] internal partial struct TestEncoderSetEscapeHTML_tagStruct {
    [GoTag(@"json:""<>&#! """)]
    public nint Valid;
    [GoTag(@"json:""\\""")]
    public nint Invalid;
}

[GoType("dyn")] internal partial struct TestEncoderSetEscapeHTML_type {
    public strMarshaler NonPtr;
    public strPtrMarshaler Ptr;
}

[GoType("dyn")] internal partial struct TestEncoderSetEscapeHTML_stringOption {
    [GoTag(@"json:""bar,string""")]
    public @string Bar;
}

[GoType("dyn")] internal partial struct TestEncoderSetEscapeHTML_tests {
    public partial ref CaseName CaseName { get; }
    internal any v;
    internal @string wantEscape;
    internal @string want;
}

public static void TestEncoderSetEscapeHTML(ж<testing.T> Ꮡt) {
    C c = default!;
    CText ct = default!;
    TestEncoderSetEscapeHTML_tagStruct tagStruct = default!;
    // This case is particularly interesting, as we force the encoder to
    // take the address of the Ptr field to use its MarshalJSON method. This
    // is why the '&' is important.
    var marshalerStruct = Ꮡ(new TestEncoderSetEscapeHTML_type(@"""<str>"""u8, @"""<str>"""u8));
    // https://golang.org/issue/34154
    var stringOption = new TestEncoderSetEscapeHTML_stringOption(@"<html>foobar</html>"u8);
    var tests = new TestEncoderSetEscapeHTML_tests[]{
        new(Name("c"u8), c, @"""\u003c\u0026\u003e"""u8, @"""<&>"""u8),
        new(Name("ct"u8), ct, @"""\""\u003c\u0026\u003e\"""""u8, @"""\""<&>\"""""u8),
        new(Name(@"""<&>"""u8), (@string)"<&>"u8, @"""\u003c\u0026\u003e"""u8, @"""<&>"""u8),
        new(
            Name(tagStructˢ), tagStruct,
            @"{""\u003c\u003e\u0026#! "":0,""Invalid"":0}"u8,
            @"{""<>&#! "":0,""Invalid"":0}"u8
        ),
        new(
            Name(strˢ), marshalerStruct.OrTypedNil(),
            @"{""NonPtr"":""\u003cstr\u003e"",""Ptr"":""\u003cstr\u003e""}"u8,
            @"{""NonPtr"":""<str>"",""Ptr"":""<str>""}"u8
        ),
        new(
            Name(stringOptionˢ), stringOption,
            @"{""bar"":""\""\\u003chtml\\u003efoobar\\u003c/html\\u003e\""""}"u8,
            @"{""bar"":""\""<html>foobar</html>\""""}"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestEncoderSetEscapeHTML_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var enc = NewEncoder(new json_test_package.strings_BuilderжWriter(Ꮡbuf));
            {
                var err = enc.Encode(ttʗ1.v); if (err != default!) {
                    tΔ1.Fatalf("%s: Encode(%s) error: %s"u8, ttʗ1.Where, ttʗ1.Name, err);
                }
            }
            {
                @string got = strings.TrimSpace(buf.String()); if (got != ttʗ1.wantEscape) {
                    tΔ1.Errorf("%s: Encode(%s):\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, ttʗ1.Name, got, ttʗ1.wantEscape);
                }
            }
            buf.Reset();
            enc.SetEscapeHTML(false);
            {
                var err = enc.Encode(ttʗ1.v); if (err != default!) {
                    tΔ1.Fatalf("%s: SetEscapeHTML(false) Encode(%s) error: %s"u8, ttʗ1.Where, ttʗ1.Name, err);
                }
            }
            {
                @string got = strings.TrimSpace(buf.String()); if (got != ttʗ1.want) {
                    tΔ1.Errorf("%s: SetEscapeHTML(false) Encode(%s):\n\tgot:  %s\n\twant: %s"u8,
                        ttʗ1.Where, ttʗ1.Name, got, ttʗ1.want);
                }
            }
        });
    }
}

public static void TestDecoder(ж<testing.T> Ꮡt) {
    for (nint i = 0; i <= len(streamTest); i++) {
        // Use stream without newlines as input,
        // just to stress the decoder even more.
        // Our test input does not include back-to-back numbers.
        // Otherwise stripping the newlines would
        // merge two adjacent JSON values.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        foreach (var (_, c) in nlines(streamEncoded, i)) {
            if (c != (rune)'\n') {
                buf.WriteRune(c);
            }
        }
        var @out = new slice<any>(i);
        var dec = NewDecoder(new json_test_package.bytes_BufferжReader(Ꮡbuf));
        foreach (var (j, _) in @out) {
            {
                var err = dec.Decode(Ꮡ(@out, j)); if (err != default!) {
                    Ꮡt.Fatalf("decode #%d/%d error: %v"u8, j, i, err);
                }
            }
        }
        if (!reflect.DeepEqual(@out, streamTest[0..(int)(i)])) {
            Ꮡt.Errorf("decoding %d items: mismatch:"u8, i);
            foreach (var (j, _) in @out) {
                if (!reflect.DeepEqual(@out[j], streamTest[j])) {
                    Ꮡt.Errorf("#%d:\n\tgot:  %v\n\twant: %v"u8, j, @out[j], streamTest[j]);
                }
            }
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameGopherExtraˢ = @"{""Name"": ""Gopher""} extra "u8;
internal static readonly @string extraˢ = " extra "u8;

[GoType("dyn")] internal partial struct TestDecoderBuffered_m {
    public @string Name;
}

public static void TestDecoderBuffered(ж<testing.T> Ꮡt) {
    var r = strings.NewReader(nameGopherExtraˢ);
    ref var m = ref heap(new TestDecoderBuffered_m(), out var Ꮡm);
    var d = NewDecoder(new json_test_package.strings_ReaderжReader(r));
    var err = d.Decode(Ꮡm);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (m.Name != "Gopher"u8) {
        Ꮡt.Errorf("Name = %s, want Gopher"u8, m.Name);
    }
    (var rest, err) = io.ReadAll(d.Buffered());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = ((@string)rest);
        @string want = extraˢ; if (got != want) {
            Ꮡt.Errorf("Remaining = %s, want %s"u8, got, want);
        }
    }
}

internal static @string nlines(@string s, nint n) {
    if (n <= 0) {
        return ""u8;
    }
    foreach (var (i, c) in s) {
        if (c == (rune)'\n') {
            {
                n--; if (n == 0) {
                    return s[0..(int)(i + 1)];
                }
            }
        }
    }
    return s;
}

[GoType("dyn")] internal partial struct TestRawMessage_data {
    public float64 X;
    public global::go.encoding.json_package.RawMessage Id;
    public float32 Y;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string rawᶜ = @"[""\u0056"",null]"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ8 = @"{""X"":0.1,""Id"":[""\u0056"",null],""Y"":0.2}"u8;

public static void TestRawMessage(ж<testing.T> Ꮡt) {
    ref var data = ref heap(new TestRawMessage_data(), out var Ꮡdata);
    @string raw = rawᶜ;
    @string want = wantᶜ8;
    var err = Unmarshal(slice<byte>(want), Ꮡdata);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
    if (((sstring)((slice<byte>)data.Id)) != raw) {
        Ꮡt.Fatalf("Unmarshal:\n\tgot:  %s\n\twant: %s"u8, ((slice<byte>)data.Id), raw);
    }
    (var got, err) = Marshal(Ꮡdata);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)got) != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

[GoType("dyn")] internal partial struct TestNullRawMessage_data {
    public float64 X;
    public global::go.encoding.json_package.RawMessage Id;
    public ж<global::go.encoding.json_package.RawMessage> IdPtr;
    public float32 Y;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ9 = @"{""X"":0.1,""Id"":null,""IdPtr"":null,""Y"":0.2}"u8;

public static void TestNullRawMessage(ж<testing.T> Ꮡt) {
    ref var data = ref heap(new TestNullRawMessage_data(), out var Ꮡdata);
    @string want = wantᶜ9;
    var err = Unmarshal(slice<byte>(want), Ꮡdata);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
    {
        @string wantΔ1 = nullˢ;
        @string gotΔ1 = ((@string)(slice<byte>)data.Id); if (wantΔ1 != gotΔ1) {
            Ꮡt.Fatalf("Unmarshal:\n\tgot:  %s\n\twant: %s"u8, gotΔ1, wantΔ1);
        }
    }
    if (data.IdPtr != nil) {
        Ꮡt.Fatalf("pointer mismatch: got non-nil, want nil"u8);
    }
    (var got, err) = Marshal(Ꮡdata);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)got) != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

[GoType("dyn")] internal partial struct TestBlocking_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
}

public static void TestBlocking(ж<testing.T> Ꮡt) {
    var tests = new TestBlocking_tests[]{
        new(Name(""u8), @"{""x"": 1}"u8),
        new(Name(""u8), @"[1, 2, 3]"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBlocking_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (r, w) = net.Pipe();
            var wʗ1 = w;
            goǃ(ᴛ1 => wʗ1.Write(ᴛ1), slice<byte>(ttʗ1.@in));
            ref var val = ref heap<any>(out var Ꮡval);
            // If Decode reads beyond what w.Write writes above,
            // it will block, and the test will deadlock.
            {
                var err = NewDecoder(new json_test_package.net_ConnᴠReader(r)).Decode(Ꮡval); if (err != default!) {
                    tΔ1.Errorf("%s: NewDecoder(%s).Decode error: %v"u8, ttʗ1.Where, ttʗ1.@in, err);
                }
            }
            r.Close();
            w.Close();
        });
    }
}

[GoType] internal partial struct decodeThis {
    internal any v;
}

[GoType("dyn")] internal partial struct TestDecodeInStream_tests {
    public partial ref CaseName CaseName { get; }
    internal @string json;
    internal slice<any> expTokens;
}

public static void TestDecodeInStream(ж<testing.T> Ꮡt) {
    var tests = new TestDecodeInStream_tests[]{ // streaming token cases

        new(CaseName: Name(""u8), json: @"10"u8, expTokens: new any[]{(float64)10D}.slice()),
        new(CaseName: Name(""u8), json: @" [10] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['), (float64)10D, ((global::go.encoding.json_package.Delim)(rune)']')}.slice()),
        new(CaseName: Name(""u8), json: @" [false,10,""b""] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['), false, (float64)10D, (@string)"b"u8, ((global::go.encoding.json_package.Delim)(rune)']')}.slice()),
        new(CaseName: Name(""u8), json: @"{ ""a"": 1 }"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)1D, ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @"{""a"": 1, ""b"":""3""}"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)1D, (@string)"b"u8, (@string)"3"u8, ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @" [{""a"": 1},{""a"": 2}] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['),
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)1D, ((global::go.encoding.json_package.Delim)(rune)'}'),
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)2D, ((global::go.encoding.json_package.Delim)(rune)'}'),
            ((global::go.encoding.json_package.Delim)(rune)']')}.slice()),
        new(CaseName: Name(""u8), json: @"{""obj"": {""a"": 1}}"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"obj"u8, ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)1D, ((global::go.encoding.json_package.Delim)(rune)'}'),
            ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @"{""obj"": [{""a"": 1}]}"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"obj"u8, ((global::go.encoding.json_package.Delim)(rune)'['),
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8, (float64)1D, ((global::go.encoding.json_package.Delim)(rune)'}'),
            ((global::go.encoding.json_package.Delim)(rune)']'), ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()), // streaming tokens with intermittent Decode()

        new(CaseName: Name(""u8), json: @"{ ""a"": 1 }"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"a"u8,
            new decodeThis((float64)1D),
            ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @" [ { ""a"" : 1 } ] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['),
            new decodeThis(new map<@string, any>{["a"u8] = (float64)1D}),
            ((global::go.encoding.json_package.Delim)(rune)']')}.slice()),
        new(CaseName: Name(""u8), json: @" [{""a"": 1},{""a"": 2}] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['),
            new decodeThis(new map<@string, any>{["a"u8] = (float64)1D}),
            new decodeThis(new map<@string, any>{["a"u8] = (float64)2D}),
            ((global::go.encoding.json_package.Delim)(rune)']')}.slice()),
        new(CaseName: Name(""u8), json: @"{ ""obj"" : [ { ""a"" : 1 } ] }"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"obj"u8, ((global::go.encoding.json_package.Delim)(rune)'['),
            new decodeThis(new map<@string, any>{["a"u8] = (float64)1D}),
            ((global::go.encoding.json_package.Delim)(rune)']'), ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @"{""obj"": {""a"": 1}}"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"obj"u8,
            new decodeThis(new map<@string, any>{["a"u8] = (float64)1D}),
            ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @"{""obj"": [{""a"": 1}]}"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), (@string)"obj"u8,
            new decodeThis(new any[]{
                new map<@string, any>{["a"u8] = (float64)1D}
            }.slice()
            ),
            ((global::go.encoding.json_package.Delim)(rune)'}')}.slice()),
        new(CaseName: Name(""u8), json: @" [{""a"": 1} {""a"": 2}] "u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'['),
            new decodeThis(new map<@string, any>{["a"u8] = (float64)1D}),
            new decodeThis(Ꮡ(new SyntaxError("expected comma after array element"u8, 11)))
        }.slice()),
        new(CaseName: Name(""u8), json: @"{ """u8 + strings.Repeat("a"u8, 513) + @""" 1 }"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'), strings.Repeat("a"u8, 513),
            new decodeThis(Ꮡ(new SyntaxError("expected colon after object key"u8, 518)))
        }.slice()),
        new(CaseName: Name(""u8), json: @"{ ""\a"" }"u8, expTokens: new any[]{
            ((global::go.encoding.json_package.Delim)(rune)'{'),
            Ꮡ(new SyntaxError("invalid character 'a' in string escape code"u8, 3))
        }.slice()),
        new(CaseName: Name(""u8), json: @" \a"u8, expTokens: new any[]{
            Ꮡ(new SyntaxError("invalid character '\\\\' looking for beginning of value"u8, 1))
        }.slice())
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestDecodeInStream_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.json)));
            foreach (var (i, vᴛ2) in ttʗ1.expTokens) {
                var want = vᴛ2;

                ref var got = ref heap<any>(out var Ꮡgot);
                error err = default!;
                {
                    var (dt, ok) = want._<decodeThis>(ᐧ); if (ok){
                        want = dt.v;
                        err = dec.Decode(Ꮡgot);
                    } else {
                        (got, err) = dec.Token();
                    }
                }
                {
                    var (errWant, ok) = want._<error>(ᐧ); if (ok){
                        if (err == default! || !reflect.DeepEqual(err, errWant)) {
                            tΔ1.Fatalf("%s:\n\tinput: %s\n\tgot error:  %v\n\twant error: %v"u8, ttʗ1.Where, ttʗ1.json, err, errWant);
                        }
                        break;
                    } else 
                    if (err != default!) {
                        tΔ1.Fatalf("%s:\n\tinput: %s\n\tgot error:  %v\n\twant error: nil"u8, ttʗ1.Where, ttʗ1.json, err);
                    }
                }
                if (!reflect.DeepEqual(got, want)) {
                    tΔ1.Fatalf("%s: token %d:\n\tinput: %s\n\tgot:  %T(%v)\n\twant: %T(%v)"u8, ttʗ1.Where, i, ttʗ1.json, got, got, want, want);
                }
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string rawᶜ1 = @"{ ""foo"": ""bar"" }"u8;

[GoType("dyn")] internal partial struct TestHTTPDecoding_foo {
    public @string Foo;
}

// Test from golang.org/issue/11893
public static void TestHTTPDecoding(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string raw = rawᶜ1;
        var ts = httptest.NewServer(new json_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(raw));
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = http.Get((~ts).URL);
        if (err != default!) {
            log.Fatalf("http.Get error: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        ref var foo = ref heap<TestHTTPDecoding_foo>(out var Ꮡfoo);
        foo = new TestHTTPDecoding_foo();
        var d = NewDecoder(new json_test_package.io_ReadCloserᴠReader((~res).Body));
        err = d.Decode(Ꮡfoo);
        if (err != default!) {
            Ꮡt.Fatalf("Decode error: %v"u8, err);
        }
        if (foo.Foo != "bar"u8) {
            Ꮡt.Errorf(@"Decode: got %q, want ""bar"""u8, foo.Foo);
        }
        // make sure we get the EOF the second time
        err = d.Decode(Ꮡfoo);
        if (!AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Decode error:\n\tgot:  %v\n\twant: io.EOF"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end json_internal_test_package
