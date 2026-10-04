# TRAIN O script set: derive report for COORD

Derived and verified 2026-10-03, 19:55 to 21:35 Central, by a workflow agent (step 3 of the O derive). Nothing was run for
real: no assembly, map, fixup, battery, converter, build or `go test`. Nothing was committed or pushed, and no worktree was
created; `/h/go2cs-tmp-coord/tN` was not touched (reads of N's object store went through `/h/Projects/go2cs`, which shares
it), and `trainN/` / `trainL/` were not edited (trainN's newest mtime is still 12:11). Evidence: this file, `tO-CHANGES.md`,
and the scratch scripts in the session scratchpad (`rename_to.py`, `edit_*.py`).

**Verdict.** The set is ready for COORD's review and the owed rulings (`tO-README.md` section 7, Q1 to Q20). The **draft
does not assemble as cut**: six rows owe a re-cut or a new ref (MS21), the same five conflicts plus A2's BOARD tip the
pre-map read. Two rows are NEW since the inputs were computed and are seated in the draft pending acceptance:
`claude/p1-linux-only-goldens-targets` d745548f77 and `claude/coord-census-dotted-handle` 2b6213d821. One N finding was
diagnosed and fixed for O: N's PUB leg never measured (rc=150, an instrument fault).

## 1. Files (all under `.claude/coord-scripts/trainO/`, git-ignored: the checklist's `add -f -n`, then `add -f`)

- **Scripts:** `tO-assemble.sh`, `tO-conflict-map.sh`, `tO-fixup.sh`, `tO-battery.sh`, `tO-emitcheck.sh`,
  `tO-modules-legs.sh`, `tO-linux-legs.sh`, `tO-i9-shard.sh`, `tO-i9-te.sh`, `tO-helpers.py`, `tO-regen-apply.py`,
  `emitdrift.py`, `tO-ng-parse.ps1` (and step 2's `tO-premap.sh`, `tO-regcheck.py`, not edited)
- **Lists:** `tO-seats-draft.txt` (34 rows, the proposed table), `tO-follow.txt` (EMPTY), `tO-i7-sweeps.txt` (93) and
  `tO-i9-shard.txt` (132), both byte-identical to N's (`cmp`)
- **Documents:** `tO-README.md`, `tO-CHANGES.md`, `COORD-LAUNCH-CHECKLIST.md`, `tO-lane-brief-i9.md`,
  `tO-lane-brief-linux.md`, `controls/README.txt`, this report; step 1's `tO-seats-candidates-step1.txt` (renamed from the
  step-1 `tO-seats-draft.txt`, unedited) and step 2's `tO-premap.md` + `premap-run1/`
- **Controls:** `controls/te-pos-goframe.patch`, `te-neg-i9-testing.patch` (byte copies)

**Hygiene, read at the end:** `bash -n` passes on all 10 `.sh`; `compile()` passes on all 4 `.py` (nothing written); no
`__pycache__` or `.pyc`; every file LF (CORRECTED by review round 1, 21:50: `tO-regcheck.py`, step 2's, was CRLF
throughout, shebang included; converted to LF, `compile()` passes); a grep for an apostrophe inside `${VAR:?...}` reads none; `git status --porcelain`
in hnd is empty and `--ignored` lists `trainO/` as `!!`.

## 2. The task's items

| Item | Status | Verified by this pass |
|---|---|---|
| names / paths | DONE (R0) | `W=/h/go2cs-tmp-coord/tO`, `claude/coord-trainO-union`, `coord-scratch/tO` (lock, scratch), `/h/go2cs-tmp-coord/tOemit*`, `refs/coord/tO-map/...`, `fixup: TRAIN O`; residue grep (`tN`, `TRAIN N`, `trainN` outside N-record pointers) clean; M-history pointers restored (O13) |
| BASE / MASTER required | DONE (O1) | no script uses 8f46a9adae or 59ee0d21bf outside comments (grep over code lines: none); every `BASE`/`MASTER` is `${...:?}`; first-parent / origin-master checks added; the battery passes `BASE` to MOD; the lane briefs carry `<BASE>` |
| `tO-seats-draft.txt` | DONE (O11) | 34 rows in premap order v2; all 34 remote tips at their seated shas (ls-remote 20:17); ancestry over every row pair = exactly the six declared stacks; the table-token extraction reads six `stack-on`, one `after` (earlier row), four `CORPUS FOOTPRINT` groups = 32 paths; rows-sha256 `7391e9e7058d` as cut; `merge-base --all` per row (59ee stand-in): no row has two bases |
| PUB_MIN 5 -> 6 | DONE (O4) | `PUB_MIN=6` |
| CNR derived at HEAD | DONE (carried) | PRE-D's derivation unchanged; its control reads N's leg-4 stamp from `N_RUN` (fallback 801, stamped); PREDICTED 819 at the routed draft (801 + 18 Go package directories added, 0 exclusivity marks: read with git) |
| release-relevant | DONE (O14, MS23) | README MS23 / 4b / section 5, checklist 5b, battery header: no i7 leg runs `release-smoke.ps1`, the per-flavour pack or arm D; the i7 legs that touch the two seats are C / CB (the converter's Mac RID pin), 2b / GT / H7 (golib buildTransitive targets) and NV51 / NV7 (push-nuget.ps1); the os-matrix `release-smoke` dispatch command and its EXPECT come from the workflow's own inputs at 5cda875206 (`goos`, `stage`) |
| N lessons | DONE | the standing-drift classifier kept byte-for-byte in logic with its control; the first-parent signature read as its own command before the push (checklist 1 and 4); NGa51 EXPECT 'ran 16, failed 0' under both editions; NEW-ref push -> ls-remote read-back -> announce (checklist 4, section 0) |
| i9 shard list | DONE (byte-identical) | the C1 banks change only darwin annotations (22 rows; columns 2/3 and linux text unchanged; 225 rows); `cmp` identical, sha256 057c78c1...cdf3d |
| lane briefs | DONE (rewritten) | `<UNION>`, `<FIXUP>`, `<HND>` kept; `<BASE>` added; O's expectations |
| syntax | DONE | section 1 |

## 3. What was MEASURED (read-only) to back the changes

- **O5 (PUB, an N finding).** N's bat1 SUMMARY line 192-194: `LEG PUB rc=150 wall=2s` / `NO VERDICT LINE`;
  `published-output.log`: 'You must install or update .NET to run this application ... pwsh.dll ... Framework
  Microsoft.NETCore.App 8.0.0 ... .NET location: <profile>\dotnet10\'. Reproduced at 20:12 on this box (no build): N's
  PATH (go pin + dotnet10 + PATH, DOTNET_ROOT unset): rc 150, the same text; the pinned-go PATH: rc 0, PSVersion 7.4.6; the
  O5 invocation with a stub `check-published-output.ps1`: `(Get-Command dotnet).Source` = `<profile>\dotnet10\dotnet.exe`,
  `dotnet --version` 10.0.400, the stub's `exit 7` propagated as rc 7. COORD's notes to 19:56 do not mention N's PUB
  finding (Q19).
- **D1.** On the pre-map's chain C commits (dangling, in the shared object store): A1's own -U0 hunk in Compile, Target and
  TranspileTests (old start 1577 / 1580 / 1577, length 0, md5 35a7d7c1) is byte-identical to the union side's hunk at the
  same position at chain step 11 (2f589b78c8, A2 merged); the merge of A1 (step 12, fc6e412756) leaves each file's line
  count UNCHANGED (2340 / 2390 / 2336) while A1's own net is +3 each: the credit is exactly the arm's error. OutputComparison
  has no shared hunk (A2 does not register PtrToAnon there): no credit, correctly.
- **O3.** On N's real union: the merge commit of N's second seat (97374b9a5f) against `8f46a9adae` + seats 1 and 2 has TWO
  best common ancestors (b3e6969582, aec4f4b20a) where a single-parent row reads one (`8f46a9adae`): the predicate the table
  NOTE and precheck use. The synthesis on that pure merge reads own = 0 on docs/GoCorpusMigration.md (net(B1..c) 512 -
  net(lca..B2) 512), where N's one-base arithmetic would have charged the row 512 lines.
- **O2 (a).** The train-commit predicate (the assembler's and the map's regex, run as a git read): r-docs-refresh and
  g-single-file-r2r-restore read **31** over `8f46a9adae` (N's 30 seat merges + its fixup) and **0** over `59ee0d21bf`;
  c2-defer-receiver-copy (cut on an N seat) and r-ptrptr-anon-struct-lift read 0 over both: the predicate fires on the
  wrong base for the 59ee rows and stays silent on the right one, and the N-seat rows are its stated blind spot.
- **O6.** `git diff 8f46a9adae 73de37d0da -- docs/ValidatedTestPackages.md`: 22 row lines re-written, each the same line
  with `· darwin: N [+ D]` appended after the linux annotation (linux text byte-identical); the prose blocks are comments.
- **D2.** The template's delta 8f46a9adae..4b30906aea is `+ <!-- Trim the framework only ... -->`, `+ <TrimMode
  Condition=...>partial</TrimMode>` and the PackageId marker; PtrToAnonStructPtr.csproj (b95dd8d6e6) differs from
  InitFrameNames.csproj (4b30906aea) by exactly those two lines beside its own name and a runtime ProjectReference; all 18
  new csproj are `<OutputType>Exe`, TrimMode 0.
- **O10.** `git log -1 -S'execution: release-tiered' 59ee0d21bf -- docs/ValidatedTestPackages.md` = 2d46eba8f0 ('roster:
  log/slog runs at the Release+TC0 default -- execution: release-tiered dropped'); its parent's roster holds the log/slog
  annotation (6 phrase occurrences there, 5 at 59ee).
- **Footprints.** p2 (18264b3359, 14 commits over e2008427b1 = over 8f46a9adae): 21 src/core paths, of which 8 generated
  `.cs` + 11 `.editorconfig` are composed into REGEN_ALLOW (the hand-own managed_impl.cs and the sibling poolqueue.cs.auto
  are not); fm-record: 11 package_info.cs (+ the hand-owns value_impl.cs and managed_impl.cs, not composed); xsys-libc: 1
  vendor file; B1's runtime/windows/os_windows.cs: generated (its three 'GoManualConversion' mentions are placeholder
  comments, none line-anchored). Linux-only csproj: r2r-restore changes the six base projects +1/-1 and not
  NativeFieldPortAlias.csproj, which lacks the TrimMode pair: LX's PREDICTED +2/-0.
- **New rows.** `claude/p1-linux-only-goldens-targets` d745548f77: cut on 758103a4ef (N row 31; CORRECTED by review round 1: N row 29 in N's first-parent order, and f6ce1a0ab7 is N row 28), 3 files +10/-10, each
  `main.cs.target` the SAME blob as its `main.cs`; P1's announce 19:59 and read-back 20:00 (mailbox 2de4b92091,
  358e0e3215). `claude/coord-census-dotted-handle` 2b6213d821: cut on 59ee0d21bf, one file (the census script, +49/-1);
  ledger 2fb409e938 (20:08) calls it 'a TRAIN O row'.
- **Unqualified HashSet.** As in the pre-map: c1-darwin-xsys-libc adds two (src/go2cs/xsysDarwinLibc_test.go); hashset's
  re-cut must convert them (MS19).

## 4. Owed COORD rulings (the README's section 7; the short form)

Q1 release-smoke's remote tip 5cda875206; Q2 the BOARD route (and who names A2's new ref at b967f9fa7f); Q3/Q4 G's pair
(single-parent fm-record route, json/package_info.cs regenerated by the fixup); Q5 B2 on B5 alone; Q6 C2's README route;
Q7 P1's hashset re-cut after N; Q8 accept P1's targets ref; Q9 r-jwt repros; Q10 p2's gate; Q11 B1's os_windows.cs via
REGEN_ALLOW; Q12 CSPROJ_TEMPLATE=accept; Q13 the trio as a set; Q14 docs-only items; Q15 g-untyped-float-operators O or P;
Q16 B3's and the C1 banks' gates; Q17 accept coord-census-dotted-handle as an O row; **Q18 (NEW) MULTI-BASE: accept the
unrun synthesis or require single-parent re-cuts; Q19 (NEW) N's PUB never measured: re-run it by hand on N's union before
N lands, or accept G's seat-level 6/6; Q20 (NEW) EDITORCONFIG_NEW default.**

## 5. Not done, and why

- **No run of any changed arm** (precheck's credit / MULTI-BASE / DARWIN, csprojtemplate's D2, emitdrift's `.editorconfig`,
  the fixup's EDITORCONFIG_NEW path, PUB, IDC, WE / WEc): the task forbids runs. Their predicates were measured with git
  where git could answer (section 3); MS9b lists the controls COORD runs before the first fixup.
- **The pre-map at BASE**: BASE does not exist. Re-run it (README step 0) once N lands.
- **N's bank step against the C1 banks' roster edits**: unmeasured (the step is not cut); the DARWIN arm and the roster
  guard x2 read it at the union.
- **`tO-premap.sh` / `tO-regcheck.py`**: step 2's, not edited; `tO-regcheck.py` (registration invariant) knows no D1 credit:
  it read `head = expected - 3` at chain C by design (PM section 4).
