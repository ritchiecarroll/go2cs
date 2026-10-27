# nugetgo.github.com.google.uuid 1.6.0.1: the wave kit

`github.com/google/uuid` v1.6.0, rebuilt against go.* 1.26.10.1, packed by C2 on 2026-10-27 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `c992731142` (branch claude/c2-nugetgo-wave3-uuid-disclosure) |
| G | `go2cs.converter=exe-259714b353b34a67`, `go2cs.release=1.26.10.1` |
| Proof | 54 matched, 0 disclosed, in one unretried run. TestVersion6 is disclosed as timing-sensitive; the converted test passed in this run, so the entry is reported as orphaned |
| Conversion repository | `ritchiecarroll/uuid-cs`, tag `nuget-1.6.0.1` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
22e0ec4e71149e4d206954f3bc56fbed9fd94c0648331ab589fb23e0c337ab70  int.1/nugetgo.github.com.google.uuid.nupkg
09e0692a7aa7e0287a4b56e0f85285b928824ac9267c2df8783dd804f02ae77f  release/nugetgo.github.com.google.uuid.nupkg
```
