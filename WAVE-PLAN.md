# Post-hop wave plan (nugetgo)

For after go.* 1.26.x is published and its record is on master (that record is `T`).

Every published module is rebuilt against go.* 1.26.x. Its stdlib range moves from [1.24.13.x, 1.25.0) to
[1.26.x.y, 1.27.0), which B4 says takes a new revision. Versions under B3 (rebuilds are X.Y.Z.N, the revision rises
with the corpus; "rev N" below is the fourth part N of the package version `X.Y.Z.N`), read from nuget.org today:

| # | module@version | published now | next | proof kind |
|:--|:--|:--|:--|:--|
| 1 | github.com/ritchiecarroll/hashset@v1.0.0 | 1.0.0, 1.0.0 rev 2 | 1.0.0 rev 3 | Go tests |
| 2 | github.com/google/uuid@v1.6.0 | 1.6.0 | 1.6.0 rev 1 | Go tests |
| 3 | github.com/golang-jwt/jwt/v5@v5.3.1 | 5.3.1 | 5.3.1 rev 1 | Go tests |
| 4 | golang.org/x/sync@v0.19.0 | 0.19.0 | 0.19.0 rev 1 | Go tests (+1 disclosed, F3) |
| 5 | github.com/spf13/pflag@v1.0.9 | 1.0.9 | 1.0.9 rev 1 | Go tests |
| 6 | github.com/inconshreveable/mousetrap@v1.1.0 | none | 1.1.0 | behavior rows, 4 RIDs |
| 7 | github.com/spf13/cobra@v1.10.2 | none | 1.10.2 | Go tests, -ExcludePackage cobra/doc |

ORDER
- go.* 1.26.x is published first. Every nugetgo pack resolves its go.* closure from nuget.org and refuses an
  unpublished go.lib/go.gen, so no nugetgo package can be packed before that release is listed.
- 1 to 5 depend only on go.*; any order.
- 6 needs its four hosted rows, which run against mousetrap 1.1.0-int.1 on int.nugettest.org (the owner's unsigned
  int push, API key, no PIN), before its release candidate can be packed.
- 7 last. Its pack resolves pflag 1.0.9 rev 1 and mousetrap 1.1.0 from nuget.org, and B4 makes a third-party edge take
  the first revision built for the same corpus, so cobra cannot be packed until 5 and 6 are listed.

PIN SESSIONS
release-nuget.ps1 signs a whole folder inside one `dotnet nuget sign` process, and the card caches the PIN for that
process only. So one PIN signs every nupkg that exists when the session starts. The dependency order gives:
- Session 0: the go.* 1.26.x release itself (release-nuget.ps1, as for every corpus release).
- Session 1: packages 1 to 6 together, six nupkgs, one PIN. Precondition: 6's rows are fetched.
- Session 2: cobra alone, after 5 and 6 are listed on nuget.org.
Three PIN sessions in all, two of them for nugetgo.
