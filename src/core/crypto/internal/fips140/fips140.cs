// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using godebug = go.crypto.@internal.fips140deps.godebug_package;
using errors = errors_package;
using runtime = runtime_package;
using go.crypto.@internal.fips140deps;

partial class fips140_package {

public static bool Enabled;

internal static bool debug;

[GoInit] internal static void init() {
    @string v = godebug.Value("#fips140"u8);
    var exprᴛ1 = v;
    if (exprᴛ1 == "on"u8 || exprᴛ1 == "only"u8) {
        Enabled = true;
    }
    else if (exprᴛ1 == "debug"u8) {
        Enabled = true;
        debug = true;
    }
    else if (exprᴛ1 == "off"u8 || exprᴛ1 == ""u8) {
    }
    else { /* default: */
        throw panic("fips140: unknown GODEBUG setting fips140=" + v);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fips1403ModeIsˢ = "FIPS 140-3 mode is incompatible with the purego build tag"u8;
private static readonly @string fips1403ModeIsˢ2 = "FIPS 140-3 mode is incompatible with ASAN"u8;
private static readonly @string fips1403ModeIsˢ3 = "FIPS 140-3 mode is incompatible with GOEXPERIMENT=boringcrypto"u8;

// Supported returns an error if FIPS 140-3 mode can't be enabled.
public static error Supported() {
    // Keep this in sync with fipsSupported in cmd/dist/test.go.

    // The purego tag changes too much of the implementation to claim the
    // validation still applies.
    if (puregoEnabled) {
        return errors.New(fips1403ModeIsˢ);
    }

    // ASAN disapproves of reading swaths of global memory in fips140/check.
    // One option would be to expose runtime.asanunpoison through
    // crypto/internal/fips140deps and then call it to unpoison the range
    // before reading it, but it is unclear whether that would then cause
    // false negatives. For now, FIPS+ASAN doesn't need to work.
    if (asanEnabled) {
        return errors.New(fips1403ModeIsˢ2);
    }

    // See EnableFIPS in cmd/internal/obj/fips.go for commentary.
    // Also, js/wasm and windows/386 don't have good enough timers
    // for the CPU jitter entropy source.
    switch (ᐧ) {
    case {} when (runtime.GOARCH == "wasm"u8) || (runtime.GOOS == "windows"u8 && runtime.GOARCH == "386"u8) || (runtime.GOOS == "openbsd"u8) || (runtime.GOOS == "aix"u8): {
        // due to -fexecute-only, see #70880
        return errors.New("FIPS 140-3 mode is not supported on "u8 + runtime.GOOS + "-"u8 + runtime.GOARCH);
    }}

    if (boringEnabled) {
        return errors.New(fips1403ModeIsˢ3);
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goCryptographicModuleˢ = "Go Cryptographic Module"u8;

public static @string Name() {
    return goCryptographicModuleˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string latestˢ = "latest"u8;

// Version returns the formal version (such as "v1.0.0") if building against a
// frozen module with GOFIPS140. Otherwise, it returns "latest".
public static @string Version() {
    // This return value is replaced by mkzip.go, it must not be changed or
    // moved to a different file.
    return latestˢ; //mkzip:version
}

} // end fips140_package
