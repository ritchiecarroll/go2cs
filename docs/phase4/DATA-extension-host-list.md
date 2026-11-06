# DATA — the build-time extension-host list (L2), measured

> **State: MEASURED (2026-11-06), one linux lane box (4 cores, ILC 10.0.14), branch `claude/p1-ext-host-list` cut at
> the AOT-assert tip `9979ee0d6b`, merged with master `4a74aecde5`.** The design and its two ruled changes are
> [`DESIGN-extension-host-list.md`](DESIGN-extension-host-list.md). The prediction (§5 there) was posted before the
> first run and is scored in §7. Raw readings are in [`data/extension-host-list/`](data/extension-host-list/).

## 1. What landed

| commit | what |
|:--|:--|
| `b7266514e6` | golib: the two attributes and `ListedExtensionHosts`; the scan reads walk, then list, then `GetTypes()` |
| `964370e2e7` | red: GolibTests' population guard (128 converted assemblies with no list at this commit) |
| `254a04ebec` | go2cs-gen: writer 1 (`ExtensionHostListGenerator`), writer 2 (`DeclareExtensionHost`), GenTests' census |

No converter, emitted `.cs` or hand-owned change. The list lives in generated files, which are not tracked.

**Writer 1** lists every top-level, non-generic static class the compilation's own sources declare: 193 classes
over 131 assemblies in GolibTests' closure, all distinct, at most 10 in one assembly (`reflect`).

**Writer 2** declares the classes templates ADD: `{pkg}ᴛ{Struct}ᴛxpkg`, `{pkg}_package_asserts`, the promoted-twins
class, and the interceptor file class `ˢInterceptors` (only its own file can name it).

**One reading of change 1, stated plainly.** A template that reopens the converter's package class and adds
extension methods to it does not re-declare that class: writer 1 already lists every static class the sources
declare, package classes included. Re-declaring would write one attribute per generated file, about 1,200 in
`runtime` alone. No template was found that cannot name its hosts at emit time.

## 2. Method

- **Probe:** the scratch `_p1aot_Probe` from [`DATA-aot-structural-assert.md`](DATA-aot-structural-assert.md),
  rebuilt from its kit at `254a04ebec`, deleted after. Base = that record's kept `882faace3a` images (the same golib
  and gen code as this cut's base). 1 warm-up and 24 launches per arm; the AOT pair was run twice, interleaved.
- **The 3-assembly AOT arm:** a scratch copy of `Behavioral/XpkgPromotedMethodSet` with `<PublishAot>` set,
  published at the tip and at the base. Its asserts cross `main`, `XpkgPromotedInnerLib`, `XpkgPromotedMidLib`,
  `encoding/json` and `sync`, and its xpkg sibling classes are writer 2's. Output diffed against `go run .`.
- **BoGo probe:** `measure.sh` (the extension-scan kit) at the tip, then again with golib and gen checked out at the
  base, back to back.
- **Build time:** `runtime.csproj` and `fmt.csproj` alone (`--no-dependencies --no-incremental`), go2cs-gen at
  the base against the tip, alternated, 3 rounds.

## 3. First structural assert

Medians, p25 to p75 in brackets:

| arm | first value assert | first pointer assert | process wall |
|:--|--:|--:|--:|
| AOT, base | 254.0 ms (252.2 to 257.0) | 1.12 ms | 462 ms |
| AOT, list | **119.7 ms** (116.3 to 137.8) | 1.14 ms | 337 ms |
| AOT, base again | 251.9 ms (249.6 to 254.7) | 1.11 ms | 458 ms |
| AOT, list again | **114.4 ms** (113.7 to 114.9) | 1.11 ms | 321 ms |
| JIT, base | 118.7 ms (117.4 to 125.1) | 11.59 ms | 578 ms |
| JIT, list | 118.6 ms (116.6 to 121.3) | 11.60 ms | 583 ms |

The AOT first assert falls by 134 to 137 ms, and process wall by 125 to 136 ms. The JIT is unchanged, as designed:
the list is never read where raw metadata exists. The probe's output is identical in all four arms.

## 4. Across three assemblies, against Go

60 of 61 lines match Go under Native AOT. Line 38 differs:

```
Go:  TT value has Error: true
AOT: TT value has Error: false
```

**The same line reads `false` at the base,** and the JIT build matches Go on all 61 lines. So the list did not cause
it: it is an existing Native AOT gap. `TT` embeds `*testing.T`, and the assert asks for the hand-owned variadic
`Error(...any)` promoted through the pointer embed. The method-set line for `TT` (`report`) matches Go under AOT, so
the method is found and the assert's match fails. Not chased in L2.

## 5. BoGo probe and build time

