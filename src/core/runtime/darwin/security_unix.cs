// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using stringslite = @internal.stringslite_package;
using @internal;

partial class runtime_package {

internal static void secure() {
    initSecureMode();

    if (!isSecureMode()) {
        return;
    }

    // When secure mode is enabled, we do one thing: enforce specific
    // environment variable values (currently we only force GOTRACEBACK=none)
    //
    // Other packages may also disable specific functionality when secure mode
    // is enabled (determined by using linkname to call isSecureMode).

    secureEnv();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gotracebackˢ2 => ref gotracebackˢ2ᵇ;
private static readonly @string gotracebackˢ2ᵇ = "GOTRACEBACK="u8;
internal static ref readonly @string gotracebackNoneˢ => ref gotracebackNoneˢᵇ;
private static readonly @string gotracebackNoneˢᵇ = "GOTRACEBACK=none"u8;

internal static void secureEnv() {
    bool hasTraceback = default;
    for (nint i = 0; i < len(envs); i++) {
        if (stringslite.HasPrefix(envs[i], gotracebackˢ2)) {
            hasTraceback = true;
            envs[i] = gotracebackNoneˢ;
        }
    }
    if (!hasTraceback) {
        envs = append(envs, (@string)"GOTRACEBACK=none");
    }
}

} // end runtime_package
