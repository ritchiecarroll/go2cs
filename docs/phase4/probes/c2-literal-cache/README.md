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

## Round 2 (2026-09-24, §8.1R and §8.1.x)

Added for COORD's review of the draft. `Caches2.cs` holds the RECOMMENDED form, measured instead of
extrapolated:
- `V8`: 2-way set-associative, 2,048 sets, FNV-1a, ≤ 16 B, no write on a hit, a miss fills way 0 if empty
  and otherwise overwrites way 1;
- `V9`: the same table with a word-at-a-time hash;
- `V10`: the module-init literal REGISTRATION table, keyed by (address, length), exact, and never evicted.

`Round2.cs` holds the rows:
- a noise-floor duplicate of the baseline;
- hits at 1, 8, 12, 16 and 32 B;
- misses and the gate's own cost;
- three contents thrashing one set;
- PerfStringMatch's `"// "u8` shape;
- a cross-thread eviction attack;
- the 8-thread stress.

```
dotnet bin/Release/net10.0/c2-literal-cache.dll --round2                                # tiering off (the csproj)
DOTNET_TieredCompilation=1 dotnet bin/Release/net10.0/c2-literal-cache.dll --round2     # tiered (env override)
dotnet publish -c Release -r linux-x64 -p:PublishAot=true -o <dir> && <dir>/c2-literal-cache --round2
dotnet bin/Release/net10.0/c2-literal-cache.dll --eviction                              # the eviction arms alone
bash gen-regbulk.sh 3805 > RegBulk.cs && dotnet build -c Release && dotnet bin/Release/net10.0/c2-literal-cache.dll --regcost
```

`RegBulk.cs` (189 KB) is generated and not committed. The csproj compiles `--regcost` only when the file
exists.

Captured output: [`output-round2-linux.txt`](output-round2-linux.txt).

**Read these corrections before its first section:**
- **The header line** of the tiered and NativeAOT captures says "JIT tiering off". That was static text in
  `Program.cs`, fixed after the runs. Each section's own `tiered=` line and its heading give the mode.
- **The tiering-off capture's eviction arms are mislabelled.** It predates the `WayOf` instrumentation, and
  its "literal in way 0" arm had in fact landed in way 1, because earlier rows had filled way 0 of its set.
  The `--eviction` section is the corrected measurement.
- **The thrash row** was labelled "per conversion". One operation is three conversions, so divide both
  `ns` and `counted` by 3. The label is fixed in the source.
- **The first `--eviction` runs** read 1.0 per 1M in way 0. That was Round2's own static constructor
  minting its two hoisted fields inside the window. The constructor now runs before the window, and the
  captured runs are after the fix.

## The RVA deduplication probe

[`../c2-rva-dedup/`](../c2-rva-dedup/Program.cs) asks the question §8.1.x's table depends on: does Roslyn
store identical u8 literal data ONCE per module, so that every `"n"u8` hands out the same address? Its
output under JIT and NativeAOT is at the end of `output-round2-linux.txt`. The printed addresses differ per
run (ASLR); the equalities are the result.

## Round 3 (2026-09-24, §8.1R2)

- **`V10` now grows.** It starts at 16 slots, doubles past a quarter full (`C2_TABLE_LOAD=2`: half), and
  publishes each grown table with one volatile store. The lookup is inlined.
- **`--stress10` (`Round3.cs`):** four writers register 50,000 keys through 13 growths while four
  readers check every hit (equal bytes, 0 counted) and every miss (equal bytes).
- **`../c2-rva-dedup/` gains a second source file.** It is recorded for Debug, Release and a ReadyToRun
  publish.

Captured output: [`output-round3-linux.txt`](output-round3-linux.txt). It also records the JIT inlining
check from the start-up probe (`../c2-startup-ab/`).

## Round 4 (revision 4, 2026-09-24): the growable table under tiered JIT

COORD's revision-3 note (fix 12) asked for a tiered run of the growable table: revision 3's "2.0-3.0×"
compared across runs, measured an inlined lookup, and ran with tiering off. `--round2` was run four times
in one session, at both load factors, with `DOTNET_TieredCompilation=0` and `=1`:

```
C2_TABLE_LOAD=<2|4> DOTNET_TieredCompilation=<0|1> dotnet bin/Release/net10.0/c2-literal-cache.dll --round2
```

Captured output: [`output-round4-tiered-linux.txt`](output-round4-tiered-linux.txt). The lookup is still
inlined, as in round 3.
