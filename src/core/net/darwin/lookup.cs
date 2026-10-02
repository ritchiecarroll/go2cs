// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using nettrace = @internal.nettrace_package;
using singleflight = @internal.singleflight_package;
using netip = net.netip_package;
using Δsync = sync_package;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using @internal;
using System.Runtime.CompilerServices;
using net;
using time = time_package;
using vendor.golang.org.x.net.dns;

partial class net_package {

// protocols contains minimal mappings between internet protocol
// names and numbers for platforms that don't have a complete list of
// protocol numbers.
//
// See https://www.iana.org/assignments/protocol-numbers
//
// On Unix, this map is augmented by readProtocols via lookupProtocol.
internal static map<@string, nint> protocols = new map<@string, nint>{
    ["icmp"u8] = 1,
    ["igmp"u8] = 2,
    ["tcp"u8] = 6,
    ["udp"u8] = 17,
    ["ipv6-icmp"u8] = 58
};

// ʕ◔ϖ◔ʔ
// services contains minimal mappings between services names and port
// numbers for platforms that don't have a complete list of port numbers.
//
// See https://www.iana.org/assignments/service-names-port-numbers
//
// On Unix, this map is augmented by readServices via goLookupPort.
internal static map<@string, map<@string, nint>> services = new map<@string, map<@string, nint>>{
    ["udp"u8] = new map<@string, nint>{
        ["domain"u8] = 53},
    ["tcp"u8] = new map<@string, nint>{
        ["ftp"u8] = 21,
        ["ftps"u8] = 990,
        ["gopher"u8] = 70,
        ["http"u8] = 80,
        ["https"u8] = 443,
        ["imap2"u8] = 143,
        ["imap3"u8] = 220,
        ["imaps"u8] = 993,
        ["pop3"u8] = 110,
        ["pop3s"u8] = 995,
        ["smtp"u8] = 25,
        ["submissions"u8] = 465,
        ["ssh"u8] = 22,
        ["telnet"u8] = 23}
};

// dnsWaitGroup can be used by tests to wait for all DNS goroutines to
// complete. This avoids races on the test hooks.
internal static ж<Δsync.WaitGroup> ᏑdnsWaitGroup = new StandardBox<Δsync.WaitGroup>(default(Δsync.WaitGroup));
internal static ref Δsync.WaitGroup dnsWaitGroup => ref ᏑdnsWaitGroup.Value;

internal const nint maxProtoLength = /* len("RSVP-E2E-IGNORE") + 10 */ 25; // with room to grow

internal static (nint, error) lookupProtocolMap(@string name) {
    array<byte> lowerProtocol = new(25); /* maxProtoLength */
    nint n = copy(lowerProtocol[..], name);
    lowerASCIIBytes(lowerProtocol.slice(0, n));
    var (proto, found) = protocols[tmpstring(lowerProtocol.slice(0, n)), ꟷ];
    if (!found || n != len(name)) {
        return (0, new AddrErrorжerror(Ꮡ(new AddrError(Err: "unknown IP protocol specified"u8, Addr: name))));
    }
    return (proto, default!);
}

// maxPortBufSize is the longest reasonable name of a service
// (non-numeric port).
// Currently the longest known IANA-unregistered name is
// "mobility-header", so we use that length, plus some slop in case
// something longer is added in the future.
internal const nint maxPortBufSize = /* len("mobility-header") + 10 */ 25;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

internal static (nint port, error error) lookupPortMap(@string network, @string service) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8) {
        {
            var (p, err) = lookupPortMapWithNetwork(tcpˢ, // no hints
 "ip"u8, service); if (err == default!) {
                return (p, default!);
            }
        }
        return lookupPortMapWithNetwork(udpˢ, "ip"u8, service);
    }
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        return lookupPortMapWithNetwork(tcpˢ, tcpˢ, service);
    }
    if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        return lookupPortMapWithNetwork(udpˢ, udpˢ, service);
    }

    return (0, new DNSErrorжerror(Ꮡ(new DNSError(Err: "unknown network"u8, Name: network + "/"u8 + service))));
}

internal static (nint port, error error) lookupPortMapWithNetwork(@string network, @string errNetwork, @string service) {
    {
        var (m, ok) = services[network, ꟷ]; if (ok) {
            array<byte> lowerService = new(25); /* maxPortBufSize */
            nint n = copy(lowerService[..], service);
            lowerASCIIBytes(lowerService.slice(0, n));
            {
                var (portΔ1, okΔ1) = m[tmpstring(lowerService.slice(0, n)), ꟷ]; if (okΔ1 && n == len(service)) {
                    return (portΔ1, default!);
                }
            }
            return (0, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errUnknownPort), errNetwork + "/"u8 + service, ""u8)));
        }
    }
    return (0, new DNSErrorжerror(Ꮡ(new DNSError(Err: "unknown network"u8, Name: errNetwork + "/"u8 + service))));
}

