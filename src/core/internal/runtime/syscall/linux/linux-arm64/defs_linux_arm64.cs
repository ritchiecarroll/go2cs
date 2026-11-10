// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.syscall;

partial class linux_package {

public static UntypedInt SYS_CLOSE => 57;
public static UntypedInt SYS_EPOLL_CREATE1 => 20;
public static UntypedInt SYS_EPOLL_CTL => 21;
public static UntypedInt SYS_EPOLL_PWAIT => 22;
public static UntypedInt SYS_FCNTL => 25;
public static UntypedInt SYS_PRCTL => 167;
public static UntypedInt SYS_MPROTECT => 226;
public static UntypedInt SYS_EPOLL_PWAIT2 => 441;
public static UntypedInt SYS_EVENTFD2 => 19;
public static UntypedInt SYS_OPENAT => 56;
public static UntypedInt SYS_PREAD64 => 67;
public static UntypedInt SYS_READ => 63;
public static UntypedInt SYS_UNAME => 160;

public static UntypedInt EFD_NONBLOCK => 0x800;

public static UntypedInt O_LARGEFILE => 0x0;

partial struct EpollEvent {
    public uint32 Events;
    internal uint32 pad_cgo_0;
    public uint64 Data;
}

} // end linux_package
