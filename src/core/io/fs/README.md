# go.io.fs

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-21_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.26.10.1/io.fs.html) [![Docs](https://img.shields.io/badge/Docs-@1.26.10-00ADD8?logo=go)](https://pkg.go.dev/io/fs@go1.26.10)\
[![Source](https://img.shields.io/badge/Source-@1.26.10-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.26.10/src/io/fs) [![Source](https://img.shields.io/badge/Source-@1.26.10.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.1/src/core/io/fs)

From C#, call this package through the static class go.io.fs_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package fs defines basic interfaces to a file system. A file system can be provided by the host operating system but also by other packages.

### Path Names

The interfaces in this package all operate on the same path name syntax, regardless of the host operating system.

Path names are UTF-8-encoded, unrooted, slash-separated sequences of path elements, like “x/y/z”. Path names must not contain an element that is “.” or “..” or the empty string, except for the special case that the name "." may be used for the root directory. Paths must not start or end with a slash: “/x” and “x/” are invalid.

### Testing

See the [testing/fstest](https://pkg.go.dev/testing/fstest@go1.26.10) package for support with testing implementations of file systems.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
