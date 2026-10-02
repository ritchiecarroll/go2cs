# go.crypto.internal.fips140.check

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/crypto/internal/fips140/check@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/crypto/internal/fips140/check) [![Source](https://img.shields.io/badge/Source-@1.24.13.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.3/src/core/crypto/internal/fips140/check)

Package check implements the FIPS 140 load-time code+data verification. Every FIPS package providing cryptographic functionality except hmac and sha256 must import crypto/internal/fips140/check, so that the verification happens before initialization of package global variables. The hmac and sha256 packages are used by this package, so they cannot import it. Instead, those packages must be careful not to change global variables during init. (If necessary, we could have check call a PostCheck function in those packages after the check has completed.)

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
