// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || (openbsd && !mips64) || solaris
namespace go.@internal;

using Δsyscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class poll_package {

//go:linkname writev syscall.writev
/*linkname*/ internal static partial (uintptr, error) writev(nint fd, slice<Δsyscall.Iovec> iovecs) {
    var (ᴛ1, ᴛ2) = Δsyscall.writev(fd, iovecs);
    return ((uintptr)(uintptr)ᴛ1, ᴛ2);
}

} // end poll_package