// ipVersion returns the provided network's IP version: '4', '6' or 0
// if network does not end in a '4' or '6' byte.
internal static byte ipVersion(@string network) {
    if (network == ""u8) {
        return 0;
    }
    var n = network[len(network) - 1];
    if (n != (rune)'4' && n != (rune)'6') {
        n = 0;
    }
    return n;
}

// DefaultResolver is the resolver used by the package-level Lookup
// functions and by Dialers without a specified Resolver.
public static ж<ж<Resolver>> ᏑDefaultResolver = new StandardBox<ж<Resolver>>(Ꮡ(new Resolver(nil)));
public static ref ж<Resolver> DefaultResolver => ref ᏑDefaultResolver.ValueSlot;

// A Resolver looks up names and numbers.
//
// A nil *Resolver is equivalent to a zero Resolver.
[GoType] partial struct Resolver {
    // PreferGo controls whether Go's built-in DNS resolver is preferred
    // on platforms where it's available. It is equivalent to setting
    // GODEBUG=netdns=go, but scoped to just this resolver.
    public bool PreferGo;
    // StrictErrors controls the behavior of temporary errors
    // (including timeout, socket errors, and SERVFAIL) when using
    // Go's built-in resolver. For a query composed of multiple
    // sub-queries (such as an A+AAAA address lookup, or walking the
    // DNS search list), this option causes such errors to abort the
    // whole query instead of returning a partial result. This is
    // not enabled by default because it may affect compatibility
    // with resolvers that process AAAA queries incorrectly.
    public bool StrictErrors;
    // Dial optionally specifies an alternate dialer for use by
    // Go's built-in DNS resolver to make TCP and UDP connections
    // to DNS services. The host in the address parameter will
    // always be a literal IP address and not a host name, and the
    // port in the address parameter will be a literal port number
    // and not a service name.
    // If the Conn returned is also a PacketConn, sent and received DNS
    // messages must adhere to RFC 1035 section 4.2.1, "UDP usage".
    // Otherwise, DNS messages transmitted over Conn must adhere
    // to RFC 7766 section 5, "Transport Protocol Selection".
    // If nil, the default dialer is used.
    public Func<context.Context, @string, @string, (Conn, error)> Dial;
    // lookupGroup merges LookupIPAddr calls together for lookups for the same
    // host. The lookupGroup key is the LookupIPAddr.host argument.
    // The return values are ([]IPAddr, error).
    internal singleflight.Group lookupGroup;
}

// TODO(bradfitz): optional interface impl override hook
// TODO(bradfitz): Timeout time.Duration?
internal static bool preferGo(this ж<Resolver> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    return Ꮡr != nil && r.PreferGo;
}

internal static bool strictErrors(this ж<Resolver> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    return Ꮡr != nil && r.StrictErrors;
}

internal static ж<singleflight.Group> getLookupGroup(this ж<Resolver> Ꮡr) {
    if (Ꮡr == nil) {
        return DefaultResolver.of(Resolver.ᏑlookupGroup);
    }
    return Ꮡr.of(Resolver.ᏑlookupGroup);
}

// LookupHost looks up the given host using the local resolver.
// It returns a slice of that host's addresses.
//
// LookupHost uses [context.Background] internally; to specify the context, use
// [Resolver.LookupHost].
public static (slice<@string> addrs, error err) LookupHost(@string host) {
    return DefaultResolver.LookupHost(context.Background(), host);
}

// LookupHost looks up the given host using the local resolver.
// It returns a slice of that host's addresses.
public static (slice<@string> addrs, error err) LookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) {
    // Make sure that no matter what we do later, host=="" is rejected.
    if (host == ""u8) {
        return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), host, ""u8)));
    }
    {
        var (_, errΔ1) = netip.ParseAddr(host); if (errΔ1 == default!) {
            return (new @string[]{host}.slice(), default!);
        }
    }
    return Ꮡr.lookupHost(ctx, host);
}

