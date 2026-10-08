// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net;

using syscall = syscall_package;

partial class route_package {

internal static bool parseable(this RIBType typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == syscall.NET_RT_STAT || exprᴛ1 == syscall.NET_RT_TRASH) {
        return false;
    }
    { /* default: */
        return true;
    }

}

// RouteMetrics represents route metrics.
partial struct RouteMetrics {
    public nint PathMTU; // path maximum transmission unit
}

// SysType implements the SysType method of Sys interface.
public static ΔSysType SysType(this ref RouteMetrics rmx) {
    return SysMetrics;
}

// Sys implements the Sys method of Message interface.
public static slice<ΔSys> Sys(this ref RouteMessage m) {
    return new ΔSys[]{new RouteMetricsжΔSys(Ꮡ(new RouteMetrics(
        PathMTU: (nint)nativeEndian.Uint32(m.raw.slice(m.extOff + 4, m.extOff + 8))
    )))
    }.slice();
}

// InterfaceMetrics represents interface metrics.
partial struct InterfaceMetrics {
    public nint Type; // interface type
    public nint MTU; // maximum transmission unit
}

// SysType implements the SysType method of Sys interface.
public static ΔSysType SysType(this ref InterfaceMetrics imx) {
    return SysMetrics;
}

// Sys implements the Sys method of Message interface.
public static slice<ΔSys> Sys(this ref InterfaceMessage m) {
    return new ΔSys[]{new InterfaceMetricsжΔSys(Ꮡ(new InterfaceMetrics(
        Type: (nint)m.raw[m.extOff],
        MTU: (nint)nativeEndian.Uint32(m.raw.slice(m.extOff + 8, m.extOff + 12))
    )))
    }.slice();
}

internal static (nint, map<nint, ж<wireFormat>>) probeRoutingStack() {
    var rtm = Ꮡ(new wireFormat(extOff: 36, bodyOff: sizeofRtMsghdrDarwin15));
    var rtmʗ1 = rtm;
        rtm.Value.parse = (RIBType p1, slice<byte> p2) => rtmʗ1.parseRouteMessage(p1, p2);
    var rtm2 = Ꮡ(new wireFormat(extOff: 36, bodyOff: sizeofRtMsghdr2Darwin15));
    var rtm2ʗ1 = rtm2;
        rtm2.Value.parse = (RIBType p1, slice<byte> p2) => rtm2ʗ1.parseRouteMessage(p1, p2);
    var ifm = Ꮡ(new wireFormat(extOff: 16, bodyOff: sizeofIfMsghdrDarwin15));
    var ifmʗ1 = ifm;
        ifm.Value.parse = (RIBType p1, slice<byte> p2) => ifmʗ1.parseInterfaceMessage(p1, p2);
    var ifm2 = Ꮡ(new wireFormat(extOff: 32, bodyOff: sizeofIfMsghdr2Darwin15));
    var ifm2ʗ1 = ifm2;
        ifm2.Value.parse = (RIBType p1, slice<byte> p2) => ifm2ʗ1.parseInterfaceMessage(p1, p2);
    var ifam = Ꮡ(new wireFormat(extOff: sizeofIfaMsghdrDarwin15, bodyOff: sizeofIfaMsghdrDarwin15));
    var ifamʗ1 = ifam;
        ifam.Value.parse = (RIBType p1, slice<byte> p2) => ifamʗ1.parseInterfaceAddrMessage(p1, p2);
    var ifmam = Ꮡ(new wireFormat(extOff: sizeofIfmaMsghdrDarwin15, bodyOff: sizeofIfmaMsghdrDarwin15));
    var ifmamʗ1 = ifmam;
        ifmam.Value.parse = (RIBType p1, slice<byte> p2) => ifmamʗ1.parseInterfaceMulticastAddrMessage(p1, p2);
    var ifmam2 = Ꮡ(new wireFormat(extOff: sizeofIfmaMsghdr2Darwin15, bodyOff: sizeofIfmaMsghdr2Darwin15));
    var ifmam2ʗ1 = ifmam2;
        ifmam2.Value.parse = (RIBType p1, slice<byte> p2) => ifmam2ʗ1.parseInterfaceMulticastAddrMessage(p1, p2);
    // Darwin kernels require 32-bit aligned access to routing facilities.
    return (4, new map<nint, ж<wireFormat>>{
        [syscall.RTM_ADD] = rtm,
        [syscall.RTM_DELETE] = rtm,
        [syscall.RTM_CHANGE] = rtm,
        [syscall.RTM_GET] = rtm,
        [syscall.RTM_LOSING] = rtm,
        [syscall.RTM_REDIRECT] = rtm,
        [syscall.RTM_MISS] = rtm,
        [syscall.RTM_LOCK] = rtm,
        [syscall.RTM_RESOLVE] = rtm,
        [syscall.RTM_NEWADDR] = ifam,
        [syscall.RTM_DELADDR] = ifam,
        [syscall.RTM_IFINFO] = ifm,
        [syscall.RTM_NEWMADDR] = ifmam,
        [syscall.RTM_DELMADDR] = ifmam,
        [syscall.RTM_IFINFO2] = ifm2,
        [syscall.RTM_NEWMADDR2] = ifmam2,
        [syscall.RTM_GET2] = rtm2
    });
}

} // end route_package
