// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class net_package {

internal static @string hostsFilePath = windows.GetSystemDirectory() + "/Drivers/etc/hosts"u8;

// Placeholders for socket system calls.
internal static Func<int32, int32, int32, ж<syscall.WSAProtocolInfo>, uint32, uint32, (syscallꓸHandle, error)> wsaSocketFunc = windows.WSASocket;
internal static Func<syscallꓸHandle, syscallꓸSockaddr, error> connectFunc = syscall.Connect;
internal static Func<syscallꓸHandle, nint, error> listenFunc = syscall.Listen;

} // end net_package
