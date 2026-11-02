// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using asan = go.@internal.asan_package;
using msan = go.@internal.msan_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using log = log_package;
using loginternal = go.log.internal_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using go.os;
using path;
using static go.log.slog_package;

partial class slog_internal_test_package {

// textTimeRE is a regexp to match log timestamps for Text handler.
// This is RFC3339Nano with the fixed 3 digit sub-second precision.
internal static ref readonly @string textTimeRE => ref textTimeREᵇ;
private static readonly @string textTimeREᵇ = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}(Z|[+-]\d{2}:\d{2})"u8;

// jsonTimeRE is a regexp to match log timestamps for Text handler.
// This is RFC3339Nano with an arbitrary sub-second precision.
internal static ref readonly @string jsonTimeRE => ref jsonTimeREᵇ;
private static readonly @string jsonTimeREᵇ = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string levelInfoMsgMsgA1B2ˢ => ref levelInfoMsgMsgA1B2ˢᵇ;
private static readonly @string levelInfoMsgMsgA1B2ˢᵇ = @"level=INFO msg=msg a=1 b=2"u8;
internal static ref readonly @string levelWarnMsgWDur3sˢ => ref levelWarnMsgWDur3sˢᵇ;
private static readonly @string levelWarnMsgWDur3sˢᵇ = @"level=WARN msg=w dur=3s"u8;
internal static ref readonly @string levelErrorMsgBadA1ˢ => ref levelErrorMsgBadA1ˢᵇ;
private static readonly @string levelErrorMsgBadA1ˢᵇ = @"level=ERROR msg=bad a=1"u8;
internal static ref readonly @string levelWarn1MsgWA1BTwoˢ => ref levelWarn1MsgWA1BTwoˢᵇ;
private static readonly @string levelWarn1MsgWA1BTwoˢᵇ = @"level=WARN\+1 msg=w a=1 b=two"u8;
internal static ref readonly @string levelInfo1MsgABCA1BTwoˢ => ref levelInfo1MsgABCA1BTwoˢᵇ;
private static readonly @string levelInfo1MsgABCA1BTwoˢᵇ = @"level=INFO\+1 msg=""a b c"" a=1 b=two"u8;
internal static ref readonly @string levelInfoMsgInfoAI1ˢ => ref levelInfoMsgInfoAI1ˢᵇ;
private static readonly @string levelInfoMsgInfoAI1ˢᵇ = @"level=INFO msg=info a.i=1"u8;

