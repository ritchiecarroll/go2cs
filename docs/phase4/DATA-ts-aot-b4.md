# DATA: why the converted TypeScript `checkdemo` does not build under Native AOT (B4)

Lane i9, 2026-11-14. Ordered by COORD (mailbox `20261114T135159Z-COORD.md`) as item B4 of
[`SCOUT-ts-perf.md`](SCOUT-ts-perf.md). This is a record: readings, runs and predictions as they were taken.
Nothing here lands code. The golib change it measures (lever E) is an experiment on the branch
`claude/i9-b4-lever-e`, and is not for merging from there. Sections 13 and 14 are dated amendments and carry the
results: `parsedemo` and `checkdemo` both build under Native AOT and match native Go.

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
  interleaved pairs, twice: no effect beyond the edge of the instrument's resolution. Lever E is an AOT and
  size lever, not a start-up one: under the JIT a generic type's statics exist only for the types a run
  touches. The readings are internal to the fleet and are in the mailbox post, not here.
- **Known gap in the experiment.** With one table for every element type, a backing array reached under a
  second element type would find the first type's chunks; the patch gives it a fresh box instead of a
  persistent one. A landing version needs a real answer there.
- **Not for landing from that branch.** The same files are being changed by the interior-pointer seat. A
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
| E4 | JIT start-up 5 to 25 ms faster | no effect beyond the instrument's resolution | MISSED |
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

On `claude/i9-b4-lever-e`: commits `21995337d9` (lever E v1) and `f6633c0e8f` (v2). This record began on that
branch and moved to `claude/i9-ts-aot-b4`, cut from master `c2919b8b16`, so that it does not ride an experiment's tree. The run records, ILC sample series,
object-file readings, census tables and the scripts that produced them (`i9b-aot-run.ps1`, `i9b_coff_strtab.py`,
`i9b_strtab_kinds.py`, `i9b_strtab_mtheads.py`, `i9b-il-census.ps1`, `i9b_startup_pair.py`,
`i9b-golibtests.ps1`) are in the lane's work folder on i9 and are named in the mailbox posts of 2026-11-14
(`20261114T143608Z`, `144002Z`, `150655Z`, `152350Z`, `163146Z`, `165002Z`, all `-i9.md`).

## 13. Amendment, 2026-11-14 18:14Z: `parsedemo` builds and matches native Go

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

## 14. Amendment, 2026-11-14 22:45Z: `checkdemo` builds and matches native Go

Run P2, on COORD's go-ahead (mailbox `20261114T195216Z-COORD.md`), with predictions C1 to C6 posted before it
(`20261114T174652Z-i9.md`); result at `20261114T223108Z-i9.md`. This is B4's stated goal.

- **Subject and publish:** the converted `checkdemo`, as in §13: `651c4ad147` with lever E v2's golib,
  `TrimMode=full`, `NativeDebugSymbols=false`, four logical CPUs.

| | Measured | Predicted |
|:--|--:|:--|
| result | exit code 0 | a pass |
| symbol string table | 1,545.2 MB, 72.0% of the stream's limit | 0.75 to 1.0 GB: MISSED, high |
| symbol-table offset | 982.9 MB, 22.9% of the 32-bit limit | 0.6 to 0.8 GB: MISSED, high |
| `.obj` | 2,795 MB, 10,913,518 symbols | |
| ILC wall | 2 h 21 m 40 s | 40 to 90 min: MISSED, long |
| ILC peak private / `PeakWorkingSet64` | 33.36 / 32.23 GB | 20 to 30 GB: MISSED, high |
| image | 577.0 MB | 350 to 450 MB: MISSED, high |
| WHEA | no event stamped during the run | |

Correctness against the native Go build of the unmodified module, by the proof kit's own tools:

- `demo_compare.py`, `checkdemo check.ts good.ts`: byte-identical, 318 bytes;
- `typecheck_slice.py --every 74` (101 compiler test units, every diagnostic): 101 of 101 match, no crash;
- as a control, a different input gives a different hash, the same on both sides.

The image is kept with its SHA-256,
`70417787592c30871ffbbf11e681b135a3c8b0cf528c22a5f737164d1971b51c` (576,976,384 bytes).

