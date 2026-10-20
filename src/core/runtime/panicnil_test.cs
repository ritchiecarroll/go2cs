// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using Δruntime = runtime_package;
using metrics = global::go.runtime.metrics_package;
using testing = testing_package;
using global::go.runtime;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defaultˢ = "default"u8;
internal static readonly @string godebugPanicnil0ˢ = "GODEBUG=panicnil=0"u8;
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string panicnil0ˢ = "panicnil=0"u8;
internal static readonly @string godebugPanicnil1ˢ = "GODEBUG=panicnil=1"u8;
internal static readonly @string panicnil1ˢ = "panicnil=1"u8;

public static void TestPanicNil(ж<testing.T> Ꮡt) {
    Ꮡt.Run(defaultˢ, (ж<testing.T> tΔ1) => {
        checkPanicNil(tΔ1, @new<Δruntime.PanicNilError>());
    });
    Ꮡt.Run(godebugPanicnil0ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Setenv(godebugˢ, panicnil0ˢ);
        checkPanicNil(tΔ2, @new<Δruntime.PanicNilError>());
    });
    Ꮡt.Run(godebugPanicnil1ˢ, (ж<testing.T> tΔ3) => {
        tΔ3.Setenv(godebugˢ, panicnil1ˢ);
        checkPanicNil(tΔ3, default!);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugNonDefaultˢ = "/godebug/non-default-behavior/panicnil:events"u8;

internal static void checkPanicNil(ж<testing.T> Ꮡt, any want) {
    GoFrame ᒐ = default;
    try {
        @string name = godebugNonDefaultˢ;
        var s = new metrics.Sample[]{new(Name: name)}.slice();
        metrics.Read(s);
        var v1 = s[0].Value.Uint64();

        var sʗ1 = s;
        defer(() => {
            var e = recover();
            if (!AreEqual(reflect.TypeOf(e), reflect.TypeOf(want))) {
                println(e, want);
                Ꮡt.Errorf("recover() = %v, want %v"u8, e, want);
                throw panic(e);
            }
            metrics.Read(sʗ1);
            var v2 = sʗ1[0].Value.Uint64();
            if (want == default!){
                if (v2 != v1 + 1) {
                    Ꮡt.Errorf("recover() with panicnil=1 did not increment metric %s"u8, name);
                }
            } else {
                if (v2 != v1) {
                    Ꮡt.Errorf("recover() with panicnil=0 incremented metric %s: %d -> %d"u8, name, v1, v2);
                }
            }
        }, ref ᒐ);
        throw panic(default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
