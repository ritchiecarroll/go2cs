// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using bytealg = @internal.bytealg_package;
using godebug = @internal.godebug_package;
using nettrace = @internal.nettrace_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using System.Runtime.CompilerServices;

partial class net_package {

internal static time.Duration defaultTCPKeepAliveIdle => /* 15 * time.Second */ 15000000000;
internal static time.Duration defaultTCPKeepAliveInterval => /* 15 * time.Second */ 15000000000;
internal static UntypedInt defaultTCPKeepAliveCount => 9;
internal const bool defaultMPTCPEnabledListen = true;
internal const bool defaultMPTCPEnabledDial = false;

// The type of service offered
//
//	0 == MPTCP disabled
//	1 == MPTCP enabled
//	2 == MPTCP enabled on listeners only
//	3 == MPTCP enabled on dialers only
internal static ж<godebug.Setting> multipathtcp = godebug.New("multipathtcp"u8);

[GoType("num:uint8")] partial struct mptcpStatusDial;

internal static mptcpStatusDial mptcpUseDefaultDial => /* iota */ 0;
internal static mptcpStatusDial mptcpEnabledDial => 1;
internal static mptcpStatusDial mptcpDisabledDial => 2;

[GoRecv] internal static bool get(this ref mptcpStatusDial m) {
    var exprᴛ1 = m;
    if (exprᴛ1 == mptcpEnabledDial) {
        return true;
    }
    if (exprᴛ1 == mptcpDisabledDial) {
        return false;
    }

    // If MPTCP is forced via GODEBUG=multipathtcp=1
    if (multipathtcp.Value() == "1"u8 || multipathtcp.Value() == "3"u8) {
        multipathtcp.IncNonDefault();
        return true;
    }
    return defaultMPTCPEnabledDial;
}

[GoRecv] internal static void set(this ref mptcpStatusDial m, bool use) {
    if (use){
        m = mptcpEnabledDial;
    } else {
        m = mptcpDisabledDial;
    }
}

[GoType("num:uint8")] partial struct mptcpStatusListen;

internal static mptcpStatusListen mptcpUseDefaultListen => /* iota */ 0;
internal static mptcpStatusListen mptcpEnabledListen => 1;
internal static mptcpStatusListen mptcpDisabledListen => 2;

[GoRecv] internal static bool get(this ref mptcpStatusListen m) {
    var exprᴛ1 = m;
    if (exprᴛ1 == mptcpEnabledListen) {
        return true;
    }
    if (exprᴛ1 == mptcpDisabledListen) {
        return false;
    }

    // If MPTCP is disabled via GODEBUG=multipathtcp=0 or only
    // enabled on dialers, but not on listeners.
    if (multipathtcp.Value() == "0"u8 || multipathtcp.Value() == "3"u8) {
        multipathtcp.IncNonDefault();
        return false;
    }
    return defaultMPTCPEnabledListen;
}

[GoRecv] internal static void set(this ref mptcpStatusListen m, bool use) {
    if (use){
        m = mptcpEnabledListen;
    } else {
        m = mptcpDisabledListen;
    }
}

// A Dialer contains options for connecting to an address.
//
// The zero value for each field is equivalent to dialing
// without that option. Dialing with the zero value of Dialer
// is therefore equivalent to just calling the [Dial] function.
//
// It is safe to call Dialer's methods concurrently.
[GoType] partial struct Dialer {
    // Timeout is the maximum amount of time a dial will wait for
    // a connect to complete. If Deadline is also set, it may fail
    // earlier.
    //
    // The default is no timeout.
    //
    // When using TCP and dialing a host name with multiple IP
    // addresses, the timeout may be divided between them.
    //
    // With or without a timeout, the operating system may impose
    // its own earlier timeout. For instance, TCP timeouts are
    // often around 3 minutes.
    public time.Duration Timeout;
    // Deadline is the absolute point in time after which dials
    // will fail. If Timeout is set, it may fail earlier.
    // Zero means no deadline, or dependent on the operating system
    // as with the Timeout option.
    public time.Time Deadline;
    // LocalAddr is the local address to use when dialing an
    // address. The address must be of a compatible type for the
    // network being dialed.
    // If nil, a local address is automatically chosen.
    public ΔAddr LocalAddr;
    // DualStack previously enabled RFC 6555 Fast Fallback
    // support, also known as "Happy Eyeballs", in which IPv4 is
    // tried soon if IPv6 appears to be misconfigured and
    // hanging.
    //
    // Deprecated: Fast Fallback is enabled by default. To
    // disable, set FallbackDelay to a negative value.
    public bool DualStack;
    // FallbackDelay specifies the length of time to wait before
    // spawning a RFC 6555 Fast Fallback connection. That is, this
    // is the amount of time to wait for IPv6 to succeed before
    // assuming that IPv6 is misconfigured and falling back to
    // IPv4.
    //
    // If zero, a default delay of 300ms is used.
    // A negative value disables Fast Fallback support.
    public time.Duration FallbackDelay;
    // KeepAlive specifies the interval between keep-alive
    // probes for an active network connection.
    //
    // KeepAlive is ignored if KeepAliveConfig.Enable is true.
    //
    // If zero, keep-alive probes are sent with a default value
    // (currently 15 seconds), if supported by the protocol and operating
    // system. Network protocols or operating systems that do
    // not support keep-alive ignore this field.
    // If negative, keep-alive probes are disabled.
    public time.Duration KeepAlive;
    // KeepAliveConfig specifies the keep-alive probe configuration
    // for an active network connection, when supported by the
    // protocol and operating system.
    //
    // If KeepAliveConfig.Enable is true, keep-alive probes are enabled.
    // If KeepAliveConfig.Enable is false and KeepAlive is negative,
    // keep-alive probes are disabled.
    public KeepAliveConfig KeepAliveConfig;
    // Resolver optionally specifies an alternate resolver to use.
    public ж<Resolver> Resolver;
    // Cancel is an optional channel whose closure indicates that
    // the dial should be canceled. Not all types of dials support
    // cancellation.
    //
    // Deprecated: Use DialContext instead.
    public /*<-*/channel<EmptyStruct> Cancel = /*<-*/channel<EmptyStruct>.RecvOnly;
    // If Control is not nil, it is called after creating the network
    // connection but before actually dialing.
    //
    // Network and address parameters passed to Control function are not
    // necessarily the ones passed to Dial. Calling Dial with TCP networks
    // will cause the Control function to be called with "tcp4" or "tcp6",
    // UDP networks become "udp4" or "udp6", IP networks become "ip4" or "ip6",
    // and other known networks are passed as-is.
    //
    // Control is ignored if ControlContext is not nil.
    public Func<@string, @string, syscall.RawConn, error> Control;
    // If ControlContext is not nil, it is called after creating the network
    // connection but before actually dialing.
    //
    // Network and address parameters passed to ControlContext function are not
    // necessarily the ones passed to Dial. Calling Dial with TCP networks
    // will cause the ControlContext function to be called with "tcp4" or "tcp6",
    // UDP networks become "udp4" or "udp6", IP networks become "ip4" or "ip6",
    // and other known networks are passed as-is.
    //
    // If ControlContext is not nil, Control is ignored.
    public Func<context.Context, @string, @string, syscall.RawConn, error> ControlContext;
    // If mptcpStatus is set to a value allowing Multipath TCP (MPTCP) to be
    // used, any call to Dial with "tcp(4|6)" as network will use MPTCP if
    // supported by the operating system.
    internal mptcpStatusDial mptcpStatus;
}

[GoRecv] internal static bool dualStack(this ref Dialer d) {
    return d.FallbackDelay >= 0;
}

internal static time.Time minNonzeroTime(time.Time a, time.Time b) {
    if (a.IsZero()) {
        return b;
    }
    if (b.IsZero() || a.Before(b)) {
        return a;
    }
    return b;
}

// deadline returns the earliest of:
//   - now+Timeout
//   - d.Deadline
//   - the context's deadline
//
// Or zero, if none of Timeout, Deadline, or context's deadline is set.
[GoRecv] internal static time.Time /*earliest*/ deadline(this ref Dialer d, context.Context ctx, time.Time now) {
    time.Time earliest = default!;

    if (d.Timeout != 0) {
        // including negative, for historical reasons
        earliest = now.Add(d.Timeout);
    }
    {
        var (dΔ1, ok) = ctx.Deadline(); if (ok) {
            earliest = minNonzeroTime(earliest, dΔ1);
        }
    }
    return minNonzeroTime(earliest, d.Deadline);
}

[GoRecv] internal static ж<Resolver> resolver(this ref Dialer d) {
    if (d.Resolver != nil) {
        return d.Resolver;
    }
    return DefaultResolver;
}

// partialDeadline returns the deadline to use for a single address,
// when multiple addresses are pending.
internal static (time.Time, error) partialDeadline(time.Time now, time.Time deadline, nint addrsRemaining) {
    if (deadline.IsZero()) {
        return (deadline, default!);
    }
    var timeRemaining = deadline.Sub(now);
    if (timeRemaining <= 0) {
        return (new time.Time(nil), errTimeout);
    }
    // Tentatively allocate equal time to each remaining address.
    var timeout = timeRemaining / ((time.Duration)(int64)addrsRemaining);
    // If the time per address is too short, steal from the end of the list.
    time.Duration saneMinimum = /* 2 * time.Second */ 2000000000;
    if (timeout < saneMinimum) {
        if (timeRemaining < saneMinimum){
            timeout = timeRemaining;
        } else {
            timeout = saneMinimum;
        }
    }
    return (now.Add(timeout), default!);
}

[GoRecv] internal static time.Duration fallbackDelay(this ref Dialer d) {
    if (d.FallbackDelay > 0){
        return d.FallbackDelay;
    } else {
        return 300 * time.Millisecond;
    }
}

internal static (@string afnet, nint proto, error err) parseNetwork(context.Context ctx, @string network, bool needsProto) {
    @string afnet = default!;
    error err = default!;

    nint i = bytealg.LastIndexByteString(network, (rune)':');
    if (i < 0) {
        // no colon
        var exprᴛ1 = network;
        if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        }
        else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        }
        else if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
            if (needsProto) {
                return ("", 0, ((UnknownNetworkError)network));
            }
        }
        else if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        }
        else { /* default: */
            return ("", 0, ((UnknownNetworkError)network));
        }

        return (network, 0, default!);
    }
    afnet = network.slice(0, i);
    var exprᴛ2 = afnet;
    if (exprᴛ2 == "ip"u8 || exprᴛ2 == "ip4"u8 || exprᴛ2 == "ip6"u8) {
        @string protostr = network.slice(i + 1);
        var (protoΔ2, iΔ2, ok) = dtoi(protostr);
        if (!ok || iΔ2 != len(protostr)) {
            (protoΔ2, err) = lookupProtocol(ctx, protostr);
            if (err != default!) {
                return ("", 0, err);
            }
        }
        return (afnet, protoΔ2, default!);
    }

    return ("", 0, ((UnknownNetworkError)network));
}

