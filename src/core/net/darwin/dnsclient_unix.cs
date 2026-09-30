// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// DNS client: see RFC 1035.
// Has to be linked into package net for Dial.
// TODO(rsc):
//	Could potentially handle many outstanding lookups faster.
//	Random UDP source port (net.Dial should do that for us).
//	Random request IDs.
namespace go;

using context = context_package;
using errors = errors_package;
using bytealg = @internal.bytealg_package;
using godebug = @internal.godebug_package;
using itoa = @internal.itoa_package;
using stringslite = @internal.stringslite_package;
using Δio = io_package;
using os = os_package;
using runtime = runtime_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using @internal;
using fs = go.io.fs_package;
using go.io;
using go.sync;
using vendor.golang.org.x.net.dns;

partial class net_package {

internal const bool useTCPOnly = true;
internal const bool useUDPOrTCP = false;
internal static UntypedInt maxDNSPacketSize => 1232;

internal static error errLameReferral = errors.New("lame referral"u8);
internal static error errCannotUnmarshalDNSMessage = errors.New("cannot unmarshal DNS message"u8);
internal static error errCannotMarshalDNSMessage = errors.New("cannot marshal DNS message"u8);
internal static error errServerMisbehaving = errors.New("server misbehaving"u8);
internal static error errInvalidDNSResponse = errors.New("invalid DNS response"u8);
internal static error errNoAnswerFromDNSServer = errors.New("no answer from DNS server"u8);
internal static ж<temporaryError> errServerTemporarilyMisbehaving = Ꮡ(new temporaryError("server misbehaving"u8));

// netedns0 controls whether we send an EDNS0 additional header.
internal static ж<godebug.Setting> netedns0 = godebug.New("netedns0"u8);

internal static (uint16 id, slice<byte> udpReq, slice<byte> tcpReq, error err) newRequest(dnsmessageꓸQuestion q, bool ad) {
    uint16 id = default!;
    slice<byte> udpReq = default!;
    slice<byte> tcpReq = default!;
    error err = default!;

    q = q.ΔClone();
    id = (uint16)randInt();
    ref var b = ref heap<dnsmessage.Builder>(out var Ꮡb);
    b = dnsmessage.NewBuilder(new slice<byte>(2, 514), new dnsmessage.Header(ID: id, RecursionDesired: true, AuthenticData: ad));
    {
        var errΔ1 = b.StartQuestions(); if (errΔ1 != default!) {
            return (0, default!, default!, errΔ1);
        }
    }
    {
        var errΔ2 = Ꮡb.Question(q); if (errΔ2 != default!) {
            return (0, default!, default!, errΔ2);
        }
    }
    if (netedns0.Value() == "0"u8){
        netedns0.IncNonDefault();
    } else {
        // Accept packets up to maxDNSPacketSize.  RFC 6891.
        {
            var errΔ3 = b.StartAdditionals(); if (errΔ3 != default!) {
                return (0, default!, default!, errΔ3);
            }
        }
        dnsmessage.ResourceHeader rh = new();
        {
            var errΔ4 = rh.SetEDNS0(maxDNSPacketSize, dnsmessage.RCodeSuccess, false); if (errΔ4 != default!) {
                return (0, default!, default!, errΔ4);
            }
        }
        {
            var errΔ5 = Ꮡb.OPTResource(rh, new dnsmessageꓸOPTResource(nil)); if (errΔ5 != default!) {
                return (0, default!, default!, errΔ5);
            }
        }
    }
    (tcpReq, err) = b.Finish();
    if (err != default!) {
        return (0, default!, default!, err);
    }
    udpReq = tcpReq[2..];
    nint l = len(tcpReq) - 2;
    tcpReq[0] = (byte)((l >> (int)(8)));
    tcpReq[1] = (byte)l;
    return (id, udpReq, tcpReq, default!);
}

internal static bool checkResponse(uint16 reqID, dnsmessageꓸQuestion reqQues, dnsmessage.Header respHdr, dnsmessageꓸQuestion respQues) {
    reqQues = reqQues.ΔClone();
    respQues = respQues.ΔClone();

    if (!respHdr.Response) {
        return false;
    }
    if (reqID != respHdr.ID) {
        return false;
    }
    if (reqQues.Type != respQues.Type || reqQues.Class != respQues.Class || !equalASCIIName(reqQues.Name, respQues.Name)) {
        return false;
    }
    return true;
}

internal static (dnsmessage.Parser, dnsmessage.Header, error) dnsPacketRoundTrip(Conn c, uint16 id, dnsmessageꓸQuestion query, slice<byte> b) {
    query = query.ΔClone();

    {
        var (_, err) = c.Write(b); if (err != default!) {
            return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), err);
        }
    }
    b = new slice<byte>(maxDNSPacketSize);
    while (ᐧ) {
        var (n, err) = c.Read(b);
        if (err != default!) {
            return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), err);
        }
        dnsmessage.Parser p = default!;
        // Ignore invalid responses as they may be malicious
        // forgery attempts. Instead continue waiting until
        // timeout. See golang.org/issue/13281.
        (var h, err) = p.Start(b.slice(0, n));
        if (err != default!) {
            continue;
        }
        (var q, err) = p.Question();
        if (err != default! || !checkResponse(id, query, h, q)) {
            continue;
        }
        return (p, h, default!);
    }
}

