# nugetgo.github.com.spf13.cobra 1.10.2 rev 3: the wave kit

`github.com/spf13/cobra` v1.10.2, rebuilt against go.* 1.26.11.1, packed by P2 on 2026-11-20 on a linux/amd64 cloud host. A package version is written here as the module's version and its revision, the fourth part of the NuGet version ("1.6.0 rev 4"); the READMEs under `conversion/` spell it in full (COORD ruling A, 2026-11-20).

| | |
|:--|:--|
| T | go2cs `ffaebde09e` (branch claude/p2-nugetgo-tools-12611): the go.* 1.26.11.1 record commit `5dd5cafd5b` with two tools commits on top, `ba63056291` (the binary-compatibility guard reports a baseline built below go.lib's break floor instead of comparing it: COORD ruling A, 2026-11-19) and `ffaebde09e` (the converted-by badge image served from cdn.jsdelivr.net). The runtime and the source generators are the record's |
| G | `go2cs.converter=exe-a7fa14af9da78692`, `go2cs.release=1.26.11.1`, built from T |
| Tools | go1.26.11, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64) |
| Proof | 405 + 0 in the root package and 19 + 0 in `doc` (validated, not packed), the module table's Expect: MET. Run without root's file-permission override (runbook Traps, 12): as uid 0 under `setpriv --inh-caps=-dac_override,-dac_read_search --bounding-set=-dac_override,-dac_read_search`, which makes root get EACCES on a 0400 file, with a private module cache filled first and made writable (`chmod -R u+w`). Binary compatibility: baseline 1.10.2 rev 2 built below the break floor 1.26.11.1 (go.lib [1.26.10.3, 1.27.0)), so not compared: reported, not gated (COORD ruling A, 2026-11-19) |
| Edges | packed from a local folder feed holding S1's pflag 1.0.9 rev 4 and mousetrap 1.1.0 rev 3 release candidates (byte-identical to their kits') beside 95 go.* 1.26.11.1 packages, the closure of every PackageReference the converted projects carry (go.gen included); the nuspec requires pflag and mousetrap at those versions as a bare minimum, and S2's block 3 refuses to push until nuget.org lists both |
| Package | one assembly per runtime: `lib/net10.0/` and `runtimes/<rid>/` for win-x64, linux-x64, linux-arm64, osx-x64 and osx-arm64 (linux-arm64 is new in this wave) |
| Fresh copy | `conversion/src/github.com/spf13/cobra` builds the packed project with `-p:GoStdLibVersion=1.26.11.1`, and its dll's version, copyright and company equal the packed linux-x64 one's; `doc` does not build from the repository, as its README says |
| Note | the lane's first cobra run validated MET and its rehearsal pack failed restore (NU1101: go.gen), because the lane's feed lacked go.gen, which the projects reference directly and no nuspec lists; the feed was rebuilt from the projects' references and this kit is the rerun |
| Ranges | every go.* dependency at [1.26.11.1, 1.27.0); the pack's closure check read every go.* package restored as the published 1.26.11.1 |
| README | the package README carries the converted-by badge from cdn.jsdelivr.net once and the old image address zero times, in both packs |
| Conversion repository | `ritchiecarroll/cobra-cs`, tag `nuget-` plus the package version (1.10.2 rev 3; the previous kit's tag carries 1.10.2 rev 2) |

`conversion/` is the conversion commit (runbook 4.5) in the MIRRORED layout: the root build files at its root, the
module tree at `src/github.com/spf13/cobra`, and pflag's converted project at `pkg/github.com/spf13/pflag`, which
cobra's project references. It also holds the root README and the license file, VALIDATION.md and proof page taken
out of the release package byte for byte. The file list equals the 1.26.10.3 wave's kit, file for file. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
e6f47b4c0406464e503cd9b31e1766e85c6080e7603ed91f54a0696c72f9f3eb  int.1/nugetgo.github.com.spf13.cobra.nupkg
22bd44ecba541c73e2829aff6f1ef1994330901972d6b07de5c810f21a8a0602  release/nugetgo.github.com.spf13.cobra.nupkg
```
