---
name: merge-hazards
description: Merge or rebase concurrent lane branches. Silent duplication and silent subtraction, stale-base illusions, adjacent-insert hunks, ordinals, registry-count coincidences, and the checks that catch what git reports as a clean merge.
---

# Merge Hazards

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 5685-6115, 7574-7813.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

### Integrating concurrent lanes (the hazards that do NOT show up as conflicts)

When several machines work the same tree, the dangerous merges are the ones git reports as clean.
Each rule below was paid for.

- **⚠ Two lanes solving the same problem produce a SILENT DUPLICATION, not a conflict.** Independently
  added blocks land at different offsets under different names, so git merges both as ordinary
  additions and marks nothing. Measured 2026-08-24: two independently written 17-element apply-set
  arrays in `migrate-tfm.ps1` auto-merged *cleanly*, and the result would have appended every site to
  `$applySites` **twice**. The mirror case bites from the other side — a symbol introduced OUTSIDE the
  markers (`$shadowed`) is left undefined at its use site if you take one side of a marked hunk. Neither
  is visible from the conflict markers. **Resolving the marked hunks is not resolving the merge: read
  the merged file whole, and run the thing.**
  ⚠ **The shape reaches CUTS, not only merges** (2026-09-02): the same two-line guard fix was written
  twice within one hour by two sessions, caught only because both announced before either merged — the
  author's branch was taken and the duplicate deleted. A coordinator-critical fix to a LIVE lane's own
  file is announced as an ASK to that lane first.
  ⚠ **Two 2026-09-04 refinements, one preventive and one diagnostic.** When two concurrent cuts need
  the SAME shared predicate, **write it at the SAME PATH on purpose** so the merge collides as a loud
  add/add conflict rather than auto-merging two differently-named definitions of one concept — the
  silent-duplication shape forced into the open — and the ruling then bases the LATER cut on the
  earlier one's file before the seat, so the assembly sees one definition and no conflict at all. And
  **a clean auto-merge of two cuts touching ONE method is read whole for two REPRESENTATIONS of one
  fact, not only for two definitions of one NAME**: a status value from one cut and a marker list from
  the other arrived in the same method from edits git had no reason to conflict, and the tree failed
  to compile ONLY because one call named the pre-merge API — a same-NAMED helper would have compiled
  with one representation silently authoritative. Resolve by DELETING the weaker representation (the
  one that reaches fewer consumers), re-run every arm on the MERGED tree, and state which TIP a CNR
  ran at rather than claiming a union CNR the train will measure.
  ⚠ **Its GUARD-PROJECT instance, 2026-09-05:** two seats that EXTEND the same behavioral guard
  project from the same base conflict on its EMISSION files (`main.cs`, the golden,
  `package_info.cs`) as well as on its Go source. **The resolution is never a hand-merge of
  goldens**: the LATER cut rebases onto the EARLIER, unions the Go source (deduping shared type
  declarations), REGENERATES the emission with a converter carrying BOTH fixes, adds a RED control
  against a converter carrying only the EARLIER fix (so the control names the LATER fix), and
  re-reads its gates at the new tip. A dispatch naming an existing guard project as the extension
  target tells the sub-agent which SIBLING seats also extend it.
  ⚠ **Its ATTRIBUTE instance, and it conflicted only by luck** (2026-09-05): two lanes widened the
  SAME attribute's `AttributeUsage` from two different rows (`+Struct` for named arrays; `+Class|Struct`
  for named pointer-to-array wrappers) and it surfaced as a TEXTUAL conflict ONLY because both edits
  landed on one line — had they not, git would have merged two widenings silently. Resolved by the
  SUPERSET, with the union's CNR plus `check-solution-integrity.ps1` as the check that the two stampers
  never double-stamp one declaration. **Read what a cross-lane attribute widening REACHES before
  merging it.**
  ⚠ **When a window EXPIRES and the lane arrives late, DIFF THE TWO ANSWERS rather than choosing by
  AUTHORSHIP** (2026-09-06): the coordinator's landed member and the lane's announced commit were
  byte-identical after whitespace — same override, same value, same placement — so the running battery
  already carried exactly the tree the lane's commit would have produced, and only the COMMENT differed.
  The better comment then lands as a FOLLOW-UP, because the mid-battery source freeze binds the worktree
  and documentation is never worth stopping a multi-hour gate run.
  ⚠ **TWO CORRECT CUTS COMPUTED AGAINST TWO STATES OF ONE FILE COMPOSE SILENTLY TO THE WRONG STATE**
  (2026-09-08): a converter fix whose WHOLE corpus footprint is a `.cs.auto` REVIEW SIBLING, which the
  build never sees, and a hand-own RE-DERIVE that copied the THEN-CURRENT sibling's mangled stamp into
  the `.cs` that COMPILES, have EMPTY file overlap, merge with ZERO conflict, and the union goes red on
  whichever seat merged LAST — **so the first, correct, gated cut is what looks broken.** Both sides
  were measured on the ladder tree. **Ordering is the remedy**: the emission fix and its sibling hunk
  land FIRST, the re-derive branch takes the one-line correction on top, and the re-derive is RETAKEN
  from the FIXED converter's emission at the hop, which carries the right spelling by construction.
- **⚠ An INSERT adjacent to a line the other side edited folds into ONE hunk, and BOTH single-side
  resolutions silently lose a line** (measured 2026-08-29: master inserted the `go/build` roster row
  directly above `go/build/constraint`, which the branch had annotated — `--ours` dropped the new
  row entirely, `--theirs` dropped the annotation; nothing in the markers says a line vanishes, so
  it reads like an ordinary either/or). Resolve by keeping BOTH sides' content, then **assert the
  structural invariant** (row/line count before == after + known inserts) instead of eyeballing it —
  and validate any re-derived aggregate by **positive control against a known-good blob first**: the
  same derivation must reproduce the other side's banked value exactly before its new value is
  believed. A derivation that cannot reproduce a known-good value is not a derivation.
  ⚠ **AND THE THIRD MEMBER RUNS THROUGH A RULE THAT IS RIGHT** (measured 2026-09-06). One train lands a
  `KeepAlive` buffer pin inside a generated darwin `pipe` body; the next train's seat replaces that whole
  body with a `GoManualConversion` placeholder. Both changes are correct alone, the rule (a hand-own
  displaces a generated body) is correct, **and taking the placeholder makes the pin's SITE cease to
  exist** — clean merge, compiles, and no gate is shaped to notice a `KeepAlive` that stopped existing.
  `syscall.Uname` runs the other way: there a registration landed without its body; here a body lands and
  removes a fix. **When a conflict hunk shows one side DELETING code the other side FIXED, verify the
  replacement carries the fix** — one grep, and only a census that reads the HUNKS instead of applying the
  rule will ask.
  ⚠ **The seat may REVERT NOTHING and a file-level resolution still lose the fix.** A darwin seat cut
  before the pin emission carried 0 pins where master carried 105 in one file; because the seat's diff
  touches that file at all (one placeholder hunk), `--theirs` at file granularity drops all 105. **Count
  the fix's tokens per file at BASE, at MASTER and on the BRANCH**: base 0 / master N / branch 0 **with
  the file in the diff** means resolve BY HUNK; the same triple with the file ABSENT from the diff means
  an ordinary merge keeps them and nothing is owed. And answer "is the pin there?" **from the BODY** — a
  hand-own that allocates native memory needs none, where a grep says "absent".
  ⚠ **The gate for this is an INVARIANT, never a constant.** A pin-count gate hardcoded `105` and
  false-redded a CORRECT resolution reading 104, because a displaced body takes its own pin with it;
  changing the literal to 104 re-breaks at the next displacement (the hoisted-literal lesson). The real
  invariant is **`pins lost <= placeholders added`**, computed from master and the ref at run time —
  controlled on the merge (1/1 holds), on master (0/0 holds), and on the pre-displacement branch (105
  lost, 1 placeholder, FAILS). That merge landed: `syscall/darwin/zsyscall_darwin_amd64.cs` reads 104 at
  `fd09034f5`, and the corpus carries 1,222 pins.
