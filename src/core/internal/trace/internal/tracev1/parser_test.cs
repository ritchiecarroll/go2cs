// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace.@internal;

using bytes = bytes_package;
using version = go.@internal.trace.version_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.@internal.trace;
using go.io;
using io = io_package;
using path;
using static go.@internal.trace.@internal.tracev1_package;

partial class tracev1_internal_test_package {

public static void TestCorruptedInputs(ж<testing.T> t) {
    // These inputs crashed parser previously.
    var tests = slice<@string>([
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x51, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x54, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0xc3, 0x02, 0x30, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x51, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x54, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0xc3, 0x02, 0x30, 0x30}))
    ]);
    foreach (var (_, data) in tests) {
        var (res, err) = Parse(new tracev1_internal_test_package.strings_ReaderжReader(strings.NewReader(data)), 5);
        if (err == nil || res.Events.Len() != 0 || res.Stacks != nil) {
            t.Fatalf("no error on input: %q", data);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "./testdata"u8;
internal static ref readonly @string goodˢ => ref goodˢᵇ;
private static readonly @string goodˢᵇ = "_good"u8;
internal static ref readonly @string unorderedˢ => ref unorderedˢᵇ;
private static readonly @string unorderedˢᵇ = "_unordered"u8;

public static void TestParseCanned(ж<testing.T> t) {
    var (files, err) = os.ReadDir(testdataˢ);
    if (err != nil) {
        t.Fatalf("failed to read ./testdata: %v", err);
    }
    foreach (var (_, f) in files) {
        var (info, errΔ1) = f.Info();
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        if (testing.Short() && info.Size() > 10000) {
            continue;
        }
        @string name = filepath.Join(testdataˢ, f.Name());
        (var data, errΔ1) = os.ReadFile(name);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var r = bytes.NewReader(data);
        (var v, errΔ1) = version.ReadHeader(new tracev1_internal_test_package.bytes_ReaderжReader(r));
        if (errΔ1 != nil) {
            t.Errorf("failed to parse good trace %s: %s", f.Name(), errΔ1);
        }
        (var trace, errΔ1) = Parse(new tracev1_internal_test_package.bytes_ReaderжReader(r), v);
        switch (ᐧ) {
        case {} when strings.HasSuffix(f.Name(), goodˢ): {
            if (errΔ1 != nil) {
                t.Errorf("failed to parse good trace %v: %v", f.Name(), errΔ1);
            }
            checkTrace(t, (nint)v, trace);
            break;
        }
        case {} when strings.HasSuffix(f.Name(), unorderedˢ): {
            if (!AreEqual(errΔ1, ErrTimeOrder)) {
                t.Errorf("unordered trace is not detected %v: %v", f.Name(), errΔ1);
            }
            break;
        }
        default: {
            t.Errorf("unknown input file suffix: %v", f.Name());
            break;
        }}
    }
}

// checkTrace walks over a good trace and makes a bunch of additional checks
// that may not cause the parser to outright fail.
internal static void checkTrace(ж<testing.T> t, nint ver, global::go.@internal.trace.@internal.tracev1_package.Trace res) {
    for (nint i = 0; i < res.Events.Len(); i++) {
        var ev = res.Events.Ptr(i);
        if (ver >= 21) {
            if (ev.Type == EvSTWStart && res.Strings[ev.Args[0]] == "unknown") {
                t.Errorf("found unknown STW event; update stwReasonStrings?");
            }
        }
    }
}

public static void TestBuckets(ж<testing.T> t) {
    ref var evs = ref heap(new global::go.@internal.trace.@internal.tracev1_package.Events(), out var Ꮡevs);

    UntypedInt N = /* eventsBucketSize*3 + 123 */ 1572987;
    for (nint i = 0; i < N; i++) {
        evs.append(new Event(Ts: ((global::go.@internal.trace.@internal.tracev1_package.Timestamp)(int64)i)));
    }

    {
        nint nΔ1 = len(evs.buckets); if (nΔ1 != 4) {
            t.Fatalf("got %d buckets, want %d", nΔ1, (nint)4);
        }
    }

    {
        nint nΔ2 = evs.Len(); if (nΔ2 != N) {
            t.Fatalf("got %d events, want %d", nΔ2, (nint)(N));
        }
    }

    nint n = default;
    Ꮡevs.All()((ж<global::go.@internal.trace.@internal.tracev1_package.Event> evΔ1) => {
        n++;
        return true;
    });
    if (n != N) {
        t.Fatalf("iterated over %d events, expected %d", n, (nint)(N));
    }

    UntypedInt consume = /* eventsBucketSize + 50 */ 524338;
    for (nint i = 0; i < consume; i++) {
        {
            var (_, ok) = evs.Pop(); if (!ok) {
                t.Fatalf("iteration failed after %d events", i);
            }
        }
    }

    if (evs.buckets[0] != nil) {
        t.Fatalf("expected first bucket to have been dropped");
    }
    foreach (var (i, b) in evs.buckets[1..]) {
        if (b == nil) {
            t.Fatalf("expected bucket %d to be non-nil", i + 1);
        }
    }

    {
        nint nΔ3 = evs.Len(); if (nΔ3 != (nint)(N - consume)) {
            t.Fatalf("got %d remaining elements, expected %d", nΔ3, (nint)(N - consume));
        }
    }

    var ev = evs.Ptr(0);
    if (ev.Ts != consume) {
        t.Fatalf("got event %d, expected %d", (nint)ev.Ts, (nint)(consume));
    }

    for (;;) {
        var (_, ok) = evs.Pop();
        if (!ok) {
            break;
        }
    }

    foreach (var (i, b) in evs.buckets) {
        if (b != nil) {
            t.Fatalf("expected bucket %d to be nil", i);
        }
    }
}

} // end tracev1_internal_test_package
