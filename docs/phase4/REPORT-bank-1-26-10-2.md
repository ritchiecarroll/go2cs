# REPORT — the 1.26.10.2 bank of record: the closing checks

> H, 2026-11-03. A dated record. Each closing check of
> [`PLAN-release-1-26-10-2.md`](../PLAN-release-1-26-10-2.md) *The closing checks* is stated as MEASURED at the bank
> tip, master **`d374c2b6f9`** (`d374c2b6f933ab49b0d78fc8a18ae51b57328559`, tree `3a13b80046a6fea276d7c7c09534fca04a8694e0`, batch 145). The bank of record landed as
> batch 144 (`56ba05d7ba`, H's seat `40e5f4ea85`, with the three windows evidence refs). The darwin ruled-count seat
> landed on top of it as batch 145 (G). Every reading in this report is of the re-pass at the freeze tip
> **`d9dd8f5499`** (`d9dd8f54994297f34be694b7a4594fc6ea826024`, batch 143).

**Result: every closing check holds at the bank tip.** The one list the checks ask for is the orphaned
disclosures. It names 3 entries that no row needs on any OS, **RETIRED in `e64424e8fa`**. 30 more are needed on no OS by name, but the
linux records count them without naming them, so they are stated as UNKNOWN with their counts, not cleared.

## The instrument

One script reads every record at one ref with `git show <ref>:<path>`, never from a working tree, so each reading
names its tree:

```
py -3 -I docs/phase4/data/rebank-1.26.10.2/closing_checks.py <repo> <ref>
```

- It writes nothing. It prints one line per check and exits 1 when any check fails.
- At `d374c2b6f9` it reads **0 check(s) failed**, rc 0.
- The darwin TSVs do not record a run's `headSha`, so the script reads each run's `head_sha` from GitHub's public
  REST API, unauthenticated and read-only. It takes the run ids from the darwin evidence commits' subjects.
  `--no-api` skips that one check and says so.

**The control** (safety floor 13). On a local branch that was never pushed, one record per check was broken:
- the `bytes` linux segment, `88 + 6` to `88 + 5`;
- the `cmp` darwin segment, removed;
- `container/ring`'s word in `i9-S.tsv`, PASS to FAIL;
- `H-S.ledger.tsv`'s tip line, zeroed;
- a fabricated `TestFakeOrphanControl` entry, added to `weak`'s manifest.

The script failed 5 checks, each naming its site, and listed the fabricated entry as ORPHANED ON ALL THREE. After
the branch was deleted, the script read byte-identical to before, and the worktree was clean.

## Linux

| Check | Measured |
|:--|:--|
| the `linux:`-annotated set equals the linux-eligible set by name | **236 = 236**. The eligible set is `eligible-linux.txt` ∩ the roster: the file lists the go1.26.10 population, and its 4 E-class rows outside the roster are unbanked and never dispatched |
| `n/a` stays on its 2 rows | `internal/syscall/windows` and `internal/syscall/windows/registry`, and no others; 236 + 2 = 238 |
| the residual set | **EMPTY**. Every numeric segment equals its row's 1.26.10.2 reading, computed as `annotate_linux.py` computes it: PASS, `verdicts` (when `matched` is a boolean) `+` (`disclosed` − host-fatal entries in scope on linux) |
| the one hand-applied segment | **`net`, `593 + 3`**, from P2's clean re-read under COORD's ruling (`linux/rereads/P2-net.tsv`, PASS 593, 3 disclosed, 0 diverged). Its pass reading, GWSL-1, reads DIVERGED and is not the reading of record. The re-read sits in `rereads/`, outside the annotator's directory (BOARD, 2026-11-03, lane H) |

The reading of record per row is P2-1, P2-2, GWSL-1 or GWSL-2, each row in exactly one of them. `net` takes its
re-read instead.

## Darwin

| Check | Measured |
|:--|:--|
| both RIDs agree, verdict for verdict | applied as the guard's §2h rule. Both RIDs read `validated`; `agree` reads yes on both, or the row holds a `$DarwinAgreementRulings` entry; matched is equal, and disclosed-divergent (`divergent − new`) is equal. Within a dispatch, a re-dispatch stem supersedes the original: `net`'s C1-15R supersedes C1-15, and no other row has two readings |
| the `darwin:`-annotated set | **229 of the 236 eligible**, and it equals, by name, the set of rows whose reading banks |
| every numeric segment equals its 1.26.10.2 reading on both RIDs | yes, all 229 |
| banked by the ruled-count table (batch 145) | `internal/cpu` **2** and `internal/runtime/gc/scan` **3092**: both RIDs read COUNT and agree, at exactly the `$DarwinRuledCounts` value (seat #8(a) `4f245d9f4b`; BOARD `d780a56fbc`; C1 `0915beab8f`) |
| an agreement ruling used | none this pass. `net` holds one, but its arm64 reads FAIL, and the ruling applies only where both RIDs read `validated` |

**The 7 eligible rows that do not bank.** None of them carries a `darwin:` segment. Each RID is shown as status,
matched / disclosed-divergent / new, then `agree`:

| Row | Dispatch | osx-arm64 | osx-x64 |
|:--|:--|:--|:--|
| `internal/synctest` | C1-8 | FAIL 56 / 0 / 3, no | FAIL 56 / 0 / 3, no |
| `internal/trace` | C1-8 | FAIL 186 / 0 / 1, no | FAIL 186 / 0 / 1, no |
| `net` | C1-15R | FAIL 550 / 3 / 2, no | validated 552 / 3 / 0, no |
| `runtime` | C1-13 | FAIL 11058 / 57 / 5, no | FAIL 11062 / 57 / 1, no |
| `runtime/pprof` | C1-11 | FAIL 144 / 7 / 4, no | FAIL 146 / 8 / 1, no |
| `syscall` | C1-12 | FAIL 16 / 0 / 17, yes | FAIL 16 / 0 / 17, yes |
| `testing` | C1-12 | FAIL 80 / 3 / 2, yes | FAIL 80 / 3 / 2, yes |

`net` read `darwin: 552 + 3` at 1.26.10.1. The bank removed that segment, with a dated comment on the row, and
did not carry it forward (BOARD, 2026-11-03, lane H, OPEN post-hop). The other six carried no segment at 1.26.10.1
either. `testing` was one of the three rows of the darwin harness seat (#8).

## Windows

§15.2's verdict, at the freeze tip `d9dd8f5499`. The shards are i9-S, H-S and G-S, under `rebank-1.26.10.2/sweep/`.

| Check | Measured |
|:--|:--|
| 238 rows, each exactly once | 238 lines over 141 + 95 + 2, with 238 distinct names, equal to the roster's set |
| every word PASS | 237 PASS at their banked counts, all rc 0, with no PASS?rc. `testing` reads **COUNT 82** (rc 1), the ruled TestTBHelper Release-only class (BOARD `82b7347b11`), which the bank applied as `82 + 3` |
| each count against the bank | all 238 equal the roster's windows matched count at the bank tip |
| zero real drift | `real_drift` 0 on all 238; no oracle re-run |
| per-row deletion censuses | all 0. That is i9 141 (`deleted_pre_restore`, and `porcelain_after` 0), H 95 (`del_census`) and G 2 (`deleted`) |
| shard-end deletion censuses | all 0: G-S in its ledger (`# SHARD END`, deleted 0 → 0); i9-S and H-S in their evidence commits (`5f4d92d053`, guard 3; `6b3fec77e5`, after the purge) |

## Per OS

| OS | read | not dispatched | each once | tree read by every run |
|:--|--:|--:|:--|:--|
| windows | 238 | 0 | yes | each ledger's tip line reads `d9dd8f5499`: i9 `tree_head`, H's line 1, G's `# HEAD` |
| linux | 236 | 0 | yes; `net` adds its one ruled re-read | the TSVs carry no `tree` column, so the record is the ledgers: all 237 lines (236 + `net`'s re-read) read `d9dd8f5499` in both `corpus_commit` and `converter_commit` |
| darwin | 236 | 0 | yes, one line per RID in each row's reading of record | all 16 runs' `headSha`, from the API, read `d9dd8f5499`: C1-1 … C1-15 and C1-15R, with run ids from the evidence commits |

Linux and darwin are 236 because their 2 `n/a` rows are not eligible. Windows is 238.

## All three platforms: the disclosures orphaned on every OS

**Computed** over every committed manifest (`go2cs_test_disclosures.json` under `src/core`, parsed as JSON),
against the three OSes' records at this ref. All 49 manifests sit under a roster row, which gives 363 entries. Per
OS, an entry is in one of three states:
- **NEEDED** when that OS's reading uses it;
- **FREE** when it is out of scope there, or when the reading uses no disclosure at all;
- **UNKNOWN** when the reading uses some disclosures and the record does not name which.

An entry is **orphaned on all three** when it is FREE on every OS.

- **Windows names its disclosures.** The row's own proof page (the row's first proof link) marks each disclosed
  verdict in its `## Verdicts` table. For every row with a manifest, the page's header equals the roster's windows
  cell at the bank tip, and the page marks exactly that many verdicts.
- **Linux and darwin record counts only.** U is `disclosed` − host-fatal entries in scope (linux), or
  `divergent − new` on each RID (darwin). M is the number of verdicts that the entries in scope, other than the
  host-fatal ones, can cover. It counts each subtest entry's parents too, because a disclosed subtest fails its
  parent and the converter discloses the parent with it. `crypto/internal/fips140test`'s page shows this: 7 entries
  and 8 disclosed verdicts, with `TestNISTECAllocations` among them. The rule:
  - U = 0 frees every entry in scope;
  - U = M needs every one;
  - anything else is UNKNOWN for that row.

  No reading exceeds its M.
- **A host-fatal entry in scope is NEEDED.** It withdraws its test before either side runs, so no run can read it
  passing, and the orphan predicate is a terminal pass (DESIGN-orphan-disclosure-check.md §3).

| Bucket | Entries |
|:--|--:|
| needed on some OS | **330**: 302 named on the windows page; 13 host-fatal in scope; 15 by a linux or darwin count of U = M |
| **orphaned on all three** | **3, RETIRED in `e64424e8fa`** |
| unknown | **30** |
| **total** | **363** |

**ORPHANED ON ALL THREE** (no row on any OS still needs them):

| Package | Entry | Class | windows | linux | darwin |
|:--|:--|:--|:--|:--|:--|
| `debug/gosym` | `TestSymVersion` | `platform-skip` | in scope, skip / skip on the page | U 0 (`linux: 10`, P2-2) | U 0 on both RIDs |
| `os/signal` | `TestSignalTrace` | `runtime-capability` | out of scope (`platforms: [darwin]`) | out of scope | U 0 on both RIDs (30 / 0, C1-11) |
| `weak` | `TestPointerEquality` | `codegen-liveness` | in scope, pass / pass on the page | U 0 (`linux: 9`) | U 0 on both RIDs |

`debug/gosym`'s entry is written for linux, where Go's test binary carries `.gosymtab`. Linux now reads the row at
`10` with nothing disclosed. `os/signal`'s entry is darwin-only, and darwin now reads the row clean on both RIDs.
`weak`'s entry fires on no OS.<!-- Until 2026-11-03 (COORD's ruling, the same day) this paragraph ended: "**This
report names them. It does not retire them**: a removal is a cross-platform manifest edit, and each needs its own
re-derivation (DESIGN-orphan-disclosure-check.md §5)." -->

**Amended 2026-11-03: the 3 are RETIRED in `e64424e8fa`** (COORD's ruling: nothing ships pinning a divergence no OS has).
Each was removed by span, with the parse proved equal to the original less that entry:
- `debug/gosym` and `weak` empty (1 → 0), so their manifests are deleted;
- `os/signal` goes 2 → 1, and `TestAllThreadsSyscallSignals` stays.

The retirement is **count-neutral on every OS**, measured before and after on the same tree:
- `check-roster-format.ps1` reads 2264 → 2263 checks. The one missing check is the per-entry `platforms` check of
  `TestSignalTrace`, the only one of the three with a `platforms` field. Its legacy-label arm reads 363 → 360
  entries. No header, cell or segment moves.
- `annotate_linux.py` and `annotate_darwin.py` print byte-identical output.
- `closing_checks.py` at `e64424e8fa` reads 47 manifests and 360 entries: 330 needed, **0 orphaned on all three**, and the
  30 UNKNOWN below. Every other check is as above.
- **Windows was measured at the retirement tree**: one `run-validated-sweep.ps1 -Filter <row> -Exact` per row, in a
  throwaway worktree, with H-S's shard guards. `debug/gosym` read PASS 10 and `weak` PASS 9, rc 0, at their banked
  counts, with no real drift and an empty deletion census.
- `debug.gosym.md` is that sweep's own rewrite. It drops the deleted manifest's note, "Linux only: … this entry never
  fires there".

**UNKNOWN: 30 entries in 2 rows, all linux-only in effect.** Each is FREE on windows and darwin (out of scope, or
`syscall`'s TestExecPtrace at darwin U 0), and is not needed on any OS. The linux record counts them without naming
them:

| Row | M on linux | U on linux | not disclosed on linux | Entries |
|:--|--:|--:|--:|:--|
| `crypto/x509` | 10 | 9 | 1 of the 10 | the 9 `TestSetFallbackRoots/*` subtests (`platform-skip`) |
| `syscall` | 22 | 17 | 5 of the 22 | 21 `platform-skip`, `cgo-configuration` and `host-limit` entries |

So at least one linux verdict in `crypto/x509` and five in `syscall` that these entries can cover are not
disclosed at the bank. Naming which needs a per-test linux record, and the linux leg commits counts only
(the only per-test files are `GWSL-1-runtime-LockRank.json` and `net`'s `rereads/P2-net-keys.txt` and
`P2-net-skips.txt`). An empty orphan list
here would have needed those names. They are UNKNOWN, not cleared.

## Limits, stated

- **Proof pages.** They are the windows record at their own dates. Of the 49 rows with a manifest, 46 pages read
  `Validated 2026-10-25`, at go1.26.10, and 3 read 2026-11-03 (`math/big`, `runtime` and `testing`). Each is
  checked count-equal to the bank at this ref, and that is all a count can check. An older page names the 1.26.10
  verdicts, and the 1.26.10.2 sweep matched them by count.
- **Darwin `headSha`.** It is read from the API at the time of the run, not from a committed record. Every
  evidence commit's subject also states "at the tip d9dd8f5499".
- **Two shard-end censuses** (i9-S and H-S) are stated in their evidence commits, not their ledgers.

<!-- Provenance (H, 2026-11-03, COORD's dispatch of the same day). Measured by closing_checks.py at d374c2b6f933ab49b0d78fc8a18ae51b57328559
     (tree 3a13b80046a6fea276d7c7c09534fca04a8694e0), which printed 0 check(s) failed with the API leg; the same script at 677cf7c341 (batch 144,
     before the ruled-count seat) read 227 darwin rows banked and the same 0 failed, with internal/cpu and
     internal/runtime/gc/scan among the unbanked as COUNT 2/0/0 and COUNT 3092/0/0 on both RIDs. Control: local
     branch tmp-h-control (68d2b2f6b1, deleted, never pushed), 5 checks failed, each naming its planted site, and
     the planted manifest entry listed as orphaned; restored output byte-identical apart from the labels changed in
     the same session. Darwin run ids: C1-1 41665882913, C1-2 41674095467, C1-3 41665786661, C1-4 41682823991, C1-5
     41683212518, C1-6 41685608101, C1-7 41686970694, C1-8 41688965804, C1-9 41693952116, C1-10 41693997921, C1-11
     41665833635, C1-12 41695688480, C1-13 41665696029, C1-14 41696366991, C1-15 41665739974, C1-15R 41701297335;
     every head_sha d9dd8f54994297f34be694b7a4594fc6ea826024. -->
