> unofficial go2cs C# conversion of github.com/inconshreveable/mousetrap v1.1.0, built on the Go 1.26.10 standard library; not affiliated with or endorsed by Alan Shreve or the Go project. Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.26.10; review before any production use.

> Flavors: this package carries RID-specific assemblies; its go2cs/source-metadata.txt describes the linux flavor's package metadata, and the windows and darwin flavors' metadata is not described.

# `github.com/inconshreveable/mousetrap` -- behavioral proof (upstream has no Go tests)

`github.com/inconshreveable/mousetrap` v1.1.0 ships no Go tests, so this page is not a test comparison. Each row below is a sample
program run on that RID's own host: the program converted by go2cs against this package, built for that RID and run
with dotnet, and the Go program run with `go run`, printed the same bytes. Each row names the assembly that loaded,
which is the package's own for that RID, and the host it ran on. No row stands for another RID.

| RID | Host | Assembly | Outputs equal |
|:--|:--|:--|:--|
| win-x64 | windows-latest (image win25-vs2026 20261030.294.1) (Windows X64: Microsoft Windows 10.0.26100) | `github.com.inconshreveable.mousetrap.dll sha256-1119dcdae0510d28770163ed65d5de181ff8e54bb64aaa36589f85ec78607e84` | true (go go1.26.10, dotnet 10.0.403) |
| linux-x64 | ubuntu-latest (image ubuntu26 20261026.180.1) (Linux X64: Ubuntu 26.04.1 LTS) | `github.com.inconshreveable.mousetrap.dll sha256-df518d2a9ce0d52343f014444509cbb250a6e7d513ef632da67c4a6befda47de` | true (go go1.26.10, dotnet 10.0.403) |
| osx-x64 | macos-15-intel (image macos15 20260824.0482.1) (macOS X64: macOS 15.7.9) | `github.com.inconshreveable.mousetrap.dll sha256-df518d2a9ce0d52343f014444509cbb250a6e7d513ef632da67c4a6befda47de` | true (go go1.26.10, dotnet 10.0.403) |
| osx-arm64 | macos-15 (image macos15 20261028.0489.1) (macOS ARM64: macOS 15.8.2) | `github.com.inconshreveable.mousetrap.dll sha256-df518d2a9ce0d52343f014444509cbb250a6e7d513ef632da67c4a6befda47de` | true (go go1.26.10, dotnet 10.0.403) |
