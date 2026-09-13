# go.testing

> Hand-implemented C# counterpart of the Go standard library's `testing` package, by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-37%2F52_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/testing.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/testing@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/testing) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/testing)

Package testing provides support for automated testing of Go packages. This is the go2cs Phase-4 test host: a hand-maintained implementation of the `testing` API — `T`, `B`, `F`, `TB`, subtests, parallelism, `TempDir` with Go-faithful `os.RemoveAll` cleanup semantics, `Setenv`, package deadlines — that runs converted `_test.go` suites and compares their verdicts one-for-one against a clean `go test -json` baseline. Every validated package's proof page on [go2cs.net/validation](https://go2cs.net/validation/index.html) was produced under this host.

---

This package is a hand-maintained implementation of the Go standard library's `testing` API rather than converted Go source — hand-owning the test host is what keeps one `testing` package shared by every converted test project. The go2cs implementation is distributed under the same BSD-3-Clause license as the converted standard library, which can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
