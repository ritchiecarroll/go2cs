# DATA: why the converted TypeScript `checkdemo` does not build under Native AOT (B4)

Lane i9, 2026-11-14. Ordered by COORD (mailbox `20261114T135159Z-COORD.md`) as item B4 of
[`SCOUT-ts-perf.md`](SCOUT-ts-perf.md). This is a record: readings, runs and predictions as they were taken.
Nothing here lands code. The golib change it measures (lever E) is an experiment on this branch,
`claude/i9-b4-lever-e`, and is not for merging from here.

## 1. Answer

The converted `checkdemo` cannot be published as a Native AOT image today, and memory is not the reason.
Three things multiply:

1. **ILC writes every symbol's full mangled name into one string table, and that table has two hard ceilings.**
   The table is a `MemoryStream` (2 GiB), and the COFF format behind it holds offsets and sizes in 32 bits
   (4 GiB). The name mangler has no length cap. No ILC or MSBuild option shortens, hashes or omits names. The
   ELF writer shares the same table class. Upstream `main` still has both limits (§3).
2. **golib declared six tables as statics of a generic type**, so each was a new framework generic
   instantiation, with its own family of helper types, per type argument: 9,757 weak-table instantiations in a
   two-line program (§5, §7).
3. **go2cs-gen's method-set generator writes one closure class per exported method of every value type**, and
   its arms keep every method of a constructed type reachable. In the converted `tsc/internal/ast` that is
   57,542 closure classes in one assembly (§8).

Measured on a two-line program (`PerfStartup`), moving those six golib tables out of the generic types cuts the
symbol string table by 43.6%, the image by 20.9% and ILC's peak memory by 38.0%, with `GolibTests` unchanged
(§7). It does not make converted programs start faster under the JIT.

Whether `checkdemo` fits under the ceilings with that change is **not measured**. A ratio model says it is
still over on names (§6). The next measured point proposed is `parsedemo`, which carries the dominant assembly.

## 2. The failure of record

The previous i9 session's sizing run (mailbox `20261114T000303Z-i9.md`):

- Subject: microsoft/TypeScript at `ba29a56299`, module `tsc/`, converted by go2cs `651c4ad147` with the proof
  kit `claude/p4-ts-proof-kit` `56c5ec548f` (its labelled xxh3 replace, no hand-patch). The JIT build is
  correct: the parse corpus matches native Go on 17,422 of 17,422 units and three demos match byte for byte.
- `dotnet publish`, win-x64, Release, `PublishAot`, the template's `TrimMode=partial`, SDK 10.0.403, ILCompiler
  10.0.14, four logical CPUs.
- ILC ran 2 h 57 m at about 23 GB, then failed while writing the object file, with its private memory rising
  to 56.5 GB in the last three minutes:

```
System.IO.IOException: Stream was too long.
   at ILCompiler.ObjectWriter.StringTableBuilder.CreateIndex(String)
   at ILCompiler.ObjectWriter.StringTableBuilder.FlushReservedStrings()
   at ILCompiler.ObjectWriter.StringTableBuilder.GetStringOffset(String)
   at ILCompiler.ObjectWriter.CoffObjectWriter.CoffSymbol.Write(Stream, CoffObjectWriter.CoffStringTable, Boolean)
   at ILCompiler.ObjectWriter.CoffObjectWriter.EmitObjectFile(String)
```

The response file held 372 `-r` references, 0 `--root` lines and one `--defaultrooting`.

## 3. The reading: ILCompiler 10.0.14

Source: dotnet/dotnet at `0427b9abbead3fa64c6655eb4290c537397131fa`, the commit named by the ILCompiler 10.0.14
package and by `ilc.exe`'s product version. Paths are under `src/runtime/src/coreclr/tools/`. MSBuild lines are
from the package's own `build/Microsoft.NETCore.Native.targets`.

