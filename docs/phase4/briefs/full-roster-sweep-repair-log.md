# Repair log: full-roster-sweep-brief.md (P2 / RN-14)

Repairer pass, 2026-09-24. Sources read at `fa18863b94` via `git show` into `sweep/rep/`: the runbook, the sweep, `_roster.ps1`, clean-bin, harness-gates, gate-forensics, validation-bank, measurement-discipline, the roster, PLAN-corpus-upgrade.md and CLAUDE.md. Also read: `869d8e4683:src/_roster.ps1`; the LEDGER (the mailbox checkout); h10-close-obligations.md, postclose-h11-h12-brief.md and batch8g-and-close-brief.md; `074a12c4ae`'s identifier census. One read-only measurement: go1.24.13 `gofmt -l` over a CR-stripped scratch copy of `fa18863b94:src/go2cs/*.go`. It printed 9 non-test and 9 `_test.go` files, the defect's list exactly.

Nothing was built, tested, converted or fetched. No repository or worktree was modified. One stray file this pass wrote under `/tmp` was deleted at once.

**Result: 39 defects. 39 SUSTAINED, 0 REJECTED.** Seven were sustained with a CHANGED remedy; each is marked (Δ) and explains why.

## Blocking

- **B1 SUSTAINED.** Confirmed: h10-close-obligations.md:122 (d15, "After the close, before the cutover merge"), :114 (d7, "The parity gate") and :86 (c6); the release brief's PAR-3 and PAR-5 are "cutover", and RN-15 rules the timing. The gofmt measurement reproduced the 9 + 9 files. Applied:
  - SF "What moves" gains items 6 (d15), 7 (d7/PAR-3), 8 (c6) and 9 (the 00:25 docs migration IF it touches converter templates). Item 9 goes beyond the defect: the order names "tooling and templates".
  - "None of these reaches a verdict input" becomes "Items 1-5 reach no verdict input. Items 6-9 each do, if they land after the sweep".
  - SF-1 excludes `src/core/*.cs.auto`, a pathspec `*` that crosses `/`, checked with `git ls-files`. The reason is validation-bank class 3, and a `git grep` of project files at `fa18863b94` finds `.cs.auto` only in comments. SF-1 gains a plant.
  - A new COORD RULING **SW-17**. d15 has NO DEFAULT and gates the dispatch, with options (a) land first, (b) re-date, and (c) a gofmt-equality SF carve-out. Option (c) is added (Δ): a third route that neither delays nor re-dates, and it has its own plant. d7 is covered by the exclusion. c6 follows RN-15, with remedy `-Filter crypto/cipher -Exact`. The Fixed-facts converter-delta list admits d15's files under (a).

## Major

- **M1 SUSTAINED.** Confirmed: `git diff fa18863b94 869d8e4683 -- src/_roster.ps1` adds 7 functions and `$RosterLivingProofLinkPattern` / `$RosterFrozenProofLinkPattern`. A brace-matched compare shows all 13 listed functions byte-identical, and the other 17 script-scope assignments unchanged. Applied, together with C2-07:
  - SF-5 reads `run-validated-sweep.ps1` only. For `_roster.ps1` it reads the comment-only diff from `<census-seat-ref>`'s blob, not a hard-coded `869d8e4683`, because the owner's 01:25 ruling adds a §2e change riding the same ref.
  - SF-3 now has (a) tuples with ConditionalDisclosures, (b) function text, and (c) the value of every script-scope variable, with the two new ones admitted by name.
  - Each tree is read in its own powershell process, since the function names collide. The plants are a Tests cell and `$RosterExecutionPattern`.
