# go.crypto.ed25519

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-9%2F10_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.5/crypto.ed25519.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/ed25519@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/ed25519) [![Source](https://img.shields.io/badge/Source-@1.24.13.5-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.5/src/core/crypto/ed25519)

Package ed25519 implements the Ed25519 signature algorithm. See [https://ed25519.cr.yp.to/](https://ed25519.cr.yp.to/).

These functions are also compatible with the “Ed25519” function defined in RFC 8032. However, unlike RFC 8032's formulation, this package's private key representation includes a public key suffix to make multiple signing operations with the same key more efficient. This package refers to the RFC 8032 private key as the “seed”.

Operations involving private keys are implemented using constant-time algorithms.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