- **⚠ Its MIRROR is the SILENT SUBTRACTION, and it is worse: one lane REMOVES a definition because
  another branch supplies the replacement, and the merge drops the supplier.** Both diffs are pure
  additions/removals, git merges them without a conflict or a warning, and the result compiles
  nowhere. Paid for 2026-08-29 (`syscall.Uname`): the converter registration that DISPLACES the
  generated wrapper merged, the hand-own `*_impl.cs` BODY it displaces to did not, and the whole
  **linux corpus went RED at master with a clean `git status`** — `kernel_version_linux.cs`
  CS0117 `'syscall_package' does not contain a definition for 'Uname'`, discovered days later by a
  lane building that flavor, not by any merge. Note this is one step PAST the regenerate-never-merge
  seam rule the guilty file's own header documents: the generated side was correct, the destination
  was missing. **Mechanical preflight, cheap and now owed:** if
  `git diff --name-only <base>..<branch>` shows a `manualConversionFuncs` registration or a
  generated-body deletion, **assert the matching `*_impl.cs` body is present in the MERGE RESULT** —
  the same shape as the `package_info.cs` ⟹ `stdlib-metadata.txt` preflight above.
  ⚠ **Its golib-API form, and it is about SIGNATURES rather than bodies** (both measured 2026-09-05).
  **A WIDENING of a shared golib door keeps the PREVIOUS arity as a forwarding overload for at least
  one train**, with a remark naming the retirement condition (grep-proven zero callers), because a
  sibling seat cut against the old arity merges at the union with no fix-up any lane's gates measured —
  the silent subtraction caught one train EARLY only because a lane read another lane's post; the
  two-word form of `OverNativeMemory` means what `unsafe.Slice` means (cap = len). Its mirror: **an
  EXACT-MATCH constructor or overload added for ONE caller silently CAPTURES every existing call that
  bound through an IMPLICIT conversion** — `Pointer(INilPointer box)` took every `new Pointer(box)`
  that had gone through `ж<T>`→`uintptr`, so overload RESOLUTION changed and not just behaviour, and
  only a guard asking the question saw it. **A golib-touching seat runs GolibTests at BOTH
  configurations before it seats**, and a fix to a defect in a SEATED train's union lands on THAT
  seat's branch tip, never on a later branch that stacks on it.
  ⚠ **A REGISTRY ROW WHOSE PRINCIPAL RELOCATED IS RE-KEYED, NEVER RETIRED — retiring a row for a LIVE
  push turns a loud refusal into a silent stub with the guard GREEN** (2026-09-07). The coordinator
  ruled "the `internal/weak` two-row RETIREMENT" for the hop's one failing converter test; the lane
  re-derived from the 1.24.13 sources before cutting and found the guard's own `(renamed? deleted?)`
  question answers RENAMED — `internal/weak` became the PUBLIC `weak` package, both bodyless consumers
  (`weak/pointer.go:93,96`) and both runtime pushes (`mheap.go:2103,2108`, pusher names UNCHANGED)
  still exist — so the cut is two map KEYS. Deleting the rows would have reproduced the
  `syscall.runtime_envs` shape (a bodyless consumer indistinguishable from an ordinary stub;
  `os.init()` down on Linux) SILENTLY. **A ruling's VOCABULARY ("retire") is a claim about the sources
  and is re-derived there before it is cut; a relocated principal moves the KEY, and the hand-own's own
  relocation is the corpus move's work.** Same family as the silent subtraction above, arriving through
  a ruling's word.
