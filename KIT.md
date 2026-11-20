# nugetgo.github.com.itchyny.gojq 0.12.19 rev 1: the wave kit

`github.com/itchyny/gojq` v0.12.19, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | the whole module in one -tests -recurse run: the library root 164 + 0 and cli 914 + 0, the module table's Expect, MET. Binary compatibility: baseline 0.12.19 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Package | the library root only: `cli` validated and excluded (-ExcludePackage), `cmd/gojq` a main package the pack leaves out. One neutral assembly set. 26 go.* dependencies at [1.26.11.1, 1.27.0) and `nugetgo.github.com.itchyny.timefmt-go` 0.1.8 rev 1 as a bare minimum |
| Feed | packed from a local folder feed (runbook section 3, From a local folder feed): S3's timefmt-go 0.1.8 rev 1 release candidate, byte-identical to its kit's, beside 88 go.* 1.26.11.1 packages, the closure of every PackageReference the converted projects carry; the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| Fresh copy | `conversion/src/github.com/itchyny/gojq` builds the packed project with `-p:GoStdLibVersion=1.26.11.1`, and its dll's version, copyright and company equal the packed one's. `cli` and `cmd/gojq` do not build from the repository (their imports, go-yaml and the rest, are not here), as its README says |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/gojq-cs`, tag `nuget-` plus the package version (0.12.19 rev 1; the previous kit's tag carries 0.12.19) |

`conversion/` is the conversion commit (runbook 4.5) in the MIRRORED layout: the root build files at its root, the
module tree at `src/github.com/itchyny/gojq`, and timefmt-go's converted project at
`pkg/github.com/itchyny/timefmt-go`, which gojq's project references. It also holds the root README and the license
file, VALIDATION.md and proof page taken out of the release package byte for byte. The file list equals the 1.26.10.3
wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
2e2fab390c27d7b498f5a439f2214b4973669328cbf730e17d5b3299d326ce6f  int.1/nugetgo.github.com.itchyny.gojq.nupkg
b465fdac09fa10d0ef49127339a4f8984aaf77ee6111fb9d099274591edc3a3e  release/nugetgo.github.com.itchyny.gojq.nupkg
```
