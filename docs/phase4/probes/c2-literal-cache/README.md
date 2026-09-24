# c2-literal-cache — sizing an INVISIBLE per-literal `@string` cache against arms A/B (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1 (DRAFT,
C2, 2026-09-24). Owner ruling 2026-09-23 10:10 (2)(3): arm C approved; arms A and B held while an
alternative that leaves the visible code as Go wrote it is sized.

**A record, not a gate.** It measures against the REAL golib `@string` and `AllocationCounter`
(`ProjectReference` to `src/core/golib`), built at `47e088d3d7`. The candidate caches live in
`Caches.cs` beside the harness, not in golib.

## The arms

| arm | form | what it stands for |
|:--|:--|:--|
| `V0` | `(@string)"n"u8` per evaluation | today: `new @string(span)` → a counted `CopyOf` |
| `V1` | `static readonly @string nˢ = "n"u8;` | Tier C's hoist, which arms A/B/C would extend (pre-boxed `object` for any-target sites) |
| `V2` | direct-mapped, FNV-1a content hash, one `byte[]` per slot | the golib cache the draft recommends (2-way in the recommendation) |
| `V5` | direct-mapped, keyed by the span's ADDRESS | a u8 literal's RVA address; refused in the draft because ASLR makes collisions vary per run |
| `V3` | `ConcurrentDictionary<byte[],byte[]>` with a span alternate lookup | an insert-only BCL table |
| `V4` | the UTF-16 literal's REFERENCE as the key | needs the converter to drop `u8` (a visible delta) |
| `V6` | `≤ 8 B` packed into one `ulong` key | a faster-hit attempt; slower as written (the pack through `Span<ulong>`) |
| `V7` | insert-only open addressing, capped | deterministic under program composition |

Shapes: arm A's degenerate literal to an `@string` parameter; arm B's `Printf("%s", v)`; arm C's
function-local const; log/slog's `...any` key/value pack (three one-letter keys); the miss path of a
NON-literal span through the same entry; a forced two-literal slot collision; 8 threads × 2M mixed
hits and misses with every result content-checked.

## Reading it

`ns` is best-of-7 per operation after a warm-up that also takes every literal's first use; `B` is
`GC.GetAllocatedBytesForCurrentThread` per operation; `counted` is golib's own
`AllocationCounter` per operation, which is what `testing.AllocsPerRun` reads. Run-to-run spread on
this VM, from the rows measured twice: V0 9.53–10.04 ns, V1 2.15–2.28 ns. The box was a shared
4-core cloud VM, not a quiet machine, so the ratios are what to read, not the absolute numbers.

```
dotnet build -c Release
dotnet bin/Release/net10.0/c2-literal-cache.dll            # the shapes
dotnet bin/Release/net10.0/c2-literal-cache.dll --tail     # miss path, thrash, concurrency
dotnet bin/Release/net10.0/c2-literal-cache.dll --refined  # V6/V7
```

Captured output: [`output-linux.txt`](output-linux.txt).
