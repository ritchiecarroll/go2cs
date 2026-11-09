# nugetgo.github.com.itchyny.gojq

[//]: # (nugetgo:generated:begin)

> unofficial go2cs C# conversion of [github.com/itchyny/gojq](https://github.com/itchyny/gojq/tree/v0.12.19) v0.12.19, built on the Go 1.26.10 standard library; not affiliated with or endorsed by itchyny or the Go project. Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.26.10; review before any production use.

[![Tests](https://img.shields.io/badge/Tests-164_matched_%2F_0_disclosed-brightgreen?logo=go)](https://github.com/ritchiecarroll/gojq-cs/blob/nuget-0.12.19/VALIDATION.md) [![C# Source](https://img.shields.io/badge/C%23_Source-@0.12.19-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/gojq-cs/tree/nuget-0.12.19)\
[![Go module](https://img.shields.io/badge/Go_module-@v0.12.19-00ADD8?logo=go)](https://pkg.go.dev/github.com/itchyny/gojq@v0.12.19) [![go2cs](https://img.shields.io/badge/go2cs-@1.26.10.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.3)

Converted from [github.com/itchyny/gojq](https://github.com/itchyny/gojq/tree/v0.12.19) source to C# in [ritchiecarroll/gojq-cs](https://github.com/ritchiecarroll/gojq-cs/tree/nuget-0.12.19); licensed under MIT — see [LICENSE](https://github.com/ritchiecarroll/gojq-cs/blob/nuget-0.12.19/LICENSE).

This package is compiled against the go2cs standard-library packages at release 1.26.10.3: use it with that release of them. A later release that changes a package's public shape needs this package's rebuild for that release.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).

[//]: # (nugetgo:generated:end)

## About itchyny/gojq

[itchyny/gojq](https://github.com/itchyny/gojq) is an implementation of the jq query language in pure Go, usable as a library: `Parse` reads a jq query, `Compile` turns it into code, and `Run` applies it to a JSON value and yields each result through an iterator. This package carries the library only; the module's command-line front end (`cli` and `cmd/gojq`) is not part of it.
