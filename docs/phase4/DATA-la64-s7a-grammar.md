# DATA — the linux/arm64 S7-a seat: the two-RID linux roster grammar (P2, 2026-11-11)

> **A point-in-time record.** Seat S7-a of `DATA-la64-s7-bank-plan.md` (G; sections 2.4 and 5.3), assigned by COORD
> on 2026-11-11 (mailbox `cbb2213c80`). Cut on the batch-284 union `claude/coord-b284-wip` at `c4b47c4f61`. Branch
> `claude/p2-la64-s7a`. The plan, its six pieces and four questions with defaults were posted before any commit
> (`88f66d6b52`); COORD accepted it with the defaults standing (`f33164d51b`). G is the second reader.

## 1. What the seat delivers

The linux bank reads every row on linux-x64 AND linux-arm64. Before this seat the roster had one linux reading per row,
`annotate_linux.py` was single-RID, and no row could say it had not been read on linux-arm64. Now:

- A linux segment carries a per-RID note: `linux: 3840 + 2 (linux-x64; linux-arm64: 3838 + 2)`, or
  `linux: 3840 + 2 (linux-x64; linux-arm64: pending)` while linux-arm64 has no reading of record. Every one of the 236
  numeric linux segments now reads pending, and a numeric segment with no note fails the guard.
- Each segment is DERIVED from the two RIDs' evidence lines, never typed, by one rule set in the guard (section 2i of
  `src/check-roster-format.ps1`) and in `annotate_linux.py --rids`.
- The Linux header states one population per RID.
- An UNMEASURED ledger names each linux-arm64 row that does not bank, and why.

## 2. Commits (each fix follows a red commit that fails by assertion at its base)

