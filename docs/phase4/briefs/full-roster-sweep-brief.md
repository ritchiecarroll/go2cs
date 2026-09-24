You are a SWEEP WORKER for the go2cs fleet coordinator (COORD): R-LAPTOP's lane, G-LAPTOP's lane, or the i7's sub-agent. This brief is §6's ONE FULL VALIDATED-ROSTER SWEEP, the ruled instrument of §5's parity gate P2 (roster parity). It is owed BEFORE the version branch cuts over into master, and so before 1.24.13.1 is released from master (owner ruling, LEDGER 2026-09-24 00:04; release brief RN-14). All 218 banked rows are re-run LIVE through the steady-state gate `src/run-validated-sweep.ps1` at ONE tree, `<sweep-base>`, split into three SHARDS. S-R (R-LAPTOP) and S-G (G-LAPTOP, windows side) launch when COORD dispatches. S-7 (the i7) is a three-row residual that starts only after the release brief's Part C ends. You run ONLY the shard your prompt names. The sweep MEASURES. It never banks, fixes, re-baselines, or commits to the version branch or master. A moved verdict is a STOP-finding reported BY NAME. COORD merges the three shard ledgers and proves that their union covers every validated row exactly once (M). Before anything else, read `CLAUDE.md` (all 16 floors), `.claude/rules/harness-gates.md`, `.claude/skills/{validation-bank,gate-forensics,measurement-discipline}/SKILL.md`, and `docs/GoCorpusMigration.md` §3.1-§3.6, §5 and §6, all AT `<sweep-base>`. **THE RUNBOOK LEADS:** where this brief and the runbook disagree, STOP and quote both. COORD's prompt supplies every `<placeholder>`:
- `<sweep-base>`: filled at CP1;
- `<version-tip>`;
- `<repo>`: your host's clone;
- `<wt>`: the shard worktree. Keep it SHORT, e.g. a two-level drive path;
- `<scratch>`: outside any work tree;
- `<goroot>`, `<dotnet10>`, `<host>` (a nickname) and `<stamp>`.

Citations: `rb :N` is `docs/GoCorpusMigration.md@fa18863b94`, `sw :N` is `src/run-validated-sweep.ps1@fa18863b94`, and `ro :N` is `src/_roster.ps1@fa18863b94`. Re-find each cite by its text at `<sweep-base>`.

## COORD RULINGS (CP1, 2026-09-24; these SUPERSEDE every "COORD RULING NEEDED" default below where they differ)
- **Fills.** `<sweep-base>` = `61724860b4`: the CP1 STAMP (the post-close batch; ledger a0aefb2f1e, CNR `NO REGRESSION`, CHANGED EMPTY). `<version-tip>` is read by ls-remote at dispatch, and ancestry is asserted as Fixed facts says; the tip moves as Parts B/C land.
- **SW-1:** the FULL state (4759 + 1) is REQUIRED, through the raised-wall invocation on R-LAPTOP. The GOFLAGS exception is recorded, under the owner's raised-wall ruling of 2026-09-23.
- **SW-2:** DEFAULT. COORD owns dispatch, rulings and the merge; the lanes run detached drivers with the survival canary and the resume ledger. The deviations from §6's "never parked by a lane" and "fastest available machine" (the i9 is excluded by the owner's order, the i7 is busy through Part C) are RECORDED in the P2 line. The per-row budget is 4 × pkgTimeout + 30 min, and the driver kills ONLY its own child tree, by recorded PID.
- **SW-3:** the DEFAULT split S-R 89 / S-G 126 / S-7 3. AMENDMENT: if G-LAPTOP's windows re-qualification is clean (Go's own `net` twice, only TestLookupCNAME failing; a symlink created unelevated), `net` and `os` run on S-G AFTER its 126 rows, with the Q5 net preflight. S-7 then keeps only `internal/trace` and any COORD-ordered re-dispatches.
- **SW-4:** `net` alone. The scoped reading of "net-family" (the reader census: `lookup=0`, `ext=0` for the other 16 rows) is recorded in the P2 line.
- **SW-5:** the counts line (`N matched · D disclosed`) is GATING. A verdict-pair move with equal counts is NOT silently a reading: it is a ROW-LEVEL FINDING, reported by test name, preserved, and reviewed by COORD at M.
- **SW-6:** DEFAULT: record, restore, continue, no in-sweep re-read. S-7 re-reads only on COORD's order, as a reading.
- **SW-7:** DEFAULT: the sweep's 10m ask, raised to each floor.
- **SW-8:** the P2 line corrects the close STAMP's inverted wording. The rule is RAISE when the largest wall is at least 0.75 × the floor, and a breach becomes a post-sweep floor seat.
- **SW-9:** Windows PowerShell 5.1 on every host.
- **SW-10:** the dirt control tree is `fa18863b94`.
- **SW-11:** DEFAULT evidence home: the per-shard lane refs, the ledger P2 line, and the DATA-sweep-row-walltimes block.
- **SW-12:** the SF gate is ACCEPTED. NAMED HUNKS ADMITTED between `<sweep-base>` and the release tree:
  - C1's docs seat `bc4f38ebd5` (docs/**, CLAUDE.md, .claude/rules, src/tour/README.md, src/version.props and src/set-version.ps1 comments, src/migrate-gorelease.ps1). Its `docs/ValidatedTestPackages.md` prose edits are admitted ONLY if `Get-ValidatedRosterRows` returns the IDENTICAL 218 rows and cells at both trees, a read-only equality COORD runs at M.
  - The release-census seat `2dc990302f` (src/_roster.ps1, src/check-roster-format.ps1, src/push-nuget.ps1), admitted on the same row-list equality, since its moved functions keep their bodies.
  - G's crypto/tls LINUX annotation `836004dd20` (a linux cell only).
  - Part C's RN-5 overlay: the README and csproj of crypto/x509/internal/macos, internal/runtime/syscall and vendor/golang.org/x/net/route. All three are platform-exclusive and not windows packages, so no windows row reaches them.
  - The announcement docs.

  The crypto/tls manifest prose is NOT edited before the release. A red SF takes the DEFAULT remedy.
