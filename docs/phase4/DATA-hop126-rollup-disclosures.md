# DATA: roll-up aggregate disclosures at the go1.26.10 re-bank, read at batch 24

**Point-in-time record, 2026-10-23, lane H.** Read at hop tip `ddebd3bed1` (batch 24) on windows/amd64, go1.26.10,
Release with tiering off, one `-tests -test-action all` run per row, unfiltered. It answers the re-bank checklist's
roll-up item: which parent rows fail only through their children, what each one needs, and the disclosure text that
need takes, drafted in the manifest's own shape. It follows
[`DATA-hop126-deferred-allocs.md`](DATA-hop126-deferred-allocs.md), which found the parents at `8c091fc3aa`.

## How the pipeline discloses a parent today

A parent is never given an entry of its own. The compare oracle reclassifies a `t.Run` parent as disclosed when
its Go=pass / C#=fail divergence is purely the roll-up of its children. Four conditions must hold: the parent prints
no failure output of its own, it has no own entry, at least one descendant is disclosed, and NO descendant is a
mismatch. Any undisclosed failing child keeps the parent a strict mismatch, so the rule can never mask a failure
(`matchTerminalStatuses`, guarded by `TestDisclosedParentAggregation` in `disclosedParentAggregation_test.go`).

An aggregated parent has no records and no pin, and the proof page and the console summary name its class
`aggregate` (COORD ruling 2026-10-13). The comparison lists it among the disclosed verdicts with an empty class,
as in `TestNISTECAllocations ():` below.

So a red parent is never fixed by writing its own entry: an entry on a parent with no output of its own has
nothing for its signature to match. It is fixed by disclosing each undisclosed failing child.

**The committed manifests agree.** Every subtest disclosure under `src/core` was censused. 22 top-level parents (79
counting intermediate levels) are disclosed purely by aggregation, and none carries an own entry.

| Package | Parent | Disclosed children |
|---|---|---|
| `crypto/internal/fips140test` | `TestNISTECAllocations` | 4 |
| `crypto/sha3` | `TestAllocations` | 4 |
| `crypto/x509` | `TestSetFallbackRoots` | 9 |
| `encoding/binary` | `TestSizeAllocs` | 4 |
| `internal/runtime/maps` | `TestTableGroupCount` | 44 |
| `internal/runtime/maps` | `TestTableIterationGrowDuplicate` | 2 |
| `log/slog` | `TestAlloc` | 12 |
| `net/netip` | `TestAddrStringAllocs` | 5 |
| `net/netip` | `TestNoAllocs` | 47 |
| `net/netip` | `TestParsePrefixAllocs` | 2 |
| `reflect` | `TestValuePointerAndUnsafePointer` | 6 |
| `runtime` | `TestStartLine` | 2 |
| `runtime` | `TestTracebackInlined` | 4 |
| `runtime` | `TestTracebackSystem` | 2 |
| `runtime` | `TestUserArenaLiveness` | 2 |
| `runtime` | `TestZeroConvT2x` | 13 |
| `strconv` | `TestAllocationsFromBytes` | 10 |
| `sync` | `TestOnceXGC` | 6 |
| `syscall` | `TestCloneNEWUSERAndRemap` | 2 |
| `unique` | `TestCanonMap` | 1 |
| `unique` | `TestCanonMapBadHash` | 1 |
| `unique` | `TestCanonMapTruncHash` | 1 |

Each of these parents stays disclosed at the re-bank only while every failing child under it is disclosed. A child
new at go1.26.10, or one whose failure line moved off its signature, turns the parent red with it.

## The rows, read at `ddebd3bed1`

- **`crypto/internal/fips140/nistec`:** validated, 44 tests, 2 declarations excluded. No parent fails here; the
  allocation parent of this family is `crypto/internal/fips140test` · `TestNISTECAllocations` (the H10 relocation
  moved it there).
- **`crypto/internal/fips140test`:** `TestNISTECAllocations` now AGGREGATES. It is listed disclosed with an empty
  class over its four disclosed curve subtests, so the parent finding of the earlier DATA record is resolved at
  batch 24 and owes nothing.
  - `TestVersion` passes on both sides (the build-info stamp seat, batch 24).
  - The one mismatch is `TestMLDSAAllocations`, new at go1.26.10. It is a top-level test with no parent, and it
    needs an own deferred entry (drafted below).
  - `TestACVP` fails on BOTH sides on this host, so it is a match, not a mismatch.
