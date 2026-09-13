# go.vendor.golang.org.x.crypto.cryptobyte

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@v0.23.1--0.20240603234054--0b431c7de36a-00ADD8?logo=go)](https://pkg.go.dev/golang.org/x/crypto@v0.23.1-0.20240603234054-0b431c7de36a/cryptobyte)\
[![Source](https://img.shields.io/badge/Source-@v0.23.1--0.20240603234054--0b431c7de36a-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/vendor/golang.org/x/crypto/cryptobyte) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/vendor/golang.org/x/crypto/cryptobyte)

Package cryptobyte contains types that help with parsing and constructing length-prefixed, binary messages, including ASN.1 DER. (The asn1 subpackage contains useful ASN.1 constants.)

The String type is for parsing. It wraps a \[]byte slice and provides helper functions for consuming structures, value by value.

The Builder type is for constructing messages. It providers helper functions for appending values and also for appending length-prefixed submessages – without having to worry about calculating the length prefix ahead of time.

See the documentation and examples for the Builder and String types to get started.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
