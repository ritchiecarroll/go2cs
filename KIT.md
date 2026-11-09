# nugetgo.github.com.itchyny.gojq 0.12.19: the wave kit

`github.com/itchyny/gojq` v0.12.19, converted against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | the whole module in one -tests -recurse run: the library root 164 + 0 and cli 914 + 0, the module table's Expect, MET. The root's 164 was a PREDICTION (P1 read 162 of 164 before class E, batch 246); it HELD, and its falsifier (either OptimizeTailRec test failing) did not fire. Binary compatibility: no published revision before this one, a new package ID (reported, not gated) |
| Package | the library root only: `cli` validated and excluded (-ExcludePackage), `cmd/gojq` a main package the pack leaves out. One neutral assembly set (the three flavors agree). 27 dependencies: each go.* at [1.26.10.3, 1.27.0) and `nugetgo.github.com.itchyny.timefmt-go` 0.1.8 as a bare minimum |
| Feed | packed from a local folder feed (runbook section 3, From a local folder feed): S3's timefmt-go 0.1.8 release candidate, byte-identical to its kit's, beside the 88 go.* 1.26.10.3 packages of the closure; the pack's closure check read every go.* package restored as the published 1.26.10.3 |
| Fresh copy | `conversion/src/github.com/itchyny/gojq` builds the packed project with `-p:GoStdLibVersion=1.26.10.3`, and its dll's version, copyright and company equal the packed one's. `cli` and `cmd/gojq` do not build from the repository (their imports, go-yaml and the rest, are not here), as its README says |
| First publish | a NEW package ID on nuget.org (404 at 2026-11-08); its conversion repository is created empty |
| Conversion repository | `ritchiecarroll/gojq-cs`, tag `nuget-0.12.19` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
3ae4e16b7a20bcf59129b74fdf9de4edba1bf72fad86de79c05f661440bdd7d8  int.1/nugetgo.github.com.itchyny.gojq.nupkg
05a4a71d1422aea07ff7d259a5d02afdeb3adec92c78534866c5648c6040299c  release/nugetgo.github.com.itchyny.gojq.nupkg
```