// resolveAddrList resolves addr using hint and returns a list of
// addresses. The result contains at least one address when error is
// nil.
internal static (addrList, error) resolveAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string op, @string network, @string addr, ΔAddr hint) {
    var (afnet, _, err) = parseNetwork(ctx, network, true);
    if (err != default!) {
        return (default!, err);
    }
    if (op == "dial"u8 && addr == ""u8) {
        return (default!, errMissingAddress);
    }
    var exprᴛ1 = afnet;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        var (addrΔ2, errΔ2) = ResolveUnixAddr(afnet, addr);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        if (op == "dial"u8 && hint != default! && addrΔ2.Network() != hint.Network()) {
            return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: "mismatched local address type"u8, Addr: hint.String()))));
        }
        return (new addrList(new ΔAddr[]{new UnixAddrжΔAddr(addrΔ2)}.slice()), default!);
    }

    (var addrs, err) = Ꮡr.internetAddrList(ctx, afnet, addr);
    if (err != default! || op != "dial"u8 || hint == default!) {
        return (addrs, err);
    }
    ж<TCPAddr> tcp = default!;
    ж<UDPAddr> udp = default!;
    ж<IPAddr> ip = default!;
    bool wildcard = default!;
    switch (hint.type()) {
    case ж<TCPAddr> hintΔ1: {
        tcp = hintΔ1;
        wildcard = tcp.isWildcard();
        break;
    }
    case ж<UDPAddr> hintΔ1: {
        udp = hintΔ1;
        wildcard = udp.isWildcard();
        break;
    }
    case ж<IPAddr> hintΔ1: {
        ip = hintΔ1;
        wildcard = ip.isWildcard();
        break;
    }}
    var naddrs = addrs[..0];
    foreach (var (_, addrΔ3) in addrs) {
        if (addrΔ3.Network() != hint.Network()) {
            return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: "mismatched local address type"u8, Addr: hint.String()))));
        }
        switch (addrΔ3.type()) {
        case ж<TCPAddr> addrΔ4: {
            if (!wildcard && !addrΔ4.isWildcard() && !(~addrΔ4).IP.matchAddrFamily((~tcp).IP)) {
                continue;
            }
            naddrs = append(naddrs, (ΔAddr)(new TCPAddrжΔAddr(addrΔ4)));
            break;
        }
        case ж<UDPAddr> addrΔ4: {
            if (!wildcard && !addrΔ4.isWildcard() && !(~addrΔ4).IP.matchAddrFamily((~udp).IP)) {
                continue;
            }
            naddrs = append(naddrs, (ΔAddr)(new UDPAddrжΔAddr(addrΔ4)));
            break;
        }
        case ж<IPAddr> addrΔ4: {
            if (!wildcard && !addrΔ4.isWildcard() && !(~addrΔ4).IP.matchAddrFamily((~ip).IP)) {
                continue;
            }
            naddrs = append(naddrs, (ΔAddr)(new IPAddrжΔAddr(addrΔ4)));
            break;
        }}
    }
    if (len(naddrs) == 0) {
        return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: hint.String()))));
    }
    return (naddrs, default!);
}

