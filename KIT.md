# nugetgo.github.com.inconshreveable.mousetrap 1.1.0.2: the wave kit

`github.com/inconshreveable/mousetrap` v1.1.0, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | no Go tests: four hosted behavior rows (os-matrix run 42583393697, on claude/c2-nugetgo-tools-1261003, the record itself), outputs equal on win-x64, linux-x64, osx-x64 and osx-arm64, each bound by the release pack to the bytes it packs for its RID. Binary compatibility CLEAN against 1.1.0.1 (apicompat 10.0.403, 5 assets) |
| Rows | read from the rehearsal package on ref claude/c2-nugetgo-1261003-mousetrap-int1 (a local folder feed, feed sha256 7755174374641f14ad8847011c6a8466d636adf9114b0b9f13709f2c1297aa3b); fetched with Get-NugetgoBehaviorRows |
| Fresh copy | `conversion/mousetrap` builds with `-p:GoStdLibVersion=1.26.10.3`; its dll's version, copyright and company equal the packed linux-x64 one's |
| Conversion repository | `ritchiecarroll/mousetrap-cs`, tag `nuget-1.1.0.2` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
7755174374641f14ad8847011c6a8466d636adf9114b0b9f13709f2c1297aa3b  int.1/nugetgo.github.com.inconshreveable.mousetrap.nupkg
3edb9379b7fb625eb30327607fbc36f490b786568560cb751641d0f91d9de6f2  release/nugetgo.github.com.inconshreveable.mousetrap.nupkg
```
