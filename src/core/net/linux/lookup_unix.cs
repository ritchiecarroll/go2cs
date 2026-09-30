// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1
namespace go;

using context = context_package;
using bytealg = @internal.bytealg_package;
using Δsync = sync_package;
using @internal;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;

partial class net_package {

internal static ж<Δsync.Once> ᏑonceReadProtocols = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once onceReadProtocols => ref ᏑonceReadProtocols.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string etcProtocolsˢ = "/etc/protocols"u8;

// readProtocols loads contents of /etc/protocols into protocols map
// for quick access.
internal static void readProtocols() {
    GoFrame ᒐ = default;
    try {
        var (Δfile, err) = open(etcProtocolsˢ);
        if (err != default!) {
            return;
        }
        var fileʗ1 = Δfile;
        defer(fileʗ1.close, ref ᒐ);
        for (var (line, ok) = Δfile.readLine(); ok; (line, ok) = Δfile.readLine()) {
            // tcp    6   TCP    # transmission control protocol
            {
                nint i = bytealg.IndexByteString(line, (rune)'#'); if (i >= 0) {
                    line = line.slice(0, i);
                }
            }
            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }
            {
                var (proto, _, okΔ1) = dtoi(f[1]); if (okΔ1) {
                    {
                        var (_, okΔ2) = protocols[f[0], ꟷ]; if (!okΔ2) {
                            protocols[f[0]] = proto;
                        }
                    }
                    foreach (var (_, alias) in f[2..]) {
                        {
                            var (_, okΔ3) = protocols[alias, ꟷ]; if (!okΔ3) {
                                protocols[alias] = proto;
                            }
                        }
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// lookupProtocol looks up IP protocol name in /etc/protocols and
// returns correspondent protocol number.
internal static (nint, error) lookupProtocol(context.Context _, @string name) {
    ᏑonceReadProtocols.Do(readProtocols);
    return lookupProtocolMap(name);
}

internal static (slice<@string> addrs, error err) lookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string host) {
    var (order, conf) = systemConf().hostLookupOrder(Ꮡr, host);
    if (order == hostLookupCgo) {
        return cgoLookupHost(ctx, host);
    }
    return Ꮡr.goLookupHostOrder(ctx, host, order, conf);
}

internal static (slice<IPAddr> addrs, error err) lookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host) {
    error err = default!;

    var (order, conf) = systemConf().hostLookupOrder(Ꮡr, host);
    if (order == hostLookupCgo) {
        return cgoLookupIP(ctx, network, host);
    }
    (var ips, _, err) = Ꮡr.goLookupIPCNAMEOrder(ctx, network, host, order, conf);
    return (ips, err);
}

internal static (nint, error) lookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) {
    // Port lookup is not a DNS operation.
    // Prefer the cgo resolver if possible.
    if (!systemConf().mustUseGoResolver(Ꮡr)) {
        var (port, err) = cgoLookupPort(ctx, network, service);
        if (err != default!) {
            // Issue 18213: if cgo fails, first check to see whether we
            // have the answer baked-in to the net package.
            {
                var (portΔ1, errΔ1) = goLookupPort(network, service); if (errΔ1 == default!) {
                    return (portΔ1, default!);
                }
            }
        }
        return (port, err);
    }
    return goLookupPort(network, service);
}

internal static (@string, error) lookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    var (order, conf) = systemConf().hostLookupOrder(Ꮡr, name);
    if (order == hostLookupCgo) {
        {
            var (cname, err, ok) = cgoLookupCNAME(ctx, name); if (ok) {
                return (cname, err);
            }
        }
    }
    return Ꮡr.goLookupCNAME(ctx, name, order, conf);
}

internal static (@string, slice<ж<SRV>>, error) lookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) {
    return Ꮡr.goLookupSRV(ctx, service, proto, name);
}

internal static (slice<ж<MX>>, error) lookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    return Ꮡr.goLookupMX(ctx, name);
}

internal static (slice<ж<NS>>, error) lookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    return Ꮡr.goLookupNS(ctx, name);
}

internal static (slice<@string>, error) lookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    return Ꮡr.goLookupTXT(ctx, name);
}

internal static (slice<@string>, error) lookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) {
    var (order, conf) = systemConf().addrLookupOrder(Ꮡr, addr);
    if (order == hostLookupCgo) {
        return cgoLookupPTR(ctx, addr);
    }
    return Ꮡr.goLookupPTR(ctx, addr, order, conf);
}

} // end net_package