internal static (dnsmessage.Parser, dnsmessage.Header, error) dnsStreamRoundTrip(Conn c, uint16 id, dnsmessageꓸQuestion query, slice<byte> b) {
    query = query.ΔClone();

    {
        var (_, errΔ1) = c.Write(b); if (errΔ1 != default!) {
            return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errΔ1);
        }
    }
    b = new slice<byte>(1280); // 1280 is a reasonable initial size for IP over Ethernet, see RFC 4035
    {
        var (_, errΔ2) = Δio.ReadFull(new ConnᴠReader(c), b[..2]); if (errΔ2 != default!) {
            return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errΔ2);
        }
    }
    nint l = (nint)(((nint)b[0] << (int)(8)) | (nint)b[1]);
    if (l > len(b)) {
        b = new slice<byte>(l);
    }
    var (n, err) = Δio.ReadFull(new ConnᴠReader(c), b.slice(0, l));
    if (err != default!) {
        return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), err);
    }
    dnsmessage.Parser p = default!;
    (var h, err) = p.Start(b.slice(0, n));
    if (err != default!) {
        return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errCannotUnmarshalDNSMessage);
    }
    (var q, err) = p.Question();
    if (err != default!) {
        return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errCannotUnmarshalDNSMessage);
    }
    if (!checkResponse(id, query, h, q)) {
        return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errInvalidDNSResponse);
    }
    return (p, h, default!);
}

// exchange sends a query on the connection and hopes for a response.
internal static (dnsmessage.Parser, dnsmessage.Header, error) exchange(this ж<Resolver> Ꮡr, context.Context ctx, @string server, dnsmessageꓸQuestion q, time.Duration timeout, bool useTCP, bool ad) {
    GoFrame ᒐ = default;
    try {
        q = q.ΔClone();

        q.Class = dnsmessage.ClassINET;
        var (id, udpReq, tcpReq, err) = newRequest(q, ad);
        if (err != default!) {
            return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errCannotMarshalDNSMessage);
        }
        slice<@string> networks = default!;
        if (useTCP){
            networks = new @string[]{"tcp"u8}.slice();
        } else {
            networks = new @string[]{"udp"u8, "tcp"u8}.slice();
        }
        foreach (var (_, network) in networks) {
            var (ctxΔ1, cancel) = context.WithDeadline(ctx, time.Now().Add(timeout));
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            var (c, errΔ1) = Ꮡr.dial(ctxΔ1, network, server);
            if (errΔ1 != default!) {
                return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errΔ1);
            }
            {
                var (d, ok) = ctxΔ1.Deadline(); if (ok && !d.IsZero()) {
                    c.SetDeadline(d);
                }
            }
            dnsmessage.Parser p = default!;
            dnsmessage.Header h = default!;
            {
                var (_, ok) = c._<PacketConn>(ᐧ); if (ok){
                    (p, h, errΔ1) = dnsPacketRoundTrip(c, id, q, udpReq);
                } else {
                    (p, h, errΔ1) = dnsStreamRoundTrip(c, id, q, tcpReq);
                }
            }
            c.Close();
            if (errΔ1 != default!) {
                return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), mapErr(errΔ1));
            }
            {
                var errΔ2 = p.SkipQuestion(); if (!AreEqual(errΔ2, dnsmessage.ErrSectionDone)) {
                    return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errInvalidDNSResponse);
                }
            }
            // RFC 5966 indicates that when a client receives a UDP response with
            // the TC flag set, it should take the TC flag as an indication that it
            // should retry over TCP instead.
            // The case when the TC flag is set in a TCP response is not well specified,
            // so this implements the glibc resolver behavior, returning the existing
            // dns response instead of returning a "errNoAnswerFromDNSServer" error.
            // See go.dev/issue/64896
            if (h.Truncated && network == "udp"u8) {
                continue;
            }
            return (p, h, default!);
        }
        return (new dnsmessage.Parser(nil), new dnsmessage.Header(nil), errNoAnswerFromDNSServer);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// checkHeader performs basic sanity checks on the header.
