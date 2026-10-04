// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using asan = go.@internal.asan_package;
using msan = go.@internal.msan_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using log = log_package;
using loginternal = go.log.internal_package;
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
using path;
using static go.log.slog_package;

partial class slog_internal_test_package {

// textTimeRE is a regexp to match log timestamps for Text handler.
// This is RFC3339Nano with the fixed 3 digit sub-second precision.
internal static readonly @string textTimeRE = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}(Z|[+-]\d{2}:\d{2})"u8;

// jsonTimeRE is a regexp to match log timestamps for Text handler.
// This is RFC3339Nano with an arbitrary sub-second precision.
internal static readonly @string jsonTimeRE = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string levelInfoMsgMsgA1B2ˢ = @"level=INFO msg=msg a=1 b=2"u8;
internal static readonly @string levelWarnMsgWDur3sˢ = @"level=WARN msg=w dur=3s"u8;
internal static readonly @string levelErrorMsgBadA1ˢ = @"level=ERROR msg=bad a=1"u8;
internal static readonly @string levelWarn1MsgWA1BTwoˢ = @"level=WARN\+1 msg=w a=1 b=two"u8;
internal static readonly @string levelInfo1MsgABCA1BTwoˢ = @"level=INFO\+1 msg=""a b c"" a=1 b=two"u8;
internal static readonly @string levelInfoMsgInfoAI1ˢ = @"level=INFO msg=info a.i=1"u8;

