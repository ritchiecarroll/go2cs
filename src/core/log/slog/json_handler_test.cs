// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using json = go.encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using buffer = go.log.slog.@internal.buffer_package;
using math = math_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.encoding;
using go.log.slog.@internal;
using path;
using static go.log.slog_package;

partial class slog_internal_test_package {

internal partial struct TestJSONHandler_type /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.HandlerOptions opts;
    internal @string want;
}

public static void TestJSONHandler(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestJSONHandler_type[]{
        new(
            "none"u8,
            new HandlerOptions(nil),
            @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""m"",""a"":1,""m"":{""b"":2}}"u8
        ),
        new(
            "replace"u8,
            new HandlerOptions(ReplaceAttr: upperCaseKey),
            @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""m"",""A"":1,""M"":{""b"":2}}"u8
        )
    }.slice()) {
        ref var test = ref heap(new TestJSONHandler_type(), out var Ꮡtest);
        test = vᴛ1;

        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var h = NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡtest.of(TestJSONHandler_type.Ꮡopts));
            var r = NewRecord(testTime, LevelInfo, "m"u8, 0);
            r.AddAttrs(Int("a"u8, 1), go.log.slog_package.Any("m"u8, new map<@string, nint>{["b"u8] = 2}));
            {
                var err = h.Handle(context.Background(), r); if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n"u8);
            if (got != Ꮡtest.Value.want) {
                tΔ1.Errorf("\ngot  %s\nwant %s"u8, got, Ꮡtest.Value.want);
            }
        });
    }
}

// for testing json.Marshaler
internal partial struct jsonMarshaler {
    internal @string s;
}

internal static @string String(this jsonMarshaler j) {
    return j.s; // should be ignored
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsonEmptyStringˢ = "json: empty string"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonMarshaler j) {
    if (j.s == ""u8) {
        return (default!, errors.New(jsonEmptyStringˢ));
    }
    return (slice<byte>(fmt.Sprintf(@"[%q]"u8, j.s)), default!);
}

