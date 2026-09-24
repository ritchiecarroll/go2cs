# Lessons learned from the 1.23.12 → 1.24.13 hop: the seed for the write-up (owner order)

**The order.** The owner, 2026-09-23 (ledger `OWNER ORDER`, 14:52): once the first official Go corpus hop
completes, write the OFFICIAL LESSONS LEARNED into the runbook `docs/GoCorpusMigration.md`, above all
anything that makes the NEXT migration QUICKER and SMOOTHER. **Exclusion (owner, 2026-09-23):** the i9's
hardware issues stay OUT; they are temporal. Keep any host lesson generic.

**When.** The hop completed 2026-09-24: 1.24.13.1 was published, and the release record is master `3f501c7b48`,
tag `nuget-1.24.13.1`. The write-up is the step after the post-publish items in RESUME-SESSIONS 1b (d).

**Method (ruled).**
- **Derive:** a READ-ONLY derivation workflow over:
  - the ledger (claude/mailbox docs/phase4/LEDGER.md), 2026-09-07 → the release;
  - the BOARD;
  - the RESUME-SESSIONS STATE DELTAs;
  - the runbook's dated H-stage amendments, including the H10 close amendment's lessons 1-7;
  - the reviews and briefs under docs/phase4/{briefs,reviews}.
- **Check:** adversarial checkers on every claimed lesson; a lesson must cite its ledger line or commit.
- **Land:**
  - ONE dated section, "Lessons from the 1.24.13 hop", in the runbook;
  - IN-PLACE procedure edits wherever a lesson changes a step (the runbook leads);
  - rule-shaped lessons routed by CLAUDE.md's placement table (.claude/rules or skills), never into CLAUDE.md itself.

**Seed list** (starting points, each to be verified against its record, not copied).

1. **Prepare hosts before the hop.**
   - DNS: a Windows resolver behind a router relay turns NXDOMAIN into SERVFAIL. The fix is public IPv4 resolvers per box,
     with IPv6 unbound, or public v6 resolvers only where there is a v6 path. Qualify with Go's own `go test -count=1 net`
     (TestLookupCNAME is tolerated).
   - WSL: `localhost` must include ::1 (`generateHosts=false` plus a hand-written hosts file).
   - Symlinks: Developer Mode, for unelevated symlinks.
   - Long paths: LongPathsEnabled.
   - crypto/tls: a many-core host, or the raised wall (GOFLAGS=-timeout=40m).
   - Cloud quota: request it early if a cloud host is needed.
2. **Read the linux leg early.** New linkname partials surface there first (vgetrandom).
3. **Allocation labels** go through the reading run, then the relabel, then the rulings. The host records only the first
   AllocsPerRun unit note.
4. **Wrappers:** the recon wrapper's defects (a floor lowering an asked timeout; execution pins not applied) were fixed
   mid-hop at d095fe8108. Test a wrapper's pins before the campaign.
5. **Push measurement commits:** a page once cited an unpushed SHA.
6. **The close derivation's catches:**
   - a costed recon-basis row for a demoted row;
   - //go:embed payloads, with -text pins and repoguard's payload admit (R-B14);
   - emitter-moved flat files git rm'd;
   - the -stdlib staging seeder must seed docs/validation (R-B13);
   - the multi-target csproj renderer kept post-block groups (R-B15);
   - a go2cs.slnx Release build must pin go2csPath (R-B16).
7. **Comms and briefs:**
   - one message per event;
   - sub-agent batteries with briefs committed on coord-handover;
   - briefs DERIVED by read-only workflows with adversarial checkers, which caught 38 / 33 / 39 defects in the three
     release-endgame briefs.
8. **The release census** enforced a set EQUALITY the design refutes. It became the NAMED IDENTITIES (232 = 218 + 10 + 4;
   225 = 218 + 4 + 3; 214 = 218 - 4), and a committed next-release existence-plus-monotonicity check was added (local and
   origin tags). release-nuget hid a red's names until fixed. Land these instrument fixes BEFORE the hop's release week.
9. **The H6 completeness gate** had no instrument until the release, and 21 hand-owns marked during the hop were never
   audited. Audit hop-time hand-owns AS THEY LAND, and build the gate at H6.
10. **The §6 full-roster sweep**, sharded across lane hosts:
    - survival canaries and resume ledgers for detached drivers;
    - the P-2 reading was inverted in the brief (NOT REWRITTEN == EQUAL);
    - Windows PowerShell 5.1 PSCustomObject and console-codepage traps in drivers;
    - clean-bin misses src/gen outputs;
    - a -tests run rewrites docs/validation/index.md;
    - deferred allocation disclosures can be host-timing (context TestAllocs).
11. **Rehearsal:** an isolated clone with no remote exercises the pre-pack signed tag mint (P5) safely.
12. **Docs migration:**
    - migrate-gorelease's anchors rot, so run a census-and-read workflow for user-facing docs;
    - walkthrough reruns need locally packed packages;
    - the Try-it command timed out cold at 2m;
    - the walkthrough's step 4 path went stale unnoticed across releases, so a walkthrough smoke test belongs in the
      release checklist.
13. **Cutover:**
    - master moves first, then the release is cut from master;
    - fold master's tooling into the version branch before the cutover, to shrink the cutover conflict (here
      fleetIdentifierCensus_test.go).
14. **Publishing:**
    - the auto-mode permission classifier blocks an agent's release credential checks, so the publish is the owner's
      console act; plan for it;
    - removed-ID deprecations run after the publish, since the alternates must exist.
15. **Process:**
    - ledger appends need a duplicate guard (a failed write once re-appended a STAMP);
    - keep the weekly-limit save-state discipline (RESUME-SESSIONS kept current at every stamp).
