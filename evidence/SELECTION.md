# Consumer selection picks the rebuild

Measured by C2 on 2026-10-10 on the linux lane box, with go2cs built at `T` = `claude/c2-nugetgo-tools` @ `c8ef680e14`.
The sample program is the hashset kit's `app/` (`example.com/hashsetdemo`), and the mapping is the registry row, unchanged:

```
github.com/ritchiecarroll/hashset	nugetgo.github.com.ritchiecarroll.hashset	canonical	https://github.com/ritchiecarroll/hashset-cs	2026-10-09	owner
```

The row maps the module to the package ID, never to a version, so the rebuild needs no registry change. B3's
tie-break picks the highest revision built for the running converter's corpus among the package versions whose
self-description matches the required module version (v1.0.0).

## The measurement

`-nuget-map-feed` pointed at a local folder holding BOTH `nugetgo.github.com.ritchiecarroll.hashset.1.0.0.nupkg` (the
hashset kit's release candidate, the bytes signed and published as 1.0.0) and this kit's
`nugetgo.github.com.ritchiecarroll.hashset.1.0.0.1.nupkg`:

```
go2cs -recurse=nuget -nuget-map mappings.txt -nuget-map-only -nuget-map-feed <folder> -go2cspath <T>/src app AR
```

| Reading | Value |
|:--|:--|
| exit code | 0 |
| provenance report | `1 third-party module(s), 1 mapped, 1 referenced as packages` |
| the mapped module | `github.com/ritchiecarroll/hashset@v1.0.0  ->  nugetgo.github.com.ritchiecarroll.hashset  canonical  PackageReference nugetgo.github.com.ritchiecarroll.hashset [1.0.0.1]` |
| `go2cs.nuget.lock` | pins `1.0.0.1`, mapping source `../mappings.txt` |
| local conversion of the module | none: `AR/src` holds only `example.com`, and there is no `AR/pkg` |

**Control:** the same command over a folder holding 1.0.0 alone selects `[1.0.0]`. The rebuild is chosen because it
is present, not by a fixed rule.

**The program:** restored with a private `NUGET_PACKAGES` (`nugetgo.*` from that folder, everything else from
nuget.org) and run with `dotnet run -c Release -p:UseSharedCompilation=false`: exit 0, and the output equals the
kit's `go run` output byte for byte (6 lines). The restored package folder holds `1.0.0.1` alone.
