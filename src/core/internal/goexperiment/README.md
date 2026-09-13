# go.internal.goexperiment

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/goexperiment@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/goexperiment) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/goexperiment)

Package goexperiment implements support for toolchain experiments.

Toolchain experiments are controlled by the GOEXPERIMENT environment variable. GOEXPERIMENT is a comma-separated list of experiment names. GOEXPERIMENT can be set at make.bash time, which sets the default experiments for binaries built with the tool chain; or it can be set at build time. GOEXPERIMENT can also be set to "none", which disables any experiments that were enabled at make.bash time.

Experiments are exposed to the build in the following ways:

\- Build tag goexperiment.x is set if experiment x (lower case) is enabled.

\- For each experiment x (in camel case), this package contains a boolean constant x and an integer constant xInt.

\- In runtime assembly, the macro GOEXPERIMENT\_x is defined if experiment x (lower case) is enabled.

In the toolchain, the set of experiments enabled for the current build should be accessed via objabi.Experiment.

The set of experiments is included in the output of runtime.Version() and "go version \<binary>" if it differs from the default experiments.

For the set of experiments supported by the current toolchain, see "go doc goexperiment.Flags".

Note that this package defines the set of experiments (in Flags) and records the experiments that were enabled when the package was compiled (as boolean and integer constants).

Note especially that this package does not itself change behavior at run time based on the GOEXPERIMENT variable. The code used in builds to interpret the GOEXPERIMENT variable is in the separate package internal/buildcfg.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
