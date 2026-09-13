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
<!-- PHASE 2 DISTILLATION, 2026-09-12. The dated narratives of the Phase-1 text are preserved
     below in comment blocks beside the rule each one justifies. Nothing was deleted; the
     byte-identical pre-split original remains docs/doctrine/JOURNAL-2026-09-12.md.
     BATCH19 MERGE, 2026-09-12: the 18 entries routed to this file from
     origin/claude/coord-doctrine-batch19 (commits 24bfc8304 / c5e17217b / e9e56b657, items
     1154-1321) are integrated here -- most as further dated evidence inside the comment of a rule
     this file already carried, seven as new visible rules, and one (the two-axis GOVERSION
     derivation) as an amendment that supersedes an earlier reading recorded in the same comment.
     That branch was never merged: it targeted the pre-split CLAUDE.md shape. -->
## Build context and the `$(go2csPath)` root
- **A STANDALONE (no-solution-context) build of anything under `src/tests` measures the machine-global DEPLOY ROOT, not the repo — and its errors look SEMANTIC.** Without `$(SolutionDir)`, `$(go2csPath)` falls back to `%USERPROFILE%/go2cs` / `%GOPATH%\src\go2cs`, which is STALE between deploys. A *missing* root is loud (CS0246 on `go`); a *stale* one produces plausible type-mismatch errors (CS1503, CS1929, CS0234 on a newer attribute) that read as real regressions and are COMMIT-INDEPENDENT. **Pin `-p:go2csPath=<repo>/src/` (forward slashes) on every standalone build under `src/tests`, and a bisect probe must carry the pin.** <!-- Paid 2026-08-30, cost one full invalidated bisect: a bisect probe built standalone reported
     "no green endpoint" across three anchors whose in-solution builds were all green. -->
- **Anything a child reads through a case-INSENSITIVE resolver must be injected ONCE — scrub-then-append, never append-and-hope — and "Windows is fine" proves nothing about the class.** A POSIX environment block is case-SENSITIVE, so `GO2CSPATH=/root/go2cs` and `go2csPath=/root/go2cs/src/` are two entries; MSBuild materializes environment variables as properties and resolves property NAMES case-INSENSITIVELY, so both fold into ONE `$(go2csPath)` and the winner is a per-process coin flip. The losing draw concatenates `$(go2csPath)gen/...` into `/root/go2csgen/...`, dangles the analyzer and every stdlib `ProjectReference`, and dies in a CS0246 storm on every golib type — intermittent, package-shuffling Linux `-tests` failures. <!-- Root-caused 2026-08-21, fixed at the converter 2026-08-22. The collision killed three
     measurement campaigns with every plausible suspect A/B-eliminated first. Windows environment
     blocks are case-insensitive at the OS level — the two names are ONE slot — so five weeks of
     Windows sweeps could not see it. The converter now (a) never exports its own derived
     GO2CSPATH (resolveGo2CSPathDefault, main.go) and (b) scrubs every case-variant from the
     inherited environment before appending the canonical entry (childEnvWithGo2CSPath,
     testConversion.go), so a child carries exactly one spelling whatever the invoking shell
     holds; guarded by childEnvGo2CSPath_test.go. The Linux harness pin (_paths.ps1) STAYS until
     a Linux lane re-measures without it. -->
- **A pin the CONVERTED side needs goes in the SHARED child-env base at LAUNCH, beside `GOROOT`/`PATH`, and is applied to BOTH sides of the comparison** — never in one side's env. `runtime.envs` is filled by a `[ModuleInitializer]` before `Main`, so no host code precedes the snapshot and `TestHost.Run` cannot pin `TZ` from inside the process; making the snapshot live would break Go's own set-at-process-start semantics. **A cross-SIDE divergence is worse than the cross-platform one it was meant to cure.** <!-- Measured 2026-09-02, the TZ pin. -->
- **A control harness reproduces the caller's ENVIRONMENT, not just its command.** Diff the CI step's environment against the repro's before believing a local red; a repro differing from its caller by one unstated variable is measuring its own shell. <!-- Measured 2026-09-02: BehavioralRunner invoked DIRECTLY inherited neither the CI job's
     GoTargetOS nor _paths.ps1's pin, so every L3 csproj took the windows default on a Linux host
     and the leg read "red by construction" — for a pin that already existed. -->
## Toolchain resolution
- **The pipeline's ORACLE side runs whatever bare `go` resolves on PATH — `GOROOT` alone does NOT pin it.** `go2cs.exe` shells out to `go test -json` for the compare oracle and that child inherits PATH. **Tell: `Go=""` for EVERY test** — the ORACLE side blank while the C# side reads plausible, the mirror of the file-lock signature (C# side blank) — and it reads like total conversion failure. **Prepend `$env:GOROOT\bin` to PATH in every pipeline shell and verify with bare `go version`, never just `go env GOROOT`.** <!-- Measured 2026-08-29, the net/http bank lane: this machine class carried ambient 1.23.1 vs
     pinned 1.23.12, so a shell setting only GOROOT ran the WRONG release's oracle. Third member
     of the mass-empty family. -->
- **A hand-invoked `-tests` run on a non-Windows host needs `GoTargetOS=linux` in its environment**, or it bypasses the pin, links the WINDOWS dependency set, and mints phantom CS0426s that read as Linux defects. MSBuild materializes environment variables as properties, so a lane can export it directly; routing net-family Linux work through `run-validated-sweep.ps1` stays the reliable habit because the sweep carries the pin, the toolchain and the cgo state together. <!-- Lane R, 2026-08-29 (a bare `go2cs -tests` on Linux). Corrected 2026-09-04 on the MECHANISM,
     the habit standing: the load-bearing part is the GoTargetOS PIN and the sweep is ONE way to
     supply it, not the only one — "it must go through the sweep" was the wrong reason. -->
- **The RIGHT SPELLING of the WRONG RELEASE passes every GOROOT check there is, and its quiet form answers NORMALLY.** On a box with side-by-side SDKs bare `go` can resolve 1.24.7 while the corpus pins 1.23.12: `go env GOROOT` stays self-consistent, the conversion succeeds and exits 0, and the spelling/namespace guards pass because nothing about the PATH is wrong. The loud form misroutes the namespace; the quiet form gives no empties, no errors, and a real comparison against a corpus the tree does not have. **`GOROOT="$(go env GOROOT)"` is the trap wearing a seatbelt: pin explicitly, put its `bin` FIRST on PATH, ABORT unless bare `go version` reports the pinned release, and re-measure anything banked under an ambient one.** <!-- Measured 2026-09-02 (a cloud lane; the container class). A conversion against the corpus
     prints `go version` AND GOROOT before it runs. Armed at BOOT too: a stale /etc/profile.d lane
     script exporting an older /usr/local/go/bin beat the newer fleet file (profile.d sources
     alphabetically — a zz- prefix fixes it), and `wsl.exe -- bash -lc` does not source profile.d
     like a real login: verify by bare `go version` in a real login shell. The container class is
     NOT uniform (no bare `go` on one host, 1.24.7 on another, 1.25.1 off PATH on a third) and a
     persistent USER-scope GOROOT can pin an old release on a laptop lane. Because nothing recorded
     WHICH release ran the oracle, oracleGoVersion now goes into the comparison record, captured as
     OBSERVED — a `go version` through the same call, directory and environment the `go test -json`
     child inherits, omitempty so a late probe failure cannot invalidate a comparison. -->
- **A toolchain pin that checks only `go version` DOES NOT CHECK THE PIN.** If `$GOROOT\bin` holds no `go.exe`, PATH falls THROUGH to an ambient toolchain and the version string can still match while describing a different install. **Assert additionally that the RESOLVED binary lives under the pinned root and that `go env GOROOT` equals it** — the version answers *which release*, the path answers *which install*, and only both together are a pin. <!-- 2026-09-07. -->
- **PRINTING a pin is not CHECKING it.** An instrument that prints its pin and proceeds has no guard: it ABORTS on mismatch, and the print is only evidence of what the abort compared. <!-- Measured 2026-09-02: a control script printed `go1.24.7` on its first line, from a
     `go env GOROOT` taken in an unpinned shell, and carried on; three findings descended from
     that run and were withdrawn. -->
- **Negative-control the abort against the box's OTHER toolchain — one that genuinely EXISTS and is invokable — and verify the control's substitution LANDED.** A control against a toolchain that does not exist cannot vary the axis: it silently tests the fall-through instead. The control must exit non-zero having run ZERO sweep stages before any green the wrapper reports is believed. <!-- Two controls failed that way in a row (2026-09-02): one pinned a release absent from the box
     (PATH fell through and it reported PIN OK for the RIGHT toolchain), the next was a `sed` whose
     pattern never matched, leaving a byte-identical copy of the script under test. -->
- **No lane assumes another's toolchain number, and every hand-invoked `-tests` run pins `-go2cspath <worktree>/src`** — its generated csproj otherwise falls back to the machine-global deploy root (MSB4006 loud; a plausible verdict from uncompiled bits quiet).
- **On a fleet host use the LOGIN shell and never prepend a toolchain path** — a lane's own `export PATH=<toolchain>/bin:$PATH` defeats the fleet's `zz-` profile.d pin by construction. A probe answering `command not found` is describing its own environment, not the host's.
- **Cross the WSL boundary with a heredoc (`wsl -- bash -s <<'EOF'`), never `wsl -- bash -lc '…'`:** every substitution inside the single-quoted string — verification prints, loop variables AND exit codes — is expanded by the OUTER shell, so prints come back EMPTY (`GOROOT=`, `HEAD=`) and read as answers. **An empty verification print is a broken instrument, never a pass.** <!-- Measured 2026-09-02 (the "third door"): three false "command not found" probes, one
     cut-presence line evaluated against the wrong tree, a false `(exit 0)` and three empty-path
     parse errors, all in one evening. The heredoc form is the only spelling for that boundary. -->
- **A PATH-ONLY TOOLCHAIN SWITCH IS HALF A SWITCH; the cure is to need NO toolchain.** Fetch the official archive, verify its SHA-256 against go.dev's own manifest (`?mode=json&include=all`), extract with the System32 `tar` NAMED BY PATH into a writable SDK directory, and prove it by the bare `go version` under the new root with `GOTOOLCHAIN=local` and that root as `GOROOT` for the ONE call — changing no default pin. **And capture every exit code BEFORE any pipe.** <!-- Coordinator, 2026-09-07 18:57, provisioning go1.24.13 on the i7: `go install` under a
     PATH-selected 1.23.12 `go` with the ambient GOROOT still resolving 1.23.1 dies
     `compile: version "go1.23.1" does not match go tool version "go1.23.12"` — and the failure was
     read as `install exit=0`, because the exit came through a pipe. Two catalogued traps in one
     line, met by the author of both entries. -->