// MultipathTCP reports whether MPTCP will be used.
//
// This method doesn't check if MPTCP is supported by the operating
// system or not.
[GoRecv] public static bool MultipathTCP(this ref Dialer d) {
    return d.mptcpStatus.get();
}

// SetMultipathTCP directs the [Dial] methods to use, or not use, MPTCP,
// if supported by the operating system. This method overrides the
// system default and the GODEBUG=multipathtcp=... setting if any.
//
// If MPTCP is not available on the host or not supported by the server,
// the Dial methods will fall back to TCP.
[GoRecv] public static void SetMultipathTCP(this ref Dialer d, bool use) {
    d.mptcpStatus.set(use);
}

// Dial connects to the address on the named network.
//
// Known networks are "tcp", "tcp4" (IPv4-only), "tcp6" (IPv6-only),
// "udp", "udp4" (IPv4-only), "udp6" (IPv6-only), "ip", "ip4"
// (IPv4-only), "ip6" (IPv6-only), "unix", "unixgram" and
// "unixpacket".
//
// For TCP and UDP networks, the address has the form "host:port".
// The host must be a literal IP address, or a host name that can be
// resolved to IP addresses.
// The port must be a literal port number or a service name.
// If the host is a literal IPv6 address it must be enclosed in square
// brackets, as in "[2001:db8::1]:80" or "[fe80::1%zone]:80".
// The zone specifies the scope of the literal IPv6 address as defined
// in RFC 4007.
// The functions [JoinHostPort] and [SplitHostPort] manipulate a pair of
// host and port in this form.
// When using TCP, and the host resolves to multiple IP addresses,
// Dial will try each IP address in order until one succeeds.
//
// Examples:
//
//	Dial("tcp", "golang.org:http")
//	Dial("tcp", "192.0.2.1:http")
//	Dial("tcp", "198.51.100.1:80")
//	Dial("udp", "[2001:db8::1]:domain")
//	Dial("udp", "[fe80::1%lo0]:53")
//	Dial("tcp", ":80")
//
// For IP networks, the network must be "ip", "ip4" or "ip6" followed
// by a colon and a literal protocol number or a protocol name, and
// the address has the form "host". The host must be a literal IP
// address or a literal IPv6 address with zone.
// It depends on each operating system how the operating system
// behaves with a non-well known protocol number such as "0" or "255".
//
// Examples:
//
//	Dial("ip4:1", "192.0.2.1")
//	Dial("ip6:ipv6-icmp", "2001:db8::1")
//	Dial("ip6:58", "fe80::1%lo0")
//
// For TCP, UDP and IP networks, if the host is empty or a literal
// unspecified IP address, as in ":80", "0.0.0.0:80" or "[::]:80" for
// TCP and UDP, "", "0.0.0.0" or "::" for IP, the local system is
// assumed.
//
// For Unix networks, the address must be a file system path.
public static (Conn, error) Dial(@string network, @string address) {
    ref var d = ref heap(new Dialer(), out var Ꮡd);
    return Ꮡd.Dial(network, address);
}

