# COORD rulings for the TRAIN M draft, after verify round 2 (2026-10-02 05:55)

These answer the drafter's questions and decide the round-2 findings that need a decision. They BIND the fix round.

## The seat list changed: 20 rows, and it may still grow until the 08:00 freeze
`H:\go2cs-tmp-coord\hnd\.claude\coord-scripts\trainM\tM-seats-draft.txt` now holds 20 rows (re-read it). New since the
draft: c1-route-sysctl 5f4eb242ba (stack-on c1-darwin-linkname-pulls), c1-darwin-inode64 0a9454986b (stack-on
c1-route-sysctl; a golib seat), c2-nuget-map cf563ac526, c2-s2-source-metadata a6ec59d5bc, coord-warnings-census
fe8bfb4c96 (one docs file). All 20 are fetched in H:\Projects\go2cs as origin/claude/<ref>. The conflict pre-map re-ran:
20 of 20 clean, map union 14dab07d87 (scratch worktree H:\go2cs-tmp-coord\tM-map, read-only for you).
Up to four more rows may be accepted before the freeze (C1's S9, P2's H1 and H2, G's g-lazy-callers 483d4ea217 as the
LAST row stacked on g-godebug-pc-line). So:

**R1. No by-name list may be a literal that only COORD can keep current.** Every list the scripts read by name (the
converter tests added by seats, the GolibTests classes and tests added by seats, the behavioral project directories and
guards added by seats, GenTests / ChannelTests / TrimTests totals, E_BISECT_SEATS, REGEN_ALLOW's seat names, MAP_UNION)
is DERIVED AT RUN TIME from git between the base (aa0a07d5fd) and the union head, or read from the seats file, with the
derived list stamped into the SUMMARY so COORD can read it. Where a derivation is not possible from git alone, keep the
literal BUT assert it against a derived superset and FAIL with the difference named (a list that is short for the
union is a finding, never silence). A total (GN, CT, TR, FX) is gated as "0 failed AND total >= the measured floor",
and the floor is stamped with its source.

## Answers to the drafter's questions
- **P2's full runtime row:** RT_CHILD=210m, RT_OUTER=450m are the DEFAULTS in the linux driver and both are named in
  the brief.
- **REGEN_ALLOW:** it holds only files a seat's own acceptance MEASURED as corpus footprint (F1+F6's
  go/internal/srcimporter/srcimporter.cs, and G's seats' files as the emission check itself reports them). A
  regenerated file outside the list STOPS the fixup before writing, for COORD. Never widen it automatically.
  The narrowing the drafter applied (files directly in the named package folders; the testing hand-own test limited to
  files directly in src/core/testing/) stands.
- **A second fixup after the union is pushed:** the ruled shape is a commit ON TOP whose subject starts
  `fixup-2: TRAIN M` (then `fixup-3`, ...). The battery's PRE admits `fixup: TRAIN M` followed by ZERO OR MORE
  `fixup-N: TRAIN M` single-parent commits at the head, in order, and nothing else single-parent. Never replace a
  pushed SHA (floor 9). The README says which legs a fixup-N obliges COORD to re-read (by the paths it touches).
- **A stale stdlib-metadata.txt at PRERES:** the regenerated asset rides the FIXUP (it is derived from the union's
  package_info.cs files, no single seat owns it). Add it to the fixup's expected set and state it in the fixup message.
- **SIBLINGS:** the default launch is SIBLINGS=report. No widening for siblings.
- **Stack-on notes for the late rows:** already in the seats file for S7b and S10; g-lazy-callers gets one if it rides.
- **The LIVE gate:** narrow the dotnet.exe arm to a command line that names the tM worktree path or a path under the
  battery's scratch root; keep testhost.exe under those same path conditions. Never by bare process name.
  LIVE_ACK=1 stays.
