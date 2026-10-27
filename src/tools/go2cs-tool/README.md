# go2cs

go2cs converts Go source code into C# that reads like the Go it came from. Slices, maps, channels, goroutines,
`defer`/`panic`/`recover`, multiple returns and struct embedding become C# that a Go developer can follow, backed
by the `go.*` packages on NuGet: the Go standard library, converted by go2cs and validated against Go's own tests.

## Install

```
dotnet tool install -g go2cs
```

The tool is the converter itself, a native executable for your platform (Windows, Linux and macOS, x64 and Arm64).
It needs no Go installation: the first conversion downloads the Go release go2cs converts with, once, into the user
cache folder, and checks it against hashes built into go2cs before using it. Building the converted C# needs the
.NET 10 SDK.

```
go2cs -recurse=nuget ./mymodule ./mymodule-cs
cd mymodule-cs && dotnet run
```

## The Go it converts with

go2cs uses, in order: the Go install `GO2CS_GOROOT` names; the Go `GOROOT` names; the `go` on `PATH` when it is the
release go2cs converts with; otherwise its own downloaded copy. `go2cs toolchain` manages that copy:

```
go2cs toolchain install            download it now
go2cs toolchain install -from FILE install it from a Go release archive or toolchain module zip you already have
go2cs toolchain env                print the GOROOT, Go version and go command go2cs converts with
go2cs toolchain prune              remove copies of other Go releases
```

A module whose `go` line names a newer Go than go2cs converts with is refused with a message that says so.

## Documentation and source

<https://github.com/ritchiecarroll/go2cs>

## License

The converter is AGPL-3.0-only with the go2cs Converter Output Exception (`LICENSE-EXCEPTION`, an additional
permission under AGPL section 7): the C# it generates does not inherit the AGPL by being generated. The Go release
it downloads is Go's own, under Go's BSD license.