// DialTimeout acts like [Dial] but takes a timeout.
//
// The timeout includes name resolution, if required.
// When using TCP, and the host in the address parameter resolves to
// multiple IP addresses, the timeout is spread over each consecutive
// dial, such that each is given an appropriate fraction of the time
// to connect.
//
// See func Dial for a description of the network and address
// parameters.
public static (Conn, error) DialTimeout(@string network, @string address, time.Duration timeout) {
    ref var d = ref heap<Dialer>(out var Ꮡd);
    d = new Dialer(Timeout: timeout);
    return Ꮡd.Dial(network, address);
}

// sysDialer contains a Dial's parameters and configuration.
[GoType] partial struct sysDialer {
    public partial ref Dialer Dialer { get; }
    internal @string network, address;
    internal Func<context.Context, @string, ж<TCPAddr>, ж<TCPAddr>, (ж<TCPConn>, error)> testHookDialTCP;
}

// Dial connects to the address on the named network.
//
// See func Dial for a description of the network and address
// parameters.
//
// Dial uses [context.Background] internally; to specify the context, use
// [Dialer.DialContext].
public static (Conn, error) Dial(this ж<Dialer> Ꮡd, @string network, @string address) {
    return Ꮡd.DialContext(context.Background(), network, address);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dialˢ = "dial"u8;

// DialContext connects to the address on the named network using
// the provided context.
//
// The provided Context must be non-nil. If the context expires before
// the connection is complete, an error is returned. Once successfully
// connected, any expiration of the context will not affect the
// connection.
//
// When using TCP, and the host in the address parameter resolves to multiple
// network addresses, any dial timeout (from d.Timeout or ctx) is spread
// over each consecutive dial, such that each is given an appropriate
// fraction of the time to connect.
// For example, if a host has 4 IP addresses and the timeout is 1 minute,
// the connect to each single address will be given 15 seconds to complete
// before trying the next one.
//
// See func [Dial] for a description of the network and address
// parameters.
public static partial (Conn, error) DialContext(this ж<Dialer> Ꮡd, context.Context ctx, @string network, @string address) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        if (ctx == default!) {
            throw panic("nil context");
        }
        var deadline = d.deadline(ctx, time.Now());
        if (!deadline.IsZero()) {
            testHookStepTime();
            {
                var (dΔ1, ok) = ctx.Deadline(); if (!ok || deadline.Before(dΔ1)) {
                    var (subCtx, cancel) = context.WithDeadline(ctx, deadline);
                    var cancelʗ1 = cancel;
                    defer(() => cancelʗ1(), ref ᒐ);
                    ctx = subCtx;
                }
            }
        }
        {
            var oldCancel = d.Cancel; if (oldCancel != default!) {
                var (subCtx, cancel) = context.WithCancel(ctx);
                var cancelʗ2 = cancel;
                defer(() => cancelʗ2(), ref ᒐ);
                var cancelʗ3 = cancel;
                var oldCancelʗ1 = oldCancel;
                var subCtxʗ1 = subCtx;
                goǃ(() => {
                    var selᴛ3 = oldCancelʗ1;
                    var selᴛ4 = subCtxʗ1.Done();
                    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
                    case 0 when selᴛ3.ꟷᐳ(out _): {
                        cancelʗ3();
                        break;
                    }
                    case 1 when selᴛ4.ꟷᐳ(out _): {
                        break;
                    }}
                });
                ctx = subCtx;
            }
        }
        // Shadow the nettrace (if any) during resolve so Connect events don't fire for DNS lookups.
        var resolveCtx = ctx;
        {
            var (trace, _) = ctx.Value(new nettrace.TraceKey(nil))._<ж<nettrace.Trace>>(ᐧ); if (trace != nil) {
                ref var shadow = ref heap<nettrace.Trace>(out var Ꮡshadow);
                shadow = trace.Value;
                shadow.ConnectStart = default!;
                shadow.ConnectDone = default!;
                resolveCtx = context.WithValue(resolveCtx, new nettrace.TraceKey(nil), Ꮡshadow);
            }
        }
        var (addrs, err) = d.resolver().resolveAddrList(resolveCtx, dialˢ, network, address, d.LocalAddr);
        if (err != default!) {
            return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: network, Source: default!, Addr: default!, Err: err))));
        }
        var sd = Ꮡ(new sysDialer(
            Dialer: d,
            network: network,
            address: address
        ));
        addrList primaries = default!;
        addrList fallbacks = default!;
        if (d.dualStack() && network == "tcp"u8){
            (primaries, fallbacks) = addrs.partition(isIPv4);
        } else {
            primaries = addrs;
        }
        return sd.dialParallel(ctx, primaries, fallbacks);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct dialParallel_dialResult {
    [GoEmbedded] public Conn Conn;
    [GoEmbedded] internal error error;
    internal bool primary;
    internal bool done;
}

