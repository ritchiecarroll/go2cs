// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using testenv = @internal.testenv_package;
using exec = os.exec_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using os;

partial class netip_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string gcflagsMˢ => ref gcflagsMˢᵇ;
private static readonly @string gcflagsMˢᵇ = "--gcflags=-m"u8;
internal static ref readonly @string netNetipˢ => ref netNetipˢᵇ;
private static readonly @string netNetipˢᵇ = "net/netip"u8;
internal static ref readonly @string canInlineSˢ => ref canInlineSˢᵇ;
private static readonly @string canInlineSˢᵇ = @" can inline (\S+)"u8;
internal static ref readonly @string canInlineˢ => ref canInlineˢᵇ;
private static readonly @string canInlineˢᵇ = " can inline "u8;
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = ".func"u8;

public static void TestInlining(ж<testing.T> t) {
    var wantInlinableᴷ = new stackAppend<@string>();

    testenv.MustHaveGoBuild(new testing_TжTB(t));
    t.Parallel();
    var (@out, err) = exec.Command(
        testenv.GoToolPath(new testing_TжTB(t)),
        buildˢ,
        gcflagsMˢ,
        netNetipˢ).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go build: %v, %s", err, @out);
    }
    var got = new map<@string, bool>{};
    var gotʗ1 = got;
    regexp.MustCompile(canInlineSˢ).ReplaceAllFunc(@out, (slice<byte> match) => {
        gotʗ1[strings.TrimPrefix((@string)match, canInlineˢ)] = true;
        return default;
    });
    var wantInlinable = slice<@string>([
        "(*uint128).halves",
        "Addr.BitLen",
        "Addr.hasZone",
        "Addr.Is4",
        "Addr.Is4In6",
        "Addr.Is6",
        "Addr.IsInterfaceLocalMulticast",
        "Addr.IsValid",
        "Addr.IsUnspecified",
        "Addr.Less",
        "Addr.Unmap",
        "Addr.Zone",
        "Addr.v4",
        "Addr.v6",
        "Addr.v6u16",
        "Addr.withoutZone",
        "AddrPortFrom",
        "AddrPort.Addr",
        "AddrPort.Port",
        "AddrPort.IsValid",
        "Prefix.IsSingleIP",
        "Prefix.Masked",
        "Prefix.IsValid",
        "PrefixFrom",
        "Prefix.Addr",
        "Prefix.Bits",
        "AddrFrom4",
        "IPv6LinkLocalAllNodes",
        "IPv6Unspecified",
        "MustParseAddr",
        "MustParseAddrPort",
        "MustParsePrefix",
        "appendDecimal",
        "appendHex",
        "uint128.addOne",
        "uint128.and",
        "uint128.bitsClearedFrom",
        "uint128.bitsSetFrom",
        "uint128.isZero",
        "uint128.not",
        "uint128.or",
        "uint128.subOne",
        "uint128.xor"
    ]);
    var exprᴛ1 = runtime.GOARCH;
    switch (ᐧ) {
    case {} when exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8: {
        // These don't inline on 32-bit.
        wantInlinable = append(wantInlinableᴷ, wantInlinable,
            (@string)"Addr.AsSlice",
            (@string)"Addr.Next",
            (@string)"Addr.Prev");
        break;
    }}

    foreach (var (_, want) in wantInlinable) {
        if (!got[want]) {
            t.Errorf("%q is no longer inlinable", want);
            continue;
        }
        delete(got, want);
    }
    foreach (var (sym, _) in got) {
        if (strings.Contains(sym, funcˢ)) {
            continue;
        }
        t.Logf("not in expected set, but also inlinable: %q", sym);
    }
}

} // end netip_package
