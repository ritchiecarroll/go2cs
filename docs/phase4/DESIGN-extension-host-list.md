# DESIGN — a build-time extension-host list for golib's scan under Native AOT

> **State: RULED (COORD, 2026-11-06 07:11Z): approved with two changes, folded in below.** Change 1: generators
> DECLARE their hosts; nothing parses generated text. Change 2: the under-listing guard covers the population. Order
> is ruled as proposed (§2.5). L3 stays OPEN and out of scope. Lever L2 of
> [`DATA-aot-structural-assert.md`](DATA-aot-structural-assert.md) §5, and BoGo lever 1's "per-assembly list
> written at build time" ([`DATA-bogo-startup.md`](DATA-bogo-startup.md) §8.4). The extension scan's JIT design is
> [`DESIGN-extension-scan-metadata.md`](DESIGN-extension-scan-metadata.md).

## 1. The problem, measured

Under Native AOT there is no raw metadata, so golib's extension scan falls back to `Assembly.GetTypes()`. On the
62-assembly probe (DATA-aot-structural-assert §4), the scan costs 250 ms of the first structural assert's 268 ms.
Of that, 140 ms is listing all 6,514 types and reading each one once, to keep the 155 classes of the right shape.

## 2. The list

**One assembly attribute per host class, plus one sentinel.** Both are defined in golib:

```csharp
[assembly: global::go.GoExtensionHostList]                          // this assembly's list is complete
[assembly: global::go.GoExtensionHost(typeof(global::go.fmt_package))] // one per host (AllowMultiple)
```

- **A host** is a top-level, non-generic static class that declares at least one extension method. That is the
  shape `DeclaredExtensionMethods` keeps, narrowed to what C# allows (CS1106).
- **Where it lives:** as assembly attributes in generated files, so nothing is added to the converted `.cs`. The
  emission and CNR do not move, and no tracked file changes (generator output is not checked in).

### 2.1 Who writes it: two writers, because no generator sees another's output

A Roslyn generator sees the converter's sources but never what a sibling generator adds in the same compilation.
Hosts come from both places:
- converter-written package classes;
- generator-written ones: `InterfaceTypeTemplate`'s `{pkg}_package_asserts` partial class, and `StructTypeTemplate`'s
  `{pkg}ᴛ{Struct}ᴛxpkg` sibling classes for cross-package promoted forwarders. `RecvGenerator`'s `ж<X>` twins go into
  the class that already declares the original method, so they add no new host.

So the list has two writers:
1. **A new `ExtensionHostListGenerator`** walks the compilation's own syntax trees, which are converter and
   hand-owned sources. It emits the sentinel and one `GoExtensionHost` per top-level non-generic static class that
   declares an extension method.
