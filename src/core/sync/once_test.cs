// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static sync_package;
using Δtesting = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

[GoType("num:nint")] partial struct one;

[GoRecv] internal static void Increment(this ref one o) {
    o++;
}

internal static void run(ж<Δtesting.T> Ꮡt, ж<Δsync.Once> Ꮡonce, ж<one> Ꮡo, channel<bool> c) {
    ref var once = ref Ꮡonce.DerefOrNull();
    ref var o = ref Ꮡo.DerefOrNull();

    Ꮡonce.Do(() => {
        Ꮡo.Value.Increment();
    });
    {
        one v = o; if (v != 1) {
            Ꮡt.Errorf("once failed inside run: %d is not 1"u8, v);
        }
    }
    c.ᐸꟷ(true);
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOnce(ж<Δtesting.T> Ꮡt) {
    var o = @new<one>();
    var once = @new<Δsync.Once>();
    var c = new channel<bool>(0);
    const nint N = 10;
    for (nint i = 0; i < N; i++) {
        goǃ(run, Ꮡt, once, o, c);
    }
    for (nint i = 0; i < N; i++) {
        ᐸꟷ(c);
    }
    if (o.Value != 1) {
        Ꮡt.Errorf("once failed outside run: %d is not 1"u8, o.Value);
    }
}

public static void TestOncePanic(ж<Δtesting.T> Ꮡt) {
    ref var once = ref heap(new Δsync.Once(), out var Ꮡonce);
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r == default!) {
                        Ꮡt.Fatalf("Once.Do did not panic"u8);
                    }
                }
            }, ref ᒐ);
            Ꮡonce.Do(() => {
                throw panic("failed");
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    Ꮡonce.Do(() => {
        Ꮡt.Fatalf("Once.Do called twice"u8);
    });
}

public static void BenchmarkOnce(ж<Δtesting.B> Ꮡb) {
    ref var once = ref heap(new Δsync.Once(), out var Ꮡonce);
    var f = () => {
    };
    var fʗ1 = f;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            Ꮡonce.Do(fʗ1);
        }
    });
}

} // end sync_test_package
