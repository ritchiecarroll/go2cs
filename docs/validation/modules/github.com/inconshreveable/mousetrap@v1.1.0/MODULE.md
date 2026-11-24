# `github.com/inconshreveable/mousetrap` -- behavioral proof (upstream has no Go tests)

`github.com/inconshreveable/mousetrap` v1.1.0 ships no Go tests, so this page is not a test comparison. Each row below is a sample
program run on that RID's own host: the program converted by go2cs against this package, built for that RID and run
with dotnet, and the Go program run with `go run`, printed the same bytes. Each row names the assembly that loaded,
which is the package's own for that RID, and the host it ran on. No row stands for another RID.

| RID | Host | Assembly | Outputs equal |
|:--|:--|:--|:--|
| win-x64 | windows-latest (image win25-vs2026 20261103.298.1) (Windows X64: Microsoft Windows 10.0.26100) | `github.com.inconshreveable.mousetrap.dll sha256-562067734645376c62a57c7def437f3383f94cbcb58b995c89eb8f55b650fbf0` | true (go go1.26.11, dotnet 10.0.404) |
| linux-x64 | ubuntu-latest (image ubuntu26 20261111.196.1) (Linux X64: Ubuntu 26.04.1 LTS) | `github.com.inconshreveable.mousetrap.dll sha256-a756271c0bf5c6b4f00351648c066f7dead309c53366c78597b1abf076377356` | true (go go1.26.11, dotnet 10.0.404) |
| osx-x64 | macos-15-intel (image macos15 20260824.0482.1) (macOS X64: macOS 15.7.9) | `github.com.inconshreveable.mousetrap.dll sha256-a756271c0bf5c6b4f00351648c066f7dead309c53366c78597b1abf076377356` | true (go go1.26.11, dotnet 10.0.404) |
| osx-arm64 | macos-15 (image macos15 20261104.0514.1) (macOS ARM64: macOS 15.8.2) | `github.com.inconshreveable.mousetrap.dll sha256-a756271c0bf5c6b4f00351648c066f7dead309c53366c78597b1abf076377356` | true (go go1.26.11, dotnet 10.0.404) |
| linux-arm64 | ubuntu-24.04-arm (image ubuntu24-arm64 20261111.182.1) (Linux ARM64: Ubuntu 24.04.5 LTS) | `github.com.inconshreveable.mousetrap.dll sha256-a756271c0bf5c6b4f00351648c066f7dead309c53366c78597b1abf076377356` | true (go go1.26.11, dotnet 10.0.404) |
