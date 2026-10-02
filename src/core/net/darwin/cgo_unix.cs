// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file is called cgo_unix.go, but to allow syscalls-to-libc-based
// implementations to share the code, it does not use cgo directly.
// Instead of C.foo it uses _C_foo, which is defined in either
// cgo_unix_cgo.go or cgo_unix_syscall.go
//go:build !netgo && ((cgo && unix) || darwin)
namespace go;

using context = context_package;
using errors = errors_package;
using bytealg = @internal.bytealg_package;
using netip = net.netip_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using @internal;
using System.Runtime.CompilerServices;
using net;
using vendor.golang.org.x.net.dns;

partial class net_package {

// cgoAvailable set to true to indicate that the cgo resolver
// is available on this system.
internal const bool cgoAvailable = true;

[GoType("num:nint")] partial struct addrinfoErrno;

internal static @string Error(this addrinfoErrno eai) {
    return _C_gai_strerror(((_C_int)eai));
}

internal static bool Temporary(this addrinfoErrno eai) {
    return eai == _C_EAI_AGAIN;
}

internal static bool Timeout(this addrinfoErrno eai) {
    return false;
}

// isAddrinfoErrno is just for testing purposes.
internal static void isAddrinfoErrno(this addrinfoErrno eai) {
}

[GoType("dyn")] internal partial struct doBlockingWithCtx_result<T> {
    internal T res;
    internal error err;
}

// doBlockingWithCtx executes a blocking function in a separate goroutine when the provided
// context is cancellable. It is intended for use with calls that don't support context
// cancellation (cgo, syscalls). blocking func may still be running after this function finishes.
// For the duration of the execution of the blocking function, the thread is 'acquired' using [acquireThread],
// blocking might not be executed when the context gets canceled early.
[MethodImpl(MethodImplOptions.NoInlining)] internal static (T, error) doBlockingWithCtx<T>(context.Context ctx, @string lookupName, Func<(T, error)> blocking) {
    GoFrame ᒐ = default;
    try {
        {
            var err = acquireThread(ctx); if (err != default!) {
                T zero = GoZero<T>();
                return (zero, new DNSErrorжerror(Ꮡ(new DNSError(
                    Name: lookupName,
                    Err: mapErr(err).Error(),
                    IsTimeout: AreEqual(err, context.DeadlineExceeded)
                ))));
            }
        }
        if (ctx.Done() == default!) {
            defer(releaseThread, ref ᒐ);
            return blocking();
        }
        var res = new channel<doBlockingWithCtx_result<T>>(1);
        var resʗ1 = res;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(releaseThread, ref ᒐ);
                ref var r = ref heap(new doBlockingWithCtx_result<T>(), out var Ꮡr);
                (r.res, r.err) = blocking();
                resʗ1.ᐸꟷ(r);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var selᴛ1 = res;
        var selᴛ2 = ctx.Done();
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var r): {
            return (r.res, r.err);
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            T zero = GoZero<T>();
            return (zero, new DNSErrorжerror(Ꮡ(new DNSError(
                Name: lookupName,
                Err: mapErr(ctx.Err()).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ))));
        }}
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<@string> hosts, error err) cgoLookupHost(context.Context ctx, @string name) {
    slice<@string> hosts = default!;
    error err = default!;

    (var addrs, err) = cgoLookupIP(ctx, "ip"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (_, vᴛ1) in addrs) {
        ref var addr = ref heap(new IPAddr(), out var Ꮡaddr);
        addr = vᴛ1;

        hosts = append(hosts, Ꮡaddr.String());
    }
    return (hosts, default!);
}