// dialParallel races two copies of dialSerial, giving the first a
// head start. It returns the first established connection and
// closes the others. Otherwise it returns an error from the first
// primary address.
internal static partial (Conn, error) dialParallel(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList primaries, addrList fallbacks) {
    GoFrame ᒐ = default;
    try {
        if (len(fallbacks) == 0) {
            return Ꮡsd.dialSerial(ctx, primaries);
        }
        var returned = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), returned, ref ᒐ);
        var results = new channel<dialParallel_dialResult>(0); // unbuffered
        var fallbacksʗ1 = fallbacks;
        var primariesʗ1 = primaries;
        var resultsʗ1 = results;
        var returnedʗ1 = returned;
        void startRacer(context.Context ctxΔ1, bool primaryΔ1) {
            var ras = primariesʗ1;
            if (!primaryΔ1) {
                ras = fallbacksʗ1;
            }
            var (c, err) = Ꮡsd.dialSerial(ctxΔ1, ras);
            var selᴛ5 = resultsʗ1.ᐸꟷ(new dialParallel_dialResult(Conn: c, error: err, primary: primaryΔ1, done: true), ꓸꓸꓸ);
            var selᴛ6 = returnedʗ1;
            switch (select(selᴛ5, ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0: {
                break;
            }
            case 1 when selᴛ6.ꟷᐳ(out _): {
                if (c != default!) {
                    c.Close();
                }
                break;
            }}
        }
        dialParallel_dialResult primary = default!;
        dialParallel_dialResult fallback = default!;
        // Start the main racer.
        var (primaryCtx, primaryCancel) = context.WithCancel(ctx);
        var primaryCancelʗ1 = primaryCancel;
        defer(() => primaryCancelʗ1(), ref ᒐ);
        var startRacerʗ1 = startRacer;
        goǃ(startRacerʗ1, primaryCtx, (bool)true);
        // Start the timer for the fallback racer.
        var fallbackTimer = time.NewTimer(Ꮡsd.of(sysDialer.ᏑDialer).fallbackDelay());
        var fallbackTimerʗ1 = fallbackTimer;
        defer(() => fallbackTimerʗ1.Stop(), ref ᒐ);
        while (ᐧ) {
            var selᴛ7 = (~fallbackTimer).C;
            var selᴛ8 = results;
            switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out _): {
                var (fallbackCtx, fallbackCancel) = context.WithCancel(ctx);
                var fallbackCancelʗ1 = fallbackCancel;
                defer(() => fallbackCancelʗ1(), ref ᒐ);
                var startRacerʗ2 = startRacer;
                goǃ(startRacerʗ2, fallbackCtx, (bool)false);
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out var res): {
                if (res.error == default!) {
                    return (res.Conn, default!);
                }
                if (res.primary){
                    primary = res;
                } else {
                    fallback = res;
                }
                if (primary.done && fallback.done) {
                    return (default!, primary.error);
                }
                if (res.primary && fallbackTimer.Stop()) {
                    // If we were able to stop the timer, that means it
                    // was running (hadn't yet started the fallback), but
                    // we just got an error on the primary path, so start
                    // the fallback immediately (in 0 nanoseconds).
                    fallbackTimer.Reset(0);
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// dialSerial connects to a list of addresses in sequence, returning
// either the first successful connection, or the first error.
internal static (Conn, error) dialSerial(this ж<sysDialer> Ꮡsd, context.Context ctx, addrList ras) {
    GoFrame ᒐ = default;
    try {
        ref var sd = ref Ꮡsd.DerefOrNull();

        error firstErr = default!;    // The error from the first address is most relevant.
        foreach (var (i, ra) in ras) {
            var selᴛ9 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out _): {
                return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: sd.network, Source: sd.LocalAddr, Addr: ra, Err: mapErr(ctx.Err())))));
            }
            default: {
                break;
            }}
            var dialCtx = ctx;
            {
                var (deadline, hasDeadline) = ctx.Deadline(); if (hasDeadline) {
                    var (partialDeadlineΔ1, errΔ1) = partialDeadline(time.Now(), deadline, len(ras) - i);
                    if (errΔ1 != default!) {
                        // Ran out of time.
                        if (firstErr == default!) {
                            firstErr = new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: sd.network, Source: sd.LocalAddr, Addr: ra, Err: errΔ1)));
                        }
                        break;
                    }
                    if (partialDeadlineΔ1.Before(deadline)) {
                        Action cancel = default!;
                        (dialCtx, cancel) = context.WithDeadline(ctx, partialDeadlineΔ1);
                        var cancelʗ1 = cancel;
                        defer(() => cancelʗ1(), ref ᒐ);
                    }
                }
            }
            var (c, err) = Ꮡsd.dialSingle(dialCtx, ra);
            if (err == default!) {
                return (c, default!);
            }
            if (firstErr == default!) {
                firstErr = err;
            }
        }
        if (firstErr == default!) {
            firstErr = new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: sd.network, Source: default!, Addr: default!, Err: errMissingAddress)));
        }
        return (default!, firstErr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// dialSingle attempts to establish and returns a single connection to
// the destination address.
internal static (Conn c, error err) dialSingle(this ж<sysDialer> Ꮡsd, context.Context ctx, ΔAddr ra) {
    Conn c = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var sd = ref Ꮡsd.DerefOrNull();

        var (trace, _) = ctx.Value(new nettrace.TraceKey(nil))._<ж<nettrace.Trace>>(ᐧ);
        if (trace != nil) {
            @string raStr = ra.String();
            if ((~trace).ConnectStart != default!) {
                (~trace).ConnectStart(sd.network, raStr);
            }
            if ((~trace).ConnectDone != default!) {
                var traceʗ1 = trace;
                defer(() => {
                    (~traceʗ1).ConnectDone(Ꮡsd.Value.network, raStr, err);
                }, ref ᒐ);
            }
        }
        var la = sd.LocalAddr;
        switch (ra.type()) {
        case ж<TCPAddr> raΔ1: {
            var (laΔ1, _) = la._<ж<TCPAddr>>(ᐧ);
            if (Ꮡsd.of(sysDialer.ᏑDialer).MultipathTCP()){
                var (ᴛ1, ᴛ2) = Ꮡsd.dialMPTCP(ctx, laΔ1, raΔ1);
                (c, err) = (new TCPConnжConn(ᴛ1), ᴛ2);
            } else {
                var (ᴛ1, ᴛ2) = Ꮡsd.dialTCP(ctx, laΔ1, raΔ1);
                (c, err) = (new TCPConnжConn(ᴛ1), ᴛ2);
            }
            break;
        }
        case ж<UDPAddr> raΔ1: {
            var (laΔ2, _) = la._<ж<UDPAddr>>(ᐧ);
            var (ᴛ1, ᴛ2) = Ꮡsd.dialUDP(ctx, laΔ2, raΔ1);
            (c, err) = (new UDPConnжConn(ᴛ1), ᴛ2);
            break;
        }
        case ж<IPAddr> raΔ1: {
            var (laΔ3, _) = la._<ж<IPAddr>>(ᐧ);
            var (ᴛ1, ᴛ2) = Ꮡsd.dialIP(ctx, laΔ3, raΔ1);
            (c, err) = (new IPConnжConn(ᴛ1), ᴛ2);
            break;
        }
        case ж<UnixAddr> raΔ1: {
            var (laΔ4, _) = la._<ж<UnixAddr>>(ᐧ);
            var (ᴛ1, ᴛ2) = Ꮡsd.dialUnix(ctx, laΔ4, raΔ1);
            (c, err) = (new UnixConnжConn(ᴛ1), ᴛ2);
            break;
        }
        default: {
            var raΔ1 = ra;
            (c, err) = (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: sd.network, Source: la, Addr: raΔ1, Err: new AddrErrorжerror(Ꮡ(new AddrError(Err: "unexpected address type"u8, Addr: sd.address))))))); goto ᒐdone;
        }}
        if (err != default!) {
            (c, err) = (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: sd.network, Source: la, Addr: ra, Err: err)))); goto ᒐdone; // c is non-nil interface containing nil pointer
        }
        (c, err) = (c, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (c, err);
}

