// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || netbsd
namespace go.@internal;

using Δruntime = runtime_package;
using syscall = syscall_package;

partial class routebsd_package {

internal static (Message, error) parseInterfaceMessage(this ref wireFormat w, slice<byte> b) {
    if (len(b) < w.bodyOff) {
        return (default!, errMessageTooShort);
    }
    nint l = (nint)nativeEndian.Uint16(b[..2]);
    if (len(b) < l) {
        return (default!, errInvalidMessage);
    }
    nuint attrs = (nuint)nativeEndian.Uint32(b[4..8]);

    var m = Ꮡ(new InterfaceMessage(
        Version: (nint)b[2],
        Type: (nint)b[3],
        Addrs: new slice<Addr>(syscall.RTAX_MAX),
        Flags: (nint)nativeEndian.Uint32(b[8..12]),
        Index: (nint)nativeEndian.Uint16(b[12..14]),
        extOff: w.extOff,
        raw: b.slice(0, l)
    ));

    // We used to require that RTA_IFP always be set.
    // It turns out that on darwin messages about the
    // utun interface may not include a name. Issue #71064.
    if ((attrs & (nuint)syscall.RTA_IFP) != 0){
        var (a, err) = parseLinkAddr(b.slice(w.bodyOff));
        if (err != default!) {
            return (default!, err);
        }
        m.Value.Addrs[syscall.RTAX_IFP] = a;
        m.Value.Name = a._<ж<LinkAddr>>().Value.Name;
    } else {
        // DragonFly seems to have unnamed interfaces
        // that we can't look up again. Just skip them.
        if (Δruntime.GOOS == "dragonfly"u8) {
            return (default!, default!);
        }
    }

    return (new InterfaceMessageжMessage(m), default!);
}

internal static (Message, error) parseInterfaceAddrMessage(this ref wireFormat w, slice<byte> b) {
    if (len(b) < w.bodyOff) {
        return (default!, errMessageTooShort);
    }
    nint l = (nint)nativeEndian.Uint16(b[..2]);
    if (len(b) < l) {
        return (default!, errInvalidMessage);
    }
    var m = Ꮡ(new InterfaceAddrMessage(
        Version: (nint)b[2],
        Type: (nint)b[3],
        Flags: (nint)nativeEndian.Uint32(b[8..12]),
        raw: b.slice(0, l)
    ));
    if (Δruntime.GOOS == "netbsd"u8){
        m.Value.Index = (nint)nativeEndian.Uint16(b[16..18]);
    } else {
        m.Value.Index = (nint)nativeEndian.Uint16(b[12..14]);
    }
    error err = default!;
    (m.Value.Addrs, err) = parseAddrs((nuint)nativeEndian.Uint32(b[4..8]), b.slice(w.bodyOff));
    if (err != default!) {
        return (default!, err);
    }
    return (new InterfaceAddrMessageжMessage(m), default!);
}

} // end routebsd_package
