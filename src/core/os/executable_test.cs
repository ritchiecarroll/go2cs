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
internal static readonly @string helperEnvVarᶜ = "OSTEST_OUTPUT_EXECPATH"u8;

public static void TestExecutable(ж<Δtesting.T> Ꮡt) {
    @string helperEnvVar = helperEnvVarᶜ;
    if (Δos.Getenv(helperEnvVar) != ""u8) {
        // First chdir to another path.
        @string dirΔ1 = "/"u8;
        if (Δruntime.GOOS == "windows"u8) {
            var (cwd, errΔ1) = Δos.Getwd();
            if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
            dirΔ1 = filepath.VolumeName(cwd);
        }
        Δos.Chdir(dirΔ1);
        {
            var (epΔ1, errΔ2) = Δos.Executable(); if (errΔ2 != default!){
                fmt.Fprint(new Δos.FileжWriter(Δos.Stderr), errorˢ, errΔ2);
            } else {
                fmt.Fprint(new Δos.FileжWriter(Δos.Stderr), epΔ1);
            }
        }
        Δos.Exit(0);
    }
    Ꮡt.Parallel();
    @string ep = testenv.Executable(new os_test_package.testing_TжTB(Ꮡt));
    // we want fn to be of the form "dir/prog"
    @string dir = filepath.Dir(filepath.Dir(ep));
    var (fn, err) = filepath.Rel(dir, ep);
    if (err != default!) {
        Ꮡt.Fatalf("filepath.Rel: %v"u8, err);
    }
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), fn, "-test.run=^"u8 + Ꮡt.Name() + "$"u8);
    // make child start with a relative program path
    cmd.Value.Dir = dir;
    cmd.Value.Path = fn;
    if (Δruntime.GOOS == "openbsd"u8 || Δruntime.GOOS == "aix"u8){
    } else {
        // OpenBSD and AIX rely on argv[0]
        // forge argv[0] for child, so that we can verify we could correctly
        // get real path of the executable without influenced by argv[0].
        cmd.Value.Args[0] = "-"u8;
    }
    cmd.Value.Env = append(cmd.Environ(), fmt.Sprintf("%s=1"u8, helperEnvVar));
    (var @out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("exec(self) failed: %v"u8, err);
    }
    @string outs = ((@string)@out);
    if (!filepath.IsAbs(outs)) {
        Ꮡt.Fatalf("Child returned %q, want an absolute path"u8, @out);
    }
    if (!sameFile(outs, ep)) {
        Ꮡt.Fatalf("Child returned %q, not the same file as %q"u8, @out, ep);
    }
}

internal static bool sameFile(@string fn1, @string fn2) {
    var (fi1, err) = Δos.Stat(fn1);
    if (err != default!) {
        return false;
    }
    (var fi2, err) = Δos.Stat(fn2);
    if (err != default!) {
        return false;
    }
    return Δos.SameFile(fi1, fi2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdelGoˢ = "testdel.go"u8;
internal static readonly @string testdelExeˢ = "testdel.exe"u8;
internal static readonly @string buildˢ = "build"u8;

public static void TestExecutableDeleted(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveGoBuild(new os_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("%v does not support deleting running binary"u8, Δruntime.GOOS);
    }
    else if (exprᴛ1 == "openbsd"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "aix"u8) {
        Ꮡt.Skipf("%v does not support reading deleted binary name"u8, Δruntime.GOOS);
    }

    Ꮡt.Parallel();
    @string dir = Ꮡt.TempDir();
    @string src = filepath.Join(dir, testdelGoˢ);
    @string exe = filepath.Join(dir, testdelExeˢ);
    var err = Δos.WriteFile(src, slice<byte>(testExecutableDeletion), 438);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var @out, err) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new os_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, src).CombinedOutput();
    Ꮡt.Logf("build output:\n%s"u8, @out);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (@out, err) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), exe).CombinedOutput();
    Ꮡt.Logf("exec output:\n%s"u8, @out);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal static readonly @string testExecutableDeletion = """
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

"""u8;

} // end os_test_package