// ListenConfig contains options for listening to an address.
[GoType] partial struct ListenConfig {
    // If Control is not nil, it is called after creating the network
    // connection but before binding it to the operating system.
    //
    // Network and address parameters passed to Control function are not
    // necessarily the ones passed to Listen. Calling Listen with TCP networks
    // will cause the Control function to be called with "tcp4" or "tcp6",
    // UDP networks become "udp4" or "udp6", IP networks become "ip4" or "ip6",
    // and other known networks are passed as-is.
    public Func<@string, @string, syscall.RawConn, error> Control;
    // KeepAlive specifies the keep-alive period for network
    // connections accepted by this listener.
    //
    // KeepAlive is ignored if KeepAliveConfig.Enable is true.
    //
    // If zero, keep-alive are enabled if supported by the protocol
    // and operating system. Network protocols or operating systems
    // that do not support keep-alive ignore this field.
    // If negative, keep-alive are disabled.
    public time.Duration KeepAlive;
    // KeepAliveConfig specifies the keep-alive probe configuration
    // for an active network connection, when supported by the
    // protocol and operating system.
    //
    // If KeepAliveConfig.Enable is true, keep-alive probes are enabled.
    // If KeepAliveConfig.Enable is false and KeepAlive is negative,
    // keep-alive probes are disabled.
    public KeepAliveConfig KeepAliveConfig;
    // If mptcpStatus is set to a value allowing Multipath TCP (MPTCP) to be
    // used, any call to Listen with "tcp(4|6)" as network will use MPTCP if
    // supported by the operating system.
    internal mptcpStatusListen mptcpStatus;
}

