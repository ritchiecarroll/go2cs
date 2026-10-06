# TRAIN Q: the derive's report (step 2: the scripts)

2026-10-06, 14:40 to 15:55 Central, a workflow agent. Input: `trainP/` as committed on the handover branch, COORD's
run-time fragments under `coord-scratch/tP/`, COORD's notes L:257 to L:369, the i9's rehearsal as step 1 restated it
(`tQ-map.md`), and each Q row's diff read from git objects. Output: this folder, UNCOMMITTED. Every change has an id
in `tQ-CHANGES.md`.

## 1. What was done

| Area | Result |
|---|---|
| The copy (R0) | 28 of P's files copied under Q's names; P's five records and `controls/` not carried; four documents rewritten rather than renamed |
| Names and paths (A) | `W`, `BRANCH`, lock, scratch, emission roots, map refs and subjects are Q's by the rename; the previous battery of record is P's run2 through `P_RUN`; `BASE` / `MASTER` required, no sha in any script; survivors justified in `tQ-CHANGES.md` section 1 |
| The lessons (B) | 41 collected; 9 of the task's floor done (one of them half open); dispositions in `tQ-CHANGES.md` section 3 |
| The seats' effect on the instruments (C) | Q12 to Q19, with what each seat's diff says |
| The assembly (D) | `tQ-resolve.py` + `tQ-ruled.txt` on TRAIN K's mechanism; the map and the assembly share them; stack, slot and candidate refusals |
| The documents (E) | the checklist, two briefs, a short README |
| New scripts | `tQ-resolve.py`, `tQ-resolve-controls.sh`, `tQ-controls.sh`, `tQ-selfcheck.sh`, `tQ-reread.sh`, `tQ-land-prep.sh`, `tQ-land-final-reads.sh`; new data `tQ-ruled.txt` |

## 2. What changed in the battery's shape

New legs and gates: **TRH** (the MSTest harness guards the union adds), **PPS51 / PPS7** (the pack path guard's
self-test), **T2** (one row twice into one tree), **XS2** (x/sync a second time into its own root, inside MOD),
**PUBSYM** (0 symbol-file refusals across the run), the S rows' **HOSTWALL baselines**, PRE-D arms 8 to 10
(CNR-FRESH, the record predicate, the module tags), the pwsh start probe, and **EXIT 7** (green, with a reading owed
at the landing head). Dropped: the record pre-clear, UF's known class. Changed readers: `te_class` (N-PARTIAL),
`realmod` (EXTERNAL), `treport` (address pairs), `hostwall`, `MAPLINE`, `emitdrift.py` (bytes), `tQ-regen-apply.py`
rule 4.

## 3. What was MEASURED (no build, no conversion, no `go test`)

| Reading | Command or instrument | Result |
|---|---|---|
| The assembler's table check on the REAL 61-row table (a scratch copy with the fetch removed and its scratch redirected; `TABLE_ONLY=1 SIGN_PROBE=0`, BASE 7098b8d3f9) | `derive/asmctl/table-real.log` | rc 2, exactly TWO problems: the two CAND rows by name; 18 `stacked on row` lines, 2 `ordered after row` lines, 2 MULTI-BASE notes; every remote tip at its seated sha |
| The resolver, eight arms | `tQ-resolve-controls.sh` | failed=0. Synthetic: ruled resolves (10 lines = 5 + 3 + 2, guard last); no ruling rc 3 naming row, path and `hunk 1: ours 3 / base 0 / theirs 2`; another sha rc 3; both sides editing one line rc 3; a ruled path beside an unruled one rc 3 with NOTHING written. REAL blobs: `TranspileTests.cs` (base 446d2c8ba0, ours 8d4ab2d7fd, theirs 0c6a2618fa) 1 hunk ours 6 / theirs 3, 796 `[TestMethod]` for 796 `Check`, worktree CRLF and index LF; the BOARD (ours c585fa199d, theirs 3196a93cdc) 1 hunk ours 139 / theirs 35, 26355 lines, guard final. The trap: `merge-file --union` gives 795 attribute lines for 796 methods |
| Every changed reader, ten arms | `tQ-controls.sh` | failed=0: CB five of five; EXT four planted arms; EXT-real on P's run2 records (`pass=8 fail=0 external=1`, where P's reader reads `fail=1`); TC (clean 0, planted 1, no tree UNREAD, path conversion off UNREAD while the old form reads 0); RG five arms; ED (a file changed under an old time is drift); TE-carrier (78 files, 195 hunks, 195 N-PARTIAL, 0 OTHER); TE-M (M's two controls unchanged in what they count); HW; RES |
| The script set, statically | `tQ-selfcheck.sh` | failed=0: plants fire; no `git -C` under path-conversion-off outside the named exemptions; no apostrophe in a `${VAR:?}`; every pwsh start in the battery's form; no profile path, account or host name; 0 CR bytes; `bash -n` on every `.sh`, a parse of every `.py` |
| `python -m py_compile` (the `.pyc` written to scratch) | 6 of 6 | ok |
| The tag derivation | `tagset` on two blobs | the base: stdlib `purego,math_big_pure_go`, module none; g-module-safe-tag's tip: module `safe` |
| Build constraints in the two real modules | grep over P's kept copies | x/sync 13 Go files, x/mod 39: 0 name safe / purego / math_big_pure_go / appengine |
| The no-inline prefix in committed test sources at the base | `git grep` | 938 lines in 180 files; 738 on a method declaration (init excluded) in 173 files; net/rpc 6 (CORRECTED at review round 1: the first figure, 14, came from a pathspec whose `*` crossed into net/rpc/jsonrpc and counted its 8), log/slog 3, encoding/json 1, runtime/debug 3, testing 4, sort 0 |
| The carrier's corpus line form | `git diff -U0` base..e7fcff2244 | every pair is prefix-removed, ` partial ` inserted |
| GoPositionMap spellings at the base | `git grep` | 3526 `go.`, 413 `global::go.` |
| Tests the rows add; skips they add | each row's own diff | 44 `+func Test` names; 2 `t.Skip` lines, both under `testing.Short()` |
| Package directories the rows add | each row's own diff | 28 (23 projects); 0 removed; behavioral tree unchanged between P's union d843ff263d and the base |
| Rows by area (own diff) | 61 rows | 31 touch `src/go2cs` (30 a non-test Go source), 8 `src/gen`, 4 golib `.cs`, 6 any golib file; 0 touch `Goroutine.cs`; 1 the roster (counts and linux annotations unmoved); 1 the runner (comments); 0 the CNR script |
| The three hand-written csproj of i9-pack-symbols | `git show` at c0c876fb27 | LangVersion lines 1 (a plain 14), 0, 0: each a hard FAIL under P's S1 census |
| P's run2 baselines | its SUMMARY | CNR 835, FX 1291, roster 1994 checks, IDC 217, GN 93, TR 26, PUB 6 of 6, 98 HOSTWALL stamps |
| Which list holds which row | the two lists | i7: reflect, internal/abi, runtime/debug, net/rpc, os/exec; i9: testing, sort, log/slog, encoding/json, encoding/gob, unicode/utf16 |
| trainP untouched | a hash of its 36 files' hashes | `cc4e1d9690219eaf` before the copy and after the last control; no `__pycache__`; nothing staged in hnd |
| The table untouched | rows-sha256 | `96ddd479f81a`, 61 rows, as step 1 left it |

## 4. NOT done, and stated

- **No script was run for real.** The map, the assembly, the fixup, the battery, the lane drivers and the three
  landing scripts are syntax-checked and their readers controlled; none met a worktree. The first real run of the
  map is the first reading of the table's ORDER (the rehearsal merged six seats elsewhere).
- **The resolver never met a real stopped merge of the union** (no worktree may be created here): its real arms use
  the real blobs in throwaway repositories. A merge of the union may put other hunks in a ruled file than the two
  measured; the resolver then refuses and names them.
- **Legs T2, TRH, PPS, XS2, S2 and the linux `twopass` are unrun.** Their expectations are P's measured readings
  (71 or 73 `.pdb`, Validated 8) and the seats' own; the publish folder path `bin/tests/publish` is P's.
- **Step 4t's prediction can be off by a method** the regex cannot class (a bodyless declaration keeps the
  attribute); a difference is a reading, read from the kept patch.
