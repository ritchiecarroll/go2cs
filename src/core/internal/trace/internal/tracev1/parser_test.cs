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

public static void TestCorruptedInputs(ж<testing.T> Ꮡt) {
    // These inputs crashed parser previously.
    var tests = new @string[]{
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x51, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x54, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0xc3, 0x02, 0x30, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x51, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x54, 0x30, 0x30, 0x02, 0x30})),
        ((@string)(new byte[]{0x67, 0x6f, 0x20, 0x31, 0x2e, 0x35, 0x20, 0x74, 0x72, 0x61, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0xc3, 0x02, 0x30, 0x30}))
    }.slice();
    foreach (var (_, data) in tests) {
        var (res, err) = Parse(new tracev1_internal_test_package.strings_ReaderжReader(strings.NewReader(data)), 5);
        if (err == default! || res.Events.Len() != 0 || res.Stacks != default!) {
            Ꮡt.Fatalf("no error on input: %q"u8, data);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "./testdata"u8;
internal static readonly @string goodˢ = "_good"u8;
internal static readonly @string unorderedˢ = "_unordered"u8;

public static void TestParseCanned(ж<testing.T> Ꮡt) {
    var (files, err) = os.ReadDir(testdataˢ);
    if (err != default!) {
        Ꮡt.Fatalf("failed to read ./testdata: %v"u8, err);
    }
    foreach (var (_, f) in files) {
        var (info, errΔ1) = f.Info();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        if (testing.Short() && info.Size() > 10000) {
            continue;
        }
        @string name = filepath.Join(testdataˢ, f.Name());
        (var data, errΔ1) = os.ReadFile(name);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        var r = bytes.NewReader(data);
        (var v, errΔ1) = version.ReadHeader(new tracev1_internal_test_package.bytes_ReaderжReader(r));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("failed to parse good trace %s: %s"u8, f.Name(), errΔ1);
        }
        (var trace, errΔ1) = Parse(new tracev1_internal_test_package.bytes_ReaderжReader(r), v);
        switch (ᐧ) {
        case {} when strings.HasSuffix(f.Name(), goodˢ): {
            if (errΔ1 != default!) {
                Ꮡt.Errorf("failed to parse good trace %v: %v"u8, f.Name(), errΔ1);
            }
            checkTrace(Ꮡt, (nint)(uint32)v, trace);
            break;
        }
        case {} when strings.HasSuffix(f.Name(), unorderedˢ): {
            if (!AreEqual(errΔ1, ErrTimeOrder)) {
                Ꮡt.Errorf("unordered trace is not detected %v: %v"u8, f.Name(), errΔ1);
            }
            break;
        }
        default: {
            Ꮡt.Errorf("unknown input file suffix: %v"u8, f.Name());
            break;
        }}
    }
}

// checkTrace walks over a good trace and makes a bunch of additional checks
// that may not cause the parser to outright fail.
internal static void checkTrace(ж<testing.T> Ꮡt, nint ver, global::go.@internal.trace.@internal.tracev1_package.Trace res) {
    for (nint i = 0; i < res.Events.Len(); i++) {
        var ev = res.Events.Ptr(i);
        if (ver >= 21) {
            if ((~ev).Type == EvSTWStart && res.Strings[(~ev).Args[0]] == "unknown") {
                Ꮡt.Errorf("found unknown STW event; update stwReasonStrings?"u8);
            }
        }
    }
}

public static void TestBuckets(ж<testing.T> Ꮡt) {
    ref var evs = ref heap(new global::go.@internal.trace.@internal.tracev1_package.Events(), out var Ꮡevs);

    UntypedInt N = /* eventsBucketSize*3 + 123 */ 1572987;
    for (nint i = 0; i < N; i++) {
        evs.append(new Event(Ts: ((global::go.@internal.trace.@internal.tracev1_package.Timestamp)(int64)i)));
    }

    {
        nint nΔ1 = len(evs.buckets); if (nΔ1 != 4) {
            Ꮡt.Fatalf("got %d buckets, want %d"u8, nΔ1, (nint)4);
        }
    }

    {
        nint nΔ2 = evs.Len(); if (nΔ2 != N) {
            Ꮡt.Fatalf("got %d events, want %d"u8, nΔ2, (nint)(N));
        }
    }

    nint n = default!;
    Ꮡevs.All()((ж<global::go.@internal.trace.@internal.tracev1_package.Event> evΔ1) => {
        n++;
        return true;
    });
    if (n != N) {
        Ꮡt.Fatalf("iterated over %d events, expected %d"u8, n, (nint)(N));
    }

    UntypedInt consume = /* eventsBucketSize + 50 */ 524338;
    for (nint i = 0; i < consume; i++) {
        {
            var (_, ok) = evs.Pop(); if (!ok) {
                Ꮡt.Fatalf("iteration failed after %d events"u8, i);
            }
        }
    }

    if (evs.buckets[0] != nil) {
        Ꮡt.Fatalf("expected first bucket to have been dropped"u8);
    }
    foreach (var (i, b) in evs.buckets[1..]) {
        if (b == nil) {
            Ꮡt.Fatalf("expected bucket %d to be non-nil"u8, i + 1);
        }
    }

    {
        nint nΔ3 = evs.Len(); if (nΔ3 != (nint)(N - consume)) {
            Ꮡt.Fatalf("got %d remaining elements, expected %d"u8, nΔ3, (nint)(N - consume));
        }
    }

    var ev = evs.Ptr(0);
    if ((~ev).Ts != consume) {
        Ꮡt.Fatalf("got event %d, expected %d"u8, (nint)(int64)(~ev).Ts, (nint)(consume));
    }

    while (ᐧ) {
        var (_, ok) = evs.Pop();
        if (!ok) {
            break;
        }
    }

    foreach (var (i, b) in evs.buckets) {
        if (b != nil) {
            Ꮡt.Fatalf("expected bucket %d to be nil"u8, i);
        }
    }
}

} // end tracev1_internal_test_package
