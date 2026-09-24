# P2 full validated-roster sweep -- shard S-R (R-LAPTOP)

Evidence for shard S-R of the section-6 full validated-roster sweep (P2), run at sweep base `61724860b4`
under the brief at `claude/coord-handover` `f223c19182`
(`docs/phase4/briefs/full-roster-sweep-brief.md`). COORD accepted the shard on 2026-09-24: 89/89 rows
are plain PASS, and `crypto/tls` passed in the FULL state (4759 in 1805 s, at the raised wall).
`REPORT.md` is the shard report as sent.

## Contents

- `ledger.tsv`: one row per swept package, with the section-3.4 fields.
- `driver.log`, `driver.err`: the detached driver's own log.
- `recompute-disclosed.ps1`, `recompute-disclosed.tsv`: the OFFLINE recompute of gotDisclosed and
  orphanedDisclosures. It was ruled by COORD after the driver misread the comparison record's
  PSCustomObject under Windows PowerShell 5.1.
- `tally.py`: the ledger arithmetic behind the report.
- `selftest.log`, `q12.log`, `q-symlink.log`, `plant-refusal.log`, `canary.log`, `canary.ps1`,
  `q12.ps1`, `q12b.ps1`: the qualification, self-test, plant and survival-canary readings.
- `cleanup-purge.log` and the two `purge-*.log` files: the purges.
- `deferred-all.txt`, `ign-final.txt`.
- `hunks/`: the class-2 `initᴛᴛtests()` hook in `package_init.cs` for the 8 rows that carried it, quoted
  from a post-DRIVER_EXIT convert-only re-emission.
- `rows/<row with / as __>/`: `sweep.log`, `sweep.err`, `numstat.txt`, `numstat-ignore-cr.txt`,
  `porcelain.txt` and `go2cs_test_comparison.json` for every row. `os__exec` also carries its rewritten
  page and its `docs/validation` diff: TestString skip -> pass, a count-neutral host-state reading.

## Scrubbing

Every absolute path is replaced by a placeholder: `<goroot>`, `<dotnet10>`, `<profile>`, `<repo>`,
`<wt>`, `<scratch>` and `<evidence-root>`. The machine name is replaced by `<machine>`. Every file
passed the identifier census `entry` (master `074a12c4ae`'s `coord-identifier-census.sh`) before it was
staged.

## Excluded, by name

- `p2-driver.ps1` (the S-R driver): its census `entry` refuses on the `unc_backslash` arm (one
  `'\\'` regex literal). It stays in the lane's scratch until M9 deletes it; `driver.log` records
  every step it took.
- `rows/net__netip/go2cs_test_comparison.json`: its census `entry` refuses on the `ipv4` arm (87 hits).
  They are IP-literal Go test NAMES from net/netip's own suite (TestParseAddr subtests named by the
  address they parse), not host identifiers, but the gate is the census. The row's verdict rests on its `sweep.log` and the
  ledger (PASS 211, 57 disclosed, recomputed before this exclusion).
- The per-row `go2cs_test_results.json` / `.xml` (about 4 MB, not needed by M), the copied pages and
  `docs/validation` diffs of the 88 NOT REWRITTEN rows (the `index.md` phantom), the NUL-separated
  `ls-files` snapshots, and `env.sh` (host paths).
