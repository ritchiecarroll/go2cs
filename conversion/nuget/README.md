# nugetgo.github.com.ritchiecarroll.hashset

[//]: # (nugetgo:generated:begin)

> go2cs C# conversion of [github.com/ritchiecarroll/hashset](https://github.com/ritchiecarroll/hashset/tree/v1.0.0) v1.0.0, published by its author, built on the Go 1.26.10 standard library; not affiliated with or endorsed by the Go project. Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.26.10; review before any production use.

[![Tests](https://img.shields.io/badge/Tests-37_matched_%2F_0_disclosed-brightgreen?logo=go)](https://github.com/ritchiecarroll/hashset-cs/blob/nuget-1.0.0.3/VALIDATION.md) [![C# Source](https://img.shields.io/badge/C%23_Source-@1.0.0.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/hashset-cs/tree/nuget-1.0.0.3)\
[![Go module](https://img.shields.io/badge/Go_module-@v1.0.0-00ADD8?logo=go)](https://pkg.go.dev/github.com/ritchiecarroll/hashset@v1.0.0) [![go2cs](https://img.shields.io/badge/go2cs-@1.26.10.1-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.26.10.1)

Converted from [github.com/ritchiecarroll/hashset](https://github.com/ritchiecarroll/hashset/tree/v1.0.0) source to C# in [ritchiecarroll/hashset-cs](https://github.com/ritchiecarroll/hashset-cs/tree/nuget-1.0.0.3); licensed under MIT — see [LICENSE](https://github.com/ritchiecarroll/hashset-cs/blob/nuget-1.0.0.3/LICENSE).

This package is compiled against the go2cs standard-library packages at release 1.26.10.1: use it with that release of them. A later release that changes a package's public shape needs this package's rebuild for that release.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).

[//]: # (nugetgo:generated:end)

## About hashset

[hashset](https://github.com/ritchiecarroll/hashset) is a small generic set type for Go from the go2cs authors. A set is a Go map from each element to an empty struct, so length, membership and range loops work as they do on any map, and its methods (Add, Remove, Contains, UnionWith, IntersectWith, ExceptWith, SetEquals and the rest) take the names of .NET's generic HashSet, so the same set code reads alike in both languages. It needs nothing outside the Go standard library.
