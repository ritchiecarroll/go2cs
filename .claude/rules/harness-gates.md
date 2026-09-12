---
paths:
  - "src/tests/**"
---

# Test harness, gates, goldens and measured budgets

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 1477-1601, 1602-1770, 2536-2687, 2688-2898, 3353-3367, 3368-3429, 3430-3609.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

- **⚠ STANDALONE (no-solution-context) builds of tests/behavioral projects measure the DEPLOY ROOT,
  not the repo — and the errors look SEMANTIC (paid 2026-08-30, cost one full invalidated bisect).**
  Without `$(SolutionDir)`, `$(go2csPath)` falls back to the machine-global deploy root
  (`%USERPROFILE%/go2cs` / `%GOPATH%\src\go2cs`), which is STALE between deploys. A missing root is
  loud (CS0246 on `go`); a stale root is not — it produces plausible type-mismatch errors (CS1503,
  CS1929, CS0234 on a newer attribute) that read as real regressions and are COMMIT-INDEPENDENT,
  which is how a bisect probe built this way reported "no green endpoint" across three anchors whose
  in-solution builds were all green. **Any standalone build of a project under `src/tests` must pin
  `-p:go2csPath=<repo>/src/` (forward slashes), and a bisect probe must carry the pin.**
  collision site (root-caused 2026-08-21, fixed at the converter 2026-08-22).** A POSIX environment
  block is case-SENSITIVE, so `GO2CSPATH=/root/go2cs` and `go2csPath=/root/go2cs/src/` are two
  entries; MSBuild materializes environment variables as properties and resolves property NAMES
  case-INSENSITIVELY, so both fold into ONE `$(go2csPath)` and the winner is decided by enumeration
  order inside the .NET env-table plumbing — a per-process coin flip. The losing draw concatenated
  `$(go2csPath)gen/...` into `/root/go2csgen/...`, dangled the analyzer and every stdlib
  ProjectReference, and the build died in a CS0246 storm on every golib type: intermittent,
  package-shuffling Linux `-tests` failures that killed three measurement campaigns with every
  plausible suspect A/B-eliminated first. **Windows environment blocks are case-insensitive at the OS
  level — the two names are ONE slot — so five weeks of Windows sweeps could not see it.** The
  converter now (a) never exports its own derived `GO2CSPATH` (`resolveGo2CSPathDefault`, `main.go`)
  and (b) scrubs every case-variant from the inherited environment before appending the canonical
  entry (`childEnvWithGo2CSPath`, `testConversion.go`), so a child carries exactly one spelling
  whatever the invoking shell holds; guarded by `childEnvGo2CSPath_test.go`. The general rule outlives
  this variable: **anything a child reads through a case-insensitive resolver must be injected ONCE —
  scrub-then-append, never append-and-hope — and "Windows is fine" proves nothing about the class.**
  The Linux harness pin (`_paths.ps1`) STAYS until a Linux lane re-measures without it.
  ⚠ **A pin the CONVERTED side needs goes in the SHARED child-env base, never in one side's env**
  (measured 2026-09-02, the TZ pin): `runtime.envs` is filled by a `[ModuleInitializer]` before
  `Main`, so no host code precedes the snapshot and `TestHost.Run` cannot pin `TZ` from inside the
  process — and making the snapshot live would break Go's own set-at-process-start semantics. The fix
  is the process environment at LAUNCH, beside GOROOT/PATH, applied to BOTH sides of the comparison:
  a cross-SIDE divergence is worse than the cross-platform one it was meant to cure.
  ⚠ **A control harness reproduces the caller's ENVIRONMENT, not just its command** (measured
  2026-09-02): `BehavioralRunner` invoked DIRECTLY inherited neither the CI job's `GoTargetOS` nor
  `_paths.ps1`'s pin, so every L3 csproj took the windows default on a Linux host and the leg read
  "red by construction" — for a pin that already existed. Diff the CI step's environment against the
  repro's before believing a local red; a repro differing from its caller by one unstated variable is
  measuring its own shell.
