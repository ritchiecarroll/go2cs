# TRAIN L battery: DRAFT 2 changes

`draft/` was copied whole to `draft2/`, and only `draft2/` was edited. Line numbers are draft2's.

Abbreviations: **B** = tL-battery.sh, **M** = tL-modules-legs.sh, **F** = tL-fixup.sh, **E** = tL-emitcheck.sh,
**I9** = tL-i9-shard.sh, **LX** = tL-linux-legs.sh, **H** = tL-helpers.py, **BI9** = tL-lane-brief-i9.md,
**BLX** = tL-lane-brief-linux.md, **RD** = README-draft.md.

New files:
- `tL-i9-te.sh`, the scripted TE-i9 reader.
- `CHANGES.md`, this file.

Where a ruling and a finding conflicted, the ruling won (noted in the row).

## Critique 0: gate coverage

| ID | File:lines (draft2) | What changed / NOT APPLIED + why |
|---|---|---|
| C0-1 | M:378-384; M:371; B:603-606 | **M:** prints `END failures=` on its own line and exits 4 when FAILS>0 or tracked changes/deletions remain. The MR6pc label no longer contains a colon. **B:** MOD now greps `': (PASS\|FAIL) -- '`, NOT MEASURED, ABORT, END and DEADLINE STOP, and stamps each line separately (no 900-char cut). It also states MOD's rc. |
| C0-2 | B:338-346 | New leg CB runs the 3 binlog tests, K's 3 testHost GOOS tests and the 5 TestProjitems* tests with `-v`. Each must print `--- PASS`, with 0 SKIP/FAIL lines. A miss is a FINDING. |
| C0-3 | B:309-315, 640-656; LX:229-243; BLX:55, 149 | `poll_start`/`poll_stop` run a 1 s background poll killed by PID. **PB** records the binlog SEEN during the run and absent after; anything else is a FINDING. **P1** gets leg LPB (`cmp -tests -test-publish-binlog`) with the same poll: rc 0, SEEN, absent after, and summ checks the record's targetGOOS=linux. An EXIT trap kills the poll if tleg aborts. |
| C0-4 | B:616-637 | New leg SPB runs `run-validated-sweep.ps1 -Filter cmp -Exact -PublishBinlog`. Expect PASS, binlog SEEN then absent, and 0 `publish.binlog` copies in the evidence root. **Conservative:** a separate leg, so HOP keeps K's exact shape. The evidence-root glob was confirmed by the verifier (RD #18); V4/V5 add the pre-check and limit the count to the folders SPB created. |
| C0-5 | tL-i9-te.sh (new); B:770-779; BI9:69; RD | The TE reader is scripted over the i9 patch. It runs as leg TE-i9 when `I9_PATCH` names the file; otherwise it is stamped NOT RUN with the exact command. A nonzero rc is a FINDING. **Conservative:** the patch arrives from another box, so it is not a hard dependency. Checked read-only on the controls: D6 positive rc=1 (8 lines), negative rc=0. |
| C0-6 | B:758-768 | After the T legs, TE-T (`git diff -U0` → teattr) and HS-T (`hs_m4`/`hs_m6`) run. A non-zero result is a FINDING. There is no restore after T: the bank material stays in the tree. |
| C0-7 | M:351-360 | New MR6e: `GODEBUG=panicnil=0 go run .` and the stamped converted prog120 run under the same env must both print `recovered nil: false`. The optional randseednop=1 arm is **NOT APPLIED** (marked optional). |
| C0-8 (+R7) | M:121-127 (ctlconv), 322-336 | New MR6c: e9009f2945's converter, built from `git archive` into `/h/go2cs-tmp-coord/coord-scratch/tL/mod-ctl-build/m4` (R7, never a worktree), runs gd120 into its own root `$OUT/m6c`. Expect rc≠0, and `hp cmpnames` must read both root tests go=pass cs=fail by name. The record path `<out>/src/<importPath>/go2cs_test_comparison.json` is INFERRED from M3's layout. |
| C0-9 (+R6) | M:133-160; H:cmpnames/lockcheck | New MR2 runs only when the 6 exact cache paths are present: the extracted dirs plus the download `.mod`/`.zip`, under `go env GOMODCACHE` = H:\go\pkg\mod. All 6 were listed as present on 2026-10-01. Otherwise it stamps `NOT MEASURED -- module absent`. The oracle runs `go mod tidy` + `go run` with GOPROXY=off (expect 5). Then `-recurse`, and `hp lockcheck` requires every lock entry == the fixture's go.sum h1 line and the set to be exactly runewidth@v0.0.13 + uniseg@v0.2.0. The fixture must be unchanged. |
| C0-10 (+R6) | M:162-196 | New JWT leg runs only when jwt/v5@v5.3.1 is in the cache (present). A consumer module requiring jwt is converted with `-recurse`. The jwt ROOT project (`<out>/pkg/github.com/golang-jwt/jwt/v5`, layout INFERRED) builds alone and must give rc 0 and **0 CS1929**. A missing project is a FAIL, not a pass. The consumer's run vs `go run` is INFORMATION only. **Conservative:** a consumer fixture, not a conversion run in the read-only cache directory. |
| C0-11 (+R8) | BI9:114-140, 159 | NGE is MANDATORY before the bank, at the union, under pwsh 7, with the seat's numbers: Validated 54; 18 `go.*` deps each `[<closure>, 1.25.0)`; consume 28/28; GPF unchanged; description, release notes and VALIDATION.md head == the ruled caveat. The post line was rewritten to match. |
| C0-12 (+R8) | B:456, 465-473 | **NGF:** `j0-consume.ps1` is added to the parse list for both editions. The new NGF-j0 legs run `dotnet msbuild J0UuidConsumer.csproj -getItem:PackageReference`: with no -p expect `go.github.com.google.uuid`; with `-p:J0UuidPackageId=nugetgo...` expect the nugetgo ID. A miss is a FINDING. |
| C0-13 | B:354-388 | New FXc. `go test -c` builds the repoguard binary outside the worktree. **FXc1:** GOROOT=go1.23.12 and GOMODCACHE=an empty folder → rc≠0 + `no GOROOT at the pinned go1.24.13`. GOMODCACHE must be empty because the H: cache **holds** toolchain@v0.0.1-go1.24.13, contrary to the critique's premise. **FXc2:** GOROOT = an H: copy of go1.24.13 src + VERSION with alignment.golden altered → rc≠0 + exactly 1 STALE FIXTURE naming it. The regex follows the SOURCE: the message reads `STALE FIXTURE at go1.24.13: src/core/go/...`, with the pin first. |
| C0-14 (+R3) | B:505-509, 527-531; H:trxmethods | The result-count probe is replaced by a by-METHOD-NAME read (see R3). |
| C0-15 | M:278-294; RD #16 | New MR4v is a **separate** module with `vendor/keep.txt` and a test reading it. Expect `vend 1` and the module unchanged. **Conservative:** separate, so a vendoring surprise cannot corrupt MR4. That `-mod=vendor` with no requirements is consistent is INFERRED; the oracle leg proves it. The GolibTests link-skip arm is requested from R (RD #16). There is no script change for links. |
| C0-16 | B:316-320, 670-672, 684-692, 748-754; H:deadlock; I9:118-139, 154; LX:146-153, 172, 298 | The deadlock count now reads the console, the `full output:` file and the fresh record's `stderr.go/csharp.text` tails (testConversion.go:7058). On the i7 this covers S, NR and TBS-W. A non-zero count on net/rpc, net/rpc/jsonrpc, NR or TBS-W is a FINDING. The i9 driver has an inline equivalent. The linux tleg and TBS-LOOP add the record stderr count. |
| C0-17 | M:263-275 | MR4c gates rc≠0 plus `hp cmpnames` over `out/m4c`'s two records: TestReadsTheModuleFixture, TestReadsTheParentFixture and TestReadsTheModuleGoMod must read go=pass cs=fail. The ENOENT line count is kept as information. Its build moved to ctlconv (R7's rule, applied to both controls). |
| C0-18 | B:541-555 | CNR reads N from the verdict line (`CNR_EXPECT_N`, default 778, INFERRED) and asserts that none of the 5 new dirs is in the platform-exclusive skip list. A mismatch is a FINDING. |
| C0-19 | I9:40-44; BI9:26 | The i9 driver aborts on any row matching `^(crypto/tls\|crypto/x509\|net)(/\|$)`. **Conservative:** crypto/x509 was added (the brief lists it on the i7). Re-read: the list has 0 such rows and its sha256 is unchanged (057c78c1…). |

