# COORD review: the 1.24.13.2 Linux packaging train, seats 2 and 3 (2026-09-24)

Independent reviews by COORD on the i7. Each ran three reviewer dimensions, then two adversarial refuters per
finding, then a completeness critic. The reviews re-derived the claims, ran red-first arms in detached worktrees
at each seat's tip, and converted real modules with archive copies of each seat's converter.

- **Seat 2:** `claude/g-stdlib-meta-goos` `ba9d193861`.
- **Seat 3:** `claude/g-asm-trampolines` `f48e8010d1`.
- **Base of both:** master `ebb4acb8cf`.

The four branches of the train are pairwise file-disjoint and merge without conflict (the legacy three-way
`git merge-tree`, since the i7's git predates `--write-tree`):

- seat 1 `claude/g-rid-compile-asset`;
- seat 2;
- seat 3;
- `claude/g-named-slice-len`.

## Verdicts

| Seat | Verdict | Why |
|:--|:--|:--|
| 2 | **ACCEPTED**, with owed follow-ups | Every claim reproduced. One train-level contract gap: follow-up 2-A is REQUIRED for the train. |
| 3 | **REWORK** | **Blocker:** the one registry row emits a forwarder that does not compile against the real `golang.org/x/sys/unix`. |
| Train | **needs a corpus half** | 1.24.13.2 packs `src/core` as committed, and `syscall.prlimit` is `internal` there. |

## Seat 2: what was confirmed

- **The asset is byte-identical to an independent rebuild.** An independent Python port of `extract()` over the
  tip tree rebuilds `stdlib-metadata.txt` byte-identically (the sha256 is the same for both). The branch's own
  generator also reproduces it from a `git archive` skeleton of the tip.
  - 397 sections = 338 unqualified + 59 flavour. Nothing is missing, nothing is extra, and there are no
    duplicate headers.
  - All 338 unqualified sections are byte-identical to master's, section by section. The file header is
    byte-identical too.
- **The 34-versus-31 reconciliation.** 34 packages carry per-GOOS `package_info.cs` copies, and none of them
  has a flat sibling. They split three ways:
  - 28 have windows, linux and darwin copies, giving 56 flavour sections.
  - 3 are windows-only (`internal/syscall/windows`, `…/registry`, `…/sysdll`) and get no flavour section.
  - 3 have NO windows copy and get flavour-only sections: `crypto/x509/internal/macos`,
    `internal/runtime/syscall` and `vendor/golang.org/x/net/route`.

  That gives 59 flavour sections over 31 packages. Master recorded 31 of the 34, all from the windows copy.
  The 3 flavour-only packages fall back to derive-from-declarations for other GOOS values, exactly as on master.
- **Red first, both axes, one axis at a time.**
  - **Arm A (master's asset):** both new tests FAIL, naming `Handle` and `ΔSockaddr` at
    `stdlibMetadata_test.go:307/:311/:382/:386`.
  - **Arm B (a goos-blind lookup):** the same failures, while `TestStdLibMetadataInSync` stays green.
  - **A planted line deleted from `##syscall@linux`:** `InSync` reds.

  Every arm was restored byte-identical.
- **A flavour-ONLY record is consumed.** A linux/amd64 probe returned `user.UnknownUserError` as an error and
  passed a `*syslog.Writer` as an `io.Writer`.
  - The base converter re-declares both `GoImplement` pairs and emits local adapters.
  - The tip emits an empty `InterfaceImplementations` block and uses the published adapters.
- **The CNR no-op premise holds by construction.** The only production change sits behind `info.PublishedStdLib`,
  which requires `-recurse=nuget`, and `check-no-regression.ps1` never passes it.
- **The full converter suite at the tip, on windows:** `ok go2cs 472.9s`, 0 FAIL.

## Seat 2: follow-ups owed (G)

**2-A (REQUIRED for the train): tie the conversion's GOOS to the compile flavour.**
Seat 2 selects the record by the conversion's `-platforms` GOOS. Seat 1 selects the compile asset by
`$(RuntimeIdentifier)`, else the SDK host RID. Nothing ties the two together, so they agree only by convention.

It fails in these cases:

- A cross-target conversion: `-platforms windows/amd64` on Linux, or `linux/amd64` on Windows.
- A tree converted on Windows and built from WSL, or `go2cs.exe` run through WSL interop.
- A host whose RID has no shipped twin: linux-musl-x64 (Alpine), linux-arm64, or osx-*. On these, the compile
  falls back to `lib/` (the windows flavour) while the record is linux or darwin.

The result is CS0426/CS0118-class errors. Before seat 2, the windows record at least matched the `lib/` surface.

**Durable fix:** the `-recurse=nuget` emission writes a conditioned `GoCompileRuntimeIdentifier` default,
derived from `-platforms`, into the emitted `Directory.Build.props`, beside `GoStdLibVersion`. Seat 1's targets
file already honours that property first.
- Mapping: `windows/*` → `win-x64`, `linux/*` → `linux-x64`, anything else → unset.
- Red-first arm: a cross-target conversion whose emitted props carry the pin.
- The supported pairs (windows/amd64 ↔ win-x64, linux/amd64 ↔ linux-x64) are stated in the post-train README
  rewrite.

**2-B: tests.**
- A POSITIVE flavour-only-record arm (the `os.user` / `log.syslog` probe above is ready-made, and it reds against
  master's asset). The linux side of the new emission test asserts only absences, so a regression that skips
  the record for every non-windows target passes today.
- A synthetic-tree unit test in `internal/stdlibmeta` for "flat wins over every per-GOOS copy". No corpus case
  exercises that rule.
- The one-line assertion `stdlibmeta.ReferenceGOOS == platformDefaultTargetOS`. The comment at
  `generate.go:52-54` claims it exists; it did not exist on master either.
- The `fmt` arm of `TestStdLibExportedMetadataSelectsTheTargetFlavor` discards both `ok` results. Assert them.
- Optional: `InSync`'s red names the first drifted section.

**2-C: wording.**
- The asset header and the generator's summary line still describe only `##<dotted package name>` and print
  "397 packages", which is a section count. Describe `##<name>@<goos>` and the fallback rule, and count
  sections, not packages.
- The `stdLibExportedMetadata` comment calls it "the embedded-record mirror of platformPackageInfoPath". It is
  not an exact mirror: for a package that has flavour copies but none for the requested GOOS, the embedded
  reader returns the windows record, while the on-disk reader derives. This is latent (every case today is
  internal or a vendored std package). Correct the comment, or make that fallback conditional.
- Stale corpus figures in `platformLayout.go` comments: "27 of them corpus-wide" and "the 275 packages". Make
  them abstract, per the no-exact-figures rule.

**2-D: docs owed at the train.**
- `DESIGN-multiplatform-corpus.md` §9(a) item 2 and §12 mechanism 2 state the "one compile surface" premise that
  seats 1 and 2 retire. Add a dated amendment block.
- `ConversionStrategies-Reference.md` (the embedded-metadata section) still says "302 files", "~128 KB" and
  "five guards". Make the figures abstract and add the per-GOOS dimension.
- The README platforms note (post-train): on Linux, a converter built from a ≥ 1.24.13.2 checkout AND ≥ 1.24.13.2
  packages are both needed, because the fix lives in the converter's embedded asset.

**Hazard class (for train assembly):** after seat 2, editing a linux or darwin `package_info.cs` alias block or
`GoImplement` records moves the asset. So a branch cut before seat 2 can pass `InSync` on its own base and red
at the union. Two current branches touch those files: `claude/c2-float-bits` and `claude/r-synctest-reentry`.
They change only `GoPositionMap` and `GoImplicitConv` lines, which `extract()` ignores, so neither is affected
today. Run `TestStdLibMetadataInSync` at every union after seat 2 lands.

## Seat 3: the blocker and the rework

**BLOCKER: the `gettimeofday` forwarder does not compile against real x/sys/unix.**
- Real `golang.org/x/sys/unix` declares `func gettimeofday(tv *Timeval) (err syscall.Errno)` with its OWN
  `unix.Timeval`, in all 18 cached x/sys versions (2021 to v0.48.0).
- `syscall.gettimeofday` takes `*syscall.Timeval`. A `go/types` check reads `identical=false`.
- The forwarder passes `tv` straight through: `ж<unix.Timeval>` into `ж<syscall.Timeval>`. That is CS1503 against
  a republished corpus and CS0122 against 1.24.13.1, where `gettimeofday` is `internal`.
- The fixture hid it: `asmTrampolines_test.go:32` declares `gettimeofday(tv *syscall.Timeval)`, which is not
  the real shape, and the test checks emitted text only.
- Before the seat, this function was a throwing stub that compiled and that the walkthrough never reached.
  After it, the WHOLE x/sys/unix project fails to build on Linux.

**MAJOR: the registry changes corpus emission, against the seat's own GOROOT scope.**
- The `asmTrampolineTargets` arm in `packageFuncAccess` makes `syscall.gettimeofday` `public` on reconvert.
- The hand-owned implementing half (`src/core/syscall/linux/syscall_linux_amd64_impl.cs:50`) is
  `internal static partial`, so the next reconvert of syscall/linux fails with CS8799.
- No test covers this arm or its `isLinknameExposed` exemption.

**MAJOR: the walkthrough reading does not carry over to the cut seat.**
- The byte-identical reading came from scratch prototypes against the published 1.24.13.1, where `prlimit` and
  `gettimeofday` are `internal`, so those prototypes differed from `f48e8010d1`.
- The union walkthrough gate is predicted RED as cut.

**Rework (minimal, and the durable shape):**

1. **A signature-identity guard.** Forward only when `types.Identical(local signature, target signature)`;
   otherwise keep the stub.
   - It closes the blocker and the general case (F2: a JMP between different Go types, e.g. the
     `LoadUintptr → Load64` atomic-helper pattern).
   - Red-first arm: the REAL x/sys shape (`gettimeofday(tv *Timeval)` with a local `Timeval`) must stay a stub.
2. **Drop `syscall.gettimeofday` from the registry.** The walkthrough does not reach it: the forwards it needs
   are `Syscall`, `Syscall6`, `RawSyscall` and `RawSyscall6` (exported and identical on linux/amd64, linux/arm64
   and darwin), plus `prlimit` through the linkname table.
   - With the registry empty, remove `asmTrampolineTargets` and its two hooks (`packageFuncAccess`,
     `isLinknameExposed`). Unexported cross-package targets simply stay stubs.
   - This also removes the corpus-accessibility change and the CS8799.
   - A bridged `gettimeofday` (the `Reinterpret<unix.Timeval, syscall.Timeval>` shape the prlimit pull already
     emits) is a follow-on, sized together with the raw-SYSCALL residual.
3. **Same-package targets.** Either exempt a same-package JMP target from ref-lowering, or keep that shape a
   stub. Today a lowered `foo(ref T)` target meets a forwarder that passes `ж<T>`. Add a red-first arm for the
   lowered case either way.
4. **Tests.**
   - A unit test for the scope's EXCLUSION direction: `isGoRootSourceDir(<goRoot>/src/sync/atomic)` is true, and
     a temp dir is false. Forcing the guard to false passes every targeted test today.
   - Strengthen the forwarder assertions to the full call line, covering argument pass-through and access, not
     a substring.
   - `emittedFunction` should not match a comment line.
5. **Parser hygiene (notes).**
   - Reject a block that contains any `#if*` line.
   - Strip `//` comments before `/* */` per line.
   - Select `.s` files with the converter's own build context and tags, not `build.Default`.
6. **The figure.** "442 pure-JMP trampolines on the corpus flavours" is FALSIFIED. The verbatim
   `asmTrampolineIndex` over GOROOT/src (cmd excluded) finds 58 per flavour, 174 in total, all in
   `internal/runtime/atomic` (22) and `sync/atomic` (36). The darwin libc trampolines cannot parse as JMP
   trampolines at all: their TEXT symbol has no `·`. Replace the figure with an abstract description in the
   comment.
7. **Docs.**
   - Correct `writeLinknameForwarder`'s precondition comment (`visitFuncDecl.go:2405-2411`): assembly gives no
     linkname-compatibility guarantee.
   - Add a pure-JMP trampoline paragraph to `ConversionStrategies-Reference.md`'s "cross-package //go:linkname
     PULL emits a forwarder" section.

**Confirmed for seat 3.**
- The emission is red first at `ebb4acb8cf`.
- The planted `rawSyscallNoError` row turns the stay-stub arm red.
- The windows control holds.
- The x/sys trampoline set is the same across all 18 cached versions.
- `MatchFile` honours `//go:build` in `.s` files.
- Caching is per directory × platform × tags, with no bleed between platforms.
- The exported forwarders are type-correct in real x/sys v0.25.0 emission.
- Seats 2 and 3 do not interact in the walkthrough emission: seat 2 changes 2 lines of x/sys/unix
  `package_info.cs`, and seat 3 changes 4 files of forwarders.

## The train's missing corpus half

`release-nuget.ps1` and `push-nuget.ps1` never run `-stdlib`: 1.24.13.2 packs `src/core` as committed.
`syscall.prlimit` is `internal` at master (`src/core/syscall/linux/syscall_linux.cs:1207`). Seat 3's
`linknameForwardTargets` row makes it `public` only on reconvert, so as cut, the x/sys `syscall_prlimit`
forwarder fails with CS0122 against 1.24.13.2.

The train therefore needs a committed corpus change: the seeded `-stdlib` reconvert of syscall's linux flavour,
which is exactly G's owed two-seeded footprint, landed as a src/core commit. It needs its own gates: H7 on the
linux flavour, and the identifier census. `GoStdLibVersion` floats (`1.24.13.*`), so consumers pick up 1.24.13.2
with no converter stamp change.

## Residual (a follow-on, not this train)

After the train, x/sys/unix on linux/amd64 still throws in these functions:

- the raw-SYSCALL blocks `SyscallNoError` and `RawSyscallNoError`, which `unix.Getpid`, `Getuid`, `Umask`,
  `Exit`, `Sync` and `Auxv` route through;
- `gettimeofday`, via `unix.Gettimeofday` and `Time`, once it is guarded out.

On the go2cs target platforms, no x/sys version has an unexported, unregistered JMP target. They exist only on
386, arm, solaris, aix and plan9.

Size a follow-on seat: a bridged forwarder for identical-layout pointer parameters, plus a recognised
raw-SYSCALL shape.

## Provenance

- The workflow runs were `wf_0e6e6a7e-467` (seat 2) and `wf_69dd191f-f6e` (seat 3). Both were restarted once
  after a usage-limit stall; completed agents replayed from cache.
- The per-agent outputs are in COORD's session scratch. This file is the record.
- The ledger line for this review is stamped the same day.
