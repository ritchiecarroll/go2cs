// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!windows && !solaris) || illumos
namespace go;

using syscall = syscall_package;
using time = time_package;

partial class net_package {

// SetKeepAliveConfig configures keep-alive messages sent by the operating system.
public static error SetKeepAliveConfig(this ж<TCPConn> Ꮡc, KeepAliveConfig config) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(TCPConn.Ꮡconn).ok()) {
        return syscall.EINVAL;
    }

    {
        var err = setKeepAlive(c.fd, config.Enable); if (err != nil) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err)));
        }
    }
    {
        var err = setKeepAliveIdle(c.fd, config.Idle); if (err != nil) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err)));
        }
    }
    {
        var err = setKeepAliveInterval(c.fd, config.Interval); if (err != nil) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err)));
        }
    }
    {
        var err = setKeepAliveCount(c.fd, config.Count); if (err != nil) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err)));
        }
    }

    return default;
}

} // end net_package
