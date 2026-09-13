# go.net.http.pprof

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-not_yet_validated-orange?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/net/http/pprof@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/net/http/pprof) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/net/http/pprof)

Package pprof serves via its HTTP server runtime profiling data in the format expected by the pprof visualization tool.

The package is typically only imported for the side effect of registering its HTTP handlers. The handled paths all begin with /debug/pprof/. As of Go 1.22, all the paths must be requested with GET.

To use pprof, link this package into your program:

	import _ "net/http/pprof"

If your application is not already running an http server, you need to start one. Add "net/http" and "log" to your imports and the following code to your main function:

	go func() {
		log.Println(http.ListenAndServe("localhost:6060", nil))
	}()

By default, all the profiles listed in [runtime/pprof.Profile](https://pkg.go.dev/runtime/pprof@go1.23.12#Profile) are available (via \[Handler]), in addition to the \[Cmdline], \[Profile], \[Symbol], and \[Trace] profiles defined in this package. If you are not using DefaultServeMux, you will have to register handlers with the mux you are using.

### Parameters

Parameters can be passed via GET query params:

  - debug=N (all profiles): response format: N = 0: binary (default), N > 0: plaintext
  - gc=N (heap profile): N > 0: run a garbage collection cycle before profiling
  - seconds=N (allocs, block, goroutine, heap, mutex, threadcreate profiles): return a delta profile
  - seconds=N (cpu (profile), trace profiles): profile for the given duration

### Usage examples

Use the pprof tool to look at the heap profile:

	go tool pprof http://localhost:6060/debug/pprof/heap

Or to look at a 30-second CPU profile:

	go tool pprof http://localhost:6060/debug/pprof/profile?seconds=30

Or to look at the goroutine blocking profile, after calling [runtime.SetBlockProfileRate](https://pkg.go.dev/runtime@go1.23.12#SetBlockProfileRate) in your program:

	go tool pprof http://localhost:6060/debug/pprof/block

Or to look at the holders of contended mutexes, after calling [runtime.SetMutexProfileFraction](https://pkg.go.dev/runtime@go1.23.12#SetMutexProfileFraction) in your program:

	go tool pprof http://localhost:6060/debug/pprof/mutex

The package also exports a handler that serves execution trace data for the "go tool trace" command. To collect a 5-second execution trace:

	curl -o trace.out http://localhost:6060/debug/pprof/trace?seconds=5
	go tool trace trace.out

To view all available profiles, open [http://localhost:6060/debug/pprof/](http://localhost:6060/debug/pprof/) in your browser.

For a study of the facility in action, visit [https://blog.golang.org/2011/06/profiling-go-programs.html](https://blog.golang.org/2011/06/profiling-go-programs.html).

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
