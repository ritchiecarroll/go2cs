# go.errors

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-61%2F61_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.24.13.3/errors.html) [![Docs](https://img.shields.io/badge/Docs-@1.24.13-00ADD8?logo=go)](https://pkg.go.dev/errors@go1.24.13)\
[![Source](https://img.shields.io/badge/Source-@1.24.13-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.24.13/src/errors) [![Source](https://img.shields.io/badge/Source-@1.24.13.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.3/src/core/errors)

Package errors implements functions to manipulate errors.

The \[New] function creates errors whose only content is a text message.

An error e wraps another error if e's type has one of the methods

	Unwrap() error
	Unwrap() []error

If e.Unwrap() returns a non-nil error w or a slice containing w, then we say that e wraps w. A nil error returned from e.Unwrap() indicates that e does not wrap any error. It is invalid for an Unwrap method to return an \[]error containing a nil error value.

An easy way to create wrapped errors is to call [fmt.Errorf](https://pkg.go.dev/fmt@go1.24.13#Errorf) and apply the %w verb to the error argument:

	wrapsErr := fmt.Errorf("... %w ...", ..., err, ...)

Successive unwrapping of an error creates a tree. The \[Is] and \[As] functions inspect an error's tree by examining first the error itself followed by the tree of each of its children in turn (pre-order, depth-first traversal).

See [https://go.dev/blog/go1.13-errors](https://go.dev/blog/go1.13-errors) for a deeper discussion of the philosophy of wrapping and when to wrap.

\[Is] examines the tree of its first argument looking for an error that matches the second. It reports whether it finds a match. It should be used in preference to simple equality checks:

	if errors.Is(err, fs.ErrExist)

is preferable to

	if err == fs.ErrExist

because the former will succeed if err wraps [io/fs.ErrExist](https://pkg.go.dev/io/fs@go1.24.13#ErrExist).

\[As] examines the tree of its first argument looking for an error that can be assigned to its second argument, which must be a pointer. If it succeeds, it performs the assignment and returns true. Otherwise, it returns false. The form

	var perr *fs.PathError
	if errors.As(err, &perr) {
		fmt.Println(perr.Path)
	}

is preferable to

	if perr, ok := err.(*fs.PathError); ok {
		fmt.Println(perr.Path)
	}

because the former will succeed if err wraps an [\*io/fs.PathError](https://pkg.go.dev/io/fs@go1.24.13#PathError).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
