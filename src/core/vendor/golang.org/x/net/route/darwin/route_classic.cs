// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd
namespace go.vendor.golang.org.x.net;

using runtime = runtime_package;
using syscall = syscall_package;

partial class route_package {

[GoRecv] internal static (slice<byte>, error) marshal(this ref RouteMessage m) {
    var (w, ok) = wireFormats[m.Type, ꟷ];
    if (!ok) {
        return (default!, errUnsupportedMessage);
    }
    nint l = (~w).bodyOff + addrsSpace(m.Addrs);
    if (runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8) {
        // Fix stray pointer writes on macOS.
        // See golang.org/issue/22456.
        l += 1024;
    }
    var b = new slice<byte>(l);
    nativeEndian.PutUint16(b[..2], (uint16)l);
    if (m.Version == 0){
        b[2] = rtmVersion;
    } else {
        b[2] = (byte)m.Version;
    }
    b[3] = (byte)m.Type;
    nativeEndian.PutUint32(b[8..12], (uint32)m.Flags);
    nativeEndian.PutUint16(b[4..6], (uint16)m.Index);
    nativeEndian.PutUint32(b[16..20], (uint32)m.ID);
    nativeEndian.PutUint32(b[20..24], (uint32)m.Seq);
    var (attrs, err) = marshalAddrs(b.slice((~w).bodyOff), m.Addrs);
    if (err != default!) {
        return (default!, err);
    }
    if (attrs > 0) {
        nativeEndian.PutUint32(b[12..16], (uint32)attrs);
    }
    return (b, default!);
}

[GoRecv] internal static (Message, error) parseRouteMessage(this ref wireFormat w, RIBType typ, slice<byte> b) {
    if (len(b) < w.bodyOff) {
        return (default!, errMessageTooShort);
    }
    nint l = (nint)nativeEndian.Uint16(b[..2]);
    if (len(b) < l) {
        return (default!, errInvalidMessage);
    }
    var m = Ꮡ(new RouteMessage(
        Version: (nint)b[2],
        Type: (nint)b[3],
        Flags: (nint)nativeEndian.Uint32(b[8..12]),
        Index: (nint)nativeEndian.Uint16(b[4..6]),
        ID: (uintptr)nativeEndian.Uint32(b[16..20]),
        Seq: (nint)nativeEndian.Uint32(b[20..24]),
        extOff: w.extOff,
        raw: b.slice(0, l)
    ));
    var errno = ((syscall.Errno)(uintptr)nativeEndian.Uint32(b[28..32]));
    if (errno != 0) {
        m.Value.Err = errno;
    }
    error err = default!;
    (m.Value.Addrs, err) = parseAddrs((nuint)nativeEndian.Uint32(b[12..16]), parseKernelInetAddr, b.slice(w.bodyOff));
    if (err != default!) {
        return (default!, err);
    }
    return (new RouteMessageжMessage(m), default!);
}

} // end route_package
