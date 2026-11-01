# Main-return wait, the hold arm on the i9: 5d017630ae against 438c43dbab

- BASE: claude/g-main-return-wait @ 5d017630ae, with no exit hold. TIP: 438c43dbab, whose 5th commit adds the exit hold in golib. The two tests and the converter are identical at base and tip; only golib differs.
- Tests: GoroutineRecoveringWhenMainReturns (`rec`) and GoroutinePanicAfterDeferredDone (`pad`).
- Box: i9 (13th Gen Intel Core i9-13900K), win-x64. .NET SDK 10.0.403; JIT on Microsoft.NETCore.App 10.0.14; Native AOT with ILCompiler 10.0.14; go1.26.10.
- WHEA 0 from 2026-11-01 03:29 to 07:21 (local, CST).
- Harness: i9-r1's race3.sh, with SPIN_S=14400. The longest session ran 5,030 s, past race3's 5,400 s default margin. `spinner_alive` is recorded per run, and every loaded run here had it alive.

## Builds (harness/r3-run.sh)

Four fresh worktrees, so no plain build follows a Native AOT publish in one tree:
- base JIT: the behavioral runner on `rec`;
- tip JIT: the runner on `rec`, then on `pad`;
- base AOT: one publish of `rec`;
- tip AOT: publishes of `rec` and `pad`.

Every runner build PASSes. Every exe starts: one idle run exits 0 (`rec`) or 2 (`pad`). Go's `rec` exe is the tip runner's; `rec`'s Go source is the same at base and tip.

## Legs, one load session each (rounds in the order given)

- H3 (run first, as the pilot): idle; tip `rec` JIT, tip `rec` AOT, tip `pad` JIT, tip `pad` AOT; N=50 each.
- H1: `load` n32 and `loadhi` n8; base AOT, tip AOT, base JIT, tip JIT, Go; N=100 each.
- H2: `load` n32 and `loadhi` n8; tip `pad` JIT, tip `pad` AOT; N=100 each.

## Files

- tally.tsv: per-arm counts, with `spinner_alive` beside N.
- runs.tsv: every run (1,600 rows).
- r3.txt: the driver's log.
- affinity.tsv: 2 s samples. Every readable test-process sample reads 0xF Normal; spinners read 0xF at their load's priority. 0x0 rows had already exited when read.
