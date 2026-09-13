# go.index.suffixarray

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-12%2F12_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/index.suffixarray.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/index/suffixarray@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/index/suffixarray) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/index/suffixarray)

Package suffixarray implements substring search in logarithmic time using an in-memory suffix array.

Example use:

	// create index for some data
	index := suffixarray.New(data)

	// lookup byte slice s
	offsets1 := index.Lookup(s, -1) // the list of all indices where s occurs in data
	offsets2 := index.Lookup(s, 3)  // the list of at most 3 indices where s occurs in data

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
