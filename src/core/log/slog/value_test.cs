// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using fmt = fmt_package;
using asan = go.@internal.asan_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using go.@internal;
using static go.log.slog_package;

partial class slog_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string groupˢ => ref groupˢᵇ;
private static readonly @string groupˢᵇ = "Group"u8;

public static partial void TestKindString(ж<testing.T> t) {
    @string got = KindGroup.String();
    @string want = groupˢ; if (got != want) {
        t.Errorf("got %q, want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fuuˢ => ref fuuˢᵇ;
private static readonly @string fuuˢᵇ = "fuu"u8;

public static void TestValueEqual(ж<testing.T> t) {
    ref var x = ref heap(new nint(), out var Ꮡx);
    ref var y = ref heap(new nint(), out var Ꮡy);
    var vals = slice<global::go.log.slog_package.Value>([
        new(),
        Int64Value(1),
        Int64Value(2),
        Float64Value(3.5D),
        Float64Value(3.7D),
        BoolValue(true),
        BoolValue(false),
        TimeValue(testTime),
        TimeValue(new time_package.Time()),
        TimeValue(time_package.Date(2001, 1, 2, 3, 4, 5, 0, time_package.ΔUTC)),
        TimeValue(time_package.Date(2300, 1, 1, 0, 0, 0, 0, time_package.ΔUTC)),            // overflows nanoseconds
        TimeValue(time_package.Date(1715, 6, 13, 0, 25, 26, 290448384, time_package.ΔUTC)), // overflowed value
        AnyValue(Ꮡx),
        AnyValue(Ꮡy),
        GroupValue(go.log.slog_package.Bool("b", true), Int("i", 3)),
        GroupValue(go.log.slog_package.Bool("b", true), Int("i", 4)),
        GroupValue(go.log.slog_package.Bool("b", true), Int("j", 4)),
        DurationValue((time.Duration)(3000000000L)),
        DurationValue(2 * time_package.ΔSecond),
        StringValue(fooˢ),
        StringValue(fuuˢ)
    ]);
    foreach (var (i, v1) in range(vals)) {
        foreach (var (j, v2) in range(vals)) {
            var got = v1.Equal(v2);
            var want = i == j;
            if (got != want) {
                t.Errorf("%v.Equal(%v): got %t, want %t", v1, v2, got, want);
            }
        }
    }
}

internal static bool /*b*/ panics(Action f) {
    bool b = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var x = recover(); if (x != nil) {
                b = true;
            }
        }, ref ᒐ);
        f();
        b = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return b;
}

internal partial struct TestValueString_type /*dyn*/ {
    internal global::go.log.slog_package.Value v;
    internal @string want;
}

public static void TestValueString(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestValueString_type>([
        new(Int64Value(-3), "-3"),
        new(Uint64Value(1), "1"),
        new(Float64Value(.15D), "0.15"),
        new(BoolValue(true), "true"),
        new(StringValue(fooˢ), "foo"),
        new(TimeValue(testTime), "2000-01-02 03:04:05 +0000 UTC"),
        new(AnyValue((time.Duration)(3000000000L)), "3s"),
        new(GroupValue(Int("a", 1), go.log.slog_package.Bool("b", true)), "[a=1 b=true]")
    ]))) {
        @string got = test.v.String(); if (got != test.want) {
            t.Errorf("%#v:\ngot  %q\nwant %q", test.v, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;

public static void TestValueNoAlloc(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }

    // Assign values just to make sure the compiler doesn't optimize away the statements.
    ref var i = ref heap(new int64(), out var Ꮡi);
    uint64 u = default;
    float64 f = default;
    bool b = default;
    @string s = default;
    ref var x = ref heap<any>(out var Ꮡx);
    ж<int64> p = Ꮡi;
    time.Duration d = default;
    ref var tm = ref heap(new time.Time(), out var Ꮡtm);
    var pʗ1 = p;
    nint a = (nint)testing.AllocsPerRun(5, () => {
        Ꮡi.Value = Int64Value(1).Int64();
        u = Uint64Value(1).Uint64();
        f = Float64Value(1D).Float64();
        b = BoolValue(true).Bool();
        s = StringValue(fooˢ).String();
        d = DurationValue(d).Duration();
        Ꮡtm.Value = TimeValue(testTime).Time();
        Ꮡx.ValueSlot = AnyValue(pʗ1.OrTypedNil()).Any();
    });
    if (a != 0) {
        t.Errorf("got %d allocs, want zero", a);
    }
    _ = u;
    _ = f;
    _ = b;
    _ = s;
    _ = x;
    _ = tm;
}

public static void TestAnyLevelAlloc(ж<testing.T> t) {
    // Because typical Levels are small integers,
    // they are zero-alloc.
    ref var a = ref heap(new global::go.log.slog_package.Value(), out var Ꮡa);
    global::go.log.slog_package.ΔLevel x = LevelDebug + 100;
    wantAllocs(t, 0, () => {
        Ꮡa.Value = AnyValue(x);
    });
    _ = a;
}

internal partial struct TestAnyValue_type /*dyn*/ {
    internal any @in;
    internal global::go.log.slog_package.Value want;
}

public static void TestAnyValue(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestAnyValue_type>([
        new((nint)1, IntValue(1)),
        new(1.5D, Float64Value(1.5D)),
        new((float32)2.5F, Float64Value(2.5D)),
        new((@string)"s", StringValue("s")),
        new(true, BoolValue(true)),
        new(testTime, TimeValue(testTime)),
        new(time_package.ΔHour, DurationValue(time_package.ΔHour)),
        new(slice<global::go.log.slog_package.Attr>([Int("i", 3)]), GroupValue(Int("i", 3))),
        new(IntValue(4), IntValue(4)),
        new((nuint)2, Uint64Value(2)),
        new((uint8)3, Uint64Value(3)),
        new((uint16)4, Uint64Value(4)),
        new((uint32)5, Uint64Value(5)),
        new((uint64)6, Uint64Value(6)),
        new((uintptr)7, Uint64Value(7)),
        new((int8)8, Int64Value(8)),
        new((int16)9, Int64Value(9)),
        new((int32)10, Int64Value(10)),
        new((int64)11, Int64Value(11))
    ]))) {
        var got = AnyValue(test.@in);
        if (!got.Equal(test.want)) {
            t.Errorf("%v (%[1]T): got %v (kind %s), want %v (kind %s)",
                test.@in, got, got.Kind(), test.want, test.want.Kind());
        }
    }
}

