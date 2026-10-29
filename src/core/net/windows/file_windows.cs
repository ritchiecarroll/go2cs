// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @internal.syscall;

partial class net_package {

internal static UntypedInt _SO_TYPE => /* windows.SO_TYPE */ 4104;

internal static (syscallꓸHandle, error) dupSocket(syscallꓸHandle h) {
    ref var info = ref heap(new syscall.WSAProtocolInfo(), out var Ꮡinfo);
    var err = windows.WSADuplicateSocket(h, (uint32)syscall.Getpid(), Ꮡinfo);
    if (err != nil) {
        return (0, err);
    }
    return windows.WSASocket(-1, -1, -1, Ꮡinfo, 0, (uint32)((uint32)windows.WSA_FLAG_OVERLAPPED | (uint32)windows.WSA_FLAG_NO_HANDLE_INHERIT));
}

internal static (syscallꓸHandle, error) dupFileSocket(ж<os.File> f) {
    // Call Fd to disassociate the IOCP from the handle,
    // it is not safe to share a duplicated handle
    // that is associated with IOCP.
    // Don't use the returned fd, as it might be closed
    // if f happens to be the last reference to the file.
    f.Fd();

    var (sc, err) = f.SyscallConn();
    if (err != nil) {
        return (0, err);
    }

    syscallꓸHandle h = default;
    ref var syserr = ref heap<error>(out var Ꮡsyserr);
    err = sc.Control((uintptr fd) => {
        (h, Ꮡsyserr.ValueSlot) = dupSocket((syscallꓸHandle)fd);
    });
    if (err == nil) {
        err = syserr;
    }
    if (err != nil) {
        return (0, err);
    }
    return (h, default);
}

} // end net_package
