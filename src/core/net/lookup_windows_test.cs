// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using context = context_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using exec = go.os.exec_package;
using reflect = reflect_package;
using Δregexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using encoding;
using go.os;
using static go.net_package;

partial class net_internal_test_package {

internal static slice<@string> nslookupTestServers = ["mail.golang.com", "gmail.com"];
internal static slice<@string> lookupTestIPs = ["8.8.8.8", "1.1.1.1"];

internal static @string toJson(any v) {
    var (data, _) = json.Marshal(v);
    return (@string)data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default/"u8;

internal static void testLookup(ж<testing.T> t, Action<ж<testing.T>, ж<global::go.net_package.Resolver>, @string> fn) {
    foreach (var (_, def) in range(slice<bool>([true, false]))) {
        foreach (var (_, server) in range(nslookupTestServers)) {
            @string name = default;
            if (def) {
                name = defaultˢ;
            } else {
                name = "go/";
            }
            t.Run(name + server, (ж<testing.T> tΔ1) => {
                tΔ1.Parallel();
                var r = DefaultResolver;
                if (!def) {
                    r = Ꮡ(new Resolver(PreferGo: true));
                }
                fn(tΔ1, r, server);
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noResultsˢ = (@string)"no results"u8;

public static void TestNSLookupMX(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    testLookup(t, (ж<testing.T> tΔ1, ж<global::go.net_package.Resolver> r, @string server) => {
        var (mx, err) = r.LookupMX(context.Background(), server);
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        if (len(mx) == 0) {
            tΔ1.Fatal(noResultsˢ);
        }
        (var expected, err) = nslookupMX(server);
        if (err != nil) {
            tΔ1.Skipf("skipping failed nslookup %s test: %s", server, err);
        }
        var byPrefAndHost = (ж<global::go.net_package.MX> a, ж<global::go.net_package.MX> b) => {
            nint rΔ1 = cmp.Compare(a.Pref, b.Pref); if (rΔ1 != 0) {
                return rΔ1;
            }
            return strings.Compare(a.Host, b.Host);
        };
        slices.SortFunc(expected, byPrefAndHost);
        slices.SortFunc(mx, byPrefAndHost);
        if (!reflect.DeepEqual(expected, mx)) {
            tΔ1.Errorf("different results %s:\texp:%v\tgot:%v", server, toJson(expected), toJson(mx));
        }
    });
}

public static void TestNSLookupCNAME(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    testLookup(t, (ж<testing.T> tΔ1, ж<global::go.net_package.Resolver> r, @string server) => {
        var (cname, err) = r.LookupCNAME(context.Background(), server);
        if (err != nil) {
            tΔ1.Fatalf("failed %s: %s", server, err);
        }
        if (cname == ""u8) {
            tΔ1.Fatalf("no result %s", server);
        }
        (var expected, err) = nslookupCNAME(server);
        if (err != nil) {
            tΔ1.Skipf("skipping failed nslookup %s test: %s", server, err);
        }
        if (expected != cname) {
            tΔ1.Errorf("different results %s:\texp:%v\tgot:%v", server, expected, cname);
        }
    });
}

public static void TestNSLookupNS(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    testLookup(t, (ж<testing.T> tΔ1, ж<global::go.net_package.Resolver> r, @string server) => {
        var (ns, err) = r.LookupNS(context.Background(), server);
        if (err != nil) {
            tΔ1.Fatalf("failed %s: %s", server, err);
        }
        if (len(ns) == 0) {
            tΔ1.Fatal(noResultsˢ);
        }
        (var expected, err) = nslookupNS(server);
        if (err != nil) {
            tΔ1.Skipf("skipping failed nslookup %s test: %s", server, err);
        }
        var byHost = (ж<global::go.net_package.NS> a, ж<global::go.net_package.NS> b) => strings.Compare(a.Host, b.Host);
        slices.SortFunc(expected, byHost);
        slices.SortFunc(ns, byHost);
        if (!reflect.DeepEqual(expected, ns)) {
            tΔ1.Errorf("different results %s:\texp:%v\tgot:%v", toJson(server), toJson(expected), ns);
        }
    });
}

public static void TestNSLookupTXT(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    testLookup(t, (ж<testing.T> tΔ1, ж<global::go.net_package.Resolver> r, @string server) => {
        var (txt, err) = r.LookupTXT(context.Background(), server);
        if (err != nil) {
            tΔ1.Fatalf("failed %s: %s", server, err);
        }
        if (len(txt) == 0) {
            tΔ1.Fatalf("no results");
        }
        (var expected, err) = nslookupTXT(server);
        if (err != nil) {
            tΔ1.Skipf("skipping failed nslookup %s test: %s", server, err);
        }
        slices.Sort<slice<@string>, @string>(expected);
        slices.Sort<slice<@string>, @string>(txt);
        if (!slices.Equal<slice<@string>, @string>(expected, txt)) {
            tΔ1.Errorf("different results %s:\texp:%v\tgot:%v", server, toJson(expected), toJson(txt));
        }
    });
}

public static void TestLookupLocalPTR(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    var (addr, err) = localIP();
    if (err != nil) {
        t.Errorf("failed to get local ip: %s", err);
    }
    (var names, err) = LookupAddr(addr.String());
    if (err != nil) {
        t.Errorf("failed %s: %s", addr, err);
    }
    if (len(names) == 0) {
        t.Errorf("no results");
    }
    (var expected, err) = lookupPTR(addr.String());
    if (err != nil) {
        t.Skipf("skipping failed lookup %s test: %s", addr.String(), err);
    }
    slices.Sort<slice<@string>, @string>(expected);
    slices.Sort<slice<@string>, @string>(names);
    if (!slices.Equal<slice<@string>, @string>(expected, names)) {
        t.Errorf("different results %s:\texp:%v\tgot:%v", addr, toJson(expected), toJson(names));
    }
}

public static void TestLookupPTR(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    foreach (var (_, addr) in range(lookupTestIPs)) {
        var (names, err) = LookupAddr(addr);
        if (err != nil) {
            // The DNSError type stores the error as a string, so it cannot wrap the
            // original error code and we cannot check for it here. However, we can at
            // least use its error string to identify the correct localized text for
            // the error to skip.
            syscall.Errno DNS_ERROR_RCODE_SERVER_FAILURE = 9002;
            if (strings.HasSuffix(err.Error(), DNS_ERROR_RCODE_SERVER_FAILURE.Error())) {
                testenv.SkipFlaky(new net_test_package.testing_TжTB(t), 38111);
            }
            t.Errorf("failed %s: %s", addr, err);
        }
        if (len(names) == 0) {
            t.Errorf("no results");
        }
        (var expected, err) = lookupPTR(addr);
        if (err != nil) {
            t.Logf("skipping failed lookup %s test: %s", addr, err);
            continue;
        }
        slices.Sort<slice<@string>, @string>(expected);
        slices.Sort<slice<@string>, @string>(names);
        if (!slices.Equal<slice<@string>, @string>(expected, names)) {
            t.Errorf("different results %s:\texp:%v\tgot:%v", addr, toJson(expected), toJson(names));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nslookupˢ => ref nslookupˢᵇ;
private static readonly @string nslookupˢᵇ = "nslookup"u8;
internal static ref readonly @string canTFindˢ => ref canTFindˢᵇ;
private static readonly @string canTFindˢᵇ = "can't find"u8;

internal static (@string, error) nslookup(@string qtype, @string name) {
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    ref var err = ref heap(new strings.Builder(), out var Ꮡerr);
    var cmd = exec.Command(nslookupˢ, "-querytype="u8 + qtype, name);
    cmd.Stdout = new net_test_package.strings_BuilderжWriter(Ꮡout);
    cmd.Stderr = new net_test_package.strings_BuilderжWriter(Ꮡerr);
    var errΔ1 = cmd.Run(); if (errΔ1 != nil) {
        return ("", errΔ1);
    }
    @string r = strings.ReplaceAll(@out.String(), "\r\n", "\n");
    // nslookup stderr output contains also debug information such as
    // "Non-authoritative answer" and it doesn't return the correct errcode
    if (strings.Contains(err.String(), canTFindˢ)) {
        return (r, errors.New(err.String()));
    }
    return (r, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mAZ09SMailExchangerSS09Sˢ => ref mAZ09SMailExchangerSS09Sˢᵇ;
private static readonly @string mAZ09SMailExchangerSS09Sˢᵇ = @"(?m)^([a-z0-9.\-]+)\s+mail exchanger\s*=\s*([0-9]+)\s*([a-z0-9.\-]+)$"u8;
internal static ref readonly @string mAZ09SMxPreferenceSS09SSˢ => ref mAZ09SMxPreferenceSS09SSˢᵇ;
private static readonly @string mAZ09SMxPreferenceSS09SSˢᵇ = @"(?m)^([a-z0-9.\-]+)\s+MX preference\s*=\s*([0-9]+)\s*,\s*mail exchanger\s*=\s*([a-z0-9.\-]+)$"u8;

internal static (slice<ж<global::go.net_package.MX>> mx, error err) nslookupMX(@string name) {
    slice<ж<global::go.net_package.MX>> mx = default;
    error err = default;

    @string r = default;
    (r, err) = nslookup("mx", name); if (err != nil) {
        return (mx, err);
    }
    mx = new slice<ж<global::go.net_package.MX>>(0, 10);
    // linux nslookup syntax
    // golang.org      mail exchanger = 2 alt1.aspmx.l.google.com.
    var rx = Δregexp.MustCompile(mAZ09SMailExchangerSS09Sˢ);
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        var (pref, _, _) = dtoi(ans[2]);
        mx = append(mx, Ꮡ(new MX(absDomainName(ans[3]), (uint16)pref)));
    }
    // windows nslookup syntax
    // gmail.com       MX preference = 30, mail exchanger = alt3.gmail-smtp-in.l.google.com
    rx = Δregexp.MustCompile(mAZ09SMxPreferenceSS09SSˢ);
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        var (pref, _, _) = dtoi(ans[2]);
        mx = append(mx, Ꮡ(new MX(absDomainName(ans[3]), (uint16)pref)));
    }
    return (mx, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mAZ09SNameserverSSAZ09ˢ => ref mAZ09SNameserverSSAZ09ˢᵇ;
private static readonly @string mAZ09SNameserverSSAZ09ˢᵇ = @"(?m)^([a-z0-9.\-]+)\s+nameserver\s*=\s*([a-z0-9.\-]+)$"u8;

internal static (slice<ж<global::go.net_package.NS>> ns, error err) nslookupNS(@string name) {
    slice<ж<global::go.net_package.NS>> ns = default;
    error err = default;

    @string r = default;
    (r, err) = nslookup("ns", name); if (err != nil) {
        return (ns, err);
    }
    ns = new slice<ж<global::go.net_package.NS>>(0, 10);
    // golang.org      nameserver = ns1.google.com.
    var rx = Δregexp.MustCompile(mAZ09SNameserverSSAZ09ˢ);
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        ns = append(ns, Ꮡ(new NS(absDomainName(ans[2]))));
    }
    return (ns, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cnameˢ2 => ref cnameˢ2ᵇ;
private static readonly @string cnameˢ2ᵇ = "cname"u8;
internal static ref readonly @string mAZ09SCanonicalNameSSAZ0ˢ => ref mAZ09SCanonicalNameSSAZ0ˢᵇ;
private static readonly @string mAZ09SCanonicalNameSSAZ0ˢᵇ = @"(?m)^([a-z0-9.\-]+)\s+canonical name\s*=\s*([a-z0-9.\-]+)$"u8;

internal static (@string cname, error err) nslookupCNAME(@string name) {
    @string cname = default;
    error err = default;

    @string r = default;
    (r, err) = nslookup(cnameˢ2, name); if (err != nil) {
        return (cname, err);
    }
    // mail.golang.com canonical name = golang.org.
    var rx = Δregexp.MustCompile(mAZ09SCanonicalNameSSAZ0ˢ);
    // assumes the last CNAME is the correct one
    @string last = name;
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        last = ans[2];
    }
    return (absDomainName(last), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string txtˢ => ref txtˢᵇ;
private static readonly @string txtˢᵇ = "txt"u8;
internal static ref readonly @string mAZ09STextSSˢ => ref mAZ09STextSSˢᵇ;
private static readonly @string mAZ09STextSSˢᵇ = @"(?m)^([a-z0-9.\-]+)\s+text\s*=\s*""(.*)""$"u8;

internal static (slice<@string> txt, error err) nslookupTXT(@string name) {
    slice<@string> txt = default;
    error err = default;

    @string r = default;
    (r, err) = nslookup(txtˢ, name); if (err != nil) {
        return (txt, err);
    }
    txt = new slice<@string>(0, 10);
    // linux
    // golang.org      text = "v=spf1 redirect=_spf.google.com"

    // windows
    // golang.org      text =
    //
    //    "v=spf1 redirect=_spf.google.com"
    var rx = Δregexp.MustCompile(mAZ09STextSSˢ);
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        txt = append(txt, ans[2]);
    }
    return (txt, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pingˢ => ref pingˢᵇ;
private static readonly @string pingˢᵇ = "ping"u8;

internal static (@string, error) ping(@string name) {
    var cmd = exec.Command(pingˢ, "-n", "1", "-a", name);
    var (stdoutStderr, err) = cmd.CombinedOutput();
    if (err != nil) {
        return ("", fmt.Errorf("%v: %v"u8, err, (@string)stdoutStderr));
    }
    @string r = strings.ReplaceAll((@string)stdoutStderr, "\r\n", "\n");
    return (r, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mPingingSAZAZ09Sˢ => ref mPingingSAZAZ09Sˢᵇ;
private static readonly @string mPingingSAZAZ09Sˢᵇ = @"(?m)^Pinging\s+([a-zA-Z0-9.\-]+)\s+\[.*$"u8;

internal static (slice<@string> ptr, error err) lookupPTR(@string name) {
    slice<@string> ptr = default;
    error err = default;

    @string r = default;
    (r, err) = ping(name); if (err != nil) {
        return (ptr, err);
    }
    ptr = new slice<@string>(0, 10);
    var rx = Δregexp.MustCompile(mPingingSAZAZ09Sˢ);
    foreach (var (_, ans) in range(rx.FindAllStringSubmatch(r, -1))) {
        ptr = append(ptr, absDomainName(ans[1]));
    }
    return (ptr, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string golangOrg80ˢ => ref golangOrg80ˢᵇ;
private static readonly @string golangOrg80ˢᵇ = "golang.org:80"u8;

internal static (global::go.net_package.IP ip, error err) localIP() {
    global::go.net_package.IP ip = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        (var conn, err) = Dial(udpˢ, golangOrg80ˢ);
        if (err != nil) {
            (ip, err) = (default, err); goto ᒐdone;
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        var localAddr = conn.LocalAddr()._<ж<global::go.net_package.UDPAddr>>();

        (ip, err) = (localAddr.IP, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (ip, err);
}

} // end net_internal_test_package