internal static error checkHeader(ж<dnsmessage.Parser> Ꮡp, dnsmessage.Header h) {
    ref var p = ref Ꮡp.DerefOrNull();

    var (rcode, hasAdd) = extractExtendedRCode(p, h);
    if (rcode == dnsmessage.RCodeNameError) {
        return new notFoundErrorжerror(errNoSuchHost);
    }
    var (_, err) = p.AnswerHeader();
    if (err != default! && !AreEqual(err, dnsmessage.ErrSectionDone)) {
        return errCannotUnmarshalDNSMessage;
    }
    // libresolv continues to the next server when it receives
    // an invalid referral response. See golang.org/issue/15434.
    if (rcode == dnsmessage.RCodeSuccess && !h.Authoritative && !h.RecursionAvailable && AreEqual(err, dnsmessage.ErrSectionDone) && !hasAdd) {
        return errLameReferral;
    }
    if (rcode != dnsmessage.RCodeSuccess && rcode != dnsmessage.RCodeNameError) {
        // None of the error codes make sense
        // for the query we sent. If we didn't get
        // a name error and we didn't get success,
        // the server is behaving incorrectly or
        // having temporary trouble.
        if (rcode == dnsmessage.RCodeServerFailure) {
            return new temporaryErrorжerror(errServerTemporarilyMisbehaving);
        }
        return errServerMisbehaving;
    }
    return default!;
}

internal static error skipToAnswer(ж<dnsmessage.Parser> Ꮡp, dnsmessage.Type qtype) {
    ref var p = ref Ꮡp.DerefOrNull();

    while (ᐧ) {
        var (h, err) = p.AnswerHeader();
        if (AreEqual(err, dnsmessage.ErrSectionDone)) {
            return new notFoundErrorжerror(errNoSuchHost);
        }
        if (err != default!) {
            return errCannotUnmarshalDNSMessage;
        }
        if (h.Type == qtype) {
            return default!;
        }
        {
            var errΔ1 = p.SkipAnswer(); if (errΔ1 != default!) {
                return errCannotUnmarshalDNSMessage;
            }
        }
    }
}

// extractExtendedRCode extracts the extended RCode from the OPT resource (EDNS(0))
// If an OPT record is not found, the RCode from the hdr is returned.
// Another return value indicates whether an additional resource was found.
internal static (dnsmessage.RCode, bool) extractExtendedRCode(dnsmessage.Parser p, dnsmessage.Header hdr) {
    p.SkipAllAnswers();
    p.SkipAllAuthorities();
    var hasAdd = false;
    while (ᐧ) {
        var (ahdr, err) = p.AdditionalHeader();
        if (err != default!) {
            return (hdr.RCode, hasAdd);
        }
        hasAdd = true;
        if (ahdr.Type == dnsmessage.TypeOPT) {
            return (ahdr.ExtendedRCode(hdr.RCode), hasAdd);
        }
        {
            var errΔ1 = p.SkipAdditional(); if (errΔ1 != default!) {
                return (hdr.RCode, hasAdd);
            }
        }
    }
}

// Do a lookup for a single name, which must be rooted
// (otherwise answer will not find the answers).
internal static (dnsmessage.Parser, @string, error) tryOneName(this ж<Resolver> Ꮡr, context.Context ctx, ж<dnsConfig> Ꮡcfg, @string name, dnsmessage.Type qtype) {
    ref var cfg = ref Ꮡcfg.DerefOrNull();

    error lastErr = default!;
    var serverOffset = Ꮡcfg.serverOffset();
    var sLen = (uint32)len(cfg.servers);
    var (n, err) = dnsmessage.NewName(name);
    if (err != default!) {
        return (new dnsmessage.Parser(nil), "", new DNSErrorжerror(Ꮡ(new DNSError(Err: errCannotMarshalDNSMessage.Error(), Name: name))));
    }
    var q = new dnsmessageꓸQuestion(
        Name: n.ΔClone(),
        Type: qtype,
        Class: dnsmessage.ClassINET
    );
    for (nint i = 0; i < cfg.attempts; i++) {
        for (var j = (uint32)0; j < sLen; j++) {
            @string server = cfg.servers[(serverOffset + j) % sLen];
            ref var p = ref heap<dnsmessage.Parser>(out var Ꮡp);
            (p, var h, var errΔ1) = Ꮡr.exchange(ctx, server, q, cfg.timeout, cfg.useTCP, cfg.trustAD);
            if (errΔ1 != default!) {
                var dnsErr = newDNSError(errΔ1, name, server);
                // Set IsTemporary for socket-level errors. Note that this flag
                // may also be used to indicate a SERVFAIL response.
                {
                    var (_, ok) = errΔ1._<ж<OpError>>(ᐧ); if (ok) {
                        dnsErr.Value.IsTemporary = true;
                    }
                }
                lastErr = new DNSErrorжerror(dnsErr);
                continue;
            }
            {
                var errΔ2 = checkHeader(Ꮡp, h); if (errΔ2 != default!) {
                    if (AreEqual(errΔ2, errNoSuchHost)) {
                        // The name does not exist, so trying
                        // another server won't help.
                        return (p, server, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, server)));
                    }
                    lastErr = new DNSErrorжerror(newDNSError(errΔ2, name, server));
                    continue;
                }
            }
            {
                var errΔ3 = skipToAnswer(Ꮡp, qtype); if (errΔ3 != default!) {
                    if (AreEqual(errΔ3, errNoSuchHost)) {
                        // The name does not exist, so trying
                        // another server won't help.
                        return (p, server, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, server)));
                    }
                    lastErr = new DNSErrorжerror(newDNSError(errΔ3, name, server));
                    continue;
                }
            }
            return (p, server, default!);
        }
    }
    return (new dnsmessage.Parser(nil), "", lastErr);
}

