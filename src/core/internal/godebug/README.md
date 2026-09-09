# go.internal.godebug

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-5%2F5_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/internal.godebug.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/godebug@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/godebug) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/godebug)

Package godebug makes the settings in the $GODEBUG environment variable available to other packages. These settings are often used for compatibility tweaks, when we need to change a default behavior but want to let users opt back in to the original. For example GODEBUG=http2server=0 disables HTTP/2 support in the net/http server.

In typical usage, code should declare a Setting as a global and then call Value each time the current setting value is needed:

	var http2server = godebug.New("http2server")

	func ServeConn(c net.Conn) {
		if http2server.Value() == "0" {
			disallow HTTP/2
			...
		}
		...
	}

Each time a non-default setting causes a change in program behavior, code must call \[Setting.IncNonDefault] to increment a counter that can be reported by [runtime/metrics.Read](https://pkg.go.dev/runtime/metrics@go1.23.12#Read). The call must only happen when the program executes a non-default behavior, not just when the setting is set to a non-default value. This is occasionally (but very rarely) infeasible, in which case the internal/godebugs table entry must set Opaque: true, and the documentation in doc/godebug.md should mention that metrics are unavailable.

Conventionally, the global variable representing a godebug is named for the godebug itself, with no case changes:

	var gotypesalias = godebug.New("gotypesalias") // this
	var goTypesAlias = godebug.New("gotypesalias") // NOT THIS

The test in internal/godebugs that checks for use of IncNonDefault requires the use of this convention.

Note that counters used with IncNonDefault must be added to various tables in other packages. See the \[Setting.IncNonDefault] documentation for details.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
