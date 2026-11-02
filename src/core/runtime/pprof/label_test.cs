// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using label = @internal.runtime.pprof.label_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal.runtime.pprof;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

internal static slice<label.Label> labelsSorted(context.Context ctx) {
    ref var ls = ref heap<slice<label.Label>>(slice<label.Label>([]), out var Ꮡls);
    ForLabels(ctx, (@string key, @string value) => {
        Ꮡls.ValueSlot = append(Ꮡls.ValueSlot, new label.Label(Key: key, Value: value));
        return true;
    });
    slices.SortFunc(ls, (label.Label a, label.Label b) => strings_package.Compare(a.Key, b.Key));
    return ls;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string keyˢ => ref keyˢᵇ;
private static readonly @string keyˢᵇ = "key"u8;
internal static ref readonly @string valueˢ => ref valueˢᵇ;
private static readonly @string valueˢᵇ = "value"u8;
internal static ref readonly @string key2ˢ => ref key2ˢᵇ;
private static readonly @string key2ˢᵇ = "key2"u8;
internal static ref readonly @string value2ˢ => ref value2ˢᵇ;
private static readonly @string value2ˢᵇ = "value2"u8;
internal static ref readonly @string value3ˢ => ref value3ˢᵇ;
private static readonly @string value3ˢᵇ = "value3"u8;
internal static ref readonly @string key4ˢ => ref key4ˢᵇ;
private static readonly @string key4ˢᵇ = "key4"u8;
internal static ref readonly @string value4aˢ => ref value4aˢᵇ;
private static readonly @string value4aˢᵇ = "value4a"u8;
internal static ref readonly @string value4bˢ => ref value4bˢᵇ;
private static readonly @string value4bˢᵇ = "value4b"u8;

public static void TestContextLabels(ж<testing.T> t) {
    // Background context starts with no labels.
    var ctx = context.Background();
    var labels = labelsSorted(ctx);
    if (len(labels) != 0) {
        t.Errorf("labels on background context: want [], got %v ", labels);
    }

    // Add a single label.
    ctx = WithLabels(ctx, Labels(keyˢ, valueˢ));
    // Retrieve it with Label.
    var (v, ok) = Label(ctx, keyˢ);
    if (!ok || v != "value"u8) {
        t.Errorf(@"Label(ctx, ""key""): got %v, %v; want ""value"", ok", v, ok);
    }
    var gotLabels = labelsSorted(ctx);
    var wantLabels = slice<label.Label>([new(Key: "key", Value: "value")]);
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        t.Errorf("(sorted) labels on context: got %v, want %v", gotLabels, wantLabels);
    }

    // Add a label with a different key.
    ctx = WithLabels(ctx, Labels(key2ˢ, value2ˢ));
    (v, ok) = Label(ctx, key2ˢ);
    if (!ok || v != "value2"u8) {
        t.Errorf(@"Label(ctx, ""key2""): got %v, %v; want ""value2"", ok", v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = slice<label.Label>([new(Key: "key", Value: "value"), new(Key: "key2", Value: "value2")]);
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        t.Errorf("(sorted) labels on context: got %v, want %v", gotLabels, wantLabels);
    }

    // Add label with first key to test label replacement.
    ctx = WithLabels(ctx, Labels(keyˢ, value3ˢ));
    (v, ok) = Label(ctx, keyˢ);
    if (!ok || v != "value3"u8) {
        t.Errorf(@"Label(ctx, ""key3""): got %v, %v; want ""value3"", ok", v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = slice<label.Label>([new(Key: "key", Value: "value3"), new(Key: "key2", Value: "value2")]);
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        t.Errorf("(sorted) labels on context: got %v, want %v", gotLabels, wantLabels);
    }

    // Labels called with two labels with the same key should pick the second.
    ctx = WithLabels(ctx, Labels(key4ˢ, value4aˢ, key4ˢ, value4bˢ));
    (v, ok) = Label(ctx, key4ˢ);
    if (!ok || v != "value4b"u8) {
        t.Errorf(@"Label(ctx, ""key4""): got %v, %v; want ""value4b"", ok", v, ok);
    }
    gotLabels = labelsSorted(ctx);
    wantLabels = slice<label.Label>([new(Key: "key", Value: "value3"), new(Key: "key2", Value: "value2"), new(Key: "key4", Value: "value4b")]);
    if (!reflect.DeepEqual(gotLabels, wantLabels)) {
        t.Errorf("(sorted) labels on context: got %v, want %v", gotLabels, wantLabels);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string key1ˢ => ref key1ˢᵇ;
private static readonly @string key1ˢᵇ = "key1"u8;
internal static ref readonly @string value1ˢ => ref value1ˢᵇ;
private static readonly @string value1ˢᵇ = "value1"u8;
internal static ref readonly @string key3ˢ => ref key3ˢᵇ;
private static readonly @string key3ˢᵇ = "key3"u8;
internal static ref readonly @string key4WithNewlineˢ => ref key4WithNewlineˢᵇ;
private static readonly @string key4WithNewlineˢᵇ = "key4WithNewline"u8;
internal static ref readonly @string value4ˢ => ref value4ˢᵇ;
private static readonly @string value4ˢᵇ = "\nvalue4"u8;

internal partial struct TestLabelMapStringer_type /*dyn*/ {
    internal global::go.runtime.pprof_package.labelMap m;
    internal @string expected;
}

public static void TestLabelMapStringer(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestLabelMapStringer_type>([
        new(
            m: new labelMap(
                // empty map
            ),
            expected: "{}"
        ), new(
        m: new labelMap(
            label.NewSet(Labels(fooˢ, barˢ).list)
        ),
        expected: @"{""foo"":""bar""}"
    ), new(
        m: new labelMap(
            label.NewSet(Labels(
                fooˢ, barˢ,
                key1ˢ, value1ˢ,
                key2ˢ, value2ˢ,
                key3ˢ, value3ˢ,
                key4WithNewlineˢ, value4ˢ).list)
        ),
        expected: @"{""foo"":""bar"", ""key1"":""value1"", ""key2"":""value2"", ""key3"":""value3"", ""key4WithNewline"":""\nvalue4""}"
    )
    ])) {
        ref var tbl = ref heap(new TestLabelMapStringer_type(), out var Ꮡtbl);
        tbl = vᴛ1;

        {
            @string got = Ꮡtbl.of(TestLabelMapStringer_type.Ꮡm).String(); if (tbl.expected != got) {
                t.Errorf("%#v.String() = %q; want %q", tbl.m, got, tbl.expected);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string setOneˢ => ref setOneˢᵇ;
private static readonly @string setOneˢᵇ = "set-one"u8;
internal static ref readonly @string mergeOneˢ => ref mergeOneˢᵇ;
private static readonly @string mergeOneˢᵇ = "merge-one"u8;
internal static ref readonly @string val1ˢ => ref val1ˢᵇ;
private static readonly @string val1ˢᵇ = "val1"u8;
internal static ref readonly @string overwriteOneˢ => ref overwriteOneˢᵇ;
private static readonly @string overwriteOneˢᵇ = "overwrite-one"u8;
internal static ref readonly @string valˢ => ref valˢᵇ;
private static readonly @string valˢᵇ = "val"u8;
internal static ref readonly @string setManyˢ => ref setManyˢᵇ;
private static readonly @string setManyˢᵇ = "set-many"u8;
internal static ref readonly @string mergeManyˢ => ref mergeManyˢᵇ;
private static readonly @string mergeManyˢᵇ = "merge-many"u8;
internal static ref readonly @string overwriteManyˢ => ref overwriteManyˢᵇ;
private static readonly @string overwriteManyˢᵇ = "overwrite-many"u8;

public static void BenchmarkLabels(ж<testing.B> b) {
    b.Run(setOneˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Do(context.Background(), Labels(keyˢ, valueˢ), (context.Context _) => {
            });
        }
    });

    b.Run(mergeOneˢ, (ж<testing.B> bΔ2) => {
        var ctx = WithLabels(context.Background(), Labels(key1ˢ, val1ˢ));

        bΔ2.ReportAllocs();
        bΔ2.ResetTimer();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            Do(ctx, Labels(key2ˢ, value2ˢ), (context.Context _) => {
            });
        }
    });

    b.Run(overwriteOneˢ, (ж<testing.B> bΔ3) => {
        var ctx = WithLabels(context.Background(), Labels(keyˢ, valˢ));

        bΔ3.ReportAllocs();
        bΔ3.ResetTimer();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            Do(ctx, Labels(keyˢ, valueˢ), (context.Context _) => {
            });
        }
    });

    foreach (var (_, scenario) in slice<@string>(["ordered", "unordered"])) {
        ref var labels = ref heap<slice<@string>>(out var Ꮡlabels);
        for (nint i = 0; i < 10; i++) {
            labels = append(labels, fmt.Sprintf("key%03d"u8, i), fmt.Sprintf("value%03d"u8, i));
        }
        if (scenario == "unordered"u8) {
            (labels[0], labels[len(labels) - 1]) = (labels[len(labels) - 1], labels[0]);
        }

        b.Run(scenario, (ж<testing.B> bΔ4) => {
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

    // TODO: hit slow path in Labels
}

} // end pprof_internal_test_package
