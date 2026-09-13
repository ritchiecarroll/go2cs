# go.strconv

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-55%2F66_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/strconv.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/strconv@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/strconv) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/strconv)

Package strconv implements conversions to and from string representations of basic data types.

### Numeric Conversions

The most common numeric conversions are \[Atoi] (string to int) and \[Itoa] (int to string).

	i, err := strconv.Atoi("-42")
	s := strconv.Itoa(-42)

These assume decimal and the Go int type.

\[ParseBool], \[ParseFloat], \[ParseInt], and \[ParseUint] convert strings to values:

	b, err := strconv.ParseBool("true")
	f, err := strconv.ParseFloat("3.1415", 64)
	i, err := strconv.ParseInt("-42", 10, 64)
	u, err := strconv.ParseUint("42", 10, 64)

The parse functions return the widest type (float64, int64, and uint64), but if the size argument specifies a narrower width the result can be converted to that narrower type without data loss:

	s := "2147483647" // biggest int32
	i64, err := strconv.ParseInt(s, 10, 32)
	...
	i := int32(i64)

\[FormatBool], \[FormatFloat], \[FormatInt], and \[FormatUint] convert values to strings:

	s := strconv.FormatBool(true)
	s := strconv.FormatFloat(3.1415, 'E', -1, 64)
	s := strconv.FormatInt(-42, 16)
	s := strconv.FormatUint(42, 16)

\[AppendBool], \[AppendFloat], \[AppendInt], and \[AppendUint] are similar but append the formatted value to a destination slice.

### String Conversions

\[Quote] and \[QuoteToASCII] convert strings to quoted Go string literals. The latter guarantees that the result is an ASCII string, by escaping any non-ASCII Unicode with \\u:

	q := strconv.Quote("Hello, 世界")
	q := strconv.QuoteToASCII("Hello, 世界")

\[QuoteRune] and \[QuoteRuneToASCII] are similar but accept runes and return quoted Go rune literals.

\[Unquote] and \[UnquoteChar] unquote Go string and rune literals.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