// MultipathTCP reports whether MPTCP will be used.
//
// This method doesn't check if MPTCP is supported by the operating
// system or not.
[GoRecv] public static bool MultipathTCP(this ref ListenConfig lc) {
    return lc.mptcpStatus.get();
}

// SetMultipathTCP directs the [Listen] method to use, or not use, MPTCP,
// if supported by the operating system. This method overrides the
// system default and the GODEBUG=multipathtcp=... setting if any.
//
// If MPTCP is not available on the host or not supported by the client,
// the Listen method will fall back to TCP.
[GoRecv] public static void SetMultipathTCP(this ref ListenConfig lc, bool use) {
    lc.mptcpStatus.set(use);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string listenˢ = "listen"u8;

// Listen announces on the local network address.
//
// See func Listen for a description of the network and address
// parameters.
//
// The ctx argument is used while resolving the address on which to listen;
// it does not affect the returned Listener.
[GoRecv] public static (Listener, error) Listen(this ref ListenConfig lc, context.Context ctx, @string network, @string address) {
    var (addrs, err) = DefaultResolver.resolveAddrList(ctx, listenˢ, network, address, default!);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: default!, Err: err))));
    }
    var sl = Ꮡ(new sysListener(
        ListenConfig: lc,
        network: network,
        address: address
    ));
    Listener l = default!;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
    case ж<TCPAddr> laΔ1: {
        if (sl.of(sysListener.ᏑListenConfig).MultipathTCP()){
            var (ᴛ1, ᴛ2) = sl.listenMPTCP(ctx, laΔ1);
            (l, err) = (new TCPListenerжListener(ᴛ1), ᴛ2);
        } else {
            var (ᴛ1, ᴛ2) = sl.listenTCP(ctx, laΔ1);
            (l, err) = (new TCPListenerжListener(ᴛ1), ᴛ2);
        }
        break;
    }
    case ж<UnixAddr> laΔ1: {
        var (ᴛ1, ᴛ2) = sl.listenUnix(ctx, laΔ1);
        (l, err) = (new UnixListenerжListener(ᴛ1), ᴛ2);
        break;
    }
    default: {
        var laΔ1 = la;
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, Addr: laΔ1, Err: new AddrErrorжerror(Ꮡ(new AddrError(Err: "unexpected address type"u8, Addr: address)))))));
    }}
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, Addr: la, Err: err)))); // l is non-nil interface containing nil pointer
    }
    return (l, default!);
}

