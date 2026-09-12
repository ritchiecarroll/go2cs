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
     context, so provenance kept this way costs ZERO tokens.
     PHASE 2 APPLIED 2026-09-12. Every dated narrative from the Phase-1 text survives here in a
     comment beside the rule it justifies; families that described one trap from several angles are
     merged into one rule with all their narratives beneath it. Route numbers #6-#8 are normative and
     cited by number elsewhere in the repo; their identity is untouched. Nothing was deleted -- every
     date, SHA, file:line, measured number, package name and named incident was MOVED. Comments are
     attached to the END of a rule's last line so stripping them leaves no residue. -->
## False-green routes #6-#8
Routes #1-#5 (stale binary, stale output, an unenumerated package, an old front end) live in
`.claude/rules/converter.md`: each RUNS a gate and measures the WRONG thing, so its verdict is merely
untrue. #6-#8 are other shapes, and re-running catches none of them.
### #6 -- an instrument that cannot find its own runner measures NOTHING and prints a pass over the hole
- **Tell: implausible speed** — `run-performance.ps1` died in 20 seconds having run nothing, where a real perf
  run is HOURS. **No standing gate can see it**: a wrapper's only preflight is the `dotnet build` exit code and
  **the build is genuinely green**, so no phase fails and nothing is compared against a golden.
- **DERIVE every path component — a hoisted literal is still a literal.** `$NetVersion` comes from
  `src\Directory.Build.props`'s `<TargetFramework>` by file-read-plus-regex with **no fallback**; **every
  wrapper ASSERTS its runner EXISTS** and `exit 1`s naming the path. <!-- Found 2026-08-24 by two lanes from
     opposite directions; closed the same day. src\_paths.ps1 held the corpus TFM as a literal ($NetVersion =
     'net9.0'), the TFM census's Class-D hoist that had gathered nine hardcoded sites out of six files into
     that one line; on a net10.0 tree every consumer composed bin/Debug/net9.0/, which does not exist.
     run-behavioral.ps1 fails loudly instead — the quiet one is the hazard. Derivation reads the props file
     with no MSBuild and no dotnet (the module is dot-sourced by every instrument on every invocation),
     comments stripped so the props file's own prose cannot be read as the property; an instrument that cannot
     know its TFM throws, naming the file. Spelling net10.0 in is the tempting fix and re-breaks at the next
     hop — this is what docs/DotNetMigration.md means by "derivation, not replacement", and it makes
     migrate-tfm.ps1 honest, since that instrument carries no site for _paths.ps1 because its census already
     believed the PowerShell probe derived. Guards landed in run-behavioral.ps1, run-performance.ps1 and
     run-performance-floor.ps1's bflat arm — that last runs at 'Continue', so a missing compiler would
     otherwise report ok off a stale $LASTEXITCODE. The explicit exit 1 rather than throw is because the exit
     CODE is the property that matters and a throw leaves it to the host: on Windows PowerShell 5.1 the
     missing-runner path already exits 1 (measured, both wrappers, -File and -Command), so the exit-0 sighting
     is a host- or wrapper-dependent swallow. -->
### #7 -- a `go2cs-gen` (analyzer) change is invisible to EVERY standing gate except a behavioral COMPILE
- **Any change under `src/gen/` owes a full behavioral COMPILE phase and one cross-assembly consumer gate
  before banking**: CNR is transpile-only, the stdlib solution compiles ONE assembly at a time (`internal`
  binds fine same-assembly), and the 307/0 + CNR-byte-identical ladder proves NOTHING. **"651 suspects" means
  "one project is red", not "the corpus is broken"** — Transpile rewrites every `.cs` first, so no assembly is
  up-to-date and one red project attributes everywhere. <!-- Found 2026-08-30, the W3a promoted-forwarder
     regression; fixed 0df5a3f2b. The W3 merge demoted net's public TCPConn.Read/Write promoted forwarders to
     internal and shipped green on exactly that ladder; caught days later by a derived net/http canary sweep,
     the only union gate that compiles a cross-assembly consumer of metadata-promoted surface. The compile
     phase is the slnx-dev build or the runner's Compile. Attribution corollary paid the same night and
     measured: exactly 1 Release assembly written corpus-wide vs a clean 78-project filtered batch. -->
### #8 -- a guard DISARMED by a LEGITIMATE change
- **Nothing is stale and nothing mis-runs: the guarded property moved house**, and a guard asserting an
  assembly-level property by grepping ONE emitted file goes silently VACUOUS **in its negative arm** — the exit
  code says two failures where the damage is four assertions, and glob-widening cannot fix it when a DRIVER
  writes the artifact. **Assert the DECISION the pass records; re-check a negative arm whenever the construct
  relocates.** **Its sharper form KILLS the suite instead of going vacuous and its verdict rides CLASS ORDER**:
  **a host parses its OWN args and NEVER mutates a process-global that converted tests read.** <!-- 2026-09-01,
     the init-hook relocation; the recorded decision was packageImportInits. "The bare form must not appear" is
     trivially satisfied by a file that no longer holds the construct at all: the positive direction fails
     loudly and gets fixed, the negative just stops testing. Sharper form measured 2026-09-02, GolibTests: the
     premise "GolibTests does not reference converted flag" was disarmed by a later ProjectReference, after
     which converted flag.Parse() parsed MSTest's OWN command line through the process-global flag.CommandLine
     (ExitOnError -> os.Exit(2)) unless a sibling class that replaced it with ContinueOnError ran first: one
     host's 460/460 was a lucky ordering, another's 82-then-abort the same defect. os.Args feeds sync's
     TestMutexMisuse, flag's TestExitCode and every self-re-exec. A divergence STATED against the ruling is how
     to diverge. -->
## Guards: assert the DECISION, never the emission
- **Every text-grepping guard fails the same way and takes the same cure.** A guard over emitted `.cs` goes
  false-RED on a hand-own's HEADER COMMENT quoting the pre-fix emission; a census of EMITTED TEXT cannot
  recover a converter decision; an under-scoped PATTERN cannot go red where it matters; a marker guard reads
  the PROSE explaining the marker and stays GREEN with the classifier DELETED; a substring "still consulted"
  check passes a rewording CONTAINING the original (`GoArchExclusiveXX`). **Derive from `go/types` plus the
  pass's own selection map; extract each CONSUMER's LIVE regex, REJECT `-notmatch` exclusion lines, require it
  to MATCH a real marker line and REJECT a prose decoy.** <!-- Measured 2026-09-03/04. (1) The false-RED guard:
     strip comments, or assert the recorded decision. (2) `[GoRecv] … this ref T` matches 5,686 declarations
     because it is ALSO the emission for every Go VALUE receiver on a struct, so a grep for "pointer-receiver
     primaries" reads the wrong population by an order of magnitude; a claim nearly carried on such a grep was
     retired by the census before it reached a design. (3) The keep-alive census's `syscall.`-qualified pattern
     scanned 83 of 351 protected sites and, over the syscall package alone, reported ZERO temps — tripping its
     own vacuity check — so an injected defect there was INVISIBLE. (4) Every instrument that classifies a
     converter stderr line carries a comment naming that line, so a strings.Contains guard stayed green through
     deletion twice until its own positive control said so; -notmatch exclusion lines carry the marker's words
     a second time and would read fine with the classifier gone. (5) The glyph-substring over-match inside a
     guard whose entire purpose is to notice rewording. Companion at the CNR: a check-only switch that empties
     the measurable set AFTER the skip block prints is what lets a live classifier be controlled without a
     transpile. -->
- **A guard whose EVIDENCE BASE is wider than its QUESTION cannot go vacuous and cannot fail to fire — it fires
  TOO OFTEN**: derive scope from the directory the manifest was loaded from, arm written RED-FIRST. <!-- 2026-09-07,
     7adfbeb45: hostFatalMintViolations globbed every proof page, so runtime's TestEmptyString was refused
     because encoding/json's page passes. Its own suite could not see it because every fixture used ONE
     package, so the package axis was never varied — "a control only tests the axis you varied", read on a
     guard's INPUT POPULATION rather than on an A/B arm. The cross-package arm, written red-first, reproduced
     the measured case by name. -->
- **A CONVERTER ATTRIBUTE that RECORDS a decision IS a census of its own population** and beats any later
  predicate; **two open items sharing a population are ONE class**. **Merge ORDER decides a guard's verdict**
  (an arm can be RED-BY-DESIGN at a tip and green on the union — measure on the ASSEMBLED tree); **RUN a guard
  before reporting its colour**; **a guard that documents a defect it does not ASSERT is PARKED**, landing with
  its arc, registration reverted. <!-- 2026-09-05: [GoValueClone] marks every struct carrying a fixed-size
     array field (or another struct that does), so the READ-side defect (a *[N]T cannot be VIEWED over native
     memory) and the WRITE-side defect (such a struct cannot be PASSED to the kernel by address) share ONE
     population: 493 attributes across 95 package metadata files plus 47 in 27 other files, both controls run;
     point remedies are debt against the model fix, named as such in its design record. Arm 2's 12 sites were
     exactly what a sibling seat fixed; a static "red at master" claim was falsified by running it, clean
     83/83. The PARKED rule measured 2026-09-03, the nine-shape dims guard and the canonical-identity
     tripwire: a commented-out row reads to the next reader as coverage. -->
