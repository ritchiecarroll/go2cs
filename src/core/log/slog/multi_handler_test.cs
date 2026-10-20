// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using testing = testing_package;
using time = time_package;
using io = io_package;
using static go.log.slog_package;

partial class slog_internal_test_package {

// mockFailingHandler is a handler that always returns an error
// from its Handle method.
internal partial struct mockFailingHandler {
    /*embed*/ public global::go.log.slog_package.ΔHandler Handler;
    internal error err;
}

// Go method set entry for the promoted 'Handler.Enabled()' - provided ONLY by the embedded
// interface field in *mockFailingHandler's method set; see the pointer-only satisfaction record.
internal static bool Enabled(this mockFailingHandler recvᴛ, context.Context arg1ᴛ, global::go.log.slog_package.ΔLevel arg2ᴛ) => recvᴛ.Handler.Enabled(arg1ᴛ, arg2ᴛ);

// Go method set entry for the promoted 'Handler.WithAttrs()' - provided ONLY by the embedded
// interface field in *mockFailingHandler's method set; see the pointer-only satisfaction record.
internal static global::go.log.slog_package.ΔHandler WithAttrs(this mockFailingHandler recvᴛ, slice<global::go.log.slog_package.Attr> attrs) => recvᴛ.Handler.WithAttrs(attrs);

// Go method set entry for the promoted 'Handler.WithGroup()' - provided ONLY by the embedded
// interface field in *mockFailingHandler's method set; see the pointer-only satisfaction record.
internal static global::go.log.slog_package.ΔHandler WithGroup(this mockFailingHandler recvᴛ, @string name) => recvᴛ.Handler.WithGroup(name);

internal static error Handle(this ref mockFailingHandler h, context.Context ctx, global::go.log.slog_package.Record r) {
    r = r.ΔClone();

    _ = h.Handler.Handle(ctx, r);
    return h.err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string handleSendsLogToAllˢ = "Handle sends log to all handlers"u8;
internal static readonly object userˢ = (@string)"user"u8;
internal static readonly @string enabledReturnsTrueIfAnyˢ = "Enabled returns true if any handler is enabled"u8;
internal static readonly object enabledShouldBeTrueForˢ = (@string)"Enabled should be true for INFO level, but got false"u8;
internal static readonly object enabledShouldBeTrueForˢ2 = (@string)"Enabled should be true for ERROR level, but got false"u8;
internal static readonly @string enabledReturnsFalseIfNoˢ = "Enabled returns false if no handlers are enabled"u8;
internal static readonly object enabledShouldBeFalseForˢ = (@string)"Enabled should be false for DEBUG level, but got true"u8;
internal static readonly @string withAttrsPropagatesˢ = "WithAttrs propagates attributes to all handlers"u8;
internal static readonly @string requestIdˢ = "request_id"u8;
internal static readonly @string requestProcessedˢ = "request processed"u8;
internal static readonly @string withGroupPropagatesGroupˢ = "WithGroup propagates group to all handlers"u8;
internal static readonly @string reqˢ = "req"u8;
internal static readonly @string userLoginˢ = "user login"u8;
internal static readonly object userIdˢ = (@string)"user_id"u8;
internal static readonly @string handlePropagatesErrorsˢ = "Handle propagates errors from handlers"u8;
internal static readonly @string mockFailingˢ = "mock failing"u8;
internal static readonly @string testMessageˢ = "test message"u8;
internal static readonly @string handleWithNoHandlersˢ = "Handle with no handlers"u8;
internal static readonly @string nothingˢ = "nothing"u8;

public static void TestMultiHandler(ж<testing.T> Ꮡt) {
    Ꮡt.Run(handleSendsLogToAllˢ, (ж<testing.T> tΔ1) => {
        ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf1), nil);
        var h2 = NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf2), nil);

        var multi = NewMultiHandler(new global::go.log.slog_package.TextHandlerжΔHandler(h1), new global::go.log.slog_package.JSONHandlerжΔHandler(h2));
        var logger = New(new global::go.log.slog_package.MultiHandlerжΔHandler(multi));

        logger.Info(helloWorldˢ, userˢ, testˢ);

