// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using Δtesting = testing_package;
using @internal;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.os;
using go.path;
using static go.os_internal_test_package;
using Δio = io_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorˢ = (@string)"ERROR: "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string helperEnvVarᶜ => ref helperEnvVarᶜᵇ;
private static readonly @string helperEnvVarᶜᵇ = "OSTEST_OUTPUT_EXECPATH"u8;

public static void TestExecutable(ж<Δtesting.T> t) {
    @string helperEnvVar = helperEnvVarᶜ;

    if (Δos.Getenv(helperEnvVar) != ""u8) {
        // First chdir to another path.
        @string dirΔ1 = "/";
        if (Δruntime.GOOS == "windows"u8) {
            var (cwd, errΔ1) = Δos.Getwd();
            if (errΔ1 != nil) {
                throw panic(errΔ1);
            }
            dirΔ1 = filepath.VolumeName(cwd);
        }
        Δos.Chdir(dirΔ1);
        {
            var (epΔ1, errΔ2) = Δos.Executable(); if (errΔ2 != nil) {
                fmt.Fprint(new Δos.FileжWriter(Δos.Stderr), errorˢ, errΔ2);
            } else {
                fmt.Fprint(new Δos.FileжWriter(Δos.Stderr), epΔ1);
            }
        }
        Δos.Exit(0);
    }

    t.Parallel();
    @string ep = testenv.Executable(new os_test_package.testing_TжTB(t));
    // we want fn to be of the form "dir/prog"
    @string dir = filepath.Dir(filepath.Dir(ep));
    var (fn, err) = filepath.Rel(dir, ep);
    if (err != nil) {
        t.Fatalf("filepath.Rel: %v", err);
    }

    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), fn, "-test.run=^"u8 + t.Name() + "$"u8);
    // make child start with a relative program path
    cmd.Dir = dir;
    cmd.Path = fn;
    if (Δruntime.GOOS == "openbsd"u8 || Δruntime.GOOS == "aix"u8) {
        // OpenBSD and AIX rely on argv[0]
    } else {
        // forge argv[0] for child, so that we can verify we could correctly
        // get real path of the executable without influenced by argv[0].
        cmd.Args[0] = "-";
    }
    cmd.Env = append(cmd.Environ(), fmt.Sprintf("%s=1"u8, helperEnvVar));
    (var @out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("exec(self) failed: %v", err);
    }
    @string outs = (@string)@out;
    if (!filepath.IsAbs(outs)) {
        t.Fatalf("Child returned %q, want an absolute path", @out);
    }
    if (!sameFile(outs, ep)) {
        t.Fatalf("Child returned %q, not the same file as %q", @out, ep);
    }
}

internal static bool sameFile(@string fn1, @string fn2) {
    var (fi1, err) = Δos.Stat(fn1);
    if (err != nil) {
        return false;
    }
    (var fi2, err) = Δos.Stat(fn2);
    if (err != nil) {
        return false;
    }
    return Δos.SameFile(fi1, fi2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdelGoˢ => ref testdelGoˢᵇ;
private static readonly @string testdelGoˢᵇ = "testdel.go"u8;
internal static ref readonly @string testdelExeˢ => ref testdelExeˢᵇ;
private static readonly @string testdelExeˢᵇ = "testdel.exe"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;

public static void TestExecutableDeleted(ж<Δtesting.T> t) {
    testenv.MustHaveGoBuild(new os_test_package.testing_TжTB(t));
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8: {
        t.Skipf("%v does not support deleting running binary", Δruntime.GOOS);
        break;
    }
    case {} when exprᴛ1 == "openbsd"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "aix"u8: {
        t.Skipf("%v does not support reading deleted binary name", Δruntime.GOOS);
        break;
    }}
    t.Parallel();

    @string dir = t.TempDir();

    @string src = filepath.Join(dir, testdelGoˢ);
    @string exe = filepath.Join(dir, testdelExeˢ);

    var err = Δos.WriteFile(src, slice<byte>(testExecutableDeletion), 438);
    if (err != nil) {
        t.Fatal(err);
    }

    (var @out, err) = testenv.Command(new os_test_package.testing_TжTB(t), testenv.GoToolPath(new os_test_package.testing_TжTB(t)), buildˢ, "-o", exe, src).CombinedOutput();
    t.Logf("build output:\n%s", @out);
    if (err != nil) {
        t.Fatal(err);
    }

    (@out, err) = testenv.Command(new os_test_package.testing_TжTB(t), exe).CombinedOutput();
    t.Logf("exec output:\n%s", @out);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static ref readonly @string testExecutableDeletion => ref testExecutableDeletionᵇ;
private static readonly @string testExecutableDeletionᵇ = """
package main

import (
	"fmt"
	"os"
)

func main() {
	before, err := os.Executable()
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to read executable name before deletion: %v\n", err)
		os.Exit(1)
	}

	err = os.Remove(before)
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to remove executable: %v\n", err)
		os.Exit(1)
	}

	after, err := os.Executable()
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to read executable name after deletion: %v\n", err)
		os.Exit(1)
	}

	if before != after {
		fmt.Fprintf(os.Stderr, "before and after do not match: %v != %v\n", before, after)
		os.Exit(1)
	}
}

"""u8.Raw();

} // end os_test_package
