// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

internal partial struct staticHostEntry {
    internal @string @in;
    internal slice<@string> @out;
}

partial struct lookupStaticHostTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<staticHostEntry> ents;
}
internal static slice<lookupStaticHostTestsᴛ1> lookupStaticHostTests = [
    new(
        "testdata/hosts",
        [
            new("odin", ["127.0.0.2", "127.0.0.3", "::2"]),
            new("thor", ["127.1.1.1"]),
            new("ullr", ["127.1.1.2"]),
            new("ullrhost", ["127.1.1.2"]),
            new("localhost", ["fe80::1%lo0"])
        ]
    ),
    new(
        "testdata/singleline-hosts", // see golang.org/issue/6646
        [
            new("odin", ["127.0.0.2"])
        ]
    ),
    new(
        "testdata/ipv4-hosts",
        [
            new("localhost", ["127.0.0.1", "127.0.0.2", "127.0.0.3"]),
            new("localhost.localdomain", ["127.0.0.3"])
        ]
    ),
    new(
        "testdata/ipv6-hosts", // see golang.org/issue/8996
        [
            new("localhost", ["::1", "fe80::1", "fe80::2%lo0", "fe80::3%lo0"]),
            new("localhost.localdomain", ["fe80::3%lo0"])
        ]
    ),
    new(
        "testdata/case-hosts", // see golang.org/issue/12806
        [
            new("PreserveMe", ["127.0.0.1", "::1"]),
            new("PreserveMe.local", ["127.0.0.1", "::1"])
        ]
    )
];

public static void TestLookupStaticHost(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        foreach (var (_, tt) in range(lookupStaticHostTests)) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in range(tt.ents)) {
                testStaticHost(t, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticHost(ж<testing.T> t, @string hostsPath, staticHostEntry ent) {
    var ins = slice<@string>([ent.@in, absDomainName(ent.@in), strings.ToLower(ent.@in), strings.ToUpper(ent.@in)]);
    foreach (var (_, @in) in range(ins)) {
        var (addrs, _) = lookupStaticHost(@in);
        if (!slices.Equal<slice<@string>, @string>(addrs, ent.@out)) {
            t.Errorf("%s, lookupStaticHost(%s) = %v; want %v", hostsPath, @in, addrs, ent.@out);
        }
    }
}

internal static slice<lookupStaticHostTestsᴛ1> lookupStaticAddrTests = [
    new(
        "testdata/hosts",
        [
            new("255.255.255.255", ["broadcasthost"]),
            new("127.0.0.2", ["odin"]),
            new("127.0.0.3", ["odin"]),
            new("::2", ["odin"]),
            new("127.1.1.1", ["thor"]),
            new("127.1.1.2", ["ullr", "ullrhost"]),
            new("fe80::1%lo0", ["localhost"])
        ]
    ),
    new(
        "testdata/singleline-hosts", // see golang.org/issue/6646
        [
            new("127.0.0.2", ["odin"])
        ]
    ),
    new(
        "testdata/ipv4-hosts",
        [
            new("127.0.0.1", ["localhost"]),
            new("127.0.0.2", ["localhost"]),
            new("127.0.0.3", ["localhost", "localhost.localdomain"])
        ]
    ),
    new(
        "testdata/ipv6-hosts", // see golang.org/issue/8996
        [
            new("::1", ["localhost"]),
            new("fe80::1", ["localhost"]),
            new("fe80::2%lo0", ["localhost"]),
            new("fe80::3%lo0", ["localhost", "localhost.localdomain"])
        ]
    ),
    new(
        "testdata/case-hosts", // see golang.org/issue/12806
        [
            new("127.0.0.1", ["PreserveMe", "PreserveMe.local"]),
            new("::1", ["PreserveMe", "PreserveMe.local"])
        ]
    )
];

public static void TestLookupStaticAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        foreach (var (_, tt) in range(lookupStaticAddrTests)) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in range(tt.ents)) {
                testStaticAddr(t, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticAddr(ж<testing.T> t, @string hostsPath, staticHostEntry ent) {
    var hosts = lookupStaticAddr(ent.@in);
    foreach (var i in range(len(ent.@out))) {
        ent.@out[i] = absDomainName(ent.@out[i]);
    }
    if (!slices.Equal<slice<@string>, @string>(hosts, ent.@out)) {
        t.Errorf("%s, lookupStaticAddr(%s) = %v; want %v", hostsPath, ent.@in, hosts, ent.@out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataIpv4Hostsˢ => ref testdataIpv4Hostsˢᵇ;
private static readonly @string testdataIpv4Hostsˢᵇ = "testdata/ipv4-hosts"u8;
internal static ref readonly @string testdataIpv6Hostsˢ => ref testdataIpv6Hostsˢᵇ;
private static readonly @string testdataIpv6Hostsˢᵇ = "testdata/ipv6-hosts"u8;

public static void TestHostCacheModification(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Ensure that programs can't modify the internals of the host cache.
        // See https://golang.org/issues/14212.
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        hostsFilePath = testdataIpv4Hostsˢ;
        var ent = new staticHostEntry("localhost", ["127.0.0.1", "127.0.0.2", "127.0.0.3"]);
        testStaticHost(t, hostsFilePath, ent);
        // Modify the addresses return by lookupStaticHost.
        var (addrs, _) = lookupStaticHost(ent.@in);
        foreach (var i in range(len(addrs))) {
            addrs[i] += "junk"u8;
        }
        testStaticHost(t, hostsFilePath, ent);

        hostsFilePath = testdataIpv6Hostsˢ;
        ent = new staticHostEntry("::1", ["localhost"]);
        testStaticAddr(t, hostsFilePath, ent);
        // Modify the hosts return by lookupStaticAddr.
        var hosts = lookupStaticAddr(ent.@in);
        foreach (var i in range(len(hosts))) {
            hosts[i] += "junk"u8;
        }
        testStaticAddr(t, hostsFilePath, ent);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct lookupStaticHostAliasesTestᴛ1 /*dyn*/ {
    internal @string lookup, res;
}
internal static slice<lookupStaticHostAliasesTestᴛ1> lookupStaticHostAliasesTest = [
    // 127.0.0.1
    new("test", "test"),
    // 127.0.0.2
    new("test2.example.com", "test2.example.com"),
    new("2.test", "test2.example.com"),
    // 127.0.0.3
    new("test3.example.com", "3.test"),
    new("3.test", "3.test"),
    // 127.0.0.4
    new("example.com", "example.com"),
    // 127.0.0.5
    new("test5.example.com", "test4.example.com"),
    new("5.test", "test4.example.com"),
    new("4.test", "test4.example.com"),
    new("test4.example.com", "test4.example.com")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataAliasesˢ => ref testdataAliasesˢᵇ;
private static readonly @string testdataAliasesˢᵇ = "testdata/aliases"u8;

public static void TestLookupStaticHostAliases(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        hostsFilePath = testdataAliasesˢ;
        foreach (var (_, ent) in range(lookupStaticHostAliasesTest)) {
            testLookupStaticHostAliases(t, ent.lookup, absDomainName(ent.res));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testLookupStaticHostAliases(ж<testing.T> t, @string lookup, @string lookupRes) {
    var ins = slice<@string>([lookup, absDomainName(lookup), strings.ToLower(lookup), strings.ToUpper(lookup)]);
    foreach (var (_, @in) in range(ins)) {
        var (_, res) = lookupStaticHost(@in);
        if (res != lookupRes) {
            t.Errorf("lookupStaticHost(%v): got %v, want %v", @in, res, lookupRes);
        }
    }
}

} // end net_internal_test_package