public static partial void TestLogTextHandler(ж<testing.T> t) {
    var ctx = context.Background();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    var l = New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil)));

    void check(@string want) {
        t.Helper();
        if (want != ""u8) {
            want = "time=" + textTimeRE + " " + want;
        }
        checkLogOutput(t, Ꮡbuf.String(), want);
        Ꮡbuf.Value.Reset();
    }

    l.Info(msgˢ, (@string)"a", (nint)1, (@string)"b", (nint)2);
    check(levelInfoMsgMsgA1B2ˢ);

    // By default, debug messages are not printed.
    l.Debug("bg", Int("a", 1), (@string)"b", (nint)2);
    check(""u8);

    l.Warn("w", go.log.slog_package.Duration(durˢ, (time.Duration)(3000000000L)));
    check(levelWarnMsgWDur3sˢ);

    l.Error(badˢ, (@string)"a", (nint)1);
    check(levelErrorMsgBadA1ˢ);

    l.Log(ctx, LevelWarn + 1, "w", Int("a", 1), go.log.slog_package.String("b", twoˢ));
    check(levelWarn1MsgWA1BTwoˢ);

    l.LogAttrs(ctx, LevelInfo + 1, aBCˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ));
    check(levelInfo1MsgABCA1BTwoˢ);

    l.Info(infoˢ2, (@string)"a", slice<global::go.log.slog_package.Attr>([Int("i", 1)]));
    check(levelInfoMsgInfoAI1ˢ);

    l.Info(infoˢ2, (@string)"a", GroupValue(Int("i", 1)));
    check(levelInfoMsgInfoAI1ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string loggerTestGoDInfoMsgA1ˢ => ref loggerTestGoDInfoMsgA1ˢᵇ;
private static readonly @string loggerTestGoDInfoMsgA1ˢᵇ = @"logger_test.go:\d+: INFO msg a=1"u8;
internal static ref readonly @string loggerTestGoDInfoMsgPNilˢ => ref loggerTestGoDInfoMsgPNilˢᵇ;
private static readonly @string loggerTestGoDInfoMsgPNilˢᵇ = @"logger_test.go:\d+: INFO msg p=<nil>"u8;
internal static ref readonly @string loggerTestGoDInfoMsgRNilˢ => ref loggerTestGoDInfoMsgRNilˢᵇ;
private static readonly @string loggerTestGoDInfoMsgRNilˢᵇ = @"logger_test.go:\d+: INFO msg r=<nil>"u8;
internal static ref readonly @string loggerTestGoDWarnMsgB2ˢ => ref loggerTestGoDWarnMsgB2ˢᵇ;
private static readonly @string loggerTestGoDWarnMsgB2ˢᵇ = @"logger_test.go:\d+: WARN msg b=2"u8;
internal static readonly object errˢ = (@string)"err"u8;
internal static ref readonly @string loggerTestGoDErrorMsgErrˢ => ref loggerTestGoDErrorMsgErrˢᵇ;
private static readonly @string loggerTestGoDErrorMsgErrˢᵇ = @"logger_test.go:\d+: ERROR msg err=EOF c=3"u8;
internal static ref readonly @string wrapDefaultHandlerˢ => ref wrapDefaultHandlerˢᵇ;
private static readonly @string wrapDefaultHandlerˢᵇ = "wrap default handler"u8;
internal static ref readonly @string loggerTestGoDInfoMsgD4ˢ => ref loggerTestGoDInfoMsgD4ˢᵇ;
private static readonly @string loggerTestGoDInfoMsgD4ˢᵇ = @"logger_test.go:\d+: INFO msg d=4"u8;
internal static ref readonly @string msg2ˢ => ref msg2ˢᵇ;
private static readonly @string msg2ˢᵇ = "msg2"u8;
internal static readonly object shouldNotAppearˢ = (@string)"should not appear"u8;
internal static readonly object msg3ˢ = (@string)"msg3"u8;
internal static ref readonly @string loggerTestGoDMsg3ˢ => ref loggerTestGoDMsg3ˢᵇ;
private static readonly @string loggerTestGoDMsg3ˢᵇ = @"logger_test.go:\d+: msg3"u8;

public static void TestConnections(ж<testing.T> t) {
    ref var logbuf = ref heap(new bytes.Buffer(), out var Ꮡlogbuf);
    ref var slogbuf = ref heap(new bytes.Buffer(), out var Ꮡslogbuf);

    // Revert any changes to the default logger. This is important because other
    // tests might change the default logger using SetDefault. Also ensure we
    // restore the default logger at the end of the test.
    var currentLogger = Default();
    var currentLogWriter = log_package.Writer();
    nint currentLogFlags = log_package.Flags();
    SetDefault(New(new global::go.log.slog_package.defaultHandlerжΔHandler(newDefaultHandler(loginternal.DefaultOutput))));
    var currentLogWriterʗ1 = currentLogWriter;
    var currentLoggerʗ1 = currentLogger;
    t.Cleanup(() => {
        SetDefault(currentLoggerʗ1);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });

    // The default slog.Logger's handler uses the log package's default output.
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(Ꮡlogbuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));
    Info(msgˢ, "a", (nint)1);
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDInfoMsgA1ˢ);
    logbuf.Reset();
    Info(msgˢ, "p", (any)(default));
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDInfoMsgPNilˢ);
    logbuf.Reset();
    ж<regexp.Regexp> r = default;
    Info(msgˢ, "r", r.OrTypedNil());
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDInfoMsgRNilˢ);
    logbuf.Reset();
    Warn(msgˢ, "b", (nint)2);
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDWarnMsgB2ˢ);
    logbuf.Reset();
    go.log.slog_package.Error(msgˢ, errˢ, io.EOF, "c", (nint)3);
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDErrorMsgErrˢ);

    // Levels below Info are not printed.
    logbuf.Reset();
    Debug(msgˢ, "c", (nint)3);
    checkLogOutput(t, Ꮡlogbuf.String(), ""u8);

    t.Run(wrapDefaultHandlerˢ, (ж<testing.T> tΔ1) => {
        // It should be possible to wrap the default handler and get the right output.
        // This works because the default handler uses the pc in the Record
        // to get the source line, rather than a call depth.
        var logger = New(new wrappingHandler(Default().Handler()));
        logger.Info(msgˢ, (@string)"d", (nint)4);
        checkLogOutput(tΔ1, Ꮡlogbuf.String(), loggerTestGoDInfoMsgD4ˢ);
    });

    // Once slog.SetDefault is called, the direction is reversed: the default
    // log.Logger's output goes through the handler.
    SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡslogbuf), Ꮡ(new HandlerOptions(AddSource: true))))));
    log_package.Print(msg2ˢ);
    checkLogOutput(t, Ꮡslogbuf.String(), "time=" + textTimeRE + @" level=INFO source=.*logger_test.go:\d{3}""? msg=msg2");

    // The default log.Logger always outputs at Info level.
    slogbuf.Reset();
    SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡslogbuf), Ꮡ(new HandlerOptions(Level: LevelWarn))))));
    log_package.Print(shouldNotAppearˢ);
    {
        @string got = Ꮡslogbuf.String(); if (got != ""u8) {
            t.Errorf("got %q, want empty", got);
        }
    }

    // Setting log's output again breaks the connection.
    logbuf.Reset();
    slogbuf.Reset();
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(Ꮡlogbuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));
    log_package.Print(msg3ˢ);
    checkLogOutput(t, Ꮡlogbuf.String(), loggerTestGoDMsg3ˢ);
    {
        @string got = Ꮡslogbuf.String(); if (got != ""u8) {
            t.Errorf("got %q, want empty", got);
        }
    }
}

internal partial struct wrappingHandler {
    internal global::go.log.slog_package.ΔHandler h;
}

internal static bool Enabled(this wrappingHandler h, context.Context ctx, global::go.log.slog_package.ΔLevel level) {
    return h.h.Enabled(ctx, level);
}

internal static global::go.log.slog_package.ΔHandler WithGroup(this wrappingHandler h, @string name) {
    return h.h.WithGroup(name);
}

internal static global::go.log.slog_package.ΔHandler WithAttrs(this wrappingHandler h, slice<global::go.log.slog_package.Attr> @as) {
    return h.h.WithAttrs(@as);
}

internal static error Handle(this wrappingHandler h, context.Context ctx, global::go.log.slog_package.Record r) {
    r = r.ΔClone();

    return h.h.Handle(ctx, r);
}