**Why the closure ratio failed as a predictor.** Five of six predictions missed, all high. The IL census of §6
put `checkdemo` at 1.17 to 1.33 times `parsedemo`. Measured, it is 2.36 times on names, 1.91 on the
symbol-table offset, 1.94 on the image and 1.86 on symbols. What the checker adds is generic instantiation:
its code names far more closed generic types per definition than the AST does. A count of type and method
definitions cannot see instantiations, and neither can a count of type specs, which are per assembly and not
per closed type in the image. The only calibration that held was a measured build of the same shape, and even
that under-read by a factor of two. `emitdemo` and `mfdemo` have `checkdemo`'s IL closure to within forty
methods, so the census says nothing about them at all.

**A stop line that did not bind.** COORD had set a stop at 30 GB of ILC private memory. ILC sat near 14.7 GB
for two hours and rose to 33.4 GB in the last two and a half minutes, while writing the object file. The
lane was polling for the build's exit every 30 seconds, so the build had ended before the crossing was read.
Nothing failed. Since then the run script's sampler enforces the line itself: it kills ILC by process id when
private bytes cross it and the run exits non-zero. Controlled once, with the line set at 3 GB under a known
6.5 GB peak: killed at 3.07 GB, no ILC process left.

**The plateau.** ILC ran on one core at flat memory for 124 of its 142 minutes, against 26 of 37 for
`parsedemo`: 4.8 times longer for a closure a third larger. Both logs carry the `IL3054` lines of §13. That the
plateau is the generic cycle's cost is a lead, taken from `2b6fe9a34d`'s own record, and not yet a finding.

**On the event-log reading.** The WHEA count read 317 at this run's start, where every earlier run read 319.
The System log is circular; its two oldest WHEA events had aged out. A count can therefore fall, and a count
delta can hide a new event. From this run on the record reads the events stamped inside the run.

**Where B4 stands.** A working Native AOT image of the converted checker exists and matches native Go, on a
tree that differs from the conversion's own commit only by lever E v2's golib. It needed all three levers of
§10. The names sit at 72% of the limit, so the route is real but not comfortable: the returned generic cycle
(§13) is measured at a quarter of the names, and it is the next lever to be read.

## 15. Amendment, 2026-11-15 00:00Z: the returned generic cycle is pinned and cut (lever G)

Ordered by COORD (mailbox `20261114T223231Z-COORD.md`) ahead of the two remaining demos. Predictions G1 to G4
were posted first (`20261114T223427Z-i9.md`).

**It reproduces on a tiny program.** A golib-only consumer (an `array<nint>` written and read by index), Native
AOT, `TrimMode=partial`, one `IL3054` line per site:

| golib tree | `IL3054` lines | Image | Publish |
|:--|--:|--:|--:|
| `2b6fe9a34d`, the first fix's own commit | 0 | 7,292,928 B | 11 s |
| master `c2919b8b16` | 654 | 19,697,664 B | 33 s |
| master + lever G | 0 | 11,214,848 B | 19 s |

**The commit that closed it again.** `git bisect` over `src/core/golib` and `src/core/go2cs`, nine steps, none
skipped, each reading 0 or between 573 and 589 lines: the first bad commit is `4ad8e8df66` (2026-10-21),
"field-view cache matches accessors without Delegate.Equals; element boxes get the view slot". The lane had
named `8a714cf6dd`, a day later; that was wrong.

**The edge.** That commit added `this is ElemRefBox<T>` to `ж<T>`'s view lookup, in `viewsHead` and in the
inlined hit path of `viewOf` (`ж.Views.cs:135` and `:175` at master). One mention is enough. Every `ж<T>` then
constructs `ElemRefBox<T>`, which names `array<T>`, whose `AliasPointer` names `ж<array<T>>`, and the loop
repeats one nesting level deeper until ILC's cut-off. `AliasPointer` is the growth edge `2b6fe9a34d` left in
place on purpose, because the converter emits calls to it.

**Lever G.** The two tests ask a non-generic interface, `IElementViewSlot`, which the element box implements.
The interface is `internal`; nothing public moves. Branch `claude/i9-b4-cycle`, `5d00eb8b91`: master plus this
one commit. COORD has ruled it a landing candidate for the release in progress, to be re-cut as a seat with a
guard; that seat is not this record.

