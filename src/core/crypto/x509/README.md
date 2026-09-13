# go.crypto.x509

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-341%2F341_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/crypto.x509.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/crypto/x509@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/crypto/x509) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/crypto/x509)

Package x509 implements a subset of the X.509 standard.

It allows parsing and generating certificates, certificate signing requests, certificate revocation lists, and encoded public and private keys. It provides a certificate verifier, complete with a chain builder.

The package targets the X.509 technical profile defined by the IETF (RFC 2459/3280/5280), and as further restricted by the CA/Browser Forum Baseline Requirements. There is minimal support for features outside of these profiles, as the primary goal of the package is to provide compatibility with the publicly trusted TLS certificate ecosystem and its policies and constraints.

On macOS and Windows, certificate verification is handled by system APIs, but the package aims to apply consistent validation rules across operating systems.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