// LookupIP looks up host using the local resolver.
// It returns a slice of that host's IPv4 and IPv6 addresses.
public static (slice<IP>, error) LookupIP(@string host) {
    var (addrs, err) = DefaultResolver.LookupIPAddr(context.Background(), host);
    if (err != default!) {
        return (default!, err);
    }
    var ips = new slice<IP>(len(addrs));
    foreach (var (i, ia) in addrs) {
        ips[i] = ia.IP;
    }
    return (ips, default!);
}

// LookupIPAddr looks up host using the local resolver.
// It returns a slice of that host's IPv4 and IPv6 addresses.
public static (slice<IPAddr>, error) LookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string host) {
    return Ꮡr.lookupIPAddr(ctx, "ip"u8, host);
}

// LookupIP looks up host for the given network using the local resolver.
// It returns a slice of that host's IP addresses of the type specified by
// network.
// network must be one of "ip", "ip4" or "ip6".
public static (slice<IP>, error) LookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) {
    var (afnet, _, err) = parseNetwork(ctx, network, false);
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = afnet;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

    if (host == ""u8) {
        return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), host, ""u8)));
    }
    (var addrs, err) = Ꮡr.internetAddrList(ctx, afnet, host);
    if (err != default!) {
        return (default!, err);
    }
    var ips = new slice<IP>(0, len(addrs));
    foreach (var (_, addr) in addrs) {
        ips = append(ips, (~addr._<ж<IPAddr>>()).IP);
    }
    return (ips, default!);
}

// LookupNetIP looks up host using the local resolver.
// It returns a slice of that host's IP addresses of the type specified by
// network.
// The network must be one of "ip", "ip4" or "ip6".
public static (slice<netipꓸAddr>, error) LookupNetIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) {
    // TODO(bradfitz): make this efficient, making the internal net package
    // type throughout be netip.Addr and only converting to the net.IP slice
    // version at the edge. But for now (2021-10-20), this is a wrapper around
    // the old way.
    var (ips, err) = Ꮡr.LookupIP(ctx, network, host);
    if (err != default!) {
        return (default!, err);
    }
    var ret = new slice<netipꓸAddr>(0, len(ips));
    foreach (var (_, ip) in ips) {
        {
            var (a, ok) = netip.AddrFromSlice(ip); if (ok) {
                ret = append(ret, a);
            }
        }
    }
    return (ret, default!);
}

// onlyValuesCtx is a context that uses an underlying context
// for value lookup if the underlying context hasn't yet expired.
[GoType] partial struct onlyValuesCtx {
    [GoEmbedded] public context_package.Context Context;
    internal context.Context lookupValues;
}

// Go method set entry for the promoted 'Context.Deadline()' - provided ONLY by the embedded
// interface field in *onlyValuesCtx's method set; see the pointer-only satisfaction record.
internal static (time.Time, bool) Deadline(this onlyValuesCtx recvᴛ) => recvᴛ.Context.Deadline();

// Go method set entry for the promoted 'Context.Done()' - provided ONLY by the embedded
// interface field in *onlyValuesCtx's method set; see the pointer-only satisfaction record.
internal static /*<-*/channel<EmptyStruct> Done(this onlyValuesCtx recvᴛ) => recvᴛ.Context.Done();

// Go method set entry for the promoted 'Context.Err()' - provided ONLY by the embedded
// interface field in *onlyValuesCtx's method set; see the pointer-only satisfaction record.
internal static error Err(this onlyValuesCtx recvᴛ) => recvᴛ.Context.Err();

internal static context.Context _ᴛ1ʗ = new onlyValuesCtxжContext(((ж<onlyValuesCtx>)nil));

// Value performs a lookup if the original context hasn't expired.
[GoRecv] internal static any Value(this ref onlyValuesCtx ovc, any key) {
    var selᴛ15 = ovc.lookupValues.Done();
    switch (trySelect(ᐸꟷ(selᴛ15, ꓸꓸꓸ))) {
    case 0 when selᴛ15.ꟷᐳ(out _): {
        return default!;
    }
    default: {
        return ovc.lookupValues.Value(key);
    }}
}

// withUnexpiredValuesPreserved returns a context.Context that only uses lookupCtx
// for its values, otherwise it is never canceled and has no deadline.
// If the lookup context expires, any looked up values will return nil.
// See Issue 28600.
internal static context.Context withUnexpiredValuesPreserved(context.Context lookupCtx) {
    return new onlyValuesCtxжContext(Ꮡ(new onlyValuesCtx(Context: context.Background(), lookupValues: lookupCtx)));
}

