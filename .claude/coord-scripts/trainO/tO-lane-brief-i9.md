# TRAIN O: the i9's complement shard

From COORD. DERIVED 2026-10-03 from TRAIN N's brief (`trainN/tN-lane-brief-i9.md`; you read N's union with it,
132/132, 0 movers, 12:12 to 13:10). The i9 sweeps its share of the banked roster at the TRAIN O union, one row at a
time, in its own clone. **Readings only**: nothing to cut, commit or push. Start when COORD's GO message names the SHAs.

| | |
|---|---|
| Union ref | `claude/coord-trainO-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN O fixup commit, or a `fixup-N: TRAIN O` on top of it) |
| Base | `<BASE: 40 hex, filled in by COORD>` = TRAIN N's LANDED master (N's union 59ee0d21bf + its fixup-2 + its bank step + its MS13 refresh). The union is that base + one signed merge per row of the seat list (34 rows drafted 2026-10-03; the driver reads the rows, never a count) + any ruled follow-up merge + the fixup. **The driver REQUIRES `BASE=` and refuses a base that is not on the union's first-parent line.** |
| Shard list | `.claude/coord-scripts/trainO/tO-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to TRAIN N's (and M's, L's and K's), sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainO/tO-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |
| Seat list | `.claude/coord-scripts/trainO/tO-seats-draft.txt` (same commit). The driver asserts every row is an ancestor of the union. |
| Helper | `.claude/coord-scripts/trainO/tO-helpers.py` (same commit). The driver calls its `execrows` reader (python 3.8 or newer, read-only). It refuses to start without the file. |

## What the shard is

