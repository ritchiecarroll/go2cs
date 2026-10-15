// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static UntypedInt _SO_TYPE => /* syscall.SO_TYPE */ 3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setnonblockˢ = "setnonblock"u8;

internal static (nint, error) dupFileSocket(ж<os.File> Ꮡf) {
    var (s, call, err) = poll.DupCloseOnExec((nint)Ꮡf.Fd());
    if (err != default!) {
        if (call != ""u8) {
            err = os.NewSyscallError(call, err);
        }
        return (-1, err);
    }
    {
        var errΔ1 = syscall.SetNonblock(s, true); if (errΔ1 != default!) {
            poll.CloseFunc(s);
            return (-1, os.NewSyscallError(setnonblockˢ, errΔ1));
        }
    }
    return (s, default!);
}

} // end net_package
