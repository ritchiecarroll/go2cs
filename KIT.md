# nugetgo.github.com.google.uuid 1.6.0 rev 4: the wave kit

`github.com/google/uuid` v1.6.0, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | 54 matched, 0 disclosed, in the run that packed this kit. TestVersion6 is disclosed as timing-sensitive; the converted test passed in this run, so the entry is reported as orphaned (the ruled alternative reading; the module table's Expect: MET). The tools rehearsal at T read 53 + 1, the other ruled reading. Binary compatibility: baseline 1.6.0 rev 3 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Fresh copy | `conversion/uuid` builds with `-p:GoStdLibVersion=1.26.11.1`; its dll's version, copyright and company equal the packed one's |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/uuid-cs`, tag `nuget-` plus the package version (1.6.0 rev 4; the previous kit's tag carries 1.6.0 rev 3) |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
90e0ede930fa12d7a6a98dea8a436ca663a338a7a95c02582b44f6e34f522d0c  int.1/nugetgo.github.com.google.uuid.nupkg
52a3f438a5768c8d577592ddea35315f3a3c8c8973588267e31b635842d3edf9  release/nugetgo.github.com.google.uuid.nupkg
```
