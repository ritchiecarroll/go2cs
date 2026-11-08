// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using reflect = reflect_package;
using testing = testing_package;

partial class errors_test_package {

public static void TestJoinReturnsNil(ж<testing.T> t) {
    {
        var err = errors.Join(); if (err != nil) {
            t.Errorf("errors.Join() = %v, want nil", err);
        }
    }
    {
        var err = errors.Join((error)(default)); if (err != nil) {
            t.Errorf("errors.Join(nil) = %v, want nil", err);
        }
    }
    {
        var err = errors.Join((error)(default), (error)(default)); if (err != nil) {
            t.Errorf("errors.Join(nil, nil) = %v, want nil", err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string err3ˢ => ref err3ˢᵇ;
private static readonly @string err3ˢᵇ = "err3"u8;

internal partial struct TestJoin_type /*dyn*/ {
    internal slice<error> errs;
    internal slice<error> want;
}

public static void TestJoin(ж<testing.T> t) {
    var err1 = errors.New(err1ˢ);
    var err2 = errors.New(err2ˢ);
    var merr = new multiErr(new error[]{errors.New(err3ˢ)}.slice());
    foreach (var (_, test) in range(slice<TestJoin_type>([new(
        errs: [err1],
        want: [err1]
    ), new(
        errs: [err1, err2],
        want: [err1, err2]
    ), new(
        errs: [err1, default, err2],
        want: [err1, err2]
    ), new(
        errs: [merr],
        want: [merr]
    )
    ]))) {
        var got = errors.Join(test.errs.ꓸꓸꓸ)._<ExampleJoin_type>().Unwrap();
        if (!reflect.DeepEqual(got, test.want)) {
            t.Errorf("Join(%v) = %v; want %v", test.errs, got, test.want);
        }
        if (len(got) != cap(got)) {
            t.Errorf("Join(%v) returns errors with len=%v, cap=%v; want len==cap", test.errs, len(got), cap(got));
        }
    }
}

internal partial struct TestJoinErrorMethod_type /*dyn*/ {
    internal slice<error> errs;
    internal @string want;
}

public static void TestJoinErrorMethod(ж<testing.T> t) {
    var err1 = errors.New(err1ˢ);
    var err2 = errors.New(err2ˢ);
    foreach (var (_, test) in range(slice<TestJoinErrorMethod_type>([new(
        errs: [err1],
        want: "err1"
    ), new(
        errs: [err1, err2],
        want: "err1\nerr2"
    ), new(
        errs: [err1, default, err2],
        want: "err1\nerr2"
    )
    ]))) {
        @string got = errors.Join(test.errs.ꓸꓸꓸ).Error();
        if (got != test.want) {
            t.Errorf("Join(%v).Error() = %q; want %q", test.errs, got, test.want);
        }
    }
}

} // end errors_test_package
