// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package entropy provides the passive entropy source for the FIPS 140-3
// module. It is only used in FIPS mode by [crypto/internal/fips140/drbg.Read]
// from the FIPS 140-3 Go Cryptographic Module v1.0.0.
//
// Later versions of the module use the CPU jitter-based entropy source in the
// crypto/internal/entropy/v1.0.0 sub-package.
//
// This complied with IG 9.3.A, Additional Comment 12, which until January 1,
// 2026 allows new modules to meet an [earlier version] of Resolution 2(b):
// "A software module that contains an approved DRBG that receives a LOAD
// command (or its logical equivalent) with entropy obtained from [...] inside
// the physical perimeter of the operational environment of the module [...]."
//
// Distributions that have their own SP 800-90B entropy source should replace
// this package with their own implementation.
//
// [earlier version]: https://csrc.nist.gov/CSRC/media/Projects/cryptographic-module-validation-program/documents/IG%209.3.A%20Resolution%202b%5BMarch%2026%202024%5D.pdf
namespace go.crypto.@internal;

using sysrand = go.crypto.@internal.sysrand_package;
using go.crypto.@internal;

partial class entropy_package {

// Depleted notifies the entropy source that the entropy in the module is
// "depleted" and provides the callback for the LOAD command.
public static void Depleted(Action<ж<array<byte>>> LOAD) {
    ref var entropy = ref heap(new array<byte>(48), out var Ꮡentropy);
    sysrand.Read(entropy[..]);
    LOAD(Ꮡentropy);
}

} // end entropy_package
