# W1 — windows page-allocator pre-blockers: PREDICTION (written before any emission)

Seat: `claude/r-w1-windows-mem` on the TRAIN G union `e6fc210500`. COORD ruling 2026-09-28: W1 = (ii) + (iii).

## The change

- **(ii)** `sysReserveOS`, `sysUsedOS`, `sysUnusedOS` join `sysAllocOS`/`sysFreeOS` in `manualConversionFuncs`
  (`goosWindows`) and get Go's bodies over kernel32 in `runtime/windows/mem_windows_impl.cs`: `VirtualAlloc`
  MEM_RESERVE (hint first, then anywhere), MEM_COMMIT and `VirtualFree` MEM_DECOMMIT, with Go's halving retry
  and Go's messages.
- **(iii)** a `[ModuleInitializer]` in `runtime/windows/os_windows_impl.cs` sets
  `physPageSize = Environment.SystemPageSize` (osinit's `getPageSize()`, which never runs here).

## Two-seeded `-stdlib` footprint (base `e6fc210500` converter vs this seat's), three targets

| target | files | what |
|---|---|---|
| windows/amd64 | **1**: `runtime/windows/mem_windows.cs` | the three bodies each become the converter's one-line hand-own placeholder (the shape `sysAllocOS`/`sysFreeOS` already have); hoisted `@string` literals referenced only by those bodies drop out of the file's literal block; the file's `GoPositionMap` lines RE-ENCODE (a removal re-encodes the map; a map line REMOVED rather than re-encoded would falsify this) |
| linux/amd64 | **0** | the rows are `goosWindows` only |
| darwin/amd64 | **0** | same |

Falsifiers: any second windows file; any linux or darwin file; a body left in place.

## CNR

**0 CHANGED.** No behavioral test converts `runtime`.

## The family re-probe (the gate)

Windows page-allocator rows (`^(TestPage|TestScav|TestPalloc)`, TestPageAccounting excluded as on linux) reach
the **A17 AccessViolation** in `scavengeIndex.alloc` exactly as linux does: first row
`TestPageAllocAlloc/AllFree1`, same frames. The A16 golib shim is NOT part of this seat, so the prediction is
stated for the probe tree (this seat + the A16 shim), and separately for this seat alone: without A16 the rows
stop at A16's refusal, as linux does today.

## Rows that move from the physPageSize fix

To be LISTED from a windows `runtime` run at this seat against the base (COORD asked for every moving row).
Predicted direction only: rows that aligned to 0 now align to 4096, so movement is expected in mallocinit- and
page-size-dependent rows; no count is predicted.

---

## SCORED 2026-09-28 (appended; the prediction above is left as written)

Two-seeded footprint, base `e6fc210500` converter vs cut `9f3a0b0ec6`, seeds 4,202 `.cs` per root (one value),
both binaries fresh, 1,856 / 1,926 / 1,926 files written per arm:

| target | predicted | measured | verdict |
|---|---|---|---|
| windows/amd64 | 1 file | **2 files, +4 −78** | **MISSED on the count** |
| linux/amd64 | 0 | 0 (+0 −0) | MET |
| darwin/amd64 | 0 | 0 (+0 −0) | MET |

**Mechanism MET, specifics MISSED.** `mem_windows.cs` (+3 −77): each of the three bodies became the one-line
hand-own placeholder, and the two hoisted literals used only by them (`runtimeFailedToDecommitˢ`,
`runtimeFailedToCommitˢ`) dropped out, as predicted. The position map RE-ENCODED (not removed), as predicted, but
it lives in **`runtime/windows/package_info.cs`** (+1 −1), not in `mem_windows.cs`: the `[assembly:
GoPositionMap(...)]` records are package-level metadata. I predicted the map's location from the wrong file;
that is the whole miss.

Found in passing: the removed `sysUnusedOS` and `sysUsedOS` bodies each carried `v.Value = (uintptr)add(v, small)`,
two more instances of the unsafe.Pointer-PARAMETER assignment defect routed to C2, invisible to the literal
`(@unsafe.Pointer)` census because their right-hand side is `add(...)`. W1's hand-own retires them here; C2's
census should expect the non-conversion shape elsewhere.

Hunk rule: `git merge-file -p <committed> <base emission> <cut emission>` per file; applied delta equals the
emission delta (3/77, 1/1); residual drift committed-vs-base = 0 and applied-vs-cut = 0 on both files.
