# TRAIN L battery: draft summary

**DRAFT 2 (2026-10-01).** Both critiques (gate coverage, mechanics) and COORD rulings R1-R8 are applied; `CHANGES.md` maps every finding to the lines it changed. Nothing here was run either.

Drafted 2026-10-01 03:31 by the battery-prep workflow. **Nothing here was run.** The only commands run were read-only
git/grep reads, syntax checks, and pure-reader helper checks against the tL tree.
Union: `/h/go2cs-tmp-coord/tL`, branch `claude/coord-trainL-union`, head 6960c8071f. It is 11 signed seat merges
onto TRAIN K's landed master 75648a022b.

## The union shape: a fixup is owed (not an emission footprint)

No seat claims a -stdlib emission footprint, and nothing read here shows one. All 11 seats state 0. Leg E is the proof,
and its bisect arms run only if E reads more than 0. The union still needs **one signed fixup** (`tL-fixup.sh`), for
two reasons measured at 6960c8071f:

| | Fact | What breaks without it | Fixup step |
|---|---|---|---|
| F1 | `ForClauseSpill.csproj` (r-d6) and `CrossPackagePromotedValueMethod.csproj` (i9-crosspkg) were cut at J's template. They still carry `<LangVersion>latest</LangVersion>` and lack the OSR opt-in. `optin.py check` fails naming exactly these two (1140 descendants, 229 test hosts). K master: 0. | CNR reads both CHANGED; PRE-1's S1 census fails; leg E's csprojdrift reads them `new-MISSING-EDIT` | K's S1 `sed -b` on exactly 2 files, then `optin.py insert` (expect `2 edited, 1138 already, 1140`), then `check` PASS. Each file reads 4/1. |
| F2 | g-deadlock added `[GoTestMatchingConsoleOutput]` to ChannelReceiveFromNil and ChannelSendToNil but not their D4 rows. 718 projects are marked and 716 listed (`tL-helpers.py outparity`, same rule as the guard). | Leg C: `TestOutputComparisonListMatchesConsoleOutputAttribute` reds on both names | R1: `OCT_REPAIR=fixup` only -- 2 rows at UpdateTestTargets' sorted positions, CRLF kept, 6/0, attributed to g-deadlock-checkdead. |

