# nugetgo.github.com.golang-jwt.jwt.v5 5.3.1.3: the wave kit

`github.com/golang-jwt/jwt/v5` v5.3.1, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | 186 + 0 (root) and 3 + 0 (request), the module table's Expect: MET; `test` excluded by the kit, `cmd/jwt` not packed. Binary compatibility CLEAN against 5.3.1.2 (apicompat 10.0.403, 2 assets) |
| Fresh copy | `conversion/jwt` builds with `-p:GoStdLibVersion=1.26.10.3`; each packed dll's version, copyright and company equal the packed one's |
| Note | the lane's first run stopped itself in VR at the driver's 10 GB free-disk floor (free fell to 10.0 GB); this kit is the rerun with the driver's own `-FloorGB 5`, whose lowest free disk was 8.8 GB |
| Conversion repository | `ritchiecarroll/jwt-cs`, tag `nuget-5.3.1.3` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
a9818aab62d8682889492939ff8f85469b522c87bc6c14685dc8f3ad89e6245b  int.1/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
c54c92307302ae3208d70ed27ede762aa41462b0974a40d2c74ccbb67f1d90ae  release/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
```
