# nugetgo.github.com.inconshreveable.mousetrap 1.1.0 rev 3: the wave kit

`github.com/inconshreveable/mousetrap` v1.1.0, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | no Go tests: five hosted behavior rows (os-matrix run 44643331991, on claude/p2-nugetgo-tools-12611 at T), outputs equal on win-x64, linux-x64, linux-arm64, osx-x64 and osx-arm64, each bound by the release pack to the bytes it packs for its RID (linux-arm64 is new in this wave). Binary compatibility: baseline 1.1.0 rev 2 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Rows | read from the rehearsal package on ref claude/c2-nugetgo-1261004-mousetrap-int1 (`553f43acec`; a local folder feed, package sha256 1b34e6d615aa5c2ad7bbaf1d65b5a172c6b53ed6bbe10f1b341dd387468486fa, the bytes of this kit's `int.1/`); fetched with Get-NugetgoBehaviorRows |
| Fresh copy | `conversion/mousetrap` builds with `-p:GoStdLibVersion=1.26.11.1`; its dll's version, copyright and company equal the packed linux-x64 one's |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/mousetrap-cs`, tag `nuget-` plus the package version (1.1.0 rev 3; the previous kit's tag carries 1.1.0 rev 2) |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
1b34e6d615aa5c2ad7bbaf1d65b5a172c6b53ed6bbe10f1b341dd387468486fa  int.1/nugetgo.github.com.inconshreveable.mousetrap.nupkg
1f0539c95c9076eac6bbfbe15a62da446acdf2e8950d65e4797b36ea752f8bd5  release/nugetgo.github.com.inconshreveable.mousetrap.nupkg
```