Battery PRE accepts exactly: first-parent = 11 seat merges [+ ONE merge whose second parent is on `origin/claude/i9-nugetgo-pack`, seated immediately before the fixup: the i9's nugetgo-pack 5.1 parse fix, R2] + 1 fixup, subject `fixup: TRAIN L`. Fetch that remote-tracking ref into tL before launch; the battery never fetches.

**R1 (ruled):** the two D4 rows go in the fixup (sorted position, CRLF kept), attributed to g-deadlock-checkdead; `OCT_REPAIR=seat` is withdrawn.

## Files (`H:/go2cs-tmp-coord/coord-scratch/tL/wf/draft/`)

| File | What |
|---|---|
| `tL-battery.sh` | The i7 battery, adapted from tK-battery.sh and tK-reread.sh. K's env, `leg`/`purge`/`tleg`, the C: backslash GOROOT, temp and caches on H:, per-run copy, rc per leg. |
| `tL-fixup.sh` | The fixup draft (F1 + F2), one signed commit, with asserts. |
| `tL-emitcheck.sh` | Leg E plus the bisect arms. `TAG` sets the evidence folder. K6 dropped, K9 kept as a regression check, plus a HANDOWN reading. |
| `tL-helpers.py` | tK-helpers verbatim, plus `rosterrow`, `outparity`, `teattr` and `wallcmp`; DRAFT 2 adds `trxmethods` (R3), `deadlock`, `cmpnames` and `lockcheck`. |
| `tL-modules-legs.sh` | The R-chain manual readings as legs (CM, MR2 [cache], JWT [cache], MR3, MR4/MR4v/MR4c, MR6/MR6c, MR6p/MR6e/MR6pc). The battery runs it as leg MOD; it exits 4 on any failed verdict. |
| `tL-i9-te.sh` | NEW: the scripted TE reading of the i9's shard patch (leg TE-i9). |
| `tL-ng-parse.ps1` | NG(b): ParseFile under the running edition. |
| `tL-i7-sweeps.txt` | **93** rows: K's 91 plus `runtime` and `runtime/pprof`, both read by the T legs and not by the S loop. |
| `tL-i9-shard.txt` | **132** rows, byte-identical to K's (sha256 `057c78c1f9ad47f1...`). It holds none of the 10 banned TLS/net rows. |
| | Coverage: 93 + 132 = **225** = roster, 0 duplicates, helper verdict OK. |
| `tL-i9-shard.sh`, `tL-lane-brief-i9.md` | The i9 driver and brief: L asserts, deadlock-line counts, named rows, and a TE patch plus HS stamp for COORD. **NGE is MANDATORY before the bank (R8).** |
| `tL-linux-legs.sh`, `tL-lane-brief-linux.md` | The P1/P2 driver and brief. |
| `tL-seats-draft.txt` | A snapshot of the hnd seat list, for the per-run copy. |
| `controls/` | TE positive controls (the D6 and crosspkg golden diffs) and a negative control (L's own src/core diff). |

## Legs, the seats they gate, and expected readings

| Leg | Gates | Expect |
|---|---|---|
| PRE / PRE-1 / 1b / 1c | union shape, all seats | Shape OK. precheck rc=0: projitems 411 = 400 + 11, slnx/table counts, S1 census, optin PASS. outparity 0 gap. Host census: 0 module-path hosts, GoDefaultGodebug in exactly 3 files. |
| PRE-2 / 3 | coverage, canaries (c1-token is bridge-touching) | 225 = 93 + 132; five canaries derived |
| C | every converter seat, c1-fixture, nugetgo's repoguard census | ok throughout (the attribute guard is green only after F2); r-m3 driver dirs in TEMP counted and removed |
| CB | c2-publish-binlog-onK (3 binlog + 3 testHost GOOS + 5 projitems tests) | 11/11 `--- PASS` by name, 0 SKIP/FAIL |
| FX | c1-fixture-currency | `tracked 1281 · current 1281 · stale 0` (C1's pre-read: read it, don't carry it) |
| FXc1 / FXc2 | c1-fixture-currency's controls on real data | FXc1 (go1.23.12 GOROOT, empty GOMODCACHE): rc≠0 + `no GOROOT at the pinned go1.24.13`. FXc2 (H: GOROOT copy, one go/printer fixture altered): rc≠0 + exactly 1 STALE FIXTURE naming it |
| E (+bisect) | footprint 0 of every seat; csproj template drift | plants OK, 6 × rc=0, union-attributable **0**, CSPROJ template-only, HANDOWN 0 |
| G1/G2, ST51/ST7, NV51/NV7/NVR, HOP, PB | c2-publish-binlog-onK | roster guard passes (count read fresh, not 1977); ST **62/0**; NVR refused by name; HOP verdict for cmp; PB rc=0, Validated 4, pre=absent, `publish.binlog` SEEN during (poll) and absent after; SPB (cmp, `-PublishBinlog`) PASS, pre=absent, binlog SEEN then absent, 0 copies in the evidence folders SPB created |
| SY, SI/SIc, 2b | r-d6, i9-crosspkg, g-deadlock projects; golib API (c1-token, g-deadlock, r-m4, r-m6) | SY clean; SI clean with the control firing six cycles; 2b errors 0 |
| NGa / NGb / NGF-j0 | i9-nugetgo-pack | `ran 29, failed 0` ×2. NGb51/NGb7 0 parse errors over the 3 nugetgo files + j0-consume.ps1: **NGb51 is GATED (R2)**, it was red before the i9's fix. NGF-j0: the consumer's PackageReference is `go.github.com.google.uuid` by default and the nugetgo ID with `-p:J0UuidPackageId`. |
| TR | r-m4, r-m6 (the testing host) | pass |
| GT Debug + Release ×3 (TRX by name) | c1-token, g-deadlock, r-m4, r-m6, the SyncMutexProfile pair | 0 failed, 0 NOT FOUND. SyncMutexProfileTests clean in all 3 Release runs (the class has **11** methods at the union; the spec says 10/10). C1 probe (R3): FOUND/NOT-FOUND by method name (`FieldOfElement`/`ElementField` in PointerTokenUniquenessTests), stated, not gated. Build and test are separate legs. |
| 4 CNR | r-d6, i9-crosspkg, g-deadlock goldens (first reading under K's converter) and the F1 csproj | NO REGRESSION over **778** (INFERRED: 784 behavioral `.go` dirs − 6 platform skips), N read from the verdict line; none of the 5 new dirs in the skip list |
| 5 + B: | r-d6, i9-crosspkg, g-deadlock (5 projects + 2 now Output-compared + 8 neighbours) | all pass; main-alone pair exit 2 / stderr fatal line; read the timeout list for the new HANG residual |
| H7 ×3 | golib/testing/runtime/godebug/time_impl | CS=0 on windows, linux and darwin |
| MOD | r-m2/m3/m4/m6 | CM 19/19 PASS, 0 SKIP. MR3: `Converting 5`, `jwtlike 1; request 1`, stamp == `go list -test`. MR4 `keys 1; parse 2`. MR4v `vend 1`. MR4c rc≠0 + the 3 tests go=pass cs=fail BY NAME. MR6 `gd120 2; override 1`; MR6c rc≠0 + both root tests go=pass cs=fail by name; MR6p `recovered nil: true`; MR6e (GODEBUG=panicnil=0) `false` on both sides; MR6pc no stamp + `false`. MR2: lock == go.sum for runewidth + uniseg. JWT: root project 0 CS1929. MR2/JWT read `NOT MEASURED: module absent` when the cache lacks them (R6). |
| S (91 i7 rows + the derived canaries not on the list; INFERRED 3 as in K: crypto/cipher, go/types, encoding/json) + NR ×4 | c1-token (reflect, fmt, canaries), g-deadlock (net/rpc, jsonrpc, time), r-m4/m6 (every host) | every row PASS at banked counts. net/rpc 15 with 0 deadlock lines (console + full output + record stderr), **5/5** with NR. Each row capped at SWEEP_ROW_CAP (4h). The sweep's rewrites are restored before the T legs. |
| TE / HS (after S), TE-T / HS-T (after T), TE-i9 | r-d6, i9-crosspkg / r-m4, r-m6 | TE 0 signature lines (patterns INFERRED, controls fire). HS 0 / exactly 3. csproj rewrites 0. |
| T:runtime/pprof, T:runtime, TBS-W | c1-token, g-deadlock (crash family by name), r-m6 | pprof **145 + 7** BANK-ELIGIBLE + roster match. runtime **10819 + 71** + roster match; crash family pass/pass; /panic and /trap disclosed. TBS-W (now BEFORE T:runtime) 0 deadlock lines. |
| TK-WALL | c1-token's element-token cost | no S/T/NR row slower than 1.25× the fastest passing K reading (K's runtime: 6162 s) |
| i9 shard | the same seats on 132 rows | 132/132; synctest TestDeadlockRoot/Child pass/pass; os TestRemoveAllWithExecutedProcess pass/pass; TE patch + HS returned |
| linux P1 | c1-token (5 rows), g-deadlock (net/rpc + LB), r-m2/m3/m6 (LCn/LCf), r-m4/m6 (GT) | GT ×2 0 failed; LCn 19/19; rows at the linux annotations (396+22, 62+1, 3, 532, 46+6, 15); LPB rc 0, pre=absent, binlog SEEN then absent |
| linux P2 | g-deadlock (runtime, TBS ×10 under load), c1-token | runtime/pprof **147 + 7**; runtime **10810 + 73** with /panic disclosed; TBS 10/10 clean |

## Open questions and things I could not determine

1. ~~**F2 route.**~~ RULED R1: fixup step 4, attributed to g-deadlock-checkdead.
2. RULED R2 (5.1 parse is mandatory; the i9 fixes it; NGb51 gated). Original note: **nugetgo-pack.ps1 under Windows PowerShell 5.1 is RED** (parse). Options: the seat owner fixes it (ASCII `--` or a
   BOM) before landing, or COORD rules it pwsh-7-only and the `#Requires` changes. Until then NGb51 is an expected
   red, and it is NOT absorbed.
3. RULED R3 (read by method name, stated not gated; the bank waits on it). Original note: **c1-token's field-of-element token probe is owed.** By reading (INFERRED): FieldRefBox's base is now the storage's
   id, so `&s[0].f == &s[1].f`. The battery reads it by name only. Should L bank c1-token before C1 cuts that arm?
4. RULED R4 (direct `-tests` legs stay). Original note: **T rows.** runtime and runtime/pprof go through K's direct `tleg` plus a new roster compare, not the sweep
   wrapper, because `$longTimeouts` has no runtime floor and the default 10 m would kill it. Confirm, or pass
   `-TestTimeout 150m` to a sweep row instead.
5. **TBS load shape** on P2 (nproc busy loops) is INFERRED. K's red came from real concurrent work.
6. **CNR N = 778** is INFERRED (784 dirs − 6 platform skips). Read it.
7. **E-bisect** covers only the two converter seats with emission predicates (D6, crosspkg). A drift in neither arm
   means COORD bisects the M chain by hand.
8. **Review siblings (ruling 857afcb47e).** If leg E's U-arm sibling list is non-empty, a post-battery sibling-refresh
   commit is owed, as in K. It is not in the fixup.
9. **MOD needs a warm `H:\nuget\packages`.** Phase B builds restore. R's first reading timed out with egress down.
10. **LCf on linux** is the first full linux converter-suite reading. Unrelated reds are possible, and the brief
    says to report them, not gate on them.
11. **The linux BehavioralRunner path** (`bin/Debug/net*/BehavioralRunner`) is INFERRED from run-behavioral.ps1. A
    missing executable reads NOT MEASURED.
12. **r-m2's TempDir cleanup** may fail on Windows with 'Access is denied' (INFERRED by R). If leg C reds there, it
    is the instrument, not the lock.
13. **TE patterns** are INFERRED. Controls: D6 fires 8 lines on its own golden diff. XPKG fires 6 of the 7 changed
    lines on crosspkg's (the method-expression lambda is not a pure insertion). The negative control is 0 on L's
    src/core diff.
14. **Stale doc, not a gate:** docs/README.md:205 still says `-tests` "cannot be combined with `-recurse`".
15. **GT-Release ×3 adds about 12 min over K's single run.** It is kept because r-m6's SyncMutexProfile arm and
    g-deadlock's profile-gate neighbours both ask for full runs.
16. **r-m4's in-module LINK skip** has no battery leg (critic 0 #15): ask R for a GolibTests arm. MR4v covers `vendor/`.
17. **DEADLINE (R5)** defaults to 17:30; the per-row cap and MOD's 4h cap stop the battery (exit 5) and LIST candidate
    orphans by command line for COORD to kill by PID; nothing is killed by name.
18. **SPB's evidence-root glob** (`scratchpad/sweep-oracle-flake/<stamp>/cmp/*/publish.binlog` under the worktree) is
    read from run-validated-sweep.ps1:557/1087 (`$repo` = repo root, confirmed by the verifier). The folder is gitignored
    and persists, so SPB counts only the `<stamp>` folders absent from a listing taken just before the leg. SPB and PB
    each record and delete any `src/core/cmp/bin/tests/publish.binlog` left over before their poll starts
    (`pre=PRESENT` is a FINDING), so SEEN can only come from that leg's own publish.
19. **The S run list adds the derived canaries** (critic 0 run-list note): by K's precedent crypto/cipher, go/types and
    encoding/json are i9 rows, so those rows run on BOTH boxes. State it in the GO.
