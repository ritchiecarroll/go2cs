// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements accept for platforms that do not provide a fast path for
// setting SetNonblock and CloseOnExec.
//go:build aix || darwin || (js && wasm) || wasip1
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acceptˢ = "accept"u8;
internal static readonly @string setnonblockˢ = "setnonblock"u8;

// Wrapper around the accept system call that marks the returned file
// descriptor as nonblocking and close-on-exec.
internal static (nint, Δsyscall.Sockaddr, @string, error) accept(nint s) {
    // See ../syscall/exec_unix.go for description of ForkLock.
    // It is probably okay to hold the lock across syscall.Accept
    // because we have put fd.sysfd into non-blocking mode.
    // However, a call to the File method will put it back into
    // blocking mode. We can't take that risk, so no use of ForkLock here.
    var (ns, sa, err) = AcceptFunc(s);
    if (err == nil) {
        Δsyscall.CloseOnExec(ns);
    }
    if (err != nil) {
        return (-1, default, acceptˢ, err);
    }
    {
        err = Δsyscall.SetNonblock(ns, true); if (err != nil) {
            CloseFunc(ns);
            return (-1, default, setnonblockˢ, err);
        }
    }
    return (ns, sa, "", default);
}

} // end poll_package
