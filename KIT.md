# nugetgo.golang.org.x.sync 0.19.0.2: the wave kit

`golang.org/x/sync` v0.19.0, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `1ca308d85f` (branch claude/c2-nugetgo-tools-1261002: the go.* 1.26.10.2 record commit itself) |
| G | `go2cs.converter=exe-44da3d6fb1025e27`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| Proof | 27 matched, 1 disclosed: errgroup 5 + 0, semaphore 7 + 1 (TestWeightedAcquire, linux, the runtime-capability divergence disclosed on .1), singleflight 12 + 0, syncmap 3 + 0 |
| Fresh copy | `conversion/sync` builds with `-p:GoStdLibVersion=1.26.10.2`; each dll's version, copyright and company equal the packed one's |
| Conversion repository | `ritchiecarroll/sync-cs`, tag `nuget-0.19.0.2` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
d211506efd5694bef89373ef070d5106a71e0d11f679a3ea0e67fdb2a27549c3  int.1/nugetgo.golang.org.x.sync.nupkg
7edae28b06f544926af4b2f91d51e708304a4d4c0b0fde77a2e67760ce8be292  release/nugetgo.golang.org.x.sync.nupkg
```
