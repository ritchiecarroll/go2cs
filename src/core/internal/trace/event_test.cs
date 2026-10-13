// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using testing = testing_package;
using static go.@internal.trace_package;

partial class trace_internal_test_package {

public static void TestPanicEvent(ж<testing.T> Ꮡt) {
    // Use a sync event for this because it doesn't have any extra metadata.
    ref var ev = ref heap<global::go.@internal.trace_package.ΔEvent>(out var Ꮡev);
    ev = syncEvent(nil, 0);

    var evʗ1 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ1.Range();
    });
    var evʗ2 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ2.Metric();
    });
    var evʗ3 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ3.Log();
    });
    var evʗ4 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ4.Task();
    });
    var evʗ5 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ5.Region();
    });
    var evʗ6 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ6.Label();
    });
    var evʗ7 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ7.RangeAttributes();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToPanicˢ = (@string)"failed to panic"u8;

internal static void mustPanic(ж<testing.T> Ꮡt, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(failedToPanicˢ);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end trace_internal_test_package
