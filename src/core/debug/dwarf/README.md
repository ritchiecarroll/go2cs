# go.debug.dwarf

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-40%2F40_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/debug.dwarf.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/debug/dwarf@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/debug/dwarf) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/debug/dwarf)

Package dwarf provides access to DWARF debugging information loaded from executable files, as defined in the DWARF 2.0 Standard at [http://dwarfstd.org/doc/dwarf-2.0.0.pdf](http://dwarfstd.org/doc/dwarf-2.0.0.pdf).

### Security

This package is not designed to be hardened against adversarial inputs, and is outside the scope of [https://go.dev/security/policy](https://go.dev/security/policy). In particular, only basic validation is done when parsing object files. As such, care should be taken when parsing untrusted inputs, as parsing malformed files may consume significant resources, or cause panics.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
