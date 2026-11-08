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
using label = @internal.runtime.pprof.label_package;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

public static partial void TestSetGoroutineLabels(ж<testing.T> t) {
    var sync = new channel<EmptyStruct>(0);

    ref var wantLabels = ref heap<map<@string, @string>>(new map<@string, @string>{}, out var ᏑwantLabels);
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            t.Errorf("Expected parent goroutine's profile labels to be empty before test, got %v", gotLabels);
        }
    }
    var syncʗ1 = sync;
    goǃ(() => {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
            t.Errorf("Expected child goroutine's profile labels to be empty before test, got %v", gotLabels);
        }
        syncʗ1.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);

    wantLabels = new map<@string, @string>{["key"] = "value"};
    var ctx = WithLabels(context.Background(), Labels(keyˢ, valueˢ));
    SetGoroutineLabels(ctx);
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            t.Errorf("parent goroutine's profile labels: got %v, want %v", gotLabels, wantLabels);
        }
    }
    var syncʗ2 = sync;
    goǃ(() => {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
            t.Errorf("child goroutine's profile labels: got %v, want %v", gotLabels, ᏑwantLabels.ValueSlot);
        }
        syncʗ2.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);

    wantLabels = new map<@string, @string>{};
    ctx = context.Background();
    SetGoroutineLabels(ctx);
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            t.Errorf("Expected parent goroutine's profile labels to be empty, got %v", gotLabels);
        }
    }
    var syncʗ3 = sync;
    goǃ(() => {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, ᏑwantLabels.ValueSlot)) {
            t.Errorf("Expected child goroutine's profile labels to be empty, got %v", gotLabels);
        }
        syncʗ3.ᐸꟷ(new EmptyStruct());
    });
    ᐸꟷ(sync);
}

public static void TestDo(ж<testing.T> t) {
    var wantLabels = new map<@string, @string>{};
    {
        var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabels)) {
            t.Errorf("Expected parent goroutine's profile labels to be empty before Do, got %v", gotLabels);
        }
    }

    Do(context.Background(), Labels(key1ˢ, value1ˢ, key2ˢ, value2ˢ), [MethodImpl(MethodImplOptions.NoInlining)] (context.Context ctx) => {
        var wantLabelsΔ1 = new map<@string, @string>{["key1"] = "value1", ["key2"] = "value2"};
        {
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabelsΔ1)) {
                t.Errorf("parent goroutine's profile labels: got %v, want %v", gotLabels, wantLabelsΔ1);
            }
        }

        var sync = new channel<EmptyStruct>(0);
        var syncʗ1 = sync;
        goǃ(() => {
            var wantLabelsΔ2 = new map<@string, @string>{["key1"] = "value1", ["key2"] = "value2"};
            var gotLabels = getProfLabel(); if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(gotLabels, wantLabelsΔ2)) {
                t.Errorf("child goroutine's profile labels: got %v, want %v", gotLabels, wantLabelsΔ2);
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
            t.Errorf("Expected parent goroutine's profile labels to be empty after Do, got %v", gotLabels);
        }
    }
}

internal static map<@string, @string> getProfLabel() {
    var l = (ж<global::go.runtime.pprof_package.labelMap>)(uintptr)(runtime_getProfLabel());
    if (l == nil) {
        return new map<@string, @string>{};
    }
    var ls = l.Set.List;
    var m = new map<@string, @string>(len(ls));
    foreach (var (_, lbl) in range(ls)) {
        m[lbl.Key] = lbl.Value;
    }
    return m;
}

} // end pprof_internal_test_package
