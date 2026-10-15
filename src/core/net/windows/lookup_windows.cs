// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// HAND-OWNED COMPANION to syscall/windows/zsyscall_windows_dnsrecord_impl.cs. Only the SIX record
// payload reads differ from the auto-conversion; everything else is the converter's own emission.
//
// GO 1.26 (H6 audit row 117): the 1.25 changes to lookup_windows.go -- lookupProtocol's buffered
// channel, newDNSError at every acquireThread and timeout site, UTF16PtrFromString in place of the
// panicking StringToUTF16Ptr -- were merged in from the go1.26.9 emission (a three-way merge over the
// go1.24.13 .auto); hunks the release did not touch keep this file's older emission spelling.
//
// Go reads each DNS record's union buffer by reinterpreting it -- `(*syscall.DNSSRVData)(
// unsafe.Pointer(&r.Data[0]))` -- and the faithful conversion of that is
//
//     p.at(syscall.DNSRecord.ᏑData, 0).Reinterpret<byte, syscall.DNSSRVData>()
//
// which CANNOT be made safe here, at any layer. Every payload carries a MANAGED REFERENCE
// (`DNSSRVData.Target`, `DNSMXData.NameExchange`, `DNSPTRData.Host`, `DNSTXTData.StringArray`), and
// golib's `ReinterpretAliasesStorage<byte, DNSSRVData>` refuses the safe managed-alias route on the
// SIZE test alone -- `Unsafe.SizeOf<DNSSRVData>()` is 16, `Unsafe.SizeOf<byte>()` is 1 -- so the
// call falls through to an `Unsafe.AsRef<DNSSRVData>` over raw bytes that loads eight of them AS AN
// OBJECT REFERENCE. That is a CLR type-safety break (an access violation, or silent heap corruption
// on a write), and it happens whether the record is native or managed. Fixing only the syscall
// wrapper would therefore have turned a contained wrong answer -- "no record" for MX/NS/TXT/SRV/PTR
// -- into memory corruption, which is why this companion exists at all.
//
// The six reads take `syscall.DnsRecordPayload<T>` instead: a REAL managed box the DnsQuery
// hand-own transcribed and anchored, whose pointer fields are genuine boxes over copied names. Every
// downstream read ((~v).Target, .Port, .NameExchange, .Host, .StringArray, .StringCount) is
// unchanged, because the accessor hands back the same shape the reinterpret was reaching for.
//
// RETIREMENT CONDITION: this file and its syscall-side partner retire together when the
// pointer-bearing-union representation arc lands -- the named converter item that stops emitting a
// Go union buffer whose payloads carry pointers as a bare `array<byte>` -- and the auto-conversion
// of both files becomes sound. At that point this hand-own must be DROPPED, not merged forward: the
// converter's own emission becomes the correct one.

[module: go.GoManualConversion]

namespace go;

using context = context_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal.syscall;

