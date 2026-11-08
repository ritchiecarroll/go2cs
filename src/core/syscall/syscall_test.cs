// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using static go.syscall_internal_test_package;

partial class syscall_test_package {

internal static void testSetGetenv(ж<testing.T> t, @string key, @string value) {
    var err = syscall.Setenv(key, value);
    if (err != nil) {
        t.Fatalf("Setenv failed to set %q: %v", value, err);
    }
    var (newvalue, found) = syscall.Getenv(key);
    if (!found) {
        t.Fatalf("Getenv failed to find %v variable (want value %q)", key, value);
    }
    if (newvalue != value) {
        t.Fatalf("Getenv(%v) = %q; want %q", key, newvalue, value);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testenvˢ => ref testenvˢᵇ;
private static readonly @string testenvˢᵇ = "TESTENV"u8;
internal static ref readonly @string avalueˢ => ref avalueˢᵇ;
private static readonly @string avalueˢᵇ = "AVALUE"u8;

public static void TestEnv(ж<testing.T> t) {
    testSetGetenv(t, testenvˢ, avalueˢ);
    // make sure TESTENV gets set to "", not deleted
    testSetGetenv(t, testenvˢ, ""u8);
}

// Check that permuting child process fds doesn't interfere with
// reporting of fork/exec status. See Issue 14979.
public static void TestExecErrPermutedFds(ж<testing.T> t) {
    testenv.MustHaveExec(new syscall_test_package.testing_TжTB(t));

    var attr = Ꮡ(new Δos.ProcAttr(Files: [Δos.Stdin, Δos.Stderr, Δos.Stdout]));
    var (_, err) = Δos.StartProcess("/", ["/"], attr);
    if (err == nil) {
        t.Fatalf("StartProcess of invalid program returned err = nil");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object secAndUsecBothZeroˢ = (@string)"Sec and Usec both zero"u8;

public static void TestGettimeofday(ж<testing.T> t) {
    if (Δruntime.GOOS == "js"u8) {
        t.Skip("not implemented on " + Δruntime.GOOS);
    }
    var tv = Ꮡ(new syscall.Timeval());
    var err = syscall.Gettimeofday(tv); if (err != nil) {
        t.Fatal(err);
    }
    if (tv.Sec == 0 && tv.Usec == 0) {
        t.Fatal(secAndUsecBothZeroˢ);
    }
}

} // end syscall_test_package