internal partial struct jsonMarshalerError {
    internal partial ref jsonMarshaler jsonMarshaler { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oopsˢ = "oops"u8;

internal static @string Error(this jsonMarshalerError _) {
    return oopsˢ;
}

public static void TestAppendJSONValue(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // jsonAppendAttrValue should always agree with json.Marshal.
    foreach (var (_, value) in new any[]{
        (@string)"hello\r\n\t\a"u8,
        (@string)@"""[{escape}]"""u8,
        (@string)"<escapeHTML&>"u8, // \u2028\u2029 is an edge case in JavaScript vs JSON.
 // \xF6 is an incomplete encoding.

        ((@string)(new byte[]{0xce, 0xb8, 0xe2, 0x80, 0xa8, 0xe2, 0x80, 0xa9, 0xef, 0xbf, 0xbf, 0xf6})),
        (@string)@"-123"u8,
        (int64)(-9_200_123_456_789_123_456L),
        (uint64)9_200_123_456_789_123_456UL,
        -12.75D,
        1.23e-9D,
        false,
        time_package.ΔMinute,
        testTime,
        new jsonMarshaler("xyz"u8),
        new jsonMarshalerError(new jsonMarshaler("pqr"u8)),
        LevelWarn
    }.slice()) {
        @string got = jsonValueString(AnyValue(value));
        var (want, err) = marshalJSON(value);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (got != want) {
            Ꮡt.Errorf("%v: got %s, want %s"u8, value, got, want);
        }
    }
}

internal static (@string, error) marshalJSON(any x) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = json.NewEncoder(new slog_test_package.bytes_BufferжWriter(Ꮡbuf));
    enc.SetEscapeHTML(false);
    {
        var err = enc.Encode(x); if (err != default!) {
            return ("", err);
        }
    }
    return (strings.TrimSpace(Ꮡbuf.String()), default!);
}

internal partial struct TestJSONAppendAttrValueSpecial_type /*dyn*/ {
    internal any value;
    internal @string want;
}

public static void TestJSONAppendAttrValueSpecial(ж<testing.T> Ꮡt) {
    // Attr values that render differently from json.Marshal.
    foreach (var (_, test) in new TestJSONAppendAttrValueSpecial_type[]{
        new(math.NaN(), @"""!ERROR:json: unsupported value: NaN"""u8),
        new(math.Inf(+1), @"""!ERROR:json: unsupported value: +Inf"""u8),
        new(math.Inf(-1), @"""!ERROR:json: unsupported value: -Inf"""u8),
        new(io.EOF, @"""EOF"""u8)
    }.slice()) {
        @string got = jsonValueString(AnyValue(test.value));
        if (got != test.want) {
            Ꮡt.Errorf("%v: got %s, want %s"u8, test.value, got, test.want);
        }
    }
}

internal static @string jsonValueString(global::go.log.slog_package.Value v) {
    ref var buf = ref heap<slice<byte>>(out var Ꮡbuf);
    var s = Ꮡ(new handleState(h: Ꮡ(new commonHandler(json: true)), buf: Ꮡbuf.Reinterpret<slice<byte>, buffer.Buffer>()));
    {
        var err = appendJSONValue(s, v); if (err != default!) {
            s.appendError(err);
        }
    }
    return ((@string)buf);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string programˢ = "program"u8;
internal static readonly @string myTestProgramˢ = "my-test-program"u8;
internal static readonly @string packageˢ = "package"u8;
internal static readonly @string logSlogˢ = "log/slog"u8;
internal static readonly @string traceIDˢ = "traceID"u8;
internal static readonly @string urlˢ = "URL"u8;
internal static readonly @string httpsPkgGoDevGolangOrgXˢ = "https://pkg.go.dev/golang.org/x/log/slog"u8;
internal static readonly @string thisIsATypicalLogMessageˢ = "this is a typical log message"u8;
internal static readonly @string moduleˢ = "module"u8;
internal static readonly @string githubComGoogleGoCmpˢ = "github.com/google/go-cmp"u8;
internal static readonly @string versionˢ = "version"u8;
internal static readonly @string v1234ˢ = "v1.23.4"u8;
internal static readonly @string countˢ = "count"u8;
internal static readonly @string numberˢ = "number"u8;

internal partial struct BenchmarkJSONHandler_type /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.HandlerOptions opts;
}

public static void BenchmarkJSONHandler(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in new BenchmarkJSONHandler_type[]{
        new("defaults"u8, new HandlerOptions(nil)),
        new("time format"u8, new HandlerOptions(
            ReplaceAttr: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                var v = a.Value;
                if (v.Kind() == KindTime) {
                    return go.log.slog_package.String(a.Key, v.Time().Format(rfc3339Millis));
                }
                if (a.Key == "level"u8) {
                    return new Attr("severity"u8, a.Value);
                }
                return a;
            }
        )),
        new("time unix"u8, new HandlerOptions(
            ReplaceAttr: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                var v = a.Value;
                if (v.Kind() == KindTime) {
                    return go.log.slog_package.Int64(a.Key, v.Time().UnixNano());
                }
                if (a.Key == "level"u8) {
                    return new Attr("severity"u8, a.Value);
                }
                return a;
            }
        ))
    }.slice()) {
        ref var bench = ref heap(new BenchmarkJSONHandler_type(), out var Ꮡbench);
        bench = vᴛ1;

        Ꮡb.Run(bench.name, (ж<testing.B> bΔ1) => {
            var ctx = context.Background();
            var l = New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(io.Discard, Ꮡbench.of(BenchmarkJSONHandler_type.Ꮡopts)))).With(
                go.log.slog_package.String(programˢ, myTestProgramˢ),
                go.log.slog_package.String(packageˢ, logSlogˢ),
                go.log.slog_package.String(traceIDˢ, "2039232309232309"u8),
                go.log.slog_package.String(urlˢ, httpsPkgGoDevGolangOrgXˢ));
            bΔ1.ReportAllocs();
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                l.LogAttrs(ctx, LevelInfo, thisIsATypicalLogMessageˢ,
                    go.log.slog_package.String(moduleˢ, githubComGoogleGoCmpˢ),
                    go.log.slog_package.String(versionˢ, v1234ˢ),
                    Int(countˢ, 23),
                    Int(numberˢ, 123456));
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requestˢ = "request"u8;
internal static readonly @string benchLogˢ = "bench.log"u8;
internal static readonly @string methodˢ = "method"u8;
internal static readonly @string getˢ = "GET"u8;
internal static readonly @string addrˢ = "addr"u8;

internal partial struct BenchmarkPreformatting_req /*dyn*/ {
    public @string Method;
    public @string URL;
    public @string TraceID;
    public @string Addr;
}

internal partial struct BenchmarkPreformatting_type /*dyn*/ {
    internal @string name;
    internal io.Writer wc;
    internal slice<any> attrs;
}

public static void BenchmarkPreformatting(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        var structAttrs = new any[]{
            go.log.slog_package.String(programˢ, myTestProgramˢ),
            go.log.slog_package.String(packageˢ, logSlogˢ),
            go.log.slog_package.Any(requestˢ, Ꮡ(new BenchmarkPreformatting_req(
                Method: "GET"u8,
                URL: "https://pkg.go.dev/golang.org/x/log/slog"u8,
                TraceID: "2039232309232309"u8,
                Addr: "127.0.0.1:8080"u8
            )))
        }.slice();
        var (outFile, err) = os.Create(filepath.Join(Ꮡb.TempDir(), benchLogˢ));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var outFileʗ1 = outFile;
        defer(() => {
            {
                var errΔ1 = outFileʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
        }, ref ᒐ);
        foreach (var (_, vᴛ1) in new BenchmarkPreformatting_type[]{
            new("separate"u8, io.Discard, new any[]{
                go.log.slog_package.String(programˢ, myTestProgramˢ),
                go.log.slog_package.String(packageˢ, logSlogˢ),
                go.log.slog_package.String(methodˢ, getˢ),
                go.log.slog_package.String(urlˢ, httpsPkgGoDevGolangOrgXˢ),
                go.log.slog_package.String(traceIDˢ, "2039232309232309"u8),
                go.log.slog_package.String(addrˢ, "127.0.0.1:8080"u8)
            }.slice()),
            new("struct"u8, io.Discard, structAttrs),
            new("struct file"u8, new os.FileжWriter(outFile), structAttrs)
        }.slice()) {
            ref var bench = ref heap(new BenchmarkPreformatting_type(), out var Ꮡbench);
            bench = vᴛ1;

            var ctx = context.Background();
            var benchʗ1 = bench;
            var ctxʗ1 = ctx;
            Ꮡb.Run(bench.name, (ж<testing.B> bΔ1) => {
                var l = New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(benchʗ1.wc, nil))).With(benchʗ1.attrs.ꓸꓸꓸ);
                bΔ1.ReportAllocs();
                bΔ1.ResetTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    l.LogAttrs(ctxʗ1, LevelInfo, thisIsATypicalLogMessageˢ,
                        go.log.slog_package.String(moduleˢ, githubComGoogleGoCmpˢ),
                        go.log.slog_package.String(versionˢ, v1234ˢ),
                        Int(countˢ, 23),
                        Int(numberˢ, 123456));
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsonMarshalˢ = "json.Marshal"u8;
internal static readonly @string encoderEncodeˢ = "Encoder.Encode"u8;

public static void BenchmarkJSONEncoding(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        var value = 3.14D;
        var buf = buffer.New();
        var bufʗ1 = buf;
        defer(bufʗ1.Free, ref ᒐ);
        var bufʗ2 = buf;
        Ꮡb.Run(jsonMarshalˢ, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (by, err) = json.Marshal(value);
                if (err != default!) {
                    bΔ1.Fatal(err);
                }
                bufʗ2.Write(by);
                bufʗ2.ValueSlot = (bufʗ2.ValueSlot)[..0];
            }
        });
        var bufʗ3 = buf;
        Ꮡb.Run(encoderEncodeˢ, (ж<testing.B> bΔ2) => {
            bΔ2.ReportAllocs();
            for (nint i = 0; i < (~bΔ2).N; i++) {
                {
                    var err = json.NewEncoder(new slog_test_package.buffer_BufferжWriter(bufʗ3)).Encode(value); if (err != default!) {
                        bΔ2.Fatal(err);
                    }
                }
                bufʗ3.ValueSlot = (bufʗ3.ValueSlot)[..0];
            }
        });
        _ = buf;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end slog_internal_test_package
