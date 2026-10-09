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

// see golang.org/issue/6646
// see golang.org/issue/8996
// see golang.org/issue/12806

partial struct lookupStaticHostTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<staticHostEntry> ents;
}
internal static slice<lookupStaticHostTestsᴛ1> lookupStaticHostTests = new lookupStaticHostTestsᴛ1[]{
    new(
        "testdata/hosts"u8,
        new staticHostEntry[]{
            new("odin"u8, new @string[]{"127.0.0.2"u8, "127.0.0.3"u8, "::2"u8}.slice()),
            new("thor"u8, new @string[]{"127.1.1.1"u8}.slice()),
            new("ullr"u8, new @string[]{"127.1.1.2"u8}.slice()),
            new("ullrhost"u8, new @string[]{"127.1.1.2"u8}.slice()),
            new("localhost"u8, new @string[]{"fe80::1%lo0"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/singleline-hosts"u8,
        new staticHostEntry[]{
            new("odin"u8, new @string[]{"127.0.0.2"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/ipv4-hosts"u8,
        new staticHostEntry[]{
            new("localhost"u8, new @string[]{"127.0.0.1"u8, "127.0.0.2"u8, "127.0.0.3"u8}.slice()),
            new("localhost.localdomain"u8, new @string[]{"127.0.0.3"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/ipv6-hosts"u8,
        new staticHostEntry[]{
            new("localhost"u8, new @string[]{"::1"u8, "fe80::1"u8, "fe80::2%lo0"u8, "fe80::3%lo0"u8}.slice()),
            new("localhost.localdomain"u8, new @string[]{"fe80::3%lo0"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/case-hosts"u8,
        new staticHostEntry[]{
            new("PreserveMe"u8, new @string[]{"127.0.0.1"u8, "::1"u8}.slice()),
            new("PreserveMe.local"u8, new @string[]{"127.0.0.1"u8, "::1"u8}.slice())
        }.slice()
    )
}.slice();

public static void TestLookupStaticHost(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);
        foreach (var (_, tt) in lookupStaticHostTests) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in tt.ents) {
                testStaticHost(Ꮡt, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticHost(ж<testing.T> Ꮡt, @string hostsPath, staticHostEntry ent) {
    var ins = new @string[]{ent.@in, absDomainName(ent.@in), strings.ToLower(ent.@in), strings.ToUpper(ent.@in)}.slice();
    foreach (var (_, @in) in ins) {
        var (addrs, _) = lookupStaticHost(@in);
        if (!slices.Equal<slice<@string>, @string>(addrs, ent.@out)) {
            Ꮡt.Errorf("%s, lookupStaticHost(%s) = %v; want %v"u8, hostsPath, @in, addrs, ent.@out);
        }
    }
}

// see golang.org/issue/6646
// see golang.org/issue/8996
// see golang.org/issue/12806
internal static slice<lookupStaticHostTestsᴛ1> lookupStaticAddrTests = new lookupStaticHostTestsᴛ1[]{
    new(
        "testdata/hosts"u8,
        new staticHostEntry[]{
            new("255.255.255.255"u8, new @string[]{"broadcasthost"u8}.slice()),
            new("127.0.0.2"u8, new @string[]{"odin"u8}.slice()),
            new("127.0.0.3"u8, new @string[]{"odin"u8}.slice()),
            new("::2"u8, new @string[]{"odin"u8}.slice()),
            new("127.1.1.1"u8, new @string[]{"thor"u8}.slice()),
            new("127.1.1.2"u8, new @string[]{"ullr"u8, "ullrhost"u8}.slice()),
            new("fe80::1%lo0"u8, new @string[]{"localhost"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/singleline-hosts"u8,
        new staticHostEntry[]{
            new("127.0.0.2"u8, new @string[]{"odin"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/ipv4-hosts"u8,
        new staticHostEntry[]{
            new("127.0.0.1"u8, new @string[]{"localhost"u8}.slice()),
            new("127.0.0.2"u8, new @string[]{"localhost"u8}.slice()),
            new("127.0.0.3"u8, new @string[]{"localhost"u8, "localhost.localdomain"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/ipv6-hosts"u8,
        new staticHostEntry[]{
            new("::1"u8, new @string[]{"localhost"u8}.slice()),
            new("fe80::1"u8, new @string[]{"localhost"u8}.slice()),
            new("fe80::2%lo0"u8, new @string[]{"localhost"u8}.slice()),
            new("fe80::3%lo0"u8, new @string[]{"localhost"u8, "localhost.localdomain"u8}.slice())
        }.slice()
    ),
    new(
        "testdata/case-hosts"u8,
        new staticHostEntry[]{
            new("127.0.0.1"u8, new @string[]{"PreserveMe"u8, "PreserveMe.local"u8}.slice()),
            new("::1"u8, new @string[]{"PreserveMe"u8, "PreserveMe.local"u8}.slice())
        }.slice()
    )
}.slice();

public static void TestLookupStaticAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);
        foreach (var (_, tt) in lookupStaticAddrTests) {
            hostsFilePath = tt.name;
            foreach (var (_, ent) in tt.ents) {
                testStaticAddr(Ꮡt, tt.name, ent);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testStaticAddr(ж<testing.T> Ꮡt, @string hostsPath, staticHostEntry ent) {
    var hosts = lookupStaticAddr(ent.@in);
    foreach (var (i, _) in ent.@out) {
        ent.@out[i] = absDomainName(ent.@out[i]);
    }
    if (!slices.Equal<slice<@string>, @string>(hosts, ent.@out)) {
        Ꮡt.Errorf("%s, lookupStaticAddr(%s) = %v; want %v"u8, hostsPath, ent.@in, hosts, ent.@out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataIpv4Hostsˢ = "testdata/ipv4-hosts"u8;
internal static readonly @string testdataIpv6Hostsˢ = "testdata/ipv6-hosts"u8;

public static void TestHostCacheModification(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Ensure that programs can't modify the internals of the host cache.
        // See https://golang.org/issues/14212.
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);
        hostsFilePath = testdataIpv4Hostsˢ;
        var ent = new staticHostEntry("localhost"u8, new @string[]{"127.0.0.1"u8, "127.0.0.2"u8, "127.0.0.3"u8}.slice());
        testStaticHost(Ꮡt, hostsFilePath, ent);
        // Modify the addresses return by lookupStaticHost.
        var (addrs, _) = lookupStaticHost(ent.@in);
        foreach (var (i, _) in addrs) {
            addrs[i] += "junk"u8;
        }
        testStaticHost(Ꮡt, hostsFilePath, ent);
        hostsFilePath = testdataIpv6Hostsˢ;
        ent = new staticHostEntry("::1"u8, new @string[]{"localhost"u8}.slice());
        testStaticAddr(Ꮡt, hostsFilePath, ent);
        // Modify the hosts return by lookupStaticAddr.
        var hosts = lookupStaticAddr(ent.@in);
        foreach (var (i, _) in hosts) {
            hosts[i] += "junk"u8;
        }
        testStaticAddr(Ꮡt, hostsFilePath, ent);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// 127.0.0.1
// 127.0.0.2
// 127.0.0.3
// 127.0.0.4
// 127.0.0.5

partial struct lookupStaticHostAliasesTestᴛ1 /*dyn*/ {
    internal @string lookup, res;
}
internal static slice<lookupStaticHostAliasesTestᴛ1> lookupStaticHostAliasesTest = new lookupStaticHostAliasesTestᴛ1[]{
    new("test"u8, "test"u8),
    new("test2.example.com"u8, "test2.example.com"u8),
    new("2.test"u8, "test2.example.com"u8),
    new("test3.example.com"u8, "3.test"u8),
    new("3.test"u8, "3.test"u8),
    new("example.com"u8, "example.com"u8),
    new("test5.example.com"u8, "test4.example.com"u8),
    new("5.test"u8, "test4.example.com"u8),
    new("4.test"u8, "test4.example.com"u8),
    new("test4.example.com"u8, "test4.example.com"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataAliasesˢ = "testdata/aliases"u8;

public static void TestLookupStaticHostAliases(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer((@string orig) => {
            hostsFilePath = orig;
        }, hostsFilePath, ref ᒐ);
        hostsFilePath = testdataAliasesˢ;
        foreach (var (_, ent) in lookupStaticHostAliasesTest) {
            testLookupStaticHostAliases(Ꮡt, ent.lookup, absDomainName(ent.res));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testLookupStaticHostAliases(ж<testing.T> Ꮡt, @string lookup, @string lookupRes) {
    var ins = new @string[]{lookup, absDomainName(lookup), strings.ToLower(lookup), strings.ToUpper(lookup)}.slice();
    foreach (var (_, @in) in ins) {
        var (_, res) = lookupStaticHost(@in);
        if (res != lookupRes) {
            Ꮡt.Errorf("lookupStaticHost(%v): got %v, want %v"u8, @in, res, lookupRes);
        }
    }
}

} // end net_internal_test_package