public static partial void TestAttrs(ж<testing.T> t) {
    void check(slice<global::go.log.slog_package.Attr> got, params Span<global::go.log.slog_package.Attr> wantʗp) {
        var want = wantʗp.slice();
        t.Helper();
        if (!attrsEqual(got, want)) {
            t.Errorf("got %v, want %v", got, want);
        }
    }

    var l1 = New(new slog_internal_test_package.captureHandlerжΔHandler(Ꮡ(new captureHandler()))).With((@string)"a", (nint)1);
    var l2 = New(l1.Handler()).With((@string)"b", (nint)2);
    l2.Info("m", (@string)"c", (nint)3);
    var h = l2.Handler()._<ж<captureHandler>>();
    check(h.attrs, Int("a", 1), Int("b", 2));
    check(attrsSlice(h.r), Int("c", 3));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilSourceˢ = (@string)"got nil source"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantFuncᶜ => ref wantFuncᶜᵇ;
private static readonly @string wantFuncᶜᵇ = "log/slog.TestCallDepth"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantFileᶜ => ref wantFileᶜᵇ;
private static readonly @string wantFileᶜᵇ = "logger_test.go"u8;

public static partial void TestCallDepth(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ctx = context.Background();
        var h = Ꮡ(new captureHandler());
        nint startLine = default;

        var hʗ1 = h;
        void check(nint count) {
            t.Helper();
            @string wantFunc = wantFuncᶜ;
            @string wantFile = wantFileᶜ;
            nint wantLine = startLine + count * 2;
            var got = hʗ1.r.Source();
            if (got == nil) {
                t.Fatal(gotNilSourceˢ);
            }
            @string gotFile = filepath.Base(got.File);
            if (got.Function != wantFunc || gotFile != wantFile || got.Line != wantLine) {
                t.Errorf("got (%s, %s, %d), want (%s, %s, %d)",
                    got.Function, gotFile, got.Line, wantFunc, wantFile, wantLine);
            }
        }

        defer(SetDefault, Default(), ref ᒐ); // restore
        var logger = New(new slog_internal_test_package.captureHandlerжΔHandler(h));
        SetDefault(logger);

        // Calls to check must be one line apart.
        // Determine line where calls start.
        var (f, _) = runtime.CallersFrames(slice<uintptr>([callerPC(2)])).Next();
        startLine = f.Line + 4;
        // Do not change the number of lines between here and the call to check(0).

        logger.Log(ctx, LevelInfo, ""u8);
        check(0);
        logger.LogAttrs(ctx, LevelInfo, ""u8);
        check(1);
        logger.Debug(""u8);
        check(2);
        logger.Info(""u8);
        check(3);
        logger.Warn(""u8);
        check(4);
        logger.Error(""u8);
        check(5);
        Debug(""u8);
        check(6);
        Info(""u8);
        check(7);
        Warn(""u8);
        check(8);
        go.log.slog_package.Error(""u8);
        check(9);
        Log(ctx, LevelInfo, ""u8);
        check(10);
        LogAttrs(ctx, LevelInfo, ""u8);
        check(11);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode"u8;
internal static readonly object logFatalˢ = (@string)"log.Fatal"u8;
internal static readonly object logFatallnˢ = (@string)"log.Fatalln"u8;
internal static ref readonly @string logOutputˢ => ref logOutputˢᵇ;
private static readonly @string logOutputˢᵇ = "log.Output"u8;
internal static readonly object logPanicˢ = (@string)"log.Panic"u8;
internal static readonly object logDefaultFatalˢ = (@string)"log.Default.Fatal"u8;
internal static readonly object logDefaultFatallnˢ = (@string)"log.Default.Fatalln"u8;
internal static ref readonly @string logDefaultOutputˢ => ref logDefaultOutputˢᵇ;
private static readonly @string logDefaultOutputˢᵇ = "log.Default.Output"u8;
internal static readonly object logDefaultPanicˢ = (@string)"log.Default.Panic"u8;
internal static ref readonly @string testCount1ˢ => ref testCount1ˢᵇ;
private static readonly @string testCount1ˢᵇ = "-test.count=1"u8;

internal partial struct TestCallDepthConnection_tests /*dyn*/ {
    internal @string name;
    internal Action log;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string envVarᶜ => ref envVarᶜᵇ;
private static readonly @string envVarᶜᵇ = "SLOGTEST_CALL_DEPTH_CONNECTION"u8;

public static partial void TestCallDepthConnection(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingTestInShortModeˢ);
    }

    testenv.MustHaveExec(new slog_test_package.testing_TжTB(t));
    var (ep, err) = os.Executable();
    if (err != nil) {
        t.Fatalf("Executable failed: %v", err);
    }

    var tests = slice<TestCallDepthConnection_tests>([
        new("log.Fatal", () => {
            log_package.Fatal(logFatalˢ);
        }),
        new("log.Fatalf", () => {
            log_package.Fatalf("log.Fatalf");
        }),
        new("log.Fatalln", () => {
            log_package.Fatalln(logFatallnˢ);
        }),
        new("log.Output", () => {
            log_package.Output(1, logOutputˢ);
        }),
        new("log.Panic", () => {
            log_package.Panic(logPanicˢ);
        }),
        new("log.Panicf", () => {
            log_package.Panicf("log.Panicf");
        }),
        new("log.Panicln", () => {
            log_package.Panicf("log.Panicln");
        }),
        new("log.Default.Fatal", () => {
            log_package.Default().Fatal(logDefaultFatalˢ);
        }),
        new("log.Default.Fatalf", () => {
            log_package.Default().Fatalf("log.Default.Fatalf");
        }),
        new("log.Default.Fatalln", () => {
            log_package.Default().Fatalln(logDefaultFatallnˢ);
        }),
        new("log.Default.Output", () => {
            log_package.Default().Output(1, logDefaultOutputˢ);
        }),
        new("log.Default.Panic", () => {
            log_package.Default().Panic(logDefaultPanicˢ);
        }),
        new("log.Default.Panicf", () => {
            log_package.Default().Panicf("log.Default.Panicf");
        }),
        new("log.Default.Panicln", () => {
            log_package.Default().Panicf("log.Default.Panicln");
        })
    ]);

    // calculate the line offset until the first test case
    var (_, _, line, ok) = runtime.Caller(0);
    if (!ok) {
        t.Fatalf("runtime.Caller failed");
    }
    line -= len(tests) + 3;

    foreach (var (i, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCallDepthConnection_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            // inside spawned test executable
            @string envVar = envVarᶜ;
            if (os.Getenv(envVar) == "1"u8) {
                var h = NewTextHandler(new os.FileжWriter(os.Stderr), Ꮡ(new HandlerOptions(
                    AddSource: true,
                    ReplaceAttr: (slice<@string> groups, global::go.log.slog_package.Attr a) => {
                        if ((a.Key == MessageKey || a.Key == SourceKey) && len(groups) == 0) {
                            return a;
                        }
                        return new Attr();
                    }
                )));
                SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(h)));
                log_package.SetFlags(log_package.Lshortfile);
                ttʗ1.log();
                os.Exit(1);
            }

            // spawn test executable
            var cmd = testenv.Command(new slog_test_package.testing_TжTB(tΔ1), ep,
                "-test.run=^"u8 + regexp.QuoteMeta(tΔ1.Name()) + "$"u8,
                testCount1ˢ);
            cmd.Env = builtin.append(cmd.Environ(), envVar + "=1");

            var (@out, errΔ1) = cmd.CombinedOutput();
            {
                var (_, okΔ1) = errors.AsType<ж<exec.ExitError>>(errΔ1); if (!okΔ1) {
                    tΔ1.Fatalf("expected exec.ExitError: %v", errΔ1);
                }
            }

            (_, var firstLine, errΔ1) = bufio.ScanLines(@out, true);
            if (errΔ1 != nil) {
                tΔ1.Fatalf("failed to split line: %v", errΔ1);
            }
            @string got = (@string)firstLine;

            @string want = fmt.Sprintf(
                @"msg=""logger_test.go:%d: %s"""u8,
                line + i, ttʗ1.name);
            if (got != want) {
                tΔ1.Errorf(
                    "output from %s() mismatch:\n\t got: %s\n\twant: %s",
                    ttʗ1.name, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string infoˢ3 => ref infoˢ3ᵇ;
private static readonly @string infoˢ3ᵇ = "Info"u8;
internal static ref readonly @string helloˢ2 => ref helloˢ2ᵇ;
private static readonly @string helloˢ2ᵇ = "hello"u8;
internal static ref readonly @string errorˢ3 => ref errorˢ3ᵇ;
private static readonly @string errorˢ3ᵇ = "Error"u8;
internal static ref readonly @string loggerInfoˢ => ref loggerInfoˢᵇ;
private static readonly @string loggerInfoˢᵇ = "logger.Info"u8;
internal static ref readonly @string loggerLogˢ => ref loggerLogˢᵇ;
private static readonly @string loggerLogˢᵇ = "logger.Log"u8;
internal static ref readonly @string pairsˢ => ref pairsˢᵇ;
private static readonly @string pairsˢᵇ = "2 pairs"u8;
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static ref readonly @string pairsDisabledInlineˢ => ref pairsDisabledInlineˢᵇ;
private static readonly @string pairsDisabledInlineˢᵇ = "2 pairs disabled inline"u8;
internal static ref readonly @string pairsDisabledˢ => ref pairsDisabledˢᵇ;
private static readonly @string pairsDisabledˢᵇ = "2 pairs disabled"u8;
internal static ref readonly @string kvsˢ => ref kvsˢᵇ;
private static readonly @string kvsˢᵇ = "9 kvs"u8;
internal static ref readonly @string pairsˢ2 => ref pairsˢ2ᵇ;
private static readonly @string pairsˢ2ᵇ = "pairs"u8;
internal static ref readonly @string attrs1ˢ => ref attrs1ˢᵇ;
private static readonly @string attrs1ˢᵇ = "attrs1"u8;
internal static ref readonly @string attrs3ˢ => ref attrs3ˢᵇ;
private static readonly @string attrs3ˢᵇ = "attrs3"u8;
internal static ref readonly @string attrs3Disabledˢ => ref attrs3Disabledˢᵇ;
private static readonly @string attrs3Disabledˢᵇ = "attrs3 disabled"u8;
internal static ref readonly @string attrs6ˢ => ref attrs6ˢᵇ;
private static readonly @string attrs6ˢᵇ = "attrs6"u8;
internal static ref readonly @string attrs9ˢ => ref attrs9ˢᵇ;
private static readonly @string attrs9ˢᵇ = "attrs9"u8;

public static void TestAlloc(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ctx = context.Background();
        var dl = New(new discardTestHandler());
        defer(SetDefault, Default(), ref ᒐ); // restore
        SetDefault(dl);

        t.Run(infoˢ3, (ж<testing.T> tΔ1) => {
            wantAllocs(tΔ1, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                Info(helloˢ2);
            });
        });
        t.Run(errorˢ3, (ж<testing.T> tΔ2) => {
            wantAllocs(tΔ2, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                go.log.slog_package.Error(helloˢ2);
            });
        });
        var dlʗ1 = dl;
        t.Run(loggerInfoˢ, (ж<testing.T> tΔ3) => {
            var dlʗ2 = dlʗ1;
            wantAllocs(tΔ3, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ2.Info(helloˢ2);
            });
        });
        var ctxʗ1 = ctx;
        var dlʗ3 = dl;
        t.Run(loggerLogˢ, (ж<testing.T> tΔ4) => {
            var ctxʗ2 = ctxʗ1;
            var dlʗ4 = dlʗ3;
            wantAllocs(tΔ4, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ4.Log(ctxʗ2, LevelDebug, helloˢ2);
            });
        });
        var dlʗ5 = dl;
        t.Run(pairsˢ, (ж<testing.T> tΔ5) => {
            @string s = abcˢ;
            nint i = 2000;
            var dlʗ6 = dlʗ5;
            wantAllocs(tΔ5, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ6.Info(helloˢ2,
                    (@string)"n", i,
                    (@string)"s", s);
            });
        });
        var ctxʗ3 = ctx;
        t.Run(pairsDisabledInlineˢ, (ж<testing.T> tΔ6) => {
            var l = New(DiscardHandler);
            @string s = abcˢ;
            nint i = 2000;
            var ctxʗ4 = ctxʗ3;
            var lʗ1 = l;
            wantAllocs(tΔ6, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                lʗ1.Log(ctxʗ4, LevelInfo, helloˢ2,
                    (@string)"n", i,
                    (@string)"s", s);
            });
        });
        var ctxʗ5 = ctx;
        t.Run(pairsDisabledˢ, (ж<testing.T> tΔ7) => {
            var l = New(DiscardHandler);
            @string s = abcˢ;
            nint i = 2000;
            var ctxʗ6 = ctxʗ5;
            var lʗ2 = l;
            wantAllocs(tΔ7, 0, () => {
                if (lʗ2.Enabled(ctxʗ6, LevelInfo)) {
                    lʗ2.Log(ctxʗ6, LevelInfo, helloˢ2,
                        (@string)"n", i,
                        (@string)"s", s);
                }
            });
        });
        var dlʗ7 = dl;
        t.Run(kvsˢ, (ж<testing.T> tΔ8) => {
            @string s = abcˢ;
            nint i = 2000;
            var d = time_package.ΔSecond;
            var dlʗ8 = dlʗ7;
            wantAllocs(tΔ8, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ8.Info(helloˢ2,
                    (@string)"n", i, (@string)"s", s, (@string)"d", d,
                    (@string)"n", i, (@string)"s", s, (@string)"d", d,
                    (@string)"n", i, (@string)"s", s, (@string)"d", d);
            });
        });
        var dlʗ9 = dl;
        t.Run(pairsˢ2, (ж<testing.T> tΔ9) => {
            var dlʗ10 = dlʗ9;
            wantAllocs(tΔ9, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ10.Info(""u8, errorˢ2, io.EOF);
            });
        });
        var ctxʗ7 = ctx;
        var dlʗ11 = dl;
        t.Run(attrs1ˢ, (ж<testing.T> tΔ10) => {
            var ctxʗ8 = ctxʗ7;
            var dlʗ12 = dlʗ11;
            wantAllocs(tΔ10, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ12.LogAttrs(ctxʗ8, LevelInfo, ""u8, Int("a", 1));
            });
            var ctxʗ9 = ctxʗ7;
            var dlʗ13 = dlʗ11;
            wantAllocs(tΔ10, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ13.LogAttrs(ctxʗ9, LevelInfo, ""u8, go.log.slog_package.Any(errorˢ2, io.EOF));
            });
        });
        var ctxʗ10 = ctx;
        var dlʗ14 = dl;
        t.Run(attrs3ˢ, (ж<testing.T> tΔ11) => {
            var ctxʗ11 = ctxʗ10;
            var dlʗ15 = dlʗ14;
            wantAllocs(tΔ11, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ15.LogAttrs(ctxʗ11, LevelInfo, helloˢ2, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Duration("c", time_package.ΔSecond));
            });
        });
        var ctxʗ12 = ctx;
        t.Run(attrs3Disabledˢ, (ж<testing.T> tΔ12) => {
            var logger = New(DiscardHandler);
            var ctxʗ13 = ctxʗ12;
            var loggerʗ1 = logger;
            wantAllocs(tΔ12, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                loggerʗ1.LogAttrs(ctxʗ13, LevelInfo, helloˢ2, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Duration("c", time_package.ΔSecond));
            });
        });
        var ctxʗ14 = ctx;
        var dlʗ16 = dl;
        t.Run(attrs6ˢ, (ж<testing.T> tΔ13) => {
            var ctxʗ15 = ctxʗ14;
            var dlʗ17 = dlʗ16;
            wantAllocs(tΔ13, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ17.LogAttrs(ctxʗ15, LevelInfo, helloˢ2,
                    Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Duration("c", time_package.ΔSecond),
                    Int("d", 1), go.log.slog_package.String("e", twoˢ), go.log.slog_package.Duration("f", time_package.ΔSecond));
            });
        });
        var ctxʗ16 = ctx;
        var dlʗ18 = dl;
        t.Run(attrs9ˢ, (ж<testing.T> tΔ14) => {
            var ctxʗ17 = ctxʗ16;
            var dlʗ19 = dlʗ18;
            wantAllocs(tΔ14, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ19.LogAttrs(ctxʗ17, LevelInfo, helloˢ2,
                    Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Duration("c", time_package.ΔSecond),
                    Int("d", 1), go.log.slog_package.String("e", twoˢ), go.log.slog_package.Duration("f", time_package.ΔSecond),
                    Int("d", 1), go.log.slog_package.String("e", twoˢ), go.log.slog_package.Duration("f", time_package.ΔSecond));
            });
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestSetAttrs_type /*dyn*/ {
    internal slice<any> args;
    internal slice<global::go.log.slog_package.Attr> want;
}

public static void TestSetAttrs(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestSetAttrs_type>([
        new(default, default),
        new(slice<any>([(@string)"a", (nint)1]), slice<global::go.log.slog_package.Attr>([Int("a", 1)])),
        new(slice<any>([(@string)"a", (nint)1, (@string)"b", twoˢ]), slice<global::go.log.slog_package.Attr>([Int("a", 1), go.log.slog_package.String("b", twoˢ)])),
        new(slice<any>([(@string)"a"]), slice<global::go.log.slog_package.Attr>([go.log.slog_package.String(badKey, "a")])),
        new(slice<any>([(@string)"a", (nint)1, (@string)"b"]), slice<global::go.log.slog_package.Attr>([Int("a", 1), go.log.slog_package.String(badKey, "b")])),
        new(slice<any>([(@string)"a", (nint)1, (nint)2, (nint)3]), slice<global::go.log.slog_package.Attr>([Int("a", 1), Int(badKey, 2), Int(badKey, 3)]))
    ])) {
        var r = NewRecord(new time_package.Time(), 0, ""u8, 0);
        r.Add(test.args.ꓸꓸꓸ);
        var got = attrsSlice(r);
        if (!attrsEqual(got, test.want)) {
            t.Errorf("%v:\ngot  %v\nwant %v", test.args, got, test.want);
        }
    }
}

