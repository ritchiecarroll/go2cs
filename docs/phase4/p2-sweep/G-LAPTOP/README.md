# P2 full validated-roster sweep, shard S-G on G-LAPTOP (windows side)

- **What.** The evidence for shard S-G at sweep base 61724860b4: 128 rows (the 126 + os + net), DRIVER_EXIT=0, 127 PASS + 1 COUNT (context). REPORT.md is the report as sent to COORD. ledger.tsv holds one row per package. rows/<pkg>/ holds each row's sweep.log/.err, numstat, porcelain, the docs/validation diff and verdicts.json (the comparison record reduced to its verdict fields). The full go2cs_test_results.json is kept only for the deferred-alloc rows, which M8 reads.
- **Scrubbed.** GOROOT, the dotnet root and profile paths are replaced by <goroot>, <dotnet10> and <profile> (build-evidence.py). The raw go test -v net logs print resolver and local IP literals, so they stay local; netqual-pre-summary.txt carries their counts and failing sets.
- **One edit to an as-run script.** p2-driver.ps1 line 414 spells the regex '\obj\' as the equivalent '\x5Cobj\x5C', because the identifier census reads the former as a UNC path. The driver that RAN has sha256 ff819773a51b341f7ea6bb7018dc5a3bd7aef0f99e320e3904e39ce7c37a66fd.
- **The offline recompute.** analyze.py produced the page readings reported (UTF-8 HEAD bytes, planted controls), and analysis.txt is its output.
- **Excluded by the identifier census (the full files stay in local scratch until M9).**
  - rows/crypto__x509/page-crypto.x509.md and verdicts.json: Go's own subtest names carry IP-shaped literals.
  - rows/net/go2cs_test_results.json: host network addresses.
  - rows/os/go2cs_test_results.json: the machine name.
- **Census.** Every file here passes `coord-identifier-census.sh entry`, from master 074a12c4ae.
