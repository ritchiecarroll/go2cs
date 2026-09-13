# go.log.slog.internal.benchmarks

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-3%2F3_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/log.slog.internal.benchmarks.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/log/slog/internal/benchmarks@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/log/slog/internal/benchmarks) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/log/slog/internal/benchmarks)

Package benchmarks contains benchmarks for slog.

These benchmarks are loosely based on github.com/uber-go/zap/benchmarks. They have the following desirable properties:

  - They test a complete log event, from the user's call to its return.

  - The benchmarked code is run concurrently in multiple goroutines, to better simulate a real server (the most common environment for structured logs).

  - Some handlers are optimistic versions of real handlers, doing real-world tasks as fast as possible (and sometimes faster, in that an implementation may not be concurrency-safe). This gives us an upper bound on handler performance, so we can evaluate the (handler-independent) core activity of the package in an end-to-end context without concern that a slow handler implementation is skewing the results.

  - We also test the built-in handlers, for comparison.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