## Positive controls, and the false-empty family
- **ALWAYS positive-control a census before believing a ZERO** — run it over a target KNOWN to contain the
  shape and require the expected count. **An instrument that cannot fail reports success over a hole.** **An
  implausible result is the cheapest positive control there is, and only if you STOP for it**; a merely
  surprising one gets none, so RE-DERIVE rather than notice.
- **Known false-empty generators here, all exiting 0:** instrumentation that never compiled in (tells: binary
  BYTE-IDENTICAL IN SIZE, `grep -c SPREADCENSUS <binary>` = 0); `grep -P` ("-P supports only unibyte and UTF-8
  locales"); Git Bash `grep -F -i -f` with a trailing-backslash pattern (rc 134); a POSIX bracket expression
  eating `\[`; a case-sensitive match; LF anchors against CRLF; a UTF-16 redirect; a non-verbose `go test
  ./...`, which prints `ok` and NO test names. **Patch converter sources with the Edit tool; `set -euo
  pipefail`, never bare `set -u`; use `rg`/Grep; run guard-count legs as `-v -run <names>` with their `=== RUN`
  lines as the control.** <!-- Instrumentation: measured 2026-08-28, the defer-multivalue-spread lane. A
     type-aware census patched an fmt.Fprintf(os.Stderr, …) marker into a converter helper via a heredoc python
     script, ran -stdlib into a seeded temp root and counted marker lines: ZERO hits across the stdlib, and the
     run looked entirely healthy — the stderr carried the normal spread of converter WARNINGs, proving the
     conversion had really traversed the corpus. The converter's .go sources are CRLF in the working tree; the
     script's anchors were LF ("\treturn tuple.Len()\n}"), so they matched zero times and python's assert fired
     — but under set -u rather than set -e execution CONTINUED and built an UNINSTRUMENTED binary. A script
     that must exist reads/writes with newline='' and anchors on CRLF. The lane's control fired 12/12 on the
     behavioral guard's spread rows and stayed silent on its two controls, which is what made the real — also
     zero — production-corpus reading trustworthy. grep -P: a false-empty census that nearly got banked, stderr
     discarded. grep -F -f: 2026-09-05, no stdout at all. Bracket expression: 2026-09-04, a GOROOT population
     of zero, an artifact until the pattern is made to FIRE on a probe known to contain the shape. Case: a grep
     for "the TABLE, as asked" against a file containing "THE TABLE, as asked" returned ZERO, read as mailbox
     content being LOST, and a fleet-wide integrity alarm was half-written before a second instrument showed
     the entry present five times. go test: 2026-09-04, "named-guard-results=0" was the INSTRUMENT, not the
     guards, which ran 11/11 in a filtered verbose run whose control is its eleven === RUN lines. -->
- **A broken instrument producing a PLAUSIBLE FULL COUNT is worse than one producing a ZERO** — it confirms the
  wrong branch and talks its author out of a right diagnosis. **A zero invites suspicion; a plausible total
  recruits the reader.** The tell is arithmetic. <!-- 2026-09-08, the collapsed $'…' pattern one door over: it
     counted every line and reported an LF-only file as fully CRLF. Tell was 1,077 of 1,078 — off by one on an
     "every single line" claim, strong enough to deserve one check. Measured properly: 1,078 lines, ZERO ending
     in a carriage return. -->
- **A positive control that reads ZERO twice may have a broken CONTROL, not a broken pattern** — an arm that
  will not fire is not evidence about the target. **A PHRASE-MATCH against a HARD-WRAPPED file reports FALSE
  ABSENCES**: cut each "missing" phrase to a distinctive FRAGMENT and re-search. **A file's contents RECALLED
  instead of READ is the same failure with no instrument in it** — judge a MIXED report by its VERIFIED half,
  then ask about the rest. <!-- Control: a nine-pattern identifier census had eight arms fire on planted lines
     while the UNC arm read 0 twice, first because printf aborted on a capital-U escape, then because a heredoc
     collapsed the doubled backslash to one; both failures were in the PLANTING, and re-planted through chr(92)
     the arm read 1 on the planted file and 0 on the real ones. Phrase-match, 2026-09-06: an
     anchor-verification pass over 28 doctrine entries reported twelve missing anchors and every one was false
     — this file wraps at ~100 columns, so a quoted sentence spans a line break and grep -F can never match it,
     one anchor missed on the single word ending the previous line; a second cause rode along, the draft
     quoting markdown with different emphasis placement. Eleven of twelve resolved on the first shortening.
     Recall: one session produced false empties from a case-wrong grep, a $ anchor against CRLF, a suffix
     match, an unresolvable ref and a cp1252 decode — and one confident wrong claim from memory alone, same
     confidence, same wrongness; the same post also positive-controlled, correctly, that a train head was
     absent from origin, so dismissing the whole would have discarded a true finding. -->
## Instruments that cannot fail: shell and patching traps
- **A `sed` that matches nothing and a `grep` that matches nothing BOTH EXIT 0 and both look like the work
  being done**; **deriving a script from the previous one by pattern substitution is a SILENT-NO-OP GENERATOR
  whose error COMPOUNDS across generations. ASSERT THE SUBSTITUTION CHANGED SOMETHING** — `sed`'s exit code
  cannot tell you. <!-- One case returned zero and read as content LOSS; the other returned success and WAS
     content loss — five distinct mailbox posts silently replaced by a copy of an older one, subject and body
     both, because a derived script's substitution pattern named a file the target did not contain. One bad
     derivation kept the OLD payload; five further generations were derived from THAT one, each with a pattern
     naming the payload it believed it was replacing, each matching nothing. Compare before/after, or grep the
     result for the new value. -->
- **A patch fed through a Bash-tool heredoc whose anchor ENDS in a backslash arrives collapsed (`SyntaxError`)
  and the chain CONTINUES past the dead patch**: patch with Edit, **assert the substitution landed (`grep -c`
  of the NEW token, >= 1) BEFORE any launch**, gate each step on the previous exit. **`sed -i` on a CRLF file
  NORMALISES line endings even when the substitution FAILS** — a second axis from a failed edit. <!-- 2026-09-07:
     the chain continued into bash -n, cp and the launch, so a train ran a THIRD time against the unpatched
     script and read the identical vacuous leg. sed -i, 2026-09-08, caught only by a line-ending census across
     every probe arm; same run, a BLANK import emits no `using`, so an arm built on one silently tests nothing
     about the alias — detected by the MISSING line, never by an exit code. -->
- **A `grep -c` whose pattern is a `$'…'` carriage-return literal inside a command substitution degenerates to
  `$` and returns the LINE COUNT**, so a CR == LF check written that way can never go red: **count CR as BYTES
  (`tr -cd` | `wc -c`), positive-controlled on an LF-only probe reading 0.** <!-- 2026-09-07, found
     independently by two sub-agents in one night: one reading 647 CR on a zero-CR file, one measuring an
     assemble script's own stamp. Every doctrine-landing structural stamp since the idiom was written was
     VACUOUS. The working-tree CLAUDE.md re-measured that way reads CR == LF, so the past stamps happened to be
     TRUE while unable to be false. -->
- **QUOTE THE HEREDOC DELIMITER — `<<'EOF'`, never `<<EOF` — and a BACKTICK inside a DOUBLE-QUOTED bash
  argument is a COMMAND SUBSTITUTION**: write verbatim content with a quoted heredoc or the Write tool, pass
  code-carrying subjects through single quotes or a file, READ BACK any pushed subject whose command printed a
  shell error, and use `chr(92)` for a literal backslash by default. <!-- The delimiter rule cost one
     coordinator THREE failures in one night (a python escape collapsing to a syntax error twice, and a
     doctrine item mangled at the moment of banking a rule about that very trap) and cost a lane three lost
     citations in a pushed post; two independent participants in one session makes it a convention rather than
     folklore. The backtick twin, 2026-09-08: a post tool's subject had its backticked span EXECUTED and
     DROPPED (command-substitution syntax error, then a not-found), while the entry BODY — written through a
     QUOTED heredoc — stayed intact. -->
- **PowerShell built-in aliases OUTRANK functions**, so helpers named `LP`/`H` are NEVER CALLED (`lp` =
  Out-Printer, `h` = Get-History) — run `Get-Alias <name>` first; **`Write-Host` goes to the INFORMATION
  stream, so capture with `*>&1`**, a bare `2>&1` dropping every `==>` line so the log reads as hung. **And a
  fixed-size `tail -N` over output whose length GROWS with the run is not an instrument** — a PASSING row reads
  as never having run, and exit 0 cannot distinguish: **read verdicts from the proof page the sweep rewrites
  (`docs/validation/current/<row>.md`) against the roster, using its mtime to tell swept from stale. `| head`
  is a silent WHERE clause.** <!-- 2026-09-03, PS 5.1, same lanes: the "long-path-safe" comparer returned $null
     for every path and null-vs-null read as a clean True; only the positive control surfaced it. The sweep
     prints drift AFTER the verdict and the drift list outgrew the tail. -->
## Second derivations: an instrument built out of the thing under test
- **An instrument built out of the thing under test cannot independently measure it; the corrective is a SECOND
  DERIVATION, chosen by asking what the FIRST's blind spot is.** Two derivations that AGREE share a blind spot;
  two that DISAGREE tell you where it is; **two landing on the same PARTITION is the strongest cross-check
  available.** **A guard keyed on the SAME spellings as the census is not a second derivation, and a CONTROL
  INHERITS ITS INSTRUMENT'S DEFECTS** — name the FLAVOUR an arm linked. <!-- General form named after five
     instances in ONE day, 2026-09-01. A ptrout-class population read off the CONVERTED CORPUS saw 6, off GO'S
     OWN SOURCES 16 — the gap is the finding, not an error in either, since a function can be in the class
     without its emission showing the shape the corpus-side probe keys on. Two agreeing parsers that both read
     text and both honoured ^ both matched a raw string literal's contents; GOROOT's own source settled it.
     Partition cross-check: a run's stack trace and a directive census both split runtime/pprof into 1 + 6 = 7,
     name for name — unlike two runs of one instrument, which prove only determinism. Other instances: a probe
     keyed on its own incomplete predicate reports the predicate; a probe blind to unwired slots reports the
     wiring; a type-name census was believable only once a go/parser derivation reproduced it, and a
     classifier's 66 only once an independent predicate agreed. Control/defects, 2026-09-05: two arms run
     without the GoTargetOS pin on Linux both linked the WINDOWS flavour and failed identically — valid
     evidence for "not caused by the cut" and WORTHLESS for "pre-existing at master"; a finding minted on the
     second reading was withdrawn. A same-spelling guard is the census run twice, blind spot included. -->
- **A `-stdlib` census is BLIND to "does the fix reach the row" when the motivating site is in a `_test.go`** —
  it never writes test emission, so **ask the `-tests` dimension whenever the motivating site is a test.**
  **Name the LAYER a census is attached to**; an empty enumeration inside a redirected log is not evidence of
  absence; **an EMPTY diff after a "fix" is the fix saying it was not needed.** <!-- 2026-09-01 structurally;
     2026-09-02 for the instance — every nil construction of pointer-to-array type in Go 1.23.12 lives in a
     _test.go (reflect 10, runtime/arena 2, encoding/binary 1), so the production census of 64 nil-to-pointer
     conversions found none. LAYER, 2026-09-01/02, two retractions in one night: a claude/g-* lookup over bare
     g-* refs returned a confident EMPTY, and a working-tree line-ending count under the eol=crlf pin was
     reported as a COMMITTED fact — the blob is LF, the checkout makes it CRLF, and `git checkout --` "cured" a
     state that was never in the index. -->
- **Count build errors strictly and as TWO numbers — `error CS[0-9]+` and `error (MSB|NETSDK)[0-9]+`, never
  folded**: a contention-born MSB3030 storm reads CS 0 / MSB 36 and clears on a solo re-run, a real regression
  reads CS N / MSB 0. **A lane starts no build into a running gate leg, and holds even a `go build`/`go list`
  while a timing-sensitive measured leg is live** — the arms are re-runnable, the leg is not. **A figure from
  an instrument later shown green for the wrong reason is WITHDRAWN until re-measured.** <!-- A loose grep -cE
     'error ' scored 1 error on a clean 831-assembly build by matching "internal.oserror ->", and matched the
     word inside Go type names ((…, error)) where a case-sensitive ERROR read 0 against the loose grep's 140
     (2026-09-01). Split measured 2026-09-04; the storm came from a filtered build started INTO a running leg.
     Cheap-line rule 2026-09-07: a perturbed verdict count is indistinguishable from a real one afterwards and
     would be scored against a prediction as if clean; the withdrawn instrument used `! -writable`, which
     answers access(2). -->
- **A census attaches to the DEFECT's boundary — defined by the EXISTING marker set — not the boundary the
  dispatch named.** **A converter hook that FIRES is not a hook that CHANGES the emission**, so instrument then
  DISBELIEVE its agreement with the emission; **a utility exiting 0 with NO output is indistinguishable from
  one that never found its input**; **an instrument built out of the thing under test is refuted by its own
  TOTALS — PRINT the count, zeros included.** <!-- Boundary: a call-argument census missed two
     composite-literal sites the pointer twin's anyBoxedPtrArgs already marks, one of them a live defect; grep
     the marker set first and attach at every site it covers. 2026-09-02, three of one shape, a property
     INFERRED from an artifact instead of measured: getExprContext returns the FIRST matching context, so cargo
     APPENDED as a second one is unreachable while the instrumentation reads healthy; thirteen typed-nil sites
     were counted correctly by two derivations, then classified off the emissions and wrong twice — what the
     named spelling preserves is C# TYPE IDENTITY, not the dimension; and a zero-output utility is a result
     only after a positive control (delete a known line, re-run, require byte-identical). Totals, 2026-09-04: a
     consumer-side test of a recovered box's IsNative, meant to drop stale label pointers, dropped ALL 91
     labels, because IsNative is the NORMAL state of that number and not evidence of staleness — caught only
     because the guard PRINTED its warning count, 182 drops where at most one was expected. There is no cheap
     consumer-side test separating a live address from a dead one; the honest form WITHHOLDS the half it cannot
     guarantee and writes the witness into the hand-own. -->
## Census scope: which population did you actually count
- **AN INSTRUMENT CAN BE PERFECTLY SOUND AND POINTED AT THE WRONG QUESTION — AND THAT FAILURE HAS NO TELL**:
  "which lines LOOK LIKE directives" is not "which lines ARE directives". **CONTROL AN EXCLUSION FILTER IN BOTH
  DIRECTIONS** — its failure mode is silence by construction. <!-- Five entries in a 259-row linkname push map
     were Go source text inside a backticked raw string: inside the literal the lines genuinely DO begin with
     //go:linkname, ^ is true, and the pattern answered the question it was asked — no error, no zero, no
     implausible number, five extra rows indistinguishable from the other 254; corrected to 254 pairs / 253
     destinations. Exclusion: a """-counting state machine desynced on a verbatim string holding an escaped
     quote, flipped into "inside a literal" for the rest of the file, and silently swallowed a REAL directive —
     six exclusions, one false, and nothing in the output said which. Control it with a known-good item KEPT
     and a known-bad one EXCLUDED. The five raw-string rows were found only because a classifier that could not
     resolve their local names SAID SO rather than dropping them. -->
- **PRINT A COUNT and assert it**: a derived set's PARSED ROW COUNT against the population's own total; any
  claim about a SET derived from the WHOLE construct by an enumeration that prints its count; **an UN-PAGINATED
  API call is a silent `WHERE` clause** — state the page size; **a SHALLOW clone answers a CONTENT question
  soundly and a HISTORY question wrongly.** <!-- A canary-set derivation whose row regex missed the roster's
     markdown link wrapper parsed 5 rows out of 204, took the largest number on each surviving line — a URL
     digit — and named a 6-verdict package as the LARGEST reflect-importing row; caught on absurdity alone,
     where the parse count is the cheap guard. 2026-09-06: a registry was enumerated from a 25-line context
     window showing six of its eight rows — a fixed-context search window is a window with a NUMBER on it and a
     number looks like completeness; two of that evening's three same-shaped failures would have died at the
     count step, because six and eight are visibly different numbers, where a resolution to be careful would
     have caught none. The count goes beside the members wherever the claim is made. API, 2026-09-08: a
     duplicate audit reported a commit count that was a LINE count over the 100 most recent commits (a default
     page size, no pagination flag, grep -c . over multi-line messages) out of a history two orders of
     magnitude larger, so it could not have seen the older duplicate it existed to find; re-run paginated the
     CONCLUSION held, and the two claims were separated — the conclusion was true, the evidence posted for it
     was not sufficient. Clone twin, same hour: content from the tip, ancestry and counts from a full clone or
     ls-remote. -->
- **READING AN INSTRUMENT'S CODE TELLS YOU WHAT IT DOES; COUNTING ITS EVIDENCE BASE TELLS YOU WHAT IT CAN
  KNOW**; **a comment presenting a bug as the reason to TRUST the code recruits the reader against finding
  it**; **make vacuity VISIBLE before fixing the schema.** **Verify the NAMES, not only the number** — a
  CORRECT AGGREGATE OVER WRONG COMPONENTS survives every total-based check. **An ASSERTION LINE CARRIES ITS
  PREDICATE, NOT A SHORT LABEL**, and **publishing a correction to a CORRECT record spends the fleet's trust.**
  <!-- Three participants mis-scoped one function in a day, each closer: the first reasoned from behaviour and
     got the DIRECTION wrong ("too narrow, widen it"), the second and third read the function and got its LOGIC
     right and its REACH wrong ("cross-platform"), and the fourth COUNTED what it reads — 203 proof pages, 195
     one platform, ONE the other — and found a windows instrument wearing a cross-platform name. The recruiting
     comment: "X passes only because its test file is //go:build unix — the Windows page never mentions it" IS
     the failure mode; it survived three readings because it reads as evidence of care. Vacuity: a rule
     returning nil from three places made "I checked and found no agreement" byte-identical to "I could not
     check", and the visibility change is the instrument that MEASURES the schema fix's population. Aggregate:
     a sed written for a PATH mangled a by-package table's KEYS and the table still summed to 37 — a
     relabelling preserves the sum, so does-it-sum, does-the-count-match and does-the-funnel-close all PASS.
     2026-09-08: a recorded declaration count of zero was "re-derived" as four by grepping FIELDS typed by the
     type, where the record's predicate counted TYPE DECLARATIONS of it; the correction was half-written before
     it was caught, and it would have sent the next reader to re-verify something settled. -->
- **Attribution rides on a caller-supplied TAG, never a stack walk** (frames inline); **a classifier applies
  the rule its CALLER asked**, so attribute by caller MODE first, verify flagged pairs against Go's own
  predicates, check whether a fast-path refusal is RECOVERED downstream, and prefer an explicit mode parameter;
  **classify each site by the QUESTION it asks before counting it** — a census can measure an option OUT OF
  EXISTENCE; **cross-check a new census against the HISTORICAL population.** <!-- Three census rules from one
     shift, 2026-09-02. A per-admit stack walk attributed 0 of 14 rows because the frames were inlined, while
     the tag read every row whatever the walk costs. 70,065 of 70,070 admits came through the marshalling
     (CONVERSION) callers, so the four pairs flagged WRONG under the ASSIGNMENT rule are legal Go conversions.
     4 of 82 GetType uses were raw eface type-word comparisons and all four sat in ONE hand-owned file, leaving
     the converter-emission remedy with nothing to emit. Historical cross-check: 70,070 against 70,071. -->
- **Glyph and pathspec over/under-match by construction**: a SUBSTRING predicate over `Δ`/`ж`/`ᴛ` names
  over-matches (they are prefixes of one another); a word-boundary escape before a MULTI-BYTE GLYPH does not
  match in this box's grep; **a `*.cs` pathspec EXCLUDES every `*.cs.auto`**; a control keyed on the FILE where
  the population is keyed on the TYPE certifies only the easy case. **Anchor on the WHOLE alias or resolve what
  the name denotes, positive-control on a planted line, and name what each side is KEYED on.** **"Carries the
  alias" is not "drifts on the other platform"** — only a transpile, mtime-verified, answers that. <!-- 2026-09-02:
     ΔHandle matched inside ΔHandler and eight census hits were never real; the drift-measured number was one.
     Three instrument faults on one population in one evening, 2026-09-08, a converter-stamp census, every one
     caught by a second derivation and none by re-running the first: (1) the same alternation without the
     escape read 51 names where the anchored form read 3, a 17x UNDER-count; (2) the file-keyed census reported
     almost every stamp mangled, because most stamp-bearing files declare no members at all, and BOTH control
     arms passed because the control file happened to declare its own — the one shape the defect cannot reach;
     (3) *.cs.auto is the converter's own output for a hand-owned file and the one place a mangled stamp could
     sit at master, so the census read ZERO where the only instance lived. Two independent instruments produced
     two DIFFERENT wrong predicates on one population, and a file-count disagreement found the third. An absurd
     number is the cheapest positive control there is, only if you stop for it, and a footprint prediction
     built on such a zero is REPLACED before the run reports, with both falsifiers stated. -->
- **An EMISSION grep is an UPPER BOUND missing every spelling it did not think of; a census keyed on ONE MINT
  OR HELPER is a LOWER BOUND** that READS as "how many exist" unless the record says otherwise. Census the
  SOURCE with `go/types`, per flavour, positive-controlled. **The DIFFERENCE between two such numbers is itself
  a measurement of an existing capability's reach.** <!-- 2026-09-05. A retention census counted FromPinnedBox(
     and missed FromBox( — the reference-bearing spelling, exactly where the token-route boxes that most need
     retention live — and missed pointer-typed VARIABLES entirely, while the converter's own predicate is
     go/types' and reached them all. 173 heap(new T(), out var) sites against 292 once `ref var n = ref
     heap<T>(out var)` was counted, the probe carrying one site per kind plus the excluded shapes; 396
     address-taken scalar locals against 232 boxed on one flavour, of which 164 were already kept unboxed by
     parameter ref-lowering, read for the first time as a by-product of sizing the next one. And twenty sites
     resolved through a pinned-box mint missed the emission that took FOUR banked rows down, because a plain
     (uintptr) conversion of a heap box is a SECOND DOOR into the same class and never touches the helper. -->
- **A NARROW census's zeros are believed only because the SAME detectors read NON-ZERO on the BROAD population
  in the SAME run**; **hedge what you ASSUMED, not what you reasoned about**; **a TWO-LEG census is scored from
  TWO legs or not at all** (a scope caveat tells the READER a finding may be wrong without telling the AUTHOR);
  **a ratio ABOVE ONE means the counters measure different POPULATIONS**; **a count's POPULATION is named by
  TYPE and by STACK before a gate row or cost canary is chosen**; **a take whose number nothing reads is
  DISPLACED, not registered.** <!-- 2026-09-04, five rules. Three exclusions predicted at ~30 combined came back
     at exactly ZERO over one capability's 220 sites, trusted because those same detectors read 19, 45 and 74
     across the 859-site population beside them — the broad population IS the narrow census's positive control,
     since a dead detector reads zero on both. The row named as the biggest exposure came back 14 against a
     reasoned ~15, while the three rows merely ASSUMED were wrong by their whole size; a prediction table says
     which rows are which. Two-leg: one leg read a headline row unmoved and a falsification went out with an
     honest scope caveat naming the unread leg — which is where the answer was; post the correction the moment
     the second leg reads. Ratio: one counter counted slot-allocating standard-box constructions while the
     other counted pins taken through the ж<T> base and so fired for element- and field-reference boxes too
     (183%, 116%); the remedy is per-kind attribution, positive-controlled with the ZEROS PRINTED (7 takes ->
     standard 7, elem 0, field 0). Population: a measured population of timers and sockaddr field boxes —
     consumers, never victims — sat in a different class from the one the falsifier named, which moved the gate
     row, moved the canary, and exposed a DEAD TAKE. -->
- **A STATED CAVEAT IS NOT PROTECTION IF IT NAMES THE WRONG HAZARD** — a hedge pointed the wrong way reads as
  care. **A regex over source counts every platform; `go test -list` under the pin counts the BUILT set** —
  reconcile to zero residue. **A census contradicting a COMMITTED RECORD states the PRIOR VALUE and the
  DIRECTION, or it publishes a regression as a characterisation.** <!-- 2026-09-07: a lane hedged "read 19 and
     525 as FLOORS" reasoning that subtests and benchmarks push a count UP, while the DOMINANT effect —
     build-constrained files — pushed it DOWN by 81; 81 + 444 = 525. Record: a row recorded at master as "5 of
     15" was re-measured as 15 verdicts with an empty converted side and reported as a fresh reading — prior
     value unstated, direction unflagged, roster untouched — leaving two records at master disagreeing about
     one row, the newer framed as a cheap-fix candidate rather than a regression somebody must root. Calling a
     prior reading "stale" is not saying what it SAID. -->
- **`git ls-tree -r` RECURSES INTO SUB-PACKAGES, so a per-package artifact census reads the CHILDREN's files
  and looks healthy**: census NON-recursively with a SIBLING package as the control. **A gap found in ONE row
  is censused across the population before it is called systemic — or isolated.** <!-- 2026-09-06: os read 25
     test artifacts recursively and 0 in its own directory, 12 of the 25 belonging to os/exec; the sibling is
     what makes a zero legible instead of looking like a parse failure. os banked with no proof page, no index
     row and no committed test sources; the census over all 204 banked rows found exactly one such row — the
     difference between "the commit policy quietly stopped being applied" and "one row's record was skipped at
     bank time", for one cheap command. -->
- **A defect measured at ONE SPELLING can have a SECOND LAYER at another, with the population living ENTIRELY
  behind the second**: census the CONSTRUCTION SITES at the Go source before choosing a layer, say the expected
  zero out loud, and **report EMITTED and REACHED sites as TWO numbers.** **A GOROOT census and an EMISSION
  census are different numbers** (`-stdlib` defaults to `-tags purego`) — census the EMISSION, every per-GOOS
  folder; **measure which EMISSION a defect's kinds funnel through before choosing a carrier**; **a census arm
  over a HAND-TYPED SUBSET is not a census arm**, every row it prints being TRUE. <!-- 2026-09-05: an
     empty-composite-literal fix was correct for its spelling and moved ZERO sites, because all five std needy
     named arrays are built by the ZERO VALUE through the wrapper's lazy backing. A census instrument reading 0
     on the probe module KNOWN to contain the shape is a broken loader, repaired before any number is believed.
     Emitted/reached measured 21 / 2: the REACHED subset is what a cut displaces now under the
     population-of-one rule, while the class remedy is RECORDED with the census as its sizing and BUILT when a
     second row reaches a third site. GOROOT: asm-path declarations (nistec's p256AffineTable) are absent from
     every GOOS folder and reference-element variants replace struct ones, which is how a five-site GOROOT
     census read TWO latent sites in the corpus; cross-check against an independent count and positive-control
     on a shape known present. 13 of 15 divergent lines passing through one generated property decided a
     carrier over a converter patch covering four. A by-address remediation arm asked 9 of 38 members, so 29
     were never asked; the sibling arms, keyed on the converter's OWN recorded decision plus a field scan,
     stood. -->
- **The strongest answer to "is this helper sufficient" is "the shape set is CLOSED", and a LANGUAGE FACT can
  prove it where no census can.** **An EMPTY census is actionable only in its STRONG form — what the population
  DOES, not what it lacks.** **A comparison whose SELECTOR matched nothing is not a comparison and fails as a
  FALSE POSITIVE**, so locate the target on each side's own content. <!-- 2026-09-06: Go declares the
     socket-address interface with an UNEXPORTED method, so no package outside its own can implement it — three
     arms in the writer, three method definitions in the tagged sources, three implementation records in the
     converted corpus, all agreeing; look for the closure proof before counting instances. Strong form: a
     caller-side by-address census found every one of four hazardous caller behaviours PRESENT and repeated
     across 43 reference-bearing sites of 80, each answered by a named mechanism the companion was written
     against; "I found nothing" would have closed nothing. Selector: a row-lookup pattern matched nothing and
     the fallback silently hashed the WHOLE FILE, so two refs' hashes differed for reasons that had nothing to
     do with the row, caught only because they differed where they should not have. -->
- **A NAME ENCODES THE SHAPE, NOT THE COUNT: every arity in a shim table comes from a DECLARATION**; **a
  THEORETICAL bound is useless for sizing** — the REACH of the consumers bounds it. **Re-derive the population
  before any design is cut against it, and state what was MEASURED, not what was concluded**; disagreement can
  be SCOPE rather than predicate, and **scoping a census with the tool just shown to under-report reproduces
  its blind spot.** **A PLACEMENT instrument needs a SECTION GUARD, harder than the matcher** — anchors come in
  THREE FORMS (naming a section, quoting a clause, chaining to a sibling), so parse the anchor's FORM first and
  keep the DRY-RUN landing-site check. <!-- 2026-09-08: a helper named for five arguments and a pair was read
     as six; its declaration is FIVE parameters, every one the pair struct, and a table sized off the name
     sizes a body against the wrong number. The theoretical bound was the callback ABI's argument-word limit;
     consumer reach split into a small ungated set and a larger one behind a toolchain gate that SKIPS ON BOTH
     SIDES. 2026-09-02: grep 6, an instrument pointed at the grep-NOMINATED packages 11, an independent
     go/packages pass over all std packages 13; the 13 split into three tiers (6, 3, 4) and the "most
     interesting" members were the tier that needs nothing — a summary restating a lane's conclusion inherits
     its unvaried axis. Placement: an anchor-resolver reached 40 of 40 and applied 760 lines at a median
     distance of 1 line from each anchor — and 52 of those lines landed in three sections the draft never
     named, because a fragment can legitimately match text elsewhere; a naive guard requiring the match to sit
     under the declared heading dropped resolution 40 -> 26, because one extractor plus a bolted-on guard
     cannot serve all three forms. Without the dry-run check a partially-correct application would have reached
     the file every lane reads first. -->
## Liveness: proving a run is alive or dead
- **Never wait on a Windows pid from Bash**: `kill -0 <pid>` resolves pids in Git Bash's own emulation
  namespace, so the wait **exits on the FIRST iteration** — no error, exit 0, indistinguishable from a real
  completion. **`Wait-Process` has also reported a still-running target as exited**, and **`exit $true` is exit
  code 1**, so `until ! powershell -Command "exit (Get-Process …)"` ends instantly. **Poll POSITIVELY from
  PowerShell** (`while (Get-Process -Id $pid) { Start-Sleep 20 }`), or make the long run the harness BACKGROUND
  TASK itself so its real exit code is the task's. <!-- kill -0 measured 2026-08-26 against a pid from
     Start-Process -PassThru: it reproduced the 2026-08-16 damage and then some — two CNR runs believed dead
     were alive, a third was launched, and THREE concurrent transpiles raced into one behavioral tree (the r41
     overlap hazard), with a partial 2-package git status reading as a reassuring near-clean verdict. The tell
     was an mtime census — 288 of 641 packages never re-transpiled — and the proof was Get-CimInstance
     Win32_Process showing both "dead" hosts alive with live go2cs.exe children. PROTOCOL v3's mailbox monitor
     already relies on run_in_background being the child of bash. Wait-Process: 2026-09-01, the residual-pass
     lane, twice in a row — a background-wrapped Wait-Process -Id said done while Get-CimInstance showed the
     host alive with a live go2cs.exe child; one redundant CNR raced into the same behavioral tree before it
     was caught; mechanism unconfirmed. Inverted poll measured 2026-08-16 — the false reading launched a SECOND
     CNR into the same tree, two racing transpiles, caught only by PID inspection. -->
- **Read process AGE from `CreationDate` against `Get-Date`**, never an assumed clock; **`pgrep -f <name>`
  matches its own wrapper's command line** so such a loop spins on its own reflection — match `/proc/*/exe`;
  **completion inferred from a SIDE EFFECT is not completion**; **a pid from `ps` seconds after `setsid` can be
  the WRAPPER and `setsid nohup … &` leaves `$!` naming the SETSID PARENT**, so **the chain PID-records itself
  from INSIDE (`echo $$`, then `exec`) and the waiter reads that pidfile**; **a restart notice is not evidence
  either way** — check PID IDENTITY before relaunching. <!-- 2026-09-01/02: a healthy three-minute-old run was
     killed in the belief it had been hanging for hours; a file reverted because a running CNR had already
     transpiled it is a footprint, not an exit code. Wrapper pid met by two lanes 2026-09-04, the $! form
     2026-09-05 — the same wrapper-pid reading through the LAUNCHER's own door rather than through ps; `ps |
     grep <script> | head -1` picks the wrapper too, because the wrapper's own eval line carries the script's
     text, and such a "process" reports EXITED instantly. Kill by an exact `bash <path>` match excluding $$.
     Restart, 2026-09-04, the container class: one restart killed the watcher and spared the detached chain,
     the next killed the chain mid-leg and left a 0-byte log. -->
- **ABSENCE AT ONE INSTANT IS NOT DEATH**: a 0-byte task-output file (the transcript is written at COMPLETION),
  a worktree with no new files for an hour, a probe landing BETWEEN spawns, a `| tail -25` buffering a task's
  whole output. **Liveness is a process census against the worktree's PATH plus the agent's own notification; a
  stopped-with-no-children notification is the only "done".** **And `git status --untracked-files=all` over a
  `bin`/`obj`-heavy worktree can run for an HOUR** — slow, not hung. <!-- 2026-09-02/03, three costumes. The
     coordinator declared a live agent dead, dispatched a duplicate, and had to stop it when the original
     reported with full gates; the quiet worktree was a seeded reconvert writing outside src; a gate wrapper
     called "dead" had merely finished its leg and then spent 26 minutes inside git checkout + git clean. The
     hour-long status is the harness's own. -->
- **CENSUS THE HOSTS, NEVER `go2cs.exe`, to ask whether a slot is held** — a CNR spawns one SHORT-LIVED
  converter per package, so a binary census reads FREE hundreds of times during a run that holds the slot.
  Census `powershell`/`pwsh`/`BehavioralRunner` whose command line names `check-no-regression`,
  `run-behavioral`, `run-validated-sweep` or `BehavioralRunner`, excluding the querying process and your own
  tree, **POSITIVE-CONTROLLED** (N holders live must report N). **But `CommandLine` is NOT reliably readable,
  so a filter over it answers ZERO and reads exactly like a dead run**: census by NAME UNFILTERED or
  corroborate with the run's OWN artifacts. **Classify a background shell by its COMMAND LINE, never by its
  RHYTHM.** <!-- Measured 2026-09-04: a lane waiting on `while (Get-Process go2cs)` would have dropped a -tests
     pipeline into a running battery on its first sampled gap — 900 s of luck before it was caught; the holder
     of a slot during a CNR has no converter of its own most of the time, and the launcher re-checking
     immediately before it starts and refusing on a live parent is what makes the rule cheap. CommandLine:
     2026-09-05, three times in one night against a run that was demonstrably alive — its converter executing,
     its emission dirt growing; same shape as the bash-kill-0 and pgrep-self-match traps, and the reason the
     positive control is not optional. Rhythm: a coordinator's task list shows SUB-AGENTS' background shells
     beside its own, and eight staggered 60 s poll loops were classified as "monitor iterations" from their
     cadence when one belonged to a sub-agent; classify from the parent bash's command line via
     Get-CimInstance, and remember a sub-agent's poller dies with the sub-agent. -->
- **A CLAIM is closed by its owner's release POST, never by an absence somebody else observed**: `Get-Process
  go2cs` = 0 and a MISSING `go2cs.exe` between two legs is exactly what a REBUILD looks like. **A ROW WHOSE
  DEADLINE FLOOR EXCEEDS A HOST'S UPTIME IS NOT MEASURABLE THERE, and is not retried.** **A measurement taken
  ACROSS a host SUSPENSION is not a measurement** — take a clean tree-kill by VERIFIED PARENTAGE, keep **no
  record**, and relaunch behind a readiness gate on an ABSOLUTE-deadline wait that re-reads the clock.
  <!-- `go build` deletes and rewrites the binary and CNR rebuilds it unconditionally, so a lane reading those
     as a release can start an hour-long run under a claim that is still live; the other sibling-misread is the
     inverted `exit $count` poll above. A claim past its stated size is re-read by ASKING its owner, not by
     inference. Uptime, 2026-09-04: an hourly-restarting container against a 30-minute row cannot produce a
     verdict however many attempts it makes — state it and move the row, or drop it. Suspension, 2026-09-03: a
     laptop going lid-closed inside net/http fabricates exactly the mid-stream death signature that row
     instruments; the tree-kill was 22 processes, and a bare go2cs kill orphans the host and locks runtime.dll.
     The readiness gate is pinned toolchain answering, network up, no converter alive, clean tree; an absolute
     deadline fires on wake rather than during standby. -->
## Kills, and censuses that match themselves
- **Never `Get-Process <name> | Stop-Process`** — it matches by NAME across the whole machine and kills a
  SIBLING worktree's in-flight suite. **Signature: exit `-1`, log truncated mid-line, no diagnostic** — read
  that as "killed externally", NOT as a compile failure. **Be UNMATCHABLE BY NAME** (`Copy-Item
  BehavioralRunner.exe myRunner.exe` in the same bin dir), **scope your own kills by path** (`Where-Object {
  $_.Path.StartsWith($myWorktree) }`), and run **no sweep by executable NAME at all while a sub-agent shares
  the box.** <!-- Includes the ad-hoc `Get-Process BehavioralRunner,testhost | Stop-Process` that is easy to
     type before a run. A full run died at 124s inside PreBuildSharedDeps and another at 163s inside
     RunCompileGo, and the same corpus then passed 521/521 untouched. Waiting for the other worktree's process
     to exit is not enough — it re-arms for its next run. The renamed apphost still launches the embedded
     BehavioralRunner.dll and AppContext.BaseDirectory is unchanged, so discovery is identical. Sub-agent
     sweep: the pattern that matched a chain's converter also matched a sub-agent's two-seeded-diff arm, twice
     in one morning. -->
- **"An instrument that can match ITSELF is measuring its own presence" — and it kills.** **Any census that
  KILLS excludes the shell/host names (`bash`, `powershell`, `pwsh`) and its own PID, or matches the EXECUTABLE
  (`/proc/*/exe`) — better, keys on something the RUNNING loop ASSIGNS.** `pkill -f <script path>` from a
  command line CARRYING that path kills the caller's own shell; **concatenating the pattern does NOT help —
  filter by AGE or ancestry**; **a census keyed on a WORKTREE NAME over-matches its PREFIX** (`sub-q2` inside
  `sub-q23`); **where the only `pwsh` is a DOTNET GLOBAL TOOL the querying shell IS the dotnet host**, so
  exclude SELF by ANCESTRY, **count an UNREADABLE command line as a BLOCKER, not absence**, and
  positive-control in the MUST-FIRE direction. <!-- Named by the lane that paid it, 2026-09-03: the
     coordinator's own census `CommandLine -like '*<worktree-id>*'` matched the coordinator's OWN bash (its
     command line carried the id as a variable) and Stop-Process killed the querying shell mid-command, exit
     255; a kill loop keyed on a branch-name fragment matched its own shell the same way; a /proc census keyed
     on a marker string counted the probe whose own `case` pattern contained it — 2 monitors reported where 1
     ran, a healthy one nearly killed. Every instance was caught by a second derivation disagreeing, never by
     the census itself; a rule met twice becomes a CHECKLIST line in the lane's own scripts, not a thing to
     remember. Kill by PID from a bracketed grep. Concatenation measured 2026-09-05: a "no chain may be live"
     gate reported 4, then 3, every one the shell performing the check, age 0m — a real long-running process is
     minutes old, read from CreationDate. pwsh-as-dotnet-tool, 2026-09-08: it caught a genuine FOREIGN blocker,
     another agent's go test, before the first timed run; a real build must read LOADED and an actively
     compiling analyzer server must read BLOCKER. When auditing for strays, exclude your own querying process —
     a `Where-Object { $_.CommandLine -like '*check-no-regression*' }` sweep matches the very command line
     performing the sweep, reports a phantom survivor, and kills your own shell. -->
- **A harness `TaskStop` does NOT reap the converter child, and stopping a background TASK kills the task's
  SHELL, not the chain**: **a stop is a TREE kill by parentage from the chain's ROOT** (`taskkill /T` on the
  root pid — in PowerShell `$pid` is a RESERVED automatic variable, so a hand-rolled walk that assigns it
  silently kills nothing), verified by a census naming ZERO survivors, keyed on `go2cs.exe` which a probe never
  spawns. **A KILLED WRAPPER TASK IS NOT A VERDICT ON THE CHILD IT LAUNCHED** — wait on the CHILD's pid
  POSITIVELY, then **RE-ASSERT both arms' written counts before diffing.** <!-- Measured 2026-09-03: go2cs.exe
     kept spawning after the stop, the PIDs taskkill reported differed from the ones listed seconds earlier,
     and a REUSED log path spliced two runs into one file — together putting TWO conversions into one seed root
     (the r41 DYNTYPE hazard), with nothing banked only because the diff never ran. 2026-09-05: an assembly
     script survived as an ORPHAN, its killed leg returned looking finished, and the relaunch put TWO chains in
     one worktree writing interleaved stamps into one log — every reading discarded; two probes matched their
     own querying shells. Killed wrapper, 2026-09-05: a -stdlib converter OUTLIVED its reaped wrapper and kept
     writing, so the staged trees were already full and a diff taken at the kill would have read a confident
     ZERO — the emitted-before-seeded retraction reached by a second route. Continuing after such a kill is
     legitimate ONLY because the run's sentinel files survive, so the written-this-run assertion can still be
     made per arm and target. Two lanes' independent arms reproducing the same per-target emission counts —
     1,656 / 1,724 / 1,727 — is a cross-check on the instrument worth naming, not a coincidence to pass
     over. -->
- **Killing `go2cs.exe` alone ORPHANS its `dotnet run` child and the test host under it**, keeping
  `runtime.dll` locked; **`dotnet build-server shutdown` is ALSO machine-global** (same truncated-log signature
  with no `Stop-Process` anywhere) — isolate your own builds instead (`MSBUILDDISABLENODEREUSE=1`,
  `-p:UseSharedCompilation=false`). **Scope DELETES by lane prefix too** — `Remove-Item <scratchpad>\*` is a
  cross-lane destructive act. <!-- Orphan measured 2026-08-16, cost one invalid run: the NEXT pipeline run
     failed MSB3027/MSB3021. build-server found 2026-08-03: one lane's startup cleanup yanked the shared
     MSBuild servers out from under a sibling's in-flight compile; reserve it for solo contexts or
     coordinator-owned quiet points. The repo's own instruments are safe by default since 2026-08-08
     (db427e6e9): run-behavioral-tests.ps1 runs its shutdowns only under an opt-in -ShutdownBuildServers
     switch, and AssemblySetup's teardown honors the same env-var contract — the hazard that remains is the
     ad-hoc, hand-typed invocation. Deletes measured 2026-08-16: a lane's cleanup swept the whole shared
     scratchpad and unrecoverably deleted sibling lanes' artifacts; name your own <lane>-* files. -->
- **The freeze binds INSTRUMENTS, not just edits: any script that mutates worktree state names the processes it
  would disrupt and REFUSES.** **ONE converter process per lane box at a time, whatever the output root**, with
  a seeded-conversion PREFLIGHT refusing while any `go2cs.exe` is alive and a run-tagged log name. **A
  PER-DISPATCH WORKTREE, NEVER A SHARED ONE: a precondition in a brief is not a lock, and a purge ordered after
  a wait is still a purge** — when two dispatches share one, **the reading is DESTROYED** and the leg is
  UNMEASURED. **Census live processes, and wait for the task notification, before relaunching into a
  worktree.** <!-- Freeze: a verify-only landing preflight was one command from running in the worktree a
     multi-hour sweep was using, where its `git checkout --detach` would have yanked the tree out from under
     the running converter and destroyed hours of gating — "verify-only" described its GIT effect, not its
     effect on a neighbour. The guard is three lines (count live converter/runner processes by path, refuse
     non-zero, name the count) and it fired on its first run, 2026-09-06. One-converter, 2026-09-03: a CNR died
     43 s after a -stdlib conversion started on the same box with a DIFFERENT output root; mechanism unrooted.
     This WIDENS the r41 rule, and footprint diffs wait for the CNR to print its verdict. Worktree, 2026-09-08:
     a delta gate was sent into the same worktree where the previous gate's solution build was still running,
     with "wait for zero build processes" written as a PRECONDITION IN THE BRIEF — and that build went from 0
     errors to hundreds in two minutes, every code an I/O or metadata-file failure from obj trees deleted under
     a live compile (one environmental failure wearing many codes, per the error-histogram rule); the leg rides
     the train battery instead. 2026-09-02: a third rebuild attempt met the second chain's in-flight reflect
     -tests convert as untracked *_test.cs and aborted on its dirt gate, the r41 overlap hazard caught only
     because that gate existed. -->
## Detachment, background tasks and orphans
- **Anything longer than a turn runs DETACHED, env-pinned in the SAME command, logged unique-per-run, polled
  POSITIVELY by PID.** Clean-death evidence before a restore is modified files with ZERO untracked.
- **The harness reaps a session's own process TREE by PARENTAGE, not by name**, so the apphost rename does not
  cover it — same truncated-log, no-diagnostic signature, which is why it reads as the by-name kill.
  **`Start-Process -WindowStyle Hidden` with output redirected to a log survives; `-NoNewWindow` +
  `Wait-Process` does NOT.** **The two stories point OPPOSITE ways**: a `-WindowStyle Hidden` launch from
  INSIDE a PowerShell TOOL call dies silently, while a Bash `run_in_background` task is reaped with the
  SESSION's tree. **And SELF-BACKGROUNDING INSIDE A TRACKED BACKGROUND CALL MAKES THE HARNESS TRACK THE
  LAUNCHER.** <!-- Tree reap measured 2026-08-12, the A2 integration agent; surviving it required Start-Process
     detachment, and a long runner started as a background Bash/PowerShell child is IN that tree. Same shape as
     the sweep caveat in the budget table (a LANE parking a detached sweep still loses it); the difference is
     that Start-Process detachment is what makes a long run survivable at all. Flags measured 2026-08-14, the
     argv-stop and os-signal lanes: the wait re-parents the session's fate onto the child and the turn boundary
     kills it exactly as if spawned inline. 2026-09-02: the tool-call form died ~15 s in (the documented
     pattern covers a BASH-launched child surviving the turn boundary, not a tool call's own job scope); a
     2-hour solo sweep launched as a background bash task died ~13 min in and sat UNDETECTED for 76, with no
     completion notification. Self-backgrounding, 2026-09-08: a `nohup … &` from inside a harness background
     task left the harness watching the wrapper, which reported EXIT 0 while the real loop died after one
     project and six legs went unmeasured under a tool that said success — the killed-wrapper rule met from the
     launcher's own side. -->
- **The scratchpad is SHARED across concurrent lanes** — session-scoped, not lane-scoped. **Prefix every scratch
  filename with your lane/branch name (`<lane>-cnr.log`) AND make it unique per RUN**: a reused log path on
  Windows can splice a fresh run's header onto a stale run's tail and fabricate readings. Treat a truncated or
  rewritten scratch log as a collision first, a gate failure second. <!-- Measured 2026-08-15: two lanes both
     writing cnr.log — one clobbered the other's gate log mid-run and the verdict had to be recovered from git
     status. File tunneling plus partial overwrite fabricated "CNR finished in 20 s", also 2026-08-15. -->
## Logs, stamps and watchers
- **A pipe masks a command's exit status only WITHOUT `set -o pipefail`** (`set -uo pipefail; false | tail -1`
  -> 1; without it -> 0); a REDIRECT preserves it. **Capture the real exit before any pipe, and read the RECORD
  rather than an exit code** — the record carries per-test verdicts an exit code cannot. **A false CONFESSION
  corrupts the record as much as a false success.** <!-- Corrected and measured 2026-09-03; the earlier
     overstatement ("an exit code is worthless the moment a pipe is in the command") is FALSE and was retracted
     within the hour by the lane that wrongly confessed it. -->
- **A `Tee-Object` onto a log that `Start-Process -RedirectStandardOutput` ALREADY HOLDS throws, aborts the
  pipeline, and leaves `$LASTEXITCODE` as TEE'S** — a battery reported **"5 legs, 0 failed" having run ZERO
  legs**, each in `0s`, DURATION the only tell (the same tell as the perf runner that "completed" in 20
  seconds). **Redirect each leg to its OWN path, and assert the leg PRODUCED A VERDICT LINE, reporting `NOT
  MEASURED` when it did not** — an exit code cannot distinguish "passed" from "never ran". <!-- 2026-09-07; it
     would have banked as a green canary run. The rewritten battery reported NOT MEASURED on its next failure
     instead of PASS — the whole difference between a false green and a caught one. Only a positive artifact
     can make that distinction. -->
- **A battery's VERDICTS must land in the log the watcher tails, or a red leg is INVISIBLE**: every leg stamps
  its EXIT CODE and one-line verdict (count, changed set), the monitor's pattern includes `exit=[1-9]`, and a
  leg that CONTINUES past a failure by design says so in the same stamp. **Gate on the CODE (`GUARD
  exit=[1-9]`), never on a WORD in the prose** — the FALSE-RED twin. **A leg keeping the runner's output in
  MEMORY leaves a `Failed: 1` with no NAME**: write the whole output to a per-leg detail file and stamp every
  `Failed <test>` line. <!-- 2026-09-04: one train's assembly log stamped only leg BOUNDARIES while the CNR's
     exit=1 and its one CHANGED file went to a per-leg log nothing watched — the boundary stamp read as
     progress, the restore stamp erased the drift, and the red sat unseen for half an hour until the leg log
     was read by hand. 2026-09-05: a launch gate spelled `grep -icE 'guard.*(FAIL|RED|MISMATCH)'` and counted
     an honest `ROSTER GUARD exit=0 (0 fail lines)` as a failure, refusing a clean launch. -->
- **A monitor built on `tail -F <log>` can die SILENTLY** — a watch whose death is indistinguishable from
  quiet. Use single-notification background waits (`until grep -q '<stamp>' log; do sleep 60; done`) re-armed
  per leg, **census a monitor's tail by command line before trusting its silence**, and kill stale tails by
  PID. **A watcher keyed on a background task's OUTPUT FILE cannot see a chain whose stdout is REDIRECTED to a
  log** — key it on the artifact the process WRITES, and check the watcher's own log for its trigger line.
  <!-- tail -F died twice in one day, 2026-09-05: its tail process gone, 0 events, the task still listed; stale
     tails hold the log open across sessions. Output-file watcher, 2026-09-03: task output 0 bytes, trigger
     never fires, watcher waits out its whole timeout looking healthy — route #6's shape, one layer out. -->
- **A MAILBOX MONITOR FIRES ON YOUR OWN POST** — confirm the new tip's AUTHOR before reading a wake as a lane,
  and re-anchor after every post. **But RE-ANCHORING IS NOT RE-ARMING**: editing the anchor constant starts
  nothing, so **the check after every re-anchor is a PROCESS CENSUS, not the edit's exit code.** **A filtered
  `ps -ef | grep <script>` reads ZERO for a live monitor** (MSYS `ps` prints only `/usr/bin/bash`) — census
  UNFILTERED and match the ARMED line's pid. <!-- 2026-09-07: a coordinator that armed and pid-verified both
     watchers at session start then re-anchored twice across two posts and left the fleet unwatched for ~8
     minutes, because the edit SUCCEEDED and the success of the edit was read as the state of the watch — the
     same distinction as an EXITED task id being evidence of a PAST arming. The own-post wake is the own-push
     blind window arriving as a FALSE WAKE. -->
## Batteries: disk, legs, refusals and coverage
- **A ROW-LIST DRIVER GUARDS `processed == listed`, PRINTED AND FLAGGED** — PowerShell eats the loop's stdin,
  so a row is swallowed WHOLE and the next arrives truncated, with no error and no failure: put `< /dev/null`
  on the child and the row list on FD 3. **A verdict grep ANCHORED AT COLUMN 0 misses the sweep's INDENTED
  verdict line**, so print `NO VERDICT LINE -- instrument fault` rather than defaulting, and ABORT a control
  arm yielding none — **four green arms in a row is the tell, not a pass.** **The driver RESTORES after EVERY
  row and REFUSES to start on a dirty tree or with a `go2cs.exe` alive.** <!-- 2026-09-04: a 3,643-verdict row
     was swallowed whole and the next arrived as "atabase/sql"; a 21-row sweep would have reported as 23. 54
     files of dirt after two rows; a relaunch started at dirty=14 because the killed run's children were still
     writing. -->
- **DISK is a gate input: a battery preflights its own floor before its first leg, and a chain's exit code is
  the OR of its legs.** The sweep floors at 25 GB. **The instrument is a per-worktree `bin`/`obj`/`Generated`
  SIZE census, not a directory count**, and a checkout is purged only after its newest build-output mtime AND a
  process census both say idle. **`src/clean-bin.ps1` asks a `Read-Host` confirmation, so a non-interactive
  invocation CANCELS SILENTLY (exit 0, nothing removed)** — purge with `Get-ChildItem -Include
  bin,obj,Generated -Recurse | Remove-Item`, **scoped so it does not match `src/go2cs/bin`**, then re-verify
  the converter exists. <!-- Measured 2026-09-03: a train battery's TAIL legs — runner, sweeps, pair, reflect —
     all aborted in ONE MINUTE on the sweep's disk preflight (18 GB free against the 25 GB floor) while the
     chain itself EXITED 0. Seven sub-agent worktrees plus one leg's own 21 GB of behavioral build output
     crossed the floor mid-battery; the chain's exit code carried nothing and the leg logs carried everything.
     The box's largest build output, 87 GB, sat in the MAIN checkout nobody had built from in two days while
     two batteries fought over the last 20 GB. The standard `find … -name bin … -exec rm -rf` idiom DELETES
     src/go2cs/bin/go2cs.exe; it failed loudly with rc=127 that time. -->
- **A BATTERY LEG REFUSED BY ITS OWN PREFLIGHT STAMPS A PLAUSIBLE-LOOKING VERDICT LINE** and the chain rolls on
  into more UNMEASURED legs; **the tell is the CLOCK.** **A chain reads every leg's log for REFUSAL markers and
  STOPS on one**, and **a leg's stamp carries its ROW COUNT.** **A chain whose FULL-SUITE leg FAILS must not
  roll into the SOLO COST leg** — which legs are still worth running after a red is a per-leg judgment the
  derive encodes. Census free space before a train launches AND after its full suite, and purge behavioral bins
  the moment the Output phase ends. <!-- 2026-09-05: the sweep's 25 GB floor refused at 20.2 GB free, the leg
     stamped "0 records preserved" and an empty PAIR:, and 22 rows went by in 24 seconds. Now wired: a DISK
     PREFLIGHT line in the sweep's log stamps the leg UNMEASURED and exits 3. The per-project bins are ~20 GB.
     Cost leg, 2026-09-05, a train chain that did: the sweeps after a red suite are still data, reading the
     same root row by row, while a cost pair measures COST on a tree that WILL change and must be re-run
     anyway. -->
- **A TRAIN BATTERY WHOSE EVERY LEG IS WINDOWS-DEFAULT CANNOT SEE A SEAT WHOSE ONLY FILE IS PER-GOOS**, and a
  green battery then reads as covering a file it never compiled: **a battery DERIVES a per-GOOS leg from the
  seats' file paths.** A comment-only change is provable without a compiler — strip whole-line comments both
  sides, equal sha256, zero block-comment delimiters introduced, stripper positive-controlled. **A host limit
  stated in advance is a routing input; discovered mid-run it is a lost measurement** — a `runtime` `-tests`
  row costs ~2.75 GB from cold. <!-- Measured 2026-09-07: a seat's sole file was
     src/core/runtime/linux/signal_posix_impl.cs, and go2cs.slnx, CNR, the behavioral suite and GolibTests are
     all windows-default or never build src/core/runtime, so no leg touched it — the L3 rule met inside the
     coordinator's own battery. A sibling lane closed it with `-p:GoTargetOS=linux runtime.csproj` (exit 0)
     carrying BOTH controls: the log names the file six times and the windows-default build of the same csproj
     touches it ZERO times. Budget, 2026-09-07: the row itself 0.443 GB, its dependency closure 2.731 GB across
     281 bin/obj directories — enough for ONE such row on a 16 GB host with margin, not for concurrent rows and
     not for a cold full sweep. -->
- **The serial order relaxes on MEASURED properties, never on impatience**: it protects exactly ONE property —
  a refusal branch tripping on a per-project transpile TIMEOUT — so CNR, separate worktrees (the r41 hazard is
  per-ROOT), a `-stdlib` A/B pinned at `-convert-timeout 90m`, and a corpus-scale control with a raised budget
  plus a "timeout-shaped refusal re-runs SOLO before belief" rule may overlap them; **anything WITHOUT those
  guards stays serial.** **What binds across worktrees is LOAD** — pass the same `-convert-timeout` to BOTH
  binaries and record each run's WALL beside its verdict; a loaded wall can only produce a false red. **And a
  `-stdlib` census is never PARKED.** <!-- 2026-09-04. CNR has no per-package budget; a parked -stdlib census
     can only be re-seeded from scratch, throwing away completed targets; the A/B's pin cannot be pushed past
     its cap. -->
## Launch hazards
- **A TRAIN SCRIPT WITHOUT ITS OWN `cd` RUNS IN THE CALLER'S CWD**: every derived train script names its
  worktree in its first lines and refuses any other (`[ "$(git rev-parse --show-toplevel)" = <expected> ] ||
  exit`) — a dirt gate is a backstop, not the address. **Normalize both sides through the same shell** (`$(cd
  "$(git rev-parse --show-toplevel)" && pwd)`), or the guard compares a drive-letter path against an MSYS
  `/c/…` path and ABORTS a correct run. <!-- 2026-09-05: a rehearsal launched from the coordinator's scripts
     folder ran inside the MAIN checkout at a stale head, and the only thing that stopped it merging sixteen
     seats into the wrong tree was the dirt gate reading an untracked .claude/ folder. 2026-09-07: the spelling
     mismatch failed SAFE with "wrong worktree" and still cost a launch — the same namespace split as a bash -f
     test against a native tool's path argument, met inside a safety check rather than inside the work. -->
- **A NESTED `pwsh -NoProfile -File` UNDER A PINNED `DOTNET_ROOT` CANNOT RUN WHERE THE ONLY `pwsh` IS A DOTNET
  GLOBAL TOOL**: it targets an OLDER runtime the pin hides, so the child exits with a large negative code in a
  tenth of a second having run NOTHING, its message naming the missing runtime version. **Invoke the harness
  script IN-PROCESS from the already-pinned shell.** **And the sweep's `-SkipBuild` expects the converter at
  `src\go2cs\bin\go2cs.exe`** — an outside-the-repo binary path is not consulted. <!-- i7, 2026-09-08; caught
     by the exit code plus the wall. A lane that built elsewhere re-pays the build or copies the exe there. -->
## A/B arms, flakes and attribution
- **The three-run flake standard: fail-WITH the change, pass CLEAN, pass again WITH it restored** — in that
  order, before anything is attributed to a commit. **Reverting the `.cs` is NOT an A/B when the instrument
  re-converts**: `run-validated-sweep.ps1` re-emits from the LIVE binary, so both arms measure the same
  converter. **Swap the PRESERVED pre-change `go2cs.exe` into the sweep path, and state which binary each arm
  ran.** <!-- 2026-09-01/02. A row that fails once is not a finding. The re-convert case is exactly what
     happened on the h2 deadline rows: a hand-reverted corpus file is overwritten before the row runs, so
     identical signatures on both sides read as "the change is innocent". -->
- **An A/B ARM that names a ref IS a ref derivation and inherits the stale-ref rule**: `git fetch origin <ref>`
  immediately before the checkout, and **PRINT the resolved SHA**. **A mid-run checkout makes a count belong to
  a tree nobody named** — check WHICH TREE a run measured. <!-- 2026-09-02: a Linux clone that had only ever
     fetched its own branch checked out origin/master at a commit predating the corpus hop, and the sweep's
     toolchain-pin guard refused it ("version.props pins 1.23.1 … NOT MEASURED, never a verdict") — the guard
     caught it, the lane did not. A sweep measuring a 13-entry tree while the branch moved under it read
     exactly like a silently-rejected disclosure. -->
- **An instrument that FAILS IDENTICALLY on BOTH ARMS confirms whatever was predicted**: **an arm proves it
  MEASURED something — a comparison record present, carrying the row's verdict count — before its number is
  compared**, and an invalid run's logs are kept as wreckage rather than deleted. <!-- 2026-09-04: four legs
     called the converter DIRECTLY on a Linux host, bypassing the GoTargetOS pin and linking the WINDOWS
     dependency set (phantom CS0426, exit 1, no comparison record), so both arms read 0 matched / 0 diverged —
     and a prediction of "unchanged within noise" is CONFIRMED by two zeros. The only tell was a `record=`
     column in the lane's own verdict line reading EMPTY. -->
## Mass-empty verdicts
- **READ THE RESULTS-FILE TAIL BEFORE ANY SHAPE ANALYSIS**: a deadline kill states itself outright, and **on a
  row behind a known capability frontier a throwing managed body is the EXPECTED cause and the tail says so** —
  making the row a frontier question rather than a defect.
- **A HOST THAT DIES BEFORE PRODUCING ANY VERDICT HAS MEASURED *NOTHING*, NOT ZERO**: characterising such a row
  as "N verdicts, converted side entirely empty" against a committed "5 of 15" reads to the channel as a **5 ->
  0 regression**; the true statement is **5 -> UNMEASURED**.
- **A locked `runtime.dll` produces `Go="pass" C#=""` for every test, which reads exactly like total conversion
  failure** — kill the process TREE by verified parentage, then re-run before believing any mass-empty verdict.
  <!-- Measured 2026-08-16: killing go2cs.exe alone orphaned its dotnet run child and the test host under it;
     the next pipeline run failed MSB3027/MSB3021 and its comparison reported the mass-empty shape. Cost one
     invalid run. -->
- **SUBTRACT THE DISCLOSED, THEN COLLAPSE TO ROOTS — read it off the comparison RECORD rather than running
  anything, and ask for ROOTS rather than for the count.** <!-- reflect read 65 differing at master; 60 were
     already DISCLOSED, 5 were not, and the 5 collapsed to THREE roots — one manifest entry (which closes TWO
     rows, because entering a missing leaf makes the parent ride the disclosed-parent aggregation), one
     delegate arm (two more rows, both its motivating cases), and one row belonging to a different lane
     entirely. The two natural guesses, "five roots" and "sixty-five things", were both wrong. -->
