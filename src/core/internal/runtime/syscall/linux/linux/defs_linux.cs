// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.syscall;

partial class linux_package {

public static UntypedInt AT_FDCWD => /* -0x64 */ -100;

public static UntypedInt ENOENT => 0x2;

public static UntypedInt EPOLLIN => 0x1;
public static UntypedInt EPOLLOUT => 0x4;
public static UntypedInt EPOLLERR => 0x8;
public static UntypedInt EPOLLHUP => 0x10;
public static UntypedInt EPOLLRDHUP => 0x2000;
public static UntypedInt EPOLLET => 0x80000000;
public static UntypedInt EPOLL_CLOEXEC => 0x80000;
public static UntypedInt EPOLL_CTL_ADD => 0x1;
public static UntypedInt EPOLL_CTL_DEL => 0x2;
public static UntypedInt EPOLL_CTL_MOD => 0x3;
public static UntypedInt EFD_CLOEXEC => 0x80000;

public static UntypedInt O_RDONLY => 0x0;
public static UntypedInt O_CLOEXEC => 0x80000;

public static UntypedInt PR_SET_VMA => 0x53564d41;
public static UntypedInt PR_SET_VMA_ANON_NAME => 0;

} // end linux_package
