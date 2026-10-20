// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || wasip1
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// Do the interface allocations only once for common
// Errno values.
internal static error errEAGAIN = ((Δsyscall.Errno)Δsyscall.EAGAIN);
internal static error errEINVAL = ((Δsyscall.Errno)Δsyscall.EINVAL);
internal static error errENOENT = ((Δsyscall.Errno)Δsyscall.ENOENT);

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
internal static error errnoErr(Δsyscall.Errno e) {
    var exprᴛ1 = e;
    if (exprᴛ1 == (Δsyscall.Errno)(0)) {
        return default!;
    }
    if (exprᴛ1 == Δsyscall.EAGAIN) {
        return errEAGAIN;
    }
    if (exprᴛ1 == Δsyscall.EINVAL) {
        return errEINVAL;
    }
    if (exprᴛ1 == Δsyscall.ENOENT) {
        return errENOENT;
    }
    return e;
}

} // end poll_package
