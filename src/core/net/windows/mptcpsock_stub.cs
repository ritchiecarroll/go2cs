// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using context = context_package;
using static net_package;

partial class net_package {

} partial class netᴛsysDialer {

internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> laddr, ж<TCPAddr> raddr) {
    return Ꮡsd.dialTCP(ctx, laddr, raddr);
}

} partial class netᴛsysListener {

internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> laddr) {
    return Ꮡsl.listenTCP(ctx, laddr);
}

} partial class net_package {

internal static bool isUsingMultipathTCP(ref netFD fd) {
    return false;
}

} // end net_package
