# DATA — the .3 `-tests` regen: every roster row's test emission re-converted at master, each changed file attributed

> **State: MEASURED (2026-11-08), win-x64, R-LAPTOP, branch `claude/r-dot3-tests-regen` from master `dab0675bd9` (batch
> 246).** COORD's 03:25Z ruling: the `-tests` half of the .3 regen, which the end regen
> ([`DATA-dot3-endregen.md`](DATA-dot3-endregen.md) section 4) left out. Evidence in
> [`data/dot3-tests-regen/`](data/dot3-tests-regen/).

## 1. Answer

**1,091 test files move and 0 production files move.** By kind: 846 `*_test.cs`, 245 test `package_info` files
(`package_test_info.cs` and `package_info_internal_test.cs`), 0 `.tests.csproj`, 0 production `.cs`. Every changed file is
explained by landed mechanisms (section 4). The classifier finds no hunk that belongs to none of them. One mechanism,
batch 204's init fold, drops a position record it should keep: 2 sites here and 1 in production. That is a finding
(section 5). This regen carries it as emitted and does not fix it.

## 2. Method

- **Converter:** `src/go2cs/bin/go2cs.exe` built from the worktree at `dab0675bd9` (go1.26.10, GOROOT
  `%USERPROFILE%\sdk\go1.26.10` as `go env GOROOT` prints it); sha256 prefix `4d5aca1a6fc209ea`.
- **Rows:** all 238 roster rows, read through `Get-ValidatedRosterRows` in `src/_roster.ps1` (never a hand list).
- **Conversion:** per row, `go2cs -tests -test-action convert -go2cspath <tree>/src <GOROOT>/src/<pkg>
  <tree>/src/core/<pkg>`. It ran serial and in place, one conversion on the box, and ran no tests. rc 0 on 238 / 238,
  03:31:06Z to 04:08:46Z
  ([`testsregen-run1.log`](data/dot3-tests-regen/testsregen-run1.log)).
- **The tree is the seed.** The end regen proved production a fixed point at `a0fba9b08a`, and master moved since only
  by golib (246), so the committed tree is the converter's own production output. The `-tests` convert rewrites
  production files too, and none of them changed (section 3).
- **`.cs.auto`:** the run rewrote two test review siblings,
  `internal/syscall/windows/exec_windows_test.cs.auto` (+1/−1) and `.../registry/registry_test.cs.auto` (+22/−24). A
  regen does not refresh siblings, so both were restored by file list and are not in the commit.

## 3. Readings

| kind | files | lines (`--ignore-cr-at-eol`) |
|:--|--:|:--|
| `*_test.cs` | 846 | +23,926 / −34,175 |
| test `package_info` (`package_test_info.cs`, `package_info_internal_test.cs`) | 245 | +784 / −783 |
| `.tests.csproj` | 0 | — |
| production (non-test) `.cs` | 0 | — |
| **total** | **1,091** | **+24,710 / −34,958** |

Raw: [`tests-numstat.txt`](data/dot3-tests-regen/tests-numstat.txt), taken before the restore: 1,093 rows, the 1,091 above
plus the two `.cs.auto` siblings of section 2. The production zero is the file list itself: every
one of the 1,091 paths has a test-kind name, and `git status` held no other row before the commit.

## 4. Attribution

The method is [`classify_tests4.py`](data/dot3-tests-regen/classify_tests4.py). It compares HEAD against the regen per
WHOLE file, with CRs stripped, which avoids the cut artifacts of a hunk-level diff. Each named mechanism contributes
two things:

- a counter, read on both sides; the delta is the mechanism's footprint in that file;
- a rewrite, applied to both sides.

A file is explained when its rewritten token streams are equal. The comparison drops whitespace, braces and
parentheses, which is a stated loss: the K, 204 and B3 shapes move only those. Two more passes help:

- metadata records are compared with their numbers normalized;
- what remains is matched chunk by chunk against the folded forms of batch 204.

**Result: 1,091 / 1,091 files explained.** 1,089 match outright. The 2 others match except for the dropped records in
section 5, which are the finding.

| mechanism (batch) | files | net instances (new − old) |
|:--|--:|--:|
| B3 (231): a slice literal is written bare, `[..]`, not `slice<T>([..])` | 436 | −7,653 |
| B3 (231): `foreach` over a collection reads `range(x)` | 653 | +3,320 |
| B3 (231): an index-only `foreach` reads `range(len(x))` (`builtin.len` where `len` is shadowed) | 177 | +446 |
| B3 (231): a `range<T>` element type gains its `global using` alias (`go/parser`) | 1 | +1 |
| 204: an if/switch init statement loses its scope block | 541 | −3,401 |
| 204: an init folded into the condition (`out _`, `out var v`, `is (var v, true)`, `is var` in an else-if) | 19 | +40 |
| 222 (K): blank lines Go does not have are dropped | 161 | −1,205 |
| 222 (K): a short function collapses to a one-line body | 107 | +641 |
| 166: `using MethodImplAttribute/MethodImplOptions` dropped where no `[MethodImpl]` is written | 196 | −392 |
| 189: a value method through a pointer reads `p.M()`, not `(~p).M()` | 10 | −64 |
| 217: a position map gains its deferred-closure field | 82 | +167 records |
| metadata re-encoded: position maps, per-call and literal tables, `GoBoundedGoroutine` line numbers | 242 | (follows the layout) |