- **`log/slog`:** both parents are red for the reason the rule states.
  - `TestAlloc` is red because of one undisclosed child, `TestAlloc/2_pairs_disabled_inline`. Its other 12
    children are disclosed.
  - `TestJSONAllocs`, new at go1.26.10, is red because neither child, `message` nor `attrs`, is disclosed.
  - Disclosing those three children makes both parents aggregate. Neither parent takes an entry.

## Drafted entries, in the manifest's shape

Four child entries, for `src/core/log/slog/go2cs_test_disclosures.json` (three) and
`src/core/crypto/internal/fips140test/go2cs_test_disclosures.json` (one), at the re-bank.

Each signature is the stable part of the failure line the converted host prints, matching the siblings already in
those manifests. Each reading is this record's. The charge sites are NOT read here: each `plan` says so, and names
the probe the re-bank owes, rather than a family this record did not measure.

```json
{
  "name": "TestAlloc/2_pairs_disabled_inline",
  "class": "deferred",
  "signature": "allocs, want ",
  "reason": "wantAllocs(t, 0, ...) over Logger.Log at LevelInfo with two key/value pairs, on a Logger over DiscardHandler, so the level check fails and nothing is formatted. go1.24.13 asserted wantAllocs(t, 2, ...) here and the converted side met it; go1.26.10 tightened the budget to 0, and the converted side still counts 2 allocations per run. The count is unchanged from batch 19 to batch 24 on windows/amd64.",
  "want": "0 allocations per run, exactly",
  "reading": "2 per run -- 10 go2cs-runtime object allocations (920 bytes) counted over 5 runs at ddebd3bed1 (batch 24), windows/amd64, Release with tiering off; unit COUNT, a LOWER BOUND; the test printed `logger_test.go:358: got 2 allocs, want 0`. The same reading at 8c091fc3aa (batch 19).",
  "plan": "Charge sites not yet read: record the stack of every charged allocation (the AllocationCounter stack probe unique's TestMakeAllocs entry used) and name each site's family, as the sibling TestAlloc/2_pairs entry does. A want-0 entry retires only at zero bytes."
}
```

```json
{
  "name": "TestJSONAllocs/message",
  "class": "deferred",
  "signature": "allocs, want ",
  "reason": "wantAllocs(t, 0, ...) over Logger.LogAttrs(ctx, LevelInfo, \"hello world\") with no attributes, through a JSONHandler writing to io.Discard. The test is new at go1.26.10. Go budgets zero allocations; the converted side counts 12 allocations per run on the same path.",
  "want": "0 allocations per run, exactly",
  "reading": "12 per run -- 60 go2cs-runtime object allocations (44,520 bytes) counted over 5 runs at ddebd3bed1 (batch 24), windows/amd64, Release with tiering off; unit COUNT, a LOWER BOUND; the test printed `json_handler_test.go:157: got 12 allocs, want 0`. The same reading at 8c091fc3aa (batch 19).",
  "plan": "Charge sites not yet read: record the stack of every charged allocation (the AllocationCounter stack probe) through JSONHandler.Handle and name each site's family. A want-0 entry retires only at zero bytes."
}
```

```json
{
  "name": "TestJSONAllocs/attrs",
  "class": "deferred",
  "signature": "allocs, want ",
  "reason": "wantAllocs(t, 1, ...) over Logger.LogAttrs(ctx, LevelInfo, \"hello world\", ...) with five attributes -- String, Int, Bool, Any over an error, Any over a three-field struct -- through a JSONHandler writing to io.Discard. The test is new at go1.26.10. Go budgets exactly one allocation; the converted side counts 28 allocations per run on the same path.",
  "want": "1 allocation per run, exactly",
  "reading": "28 per run -- 140 go2cs-runtime object allocations (75,200 bytes) counted over 5 runs at ddebd3bed1 (batch 24), windows/amd64, Release with tiering off; unit COUNT, a LOWER BOUND; the test printed `json_handler_test.go:164: got 28 allocs, want 1`. The same reading at 8c091fc3aa (batch 19).",
  "plan": "Charge sites not yet read: record the stack of every charged allocation (the AllocationCounter stack probe) through JSONHandler.Handle and its attribute appends, and name each site's family. As in the sibling TestAlloc entries, Go's budget is an `any` conversion the counter never charges, so the entry is deferred on its counted surplus."
}
```

