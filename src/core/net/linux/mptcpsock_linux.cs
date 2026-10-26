// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using poll = @internal.poll_package;
using unix = @internal.syscall.unix_package;
using Δsync = sync_package;
using syscall = syscall_package;
using @internal;
using @internal.syscall;

partial class net_package {

internal static ж<Δsync.Once> ᏑmptcpOnce = new StaticBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once mptcpOnce => ref ᏑmptcpOnce.Value;
internal static bool mptcpAvailable;
internal static bool hasSOLMPTCP; // only valid if mptcpAvailable is true

// These constants aren't in the syscall package, which is frozen
internal static UntypedInt _IPPROTO_MPTCP => 0x106;
internal static UntypedInt _SOL_MPTCP => 0x11c;
internal static UntypedInt _MPTCP_INFO => 0x1;

internal static bool supportsMultipathTCP() {
    ᏑmptcpOnce.Do(initMPTCPavailable);
    return mptcpAvailable;
}

// Check that MPTCP is supported by attempting to create an MPTCP socket and by
// looking at the returned error if any.
internal static void initMPTCPavailable() {
    nint family = syscall.AF_INET;
    if (!supportsIPv4()) {
        family = syscall.AF_INET6;
    }
    var (s, err) = sysSocket(family, syscall.SOCK_STREAM, _IPPROTO_MPTCP);

    var matchᴛ1 = false;
    if (errors.Is(err, syscall.EPROTONOSUPPORT)) { matchᴛ1 = true; // Not supported: >= v5.6
        return;
    }
    if (errors.Is(err, syscall.EINVAL)) { matchᴛ1 = true; // Not supported: < v5.6
        return;
    }
    if (err == default!) { matchᴛ1 = true; // Supported and no error
        poll.CloseFunc(s);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        // another error: MPTCP was not available but it might be later
        mptcpAvailable = true;
    }

    // SOL_MPTCP only supported from kernel 5.16.
    hasSOLMPTCP = unix.KernelVersionGE(5, 16);
}

internal static (ж<TCPConn>, error) dialMPTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    if (supportsMultipathTCP()) {
        {
            var (conn, err) = Ꮡsd.doDialTCPProto(ctx, Ꮡladdr, Ꮡraddr, _IPPROTO_MPTCP); if (err == default!) {
                return (conn, default!);
            }
        }
    }

    // Fallback to dialTCP if Multipath TCP isn't supported on this operating
    // system. But also fallback in case of any error with MPTCP.
    //
    // Possible MPTCP specific error: ENOPROTOOPT (sysctl net.mptcp.enabled=0)
    // But just in case MPTCP is blocked differently (SELinux, etc.), just
    // retry with "plain" TCP.
    return Ꮡsd.dialTCP(ctx, Ꮡladdr, Ꮡraddr);
}

internal static (ж<TCPListener>, error) listenMPTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) {
    if (supportsMultipathTCP()) {
        {
            var (dial, err) = Ꮡsl.listenTCPProto(ctx, Ꮡladdr, _IPPROTO_MPTCP); if (err == default!) {
                return (dial, default!);
            }
        }
    }

    // Fallback to listenTCP if Multipath TCP isn't supported on this operating
    // system. But also fallback in case of any error with MPTCP.
    //
    // Possible MPTCP specific error: ENOPROTOOPT (sysctl net.mptcp.enabled=0)
    // But just in case MPTCP is blocked differently (SELinux, etc.), just
    // retry with "plain" TCP.
    return Ꮡsl.listenTCP(ctx, Ꮡladdr);
}

// hasFallenBack reports whether the MPTCP connection has fallen back to "plain"
// TCP.
//
// A connection can fallback to TCP for different reasons, e.g. the other peer
// doesn't support it, a middle box "accidentally" drops the option, etc.
//
// If the MPTCP protocol has not been requested when creating the socket, this
// method will return true: MPTCP is not being used.
//
// Kernel >= 5.16 returns EOPNOTSUPP/ENOPROTOOPT in case of fallback.
// Older kernels will always return them even if MPTCP is used: not usable.
internal static bool hasFallenBack(ж<netFD> Ꮡfd) {
    var (_, err) = Ꮡfd.of(netFD.Ꮡpfd).GetsockoptInt(_SOL_MPTCP, _MPTCP_INFO);

    // 2 expected errors in case of fallback depending on the address family
    //   - AF_INET:  EOPNOTSUPP
    //   - AF_INET6: ENOPROTOOPT
    return AreEqual(err, syscall.EOPNOTSUPP) || AreEqual(err, syscall.ENOPROTOOPT);
}

// isUsingMPTCPProto reports whether the socket protocol is MPTCP.
//
// Compared to hasFallenBack method, here only the socket protocol being used is
// checked: it can be MPTCP but it doesn't mean MPTCP is used on the wire, maybe
// a fallback to TCP has been done.
internal static bool isUsingMPTCPProto(ж<netFD> Ꮡfd) {
    var (proto, _) = Ꮡfd.of(netFD.Ꮡpfd).GetsockoptInt(syscall.SOL_SOCKET, syscall.SO_PROTOCOL);

    return proto == _IPPROTO_MPTCP;
}

// isUsingMultipathTCP reports whether MPTCP is still being used.
//
// Please look at the description of hasFallenBack (kernel >=5.16) and
// isUsingMPTCPProto methods for more details about what is being checked here.
internal static bool isUsingMultipathTCP(ж<netFD> Ꮡfd) {
    if (!supportsMultipathTCP()) {
        return false;
    }

    if (hasSOLMPTCP) {
        return !hasFallenBack(Ꮡfd);
    }

    return isUsingMPTCPProto(Ꮡfd);
}

} // end net_package
