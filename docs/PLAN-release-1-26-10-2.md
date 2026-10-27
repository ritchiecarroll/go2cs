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
| 9 | Spawned test processes start faster: the crypto/tls test host is published ReadyToRun (a per-package opt-in), and its BoGo fan-out banks only from a measured full run of 480 s or less on the bank host | harness | P1 |
| 10 | Alloc-free element-address compare (`&a[i] == &b[j]`) | converter + golib | after #1 lands; lane by ruling |
| 11 | The `go2cs` dotnet tool: install with `dotnet tool install -g go2cs` and convert with no Go installed (Go fetched on first use, verified), per [`PLAN-dotnet-tool-installer.md`](PLAN-dotnet-tool-installer.md) | converter + packaging + release ritual | P2 |
| 12 | Signed release binaries: the release machine downloads the CI-built archives, Authenticode-signs the Windows executables in the release's PIN session, re-packs and re-uploads them, and GPG-signs `SHA256SUMS`; the attestation covers the bytes users download | release ritual + workflow | P1 |
| 13 | Proof pages show visitor-facing disclosure reasons: each disclosure gains a rendered `reason` in the present tense and a non-rendered history field, and the proof pages are regenerated (the owner's ruling that pages reachable from go2cs.net carry no fleet language) | test tooling + docs | R |
| 14 | A ReadyToRun publish of a converted program does not run out of memory on golib's nested generic types: the project template passes crossgen2's generic-cycle detection, and the profiles' ReadyToRun workaround comes off, gated on the single-file publish completing inside an 11 GiB memory limit | template + docs | P1 |

**#1's NEWS text (breaking for compiled C#).** Release day lifts this verbatim into the 1.26.10.2 NEWS entry and the
release notes:

> **Another package's variables, by address.** Taking the address of a variable another package exports now
> reaches the variable, as in Go: `&otherpkg.V`, `flag.BoolVar(&otherpkg.Debug, ...)`, a pointer-receiver method
> value such as `f := otherpkg.V.Reset`, and an address inside it, `&otherpkg.S.Field` or `&otherpkg.Arr[i]`. Only a
> pointer- or interface-typed variable is still handed over as a copy ([Limitations](Limitations.md)). **This is a
> breaking change for compiled C# code**: each exported variable of a value type is now a `ref` property over its
> storage, not a field, so an assembly compiled against 1.26.10.1 must be rebuilt against 1.26.10.2. C# source that
> reads or assigns `pkg.V` compiles unchanged. go.* packages now depend on each other at the exact release version,
> so a project that mixes 1.26.10.1 and 1.26.10.2 packages fails at restore and names them. A read of such a variable
> costs about 0.5 ns more than a field read on the JIT, and about 0.7 to 0.9 ns more under Native AOT (measured on
> `win-x64`, one development machine).

<!-- From docs/phase4/DESIGN-crosspkg-exported-box.md section 11 (the seat as built), where the same text sits under
     COORD's OQ-2 ruling; COORD ruled 2026-10-27 that it ships on release day, lifted from here. The per-read costs are
     that section's bench (probe docs/phase4/probes/g-crosspkg-read-ab, median of 9, each runtime run twice): JIT nint
     0.25 for a field against 0.80 adopted; AOT nint 0.32-0.47 against 1.18. -->

**Not in this release, already scheduled:** the performance re-baseline against the published 1.26.10.1 packages
([`PLAN-perf-rebaseline.md`](PLAN-perf-rebaseline.md), G with the i9's AOT builds), the nugetgo rebuild wave against
1.26.10.1 (C2), and R's docs follow-up from the 1.26.10.1 veracity review. They need no converter change.

**Later, sized but not scheduled:** breaking the golib generic nest crossgen2 still expands, which is the root cause behind #14's flag (one lane, golib only), stack-allocation parity
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