**Measured, each arm against a tree that differs by lever G alone:**

| | PerfStartup: S7 to S8 | `parsedemo`: P1 to P3 |
|:--|:--|:--|
| base tree | master `ffaafa144f` + lever E v2 | `651c4ad147` + lever E v2 |
| `IL3054` lines | 882 to 0 | 884 to 0 |
| symbol string table | 206.7 to 115.7 MB (-44.0%) | 655.1 to 429.6 MB (-34.4%) |
| symbols | 2,311,421 to 1,617,603 | 5,864,759 to 4,281,792 |
| image | 120.5 to 87.6 MB (-27.3%) | 297.4 to 219.5 MB (-26.2%) |
| ILC wall | 7 m 14 s to 6 m 29 s (-10.4%) | 36 m 49 s to 31 m 58 s (-13.2%) |
| ILC peak private | 6.54 to 3.84 GB (-41.3%) | 17.60 to 11.66 GB (-33.8%) |
| single-core plateau | not read | 26 to 27.5 min |

Both images print what native Go prints: PerfStartup its checksum line, `parsedemo` the demo and 17,422 of
17,422 corpus units. `GolibTests` whole (Release, windows) on the S8 tree: 2,624 passed, 0 failed, every test's
verdict equal to the unpatched tree's.

Against unpatched master, levers E v2 and G together take PerfStartup's names from 366.8 to 115.7 MB (-68.4%),
its image from 152.3 to 87.6 MB and ILC's peak from 11.83 to 3.84 GB.

**Scored.** G1 (reproduces at master, 0 at the first fix): MET. G2 (the bisect lands on `8a714cf6dd`): MISSED.
G3: consumer to 0 lines, MET; PerfStartup names fall 20 to 35%, MISSED on the good side (44.0%), wall falls 10
to 30%, MET (10.4%); `parsedemo` names fall 15 to 30%, MISSED on the good side (34.4%); **`parsedemo`'s
single-core plateau falls by more than half, MISSED: it did not move.** G4 (`GolibTests` verdicts equal): MET.

**The lead that is now refuted.** Sections 13 and 14 carried, as a lead, that ILC's long single-core phase at
flat memory is the generic cycle's cost. It is not. With the cycle gone the plateau is unchanged; what shrank
is the phase before it and the memory. The plateau was 86% of this `parsedemo` build's ILC time and 124 minutes
of `checkdemo`'s. What ILC does there is not identified, and it is the lever on build time that nobody holds.

**The stop line was controlled before it guarded anything.** With the line at 3 GB under a known 6.5 GB peak,
the sampler logged 3.07 GB, killed ILC by process id, and the run exited non-zero with no ILC process left.

**No guard counts cut-off lines.** That is how the cycle returned unseen a week after it was fixed. The
consumer above is such a guard in under a minute; a cheaper one that needs no publish is owed by the seat.

## 16. Amendment, 2026-11-15 04:00Z: lever G is seated, in a different form from the one section 15 measured

**The form changed.** Section 15's lever G asked an internal interface, `IElementViewSlot`. Measured on
`PerfFieldView` under the JIT before landing, that form is a real regression on the row: the hit path of the
field-view lookup is inlined at every `&p.f` operand, and an interface test plus an interface call per operand
is not an exact type test. It was dropped. The seat carries `ElementViews`, an internal **virtual** on `ж<T>`
that answers a null reference and that the element box overrides with its own slot. It sits with `ArrayRef`,
`TryGetElementStorage` and `TryPinnedReinterpret` in `ж.cs`, which is the same arrangement: the base names
nothing, the one kind that has the thing overrides. No method of `ж<T>` names `ElemRefBox<T>` in either form.

**Its cost, recorded as a cost: +2.7% on `PerfFieldView` under the JIT, one virtual call per element-box
field-view lookup** (the slotted and field-view kinds are still found by their exact type tests first). That
reading is outside its own A/A band and was taken on a loaded box; the timing of record is the Performance
re-baseline's. COORD ruled the price paid: 0 `IL3054` lines and the image and memory reductions of section 15
for every Native AOT consumer.

