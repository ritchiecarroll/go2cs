// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> v) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.writeBuffers(v);
    if (err != nil) {
        return (n, new OpErrorжerror(Ꮡ(new OpError(Op: "writev"u8, Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err))));
    }
    return (n, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writevˢ = "writev"u8;

internal static (int64 n, error err) writeBuffers(this ж<netFD> Ꮡfd, ж<Buffers> v) {
    int64 n = default;
    error err = default;

    (n, err) = Ꮡfd.of(netFD.Ꮡpfd).Writev(v.of(Buffers.Ꮡm_value));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writevˢ, err));
}

} // end net_package