- **The provisioning CAPABILITY test** (amended on a lane's measurement, 2026-09-07): (a) executes with a self-consistent `GOROOT`; (b) COMPILES a probe module, with `go version <bin>` stamping the release; (c) `go list std` at that root returns the host's census count; (d) ZERO read-only files under `src/` measured by MODE (`! -perm -u+w` — **`! -writable` answers access(2) and is VACUOUS for root**); (e) pins unchanged. **`test/typeparam` presence is a RECORDED acquisition-route property, never a gate** — present in the archive, absent from a `golang.org/dl` download and from module-cache roots; as a gate it rejected the corpus's own pinned 1.23.12 on two lanes.
- **A MODULE-CACHE TOOLCHAIN SATISFIES THE TWO-PART PIN AND IS NOT AN INSTALL, AND ONE COMMAND SETTLES WHETHER A BOX HAS THE CORPUS RELEASE:** `GOTOOLCHAIN=go<rel> go version` materialises it under `golang.org/toolchain@v0.0.1-go<rel>.<goos>-<goarch>`, checksum-verified, read-only and proxy-dependent (`go clean -modcache` removes it), and the two-part assertion holds against it — the resolved binary lives under that root and `go env GOROOT` equals it. **Admissible for GOROOT-side census and oracle-side Go builds; NOT claimed for emission; and arm (a) of the capability bar ONLY, never a provisioning install.** **A capability claimed ABSENT gets the one-command check BEFORE the claim is repeated** — a standing "cannot measure that here" turns into "can" for every container lane. <!-- 2026-09-08: "not installed on this box" was said THREE times before one command checked it. -->

## Goldens and re-baselining
- **`TargetComparisonTests` compares goldens with line endings NORMALIZED** (CRLF→LF; `TargetComparisonTests.FileMatch` / `BehavioralRunner.FilesEqual` both strip CRs). Content diffs are still caught exactly; a pure line-ending difference is ignored. Re-baseline an *intended* output change with the **`UpdateTestTargets`** project and **`--createTargetFiles`**, or `run-behavioral.ps1 --update-targets` — **never by hand-editing a golden.** <!-- Raw byte-for-byte compare until 2026-07-07. A line-ending difference can only come from
     autocrlf, never from the deterministic converter. -->
- **Both re-baseline paths RE-TRANSPILE each project unconditionally immediately before the copy, and REFUSE — exit non-zero, by name — when that transpile fails, times out, or exits 0 having converted best-effort.** There is deliberately **no** up-to-date predicate on either path: `UpToDate` answers on mtimes, and a `.cs`-only restore, a `Copy-Item` or an editor save all leave a `.cs` newer than both its `.go` and `go2cs.exe` while its CONTENT came from some other converter — after which the copy makes `.cs`, `.cs.target` and `UpToDate` agree by construction and no later run can see it. **A stale COMPARISON is recoverable; a stale RECORD is not.** `--only <Name>[,<Name>…]` narrows one invocation's transpile-and-copy (the four `<TestMethods>` blocks stay a function of the whole project set). <!-- Landed 2026-09-04. Until then neither path re-transpiled: the utility ran the converter *not
     at all* and this file carried the prerequisite as a sentence ("re-transpile first … or the copy
     silently re-baselines stale output"), while the runner ran it only when its mtime predicate said
     the project was out of date. Both are the same hole and it is FALSE-GREEN ROUTE #2 turned on the
     goldens. CNR has never had an up-to-date predicate for the same reason. --only exists so the
     refusal branch is not a ~25-minute control nobody runs. The demonstration that such a path is
     broken is the runner printing `ok` having invoked the converter ZERO times and minting a
     poisoned .cs into the golden at exit 0. -->
- **A byte-identical CNR verdict is the PRECONDITION of a whole-corpus re-baseline and runs FIRST, never after** — the copy banks any `.cs`-vs-committed drift into the goldens SILENTLY. **The refusal property asserted is the golden UNCHANGED ON DISK** (a refusal that has already copied is a report, not a refusal), and a corpus-wide control states its BASELINE. <!-- Three rules that fell out of landing it, 2026-09-04. The baseline stated then: 718 golden
     pairs byte-identical at the head is what makes a whole-corpus copy a no-op and ONE moved golden
     a measurement. (Counts drift — measure, don't quote.) -->
- **A golden that byte-matches a DEGRADED emission is a phase actively VOUCHING for the hole**, not one merely proving nothing. A harness that cannot MEASURE a transpile skips its Target compare exactly as it does for Fail and Timeout — **a golden compare is a statement about a measured emission or it is nothing.** <!-- 2026-09-04: at master the Target phase PASSED over a best-effort main.cs. -->
- **A RED control leaves the DEFECTIVE emission on disk beside the FIXED golden.** After any control that re-transpiles: **restore the source, purge `bin`/`obj`, re-transpile, and require the `.cs` CR-strip-identical to its golden before staging** — and re-read the worktree's state after any interruption, since a resumed agent inherits whatever the control left behind. <!-- 2026-09-05: reverting a fix and re-transpiling the guard to reproduce the defect rewrites its
     .cs while the .cs.target still holds the FIXED form, so a commit taken there banks the wrong
     emission next to the right golden and only a later Target phase would catch it. -->
- **A GOLDEN PREDICTION IS SCORED ON THE POPULATION IT NAMES, NEVER ON THE FILE'S BARE COUNT.** A prediction of three case-label `==` and zero `is` HELD at 3/0 on the named population while a bare `grep` over the same file reads 5 — two of them ordinary source expressions (a result comparison, a `Println`) that the prediction never claimed. **Score the predicate that was worded, or the prediction mis-scores in whichever direction the extra matches fall.** <!-- 2026-09-08, batch19. -->
## Line endings and `.gitattributes`
- **The CRLF working-tree form is PINNED by `.gitattributes`, not inherited from `core.autocrlf`.** A `text eol=crlf` block covers every converter-emitted artifact type — `*.cs`, `*.cs.auto`, `*.cs.target`, `*.csproj`, `*.slnx`, `*.props`, `*.targets`, `src/core/**/README.md` — ordered ABOVE the `-text` blocks so those keep their verbatim-bytes exemption (last matching pattern wins). **Do not "fix" a `.cs` to LF to match a Linux habit; the pin will put it back**, and a whole-tree renormalization is not owed. <!-- 2026-08-08, r46c. Rationale: the converter emits CRLF *unconditionally*, so the checkout was
     the only variable, and a clone with autocrlf=false (git's default on Linux/macOS) materialized
     LF and made check-no-regression report the entire corpus as drifted before any work started.
     Nothing about the Windows lane changed — eol=crlf reproduces exactly what autocrlf=true was
     already doing, verified by `git add --renormalize .` over all 9,380 tracked files staging ZERO
     corpus files (every non-LF blob in the index was already -text). -->
- **`-text` is a RUNTIME concern, not a golden-compare one.** The converter emits CRLF for C# line endings but preserves the Go source's LF inside multi-line string literals, so those `.cs`/`.cs.target` are mixed. (1) The byte compare is line-ending-insensitive, so **no `-text` mark is needed just for it.** (2) If a project's *compiled program* embeds and OBSERVES a multi-line string literal (`Solitaire`'s board, printed via `println`), a smudge bakes the wrong `\r` runes into the value on any build that compiles the committed `.cs` *without* re-transpiling (VS, CI `dotnet build`, an up-to-date skip) and the program misbehaves — Solitaire's board geometry breaks and the solver hangs. **`Solitaire`/`SortArrayType`/`StdLibInternalAbi` keep their `.cs` `-text` marks**; a NEW multi-line-string test needs one only if its program's behavior/output depends on the literal's exact bytes. The phantom is platform-independent now, but unchanged in shape.
- **A LINE-ENDING GATE THAT IGNORES THE FILE'S OWN ATTRIBUTE REFUSES A FILE THAT IS RIGHT BY DESIGN — name the LAYER and the ATTRIBUTE before calling a line ending wrong.** `docs/` is OUTSIDE the `eol=crlf` pin, so an ordinary `docs/*.md` reads `i/lf w/crlf` only through `autocrlf` and a `-text` file there reads `i/lf w/lf` legitimately. **Measure the LAYER — `git ls-files --eol` per file at the assembled head — with a planted non-exempt LF control REFUSED**, and key any accepting path on the attribute rather than on a blanket CRLF assertion. <!-- 2026-09-08, batch19: a train leg asserted every docs/*.md in its delta CRLF "under the
     eol=crlf pin"; the pin's patterns are *.cs, *.cs.auto, *.cs.target, *.csproj, *.slnx, *.props,
     *.targets and src/core/**/README.md and cover no docs/ file at all. Eight ordinary files read
     i/lf w/crlf through autocrlf, and one seated probe README carries -text (verbatim bytes,
     i/lf w/lf) — the same -text phantom recorded just above for testdata, in a docs costume. It rode
     green from the previous train only because every docs seat there happened to be CRLF. Settled by
     measuring the layer per file at the assembled head with a planted non-exempt LF control refused,
     and accepted through a named land-script path keyed on the attribute. -->

## MSTest, testhost and GolibTests
- **`MSB3027` "file locked by testhost" is not a compile error.** A stray `testhost`/`vstest.console` from a prior run locks `BehavioralTests.dll`; kill it, and `dotnet build-server shutdown` frees bin/obj locks. **Prefer `src/tests/Behavioral/run-behavioral-tests.ps1`** — it clears stale hosts *before* the build, where the lock manifests, and runs with `--blame-hang` — over a bare `dotnet test`. <!-- Root cause + mitigation 2026-06-30: MSTest's Exec() used an unbounded WaitForExit(), so a hung
     child (a deadlocked transpiled program, or a build blocked on a lock) hung the suite forever and
     orphaned testhost. Exec now has a per-call timeout (180s build/transpile, 30s run) that kills the
     whole child PROCESS TREE, and disables MSBuild node reuse (MSBUILDDISABLENODEREUSE=1) so in-test
     builds don't leave lock-holding worker nodes; AssemblySetup.[AssemblyCleanup] runs
     `dotnet build-server shutdown` only for a bare `dotnet test` — a run-behavioral-tests.ps1 run sets
     an env-var contract that suppresses it, since the script's default path isolates its own children
     instead (chip 6fe128108, 2026-08-08). -->
- **An MSTest verdict WORD is not a verdict — an ABORTED run prints one anyway.** The second-to-last line can read `Passed! - Failed: 0, Passed: 82` with the LAST reading `Test Run Aborted.`; the exit code is honestly 1, but a verdict-word grep reads green and `$?` after a pipe is the LAST command's status. **A GolibTests gate greps for `Test Run Aborted` AND compares the run's Total against the DECLARED count, capturing the raw exit BEFORE any pipe. An abort is an UNMEASURED suite, never a pass.** **And a `--filter` matching NOTHING exits 0 printing `No test matches`, so an rc-only gate reports 10 of 10 from a run that executed ZERO tests: a filtered MSTest gate asserts the EXECUTED count against the expected count and greps for `No test matches`, never rc alone.** Run it `--no-build` behind the solution leg. <!-- Measured 2026-09-02, GolibTests on a Linux lane, against a declared count near 470; the tell was
     adding 7 tests and watching the total stay 82. A `dotnet test` that BUILDS raced twice in one
     night on a spurious CS0234/CS0246 that was gone on --no-build against the build just completed.
     The zero-match --filter member measured 2026-09-08 on the i9 via C1 (batch19): the vacuous-green
     class arriving through the tool everyone trusts to be loud. -->
- **The DECLARED count is derived from the COMPILE SET, not from a raw `grep -c '\[TestMethod\]'`.** `GolibTests.csproj` `Compile Remove`s files by `$(GoTargetOS)`, so a run reporting 474 against 479 grep-counted methods is COUNT-MATCHED, not an abort. Subtract the methods in `Remove`d files whose condition holds before reading a shortfall as a truncated suite. **DERIVE IT WITH MSBuild `-getItem:Compile`, AND STATE A COUNT WITH ITS DERIVATION** — a hand subtraction can reach the right number BY LUCK and read as confirmation. <!-- Measured 2026-09-02. The by-luck member measured 2026-09-08 by lane G (batch19): 752 was
     reached by subtracting the FIRST FOUR files of an EIGHT-file `Compile Remove` group — arithmetic
     that is wrong twice and lands on the right total — the inverse of a sibling lane's 735-vs-739
     slip. `-getItem:Compile` is the sound source (129 items on the linux flavour there). Companion
     from the same run: three `FixtureLinkStagingTests` failures were the symbolic-link privilege, a
     HOST capability shared by G-LAPTOP and the i9 box, named from the error text rather than routed
     as a defect. -->
- **Derive the ADMISSIBLE totals from the csproj arithmetic — no build at all — before spending a suite on one sample.** Computing the compile set from the conditional `ItemGroup`s against the declared-method count yields the exhaustive set of legitimate totals per flavour, and a reported total matching NONE of them is a stronger statement than any single re-run can make. Hand the lane the target so its re-run has something to be checked against. **A GROUP WHOSE CONDITION REQUIRES A NON-EMPTY `GoTargetOS` REMOVES NOTHING ON A HOST WHERE IT IS UNSET, AND NO COUNT CARRIES ACROSS FLAVOURS OR HOSTS** — derive per flavour before the run, and positive-control the instrument by having it re-derive ANOTHER flavour's known total. <!-- At that tree (2026-09-06): 696 unset, 696 windows, 727 linux, 692 darwin. A PER-FILE method
     count is a property of the blob it was taken at, exactly as the total is — carrying per-file
     counts from one seat's tree to master came within one command of accusing a lane whose
     arithmetic was right (one test file had grown from 9 methods to 14 between the trees).
     Re-derived 2026-09-08 (batch19) at a grown tree, three readings of the same rule:
     (a) 782 methods declared; group 1 (`!= 'linux'`) removes 41 in 8 files; group 2 (`!= ''`)
     removes NOTHING on an unset host — so 741 is the ONLY legitimate total there, which is what
     turns "741 == 741" from a coincidence into a statement, and is why a group-2 test class compiled
     and RAN on that host at all.
     (b) the i9 windows flavour read 752.
     (c) the LINUX flavour is not the windows 752 at all — the `!= 'linux'` group that removes eight
     files on windows KEEPS them on linux while the `!= '' and != 'windows'` group removes two — so
     GolibTests read 784/0/1 = 785, exactly what `-getItem:Compile` (129 items) had derived, with the
     same instrument's windows arm re-deriving the i9's 752 as the positive control. A reused 752
     would have read a correct run as 33 short.
     Three companions from that first-contact run: a skip whose name was observable only in an
     interim read (that host's temp directory did not survive between invocations) was re-derived
     from the TRX artifact rather than quoted; a first clone 645 commits stale — the Windows repo's
     LOCAL master, not origin/master — was caught before any reading; and the one deviation from the
     install scoping (two profile export lines, user state, required by the login-shell bar) was
     NAMED by the installer rather than found by the reader. -->
- **A guard placed in a file that is `Compile Remove`d on the target it must exercise is VACUOUS AND READS GREEN.** **Read the csproj's condition groups before choosing a guard's home.** As of 2026-09-07 the groups are `!= 'linux'` (8 files) and `!= '' and != 'windows'` (1 file), and **no group matches darwin at all** — a linux-OR-darwin arm needs a NEW group that removes on windows/unset, never an existing one. **"CONTRACT test" and "test that exercises a platform-gated implementation" are different shapes wearing one word**, the first not precedent for the second: ask what a test TOUCHES, not what it is named, before citing a neighbour as permission. <!-- GolibTests.csproj removes LinuxSpawnSeamTests.cs under '$(GoTargetOS)' != 'linux', so a darwin
     arm placed there compiles only on linux (where the darwin hand-own does not exist) and is absent
     on darwin (where it does). DarwinSigmaskContractTests.cs sits inside the linux-only group
     legitimately because it asserts a SHAPE and needs no darwin host. -->
- **A per-GOOS hand-own's `Go`-prefixed test helpers exist only under that flavour, so the GolibTests classes referencing them are linux-only FILES by construction** and belong in the `Compile Remove` set for every other `$(GoTargetOS)`. The WINDOWS-flavour solution build at the union is the gate that sees it. A lane that cannot build the other flavour's solution STATES that gap in its seat and adds the `Remove` in the same cut. **A chain is STOPPED at a red compile gate**, never run on to legs that cannot mean anything on a broken union. <!-- 2026-09-05: 22 CS0103 on five helper names. -->
- **A self-consistent check needs at least one input anchored to a NAMED REF.** The Total-against-declared check PASSED on a run 42 methods short — the exact truncated-suite signature it exists to detect — because the declared count and the total were both taken from a checkout 42 commits behind. Not skipped, not fabricated, not scoped-empty: RUN, and defeated by its own inputs agreeing with each other instead of with reality. <!-- Measured 2026-09-06; the vacuous-green class one level deeper. -->
- **Every gate line naming GolibTests STATES ITS CONFIGURATION.** Exactly three tests RUN at Release and SKIP at Debug — the GC/pin-liveness class self-skipping where a non-optimizing frame would root its temporaries and make the assertion unfalsifiable — so a Debug-only reading is NOT equivalent to Release+TC0 even when both read zero failures. **A skip delta of exactly 3 between Release and Debug is a usable self-consistency check** that both legs of a two-configuration cut ran what they should. **CONFIGURATION INCLUDES THE FLAVOUR PIN: GolibTests on a Linux host without `-p:GoTargetOS=linux` IS RED BY CONSTRUCTION** — every failure block a `DllNotFoundException` on kernel32 out of the converted `syscall_package` static initializer — so **a gate routed to lanes on two OS families carries the flavour pin IN ITS INSTRUCTION.** **And a skip delta that MOVES names tests that never executed:** environment-gated `Assert.Inconclusive` controls read COUNT-MATCHED while running nothing, so "green" for such a branch is clean-because-untested — **the gate for an env-gated control set is the leg with the variable SET, posted with the controls' own lines.** <!-- Measured 2026-09-06: two greens, different coverage, identical-looking — the vacuous-green
     family in its subtlest form, not a check that cannot fail but a configuration in which three
     checks quietly do not run.
     Flavour-pin member measured 2026-09-08 by C2 (batch19): 48 failed / 744 at BOTH configurations
     on Linux, pinned — after a depth-UNLIMITED purge of 2,021 output directories — at 0 failed / 781
     both, count-reconciled as 788 [TestMethod] − 4 in a netapi32 file − 3 filtered = 781. The
     csproj's own 2026-08-31 comment had already ruled it.
     Env-gated member measured 2026-09-08 on the i9: 747 count-matched on four legs with eight census
     controls SKIPPED — skip delta +8 in BOTH configurations, exactly the new file. Companion from
     that run: two branch Debug runs against one base run establish "not a deterministic regression",
     never a RATE. -->
- **MSTest RUNS SERIALLY WITHIN AN ASSEMBLY UNLESS TOLD OTHERWISE, so process-global state IS assertable in GolibTests** — no `[Parallelize]` attribute, no `.runsettings`, and a sibling test class already mutates process-global ENVIRONMENT state on exactly that basis. **Check the attribute and the settings file before declaring a global unguardable.** Treat "X cannot be done here" as a claim owing evidence exactly like "X works", and **state what you MEASURED and what you ASSUMED as two different sentences** — a declared limit is never re-derived, because the next reader does not go back over a limit somebody has already declared. <!-- A lane announced that a working-directory guard "cannot assert without racing every sibling" —
     first clause true, second an assumption never checked, refuted by ONE COMMAND (2026-09-07) — and
     the sentence was the one its author was PLEASED with, for being honest about a gap. That is the
     trap: candour about a limit reads as rigour and is accepted without check. A wrong claim in an
     announcement is worse than wrong code: wrong code meets an oracle and is refuted. -->
- **A green taken under known contamination still holds; only a RED needs the clean re-run.** A concurrent partial build racing a suite can tear an assembly and make a passing project fail; it cannot make a failing project pass. State the asymmetry rather than discarding the run.
- **A liveness census is by executable PATH (`Win32_Process.ExecutablePath -like '*<worktree>*'`), never by name** — the rename that makes a runner unmatchable by a sibling's `Get-Process <name> | Stop-Process` also makes it invisible to a by-name census, and the two defences collide silently. **BUT A PATH CENSUS IS BLIND TO A `go test`** — it builds `<pkg>.test.exe` into a TEMP build directory and runs it with the package directory as cwd, so neither `ExecutablePath` nor `CommandLine` carries the worktree string, and a census reading ZERO `go.exe` under that worktree mid-suite says NOTHING. **Liveness of a `go test` is the harness task's own completion notification, or the TEST BINARY by name.** <!-- Measured 2026-09-06: `Get-Process BehavioralRunner` read 0 while coordT31Runner.exe PID 35520
     had been running 48 minutes, and a second runner was launched into the same worktree on that
     reading. The name is exactly the field the kill-scoping defence changes.
     NARROWED 2026-09-08 by the coordinator (batch19): the by-path rule above is right for a runner
     and VACUOUS for a `go test`, whose binary lives outside the worktree by construction. Newer
     entry wins on reach; the by-path rule stands for everything it can actually see. -->
- **A log frozen mid-line is not evidence of death when the phase it stopped in is silent and long** — `[Compile] Go (per project)... ` with no newline prints nothing until it completes. **The PROCESS answers whether a run is alive**, and a live `go.exe` under the runner is positive evidence of progress no log tail can supply. And **an operation refused for a reason you did not predict is data about the world, not an obstacle to route around**: "Device or resource busy" on a copy onto a running apphost name is a LIVENESS signal. **A ZERO-LINE log at a FRESH mtime is the run's buffering, not its death.** <!-- Two readings from the same night, 2026-09-06; the zero-line/fresh-mtime member 2026-09-08
     (batch19), read off the same `go test` census that the by-path rule above could not see. -->
## BehavioralRunner, CNR and solution integrity
- **`src/tests/Behavioral/BehavioralRunner` is the faster alternative to MSTest** — a dependency-free console app running the same four phases over every behavioral project, not hosted in testhost, so the self-lock failure mode is structurally absent. It collapses the per-project `dotnet build` calls into one parallel MSBuild invocation (pre-building the ~31 shared `golib`/analyzer/`core/*` deps sequentially first to avoid the parallel-build MSB3026/27 race, then fanning out), and is at parity with MSTest. Drive it via **`run-behavioral.ps1 [--filter X] [--phase transpile,compile,target,output] [--update-targets] [--list]`**. Only `[GoTestMatchingConsoleOutput]` projects are `go build`- and stdout-compared. <!-- Added 2026-06-30. -->
- **For a pure converter no-regression check with no compile/run at all, use `check-no-regression.ps1`** — it re-transpiles every behavioral dir and `git status`es the converter-emitted **`.cs` AND `.csproj`** (the transpile rewrites both; the `.cs`-only pathspec it had until 2026-08-08 made a csproj-emission change invisible on every platform). Converter stderr is captured, not discarded: a package the run could not fully regenerate — best-effort "did not fully type-check", a recovered "visit file error", or a non-zero exit — **fails the gate BY NAME as `NOT MEASURED` even with a clean `git status`**, so the byte-identical verdict is never vacuous; other WARNINGs are advisory, never fatal. <!-- Coordinator ruling 2026-08-08, from lane r48b's Linux FindFirstFileData finding — see
     docs/PLAN-linux-operation.md. Until F8 platform-gates the enumeration, a Linux CNR run reports
     FindFirstFileData as NOT MEASURED by design. -->
- **A converter that EXITS 0 on a DEGRADED emission makes "the exit code" a false-green predicate in every harness that asks it** — which is why the classification of a converter stderr line lives in ONE linked predicate the harnesses share rather than being re-derived per instrument (the same shape as `ConverterBuildInputs` for the staleness set, route #5). **Two harness statuses whose REMEDIES are opposite are separate members**: a budget that expired wants MORE BUDGET, a best-effort conversion wants a HOST THAT CAN TYPE-CHECK — a remediation hint names BOTH or neither. <!-- 2026-09-04. -->
- **A converter EMISSION change is measured by CNR BEFORE it is seated — a filtered behavioral run cannot see a project it does not build.** A lane that falsifies its own seated commit posts the HOLD before finishing the measurement. <!-- Measured 2026-09-02: a seated commit's PREMISE was false (golib has Func-shaped defer overloads
     at arities 1–16; only arity 0 lacks one), so its rung rewrote corpus emission for a reason that
     does not exist and carried no footprint; the drift surfaced only when CNR ran for the NEXT commit
     and reported DeferTypelessReturns drifting on the rung alone. -->
- **The emitted corpus's project-reference graph must be ACYCLIC, and `check-solution-integrity.ps1` — CNR's preflight — asserts it on every run.** It DFSes the `src/core` `.csproj` graph once per `$(GoTargetOS)` (windows, linux, darwin: the per-GOOS `<ItemGroup>` blocks make each target a *different* graph), requires 0 cycles, and names every cycle it finds. A C# project reference is a **compile-time** edge, so a cycle is MSB4006 and every project on the path stops building. Go's own imports are acyclic, so the only thing that can create one is a reference the converter introduces — a `//go:linkname` forwarding property, which points wherever the directive names, in **either** direction. **Invariant: a `-tests` conversion's production emission may differ from `-stdlib`'s only in ways that do not change the project GRAPH.** Positive control (a parameter, so it needs no tracked-file edit): `./check-solution-integrity.ps1 -TargetOS windows -InjectReference 'runtime=internal/syscall/windows'` must print exactly the six W1 cycles and exit 1. <!-- 2026-08-30. W1 is docs/phase4/DESIGN-linkname-push-cycles.md: a -tests conversion of `runtime`
     emitted runtime → internal/syscall/windows, and since Go's own imports contain
     internal/syscall/windows → syscall → runtime, no conversion order can undo it — the emitted edge
     itself has to go. The invariant is narrower than "-tests must not rewrite the production
     emission" (which the four standing closure families contradict) and sharper than "the push must
     not add a reference". -->
- **A RELEASE HOP CAN SPLIT A PACKAGE, AND THE SPLIT'S SEVERITY IS NOT THE COMPILE ERROR — IT IS THE THROWING STUB THAT COMPILES CLEAN AND REDDENS NO GATE.** A hand-own left implementing partials nothing declares is LOUD (**CS0759**, an implementing declaration with no defining one); the arriving package's bodyless partials, filled with THROWING stubs, are SILENT — the package builds, every gate passes, and the primitive throws the first time it is called. **Enumerate a split on BOTH sides — what LEAVES and what ARRIVES — and treat a fact on the record as NOT on the work list until a seat carries it.** <!-- 2026-09-08, batch19, the Go 1.24 hop read at the sources: `sync.Mutex` came to wrap a NEW
     `internal/sync.Mutex` and the four linkname stubs MOVED there, so the existing
     `sync/runtime_impl.cs` implements partials nothing declares — and `internal/sync` cannot
     reference `sync`, which is the W1 cycle class above. A row that read as a "re-write" was a
     package split with a cycle constraint. Severity measured in the emission: every
     `sync.Mutex.Lock()` at that release throws while the package compiles clean.
     The ARRIVE side, missed the same day by the same lane: a morning table named
     `runtime_SemacquireWaitGroup` as a 1.24 ARRIVAL in `sync`, and the lane's own scope restatement
     — written AFTER correcting six declarations to eight — enumerated only the four DROPS and missed
     it. Measured in the emission: the declaration is already filled by a throwing stub and REACHED
     on the path where `Wait()` must block, so every blocking `WaitGroup.Wait()` at the new release
     throws while the package compiles clean. The arrival also needs a golib `WaitReason` member new
     at that release, whose absence a first enum-shaped grep UNDER-counted (12 against the
     string-mapping switch's 14). -->
- **A DISPLACED PRIMITIVE'S HOME IS DECIDED BY THE REFERENCE GRAPH, NOT BY PREFERENCE** — when a shim is declared in packages that cannot reference each other, the only legal home is the assembly below ALL of them (golib), registered from `runtime`'s module initializer like its sibling. **And two provably-disjoint tables become ONE BUG the day the disjointness stops holding, with nothing in the tree to say so: HOIST rather than duplicate, even when duplication MEASURES safe.** **Census the CALLERS before sizing the increment** — displacing callers can leave functions UNREACHED rather than unimplemented and turn three severs into two. <!-- 2026-09-08, batch19: the fatal shims are declared in THREE packages (`runtime`, `sync`, and at
     the new release `internal/sync`), golib is the only assembly below all three, and
     `internal/sync` referencing `sync` is the W1 cycle — so the report type sits in golib. Duplicating
     the semaphore table was MEASURED safe (acquire and release both declared in the new package; the
     word lives in its own `Mutex`) and still not cut that way. Caller census: `fatalthrow` has exactly
     two callers (`throw`, `fatal`) and `fatalpanic` one (`gopanic`, dead at its own caller-PC
     intrinsic), so displacing the TWO callers leaves four functions UNREACHED — two displacements
     instead of three severs — and `runtime`'s two linkname PUSHERS then land on the same primitive as
     the hand-own, one rule rather than two. Two mechanism notes: a hard-coded anchor that a second
     entry point would have silently fallen past became a PARAMETER, and the fatal path drops frames up
     to the LAST primitive frame BY TYPE so a delegate stub between is harmless. -->
## Platform- and arch-exclusive behavioral projects (F8)
- **A behavioral guard written against ONE platform's syscall API cannot type-check on the other and turns THAT host's CNR red by name** — a lane's own-platform CNR green says nothing about the other host's gate; the union battery is where it surfaces. **F8 is the remedy:** a converter-preserved `[GoPlatformExclusive("<goos>")]` marker in `package_info.cs` naming the native platform(s), plus a LOUD skip-by-name BEFORE transpile in every enumerator (CNR, `BehavioralRunner`, MSTest as `Inconclusive`). **Commit markers before any CNR `-Revert`, which destroys uncommitted ones.** <!-- F8 landed with train 11 (2026-09-02); its gating set was DERIVED from the other platform's NOT
     MEASURED list (six windows-native, ScmRightsSeam linux) and positive-controlled both ways. -->
- **A best-effort conversion on a NON-native host REWRITES the package's csproj and `package_info.cs`** (the stdlib `ProjectReference`s and import aliases drop when the type-check that supplies them fails), so a Windows CNR **POISONS** a Linux-only behavioral package and every later leg measures the poisoned file — 5 CS0246/CS0234 reading as a missing-reference regression. **A chain RESTORES behavioral dirt (`git checkout HEAD -- src/tests/Behavioral`) between CNR and any build leg, and F8's skip must precede the converter.** Such a guard also carries a `runtime.GOOS` early-out as `main`'s first statement (raw `syscall.Socket` panics on Windows without the WSAStartup `net` performs), goldens stay WINDOWS-generated, and **a Linux CNR's DRIFT column is noisy by construction — the NOT MEASURED column is the honest one there.**
- **F8 has TWO halves — the harnesses' skip AND the `go2cs.slnx` UNREGISTRATION — and `check-solution-integrity.ps1` is the one gate that sees the second: run it.** The `.slnx` exemption criterion is **platform-exclusive AND not-windows-native**: the solution has one Windows flavour, so a `linux`/`darwin` marker unregisters the project and a `windows` marker changes registration not at all. **A platform-exclusive guard's golden (`.cs.target`) and its four MSTest entries are verified ONLY on a native-host leg** — on the other host F8 skips every phase, Target included, and CNR is transpile-only. The marker also covers a package that type-checks everywhere but FAULTS at run time (`LocalTimeZone`'s kernel32 call): an Output-phase exclusive. <!-- Measured at F8's landing, 2026-09-02; the second-half criterion stated the same day, after a
     guard's own analogy check caught it in seconds. ScmRightsSeam landed with neither golden nor
     MSTest entries verified and nothing could see it. -->
- **The OTHER cross-platform class is ACCEPTED, not gated** — a package that runs meaningfully on both platforms whose emission differs only by the `Δ`-alias flavour. **The remedy is decided by whether the package's NATIVE platform matches the host that captured its golden — never by how the drift looks in a diff.** Name the class beside the package, with a standing Linux-CNR derivation (CHANGED files whose whole diff is the alias hunk or the adapter-name hunk) so members surface by census rather than one at a time, and **re-derive a class claim at the TIP before quoting it.** <!-- Its two members were remediated by OPPOSITE mechanisms: EnvironBlockWalk is WINDOWS-native with
     a golden captured on Windows and read on Linux, so it takes [GoPlatformExclusive("windows")];
     SendtoSeam is LINUX-native with a golden captured on Windows, so it was REGENERATED on its own
     platform and marked linux (e731145b7c, train 12). The class is EMPTY at master as of C2's marker
     seat (landing with train 14) and the COUNT is what retires there — the next platform-varying
     guard brings the next member. MECHANISM, which stays doctrine whatever the count: a generated
     ADAPTER TYPE NAME in production .cs follows the imported alias — SockaddrInet4жΔSockaddr on
     Windows, жSockaddr on Linux — measured against a master control with identical numstat on both
     trees. A follow-up census naming a third member was a glyph SUBSTRING over-match, ΔHandle inside
     ΔHandler, its transpile byte-identical. A Linux CNR's honest verdict WAS "clean modulo the
     windows-alias class" until C2's marker seat landed with train 14; since then it is "clean" with
     no modifier (measured at 038c87786e: 688 byte-identical, 8 platform-exclusives skipped by name,
     0 NOT MEASURED). -->
- **CNR is the instrument of record; a corroborating hand sweep corroborates and nothing more.** A hand sweep without the harness's platform skip-list transpiles a platform-exclusive project on the WRONG HOST and reads its golden as drift (the alias added to a generated adapter name is that wrong-platform artifact, not a second mangling site), **a golden-keyed enumeration MISSES every nested sub-library**, and it skips the project-graph and integrity preflights. <!-- 2026-09-08: 537 projects against CNR's 728. Retracted by its own author BY NAME before it
     reached anyone's census. -->
- **F8's class one axis over is ARCH, and it is decided by the ORACLE.** The arch a census transpiles for is the RUNNER's host arch — no harness passes `-platforms` and the converter defaults to `runtime.GOOS/GOARCH` — so two census legs on one corpus can differ at TRANSPILE while both compile the committed corpus identically. A behavioral project whose own **Go source does not BUILD on an arch is arch-exclusive by the oracle** (`go run` cannot build it there), so no layout dimension and no emission change can make it measurable without hardware to capture goldens; skip-by-name is the remedy a fleet can implement AND verify. **The acceptance number of a skip-by-name cut is N−1 measurable with the skip NAMED, never N** — a dispatch quoting the old N would read the fix as a failure. <!-- 2026-09-04. -->
- **What F8 is NOT for: a flavour with no RUN layer.** A guard for such a flavour is neither a `[GoPlatformExclusive]` row skipped fleet-wide (a guard that cannot go red ANYWHERE — a coverage claim the fleet cannot honour) nor a text grep over the companion (route #8). It is an **ARM of the ACCEPTANCE PROBE** that runs on that flavour's CI legs, asserting the PROPERTY with a neuter and a SHA-identical restore. And **a hand-own companion's MISSING `using` is invisible to the registration ledger and to the emission diff alike — only that flavour's own BUILD sees it, which is why per-flavour builds are battery legs.** <!-- 2026-09-05; the property asserted there was Wait4(-1) → ECHILD after a failed Foreground start
     with a non-tty Ctty forcing ENOTTY, and the companion was 548 lines. -->
## Solutions and the iteration loop
- **Run the behavioral suite via the SOLUTION, not the project: `dotnet test src/go2cs.slnx`.** `dotnet test` on `BehavioralTests.csproj` directly breaks because `$(go2csPath)` (→ `$(SolutionDir)`) has no solution context and the `core\golib` ref fails to resolve. `src/go2cs-stdlib.slnx` is auto-generated by the converter's `-stdlib` run (`solutionGenerator.go`) with solution folders mirroring the Go package namespaces; since the trees unified, its project paths match the repository's, so **adopt a fresh one by copying it from the output root VERBATIM** (no rewriting; verified byte-identical). The old hand-maintained classic `.sln` is retired.
- **VS prompts to save `go2cs.slnx`/`go2cs-stdlib.slnx` on EVERY open — expected, harmless, and unfixable at the file level. Do NOT re-diagnose it as file drift, a generator formatting defect, or a reason to restructure the Symbols import.** Any `.slnx` containing a project that imports a `.projitems` shared-items file (`golib` and `go2cs-gen` both import `core/go2cs/go2cs.projitems`) is marked dirty by VS's shared-project bookkeeping; every save, Save-As included, writes byte-identical content. Accept or dismiss the prompt — nothing changes on disk either way. <!-- Bisected 2026-08-06, ten probe solutions. Classic .sln serialized it (SharedMSBuildProjectFiles
     section); .slnx has no element for it, so the model always differs from the parsed file.
     Hash-verified: SolutionPersistence 1.0.52 round-trips both solutions exactly, and that is the
     version VS ships. Upstream: https://github.com/microsoft/vs-solutionpersistence/issues/156 — if
     the format gains a shared-items element, or VS stops dirtying non-serializable state, this
     caveat retires. -->
- **When iterating on regression work, use FILTERED + `--no-build` — don't run the full suite each time.** From `src/tests/Behavioral/BehavioralTests`: `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` reuses the existing test assembly and runs just that project's 4 phases in seconds; `--no-build` is valid as long as the `*Tests.cs` files haven't changed (`git status` them). The full `dotnet test go2cs.slnx` rebuilds every registered project first and can take 10+ min or hang under Visual Studio lock contention — **reserve a single full-suite run for final confirmation.** Faster still for a pure no-regression check: re-transpile and `git status` the `.cs` + `.csproj` — byte-identical generated code ⟹ identical compile+output ⟹ identical results.

## Measured budgets
**Budget every command against its MEASURED baseline, from the TOP of its range, and re-measure the table when the corpus grows** — a stale baseline is what makes a healthy run look hung, and vice versa. The spreads below are real run-to-run variance (machine load) on one corpus. A converter rebuild invalidates every project's up-to-date check, so the *next* full run after one always pays full price. **Materially PAST these numbers means the test host has hung under lock contention, not real work** — stop and clear it rather than waiting 10–20 min.
- **Rows are DESKTOP (i9-13900K) history; the current coordinator class is an i7-5820K and runs them at roughly 3–4x those numbers.** Budget from the i7 figures, or 3–4x a row's i9 ceiling when unmeasured. **Do NOT re-baseline a row from a laptop run** (a 15–28W mobile part is the machine, not corpus growth).
- **`go test`'s own DEFAULT `-timeout` is 10m and a loaded converter suite now reaches it: pass `go test -timeout 30m` on any box carrying concurrent work, and read a FAIL at ~600s as the WALL, not the code.**
- **Treat HARD-CODED harness watchdogs as suspects on the slow class** — `PerformanceRunner`'s AOT-publish cap and `BehavioralRunner`'s build-all cap both fired on healthy runs here and faked failures. A timeout is a safety net against a hung child, never a performance assumption.
- **Native-AOT perf publishes are the extreme case (~25 min each post-unification), so a full perf run is HOURS and must run SOLO.**
- **An EXTRAPOLATION written in a MEASUREMENT's voice is a false measurement.** Mark a derived figure PROVISIONAL, state the measured points separately, and let the first real run replace it. **Every row below is a measurement or it does not belong in it.** <!-- Table re-measured 2026-08-04 by r40 at 569 transpiled packages / 571 registered .csproj; the old
     flat "~3 min" cap was already wrong (a 600s ceiling once killed a *passing* full suite). Corpus
     growth: 371 → 457 → 518 → 543 → 569 packages.
     The i9-13900K desktop DIED of hardware failure 2026-08-09. Laptop (Ryzen 7 PRO 6850U): full
     behavioral suite 1,792s on 2026-08-07 with nothing else running.
     The replacement coordinator (2026-08-10) is an i7-5820K — 2014 Haswell-E, 6C/12T, 32 GB. Day one:
     full behavioral suite 2,820–4,131s solo (4,131s cold-ish; 2,820s re-measured 2026-08-10), CNR
     1,505s solo / ~3,190s with two sibling lanes, converter `go test ./...` 200s solo / 332s loaded,
     full go2cs.slnx Debug build 1,432s cold, archive/zip Debug test suite 774s (vs 391s on the i9).
     go test -timeout: a healthy converter suite was killed at exactly 600.4s with a goroutine dump
     that reads like a hang (2026-09-01; 236s solo, 578s under one sub-agent's load, dead at the wall
     under two).
     Re-measured 2026-08-21 on the same i7-5820K: full behavioral suite ~6,552s at 603 packages (and
     the runner batch-build default needed 9,000s at 604 projects — the stock 2,400s false-redded a
     healthy run, 2026-08-22); full go2cs.slnx Debug --no-incremental ~3,546s at 722 projects.
     go2cs.slnx re-measured 2026-08-29, same box: 845s wall SOLO at 802 assemblies
     (--no-incremental -m -p:UseSharedCompilation=false, golib rebuilt, 385 corpus warnings emitted —
     positive evidence of a genuine full compile rather than a skipped-work green, which is the only
     reason the number is worth quoting). The tree GREW (722 projects → 802 assemblies) while the wall
     FELL 3,546s → 845s, and no corpus change runs that direction — so read 3,546s as the
     under-sibling-load end (never recorded as solo) and 845s as the current solo baseline. Budget the
     row from the loaded end (~3,600s), and treat a SOLO run materially past ~900s as contention to go
     find rather than work to wait out.
     Watchdogs raised 2026-08-10 with the evidence in a source comment: PerformanceRunner's 600s AOT
     cap and BehavioralRunner's 300s build-all cap. AOT publish cost ~7s each on the i9 in the stub
     era; ~25 min each on the i7-5820K now that ILC compiles the full converted-stdlib closure per
     benchmark (post-unification). Concurrent lane load pushed a healthy publish past even an 1,800s
     cap once, and only the Measure phase's numbers are trustworthy on a quiet machine anyway.
     The extrapolation rule is 2026-09-02: a budget comment presented "~236 s fixed, ~62 min" as
     measured when the fixed term is not constant at all (6 shared deps in a 3-project slice against
     ~31 corpus-wide) and the runs behind it had timed a different flavour. -->
  | Command | Measured (warm) | Set timeout | Notes |
  |---|---|---|---|
  | `run-behavioral.ps1` (full, 4 phases) | **~370–1575s (6–26 min)** at 544–549 projects; **1,916s at 652 projects**, laptop-class, SOLO | 2100s | Transpile+Compile+Target over all; Output-compared only where `[GoTestMatchingConsoleOutput]`. The top of the range is concurrent-lane load — budget for it. ⚠ At that load the **Go toolchain itself** can crash building one project (`panic: … compress/flate.(*huff…` inside `go build`) and the runner reports it as a Go build failure; re-run that one project filtered before believing it |
  | `check-no-regression.ps1` (full) | **~1,050–1,750s (17–29 min)** at ~625 packages on the i7-5820K (1,059s/1,132s solo, 1,440s/1,711s under sibling load; laptops 720s and 1,060s) | 2400s | transpile-only, no compile/run; re-transpiles unconditionally |
  | `run-behavioral.ps1 --filter <Name>` | **~10–20s** (8 projects) | default | the iteration loop — use this, not the full suite |
  | `go2cs -stdlib -comments` (full reconvert) | **~195–240s** | 600s | 307 projects; per-file work is sub-second, the cost is `go/packages`. A three-target `-platforms` merge is ~3x this (545s measured r50a) |
  | single `core` pkg build | **~6s** (log/slog) – **~60s** cold (go/types) | 180–400s | cold includes the dependency chain |
  | full `go2cs-stdlib.slnx` build | **~92–188s** warm (307 projects, `-p:UseSharedCompilation=false`). ⚠ **i7-5820K on a healthy disk: 516s** `--no-incremental` | 600s (900s on the i7 class) | cold restore adds a few minutes. `-p:GoTargetOS=linux` is a DIFFERENT build and completes clean: **307/307, 0 errors, 475s**. It must be run `--no-incremental`: what differs between targets is the `<Compile>` ITEM SET, not any source timestamp |
  | full `go2cs.slnx` build | **~87s** `--no-incremental` / **~39s** incremental at 573 projects; **845s solo at 802 assemblies** | 900s | the ONLY gate that compiles the non-generated solution members (utilities, examples) — run it after any golib/runtime API change. ⚠ Under concurrent-lane load a `go2cs-gen` run can die with `AccessViolationException` inside `TypeGenerator`'s recursive `PromotedStructDeclarations`, reported as an `error` against the package: re-run before believing it |
  | `run-validated-sweep.ps1` (full roster) | **~46–53 min solo** (i9 era, 3,138s at 109 packages / 13,611 verdicts); i9 full roster **7,059–7,705s** at 159–162 rows | run it BACKGROUNDED from the COORDINATOR session only — ⚠ a LANE parking a detached sweep and ending its turn gets it KILLED (the lane's process tree is reaped); recovery: re-run `roster − logged` inline and check the verdict arithmetic closes | ~29 s for a typical package; use `-Filter` for anything but a final gate. ⚠ **ELEVEN packages carry per-package deadline FLOORS in the script's `$longTimeouts`, slow-host-calibrated — the script is the authority, this prose is a pointer.** The table is a **FLOOR, not an override**: a LARGER `-TestTimeout` raises it for a still-slower box; a smaller one still loses | <!-- Table-row derivations moved here 2026-09-12.
     run-behavioral.ps1: 642s measured 2026-08-07 at 549 projects with a sibling lane converting;
     416–957s on 2026-08-05 SOLO at 545 across four r41 stage gates (the spread is warm-vs-cold C#
     build state, not load); 626s on 2026-08-04 at 544; 1575s on 2026-08-02 with THREE sibling
     worktrees running pipelines. 549/549 Transpile+Compile+Target; 523 Output-compared, 26 skipped
     (no `package main`). Data point 2026-09-01: 1,916s at 652 projects, laptop-class host, SOLO,
     runner invoked DIRECTLY (not via the Stop-preference wrapper) with --build-timeout 10800
     --build-one-timeout 900 — the stock 2400s batch cap sized at ~604 projects would have reported
     the whole corpus NOT MEASURED at 652.
     check-no-regression.ps1: re-measured 2026-08-17/19 at ~625 packages on the i7-5820K. The prior
     row read 350–510s/700s at 574 packages on the dead i9 — a timeout kept at that figure kills every
     healthy run on this corpus.
     go2cs -stdlib -comments: 240s measured r47a 2026-08-08 with two sibling lanes; 223s at r41,
     2026-08-05. Three-target merge 545s measured r50a.
     go2cs-stdlib.slnx: 149s measured r50a at -p:GoTargetOS=windows, 188s at r41, 158s at r40, all
     with -p:UseSharedCompilation=false (the isolation flag a lane uses instead of build-server
     shutdown). 516s --no-incremental on the i7-5820K, 2026-08-14. The linux target's 475s is
     2026-08-14, after the three-target regen wave — docs/phase4/CENSUS-linux-compile-wall.md §10.
     go2cs.slnx: 573 projects measured 2026-08-07. The AccessViolationException was seen once on
     core/runtime and was NOT reproducible in two immediate retries with identical flags.
     run-validated-sweep.ps1: 3,138s measured 2026-08-07 at 109 packages / 13,611 verdicts; the roster
     was 131 packages / 14,769 matching verdicts / 47 disclosed when re-measured 2026-08-14 — so budget
     well ABOVE 3,138s and re-measure. ~90+ min under two concurrent lane loads; both r47 attempts were
     killed externally before finishing, so no clean loaded figure exists. The lane-kill happened twice
     on 2026-08-08 at 106/110 and 98/110, log ending between packages with no summary. i9 full roster
     7,059–7,705s at 159–162 rows (2026-08-22) — the 46–53 min row is the dead i9-13900K era and stands
     only as the ratio anchor. $longTimeouts re-counted 2026-09-02 at ELEVEN: sync/atomic 60m, net 40m
     and net/http 60m joined the original eight (hash/maphash 60m, index/suffixarray 120m, crypto/dsa
     120m, archive/zip 60m, go/parser 90m, crypto/internal/mlkem768 30m, crypto/tls 30m, time 40m — its
     1.23.12 suite is 169 tests and ~19 min on laptop-class). net/http's floor was sized to a TRUNCATED
     Debug measurement on the i7 class, where the row's two arms bracket the train's 30m at 1,836s and a
     deadline-killed 2,171s. The table grew three rows and two floors moved by 2026-08-25, which is WHY
     the shard map derives its reserved set from the script at generation time instead of copying a
     sentence. Slow-host-calibrated since 2026-08-10 — the original i9-sized values false-red every bare
     sweep on this machine class (hash/maphash and crypto/dsa both reported
     `FAIL … package timeout after 00:30:00` here; maphash then validated 22/22 in 2,406s / 40.1 min
     given room). FLOOR-not-override since the same date: before that fix the -TestTimeout flag was
     silently ignored for exactly the four packages that need it. -->
## Runner timeout budgets, statuses and sharding
- **`BehavioralRunner` has its OWN internal timeout budgets, and no timeout the CALLER sets can influence them** — a generous outer budget on the `run-behavioral.ps1` call does nothing if the runner kills its own child first. They are overridable, in SECONDS, at **flag > environment variable > default**: `--build-timeout`/`GO2CS_BUILD_TIMEOUT` (batch build, **2400**), `--build-one-timeout`/`GO2CS_BUILD_ONE_TIMEOUT` (per-project build, shared-dep pre-build, `go build`, **300**), `--transpile-timeout`/`GO2CS_TRANSPILE_TIMEOUT` (**60**), `--run-timeout`/`GO2CS_RUN_TIMEOUT` (one program run in the Output phase, **30**). **The build defaults are sized for the slowest legitimate host; a fast lane opts DOWN explicitly (`--build-timeout 300`).** <!-- Hardcoded constants until 2026-08-10. The slow-machine row that sized them (i7-5820K 6C/12T,
     ~3x the desktop rows, at 555 packages): the one-shot parallel build exceeded the stock 300s COLD
     AND WARM ALIKE — warm state cannot save it, because the Transpile phase rewrites every .cs
     immediately before Compile, so the batch is never an incremental no-op. For scale, a full
     `dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false` of the same tree took
     1,432s cold (573 projects, 0 errors), ~5x the old 300s batch budget; a single cold filtered
     project measured 163s. -->
- **A budget that EXPIRES is reported as `NOT MEASURED`, never as a failure** — a fourth `Status.Timeout` alongside Pass/Fail/Skip, borrowing CNR's word. Timeouts still fail the run and still exit 1 (an unmeasured project must never read as a pass) but are counted, listed and summarized separately, and the per-project fallback bails out after **3 consecutive** timeouts. <!-- This closes a FALSE RED, the mirror of the false-green routes: on the cold slow machine the batch
     timed out, all 555 projects fell to the sequential per-project fallback, each also exceeded 180s
     (every one must first build the core dependency closure), and ~15 minutes produced zero assemblies
     and 555 Status.Fail entries that read exactly like a corpus regression. Two related traps the same
     change closed: an Output-phase run timeout used to surface as `exit code mismatch: C# -1 vs Go 0`,
     i.e. as a *behavioral* divergence naming a real test; and the fallback used to spend the full
     budget on all 555 to re-learn one fact. -->
- **An unfiltered Output leg must SHARD, and the shape is shard-with-purge: alphabetical slices, `clean-bin` between them, verdicts unioned — never a narrowed enumeration.** Every behavioral project's build output copies the same ~55-dll core closure into its own `bin` (~29 MB each, ~20.5 GB at 695 projects), so one batch cannot fit a hosted runner's disk. **`BehavioralRunner`'s `--filter` is a case-insensitive SUBSTRING, so no filter set can partition the enumeration**: a sharding leg takes an INDEX SLICE over the deepest-first list and asserts the slice counts sum to the whole. <!-- 2026-09-02: filter `S` matched 455 of 664. The durable follow-up is a shared-closure csproj
     template. -->
- **THE RUNNER'S OUTPUT PREDICATE IS THE OPT-IN ATTRIBUTE, NOT `package main`** — the predicate is "the package's `package_info.cs` holds a line equal to the attribute", so the projects without a `main` are a strict SUBSET of those skipped, and **a battery expectation written as "enumeration minus the no-main projects" reads a CORRECT run as short by the difference.** Platform-exclusives sit OUTSIDE the enumeration entirely — do not subtract them twice. **A DIRECT runner invocation SKIPS the wrapper's disk preflight**, so check that floor by hand before launch. <!-- 2026-09-08. Two internally consistent WRONG arithmetics preceded the one that closed, the first
     double-subtracting the platform-exclusives. Companions from the same run: the converter REBUILT
     under the two-pin pairing came out BYTE-IDENTICAL (hash equal, mtime moved), so the build is
     deterministic, measured for the first time; and a tree reading clean after a full round of
     in-place transpiles is the Target phase's verdict reached by a second route. -->
## Reading logs and driving PowerShell instruments
- **Redirect a long run to a file with `Start-Process -RedirectStandardOutput` and read the file — never `... *>&1 | Out-File`, `Select-Object -Last N` or `-First N`.** `-Last N` and the pipeline redirect BUFFER all output until completion, so a backgrounded suite looks stuck at its first line for its entire duration and a run that dies leaves a few-hundred-byte log ending mid-line, indistinguishable from an external kill. **`-First N` is WORSE: it terminates the pipeline once satisfied and KILLS the upstream native process mid-run.** Check liveness with a process census, not the output file. **In BASH, `*>&1` is not redirection syntax at all — the shell GLOBS it, silently no-op'ing the command.** <!-- -First measured 2026-08-16: a -stdlib reconvert died at ~100/304 with exit −1, reading exactly
     like a converter failure. Buffered-redirect measured 2026-08-31: a 485-byte log from a dead
     full-suite run. Bash glob measured 2026-08-31: one CNR and two runner attempts read as failures
     that never ran. -->
- **PowerShell-REDIRECTED output is UTF-16LE, and an ASCII grep over it returns a well-formed EMPTY.** Both `go2cs.exe … > log 2>&1` and a `Tee-Object` log land as UTF-16LE, so `grep <marker>` finds nothing and reads as "probes never fired" / "the run never happened". **Tell, one command: `head -c 200 <log> | tr -d -c '\000' | wc -c` — a nonzero NUL count means every grep against that log has been lying** — then decode (`iconv -f UTF-16LE`) before grepping. <!-- Measured twice 2026-08-31, independently: a full retraction was built on six such empty greps,
     and a CNR verdict was nearly lost the same way. Same silence-not-error family as the globbed
     *>&1 and the buffered pipe. -->
- **THE TRUNCATED-LOG READING INVERTS FOR POWERSHELL WRAPPERS.** A wrapper running at `$ErrorActionPreference='Stop'` (`run-behavioral.ps1` line 49) dies on the FIRST native stderr line — killing the WRAPPER and leaving the runner alive, orphaned and invisible — and the truncated log reads exactly like the run being killed, inviting the restart that puts two runners in one behavioral tree. **Before believing a truncated wrapper log, census for the CHILD by executable path; a lane driving a long native child invokes it DIRECTLY (or at `'Continue'`).** <!-- Measured 2026-09-01, a self-inflicted two-runner race. -->
- **A COMMAND THAT DOES TWO THINGS CAN HALF-SUCCEED, AND THE EXIT CODE TELLS YOU NOTHING ABOUT WHICH HALF — a timeout does not roll back what already ran.** Check BOTH effects independently. **And a tool's own timeout is not the COMMAND's timeout; the shorter one wins silently** — a solution build given a 30-minute internal budget died at 10, the harness cap, reporting exit 143 with an empty log, which reads exactly like a build that produced nothing. <!-- 2026-09-07: one invocation launched a detached build, wrote a mailbox entry file and posted it;
     the tool's 2-minute cap fired partway, the build started and the post silently did not, and the
     entry file the post needed was never written — so the retry failed on a MISSING FILE rather than
     on the real cause. Check a process census for the build and the mailbox tip for the post. -->
- **PowerShell's `.Count` on an EMPTY result prints BLANK, not `0`** — wrap it `@(...).Count`, and **read a blank where a number belongs as a BROKEN INSTRUMENT**, not as zero. <!-- `(Get-CimInstance … | Where-Object {…}).Count` rendered as `go2cs.exe:` with nothing after it and
     was read as "no converter running" when the honest reading is "this told me nothing". -->
- **A WINDOW-GREP (`grep -A8`) OVER A STRUCTURED LOG INVENTS FINDINGS** — a fixed window spans record boundaries and attributes one block's lines to the next, so the census reports a variety the log does not contain. **Parse a structured log on ITS OWN record boundaries, never on a line window.** <!-- Measured 2026-09-08 by C2 (batch19): `-A8` over GolibTests failure blocks INVENTED six
     non-kernel32 findings where block-boundary parsing gave 48 of 48 identical
     DllNotFoundException-on-kernel32 blocks. -->
- **Never inject non-ASCII C# source (`Ꮡ`, `ж`, `Δ`) through a PowerShell command STRING** — the argument pass mojibakes it even when file I/O is correct; write such content with the Edit/Write tools.
- **A `.ps1` written UTF-8 WITHOUT A BOM is parsed by Windows PowerShell 5.1 under the system codepage, so a literal non-ASCII glyph in the script's own source mojibakes at PARSE time and the instrument reports whatever a never-matching pattern reports.** Here, file I/O is NOT correct, so the Edit/Write fix does not apply: **write the `.ps1` with a UTF-8 BOM** (`[System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true))`). **Positive-control any regex-bearing PowerShell instrument that embeds a converter glyph literally** — run it against a known-populated target and confirm a nonzero count before trusting a zero anywhere else. <!-- Measured 2026-08-30, the syscall-pinning census-guard lane: a regex matching ᴋ / a comparison
     against Ꮡ produced a false "0 sites found" that read as a correct RED against a not-yet-fixed
     corpus, and stayed silently wrong against a freshly fixed one until the fresh run's *also* being
     zero broke the positive control. The BOM was confirmed to make 5.1 parse the literal correctly. -->
- **A SHARED PowerShell instrument owes a run on BOTH editions before it banks: 5.1 on a Windows lane AND 7 on a Linux lane — or the OS-matrix linux leg — before the change merges.** The check IS the PARSE of every shared script under pwsh 7 Core, plus one row actually run. **Measured is not parsed, and not-yet-closed is not untested — say which of the three a check was.** <!-- Measured 2026-09-02: _roster.ps1's comparison reader took `Add-Type -AssemblyName
     System.Web.Extensions` — a genuine PS 5.1 case-folding fix, smoke-proven on Windows only, and
     .NET-Framework-ONLY. Under pwsh 7 the script died at its second block, so the sweep's three
     absorption arms (host-conditional, capability-absent, host-limit) silently DECLINED on every Linux
     host, and the catch's own message named the missing assembly rather than the missing capability —
     pointing at the wrong artifact, in the file that decides whether a row banks. Fix: an
     edition-conditional reader (Desktop keeps JavaScriptSerializer; Core uses
     System.Text.Json.JsonDocument, explicit, never -AsHashtable behaviour inherited from a newer
     host), and the guard exercises both.
     Cloud hosts (2026-09-02): a container may carry NO PowerShell on PATH
     (`dotnet tool install --global PowerShell` lands one on the user's tool path) and its writable
     allowance may sit under the sweep's own disk-preflight floor — such a host runs the edition and
     gate checks with -IgnoreDiskPreflight STATED, and never banks a Linux row. Corrected 2026-09-04:
     verify what a host HAS before stating what it LACKS — the container that reported "no PowerShell
     at all" corrected itself the same hour; pwsh 7.6.5 was installed at the dotnet global-tool path
     and simply not on PATH. LOCAL form for a lane with no Linux host reachable (2026-09-04): the
     load-bearing half is EXERCISED on 5.1 Desktop AND pwsh 7 Core on the REAL path with a DECOY (a
     Framework-only API behind a variable is the System.Web.Extensions shape), with the Linux host
     NAMED as the honest closer. -->
- **PowerShell resolves COMMAND and VARIABLE names case-INSENSITIVELY.** A function named `Git` shadows `git.exe`, so `& git` inside it recurses until "call depth overflow" — and that line, captured through `2>&1`, counts as ONE dirty entry in a `status --porcelain` check and reads like real tree dirt. **Name wrappers distinctly, invoke `git.exe` explicitly, and take `status --porcelain` with stderr dropped.** Likewise a results array `$main = @()` silently overwrites a `$Main` parameter — **name arrays distinctly from parameters**, and let a function that RETURNS a number write its progress with `Write-Host` (a body that `Write-Output`s progress returns those lines AS its value and the caller's `Measure-Object` chokes on strings). <!-- Both measured 2026-09-02, coordinator. The overflow aborted a rebuild twice. The $main/$Main
     collision made every main-tree round run against an empty path and report "(no verdict line) 0s"
     while the control rounds read fine. -->
- **A `git` command run from a DELETED cwd prints plausible answers** — "0 commits not in master" for all seven branches, the only tell a `getcwd: cannot access parent directories` line at the END of the output. Re-run from the repo root before believing any count taken after a worktree removal.

## Naming a divergence
- **Before a divergence is NAMED, read the ORACLE at the ROW's own source and measure it under the SAME shape.** The source to read is the row's own `TestMain`, not the file the flag was registered in: `crypto/tls`'s prints `Usage of %s` over `os.Args` and exits 89 in bogo mode **by its own line**, which was reported as a divergence from what the flag package "normally does". **"Not what the package normally does" is not "not what Go does here."** <!-- 2026-09-02: a converted crypto/tls shim exiting 89 under bogo's flag set was compared against
     Go's 2, measured with the flag ALONE. -->
- **A shared CONVENTION name is not a shared MECHANISM.** `GO_WANT_HELPER_PROCESS` spans suites whose re-exec paths differ (`exec.Command` → `posixSpawnForkExec` against `syscall.Exec` → `execve`), so a "row X after fix Y" dependency adopted from a lane's note was false and died on the fix's own measured null. **Read the CALL PATH before scheduling a row behind a fix.**
- **A dramatic finding is re-derived from ITS OWN record before it is posted** — the record one file over is not this row's record. <!-- A `"disclosed": []` grep read off net/http's record was nearly published as `sync` falsifying its
     own TestOnceXGC disclosure. -->
## Performance comparison suite (`src/tests/Performance`)
- **Purpose: answer "how fast is the transpiled C# vs the original Go?"** 14 small `Perf*` benchmark projects (Startup, Fib, Sieve, MatMul, String, StringView, StringMatch, Map, Sort, Channel, IfaceCall, Iface, IfaceShell, RefLower), each a behavioral-test-shaped folder, measured across **three variants**: Go binary, C# JIT (`Release`), C# **Native AOT** self-contained. Drive via **`run-performance.ps1 [--filter X] [--no-aot] [--runs N] [--update-readme]`** (standalone `PerformanceRunner`, no testhost; phases Transpile → Build → Verify → Measure; **Verify requires identical timing-filtered stdout across all three binaries before anything is timed**). The results table lives in `src/tests/Performance/README.md` between `PERF-RESULTS` markers; prior toolchain tables accumulate in its *History* section. <!-- Added 2026-07-02. -->
- **Mechanics gotchas.** Benchmarks self-time via `time.Now().UnixNano()` (added to the baseline `core/time` stub for this) and print `elapsed_ns:` lines the runner strips before output comparison. The converter **regenerates each benchmark csproj on transpile**, so shared settings live in `Directory.Build.props`/`.targets` there. AOT is gated by a custom `-p:PerfAot=true` — **passing `PublishAot` globally breaks the netstandard2.0 `go2cs-gen` analyzer with NETSDK1207**; AOT publish needs MSVC `link.exe` and the runner prepends the VS Installer dir to PATH for the SDK's `vswhere` probe; AOT trims with `TrimMode=partial` because golib `fmt` formatting and sort's `Interface<T>` bind members via reflection. **Each AOT publish now ILC-compiles the full converted-stdlib closure (~25 min each on the i7-5820K), so a full run is HOURS and must run SOLO; `--no-aot` drops the whole column.** Keep each benchmark ≥50 ms and output deterministic (inline xorshift, no `math/rand`). <!-- Cost changed at the 2026-08-01 tree unification: ~7s each in the stub era. Concurrent lane load
     once pushed a healthy publish past an 1,800s watchdog. -->
- **A silently-UNREACHED package init is a corpus-wide false green — trace the CALL CHAIN, not a `catch`.** `schedinit` never runs, so `cpuinit`/`cpu.Initialize`/`doinit`/`cpuid` are UNREACHABLE and every `X86.Has*` is simply its zero value: x86 feature detection is all-false corpus-wide and every AES-NI/AVX fast path runs its software fallback (the converted `crypto/tls` negotiates ChaCha20-Poly1305 where Go negotiates AES-128-GCM on the same host). The fix is a `[ModuleInitializer]` stand-in (the `goenvs`/`goargs` precedent) hand-owning `internal/cpu` over `System.Runtime.Intrinsics.X86`, 14 of Go's 20 flags mapped and 5 left false as the conservative direction. <!-- Found by the Verify phase 2026-09-02 (a SEMANTIC divergence before anything was timed) and
     originally root-caused as a SWALLOWED throw from the generated cpuid stub; CORRECTED the same day —
     there is no swallow, the chain is simply never entered. Read both halves together. -->
- **For a near-threshold SERIAL-latency row, core count is the wrong lever — a NATIVE control on the same host is what exonerates the stack.** Go passed at 250 ms where the managed side failed at 250/500/1000 ms in the same run, leaving managed-vs-native handshake latency as the residual — which was later FALSIFIED as the h2 pair's cause: a clean negative A/B moved 0 rows with AES-GCM negotiated, an isolated handshake is ~44 ms (which cannot blow a 250 ms rung), and the pair is a build-CONFIGURATION artifact. **A cut's justification stays what it MEASURED.**
- **A REFERENCE BENCHED THROUGH A COMMON WRAPPER COMPRESSES EVERY RATIO, AND A BAR THAT ABORTS ON ITS OWN REFERENCE IS THE ARM WORKING.** Adding shared overhead to BOTH sides moves every ratio toward 1, so a candidate that clears the bar on a direct measurement fails it through the wrapper. **Calibrate a bar on the DIRECT call the specification measures, and report an abort as HAVING FIRED — never smoothed into "validated first time".** <!-- 2026-09-08, batch19: a harness benched the user-mode reference THROUGH the syscall trampoline
     (63 ns), adding ~55 ns of common overhead to both sides, so the proposed kernel anchor scored
     4.58x and the harness ABORTED emitting no ratio. The bar is calibrated on the direct call
     (2.1–2.6 ns); re-measured direct, `GetProcessId(GetCurrentProcess())` sits at 68–87x on 20 of 20
     processes and is the anchor. -->
- **The unreached-init class, one member wider: a converted package can be TWO CONTRADICTING HALF-IMPLEMENTATIONS** — hand-owned no-ops beside converted checkers that nil-deref or return at their first line — so neither half can be read as the package's behaviour. **Read the DEFAULT's ASSIGNMENT PATH, not the field's declaration, before believing a check runs**: `debug.cgocheck = 1` is assigned only on the `schedinit` → `parsedebugvars` path, which never runs, so the field's DECLARED value is not the value any check sees. <!-- 2026-09-05; same door as internal/cpu's. -->
- **A CONVERTED CALL GRAPH IS NOT GO'S CALL GRAPH: a builtin displaced into golib SEVERS the chain below it, and an UNRESOLVED node in a call-graph census is precisely where the severance announces itself.** A prediction read off Go's call graph does not carry to the conversion when a node is DISPLACED at the golib boundary. <!-- 2026-09-08. One author had the unresolved-node tell in hand, filed it as a footnote, and traced
     Go's whole write chain anyway "because that is what Go does" — distinct from citing a call site
     without reading the callee: here every callee Go HAS was read, and nobody asked which of them our
     corpus still OWNS. The measurement that settled it: one flavour was predicted MUTE on a runtime
     fatal (Go's print chain bottoms out in a bodyless stub there) and it PRINTED, frame for frame
     identical to the other flavour, because the converted print IS the golib call and the runtime's
     own write chain is never entered on ANY flavour; the stub is exactly as read and simply UNREACHED.
     Two companions: the falsifier that FIRED took an item OFF the remedy (one shape on three flavours
     rather than two), and the insurance control was NOT load-bearing for the non-null result and its
     author said so rather than letting it read as the rescue. -->
## Adding a regression test when a converter defect is fixed
When a meaningful converter bug is fixed, lock it in with a behavioral test. **Prefer extending an existing behavioral project** if one already covers a similar construct; otherwise add a new one (example: `tests/Behavioral/GlobalStructFieldPointers`, guarding the `&cpu.X86.HasADX` cross-file address-of-field fix).

1. **New folder** `src/tests/Behavioral/<Name>/` with a Go program exercising the specific construct (multiple `.go` files are fine and run as one package — needed for cross-file bugs). Include a `go.mod` (`module go2cs/<Name>`), copy `go2cs.ico` + a `<Name>.csproj` from a sibling test (adjust `AssemblyName`; keep the `golib`/`fmt` refs the program needs), and verify with `go run .` first. ⚠ **A test carrying a nested sub-library PACKAGE inside its own module takes a BARE `module <Name>` and imports `<Name>/<sub>`** (the `NamedSliceChildPkg`/`netlike` pattern) — the converter references the sub-library as `<sub>/<Name>.<sub>.csproj`, the name it also emits for the sub-library's own project.
2. **Make the Go↔C# output match** so `OutputComparisonTests` passes. Mind known runtime limitations — `Ꮡ(value)` (address of a non-boxed value) currently boxes a *copy*, so don't write through a `&global.field` pointer and then read the *original* global; read back through the same pointer.
3. **Register in the solution** — add `<Project Path="tests/Behavioral/<Name>/<Name>.csproj" />` under the `/tests/behavioral/target-projects/` folder in `src/go2cs.slnx` (alphabetical). **If the test pulls in a sibling library sub-project via `<ProjectReference>`, register THAT too**, on the line right after its parent. **Then verify it stuck: run `./check-solution-integrity.ps1`** from `src/tests/Behavioral` (it asserts every behavioral `.csproj` on disk is registered and flags dangling entries, exit 1 on violation; it also runs as CNR's preflight). This matters because **the harness builds each `.csproj` BY PATH, not via the solution, so a missing registration still passes the whole suite** — it only breaks the `go2cs.slnx` build in Visual Studio (CS0246/CS0234). If VS has the `.slnx` open it can rewrite the file and silently drop an external edit — re-add and re-verify.
4. **Transpile once** (`go2cs.exe src/tests/Behavioral/<Name>`, no `-comments` — behavioral goldens omit them) to generate the `.cs` + `package_info.cs`, and add `[GoTestMatchingConsoleOutput]` to the generated `package_info.cs` class for output comparison (a hand-added attribute the converter preserves).
5. **Generate tests + goldens:** run **`UpdateTestTargets --createTargetFiles`** (from its `bin/Debug/net10.0`). It scans every `tests/Behavioral/*` folder, rewrites the `// <TestMethods>` blocks in all four `*Tests.cs` classes (adding `Check<Name>()`), re-transpiles every project it is about to re-baseline, and copies each freshly transpiled `.cs` to a `.cs.target`. `git status` should then show only your new project + four `+3`-line test-class diffs. A whole-tree run is CNR-length; **`--only <Name>`** re-baselines one project and exercises the refusal branch.
6. **Verify (filtered, fast):** from `src/tests/Behavioral`, `./run-behavioral.ps1 --filter <Name>` → the 4 phases in seconds with no testhost/lock risk. Equivalent MSTest path: `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` from `BehavioralTests`. Avoid the full `dotnet test go2cs.slnx` while iterating.
7. **Record the conversion decision in the same change** — the strategy lives in TWO documents. [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) is the exhaustive technical reference and takes nearly every decision: add or update the `###` subsection under the matching `##` topic with the emitted form, the edge case, the reasoning, and the guarding behavioral test. [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) is the high-level summary — update it only when the decision changes the *headline* mapping of a construct or warrants a clearer example. **Verify every C# snippet against the actual `.cs.target` golden** (the authoritative record of emitted forms — `u8` format strings, `throw panic(...)`, `ж<T>`/`Ꮡ`); prefer real snippets from the converted stdlib in `src/core`. Skip only for pure bug-fixes restoring already-documented behavior. This applies to *any* commit landing a notable conversion decision, not just this flow. <!-- Step 1's bare-module rule: measured 2026-09-02 — under `module go2cs/<Name>` the parent's
     reference becomes go2cs.<Name>.<sub>.csproj, a file nothing emits, and the build dies CS0246 on
     the sub-library's namespace inside the GENERATED shells, pointing away from the go.mod (paid
     2026-09-05, the ReflectFieldMetadata guard). The corpus agrees: 24 of the 27 behavioral projects
     with a nested sub-package are bare, and the three that are not give the sub-library its own
     go.mod, i.e. a separate module path.
     Step 3's registration examples: GoNamespaceShadow → nsshadowlib/go.nsshadow.csproj;
     IoLike → IoLike/FsLike; NamedSliceChildPkg → .../netlike. That is exactly how `nsshadow` slipped
     through (added in 96eff53cd, unregistered until 53dd2497e).
     Step 5: the transpile is what makes step 4 a convenience rather than a prerequisite. The refusal
     branch exits non-zero naming any project whose transpile failed, timed out, or degraded.
     Step 6: the golden comparison is line-ending-insensitive, so a multi-line string literal needs NO
     .gitattributes handling for the byte compare — mark the .cs -text only if the compiled program's
     behavior/output depends on that literal's exact newlines. -->
- ⚠ **Windows CASE trap when you `git add` the new folder.** `git add .` / `git add -A` — and any add run from a cwd *inside* the tree — records the path git gets from **readdir, i.e. the ON-DISK casing**, whereas an explicit lowercase pathspec (`git add src/tests/Behavioral/<Name>`) is canonicalized to the casing already in the index. Under `core.ignorecase=true` the difference is invisible locally: ONE directory on Windows, TWO on any case-sensitive filesystem, where the `.slnx`'s lowercase registration then fails to resolve. `check-solution-integrity.ps1` now asserts case-sensitively that every tracked path under the behavioral tree is exactly `src/tests/Behavioral/…`. **If it fires: `git mv` will NOT do a case-only rename on Windows** — rewrite the INDEX with plumbing (`git update-index --force-remove <wrong-cased-path>`, then `git update-index --add --cacheinfo 100644,<sha>,<lowercase-path>` reusing the SHAs from `git ls-tree -r HEAD`, which keeps the blobs byte-identical) **and fix the on-disk directory casing too** (rename through a temp name, `Tests` → `__tmp__` → `tests`), or the next `git add -A` re-creates the wrong path. Both are working-tree-invisible: `git status` stays clean throughout. <!-- Found 2026-08-07: a clone whose src\tests had drifted to a capital src\Tests on disk banked
     DeferFrameScopes at src/Tests/Behavioral/… while the other 4,240 files stayed src/tests/…. -->
- ⚠ **DO NOT CAPTURE A GOLDEN AGAINST AN UNSETTLED QUESTION.** **A golden is read as a SPECIFICATION by everyone who meets it afterwards, and nothing in the file says which it is** — so capturing one while a bridge question is still open records what the bridge DOES rather than what it SHOULD do, and converts a divergence into a contract by accident. Same move as laundering a bug into a disclosure class, in a different artifact.
- ⚠ **ONE WORKTREE PER CUT — `UpdateTestTargets` enumerates the DIRECTORY, not your change.** A stray untracked project left by ANOTHER cut is enumerated into this cut's four test classes, and **the ASYMMETRY is the tell**: one new project gives `3/3/3/3`. Neither that nor two dirty converter files from a neighbour fails a gate, so the check is the diff's shape — **count the added `Check<Name>()` lines per class before staging.** <!-- Measured 2026-09-02: that run gave 6/3/6/6. -->
## Toolchain pins around goldens and guards
- **A golden regenerated under the wrong toolchain is worse than every other toolchain trap this repo carries, because those eventually fail loudly while this one rewrites the RECORD later comparisons are measured against** — the wrong release's emission becomes the definition of correct, at exit 0 with nothing refused. **The re-transpile that precedes a `--createTargetFiles` copy is toolchain-pinned like any other emission, and any golden-regeneration path that rebuilds `go2cs.exe` itself RESOLVES and CHECKS the toolchain first and ABORTS naming both releases.** Printing the pin is not enough; this repo has already paid for an instrument that printed its pin and carried on.
- **A GOLDEN RE-BASELINED UNDER A RUN `GOROOT` THAT DIFFERS FROM THE CORPUS'S RELEASE RECORDS THE EMISSION FOR A CORPUS THAT DOES NOT EXIST — and the tell is Target GREEN with Compile RED.** Same binary, same sources, opposite stamp: the decision is a function of the LOADED CLOSURE, i.e. of the run `GOROOT`. **A golden is re-baselined AT the corpus hop, never in the window before it.** **A mint showing that pair is HELD and DELETED, its registrations reverted with it — never left uncommitted for the next reader to meet as a record.** <!-- The Target-PASS/Compile-FAIL pair re-read 2026-09-08 (batch19) on a mint whose golden matched an
     emission that does not build: held, deleted, registrations reverted, rather than parked.
     2026-09-08: eight goldens "drifted" under the newer run pin by exactly one change each — an alias
     prefix dropping off one package's name — the re-baseline made each golden match the emission to
     the byte, and the emission did NOT BUILD against the corpus at its own release. Under the two-pin
     pairing (converter built at the newer release, environment re-exported to the corpus's) all eight
     are byte-identical to the goldens already committed. -->
- **Placement: a guard must PRECEDE the check that DRIVES the bad outcome, or it is inert while looking identical in review.** The shared staleness predicate compares the binary's embedded release against live `GOVERSION`, so on an unpinned host **the predicate itself triggers the wrong-toolchain rebuild** and a pin check placed after it runs on a binary already rebuilt wrong. Ask what ORDER the bad outcome is produced in, not merely where the check "belongs" — **and run the control in the environment the tool EXPECTS** (from the repository root the utility exits on its own path derivation and never reaches the guard, so the first control "passed" against code that never executed).
- **WHEN THE INSTRUMENT THAT HAS THE GUARD CANNOT RUN AND ONE THAT LACKS IT CAN, CHOOSING THE GUARDLESS TOOL IS A RULING, NOT A LANE'S CONVENIENCE.** **Deliver the half that passes and mark the other BLOCKED, not skipped.** **And evaluate a two-part remedy AS THE PAIR** — a correction that reads one half objects to something nobody proposed, and adopting it on reading rather than re-deriving the axis is most expensive in exactly the window a lane is about to act on it. <!-- 2026-09-08, batch19: `UpdateTestTargets` refused a golden under BOTH toolchain settings in the
     two-pin window — under `auto` its guard reads `go env GOVERSION` with cwd = the converter module
     (which switches up) and reports a mismatch that does not exist in the environment; under `local`
     its single-toolchain staleness predicate rebuilds the converter and the rebuild refuses
     (`go.mod requires go >= …`). Meanwhile the behavioral runner's `--update-targets` reads embedded
     == live at ITS OWN cwd and would have MINTED without complaint, which is exactly how eight wrong
     goldens were minted earlier (see the run-GOROOT rule above). The lane did NOT route around the
     guard: it delivered four registrations (+3/−0 each) and left the other half BLOCKED. The
     pair-evaluation half is the correction/retraction sequence recorded in the axis comment below. -->
- **A toolchain guard probes from where the BUILD runs** — correct under both `GOTOOLCHAIN` settings: under `auto` the build switches to the release the module's `go` directive requests and the guard sees the switched release; under `local` no switch occurs and the guard sees the ambient one and refuses. **The invariant is *measure the toolchain that mints the artifact*.** And **reconcile the FORMATS**: `<GoStdLibVersion>` yields `1.23.12` while `go env GOVERSION` yields `go1.23.12` — without the prefix reconciliation the comparison mismatches ALWAYS, **a guard that refuses everything, which is exactly as useless as one that passes everything.** **A FORMAT EXPECTATION IS READ OFF THE PRODUCER'S SOURCE BEFORE IT BECOMES A PREDICATE** — the comparison record's `oracleGoVersion` holds the bare `go version` OUTPUT (`go version go1.23.12 windows/amd64`, `testConversion.go`), not the bare token. **And reconcile the AXES as well as the formats: the BUILD axis is the binary's embedded release against the converter module's own `go` directive; the corpus EMISSION pin (`version.props`, and the ambient toolchain that a no-`go.mod` cwd resolves) is a DIFFERENT axis that the two-pin window separates by definition** — a guard comparing one against the other refuses a RULED condition, and **a guard that refuses a ruled condition trains people to route around it.** State the emission axis in the guard's own text as the window's accepted premise rather than dropping it silently. <!-- Ruled after one inverted finding. A predicate probing with NO module context compares ambient
     against embedded and reports STALE forever wherever they differ — a permanent spurious rebuild,
     which fails SAFE and is a cost question, not a correctness one.
     Format member measured 2026-09-08 (batch19): a train leg expected `oracleGoVersion` to read the
     bare token `go1.23.12`, so BOTH passing canary rows — 2,195 and 683 verdicts, oracle genuinely at
     the corpus pin — were stamped REFUSED, and the land script's required stamp carried the same
     unmatched spelling. The landing accepted them through a SECOND named instrument-fault path:
     exactly two such lines, no refusal of any OTHER value among them, both VERDICT PASS with `exit=0`
     present, plus a standalone re-measurement whose corrected predicate accepts 2 of 2 and REFUSES
     planted `go1.24.13`, `go1.23.1` and the bare token. The next derive corrects the template.
     Axis member, 2026-09-08, in TWO readings — the second supersedes the first and both are kept:
     (1) `UpdateTestTargets` read `go env GOVERSION` at the converter module (the BUILD axis, correct
     cwd) and compared it to `version.props` (the corpus EMISSION pin), refusing a condition the
     two-pin window deliberately establishes; the fix moves the COMPARE TARGET to the converter's own
     `go` directive, NOT the probe. That entry also claimed a probe at a no-go.mod cwd measures "the
     AMBIENT toolchain, a third thing that is neither axis", citing the STALE-forever note above.
     (2) SUPERSEDES (1) on that point, re-derived the same day: the converter's loader resolves GOROOT
     from the environment and every corpus and behavioral module declares the older directive, so the
     ambient toolchain at a no-go.mod cwd IS the emission axis (the release that parses the sources
     the utility re-transpiles), while the build axis is embedded-versus-directive. The STALE-forever
     note above stands and is now EXPLAINED: it compares a build-axis value against an emission-axis
     one. Both retractions went out by SHA within the hour and the lane holding the cut received ONE
     final instruction rather than three. -->
- **A PIN ASSERTION TAKEN INSIDE A MODULE UNDER `GOTOOLCHAIN=auto` IS CWD-SENSITIVE AND ANSWERS FOR THE SWITCHED TOOLCHAIN.** Inside the converter's own module both `go version` and `go env GOROOT` report the SWITCHED release while the resolved `go` still lives under the pinned root, so a pin assertion taken there passes — or aborts — for the wrong reason. **Assert the pin from a directory with NO module file, or take the assertion under `local` even when the run needs `auto`; never read `go env GOROOT` as pin evidence from inside a module; and where a BUILD pin matters, verify the produced BINARY, which no cwd can switch.** <!-- 2026-09-08. Under `local` the converter cannot be built at all under the corpus pin. -->
- **`GOTOOLCHAIN=auto` ONLY SWITCHES UP**, so "probe from where the build runs" holds only when the ambient release is OLDER: with the directive `go 1.23.12` and an ambient go1.24.7, `auto` performs NO switch — a newer ambient SATISFIES the directive — and the built binary reports go1.24.7, so the guard runs against the wrong `GOROOT` and reports about it. **The right-spelling-of-the-wrong-release member, arriving through the door that paragraph calls safe.** <!-- Measured 2026-09-07/08. Corollary: a cloud lane reports its CONTAINER when it reports
     converter-suite colour — two container-only failures (a 1.24.7 GOROOT with no internal/weak, and
     a SHALLOW clone refusing a seeding push) reproduce at master with the change stashed, and neither
     can be fixed by care. -->
- **The `auto` requirement is a property of the SINGLE-ROOT design, not of the pairing: a SPLIT pin — one root named per `go build`, another named per conversion — needs no switch and is correct under BOTH settings.** But **the conversion half DOES depend on the toolchain setting, through the CHILD `go` the converter shells out to for package loading**: with a fixed binary and only that setting varying, a probe module declaring the NEWER directive converted against the NEWER standard library under `auto` and was refused verbatim under `local`. A split pin is sound on THIS corpus only because every corpus module declares a directive BELOW the convert pin — **the corpus's own directives, not the naming of two roots, are what keep the loader still** — and a module declaring above the pin (an end-user recursive conversion, or a hopped corpus against a stale pin) switches SILENTLY.
- **A CONVERTER-PIN MOVE IS NOT A CORPUS HOP.** The converter module's directive can move to a newer release while the corpus release property does NOT, and "the converter pins the newer release from here" reads at a glance as the corpus having moved. In that window: harness legs run entirely under the new pin, while **`-tests` rows run with the converter BUILT under the new pin and the pipeline RUN under the corpus's, because the ORACLE's release IS the corpus's release**; **a classification measured with the old-pinned converter is TREE-LOCKED to that pin** — a re-measurement with a newer-built front end over older sources is a DIFFERENT measurement, named with the pin on both sides, never a refutation; and **`-SkipBuild` is MANDATORY for the sweep**, whose own guard REQUIRES the corpus release. The guards split BY INVOCATION: a direct `-tests` conversion meets only the converter's own mtime guard, while any HARNESS-driven path meets the embedded-release-versus-toolchain predicate and REFUSES under the corpus pin. <!-- 2026-09-08. The refusal was MEASURED both ways: a corpus-pinned shell exits non-zero with NO
     binary (the module requires the newer release under `local`) while the same build under the newer
     pin exits 0 — so a pre-move instrument is UNBUILDABLE from master, not merely different, and its
     measurements are reproducible only from a pre-move checkout. -->
- **`-goroot` selects the corpus SOURCE tree but does NOT isolate the package LOADER**: the ambient root leaks into the loader's resolution of the internal packages, so a run whose shell still carries the BUILD pin fails with scores of undefined-symbol errors that read exactly like a corpus break. **Re-export the RUN's environment to the corpus pin, in a separate shell or explicitly, before the converter is invoked.**
