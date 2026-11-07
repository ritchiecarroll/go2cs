# DESIGN — L3: scope the extension scan by the receiver's home assembly

> **State: SCOUT and PREDICTION (2026-11-07), nothing built.** COORD ordered L3 at 01:52Z: a census of where `-tests`
> whitebox bridges declare methods on another assembly's types, the design that keeps them correct, the expected
> savings and the falsifiers. Measured on one linux lane box (4 cores, SDK 10.0.403, go1.26.10) at master
> `df7c09ab7b`, with a startup-hook instrument that is never shipped. Data:
> [`data/extension-receiver-scope/`](data/extension-receiver-scope/).

## 1. What L3 is

golib's extension scan (`TypeExtensions.GetExtensionMethods()`) reads EVERY extension method of every loaded
converted assembly, and asks each one's receiver with `GetParameters()`, before it can answer the first method-set
question. Every consumer then filters that list down to one type: `GetGoMethodSetCandidates` keeps receivers whose
element IS the asked element (or its generic definition), and `GetExtensionMethods(Type)` keeps receivers assignable
from the asked type. L3 indexes the methods by the receiver's **home assembly** and reads only the homes a question
can match. The records before this one priced it at `GetParameters`: 72 ms under Native AOT and 48 ms under the JIT
on the structural-assert probe ([`DATA-aot-structural-assert.md`](DATA-aot-structural-assert.md) §4-5).

The behavior question left OPEN by L2 ([`DESIGN-extension-host-list.md`](DESIGN-extension-host-list.md) §2.4) was
whether a key for type `X` may read only `X`'s own assembly, given that a `-tests` host declares methods on production
types. The answer this design gives: scope by the RECEIVER's home, never by the declaring assembly. A bridge method is
indexed under the production type's assembly wherever it is declared, so it is found exactly as today.

## 2. The census

**Go source** ([`bridges-goroot-go1.26.10.txt`](data/extension-receiver-scope/bridges-goroot-go1.26.10.txt), go/ast,
every directory of go1.26.10 `src` with test files): a method declared in a package's INTERNAL test file on a type
its non-test files declare becomes, under `-tests`, an extension method in the test host assembly on a production
type. That happens in **26 packages, 106 methods** (cmd included). crypto/tls has 18 (16 `Generate` methods for
testing/quick, and `SessionState`'s `marshal` and `unmarshal`); runtime 9, log/slog 8, simd/archsimd 8, database/sql 7,
internal/runtime/maps 7, sync 6. The test files' other 928 methods are on types the test files declare, so their home
is the host itself. An external test package (`p_test`) cannot declare methods on `p`'s types.

**Run time, the BoGo probe's closure** ([`scan-parts-run1.tsv`](data/extension-receiver-scope/scan-parts-run1.tsv),
[`cross-assembly-receivers.tsv`](data/extension-receiver-scope/cross-assembly-receivers.tsv)): the crypto/tls `-tests`
host loads 242 assemblies, 182 of them with extension hosts, declaring **10,205** extension methods. The instrument
walks them as golib does and resolves each receiver's home: the receiver itself, the element of a `ж<X>` box, the
generic definition of a generic type, a by-ref receiver's element.
- **35 of the 10,205** have a home other than their declaring assembly. All 35 are in `crypto.tls.tests`, on
  `crypto.tls` types, in the class `crypto.tls_internal_test_package`: the 18 Go bridge methods above, as a `ref`
  original plus its `ж<X>` twin (`marshal` has only the twin). The source census and the run-time census agree.
- **No production assembly** declares a method on another assembly's type: generated promoted forwarders, generic
  definitions and hand-owned files all have their own assembly as the receiver's home.
- **No receiver is a bare type parameter** (`this T`), which no home could describe.

**What the probe asks** ([`probe-keys.txt`](data/extension-receiver-scope/probe-keys.txt)): the whole launch
(`-bogo-mode -is-handshaker-supported`) asks the scan **3 questions**, all method-set candidates, from flag
registration: `crypto.tls.tests`' `intSlice`, `flag.boolValue` and testing's `ChattyFlag`. Their three home
assemblies declare 545 of the 10,205 methods. The instrument read the caches golib clears on each assembly load,
just before each clear, so no key asked between two loads is missed.

## 3. Where the scan's time goes

