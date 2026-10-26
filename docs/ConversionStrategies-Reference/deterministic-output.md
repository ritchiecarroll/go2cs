# Deterministic Output

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#deterministic-output)

Converter output is **byte-reproducible**: converting the same Go source with the same converter build produces byte-identical C# every run. This is a hard guarantee the goldens, the full-conversion error measurements, and any future release tag all rest on. Three mechanisms enforce it: <!-- All landed 2026-07-01, proven by two consecutive full-stdlib conversions diffing to zero across 305 packages (count at landing; not re-measured by any guard). -->

* **Files convert sequentially, in sorted-filename order.** The per-file visitors share package-level state claimed at
  visit time — `initΔN` module-initializer indices, blank-identifier temp numbering (`_ᴛN`), and imported
  collision-rename visibility — so converting files in concurrent goroutines would make claim order follow goroutine
  scheduling: a file could mark an imported `package_info.cs` "parsed" *before* the parse finished, and a
  concurrently-converting file would skip the wait and emit an imported renamed const **bare** (`abi.String` instead of
  `abi.ΔString` — a compile error that comes and goes with scheduling). <!-- history: per-file conversion ran in
  concurrent goroutines until 2026-07-01; the blank-temp map was an unsynchronized map, a genuine data race. -->
  Sequential conversion costs nothing measurable: a conversion's time is dominated by `go/packages` type-graph loading,
  not emission. <!-- Measured at landing: a full-stdlib conversion (305 packages) took 3m42s concurrent vs 3m39s
  sequential, recorded in
  src/go2cs/conversionDriver.go:539-550 (`3m42s with the concurrent per-file goroutines and 3m39s sequential`);
  the platform was not recorded. -->
* **The stdlib conversion queue is deterministic and dependency-complete.** The topological sort walks sorted roots
  (map-iteration roots would make unrelated packages' order flip run-to-run), and a GOROOT-**vendored** dependency
  (imported as `golang.org/x/…` but keyed on disk as `vendor/golang.org/x/…`) is resolved to its vendored key. A dropped
  vendored edge would let an importer (e.g. `x/text/secure/bidirule`) convert *before* its dependency
  (`x/text/unicode/bidi`), whose `package_info.cs` — the source of imported collision-rename aliases like `bidiꓸClass` —
  would not exist yet at that point. <!-- history: before 2026-07-01 the roots came from map iteration and the vendored
  edge was silently dropped. -->
* **Multi-box re-alias emission is sorted.** A multi-assignment that repoints several pointer boxes (`(Ꮡx, Ꮡy) = (Ꮡy, Ꮡx)`) emits its independent `n = ref Ꮡn.DerefOrNull()` refreshers in sorted name order (the set backing them is a map; `src/go2cs/visitAssignStmt.go`, `sortedLeftExprs`).

---

[← Comments](comments.md) · [Index](README.md) · [Packages That Do Not Type-Check →](packages-that-do-not-type-check.md)