**The seat.** `claude/i9-golib-elemview-cycle`, `c08c2bba8c` on master `091935257c`, seated in batch 308. Three
golib files (+26 -4), no converter or generator change, nothing public, 0 B of instance state.

**The guard needs no publish.** `GolibTests` `PointerBoxGenericCycleTests` reads the IL of every method of
`ж<T>` and of the types nested in it, and fails if one names `ElemRefBox<T>` over the type's OWN parameter. That
qualifier is the edge: `at<Telem>` names `ElemRefBox<Telem>` over a METHOD's type parameter by design, is
compiled only for the element types a program takes addresses of, and leads from `ж<array<Telem>>` back to
`ж<array<Telem>>`, no deeper. A first cut of the guard without the qualifier would have stayed red after the
fix. Two controls hold the reader: on a generic type it must report the type test, and it must see
`ElemRefBox<Telem>` without calling it the edge.

| leg | reading |
|:--|:--|
| red first, guard only (`a23a6e6182`) | 3 executed; the guard fails naming exactly `viewsHead` and `viewOf`; both controls pass |
| guard, fix (`d22b5edaab`) and merge result (`c08c2bba8c`) | 3 of 3 |
| `GolibTests` whole, master `d7f5a59f79` | 2,723 total, 0 failed |
| `GolibTests` whole, `d22b5edaab` | 2,726 total, 0 failed; by name master's verdicts plus the 3 new tests, and one scheduling-dependent test (below) |
| `GolibTests` whole, `c08c2bba8c` | 2,757 total, 0 failed; the 31 further names are all batch 307's new tests |
| golib-only Native AOT consumer, `IL3054` lines | 654 at master `d7f5a59f79` (19.70 MB), 0 at `c08c2bba8c` (11.20 MB) |
| `go2cs.slnx`, Debug, at `d22b5edaab` | 0 errors |

**A finding beside the seat.** `RuntimeStopTheWorldContractTests.AHolderIsItsGoroutineNotTheThreadItRanOn` ends
in `Assert.Inconclusive` ("goroutine B never landed on goroutine A's reused thread; nothing to assert") on most
runs: alone, it passed 2 of 5 on the seat build and 1 of 5 on the master build. It moved between passed and
not executed across the whole-suite runs above for that reason and no other. Routed by COORD as a BOARD row.

**Scored.** Red leg names the two lookups: MET. Whole suite total 2,726: MET. The interface form inside the
noise on `PerfFieldView`: **MISSED**, a regression. The virtual form under +3% and clearly cheaper than the
interface: MET, MET. Verdicts by name equal to master's plus three: MISSED by the one name above. Consumer 654
and 0: MET, MET.

## 17. Amendment, 2026-11-15 06:00Z: the converted demos on Windows, under the JIT, against native Go

Not a Native AOT result, but the same closure on the same box, and it is what the AOT images are compared
against. P4's `ts-proof-kit` gate script (`run-gate.ps1`, `b3dbdd3ef8`) at three trees, each a fresh worktree
and a fresh conversion, TypeScript `ba29a56299`, go1.26.11, SDK 10.0.403, `GoTargetOS=windows`.

**At master the Windows build of the TypeScript closure has been red since batch 302.** Two packages that only
Windows compiles, so no linux gate could see it:

| tree | build | wall |
|:--|:--|:--|
| master `c2919b8b16` | failed | `golang.org/x/sys/windows`: `CS0246`, the `[GoInlineLayout]` stamp emitted unqualified in a file with no `using go.golib` |
| `76bf490d38` (master + C2's two fixes) | failed | `x/sys/windows` builds; `github.com/Microsoft/go-winio/pkg/guid`, 17 errors: `type GUID windows.GUID` is a named type in one module over an inline-layout struct of another, and its consumer still emits a value clone (`ΔClone`) and a range slice of the `array8<byte>` field |
| `651c4ad147` (the last master whose Windows closure builds) | built | none |

The inline-array admission of a type did not reach a named type declared over it in another package. Routed to
C2 as release-blocking.

**At `651c4ad147` every comparison matches native Go on Windows:**

| comparison | result |
|:--|:--|
| `parsedemo` demo | match |
| parse corpus | 17,422 of 17,422 |
| `checkdemo`, `emitdemo`, `mfdemo` demos | match, match, match |
| full emit, every single-file compiler case | 7,404 of 7,404 |
| full type-check, the same cases | 7,404 of 7,404 |

None of the three Windows walls the kit's author predicted (path spelling, the `newLine` default, the
`os`/`filepath`/`syscall` closure) appears. This is the reading at the last buildable master, not the gate of
record: that is one rerun at the tree that carries the `go-winio` fix.

