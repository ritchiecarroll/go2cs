# Raw readings for DATA-extension-host-list.md

All from one linux lane box (4 cores, ILC 10.0.14), 2026-11-06. The record that reads them is
[`../../DATA-extension-host-list.md`](../../DATA-extension-host-list.md).

| file | what it holds |
|:--|:--|
| `probe-launches.tsv` | 24 launches per arm of the scratch structural-assert probe. `aot-fix`/`aot-fix2` = base Native AOT image (run twice, interleaved), `aot-l2`/`aot-l2b` = the list, `jit-fix`/`jit-l2` = JIT. Times in µs except `second_pair_us` and `main_end_ms` as named. Columns as in `../aot-structural-assert/probe-launches.tsv`. |
| `xpkg-go.txt` | `go run .` output of `Behavioral/XpkgPromotedMethodSet` (61 lines). |
| `xpkg-aot.txt` | the same program published Native AOT at the tip. Line 38 differs from Go. |
| `xpkg-aot-base.txt` | Native AOT at the base. Identical to `xpkg-aot.txt`: line 38 differs there too. |
| `xpkg-jit.txt` | JIT at the tip. Byte-identical to `xpkg-go.txt`. |
| `red.txt` | GolibTests guard at the red commit `964370e2e7`: 128 assemblies with no list. |
| `dropA.txt` | GolibTests and GenTests with writer 2's `_package_asserts` declaration dropped: 31 assemblies, and the GenTests case. |
| `newsite.txt` | GenTests census with a new `static class` literal added in `FieldAccessGenerator`. |
| `bogo-rounds.txt` | the extension-scan kit's `measure.sh`, golib and gen at the base and at the tip, back to back. |
| `buildtime.tsv` | `runtime.csproj` and `fmt.csproj` alone, `--no-dependencies --no-incremental`, base gen against tip gen, 3 rounds. |
