// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class poll_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fdopendirˢ = "fdopendir"u8;

// OpenDir returns a pointer to a DIR structure suitable for
// ReadDir. In case of an error, the name of the failed
// syscall is returned along with a syscall.Errno.
public static (uintptr, @string, error) OpenDir(this ж<FD> Ꮡfd) {
    // fdopendir(3) takes control of the file descriptor,
    // so use a dup.
    var (fd2, call, err) = Ꮡfd.Dup();
    if (err != default!) {
        return (0, call, err);
    }
    uintptr dir = default!;
    while (ᐧ) {
        (dir, err) = fdopendir(fd2);
        if (!AreEqual(err, Δsyscall.EINTR)) {
            break;
        }
    }
    if (err != default!) {
        Δsyscall.Close(fd2);
        return (0, fdopendirˢ, err);
    }
    return (dir, "", default!);
}

// Implemented in syscall/syscall_darwin.go.
//
//go:linkname fdopendir syscall.fdopendir
[global::System.Diagnostics.StackTraceHidden] internal static (uintptr dir, error err) fdopendir(nint fd) {
    var (ᴛ1, ᴛ2) = Δsyscall.fdopendir(fd);
    return ((uintptr)(uintptr)ᴛ1, ᴛ2);
}

} // end poll_package
