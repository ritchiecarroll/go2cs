# nugetgo.golang.org.x.sync 0.19.0 rev 4: the wave kit

`golang.org/x/sync` v0.19.0, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | errgroup 5 + 0, semaphore 7 + 1, singleflight 12 + 0, syncmap 3 + 0: the module table's Expect, MET. Binary compatibility: baseline 0.19.0 rev 3 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Fresh copy | `conversion/sync` builds the four packed projects with `-p:GoStdLibVersion=1.26.11.1`; each dll's version, copyright and company equal the packed one's |
| Note | the lane's first three runs stopped themselves at the driver's 10 GB free-disk floor; this kit is the run with 17 GB free at its start |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/sync-cs`, tag `nuget-` plus the package version (0.19.0 rev 4; the previous kit's tag carries 0.19.0 rev 3) |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
92929f7931a9515f0db2470386f37e9525d52581993122d1e17630e971bf5711  int.1/nugetgo.golang.org.x.sync.nupkg
4cf6cac28fff5cfce5453b70d87eed8988d896cfba4aabd34d6c0254726c38fc  release/nugetgo.golang.org.x.sync.nupkg
```
