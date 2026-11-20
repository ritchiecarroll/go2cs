# nugetgo.github.com.golang-jwt.jwt.v5 5.3.1 rev 4: the wave kit

`github.com/golang-jwt/jwt/v5` v5.3.1, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | 186 + 0 (root) and 3 + 0 (request), the module table's Expect: MET; `test` excluded by the kit, `cmd/jwt` not packed. Binary compatibility: baseline 5.3.1 rev 3 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Fresh copy | `conversion/jwt` builds both packed projects with `-p:GoStdLibVersion=1.26.11.1`; each dll's version, copyright and company equal the packed one's |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/jwt-cs`, tag `nuget-` plus the package version (5.3.1 rev 4; the previous kit's tag carries 5.3.1 rev 3) |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
536640b0c57d31127a469256b0289ded21d9cd3fa78fe1e0c662d45e82edf3a3  int.1/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
102683c6cbe6538a6512acb8c6927191c5efa8803042153c00c869044299badd  release/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
```