2. **Each generator template that emits an extension host declares it** (COORD's change 1), in its own output:
   `[assembly: GoExtensionHost(typeof(X))]` beside the class. Assembly attributes are legal in any file, so no
   sibling's output is needed. The template passes its host names to the one door,
   `GeneratedSource.AddGeneratedSource`, which writes the attribute lines; the door never parses source. A template
   that reopens a converter class as a partial and adds an extension method declares that class too.

Over-listing is harmless: a listed class with no extension method yields nothing from the unchanged method filter.
Under-listing loses Go methods under AOT only. That is why the guard in §4 compares the list against the metadata
walk.

### 2.2 How golib finds it without reflecting over every type

`LoadAssemblyExtensionMethods` keeps today's order:
1. `ExtensionHostTypes(assembly)`, the TypeDef walk, wherever raw metadata exists. This covers the JIT and
   ReadyToRun, and is unchanged.
2. Otherwise, if the assembly carries `GoExtensionHostList`, it reads `GetCustomAttributes<GoExtensionHost>()`, which
   reads only that assembly's own attribute table.
3. Otherwise `GetTypes()`, today's fallback.

**Reading the list only where there is no raw metadata is deliberate.** Under the JIT the TypeDef walk is exact by
construction and has no staleness surface. Using the list there would save only the 7 to 12 ms walk, and would put a
second, stale-able source on the path every behavioral test runs.

### 2.3 An assembly built without the generator

It has no sentinel, so it falls back to `GetTypes()`. That is today's behavior, per assembly; an assembly with a list
and one without can share a process.

The **stale-generator trap** (route #7's twin) is the reverse case: an assembly built by an older generator has no
sentinel, so it falls back, which is correct, only slower. The unsafe case is a NEW generator that under-lists. §4
guards that.

### 2.4 -tests whitebox bridges

A `-tests` host assembly is a converted project with go2cs-gen as its analyzer, like every other. Its whitebox
bridge classes (methods declared on production types, from the test assembly) are top-level static classes in that
compilation. Its own writer 1 lists them, and golib reads each scanned assembly's list. So the list holds for
bridges.

This is NOT L3's question. Whether a key for type `X` may read only `X`'s own assembly stays **OPEN**, and nothing in
L2 depends on it: L2 still reads every assembly's list.

### 2.5 Order

Today the scan returns methods in each assembly's `GetTypes()` order. The list's order is the attribute order, which
follows the generator files' order. Order is observable only through `GetExtensionMethod`'s `MinBy` tie-break, and
only between two same-named methods whose receivers have equal precedence. Within one Go package that does not occur,
because Go has one method per name and the twins differ in receiver type. Across assemblies, order already follows
load order today.

**OPEN:** the guard asserts the method SETS are equal per assembly. It also asserts that no assembly has two such
equal-precedence same-named methods, so that order cannot matter. If COORD wants the order itself equal, writer 1
can emit converter hosts in syntax-tree order. Generated hosts then cannot be placed in metadata order, which is the
reason to prefer the no-tie assertion.

## 3. The files

| file | change |
|:--|:--|
| `src/gen/go2cs-gen/ExtensionHostListGenerator.cs` | new: writer 1 |
| `src/gen/go2cs-gen/GeneratedSource.cs` and the host-emitting templates | writer 2: the templates declare, the door writes |
| `src/core/golib/GoExtensionHostAttribute.cs` | new: the two attributes |
| `src/core/golib/runtime/TypeExtensions.ExtensionMethodRegistry.cs` | step 2 of §2.2 |
| `src/tests/GenTests/ExtensionHostListTests.cs` | new: the template census, §4 |
| `src/tests/GolibTests/ExtensionHostListTests.cs` | new: the population guard, §4 |

There are no converter changes, no emitted `.cs` changes, and no hand-owned file changes.

## 4. Guards

- **GenTests, the template census** (change 2b). Every template that can emit a static class with an extension
  method is covered by a case asserting its host is listed. The census reads the generated trees in the TEST, not in
  the generator: any top-level static class with an extension method and no `GoExtensionHost` fails it, so a new
  template without a listing fails. Made to fail once.
- **GolibTests, the population against the metadata** (change 2a). The test loads every converted `src/core`
  assembly the test project can reach, not only those already loaded. For each one carrying the sentinel, the listed
  hosts must yield exactly the TypeDef walk's extension methods, with no same-name, equal-precedence tie. This runs
  under the JIT, where both sources exist. Made to fail once by dropping a host.
- **A Native AOT arm** (change 2c): a behavioral or Perf program whose asserts cross three or more assemblies, read
  against Go's output, beside the probe from DATA-aot-structural-assert.
- **Route #7, item 4:** a gen change that alters runtime behavior while emitting byte-identical `.cs` owes the full
  behavioral suite on all phases. Under the JIT the list is never read, so that suite is expected to be identical.
  The AOT probe is the arm that reads it.

## 5. The prediction

| quantity | base | predicted | reason |
|:--|--:|:--|:--|
| AOT first structural assert (probe) | 255 ms | **110 to 150 ms** | removes `GetTypes` (75 ms) and the first read of 6,359 non-host types (about 63 ms); adds the attribute reads (3 to 15 ms over 56 assemblies) |
| BoGo probe launch (JIT, R2R) | 836 ms | **unchanged within 3%** | the list is not read where raw metadata exists |
| JIT first structural assert (probe) | 117 ms | unchanged within 3% | as above |
| emitted `.cs` and CNR | — | byte-identical | the list lives in generated files only |
| GenTests | — | +5 tests, all others unchanged | |
| build time per converted assembly | — | under +3% | one syntax walk of the converter sources for writer 1 |

**Falsifiers:** the AOT first assert above 170 ms (a saving under 85 ms); any assert result or checksum that differs
from Go; any CNR drift.

Inferred, not measured: PerfTlsHandshake's 516 ms AOT warm-up would fall by the same share, about 250 to 290 ms. Its
ILC needs about 23 GB, so that is the i9's measurement.

## 6. Not in L2

- **L3,** carrying each method's receiver type in the list. It would save `GetParameters`: 72 ms under AOT and 48 ms
  under the JIT. It is the part that would move the BoGo probe. The list could carry it later.
- **The receiver-assembly scope.** OPEN, as §2.4 says.
