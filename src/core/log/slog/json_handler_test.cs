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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
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

public static void TestJSONHandler(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestJSONHandler_type>([
        new(
            "none",
            new HandlerOptions(),
            @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""m"",""a"":1,""m"":{""b"":2}}"
        ),
        new(
            "replace",
            new HandlerOptions(ReplaceAttr: upperCaseKey),
            @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""m"",""A"":1,""M"":{""b"":2}}"
        )
    ]))) {
        ref var test = ref heap(new TestJSONHandler_type(), out var Ꮡtest);
        test = vᴛ1;

        t.Run(test.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var h = NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡtest.of(TestJSONHandler_type.Ꮡopts));
            var r = NewRecord(testTime, LevelInfo, "m", 0);
            r.AddAttrs(Int("a", 1), go.log.slog_package.Any("m", new map<@string, nint>{["b"] = 2}));
            var err = h.Handle(context.Background(), r); if (err != nil) {
                tΔ1.Fatal(err);
            }
            @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n");
            if (got != Ꮡtest.Value.want) {
                tΔ1.Errorf("\ngot  %s\nwant %s", got, Ꮡtest.Value.want);
            }
        });
    }
}

// for testing json.Marshaler
internal partial struct jsonMarshaler {
    internal @string s;
}

internal static @string String(this jsonMarshaler j) { return j.s; } // should be ignored

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string jsonEmptyStringˢ => ref jsonEmptyStringˢᵇ;
private static readonly @string jsonEmptyStringˢᵇ = "json: empty string"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonMarshaler j) {
    if (j.s == ""u8) {
        return (default, errors.New(jsonEmptyStringˢ));
    }
    return (slice<byte>(fmt.Sprintf(@"[%q]"u8, j.s)), default);
}

