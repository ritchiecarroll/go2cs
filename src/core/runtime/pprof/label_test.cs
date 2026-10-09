// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

internal static slice<global::go.runtime.pprof_package.label> labelsSorted(context.Context ctx) {
    ref var ls = ref heap<slice<global::go.runtime.pprof_package.label>>(out var Ꮡls);
    ls = new global::go.runtime.pprof_package.label[]{}.slice();
    ForLabels(ctx, (@string key, @string value) => {
        Ꮡls.ValueSlot = append(Ꮡls.ValueSlot, new label(key, value));
        return true;
    });
    slices.SortFunc(ls, (global::go.runtime.pprof_package.label a, global::go.runtime.pprof_package.label b) => strings_package.Compare(a.key, b.key));
    return ls;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string valueˢ = "value"u8;
internal static readonly @string key2ˢ = "key2"u8;
internal static readonly @string value2ˢ = "value2"u8;
internal static readonly @string value3ˢ = "value3"u8;
internal static readonly @string key4ˢ = "key4"u8;
internal static readonly @string value4aˢ = "value4a"u8;
internal static readonly @string value4bˢ = "value4b"u8;

public static void TestContextLabels(ж<testing.T> Ꮡt) {
    // Background context starts with no labels.
    var ctx = context.Background();
    var labels = labelsSorted(ctx);
    if (len(labels) != 0) {
        Ꮡt.Errorf("labels on background context: want [], got %v "u8, labels);
    }
    // Add a single label.
    ctx = WithLabels(ctx, Labels(keyˢ, valueˢ));
    // Retrieve it with Label.
    var (v, ok) = Label(ctx, keyˢ);
    if (!ok || v != "value"u8) {
        Ꮡt.Errorf(@"Label(ctx, ""key""): got %v, %v; want ""value"", ok"u8, v, ok);
    }
    var gotLabels = labelsSorted(ctx);
    var wantLabels = new global::go.runtime.pprof_package.label[]{new("key"u8, "value"u8)}.slice();
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        Ꮡt.Errorf("(sorted) labels on context: got %v, want %v"u8, gotLabels, wantLabels);
    }
    // Add a label with a different key.
    ctx = WithLabels(ctx, Labels(key2ˢ, value2ˢ));
    (v, ok) = Label(ctx, key2ˢ);
    if (!ok || v != "value2"u8) {
        Ꮡt.Errorf(@"Label(ctx, ""key2""): got %v, %v; want ""value2"", ok"u8, v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = new global::go.runtime.pprof_package.label[]{new("key"u8, "value"u8), new("key2"u8, "value2"u8)}.slice();
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        Ꮡt.Errorf("(sorted) labels on context: got %v, want %v"u8, gotLabels, wantLabels);
    }
    // Add label with first key to test label replacement.
    ctx = WithLabels(ctx, Labels(keyˢ, value3ˢ));
    (v, ok) = Label(ctx, keyˢ);
    if (!ok || v != "value3"u8) {
        Ꮡt.Errorf(@"Label(ctx, ""key3""): got %v, %v; want ""value3"", ok"u8, v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = new global::go.runtime.pprof_package.label[]{new("key"u8, "value3"u8), new("key2"u8, "value2"u8)}.slice();
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        Ꮡt.Errorf("(sorted) labels on context: got %v, want %v"u8, gotLabels, wantLabels);
    }
    // Labels called with two labels with the same key should pick the second.
    ctx = WithLabels(ctx, Labels(key4ˢ, value4aˢ, key4ˢ, value4bˢ));
    (v, ok) = Label(ctx, key4ˢ);
    if (!ok || v != "value4b"u8) {
        Ꮡt.Errorf(@"Label(ctx, ""key4""): got %v, %v; want ""value4b"", ok"u8, v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = new global::go.runtime.pprof_package.label[]{new("key"u8, "value3"u8), new("key2"u8, "value2"u8), new("key4"u8, "value4b"u8)}.slice();
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        Ꮡt.Errorf("(sorted) labels on context: got %v, want %v"u8, gotLabels, wantLabels);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string key1ˢ = "key1"u8;
internal static readonly @string value1ˢ = "value1"u8;
internal static readonly @string key3ˢ = "key3"u8;
internal static readonly @string key4WithNewlineˢ = "key4WithNewline"u8;
internal static readonly @string value4ˢ = "\nvalue4"u8;

internal partial struct TestLabelMapStringer_type /*dyn*/ {
    internal global::go.runtime.pprof_package.labelMap m;
    internal @string expected;
}

public static void TestLabelMapStringer(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestLabelMapStringer_type[]{
        new(
            m: new labelMap(nil), // empty map

            expected: "{}"u8
        ), new(
        m: new labelMap(
            Labels(fooˢ, barˢ)
        ),
        expected: @"{""foo"":""bar""}"u8
    ), new(
        m: new labelMap(
            Labels(
                fooˢ, barˢ,
                key1ˢ, value1ˢ,
                key2ˢ, value2ˢ,
                key3ˢ, value3ˢ,
                key4WithNewlineˢ, value4ˢ)
        ),
        expected: @"{""foo"":""bar"", ""key1"":""value1"", ""key2"":""value2"", ""key3"":""value3"", ""key4WithNewline"":""\nvalue4""}"u8
    )
    }.slice()) {
        ref var tbl = ref heap(new TestLabelMapStringer_type(), out var Ꮡtbl);
        tbl = vᴛ1;

        {
            @string got = Ꮡtbl.of(TestLabelMapStringer_type.Ꮡm).String(); if (tbl.expected != got) {
                Ꮡt.Errorf("%#v.String() = %q; want %q"u8, tbl.m, got, tbl.expected);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setOneˢ = "set-one"u8;
internal static readonly @string mergeOneˢ = "merge-one"u8;
internal static readonly @string val1ˢ = "val1"u8;
internal static readonly @string overwriteOneˢ = "overwrite-one"u8;
internal static readonly @string valˢ = "val"u8;
internal static readonly @string setManyˢ = "set-many"u8;
internal static readonly @string mergeManyˢ = "merge-many"u8;
internal static readonly @string overwriteManyˢ = "overwrite-many"u8;

public static void BenchmarkLabels(ж<testing.B> Ꮡb) {
    Ꮡb.Run(setOneˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Do(context.Background(), Labels(keyˢ, valueˢ), (context.Context _) => {
            });
        }
    });
    Ꮡb.Run(mergeOneˢ, (ж<testing.B> bΔ2) => {
        var ctx = WithLabels(context.Background(), Labels(key1ˢ, val1ˢ));
        bΔ2.ReportAllocs();
        bΔ2.ResetTimer();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            Do(ctx, Labels(key2ˢ, value2ˢ), (context.Context _) => {
            });
        }
    });
    Ꮡb.Run(overwriteOneˢ, (ж<testing.B> bΔ3) => {
        var ctx = WithLabels(context.Background(), Labels(keyˢ, valˢ));
        bΔ3.ReportAllocs();
        bΔ3.ResetTimer();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            Do(ctx, Labels(keyˢ, valueˢ), (context.Context _) => {
            });
        }
    });
    foreach (var (_, scenario) in new @string[]{"ordered"u8, "unordered"u8}.slice()) {
        ref var labels = ref heap<slice<@string>>(out var Ꮡlabels);
        for (nint i = 0; i < 10; i++) {
            labels = append(labels, fmt.Sprintf("key%03d"u8, i), fmt.Sprintf("value%03d"u8, i));
        }
        if (scenario == "unordered"u8) {
            (labels[0], labels[len(labels) - 1]) = (labels[len(labels) - 1], labels[0]);
        }
        Ꮡb.Run(scenario, (ж<testing.B> bΔ4) => {
            bΔ4.Run(setManyˢ, (ж<testing.B> bΔ5) => {
                bΔ5.ReportAllocs();
                bΔ5.ResetTimer();
                for (nint i = 0; i < (~bΔ5).N; i++) {
                    Do(context.Background(), Labels(Ꮡlabels.ValueSlot.ꓸꓸꓸ), (context.Context _) => {
                    });
                }
            });
            bΔ4.Run(mergeManyˢ, (ж<testing.B> bΔ6) => {
                var ctx = WithLabels(context.Background(), Labels(Ꮡlabels.ValueSlot.slice(0, len(Ꮡlabels.ValueSlot) / 2).ꓸꓸꓸ));
                bΔ6.ResetTimer();
                bΔ6.ReportAllocs();
                for (nint i = 0; i < (~bΔ6).N; i++) {
                    Do(ctx, Labels(Ꮡlabels.ValueSlot.slice(len(Ꮡlabels.ValueSlot) / 2).ꓸꓸꓸ), (context.Context _) => {
                    });
                }
            });
            bΔ4.Run(overwriteManyˢ, (ж<testing.B> bΔ7) => {
                var ctx = WithLabels(context.Background(), Labels(Ꮡlabels.ValueSlot.ꓸꓸꓸ));
                bΔ7.ReportAllocs();
                bΔ7.ResetTimer();
                for (nint i = 0; i < (~bΔ7).N; i++) {
                    Do(ctx, Labels(Ꮡlabels.ValueSlot.ꓸꓸꓸ), (context.Context _) => {
                    });
                }
            });
        });
    }
}

// TODO: hit slow path in Labels

} // end pprof_internal_test_package
