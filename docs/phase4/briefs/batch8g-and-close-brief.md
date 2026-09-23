You are an i7 sub-agent for the go2cs fleet coordinator. This brief has TWO PARTS that run IN SEQUENCE with a COORD CHECKPOINT between them. Each part is dispatched by its own prompt, and you run only the part your prompt names. **PART A** applies BATCH 8g, the last pre-close batch: five refs merged onto the version branch in a LINKED worktree ON A BRANCH, crypto/tls's 1.24.13 test artifacts re-emitted convert-only, one battery, the header derived, a report, then STOP. **PART B** is THE H10 CLOSE SEAT. It is dispatched only after COORD has stamped 8g and cleared the checkpoint, and after COORD has filled Part B's rulings. It moves net/http to the candidates, demotes go/internal/srcimporter's linux annotation by name, sweeps the core-refs table, runs the hop's final seeded `-stdlib` reconvert with its two-seeded corpus diff, runs the gates, derives the figures, regenerates the index, runs the closing checks and reports. **DO NOT PUSH** in either part. Read `CLAUDE.md` first (floors 1-9 and 11-16), then `.claude/rules/harness-gates.md`, `.claude/rules/corpus.md` and `.claude/skills/validation-bank/SKILL.md`. For PART B also read `.claude/skills/corpus-reconvert/SKILL.md`, `.claude/rules/converter.md`, and these sections of `docs/GoCorpusMigration.md`: H5 (:523-572, and its amendments at :574, :682, :753, :813 and :1051), H7 (:1348-1450, both amendments), H10 (:2190-2930, every dated amendment) and §4 (:3307-3330). THE RUNBOOK LEADS ON PROCEDURE: where this brief and the runbook disagree, STOP and quote both. Never modify the main checkout or any other worktree under the coordinator's temp root. The coordinator's prompt supplies the absolute values for every <placeholder>.

## Why
H10 (re-validate every roster row at go1.24.13) closes only when every banked row re-banks at the new release or is demoted to a candidate BY NAME, with the header recomputed by the guard (runbook :2244-2255). At the 8f STAMP d1a0d314d5, 217 of 219 rows' first proof page reads Go 1.24.13. The two exceptions are crypto/tls and net/http. crypto/tls banks in 8g: R's raised-wall bank `claude/r-tls-bank` 3ec2c9ff39 (owner ruling, LEDGER 2026-09-23 11:59; accepted 12:36), G's linux annotation on top of it `claude/g-tls-linux-bank` b57703b1cd (12:36 "so 8g carries it"; ruled 12:56), and the 1.24.13 test artifacts R did not bank, which this battery re-emits convert-only (12:36). net/http moves to the candidates at the close (owner ruling, LEDGER 2026-09-22 17:33). A row never carries two Go versions (LEDGER 2026-09-23 12:10), so go/internal/srcimporter's 1.23.12 linux annotation is demoted by name, because Go's own `TestImportStdLib` fails on the linux host. The hop's final `-stdlib` reconvert is named debt owed BEFORE the close (LEDGER 2026-09-22 11:10 ruling 2; 08:18; 14:49). The runbook's H10 section carries no close checklist. This brief assembles one from the runbook's gates, the ledger's rulings and STATE DELTA 26. The runbook's own in-stage amendment for the close is COORD's item (c5), and it is a precondition of the close STAMP, checked at B12.10.

## Rules for BOTH parts
- **Pins, GPG, census and sibling check**, inlined from batch 8c's brief (`origin/claude/coord-handover:docs/phase4/briefs/batch8c-brief.md`, blob ad20c24af5; that path does NOT resolve in a version-branch worktree). Read each pin back by OUTPUT:
  - the machine-scope GOROOT is a DEAD 1.23.1 pin, so unset it; the 1.24.13 SDK's `bin` first on PATH; `go version` = go1.24.13; `go env GOROOT` prints the BACKSLASH spelling, and that spelling is the pin (floor 6); GOTOOLCHAIN=local; CGO_ENABLED=0; GOFLAGS empty.
  - the dotnet10 root as DOTNET_ROOT and first on PATH for every `dotnet`; pwsh 7 (net8 apphost) launched with DOTNET_ROOT UNSET and the dotnet10 root OFF the parent PATH, with the pins set inside via `$env:`, and `Get-Command git` asserted inside before CNR.
  - an explicit `-timeout` on every `go test`, and `-count=1` on every guard.
  - exit codes captured on the NEXT line, before any pipe (floor 7).
  - the sibling check before starting, before every converting leg and before every row: another go2cs.exe outside this worktree => STOP. The harness's own dotnet-hosted pwsh shells are not siblings (read the COMMAND LINE, runbook :2567). NEVER kill by name (floor 5); a stray process is a STOP, never a kill.
  - GPG: probe before every `-S` with `echo test | <gpg4win-gpg> --batch --pinentry-mode error --clearsign -u 941694536F21BAFF` (rc 0 = warm). Cold => re-probe every 60 s for up to 30 min, then STOP. Never bypass, never kill gpg-agent, never run a non-batch gpg.
  - the census: the main checkout's `.claude/coord-scripts/coord-identifier-census.sh` (master 20eb0af70e), `converted` on staged converted test sources and fixtures, `entry` on pages, the roster, the index and your report.
- **SERIAL, ALWAYS.** Every leg runs to completion before the next starts. A backgrounded run is awaited to its exit, and its rc is read, before anything else launches. Before every converting leg (Part A's re-emission, both CNRs, B3, each B8 arm, B9(e), each B9(g) row), the box's go2cs.exe count is 0: a count by image path, never a kill (floors 1, 5). Named cases: Part A's re-emission completes before (a); Part A's CNR completes before anything else converts; all of B2 completes before B3; B9(b) completes before B9(e). The "ONE battery on this box" rule governs batteries; this rule governs legs.
- **STAGING.** Every commit is staged by explicit path, never `git add -A` or `git add .` (floor 8). After EVERY purge, restore, clean or pipeline run, print `git status --porcelain` UNFILTERED first (floor 16), then assert that `git status --porcelain | grep '^ D'` prints nothing. A deletion this brief admits is staged with `git rm <path>` by name, so it reads `D ` and never ` D`.
- **DISK.** Read free disk (>= 25 GB) before the battery, before each conversion, before each build leg and before every row. Purge bin/obj/Generated after each build leg and each row, then run the `^ D` assert.

# PART A -- BATCH 8g

## Fixed facts (assert each by `git ls-remote origin`; STOP on any mismatch)
- BASE: `claude/version-go1.24.13` = `d1a0d314d5` (the batch-8f STAMP).
- Worktree: `git worktree add -b i7/h10-batch8g-<mmddHHMM> <coord-tmp>/ab8g d1a0d314d5`. Lock `<coord-tmp>/ab8g.lock` + PID. Scratch `<scratchpad>/ab8g/`.
- **crypto/tls is NOT RUN on this host.** Its bank is R's raised-wall run, and the i7 cannot reach the full-host BoGo state at the standard wall. Exactly ONE crypto/tls operation happens here: step (0)'s convert-only re-emission (LEDGER 12:36: "convert-only, deterministic emission, no re-run"). It builds no C# and runs no test. Step (c) builds the test project and never runs it. Step (d) checks the bank statically.
- **`run-h10-recon.ps1` is invoked ONLY with `-SelfTest` in Part A.** Any other invocation, `-DryRun` included, runs a row under `-test-action all` and publishes a page, the index and a README into the worktree (`run-h10-recon.ps1:251-254`).
- THE RULINGS THIS PART CONSUMES. The prompt states each one. A ruling missing from the prompt => STOP before step (0).
  - **R-A1**, the crypto/tls deadline floor (H10 step 5, re-checked against the full-host run; LEDGER 2026-09-22 18:02 item 3). The readings, by what each is:
    - R's BANK row: wall 1,761 s under `-TestTimeout 90m` with GOFLAGS=-timeout=40m (3ec2c9ff39's commit message; LEDGER 12:36). That is 0.98x of the 30m floor.
    - R's 2026-09-22 EVIDENCE row: 2,216 s at d095fe8108 (claude/r-tls-bogo-evidence 7462befde0, rows.tsv), where nothing was banked (LEDGER 09-22 21:55). Evidence only.
    - AZ1's full-host STANDARD-wall row: 467 s = 0.26x (claude/az1-tls-evidence fad839a224, rows.tsv), DIVERGED on Go-side flakes.
    - The table's rule is to raise when wall >= 0.75 x floor, and never to lower (LEDGER 09-22 18:02). By the bank row, it says RAISE. 60m puts 1,761 s at 0.49x and 2,216 s at 0.62x.
    - HOLD at 30m is legal ONLY with COORD's ruling that the standard-wall reading (AZ1, 467 s) governs a raised-wall row's floor. The prompt must then name that ruling.
    - The line's exact text comes from the prompt.
  - **R-A2**, the crypto/tls row's linux fourth-state sentence (LEDGER 12:56 RULING: "the batch adds ONE dated sentence to the row naming the linux fourth state (Go's shim loopback refusal on the WSL arm, unattributed)"). The prompt gives the text, or says `default`. The default text is: `On linux (the WSL arm, unprivileged; G, b57703b1cd, 2026-09-23) the host is a fourth, oracle-broken state: Go's own BoGo run fails there, its shim refusing loopback connections to the runner (769 of 3,418 cases), so TestBogoSuite reads fail/fail matched under the host-limit signature; the cause is not attributed.` Place it in the crypto/tls row's prose, immediately before the ` · linux:` annotation. Present tense.

