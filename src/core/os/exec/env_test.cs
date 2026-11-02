// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using slices = slices_package;
using testing = testing_package;
using static go.os.exec_package;

partial class exec_internal_test_package {

internal partial struct TestDedupEnv_tests /*dyn*/ {
    internal bool noCase;
    internal bool nulOK;
    internal slice<@string> @in;
    internal slice<@string> want;
    internal bool wantErr;
}

public static void TestDedupEnv(ж<testing.T> t) {
    t.Parallel();

    var tests = slice<TestDedupEnv_tests>([
        new(
            noCase: true,
            @in: slice<@string>(["k1=v1", "k2=v2", "K1=v3"]),
            want: slice<@string>(["k2=v2", "K1=v3"])
        ),
        new(
            noCase: false,
            @in: slice<@string>(["k1=v1", "K1=V2", "k1=v3"]),
            want: slice<@string>(["K1=V2", "k1=v3"])
        ),
        new(
            @in: slice<@string>(["=a", "=b", "foo", "bar"]),
            want: slice<@string>(["=b", "foo", "bar"])
        ),
        new(
            // #49886: preserve weird Windows keys with leading "=" signs.
            noCase: true,
            @in: slice<@string>([@"=C:=C:\golang", @"=D:=D:\tmp", @"=D:=D:\"]),
            want: slice<@string>([@"=C:=C:\golang", @"=D:=D:\"])
        ),
        new(
            // #52436: preserve invalid key-value entries (for now).
            // (Maybe filter them out or error out on them at some point.)
            @in: slice<@string>(["dodgy", "entries"]),
            want: slice<@string>(["dodgy", "entries"])
        ),
        new(
            // Filter out entries containing NULs.
            @in: slice<@string>([((@string)(new byte[]{0x41, 0x3d, 0x61, 0x00, 0x62})), "B=b", ((@string)(new byte[]{0x43, 0x00, 0x43, 0x3d, 0x63}))]),
            want: slice<@string>(["B=b"]),
            wantErr: true
        ),
        new(
            // Plan 9 needs to preserve environment variables with NUL (#56544).
            nulOK: true,
            @in: slice<@string>(["path=one\x00two"]),
            want: slice<@string>(["path=one\x00two"])
        )
    ]);
    foreach (var (_, tt) in tests) {
        var (got, err) = dedupEnvCase(tt.noCase, tt.nulOK, tt.@in);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want) || (err != nil) != tt.wantErr) {
            t.Errorf("Dedup(%v, %q) = %q, %v; want %q, error:%v", tt.noCase, tt.@in, got, err, tt.want, tt.wantErr);
        }
    }
}

} // end exec_internal_test_package