Cold, before `Main`, the instrument's walk of the 242 assemblies, medians of 3 runs, tiering off as the probe runs:

| part | all assemblies | the 3 asked homes |
|:--|--:|--:|
| metadata walk to the host classes | 17.9 ms | 0.4 ms |
| `GetMethods` + `IsDefined(Extension)` | 12.6 ms | 0.5 ms |
| `GetParameters()` on each receiver | **114.8 ms** | 5.3 ms |

Inside the launch the scan is cheaper, because some of those types are already loaded by then: golib's scan seam
(`ScanProbeForTest`) puts its passes at **at least 70 to 89 ms** across 5 launches. That figure adds up the spans from
first to last assembly start, so it leaves out each pass's last assembly
([`scan-passes/`](data/extension-receiver-scope/scan-passes/)).

## 4. The design

**One index, two sources, as L2 has.**
1. **Under the JIT (raw metadata), golib only.** The metadata walk that finds the host classes already holds a
   `MetadataReader`. For each method of a host class carrying `ExtensionAttribute`, it decodes the first parameter of
   the method's signature blob, with no type load:
   - a by-ref is read through to its element;
   - golib's `ж<X>` is read through to `X`;
   - a generic instance is read as its definition;
   - a `TypeDef` is homed in this assembly; a `TypeRef` is homed in its resolution scope's assembly name.
   The method's token goes into an index keyed by home name. `MethodInfo` and `GetParameters` are resolved only
   when a question asks that home, then cached per (home, assembly) in visit order. That order is today's, so the
   precedence tie-break sees the same order.
2. **Under Native AOT (no metadata), the generator.** L2's list gains, on each `GoExtensionHost` whose methods
   extend another assembly's types, the homes it extends (a type from each), and a new completeness marker that says
   the homes are listed. A host without the marker is read for every question, which is today's behavior, so an
   assembly from an older generator stays correct.
3. **The question's homes.** For a method-set candidate key: the element's assembly (the generic definition's for a
   generic element). For `GetExtensionMethods(Type)` (assignability): the assemblies of the type, its base types and
   its interfaces. The existing filters then run unchanged on what the homes hold, so every answer is the same
   subset as today's.
4. **Loads late.** A new assembly is walked into the index when it loads, never re-walked, and the per-key caches are
   cleared as now. The no-lock rule of the scan stays as it is.

**What it changes for a bridge**: nothing observable. `clientHelloMsg`'s home is `crypto.tls`. Its key reads
`crypto.tls`'s methods and the test host's `Generate`, which the index holds under `crypto.tls`.

**Guards (red first):**
- GolibTests: for every assembly the test can reach and every receiver type found, the home-scoped answer equals
  the full scan's filter, for both consumers. Made to fail once by homing a bridge under its declaring assembly.
- GolibTests: a two-assembly bridge fixture.
- GenTests: the homes list for a bridge host, and none for an ordinary host.
- Under AOT, the L2 guard extended: the listed homes equal the metadata-decoded homes, per host.
- Route #7: the full behavioral suite, and the `-tests` hosts of a few bridge packages (crypto/tls, runtime, sync,
  log/slog) at their banked counts.

## 5. The prediction

| quantity | base | predicted |
|:--|--:|:--|
| BoGo probe launch (JIT, tiering off, interleaved pairs) | 862 to 882 ms | **−45 to −90 ms** |
| JIT first structural assert (the assert probe) | 118.7 ms | **55 to 80 ms**: removes most of `GetParameters` 48.5, `IsDefined` 5.8, `GetMethods` 3.0 (inferred from §4 of the assert record, not measured here) |
| Native AOT first structural assert (the assert probe, after L2) | 114 to 120 ms | **20 to 50 ms**: removes most of `GetParameters` 71.6, `IsDefined` 25.7, `GetMethods` 9.6 (inferred) |
| behavioral suite, GolibTests, `-tests` bridge hosts | — | identical results |
| CNR | — | byte-identical (the JIT part is golib only, and the AOT list lives in generated files) |

**Falsifiers:**
- the BoGo probe falls by less than 35 ms;
- the JIT first assert stays above 90 ms;
- any answer of either consumer differs from the full scan on any reachable type;
- a bridge package's `-tests` counts move;
- CNR drift.

**Not scored here:** PerfTlsHandshake's AOT warm-up (G's 516 ms, the i9's box).

— P1 (2026-11-07)