// lookupIPAddr looks up host using the local resolver and particular network.
// It returns a slice of that host's IPv4 and IPv6 addresses.
[MethodImpl(MethodImplOptions.NoInlining)] internal static (slice<IPAddr>, error) lookupIPAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) {
    // Make sure that no matter what we do later, host=="" is rejected.
    if (host == ""u8) {
        return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), host, ""u8)));
    }
    {
        var (ip, err) = netip.ParseAddr(host); if (err == default!) {
            return (new IPAddr[]{new(IP: ((IP)ip.AsSlice()).To16(), Zone: ip.Zone())}.slice(), default!);
        }
    }
    var (trace, _) = ctx.Value(new nettrace.TraceKey(nil))._<ж<nettrace.Trace>>(ᐧ);
    if (trace != nil && (~trace).DNSStart != default!) {
        (~trace).DNSStart(host);
    }
    // The underlying resolver func is lookupIP by default but it
    // can be overridden by tests. This is needed by net/http, so it
    // uses a context key instead of unexported variables.
    var resolverFunc = (context.Context p1, @string p2, @string p3) => Ꮡr.lookupIP(p1, p2, p3);
    {
        var (alt, _) = ctx.Value(new nettrace.LookupIPAltResolverKey(nil))._<Func<context.Context, @string, @string, (slice<IPAddr>, error)>>(ᐧ); if (alt != default!) {
            resolverFunc = alt;
        }
    }
    // We don't want a cancellation of ctx to affect the
    // lookupGroup operation. Otherwise if our context gets
    // canceled it might cause an error to be returned to a lookup
    // using a completely different context. However we need to preserve
    // only the values in context. See Issue 28600.
    var (lookupGroupCtx, lookupGroupCancel) = context.WithCancel(withUnexpiredValuesPreserved(ctx));
    @string lookupKey = network + "\u0000"u8 + host;
    ᏑdnsWaitGroup.Add(1);
    var lookupGroupCtxʗ1 = lookupGroupCtx;
    var resolverFuncʗ1 = resolverFunc;
    var ch = Ꮡr.getLookupGroup().DoChan(lookupKey, () => {
        var (ᴛ1, ᴛ2) = testHookLookupIP(lookupGroupCtxʗ1, resolverFuncʗ1, network, host);
        return (ᴛ1, ᴛ2);
    });
    void dnsWaitGroupDone(/*<-*/channel<singleflight.Result> chΔ1, Action cancelFn) {
        ᐸꟷ(chΔ1);
        ᏑdnsWaitGroup.Done();
        cancelFn();
    }
    var selᴛ16 = ctx.Done();
    var selᴛ17 = ch;
    switch (select(ᐸꟷ(selᴛ16, ꓸꓸꓸ), ᐸꟷ(selᴛ17, ꓸꓸꓸ))) {
    case 0 when selᴛ16.ꟷᐳ(out _): {
        if (Ꮡr.getLookupGroup().ForgetUnshared(lookupKey)){
            // Our context was canceled. If we are the only
            // goroutine looking up this key, then drop the key
            // from the lookupGroup and cancel the lookup.
            // If there are other goroutines looking up this key,
            // let the lookup continue uncanceled, and let later
            // lookups with the same key share the result.
            // See issues 8602, 20703, 22724.
            lookupGroupCancel();
            var dnsWaitGroupDoneʗ1 = dnsWaitGroupDone;
            goǃ(dnsWaitGroupDoneʗ1, ch, () => {
            });
        } else {
            var dnsWaitGroupDoneʗ2 = dnsWaitGroupDone;
            goǃ(dnsWaitGroupDoneʗ2, ch, lookupGroupCancel);
        }
        var err = newDNSError(mapErr(ctx.Err()), host, ""u8);
        if (trace != nil && (~trace).DNSDone != default!) {
            (~trace).DNSDone(default!, false, new DNSErrorжerror(err));
        }
        return (default!, new DNSErrorжerror(err));
    }
    case 1 when selᴛ17.ꟷᐳ(out var rΔ1): {
        ᏑdnsWaitGroup.Done();
        lookupGroupCancel();
        var err = rΔ1.Err;
        if (err != default!) {
            {
                var (_, ok) = err._<ж<DNSError>>(ᐧ); if (!ok) {
                    err = new DNSErrorжerror(newDNSError(mapErr(err), host, ""u8));
                }
            }
        }
        if (trace != nil && (~trace).DNSDone != default!) {
            var (addrs, _) = rΔ1.Val._<slice<IPAddr>>(ᐧ);
            (~trace).DNSDone(ipAddrsEface(addrs), rΔ1.Shared, err);
        }
        return lookupIPReturn(rΔ1.Val, err, rΔ1.Shared);
    }}
    return default!;
}

