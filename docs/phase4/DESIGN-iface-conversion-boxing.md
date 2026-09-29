# DESIGN (plan record) -- REC-F: the box an interface conversion allocates where Go's convT does not

> **Status: PLAN RECORD, 2026-09-29.** Minted by coordinator ruling 2026-09-28 22:01 (P1's runtime
> disclosure sizing, `claude/p1-disclosure-sizing` 3a574f110f) so that the interface-conversion
> family's `deferred` entries cite a record that names the stages which remove the counted allocation,
> under the 2026-09-05 deferred-class ruling (measured reading, named plan, re-measured every sweep).
> **Owner: P1** (the record); the stages are unowned seats until routed. Nothing is cut against this
> record yet. Every reading below is LINUX, master `2ff42f7a16`, Release, tiered off, the full runtime
> row. Feasibility and cost are UNMEASURED where they say so.

## 0. The class

A Go interface conversion (`e = v`, `i1 = v`, `m[v]` with an interface key, `e == v`) calls one of
the runtime's `convT*` helpers, and three of their paths allocate nothing:

1. **Small integers, zero included.** `convT16`, `convT32` and `convT64` return
   `&staticuint64s[val]` when `val < len(staticuint64s)` (256; runtime/iface.go:365-400), so a zero or
   one-byte integer value never allocates (the E8/I8 and zero-integer cases).
2. **Zero strings and slices.** `convTstring` and `convTslice` return `&zeroVal[0]` for the zero value
   (runtime/iface.go:421 and :441).
3. **Constants.** The compiler lays a constant operand out in read-only data and converts its address
   (`e = 99.0`).

go2cs converts all three to a C# boxing conversion of a value-typed Go value into `object`. The box is
emitted by the C# compiler, not by golib, so golib's allocation counter charges none of it and
`testing.AllocsPerRun` reports it through its BYTES fallback: 24 B for a boxed scalar, 32 B for a boxed
`@string` header, 56 B for a boxed `slice<T>` header (the rows' own unit notes).

## 1. Members (the entries that cite this record)

| Test | Case | Reading (bytes per run) | Removed by |
|---|---|---:|---|
| TestZeroConvT2x | E16, E32, E64, I16, I32, I64 | 24 | F1 |
| TestZeroConvT2x | Estr, Istr | 32 | F1 |
| TestZeroConvT2x | Eslice, Islice | 56 | F1 |
| TestZeroConvT2x | E8, I8 | 24 | F2 |
| TestZeroConvT2x | Econstflt | 24 | F3 |
| TestNonEscapingConvT2E | `m[0]`, key `any` | 24 | F1 |
| TestNonEscapingConvT2I | `m[TM(0)]`, key `I1` | 24 | F1 |
| TestCmpIfaceConcreteAlloc | `e == ts`, `i1 == ts`, `e == 1` | 72 (1 run) | F1 (`ts` is the zero TS) + F2 (`1`) |

TestZeroConvT2x's Econststr case already PASSES at master (a string constant); why it does not box is
not read here, and F3 reads it first.

## 2. Stages

- **F1 -- a cached box per type for the zero value** (Go's `zeroVal` rule, and the zero case of its
  `staticuint64s` rule). At an interface conversion of a non-pointer,
  non-interface value type `T`, the emission routes through one golib helper instead of C#'s implicit
  boxing: when the value equals `default(T)` it returns a process-wide cached box of `default(T)`,
  otherwise it boxes as today. Go's semantics allow the sharing: an interface's dynamic value is
  immutable, and no Go operation can reach the boxed copy to write it. The golib-side question to
  settle first is whether any golib path unboxes by reference and writes (an `Unsafe.Unbox` store); a
  shared box would make that visible across conversions, so such a path is refused by name before F1
  lands. Converter change plus golib helper; its corpus footprint is every conversion site and is
  UNMEASURED.
- **F2 -- small-integer boxes.** Inside the same helper, integer kinds whose value is below 256 return
  a box from a per-type 256-entry cache (the rest of Go's `staticuint64s` rule, which covers every
  one-byte value).
- **F3 -- constant boxes.** A conversion whose operand is a Go constant boxes once per site into a
  `static readonly object` field, which is what Go's read-only data gives it.

Each stage is measured by these rows: a stage is done for an entry when the entry's reading reaches its
want (0), and the entry then leaves the manifest. Reducing bytes without reaching zero is not a
retirement (the want-zero assert passes only at zero bytes).

## 3. Cost, stated before any cut

F1 and F2 add a value comparison to every interface conversion of a value type. That cost is the
first thing a stage measures (a microbenchmark A/B over the conversion, and the performance suite),
with its tolerance stated before the first reading. UNMEASURED today.

## 4. Not in this record

- `string(b)` temporaries (DESIGN-string-byte-window.md §7), non-escaping maps and buffers
  (DESIGN-nonescaping-locals.md, REC-B) and fixed-array values (DESIGN-array-value-storage.md, REC-A).
- A typed `iface == concrete` comparison that never converts: F1 and F2 already cover the one row that
  asks, so it is not planned.