## Critique 1: mechanics

| ID | File:lines (draft2) | What changed / NOT APPLIED + why |
|---|---|---|
| C1-1 | M:378-384; B:603-607 | Same fix as C0-1: exit 4, and ABORT/END/DEADLINE lines in the MOD stamp. |
| C1-2 | B:721-723, 748-755 | TBS-W now runs BEFORE `tleg runtime 150m all`, so the bank reading is the last record left in the tree. |
| C1-3 | B:705-711 | After the TE/HS capture, `restore_paths S` runs, then `purge after-sweep`. TE/HS non-zero results are FINDINGs. |
| C1-4 | B:82-83; M:30 | PRE refuses an existing `tL-logs/SUMMARY.txt` or `mod-logs/SUMMARY.txt` before writing anything. M refuses its own. |
| C1-5 | B:113-125; M:51-59; E:34-43 | The GOROOT value is checked before the export: no `/`, drive-rooted, ending `\sdk\go1.24.13`, == `<cygpath -w $HOME>\sdk\go1.24.13`, VERSION go1.24.13, and == `env -u GOROOT go.exe env GOROOT`. After the export, `go version` must match. **Deviation:** the critique's literal user-profile path is NOT written into the scripts, because they are pushed to claude/coord-handover and the security rule bars username paths. It is derived from $HOME instead. The draft's self-compare was removed (B:146). |
| C1-6 | B:279-286; M:117-120 | convbuild runs `rm -f` on the exe first, reads `go build`'s rc, and aborts on rc≠0 or a missing exe. |
| C1-7 | B:293-307, 442-446, 603, 612, 627, 668, 688 | `capped` wraps a leg in `/usr/bin/timeout -k 120 <cap>`. MOD has a 4h cap; HOP, SPB, every S row and NR have `SWEEP_ROW_CAP` (4h); NV51/NV7/NVR have 30m. When a cap fires, `timeout_stop` LISTS candidate orphans: command lines naming the tL worktree, a pattern assembled inside PowerShell so it cannot match the querying shell, with its own PID excluded. Then it exits 5 without a purge. **Conservative:** nothing is killed automatically, because floor 5 bars that and dotnet10's exe path is shared. COORD kills by PID. PRE also requires `/usr/bin/timeout`. |
| C1-8 | B:341, 350; M:109, 216, 248, 288, 309 | Explicit `-timeout` on every `go test`: CB 20m, FX 15m, the oracles 10m (CM already had 30m). FXc passes `-test.timeout 10m`. |
| C1-9 | B:84-91 | PRE refuses a launch from tL, hnd, any `wf/draft*` or the main checkout. It also requires the 8 companion files and the seat list in the run copy. |
| C1-10 | B:76, 95-99; M:22-38 | An atomic `mkdir` lock at `coord-scratch/tL/.battery.lock`, released by the EXIT trap. M takes the same lock when run standalone, and refuses unless `IN_BATTERY=1` when the battery holds it. The battery passes `IN_BATTERY=1`. |
| C1-11 | B:271-282; LX:255-265 | `restore_paths` takes `git diff --name-only -z HEAD` into `--pathspec-from-file --pathspec-file-nul`, reads the restore rc, and makes leftover tracked changes a FINDING. The same parse fix is applied to LX's LB restore, which aborts if the restore leaves changes. |
| C1-12 | B:110, 410, 412, 416, 437, 444, 459, 463 | Every leg that drops dotnet10 from PATH now uses `GOPIN_PATH` (pinned go first): G1, G2, SY, ST7, NV7, NGa7, NGb7. |
| C1-13 | B:482-491, 510-521 | TR and each GT run are split into a build leg and a test leg. A failed build stamps `BUILD FAILED` (error count) and the test leg is NOT MEASURED. The trx logger argument is now passed directly instead of through `bash -c`. |
| C1-14 | BLX:118-130 | K's gated removal is restored, with `git -C "$W-ctl" checkout -- src/core/<pkg>` before the `n=…; [ "$n" = 0 ] && worktree remove --force` check, plus STOP-and-post. |
| C1-15 | E:78-80 | The native exe gets `-go2cspath "$(cygpath -w "$R/src")"` explicitly. |

