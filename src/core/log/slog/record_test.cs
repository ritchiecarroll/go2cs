// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.log.slog_package;

partial class slog_internal_test_package {

public static void TestRecordAttrs(ж<testing.T> t) {
    var @as = slice<global::go.log.slog_package.Attr>([Int("k1", 1), go.log.slog_package.String("k2", fooˢ), Int("k3", 3),
        go.log.slog_package.Int64("k4", -1), go.log.slog_package.Float64("f", 3.1D), go.log.slog_package.Uint64("u", 999)]);
    var r = newRecordWithAttrs(@as);
    {
        nint g = r.NumAttrs();
        nint w = len(@as); if (g != w) {
            t.Errorf("NumAttrs: got %d, want %d", g, w);
        }
    }
    {
        var got = attrsSlice(r); if (!attrsEqual(got, @as)) {
            t.Errorf("got %v, want %v", got, @as);
        }
    }

    // Early return.
    // Hit both loops in Record.Attrs: front and back.
    foreach (var (_, stop) in slice<nint>([2, 6])) {
        ref var got = ref heap<slice<global::go.log.slog_package.Attr>>(out var Ꮡgot);
        r.Attrs((global::go.log.slog_package.Attr a) => {
            Ꮡgot.ValueSlot = builtin.append(Ꮡgot.ValueSlot, a);
            return len(Ꮡgot.ValueSlot) < stop;
        });
        var want = @as.slice(0, stop);
        if (!attrsEqual(got, want)) {
            t.Errorf("got %v, want %v", got, want);
        }
    }
}

internal partial struct TestRecordSource_type /*dyn*/ {
    internal nint depth;
    internal @string wantFunction;
    internal @string wantFile;
    internal bool wantLinePositive;
    internal bool wantNil;
}

public static void TestRecordSource(ж<testing.T> t) {
    // Zero call depth => nil *Source.
    foreach (var (_, test) in slice<TestRecordSource_type>([
        new(0, "", "", false, true),
        new(-16, "", "", false, true),
        new(1, "log/slog.TestRecordSource", "record_test.go", true, false), // 1: caller of NewRecord
        new(2, "testing.tRunner", "testing.go", true, false)
    ])) {
        uintptr pc = default;
        if (test.depth > 0) {
            pc = callerPC(test.depth + 1);
        }
        var r = NewRecord(new time_package.Time(), 0, ""u8, pc);
        var got = r.Source();
        if (test.wantNil) {
            if (got != nil) {
                t.Errorf("depth %d: got non-nil Source, want nil", test.depth);
            }
            continue;
        }
        if (got == nil) {
            t.Errorf("depth %d: got nil Source, want non-nil", test.depth);
            continue;
        }
        {
            nint i = strings.LastIndexByte(got.File, (byte)'/'); if (i >= 0) {
                got.File = got.File.slice(i + 1);
            }
        }
        if (got.Function != test.wantFunction || got.File != test.wantFile || (got.Line > 0) != test.wantLinePositive) {
            t.Errorf("depth %d: got (%q, %q, %d), want (%q, %q, %t)",
                test.depth,
                got.Function, got.File, got.Line,
                test.wantFunction, test.wantFile, test.wantLinePositive);
        }
    }
}

public static partial void TestAliasingAndClone(ж<testing.T> t) {
    slice<global::go.log.slog_package.Attr> intAttrs(nint from, nint to) {
        slice<global::go.log.slog_package.Attr> @as = default;
        for (nint i = from; i < to; i++) {
            @as = builtin.append(@as, Int("k", i));
        }
        return @as;
    }

    void check(global::go.log.slog_package.Record r, slice<global::go.log.slog_package.Attr> want) {
        r = r.ΔClone();
        t.Helper();
        var got = attrsSlice(r);
        if (!attrsEqual(got, want)) {
            t.Errorf("got %v, want %v", got, want);
        }
    }

    // Create a record whose Attrs overflow the inline array,
    // creating a slice in r.back.
    var r1 = NewRecord(new time_package.Time(), 0, ""u8, 0);
    r1.AddAttrs(intAttrs(0, nAttrsInline + 1).ꓸꓸꓸ);
    // Ensure that r1.back's capacity exceeds its length.
    var b = new slice<global::go.log.slog_package.Attr>(len(r1.back), len(r1.back) + 1);
    copy(b, r1.back);
    r1.back = b;
    // Make a copy that shares state.
    var r2 = r1.ΔClone();
    // Adding to both should insert a special Attr in the second.
    var r1AttrsBefore = attrsSlice(r1);
    r1.AddAttrs(Int("p", 0));
    r2.AddAttrs(Int("p", 1));
    check(r1, builtin.append(slices.Clip<slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr>(r1AttrsBefore), Int("p", 0)));
    var r1Attrs = attrsSlice(r1);
    check(r2, builtin.append(slices.Clip<slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr>(r1AttrsBefore),
        go.log.slog_package.String(bugˢ, addAttrsUnsafelyCalledOnˢ), Int("p", 1)));

    // Adding to a clone is fine.
    r2 = r1.Clone();
    check(r2, r1Attrs);
    r2.AddAttrs(Int("p", 2));
    check(r1, r1Attrs); // r1 is unchanged
    check(r2, builtin.append(slices.Clip<slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr>(r1Attrs), Int("p", 2)));
}

internal static global::go.log.slog_package.Record newRecordWithAttrs(slice<global::go.log.slog_package.Attr> @as) {
    var r = NewRecord(time_package.Now(), LevelInfo, ""u8, 0);
    r.AddAttrs(@as.ꓸꓸꓸ);
    return r.ΔClone();
}

internal static slice<global::go.log.slog_package.Attr> attrsSlice(global::go.log.slog_package.Record r) {
    r = r.ΔClone();

    ref var s = ref heap<slice<global::go.log.slog_package.Attr>>(new slice<global::go.log.slog_package.Attr>(0, r.NumAttrs()), out var Ꮡs);
    r.Attrs((global::go.log.slog_package.Attr a) => {
        Ꮡs.ValueSlot = builtin.append(Ꮡs.ValueSlot, a);
        return true;
    });
    return s;
}

internal static bool attrsEqual(slice<global::go.log.slog_package.Attr> as1, slice<global::go.log.slog_package.Attr> as2) {
    return slices.EqualFunc<slice<global::go.log.slog_package.Attr>, slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr, global::go.log.slog_package.Attr>(as1, as2, ((Func<global::go.log.slog_package.Attr, global::go.log.slog_package.Attr, bool>)(global::go.log.slog_package.Equal)));
}

// Currently, pc(2) takes over 400ns, which is too expensive
// to call it for every log message.
public static void BenchmarkPC(ж<testing.B> b) {
    for (nint depthᴛ1 = 0; depthᴛ1 < 5; depthᴛ1++) {
        var depth = depthᴛ1;
        b.Run(strconv.Itoa(depth), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            uintptr x = default;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                x = callerPC(depth);
            }
            _ = x;
        });
    }
}

public static void BenchmarkRecord(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint nAttrs = /* nAttrsInline * 10 */ 50;
    ref var a = ref heap(new global::go.log.slog_package.Attr(), out var Ꮡa);

    for (nint i = 0; i < b.N; i++) {
        var r = NewRecord(new time_package.Time(), LevelInfo, ""u8, 0);
        for (nint j = 0; j < nAttrs; j++) {
            r.AddAttrs(Int("k", j));
        }
        r.Attrs((global::go.log.slog_package.Attr bΔ1) => {
            Ꮡa.Value = bΔ1;
            return true;
        });
    }
    _ = a;
}

} // end slog_internal_test_package