| | base | list |
|:--|--:|--:|
| BoGo probe launch (24) | 871 ms (844 to 894) | 872 ms (836 to 913) |
| PerfStartup, runner JIT | 463.2 ms | 464.8 ms |
| PerfStartup, JIT exe | 510.1 ms | 495.9 ms |
| PerfStartup, own R2R | 305.1 ms | 298.4 ms |

| compile, seconds | round 1 | round 2 | round 3 | median |
|:--|--:|--:|--:|--:|
| runtime, base gen | 170.3 | 202.6 | 198.1 | 198.1 |
| runtime, tip gen | 200.5 | 208.6 | 204.0 | 204.0 |
| fmt, base gen | 8.1 | 12.2 | 12.1 | 12.1 |
| fmt, tip gen | 11.4 | 11.2 | 11.9 | 11.4 |

`runtime` reads +3.0% on the medians (+2.9% and +3.0% in the two paired rounds after the first); `fmt` reads -6%.
Round to round the same arm moves by up to 19% (runtime, base) and 51% (fmt, base), so this instrument does not
resolve 3%.

## 6. Guards, each made to fail once

- **GolibTests `ExtensionHostListTests`** loads every assembly in its output directory that references golib.
  At the tip: every one carries the list, the list yields exactly the TypeDef walk's extension methods on all of
  them, and no name is declared by two host classes over the same receiver or over two interfaces.
  - Red at `964370e2e7`: 128 assemblies with no list.
  - Dropping writer 2's `_package_asserts` declaration: 31 assemblies named, each with its omitted class.
- **GenTests `ExtensionHostListTests`:** one case per host-writing site, writer 1's selection and binding, the
  no-attribute paths, and a census of every generator literal that writes `static class`.
  - The same drop failed `TheInterfaceAssertsClassIsListed`.
  - A new `static class` literal in `FieldAccessGenerator` failed the census, naming `FieldAccessGenerator:Source`.
- Three assemblies that used nothing from golib now reference it through the attribute; 128 became 131.
- `GoFieldAccess` (C# 14 extension blocks) carries `[Extension]` on the class and no extension method, so the walk
  and the list both yield nothing from it.

## 7. The prediction, scored

| quantity | predicted | measured | score |
|:--|:--|:--|:--|
| AOT first assert | 255 → 110 to 150 ms | 254.0 → 119.7, 251.9 → 114.4 | **MET** |
| BoGo probe | unchanged within 3% | 871 → 872 ms | **MET** |
| JIT first assert | unchanged within 3% | 118.7 → 118.6 ms | **MET** |
| emitted `.cs` and CNR | byte-identical | see §8 | |
| GenTests | +5, all others unchanged | +9 (431 → 440), all others pass | **MISSED** on the count |
| build time per assembly | under +3% | runtime +3.0%, fmt -6%, noise up to 19% | **NOT RESOLVED** |

The GenTests count missed because the census asked for one case per site, plus binding and no-attribute cases.

**Falsifiers.** AOT first assert above 170 ms: did not fire. CNR drift: see §8. Any assert unlike Go: one line is
unlike Go under AOT (§4), and it is identical at the base, so it is not this change's; recorded as an existing gap.

## 8. Gates

At `6cc3c2ec83`, the cut merged with master `4a74aecde5` (batches 205 to 210):

| gate | result |
|:--|:--|
| GenTests | 440 passed, 0 failed (431 before the cut) |
| GolibTests Debug | 2,386 passed, 46 skipped, 0 failed |
| GolibTests Release | 2,411 passed, 21 skipped, 0 failed |
| `go test ./...` (`-timeout 40m`) | ok |
| CNR | byte-identical on all 957 behavioral packages (12 platform-exclusive skipped) |
| full behavioral suite (route #7 item 4), all four phases | 3,564 passed, 48 skipped, 0 failed, of 3,612 |
| `go2cs.slnx` members outside `tests/Behavioral` | 103 of 103 `.csproj` built, 0 failed (the 2 `.shproj` are shared, not built alone) |

The behavioral suite ran as `dotnet test BehavioralTests.csproj` in 13 first-letter chunks plus the runtime-test
classes, purging the behavioral `bin`/`obj` between chunks: this box holds about 17 GB, and building every member
of `go2cs.slnx` at once filled it. The chunks partition the 3,612 test methods exactly. The 963 behavioral
`go2cs.slnx` members were built by that suite, except the platform-exclusive ones, which do not build on linux at
master either.

The same gates at the first merge (`489035c3d9`, master `52d69e8968`) were green too: GenTests 440, GolibTests
Debug 2,383 and Release 2,408 with 0 failed, `go test` ok, CNR byte-identical on 957.