- **GN and FX:** gated as floors per R1 (GN: 0 failed, total >= 68; FX: tracked fixtures >= 1281), the floors stamped.
- **encoding/json:** ADD it to X_ROWS so two boxes read it.
- **M12, the log/slog reason:** the correction rides the FIXUP as a comment-and-prose-only hunk (src/_roster.ps1's
  comment and the roster prose): the reason is "a timing flake on a busy disk: symbol-file reads at the first
  source-line resolution; rooted in runtime.Callers symbolizing at capture", not "first-call JIT latency". No row,
  count or execution config changes. COORD writes the exact words at the fixup; leave a named manual step for it.
- **HOSTWALL:** the stamp per S row is enough. Do not keep the result files.

## Decisions on round-2 findings
- **Stale go2cs_test_comparison.json (high):** take the verifier's fix. A T/NR/bank leg must prove its record is THIS
  run's: delete the row's record files before the leg starts (they are gitignored build output of that row) and treat
  a missing record after the leg as a FAILED leg, never as "no deadlock lines". No gate may pass by absence.
- **Red leg 4 then leg 5 re-dirties the tree (medium, both lenses):** restore CNR's and the behavioral legs' rewrites
  again immediately before the module legs, with the CR-only handling TRAIN L's postmerge2 restore_all uses, and assert
  tracked changes = 0 there. Stamp what was restored.
- **Test-host legs without a time limit (medium):** GT, CT, GN and TR run under the capped wrapper with a wall cap
  (GolibTests 45m per run, ChannelTests 10m, GenTests 20m, TrimTests 20m). A fired cap lists the orphans by PID and
  path, kills nothing, keeps the lock, and the battery stops (exit 5), as the existing wall caps do.
- **DEADLINE (medium):** replace the same-day HHMM integer with an absolute time. Accept DEADLINE as
  'YYYY-MM-DD HH:MM' (local), convert once to epoch seconds at launch, refuse a deadline already in the past, and
  compare epochs. The default is launch time + 14 hours, stamped. Same in the module legs.
- **END legend (medium):** remove "TBS-W is a filtered diagnostic" from the expected-non-zero list; a non-zero TBS-W is
  a red to read.
- **XS and host finding H1 (medium):** when the seats file has NO row for the H1 host seat (a ref matching
  `p2-*test-list*` or noted "H1"), XS does NOT run golang.org/x/sync's singleflight on windows: stamp it
  "NOT RUN (KNOWN: H1, child fan-out)". When the H1 seat IS in the union, run it and expect 12 of 12. After the
  module legs, census host processes BY EXECUTABLE PATH under the module out root; any survivor is a finding and keeps
  the lock.
- **The linux driver cannot fail (medium):** the END line exits non-zero when any expectation misses: a count that
  differs from the brief's, a MOVER, a missing by-name pass, a REALMOD FAIL outside the known list, a freshness fail.
  Keep every stamp; add the gate.
- **The battery's exit status (low):** the last command is `exit $rc`, non-zero when any leg outside the documented
  expected-non-zero set is non-zero or any FINDING line exists.
- **Emission check and module legs launched by hand (medium/low):** give both the floor-4 launch refusal, the lock, the
  LIVE gate and the disk preflight of the battery, so a hand launch is as safe as a call from the battery.
- **The assembly's table check has no control (medium):** add TABLE_ONLY=1 (check the seat table and exit before any
  worktree or merge), and name three controls in the README (a duplicated sha, a stacked row above its base, a row
  whose sha is already on the base), each expected to be refused by name.
- **Stale G frame class in committed TEST sources (medium):** RULED: not in M's fixup. The corpus-wide refresh of
  committed -tests sources (this class and the older .slice spelling class) is a queued COORD item fed by a windows
  sweep's re-emission after M lands. The README names it as a ruled deferral (M13) and the TE leg stays a reading.
- **Assembly tip check (low):** verify each seat's tip by `git ls-remote` at assembly time, not by the local tracking
  ref; a row whose remote branch is gone or whose tip is not the seated sha (and not a descendant holding only ruled
  follow-ups) is refused by name. No prune.
- Every other round-2 finding: apply the verifier's proposed fix unless the file shows the finding is wrong, in
  which case say why.

## Not yours to change
The seats file, anything under hnd, any git state. COORD copies the final draft into hnd and commits it.