**The script exits 1, from its census step only.** 1,267 hits over the staged tree: 1,266 are a deny list derived
on this box from the account and the git identity matching the repository's own public URL, which the
converter writes into every emitted project file (0 hits outside that URL); 1 is the `local` mode's `go2csPath`
fallback, an absolute path, in the staged `Directory.Build.props`. Neither is a leak; both are for the kit.

**Scored.** Converts and builds at `76bf490d38` (85%): MISSED, the second wall. At `651c4ad147`: converts and
builds (90%), the four small comparisons match (85%), full emit all match (60%), full check all match (65%):
all MET.

## 18. Amendment, 2026-11-15 07:30Z: lever G alone is enough, and ILC's long phase is method body folding

**Lever G alone fits.** `parsedemo`, `651c4ad147`, full trim, no native debug symbols, one build per arm, each
image matching native Go on the demo and 17,422 of 17,422 corpus units:

| | P1: E v2 alone | P4: G alone | P3: E v2 + G |
|:--|:--|:--|:--|
| symbol string table | 655.1 MB | 616.2 MB | 429.6 MB |
| image | 297.4 MB | 252.3 MB | 219.5 MB |
| ILC peak private | 17.60 GB | 15.69 GB | 11.66 GB |
| `IL3054` lines | 884 | 0 | 0 |

G alone is smaller than E v2 alone, and E v2 on top of G takes a further 30% off the names, not the 44% it took
at PerfStartup without G. COORD ruled on it: the release in progress ships lever G only (section 16), and lever
E is an option for the release after, with its own seat (the BOARD row of 2026-11-15). P4 is built with the
form of G that landed, the virtual; P3 was the interface form.

**The long single-core phase is located and then identified.** Sections 13 to 15 carried ILC's long phase at
flat memory as unidentified. ILC's own `--verbose` lines (`IlcVerboseLogging`), laid against the per-process
sampler on one wall clock, on P4:

| ILC's line | from start | cores | private |
|:--|:--|:--|:--|
| `Scanning N methods...` (38 rounds) | 0 | 1.5 to 2.3 | to 5.8 GB |
| `Compiling 654280 methods...` (16 rounds) | 2 m 33 s | up to 2.7 | to 7.9 GB |
| `Writing 6322655 object nodes...` | 4 m 50 s | 1.0 | flat, 7.8 to 8.1 GB |
| `10%...` through `100%...`, all inside 25 s | 31 m 40 s | 1 | rising to 15.69 GB |
| `Done writing object file in 00:27:35` | 32 m 30 s | | |

The phase is not spread over the object nodes: it is everything between the writer's first line and the first
tenth of them, 26 m 50 s of a 32 m 20 s run. One instrument note: the package's `Exec` task logs ILC's output at
low importance, so the verbose lines reach only a detailed logger; a file logger carries them, and they have
no timestamps of their own, so the log was clocked every 5 s beside the sampler.

**It is method body folding.** The SDK passes `--methodbodyfolding:generic` at `-O` by default
(`Microsoft.NETCore.Native.targets`, the `_IlcMethodBodyFoldingValue` group). One property turns it off,
`IlcFoldIdenticalMethodBodies=false`, and the rsp then reads `--methodbodyfolding:none`. Each row is one build
that differs from its neighbour by that property alone:

| | PerfStartup (E v2 + G): on | off | `parsedemo` (G alone): on | off |
|:--|:--|:--|:--|:--|
| ILC wall | 6 m 29 s | 1 m 27 s | 32 m 20 s | 7 m 06 s |
| object write, as ILC logs it | about 4.7 min | 15.7 s | 27 m 35 s | 2 m 20 s |
| symbols | 1,617,603 | 2,111,528 | 5,098,182 | 6,105,032 |
| symbol string table | 115.7 MB | 131.8 MB (+13.9%) | 616.2 MB | 657.3 MB (+6.7%) |
| image | 87.6 MB | 101.0 MB (+15.3%) | 252.3 MB | 280.1 MB (+11.0%) |
| ILC peak private | 3.84 GB | 4.35 GB (+13%) | 15.69 GB | 17.66 GB (+12.6%) |
| output | same as native Go | same | same | same |

So on these closures folding is three quarters to five sixths of ILC's time, it is the SDK default, and every
Native AOT publish of a converted program pays it. Turning it off is a build-time lever a user holds: a build
four and a half times faster for an image 11 to 15% larger. Its cost on the limit of section 3 is the symbol
table, which grows 7 to 14%. That ILC computes the folding lazily, on the writer's first de-duplication call,
is an inference from `ObjectWriter.cs` and from where the phase sits; it is not read from the interner.

**`emitdemo` and `mfdemo`, built with lever G alone and without folding:**

| | `emitdemo` | `mfdemo` |
|:--|:--|:--|
| symbol string table | 1,248.9 MB (58.2% of 2 GiB) | 1,249.0 MB (58.2%) |
| symbols | 10,031,055 | 10,032,887 |
| object file | 2,344.1 MB | 2,344.4 MB |
| image | 477.1 MB | 477.2 MB |
| ILC wall | 12 m 59 s | 12 m 54 s |
| ILC peak private | 30.09 GB | 29.93 GB |
| `IL3054` lines | 0 | 0 |
| output against native Go | demo matches; emit slice 102 of 102 | demo matches |

A table that fits without folding fits with it, since folding only removes symbols. With these two, every one
of the four converted TypeScript demos has a Native AOT image on Windows that prints what native Go prints.

**Scored.** `parsedemo` with G alone: builds and matches, MET; names 720 MB in 600 to 800, MET (616.2); peak 19
GB in 15 to 23, MET (15.69); the plateau still there, MET; **which phase it is (a folding or ordering pass
after compilation 50%, a second dependency phase 30%, unnamed 20%, falsified by verbose lines that place it
inside `Writing N object nodes`): REFUTED by that falsifier as worded**, and the first clause named the right
cause in the wrong place; `dotnet-stack ps` does not list `ilc`: MISSED, it does. Folding is the phase (55%):
MET. Names up 10 to 40% and image up 5 to 25% at PerfStartup: MET. `parsedemo` without folding: ILC under 8 min,
MET; the writer under 2 min, MISSED (2 m 20 s); names 760 MB in 700 to 860, MISSED on the good side (657.3);
peak and image, MET. `emitdemo`: builds and matches, MET; names 1,550 MB in 1,400 to 1,750, ILC 22 min in 16 to
32, image 570 MB in 520 to 660, **all three MISSED on the good side** (1,248.9 MB, 12 m 59 s, 477.1 MB); peak
33.5 GB in 30 to 37, MET at the bottom (30.09). The three misses share a cause: `emitdemo` was sized from
`checkdemo`'s object on an IL census that reads the two closures as equal, and the IL census has now failed as
a predictor twice.

## 19. Amendment, 2026-11-15 08:00Z: the four demos with the lever that ships, and what the IL census is worth

**`checkdemo` with lever G alone is measured, not projected.** Section 18 could only project it from
`parsedemo`'s ratio (1,453 MB with folding). Built like `emitdemo` and `mfdemo`, `651c4ad147` plus lever G alone,
no folding: exit 0, 0 `IL3054` lines, `checkdemo check.ts good.ts` matches native Go, and the type-check slice
matches 102 of 102. With it the answer of section 1 no longer rests on lever E for any image.

**The four demos, lever G alone, full trim, no native debug symbols.** Image size and ILC wall, with method
body folding off and on:

| demo | folding | symbol string table | image | ILC wall | ILC peak private | matches Go |
|:--|:--|:--|:--|:--|:--|:--|
| `parsedemo` | off | 657.3 MB | 280.1 MB | 7 m 06 s | 17.66 GB | demo; corpus 17,422 of 17,422 |
| `parsedemo` | on (SDK default) | 616.2 MB | 252.3 MB | 32 m 20 s | 15.69 GB | demo; corpus 17,422 of 17,422 |
| `checkdemo` | off | 1,248.6 MB | 476.8 MB | 13 m 00 s | 30.16 GB | demo; type-check slice 102 of 102 |
| `emitdemo` | off | 1,248.9 MB | 477.1 MB | 12 m 59 s | 30.09 GB | demo; emit slice 102 of 102 |
| `mfdemo` | off | 1,249.0 MB | 477.2 MB | 12 m 54 s | 29.93 GB | demo |
| `checkdemo`, `emitdemo`, `mfdemo` | on | not built | not built | not built | not built | |

The three large demos were not built with folding on. A table that fits without folding fits with it, so
their fit under the default is implied; their sizes and times under the default are **projections** from
`parsedemo`'s two rows (names about 6% lower, image about 10% lower, and an ILC run several times longer), not
readings. The one large image that was built with folding on is `checkdemo` under lever E v2 alone (section
14): 1,545.2 MB of names, 577.0 MB, 2 h 21 m 40 s of ILC, of which 124 minutes were the same one-core phase at flat memory.

**The largest table is 58.2% of the 2 GiB stream** with lever G alone and no folding. The limit of section 3 is
still there and still silent past 4 GiB; these images are under it with room, not past it.

**The IL census failed as a predictor of size twice, and it is recorded here as failed.** Section 6 built a
ratio model on IL metadata counts and section 13 showed it could not see generic instantiation. It was used
again to size `emitdemo` from `checkdemo` (their closures read equal to 0.02% in types and methods), and
`emitdemo` came in 19% under that size. The measured `checkdemo` cell says why: the three large objects ARE
equal, to 0.04%, exactly as the census read them. What was wrong was the ratio carried from `parsedemo`. Like for
like (lever G alone without folding, against lever E v2 alone with it), the names are 657.3 against 655.1 MB on
`parsedemo`, equal, and 1,248.6 against 1,545.2 MB on `checkdemo`, 19% lower: lever G is worth more the larger
the closure, and no ratio measured on the small one carries to the large one. So the census is good for one
thing, saying two closures are the same size, and for nothing else; a size is measured on an object.

**Stack frames of the folding pass were taken and have no names.** `dotnet-stack ps` lists `ilc.exe` (it has a
diagnostic port, which the prediction said it would not). `dotnet-stack report` at one, two and three minutes
into the pass on a default-folding PerfStartup build, and a 30 s `dotnet-trace`, all attached and returned
cleanly with ILC unharmed, and every frame reads `?!?`: `ilc.exe` is a Native AOT image shipped without symbols.
One busy thread about 13 frames deep and one idle thread, three times. That build also read ILC at 6 m 34 s
with the verbose logger on, against 6 m 29 s without it, so the logger does not move the wall.

**Scored.** `checkdemo` with G alone: names 1,300 MB in 1,200 to 1,560, peak 31 GB in 29 to 37, ILC 14 min in 12
to 20, matches Go: all MET. `mfdemo`: as `emitdemo` in every row. The sampler's 37 GB line was not reached by
any of the three; the smallest available RAM at a peak was 7.6 GB.

## 20. Amendment, 2026-11-15 11:30Z: the Windows run of record, at the tree that carries the `go-winio` fix

Section 17 read the demos on Windows at the last master that built there. This is the same script at
`5cc8de74b2` (`claude/c2-inline-foreign-slice`, which contains master `091935257c`): a fresh worktree, a fresh
conversion, TypeScript `ba29a56299`, go1.26.11, SDK 10.0.403, `GoTargetOS=windows`, under the JIT.

**The Windows build of the TypeScript closure is green again, for the first time since batch 302.** All four
demos build with 0 errors. `github.com.Microsoft.go-winio.pkg.guid`, the project that failed with 17 errors in
section 17, builds clean, and so do the two projects that reference it and that MSBuild had skipped behind it
(`go-winio.internal.socket` and `go-winio` itself), with everything above them: no fourth wall stood behind
the third.