// A resolverConfig represents a DNS stub resolver configuration.
[GoType] partial struct resolverConfig {
    internal Δsync.Once initOnce; // guards init of resolverConfig
    // ch is used as a semaphore that only allows one lookup at a
    // time to recheck resolv.conf.
    internal channel<EmptyStruct> ch; // guards lastChecked and modTime
    internal time.Time lastChecked;     // last time resolv.conf was checked
    internal atomic.Pointer<dnsConfig> dnsConfig; // parsed resolv.conf structure used in lookups
}

internal static ж<resolverConfig> ᏑresolvConf = new StandardBox<resolverConfig>(default(resolverConfig));
internal static ref resolverConfig resolvConf => ref ᏑresolvConf.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string etcResolvConfˢ = "/etc/resolv.conf"u8;

internal static ж<dnsConfig> getSystemDNSConfig() {
    ᏑresolvConf.tryUpdate(etcResolvConfˢ);
    return ᏑresolvConf.of(resolverConfig.ᏑdnsConfig).Load();
}

// init initializes conf and is only called via conf.initOnce.
internal static void init(this ж<resolverConfig> Ꮡconf) {
    ref var conf = ref Ꮡconf.DerefOrNull();

    // Set dnsConfig and lastChecked so we don't parse
    // resolv.conf twice the first time.
    Ꮡconf.of(resolverConfig.ᏑdnsConfig).Store(dnsReadConfig(etcResolvConfˢ));
    conf.lastChecked = time.Now();
    // Prepare ch so that only one update of resolverConfig may
    // run at once.
    conf.ch = new channel<EmptyStruct>(1);
}

// tryUpdate tries to update conf with the named resolv.conf file.
// The name variable only exists for testing. It is otherwise always
// "/etc/resolv.conf".
internal static void tryUpdate(this ж<resolverConfig> Ꮡconf, @string name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var conf = ref Ꮡconf.DerefOrNull();

        Ꮡconf.of(resolverConfig.ᏑinitOnce).Do(Ꮡconf.init);
        if ((~Ꮡconf.of(resolverConfig.ᏑdnsConfig).Load()).noReload) {
            return;
        }
        // Ensure only one update at a time checks resolv.conf.
        if (!conf.tryAcquireSema()) {
            return;
        }
        ᒐd1 = true;
        var now = time.Now();
        if (conf.lastChecked.After(now.Add((time.Duration)(-5000000000L)))) {
            return;
        }
        conf.lastChecked = now;
        var exprᴛ1 = runtime.GOOS;
        if (exprᴛ1 == "windows"u8) {
        }
        else { /* default: */
// There's no file on disk, so don't bother checking
// and failing.
//
// The Windows implementation of dnsReadConfig (called
// below) ignores the name.
            time.Time mtime = default!;
            {
                var (fi, err) = os.Stat(name); if (err == default!) {
                    mtime = fi.ModTime();
                }
            }
            if (mtime.Equal((~Ꮡconf.of(resolverConfig.ᏑdnsConfig).Load()).mtime)) {
                return;
            }
        }

        var dnsConf = dnsReadConfig(name);
        Ꮡconf.of(resolverConfig.ᏑdnsConfig).Store(dnsConf);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡconf.DerefOrNull().releaseSema(); ᒐ.Run(); }
}

[GoRecv] internal static bool tryAcquireSema(this ref resolverConfig conf) {
    var selᴛ10 = conf.ch.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
    switch (trySelect(selᴛ10)) {
    case 0: {
        return true;
    }
    default: {
        return false;
    }}
}

[GoRecv] internal static void releaseSema(this ref resolverConfig conf) {
    ᐸꟷ(conf.ch);
}