## COORD rulings

| ID | File:lines (draft2) | What changed / NOT APPLIED + why |
|---|---|---|
| R1 | F:16-24, 39-40, 198-202; B:22-24; RD | The two OutputComparisonTests rows go in the fixup at sorted positions, re-read at 6960c8071f: ChannelReceiveFromClosed < ChannelReceiveFromNil < ChannelRendezvous < ChannelSendToNil < ClearBuiltinShadow. CRLF is kept (the file is `w/crlf`, `eol=crlf`). Any `OCT_REPAIR` other than `fixup` is refused. The commit message item (4) opens `Seat: g-deadlock-checkdead (0377dbff39)`. The csproj refresh of the two files is unchanged. The subject stays `fixup: TRAIN L`. |
| R2 | B:13-17, 158-187, 449-462, 787; F:20-23, 76-92, 107, 178, 194; BI9:10, 132-135; BLX:10; tL-ng-parse.ps1:6-7 | **PRE** (B and F) accepts 11 seat merges, or 12 when the extra merge is HEAD^ (B) / HEAD (F) whose ^2 is on `refs/remotes/origin/claude/i9-nugetgo-pack`, descends from 1f99ee7e9f, is not it, and comes after the seat. The ref must be fetched before launch; the scripts never fetch. A `seats-effective.txt` adds the fix to precheck (counted once by the ancestor rule) and to F's new-csproj derivation. **NGb51** is GATED: its expected-red wording is removed, and NONZERO LEGS lists it as a finding. |
| R3 | B:505-509, 527-531, 786; H:trxmethods | Each GT TRX is read by method name in PointerTokenUniquenessTests, regex `FieldOfElement\|ElementField`. The output is a `TRXM … FOUND n: names=outcomes` or `NOT-FOUND` line, plus matches in other classes as information. Stated in the GT stamp and END; not gated. Self-tested on a synthetic TRX. |
| R4 | B:741-755 (unchanged route); RD #4 | runtime and runtime/pprof stay direct `-tests` tleg legs (only TBS-W's order changed, C1-2). |
| R5 | B:73, 92, 233-249, 729; M:24, 31, 79-86; B:603 | `leg()` (and so `capped`) and `tleg()` refuse to START a leg after `$DEADLINE` (HH:MM, default 17:30, validated): stamp `DEADLINE STOP before <leg>`, restore any tracked rewrites (patch kept, V1), purge in no-abort mode, exit 9. M has the same check: it removes the converter bin and control builds, then exits 9. The battery passes DEADLINE to M. |
| R6 | M:73, 133-160, 162-196 | MR2 and JWT run only when their exact module paths exist under `go env GOMODCACHE`, read with `[ -e ]` per path (no walk). Otherwise they stamp `NOT MEASURED -- module absent (<path>)`: neither a pass nor a fail. GOPROXY=off and GOSUMDB=off, never a download. **Note:** the battery does not export GOMODCACHE. `go env -w` resolves it to H:\go\pkg\mod, and that value is stamped in B's PRE and M's PRE. **Conservative:** exporting it would change the fixture guard's toolchain lookup in leg C. |
| R7 | M:22, 121-127, 265, 325 | Control converters (M3 for MR4c, M4 for MR6c) are built from `git archive` into `/h/go2cs-tmp-coord/coord-scratch/tL/mod-ctl-build/<name>`, each with its own output root (`$OUT/m4c`, `$OUT/m6c`). They are removed at END and at a deadline stop. |
| R8 | BI9:114-140, 159; B:465-473 | NGE is mandatory in the i9 brief with the seat's numbers (C0-11). The J0 consumer checks are in the NG/NGF group as legs NGF-j0default and NGF-j0nugetgo, with j0-consume.ps1 in the NGF parse list (C0-12). |

