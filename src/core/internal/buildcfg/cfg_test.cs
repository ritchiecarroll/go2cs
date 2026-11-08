// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using os = os_package;
using testing = testing_package;
using static go.@internal.buildcfg_package;

partial class buildcfg_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rva20u64ˢ => ref rva20u64ˢᵇ;
private static readonly @string rva20u64ˢᵇ = "rva20u64"u8;
internal static ref readonly @string rva22u64ˢ => ref rva22u64ˢᵇ;
private static readonly @string rva22u64ˢᵇ = "rva22u64"u8;
internal static ref readonly @string rva23u64ˢ => ref rva23u64ˢᵇ;
private static readonly @string rva23u64ˢᵇ = "rva23u64"u8;
internal static ref readonly @string rva22ˢ => ref rva22ˢᵇ;
private static readonly @string rva22ˢᵇ = "rva22"u8;
internal static ref readonly @string v70ˢ => ref v70ˢᵇ;
private static readonly @string v70ˢᵇ = "v7.0"u8;
internal static ref readonly @string v80Lsbˢ => ref v80Lsbˢᵇ;
private static readonly @string v80Lsbˢᵇ = "v8.0,lsb"u8;
internal static ref readonly @string v80Lseˢ => ref v80Lseˢᵇ;
private static readonly @string v80Lseˢᵇ = "v8.0,lse"u8;
internal static ref readonly @string v80Cryptoˢ => ref v80Cryptoˢᵇ;
private static readonly @string v80Cryptoˢᵇ = "v8.0,crypto"u8;
internal static ref readonly @string v80CryptoLseˢ => ref v80CryptoLseˢᵇ;
private static readonly @string v80CryptoLseˢᵇ = "v8.0,crypto,lse"u8;
internal static ref readonly @string v80LseCryptoˢ => ref v80LseCryptoˢᵇ;
private static readonly @string v80LseCryptoˢᵇ = "v8.0,lse,crypto"u8;
internal static ref readonly @string v90ˢ => ref v90ˢᵇ;
private static readonly @string v90ˢᵇ = "v9.0"u8;