internal static (dnsmessage.Parser, @string, error) lookup(this ж<Resolver> Ꮡr, context.Context ctx, @string name, dnsmessage.Type qtype, ж<dnsConfig> Ꮡconf) {
    ref var conf = ref Ꮡconf.DerefOrNull();

    if (!isDomainName(name)) {
        // We used to use "invalid domain name" as the error,
        // but that is a detail of the specific lookup mechanism.
        // Other lookups might allow broader name syntax
        // (for example Multicast DNS allows UTF-8; see RFC 6762).
        // For consistency with libc resolvers, report no such host.
        return (new dnsmessage.Parser(nil), "", new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8)));
    }
    if (Ꮡconf == nil) {
        Ꮡconf = getSystemDNSConfig(); conf = ref Ꮡconf.DerefOrNull();
    }
    dnsmessage.Parser p = default!;
    @string server = default!;
    error err = default!;
    foreach (var (_, fqdn) in conf.nameList(name)) {
        (p, server, err) = Ꮡr.tryOneName(ctx, Ꮡconf, fqdn, qtype);
        if (err == default!) {
            break;
        }
        {
            var (nerr, ok) = err._<ΔError>(ᐧ); if (ok && nerr.Temporary() && Ꮡr.strictErrors()) {
                // If we hit a temporary error with StrictErrors enabled,
                // stop immediately instead of trying more names.
                break;
            }
        }
    }
    if (err == default!) {
        return (p, server, default!);
    }
    {
        var (errΔ1, ok) = err._<ж<DNSError>>(ᐧ); if (ok) {
            // Show original name passed to lookup, not suffixed one.
            // In general we might have tried many suffixes; showing
            // just one is misleading. See also golang.org/issue/6324.
            errΔ1.Value.Name = name;
        }
    }
    return (new dnsmessage.Parser(nil), "", err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onionˢ = ".onion"u8;

// avoidDNS reports whether this is a hostname for which we should not
// use DNS. Currently this includes only .onion, per RFC 7686. See
// golang.org/issue/13705. Does not cover .local names (RFC 6762),
// see golang.org/issue/16739.
internal static bool avoidDNS(@string name) {
    if (name == ""u8) {
        return true;
    }
    name = stringslite.TrimSuffix(name, "."u8);
    return stringsHasSuffixFold(name, onionˢ);
}

// nameList returns a list of names for sequential DNS queries.
[GoRecv] internal static slice<@string> nameList(this ref dnsConfig conf, @string name) {
    // Check name length (see isDomainName).
    nint l = len(name);
    var rooted = l > 0 && name[l - 1] == (rune)'.';
    if (l > 254 || l == 254 && !rooted) {
        return default!;
    }
    // If name is rooted (trailing dot), try only that name.
    if (rooted) {
        if (avoidDNS(name)) {
            return default!;
        }
        return new @string[]{name}.slice();
    }
    var hasNdots = bytealg.CountString(name, (rune)'.') >= conf.ndots;
    name += "."u8;
    l++;
    // Build list of search choices.
    var names = new slice<@string>(0, 1 + len(conf.search));
    // If name has enough dots, try unsuffixed first.
    if (hasNdots && !avoidDNS(name)) {
        names = append(names, name);
    }
    // Try suffixes that are not too long (see isDomainName).
    foreach (var (_, suffix) in conf.search) {
        @string fqdn = name + suffix;
        if (!avoidDNS(fqdn) && len(fqdn) <= 254) {
            names = append(names, fqdn);
        }
    }
    // Try unsuffixed, if not tried first above.
    if (!hasNdots && !avoidDNS(name)) {
        names = append(names, name);
    }
    return names;
}

[GoType("num:nint")] partial struct ΔhostLookupOrder;

internal static ΔhostLookupOrder hostLookupCgo => /* iota */ 0;
internal static ΔhostLookupOrder hostLookupFilesDNS => 1; // files first
internal static ΔhostLookupOrder hostLookupDNSFiles => 2; // dns first
internal static ΔhostLookupOrder hostLookupFiles => 3; // only files
internal static ΔhostLookupOrder hostLookupDNS => 4; // only DNS

internal static map<ΔhostLookupOrder, @string> lookupOrderName = new map<ΔhostLookupOrder, @string>{
    [hostLookupCgo] = "cgo"u8,
    [hostLookupFilesDNS] = "files,dns"u8,
    [hostLookupDNSFiles] = "dns,files"u8,
    [hostLookupFiles] = "files"u8,
    [hostLookupDNS] = "dns"u8
};

internal static @string String(this ΔhostLookupOrder o) {
    {
        var (s, ok) = lookupOrderName[o, ꟷ]; if (ok) {
            return s;
        }
    }
    return "hostLookupOrder="u8 + itoa.Itoa((nint)o) + "??"u8;
}

internal static (slice<@string> addrs, error err) goLookupHostOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string name, ΔhostLookupOrder order, ж<dnsConfig> Ꮡconf) {
    slice<@string> addrs = default!;
    error err = default!;

    if (order == hostLookupFilesDNS || order == hostLookupFiles) {
        // Use entries from /etc/hosts if they match.
        (addrs, _) = lookupStaticHost(name);
        if (len(addrs) > 0) {
            return (addrs, err);
        }
        if (order == hostLookupFiles) {
            return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8)));
        }
    }
    (var ips, _, err) = Ꮡr.goLookupIPCNAMEOrder(ctx, "ip"u8, name, order, Ꮡconf);
    if (err != default!) {
        return (addrs, err);
    }
    addrs = new slice<@string>(0, len(ips));
    foreach (var (_, vᴛ1) in ips) {
        ref var ip = ref heap(new IPAddr(), out var Ꮡip);
        ip = vᴛ1;

        addrs = append(addrs, Ꮡip.String());
    }
    return (addrs, err);
}

