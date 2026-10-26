# mousetrap 1.1.0-int.1: the rehearsal package for the hosted behavior rows

`nupkg/int.1/nugetgo.github.com.inconshreveable.mousetrap.1.1.0-int.1.nupkg` is the unsigned rehearsal package of
`github.com/inconshreveable/mousetrap` v1.1.0, packed by C2 on 2026-10-26 against go.* 1.26.10.1 (docs/NugetgoPublish.md,
section 2). `nupkg/SHA256SUMS` holds its SHA-256.

It goes to int.nugettest.org only, never to nuget.org (runbook section 3, step 1). The owner pushes it with the test
gallery's own API key:

```
dotnet nuget push nupkg/int.1/nugetgo.github.com.inconshreveable.mousetrap.1.1.0-int.1.nupkg --source https://apiint.nugettest.org/v3/index.json
```

The hosted `nugetgo-behavior` rows then run against `1.1.0-int.1` (runbook section 3, *The hosted rows*). The release
candidate is packed after the rows are fetched, and it binds each row's `assembly` line to the bytes it packs.

The package is RID-specific: the windows flavor differs from the others, so `lib/net10.0/` carries the windows
assembly and `runtimes/<rid>/lib/net10.0/` carries each RID's (win-x64, linux-x64, osx-x64, osx-arm64).
