// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin
namespace go;

using syscall = syscall_package;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipeˢ = "pipe"u8;

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any.
public static (ж<File> r, ж<File> w, error err) Pipe() {
    array<nint> p = new(2);

    // See ../syscall/exec.go for description of lock.
    syscall.ᏑForkLock.RLock();
    var e = syscall.Pipe(p[0..]);
    if (e != default!) {
        syscall.ᏑForkLock.RUnlock();
        return (default!, default!, NewSyscallError(pipeˢ, e));
    }
    syscall.CloseOnExec(p[0]);
    syscall.CloseOnExec(p[1]);
    syscall.ᏑForkLock.RUnlock();

    return (newFile(p[0], "|0"u8, kindPipe, false), newFile(p[1], "|1"u8, kindPipe, false), default!);
}

} // end os_package