public static partial void TestSetDefault(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Verify that setting the default to itself does not result in deadlock.
        var (ctx, cancel) = context.WithTimeout(context.Background(), time_package.ΔSecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        defer((io.Writer w) => {
            log_package.SetOutput(w);
        }, log_package.Writer(), ref ᒐ);
        log_package.SetOutput(io.Discard);
        var cancelʗ2 = cancel;
        goǃ(() => {
            Info("A");
            SetDefault(Default());
            Info("B");
            cancelʗ2();
        });
        ᐸꟷ(ctx.Done());
        {
            var err = ctx.Err(); if (!AreEqual(err, context.Canceled)) {
                t.Errorf("wanted canceled, got %v", err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestLogLoggerLevelForDefaultHandler_type /*dyn*/ {
    internal global::go.log.slog_package.ΔLevel logLevel;
    internal Actionꓸꓸꓸ<@string, any> logFn;
    internal @string want;
}

// Test defaultHandler minimum level without calling slog.SetDefault.
public static void TestLogLoggerLevelForDefaultHandler(ж<testing.T> t) {
    // Revert any changes to the default logger, flags, and level of log and slog.
    global::go.log.slog_package.ΔLevel currentLogLoggerLevel = ᏑlogLoggerLevel.Level();
    var currentLogWriter = log_package.Writer();
    nint currentLogFlags = log_package.Flags();
    var currentLogWriterʗ1 = currentLogWriter;
    t.Cleanup(() => {
        ᏑlogLoggerLevel.Set(currentLogLoggerLevel);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });

    ref var logBuf = ref heap(new bytes.Buffer(), out var ᏑlogBuf);
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf));
    log_package.SetFlags(0);

    foreach (var (_, test) in slice<TestLogLoggerLevelForDefaultHandler_type>([
        new(LevelDebug, Debug, "DEBUG a"),
        new(LevelDebug, Info, "INFO a"),
        new(LevelInfo, Debug, ""),
        new(LevelInfo, Info, "INFO a")
    ])) {
        SetLogLoggerLevel(test.logLevel);
        test.logFn("a");
        checkLogOutput(t, ᏑlogBuf.String(), test.want);
        logBuf.Reset();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string levelErrorMsgErrorˢ => ref levelErrorMsgErrorˢᵇ;
private static readonly @string levelErrorMsgErrorˢᵇ = @"level=ERROR msg=error"u8;

// Test handlerWriter minimum level by calling slog.SetDefault.
public static void TestLogLoggerLevelForHandlerWriter(ж<testing.T> t) {
    var removeTime = (slice<@string> _, global::go.log.slog_package.Attr a) => {
        if (a.Key == TimeKey) {
            return new Attr();
        }
        return a;
    };

    // Revert any changes to the default logger. This is important because other
    // tests might change the default logger using SetDefault. Also ensure we
    // restore the default logger at the end of the test.
    var currentLogger = Default();
    global::go.log.slog_package.ΔLevel currentLogLoggerLevel = ᏑlogLoggerLevel.Level();
    var currentLogWriter = log_package.Writer();
    nint currentFlags = log_package.Flags();
    var currentLogWriterʗ1 = currentLogWriter;
    var currentLoggerʗ1 = currentLogger;
    t.Cleanup(() => {
        SetDefault(currentLoggerʗ1);
        ᏑlogLoggerLevel.Set(currentLogLoggerLevel);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentFlags);
    });

    ref var logBuf = ref heap(new bytes.Buffer(), out var ᏑlogBuf);
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf));
    log_package.SetFlags(0);
    SetLogLoggerLevel(LevelError);
    SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf), Ꮡ(new HandlerOptions(ReplaceAttr: removeTime))))));
    log_package.Print(errorˢ2);
    checkLogOutput(t, ᏑlogBuf.String(), levelErrorMsgErrorˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string levelErrorMsgMsgErrEofA1ˢ => ref levelErrorMsgMsgErrEofA1ˢᵇ;
private static readonly @string levelErrorMsgMsgErrEofA1ˢᵇ = @"level=ERROR msg=msg err=EOF a=1"u8;
internal static ref readonly @string levelErrorMsgMsgErrEofˢ => ref levelErrorMsgMsgErrEofˢᵇ;
private static readonly @string levelErrorMsgMsgErrEofˢᵇ = @"level=ERROR msg=msg err=EOF !BADKEY=a"u8;

public static void TestLoggerError(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    var removeTime = (slice<@string> _, global::go.log.slog_package.Attr a) => {
        if (a.Key == TimeKey) {
            return new Attr();
        }
        return a;
    };
    var l = New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡ(new HandlerOptions(ReplaceAttr: removeTime)))));
    l.Error(msgˢ, errˢ, io.EOF, (@string)"a", (nint)1);
    checkLogOutput(t, Ꮡbuf.String(), levelErrorMsgMsgErrEofA1ˢ);
    buf.Reset();
    // use local var 'args' to defeat vet check
    var args = slice<any>([errˢ, io.EOF, (@string)"a"]);
    l.Error("msg"u8, args.ꓸꓸꓸ);
    checkLogOutput(t, Ꮡbuf.String(), levelErrorMsgMsgErrEofˢ);
}

