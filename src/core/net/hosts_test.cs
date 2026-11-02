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
internal static slice<lookupStaticHostTestsᴛ1> lookupStaticHostTests = slice<lookupStaticHostTestsᴛ1>([
    new(
        "testdata/hosts",
        slice<staticHostEntry>([
            new("odin", slice<@string>(["127.0.0.2", "127.0.0.3", "::2"])),
            new("thor", slice<@string>(["127.1.1.1"])),
            new("ullr", slice<@string>(["127.1.1.2"])),
            new("ullrhost", slice<@string>(["127.1.1.2"])),
            new("localhost", slice<@string>(["fe80::1%lo0"]))
        ])
    ),
    new(
        "testdata/singleline-hosts", // see golang.org/issue/6646
        slice<staticHostEntry>([
            new("odin", slice<@string>(["127.0.0.2"]))
        ])
    ),
    new(
        "testdata/ipv4-hosts",
        slice<staticHostEntry>([
            new("localhost", slice<@string>(["127.0.0.1", "127.0.0.2", "127.0.0.3"])),
            new("localhost.localdomain", slice<@string>(["127.0.0.3"]))
        ])
    ),
    new(
        "testdata/ipv6-hosts", // see golang.org/issue/8996
        slice<staticHostEntry>([
            new("localhost", slice<@string>(["::1", "fe80::1", "fe80::2%lo0", "fe80::3%lo0"])),
            new("localhost.localdomain", slice<@string>(["fe80::3%lo0"]))
        ])
    ),
    new(
        "testdata/case-hosts", // see golang.org/issue/12806
        slice<staticHostEntry>([
            new("PreserveMe", slice<@string>(["127.0.0.1", "::1"])),
            new("PreserveMe.local", slice<@string>(["127.0.0.1", "::1"]))
        ])
    )
]);

public static void TestLookupStaticHost(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        foreach (var (_, tt) in lookupStaticHostTests) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in tt.ents) {
                testStaticHost(t, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticHost(ж<testing.T> t, @string hostsPath, staticHostEntry ent) {
    var ins = slice<@string>([ent.@in, absDomainName(ent.@in), strings.ToLower(ent.@in), strings.ToUpper(ent.@in)]);
    foreach (var (_, @in) in ins) {
        var (addrs, _) = lookupStaticHost(@in);
        if (!slices.Equal<slice<@string>, @string>(addrs, ent.@out)) {
            t.Errorf("%s, lookupStaticHost(%s) = %v; want %v", hostsPath, @in, addrs, ent.@out);
        }
    }
}

internal static slice<lookupStaticHostTestsᴛ1> lookupStaticAddrTests = slice<lookupStaticHostTestsᴛ1>([
    new(
        "testdata/hosts",
        slice<staticHostEntry>([
            new("255.255.255.255", slice<@string>(["broadcasthost"])),
            new("127.0.0.2", slice<@string>(["odin"])),
            new("127.0.0.3", slice<@string>(["odin"])),
            new("::2", slice<@string>(["odin"])),
            new("127.1.1.1", slice<@string>(["thor"])),
            new("127.1.1.2", slice<@string>(["ullr", "ullrhost"])),
            new("fe80::1%lo0", slice<@string>(["localhost"]))
        ])
    ),
    new(
        "testdata/singleline-hosts", // see golang.org/issue/6646
        slice<staticHostEntry>([
            new("127.0.0.2", slice<@string>(["odin"]))
        ])
    ),
    new(
        "testdata/ipv4-hosts",
        slice<staticHostEntry>([
            new("127.0.0.1", slice<@string>(["localhost"])),
            new("127.0.0.2", slice<@string>(["localhost"])),
            new("127.0.0.3", slice<@string>(["localhost", "localhost.localdomain"]))
        ])
    ),
    new(
        "testdata/ipv6-hosts", // see golang.org/issue/8996
        slice<staticHostEntry>([
            new("::1", slice<@string>(["localhost"])),
            new("fe80::1", slice<@string>(["localhost"])),
            new("fe80::2%lo0", slice<@string>(["localhost"])),
            new("fe80::3%lo0", slice<@string>(["localhost", "localhost.localdomain"]))
        ])
    ),
    new(
        "testdata/case-hosts", // see golang.org/issue/12806
        slice<staticHostEntry>([
            new("127.0.0.1", slice<@string>(["PreserveMe", "PreserveMe.local"])),
            new("::1", slice<@string>(["PreserveMe", "PreserveMe.local"]))
        ])
    )
]);

public static void TestLookupStaticAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);

        foreach (var (_, tt) in lookupStaticAddrTests) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in tt.ents) {
                testStaticAddr(t, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticAddr(ж<testing.T> t, @string hostsPath, staticHostEntry ent) {
    var hosts = lookupStaticAddr(ent.@in);
    foreach (var i in range(ent.@out)) {
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
        var ent = new staticHostEntry("localhost", slice<@string>(["127.0.0.1", "127.0.0.2", "127.0.0.3"]));
        testStaticHost(t, hostsFilePath, ent);
        // Modify the addresses return by lookupStaticHost.
        var (addrs, _) = lookupStaticHost(ent.@in);
        foreach (var i in range(addrs)) {
            addrs[i] += "junk"u8;
        }
        testStaticHost(t, hostsFilePath, ent);

        hostsFilePath = testdataIpv6Hostsˢ;
        ent = new staticHostEntry("::1", slice<@string>(["localhost"]));
        testStaticAddr(t, hostsFilePath, ent);
        // Modify the hosts return by lookupStaticAddr.
        var hosts = lookupStaticAddr(ent.@in);
        foreach (var i in range(hosts)) {
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
internal static slice<lookupStaticHostAliasesTestᴛ1> lookupStaticHostAliasesTest = slice<lookupStaticHostAliasesTestᴛ1>([
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
]);

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
        foreach (var (_, ent) in lookupStaticHostAliasesTest) {
            testLookupStaticHostAliases(t, ent.lookup, absDomainName(ent.res));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testLookupStaticHostAliases(ж<testing.T> t, @string lookup, @string lookupRes) {
    var ins = slice<@string>([lookup, absDomainName(lookup), strings.ToLower(lookup), strings.ToUpper(lookup)]);
    foreach (var (_, @in) in ins) {
        var (_, res) = lookupStaticHost(@in);
        if (res != lookupRes) {
            t.Errorf("lookupStaticHost(%v): got %v, want %v", @in, res, lookupRes);
        }
    }
}

} // end net_internal_test_package
