// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log.slog.@internal;

using context = context_package;
using flag = flag_package;
using race = go.@internal.race_package;
using io = io_package;
using slog = go.log.slog_package;
using @internal = go.log.slog.internal_package;
using testing = testing_package;
using go.@internal;
using go.log;
using go.log.slog;
using static go.log.slog.@internal.benchmarks_package;
using time = time_package;

partial class benchmarks_internal_test_package {

[GoInit] internal static void init() {
    flag.BoolVar(Ꮡ(@internal.IgnorePC), "nopc"u8, false, "do not invoke runtime.Callers"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingBenchmarkInRaceˢ = (@string)"skipping benchmark in race mode"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string statusˢ = "status"u8;
internal static readonly @string durationˢ = "duration"u8;
internal static readonly @string timeˢ2 = "time"u8;
internal static readonly @string errorˢ = "error"u8;

internal partial struct BenchmarkAttrs_type /*dyn*/ {
    internal @string name;
    internal slogꓸHandler h;
    internal bool skipRace;
}

internal partial struct BenchmarkAttrs_typeᴛ1 /*dyn*/ {
    internal @string name;
    internal Action f;
}

// We pass Attrs inline because it affects allocations: building
// up a list outside of the benchmarked code and passing it in with "..."
// reduces measured allocations.
public static void BenchmarkAttrs(ж<testing.B> Ꮡb) {
    var ctx = context.Background();
    foreach (var (_, vᴛ1) in new BenchmarkAttrs_type[]{
        new("disabled"u8, new benchmarks_internal_test_package.benchmarks_disabledHandlerᴠΔHandler(new disabledHandler(nil)), false),
        new("async discard"u8, new benchmarks_internal_test_package.benchmarks_asyncHandlerжΔHandler(newAsyncHandler()), true),
        new("fastText discard"u8, newFastTextHandler(io.Discard), false),
        new("Text discard"u8, new slog.TextHandlerжΔHandler(slog.NewTextHandler(io.Discard, nil)), false),
        new("JSON discard"u8, new slog.JSONHandlerжΔHandler(slog.NewJSONHandler(io.Discard, nil)), false)
    }.slice()) {
        ref var handler = ref heap(new BenchmarkAttrs_type(), out var Ꮡhandler);
        handler = vᴛ1;

        var logger = slog.New(handler.h);
        var ctxʗ1 = ctx;
        var handlerʗ1 = handler;
        var loggerʗ1 = logger;
        Ꮡb.Run(handler.name, (ж<testing.B> bΔ1) => {
            if (handlerʗ1.skipRace && race.Enabled) {
                bΔ1.Skip(skippingBenchmarkInRaceˢ);
            }
                    var loggerʗ2 = loggerʗ1;

                    var ctxʗ2 = ctxʗ1;
                    var loggerʗ3 = loggerʗ1;

                    var loggerʗ4 = loggerʗ1;

                    var loggerʗ5 = loggerʗ1;
            foreach (var (_, vᴛ2) in new BenchmarkAttrs_typeᴛ1[]{
                new(
                    "5 args"u8, // The number should match nAttrsInline in slog/record.go.
 // This should exercise the code path where no allocations
 // happen in Record or Attr. If there are allocations, they
 // should only be from Duration.String and Time.String.

                    () => {
                        loggerʗ2.LogAttrs(default!, slog.LevelInfo, testMessage,
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError));
                    }
                ),
                new(
                    "5 args ctx"u8,
                    () => {
                        loggerʗ3.LogAttrs(ctxʗ2, slog.LevelInfo, testMessage,
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError));
                    }
                ),
                new(
                    "10 args"u8,
                    () => {
                        loggerʗ4.LogAttrs(default!, slog.LevelInfo, testMessage,
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError));
                    }
                ),
                new(
                    "40 args"u8, // Try an extreme value to see if the results are reasonable.

                    () => {
                        loggerʗ5.LogAttrs(default!, slog.LevelInfo, testMessage,
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError),
                            slog.String(stringˢ, testString),
                            slog.Int(statusˢ, testInt),
                            slog.Duration(durationˢ, testDuration),
                            slog.Time(timeˢ2, testTime),
                            slog.Any(errorˢ, testError));
                    }
                )
            }.slice()) {
                ref var call = ref heap(new BenchmarkAttrs_typeᴛ1(), out var Ꮡcall);
                call = vᴛ2;

                var callʗ1 = call;
                bΔ1.Run(call.name, (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    var callʗ2 = callʗ1;
                    bΔ2.RunParallel((ж<testing.PB> pb) => {
                        while (pb.Next()) {
                            callʗ2.f();
                        }
                    });
                });
            }
        });
    }
}

} // end benchmarks_internal_test_package
