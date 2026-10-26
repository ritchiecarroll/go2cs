# go.go.parser

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-183_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.26.10.1/go.parser.html) [![Docs](https://img.shields.io/badge/Docs-@1.26.10-00ADD8?logo=go)](https://pkg.go.dev/go/parser@go1.26.10)\
[![Source](https://img.shields.io/badge/Source-@1.26.10-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.26.10/src/go/parser) [![Source](https://img.shields.io/badge/Source-@1.26.10.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.1/src/core/go/parser)

From C#, call this package through the static class go.go.parser_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package parser implements a parser for Go source files.

The \[ParseFile] function reads file input from a string, \[]byte, or io.Reader, and produces an \[ast.File] representing the complete abstract syntax tree of the file.

The \[ParseExprFrom] function reads a single source-level expression and produces an \[ast.Expr], the syntax tree of the expression.

The parser accepts a larger language than is syntactically permitted by the Go spec, for simplicity, and for improved robustness in the presence of syntax errors. For instance, in method declarations, the receiver is treated like an ordinary parameter list and thus may contain multiple entries where the spec permits exactly one. Consequently, the corresponding field in the AST (ast.FuncDecl.Recv) field is not restricted to one entry.

Applications that need to parse one or more complete packages of Go source code may find it more convenient not to interact directly with the parser but instead to use the Load function in package [golang.org/x/tools/go/packages](https://pkg.go.dev/golang.org/x/tools/go/packages).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