public static void TestLogTextHandler(ж<testing.T> Ꮡt) {
    var ctx = context.Background();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var l = New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil)));
    void check(@string want) {
        Ꮡt.Helper();
        if (want != ""u8) {
            want = "time=" + textTimeRE + " " + want;
        }
        checkLogOutput(Ꮡt, Ꮡbuf.String(), want);
        Ꮡbuf.Value.Reset();
    }
    l.Info(msgˢ, (@string)"a"u8, (nint)(1), (@string)"b"u8, (nint)(2));
    check(levelInfoMsgMsgA1B2ˢ);
    // By default, debug messages are not printed.
    l.Debug("bg"u8, Int("a"u8, 1), (@string)"b"u8, (nint)(2));
    check(""u8);
    l.Warn("w"u8, go.log.slog_package.Duration(durˢ, (time.Duration)(3000000000L)));
    check(levelWarnMsgWDur3sˢ);
    l.Error(badˢ, (@string)"a"u8, (nint)(1));
    check(levelErrorMsgBadA1ˢ);
    l.Log(ctx, LevelWarn + 1, "w"u8, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ));
    check(levelWarn1MsgWA1BTwoˢ);
    l.LogAttrs(ctx, LevelInfo + 1, aBCˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ));
    check(levelInfo1MsgABCA1BTwoˢ);
    l.Info(infoˢ2, (@string)"a"u8, new global::go.log.slog_package.Attr[]{Int("i"u8, 1)}.slice());
    check(levelInfoMsgInfoAI1ˢ);
    l.Info(infoˢ2, (@string)"a"u8, GroupValue(Int("i"u8, 1)));
    check(levelInfoMsgInfoAI1ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string loggerTestGoDInfoMsgA1ˢ = @"logger_test.go:\d+: INFO msg a=1"u8;
internal static readonly @string loggerTestGoDInfoMsgPNilˢ = @"logger_test.go:\d+: INFO msg p=<nil>"u8;
internal static readonly @string loggerTestGoDInfoMsgRNilˢ = @"logger_test.go:\d+: INFO msg r=<nil>"u8;
internal static readonly @string loggerTestGoDWarnMsgB2ˢ = @"logger_test.go:\d+: WARN msg b=2"u8;
internal static readonly object errˢ = (@string)"err"u8;
internal static readonly @string loggerTestGoDErrorMsgErrˢ = @"logger_test.go:\d+: ERROR msg err=EOF c=3"u8;
internal static readonly @string wrapDefaultHandlerˢ = "wrap default handler"u8;
internal static readonly @string loggerTestGoDInfoMsgD4ˢ = @"logger_test.go:\d+: INFO msg d=4"u8;
internal static readonly @string msg2ˢ = "msg2"u8;
internal static readonly object shouldNotAppearˢ = (@string)"should not appear"u8;
internal static readonly object msg3ˢ = (@string)"msg3"u8;
internal static readonly @string loggerTestGoDMsg3ˢ = @"logger_test.go:\d+: msg3"u8;

public static void TestConnections(ж<testing.T> Ꮡt) {
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
    Ꮡt.Cleanup(() => {
        SetDefault(currentLoggerʗ1);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });
    // The default slog.Logger's handler uses the log package's default output.
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(Ꮡlogbuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));
    Info(msgˢ, (@string)"a"u8, (nint)(1));
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDInfoMsgA1ˢ);
    logbuf.Reset();
    Info(msgˢ, (@string)"p"u8, (any)(default!));
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDInfoMsgPNilˢ);
    logbuf.Reset();
    ж<regexp.Regexp> r = default!;
    Info(msgˢ, (@string)"r"u8, r.OrTypedNil());
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDInfoMsgRNilˢ);
    logbuf.Reset();
    Warn(msgˢ, (@string)"b"u8, (nint)(2));
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDWarnMsgB2ˢ);
    logbuf.Reset();
    go.log.slog_package.Error(msgˢ, errˢ, io.EOF, (@string)"c"u8, (nint)(3));
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDErrorMsgErrˢ);
    // Levels below Info are not printed.
    logbuf.Reset();
    Debug(msgˢ, (@string)"c"u8, (nint)(3));
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), ""u8);
    Ꮡt.Run(wrapDefaultHandlerˢ, (ж<testing.T> tΔ1) => {
        // It should be possible to wrap the default handler and get the right output.
        // This works because the default handler uses the pc in the Record
        // to get the source line, rather than a call depth.
        var logger = New(new wrappingHandler(Default().Handler()));
        logger.Info(msgˢ, (@string)"d"u8, (nint)(4));
        checkLogOutput(tΔ1, Ꮡlogbuf.String(), loggerTestGoDInfoMsgD4ˢ);
    });
    // Once slog.SetDefault is called, the direction is reversed: the default
    // log.Logger's output goes through the handler.
    SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡslogbuf), Ꮡ(new HandlerOptions(AddSource: true))))));
    log_package.Print(msg2ˢ);
    checkLogOutput(Ꮡt, Ꮡslogbuf.String(), "time=" + textTimeRE + @" level=INFO source=.*logger_test.go:\d{3}""? msg=msg2");
    // The default log.Logger always outputs at Info level.
    slogbuf.Reset();
    SetDefault(New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡslogbuf), Ꮡ(new HandlerOptions(Level: LevelWarn))))));
    log_package.Print(shouldNotAppearˢ);
    {
        @string got = Ꮡslogbuf.String(); if (got != ""u8) {
            Ꮡt.Errorf("got %q, want empty"u8, got);
        }
    }
    // Setting log's output again breaks the connection.
    logbuf.Reset();
    slogbuf.Reset();
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(Ꮡlogbuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));
    log_package.Print(msg3ˢ);
    checkLogOutput(Ꮡt, Ꮡlogbuf.String(), loggerTestGoDMsg3ˢ);
    {
        @string got = Ꮡslogbuf.String(); if (got != ""u8) {
            Ꮡt.Errorf("got %q, want empty"u8, got);
        }
    }
}

