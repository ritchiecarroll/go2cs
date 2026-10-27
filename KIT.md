# nugetgo.github.com.spf13.cobra 1.10.2: the wave kit

`github.com/spf13/cobra` v1.10.2, built against go.* 1.26.10.1, packed by C2 on 2026-10-27 on a linux/amd64 cloud host,
without root privileges.

| | |
|:--|:--|
| T | go2cs `ee99292ba8` (branch claude/c2-nugetgo-wave3-cobra-pack before its batch 52 merge; no golib or go2cs-gen change since 1.26.10.1's record commit) |
| G | `go2cs.converter=exe-f489687f8d26d82b`, `go2cs.release=1.26.10.1`, built from T |
| Proof | `github.com/spf13/cobra` 405 matched, 0 disclosed; `doc` 19 of 19, not in the package |
| Dependencies | `nugetgo.github.com.spf13.pflag` 1.0.9.1 and `nugetgo.github.com.inconshreveable.mousetrap` 1.1.0, both restored from nuget.org |
| Conversion repository | `ritchiecarroll/cobra-cs`, tag `nuget-1.10.2` |

`conversion/` is the conversion commit (runbook 4.5) in the MIRRORED layout: the root build files at its root, the
module tree at `src/github.com/spf13/cobra`, and pflag's converted project at `pkg/github.com/spf13/pflag`, which
cobra's project references. It also holds the root README and the license file, VALIDATION.md and proof page taken
out of the release package byte for byte. A fresh copy builds with `dotnet build -c Release
-p:GoStdLibVersion=1.26.10.1` in `src/github.com/spf13/cobra` (36 s, 0 warnings), and its dll's version, copyright and
company equal the packed `runtimes/linux-x64` dll's. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release
candidate (`release/`); their SHA-256:

```
8c7c332a62d9368784ceeef89a8d7be3901b313fb9686ff369a66961865f00d1  int.1/nugetgo.github.com.spf13.cobra.nupkg
3faefbcc950e571882290bf5aee4d0a6600bdc2bd26eb3115f0681473175b95c  release/nugetgo.github.com.spf13.cobra.nupkg
```
