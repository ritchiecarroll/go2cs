# go.image

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-8%2F8_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/image.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/image@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/image) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/image)

Package image implements a basic 2-D image library.

The fundamental interface is called \[Image]. An \[Image] contains colors, which are described in the image/color package.

Values of the \[Image] interface are created either by calling functions such as \[NewRGBA] and \[NewPaletted], or by calling \[Decode] on an [io.Reader](https://pkg.go.dev/io@go1.23.12#Reader) containing image data in a format such as GIF, JPEG or PNG. Decoding any particular image format requires the prior registration of a decoder function. Registration is typically automatic as a side effect of initializing that format's package so that, to decode a PNG image, it suffices to have

	import _ "image/png"

in a program's main package. The \_ means to import a package purely for its initialization side effects.

See "The Go image package" for more details: [https://golang.org/doc/articles/image\_package.html](https://golang.org/doc/articles/image_package.html)

### Security Considerations

The image package can be used to parse arbitrarily large images, which can cause resource exhaustion on machines which do not have enough memory to store them. When operating on arbitrary images, \[DecodeConfig] should be called before \[Decode], so that the program can decide whether the image, as defined in the returned header, can be safely decoded with the available resources. A call to \[Decode] which produces an extremely large image, as defined in the header returned by \[DecodeConfig], is not considered a security issue, regardless of whether the image is itself malformed or not. A call to \[DecodeConfig] which returns a header which does not match the image returned by \[Decode] may be considered a security issue, and should be reported per the \[Go Security Policy]([https://go.dev/security/policy](https://go.dev/security/policy)).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