internal static (nint port, error err) cgoLookupPort(context.Context ctx, @string network, @string service) {
    ref var hints = ref heap(new _C_struct_addrinfo(), out var Ꮡhints);
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8) {
    }
    else if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        _C_ai_socktype(Ꮡhints).Value = _C_SOCK_STREAM;
        _C_ai_protocol(Ꮡhints).Value = _C_IPPROTO_TCP;
    }
    else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        _C_ai_socktype(Ꮡhints).Value = _C_SOCK_DGRAM;
        _C_ai_protocol(Ꮡhints).Value = _C_IPPROTO_UDP;
    }
    else { /* default: */
        return (0, new DNSErrorжerror(Ꮡ(new DNSError( // no hints
Err: "unknown network"u8, Name: network + "/"u8 + service))));
    }

    switch (ipVersion(network)) {
    case (rune)'4': {
        _C_ai_family(Ꮡhints).Value = _C_AF_INET;
        break;
    }
    case (rune)'6': {
        _C_ai_family(Ꮡhints).Value = _C_AF_INET6;
        break;
    }}

    return doBlockingWithCtx(ctx, network + "/"u8 + service, (nint, error) () => cgoLookupServicePort(Ꮡhints, network, service));
}

internal static (nint port, error err) cgoLookupServicePort(ж<_C_struct_addrinfo> Ꮡhints, @string network, @string service) {
    nint port = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        (var cservice, err) = syscall.ByteSliceFromString(service);
        if (err != default!) {
            (port, err) = (0, new DNSErrorжerror(Ꮡ(new DNSError(Err: err.Error(), Name: network + "/"u8 + service)))); goto ᒐdone;
        }
        // Lowercase the C service name.
        foreach (var (i, b) in cservice.slice(0, len(service))) {
            cservice[i] = lowerASCII(b);
        }
        ref var res = ref heap<ж<_C_struct_addrinfo>>(out var Ꮡres);
        (var gerrno, err) = _C_getaddrinfo(nil, Ꮡ(cservice, 0), Ꮡhints, Ꮡres);
        if (gerrno != 0) {
            var exprᴛ1 = gerrno;
            if (exprᴛ1 == _C_EAI_SYSTEM) {
                if (err == default!) {
                    // see golang.org/issue/6232
                    err = syscall.EMFILE;
                }
                (port, err) = (0, new DNSErrorжerror(newDNSError(err, network + "/"u8 + service, ""u8))); goto ᒐdone;
            }
            if (exprᴛ1 == _C_EAI_SERVICE || exprᴛ1 == _C_EAI_NONAME) {
                (port, err) = (0, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errUnknownPort), // Darwin returns EAI_NONAME.
 network + "/"u8 + service, ""u8))); goto ᒐdone;
            }
            { /* default: */
                (port, err) = (0, new DNSErrorжerror(newDNSError(((addrinfoErrno)gerrno), network + "/"u8 + service, ""u8))); goto ᒐdone;
            }

        }
        defer(_C_freeaddrinfo, res, ref ᒐ);
        for (var r = res; r != nil; r = _C_ai_next(r).ValueSlot) {
            var exprᴛ2 = _C_ai_family(r).Value;
            if (exprᴛ2 == _C_AF_INET) {
                var sa = _C_ai_addr(r).ValueSlot.Reinterpret<_C_struct_sockaddr, syscall.RawSockaddrInet4>();
                var p = (NativeFieldArrayPointer<byte>(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2) ?? (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(sa.of(syscall.RawSockaddrInet4.ᏑPort))));
                (port, err) = ((nint)(((nint)p.ElementRef(0) << (int)(8)) | (nint)p.ElementRef(1)), default!); goto ᒐdone;
            }
            if (exprᴛ2 == _C_AF_INET6) {
                var sa = _C_ai_addr(r).ValueSlot.Reinterpret<_C_struct_sockaddr, syscall.RawSockaddrInet6>();
                var p = (NativeFieldArrayPointer<byte>(sa.of(syscall.RawSockaddrInet6.ᏑPort), 2) ?? (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(sa.of(syscall.RawSockaddrInet6.ᏑPort))));
                (port, err) = ((nint)(((nint)p.ElementRef(0) << (int)(8)) | (nint)p.ElementRef(1)), default!); goto ᒐdone;
            }

        }
        (port, err) = (0, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errUnknownPort), network + "/"u8 + service, ""u8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (port, err);
}

