# nugetgo.github.com.spf13.pflag 1.0.9.2: the wave kit

`github.com/spf13/pflag` v1.0.9, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `1ca308d85f` (branch claude/c2-nugetgo-tools-1261002: the go.* 1.26.10.2 record commit itself) |
| G | `go2cs.converter=exe-44da3d6fb1025e27`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| Proof | 177 matched, 0 disclosed |
| Fresh copy | `conversion/pflag` builds with `-p:GoStdLibVersion=1.26.10.2`; each dll's version, copyright and company equal the packed one's |
| Conversion repository | `ritchiecarroll/pflag-cs`, tag `nuget-1.0.9.2` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
1deef871d51053af539d03484c46ae3606005471159d00e3c15d43465a9a3141  int.1/nugetgo.github.com.spf13.pflag.nupkg
c6f381c5f78b499837a22af48cd5eef2044fc1410212b16608c1216219ecca7e  release/nugetgo.github.com.spf13.pflag.nupkg
```
