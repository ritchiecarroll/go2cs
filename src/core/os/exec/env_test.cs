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
            @in: ["k1=v1", "k2=v2", "K1=v3"],
            want: ["k2=v2", "K1=v3"]
        ),
        new(
            noCase: false,
            @in: ["k1=v1", "K1=V2", "k1=v3"],
            want: ["K1=V2", "k1=v3"]
        ),
        new(
            @in: ["=a", "=b", "foo", "bar"],
            want: ["=b", "foo", "bar"]
        ),
        new(
            // #49886: preserve weird Windows keys with leading "=" signs.
            noCase: true,
            @in: [@"=C:=C:\golang", @"=D:=D:\tmp", @"=D:=D:\"],
            want: [@"=C:=C:\golang", @"=D:=D:\"]
        ),
        new(
            // #52436: preserve invalid key-value entries (for now).
            // (Maybe filter them out or error out on them at some point.)
            @in: ["dodgy", "entries"],
            want: ["dodgy", "entries"]
        ),
        new(
            // Filter out entries containing NULs.
            @in: [((@string)(new byte[]{0x41, 0x3d, 0x61, 0x00, 0x62})), "B=b", ((@string)(new byte[]{0x43, 0x00, 0x43, 0x3d, 0x63}))],
            want: ["B=b"],
            wantErr: true
        ),
        new(
            // Plan 9 needs to preserve environment variables with NUL (#56544).
            nulOK: true,
            @in: ["path=one\x00two"],
            want: ["path=one\x00two"]
        )
    ]);
    foreach (var (_, tt) in range(tests)) {
        var (got, err) = dedupEnvCase(tt.noCase, tt.nulOK, tt.@in);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want) || (err != nil) != tt.wantErr) {
            t.Errorf("Dedup(%v, %q) = %q, %v; want %q, error:%v", tt.noCase, tt.@in, got, err, tt.want, tt.wantErr);
        }
    }
}

} // end exec_internal_test_package
