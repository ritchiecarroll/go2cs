# nugetgo.golang.org.x.sync 0.19.0.3: the wave kit

`golang.org/x/sync` v0.19.0, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | errgroup 5 + 0, semaphore 7 + 1, singleflight 12 + 0, syncmap 3 + 0: the module table's Expect, MET. Binary compatibility CLEAN against 0.19.0.2 (apicompat 10.0.403, 4 assets) |
| Fresh copy | `conversion/sync` builds with `-p:GoStdLibVersion=1.26.10.3`; each dll's version, copyright and company equal the packed one's |
| Note | the lane's first run stopped itself in VR at the driver's 10 GB free-disk floor (free fell to 9.9 GB); this kit is the rerun with the driver's own `-FloorGB 5`, whose lowest free disk was 7.6 GB |
| Conversion repository | `ritchiecarroll/sync-cs`, tag `nuget-0.19.0.3` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
d4d40f44e708e64647d0f24a520f254477f2b67e96e2128fa9ea7a48edc24950  int.1/nugetgo.golang.org.x.sync.nupkg
95e73522f9d4d98df5d8e1dcd981ea6511de4029541f7099de4349a99248184d  release/nugetgo.golang.org.x.sync.nupkg
```
