# 1.26.10.2 re-bank: darwin reading at the freeze tip f6666d98a2

Lane C1. This is the DISCOVERY pass (COORD, 2026-11-02 16:23Z). The bank of record is the re-pass at the new freeze tip
that follows G's golib seat for fips140cache. This ref was cut from f6666d98a21ad4c3f992c959769f844ee6422ceb, under
COORD's evidence-ref ruling (2026-11-02 15:59Z), and carries only readings taken at that tip. It holds nothing from the
f75e0c53ec dry run: that run stays on `claude/c1-darwin-1262`, unchanged.

Each `C1-<n>.tsv` is one os-matrix `sweep-shard` dispatch, run with `goos=darwin` on both mac RIDs (osx-arm64 and
osx-x64). Every run's headSha is f6666d98a2. The rows are those of the C1 block of `docs/PLAN-release-1-26-10-2.md`
("The re-bank legs") at 6f2bf7e95e: 236 rows, each with one line per RID.

| Dispatch | Run | Dispatch | Run | Dispatch | Run |
|:--|:--|:--|:--|:--|:--|
| C1-1 | 41477596603 | C1-6 | 41500009759 | C1-11 | 41477548496 |
| C1-2 | 41482106317 | C1-7 | 41500705985 | C1-12 | 41508606636 |
| C1-3 | 41477502465 | C1-8 | 41501731134 | C1-13 | 41477406030 |
| C1-4 | 41492155409 | C1-9 | 41503062854 | C1-14 | 41510557989 |
| C1-5 | 41497985107 | C1-10 | 41507272717 | C1-15 | 41477450611 |

## Columns

`shard row RID verdicts matched disclosed divergent new status agree win_expected win_disclosed darwin_seg win_differs`

- `verdicts`: all verdict names, united over the Go and C# sides.
- `matched`: names whose two verdicts are equal.
- `disclosed`: the length of the record's disclosure list.
- `divergent`: verdicts minus matched. This is the disclosed-divergent count a bank reads.
- `new`: divergent names that no disclosure names.
- `status`: the sweep's word when it is not PASS (FAIL, COUNT, DISC, CVAC, ORACLE). On PASS it is the record's status.
  - `NO-RECORD:<word>`: the row has a word and no comparison record.
  - `PASS?rc`: the RID's job failed and no row explains it.
- `agree`: the two RIDs match verdict for verdict, on both sides.
- `win_expected` and `win_disclosed`: the roster's windows column at the tip.
- `darwin_seg`: the roster's `darwin:` segment at the tip; `-` means there is none.
- `win_differs`: whether matched differs from `win_expected`.

The counts were read from the run artifacts by the relay on `claude/c1-darwin-1262-relay`. Each row's word comes from
the sweep's own console log in the artifact.

## The eight rows with no darwin segment

COORD expected these to read CVAC. Two read PASS instead, because the sweep scored them against the windows column.

| Row | osx-arm64 status, matched/disclosed | osx-x64 status, matched/disclosed | Banks a new segment |
|:--|:--|:--|:--|
| crypto/sha1 | validated, 13/1 (word PASS) | validated, 13/1 (word PASS) | yes, 13/1; equal to windows |
| internal/coverage/cfile | validated, 16/0 (word PASS) | validated, 16/0 (word PASS) | yes, 16/0; equal to windows |
| testing | FAIL, 80/3 | FAIL, 80/3 | no |
| internal/synctest | FAIL, 56/0 | FAIL, 56/0 | no (the RIDs disagree) |
| internal/trace | FAIL, 185/0 | FAIL, 186/0 | no (the RIDs disagree) |
| runtime | FAIL, 11058/63 | FAIL, 11059/63 | no (the RIDs disagree) |
| runtime/pprof | FAIL, 144/13 | FAIL, 146/14 | no (the RIDs disagree) |
| syscall | FAIL, 16/0 | FAIL, 16/0 | no |

## Notes the TSVs cannot carry

- runtime (C1-13), osx-x64: TestSmhasherAvalanche passes the 60 m per-test deadline. COORD placed it (18:37Z) with
  the standing x64 per-test timeouts under load, in the channel-barrier pair's class: a host-load timeout on the
  slower RID, not a new mechanism. It moves no ruling, because runtime does not bank on darwin while the RIDs disagree.
- internal/cpu (2 against 9) and internal/runtime/gc/scan (3092 against 3095) read COUNT on both RIDs. COORD HELD both
  (20:05Z): a count below the bank never banks unrooted.
  - Rooted afterwards by the relay's names probe on runs 41500705985 and 41501731134. Both records carry
    `targetArchExcluded`:
    - internal/cpu: the 7 tests in `cpu_x86_test.go` and `datacache_x86_test.go` (`//go:build 386 || amd64`);
    - gc/scan: TestExpandAVX512, TestFilterNilAVX512 and TestScanSpanPackedAVX512, from its `*_amd64_test.go` files.
  - This is seat #8 (a)'s per-OS withdrawal (4f245d9f4b, COORD's option-2 ruling): a test that a shipped darwin
    architecture does not build leaves both sides on both RIDs.
  - The BOARD amendment of 2026-10-26 (d780a56fbc) predicted exactly these counts for this re-bank: internal/cpu 2
    and gc/scan 3092.
- crypto/internal/fips140cache: TestCache passes the 15 m deadline on both RIDs. This is the discovery pass's
  finding #1, the GC-liveness root in golib's `heap(in T, out)`. G's seat fixes it.
- log (24/0, was 23/1) and math/big (249/2, was 248/3) read DISC on both RIDs. COORD recorded them as retirement
  candidates (20:05Z).
- internal/synctest (C1-8): a relay probe (relay a68242c62a, on run 41501731134) shows the C# host DIED in
  TestWaitGroupManyBubbles on both RIDs.
  - The error is `System.OutOfMemoryException` at `Thread.StartCore`, reached from `GoroutineThreadPool.Worker.Start`
    via `Goroutine.StartWithCreator`.
  - The test runs 100 bubbles of 100 blocked goroutines each. The thread pool could not start another OS thread.
  - TestWeak has no C# verdict on either RID: it was never reached. It is not evidence for the GC-reachability class.
  - TestHappensBefore has no verdict on arm64 and reads infrastructure-error on x64.
  - Windows banks 59/0 for this row, so this is a darwin finding. The candidate cause is the darwin per-process
    thread limit against one OS thread per blocked goroutine. It is a candidate, not rooted.
- runtime/debug banks 8/1, equal to its segment. Its record lists 2 disclosures, only one of which diverged.
