# PLAN — the nugetgo first wave: hashset, uuid and jwt/v5

> **STATUS: DRAFT (C2, 2026-10-09). Its four open questions were RULED by COORD on 2026-10-09 (section 5).**
> Document type: an [instance plan](Glossary.md#plan). It supplies the first wave's values, readings, sequencing
> and rulings. The procedure is the runbook [`NugetgoPublish.md`](NugetgoPublish.md), referenced by section
> number and never restated here. The wave itself is owner ruling B8 in [`PLAN-nugetgo.md`](PLAN-nugetgo.md),
> section 8, and the order follows the [Roadmap](Roadmap.md): hashset leads, then uuid and jwt. When the wave
> executes, this plan becomes a record.

## 1. Sequencing

The order COORD set on 2026-10-09: TRAIN FL lands, the face-lift go.* release `R` = 1.24.13.5 publishes, then
hashset packs against `R`. The nugetgo pack guards (cuts 1-8 on the `claude/c2-nugetgo-*` branches) seat in the train
AFTER that release, so the release tag does not carry them.

**`T` (RULED, COORD 2026-10-09):** `R`'s record commit, where `corpus-release.txt` names 1.24.13.5, with cuts 1-8
merged onto it, as the branch `claude/c2-nugetgo-tools`. Master is not `T`, because the next train's golib differs from
the release's (G's trim stack). C2 cuts it with `src/tools/nugetgo/New-NugetgoToolsTree.ps1` after the record commit
exists, and announces it.