        checkLogOutput(tΔ1, Ꮡbuf1.String(), "time=" + textTimeRE + @" level=INFO msg=""hello world"" user=test");
        checkLogOutput(tΔ1, Ꮡbuf2.String(), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""hello world"",""user"":""test""}");
    });

    Ꮡt.Run(enabledReturnsTrueIfAnyˢ, (ж<testing.T> tΔ2) => {
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))), Ꮡ(new HandlerOptions(Level: LevelError)));
        var h2 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))), Ꮡ(new HandlerOptions(Level: LevelInfo)));

        var multi = NewMultiHandler(new global::go.log.slog_package.TextHandlerжΔHandler(h1), new global::go.log.slog_package.TextHandlerжΔHandler(h2));

        if (!multi.Enabled(context.Background(), LevelInfo)) {
            tΔ2.Error(enabledShouldBeTrueForˢ);
        }
        if (!multi.Enabled(context.Background(), LevelError)) {
            tΔ2.Error(enabledShouldBeTrueForˢ2);
        }
    });

    Ꮡt.Run(enabledReturnsFalseIfNoˢ, (ж<testing.T> tΔ3) => {
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))), Ꮡ(new HandlerOptions(Level: LevelError)));
        var h2 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))), Ꮡ(new HandlerOptions(Level: LevelInfo)));

        var multi = NewMultiHandler(new global::go.log.slog_package.TextHandlerжΔHandler(h1), new global::go.log.slog_package.TextHandlerжΔHandler(h2));

        if (multi.Enabled(context.Background(), LevelDebug)) {
            tΔ3.Error(enabledShouldBeFalseForˢ);
        }
    });

    Ꮡt.Run(withAttrsPropagatesˢ, (ж<testing.T> tΔ4) => {
        ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf1), nil);
        var h2 = NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf2), nil);

        var multi = NewMultiHandler(new global::go.log.slog_package.TextHandlerжΔHandler(h1), new global::go.log.slog_package.JSONHandlerжΔHandler(h2)).WithAttrs(new global::go.log.slog_package.Attr[]{go.log.slog_package.String(requestIdˢ, "123"u8)}.slice());
        var logger = New(multi);

        logger.Info(requestProcessedˢ);

        checkLogOutput(tΔ4, Ꮡbuf1.String(), "time=" + textTimeRE + @" level=INFO msg=""request processed"" request_id=123");
        checkLogOutput(tΔ4, Ꮡbuf2.String(), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""request processed"",""request_id"":""123""}");
    });

    Ꮡt.Run(withGroupPropagatesGroupˢ, (ж<testing.T> tΔ5) => {
        ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf1), Ꮡ(new HandlerOptions(AddSource: false)));
        var h2 = NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf2), Ꮡ(new HandlerOptions(AddSource: false)));

        var multi = NewMultiHandler(new global::go.log.slog_package.TextHandlerжΔHandler(h1), new global::go.log.slog_package.JSONHandlerжΔHandler(h2)).WithGroup(reqˢ);
        var logger = New(multi);

        logger.Info(userLoginˢ, userIdˢ, (nint)42);

        checkLogOutput(tΔ5, Ꮡbuf1.String(), "time=" + textTimeRE + @" level=INFO msg=""user login"" req.user_id=42");
        checkLogOutput(tΔ5, Ꮡbuf2.String(), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""user login"",""req"":{""user_id"":42}}");
    });

    Ꮡt.Run(handlePropagatesErrorsˢ, (ж<testing.T> tΔ6) => {
        var errFail = errors.New(mockFailingˢ);

        ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf1), nil);
        var h2 = Ꮡ(new mockFailingHandler(Handler: new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf2), nil)), err: errFail));

        var multi = NewMultiHandler(new slog_internal_test_package.mockFailingHandlerжΔHandler(h2), new global::go.log.slog_package.TextHandlerжΔHandler(h1));

        var err = multi.Handle(context.Background(), NewRecord(time_package.Now(), LevelInfo, testMessageˢ, 0));
        if (!errors.Is(err, errFail)) {
            tΔ6.Errorf("Expected error: %v, but got: %v"u8, errFail, err);
        }

        checkLogOutput(tΔ6, Ꮡbuf1.String(), "time=" + textTimeRE + @" level=INFO msg=""test message""");
        checkLogOutput(tΔ6, Ꮡbuf2.String(), @"{""time"":""" + jsonTimeRE + @""",""level"":""INFO"",""msg"":""test message""}");
    });

    Ꮡt.Run(handleWithNoHandlersˢ, (ж<testing.T> tΔ7) => {
        var multi = NewMultiHandler();
        var logger = New(new global::go.log.slog_package.MultiHandlerжΔHandler(multi));

        logger.Info(nothingˢ);

        var err = multi.Handle(context.Background(), NewRecord(time_package.Now(), LevelInfo, testˢ, 0));
        if (err != default!) {
            tΔ7.Errorf("Handle with no sub-handlers should return nil, but got: %v"u8, err);
        }
    });
}

// Test that NewMultiHandler copies the input slice and is insulated from future modification.
public static void TestNewMultiHandlerCopy(ж<testing.T> Ꮡt) {
    ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
    var h1 = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf1), nil);
    var Δslice = new global::go.log.slog_package.ΔHandler[]{new global::go.log.slog_package.TextHandlerжΔHandler(h1)}.slice();
    var multi = NewMultiHandler(Δslice.ꓸꓸꓸ);
    Δslice[0] = default!;

    var err = multi.Handle(context.Background(), NewRecord(time_package.Now(), LevelInfo, testMessageˢ, 0));
    if (err != default!) {
        Ꮡt.Errorf("Expected nil error, but got: %v"u8, err);
    }
    checkLogOutput(Ꮡt, Ꮡbuf1.String(), "time=" + textTimeRE + @" level=INFO msg=""test message""");
}

} // end slog_internal_test_package