| Red | Fix | Piece | Red at its base |
|:--|:--|:--|:--|
| `d24b820a57` | `67308ad2c2` | 1. The note admits linux's RIDs and `pending`; per-RID expectation and host RID on linux | 12 guard checks |
| `720b4376fb` | `9585dadf8f` | 3. The two-RID segment: equal, arch-strict, arch-count, the new combined form; host difference; unread | 40 guard checks |
| `c71316ec19` | `abf0406a46` | 4. `annotate_linux.py --rids`, rule for rule | 20 annotator cases |
| `e9e4be7c9c` | `408815b48e` | 2. The Linux header, one population per RID (the roster's header line edited) | 13 guard checks |
| `b96f6de46b` | `194d93ee42` | 1, follow-up. Only linux-arm64 is ever pending | 1 guard check |
| `6cb1b41a2c` | `783ac015c5` | 5. The UNMEASURED ledger, guard and annotator | 10 guard checks, 3 annotator cases |
| `809c16769e` | `a2a3c43024` | 6. The note is mandatory, and the mechanical commit writes it on 236 rows | 2 guard checks |

At the last commit the guard passes 2488 checks and the annotator self-test 39 cases.

## 3. The derived segment

`Get-LinuxPairReading` (guard) and `pair_reading` (annotator) read the row's two lines in its latest declared wave and
decide in this order:

| Form | Condition | Segment |
|:--|:--|:--|
| refused | not exactly one line per RID, or no numeric segment | none: the guard and the annotator name it |
| host difference | the same dispatch's linux-x64 line is not the roster's linux-x64 reading (status validated, `new` 0, equal matched and disclosed-divergent) | pending |
| equal | both validated, `agree` yes on both, equal verdicts, matched and disclosed-divergent | `N + d (linux-x64; linux-arm64: N + d)` |
| arch-strict | `Get-DarwinArchStrictReading`, unchanged (it was already RID-generic) | the RID carrying the disclosures headlines |
| arch-count | C1's `Get-ArchCountReading`, unchanged; a Refusal passes through | the longer RID headlines |
| combined | `Get-ArchCombinedReading`, new (below); a Refusal passes through | the longer RID headlines, each RID with its own disclosed share |
| pending | anything else | `<linux-x64 reading> (linux-x64; linux-arm64: pending)` |

**The combined form** (COORD's ruling of G's Q1, for runtime-shaped rows). Verdicts AND disclosed-divergent both differ.
Both lines validated, COUNT or DISC, `new` 0 on both, matched + divergent = verdicts on each. It banks only when both
deltas are named:

- the verdict delta: the shorter RID's `absent` names exactly (longer - shorter) distinct verdicts, and the longer
  RID names none (arch-count's rule);
- the disclosed delta: each RID's new `divergent_names` column (Q2 default) names exactly its divergent - new
  distinct verdicts, so the two lists name every disclosure that applies on one RID only;
- no verdict is both absent and disclosed on the shorter RID.

Anything less is a Refusal by name. Example: linux-x64 20 verdicts, 17 matched, 3 divergent; linux-arm64 17 verdicts,
15 matched, 2 divergent, with 3 absent names and 2 disclosed names gives `17 + 3 (linux-x64; linux-arm64: 15 + 2)`.

**The host difference** is not in the posted plan. It applies COORD's ruling on G's Q4 (ledger, 2026-11-10: "differing
x64 readings recorded not written"): the roster's linux-x64 reading is never rewritten from the arm leg's dispatch, and
a pair whose hosted linux-x64 line differs from it is not compared, so linux-arm64 stays pending and the reason is
listed.

**Refusals and the ledger.** The guard reports a Refusal on any row. On a pending row, a reasoned ledger line accepts it
(`Get-LinuxSegmentRefusal -Ledgered`); on a row that carries a reading, nothing does. The annotator exits non-zero,
naming every refused row and writing nothing, unless the row has a reasoned ledger line; then it writes the row pending
and lists it as UNMEASURED.

**Unread rows.** A row the declared evidence does not name may not carry a linux-arm64 reading
(`Get-LinuxUnreadRefusal`); the annotator writes it back to pending with its linux-x64 reading kept.

## 4. The header and the ledger

The Linux line now reads: "Linux: 236 of 236 applicable rows validated at their linux-x64 counts — 77,507 matching
verdicts · 404 disclosed · linux-arm64: 0 of 236 banked, 236 pending · 2 rows platform-exclusive". The linux-x64 sums
take each segment's linux-x64 reading, which is the note's where linux-arm64 headlines an arch-strict row. The
linux-arm64 clause counts rows that carry a linux-arm64 reading against the rest.

The ledger is `$LinuxArm64LedgerPath`, `docs/phase4/rebank-1.26.10.4/linux-arm64/UNMEASURED.tsv` (no file yet), with
columns row, reason, shard, run. Refused from the start: a wrong header, a blank reason, shard or run ('-' where never
dispatched), a line for a banked row, a line naming no applicable linux row, a row listed twice. Once
`$LinuxArm64BankClosed` is `$true` (S7-d flips it), a pending row with no line is refused too.

## 5. The naming control

At `abf0406a46`, the combined form's naming checks were disabled in each suite and then restored by checkout (`git
status --porcelain` empty afterwards):

- guard, `Get-ArchCombinedReading`'s Refusal forced to `$null`: 24 of 2454 checks red (the eight combined refusal
  fixtures, three checks each);
- annotator, `arch_combined` returning before its naming checks: 5 of 36 cases red (the five combined refusals).

## 6. Predictions and their scores

| Prediction (posted with the plan) | Reading | Score |
|:--|:--|:--|
| The mechanical commit touches 236 rows and only their linux segments, plus the header line | 236 lines changed (numstat 236/236); each differs from its old text by the note alone, checked by script; the 2 n/a rows untouched. The header line changed in piece 2, not piece 6: its numbers were already true before the notes existed | MET; the header moved one commit earlier |
| The guard's live read stays green at every commit | green at every fix commit. Each red commit fails by design, and piece 2's red also fails the live header checks, because its red is the header lacking the new wording and clause | MET at the fix commits |
| CNR untouched | no converter, golib or generator file changes | MET (nothing to run) |

The rule of piece 6 was measured before its edit: with the rule in place and the roster unedited, the guard read 236 of
2724 checks failed, one per row.

## 7. What later seats need

- **S7-b (evidence).** Mac-form TSVs with RID values `linux-x64` and `linux-arm64`, and the columns `status`, `agree`,
  `verdicts`, `matched`, `divergent`, `new`, `absent` and `divergent_names` (';'-separated, '-' for none). A row without
  `divergent_names` can bank equal, arch-strict or arch-count, never combined.
- **S7-d (the bank).** Declare `$LinuxRidEvidenceDirectory` and `$LinuxRidEvidenceWaves`, run `annotate_linux.py <tree>
  --rids --write`, write the ledger, and flip `$LinuxArm64BankClosed`.
- **Not changed.** Three scripts of the closed 1.26.10.2 re-bank (`legs_check.py`, `closing_checks.py`,
  `repass_cut.py`) read a bare linux segment. They are records and were not re-pointed; a new re-bank's tools read the
  noted form.

## 8. Amendment, 2026-11-11: G's second read, and the fixups seat

S7-a seated in the batch-284 union. G's second read found no defect that banks a wrong number, and COORD relayed three
follow-ups for before the first combined bank. Branch `claude/p2-la64-s7a-fixups`, cut on the union at `51c6a3aa4f`:
red `d91591a106` (guard 8 of 2504 checks, annotator 3 of 55 cases), fix `221bdaf7f9`.

1. **A combined pair where one RID discloses nothing.** Section 3's combined example above held, but its zero case did
   not: the guard read a `-` in `divergent_names` as a missing column (PowerShell unrolls an empty list to `$null` at
   the caller), so it refused what the annotator banked. It failed closed. Column presence is now read from the line.
2. **The host-difference clauses had no test.** Each of the four is now pinned alone in both suites. Removing any
   one reds 1 of 2504 guard checks, or 1 to 2 of 55 annotator cases.
3. **Smaller items.** A non-numeric count cell is refused, not a crash. Equal requires linux-arm64 `new` 0. A combined
   pair with no `absent` column is refused for that reason. A malformed per-RID note is refused by name: a separator,
   a key, a count and a parenthesis opening on a RID that the segment pattern did not read. Words after a count stay
   prose, as ruled before S7-a. One arm of section 2 changes with this: `darwin: 3 (osx-arm64; win-arm64: 4)` read as
   prose and is now refused. A duplicate RID line and the self-test's agree and longer-RID cases are now pinned. An
   empty ledger reason was already refused by the guard; the new fixtures show it through the reader.
