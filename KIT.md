# nugetgo.github.com.spf13.pflag 1.0.9 rev 4: the wave kit

`github.com/spf13/pflag` v1.0.9, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | 177 matched, 0 disclosed (the module table's Expect: MET). Binary compatibility: baseline 1.0.9 rev 3 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Package | one assembly per runtime: `lib/net10.0/` and `runtimes/<rid>/` for win-x64, linux-x64, linux-arm64, osx-x64 and osx-arm64 (linux-arm64 is new in this wave) |
| Fresh copy | `conversion/pflag` builds with `-p:GoStdLibVersion=1.26.11.1`; its dll's version, copyright and company equal the packed linux-x64 one's |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/pflag-cs`, tag `nuget-` plus the package version (1.0.9 rev 4; the previous kit's tag carries 1.0.9 rev 3) |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
0668915f23f6f2c41f0ed756907a6b67ed9e57fc9dadf724aedadd3ffeaef385  int.1/nugetgo.github.com.spf13.pflag.nupkg
58276a15cd225f56bacf085fa8b41960b084b28f6b0b0196958b100744f368d0  release/nugetgo.github.com.spf13.pflag.nupkg
```
