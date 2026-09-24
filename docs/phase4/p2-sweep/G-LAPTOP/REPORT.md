G -> COORD: S-G REPORT. DRIVER_EXIT=0, processed 128 == listed 128. Score: 127 PASS + 1 COUNT (context); 0 FAIL/DISC/CVAC/ORACLE/TIMEOUT/NOT MEASURED; 0 RERUN; no run-level STOP, no budget kill, no START-gate restore. CLEANUP is STOPPED at step 3 on named residue (below), so the worktree is NOT yet removed.
HEADER:
- host G-LAPTOP; <sweep-base> 61724860b4 asserted (the version tip equalled it at dispatch).
- Converter: plain sha256 cf743ca6…c8dd9a2b; IDENTITY 38be1ce6…9a978664, == A2(4).
- Run: 02:20:51 -> 04:30:59. A first launch at 02:19:45 died in 25 s, before the START gate, on MY driver bug: a function named Git shadowed git.exe and recursed. No row ran and the tree was untouched; fixed, self-test 44/44, relaunched.
Q:
- Pins OK by output; no GoTargetOS, DOTNET_Tiered*, COMPlus_* or credential names.
- 12 CPUs, 30.8 GB RAM, uptime 16.5 h; not domain-joined; LongPathsEnabled 1; no C toolchain; echo present.
- Disk 212.6 GB -> 226.3 GB. COORD's machine-global-hazards assertion recorded.
- Q3 symlink FUNCTIONAL: InRoot and NoRoot ran and passed unelevated (Developer Mode).
- Q5: Wi-Fi IPv6 False (other adapters bound). net qualifier pre-launch x2 and in-driver x2: each rc 1, failing = TestLookupCNAME only, no panic. The five criterion arms passed in the self-test.
- Survival canary proven across a turn boundary.
LIST: 126 (archive/tar .. internal/godebugs, minus crypto/tls) + os + net = 128, per the SW-3 amendment.
- Σ got 48,504 vs Σ banked Tests 48,503; the +1 is context.
- No absorption fired.
- internal/godebug: `PASS 5 [release-tiered]`, its record tiered=true. Every other record reads Release, tiered=false.
- crypto/rsa: `PASS 568 [124s]`, no CS8785/CS9248 (a PASS; generated trees preserved).
- net: `PASS 476 [322s]`. os: `PASS 1105 [94s]`.
ROW-LEVEL FINDINGS:
1. **context: COUNT 58 | 0 vs banked 57 | 1.** TestAllocs is pass/pass, and its record lists it in orphanedDisclosures (deferred, windows).
   - Release, tiering off, confirmed by the record.
   - The C# first-call note: 1/run (100 objects, 37,112 B over 100 runs), COUNT, a lower bound, identical to the bank's.
   - The want is <= 8 on the WithTimeout(bg, 5ms) leg (the bank printed 9). On a pass neither side prints per-leg counts, so Go's and C#'s leg counts are NOT available. The flip is unattributed; timing is plausible.
   - The README badge drift is its consequence.
2. **archive/tar, count-neutral:** TestFileInfoHeaderSymlink skip/skip -> pass/pass (Developer Mode's symlinks).
3. **os, count-neutral:** TestNetworkSymbolicLink pass/pass -> skip/skip, unattributed; it likely needs more than Developer Mode's symlink right.
PAGES, recomputed OFFLINE (analyze.py) with UTF-8 HEAD bytes plus planted non-ASCII controls:
- 125 NOT REWRITTEN (= EQUAL, your ruling) and 3 MOVED (the three above). The live driver's cp437 misreads of os's non-ASCII names are voided.
- P-2(a) holds on all 128. P-2(b) fails only on context.
DRIFT (CR-ignored numstat):
- 110 clean; 15 rows class 2 (the sweep's own documented package_init.cs); context README (the badge).
- go/printer testdata/generics.golden and .input, +2 each: the committed copies LAG go1.24.13's GOROOT (missing `type _[P *T,]`, `type _[P ~int]` and the input twins). The run re-staged GOROOT's; fa18863b94 is the same blob. A stale-fixture finding, not converter drift.
- crypto/ecdh package_test_info.cs 2/2: UNCLASSIFIED. My driver kept numstat only, so the hunk text was NOT captured.
WALLS:
- Every floored row is <= 0.09x its floor: go/parser 350 s / 90m, hash/maphash 336 s / 60m, net 322 s / 120m, zip, suffixarray, dsa, both mlkem.
- No unfloored row reached 450 s.
DEFERRED: 28 manifests at the sweep base; the brief's 29 included net/netip, which carries none here. Their records are under rows/<row>/.
ORPHANED DISCLOSURES: context TestAllocs only.
CLEANUP STOP (step 3), residue outside the expected class:
- src/gen/go2cs-gen/bin/ and src/gen/go2cs-gen/obj/: the analyzer's build output, outside clean-bin's -Root src/core.
- src/core/go/parser/testdata/issue42951/not_a_file.go/: a staged fixture directory whose name ends in .go.
- Otherwise porcelain is empty, tracked 15,394 == S0, and the purge was rc 0.
NOT VERIFIED: the ecdh hunk text; the TestAllocs per-leg counts; the self-test's one lost-stdout anomaly (a child writing stderr, unattributed; the live logs were verified).
ASK:
1. Accept the residue and let me remove the worktree (children-first)?
2. Evidence commit to claude/g-p2-sweep-evidence on your word.
