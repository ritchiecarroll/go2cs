# PREREAD — H6 sync and synctest share, go1.24.13 → go1.25.14 → go1.26.9 (DRAFT)

> **Point-in-time draft, read 2026-10-15** on the hop tip `f7d8f0b135`, for H6's attributed review of lane R's
> share (PLAN-hop-go126 §5, H6: "the sync and synctest hand-owns"). Docs only; nothing here changes code. It
> follows the shape of `PREREAD-h6-runtime-go126.md` and reads the same instrument the same way. Amended with
> dated blocks, never rewritten.

## 1. What was measured, and how

- **The population** is `src/handown-census.ps1`'s at this tip (184 marked files), run three times: **leg 1**
  1.24.13 → 1.25.14, **leg 2** 1.25.14 → 1.26.9, and **end to end** 1.24.13 → 1.26.9. The end-to-end totals
  match the runtime pre-read's: 79 substantive, 6 trivial, 36 untouched, 63 no-upstream.
- **The share** is every marked file under `sync`, `sync/atomic`, `internal/sync` and `internal/synctest`:
  **15 files**. End to end, **7 are substantive** and 2 trivial. The other 6 (`sync/cond.cs`, `cond_impl.cs`,
  `mutex.cs`, `rwmutex.cs`, `poolqueue.cs`, `internal/sync/runtime_impl.cs`) are untouched or have no upstream
  counterpart on both legs.
- **Per file and per leg**: lines are `git diff --no-index --numstat` of the upstream file. Declarations are the
  top-level Go declarations whose comment-free, whitespace-collapsed rendering differs, read with the runtime
  pre-read's `declchg` spec (rebuilt to that spec for this read). "Owned" names a changed declaration the
  hand-own declares, read by hand here because the share is small.

## 2. Structural findings (before the per-file reading)

1. **One build break at H5, not H6.** `runtime_SemacquireWaitGroup` is DECLARED in the converted `sync/runtime.cs`,
   which H5 regenerates. 1.25 gives it a second parameter, `synctestDurable bool` (sync/runtime.go:17). Its BODY is
   in the hand-owned `sync/runtime_impl.cs` with the 1.24 one-parameter signature. Once the declaration
   regenerates, the old body matches no defining declaration (CS0759) and the build is red. **The body's new
   signature has to land with H5**, as a pre-seat, the way G's pre-seats do.
2. **The synctest association API is new runtime surface.** 1.25's `internal/synctest` adds `IsInBubble` and three
   pulls behind `Associate`, `Disassociate` and `IsAssociated` (`associate`, `disassociate`, `isAssociated`). In Go
   the runtime attaches a "special" to the pointee. golib's bubble (`go.golib.SyncTestBubble`) has no association
   state today, so a managed association map lives in golib. That is golib's seam, not the hand-own's.
3. **`WaitGroup` is native here, so its 1.25 rewrite arrives as design, not as a diff.** `sync/waitgroup.cs` is a
   reimplementation (a guarded counter and a latch), not Go's packed `state` word. Go 1.25's new `Go` method, the
   bubble association in `Add` and `Wait`, and 1.26's change to `Go` all have to be expressed against that design.
4. **Two native files are unreached.** The one-line upstream changes in `internal/sync/hashtriemap.go` (a longer
   panic text) and `sync/pool.go` (`runtime_StoreReluintptr` loses its result) do not reach observable behavior
   in the native `hashtriemap.cs` and `pool.cs`. See §3.

## 3. Per file

