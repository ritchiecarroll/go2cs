// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using netip = net.netip_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using net;
using static go.net_package;
using Δio = io_package;

partial class net_internal_test_package {

internal static ж<global::go.net_package.Resolver> ᏑgoResolver = new StandardBox<global::go.net_package.Resolver>(new Resolver(PreferGo: true));
internal static ref global::go.net_package.Resolver goResolver => ref ᏑgoResolver.Value;

internal static bool hasSuffixFold(@string s, @string suffix) {
    return strings.HasSuffix(strings.ToLower(s), strings.ToLower(suffix));
}

internal static (slice<global::go.net_package.IPAddr>, error) lookupLocalhost(context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) {
    var exprᴛ1 = host;
    if (exprᴛ1 == "localhost"u8) {
        return (new global::go.net_package.IPAddr[]{
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback)
        }.slice(), default!);
    }
    { /* default: */
        return fn(ctx, network, host);
    }

}

// The Lookup APIs use various sources such as local database, DNS or
// mDNS, and may use platform-dependent DNS stub resolver if possible.
// The APIs accept any of forms for a query; host name in various
// encodings, UTF-8 encoded net name, domain name, FQDN or absolute
// FQDN, but the result would be one of the forms and it depends on
// the circumstances.
// non-standard back door

partial struct lookupGoogleSRVTestsᴛ1 /*dyn*/ {
    internal @string service, proto, name;
    internal @string cname, target;
}
internal static slice<lookupGoogleSRVTestsᴛ1> lookupGoogleSRVTests = new lookupGoogleSRVTestsᴛ1[]{
    new(
        "ldap"u8, "tcp"u8, "google.com"u8,
        "google.com."u8, "google.com."u8
    ),
    new(
        "ldap"u8, "tcp"u8, "google.com."u8,
        "google.com."u8, "google.com."u8
    ),
    new(
        ""u8, ""u8, "_ldap._tcp.google.com"u8,
        "google.com."u8, "google.com."u8
    ),
    new(
        ""u8, ""u8, "_ldap._tcp.google.com."u8,
        "google.com."u8, "google.com."u8
    )
}.slice();

internal static array<time.Duration> backoffDuration = new time.Duration[]{time.ΔSecond, (time.Duration)(5000000000L), (time.Duration)(30000000000L)}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noResolvConfOnIOSˢ = (@string)"no resolv.conf on iOS"u8;
internal static readonly object iPv4IsRequiredˢ = (@string)"IPv4 is required"u8;
internal static readonly object gotNoRecordˢ = (@string)"got no record"u8;

public static void TestLookupGoogleSRV(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    mustHaveExternalNetwork(Ꮡt);
    if (Δruntime.GOOS == "ios"u8) {
        Ꮡt.Skip(noResolvConfOnIOSˢ);
    }
    if (!supportsIPv4() || !testIPv4.Value) {
        Ꮡt.Skip(iPv4IsRequiredˢ);
    }
    nint attempts = 0;
    for (nint i = 0; i < len(lookupGoogleSRVTests); i++) {
        var tt = lookupGoogleSRVTests[i];
        var (cname, srvs, err) = LookupSRV(tt.service, tt.proto, tt.name);
        if (err != default!) {
            testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
            if (attempts < len(backoffDuration)) {
                var dur = backoffDuration[attempts];
                Ꮡt.Logf("backoff %v after failure %v\n"u8, dur, err);
                time.Sleep(dur);
                attempts++;
                i--;
                continue;
            }
            Ꮡt.Fatal(err);
        }
        if (len(srvs) == 0) {
            Ꮡt.Error(gotNoRecordˢ);
        }
        if (!hasSuffixFold(cname, tt.cname)) {
            Ꮡt.Errorf("got %s; want %s"u8, cname, tt.cname);
        }
        foreach (var (_, srv) in srvs) {
            if (!hasSuffixFold((~srv).Target, tt.target)) {
                Ꮡt.Errorf("got %v; want a record containing %s"u8, srv.OrTypedNil(), tt.target);
            }
        }
    }
}


partial struct lookupGmailMXTestsᴛ1 /*dyn*/ {
    internal @string name, host;
}
internal static slice<lookupGmailMXTestsᴛ1> lookupGmailMXTests = new lookupGmailMXTestsᴛ1[]{
    new("gmail.com"u8, "google.com."u8),
    new("gmail.com."u8, "google.com."u8)
}.slice();

public static void TestLookupGmailMX(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    mustHaveExternalNetwork(Ꮡt);
    if (Δruntime.GOOS == "ios"u8) {
        Ꮡt.Skip(noResolvConfOnIOSˢ);
    }
    if (!supportsIPv4() || !testIPv4.Value) {
        Ꮡt.Skip(iPv4IsRequiredˢ);
    }
    nint attempts = 0;
    for (nint i = 0; i < len(lookupGmailMXTests); i++) {
        var tt = lookupGmailMXTests[i];
        var (mxs, err) = LookupMX(tt.name);
        if (err != default!) {
            testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
            if (attempts < len(backoffDuration)) {
                var dur = backoffDuration[attempts];
                Ꮡt.Logf("backoff %v after failure %v\n"u8, dur, err);
                time.Sleep(dur);
                attempts++;
                i--;
                continue;
            }
            Ꮡt.Fatal(err);
        }
        if (len(mxs) == 0) {
            Ꮡt.Error(gotNoRecordˢ);
        }
        foreach (var (_, mx) in mxs) {
            if (!hasSuffixFold((~mx).Host, tt.host)) {
                Ꮡt.Errorf("got %v; want a record containing %s"u8, mx.OrTypedNil(), tt.host);
            }
        }
    }
}

internal static slice<lookupGmailMXTestsᴛ1> lookupGmailNSTests = new lookupGmailMXTestsᴛ1[]{
    new("gmail.com"u8, "google.com."u8),
    new("gmail.com."u8, "google.com."u8)
}.slice();

public static void TestLookupGmailNS(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    mustHaveExternalNetwork(Ꮡt);
    if (Δruntime.GOOS == "ios"u8) {
        Ꮡt.Skip(noResolvConfOnIOSˢ);
    }
    if (!supportsIPv4() || !testIPv4.Value) {
        Ꮡt.Skip(iPv4IsRequiredˢ);
    }
    nint attempts = 0;
    for (nint i = 0; i < len(lookupGmailNSTests); i++) {
        var tt = lookupGmailNSTests[i];
        var (nss, err) = LookupNS(tt.name);
        if (err != default!) {
            testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
            if (attempts < len(backoffDuration)) {
                var dur = backoffDuration[attempts];
                Ꮡt.Logf("backoff %v after failure %v\n"u8, dur, err);
                time.Sleep(dur);
                attempts++;
                i--;
                continue;
            }
            Ꮡt.Fatal(err);
        }
        if (len(nss) == 0) {
            Ꮡt.Error(gotNoRecordˢ);
        }
        foreach (var (_, ns) in nss) {
            if (!hasSuffixFold((~ns).Host, tt.host)) {
                Ꮡt.Errorf("got %v; want a record containing %s"u8, ns.OrTypedNil(), tt.host);
            }
        }
    }
}