- **Rule 4's ladder (c) fails closed** on a flat path one target moved; nothing provides the output list yet (Q-C).
- **The premap set is not adapted** (`tQ-CHANGES.md` section 6): its comments still describe P's pre-map.
- **The battery's mid-run `git status | grep` restores are P's** (about twenty sites): guarded, not rewritten.
- **`tQ-README.md` is short by decision**, not P's manual re-derived.
- **Two files in this folder are not this step's**: `tQ-seats-gen.sh` (byte-equal to COORD's scratch generator) and
  `tQ-seats-v0-coord.txt` (62 rows) appeared at 15:48 while the derive ran. They were not edited. The self-check
  passes with them (22 `.sh`); the checklist's staged count is computed at run time and reads 42 with them.
- **L:369 arrived at 15:00, during the derive**: G's 633b045e8a is accepted (its row is owed `READING IN`), C1's
  trim default becomes a new tip, aot-smoke is its own non-gating dispatch. The checklist (0 and 5b) and Q-H carry it;
  the table was not edited.

## 5. Questions for COORD

`tQ-CHANGES.md` section 9, Q-A to Q-L. The four that block the map: Q-G (the sixth BOARD hunk), Q-H (the CAND
rows), Q-B (S1's amendment; it blocks precheck, not the map), and the acceptance words the table's header lists.

## 6. REVIEW ROUND 1 (2026-10-06, 16:34 to the time stamped in `tQ-CHANGES.md`): a dated block, the text above stands as the derive wrote it

Two reviews read the table and the scripts and returned 7 + 14 findings (4 blockers). All were applied but the ones
`tQ-CHANGES.md`'s REVIEW ROUND 1 section names as questions; that section is the record: every change, the controls
that ran with their results, and the open items. What moved in THIS report's own statements:

- Section 3, the table: no longer 61 rows at `96ddd479f81a`. It is 62 rows (c1-aot-smoke added; the census-limit row
  moved below its base row; the plan branch at its new tip): the hash is on the table's line 1.
- Section 3, the controls: the suite has 16 arms, not ten, counts an arm that did not run, and every arm was made to
  fail once on a scratch copy.
- Section 3, net/rpc: 6, not 14 (corrected in place above).
- Section 4, 'the landing scripts are syntax-checked and their readers controlled': their DECISIONS are controlled
  now too (the run-folder tie and the owed-file gate are one extracted function, run on planted folders and on P's
  real run2 files).
- Section 4, the legs that are unrun: PUB2 and PUB2c join them (new at review round 1).
- Section 5: Q-G is answered in the draft (the row is moved); Q-H is replaced by the table's QUESTION 7 (c1-aot-smoke
  does not merge as cut) and QUESTION 8 (the windows Native AOT failure).
- The base: master moved twice during the round (7098b8d3f9, then b6e4856883 at 16:49, then 0457242046 at 16:55).
