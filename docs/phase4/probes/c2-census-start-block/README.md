# Probe: the Q44 census's START block — making a zero a measurement

## What it is for

The census writes a block for a reason: the flush at conversion 1, the flush every 250,000, the
exit hook. Every one of those needs work to have happened. So a process that **armed the census and
converted nothing** wrote no file, and `no file` read identically to four different things:

- the gate (`GO2CS_Q44_CENSUS`) was never set,
- `golib` never loaded in that process,
- the host died before the exit hook,
- or the write failed.

COORD ruling 3 (`82c60cec4`) closed the third from one side by adding the partial flush. This closes
the **first-and-fourth-versus-second** ambiguity from the other: golib's module initializer now
writes a **START block** before any conversion, so the artifact itself says the census armed here.
i9 confirmed (`d6306f2d12`) that the pipeline keeps no process record, so the disambiguation cannot
come from outside the file.

`reflect` is the row this was built for: it was recorded UNMEASURED rather than as a zero because
nothing in the output could tell those cases apart.

## The distinction that matters, and it is NOT "one block"

A process that exits **cleanly** having converted nothing runs its exit hook and writes a final zero
block. **That case was never ambiguous.** The case that left no file is the one that **died** having
converted nothing — and there the START block is the last thing in the file.

So `ARMED-ZERO` is *"the START block survived the fold"*, not *"there is only one block"*. The reader
(`docs/phase4/probes/c2-census-read`) decides it by whether the `Q44CENSUS-START` marker sits after
the file's LAST totals line, which is exactly "no later block superseded it".

The start block carries the **PARTIAL** header deliberately: it *is* a cumulative snapshot, taken
before any conversion, so every existing reader folds it correctly with no change (last block per
file, summed across files) and cannot double-count it.

## Running it

`Program.cs` takes `argv[0]` = conversions to perform, and `argv[1] = hang` to sleep forever
afterwards so the harness can kill the process before its exit hook runs. Build it against the
worktree's `golib` with a generated csproj — the reference is an absolute path, which is why no
csproj is committed here (same shape as `c2-census-partial-flush`).

## The five arms, and what each one proves

| arm | setup | must read |
|:--|:--|:--|
| A1 | gate ON, 0 conversions, clean exit | file exists, 2 blocks, `final`, conv 0 — the case that was already fine |
| A2 | gate ON, 0 conversions, **SIGKILL** | file exists, 1 block, **`ARMED-ZERO`**, conv 0 — **the case that left no file at all** |
| B | gate **OFF** | **no file**, and the reader REFUSES with exit 1 — so "no file" keeps its one remaining meaning |
| C | gate ON, 5 conversions, clean exit | 3 blocks and ROW TOTAL conversions **== 5 exactly** — the no-perturbation assertion |
| N | A2's file with only the `Q44CENSUS-START` line deleted | falls back to `PARTIAL-ONLY`, ARMED-ZERO 0 — the verdict comes from the MARKER, not the block count |

All five were measured against predictions written before the run, and all five hit.

## ⚠ Two harness lessons this probe paid for, both on its FIRST run

**The instrument never compiled in.** The first run measured a binary built before the `hang` branch
existed: the probe exited cleanly, wrote its exit block, and **arm A2 read identically to arm A1** —
the killed-before-exit arm agreeing with the clean arm it was supposed to differ from. That
agreement is the tell. The harness now rebuilds and then *asserts* the binary is newer than its
source; a remembered rebuild is not a fix.

**The staleness checker was itself broken.** The assertion that the `hang` literal is present in the
built assembly used 8-bit `strings`, which reports **zero** for a .NET string literal because those
are UTF-16 — so the gate aborted a build that was completely fine. It now uses `strings -el` **and
positive-controls itself** against a literal known to be present (`PROBE-DID`) before its verdict on
the literal under test is believed. A checker that cannot find the control string has no business
reporting on anything else.

⚠ **And it counts with `grep -c` plus an integer test, never `grep -q`.** The harness sets
`set -uo pipefail`, and `strings -el <assembly> | grep -q X` is R's SIGPIPE class exactly: a
many-line producer feeding an early-exiting consumer **whose exit status is the answer**, so the
verdict is a timing race rather than a measurement. `grep -c` reads the whole input and cannot
SIGPIPE its producer. The failure direction here was the safe one — a false ABORT, never a false
pass — but a gate that can abort at random is not a gate, and this one was written *after* the class
was already known.
