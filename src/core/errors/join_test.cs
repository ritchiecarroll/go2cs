// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using reflect = reflect_package;
using testing = testing_package;

partial class errors_test_package {

public static void TestJoinReturnsNil(ж<testing.T> Ꮡt) {
    {
        var err = errors.Join(); if (err != default!) {
            Ꮡt.Errorf("errors.Join() = %v, want nil"u8, err);
        }
    }
    {
        var err = errors.Join((error)(default!)); if (err != default!) {
            Ꮡt.Errorf("errors.Join(nil) = %v, want nil"u8, err);
        }
    }
    {
        var err = errors.Join((error)(default!), (error)(default!)); if (err != default!) {
            Ꮡt.Errorf("errors.Join(nil, nil) = %v, want nil"u8, err);
        }
    }
}

internal partial struct TestJoin_type /*dyn*/ {
    internal slice<error> errs;
    internal slice<error> want;
}

internal partial interface TestJoin_typeᴛ1 /*dyn*/ {
    slice<error> Unwrap();
}

public static void TestJoin(ж<testing.T> Ꮡt) {
    var err1 = errors.New(err1ˢ);
    var err2 = errors.New(err2ˢ);
    foreach (var (_, test) in new TestJoin_type[]{new(
        errs: new error[]{err1}.slice(),
        want: new error[]{err1}.slice()
    ), new(
        errs: new error[]{err1, err2}.slice(),
        want: new error[]{err1, err2}.slice()
    ), new(
        errs: new error[]{err1, default!, err2}.slice(),
        want: new error[]{err1, err2}.slice()
    )
    }.slice()) {
        var got = errors.Join(test.errs.ꓸꓸꓸ)._<TestJoin_typeᴛ1>().Unwrap();
        if (!reflect.DeepEqual(got, test.want)) {
            Ꮡt.Errorf("Join(%v) = %v; want %v"u8, test.errs, got, test.want);
        }
        if (len(got) != cap(got)) {
            Ꮡt.Errorf("Join(%v) returns errors with len=%v, cap=%v; want len==cap"u8, test.errs, len(got), cap(got));
        }
    }
}

internal partial struct TestJoinErrorMethod_type /*dyn*/ {
    internal slice<error> errs;
    internal @string want;
}

public static void TestJoinErrorMethod(ж<testing.T> Ꮡt) {
    var err1 = errors.New(err1ˢ);
    var err2 = errors.New(err2ˢ);
    foreach (var (_, test) in new TestJoinErrorMethod_type[]{new(
        errs: new error[]{err1}.slice(),
        want: "err1"u8
    ), new(
        errs: new error[]{err1, err2}.slice(),
        want: "err1\nerr2"u8
    ), new(
        errs: new error[]{err1, default!, err2}.slice(),
        want: "err1\nerr2"u8
    )
    }.slice()) {
        @string got = errors.Join(test.errs.ꓸꓸꓸ).Error();
        if (got != test.want) {
            Ꮡt.Errorf("Join(%v).Error() = %q; want %q"u8, test.errs, got, test.want);
        }
    }
}

} // end errors_test_package
