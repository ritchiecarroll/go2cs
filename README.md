# mousetrap revision 3, int.1: the rehearsal package for the hosted behavior rows

The one `.nupkg` under `nupkg/int.1/` is the unsigned rehearsal package (prerelease label `int.1`) of
`github.com/inconshreveable/mousetrap` v1.1.0, revision 3, packed by P2 on 2026-11-20 against go.* 1.26.11.1 at go2cs
`ffaebde09e` (T'': the 1.26.11.1 record `5dd5cafd5b` plus two tools commits; docs/NugetgoPublish.md, section 2).
Its file name carries the package version, which the hosted row matches against `nugetgo_package`.

SHA-256 of that file: `1b34e6d615aa5c2ad7bbaf1d65b5a172c6b53ed6bbe10f1b341dd387468486fa`

It is never pushed to a gallery. The hosted `nugetgo-behavior` rows read it from this ref as a local folder feed
(runbook section 3, *From a local folder feed*):

```
gh workflow run os-matrix.yml --ref claude/p2-nugetgo-tools-12611 -f stage=nugetgo-behavior \
  -f nugetgo_module=github.com/inconshreveable/mousetrap@v1.1.0 -f nugetgo_package=<the version in the file name> \
  -f nugetgo_feed=git:claude/c2-nugetgo-1261004-mousetrap-int1:nupkg/int.1 -f nugetgo_feed_sha256=<the SHA-256 above>
```

The release candidate is packed after the rows are fetched, and it binds each row's `assembly` line to the bytes it
packs. The package is RID-specific: `lib/net10.0/` carries the windows assembly and `runtimes/<rid>/lib/net10.0/`
carries each RID's (win-x64, linux-x64, osx-x64, osx-arm64, linux-arm64).
