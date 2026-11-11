# DATA — seat C43: -recurse's default tags become safe,purego,noasm (P2, 2026-11-11)

> **A point-in-time record.** TypeScript readiness row C43, assigned by COORD on 2026-11-11 (mailbox `30b5d92d63`).
> Branch `claude/p2-c43-recurse-tags`, cut on master `50dd9a065c` (batch 284 landed). The census and the footprint
> prediction were posted before any commit (mailbox `8bd78952e0`), cut on the then master `b9a6d261fc`; batch 284
> changed none of the files the census read.

## 1. What the seat delivers

A `-recurse` conversion, in every reference mode (local project references, `-recurse=module`, `-recurse=nuget`),
now resolves its default build tags to `safe,purego,noasm`, where it resolved `safe` alone. `-recurse -tests`
resolves to `purego,math_big_pure_go,safe,noasm`: the stdlib default first, then the module default, each tag once.
The run prints the tags at its start, with the line `-stdlib` already printed
(`Applying build tags: safe,purego,noasm (default; pass -tags to override)`). An explicit `-tags` still replaces
the whole default, and `-module-safe-tag=false` drops all three module tags.

Why: klauspost/cpuid/v2 v2.2.10 selects `detect_x86.go` (amd64) or `detect_arm64.go` (arm64) unless built with
`noasm`. Each declares its CPU probe with no body (the body is in a `.s` file), and the package's `init` calls it, so
every converted program importing cpuid threw before `main`. zeebo/xxh3 imports cpuid.

## 2. The census (posted before the cut)

Method: each module copied from the module cache, then `go list -e -deps -test -tags <set> ./...` over its whole
non-standard closure, recording each package's GoFiles, TestGoFiles, XTestGoFiles and SFiles, on windows/amd64,
linux/amd64, linux/arm64, darwin/amd64 and darwin/arm64, with CGO_ENABLED=0. Two comparisons: the plain `-recurse`
sets (`safe` against `safe,purego,noasm`) and the `-recurse -tests` sets (`purego,math_big_pure_go,safe` against
`purego,math_big_pure_go,safe,noasm`).

| Module | Closure packages (linux/amd64) | Changed files, plain sets | Changed files, -tests sets |
|:--|--:|:--|:--|
| github.com/ritchiecarroll/hashset v1.0.0 | 1 | none | none |
| github.com/google/uuid v1.6.0 | 1 | none | none |
| github.com/golang-jwt/jwt/v5 v5.3.1 | 4 | none | none |
| golang.org/x/sync v0.19.0 | 4 | none | none |
| github.com/spf13/pflag v1.0.9 | 1 | none | none |
| github.com/inconshreveable/mousetrap v1.1.0 | 1 | none | none |
| github.com/spf13/cobra v1.10.2 | 6 (7 on windows) | none | none |
| github.com/itchyny/timefmt-go v0.1.8 | 1 | none | none |
| github.com/itchyny/gojq v0.12.19 | 16 (15 on windows) | none | none |
| control: github.com/zeebo/xxh3 v1.1.0 | 4 | cpuid only, on the 3 amd64 targets | the same |
| control: github.com/klauspost/cpuid/v2 v2.2.10 | 2 | 5 of 5 targets | the same |

The nine are the modules of `src/tools/nugetgo/nugetgo-modules.psd1`. None selects a different file on any target
under either comparison, so none is a rebuild-wave item for this change. GoInputDigest has no tag input
(`packageInputDigest.go` hashes the selected files), so an unchanged selection leaves it unchanged apart from the
converter revision every converter commit moves.

The control moves exactly as predicted: cpuid's `detect_x86.go` with `cpuid_amd64.s`, or `detect_arm64.go` with
`cpuid_arm64.s`, become `detect_ref.go`. The deciding tag is `noasm`: `purego` alone moves nothing in cpuid, which is
why the `-tests` comparison (purego already present) moves the same files.

xxh3's own selection does not change on any target. `accum_stubs_amd64.go` (six bodyless declarations:
accumAVX2, accumAVX512, accumSSE and their Block forms) is gated on GOARCH only. Under `detect_ref.go` every cpuid
feature reads false, so `hasAVX2`, `hasSSE2` and `hasAVX512` are false and hashing takes the generic path.
`compat_vector_test.go` forces them on through `withOverrides(true, ...)`, which still reaches the stubs. Prediction
for the TS lane's scoring: production hashing runs; xxh3's forced-vector tests still throw.

Fixtures: no behavioral project converts with `-recurse` (none in `BehavioralTestBase.cs` or any project). The
`go.mod` fixtures under `src/tests/ModuleBehavior` (7), `src/tests/Performance` and `src/tests/PackageTests`: 130
fixture-target pairs, 0 differ.

Tests that pinned the old default: `resolveBuildTags_test.go` only (`TestDefaultModuleBuildTagsContent` and two
table rows). `moduleProofPages_test.go` and `oracleTestArgs_test.go` derive their sets from `resolveBuildTags`.

## 3. Defaults taken (posted as Q1-Q5, all approved by COORD, mailbox `78099c7554`)