public static void TestNewLogLogger(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var h = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil);
    var ll = NewLogLogger(new global::go.log.slog_package.TextHandlerжΔHandler(h), LevelWarn);
    ll.Print(helloˢ2);
    checkLogOutput(t, Ꮡbuf.String(), "time=" + textTimeRE + @" level=WARN msg=hello");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wantedReceiverDidnTGetItˢ = (@string)"wanted receiver, didn't get it"u8;

public static void TestLoggerNoOps(ж<testing.T> t) {
    var l = Default();
    if (l.With() != l) {
        t.Error(wantedReceiverDidnTGetItˢ);
    }
    if (With() != l) {
        t.Error(wantedReceiverDidnTGetItˢ);
    }
    if (l.WithGroup(""u8) != l) {
        t.Error(wantedReceiverDidnTGetItˢ);
    }
}

internal partial struct TestContext_type /*dyn*/ {
    internal Actionꓸꓸꓸ<context.Context, @string, any> f;
    internal global::go.log.slog_package.ΔLevel wantLevel;
}

public static void TestContext(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Verify that the context argument to log output methods is passed to the handler.
        // Also check the level.
        var h = Ꮡ(new captureHandler());
        var l = New(new slog_internal_test_package.captureHandlerжΔHandler(h));
        defer(SetDefault, Default(), ref ᒐ); // restore
        SetDefault(l);

        foreach (var (_, test) in slice<TestContext_type>([
            new(l.DebugContext, LevelDebug),
            new(l.InfoContext, LevelInfo),
            new(l.WarnContext, LevelWarn),
            new(l.ErrorContext, LevelError),
            new(DebugContext, LevelDebug),
            new(InfoContext, LevelInfo),
            new(WarnContext, LevelWarn),
            new(ErrorContext, LevelError)
        ])) {
            h.clear();
            var ctx = context.WithValue(context.Background(), "L", test.wantLevel);

            test.f(ctx, "msg");
            {
                var gv = h.ctx.Value((@string)"L"); if (!AreEqual(gv, test.wantLevel) || h.r.Level != test.wantLevel) {
                    t.Errorf("got context value %v, level %s; want %s for both", gv, h.r.Level, test.wantLevel);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void checkLogOutput(ж<testing.T> t, @string got, @string wantRegexp) {
    t.Helper();
    got = clean(got);
    wantRegexp = "^"u8 + wantRegexp + "$"u8;
    var (matched, err) = regexp.MatchString(wantRegexp, got);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!matched) {
        t.Errorf("\ngot  %s\nwant %s", got, wantRegexp);
    }
}

// clean prepares log output for comparison.
internal static @string clean(@string s) {
    if (len(s) > 0 && s[len(s) - 1] == '\n') {
        s = s.slice(0, len(s) - 1);
    }
    return strings.ReplaceAll(s, "\n", "~");
}

internal partial struct captureHandler {
    internal sync.Mutex mu;
    internal context.Context ctx;
    internal global::go.log.slog_package.Record r;
    internal slice<global::go.log.slog_package.Attr> attrs;
    internal slice<@string> groups;
}

internal static error Handle(this ж<captureHandler> Ꮡh, context.Context ctx, global::go.log.slog_package.Record r) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        r = r.ΔClone();
        ref var h = ref Ꮡh.DerefOrNull();

        h.mu.Lock();
        ᒐd1 = true;
        h.ctx = ctx;
        h.r = r.ΔClone();
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡh.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static bool Enabled(this ref captureHandler _Δp0, context.Context _Δp1, global::go.log.slog_package.ΔLevel _Δp2) {
    return true;
}

internal static global::go.log.slog_package.ΔHandler WithAttrs(this ж<captureHandler> Ꮡc, slice<global::go.log.slog_package.Attr> @as) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        ref var c2 = ref heap(new captureHandler(), out var Ꮡc2);
        c2.r = c.r.ΔClone();
        c2.groups = c.groups;
        c2.attrs = concat(c.attrs, @as);
        return new slog_internal_test_package.captureHandlerжΔHandler(Ꮡc2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static global::go.log.slog_package.ΔHandler WithGroup(this ж<captureHandler> Ꮡc, @string name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        ref var c2 = ref heap(new captureHandler(), out var Ꮡc2);
        c2.r = c.r.ΔClone();
        c2.attrs = c.attrs;
        c2.groups = builtin.append(slices.Clip<slice<@string>, @string>(c.groups), name);
        return new slog_internal_test_package.captureHandlerжΔHandler(Ꮡc2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void clear(this ж<captureHandler> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        c.ctx = default;
        c.r = new Record();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal partial struct discardTestHandler {
    internal slice<global::go.log.slog_package.Attr> attrs;
}

internal static bool Enabled(this discardTestHandler d, context.Context _Δp1, global::go.log.slog_package.ΔLevel _Δp2) {
    return true;
}

internal static error Handle(this discardTestHandler _Δp0, context.Context _Δp1, global::go.log.slog_package.Record _Δp2) {
    return default;
}

internal static global::go.log.slog_package.ΔHandler WithAttrs(this discardTestHandler d, slice<global::go.log.slog_package.Attr> @as) {
    d.attrs = concat(d.attrs, @as);
    return d;
}

internal static global::go.log.slog_package.ΔHandler WithGroup(this discardTestHandler h, @string name) {
    return h;
}

// concat returns a new slice with the elements of s1 followed
// by those of s2. The slice has no additional capacity.
internal static slice<T> concat<T>(slice<T> s1, slice<T> s2) {
    var s = new slice<T>(len(s1) + len(s2));
    copy(s, s1);
    copy(s.slice(len(s1)), s2);
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noAttrsˢ => ref noAttrsˢᵇ;
private static readonly @string noAttrsˢᵇ = "no attrs"u8;
internal static ref readonly @string attrsParallelˢ => ref attrsParallelˢᵇ;
private static readonly @string attrsParallelˢᵇ = "attrs-parallel"u8;
internal static ref readonly @string keysValuesˢ => ref keysValuesˢᵇ;
private static readonly @string keysValuesˢᵇ = "keys-values"u8;
internal static ref readonly @string withContextˢ => ref withContextˢᵇ;
private static readonly @string withContextˢᵇ = "WithContext"u8;
internal static ref readonly @string withContextParallelˢ => ref withContextParallelˢᵇ;
private static readonly @string withContextParallelˢᵇ = "WithContext-parallel"u8;

// This is a simple benchmark. See the benchmarks subdirectory for more extensive ones.
public static void BenchmarkNopLog(ж<testing.B> b) {
    var ctx = context.Background();
    var l = New(new slog_internal_test_package.captureHandlerжΔHandler(Ꮡ(new captureHandler())));
    var ctxʗ1 = ctx;
    var lʗ1 = l;
    b.Run(noAttrsˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            lʗ1.LogAttrs(ctxʗ1, LevelInfo, msgˢ);
        }
    });
    var ctxʗ2 = ctx;
    var lʗ2 = l;
    b.Run(attrsˢ, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            lʗ2.LogAttrs(ctxʗ2, LevelInfo, msgˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Bool("c", true));
        }
    });
    var ctxʗ3 = ctx;
    var lʗ3 = l;
    b.Run(attrsParallelˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        var ctxʗ4 = ctxʗ3;
        var lʗ4 = lʗ3;
        bΔ3.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                lʗ4.LogAttrs(ctxʗ4, LevelInfo, msgˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Bool("c", true));
            }
        });
    });
    var ctxʗ5 = ctx;
    var lʗ5 = l;
    b.Run(keysValuesˢ, (ж<testing.B> bΔ4) => {
        bΔ4.ReportAllocs();
        for (nint i = 0; i < (~bΔ4).N; i++) {
            lʗ5.Log(ctxʗ5, LevelInfo, msgˢ, (@string)"a", (nint)1, (@string)"b", twoˢ, (@string)"c", true);
        }
    });
    var ctxʗ6 = ctx;
    var lʗ6 = l;
    b.Run(withContextˢ, (ж<testing.B> bΔ5) => {
        bΔ5.ReportAllocs();
        for (nint i = 0; i < (~bΔ5).N; i++) {
            lʗ6.LogAttrs(ctxʗ6, LevelInfo, msg2ˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Bool("c", true));
        }
    });
    var ctxʗ7 = ctx;
    var lʗ7 = l;
    b.Run(withContextParallelˢ, (ж<testing.B> bΔ6) => {
        bΔ6.ReportAllocs();
        var ctxʗ8 = ctxʗ7;
        var lʗ8 = lʗ7;
        bΔ6.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                lʗ8.LogAttrs(ctxʗ8, LevelInfo, msgˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ), go.log.slog_package.Bool("c", true));
            }
        });
    });
}

