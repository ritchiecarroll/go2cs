// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;
using static net_package;

partial class net_package {

} partial class net_package {

internal static (int64, error) writeBuffersᴍ(ж<conn> Ꮡc, ж<Buffers> v) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.writeBuffers(v);
    if (err != nil) {
        return (n, new OpErrorжerror(Ꮡ(new OpError(Op: "writev", Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err))));
    }
    return (n, default);
}

} partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writevˢ => ref writevˢᵇ;
private static readonly @string writevˢᵇ = "writev"u8;

internal static (int64 n, error err) writeBuffersᴍ(ж<netFD> Ꮡfd, ж<Buffers> v) {
    int64 n = default;
    error err = default;

    (n, err) = Ꮡfd.Ꮡpfd.Writev(v.of(Buffers.Ꮡm_value));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(writevˢ, err));
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class netᴛconn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> v) => net_package.writeBuffersᴍ(Ꮡc, v);
}
partial class netᴛnetFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) writeBuffers(this ж<netFD> Ꮡfd, ж<Buffers> v) => net_package.writeBuffersᴍ(Ꮡfd, v);
}
// </GlyphForwarders>
