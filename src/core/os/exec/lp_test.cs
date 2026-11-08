// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using testing = testing_package;
using static go.os.exec_package;

partial class exec_internal_test_package {

internal static slice<@string> nonExistentPaths = [
    "some-non-existent-path",
    "non-existent-path/slashed"
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lookPathErrorIsNotAnExecˢ = (@string)"LookPath error is not an exec.Error"u8;

public static void TestLookPathNotFound(ж<testing.T> t) {
    foreach (var (_, name) in range(nonExistentPaths)) {
        var (path, err) = LookPath(name);
        if (err == nil) {
            t.Fatalf("LookPath found %q in $PATH", name);
        }
        if (path != ""u8) {
            t.Fatalf("LookPath path == %q when err != nil", path);
        }
        var (perr, ok) = err._<ж<global::go.os.exec_package.ΔError>>(ᐧ);
        if (!ok) {
            t.Fatal(lookPathErrorIsNotAnExecˢ);
        }
        if (perr.Name != name) {
            t.Fatalf("want Error name %q, got %q", name, perr.Name);
        }
    }
}

} // end exec_internal_test_package
