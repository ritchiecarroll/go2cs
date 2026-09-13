# go.encoding.csv

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-71%2F71_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/encoding.csv.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/encoding/csv@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/encoding/csv) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/encoding/csv)

Package csv reads and writes comma-separated values (CSV) files. There are many kinds of CSV files; this package supports the format described in RFC 4180, except that \[Writer] uses LF instead of CRLF as newline character by default.

A csv file contains zero or more records of one or more fields per record. Each record is separated by the newline character. The final record may optionally be followed by a newline character.

	field1,field2,field3

White space is considered part of a field.

Carriage returns before newline characters are silently removed.

Blank lines are ignored. A line with only whitespace characters (excluding the ending newline character) is not considered a blank line.

Fields which start and stop with the quote character " are called quoted-fields. The beginning and ending quote are not part of the field.

The source:

	normal string,"quoted-field"

results in the fields

	{`normal string`, `quoted-field`}

Within a quoted-field a quote character followed by a second quote character is considered a single quote.

	"the ""word"" is true","a ""quoted-field"""

results in

	{`the "word" is true`, `a "quoted-field"`}

Newlines and commas may be included in a quoted-field

	"Multi-line
	field","comma is ,"

results in

	{`Multi-line
	field`, `comma is ,`}

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
