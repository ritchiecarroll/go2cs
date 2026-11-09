# nugetgo.github.com.spf13.cobra 1.10.2.2: the wave kit

`github.com/spf13/cobra` v1.10.2, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | 405 + 0 in the root package and 19 + 0 in `doc` (validated, not packed), the module table's Expect: MET; run without root's file capabilities, on a private writable module cache (runbook Traps, 12). Binary compatibility CLEAN against 1.10.2.1 (apicompat 10.0.403, 5 assets) |
| Edges | packed from a local folder feed holding S1's pflag 1.0.9.3 and mousetrap 1.1.0.2 release candidates (byte-identical to their kits') beside the go.* 1.26.10.3 closure; the nuspec requires each at that version as a bare minimum, and S2's block 3 refuses to push until nuget.org lists both |
| Fresh copy | `conversion/src/github.com/spf13/cobra` builds the packed project with `-p:GoStdLibVersion=1.26.10.3`, and its dll's version, copyright and company equal the packed linux-x64 one's; `doc` does not build from the repository, as its README says |
| Conversion repository | `ritchiecarroll/cobra-cs`, tag `nuget-1.10.2.2` |

`conversion/` is the conversion commit (runbook 4.5) in the MIRRORED layout: the root build files at its root, the
module tree at `src/github.com/spf13/cobra`, and pflag's converted project at `pkg/github.com/spf13/pflag`, which
cobra's project references. It also holds the root README and the license file, VALIDATION.md and proof page taken
out of the release package byte for byte. A fresh copy builds the packed project with `dotnet build -c Release
-p:GoStdLibVersion=1.26.10.3` in `src/github.com/spf13/cobra`, and its dll's version, copyright and company equal the
packed `runtimes/linux-x64` dll's; `doc` does not build there (its imports are not nugetgo packages; the README says so). `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
05e175975545ae674385f99353dc4a6655132eaf981c127406e6079274cbb013  int.1/nugetgo.github.com.spf13.cobra.nupkg
8b8f7d09ce3920b950094c54fa08c23ae5c5e233452f679dd53882c30b4b1fda  release/nugetgo.github.com.spf13.cobra.nupkg
```
