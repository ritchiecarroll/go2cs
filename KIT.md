# nugetgo.github.com.spf13.cobra 1.10.2.1: the wave kit

`github.com/spf13/cobra` v1.10.2, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `1ca308d85f` (branch claude/c2-nugetgo-tools-1261002: the go.* 1.26.10.2 record commit itself) |
| G | `go2cs.converter=exe-44da3d6fb1025e27`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| Proof | `github.com/spf13/cobra` 405 matched, 0 disclosed, run without root file capabilities (cap_dac_override and cap_dac_read_search dropped) and a writable private module cache; `doc` 19 of 19, not in the package |
| Dependencies | `nugetgo.github.com.spf13.pflag` 1.0.9.2 and `nugetgo.github.com.inconshreveable.mousetrap` 1.1.0.1, packed from a local folder feed of S1's release candidates beside the go.* 1.26.10.2 closure |
| Conversion repository | `ritchiecarroll/cobra-cs`, tag `nuget-1.10.2.1` |

`conversion/` is the conversion commit (runbook 4.5) in the MIRRORED layout: the root build files at its root, the
module tree at `src/github.com/spf13/cobra`, and pflag's converted project at `pkg/github.com/spf13/pflag`, which
cobra's project references. It also holds the root README and the license file, VALIDATION.md and proof page taken
out of the release package byte for byte. A fresh copy builds the packed project with `dotnet build -c Release
-p:GoStdLibVersion=1.26.10.2` in `src/github.com/spf13/cobra`, and its dll's version, copyright and company equal the
packed `runtimes/linux-x64` dll's; `doc` does not build there (its imports are not nugetgo packages; the README says so). `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
e2e1f0287db6c0c3f86488331085a633a1c7eb30c74374c64b70c55a5f91604d  int.1/nugetgo.github.com.spf13.cobra.nupkg
c8874815af5f4916aeae1dcad00f86344874bd65b30e5276f9c1929751ee839f  release/nugetgo.github.com.spf13.cobra.nupkg
```
