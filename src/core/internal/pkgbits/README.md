# go.internal.pkgbits

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/pkgbits@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/pkgbits) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/pkgbits)

Package pkgbits implements low-level coding abstractions for Unified IR's export data format.

At a low-level, a package is a collection of bitstream elements. Each element has a "kind" and a dense, non-negative index. Elements can be randomly accessed given their kind and index.

Individual elements are sequences of variable-length values (e.g., integers, booleans, strings, go/constant values, cross-references to other elements). Package pkgbits provides APIs for encoding and decoding these low-level values, but the details of mapping higher-level Go constructs into elements is left to higher-level abstractions.

Elements may cross-reference each other with "relocations." For example, an element representing a pointer type has a relocation referring to the element type.

Go constructs may be composed as a constellation of multiple elements. For example, a declared function may have one element to describe the object (e.g., its name, type, position), and a separate element to describe its function body. This allows readers some flexibility in efficiently seeking or re-reading data (e.g., inlining requires re-reading the function body for each inlined call, without needing to re-read the object-level details).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