// lookupIPReturn turns the return values from singleflight.Do into
// the return values from LookupIP.
internal static (slice<IPAddr>, error) lookupIPReturn(any addrsi, error err, bool shared) {
    if (err != default!) {
        return (default!, err);
    }
    var addrs = addrsi._<slice<IPAddr>>();
    if (shared) {
        var clone = new slice<IPAddr>(len(addrs));
        copy(clone, addrs);
        addrs = clone;
    }
    return (addrs, default!);
}

// ipAddrsEface returns an empty interface slice of addrs.
internal static slice<any> ipAddrsEface(slice<IPAddr> addrs) {
    var s = new slice<any>(len(addrs));
    foreach (var (i, v) in addrs) {
        s[i] = v;
    }
    return s;
}

// LookupPort looks up the port for the given network and service.
//
// LookupPort uses [context.Background] internally; to specify the context, use
// [Resolver.LookupPort].
public static (nint port, error err) LookupPort(@string network, @string service) {
    return DefaultResolver.LookupPort(context.Background(), network, service);
}

// LookupPort looks up the port for the given network and service.
//
// The network must be one of "tcp", "tcp4", "tcp6", "udp", "udp4", "udp6" or "ip".
public static (nint port, error err) LookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) {
    nint port = default!;
    error err = default!;

    (port, var needsLookup) = parsePort(service);
    if (needsLookup) {
        var exprᴛ1 = network;
        if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8 || exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8 || exprᴛ1 == "ip"u8) {
        }
        else if (exprᴛ1 == ""u8) {
            network = "ip"u8;
        }
        else { /* default: */
            return (0, new AddrErrorжerror(Ꮡ(new AddrError( // a hint wildcard for Go 1.0 undocumented behavior
Err: "unknown network"u8, Addr: network))));
        }

        (port, err) = Ꮡr.lookupPort(ctx, network, service);
        if (err != default!) {
            return (0, err);
        }
    }
    if (0 > port || port > 65535) {
        return (0, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid port"u8, Addr: service))));
    }
    return (port, default!);
}

// LookupCNAME returns the canonical name for the given host.
// Callers that do not care about the canonical name can call
// [LookupHost] or [LookupIP] directly; both take care of resolving
// the canonical name as part of the lookup.
//
// A canonical name is the final name after following zero
// or more CNAME records.
// LookupCNAME does not return an error if host does not
// contain DNS "CNAME" records, as long as host resolves to
// address records.
//
// The returned canonical name is validated to be a properly
// formatted presentation-format domain name.
//
// LookupCNAME uses [context.Background] internally; to specify the context, use
// [Resolver.LookupCNAME].
public static (@string cname, error err) LookupCNAME(@string host) {
    return DefaultResolver.LookupCNAME(context.Background(), host);
}

// LookupCNAME returns the canonical name for the given host.
// Callers that do not care about the canonical name can call
// [LookupHost] or [LookupIP] directly; both take care of resolving
// the canonical name as part of the lookup.
//
// A canonical name is the final name after following zero
// or more CNAME records.
// LookupCNAME does not return an error if host does not
// contain DNS "CNAME" records, as long as host resolves to
// address records.
//
// The returned canonical name is validated to be a properly
// formatted presentation-format domain name.
public static (@string, error) LookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host) {
    var (cname, err) = Ꮡr.lookupCNAME(ctx, host);
    if (err != default!) {
        return ("", err);
    }
    if (!isDomainName(cname)) {
        return ("", new DNSErrorжerror(Ꮡ(new DNSError(Err: errMalformedDNSRecordsDetail, Name: host))));
    }
    return (cname, default!);
}

// LookupSRV tries to resolve an [SRV] query of the given service,
// protocol, and domain name. The proto is "tcp" or "udp".
// The returned records are sorted by priority and randomized
// by weight within a priority.
//
// LookupSRV constructs the DNS name to look up following RFC 2782.
// That is, it looks up _service._proto.name. To accommodate services
// publishing SRV records under non-standard names, if both service
// and proto are empty strings, LookupSRV looks up name directly.
//
// The returned service names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
public static (@string cname, slice<ж<SRV>> addrs, error err) LookupSRV(@string service, @string proto, @string name) {
    return DefaultResolver.LookupSRV(context.Background(), service, proto, name);
}