[GoType] internal partial struct wrappingHandler {
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

public static void TestAttrs(ж<testing.T> Ꮡt) {
    void check(slice<global::go.log.slog_package.Attr> got, params Span<global::go.log.slog_package.Attr> wantʗp) {
        var want = wantʗp.slice();
        Ꮡt.Helper();
        if (!attrsEqual(got, want)) {
            Ꮡt.Errorf("got %v, want %v"u8, got, want);
        }
    }
    var l1 = New(new slog_internal_test_package.captureHandlerжΔHandler(Ꮡ(new captureHandler(nil)))).With((@string)"a"u8, (nint)(1));
    var l2 = New(l1.Handler()).With((@string)"b"u8, (nint)(2));
    l2.Info("m"u8, (@string)"c"u8, (nint)(3));
    var h = l2.Handler()._<ж<captureHandler>>();
    check((~h).attrs, Int("a"u8, 1), Int("b"u8, 2));
    check(attrsSlice((~h).r), Int("c"u8, 3));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantFuncᶜ = "log/slog.TestCallDepth"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantFileᶜ = "logger_test.go"u8;

public static void TestCallDepth(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ctx = context.Background();
        var h = Ꮡ(new captureHandler(nil));
        nint startLine = default!;
        var hʗ1 = h;
        void check(nint count) {
            Ꮡt.Helper();
            @string wantFunc = wantFuncᶜ;
            @string wantFile = wantFileᶜ;
            nint wantLine = startLine + count * 2;
            var got = (~hʗ1).r.source();
            @string gotFile = filepath.Base((~got).File);
            if ((~got).Function != wantFunc || gotFile != wantFile || (~got).Line != wantLine) {
                Ꮡt.Errorf("got (%s, %s, %d), want (%s, %s, %d)"u8,
                    (~got).Function, gotFile, (~got).Line, wantFunc, wantFile, wantLine);
            }
        }
        defer(SetDefault, Default(), ref ᒐ); // restore
        var logger = New(new slog_internal_test_package.captureHandlerжΔHandler(h));
        SetDefault(logger);
        // Calls to check must be one line apart.
        // Determine line where calls start.
        var (f, _) = runtime.CallersFrames(new uintptr[]{callerPC(2)}.slice()).Next();
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
internal static readonly @string infoˢ3 = "Info"u8;
internal static readonly @string helloˢ = "hello"u8;
internal static readonly @string errorˢ2 = "Error"u8;
internal static readonly @string loggerInfoˢ = "logger.Info"u8;
internal static readonly @string loggerLogˢ = "logger.Log"u8;
internal static readonly @string pairsˢ = "2 pairs"u8;
internal static readonly @string abcˢ = "abc"u8;
internal static readonly @string pairsDisabledInlineˢ = "2 pairs disabled inline"u8;
internal static readonly @string pairsDisabledˢ = "2 pairs disabled"u8;
internal static readonly @string kvsˢ = "9 kvs"u8;
internal static readonly @string pairsˢ2 = "pairs"u8;
internal static readonly @string errorˢ3 = "error"u8;
internal static readonly @string attrs1ˢ = "attrs1"u8;
internal static readonly @string attrs3ˢ = "attrs3"u8;
internal static readonly @string attrs3Disabledˢ = "attrs3 disabled"u8;
internal static readonly @string attrs6ˢ = "attrs6"u8;
internal static readonly @string attrs9ˢ = "attrs9"u8;

public static void TestAlloc(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ctx = context.Background();
        var dl = New(new discardTestHandler(nil));
        defer(SetDefault, Default(), ref ᒐ); // restore
        SetDefault(dl);
        Ꮡt.Run(infoˢ3, (ж<testing.T> tΔ1) => {
            wantAllocs(tΔ1, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                Info(helloˢ);
            });
        });
        Ꮡt.Run(errorˢ2, (ж<testing.T> tΔ2) => {
            wantAllocs(tΔ2, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                go.log.slog_package.Error(helloˢ);
            });
        });
        var dlʗ1 = dl;
        Ꮡt.Run(loggerInfoˢ, (ж<testing.T> tΔ3) => {
            var dlʗ2 = dlʗ1;
            wantAllocs(tΔ3, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ2.Info(helloˢ);
            });
        });
        var ctxʗ1 = ctx;
        var dlʗ3 = dl;
        Ꮡt.Run(loggerLogˢ, (ж<testing.T> tΔ4) => {
            var ctxʗ2 = ctxʗ1;
            var dlʗ4 = dlʗ3;
            wantAllocs(tΔ4, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ4.Log(ctxʗ2, LevelDebug, helloˢ);
            });
        });
        var dlʗ5 = dl;
        Ꮡt.Run(pairsˢ, (ж<testing.T> tΔ5) => {
            @string s = abcˢ;
            nint i = 2000;
            var dlʗ6 = dlʗ5;
            wantAllocs(tΔ5, 2, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ6.Info(helloˢ,
                    (@string)"n"u8, i,
                    (@string)"s"u8, s);
            });
        });
        var ctxʗ3 = ctx;
        Ꮡt.Run(pairsDisabledInlineˢ, (ж<testing.T> tΔ6) => {
            var l = New(DiscardHandler);
            @string s = abcˢ;
            nint i = 2000;
            var ctxʗ4 = ctxʗ3;
            var lʗ1 = l;
            wantAllocs(tΔ6, 2, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                lʗ1.Log(ctxʗ4, LevelInfo, helloˢ,
                    (@string)"n"u8, i,
                    (@string)"s"u8, s);
            });
        });
        var ctxʗ5 = ctx;
        Ꮡt.Run(pairsDisabledˢ, (ж<testing.T> tΔ7) => {
            var l = New(DiscardHandler);
            @string s = abcˢ;
            nint i = 2000;
            var ctxʗ6 = ctxʗ5;
            var lʗ2 = l;
            wantAllocs(tΔ7, 0, () => {
                if (lʗ2.Enabled(ctxʗ6, LevelInfo)) {
                    lʗ2.Log(ctxʗ6, LevelInfo, helloˢ,
                        (@string)"n"u8, i,
                        (@string)"s"u8, s);
                }
            });
        });
        var dlʗ7 = dl;
        Ꮡt.Run(kvsˢ, (ж<testing.T> tΔ8) => {
            @string s = abcˢ;
            nint i = 2000;
            var d = time_package.ΔSecond;
            var dlʗ8 = dlʗ7;
            wantAllocs(tΔ8, 10, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ8.Info(helloˢ,
                    (@string)"n"u8, i, (@string)"s"u8, s, (@string)"d"u8, d,
                    (@string)"n"u8, i, (@string)"s"u8, s, (@string)"d"u8, d,
                    (@string)"n"u8, i, (@string)"s"u8, s, (@string)"d"u8, d);
            });
        });
        var dlʗ9 = dl;
        Ꮡt.Run(pairsˢ2, (ж<testing.T> tΔ9) => {
            var dlʗ10 = dlʗ9;
            wantAllocs(tΔ9, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ10.Info(""u8, errorˢ3, io.EOF);
            });
        });
        var ctxʗ7 = ctx;
        var dlʗ11 = dl;
        Ꮡt.Run(attrs1ˢ, (ж<testing.T> tΔ10) => {
            var ctxʗ8 = ctxʗ7;
            var dlʗ12 = dlʗ11;
            wantAllocs(tΔ10, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ12.LogAttrs(ctxʗ8, LevelInfo, ""u8, Int("a"u8, 1));
            });
            var ctxʗ9 = ctxʗ7;
            var dlʗ13 = dlʗ11;
            wantAllocs(tΔ10, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ13.LogAttrs(ctxʗ9, LevelInfo, ""u8, go.log.slog_package.Any(errorˢ3, io.EOF));
            });
        });
        var ctxʗ10 = ctx;
        var dlʗ14 = dl;
        Ꮡt.Run(attrs3ˢ, (ж<testing.T> tΔ11) => {
            var ctxʗ11 = ctxʗ10;
            var dlʗ15 = dlʗ14;
            wantAllocs(tΔ11, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ15.LogAttrs(ctxʗ11, LevelInfo, helloˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Duration("c"u8, time_package.ΔSecond));
            });
        });
        var ctxʗ12 = ctx;
        Ꮡt.Run(attrs3Disabledˢ, (ж<testing.T> tΔ12) => {
            var logger = New(DiscardHandler);
            var ctxʗ13 = ctxʗ12;
            var loggerʗ1 = logger;
            wantAllocs(tΔ12, 0, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                loggerʗ1.LogAttrs(ctxʗ13, LevelInfo, helloˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Duration("c"u8, time_package.ΔSecond));
            });
        });
        var ctxʗ14 = ctx;
        var dlʗ16 = dl;
        Ꮡt.Run(attrs6ˢ, (ж<testing.T> tΔ13) => {
            var ctxʗ15 = ctxʗ14;
            var dlʗ17 = dlʗ16;
            wantAllocs(tΔ13, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ17.LogAttrs(ctxʗ15, LevelInfo, helloˢ,
                    Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Duration("c"u8, time_package.ΔSecond),
                    Int("d"u8, 1), go.log.slog_package.String("e"u8, twoˢ), go.log.slog_package.Duration("f"u8, time_package.ΔSecond));
            });
        });
        var ctxʗ16 = ctx;
        var dlʗ18 = dl;
        Ꮡt.Run(attrs9ˢ, (ж<testing.T> tΔ14) => {
            var ctxʗ17 = ctxʗ16;
            var dlʗ19 = dlʗ18;
            wantAllocs(tΔ14, 1, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                dlʗ19.LogAttrs(ctxʗ17, LevelInfo, helloˢ,
                    Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Duration("c"u8, time_package.ΔSecond),
                    Int("d"u8, 1), go.log.slog_package.String("e"u8, twoˢ), go.log.slog_package.Duration("f"u8, time_package.ΔSecond),
                    Int("d"u8, 1), go.log.slog_package.String("e"u8, twoˢ), go.log.slog_package.Duration("f"u8, time_package.ΔSecond));
            });
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestSetAttrs_type {
    internal slice<any> args;
    internal slice<global::go.log.slog_package.Attr> want;
}

public static void TestSetAttrs(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestSetAttrs_type[]{
        new(default!, default!),
        new(new any[]{(@string)"a"u8, (nint)(1)}.slice(), new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()),
        new(new any[]{(@string)"a"u8, (nint)(1), (@string)"b"u8, (@string)"two"u8}.slice(), new global::go.log.slog_package.Attr[]{Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ)}.slice()),
        new(new any[]{(@string)"a"u8}.slice(), new global::go.log.slog_package.Attr[]{go.log.slog_package.String(badKey, "a"u8)}.slice()),
        new(new any[]{(@string)"a"u8, (nint)(1), (@string)"b"u8}.slice(), new global::go.log.slog_package.Attr[]{Int("a"u8, 1), go.log.slog_package.String(badKey, "b"u8)}.slice()),
        new(new any[]{(@string)"a"u8, (nint)(1), (nint)(2), (nint)(3)}.slice(), new global::go.log.slog_package.Attr[]{Int("a"u8, 1), Int(badKey, 2), Int(badKey, 3)}.slice())
    }.slice()) {
        var r = NewRecord(new time_package.Time(nil), 0, ""u8, 0);
        r.Add(test.args.ꓸꓸꓸ);
        var got = attrsSlice(r);
        if (!attrsEqual(got, test.want)) {
            Ꮡt.Errorf("%v:\ngot  %v\nwant %v"u8, test.args, got, test.want);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestSetDefault(ж<testing.T> Ꮡt) {
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
            Info("A"u8);
            SetDefault(Default());
            Info("B"u8);
            cancelʗ2();
        });
        ᐸꟷ(ctx.Done());
        {
            var err = ctx.Err(); if (!AreEqual(err, context.Canceled)) {
                Ꮡt.Errorf("wanted canceled, got %v"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestLogLoggerLevelForDefaultHandler_type {
    internal global::go.log.slog_package.ΔLevel logLevel;
    internal Actionꓸꓸꓸ<@string, any> logFn;
    internal @string want;
}

// Test defaultHandler minimum level without calling slog.SetDefault.
public static void TestLogLoggerLevelForDefaultHandler(ж<testing.T> Ꮡt) {
    // Revert any changes to the default logger, flags, and level of log and slog.
    global::go.log.slog_package.ΔLevel currentLogLoggerLevel = ᏑlogLoggerLevel.Level();
    var currentLogWriter = log_package.Writer();
    nint currentLogFlags = log_package.Flags();
    var currentLogWriterʗ1 = currentLogWriter;
    Ꮡt.Cleanup(() => {
        ᏑlogLoggerLevel.Set(currentLogLoggerLevel);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });
    ref var logBuf = ref heap(new bytes.Buffer(), out var ᏑlogBuf);
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf));
    log_package.SetFlags(0);
    foreach (var (_, test) in new TestLogLoggerLevelForDefaultHandler_type[]{
        new(LevelDebug, Debug, "DEBUG a"u8),
        new(LevelDebug, Info, "INFO a"u8),
        new(LevelInfo, Debug, ""u8),
        new(LevelInfo, Info, "INFO a"u8)
    }.slice()) {
        SetLogLoggerLevel(test.logLevel);
        test.logFn("a"u8);
        checkLogOutput(Ꮡt, ᏑlogBuf.String(), test.want);
        logBuf.Reset();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string levelErrorMsgErrorˢ = @"level=ERROR msg=error"u8;

// Test handlerWriter minimum level by calling slog.SetDefault.
public static void TestLogLoggerLevelForHandlerWriter(ж<testing.T> Ꮡt) {
    var removeTime = (slice<@string> _, global::go.log.slog_package.Attr a) => {
        if (a.Key == TimeKey) {
            return new Attr(nil);
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
    Ꮡt.Cleanup(() => {
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
    log_package.Print(errorˢ3);
    checkLogOutput(Ꮡt, ᏑlogBuf.String(), levelErrorMsgErrorˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string levelErrorMsgMsgErrEofA1ˢ = @"level=ERROR msg=msg err=EOF a=1"u8;
internal static readonly @string levelErrorMsgMsgErrEofˢ = @"level=ERROR msg=msg err=EOF !BADKEY=a"u8;

public static void TestLoggerError(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var removeTime = (slice<@string> _, global::go.log.slog_package.Attr a) => {
        if (a.Key == TimeKey) {
            return new Attr(nil);
        }
        return a;
    };
    var l = New(new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡ(new HandlerOptions(ReplaceAttr: removeTime)))));
    l.Error(msgˢ, errˢ, io.EOF, (@string)"a"u8, (nint)(1));
    checkLogOutput(Ꮡt, Ꮡbuf.String(), levelErrorMsgMsgErrEofA1ˢ);
    buf.Reset();
    // use local var 'args' to defeat vet check
    var args = new any[]{(@string)"err"u8, io.EOF, (@string)"a"u8}.slice();
    l.Error("msg"u8, args.ꓸꓸꓸ);
    checkLogOutput(Ꮡt, Ꮡbuf.String(), levelErrorMsgMsgErrEofˢ);
}

public static void TestNewLogLogger(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var h = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil);
    var ll = NewLogLogger(new global::go.log.slog_package.TextHandlerжΔHandler(h), LevelWarn);
    ll.Print(helloˢ);
    checkLogOutput(Ꮡt, Ꮡbuf.String(), "time=" + textTimeRE + @" level=WARN msg=hello");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wantedReceiverDidnTGetItˢ = (@string)"wanted receiver, didn't get it"u8;

public static void TestLoggerNoOps(ж<testing.T> Ꮡt) {
    var l = Default();
    if (l.With() != l) {
        Ꮡt.Error(wantedReceiverDidnTGetItˢ);
    }
    if (With() != l) {
        Ꮡt.Error(wantedReceiverDidnTGetItˢ);
    }
    if (l.WithGroup(""u8) != l) {
        Ꮡt.Error(wantedReceiverDidnTGetItˢ);
    }
}

[GoType("dyn")] internal partial struct TestContext_type {
    internal Actionꓸꓸꓸ<context.Context, @string, any> f;
    internal global::go.log.slog_package.ΔLevel wantLevel;
}

public static void TestContext(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Verify that the context argument to log output methods is passed to the handler.
        // Also check the level.
        var h = Ꮡ(new captureHandler(nil));
        var l = New(new slog_internal_test_package.captureHandlerжΔHandler(h));
        defer(SetDefault, Default(), ref ᒐ); // restore
        SetDefault(l);
        foreach (var (_, test) in new TestContext_type[]{
            new(l.DebugContext, LevelDebug),
            new(l.InfoContext, LevelInfo),
            new(l.WarnContext, LevelWarn),
            new(l.ErrorContext, LevelError),
            new(DebugContext, LevelDebug),
            new(InfoContext, LevelInfo),
            new(WarnContext, LevelWarn),
            new(ErrorContext, LevelError)
        }.slice()) {
            h.clear();
            var ctx = context.WithValue(context.Background(), (@string)"L"u8, test.wantLevel);
            test.f(ctx, "msg"u8);
            {
                var gv = (~h).ctx.Value((@string)"L"u8); if (!AreEqual(gv, test.wantLevel) || (~h).r.Level != test.wantLevel) {
                    Ꮡt.Errorf("got context value %v, level %s; want %s for both"u8, gv, (~h).r.Level, test.wantLevel);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void checkLogOutput(ж<testing.T> Ꮡt, @string got, @string wantRegexp) {
    Ꮡt.Helper();
    got = clean(got);
    wantRegexp = "^"u8 + wantRegexp + "$"u8;
    var (matched, err) = regexp.MatchString(wantRegexp, got);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!matched) {
        Ꮡt.Errorf("\ngot  %s\nwant %s"u8, got, wantRegexp);
    }
}

// clean prepares log output for comparison.
internal static @string clean(@string s) {
    if (len(s) > 0 && s[len(s) - 1] == (rune)'\n') {
        s = s.slice(0, len(s) - 1);
    }
    return strings.ReplaceAll(s, "\n"u8, "~"u8);
}

[GoType] [GoValueClone("r")] internal partial struct captureHandler {
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
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡh.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static bool Enabled(this ref captureHandler _Δp0, context.Context _Δp1, global::go.log.slog_package.ΔLevel _Δp2) {
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void clear(this ж<captureHandler> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        c.ctx = default!;
        c.r = new Record(nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoType] internal partial struct discardTestHandler {
    internal slice<global::go.log.slog_package.Attr> attrs;
}

internal static bool Enabled(this discardTestHandler d, context.Context _Δp1, global::go.log.slog_package.ΔLevel _Δp2) {
    return true;
}

internal static error Handle(this discardTestHandler _Δp0, context.Context _Δp1, global::go.log.slog_package.Record _Δp2) {
    return default!;
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
internal static readonly @string noAttrsˢ = "no attrs"u8;
internal static readonly @string attrsˢ = "attrs"u8;
internal static readonly @string attrsParallelˢ = "attrs-parallel"u8;
internal static readonly @string keysValuesˢ = "keys-values"u8;
internal static readonly @string withContextˢ = "WithContext"u8;
internal static readonly @string withContextParallelˢ = "WithContext-parallel"u8;

// This is a simple benchmark. See the benchmarks subdirectory for more extensive ones.
public static void BenchmarkNopLog(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ctx = context.Background();
    var l = New(new slog_internal_test_package.captureHandlerжΔHandler(Ꮡ(new captureHandler(nil))));
    var ctxʗ1 = ctx;
    var lʗ1 = l;
    Ꮡb.Run(noAttrsˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            lʗ1.LogAttrs(ctxʗ1, LevelInfo, msgˢ);
        }
    });
    var ctxʗ2 = ctx;
    var lʗ2 = l;
    Ꮡb.Run(attrsˢ, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            lʗ2.LogAttrs(ctxʗ2, LevelInfo, msgˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Bool("c"u8, true));
        }
    });
    var ctxʗ3 = ctx;
    var lʗ3 = l;
    Ꮡb.Run(attrsParallelˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        var ctxʗ4 = ctxʗ3;
        var lʗ4 = lʗ3;
        bΔ3.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                lʗ4.LogAttrs(ctxʗ4, LevelInfo, msgˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Bool("c"u8, true));
            }
        });
    });
    var ctxʗ5 = ctx;
    var lʗ5 = l;
    Ꮡb.Run(keysValuesˢ, (ж<testing.B> bΔ4) => {
        bΔ4.ReportAllocs();
        for (nint i = 0; i < (~bΔ4).N; i++) {
            lʗ5.Log(ctxʗ5, LevelInfo, msgˢ, (@string)"a"u8, (nint)(1), (@string)"b"u8, twoˢ, (@string)"c"u8, true);
        }
    });
    var ctxʗ6 = ctx;
    var lʗ6 = l;
    Ꮡb.Run(withContextˢ, (ж<testing.B> bΔ5) => {
        bΔ5.ReportAllocs();
        for (nint i = 0; i < (~bΔ5).N; i++) {
            lʗ6.LogAttrs(ctxʗ6, LevelInfo, msg2ˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Bool("c"u8, true));
        }
    });
    var ctxʗ7 = ctx;
    var lʗ7 = l;
    Ꮡb.Run(withContextParallelˢ, (ж<testing.B> bΔ6) => {
        bΔ6.ReportAllocs();
        var ctxʗ8 = ctxʗ7;
        var lʗ8 = lʗ7;
        bΔ6.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                lʗ8.LogAttrs(ctxʗ8, LevelInfo, msgˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ), go.log.slog_package.Bool("c"u8, true));
            }
        });
    });
}

// callerPC returns the program counter at the given stack depth.
[MethodImpl(MethodImplOptions.NoInlining)] internal static uintptr callerPC(nint depth) {
    array<uintptr> pcs = new(1);
    runtime.Callers(depth, pcs[..]);
    return pcs[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInRaceAsanˢ = (@string)"skipping test in race, asan, and msan modes"u8;

internal static void wantAllocs(ж<testing.T> Ꮡt, nint want, Action f) {
    if (race.Enabled || asan.Enabled || msan.Enabled) {
        Ꮡt.Skip(skippingTestInRaceAsanˢ);
    }
    testenv.SkipIfOptimizationOff(new slog_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Helper();
    nint got = (nint)testing.AllocsPerRun(5, f);
    if (got != want) {
        Ꮡt.Errorf("got %d allocs, want %d"u8, got, want);
    }
}

// panicTextAndJsonMarshaler is a type that panics in MarshalText and MarshalJSON.
[GoType] internal partial struct panicTextAndJsonMarshaler {
    internal any msg;
}

internal static (slice<byte>, error) MarshalText(this panicTextAndJsonMarshaler p) {
    throw panic(p.msg);
}

internal static (slice<byte>, error) MarshalJSON(this panicTextAndJsonMarshaler p) {
    throw panic(p.msg);
}

[GoType("dyn")] internal partial struct TestPanics_type {
    internal any @in;
    internal @string @out;
}

public static void TestPanics(ж<testing.T> Ꮡt) {
    // Revert any changes to the default logger. This is important because other
    // tests might change the default logger using SetDefault. Also ensure we
    // restore the default logger at the end of the test.
    var currentLogger = Default();
    var currentLogWriter = log_package.Writer();
    nint currentLogFlags = log_package.Flags();
    var currentLogWriterʗ1 = currentLogWriter;
    var currentLoggerʗ1 = currentLogger;
    Ꮡt.Cleanup(() => {
        SetDefault(currentLoggerʗ1);
        log_package.SetOutput(currentLogWriterʗ1);
        log_package.SetFlags(currentLogFlags);
    });
    ref var logBuf = ref heap(new bytes.Buffer(), out var ᏑlogBuf);
    log_package.SetOutput(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf));
    log_package.SetFlags((nint)((nint)log_package.Lshortfile & ~(nint)(nint)log_package.LstdFlags));
    SetDefault(New(new global::go.log.slog_package.defaultHandlerжΔHandler(newDefaultHandler(loginternal.DefaultOutput))));
    foreach (var (_, pt) in new TestPanics_type[]{
        new(((ж<panicTextAndJsonMarshaler>)nil), @"logger_test.go:\d+: INFO msg p=<nil>"u8),
        new(new panicTextAndJsonMarshaler(io.ErrUnexpectedEOF), @"logger_test.go:\d+: INFO msg p=""!PANIC: unexpected EOF"""u8),
        new(new panicTextAndJsonMarshaler((@string)"panicking"u8), @"logger_test.go:\d+: INFO msg p=""!PANIC: panicking"""u8),
        new(new panicTextAndJsonMarshaler((nint)(42)), @"logger_test.go:\d+: INFO msg p=""!PANIC: 42"""u8)
    }.slice()) {
        Info(msgˢ, (@string)"p"u8, pt.@in);
        checkLogOutput(Ꮡt, ᏑlogBuf.String(), pt.@out);
        logBuf.Reset();
    }
    SetDefault(New(new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(ᏑlogBuf), nil))));
    foreach (var (_, pt) in new TestPanics_type[]{
        new(((ж<panicTextAndJsonMarshaler>)nil), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":null}"),
        new(new panicTextAndJsonMarshaler(io.ErrUnexpectedEOF), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: unexpected EOF""}"),
        new(new panicTextAndJsonMarshaler((@string)"panicking"u8), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: panicking""}"),
        new(new panicTextAndJsonMarshaler((nint)(42)), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""msg"",""p"":""!PANIC: 42""}")
    }.slice()) {
        Info(msgˢ, (@string)"p"u8, pt.@in);
        checkLogOutput(Ꮡt, ᏑlogBuf.String(), pt.@out);
        logBuf.Reset();
    }
}

} // end slog_internal_test_package
