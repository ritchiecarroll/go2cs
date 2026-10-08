// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd

// Package route provides basic functions for the manipulation of
// packet routing facilities on BSD variants.
//
// The package supports any version of Darwin, any version of
// DragonFly BSD, FreeBSD 7 and above, NetBSD 6 and above, and OpenBSD
// 5.6 and above.
namespace go.vendor.golang.org.x.net;

using errors = errors_package;
using os = os_package;
using syscall = syscall_package;

partial class route_package {

internal static error errUnsupportedMessage = errors.New("unsupported message"u8);
internal static error errMessageMismatch = errors.New("message mismatch"u8);
internal static error errMessageTooShort = errors.New("message too short"u8);
internal static error errInvalidMessage = errors.New("invalid message"u8);
internal static error errInvalidAddr = errors.New("invalid address"u8);
internal static error errShortBuffer = errors.New("short buffer"u8);

// A RouteMessage represents a message conveying an address prefix, a
// nexthop address and an output interface.
//
// Unlike other messages, this message can be used to query adjacency
// information for the given address prefix, to add a new route, and
// to delete or modify the existing route from the routing information
// base inside the kernel by writing and reading route messages on a
// routing socket.
//
// For the manipulation of routing information, the route message must
// contain appropriate fields that include:
//
//	Version       = <must be specified>
//	Type          = <must be specified>
//	Flags         = <must be specified>
//	Index         = <must be specified if necessary>
//	ID            = <must be specified>
//	Seq           = <must be specified>
//	Addrs         = <must be specified>
//
// The Type field specifies a type of manipulation, the Flags field
// specifies a class of target information and the Addrs field
// specifies target information like the following:
//
//	route.RouteMessage{
//		Version: RTM_VERSION,
//		Type: RTM_GET,
//		Flags: RTF_UP | RTF_HOST,
//		ID: uintptr(os.Getpid()),
//		Seq: 1,
//		Addrs: []route.Addrs{
//			RTAX_DST: &route.Inet4Addr{ ... },
//			RTAX_IFP: &route.LinkAddr{ ... },
//			RTAX_BRD: &route.Inet4Addr{ ... },
//		},
//	}
//
// The values for the above fields depend on the implementation of
// each operating system.
//
// The Err field on a response message contains an error value on the
// requested operation. If non-nil, the requested operation is failed.
partial struct RouteMessage {
    public nint Version;    // message version
    public nint Type;    // message type
    public nint Flags;    // route flags
    public nint Index;    // interface index when attached
    public uintptr ID; // sender's identifier; usually process ID
    public nint Seq;    // sequence number
    public error Err;   // error on requested operation
    public slice<Addr> Addrs; // addresses
    internal nint extOff;   // offset of header extension
    internal slice<byte> raw; // raw message
}

// Marshal returns the binary encoding of m.
public static (slice<byte>, error) Marshal(this ref RouteMessage m) {
    return m.marshal();
}

partial struct RIBType /*num:nint*/;

public static RIBType RIBTypeRoute => /* syscall.NET_RT_DUMP */ 1;
public static RIBType RIBTypeInterface => /* syscall.NET_RT_IFLIST */ 3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sysctlˢ = "sysctl"u8;

// FetchRIB fetches a routing information base from the operating
// system.
//
// The provided af must be an address family.
//
// The provided arg must be a RIBType-specific argument.
// When RIBType is related to routes, arg might be a set of route
// flags. When RIBType is related to network interfaces, arg might be
// an interface index or a set of interface flags. In most cases, zero
// means a wildcard.
public static (slice<byte>, error) FetchRIB(nint af, RIBType typ, nint arg) {
    nint @try = 0;
    while (ᐧ) {
        @try++;
        var mib = new int32[]{syscall.CTL_NET, syscall.AF_ROUTE, 0, (int32)af, (int32)(nint)typ, (int32)arg}.array();
        ref var n = ref heap<uintptr>(out var Ꮡn);
        n = (uintptr)0;
        {
            var err = sysctl(mib[..], nil, Ꮡn, nil, 0); if (err != default!) {
                return (default!, os.NewSyscallError(sysctlˢ, err));
            }
        }
        if (n == 0) {
            return (default!, default!);
        }
        var b = new slice<byte>((nint)(n));
        {
            var err = sysctl(mib[..], Ꮡ(b, 0), Ꮡn, nil, 0); if (err != default!) {
                // If the sysctl failed because the data got larger
                // between the two sysctl calls, try a few times
                // before failing. (golang.org/issue/45736).
                const nint maxTries = 3;
                if (AreEqual(err, syscall.ENOMEM) && @try < maxTries) {
                    continue;
                }
                return (default!, os.NewSyscallError(sysctlˢ, err));
            }
        }
        return (b.slice(0, (nint)(n)), default!);
    }
}

} // end route_package
