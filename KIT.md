# nugetgo.github.com.spf13.pflag 1.0.9.3: the wave kit

`github.com/spf13/pflag` v1.0.9, rebuilt against go.* 1.26.10.3, packed by C2 on 2026-11-09 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `e1761e9a17` (branch claude/c2-nugetgo-tools-1261003: the go.* 1.26.10.3 record commit itself) |
| G | `go2cs.converter=exe-a47155634ca3e721`, `go2cs.release=1.26.10.3`, built from T |
| Tools | go1.26.10, pwsh 7.6.9, and .NET SDK 10.0.403 (linux-x64), user-local in the lane's scratch directory as `dotnet403/` (owner-approved, 2026-11-09): the archive verified against the SHA-512 in Microsoft's release metadata, archive sha256 `cd7224fe8e8ba57c15f1601439ff8785a1a72b54ae5c89dd5302a32dcfbddf18` |
| Proof | 177 matched, 0 disclosed (the module table's Expect: MET); binary compatibility CLEAN against 1.0.9.2 (apicompat 10.0.403, 5 assets) |
| Fresh copy | `conversion/pflag` builds with `-p:GoStdLibVersion=1.26.10.3`; its dll's version, copyright and company equal the packed one's |
| Conversion repository | `ritchiecarroll/pflag-cs`, tag `nuget-1.0.9.3` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
376015e9ebcb009a7e2aa5569da3e901f01b356146f7c46f0f01a80819458c91  int.1/nugetgo.github.com.spf13.pflag.nupkg
7d08d205d3eaadfe7f97eb49c40d484cbdc0bf9ac60540aa853a0766db18e072  release/nugetgo.github.com.spf13.pflag.nupkg
```
