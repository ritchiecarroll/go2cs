# Data for DATA-aot-variadic-shell.md

Readings from one linux lane box (4 cores, 15 GB, ILC 10.0.14, SDK 10.0.403, go1.26.10).

| path | what it is |
|:--|:--|
| `probe/main.go.txt` | the scratch 10-assert probe that found the split (deleted from the tree after the seat) |
| `probe/go.txt`, `probe/jit.txt`, `probe/aot-base.txt` | the probe under Go, the JIT and Native AOT at master `53b3eb88d7`: 10, 10 and 3 true |
| `probe/aot-base-traced.txt` | the same AOT probe with a scratch golib print: "no object shell", then the `MakeGenericType` failure |
| `repro-base/`, `repro-fix/` | `Behavioral/VariadicInterfaceAssert` under Go and Native AOT at base and fix, with the diff; `repro-fix/jit.txt` is the JIT |
| `xpkg/` | XpkgPromotedMethodSet under Go and Native AOT; `aot-base.txt` is L2's reading at `d62e7d34aa`, `aot-fix.txt` this seat's |
| `aot-exe-size.tsv` | AOT image bytes at base and fix (XpkgPromotedMethodSet has no base publish here) |
| `bogo/sequential-*-wall.tsv` | BoGo probe launches, base then fix: launch index, wall in µs, exit code (index 0 is the dropped warm-up) |
| `bogo/interleaved-wall.tsv` | the interleaved run: arm, pair index, wall in µs, exit code (pair 0 dropped) |
| `census/main.go.txt`, `census/goroot-go1.26.10.txt` | the go/ast census tool and its output over go1.26.10 `src`: generic variadic receivers, generic embeds, variadic interface members |
| `behavioral-chunks.txt` | the full behavioral suite at `9c645c5e93`, one line per chunk (chunk 1 is the hang-guard stop, rerun as chunk 7) |
| `kit/` | the scripts: `aot.sh` (one AOT read), `bogo.sh` (sequential launches), `bogo-ab.sh` (interleaved launches) |
