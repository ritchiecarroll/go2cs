# go.plugin

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-1%2F1_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/plugin.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/plugin@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/plugin) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/plugin)

Package plugin implements loading and symbol resolution of Go plugins.

A plugin is a Go main package with exported functions and variables that has been built with:

	go build -buildmode=plugin

When a plugin is first opened, the init functions of all packages not already part of the program are called. The main function is not run. A plugin is only initialized once, and cannot be closed.

### Warnings

The ability to dynamically load parts of an application during execution, perhaps based on user-defined configuration, may be a useful building block in some designs. In particular, because applications and dynamically loaded functions can share data structures directly, plugins may enable very high-performance integration of separate parts.

However, the plugin mechanism has many significant drawbacks that should be considered carefully during the design. For example:

  - Plugins are currently supported only on Linux, FreeBSD, and macOS, making them unsuitable for applications intended to be portable.

  - Applications that use plugins may require careful configuration to ensure that the various parts of the program be made available in the correct location in the file system (or container image). By contrast, deploying an application consisting of a single static executable is straightforward.

  - Reasoning about program initialization is more difficult when some packages may not be initialized until long after the application has started running.

  - Bugs in applications that load plugins could be exploited by an attacker to load dangerous or untrusted libraries.

  - Runtime crashes are likely to occur unless all parts of the program (the application and all its plugins) are compiled using exactly the same version of the toolchain, the same build tags, and the same values of certain flags and environment variables.

  - Similar crashing problems are likely to arise unless all common dependencies of the application and its plugins are built from exactly the same source code.

  - Together, these restrictions mean that, in practice, the application and its plugins must all be built together by a single person or component of a system. In that case, it may be simpler for that person or component to generate Go source files that blank-import the desired set of plugins and then compile a static executable in the usual way.

For these reasons, many users decide that traditional interprocess communication (IPC) mechanisms such as sockets, pipes, remote procedure call (RPC), shared memory mappings, or file system operations may be more suitable despite the performance overheads.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
