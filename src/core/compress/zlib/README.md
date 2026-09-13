# go.compress.zlib

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-6%2F6_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/compress.zlib.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/compress/zlib@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/compress/zlib) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/compress/zlib)

Package zlib implements reading and writing of zlib format compressed data, as specified in RFC 1950.

The implementation provides filters that uncompress during reading and compress during writing.  For example, to write compressed data to a buffer:

	var b bytes.Buffer
	w := zlib.NewWriter(&b)
	w.Write([]byte("hello, world\n"))
	w.Close()

and to read that data back:

	r, err := zlib.NewReader(&b)
	io.Copy(os.Stdout, r)
	r.Close()

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
