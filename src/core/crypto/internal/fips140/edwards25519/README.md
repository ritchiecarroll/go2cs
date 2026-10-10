# go.crypto.internal.fips140.edwards25519

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-54_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.5/crypto.internal.fips140.edwards25519.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/fips140/edwards25519@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/internal/fips140/edwards25519) [![Source](https://img.shields.io/badge/Source-@1.24.13.5-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.5/src/core/crypto/internal/fips140/edwards25519)

From C#, call this package through the static class go.crypto.@internal.fips140.edwards25519_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package edwards25519 implements group logic for the twisted Edwards curve

	-x^2 + y^2 = 1 + -(121665/121666)*x^2*y^2

This is better known as the Edwards curve equivalent to Curve25519, and is the curve used by the Ed25519 signature scheme.

Most users don't need this package, and should instead use crypto/ed25519 for signatures, golang.org/x/crypto/curve25519 for Diffie-Hellman, or github.com/gtank/ristretto255 for prime order group logic.

However, developers who do need to interact with low-level edwards25519 operations can use filippo.io/edwards25519, an extended version of this package repackaged as an importable module.

(Note that filippo.io/edwards25519 and github.com/gtank/ristretto255 are not maintained by the Go team and are not covered by the Go 1 Compatibility Promise.)

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
