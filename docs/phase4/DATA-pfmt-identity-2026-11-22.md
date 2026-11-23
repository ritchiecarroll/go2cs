# `%p` against `unsafe.Pointer`: one Go pointer, two numbers (2026-11-22)

Lane H, owner by COORD's ruling on C2's BOARD entry of 2026-11-22 ("`%p` of a pointer and `unsafe.Pointer` of it
print different addresses"). This record holds each prediction, committed before its measurement, then its score
appended below it. A prediction is never edited after its measurement.

## 1. The red's prediction (committed before the run)

**Instrument.** The behavioral program `src/tests/Behavioral/PointerPrintIdentity`, converted at train 4's tip
`afb01dfb6c` on windows/amd64 and run there. It prints only booleans. Under Go 1.26.11 every line reads `true`.
`check` prints three per pointer: `%p` of p equals `%v` of `unsafe.Pointer(p)`; `%p` equals `%#x` of
`uintptr(unsafe.Pointer(p))`; and `reflect.ValueOf(p).Pointer() == uintptr(unsafe.Pointer(p))`.

**Mechanism, read from the code.** `%p` reaches `reflect.Value.UnsafePointer`, which answers
`reflectPointerToken`, whose pointer arm is the box's `PointerOrderToken` (`reflect/value_impl.cs`). It never pins.
`unsafe.Pointer(&x)` is `FromPinnedBox`, which takes the box's `uintptr` conversion (`golib/ж.cs`). That conversion
answers the order token only where `StorageKind` is `None`; otherwise it answers a machine address, pinned when the
kind is `Pinnable`. So the two numbers agree exactly where `StorageKind` is `None`.

**Predicted lines.**

| Line | Box, `StorageKind` | Predicted |
|---|---|---|
| int, %p first | StandardBox int, Pinnable | false |
| int, %p again | the same token twice | true |
| int, address first | StandardBox int, Pinnable | false |
| int local | StandardBox int, Pinnable | false false false |
| new(int64) | StandardBox int64, Pinnable | false false false |
| reference-free field | FieldRefBox int in a struct holding a string, Unpinnable | false false false |
| reference-bearing field | FieldRefBox string, None | true true true |
| slice element | ElemRefBox int32, Pinnable | false false false |
| array | StandardBox [4]byte, the pinned-data arm | false false false |
| string local | StandardBox string, None | true true true |
| struct with a string | StandardBox of that struct, None | true true true |
| one variable, one %p | the same token twice | true |
| two variables, two %p | two tokens | true |

**Falsifiers.** Any `None` row reading false, or any machine-address row reading true. The reference-free field is
the least certain row: its root holds a string, so its address is taken unpinned. It still answers a machine address,
never the token.

## 2. The red's score (appended after the run)

**MET, all 13 lines, as predicted.** Converted at `afb01dfb6c`, built Release with 0 warnings and 0 errors, run on
windows/amd64. Every row whose box answers a machine address reads false; every `None` row reads true; the two
same-token rows read true.

## 3. The prototype's prediction (committed before the run)

**The change.** reflect's pointer arm answers `ManagedPointerTokens.PointerNumber(p)`, the box's own `uintptr`
conversion, in place of its order token. The exception is an `Unpinnable` box, which keeps the token. `IGoReflectBox`
(internal) gains `StorageKind`, which every implementer already declares through `ж<T>`.

**Predicted lines,** with the same program and instrument as section 1, rebuilt over the changed golib and reflect:
every line reads true, except `reference-free field`, which stays `false false false`. That box is `Unpinnable`, so
it keeps the token while its `uintptr` is the unpinned address. **Falsifier:** any other line reading false, or that
row reading true.

## 4. The prototype's score (appended after the run)

**MISSED.** The second and third columns turned true on every row except `reference-free field`, as predicted. But the
first column, `%p` against `%v` of `unsafe.Pointer(p)`, read false on all 11 `check` and order rows. That includes
the four `None` rows, which read true before the change. `int, %p first` and `int, address first` read false.
- **Cause, by reading.** `%v` of an `unsafe.Pointer` reflects over the `Pointer` value itself, a
  `StandardBox<uintptr>` whose order token is the number it carries (`unsafe/unsafe.cs:331`). `PointerNumber` took
  that box's own conversion instead, which pins the slot holding the number and answers the slot's address. An
  `unsafe.Pointer` value is a number, not a box over storage.
- **Fix:** `PointerNumber` keeps the order token for an `IUnsafePointer`, as it does for an `Unpinnable` box.

## 5. The second prototype's prediction (committed before the run)

The same program and instrument. Every line reads true except `reference-free field`, which stays
`false false false`. **Falsifier:** any other line reading false, or that row reading true.

## 6. The second prototype's score, and its exploration (appended after the runs)

**MET, all 13 lines.** Every line reads true except `reference-free field`, which reads `false false false`, as
predicted. That residual is the `Unpinnable` kind: a reference-free field inside a struct that holds a reference.
Its `uintptr` is an address that nothing holds still, so its projection keeps the token to stay stable.

**Exploration, FILTERED, not a gate.** Neither run below was predicted, and neither is a gate reading.
- GolibTests, Release, filtered to the nine classes that read reflect's pointer number or the token layout
  (PointerTokenLayoutTruth, ReflectHashTokenBand, ReflectNewAtResolution, UintptrReinterpretView, ZeroBaseIdentity,
  NamedPointerAssignability, RuntimePinner, AliasOverlapRace, GoCgoArgs): 115 passed, 0 failed.
- BehavioralTests OutputComparisonTests, Release, filtered to ten names, 9 of which matched: 7 passed, 2 failed.
  - `PointerPrintIdentity` fails on its residual row alone, because Go's oracle prints true there.
  - **`ReflectNewAtField` is a REGRESSION.** go-cmp's `NewAt(f.Type, v.UnsafeAddr() + f.Offset)` over the
    reference-free struct `Q` reads `4 0 0` where Go reads `4 5 6`, and the write to `b` is lost (`5`, where Go
    reads `50`).
  - Cause, by reading: `UnsafeAddr` is `Addr().UnsafePointer()`, so it now answers the pinned machine address.
    `NewAt` resolves EXACT ONLY, through the token table (`GoReflect.ResolveNewAtPointee`, ruled 2026-10-04). A
    pinned address plus an offset misses that table and falls to a zero box. The field at offset 0 still resolves.
  - The direct Go form, `unsafe.Pointer(uintptr(unsafe.Pointer(&q)) + off)`, already reaches `NewAt` as a pinned
    address. So that route misses today, before any change. Not measured.

## 7. COORD's ruling, the withdrawn carve-out, and the next prediction

**Ruling (COORD, 2026-11-22): option A, NARROW.**
- `Value.Pointer` and `Value.UnsafePointer` answer `PointerNumber`, and so does fmt's `%p` through them.
- `Value.UnsafeAddr` keeps the order token, so go-cmp's `NewAt(f.Type, v.UnsafeAddr() + f.Offset)` is unchanged.
- Option B (a pinned address plus an offset resolving through `NewAt`) is refused. It would reopen the 2026-10-04
  EXACT ONLY scope for a shape no measured consumer needs.
- Two residuals are disclosed in pointers.md, replacing "Not covered":
  - for reference-free storage, `UnsafeAddr` differs from `uintptr(unsafe.Pointer(&field))`;
  - an `Unpinnable` box keeps its token.

**Withdrawn: the fmtsort carve-out.** COORD first ruled that internal/fmtsort keep the token, so that printing a
pointer-keyed map would pin nothing. The converted `compare` calls `aVal.Pointer()` itself. So keeping the token
there would have cost a converter registration displacing `compare` to a hand-own (which drifts at every hop), a new
public reflect accessor, and a regen. That bill was paid to avoid a cost nobody had measured. COORD withdrew the
carve-out: fmtsort stays as converted, and each pointer key pins once, like any other `Pointer()` observer. Instead,
the cost canary gains a pointer-keyed-map arm. If that arm reads more than 2x wall time, or a pinned count that does
not return to baseline once the map is dropped, it is posted before seating.

**The change since section 6.**
- `reflectPointerToken` takes `asNumber`. `UnsafeAddr` calls it with `false` and keeps the token, where it used to
  call `Addr().Pointer()`.
- The program's `Unpinnable` row moves off the output test, because Go's oracle prints true there. `reference-free
  field` is now a field of a reference-free struct, which is `Pinnable`.
- A `zero-size` row is added: `new(struct{})`, whose token and number are both the zerobase's but differ.

**Prediction (committed before the run).**
- The converted program, Release, windows/amd64, at this commit: all 14 lines true.
- The filtered output tests `PointerPrintIdentity` and `ReflectNewAtField` both pass.
- **Falsifier:** any false line, or either test failing.

## 8. Section 7's score, and the leak and cost arms (named and predicted before they run)

**Section 7: MET.** The converted program reads true on all 14 lines. The filtered output tests `PointerPrintIdentity`
and `ReflectNewAtField` both pass, 2 of 2.

**Instrument.** A C# console probe, kept in H's scratch and not in the tree. It references `src/core/fmt` and is
built Release at two trees on this box (windows/amd64): BASE is `git archive afb01dfb6c` of src/core, src/gen and
src's props, and TIP is this branch's src/core. Each arm runs in its own process, and every wall reading is the floor
over 5 rounds. Pinned objects are the GC's own `PinnedObjectsCount` after a forced, blocking, compacting full
collection, followed by `WaitForPendingFinalizers` and a second collection. Live registry entries are the
`ManagedPointerTokens` table entries whose weak target is alive, read by reflection.

**Arm L, the leak (the stop-line).** 100,000 distinct transient `ж<long>` boxes. Each is printed once with
`fmt.Sprintf("%p", box)`, and all are held. Read pinned objects and live entries, then drop the boxes, collect, and
read both again.
- Predicted at TIP: while the boxes are held, pinned objects rise by about 100,000 over baseline. After the drop
  and the collections, both readings are back at baseline (pinned exactly; live entries within 16).
- Predicted at BASE: no rise at any point.
- **Stop-line:** a TIP count that stays above baseline after the drop.

**Arm C1, a %p-heavy wall, distinct boxes.** 200,000 fresh boxes, each printed once.
- Predicted: TIP/BASE between 1.5x and 6x, because each box pays one pin, one `PinnedBuffer` and one registry
  record.

**Arm C2, a %p-heavy wall, one box.** One box printed 1,000,000 times.
- Predicted: TIP/BASE between 0.8x and 1.4x. A repeat print takes the idempotent pinned-registry early return,
  where BASE takes the token registry's.

**Arm C3, a pointer-keyed map (COORD's threshold).** A `map[*int]int` of 10,000 keys, printed with `fmt.Sprint` 20
times per round. Then the map is dropped, and pinned objects are read after the collections.
- Predicted: TIP/BASE floor between 0.8x and 1.5x. Only the first print of each key pins, and the floor round is a
  repeat. TIP's first round is reported separately, predicted under 2x BASE's first round.
- Pinned objects return to baseline after the drop.
- **COORD's threshold:** more than 2x wall time, or a pinned count that does not return to baseline, is posted
  before seating.

## 9. The arms' score (appended after two readings)

The probes were built at TIP `28b8d4423d` and BASE `afb01dfb6c`. Their `golib`, `go.reflect` and `go.fmt` assemblies
differ by hash. There were two readings, the second in reversed arm order, one process per arm, on one box, with
nothing else running.

| Arm | Reading 1, BASE / TIP | Reading 2, BASE / TIP | TIP/BASE | Predicted | Score |
|---|---|---|---|---|---|
| L pinned: baseline, held, after the drop | 2, 2, 2 / 3, 100,003, 3 | 2, 2, 2 / 3, 100,003, 3 | +100,000 held, back to baseline | the same | **MET** |
| L live entries: baseline, held, after the drop | 5, 100,005, 5 / 5, 100,005, 5 | the same | unchanged | within 16 of baseline | **MET** |
| C1, 200k distinct boxes, ms | 302.6 / 289.1 | 249.8 / 277.3 | 0.96x, 1.11x | 1.5x to 6x | **MISSED, cheaper** |
| C2, one box 1M times, ms | 967.9 / 913.4 | 937.9 / 934.5 | 0.94x, 1.00x | 0.8x to 1.4x | **MET** |
| C3 floor, 10k-key map, 20 prints, ms | 437.2 / 421.6 | 426.3 / 405.9 | 0.96x, 0.95x | 0.8x to 1.5x | **MET** |
| C3 first round, ms | 1,357.4 / 1,324.8 | 1,401.6 / 1,273.1 | 0.98x, 0.91x | under 2x | **MET** |
| C3 pinned after the drop | 2 / 3 (baseline 2 / 3) | 2 / 3 (baseline 2 / 4) | at or under baseline | back to baseline | **MET** |

- **The stop-line held.** A `%p` pin dies with its box. 100,000 pins come and go with their boxes, and the table
  keeps no live entry for a dead box.
- **COORD's map threshold is not reached:** 0.91x to 0.98x, against a 2x limit.
- **C1's miss.** The prediction priced a pin as extra work on top of the token path. But BASE already pays a
  dictionary write and a `WeakReference` per distinct box (`Register` of the token). TIP pays `RegisterPinned` and
  one `PinnedBuffer` instead, and `Sprintf` itself dominates both.
- **One residual, bounded and not proportional to N.** TIP rests at 1 or 2 more pinned objects than BASE. Arm L
  prints inside a frame that has returned, and it reads exactly its own baseline after 100,000 pins. So the extra
  objects are the most recent box or two, still reachable from the probe's own `Main` frame. Main holds its warm-up
  box and its loop variable, which tier-0 code keeps alive. That attribution is by reading and not measured.
