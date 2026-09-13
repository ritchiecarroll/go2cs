# go.go.doc.comment

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-10059%2F10059_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/go.doc.comment.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/go/doc/comment@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/go/doc/comment) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/go/doc/comment)

Package comment implements parsing and reformatting of Go doc comments, (documentation comments), which are comments that immediately precede a top-level declaration of a package, const, func, type, or var.

Go doc comment syntax is a simplified subset of Markdown that supports links, headings, paragraphs, lists (without nesting), and preformatted text blocks. The details of the syntax are documented at [https://go.dev/doc/comment](https://go.dev/doc/comment).

To parse the text associated with a doc comment (after removing comment markers), use a \[Parser]:

	var p comment.Parser
	doc := p.Parse(text)

The result is a \[\*Doc]. To reformat it as a doc comment, HTML, Markdown, or plain text, use a \[Printer]:

	var pr comment.Printer
	os.Stdout.Write(pr.Text(doc))

The \[Parser] and \[Printer] types are structs whose fields can be modified to customize the operations. For details, see the documentation for those types.

Use cases that need additional control over reformatting can implement their own logic by inspecting the parsed syntax itself. See the documentation for \[Doc], \[Block], \[Text] for an overview and links to additional types.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
