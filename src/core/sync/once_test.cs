// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static sync_package;
using Δtesting = testing_package;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

partial struct one /*num:nint*/;

internal static void Increment(this ref one o) {
    o++;
}

internal static void run(ж<Δtesting.T> t, ж<Δsync.Once> once, ж<one> Ꮡo, channel<bool> c) {
    ref var o = ref Ꮡo.DerefOrNull();

    once.Do(() => {
        Ꮡo.Value.Increment();
    });
    one v = o; if (v != 1) {
        t.Errorf("once failed inside run: %d is not 1", v);
    }
    c.ᐸꟷ(true);
}

public static partial void TestOnce(ж<Δtesting.T> t) {
    var o = @new<one>();
    var once = @new<Δsync.Once>();
    var c = new channel<bool>(0);
    const nint N = 10;
    for (nint i = 0; i < N; i++) {
        goǃ(run, t, once, o, c);
    }
    for (nint i = 0; i < N; i++) {
        ᐸꟷ(c);
    }
    if (o.Value != 1) {
        t.Errorf("once failed outside run: %d is not 1", o.Value);
    }
}

public static void TestOncePanic(ж<Δtesting.T> t) {
    ref var once = ref heap(new Δsync.Once(), out var Ꮡonce);
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var r = recover(); if (r == nil) {
                    t.Fatalf("Once.Do did not panic");
                }
            }, ref ᒐ);
            Ꮡonce.Do(() => {
                throw panic("failed");
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    Ꮡonce.Do(() => {
        t.Fatalf("Once.Do called twice");
    });
}

public static void BenchmarkOnce(ж<Δtesting.B> b) {
    ref var once = ref heap(new Δsync.Once(), out var Ꮡonce);
    var f = () => {
    };
    var fʗ1 = f;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            Ꮡonce.Do(fʗ1);
        }
    });
}

} // end sync_test_package