| hand-own (kind) | upstream | L1 | L2 | e2e | decl (L1/L2) | owned | first class | born |
|---|---|--:|--:|--:|--:|---|---|---|
| `sync/waitgroup.cs` (whole file, native) | `sync/waitgroup.go` | +128/−16 | +23/−4 | +149/−18 | 4/2 | L1: `Add`, `Wait`, `Go` (absent); L2: `Go`, `Wait` | **new API + semantic** (1.25 `WaitGroup.Go`; bubble association, two fatal texts; 1.26 `Go` re-panics without `Done`) | both |
| `internal/synctest/synctest_impl.cs` (companion) | `internal/synctest/synctest.go` | +43/−1 | 0 | +43/−1 | 11/0 | L1: four new pulls (`IsInBubble`, `associate`, `disassociate`, `isAssociated`) | **new subsystem** (bubble association) | 1.25 |
| `sync/oncefunc.cs` (whole file) | `sync/oncefunc.go` | +59/−50 | 0 | +59/−50 | 3/0 | L1: `OnceFunc`, `OnceValue`, `OnceValues` | semantic (one struct, one allocation; `f` released even when it panics) | 1.25 |
| `sync/runtime_impl.cs` (companion) | `sync/runtime.go` | +1/−1 | 0 | +1/−1 | 1/0 | L1: `runtime_SemacquireWaitGroup` | **build-critical at H5** (signature; see §2.1), then semantic (the durable wait) | 1.25 |
| `sync/once.cs` (hand-finished) | `sync/once.go` | +4/−4 | +1/−1 | +5/−5 | 3/0 | L1: `Once`, `Do`, `doSlow` | mechanical (`done` becomes `atomic.Bool`; L2 is a comment) | 1.25 |
| `sync/pool.cs` (whole file, native) | `sync/pool.go` | 0 | +1/−1 | +1/−1 | 0/1 | L2: `runtime_StoreReluintptr` (its declaration is in this file) | mechanical (the result dropped; unused) | 1.26 |
| `internal/sync/hashtriemap.cs` (whole file, native) | `internal/sync/hashtriemap.go` | 0 | +3/−3 | +3/−3 | 0/2 | none (the native trie has no "ran out of hash bits" path) | unreached (panic text; a parameter rename) | 1.26 |
| `sync/atomic/type.cs` | `sync/atomic/type.go` | +14/0 | 0 | +14/0 | 0/0 | — | trivial (doc comments: "must not be copied after first use") | 1.25 |
| `sync/atomic/value.cs` | `sync/atomic/value.go` | 0 | +2/−4 | +2/−4 | 0/0 | — | trivial | 1.26 |

## 4. Per file: what H6 owes

### 4.1 `sync/runtime_impl.cs`, the H5 pre-seat

- Re-sign the body: `runtime_SemacquireWaitGroup(ж<uint32> s, bool synctestDurable)`, matching the regenerated
  declaration.
- Go's `sync_runtime_SemacquireWaitGroup` parks with `waitReasonSyncWaitGroupWait`, or with
  `waitReasonSynctestWaitGroupWait` when `synctestDurable` is true (runtime/sema.go:109-115). Only the durable
  reason counts as idle for the bubble's clock. The body passes the flag through to golib's
  `RuntimeSemaphore.Acquire` as the wait reason.
- **Size: 0.25 d.** It must merge with H5, or H5's build is red.

### 4.2 `internal/synctest/synctest_impl.cs` and golib's association map

- `IsInBubble` forwards to `SyncTestBubble.Current is not null`.
- `associate`, `disassociate` and `isAssociated` need a golib association keyed by the POINTEE. A managed
  `unsafe.Pointer` to a `WaitGroup` names its box, so the key is the box's identity: a `ConditionalWeakTable`
  from the referent to its bubble, which never keeps a `WaitGroup` alive. `associate` returns `Unbubbled`
  outside a bubble, `CurrentBubble` after the first association or a repeat in the same bubble, and
  `OtherBubble` from another bubble (runtime/synctest.go:381-430, `getOrSetBubbleSpecial`).
- **One rule a managed map does not get for free:** Go answers `Unbubbled` for a pointer that is not in the heap
  (`spanOfHeap` is nil), so a package-level `WaitGroup` is never associated. A converted package-level variable is
  a static field here, and the map must give it the same answer rather than associating it.
- **Size: 0.5–1 d.** The map is a golib change, so it is golib's seam: posted as a SEAM line before it is cut.

### 4.3 `sync/waitgroup.cs`

- **`WaitGroup.Go(f)`** (1.25 API): `Add(1)`, then a goroutine running `f` whose deferred function calls `Done`.
  Under 1.26, a panic in `f` is re-raised and `Done` is not called (`if x := recover(); x != nil { panic(x) }`).
