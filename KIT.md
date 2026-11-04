# nugetgo.github.com.google.uuid 1.6.0.2: the wave kit

`github.com/google/uuid` v1.6.0, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `1ca308d85f` (branch claude/c2-nugetgo-tools-1261002: the go.* 1.26.10.2 record commit itself) |
| G | `go2cs.converter=exe-44da3d6fb1025e27`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| Proof | 54 matched, 0 disclosed, in one unretried run. TestVersion6 is disclosed as timing-sensitive; the converted test passed in this run, so the entry is reported as orphaned (the ruled alternative reading) |
| Fresh copy | `conversion/uuid` builds with `-p:GoStdLibVersion=1.26.10.2`; its dll's version, copyright and company equal the packed one's |
| Conversion repository | `ritchiecarroll/uuid-cs`, tag `nuget-1.6.0.2` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
adad62268b136b37b3039ae255b9b9a1de5ee33b3c545c841611f839eb65fe9c  int.1/nugetgo.github.com.google.uuid.nupkg
94d80673b06888b0733689355627aa5988854a7ce7168698ff9b22c340644284  release/nugetgo.github.com.google.uuid.nupkg
```
