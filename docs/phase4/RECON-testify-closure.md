# RECON: the testify closure under `-tests -recurse`, and the module-cache integrity defect it found

A point-in-time record, 2026-11-22, lane C2 (COORD's rulings 172441Z and 175421Z). Amend with dated blocks; never
rewrite or execute from it.

**Subject.** `github.com/stretchr/testify` and the modules its go.mod pins at **v1.11.1**: go-spew v1.1.1, go-difflib
v1.0.0, objx v0.5.2 and gopkg.in/yaml.v3 v3.0.1. testify's latest release, v1.12.1, pins only objx v0.5.3 and
`go.yaml.in/yaml/v3` v3.0.5. It carries spew and difflib as `internal/spew` and `internal/difflib`, the first with a
cgo `testdata` package. It is not measured here.

**How each row was read.** `go2cs -tests -recurse -test-action all <module-cache release> <out>`, Go 1.26.11,
`linux/amd64`, `Release` with the tiered JIT off. Each row is built on this branch's converted standard library. The
converter for each row:
- 3f94b4818a: go-spew, objx, and testify (rerun on a cleaned cache).
- 5eccd36400: yaml.v3.
- go-difflib's row is banked (519ffacf51); its reading is 3f94b4818a with `-test-oracle-vet-off`.

| Module | Version | Tested packages | Matched | Diverged | Disclosed | Not converted | First red |
|---|---|--:|--:|--:|--:|--:|---|
| `github.com/pmezard/go-difflib` | v1.0.0 | 1 | 14 | 0 | 0 | 0 | none: BANKED, with Go's vet findings disclosed |
| `github.com/davecgh/go-spew` | v1.1.1 | 1 | 7 | 2 | 0 | 0 | TestDump (6 cases), TestFormatter (24): `%p` vs `unsafe.Pointer` (BOARD, owner H) |
| `github.com/stretchr/objx` | v0.5.2 | 1 | 255 | 0 | 0 | 0 | none: 255 of 255 |
| `gopkg.in/yaml.v3` | v3.0.1 | 1 | 0 | 0 | 0 | 1 | the test project does not build: CS0104, `bytes_BufferжWriter` ambiguous between the test package's and the dot-imported `gopkg.in/check.v1`'s |
| `github.com/stretchr/testify` | v1.11.1 | 6 | 782 | 11 | 0 | 0 | see below |

**testify, by package.**
- testify 1/0, `assert/internal/unsafetests` 1/0 and `require` 116/0: validate.
- `assert` 502/3:
  - TestEventuallyTrue, Go pass, C# fail: "Condition never satisfied". Not rooted.
  - Two rows are ONE subtest of TestObjectsAreEqualValues, named by `time.Date(...)` of the run's own clock. Each side
    reports its own name, and both pass. This is a comparison artifact, not a divergence of the code.
- `mock` 112/4, each Go pass and C# fail:
  - Test_Mock_Chained_On, Test_Mock_Chained_UnsetOnlyUnsetsLastCall and Test_Mock_UnsetByOnMethodSpecAmongOthers:
    the first compares `[]*mock.Call` unequal, and the two Diff sides show different Call contents. The other two are
    not read.
  - Test_Mock_On_WithFuncTypeArg: `AnythingOfType` does not match a `func(string) error` value, "mock: Unexpected
    Method Call".
- `suite` 50/4: TestRunSuite (Go skip, C# fail), plus TestRunSuite/TestSubtest and its subtests `first` and `second`
  (Go pass, C# fail).
  - `suite.NotEqual(subTestT, suiteT)` finds two different `*testing.T` equal.
  - The host's `testing.T` has three fields (Execution, DetachedFailed, DetachedSkipped), so two Ts compare DeepEqual.

**Counted as.** Matched is the verdict rows equal on both sides. Diverged is the rows that differ, with the time-named
pair counted as 2. No row is disclosed: nothing is banked from this table but go-difflib.

## The module-cache integrity defect (STOP-LINE, 175322Z; cure on this branch)

- **What.** `goModDownloadPreflight` ran `go mod download` with a module-cache release as the main module. For a
  go.mod that has no `go` directive and requires a module, `go mod download` adds the toolchain's `go` line and
  writes a go.sum, even under `-mod=readonly`, and a root process is not stopped by the cache's read-only modes.
  Measured on gopkg.in/yaml.v3 v3.0.1, whose cache entry gained `go 1.26.11` and a go.sum in this lane's run.
- **Not this defect: uuid.** github.com/google/uuid v1.6.0's cache entry gained `go 1.26.11` on 2026-11-20 07:12Z.
  Its go.mod requires nothing, and with nothing to download `go mod download` leaves a go.mod alone (measured). So
  that rewrite came from something else on this box, not identified. The STOP-LINE post listed uuid beside yaml.v3;
  this corrects it.
- **The cure** (601dc1d111 red, 85efd7b257; d58426a33b red, 5eccd36400):
  - every module-cache release converts and tests from a copy;
  - the preflight puts a go.mod's own bytes back, in a copy and in a user's checkout;
  - `-tests` fills the copy's go.sum with `go mod download all` and keeps its go.mod.
  Before that last step, the copy of yaml.v3 was read at Go 1.26's language version instead of the 1.16 its go.mod
  means.
- **Arms.**
  - The planted cache is byte-unchanged under plain `-recurse` and `-tests -recurse`, with every file hashed and
    go.sum absence included. A go.mod with no go line is RED against the pre-copy source (519ffacf51), and the
    control (a go line) is green on both.
  - The preflight keeps the go.mod.
  - A release with no sum for a test dependency loads its tests.
  - yaml.v3 end to end: cache byte-unchanged, no copy left, past the load. It now stops at the CS0104 above.
- **The banked uuid row (COORD's item 3).** From a PRISTINE GOMODCACHE, fresh and never touched by go2cs:
  - `-tests -recurse -test-action all` validates 54 tests, and the cache is byte-unchanged;
  - the verdict rows equal the banked proof's 54 of 54;
  - the emission, converted from the pristine cache and from one with uuid's go.mod given `go 1.26.11`, is 17 files
    and 0 differing lines.
  The banked row stands either way.
- **Residual, not cured.** A `log.Fatalf` during phase B (a package's test conversion failing) exits without the
  copy's deferred removal: yaml.v3's first post-cure run left one `go2cs-modcopy-*`. The next copy sweeps any
  older than 24 hours; the converter has 93 `log.Fatal` sites and no exit hook.

**Restored and verified (COORD's item 4).** yaml.v3@v3.0.1 and uuid@v1.6.0 on this box were deleted and re-extracted
from cache/download. `go mod verify` in a scratch module requiring both: "all modules verified".

## 2026-11-22 (later) — testify's 11 divergences by root cause, and the v1.12.1 closure measured

Converter at claude/c2-objx-bank ea636be70d, linux/amd64, go1.26.11, from a copy of each module-cache release. The
root-cause table went to COORD before any cure (posted 20:20Z). In short:

| Rows | Package | Root cause | Established by |
|--:|---|---|---|
| 4 | suite | two different subtests' `*testing.T` compare deep-equal (hand-owned `testing.T` holds only a host `TestExecution` reference) | failure text; the reflect member is not read |
| 3 | mock | a method chain spanning lines is emitted on one C# line, so `runtime.Caller` gives every call in it the first call's line | repro: Go offsets 3 4, C# 3 3 (predicted 2 2: mechanism right, line wrong) |
| 2 | assert | harness: the subtest name formats `time.Now()`, and the two runs start about 40 s apart; both sides pass | both runs |
| 1 | mock | a defined func type boxed into an interface reads as its underlying type (`%T`: Go `main.F`, C# `func(string) error`) | repro |
| 1 | assert | TestEventuallyTrue (100 ms window, 20 ms tick) diverged in 1 of 2 v1.11.1 runs and matched in the v1.12.1 run | not rooted |

**testify v1.12.1** (requires objx v0.5.3 and go.yaml.in/yaml/v3 v3.0.5; vendors spew and difflib under
`internal/`). go test's vet passes on all 8 tested packages. Predicted before the run: 8 packages, 12-14 diverged,
no new mechanism. Measured (matched/diverged): root 1/0, assert 588/2, assert/internal/unsafetests 1/0,
internal/difflib 11/0, internal/spew 7/2, mock 115/4, require 116/0, suite 59/4. Total 898/12: the table's
mechanisms plus spew's TestDump and TestFormatter on `%p` (the BOARD's pointer-number finding). MET.
Both sides fail assert's TestDirExists and TestNoDirExists on this box, so they count as matched.

**The closure modules.** objx v0.5.3: 255/0, predicted MET. go.yaml.in/yaml/v3 v3.0.5: converts and builds (MET)
with 704/0, against a predicted 1 (MISSED). Its tests no longer import gopkg.in/check.v1, so each test and subtest is
its own row, and the CS0104 cure is not exercised by this release. I assumed v3.0.1's test layout without reading
v3.0.5's test files. Both cache entries are byte-equal to their zips after the runs, and no copy was left.
objx v0.5.3 is banked on claude/c2-closure12-bank. yaml v3.0.5 is held: its generated proof page carries
`{{.}}` in two test names and no raw guard (repoguard's Jekyll/Liquid check names lines 48 and 713), and the
identifier census refuses a dotted-quad YAML input that five test names carry, since its converted mode admits only
`docs/validation/current/` pages.