// LookupSRV tries to resolve an [SRV] query of the given service,
// protocol, and domain name. The proto is "tcp" or "udp".
// The returned records are sorted by priority and randomized
// by weight within a priority.
//
// LookupSRV constructs the DNS name to look up following RFC 2782.
// That is, it looks up _service._proto.name. To accommodate services
// publishing SRV records under non-standard names, if both service
// and proto are empty strings, LookupSRV looks up name directly.
//
// The returned service names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
public static (@string, slice<ж<SRV>>, error) LookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) {
    var (cname, addrs, err) = Ꮡr.lookupSRV(ctx, service, proto, name);
    if (err != default!) {
        return ("", default!, err);
    }
    if (cname != ""u8 && !isDomainName(cname)) {
        return ("", default!, new DNSErrorжerror(Ꮡ(new DNSError(Err: "SRV header name is invalid"u8, Name: name))));
    }
    var filteredAddrs = new slice<ж<SRV>>(0, len(addrs));
    foreach (var (_, addr) in addrs) {
        if (addr == nil) {
            continue;
        }
        if (!isDomainName((~addr).Target)) {
            continue;
        }
        filteredAddrs = append(filteredAddrs, addr);
    }
    if (len(addrs) != len(filteredAddrs)) {
        return (cname, filteredAddrs, new DNSErrorжerror(Ꮡ(new DNSError(Err: errMalformedDNSRecordsDetail, Name: name))));
    }
    return (cname, filteredAddrs, default!);
}

// LookupMX returns the DNS MX records for the given domain name sorted by preference.
//
// The returned mail server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
//
// LookupMX uses [context.Background] internally; to specify the context, use
// [Resolver.LookupMX].
public static (slice<ж<MX>>, error) LookupMX(@string name) {
    return DefaultResolver.LookupMX(context.Background(), name);
}

// LookupMX returns the DNS MX records for the given domain name sorted by preference.
//
// The returned mail server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
public static (slice<ж<MX>>, error) LookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    var (records, err) = Ꮡr.lookupMX(ctx, name);
    if (err != default!) {
        return (default!, err);
    }
    var filteredMX = new slice<ж<MX>>(0, len(records));
    foreach (var (_, mx) in records) {
        if (mx == nil) {
            continue;
        }
        if (!isDomainName((~mx).Host)) {
            continue;
        }
        filteredMX = append(filteredMX, mx);
    }
    if (len(records) != len(filteredMX)) {
        return (filteredMX, new DNSErrorжerror(Ꮡ(new DNSError(Err: errMalformedDNSRecordsDetail, Name: name))));
    }
    return (filteredMX, default!);
}

// LookupNS returns the DNS NS records for the given domain name.
//
// The returned name server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
//
// LookupNS uses [context.Background] internally; to specify the context, use
// [Resolver.LookupNS].
public static (slice<ж<NS>>, error) LookupNS(@string name) {
    return DefaultResolver.LookupNS(context.Background(), name);
}

// LookupNS returns the DNS NS records for the given domain name.
//
// The returned name server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the remaining results, if any.
public static (slice<ж<NS>>, error) LookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    var (records, err) = Ꮡr.lookupNS(ctx, name);
    if (err != default!) {
        return (default!, err);
    }
    var filteredNS = new slice<ж<NS>>(0, len(records));
    foreach (var (_, ns) in records) {
        if (ns == nil) {
            continue;
        }
        if (!isDomainName((~ns).Host)) {
            continue;
        }
        filteredNS = append(filteredNS, ns);
    }
    if (len(records) != len(filteredNS)) {
        return (filteredNS, new DNSErrorжerror(Ꮡ(new DNSError(Err: errMalformedDNSRecordsDetail, Name: name))));
    }
    return (filteredNS, default!);
}

// LookupTXT returns the DNS TXT records for the given domain name.
//
// If a DNS TXT record holds multiple strings, they are concatenated as a
// single string.
//
// LookupTXT uses [context.Background] internally; to specify the context, use
// [Resolver.LookupTXT].
public static (slice<@string>, error) LookupTXT(@string name) {
    return DefaultResolver.lookupTXT(context.Background(), name);
}

// LookupTXT returns the DNS TXT records for the given domain name.
//
// If a DNS TXT record holds multiple strings, they are concatenated as a
// single string.
public static (slice<@string>, error) LookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    return Ꮡr.lookupTXT(ctx, name);
}