## THE REFS, merged `--no-ff -S` IN THIS ORDER
Assert each by ls-remote. Run the additive-blob proof per ref. `git diff --diff-filter=D d1a0d314d5 HEAD` must name no file other than a generated test artifact that step (0) measured removed, admitted by name.
1. `claude/g-sync-atomic-floor` = `48e1e4d245`: one unsigned commit on fc6269b0bf. It changes `src/run-validated-sweep.ps1` (+9/-2): `$longTimeouts['sync/atomic']` goes 90m -> 150m on :927 with a dated provenance block, and the re-check table's sync/atomic line is annotated (LEDGER 2026-09-23 11:51, 11:59). That file is unchanged between fc6269b0bf and d1a0d314d5, so the merge is predicted CLEAN. ALLOWED: that file's :924-990 region.
2. `claude/r-recg-chancore-design` = `bb69175480`: one signed docs-only commit on fc6269b0bf. It adds `docs/phase4/DESIGN-channels.md` §7 (+136), the REC-G design record (LEDGER 2026-09-23 11:43). Predicted CLEAN. ALLOWED: that file.
3. `claude/g-net-linux-bank` = `2b65f11474`: one unsigned commit on d1a0d314d5. It changes the roster (+2/-2): net's annotation `linux: 577 + 2` -> `linux: 581 + 3` (roster :391), and the header's linux line (:167) 48,692 / 162 -> 48,696 / 163 (LEDGER 2026-09-23 12:10). Predicted CLEAN. ALLOWED: roster :391 and :167.
4. `claude/r-tls-bank` = `3ec2c9ff39`: R's crypto/tls RAISED-WALL BANK, ACCEPTED at LEDGER 2026-09-23 12:36. It is two signed commits (%G? `U`) that fast-forward d1a0d314d5: `aaa61ef620`, the NOTES removal, and `3ec2c9ff39`, the bank. `git diff --stat d1a0d314d5 3ec2c9ff39` reads 6 files. Score each item as present or absent:
   - the page `docs/validation/current/crypto.tls.md`: VALIDATED `**4759 matched · 1 disclosed**`, Go 1.24.13, TestCertCache the one disclosure, provenance line `*Validated 2026-09-23 · converter `4c5b0a3b7`*`;
   - the README Tests badge 4759/4760 (`src/core/crypto/tls/README.md`, that line only);
   - the roster row (:273): cells `| 3643 | 1 |` -> `| 4759 | 1 |`; the prose 3,243 -> 3,419 BoGo sub-verdicts; the expired-fixture caveat withdrawn; one dated sentence naming the raised wall ("raised to 40m on both sides (GOFLAGS)") and AZ1's 213 s standard-wall C# measurement;
   - the manifest with the notes block removed: blob `bbc8268e00`, which equals `4a960d31a3:src/core/crypto/tls/go2cs_test_disclosures.json`;
   - `src/run-validated-sweep.ps1` :626-647: the derivation comment re-stated at both versions, and `$capabilityConditionalBlocks['crypto/tls'].BlockSize` 3243 -> 3419 (:647);
   - the roster HEADER (:137) and `docs/README.md` NEWS, 57,203 -> 58,319 matching, "as the guard derives them".

   ALLOWED paths: that page; `docs/ValidatedTestPackages.md` :273 and :137; `docs/README.md` (the NEWS figure); `src/core/crypto/tls/README.md` (the Tests badge line); `src/core/crypto/tls/go2cs_test_disclosures.json`; `src/run-validated-sweep.ps1` :626-647.
   - **The header and NEWS hunks are ADMITTED as guard-derived carriers.** COORD accepted them at 12:36, which is an exception to runbook :2777-2783 ("never the roster header"). COORD records that exception at the 8g STAMP and folds it into c5. They are never trusted: (f) overwrites them with the guard's output, and both are quoted. Refs 3 and 5 carry the header's linux line the same way.
   - A production `.cs` in this ref => STOP. A re-bank ref stages TEST artifacts only (LEDGER 2026-09-22 04:01). `docs/validation/index.md` in any ref => STOP (runbook :2797-2800).
   - This ref carries NO test artifacts and NO h10-evidence. R's commit says so: "Not banked: the run's re-emitted tests, tests.csproj and eight testdata recordings (restored by path)". Step (0) supplies them.
5. `claude/g-tls-linux-bank` = `b57703b1cd`: one unsigned commit on 3ec2c9ff39 (LEDGER 12:36, 12:56). It changes the roster (+2/-2): crypto/tls's annotation `linux: 401 + 1` -> `linux: 1341 + 1` (:273), and the header's linux line (:167) 48,692 -> 49,632 matching, 162 disclosed. ALLOWED: roster :273 (the annotation) and :167.
   - **PREDICTED CONFLICT at :167 with ref 3.** Run the guard on the merged rows and write the figure it expects: 49,636 / 163, the union G's commit message states. Quote the resolved hunk. (f) re-derives it.

CONFLICT RULES (as in 8e):
- The roster: take each side's intended ROWS, and let THE GUARD derive the header and the README NEWS. Never hand-choose a figure. Quote every resolved hunk.
- The `$longTimeouts` line (:927), if two refs edit it: THE UNION on that one line, with both provenance blocks kept. R-A1's own edit lands later, as its own commit (h)(3).
- A converter-emitted file: re-emit it, never hand-merge it.
- Anything else => STOP with the hunk quoted.

## (0) THE crypto/tls TEST-ARTIFACT RE-EMISSION (LEDGER 12:36), after the merges and before the battery
1. Build go2cs.exe from the worktree's `src/go2cs` at the merged HEAD under the pin. Assert `go version <exe>` = go1.24.13 and that the MTIME is newer than its sources. Then check go2cs.exe count 0, the sibling check, disk, and `git status --porcelain` empty.
2. Run, from Git Bash, with GOROOT exported exactly as `go env GOROOT` prints it:
   `CGO_ENABLED=0 "<tree>/src/go2cs/go2cs.exe" -tests -test-action convert -go2cspath "<tree>/src" "<GOROOT>\src\crypto\tls" "<tree>/src/core/crypto/tls" > "<scratchpad>/ab8g/tls-reemit.log" 2>&1`, then `rc=$?` on the next line.
   - The OUTPUT DIR is the SECOND positional (floor 3). This mirrors the wrapper's own argv (`Get-RowConverterArgs`), with `convert` in place of `all`.
   - The worktree's `src/core` IS the seed (runbook :2561).
   - `-test-tiered` / `-test-config` do not change what the converter emits (`main.go:260`), and crypto/tls carries no execution pin.
   - rc 0; stderr is non-fatal.
