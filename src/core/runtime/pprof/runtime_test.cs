// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using maps = maps_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

public static partial void TestSetGoroutineLabels(ж<testing.T> Ꮡt) {
    var sync = new channel<EmptyStruct>(0);
    ref var wantLabels = ref heap<map<@string, @string>>(out var ᏑwantLabels);
    wantLabels = new map<@string, @string>{};
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            Ꮡt.Errorf("Expected parent goroutine's profile labels to be empty before test, got %v"u8, gotLabels);
        }
    }
    var syncʗ1 = sync;
    goǃ(() => {
        {
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
                Ꮡt.Errorf("Expected child goroutine's profile labels to be empty before test, got %v"u8, gotLabels);
            }
        }
        syncʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);
    wantLabels = new map<@string, @string>{["key"u8] = "value"u8};
    var ctx = WithLabels(context.Background(), Labels(keyˢ, valueˢ));
    SetGoroutineLabels(ctx);
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            Ꮡt.Errorf("parent goroutine's profile labels: got %v, want %v"u8, gotLabels, wantLabels);
        }
    }
    var syncʗ2 = sync;
    goǃ(() => {
        {
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
                Ꮡt.Errorf("child goroutine's profile labels: got %v, want %v"u8, gotLabels, ᏑwantLabels.ValueSlot);
            }
        }
        syncʗ2.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);
    wantLabels = new map<@string, @string>{};
    ctx = context.Background();
    SetGoroutineLabels(ctx);
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            Ꮡt.Errorf("Expected parent goroutine's profile labels to be empty, got %v"u8, gotLabels);
        }
    }
    var syncʗ3 = sync;
    goǃ(() => {
        {
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
                Ꮡt.Errorf("Expected child goroutine's profile labels to be empty, got %v"u8, gotLabels);
            }
        }
        syncʗ3.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);
}

public static void TestDo(ж<testing.T> Ꮡt) {
    var wantLabels = new map<@string, @string>{};
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            Ꮡt.Errorf("Expected parent goroutine's profile labels to be empty before Do, got %v"u8, gotLabels);
        }
    }
    Do(context.Background(), Labels(key1ˢ, value1ˢ, key2ˢ, value2ˢ), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context ctx) => {
        var wantLabelsΔ1 = new map<@string, @string>{["key1"u8] = "value1"u8, ["key2"u8] = "value2"u8};
        {
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabelsΔ1)) {
                Ꮡt.Errorf("parent goroutine's profile labels: got %v, want %v"u8, gotLabels, wantLabelsΔ1);
            }
        }
        var sync = new channel<EmptyStruct>(0);
        var syncʗ1 = sync;
        goǃ(() => {
            var wantLabelsΔ2 = new map<@string, @string>{["key1"u8] = "value1"u8, ["key2"u8] = "value2"u8};
            {
                var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabelsΔ2)) {
                    Ꮡt.Errorf("child goroutine's profile labels: got %v, want %v"u8, gotLabels, wantLabelsΔ2);
                }
            }
            syncʗ1.ᐸꟷ(new EmptyStruct());
        });
        ᐸꟷ(sync);
    });
    wantLabels = new map<@string, @string>{};
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            fmt.Printf("%#v"u8, gotLabels);
            fmt.Printf("%#v"u8, wantLabels);
            Ꮡt.Errorf("Expected parent goroutine's profile labels to be empty after Do, got %v"u8, gotLabels);
        }
    }
}

internal static map<@string, @string> getProfLabel() {
    var l = (ж<global::go.runtime.pprof_package.labelMap>)(uintptr)(runtime_getProfLabel());
    if (l == nil) {
        return new map<@string, @string>{};
    }
    var m = new map<@string, @string>(len((~l).list));
    foreach (var (_, lbl) in (~l).list) {
        m[lbl.key] = lbl.value;
    }
    return m;
}

} // end pprof_internal_test_package