partial struct lookupGmailTXTTestsᴛ1 /*dyn*/ {
    internal @string name, txt, host;
}
internal static slice<lookupGmailTXTTestsᴛ1> lookupGmailTXTTests = new lookupGmailTXTTestsᴛ1[]{
    new("gmail.com"u8, "spf"u8, "google.com"u8),
    new("gmail.com."u8, "spf"u8, "google.com"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnPlan9SeeHttpsˢ = (@string)"skipping on plan9; see https://golang.org/issue/29722"u8;

public static void TestLookupGmailTXT(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "plan9"u8) {
        Ꮡt.Skip(skippingOnPlan9SeeHttpsˢ);
    }
    Ꮡt.Parallel();
    mustHaveExternalNetwork(Ꮡt);
    if (Δruntime.GOOS == "ios"u8) {
        Ꮡt.Skip(noResolvConfOnIOSˢ);
    }
    if (!supportsIPv4() || !testIPv4.Value) {
        Ꮡt.Skip(iPv4IsRequiredˢ);
    }
    nint attempts = 0;
    for (nint i = 0; i < len(lookupGmailTXTTests); i++) {
        ref var tt = ref heap<lookupGmailTXTTestsᴛ1>(out var Ꮡtt);
        tt = lookupGmailTXTTests[i];
        var (txts, err) = LookupTXT(tt.name);
        if (err != default!) {
            testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
            if (attempts < len(backoffDuration)) {
                var dur = backoffDuration[attempts];
                Ꮡt.Logf("backoff %v after failure %v\n"u8, dur, err);
                time.Sleep(dur);
                attempts++;
                i--;
                continue;
            }
            Ꮡt.Fatal(err);
        }
        if (len(txts) == 0) {
            Ꮡt.Error(gotNoRecordˢ);
        }
        var ttʗ1 = tt;
        if (!slices.ContainsFunc(txts, (@string txt) => strings.Contains(txt, ttʗ1.txt) && (strings.HasSuffix(txt, ttʗ1.host) || strings.HasSuffix(txt, ttʗ1.host + "."u8)))) {
            Ꮡt.Errorf("got %v; want a record containing %s, %s"u8, txts, tt.txt, tt.host);
        }
    }
}

internal static slice<@string> lookupGooglePublicDNSAddrTests = new @string[]{
    "8.8.8.8"u8,
    "8.8.4.4"u8,
    "2001:4860:4860::8888"u8,
    "2001:4860:4860::8844"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string googleComˢ = ".google.com."u8;
internal static readonly @string googleˢ = ".google."u8;

public static void TestLookupGooglePublicDNSAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(Ꮡt);
        if (!supportsIPv4() || !supportsIPv6() || !testIPv4.Value || !testIPv6.Value) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (_, ip) in lookupGooglePublicDNSAddrTests) {
            var (names, err) = LookupAddr(ip);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (len(names) == 0) {
                Ꮡt.Error(gotNoRecordˢ);
            }
            foreach (var (_, name) in names) {
                if (!hasSuffixFold(name, googleComˢ) && !hasSuffixFold(name, googleˢ)) {
                    Ꮡt.Errorf("got %q; want a record ending in .google.com. or .google."u8, name);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv6IsRequiredˢ = (@string)"IPv6 is required"u8;
internal static readonly @string fe801Lo0ˢ = "fe80::1%lo0"u8;

public static void TestLookupIPv6LinkLocalAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!supportsIPv6() || !testIPv6.Value) {
            Ꮡt.Skip(iPv6IsRequiredˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        var (addrs, err) = LookupHost(localhostˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!slices.Contains(addrs, fe801Lo0ˢ)) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }
        {
            var (_, errΔ1) = LookupAddr(fe801Lo0ˢ); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lo0ˢ = "lo0"u8;

public static void TestLookupIPv6LinkLocalAddrWithZone(ж<testing.T> Ꮡt) {
    if (!supportsIPv6() || !testIPv6.Value) {
        Ꮡt.Skip(iPv6IsRequiredˢ);
    }
    var (ipaddrs, err) = DefaultResolver.LookupIPAddr(context.Background(), fe801Lo0ˢ);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    foreach (var (_, addr) in ipaddrs) {
        {
            @string e = lo0ˢ;
            @string a = addr.Zone; if (e != a) {
                Ꮡt.Errorf("wrong zone: want %q, got %q"u8, e, a);
            }
        }
    }
    (var addrs, err) = DefaultResolver.LookupHost(context.Background(), fe801Lo0ˢ);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    foreach (var (_, addr) in addrs) {
        {
            @string e = fe801Lo0ˢ;
            @string a = addr; if (e != a) {
                Ꮡt.Errorf("wrong host: want %q got %q"u8, e, a);
            }
        }
    }
}


partial struct lookupCNAMETestsᴛ1 /*dyn*/ {
    internal @string name, cname;
}
internal static slice<lookupCNAMETestsᴛ1> lookupCNAMETests = new lookupCNAMETestsᴛ1[]{
    new("www.iana.org"u8, "icann.org."u8),
    new("www.iana.org."u8, "icann.org."u8),
    new("www.google.com"u8, "google.com."u8),
    new("google.com"u8, "google.com."u8),
    new("cname-to-txt.go4.org"u8, "test-txt-record.go4.org."u8)
}.slice();

public static void TestLookupCNAME(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(Ꮡt);
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv4() || !testIPv4.Value) {
            Ꮡt.Skip(iPv4IsRequiredˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        nint attempts = 0;
        for (nint i = 0; i < len(lookupCNAMETests); i++) {
            var tt = lookupCNAMETests[i];
            var (cname, err) = LookupCNAME(tt.name);
            if (err != default!) {
                testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
                if (attempts < len(backoffDuration)) {
                    var dur = backoffDuration[attempts];
                    Ꮡt.Logf("backoff %v after failure %v\n"u8, dur, err);
                    time.Sleep(dur);
                    attempts++;
                    i--;
                    continue;
                }
                Ꮡt.Fatal(err);
            }
            if (!hasSuffixFold(cname, tt.cname)) {
                Ꮡt.Errorf("got %s; want a record containing %s"u8, cname, tt.cname);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


partial struct lookupGoogleHostTestsᴛ1 /*dyn*/ {
    internal @string name;
}
internal static slice<lookupGoogleHostTestsᴛ1> lookupGoogleHostTests = new lookupGoogleHostTestsᴛ1[]{
    new("google.com"u8),
    new("google.com."u8)
}.slice();

public static void TestLookupGoogleHost(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(Ꮡt);
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv4() || !testIPv4.Value) {
            Ꮡt.Skip(iPv4IsRequiredˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (_, tt) in lookupGoogleHostTests) {
            var (addrs, err) = LookupHost(tt.name);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (len(addrs) == 0) {
                Ꮡt.Error(gotNoRecordˢ);
            }
            foreach (var (_, addr) in addrs) {
                if (ParseIP(addr) == default!) {
                    Ꮡt.Errorf("got %q; want a literal IP address"u8, addr);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string golangRscIoˢ = "golang.rsc.io"u8;

public static void TestLookupLongTXT(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 22857);
        mustHaveExternalNetwork(Ꮡt);
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        var (txts, err) = LookupTXT(golangRscIoˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        slices.Sort<slice<@string>, @string>(txts);
        var want = new @string[]{
            strings.Repeat("abcdefghijklmnopqrstuvwxyABCDEFGHJIKLMNOPQRSTUVWXY"u8, 10),
            "gophers rule"u8
        }.slice();
        if (!slices.Equal<slice<@string>, @string>(txts, want)) {
            Ꮡt.Fatalf("LookupTXT golang.rsc.io incorrect\nhave %q\nwant %q"u8, txts, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<lookupGoogleHostTestsᴛ1> lookupGoogleIPTests = new lookupGoogleHostTestsᴛ1[]{
    new("google.com"u8),
    new("google.com."u8)
}.slice();

public static void TestLookupGoogleIP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(Ꮡt);
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv4() || !testIPv4.Value) {
            Ꮡt.Skip(iPv4IsRequiredˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (_, tt) in lookupGoogleIPTests) {
            var (ips, err) = LookupIP(tt.name);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (len(ips) == 0) {
                Ꮡt.Error(gotNoRecordˢ);
            }
            foreach (var (_, ip) in ips) {
                if (ip.To4() == default! && ip.To16() == default!) {
                    Ꮡt.Errorf("got %v; want an IP address"u8, ip);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


partial struct revAddrTestsᴛ1 /*dyn*/ {
    public @string Addr;
    public @string Reverse;
    public @string ErrPrefix;
}
internal static slice<revAddrTestsᴛ1> revAddrTests = new revAddrTestsᴛ1[]{
    new("1.2.3.4"u8, "4.3.2.1.in-addr.arpa."u8, ""u8),
    new("245.110.36.114"u8, "114.36.110.245.in-addr.arpa."u8, ""u8),
    new("::ffff:12.34.56.78"u8, "78.56.34.12.in-addr.arpa."u8, ""u8),
    new("::1"u8, "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa."u8, ""u8),
    new("1::"u8, "0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.1.0.0.0.ip6.arpa."u8, ""u8),
    new("1234:567::89a:bcde"u8, "e.d.c.b.a.9.8.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.7.6.5.0.4.3.2.1.ip6.arpa."u8, ""u8),
    new("1234:567:fefe:bcbc:adad:9e4a:89a:bcde"u8, "e.d.c.b.a.9.8.0.a.4.e.9.d.a.d.a.c.b.c.b.e.f.e.f.7.6.5.0.4.3.2.1.ip6.arpa."u8, ""u8),
    new("1.2.3"u8, ""u8, "unrecognized address"u8),
    new("1.2.3.4.5"u8, ""u8, "unrecognized address"u8),
    new("1234:567:bcbca::89a:bcde"u8, ""u8, "unrecognized address"u8),
    new("1234:567::bcbc:adad::89a:bcde"u8, ""u8, "unrecognized address"u8)
}.slice();

public static void TestReverseAddress(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (i, tt) in revAddrTests) {
            var (a, err) = reverseaddr(tt.Addr);
            if (len(tt.ErrPrefix) > 0 && err == default!) {
                Ꮡt.Errorf("#%d: expected %q, got <nil> (error)"u8, i, tt.ErrPrefix);
                continue;
            }
            if (len(tt.ErrPrefix) == 0 && err != default!) {
                Ꮡt.Errorf("#%d: expected <nil>, got %q (error)"u8, i, err);
            }
            if (err != default! && (~err._<ж<global::go.net_package.DNSError>>()).Err != tt.ErrPrefix) {
                Ꮡt.Errorf("#%d: expected %q, got %q (mismatched error)"u8, i, tt.ErrPrefix, (~err._<ж<global::go.net_package.DNSError>>()).Err);
            }
            if (a != tt.Reverse) {
                Ꮡt.Errorf("#%d: expected %q, got %q (reverse address)"u8, i, tt.Reverse, a);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testDisabledUseDnsfloodˢ = (@string)"test disabled; use -dnsflood to enable"u8;
internal static readonly object deadlineExceededˢ = (@string)"deadline exceeded"u8;

internal partial struct TestDNSFlood_qstats /*dyn*/ {
    internal nint succeeded, failed;
    internal nint timeout, temporary, other;
    internal nint unknown;
}

public static partial void TestDNSFlood(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testDNSFlood.Value) {
            Ꮡt.Skip(testDisabledUseDnsfloodˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        nint N = 5000;
        if (Δruntime.GOOS == "darwin"u8 || Δruntime.GOOS == "ios"u8) {
            // On Darwin this test consumes kernel threads much
            // than other platforms for some reason.
            // When we monitor the number of allocated Ms by
            // observing on runtime.newm calls, we can see that it
            // easily reaches the per process ceiling
            // kern.num_threads when CGO_ENABLED=1 and
            // GODEBUG=netdns=go.
            N = 500;
        }
        time.Duration timeout = /* 3 * time.Second */ 3000000000;
        var (ctxHalfTimeout, cancel) = context.WithTimeout(context.Background(), timeout / 2);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (var ctxTimeout, cancel) = context.WithTimeout(context.Background(), timeout);
        var cancelʗ2 = cancel;
        defer(() => cancelʗ2(), ref ᒐ);
        var c = new channel<error>(2 * N);
        for (nint i = 0; i < N; i++) {
            @string name = fmt.Sprintf("%d.net-test.golang.org"u8, i);
            var cʗ1 = c;
            var ctxHalfTimeoutʗ1 = ctxHalfTimeout;
            goǃ(() => {
                var (_, err) = DefaultResolver.LookupIPAddr(ctxHalfTimeoutʗ1, name);
                cʗ1.ᐸꟷ(err);
            });
            var cʗ2 = c;
            var ctxTimeoutʗ1 = ctxTimeout;
            goǃ(() => {
                var (_, err) = DefaultResolver.LookupIPAddr(ctxTimeoutʗ1, name);
                cʗ2.ᐸꟷ(err);
            });
        }
        var qstats = new TestDNSFlood_qstats();
        var deadline = time.After((time.Duration)(4000000000L));
        for (nint i = 0; i < 2 * N; i++) {
            var selᴛ4 = deadline;
            var selᴛ5 = c;
            switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
            case 0 when selᴛ4.ꟷᐳ(out _): {
                Ꮡt.Fatal(deadlineExceededˢ);
                break;
            }
            case 1 when selᴛ5.ꟷᐳ(out var err): {
                switch (err.type()) {
                case null: {
                    qstats.succeeded++;
                    break;
                }
                case {} ΔerrΔ1 when ΔerrΔ1._<ΔError>(out var errΔ1): {
                    qstats.failed++;
                    if (errΔ1.Timeout()) {
                        qstats.timeout++;
                    }
                    if (errΔ1.Temporary()) {
                        qstats.temporary++;
                    }
                    if (!errΔ1.Timeout() && !errΔ1.Temporary()) {
                        qstats.other++;
                    }
                    break;
                }
                default: {
                    var errΔ1 = err;
                    qstats.failed++;
                    qstats.unknown++;
                    break;
                }}
                break;
            }}
        }
        // A high volume of DNS queries for sub-domain of golang.org
        // would be coordinated by authoritative or recursive server,
        // or stub resolver which implements query-response rate
        // limitation, so we can expect some query successes and more
        // failures including timeout, temporary and other here.
        // As a rule, unknown must not be shown but it might possibly
        // happen due to issue 4856 for now.
        Ꮡt.Logf("%v succeeded, %v failed (%v timeout, %v temporary, %v other, %v unknown)"u8, qstats.succeeded, qstats.failed, qstats.timeout, qstats.temporary, qstats.other, qstats.unknown);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netgoˢ = "netgo"u8;
internal static readonly @string netcgoˢ = "netcgo"u8;

public static void TestLookupDotsWithLocalSource(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!supportsIPv4() || !testIPv4.Value) {
            Ꮡt.Skip(iPv4IsRequiredˢ);
        }
        mustHaveExternalNetwork(Ꮡt);
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (i, fn) in new Func<Action>[]{forceGoDNS, forceCgoDNS}.slice()) {
            var fixup = fn();
            if (fixup == default!) {
                continue;
            }
            var (names, err) = LookupAddr("127.0.0.1"u8);
            fixup();
            if (err != default!) {
                Ꮡt.Logf("#%d: %v"u8, i, err);
                continue;
            }
            @string mode = netgoˢ;
            if (i == 1) {
                mode = netcgoˢ;
            }
loop:
            foreach (var (iΔ1, name) in names) {
                if (strings.Index(name, "."u8) == len(name) - 1){
                    // "localhost" not "localhost."
                    foreach (var (j, _) in names) {
                        if (j == iΔ1) {
                            continue;
                        }
                        if (names[j] == name.slice(0, len(name) - 1)) {
                            // It's OK if we find the name without the dot,
                            // as some systems say 127.0.0.1 localhost localhost.
                            goto continue_loop;
                        }
                    }
                    Ꮡt.Errorf("%s: got %s; want %s"u8, mode, name, name.slice(0, len(name) - 1));
                } else 
                if (strings.Contains(name, "."u8) && !strings.HasSuffix(name, "."u8)) {
                    // "localhost.localdomain." not "localhost.localdomain"
                    Ꮡt.Errorf("%s: got %s; want name ending with trailing dot"u8, mode, name);
                }
continue_loop:;
            }
break_loop:;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgoˢ = "cgo"u8;

public static void TestLookupDotsWithRemoteSource(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "darwin"u8 || Δruntime.GOOS == "ios"u8) {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 27992);
        }
        mustHaveExternalNetwork(Ꮡt);
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv4() || !testIPv4.Value) {
            Ꮡt.Skip(iPv4IsRequiredˢ);
        }
        if (Δruntime.GOOS == "ios"u8) {
            Ꮡt.Skip(noResolvConfOnIOSˢ);
        }
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        {
            var fixup = forceGoDNS(); if (fixup != default!) {
                testDots(Ꮡt, "go"u8);
                fixup();
            }
        }
        {
            var fixup = forceCgoDNS(); if (fixup != default!) {
                testDots(Ꮡt, cgoˢ);
                fixup();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wwwMitEduˢ = "www.mit.edu"u8;
internal static readonly @string googleComˢ2 = "google.com"u8;
internal static readonly @string ldapˢ = "ldap"u8;

internal static void testDots(ж<testing.T> Ꮡt, @string mode) {
    var (names, err) = LookupAddr("8.8.8.8"u8); // Google dns server
    if (err != default!){
        Ꮡt.Errorf("LookupAddr(8.8.8.8): %v (mode=%v)"u8, err, mode);
    } else {
        foreach (var (_, name) in names) {
            if (!hasSuffixFold(name, googleComˢ) && !hasSuffixFold(name, googleˢ)) {
                Ꮡt.Errorf("LookupAddr(8.8.8.8) = %v, want names ending in .google.com or .google with trailing dot (mode=%v)"u8, names, mode);
                break;
            }
        }
    }
    (var cname, err) = LookupCNAME(wwwMitEduˢ);
    if (err != default!){
        Ꮡt.Errorf("LookupCNAME(www.mit.edu, mode=%v): %v"u8, mode, err);
    } else 
    if (!strings.HasSuffix(cname, "."u8)) {
        Ꮡt.Errorf("LookupCNAME(www.mit.edu) = %v, want cname ending in . with trailing dot (mode=%v)"u8, cname, mode);
    }
    (var mxs, err) = LookupMX(googleComˢ2);
    if (err != default!){
        Ꮡt.Errorf("LookupMX(google.com): %v (mode=%v)"u8, err, mode);
    } else {
        foreach (var (_, mx) in mxs) {
            if (!hasSuffixFold((~mx).Host, googleComˢ)) {
                Ꮡt.Errorf("LookupMX(google.com) = %v, want names ending in .google.com. with trailing dot (mode=%v)"u8, mxString(mxs), mode);
                break;
            }
        }
    }
    (var nss, err) = LookupNS(googleComˢ2);
    if (err != default!){
        Ꮡt.Errorf("LookupNS(google.com): %v (mode=%v)"u8, err, mode);
    } else {
        foreach (var (_, ns) in nss) {
            if (!hasSuffixFold((~ns).Host, googleComˢ)) {
                Ꮡt.Errorf("LookupNS(google.com) = %v, want names ending in .google.com. with trailing dot (mode=%v)"u8, nsString(nss), mode);
                break;
            }
        }
    }
    (cname, var srvs, err) = LookupSRV(ldapˢ, tcpˢ, googleComˢ2);
    if (err != default!){
        Ꮡt.Errorf("LookupSRV(ldap, tcp, google.com): %v (mode=%v)"u8, err, mode);
    } else {
        if (!hasSuffixFold(cname, googleComˢ)) {
            Ꮡt.Errorf("LookupSRV(ldap, tcp, google.com) returned cname=%v, want name ending in .google.com. with trailing dot (mode=%v)"u8, cname, mode);
        }
        foreach (var (_, srv) in srvs) {
            if (!hasSuffixFold((~srv).Target, googleComˢ)) {
                Ꮡt.Errorf("LookupSRV(ldap, tcp, google.com) returned addrs=%v, want names ending in .google.com. with trailing dot (mode=%v)"u8, srvString(srvs), mode);
                break;
            }
        }
    }
}

internal static @string mxString(slice<ж<global::go.net_package.MX>> mxs) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    @string sep = ""u8;
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "["u8);
    foreach (var (_, mx) in mxs) {
        fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "%s%s:%d"u8, sep, (~mx).Host, (~mx).Pref);
        sep = " "u8;
    }
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "]"u8);
    return buf.String();
}

internal static @string nsString(slice<ж<global::go.net_package.NS>> nss) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    @string sep = ""u8;
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "["u8);
    foreach (var (_, ns) in nss) {
        fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "%s%s"u8, sep, (~ns).Host);
        sep = " "u8;
    }
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "]"u8);
    return buf.String();
}

internal static @string srvString(slice<ж<global::go.net_package.SRV>> srvs) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    @string sep = ""u8;
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "["u8);
    foreach (var (_, srv) in srvs) {
        fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "%s%s:%d:%d:%d"u8, sep, (~srv).Target, (~srv).Port, (~srv).Priority, (~srv).Weight);
        sep = " "u8;
    }
    fmt.Fprintf(new net_test_package.strings_BuilderжWriter(Ꮡbuf), "]"u8);
    return buf.String();
}

// See https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml
//
// Please be careful about adding new test cases.
// There are platforms which have incomplete mappings for
// restricted resource access and security reasons.
internal partial struct TestLookupPort_test /*dyn*/ {
    internal @string network;
    internal @string name;
    internal nint port;
    internal bool ok;
}

public static void TestLookupPort(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// Issue 13610: LookupPort("tcp", "")
    slice<TestLookupPort_test> tests = new TestLookupPort_test[]{
        new("tcp"u8, "0"u8, 0, true),
        new("udp"u8, "0"u8, 0, true),
        new("udp"u8, "domain"u8, 53, true),
        new("--badnet--"u8, "zzz"u8, 0, false),
        new("tcp"u8, "--badport--"u8, 0, false),
        new("tcp"u8, "-1"u8, 0, false),
        new("tcp"u8, "65536"u8, 0, false),
        new("udp"u8, "-1"u8, 0, false),
        new("udp"u8, "65536"u8, 0, false),
        new("tcp"u8, "123456789"u8, 0, false),
        new("tcp"u8, ""u8, 0, true),
        new("tcp4"u8, ""u8, 0, true),
        new("tcp6"u8, ""u8, 0, true),
        new("udp"u8, ""u8, 0, true),
        new("udp4"u8, ""u8, 0, true),
        new("udp6"u8, ""u8, 0, true)
    }.slice();
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8) {
        if (netGoBuildTag) {
            Ꮡt.Skipf("not supported on %s without cgo; see golang.org/issues/14576"u8, Δruntime.GOOS);
        }
    }
    else { /* default: */
        tests = append(tests, new TestLookupPort_test("tcp"u8, "http"u8, 80, true));
    }

    foreach (var (_, tt) in tests) {
        var (port, err) = LookupPort(tt.network, tt.name);
        if (port != tt.port || (err == default!) != tt.ok) {
            Ꮡt.Errorf("LookupPort(%q, %q) = %d, %v; want %d, error=%t"u8, tt.network, tt.name, port, err, tt.port, !tt.ok);
        }
        if (err != default!) {
            {
                var perr = parseLookupPortError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
        }
    }
}

internal partial struct TestLookupPort_Minimal_test /*dyn*/ {
    internal @string network;
    internal @string name;
    internal nint port;
}

// Like TestLookupPort but with minimal tests that should always pass
// because the answers are baked-in to the net package.
public static void TestLookupPort_Minimal(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// case shouldn't matter
    slice<TestLookupPort_Minimal_test> tests = new TestLookupPort_Minimal_test[]{
        new("tcp"u8, "http"u8, 80),
        new("tcp"u8, "HTTP"u8, 80),
        new("tcp"u8, "https"u8, 443),
        new("tcp"u8, "ssh"u8, 22),
        new("tcp"u8, "gopher"u8, 70),
        new("tcp4"u8, "http"u8, 80),
        new("tcp6"u8, "http"u8, 80)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (port, err) = LookupPort(tt.network, tt.name);
        if (port != tt.port || err != default!) {
            Ꮡt.Errorf("LookupPort(%q, %q) = %d, %v; want %d, error=nil"u8, tt.network, tt.name, port, err, tt.port);
        }
    }
}

internal partial struct TestLookupProtocol_Minimal_test /*dyn*/ {
    internal @string name;
    internal nint want;
}

public static void TestLookupProtocol_Minimal(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// case shouldn't matter
    slice<TestLookupProtocol_Minimal_test> tests = new TestLookupProtocol_Minimal_test[]{
        new("tcp"u8, 6),
        new("TcP"u8, 6),
        new("icmp"u8, 1),
        new("igmp"u8, 2),
        new("udp"u8, 17),
        new("ipv6-icmp"u8, 58)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (got, err) = lookupProtocol(context.Background(), tt.name);
        if (got != tt.want || err != default!) {
            Ꮡt.Errorf("LookupProtocol(%q) = %d, %v; want %d, error=nil"u8, tt.name, got, err, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bogusDomainˢ = "!!!.###.bogus..domain."u8;

public static void TestLookupNonLDH(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        {
            var fixup = forceGoDNS(); if (fixup != default!) {
                var fixupʗ1 = fixup;
                defer(fixupʗ1, ref ᒐ);
            }
        }
        // "LDH" stands for letters, digits, and hyphens and is the usual
        // description of standard DNS names.
        // This test is checking that other kinds of names are reported
        // as not found, not reported as invalid names.
        var (addrs, err) = LookupHost(bogusDomainˢ);
        if (err == default!) {
            Ꮡt.Fatalf("lookup succeeded: %v"u8, addrs);
        }
        if (!strings.HasSuffix(err.Error(), errNoSuchHost.Error())) {
            Ꮡt.Fatalf("lookup error = %v, want %v"u8, err, errNoSuchHost.OrTypedNil());
        }
        if (!(~err._<ж<global::go.net_package.DNSError>>()).IsNotFound) {
            Ꮡt.Fatalf("lookup error = %v, want true"u8, (~err._<ж<global::go.net_package.DNSError>>()).IsNotFound);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestLookupContextCancel(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        mustHaveExternalNetwork(Ꮡt);
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            ᏑdnsWaitGroup.Wait();
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        var (lookupCtx, cancelLookup) = context.WithCancel(context.Background());
        var unblockLookup = new channel<EmptyStruct>(0);
        // Set testHookLookupIP to start a new, concurrent call to LookupIPAddr
        // and cancel the original one, then block until the canceled call has returned
        // (ensuring that it has performed any synchronous cleanup).
        var cancelLookupʗ1 = cancelLookup;
        var unblockLookupʗ1 = unblockLookup;
        testHookLookupIP = [MethodImpl(MethodImplOptions.NoInlining)] (slice<global::go.net_package.IPAddr>, error) (context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => {
            var selᴛ6 = unblockLookupʗ1;
            switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0 when selᴛ6.ꟷᐳ(out _): {
                break;
            }
            default: {
                Ꮡt.Logf("starting concurrent LookupIPAddr"u8);
                ᏑdnsWaitGroup.Add(1);
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        // Start a concurrent LookupIPAddr for the same host while the caller is
                        // still blocked, and sleep a little to give it time to be deduplicated
                        // before we cancel (and unblock) the caller.
                        // (If the timing doesn't quite work out, we'll end up testing sequential
                        // calls instead of concurrent ones, but the test should still pass.)
                        defer(ᏑdnsWaitGroup.Done, ref ᒐ);
                        var (_, errΔ1) = DefaultResolver.LookupIPAddr(context.Background(), host);
                        if (errΔ1 != default!) {
                            Ꮡt.Error(errΔ1);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                time.Sleep(1 * time.Millisecond);
                break;
            }}
            cancelLookupʗ1();
            ᐸꟷ(unblockLookupʗ1);
            // If the concurrent lookup above is deduplicated to this one
            // (as we expect to happen most of the time), it is important
            // that the original call does not cancel the shared Context.
            // (See https://go.dev/issue/22724.) Explicitly check for
            // cancellation now, just in case fn itself doesn't notice it.
            {
                var errΔ2 = ctx.Err(); if (errΔ2 != default!) {
                    Ꮡt.Logf("testHookLookupIP canceled"u8);
                    return (default!, errΔ2);
                }
            }
            Ꮡt.Logf("testHookLookupIP performing lookup"u8);
            return fn(ctx, network, host);
        };
        var (_, err) = DefaultResolver.LookupIPAddr(lookupCtx, googleComˢ2);
        {
            var (dnsErr, ok) = err._<ж<global::go.net_package.DNSError>>(ᐧ); if (!ok || (~dnsErr).Err != errCanceled.Error()) {
                Ꮡt.Errorf("unexpected error from canceled, blocked LookupIPAddr: %v"u8, err);
            }
        }
        builtin.close(unblockLookup);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gmailComˢ = "gmail.com"u8;
internal static readonly @string smtpˢ = "smtp"u8;
internal static readonly @string serviceˢ = "service"u8;
internal static readonly @string protoˢ = "proto"u8;
internal static readonly @string nameˢ = "name"u8;

// Issue 24330: treat the nil *Resolver like a zero value. Verify nothing
// crashes if nil is used.
public static void TestNilResolverLookup(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    mustHaveExternalNetwork(Ꮡt);
    ж<global::go.net_package.Resolver> r = default!;
    var ctx = context.Background();
    // Don't care about the results, just that nothing panics:
    r.LookupAddr(ctx, "8.8.8.8"u8);
    r.LookupCNAME(ctx, googleComˢ2);
    r.LookupHost(ctx, googleComˢ2);
    r.LookupIPAddr(ctx, googleComˢ2);
    r.LookupIP(ctx, "ip"u8, googleComˢ2);
    r.LookupMX(ctx, gmailComˢ);
    r.LookupNS(ctx, googleComˢ2);
    r.LookupPort(ctx, tcpˢ, smtpˢ);
    r.LookupSRV(ctx, serviceˢ, protoˢ, nameˢ);
    r.LookupTXT(ctx, gmailComˢ);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string googleᶜ = "www.google.com"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string invalidDomainᶜ = "invalid.invalid"u8;

// TestLookupHostCancel verifies that lookup works even after many
// canceled lookups (see golang.org/issue/24178 for details).
public static void TestLookupHostCancel(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    mustHaveExternalNetwork(Ꮡt);
    testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel(); // Executes 600ms worth of sequential sleeps.
    @string google = googleᶜ;
    @string invalidDomain = invalidDomainᶜ; // RFC 2606 reserves .invalid
    const nint n = 600;      // this needs to be larger than threadLimit size
    var (_, err) = LookupHost(google);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (ctx, cancel) = context.WithCancel(context.Background());
    cancel();
    for (nint i = 0; i < n; i++) {
        var (addr, errΔ1) = DefaultResolver.LookupHost(ctx, invalidDomain);
        if (errΔ1 == default!) {
            Ꮡt.Fatalf("LookupHost(%q): returns %v, but should fail"u8, invalidDomain, addr);
        }
        // Don't verify what the actual error is.
        // We know that it must be non-nil because the domain is invalid,
        // but we don't have any guarantee that LookupHost actually bothers
        // to check for cancellation on the fast path.
        // (For example, it could use a local cache to avoid blocking entirely.)
        // The lookup may deduplicate in-flight requests, so give it time to settle
        // in between.
        time.Sleep(time.Millisecond * 1);
    }
    (_, err) = LookupHost(google);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal partial struct lookupCustomResolver {
    public partial ref ж<global::go.net_package.Resolver> Resolver { get; }
    internal Δsync.RWMutex mu;
    internal bool dialed;
}

internal static Func<context.Context, @string, @string, (global::go.net_package.Conn, error)> dial(this ж<lookupCustomResolver> Ꮡlcr) {
    return (context.Context ctx, @string network, @string address) => {
        Ꮡlcr.of(lookupCustomResolver.Ꮡmu).Lock();
        Ꮡlcr.Value.dialed = true;
        Ꮡlcr.of(lookupCustomResolver.Ꮡmu).Unlock();
        return Dial(network, address);
    };
}

// TestConcurrentPreferGoResolversDial tests that multiple resolvers with the
// PreferGo option used concurrently are all dialed properly.
public static partial void TestConcurrentPreferGoResolversDial(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("skip on %v"u8, // TODO: plan9 implementation of the resolver uses the Dial function since
 // https://go.dev/cl/409234, this test could probably be reenabled.
 Δruntime.GOOS);
        }

        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        var resolvers = new slice<ж<lookupCustomResolver>>(2);
        foreach (var (i, _) in resolvers) {
            ref var cs = ref heap<lookupCustomResolver>(out var Ꮡcs);
            cs = new lookupCustomResolver(Resolver: Ꮡ(new Resolver(PreferGo: true)));
            cs.Dial = Ꮡcs.dial();
            resolvers[i] = Ꮡcs;
        }
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(len(resolvers));
        foreach (var (i, resolver) in resolvers) {
            goǃ((ж<global::go.net_package.Resolver> r, nint index) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (_, err) = r.LookupIPAddr(context.Background(), googleComˢ2);
                    if (err != default!) {
                        Ꮡt.Errorf("lookup failed for resolver %d: %q"u8, index, err);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, (~resolver).Resolver, i);
        }
        Ꮡwg.Wait();
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        foreach (var (i, resolver) in resolvers) {
            if (!(~resolver).dialed) {
                Ꮡt.Errorf("custom resolver %d not dialed during lookup"u8, i);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


partial struct ipVersionTestsᴛ1 /*dyn*/ {
    internal @string network;
    internal byte version;
}
internal static slice<ipVersionTestsᴛ1> ipVersionTests = new ipVersionTestsᴛ1[]{
    new("tcp"u8, 0),
    new("tcp4"u8, (rune)'4'),
    new("tcp6"u8, (rune)'6'),
    new("udp"u8, 0),
    new("udp4"u8, (rune)'4'),
    new("udp6"u8, (rune)'6'),
    new("ip"u8, 0),
    new("ip4"u8, (rune)'4'),
    new("ip6"u8, (rune)'6'),
    new("ip7"u8, 0),
    new(""u8, 0)
}.slice();

public static void TestIPVersion(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipVersionTests) {
        {
            var version = ipVersion(tt.network); if (version != tt.version) {
                Ꮡt.Errorf("Family for: %s. Expected: %s, Got: %s"u8, tt.network,
                    ((@string)tt.version), ((@string)version));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string golangOrgˢ = "golang.org"u8;

internal partial struct TestLookupIPAddrPreservesContextValues_keyValues /*dyn*/ {
    internal any key, value;
}

// Issue 28600: The context that is used to lookup ips should always
// preserve the values from the context that was passed into LookupIPAddr.
public static void TestLookupIPAddrPreservesContextValues(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        var keyValues = new TestLookupIPAddrPreservesContextValues_keyValues[]{
            new((@string)"key-1"u8, (nint)(12)),
            new((nint)(384), (@string)"value2"u8),
            new(@new<float64>(), (nint)(137))
        }.slice();
        var ctx = context.Background();
        foreach (var (_, kv) in keyValues) {
            ctx = context.WithValue(ctx, kv.key, kv.value);
        }
        var wantIPs = new global::go.net_package.IPAddr[]{
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback)
        }.slice();
        var keyValuesʗ1 = keyValues;
        var wantIPsʗ1 = wantIPs;
        var checkCtxValues = (slice<global::go.net_package.IPAddr>, error) (context.Context ctx_, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => {
            foreach (var (_, kv) in keyValuesʗ1) {
                var (g, w) = (ctx_.Value(kv.key), kv.value);
                if (!reflect.DeepEqual(g, w)) {
                    Ꮡt.Errorf("Value lookup:\n\tGot:  %v\n\tWant: %v"u8, g, w);
                }
            }
            return (wantIPsʗ1, default!);
        };
        testHookLookupIP = checkCtxValues;
        var resolvers = new ж<global::go.net_package.Resolver>[]{
            default!,
            @new<global::go.net_package.Resolver>()
        }.slice();
        foreach (var (i, resolver) in resolvers) {
            var (gotIPs, err) = resolver.LookupIPAddr(ctx, golangOrgˢ);
            if (err != default!) {
                Ꮡt.Errorf("Resolver #%d: unexpected error: %v"u8, i, err);
            }
            if (!reflect.DeepEqual(gotIPs, wantIPs)) {
                Ꮡt.Errorf("#%d: mismatched IPAddr results\n\tGot: %v\n\tWant: %v"u8, i, gotIPs, wantIPs);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 30521: The lookup group should call the resolver for each network.
public static partial void TestLookupIPAddrConcurrentCallsForNetworks(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        var queries = new slice<@string>[]{
            new @string[]{"udp"u8, "golang.org"u8}.slice(),
            new @string[]{"udp4"u8, "golang.org"u8}.slice(),
            new @string[]{"udp6"u8, "golang.org"u8}.slice(),
            new @string[]{"udp"u8, "golang.org"u8}.slice(),
            new @string[]{"udp"u8, "golang.org"u8}.slice()
        }.slice();
        var results = new map<array<@string>, slice<global::go.net_package.IPAddr>>{
            [new @string[]{"udp"u8, "golang.org"u8}.array()] = new global::go.net_package.IPAddr[]{
                new(IP: IPv4(127, 0, 0, 1)),
                new(IP: IPv6loopback)}.slice(),
            [new @string[]{"udp4"u8, "golang.org"u8}.array()] = new global::go.net_package.IPAddr[]{
                new(IP: IPv4(127, 0, 0, 1))}.slice(),
            [new @string[]{"udp6"u8, "golang.org"u8}.array()] = new global::go.net_package.IPAddr[]{
                new(IP: IPv6loopback)}.slice()
        };
        ref var calls = ref heap<int32>(out var Ꮡcalls);
        calls = (int32)0;
        var waitCh = new channel<EmptyStruct>(0);
        var resultsʗ1 = results;
        var waitChʗ1 = waitCh;
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context ctxΔ1, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => {
            // We'll block until this is called one time for each different
            // expected result. This will ensure that the lookup group would wait
            // for the existing call if it was to be reused.
            if (atomic.AddInt32(Ꮡcalls, 1) == (int32)len(resultsʗ1)) {
                builtin.close(waitChʗ1);
            }
            var selᴛ7 = waitChʗ1;
            var selᴛ8 = ctxΔ1.Done();
            switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out _): {
                return (default!, ctxΔ1.Err());
            }}
            return (resultsʗ1[new @string[]{network, host}.array()], default!);
        };
        var (ctx, cancel) = context.WithTimeout(context.Background(), (time.Duration)(10000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ref var wg = ref heap<Δsync.WaitGroup>(out var Ꮡwg);
        wg = new Δsync.WaitGroup(nil);
        foreach (var (_, q) in queries) {
            @string network = q[0];
            @string host = q[1];
            Ꮡwg.Add(1);
            var ctxʗ1 = ctx;
            var resultsʗ2 = results;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (gotIPs, err) = DefaultResolver.lookupIPAddr(ctxʗ1, network, host);
                    if (err != default!) {
                        Ꮡt.Errorf("lookupIPAddr(%v, %v): unexpected error: %v"u8, network, host, err);
                    }
                    var wantIPs = resultsʗ2[new @string[]{network, host}.array()];
                    if (!reflect.DeepEqual(gotIPs, wantIPs)) {
                        Ꮡt.Errorf("lookupIPAddr(%v, %v): mismatched IPAddr results\n\tGot: %v\n\tWant: %v"u8, network, host, gotIPs, wantIPs);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object defaultResolverLookupIPˢ = (@string)"DefaultResolver.LookupIP for empty host success, want no host error"u8;

// Issue 53995: Resolver.LookupIP should return error for empty host name.
public static void TestResolverLookupIPWithEmptyHost(ж<testing.T> Ꮡt) {
    var (_, err) = DefaultResolver.LookupIP(context.Background(), "ip"u8, ""u8);
    if (err == default!) {
        Ꮡt.Fatal(defaultResolverLookupIPˢ);
    }
    if (!strings.HasSuffix(err.Error(), errNoSuchHost.Error())) {
        Ꮡt.Fatalf("lookup error = %v, want %v"u8, err, errNoSuchHost.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string key1ˢ = "key-1"u8;

public static void TestWithUnexpiredValuesPreserved(ж<testing.T> Ꮡt) {
    var (ctx, cancel) = context.WithCancel(context.Background());
    // Insert a value into it.
    @string key = key1ˢ;
    nint value = 2;
    ctx = context.WithValue(ctx, key, value);
    // Now use the "values preserving context" like
    // we would for LookupIPAddr. See Issue 28600.
    ctx = withUnexpiredValuesPreserved(ctx);
    // Lookup before expiry.
    {
        var g = ctx.Value(key);
        nint w = value; if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Lookup before expiry: Got %v Want %v"u8, g, w);
        }
    }
    // Cancel the context.
    cancel();
    // Lookup after expiry should return nil
    {
        var g = ctx.Value(key); if (g != default!) {
            Ꮡt.Errorf("Lookup after expiry: Got %v want nil"u8, g);
        }
    }
}

// Issue 31597: don't panic on null byte in name
public static void TestLookupNullByte(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
    LookupHost(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72}))); // check that it doesn't panic; it used to on Windows
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notSupportedˢ = (@string)"not supported"u8;

internal partial struct TestResolverLookupIP_type /*dyn*/ {
    internal @string name;
    internal Func<Action> fn;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hostᶜ = "google.com"u8;

public static void TestResolverLookupIP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        var v4Ok = supportsIPv4() && testIPv4.Value;
        var v6Ok = supportsIPv6() && testIPv6.Value;
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        foreach (var (_, vᴛ1) in new TestResolverLookupIP_type[]{
            new("go"u8, forceGoDNS),
            new("cgo"u8, forceCgoDNS)
        }.slice()) {
            ref var impl = ref heap(new TestResolverLookupIP_type(), out var Ꮡimpl);
            impl = vᴛ1;

            var implʗ1 = impl;
            Ꮡt.Run("implementation: "u8 + impl.name, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var fixup = implʗ1.fn();
                    if (fixup == default!) {
                        tΔ1.Skip(notSupportedˢ);
                    }
                    var fixupʗ1 = fixup;
                    defer(fixupʗ1, ref ᒐ);
                    foreach (var (_, network) in new @string[]{"ip"u8, "ip4"u8, "ip6"u8}.slice()) {
                        tΔ1.Run("network: "u8 + network, (ж<testing.T> tΔ2) => {
                            switch (ᐧ) {
                            case {} when network == "ip4"u8 && !v4Ok: {
                                tΔ2.Skip(iPv4IsNotSupportedˢ);
                                break;
                            }
                            case {} when network == "ip6"u8 && !v6Ok: {
                                tΔ2.Skip(iPv6IsNotSupportedˢ);
                                break;
                            }}

                            // google.com has both A and AAAA records.
                            @string host = hostᶜ;
                            var (ips, err) = DefaultResolver.LookupIP(context.Background(), network, host);
                            if (err != default!) {
                                testenv.SkipFlakyNet(new net_test_package.testing_TжTB(tΔ2));
                                tΔ2.Fatalf("DefaultResolver.LookupIP(%q, %q): failed with unexpected error: %v"u8, network, host, err);
                            }
                            slice<netipꓸAddr> v4Addrs = default!;
                            slice<netipꓸAddr> v6Addrs = default!;
                            foreach (var (_, ip) in ips) {
                                {
                                    var (addr, ok) = netip.AddrFromSlice(ip); if (ok){
                                        if (addr.Is4()){
                                            v4Addrs = append(v4Addrs, addr);
                                        } else {
                                            v6Addrs = append(v6Addrs, addr);
                                        }
                                    } else {
                                        tΔ2.Fatalf("IP=%q is neither IPv4 nor IPv6"u8, ip);
                                    }
                                }
                            }
                            // Check that we got the expected addresses.
                            if (network == "ip4"u8 || network == "ip"u8 && v4Ok) {
                                if (len(v4Addrs) == 0) {
                                    tΔ2.Errorf("DefaultResolver.LookupIP(%q, %q): no IPv4 addresses"u8, network, host);
                                }
                            }
                            if (network == "ip6"u8 || network == "ip"u8 && v6Ok) {
                                if (len(v6Addrs) == 0) {
                                    tΔ2.Errorf("DefaultResolver.LookupIP(%q, %q): no IPv6 addresses"u8, network, host);
                                }
                            }
                            // Check that we didn't get any unexpected addresses.
                            if (network == "ip6"u8 && len(v4Addrs) > 0) {
                                tΔ2.Errorf("DefaultResolver.LookupIP(%q, %q): unexpected IPv4 addresses: %v"u8, network, host, v4Addrs);
                            }
                            if (network == "ip4"u8 && len(v6Addrs) > 0) {
                                tΔ2.Errorf("DefaultResolver.LookupIP(%q, %q): unexpected IPv6 or IPv4-mapped IPv6 addresses: %v"u8, network, host, v6Addrs);
                            }
                        });
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorˢ = (@string)"expected an error"u8;
internal static readonly @string golang1Orgˢ = "golang1.org"u8;
internal static readonly @string golang2Orgˢ = "golang2.org"u8;

// A context timeout should still return a DNSError.
public static partial void TestDNSTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        defer(ᏑdnsWaitGroup.Wait, ref ᒐ);
        ref var timeoutHookGo = ref heap<channel<bool>>(out var ᏑtimeoutHookGo);
        timeoutHookGo = new channel<bool>(1);
        var timeoutHook = (slice<global::go.net_package.IPAddr>, error) (context.Context ctxΔ1, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => {
            ᐸꟷ(ᏑtimeoutHookGo.ValueSlot);
            return (default!, context.DeadlineExceeded);
        };
        testHookLookupIP = timeoutHook;
        void checkErr(error errΔ1) {
            Ꮡt.Helper();
            if (errΔ1 == default!){
                Ꮡt.Error(expectedAnErrorˢ);
            } else 
            {
                var (dnserr, ok) = errΔ1._<ж<global::go.net_package.DNSError>>(ᐧ); if (!ok){
                    Ꮡt.Errorf("got error type %T, want %T"u8, errΔ1, ((ж<global::go.net_package.DNSError>)nil));
                } else 
                if (!(~dnserr).IsTimeout){
                    Ꮡt.Errorf("got error %#v, want IsTimeout == true"u8, dnserr.OrTypedNil());
                } else 
                {
                    var isTimeout = dnserr.Timeout(); if (!isTimeout) {
                        Ꮡt.Errorf("got err.Timeout() == %t, want true"u8, isTimeout);
                    }
                }
            }
        }
        // Single lookup.
        timeoutHookGo.ᐸꟷ(true);
        var (_, err) = LookupIP(golangOrgˢ);
        checkErr(err);
        // Double lookup.
        ref var err1 = ref heap<error>(out var Ꮡerr1);
        ref var err2 = ref heap<error>(out var Ꮡerr2);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                (_, Ꮡerr1.ValueSlot) = LookupIP(golang1Orgˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                (_, Ꮡerr2.ValueSlot) = LookupIP(golang1Orgˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        builtin.close(timeoutHookGo);
        Ꮡwg.Wait();
        checkErr(err1);
        checkErr(err2);
        // Double lookup with context.
        timeoutHookGo = new channel<bool>(0);
        var (ctx, cancel) = context.WithTimeout(context.Background(), time.ΔNanosecond);
        Ꮡwg.Add(2);
        var ctxʗ1 = ctx;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                (_, Ꮡerr1.ValueSlot) = DefaultResolver.LookupIPAddr(ctxʗ1, golang2Orgˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var ctxʗ2 = ctx;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                (_, Ꮡerr2.ValueSlot) = DefaultResolver.LookupIPAddr(ctxʗ2, golang2Orgˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        time.Sleep(10 * time.ΔNanosecond);
        builtin.close(timeoutHookGo);
        Ꮡwg.Wait();
        checkErr(err1);
        checkErr(err2);
        cancel();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notSupportedOnPlan9ˢ = (@string)"not supported on plan9"u8;
internal static readonly @string defaultResolverˢ = "default resolver"u8;
internal static readonly @string forcedGoResolverˢ = "forced go resolver"u8;
internal static readonly @string forcedCgoResolverˢ = "forced cgo resolver"u8;

public static void TestLookupNoData(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "plan9"u8) {
        Ꮡt.Skip(notSupportedOnPlan9ˢ);
    }
    mustHaveExternalNetwork(Ꮡt);
    testLookupNoData(Ꮡt, defaultResolverˢ);
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(forceGoDNS(), ref ᒐ);
            testLookupNoData(Ꮡt, forcedGoResolverˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(forceCgoDNS(), ref ᒐ);
            testLookupNoData(Ꮡt, forcedCgoResolverˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string golangRscIoˢ2 = "golang.rsc.io."u8;

internal static void testLookupNoData(ж<testing.T> Ꮡt, @string prefix) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint attempts = 0;
    while (ᐧ) {
        // Domain that doesn't have any A/AAAA RRs, but has different one (in this case a TXT),
        // so that it returns an empty response without any error codes (NXDOMAIN).
        var (_, err) = LookupHost(golangRscIoˢ2);
        if (err == default!) {
            Ꮡt.Errorf("%v: unexpected success"u8, prefix);
            return;
        }
        ref var dnsErr = ref heap<ж<global::go.net_package.DNSError>>(out var ᏑdnsErr);
        if (errors.As(err, ᏑdnsErr)) {
            var succeeded = true;
            if (!(~dnsErr).IsNotFound) {
                succeeded = false;
                Ꮡt.Logf("%v: IsNotFound is set to false"u8, prefix);
            }
            if ((~dnsErr).Err != errNoSuchHost.Error()) {
                succeeded = false;
                Ꮡt.Logf("%v: error message is not equal to: %v"u8, prefix, errNoSuchHost.Error());
            }
            if (succeeded) {
                return;
            }
        }
        testenv.SkipFlakyNet(new net_test_package.testing_TжTB(Ꮡt));
        if (attempts < len(backoffDuration)) {
            var dur = backoffDuration[attempts];
            Ꮡt.Logf("%v: backoff %v after failure %v\n"u8, prefix, dur, err);
            time.Sleep(dur);
            attempts++;
            continue;
        }
        Ꮡt.Errorf("%v: unexpected error: %v"u8, prefix, err);
        return;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownServiceˢ = "_-unknown-service-"u8;

public static void TestLookupPortNotFound(ж<testing.T> Ꮡt) {
    allResolvers(Ꮡt, (ж<testing.T> tΔ1) => {
        var (_, err) = LookupPort(udpˢ, unknownServiceˢ);
        ref var dnsErr = ref heap<ж<global::go.net_package.DNSError>>(out var ᏑdnsErr);
        if (!errors.As(err, ᏑdnsErr) || !(~dnsErr).IsNotFound) {
            tΔ1.Fatalf("unexpected error: %v"u8, err);
        }
    });
}

// plan9 does not have submissions service defined in the service database.
// submissions service is only available through a tcp network, see:
// https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml?search=submissions
internal static @string tcpOnlyService = ((Func<@string>)(() => {
    if (Δruntime.GOOS == "plan9"u8) {
        return "https"u8;
    }
    return "submissions"u8;
}))();

public static void TestLookupPortDifferentNetwork(ж<testing.T> Ꮡt) {
    allResolvers(Ꮡt, (ж<testing.T> tΔ1) => {
        var (_, err) = LookupPort(udpˢ, tcpOnlyService);
        ref var dnsErr = ref heap<ж<global::go.net_package.DNSError>>(out var ᏑdnsErr);
        if (!errors.As(err, ᏑdnsErr) || !(~dnsErr).IsNotFound) {
            tΔ1.Fatalf("unexpected error: %v"u8, err);
        }
    });
}

public static void TestLookupPortEmptyNetworkString(ж<testing.T> Ꮡt) {
    allResolvers(Ꮡt, (ж<testing.T> tΔ1) => {
        var (_, err) = LookupPort(""u8, tcpOnlyService);
        if (err != default!) {
            tΔ1.Fatalf("unexpected error: %v"u8, err);
        }
    });
}

public static void TestLookupPortIPNetworkString(ж<testing.T> Ꮡt) {
    allResolvers(Ꮡt, (ж<testing.T> tΔ1) => {
        var (_, err) = LookupPort("ip"u8, tcpOnlyService);
        if (err != default!) {
            tΔ1.Fatalf("unexpected error: %v"u8, err);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownˢ = "unknown"u8;
internal static readonly object isNotFoundIsSetToFalseˢ = (@string)"IsNotFound is set to false"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testNXDOMAINᶜ = "invalid.invalid."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testNODATAᶜ = "_ldap._tcp.google.com."u8;

internal partial struct TestLookupNoSuchHost_tests /*dyn*/ {
    internal @string name;
    internal Func<error> query;
}

public static void TestLookupNoSuchHost(ж<testing.T> Ꮡt) {
    mustHaveExternalNetwork(Ꮡt);
    @string testNXDOMAIN = testNXDOMAINᶜ;
    @string testNODATA = testNODATAᶜ;
    var tests = new TestLookupNoSuchHost_tests[]{
        new(
            name: "LookupCNAME NXDOMAIN"u8,
            query: () => {
                var (_, err) = LookupCNAME(testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupHost NXDOMAIN"u8,
            query: () => {
                var (_, err) = LookupHost(testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupHost NODATA"u8,
            query: () => {
                var (_, err) = LookupHost(testNODATA);
                return err;
            }
        ),
        new(
            name: "LookupMX NXDOMAIN"u8,
            query: () => {
                var (_, err) = LookupMX(testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupMX NODATA"u8,
            query: () => {
                var (_, err) = LookupMX(testNODATA);
                return err;
            }
        ),
        new(
            name: "LookupNS NXDOMAIN"u8,
            query: () => {
                var (_, err) = LookupNS(testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupNS NODATA"u8,
            query: () => {
                var (_, err) = LookupNS(testNODATA);
                return err;
            }
        ),
        new(
            name: "LookupSRV NXDOMAIN"u8,
            query: () => {
                var (_, _, err) = LookupSRV(unknownˢ, tcpˢ, testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupTXT NXDOMAIN"u8,
            query: () => {
                var (_, err) = LookupTXT(testNXDOMAIN);
                return err;
            }
        ),
        new(
            name: "LookupTXT NODATA"u8,
            query: () => {
                var (_, err) = LookupTXT(testNODATA);
                return err;
            }
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var v = ref heap(new TestLookupNoSuchHost_tests(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        Ꮡt.Run(v.name, (ж<testing.T> tΔ1) => {
            var vʗ2 = vʗ1;
            allResolvers(tΔ1, (ж<testing.T> tΔ2) => {
                nint attempts = 0;
                while (ᐧ) {
                    var err = vʗ2.query();
                    if (err == default!) {
                        tΔ2.Errorf("unexpected success"u8);
                        return;
                    }
                    {
                        var (dnsErr, ok) = err._<ж<global::go.net_package.DNSError>>(ᐧ); if (ok) {
                            var succeeded = true;
                            if (!(~dnsErr).IsNotFound) {
                                succeeded = false;
                                tΔ2.Log(isNotFoundIsSetToFalseˢ);
                            }
                            if ((~dnsErr).Err != errNoSuchHost.Error()) {
                                succeeded = false;
                                tΔ2.Logf("error message is not equal to: %v"u8, errNoSuchHost.Error());
                            }
                            if (succeeded) {
                                return;
                            }
                        }
                    }
                    testenv.SkipFlakyNet(new net_test_package.testing_TжTB(tΔ2));
                    if (attempts < len(backoffDuration)) {
                        var dur = backoffDuration[attempts];
                        tΔ2.Logf("backoff %v after failure %v\n"u8, dur, err);
                        time.Sleep(dur);
                        attempts++;
                        continue;
                    }
                    tΔ2.Errorf("unexpected error: %v"u8, err);
                    return;
                }
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnPlan9ˢ = (@string)"skipping on plan9"u8;
internal static readonly @string testGoDevˢ = "test.go.dev"u8;
internal static readonly @string textGoDevˢ = "text.go.dev"u8;

public static void TestDNSErrorUnwrap(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δruntime.GOOS == "plan9"u8) {
        // The Plan 9 implementation of the resolver doesn't use the Dial function yet. See https://go.dev/cl/409234
        Ꮡt.Skip(skippingOnPlan9ˢ);
    }
    var rDeadlineExcceeded = Ꮡ(new Resolver(PreferGo: true, Dial: (context.Context ctxΔ1, @string network, @string address) => (default!, context.DeadlineExceeded)
    ));
    var rCancelled = Ꮡ(new Resolver(PreferGo: true, Dial: (context.Context ctxΔ2, @string network, @string address) => (default!, context.Canceled)
    ));
    var (_, err) = rDeadlineExcceeded.LookupHost(context.Background(), testGoDevˢ);
    if (!errors.Is(err, context.DeadlineExceeded)) {
        Ꮡt.Errorf("errors.Is(err, context.DeadlineExceeded) = false; want = true"u8);
    }
    (_, err) = rCancelled.LookupHost(context.Background(), testGoDevˢ);
    if (!errors.Is(err, context.Canceled)) {
        Ꮡt.Errorf("errors.Is(err, context.Canceled) = false; want = true"u8);
    }
    var (ctx, cancel) = context.WithCancel(context.Background());
    cancel();
    (_, err) = ᏑgoResolver.LookupHost(ctx, textGoDevˢ);
    if (!errors.Is(err, context.Canceled)) {
        Ꮡt.Errorf("errors.Is(err, context.Canceled) = false; want = true"u8);
    }
}

} // end net_internal_test_package
