// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd
namespace go.vendor.golang.org.x.net;

partial class route_package {

[GoRecv] internal static (Message, error) parseInterfaceMulticastAddrMessage(this ref wireFormat w, RIBType _, slice<byte> b) {
    if (len(b) < w.bodyOff) {
        return (default!, errMessageTooShort);
    }
    nint l = (nint)nativeEndian.Uint16(b[..2]);
    if (len(b) < l) {
        return (default!, errInvalidMessage);
    }
    var m = Ꮡ(new InterfaceMulticastAddrMessage(
        Version: (nint)b[2],
        Type: (nint)b[3],
        Flags: (nint)nativeEndian.Uint32(b[8..12]),
        Index: (nint)nativeEndian.Uint16(b[12..14]),
        raw: b.slice(0, l)
    ));
    error err = default!;
    (m.Value.Addrs, err) = parseAddrs((nuint)nativeEndian.Uint32(b[4..8]), parseKernelInetAddr, b.slice(w.bodyOff));
    if (err != default!) {
        return (default!, err);
    }
    return (new InterfaceMulticastAddrMessageжMessage(m), default!);
}

} // end route_package
