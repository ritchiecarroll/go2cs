---
name: gate-forensics
description: Read a gate, build or test result that looks wrong. False-green routes 1-8, instrument traps, concurrent-session kills, truncated logs, mass-empty signatures, and the census and launch hazards that make a green vacuous or a red false. Use before believing any gate verdict.
---

# Gate Forensics

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 1082-1128, 1368-1443, 1771-2535, 3115-3151.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

- **FALSE-GREEN route #6 — an instrument that cannot find its own runner reports SUCCESS (found
  2026-08-24 by two lanes from opposite directions; closed the same day).** A different SHAPE from
  #1–#5: those all run a gate and measure the WRONG thing — a stale binary, stale output, an
  unenumerated package, an old front end — so each yields a verdict that is merely untrue. This one
  measures **nothing** and prints a pass over the hole. `src\_paths.ps1` spelled the corpus TFM as a
  **literal** (`$NetVersion = 'net9.0'`), the TFM census's Class-D hoist that had gathered nine
  hardcoded sites out of six files into that one line. Hoisting fixes the SPREAD, not the KIND: a
  hoisted literal is still a literal, and on a `net10.0` tree every consumer composes
  `bin/Debug/net9.0/`, which does not exist. `run-behavioral.ps1` fails loudly; **`run-performance.ps1`
  died in 20 seconds having run nothing**, and the only tell was the implausible speed — a full perf
  run is HOURS on this machine class. **No existing gate can see it**, because each wrapper's only
  preflight is the `dotnet build` exit code and **the build is genuinely green** (it writes to the TFM
  the projects declare); the runner that would have counted anything is never reached, so there is no
  phase to fail, no project list to come up short, and nothing to compare against a golden. Both
  halves are closed. **(a) `$NetVersion` is DERIVED** from the property of record —
  `src\Directory.Build.props`'s `<TargetFramework>` element, read by one file-read-plus-regex with no
  MSBuild and no `dotnet` (this module is dot-sourced by every instrument on every invocation),
  comments stripped so the props file's own prose cannot be read as the property, and **no fallback to
  a literal**: an instrument that cannot know its TFM throws, naming the file. Replacing the literal
  with `net10.0` is the tempting fix and the wrong one — it re-breaks at the next hop, which is what
  `docs/DotNetMigration.md`'s *derivation, not replacement* means. It also makes `migrate-tfm.ps1`
  honest: that instrument carries no site for `_paths.ps1` because its census already believed the
  PowerShell probe derived. **(b) Every wrapper that launches a runner asserts the executable EXISTS**
  before invoking it, and exits **non-zero** naming the expected path when it does not
  (`run-behavioral.ps1`, `run-performance.ps1`, and `run-performance-floor.ps1`'s bflat arm — which
  runs at `'Continue'`, so a missing compiler would otherwise report `ok` off a stale `$LASTEXITCODE`).
  Derivation removes today's trigger; the guards close the class, since any future cause of a missing
  runner is now loud. ⚠ The guards use an explicit `exit 1` rather than `throw` because the exit CODE
  is the property that matters and a `throw` leaves it to the host: on Windows PowerShell 5.1 the
  missing-runner path already exits 1 (measured, both wrappers, `-File` and `-Command`), so the
  exit-**0** sighting is a host- or wrapper-dependent swallow — which is the argument for stating the
  code rather than inheriting it.