internal static (slice<IPAddr> addrs, error err) cgoLookupHostIP(@string network, @string name) {
    slice<IPAddr> addrs = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var hints = ref heap(new _C_struct_addrinfo(), out var Ꮡhints);
        _C_ai_flags(Ꮡhints).Value = cgoAddrInfoFlags;
        _C_ai_socktype(Ꮡhints).Value = _C_SOCK_STREAM;
        _C_ai_family(Ꮡhints).Value = _C_AF_UNSPEC;
        switch (ipVersion(network)) {
        case (rune)'4': {
            _C_ai_family(Ꮡhints).Value = _C_AF_INET;
            break;
        }
        case (rune)'6': {
            _C_ai_family(Ꮡhints).Value = _C_AF_INET6;
            break;
        }}

        (var h, err) = syscall.BytePtrFromString(name);
        if (err != default!) {
            (addrs, err) = (default!, new DNSErrorжerror(Ꮡ(new DNSError(Err: err.Error(), Name: name)))); goto ᒐdone;
        }
        ref var res = ref heap<ж<_C_struct_addrinfo>>(out var Ꮡres);
        (var gerrno, err) = _C_getaddrinfo(h, nil, Ꮡhints, Ꮡres);
        if (gerrno != 0) {
            var exprᴛ1 = gerrno;
            var matchᴛ1 = false;
            if (exprᴛ1 == _C_EAI_SYSTEM) { matchᴛ1 = true;
                if (err == default!) {
                    // err should not be nil, but sometimes getaddrinfo returns
                    // gerrno == _C_EAI_SYSTEM with err == nil on Linux.
                    // The report claims that it happens when we have too many
                    // open files, so use syscall.EMFILE (too many open files in system).
                    // Most system calls would return ENFILE (too many open files),
                    // so at the least EMFILE should be easy to recognize if this
                    // comes up again. golang.org/issue/6232.
                    err = syscall.EMFILE;
                }
                (addrs, err) = (default!, new DNSErrorжerror(newDNSError(err, name, ""u8))); goto ᒐdone;
            }
            if (exprᴛ1 == _C_EAI_NONAME || exprᴛ1 == _C_EAI_NODATA) { matchᴛ1 = true;
                (addrs, err) = (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8))); goto ᒐdone;
            }
            if (exprᴛ1 == _C_EAI_ADDRFAMILY) { matchᴛ1 = true;
                if (runtime.GOOS == "freebsd"u8) {
                    // FreeBSD began returning EAI_ADDRFAMILY for valid hosts without
                    // an A record in 13.2. We previously returned "no such host" for
                    // this case.
                    //
                    // https://bugs.freebsd.org/bugzilla/show_bug.cgi?id=273912
                    (addrs, err) = (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8))); goto ᒐdone;
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                (addrs, err) = (default!, new DNSErrorжerror(newDNSError(((addrinfoErrno)gerrno), name, ""u8))); goto ᒐdone;
            }

        }
        defer(_C_freeaddrinfo, res, ref ᒐ);
        for (var r = res; r != nil; r = _C_ai_next(r).ValueSlot) {
            // We only asked for SOCK_STREAM, but check anyhow.
            if (_C_ai_socktype(r).Value != _C_SOCK_STREAM) {
                continue;
            }
            var exprᴛ2 = _C_ai_family(r).Value;
            if (exprᴛ2 == _C_AF_INET) {
                var sa = _C_ai_addr(r).ValueSlot.Reinterpret<_C_struct_sockaddr, syscall.RawSockaddrInet4>();
                var addr = new IPAddr(IP: copyIP((~sa).Addr[..]));
                addrs = append(addrs, addr);
            }
            else if (exprᴛ2 == _C_AF_INET6) {
                var sa = _C_ai_addr(r).ValueSlot.Reinterpret<_C_struct_sockaddr, syscall.RawSockaddrInet6>();
                var addr = new IPAddr(IP: copyIP((~sa).Addr[..]), Zone: zoneCache.name((nint)(~sa).Scope_id));
                addrs = append(addrs, addr);
            }

        }
        (addrs, err) = (addrs, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (addrs, err);
}

