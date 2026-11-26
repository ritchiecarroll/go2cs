// check_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// This file is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - init: DERIVED FROM src/crypto/internal/fips140/check/check.go, Go 1.26.11, func init. Go's statements except the
//   section hash, which a CLR assembly has nothing to feed.

using go;
using go.golib;

// The marker every manualConversionFuncs companion carries: crypto/internal/fips140/check's init
// is suppressed at emission and provided here. A displaced init emits only the placeholder, so this
// file carries the [GoInit] module initializer itself.
[module: GoManualConversion]

namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using godebug = go.crypto.@internal.fips140deps.godebug_package;

// Hand-written body for crypto/internal/fips140/check's init (R, COORD ruling 2026-09-22).
//
// WHY. The FIPS 140 integrity check HMACs the module's text and rodata sections, located through
// `Linkinfo`, a `go:fipsinfo` symbol the GO LINKER synthesizes (cmd/link/internal/ld/fips.go): the
// hash is a property of the Go BUILD's layout, not of the algorithms under test, and a CLR assembly
// carries no fips140 module sections, so there is nothing to hash and no linker to write the sum.
//
// WHAT WENT WRONG. Converted faithfully, `Linkinfo` is a ZERO-valued struct, so whenever
// fips140.Enabled (GODEBUG=fips140=on) the static initializer panicked `fips140: no verification
// checksum found`, and the section loop beyond it would HMAC `unsafe.Slice` bounds a managed
// assembly does not have. MEASURED: the re-exec'd GODEBUG=fips140=on child of TestCASTPasses,
// TestCASTFailures and TestConditionals died in that initializer -- 44 of
// crypto/internal/fips140test's 52 divergences.
//
// WHAT IS KEPT. Everything that is not the section hash keeps Go's shape: fips140.Enabled off
// returns at once with Verified false; an unsupported platform still panics with Go's message; the
// `#fips140=debug` line still prints; Verified is set true exactly where Go sets it. The CAST
// self-tests the suite exercises live in their own packages and are untouched. `Linkinfo`,
// `fipsMagic` and `zeroSum` stay auto-converted for any reader of the exported symbol.
partial class check_package {

[GoInit] internal static void init() {
    if (!fips140.Enabled) {
        return;
    }
    {
        var err = fips140.Supported(); if (err != default!) {
            throw panic("fips140: " + err.Error());
        }
    }
    // No module sections to hash: see the header. The checksum step is vacuous, not skipped.
    if (godebug.Value("#fips140"u8) == "debug"u8) {
        println((@string)"fips140: verified code+data"u8);
    }
    Verified = true;
}

} // end check_package
