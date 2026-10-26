# go.encoding.json

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-550_matched_%2F_0_disclosed-brightgreen?logo=go)](https://go2cs.net/validation/1.26.10.1/encoding.json.html) [![Docs](https://img.shields.io/badge/Docs-@1.26.10-00ADD8?logo=go)](https://pkg.go.dev/encoding/json@go1.26.10)\
[![Source](https://img.shields.io/badge/Source-@1.26.10-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.26.10/src/encoding/json) [![Source](https://img.shields.io/badge/Source-@1.26.10.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.1/src/core/encoding/json)

From C#, call this package through the static class go.encoding.json_package; see [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html).

Package json implements encoding and decoding of JSON as defined in RFC 7159. The mapping between JSON and Go values is described in the documentation for the Marshal and Unmarshal functions.

See "JSON and Go" for an introduction to this package: [https://golang.org/doc/articles/json\_and\_go.html](https://golang.org/doc/articles/json_and_go.html)

### Security Considerations

The JSON standard (RFC 7159) is lax in its definition of a number of parser behaviors. As such, many JSON parsers behave differently in various scenarios. These differences in parsers mean that systems that use multiple independent JSON parser implementations may parse the same JSON object in differing ways.

Systems that rely on a JSON object being parsed consistently for security purposes should be careful to understand the behaviors of this parser, as well as how these behaviors may cause interoperability issues with other parser implementations.

Due to the Go Backwards Compatibility promise ([https://go.dev/doc/go1compat](https://go.dev/doc/go1compat)) there are a number of behaviors this package exhibits that may cause interopability issues, but cannot be changed. In particular the following parsing behaviors may cause issues:

  - If a JSON object contains duplicate keys, keys are processed in the order they are observed, meaning later values will replace or be merged into prior values, depending on the field type (in particular maps and structs will have values merged, while other types have values replaced).
  - When parsing a JSON object into a Go struct, keys are considered in a case-insensitive fashion.
  - When parsing a JSON object into a Go struct, unknown keys in the JSON object are ignored (unless a \[Decoder] is used and \[Decoder.DisallowUnknownFields] has been called).
  - Invalid UTF-8 bytes in JSON strings are replaced by the Unicode replacement character.
  - Large JSON number integers will lose precision when unmarshaled into floating-point types.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
