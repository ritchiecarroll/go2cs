---
paths:
  - "src/go2cs/**"
---

# Converter internals and the stale-binary routes

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 181-189, 225-771, 772-831, 832-997, 998-1081.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

Converter internals (full taxonomy in [`docs/Architecture.md`](docs/Architecture.md)):
- Entry: `src/go2cs/main.go`. Stdlib driver: `src/go2cs/stdLibConverter.go` (builds the package
  dependency graph + topological `sortedQueue`).
- `visit*.go` — walk AST nodes → C# declarations/statements (e.g. `visitFuncDecl.go`, `visitRangeStmt.go`,
  `visitDeferStmt.go`, `visitSelectStmt.go`).
- `conv*.go` — convert expressions/types (e.g. `convCallExpr.go`, `convSliceExpr.go`, `convStarExpr.go`).
- Analysis passes: `escapeAnalysisOperations.go`, `variableAnalysisOperations.go` (shadowing),
  `nameCollisionAnalysisOperations.go`, `constraintOperations.go` (generics), `importOperations.go`.

## Build / test workflow

- **Converter (Go):** built with the Go toolchain from `src/go2cs/`. Usage:
  `go2cs [options] <input_dir> [output_dir]`. Key flags (from `main.go`, authoritative):
  - `-stdlib` — convert the Go stdlib. `-stdlib fmt strings io` — convert only those packages (+filter).
  - `-recurse` — recursively convert an end-user module + its third-party deps (references the pre-converted
    stdlib via local `$(go2csPath)` project refs). A second positional output root isolates the generated
    `src\` app + `pkg\` dependency trees from that runtime root; converted packages reference one another
    relatively. Without it, recurse output defaults to `-go2cspath` for backward compatibility.
    `-recurse=module` narrows the SCOPE to the input module's own packages: every third-party package is
    still referenced into `pkg\<import-path>` but none is converted, so a dependency closure go2cs cannot
    convert can't hold up the module's own code (issue #32). Values compose — `-recurse=module,nuget`.
    A local-refs recurse conversion pins `$(go2csPath)` to the resolved runtime root in the output root's
    generated `Directory.Build.props` (condition-guarded default; relative `$(MSBuildThisFileDirectory)`
    form when the roots coincide, absolute otherwise) — before that pin an isolated output root fell back
    to the csproj template's `$(USERPROFILE)/go2cs/` default and no stdlib reference resolved (issue #36).
    `-recurse=nuget` instead emits NuGet PackageReferences
    (`go.<pkg>`/`go.lib`/`go.gen`, versioned `$(GoStdLibVersion)`) for the go2cs stdlib/runtime/analyzer so a
    converted app restores from nuget.org with no `deploy-core` staging; the app's own converted packages
    stay relative project refs, and the converter emits an output-root `Directory.Build.props` with a
    floating `GoStdLibVersion` default.
  - `-tests` — also convert the package's eligible `_test.go` suite + emit a runnable test-host project
    (default off; mutually exclusive with `-recurse` — `log.Fatal` on both). Forces `-comments` on (test
    conversions are derivative works), resolves the output path absolute, and self-locates `$(go2csPath)` by
    walking the output dir up to the first root containing `core/golib` — so the canonical two-argument form
    `go2cs -tests -test-action all <goroot-pkg-dir> <converted-pkg-dir>` needs no flags or env from a clone.
    ⚠ **Pass GOROOT EXACTLY as `go env GOROOT` spells it — a forward-slash path silently misroutes the
    whole emission into `namespace go.std.*`** (found 2026-08-24). `getProjectName`
    (`importOperations.go:48`) decides the namespace with `strings.HasPrefix(importPath, options.goRoot)`;
    on Windows a `C:/Users/.../go1.23.1/src/unicode/utf8` argument fails that prefix test against the
    backslash form `go env` returns, so the walk-up branch runs instead and finds **`$GOROOT/src/go.mod`,
    which declares `module std`**. Every file is then emitted into `go.std.unicode` rather than
    `go.unicode`, the conversion **exits reporting success**, and the damage surfaces as
    `error CS0117: 'utf8_package' does not contain a definition for …` in the CONSUMER packages
    (`strings`, `syscall/windows`) — pointing away from the cause and reading exactly like a converter
    regression that dropped public members. Same family as the `-go2cspath` empty-`<ImportedTypeAliases>`
    trap below: **a path the converter half-recognizes is worse than one it rejects.** Native paths convert
    clean first time. (The durable fix LANDED 2026-08-28, `433e9e4e0`: `isPathUnder` +
    `checkGoRootSpelling` + 3 guard tests — the loader-side comparison is path-normalized now. ⚠ The
    PROJECT-IDENTITY side has a measured open residual: `std.<pkg>`-named csproj artifacts dated AFTER
    the fix, with all sources namespace-correct — 2 csproj carrying `RootNamespace=go.std` while 13
    `.cs` declare `namespace go;` — from a run that exited reporting success. Mechanism unestablished,
    G owns the root-cause; do not re-diagnose the loader side, it is fixed and guarded.)
    ⚠ **It bites through the ENVIRONMENT just as readily as through an argument, and the Bash tool is
    where that happens** (paid again 2026-08-26). `run-validated-sweep.ps1` and the `-tests` pipeline
    read `GOROOT` from the environment, so `export GOROOT="C:/Users/.../sdk/go1.23.12"` — the
    forward-slash spelling a Bash-side lane naturally types, and which `go` itself accepts — routes the
    whole emission into `namespace go.std.*` exactly as the argument form does. **The visible tell is the
    project NAME**: the run writes `std.<pkg>.csproj` / `std.<pkg>.tests.csproj` beside the committed
    `<pkg>.csproj`, and the failure surfaces as CS0246 on a generated adapter type in a CONSUMER file
    (`writer.cs: 'sparseFileWriterжWriter' could not be found`) — which reads like a witness/generator
    regression and invites a hunt in the wrong package. It also survives an A/B: running the SAME sweep on
    a baseline converter reproduces it identically, so "it fails at master too" is NOT evidence the tree is
    at fault when the environment is the variable. Check for `std.*` artifacts before believing any such
    diagnosis, and set `GOROOT` from `go env GOROOT` verbatim (single-quoted in Bash so the backslashes
    survive).
  - `-test-action convert|build|run|compare|all` (default `convert`) — `convert`/`all` convert-and-hook
    (production sources then tests); `build`/`run`/`compare` act on EXISTING digest-validated artifacts
    without reconverting; `compare` (and `all`) diffs the C# host's terminal results vs `go test -json -count=1`.
  - `-test-timeout <dur>` — the **package deadline** for a converted-test action (build/run/compare);
    Go duration syntax, default `2m`, must be > 0. For `run`/`compare` it is handed to **both** sides
    (`go test -timeout` and the converted host's own `-timeout`) so they agree, and the child process
    is killed one minute later purely as a safety net. Before that threading each side fell back to
    its OWN 10-minute default, so **no** value of the flag could let a slower-than-Go suite finish —
    `hash/maphash` self-terminated at exactly 600 s under `-test-timeout 40m` and reported its
    still-running `TestSmhasherAvalanche` as an empty verdict that reads like a real failure. A suite
    whose C# run legitimately exceeds 10 min needs an explicit value (maphash: `-test-timeout 30m`,
    ~15 min in C# vs 7.6 s in Go — a performance gap, not a correctness one).
    ⚠ **A REBUILT CONVERTER makes `-test-action build` exit 1 with ZERO compile errors** (measured
    2026-09-06) — a false red that reads exactly like the route-#7 gate it was being run as. The tail
    states it outright (`test manifest is stale: input digest changed (run -tests -test-action convert)`)
    and **the error-code histogram is EMPTY, which is the tell: a build failure carrying no
    `error (CS|MSB|NETSDK)[0-9]+` line is not a build failure.** Since `build`/`run`/`compare` act on
    existing digest-validated artifacts, any gate that rebuilds the converter first must run **convert
    THEN build**; both `reflect` and `internal/reflectlite` went green on the corrected invocation.
    The general form, from the same day: **a build reporting N errors whose codes are ALL I/O
    (MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104, every message "not enough space on the disk") is ONE
    environmental failure wearing N codes, not N defects** — read the code histogram before believing a
    count.
    ⚠ **The 2m default is FIVE TIMES SMALLER than the sweep's, so a hand-invoked `-tests` run fails
    where `run-validated-sweep.ps1` passes — on nothing but which default applied** (measured
    2026-08-24: `bytes` reported `Go="pass" C#=""` on 38 tests with ZERO reported, which is the exact
    signature the orphaned-`dotnet run` file lock produces and reads as total conversion failure; the
    sweep's `-TestTimeout` default is `10m`, and at that value the same tree validated 82/82, exit 0).
    **The tell is the SHAPE of the empty set, and it generalizes to any mass-empty comparison:** a
    contiguous **alphabetical tail** is a run that died partway (deadline or crash) because the host
    reports in sorted order; **scattered** empties are genuine divergence; **ALL** empty is the
    documented file-lock case. Check the ordering before believing the diagnosis — and pass an
    explicit `-test-timeout 10m` on any hand-invoked row so the default is never the variable.
    ⚠ **Refinement measured 2026-08-29 (`net`), and it is the one exception to "scattered =
    divergence": SCATTERED empties that EXACTLY EQUAL the package's `t.Parallel()` test set are ONE
    serial-phase death, not divergence.** The host reports in TWO phases — the serial tests first,
    then the parallel batch — so a single deadlock in the serial phase leaves a contiguous tail there
    AND parks the entire parallel batch unreported, and the union of the two reads as scattered
    because the parallel names interleave alphabetically with the serial ones. `net`'s 43-name
    "deadline family" was exactly this: one deadlock seen from two phases, and the arithmetic closed
    to the verdict once the TransmitFile seam landed. **Compare the empty set against the parallel
    set before reading scattered as genuine divergence** — a set equality is one grep, and it is the
    difference between one root and 43 phantom findings.
    ⚠ **A PREDICATE OVER TEST BODIES MUST FOLLOW HELPER DELEGATION, OR IT MANUFACTURES DIVERGENCES OUT
    OF PARKED TESTS.** A row reported 799 absent verdicts whose set was NOT a contiguous prefix but
    carried 20 holes inside its span; **all 20 were `t.Parallel()` tests and 0 of the package's 67
    parallel tests reported at all** — one serial-phase death parking the whole parallel batch, the
    documented two-phase shape (2026-09-07). ⚠ **The first classification said 18 of 20**: two
    "exceptions" delegate to a helper that calls `t.Parallel()` one frame down, which the regex did not
    follow. Set equality against the parallel set is the discriminator, and the predicate that computes
    that set follows helpers.
    ⚠ **A deadline RAISED with a byte-identical result is a BLOCK, not slowness** (measured 2026-09-02,
    `net` at 40m vs 60m: 501 terminal verdicts / 27 orphans on BOTH runs, both ending at the same test)
    — more budget cannot move a hang, and the stream's last `run` event is what places it. Compare the
    terminal and orphan SETS across the two deadlines before budgeting a longer one: equal sets mean the
    unreported names are one hang plus its serial tail and the parked parallel batch — the re-pricing
    shape (one test worth N verdicts), not N divergences.
    ⚠ **A death attributed in the PARALLEL phase is a budget EXPIRY with an arbitrary name**
    (2026-09-04): a skip-list census names "the last STARTED unfinished row" soundly only in the
    SERIAL phase, because in the parallel phase every row carries its run event when the batch opens,
    the host emits no pause/continue events and several execute at once — the in-flight set at the
    deadline fluctuated 35 → 12 → 33 → 29 → 22 → 20 → 29 → 25 across iterations with rows leaving AND
    joining, so which rows reach a slot inside the budget is SCHEDULING. **The batch's true hang, if
    any, is found only by running each member SOLO under its own deadline.** Companion: **a stub can
    HIDE the frame before it** — rows dying at a hash stub were reinterpreting a slice header one
    frame earlier (a native box over pinned bytes for an unaliasable pair), so measure the EARLIER
    frame on a probe before predicting that a stub's body clears the row.
    ⚠ **And before the tail: check WHICH TREE produced it.** A canary leg returning 232 rows all `C#=""`
    was withdrawn in full without diagnosis, because the tree it came from was not the merge result.
    **Diagnosing a mass-empty from an invalid tree spends real hours on a finding that cannot be
    attributed either way** — and worse, a careless cleanup afterwards (killing the converter alone
    orphans a child holding `runtime.dll`) manufactures a SECOND mass-empty with a different cause and an
    identical shape, whose natural reading is that the first one was real.
    ⚠ **Before ANY shape analysis, read the results-file TAIL — a deadline kill states itself
    outright** (added 2026-08-29 after the third instance in one week): the C# host's
    `go2cs_test_results.json` ends with an explicit
    `{"test":"","action":"timeout","output":"package timeout after <hh:mm:ss>"}` event when the
    package deadline killed it, so the mass-empty diagnosis is a one-line read, not an inference.
    The three lanes that paid it — `bytes` at the 2m default, `sync/atomic` at a lane's own 30m,
    `net/http` at 25m (213 empties published as "divergences across 87 parents" before the tail
    was read) — each had the explicit event sitting in the file the whole time. The shape
    heuristics above remain for the cases the tail cannot settle (a crash leaves no timeout
    event), but the tail is checked FIRST and quoted in any census that reports empty verdicts.
    ⚠ **That tail is a SECOND artifact, and this file named the wrong one until 2026-09-02**
    (corrected against the converter source, not against habit): the package deadline is reported by
    `TestHost` into the host's own `--result` file — `go2cs_test_results.json` — and that event
    carries `"test":""`, so it never reaches the comparison record's per-test maps at all. The
    comparison record is the FLAT `<pkg>/go2cs_test_comparison.json` (`writeComparisonRecord`,
    `testConversion.go`); there is no `go2cs_test_comparison/` DIRECTORY anywhere in the pipeline, and
    `src/core/.gitignore` lists the three files under their real names. Two artifacts, two questions:
    the record answers WHICH verdicts diverged, the results file answers WHETHER the run was killed.
    ⚠ Not every crash is tail-silent: a **module-init** death states itself there too — a
    `NotImplementedException` thrown from the host's static constructor is written into the tail
    verbatim (2026-09-01) — so a mass-empty on a flavor with no run layer is the same one-line
    read, not an inference. ⚠ A second signature for the same door (2026-09-05): 389 rows reading
    `Go=pass / C#=""` whose INNERMOST exception is a `TypeInitializationException` on a per-GOOS
    package — `syscall/windows` loading `kernel32` on a Linux host — is ONE module-init death, i.e.
    the re-pricing shape, not 389 divergences.
    ⚠ **On a flavour's FIRST CONTACT the INIT door is billed separately and FIRST** (2026-09-04): a
    build-tagged `_test.go` whose `init()` reaches a throwing stub takes the host down in the test
    package's STATIC CONSTRUCTOR and shadows EVERY row (436 of 436), the record reading
    conversion-blocked with N Go entries and 0 C# while the classifier's host-crash-at-init
    short-circuit fires. The census probes PAST that door with an ITEMISED, unbanked patch on the
    emitted files before counting anything, then counts BY DOOR — build / crash+unreached / stub /
    oracle / divergence — with unreached shadows on their own line, never as divergences. (A
    prediction can miss in the SHARPER direction — "the first alphabetical half" missing BEFORE the
    first test — and is scored as such.)
    ⚠ And the tail read has its own false-empty: the event can be carried as an ESCAPED JSON
    string, so a substring count of `"action":"timeout"` returned **0** on a record whose tail states
    the kill (2026-09-02) — match the escaped form too, or parse the field.
    ⚠ A new tail-stated member (2026-09-02, the `chanDir` arm): 388 divergences, every verdict `C#=""`,
    stream 0/0/0 — reading like a corpus-wide regression from the lane's own cut — with the tail saying
    `exit status 0xc0000142` (STATUS_DLL_INIT_FAILED), a TORN `bin`/publish tree from an interrupted
    run. Delete the publish dir and re-run; `0xc0000142` in the tail is a cleanup, never a finding.
    ⚠ **The tail rule presumes the results file is the RUN'S OWN — verify freshness before
    reading it** (measured 2026-08-29, the gated-census lane): a host invoked **DIRECTLY** with
    `--run` did not rewrite `go2cs_test_results.json`/`.xml` (four-way A/B: order- and
    exit-code-independent; `WriteResults` has no filter guard and the arg parse advances, so the
    mechanism is UNROOTED — do not assert one), while the comparison beside them was written
    fresh. **Scope NARROWED same day by the same lane's own re-test: the `-test-action compare`
    PIPELINE path with a filter DOES write results.json fresh** — the suppression reproduces
    only on direct host invocation, and which half of that difference is load-bearing is
    unmeasured (the routed chip re-measures both paths before anyone asserts a mechanism). The
    durable half of the rule stands regardless: a stale results.json next to a fresh comparison
    is NOT a deadline kill; a gated/filtered census gates on the CAPTURED STREAM; and the cheap
    check is the results file's timestamp against the comparison's.
    ⚠ **Three record-file rules, all measured 2026-09-02.** (1) A **gated** (`-test-filter`) run
    REWRITES the package's comparison record — a harvest read `runtime/debug` as bankable off a
    filtered control's record, the only tell being 9 go entries where the full run had 10 — so after
    any gated diagnostic that record is poisoned for banking until an UNGATED run overwrites it.
    ⚠ **This clause said "with nothing marking it gated" until 2026-09-06, and that half is now FALSE:
    a gated record is SELF-MARKING.** The record carries `testFilter` with the filter expression as its
    value (`commandLineOptions.go:61`), guarded BOTH ways by `filterZeroMatch_test.go` — `:149`/`:176`
    require the key on a filtered run, `:163` requires its ABSENCE on an unfiltered one, and `:195`
    records why the key is `testFilter` rather than `gated`. **The ritual above stands unchanged on its
    own merits — a gated record is still not bank-eligible — but "you cannot tell" has become "you can
    tell, and here is the field", so the detection is one read rather than an inference from entry
    counts.** Found by a lane reading a record it had just written, confirmed independently from the
    converter source, and verified at master before this edit: the standing example of why a doc
    describes what was true when it was written and only somebody with a record open ever re-checks it. (2) A paired before/after measurement needs two FILES, not two runs: the
    record is git-ignored, so a branch restore cannot bring the "after" back and the baseline overwrites
    it in place — the diff then compares a file with itself and reads "zero moved"; copy each side's
    `results.json` to a distinct path first. (3) `git checkout HEAD -- src/core` + `git clean -fd` clears
    NONE of the pipeline's git-ignored state (`bin/`, `obj/`, the manifest, the comparison and results
    files), so a "restored" tree is WARM and a filtered run's record travels into the next one: delete
    the record files after every sweep, and state cold-vs-warm when comparing two runs.
    ⚠ **Two more, 2026-09-02.** (4) A gate PRESERVES a failed row's comparison record to a distinct
    path BEFORE any restore or cleanup — a union battery deleted the records after a `net/http` sweep
    FAILED, discarding the only evidence of which rows diverged; deletion is for hygiene, never for
    evidence. (5) `run-validated-sweep.ps1` walks the ROSTER, so `-Filter <pkg> -Exact` on an UNBANKED
    row throws "No banked packages matched" while the battery leg wrapping it exits 0 over the hole —
    route #6 in a coordinator instrument: run an unbanked row through the pipeline DIRECTLY, and carry
    every leg's failure in the wrapper's exit code.
    ⚠ **A GOLIB-EMITTED STDERR LINE IS INVISIBLE THROUGH `run-validated-sweep.ps1`** (2026-09-08): a
    probe's control and summary lines appeared in NEITHER the sweep log, NOR its stderr, NOR the
    results file, so a multi-row plan driven through the sweep would have completed GREEN having
    measured nothing — caught only because the runbook's first rule was "read the CONTROL line before
    any number". **Probes drive the published host DIRECTLY**, and the sweep owes a bounded
    host-stderr capture as its own instrument item. Beside it, the reading that made the probe
    trustworthy: two counters DISAGREEING MAXIMALLY — one pegged at zero on every row while the other
    reached millions — was the prediction's own stated clause, not a fault.
    ⚠ **Two more, 2026-09-06.** (6) **A gate whose CLEANUP destroys the artifact it measures reads as a
    clean sweep**: a checkout over the corpus reverted the very disclosure manifest the leg existed to
    exercise, and the log line for it was an innocuous swept-dirt-restored count of one. The repair is
    BOTH halves — EXCLUDE the artifact's path from the cleanup AND assert the artifact is present before
    each leg — so a future silent revert fails loudly. (7) Rule (5)'s substitute has a standing now:
    **the converter pipeline is not merely what a roster-walking sweep is replaced BY on an unrostered
    row, it is the STRONGER instrument** — it is what READS the manifest, and it hard-errors on any entry
    naming a test that records a MATCHING verdict, so a clean pipeline run with the entries present is
    itself the discriminating check. Record that the sweep wrapper was ruled and the pipeline
    substituted, so the next reader sees a substitution rather than an unexplained difference.
    ⚠ **A `-clp:ErrorsOnly` BUILD LOG CANNOT CORROBORATE AN ASSEMBLY COUNT — the flag suppresses the
    very lines the grep counts.** A stdlib leg stamped `exit=0 CS=0 MSB=0 asmLines=0` and the zero was
    the INSTRUMENT, not the build (2026-09-07). **Count artifacts on DISK** (`find … -newermt
    <pre-build stamp>`) **and count them BEFORE the next iteration's purge** — the same chain's
    `clean-bin` destroyed the windows assemblies ~28 s after the leg ended, so a later census reads 0
    for the purge and not for the build. Second instance in one session of a gate whose CLEANUP
    destroys the artifact it measures; the exit code remained the sound primary verdict in both.
    ⚠ **How a preservation leg is WRITTEN — five mechanics (2026-09-04).** It keys on the row's **EXIT
    CODE, never on the sweep's printed word**: the exit is non-zero for every non-pass shape the sweep
    knows AND for every way a row can go NOT MEASURED (an unbanked filter, a toolchain refusal, a
    preflight abort), and the row whose record you most need is often the one that never RAN. Beside
    it: ONE definition of the preserved-record NAMING that every leg dot-sources (a second copy of the
    string is the thing that drifts, and a leg that cannot find its helper aborts loudly); a train
    LABEL derived from the running script's own basename, never written out, because a train script is
    a copy of the previous one and a hand-written label survives the copy; a RED control neutered by
    removing the SOURCE rather than by a switch in the production path (a gate with a lie-lever in it
    is one more thing that can be left on); and the property that decides the class is ORDERING —
    proven by a LIVE arm that plants records, runs the real leg, and reads the preserved copy PRESENT
    and the worktree copies GONE after it, which is the exact property a battery lost when its hygiene
    delete ran first. ⚠ The preserved-record NAMESPACE is load-bearing in turn: a moved-set instrument
    that takes the NEWEST preserved record as "the previous run" reads a control's synthetic one-row
    record as the next train's baseline and prints a whole-suite FIXED/BROKEN set that reads exactly
    like a catastrophic regression — so a control writing into that namespace REMOVES its own artifact
    and ASSERTS the removal, and the newest record is verified by hand to be the real prior run before
    the next battery.
    ⚠ **And what that preserved record answers, which the log cannot** (2026-09-02): a crashed host's
    EXIT CODE is nowhere in the sweep log's printed FAIL block (the stream's last three JSON events) —
    it is inside the comparison record's oracle-side error text (`child error exit status 0xc0000005`,
    with the child's stderr quoted). Two neighbours: the `-tests` pipeline is **SILENT on success**
    (nothing on stdout or stderr, exit 0), so a run's evidence is its ARTIFACTS — the `*_test.cs`, host
    and csproj files — never its exit code; and a diagnostic patch applied to a BANK host's tree is
    restored, and its records deleted, before anything banks from that host.
    ⚠ **And the filtered-status trap has a SEARCH costume** (2026-09-02): `find … | head` filled ten
    lines with unrelated paths and the truncated view was read as the ABSENCE of a preserved record
    that was sitting there. An unfiltered enumeration answers "is it there"; a head-limited one
    answers a different question — the same split as filtered vs unfiltered `git status --porcelain`.
    ⚠ **"ABSENT FROM SOURCE" AND "ABSENT FROM THIS PATH" ARE DIFFERENT CLAIMS, AND THE WEAKER PHRASING
    IS FALSIFIABLE IN ONE COMMAND — taking the conclusion down with it.** A lane reported a helper as
    *"in EIGHT DOCS and ZERO source files"*; it is in **176 source files**, including the very package
    under discussion. What they had actually measured, and what carried their argument, was that it is
    absent from one specific STORAGE PATH — a sharper and true claim (2026-09-07). **State the scope you
    measured, not the scope that sounds stronger**; a correct conclusion resting on an overstated premise
    is discarded by the first person who greps. ⚠ **And a VERIFICATION SCOPED BY THE CLAIM IT IS
    CHECKING IS NOT INDEPENDENT — it can only CONFIRM, never refute, because the claim chose where to
    look.** The coordinator searched the two files that framing implied, found nothing, and endorsed the
    conclusion as "the sharper claim" — while the disputed symbol existed at a third path and the
    original record had been correct all along. **Correcting a claim's premise and adopting its
    conclusion in the same breath reads like scrutiny and is not**: run the UNFILTERED search over the
    whole tree first. ⚠ The originating trap, for completeness: `git grep -ln <x> | head -8` returned
    eight DOC paths and was reported as "zero source files" — **`| head` is a silent WHERE clause** — and
    the lane had quoted that exact rule to the fleet twice the same evening before walking into it.
    **Knowing a trap by name does not immunise you against it; only the unfiltered command does.**
    ⚠ **A FAILED `-tests` BUILD leaves the PREVIOUS comparison record in place** — the family's
    nastiest member, paid three times by one lane (2026-08/09). The pipeline rewrites
    `go2cs_test_comparison.json` only when a run completes, so a fix whose build DIED
    (e.g. a hand-own registered under a bare name where Go declares a method — key `"Type.method"`
    — displacing nothing and duplicating into CS0111) re-reads the OLD record and reports the OLD
    failures: it reads exactly like "the fix does not work", or worse, like a stable count. Before
    believing any post-fix count, verify the build the record claims actually succeeded and the
    record is newer than the edit; for the registry case, checking the placeholder was actually
    emitted is the cheap tell. The record is only the verdict when the run that wrote it completed.
    ⚠ **THREE host-death signatures, and the third is MARKERLESS** (measured 2026-09-03): (1) a
    goroutine panic writes `died on an unrecovered panic in a goroutine`; (2) a package deadline
    writes `"action":"timeout"` into the RESULTS file — **not the log**, so a per-slice kill check
    that greps the log reads a deadline as an unexplained short count; (3) a **.NET exception from an
    unimplemented linkname stub thrown inside `Goroutine.Run` stops the results stream mid-test with
    NO event at all** — the mass-empty family's silent member, one stub costing every test after it.
    (The `runtime.Stack(all)` host-killer is a FAMILY of six reached through helpers referenced as
    TABLE VALUES, invisible to a call-graph grep; a derivation with a positive control — one member
    watched killing a slice — is what made the six trustworthy after two confident wrong sets.)
    ⚠ **The host-fatal class is crash OR deadline-consuming HANG** (2026-09-05): both lose every test
    after the member in its phase, so a hang belongs in the same skip list as a crash — but **a
    widening of a RULED class is asked, not assumed**, and such an entry states a fact about TODAY's
    host and carries its own retirement trigger. Two companions from the same read: a package priced
    "mostly stub" from its NAMES read mostly MATCHED once run (120 against 35) — the
    phantom-divergence shape, sized off an artifact instead of a run; and **thirteen rows can be ONE
    root** when the package's own ordering leaks a flag (`StartCPUProfile` sets `cpu.profiling`
    before the throw), proven by skipping ONE test and watching exactly one row move.
    ⚠ **A REPAIR THAT UNLOCKS HUNDREDS OF TESTS BY SUPPRESSING A FAITHFUL BEHAVIOUR IS A FALSE GREEN,
    and it will look like the biggest win of the campaign.** `runtime`'s host dies because an unwaited
    `testenv.CommandContext` goroutine calls `t.Logf` after its test returned and the converted testing
    host panics — **correctly, reproducing Go's own guard** (2026-09-07). Stopping that panic unlocks
    the parked parallel batch and ~780 unmeasured tests; it also removes real behaviour, masks the
    actual defect (the unwaited command), and makes every verdict harvested behind a weakened host
    unbankable. The legitimate fix is at the tracer root that made the command fail. **Three repairs
    were priced BEFORE being written that week and the answers were all different — pprof's leak WORSE,
    the crash fix BETTER, this one catastrophically better-LOOKING and worse. None was predictable from
    the bug's shape.**
    ⚠ **A host must write its results file on EVERY exit path, and two 2026-09-03 readings say why.**
    A sweep's printed FAIL block — the stream's last three events plus `oracle-only check: no
    converted-host results file` — read exactly like a dead host while the comparison RECORD held
    1,345 rows both sides, 1,323 passing and three real divergences: the row had reached the END of
    its stream and the acceptance it existed to measure was MET. And an **ALL-PASS stream with exit
    status 1 and no results file** is a third mass-empty member whose cause sat in the record's own
    stderr: `net/http`'s `TestMain` runs Go's goroutine-leak check after the suite, the converted
    `runtime.Stack(all)` rendered the checking goroutine one frame too shallow and through the
    hand-owned host's frame names, none of Go's filter strings matched, the main goroutine counted as
    a leak, and `os.Exit(1)` skipped the results write. **Read the NON-JSON lines of a record's error
    text before naming an exit code**, and note the second rule that falls out: a converted
    `runtime.Stack` must render the innermost caller and the runner's own frames the way Go's filters
    expect, because test suites string-match their own stacks.
    ⚠ **That leak check's ROOT, measured 2026-09-04: a host IDENTITY artifact, not a leak and not a
    missing frame.** Go runs `M.Run` on the MAIN goroutine with the deadline as a timer; the converted
    host parked the registered main goroutine in its wait and ran `TestMain` on a pool thread with no
    identity — so the host's own main goroutine is a FRAMELESS foreign block Go's leak filter cannot
    drop, and a row whose verdicts all agree exits 1. The fix is the running thread ADOPTING the main
    identity for its scope (one static reference, no per-object state), never a re-plumbed deadline.
    Three companions. A guard arm that filters survivors by a SUBSTRING cannot see a frameless block
    and reads green against the defect (route #8, named by its author). A mechanism read on ONE
    platform is not the other platform's until measured there (the Windows records carry BOTH shapes).
    And **a record's frame list is read against Go's OWN call chain before its shape is named** —
    `Stack` never renders its own frame in Go either, so a block beginning at the caller's caller is
    ONE frame short, and of the three mechanisms that produce it a `NoInlining`-at-the-callee remedy
    fixes exactly one: the guard that DISCRIMINATES them runs before the 35-minute arms.
    ⚠ **And a check that NEVER RAN is not a clean check** (2026-09-04, the same `TestMain`): the leak
    check is guarded by `if v == 0 && goroutineLeaked()`, so on a run where two leaves fail it is
    SKIPPED — and a summariser reading "no Too-many-goroutines text" reported the leak check CLEAN, an
    absence of the failure's text summarised as the check's pass. Confirm the check's own evidence
    that it EXECUTED (here, the per-call dumps, which showed the survivor in every one) before banking
    a clean: the false-empty family's summariser member.
    ⚠ **Three record-reading rules from the same week.** A comparison record's **"differing" count
    still COUNTS disclosed rows** — a disclosure is a row that differs by design — so a tail quoted as
    76 was 16 UNDISCLOSED (4.75x inflation on every sizing sentence it appeared in); every tail census
    prints **differing / disclosed / UNDISCLOSED** as three figures. A regression's row set is read
    from the preserved record's own `disclosed` and `errors` ARRAYS before a cause is assigned: two of
    four "regressed" rows were standing disclosures measured four days earlier, and a fix justified by
    them would have rested on a premise the record falsifies. And a **mass-empty member one row wide**:
    a `C#=""` beside a SUBTEST NAME Go never produced (`[][]uint8#01` — `t.Run`'s dedup of two types
    that render to one string) is two sides running DIFFERENT-NAMED subtests, not an empty verdict.
    ⚠ **One HANG as the FIRST test executed after a gate is N phantom empties** (2026-09-04): every
    later name reads `C#=""` for one blocked test, so EXCLUDE it and re-run to recover the table
    before reading any other row as a divergence.
    ⚠ **THE CRASH SHAPE PLACES A ROOT WITHOUT A BISECT, and the two shapes say OPPOSITE things**
    (2026-09-05, a union whose blast radius on banked rows was read entirely from the sweep leg). A
    contiguous alphabetical tail of empty verdicts with **NO results file** is a DEAD PROCESS, and the
    FIRST MISSING NAME is the first test that reaches the defect — three rows died at their first
    HTTP-server test, their first real connection and their first dial while 18 other rows passed on the
    same host the same hour. Its complement: **a CRASHED row and a FAILED row are different evidence
    about ONE defect** — a process that produced its verdicts and WROTE its results file never reached
    the faulting call, so its failure is UPSTREAM of it (one row completed 341 verdicts and lost only a
    test whose Go source dials in a loop, which moved the suspect off the cert-chain candidates
    entirely). Read the crash/no-crash split before pricing a bisect.
    ⚠ **A stub's NAME in the comparison record is not the whole reading when the row's text went to
    fd 2** (2026-09-05): Go's `throw` PRINTS before it calls `fatalthrow`, the host printed
    `fatal error: …` too, and the record lacked it because the results stream captures the TEST's
    output and not the process's stderr. **Read the host's stderr for the row before sizing a body to
    "make the text print"** — the fix may be record-side (carry the tail) — and note that a body
    which continues Go's death path buys fidelity that is NEGATIVE in verdicts.
    ⚠ **That fd-2 reading can be PRESENT but UNADDRESSABLE, which is a different defect from absent**
    (2026-09-05): the whole combined stream sat inside ONE half-megabyte `errors[]` string with a .NET
    stack trace at its tail, so "unfindable" is the honest statement — while two OTHER paths lose it
    outright (a deadline kill's returned-then-dropped output; a forgiven non-zero exit whose error is
    nil'd). The remedy DERIVES a bounded tail from the stream the pipeline already holds — the lines
    that do not decode as test events — rather than splitting descriptors, because a second pipe
    changes the capture and interleaving of the very stream the verdicts are parsed from; the attach
    predicate snapshots the RAW error BEFORE any forgiveness arm; and because a validated row's record
    stays byte-identical, **the banked row is the no-regression control and the guard is the positive
    control**.
    ⚠ **An instrumented `-tests` probe reads ZERO because `-test-action all`/`compare` RE-CONVERTS
    every non-marked corpus file before building** (measured 2026-09-03) — the marker was wiped
    between the edit and the binary, the same mechanism that silently reverted a hand-own prototype
    hours earlier. Instrument in the sequence `convert` → **edit** → `compare`, and `grep -c MARKER
    <file>` immediately AFTER the run is the one-command tell. (The probe's own readings VARYING
    across the population were what made its non-zero answer trustworthy — a probe whose output is
    constant is its own false-empty.) ⚠ And the gated/direct freshness split narrows again: a gated
    PIPELINE run ALSO reproduced the stale `results.json` beside a fresh comparison, so the mechanism
    stays unrooted and **the freshness check — the results file's timestamp against the
    comparison's — is the rule**, not the invocation path.
    ⚠ **A test that sizes its own timeout from `t.Deadline()` converts a DEADLINE into a DURATION**
    (measured 2026-09-03 at 60m and 15m): `internal/poll`'s `TestSplicePipePool` consumes 0.9 × T + 6 s
    at ANY package deadline, so the row CANNOT time out and a `$longTimeouts` floor would only make it
    cost 54 minutes instead of 9 — the budget-vs-wall reasoning of the timeout table runs BACKWARDS
    for that class. Its neighbour: **a load-sensitive row is measured SOLO and carries the host's load
    beside its verdict.** `time`'s `TestLongAdjustTimers` gives itself a hard 60-second wall budget
    after 5,000 goroutines (Go's own source says it fails on slow hosts) and failed on TWO trees at
    once while two `time` suites ran CONCURRENTLY on the i7 — two arms failing together under a shared
    load is not an A/B. No two `time` suites share a box; a control REFUSED by the disk preflight is
    reported UNMEASURED, never argued around.
    ⚠ **Three tail-reading rules, 2026-09-06.** **A row's NUMBER can be produced by two mutually
    exclusive mechanisms, and the tail separates them in one command**: 19 of 20 is the same count
    whether the assertion FAILS (the bytes are retained and Go's own `t.Fatal` fires) or the row HANGS
    (the bytes are collected, the finalizer blocks, the collection call never returns and the package
    deadline kills it) — so read the tail BEFORE specifying an instrument, because a hang and a failure
    are not the same question and only one of them is disclosable. **The tail rule is right and its PATH
    is not universal**: for a row whose record is the FLAT root comparison file holding the full stream,
    a tail read scripted against a subdirectory path reports that the run did not get that far — on a run
    that completed perfectly, which is the worst false reading that rule can produce — so a tail read
    looks for BOTH shapes and SAYS WHICH IT FOUND. And **a blocker that moves one deeper with an
    UNCHANGED count is a RESULT**: a host dying on a different symbol in each arm, both fatal, reads as
    an identical verdict count while the wall moved — and what it moved to was the one destination
    neither registry could serve, which turned a declined widening into a measured item on a row's
    critical path. Compare the SYMBOL, not only the count.
  - `-convert-timeout <dur>` — the `-stdlib` driver's cap on ONE package's conversion; Go duration
    syntax, default **10m**, must be > 0 (`log.Fatal` otherwise). It is a **safety net against a hung
    conversion, never a performance assumption**: the value has to clear the slowest legitimate
    package on the slowest legitimate host, because a killed-but-healthy conversion is reported as a
    FAILED package — named in the log, counted in the summary, listed in `failed_packages.txt` — and
    reads exactly like a converter defect. It was hard-coded at 10m until 2026-09-02, when concurrent
    lane load on the i7 class pushed one package past it mid two-seeded A/B, which would have banked a
    whole package as a spurious emission difference. The fired message names the package, the elapsed
    budget and this flag, so raise it there (`-convert-timeout 90m`) rather than editing a constant —
    and pass the SAME value to both binaries of an A/B, since the cap is part of what a run measures.
  - `-go2cspath <dir>` — runtime/stdlib root and default output root for converted code (default `~/go2cs`;
    env `GO2CSPATH`). `go2cs -recurse <input> <output>` keeps generated code under the explicit output root
    while `$(go2csPath)` references continue to resolve against this runtime root. **It is also the root the
    converter reads each imported package's `package_info.cs` from** to mint the emitted
    `<ImportedTypeAliases>` block, so a stale/missing root used to emit a silently EMPTY block — no warning,
    exit 0 — and the OUTPUT varied with the shell's ambient `GO2CSPATH` (found 2026-08-06). Two protections
    since: **self-location** — any single-package or `-tests` conversion whose configured root is not a go2cs
    root (no `core\golib\golib.csproj`) walks its OUTPUT path's ancestors for one, so a bare
    `go2cs <pkg-dir>` inside a clone resolves against that clone with no flag or env; and a **loud
    once-per-run stderr warning** naming the resolved path and the consequence when none is found
    (deliberately NOT fatal — converting standalone code with no deployed root is legitimate). An explicitly
    configured *working* root always wins.
    ⚠ **Single-package mode emits BESIDE ITS INPUT — `-go2cspath` does NOT redirect its output**
    (measured 2026-08-31: `go2cs -go2cspath <tmp>\src <GOROOT>\src\internal\abi` wrote seventeen
    artifacts into GOROOT and nothing into the temp root, and the byte-identity gate then diffed
    the seeded copy against its own source — IDENTICAL, vacuously, with oracle contamination on
    top). Pass the output dir as the SECOND POSITIONAL for any single-package emission you intend
    to diff. Two tells, both cheap: an "emission" whose mtimes predate the seed's copy is not an
    emission; and a byte-identity green is only believable after its negative control (inject one
    blank line → the gate must go red → the restore must be byte-identical) — a gate diffing a
    seeded copy against its own source is a gate that cannot go red.
    ⚠ **Paid twice more on 2026-09-02, and the REPO-ROOT form is the silent one.** A lane omitting the
    positional wrote 167 `.cs` into a GOROOT — loud, because GOROOT is not supposed to hold `.cs`; the
    same omission with the repo root as cwd left **41 untracked byte-identical copies of
    `src/core/strconv` in the REPOSITORY ROOT for eight hours**, invisible to every `| head`/`| grep`
    status check shaped around expected files. A FILTERED `git status` answers "did my change land";
    only an UNFILTERED `git status --porcelain`, read whole, answers "is the tree clean".
    `-recurse` warns but never self-locates (without a second
    positional its root doubles as the output root, so moving it would move the generated tree);
    `-recurse=nuget` does neither (published package refs need no local root); `-stdlib` does neither (its
    root IS the output root the run itself populates, so an absent `golib` is the normal first-conversion
    state). Every harness that invokes the converter — `check-no-regression.ps1`, `BehavioralRunner`,
    `BehavioralTestBase`, `PerformanceRunner`, `run-validated-sweep.ps1` — now passes an EXPLICIT
    `-go2cspath <repo>\src` computed from its own location, so no gate's verdict can move with the ambient
    variable again.
    ⚠ The same `git diff --name-only <a>..<b> -- '*package_info.cs'` returned four paths from the repo
    root and **NOTHING** from `src/go2cs`, and the empty read was briefly taken as "no metadata debt"
    (2026-09-06). **Anchor the pathspec with `:/`, or run from the root and say which.**
    ⚠ **The not-postable-emission rule has an OUTPUT door as well as an input one** (2026-09-04): a
    scratch OUTPUT root injects ABSOLUTE paths into `GoPositionMap`'s first argument exactly as a
    scratch input does, so a seeded-scratch run's delta is read by counting its line KINDS (twelve
    position-map first arguments, tables byte-identical, nothing else) rather than by "one file
    differs" — and none of it is postable without redaction (see the SECURITY convention). A silent
    exit-0 ZERO-diff run is proven a MEASUREMENT rather than a no-op by the emitted files' mtimes
    against the checkout time, since the pipeline's documented silence on success makes the artifacts
    the only evidence. ⚠ **A marker-PRESERVATION measurement is the one shape that runs IN PLACE**
    (input = output, the harnesses' own invocation): `writePackageInfoFile` preserves a hand-added
    line by reading the EXISTING file at the output path, so a fresh scratch output has no marker to
    preserve and the diff is VACUOUS — run it in a worktree and diff against git HEAD, while the
    output-positional rule above guards the other trap (a gate diffing a seeded copy against its own
    source). Where two ten-second forms could disagree, run BOTH: the disagreement would be the
    finding.
  - `-platforms os/arch` — the ONE target a conversion emits for (default: the host). It also accepts a
    comma-separated **list** (`-platforms windows/amd64,linux/amd64,darwin/amd64`). With `-stdlib` a list
    now performs the multi-platform **EMISSION** (`platformEmit.go`): it converts once per target into a
    seeded staging root (`-platform-stage <dir>`) and MERGES the emissions into the `-go2cspath` corpus as
    layout L3 — shared files flat, platform-varying ones in per-GOOS folders, hand-owns routed to their
    principal's platform set. ~560 s for three targets (measured r51b). `-platform-census` remains the
    READ-ONLY instrument over the same staging (a manifest, no corpus output). A list without `-stdlib`
    is rejected rather than silently converting the first target.
  - `-platform-census <dir>` — the **multi-platform emission census** (increment 1, landed 2026-08-08).
    With `-stdlib` and ≥2 `-platforms` targets it converts once per target into `<dir>\<goos>-<goarch>\src`
    — each staging root SEEDED from `-go2cspath` per the reconvert ritual below, wiped and re-seeded per
    run so the r41 "never convert twice into one root" rule is mechanical rather than remembered — then
    classifies every emitted artifact (shared / variant / partial / exclusive) and writes
    `<dir>\platform-manifest.json`. It writes **nothing** into the corpus: `-go2cspath` is read as the seed
    and never as an output. ⚠ In any multi-target staging comparison, "differs" means NOTHING until
    you know which side was actually WRITTEN: a single-target conversion re-emits only its own
    target's per-GOOS files, so a per-PATH diff across staging roots reports fresh-vs-seeded pairs
    as differences (a confounded census nearly banked 60 false hits, 2026-09-01) — compare only
    paths BOTH conversions write, or classify by write-evidence first.
    ⚠ Its MIRROR, measured 2026-09-02: **IDENTICAL means nothing when the side was not WRITTEN
    either.** A windows-default single-target reconvert reported ZERO diff on an L3 package's
    `linux/` files — the very files another lane had measured, under a linux-target conversion, as
    carrying four missing forced-init hooks. Classify by write-evidence PER TARGET, and measure an L3
    package with the three-target `-platforms` emission rather than the host default.
    Emitted-vs-seeded is decided by a sentinel modification time, not by content,
    because the control target's emission is *supposed* to reproduce the seed byte for byte. The manifest
    carries the marker gate per target (hand-owned files the seed held, and any the run emitted as a plain
    `.cs` — must be zero) so a failed seeding cannot be mistaken for a platform finding.
  - `-goroot` / `-gopath`, `-indent 4`, `-var` (default on),
    `-uco` (channel operators, default on), `-comments`, `-cgo`, `-tree`, `-csproj <tmpl>`, `-debug`.
  - `-license <SPDX expression>` — overrides the emitted NuGet license METADATA
    (`PackageLicenseExpression`) for every emitted project; it never relicenses input source.
    Without it a library packs a local `LICENSE` when one exists, a converted stdlib package packs
    `src/core/LICENSE` by relative path, and a `-recurse` dependency module packs its own module's
    license file, copied verbatim to the converted module root once per module (`licensing.go`,
    2026-09-11). `-provenance` — adds a deterministic `// Converted from Go source: "<path>"`
    comment (GOROOT- or module-relative, default OFF, no timestamp). Recognized leading
    copyright/license notices in a Go file are emitted even WITHOUT `-comments`, so a Go-derived
    behavioral fixture's golden begins with the Go notice. Licensing boundaries: `LICENSING.md`;
    the converter is AGPL-3.0-only with the output exception in `src/go2cs/LICENSE-EXCEPTION`,
    and every converter `.go` header carries the AGPL section 7 notice line pointing at it
    (guarded by `TestLicensingConverterHeaders`).
  - Single project/file: `go2cs package_dir` or `go2cs example.go [out.cs]`.
  - **Always pass `-comments` when converting the Go stdlib.** It defaults **off**, but the converted C#
    is a derivative work: the per-file `// Copyright … The Go Authors … BSD-style license` header **must be
    preserved** (license requirement), and the Go doc-comments are what make the output readable. Without
    it the header and all comments are stripped. (Behavioral-test goldens were captured *without* comments,
    so don't flip the default — pass the flag on stdlib `-stdlib` runs.)
  ⚠ **And a single-target number is not "the" population.** Re-derived from the generator's own output
  on all three targets, an unimplemented-stub population read **windows 232 / linux 256 / darwin 458 —
  union 510, intersection 214** (2026-09-06). A single-target run would have reported one of those
  three as the population and hidden a spread of nearly 300. **The gap between intersection and union
  is the finding**; a total conceals which members are platform-specific and which are universal.
- **Converted C# projects:** standard `dotnet build` (target **net10.0**, C# latest). Each converted
  `.csproj` references `golib`, the `go2cs-gen` analyzer, and the stdlib packages it imports. The
  `$(go2csPath)` MSBuild property resolves to `$(SolutionDir)` in Debug builds (so refs point at
  `src/core/...`); it is **distinct** from the converter's `-go2cspath` output flag.
- **Behavioral tests** (`src/tests/Behavioral/`): each test references `golib` + the `go2cs-gen` analyzer;
  most also reference `core/fmt` (a few reference `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`/
  `reflect`). Since 2026-08-01 those references bind the **converted** packages, so the suite's 515
  stdout comparisons against `go run` are also the broadest running validation the converted `fmt` gets —
  its closure is 57 projects (cold ~48 s, warm ~4 s).
  The `BehavioralTests` MSTest runner has these phases: `TranspileTests`, `CompileTests`,
  `OutputComparisonTests` (runs Go vs C#, compares stdout), `TargetComparisonTests` (byte-compares the
  transpiled `.cs` against a `.cs.target` golden).

### Test-harness mechanics (important when changing the converter)
- **`dotnet build` does NOT run the converter** — it only compiles committed C#. A clean build leaves the
  tree clean. **Running the tests re-runs the converter:** `BehavioralTestBase` rebuilds `go2cs.exe` via
  `go build` whenever any converter `*.go` is newer than the binary, then re-transpiles. So after a
  converter change, running the suite regenerates the behavioral `.cs` from current source (and may show
  them as modified in git — that's expected).
- **A hand-invoked `-stdlib` or `-tests` run REFUSES a stale binary — route #1 closed from inside the
  converter, where those two paths have no caller to instrument.** `go2cs` compares its own executable's
  mtime against every build input in the source tree beside it (`converterStaleness.go`, over the set
  `ConverterBuildInputs.cs` defines) and, when any is newer, ENUMERATES the extent: the count, the ten
  newest paths (all of them at ten or fewer), and which are emission-affecting — everything except a
  `_test.go`, which `go build` excludes from the binary. For those two drivers, whose output is banked
  or measured, it then exits non-zero; **`-allow-stale-converter`** proceeds deliberately and is what an
  A/B against a PRESERVED binary passes, so a stale run says so in its own command line. Every other
  shape — a single file or package, `-recurse` — keeps the warning and runs, because that is the
  scratch-probe loop and a pinned binary there is ordinary. Two limits to know: it is silent when no
  converter source tree sits beside the executable (a deployed binary), and it is blind to a TOOLCHAIN
  hop, which stays route #4's embedded-stamp comparison in the harness predicate.
  ⚠ **What that refusal replaced, and why ENUMERATING the extent is the load-bearing half**
  (measured 2026-09-03, the run that motivated the cut): a converter built before a train landing
  emitted the PREVIOUS converter's output while reporting success, and the old ADVISORY named ONE
  file where `find -newer` gave **six** — four of them paths that affect every package's emission —
  so "that entry only touches syscall" was a confident wrong justification one line from banking a
  right number off an invalid measurement. **A staleness warning names a SYMPTOM, not the extent.**
  Four holes are recorded in the cut itself: a plain `cp` without `-p` stamps a fresh mtime; mtime is
  not content, so a branch switch can fire the refusal; there is no toolchain comparison (route #4
  stays the harness's); and it is silent for a relocated binary. One method note from the same cut —
  the Go-side predicate is byte-for-byte the C# `ConverterBuildInputs` one, and a THIRD derivation of
  the embed-directive rule had drifted (no whitespace guard): **three derivations of one predicate are
  kept in lockstep, or one of them under-reports.**
- **FALSE-GREEN route #2 — stale OUTPUT (fixed 2026-07-20).** Distinct from the stale-`go2cs.exe` trap
  (route #1, where an un-rebuilt binary runs old logic): here the exe IS current but the runners *skip
  transpiling* and validate the **previous** converter's `.cs`. All three of `BehavioralRunner.UpToDate`,
  `PerformanceRunner.UpToDate`, and MSTest `BehavioralTestBase.TranspileProject` short-circuited on a
  `.cs`-newer-than-`.go` check alone. Converter work is exactly the case where the `.go` files *don't*
  change, so every project stayed "up to date", transpile was skipped for all of them, and Target/Output
  then compared the old converter's output against goldens that same converter had generated — everything
  matched and the suite printed **PASS**. A guard test "validated" that way guards nothing. All three now
  also require the `.cs` to be newer than **`go2cs.exe`**, so any converter rebuild invalidates the whole
  corpus. Verified by neutering a real converter fix (`lhsReusedInLaterRhs`) and rebuilding: the old
  runner reported PASS, the fixed runner reports `FAIL [Target,Output]` with no manual touch.
  ⚠ **Route #2 has a door NO BINARY opens — a `git checkout --` of the behavioral tree** (measured
  2026-09-03). The restore stamps every `.cs` with a fresh mtime NEWER than `go2cs.exe`, so the
  up-to-date predicate (`.cs` newer than `.go` *and* than the binary) is satisfied by HEAD's OWN
  committed emission: transpile is skipped, `--update-targets` copies that `.cs` over its golden as a
  no-op and reports `Updated`, and the four-phase run behind it validates the OLD emission and
  passes — a golden re-baseline that re-baselined nothing. The tell is arithmetic (an EMPTY numstat
  where CNR had just read `1 1`); the remedy is CNR's own — rebuild the converter first so every
  project is stale again (a re-baseline path that transpiles unconditionally is queued). **A
  re-baseline is believed only after its diff is non-empty and its golden byte-compares against the
  on-disk emission.**
  ⚠ **Corrected 2026-09-04 on the MECHANISM, the door itself standing:** a WHOLE-TREE `git checkout`
  writes the `.cs` and the `.go` within the same instant, so under a strict mtime comparison it does
  NOT reliably leave the up-to-date relation over foreign content — what does is a `.cs`-ONLY restore,
  a `Copy-Item`, or an editor save. The comments in the code say the MEASURED shape, not the plausible
  one; the remedy (transpile unconditionally) is unchanged either way, since it does not depend on
  which restore stamped what.
- **`check-no-regression.ps1` re-transpiles UNCONDITIONALLY** (it has no `UpToDate` equivalent), which is
  why CNR was immune to both false-green routes and remains the authoritative drift instrument for
  converter changes. Preserve that asymmetry: never add an up-to-date skip to CNR.
- **A new converter `.go` file must be registered in `src/go2cs/go2cs-src.projitems`** — the VS
  shared-project item list `go2cs-src.shproj` imports (and that shproj is a member of `go2cs.slnx`).
  Nothing *builds* from it (`go build` walks the directory), so a missing entry is invisible at the
  command line and only bites in Visual Studio, where the unlisted source is absent from Solution
  Explorer. It had drifted silently until 2026-08-06. **`projitemsIntegrity_test.go`** now gates it
  both ways — every `*.go` on disk (including `internal\*`) is registered, every registered path
  exists — under the plain `go test ./...` run from `src/go2cs`, so no new harness and nothing to
  remember; a failure prints the exact `<None Include=… />` line and the entry it goes after. (Same
  invariant `tests/Behavioral/check-solution-integrity.ps1` applies to `go2cs.slnx`.) The file is
  UTF-8 **with BOM** and its line endings are uniform — a third guard holds both, so edit it in place
  or via `[System.IO.File]::ReadAllText/WriteAllText`, never PS 5.1 `Get-Content`/`Out-File`.
  ⚠ **A behavioral guard reading a child's state through an interpreter the harness does not install
  fails in BOTH directions.** Measured before asserting (2026-09-06): exactly three behavioral projects
  call `exec.Command` and all three launch `os.Args[0]` or `exec.LookPath`; **ZERO reference `python`,
  and no workflow installs it.** Such a guard is a **false-RED generator on the host that lacks the
  tool and a false-GREEN on the host where it silently errors into the same string on both sides** —
  and the second is the dangerous one, because it is two sides agreeing BECAUSE NEITHER RAN.
  **Three more census/launch traps, each paid repeatedly (2026-08-17):** a DEFAULT ripgrep honors
  `src/core/.gitignore` and under-counts the marker census by one — census with `git grep` or a raw
  filesystem walk, never bare `rg` over `src/core`. A census over CONVERTED C# never keys on a
  type's spelled NAME: the converter deliberately mints aliases (`_type`, `Δio`, `abiꓸFuncType`,
  the whole `ꓸ` family), so a spelling-matched scan silently under-reports by every alias in scope
  — measured 2026-08-31 at ~1.9x, when 59 of 117 `Reinterpret` descriptor sites were spelled
  `_type` (runtime's `global using` alias for `abi.Type`) and were invisible to a name-keyed census
  however many times it was re-run. Resolve what the name denotes, or enumerate the aliases first
  and search for all of them. `Start-Process -ArgumentList` in ARRAY form does
  not quote a path containing a space (`C:\Program Files\Go` dies as `Failed to access input file
  path "C:\Program"`, reading exactly like a missing GOROOT — three lanes paid this); pass ONE
  pre-quoted argument string. And `MSB4166 "child node exited prematurely"` is a BUILD-INFRASTRUCTURE
  crash, not a package root — a `-tests` batch measured a package as a hard build failure (eleven
  MSB4166s, zero CS diagnostics) that reached its real 9-of-10 verdict in 45 s once
  `MSBUILDDISABLENODEREUSE=1` was set; set it for any back-to-back `-tests` queue before believing a
  diagnostic-free build failure.
  **Five more launch/instrument traps, each paid 2026-09-01/02.** (1) **Git Bash rewrites `cmd /c`
  into `cmd C:\`** — MSYS path conversion eats the `/c` — so the command never runs: `cmd` opens
  interactively, reads EOF, exits **0**. A "runner gate" passed that way with a log holding only the
  cmd banner; the EMPTY grep for its verdict line, not the exit code, is what caught it. Drive
  `cmd /c` from PowerShell (a `.ps1` launched by `powershell -File`) or set `MSYS_NO_PATHCONV=1`,
  and **grep a gate's log for its verdict line before believing "exit 0"** — route #6's shape,
  hand-typed — but read the NINTH trap below before reaching for that variable.
  (2) **`robocopy` from Git Bash with forward-slash paths copies NOTHING and exits 1**,
  which is robocopy's SUCCESS code — a silent no-op that reads as a completed stage. (3) **The
  behavioral runner shells out to a BARE `dotnet`**, so the SDK must be on PATH: `DOTNET_ROOT`
  alone does not prevent NETSDK1045. (4) **Never locate a comparison binary by a recursive glob's
  first hit** — `Get-ChildItem -Recurse -Filter <name>.exe | Select -First 1` returned
  `bin\Release\Go\…` ahead of `bin\Release\net10.0\…` (G sorts before n), so a byte-identical
  289/289 "C# matches Go exactly" reading was ONE binary printed twice, and the runner reporting a
  real gap was right; name the TFM path, and positive-control an output comparison by making the C#
  side differ once. (5) **The LF-anchor trap is not converter-only** — harness C# is CRLF under the
  same `eol=crlf` pin, so an LF-anchored patch to `BehavioralRunner/Program.cs` matches zero times
  and the build that follows reports exit 0 with 0 errors *because the file was never changed*.
  (`strings` also cannot see a .NET UTF-16 literal: use `strings -el`, and check the checker against
  a literal known to be present.)
  **A sixth, 2026-09-02:** a WSL reconfiguration can silently change which USER a lane's automation
  runs as — after a resolver change the default user flipped, the lane's scripts became unreadable, and
  the wrapper EXITED 0 over a permission error in its log: route #6's shape again, a runner that cannot
  reach its own work reporting success. The LOG caught it, not the exit code; `wsl -u root` is the fix.
  **A seventh, 2026-09-02 — the exit code a PIPE throws away, in three costumes in one day.**
  `git push | tail` makes `$?` **tail's** status, so a mailbox tool reported a REJECTED push as success
  and advanced its read anchor to a local-only commit, marking unread posts read; `cmd | head -N`
  masked a toolchain wrapper's abort, which is why its first negative control read 0; and
  `cmd || true; echo "exit: $?"` reports `true`'s status. Capture the real exit BEFORE any pipe (to a
  file when the command must also be read), and make a failing state-advancing tool reset itself and
  exit non-zero — with its rejection path POSITIVE-CONTROLLED, since no normal run exercises it.
  ⚠ **A FOURTH costume, and it needs no pipe at all** (2026-09-05): a `$(...)` substitution INSIDE the
  line that READS `$?` resets it — `echo "end $(date) exit=$?"` reported `exit=0` for a script that had
  exited 2 having deleted nothing. **Capture `rc=$?` as the FIRST statement after the command, then
  format.** Three companions from one bad call the same day, all the same shape: a python patch whose
  anchor predicate was NEVER satisfied raised `StopIteration` and the chain CONTINUED past it because
  the `&&` after the heredoc was missing; a control then invoked a switch that did not exist; and
  `cmd | tail -1; rc=$?` captured `tail`'s zero — after which a post asserted the fix as done. So:
  **gate every step on the previous one's exit**, and **a post claims an instrument change only AFTER
  its control has printed the expected line.**
  **An eighth, 2026-09-03/04 — a CI workflow step is validated TWICE, one layer apart.** A PyYAML
  parse passed a `run:` block whose PowerShell died at PARSE one second into the step on both mac
  legs (`$p:` inside an interpolated string is a scoped-variable reference), so the gate was one layer
  short: **a step is parsed by the INTERPRETER that will run it** — extract every `shell: pwsh` run
  block from the parsed workflow and hand it to the PowerShell parser, positive-controlled on the
  broken step (3 errors before the fix, 0 after). The amendment came the same hour, because that
  parser cannot see argument SPLITTING: a bare `transpile,compile` in PowerShell argument position is
  an ARRAY, so the runner received two words and died one layer past the parse gate. **So: parse it
  with the real interpreter, then RUN it once locally end to end, in whatever flavour the host has,
  with the step's own env block, before the push** — the third dispatch was the first whose step had
  executed anywhere. Same family as the `cmd /c` and `$ErrorActionPreference` traps. ⚠ A related
  reading rule: **a build summary's own predicate is checked against the artifacts' mtimes before
  "0 assemblies written" is believed** — 13,779 in-window assemblies stood behind a wrapper line that
  read zero.
  **A ninth, 2026-09-04 — `MSYS_NO_PATHCONV=1` cuts BOTH ways.** The switch that keeps a `cmd /c` or
  PowerShell argument intact ALSO disables the conversion Windows **git** relies on, so
  `git commit -F /c/path/msg.txt` fails with `could not read log file … No such file or directory`
  while the file verifiably exists — a message that reads as a missing file and is a path-FORM
  failure, diagnosed only when a second attempt failed identically with the file's existence proven
  first. In a shell that exports the variable, spell every path handed to a native Windows tool in
  Windows form (`cygpath -w`), or scope the export to the ONE command that needs it: a fixup that must
  commit and then run a PowerShell instrument is exactly where the two requirements collide.
  ⚠ **A CENSUS TAKEN WHILE THE BUILD IS RUNNING IS A COUNT OF THE BUILD, NOT OF THE PACKAGE.** A
  mid-build stub census read 138 where the settled count was 142 (2026-09-07) — four generator outputs
  had not been written yet. The reading was not wrong about what it SAW; it was wrong about what it was
  MEASURING. **Any census over generated artifacts waits for the producing step to EXIT, and a count
  that disagrees with a later one is a scheduling question before it is a finding.** Beside it: a
  PowerShell `-match` per line captures only the FIRST hit, so a line bearing two stubs counts one — a
  Python `finditer` re-derivation is what settled that number.
  ⚠ It cuts a third way, at `git` itself: **`git -C <drive-letter path>` from Git Bash under
  `MSYS_NO_PATHCONV` can FAIL SILENTLY**, after which the wrapper around it reports "0 dirty" — the
  INSTRUMENT's zero, not the tree's (2026-09-04). **Verify tree state from INSIDE the tree** (`cd`,
  then `git`).
  ⚠ And the same split bites with **no variable set at all: a bash `-f` test and a native tool's
  path ARGUMENT are different namespaces** (2026-09-06). A probe printed `record: present` from bash
  and then died `FileNotFoundError` inside python on the SAME path string, because `/c/...` resolves
  for the shell and not for a native interpreter. The contradiction reads as a race or a vanished
  artifact and is one path spelled for two namespaces: **`cygpath -w` before handing a path to a
  native tool, and read a present-then-absent pair as a namespace split rather than a timing bug.**
  **A tenth, 2026-09-04 — bare `tar` resolves to TWO PROGRAMS on a Windows box depending on PATH
  order**, and only the system one (`System32\tar.exe`) reads a Windows path: MSYS tar reads `C:\…` as
  a REMOTE HOST (`Cannot connect to C: resolve failed`), so a two-seeded A/B's OLD arm never extracted
  and its binary never built. "It worked last time" proves which binary answered last time and nothing
  else. The abort guard — verify the built binary exists at the exact path you will invoke — fired, so
  nothing was measured against an absent arm; but the invocation's `> log 2>&1; echo EXIT=$?; grep`
  reported the GREP's exit over that abort, and the tell that remained was a three-minute wall against
  an expected twenty-five. **Name the tar by path in an instrument, and capture the native exit BEFORE
  anything touches `$?`.**
  **An eleventh, 2026-09-05 — the Bash tool TRUNCATES a long command, and nothing runs.** A command
  past roughly 6 KB is cut MID-HEREDOC, so the shell reports `unexpected EOF` *inside the heredoc* and
  not one statement executes — the silence-not-error family through the tool's own door. **Route long
  text through Write/Edit and keep the Bash call short.** Its backslash companion: a heredoc-fed python
  anchor containing a backslash must BUILD it from `chr(92)`, because the doubled form collapses on the
  way through — the same reason a census grep pattern that ENDS in a backslash is a malformed ERE and
  must be bracketed. (`.Replace` for a literal in PowerShell, too: a `-replace` whose pattern is a lone
  backslash is an invalid regex that errors on every run WITHOUT stopping the script.)
  **A twelfth, 2026-09-06 — a NATIVE binary invoked from the POSIX shell never sees a pattern with a
  LEADING SLASH.** The shell's path conversion rewrites the ARGUMENT before the binary starts, so a
  ripgrep count of a home-directory prefix spelled with its leading slash reads a clean, well-formed
  ZERO against a file that plainly contains it, while the same pattern without the leading slash reads
  1 and the MSYS-side `grep -cF` on the slash-leading form reads 1 (measured both directions, plus the
  control that disabling the conversion then breaks the FILE path too — which is what proves the
  mechanism rather than merely fitting it). **Every security census keyed on a home-directory prefix
  uses the MSYS `grep`, the Grep tool, or a pattern with no leading slash** — the same path-conversion
  family as the shell eating a command interpreter's switch, one argument over, and exactly the
  false-clean a pre-post census exists to prevent.
  ⚠ **TRAP (5)'s FAMILY IN A THIRD COSTUME, AND THE LABEL IT WORE (2026-09-06).** Python's
  `print` to a REDIRECTED file on Windows emits CRLF, so a bash `while IFS=… read` loop's **LAST
  FIELD carries a trailing carriage return** and any path built from it silently fails: a drop-train
  assembler read its seat list that way, every `-F <message-file>` path ended in a CR, `git merge`
  failed on the bad path — and the script reported **CONFLICT**. Two tells, both cheap: the same
  merge run BY HAND succeeds, and the reported conflict lists **NO unmerged paths**. Strip every
  field (`${var%$'\r'}`), or have the writer emit LF explicitly, and `cat -A` the intermediate file
  before believing any loop that reads it — the same family as the LF-anchored patch and the UTF-16
  log. And **a failure LABEL is part of the instrument; a wrong one costs more than no label**: that
  `CONFLICT at <seat>` named a CAUSE that would have been acted on (a conflict resolution nobody
  needed) and inflated a published cost estimate, so a failure branch prints what it OBSERVED
  (`MERGE FAILED — unmerged paths follow; NONE means it was not a conflict`), never what it assumes,
  and **an estimate derived from a broken instrument is withdrawn EXPLICITLY** — said to be
  unverified until a clean run replaces it — rather than quietly re-derived.
  **A thirteenth, 2026-09-07 — a PINNED PATH HIDES A GLOBAL TOOL'S RUNTIME.** With the .NET 10 root
  prepended, `pwsh` — a dotnet global tool needing the .NET 8 runtime — exits 150 on a GOOD and a
  BROKEN file alike, so a workflow-parse arm that unsets only `DOTNET_ROOT` is still dead under the
  pin. A train assembly's own parse control caught it (good=150 / broken=150 against a want of 0 / 1)
  and REFUSED before merging anything; the arm runs with the PRE-PIN PATH captured before the pin
  (`env -u DOTNET_ROOT PATH="$ORIG_PATH" pwsh`), proven good=0 / broken=1 / without-restore=150 in the
  pinned shell. **A control that runs INSIDE the instrument under the instrument's own environment is
  worth more than the same control run beside it.** ⚠ **And the capture itself has a LAUNCH
  dependency**: the next train's assembly was launched from a shell that had ALREADY exported the
  go/dotnet pin, so `ORIG_PATH` captured the PINNED path and the same arm read 150 on both files again
  — the control caught it (NOTHING assembled, ABORT) and the relaunch cost two minutes (2026-09-08
  00:00). **A derived script states its launch shape in its header, and a robust one derives
  `ORIG_PATH` by REMOVING the pin directories rather than by trusting the launch.**
- **FALSE-GREEN route #3 — NESTED sub-library packages were never enumerated (fixed 2026-08-02).** All
  three transpile gates walked `tests\Behavioral\*` **top-level only**, so the 22 sub-library packages
  nested inside a test folder (`IoLike\FsLike`, `VersionedImport\vlib`, `CrossPackageArrayZeroValue\bufpkg`,
  `GoNamespaceShadow\nsshadowlib`, …) were transpiled by **no gate at all**. Two consequences, the second
  the dangerous one: (1) their committed `.cs` froze at whatever converter last touched them by hand — 17
  files across 13 packages had drifted by 2026-08-02, spanning three separate increments (the
  `// <TypeAccessibility>` block, string-literal hoisting, the compound-assign result cast); (2) a
  sub-library's `package_info.cs` is an **INPUT** to its parent's transpile — the parent reads the
  sibling's `[assembly: GoImplement]` records to decide whether to mint a local `ᴠ` value adapter — so a
  regression in that area could not make the parent's golden fail, because the parent kept reading the
  stale-but-plausible records. That silently disarmed the `ForeignValueImplementSuppression`,
  `ValueAdapterDynamicType` and `SamePackageImplementNoWitness` guards. All three gates now walk
  **recursively, DEEPEST-FIRST** (`GoPackageDirs` in `BehavioralRunner`/`BehavioralTestBase`; a recursive
  `Get-ChildItem` + depth sort in CNR) so a sub-library is regenerated before its parent consumes it, and
  `UpToDate` in both runners considers the nested packages too. Enumeration is now **570 packages**
  (545 top-level + 25 nested; it was 543 = 521 + 22 when this note was written), not 521. Note goldens remain top-level-only: nested packages have no
  `.cs.target` (`UpdateTestTargets` is deliberately unchanged), so nested drift is caught by CNR's
  `git status`, while the *cross-package* effect is caught by the parent's golden.
- **FALSE-GREEN route #4 — a TOOLCHAIN hop did not invalidate `go2cs.exe` (CLOSED 2026-08-24, H1.4).** A
  fresh instance of route #1's stale-binary trap that route #1's mitigation does not cover. Every rebuild
  predicate — `BehavioralTestBase`, `BehavioralRunner`, `PerformanceRunner` — rebuilds the converter when
  a converter **`*.go` file** is newer than the binary. Installing a new Go toolchain touches **none** of
  them, so after a hop every predicate still says "up to date" and every gate keeps running a binary that
  embeds the OLD release's `go/parser` + `go/types` front end (`conversionDriver.go` uses
  `packages.LoadAllSyntax`, i.e. the converter's OWN compiled-in type-checker) against the NEW release's
  sources. It does not fail cleanly: the old parser mis-parses or rejects the new constructs and the run
  degrades into the converter's best-effort "did not fully type-check" path — which CNR reports as **NOT
  MEASURED** (good) but the runners do not. **The remedy landed (H1.4, 2026-08-24) and came out smaller
  than planned: nothing needed stamping, because every Go binary ALREADY embeds its toolchain release and
  `go version <exe>` reads it back.** So the whole fix is one compare, in the ONE shared helper all three
  predicates already delegate to since route #5 — `src/tests/ConverterBuildInputs.IsConverterStale` — which
  fails stale-wards (unreadable stamp or unanswerable GOVERSION forces the rebuild) and is guarded by
  `TestConverterStalenessConsultsTheToolchain`. **No explicit `go build` is owed after a toolchain change
  any more; the predicates rebuild on mismatch exactly as on an mtime change.**
  ⚠ **THE TWO-PIN PAIRING IS ENFORCED BY THE MODULE GRAPH, NOT ONLY BY THE SHELL** (2026-09-08): with
  the run environment re-exported to the corpus release and the toolchain rule left on `auto`, Go
  switches ONLY the converter's own build up to the newer directive and leaves every corpus module
  loading at the corpus release — CNR under that shell read every package byte-identical, 0 NOT
  MEASURED, with the converter still stamped at the newer release afterwards, gated by an AFTER-GUARD
  that re-reads the BINARY's release (NOT MEASURED being the alternative to a count nobody can stand
  behind). ⚠ **And the behavioral runner is GREEN BY A DIFFERENT ROUTE than CNR**: its staleness
  predicate reads the toolchain version at the RUNNER's cwd, which has no module file above it and so
  answers the CORPUS release, against the binary's NEWER embedded stamp — so it reads PERMANENTLY
  STALE and rebuilds the converter on EVERY invocation (seconds, the content-addressed cache
  re-linking only), which **fails safe: never a stale binary, never a Transpile skip.** The property
  is cwd-dependent and fails safe both ways; the switch resolves through the module cache, so a cold
  box fetches the newer release there.
- **FALSE-GREEN route #5 — a converter build INPUT that is not a top-level `*.go` file invalidated
  `go2cs.exe` NOWHERE (found 2026-08-21 by the hop-campaign planning read; fixed 2026-08-22).** The
  third instance of route #1's stale-binary trap, and the one with the widest trigger. All three
  rebuild predicates — `BehavioralRunner` (`Program.cs`), MSTest `BehavioralTestBase`,
  `PerformanceRunner` (`Program.cs`) — asked whether any **top-level** `*.go` in `src\go2cs` was newer
  than the binary. The converter is built from more than that, and each omission changes what it
  **emits** while touching no top-level `.go` file at all: (a) the `//go:embed` assets —
  `embeddedTemplates.go` embeds both csproj templates, the `package_info.cs` skeleton, the icons and
  `profiles/*`, and `stdlibMetadata.go` embeds `stdlib-metadata.txt`; (b) the `internal\` packages the
  converter imports (`internal\stdlibmeta` and siblings), which a top-level walk never saw either; (c)
  `go.mod`/`go.sum`. Measured at the fix: **204 top-level `*.go` seen, 224 real inputs — 20 invisible.**
  Edit one and every predicate reports "up to date", the OLD binary keeps running, and every runner gate
  validates the PREVIOUS emission and prints PASS. The edit reads as a no-op, which is
  indistinguishable from "the change was already correct" — and a **.NET migration's TFM stage edits
  exactly those templates and profiles** (`docs/DotNetMigration.md` §5.2), which makes it the step in
  the project most likely to meet this route. Route #4's `runtime.Version()` stamp does not cover it: a
  stamp says nothing about a template's modification time. **Remedy (landed):**
  `src\tests\ConverterBuildInputs.cs` — one definition of the converter's build-input set, LINKED into
  all three projects (the two runners take no assembly dependency, so a shared assembly is not
  available), with the embedded half **DERIVED from the `//go:embed` directives themselves** rather
  than listed, so a directive added tomorrow is covered the day it is written. Two guards under the
  plain converter `go test ./...` (`embeddedAssets_test.go`): the directive **forms** stay inside the
  subset the C# resolver understands, and the three predicates still delegate to the shared helper.
  **`check-no-regression.ps1` was never exposed** — it has no rebuild predicate at all, it runs
  `go build` unconditionally, and `go build`'s cache is content-addressed over embedded assets
  (A/B-verified: editing `csproj-template.xml` changes the linked binary's hash, reverting reproduces
  it byte-for-byte). That is the same asymmetry that made CNR immune to routes #2 and #4 — preserve it.
  ⚠ One caveat on the second guard: cmd/go's test cache **drops files that resolve outside the module
  root** (`computeTestInputsID`, "Do not recheck files outside the module, GOPATH, or GOROOT root"), and
  the three predicate sources live under `src\tests`, outside `src\go2cs`. A narrowed predicate therefore
  reports `ok (cached)` and only fails under **`-count=1`** — so a change touching ONLY harness C# owes
  `go test -count=1 ./...`. The first guard has no such gap (every input it reads is inside the module).
  ⚠ **The same cache serves a NESTED invocation, which is the hardest layer of the vacuous-green class
  to see.** When a Go guard shells out to a script that itself runs `go test`, the inner run can be
  served from cache: the arm reports `ok (cached)` and **tests nothing**, while the outer suite is
  genuinely running and the arm genuinely appears in its output — every visible signal healthy. **A
  nested `go test` is always `-count=1`.**
