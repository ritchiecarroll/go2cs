// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;

partial class net_internal_test_package {

[GoType] internal partial struct dnsNameTest {
    internal @string name;
    internal bool result;
}

// RFC 2181, section 11.
internal static slice<dnsNameTest> dnsNameTests = new dnsNameTest[]{
    new("_xmpp-server._tcp.google.com"u8, true),
    new("foo.com"u8, true),
    new("1foo.com"u8, true),
    new("26.0.0.73.com"u8, true),
    new("10-0-0-1"u8, true),
    new("fo-o.com"u8, true),
    new("fo1o.com"u8, true),
    new("foo1.com"u8, true),
    new("a.b..com"u8, false),
    new("a.b-.com"u8, false),
    new("a.b.com-"u8, false),
    new("a.b.."u8, false),
    new("b.com."u8, true)
}.slice();

internal static void emitDNSNameTest(channel/*<-*/<dnsNameTest> ch) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        @string char63 = ""u8;
        for (nint i = 0; i < 63; i++) {
            char63 += "a"u8;
        }
        @string char64 = char63 + "a"u8;
        @string longDomain = strings.Repeat(char63 + "."u8, 5) + "example"u8;
        foreach (var (_, tc) in dnsNameTests) {
            ch.ᐸꟷ(tc);
        }
        ch.ᐸꟷ(new dnsNameTest(char63 + ".com"u8, true));
        ch.ᐸꟷ(new dnsNameTest(char64 + ".com"u8, false));
        // Remember: wire format is two octets longer than presentation
        // (length octets for the first and [root] last labels).
        // 253 is fine:
        ch.ᐸꟷ(new dnsNameTest(longDomain.slice(len(longDomain) - 253), true));
        // A terminal dot doesn't contribute to length:
        ch.ᐸꟷ(new dnsNameTest(longDomain.slice(len(longDomain) - 253) + ".", true));
        // 254 is bad:
        ch.ᐸꟷ(new dnsNameTest(longDomain.slice(len(longDomain) - 254), false));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDNSName(ж<testing.T> Ꮡt) {
    var ch = new channel<dnsNameTest>(0);
    goǃ(emitDNSNameTest, ch.WithDirection(GoChanDir.Send));
    foreach (var tc in ch) {
        if (isDomainName(tc.name) != tc.result) {
            Ꮡt.Errorf("isDomainName(%q) = %v; want %v"u8, tc.name, !tc.result, tc.result);
        }
    }
}

public static void BenchmarkDNSName(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    var benchmarks = appendꓸꓸꓸ(dnsNameTests, new dnsNameTest[]{
        new(strings.Repeat("a"u8, 63), true),
        new(strings.Repeat("a"u8, 64), false)
    }.slice());
    for (nint n = 0; n < b.N; n++) {
        foreach (var (_, tc) in benchmarks) {
            if (isDomainName(tc.name) != tc.result) {
                Ꮡb.Errorf("isDomainName(%q) = %v; want %v"u8, tc.name, !tc.result, tc.result);
            }
        }
    }
}

} // end net_internal_test_package
