// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using Δslog = go.log.slog_package;
using slogtest = go.log.slog.@internal.slogtest_package;
using os = os_package;
using go.log;
using go.log.slog.@internal;
using io = io_package;
using static go.log.slog_internal_test_package;

partial class slog_test_package {

// A LevelHandler wraps a Handler with an Enabled method
// that returns false for levels below a minimum.
partial struct LevelHandler {
    internal Δslog.Leveler level;
    internal slogꓸHandler handler;
}

// NewLevelHandler returns a LevelHandler with the given level.
// All methods except Enabled delegate to h.
public static ж<LevelHandler> NewLevelHandler(Δslog.Leveler level, slogꓸHandler h) {
    // Optimization: avoid chains of LevelHandlers.
    {
        var (lh, ok) = h._<ж<LevelHandler>>(ᐧ); if (ok) {
            h = lh.Handler();
        }
    }
    return Ꮡ(new LevelHandler(level, h));
}

// Enabled implements Handler.Enabled by reporting whether
// level is at least as large as h's level.
public static bool Enabled(this ref LevelHandler h, context.Context _, slogꓸLevel level) {
    return level >= h.level.Level();
}

// Handle implements Handler.Handle.
public static error Handle(this ref LevelHandler h, context.Context ctx, Δslog.Record r) {
    r = r.ΔClone();

    return h.handler.Handle(ctx, r);
}

// WithAttrs implements Handler.WithAttrs.
public static slogꓸHandler WithAttrs(this ref LevelHandler h, slice<Δslog.Attr> attrs) {
    return new slog_test_package.LevelHandlerжΔHandler(NewLevelHandler(h.level, h.handler.WithAttrs(attrs)));
}

// WithGroup implements Handler.WithGroup.
public static slogꓸHandler WithGroup(this ref LevelHandler h, @string name) {
    return new slog_test_package.LevelHandlerжΔHandler(NewLevelHandler(h.level, h.handler.WithGroup(name)));
}

// Handler returns the Handler wrapped by h.
public static slogꓸHandler Handler(this ref LevelHandler h) {
    return h.handler;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notPrintedˢ = "not printed"u8;
internal static readonly @string printedˢ = "printed"u8;

// This example shows how to Use a LevelHandler to change the level of an
// existing Handler while preserving its other behavior.
//
// This example demonstrates increasing the log level to reduce a logger's
// output.
//
// Another typical use would be to decrease the log level (to LevelDebug, say)
// during a part of the program that was suspected of containing a bug.
public static void ExampleHandler_levelHandler() {
    var th = Δslog.NewTextHandler(new os.FileжWriter(os.Stdout), Ꮡ(new Δslog.HandlerOptions(ReplaceAttr: slogtest.RemoveTime)));
    var logger = Δslog.New(new slog_test_package.LevelHandlerжΔHandler(NewLevelHandler(Δslog.LevelWarn, new Δslog.TextHandlerжΔHandler(th))));
    logger.Info(notPrintedˢ);
    logger.Warn(printedˢ);
}

// Output:
// level=WARN msg=printed

} // end slog_test_package
