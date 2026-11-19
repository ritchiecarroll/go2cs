# The array pointer doors over ccgo union elements (W19), 2026-11-16

Evidence for design section 16.11 (`docs/phase4/DESIGN-array-pointer-doors.md`) and the BOARD row of 2026-11-16
that retires G's W12b stand-in pending G's re-run. Read on linux/amd64, Go 1.26.11, Release. Base = D4's tip
`06eccfb59d`; cut = the converter commit `1152f6b644`.

- `census/`: the census tool (`tool/`, module `w19census`) and the module it reads (`sq/`, which requires
  modernc.org/sqlite v1.62.0; libc v1.80.1 in its graph). It counts every `(*[N]E)(x)`, x an unsafe.Pointer, whose
  element E is managed under the converter's plain reading only because it holds arrays, and unmanaged under the
  inline one. `sq-linux-amd64.out`: 19 sites (TExpr 7, TRtreeCoord 4, Taddress 4, the sockaddr union 4).
  `sq-windows-amd64.out`: 11 (TExpr 7, TRtreeCoord 4). The control line counts every array pointer conversion of an
  unsafe.Pointer.
- `unit-red.txt`: the converter test `TestArrayPointerDoorTakesAnAdmittedUnionElement` at the base: its four
  positives (read, store, nested union, nonzero inline array) red on the box route; the control, a union the cap
  keeps out, passes at both. With the change the test is green.
- `probe/`: the seven-arm program (it is the behavioral program `src/tests/Behavioral/UnionElementArrayDoor`,
  same source; this copy's module is `w19`). `go.out` is Go's answer. `run.txt` holds both converters' runs: at
  the base six arms panic (index out of range with length 0; slice bounds [1:0]) and arm F, a Go array of the same
  type, is equal; with the change 7 of 7 equal Go. Each run ends with the emitted lines of the arms.
- `footprint.txt`: the two-seeded `-stdlib` reconvert on four targets (linux/amd64, windows/amd64, darwin/amd64,
  linux/arm64), each target into its own seed. 0 corpus files differ on every target; the two run reports
  (`conversion_report.html`, `stdlib_conversion_progress.txt`) differ on every two-seeded run and are not
  footprint. 0 unresolved markers.

One edit for the repository's census: scratch paths in `unit-red.txt` read `<tmp>`.
