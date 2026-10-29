// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux || netbsd || openbsd || solaris
namespace go;

using syscall = syscall_package;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipe2ˢ = "pipe2"u8;

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any.
public static (ж<File> r, ж<File> w, error err) Pipe() {
    array<nint> p = new(2);

    var e = syscall.Pipe2(p[0..], syscall.O_CLOEXEC);
    if (e != nil) {
        return (default, default, NewSyscallError(pipe2ˢ, e));
    }

    return (newFile(p[0], "|0"u8, kindPipe, false), newFile(p[1], "|1"u8, kindPipe, false), default);
}

} // end os_package
