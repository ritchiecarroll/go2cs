# The `sync.Mutex` copy census (sqlite-cs W9a): the patch, its controls, and what it read

**What it asks.** Until W9a the hand-owned `sync.Mutex` held one reference to its gate, so a COPY of a used mutex
shared the gate and stayed one lock with the original. With Go's layout a copy is its own lock, as in Go. A copy
the CONVERTER makes where Go has none was therefore harmless before W9a and is two locks after it. The census
looks for such copies in use, on master's mutex, before W9a's battery.

**How.** `census.patch` applies to `src/core/sync/mutex.cs` at master `b2214e00e4` and is never seated. Each gate
remembers the address of the struct it was last reached through and the one before, within one collection
epoch (a collection is the only thing that moves a struct, so a use that straddles one is skipped).

| Report | Meaning |
|:--|:--|
| `ALTERNATING` | one gate used at address A, then B, then A again with no collection between: both copies are in use |
| `SECOND-ADDRESS` | one gate used at A and then at B only: the struct was copied and the original left behind (slice growth, a value on its way to the heap), which is one lock on both layouts |
| `SUMMARY` | written at process exit: uses, gates, and the two counts |

It is off unless `GO2CS_MUTEX_COPY_CENSUS` names a directory; each process writes `<entry assembly>-<pid>.txt`
there, at most 40 reports with their stack traces. Master's mutex reaches its gate on every `Lock`, `TryLock`
and `Unlock`, and the patch reports every one of them, the gate's creation included, so a process that uses a
mutex at all and exits normally writes at least its summary; a process that never touches one writes no file.

**Controls** (`control/`, `run.sh <patched tree's src directory>`), each arm its own process:

| Arm | Does | Must read |
|:--|:--|:--|
| `copy` | copies a used mutex and uses both in turn | `ALTERNATING` |
| `threads` | a second thread uses the copy and the first uses the original, taking turns 1000 times | `ALTERNATING` |
| `grow` | copies a mutex into a larger array and drops the old one | `SECOND-ADDRESS`, no `ALTERNATING` |
| `gcmove` | a heap mutex moved by a compacting collection | nothing |
| `plain` | one mutex from four threads | nothing |
| off | the variable unset | no file |

**Read on 2026-11-15** at the patch as committed here (summary line of each arm):

| Arm | Reports | Summary |
|:--|:--|:--|
| `copy` | `SECOND-ADDRESS`, then `ALTERNATING` | uses=6 gates=1 second-address=1 alternating=1 |
| `threads` | `ALTERNATING` and `SECOND-ADDRESS` | uses=4002 gates=1 second-address=1 alternating=1 |
| `grow` | `SECOND-ADDRESS` | uses=22 gates=1 second-address=1 alternating=0 |
| `gcmove` | none (the arm prints that the mutex moved) | uses=22 gates=1 second-address=0 alternating=0 |
| `plain` | none | uses=800000 gates=1 second-address=0 alternating=0 |
| off | no file | |

The first draft of the patch reported a use only where a gate was CREATED. A copy shares the gate and never
creates one, so that draft could not have reported any copy; it was found by reading the patch against these
arms before they first ran, and the patch now reports on the path that finds the gate already made as well.

The `threads` arm takes turns on purpose. Its first form let the two threads run freely, and on a later run the
first thread finished its 1000 uses before the second started: the arm read `SECOND-ADDRESS` only and the
census stopped at its controls, as it should. `ALTERNATING` needs the two addresses used in turn, so the arm
now makes them take turns (two semaphores) and reads the same on every run.

**What was run**, linux/amd64, Go 1.26.11, .NET 10.0.14: the eight behavioral slices and the first 20 rows of
W9a's sweep order (`sync`, `sync/atomic`, `internal/sync`, `runtime`, `reflect`, `internal/abi`, `database/sql`,
`net/http`, `crypto/tls`, `go/types`, `crypto/cipher`, `encoding/json`, `net`, `context`, `os`, `os/exec`, `io`,
`log`, `time`, `testing`), one row per sweep invocation.

**Read on 2026-11-15, 16:49Z to 21:06Z**, at master `b2214e00e4` (batch 308) plus the patch. `mutex.cs` is the
same file at `091935257c`, at `b2214e00e4` and at `0c1d295ea8` (batch 309, which landed while the census ran),
so the patch applies to all three; the converter that was read is `b2214e00e4`'s.

| Step | Read | Processes that wrote a file | `ALTERNATING` | `SECOND-ADDRESS` |
|:--|:--|--:|--:|--:|
| 8 behavioral slices | 979 projects, 953 compared, 0 failed, 8 NOT COMPARED | 964 | 0 | 0 |
| 20 sweep rows | 18 PASS at their banked counts; `net` 594 of 597; `os` 3831 of 3840 | 3635 | 0 | 0 |
| total | 17,364,705 uses of 405,140 gates in 4655 summaries | 4599 | 0 | 0 |

No copy of a used mutex was in use beside its original anywhere the census looked, so it names no converter
copy to cure before W9a seats.

The two count rows are not the census's doing. `os` read 3831 again in the same tree with the variable unset,
and the two runs list the same 3833 verdicts line for line; the nine missing verdicts are this host's, not
named here (the committed `os` record is the windows one, so there is no linux list to subtract). `net` reads
594 of 597 on this kind of host at master; it was not run again with the variable unset.

What this does not show: a package outside the 20 rows; windows and darwin; a path no suite runs; a copy used
only across a collection (skipped by design, see *How*); a process that exits abnormally (it writes no
summary, so the file count is of normal exits); and the converter of batch 309.