internal static (slice<IPAddr> addrs, error err) cgoLookupIP(context.Context ctx, @string network, @string name) {
    return doBlockingWithCtx(ctx, name, (slice<IPAddr>, error) () => cgoLookupHostIP(network, name));
}

// These are roughly enough for the following:
//
//	 Source		Encoding			Maximum length of single name entry
//	 Unicast DNS		ASCII or			<=253 + a NUL terminator
//				Unicode in RFC 5892		252 * total number of labels + delimiters + a NUL terminator
//	 Multicast DNS	UTF-8 in RFC 5198 or		<=253 + a NUL terminator
//				the same as unicast DNS ASCII	<=253 + a NUL terminator
//	 Local database	various				depends on implementation
internal static UntypedInt nameinfoLen => 64;

internal static UntypedInt maxNameinfoLen => 4096;

internal static (slice<@string> names, error err) cgoLookupPTR(context.Context ctx, @string addr) {
    error err = default!;

    (var ip, err) = netip.ParseAddr(addr);
    if (err != default!) {
        return (default!, new DNSErrorжerror(Ꮡ(new DNSError(Err: "invalid address"u8, Name: addr))));
    }
    var (sa, salen) = cgoSockaddr(((IP)ip.AsSlice()), ip.Zone());
    if (sa == nil) {
        return (default!, new DNSErrorжerror(Ꮡ(new DNSError(Err: "invalid address "u8 + ip.String(), Name: addr))));
    }
    var saʗ1 = sa;
    return doBlockingWithCtx(ctx, addr, (slice<@string>, error) () => cgoLookupAddrPTR(addr, saʗ1, salen));
}

internal static (slice<@string> names, error err) cgoLookupAddrPTR(@string addr, ж<_C_struct_sockaddr> Ꮡsa, _C_socklen_t salen) {
    error err = default!;

    nint gerrno = default!;
    slice<byte> b = default!;
    for (nint l = nameinfoLen; l <= maxNameinfoLen; l *= 2) {
        b = new slice<byte>(l);
        (gerrno, err) = cgoNameinfoPTR(b, Ꮡsa, salen);
        if (gerrno == 0 || gerrno != _C_EAI_OVERFLOW) {
            break;
        }
    }
    if (gerrno != 0) {
        var exprᴛ1 = gerrno;
        if (exprᴛ1 == _C_EAI_SYSTEM) {
            if (err == default!) {
                // see golang.org/issue/6232
                err = syscall.EMFILE;
            }
            return (default!, new DNSErrorжerror(newDNSError(err, addr, ""u8)));
        }
        if (exprᴛ1 == _C_EAI_NONAME) {
            return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), addr, ""u8)));
        }
        { /* default: */
            return (default!, new DNSErrorжerror(newDNSError(((addrinfoErrno)gerrno), addr, ""u8)));
        }

    }
    {
        nint i = bytealg.IndexByte(b, 0); if (i != -1) {
            b = b.slice(0, i);
        }
    }
    return (new @string[]{absDomainName(((@string)b))}.slice(), default!);
}

internal static (ж<_C_struct_sockaddr>, _C_socklen_t) cgoSockaddr(IP ip, @string zone) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            return (cgoSockaddrInet4(ip4), ((_C_socklen_t)syscall.SizeofSockaddrInet4));
        }
    }
    {
        var ip6 = ip.To16(); if (ip6 != default!) {
            return (cgoSockaddrInet6(ip6, zoneCache.index(zone)), ((_C_socklen_t)syscall.SizeofSockaddrInet6));
        }
    }
    return (default!, 0);
}

