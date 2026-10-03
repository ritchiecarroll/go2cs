# TRAIN N script set: final verification report for COORD

Verified 2026-10-03, 04:10 to 04:26 Central, by the final read-only verifier. The set was derived from `trainM/` and then
revised in review round 1 (see `tN-CHANGES.md` section 4). Nothing was run for real: no assembly, map, fixup, battery,
converter or build. Nothing was committed or pushed, and no worktree was created. Evidence from this pass is in
`coord-scratch/tN/derive/verify-final/`.

**Verdict.** The set is ready to launch once COORD has ruled. Every item from N1 to N14 is either implemented or carried
as an owed ruling. The **draft list does not assemble as cut**. This pass confirmed that independently and found three
more things COORD must rule before the map (Q18 to Q20 below).

## 1. Files (all under `.claude/coord-scripts/trainN/`, git-ignored, so they need the checklist's `add -f`)

- **Scripts:** `tN-assemble.sh`, `tN-conflict-map.sh`, `tN-fixup.sh`, `tN-battery.sh`, `tN-emitcheck.sh`,
  `tN-modules-legs.sh`, `tN-linux-legs.sh`, `tN-i9-shard.sh`, `tN-i9-te.sh`, `tN-helpers.py`, `tN-regen-apply.py`,
  `emitdrift.py`, `tN-ng-parse.ps1`
- **Lists:** `tN-follow.txt` (empty), plus `tN-i7-sweeps.txt` (93 rows) and `tN-i9-shard.txt` (132 rows). Both lists
  are byte-identical to M's (checked with `cmp`).
- **Documents:** `tN-README.md`, `tN-CHANGES.md`, `COORD-LAUNCH-CHECKLIST.md`, `tN-lane-brief-i9.md`,
  `tN-lane-brief-linux.md`, `controls/README.txt`, and this report
- **Controls:** `controls/te-pos-goframe.patch` and `te-neg-i9-testing.patch`, both identical to M's
- **COORD's file:** `tN-seats-draft.txt` is tracked and was not edited by the derive, the review or this pass.

**Hygiene checks:**
- `bash -n` passes on all 9 `.sh` files, and `compile()` passes on all 3 `.py` files.
- Every file is LF, and there is no `__pycache__` or `.pyc`.
- `git status --porcelain` in hnd is empty.
- `git status --porcelain --ignored -- .claude/coord-scripts/trainN` lists only the trainN entries above, all `!!`: 21
  files plus `controls/`, which makes 22. This report makes 23.
- trainM is untouched: `git diff --stat -- .claude/coord-scripts/trainM` is empty, all 27 of its files are tracked and
  unmodified, and every mtime is 2026-10-02 (before the derive).

## 2. Items