// LookupAddr performs a reverse lookup for the given address, returning a list
// of names mapping to that address.
//
// The returned names are validated to be properly formatted presentation-format
// domain names. If the response contains invalid names, those records are filtered
// out and an error will be returned alongside the remaining results, if any.
//
// When using the host C library resolver, at most one result will be
// returned. To bypass the host resolver, use a custom [Resolver].
//
// LookupAddr uses [context.Background] internally; to specify the context, use
// [Resolver.LookupAddr].
public static (slice<@string> names, error err) LookupAddr(@string addr) {
    return DefaultResolver.LookupAddr(context.Background(), addr);
}

// LookupAddr performs a reverse lookup for the given address, returning a list
// of names mapping to that address.
//
// The returned names are validated to be properly formatted presentation-format
// domain names. If the response contains invalid names, those records are filtered
// out and an error will be returned alongside the remaining results, if any.
public static (slice<@string>, error) LookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) {
    var (names, err) = Ꮡr.lookupAddr(ctx, addr);
    if (err != default!) {
        return (default!, err);
    }
    var filteredNames = new slice<@string>(0, len(names));
    foreach (var (_, name) in names) {
        if (isDomainName(name)) {
            filteredNames = append(filteredNames, name);
        }
    }
    if (len(names) != len(filteredNames)) {
        return (filteredNames, new DNSErrorжerror(Ꮡ(new DNSError(Err: errMalformedDNSRecordsDetail, Name: addr))));
    }
    return (filteredNames, default!);
}

// errMalformedDNSRecordsDetail is the DNSError detail which is returned when a Resolver.Lookup...
// method receives DNS records which contain invalid DNS names. This may be returned alongside
// results which have had the malformed records filtered out.
internal static @string errMalformedDNSRecordsDetail = "DNS response contained records which contain invalid names"u8;

// dial makes a new connection to the provided server (which must be
// an IP address) with the provided network type, using either r.Dial
// (if both r and r.Dial are non-nil) or else Dialer.DialContext.
internal static (Conn, error) dial(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string server) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Calling Dial here is scary -- we have to be sure not to
    // dial a name that will require a DNS lookup, or Dial will
    // call back here to translate it. The DNS config parser has
    // already checked that all the cfg.servers are IP
    // addresses, which Dial will use without a DNS lookup.
    Conn c = default!;
    error err = default!;
    if (Ꮡr != nil && r.Dial != default!){
        (c, err) = r.Dial(ctx, network, server);
    } else {
        ref var d = ref heap(new Dialer(), out var Ꮡd);
        (c, err) = Ꮡd.DialContext(ctx, network, server);
    }
    if (err != default!) {
        return (default!, mapErr(err));
    }
    return (c, default!);
}

// goLookupSRV returns the SRV records for a target name, built either
// from its component service ("sip"), protocol ("tcp"), and name
// ("example.com."), or from name directly (if service and proto are
// both empty).
//
// In either case, the returned target name ("_sip._tcp.example.com.")
// is also returned on success.
//
// The records are sorted by weight.
internal static (@string target, slice<ж<SRV>> srvs, error err) goLookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) {
    @string target = default!;
    slice<ж<SRV>> srvs = default!;
    error err = default!;

    if (service == ""u8 && proto == ""u8){
        target = name;
    } else {
        target = "_"u8 + service + "._"u8 + proto + "."u8 + name;
    }
    ref var server = ref heap<@string>(out var Ꮡserver);
    (var p, server, err) = Ꮡr.lookup(ctx, target, dnsmessage.TypeSRV, nil);
    if (err != default!) {
        return ("", default!, err);
    }
    dnsmessage.Name cname = new();
    while (ᐧ) {
        var (h, errΔ1) = p.AnswerHeader();
        if (AreEqual(errΔ1, dnsmessage.ErrSectionDone)) {
            break;
        }
        if (errΔ1 != default!) {
            return ("", default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        if (h.Type != dnsmessage.TypeSRV) {
            {
                var errΔ2 = p.SkipAnswer(); if (errΔ2 != default!) {
                    return ("", default!, new DNSErrorжerror(Ꮡ(new DNSError(
                        Err: "cannot unmarshal DNS message"u8,
                        Name: name,
                        Server: server
                    ))));
                }
            }
            continue;
        }
        if (cname.Length == 0 && h.Name.Length != 0) {
            cname = h.Name.ΔClone();
        }
        (var srv, errΔ1) = p.SRVResource();
        if (errΔ1 != default!) {
            return ("", default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        srvs = append(srvs, Ꮡ(new SRV(Target: srv.Target.String(), Port: srv.Port, Priority: srv.Priority, Weight: srv.Weight)));
    }
    ((byPriorityWeight)srvs).sort();
    return (cname.String(), srvs, default!);
}

// goLookupMX returns the MX records for name.
internal static (slice<ж<MX>>, error) goLookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    ref var server = ref heap<@string>(out var Ꮡserver);
    (var p, server, var err) = Ꮡr.lookup(ctx, name, dnsmessage.TypeMX, nil);
    if (err != default!) {
        return (default!, err);
    }
    slice<ж<MX>> mxs = default!;
    while (ᐧ) {
        var (h, errΔ1) = p.AnswerHeader();
        if (AreEqual(errΔ1, dnsmessage.ErrSectionDone)) {
            break;
        }
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        if (h.Type != dnsmessage.TypeMX) {
            {
                var errΔ2 = p.SkipAnswer(); if (errΔ2 != default!) {
                    return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                        Err: "cannot unmarshal DNS message"u8,
                        Name: name,
                        Server: server
                    ))));
                }
            }
            continue;
        }
        (var mx, errΔ1) = p.MXResource();
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        mxs = append(mxs, Ꮡ(new MX(Host: mx.MX.String(), Pref: mx.Pref)));
    }
    ((byPref)mxs).sort();
    return (mxs, default!);
}