// lookup entries from /etc/hosts
internal static (slice<IPAddr> addrs, @string canonical) goLookupIPFiles(@string name) {
    slice<IPAddr> addrs = default!;
    @string canonical = default!;

    (var addr, canonical) = lookupStaticHost(name);
    foreach (var (_, haddr) in addr) {
        var (haddrΔ1, zone) = splitHostZone(haddr);
        {
            var ip = ParseIP(haddrΔ1); if (ip != default!) {
                var addrΔ1 = new IPAddr(IP: ip, Zone: zone);
                addrs = append(addrs, addrΔ1);
            }
        }
    }
    sortByRFC6724(addrs);
    return (addrs, canonical);
}

// goLookupIP is the native Go implementation of LookupIP.
// The libc versions are in cgo_*.go.
internal static (slice<IPAddr> addrs, error err) goLookupIP(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string host, ΔhostLookupOrder order, ж<dnsConfig> Ꮡconf) {
    slice<IPAddr> addrs = default!;
    error err = default!;

    (addrs, _, err) = Ꮡr.goLookupIPCNAMEOrder(ctx, network, host, order, Ꮡconf);
    return (addrs, err);
}

[GoType("dyn")] internal partial struct goLookupIPCNAMEOrder_result {
    internal dnsmessage.Parser p;
    internal @string server;
    internal error error;
}

