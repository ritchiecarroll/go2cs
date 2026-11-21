# The oracle's tag rule, checked per roster row with `go list` (C1, 2026-11-20)

`src/go2cs/oracleTagFileSets_test.go` (TestOracleTagFileSetsOverTheRoster) lists GoFiles, CgoFiles, TestGoFiles and
XTestGoFiles for every roster row with the conversion's tags and with `math_big_pure_go` dropped, on windows, linux
and darwin (amd64, CGO_ENABLED=0), Go 1.26.11. Standard-library rows: the 238 of docs/ValidatedTestPackages.md under
`purego,math_big_pure_go`. Modules: every package of the 9 modules in docs/ValidatedModules.md (18 packages, each
listed with real files and no error) under `purego,math_big_pure_go,safe,noasm`.

Reading, predicted in 20261120T072749Z-C1 before the run (all MET):
- math/big DIFFERS on all three targets by exactly GoFiles -arith_decl_pure.go +arith_decl.go +arith_amd64.go and
  TestGoFiles +arith_amd64_test.go: the positive control, and why math/big's own row keeps the tag.
- The other 237 standard-library rows and all 18 module packages: equal on all three targets.

So for every row but math/big's, dropping the tag changes no file of the package under test or of its tests. What this
does not measure is the behavioral half: the same Go verdicts with math/big's assembly underneath.
