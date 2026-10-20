# Disclosure reasons: rewording style sheet

> **STATE: RATIFIED 2026-10-15. It is applied at H10's disclosure re-sign.** Every stdlib manifest
> (`src/core/**/go2cs_test_disclosures.json`) is re-signed against the incoming release at H10 anyway, so
> the rewording rides that pass and costs one pass instead of two. The S4 wording guard
> (`src/go2cs/internal/repoguard/moduleDisclosureWording_test.go`) extends to these manifests **after**
> H10. It would be red until then by design, so it is not built before H10.
>
> This sheet is written to be applied mechanically: §3 gives literal before → after replacements for the
> templated families, §4 gives the vocabulary for the rest, and §6 gives the check that says when a file
> is done.

<!-- C2, 2026-10-15, at master 1dbde1dbcb. Commissioned by COORD the same day ("It is a REWORDING campaign,
     not a guard seat. It rides H10's DISCLOSURE RE-SIGN ... Please write the rewording STYLE SHEET now"),
     after the C2 count post of 2026-10-15 09:50Z: 251 of 362 rendered reason strings carry a fleet word
     from the module guard's list, 125 distinct texts, in 35 of the 48 manifests; 274 with lane and box
     names. Every count below was re-read at 1dbde1dbcb with the scripts in §6. The §6 check also matches P1
     and branch names, which the count post did not: 59 runtime strings name P1. That raises the total to 320
     strings in 38 manifests. -->

## 1. Why: the reason is a public sentence

`validationProofPages.go` writes each disclosure's `reason` verbatim into the **Pinned reason** column of
`docs/validation/<release>/<package>.md`. For a two-half entry it writes each half's `reason`
(`halves[].reason`). Those pages are the public proof behind every roster row: bufio's single reason
appears on 17 release pages. A visitor reading one needs two things: **what the converted program does
differently** (the mechanism) and **how that was established** (the evidence). Who decided the class, on
which day and in which channel tells a visitor nothing, so it does not belong in the sentence.

`want`, `reading`, `plan`, `notes` and the other fields are **not rendered**. The page links the JSON on
GitHub, so they are public, but they are not on the page.

## 2. Scope (ruled 2026-10-15)

| Field | Reworded? |
|:--|:--|
| `reason` | **Yes**, every one. |
| `halves[].reason` | **Yes**, every one. |
| `plan`, `reading`, `notes`, `want`, `records`, `floor`, `proof` | Only where the same JSON line also carries a rendered field. Otherwise leave them as they are. |
| `signature`, `class`, `name`, `platforms` | **Never.** The signature is matched against failure text, so changing it changes behaviour. |

**Remove from rendered text:**

- the module guard's words: `COORD`, `BOARD` (uppercase, whole word), and in any case `ruling`, `rulings`,
  `ruled`, `lane`, `lanes`, `bank`, `banks`, `banked`;
- `coordinator`, and `OWNER` used as a decision-maker ("the OWNER ruled …");
- lane and box names: `C1`, `C2`, `P1`, `P2`, `i7`, `i9`, `R-LAPTOP`, `G-LAPTOP`, and a single lane
  letter used as an author ("G's reading");
- Q-numbers: `Q44`, `Q28`, `SUB-Q27`;
- `docs/phase4/...` paths;
- branch names (`claude/p1-runtime-fixable`, `claude/coord-h10-readings`). They carry lane names in lower case.
  Keep the SHA beside a branch name and drop the name.

**May stay:** commit SHAs, as provenance. Dates, Go releases, platforms and build conditions are
evidence, so they stay too.

**Not in the ruled list; recommended.** References into the fleet's process channels: `ledger <sha or
time>`, `mailbox <sha>`, `train <n>`, `seat`, `sweep`, and the decision ladder's arm names ("the COUNT arm
of the 2026-09-05 ladder"). They pass the guard but read as jargon. Replace them where the sentence
survives without them, and leave them where removing them would change the claim.

**Rewording never changes a claim.** If a claim reads stale while you reword it ("DOES NOT FIRE YET",
"lands in its own converter seat"), report it as a re-sign finding and leave the claim's meaning alone.
The re-sign checks whether the claim is still true; the rewording pass does not.

## 3. The templated families: literal replacements

At 1dbde1dbcb five families account for most of the hits. Apply them first, as exact string
replacements, in the order given.

### F1. The reclassification clause: 127 strings, 133 occurrences

**Shape:** `RELABEL|REVERSED <date> (<who, under what decision>): <old class> -> <new class>[, <detail>].`

**Rule:** keep the date, the two classes and any **measurement** conditions (a SHA, Release, tiering off).
Drop the parenthetical's decision trail (lane, box, ledger, ruling, seat). The verb becomes
**Reclassified**.

| # | Before (exact) | After | Strings |
|:--|:--|:--|--:|
| 1a | `RELABEL 2026-09-23 (C1, from the i7 reading run at bb54ff0920, Release with tiering off): alloc-profile -> deferred.` | `Reclassified 2026-09-23 from alloc-profile to deferred, on a reading run at bb54ff0920 (Release, tiering off).` | 70 |
| 1b | `RELABEL 2026-09-23 (C1, as ruled at ledger 2026-09-23 03:37 O2): alloc-profile -> deferred.` (also `O1`, `O3`, `O5`) | `Reclassified 2026-09-23 from alloc-profile to deferred.` | 21 + 7 + 1 + 7 |
| 1c | `RELABEL 2026-09-23 (C1, as ruled at ledger 2026-09-23 03:37 O4): alloc-count-semantics -> deferred.` | `Reclassified 2026-09-23 from alloc-count-semantics to deferred.` | 5 |
| 1d | `REVERSED 2026-09-23 (C1, as ruled at ledger 2026-09-23 03:37 X(3)): structural -> deferred, no floor.` | `Reclassified 2026-09-23 from structural back to deferred, with no floor.` | 6 |
| 1e | `RELABEL 2026-09-23 (C1): alloc-profile -> structural, as ruled (edwards25519 and nistec pre-ruled, XAES ruled 2026-09-22).` | `Reclassified 2026-09-23 from alloc-profile to structural, as edwards25519 and nistec already were (XAES since 2026-09-22).` | 6 |
| 1f | `RELABEL 2026-09-27 (C1, on COORD's seat of 2026-09-27 11:50, ledger 9760611653, under the H10 rulings of ledger 2026-09-23 03:37): alloc-profile -> alloc-count-semantics.` | `Reclassified 2026-09-27 from alloc-profile to alloc-count-semantics.` | 3 |
| 1g | `RELABEL 2026-09-23 (C1, from the i7 reading run at bb54ff0920, Release with tiering off; claude/coord-h10-readings ac9f8251ee): alloc-profile -> alloc-count-semantics.` | `Reclassified 2026-09-23 from alloc-profile to alloc-count-semantics, on a reading run at bb54ff0920 (Release, tiering off; readings at ac9f8251ee).` | 2 |
| 1h | `RELABEL 2026-09-23 (C1, as ruled -- COORD's net-reading ruling, 2026-09-23): RULING OWED -> deferred.` | `Classified deferred 2026-09-23.` | 2 |
| 1i | `RELABEL 2026-09-27 (COORD, on the i7 readings requested at ledger 2026-09-27 14:58): alloc-profile -> STRUCTURAL.` | `Reclassified 2026-09-27 from alloc-profile to structural, on a reading run.` | 1 |
| 1j | `RELABEL 2026-09-23 (C1, from the i7 reading run at bb54ff0920, Release with tiering off): alloc-count-semantics -> deferred.` | `Reclassified 2026-09-23 from alloc-count-semantics to deferred, on a reading run at bb54ff0920 (Release, tiering off).` | 1 |
| 1k | `RELABEL 2026-09-23 (C1, as ruled at ledger 2026-09-23 03:37 O4): alloc-count-semantics -> STRUCTURAL, on a RE-GROUNDED proof (not the edwards/nistec analogy, which X(3) reverses).` | `Reclassified 2026-09-23 from alloc-count-semantics to structural, on its own proof rather than the edwards25519/nistec analogy.` | 1 |

`alloc-profile` is a class name that no longer exists. Keep it in the "from" position: the sentence
records what the entry used to say.

### F2. The decision trailer: 91 strings, 105 occurrences (counted as `coordinator ruling|approval`, any case)

**Shape:** a closing sentence or parenthetical that names who approved the entry and when:
`Coordinator ruling, <date>.` / `Coordinator approval, <date>` / `(coordinator ruling <date>[, mailbox <sha>])`
/ `the OWNER RULING of <date> <time> (ledger <sha>)`.

**Rule:** **delete it.** It carries no evidence. If it was the reason's only date, and the date matters
(the reason says "measured" without a date), keep the date as `Recorded <date>.` Otherwise drop it
whole.

| Before (exact) | After | Strings |
|:--|:--|--:|
| ` Coordinator ruling, 2026-09-22.` (sentence-final) | *(deleted)* | 58 |
| ` Coordinator ruling, 2026-09-02` (sentence-final, no stop) | *(deleted)* | 14 |
| ` Coordinator ruling, 2026-08-29` | *(deleted)* | 1 |
| ` Coordinator approval, 2026-09-02` | *(deleted)* | 1 |
| `(coordinator ruling 2026-09-03, mailbox 82ec6654c)` and the other parenthetical forms | *(the parenthetical deleted; fix the spacing around it)* | the rest |

### F3. The heap-box plan: 54 strings (net/netip), two phrases each

The plan's own name is fleet shorthand. A visitor needs to know that a **named plan** removes the
sites, and the `plan` field (unrendered) already carries the design pointer.

| Before (exact) | After | Strings |
|:--|:--|--:|
| `Reduction of exactly these sites is the banked zh-box reduction arc's netip harvest;` | `Removing exactly these sites is part of the planned heap-box reduction for net/netip;` | 54 |
| `the family this reason already assigns to the zh-box netip harvest.` | `the family this reason already assigns to that plan.` | 54 |

The 15 other "zh-box" strings (bufio, strings, slices, io, testing, crypto/md5, crypto/rsa,
crypto/internal/fips140test) cite design documents by file and line. Treat them under §4's
design-reference rule.

### F4. The order-token refusal: 44 strings (internal/runtime/maps)

`Q44 section 10.3 arm-2a` names a design decision. The mechanism behind it is that golib refuses to
dereference a pointer order token as a Go-layout address, and panics with
`dereference of a managed pointer with no address`. That panic text is the entry's signature.

| Before (exact) | After | Strings |
|:--|:--|--:|
| `and its first dereference takes the ruled Q44 section 10.3 arm-2a refusal by name -- the signature below --` | `and golib refuses its first dereference by name -- the signature below, the panic for dereferencing a managed pointer that has no address --` | 44 |

The same 44 strings end with F2's `Coordinator ruling, 2026-09-22.`, which is deleted.

### F5. The measurement attribution: 59 strings (runtime)

**Shape:** `MEASURED on linux (P1, <tree>, <build conditions>)` and its companion sentence, which names who did
not measure the other platforms.

**Rule:** keep the platform, the SHAs and the build conditions. Drop the lane, the branch names and
`TRAIN`. The companion becomes a plain statement of which platforms were not measured.

| Before (exact) | After | Strings |
|:--|:--|--:|
| `MEASURED on linux (P1, master 2ff42f7a16 plus the P1 disclosure seat, Release, tiered off, .NET 10.0.12, go1.24.13, the full runtime row of 10,883 verdicts).` | `MEASURED on linux (master 2ff42f7a16 plus these disclosure entries, Release, tiered off, .NET 10.0.12, go1.24.13, the full runtime row of 10,883 verdicts).` | 40 |
| `MEASURED on linux (P1, the P1 union tree (claude/p1-runtime-fixable bdfbd2244f + the refusal cuts + the disclosure seat 7c27acb6c5 + TRAIN I 61cf81290c, P1's measurement merge), linux, Release with tiering off, .NET 10.0.12, go1.24.13, the full 120 m runtime row of that tree, 10,883 verdicts; the six allocation and layout rows read the same counts in the full row of claude/p1-runtime-fixable bdfbd2244f alone).` | `MEASURED on linux (a tree joining bdfbd2244f, the refusal changes, the disclosure entries 7c27acb6c5 and 61cf81290c; Release with tiering off, .NET 10.0.12, go1.24.13, the full 120 m runtime row of that tree, 10,883 verdicts; the six allocation and layout rows read the same counts at bdfbd2244f alone).` | 16 |
| `MEASURED on linux (P1, claude/p1-unsafe-slice-length on claude/p1-runtime-ifacehash, Release with tiering off, .NET 10.0.12, go1.24.13, the row read by name).` | `MEASURED on linux (a build carrying the unsafe-slice-length fix, Release with tiering off, .NET 10.0.12, go1.24.13, the row read by name).` | 1 |
| `MEASURED on linux (P1, the stack 959a88df3 plus the metrics-histogram fix 0a074655e, Release, tiered off)` | `MEASURED on linux (959a88df3 plus the metrics-histogram fix 0a074655e, Release, tiered off)` | 1 |
| `Windows and darwin are NOT read by P1.` | `Windows and darwin were not measured for this entry.` | 16 |
| `P1 did not read windows.` | `Windows was not measured here.` | 6 |
| `Windows was read by the i9, not by P1.` | `Windows was measured separately.` | 2 |
| `the i9's census (TRAIN I union 9e1594dcd2)` / `the i9's census` | `the Windows census at 9e1594dcd2` / `the Windows census` | 4 / 6 |

## 4. The rest: vocabulary

After §3, the residue is hand-written text. Reword it one sentence at a time with this table, keeping
the sentence's claim.

| Fleet form | Visitor form |
|:--|:--|
| `banked` (a row, a measurement) | `validated`, or `recorded` for a measurement |
| `the 1.23.12 bank` | `the 1.23.12 validation` |
| `ruled X` / `as ruled` / `a ruled correctness decision` | state X as the property it is: `a deliberate correctness decision`, `classed X because …` |
| `same channel, same ruling` | `same channel, same class` |
| `the standing host-identity ruling forecloses …` | `which the host never does: it never claims <file> as its own` (the mechanism, not the rule's name) |
| `the OWNER ruled the family STRUCTURAL on <date> (ledger …)` | delete; the `class` field already says it |
| `(i9, mailbox d0687ff2)` / `Windows was read by the i9, not by P1` | `(measured on Windows)` / delete |
| `the i9's windows census (CENSUS-….md, TRAIN I union 9e1594dcd2 …)` | `the Windows census at 9e1594dcd2` |
| `G's reading, ledger <time>, is why …` | `as a separate reading showed, …` |
| `This is the row SUB-Q27 measured as …` | `This row was measured as …` |
| `train 22`, `in their own converter seat` | `in a converter change`, `a later converter change` |
| design-document citations (`DESIGN-zh-box-reduction.md:513-532`, `ConversionStrategies-Reference.md:21461`) | name what the cited text says, in the sentence. Leave the file pointer to the unrendered `plan` field. A `docs/phase4/` path is never rendered. |

## 5. Worked examples: one per class

Each pair is a real rendered reason at 1dbde1dbcb, chosen as the shortest one with a hit in its class.
**Bold** marks the changed text. The signature, class and name stay exactly as they are.

**alloc-count-semantics** (reflect `TestSmallZero`)
- Before: … allocates nothing; the managed bridge must materialize a boxed zero for the same Value.
  RELABEL 2026-09-27 (C1, on COORD's seat of 2026-09-27 11:50, ledger 9760611653, under the H10 rulings of
  ledger 2026-09-23 03:37): alloc-profile -> alloc-count-semantics. The run's own unit note is BYTES, golib's
  counter charging none of it, so no object count exists to compare with the want of zero: the ladder's
  incomparable-unit arm. Nothing to retire and no plan.
- After: … allocates nothing; the managed bridge must materialize a boxed zero for the same Value.
  **Reclassified 2026-09-27 from alloc-profile to alloc-count-semantics.** The run's own unit note is BYTES,
  golib's counter charging none of it, so no object count exists to compare with the want of zero**: the
  units are incomparable.** Nothing to retire and no plan.

**cgo-configuration** (os/signal `TestAllThreadsSyscallSignals`; first two sentences and the last)
- Before: CLASS NOTE (coordinator ruling 2026-09-03, mailbox 82ec6654c): this entry carries
  `cgo-configuration`, … The ruling is to make the PIPELINE admit the class rather than re-label the
  entries, because the class name carries WHY (the cgo axis, ruled the state of record the same day) and
  re-labelling would throw that away; the admitting arm, its positive control and syscall's missing
  TestUseCgroupFD entry land in their own converter seat, and this row validates at its re-annotated
  `linux: 28 + 2` only once that seat lands. Both are train 22, so the landed master carries them together.
  … The row's Linux line was banked (29 + 2) under the Linux host's default cgo-ON, …
- After: CLASS NOTE: this entry carries `cgo-configuration`, … **The pipeline is to admit the class rather
  than the entries being re-labelled**, because the class name carries WHY (the cgo axis, **the state of
  record since 2026-09-03**) and re-labelling would throw that away; the admitting arm, its positive control
  and syscall's missing TestUseCgroupFD entry land in **a converter change**, and this row validates at its
  re-annotated `linux: 28 + 2` only once that change lands. **[sentence deleted]** … The row's Linux line was
  **validated** (29 + 2) under the Linux host's default cgo-ON, …
- Re-sign flag: "DOES NOT FIRE YET" and "only once that change lands" are claims about the pipeline at the
  time of writing. Check them at the re-sign, per §2.

**codegen-liveness** (crypto/tls `TestCertCache`)
- Before: … and sync's banked TestPoolGC measures 98 of 100 objects finalizing on the first try …
- After: … and sync's **validated** TestPoolGC measures 98 of 100 objects finalizing on the first try …

**compiler-property** (runtime/pprof `TestCPUProfileInlining`)
- Before: containsInlinedCall(inlinedCaller, 4<<10) at pprof_test.go:284. Go's own compiler-inlining
  self-check, the first statement of the test, and the OWNER ruled the family STRUCTURAL on 2026-09-26
  (ledger 4a122cd994, class I of docs/phase4/DESIGN-managed-profiling.md). The check scans …
- After: containsInlinedCall(inlinedCaller, 4<<10) at pprof_test.go:284. Go's own compiler-inlining
  self-check, the first statement of the test. The check scans …

**deferred** (bytes `TestWriteAppend`)
- Before: … where Go's sufficiently-sized buffer appends allocation-free. RELABEL 2026-09-23 (C1, as ruled
  at ledger 2026-09-23 03:37 O1): alloc-profile -> deferred. Read at strconv/itoa.cs:89: …
- After: … where Go's sufficiently-sized buffer appends allocation-free. **Reclassified 2026-09-23 from
  alloc-profile to deferred.** Read at strconv/itoa.cs:89: …

**host-fatal** (runtime/pprof `TestBlockMutexProfileInlineExpansion`)
- Before: … This is the row SUB-Q27 measured as the package's host-killer at alphabetical position 2, behind
  which 180 rows went unmeasured. …
- After: … **This row was measured** as the package's host-killer at alphabetical position 2, behind which
  180 rows went unmeasured. …

**host-identity** (testing `TestMorePanic`)
- Before: … where Go prints its panic layout. Same channel, same ruling. Retires only if the host is made to
  impersonate Go's testing-package terminal output, which the standing host-identity ruling (2026-08-26)
  forecloses: the host never claims testing/testing.go.
- After: … where Go prints its panic layout. Same channel, same **class**. Retires only if the host is made
  to impersonate Go's testing-package terminal output, which **it never does**: the host never claims
  testing/testing.go.

**host-limit** (os/exec `TestCredentialNoSetGroups`)
- Before: … and the reason it lacks it is a ruled correctness decision rather than an omission: … and the
  Windows count is complete without it. Coordinator ruling, 2026-08-29
- After: … and the reason it lacks it is a **deliberate** correctness decision rather than an omission: …
  and the Windows count is complete without it.

**platform-skip** (syscall `TestUnshareUidGidMapping`)
- Before: … It was never minted because it never diverged where the Linux row was banked: as ROOT, Go skips
  it itself (exec_linux_test.go:345, 'fails with privileges'), so the 1.23.12 bank read skip/skip; …
- After: … It was never minted because it never diverged where the Linux row was **first validated**: as
  ROOT, Go skips it itself (exec_linux_test.go:345, 'fails with privileges'), so the 1.23.12 **validation**
  read skip/skip; …

**runtime-capability** (runtime/pprof `TestMorestack`)
- Before: Go PASS / C# SKIP, admitted by the OWNER RULING of 2026-09-28 03:17 (ledger 435557647c). The test
  requires samples in runtime.newstack beneath growstack; … and the test takes Go's issue-13841 skip path
  (G's reading, ledger 2026-09-28 03:14, is why Go's per-OS false is unusable). …
- After: Go PASS / C# SKIP**, admitted as a disclosure on 2026-09-28.** The test requires samples in
  runtime.newstack beneath growstack; … and the test takes Go's issue-13841 skip path **(a separate reading
  showed why Go's per-OS false is unusable)**. …

**structural** (runtime `TestReadMetricsConsistency`, a half)
- Before: structural (ruled 2026-09-28, ledger 96edeb700a): /gc/scan/{heap,stack,globals}:bytes read … so
  no managed measurement means the same quantity, and the honest reading is 0 (DESIGN-readmemstats-surface's
  rule: a field is answered only when a managed measurement means what the Go field means). …
- After: structural **(2026-09-28)**: /gc/scan/{heap,stack,globals}:bytes read … so no managed measurement
  means the same quantity, and the honest reading is 0 (**the surface's rule**: a field is answered only when
  a managed measurement means what the Go field means). …

## 6. When a file is done

A manifest is done when all three hold:

1. **No hit.** The check below prints nothing for it.
2. **Only reasons moved.** `git diff --word-diff` on the file shows changes only inside `"reason"` values
   (and the unrendered fields §2 allows), and the file still parses as JSON.
3. **Same signatures.** The `signature` values are byte-identical before and after. The re-sign's own run
   is the behavioural check: a reworded reason cannot change a verdict, and a changed signature can.

The check, run from the repository root (Python 3, no dependencies). It reads only the rendered fields,
using the module guard's words plus §2's lane, box, Q-number, path and branch-name shapes:

```python
import json, pathlib, re
pat = re.compile(r'\b(?:COORD|BOARD)\b|(?i:\b(?:rul(?:ing|ings|ed)|lanes?|bank(?:s|ed)?|coordinator)\b)'
                 r'|\b(?:C1|C2|P1|P2|i7|i9|R-LAPTOP|G-LAPTOP)\b|\b(?:SUB-)?Q\d+\b|docs/phase4|\bclaude/[\w./-]+')
for p in sorted(pathlib.Path('src/core').rglob('go2cs_test_disclosures.json')):
    for d in json.loads(p.read_text(encoding='utf-8-sig'))['disclosures']:
        texts = [d.get('reason')] + [h.get('reason') for h in d.get('halves') or [] if isinstance(h, dict)]
        for t in texts:
            if isinstance(t, str) and pat.search(t):
                print(f"{p.parent.as_posix()} {d['name']}: {sorted(set(pat.findall(t)))}")
```

At 1dbde1dbcb it prints 320 lines in 38 manifests. Each line is one rendered reason string, so a two-half
entry can print twice.

The pattern does not catch a single lane letter used as an author ("G's reading"). Read for that by eye.
