// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using runtime = runtime_package;
using testing = testing_package;

partial class testtrace_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object currentPlatformHasNoTrueˢ = (@string)"current platform has no true syscall events"u8;

// MustHaveSyscallEvents skips the current test if the current
// platform does not support true system call events.
public static void MustHaveSyscallEvents(ж<testing.T> Ꮡt) {
    if (HasSyscallEvents()) {
        return;
    }
    Ꮡt.Skip(currentPlatformHasNoTrueˢ);
}

// HasSyscallEvents returns true if the current platform
// has real syscall events available.
public static bool HasSyscallEvents() {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        // js and wasip1 emulate system calls by blocking on channels
        // while yielding back to the environment. They never actually
        // call entersyscall/exitsyscall.
        return false;
    }
    return true;
}

} // end testtrace_package