| Question | Answer | Where |
|:--|:--|:--|
| What overflowed? | The symbol string table is one `MemoryStream`. `CreateIndex` appends each name as UTF-8 plus a NUL. | `aot/ILCompiler.Compiler/Compiler/ObjectWriter/StringTableBuilder.cs:16`, `:156-168` |
| Is there a second ceiling? | Yes. A name's offset and the table's size are written as `uint32`, and every file offset in the object is a `uint` computed without an overflow check. Past 4 GiB the header would carry a wrapped offset, silently. | `CoffObjectWriter.cs:966-967`, `:1043-1054`, `:481`, `:495`, `:500`, `:505` |
| Which names go in? | Every defined and undefined symbol of every emitted node, by full mangled name. | `ObjectWriter.cs:414-419`; `CoffObjectWriter.cs:218-246`, `:552-559` |
| How long is a name? | Assembly name plus full type name; a generic instantiation spells out every argument's full name, recursively; a method adds its owner and its own instantiation. There is no cap: the SHA-256 shortening applies only to the compilation-unit prefix and to literals. | `Common/Compiler/NativeAotNameMangler.cs:169-329`, `:288-304`, `:334-355`, `:438-459`, `:92-115` |
| Is there a switch? | No option shortens, hashes or omits names, and none skips the table. `--map` and `--mstat` are opt-in dumps and were not in the failed run. | `aot/ILCompiler/ILCompilerRootCommand.cs:19-183`; targets `:248-249` |
| Can the object be split? | Only by `--multifile` (`IlcMultiModule`), which needs the framework compiled as separate objects and is not a supported publish mode. | `ILCompilerRootCommand.cs:67`; targets `:243` |
| Does Linux escape it? | No. The ELF writer's string table derives from the same class, and it names every defined symbol too. ELF64 lifts the 4 GiB offsets, not the 2 GiB stream. | `ElfObjectWriter.cs:1177`, `:744`, `:256-310` |
| Is it fixed later? | No. On dotnet/runtime `main` the builder moved to `Common/Compiler/ObjectWriter` and went UTF-8 (last change `f540517cb0`, 2026-09-02); it still holds `MemoryStream _stream`, the COFF writer still holds `uint dataOffset`, and the mangler still has no cap. | read 2026-11-14 |
| What does `TrimMode=partial` do? | It passes `--defaultrooting`. An assembly that is neither listed nor marked trimmable is then rooted whole the first time one of its types is used: every type in it. | targets `:297`; `UsageBasedMetadataManager.cs:316-353` |

**Correction made during the work.** The first posting of this reading said partial trim produced one `--root`
line per assembly. The real response file has none; the rooting is `--defaultrooting` alone, per used assembly.
The effect described was right and the mechanism was not.

## 4. Sizing runs

All on i9 (i9-13900K, 63.7 GB), one job at a time, four logical CPUs (affinity 0xF, inherited by ILC), SDK
10.0.403, ILCompiler 10.0.14. Subject: `src/tests/Performance/PerfStartup`, a two-line program, published
with `-p:PerfAot=true`. ILC memory was sampled by PID every 5 s; the peak rows are the series maximum and ILC's
own `PeakWorkingSet64`. The string table and the symbol-table offset were read from each `.obj`'s COFF header
and table directly. Every image printed what `go run` prints. WHEA-Logger read 319 before and after every run.

| Run | Tree | Publish properties | String table | Symbol-table offset | `.obj` | Image | ILC wall | ILC peak private |
|:--|:--|:--|--:|--:|--:|--:|--:|--:|
| S1 | `651c4ad147` | partial trim (the default) | 453.1 MB | 901.9 MB | 1,447 MB | 195.5 MB | 11 m 02 s | 14.80 GB |
| S2 | `651c4ad147` | `TrimMode=full` | 366.7 MB | 748.1 MB | 1,188 MB | 152.3 MB | 8 m 39 s | 12.26 GB |
| S3 | master `ffaafa144f` | `TrimMode=full` | 366.8 MB | 748.2 MB | 1,189 MB | 152.3 MB | 8 m 36 s | 11.83 GB |
| S4 | master + lever E v1 | `TrimMode=full` | 266.2 MB | 595.5 MB | 925 MB | 133.1 MB | 8 m 46 s | 8.94 GB |
| S5 | master + lever E v2 | `TrimMode=full` | 206.7 MB | 504.6 MB | 767 MB | 120.5 MB | 7 m 24 s | 7.33 GB |
| S6 | master + lever E v2 | S5 + `IlcFoldIdenticalMethodBodies=true` | 202.4 MB | 483.4 MB | 739 MB | 116.9 MB | 8 m 25 s | 7.62 GB |
| S7 | master + lever E v2 | S5 + `NativeDebugSymbols=false` | 206.7 MB | 205.6 MB | 468 MB | 120.5 MB | 7 m 14 s | 6.54 GB |