partial class net_package {

// cgoAvailable set to true to indicate that the cgo resolver
// is available on Windows. Note that on Windows the cgo resolver
// does not actually use cgo.
internal const bool cgoAvailable = true;

internal static syscall.Errno _DNS_ERROR_RCODE_NAME_ERROR => /* syscall.Errno(9003) */ 9003;
internal static syscall.Errno _DNS_INFO_NO_RECORDS => /* syscall.Errno(9501) */ 9501;
internal static syscall.Errno _WSAHOST_NOT_FOUND => /* syscall.Errno(11001) */ 11001;
internal static syscall.Errno _WSATRY_AGAIN => /* syscall.Errno(11002) */ 11002;
internal static syscall.Errno _WSATYPE_NOT_FOUND => /* syscall.Errno(10109) */ 10109;

internal static error winError(@string call, error err) {
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, _WSAHOST_NOT_FOUND) || AreEqual(exprᴛ1, _DNS_ERROR_RCODE_NAME_ERROR) || AreEqual(exprᴛ1, _DNS_INFO_NO_RECORDS)) {
        return new notFoundErrorжerror(errNoSuchHost);
    }

    return os.NewSyscallError(call, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getprotobynameˢ = "getprotobyname"u8;

internal static (nint proto, error err) getprotobyname(@string name) {
    error err = default!;

    (var p, err) = syscall.GetProtoByName(name);
    if (err != default!) {
        return (0, winError(getprotobynameˢ, err));
    }
    return ((nint)(~p).Proto, default!);
}

// GetProtoByName return value is stored in thread local storage.
// Start new os thread before the call to prevent races.
[GoType("dyn")] internal partial struct lookupProtocol_result {
    internal nint proto;
    internal error err;
}

// lookupProtocol looks up IP protocol name and returns correspondent protocol number.
internal static (nint, error) lookupProtocol(context.Context ctx, @string name) {
    var ch = new channel<lookupProtocol_result>(1); // buffer so that next goroutine never blocks
    var chʗ1 = ch;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            {
                var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
                    chʗ1.ᐸꟷ(new lookupProtocol_result(err: mapErr(errΔ1)));
                    return;
                }
            }
            defer(releaseThread, ref ᒐ);
            Δruntime.LockOSThread();
            defer(Δruntime.UnlockOSThread, ref ᒐ);
            var (proto, err) = getprotobyname(name);
            chʗ1.ᐸꟷ(new lookupProtocol_result(proto: proto, err: err));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var selᴛ13 = ch;
    var selᴛ14 = ctx.Done();
    switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
    case 0 when selᴛ13.ꟷᐳ(out var r): {
        if (r.err != default!) {
            {
                var (proto, err) = lookupProtocolMap(name); if (err == default!) {
                    return (proto, default!);
                }
            }
            r.err = new DNSErrorжerror(newDNSError(r.err, name, ""u8));
        }
        return (r.proto, r.err);
    }
    case 1 when selᴛ14.ꟷᐳ(out _): {
        return (0, new DNSErrorжerror(newDNSError(mapErr(ctx.Err()), name, ""u8)));
    }}
    return default!;
}