// goLookupNS returns the NS records for name.
internal static (slice<ж<NS>>, error) goLookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    ref var server = ref heap<@string>(out var Ꮡserver);
    (var p, server, var err) = Ꮡr.lookup(ctx, name, dnsmessage.TypeNS, nil);
    if (err != default!) {
        return (default!, err);
    }
    slice<ж<NS>> nss = default!;
    while (ᐧ) {
        var (h, errΔ1) = p.AnswerHeader();
        if (AreEqual(errΔ1, dnsmessage.ErrSectionDone)) {
            break;
        }
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        if (h.Type != dnsmessage.TypeNS) {
            {
                var errΔ2 = p.SkipAnswer(); if (errΔ2 != default!) {
                    return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                        Err: "cannot unmarshal DNS message"u8,
                        Name: name,
                        Server: server
                    ))));
                }
            }
            continue;
        }
        (var ns, errΔ1) = p.NSResource();
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        nss = append(nss, Ꮡ(new NS(Host: ns.NS.String())));
    }
    return (nss, default!);
}

// goLookupTXT returns the TXT records from name.
internal static (slice<@string>, error) goLookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    ref var server = ref heap<@string>(out var Ꮡserver);
    (var p, server, var err) = Ꮡr.lookup(ctx, name, dnsmessage.TypeTXT, nil);
    if (err != default!) {
        return (default!, err);
    }
    slice<@string> txts = default!;
    while (ᐧ) {
        var (h, errΔ1) = p.AnswerHeader();
        if (AreEqual(errΔ1, dnsmessage.ErrSectionDone)) {
            break;
        }
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        if (h.Type != dnsmessage.TypeTXT) {
            {
                var errΔ2 = p.SkipAnswer(); if (errΔ2 != default!) {
                    return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                        Err: "cannot unmarshal DNS message"u8,
                        Name: name,
                        Server: server
                    ))));
                }
            }
            continue;
        }
        (var txt, errΔ1) = p.TXTResource();
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: "cannot unmarshal DNS message"u8,
                Name: name,
                Server: server
            ))));
        }
        // Multiple strings in one TXT record need to be
        // concatenated without separator to be consistent
        // with previous Go resolver.
        nint n = 0;
        foreach (var (_, s) in txt.TXT) {
            n += len(s);
        }
        var txtJoin = new slice<byte>(0, n);
        foreach (var (_, s) in txt.TXT) {
            txtJoin = append(txtJoin, s.ꓸꓸꓸ);
        }
        if (len(txts) == 0) {
            txts = new slice<@string>(0, 1);
        }
        txts = append(txts, ((@string)txtJoin));
    }
    return (txts, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noCnameRecordReceivedˢ = "no CNAME record received"u8;
internal static readonly @string couldNotParseCnameRecordˢ = "could not parse CNAME record"u8;

internal static (@string, error) parseCNAMEFromResources(slice<dnsmessage.Resource> resources) {
    if (len(resources) == 0) {
        return ("", errors.New(noCnameRecordReceivedˢ));
    }
    var (c, ok) = resources[0].Body._<ж<dnsmessageꓸCNAMEResource>>(ᐧ);
    if (!ok) {
        return ("", errors.New(couldNotParseCnameRecordˢ));
    }
    return ((~c).CNAME.String(), default!);
}

} // end net_package
