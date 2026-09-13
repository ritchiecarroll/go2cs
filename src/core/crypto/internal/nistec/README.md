# go.crypto.internal.nistec

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-2195%2F2200_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/crypto.internal.nistec.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/nistec@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/crypto/internal/nistec) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/crypto/internal/nistec)

Package nistec implements the NIST P elliptic curves from FIPS 186-4.

This package uses fiat-crypto or specialized assembly and Go code for its backend field arithmetic (not math/big) and exposes constant-time, heap allocation-free, byte slice-based safe APIs. Group operations use modern and safe complete addition formulas where possible. The point at infinity is handled and encoded according to SEC 1, Version 2.0, and invalid curve points can't be represented.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
