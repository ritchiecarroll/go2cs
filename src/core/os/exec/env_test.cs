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

public static void TestDedupEnv(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var tests = new TestDedupEnv_tests[]{
        new(
            noCase: true,
            @in: new @string[]{"k1=v1"u8, "k2=v2"u8, "K1=v3"u8}.slice(),
            want: new @string[]{"k2=v2"u8, "K1=v3"u8}.slice()
        ),
        new(
            noCase: false,
            @in: new @string[]{"k1=v1"u8, "K1=V2"u8, "k1=v3"u8}.slice(),
            want: new @string[]{"K1=V2"u8, "k1=v3"u8}.slice()
        ),
        new(
            @in: new @string[]{"=a"u8, "=b"u8, "foo"u8, "bar"u8}.slice(),
            want: new @string[]{"=b"u8, "foo"u8, "bar"u8}.slice()
        ),
        new(
            noCase: true, // #49886: preserve weird Windows keys with leading "=" signs.

            @in: new @string[]{@"=C:=C:\golang"u8, @"=D:=D:\tmp"u8, @"=D:=D:\"u8}.slice(),
            want: new @string[]{@"=C:=C:\golang"u8, @"=D:=D:\"u8}.slice()
        ),
        new(
            @in: new @string[]{ // #52436: preserve invalid key-value entries (for now).
 // (Maybe filter them out or error out on them at some point.)
"dodgy"u8, "entries"u8}.slice(),
            want: new @string[]{"dodgy"u8, "entries"u8}.slice()
        ),
        new(
            @in: new @string[]{ // Filter out entries containing NULs.
((@string)(new byte[]{0x41, 0x3d, 0x61, 0x00, 0x62})), "B=b"u8, ((@string)(new byte[]{0x43, 0x00, 0x43, 0x3d, 0x63}))}.slice(),
            want: new @string[]{"B=b"u8}.slice(),
            wantErr: true
        ),
        new(
            nulOK: true, // Plan 9 needs to preserve environment variables with NUL (#56544).

            @in: new @string[]{"path=one\x00two"u8}.slice(),
            want: new @string[]{"path=one\x00two"u8}.slice()
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var (got, err) = dedupEnvCase(tt.noCase, tt.nulOK, tt.@in);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want) || (err != default!) != tt.wantErr) {
            Ꮡt.Errorf("Dedup(%v, %q) = %q, %v; want %q, error:%v"u8, tt.noCase, tt.@in, got, err, tt.want, tt.wantErr);
        }
    }
}

} // end exec_internal_test_package
