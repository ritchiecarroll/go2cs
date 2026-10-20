// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using strconv = strconv_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void BenchmarkTraceStack(ж<testing.B> Ꮡb) {
    foreach (var (_, stackDepth) in new nint[]{1, 10, 100}.slice()) {
        Ꮡb.Run("stackDepth="u8 + strconv.Itoa(stackDepth), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            benchmarkTraceStack(bΔ1, stackDepth);
        });
    }
}

internal static partial void benchmarkTraceStack(ж<testing.B> Ꮡb, nint stackDepth) {
    GoFrame ᒐ = default;
    try {
        ref var tab = ref heap(new global::go.runtime_internal_test_package.TraceStackTable(), out var Ꮡtab);
        defer(Ꮡtab.Reset, ref ᒐ);

        var wait = new channel<EmptyStruct>(0);
        var ready = new channel<EmptyStruct>(0);
        var done = new channel<EmptyStruct>(0);
        ref var gp = ref heap<ж<G>>(out var Ꮡgp);
        var doneʗ1 = done;
        var readyʗ1 = ready;
        var waitʗ1 = wait;
        goǃ(() => {
            Ꮡgp.ValueSlot = runtime_internal_test_package.Getg();
            var readyʗ2 = readyʗ1;
            var waitʗ2 = waitʗ1;
            useStackAndCall(stackDepth, () => {
                readyʗ2.ᐸꟷ(new EmptyStruct());
                ᐸꟷ(waitʗ2);
            });
            doneʗ1.ᐸꟷ(new EmptyStruct());
        });
        ᐸꟷ(ready);

        while (Ꮡb.Loop()) {
            runtime_internal_test_package.TraceStack(gp, Ꮡtab);
        }

        // Clean up.
        wait.ᐸꟷ(new EmptyStruct());
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