- Q1. `-module-safe-tag=false` drops the whole module default; the flag keeps its name.
- Q2. `-recurse -tests` drops the repeated `purego`: `purego,math_big_pure_go,safe,noasm`. A proof page made from
  now on states that set; the committed `docs/validation/modules` pages state the set their readings used and are
  unchanged.
- Q3. `release-smoke.ps1`: arm baselines run `go run -tags safe,purego,noasm`, and arm B's probe has one file pair
  per default tag, so a disagreement names the tag (checked with Go: all three tags give `safe purego noasm`, none
  gives `no-safe no-purego no-noasm`, `-tags safe` gives `safe no-purego no-noasm`).
- Q4. NEWS: master had no .4 draft, so `docs/DRAFT-news-1-26-10-4.md` starts one on the .3 precedent, one
  section per item (COORD: other seats' items land in the same file), naming the architecture-only limit.
- Q5. The guard is a go test over a real `ModuleConverter` run (below).

## 4. Commits

| Red | Fix | Piece |
|:--|:--|:--|
| `8ec733d7b4` | `b8d755532b` | the default, its dedupe and the run-start line; the guard fixture and the pins; the docs, release-smoke and the NEWS draft |

**The guard.** `recurseDefaultTags_test.go` converts an app + lib pair whose lib has `impl_asm.go`
(`//go:build !noasm`, a bodyless `implAsm`) and `impl_pure.go` (`//go:build noasm`).
`TestRecurseDefaultTagsSelectThePureFallback`: with no `-tags`, `impl_pure.cs` is written, `impl_asm.cs` is not, and
the run prints the default line. `TestRecurseExplicitTagsReplaceTheDefault`: with `-tags safe`, the reverse, and
the line names exactly `safe`. At master both fail by assertion (the default converts `impl_asm.go`; neither run
prints a tag line), with `TestResolveBuildTags` and `TestDefaultModuleBuildTagsContent` (4 tests red).

## 5. Predictions and their scores

| Prediction (posted before the cut) | Reading | Score |
|:--|:--|:--|
| src/core untouched; CNR 0; corpus 0 | src/core untouched. CNR changed one file, `CgoPseudoPackageTypes/.editorconfig` (`# CS0219: windows` became `# CS0219: linux windows`). The master binary, built from `50dd9a065c`, writes the same line on this linux-x64 host from a fresh copy of the fixture, so it is the base's own host drift, not this seat's; the file was restored | MET (one base-drift line, attributed by the master arm) |
| The files: commandLineOptions.go, main.go, resolveBuildTags_test.go, the new fixture, the docs named, release-smoke.ps1 + README, the NEWS draft | all of those, plus three not named: moduleConverter.go (the run-start line is printed in ConvertModule, not main.go), stdLibConverter.go (its line now comes from the shared Options.buildTagsAnnouncement, text unchanged) and validationProofPages.go (a comment). The renamed reference heading also moved docs/ConversionStrategies-Reference/README.md | MISSED by four files, each named here |
| uuid and gojq converted with -recurse at master and at the cut: byte-identical output | see the correction below | MISSED as posted |

**Corrected before the measurement** (2026-11-11, before either emission run): the emission prediction above
missed one line per converted project by construction. Each module project records GoInputDigest, and the digest
covers the converter revision, which `converterRevision()` (`testConversion.go`) takes from the executable's own
hash, so two different binaries write two different digests. The corrected prediction: the two roots differ only in
each project's `<GoInputDigest>` line, and in no `.cs` file.

**Reading** (both binaries built from `git archive` of `src/go2cs` at `50dd9a065c` and `3ac8db9d54`, the fix as first
committed, each run alone,
plain `-recurse` with `-go2cspath` at the cut's `src`, linux-x64): uuid 22 files each, the one difference its
project's GoInputDigest line. gojq 165 files each (11 of 11 packages), 36 differing lines: 11 projects'
GoInputDigest lines and `go2cs.modules.lock`'s seven module lines, whose last column is the converter revision
(`exe-<hash>`). No `.cs` file differs in either module. Score: no emitted C# changed, as predicted; the corrected
prediction named one of the two revision-carrying files and missed the modules lock. The cut's runs print
`Applying build tags: safe,purego,noasm (default; pass -tags to override)`; master's print no tag line.

## 6. Gates

The fix was first committed as `3ac8db9d54`. The full `go test` at that commit failed one test,
`TestProjitemsRegistersEveryGoSource`: the new test file was not registered in `go2cs-src.projitems`. Both commits
were unpushed and unposted, so they were rewritten: the registration went into the red commit (`8ec733d7b4`), and
the fix (`b8d755532b`) also gained the honest empty-set line (`Applying build tags: none` when nobody passed
`-tags=`, as under `-module-safe-tag=false`; the unit tests had shown it claiming that `-tags=` cleared the default)
and `TestBuildTagsAnnouncement`. `3ac8db9d54..b8d755532b` changes only that print branch, the test, the projitems
line and one citation, so the CNR and emission readings taken at `3ac8db9d54` describe the same emitted files.

- CNR (`check-no-regression.ps1`, linux-x64, at `3ac8db9d54`): the one base-drift line above, restored.
- `go test -timeout 90m ./...` from `src/go2cs`, at `b8d755532b`: rc 0, every package ok (go2cs 862 s, repoguard 138 s).
- `go vet` and `gofmt` on the changed files: clean.

