// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.build;

using fmt = fmt_package;
using testing = testing_package;
using static global::go.go.build.constraint_package;

partial class constraint_internal_test_package {


partial struct testsᴛ1 /*dyn*/ {
    internal @string @in;
    internal nint @out;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new("//go:build linux && go1.60"u8, 60),
    new("//go:build ignore && go1.60"u8, 60),
    new("//go:build ignore || go1.60"u8, -1),
    new("//go:build go1.50 || (ignore && go1.60)"u8, 50),
    new("// +build go1.60,linux"u8, 60),
    new("// +build go1.60 linux"u8, -1),
    new("//go:build go1.50 && !go1.60"u8, 50),
    new("//go:build !go1.60"u8, -1),
    new("//go:build linux && go1.50 || darwin && go1.60"u8, 50),
    new("//go:build linux && go1.50 || !(!darwin || !go1.60)"u8, 50)
}.slice();

public static void TestGoVersion(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in tests) {
        var (x, err) = Parse(tt.@in);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string v = GoVersion(x);
        @string want = ""u8;
        if (tt.@out == 0){
            want = go1ˢ;
        } else 
        if (tt.@out > 0) {
            want = fmt.Sprintf("go1.%d"u8, tt.@out);
        }
        if (v != want) {
            Ꮡt.Errorf("GoVersion(%q) = %q, want %q, nil"u8, tt.@in, v, want);
        }
    }
}

} // end constraint_internal_test_package