Files usually carry several labels at once: 109 distinct combinations, listed in
[`tests-classified4.txt`](data/dot3-tests-regen/tests-classified4.txt). The metadata row is a consequence, not a cause.
B3, 204 and K move the C# lines, so the records that index them re-encode, which is the expected line kind for a
removal. Every one of these batches landed after batch 143, the last `-tests` regen at `f8271bd73d`, and each applied
only its production footprint at landing.

**Master since:** `dab0675bd9` → `663561b7a3` (247, 248) changes golib, GolibTests and docs only, no converter or
generator source. The reading therefore holds at the current master.

## 5. Finding: batch 204's init fold drops a per-call position record

The census is [`calls_census.py`](data/dot3-tests-regen/calls_census.py). It compares the per-call table (the
`GoPositionMap` sixth field) by payload, with the C# line key stripped, so a layout move re-keys an entry but only a
payload that disappears counts.

| range | payloads lost | gained |
|:--|--:|--:|
| this regen (HEAD → regen) | 2 | 0 |
| batch 204's own landing (`642621b43f^1` → `642621b43f`, production) | 1 | 0 |

The three sites ([`calls-tests.txt`](data/dot3-tests-regen/calls-tests.txt),
[`calls-204.txt`](data/dot3-tests-regen/calls-204.txt)):

- `crypto/internal/fips140test/entropy_test.go:84-86`: `if err := testenv.Command(t, ... ).Run(); err != nil {`, where
  `Run` sits on Go line 86 and its entry `Run/1/1/2` is lost;
- `net/http/serve_test.go:6467-6468`: `if _, err := fmt.Fprintf(cn, ..., strings.Repeat("x", size)); err != nil {`,
  where `Repeat` sits on Go line 6468 and its entry `Repeat/1/1/1` is lost;
- production `internal/coverage/cformat/format.go`, the same shape: `perc(...)` inside a two-line `fmt.Fprintf` init,
  entry `Invoke/1/1/1` lost.

All three are one shape. An if init statement spans two Go lines and calls something on its later line. The init
moves onto the `if`'s own C# line (204's shape (a), `init; if (cond) {`). Its later-line call then loses its per-call
entry. `encodePositionTable` already skips a second statement on a shared line ("an init clause; the first already
recorded it"), and the per-call payload appears to be lost the same way.

**Consequence:** a frame inside that call reports the statement's first Go line (84, 6467) where Go reports the call's
own line (86, 6468). That breaks the route-2 per-call ruling. The defect is unrooted in code and was not fixed here,
because that is out of brief. It is routed to batch 204's seat (H).

## 6. Fixed point

A second full pass ran the same 238 rows over the committed regen (`1d164c6eb4`), with the same binary: rc 0 on 238 /
238, 04:52:36Z to 05:26:16Z ([`testsregen-run2.log`](data/dot3-tests-regen/testsregen-run2.log)). Afterwards `git status`
named only the same two `.cs.auto` siblings as the first pass, and both were restored by file list. **No other file
moved, so the committed test emission is a fixed point of the converter.**

## 7. Docs the regen moved

Eleven docs code samples and one provenance note cite lines in test files that this regen moved. Batch 247 re-cited
its notes the same way, so these are fixed in this batch.

- **Samples:** `docsamples -base dab0675bd9 -target HEAD -goroot <GOROOT> -recite -write` rewrote 11 samples on 7
  pages, refused 0 and raised 0 errors; 975 samples were unchanged. Nine of the 11 are re-cited line numbers only.
  The other two show the new emission, a `range(...)` and the path test's bare `[ … ]` table with its init statement
  out of its block.
- **Two pages hand-edited** where the new sample would have contradicted its prose:
  - `ConversionStrategies.md`'s TestExt walkthrough: its slice, `range` and init-clause bullets now describe the
    emitted shape.
  - `shadowing.md`'s alias-directive sample: batch 166 left `go/types/stdlib_test.cs` with no `[MethodImpl]` and so
    no alias, so the sample now cites `go/types/api_test.cs:19, 21-22`, which still shows both directives.
- **Provenance note:** `ConversionStrategies.md`'s `codec_test.cs:1206#1265b9` is re-cited at `:1197`. The
  fingerprint is unchanged, and the guard read the same line there.
- **Gates:** `go test ./internal/repoguard/` passes 113 / 113 at the branch. Before the docs fix it failed 9; three of
  those were these docs guards and their planted twins. The other six were native-gate tests reading files the
  concurrent second pass was rewriting, and they pass on a quiet tree.
