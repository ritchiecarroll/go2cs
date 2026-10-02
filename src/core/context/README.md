# go.context

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-57%2F58_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.3/context.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/context@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/context) [![Source](https://img.shields.io/badge/Source-@1.24.13.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.3/src/core/context)

Package context defines the Context type, which carries deadlines, cancellation signals, and other request-scoped values across API boundaries and between processes.

Incoming requests to a server should create a \[Context], and outgoing calls to servers should accept a Context. The chain of function calls between them must propagate the Context, optionally replacing it with a derived Context created using \[WithCancel], \[WithDeadline], \[WithTimeout], or \[WithValue].

A Context may be canceled to indicate that work done on its behalf should stop. A Context with a deadline is canceled after the deadline passes. When a Context is canceled, all Contexts derived from it are also canceled.

The \[WithCancel], \[WithDeadline], and \[WithTimeout] functions take a Context (the parent) and return a derived Context (the child) and a \[CancelFunc]. Calling the CancelFunc directly cancels the child and its children, removes the parent's reference to the child, and stops any associated timers. Failing to call the CancelFunc leaks the child and its children until the parent is canceled. The go vet tool checks that CancelFuncs are used on all control-flow paths.

The \[WithCancelCause], \[WithDeadlineCause], and \[WithTimeoutCause] functions return a \[CancelCauseFunc], which takes an error and records it as the cancellation cause. Calling \[Cause] on the canceled context or any of its children retrieves the cause. If no cause is specified, Cause(ctx) returns the same value as ctx.Err().

Programs that use Contexts should follow these rules to keep interfaces consistent across packages and enable static analysis tools to check context propagation:

Do not store Contexts inside a struct type; instead, pass a Context explicitly to each function that needs it. This is discussed further in [https://go.dev/blog/context-and-structs](https://go.dev/blog/context-and-structs). The Context should be the first parameter, typically named ctx:

	func DoSomething(ctx context.Context, arg Arg) error {
		// ... use ctx ...
	}

Do not pass a nil \[Context], even if a function permits it. Pass [context.TODO](https://pkg.go.dev/context@go1.24.13#TODO) if you are unsure about which Context to use.

Use context Values only for request-scoped data that transits processes and APIs, not for passing optional parameters to functions.

The same Context may be passed to functions running in different goroutines; Contexts are safe for simultaneous use by multiple goroutines.

See [https://go.dev/blog/context](https://go.dev/blog/context) for example code for a server that uses Contexts.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
