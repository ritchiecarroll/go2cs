// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fips140tls controls whether crypto/tls requires FIPS-approved settings.
namespace go.crypto.tls.@internal;

using fips140 = go.crypto.fips140_package;
using atomic = sync.atomic_package;
using go.crypto;
using sync;

partial class fips140tls_package {

internal static readonly ж<atomic.Bool> Ꮡrequired = new StaticBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool @required => ref ((StaticBox<atomic.Bool>)Ꮡrequired).ValueSlot;

[GoInit] internal static void init() {
    if (fips140.Enabled()) {
        Force();
    }
}

// Force forces crypto/tls to restrict TLS configurations to FIPS-approved settings.
// By design, this call is impossible to undo (except in tests).
public static void Force() {
    Ꮡrequired.Store(true);
}

// Required reports whether FIPS-approved settings are required.
//
// Required is true if FIPS 140-3 mode is enabled with GODEBUG=fips140=on, or if
// the crypto/tls/fipsonly package is imported by a Go+BoringCrypto build.
public static bool Required() {
    return Ꮡrequired.Load();
}

public static void TestingOnlyAbandon() {
    Ꮡrequired.Store(false);
}

} // end fips140tls_package