public static void TestConfigFlags(ж<testing.T> t) {
    os.Setenv(goamd64ˢ, "v1");
    if (goamd64() != 1) {
        t.Errorf("Wrong parsing of GOAMD64=v1");
    }
    os.Setenv(goamd64ˢ, "v4");
    if (goamd64() != 4) {
        t.Errorf("Wrong parsing of GOAMD64=v4");
    }
    Error = default;
    os.Setenv(goamd64ˢ, "1");
    goamd64(); if (Error == nil) {
        t.Errorf("Wrong parsing of GOAMD64=1");
    }

    os.Setenv(goriscv64ˢ, rva20u64ˢ);
    if (goriscv64() != 20) {
        t.Errorf("Wrong parsing of RISCV64=rva20u64");
    }
    os.Setenv(goriscv64ˢ, rva22u64ˢ);
    if (goriscv64() != 22) {
        t.Errorf("Wrong parsing of RISCV64=rva22u64");
    }
    os.Setenv(goriscv64ˢ, rva23u64ˢ);
    if (goriscv64() != 23) {
        t.Errorf("Wrong parsing of RISCV64=rva23u64");
    }
    Error = default;
    os.Setenv(goriscv64ˢ, rva22ˢ);
    _ = goriscv64(); if (Error == nil) {
        t.Errorf("Wrong parsing of RISCV64=rva22");
    }
    Error = default;
    os.Setenv(goarm64ˢ, v70ˢ);
    _ = goarm64(); if (Error == nil) {
        t.Errorf("Wrong parsing of GOARM64=7.0");
    }
    Error = default;
    os.Setenv(goarm64ˢ, "8.0");
    _ = goarm64(); if (Error == nil) {
        t.Errorf("Wrong parsing of GOARM64=8.0");
    }
    Error = default;
    os.Setenv(goarm64ˢ, v80Lsbˢ);
    _ = goarm64(); if (Error == nil) {
        t.Errorf("Wrong parsing of GOARM64=v8.0,lsb");
    }
    os.Setenv(goarm64ˢ, v80Lseˢ);
    if (goarm64().Version != "v8.0"u8 || goarm64().LSE != true || goarm64().Crypto != false) {
        t.Errorf("Wrong parsing of GOARM64=v8.0,lse");
    }
    os.Setenv(goarm64ˢ, v80Cryptoˢ);
    if (goarm64().Version != "v8.0"u8 || goarm64().LSE != false || goarm64().Crypto != true) {
        t.Errorf("Wrong parsing of GOARM64=v8.0,crypto");
    }
    os.Setenv(goarm64ˢ, v80CryptoLseˢ);
    if (goarm64().Version != "v8.0"u8 || goarm64().LSE != true || goarm64().Crypto != true) {
        t.Errorf("Wrong parsing of GOARM64=v8.0,crypto,lse");
    }
    os.Setenv(goarm64ˢ, v80LseCryptoˢ);
    if (goarm64().Version != "v8.0"u8 || goarm64().LSE != true || goarm64().Crypto != true) {
        t.Errorf("Wrong parsing of GOARM64=v8.0,lse,crypto");
    }
    os.Setenv(goarm64ˢ, v90ˢ);
    if (goarm64().Version != "v9.0"u8 || goarm64().LSE != true || goarm64().Crypto != false) {
        t.Errorf("Wrong parsing of GOARM64=v9.0");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string v93ˢ => ref v93ˢᵇ;
private static readonly @string v93ˢᵇ = "v9.3"u8;
internal static ref readonly @string v94ˢ => ref v94ˢᵇ;
private static readonly @string v94ˢᵇ = "v9.4"u8;
internal static ref readonly @string v88ˢ => ref v88ˢᵇ;
private static readonly @string v88ˢᵇ = "v8.8"u8;
internal static ref readonly @string v89ˢ => ref v89ˢᵇ;
private static readonly @string v89ˢᵇ = "v8.9"u8;
internal static ref readonly @string lseˢ => ref lseˢᵇ;
private static readonly @string lseˢᵇ = ",lse"u8;

public static void TestGoarm64FeaturesSupports(ж<testing.T> t) {
    var (g, _) = ParseGoarm64(v93ˢ);

    if (!g.Supports(v93ˢ)) {
        t.Errorf("Wrong goarm64Features.Supports for v9.3, v9.3");
    }

    if (g.Supports(v94ˢ)) {
        t.Errorf("Wrong goarm64Features.Supports for v9.3, v9.4");
    }

    if (!g.Supports(v88ˢ)) {
        t.Errorf("Wrong goarm64Features.Supports for v9.3, v8.8");
    }

    if (g.Supports(v89ˢ)) {
        t.Errorf("Wrong goarm64Features.Supports for v9.3, v8.9");
    }

    if (g.Supports(lseˢ)) {
        t.Errorf("Wrong goarm64Features.Supports for v9.3, ,lse");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string arm64ˢ => ref arm64ˢᵇ;
private static readonly @string arm64ˢᵇ = "arm64"u8;
internal static ref readonly @string v95ˢ => ref v95ˢᵇ;
private static readonly @string v95ˢᵇ = "v9.5"u8;
internal static readonly object wrongTagsForGoarm64V95ˢ = (@string)"Wrong tags for GOARM64=v9.5"u8;

public static void TestGogoarchTags(ж<testing.T> t) {
    @string old_goarch = GOARCH;
    var old_goarm64 = GOARM64;

    GOARCH = arm64ˢ;

    os.Setenv(goarm64ˢ, v95ˢ);
    GOARM64 = goarm64();
    var tags = gogoarchTags();
    var want = slice<@string>(["arm64.v9.0", "arm64.v9.1", "arm64.v9.2", "arm64.v9.3", "arm64.v9.4", "arm64.v9.5",
        "arm64.v8.0", "arm64.v8.1", "arm64.v8.2", "arm64.v8.3", "arm64.v8.4", "arm64.v8.5", "arm64.v8.6", "arm64.v8.7", "arm64.v8.8", "arm64.v8.9"]);
    if (len(tags) != len(want)) {
        t.Errorf("Wrong number of tags for GOARM64=v9.5");
    } else {
        foreach (var (i, v) in range(tags)) {
            if (v != want[i]) {
                t.Error(wrongTagsForGoarm64V95ˢ);
                break;
            }
        }
    }

    GOARCH = old_goarch;
    GOARM64 = old_goarm64;
}

internal static slice<@string> goodFIPS = [
    "v1.0.0",
    "v1.0.1",
    "v1.2.0",
    "v1.2.3"
];

internal static slice<@string> badFIPS = [
    "v1.0.0-fips",
    "v1.0.0+fips",
    "1.0.0",
    "x1.0.0"
];

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string vᶜ => ref vᶜᵇ;
private static readonly @string vᶜᵇ = "v1.2.3"u8;

public static void TestIsFIPSVersion(ж<testing.T> t) {
    // good
    foreach (var (_, s) in range(goodFIPS)) {
        if (!isFIPSVersion(s)) {
            t.Errorf("isFIPSVersion(%q) = false, want true", s);
        }
    }
    // truncated
    @string v = vᶜ;
    for (nint i = 0; i < len(v); i++) {
        if (isFIPSVersion(v.slice(0, i))) {
            t.Errorf("isFIPSVersion(%q) = true, want false", v.slice(0, i));
        }
    }
    // bad
    foreach (var (_, s) in range(badFIPS)) {
        if (isFIPSVersion(s)) {
            t.Errorf("isFIPSVersion(%q) = true, want false", s);
        }
    }
}

} // end buildcfg_internal_test_package
