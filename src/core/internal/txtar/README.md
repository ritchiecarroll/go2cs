# go.internal.txtar

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/txtar@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/txtar) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/txtar)

Package txtar implements a trivial text-based file archive format.

The goals for the format are:

  - be trivial enough to create and edit by hand.
  - be able to store trees of text files describing go command test cases.
  - diff nicely in git history and code reviews.

Non-goals include being a completely general archive format, storing binary data, storing file modes, storing special files like symbolic links, and so on.

### Txtar format

A txtar archive is zero or more comment lines and then a sequence of file entries. Each file entry begins with a file marker line of the form "-- FILENAME --" and is followed by zero or more file content lines making up the file data. The comment or file content ends at the next file marker line. The file marker line must begin with the three-byte sequence "-- " and end with the three-byte sequence " --", but the enclosed file name can be surrounding by additional white space, all of which is stripped.

If the txtar file is missing a trailing newline on the final line, parsers should consider a final newline to be present anyway.

There are no possible syntax errors in a txtar archive.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
