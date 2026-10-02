# go.crypto.internal.entropy

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/entropy@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/internal/entropy) [![Source](https://img.shields.io/badge/Source-@1.24.13.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.3/src/core/crypto/internal/entropy)

Package entropy provides the passive entropy source for the FIPS 140-3 module. It is only used in FIPS mode by [crypto/internal/fips140/drbg.Read](https://pkg.go.dev/crypto/internal/fips140/drbg@go1.24.13#Read).

This complies with IG 9.3.A, Additional Comment 12, which until January 1, 2026 allows new modules to meet an [earlier version](https://csrc.nist.gov/CSRC/media/Projects/cryptographic-module-validation-program/documents/IG%209.3.A%20Resolution%202b%5BMarch%2026%202024%5D.pdf) of Resolution 2(b): "A software module that contains an approved DRBG that receives a LOAD command (or its logical equivalent) with entropy obtained from \[...] inside the physical perimeter of the operational environment of the module \[...]."

Distributions that have their own SP 800-90B entropy source should replace this package with their own implementation.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
