// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Process etc.
namespace go;

using testlog = @internal.testlog_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class os_package {

// Args hold the command-line arguments, starting with the program name.
public static slice<@string> Args;

[GoInit] internal static void initΔ1() {
    if (runtime.GOOS == "windows"u8) {
        // Initialized in exec_windows.go.
        return;
    }
    Args = runtime_args();
}

[global::System.Diagnostics.StackTraceHidden] internal static slice<@string> runtime_args() {
    return runtime.os_runtime_args();
}

// in package runtime

// Getuid returns the numeric user id of the caller.
//
// On Windows, it returns -1.
public static nint Getuid() {
    return syscall.Getuid();
}

// Geteuid returns the numeric effective user id of the caller.
//
// On Windows, it returns -1.
public static nint Geteuid() {
    return syscall.Geteuid();
}

// Getgid returns the numeric group id of the caller.
//
// On Windows, it returns -1.
public static nint Getgid() {
    return syscall.Getgid();
}

// Getegid returns the numeric effective group id of the caller.
//
// On Windows, it returns -1.
public static nint Getegid() {
    return syscall.Getegid();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getgroupsˢ = "getgroups"u8;

// Getgroups returns a list of the numeric ids of groups that the caller belongs to.
//
// On Windows, it returns [syscall.EWINDOWS]. See the [os/user] package
// for a possible alternative.
public static (slice<nint>, error) Getgroups() {
    var (gids, e) = syscall.Getgroups();
    return (gids, NewSyscallError(getgroupsˢ, e));
}

// Exit causes the current program to exit with the given status code.
// Conventionally, code zero indicates success, non-zero an error.
// The program terminates immediately; deferred functions are not run.
//
// For portability, the status code should be in the range [0, 125].
public static void Exit(nint code) {
    if (code == 0 && testlog.PanicOnExit0()) {
        // We were told to panic on calls to os.Exit(0).
        // This is used to fail tests that make an early
        // unexpected call to os.Exit(0).
        throw panic("unexpected call to os.Exit(0) during test");
    }
    // Inform the runtime that os.Exit is being called. If -race is
    // enabled, this will give race detector a chance to fail the
    // program (racy programs do not have the right to finish
    // successfully). If coverage is enabled, then this call will
    // enable us to write out a coverage data file.
    runtime_beforeExit(code);
    syscall.Exit(code);
}

internal static partial void runtime_beforeExit(nint exitCode);

// implemented in runtime

} // end os_package