- **FALSE-GREEN route #7 — a `go2cs-gen` (analyzer) change is invisible to EVERY standing gate except
  a behavioral COMPILE (found 2026-08-30, the W3a promoted-forwarder regression; fixed `0df5a3f2b`).**
  CNR is transpile-only, so generator output never enters its verdict; the stdlib solution compiles
  one assembly at a time, so an accessibility demotion that breaks only CROSS-assembly consumers
  stays green there (`internal` binds fine same-assembly); and the corpus 307/0 + CNR-byte-identical
  ladder a converter arc normally runs therefore proves NOTHING about gen changes. The W3 merge
  demoted net's public `TCPConn.Read/Write` promoted forwarders to internal and shipped green on
  exactly that ladder; the escape was caught days later by a derived net/http canary sweep — the only
  union gate that compiles a cross-assembly consumer of metadata-promoted surface. **Rule: any change
  under `src/gen/` owes a full behavioral COMPILE phase (slnx-dev build or the runner's Compile) and
  at least one cross-assembly consumer gate before banking.** Corollary paid the same night: ONE red
  behavioral project collapses the full-suite verdict into 651-suspect attribution (the Transpile
  phase rewrites every `.cs` first, so no assembly is up-to-date and the batch-build failure
  attributes everywhere) — measured: exactly 1 Release assembly written corpus-wide vs a clean
  78-project filtered batch. "651 suspects" means "one project is red", not "the corpus is broken".
- **FALSE-GREEN route #8 — a guard DISARMED by a LEGITIMATE change (found 2026-09-01, the
  init-hook relocation).** Distinct from routes #1–#7: nothing is stale and nothing mis-runs — the
  guarded property genuinely moved house, and a guard asserting an assembly-level property by
  grepping ONE emitted file goes silently VACUOUS in its negative direction ("the bare form must
  not appear" is trivially satisfied by a file that no longer holds the construct at all). The
  positive direction fails loudly and gets fixed; the negative just stops testing, and the exit
  code says two failures when the real damage is four assertions. Glob-widening cannot fix the
  class when a DRIVER writes the artifact the exercised call never touches. Remedy: assert the
  DECISION (the recorded map/registry the pass writes — `packageImportInits` in the measured
  case), never the artifact's text, and re-check a guard's negative arm whenever the construct it
  greps for legitimately relocates.
  ⚠ **Route #8's sharper form KILLS the suite instead of going vacuous, and its verdict rides CLASS
  ORDER** (measured 2026-09-02, GolibTests): the guard's premise — "GolibTests does not reference
  converted `flag`" — was disarmed by a later `ProjectReference`, after which the converted `flag.Parse()`
  parsed MSTest's OWN command line through the process-global `flag.CommandLine` (`ExitOnError` →
  `os.Exit(2)`) unless a sibling class that replaced it with `ContinueOnError` happened to run first: one
  host's 460/460 was a lucky ordering, another's 82-then-abort the same defect. The fix SHAPE matters as
  much — the host parses its OWN args and **never mutates a process-global that converted tests read**
  (`os.Args` feeds `sync`'s `TestMutexMisuse`, `flag`'s `TestExitCode` and every self-re-exec): a
  divergence STATED against the ruling is how to diverge.
  ⚠ **Route #8's TEXT-GREPPING family, three members measured 2026-09-03/04, all cured by the same
  remedy — assert the DECISION, not the emission.** (1) A guard grepping emitted `.cs` went false-RED
  at master on a hand-own's HEADER COMMENT quoting the pre-fix emission: strip comments, or assert the
  recorded decision. (2) **A census of the EMITTED text cannot recover a decision the converter made**
  — `[GoRecv] … this ref T` matches **5,686** declarations because it is ALSO the emission for every
  Go VALUE receiver on a struct, so a grep for "pointer-receiver primaries" reads the wrong population
  by an order of magnitude; records that describe a converter decision derive from the front end
  (`go/types` receiver kind) plus the pass's OWN selection map, and a guard over them compares against
  that map. A claim nearly carried on such a grep was retired by the census before it reached a
  design. (3) **A guard whose PATTERN under-scopes cannot go red where it matters**: the keep-alive
  census's `syscall.`-qualified pattern scanned 83 of 351 protected sites and, over the `syscall`
  package alone, reported ZERO temps — tripping its own vacuity check — so an injected defect there
  was INVISIBLE. Two companions from the same arc: **a guard arm can be RED-BY-DESIGN at a branch tip
  and correct** (arm 2's 12 sites are exactly what a sibling seat fixes, green on the union), so merge
  ORDER decides a guard's verdict and the train's ASSEMBLED tree is what it is measured on; and a
  STATIC "this guard is red at master" claim was FALSIFIED by running it (clean 83/83) — run the
  guard before reporting its colour.
  ⚠ **Two more members, measured 2026-09-04, both inside guards written to notice a REWORDING.**
  (4) **A guard that greps a source file for a MARKER STRING reads the PROSE that explains the marker
  beside the code that matches on it** — every instrument that classifies a converter stderr line
  carries a comment naming that line — so a `strings.Contains` guard stayed GREEN with the live
  classifier DELETED, twice, until its own positive control said so. The guard extracts the LIVE regex
  literal from the code, and REJECTS `-notmatch` EXCLUSION lines, which carry the marker's words a
  second time and would read fine with the classifier gone. (5) **A substring "is the marker still
  consulted" check passes a reworded pattern that CONTAINS the original** (`GoArchExclusiveXX`) — the
  glyph-substring over-match inside a guard whose entire purpose is to notice rewording: extract each
  CONSUMER's live pattern, compile it, and require it both to MATCH a real marker line and to REJECT a
  prose decoy. Their companion at the CNR: a **check-only switch that empties the measurable set AFTER
  the skip block prints** is what lets a live classifier be controlled without a transpile.
  ⚠ **(6) A guard whose EVIDENCE BASE is wider than its QUESTION cannot go vacuous and cannot fail to
  fire — it fires TOO OFTEN, on a population it was never scoped to** (2026-09-07, `7adfbeb45`).
  `hostFatalMintViolations` globbed every proof page, so runtime's `TestEmptyString` was refused
  because `encoding/json`'s page passes. Its own suite could not see it because every fixture used ONE
  package, so the package axis was never varied: **"a control only tests the axis you varied", read on
  a guard's INPUT POPULATION rather than on an A/B arm.** The fix derives the scope from the same
  directory the manifest was loaded from, so the two cannot disagree; the cross-package arm, written
  RED-FIRST, reproduced the measured case by name.
  ⚠ **The constructive form of "assert the DECISION": a CONVERTER ATTRIBUTE that RECORDS a decision IS
  a census of its own population, and beats any predicate written later** (2026-09-05).
  `[GoValueClone]` marks every struct carrying a fixed-size array field (or another struct that does),
  so the READ-side defect (a `*[N]T` cannot be VIEWED over native memory) and the WRITE-side defect
  (such a struct cannot be PASSED to the kernel by address) share ONE population — 493 attributes
  across 95 package metadata files plus 47 in 27 other files, measured with both controls. **Two open
  items that share a population are ONE class**, and the point remedies are debt against the model fix,
  named as such in its design record.
  ⚠ **A guard that documents a defect it does not ASSERT is PARKED, not landed half-green** (measured
  2026-09-03, the nine-shape dims guard and the canonical-identity tripwire). A red-by-construction
  guard lands WITH its arc — complete, with its registration reverted — rather than shipping a
  commented-out row that the next reader reads as coverage.
  ⚠ **The freeze binds INSTRUMENTS, not just edits.** A verify-only landing preflight was one command
  from running in the worktree a multi-hour sweep was using, where its `git checkout --detach` would
  have yanked the tree out from under the running converter and destroyed hours of gating —
  "verify-only" described its GIT effect, not its effect on a neighbour. **Any script that mutates
  worktree state names the processes it would disrupt and REFUSES, rather than trusting the operator to
  remember what else is running there.** The guard is three lines (count live converter/runner
  processes by path, refuse non-zero, name the count) and it fired on its first run (2026-09-06).
- **⚠ CONCURRENT-SESSION KILLS — worktree isolation does NOT isolate `Get-Process <name> | Stop-Process`.**
  Those cleanup preambles (here, and the ad-hoc `Get-Process BehavioralRunner,testhost | Stop-Process` that
  is easy to type before a run) match by process NAME across the whole machine, so they kill a SIBLING
  worktree's in-flight suite. Signature: **exit `-1` with the log truncated mid-line and no diagnostic** —
  e.g. a full run died at 124s inside `PreBuildSharedDeps` and another at 163s inside `RunCompileGo`, and
  the same corpus then passed 521/521 untouched. Read that as "killed externally", NOT as a compile failure,
  and do not go hunting for a runner bug. Waiting for the other worktree's process to exit is not enough
  (it re-arms for its next run); the reliable defence is to be **unmatchable by name** — copy the apphost to
  a unique name in the same bin dir and run that (`Copy-Item BehavioralRunner.exe myRunner.exe`; it still
  launches the embedded `BehavioralRunner.dll` and `AppContext.BaseDirectory` is unchanged, so discovery is
  identical). Scope your own kills by path (`Where-Object { $_.Path.StartsWith($myWorktree) }`) so you are
  not the one doing this to somebody else.
  **⚠ The apphost rename does NOT cover the other reaper — harness background-task TREE reaping (measured
  2026-08-12, the A2 integration agent).** Being unmatchable by name defends against a sibling's
  `Get-Process <name> | Stop-Process`; it does nothing against the harness reaping a session's own process
  tree when a turn ends, because that walks parentage, not names. A long runner started as a background
  Bash/PowerShell child is IN that tree and dies with it — the same truncated-log, no-diagnostic signature,
  which is why it reads as the by-name kill and gets misdiagnosed as one. Surviving it required launching
  the runner **DETACHED** via `Start-Process` so it is not a child of the turn's process tree. Same shape
  as the sweep caveat in the budget table below (a LANE parking a detached sweep still loses it); the
  difference is that `Start-Process` detachment is what makes a long run survivable at all.
  **The detachment flags are load-bearing (measured 2026-08-14, the argv-stop and os-signal lanes):**
  `Start-Process -WindowStyle Hidden` with output redirected to a log file survives the reap;
  `Start-Process -NoNewWindow` followed by `Wait-Process` does NOT — the wait re-parents the session's
  fate onto the child and the turn boundary kills it exactly as if it had been spawned inline.
  ⚠ **The two detachment stories are measured and point OPPOSITE ways (2026-09-02).** A
  `Start-Process -WindowStyle Hidden` from INSIDE a PowerShell TOOL call died silently ~15 s in (the
  documented pattern covers a BASH-launched child surviving the turn boundary, not a tool call's own
  job scope), while a Bash `run_in_background` task is reaped with the SESSION's process tree — a
  2-hour solo sweep died ~13 min in and sat UNDETECTED for 76, with no completion notification.
  Anything longer than a turn runs DETACHED, env-pinned in the SAME command, logged unique-per-run
  and polled POSITIVELY by PID;
  clean-death evidence before a restore is modified files with ZERO untracked.
  ⚠ `Wait-Process` has ALSO reported a still-running target as exited, twice in a row (2026-09-01,
  the residual-pass lane): a background-wrapped `Wait-Process -Id` said done while
  `Get-CimInstance Win32_Process` showed the host alive with a live `go2cs.exe` child — one
  redundant CNR raced into the same behavioral tree before it was caught (the r41 overlap hazard,
  avoided only just). Mechanism unconfirmed; treat any `Wait-Process` "done" as unverified until a
  positive `Get-Process -Id` poll agrees (`while (Get-Process -Id $pid) { Start-Sleep 20 }` read
  correctly where the wait lied). Poll the
  log file (or the process by PID) instead of `Wait-Process` — and write the poll POSITIVELY
  (`while` + explicit `exit 0`/`exit 1`), never `until ! powershell -Command "exit (Get-Process …)"`:
  `exit $true` is exit code 1, so that loop ends instantly and reports "exited" while the process
  still runs (measured 2026-08-16 — the false reading launched a SECOND CNR into the same tree, two
  racing transpiles, caught only by PID inspection).
  ⚠ **The same false-"exited" reading has a SECOND, entirely different mechanism: Git Bash's
  `kill -0 <pid>` cannot see a WINDOWS pid** (measured 2026-08-26). The Bash tool's `kill` resolves
  pids in its own emulation namespace, so `while kill -0 $PID; do sleep 30; done` against a pid from
  `Start-Process -PassThru` (or any `Get-Process` id) exits on the FIRST iteration and reports the
  process gone while it is still running — no error, exit 0, indistinguishable from a real
  completion. It reproduced the 2026-08-16 damage exactly and then some: two CNR runs believed dead
  were alive, a third was launched, and THREE concurrent transpiles raced into one behavioral tree
  (the r41 "never let two conversions overlap" hazard), with a partial 2-package `git status` that
  read as a reassuring near-clean verdict. The tell was an mtime census — 288 of 641 packages never
  re-transpiled — and the proof was `Get-CimInstance Win32_Process` showing both "dead" hosts alive
  with live `go2cs.exe` children. **Rule: never wait on a Windows pid from Bash.** Wait from
  PowerShell (`Get-Process -Id`), or — better — make the long run the harness BACKGROUND TASK itself
  (`run_in_background`, the child of bash) so its real exit code is the task's, which is what
  PROTOCOL v3's mailbox monitor already relies on. And when auditing for strays, exclude your own
  querying process: a `Where-Object { $_.CommandLine -like '*check-no-regression*' }` sweep matches
  the very command line performing the sweep, so it reports a phantom survivor and, if you kill it,
  kills your own shell.
  **Three probe-hygiene rules from the same family (2026-09-01/02).** **Process AGE is read from
  `CreationDate` against `Get-Date`**, never against an assumed clock — a healthy three-minute-old
  run was killed in the belief it had been hanging for hours. **`pgrep -f <name>` matches its own
  wrapper's command line** — the bash edition of the self-match above — so a `while pgrep -f` wait
  loop spins forever on its own reflection after the child has exited; match on `/proc/*/exe`, i.e.
  the executable, never on a pattern that can match the process running the check. And **completion
  inferred from a SIDE EFFECT is not completion**: a file reverted because a running CNR had already
  transpiled it is a footprint, not an exit code — check the run.
  **⚠ Two more, 2026-09-02.** **Relaunching a chain while its predecessor's TAIL leg is still alive
  puts two runs in one worktree** — a third rebuild attempt met the second chain's in-flight `reflect`
  `-tests` convert as untracked `*_test.cs` and aborted on its dirt gate, the r41 overlap hazard
  caught only because that gate existed: census live processes (and wait for the task notification)
  before relaunching anything into a worktree. And **the harness's own
  `git status --untracked-files=all` over a worktree full of `bin`/`obj` can run for an HOUR** —
  slow, not hung, and not evidence of anything else.
  ⚠ **TWO SUB-AGENTS DISPATCHED INTO ONE SCRATCH WORKTREE IS THE OVERLAP HAZARD WITH THE
  COORDINATOR'S OWN HAND ON IT** (2026-09-08): a delta gate was sent into the same worktree where the
  previous gate's solution build was still running, with "wait for zero build processes" written as a
  PRECONDITION IN THE BRIEF — and that build went from 0 errors to hundreds in two minutes, every
  code an I/O or metadata-file failure from `obj` trees deleted under a live compile. **A
  precondition in a brief is not a lock; a purge step ordered after a wait is still a purge.** The
  reading is DESTROYED (one environmental failure wearing many codes, per the error-histogram rule),
  the leg is UNMEASURED for that tip and rides the train battery, and the rule is **a per-dispatch
  worktree, never a shared one.**
  **⚠ A pid captured from `ps` seconds after `setsid` can be the WRAPPER** (met by two lanes on
  2026-09-04), and `ps | grep <script> | head -1` picks it too, because the wrapper's own eval line
  carries the script's text — such a "process" reports EXITED instantly, the false-"exited" family
  through a third door. **The chain writes its own PIDFILE and the waiter reads that.** Beside it, on
  the container class: **a restart notice is not evidence either way** — one restart killed the
  watcher and spared the detached chain, the next killed the chain mid-leg and left a 0-byte log — so
  check PID IDENTITY before relaunching, and never relaunch on assumption.
  ⚠ **SELF-BACKGROUNDING INSIDE A TRACKED BACKGROUND CALL MAKES THE HARNESS TRACK THE LAUNCHER**
  (2026-09-08): a `nohup … &` issued from inside a harness background task left the harness watching
  the wrapper, which reported EXIT 0 while the real loop died after one project and six legs went
  unmeasured under a tool that said success — **the killed-wrapper rule met from the launcher's own
  side.**
  ⚠ **And `setsid nohup … &` leaves `$!` naming the SETSID PARENT**, not the chain (2026-09-05) — the
  same wrapper-pid reading through the LAUNCHER's own door rather than through `ps`. **PID-record a
  detached run from INSIDE it** (`echo $$`, then `exec`), and kill by an exact `bash <path>` match
  that excludes `$$`.
  ⚠ **A ROW WHOSE DEADLINE FLOOR EXCEEDS A HOST'S UPTIME IS NOT MEASURABLE THERE, and is not
  retried** (2026-09-04): an hourly-restarting container against a 30-minute row cannot produce a
  verdict however many attempts it makes. State it, and move the row to a host that stays up — or drop
  it.
  **⚠ "An instrument that can match ITSELF is measuring its own presence" (named by the lane that
  paid it, 2026-09-03) — and it kills.** The coordinator's own process census
  `CommandLine -like '*<worktree-id>*'` matched the coordinator's OWN bash (whose command line carried
  the id as a variable) and `Stop-Process` killed the querying shell mid-command, exit 255; a kill
  loop keyed on a branch-name fragment matched its own shell the same way; and a `/proc` census keyed
  on a marker string counted the probe whose own `case` pattern contained it (2 monitors reported
  where 1 ran, a healthy one nearly killed). **Any census that KILLS excludes the shell/host process
  names (`bash`, `powershell`, `pwsh`) and its own PID, or matches on the EXECUTABLE
  (`/proc/*/exe`) — and better, keys on something the RUNNING loop ASSIGNS** (a variable the loop
  sets, which a probe merely describing the loop does not contain). Every instance was caught only by
  a second derivation disagreeing, never by the census itself. A rule met twice becomes a CHECKLIST
  line in the lane's own scripts, not a thing to remember.
  (`pkill -f <script path>` issued from a command line that CARRIES that path is the same self-match
  in bash, and it kills the caller's own shell: kill by PID from a bracketed grep.)
  ⚠ **And SPLITTING the pattern with concatenation does NOT help** (measured 2026-09-05): the
  substrings are still in the querying shell's own command line, so the wildcard still matches — a "no
  chain may be live" gate reported 4, then 3, every one of them the shell performing the check, age
  0m. **Filter by AGE** — a real long-running process is minutes old and the querying shell is seconds
  old, read from `CreationDate` as above — **or by ancestry.**
  ⚠ **ON A BOX WHERE `pwsh` IS A DOTNET GLOBAL TOOL THE QUERYING SHELL IS THE DOTNET HOST, so a
  quiet-box census by process NAME matches ITSELF and can never report quiet** (2026-09-08): exclude
  SELF by ANCESTRY (never by command-line text, the self-match trap's other door), count an
  UNREADABLE command line as a BLOCKER rather than as absence, and positive-control the census in the
  MUST-FIRE direction — a real build must read LOADED and an actively compiling analyzer server must
  read BLOCKER. It caught a genuine FOREIGN blocker, another agent's `go test`, before the first
  timed run.
  **⚠ ABSENCE AT ONE INSTANT IS NOT DEATH, in three costumes (2026-09-02/03).** A sub-agent's
  **0-byte task-output file does not mean it died** — the transcript is written at COMPLETION, and a
  worktree with no new files for an hour can be a seeded reconvert writing outside `src`; the
  coordinator declared a live agent dead, dispatched a duplicate, and had to stop it when the original
  reported with full gates. A gate wrapper called "dead" had merely finished its leg and then spent
  **26 minutes** inside `git checkout` + `git clean` over a `bin`/`obj`-heavy worktree — the process
  probe landed BETWEEN spawns — while `| tail -25` buffered the task's whole output so its log read
  empty while it ran. **Liveness is a process census against the worktree's path PLUS the agent's own
  notification; a stopped-with-no-children notification is the only "done".**
  **⚠ Correction to the pipe rule, measured 2026-09-03: a pipe masks a command's exit status only
  WITHOUT `set -o pipefail`** (`set -uo pipefail; false | tail -1` → 1; without pipefail → 0), and a
  REDIRECT (`> log 2>&1`) preserves the command's own status. The durable half of the earlier note
  stands — capture the real exit before any pipe, and read the RECORD rather than an exit code,
  because the record carries per-test verdicts an exit code cannot — but "an exit code is worthless
  the moment a pipe is in the command" is FALSE and was retracted within the hour by the lane that
  wrongly confessed it. **A false CONFESSION corrupts the record as much as a false success.**
  **⚠ A harness `TaskStop` (or killing the bash) does NOT reap the converter child** (measured
  2026-09-03): `go2cs.exe` kept spawning after the stop, the PIDs `taskkill` reported differed from
  the ones listed seconds earlier, and a REUSED log path spliced two runs into one file — together
  putting TWO conversions into one seed root (the r41 DYNTYPE hazard), with nothing banked only
  because the diff never ran. **A seeded-conversion instrument carries a PREFLIGHT that refuses to
  start while any `go2cs.exe` is alive, and a run-tagged log name.** ⚠ And the r41 rule WIDENS
  (2026-09-03): **ONE converter process per lane box at a time** — a CNR died 43 s after a `-stdlib`
  conversion started on the same box with a DIFFERENT output root (mechanism unrooted), so footprint
  diffs wait for the CNR to print its verdict.
  **⚠ Two 2026-09-05 sharpenings, and the first inverts how a killed run is read.** **A KILLED WRAPPER
  TASK IS NOT A VERDICT ON THE CHILD IT LAUNCHED**: a `-stdlib` converter OUTLIVED its reaped wrapper
  and kept writing, so the staged trees were already full and a diff taken at the kill would have read
  a confident ZERO — the emitted-before-seeded retraction reached by a second route. Continuing after
  such a kill is legitimate ONLY because the run's sentinel files survive (the written-this-run
  assertion can still be made per arm and target): **wait on the CHILD's pid POSITIVELY from
  PowerShell** (never bash `kill -0`, never `Wait-Process`), then RE-ASSERT both arms' written counts
  before diffing. (Two lanes' independent arms reproducing the same per-target emission counts —
  1,656 / 1,724 / 1,727 — is a cross-check on the instrument worth naming, not a coincidence to pass
  over.) And **stopping a background TASK kills the task's SHELL, not the chain it launched**: an
  assembly script survived as an ORPHAN, its killed leg returned looking finished, and the relaunch
  put TWO chains in one worktree writing interleaved stamps into one log — every reading discarded.
  **A stop is a TREE kill by parentage from the chain's ROOT** (`taskkill /T` on the root pid — and in
  PowerShell `$pid` is a RESERVED automatic variable, so a hand-rolled walk that assigns it silently
  kills nothing), verified by a census that names ZERO survivors; and **no process sweep by executable
  NAME while a sub-agent shares the box** — the pattern that matched the chain's converter also matched
  a sub-agent's two-seeded-diff arm, twice in one morning.
  **⚠ WHY that slot is censused by the PARENT processes and NEVER by `go2cs.exe`** (measured
  2026-09-04). A CNR re-transpiles every behavioral dir by spawning one SHORT-LIVED converter per
  package, so between any two packages there is a real interval with ZERO `go2cs.exe` alive: a binary
  census reads FREE hundreds of times during a run that holds the slot, and a lane waiting on
  `while (Get-Process go2cs)` would have dropped a `-tests` pipeline into a running battery on its
  first sampled gap (900 s of luck before it was caught). The holder of a slot during a CNR has no
  converter of its own most of the time. **Census the HOSTS** — `powershell`/`pwsh`/`BehavioralRunner`
  whose command line names `check-no-regression`, `run-behavioral`, `run-validated-sweep` or
  `BehavioralRunner` — excluding the querying process and the lane's own tree, POSITIVE-CONTROLLED
  before it is trusted (with N holders live it must report N), with the launcher RE-CHECKING
  immediately before it starts and refusing on a live parent, which is what makes the rule cheap.
  ⚠ **BUT `CommandLine` IS NOT RELIABLY READABLE FOR EVERY PROCESS, so a filter over it answers ZERO
  and reads exactly like a dead run** (2026-09-05, three times in one night against a run that was
  demonstrably alive — its converter executing, its emission dirt growing). **Census by process NAME
  UNFILTERED, or corroborate with the run's OWN artifacts** (emitted files, log mtime), before
  believing a zero: the same shape as the bash-`kill -0`-on-a-Windows-pid and `pgrep`-self-match traps
  above, and the reason the positive control on the host census is not optional. Its other half, from
  the same week: **classify a background shell by its COMMAND LINE — the parent bash's, via
  `Get-CimInstance` — and NEVER by its RHYTHM.** A coordinator's task list shows SUB-AGENTS' background
  shells beside its own, and eight staggered 60 s poll loops were classified as "monitor iterations"
  from their cadence when one belonged to a sub-agent; a sub-agent's poller dies with the sub-agent.
  ⚠ **And a CLAIM is closed by its owner's release POST, never by an absence somebody else observed**:
  `Get-Process go2cs` = 0 and a MISSING `go2cs.exe` between two legs of a gate chain is exactly what a
  REBUILD looks like (`go build` deletes and rewrites the binary; CNR rebuilds it unconditionally), so
  a lane reading them as a release can start an hour-long run under a claim that is still live — the
  other sibling-misread being the inverted `exit $count` poll above. A claim past its stated size is
  re-read by ASKING its owner, not by inference.
  ⚠ **The serial order relaxes on MEASURED properties, never on impatience** (2026-09-04): it protects
  exactly ONE property — a refusal branch that trips on a per-project transpile TIMEOUT — so CNR (no
  per-package budget), separate worktrees (the r41 hazard is per-ROOT), a `-stdlib` A/B pinned at
  `-convert-timeout 90m` that cannot be pushed past its cap, and a corpus-scale control carrying a
  raised transpile budget plus a "timeout-shaped refusal re-runs SOLO before belief" rule may overlap
  them; what stays serial is anything WITHOUT those guards. What binds across worktrees is LOAD, which
  is why an A/B under concurrent lanes passes the same `-convert-timeout` to BOTH binaries and every
  loaded run records its WALL beside its verdict — a loaded wall can only produce a false red. And **a
  `-stdlib` census is never PARKED**: it can only be re-seeded from scratch, throwing away completed
  targets. ⚠ One census trap from the same family: **a process census keyed on a WORKTREE NAME
  over-matches its PREFIX** (`sub-q2` inside `sub-q23`) — the glyph-substring rule in a new costume.
  **⚠ A watcher keyed on a background task's OUTPUT FILE cannot see a chain whose stdout is
  REDIRECTED to a log** (2026-09-03): the task output is 0 bytes, the trigger never fires, and the
  watcher waits out its whole timeout looking healthy — route #6's shape, one layer out. Key a watcher
  on the artifact the watched process actually WRITES, and check the watcher's own log for its trigger
  line rather than assuming it armed.
  **⚠ A battery's VERDICTS must land in the log the watcher tails, or a red leg is INVISIBLE**
  (2026-09-04): one train's assembly log stamped only leg BOUNDARIES while the CNR's `exit=1` and its
  one CHANGED file went to a per-leg log nothing watched — the boundary stamp read as progress, the
  restore stamp erased the drift, and the red sat unseen for half an hour until the leg log was read
  by hand. **Every leg stamps its EXIT CODE and its one-line verdict** (the count, the changed set)
  into the assembly log, the monitor's pattern includes `exit=[1-9]`, and a leg that CONTINUES past a
  failure by design (so later legs' verdicts transfer when the remedy is a golden) says so in the same
  stamp.
  **⚠ Three 2026-09-05 sharpenings of that stamp, and the first is the FALSE-RED twin of the
  verdict-word false green.** **Gate on the CODE the instrument prints (`GUARD exit=[1-9]`), never on
  a WORD in its prose**: a launch gate spelled `grep -icE 'guard.*(FAIL|RED|MISMATCH)'` counted an
  honest `ROSTER GUARD exit=0 (0 fail lines)` as a failure and refused a clean launch. **A leg that
  keeps the runner's output in MEMORY and stamps only summary lines leaves a `Failed: 1` with no
  NAME** — write the WHOLE output to a per-leg detail file and stamp every `Failed <test>` line into
  the assembly log beside the exit code. And **a monitor built on `tail -F <log>` can die SILENTLY**
  (twice in one day: its tail process gone, 0 events, the task still listed) — a watch whose death is
  indistinguishable from quiet. Watch a chain with SINGLE-NOTIFICATION background waits
  (`until grep -q '<stamp>' log; do sleep 60; done`) re-armed per leg, census a monitor's tail by
  command line before trusting its silence, and kill stale tails by PID — they hold the log open
  across sessions.
  ⚠ **A MAILBOX MONITOR FIRES ON YOUR OWN POST.** An exit-on-change watcher anchored before a
  coordinator's own entry wakes on that entry — the own-push blind window arriving as a FALSE WAKE.
  **Confirm the new tip's AUTHOR before reading a wake as a lane**, and re-anchor after every post.
  ⚠ **But RE-ANCHORING A MONITOR IS NOT RE-ARMING IT**: editing the anchor constant in a watcher's
  script changes what the NEXT run compares against and **starts nothing**. A coordinator that armed and
  pid-verified both watchers at session start then re-anchored twice across two posts and left the fleet
  unwatched for ~8 minutes, because the edit SUCCEEDED and the success of the edit was read as the state
  of the watch (2026-09-07). **The check after every re-anchor is a PROCESS CENSUS, not the edit's exit
  code** — the same distinction as an EXITED task id being evidence of a PAST arming. Companion: a
  filtered `ps -ef | grep <script>` reads **ZERO for a live monitor**, because MSYS `ps` prints only
  `/usr/bin/bash` and never the command line; census UNFILTERED and match the ARMED line's pid.
  **⚠ A ROW-LIST DRIVER GUARDS `processed == listed`, PRINTED AND FLAGGED** (2026-09-04): PowerShell
  ate the loop's stdin, so a 3,643-verdict row was swallowed WHOLE and the next row arrived as
  `atabase/sql` — no error, no failure, a 21-row sweep that would have reported as 23 (the fix is
  `< /dev/null` on the child, with the row list on FD 3). Four companions from the same driver: a
  verdict grep ANCHORED AT COLUMN 0 misses the sweep's INDENTED verdict line, so every passing row
  read as a fault — and the fallback printing `NO VERDICT LINE -- instrument fault` rather than
  defaulting to a verdict is the right shape (a control runner whose arms yield no verdict line ABORTS
  the arm; four green arms in a row is the tell, not a pass); the driver RESTORES after EVERY row, not
  at the end (54 files of dirt after two rows); it REFUSES to start on a dirty tree or with a
  `go2cs.exe` alive (a relaunch started at dirty=14 because the killed run's children were still
  writing); and **a harness `TaskStop` kills the harness TASK, not the detached bash → PowerShell →
  converter chain beneath it** — kill by process ANCESTRY keyed on `go2cs.exe`, which a probe never
  spawns (two probes matched their own querying shells).
  **⚠ A measurement taken ACROSS a host SUSPENSION is not a measurement** (2026-09-03): a laptop going
  lid-closed inside `net/http` fabricates exactly the mid-stream death signature that row instruments.
  The honest choice is a clean tree-kill by VERIFIED PARENTAGE (22 processes; a bare `go2cs` kill
  orphans the host and locks `runtime.dll`) and **no record**, then a relaunch behind a readiness gate
  (pinned toolchain answering, network up, no converter alive, clean tree) on an ABSOLUTE-deadline
  wait that re-reads the clock, so it fires on wake rather than during standby.
  **⚠ DISK is a gate input, and a battery preflights its own floor (measured 2026-09-03).** A train
  battery's TAIL legs — runner, sweeps, pair, `reflect` — all aborted in ONE MINUTE on the sweep's
  disk preflight (18 GB free against a 25 GB floor) while the chain itself EXITED 0: seven sub-agent
  worktrees plus one leg's own 21 GB of behavioral build output crossed the floor mid-battery, the
  chain's exit code carried nothing, and the leg logs carried everything. Three rules. **A battery
  carries a disk-floor preflight of its OWN before its first leg, and a chain's exit code is the OR
  of its legs.** **The instrument is a per-worktree `bin`/`obj`/`Generated` SIZE census, not a
  directory count** — the box's largest build output (87 GB) sat in the MAIN checkout nobody had built
  from in two days while two batteries fought over the last 20 GB; a checkout is purged only after its
  newest build-output mtime AND a process census both say idle. And **`src/clean-bin.ps1` asks a
  `Read-Host` confirmation**, so a non-interactive invocation CANCELS silently (exit 0, nothing
  removed) — purge with a direct `Get-ChildItem -Include bin,obj,Generated -Recurse | Remove-Item`,
  ⚠ scoped so it does not match `src/go2cs/bin` (the standard `find … -name bin … -exec rm -rf`
  idiom DELETES `src/go2cs/bin/go2cs.exe`; it failed loudly with rc=127 that time), and re-verify the
  converter exists at the invoked path after any purge.
  ⚠ **A BATTERY LEG REFUSED BY ITS OWN PREFLIGHT STAMPS A PLAUSIBLE-LOOKING VERDICT LINE**
  (2026-09-05): the sweep's 25 GB disk floor refused at 20.2 GB free, the leg stamped "0 records
  preserved" and an empty `PAIR:`, and the chain rolled on into two more UNMEASURED legs — the tell
  was the CLOCK (22 rows in 24 seconds). **A chain reads every leg's log for its REFUSAL markers and
  STOPS on one** (now wired: a DISK PREFLIGHT line in the sweep's log stamps the leg UNMEASURED and
  exits 3), and **a leg's stamp carries its ROW COUNT** so an empty leg cannot read as a green one.
  Disk is a battery INPUT: census free space before a train launches AND after its full suite (the
  per-project bins are ~20 GB), and purge the behavioral bins the moment the Output phase ends.
  ⚠ **A `Tee-Object` ONTO A LOG THAT `Start-Process -RedirectStandardOutput` ALREADY HOLDS THROWS,
  ABORTS THE PIPELINE, AND LEAVES `$LASTEXITCODE` AS *TEE'S*** — so a battery reported **"5 legs, 0
  failed" having run ZERO legs**, each in `0s`, and would have banked as a green canary run
  (2026-09-07). The DURATION was the only tell, the same tell as the perf runner that "completed" in
  20 seconds. **Capture a leg's exit BEFORE any pipe, redirect each leg to its OWN path, and never Tee
  onto a stream the launcher already owns.** ⚠ **A gate's THIRD outcome is what saves it: assert that
  the leg PRODUCED A VERDICT LINE and report `NOT MEASURED` when it did not** — the rewritten battery
  reported NOT MEASURED on its next failure instead of PASS, which is the whole difference between a
  false green and a caught one. An exit code cannot distinguish "passed" from "never ran"; only a
  positive artifact can.
  ⚠ **And a chain whose FULL-SUITE leg FAILS must not roll into the SOLO COST leg** (2026-09-05, a
  train chain that did): the SWEEPS after a red suite are still data — they read the same root row by
  row — but a cost pair measures COST on a tree that WILL change and must be re-run anyway, so the
  derive STOPS the chain before the pair on suite red. Which legs are still worth running after a red
  is a per-leg judgment the derive encodes, not a blanket continue.
  ⚠ **A TRAIN BATTERY WHOSE EVERY LEG IS WINDOWS-DEFAULT CANNOT SEE A SEAT WHOSE ONLY FILE IS PER-GOOS,
  and a green battery then reads as covering a file it never compiled.** Measured 2026-09-07: a seat's
  sole file was `src/core/runtime/linux/signal_posix_impl.cs`, and `go2cs.slnx`, CNR, the behavioral
  suite and GolibTests are all windows-default or never build `src/core/runtime`, so **no leg touched
  it** — the L3 rule, met inside the coordinator's own battery. A sibling lane closed it with
  `-p:GoTargetOS=linux runtime.csproj` (exit 0) carrying BOTH controls: the log names the file six times
  and the windows-default build of the same csproj touches it ZERO times. **A battery DERIVES a per-GOOS
  leg from the seats' file paths**; a comment-only change is additionally provable without a compiler
  (strip whole-line comments from both sides, equal sha256, zero block-comment delimiters introduced,
  stripper positive-controlled). ⚠ Budget input from the same week: **a `runtime` `-tests` row costs
  ~2.75 GB from cold** — the row itself 0.443 GB, its dependency closure 2.731 GB across 281 `bin`/`obj`
  directories (2026-09-07) — enough for ONE such row on a 16 GB host with margin, not for concurrent
  rows and not for a cold full sweep. **A host limit stated in advance is a routing input; discovered
  mid-run it is a lost measurement.**
  ⚠ **A TRAIN SCRIPT WITHOUT ITS OWN `cd` RUNS IN THE CALLER'S CWD** (2026-09-05): a rehearsal
  launched from the coordinator's scripts folder ran inside the MAIN checkout at a stale head, and
  the only thing that stopped it merging sixteen seats into the wrong tree was the dirt gate reading
  an untracked `.claude/` folder. **Every derived train script names its worktree in its first lines
  and refuses any other** (`[ "$(git rev-parse --show-toplevel)" = <expected> ] || exit`); a dirt
  gate is a backstop, not the address.
  ⚠ **A WORKTREE GUARD COMPARING `git rev-parse --show-toplevel` AGAINST AN MSYS PATH COMPARES TWO
  SPELLINGS OF ONE PATH.** git prints the drive-letter form, `pwd` prints the `/c/…` form, and the
  guard ABORTED a correct run with "wrong worktree" (2026-09-07). It failed SAFE and it still cost a
  launch. **Normalize both sides through the same shell** — `$(cd "$(git rev-parse --show-toplevel)" &&
  pwd)` — the same namespace split as a bash `-f` test against a native tool's path argument, met
  inside a safety check rather than inside the work.
  **⚠ Two more instrument-name traps (2026-09-03).** **PowerShell helper functions named `LP` and `H`
  were NEVER CALLED** — the built-in aliases `lp` (Out-Printer) and `h` (Get-History) outrank
  functions, so a "long-path-safe" comparer returned `$null` for every path and null-vs-null read as a
  clean `True`; only the positive control surfaced it. Run `Get-Alias <name>` before defining an
  instrument helper. And **a fixed-size `tail -N` over an output whose length GROWS with the run is
  not an instrument**: the sweep prints drift AFTER the verdict, the drift list outgrew the tail, and
  a PASSING row read as never having run (exit 0 could not distinguish) — read verdicts from the proof
  page the sweep rewrites (`docs/validation/current/<row>.md`) against the roster, using the page's
  mtime to tell swept from stale.
  Two adjacent PS 5.1 traps the same lanes paid for:
  a repo script's `Write-Host` output goes to the INFORMATION stream, so capture with `*>&1`
  (a bare `2>&1` silently drops every `==>` status line and the log reads as hung); and the sweep's
  `-SkipBuild` expects the converter at `src\go2cs\bin\go2cs.exe` — an outside-the-repo binary path is
  not consulted, so a lane that built elsewhere re-pays the build or copies the exe there first.
  ⚠ **A NESTED `pwsh -NoProfile -File` UNDER A PINNED `DOTNET_ROOT` CANNOT RUN WHERE THE ONLY `pwsh`
  IS A DOTNET GLOBAL TOOL** (i7, 2026-09-08): that build targets an OLDER runtime and the pin hides
  every runtime but the pinned one from it, so the child exits with a large negative code in a tenth
  of a second having run NOTHING, its message naming the missing runtime version. Caught by the exit
  code plus the wall; **invoke the harness script IN-PROCESS from the already-pinned shell** rather
  than spawning a nested one.
  **⚠ The scratchpad directory is SHARED across concurrent lanes on one machine** (measured
  2026-08-15: two lanes both writing `cnr.log` — one clobbered the other's gate log mid-run, and the
  verdict had to be recovered from `git status`). It is session-scoped, not lane-scoped. Prefix every
  scratch filename with your lane/branch name (`<lane>-cnr.log`), and treat an unexpectedly truncated
  or rewritten scratch log as a collision first, a gate failure second. Make the name unique per RUN
  too, not just per lane — a REUSED log path on Windows can splice a fresh run's header onto a stale
  run's tail (file tunneling + partial overwrite) and fabricate readings like "CNR finished in 20 s"
  (measured 2026-08-15). And never census with `grep -P` on this box: it dies with "-P supports only
  unibyte and UTF-8 locales", so with stderr discarded it returns 0 matches and reads as "no sites"
  — a false-empty census that nearly got banked. Use ripgrep (`rg`)/the Grep tool.
  **⚠ Two more, 2026-09-04, and the first is the family's `go test` member.** A NON-VERBOSE
  `go test ./...` prints package-level `ok` lines and **no test names**, so a ladder leg counting
  named-guard results off it reads ZERO by construction — "named-guard-results=0" was the INSTRUMENT,
  not the guards, which ran 11/11 in a filtered verbose run whose own positive control is its eleven
  `=== RUN` lines: **a guard-count leg runs `-v -run <names>` and treats the RUN lines as its
  control.** And **a POSIX bracket expression eats `\[`**, so a GOROOT population of zero taken from
  such a grep is an artifact until the pattern has been made to FIRE on a probe known to contain the
  shape.
  ⚠ **A third member, 2026-09-05:** Git Bash's `grep -F -i -f <patterns>` ABORTS (rc 134, no stdout)
  when one pattern ends in a backslash, and the empty output reads as an EMPTY CENSUS rather than as
  a dead instrument. Use ripgrep, and positive-control the detector on planted lines before believing
  a zero.
  **⚠ The false-empty family has a deeper member: instrumentation that never compiled in (measured
  2026-08-28, the defer-multivalue-spread lane).** A type-aware census was built by patching an
  `fmt.Fprintf(os.Stderr, …)` marker into a converter helper via a heredoc python script, running
  `-stdlib` into a seeded temp root, and counting marker lines: ZERO hits across the whole stdlib,
  and the run looked entirely healthy — the stderr carried the normal spread of converter WARNINGs,
  proving the conversion had really traversed the corpus. The zero was an artifact. The converter's
  `.go` sources are CRLF in the working tree; the script's anchors were LF
  (`"\treturn tuple.Len()\n}"`), so they matched zero times and python's `assert` fired — but the
  script ran under `set -u` rather than `set -e`, so execution CONTINUED and built an
  UNINSTRUMENTED binary. Every downstream step then behaved normally, and the census counted a
  marker that was never compiled in. Two cheap tells were sitting there: the "instrumented" binary
  was BYTE-IDENTICAL IN SIZE to the uninstrumented one, and `grep -c SPREADCENSUS <binary>`
  returned 0 — the marker string was not in the executable at all. The durable rules: patch
  converter sources with the Edit tool (it matches the file's actual bytes), never an LF-anchored
  script — a script that must exist reads/writes with `newline=''` and anchors on CRLF, or
  normalizes first; `set -euo pipefail`, never bare `set -u`, in any instrument whose later steps
  assume an earlier edit succeeded; and ALWAYS positive-control a census before believing a zero —
  run the instrumented binary over a target KNOWN to contain the shape and confirm it fires with
  the expected count (the lane's control fired 12/12 on the behavioral guard's spread rows and
  stayed silent on its two controls, which is what made the real — also zero — production-corpus
  reading trustworthy). Same family as the `grep -P` and bare-`rg` notes: an instrument that
  cannot fail reports success over a hole.
  ⚠ **A `sed` THAT MATCHES NOTHING AND A `grep` THAT MATCHES NOTHING BOTH EXIT 0 AND BOTH LOOK LIKE THE WORK
  BEING DONE.** One returned zero and read as content LOSS; the other returned success and **WAS** content loss
  — five distinct mailbox posts silently replaced by a copy of an older one, subject and body both, because a
  derived script's substitution pattern named a file the target did not contain.
  ⚠ **`sed -i` ON A CRLF FILE NORMALISES ITS LINE ENDINGS EVEN WHEN THE SUBSTITUTION FAILS**
  (2026-09-08) — a SECOND AXIS introduced by a FAILED edit, caught only by a line-ending census
  across every probe arm. Its neighbour from the same run: **a BLANK import emits no `using`**, so an
  arm built on one silently tests nothing about the alias — detected by the MISSING line, never by an
  exit code.
  ⚠ **DERIVING A SCRIPT FROM THE PREVIOUS ONE BY PATTERN SUBSTITUTION IS A SILENT-NO-OP GENERATOR, AND THE
  ERROR COMPOUNDS ACROSS GENERATIONS.** One bad derivation kept the OLD payload; five further generations were
  derived from THAT one, each with a pattern naming the payload it believed it was replacing, each matching
  nothing. **ASSERT THE SUBSTITUTION ACTUALLY CHANGED SOMETHING** — compare before/after, or grep the result
  for the new value; **`sed`'s exit code cannot tell you it did.**
  ⚠ **TWO SHELL IDIOMS THAT MAKE AN INSTRUMENT UNABLE TO FAIL, both measured 2026-09-07.** (1) **A
  python patch fed through a Bash-tool heredoc whose anchor ENDS in a backslash arrives with the
  backslash collapsed** — the quote it precedes becomes escaped, the string unterminated, `SyntaxError`
  at parse — **and the chain CONTINUED past the dead patch** into `bash -n`, `cp` and the launch, so a
  train ran a THIRD time against the unpatched script and read the identical vacuous leg. The
  instrument-that-never-compiled-in family through the coordinator's own door: patch scripts with the
  Edit tool, **assert the substitution landed (`grep -c` of the NEW token, ≥ 1) BEFORE any launch**,
  and gate every step on the previous one's exit. (2) **A `grep -c` whose pattern is a `$'…'`
  carriage-return literal, INSIDE a command substitution in a script file, degenerates to the pattern
  `$` and returns the LINE COUNT** — an LF-only 3-line file reads 3, so a CR == LF structural check
  written that way can never go red, and **every doctrine-landing structural stamp since the idiom was
  written was VACUOUS** (found independently by two sub-agents in one night: one reading 647 CR on a
  zero-CR file, one measuring an assemble script's own stamp). Count CR as BYTES (`tr -cd` piped to
  `wc -c`) with a positive control that an LF-only probe reads 0. The working-tree CLAUDE.md
  re-measured that way reads CR == LF, so the past stamps happened to be TRUE while unable to be false.
  ⚠ **A BROKEN INSTRUMENT THAT PRODUCES A PLAUSIBLE FULL COUNT IS WORSE THAN ONE THAT PRODUCES A
  ZERO** (2026-09-08, the same collapsed pattern one door over): it counted every line and reported
  an LF-only file as fully CRLF, which "confirmed" the wrong branch and talked its author OUT of a
  diagnosis that had been right first time. **The tell was arithmetic: 1,077 of 1,078** — off by one
  on an "every single line" claim, which is strong enough to deserve one check. Measured properly:
  1,078 lines, ZERO ending in a carriage return. **A zero invites suspicion; a plausible total
  recruits the reader.**
  ⚠ **CASE-SENSITIVITY IS A FALSE-EMPTY GENERATOR** and joins `grep -P` on this box, backslash
  collapse in command strings, LF anchors and UTF-16 redirects: a case-sensitive grep for
  `the TABLE, as asked` against a file containing `THE TABLE, as asked` returned ZERO, which read as
  mailbox content being LOST, and a fleet-wide integrity alarm was half-written before a second
  instrument showed the entry present five times. **An implausible result is the cheapest positive
  control there is — and it only works if you stop for it**; a result that is merely surprising gets no
  such protection, which is why the rule is to re-derive rather than to notice.
  ⚠ **A file's contents RECALLED instead of READ is the same failure as every broken instrument, with
  no instrument in it.** One session produced false empties from a case-wrong grep, a `$` anchor
  against CRLF, a suffix match, an unresolvable ref and a cp1252 decode — **and one confident wrong
  claim from memory alone**, same confidence, same wrongness. Judge a MIXED report by its verified
  half's quality and then ask about the rest: the same post that asserted a file's contents from memory
  also positive-controlled, correctly, that a train head was absent from origin — dismissing the whole
  because one half was recalled discards a true finding.
  ⚠ **A PHRASE-MATCH AGAINST A HARD-WRAPPED FILE REPORTS FALSE ABSENCES — join the lines, or match SHORT
  fragments, before believing any miss.** An anchor-verification pass over 28 doctrine entries reported
  **twelve missing anchors and every one was false** (2026-09-06): this file wraps at ~100 columns, so a
  quoted sentence spans a line break and `grep -F` can never match it — one anchor missed on the single
  word ending the previous line — and a second cause rode along, the draft quoting markdown with
  different emphasis placement, so even an unwrapped match would have failed. **The positive control is
  the whole check: cut each "missing" phrase to a distinctive fragment and re-search; eleven of twelve
  resolved on the first shortening.** Same false-empty family as `grep -P`, bare `rg` and the UTF-16 log.
  ⚠ **And a PLACEMENT instrument needs a SECTION GUARD, which is harder than the matcher**: an
  anchor-resolver reached 40 of 40 and applied 760 lines with a median distance of 1 line from each
  anchor — **and 52 of those lines landed in three sections the draft never named**, because a fragment
  can legitimately match text elsewhere. A naive guard requiring the match to sit under the declared
  heading dropped resolution 40 → 26, because anchors come in THREE FORMS — naming a section, quoting a
  clause, chaining to a sibling — and one extractor plus a bolted-on guard cannot serve all three.
  **Parse the anchor's FORM first and resolve per form**; the DRY-RUN landing-site check is what caught
  it, and without it a partially-correct application would have reached the file every lane reads first.
  ⚠ **ALWAYS QUOTE THE HEREDOC DELIMITER when the body carries backslashes, dollars or citations —
  `<<'EOF'`, never `<<EOF`.** An unquoted delimiter lets the shell expand and eat content silently: it
  cost one coordinator THREE separate failures in one night (a python escape collapsing to a syntax
  error twice, and a doctrine item mangled at the moment of banking a rule about that very trap) and
  cost a lane three lost citations in a pushed post. **Two independent participants in one session
  makes it a convention rather than folklore**; for content that must survive verbatim, write the file
  with a quoted heredoc or the Write tool, never through an interpolating shell. Use `chr(92)` for a
  literal backslash **by default, not as a fallback**.
  ⚠ **A BACKTICK INSIDE A DOUBLE-QUOTED BASH ARGUMENT IS A COMMAND SUBSTITUTION** (2026-09-08): a
  post tool's subject passed that way had its backticked span EXECUTED and DROPPED (a
  command-substitution syntax error, then a not-found), while the entry BODY — written through a
  QUOTED heredoc — stayed intact. **The quote-the-delimiter rule has an ARGUMENT twin**: a subject
  that must carry code spans goes through single quotes or a file, never a double-quoted argument,
  and the pushed subject is READ BACK after any post whose command printed a shell error.
  ⚠ **And a positive control that reads ZERO twice may have a broken CONTROL, not a broken pattern —
  keep going until it fires.** A nine-pattern identifier census had eight arms fire on planted lines
  while the UNC arm read 0 twice: first because `printf` aborted on a capital-`U` escape, then because
  a heredoc collapsed the doubled backslash to one. **Both failures were in the PLANTING, not the
  detector**; re-planted through `chr(92)` the arm read 1 on the planted file and 0 on the real ones.
  **An arm that will not fire is not evidence about the target until the control itself is proven.**
  **⚠ The general form, named after five instances in ONE day (2026-09-01): an instrument built out
  of the thing under test cannot independently measure it — the corrective is a SECOND
  DERIVATION.** The sharpest instance is structural: **a `-stdlib` census answers "how much does the
  corpus change" and is BLIND to "does the fix reach the row" whenever the motivating site is in a
  `_test.go` file** — `-stdlib` never writes test emission, so those are two questions and they cost
  two runs. The others rhyme: a probe keyed on its own incomplete predicate reports the predicate;
  a probe blind to unwired slots reports the wiring; a type-name census was believable only once a
  `go/parser` derivation reproduced it, and a classifier's 66 only once an independent predicate
  reached the same number. Four corollaries, all measured 2026-09-01/02. **Name the LAYER a census
  is attached to**: a `claude/g-*` lookup over bare `g-*` refs returned a confident EMPTY, and a
  working-tree line-ending count under the `eol=crlf` pin was reported as a COMMITTED fact (the blob
  is LF, the checkout makes it CRLF, and `git checkout --` "cured" a state that was never in the
  index) — two retractions in one night, both from an unnamed layer. **An empty enumeration inside a
  redirected log is not evidence of absence** until a second instrument agrees. **An EMPTY diff
  after a "fix" is the fix saying it was not needed**, never the gate agreeing. And **count errors
  with the strict `error (CS|MSB|NETSDK)[0-9]+` pattern only** — a loose `grep -cE 'error '` scored
  1 error on a clean 831-assembly build by matching `internal.oserror ->`, and matched the word
  inside Go type names (`(…, error)`) where a case-sensitive `ERROR` read 0 against the loose
  grep's 140. ⚠ Finally, **a census attaches to the DEFECT's boundary — defined by the EXISTING
  marker set — not to the boundary the dispatch named**: a call-argument census missed two
  composite-literal sites the pointer twin's `anyBoxedPtrArgs` already marks, one of them a live
  defect. Grep the marker set first, and attach at every site it covers.
  ⚠ **A CENSUS WHOSE TWO DERIVATIONS AGREE TELLS YOU THEY SHARE A BLIND SPOT; ONE WHOSE DERIVATIONS
  DISAGREE TELLS YOU WHERE THE BLIND SPOT IS.** A `ptrout`-class population read off the CONVERTED CORPUS
  saw 6; read off GO'S OWN SOURCES it saw 16 — **and the gap is the finding, not an error in either**,
  since a function can be in the class without its emission showing the shape the corpus-side probe keys
  on. **When seeking a second derivation, ask what the FIRST one's blind spot is and choose an instrument
  that cannot have it**: two agreeing parsers that both read text and both honoured `^` both matched a raw
  string literal's contents, and the derivation that settled it was GOROOT's own source. Conversely, **two
  independent derivations landing on the same PARTITION is the strongest cross-check available** — a run's
  stack trace and a directive census both split `runtime/pprof` into 1 + 6 = 7, name for name — unlike two
  runs of one instrument, which prove only determinism.
  ⚠ **AN INSTRUMENT CAN BE PERFECTLY SOUND AND STILL BE POINTED AT THE WRONG QUESTION — AND THAT FAILURE
  HAS NO TELL.** Five entries in a 259-row linkname push map were Go source text **inside a backticked raw
  string**: inside the literal the lines genuinely DO begin with `//go:linkname`, `^` is true, and the
  pattern was answering the question it was asked. **The question asked was "which lines LOOK LIKE
  directives"; the question needed was "which lines ARE directives."** No error, no zero, no implausible
  number — five extra rows indistinguishable from the other 254 (corrected to 254 pairs / 253 destinations).
  ⚠ **A NAME ENCODES THE SHAPE, NOT THE COUNT** (2026-09-08): a helper named for five arguments and a
  pair was read as six, and its declaration is FIVE parameters, every one the pair struct — **every
  arity in a shim table comes from a DECLARATION, never from a name**, or the table sizes a body
  against the wrong number. Companion: **a THEORETICAL bound is useless for sizing** (here the
  callback ABI's argument-word limit); the REACH of the consumers bounds it, and that reach split
  into a small ungated set and a larger one behind a toolchain gate that SKIPS ON BOTH SIDES.
  ⚠ **CONTROL AN EXCLUSION FILTER IN BOTH DIRECTIONS: a known-good item KEPT, a known-bad one EXCLUDED.** A
  `"""`-counting state machine desynced on a verbatim string holding an escaped quote, flipped itself into
  "inside a literal" for the rest of the file, and **silently swallowed a REAL directive** — six exclusions,
  one false, and nothing in the output said which. **An exclusion filter's failure mode is silence by
  construction**, and the five raw-string rows above were found at all only because a classifier that could
  not resolve their local names SAID SO rather than dropping them.
  ⚠ **ASSERT A DERIVED SET'S PARSED ROW COUNT AGAINST THE POPULATION'S OWN TOTAL.** A canary-set derivation
  whose row regex missed the roster's markdown link wrapper parsed **5 rows out of 204**, took the largest
  number on each surviving line — a URL digit — and named a 6-verdict package as the LARGEST
  reflect-importing row. Caught on absurdity alone; the parse count is the cheap guard absurdity happened
  to substitute for.
  ⚠ **AN UN-PAGINATED API CALL IS A SILENT `WHERE` CLAUSE — the head-limited hazard through an HTTP
  door** (2026-09-08): a duplicate audit reported a commit count that was a LINE count over the 100
  most recent commits (a default page size, no pagination flag, `grep -c .` over multi-line messages)
  out of a history two orders of magnitude larger — so the audit could not have seen the older
  duplicate it existed to find. Re-run paginated, the CONCLUSION held, and the two claims were
  separated: the conclusion was true, the evidence posted for it was not sufficient. **Any census
  over an API or a listing states its page size and asserts the total it examined against an
  independent count.** Its clone-side twin the same hour: **a SHALLOW clone answers a CONTENT
  question soundly** (an append-only tip blob carries every entry) **and a HISTORY question wrongly**
  — content from the tip, ancestry and counts from a full clone or `ls-remote`.
  ⚠ **READING AN INSTRUMENT'S CODE TELLS YOU WHAT IT DOES; COUNTING ITS EVIDENCE BASE TELLS YOU WHAT IT CAN
  KNOW — those feel like one question and are two.** Three participants mis-scoped one function in a day,
  each closer: the first reasoned from behaviour and got the DIRECTION wrong ("too narrow, widen it"), the
  second and third read the function and got its LOGIC right and its REACH wrong ("cross-platform"), and
  **the fourth COUNTED what it reads — 203 proof pages, 195 one platform, ONE the other — and found a
  windows instrument wearing a cross-platform name.** Beside it: **a comment that presents a bug as the
  reason to TRUST the code recruits the reader against finding it** (*"X passes only because its test file
  is `//go:build unix` — the Windows page never mentions it"* IS the failure mode; it survived three
  readings because it reads as evidence of care). And **make vacuity VISIBLE before fixing the schema** — a
  rule returning `nil` from three places made "I checked and found no agreement" byte-identical to "I could
  not check", and the visibility change is the instrument that MEASURES the schema fix's population.
  ⚠ **A CORRECT AGGREGATE OVER WRONG COMPONENTS is a shape distinct from a false empty and from a vacuous
  green, and invisible to every total-based check.** A `sed` written for a PATH mangled a by-package table's
  KEYS **and the table still summed to 37** — does-it-sum, does-the-count-match and does-the-funnel-close
  all PASS, because a relabelling preserves the sum. **Only reading the ROWS against what they should SAY
  can catch it: verify the NAMES, not only the number.**
  ⚠ **AN ASSERTION LINE CARRIES ITS PREDICATE, NOT A SHORT LABEL**, or it is re-derivable only by
  whoever still has the script (2026-09-08): a recorded declaration count of zero was "re-derived" as
  four by grepping FIELDS typed by the type, where the record's predicate counted TYPE DECLARATIONS
  of it — a correction to a CORRECT record, half-written before it was caught. **Publishing a
  correction to a correct record spends the fleet's trust in the record and sends the next reader to
  re-verify something settled.**
  ⚠ **And that strict pattern stays SPLIT into TWO numbers — `error CS[0-9]+` and
  `error (MSB|NETSDK)[0-9]+` — never folded into one "errors" count** (2026-09-04): a contention-born
  MSB3030 storm, from a filtered build started INTO a running leg, reads CS 0 / MSB 36 and clears on a
  solo re-run, while a real regression reads CS N / MSB 0 — folding them makes the two
  indistinguishable. Its companion, which is why that storm existed at all: **a lane does not start
  any build into a running gate leg**, however small the guard it wants sooner.
  ⚠ **A LANE OWING A CHEAP LINE — a `go build`, a `go list` — HOLDS IT WHILE A TIMING-SENSITIVE
  MEASURED LEG IS LIVE ON THE SAME BOX** (2026-09-07): a perturbed verdict count is indistinguishable
  from a real one afterwards and would be scored against a prediction as if clean, and **the arms are
  re-runnable while the leg is not.** Companion: **a figure produced by an instrument later shown to
  read green for the wrong reason** (here `! -writable`, which answers access(2)) **is WITHDRAWN until
  re-measured by the corrected instrument**, never carried forward on the lane's own say-so.
  ⚠ **Two 2026-09-02 refinements from one census that read ZERO against thirteen real sites.** Every nil
  construction of pointer-to-array type in Go 1.23.12 lives in a `_test.go` (reflect 10, runtime/arena 2,
  encoding/binary 1), so the production census of 64 nil-to-pointer conversions found none: **ask the
  `-tests` dimension whenever the motivating site is a test.** Where three derivations disagreed (grep 6,
  an instrument pointed at the grep-NOMINATED packages 11, an independent `go/packages` pass over all std
  packages 13) the disagreement was SCOPE, not predicate: scoping a census with the tool just shown to
  under-report reproduces its blind spot. Then **re-derive the population before any design is cut against
  it**: the 13 split into three tiers (6, 3, 4) and the "most interesting" members were the tier that
  needs nothing — a summary restating a lane's conclusion inherits its unvaried axis, so state what was
  MEASURED, not what was concluded.
  ⚠ **Three more, 2026-09-02, one shape: a property INFERRED from an artifact instead of measured.** A
  census can be exactly right about what EXISTS and exactly wrong about what it MEANS — thirteen
  typed-nil sites counted correctly by two derivations, then classified off the emissions and wrong
  twice (what the named spelling preserves is C# TYPE IDENTITY, not the dimension). **A converter hook
  that FIRES is not a hook that CHANGES the emission**: `getExprContext` returns the FIRST matching
  context, so cargo APPENDED as a second one is unreachable while the instrumentation reads healthy —
  instrument, then DISBELIEVE the instrument's agreement with the emission. And **a utility that exits
  0 with NO output is indistinguishable from one that never found its input**: its zero is a result
  only after a positive control (delete a known line, re-run unchanged, require it byte-identical).
  ⚠ **Three census rules from one shift, 2026-09-02.** **Attribution rides on a caller-supplied TAG,
  never on a stack walk** — a per-admit walk attributed 0 of 14 rows because the frames were inlined,
  while the tag read every row, whatever the walk costs. **A classifier applies the rule its CALLER
  asked**: 70,065 of 70,070 admits came through the marshalling (CONVERSION) callers, so the four pairs
  flagged WRONG under the ASSIGNMENT rule are legal Go conversions — attribute by caller MODE before
  classifying, verify flagged pairs against Go's own predicates, check whether a refusal at a fast path
  is RECOVERED downstream before predicting breakage, and prefer an explicit mode parameter over
  relying on that recovery. **And classify each site by the QUESTION it asks before counting it**,
  because a census can measure an option OUT OF EXISTENCE: 4 of 82 `GetType` uses were raw eface
  type-word comparisons and all four sat in ONE hand-owned file, leaving the converter-emission remedy
  with nothing to emit. A new census is also cross-checked against the HISTORICAL population (70,070
  against 70,071 admits) before its counts are believed.
  ⚠ **A SUBSTRING predicate over converter-minted GLYPH names over-matches BY CONSTRUCTION**
  (measured 2026-09-02): the `Δ`/`ж`/`ᴛ` families are prefixes of one another's identifiers, so
  `ΔHandle` matched inside `ΔHandler` and eight census hits were never real. Anchor on the WHOLE
  alias, or resolve what the name denotes — the alias-census rule above, one layer down. Its
  companion: **"carries the alias" is not "drifts on the other platform"** — only a transpile,
  mtime-verified, answers the class question, and the drift-measured number was one.
  ⚠ **THREE INSTRUMENT FAULTS ON ONE POPULATION IN ONE EVENING, EVERY ONE CAUGHT BY A SECOND
  DERIVATION AND NONE BY RE-RUNNING THE FIRST** (2026-09-08, a converter-stamp census). (1) **A
  word-boundary escape before a MULTI-BYTE GLYPH does not match in this box's grep**: the same
  alternation without it read 51 names where the anchored form read 3, a 17x UNDER-count — anchor
  converter-glyph patterns on whole tokens by other means, and positive-control every glyph-keyed
  pattern on a planted line before believing a small number. (2) **A control drawn from the EASY case
  certifies the easy case**: a census keyed on the FILE where the population is keyed on the TYPE
  reported almost every stamp mangled, because most stamp-bearing files declare no members at all —
  and BOTH control arms passed because the control file happened to declare its own, the one shape
  the defect cannot reach. (3) **A `*.cs` pathspec EXCLUDES every `*.cs.auto`** — the converter's own
  output for a hand-owned file, and the one place a mangled stamp could sit at master — so a census
  read ZERO where the only instance lived. Two independent instruments produced two DIFFERENT wrong
  predicates on one population, and a file-count disagreement is what found the third. **An absurd
  number is the cheapest positive control there is, only if you stop for it**, and a footprint
  prediction built on such a zero is REPLACED before the run reports, with both falsifiers stated.
  ⚠ **And a census of an emitted SPELLING under-reports by every spelling it did not enumerate**
  (2026-09-05): a retention census counted `FromPinnedBox(` and missed `FromBox(` — the
  reference-bearing spelling, which is exactly where the token-route boxes that most need retention
  live — and missed pointer-typed VARIABLES entirely, while the converter's own predicate is
  `go/types`' and reached them all. The alias-census rule one spelling over, with a sharper
  corollary: **a guard keyed on the SAME spellings as the census is not a second derivation** — it is
  the census run twice, and it shares the blind spot.
  ⚠ **So an EMISSION grep is an UPPER BOUND that misses every spelling it did not think of, and a
  census keyed on ONE MINT OR HELPER is a LOWER BOUND on that shape** (both 2026-09-05). The first:
  173 `heap(new T(), out var)` sites against **292** once `ref var n = ref heap<T>(out var)` was
  counted — the population is a `go/types` census over the SOURCE, per flavour, positive-controlled on
  a probe carrying one site per kind plus the excluded shapes; and the DIFFERENCE between the two
  numbers (396 address-taken scalar locals against 232 boxed on one flavour) is itself a MEASUREMENT
  of an existing capability's reach — 164 already kept unboxed by parameter ref-lowering, read for the
  first time as a by-product of sizing the next one. The second: twenty sites resolved through a
  pinned-box mint missed the emission that took FOUR banked rows down, because a plain `(uintptr)`
  conversion of a heap box is a SECOND DOOR into the same class and never touches the helper. **A
  helper-keyed count answers "how many go through THIS door" and READS as "how many exist" unless the
  record says otherwise.**
  ⚠ **Five census rules from 2026-09-04; the first two are about how a ZERO and a LEG are believed.**
  **A NARROW census's zeros are believed only because the SAME detectors read NON-ZERO on the BROAD
  population in the SAME run** — three exclusions predicted at ~30 combined came back at exactly ZERO
  over one capability's 220 sites, and were trusted because those same detectors read 19, 45 and 74
  across the 859-site population beside them: the broad population IS the narrow census's positive
  control, since a dead detector reads zero on both. The prediction's own inversion is the other half
  — the row named as the biggest exposure came back 14 against a reasoned ~15, while the three rows
  merely ASSUMED were wrong by their whole size — so **hedge what you assumed, not what you reasoned
  about, and let a prediction table say which rows are which.** **A TWO-LEG census is scored from TWO
  legs or it is not scored**: one leg read a headline row unmoved and a falsification went out with an
  honest scope caveat naming the unread leg — which is where the answer was — and a scope caveat tells
  the READER a finding may be wrong without telling the AUTHOR. Read every leg before scoring, and
  post the correction the moment the second leg reads. **A ratio reading ABOVE ONE is the instrument
  saying its counters measure different POPULATIONS**: one counted slot-allocating standard-box
  constructions while the other counted pins taken through the `ж<T>` base and so fired for element-
  and field-reference boxes too (183%, 116%) — the remedy is per-kind attribution, positive-controlled
  with the ZEROS PRINTED (7 takes → standard 7, elem 0, field 0) before any row is read. **A count's
  POPULATION is named by TYPE and by STACK before a design's gate row or its cost canary is chosen**:
  a measured population of timers and sockaddr field boxes — consumers, never victims — sat in a
  different class from the one the falsifier named, which moved the gate row, moved the canary, and
  exposed a DEAD TAKE whose result nothing reads (**a take whose number nothing reads is DISPLACED,
  not registered**). And **an instrument built out of the thing under test is refuted by its own
  TOTALS**: a consumer-side test of a recovered box's `IsNative` to drop stale label pointers dropped
  ALL 91 labels, because `IsNative` is the NORMAL state of that number and not evidence of staleness —
  caught only because the guard PRINTED its warning count (182 drops where at most one was expected).
  There is no cheap consumer-side test separating a live address from a dead one; the honest form
  WITHHOLDS the half it cannot guarantee and writes the witness into the hand-own.
  ⚠ **A STATED CAVEAT IS NOT PROTECTION IF IT NAMES THE WRONG HAZARD** (2026-09-07): a lane hedged
  "read 19 and 525 as FLOORS" on the reasoning that subtests and benchmarks push a count UP, while the
  DOMINANT effect pushed it DOWN by 81 — build-constrained files. **A regex over source counts every
  platform; `go test -list` under the pin counts the BUILT set**, and the two are reconciled to zero
  residue (81 + 444 = 525) rather than hedged. A hedge pointed the wrong way reads as care and leaves
  the reader more confident than the number deserves.
  ⚠ **A CENSUS THAT CONTRADICTS A COMMITTED RECORD MUST STATE THE PRIOR VALUE AND THE DIRECTION OF THE
  MOVE, OR IT PUBLISHES A REGRESSION AS A CHARACTERISATION.** A row recorded at master as "5 of 15" was
  re-measured as 15 verdicts with an empty converted side and reported as a fresh reading — prior value
  unstated, direction unflagged, roster untouched — **leaving two records at master disagreeing about
  one row, with the newer framed as a cheap-fix candidate rather than a regression somebody must root.**
  Acknowledging that a prior reading is "stale" is not the same as saying what it SAID.
  ⚠ **`git ls-tree -r` RECURSES INTO SUB-PACKAGES, so a per-package artifact census reads the
  CHILDREN's files and looks healthy.** `os` read **25** test artifacts recursively and **0** in its own
  directory, 12 of the 25 belonging to `os/exec` (2026-09-06). **Census a package's own directory
  NON-recursively, and use a SIBLING package as the control** — the sibling is what makes a zero legible
  instead of looking like a parse failure. ⚠ And **a gap found in ONE row is censused across the
  population before it is called systemic — or isolated**: `os` banked with no proof page, no index row
  and no committed test sources, and the census over all 204 banked rows found **exactly one** such row.
  That is the difference between "the commit policy quietly stopped being applied" and "one row's record
  was skipped at bank time" — a different remedy and a different level of alarm, for one cheap command.
  ⚠ **And the publishing-side twin of the orphan-check rule: A HOST THAT DIES BEFORE PRODUCING ANY
  VERDICT HAS MEASURED *NOTHING*, NOT ZERO.** Characterising that same row as "15 verdicts, converted
  side entirely empty, host dead before any verdict" against a committed "5 of 15" reads to the channel
  as a **5 → 0 regression**; the true statement is **5 → UNMEASURED**. The skipped first move is already
  written down — read the results-file TAIL before ANY shape analysis — and **on a row behind a known
  capability frontier a throwing managed body is the EXPECTED cause and the tail says so**, which makes
  the row a frontier question rather than a defect.
  ⚠ **Two more, 2026-09-05, both about WHICH population a census counted.** **A defect measured at
  ONE SPELLING can have a SECOND LAYER at another, and the population can live ENTIRELY behind the
  second**: an empty-composite-literal fix was correct for its spelling and moved ZERO sites, because
  all five std needy named arrays are built by the ZERO VALUE through the wrapper's lazy backing —
  so **census the CONSTRUCTION SITES at the Go source before choosing the layer to cut**, and say the
  expected zero out loud before an empty diff can read as a pass. (A census instrument reading 0 on
  the probe module KNOWN to contain the shape is a broken loader, repaired before any number is
  believed.) And **a class census reports EMITTED sites and REACHED sites as TWO numbers** (21 / 2):
  the REACHED subset is what a cut displaces now under the population-of-one rule, while the class
  remedy is RECORDED with the census as its sizing and BUILT when a second row reaches a third site.
  ⚠ **Two more, 2026-09-05, and the first is why a GOROOT census and an EMISSION census are different
  numbers.** **A census over GOROOT with `go/packages` counts types the EMITTED corpus never carries**
  — `-stdlib` defaults to `-tags purego`, so asm-path declarations (nistec's `p256AffineTable`) are
  absent from every GOOS folder and reference-element variants replace struct ones, which is how a
  five-site GOROOT census read TWO latent sites in the corpus. **Census the EMISSION, every per-GOOS
  folder**, cross-check against an independent count, and positive-control the instrument on a shape
  known present before believing its number. Its selection corollary: when a defect appears at N site
  KINDS, **measure which EMISSION each funnels through before choosing a carrier** — 13 of 15 divergent
  lines passing through one generated property decided a carrier over a converter patch covering four.
  And **a census arm that runs over a HAND-TYPED SUBSET is not a census arm**: it reports on the
  population while testing the sample, and the shortfall is invisible because every row it prints is
  TRUE (a by-address remediation arm asked 9 of 38 members, so 29 were never asked; the sibling arms,
  keyed on the converter's OWN recorded decision plus a field scan, stood).
  ⚠ **Four census rules from 2026-09-06.** **The strongest answer to "is this helper sufficient" is
  "the shape set is CLOSED", and a LANGUAGE FACT can prove it where no census can**: Go declares the
  socket-address interface with an UNEXPORTED method, so no package outside its own can implement it —
  three arms in the writer, three method definitions in the tagged sources, three implementation
  records in the converted corpus, all agreeing. Look for the closure proof before counting instances.
  **An EMPTY census is actionable only in its STRONG form — what the population DOES, not what it
  lacks**: a caller-side by-address census found every one of four hazardous caller behaviours PRESENT
  and repeated across 43 reference-bearing sites of 80, each answered by a named mechanism the
  companion was written against; "I found nothing" would have closed nothing. **Any claim about a SET —
  a map's members, a registry's population, a census's rows — is derived from the WHOLE construct by an
  enumeration that PRINTS A COUNT, and the count goes beside the members wherever the claim is made**:
  a fixed-context search window is a window with a NUMBER on it and a number looks like completeness,
  which is how a registry was enumerated from a 25-line context window showing six of its eight rows.
  Two of that evening's three same-shaped failures would have died at the count step, because six and
  eight are visibly different numbers, where a resolution to be careful would have caught none of them
  — the head-limited status check's family, one layer up. And **a comparison whose SELECTOR matched
  nothing is not a comparison, and it fails as a FALSE POSITIVE rather than as an error**: a row-lookup
  pattern matched nothing and the fallback silently hashed the WHOLE FILE, so two refs' hashes differed
  for reasons that had nothing to do with the row, caught only because they differed where they should
  not have. Redo such a comparison by locating the target on each side's own content.
  **Scope DELETES by lane prefix too, not just writes** (measured 2026-08-16: a lane's cleanup swept
  the whole shared scratchpad and unrecoverably deleted sibling lanes' artifacts). A cleanup command
  must name your own `<lane>-*` files; `Remove-Item <scratchpad>\*` is a cross-lane destructive act.
  **⚠ Killing `go2cs.exe` alone ORPHANS its `dotnet run` child and the test host under it**, which
  keeps `runtime.dll` locked — the NEXT pipeline run then fails MSB3027/MSB3021 and its comparison
  reports `Go="pass" C#=""` for every test, reading exactly like total conversion failure (measured
  2026-08-16, cost one invalid run). It is a file lock: kill the process TREE by verified parentage,
  then re-run before believing any mass-empty verdict.
  **⚠ `dotnet build-server shutdown` is ALSO machine-global** (found 2026-08-03: one lane's startup
  cleanup yanked the shared MSBuild servers out from under a sibling's in-flight compile — same
  truncated-log signature, no Stop-Process anywhere). While sibling sessions may be building, do NOT
  run it; isolate your own builds instead (`MSBUILDDISABLENODEREUSE=1`, `-p:UseSharedCompilation=false`)
  and reserve `build-server shutdown` for solo contexts or coordinator-owned quiet points. The repo's
  own instruments are safe by default since 2026-08-08 (`db427e6e9`): `run-behavioral-tests.ps1` runs
  its shutdowns only under an opt-in `-ShutdownBuildServers` switch, and `AssemblySetup`'s teardown
  honors the same env-var contract — the hazard that remains is the ad-hoc, hand-typed invocation.
- **⚠ The three-run flake standard, and the A/B a re-converting SWEEP silently invalidates
  (2026-09-01/02).** A row that fails once is not a finding: the standard is **fail-WITH the change,
  pass CLEAN, pass again WITH the change restored** — three runs, in that order, before anything is
  attributed to a commit. The strong form is what costs lanes: **reverting the `.cs` is not an A/B
  when the instrument re-converts.** `run-validated-sweep.ps1` re-emits from the LIVE binary, so a
  hand-reverted corpus file is overwritten before the row runs and both arms measure the same
  converter — which is exactly what happened on the h2 deadline rows, identical signatures on both
  sides reading as "the change is innocent". Swap the **PRESERVED pre-change `go2cs.exe`** into the
  sweep path instead, and state which binary each arm ran.
  ⚠ **An A/B ARM that names a ref IS a ref derivation, and inherits the stale-ref rule** (2026-09-02):
  a Linux clone that had only ever fetched its own branch checked out `origin/master` at a commit
  predating the corpus hop, and the sweep's toolchain-pin guard refused it ("version.props pins
  1.23.1 … NOT MEASURED, never a verdict") — the guard caught it, the lane did not. Any arm naming a
  ref runs `git fetch origin <ref>` immediately before the checkout and PRINTS the resolved SHA; and a
  mid-run checkout makes a count belong to a tree nobody named, so check WHICH TREE a run measured
  before believing its number (a sweep measuring a 13-entry tree while the branch moved under it read
  exactly like a silently-rejected disclosure).
  ⚠ **An instrument that FAILS IDENTICALLY on BOTH ARMS confirms whatever was predicted**
  (2026-09-04): four legs called the converter DIRECTLY on a Linux host, bypassing the `GoTargetOS`
  pin and linking the WINDOWS dependency set (phantom CS0426, exit 1, no comparison record), so both
  arms read 0 matched / 0 diverged — and a prediction of "unchanged within noise" is CONFIRMED by two
  zeros. The only tell was a `record=` column in the lane's own verdict line reading EMPTY. **An arm
  proves it MEASURED something — a comparison record present, carrying the row's verdict count —
  before its number is compared to the other arm**, and the invalid run's logs are kept as wreckage
  rather than deleted.
  ⚠ **Stated generally 2026-09-05: A CONTROL INHERITS ITS INSTRUMENT'S DEFECTS.** Two arms both run
  without the `GoTargetOS` pin on Linux both linked the WINDOWS flavour and failed identically — which
  is valid evidence for "not caused by the cut" and WORTHLESS for "pre-existing at master", and a
  finding minted on the second reading was withdrawn. **Name the FLAVOUR an arm linked before believing
  a pre-existing.**
  **SUBTRACT THE DISCLOSED, THEN COLLAPSE TO ROOTS.** `reflect` read 65 differing at master; 60 were
  already DISCLOSED, 5 were not, and the 5 collapsed to **THREE roots** — one manifest entry (which
  closes TWO rows, because entering a missing leaf makes the parent ride the disclosed-parent
  aggregation), one delegate arm (two more rows, both its motivating cases), and one row belonging to a
  different lane entirely. **Read it off the comparison RECORD rather than running anything, and ask for
  ROOTS rather than for the count** — the two natural guesses, "five roots" and "sixty-five things", were
  both wrong.
