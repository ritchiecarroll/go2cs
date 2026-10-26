> unofficial go2cs C# conversion of github.com/inconshreveable/mousetrap v1.1.0, built on the Go 1.26.10 standard library; not affiliated with or endorsed by Alan Shreve or the Go project. Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.26.10; review before any production use.

> Flavors: this package carries RID-specific assemblies; its go2cs/source-metadata.txt describes the linux flavor's package metadata, and the windows and darwin flavors' metadata is not described.

# `github.com/inconshreveable/mousetrap` -- behavioral proof (upstream has no Go tests)

`github.com/inconshreveable/mousetrap` v1.1.0 ships no Go tests, so this page is not a test comparison. Each row below is a sample
program run on that RID's own host: the program converted by go2cs against this package, built for that RID and run
with dotnet, and the Go program run with `go run`, printed the same bytes. Each row names the assembly that loaded,
which is the package's own for that RID, and the host it ran on. No row stands for another RID.

| RID | Host | Assembly | Outputs equal |
|:--|:--|:--|:--|
| win-x64 | windows-latest (image win25-vs2026 20261021.278.1) (Windows X64: Microsoft Windows 10.0.26100) | `github.com.inconshreveable.mousetrap.dll sha256-49e0b468a306b291612feb176990d3addac3dbc21dda7634bbe5c39742d66363` | true (go go1.26.10, dotnet 10.0.402) |
| linux-x64 | ubuntu-latest (image ubuntu24 20261023.334.3) (Linux X64: Ubuntu 24.04.5 LTS) | `github.com.inconshreveable.mousetrap.dll sha256-049267b1dbcfe89eea75600837534650b2ae3979bf8a66959802e19f6cdc44bf` | true (go go1.26.10, dotnet 10.0.402) |
| osx-x64 | macos-15-intel (image macos15 20260824.0482.1) (macOS X64: macOS 15.7.9) | `github.com.inconshreveable.mousetrap.dll sha256-049267b1dbcfe89eea75600837534650b2ae3979bf8a66959802e19f6cdc44bf` | true (go go1.26.10, dotnet 10.0.402) |
| osx-arm64 | macos-15 (image macos15 20260907.0337.1) (macOS ARM64: macOS 15.7.9) | `github.com.inconshreveable.mousetrap.dll sha256-049267b1dbcfe89eea75600837534650b2ae3979bf8a66959802e19f6cdc44bf` | true (go go1.26.10, dotnet 10.0.402) |