- **SW-13:** CONFIRMED: `-VerifyOnly` re-runs at the post-cutover master tip, and a clean sweep owes none of its own.
- **SW-14:** NO linux leg at P2.
- **SW-15:** CONFIRMED: P3 was discharged at the close STAMP (B9(e): 691 projects, fail set equal to its control), and A7's not-owed stands for the zero-footprint batch.
- **SW-16:** R clears its post-hop work for S-R's duration, and Q records R-LAPTOP's network, free disk and idle state. The i7's reclaims are done (34 GB free). The owner has applied G-LAPTOP's DNS, Developer Mode and long paths; its IPv6 unbind is pending. Each dispatch asserts that no other session on the box runs `dotnet build-server shutdown` or a name-scoped kill for the shard's duration.
- **SW-17:**
  - d15 (the gofmt pass) is RE-DATED to POST-RELEASE (option b): it is cosmetic, and the converter stays frozen between the sweep and the release.
  - d7 / PAR-3: DEFAULT.
  - c6-c9 are POST-HOP (RN-15's part), so no crypto/cipher re-run is owed.
  - The docs migration does not touch src/go2cs, VERIFIED by `bc4f38ebd5`'s file list.
- **SW-18:** option (a): d6's live absorption is discharged by check-roster-format's §1b2 synthetic arm plus 8g's page block count (3419). No extra reading.
- **SW-19:** WAIVED and recorded: the first row builds its closure, and the purge every 30 rows removes a solution build's output anyway.

## Why
- **P2 (rb :3587):** "every roster row appears in exactly one shard ledger (nothing lost) and the absolute row count ≥ prior ... Both absolute and percentage reported. Every row backed by a regenerated proof page and a re-derived, re-signed manifest." Its arithmetic half was already read at the close STAMP (LEDGER 2026-09-23 22:30):
  - 218/230 = 94.8% and 218/224 = 97.3%;
  - absolute 204 -> 218;
  - 56,974 matching / 283 disclosed;
  - 218/218 first proof pages at 1.24.13.
  - The manifest half rests on H10 step 3: C1's orphan census over all 48 manifests and 327 pins (LEDGER 2026-09-22 15:59, `f4903fad5f`) and the re-sign `claude/c1-manifest-resign` (ACCEPTED 16:22, `082ec41bb0`, batch 8b). M6b re-reads it live.

  What is still owed is the LIVE re-measurement of every banked row at one tree. PLAN-corpus-upgrade.md:555 names `run-validated-sweep.ps1` over the full roster as P2's instrument. §6 (rb :3609) owes it "once, at the parity gate: coordinator-owned, backgrounded, on the fastest available machine, never parked by a lane". The close swept only 8 rows (B9(g)) and deferred the rest here by name, including crypto/tls's live absorption with BlockSize 3419 (close obligation d6; batch8g brief :108).
- **Consolidation.** This is also the 1.24.13 CONSOLIDATION SWEEP. Its per-row logs and walls are the next hop's shard-map basis, "unrecoverable afterward" (rb §3.2 :3246-3249). It also carries the crypto/rsa generator-transient re-run that was folded into "the next full sweep" (gate-forensics SKILL :1737-1766).
- **Not this instrument.** `run-h10-recon.ps1` is H10's re-derivation wrapper, "never the sweep" (rb :2203). It has no `-Exact` and applies no acceptance predicate; `-Exact` belongs to the sweep. The committed `h10-dispatch-plan.tsv` and `shardmap.py` are unusable here: they hard-code the excluded i9, and they carry candidate rows that the sweep refuses (`No banked packages matched`, sw :244).

## Fixed facts (fetch, then assert each by `git ls-remote origin`; STOP on any mismatch)
- `refs/heads/claude/version-go1.24.13` == `<version-tip>`. Both `git merge-base --is-ancestor <sweep-base> <version-tip>` and `git merge-base --is-ancestor fa18863b94 <sweep-base>` read rc 0. `<sweep-base>` is Part A's pushed batch head: `fa18863b94` + C2's `781c1c3c31` (the `-stdlib` staging seeder) + `ae813db069` (the multi-target csproj merge renderer) + C1's `13e9b039ea` (docs), with a ZERO corpus footprint and CNR `NO REGRESSION` with CHANGED EMPTY (release brief A4(9), A5). `git diff --name-only fa18863b94 <sweep-base> -- src/core src/gen docs/validation docs/ValidatedTestPackages.md src/run-validated-sweep.ps1 src/_roster.ps1 src/_paths.ps1 src/version.props src/Directory.Build.props src/.editorconfig .gitattributes`, then `rc=$?`, must print NOTHING. If it names any path, STOP.
  - The converter delta is pinned by name: `git diff --name-only fa18863b94 <sweep-base> -- src/go2cs src/gen`, then `rc=$?`, prints EXACTLY `src/go2cs/platformCensus.go`, `src/go2cs/platformCensus_test.go`, `src/go2cs/platformProject.go` and `src/go2cs/platformProject_test.go`. If SW-17 rules d15 option (a), it also prints d15's files, and nothing else. Any other path is a STOP.
- master == `074a12c4ae`. Read-only here; COORD's SF gate reads it.
- At `<sweep-base>`, read by OUTPUT after dot-sourcing `<wt>/src/_roster.ps1`:
  - `Get-ValidatedRosterRows -Path <wt>/docs/ValidatedTestPackages.md` returns 218 rows in TABLE ORDER. Row 51 is `crypto/tls`, row 127 is `internal/godebugs`, row 128 is `internal/gover`, and row 218 is `weak`.
  - Cells: `crypto/tls` 4759 | 1; `net` 476 | 3; `os` 1105 | 2; `internal/trace` 92 | 0; `path/filepath` 61, plus 6 host-conditional `TestWalkSymlinkRoot/*` names; `os/exec` 116, with the host-conditional-disclosure `TestExtraFiles`.
  - Exactly two `execution:` pins, `internal/godebug` and `log/slog`, both `release-tiered` (roster :348, :378).
  - The header reads 56,974 / 283.
  - `src/version.props`: GoStdLibVersion 1.24.13.
  - `$longTimeouts` (sw :937) has 12 entries: hash/maphash 60m, index/suffixarray 120m, crypto/dsa 120m, archive/zip 60m, go/parser 90m, crypto/internal/fips140/mlkem 30m, crypto/mlkem 30m, time 40m, crypto/tls 60m, sync/atomic 150m, net 120m, and net/http 60m (a candidate the sweep never reaches).
  - `$capabilityConditionalBlocks['crypto/tls']` = TestBogoSuite / 3419 (sw :646-647).
- **Hosts, by the record.**
  - EXCLUDED: the i9 (owner's order, CPU defect; LEDGER 2026-09-22 21:33), AZ1 (deleted), and C1/C2 (no pwsh or dotnet).
  - **R-LAPTOP.** 8C/16T. The only host measured at crypto/tls's FULL windows state: 1,761 s at the raised wall (LEDGER 2026-09-23 12:36). NOT net-qualified: the 2026-08-29 DNS fix did not hold by 09-08, and nothing is recorded since. No symlink privilege. Its current network, free disk (34 GB, old) and idle state are UNRECORDED; R is active on post-hop work (LEDGER 2026-09-24 00:45, 00:54).
  - **G-LAPTOP, windows side.** 6C/12T; 215 GB free. NOT net-qualified (TestLookupNoSuchHost x23; router relay). No symlink privilege. LongPathsEnabled=0 (LEDGER 2026-09-24 00:33).
  - **The i7.** The only DNS-QUALIFIED windows net host (LEDGER 2026-09-23 10:10). Holds the symlink privilege. 34 GB free at the derivation. Busy with Parts A-C until Part C ends. One battery per box.

## Rules for EVERY shard
- **Floors (CLAUDE.md)**, each binding here:
  - 1: your shard's driver is the only `-tests` run in `<wt>`.
  - 4: converter, gen and golib source is frozen, and every run goes from a per-run COPY of the sweep.
  - 5: never Stop-Process by name. A stray go2cs.exe is a STOP, counted by image path, never killed.
  - 6: GOROOT is spelled exactly as `go env GOROOT` prints it, with backslashes.
  - 7: `rc=$?` on the NEXT line, before any pipe. The driver prints `DRIVER_EXIT=<n>`, and the launcher greps for it.
  - 8: never `git add -A`. `^ D` is empty after every restore.
  - 11: one worktree per shard, never the main checkout.
  - 12: at least 25 GB free, and children-first reclaim.
  - 13: every gate is plant-proven.
  - 14: read the results tail FIRST on any non-PASS.
  - 15: measure at `<sweep-base>` after a fetch.
  - 16: every census runs unfiltered.
- **PINS**, read back by OUTPUT before the first row:
  - GOROOT removed or overridden in the PROCESS only (the machine-scope value is the dead 1.23.1 pin on the i7; never edit Machine or User scope), and `<goroot>\bin` first on PATH;
  - `go version` = go1.24.13 windows/amd64, and `go env GOROOT` = `<goroot>`;
  - GOTOOLCHAIN=local and CGO_ENABLED=0 (the sweep also pins CGO_ENABLED, sw :159-161);
  - DOTNET_ROOT=`<dotnet10>`, first on PATH, and `dotnet --version` reads 10.0.x;
  - GOFLAGS unset, and `go env GOFLAGS` empty. Go reads an unset or EMPTY variable from the go env file, so a non-empty `go env GOFLAGS` with the variable unset comes from that file: STOP and report. Never `go env -w` or `-u`;
  - `GoTargetOS` unset (Get-SweepTargetGoos returns it before falling back to windows, ro :194-203; `GoTargetOS=linux` is the linux-arm habit, harness-gates :43), and no `DOTNET_Tiered*`, `DOTNET_TC_*` or `COMPlus_*` variable present (an ambient one can override the Release/tiering-off configuration of record and the `release-tiered` pins). Print the NAMES found, never values. Every sweep header must lack a `target OS` line (sw :327-331).

  The sweep itself sets none of GOTOOLCHAIN, PATH or DOTNET_ROOT, so a missing pin surfaces as NETSDK1045.
  - **EVERY SHELL, not just the driver.** Shell state does not persist between tool calls, and a fresh shell inherits the ambient environment. Write `<scratch>/p2/<host>/env.sh` once. It exports PATH=`"<goroot-posix>/bin:<dotnet10-posix>:$PATH"`, GOROOT=`'<goroot>'` (the backslash spelling, single-quoted), GOTOOLCHAIN=local, CGO_ENABLED=0 and DOTNET_ROOT=`'<dotnet10>'`, and unsets GOFLAGS, GoTargetOS, NUGET_API_KEY and NuGetCertFingerprint. SOURCE it at the head of EVERY Bash command that runs go, dotnet or powershell (Q3, Q4, S0.2's two builds, S0.4's plant, CLEANUP), and assert `go version` and `go env GOROOT` in the SAME command (gate-forensics SKILL :1193: "env-pinned in the SAME command"). S0.2's identity build runs under exactly these pins, the pins A2(4) was built under.
- **THE SWEEP'S HOST is Windows PowerShell 5.1 (`powershell`) on every host (COORD RULING NEEDED, SW-9).** It is .NET Framework, so the dotnet10 pin cannot break it. The pin does break G's pwsh (a net10 dotnet tool) and the i7's pwsh (a net8 apphost; gate-forensics SKILL :1504-1511). `_roster.ps1` supports 5.1 (ro :56; its Desktop JSON reader, ro :1846-1850).
- **CREDENTIALS.** In every shell and in the driver, remove NUGET_API_KEY and NuGetCertFingerprint from the PROCESS environment, then assert both absent by BOOLEAN. Never read or print either one.
- **NEVER PASS**:
  - `-TestConfig` or `-TestTiered`: either one, even at the default value, overrides every row's `execution:` pin and prints "an A/B measurement, not a bank-eligible sweep" (sw :303-325);
  - `-IgnoreDiskPreflight`;
  - `-ShardCount/-ShardIndex`: their contiguous slices cannot route rows by host;
  - GOFLAGS, except in crypto/tls's own invocation (D2).
- **NEVER RUN**:
  - `run-h10-recon.ps1` (except `-SelfTest`) or `run-h10-dispatch.ps1`;
  - `clean-bin.ps1` without `-Root <wt>\src\core` before cleanup. Its default root deletes `src/go2cs/bin/go2cs.exe` and breaks `-SkipBuild`;
  - `release-nuget.ps1` or `push-nuget.ps1`;
  - any `git tag`;
  - `git push`, except to your OWN lane ref, and only when COORD says so. Lane commits are unsigned.
- **NEVER BANK.** Nothing moves from the sweep tree: no roster, page, badge, manifest, index or source change (validation-bank SKILL :1003, "classify the dirt, never bank it"). The worker contract (rb §3.6 :3470-3480) gives raw output only, with no rulings. Classify only against the documented classes, and post anything else UNCLASSIFIED. A worker never raises a budget, and never re-runs a red with changed parameters.
- **SURVIVING THE REAP (a PRE-LAUNCH COORD RULING, SW-2; no lane launches without it).** The records DISAGREE on whether a lane's detached run survives its turn boundary, and all three texts that say it does not are quoted here:
  - rb :3609 (§6): "never parked by a lane — a lane's process tree is reaped at its turn boundary, and sweeps have been lost to exactly that";
  - harness-gates :342 (budget table, full-roster sweep): "run it BACKGROUNDED from the COORDINATOR session only — ⚠ a LANE parking a detached sweep and ending its turn gets it KILLED";
  - gate-forensics SKILL :1203 (the comment under :1195-1200): "(a LANE parking a detached sweep still loses it)".

  Against them: :1197 says "`Start-Process -WindowStyle Hidden` with output redirected to a log survives", and lanes have completed long batteries since: G's linux sync/atomic row, 5,868 s (LEDGER 2026-09-23 11:51), and R's crypto/tls bank row, 1,761 s (12:36). Neither ledger line records the launch shape, so they are consistent with survival, not proof of it. The shard's DEFAULT shape is a DETACHED driver (D), launched from a FOREGROUND Bash call through `Start-Process -WindowStyle Hidden` with output redirected to a log. A Hidden launch from inside a PowerShell TOOL call, or from a `run_in_background` task, dies (gate-forensics SKILL :1197-1200). The hidden window still gives the child a console, which os/signal's TestCtrlBreak reads.
  - PROVE survival BEFORE row 1, cheaply: launch a detached sleeper by the SAME LAUNCH shape (a 5.1 script that appends a timestamp to `<scratch>/p2/<host>/canary.log` every 60 s for 30 min), then end your turn once. On the next wake, the sleeper's PID must be alive by a census on image path and command line (never the output file), and `canary.log` must have advanced across the boundary. A dead sleeper is a STOP for COORD: the driver is not launched. S-7 is exempt (its sub-agent never ends a turn while its driver runs; see S-7).
  - The ledger is the resume record. If the driver dies, re-launch the same driver: its START gate (D) restores the tree first, then recovery is `list − terminal` (rb :3385, :3496-3497).
  - On a dead driver, CLASSIFY before diagnosing (rb :3488-3493): record uptime first, then which of the four causes applies (a sibling's bare-name kill, the harness tree reap, a sibling's machine-global `dotnet build-server shutdown`, a reboot).
- **Nicknames only.** Quote every log line with the GOROOT, dotnet root and profile paths replaced. Run the census `entry` (`coord-identifier-census.sh` from master `074a12c4ae`) on the report before sending it, and on every evidence file before it is staged (EVIDENCE COMMIT, after CLEANUP).
- **Machine-global hazards.** For the shard's whole duration no other session on the box may run `dotnet build-server shutdown` or any name-scoped process kill: both are machine-global, and worktree isolation does not help (rb :3488-3492). COORD asserts this in the dispatch; the worker records it in Q2.

## THE SHARD PLAN (by ROSTER TABLE ORDER at `<sweep-base>`, as `Get-ValidatedRosterRows` returns it)
| shard | host | rows | projected wall (+ ~110 s first closure build) | starts |
|:--|:--|:--|:--|:--|
| **S-R** | R-LAPTOP | `crypto/tls` FIRST (raised wall), then rows 128-218 (`internal/gover` .. `weak`) MINUS `internal/trace`, `net`, `os`: **89 rows** | **6,454 s ≈ 1.8 h** (crypto/tls 1,761 s) | at dispatch |
| **S-G** | G-LAPTOP windows | rows 1-127 (`archive/tar` .. `internal/godebugs`) MINUS `crypto/tls`: **126 rows** | **6,495 s ≈ 1.8 h** (+ ~885 s for SW-18 (b)'s extra crypto/tls READING, if ruled) | at dispatch |
| **S-7** | the i7 | `os`, `internal/trace`, `net`: **3 rows** | **≈ 563 s** + two net qualifiers (~50 s each) ≈ 0.2 h | after Part C ends; before the cutover |

- **Checksum and derivation.** 89 + 126 + 3 = 218, each row exactly once. Each driver DERIVES its list from the parser at `<wt>` by this rule, prints the list with its count, and asserts the boundary names. COORD cross-checks the lists before launch (M1): at S-R/S-G launch, the two printed lists plus S-7's FIXED name list (`os`, `internal/trace`, `net`), because the i7 is busy through Part C; S-7's printed list is re-asserted against the same union when it launches.
- **Projection basis.** COORD's copy of the drafter's `projected-walls.tsv`:
  - 199 rows measured at 1.24.13, on laptop-class hosts or from the ledger;
  - 15 rows scaled from i9 walls ×2.0;
  - 4 abort lower bounds: crypto/ecdh, crypto/sha3, embed/internal/embedtest and sync.

  It EXCLUDES oracle re-runs, purge rebuilds (~110 s each) and the sweep's per-invocation end-of-run `git diff --numstat -- src/core`. The measured cross-host factors are close: R/i9 1.99 and G/i9 1.82. Budget from the top: 1.8-2.8 h per lane shard.
- **Budgets (rb §3.6 :3476-3477 and :3484, which LEAD).** The runbook says "A run that exceeds its stated budget is **killed and reported as a timeout with the log tail**" and "A worker's own outer wrapper must clear the instrument's internal budget". The STATED budget is therefore PER ROW, in the driver: `4 × pkgTimeout + 30 min`, where pkgTimeout = max(the ask, the row's `$longTimeouts` floor) exactly as sw :1092-1099 computes it. That clears the instrument's own worst case: two attempts (the oracle re-run) × two test sides, each side bounded by `-test-timeout`, plus conversion and builds. If the two sides overlap, the budget is generous, which is the safe direction. -test-timeout bounds only the test children (sw :835-846), so this is what bounds a hang in conversion, the build, the drift census or git. crypto/tls: 4 × 90m + 30m = 390 min. Breach handling is D2's. The shard-level 4 h is a READ CHECKPOINT, not a budget: COORD reads the ledger tail and the running row's results tail, and nothing is killed at the shard level.
- **Routing, by the record.**
  - **crypto/tls -> R only.** R alone reached the FULL windows state (the bank's 1,761 s; the 09-22 evidence at 2,216 s). On its windows side G read only the host-limit state at the standard wall (885 s, 1,340 + 2; sw :986), and its windows raised-wall run is unmeasured. G's HARDWARE did reach the full fan-out at the raised wall on its WSL linux arm: 1,824 s, TestBogoSuite 1,023 pass / 2,396 skip on both sides (LEDGER 2026-09-24 00:31, `836004dd20`), well inside the runner's 2,400 s wall. The i9 is retired from this row.
  - **net -> the i7 only.** It is the only DNS-qualified windows host.
  - **os -> the i7 only.** os was banked at 1105 on a PRIVILEGED host and carries NO host-conditional annotation. Without the privilege, `TestOpenFileCreateExclDanglingSymlink` skips before it spawns its InRoot/NoRoot subtests (os_test.go:2227), and both lane hosts read 1103 (G at recon `0dc65a8e8d`, R at H10 s1 `c6fdbe73c3`). It is predicted COUNT on either lane.
  - **internal/trace -> the i7.** It validated 92 there at 8c. On G at `8fc439415f` it DIVERGED on 4, with TestTraceCPUProfile's "use of internal package internal/profile not allowed" still unattributed. It is unmeasured on R since the junction seat.
  - **Host-neutral rows stay where the cut puts them.** internal/coverage/cfile (validated 15 + 1 on G after the seat) stays on G. These are neutral by record: path/filepath (host-conditional), os/exec (host-conditional disclosure), go/internal/srcimporter (a slower host is the safe direction, LEDGER 2026-09-23 22:37), internal/godebug and log/slog (the default path applies their pins), and the loopback net-family rows.
- **Net-family scope (COORD RULING NEEDED, SW-4).** The rule is measurement-discipline SKILL :294: "**Preflight `go test -count=1 net` before any net-family run**". The roster's net row says "a two-line preflight for every net-family run". Neither defines "net-family". The DEFAULT (`net` alone) departs from the rule's widest reading, and its evidence basis is this: in go1.24.13's tests, only `net` resolves nonexistent names or runs the DNS families. The drafter's reader census (COORD's copy of `hostmarks.tsv`, one row per banked package) reads `lookup=0` and `ext=0` for internal/poll and all 15 net/* rows, against `lookup=12`, `ext=9` for `net`. Those 16 rows use loopback or literals, and they banked on unqualified R and G at H10 pass 1: internal/poll; net/http/{cgi,cookiejar,fcgi,httptest,httptrace,httputil,internal,internal/ascii}; net/mail, net/netip, net/rpc, net/rpc/jsonrpc, net/smtp, net/textproto and net/url (~754 s). COORD records the scoped reading of "net-family" in the P2 ledger line. Under a wide ruling, those 16 rows move from S-R to S-7.
- **Alternatives (COORD RULING NEEDED, SW-3).** (a) W=3 balanced: R 69 / G 74 / i7 75 rows at ~4,500 s each, a makespan of ~1.3 h, but the lanes wait on Part C. (b) The owner's per-box DNS fix and/or Developer Mode on a lane box, re-qualified by Q, would move net and/or os off the i7.

## Q -- HOST QUALIFICATION (each host runs this FIRST, before cutting the tree; every reading goes into the report)
- **Q1 Pins**, as in Rules.
- **Q2 The box.**
  - Record logical CPUs, RAM, the git version and UPTIME (last boot time).
  - Free disk on `<wt>`'s drive must be at least 25 GB; 60 GB or more is advised, since per-row growth is unmeasured. Also read the PROFILE drive (GOCACHE, the NuGet cache and %TEMP% grow there) and record both.
  - go2cs.exe count by image path must be 0.
  - NO other battery or `-tests` run may be live on the box: the lane's post-hop worktrees are idle, and so is G's WSL arm. Record COORD's machine-global-hazards assertion (Rules).
  - `LongPathsEnabled` (a read of `HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem`, never a write), recorded on EVERY host. G's `0` is on record (LEDGER 2026-09-24 00:33); R's is unrecorded.
  - Domain-joined, as a BOOLEAN only (`(Get-CimInstance Win32_ComputerSystem).PartOfDomain`); never print the domain, account or SID. A one-run oracle verdict on a networked, domain-joined host can be wrong in either direction (measurement-discipline SKILL :374-381: os/user PASS 1.05 s vs FAIL 21.2 s on one host).
  - The C-TOOLCHAIN capability (rb §3.3 :3337-3340): whether `gcc`, `cc` or `clang` resolves on the sweep's PATH, as booleans (the sweep pins CGO_ENABLED=0, so the Go side does not use it; record it anyway).
  - The §3.3 precondition "The whole-solution build has been run once" (rb :3334) is NOT run by default: the first row builds its closure (~110 s), and the purge every 30 rows deletes a solution build's output anyway. This is a recorded deviation, COORD's to confirm (SW-19).
- **Q3 Symlink privilege, FUNCTIONAL.** Never `whoami /priv`, which Developer Mode leaves unchanged.
  - Run Go's own `go test -count=1 -timeout 10m -run 'TestOpenFileCreateExclDanglingSymlink' -v os > <scratch>/q-symlink.log 2>&1`, then `rc=$?`, and record whether the InRoot and NoRoot subtests RAN.
  - Predicted: absent on R and G, PRESENT on the i7. On the i7, absent is a STOP for `os`.
- **Q4 `echo` on the sweep's PATH.** A reading only: os/exec's TestString is count-neutral.
- **Q5 THE NET QUALIFIER.** Run it only on a shard that carries `net` (S-7, or any shard SW-4 widens), IMMEDIATELY before net's row, because the owner may re-bind IPv6. It is a DRIVER PRE-ROW HOOK for `net` (and for every row SW-4 adds), never a foreground command beside a live driver: two runs on one box would be two batteries (floor 1).
  - First read the IPv6 binding per adapter (`Get-NetAdapterBinding -ComponentID ms_tcpip6`, read-only) and record it. net banked with the i7's IPv6 UNBOUND (LEDGER 2026-09-23 10:10). A different state is a STOP before `net`, for the owner.
  - The hook runs Go's own `go test -count=1 -timeout 40m net` twice (`netqual-1`, `netqual-2`), each by D2's Start-Process shape into `.log` and `.err`, with GOFLAGS empty, and records each rc.
  - THE CRITERION, `.`-sourced from ONE block the driver and its -SelfTest share. Each run passes only when ALL of these hold: (i) the tail is `ok  \tnet\t` with rc 0, or `FAIL\tnet\t` with rc 1; (ii) no `panic:` and no `test timed out` line (floor 14: the tail states a deadline kill outright); (iii) the FAILING SET, the names on `--- FAIL:` lines at any indent, printed unfiltered, is a SUBSET of {TestLookupCNAME} (the named, evidenced tolerated leaf; LEDGER 2026-09-23 10:10; it has no subtests, lookup_test.go:345-376). A clean run passes. Any other failing name STOPS `net` by name, and so does any other rc.
  - PLANT (floor 13), four arms through the SAME sourced block against the live logs (measurement-discipline SKILL :297-298): the tolerated set alone passes; tolerated + `--- FAIL: TestPlant` aborts naming TestPlant; `--- FAIL: TestPlant` alone aborts; an empty set passes and prints the absent tolerated leaf. Add a fifth: a `panic: test timed out after 40m0s` line must red (ii).
- **Q6 R only: crypto/tls needs the network** (TestVerifyHostname, TestRealResumption, and the pinned BoringSSL module fetch). There is no separate qualifier: the row's own STATE is the reading (P).

## S0 -- THE TREE, THE CONVERTER, THE PER-RUN COPY
1. **The tree.**
   - `git -C <repo> fetch origin`, then `rc=$?`. Assert the Fixed facts.
   - `git -C <repo> worktree add -b p2/<host>-<stamp> <wt> <sweep-base>`, then `rc=$?`. It is a local branch that is never pushed; G's DETACHED read tree failed H10's preflight rows 10-11 (LEDGER 2026-09-22 00:05).
   - Assert HEAD == `<sweep-base>`, `--git-dir` != `--git-common-dir`, and `git status --porcelain` empty (unfiltered).
   - `git -C <wt> status --porcelain --ignored=matching -- src/core src/gen docs/validation`, then `rc=$?`, must show zero `!!`. PLANT: one ignored scratch file under src/core must be named, then removed.
   - Record the tracked-file count without a pipe (floor 7): `git -C <wt> ls-files -z > <scratch>/p2/<host>/ls0.z`, then `rc=$?`, then count the NULs in that file. Every later count uses the same shape. Take `<wt>.lock` holding the driver PID.
2. **The converter**, built the sweep's own way (sw :334-341); rows then run with `-SkipBuild`.
   - `(cd <wt>/src/go2cs && go build -o bin/go2cs.exe .)`, then `rc=$?`.
   - The IDENTITY build: `(cd <wt>/src/go2cs && go build -trimpath -buildvcs=false -o <scratch>/go2cs-identity.exe .)`, then `rc=$?`.
   - Record both sha256 values, `go version` of each (go1.24.13), and each size and mtime. The identity sha must EQUAL Part A's A2(4) identity (COORD quotes it) and the other shards' (M5).
3. **Per-run copy (floor 4).** `cp <wt>/src/run-validated-sweep.ps1 <wt>/src/run-validated-sweep.p2-<host>.ps1`. It MUST sit in `<wt>/src`: it dot-sources `_paths.ps1` and `_roster.ps1` and derives its roots and preflight drive from `$PSScriptRoot`. It reads `??` in every porcelain listing until cleanup deletes it.
4. **PLANT the refusal path** (it costs seconds and converts nothing). Run the copy with `-Filter net/http -Exact -SkipBuild` in the bash shape `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <wt>/src/run-validated-sweep.p2-<host>.ps1)" -Filter net/http -Exact -SkipBuild < /dev/null > <scratch>/p2/<host>/plant-refusal.log 2>&1`, then `rc=$?`. It must pass the toolchain check and then throw `No banked packages matched filter 'net/http'` (sw :182-244), with rc != 0 and `git status --porcelain` unchanged. The driver's refusal-marker scanner must name the marker in that log (a -SelfTest input).
5. **Evidence root** `<scratch>/p2/<host>/`, outside any work tree, holding `rows/`, `ledger.tsv`, `driver.log` and `driver.pid`.

## D -- THE DRIVER (`<scratch>/p2/<host>/p2-driver.ps1`; Windows PowerShell 5.1; detached)
The driver sets the Rules' pins with `$env:` inside, removes both credentials, runs `Set-Location <evidence-root>` and uses ABSOLUTE `<evidence-root>\rows\<row__>\` paths only (`<row__>` is the row with `/` replaced by `__`), each created with `New-Item -ItemType Directory -Force` before its row. It runs at `$ErrorActionPreference = 'Continue'` throughout: under 'Stop', Windows PowerShell 5.1 turns a native child's first stderr line into a terminating error (sw :381-385; harness-gates :410). It carries the `$longTimeouts` table asserted in Fixed facts, and it derives the list by the SHARD PLAN's rule. Its whole body sits in try/finally, and the outer finally prints `DRIVER_EXIT=<n>` on EVERY exit path, a run-level STOP included.

**-SelfTest** (must pass before launch). One fixture per admissible and inadmissible shape, each parsed by the SAME functions the live path calls:
- the plain PASS; PASS with ` [release-tiered]`; the host-conditional PASS (sw :1253); the host-conditional-disclosure PASS (sw :1261); the capability-absent PASS (sw :1266); the host-limit PASS (sw :1275); COUNT; DISC; CVAC; FAIL; ORACLE;
- RERUN, then the `ORACLE FLAKED ONCE` note, then PASS (a discharge, NAMED); RERUN then ORACLE;
- must NOT parse as a verdict: a column-0 look-alike; an 8-space-indented tail line (sw :1341); a no-verdict log; a log with TWO final verdict lines (it must red "doubled");
- each refusal marker, placed in the `.err` file as well as the `.log`, and S0.4's real `plant-refusal.log`;
- the page reader: EQUAL, MOVED (one verdict cell changed), and NOT REWRITTEN (page mtime before the row start);
- Q5's criterion arms (Q5), for a shard that carries `net`;
- the NUL-count tell (harness-gates :407) over a log produced by D2's own Start-Process redirection, which must read 0.

**START GATE** (every launch, the first included). Record uptime. If `git -C <wt> status --porcelain` shows anything beyond the per-run copy, or a `rows/<row__>/INFLIGHT` marker exists (D2 writes it, D6 deletes it):
1. record the §3.6 classification (Rules) in `driver.log`;
2. move the in-flight row's records to `rows/<row__>/aborted-<n>/`, and append an `ABORTED` ledger row for it;
3. restore both roots, and remove the untracked paths by explicit name, as in D5;
4. purge with clean-bin (as in D1; rc 0 or STOP). A killed row can leave a truncated `.cs` or build output newer than its inputs, which §3.4's false-green #4 then protects (rb :3420-3425). A full purge covers the whole closure, not just the in-flight row;
5. assert `^ D` is empty, the `-z` tracked count equals S0's, and the go2cs.exe count by image path is 0 (a survivor of the dead driver is a STOP).

Then resume `list − terminal`: skip a row only if its ledger word is TERMINAL at `<sweep-base>` (rb :3385). TERMINAL means PASS, COUNT, DISC, CVAC, FAIL, ORACLE, N/A, NOT MEASURED or TIMEOUT; the last two go back to COORD for a raised-budget re-dispatch and are never re-run at the same budget (rb :3499). REFUSED and ABORTED rows re-run WHOLE, under an attempt suffix `rows/<row__>/attempt-<n>/`.

Then, for each row IN LIST ORDER:
1. **PRE-ROW.**
   - If `<scratch>/p2/<host>/STOP` exists, restore and exit with `DRIVER_EXIT=3`. This is COORD's only row-boundary stop mechanism (a second battery, a COORD order).
   - Read free disk on the `<wt>` drive AND the profile drive. When the `<wt>` drive is under 35 GB, or 30 rows have passed since the last purge, purge: `Start-Process powershell -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','<wt>\src\clean-bin.ps1','-Root','<wt>\src\core','-Force'` in D2's shape. Only rc 0 is OK. ANY other code is a STOP (clean-bin :34-43: 1, 2, 3 and 4 all mean something was left).
   - After a purge, assert `<wt>\src\go2cs\bin\go2cs.exe` still exists, and record a bin/obj/Generated SIZE census. Under 25 GB on either drive after a purge is a STOP.
   - go2cs.exe count by image path must be 0.
   - For `net` (and any row SW-4 adds): Q5's hook, then its gate.
2. **RUN.** Write `rows/<row__>/INFLIGHT` holding the row start time. Then:
   `$p = Start-Process powershell -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','<wt>\src\run-validated-sweep.p2-<host>.ps1','-Filter','<row>','-Exact','-SkipBuild' -NoNewWindow -PassThru -RedirectStandardOutput '<evidence-root>\rows\<row__>\sweep.log' -RedirectStandardError '<evidence-root>\rows\<row__>\sweep.err'; $null = $p.Handle; $done = $p.WaitForExit(<budget-ms>); $rc = $p.ExitCode`.
   - `$null = $p.Handle` is what makes `ExitCode` readable on 5.1. WaitForExit, never `-Wait`: on 5.1, `-Wait` also waits for descendants such as a lingering compiler server. `-NoNewWindow` keeps the child on the driver's hidden console (TestCtrlBreak). The reap rule concerns the LAUNCH from the session, not this inner child. The logs are the child's console bytes, NOT UTF-16LE, so bash and python read them directly.
   - No `-TestTimeout`: the default 10m applies, raised to each floor as max(asked, floor) (sw :1092-1099; COORD RULING NEEDED, SW-7).
   - **crypto/tls ONLY:** read `go env GOFLAGS` (empty), set `$env:GOFLAGS='-timeout=40m'`, add `'-TestTimeout','90m'`, and remove it in the row's `finally` with `Remove-Item Env:GOFLAGS -ErrorAction SilentlyContinue`. Then read `go env GOFLAGS` again: it must be empty (owner ruling, LEDGER 2026-09-23 11:59: "the row environment only"; COORD RULING NEEDED, SW-1).
   - **THE PER-ROW BUDGET** (Budgets): `<budget-ms>` = `4 × pkgTimeout + 30 min`. If WaitForExit returns false, the word is TIMEOUT:
     - read the results tail FIRST (floor 14);
     - kill the child's OWN tree by its recorded PID, from the driver: `taskkill /T /F /PID $p.Id`, which is PowerShell, so there is no MSYS path conversion. Never by name (floor 5);
     - census for survivors with Get-CimInstance: ExecutablePath under `<wt>`, or a CommandLine naming `run-validated-sweep.p2-<host>.ps1`. Exclude the driver's own PID and its parents: a kill query matches its own shell (gate-forensics SKILL :1182-1186). Any survivor is a run-level STOP, reported, never killed. Log the kill line only AFTER the PID is confirmed dead (gate-forensics SKILL :1221-1222);
     - then run the START GATE's steps 3-5 and continue. COORD re-dispatches the row at a raised budget (rb :3499).
3. **PARSE**, over BOTH `sweep.log` and `sweep.err`.
   - The verdict words are PASS, COUNT, DISC, CVAC, FAIL, ORACLE and N/A. EXACTLY ONE indented line must match `^  (PASS|COUNT|DISC|CVAC|FAIL|ORACLE|N/A) +(\S+) ` with the second token equal to `<row>` (the label is padded to 34 columns, sw :1089; never anchor at column 0), plus its `[Ns]` wall and the `sweep:` summary line.
   - RERUN is NOT a verdict word. Count `^  RERUN +<row> ` separately; it may appear 0 or 1 times (sw :1160). When it is 1, the final line must be ORACLE, or the `        ORACLE FLAKED ONCE` note must follow the final line (sw :1323-1325; the verdict is run 2's).
   - The execution suffix: the PASS or COUNT line carries ` [<Execution>]` exactly when the row's roster `Execution` is non-empty, so ` [release-tiered]` on internal/godebug and log/slog and on no other row (sw :1125, :1249, :1302). Those two headers must read `  1 row(s) carry a per-row execution config: <row> [release-tiered]` (sw :319-320), and no header may contain `an A/B measurement` (sw :323-324). Together these prove the pin applied without `-TestConfig`.
   - Scan both files for the refusal markers `DISK PREFLIGHT`, `Toolchain pin`, `No banked packages matched`, `Converter not built` and `converter build failed`. A refusal is a `throw` (sw :214, :244, :345), so it lands in `sweep.err`. rc alone cannot tell a failing row from a refused run.
   - `gotDisclosed` comes from the row's comparison record: the count of its `disclosed` array, read with `_roster.ps1`'s own ConvertFrom-ComparisonRecord (ro :1826-1926). The sweep prints the converter's `K disclosed-divergent` clause (sw :542-549) only on the DISC and absorption lines, not on a plain PASS.
4. **EVIDENCE, BEFORE any restore, for EVERY row.**
   - Copy `<wt>/src/core/<row>/go2cs_test_comparison.json`, `go2cs_test_results.json` and `go2cs_test_results.xml` into `rows/<row__>/` (gate-forensics :1383-1395) ONLY IF PRESENT. Record each file's mtime against the row start. An absent file is ABSENT; one older than the row start is STALE, never this row's evidence (the ignored records of a killed attempt persist, src/core/.gitignore :18-21).
   - On any non-PASS, read the results tail FIRST (floor 14): the last 400 lines or 256 KB, bannered.
   - Save `git -C <wt> diff --numstat -- src/core` and `git -C <wt> status --porcelain`, both UNFILTERED.
   - Copy the rewritten `docs/validation/current/<row-dotted>.md` and `git -C <wt> diff -- docs/validation` into `rows/<row__>/`, so a reader bug can be re-read offline rather than by re-running rows.
   - **THE PAGE READING.** Compare the rewritten page against `git show HEAD:` of the same path:
     - (i) the `## Verdicts` table's names and verdict cells, read with the sweep's OWN reader (heading ``^##\s+Verdicts\b``, row ``^\|\s*`([^`]+)`\s*\|``, sw :583-584);
     - (ii) the counts line, parsed with the digit-only regexes `\*\*(\d+) matched` and `(\d+) disclosed`. U+00B7 appears on both that line and the excluded date line. Spell it `[char]0x00B7` wherever it is matched literally, as ro :57-63 does; a BOM-less 5.1 script is decoded as ANSI.

     Exclude the volatile `*Validated <date> · converter <sha>*` line and the oracle/config line. Record EQUAL, MOVED with every moved name, or NOT REWRITTEN when the page's mtime predates the row start. NOT REWRITTEN is never EQUAL (gate-forensics :408: the mtime tells swept from stale).
   - **crypto/rsa ONLY (SW-rsa).** The row runs exactly like every other row, with no binlog and no `/p:ReportAnalyzer`: it IS the unchanged re-run (gate-forensics SKILL :1761-1767). On any non-PASS, grep `sweep.log`, `sweep.err` and every copied file for `CS8785` and `CS9248`, and quote the CS8785 diagnostic TEXT, which names the throwing frame (:1743). Before restore, copy the row's generated-source trees (`Generated` and `obj/**/generated` under `src/core/crypto/rsa`) into `rows/crypto__rsa/generated/`. If no diagnostic text appears in any file, report "CS8785 text not captured", never "did not fire".
5. **RESTORE BOTH ROOTS** (validation-bank :1003; gate-forensics :1446-1453). The run rewrote the row's page, the shared `docs/validation/index.md`, the package README and its test artifacts.
   - `git -C <wt> checkout -- src/core docs/validation`, and check its exit code.
   - `git -C <wt> ls-files --others --exclude-standard -z -- src/core docs/validation > <scratch>/p2/<host>/untracked.z`, and check its exit code BEFORE removing any path. Then remove exactly the listed paths, by explicit path. Never a glob, never `-x`.
   - Assert the porcelain shows only the per-run copy, `^ D` is empty, and the `-z` tracked count equals S0's.
6. **LEDGER.** Append one LF-only UTF-8 (no BOM) `ledger.tsv` row with the §3.4 fields (rb :3381-3384): package, shard, host, kind (`verdict`, or `reading` for a named extra reading), attempt, start, end, outer wall, the sweep wall `[Ns]`, deadline used, word, got, gotDisclosed, the banked Tests and Disclosed values, absorption class or `none`, page reading, drift class, the log path RELATIVE to the evidence root, `<sweep-base>` as both the corpus commit and the converter commit, the converter sha256 and mtime, the C-toolchain capability (Q2), and rc. NOT MEASURED is a first-class word, never a pass or a fail (rb :3387). Then delete `INFLIGHT`.
7. **END.** Assert `processed == listed`. The outer finally then prints `DRIVER_EXIT=<n>`. It is 0 only when every row ran and every word parsed; the verdict score belongs to P, not to the driver.

**LAUNCH, from a foreground Bash call:** `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -Command "(Start-Process powershell -WindowStyle Hidden -PassThru -WorkingDirectory '<evidence-root-winpath>' -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','<driver-winpath>' -RedirectStandardOutput '<driver.log-winpath>' -RedirectStandardError '<driver.err-winpath>').Id" < /dev/null > <scratch>/p2/<host>/driver.pid`, then `rc=$?`. Every path goes through `cygpath -w`.

## P -- THE PASS CRITERION PER ROW
A row is DISCHARGED for P2 only when all three hold:
- **P-1: the sweep's own verdict.** rc 0; exactly one final verdict line (D3); word PASS; `got` == the Tests cell in the plain form `PASS <row> <got>[ [<Execution>]] [Ns]` (sw :1249), where the bracketed suffix is present exactly when the row's roster `Execution` is non-empty: ` [release-tiered]` on internal/godebug and log/slog, and on no other row. The only other admissible forms are the named absorptions in the table below, each reported by class.
- **P-2: the page reading (COORD RULING NEEDED, SW-5).** DEFAULT: the `N matched · D disclosed` line is GATING, because the windows columns path never compares D (ro :1097). It is judged against the run's OWN proven reading, never against HEAD's page alone, because every named absorption rewrites that line:
  - (a) the page's N and D equal the run's `got` and `gotDisclosed` (D3). A mismatch means the page is not this run's reading, which is a finding.
  - (b) the banked Tests and Disclosed equal the page's N and D after the named absorption's PROVEN delta is removed. The delta is recorded by class. With no absorption, they are equal. path/filepath, host-conditional: N − e = Tests, where e is the extras count sw :1253 prints (6 on a privileged host). os/exec, host-conditional-disclosure: N + k = Tests and D − k = Disclosed, where k is the fired count sw :1261 prints. In (i), the only admissible MOVED names are that absorption's own entries: the six `TestWalkSymlinkRoot/*` names, or `TestExtraFiles`. crypto/tls in a non-FULL state is not a discharge anyway (SW-1).
  - A verdict-pair move with both counts equal is a READING. It is classified against the count-neutral host-state list (os/exec TestString with `echo` on PATH; os/signal TestCtrlBreak without a console; os TestStatLxSymLink) or posted UNCLASSIFIED.
- **P-3: drift-clean.** The pre-restore numstat must classify ONLY as validation-bank's classes 1-3: CRLF phantoms, `-tests` closure files, and `.cs.auto` siblings.
  - REAL DRIFT is a STOP-finding: class 4, any production `.csproj` change, or a production `.cs` numstat outside those classes. COORD controls it at `fa18863b94` before charging it (validation-bank :1064-1066; COORD RULING NEEDED, SW-10).
  - The committed TEST artifacts a `-tests` run re-emits (`*_test.cs`, `package_test_info.cs`, `go2cs_test_host.cs`, `<pkg>.tests.csproj`) are classified too. At steady state, a content numstat on one is REAL DRIFT unless it matches a named ONE-WAY class (validation-bank :1030-1032: the `-tests` init-forcing hook appears at a row's next test-source regeneration and stays). It is controlled at `fa18863b94` like production drift.

NAMED ABSORPTIONS. The sweep counts each of these as a PASS, and warns that "a full sweep must not report their banked verdicts as re-validated when they were not" (sw :355-359).
| row | admitted form | discharge |
|:--|:--|:--|
| crypto/tls | FULL: plain `PASS 4759`, TestBogoSuite pass/pass | FULL discharges. host-limit (`- 3419 (TestBogoSuite host-limit disclosed ...)`, sw :1275; 1,340 + 2) and capability-absent (sw :1266; 1,340) are sweep PASSes but NOT discharges: a STOP-finding by state (COORD RULING NEEDED, SW-1) |
| path/filepath | plain 61 (no privilege; predicted on R), or `61 banked + 6 host-conditional` (sw :1253) | discharges either way; the state is named, and P-2(b) is read with its delta |
| os/exec | plain 116, or `- k host-conditional disclosure (TestExtraFiles fired ...)` (sw :1261) | discharges; the state and its matched delta are named (M4), and P-2(b) is read with that delta |
| any row | PASS after the sweep's one automatic oracle re-run (sw :14-21) | discharges, NAMED. Copy `<wt>/scratchpad/sweep-oracle-flake/` out before cleanup |

## MOVED VERDICTS AND STOP CONDITIONS
- **ROW-LEVEL FINDINGS.** Record the row, preserve its evidence, restore BOTH roots, and CONTINUE the shard (DEFAULT, COORD RULING NEEDED, SW-6). The findings are:
  - COUNT, DISC, FAIL or CVAC;
  - ORACLE-unstable;
  - an N/A on windows;
  - NOT MEASURED or TIMEOUT;
  - a page counts line MOVED;
  - REAL DRIFT;
  - crypto/tls in a non-FULL state. Message COORD about this one AT ONCE, not only in the report.

  Report each BY NAME with its first error, taken from the results tail FIRST ("a long wall is not a TIMEOUT", rb :2773). Classify only against the documented classes, or post it UNCLASSIFIED. It means "the table and reality must agree; one of them is now wrong" (sw ~:1290-1299): a finding, never absorbed.
  - NEVER re-run it with changed parameters, edit the roster or a page, re-bank, or fix. Re-banking is H10's per-package pipeline act (rb :2203).
  - ORACLE-unstable is "a host to qualify ... never a corpus regression, and never a green gate" (sw :1570-1574). Run `go test -count=1 -timeout 40m <pkg>` once on this box, but only AFTER `DRIVER_EXIT`, in the foreground with env.sh sourced and before CLEANUP, and report it. It is never run beside a live driver: that would be two batteries on one box (floor 1).
  - NOT MEASURED and TIMEOUT go back to COORD for a RAISED-budget re-dispatch (rb :3499) on S-7 or the other lane. A re-dispatch starts on a box only after that box's own `DRIVER_EXIT`.
  - **Verdict of record** (M2). A raised-budget re-dispatch of a NOT MEASURED or TIMEOUT row, and the SW-1 crypto/tls fallback, REPLACE the origin row's verdict of record: the origin ledger keeps its word, marked superseded. A second-host run of an ORACLE-unstable, COUNT, DISC or FAIL row is a READING only, and the origin verdict stands.
- **RUN-LEVEL STOPS.** Stop the driver at the next row boundary, restore, and report. The driver's own mechanism is the exit after restore; COORD's is the `STOP` file (D1). Nothing is killed except D2's budget kill of the driver's OWN child tree by recorded PID. Every other process, go2cs.exe and dotnet.exe included, is counted by image path and command line and never killed. The triggers:
  - any refusal marker, in `sweep.log` or `sweep.err`;
  - a stray go2cs.exe at a row start, or a survivor after a budget kill;
  - `^ D` non-empty, or the tracked count moved, after a restore;
  - the final verdict line missing or doubled (RERUN is not a verdict line, D3);
  - Q5's gate failing, or the IPv6 binding differing from the bank's;
  - THREE consecutive non-PASS rows with one first-error class, which means host breakage, not verdicts;
  - a host fault (bugcheck or reboot);
  - any second battery starting on the box, or a sibling's `dotnet build-server shutdown` or name-scoped kill (Rules);
  - free disk under 25 GB after a purge;
  - a path-length error.
- **Consequence (COORD's, not the worker's).** Any undischarged row keeps P2 RED, which blocks the cutover and so the release (rb :3581-3583; LEDGER 2026-09-24 00:04). A later fix owes `-Filter <pkg> -Exact` at the merge RESULT (validation-bank :33-34), and it re-opens SF and the release brief's C6. A demotion follows H10's close step 5: the row deleted, its page `git rm`'d, and its badge set orange by hand.

## SHARD S-R -- R-LAPTOP
- Run Q1-Q4 and Q6. The box must be IDLE of every other battery: R's post-hop worktrees are quiet, and its WSL is unused. The survival sleeper (Rules) runs and is read BEFORE row 1, so a failed canary never costs the crypto/tls row. When COORD says, the evidence ref is `claude/r-p2-sweep-evidence`.
- **Row 1 is `crypto/tls`, in D2's raised-wall form.**
  - Predicted: `PASS crypto/tls 4759` in ~1,761 s (the bank's wall; the 09-22 evidence read 2,216 s).
  - The 90m ask exceeds the 60m floor. GOFLAGS is the only thing that reaches BoringSSL's nested `go test`, which otherwise inherits Go's 10m default.
- **Then 88 rows**, `internal/gover` .. `weak`, minus internal/trace, net and os.
  - The heaviest: time 592 s (the i7's wall; floor 40m), regexp 384 s (0.64 of the 10m ask), os/exec 223 s, sync/atomic ~184 s (floor 150m; its 5,868 s wall is linux-only), testing/iotest 136 s, unicode/utf8 120 s, os/user 119 s, and log/slog 119 s (release-tiered).
  - Predicted: path/filepath plain 61. The shard totals 6,454 s + ~110 s.
  - os/user: read any non-PASS against Q2's domain-joined boolean, with the elapsed times as the tell (a domain-trust timeout reads ~20 s against ~1 s), BEFORE classifying it (measurement-discipline SKILL :374-381). Never quote a failing line that carries a domain, account, profile path or SID.

## SHARD S-G -- G-LAPTOP (windows side)
- Run Q1-Q4. The WSL arm stays IDLE for the whole shard (one battery per box). G's pwsh 7.5.4 is a net10 tool, and dotnet 9.0 sits bare on the windows PATH, so the driver's own dotnet10 pin is load-bearing: read `dotnet --version` INSIDE the driver. When COORD says, the evidence ref is `claude/g-p2-sweep-evidence`.
- **126 rows**, `archive/tar` .. `internal/godebugs`, minus crypto/tls.
  - The heaviest: go/internal/gcimporter 337 s, crypto/internal/fips140/edwards25519 325 s, hash/maphash 282 s (floor 60m), go/parser ~278 s (floor 90m), go/types ~256 s, crypto/x509 200 s (TestHybridPool needs outbound internet; a skip is count-neutral), index/suffixarray ~192 s (floor 120m), and internal/godebugs 140 s. The shard totals 6,495 s + ~110 s.
  - internal/godebug runs release-tiered.
  - internal/coverage/cfile is predicted 15 + 1; the junction seat, PackageAncestry.cs:593-640, is at `<sweep-base>`.
  - go/internal/srcimporter is predicted PASS 7 (34 s at G's recon). A split is a host-timing finding under the 22:37 post-hop ruling, never T5.
  - crypto/rsa carries the folded generator-transient re-run (SW-rsa; D4's crypto/rsa step).
  - **IF SW-18 rules (b):** after `internal/godebugs`, one extra READING of `crypto/tls` at the STANDARD wall. There is no GOFLAGS and no `-TestTimeout`, so its 60m floor applies. This is G's recorded host-limit state (885 s, sw :986). Its verdict line must read `- 3419 (TestBogoSuite host-limit disclosed; capability PRESENT, converted side over the deadline)` (sw :1275). It is logged with kind `reading`, never as crypto/tls's verdict of record (S-R holds that), and it needs the network (Q6). It adds ~885 s to S-G.
- LongPathsEnabled=0, so keep `<wt>` short: MSBuild's project loader fails past about 260 characters regardless.

## SHARD S-7 -- the i7 (the residual; after Part C, before the cutover)
- **Start conditions.** Start only after Part C's READINESS REPORT, and only on COORD's word. First, COORD reclaims `abpost-stage`, `abclose`, `abclose-stage`, `abh12-dry` and RN-7's `abh12-clone`, children-first (floor 12; the close's control dipped to 22 GB). The i7's pwsh 7.4.6 runs NOTHING with the dotnet10 pin in the parent, so use 5.1 throughout. The machine GOROOT is the dead 1.23.1 pin.
- **Qualification.** Run Q1-Q4; Q3 MUST show the privilege, or os is not run and is reported. Q5 (the IPv6 reading, then the two runs) runs as the driver's pre-row hook for `net`.
- **No survival canary.** The i7's sub-agent ends when it returns, so it cannot end a turn and wake again. It launches the driver by the same LAUNCH shape, then WAITS for `DRIVER_EXIT` by a POSITIVE poll: repeated bounded foreground until-loops (each under the tool's 600 s cap) or Monitor, each checking `driver.log` for `DRIVER_EXIT=` AND the driver PID by a census on image path and command line. It never uses a bare foreground sleep, and it returns its REPORT only after `DRIVER_EXIT` and CLEANUP. Alternatively, COORD adopts the poll after the launch.
- **Rows, in this order:**
  - `os`: predicted PASS 1105, ~78-120 s;
  - `internal/trace`: predicted PASS 92 with TestTraceCPUProfile pass/pass, ~153 s;
  - `net`: predicted PASS 476 with every DNS test pass/pass; 332 s at 8f, floor 120m.
- **FALLBACK.** S-7 takes COORD-ordered re-dispatches from S-R and S-G: NOT MEASURED or TIMEOUT at a raised budget, and oracle-unstable read on a second host. Each is logged with its origin shard named, and each starts only after S-7's own `DRIVER_EXIT`. The verdict-of-record rule (MOVED VERDICTS) applies: a raised-budget re-dispatch REPLACES the origin verdict, and a second-host run of an ORACLE-unstable, COUNT, DISC or FAIL row is a READING (COORD RULING NEEDED, SW-6).
- The evidence goes to a local branch. COORD pushes it.

## CLEANUP (each shard, after `DRIVER_EXIT`; every command env.sh-sourced)
1. Copy `<wt>/scratchpad/sweep-oracle-flake/` into `<scratch>/p2/<host>/`.
2. Run `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <wt>/src/clean-bin.ps1)" -Root "$(cygpath -w <wt>/src/core)" -Force < /dev/null > <scratch>/p2/<host>/cleanup-purge.log 2>&1`, then `rc=$?`. Gate on rc 0; ANY other code is a STOP (clean-bin :34-43). Delete the per-run copy.
3. Print `git status --porcelain` UNFILTERED; it must be empty, with no ` D`. Confirm that the `-z` tracked count equals S0's count (floor 7's shape, S0.1). Re-take the ignored-residue census. Under S0.1's scope, the EXPECTED residue is NOT zero. It is the ignored per-run artifacts clean-bin does not remove: the staged `*.go` copies and `go2cs_test_manifest.json`, `go2cs_test_comparison.json`, `go2cs_test_results.json` and `go2cs_test_results.xml` under src/core (src/core/.gitignore :11-21). Any other `!!` path is a STOP, named.
4. `git -C <repo> worktree remove <wt>`, then `rc=$?`, then `worktree prune`. Take the PARENT test AT THE ACT: a tree whose `--git-common-dir` equals its `--git-dir` and whose `worktree list` has more than one row is never removed.
5. Release the lock. KEEP `<scratch>/p2/<host>/` and the local branch until COORD's M9.

## EVIDENCE COMMIT (only when COORD says; SW-11)
- NEVER from the swept `<wt>` (floor 8). Cut a SEPARATE fresh worktree off `<sweep-base>` on the local branch `claude/<lane>-p2-sweep-evidence`, and assert that its porcelain is empty.
- Copy from `<scratch>/p2/<host>/` into ONE directory, `docs/phase4/p2-sweep/<host>/` (ledger.tsv, a README, the per-row logs and records). In every text file, replace `<goroot>`, `<dotnet10>` and every profile path by their placeholders. Every ledger path is already relative (D6). Write every text file as UTF-8. The census cannot read UTF-16 (harness-gates :407), so run the NUL-count tell on each file first.
- Extract the census with its two siblings: `git -C <repo> archive 074a12c4ae .claude/coord-scripts | tar -x -C <scratch>/idc`, then `rc=$?`. The script loads `coord-identifier-patterns.txt` and `coord-identifier-hashes.txt` from its OWN directory (coord-identifier-census.sh :233-236). Run `<scratch>/idc/.claude/coord-scripts/coord-identifier-census.sh entry <file>` on EVERY file to be staged, each in its own command, gated on rc.
- Stage with `git add -- docs/phase4/p2-sweep/<host>` ONLY, never `-A`. Assert that `git diff --cached --name-only` lists only that directory, and that the unfiltered porcelain shows nothing else.
- Commit unsigned. ANNOUNCE, then push to your OWN lane ref only (floor 9). S-7's evidence stays local, and COORD pushes it.

## REPORT (one per shard; the final message, then STOP)
- **HEADER:** shard, host, `<sweep-base>` as asserted, the converter sha256 values (plain and identity), start and end times, and `DRIVER_EXIT`.
- **Q readings:** pins by output (GoTargetOS and the DOTNET_/COMPlus_ names included); CPUs, RAM and uptime; disk on both drives before and after; LongPathsEnabled; domain-joined (boolean); the C-toolchain capability; COORD's machine-global-hazards assertion; the functional symlink reading; `echo`; for a shard carrying `net`, the IPv6 binding and both net-qualifier runs (rc, tail, failing set), with the five criterion arms; the survival sleeper's reading (or S-7's exemption).
- **The list:** its count, the boundary names, and `processed == listed`.
- **Per row:** every verdict line and `sweep:` summary line, verbatim, plus the ledger.
- **THE SCORE:**
  - the discharged rows;
  - each named absorption, with its state;
  - each row-level finding BY NAME, with its first error, its class (or UNCLASSIFIED) and its evidence path;
  - any run-level STOP.
- **Σ got** over the shard beside Σ banked Tests over the same rows, with every difference named.
- **Pages:** the EQUAL count, every MOVED name, every NOT REWRITTEN row, and P-2(a)/(b) per row, with each absorption's delta named by class.
- **Drift:** the class per row, and every REAL DRIFT hunk quoted CR-stripped.
- **Walls:**
  - every floored row's wall against 0.75 × its floor;
  - every unfloored row whose wall is 450 s or more (0.75 × the 10m ask; a verdict wall covers all children, so it overstates any single one).

  Each one is flagged, never acted on.
- **Deferred-alloc rows in the shard.** At `fa18863b94`, 29 manifests carry `"class": "deferred"`: bufio, bytes, context, crypto/ed25519, crypto/internal/fips140test, crypto/md5, crypto/rand, crypto/rsa, crypto/sha1, crypto/sha256, crypto/sha3, crypto/sha512, database/sql, encoding/binary, fmt, io, log, log/slog, math/big, mime, net, net/http/internal, net/netip, os, slices, strconv, strings, unicode/utf16 and unicode/utf8. Re-derive the list at `<sweep-base>` by `git grep`, and list the paths of their comparison records.
- **orphanedDisclosures** per row, read from each comparison record. Non-gating (testConversion.go:7236-7272).
- **crypto/rsa's row:** the folded generator-transient re-run (D4): its verdict line, and on a non-PASS the quoted CS8785 text or "CS8785 text not captured", with the preserved generated-tree path.
- **Disk, purges and SIZE censuses**, plus the cleanup assertions, every START-gate restore (with its §3.6 classification), and every budget kill with its survivor census.
- Everything not verified, stated as such.

## M -- COORD'S MERGE OF READINGS (COORD's work; P2 is read HERE and nowhere else)
- **M1 (before launch).** At S-R/S-G launch: S-R's and S-G's printed lists and S-7's FIXED name list are pairwise disjoint, and their union == the 218 rows parsed at `<sweep-base>`. At S-7's launch, re-assert the check with S-7's PRINTED list. PLANT: a list with one row duplicated and one list with a row dropped must each red the check.
- **M2 (coverage).** Over the three ledgers, `roster − logged` is EMPTY, and every package appears in EXACTLY ONE ledger as its verdict of record, under the verdict-of-record rule (MOVED VERDICTS): a raised-budget re-dispatch or the SW-1 crypto/tls fallback REPLACES the origin row's verdict, and a second-host run of an ORACLE-unstable, COUNT, DISC or FAIL row is a READING. Readings (kind `reading`, fallback readings, SW-18's extra crypto/tls reading) are marked as such and never counted. This is §5's "exactly one shard ledger", and §3.5 calls a row missing from every ledger the campaign's one unrecoverable failure mode. A missing row is re-run inline before P2 is read.
- **M3 (arithmetic).** Plain PASS + each named absorption (counted apart) + COUNT + DISC + FAIL + CVAC + oracle-unstable + NOT MEASURED = 218, with N/A = 0.
- **M4 (matched sum).** Σ got over the 218 rows == 56,974 when no absorption fires. Name every absorption's delta: path/filepath +6 when privileged, os/exec −k when fired, crypto/tls −3,419 in a non-FULL state. Report both P2 numbers: absolute 218 (≥ the prior 204), 218/230 = 94.8% and 218/224 = 97.3%.
- **M5 (converter).** The identity sha256 is EQUAL across the three shards and equal to A2(4)'s.
- **M6 (pages and drift).** Every page reading is EQUAL, or per SW-5 with P-2(a) and (b) holding. NOT REWRITTEN is never EQUAL. Every production drift class is 1-3, and every test-artifact numstat is class 1 or a named ONE-WAY class (P-3). Any REAL DRIFT is controlled at `fa18863b94`.
- **M6b (the manifest half of rb :3587).** It rests on H10 step 3: the orphan census over all 48 manifests (LEDGER 2026-09-22 15:59, `f4903fad5f`) and the re-sign (16:22, `082ec41bb0`). Its live cross-check comes from this sweep: every row's `orphanedDisclosures` from its comparison record, plus every `go2cs_test_disclosures.json` committed after `082ec41bb0`, named by `git log` at `<sweep-base>`, beside its row's live reading. A non-empty windows orphan set is named in the P2 line for COORD's ruling. It is not a worker's classification.
- **M7 (walls).** Walls against floors, by the RULED rule: RAISE when the largest wall is ≥ 0.75 × the floor (sw :968-975; LEDGER 2026-09-23 11:51). A breach is a POST-sweep floor seat, never an in-sweep edit. The P2 ledger line also corrects the close STAMP's inverted wording, "every floor sits below 0.75 of its largest wall" (COORD RULING NEEDED, SW-8).
- **M8 (d14).** Compare each deferred entry's note against its reading by hand, from the preserved comparison records (h10-close-obligations.md:121).
- **M9 (record).** Write the ledger P2 line, with the time read from `date`, and accept the evidence home (COORD RULING NEEDED, SW-11). The walls go into a labelled block in DATA-sweep-row-walltimes.md (one section per OS, SHA and machine, with a digest). Then reclaim the scratch, and delete each shard's local branch.
- **VERDICT.** P2 is GREEN only when M2-M6b hold and every row is discharged. Otherwise P2 is RED, with every undischarged row named.

## SF -- WHY A SWEEP AT `<sweep-base>` STANDS FOR THE RELEASE TREE
**Inputs.** A row's verdict is a function of exactly these:
- the converter: `src/go2cs` non-test files, embedded assets included;
- `src/core`: corpus, golib, manifests and committed test sources;
- `src/gen`;
- `src/Directory.Build.props`, `src/.editorconfig` (its C# analyzer severities feed every build), and `src/version.props`'s GoStdLibVersion;
- the instrument: `run-validated-sweep.ps1`, `_paths.ps1`, and the functions of `_roster.ps1` that the sweep calls, with the script-scope variables they read;
- each row's WINDOWS expectations in `docs/ValidatedTestPackages.md`;
- HEAD's `docs/validation/current` pages, which the absorption arms read;
- `.gitattributes` (fixture EOL);
- the host, which Q records.

**What moves between `<sweep-base>` and the release tree** (master after the cutover merge; RN-12/13):
1. **Part B's census seat `<census-seat-ref>`** (`869d8e4683` at the 00:23 acceptance: `_roster.ps1`, `check-roster-format.ps1`, `push-nuget.ps1`; +880/-143; the owner's 01:25 ruling adds a §2e guard change that rides the same ref). The sweep calls 11 `_roster.ps1` functions, and all 11 are byte-identical at `869d8e4683`; so are the two exclusion readers, which the sweep never calls. It adds 7 functions and 2 script-scope variables (`$RosterLivingProofLinkPattern`, `$RosterFrozenProofLinkPattern`) that neither the sweep nor `_paths.ps1` references, and it changes `ConvertTo-FrozenRosterText`, which only the freeze uses. It is real CODE in `_roster.ps1`, which is why SF-5 no longer reads that file (SF-3 does).
2. **G's `836004dd20`**, `docs/ValidatedTestPackages.md` +2/-2: the header's linux line, plus crypto/tls's linux annotation and prose. The windows cells stay 4759 | 1.
3. **Part C's changes:**
   - the C4/C5 prose and the owner's 00:25 docs migration;
   - the hand-owned README lines (C2);
   - IF RN-10 admits them, crypto/tls's manifest `reason` and `hostConditional` prose. The converter reads `hostConditional` only for non-emptiness (testConversion.go:7889) and matches on `signature` and `hostConditionalSignature`;
   - IF RN-10 admits them, the comment-only lines at ro :1142, 1168, 1240, 1242 and 1400-1401, and sw :630.
4. **Master's side**, 13 commits from `9e12c3e7d0` to `074a12c4ae`: coord-scripts, `.claude/rules/docs-records.md`, `regen-validation-index.py`, and `fleetIdentifierCensus_test.go` (a `_test.go`, so not in go2cs.exe).
5. **The owner's run:** GoBuildNumber 0 -> 1, `docs/validation/1.24.13.1/`, and the retargeted badges.
6. **d15, the gofmt pass** (h10-close-obligations.md:122, "After the close, before the cutover merge"; the release brief's PAR-5, "cutover"; RN-15 rules its timing). It REWRITES CONVERTER SOURCE. Read-only at `fa18863b94` (a CR-stripped copy, go1.24.13 `gofmt -l`), 9 non-test files are unclean: convBinaryExpr, convExpr, deferFinallyLowering, manualTypeOperations, positionMapOperations, testConversion, visitCommClause, visitSelectStmt and visitTypeSpec. Nine `_test.go` files are unclean as well. Several changes move line counts, so the line tables and SF-6's identity sha move too. Landed after the sweep, it reds SF-1 and SF-6. Pre-ruled by SW-17.
7. **d7 / PAR-3, the H6 audit's `.cs.auto` refresh and any T4 hunks** (h10-close-obligations.md:114, "The parity gate"). A `*.cs.auto` sibling is not a `*.cs` Compile item: it is a review sibling (validation-bank class 3, :1052-1054), and no project file compiles one at `fa18863b94` (a `git grep` over `*.csproj`, `*.props`, `*.targets` and `*.projitems` finds it only in comments). SF-1 therefore excludes it. Any T4 hunk that touches a compiled `.cs` still reds SF-1.
8. **c6, the crypto/cipher manifest retire** (h10-close-obligations.md:86; RN-15: pre-release or post-hop). It is a CONDITIONAL mover. Landed pre-release, it reds SF-1 on `src/core/crypto/cipher/go2cs_test_disclosures.json`, and its remedy is `-Filter crypto/cipher -Exact` at the release tree, with that row's P-1..P-3.
9. **The owner's 00:25 docs migration**, if it edits any converter template or embedded asset under `src/go2cs` (the order names "tooling and templates"). That is a converter mover: SF-1 and SF-6 red.

Items 1-5 reach no verdict input. Items 6-9 each do, if they land after the sweep: they are gated by SF and pre-ruled by SW-17. Part A's CNR, `NO REGRESSION` with CHANGED EMPTY, also shows that single-package emission at `<sweep-base>` equals `fa18863b94`'s, so these readings stand for the close tree too, which is why `fa18863b94` is the drift control. Post-hop work stays out until the release (LEDGER 2026-09-24 00:45: R's re-entry train "would invalidate the §6 sweep").

**THE GATE.** COORD runs it read-only at the release tree before CP3's `-VerifyOnly` and the owner's run, and again whenever anything lands after that.
- **SF-1:** `git diff --name-only <sweep-base> <release-tree> -- src/go2cs src/gen src/core src/Directory.Build.props src/.editorconfig src/_paths.ps1 src/run-validated-sweep.ps1 docs/validation/current .gitattributes ':(exclude)src/go2cs/*_test.go' ':(exclude)src/core/*README.md' ':(exclude)src/core/crypto/tls/go2cs_test_disclosures.json' ':(exclude)src/core/*.cs.auto'`, then `rc=$?`. It prints NOTHING, except `run-validated-sweep.ps1` when SF-5 admits it. The `.cs.auto` exclusion is item 7's reason. A T4 hunk in a compiled `.cs` still prints. PLANT: a one-line change to a compiled `.cs` beside a `.cs.auto` in a scratch diff must still print.
- **SF-2:** in crypto/tls's manifest, every entry's name, class, signature and hostConditionalSignature is byte-identical at both trees, and `hostConditional` is still non-empty (a JSON field compare of the two `git show` blobs). PLANT: a one-byte signature change must red it.
- **SF-3:** read EACH tree's own `_roster.ps1` in its OWN `powershell -NoProfile` process that writes JSON, then compare the two files. The two trees define the same function names, so they cannot be dot-sourced into one session. The checks:
  - (a) the 218 windows tuples from `Get-ValidatedRosterRows` (Package, Expected, Disclosed, Execution, the Conditional names AND the ConditionalDisclosures names) are EQUAL;
  - (b) the AST text of the 11 sweep-called functions is byte-identical: Get-SweepTargetGoos, Get-ValidatedRosterRows, Get-RosterRowExpectation, Get-RosterExecutionArgs, Get-SweepRowClassification, Test-CapabilityAbsentDelta, Test-HostConditionalDisclosureDelta, Test-HostLimitDelta, Test-OracleOnlyFailure, Get-ResultsTailText and ConvertFrom-ComparisonRecord. The two exclusion readers are compared too, though the sweep never calls them;
  - (c) the VALUE of every script-scope variable `_roster.ps1` defines at `<sweep-base>` is equal at the release tree. That includes `$RosterRowPattern`, `$RosterConditionalPattern`, `$RosterConditionalDisclosurePattern`, `$RosterExecutionValues`, `$RosterExecutionPattern`, `$HostLimitDisclosureClass`, `$HostLimitGoRootVerdicts`, `$OracleOnlyDivergencePattern`, `$OracleOnlyToleratedErrorPrefixes` and `$OracleOnlyKillSignatures`, enumerated from the file rather than from this list. New variables are reported by name, and the only ones admitted are the census seat's two.

  PLANTS, each in a scratch copy: one Tests cell changed; one character changed in `$RosterExecutionPattern`. Each must red SF-3.
- **SF-4:** `src/version.props` GoStdLibVersion reads 1.24.13 at both trees (the sweep's toolchain guard throws otherwise). GoBuildNumber may differ only by the owner's `-BumpBuild`.
- **SF-5:** if RN-10 admitted comment edits, every changed line in `git diff -U0 <sweep-base> <release-tree> -- src/run-validated-sweep.ps1` is a `#` comment. For `_roster.ps1`, every changed line in the diff from `<census-seat-ref>`'s blob to the release tree's blob is a `#` comment, because the seat's own code is SF-3's. `_paths.ps1` admits nothing: any change reds SF-1. PLANT: a code-line change in a scratch diff must red it.
- **SF-6:** the `go build -trimpath -buildvcs=false` of `src/go2cs` at the release tree has a sha256 EQUAL to M5's.
- **A red SF** names its path. Every row that path can reach re-runs by `-Filter <pkg> -Exact` at the release tree, or the whole sweep re-runs (COORD RULING NEEDED, SW-12).
- **`-VerifyOnly`.** A clean sweep commits nothing and restores both roots, so it owes no `-VerifyOnly` of its own. `release-nuget -VerifyOnly` re-runs at the post-cutover master tip in any case, because the cutover merge moves the roster and badges on master (release brief CP3 item 4). Anything the sweep causes to be committed re-opens C6 (COORD RULING NEEDED, SW-13).

## OBLIGATIONS LEDGER
| id | stage | gate (predicted) | source |
|:--|:--|:--|:--|
| SW-P2 | M | every row discharged; M2-M6b hold; 218 absolute, 94.8% / 97.3% | rb :3587, :3609; PLAN :555; PAR-2 |
| SW-d6(i) | S-R row 1 | the validation-bank :33 merge-result sweep of crypto/tls, deferred by 8g: a FULL-state PASS 4759 discharges it, or a named STOP-finding | close d6; batch8g brief :108 item 5 |
| SW-d6(ii) | SW-18 | the LIVE absorption at BlockSize 3419. A FULL PASS never exercises it: the arms run only when the class is not `pass` (sw :1224). Discharged by ruling, or by S-G's extra host-limit READING | close d6; batch8g brief :108 item 5; sw :1224 |
| SW-d14 | M8 | deferred entries compared by hand from the preserved records | h10-close-obligations.md:121 |
| SW-3.2 | M9 | per-row logs and walls retained and banked as the next shard-map basis | rb :3246-3249, :3396-3406 |
| SW-rsa | S-G | crypto/rsa's generator-transient re-run folded in, unchanged, with D4's capture | gate-forensics :1737-1767 |
| SW-fl | M7 | wall < 0.75 × floor on every floored row, or a post-sweep floor seat | sw :968-975; L 09-23 11:51 |
| SW-SF | CP3 | SF-1..SF-6 green at the release tree, with items 6-9 ruled by SW-17 | this brief |

## COORD RULINGS NEEDED (each is also marked where it bites)
- **SW-1: crypto/tls.**
  - DEFAULT: the FULL state is required. It comes from the raised-wall invocation on R-LAPTOP (GOFLAGS=-timeout=40m in THAT child only, with -TestTimeout 90m), which is an explicit exception to the release brief's empty-GOFLAGS rule.
  - The alternative: accept the sweep's host-limit (1,340 + 2) or capability-absent (1,340) state as a discharge.
  - If R cannot produce the FULL state: G at the raised wall, or the owner's word. G's WINDOWS side is unmeasured at the raised wall. Its hardware reached the full fan-out at the raised wall on the WSL linux arm in 1,824 s (LEDGER 2026-09-24 00:31, `836004dd20`), against the runner's 2,400 s wall. That is a linux-arm measurement, not a windows one.
- **SW-2: the §6 wording, and the runbook points this brief departs from. A PRE-LAUNCH ruling: no lane launches without it.**
  - §6 reads "coordinator-owned ... on the fastest available machine, **never parked by a lane**". harness-gates :342 and gate-forensics SKILL :1203 say a lane's parked detached sweep is KILLED (all three quoted under SURVIVING THE REAP). The counter-evidence is two completed lane batteries of unrecorded launch shape. DEFAULT: COORD owns dispatch, rulings and the merge; the lanes run DETACHED drivers under §3.6's worker contract, with the pre-row-1 survival sleeper and the resume ledger. Record the deviation, or amend the runbook in-stage.
  - "The fastest available machine" (rb :3609): the i9 is excluded by the owner's order, and the i7 is busy through Part C. The shards run on the laptops. Record it as a deviation.
  - rb §3.6 :3476: "A run that exceeds its stated budget is killed". DEFAULT: the per-row budget `4 × pkgTimeout + 30 min`, with the driver killing ONLY its own child tree by recorded PID (D2). The shard-level 4 h is a read checkpoint. The alternative: on a breach the driver reports TIMEOUT and waits, and COORD orders any kill by enumerated PIDs (gate-forensics SKILL :1187-1191).
  - rb §3.3 :3334, "The whole-solution build has been run once": see SW-19.
- **SW-3: shard shape.** DEFAULT: two lanes plus the i7 residual. Alternatives: W=3 balanced after Part C; the owner's DNS fix or Developer Mode on a lane box.
- **SW-4: net-family scope.** measurement-discipline SKILL :294: "Preflight `go test -count=1 net` before any net-family run". DEFAULT: `net` alone, on the reader census basis (SHARD PLAN: `lookup=0`, `ext=0` for all 16 other rows). COORD records the scoped reading of "net-family" in the P2 ledger line. Under the wide reading, 16 rows (~754 s) move to S-7, and Q5's hook precedes the first of them.
- **SW-5: P2's per-row criterion.** DEFAULT: exit 0 with named absorptions, the counts line of the page reading GATING, and verdict-pair moves as readings. Alternatives: full page equality, or the sweep's exit 0 alone.
- **SW-6: on a red.** DEFAULT: record, restore, continue, and report every red, with no in-sweep re-read. The linux-leg precedent re-read each FAIL once; COORD may order that on S-7 as a reading. After a fix: `-Filter <pkg> -Exact` at the merge result plus SF, or a full re-sweep.
- **SW-7: the ask.** DEFAULT: the sweep's 10m ask, with floors raising it. The alternative is a 20m ask for unfloored rows under laptop load.
- **SW-8: the floor rule.** Correct the close STAMP's inverted wording in the record. A floor breach is a post-sweep floor seat.
- **SW-9: the sweep's PowerShell host.** DEFAULT: 5.1 on every host. The alternative is pwsh 7 on R (the H10-slice precedent).
- **SW-10: the dirt control tree.** DEFAULT: `fa18863b94`, the close STAMP. The alternative is `<sweep-base>`.
- **SW-11: the evidence home.** DEFAULT: the per-shard lane refs (ledger.tsv, README and logs), plus the ledger P2 line and the DATA-sweep-row-walltimes block. The alternative is the report and ledger only.
- **SW-12: SF.** Accept the gate. Admit Part C's comment-only edits and crypto/tls's manifest prose as named hunks (DEFAULT: admitted under SF-2/SF-5), or hold them until after the cutover. Name the remedy for a red SF. The DEFAULT remedy for a red path is `-Filter <pkg> -Exact` at the release tree for every row that path can reach. A converter or golib path reaches every row, so it means the whole sweep again. That is why SW-17 is pre-ruled.
- **SW-13: `-VerifyOnly`.** Confirm that it re-runs at the post-cutover master tip, and that a clean sweep owes none of its own.
- **SW-14: a LINUX leg.**
  - DEFAULT: NOT owed at P2. P1 is keyed "under the default target OS" (rb :3586), "hop completion is same-platform by construction" (rb :2537), and the close re-read every numeric linux annotation (B12.2 residual 0).
  - If it is owed: G's WSL arm runs it, serially after S-G, at a tree carrying `836004dd20`. Its gate: every annotated row PASSES at its annotation, and the 29 unannotated applicable rows fall in a named CVAC or FAIL class, because a full linux sweep exits non-zero by construction.
- **SW-15: the full behavioral suite** ("at H9 and at the parity gate", §6). Confirm on the record that P3 was discharged at the close STAMP, and that A7's not-owed stands for the zero-footprint batch.
- **SW-16: host readiness.**
  - R-LAPTOP's current network, free disk and idle state: COORD clears R's post-hop work for S-R's duration.
  - The i7's reclaims before S-7.
  - Whether the owner applies the DNS fix or Developer Mode before launch.
  - COORD asserts in each dispatch that no other session on the box will run `dotnet build-server shutdown` or a name-scoped kill for the shard's duration (Rules).
- **SW-17: the movers that reach a verdict input (SF items 6-9). Rule each BEFORE S-R/S-G dispatch.**
  - **d15 (gofmt). NO DEFAULT: S-R and S-G are not dispatched until it is ruled.** (a) Land d15 BEFORE dispatch, with its suite and CNR `NO REGRESSION` with CHANGED EMPTY. Then `<sweep-base>` is the post-gofmt head: the Fixed-facts commit list and the converter-delta list gain d15's files, and M5's reference identity is THAT head's A2(4)-style `-trimpath -buildvcs=false` identity. (b) COORD re-dates d15 to post-release. (c) Land it after the sweep under an SF carve-out. Its gate: for every converter file SF-1 names, the go1.24.13 `gofmt` of `<sweep-base>`'s blob, CR-stripped, equals the release tree's blob, CR-stripped; plus CNR `NO REGRESSION` with CHANGED EMPTY. For d15's files alone, that gate replaces SF-6's identity equality. (c) needs its own plant (a one-token edit must red it).
  - **d7 / PAR-3.** DEFAULT: `.cs.auto` refreshes pass SF-1 by its exclusion (item 7). A T4 hunk in a compiled `.cs` reds SF-1, and its remedy is SW-12's.
  - **c6.** Its timing is RN-15's. If RN-15 rules it pre-release and it lands after the sweep: `-Filter crypto/cipher -Exact` at the release tree, P-1..P-3, logged in the P2 line.
  - **The 00:25 docs migration.** It must not touch `src/go2cs`. If it does, it takes d15's options.
- **SW-18: d6's second half, the LIVE absorption at BlockSize 3419.** A FULL PASS on R never exercises it (sw :1224). (a) Rule it discharged by `check-roster-format.ps1` section 1b2's synthetic arm (TestFakeSuite) plus 8g's page block count (batch8g brief :102-107: 3419 `TestBogoSuite` names, the plant reading 3418 != 3419). (b) S-G takes one extra READING of crypto/tls at the standard wall with no GOFLAGS: G's recorded host-limit state, ~885 s. It must print `- 3419 (TestBogoSuite host-limit disclosed ...)`, and it is logged as a reading, never as the verdict of record. It needs the network.
- **SW-19: the whole-solution build precondition** (rb §3.3 :3334). DEFAULT: waived and recorded, because the first row builds its closure and the purge every 30 rows removes a solution build's output. The alternative: one `go2cs-stdlib.slnx` build in `<wt>` before row 1, SIZE-censused, with the disk floor re-read after it.
