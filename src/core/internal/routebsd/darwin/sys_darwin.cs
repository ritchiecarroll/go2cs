// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using syscall = syscall_package;

partial class routebsd_package {

// MTU returns the interface MTU.
public static nint MTU(this ref InterfaceMessage m) {
    return (nint)nativeEndian.Uint32(m.raw.slice(m.extOff + 8, m.extOff + 12));
}

// sizeofIfMsghdr2 is copied from x/sys/unix.
internal static UntypedInt sizeofIfMsghdr2 => 0xa0;

internal static (nint, map<nint, ж<wireFormat>>) probeRoutingStack() {
    var ifm = Ꮡ(new wireFormat(extOff: 16, bodyOff: syscall.SizeofIfMsghdr));
    var ifmʗ1 = ifm;
        ifm.Value.parse = (slice<byte> p1) => ifmʗ1.parseInterfaceMessage(p1);
    var ifm2 = Ꮡ(new wireFormat(extOff: 32, bodyOff: sizeofIfMsghdr2));
    var ifm2ʗ1 = ifm2;
        ifm2.Value.parse = (slice<byte> p1) => ifm2ʗ1.parseInterfaceMessage(p1);
    var ifam = Ꮡ(new wireFormat(extOff: syscall.SizeofIfaMsghdr, bodyOff: syscall.SizeofIfaMsghdr));
    var ifamʗ1 = ifam;
        ifam.Value.parse = (slice<byte> p1) => ifamʗ1.parseInterfaceAddrMessage(p1);
    var ifmam = Ꮡ(new wireFormat(extOff: syscall.SizeofIfmaMsghdr, bodyOff: syscall.SizeofIfmaMsghdr));
    var ifmamʗ1 = ifmam;
        ifmam.Value.parse = (slice<byte> p1) => ifmamʗ1.parseInterfaceMulticastAddrMessage(p1);
    var ifmam2 = Ꮡ(new wireFormat(extOff: syscall.SizeofIfmaMsghdr2, bodyOff: syscall.SizeofIfmaMsghdr2));
    var ifmam2ʗ1 = ifmam2;
        ifmam2.Value.parse = (slice<byte> p1) => ifmam2ʗ1.parseInterfaceMulticastAddrMessage(p1);
    // Darwin kernels require 32-bit aligned access to routing facilities.
    return (4, new map<nint, ж<wireFormat>>{
        [syscall.RTM_NEWADDR] = ifam,
        [syscall.RTM_DELADDR] = ifam,
        [syscall.RTM_IFINFO] = ifm,
        [syscall.RTM_NEWMADDR] = ifmam,
        [syscall.RTM_DELMADDR] = ifmam,
        [syscall.RTM_IFINFO2] = ifm2,
        [syscall.RTM_NEWMADDR2] = ifmam2
    });
}

} // end routebsd_package
