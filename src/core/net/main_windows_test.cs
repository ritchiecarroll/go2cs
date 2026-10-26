// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using exec = go.os.exec_package;
using syscall = syscall_package;
using @internal;
using go.os;
using net.@internal;
using socktest = net.@internal.socktest_package;
using static go.net_package;

partial class net_internal_test_package {

// Placeholders for saving original socket system calls.
internal static Func<int32, int32, int32, ж<syscall.WSAProtocolInfo>, uint32, uint32, (syscallꓸHandle, error)> origWSASocket;
internal static void initᴛorigWSASocket() { origWSASocket = wsaSocketFunc; }
internal static Func<syscallꓸHandle, error> origClosesocket = poll.CloseFunc;
internal static Func<syscallꓸHandle, syscallꓸSockaddr, error> origConnect;
internal static void initᴛorigConnect() { origConnect = connectFunc; }
internal static Func<syscallꓸHandle, syscallꓸSockaddr, ж<byte>, uint32, ж<uint32>, ж<syscall.Overlapped>, error> origConnectEx = poll.ConnectExFunc;
internal static Func<syscallꓸHandle, nint, error> origListen;
internal static void initᴛorigListen() { origListen = listenFunc; }
internal static Func<syscallꓸHandle, syscallꓸHandle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<syscall.Overlapped>, error> origAccept = poll.AcceptFunc;

internal static void installTestHooks() {
    wsaSocketFunc = Ꮡsw.WSASocket;
    poll.CloseFunc = Ꮡsw.Closesocket;
    connectFunc = Ꮡsw.Connect;
    poll.ConnectExFunc = Ꮡsw.ConnectEx;
    listenFunc = Ꮡsw.Listen;
    poll.AcceptFunc = Ꮡsw.AcceptEx;
}

internal static void uninstallTestHooks() {
    wsaSocketFunc = origWSASocket;
    poll.CloseFunc = origClosesocket;
    connectFunc = origConnect;
    poll.ConnectExFunc = origConnectEx;
    listenFunc = origListen;
    poll.AcceptFunc = origAccept;
}

// forceCloseSockets must be called only from TestMain.
internal static void forceCloseSockets() {
    foreach (var (s, _) in Ꮡsw.Sockets()) {
        poll.CloseFunc(s);
    }
}

internal static void addCmdInheritedHandle(ж<exec.Cmd> Ꮡcmd, uintptr fd) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    // Inherited handles are not inherited by default in Windows.
    // We need to set the handle inheritance flag explicitly.
    // See https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessa#parameters
    // for more details.
    if (cmd.SysProcAttr == nil) {
        cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(nil));
    }
    cmd.SysProcAttr.Value.AdditionalInheritedHandles = append((~cmd.SysProcAttr).AdditionalInheritedHandles, (syscallꓸHandle)fd);
}

} // end net_internal_test_package
