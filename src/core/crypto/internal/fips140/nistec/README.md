# go.crypto.internal.fips140.nistec

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-44_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.5/crypto.internal.fips140.nistec.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/fips140/nistec@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/internal/fips140/nistec) [![Source](https://img.shields.io/badge/Source-@1.24.13.5-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.5/src/core/crypto/internal/fips140/nistec)

Package nistec implements the elliptic curves from NIST SP 800-186.

This package uses fiat-crypto or specialized assembly and Go code for its backend field arithmetic (not math/big) and exposes constant-time, heap allocation-free, byte slice-based safe APIs. Group operations use modern and safe complete addition formulas where possible. The point at infinity is handled and encoded according to SEC 1, Version 2.0, and invalid curve points can't be represented.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