internal partial struct jsonMarshalerError {
    /*embed*/ internal jsonMarshaler jsonMarshaler;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oopsˢ => ref oopsˢᵇ;
private static readonly @string oopsˢᵇ = "oops"u8;

internal static @string Error(this jsonMarshalerError _) { return oopsˢ; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloˢ = (@string)"hello\r\n\t\a"u8;
internal static readonly object escapeˢ = (@string)@"""[{escape}]"""u8;
internal static readonly object escapeHTMLˢ = (@string)"<escapeHTML&>"u8;

public static void TestAppendJSONValue(ж<testing.T> t) {
    // jsonAppendAttrValue should always agree with json.Marshal.
    foreach (var (_, value) in range(slice<any>([
        helloˢ,
        escapeˢ,
        escapeHTMLˢ,
        // \u2028\u2029 is an edge case in JavaScript vs JSON.
        // \xF6 is an incomplete encoding.
        ((@string)(new byte[]{0xce, 0xb8, 0xe2, 0x80, 0xa8, 0xe2, 0x80, 0xa9, 0xef, 0xbf, 0xbf, 0xf6})),
        (@string)@"-123",
        (int64)(-9_200_123_456_789_123_456L),
        (uint64)9_200_123_456_789_123_456UL,
        -12.75D,
        1.23e-9D,
        false,
        time_package.ΔMinute,
        testTime,
        new jsonMarshaler("xyz"),
        new jsonMarshalerError(new jsonMarshaler("pqr")),
        LevelWarn
    ]))) {
        @string got = jsonValueString(AnyValue(value));
        var (want, err) = marshalJSON(value);
        if (err != nil) {
            t.Fatal(err);
        }
        if (got != want) {
            t.Errorf("%v: got %s, want %s", value, got, want);
        }
    }
}

internal static (@string, error) marshalJSON(any x) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = json.NewEncoder(new slog_test_package.bytes_BufferжWriter(Ꮡbuf));
    enc.SetEscapeHTML(false);
    var err = enc.Encode(x); if (err != nil) {
        return ("", err);
    }
    return (strings.TrimSpace(Ꮡbuf.String()), default);
}

internal partial struct TestJSONAppendAttrValueSpecial_type /*dyn*/ {
    internal any value;
    internal @string want;
}

public static void TestJSONAppendAttrValueSpecial(ж<testing.T> t) {
    // Attr values that render differently from json.Marshal.
    foreach (var (_, test) in range(slice<TestJSONAppendAttrValueSpecial_type>([
        new(math.NaN(), @"""!ERROR:json: unsupported value: NaN"""),
        new(math.Inf(+1), @"""!ERROR:json: unsupported value: +Inf"""),
        new(math.Inf(-1), @"""!ERROR:json: unsupported value: -Inf"""),
        new(io.EOF, @"""EOF""")
    ]))) {
        @string got = jsonValueString(AnyValue(test.value));
        if (got != test.want) {
            t.Errorf("%v: got %s, want %s", test.value, got, test.want);
        }
    }
}

internal static @string jsonValueString(global::go.log.slog_package.Value v) {
    ref var buf = ref heap<slice<byte>>(out var Ꮡbuf);
    var s = Ꮡ(new handleState(h: Ꮡ(new commonHandler(json: true)), buf: Ꮡbuf.Reinterpret<slice<byte>, buffer.Buffer>()));
    var err = appendJSONValue(s, v); if (err != nil) {
        s.appendError(err);
    }
    return (@string)buf;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string anErrorOccurredˢ => ref anErrorOccurredˢᵇ;
private static readonly @string anErrorOccurredˢᵇ = "an error occurred"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;
internal static ref readonly @string attrsˢ => ref attrsˢᵇ;
private static readonly @string attrsˢᵇ = "attrs"u8;
internal static ref readonly @string componentˢ => ref componentˢᵇ;
private static readonly @string componentˢᵇ = "component"u8;
internal static ref readonly @string subtestˢ => ref subtestˢᵇ;
private static readonly @string subtestˢᵇ = "subtest"u8;
internal static ref readonly @string flagˢ => ref flagˢᵇ;
private static readonly @string flagˢᵇ = "flag"u8;
internal static ref readonly @string errorˢ2 => ref errorˢ2ᵇ;
private static readonly @string errorˢ2ᵇ = "error"u8;
internal static ref readonly @string eventˢ => ref eventˢᵇ;
private static readonly @string eventˢᵇ = "event"u8;

internal partial struct TestJSONAllocs_testEvent /*dyn*/ {
    public nint ID;
    public @string Scope;
    public bool Enabled;
}

public static void TestJSONAllocs(ж<testing.T> t) {
    var ctx = t.Context();
    var l = New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(io.Discard, Ꮡ(new HandlerOptions()))));
    var testErr = errors.New(anErrorOccurredˢ);
    ref var testEvent = ref heap<TestJSONAllocs_testEvent>(new TestJSONAllocs_testEvent(
        123456, "abcdefgh", true
    ), out var ᏑtestEvent);