3. CLASSIFY every path `git status --porcelain` names:
   - (T) KEEP the ruled test-artifact set (LEDGER 2026-09-22 04:01): `*_test.cs`, `package_test_info.cs`, `package_info_internal_test.cs`, `package_init_internal_test.cs`, `go2cs_test_host.cs` and `crypto.tls.tests.csproj`. Keep Go's own `testdata/**` byte-equal to GOROOT's (09-22 11:10 ruling 1; R's run moved eight testdata recordings). Take all of this from the row's OWN directory MINUS its sub-package directories, here `src/core/crypto/tls/internal/fips140tls/**` and any `fipsonly/` (runbook :2891-2898).
   - (P) RESTORE every production `.cs` and `src/core/crypto/tls/README.md` with `git checkout -- <path>`, and COUNT them by class (04:01: "the production emission of record is -stdlib's"). R's README badge stands.
   - (X) Restore anything else, name it, and quote it. That includes any `docs/validation/**` write and anything under a sub-package directory.
   - (D) A generated test artifact the run REMOVED is admitted by name and staged with `git rm`. The precedent is i9 s1's `crypto/cipher/package_info_internal_test.cs` (LEDGER 09-22 04:14). Any other deletion => STOP.
4. PREDICTED:
   - `crypto.tls.tests.csproj` names `runtime/internal/math` 0 times and `crypto/internal/mlkem768` 0 times. At d1a0d314d5 it names them at :320 and :291.
   - `example_test.cs` and `fips_test.cs` are ADDED (new `_test.go` files at 1.24.13); `handshake_unix_test.cs` is absent (unix-only).
   - every kept `*_test.cs` maps to a go1.24.13 `_test.go` or is generated;
   - the eight testdata recordings sha256-equal to GOROOT's;
   - no production `.cs` staged.
5. Census `converted` on the staged test sources and testdata. A refusal on Go's own test data is reported, not a stop; repoguard is the gate of record.
6. COMMIT (0), signed, staged by explicit path: "crypto/tls: the go1.24.13 test artifacts re-emitted at the merged tree (convert-only; LEDGER 12:36)". Name the restored production count by class in the message.
7. Then apply R-A2's sentence as COMMIT (1), signed.

## Battery (converter/gen/golib source FROZEN while it runs)
(a) Run `go test ./... -count=1 -timeout 40m` in src/go2cs. Expected GREEN. This covers the manifest loader over crypto/tls's manifest.
   - Then run `go test ./internal/repoguard -run TestCommittedCoreReferencesResolve -count=1 -v -timeout 10m` and quote its summary line.
   - Predicted: `declared 8 · measured 1 · retired 7`. crypto/tls's two rows retire with (0)'s re-emitted tests.csproj. net/http's row stays measured.
   - A reading of `measured 3 · retired 5` means (0) did not land its csproj. That is a STOP-finding.
   - Do NOT delete the retired rows. The sweep to zero is the close seat's (ruling 11, LEDGER 2026-09-22 12:15/12:21).
(b) CNR from a per-run copy: `NO REGRESSION`, with CHANGED predicted EMPTY (no converter change). No golib, gen or production C# change is aboard, so GolibTests, GenTests, the stdlib build and the behavioral suite are NOT owed. Say so.
(c) crypto/tls's re-emitted TEST PROJECT BUILDS at the merged tree. The testing host moved in 8f (HostDescriptorLimit.cs) after R's measured tree.
   - Run `dotnet build src/core/crypto/tls/crypto.tls.tests.csproj -c Release` with the dotnet10 pins: 0 errors. This is a BUILD only, never a run.
   - Purge that closure's bin/obj afterwards and count them.
   - If the build needs an ignored pipeline artifact, report the error. Do not run the pipeline to make one.
(d) THE BANK'S CONSISTENCY, WITHOUT A RE-RUN:
   1. **The page headline and provenance.** Quote the `**N matched · M disclosed** -- Go <ver>` line and the `*Validated <date> · converter <sha>*` line (predicted `4c5b0a3b7`).
      - Run `git cat-file -e 4c5b0a3b7^{commit}`: predicted to FAIL. It is R's pre-rebase local tree, not an object on origin.
      - R's commit message records "Measured at 4c5b0a3b7 = fc6269b0bf (batch 8e) + the notes commit 4a960d31a3". On origin, `4a960d31a3^{tree}` = `2827307647`.
      - If the prompt supplies R's `git rev-parse 4c5b0a3b7^{tree}` (checkpoint pre-stamp item 1), compare it with 2827307647 and report EQUAL or NOT.
      - There is no bank evidence ref and no rows.tsv to compare. REPORT this; it is not a stop. COORD resolves it before the stamp.
   2. **The page's `## Verdicts` table, read with the sweep's OWN reader.** Port its two regexes verbatim from `run-validated-sweep.ps1:579-585`: the section-heading test on :583 (`^##\s+Verdicts\b`) and the row pattern on :584, which captures the first code-span cell of a table row. The same reader is repeated at :680 and :719. Check three things:
      - the total names == Tests + Disclosed (predicted 4760);
      - the names equal to `TestBogoSuite` or starting `TestBogoSuite/` == the merged tree's registered BlockSize (predicted 3419). Both absorption readers refuse on exactly this inequality (`_roster.ps1:1198-1204`, `:1478-1484`);
      - Tests - BlockSize == 1340, G's reduced-host non-BoGo core (LEDGER 2026-09-22 16:03: "1340 + 2 holds").
   3. **PLANT (floor 13).** Run the same reader over a scratch copy of the page with one `TestBogoSuite/` row deleted. It must print 3418 != 3419. The committed page must be untouched (`git diff --quiet`).
   4. **The registration.** `BlockSize = 3419` appears exactly once, and the registration no longer reads 3243. Predicted: CARRIED by ref 4. If not, cut it here as its own signed commit: that one line plus a dated comment naming R's page as the derivation. The same-batch rule is LEDGER 2026-09-22 21:09 item 3.
   5. **The live absorption rules** (Test-CapabilityAbsentDelta / Test-HostLimitDelta) are exercised by the guard's own synthetic arm (`check-roster-format.ps1` section 1b2, TestFakeSuite). They run live only at the §6 full-roster sweep. The validation-bank rule that a banking merge owes `run-validated-sweep.ps1 -Filter <pkg>` at the merge result (SKILL :33) is DEFERRED BY NAME to that sweep, on the owner's raised-wall ruling. crypto/tls is not run here. Say so.
   6. **The manifest.** Its blob equals `4a960d31a3:src/core/crypto/tls/go2cs_test_disclosures.json` (bbc8268e00). Quote any further edit.
   7. **The raised-wall record.**
      - In the crypto/tls ROW, grep separately: `GOFLAGS` (predicted 1), `40m` (predicted 1) and `213` (predicted 1). The literal `GOFLAGS=-timeout=40m` does not occur there; the row reads "raised to 40m on both sides (GOFLAGS)".
      - In the PAGE, grep `GOFLAGS|40m|timeout`: predicted 0. The page records no wall, and the comparison JSON's environment block does not either (LEDGER 2026-09-22 21:55).
      - REPORT both. COORD's pre-stamp ruling (checkpoint item 2) decides where the pin lives.
   8. **Stale-figure census.** Run `git grep -nE '3,?24[23]\b'` over the merged tree and list every hit with its class.
      - The REGISTRATION hit must be gone.
      - The crypto/tls ROW prose must read 3,419 and 1340, and must not present the expired-fixture caveat as current. A stale sentence is a finding, fixed by COORD by hand as in 8f, not a stop.
      - Comments and manifest prose are REPORTED for H12, never edited here: the `hostConditional` text, the `_roster.ps1` / `check-roster-format.ps1` / converter comments, the sweep's own "(3,243 = ... at go1.23.12)", and roster :71.
(e) THE FLOOR TABLE.
   1. Run a per-run copy of `src/run-h10-recon.ps1 -SelfTest`. Pass the REAL `-Tree <tree>`: a per-run copy finds the tree's `run-validated-sweep.ps1` and `_roster.ps1` only through `-Tree` (`Resolve-InstrumentSource`, :645-654), so a dummy tree reads CANNOT RUN. Give the other five mandatory parameters dummy values; the script's own note at :238-244 says -SelfTest invents them and never uses them. Expected: `SELF-TEST PASSED`. Its deadline cases are LITERALS (:727-733), so it does NOT read the tree's floors.
   2. Apply R-A1's line as its own signed commit: a HOLD line in the re-check table, or a RAISE on :927 plus its table line.
   3. THEN derive the floors READ-ONLY from the committed tree with the wrapper's own reader (`run-h10-recon.ps1:865-883`). Take the text between `$longTimeouts\s*=\s*@\{` and the first `}` in `src/run-validated-sweep.ps1`, and match `'([^']+)'\s*=\s*'([^']+)'`.
      - Print the entry count (predicted 12; the wrapper refuses below 5), sync/atomic (predicted 150m) and crypto/tls (per R-A1).
      - PLANT: run the same reader on a scratch copy with sync/atomic set to `151m`. It must print 151m. The tree is untouched.
   4. Then run `python docs/phase4/hopA-inputs/shardmap.py --timings docs/phase4/hopA-inputs/recon-basis.tsv`: exit 0. Its reserved-set extractor is brace-matched and count-checked; refs 1 and R-A1 change values, not the count. Quote the CLOSED line (last recorded: 214 + 14 + 2 = 230, runbook :2353-2363).
(f) THE GUARD, run from `src/` under pwsh 7: every section green (`0 of N`). The 'ledger scope' arm is host-dependent: red on R's box under PS 5.1, green on G's and on the i7 under pwsh 7 (LEDGER 12:36, 12:56). A red there under pwsh 7 on the i7 is a finding.
   - Write the HEADER and the README NEWS (`docs/README.md`) EXACTLY as the guard derives them, overwriting the carried hunks of refs 3, 4 and 5. Quote each carried hunk beside the derived one. PREDICTED:
     - 219 / 230 = 95.2%, with 58,319 matching (57,203 + 1,116) and 283 disclosed. R's carried figure predicts an unchanged write;
     - 219 / 224 = 97.8%;
     - LINUX 188 / 217, with 49,636 matching (48,692 + 4 + 940) and 163 disclosed;
     - 2f: 218 checked, 1 older (net/http), 0 unread;
     - 2g: 218 rows at the pin, 1 older, 0 refused;
     - 2b2: 225 = 219 + 4 + 2.
   - Quote the lines. No row carries two Go versions after this batch: crypto/tls's page and its linux annotation are both 1.24.13.
(g) THE INDEX: self-test, dry run, then `--write` only if it changes. Predicted unchanged: 219 rows and 233 pages.
(h) COMMITS on the branch, each signed and each staged by explicit path:
   - the five merges;
   - (0) crypto/tls's re-emitted test artifacts;
   - (1) R-A2's linux sentence;
   - (2) BlockSize, only if (d)4 cut it;
   - (3) R-A1's floor line;
   - (4) the header + NEWS (+ the index if it changed).

   Census: `converted` on commit (0)'s staged test sources and testdata; `entry` on the page, the roster, `docs/README.md`, the crypto/tls README, DESIGN-channels.md and your report. A census refusal on Go's own test data is reported, not a stop; repoguard is the gate of record. Then run repoguard `TestNoFleetIdentifiersInTrackedFiles -count=1` with a planted control.

Close-out: restore side effects. `git status --porcelain` must be empty (unfiltered) with no ` D`. go2cs.exe count 0. Lock released. Worktree IN PLACE. NOTHING PUSHED.

## Report (final message), then STOP -- do not start Part B
Include:
- the base and refs as asserted; each merge's SHA and every resolved hunk (the :167 conflict quoted);
- ref 4 scored item by item against its list, and ref 5's annotation;
- (0): the rc, the kept set by class with counts, the restored production count by class, any admitted deletion by name, the csproj grep, and the added `_test.cs` files;
- the (a)-(e) verdicts: the core-refs summary line, the CNR line, the build line, the eight consistency arms with the plant's red, the provenance finding, and the floors with their plant;
- the guard's lines and the header figures written (windows and linux), beside the carried hunks;
- the index line;
- `git log --oneline --first-parent d1a0d314d5..HEAD` with %G?;
- disk;
- everything not verified, stated as such.

# COORD CHECKPOINT (COORD's work, not the sub-agent's)
BEFORE the 8g STAMP:
1. **The page provenance.** `4c5b0a3b7` is not an object on origin. Either R reports `git rev-parse 4c5b0a3b7^{tree}` (equal to `4a960d31a3^{tree}` = 2827307647 resolves it) or R pushes the measured tree or an evidence ref (rows.tsv, the comparison record). Otherwise COORD rules and records the unresolvable provenance SHA on the ledger.
2. **The raised-wall pin's home** (LEDGER 09-22 21:55: "page + registry"). The 11:59 ruling moves the registry half to a post-hop instrument seat (d12). Rule that the roster prose carries the pin at the close, and that the page half joins d12.
3. Review Part A's report.

THEN:
4. Announce, then push the 8g STAMP (floor 9). Write the ledger STAMP line, with its time read from `date`, recording the header-in-lane-ref exception of ref 4. Refresh the record.
5. **THE HOST RULE for net/http** (runbook :2541-2549, "at the same tip before it is classified or demoted"). Either dispatch G's net/http linux re-read at the 8g STAMP (`<G_NETHTTP_LINUX>`), or rule that the 971d919113 seat-tree read (LEDGER 08:51, "fails exactly its 17 synctest leaves+parents") suffices. Do this BEFORE Part B's prompt is cut. It fills R-B3.
6. **THE CLOSE PRECONDITIONS c3, c4, c5.** Each is either carried as a COORD docs ref (`docs/**` only) in `<B_BASE>` or B1, or named in Part B's prompt with the ref that will land it before the close STAMP:
   - c3: the B5 BOARD finding (LEDGER 04:43 item 6 ties it to COORD's H10-close finding);
   - c4: the database/sql TestRawBytesAllocs attribution, owed before the close, with a BOARD finding only if it stays unexplained (O5, LEDGER 03:37);
   - c5: the runbook's in-stage close amendment.

   B12.10 checks all three.
7. **The srcimporter cgo flag.** G's leg read the oracle cgo-OFF (the sweep pins CGO_ENABLED=0 for the whole run, `run-validated-sweep.ps1:159-161`; g-linux-leg README line 5; LEDGER 06:18). The row's 1.23.12 annotation was derived cgo-ON, and the sweep's own comment (:1018-1021) says a session-wide zero "would bring them back short". Go's `srcimporter` runs `go tool cgo` on cgo packages only when cgo is on (`go/internal/srcimporter/srcimporter.go:130-141`, reached only when go/build selected `CgoFiles`). The ORACLE failure may be the cgo state, not the host. Rule R-B11 before B5 lands.
8. Fill Part B's rulings R-B1 through R-B12 below. Each has a stated default, and Part B STOPS on any ruling missing from its prompt. Name `<B_BASE>` and `<REGEN_BASE>`.

# PART B -- THE H10 CLOSE SEAT

## Fixed facts and the rulings this part consumes (assert refs by ls-remote; STOP on a mismatch or a missing ruling)
- BASE `<B_BASE>` = the 8g STAMP, plus any COORD docs refs COORD fast-forwarded.
- Worktree: `git worktree add -b i7/h10-close-<mmddHHMM> <coord-tmp>/abclose <B_BASE>`, a NEW worktree (floor 11), never ab8g. Lock `<coord-tmp>/abclose.lock` + PID. Scratch `<scratchpad>/abclose/`.
- CONTROL worktree (B2 only): `git worktree add --detach <coord-tmp>/abclose-ctl <B_BASE>`, a linked child of the same repository. Remove it at close-out, after its tracked count is asserted (runbook :2579-2581). Assert first that its `--git-dir` != `--git-common-dir` (floor 12).
- Stage root `<stage>` = `<coord-tmp>/abclose-stage`, which must NOT exist yet.
- **CLAUDE.md floors 1 and 2, verbatim, because this part converts:**
  1. **Never run two conversions into one output root**, and never let two overlap on one box. A raced conversion corrupts one file with unresolved lift markers and reads exactly like a converter bug.
  2. **Seed the temp root from `src/core` before any `-stdlib` reconvert.** An unseeded root silently clobbers every hand-owned file with an auto conversion that compiles and is operationally broken.
- The rulings (defaults stated; COORD confirms or replaces each in the prompt):
  - **R-B1**, net/http's close reading: RUN it. The owner's ruling says "TestRegisterErr//a -- the execution pin, applied at the close" (LEDGER 17:33), and no version-branch battery has read net/http since the wrapper started applying pins (8c, 8e: "NOT run net/http").
  - **R-B2**, net/http's re-emitted 1.24.13 TEST artifacts: COMMIT them as candidate evidence. A candidate with artifacts is legal under 2b2, and committing them retires the last core-refs row. The alternative is to keep the 1.23.12 artifacts and amend the core-refs row's comment with COORD's text.
  - **R-B3**, the host rule for net/http (runbook :2541-2549): cite G's linux re-read at the 8g STAMP (`<G_NETHTTP_LINUX>`), or COORD's ruling that the seat-tree reading of LEDGER 2026-09-23 08:51 suffices (checkpoint item 5).
  - **R-B4**, `docs/validation/current/net.http.md`: REMOVE it (`git rm`).
    - The 1.23.12 record survives in the write-once `docs/validation/1.23.12.3/net.http.md`.
    - This is how every candidate sits today: runtime, reflect, runtime/pprof, net/http/pprof and internal/synctest have no current/ page.
    - While the page exists, the badge writer composes GREEN from it (`readmeValidationBadge.go:436-446`: a committed tests.csproj AND a current/ page). The index tool refuses a page no row names, links or excludes (`regen-validation-index.py:206-226`).
    - Runbook :2370 ("No proof file is moved, renamed or created") governs relocated rows. Name it in the commit.
  - **R-B5**, `<REGEN_BASE>` for the two-seeded pair: `54dec61728`, the last corpus-wide regen (H5 step 2, 2026-09-13). The base must predate EVERY converter change whose footprint never landed. A later base would misfile those files as committed drift that neither converter reproduces.
  - **R-B6**, the `-text` pins. Pin EVERY `<EmbeddedResource Include>` payload outside `testdata/` in `.gitattributes`, beside the testdata pin (:157-160), in the overlay commit.
    - Derive the set from every tracked AND emitted csproj. Predicted: `src/core/internal/trace/traceviewer/static/**` (the overlay's two payloads), `src/core/embed/internal/embedtest/concurrency.txt` (read as "Concurrency is not parallelism.\n", `embed_test.cs:79`) and `src/core/crypto/internal/fips140test/acvp_capabilities.json`.
    - `git check-attr text` reads `unspecified` for all three today, and `core.autocrlf=true` checks out CRLF where Go embeds LF.
  - **R-B7**, a flat file the emitter itself moves to per-GOOS folders (`Stale copies removed`, `platformEmit.go:259-295`): `git rm` it in the overlay commit, named, when the overlay adds its per-GOOS copy for ALL THREE targets. Any other absent-in-stage file => STOP.
  - **R-B8**, the derived row set in B9(g): cap 12 rows.
  - **R-B9**, shardmap's closing check after the demotion: append net/http's costed row to `recon-basis.tsv`, following the 2026-09-22 pprof precedent (runbook :2353-2363). The alternative is COORD's text ruling net/http out on the record.
  - **R-B10**, crypto/tls in B9(g). If commit 6 touches crypto/tls's non-test files, the row owes its sweep as the seat's own gate (train-assembly SKILL :211), and the i7 cannot run it. DEFAULT: LIST it, and R runs the raised-wall row at the close head (the 12:36 recipe) before the close STAMP. The alternative is a named deferral to the §6 sweep, in COORD's text.
  - **R-B11**, srcimporter's demotion (checkpoint item 7). DEFAULT: demote as ruled (LEDGER 12:10), with the cgo ON -> OFF change named in the B5 sentence. The alternative is that G reads the oracle cgo-ON first and COORD re-rules.
  - **R-B12**, `src/core/crypto/internal/fips140deps/README.md`: an E4 exclusion row with a committed tests.csproj AND a current/ page (1 matched · 0 disclosed), so both arms compose GREEN (`readmeValidationBadge.go:436-446`). Its committed badge is orange BY HAND (8f6abad087, "restore fips140deps's badge"; LEDGER 09-22 16:31). DEFAULT: restore the committed README after the overlay, by the 8b precedent, as named class (iv) in B8(5). The alternative is a badge-writer fix, a separate converter seat that is never cut here.

## B1 -- refs (merged `--no-ff -S`; ls-remote; the additive-blob proof; STOP on any path outside ALLOWED)
- The COORD docs refs the prompt names (c3, c4, c5, if not already in `<B_BASE>`). ALLOWED: `docs/**` only.
- Then the guard as a CONTROL, quoted. Predicted green, with 2f at 218 checked / 1 older (net/http) and the linux line at 188 / 217 with 49,636 matching and 163 disclosed.

## B2 -- controls BEFORE any close edit (restore after each)
In `abclose` (non-building):
- `shardmap.py --timings recon-basis.tsv`: exit 0; quote CLOSED.
- The core-refs guard `-count=1 -v`: predicted `declared 8 · measured 1 · retired 7`.
- `regen-validation-index.py --selftest` and a dry run: 219 rows; 233 pages = 219 by name + 10 by link + 4 by exclusion.

In `abclose-ctl` ONLY: every PREFLIGHT BUILD runs in a tree that is NOT the leg's (runbook :2568). Build go2cs.exe there under the pin. Serial, per-run copies, each read results-tail FIRST (floor 14):
- `run-validated-sweep.ps1 -Filter hash/maphash` (about 282 s on the i7). This is the CONTROL for the overlay's effect. Predicted: it REDS on `hash/maphash/maphash.cs`'s production drift (the `escapeForHash` body; LEDGER 2026-09-22 11:10 ruling 2). That is UNVERIFIED, so record the word either way.
- GolibTests (Release, windows) CONTROL: 8f read 836 / 15 / 851.
- THE FULL BEHAVIORAL SUITE CONTROL (four phases; a per-run copy of `run-behavioral.ps1`; budget from the harness-gates table, from the top of the range): predicted to fail exactly the base two, FuncLiteralCallerNames and GoroutineWaitState.

Then, in `abclose`, IMMEDIATELY BEFORE B3 (runbook :2566, :2575-2578):
- `git status --porcelain --ignored=matching` over the whole tree, counting `!!` rows.
- Its CONTROL: the same predicate on a never-built tree answers 0.
- Record the BEFORE number. If it is non-zero, clear it (`git clean -ndX`, then `-fdX`), re-census to 0, and assert `^ D` empty.
- Only THEN build `abclose`'s go2cs.exe under the pin. The wrapper refuses a tree without one (`run-h10-recon.ps1:851-853`), and the clean removes it. Assert `go version <exe>` = go1.24.13 and that its MTIME is newer than its sources. Re-census: exactly ONE `!!` row, `src/go2cs/go2cs.exe`.
- A residue that stays non-zero owes the two-row arm of runbook :2628-2634 (for this one-row leg: net/http re-read in a second throwaway worktree at the same tip). STOP and report it.

## B3 -- net/http's CLOSE READING (R-B1)
- Run it through a per-run copy of the tree's `src/run-h10-recon.ps1`, with the WRAPPER's own parameters (:88-131):
  - `-NameList <scratchpad>/abclose/b3/names.txt` (one line: `net/http`);
  - `-Tree <coord-tmp>/abclose`;
  - `-GoRoot <GOROOT>`;
  - `-Out <scratchpad>/abclose/b3/rows.tsv` (a file; its parent exists);
  - `-ExpectTip <git rev-parse HEAD, read after B1>`;
  - `-Scratch <scratchpad>/abclose/b3` (outside any work tree);
  - `-TestConfig Release -TestTimeout 60m`;
  - `-AllowBranch`.
- The worktree is ON A BRANCH, so without `-AllowBranch` the wrapper Denies (:267-273; runbook :2718-2731). With it, the wrapper REPORTS the switch (`tree : ON BRANCH ... permitted by -AllowBranch`). Quote that line.
- The wrapper itself builds the converter argv, `-tests -test-action all <exec> -test-timeout <deadline> -go2cspath <tree>/src <GoDir> <OutDir>`, with OutDir the SECOND positional (floor 3). It applies the ROSTER EXECUTION PIN. Assert the printed `execution: release-tiered (roster pin) -> ... -test-tiered` line and the `deadline: 60m (floor 60m ...)` line.
- Read the results tail FIRST (floor 14).
- PREDICTED:
  - DIVERGED;
  - the divergence set EXACTLY the synctest class, 17 names = 11 synctest.Run infrastructure leaves + 6 synctest aggregation parents. The names come from the i9 recon record (`docs/phase4/hopA-inputs/recon-evidence/i9/net/http/go2cs_test_comparison.json`) and G's pass-3 evidence 1e7709ac7a (classes.txt:45-50):
    - the leaves: TestNewClientServerTest/synctest/{h1,h2,https1}, TestServerShutdownStateNew/{h1,h2}, TestTransportIdleConnRacesRequest/{h1,h2unencrypted}, TestTransportRemovesConnsAfterBroken/{h1,h2} and TestTransportRemovesConnsAfterIdle/{h1,h2};
    - the parents: TestNewClientServerTest, TestNewClientServerTest/synctest, TestServerShutdownStateNew, TestTransportIdleConnRacesRequest, TestTransportRemovesConnsAfterBroken and TestTransportRemovesConnsAfterIdle.

    Linux reads exactly these 17 (LEDGER 08:51). A twelfth leaf is the one named uncertainty (LEDGER 2026-09-22 16:32);
  - `TestRegisterErr` and `TestRegisterErr//a:&http.handler{i:0}` AGREEING under the pin (R's S4 evidence, LEDGER 2026-09-23 11:33). The owner's "7 aggregation parents" (17:33) = these 6 + TestRegisterErr. If the pair still diverges, the set is 19. That is REPORTED, the pair becomes the named class, and it is not a stop;
  - no proof page written, because a DIVERGED run publishes none (`testConversion.go:8611-8617`).
- KEEP the re-emitted test artifacts (R-B2). Keep exactly the ruled test-artifact set of LEDGER 2026-09-22 04:01: `*_test.cs`, `package_test_info.cs`, `package_info_internal_test.cs`, `package_init_*_test.cs`, `go2cs_test_host.cs` and `net.http.tests.csproj`. Keep Go's own `testdata/**` from the row's own directory minus sub-package directories.
- RESTORE every production re-emission, the README and any `docs/validation/**` write, and count them by class.
- DELETIONS: the run may remove a generated artifact it no longer emits (LEDGER 09-22 04:14). The candidates, by a census at d1a0d314d5, are the three committed generated files with no GOROOT `_test.go`: `package_info_internal_test.cs`, `package_init_external_test.cs` and `package_init_internal_test.cs`. A measured removal of one of these is ADMITTED by name (`git rm`) in commit 2. Any other deletion => STOP.
- Report the ADDED set. Every added `*_test.cs` maps to a go1.24.13 `_test.go`: `async_test.go` and `netconn_test.go` are new at 1.24 and absent from the tree.
- Copy the comparison record, the results tail and the wrapper's row TSV to `docs/phase4/h10-evidence/close-net-http/`.

## B4 -- net/http TO THE CANDIDATES (commit 2 = the B3 evidence; commit 3 = this step)
(a) The roster: delete net/http's table row whole. It is the row whose first cell is the linked code span for net/http (roster :392), the shape `_roster.ps1:71` parses.
(b) A DATED SUCCESSOR of `### The 230, derived (go1.24.13, 2026-09-22)` (roster :896-960).
   - Place it ABOVE that block, which stays verbatim as a record. Use present tense, cite ledger lines, and give the date `<CLOSE_DATE>`.
   - It states `218 banked + 6 candidates + 6 exclusion rows = 230`. It restates `212 dispatched + 18 unscheduled = 230` as the leg's record. It gives the csproj census as the NAMED identity `225 tracked tests.csproj = 218 rows + 4 exclusion rows keeping artifacts + 3 rowless candidates (reflect, runtime, net/http)` (LEDGER 2026-09-22 16:31).
   - It carries the six-row candidate table: runtime, reflect, internal/synctest, net/http/pprof, runtime/pprof and net/http. Use PLAIN code spans, never the linked-row shape the parser reads as a roster row (roster :964-967).
   - net/http's cell names:
     - its classes, WORDED FROM B3'S ACTUAL OUTCOME: 11 synctest.Run infrastructure leaves and 6 synctest aggregation parents, plus the TestRegisterErr pair (`TestRegisterErr//a:&http.handler{i:0}` and its parent) either agreeing under the pin or named as diverging;
     - its evidence: B3's path, the owner ruling (LEDGER 17:33) and R's post-hop S4 read, 1387/1387 with all 16 synctest verdicts agreeing (LEDGER 11:33);
     - its execution pin to carry on re-entry, `execution: release-tiered`;
     - its 1.23.12 record in the 1.23.12.3 snapshot;
     - its re-entry path, the post-hop synctest train.
   - Correct the stale 2026-09-22 rows (embedtest, internal/runtime/maps and crypto/sha3 are banked) in the successor only.
(c) The page, per R-B4.
(d) `src/core/net/http/README.md`: set its Tests badge line by hand to the converter's exact orange form, `[![Tests](https://img.shields.io/badge/Tests-not_yet_validated-orange?logo=go)](https://go2cs.net/ValidatedTestPackages.html)`. That is `readmeValidationBadge.go:455-456`, and it is the form runtime's and reflect's READMEs carry. This is a PREDICTION that B8's overlay reproduces the line byte-identically (CR-stripped).
(e) Per R-B9, append net/http's costed row, taken from B3's row TSV: columns read by NAME, `post_s` dropped to the basis's ten, LF only. Regenerate no plan.

Gates:
- shardmap RED-FIRST. With (a) applied and (e) NOT yet applied, in the working tree, it must REFUSE, naming net/http as a LIVE unreached member. This is predicted by a code read, not a run: `shardmap.py:416-417` makes UNSCHEDULED the roster rows without a cost, and net/http is in no basis row. With (e) applied, it exits 0: CLOSED 228 reached + 2 excluded = 230 (dispatched +1, unscheduled -1 against B2).
- Then the guard. Quote its failure set, predicted EXACTLY the header and NEWS figure arms (2, 2e) until B10. Then 2b (N 230, net/http a member), 2b2 `225 = 218 + 4 + 3`, 2b3 (no validated badge on a non-row), 2f (218 checked / 0 older / 0 unread) and 2g (218 / 0 older / 0 refused), all green.

## B5 -- go/internal/srcimporter's linux annotation DEMOTED BY NAME (commit 4; per R-B11)
- In roster :311, remove the `· linux: 7 ·` segment. Keep the row's shape as a row with no annotation.
- Add ONE dated sentence. It names:
  - the ORACLE state: Go's own `TestImportStdLib` fails on the linux host, twice, while C# passes (claude/g-linux-leg-evidence 6836e605ac, rows.tsv);
  - the rule that a row never carries two Go versions (LEDGER 12:10);
  - the cgo state of that oracle read as the evidence records it: cgo OFF (`CGO_ENABLED=0`, g-linux-leg README line 5; LEDGER 06:18; the sweep's whole-run pin, `run-validated-sweep.ps1:159-161`);
  - beside the host fact, the STATE CHANGE: the 1.23.12 annotation was derived cgo-ON (`run-validated-sweep.ps1:1018-1021`), so the failing read differs from the banked one in cgo state as well as release. Neither cause is attributed.
- The windows row stays at 7. The guard's linux line is predicted at 187 of 216, 49,629 matching, 163 disclosed.

## B6 -- THE CORE-REFS TABLE SWEPT TO ZERO (commit 5; done BEFORE the freeze, since it edits src/go2cs test source -- floor 4)
- Before the edit, run `go test ./internal/repoguard -run 'TestCommittedCoreReferencesResolve|TestCoreReferenceScannerFires' -count=1 -v -timeout 10m`. Predicted: `declared 8 · measured 0 · retired 8`, with every RETIRED line quoted by name.
- Delete every row of `staleCoreReferences` (`coreReferencesResolve_test.go:73-82`), and state in the table's comment that the close condition was met.
- After the edit, the same run must read `declared 0 · measured 0`.
- PLANT: in the working tree, add a `$(go2csPath)core/runtime/internal/math/...` ProjectReference to one tracked tests.csproj. The run must fail `UNDECLARED STALE REFERENCE` naming it. Restore (`git diff --quiet`), then green.
- If R-B2 chose not to commit, net/http's row stays with COORD's amended comment and the gate reads `declared 1 · measured 1`.

## B7 -- the freeze
From here to the end of B9, converter/gen/golib source is FROZEN, with ONE named exception: B8(8)'s `go generate .` rewrites `src/go2cs/stdlib-metadata.txt`, an embedded converter asset (`embeddedAssets_test.go:18-24`). It runs after both conversions have completed and before B9 starts, when nothing is running. Every long run goes from a per-run COPY of its script. Before B8 seeds anything, the tree must pass two checks:
- `git status --porcelain` is empty (unfiltered), AND
- `git status --porcelain --ignored=matching -- src/core src/gen docs/validation` prints ZERO `!!` rows. Clear it with `git clean -ndX` then `-fdX` scoped to those paths. Prove the check with a control: plant an ignored file, see it listed as `!!`, then remove it. Then confirm `^ D` is empty.
- The seed is a tracked-only `git archive`, so the filtered reading guards the tree, not the seed. The unfiltered porcelain beside it answers "is it clean".

## B8 -- THE HOP'S FINAL SEEDED `-stdlib` RECONVERT, a deliberate REGEN, with the two-seeded corpus diff (commit 6)
(1) BINARIES.
   - Arm B (tip) is built from the worktree's `src/go2cs` at HEAD.
   - Arm A (base) is built from `git archive <REGEN_BASE> src/go2cs`, extracted into `<stage>/base-src`.
   - Build both with `go build -trimpath -buildvcs=false` under the pin, into `<stage>/bin/A/go2cs.exe` and `<stage>/bin/B/go2cs.exe`. The path must not already exist.
   - For each binary: assert `go version <exe>` = go1.24.13; record its sha256; confirm that its MTIME is newer than its sources; and confirm that its usage output lists `-stdlib -comments -go2cspath -platforms -platform-stage -convert-timeout`.
(2) SEED BOTH ROOTS before EITHER converts, from ONE frozen snapshot.
   - Run `git archive -o <stage>/seed.tar HEAD src/core src/gen src/Directory.Build.props src/version.props docs/validation`, then `rc=$?` on the next line.
   - Extract that FILE into `<stage>/A` and into `<stage>/B` (`tar -xf <stage>/seed.tar -C <stage>/<R>`, then `rc=$?`). Never pipe the archive: a pipe reports only tar's code.
   - Assert that each root's seeded `src/core` `.cs` count equals the tracked count under `core.quotePath=false`. Re-derive it; never carry it. It read 4164 at d1a0d314d5 and moves with 8g's and B3's test artifacts.
   - Assert `<GoStdLibVersion>` 1.24.13 and GoBuildNumber 0 in each seeded version.props.
   - Seed only after commit 5 (B7's freeze), so the seed carries B4's page decision and B6's table by construction. The seeded `docs/validation` then lacks `current/net.http.md`, and the emitted net/http README composes orange.
(3) CONVERT A, then B, SERIALLY.
   - Before each: `go2cs.exe` count 0 (a count, never a kill); disk; `touch <stage>/<R>.run.stamp; sleep 2`.
   - Set `CGO_ENABLED=0`.
   - Command: `"<exe>" -stdlib -comments -go2cspath "<stage>/<R>/src" -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage "<stage>/<R>-stage" -convert-timeout 90m > "<log>" 2>&1`, then `rc=$?` on the next line. Both arms get the SAME `-convert-timeout`.
   - stderr is non-fatal (runbook :753-768). `GOROOT` is spelled exactly as `go env GOROOT` prints it (floor 6).
   - Run each arm BACKGROUNDED and await its exit. A three-target run outlasts a 10-minute foreground tool call.
   - **RETRY RULE** (runbook :542-544, "Delete and re-seed per run"): NEVER convert twice into one root. On any failed, killed or interrupted arm:
     1. delete that root, its `-stage` directory and its sentinel;
     2. re-extract the SAME `seed.tar`;
     3. re-assert the seeded count and version.props;
     4. re-run.

     A non-zero go2cs.exe count is a STOP, never a kill.
(4) CHECKS PER ARM (runbook :775-788). Gating for B; reported for A.
   - rc 0, with the wall posted;
   - 0 NULs in the log;
   - 0 `did not fully type-check`;
   - three `Failed: 0`;
   - the `toolchain: GOROOT` line reads `VERSION go1.24.13`;
   - a non-zero count of `.cs` newer than the sentinel;
   - per-target counts;
   - 0 `^namespace go[.]std`;
   - 0 GOROOT basenames in package_info.cs;
   - the path-precise marker gate at 0 violations / 0 missing (:790-798);
   - the `Stale copies removed` line quoted;
   - `du -sh` of each root.
(5) THE TWO-SEEDED CLASSIFICATION. THE GATE (H5, runbook :571): "every diff classified (§4)", with ZERO T5. Write-evidence is by CONTENT: CR-stripped equality against the seed, `*.cs.auto` included, line kinds counted per file. Never use mtime alone (corpus-reconvert SKILL :92, :94, :106-108).
   - **B-only** (B wrote it, A did not): a converter change since `<REGEN_BASE>` whose footprint never landed. ATTRIBUTE it to a named first-parent merge in `git log --first-parent <REGEN_BASE>..HEAD -- src/go2cs` (non-test). PREDICTED members, each scored present or absent:
     - `hash/maphash/maphash.cs`, the escapeForHash body goes from `throw panic("intrinsic")` to a no-op (666555ac26 / 5291f72c8e);
     - `internal/trace/traceviewer`: http.cs's `staticContent` initializer, the csproj's `EmbeddedResource` items, and the two staged payloads `static/trace_viewer_full.html` (2,618,942 B) and `static/webcomponents.min.js` (118,419 B), each sha256-equal to GOROOT's (685d69e428 / 34d8a5be0b). This is the only production `//go:embed` in go1.24.13 std;
     - batch 3's measured 20-file footprint (LEDGER 2026-09-22 02:36): 7 types in 6 packages gain `public` (1271662ec9); type arguments spelled in mlkem768 / mlkem1024 / h2_bundle; ref 10's MapType placeholder; and `src/core/os/exec.cs` RECLASSIFIED from shared to per-GOOS (C1's union seat was never cut);
     - runtime/pprof's linux/darwin artifacts, if any moved (8c's re-emission was windows-only; LEDGER 2026-09-23 04:03).

     The COUNT beyond these is NOT predicted. The footprints of batches 4-8 were never measured corpus-wide.
   - **A-only** (A wrote it, B did not): a converter change already applied to the committed tree. These are counted and listed by attributing merge, and never overlaid. Predicted LARGE: every README from 8fa5cc2e7d's published-stamp badge targeting, the vgetrandom files from 5287edebf4, pprof.cs from b04e45ad19, and so on. A non-empty A-only set is the POSITIVE CONTROL that arm A really is the older converter. An empty one means the pair measured nothing, so ABORT.
   - **Both wrote, EA == EB** (committed bytes NEITHER converter reproduces), and **Both wrote, EA != EB** where `git merge-file -p <committed> <EA> <EB>` leaves any residue against EB (CR-normalised): classify every such file or hunk through §4 IN ORDER (runbook :3315-3325), naming the class's evidence:
     - **T0**: CR-only / empty numstat. RESTORE.
     - **T1**: upstream, attributed. The committed file is an emission of a 1.23.12 source never re-emitted at 1.24.13; name the upstream commit. Overlay B.
     - **T1b**: dependency relocation; name the relocation. Overlay B. Test it BEFORE T2.
     - **T2**: test-closure re-emission. A production file a `-tests` run committed, for example before the 2026-09-22 04:01 banking rule, in one of T2's named shapes. Overlay B, which IS the restore; name the committing `-tests` commit.
     - **T3**: born-stale. The committed artifact predates an emission that has since landed; overlay B levels it. The named instances: (i) `GoPositionMap`-only hunks, the regen's own map levelling (corpus-reconvert SKILL :190, :198-199); (ii) `runtime/{windows,linux,darwin}/package_info.cs` and `profBufReadMode` accessibility (LEDGER 2026-09-22 14:49); and any B-emitted file absent from the committed tree whose emission both arms share.
     - **T4**: hand-own consequence (H6's differential). NOT overlaid; report it to §5's hand-own audit.
     - **(iv)**, a RULED HAND-SET LINE, tested before T5: `crypto/internal/fips140deps/README.md`, a hand-restored orange badge on an exclusion row with artifacts (8f6abad087; LEDGER 09-22 16:31). Both arms compose GREEN, so it lands in EA == EB or in the EA != EB residue. PREDICTED present, exactly one file. Disposition per R-B12. Any other hand-set line is not (iv).
     - **T5**: only where NONE of the above fits. STOP with the hunk quoted.
   - **PLANT (floor 13), before trusting a zero T5.** In a scratch copy, give one committed file an extra non-map line that both arms' emissions agree against. The classifier must name it T5. Remove the plant, re-run, and see the plant gone.
   - Also PREDICTED: no README is emitted for the three README-less exclusion rows with artifacts (internal/copyright, net/internal/cgotest, runtime/internal/wasitest). A new one would red 2b3.
(6) H5c, per runbook H5c step 2 (:860-876): EVERY FLAVOUR, each gated on its own rc.
   - For each `os` in windows, linux and darwin, run: `src/reconvert-deletions.ps1 -Root <stage>\B\src -GoRoot <GOROOT-1.24.13> -ExpectGo go1.24.13 -SourceGoRoot <GOROOT-1.24.13> -ExpectSourceGo go1.24.13 -Sentinel <stage>\B.run.stamp -Goos <os> -Goarch amd64`, a dry run with its log per flavour, then `rc=$?` on the next line.
   - `-ExpectSourceGo` is ALWAYS passed (:860-862). Without `-Goos`, the script takes the host flavour (`reconvert-deletions.ps1:415`), and a single flavour answers only for its own files (:673-676).
   - rc 0 or 2; any other code => STOP with the log.
   - Extract per flavour, one flavour's log at a time.
   - Check (f) (:913-914): a flat row that is not in the same class in all three flavour files is posted by name.
   - `-Apply` only with zero UNRESOLVED and full flavour agreement. PREDICTED: DELETE-ABSENT 0 and DELETE-DESELECTED 0 on each flavour, because a same-release regen removes no upstream file. Any DELETE or UNRESOLVED row is posted and is a STOP.
(7) THE OVERLAY, B's root only.
   - Copy `.cs`, `.csproj` and `README.md`, never `*.cs.auto` (runbook :1070-1081). Widen that set by every file an emitted csproj names in `<EmbeddedResource Include>`, derived from the emission. This closes a GAP in the ruled recipe; name it in the report. Predicted: exactly traceviewer's two payloads.
   - Use a straight tar copy under `set -o pipefail`, or capture `${PIPESTATUS[@]}` on the next line. Runbook :1075's `; echo "rc=$?"` reports only the extracting tar. Then `grep -c '^ D'` = 0.
   - The absent-in-stage set (:1083-1096): want EMPTY, or exactly R-B7's flat files (predicted: `src/core/os/exec.cs`).
   - Restore class T0, the T4 set, class (iv) (fips140deps's README, per R-B12) and the six root attribution files (empty numstat). Assert `^ D` empty.
   - Apply R-B6's pins in the working tree. They are PROVED after commit 6, at (9b).
   - Adopt `go2cs-stdlib.slnx` from B's root verbatim, and only after the absent set is disposed of (predicted 344 projects).
   - Re-measure the marker census in the tree: every marked file byte-identical.
   - SCORE B4(d)'s prediction on net/http's README.
   - Count emitted `.cs.auto` files against the committed ones, CR-stripped, per target. Report them and do NOT overlay them. Refreshing them is the §5 hand-own audit's job.
(8) `go generate .` in `src/go2cs`, AFTER the overlay: B7's one named exception. Only `stdlib-metadata.txt` may change. Commit it WITH the overlay (`.claude/rules/corpus.md:277-278`).
(9) COMMIT 6: stage by explicit path, never `git add -A`. The message carries the §4 class table with counts, the attribution list, the widened payload set and R-B6's pins.
(9b) PROVE each R-B6-pinned payload BY RE-CHECKOUT, after commit 6. The tar-copied bytes, and an i7 checkout's pre-pin CRLF copies of the two test payloads, say nothing about the pin, because neither was checked out through it.
   1. `rm` each payload and `git checkout HEAD -- <paths>`;
   2. compare sha256 with GOROOT's; want EQUAL for every payload;
   3. CONTROL: delete the new pin lines from the WORKING-TREE `.gitattributes` only, `rm` + `git checkout HEAD -- <paths>`, and see CR bytes appear (sha256 != GOROOT);
   4. then `git checkout HEAD -- .gitattributes`, `rm` + `git checkout HEAD -- <paths>` again, re-verify EQUAL, and `git diff --quiet`; `^ D` empty.

   A payload with no LF cannot red the control; name it.
(10) PRESERVE `<stage>` (both roots, both binaries, `seed.tar`, every diff) until COORD accepts the report (SKILL §6).

## B9 -- post-reconvert gates (per-run copies; each budgeted from the harness-gates table, i7 class; serial; disk >= 25 GB before each leg and each row; bin/obj/Generated purged after each leg and each row, then `^ D`)
(a) The converter suite, `go test ./... -count=1 -timeout 40m`: GREEN. This includes TestStdLibMetadataInSync, TestCommittedCoreReferencesResolve, TestPublishedCounterMatchesTheRecordedReleases and the reconvert-deletions skip-list guard.
(b) CNR: `NO REGRESSION`, with CHANGED predicted EMPTY. The falsifier is a CHANGED project whose diff traces to a package_info.cs alias record the overlay moved (runbook :59-61, channel 2). Classify any such project by §4, with zero T5. It must COMPLETE before (e) starts.
(c) STDLIB, by H7 AS AMENDED (runbook :1357-1383 and the 2026-09-16 amendment :1420-1442) on ALL THREE flavours, windows included.
   - One flavour per invocation, serially, from a per-run copy of the H7 script: Debug, `-p:GoTargetOS=<fl>`, `--no-incremental`, `-m`, `-p:UseSharedCompilation=false`, bin/obj/Generated purged between flavours with `^ D` empty after each purge, and the build root spelled `C:/...`.
   - The gate per flavour, EACH ARM ON ITS OWN COMMAND'S rc: exit 0; CS occurrences 0; MSB/NETSDK occurrences 0; unique sites 0; ASM == projects.
   - Predicted: ASM 343 of 343 own assemblies (core csproj excluding `*.tests.csproj`). A distinct-produced-assemblies count from the log reads 344 (the solution's one non-core member). Name both units; the house "344 / 0" is the second unit.
   - "Every unbuilt project platform-exclusive" is INERT on this corpus (:1420-1426) and is never cited as evidence.
(d) GolibTests (Release, windows) at the head: equal to B2's control NAME BY NAME. Golib is not a conversion target, but the overlay moves the core packages GolibTests references, so name any delta.
(e) THE FULL BEHAVIORAL SUITE at the head. The fail set must EQUAL B2's control BY NAME (the base two), with APPEARED empty and NOT MEASURED 0. It is owed because the overlay moves the core packages every behavioral project compiles against, which CNR (transpile-only) cannot see.
(f) `src/go2cs.slnx` Release build: exit 0, 0 errors, plus `check-solution-integrity.ps1`. It is owed after the hop's golib/runtime API changes (runbook :3398; CLAUDE.md: "Nothing routinely builds go2cs.slnx") and recorded in no 2026-09-22/23 ledger line. A red is a STOP-finding naming its first error; do not fix it here.
(g) ROWS WHOSE OWN PRODUCTION MOVED.
   - Derive the set: every banked roster row whose `src/core/<row>/` non-test files, in any GOOS folder, commit 6 touches. Print the set.
   - Run each through `run-validated-sweep.ps1 -Filter <row>`, the steady-state gate, reading the results tail FIRST (floor 14). It is meaningful now because the committed production IS the emission.
   - Predicted: PASS at the banked count and drift-clean. hash/maphash is scored against B2's control (predicted red -> PASS 59). os is included if exec.cs moved.
   - Over R-B8's cap: run hash/maphash and os only, and STOP with the list. The full roster sweep belongs to §5/§6.
   - crypto/tls: if it is in the set, LIST it and apply R-B10. It is never run on the i7.

## B10 -- THE FIGURES (commit 7; the guard derives them, never a hand)
Run `src/check-roster-format.ps1`. Write the roster header and the `docs/README.md` NEWS figures EXACTLY as the guard derives them, then re-run it to `0 of N`.

PREDICTIONS, scored as worded:
- 218 / 230 = 94.8%, with 56,974 matching (58,319 - 1,345) and 283 disclosed;
- 218 / 224 = 97.3%;
- LINUX 187 of 216, with 49,629 matching (49,636 - 7) and 163 disclosed;
- 2f: 218 checked / 0 older / 0 unread;
- 2g: 218 rows / 0 older / 0 refused;
- 2b2: 225 = 218 + 4 + 3.

THE H10 GATE (runbook :2239-2242) reports BOTH numbers against the 1.23.12 anchor (`docs/NEWS.md:13-16`: 204 / 215 = 94.9%, 97.6% of 209, linux 198 of 202):
- The absolute count goes 204 -> 218. It is >=, so the gate HOLDS; the ten relocation retirements are the recorded exceptions.
- Both percentages FALL, 94.9 -> 94.8 and 97.6 -> 97.3. This is the opposite-direction case the gate exists to report.
- The linux absolute falls 198 -> 187.

State all three in the commit message. The NEWS framing ("the Go 1.23.12 anchor") is H12's; leave it.

## B11 -- THE INDEX, ONCE, after the last roster edit (commit 7)
Run `regen-validation-index.py` with `--selftest`, then a dry run, then `--write`. Predicted:
- 218 rows == 218 roster rows;
- 232 pages = 218 by name + 10 by link + 4 by exclusion;
- 0 orphans and 0 page-less rows;
- FROZEN SNAPSHOTS unchanged.

## B12 -- CLOSING CHECKS (checked, not felt; each printed with its count, unfiltered)
1. FIRST-PAGE CENSUS: every roster row's FIRST `[proof]` page reads Go 1.24.13, 218 / 218. 2f and 2g both print 0 older. PLANT: in a scratch copy of the roster, point one row's first `[proof]` at a 1.23.12.3 page. The census must name that row. The tree is untouched.
2. LINUX: every numeric `linux:` annotation equals a 1.24.13 linux reading on record.
   - The sources are G's leg rows.tsv `linux_new` (6836e605ac) and the re-reads 971d919113 (message), ce065f8aa9, 2b65f11474 and b57703b1cd.
   - The residual set must be EMPTY. No row carries two Go versions.
   - PLANT: in a scratch copy, change one annotation (e.g. net `581 + 3` -> `580 + 3`). The check must name it.
3. `shardmap.py --timings recon-basis.tsv` exits 0.
4. THE RELEASE CENSUS as named identities. Runbook :2257-2259 says: "the count of banked test project files must equal the roster's row count". LEDGER 2026-09-22 16:31 REFUTED that equality as by design ("exclusion rows keep artifacts, relocation anchors keep source pages; restate it as a named identity at the close"). Quote both; c5's amendment corrects the sentence.
   - tests.csproj 225 = 218 + 4 + 3;
   - pages 232;
   - validated badges 214 = 218 - 4 README-less test-only rows (name the four), with fips140deps orange after R-B12's restore and 2b3 scored AFTER that restore;
   - index 218.

   The tip read 215 / 233 / 219 / 225.
5. DEADLINE FLOORS: every floored row holds against its largest 1.24.13 wall (the re-check table + 8g's crypto/tls line + sync/atomic 150m). runtime owes none, since it is a candidate (LEDGER 2026-09-22 18:02).
6. The core-refs table reads `declared 0 · measured 0`.
7. `git diff --diff-filter=D <B_BASE> HEAD` names exactly the ruled deletions and nothing else: current/net.http.md, R-B7's flat files, and commit 2's measured generated-test-artifact removals, each by name.
8. Census: `entry` on every staged doc, page, README and the report; `converted` on staged converted test sources. Then repoguard `TestNoFleetIdentifiersInTrackedFiles -count=1` with a planted control.
9. `git status --porcelain` empty (unfiltered), no ` D`; go2cs.exe count 0; `abclose-ctl` removed (a child, after its tracked count is asserted); lock released; worktree IN PLACE; `<stage>` preserved; NOTHING PUSHED.
10. THE CLOSE PRECONDITIONS: c3 (the B5 BOARD finding), c4 (database/sql TestRawBytesAllocs: attribution, or a BOARD finding if it stays unexplained) and c5 (the runbook's close amendment). Name each with its discharge ref: a commit in `<B_BASE>..HEAD`, or the ref the prompt named to land before the close STAMP. A precondition with neither is reported as OWED BEFORE THE CLOSE STAMP.

## Commits (on the branch, each signed, each staged by explicit path)
1. The B1 merges.
2. net/http's close-reading evidence (test artifacts, their admitted deletions, and `docs/phase4/h10-evidence/close-net-http/`).
3. net/http to the candidates (the row, the dated block, the page per R-B4, the README badge, the basis row).
4. srcimporter's demotion.
5. The core-refs sweep.
6. The regen overlay + `stdlib-metadata.txt` + R-B6's `.gitattributes` pins + R-B7's named deletions.
7. The header + NEWS + index.

## Report (final message)
Include:
- the base and refs as asserted;
- every ruling as applied;
- B2's controls, with the tree each ran in, and the BEFORE ignored census with its control;
- B3's reading: the word, go/C# counts, the divergence set by name against the prediction, TestRegisterErr's pair, the pin and `-AllowBranch` lines as the wrapper printed them, the wall, and the added and admitted-deleted sets;
- B4's shardmap red then green, and the guard's failure set;
- B6's before, after and plant lines;
- B8:
  - both binaries' sha256 and go version;
  - the per-arm check table, with any retry named;
  - the §4 classification table (B-only by attributing merge, with the predicted members scored; A-only by merge; T0-T4 and (iv) by named instance; T5 = 0 or the STOP) and the classifier's plant;
  - H5c's per-flavour table and the flavour-agreement check;
  - the overlay set with the widened payloads, the absent set, the re-checkout proof and its control, the .cs.auto counts, and net/http's README score;
- the B9 (a)-(g) verdicts with control comparisons by name, and the H7 arms per flavour with both units;
- the B10 figures scored against the predictions, and BOTH gate numbers;
- the B11 index line;
- the B12 checks 1-10, with the plants' reds;
- `git log --oneline --first-parent <B_BASE>..HEAD` with %G?;
- disk;
- everything not verified, stated as such.