// callerPC returns the program counter at the given stack depth.
internal static partial uintptr callerPC(nint depth) {
    array<uintptr> pcs = new(1);
    runtime.Callers(depth, pcs[..]);
    return pcs[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInRaceAsanˢ = (@string)"skipping test in race, asan, and msan modes"u8;

internal static void wantAllocs(ж<testing.T> t, nint want, Action f) {
    if (race.Enabled || asan.Enabled || msan.Enabled) {
        t.Skip(skippingTestInRaceAsanˢ);
    }
    testenv.SkipIfOptimizationOff(new slog_test_package.testing_TжTB(t));
    t.Helper();
    nint got = (nint)testing.AllocsPerRun(5, f);
    if (got != want) {
        t.Errorf("got %d allocs, want %d", got, want);
    }
}

// panicTextAndJsonMarshaler is a type that panics in MarshalText and MarshalJSON.
internal partial struct panicTextAndJsonMarshaler {
    internal any msg;
}

internal static (slice<byte>, error) MarshalText(this panicTextAndJsonMarshaler p) {
    throw panic(p.msg);
}

internal static (slice<byte>, error) MarshalJSON(this panicTextAndJsonMarshaler p) {
    throw panic(p.msg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object panickingˢ = (@string)"panicking"u8;

internal partial struct TestPanics_type /*dyn*/ {
    internal any @in;
    internal @string @out;
}

public static void TestPanics(ж<testing.T> t) {
    // Revert any changes to the default logger. This is important because other
    // tests might change the default logger using SetDefault. Also ensure we
    // restore the default logger at the end of the test.
    var currentLogger = Default();
    var currentLogWriter = log_package.Writer();
    nint currentLogFlags = log_package.Flags();
    var currentLogWriterʗ1 = currentLogWriter;
    var currentLoggerʗ1 = currentLogger;
    t.Cleanup(() => {
        SetDefault(currentLoggerʗ1);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });

    ref var logBuf = ref heap(new bytes.Buffer(), out var ᏑlogBuf);
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));

    SetDefault(New(new global::go.log.slog_package.defaultHandlerжΔHandler(newDefaultHandler(loginternal.DefaultOutput))));
    foreach (var (_, pt) in slice<TestPanics_type>([
        new((ж<panicTextAndJsonMarshaler>)nil, @"logger_test.go:\d+: INFO msg p=<nil>"),
        new(new panicTextAndJsonMarshaler(io.ErrUnexpectedEOF), @"logger_test.go:\d+: INFO msg p=""!PANIC: unexpected EOF"""),
        new(new panicTextAndJsonMarshaler(panickingˢ), @"logger_test.go:\d+: INFO msg p=""!PANIC: panicking"""),
        new(new panicTextAndJsonMarshaler((nint)42), @"logger_test.go:\d+: INFO msg p=""!PANIC: 42""")
    ])) {
        Info(msgˢ, "p", pt.@in);
        checkLogOutput(t, ᏑlogBuf.String(), pt.@out);
        logBuf.Reset();
    }

    SetDefault(New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf), nil))));
    foreach (var (_, pt) in slice<TestPanics_type>([
        new((ж<panicTextAndJsonMarshaler>)nil, @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":null}"),
        new(new panicTextAndJsonMarshaler(io.ErrUnexpectedEOF), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: unexpected EOF""}"),
        new(new panicTextAndJsonMarshaler(panickingˢ), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: panicking""}"),
        new(new panicTextAndJsonMarshaler((nint)42), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: 42""}")
    ])) {
        Info(msgˢ, "p", pt.@in);
        checkLogOutput(t, ᏑlogBuf.String(), pt.@out);
        logBuf.Reset();
    }
}

} // end slog_internal_test_package