public static void TestValueAny(ж<testing.T> t) {
    foreach (var (_, want) in range(slice<any>([
        default,
        LevelDebug + 100,
        time_package.ΔUTC.OrTypedNil(), // time.Locations treated specially...
        KindBool,                       // ...as are Kinds
        slice<global::go.log.slog_package.Attr>([Int("a", 1)]),
        (int64)2,
        (uint64)3,
        true,
        time_package.ΔMinute,
        new time_package.Time(),
        3.14D,
        fooˢ
    ]))) {
        var v = AnyValue(want);
        var got = v.Any();
        if (!reflect.DeepEqual(got, want)) {
            t.Errorf("got %v, want %v", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string replacedˢ => ref replacedˢᵇ;
private static readonly @string replacedˢᵇ = "replaced"u8;
internal static ref readonly @string testLogValueˢ => ref testLogValueˢᵇ;
private static readonly @string testLogValueˢᵇ = "TestLogValue"u8;

public static void TestLogValue(ж<testing.T> t) {
    @string want = replacedˢ;
    var r = Ꮡ(new replace(StringValue(want)));
    var v = AnyValue(r.OrTypedNil());
    global::go.log.slog_package.ΔKind g = v.Kind();
    global::go.log.slog_package.ΔKind w = KindLogValuer; if (g != w) {
        t.Errorf("got %s, want %s", g, w);
    }
    var got = v.LogValuer().LogValue().Any();
    if (!AreEqual(got, want)) {
        t.Errorf("got %#v, want %#v", got, want);
    }

    // Test Resolve.
    got = v.Resolve().Any();
    if (!AreEqual(got, want)) {
        t.Errorf("got %#v, want %#v", got, want);
    }

    // Test Resolve max iteration.
    r.v = AnyValue(r.OrTypedNil()); // create a cycle
    got = AnyValue(r.OrTypedNil()).Resolve().Any();
    var (_, okΔ1) = got._<error>(ᐧ); if (!okΔ1) {
        t.Errorf("expected error, got %T", got);
    }

    // Groups are not recursively resolved.
    var c = go.log.slog_package.Any("c", Ꮡ(new replace(StringValue("d"))));
    v = AnyValue(Ꮡ(new replace(GroupValue(Int("a", 1), Group("b", c)))));
    var got2 = v.Resolve().Any()._<slice<global::go.log.slog_package.Attr>>();
    var want2 = slice<global::go.log.slog_package.Attr>([Int("a", 1), Group("b", c)]);
    if (!attrsEqual(got2, want2)) {
        t.Errorf("got %v, want %v", got2, want2);
    }

    // Verify that panics in Resolve are caught and turn into errors.
    v = AnyValue(new panickingLogValue());
    got = v.Resolve().Any();
    var (gotErr, ok) = got._<error>(ᐧ);
    if (!ok) {
        t.Errorf("expected error, got %T", got);
    }
    // The error should provide some context information.
    // We'll just check that this function name appears in it.
    @string gotΔ1 = gotErr.Error();
    @string wantΔ1 = testLogValueˢ; if (!strings.Contains(gotΔ1, wantΔ1)) {
        t.Errorf("got %q, want substring %q", gotΔ1, wantΔ1);
    }
}

public static void TestValueTime(ж<testing.T> t) {
    // Validate that all representations of times work correctly.
    foreach (var (_, tm) in range(slice<time.Time>([
        new time_package.Time(),
        time_package.Unix(0, 1000000000000000), // UnixNanos is defined
        time_package.Date(2300, 1, 1, 0, 0, 0, 0, time_package.ΔUTC) // overflows UnixNanos
    ]))) {
        var got = TimeValue(tm).Time();
        if (!got.Equal(tm)) {
            t.Errorf("got %s (%#[1]v), want %s (%#[2]v)", got, tm);
        }
        var (g, w) = (got.Location(), tm.Location()); if (g != w) {
            t.Errorf("%s: location: got %v, want %v", tm, g.OrTypedNil(), w.OrTypedNil());
        }
    }
}

public static void TestEmptyGroup(ж<testing.T> t) {
    var g = GroupValue(
        Int("a", 1),
        Group("g1", Group("g2")),
        Group("g3", Group("g4", Int("b", 2))));
    var got = g.Group();
    var want = slice<global::go.log.slog_package.Attr>([Int("a", 1), Group("g3", Group("g4", Int("b", 2)))]);
    if (!attrsEqual(got, want)) {
        t.Errorf("\ngot  %v\nwant %v", got, want);
    }
}

internal partial struct replace {
    internal global::go.log.slog_package.Value v;
}

internal static global::go.log.slog_package.Value LogValue(this ref replace r) { return r.v; }

internal partial struct panickingLogValue {
}

internal static global::go.log.slog_package.Value LogValue(this panickingLogValue _) { throw panic("bad"); }

// A Value with "unsafe" strings is significantly faster:
// safe:  1785 ns/op, 0 allocs
// unsafe: 690 ns/op, 0 allocs

// Run this with and without -tags unsafe_kvs to compare.
public static void BenchmarkUnsafeStrings(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var dst = new slice<global::go.log.slog_package.Value>(100);
    var src = new slice<global::go.log.slog_package.Value>(len(dst));
    Ꮡb.Logf("Value size = %d", /* unsafe.Sizeof(Value{}) */ (uintptr)24);
    foreach (var i in range(len(src))) {
        src[i] = StringValue(fmt.Sprintf("string#%d"u8, i));
    }
    b.ResetTimer();
    @string d = default;
    for (nint i = 0; i < b.N; i++) {
        copy(dst, src);
        foreach (var (_, a) in range(dst)) {
            d = a.String();
        }
    }
    _ = d;
}

} // end slog_internal_test_package