internal static (slice<@string>, error) lookupHost(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    var (ips, err) = Ꮡr.lookupIP(ctx, "ip"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    var addrs = new slice<@string>(0, len(ips));
    foreach (var (_, vᴛ1) in ips) {
        ref var ip = ref heap(new IPAddr(), out var Ꮡip);
        ip = vᴛ1;

        addrs = append(addrs, Ꮡip.String());
    }
    return (addrs, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getaddrinfowˢ = "getaddrinfow"u8;

[GoType("dyn")] internal partial struct lookupIP_ret {
    internal slice<IPAddr> addrs;
    internal error err;
}

internal static (slice<IPAddr>, error) lookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string name) {
    {
        var (order, conf) = systemConf().hostLookupOrder(Ꮡr, name); if (order != hostLookupCgo) {
            return Ꮡr.goLookupIP(ctx, network, name, order, conf);
        }
    }
    // TODO(bradfitz,brainman): use ctx more. See TODO below.
    int32 family = syscall.AF_UNSPEC;
    switch (ipVersion(network)) {
    case (rune)'4': {
        family = syscall.AF_INET;
        break;
    }
    case (rune)'6': {
        family = syscall.AF_INET6;
        break;
    }}

    (slice<IPAddr>, error) getaddr() {
        GoFrame ᒐ = default;
        try {
            {
                var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
                    return (default!, new DNSErrorжerror(newDNSError(mapErr(errΔ1), name, ""u8)));
                }
            }
            defer(releaseThread, ref ᒐ);
            ref var hints = ref heap<syscall.AddrinfoW>(out var Ꮡhints);
            hints = new syscall.AddrinfoW(
                Family: family,
                Socktype: syscall.SOCK_STREAM,
                Protocol: syscall.IPPROTO_IP
            );
            ref var result = ref heap<ж<syscall.AddrinfoW>>(out var Ꮡresult);
            var (name16p, err) = syscall.UTF16PtrFromString(name);
            if (err != default!) {
                return (default!, new DNSErrorжerror(newDNSError(err, name, ""u8)));
            }
            var dnsConf = getSystemDNSConfig();
            var start = time.Now();
            error e = default!;
            for (nint i = 0; i < (~dnsConf).attempts; i++) {
                e = syscall.GetAddrInfoW(name16p, nil, Ꮡhints, Ꮡresult);
                if (e == default! || !AreEqual(e, _WSATRY_AGAIN) || time.Since(start) > (~dnsConf).timeout) {
                    break;
                }
            }
            if (e != default!) {
                return (default!, new DNSErrorжerror(newDNSError(winError(getaddrinfowˢ, e), name, ""u8)));
            }
            defer(syscall.FreeAddrInfoW, result, ref ᒐ);
            var addrs = new slice<IPAddr>(0, 5);
            for (; result != nil; result = result.Value.Next) {
                @unsafe.Pointer addr = ((@unsafe.Pointer)(uintptr)((~result).Addr));
                var exprᴛ1 = (~result).Family;
                if (exprᴛ1 == syscall.AF_INET) {
                    var a = ((ж<syscall.RawSockaddrInet4>)(uintptr)(addr)).Value.Addr.Clone();
                    addrs = append(addrs, new IPAddr(IP: copyIP(a[..])));
                }
                else if (exprᴛ1 == syscall.AF_INET6) {
                    var a = ((ж<syscall.RawSockaddrInet6>)(uintptr)(addr)).Value.Addr.Clone();
                    @string zone = zoneCache.name((nint)((ж<syscall.RawSockaddrInet6>)(uintptr)(addr)).Value.Scope_id);
                    addrs = append(addrs, new IPAddr(IP: copyIP(a[..]), Zone: zone));
                }
                else { /* default: */
                    return (default!, new DNSErrorжerror(newDNSError(syscall.EWINDOWS, name, ""u8)));
                }

            }
            return (addrs, default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    channel<lookupIP_ret> ch = default!;
    if (ctx.Err() == default!) {
        ch = new channel<lookupIP_ret>(1);
        var chʗ1 = ch;
        var getaddrʗ1 = getaddr;
        goǃ(() => {
            var (addr, err) = getaddrʗ1();
            chʗ1.ᐸꟷ(new lookupIP_ret(addrs: addr, err: err));
        });
    }

    var selᴛ15 = ch;
    var selᴛ16 = ctx.Done();
    switch (select(ᐸꟷ(selᴛ15, ꓸꓸꓸ), ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
    case 0 when selᴛ15.ꟷᐳ(out var rΔ1): {
        return (rΔ1.addrs, rΔ1.err);
    }
    case 1 when selᴛ16.ꟷᐳ(out _): {
        // TODO(bradfitz,brainman): cancel the ongoing
        // GetAddrInfoW? It would require conditionally using
        // GetAddrInfoEx with lpOverlapped, which requires
        // Windows 8 or newer. I guess we'll need oldLookupIP,
        // newLookupIP, and newerLookUP.
        //
        // For now we just let it finish and write to the
        // buffered channel.
        return (default!, new DNSErrorжerror(newDNSError(mapErr(ctx.Err()), name, ""u8)));
    }}
    return default!;
}

internal static (nint, error) lookupPort(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string service) {
    GoFrame ᒐ = default;
    try {
        if (systemConf().mustUseGoResolver(Ꮡr)) {
            return lookupPortMap(network, service);
        }
        // TODO(bradfitz): finish ctx plumbing
        {
            var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
                return (0, new DNSErrorжerror(newDNSError(mapErr(errΔ1), network + "/"u8 + service, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        ref var hints = ref heap(new syscall.AddrinfoW(), out var Ꮡhints);
        var exprᴛ1 = network;
        if (exprᴛ1 == "ip"u8) {
        }
        else if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
            hints.Socktype = syscall.SOCK_STREAM;
            hints.Protocol = syscall.IPPROTO_TCP;
        }
        else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
            hints.Socktype = syscall.SOCK_DGRAM;
            hints.Protocol = syscall.IPPROTO_UDP;
        }
        else { /* default: */
            return (0, new DNSErrorжerror(Ꮡ(new DNSError( // no hints
Err: "unknown network"u8, Name: network + "/"u8 + service))));
        }

        switch (ipVersion(network)) {
        case (rune)'4': {
            hints.Family = syscall.AF_INET;
            break;
        }
        case (rune)'6': {
            hints.Family = syscall.AF_INET6;
            break;
        }}

        var (servicep, err) = syscall.UTF16PtrFromString(service);
        if (err != default!) {
            return (0, new DNSErrorжerror(newDNSError(err, network + "/"u8 + service, ""u8)));
        }

        ref var result = ref heap<ж<syscall.AddrinfoW>>(out var Ꮡresult);
        var e = syscall.GetAddrInfoW(nil, servicep, Ꮡhints, Ꮡresult);
        if (e != default!) {
            {
                var (port, errΔ2) = lookupPortMap(network, service); if (errΔ2 == default!) {
                    return (port, default!);
                }
            }
            // The _WSATYPE_NOT_FOUND error is returned by GetAddrInfoW
            // when the service name is unknown. We are also checking
            // for _WSAHOST_NOT_FOUND here to match the cgo (unix) version
            // cgo_unix.go (cgoLookupServicePort).
            if (AreEqual(e, _WSATYPE_NOT_FOUND) || AreEqual(e, _WSAHOST_NOT_FOUND)) {
                return (0, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errUnknownPort), network + "/"u8 + service, ""u8)));
            }
            return (0, new DNSErrorжerror(newDNSError(winError(getaddrinfowˢ, e), network + "/"u8 + service, ""u8)));
        }
        defer(syscall.FreeAddrInfoW, result, ref ᒐ);
        if (result == nil) {
            return (0, new DNSErrorжerror(newDNSError(syscall.EINVAL, network + "/"u8 + service, ""u8)));
        }
        @unsafe.Pointer addr = ((@unsafe.Pointer)(uintptr)((~result).Addr));
        var exprᴛ2 = (~result).Family;
        if (exprᴛ2 == syscall.AF_INET) {
            var a = (ж<syscall.RawSockaddrInet4>)(uintptr)(addr);
            return ((nint)syscall.Ntohs((~a).Port), default!);
        }
        if (exprᴛ2 == syscall.AF_INET6) {
            var a = (ж<syscall.RawSockaddrInet6>)(uintptr)(addr);
            return ((nint)syscall.Ntohs((~a).Port), default!);
        }

        return (0, new DNSErrorжerror(newDNSError(syscall.EINVAL, network + "/"u8 + service, ""u8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dnsqueryˢ = "dnsquery"u8;

internal static (@string, error) lookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    GoFrame ᒐ = default;
    try {
        {
            var (order, conf) = systemConf().hostLookupOrder(Ꮡr, name); if (order != hostLookupCgo) {
                return Ꮡr.goLookupCNAME(ctx, name, order, conf);
            }
        }
        // TODO(bradfitz): finish ctx plumbing
        {
            var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
                return ("", new DNSErrorжerror(newDNSError(mapErr(errΔ1), name, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(name, syscall.DNS_TYPE_CNAME, 0, nil, Ꮡrec, nil);
        // windows returns DNS_INFO_NO_RECORDS if there are no CNAME-s
        {
            var (errno, ok) = e._<syscall.Errno>(ᐧ); if (ok && errno == syscall.DNS_INFO_NO_RECORDS) {
                // if there are no aliases, the canonical name is the input name
                return (absDomainName(name), default!);
            }
        }
        if (e != default!) {
            return ("", new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), name, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);

        var (namep, err) = syscall.UTF16PtrFromString(name);
        if (err != default!) {
            return ("", new DNSErrorжerror(newDNSError(err, name, ""u8)));
        }

        var resolved = resolveCNAME(namep, rec);
        @string cname = windows.UTF16PtrToString(resolved);
        return (absDomainName(cname), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (@string, slice<ж<SRV>>, error) lookupSRV(this ж<Resolver> Ꮡr, context.Context ctx, @string service, @string proto, @string name) {
    GoFrame ᒐ = default;
    try {
        if (systemConf().mustUseGoResolver(Ꮡr)) {
            return Ꮡr.goLookupSRV(ctx, service, proto, name);
        }
        // TODO(bradfitz): finish ctx plumbing
        {
            var err = acquireThread(ctx); if (err != default!) {
                return ("", default!, new DNSErrorжerror(newDNSError(mapErr(err), name, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        @string target = default!;
        if (service == ""u8 && proto == ""u8){
            target = name;
        } else {
            target = "_"u8 + service + "._"u8 + proto + "."u8 + name;
        }
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(target, syscall.DNS_TYPE_SRV, 0, nil, Ꮡrec, nil);
        if (e != default!) {
            return ("", default!, new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), name, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);
        var srvs = new slice<ж<SRV>>(0, 10);
        foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_SRV, target)) {
            var v = syscall.DnsRecordPayload<syscall.DNSSRVData>(p);
            srvs = append(srvs, Ꮡ(new SRV(absDomainName(syscall.UTF16ToString((~array<uint16>.AliasPointer((~v).Target, 256))[..])), (~v).Port, (~v).Priority, (~v).Weight)));
        }
        ((byPriorityWeight)srvs).sort();
        return (absDomainName(target), srvs, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<ж<MX>>, error) lookupMX(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    GoFrame ᒐ = default;
    try {
        if (systemConf().mustUseGoResolver(Ꮡr)) {
            return Ꮡr.goLookupMX(ctx, name);
        }
        // TODO(bradfitz): finish ctx plumbing.
        {
            var err = acquireThread(ctx); if (err != default!) {
                return (default!, new DNSErrorжerror(newDNSError(mapErr(err), name, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(name, syscall.DNS_TYPE_MX, 0, nil, Ꮡrec, nil);
        if (e != default!) {
            return (default!, new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), name, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);
        var mxs = new slice<ж<MX>>(0, 10);
        foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_MX, name)) {
            var v = syscall.DnsRecordPayload<syscall.DNSMXData>(p);
            mxs = append(mxs, Ꮡ(new MX(absDomainName(windows.UTF16PtrToString((~v).NameExchange)), (~v).Preference)));
        }
        ((byPref)mxs).sort();
        return (mxs, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<ж<NS>>, error) lookupNS(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    GoFrame ᒐ = default;
    try {
        if (systemConf().mustUseGoResolver(Ꮡr)) {
            return Ꮡr.goLookupNS(ctx, name);
        }
        // TODO(bradfitz): finish ctx plumbing.
        {
            var err = acquireThread(ctx); if (err != default!) {
                return (default!, new DNSErrorжerror(newDNSError(mapErr(err), name, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(name, syscall.DNS_TYPE_NS, 0, nil, Ꮡrec, nil);
        if (e != default!) {
            return (default!, new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), name, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);
        var nss = new slice<ж<NS>>(0, 10);
        foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_NS, name)) {
            var v = syscall.DnsRecordPayload<syscall.DNSPTRData>(p);
            nss = append(nss, Ꮡ(new NS(absDomainName(syscall.UTF16ToString((~array<uint16>.AliasPointer((~v).Host, 256))[..])))));
        }
        return (nss, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<@string>, error) lookupTXT(this ж<Resolver> Ꮡr, context.Context ctx, @string name) {
    GoFrame ᒐ = default;
    try {
        if (systemConf().mustUseGoResolver(Ꮡr)) {
            return Ꮡr.goLookupTXT(ctx, name);
        }
        // TODO(bradfitz): finish ctx plumbing.
        {
            var err = acquireThread(ctx); if (err != default!) {
                return (default!, new DNSErrorжerror(newDNSError(mapErr(err), name, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(name, syscall.DNS_TYPE_TEXT, 0, nil, Ꮡrec, nil);
        if (e != default!) {
            return (default!, new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), name, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);
        var txts = new slice<@string>(0, 10);
        foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_TEXT, name)) {
            var d = syscall.DnsRecordPayload<syscall.DNSTXTData>(p);
            @string s = ""u8;

            // Go walks the union's variable-length `pStringArray[1]` by aliasing a huge window from
            // &StringArray[0] and re-slicing to StringCount. That idiom cannot survive the managed
            // payload and must NOT be restored: `Ꮡ(expr)` on a dereferenced value boxes a COPY, so
            // `array<T>.AliasPointer` finds no element storage (`TryGetElementStorage` fails), takes
            // its documented raw-address fork, and yields a window of capacity ZERO -- which panics
            // as `slice bounds out of range [::1] with capacity 0` the moment StringCount is 1. The
            // transcription already allocated exactly StringCount elements, so the count IS the
            // length here and a direct index is both simpler and the only correct form.
            for (nint i = 0; i < (~d).StringCount; i++) {
                s += windows.UTF16PtrToString((~d).StringArray[i]);
            }
            txts = append(txts, s);
        }
        return (txts, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<@string>, error) lookupAddr(this ж<Resolver> Ꮡr, context.Context ctx, @string addr) {
    GoFrame ᒐ = default;
    try {
        {
            var (order, conf) = systemConf().addrLookupOrder(Ꮡr, addr); if (order != hostLookupCgo) {
                return Ꮡr.goLookupPTR(ctx, addr, order, conf);
            }
        }
        // TODO(bradfitz): finish ctx plumbing.
        {
            var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
                return (default!, new DNSErrorжerror(newDNSError(mapErr(errΔ1), addr, ""u8)));
            }
        }
        defer(releaseThread, ref ᒐ);
        var (arpa, err) = reverseaddr(addr);
        if (err != default!) {
            return (default!, err);
        }
        ref var rec = ref heap<ж<syscall.DNSRecord>>(out var Ꮡrec);
        var e = syscall.DnsQuery(arpa, syscall.DNS_TYPE_PTR, 0, nil, Ꮡrec, nil);
        if (e != default!) {
            return (default!, new DNSErrorжerror(newDNSError(winError(dnsqueryˢ, e), addr, ""u8)));
        }
        defer(syscall.DnsRecordListFree, rec, (uint32)(1), ref ᒐ);
        var ptrs = new slice<@string>(0, 10);
        foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_PTR, arpa)) {
            var v = syscall.DnsRecordPayload<syscall.DNSPTRData>(p);
            ptrs = append(ptrs, absDomainName(windows.UTF16PtrToString((~v).Host)));
        }
        return (ptrs, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static UntypedInt dnsSectionMask => 0x0003;

// returns only results applicable to name and resolves CNAME entries.
internal static slice<ж<syscall.DNSRecord>> validRecs(ж<syscall.DNSRecord> Ꮡr, uint16 dnstype, @string name) {
    var (cname, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return default!;
    }
    if (dnstype != syscall.DNS_TYPE_CNAME) {
        cname = resolveCNAME(cname, Ꮡr);
    }
    var rec = new slice<ж<syscall.DNSRecord>>(0, 10);
    for (var p = Ꮡr; p != nil; p = p.Value.Next) {
        // in case of a local machine, DNS records are returned with DNSREC_QUESTION flag instead of DNS_ANSWER
        if ((uint32)((~p).Dw & (uint32)dnsSectionMask) != syscall.DnsSectionAnswer && (uint32)((~p).Dw & (uint32)dnsSectionMask) != syscall.DnsSectionQuestion) {
            continue;
        }
        if ((~p).Type != dnstype) {
            continue;
        }
        if (!syscall.DnsNameCompare(cname, (~p).Name)) {
            continue;
        }
        rec = append(rec, p);
    }
    return rec;
}

// returns the last CNAME in chain.
internal static ж<uint16> resolveCNAME(ж<uint16> Ꮡname, ж<syscall.DNSRecord> Ꮡr) {
    ref var name = ref Ꮡname.DerefOrNull();

    // limit cname resolving to 10 in case of an infinite CNAME loop
Cname:
    for (nint cnameloop = 0; cnameloop < 10; cnameloop++) {
        for (var p = Ꮡr; p != nil; p = p.Value.Next) {
            if ((uint32)((~p).Dw & (uint32)dnsSectionMask) != syscall.DnsSectionAnswer) {
                continue;
            }
            if ((~p).Type != syscall.DNS_TYPE_CNAME) {
                continue;
            }
            if (!syscall.DnsNameCompare(Ꮡname, (~p).Name)) {
                continue;
            }
            Ꮡname = (syscall.DnsRecordPayload<syscall.DNSPTRData>(Ꮡr)).Value.Host; name = ref Ꮡname.DerefOrNull();
            goto continue_Cname;
        }
        break;
continue_Cname:;
    }
break_Cname:;
    return Ꮡname;
}

// concurrentThreadsLimit returns the number of threads we permit to
// run concurrently doing DNS lookups.
internal static nint concurrentThreadsLimit() {
    return 500;
}

} // end net_package