```json
{
  "name": "TestMLDSAAllocations",
  "class": "deferred",
  "signature": "expected 5 allocations, got ",
  "reason": "AllocsPerRun(100, ...) over an ML-DSA-44 round trip: GenerateKey44, PrivateKey.Bytes, NewPrivateKey44, Equal, PublicKey().Bytes, NewPublicKey44, Sign, Verify. Go allows at most 5 allocations per run -- by the test's own comment, the PrivateKey structs k and kk, the PublicKey struct pk, and the pkBytes and sig byte slices -- and 6 when fips140.Enabled adds the PCT's signature. The converted side counts about 4,600 allocations per run. The test is new at go1.26.10.",
  "want": "at most 5 allocations per run",
  "reading": "4,642 per run -- 464,204 go2cs-runtime object allocations (222,704,072 bytes) counted over 100 runs at ddebd3bed1 (batch 24), windows/amd64, Release with tiering off; unit COUNT, a LOWER BOUND; the test printed `mldsa_test.go:180: expected 5 allocations, got 4642.0`. At 8c091fc3aa (batch 19) the same test read 4,081 per run; the rise is not rooted (see the record's open finding).",
  "plan": "Charge sites not yet read: record the stack of every charged allocation (the AllocationCounter stack probe) over one round trip. A hypothesis for the probe to test, not a measurement: ML-DSA's working polynomials are the kind of non-escaping temporaries docs/phase4/DESIGN-nonescaping-locals.md (REC-B) names for TestXAESAllocations."
}
```

## Open finding: `TestMLDSAAllocations` rose between batch 19 and batch 24

On windows/amd64, `TestMLDSAAllocations` read 4,081 allocations per run (408,117 over 100 runs, 190,760,736 bytes)
at `8c091fc3aa` (batch 19), and 4,642 per run (464,204, 222,704,072 bytes) at `ddebd3bed1` (batch 24). That is 561
more per run, +13.7% by count and +16.7% by bytes.

The three `log/slog` readings above are byte-identical across the same two tips, so the counter and the host are
unchanged in what they charge on that path. The rise therefore comes from something the ML-DSA path reaches that
batches 20 through 24 changed. It is not rooted here, and is not a seat of this record.

A two-tip bisect over the converter and golib, with this one test as the probe, is the next step if the re-bank wants
the entry's reading to be explained rather than restated.

**Amended 2026-10-23 (lane H): the finding above is WITHDRAWN. There is no rise; the count is not reproducible.**

The bisect's control read the probe twice at each end, on windows/amd64 at the same config, `-tests -test-action all
-test-filter '^TestMLDSAAllocations$'`, with each tip's converter built from its own tree:

| Tip | Readings, allocations per run |
|---|---|
| `8c091fc3aa` (batch 19) | 4,081 (this record's earlier read), 4,151, 4,432 |
| `ddebd3bed1` (batch 24) | 4,642 (this record's read above), 4,071, 4,072 |

The two sets overlap: b19 spans 4,081–4,432 and b24 spans 4,071–4,642, so the 4,081 → 4,642 pair was two draws
from one spread, not a change between the tips. The bisect was not run, since a probe that does not repeat at a
fixed tip cannot order the tips between.

**Why the count varies.** The test builds a fresh random key and signs with fresh randomness on each of its 100 runs.
ML-DSA's `Sign` is a rejection-sampling loop (`mldsa.go`, the "Main rejection sampling loop"), and key expansion
samples by rejection too (`field.go`), so the number of iterations varies with the randomness. Go allocates nothing
per iteration, so its count is fixed at 5; the converted side allocates on every iteration, so its count follows the
random iteration count.

**What changes in the drafted `TestMLDSAAllocations` entry.** Its `reading` should state the spread rather than one
draw, for example "4,071 to 4,642 per run over six readings at batch 19 and batch 24, windows/amd64, Release with
tiering off; unit COUNT, a LOWER BOUND; the per-run count varies with ML-DSA's rejection-sampling iterations". The
"rose from 4,081" clause is dropped. Its signature, `expected 5 allocations, got `, is unaffected, since every
reading fails the same assertion. The `plan`'s probe should count allocations per Sign iteration, not per run.

<!-- Provenance. Lane H, 2026-10-23. Two worktrees at ddebd3bed1: claude/h-rollup-disclosures for this record, and a
     detached one for the row reads, so no pipeline output reached the record's branch. Rows read in sequence, never
     two conversions at once: log/slog 333 s (exit 1), crypto/internal/fips140/nistec 37 s (exit 0, validated 44),
     crypto/internal/fips140test 283 s (exit 1). Readings come from each row's go2cs_test_comparison.json and
     go2cs_test_results.json. The census read every committed src/core/**/go2cs_test_disclosures.json, and keyed
     each subtest name on its ancestors. -->