// ListenPacket announces on the local network address.
//
// See func ListenPacket for a description of the network and address
// parameters.
//
// The ctx argument is used while resolving the address on which to listen;
// it does not affect the returned Listener.
[GoRecv] public static (PacketConn, error) ListenPacket(this ref ListenConfig lc, context.Context ctx, @string network, @string address) {
    var (addrs, err) = DefaultResolver.resolveAddrList(ctx, listenˢ, network, address, default!);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: default!, Err: err))));
    }
    var sl = Ꮡ(new sysListener(
        ListenConfig: lc,
        network: network,
        address: address
    ));
    PacketConn c = default!;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
    case ж<UDPAddr> laΔ1: {
        var (ᴛ1, ᴛ2) = sl.listenUDP(ctx, laΔ1);
        (c, err) = (new UDPConnжPacketConn(ᴛ1), ᴛ2);
        break;
    }
    case ж<IPAddr> laΔ1: {
        var (ᴛ1, ᴛ2) = sl.listenIP(ctx, laΔ1);
        (c, err) = (new IPConnжPacketConn(ᴛ1), ᴛ2);
        break;
    }
    case ж<UnixAddr> laΔ1: {
        var (ᴛ1, ᴛ2) = sl.listenUnixgram(ctx, laΔ1);
        (c, err) = (new UnixConnжPacketConn(ᴛ1), ᴛ2);
        break;
    }
    default: {
        var laΔ1 = la;
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, Addr: laΔ1, Err: new AddrErrorжerror(Ꮡ(new AddrError(Err: "unexpected address type"u8, Addr: address)))))));
    }}
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, Addr: la, Err: err)))); // c is non-nil interface containing nil pointer
    }
    return (c, default!);
}

// sysListener contains a Listen's parameters and configuration.
[GoType] partial struct sysListener {
    public partial ref ListenConfig ListenConfig { get; }
    internal @string network, address;
}

// Listen announces on the local network address.
//
// The network must be "tcp", "tcp4", "tcp6", "unix" or "unixpacket".
//
// For TCP networks, if the host in the address parameter is empty or
// a literal unspecified IP address, Listen listens on all available
// unicast and anycast IP addresses of the local system.
// To only use IPv4, use network "tcp4".
// The address can use a host name, but this is not recommended,
// because it will create a listener for at most one of the host's IP
// addresses.
// If the port in the address parameter is empty or "0", as in
// "127.0.0.1:" or "[::1]:0", a port number is automatically chosen.
// The [Addr] method of [Listener] can be used to discover the chosen
// port.
//
// See func [Dial] for a description of the network and address
// parameters.
//
// Listen uses context.Background internally; to specify the context, use
// [ListenConfig.Listen].
public static (Listener, error) Listen(@string network, @string address) {
    ListenConfig lc = default!;
    return lc.Listen(context.Background(), network, address);
}

// ListenPacket announces on the local network address.
//
// The network must be "udp", "udp4", "udp6", "unixgram", or an IP
// transport. The IP transports are "ip", "ip4", or "ip6" followed by
// a colon and a literal protocol number or a protocol name, as in
// "ip:1" or "ip:icmp".
//
// For UDP and IP networks, if the host in the address parameter is
// empty or a literal unspecified IP address, ListenPacket listens on
// all available IP addresses of the local system except multicast IP
// addresses.
// To only use IPv4, use network "udp4" or "ip4:proto".
// The address can use a host name, but this is not recommended,
// because it will create a listener for at most one of the host's IP
// addresses.
// If the port in the address parameter is empty or "0", as in
// "127.0.0.1:" or "[::1]:0", a port number is automatically chosen.
// The LocalAddr method of [PacketConn] can be used to discover the
// chosen port.
//
// See func [Dial] for a description of the network and address
// parameters.
//
// ListenPacket uses context.Background internally; to specify the context, use
// [ListenConfig.ListenPacket].
public static (PacketConn, error) ListenPacket(@string network, @string address) {
    ListenConfig lc = default!;
    return lc.ListenPacket(context.Background(), network, address);
}

} // end net_package
