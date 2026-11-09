# nugetgo.github.com.google.uuid 1.6.0.3: the wave kit

`github.com/google/uuid` v1.6.0, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | 54 matched, 0 disclosed, in the run that packed this kit. TestVersion6 is disclosed as timing-sensitive; the converted test passed in this run, so the entry is reported as orphaned (the ruled alternative reading; the module table's Expect: MET). Binary compatibility CLEAN against 1.6.0.2 (apicompat 10.0.403, 1 asset) |
| Fresh copy | `conversion/uuid` builds with `-p:GoStdLibVersion=1.26.10.3`; its dll's version, copyright and company equal the packed one's |
| Note | the lane's first run (07:00Z) read 53 + 1, MET, and its release pack was refused because C2's own fresh-build check of another kit restored into the shared global packages folder while it ran (the pack's "global packages folder GREW" guard); this kit is the clean rerun |
| Conversion repository | `ritchiecarroll/uuid-cs`, tag `nuget-1.6.0.3` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
5200e940b90874677156ded748eb583b9b50af9fdc7ec31d063f911d88b2a95b  int.1/nugetgo.github.com.google.uuid.nupkg
91e93d9fab5e88e57c85a98711f0e78728205d3fc3e9f6fe004b7d2c92e1eb01  release/nugetgo.github.com.google.uuid.nupkg
```
