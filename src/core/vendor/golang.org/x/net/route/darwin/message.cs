// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.vendor.golang.org.x.net;

partial class route_package {

// A Message represents a routing message.
[GoType] partial interface Message {
    // Sys returns operating system-specific information.
    slice<ΔSys> Sys();
}

// A Sys reprensents operating system-specific information.
[GoType] partial interface ΔSys {
    // SysType returns a type of operating system-specific
    // information.
    ΔSysType SysType();
}

[GoType("num:nint")] partial struct ΔSysType;

public static ΔSysType SysMetrics => /* iota */ 0;
public static ΔSysType SysStats => 1;

// ParseRIB parses b as a routing information base and returns a list
// of routing messages.
public static (slice<Message>, error) ParseRIB(RIBType typ, slice<byte> b) {
    if (!typ.parseable()) {
        return (default!, errUnsupportedMessage);
    }
    slice<Message> msgs = default!;
    nint nmsgs = 0;
    nint nskips = 0;
    while (len(b) > 4) {
        nmsgs++;
        nint l = (nint)nativeEndian.Uint16(b[..2]);
        if (l == 0) {
            return (default!, errInvalidMessage);
        }
        if (len(b) < l) {
            return (default!, errMessageTooShort);
        }
        if (b[2] != rtmVersion) {
            b = b.slice(l);
            continue;
        }
        {
            var (w, ok) = wireFormats[(nint)b[3], ꟷ]; if (!ok){
                nskips++;
            } else {
                var (m, err) = (~w).parse(typ, b.slice(0, l));
                if (err != default!) {
                    return (default!, err);
                }
                if (m == default!){
                    nskips++;
                } else {
                    msgs = append(msgs, m);
                }
            }
        }
        b = b.slice(l);
    }
    // We failed to parse any of the messages - version mismatch?
    if (nmsgs != len(msgs) + nskips) {
        return (default!, errMessageMismatch);
    }
    return (msgs, default!);
}

} // end route_package
