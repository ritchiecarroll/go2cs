// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || freebsd || solaris
namespace go;

using syscall = syscall_package;

partial class net_package {

internal static UntypedInt readMsgFlags => 0;

internal static void setReadMsgCloseOnExec(slice<byte> oob) {
    var (scms, err) = syscall.ParseSocketControlMessage(oob);
    if (err != default!) {
        return;
    }

    foreach (var (_, vᴛ1) in scms) {
        ref var scm = ref heap(new syscall.SocketControlMessage(), out var Ꮡscm);
        scm = vᴛ1;

        if (scm.Header.Level == syscall.SOL_SOCKET && scm.Header.Type == syscall.SCM_RIGHTS) {
            var (fds, errΔ1) = syscall.ParseUnixRights(Ꮡscm);
            if (errΔ1 != default!) {
                continue;
            }
            foreach (var (_, fd) in fds) {
                syscall.CloseOnExec(fd);
            }
        }
    }
}

} // end net_package
