// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements accept for platforms that provide a fast path for
// setting SetNonblock and CloseOnExec.
//go:build dragonfly || freebsd || linux || netbsd || openbsd
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string accept4ˢ = "accept4"u8;

// Wrapper around the accept system call that marks the returned file
// descriptor as nonblocking and close-on-exec.
internal static (nint, Δsyscall.Sockaddr, @string, error) accept(nint s) {
    var (ns, sa, err) = Accept4Func(s, (nint)((nint)Δsyscall.SOCK_NONBLOCK | (nint)Δsyscall.SOCK_CLOEXEC));
    if (err != nil) {
        return (-1, default, accept4ˢ, err);
    }
    return (ns, sa, "", default);
}

} // end poll_package
