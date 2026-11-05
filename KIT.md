# nugetgo.github.com.inconshreveable.mousetrap 1.1.0.1: the wave kit

`github.com/inconshreveable/mousetrap` v1.1.0, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `1ca308d85f` (branch claude/c2-nugetgo-tools-1261002: the go.* 1.26.10.2 record commit itself) |
| G | `go2cs.converter=exe-44da3d6fb1025e27`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| Proof | no Go tests: four hosted behavior rows (os-matrix run 41938519165, on claude/c2-nugetgo-tools-1261002-rows: T plus the rehearsal-selection fix, which the rows need; a row binds the packed bytes, never the converter), outputs equal on win-x64, linux-x64, osx-x64 and osx-arm64, each bound to the assembly the release candidate packs for its RID |
| Fresh copy | `conversion/mousetrap` builds with `-p:GoStdLibVersion=1.26.10.2`; its dll's version, copyright and company equal the packed linux-x64 one's |
| Conversion repository | `ritchiecarroll/mousetrap-cs`, tag `nuget-1.1.0.1` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
13e924e8d363012041e87aef2b62e65f452498dedf8ec12ce533b71df886c413  int.1/nugetgo.github.com.inconshreveable.mousetrap.nupkg
1c7e22e5fd5dddab99456f848d63382854c0a03badee9eca510df328e4df8ad1  release/nugetgo.github.com.inconshreveable.mousetrap.nupkg
```
