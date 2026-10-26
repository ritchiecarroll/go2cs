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
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string gcflagsMˢ = "--gcflags=-m"u8;
internal static readonly @string netNetipˢ = "net/netip"u8;
internal static readonly @string canInlineSˢ = @" can inline (\S+)"u8;
internal static readonly @string canInlineˢ = " can inline "u8;
internal static readonly @string funcˢ = ".func"u8;

public static void TestInlining(ж<testing.T> Ꮡt) {
    var wantInlinableᴷ = new stackAppend<@string>();

    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    var (@out, err) = exec.Command(
        testenv.GoToolPath(new testing_TжTB(Ꮡt)),
        buildˢ,
        gcflagsMˢ,
        netNetipˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go build: %v, %s"u8, err, @out);
    }
    var got = new map<@string, bool>{};
    var gotʗ1 = got;
    regexp.MustCompile(canInlineSˢ).ReplaceAllFunc(@out, (slice<byte> match) => {
        gotʗ1[strings.TrimPrefix((@string)match, canInlineˢ)] = true;
        return default!;
    });
    var wantInlinable = new @string[]{
        "(*uint128).halves"u8,
        "Addr.BitLen"u8,
        "Addr.hasZone"u8,
        "Addr.Is4"u8,
        "Addr.Is4In6"u8,
        "Addr.Is6"u8,
        "Addr.IsInterfaceLocalMulticast"u8,
        "Addr.IsValid"u8,
        "Addr.IsUnspecified"u8,
        "Addr.Less"u8,
        "Addr.Unmap"u8,
        "Addr.Zone"u8,
        "Addr.v4"u8,
        "Addr.v6"u8,
        "Addr.v6u16"u8,
        "Addr.withoutZone"u8,
        "AddrPortFrom"u8,
        "AddrPort.Addr"u8,
        "AddrPort.Port"u8,
        "AddrPort.IsValid"u8,
        "Prefix.IsSingleIP"u8,
        "Prefix.Masked"u8,
        "Prefix.IsValid"u8,
        "PrefixFrom"u8,
        "Prefix.Addr"u8,
        "Prefix.Bits"u8,
        "AddrFrom4"u8,
        "IPv6LinkLocalAllNodes"u8,
        "IPv6Unspecified"u8,
        "MustParseAddr"u8,
        "MustParseAddrPort"u8,
        "MustParsePrefix"u8,
        "appendDecimal"u8,
        "appendHex"u8,
        "uint128.addOne"u8,
        "uint128.and"u8,
        "uint128.bitsClearedFrom"u8,
        "uint128.bitsSetFrom"u8,
        "uint128.isZero"u8,
        "uint128.not"u8,
        "uint128.or"u8,
        "uint128.subOne"u8,
        "uint128.xor"u8
    }.slice();
    var exprᴛ1 = runtime.GOARCH;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
        // These don't inline on 32-bit.
        wantInlinable = append(wantInlinableᴷ, wantInlinable,
            "Addr.AsSlice"u8,
            "Addr.Next",
            "Addr.Prev");
    }

    foreach (var (_, want) in wantInlinable) {
        if (!got[want]) {
            Ꮡt.Errorf("%q is no longer inlinable"u8, want);
            continue;
        }
        delete(got, want);
    }
    foreach (var (sym, _) in got) {
        if (strings.Contains(sym, funcˢ)) {
            continue;
        }
        Ꮡt.Logf("not in expected set, but also inlinable: %q"u8, sym);
    }
}

} // end netip_package