Readings:

- **Full trim removes 19% of the names, not most of them** (S1 to S2). The two response files differ by one
  line, `--defaultrooting`. The 9 MB full-trim image recorded in
  [`../PLAN-bflat-perf-exploration.md`](../PLAN-bflat-perf-exploration.md) (G-LAPTOP, .NET 9, August) does not
  hold at this master. Which change moved it was not measured; §8 names a candidate.
- **Master equals `651c4ad147`** on this subject (S2 to S3), so the baseline for lever E did not move.
- **Folding all identical bodies is not worth keeping** (S5 to S6): 2.1% of the names, for a minute more ILC.
- **`NativeDebugSymbols=false` leaves the names untouched, byte for byte, and cuts the symbol-table offset by
  59.3%** (S5 to S7). It is the lever on the 32-bit offset ceiling. It drops the native `.pdb`.

Two launches failed before S1 and are not rows: a by-path Release publish needs `-p:go2csPath`, and the link
step needs `vswhere` on `PATH`. In the second, ILC had already finished, so its object file is S1's.

## 5. What fills the table

Read on S2 (full trim, `651c4ad147`): 2,109,859 stored names, mean 174 bytes, 97.6% of the bytes in names that
hold a generic instantiation.

| Kind of symbol | Share of the bytes | Names |
|:--|--:|--:|
| method tables | 23.9% | 451,846 |
| writable data | 23.6% | 431,863 |
| generic dictionaries | 12.0% | 233,595 |
| unwind info | 10.4% | 344,270 |
| GC statics | 6.3% | 125,164 |
| interface dispatch cells | 5.5% | 73,901 |
| names that are only a method body | 4.9% | 161,869 |
| non-GC statics | 3.7% | 77,748 |

Of the 451,846 method tables, 6,080 are non-generic. 80.1% of all names mention a golib type; 2.8% mention no
converted type at all. So the volume is generic **type** instantiations and their data, which trimming unused
methods does not reduce.

The largest single family was the framework's `ConditionalWeakTable<K,V>`: 9,757 instantiations, each bringing
its `Entry`, `Container`, `CreateValueCallback`, closure and `Enumerator` types, plus `IEnumerable<T>`,
`ICollection<T>`, `IList<T>` and the rest over its `KeyValuePair<K,V>` and its entry arrays. `IEnumerable<T>`
alone had 28,707 method tables, 12,254 of them over a `KeyValuePair` and 9,758 over a weak-table entry.

## 6. Census and the ratio model

