# go.go.types

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-557%2F557_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/go.types.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/go/types@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/go/types) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/go/types)

Package types declares the data types and implements the algorithms for type-checking of Go packages. Use \[Config.Check] to invoke the type checker for a package. Alternatively, create a new type checker with \[NewChecker] and invoke it incrementally by calling \[Checker.Files].

Type-checking consists of several interdependent phases:

Name resolution maps each identifier (\[ast.Ident]) in the program to the symbol (\[Object]) it denotes. Use the Defs and Uses fields of \[Info] or the \[Info.ObjectOf] method to find the symbol for an identifier, and use the Implicits field of \[Info] to find the symbol for certain other kinds of syntax node.

Constant folding computes the exact constant value (\[constant.Value]) of every expression (\[ast.Expr]) that is a compile-time constant. Use the Types field of \[Info] to find the results of constant folding for an expression.

Type deduction computes the type (\[Type]) of every expression (\[ast.Expr]) and checks for compliance with the language specification. Use the Types field of \[Info] for the results of type deduction.

For a tutorial, see [https://go.dev/s/types-tutorial](https://go.dev/s/types-tutorial).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