- **M2 SUSTAINED.** Confirmed: sw :1160 prints `  RERUN $label ...`, then the final line at :1249, :1335 or :1340, so the brief's regex matched two lines. Applied: RERUN is removed from the verdict set and counted separately (0 or 1), with the ORACLE-FLAKED note or ORACLE required after it. The STOP now reads "final verdict line missing or doubled". There are RERUN+PASS and RERUN+ORACLE fixtures.
- **M3 SUSTAINED.** Confirmed: the sw :1253 and :1261 absorptions rewrite the page counts, so a HEAD-page compare fails a discharged row. Applied: P-2 is now (a) the page's N and D == the run's `got` and `gotDisclosed`, and (b) banked == page after the proven delta, by class: path/filepath N − e, os/exec N + k and D − k. The admissible MOVED names are only the absorption's own entries. The absorption table and M6 are updated.
- **M4 SUSTAINED (Δ).** Confirmed: rb :3420-3425 (false-green #4) and :3488-3498. Applied: a driver START GATE on every launch, with an INFLIGHT marker, `aborted-<n>/` preservation, restore of both roots, explicit untracked removal and `^ D`/count asserts, then resume `list − terminal`. Δ: a FULL `clean-bin -Root <wt>\src\core -Force` purge replaces "delete that row's bin/obj". A killed row may have been building any closure dependency, not only its own project.
- **M5 SUSTAINED (Δ).** Confirmed: rb :3476-3477 ("killed and reported as a timeout") and :3484 (the wrapper must clear the instrument's budget); sw :835-846. Applied:
  - A per-row budget in the driver, with WaitForExit(budget). On a breach, the word is TIMEOUT, the results tail is read first, and the driver kills its OWN child tree by recorded PID, `taskkill /T /F /PID` issued from PowerShell. A survivor census follows, with the self-match exclusion; any survivor is a STOP.
  - The shard-level 4 h is reframed as a read checkpoint. Both texts are quoted, and the alternative (the driver waits, COORD kills) sits under SW-2.
  - Δ: the budget is `4 × pkgTimeout + 30 min`, not `3 × + 15`. The instrument's own worst case is two attempts (the oracle re-run, sw :1151-1175) × two test sides. 3× would not clear it, which is the false-red generator rb :3484 names. Δ: the defect's `MSYS_NO_PATHCONV` does not apply, because the kill is issued from the PS driver, not from bash.
- **M6 SUSTAINED.** Confirmed: harness-gates :342 ("a LANE parking a detached sweep ... gets it KILLED"), gate-forensics :1203 (in the comment) and rb :3609. The brief had quoted only :1195-1203's surviving half. Applied:
  - All three texts are quoted, beside :1197's counter-text.
  - The counter-evidence (L 11:51, 5,868 s; L 12:36, 1,761 s) is cited as completed lane batteries of UNRECORDED launch shape. Neither ledger line says "detached", so it is consistent with survival, not proof of it. This is a correction to the defect's wording.
  - SW-2 is a PRE-LAUNCH ruling. A detached SLEEPER canary is proved across one turn boundary BEFORE row 1, so it never costs crypto/tls's row. S-7 is exempt (C2-11).
- **M7 SUSTAINED.** Confirmed: sw :1224 consults the absorption arms only when the class is not `pass`; batch8g brief :108 item 5; d6 at :113. G's standard-wall host-limit wall is 885 s at sw :986. Applied: the obligation splits into SW-d6(i) (the merge-result sweep: a FULL PASS discharges it) and SW-d6(ii) (the live absorption). The new **SW-18** offers (a) discharge by 1b2's synthetic arm plus 8g's page block count (batch8g brief :102-107) or (b) S-G's extra host-limit READING. S-G's section and the shard table carry (b)'s +~885 s.
- **M8 SUSTAINED (Δ).** Confirmed: rb :3381-3384 and §3.3 :3337-3340. Applied: the ledger gains gotDisclosed, the converter commit (`<sweep-base>`), the C-toolchain capability, kind and attempt, and Q2 records the C-toolchain. Δ: gotDisclosed is read from the comparison record's `disclosed` array via ro :1826-1926's own reader, NOT from the sweep log. The converter's `K disclosed-divergent` line is captured into `$out` (sw :1129, :1180) and printed only on DISC and absorption lines, so a plain PASS log does not carry it.

## Minor

- **m1 SUSTAINED.** Confirmed: sw :1125 and :1249 print `$got$osSuffix$execSuffix`. Applied: P-1 admits ` [<Execution>]` exactly on the pinned rows. D3 asserts the header at sw :319-320 and the absence of "an A/B measurement" (:323-324). -SelfTest covers every shape (:1249-:1340, plus RERUN).
- **m2 SUSTAINED.** Confirmed: `src/.editorconfig` is tracked, and the Fixed-facts pathspec lacked `.gitattributes`. Applied to both pathspecs, and the SF Inputs list names .editorconfig. The converter delta is pinned by name (781c1c3c31: platformCensus.go and its test; ae813db069: platformProject.go and its test, verified by `git show --stat`).
- **m3 SUSTAINED.** Confirmed: ro :194-203; the sweep's default is Release with tiering off (sw :87-97), so an ambient CLR override matters for every row. Applied to PINS: GoTargetOS unset, no `DOTNET_Tiered*`/`DOTNET_TC_*`/`COMPlus_*` names, and no `target OS` header (sw :327-331).
- **m4 SUSTAINED.** Confirmed: L 00:33 (the three axes) and L 10:10 (IPv6 off). Applied: LongPathsEnabled is read on every host. The IPv6 binding is read inside Q5's hook, and a differing state is a STOP before net.
- **m5 SUSTAINED.** Confirmed: measurement-discipline :297-298. Applied: four arms through ONE sourced block, plus a fifth arm (a timeout panic) for C2-09's criterion.
- **m6 SUSTAINED (partly).** The brief DID state a basis ("only net resolves nonexistent names ..."), but uncited. Applied: the :294 rule is quoted, the reader census is cited as COORD's copy of `hostmarks.tsv` (lookup=0 and ext=0 for internal/poll and all 15 net/*; net lookup=12, ext=9, confirmed in `sweep/r3/hostmarks.tsv`), and COORD records the scoped reading in the P2 line.
- **m7 SUSTAINED.** Confirmed: measurement-discipline :374-381. Applied: a domain-joined BOOLEAN in Q2, and an os/user reading rule in S-R.
- **m8 SUSTAINED.** Confirmed: L 00:31 (1,824 s, 1,023/2,396 both sides, the raised wall). Applied in the Routing section and SW-1, with the windows side still "unmeasured".
- **m9 SUSTAINED.** Confirmed: rb :3586 is Compile parity and :3587 is Roster parity. A grep of the sweep shows 11 `_roster` functions referenced, and 0 references to Get-ExclusionLedgerSectionLines or Get-ExclusionLedgerRows. Applied: Why, SW-P2 and SF item 1 / SF-3. SW-14's `rb :3586` is correct (P1) and is kept.
- **m10 SUSTAINED.** Applied: M1 checks at S-R/S-G launch against S-7's FIXED list and re-asserts at S-7's launch. There is a verdict-of-record rule: a raised-budget re-dispatch or the SW-1 fallback REPLACES the origin verdict, while a second-host run of an ORACLE, COUNT, DISC or FAIL row is a reading. M2, FALLBACK and MOVED VERDICTS are updated.
- **m11 SUSTAINED (Δ).** Confirmed (PS 5.1 `*>` writes UTF-16LE; sw :381-385). Δ: the remedy is C2-02's `Start-Process -RedirectStandardOutput/-RedirectStandardError` shape, NOT `2>&1 | Out-File -Encoding utf8`. harness-gates :403 forbids the pipeline redirect because it buffers until completion. The driver runs at 'Continue', and -SelfTest runs the NUL-count tell over a log produced by the same redirection.
- **m12 SUSTAINED.** Applied:
  - §3.3's whole-solution build becomes **SW-19** (DEFAULT: a recorded waiver, with the reason).
  - §3.6's "check uptime first" is in Rules, Q2 and the START GATE.
  - §6's "fastest available machine" is recorded as a deviation under SW-2.
  - The machine-global hazards (build-server shutdown, name-scoped kill) are added to Rules, Q2, the run-level STOPs and SW-16.
- **m13 SUSTAINED.** Confirmed: validation-bank :1030-1032. Applied: the P-3 test-artifact class, controlled at `fa18863b94`, and M6.
- **m14 SUSTAINED.** Confirmed: gate-forensics :1737-1767. Applied: D4's crypto/rsa step (unchanged run; the CS8785/CS9248 grep over both logs and records; the generated trees preserved; "not captured" is never "did not fire"). The REPORT line and SW-rsa are moved to S-G, where crypto/rsa sits in table order.
- **m15 SUSTAINED.** Confirmed: rb :3587's manifest clause. The discharge record found: C1's orphan census, 48 manifests / 327 pins (L 2026-09-22 15:59, `f4903fad5f`), and the re-sign ACCEPTED 16:22 (`082ec41bb0`). Applied: Why, and a new **M6b** (live orphanedDisclosures, plus the manifests committed after the re-sign, named).
- **C2-01 SUSTAINED.** Confirmed: gate-forensics :1193; release brief Rules :14. Applied: `env.sh` sourced in every Bash command that runs go, dotnet or powershell, with `go version`/`go env GOROOT` asserted in the same command. The GOROOT override is process-only. A non-empty `go env GOFLAGS` with the variable unset means the go env file: STOP. S0.2's identity build is pinned identically to A2(4).
- **C2-02 SUSTAINED.** Confirmed: harness-gates :403, :407 and :410; sw :214, :244 and :345 throw. Applied: the per-row Start-Process with `-NoNewWindow -PassThru`, both redirects, `$p.Handle`, and WaitForExit (not -Wait). The driver runs at 'Continue'. The row's `finally` removes GOFLAGS, and the outer `finally` prints DRIVER_EXIT. Both files are scanned.
- **C2-03 SUSTAINED.** Applied: Set-Location, ABSOLUTE row paths created per row, `-WorkingDirectory` on the LAUNCH, and `< /dev/null`.
- **C2-04 SUSTAINED** (it duplicates M2 and m1, and is applied with them). It also notes the 34-column label padding (sw :1089).
- **C2-05 SUSTAINED.** Applied: Q5 is a driver PRE-ROW HOOK. The oracle-unstable qualifier runs only after DRIVER_EXIT. A re-dispatch starts only after the target box's DRIVER_EXIT.
- **C2-06 SUSTAINED.** Confirmed: census :233-236 loads its siblings from its own directory. Applied: a new EVIDENCE COMMIT section with a separate worktree, one directory, placeholder scrubbing, UTF-8, the archive extraction with siblings, a per-file `entry` gate, `git add -- <dir>` only, a staged-set assert, and announce-then-push.
- **C2-07 SUSTAINED** (with M1).
- **C2-08 SUSTAINED.** Confirmed: the page carries U+00B7 on both lines (`cat -A` at `fa18863b94`), ro :57, and gate-forensics :408. Applied: the page and its `git diff` preserved before restore, digit-only regexes, U+00B7 by code point, NOT REWRITTEN by mtime, and the three reader fixtures.
- **C2-09 SUSTAINED.** Confirmed: go1.24.13 lookup_test.go:345-376 (TestLookupCNAME has no subtests). Applied: tail-and-rc gate, no panic or timeout, and the failing set a SUBSET of {TestLookupCNAME}; a clean run passes.
- **C2-10 SUSTAINED** (with M5). Applied: the `<scratch>/p2/<host>/STOP` sentinel checked at PRE-ROW (exit 3). The kill rules are in D2 and SW-2's alternative, with uptime first.
- **C2-11 SUSTAINED.** Applied: S-7 has no canary and waits by a positive poll (bounded until-loops or Monitor, plus a PID census), then reports after DRIVER_EXIT and CLEANUP.
- **C2-12 SUSTAINED.** Applied: `ls-files -z` to a file with rc captured (S0.1, D5, CLEANUP); the untracked listing is captured before removal.
- **C2-13 SUSTAINED.** Confirmed: clean-bin :34-43. Applied: only rc 0 is OK, everywhere, with bash shapes spelled for S0.4 and CLEANUP.
- **C2-14 SUSTAINED.** Confirmed: src/core/.gitignore :11-21. Applied: both drives read, evidence copies ABSENT/STALE by mtime, and the expected cleanup residue stated.
- **C2-15 SUSTAINED (Δ).** Applied: the ledger fields, the C-toolchain reading and SW-19. Δ on the skip rule: TERMINAL = PASS, COUNT, DISC, CVAC, FAIL, ORACLE, N/A, NOT MEASURED and TIMEOUT. REFUSED and ABORTED re-run under an attempt suffix. NOT MEASURED and TIMEOUT are terminal FOR THE DRIVER, because rb :3499 forbids re-running at the same budget, and COORD re-dispatches them raised. The defect's list would have re-run them at the same budget.

## Not changed

- The shard plan's rows (89 / 126 / 3). No defect moves a row. SW-18 (b) adds one READING to S-G, never a verdict row.
- The deferred-alloc list, SW-3, SW-5 through SW-11 (text), SW-13, SW-14 and SW-15 are unchanged except where cited above.
