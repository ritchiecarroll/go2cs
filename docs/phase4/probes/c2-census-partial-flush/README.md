# The partial-flush control (COORD ruling 3, `82c60cec4`)

`reflect` read **"0 files, no census output"** — the census reported only from a `ProcessExit` hook, so a
host that dies before exit writes **nothing**, which is indistinguishable from a row that performed no
conversions. That is the unrun-instrument falsifier wearing a result's clothes, and it is why `reflect`
had to be recorded UNMEASURED rather than 0.

The remedy writes a **partial block** at the first conversion and every 250,000 after it. This probe is
the control COORD required: kill a host mid-run and require a non-empty reading.

## Running it

Build against the worktree's golib, then, under a process ceiling:

```
GO2CS_Q44_CENSUS=1 GO2CS_Q44_CENSUS_FILE=<dir>/kill-{pid}.txt ./c2killprobe &
sleep 6; kill -9 $!            # SIGKILL: no exit hook can run
```

## What it must show — both arms, measured 2026-09-08

| arm | requirement | reading |
|:--|:--|:--|
| SIGKILL mid-run | file non-empty | **1 file** |
| | block marked `Q44CENSUS-PARTIAL` | **yes** |
| | no `Q44CENSUS-BROKEN` | **none** |
| | no FINAL block (a killed host cannot write one) | **none** |
| clean exit | a FINAL `Q44CENSUS` block | **yes** |
| | reconciles EXACTLY | **yes** |

## ⚠ What the control caught, which is why it exists

The first implementation flushed on the conversion increment **before** the arm counter, so the flushing
thread had counted its conversion and not its arm. Every partial then reported
`Q44CENSUS-BROKEN arms sum to 249999 but conversions is 250000` — the census's own not-exhaustive alarm,
fired by the instrument on itself, on every single flush. Two changes followed: the flush moved **after**
the arm counter, and a partial that still skews (threads genuinely mid-arm under concurrency) reports
`Q44CENSUS-PARTIAL-SKEW` with the delta rather than `BROKEN`. **Only a FINAL block asserts exact
reconciliation** — mid-flight it cannot, by construction. Crying wolf on every partial would teach a
reader to ignore the one alarm that matters.

## ⚠ Neutrality

This instrument broke neutrality twice — once through a second `Resolve`, once through a counter added to
prove it did not. So the hot path gains **no atomic operation**: the conversion counter was already an
`Interlocked.Increment` and its return value is now read instead of discarded, leaving one comparison
against a constant per conversion. One thread flushes at a time and a concurrent arrival **skips** rather
than queues, because a census must never become a lock the program under test waits on. **A fresh `os`
neutrality gate is owed before any row is measured with this**, on a host that can run one.

## The same binary, run TWICE concurrently: the shared-path clobber control (2026-09-08)

This probe writes continuously, which makes it the instrument for a SECOND question: does a census
path shared by two processes lose blocks? Two instances against one path, the second started a few
seconds after the first has flushed, then both killed:

```
GO2CS_Q44_CENSUS=1 GO2CS_Q44_CENSUS_FILE=<dir>/shared.txt ./c2killprobe &   # A
sleep 4
GO2CS_Q44_CENSUS=1 GO2CS_Q44_CENSUS_FILE=<dir>/shared.txt ./c2killprobe &   # B
sleep 5; kill -9 %1 %2
```

| what it answered | reading |
|:--|:--|
| shared path, two SEQUENTIAL runs (before the fix) | **1 pid** — A's 19 blocks silently destroyed by B's first write |
| shared path, two CONCURRENT instances, timestamp heuristic | **2 pids** — the heuristic's hole did NOT reproduce here |
| shared path, two concurrent instances, per-process fix | **2 files, 2 pids**, each carrying `Q44CENSUS-PATH` |
| `{pid}` path, two concurrent instances, per-process fix | **2 files, 2 pids**, no `Q44CENSUS-PATH` — unchanged |

⚠ **Why the middle row killed a fix rather than saving it.** A timestamp rule (truncate only a file
older than this process's start) passed both orderings anyone could build here — but only because
this probe flushes CONTINUOUSLY, so the other process's last write is always milliseconds old. The
hole needs a first writer that flushes ONCE and goes quiet, and three instruments in a row could not
produce that ordering. A correctness rule whose verdict depends on how often the other process
happens to write is not one to ship in an instrument whose whole job is not to lose data quietly, so
the heuristic was dropped for the structural fix. **This probe cannot discriminate that axis — it is
recorded here so nobody reads its 2-pid green as evidence the heuristic was sound.**

