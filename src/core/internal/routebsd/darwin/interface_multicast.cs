// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd
namespace go.@internal;

partial class routebsd_package {

internal static (Message, error) parseInterfaceMulticastAddrMessage(this ref wireFormat w, slice<byte> b) {
    if (len(b) < w.bodyOff) {
        return (default, errMessageTooShort);
    }
    nint l = (nint)nativeEndian.Uint16(b[..2]);
    if (len(b) < l) {
        return (default, errInvalidMessage);
    }
    var m = Ꮡ(new InterfaceMulticastAddrMessage(
        Version: (nint)b[2],
        Type: (nint)b[3],
        Flags: (nint)nativeEndian.Uint32(b[8..12]),
        Index: (nint)nativeEndian.Uint16(b[12..14]),
        raw: b.slice(0, l)
    ));
    error err = default;
    (m.Addrs, err) = parseAddrs((nuint)nativeEndian.Uint32(b[4..8]), b.slice(w.bodyOff));
    if (err != nil) {
        return (default, err);
    }
    return (new InterfaceMulticastAddrMessageжMessage(m), default);
}

} // end routebsd_package