## Verifier fixes (DRAFT 2, second pass)

Line numbers in every row above were remapped to the post-fix B (shifts: +1 from 97, +4..+6 through deadline_check, +9 after purge, +12 after FXc2-prep).
Third pass (V4-V6): B refs at or after line 620 were remapped again (+6 from the SPB pre-check, +9 after SPB, +12 after the PB pre-check, +13 after PB); C0-5, C0-6 and C1-3 were re-read against the text and corrected by one line. LX refs after line 232 moved +3 (C0-3, C0-16, C1-11).

| ID | File:lines (draft2) | What changed |
|---|---|---|
| V1 | B:234-246, 259-271 | **deadline_check**: if `git status --porcelain \| grep -vc '^??'` is non-zero, it runs `restore_paths DEADLINE` first (saves `DEADLINE-rewrites.patch` + the NUL path list, restores exactly those paths, a leftover is a FINDING). It removes `$FXC` if set, then calls `purge "deadline-stop" noabort`. **purge** takes an optional `noabort` 2nd arg: an incomplete purge or a surviving ` D` is stamped and listed as FINDING `PURGE-deadline-stop`, and purge returns, so a deadline stop is always exit 9, never exit 3. Every other purge call is unchanged (K verbatim, exit 3). **Note:** a stop between the T legs restores the T bank material too. It survives in the patch; COORD re-applies it if it is wanted. |
| V2 | B:97-98, 360, 368-372, 385 | The FXc2 GOROOT copy is now leg `FXc2-prep` (log `fixture-currency-c2-prep.log`; paths passed as env SRC/DST, no nested quoting), so it gets the R5 deadline check and the 30G/8G preflight. `fxcp=$LEG_RC`, and a failed copy still yields `FINDING FXc2 NOT MEASURED`. `FXC=''` is declared before the EXIT trap. The trap `cleanup` removes `$FXC` on ANY exit (deadline 9, disk 3, timeout 5), deadline_check removes it too, and the normal path clears `FXC=''` after its `rm -rf`. |
| V3 | RD:19 | The F2 row's fixup cell now reads `R1: OCT_REPAIR=fixup only -- 2 rows at UpdateTestTargets' sorted positions, CRLF kept, 6/0, attributed to g-deadlock-checkdead.` The withdrawn `OCT_REPAIR=seat` option is gone from the table (the R1 note at RD:23 is unchanged). |
| V4 | B:620-636, 645-656; LX:232-241; RD:55, 69, 105-109 | **Stale binlog before the poll.** Immediately before each `poll_start` (SPB and PB) the battery records `pre=PRESENT\|absent` for `src/core/cmp/bin/tests/publish.binlog`, then runs `rm -f` on it, so SEEN can only come from that leg's own publish (a red SPB KEEPS its binlog, and PB's poll would otherwise read it before PB's publish starts). `pre=` is in the SPB and PB stamps and FINDING lines, and `pre=PRESENT` is its own FINDING. **Conservative:** the same pre-check is applied to linux LPB (stamped, since LX has no FINDING list). The deleted file is gitignored build output under bin/. |
| V5 | B:621-625, 631-636; RD:105-109 | **SPB evidence copies limited to SPB's own run.** `ls -1 scratchpad/sweep-oracle-flake` is saved (sorted) just before the leg and again after it. `comm -13` gives the stamp folders SPB created, and only `<new>/cmp/*/publish.binlog` is counted. **Deviation:** this uses a name-set difference, not `find -newer <marker>`, because an old folder's mtime moves when anything is written into it. The stamp lists the new folders (count + first 120 chars). RD #18 no longer calls the glob INFERRED: the verifier confirmed it against run-validated-sweep.ps1:557/1087. |
| V6 | RD:63; B:594 | **MR4c doc text.** The README MOD cell drops the stale `MR4c rc≠0 with ≥3 ENOENT` and keeps only `MR4c rc≠0 + the 3 tests go=pass cs=fail BY NAME`. The battery's MOD header comment now reads `rc!=0 + the 3 tests go=pass cs=fail by name, ENOENT count information only`, matching M:263-275. Comment-only in B. |

## Other edits

- **Critique 0's run-list note:** tL-i9-shard.sh now has a row-name guard (C0-19). The canaries running on both boxes is RD #19, to state in the GO.
- **H:** usage header plus 4 new pure-reader subcommands (`trxmethods`, `deadlock`, `cmpnames`, `lockcheck`), self-tested on synthetic inputs in a temporary folder under draft2 that was then removed.
- **I9 and LX headers** note the DRAFT 2 deltas. tL-i9-shard.txt is byte-identical (sha256 057c78c1f9ad…).
- **RD:** DRAFT 2 banner, new legs in the tables, rulings marked on open questions 1-4, new items 16-19.

## Syntax checks (`bash -n`, run in draft2 after the last edit)

| File | rc |
|---|---|
| tL-battery.sh | 0 |
| tL-emitcheck.sh | 0 |
| tL-fixup.sh | 0 |
| tL-i9-shard.sh | 0 |
| tL-i9-te.sh | 0 |
| tL-linux-legs.sh | 0 |
| tL-modules-legs.sh | 0 |

`python -m py_compile tL-helpers.py` passed (the cache folder was removed). Every edited file is LF; no CR was introduced.

Nothing was built, run or converted. No repository worktree was written. The only reads were read-only git, grep and ls on exact module-cache paths.