The complement of the i7's list over the **225** banked rows (`docs/ValidatedTestPackages.md` at the union). No O row
adds or removes a roster row (the two C1 rows annotate 22 rows for **darwin**, which no windows sweep reads), so the
split is N's: 93 rows on the i7 (with `runtime` and `runtime/pprof` as direct `-tests` legs there), 132 here. No row of
yours is read on the i7 in this train beyond the canaries COORD names in the GO (`log/slog` was N's two-box row; at O it
is an ordinary row of yours, at the default).

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` all stay on the i7. Never add one. The driver refuses, by row name, any list row matching `crypto/tls`,
`crypto/x509`, `net` or `net/*`. (The owner's 2026-10-02 crypto/tls limits experiment is separate: no leg depends on it.)

## 1. Setup (N's, with the O names)

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for N's shard:
   `git fetch origin claude/coord-trainO-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO, and `git -C <path> cat-file -e <BASE>^{commit}`.
2. A **run folder outside the worktree** (floor 4). The driver, the list, the seat list and the helper, as blob bytes,
   all four from ONE fetch:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainO/tO-i9-shard.sh    > <run>/tO-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainO/tO-i9-shard.txt   > <run>/tO-i9-shard.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainO/tO-seats-draft.txt > <run>/tO-seats-draft.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainO/tO-helpers.py     > <run>/tO-helpers.py
   ```
3. go1.24.13 with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (floor 6); a .NET 10 SDK that resolves
   under `global.json`. **The union's converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0`** (`p1-hashset-module`,
   an O row). If your box builds with the proxy off, warm the cache first, in the worktree at the union (go.sum is
   complete, so nothing tracked changes): `(cd <path>/src/go2cs && go mod download)`; then
   `git -C <path> status --porcelain` must still print nothing.
4. Concurrency as at N: MSBuild `-m:4`, go `-p 4`. Heavy work is allowed. WHEA corrected errors are counted per row and
   are not a stop. Two reboots inside 24 h means light-only work and a line to COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> BASE=<BASE from the GO> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=<from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tO-i9-shard.sh
   ```
   While it runs the worktree is FROZEN (floor 4).

## 2. What the driver does (N's driver; the O changes)

- **Asserts.** It requires `BASE` on the union's first-parent line and EVERY row of `tO-seats-draft.txt` as an
  ancestor, a `fixup: TRAIN O` commit, and 0 csproj still at `<LangVersion>latest</LangVersion>`.
- **Execution configs.** The PRE line `execution-config rows` shows, read from the roster's annotation field: **NONE at
  the base and NONE at the union** (N left no annotated row; no O row adds one). The reader's control moved (O): it reads
  the roster at the parent of the NEWEST commit that changes the count of `execution: release-tiered` in the base's
  roster AND whose parent annotates `log/slog` (derived with `git log -S`, walking back at most 12 such commits:
  `g-slog-roster-tc0`'s roster commit 2d46eba8f0 unless N's landing changed the phrase's count; a walk past the first
  commit is stamped); the PRE line states that ref and its list. If the union's list shows ANY row the driver STOPS itself (it compares with the ruled
  set, `tO-helpers.py execruled`, empty): post the PRE line and the ABORT line; do not sweep by hand.
- **Named readings** from each row's FRESH comparison record: `log/slog` (`TestSetDefault`, `TestPanics`,
  `TestCallDepth`; no ENV verdict at O), `internal/godebug` (`TestCmdBisect`), `internal/synctest` (`TestDeadlockRoot`,
  `TestDeadlockChild`), `os` (`TestRemoveAllWithExecutedProcess`).
- **The refresh patch** (as at N). At the end, beside the TE patch (`tO-i9-logs/tracked-changes-U0.patch`), the driver
  writes the SAME rewrites with context, binary-safe: **`tO-i9-logs/tracked-changes.patch`**, and stamps its line count
  and sha256 on the HS line. It is the i9's input to COORD's MS13 refresh of committed `-tests` sources at O (a bank
  step after the battery). **Post its path and its sha256**, and keep the file until COORD says the refresh is committed.
- **Unchanged from N:** the row-loop count assert and exit 4 on a failed row; the record deletion before each row; the
  infra rerun rule; `GO2CS_MODULE_ROOT` unset; the per-row deadlock-line count; HS and UF; every `SOFT:` line.

**Do not pass these switches:** `-Hop`, `-TestConfig` / `-TestTiered`, `-SkipBuild`, `-IgnoreDiskPreflight`,
`-PublishBinlog`. The sweep takes each row's execution config from the roster.

## 3. What to expect

**132 of 132 at their banked counts, no mover.** You read 132/132 at TRAIN N's union.

What O adds on top (by the seat lines; NOT MEASURED on your rows by anyone):
- **reflect** (hand-owned `reflect/value_impl.cs`): `r-reflect-value-equality` (a `reflect.Value` compares by its
  datum's identity, as Go compares `ptr`; DISCLOSED: `ValueOf(5)==ValueOf(5)` is false here), `r-reflect-typed-nil-store`
  (a nil pointer or func stored by reflect keeps its type), `i9-reflect-nil-map-key` (yours: nil and typed-nil map keys),
  `g-method-value-names`. Watch every `reflect`-heavy row; a moved verdict that names `Value` equality, `MapIndex`,
  `SetMapIndex` or a nil interface is theirs to read first.
- **frame names** (hand-owned `runtime/managed_impl.cs`): `g-method-value-names` and `g-method-value-fm-record` name a
  method value `<entry>-fm` as Go does, a method expression by its receiver type, and `main.main`; the `-fm` frame is
  hidden from `Callers` expansion. Any row asserting on a function name through a method value can move.
- **golib `UntypedInt`** (`g-float-untyped-const-compare`): a float beside an untyped integer constant now uses the float
  operator. math / strconv rows are the ones that compare such values.
- **corpus**: `p2-converter-warning-clears` changes `context.cs`, encoding/json's `decode.cs`, net/http/httptest and
  `runtime1.cs` (an `AreEqual` defect: a silent wrong answer before), and writes per-file `.editorconfig` warning entries
  (compiler warnings only; no verdict should move from them).
- **-tests conversion itself**: `c1-tests-corpus-arch` (GOROOT packages target the committed corpus's arch; amd64 emission
  byte-identical), `r-test-global-alias-type`, `c2-literal-lift-access`. A row that fails to CONVERT its tests is theirs.
- **the host template**: `g-single-file-r2r-restore` makes `PublishReadyToRun` unconditional again in every csproj,
  `-tests` host projects included; hosts never publish single-file, so no verdict should move, but a `.tests.csproj`
  among the rewrites is a finding: post its diff.
- The converter rows re-convert every row's test sources. **N's MS13 refresh is in the base**, so G's NoInlining class
  should be gone from the TE patch: a G-FRAME hunk is a source the refresh missed (post it), OTHER is what O moves.

## 4. A row that fails

- Read its saved row output and classify it: count drift, divergence (test names plus the first line of the C# text),
  timeout, or build/infra. Compare it with that row's reading in your TRAIN N shard.
- For a verdict failure, run a **CONTROL** after the shard, never beside it: the same sweep command on the same row at
  the base **`<BASE>`**, in a second fresh worktree. Name a mover by row and test, with `control -> union` verdicts.
- Do not fix it. Report it.

## 5. What is NOT yours in this train

- Linux and darwin items: the linux lanes read the golib rows (GolibTests, the behavioral projects O adds, the linux-only
  goldens P1's targets row completes); darwin is a mac CI dispatch (and the release-smoke CI run is COORD's).
- GenTests, ChannelTests, go2cs.slnx, the warning-entries gate: the i7's battery runs them.
- The TLS limits experiment is not a shard row.
- The MS13 refresh itself: COORD applies it on the i7; your part is the patch.

## 6. What to post back

One message to COORD, at most 40 lines. Subject: `TRAIN O i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN O complement shard at <sha10>: <pass>/132 at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; base <sha10> on the first-parent line; list sha <16 hex> rows <n> (= the GO's); seats <n> all ancestors; go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- PRE execution-config rows: base [<expect none>] union [<expect none>]; control ref <sha10>^ [<expect log/slog>]
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt lines | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at <BASE>; N-shard verdict> (one line each)
- reflect / frame rows: <any named reading that moved | none>; log/slog: <N + D>, TestSetDefault/TestPanics/TestCallDepth <v>; internal/godebug: <N + D>, TestCmdBisect <v>
- deadlock lines: <0 | pkg=n ...>; NAMED internal/synctest TestDeadlockRoot/Child <v>; os TestRemoveAllWithExecutedProcess <v>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; HS <module-path hosts n, GoDefaultGodebug files n>; UF <n>
- driver exit <0 | 4>; SOFT lines: <none | each line>
- TE patch: <run>/tO-i9-logs/tracked-changes-U0.patch (<lines> lines; G-FRAME hunks <n>); your TRAIN N patch, if you still hold it: <path | gone>
- REFRESH patch (MS13): <run>/tO-i9-logs/tracked-changes.patch (<lines> lines, sha256 <16 hex>)
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's refresh | the control you are running>
```
Keep `<run>/tO-i9-logs/` and the worktree until COORD says the shard is read and the refresh is committed. If you still
hold your TRAIN N run folder, keep its `tN-i9-logs/tracked-changes-U0.patch` too: it is the TE-i9 baseline (COORD also
holds a copy under `coord-scratch/tN/i9-patches`).
