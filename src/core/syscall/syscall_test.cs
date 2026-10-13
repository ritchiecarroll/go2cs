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

internal static void testSetGetenv(ж<testing.T> Ꮡt, @string key, @string value) {
    var err = syscall.Setenv(key, value);
    if (err != default!) {
        Ꮡt.Fatalf("Setenv failed to set %q: %v"u8, value, err);
    }
    var (newvalue, found) = syscall.Getenv(key);
    if (!found) {
        Ꮡt.Fatalf("Getenv failed to find %v variable (want value %q)"u8, key, value);
    }
    if (newvalue != value) {
        Ꮡt.Fatalf("Getenv(%v) = %q; want %q"u8, key, newvalue, value);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testenvˢ = "TESTENV"u8;
internal static readonly @string avalueˢ = "AVALUE"u8;

public static void TestEnv(ж<testing.T> Ꮡt) {
    testSetGetenv(Ꮡt, testenvˢ, avalueˢ);
    // make sure TESTENV gets set to "", not deleted
    testSetGetenv(Ꮡt, testenvˢ, ""u8);
}

// Check that permuting child process fds doesn't interfere with
// reporting of fork/exec status. See Issue 14979.
public static void TestExecErrPermutedFds(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new syscall_test_package.testing_TжTB(Ꮡt));

    var attr = Ꮡ(new Δos.ProcAttr(Files: new ж<Δos.File>[]{Δos.Stdin, Δos.Stderr, Δos.Stdout}.slice()));
    var (_, err) = Δos.StartProcess("/"u8, new @string[]{"/"u8}.slice(), attr);
    if (err == default!) {
        Ꮡt.Fatalf("StartProcess of invalid program returned err = nil"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object secAndUsecBothZeroˢ = (@string)"Sec and Usec both zero"u8;

public static void TestGettimeofday(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "js"u8) {
        Ꮡt.Skip("not implemented on " + Δruntime.GOOS);
    }
    var tv = Ꮡ(new syscall.Timeval(nil));
    {
        var err = syscall.Gettimeofday(tv); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    if ((~tv).Sec == 0 && (~tv).Usec == 0) {
        Ꮡt.Fatal(secAndUsecBothZeroˢ);
    }
}

} // end syscall_test_package
