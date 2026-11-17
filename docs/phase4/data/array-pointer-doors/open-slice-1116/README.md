# The open slice of an unnamed array through the array pointer doors (D4), 2026-11-16

Evidence for design section 16.10 (`docs/phase4/DESIGN-array-pointer-doors.md`) and the BOARD row of 2026-11-16
that closes shape Q of the 2026-11-14 row "an array viewed over another array type answers length 0". Read on
linux/amd64, Go 1.26.11, Release. Base = the W17 tip `26c2938653`; cut = the converter commit `e3adf16683`.

- `unit-red.txt`: the converter test `TestArrayPointerViewDoorReplacesTheCopyingSpan` at the base: its three new
  positives (an open view, a copy into, a copy from, each of an unnamed array) red on the old route. With the
  change the whole package is green.
- `probe/`: the seven-arm program (it is the behavioral program `src/tests/Behavioral/OpenSliceUnnamedArray`,
  same source). `go.out` is Go's answer. `run-base.txt` is the base converter's: arms A and B panic where Go
  slices, then arm C (reflect's `nameOff` copy into an int32's bytes) ends the process, exit 134, an access
  violation in `slice<byte>`'s constructor, after which no arm runs. `run-trial.txt`: with the change, 7 of 7
  equal Go. Each file ends with the emitted lines of the arms.
- `footprint.txt`: the two-seeded `-stdlib` reconvert on four targets (linux/amd64, linux/arm64, windows/amd64,
  darwin/amd64), each target into its own seed. The two run reports (`conversion_report.html`,
  `stdlib_conversion_progress.txt`) differ on every two-seeded run and are not footprint. 0 unresolved markers.

Two edits for the repository's census: the assembly-qualified type name in the base run's trace is shortened (the
`Version=` / `PublicKeyToken=` parts removed), and scratch paths read `<scratch>`.