- **⚠ TOOLCHAIN RESOLUTION: the pipeline's ORACLE side runs whatever bare `go` resolves on PATH —
  GOROOT alone does NOT pin it (measured 2026-08-29, the net/http bank lane).** `go2cs.exe` shells
  out to `go test -json` for the compare oracle, and that child inherits PATH; on a box whose system
  SDK differs from the pinned one (this machine class: ambient 1.23.1 vs pinned 1.23.12), a shell
  setting only GOROOT runs the WRONG release's oracle. The failure shape is a new member of the
  mass-empty family: **Go="" for every test** — the ORACLE side blank while the C# side reads
  plausible — the mirror of the file-lock signature (C# side blank), and it reads like total
  conversion failure. Prepend `$env:GOROOT\bin` to PATH in every pipeline shell and verify with bare
  `go version`, never just `go env GOROOT`. Same family, third member (lane R, 2026-08-29): a bare
  `go2cs -tests` on a **Linux** host bypasses the sweep's `GoTargetOS` pin and links the WINDOWS
  dependency set, minting phantom CS0426s that read as Linux defects — net-family Linux work routes
  through the SWEEP, always.
  ⚠ **Corrected 2026-09-04 on the MECHANISM, the habit standing: the load-bearing part is the
  `GoTargetOS` PIN, and the sweep is ONE way to supply it, not the only one.** A hand-invoked `-tests`
  run on a non-Windows host needs `GoTargetOS=linux` in its environment — MSBuild materializes
  environment variables as properties — which a lane can export directly; routing net-family Linux
  work through the sweep stays the reliable habit because the sweep carries the pin, the toolchain and
  the cgo state together, but "it must go through the sweep" was the wrong reason.
  ⚠ **Fourth member — the RIGHT SPELLING of the WRONG RELEASE, and every existing GOROOT check is
  blind to it** (measured 2026-09-02, a cloud lane): on a box carrying side-by-side SDKs bare `go`
  resolved 1.24.7 while the corpus pins 1.23.12 — `go env GOROOT` stays self-consistent, the conversion
  succeeds and exits 0, and the spelling/namespace guards pass because nothing about the PATH is wrong.
  **A conversion against the corpus prints `go version` AND GOROOT before it runs.** It is armed at
  BOOT too: a stale `/etc/profile.d` lane script exporting an older `/usr/local/go/bin` beat the newer
  fleet file (profile.d sources alphabetically — a `zz-` prefix fixes it), and `wsl.exe -- bash -lc` does
  not source profile.d like a real login: verify by bare `go version` in a real login shell.
  ⚠ **A TOOLCHAIN PIN THAT CHECKS ONLY `go version` DOES NOT CHECK THE PIN.** If `$GOROOT\bin` holds
  no `go.exe`, PATH falls THROUGH to an ambient toolchain and the version string can still match while
  describing a different install (2026-09-07). **Assert additionally that the RESOLVED binary lives
  under the pinned root and that `go env GOROOT` equals it** — the version answers *which release*, the
  path answers *which install*, and only both together are a pin. ⚠ **And a negative control against a
  toolchain that DOES NOT EXIST cannot vary the axis — it silently tests the fall-through instead.**
  Two controls failed that way in a row: one pinned a release absent from the box (PATH fell through
  and it reported PIN OK for the RIGHT toolchain), the next was a `sed` whose pattern never matched,
  leaving a byte-identical copy of the script under test. **Verify the control's substitution LANDED
  and that its alternative is genuinely invokable before reading its verdict.**
  ⚠ **And its QUIET shape, with the seatbelt that is not one (2026-09-02, the container class):** where
  the loud form misroutes the namespace and exits 0, an oracle run under an ambient 1.24.7 against a
  1.23.12 corpus answers NORMALLY — no empties, no errors, a real comparison against a corpus the tree
  does not have. `GOROOT="$(go env GOROOT)"` is the trap wearing a seatbelt: pin explicitly, put its
  `bin` FIRST on PATH, ABORT unless bare `go version` reports the pinned release, and re-measure
  anything banked under an ambient one. The container class is NOT uniform (no bare `go` on one host,
  1.24.7 on another, 1.25.1 off PATH on a third) and a persistent USER-scope GOROOT can pin an old
  release on a laptop lane, so no lane assumes another's toolchain number — and pin `-go2cspath
  <worktree>/src` on every hand-invoked `-tests` run, whose generated csproj otherwise falls back to
  the machine-global deploy root (MSB4006 loud; a plausible verdict from uncompiled bits quiet).
  Because nothing recorded WHICH release ran the oracle, `oracleGoVersion` now goes into the comparison
  record, captured as OBSERVED — a `go version` through the same call, directory and environment the
  `go test -json` child inherits, `omitempty` so a late probe failure cannot invalidate a comparison.
  ⚠ **Third door, and the instrument that closes all three (2026-09-02).** The lane's OWN
  `export PATH=<toolchain>/bin:$PATH` defeats the fleet's `zz-` profile.d pin by construction — on a
  fleet host use the login shell and never prepend a toolchain path; a probe answering
  `command not found` is describing its own environment, not the host's. Cross the WSL boundary with a
  heredoc (`wsl -- bash -s <<'EOF'`): `wsl -- bash -lc '…'` expands `$(...)`/`$VAR` in the OUTER shell,
  so verification prints come back EMPTY (`GOROOT=`, `HEAD=`) and read as answers — three false
  "command not found" probes and one cut-presence line evaluated against the wrong tree. An empty
  verification print is a broken instrument, never a pass. And the wrapper that prints bare
  `go version` and ABORTS on a mismatch is itself NEGATIVE-CONTROLLED once against the box's other
  toolchain — the control must exit non-zero having run zero sweep stages — before any green it
  reports is believed.
  ⚠ **Two amendments to that third door, both measured 2026-09-02.** (1) **PRINTING a pin is not
  CHECKING it** — the "prints `go version` AND GOROOT before it runs" wording above is satisfied by a
  DECORATION: a control script printed `go1.24.7` on its first line, from a `go env GOROOT` taken in an
  unpinned shell, and carried on; three findings descended from that run and were withdrawn. An
  instrument that prints its pin and proceeds has no guard — it ABORTS on mismatch, and the print is
  only evidence of what the abort compared. (2) The WSL quoting rule WIDENS: **every substitution
  inside a single-quoted `wsl … -lc '…'` string is expanded by the OUTER shell** — verification prints,
  loop variables AND exit codes, which is how a false `(exit 0)` and three empty-path parse errors
  landed in one evening. The heredoc form (`wsl -- bash -s <<'EOF'`) is the only spelling for crossing
  that boundary.
  ⚠ **A PATH-ONLY TOOLCHAIN SWITCH IS HALF A SWITCH** (coordinator, 2026-09-07 18:57, provisioning
  go1.24.13 on the i7): `go install` under a PATH-selected 1.23.12 `go` with the ambient GOROOT still
  resolving 1.23.1 dies `compile: version "go1.23.1" does not match go tool version "go1.23.12"` — and
  the failure was read as `install exit=0`, because the exit came through a pipe. Two catalogued traps
  in one line, met by the author of both entries. **The cure is to need NO toolchain**: fetch the
  official archive, verify its SHA-256 against go.dev's own manifest (`?mode=json&include=all`),
  extract with the System32 `tar` NAMED BY PATH into a writable SDK directory, and prove it by the bare
  `go version` under the new root with `GOTOOLCHAIN=local` and that root as `GOROOT` for the ONE call —
  changing no default pin. And capture every exit BEFORE any pipe.
  ⚠ **The provisioning bar that follows from it** (amended on a lane's measurement, 2026-09-07): the
  CAPABILITY test is (a) executes with a self-consistent GOROOT, (b) COMPILES a probe module with
  `go version <bin>` stamping the release, (c) `go list std` at that root returns the host's census
  count, (d) ZERO read-only files under `src/` measured by MODE (`! -perm -u+w` — **`! -writable`
  answers access(2) and is VACUOUS for root**), (e) pins unchanged. `test/typeparam` presence is a
  RECORDED acquisition-route property — present in the archive, absent from a `golang.org/dl` download
  and from module-cache roots — and **never a gate**: as a gate it rejected the corpus's own pinned
  1.23.12 on two lanes.
- **`TargetComparisonTests` compares goldens with line endings NORMALIZED** (CRLF→LF; see
  `TargetComparisonTests.FileMatch` / `BehavioralRunner.FilesEqual`, both strip CRs). It was a raw
  byte-for-byte compare until 2026-07-07. Content diffs are still caught exactly; a pure line-ending
  difference is ignored (it can only come from autocrlf, never from the deterministic converter). To
  re-baseline goldens after an *intended* output change, run the **`UpdateTestTargets`** project with
  **`--createTargetFiles`**, or `run-behavioral.ps1 --update-targets` — don't hand-edit goldens.
  ⚠ **Both re-baseline paths RE-TRANSPILE each project they are about to re-baseline, unconditionally,
  immediately before the copy, and REFUSE (exit non-zero, by name) when that transpile fails, times
  out, or exits 0 having converted best-effort (2026-09-04).** Until then neither did: the utility ran
  the converter *not at all* and this file carried the prerequisite as a sentence ("re-transpile
  first … or the copy silently re-baselines stale output"), while the runner ran it only when its
  **mtime** predicate said the project was out of date. Both are the same hole and it is FALSE-GREEN
  ROUTE #2 turned on the goldens: `UpToDate` answers on mtimes, and a `.cs`-only restore, a
  `Copy-Item` or an editor save all leave a `.cs` newer than both its `.go` and `go2cs.exe` while its
  CONTENT came from some other converter — after which the copy makes `.cs`, `.cs.target` and
  `UpToDate` agree by construction and no later run can see it. There is deliberately **no**
  up-to-date predicate on either path now, for the reason CNR has never had one: a stale COMPARISON
  is recoverable, a stale RECORD is not. The utility's `--only <Name>[,<Name>…]` narrows one
  invocation's transpile-and-copy (the four `<TestMethods>` blocks stay a function of the whole
  project set) — it exists so the refusal branch is not a ~25-minute control nobody runs.
  ⚠ **Three rules that fell out of landing it (2026-09-04).** A whole-corpus re-baseline banks any
  `.cs`-vs-committed drift into the goldens SILENTLY, so **a byte-identical CNR verdict is its
  PRECONDITION and runs FIRST, never after**. The **refusal property asserted is the golden UNCHANGED
  ON DISK** — a refusal that has already copied is a report, not a refusal. And a corpus-wide control
  states its BASELINE: 718 golden pairs byte-identical at the head is what makes a whole-corpus copy a
  no-op and ONE moved golden a measurement. (The demonstration that such a path is broken is the
  runner printing `ok` having invoked the converter ZERO times and minting a poisoned `.cs` into the
  golden at exit 0.)
  ⚠ **A golden that byte-matches a DEGRADED emission is a phase actively VOUCHING for the hole**, not
  one merely proving nothing (2026-09-04): at master the Target phase PASSED over a best-effort
  `main.cs`. A harness that cannot MEASURE a transpile skips its Target compare exactly as it does for
  Fail and Timeout — a golden compare is a statement about a measured emission or it is nothing.
  ⚠ **A RED CONTROL LEAVES THE DEFECTIVE EMISSION ON DISK BESIDE THE FIXED GOLDEN** (2026-09-05):
  reverting the fix and re-transpiling the guard to reproduce the defect rewrites its `.cs` while the
  `.cs.target` still holds the FIXED form, so a commit taken there banks the wrong emission next to
  the right golden and only a later Target phase would catch it. **After any control that
  re-transpiles: restore the source, purge `bin`/`obj`, re-transpile, and require the `.cs`
  CR-strip-identical to its golden before staging** — and re-read the worktree's state after any
  interruption, since a resumed agent inherits whatever the control left behind.
- **autocrlf gotcha (`core.autocrlf=true`) — two SEPARATE concerns:** the converter emits CRLF for C# line
  endings but preserves the Go source's LF inside multi-line string literals, so those `.cs`/`.cs.target`
  contain mixed CRLF/LF, and autocrlf rewrites the in-string LFs to CRLF on checkout.
  (1) **Golden text comparison** — no longer an issue: the comparison is line-ending-insensitive (above),
  so a smudged golden still matches and **no `-text` mark is needed just for the byte compare**.
  (2) **Runtime correctness** — still needs `-text`: if a project's *compiled program* embeds and observes
  a multi-line string literal at runtime (e.g. `Solitaire`'s board, printed via `println`), autocrlf smudges
  that literal's newlines to CRLF in the on-disk `.cs`, and any build that compiles the committed `.cs`
  *without* re-transpiling (VS, CI `dotnet build`, or the runner's up-to-date-skip) bakes the wrong `\r`
  runes into the value → the program misbehaves (Solitaire's board geometry breaks and the solver hangs).
  So `Solitaire`/`SortArrayType`/`StdLibInternalAbi` keep their `.cs` `-text` marks. A NEW multi-line-string
  test only needs `-text` if its program's *behavior/output* depends on the literal's exact bytes; if the
  literal is inert (never printed/measured), no mark is needed and the golden compare stays green regardless.
  **⚠ The CRLF working-tree form is now PINNED, not inherited from `core.autocrlf` (2026-08-08, r46c).**
  `.gitattributes` carries a `text eol=crlf` block for every converter-emitted artifact type — `*.cs`,
  `*.cs.auto`, `*.cs.target`, `*.csproj`, `*.slnx`, `*.props`, `*.targets`, `src/core/**/README.md` —
  ordered ABOVE the `-text` blocks so those keep their verbatim-bytes exemption (last matching pattern
  wins). Rationale: the converter emits CRLF *unconditionally*, so the checkout was the only variable,
  and a clone with `autocrlf=false` (git's default on Linux/macOS) materialized LF and made
  `check-no-regression` report the entire corpus as drifted before any work started. **Nothing about
  the Windows lane changed** — `eol=crlf` reproduces exactly what `autocrlf=true` was already doing,
  verified by `git add --renormalize .` over all 9,380 tracked files staging **zero** corpus files
  (every non-LF blob in the index was already `-text`). Two consequences worth carrying: a whole-tree
  renormalization is **not** owed, and the mixed-CRLF/LF phantom described above is *unchanged* in
  shape — it is simply platform-independent now. Do not "fix" a `.cs` to LF to match a Linux habit;
  the pin will put it back.
- **testhost lock gotcha:** a stray `testhost`/`vstest.console` from a prior run can lock
  `BehavioralTests.dll` → next build fails with `MSB3027` ("file locked by testhost"). Kill it (and
  `dotnet build-server shutdown` frees bin/obj locks) before rebuilding — not a real compile error.
  **Root cause + mitigation (2026-06-30):** the MSTest `Exec()` used an unbounded `WaitForExit()`, so a
  hung child (a deadlocked transpiled program, or a build blocked on a lock) hung the suite forever and
  orphaned testhost. `Exec` now has a per-call timeout (180s build/transpile, 30s run) that kills the
  whole child **process tree**, and disables MSBuild node reuse (`MSBUILDDISABLENODEREUSE=1`) so in-test
  builds don't leave lock-holding worker nodes; `AssemblySetup.[AssemblyCleanup]` runs
  `dotnet build-server shutdown` **only for a bare `dotnet test`** — a `run-behavioral-tests.ps1` run
  sets an env-var contract that suppresses it, since the script's default path isolates its own children
  instead (chip `6fe128108`, 2026-08-08). Prefer **`src/tests/Behavioral/run-behavioral-tests.ps1`**
  (clears stale hosts *before* the build — the lock manifests at build time — and runs with
  `--blame-hang`) over a bare `dotnet test`.
  **⚠ An MSTest verdict WORD is not a verdict — an ABORTED run prints one anyway** (measured 2026-09-02,
  GolibTests on a Linux lane): the second-to-last line reads `Passed! - Failed: 0, Passed: 82` and the
  LAST reads `Test Run Aborted.`, against a declared count near 470 — the exit code is honestly 1, but a
  verdict-word grep reads green, and `$?` after a pipe is the LAST command's status (grep's), so a piped
  invocation captures the raw exit first. **A GolibTests gate greps for `Test Run Aborted` AND compares
  the run's Total against the DECLARED count (`grep -c '\[TestMethod\]'`)**; an abort is an UNMEASURED
  suite, never a pass — the tell was adding 7 tests and watching the total stay 82. Run it `--no-build`
  behind the solution leg, too: a `dotnet test` that BUILDS raced twice in one night on a spurious
  CS0234/CS0246 that was gone on `--no-build` against the build just completed.
  ⚠ **And that DECLARED count is derived from the COMPILE SET, not from a raw `[TestMethod]` grep**
  (measured 2026-09-02): `GolibTests.csproj` `Compile Remove`s the Linux-only test files when
  `$(GoTargetOS)` is not linux, so a run reporting 474 against 479 grep-counted methods is
  COUNT-MATCHED, not an abort. Subtract the methods in `Remove`d files whose condition holds before
  reading a shortfall as a truncated suite.
  ⚠ **And WHAT belongs in that `Remove` set is decided by the hand-own's flavour** (2026-09-05): a
  per-GOOS hand-own's `Go`-prefixed test helpers exist only under that flavour, so the GolibTests
  classes referencing them are linux-only FILES **by construction** and belong in the `Compile Remove`
  set for every other `$(GoTargetOS)`. The WINDOWS-flavour solution build at the union is the gate
  that sees it (22 CS0103 on five helper names); a lane that cannot build the other flavour's solution
  STATES that gap in its seat and adds the `Remove` in the same cut. **A chain is STOPPED at a red
  compile gate**, never run on to legs that cannot mean anything on a broken union.
  ⚠ **A TEST FILE'S `Compile Remove` CONDITION DECIDES WHICH TARGETS ITS GUARDS CAN RUN ON — a guard
  placed in a file removed on the target it must exercise is VACUOUS AND READS GREEN.**
  `GolibTests.csproj` removes `LinuxSpawnSeamTests.cs` under `'$(GoTargetOS)' != 'linux'`, so a darwin
  arm placed there compiles only on linux (where the darwin hand-own does not exist) and is absent on
  darwin (where it does). **Read the csproj's condition groups before choosing a guard's home**: as of
  2026-09-07 the groups are `!= 'linux'` (8 files) and `!= '' and != 'windows'` (1 file), and **no group
  matches darwin at all** — a linux-OR-darwin arm needs a NEW group that removes on windows/unset,
  never an existing one. ⚠ And **"CONTRACT test" and "test that exercises a platform-gated
  implementation" are different shapes wearing one word, the first NOT precedent for the second**:
  `DarwinSigmaskContractTests.cs` sits inside the linux-only group legitimately because it asserts a
  SHAPE and needs no darwin host. **Ask what a test TOUCHES, not what it is named**, before citing a
  neighbour as permission.
  ⚠ **Both inputs from one stale tree defeat it — the vacuous-green class one level deeper** (measured
  2026-09-06). The Total-against-declared check this file prescribes specifically to catch an aborted
  MSTest run **PASSED on a run 42 methods short** — the exact truncated-suite signature it exists to
  detect — because the declared count and the total were both taken from a checkout 42 commits behind.
  Not skipped, not fabricated, not scoped-empty: **RUN, and defeated by its own inputs agreeing with
  each other instead of with reality. A self-consistent check needs at least one input anchored to a
  named ref.**
  ⚠ **The cheap layer settles more than the expensive one: a run gives you a number, the arithmetic
  tells you which numbers are POSSIBLE.** Computing the compile set from the csproj's conditional
  `ItemGroup`s against the declared-method count — **no build at all** — yields the exhaustive set of
  legitimate totals per flavour (696 unset, 696 windows, 727 linux, 692 darwin at that tree). A
  reported total matching NONE of them is a stronger statement than any single re-run can make. Derive
  the admissible range before spending a suite on one sample, and hand the lane the target so its
  re-run has something to be checked against. ⚠ And **a PER-FILE method count is a property of the blob
  it was taken at, exactly as the total is** — carrying per-file counts from one seat's tree to master
  came within one command of accusing a lane whose arithmetic was right (one test file had grown from 9
  methods to 14 between the trees).
  ⚠ **A DEBUG-ONLY GolibTests reading silently under-measures the GC/pin-liveness class by THREE**
  (measured 2026-09-06). Exactly three tests RUN at Release and SKIP at Debug — the liveness class
  self-skipping where a non-optimizing frame would root its temporaries and make the assertion
  unfalsifiable, which is the tree's own discipline working. **The consequence: a gate line quoting
  Debug alone is NOT equivalent to one quoting Release+TC0 even when both read zero failures** — two
  greens, different coverage, identical-looking; the vacuous-green family in its subtlest form, not a
  check that cannot fail but a configuration in which three checks quietly do not run. **Every gate
  line naming GolibTests states its configuration.** The number earns its keep twice: a skip delta of
  exactly 3 between Release and Debug is a usable self-consistency check that both legs of a
  two-configuration cut ran what they should.
  A concurrent partial build racing a suite can tear an assembly and make a passing project fail; it
  cannot make a failing project pass. **So a green taken under known contamination still holds, and
  only a RED needs the clean re-run** — state the asymmetry rather than discarding the run.
  ⚠ **MSTest RUNS SERIALLY WITHIN AN ASSEMBLY UNLESS TOLD OTHERWISE, so process-global state IS
  assertable in GolibTests** — no `[Parallelize]` attribute, no `.runsettings`, and a sibling test class
  already mutates process-global ENVIRONMENT state there on exactly that basis. **Check the attribute
  and the settings file before declaring a global unguardable.** ⚠ **A STATED LIMITATION CLOSES A
  QUESTION INSTEAD OF OPENING ONE, WHICH MAKES A WRONG CLAIM IN AN ANNOUNCEMENT WORSE THAN WRONG
  CODE**: wrong code meets an oracle and is refuted, while **a declared limit is never re-derived,
  because the next reader does not go back over a limit somebody has already declared.** A lane
  announced that a working-directory guard "cannot assert without racing every sibling" — first clause
  true, second an assumption never checked, refuted by ONE COMMAND (2026-09-07) — and **the sentence was
  the one its author was PLEASED with, for being honest about a gap**, which is the trap: candour about
  a limit reads as rigour and is accepted without check. **State what you MEASURED and what you ASSUMED
  as two different sentences**, and treat "X cannot be done here" as a claim owing evidence exactly like
  "X works".
  ⚠ **THOSE TWO RULES COLLIDE, AND THE COLLISION IS SILENT** (measured 2026-09-06). The rename that
  makes a runner unmatchable by a sibling's `Get-Process <name> | Stop-Process` also makes it invisible
  to the by-name census the overlap rule asks for: `Get-Process BehavioralRunner` read **0** while
  `coordT31Runner.exe` PID 35520 had been running 48 minutes, and a second runner was launched into the
  same worktree on that reading. **A liveness census is by executable PATH**
  (`Win32_Process.ExecutablePath -like '*<worktree>*'`), never by name — the name is exactly the field
  the defence changes.
  ⚠ Two readings from the same night. **A log frozen mid-line is not evidence of death when the phase
  it stopped in is silent and long**: `[Compile] Go (per project)... ` with no newline read as the
  documented truncated-log kill signature, and the phase simply prints nothing until it completes — the
  PROCESS answers whether a run is alive, and a live `go.exe` under the runner is positive evidence of
  progress no log tail can supply. And **"Device or resource busy" on a copy is a LIVENESS signal**:
  the failed `cp` onto the unique apphost name was the first true reading of the evening, because the
  file was locked by a process running it. **An operation refused for a reason you did not predict is
  data about the world, not an obstacle to route around.**
- **Faster alternative to MSTest — the standalone runner `src/tests/Behavioral/BehavioralRunner`
  (2026-06-30).** A dependency-free console app that runs the same four phases over every behavioral
  project but is **not** hosted in testhost, so the
  self-lock failure mode above is structurally absent. It collapses the per-project `dotnet build`
  calls into one parallel MSBuild invocation (pre-building the ~31 shared `golib`/analyzer/`core/*` deps
  sequentially first to avoid the parallel-build MSB3026/27 race, then fanning out). **All green**, at
  parity with MSTest — the parallel MSBuild invocation keeps wall-time from
  scaling linearly with project count. Drive it via **`run-behavioral.ps1 [--filter X]
  [--phase transpile,compile,target,output] [--update-targets] [--list]`**. Only output-compared
  (`[GoTestMatchingConsoleOutput]`) projects are `go build`- and stdout-compared, matching MSTest
  (library-style projects like `Constraints` have no `package main`). For a pure converter no-regression
  check with no compile/run at all, use **`check-no-regression.ps1`** (re-transpiles every behavioral dir
  and `git status`es the converter-emitted `.cs` **and `.csproj`** — the transpile rewrites both, and the
  `.cs`-only pathspec it had until 2026-08-08 made a csproj-emission change invisible on every platform.
  Converter stderr is captured, not discarded: a package the run could not fully regenerate — best-effort
  "did not fully type-check", a recovered "visit file error", or a non-zero exit — fails the gate by name
  as **NOT MEASURED** even with a clean `git status`, so the byte-identical verdict is never vacuous;
  other WARNINGs are counted as advisory, never fatal. Coordinator ruling 2026-08-08, from lane r48b's
  Linux `FindFirstFileData` finding — see `docs/PLAN-linux-operation.md`. Until F8 platform-gates the
  enumeration, a Linux CNR run therefore reports `FindFirstFileData` as NOT MEASURED by design).
  ⚠ **A converter that EXITS 0 on a DEGRADED emission makes "the exit code" a false-green predicate in
  every harness that asks it** (2026-09-04), which is why the classification of a converter stderr line
  lives in ONE linked predicate the harnesses share rather than being re-derived per instrument — the
  same shape as `ConverterBuildInputs` for the staleness set (route #5). ⚠ And **two harness statuses
  whose REMEDIES are opposite are separate members**: a budget that expired wants MORE BUDGET, a
  best-effort conversion wants a HOST THAT CAN TYPE-CHECK. A report that cannot tell them apart — or a
  remediation hint naming one remedy for both — sends the reader to the wrong fix, so the hint names
  BOTH or neither.
  ⚠ **The class bites in BOTH directions now (2026-09-02): a behavioral guard written against ONE
  platform's syscall API cannot type-check on the other and turns THAT host's CNR red by name.** A
  lane's own-platform CNR green says nothing about the other host's gate — the union battery there is
  where it surfaces. F8 landed with train 11 (2026-09-02): a converter-preserved
  `[GoPlatformExclusive("<goos>")]` marker in `package_info.cs` naming the native platform(s), plus a
  LOUD skip-by-name BEFORE transpile in every enumerator (CNR, `BehavioralRunner`, MSTest as
  `Inconclusive`), its gating set DERIVED from the other platform's NOT MEASURED list (six
  windows-native, `ScmRightsSeam` linux) and positive-controlled both ways; commit markers before any
  CNR `-Revert`, which destroys uncommitted ones. Worse, a best-effort conversion on a
  NON-native host REWRITES the package's csproj and `package_info.cs` (the stdlib ProjectReferences and
  import aliases drop when the type-check that supplies them fails), so a Windows CNR POISONS a
  Linux-only behavioral package and every later leg of the chain measures the poisoned file — 5
  CS0246/CS0234 reading as a missing-reference regression. A chain therefore RESTORES behavioral dirt
  (`git checkout HEAD -- src/tests/Behavioral`) between CNR and any build leg, and F8's skip must
  precede the converter. Such a guard also carries a `runtime.GOOS` early-out as `main`'s first
  statement (raw `syscall.Socket` panics on Windows without the WSAStartup `net` performs), goldens
  stay WINDOWS-generated, and a Linux CNR-EQUIVALENT's DRIFT column is noisy by construction — the NOT
  MEASURED column is the honest one there.
  ⚠ **F8's consequences, measured at its landing (2026-09-02).** The marker has TWO halves — the
  harnesses' skip on a foreign host AND the `go2cs.slnx` UNREGISTRATION (the solution has one Windows
  flavour, so a non-windows-native package cannot compile there on any host) — and
  `check-solution-integrity.ps1` is the one gate that sees the second: run it. A platform-exclusive
  guard's golden (`.cs.target`) and its four MSTest entries are therefore verified ONLY on a
  native-host leg — on the other host F8 skips every phase, Target included, and CNR is transpile-only
  — which is how `ScmRightsSeam` landed with neither and nothing could see it. The same marker also
  covers a package that type-checks everywhere but FAULTS at run time (`LocalTimeZone`'s kernel32
  call): an Output-phase exclusive. The OTHER cross-platform class is ACCEPTED, not gated — a package
  that runs meaningfully on both platforms whose emission differs only by the `Δ`-alias flavour
  (`EnvironBlockWalk` and `SendtoSeam` — the class is EMPTY at master as of C2's marker seat, landing
  with train 14, and the COUNT is what retires there: the derivation below stands, because the next
  platform-varying guard brings the next member. Its two members were remediated by OPPOSITE
  mechanisms. `EnvironBlockWalk` is WINDOWS-native with a golden captured on Windows and read on
  Linux, so it takes the `[GoPlatformExclusive("windows")]` marker; `SendtoSeam` is LINUX-native with
  a golden captured on Windows, so it was REGENERATED on its own platform and marked linux
  (`e731145b7c`, train 12). **The remedy is decided by whether the package's NATIVE platform matches
  the host that captured its golden — never by how the drift looks in a diff.** The MECHANISM stays
  doctrine whatever the count: a generated ADAPTER TYPE NAME in production `.cs` follows the imported
  alias — `SockaddrInet4жΔSockaddr` on Windows, `жSockaddr` on Linux — measured against a master
  control with identical numstat on both trees; a follow-up census naming a third member was a glyph
  SUBSTRING over-match, `ΔHandle` inside `ΔHandler`, its transpile byte-identical. A class claim is
  re-derived at the TIP before it is quoted) is
  NAMED beside the package, with a standing Linux-CNR derivation (CHANGED files whose whole diff is
  the alias hunk or the adapter-name hunk) so its members surface by census rather than one at a time;
  a Linux CNR's honest verdict on this corpus WAS "clean modulo the windows-alias class" until C2's marker
  seat landed with train 14 — since then it is "clean" with no modifier (measured at `038c87786e`: 688
  byte-identical, 8 platform-exclusives skipped by name, 0 NOT MEASURED).
  ⚠ **A HAND SWEEP WITHOUT THE HARNESS'S PLATFORM SKIP-LIST TRANSPILES A PLATFORM-EXCLUSIVE PROJECT
  ON THE WRONG HOST AND READS ITS GOLDEN AS DRIFT** (2026-09-08): the alias added to a generated
  adapter name is the WRONG-PLATFORM artifact this file already documents, not a second mangling site
  — and a golden-keyed enumeration MISSES every nested sub-library (537 against CNR's 728) and skips
  the project-graph and integrity preflights. **CNR is the instrument of record; a corroborating
  sweep corroborates and nothing more** — retracted by its own author BY NAME before it reached
  anyone's census.
  ⚠ **The `.slnx` exemption criterion is platform-exclusive AND not-windows-native** (stated
  2026-09-02): the solution has ONE Windows flavour, so a `linux`/`darwin` marker unregisters the
  project and a `windows` marker changes registration not at all. A guard's own analogy check caught
  that in seconds — read the criterion's second half before predicting a registration change.
  ⚠ **F8's class ONE AXIS OVER — ARCH, and it is decided by the ORACLE (2026-09-04).** The arch a
  census transpiles for is the RUNNER's host arch: no harness passes `-platforms` and the converter
  defaults to `runtime.GOOS/GOARCH`, so two census legs on one corpus can differ at TRANSPILE while
  both compile the committed corpus identically. A behavioral project whose own **Go source does not
  BUILD on an arch is arch-exclusive by the oracle** — `go run` cannot build it there — so no layout
  dimension and no emission change can make it measurable without hardware to capture goldens;
  skip-by-name is the remedy a fleet can implement AND verify. **The acceptance number of a
  skip-by-name cut is N−1 measurable with the skip NAMED, never N** — a dispatch quoting the old N
  would read the fix as a failure.
  ⚠ **What F8 is NOT for: a flavour with no RUN layer** (2026-09-05). A guard for such a flavour is
  neither a `[GoPlatformExclusive]` row skipped fleet-wide (a guard that cannot go red ANYWHERE — a
  coverage claim the fleet cannot honour) nor a text grep over the companion (route #8). It is an ARM
  of the ACCEPTANCE PROBE that runs on that flavour's CI legs, asserting the PROPERTY (here:
  `Wait4(-1)` → ECHILD after a failed Foreground start with a non-tty `Ctty` forcing ENOTTY) with a
  neuter and a SHA-identical restore. And a 548-line hand-own companion's MISSING `using` is invisible
  to the registration ledger and to the emission diff alike — **only that flavour's own BUILD sees it,
  which is why per-flavour builds are battery legs.**
- **The emitted corpus's project-reference graph must be ACYCLIC, and that is now asserted on every
  CNR run (2026-08-30).** `check-solution-integrity.ps1` — CNR's preflight — DFSes the `src/core`
  `.csproj` graph once per `$(GoTargetOS)` (windows, linux, darwin: the per-GOOS `<ItemGroup>` blocks
  make each target a *different* graph) and requires 0 cycles, naming every cycle it finds. A C#
  project reference is a **compile-time** edge, so a cycle is MSB4006 and every project on the path
  stops building; Go's own imports are acyclic by construction, so the only thing that can create one
  is a reference the converter introduces that Go's graph does not contain — a `//go:linkname`
  forwarding property, which points wherever the directive names, in **either** direction. That is
  W1 (`docs/phase4/DESIGN-linkname-push-cycles.md`): a `-tests` conversion of `runtime` emitted
  `runtime → internal/syscall/windows`, and since Go's own imports contain
  `internal/syscall/windows → syscall → runtime`, **no conversion order can undo it** — the emitted
  edge itself has to go. The invariant this makes mechanical is narrower than "`-tests` must not
  rewrite the production emission" (which the four standing closure families contradict) and sharper
  than "the push must not add a reference": **a `-tests` conversion's production emission may differ
  from `-stdlib`'s only in ways that do not change the project GRAPH.** Positive control, kept as a
  parameter so it needs no tracked-file edit:
  `./check-solution-integrity.ps1 -TargetOS windows -InjectReference 'runtime=internal/syscall/windows'`
  must print exactly the six W1 cycles and exit 1.
- **Run the behavioral suite via the solution, not the project:** `dotnet test src/go2cs.slnx`. Running
  `dotnet test` on `BehavioralTests.csproj` directly breaks because `$(go2csPath)` (→ `$(SolutionDir)`)
  has no solution context, so the `core\golib` ref fails to resolve. The baseline solution is now an
  **`.slnx`** (`src/go2cs.slnx`); `src/go2cs-stdlib.slnx` is ALSO `.slnx` — auto-generated by the converter's `-stdlib` run (solutionGenerator.go) with solution folders mirroring the Go package namespaces. Since the trees unified its project paths match the repository's, so a fresh one is adopted by **copying it from the output root verbatim** (no rewriting; verified byte-identical). The old hand-maintained classic `.sln` is retired.
- **VS prompts to save `go2cs.slnx`/`go2cs-stdlib.slnx` on EVERY open — expected, harmless, and
  unfixable at the file level (bisected 2026-08-06, ten probe solutions).** Any `.slnx` containing a
  project that imports a `.projitems` shared-items file (`golib` and `go2cs-gen` both import
  `core/go2cs/go2cs.projitems`, the Symbols shared project) is marked dirty by VS's shared-project
  bookkeeping: classic `.sln` serialized it (`SharedMSBuildProjectFiles` section), `.slnx` has no
  element for it, so the model always differs from the parsed file while every save — including
  Save-As — writes **byte-identical** content (hash-verified; SolutionPersistence 1.0.52 round-trips
  both solutions exactly, and that is the version VS ships). Accept or dismiss the prompt, nothing
  changes on disk either way. Do NOT re-diagnose this as file drift, a generator formatting defect,
  or a reason to restructure the Symbols import. (Upstream: filed as
  [vs-solutionpersistence#156](https://github.com/microsoft/vs-solutionpersistence/issues/156) —
  the format has no shared-items element as of 1.0.52; if it gains one, or VS stops dirtying
  non-serializable state, this caveat retires.)
- **When iterating on regression work, use FILTERED + `--no-build` tests — don't run the full suite each
  time.** The full `dotnet test go2cs.slnx` rebuilds all **502** registered projects first and can take
  10+ min or hang under Visual Studio lock contention. Instead, from `src/tests/Behavioral/BehavioralTests`, run
  `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` — that reuses the existing test
  assembly and runs just that project's 4 phases (Transpile/Compile/TargetComparison/OutputComparison) in
  seconds. `--no-build` is valid as long as the `*Tests.cs` files haven't changed (`git status` them).
  Reserve a single full-suite run for final confirmation. Faster still for a pure no-regression check:
  re-transpile every behavioral dir and `git status` the `.cs` + `.csproj` — byte-identical generated code
  ⟹ identical compile+output ⟹ identical results, with no compile/run at all.
  ⚠ **A converter EMISSION change is measured by CNR BEFORE it is seated — a filtered behavioral run
  cannot see a project it does not build** (measured 2026-09-02). A seated commit's PREMISE was false
  (golib has `Func`-shaped defer overloads at arities 1–16; only arity 0 lacks one), so its rung
  rewrote corpus emission for a reason that does not exist and carried no footprint; the drift surfaced
  only when CNR ran for the NEXT commit and reported `DeferTypelessReturns` drifting on the rung alone.
  A lane that falsifies its own seated commit posts the HOLD before finishing the measurement.
- **Budget each command against its MEASURED baseline — the old flat "~3 min" cap is no longer right for
  the full runs (re-measured 2026-08-04 by r40, corpus at 569 transpiled packages / 571 registered `.csproj`).** The
  corpus keeps growing (371 → 457 → 518 → 543 → 569 packages), and both full instruments
  legitimately exceed three minutes. Timeouts must clear the real number or a healthy run gets killed
  mid-flight (a 600s ceiling killed a *passing* full suite once). ⚠ **These are DESKTOP numbers —
  and that desktop (the i9-13900K) DIED of hardware failure on 2026-08-09.** The
  same repo is also worked from a laptop (Ryzen 7 PRO 6850U, a 15–28W mobile part), where the parallel
  MSBuild phases run materially slower — a full behavioral suite measured **1,792s** there on 2026-08-07
  with nothing else running. A run over the table on the laptop is the machine, not corpus growth: do
  **not** re-baseline these rows from a laptop run, and size timeouts from the top of the range.
  ⚠ **The replacement coordinator machine (2026-08-10) is an i7-5820K — 2014 Haswell-E, 6C/12T,
  32 GB — and runs the table's rows at roughly 3–4x the i9 numbers.** Measured there on day one:
  full behavioral suite **2,820–4,131s** solo (the 4,131s end was a cold-ish tree; **2,820s**
  re-measured 2026-08-10 — either end is well over the table's 1,575s ceiling), CNR **1,505s** solo / **~3,190s** with two
  sibling lanes, converter `go test ./...` **200s** solo / **332s** loaded — ⚠ and go test's own
  DEFAULT `-timeout` is 10m, which a loaded run on this class now reaches: a healthy suite was
  killed at exactly 600.4s with a goroutine dump that reads like a hang (2026-09-01; 236s solo,
  578s under one sub-agent's load, dead at the wall under two) — pass an explicit
  `go test -timeout 30m` on any box carrying concurrent work, and read a FAIL at ~600s as the
  wall, not the code — full `go2cs.slnx` Debug
  build **1,432s** cold, `archive/zip`'s Debug test suite **774s** (vs 391s on the i9). ⚠ Those
  day-one figures are themselves STALE as the corpus grows — re-measured 2026-08-21 on the same
  i7-5820K: full behavioral suite **~6,552s at 603 packages** (and the runner batch-build default needed **9,000s** at 604 projects -- the stock 2,400s false-redded a healthy run, 2026-08-22), full `go2cs.slnx` Debug
  `--no-incremental` **~3,546s at 722 projects** — so budget those two from the 2026-08-21/22
  numbers and re-measure again at the next corpus jump. ⚠ **The `go2cs.slnx` row re-measured
  2026-08-29 on the same i7-5820K, and the spread is LOAD, not corpus growth: 845s wall SOLO at
  802 assemblies** (`--no-incremental -m -p:UseSharedCompilation=false`, golib rebuilt, 385 corpus
  warnings emitted — positive evidence of a genuine full compile rather than a skipped-work green,
  which is the only reason the number is worth quoting). The tree GREW over that interval (722
  projects → 802 assemblies) while the wall FELL 3,546s → 845s, and no corpus change runs that
  direction — so read the **3,546s as the under-sibling-load end** (it was never recorded as solo)
  and **845s as the current solo baseline**. Budget the row from the loaded end as this table
  always does — ~3,600s, not 845s — and treat a SOLO run materially past ~900s as contention to
  go find rather than work to wait out. Keep the i9
  columns as the historical reference the ratios hang off; budget commands from the i7-5820K figures
  (or 3–4x a row's i9 ceiling when unmeasured), and treat HARD-CODED harness watchdogs as suspects on
  this class of machine — at the old sizes, `PerformanceRunner`'s 600s AOT-publish cap and
  `BehavioralRunner`'s 300s build-all cap BOTH fired on healthy runs here and faked failures (each
  was raised 2026-08-10 with the evidence in a source comment; a timeout is a safety net against a
  hung child, never a performance assumption). Native-AOT perf publishes are the extreme case: ~7s
  each on the i9 in the stub era, **~25 min each** on this machine now that ILC compiles the full
  converted-stdlib closure per benchmark (post-unification), so a full perf run is hours, not
  minutes — and it must run SOLO: concurrent lane load pushed a healthy publish past even an 1,800s
  cap once, and only the Measure phase's numbers are trustworthy on a quiet machine anyway:

  | Command | Measured (warm) | Set timeout | Notes |
  |---|---|---|---|
  | `run-behavioral.ps1` (full, 4 phases) | **~370–1575s (6–26 min; 642s measured 2026-08-07 at 549 projects with a sibling lane converting; 416–957s on 2026-08-05 SOLO at 545 across four r41 stage gates — the spread is warm-vs-cold C# build state, not load; 626s on 2026-08-04 at 544, 1575s on 2026-08-02 with THREE sibling worktrees running pipelines)** | 2100s | 549/549 Transpile+Compile+Target; 523 Output-compared, 26 skipped (no `package main`); the top of the range is concurrent-lane load — budget for it. ⚠ At that load the **Go toolchain itself** can crash building one project (`panic: … compress/flate.(*huff…` inside `go build`) and the runner reports it as a Go build failure; re-run that one project filtered before believing it. Data point 2026-09-01: **1,916s at 652 projects, laptop-class host, SOLO, runner invoked DIRECTLY** (not via the Stop-preference wrapper) with `--build-timeout 10800 --build-one-timeout 900` — the stock 2400s batch cap sized at ~604 projects would have reported the whole corpus NOT MEASURED at 652 |
  | `check-no-regression.ps1` (full) | **~1,050–1,750s (17–29 min; re-measured 2026-08-17/19 at ~625 packages on the i7-5820K: 1,059s and 1,132s solo, 1,440s and 1,711s under sibling-lane load; laptops ran 720s (G) and 1,060s (R). The prior row read 350–510s/700s at 574 packages on the dead i9 — a timeout kept at that figure kills every healthy run on this corpus)** | 2400s | transpile-only, no compile/run; re-transpiles unconditionally |
  | `run-behavioral.ps1 --filter <Name>` | **~10–20s** (8 projects) | default | the iteration loop — use this, not the full suite |
  | `go2cs -stdlib -comments` (full reconvert) | **~195–240s (240s measured r47a 2026-08-08 with two sibling lanes; 223s at r41, 2026-08-05)** | 600s | 307 projects; per-file work is sub-second, the cost is `go/packages`. A three-target `-platforms` merge is ~3x this (545s measured r50a) |
  | single `core` pkg build | **~6s** (log/slog) – **~60s** cold (go/types) | 180–400s | cold includes the dependency chain |
  | full `go2cs-stdlib.slnx` build | **~92–188s** warm (307 projects; 149s measured r50a at `-p:GoTargetOS=windows`, 188s at r41 and 158s at r40, all with `-p:UseSharedCompilation=false`, the isolation flag a lane uses instead of `build-server shutdown`). ⚠ **i7-5820K on a healthy disk: 516s** `--no-incremental` (2026-08-14) | 600s (900s on the i7 class) | cold restore adds a few minutes. `-p:GoTargetOS=linux` is a DIFFERENT build and **completes clean: 307/307, 0 errors, 475s** (2026-08-14, after the three-target regen wave — `docs/phase4/CENSUS-linux-compile-wall.md` §10). It must be run `--no-incremental`: what differs between targets is the `<Compile>` ITEM SET, not any source timestamp |
  | full `go2cs.slnx` build | **~87s** `--no-incremental` / **~39s** incremental (573 projects; measured 2026-08-07) | 900s | the ONLY gate that compiles the non-generated solution members (utilities, examples) — run it after any golib/runtime API change. ⚠ Under concurrent-lane load a `go2cs-gen` run can die with `AccessViolationException` inside `TypeGenerator`'s recursive `PromotedStructDeclarations`, reported as an `error` against the package (seen once on `core/runtime`, NOT reproducible in two immediate retries with identical flags): re-run before believing it, exactly as with the Go-toolchain crash above |
  | `run-validated-sweep.ps1` (full roster) | **~46–53 min solo (3,138s measured 2026-08-07 at 109 packages / 13,611 verdicts; the roster is 131 packages / 14,769 matching verdicts / 47 disclosed, re-measured 2026-08-14 — so budget well ABOVE the 3,138s figure, and re-measure; ~90+ min under two concurrent lane loads — both r47 attempts were killed externally before finishing, so no clean loaded figure exists)** | run it BACKGROUNDED from the COORDINATOR session only — ⚠ a LANE parking a detached sweep and ending its turn gets it KILLED (the lane's process tree is reaped; happened twice on 2026-08-08 at 106/110 and 98/110, log ends between packages with no summary — recovery: re-run `roster − logged` inline and check the verdict arithmetic closes) | ~29 s for a typical package; i9 full roster measured **7,059-7,705s** at 159-162 rows (2026-08-22) -- the 46-53 min row is the dead i9-13900K era and stands only as the ratio anchor; use `-Filter` for anything but a final gate. ⚠ **ELEVEN** packages carry per-package deadline FLOORS in the script's `$longTimeouts` (re-counted 2026-09-02; `sync/atomic` 60m, `net` 40m and `net/http` 60m joined since the eight — the last sized to a TRUNCATED Debug measurement on the i7 class, where the row's two arms bracket the train's 30m at 1,836 s and a deadline-killed 2,171 s) (`hash/maphash` 60m, `index/suffixarray` 120m, `crypto/dsa` 120m, `archive/zip` 60m, `go/parser` 90m, `crypto/internal/mlkem768` 30m, `crypto/tls` 30m, `time` 40m -- its 1.23.12 suite is 169 tests and ~19 min on laptop-class — the table grew three rows and two floors moved by 2026-08-25, which is WHY the shard map derives its reserved set from the script at generation time instead of copying this sentence; **the script is the authority, this prose is a pointer**), **slow-host-calibrated** since 2026-08-10 — the original i9-sized values false-red every bare sweep on this machine class (hash/maphash and crypto/dsa both reported `FAIL … package timeout after 00:30:00` here; maphash then validated **22/22 in 2,406 s / 40.1 min** given room). The table is also a **floor, not an override** since the same date: a LARGER `-TestTimeout` raises it for a still-slower box (a smaller one still loses, since under-budgeting these four is the false red the table exists to prevent) — before that fix the flag was silently ignored for exactly the four packages that need it |

  Materially *past* these means the test host has hung under lock contention, not real work — stop and
  clear it rather than waiting 10–20 min. **Re-measure and update this table when the corpus grows again**;
  a stale baseline is what makes a healthy run look hung (and vice versa). The spreads above are real
  run-to-run variance on the same corpus (machine load), so budget from the TOP of the range, not the
  midpoint. A converter rebuild invalidates every project's up-to-date check, so the *next* full run
  after one always pays full price.
  ⚠ **An EXTRAPOLATION written in a MEASUREMENT's voice is a false measurement** (2026-09-02): a
  budget comment presented "~236 s fixed, ~62 min" as measured when the fixed term is not constant at
  all (6 shared deps in a 3-project slice against ~31 corpus-wide) and the runs behind it had timed a
  different flavour. The fix is a LABEL, not a better guess — mark the figure PROVISIONAL, state the
  measured points separately, and let the first real run replace it. Every row in the table above is
  a measurement or it does not belong in it.

  ⚠ **`BehavioralRunner` has its OWN internal timeout budgets, and no timeout the CALLER sets can
  influence them** — a generous outer budget on the `run-behavioral.ps1` call does nothing if the
  runner kills its own child first. They were hardcoded constants until 2026-08-10; they are now
  overridable, in SECONDS, at **flag > environment variable > default**:
  `--build-timeout`/`GO2CS_BUILD_TIMEOUT` (batch build, **2400**), `--build-one-timeout`/
  `GO2CS_BUILD_ONE_TIMEOUT` (per-project build, shared-dep pre-build, `go build`, **300**),
  `--transpile-timeout`/`GO2CS_TRANSPILE_TIMEOUT` (**60**), `--run-timeout`/`GO2CS_RUN_TIMEOUT`
  (one program run in the Output phase, **30**). The build defaults are sized for the slowest
  legitimate host per the safety-net doctrine (the i7-5820K measurement below is what sized them);
  a fast lane that wants the old fail-fast behavior opts DOWN explicitly (`--build-timeout 300`).
  **The slow-machine row this table was missing (measured 2026-08-10, i7-5820K 6C/12T, ~3x slower than
  the desktop rows, at 555 packages):** the one-shot parallel build exceeded the stock 300 s **cold and
  warm alike** — warm state cannot save it, because the Transpile phase rewrites every `.cs` immediately
  before Compile, so the batch is never an incremental no-op. For scale, a full
  `dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false` of the same tree took **1,432 s
  cold** (573 projects, 0 errors), ~5x the old 300 s batch budget; a single cold filtered project
  measured 163 s. That measurement is what sized the current build defaults, so such a machine needs
  no configuration; the overrides exist to opt a fast lane back down or to survive a still-slower host.
  **A budget that expires is now reported as `NOT MEASURED`, never as a failure** — a fourth
  `Status.Timeout` alongside Pass/Fail/Skip, borrowing CNR's word for the same idea. This closes a
  **FALSE RED**, the mirror of the false-green routes catalogued above: on the cold slow machine the
  batch timed out, all 555 projects fell to the sequential per-project fallback, each *also* exceeded
  180 s (every one must first build the core dependency closure), and ~15 minutes produced zero
  assemblies and 555 `Status.Fail` entries that read exactly like a corpus regression. Timeouts still
  fail the run and still exit 1 — an unmeasured project must never read as a pass — but they are
  counted, listed and summarized separately. Two related traps the same change closed: an Output-phase
  run timeout used to surface as `exit code mismatch: C# -1 vs Go 0`, i.e. as a *behavioral* divergence
  naming a real test; and the per-project fallback now bails out after **3 consecutive** timeouts rather
  than spending the full budget on all 555 to re-learn one fact.
  ⚠ **A behavioral leg that must SHARD, and the two facts that decide how (2026-09-02).** Every
  behavioral project's build output copies the same ~55-dll core closure into its own `bin` (~29 MB
  each, ~20.5 GB at 695 projects), so an unfiltered Output leg cannot fit a hosted runner's disk in one
  batch: the ruled shape is shard-with-purge — alphabetical slices, `clean-bin` between them, verdicts
  unioned — never a narrowed enumeration (the durable follow-up is a shared-closure csproj template).
  And **`BehavioralRunner`'s `--filter` is a case-insensitive SUBSTRING** (filter `S` matched 455 of
  664), so no filter set can partition the enumeration: a sharding leg takes an INDEX SLICE over the
  deepest-first list and asserts the slice counts sum to the whole.
  ⚠ **THE RUNNER'S OUTPUT PREDICATE IS THE OPT-IN ATTRIBUTE, NOT `package main`** (2026-09-08): the
  predicate is "the package's `package_info.cs` holds a line equal to the attribute", so the projects
  WITHOUT a `main` are a strict SUBSET of those skipped, and a battery expectation written as
  "enumeration minus the no-main projects" reads a CORRECT run as short by the difference. Two
  internally consistent WRONG arithmetics preceded the one that closed — the first double-subtracting
  the platform-exclusives, which sit OUTSIDE the enumeration entirely. Companions from the same run:
  **the converter REBUILT under the two-pin pairing came out BYTE-IDENTICAL** (hash equal, mtime
  moved), so the build is deterministic, measured for the first time; **a DIRECT runner invocation
  SKIPS the wrapper's disk preflight**, so that floor is checked by hand before launch; and a tree
  reading clean after a full round of in-place transpiles is the Target phase's verdict reached by a
  second route.
  ⚠ **Piping a long run through `Select-Object -Last N` buffers ALL output until it completes** — a
  backgrounded suite will look stuck at its first line for its entire duration. Check liveness with
  `Get-Process BehavioralRunner,dotnet`, not the output file. **`-First N` is WORSE: it terminates
  the pipeline once satisfied and KILLS the upstream native process mid-run** (measured 2026-08-16:
  a `-stdlib` reconvert died at ~100/304 with exit −1, reading exactly like a converter failure).
  Redirect long runs to a file and read the file — and redirect with **`Start-Process
  -RedirectStandardOutput`**, not `... *>&1 | Out-File`: the pipeline form BUFFERS, so a run that
  dies leaves a few-hundred-byte log ending mid-line, indistinguishable from an external kill
  (measured 2026-08-31, a 485-byte log from a dead full-suite run). In BASH, `*>&1` is not
  redirection syntax at all — the shell GLOBS it, silently no-op'ing the command (measured
  2026-08-31: one CNR and two runner attempts read as failures that never ran).
  ⚠ **A COMMAND THAT DOES TWO THINGS CAN HALF-SUCCEED, AND THE EXIT CODE TELLS YOU NOTHING ABOUT WHICH
  HALF.** One invocation launched a detached build, wrote a mailbox entry file and posted it; the tool's
  2-minute cap fired partway, **the build started and the post silently did not**, and the entry file
  the post needed was never written — so the retry failed on a MISSING FILE rather than on the real
  cause (2026-09-07). Check BOTH effects independently (a process census for the build, the mailbox tip
  for the post); **a timeout does not roll back what already ran.** ⚠ Its neighbour: **a tool's own
  timeout is not the COMMAND's timeout, and the shorter one wins silently** — a solution build given a
  30-minute internal budget died at 10, the harness cap, reporting exit 143 with an empty log, which
  reads exactly like a build that produced nothing. ⚠ And **PowerShell's `.Count` on an EMPTY result
  prints BLANK, not `0`** — `(Get-CimInstance … | Where-Object {…}).Count` rendered as `go2cs.exe:`
  with nothing after it and was read as "no converter running" when the honest reading is "this told me
  nothing": wrap it `@(...).Count`, and read a blank where a number belongs as a BROKEN INSTRUMENT.
  **⚠ PowerShell-REDIRECTED output is UTF-16, and an ASCII grep over it returns a well-formed
  EMPTY** (measured twice 2026-08-31, independently): both `go2cs.exe … > log 2>&1` and a
  `Tee-Object` log land as UTF-16LE, so `grep <marker>` finds nothing and reads as "probes never
  fired" / "the run never happened" — a full retraction was built on six such empty greps, and a
  CNR verdict was nearly lost the same way. The tell costs one command:
  `head -c 200 <log> | tr -d -c '\000' | wc -c` — a nonzero NUL count means every grep against
  that log has been lying — then decode (`iconv -f UTF-16LE`) before grepping. Same
  silence-not-error family as the globbed `*>&1` and the buffered pipe above.
  **⚠ THE TRUNCATED-LOG READING INVERTS FOR POWERSHELL WRAPPERS (measured 2026-09-01, a
  self-inflicted two-runner race):** a wrapper running at `$ErrorActionPreference='Stop'`
  (`run-behavioral.ps1` line 49) dies on the FIRST native stderr line — killing the WRAPPER and
  leaving the runner alive, orphaned, and invisible. The truncated log reads exactly like the run
  being killed and invites the restart that puts two runners in one behavioral tree. Before
  believing a truncated wrapper log, census for the CHILD by executable path; a lane driving a
  long native child invokes it DIRECTLY (or at `'Continue'`), never through a Stop-preference
  wrapper. And never inject
  non-ASCII C# source (`Ꮡ`, `ж`, `Δ`) through a PowerShell command STRING — the argument pass
  mojibakes it even when file I/O is correct; write such content with the Edit/Write tools.
  **⚠ The same mojibake hits a `.ps1` SCRIPT FILE ITSELF when Windows PowerShell 5.1 parses it —
  and unlike the argument case, file I/O is NOT correct here, so the usual fix does not apply**
  (measured 2026-08-30, the syscall-pinning census-guard lane). A `.ps1` written UTF-8 without a
  BOM (the Write/Edit tools' default) is read back by 5.1's PARSER under the system codepage, not
  UTF-8 — so a literal non-ASCII glyph embedded in the script's own source (a regex pattern
  matching `ᴋ`, a string comparison against `Ꮡ`) silently decodes to mojibake at PARSE time, before
  the script ever runs. The instrument does not error: it runs, and reports whatever a
  never-matching pattern reports — in this case a false "0 sites found" that read as a correct RED
  result against a not-yet-fixed corpus, and stayed silently wrong against a freshly fixed one
  until the fresh run's *also* being zero broke the positive control. The fix is a UTF-8 BOM on the
  `.ps1` file itself (`[System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true))`
  after writing it any other way) — confirmed to make 5.1 parse the literal correctly. Positive-control
  any regex-bearing PowerShell instrument that embeds a converter glyph literally: run it against a
  known-populated target and confirm it finds a nonzero count before trusting a zero anywhere else.
  **⚠ A SHARED PowerShell instrument owes a run on BOTH editions before it banks (measured
  2026-09-02).** `_roster.ps1`'s comparison reader took `Add-Type -AssemblyName
  System.Web.Extensions` — a genuine PS 5.1 case-folding fix, smoke-proven on Windows only, and
  .NET-Framework-ONLY. Under pwsh 7 the script died at its second block, so the sweep's three
  absorption arms (host-conditional, capability-absent, host-limit) silently **DECLINED on every
  Linux host**, and the catch's own message named the missing assembly rather than the missing
  capability — pointing at the wrong artifact, in the file that decides whether a row banks. The
  fix is an edition-conditional reader (Desktop keeps `JavaScriptSerializer`; Core uses
  `System.Text.Json.JsonDocument`, explicit, never `-AsHashtable` behaviour inherited from a newer
  host), and the guard exercises both. **Rule: 5.1 on a Windows lane AND 7 on a Linux lane — or the
  OS-matrix linux leg — before a shared `.ps1` change merges.**
  ⚠ **What that check IS, stated 2026-09-02: the PARSE of every shared script under pwsh 7 Core, plus
  one row actually run.** A cloud container may carry NO PowerShell at all (`dotnet tool install
  --global PowerShell` lands one on the user's tool path) and its writable allowance may sit under the
  sweep's own disk-preflight floor — such a host runs the edition and gate checks with
  `-IgnoreDiskPreflight` STATED, and never banks a Linux row.
  ⚠ **Verify what a host HAS before stating what it LACKS** (2026-09-04): the container that reported
  "no PowerShell at all" corrected itself the same hour — pwsh 7.6.5 was installed at the dotnet
  global-tool path and simply not on `PATH`.
  ⚠ **And its LOCAL form, for a lane with no Linux host reachable** (2026-09-04): the load-bearing
  half is EXERCISED on 5.1 Desktop AND pwsh 7 Core on the REAL path with a DECOY (a Framework-only API
  behind a variable is the `System.Web.Extensions` shape), with the Linux host NAMED as the honest
  closer. Measured is not parsed, and not-yet-closed is not untested — say which of the three a check
  was.
  **⚠ And a PowerShell FUNCTION named `Git` shadows `git.exe`** — command names resolve
  case-insensitively, so `& git` inside it recurses until "call depth overflow" (measured 2026-09-02,
  coordinator). The overflow line, captured through `2>&1`, then counted as ONE dirty entry in a
  `status --porcelain` check and aborted a rebuild twice with a message that read like real tree
  dirt. Name wrappers distinctly, invoke `git.exe` explicitly, and take `status --porcelain` with
  stderr dropped.
  **⚠ The same case-insensitivity binds PowerShell VARIABLE names** (measured 2026-09-02): a results
  array `$main = @()` silently overwrote the `$Main` worktree PARAMETER, so every main-tree round ran
  against an empty path and reported "(no verdict line) 0s" while the control rounds read fine. Name
  arrays distinctly from parameters, and let a function that RETURNS a number write its progress with
  `Write-Host` — a body that `Write-Output`s its progress returns those lines AS its value, and the
  caller's `Measure-Object` then chokes on strings. **And a `git` command run from a DELETED cwd prints
  plausible answers** — "0 commits not in master" for all seven branches, the only tell a
  `getcwd: cannot access parent directories` line at the END of the output: re-run from the repo root
  before believing any count taken after a worktree removal.
- **⚠ Before a divergence is NAMED, read the ORACLE at the ROW's own source and measure it under the
  SAME shape (2026-09-02).** A converted `crypto/tls` shim exiting 89 under bogo's flag set was
  compared against Go's 2 measured with the flag ALONE — and the answer was in Go's OWN source, not in
  a run: the row's TestMain exits 89 under bogo mode by its own line. The source to read is the
  row's own `TestMain`, not the file the flag was registered in: crypto/tls's prints `Usage of %s` over
  `os.Args` and exits 89 in bogo mode by its own line, and both were reported as divergences from what
  the flag package "normally does" — **"not what the package normally does" is not "not what Go does
  here."** Two neighbours from the same week. A shared CONVENTION name is not a shared MECHANISM:
  `GO_WANT_HELPER_PROCESS` spans suites whose re-exec paths differ (`exec.Command` →
  `posixSpawnForkExec` against `syscall.Exec` → `execve`), so a "row X after fix Y" dependency adopted
  from a lane's note was false and died on the fix's own measured null — read the CALL PATH before
  scheduling a row behind a fix. And a dramatic finding is re-derived from ITS OWN record before it is
  posted: a `"disclosed": []` grep read off `net/http`'s record was nearly published as `sync`
  falsifying its own `TestOnceXGC` disclosure — the record one file over is not this row's record.

### Performance comparison suite (`src/tests/Performance`, 2026-07-02)
- **Purpose:** answer "how fast is the transpiled C# vs the original Go?" — 14 small `Perf*` benchmark
  projects (Startup, Fib, Sieve, MatMul, String, StringView, StringMatch, Map, Sort, Channel, IfaceCall,
  Iface, IfaceShell, RefLower), each a behavioral-test-shaped folder,
  measured across **three variants**: Go binary, C# JIT (`Release`), C# **Native AOT** self-contained.
  Drive via **`run-performance.ps1 [--filter X] [--no-aot] [--runs N] [--update-readme]`** (standalone
  `PerformanceRunner`, no testhost; phases Transpile → Build → Verify → Measure; Verify requires identical
  timing-filtered stdout across all three binaries before anything is timed). The results table lives in
  `src/tests/Performance/README.md` between `PERF-RESULTS` markers (`--update-readme` rewrites it; prior
  toolchain tables accumulate in its *History* section for .NET 9 → 10 comparisons).
- **Mechanics gotchas:** benchmarks self-time via `time.Now().UnixNano()` (added to the baseline
  `core/time` stub for this) and print `elapsed_ns:` lines the runner strips before output comparison; the
  converter **regenerates each benchmark csproj on transpile**, so shared settings live in
  `Directory.Build.props`/`.targets` there (AOT is gated by custom `-p:PerfAot=true` — passing `PublishAot`
  globally breaks the netstandard2.0 `go2cs-gen` analyzer with NETSDK1207); AOT publish needs MSVC
  `link.exe` and the runner prepends the VS Installer dir to PATH for the SDK's `vswhere` probe; AOT trims
  with `TrimMode=partial` because golib `fmt` formatting and sort's `Interface<T>` bind members via
  reflection. ⚠ Cost changed at the 2026-08-01 tree unification: each AOT publish now ILC-compiles the
  full converted-stdlib closure (~7 s each in the stub era; **~25 min each on the i7-5820K**), so a full
  run is HOURS and must run SOLO — concurrent lane load once pushed a healthy publish past an 1,800s
  watchdog. `--no-aot` drops the whole column and stays fast. Keep each
  benchmark ≥50 ms and output deterministic (inline xorshift, no `math/rand`).
- **⚠ Two measured 2026-09-02, both from the TLS-handshake row.** Verify found a **SEMANTIC** divergence
  before anything was timed: the converted `crypto/tls` negotiates ChaCha20-Poly1305 where Go negotiates
  AES-128-GCM on the same host, because `internal/cpu`'s `doinit()` calls `cpuid` — x86 assembly, a
  throwing generated stub — and the throw is SWALLOWED, so x86 feature detection is all-false corpus-wide
  and every AES-NI/AVX fast path runs its software fallback. **A silently-ignored package init is a
  corpus-wide false green**; trace the swallow before pricing anything above it. And for a
  near-threshold SERIAL-latency row, **core count is the wrong lever — a NATIVE control on the same host
  is what exonerates the stack**: Go passed at 250 ms where the managed side failed at 250/500/1000 ms in
  the same run, leaving managed-vs-native handshake latency as the residual.
- **⚠ Both halves of that row are CORRECTED by later measurement (2026-09-02) — read them together.**
  There is no swallow: `schedinit` never runs, so `cpuinit`/`cpu.Initialize`/`doinit`/`cpuid` are
  UNREACHABLE and every `X86.Has*` is simply its zero value; the fix is a `[ModuleInitializer]`
  stand-in (the `goenvs`/`goargs` precedent) hand-owning `internal/cpu` over
  `System.Runtime.Intrinsics.X86`, 14 of Go's 20 flags mapped and 5 left false as the conservative
  direction. **A silently-UNREACHED package init is the same corpus-wide false green as a swallowed
  one** — trace the CALL CHAIN, not a `catch`. And the handshake residual was FALSIFIED as the h2
  pair's cause: a clean negative A/B moved 0 rows with AES-GCM negotiated, an isolated handshake is
  ~44 ms (which cannot blow a 250 ms rung), and the pair is a build-CONFIGURATION artifact — see the
  Debug-publish rule above; a cut's justification stays what it MEASURED.
- **⚠ The unreached-init class, ONE MEMBER WIDER (2026-09-05).** A converted package can be TWO
  CONTRADICTING HALF-IMPLEMENTATIONS — hand-owned no-ops sitting beside converted checkers that
  nil-deref or return at their first line — so neither half can be read as the package's behaviour.
  Its root is the same door as `internal/cpu`'s: `debug.cgocheck = 1` is assigned only on the
  `schedinit` → `parsedebugvars` path, which never runs, so the field's DECLARED value is not the
  value any check sees. **Read the DEFAULT's ASSIGNMENT PATH, not the field's declaration, before
  believing a check runs.**
  ⚠ **A CONVERTED CALL GRAPH IS NOT GO'S CALL GRAPH** (2026-09-08): a builtin displaced into golib
  SEVERS the chain below it, and **an UNRESOLVED node in a call-graph census is precisely where the
  severance announces itself** — one author had that tell in hand, filed it as a footnote, and traced
  Go's whole write chain anyway "because that is what Go does". Distinct from citing a call site
  without reading the callee: here every callee Go HAS was read, and nobody asked which of them our
  corpus still OWNS. ⚠ **The measurement that settled it: a prediction read off Go's call graph does
  not carry to the conversion when a node is DISPLACED at the golib boundary.** One flavour was
  predicted MUTE on a runtime fatal — Go's print chain bottoms out in a bodyless stub there — and it
  PRINTED, frame for frame identical to the other flavour, because the converted print IS the golib
  call and the runtime's own write chain is never entered on ANY flavour; the stub is exactly as read
  and simply UNREACHED. Two companions: the falsifier that FIRED took an item OFF the remedy (one
  shape on three flavours rather than two), and the insurance control was NOT load-bearing for the
  non-null result and its author said so rather than letting it read as the rescue.

### Adding a regression test when a converter defect is fixed
When a meaningful converter bug is fixed, lock it in with a behavioral test so later changes can't silently
reintroduce it. **Prefer extending an existing behavioral project** if one already covers a similar
construct; otherwise add a new one (example: `tests/Behavioral/GlobalStructFieldPointers`, which guards the
`&cpu.X86.HasADX` cross-file address-of-field fix). To add one:
1. **New folder** `src/tests/Behavioral/<Name>/` with a Go program that *exercises the specific construct*
   (multiple `.go` files are fine and run as one package — needed to reproduce cross-file bugs). Include a
   `go.mod` (`module go2cs/<Name>` — ⚠ but a test carrying a nested sub-library PACKAGE inside its own
   module takes a BARE `module <Name>` instead, or the sub-library's namespace and the consumer's
   emitted alias disagree and the parent fails CS0234; measured 2026-09-02, and the corpus agrees —
   24 of the 27 behavioral projects with a nested sub-package are bare, and the three that are not give
   the sub-library its own `go.mod`, i.e. a separate module path), and copy `go2cs.ico` + a
   `<Name>.csproj` from a sibling test (adjust
   `AssemblyName`; keep the `golib`/`fmt` refs the program needs). Verify it with `go run .` first. ⚠ A test that imports a SIBLING sub-library names its module `<Name>` with NO `go2cs/` prefix and imports `<Name>/<sub>` (the `NamedSliceChildPkg`/`netlike` pattern): the converter references the sub-library as `<sub>/<Name>.<sub>.csproj`, the name it also emits for the sub-library's own project. Under `module go2cs/<Name>` the parent's reference becomes `go2cs.<Name>.<sub>.csproj`, a file nothing emits, and the build dies CS0246 on the sub-library's namespace inside the GENERATED shells -- pointing away from the `go.mod` (paid 2026-09-05, the ReflectFieldMetadata guard).
2. **Make the Go↔C# output match** so `OutputComparisonTests` passes. Mind known runtime limitations — e.g.
   `Ꮡ(value)` (address of a non-boxed value) currently boxes a *copy*, so don't write through a
   `&global.field` pointer and then read the *original* global; read back through the same pointer.
3. **Register in the solution** — add a `<Project Path="tests/Behavioral/<Name>/<Name>.csproj" />` line under
   the `/tests/behavioral/target-projects/` folder in `src/go2cs.slnx` (alphabetical). **If the test pulls in
   a sibling library sub-project via `<ProjectReference>`** (e.g. `GoNamespaceShadow` → `nsshadowlib/go.nsshadow.csproj`),
   register **that** too, on the line right after its parent (the pattern used by `IoLike`→`IoLike/FsLike`,
   `NamedSliceChildPkg`→`.../netlike`). **Then verify it stuck** — run **`./check-solution-integrity.ps1`**
   (from `src/tests/Behavioral`): it asserts every behavioral `.csproj` on disk is registered in `go2cs.slnx`
   and flags any dangling entry, exit-1 on violation. (Also runs automatically as the preflight of
   `check-no-regression.ps1`.) This matters because the harness builds each `.csproj` **by path**, not via the
   solution, so a missing registration still passes the whole suite — it only breaks the `go2cs.slnx` build in
   Visual Studio (the unregistered project loses the Debug/`$(go2csPath)` context and its `core\*`/`gen\*` refs
   fail: CS0246/CS0234). That is exactly how `nsshadow` slipped through (added in `96eff53cd`, unregistered
   until `53dd2497e`). If Visual Studio has the `.slnx` open it can rewrite/reformat the file and silently drop
   an external edit — re-add and re-verify if so.
   **⚠ Windows CASE trap when you `git add` the new folder (found 2026-08-07).** `git add .` / `git add -A`
   — and any add run from a cwd *inside* the tree — records the path git gets from **readdir, i.e. the
   ON-DISK casing**, whereas an explicit lowercase pathspec (`git add src/tests/Behavioral/<Name>`) is
   canonicalized to the casing already in the index. Under `core.ignorecase=true` the difference is
   invisible locally, so a clone whose `src\tests` had drifted to a capital `src\Tests` on disk banked
   `DeferFrameScopes` at `src/Tests/Behavioral/…` while the other 4,240 files stayed `src/tests/…` — ONE
   directory on Windows, TWO on any case-sensitive filesystem (Linux clone, container CI, case-sensitive
   macOS volume), where the `.slnx`'s lowercase `tests/Behavioral/…` registration then fails to resolve.
   `check-solution-integrity.ps1` now asserts case-sensitively that every tracked path under the behavioral
   tree is exactly `src/tests/Behavioral/…`, so this cannot recur silently. If it fires: `git mv` will NOT
   do a case-only rename on Windows — rewrite the INDEX with plumbing (`git update-index --force-remove
   <wrong-cased-path>`, then `git update-index --add --cacheinfo 100644,<sha>,<lowercase-path>` reusing the
   SHAs from `git ls-tree -r HEAD`, which keeps the blobs byte-identical) — **and fix the on-disk directory
   casing too** (rename through a temp name, `Tests` → `__tmp__` → `tests`), or the next `git add -A`
   re-creates the wrong path. Both are working-tree-invisible: `git status` stays clean throughout.
4. **Transpile once** (`go2cs.exe src/tests/Behavioral/<Name>`, no `-comments` — behavioral goldens omit
   them) to generate the `.cs` + `package_info.cs`. For output comparison, add `[GoTestMatchingConsoleOutput]`
   to the generated `package_info.cs` class (a hand-added attribute the converter preserves).
5. **Generate tests + goldens:** run the **`UpdateTestTargets`** utility **with `--createTargetFiles`** (from
   its `bin/Debug/net10.0`). It scans every `tests/Behavioral/*` folder, rewrites the `// <TestMethods>`
   blocks in all four `*Tests.cs` classes (adding `Check<Name>()`), then **re-transpiles every project it
   is about to re-baseline** and copies each freshly transpiled `.cs` to a `.cs.target` golden. It only
   emits an `OutputComparison` test for projects whose `package_info.cs` has
   `[GoTestMatchingConsoleOutput]`. Afterward, `git status` should show only your new project + four
   `+3`-line test-class diffs (no other `.target` churn). The transpile is what makes step 4 above a
   convenience rather than a prerequisite — and because it walks the whole corpus, a whole-tree run is
   CNR-length; add **`--only <Name>`** to re-baseline one project (and to exercise the refusal branch,
   which exits non-zero naming any project whose transpile failed, timed out, or degraded).
   ⚠ **DO NOT CAPTURE A GOLDEN AGAINST AN UNSETTLED QUESTION.** A probe's disposition arms measured a
   still-open bridge question, so a `.cs.target` taken that day would have recorded what the bridge DOES
   rather than what it SHOULD do. **A golden is read as a SPECIFICATION by everyone who meets it
   afterwards, and nothing in the file says which it is** — so capturing one against an open question
   converts a divergence into a contract by accident. Same move as laundering a bug into a disclosure
   class, in a different artifact.
   ⚠ **ONE WORKTREE PER CUT — `UpdateTestTargets` enumerates the DIRECTORY, not your change**
   (measured 2026-09-02): a stray untracked project left by ANOTHER cut was enumerated into this
   cut's four test classes, and the ASYMMETRY is the tell — one new project gives `3/3/3/3`, that run
   gave `6/3/6/6`. Two dirty converter files from the same neighbour would also have made any build
   there measure a MIX. Neither fails a gate, so the check is the diff's shape: count the added
   `Check<Name>()` lines per class before staging.
6. **Verify (filtered, fast):** preferred — from `src/tests/Behavioral`, run
   `./run-behavioral.ps1 --filter <Name>` → the 4 phases (Transpile, Compile, TargetComparison,
   OutputComparison) for that project via the standalone runner, in seconds, with no testhost/lock risk.
   Equivalent MSTest path (still valid): from `src/tests/Behavioral/BehavioralTests`, run
   `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"`. Either way, avoid the full
   `dotnet test go2cs.slnx` while iterating — it rebuilds everything and can hang under VS lock contention
   (see the test-harness notes above). The golden comparison is line-ending-insensitive, so a multi-line
   string literal needs **no** `.gitattributes` handling for the byte compare — mark the `.cs` `-text` **only
   if** the compiled program's behavior/output depends on that literal's exact newlines (autocrlf gotcha above).
7. **Record the conversion decision (keep the strategy docs living).** The conversion strategy lives in
   **two** documents, and a notable decision updates the right one (often both):
   - [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) — the exhaustive
     **technical reference**. Nearly every conversion decision lands here: add or update the `###` subsection
     under the matching `##` topic with the emitted form, the edge case, the reasoning, and the guarding
     behavioral test. This is where the deep detail and history accumulate.
   - [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — the high-level **summary** (one section
     per topic, tight prose + a couple of real Go→C# examples, each linking into the reference). Update it
     only when the decision changes the *headline* mapping of a construct or warrants a better/clearer
     example — not for every edge-case fix. Keep it short and readable; push the detail to the reference.

   Do this **in the same change** so both docs keep matching reality. Verify every C# snippet against the
   actual `.cs.target` golden (it is the authoritative record of emitted forms — e.g. `u8` format strings,
   `throw panic(...)`, `ж<T>`/`Ꮡ`); the summary's examples should prefer real snippets pulled from the
   converted stdlib in `src/core` (Go source ↔ converted C#). Skip only for pure bug-fixes that restore an
   already-documented behavior. (This rule is not limited to the regression-test flow — it applies to *any*
   commit that lands a notable conversion decision.)
   ⚠ **A golden regenerated under the wrong toolchain is worse than every other toolchain trap this
   repo carries, because those eventually fail loudly while this one rewrites the RECORD later
   comparisons are measured against** — the wrong release's emission becomes the definition of correct
   and every subsequent green is measured against it, at exit 0 with nothing refused. So the
   re-transpile that precedes a `--createTargetFiles` copy is toolchain-pinned like any other emission,
   and any golden-regeneration path that rebuilds `go2cs.exe` itself **resolves and CHECKS the
   toolchain first and ABORTS naming both releases** — printing the pin is not enough; this repo has
   already paid for an instrument that printed its pin and carried on.
   ⚠ **A GOLDEN RE-BASELINED UNDER A RUN GOROOT THAT DIFFERS FROM THE CORPUS'S RELEASE RECORDS THE
   EMISSION FOR A CORPUS THAT DOES NOT EXIST, and the tell is Target GREEN with Compile RED**
   (2026-09-08): eight goldens "drifted" under the newer run pin by exactly one change each — an alias
   prefix dropping off one package's name — the re-baseline made each golden match the emission to the
   byte, and the emission did NOT BUILD against the corpus at its own release. Under the two-pin
   pairing (converter built at the newer release, environment re-exported to the corpus's) all eight
   are byte-identical to the goldens already committed. **Same binary, same sources, opposite stamp:
   the decision is a function of the LOADED CLOSURE, i.e. of the run GOROOT. A golden is re-baselined
   AT the corpus hop, never in the window before it.**
   ⚠ **Placement: a guard must precede the check that DRIVES the bad outcome, or it is inert while
   looking identical in review.** The shared staleness predicate compares the binary's embedded release
   against live GOVERSION, so on an unpinned host **the predicate itself triggers the wrong-toolchain
   rebuild** — a pin check placed after it runs on a binary already rebuilt wrong. Ask what ORDER the
   bad outcome is produced in, not merely where the check "belongs"; and run the control in the
   environment the tool EXPECTS (from the repository root the utility exits on its own path derivation
   and never reaches the guard, so the first control "passed" against code that never executed).
   ⚠ **Where to probe, ruled after one inverted finding:** a toolchain guard probes **from where the
   BUILD runs**, which is correct under both `GOTOOLCHAIN` settings — under `auto` the build switches to
   the release the module's `go` directive requests and the guard sees the switched release; under
   `local` no switch occurs and the guard sees the ambient one and refuses. The invariant is *measure
   the toolchain that mints the artifact*, and any probe site satisfying it is right regardless of what
   a neighbouring predicate does. (A predicate probing with NO module context compares ambient against
   embedded and reports STALE forever wherever they differ — a permanent spurious rebuild, which fails
   SAFE and is a cost question, not a correctness one.) ⚠ And **reconcile the FORMATS**:
   `<GoStdLibVersion>` yields `1.23.12` while `go env GOVERSION` yields `go1.23.12` — without the prefix
   reconciliation the comparison mismatches ALWAYS, a guard that refuses everything, which is exactly as
   useless as one that passes everything.
   ⚠ **A PIN ASSERTION TAKEN INSIDE A MODULE UNDER `GOTOOLCHAIN=auto` IS CWD-SENSITIVE AND ANSWERS
   FOR THE SWITCHED TOOLCHAIN** (2026-09-08): inside the converter's own module both `go version` and
   `go env GOROOT` report the SWITCHED release while the resolved `go` still lives under the pinned
   root, so a pin assertion taken there passes — or aborts — for the wrong reason; under `local` the
   converter cannot be built at all under the corpus pin. **Assert the pin from a directory with NO
   module file, or take the assertion under `local` even when the run needs `auto`; never read
   `go env GOROOT` as pin evidence from inside a module; and where a BUILD pin matters, verify the
   produced BINARY, which no cwd can switch.** ⚠ Two narrowings, neither contradicting it. **The
   requirement for `auto` is a property of the SINGLE-ROOT design, not of the pairing**: a SPLIT pin —
   one root named per `go build`, another named per conversion — needs no switch and was measured
   correct under BOTH settings. And **the conversion half DOES depend on the toolchain setting,
   through the CHILD `go` the converter shells out to for package loading**: with a fixed binary and
   only that setting varying, a probe module declaring the NEWER directive converted against the
   NEWER standard library under `auto` and was refused verbatim under `local`. A split pin is sound on
   THIS corpus only because every corpus module declares a directive BELOW the convert pin — **the
   corpus's own directives, not the naming of two roots, are what keep the loader still** — and a
   module declaring above the pin (an end-user recursive conversion, or a hopped corpus against a
   stale pin) switches SILENTLY.
   ⚠ **`GOTOOLCHAIN=auto` ONLY SWITCHES UP, so the sentence above holds only when the ambient release
   is OLDER** (measured 2026-09-07/08): with the directive `go 1.23.12` and an ambient go1.24.7, `auto`
   performs NO switch — a newer ambient SATISFIES the directive — and the built binary reports
   go1.24.7, so a guard that "probes from where the build runs" runs against the wrong GOROOT and
   reports about it. The right-spelling-of-the-wrong-release member, arriving through the door this
   paragraph calls safe. ⚠ Corollary: **a cloud lane reports its CONTAINER when it reports
   converter-suite colour** — two container-only failures (a 1.24.7 GOROOT with no `internal/weak`, and
   a SHALLOW clone refusing a seeding push) reproduce at master with the change stashed, and neither
   can be fixed by care.
   ⚠ **A CONVERTER-PIN MOVE IS NOT A CORPUS HOP** (2026-09-08): the converter module's directive moved
   to the newer release while the corpus release property did NOT — the converter's BUILD toolchain
   hopped, the corpus release did not — and "the converter pins the newer release from here" reads at
   a glance as the corpus having moved. Two consequences for the window that follows: harness legs run
   entirely under the new pin, while `-tests` rows run with the converter BUILT under the new pin and
   the pipeline RUN under the corpus's, because the ORACLE's release IS the corpus's release; and **a
   classification measured with the old-pinned converter is TREE-LOCKED to that pin** — a
   re-measurement with a newer-built front end over older sources is a DIFFERENT measurement, named
   with the pin on both sides, never a refutation. The refusal was then MEASURED both ways: a
   corpus-pinned shell exits non-zero with NO binary (the module requires the newer release under
   `local`) while the same build under the newer pin exits 0, so **a pre-move instrument is
   UNBUILDABLE from master, not merely different**, and its measurements are reproducible only from a
   pre-move checkout. The guards split BY INVOCATION: a direct `-tests` conversion meets only the
   converter's own mtime guard, while any HARNESS-driven path meets the embedded-release-versus-
   toolchain predicate and REFUSES under the corpus pin — which is why `-SkipBuild` is MANDATORY for
   the sweep in that window, its own guard REQUIRING the corpus release. ⚠ And **`-goroot` selects the
   corpus SOURCE tree but does NOT isolate the package LOADER**: the ambient root leaks into the
   loader's resolution of the internal packages, so a run whose shell still carries the BUILD pin
   fails with scores of undefined-symbol errors that read exactly like a corpus break — **the RUN's
   environment is re-exported to the corpus pin, in a separate shell or explicitly, before the
   converter is invoked.**

