# nugetgo.github.com.golang-jwt.jwt.v5 5.3.1.2: the wave kit

`github.com/golang-jwt/jwt/v5` v5.3.1, rebuilt against go.* 1.26.10.2, packed by C2 on 2026-11-04 on a linux/amd64 cloud host.

| | |
|:--|:--|
| T | go2cs `5af6605256` (branch claude/c2-nugetgo-tools-1261002-jwt): the record `1ca308d85f` plus two converter fixes cherry-picked (claude/c2-nugetgo-revision-rehearsal, claude/c2-fieldaccess-fieldtypes); golib, gen and src/tools identical to the record. At the record itself the root package does not validate: jwt.tests fails CS0012 on `claims.Issuer` |
| G | `go2cs.converter=exe-06cef13a977c0d2b`, `go2cs.release=1.26.10.2`, built from T |
| Tools | go1.26.10, .NET SDK 10.0.112, pwsh 7.6.9 |
| G vs G2 | COORD's condition (2026-11-05): jwt packed with T's G (`exe-44da3d6fb1025e27`, an unvalidated rehearsal from a G-converted PR) carries byte-identical production assemblies: `github.com.golang-jwt.jwt.v5.dll` `ebbef669e3c184310d1c75ca85335772d48e895ce3fd48be9047f13016d1dc69` and `github.com.golang-jwt.jwt.v5.request.dll` `b60f9c5c743f839be9469977a7dec7907d650195b2fe95c073fb39b334ec61a9`, equal to this release candidate's |
| Proof | 186 + 0 (root) and 3 + 0 (request); `test` excluded by the kit, `cmd/jwt` not packed |
| Fresh copy | `conversion/jwt` builds with `-p:GoStdLibVersion=1.26.10.2`; each packed dll's version, copyright and company equal the packed one's |
| Conversion repository | `ritchiecarroll/jwt-cs`, tag `nuget-5.3.1.2` |

`conversion/` is the conversion commit (runbook 4.5): the module tree as the release pack left it, the root build
files, the root README, and the license file, VALIDATION.md and proof pages taken out of the release package byte for
byte. `nupkg/` holds the unsigned rehearsal (`int.1/`) and release candidate (`release/`); their SHA-256:

```
df4329ade812597d1a723a7c966a4e9aeaaa821d4c1d32a754502462c3865d17  int.1/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
e42b138bab2360939127dcfb3ea55b5fb6a8c0ec9a9830c9b385e457a1618d82  release/nugetgo.github.com.golang-jwt.jwt.v5.nupkg
```