| Item | Status | Verified by this pass |
|---|---|---|
| N1 rename, base | DONE | Every script names `BASE` / `MASTER` 8f46a9adae (full sha in assemble and map). The union branch, the lock, `refs/coord/tN-map`, the `fixup: TRAIN N` / `fixup-N: TRAIN N` subjects and the `FIXUP_SUBJ` regex are N's. origin/master = 8f46a9adae (ls-remote). |
| N2 seat count, CNR N | DONE (derived) | The seat count is read from the list; `SEATS_EXPECTED` is optional. `CNR_EXPECT_N` comes from `cnrexpect` at HEAD, with a control at the base. **Re-run here:** 791 / 6 / 785, +7 vs aa0a07d5fd, matching run3's `4 asserts: N=785`. Per-row deltas at the seat shas: 17 added, NativeFieldPortAlias skipped, so 801 at the draft as cut. The reconciliation is a finding only when it differs. |
| N3 EXEC empty | DONE | `EXEC_RULED = []`; `EXEC_ROWS_EXPECT` defaults to 0 from `execruled`. The linux L4 log/slog leg is retired. The repo template's lines 278-280 are outside trainN (Q10). |
| N4 PUB leg | DONE | The verdict regex matches `check-published-output.ps1:280` at 3ced87fd40, and the `NOT MEASURED` / `HUNG` wording matches. go comes first on PATH, then dotnet10, with DOTNET_ROOT unset. Cap 30m, n >= 5. The control is documented, not re-run. |
| N5 NGF-j0 | DONE | Both arms are kept, plus `-getProperty` and the script default. At a7873ee6c2, `j0-consume.ps1:26` and `J0UuidConsumer.csproj:17` both read `nugetgo.github.com.google.uuid`. |
| N6 CC leg | DONE | `go test -count=1 ./... -timeout 120s` in `src/tools/comparison-classifier` (present at the base). |
| N7 .editorconfig | DONE as a known class; **premise owed (Q16)** | emitdrift keeps `.editorconfig` out of drift. `editorconfig-written=0` is checked: a die in fixup step 4, a finding in leg E. COORD's notes (L:83 owner approval) make these entries converter emission, so the task's "not emission" premise is in question. |
| N8 signing by KEY | DONE | Checklist: `%GK` = 941694536F21BAFF, `%G?` G or U. Confirmed `user.signingkey` = 941694536F21BAFF, and 8f46a9adae / e2008427b1 read `U 941694536F21BAFF`. |
| N9a LX compare | DONE | The six names are CORRECTED from run3's `cnr.log`: MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName, WritevIovecSeam. "UnixAbstractSocketRoundTrip" is not a project. The set is derived with `cnrexpect`; `LX_KNOWN` is the check. LX now checks only its OWN restore. |
| N9b UF carve-out | DONE | The class is real (P1 read 17 and P2 read 21 at M). `uflinux` carves it out by GOOS=linux-only `_test.go`, by name. Not run, because there is no linux box here. |
| N10 MS13 refresh | DESIGNED, NOT RUN | `testsrc-refresh` plus README MS13 and checklist section 6. **Smoke re-run here** on run3's patches (output to scratch, nothing applied): files 338, refused 0, conflicts 0, excluded 27 (19 production, 2 proof pages, 5 siblings, 1 README). |
| N11 i9 brief | DONE | The shard list is unchanged and the crypto/tls / net ban stays. The driver writes `tracked-changes.patch`, and the brief asks for its path and sha256. |
| N12 manual steps | DONE | M1..M13 map to MS1..MS13 (MS3, MS6 and MS12 dropped, with reasons). N's steps are MS14..MS19, and review round 1 added MS20 and MS21. |
| N13 sweeps | DONE | Byte-identical. The crypto/tls reading is still `1340 = 4759 - 3419`. |
| N14 CHANGES | DONE | `tN-CHANGES.md` sections 2 and 4. |

**Surviving TRAIN M literals.** None is used as an N expectation. What remains:
- provenance comments (`trainM/tM-CHANGES.md section 9-11`, `COORD-RULINGS-r2/r3`);
- M's run3 record, used on purpose as the previous-train baseline (`M_SUMS`, `M_REWRITES`, `M_REWRITES_T`);
- the CNR control's fallback literal 785, which is stamped when used;
- aa0a07d5fd, only in comments and in the checklist's `cnrexpect` control command.

Some figures in the README and checklist are typed: 27 rows, `d62f0b3453e0`, 801. They describe the draft as cut, and
the docs say to read the printed value instead.

## 3. The dry run (`TABLE_ONLY=1 SIGN_PROBE=0`, from a per-run copy; 04:20, 56 s)

```
TABLE row 13 c2-nugetgo-id-pattern: stacked on row 12 r-module-license (declared; ancestor verified)
TABLE row 23 r-jwt-promoted-iface-repros: 'stack-on i9-crosspkg-promoted-forwarders' but a9cc98c1e3 is not an ancestor of ee52639557
ABORT: 1 seat-table problem(s) ...   rc=2
```

This is the same single problem the derive and review round 1 read. Every remote tip is at its seated sha, and there is
no origin/master NOTE. Afterwards: no `refs/coord/tN-map` ref, no `claude/coord-trainN-union` branch, no `/h/go2cs-tmp-coord/tN`.

**Independent assembly checks (git merge-tree, old 3-way form; controls fired on the three known pairs):**
- **Overlapping pairs:** 92 of the 351 row pairs share a path. Exactly THREE conflict: rows 4x9 (the BOARD), 17x27
  (`go2cs-src.projitems`) and 19x21 (`visitAssignStmt.go`). This confirms MS21 items 1 to 3.
- **MS19 HashSet scan, over every row:** only `g-using-static-alias-guards 2`. This confirms MS21 item 4.
- **The three rows accepted after the draft:** c2-toml-test-host 15838bfff5, r-test-host-init-order 0445460d7b and
  r-ptrptr-anon-struct-lift b95dd8d6e6. Each reads 0 conflicts against every overlapping draft row, and 0 unqualified
  HashSet uses.
