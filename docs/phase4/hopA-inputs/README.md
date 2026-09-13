# hop-A inputs — the recon's raw data, BANKED (was: stranded in a session scratchpad)

The corpus-hop audit found the inputs behind
[`RECON-go12312-diff.md`](../RECON-go12312-diff.md) living only in the recon session's scratchpad
("Raw TSVs beside this file for re-derivation" — beside the *report*, but the report moved to the
repo and the TSVs did not). A hop input that exists only in a session scratchpad re-derives from
zero if that session dies. These are the same five files, **re-derived independently on a second
machine and verified against every count the recon itself published** — which makes this bank
stronger than a copy would have been: a copy proves the files were saved; a re-derivation proves
the recon's numbers are reproducible.

## The files

| file | rows | what |
|:--|--:|:--|
| `commits.tsv` | 83 | `<sha>\t<date>\t<subject>` for every commit in go1.23.1..go1.23.12 |
| `files-by-commit.tsv` | 215 | `<sha>\t<file>` — per-commit changed-file list (the compare API caps its files array; per-commit enumeration is ground truth, same rule the recon used) |
| `files-unique.txt` | 161 | sorted unique changed files, repo-wide |
| `src-files-classified.tsv` | 150 | `<class>\t<file>` for the `src/` subset; classes `cmd` / `runtime-tree` / `stdlib` |
| `roster.txt` | 162 | the validated-package roster as read from `docs/ValidatedTestPackages.md` at this bank's commit |

**And the rehearsal's own raws, banked 2026-08-24** — [`../REHEARSAL-go12312.md`](../REHEARSAL-go12312.md)
promised these "beside this file" and they were in a session scratchpad, one purge from gone. They are
copied here **verbatim**, not re-derived:

| file | rows | what |
|:--|--:|:--|
| `census-raw.txt` | 152 | the full stdout of `migrate-gorelease.ps1 -To 1.23.12` in census mode — the run behind the rehearsal's §1 arithmetic (20 sites / 8 files, UNCLASSIFIED: none) |
| `h3-files.tsv` | 160 | `<status>\t<file>` for the compare endpoint's file list — the rehearsal's independent H3 derivation, a strict subset of `files-unique.txt` by exactly one `cmd/` test file |
| `h3-compare-head.json` | 1 | the compare endpoint's headline object, `{"commits":83,"files":160,"status":"ahead"}` — the truncation the recon predicted, preserved as the evidence for it |

## Derivation (reproducible from a clean machine)

```bash
git clone --bare --shallow-exclude=go1.23.1 --branch go1.23.12 https://github.com/golang/go gogit
git -C gogit fetch --deepen=1 origin   # REQUIRED: the shallow boundary commit is grafted, and a
                                       # grafted commit's --name-only lists its ENTIRE TREE
                                       # (13,261 phantom files); one deepen restores its parent
git -C gogit log --format='%H%x09%ad%x09%s' --date=short go1.23.12            # commits.tsv
git -C gogit log --format='COMMIT%x09%H' --name-only -83 go1.23.12            # files-by-commit
```

Classification rule (matches the recon's headline arithmetic): `cmd` = `src/cmd/**`;
`runtime-tree` = `src/runtime*` **including `runtime/debug`** (the recon's package-bucketed table
lists `runtime/debug` under stdlib-visible, but its headline's 42/59 split buckets it with the
runtime tree — both conventions differ by exactly that one file, `runtime/debug/mod.go`);
`stdlib` = the rest.

## Verification against the recon's published counts — all exact

| reading | recon | this bank |
|:--|--:|--:|
| commits in range | 83 | **83** |
| unique changed files | 161 | **161** |
| under `src/` | 150 | **150** |
| outside `src/` | 11 | **11** |
| `src/cmd/**` | 49 | **49** |
| runtime tree | 42 | **42** |
| stdlib-visible remainder | ~59 | **59** |
| roster rows | 162 | **162** |

## ⚠ The shard map — still unbanked, and its reserved-set gap is WIDER than the audit recorded

> **CLOSED, both halves, 2026-08-24.** The draft and its generator banked into this directory
> verbatim ([`shard-map-draft.md`](shard-map-draft.md), [`shardmap.py`](shardmap.py), `e0d8930e1`) —
> as found, so the *next* commit's subject was visible rather than smuggled — and the recommendation
> at the end of this section then **landed**: `shardmap.py` derives its reserved set from
> `$longTimeouts` at generation time (`549b4e556`, lines 71–88), so the copied list is gone rather
> than corrected. `PLAN-hop-campaign.md` §4.3's static table is marked SUPERSEDED BY GENERATOR, and
> the derive-never-copy rule is generalized into `GoCorpusMigration.md` §3.2. The paragraph below is
> kept as written, because it is the reasoning that produced the fix.

The 28 KB shard-map draft and its generator (`shardmap.py`) remain on the coordinator machine's
session scratchpad — not reachable from this lane, so not banked here; that half of the audit item
stays open until the coordinator pushes them (or the generator is rewritten against banked data).

What this lane could verify: the audit said the map's reserved set is missing **`go/parser` (90m)**
and **`crypto/internal/mlkem768` (30m)**. Measured against the LIVE `$longTimeouts` table
(`src/run-validated-sweep.ps1:495`) the gap is now **three** — `crypto/tls (30m)` joined after the
audit — and two floors moved under it (`crypto/dsa` 60m→120m, `archive/zip` 30m→60m). The table has
grown twice since CLAUDE.md described it as four rows. **Recommendation for the map's banking:
the generator should DERIVE its reserved set from `$longTimeouts` at generation time, never carry a
copied list** — a copied list has already drifted twice in the map's short life, which is the same
hoist-vs-derive defect this campaign fixed in `_paths.ps1` the same week.

## The recon leg's per-row cost data, banked 2026-09-13

The hop shard map is parameterized by per-row wall times. [`DATA-recon-pass1-2026-09-13.md`](../DATA-recon-pass1-2026-09-13.md)
re-measures the whole roster at the campaign's own corpus, TWICE, in both dispatch modes. These are the
tables it was derived from, banked here for the same reason as everything above: the record quotes
figures, and a re-derivation needs the rows.

| file | rows | what |
|:--|--:|:--|
| `recon-pass1-20260913T123456Z-reparsed.tsv` | 205 | pass 1, ISOLATED (per-row `-Filter <row> -Exact`). `row / word / verdicts / banked / sweep_s / exit / last_test_on_failure`. 205 lines for 204 unique names: `archive/tar` appears twice (control run and roster run). RE-PARSED from the retained per-row logs with the sweep's complete row-word vocabulary; the as-run TSV is superseded and is NOT banked. |
| `recon-pass2-20260913T144128Z.tsv` | 204 | pass 2, IN-SWEEP (one full-roster run). `row / word / verdicts / banked / sweep_s`. |

**Reading rules, and they are not optional.** Use `sweep_s` — the sweep's own per-row figure — and never
a shell wall, which is larger by process start-up and is not the quantity the older record holds. **Drop
`net` from any fit**: its figures in both passes (678 s and 3,107 s) are lower bounds produced by a human
stopping the row, not costs, and the record says exactly how. `crypto/rsa` is the only row whose VERDICT
differs between the passes, so its `sweep_s` is not comparable across them either — and its in-sweep
failure is a generator crash on a project in its build closure, not a property of the mode.

**Both passes ran at tree `a02ac3df3` on the same box with the same converter binary**, so the dispatch
mode is the only variable between the two files. That is what makes them a one-axis pair rather than two
measurements.
