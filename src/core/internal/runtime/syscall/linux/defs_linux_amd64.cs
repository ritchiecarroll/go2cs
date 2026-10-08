// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

partial class syscall_package {

public static UntypedInt SYS_MPROTECT => 10;
public static UntypedInt SYS_FCNTL => 72;
public static UntypedInt SYS_EPOLL_CTL => 233;
public static UntypedInt SYS_EPOLL_PWAIT => 281;
public static UntypedInt SYS_EPOLL_CREATE1 => 291;
public static UntypedInt SYS_EPOLL_PWAIT2 => 441;
public static UntypedInt SYS_EVENTFD2 => 290;
public static UntypedInt EFD_NONBLOCK => 0x800;

partial struct EpollEvent {
    public uint32 Events;
    public array<byte> Data = new(8); // unaligned uintptr
}

} // end syscall_package