- **The i7 module cache:** exact paths only. `hashset@v1.0.0` (dir, `.zip`, `.mod`) is ABSENT, and GOPROXY is the
  default proxy.

## 4. Owed COORD rulings, each with a recommendation

Q1 and Q3 to Q17 are numbered as in `tN-README.md` section 7 (Q2 was merged into Q1). **Q18 to Q20 are NEW from this pass
and are recorded only here.**

1. **Q1, row 23's note and H4 (one decision).** Route (b) if R pushes by 10:00: a re-cut on a9cc98c1e3 with committed
   emission and registrations, which makes the stack-on note true. Otherwise (c): row 23 waits for TRAIN O. Never apply
   the `after` edit by itself.
2. **Q3, linux log/slog.** Keep one plain L4 reading: `rowleg log/slog`, roster linux 199 + 17, `tiered=False`.
3. **Q4, os/exec 86 + 2 on P1.** Add an exception keyed by name: `TestExtraFiles go=pass cs=skip` and nothing else.
   Anything else stays a MOVER.
4. **Q5, `X_ROWS=encoding/json`.** Keep it as regression (about 2 min).
5. **Q6, `E_BISECT_KNOWN` empty.** Leave it empty, and name arms only if leg E reads above 0.
6. **Q7, PUB's environment (inferred).** Accept it. Read a NOT MEASURED result as the instrument first.
7. **Q8, `EXP_DESC` / `EXP_TH`.** Leave them unset (reported).
8. **Q9, MS13.** Confirm three things: the refresh commit rides before master's fast-forward, it needs no re-read, and
   linux patches stay out.
9. **Q10, outside trainN.** The template's `linux-legs.sh` lines 278-280 and `measurement-discipline` SKILL.md :40 / :99
   go to the doctrine batch.
10. **Q11, warm the i7 cache.** Run `go mod download` in the tN worktree's `src/go2cs` before the fixup. hashset v1.0.0
    is absent, so this keeps the network out of the gated runs.
11. **Q12, NativeFieldPortAlias.csproj (+3/-1 on linux).** Before the cutoff, C2 adds the linux re-emission to
    c2-native-array-view, or p1-linux-only-goldens carries it. Otherwise name it as a known mover in LX.
12. **Q13, MS21's routes, before the map.**
    - (1) G re-cuts g-trim-annotations without c2e7cef853; that BOARD entry lands as a docs commit after N.
    - (3) P2 re-cuts p2-n-cleanup on 537abad2a2 with `stack-on r-module-driver-gomod-less`, still the last row.
    - (2) + (4): **see Q20**. The two-parent re-cut that review round 1 recommended breaks precheck.
13. **Q14, re-cuts or machinery.** Re-cuts. Build no pre-resolution path.
14. **Q15, MS20's template-class csproj.** Launch the fixup with `CSPROJ_TEMPLATE=accept`: 13 csproj, or 14 with the
    toml row.
15. **Q16, `.editorconfig`.** Treat it as converter emission. If p2 converter-warning-clears seats at N, a script change
    is owed first (ordinary emission, drop the die and the finding). If it does not seat, keep the check as worded. No
    `claude/p2-*warning*` ref is on the remote.
16. **Q17, COORD's table edits.**
    - Add `c2-toml-test-host|15838bfff5|...` before p2-n-cleanup (L:172).
    - Drop `(owes go2cs.slnx)` from row 22 (N:24); a9cc98c1e3 registers its projects.
    - Re-point every routed row, which gives a new `<n>`, a new hash and a new map.
17. **Q18 (NEW), r-test-host-init-order 0445460d7b.** ACCEPTED at 03:40 (L:173, "add ... before p2-n-cleanup"), but the
    derive still lists it as PENDING (MS18).
    - **The problem:** it adds `src/tests/Behavioral/TestInitReadsModuleFixture/{control,fixture}`, which hold `.go`,
      `go.mod` and `keys/k` and no `.cs` and no csproj. That is the H4 class. PRERES dies, CNR transpiles 2 directories
      and reads the untracked output as CHANGED, and the derived N moves by +2. It is the same class as M's
      TestNeedsTransitiveApiRef, which was deleted in-seat.
    - **What is clean:** merge-tree reads 0 conflicts against all 27 rows (it shares `testConversion.go` with rows 17 and
      21, and the projitems file), and the HashSet scan reads 0.
    - **Recommendation:** R moves the fixture out of `src/tests/Behavioral` (route a) and pushes before 10:00, or commits
      and registers its emission (route b). Otherwise the row waits for TRAIN O. Do not seat it as cut.