- **Bubble association in `Add`**: inside a bubble, `Associate(wg)`. `OtherBubble` is fatal ("sync: WaitGroup.Add
  called from multiple synctest bubbles"), and so is a mix of bubbled and unbubbled `Add`s ("sync: WaitGroup.Add
  called from inside and outside synctest bubble"). The counter reaching zero disassociates.
- **`Wait`**: a bubbled wait on an associated group is durable (§4.1). 1.26 also disassociates on a zero-count
  `Wait` with no waiters.
- **Size: 1–1.5 d**, after §4.2.
- **Pre-existing, not 1.25/1.26** (a finding for the record, not this bill): the native file carries Go's
  "negative WaitGroup counter" panic but neither "WaitGroup misuse: Add called concurrently with Wait" nor
  "WaitGroup is reused before previous Wait has returned".

### 4.4 `sync/oncefunc.cs`

- Port 1.25's shape while keeping the hand-own's reason for existing (the panic VALUE replayed to every later
  caller across a foreign unwind):
  - one struct holding `f`, the `once`, `valid` and `p`, so one allocation;
  - `f` released inside the deferred function, so it is dropped even when `f` panics. 1.24 dropped it only on
    success.
- **Size: 0.25–0.5 d.** The release-on-panic change is observable as a collectable `f`, and the allocation count
  through `AllocsPerRun`.

### 4.5 `sync/once.cs`, `sync/pool.cs`, `internal/sync/hashtriemap.cs`, `sync/atomic`

- `once.cs`: `done` becomes `atomic.Bool`. `Do`'s zero-allocation fast path keeps its form through
  `atomic.Bool`'s `ref` receiver (`!o.done.Load()`), and `doSlow` stores `true`. **0.25 d.**
- `pool.cs`: `runtime_StoreReluintptr` drops its result in both the declaration (this file) and the body
  (`runtime_impl.cs`). Nothing reads it. **0.1 d.**
- `hashtriemap.cs`: nothing owed. The native trie has no path that reaches Go's "ran out of hash bits" panic, and
  `Store`'s parameter rename is not observable.
- `sync/atomic/type.cs`, `value.cs`: nothing owed (doc comments).

## 5. Seams and order

- **H5 (pre-seat, R):** §4.1's signature, merged with H5.
- **golib (seam):** §4.2's association map. Posted as a SEAM line naming golib's owner before it is cut.
- **H6 (R), after the golib map:** §4.2's forwards, §4.3, §4.4, §4.5.
- **Not this share:** time's new pull `runtimeIsBubbled` (time/time.go:1328) sits in time's hand-owned companion;
  NOTES §3.3 names it beside these.

## 6. What the share adds to the H6 bill

About **2.5–4 d**: 0.25 (H5 pre-seat) + 0.5–1 (association) + 1–1.5 (`WaitGroup`) + 0.25–0.5 (`oncefunc`) +
0.35 (the small three). The red-first cases, by name:

- a bubbled `WaitGroup.Wait` lets the fake clock advance;
- `WaitGroup.Add` from two bubbles is fatal with Go's text;
- `WaitGroup.Go` runs `f` and is waited for, and a panicking `f` is re-raised without `Done`;
- `OnceFunc` drops `f` after `f` panics;
- `Once`'s fast path still allocates nothing.

<!-- Sources, all read 2026-10-15 at hop tip f7d8f0b135 with go1.24.13, go1.25.14 and go1.26.9 side by side:
  census: src/handown-census.ps1 -FromGoRoot <sdk>/go1.24.13 -ToGoRoot <sdk>/go1.25.14 (and the other two legs);
  sync/runtime.go:17 (the synctestDurable parameter); sync/waitgroup.go:60, :77-144, :160-218, :236-260 at go1.26.9;
  internal/synctest/synctest.go:20-62 at go1.26.9; sync/oncefunc.go at go1.25.14; sync/once.go:27 at go1.26.9;
  sync/pool.go:318 at go1.26.9; internal/sync/hashtriemap.go:181, :199 at go1.26.9.
  Hand-own reads: src/core/sync/runtime.cs:18 (the converted declaration), runtime_impl.cs:58 (the body),
  pool.cs:310 (the declaration it carries), waitgroup.cs:72 (the one counter panic present). -->
