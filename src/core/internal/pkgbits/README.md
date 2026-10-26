# go.internal.pkgbits

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-2_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.7/internal.pkgbits.html) [![Docs](https://img.shields.io/badge/Docs-@1.26.10-00ADD8?logo=go)](https://pkg.go.dev/internal/pkgbits@go1.26.10)\
[![Source](https://img.shields.io/badge/Source-@1.26.10-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.26.10/src/internal/pkgbits) [![Source](https://img.shields.io/badge/Source-@1.24.13.7-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.7/src/core/internal/pkgbits)

From C#, call this package through the static class go.@internal.pkgbits_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package pkgbits implements low-level coding abstractions for Unified IR's (UIR) binary export data format.

At a low-level, the exported objects of a package are encoded as a byte array. This array contains byte representations of primitive, potentially variable-length values, such as integers, booleans, strings, and constants.

Additionally, the array may contain values which denote indices in the byte array itself. These are termed "relocations" and allow for references.

The details of mapping high-level Go constructs to primitives are left to other packages.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