internal static (@string cname, error err, bool completed) cgoLookupCNAME(context.Context ctx, @string name) {
    @string cname = default!;
    error err = default!;
    bool completed = default!;

    (var resources, err) = resSearch(ctx, name, (nint)(uint16)dnsmessage.TypeCNAME, (nint)(uint16)dnsmessage.ClassINET);
    if (err != default!) {
        return (cname, err, completed);
    }
    (cname, err) = parseCNAMEFromResources(resources);
    if (err != default!) {
        return ("", err, false);
    }
    return (cname, default!, true);
}

// resSearch will make a call to the 'res_nsearch' routine in the C library
// and parse the output as a slice of DNS resources.
internal static (slice<dnsmessage.Resource>, error) resSearch(context.Context ctx, @string hostname, nint rtype, nint @class) {
    return doBlockingWithCtx(ctx, hostname, (slice<dnsmessage.Resource>, error) () => cgoResSearch(hostname, rtype, @class));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resNsearchFailureˢ = "res_nsearch failure"u8;

internal static (slice<dnsmessage.Resource>, error) cgoResSearch(@string hostname, nint rtype, nint @class) {
    GoFrame ᒐ = default;
    try {
        var resStateSize = /* unsafe.Sizeof(_C_struct___res_state{}) */ (uintptr)552;
        ж<_C_struct___res_state> state = default!;
        if (resStateSize > 0) {
            @unsafe.Pointer mem = (uintptr)_C_malloc(resStateSize);
            defer(_C_free, mem, ref ᒐ);
            var memSlice = @unsafe.Slice((ж<byte>)(uintptr)(mem), resStateSize);
            clear(memSlice);
            state = Ꮡ(memSlice, 0).Reinterpret<byte, _C_struct___res_state>();
        }
        {
            var errΔ1 = _C_res_ninit(state); if (errΔ1 != default!) {
                return (default!, errors.New("res_ninit failure: "u8 + errΔ1.Error()));
            }
        }
        defer(_C_res_nclose, state, ref ᒐ);
        // Some res_nsearch implementations (like macOS) do not set errno.
        // They set h_errno, which is not per-thread and useless to us.
        // res_nsearch returns the size of the DNS response packet.
        // But if the DNS response packet contains failure-like response codes,
        // res_search returns -1 even though it has copied the packet into buf,
        // giving us no way to find out how big the packet is.
        // For now, we are willing to take res_search's word that there's nothing
        // useful in the response, even though there *is* a response.
        nint bufSize = maxDNSPacketSize;
        var buf = (ж<_C_uchar>)(uintptr)(_C_malloc((uintptr)bufSize));
        defer(_C_free, @unsafe.Pointer.FromPinnedBox(buf), ref ᒐ);
        var (s, err) = syscall.BytePtrFromString(hostname);
        if (err != default!) {
            return (default!, err);
        }
        nint size = default!;
        while (ᐧ) {
            nint sizeΔ1 = _C_res_nsearch(state, s, @class, rtype, buf, bufSize);
            if (sizeΔ1 <= 0 || sizeΔ1 > 0xffff) {
                return (default!, errors.New(resNsearchFailureˢ));
            }
            if (sizeΔ1 <= bufSize) {
                break;
            }
            // Allocate a bigger buffer to fit the entire msg.
            _C_free(@unsafe.Pointer.FromPinnedBox(buf));
            bufSize = sizeΔ1;
            buf = (ж<_C_uchar>)(uintptr)(_C_malloc((uintptr)bufSize));
        }
        dnsmessage.Parser p = default!;
        {
            var (_, errΔ2) = p.Start(@unsafe.Slice(buf, size)); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        p.SkipAllQuestions();
        (var resources, err) = p.AllAnswers();
        if (err != default!) {
            return (default!, err);
        }
        return (resources, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end net_package