internal static (slice<IPAddr> addrs, dnsmessage.Name cname, error err) goLookupIPCNAMEOrder(this ж<Resolver> Ꮡr, context.Context ctx, @string network, @string name, ΔhostLookupOrder order, ж<dnsConfig> Ꮡconf) {
    slice<IPAddr> addrs = default!;
    dnsmessage.Name cname = new();

    ref var conf = ref Ꮡconf.DerefOrNull();
    if (order == hostLookupFilesDNS || order == hostLookupFiles) {
        @string canonical = default!;
        (addrs, canonical) = goLookupIPFiles(name);
        if (len(addrs) > 0) {
            error errΔ1 = default!;
            (cname, errΔ1) = dnsmessage.NewName(canonical);
            if (errΔ1 != default!) {
                return (default!, new dnsmessage.Name(nil), errΔ1);
            }
            return (addrs, cname.ΔClone(), default!);
        }
        if (order == hostLookupFiles) {
            return (default!, new dnsmessage.Name(nil), new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8)));
        }
    }
    if (!isDomainName(name)) {
        // See comment in func lookup above about use of errNoSuchHost.
        return (default!, new dnsmessage.Name(nil), new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), name, ""u8)));
    }
    if (Ꮡconf == nil) {
        Ꮡconf = getSystemDNSConfig(); conf = ref Ꮡconf.DerefOrNull();
    }
    var lane = new channel<goLookupIPCNAMEOrder_result>(1);
    var qtypes = new dnsmessage.Type[]{dnsmessage.TypeA, dnsmessage.TypeAAAA}.slice();
    if (network == "CNAME"u8) {
        qtypes = append(qtypes, dnsmessage.TypeCNAME);
    }
    switch (ipVersion(network)) {
    case (rune)'4': {
        qtypes = new dnsmessage.Type[]{dnsmessage.TypeA}.slice();
        break;
    }
    case (rune)'6': {
        qtypes = new dnsmessage.Type[]{dnsmessage.TypeAAAA}.slice();
        break;
    }}

    Action<@string, dnsmessage.Type> queryFn = default!;
    Func<@string, dnsmessage.Type, goLookupIPCNAMEOrder_result> responseFn = default!;
    if (conf.singleRequest){
        queryFn = (@string fqdn, dnsmessage.Type qtype) => {
        };
        responseFn = (@string fqdn, dnsmessage.Type qtype) => {
            GoFrame ᒐ = default;
            try {
                ᏑdnsWaitGroup.Add(1);
                defer(ᏑdnsWaitGroup.Done, ref ᒐ);
                var (p, server, errΔ2) = Ꮡr.tryOneName(ctx, Ꮡconf, fqdn, qtype);
                return new goLookupIPCNAMEOrder_result(p, server, errΔ2);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        };
    } else {
        var laneʗ1 = lane;
        queryFn = (@string fqdn, dnsmessage.Type qtype) => {
            ᏑdnsWaitGroup.Add(1);
            var laneʗ2 = laneʗ1;
            goǃ((dnsmessage.Type qtypeΔ1) => {
                ref var p = ref heap<dnsmessage.Parser>(out var Ꮡp);
                (p, var server, var errΔ3) = Ꮡr.tryOneName(ctx, Ꮡconf, fqdn, qtypeΔ1);
                laneʗ2.ᐸꟷ(new goLookupIPCNAMEOrder_result(p, server, errΔ3));
                ᏑdnsWaitGroup.Done();
            }, qtype);
        };
        var laneʗ3 = lane;
        responseFn = (@string fqdn, dnsmessage.Type qtype) => ᐸꟷ(laneʗ3);
    }
    error lastErr = default!;
    foreach (var (_, fqdn) in conf.nameList(name)) {
        foreach (var (_, qtype) in qtypes) {
            queryFn(fqdn, qtype);
        }
        var hitStrictError = false;
        foreach (var (_, qtype) in qtypes) {
            var result = responseFn(fqdn, qtype);
            if (result.error != default!) {
                {
                    var (nerr, ok) = result.error._<ΔError>(ᐧ); if (ok && nerr.Temporary() && Ꮡr.strictErrors()){
                        // This error will abort the nameList loop.
                        hitStrictError = true;
                        lastErr = result.error;
                    } else 
                    if (lastErr == default! || fqdn == name + "."u8) {
                        // Prefer error for original name.
                        lastErr = result.error;
                    }
                }
                continue;
            }
            // Presotto says it's okay to assume that servers listed in
            // /etc/resolv.conf are recursive resolvers.
            //
            // We asked for recursion, so it should have included all the
            // answers we need in this one packet.
            //
            // Further, RFC 1034 section 4.3.1 says that "the recursive
            // response to a query will be... The answer to the query,
            // possibly preface by one or more CNAME RRs that specify
            // aliases encountered on the way to an answer."
            //
            // Therefore, we should be able to assume that we can ignore
            // CNAMEs and that the A and AAAA records we requested are
            // for the canonical name.
loop:
            while (ᐧ) {
                var (h, errΔ4) = result.p.AnswerHeader();
                if (errΔ4 != default! && !AreEqual(errΔ4, dnsmessage.ErrSectionDone)) {
                    lastErr = new DNSErrorжerror(Ꮡ(new DNSError(
                        Err: errCannotUnmarshalDNSMessage.Error(),
                        Name: name,
                        Server: result.server
                    )));
                }
                if (errΔ4 != default!) {
                    break;
                }
                var exprᴛ1 = h.Type;
                if (exprᴛ1 == dnsmessage.TypeA) {
                    var (a, errΔ7) = result.p.AResource();
                    if (errΔ7 != default!) {
                        lastErr = new DNSErrorжerror(Ꮡ(new DNSError(
                            Err: errCannotUnmarshalDNSMessage.Error(),
                            Name: name,
                            Server: result.server
                        )));
                        goto break_loop;
                    }
                    addrs = append(addrs, new IPAddr(IP: ((IP)(a.A[..]))));
                    if (cname.Length == 0 && h.Name.Length != 0) {
                        cname = h.Name.ΔClone();
                    }
                }
                else if (exprᴛ1 == dnsmessage.TypeAAAA) {
                    var (aaaa, errΔ8) = result.p.AAAAResource();
                    if (errΔ8 != default!) {
                        lastErr = new DNSErrorжerror(Ꮡ(new DNSError(
                            Err: errCannotUnmarshalDNSMessage.Error(),
                            Name: name,
                            Server: result.server
                        )));
                        goto break_loop;
                    }
                    addrs = append(addrs, new IPAddr(IP: ((IP)(aaaa.AAAA[..]))));
                    if (cname.Length == 0 && h.Name.Length != 0) {
                        cname = h.Name.ΔClone();
                    }
                }
                else if (exprᴛ1 == dnsmessage.TypeCNAME) {
                    var (c, errΔ9) = result.p.CNAMEResource();
                    if (errΔ9 != default!) {
                        lastErr = new DNSErrorжerror(Ꮡ(new DNSError(
                            Err: errCannotUnmarshalDNSMessage.Error(),
                            Name: name,
                            Server: result.server
                        )));
                        goto break_loop;
                    }
                    if (cname.Length == 0 && c.CNAME.Length > 0) {
                        cname = c.CNAME.ΔClone();
                    }
                }
                else { /* default: */
                    {
                        var errΔ10 = result.p.SkipAnswer(); if (errΔ10 != default!) {
                            lastErr = new DNSErrorжerror(Ꮡ(new DNSError(
                                Err: errCannotUnmarshalDNSMessage.Error(),
                                Name: name,
                                Server: result.server
                            )));
                            goto break_loop;
                        }
                    }
                    continue;
                }

continue_loop:;
            }
break_loop:;
        }
        if (hitStrictError) {
            // If either family hit an error with StrictErrors enabled,
            // discard all addresses. This ensures that network flakiness
            // cannot turn a dualstack hostname IPv4/IPv6-only.
            addrs = default!;
            break;
        }
        if (len(addrs) > 0 || network == "CNAME"u8 && cname.Length > 0) {
            break;
        }
    }
    {
        var (lastErrΔ1, ok) = lastErr._<ж<DNSError>>(ᐧ); if (ok) {
            // Show original name passed to lookup, not suffixed one.
            // In general we might have tried many suffixes; showing
            // just one is misleading. See also golang.org/issue/6324.
            lastErrΔ1.Value.Name = name;
        }
    }
    sortByRFC6724(addrs);
    if (len(addrs) == 0 && !(network == "CNAME"u8 && cname.Length > 0)) {
        if (order == hostLookupDNSFiles) {
            @string canonical = default!;
            (addrs, canonical) = goLookupIPFiles(name);
            if (len(addrs) > 0) {
                error errΔ11 = default!;
                (cname, errΔ11) = dnsmessage.NewName(canonical);
                if (errΔ11 != default!) {
                    return (default!, new dnsmessage.Name(nil), errΔ11);
                }
                return (addrs, cname.ΔClone(), default!);
            }
        }
        if (lastErr != default!) {
            return (default!, new dnsmessage.Name(nil), lastErr);
        }
    }
    return (addrs, cname.ΔClone(), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cnameˢ = "CNAME"u8;

// goLookupCNAME is the native Go (non-cgo) implementation of LookupCNAME.
internal static (@string, error) goLookupCNAME(this ж<Resolver> Ꮡr, context.Context ctx, @string host, ΔhostLookupOrder order, ж<dnsConfig> Ꮡconf) {
    var (_, cname, err) = Ꮡr.goLookupIPCNAMEOrder(ctx, cnameˢ, host, order, Ꮡconf);
    return (cname.String(), err);
}

// goLookupPTR is the native Go implementation of LookupAddr.
internal static (slice<@string>, error) goLookupPTR(this ж<Resolver> Ꮡr, context.Context ctx, @string addr, ΔhostLookupOrder order, ж<dnsConfig> Ꮡconf) {
    if (order == hostLookupFiles || order == hostLookupFilesDNS) {
        var names = lookupStaticAddr(addr);
        if (len(names) > 0) {
            return (names, default!);
        }
        if (order == hostLookupFiles) {
            return (default!, new DNSErrorжerror(newDNSError(new notFoundErrorжerror(errNoSuchHost), addr, ""u8)));
        }
    }
    var (arpa, err) = reverseaddr(addr);
    if (err != default!) {
        return (default!, err);
    }
    ref var server = ref heap<@string>(out var Ꮡserver);
    (var p, server, err) = Ꮡr.lookup(ctx, arpa, dnsmessage.TypePTR, Ꮡconf);
    if (err != default!) {
        ref var dnsErr = ref heap<ж<DNSError>>(out var ᏑdnsErr);
        if (errors.As(err, ᏑdnsErr) && (~dnsErr).IsNotFound) {
            if (order == hostLookupDNSFiles) {
                var names = lookupStaticAddr(addr);
                if (len(names) > 0) {
                    return (names, default!);
                }
            }
        }
        return (default!, err);
    }
    slice<@string> ptrs = default!;
    while (ᐧ) {
        var (h, errΔ1) = p.AnswerHeader();
        if (AreEqual(errΔ1, dnsmessage.ErrSectionDone)) {
            break;
        }
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: errCannotUnmarshalDNSMessage.Error(),
                Name: addr,
                Server: server
            ))));
        }
        if (h.Type != dnsmessage.TypePTR) {
            var errΔ2 = p.SkipAnswer();
            if (errΔ2 != default!) {
                return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                    Err: errCannotUnmarshalDNSMessage.Error(),
                    Name: addr,
                    Server: server
                ))));
            }
            continue;
        }
        (var ptr, errΔ1) = p.PTRResource();
        if (errΔ1 != default!) {
            return (default!, new DNSErrorжerror(Ꮡ(new DNSError(
                Err: errCannotUnmarshalDNSMessage.Error(),
                Name: addr,
                Server: server
            ))));
        }
        ptrs = append(ptrs, ptr.PTR.String());
    }
    return (ptrs, default!);
}

} // end net_package