- **⚠ A seam check that verifies a displacement HAPPENED but not that its destination EXISTS passes
  the exact failure it was written for, in mirror form.** The ten-names/zero-bodies property offered
  as the struct-passing merge instrument — every registered name has zero generated bodies and
  exactly one placeholder — was run twice over all ten names and reported as the check that would
  catch a lost registration. It is ONE-SIDED: a placeholder pointing at a hand-own body that does not
  exist passes it cleanly, which is exactly what master held, and the branch carrying the check
  carried the same gap (2026-08-29). **Every seam check carries both sides of the ledger** —
  registration ⇒ displaced wrapper ⇒ body, and the reverse (a dead hand-own nothing displaces) where
  the shape allows it cheaply. Put it in the tier every lane already pays for (the converter's own
  `go test ./...`, beside `projitemsIntegrity_test`) so the class turns into a red converter suite at
  the merge rather than a red corpus later.
  ⚠ **And check what the check's WITNESS is made of.** A displacement guard whose witness is
  ON-DISK placeholders is ENVIRONMENT-dependent for TEST-side hand-owns: it passes on a tree that
  has run that package's `-tests` and fails on every clean clone, because an unbanked row has no
  committed test emission (2026-09-02). Ruled remedy: a GOROOT `_test.go` witness arm, matched by
  CLASS rather than by name and counted separately as the weaker witness; the production arm is
  unchanged.
  ⚠ **A WITNESS CARRYING FEWER AXES THAN THE PROPERTY GOES GREEN ON EXACTLY THE CASE THE PROPERTY WAS
  WRITTEN FOR** (2026-09-05): the registration ledger's body witness collapsed every per-GOOS folder
  into ONE name set, so a scope widened to a flavour with NO body passed whenever another flavour held
  one — route #8's shape, disarmed by the legitimate arrival of a second flavour. **The witness takes
  the PROPERTY's axes** (name × flavour), asserted both ways per flavour. Three rules from correcting
  it. The census of what the CORRECTED guard fires on at master is posted as FINDINGS before any fix,
  and is positive-controlled IN THE DIRECTION OF THE FINDING before its list is believed (the known
  case fires nothing at its real scope and fires on the widened flavours when the scope is simulated
  wider); rows the raw predicate flags are CLASSIFIED before they are counted (a `partial` completion
  is the OTHER displacement mechanism — exempt, with its own control that stripping `partial` goes
  red; a bare-name regex over-collects a different member of the same name, stated at the arm, costing
  a false pass only); and **a corrected GENERAL guard retires a PER-FAMILY guard only if it reproduces
  every one of that guard's controls and is at least as sharp** — a receiver-discriminating witness
  stays beside a bare-tail one. ⚠ Its reading twin: **a NAME appearing in a hand-owned file cannot
  distinguish a WRAPPER DISPLACEMENT from a CONSUMER-SIDE remedy** — a consumer that transcribes a
  native structure MENTIONS the wrapper it is avoiding, textually identical and semantically opposite —
  so remediation status takes THREE signals (a `manualConversionFuncs` key, the placeholder in the
  generated file, a body) and NAMES the mechanism; anything less ranks the work backwards.
  ⚠ **The ledger's REVERSE arm earns its keep on hoisted literals** (2026-09-02): the converter hoists a
  body's string literals WITH the body, so a displaced body's `…ˢ` literals cease to exist and any
  hand-own referencing one dangles — the reverse side found it before a compile did; a hand-own spells
  its own panic text and depends on no hoist the displacement removes. ⚠ **And that clause names only
  the literals with NO OTHER USER: a SHARED hoisted literal RELOCATES into its next user's file**
  (2026-09-05, runtime's `cannot allocate memory` leaving `malloc.cs` for `mheap.cs`, +3), so the
  two-seeded diff shows the relocation as a THIRD file and a footprint applied without that hunk fails
  CS0103. **Read a displacement's diff for hoists that MOVE, not only for hoists that vanish.**
  ⚠ And a **linkname destination declared in a `_test.go` lands in the INTERNAL-test class, where a
  production-side push cannot reach
  it** (`reflect`'s `gcbits`, provided by runtime via linkname: emitted bodyless into the internal-test
  package and picked up by the throwing partial stub) — completion is the reflectlite pattern,
  registration plus a body in `export_impl_test.cs`, witnessed by the guard's test-side arm.
- **A branch behind master shows master's newer files as DELETIONS.** This is the stale-base illusion,
  not data loss, and it has been mis-read as a lane destroying work more than once. Diff from the
  **merge base** (`git merge-base A B`), never from a moving tip.
  ⚠ **And a BRANCH census takes the THREE-DOT form — `git diff master...branch` — never the two-dot
  tree diff against master's tip** (2026-09-04, the coordinator against itself): with a branch based
  on an older master the two-dot form shows master's OWN newer content REVERSED, which read as 33
  "identifier hits" that were pre-scrub doc lines master had since scrubbed, five "golib files" that
  were other trains' landed golib, and an is-ancestor NO against master's tip — all of it looking like
  a lane's defects. The stale-base illusion in a census's clothes: **the merge-base rule binds every
  branch instrument, the SECURITY census included.** The merge's own file count (63 = 24 + 39) was the
  honest number all along.
  ⚠ **IN A GATE LINE, THE VERDICTS GET CHECKED AND THE ADJECTIVES RIDE IN FREE.** "matched 324, disclosed
  55, 388/388, zero orphaned, zero mint violations" — the first three computed from the record, **the last
  two never measured at all**: one had no source anywhere in the pipeline, the other was an early-return
  check over a manifest with no entries of its class. **A gate line is exactly where the asymmetry hides,
  because the verdict is the thing under scrutiny. Every decoration on a gate line is a claim owing the
  same provenance as the verdict.**
  ⚠ **A GATE INVENTORY IS NOT A GATE RESULT.** A commit listing "converter go test; darwin builds; linux
  build; CNR; integrity per GOOS" with no exit codes, no counts and no timings has published nothing
  falsifiable — and a six-line follow-up seat can drag the whole increment in on it. **Every gate line
  carries a number or an exit code, or it is prose.**
  ⚠ **A STATED REASON FOR SKIPPING A GATE IS ITSELF A CLAIM AND GETS AUDITED LIKE ANY OTHER** — "CNR is
  not re-run and does not need to be: the change is a `_test.go`, so the converter binary is unchanged",
  **on a commit that also changed two non-test converter sources `go build` compiles in.** A skipped gate
  leaves no artifact, so **the JUSTIFICATION is the only thing standing between the reader and an
  unmeasured tree** — and it is justified **by CALL GRAPH, not by file**: "it is only a test file", "the
  binary is unchanged", "the guard change cannot affect emission" are unfalsifiable prose, where *"one
  production caller, reached only through X, which `main.go` guards with `<condition>`, so no `-stdlib`
  run reaches it and the two-seeded diff is zero BY CONSTRUCTION"* can be refuted in one grep.
  ⚠ **And refusing to quote a MEANINGLESS green is the same discipline as catching a vacuous one, and it
  is cheaper**: *"`gofmt -l` is NOT a signal in this tree — the corpus is CRLF, so it lists all 258 files
  at master and here alike. Stated rather than quoted as a pass."*
- **Check the diffstat against the claim BEFORE the push, never after.** A merge whose file list does
  not match what the commit says it does is stopped at that point, not explained afterwards.
  ⚠ **A CORRECTION GETS A CENSUS, NOT A FIX OF THE INSTANCE YOU WERE SHOWN.** A lane announced a fresh SHA
  to fix a refuted claim in a blockquote and left the IDENTICAL claim one sentence away, in the other line
  the same commit had added; censusing the FILE for the claim's SUBSTANCE found THREE — one pre-existing at
  master, one its own commit PRESERVED while correcting the number in front of it, one that same commit
  WROTE from the blockquote it then fixed. **Residual zero BY GREP rather than by reading — and ask whether
  the guard you are adding can see the class of error you just made.**
  ⚠ **A CURATION OR SUMMARY PASS READS AT ONE TREE, and any claim it phrases in the PRESENT TENSE
  inherits that tree — writing it into DOCTRINE converts a stale reading into a standing rule.** A
  doctrine batch curated at one commit drafted a present-tense claim about a converter guard that was
  true there and FALSE at master, the guard having been corrected hours earlier; pasted as drafted it
  introduced into CLAUDE.md exactly the defect its own drop-entry existed to fix (2026-09-06/07).
  **Every present-tense claim in a drafted doctrine entry is re-verified at the tree it will LAND on,
  not the tree it was read at**, and a claim about a FIXED defect is written in the PAST tense with its
  resolution named. ⚠ Two riders. **A fact that lives only in mailbox history is a fact you will
  eventually contradict** — one mechanism was stated exactly right in a coordinator post and published
  in its OPPOSITE form hours later as the stated reason for a ruling: **transport is not record, and a
  finding that decides how a defect gets FIXED belongs in the doctrine file the day it is established.**
  ⚠ **And a REMEDY sentence is the most dangerous place to skip the read**: two successive wrong claims
  were published about one file, the second naming as the remedy a walk that had ALREADY LANDED — **a
  doctrine line naming work that is already done sends its next reader to write code that exists**, and
  it reads as authoritative precisely because it is specific. Name the SHA, open the file, quote the
  lines.
  ⚠ **WHEN A PREMISE DIES, EVERYTHING DERIVED FROM IT DIES WITH IT, INCLUDING THE GENERAL RULE THAT FELT
  INDEPENDENTLY SENSIBLE** — a lane verified its own amendment's premise, found it FALSE, and withdrew the
  class rule as well as the amendment; **the withdrawn generalisation is the part that keeps doctrine
  clean.** Its mirror: **retracting a READING does not retract what you CONCLUDED from it** — a lane
  correctly diagnosed a false empty and an hour later raised an urgent correction whose premise was the
  retracted reading's conclusion. **Re-derive anything the bad reading fed.**
  ⚠ **A CORRECTION NAMES THE POST IT SUPERSEDES BY SHA and names exactly WHICH downstream number is at
  risk**, handing the settling measurement to whoever owns the artifact. Two participants posted the same
  reversal within a minute and one carried an explicit "do not bank `<sha>`"; without it the mailbox holds
  two live claims and a later reader takes whichever it meets first. Pointed at your own artifact: **RECORD
  A WRONG TABLE IN PLACE RATHER THAN QUIETLY FIXING IT — a corrected record hides that the wrong version was
  PLAUSIBLE**, which is what let it pass every check anyone would run.
  ⚠ **"NAME THE LAYER" GETS A RETRACTION WHERE A FLAT CONTRADICTION GETS AN ARGUMENT.** Told its report was
  not reproducible, a lane could have defended it; asked instead WHICH LAYER the label lived on — committed,
  local-uncommitted, or recalled — it looked and retracted in one message: *"I asserted a file's contents
  from memory."* **The question asks the person to LOOK rather than to defend, and it costs the asker
  nothing to be wrong.** Its companion: **a control distinguishes a broken probe from a true zero, including
  when the "broken probe" reading is the FLATTERING one** — offered a face-saving diagnosis, a lane ran the
  control instead, proved its earlier zero TRUE, and refused it.
  ⚠ **A NOTE ARRIVING AFTER A SURPRISING NUMBER IS AN EXPLANATION; THE SAME NOTE BEFORE IT IS A CONTROL —
  and a stated expectation makes an anomaly LEGIBLE EVEN WHEN IT IS NOT THE ANOMALY YOU EXPECTED.** A caveat
  written about a host-conditional COUNT caught a defect in the RUN: the leg exited in two seconds on "No
  banked packages matched filter" — the most shruggable output a sweep produces — and because an expectation
  had been posted the lane looked instead of shrugging, finding a tree 42 commits stale and a forty-minute
  five-leg canary invalid before it started. **The prediction did not have to be about the right failure; it
  only had to make "that is not what should happen here" a thought somebody had.** Beside it: **separate the
  RULING from the CANDIDATE** — a boundary ruling made on a source read, the cause-hypothesis carried with
  an explicit one-string falsifier and an explicit statement that the boundary stands either way — **and a
  falsifier that fires AND reveals it could never have fired is worth more than one that simply fires** (a
  row's stderr was read for a string a SOLO run structurally cannot produce; you can only say "the row was
  never the right instrument" AFTER the run).
  ⚠ **SCOPE-OF-IMPACT IS A SEPARATE CLAIM FROM EXISTENCE AND IT GETS ITS OWN CHECK.** A duplicate manifest
  entry between two branches was posted as a LIVE assembly hazard; only one was boarding, so the train would
  have landed exactly one entry — **"live in this train" was an INFERENCE from having found it during an
  assembly census, and the check was one command, run after the post rather than before.** The finding stood
  in every particular. ⚠ **But judge an ESCALATION by its ASYMMETRY, not by whether it turned out right**:
  raising a stop-the-line concern on a stale premise costs one exchange; not raising it costs the assembly.
  And **a self-audit pays twice, the FALSE finding included** — one returned a real failure (a census
  contradicting a committed record, which pulled the seat) and one INVERTED (a guard declared broken that
  was correct), the second teaching that **a mechanism can be assembled entirely from true parts and still
  point the wrong way.**
  ⚠ **A CORROBORATION IS CITED ONLY AFTER ASKING WHAT THE CORROBORATING TREE CONTAINED** (2026-09-08):
  "the same fix observed through a different instrument" was offered as the external evidence carrying
  a footprint whose OWN positive control had failed — but that rung reached its number by
  HAND-APPLYING the emitted stamp to the compiled file the converter never writes, so it corroborated
  the fix's CORRECTNESS and not that the cut AS LANDED produces that state (the branches compose to
  the unfixed number). **Leaning on an external reading BECAUSE one's own control failed is the wrong
  direction to lean.** ⚠ **And A PROVENANCE STATEMENT IS NOT A SCOPE STATEMENT**: the rung post proved
  WHERE its applied line came from — the emission's line, matched by structure, the converter's own
  bytes, every clause true — and never said WHAT THE CUT ALONE ACHIEVES, so a reader hunting
  corroboration drew the stronger inference the heading invited; one sentence would have closed it.
  **A measurement post says what the TREE CONTAINED, not only where each line came from.** ⚠ The
  two-sided mechanical check, so it never rests on two lanes resolving to read more carefully: **the
  CITER names the TREE the cited measurement ran on** — a number without a tree is a number, not a
  measurement — **and the AUTHOR states IN THE RESULT, not only in a provenance section, any step the
  cut does not perform.** Either sentence alone would have caught a composition defect that fails in
  the direction that punishes the innocent seat. Companions: a cross-branch composition finding
  belongs to the LADDER ROLE, the only tree where both changes coexist, which is the argument for
  announce-before-push putting both readings where the other lane can reach them; and "one line" is
  VERIFIED across the whole re-derived set rather than inferred from the one error that happened to
  show.
- **⚠ A resolver that FAILS must stop the commit** (2026-09-02): a conflict-resolver script's
  assertion failed and the `git add; git commit` chained after it with `;` rather than `&&` committed
  a board carrying three conflict markers — caught only by the marker count printed beside the
  commit, and amended before the push. Chain `python … && git add … && git commit`, and grep every
  merge commit's blobs for `^<<<<<<<` before pushing.
  ⚠ **A CONFLICT-MARKER SCAN OVER `git show HEAD:<path>` IS A SILENT FALSE GREEN FOR FILES WHOSE NAMES
  CARRY CONVERTER GLYPHS** (2026-09-07, found by a land derive): under the default `core.quotepath`,
  git prints such a name octal-escaped, `git show` of that spelling FAILS, and a grep over EMPTY output
  reads 0 markers for a file that was never read — ten of one trio re-seat's 31 files. **Read paths
  NUL-delimited with `-c core.quotepath=false`, and ASSERT every blob read non-zero bytes**, with a
  control proving the unread-detector fires. Any train touching `src/core/golib` inherits the trap.
  ⚠ **A docs seat can split a file's FINAL guard line, and no gate sees it** (2026-09-02): the board's
  closing `endraw` guard was deleted, a bare HTML-comment opener written, 284 lines appended and the
  tail half re-added LAST — so the new section published INSIDE a comment, invisible, while the commit
  read normally. A board-touching merge asserts the structural invariant before it lands: one `raw`,
  one `endraw`, the `endraw` FINAL, zero bare openers.
  ⚠ **Two seats told to append a dated block "at the END" of one record collide BY CONSTRUCTION**
  (2026-09-04, met twice in two files on one train): an add/add at the TAIL is the adjacent-insert
  hunk in its purest form. The resolution is mechanical exactly when both sides are PURE APPENDS over
  the merge base (**asserted, not assumed**) — the merged file is HEAD's bytes plus the other side's
  suffix over the base, a duplicate numbered heading renumbered and STATED in the merge message, and
  the line count asserted as base plus both inserts: both sides kept, neither lost, which is what the
  adjacent-insert rule requires. The instruction that AVOIDS the collision names a per-seat ANCHOR (a
  dated heading the resolver can key on) or generalises the board's append-append resolver to design
  records — **"append at the end" is not an anchor.** Its routing companion: **a correction to another
  lane's ROW is that lane's cut**, and a design-record correction lands as a dated block when the
  owner's branch next touches the section.
  ⚠ **A DOCTRINE LANDING IS MEASURED STRUCTURALLY BEFORE IT IS BELIEVED** (2026-09-04): the
  line-ending count unchanged in KIND (CR == LF), ZERO table lines in the diff when no table was meant
  to move, bullet indentation matched at every insertion point, enumerations intact (an amendment
  written INSIDE a numbered list orphaned an item onto a run-on line and was moved out), and the
  removed-line count ACCOUNTED FOR — re-emitted paragraph anchors plus exactly the wording changes the
  batch itself rules on. A batch's item RANGE is re-counted from the accumulator at landing, never
  taken from the dispatch, and the accumulator's own "BATCH n LANDED" line belongs to the train that
  lands it.
  ⚠ **A LIST NAMES TIPS, AND TIPS CARRY ANCESTORS — a branch's evidence is the evidence of everything it
  drags in.** An audit of fifteen train tips found **eight unlisted rider commits that would board with
  them, and the two worst gate lines in the batch were BOTH riders** (one a count-match satisfied against
  a stale tree, one a gate inventory with no results); every audit and reconciliation in that campaign had
  been conducted at the TIP. This is a RULE INTERACTION rather than anyone's mistake: **announce-then-push
  guarantees that a tip is disproportionately the smallest, most cosmetic commit on the branch**, since
  never rewriting a posted SHA means a correction lands ON TOP and corrections are small (a six-line
  `gofmt` tip over a 569-line hand-own). Do not weaken the SHA rule — **audit the RANGE, `base..tip`,
  never the commit the list points at.**
  ⚠ **SHARED FILES BETWEEN TWO BRANCHES IS EVIDENCE OF ANCESTRY AS OFTEN AS OF CONFLICT.** Three branches
  appeared to collide on EIGHT files; they were one chain three deep, listed three times — twenty-two
  candidates were nineteen. `git merge-base --is-ancestor` over the pairs settles it in one command, and
  the consequence is not cosmetic: **a gate-line defect found on a chain's TIP belongs to the whole
  chain.**
  ⚠ **And ANCESTRY-INDEPENDENCE IS NOT COMPILE-INDEPENDENCE.** A seat verified NOT a descendant of a
  withdrawn branch (`--is-ancestor` rc=1 both ways, none of its five commits) still could not build
  without it: the type it overrides is declared only on that branch, so the union is `CS0246` plus an
  `override` with nothing to override. **Three checks ran — name-prefix (wrong), ancestry (correct), and
  neither was the right question, which was "does it stand up ALONE."** The commit body stated the
  dependency in its own first paragraph and nobody read it.
- **A separated stack must be verified from BOTH branches** — `git log --oneline master..<branch>` on
  each shows what a merge would really carry.
  ⚠ **A HASH IS READ, NEVER EXPANDED FROM A PREFIX.** A `--force-with-lease` was given a full SHA written
  out by hand from the short form — **nine characters of truth and thirty-one of invention.** The lease
  refused, because a fabricated expected-SHA can never match, so the mechanism failed CLOSED **by luck
  rather than by design**, and the dangerous sequel is that a "stale info" rejection invites reaching for
  plain `--force`, which protects nothing. The cheap form is `OLD=$(git ls-remote origin <ref> | cut -f1)`.
  ⚠ **A QUOTED SHA IS RESOLVED BEFORE IT IS ACTED ON** — `git cat-file -e <sha>^{commit}`. An
  announce-then-push protocol runs on SHAs participants TYPE into posts, and a coordinator can verify
  branch TIPS all night with `rev-parse`/`ls-remote` while never once checking that a QUOTED SHA exists.
  **But FETCH before you resolve: "unresolved" has two causes, opposite in severity** — after a fetch it
  is a typo or an invention, before one it is only a stale clone. The rule's first real run flagged two
  SHAs that were another lane's freshly-pushed commits, and treating a stale clone as an invention would
  have meant accusing a lane that had done everything right. **A check written in reaction to a scary
  failure mode is exactly the one most likely to manufacture false alarms about it.**
  ⚠ **AN UNREACHABLE PUBLISHED SHA IS INDISTINGUISHABLE FROM A FABRICATED ONE**, so published provenance
  must be REACHABLE and not merely true when written. A lane published two A/B tree SHAs held by nothing
  but a throwaway detached worktree: `git for-each-ref --contains` returned ZERO for each, so a worktree
  removal or a `gc` would have left the record citing trees that no longer exist — **and a reader applying
  the resolve-before-acting rule to such a SHA gets exactly the failure an INVENTION produces.** Run
  `--contains` before posting a SHA from a detached or scratch worktree, and TAG what nothing else holds.
  ⚠ **A COORDINATOR NAMING A LOCAL-ONLY SHA IN A DISPATCH IS THE UNREACHABLE-SHA RULE VIOLATED BY ITS
  OWN AUTHOR.** An assembled train head and a lane's union were both HTTP 422 on the remote while a
  dispatch told a lane to measure "at the head you named" (2026-09-07). The lane did the two right
  things: RECONSTRUCTED the union from the pushed seats rather than guessing, and posted its TREE SHA
  for the coordinator to compare rather than asserting equality — *"I assert no equality"* — and the
  trees matched to the byte, so the reading counted. **Before naming an assembled head in any dispatch,
  push it as a transient read-only reference ref** (`claude/coord-trainNN-head`, nothing based on it,
  deleted at landing) — **and compare TREE SHAs, not commit SHAs, when a reconstruction stands in for a
  head.** ⚠ Two riders from the same week: **a sub-agent deliverable is NAMED at dispatch**, or two
  derivations collide on one filename (a coordinator hand-derived a battery script while a sub-agent was
  dispatched to write one with nearly the same name; a suffix is what kept them apart); and **a lane's
  ANNOUNCED branch is verified on the remote by `ls-remote` at the MOMENT IT IS NEEDED, not at the moment
  it was announced** — one announced twice, the second a fast-forward, answered "couldn't find remote
  ref" when a train went to fetch it.
- **Re-fetch immediately before any merge in a live campaign.** Refs move under you; arithmetic against
  a SHA you read ten minutes ago is arithmetic against a tree nobody has. ⚠ A rebase REWRITES a SHA
  someone else has already been handed: **never force-push a tip whose SHA has been posted — post
  the fresh SHA first** (paid twice in one day, 2026-09-01, the second time crossing a coordinator
  merge that was reading the old one).
  ⚠ **And the rule binds a FAST-FORWARD too** (2026-09-02, three pushes announced AFTER the fact on the
  reasoning that the announced SHA was still reachable): the reader takes the REMOTE TIP, so an ADD
  moves the thing being read exactly as a rewrite does. The form is **announce, THEN push**, for any
  commit on a branch whose SHA has been posted, whatever the update's shape — and a sentence in an
  already-announced commit is retracted in the MERGE message, never by rewriting the SHA.
  ⚠ **Met again 2026-09-05 as a NON-DESCENDANT replacement, which is the worst shape**: a fix to a
  commit whose SHA has been announced, pushed and VERIFIED is a commit **ON TOP**, never a rewrite —
  one narrowed guard replaced its announced SHA with a non-descendant on the remote, leaving the record
  naming a SHA the branch no longer holds, and **announcing the new SHA first does not license the
  rewrite**. SHA-pinned seats survive it only because the coordinator re-points them, and the record
  states that a rewrite happened. Its automation half: **a land script's branch prune FETCHES the
  branch immediately before its ancestry check** — `origin/<branch>` as last fetched at assembly is the
  SEATED SHA by construction, so a lane that kept cutting on the seated branch would have its unmerged
  commits deleted from the remote while the check read "tip in master" (fixed fetch-then-check).
  ⚠ **A COORDINATOR BROADCAST THAT REPEATS A LANE'S SHA CLAIM WITHOUT `ls-remote` PUBLISHES THE LANE'S
  UNVERIFIED CLAIM WITH THE COORDINATOR'S AUTHORITY** (2026-09-07 19:21). `claude/g-hop-h1 bef7a6dbd`
  was announced, acknowledged, and written into a fleet status post as "holds" — and existed in exactly
  ONE worktree. Every LOCAL check the lane ran passed (real commits, real gates, clean status), because
  **reachability is the one property invisible from inside the worktree that holds the work.** The lane
  mechanised the check into its post tool: a `claude/g-*` token absent from `ls-remote` AND not an
  ancestor of master → REFUSE, the master-ancestor arm being what keeps it usable, since landed
  branches are pruned by design. **The coordinator's post tool takes the same guard for EVERY
  `claude/*` branch token it names — other lanes' included — because a broadcast is the coordinator's
  claim, whoever first made it.** ⚠ And the disposition when the announce-then-push rule is broken by a
  lane's own hand: **a lane that pushed first and SELF-REPORTED before anyone discovered it took the
  correct form, and nothing beyond the note is owed** (G h9-prep e7d3af1e7 → da71a3b7b, 2026-09-07).
- **Three merge mechanics, measured 2026-09-01/02.** **Union CNR is never skipped on composition
  reasoning** — "both sides are transpile-clean, so the union is" is not a verdict, and the case it
  cannot see is exactly the one that bit: a merge carrying a NEW behavioral test cut from an older
  base went red at the union (the `CollidingPackageNames` red). **A conflict dry-run does not need
  `git merge-tree --write-tree`** — that subcommand is unavailable on this box's git; the form that
  works is a temporary-index `read-tree -m --aggressive -i <base> <ours> <theirs>` plus
  `git merge-file -p` over each unmerged path, a 3-way CONTENT check that never touches the
  worktree, so it is legal under the mid-battery source freeze. And **rebase equivalence is checked
  by TREE, not by commit list**: `git diff <merge-of-old-tip> <rebased-tip>` coming back EMPTY is
  what proves a running battery's verdicts transfer to a train rebuilt on new SHAs.
  ⚠ **The ONE verdict a rebase cannot transfer is a GOLDEN's** (2026-09-05): equivalence transfers the
  verdicts of gates that were RUN, and a golden's validity against the UNION's converter was never
  among them — so **a rebase onto a master whose CONVERTER moved owes a CNR of the branch's OWN
  goldens at the rebased tip even when the branch touches no converter file.** One guard's golden had
  been baselined by a pre-train converter and its nine `FromPinnedBox` lines were invisible to the
  union CNR because the rows carrying them arrived WITH the rebase. Remedy: re-baseline as ONE
  one-file commit — transpile with the REBUILT converter at the rebased tip, then `--update-targets` —
  and never classify a known-stale golden's CHANGED at the battery, nor land it.
- **⚠ A merge that touches `package_info.cs` must carry the matching `stdlib-metadata.txt` change —
  check it in the PREFLIGHT.** `stdlib-metadata.txt` is generated FROM the corpus (`go generate .` in
  `src/go2cs`, gated by `TestStdLibMetadataInSync` under the converter's own `go test`), and a corpus
  bank that moves `GoImplement` records without it leaves that guard red for whoever runs the
  converter suite next. Three banked regens missed it in two days (2026-08-24/25) — the step was
  documented and still skipped, because no MERGE checked for it: if
  `git diff --name-only <base>..<branch>` lists a `package_info.cs` but no `stdlib-metadata.txt`,
  stop and have the branch run the generate before it merges.
  ⚠ **A COUNT CHECK READS "UNCHANGED" ON A FILE WHERE SIX ENTRIES MOVED** (2026-09-06): master 59
  entries, one seat 56, the other 62, union 59 — **three removed and three different ones added land
  exactly back on master's number.** The coincidence is not rare; it is what removals and additions of
  equal size always produce. Different entries at different offsets merge clean and git marks nothing
  (the silent-duplication class), so a registry several seats add to needs a **POST-MERGE ASSERTION, not
  a resolution**: on the MERGED file, every entry from every seat present and none duplicated,
  arithmetically, never by eyeballing conflict markers that never appeared.
  ⚠ **And key on the TUPLE the data model uses, not on the name.** "Each registry key appears exactly
  once" false-fires on a name that legitimately exists under two package maps — `manualConversionFuncs`
  carries `"pipe"` under BOTH `runtime` and `syscall` at master — because a duplicate inside ONE map
  would not compile, so any legitimate second occurrence is necessarily in a different scope and the
  sound key is `(package, name)`. The same correction moved one owed set from 11 entries to 8: three
  showed as `+` lines in the diff and already existed at master — **MOVED, not added — so a checker
  demanding them would have failed a CORRECT merge.**
  ⚠ **ORDINALS are the same class in prose.** Three doctrine branches all appended at one anchor and two
  independently wrote "An eleventh"; because the additions sit in DIFFERENT HUNKS, git produces a clean
  merge containing two "eleventh" items and no gate reads section numbering. The whole fix was ONE WORD,
  found only by extracting every ordinal TOKEN each branch introduces and diffing the sequences.
  **Census the ordinals before merging two seats that append to one numbered list.**
- **⚠ Two branches writing the SAME wrong number auto-merge CLEANLY.** The roster's header is the
  measured case (2026-08-29, the banking window): master and an incoming bank both moved the row
  count 189 → 190 — identical text on both sides, so git folded them without a conflict while the
  union's truth was 191. The silent-duplication rule's arithmetic twin: at any multi-branch window,
  header/summary numbers are RECOMPOSED from the merged table, never accepted from either side, and
  the format guard (guard-as-calculator) runs after EVERY resolution — it caught this one and a
  hand-composed Linux-denominator slip the same evening.
  ⚠ **A COUNT IS NOT A SET, AND A COPIED NUMBER IS NOT A DERIVED ONE — three readings of one roster,
  2026-09-06.** **QUOTE THE SET, NEVER THE COUNT**: an `os` row read 685 = 683 + 1 + 1, its board
  record 683 = 681 + 1 + 1, and a summary 682 of 686 counting the capability-GATED rows against the
  total — three internally consistent compositions over ONE failure set, so a dispatch quoting a count
  ("four failing verdicts") can be wrong while every number it came from is right. The invariant
  across compositions is the SET. **A TRACKER THAT COPIES A DERIVED NUMBER GOES STALE SILENTLY, AND IN
  THE FLATTERING DIRECTION** — the coordinator's remaining-rows list named a row unowned that had
  banked four days earlier, and a probe spent on it measured nothing but the bookkeeping. ⚠ And the
  obvious remedy is itself a trap, which is the correction worth carrying: **read the
  guard-recomputed HEADER, or count the table rows — never the roster's PROSE derivation.** A
  document's derivation and its computed figure look alike on the page and go stale differently: the
  roster carries a DATED prose derivation ("202 banked, eight remaining", correct on the day it was
  written) beside a header the format guard recomputes from the table on every change (203/210), and
  a coordinator who wrote "do not carry a list, re-read the roster's derived section" then published
  the stale number TWICE — in the post whose own point was that stale counts are dangerous. The prose
  explains HOW a number was reached and is a record of one day's reasoning. Its constructive note:
  every candidate queued for an exclusion ruling that has actually reached a MEASUREMENT has come back
  implementable, three for three, which is the argument for measuring the remainder rather than
  reasoning about it.
  ⚠ **TWO MEASUREMENTS AGREEING ON THE RESIDUALS AND DISAGREEING ON THE DENOMINATOR IS RESOLVED BY THE
  NAME LIST, NEVER BY THE COUNTS — and the disagreement is usually ONE enumeration plus one bad
  EXTRACTION, which only the names can distinguish.** Two hosts read `net/http/pprof` at one base and
  one configuration in 2026-09-06: identical four divergences, identically rooted, host surviving on
  both — and 49 verdict entries against 15. Diffing the two name lists returned **fifteen distinct test
  names on both sides** (four `func Test` fanning out through eleven subtests), matching the roster's
  own historical figure; the 49 was one lane's extraction artifact. Arguing totals cannot separate "we
  enumerated different things" from "we enumerated the same thing and one of us counted it wrong."
  **A row cannot bank on a denominator two hosts disagree about**, and the tell was already visible —
  a before-state of 15 expanding to 49 on a fix nobody claimed had added subtests.
  ⚠ **POST THE BLOCKER, NOT THE SILENCE.** A silence-watch asks "does the lane hold a dispatch?" — a lane
  held NONE and was stuck anyway, on a disk constraint nobody upstream knew about. **The right question is
  "can it run its next step?", which the lane knows immediately and the coordinator cannot see at all.**
  ⚠ **A CLAIM ABOUT A BRANCH'S BASE REPORTED AS A CLAIM ABOUT THE PAIR IS A LAYER ERROR, AND IT CHANGES
  AN ARITHMETIC SOMEBODY IS ABOUT TO SEAT ON.** A lane reported a held branch pair as "missing TWO
  overrides"; measured — detach at one branch, merge the sibling, grep the overrides, abort — the pair
  supplies **six of seven**, because the sibling's ENTIRE diff (1 file, +12 lines) is the override in
  question (2026-09-07). "Neither is in the BASE" was true; "the pair is missing two" was not. **Measure
  an amendment that changes a seat count rather than taking it on trust** — one merge, and the
  coordinator would not have caught it by reading either. ⚠ **Two lanes reaching the same answer for the
  same RECORDED REASON, independently and days apart, is a stronger warrant than either alone** — a
  ruling and an existing implementation converging on a REASON rather than merely on a value is the
  cheapest cross-check a fleet gets, and it is worth recording. ⚠ **And an "honest fit" problem can be
  PRE-EXISTING, and saying so is the finding — not a workaround**: asked whether an enum's documented
  members admit an honest answer for a new kind, the right answer was that they already did not, a
  SIBLING kind having redefined the word at its own site. The new kind joins an existing site-level
  redefinition, and the argument for a further member is real but is **its own increment covering the
  sibling too** — never a blocker for the cut in front of it. **Name the state of the enum, not the
  nearest-looking member.**
  ⚠ **A LANE REPORTING A DISK BLOCKER STATES ITS OWN OUTPUT-DIRECTORY COUNT FIRST** — a 12 GB box against a
  25 GB floor, reported as the machine's constraint, was **598 of the lane's OWN `bin`/`obj`/`Generated`
  directories**: "my box is small" and "my box is full" are different problems with different fixes, and one
  command separates them. Its preventive form: **watch the disk RATE during a long sweep and purge
  PREEMPTIVELY** — 47.7 → 41.2 → 38.1 GB over 34 rows is ~0.28 GB/row, and 139 rows remaining projected
  ~39 GB against 38 available, so it would have died near the END after hours of work; purging the 1,484
  behavioral output directories under `src/tests` — the same worktree, a subtree the sweep never touches —
  freed **40 GB** mid-run without disturbing it. **Project the rate against the remaining rows, and know
  which subtree the running leg actually needs.**
  ⚠ **CAPACITY IS FUNGIBLE ACROSS THE FLEET; OWNERSHIP OF A CLAIM IS NOT.** Twice in one night the remedy
  for a lane that could not run its own next step was another machine running the build **while the OWNING
  lane kept the prediction, the acceptance and the reading** — the borrowing lane posts the raw artifacts
  (exit code, error histogram, record) and does not interpret them. **A lane that borrows a machine does not
  borrow a conclusion.**
  ⚠ **AN ITEM CARRIED AS "BLOCKED" IS A CLAIM, AND CLAIMS GET CHECKED — especially the ones you INHERITED
  rather than measured**; before escalating a block, run the cheapest thing that would disprove it. ⓥ **But
  run the RIGHT cheap thing, and check it against what this file already documents**: `git push --delete
  --dry-run` exiting 0 is **not** a disproof of the remote-branch-deletion block, because the recorded
  failure mode is precisely that **a refused delete answers `Everything up-to-date` with exit 0** (see the
  `ls-remote` mirror in the state-advancing-tool bullet). **The tell is `ls-remote` AFTERWARDS, never the
  exit code.** A blocker inherited from a note and re-reported is indistinguishable from one that exists —
  and so is a blocker "disproven" by the exact signal the block is known to produce.
  ⚠ **THE ANSWER IS USUALLY ALREADY WRITTEN DOWN, AND A QUESTION THAT FEELS LIKE NEW WORK NEEDS A LOOKUP.**
  Three instances in one day: a coordinator asked a lane to name a branch his own drafted-messages directory
  already held (two of his records disagreed and he read the short one — **a distinct failure from guessing:
  the redundancy existed and was not opened**); a stale A/B baseline found by an independent reader going to
  the commit BODIES rather than to a summary; a five-day disposition question settled by ONE grep of the
  file every session loads first. **The failure is never the record — it is that the question FEELS like it
  needs new work when it needs a lookup**, strongest exactly when the answer was written by someone who has
  since compacted or moved on. Second costume: **"I checked master and never checked my own branches"** — a
  question answered against the layer that came to mind rather than the layer that holds the answer.
  ⚠ **A SMART-CARD PIN PROMPT RAISED BY A SIGNING PROCESS SURFACES ON EVERY SESSION OF THE INTERACTIVE
  USER, THE REMOTE-DESKTOP SESSION INCLUDED** — and a dialog dismissed there CANCELS the package in
  flight ("The action was cancelled by the user"), after which the signer moves on and re-prompts for
  the next one (owner, 2026-09-07, two of 307 packages). **PIN prompts are answered only at the
  physical console.** And the coordinator INFERRED "timed out" for a mechanism the owner could state
  first-hand: **ask the person at the console before naming a mechanism for a dialog.**
  ⚠ **A POST'S HEADING IS A SUMMARY, and acting on it before reading the body is acting on someone
  else's COMPRESSION of their own finding.** A lane headed a post "STOP THE DUPLICATE RUN" and, four
  paragraphs down, wrote *"your run at the landed tree measures the union and is still worth having"* —
  their arms were two seats in isolation, the coordinator's was the sixteen-seat merge result, and they
  had said so explicitly. **The run was killed on the heading**, twice in one shift (2026-09-06/07). A
  heading is written to be findable, not to be complete: **read to the end before acting, especially
  when the action is destructive or expensive.** ⚠ Its instrument twin: **a TRUNCATED VIEW of N records
  invites a generalisation the records do not support.** Seven divergent rows were characterised as all
  wanting one runtime symbol; only FOUR do — the claim came from reading each row's output at a
  110-character truncation, enough to see the shape they shared and not enough to see where they
  differed. **A truncation is a sampling instrument: it shows the head of every record and hides the
  tail of all of them, so it systematically OVER-REPORTS HOMOGENEITY.** Read the full field for at least
  the rows a claim quantifies, and say so when a count is asserted over records viewed truncated.
  ⚠ **REHEARSE A BANK AGAINST THE GUARD; DO NOT STAGE IT AGAINST YOUR OWN READING.** `check-roster-format.ps1`
  over a prepared `os` bank ran 615 checks at 204 rows and 2 failed: the matching-verdict TOTAL and the
  disclosed TOTAL, both stale — **the row count and both percentages, the three figures a human eye checks,
  were correct and the two only an adding machine checks were wrong.** A drafted row plus a validated manifest
  is NOT a rehearsed bank. ⓥ **And a guard that ITERATES ROSTER ROWS is blind to a package that has no row
  yet — the state of EVERY package at the moment it banks**: `os`'s disclosure manifest banked with no arm
  reading it in either direction. Fixed at master (section 2c of that guard now enumerates manifests from the
  FILESYSTEM — 45 committed, three of them belonging to packages with no roster row), but the class is
  durable: **check where a guard's population comes FROM before trusting its verdict at a first landing.**
  ⚠ **A GUARD-AS-CALCULATOR THAT ALSO POLICES PROSE IS WORTH MORE THAN ONE THAT ONLY CHECKS THE
  HEADER.** `check-roster-format.ps1` refused a bare `205 / 210 — 97.6%` written as a HYPOTHETICAL —
  *what banking would have looked like* — because its rule is that every prose ratio is either the LIVE
  figure or carries `as of YYYY-MM-DD` (2026-09-07). **It is right: nothing distinguishes a hypothetical
  ratio from a stale one to a later reader**, and the author is the last person to see the ambiguity.
  State the alternative without the ratio form, or date it.
  ⚠ **A PRECEDENT FOR TEXT-GREPPING A SCRIPT earns its keep only where the two sides can DRIFT.** Tests that
  read a regex literal out of a harness script are sound because the pattern must stay in lockstep with an
  independently-maintained predicate — the test measures the DRIFT. **A test asserting that a test file still
  contains its own test names measures nothing: it asserts arms EXIST, not that they FIRE**, which is route
  #8's vacuous shape. Check whether the precedent's two sides are genuinely independent before borrowing it.
- **A liveness/health probe must be able to OBSERVE the thing it asks about** (2026-08-29, the iter
  lane): a process filter on the worktree path can never match `dotnet.exe` running from Program
  Files, so a healthy 18-minute build read as reaped and was reported as owed. Silence is not
  evidence of death any more than exit 0 is evidence of success — the rule cuts both directions:
  read the output, and first verify the check CAN see its target (positive-control the probe the
  way gates are positive-controlled). ⚠ Met again 2026-09-04 in its simplest form: a probe looking for
  `go2cs`/`dotnet` while the suite runs as `BehavioralRunner.exe` reported a healthy run as dead —
  **name the process you are actually waiting on.**
  ⚠ **"Armed" is a claim about a task verifiably STILL RUNNING** (2026-09-02): a task id that has
  EXITED is evidence of a PAST arming, and a lane went silent for hours with BOTH legs down — its
  exit-on-change watcher had fired on the lane's own post and was never re-armed, while the backstop
  that exists to catch exactly that first failure was itself gone. A protocol step that must be
  remembered at the end of the busiest turn, and whose failure is silent, fails on a schedule: DELETE
  the step (a persistent monitor needs no re-arm on a local lane; on the cloud-container class it is
  hard-capped at ~30 min, so there the relaunch leg is load-bearing) rather than reminding harder, and
  back it with a leg that verifies LIVENESS, not existence, and checks its own existence on every
  firing. Its reading
  half: a filter built from expectations can be simply where you stopped reading — read every numbered
  item of a post addressed to you, and read anchor..tip before starting the next one.
- **Positive-control the DETECTOR, not just the gate** (2026-08-30, the pinning census guard): a
  BOM-less `.ps1` under Windows PowerShell 5.1 mis-reads non-ASCII literals through the system
  codepage, so a guard's `ᴋ`-matching regex was silently broken and its "0 findings" red was
  accidentally right for the wrong reason. A new false-signal species: a red whose detector is
  dead. Any regex-bearing guard on PS 5.1 gets a BOM if it carries non-ASCII, and gets its
  detection deliberately regressed once before its verdicts are believed.
  ⚠ Same species one layer up (2026-09-02, met independently by two lanes): a checker printed
  **PARSES CLEAN** while its own `[ref]` binding had thrown on an undeclared variable — the `else`
  branch prints clean regardless. Declare a checker's ref targets, and run it once against a
  deliberately BROKEN copy before believing any "clean".
- **GC/liveness probes: ONE ARM PER PROCESS** (2026-08-30, the StringData lane): running probe
  arms back-to-back contaminates them — an in-frame arm's object collects as soon as a LATER arm
  clobbers the frame, so only the last arm's reading is honest; three arms flipped verdicts on
  run order before isolation. Same family as the tier-0 finding: what the frame holds decides
  what collects, so each measurement gets a fresh process.
  ⚠ **A FITTING story is not a root — and this family's most convincing one was measured FALSE**
  (2026-09-02). A non-optimizing JIT roots every local for its method's life, so a test looping
  `runtime.GC()` for finalizers cannot see them become due at Debug; the mechanism is real,
  `mfinal.cs`'s own comment predicted it, and it fit `TestSplicePipePool`'s symptom perfectly — total,
  permanent, immune to repeated GC. One one-axis run killed it: `internal/poll` at Release+TC0 fails
  IDENTICALLY to Debug (zero rows moved, identical fd set, 2.6 s across a 54-minute window). Four
  candidates are measured out now — SetFinalizer keying, `sync.Pool` aging, the `runtime.GC` sequence,
  the JIT tier — and after four the next step is an INSTRUMENT (a heap root-path read), never a fifth
  hypothesis. Prediction-on-record is what made that run decisive, and what makes a falsification
  cheap.
  ⚠ **The instrument arrived, and it named a GC-LIVENESS DIVERGENCE CLASS** (2026-09-03, rooted
  verbatim with `dotnet-dump gcroot`): `TestSplicePipePool`'s 64 pipe boxes were rooted from **three
  slots of the test's OWN frame** — slice-header copies made at `append` and at the range loop, with
  the pool chains empty and the finalizers unreached. Go's precise stack maps report those copies dead
  after the loop; the CLR reports untracked struct locals live for the whole method, **at every
  configuration**. That is the `sync` `TestOnceXGC` class: disclose by SIGNATURE with the mechanism,
  after checking whether any slot is a converter-minted local a `= default` after the loop could null.
  ⚠ The addendum is what makes it a class rather than an emission bug: the three roots are
  append-result and JIT-spill copies with NO source-level name, and removing the one EMISSION-level
  copy (the range enumeration, replaced by an index loop, dll verified newer than the patched source)
  left 64 boxes / 67 sentinels UNCHANGED — the emission remedy FALSIFIED, not merely unchosen.
  **"Right about WHERE, wrong about WHY" is still a wrong story until the arm runs.**
  ⚠ **The ONE-AXIS PAIR is what earns the MECHANISM sentence, and three 2026-09-06 readings say what
  it bought.** Retained with the frame slot LIVE, collected with it OVERWRITTEN, at the configuration of
  record — so the pin is the caller's FRAME SLOT and the by-value hand-off adds none, a fourth arm
  tracking the overwrite arm to the letter proving the second half. Had both arms read retained, a
  sentence copied from the neighbouring disclosed family would have been WRONG, which is exactly what
  the pair existed to prevent, and it cost one process. **Optimization honours an OVERWRITTEN slot and
  does not rescue a LIVE one**: measured across three configurations, a slot that is merely DEAD — still
  in scope, never read again — is freed at NONE of them, while an overwritten slot is freed only at the
  optimizing configuration; that is the mechanism under the claim that a family of rows joins once
  conservative liveness is optimized away. And **a SOURCE-LEVEL exoneration is PROVISIONAL until a
  one-axis arm carries it**: the coordinator exonerated an intern path by reading the code, the lane
  treated that as provisional and added an arm — the same body plus the real call, the handle kept alive
  so the map is genuinely live — which read IDENTICAL to its reference in all four columns. Six readings
  then partitioned into two families with nothing left over, every candidate but the frame slot
  eliminated by a control differing in exactly one axis. **A source reading and a live measurement are
  different evidence, and the gap between them is where an exoneration hides a real retention if the
  code has drifted.**
- **The `-tests` graph invariant (ruled 2026-08-30, from the W1 arc):** a `-tests` conversion's
  production emission may differ from `-stdlib`'s only in ways that do NOT change the project
  GRAPH. The documented closure families all change file text and no reference; the
  `canUseLongPaths` csproj flip was the first edge-mover and it was fatal (6 cycles), which is
  the boundary's proof. Mechanical form: `check-solution-integrity.ps1`'s per-GOOS cycle
  assertion (G2), whose positive control injects the historical edge and requires exactly the
  six named cycles.
  ⚠ **MEASURE THE PRECONDITION EVEN WHEN THE ANSWER IS THE ONE THE SYSTEM ALREADY IMPLEMENTS**
  (2026-09-06): a profile push's graph invariant came back **38/36/36 cycles for the direction Go's
  directive names and 0 for the inverse** — a shape the converter would never emit — so the design was
  unchanged in OUTCOME and would have been ASSUMING ITS OWN ANSWER without the run. Go's own directive
  ARITY then split the eight destinations one-to-one onto two existing registries (a one-argument
  handle authorizing a PULL, the two-argument form PUSHING), which came from reading Go's text rather
  than designing around it.

