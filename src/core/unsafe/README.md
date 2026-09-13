# go.unsafe

> Hand-implemented C# counterpart of the Go standard library's `unsafe` package, by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.1-00ADD8?logo=go)](https://pkg.go.dev/unsafe@go1.23.1) [![Source](https://img.shields.io/badge/Source-@1.23.1-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/unsafe) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/unsafe)

Package unsafe contains operations that step around the type safety of Go programs. In Go it is a compiler intrinsic with no Go source to convert; this package is its hand-maintained managed implementation — pointer reinterpretation, `Sizeof`/`Alignof`/`Offsetof`, and the `Slice`/`String`/`SliceData`/`StringData` constructors — built over the go2cs runtime's `ж<T>` box and pinned-buffer machinery.

---

This package is a hand-maintained implementation of the Go standard library's `unsafe` API rather than converted Go source (the original is a compiler intrinsic). The go2cs implementation is distributed under the same BSD-3-Clause license as the converted standard library, which can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
