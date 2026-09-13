# go.internal.weak

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-4%2F4_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/internal.weak.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/weak@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/weak) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/weak)

The weak package is a package for managing weak pointers.

Weak pointers are pointers that explicitly do not keep a value live and must be queried for a regular Go pointer. The result of such a query may be observed as nil at any point after a weakly-pointed-to object becomes eligible for reclamation by the garbage collector. More specifically, weak pointers become nil as soon as the garbage collector identifies that the object is unreachable, before it is made reachable again by a finalizer. In terms of the C# language, these semantics are roughly equivalent to the the semantics of "short" weak references. In terms of the Java language, these semantics are roughly equivalent to the semantics of the WeakReference type.

Using go:linkname to access this package and the functions it references is explicitly forbidden by the toolchain because the semantics of this package have not gone through the proposal process. By exposing this functionality, we risk locking in the existing semantics due to Hyrum's Law.

If you believe you have a good use-case for weak references not already covered by the standard library, file a proposal issue at [https://github.com/golang/go/issues](https://github.com/golang/go/issues) instead of relying on this package.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