Metadata rows of the converted (non-framework) assemblies in each publish closure, read from the IL that ILC
was given (for `checkdemo`, the failed run's own response file, with the closure rebuilt):

| | PerfStartup | checkdemo | Ratio |
|:--|--:|--:|--:|
| assemblies | 66 | 196 | |
| type definitions | 7,814 | 84,626 | 10.8x |
| type specs | 12,559 | 95,195 | 7.6x |
| fields | 26,033 | 200,349 | 7.7x |
| method definitions | 60,183 | 465,882 | 7.7x |
| assembly-name length, weighted by type definitions | 11.4 | 43.0 | |

Scaling PerfStartup's tables by those ratios. This is a model with one calibration program, not a measurement
of `checkdemo`:

| checkdemo configuration | Names (2.15 GB ceiling) | Symbol-table offset (4.29 GB ceiling) |
|:--|:--|:--|
| partial trim (the failed run) | 3.4 to 4.9 GB | 6.8 to 9.8 GB |
| full trim | 2.8 to 4.0 GB | 5.7 to 8.1 GB |
| full trim + lever E v2 | 1.6 to 2.2 GB | 3.8 to 5.5 GB |
| full trim + lever E v2 + no native debug symbols | 1.6 to 2.2 GB | 1.6 to 2.2 GB |

Two cautions on the last row. The ratios do not carry name length, and `checkdemo`'s names are longer: a
mangled type name carries its assembly name twice, so with about one and a half converted-type mentions per
name the names figure is roughly 1.7 times higher, 2.6 to 3.7 GB. And most of `checkdemo`'s extra type
definitions are the closures of §8, which do not mint golib's per-type family the way a struct does, so the
ratios may overstate. The model does not put `checkdemo` under the names ceiling with margin.

The model's first row agrees with the failure (above 2 GiB). The second ceiling was not observed: the failed
run's object file was purged before its size was read.

## 7. Lever E: golib's per-instantiation tables

A static field of a generic class exists once per closed type. golib declared these as such:

| Site (master `ffaafa144f`) | Table | Per |
|:--|:--|:--|
| `ж.Views.cs:123` | `ConditionalWeakTable<ж<T>, ViewTable>` | box type |
| `ж.cs:251` | `ConditionalWeakTable<FieldRefFunc<T, TElem>, FieldRefFunc<TElem>>` | box type and field type |
| `ж.ElemRefBox.cs:160` | `ConditionalWeakTable<T[], ElemRefBox<T>?[]?[]>` | element type |
| `ж.NilArrayBox.cs:133` | `ConcurrentDictionary<GoArrayDimsKey, ж<T>>` | box type |
| `ж.FieldRefBox.cs:386` | `ConditionalWeakTable<Delegate, GoOffset>` | field type |
| `ж.FieldRefBox.cs:546` | `ConditionalWeakTable<Delegate, Displacement>` | field type |

The last two read as non-generic. Their value types are records nested in `FieldRefBox<T>`, so each is one
more weak table, and one more record type with every compiler-generated record member, per field type. Nothing
in either record depends on `T`.

The experiment (`GoBoxTables.cs`) holds one non-generic table for each. Weak-table keys compare by reference,
so one table keyed on the base type holds every type's entries. v1 (`21995337d9`) moved the first four; v2
(`f6633c0e8f`) moved the last two and their records.

| | S3 unpatched | S4 v1 | S5 v2 |
|:--|--:|--:|--:|
| symbol string table | 366.8 MB | 266.2 MB (-27.4%) | 206.7 MB (-43.6%) |
| method tables | 451,846 (read on S2) | 308,994 | 228,640 |
| weak-table instantiations | 9,757 (read on S2) | 4,510 | 46 |
| image | 152.3 MB | 133.1 MB (-12.6%) | 120.5 MB (-20.9%) |
| ILC wall | 8 m 36 s | 8 m 46 s (+1.9%) | 7 m 24 s (-14.0%) |
| ILC peak private | 11.83 GB | 8.94 GB (-24.4%) | 7.33 GB (-38.0%) |
| `GolibTests`, Release, windows: passed / failed | 2,624 / 0 | 2,623 / 1 | 2,624 / 0 |

- **`GolibTests`.** v1's one failure is `NoUncountedBackingAllocations`, a source-text guard naming the
  `ElemRefBox` line the patch edits. v2 updates that allow-list entry, and every test then has the same verdict
  as the unpatched tree.
- **JIT start-up.** A published hello (the template's default publish, ReadyToRun), unpatched against v2, 40
  interleaved pairs, twice: 198.7 to 197.2 ms and 199.5 to 196.5 ms, with A/A spreads of 0.2 and 1.8 ms. About
  2 ms, 1%. Lever E is an AOT and size lever, not a start-up one: under the JIT a generic type's statics exist
  only for the types a run touches. These are internal timings from this box.
- **Known gap in the experiment.** With one table for every element type, a backing array reached under a
  second element type would find the first type's chunks; the patch gives it a fresh box instead of a
  persistent one. A landing version needs a real answer there.
- **Not for landing from this branch.** The same files are being changed by the interior-pointer seat. A
  landing version is re-cut on top of it and routed by COORD.

What remains large after v2, by method-table count on S5: `IEquatable<T>` 10,418, `Func<,>` 8,998, the three
`EqualityComparer<T>` types about 6,000 each, `FieldRefFunc<,>` 5,202, `ж<T>.FieldRefWrappers<TElem>`'s two
closure types 5,160 each, `GoMethodText`'s closures about 4,000 each. No single family dominates any more.

## 8. The third multiplier: method-set closures

`go2cs-gen`'s `MethodSet.cs` (`d54f156b81`, 2026-10-19) writes `IGoMethodSet.ᴛMethod` on the generated part of
every value type with exported Go methods, so reflect's method paths hand out compiled delegates under Native
AOT. Each exported method gets an unbound arm and a bound arm. The bound arm declares a case-local receiver and
returns a lambda that captures it, so the C# compiler mints one closure class per arm.

In the converted `tsc/internal/ast`:

- 59,638 type definitions, of which 226 are top-level and 57,542 are closure classes
  (`<>c__DisplayClassN_M`), in 243 declaring types, 237 each on average. The ordinary node types read 266 or
  267, because every node type promotes the same couple of hundred methods from its embedded bases.
- 242 generated `MethodSet.g.cs` files, 57,519 arms, 58.7 MB of generated source. The assembly is 37.4 MB and
  holds 256,277 of `checkdemo`'s 465,882 converted method definitions.

Two further readings, from the full-trim object S2:

- The arms are compiled under full trim too: 2,603 method bodies there name `IGoMethodSet`, and
  `AdapterBinder.VariadicBinder` and `GoReflect.GeneratedMethodValue` dispatch on the slot.
- Each arm calls its method. So once one type's `ᴛMethod` is reachable, every exported method of that type is.
  This is a candidate for why full trim stopped trimming (§4). It has **not** been isolated by an A/B.

The generator's own header records the cost as accepted on a fmt-only program (+2.59 MB) and says a partial
trim keeps every arm. A non-capturing form would remove the closure class per arm. That is generator work,
unmeasured here, and routed to G by COORD.

## 9. Predictions, scored as worded

| | Prediction | Measured | Score |
|:--|:--|:--|:--|
| P1 | PerfStartup partial: string table 80 to 400 MB | 453.1 MB | MISSED, high |
| P1 | image 250 to 350 MB | 195.5 MB | MISSED, low |
| P1 | ILC wall 5 to 25 min | 11 m 02 s | MET |
| P2 | over 60% of the table's bytes are generic names | 97.5% | MET |
| P3 | PerfStartup full trim: string table under 15 MB | 366.7 MB | MISSED, 24x |
| P4 | the scaled estimate for checkdemo partial is above 2 GiB | 3.4 to 4.9 GB | MET |
| E1 | lever E: string table falls 30 to 50% | v1 27.4%; v2 43.6% | v1 MISSED; v2 MET |
| E2 | image falls 15 to 35% | v1 12.6%; v2 20.9% | v1 MISSED; v2 MET |
| E2 | ILC wall falls 15 to 35% | v1 +1.9%; v2 14.0% | MISSED both, v2 by a point |
| E2 | ILC peak private falls 10 to 30% | v1 24.4%; v2 38.0% | v1 MET; v2 MISSED, on the good side |
| E3 | weak-table instantiations under 50 | v1 4,510; v2 46 | v1 MISSED; v2 MET |
| E4 | JIT start-up 5 to 25 ms faster | about 2 ms | MISSED |
| E5 | `GolibTests`: the same verdict for every test | v1 one text guard differs; v2 identical | v1 MISSED; v2 MET |
| S6 | fold all: another 8 to 15% off the table | 2.1% | MISSED |
| S7 | no debug symbols: table unchanged within 1% | identical | MET |
| S7 | symbol-table offset falls by more than 40% | 59.3% | MET |

The E predictions were posted when only v1 existed; v2 was built after v1's reading showed the two missed
tables. S7 was posted as S6 plus the property and run as S5 plus the property, one axis.

## 10. Route

Ranked by what was measured, cheapest first:

1. **Publish with `TrimMode=full` and `NativeDebugSymbols=false`.** No code change. Full trim is worth 19% of
   the names on its own; the second removes the 32-bit offset ceiling from the picture. Full trim carries the
   correctness risk the template's comment names (members reached by reflection), so the gate is byte-identical
   output against native Go.
2. **Land a version of lever E** after the interior-pointer seat: 43.6% of the names, 38.0% of ILC's memory and
   20.9% of the image on a two-line program, with `GolibTests` unchanged. It helps every Native AOT publish of
   converted code, not only this one.
3. **A non-capturing method-set arm** in `go2cs-gen` (§8), measured by its owner.
4. **Report the two ceilings upstream** (§11). Whatever go2cs does, a large enough program meets them, and the
   second fails silently.
5. Not worth pursuing: folding all method bodies (2.1%); building on Linux (same string table);
   `--multifile` (unsupported).

Open: whether 1 and 2 together bring a TypeScript demo under the names ceiling. The proposed next reading is a
`parsedemo` publish with full trim, lever E v2 and no native debug symbols. `parsedemo` carries `tsc/internal/ast`
and is the program the hello and parse-corpus comparisons run.

## 11. Upstream issue, draft

Filing is the owner's decision. Draft text:

> **Title:** NativeAOT object writer: COFF file offsets overflow `uint` without a check, and the symbol string
> table is limited to 2 GiB (`MemoryStream`)
>
> **Version:** ILCompiler 10.0.14 (dotnet/dotnet `0427b9abbead`), win-x64. Both limits are still present on
> `main` as of 2026-11-14.
>
> **The silent one first.** `CoffObjectWriter.EmitObjectFile` accumulates section and relocation sizes into a
> `uint dataOffset` and stores it as `PointerToRawData`, `PointerToRelocations` and `PointerToSymbolTable`
> (`CoffObjectWriter.cs:481-505`) with no overflow check. An object whose section data and relocations exceed
> 4 GiB would be written with wrapped offsets and no error. We have not produced such an object: the failure
> below stops the write first. The string-table size field and the per-symbol name offsets are `uint32` too
> (`:966-967`, `:1043-1054`).
>
> **The one that reports.** Publishing a large application with `PublishAot` fails at the end of compilation,
> after about three hours here:
>
> ```
> System.IO.IOException: Stream was too long.
>    at ILCompiler.ObjectWriter.StringTableBuilder.CreateIndex(String)
>    at ILCompiler.ObjectWriter.StringTableBuilder.FlushReservedStrings()
>    at ILCompiler.ObjectWriter.StringTableBuilder.GetStringOffset(String)
>    at ILCompiler.ObjectWriter.CoffObjectWriter.CoffSymbol.Write(Stream, CoffObjectWriter.CoffStringTable, Boolean)
>    at ILCompiler.ObjectWriter.CoffObjectWriter.EmitObjectFile(String)
> ```
>
> **Its cause.** `StringTableBuilder` holds the table in a `MemoryStream` (`StringTableBuilder.cs:16`), which
> cannot exceed `int.MaxValue` bytes. Every symbol's full mangled name is reserved
> (`CoffObjectWriter.cs:552-559`), and `NativeAotNameMangler` expands generic instantiations recursively with
> no length cap (`NativeAotNameMangler.cs:288-304`), so an application with many generic instantiations over
> types with long namespace-qualified names reaches 2 GiB of names well before any other limit. The ELF
> writer uses the same class. Replacing the stream alone moves this limit to the 4 GiB of the `uint32` fields
> above, not away.
>
> **Scale, for reference.** A two-line program in the same code base produces a 1.45 GB object with 3.87 million
> symbols and a 453 MB string table, 97.5% of it names containing a generic instantiation, mean length 170 bytes.
>
> **Suggestions.** (a) Fail early with a diagnostic that names the limit, instead of an `IOException` after the
> whole compile, and check the `uint` offset arithmetic. (b) Cap or hash mangled names past some length, as the
> compilation-unit prefix already is. (c) Consider not emitting names for symbols that are never referenced
> from outside the object in single-file compilation.

## 12. Evidence

On this branch: commits `21995337d9` (lever E v1) and `f6633c0e8f` (v2). The run records, ILC sample series,
object-file readings, census tables and the scripts that produced them (`i9b-aot-run.ps1`, `i9b_coff_strtab.py`,
`i9b_strtab_kinds.py`, `i9b_strtab_mtheads.py`, `i9b-il-census.ps1`, `i9b_startup_pair.py`,
`i9b-golibtests.ps1`) are in the lane's work folder on i9 and are named in the mailbox posts of 2026-11-14
(`20261114T143608Z`, `144002Z`, `150655Z`, `152350Z`, `163146Z`, `165002Z`, all `-i9.md`).

## 13. Amendment, 2026-11-14 18:20Z: `parsedemo` builds and matches native Go

Run P1, approved by COORD (mailbox `20261114T165219Z-COORD.md`), predictions posted first
(`20261114T170500Z-i9.md`), result posted at `20261114T174652Z-i9.md`.

- **Subject:** the converted `parsedemo`, on the conversion's own commit `651c4ad147` with the golib half of
  lever E v2 applied (local branch `claude/i9-b4-lever-e-651c`, `b1769f6df1`). No converter change, no
  regenerated code.
- **Publish:** win-x64, Release, `PublishAot` for the executable, `TrimMode=full`, `NativeDebugSymbols=false`.
  Four logical CPUs.

| | Measured | Predicted |
|:--|--:|:--|
| result | exit code 0 | a pass, called at about 60% |
| symbol string table | 655.1 MB, 30.5% of the stream's limit | 0.8 to 2.6 GB: MISSED, low |
| symbol-table offset | 514.8 MB | 1.2 to 1.9 GB: MISSED, low |
| `.obj` | 1,314 MB, 5,864,759 symbols | |
| ILC wall | 36 m 49 s | 45 to 150 min: MISSED, low |
| ILC peak private / `PeakWorkingSet64` | 17.60 / 17.06 GB | 15 to 35 GB: MET |
| image | 297.4 MB | 0.5 to 1.1 GB: MISSED, low |
| WHEA | 319 before and after | |

Correctness, by the proof kit's `compare.py` against the native Go build of the unmodified module:

- the demo (`good.ts bad.ts`) is byte-identical, 8,104 bytes;
- the parse corpus matches on 17,422 of 17,422 units, with no crash on either side;
- as a control, a different input gives a different hash, the same on both sides.

The image is kept with its SHA-256,
`8421c3a363ac3e321c4cd00d42a1a36fb3531c4fa41a5995a0642791b4123863` (297,359,360 bytes).

**What this changes above.** §6's ratio model overestimated by about three times; PerfStartup is a poor
calibration for a closure whose types are mostly closure classes. `parsedemo` is the calibration of the same
shape: `checkdemo`'s closure is 1.17 to 1.33 times `parsedemo`'s on the four counts, which puts `checkdemo`
with the same three levers at 0.75 to 1.0 GB of names and 0.6 to 0.8 GB of offset, under both ceilings. That
build is asked of COORD and not yet run. §1's "not measured" stands for `checkdemo` until it is.

### A fourth multiplier: golib's `array<T>` generic cycle is back

ILC reports `IL3054` ("generic expansion ... aborted due to generic recursion") in every run of this record:
1,292 lines in S1, 886 in S2 and S3 (unpatched master), 882 with lever E, 884 in P1. The expansions are
`array<array<array<array<array<T>>>>>` chains through `slice<T>`, the pointer box and its box kinds.

Names holding array-in-array nesting are 25.4% of P1's string table and 32.5% of S7's, spread evenly over
depths 2, 3 and 4. That is the shape of an expansion run to ILC's cut-off, not of types a Go program declares.

`2b6fe9a34d` (2026-10-14) broke this cycle once and recorded its three edges; the growth edge,
`array<T>.AliasPointer` returning a pointer to `array<T>`, was left in place because the converter emits calls
to it. That commit measured 0 cut-off lines afterwards. The cycle is closed again at master. A candidate for
the new edge, by reading and by date only, is `8a714cf6dd` (2026-10-22), which added
`ElemRefBox<T>.OfArrayElement(array<T>, int)`; it has not been pinned with ILC's scan graph, which is how the
first fix found its edges. No standing gate counts cut-off lines, so the return went unseen.

Breaking it again is a golib lever of the same order as lever E, and unlike E it also removes work ILC does
only to throw it away.
