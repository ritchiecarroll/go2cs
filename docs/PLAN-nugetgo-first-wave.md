# PLAN — the nugetgo first wave: hashset, uuid and jwt/v5

> **STATUS: DRAFT (C2, 2026-10-09). Not ruled.** Document type: an
> [instance plan](Glossary.md#plan). It supplies the first wave's values, readings, sequencing and open questions.
> The procedure is the runbook [`NugetgoPublish.md`](NugetgoPublish.md), referenced by section number and never
> restated here. The wave itself is owner ruling B8 in [`PLAN-nugetgo.md`](PLAN-nugetgo.md), section 8, and the
> order follows the [Roadmap](Roadmap.md): hashset leads, then uuid and jwt. When the wave executes, this plan
> becomes a record.

## 1. Sequencing

The order COORD set on 2026-10-09: TRAIN FL lands, the face-lift go.* release `R` publishes, then hashset packs
against `R`. The nugetgo pack guards (cuts 1-7 on the `claude/c2-nugetgo-*` branches) seat in the train AFTER that
release, so the release tag does not carry them, and runbook section 0.3 decides `T`:

1. **`T` = master**, once the guards' train has landed, if golib and the source generators are byte-identical to
   `R`'s record commit (`git diff --quiet <record commit> master -- src/core/golib src/gen`).
2. **Otherwise `T` = a branch from `R`'s record commit with the seven guard branches merged onto it.** The same train
   carries G's trim stage 3c-2a (golib), so this case is the likely one. All seven branches stand on `56f0f1f254`
   (TRAIN FL's landing head, which `R` descends from), and on 2026-10-09 they merged with one another with no
   conflict.

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
| Library packages packed | 1 | 1 | 3: the root, `request`, `test` (`cmd/jwt` is a `main`, excluded) |
| Third-party dependencies | none | none (`go.mod` has no `require`) | none (`go.mod` has no `require`) |
| Description form (B6) | the author's (`-UpstreamPublishes`) | third-party | third-party |
| Upstream LICENSE holders | The go2cs Authors | Google Inc. | Dave Grijalva; golang-jwt maintainers |
| Conversion repository (`RepositoryUrl`) | `https://github.com/ritchiecarroll/hashset-cs` | OWNER to name | OWNER to name |
| Registry status | `canonical` (the repository is under the module's org) | `community` | `community` |
| Validation reading | 37 matched, 0 disclosed, 5 Example declarations excluded (the 2026-10-09 rehearsal at `56f0f1f254`) | the J0 pilot (`phase4/SIZING-j0-uuid.md`); re-read at `T` | not yet read as a module proof; read at `T` |

Every reading above is re-read at `T` in runbook section 1; none is carried forward.

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
| before uuid's and jwt/v5's section 2 | Name and create their conversion repositories. Each name is the package's `RepositoryUrl`. | 0.6 |
| before the first section 3 | An int.nugettest.org account and API key for B1's publisher. | 3.1 |
| per module | Push the rehearsal to int.nugettest.org; sign and push the release candidate to nuget.org; push the conversion source. | 3.1, 4 |
| before hashset's row | A nugetgo pull request adding `R` to `cmd/sitegen/go2cs-releases.txt`, which lists `1.24.13.3` and `1.24.13.4` at `bd121c760e`. | 5.1 |
| per module | The row's pull request; for uuid and jwt/v5 (`community`), the review and merge. | 5.2-5.5 |

## 5. Open questions for COORD

1. **`T`.** Section 1 recommends a branch from `R`'s record commit with the guards merged, unless master passes the
   diff gate. Which is ruled, and who cuts the branch?
2. **jwt/v5's `test` package.** The pack ships every library package of the module, so jwt/v5's `test` helper
   package would be the third assembly in the nupkg. Ship it, or exclude it, which would be a pack change?
3. **Check 4(c).** The runbook proposes a byte comparison of a freshly generated self-description with the
   published one. Does the nugetgo repository take that as its documented method?
4. **The rehearsal suffix.** The runbook uses `int.1`. Any label that passes the pack's lowercase-label check works.