    var ctxʗ1 = ctx;
    var lʗ1 = l;
    t.Run(messageˢ, (ж<testing.T> tΔ1) => {
        var ctxʗ2 = ctxʗ1;
        var lʗ2 = lʗ1;
        wantAllocs(tΔ1, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
            lʗ2.LogAttrs(ctxʗ2, LevelInfo,
                helloWorldˢ);
        });
    });
    var ctxʗ3 = ctx;
    var lʗ3 = l;
    var testErrʗ1 = testErr;
    var testEventʗ1 = testEvent;
    t.Run(attrsˢ, (ж<testing.T> tΔ2) => {
        var ctxʗ4 = ctxʗ3;
        var lʗ4 = lʗ3;
        var testErrʗ2 = testErrʗ1;
        var testEventʗ2 = testEventʗ1;
        wantAllocs(tΔ2, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
            lʗ4.LogAttrs(ctxʗ4, LevelInfo,
                helloWorldˢ,
                go.log.slog_package.String(componentˢ, subtestˢ),
                Int("id", 67890),
                go.log.slog_package.Bool(flagˢ, true),
                go.log.slog_package.Any(errorˢ2, testErrʗ2),
                go.log.slog_package.Any(eventˢ, testEventʗ2));
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string programˢ => ref programˢᵇ;
private static readonly @string programˢᵇ = "program"u8;
internal static ref readonly @string myTestProgramˢ => ref myTestProgramˢᵇ;
private static readonly @string myTestProgramˢᵇ = "my-test-program"u8;
internal static ref readonly @string packageˢ => ref packageˢᵇ;
private static readonly @string packageˢᵇ = "package"u8;
internal static ref readonly @string logSlogˢ => ref logSlogˢᵇ;
private static readonly @string logSlogˢᵇ = "log/slog"u8;
internal static ref readonly @string traceIDˢ => ref traceIDˢᵇ;
private static readonly @string traceIDˢᵇ = "traceID"u8;
internal static ref readonly @string urlˢ => ref urlˢᵇ;
private static readonly @string urlˢᵇ = "URL"u8;
internal static ref readonly @string httpsPkgGoDevGolangOrgXˢ => ref httpsPkgGoDevGolangOrgXˢᵇ;
private static readonly @string httpsPkgGoDevGolangOrgXˢᵇ = "https://pkg.go.dev/golang.org/x/log/slog"u8;
internal static ref readonly @string thisIsATypicalLogMessageˢ => ref thisIsATypicalLogMessageˢᵇ;
private static readonly @string thisIsATypicalLogMessageˢᵇ = "this is a typical log message"u8;
internal static ref readonly @string moduleˢ => ref moduleˢᵇ;
private static readonly @string moduleˢᵇ = "module"u8;
internal static ref readonly @string githubComGoogleGoCmpˢ => ref githubComGoogleGoCmpˢᵇ;
private static readonly @string githubComGoogleGoCmpˢᵇ = "github.com/google/go-cmp"u8;
internal static ref readonly @string versionˢ => ref versionˢᵇ;
private static readonly @string versionˢᵇ = "version"u8;
internal static ref readonly @string v1234ˢ => ref v1234ˢᵇ;
private static readonly @string v1234ˢᵇ = "v1.23.4"u8;
internal static ref readonly @string countˢ => ref countˢᵇ;
private static readonly @string countˢᵇ = "count"u8;
internal static ref readonly @string numberˢ => ref numberˢᵇ;
private static readonly @string numberˢᵇ = "number"u8;

internal partial struct BenchmarkJSONHandler_type /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.HandlerOptions opts;
}

public static void BenchmarkJSONHandler(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(slice<BenchmarkJSONHandler_type>([
        new("defaults", new HandlerOptions()),
        new("time format", new HandlerOptions(
            ReplaceAttr: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                var v = a.Value;
                if (v.Kind() == KindTime) {
                    return go.log.slog_package.String(a.Key, v.Time().Format(rfc3339Millis));
                }
                if (a.Key == "level"u8) {
                    return new Attr("severity", a.Value);
                }
                return a;
            }
        )),
        new("time unix", new HandlerOptions(
            ReplaceAttr: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                var v = a.Value;
                if (v.Kind() == KindTime) {
                    return go.log.slog_package.Int64(a.Key, v.Time().UnixNano());
                }
                if (a.Key == "level"u8) {
                    return new Attr("severity", a.Value);
                }
                return a;
            }
        ))
    ]))) {
        ref var bench = ref heap(new BenchmarkJSONHandler_type(), out var Ꮡbench);
        bench = vᴛ1;

        b.Run(bench.name, (ж<testing.B> bΔ1) => {
            var ctx = context.Background();
            var l = New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(io.Discard, Ꮡbench.of(BenchmarkJSONHandler_type.Ꮡopts)))).With(
                go.log.slog_package.String(programˢ, myTestProgramˢ),
                go.log.slog_package.String(packageˢ, logSlogˢ),
                go.log.slog_package.String(traceIDˢ, "2039232309232309"),
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
internal static ref readonly @string requestˢ => ref requestˢᵇ;
private static readonly @string requestˢᵇ = "request"u8;
internal static ref readonly @string benchLogˢ => ref benchLogˢᵇ;
private static readonly @string benchLogˢᵇ = "bench.log"u8;
internal static ref readonly @string methodˢ => ref methodˢᵇ;
private static readonly @string methodˢᵇ = "method"u8;
internal static ref readonly @string getˢ => ref getˢᵇ;
private static readonly @string getˢᵇ = "GET"u8;
internal static ref readonly @string addrˢ => ref addrˢᵇ;
private static readonly @string addrˢᵇ = "addr"u8;

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

public static void BenchmarkPreformatting(ж<testing.B> b) {
    GoFrame ᒐ = default;
    try {
        var structAttrs = slice<any>([
            go.log.slog_package.String(programˢ, myTestProgramˢ),
            go.log.slog_package.String(packageˢ, logSlogˢ),
            go.log.slog_package.Any(requestˢ, Ꮡ(new BenchmarkPreformatting_req(
                Method: "GET",
                URL: "https://pkg.go.dev/golang.org/x/log/slog",
                TraceID: "2039232309232309",
                Addr: "127.0.0.1:8080"
            )))
        ]);

        var (outFile, err) = os.Create(filepath.Join(b.TempDir(), benchLogˢ));
        if (err != nil) {
            b.Fatal(err);
        }
        var outFileʗ1 = outFile;
        defer(() => {
            var errΔ1 = outFileʗ1.Close(); if (errΔ1 != nil) {
                b.Fatal(errΔ1);
            }
        }, ref ᒐ);

        foreach (var (_, vᴛ1) in range(slice<BenchmarkPreformatting_type>([
            new("separate", io.Discard, slice<any>([
                go.log.slog_package.String(programˢ, myTestProgramˢ),
                go.log.slog_package.String(packageˢ, logSlogˢ),
                go.log.slog_package.String(methodˢ, getˢ),
                go.log.slog_package.String(urlˢ, httpsPkgGoDevGolangOrgXˢ),
                go.log.slog_package.String(traceIDˢ, "2039232309232309"),
                go.log.slog_package.String(addrˢ, "127.0.0.1:8080")
            ])),
            new("struct", io.Discard, structAttrs),
            new("struct file", new os.FileжWriter(outFile), structAttrs)
        ]))) {
            ref var bench = ref heap(new BenchmarkPreformatting_type(), out var Ꮡbench);
            bench = vᴛ1;

            var ctx = context.Background();
            var benchʗ1 = bench;
            var ctxʗ1 = ctx;
            b.Run(bench.name, (ж<testing.B> bΔ1) => {
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
internal static ref readonly @string jsonMarshalˢ => ref jsonMarshalˢᵇ;
private static readonly @string jsonMarshalˢᵇ = "json.Marshal"u8;
internal static ref readonly @string encoderEncodeˢ => ref encoderEncodeˢᵇ;
private static readonly @string encoderEncodeˢᵇ = "Encoder.Encode"u8;

public static void BenchmarkJSONEncoding(ж<testing.B> b) {
    GoFrame ᒐ = default;
    try {
        var value = 3.14D;
        var buf = buffer.New();
        var bufʗ1 = buf;
        defer(bufʗ1.Free, ref ᒐ);
        var bufʗ2 = buf;
        b.Run(jsonMarshalˢ, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (by, err) = json.Marshal(value);
                if (err != nil) {
                    bΔ1.Fatal(err);
                }
                bufʗ2.Write(by);
                bufʗ2.ValueSlot = (bufʗ2.ValueSlot)[..0];
            }
        });
        var bufʗ3 = buf;
        b.Run(encoderEncodeˢ, (ж<testing.B> bΔ2) => {
            bΔ2.ReportAllocs();
            for (nint i = 0; i < (~bΔ2).N; i++) {
                var err = json.NewEncoder(new slog_test_package.buffer_BufferжWriter(bufʗ3)).Encode(value); if (err != nil) {
                    bΔ2.Fatal(err);
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
