# mousetrap 1.1.0.1-int.1: the rehearsal package for the hosted behavior rows

`nupkg/int.1/nugetgo.github.com.inconshreveable.mousetrap.1.1.0.1-int.1.nupkg` is the unsigned rehearsal package of
`github.com/inconshreveable/mousetrap` v1.1.0, packed by C2 on 2026-11-04 against go.* 1.26.10.2 at go2cs `1ca308d85f`
(the 1.26.10.2 record; docs/NugetgoPublish.md, section 2). `nupkg/SHA256SUMS` holds its SHA-256.

It is never pushed to a gallery. The hosted `nugetgo-behavior` rows read it from this ref as a local folder feed
(runbook section 3, *From a local folder feed*):

```
gh workflow run os-matrix.yml --ref claude/c2-nugetgo-tools-1261002 -f stage=nugetgo-behavior \
  -f nugetgo_module=github.com/inconshreveable/mousetrap@v1.1.0 -f nugetgo_package=1.1.0.1-int.1 \
  -f nugetgo_feed=git:claude/c2-nugetgo-1261002-mousetrap-int1:nupkg/int.1 -f nugetgo_feed_sha256=<the SHA-256 in nupkg/SHA256SUMS>
```

The release candidate is packed after the rows are fetched, and it binds each row's `assembly` line to the bytes it
packs. The package is RID-specific: the windows flavor differs from the others, so `lib/net10.0/` carries the windows
assembly and `runtimes/<rid>/lib/net10.0/` carries each RID's (win-x64, linux-x64, osx-x64, osx-arm64).