**The merge, rehearsed (2026-10-09, linux, on `56f0f1f254`, the eight branches' common base):** the script merged
cuts 1-8 cleanly (local branch `claude/c2-nugetgo-tools-rehearsal` @ `cc6b6a38fd`, not pushed). golib and the
generators read byte-identical to the base, all five nugetgo suites passed with no SKIP line, and `go test ./...` and
`go vet` passed on the merged tree. Three refusal arms
named their cause: a `-Release` the record commit does not read; an existing branch; and a planted one-file golib
change, which merged cleanly and was then refused by the dialect gate.

Then, per module, runbook sections 1 to 6:

| Step | hashset | uuid | jwt/v5 |
|:--|:--|:--|:--|
| 1-3 (convert, validate, pack, rehearse) | first | after hashset's section 3 is green | after hashset's section 3 is green |
| 4 (nuget.org) | first | after hashset's section 6 | after hashset's section 6 |
| 5 (registry row) | row #1 | its own pull request | its own pull request |
| 6 (demo) | the launch demo | after its row merges | after its row merges |

uuid and jwt/v5 may run sections 1-3 in parallel, on two boxes or two output roots, never two conversions into one
root. Neither goes to nuget.org before hashset's demo has passed, so that a defect the demo finds is fixed once,
before three packages carry it.

## 2. The modules

Identities read with `Get-NugetgoPackageId` and `Get-NugetgoVersion` at `56f0f1f254`. The nuget.org column was read
from the flat container on 2026-10-09.

| | hashset | uuid | jwt/v5 |
|:--|:--|:--|:--|
| `M` | `github.com/ritchiecarroll/hashset` | `github.com/google/uuid` | `github.com/golang-jwt/jwt/v5` |
| `V` | `v1.0.0` | `v1.6.0` | `v5.3.1` |
| `ID` | `nugetgo.github.com.ritchiecarroll.hashset` | `nugetgo.github.com.google.uuid` | `nugetgo.github.com.golang-jwt.jwt.v5` |
| `PV` (B3, revision 0) | `1.0.0` | `1.6.0` | `5.3.1` |
| `ID` on nuget.org | not held (404) | not held (404) | not held (404) |
| Library packages packed | 1 | 1 | 2: the root and `request`. `test` is left out with `-ExcludePackage github.com/golang-jwt/jwt/v5/test` (COORD ruling: it holds jwt's own test fixtures, not API a consumer imports). `cmd/jwt` is a `main`, which the pack always leaves out. |
| Third-party dependencies | none | none (`go.mod` has no `require`) | none (`go.mod` has no `require`) |
| Description form (B6) | the author's (`-UpstreamPublishes`) | third-party | third-party |
| Upstream LICENSE holders | The go2cs Authors | Google Inc. | Dave Grijalva; golang-jwt maintainers |
| Conversion repository (`RepositoryUrl`) | `https://github.com/ritchiecarroll/hashset-cs` | `https://github.com/ritchiecarroll/uuid-cs` (COORD ruling 2026-10-09: the existing repository) | `https://github.com/ritchiecarroll/jwt-cs` (the same ruling) |
| Registry status | `canonical` (the repository is under the module's org) | `community` | `community` |
| Conversion repository layout (runbook 4.5, COORD ruling) | flat: the pack writes no module `Directory.Build.targets` (the holder is The go2cs Authors) | nested: the pack writes a module `Directory.Build.targets` (holder Google Inc.), so the root build files sit one directory above the module's files | nested, for the same reason (holders Dave Grijalva and the golang-jwt maintainers) |
| Validation reading | 37 matched, 0 disclosed, 5 Example declarations excluded (the 2026-10-09 rehearsal at `56f0f1f254`) | 54 matched, 0 disclosed, 1 package, `windows/amd64` and `linux/amd64` (`ValidatedModules.md`, banked at `05d40930aa`) | 189 matched, 0 disclosed, 2 packages (root, `request`), `windows/amd64` and `linux/amd64` (`ValidatedModules.md`, banked at `05d40930aa`); `test` and `cmd/jwt` have no tests |

Every reading above is re-read at `T` in runbook section 1; none is carried forward.

**The conversion repositories for uuid and jwt/v5 already exist** (COORD ruling, 2026-10-09). COORD created
`uuid-cs` and `jwt-cs` on 2026-09-30, on the owner's authorization, as the conversion-source repositories, each with a
README, the upstream LICENSE verbatim and a `.gitattributes` (`uuid-cs`'s reads `* text=auto eol=lf`). The conversion lands on top of the
existing `main` as one new commit in the nested layout: the README is replaced by the runbook's README, which keeps
its "unofficial" and "not affiliated" sentences, and the LICENSE stays as it is. COORD pushes that commit, so the
owner's section 4 for these two modules is sign, push and poll. The generated files are CRLF and the repositories
normalize to LF; for uuid (2026-10-09) the LF tree built with a dll of the packed dll's size and strings.

**uuid at `T` (2026-10-09, linux/amd64): 54 matched, 0 disclosed, the banked reading.** The first `-tests` run read
`TestVersion6` Go=pass C#=fail ("time reversed"). The cause is an upstream test race, and the converted code is not
at fault: in google/uuid v1.6.0, `NewV6` writes the version nibble over bits 12-15 of the timestamp and `Time()` reads
those bits back unmasked, so the test fails whenever the timestamp crosses a multiple of 0x1000 (409.6 microseconds)
between its two `NewV6` calls. A Go probe that encodes two timestamps the way `NewV6` does shows it deterministically,
and native Go fails the test 3 times in 20,000 runs (`go test -run 'TestVersion6$' -count=20000`). COORD ruled it
CITED, NOT DISCLOSED: a disclosure states a stable Go-versus-C# divergence, and this is neither stable nor a
divergence. The proof is a green run; the same note is on the package's proof page, from a notes-only module
disclosure manifest (`github.com/google/uuid@v1.6.0/go2cs_test_disclosures.json`, on the uuid kit ref until it is
seated under `src/tests/ModuleDisclosures`), and in the row's pull request.

## 3. Sample programs

Each module's demo program has output that does not vary between runs, so the runbook's `dotnet run` against
`go run` comparison is exact. The `go run` output below was read on 2026-10-09 (linux/amd64, go1.24.13).

**hashset.** The rehearsal's `docs/phase4/rehearsal-nugetgo-hashset/main.go.txt` on `claude/c2-nugetgo-rehearsal`.

**uuid** (module `example.com/uuiddemo`, requiring `github.com/google/uuid v1.6.0`):

```go
package main

import (
	"fmt"

	"github.com/google/uuid"
)

func main() {
	fmt.Println(uuid.NewSHA1(uuid.NameSpaceDNS, []byte("go2cs.net")))
	fmt.Println(uuid.NewMD5(uuid.NameSpaceDNS, []byte("go2cs.net")))
	u, err := uuid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479")
	fmt.Println(u.Version(), u.Variant(), err)
}
```

```
5dc9fe3a-322f-536c-9e0d-d13b57bddeab
17ff6fb0-d3fd-3a99-988a-d3d2fea149a8
VERSION_4 RFC4122 <nil>
```

**jwt/v5** (module `example.com/jwtdemo`, requiring `github.com/golang-jwt/jwt/v5 v5.3.1`). It signs a fixed claim set
with HS256 and a fixed key, so the token is the same on every run (`encoding/json` sorts map keys), then parses it
back:

```go
package main

import (
	"fmt"

	"github.com/golang-jwt/jwt/v5"
)

func main() {
	key := []byte("go2cs-demo-key")
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{"sub": "go2cs", "aud": "nugetgo"})
	signed, err := token.SignedString(key)
	fmt.Println(signed, err)
	parsed, err := jwt.Parse(signed, func(*jwt.Token) (any, error) { return key, nil }, jwt.WithValidMethods([]string{"HS256"}))
	fmt.Println(parsed.Valid, err)
	subject, err := parsed.Claims.GetSubject()
	fmt.Println(subject, err)
}
```

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJudWdldGdvIiwic3ViIjoiZ28yY3MifQ.eAVcJq6WHY7i7Nbd3ILAobeRWWdEben1uPOKEAaaM6w <nil>
true <nil>
go2cs <nil>
```

## 4. Owner steps, collected

| When | Step | Runbook |
|:--|:--|:--|
| before hashset's section 4 | Make `hashset-cs` public. On 2026-10-09 an anonymous `git ls-remote` of it asked for credentials, so it is private or does not exist yet. | 0.6 |
| before uuid's and jwt/v5's section 4 | None: `uuid-cs` and `jwt-cs` exist, and COORD pushes each conversion commit. | 0.6, 4.5 |
| before the first section 3 | An int.nugettest.org account and API key for B1's publisher. | 3.1 |
| per module | Push the rehearsal to int.nugettest.org; sign and push the release candidate to nuget.org; push the conversion source. | 3.1, 4 |
| before hashset's row | COORD, on release day: the nugetgo pull request adding 1.24.13.5 to `cmd/sitegen/go2cs-releases.txt`, which lists `1.24.13.3` and `1.24.13.4` at `bd121c760e`. | 5.1 |
| per module | The row's pull request; for uuid and jwt/v5 (`community`), the review and merge. | 5.2-5.5 |

## 5. Rulings (COORD, 2026-10-09)

1. **`T`:** `R`'s record commit with cuts 1-8 merged, cut as `claude/c2-nugetgo-tools` after the record commit exists
   (section 1).
2. **jwt/v5's `test` package:** excluded from the pack. The exclusion needed a pack change, which is cut 8,
   `claude/c2-nugetgo-pack-exclude` (`-ExcludePackage`).
3. **Check 4(c):** the byte comparison of a freshly generated self-description with the published one is the
   maintainer procedure (runbook 5.4), and it goes into the nugetgo repository's CONTRIBUTING.
4. **The rehearsal label:** `int.1`.

Every conversion of a module records the `-platforms` value it used (runbook `P`), and the row's pull request states
it, because check 4(c) converts for the same target.
