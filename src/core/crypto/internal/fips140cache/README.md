# go.crypto.internal.fips140cache

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-1_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.7/crypto.internal.fips140cache.html) [![Docs](https://img.shields.io/badge/Docs-@1.26.10-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/fips140cache@go1.26.10)\
[![Source](https://img.shields.io/badge/Source-@1.26.10-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.26.10/src/crypto/internal/fips140cache) [![Source](https://img.shields.io/badge/Source-@1.24.13.7-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.7/src/core/crypto/internal/fips140cache)

From C#, call this package through the static class go.crypto.@internal.fips140cache_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package fips140cache provides a weak map that associates the lifetime of values with the lifetime of keys.

It can be used to associate a precomputed value (such as an internal/fips140 PrivateKey value, which in FIPS 140-3 mode may have required an expensive pairwise consistency test) with a type that doesn't have private fields (such as an ed25519.PrivateKey), or that can't be safely modified because it may be concurrently copied (such as an ecdsa.PrivateKey).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