| comparison against native Go | `651c4ad147` (section 17) | `5cc8de74b2` |
|:--|:--|:--|
| `parsedemo` demo | match | match |
| parse corpus | 17,422 of 17,422 | 17,422 of 17,422 |
| `checkdemo`, `emitdemo`, `mfdemo` demos | match, match, match | match, match, match |
| full emit, every single-file compiler case | 7,404 of 7,404 | 7,404 of 7,404 |
| full type-check, the same cases | 7,404 of 7,404 | 7,404 of 7,404 |

The four demo outputs hash the same at both trees. Between the two trees are batches 301 to 307 and the
`go-winio` cut, which change how arrays are stored; nothing in the converted compiler's output moved.

**The script's exit is 1 from its census step alone, as in section 17 and for the same two reasons:** 1,267
hits, of which 1,266 are the locally derived deny list matching the repository's own public URL in emitted
project files (0 outside that URL) and 1 is the `local` mode's absolute `go2csPath` in the staged
`Directory.Build.props`.

**If the converter changes after this run,** a rerun is owed only if the emitted `.cs` of the TypeScript closure
differs between the two conversions; this run's converted tree is kept for that comparison.

**Scored.** Converts and builds all four (65%): MET. One clean build line for the `guid` project (80%): MET. The
four small comparisons and the corpus (85%): MET. Full emit all match (85%): MET. Full check all match (85%):
MET. The script exits 1 from the census alone, the same two classes (90%): MET.

## 21. Amendment, 2026-11-15 15:30Z: the four images re-cut on the landed tree

Sections 13 to 19 built their images on `651c4ad147` with lever G applied by hand. Lever G landed in batch 308.
This is the same four images on master `b2214e00e4` as it stands: a fresh worktree, a fresh conversion (the gate
script's own first three steps), golib as landed with no patch, full trim, no native debug symbols, no method
body folding, one ILC at a time.

| demo | symbol string table | against section 19 | image | ILC wall | ILC peak private | `IL3054` | matches Go |
|:--|:--|:--|:--|:--|:--|:--|:--|
| `parsedemo` | 667.7 MB | +1.6% | 286.4 MB | 7 m 22 s | 17.17 GB | 0 | demo; corpus 17,422 of 17,422 |
| `checkdemo` | 1,270.3 MB | +1.7% | 488.3 MB | 13 m 40 s | 31.05 GB | 0 | demo; type-check slice 102 of 102 |
| `emitdemo` | 1,270.6 MB | +1.7% | 488.6 MB | 13 m 40 s | 30.90 GB | 0 | demo; emit slice 102 of 102 |
| `mfdemo` | 1,270.8 MB | +1.7% | 488.7 MB | 13 m 38 s | 31.35 GB | 0 | demo |

The largest table is 59.2% of the 2 GiB stream. The eight batches between the two trees (301 to 308), which
change how arrays are stored, cost these images under 2% in names and 2.2 to 2.4% in size, and nothing in what
they print. The
kept binaries are now the release tree's.

**One hand edit stands between a fresh local conversion and any Native AOT publish, and every build in this
record went through it.** `go2cs -recurse` emits a tree whose projects reference the `go2cs-gen` analyzer as a
project. That project targets `netstandard2.0`, and a `PublishAot=true` that reaches it fails with `NETSDK1207`
(reproduced at this tree by restoring the analyzer project alone with the property set). So each converted tree
used here carries four added lines in its `Directory.Build.targets`: `PublishAot=true` for the `Exe` project
only, under a private switch. `docs/Tuning.md` gives `dotnet publish -r <rid> -p:PublishAot=true` as the
route; that was not tested here against a tree converted with `-recurse=nuget`, where the analyzer arrives as a
package and there is no analyzer project for the property to reach. Whether `-recurse` should emit the `Exe`-only
switch itself is a converter question this record only raises.

**Scored.** All four build and match (85%): MET. Names within 3% of section 19's: MET (+1.6 to +1.7%). Peaks
within 2 GB of section 19's: MET (-0.49, +0.89, +0.81 and +1.42 GB). The sampler's 37 GB line was not reached.
