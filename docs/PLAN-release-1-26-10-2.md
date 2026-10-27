# PLAN — go.* 1.26.10.2: the first fix release on the Go 1.26 line

> **STATUS: DRAFT (COORD, 2026-10-26).** The owner asked for the post-publish wave and this plan on 2026-10-26, right
> after 1.26.10.1 published. Document type: an [instance plan](doctrine/ProcessGlossary.md#plan). It names the scope of the next
> release on master's Go 1.26.10 line, the seats and their lanes, how they land, and what gates the release.
> Procedure is not restated: seats land by the train discipline in the `train-assembly` skill, a re-bank follows
> [`GoCorpusMigration.md`](GoCorpusMigration.md), and the release follows its *Release day* section. If Go publishes
> 1.26.11 before this release is cut, the release becomes the patch hop's 1.26.11.1 instead
> ([`PLAN-version-lines.md`](PLAN-version-lines.md)), and the seats below ride it unchanged.
<!-- Sources: the post-publish queue COORD kept through the hop (rulings dated 2026-10-15 .. 2026-10-26, each in the
     ledger on claude/mailbox), the BOARD rows those rulings cite, and the 1.26.10.1 record (master 5100d1b16c). -->

---

## 1. Scope

**The headline: a pointer to another package's exported variable reaches the variable.** Today `&otherpkg.V`,
`otherpkg.V.M` taken as a method value, and `flag.BoolVar(&otherpkg.Flag, ...)` bind a copy, because the owning
package cannot see that an importer takes the address. 1.26.10.1 fixed the same case inside one package and documents
the cross-package case as a limitation. The fix, as ruled: **box every exported package-level value variable**. That
moves emission across the corpus, so this release carries a **full re-bank**.

**Also in scope**, each a seat with its own BOARD row:

| # | Item | Kind | Lane |
|:--|:--|:--|:--|
| 1 | Cross-package exported-variable box (the headline): design note, then seat | converter + golib | G (design), then a seat lane by ruling |
| 2 | Package-level box marker (`StaticBox`): a package-level `sync.WaitGroup` is Unbubbled, as in Go | converter + golib, 311 files | G (gated seat `a07906ae84`, re-base and re-A/B) |
| 3 | Goroutine stack reserve, option A: a small default reserve plus converter-marked large reserves | golib + converter | R |
| 4 | A deferred or `go` method call on an array element reads the element at the statement | converter | P2 |
| 5 | Converting a single `.go` file names its output correctly; a relative `-go2cspath` resolves | converter CLI | P1 |
| 6 | A rate-aware, resumable NuGet push (honours Retry-After; skips what is already published) | release tooling | P1 |
| 7 | golib's quiet build, stage 4 of [`PLAN-golib-full-trim.md`](PLAN-golib-full-trim.md): the 14 IL warnings the 1.26.10.1 pack printed | golib | H (G reviews) |
| 8 | Darwin harness items: the amd64-only test skip on the arm64 RID, cfile's host identity, `testing`'s working directory (fix B) | harness + hand-owned `testing` | C1 |
| 9 | Spawned test processes start faster (ReadyToRun or tiered JIT), so the BoGo suite fits its wall on desktop hosts | harness | G sizes; P1 measures |
| 10 | Alloc-free element-address compare (`&a[i] == &b[j]`) | converter + golib | after #1 lands; lane by ruling |
| 11 | The `go2cs` dotnet tool: install with `dotnet tool install -g go2cs` and convert with no Go installed (Go fetched on first use, verified), per [`PLAN-dotnet-tool-installer.md`](PLAN-dotnet-tool-installer.md) | converter + packaging + release ritual | P2 |
| 12 | Signed release binaries: the release machine downloads the CI-built archives, Authenticode-signs the Windows executables in the release's PIN session, re-packs and re-uploads them, and GPG-signs `SHA256SUMS`; the attestation covers the bytes users download | release ritual + workflow | P1 |
| 13 | Proof pages show visitor-facing disclosure reasons: each disclosure gains a rendered `reason` in the present tense and a non-rendered history field, and the proof pages are regenerated (the owner's ruling that pages reachable from go2cs.net carry no fleet language) | test tooling + docs | R |

**Not in this release, already scheduled:** the performance re-baseline against the published 1.26.10.1 packages
([`PLAN-perf-rebaseline.md`](PLAN-perf-rebaseline.md), G with the i9's AOT builds), the nugetgo rebuild wave against
1.26.10.1 (C2), and R's docs follow-up from the 1.26.10.1 veracity review. They need no converter change.

**Later, sized but not scheduled:** stack-allocation parity
([`DESIGN-stack-allocation.md`](DESIGN-stack-allocation.md)), the deferred-closure frame marker, `runtime.Goexit` on the
main goroutine, real `debug.ReadBuildInfo` data, the goroutine-leak profile, the crash block's `created by` line, the
remaining darwin list items that need a design, and stage 3d of
[`PLAN-golib-full-trim.md`](PLAN-golib-full-trim.md) (the dynamic-code class under the AOT analyzer, which reads 26
IL3050 sites at the plan's base), the named-result constant shift that masks its count, and long `TMPDIR` paths that skip the unix-socket tests
(both stated in Limitations and Known issues until fixed).

## 2. Order and landing

1. **Independent seats land first, in small batches on master**: #4, #5, #6, #7 and #8. Each is red-first, carries
   its own `src/core` footprint where it changes emission, and passes the batch gates (go test, GenTests, GolibTests,
   the solution builds, CNR). None of them moves a banked count.
2. **#2 and #3 land next.** Both change emission corpus-wide; each lands alone with its two-seeded footprint.
3. **#1 lands last of the emission seats**, after its design note is ruled, because it moves the most emission and
   triggers the re-bank.
4. **The re-bank**, sharded across the windows boxes with linux and darwin annotation legs, as for 1.26.10.1.
5. **Release day**, by the runbook, including the push from #6.

## 3. What gates the release

- Every seat landed, each with its BOARD row closed or amended.
- The full re-bank: every banked row at or above its count, every disclosure re-signed.
- Release-smoke on the four shipped RIDs, the darwin census, and the ritual rehearsal on the release machine.
- The docs state what the release does, in the present tense.

## 4. Open questions for the owner

- **OQ-1.** Four pages on the published docs site use fleet language: CIMatrix, Glossary,
  DESIGN-mn-scheduler-sizing and NugetgoPublish. Rewrite them for visitors, or exclude them from the site?
