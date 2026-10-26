# nugetgo.golang.org.x.sync

[//]: # (nugetgo:generated:begin)

> unofficial go2cs C# conversion of [golang.org/x/sync](https://pkg.go.dev/golang.org/x/sync@v0.19.0) v0.19.0, built on the Go 1.26.10 standard library; not affiliated with or endorsed by The Go Authors or the Go project. Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.26.10; review before any production use.

[![Tests](https://img.shields.io/badge/Tests-27_matched_%2F_1_disclosed-brightgreen?logo=go)](https://github.com/ritchiecarroll/sync-cs/blob/nuget-0.19.0.1/VALIDATION.md) [![C# Source](https://img.shields.io/badge/C%23_Source-@0.19.0.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/sync-cs/tree/nuget-0.19.0.1)\
[![Go module](https://img.shields.io/badge/Go_module-@v0.19.0-00ADD8?logo=go)](https://pkg.go.dev/golang.org/x/sync@v0.19.0) [![go2cs](https://img.shields.io/badge/go2cs-@1.26.10.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.1)

Converted from [golang.org/x/sync](https://pkg.go.dev/golang.org/x/sync@v0.19.0) source to C# in [ritchiecarroll/sync-cs](https://github.com/ritchiecarroll/sync-cs/tree/nuget-0.19.0.1); licensed under BSD-3-Clause — see [LICENSE](https://github.com/ritchiecarroll/sync-cs/blob/nuget-0.19.0.1/LICENSE).

This package is compiled against the go2cs standard-library packages at release 1.26.10.1: use it with that release of them. A later release that changes a package's public shape needs this package's rebuild for that release.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).

[//]: # (nugetgo:generated:end)

## About golang.org/x/sync

[golang.org/x/sync](https://github.com/golang/sync) is a module from the Go project with concurrency tools beyond the standard library's sync package. errgroup runs a group of goroutines and returns the first error any of them reports, optionally limiting how many run at once; semaphore is a weighted semaphore; singleflight lets concurrent callers asking for the same key share the result of one call; and syncmap is the concurrent map that became the standard library's sync.Map.
