# S7 wave 1 dispatch instrument

The tool that lane G prepared for wave 1 of the linux/arm64 bank, committed so the lane that executes the
wave runs the same tool. The plan of record is
[`DATA-la64-s7-bank-plan.md`](../../DATA-la64-s7-bank-plan.md); this directory adds no ruling to it.

## What is here

| File | Role |
|---|---|
| `wave1.sh` | `check`, `plan`, `refs`, `dispatch`, `runs`. Its header documents the modes and the environment. |
| `A-13.filter` ... `A-2.filter` | Wave 1's seven row filters: comma-separated roster package names, no line end. 116 rows. |
| `filters.py` | Derives the seven filters from C1's macOS shard lists and checks them against the plan. |

A-N is C1's shard N: the `shard` and `row` columns of `docs/phase4/rebank-1.26.10.3/darwin/C1-N.tsv`.
Dispatch order is longest ESTIMATE first: A-13, A-7, A-8, A-9, A-6, A-12, A-2.

## Use

```bash
mkdir -p /path/outside/the/repo/wave1-state
bash docs/phase4/la64-s7/wave1/wave1.sh check /path/outside/the/repo/wave1-state
TIP_PREFIX=claude/<lane>-tip bash docs/phase4/la64-s7/wave1/wave1.sh plan /path/outside/the/repo/wave1-state
```

`refs` and `dispatch` take the same two arguments and the same `TIP_PREFIX`, and both refuse (rc 3) until
`check` reads READY. `runs` reads the state directory's `runs.tsv` (shard, ref, run, tip).

To regenerate the filters into an empty directory and compare them with the committed ones:

```bash
python docs/phase4/la64-s7/wave1/filters.py docs/phase4/rebank-1.26.10.3/darwin /path/to/empty/dir
```

## The gate

`check` reads READY only when both commits are ancestors of master's tip on origin:

- `0282fe03f4`, the stub bodies seat (linux `sched_getaffinity`, `exit`, `osyield`);
- `fbb9169c42`, the S7-a fixups.

The ledger edit `03f3f0edc2` is reported and is not a gate. A seat that lands restated under another commit
is not an ancestor: set `GATES="<sha>:<name> ..."` to the commits that did land, and record that in the bank post.

## Why one ref per shard

The workflow's concurrency group is keyed by ref, goos and stage, and keeps one running plus one pending
run. Seven dispatches on one ref lose five. `refs` pushes `<TIP_PREFIX>-<master sha10>-<i>` for i = 1..7,
all at one tip, and `dispatch` stops if a ref is not at that tip.

## Reading a wave

- Read each run's head SHA before its rows (`runs` does it): a run whose head is not the recorded tip is not evidence.
- A test's `READING ` lines are in the run's trx artifact, never in the job log.
- A wave is read before the next is dispatched.

## What was exercised (2026-11-11, master `742a3dbb24`)

| Path | Result |
|---|---|
| `check` with the stub bodies seat not yet on master | NOT READY, rc 1 |
| `refs` at NOT READY | refused, rc 3 |
| `plan` without `TIP_PREFIX`; a state directory inside the repository | refused, rc 2 each |
| `plan` with `GATES` reduced to the fixups (the READY path) | seven would-push lines, rc 0 |
| `dispatch` on the READY path with no tip ref on origin | stopped before any dispatch, rc 4 |
| `runs` against a finished run, with a matching and a non-matching recorded tip | named each correctly |
| `filters.py` against the committed filters | seven files byte-identical, 116 rows |

Not exercised: a real `refs` push and a real `dispatch`. No tip ref was pushed and nothing was dispatched
before the handover, so the first real run of those two modes is the executing lane's. `dispatch` skips a
shard that already has a row in `runs.tsv`, so a stopped run can be resumed with the same command.