18. **Q19 (NEW), r-ptrptr-anon-struct-lift b95dd8d6e6.** Accepted at 03:40, to board ONLY together with G's A2 seat.
    - **A2:** `claude/g-float-untyped-const-compare` was not on the remote at 04:2x.
    - **Its base:** d834821da0 (`r-atlas-batch-repros`) is not a row. It must be seated as an earlier row for the stack-on
      note to pass the table.
    - **H4:** d834821da0's `FloatCompareUntypedMaxUint64` is Go-only, so H4 fires unless A2 commits its emission and
      registrations.
    - **What is clean:** merge-tree reads 0 conflicts against the draft rows, including `visitAssignStmt.go` against rows
      19 and 21 (pairwise only).
    - **Recommendation:** board the trio (d834821da0, A2, b95dd8d6e6) only if A2 is pushed and accepted by 10:00 and H4
      reads 0 on it. Otherwise TRAIN O.
19. **Q20 (NEW), the two-parent re-cut for MS21 items 2 and 4.** Review round 1 recommended one P1 re-cut on a local
    merge of b9e2d5ffa1 and 1c182b5ca2, with two stack-on notes.
    - **Why it fails:** rows 15 and 19 are siblings (both on 8f46a9adae; neither descends from the other). precheck
      computes each seat's own base as `git merge-base <sha> <base> <earlier rows...>` with no `--all` (`tN-helpers.py`
      line 195), so it returns ONE of the two best bases. Shown on real commits: `git merge-base 209f055a89 aa0a07d5fd
      4904135fda 483d4ea217` prints only 483d4ea217, while `--all` prints both.
    - **The effect:** P1's "own" diff would then carry the other sibling's registration inserts (go2cs.slnx and the four
      BehavioralTests files). The COUNT arm reads expect too high and gives a false FAIL at PRERES, step 6, PRE-1 and the
      landing precheck.
    - **Recommendation, a single chain:** P1 re-cuts p1-hashset-module on 1c182b5ca2 only (`stack-on
      c2-multi-func-define`, which resolves item 2). For item 4, EITHER G re-cuts g-using-static-alias-guards on P1's new
      tip with the two lines spelled `hashset.HashSet[string]{}` (plus the import), moving that row after
      p1-hashset-module with `stack-on p1-hashset-module`, OR a ruled follow-up merge on its own branch (README MS21's
      alternative). The two-parent route is usable only after precheck's own-base learns about multiple merge bases (a
      script change with a control, COORD's call). Otherwise p1-hashset-module waits for TRAIN O.

## 5. What COORD must do before the assembly (in order; cutoff 10:00 Central)

1. **Route the re-cuts:** Q13 / Q20 to G, P1 and P2 (plus G again or a follow-up for item 4), and Q1 to R. Decide Q18
   and Q19, and post the routes.
2. **Edit `tN-seats-draft.txt`** (Q17 plus the routed shas, stack-on notes and new rows; the toml row before
   p2-n-cleanup; p2-n-cleanup stays last).
3. **Before the map, from a fresh run folder** (`cp -r trainN/. $R/`):
   - Run MS9c's controls, then the real `TABLE_ONLY`. EXPECT `TABLE OK: <n> rows ... rows-sha256=<h>`.
   - Run MS19's HashSet scan. EXPECT no line.
   - Run an H4 census of the new list: Go-only behavioral directories per row. EXPECT 0 once Q1 and Q18 are routed.
4. **Rule Q15** (`CSPROJ_TEMPLATE=accept`) **and Q11** (warm the cache) before the fixup, and Q16 if p2's warning seat
   boards.
5. **Commit the script set** with checklist section 1: `add -f -n` first, then `add -f` (this report rides along),
   signed. Read the signature by KEY, announce, then push hnd.
6. **Map, then assemble.** `tN-conflict-map.sh` must print every row clean. Then `tN-assemble.sh`; the assembly refuses
   a list no map has read.
