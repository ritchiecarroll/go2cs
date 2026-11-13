# DESIGN — B3: start-up method sets read by receiver type, not by package

> **State: DESIGN and PREDICTION (P5, 2026-11-13). Nothing is built.** Ordered by COORD (2026-11-13 22:30Z) for
> release 1.26.11.1, design first; COORD reviews this before any code. Source: P4's scout, `SCOUT-ts-perf.md` §B3
> (branch `claude/p4-ts-perf-scout`, `895e08794d`).
> Measured on one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400, go1.26.10), Release builds under the JIT,
> at master `651c4ad147` with a probe that is never merged. Data, probe and tools:
> [`data/startup-method-sets/`](data/startup-method-sets/). Every number is marked **measured** (here), **P4's**
> (read from P4's committed profile, not re-measured) or **inferred**.

## 1. Summary

1. **A converted `hello` asks no method-set question at all.** Its 336 ms is Go package initialization before
   `main` (297 of 331 sampled ms). B3 cannot move it. That is finding F1 in §11, outside the scope wall.
2. **A program that prints named types pays 120 to 280 ms more**, and the method-set path is 80 to 130 ms of it.
   The cost is not the question. It is that answering a question about ONE type reads EVERY extension method of
   every host class in that type's home assemblies: 5,931 methods read to find 197 candidates (**measured**, `wide`).
3. **The table already exists in each assembly, and the scan already reads it.** `WalkedExtensionHosts` decodes every
   extension method's receiver from the assembly's metadata to find each host's home assemblies, then throws the
   receiver away. Keeping it gives each host class the set of receiver types it extends. A question then reads only
   the hosts whose receivers it can match.
4. **That only pays once a host holds one type's methods.** On master a package class holds all of a package's
   methods, so scoping by receiver reads the same class. P1's method-holder move (OQ2, branch
   `claude/p1-oq2-method-holder`, `82fb35fc28`, queued ahead of this) puts each type's methods in a class of their
   own. On a build of that branch, reading only the asked types' hosts read 429 methods where the home scope reads
   3,138, and took 4.4 to 7.0 ms where the home scope took 24 to 28 ms (**measured**, §3). **B3 builds on OQ2.**
5. **go2cs-gen writes the same table for Native AOT**, where an assembly has no metadata to walk: the
   `GoExtensionHost` list gains each host's receivers, as it gained homes in L3. A guard holds the two sources equal.
6. Four arms, each with its own prediction (§8): A1 receiver-scoped host reads; A2 `error<T>` stops asking the
   whole scan; A3 the walk visits only the asked homes; A4 adapter registration runs per home. Together they are
   predicted to remove 100 to 135 ms of `wide`'s 650 ms on this box, and (inferred from P4's profile) 0.28 to 0.40 s
   of the TypeScript `hello`'s 2.08 s on P4's box.

## 2. Measurements

Three programs ([`programs/`](data/startup-method-sets/programs/)): `hello` (one `fmt.Println("hello")`);
`stringer` (prints a value type, a pointer, a `Stringer`, an `error` and a slice, all declared in `main`); `wide`
(prints values from `time`, `math/big`, `net/url`, `go/token`, `reflect`, `os`, `go/ast`, `runtime`, `bytes`; its
output hash equals Go's). Wall time is the median of 15 runs after 2 warm-ups (`t.py`).

| program | wall | CPU | `main` entered at | assemblies | methods jitted before `main` / at exit |
|:--|--:|--:|--:|--:|--:|
| plain C# hello (control) | 16 ms | | | | |
| Go `hello` (native) | 1 to 2 ms | | | | |
| `hello` | 336 ms | 503 ms | ~310 to 340 ms | 68 | ~2,900 / ~3,300 |
| `stringer` | 457 ms | | ~300 to 350 ms | 71 | ~2,900 / ~4,300 |
| `wide` | 651 ms | 1,059 ms | ~360 to 377 ms | 87 | ~3,330 / ~5,850 |

**What the method-set path does** (probe, one run each; inclusive times overlap, so they do not add):

| | `hello` | `stringer` | `wide` |
|:--|--:|--:|--:|
| structural questions (`StructurallyImplements`), distinct (type, interface) pairs | 0 | 16 | 28 |
| of them answered yes | 0 | 1 | 8 |
| method-set fills (`CollectGoMethodSetCandidates`), distinct (type, pointer?) keys | 0 | 7 | 14 |
| distinct Go types asked about | 0 | 4 | 11 |
| candidates those fills returned | 0 | 4 | 197 |
| host classes read / extension methods read | 0 | 3 / 12 | 152 / 5,931 |
| `StructurallyImplements`, inclusive | 0 | 40 ms | 80 ms |
| host method reads (`GetMethods`, `IsDefined`, `GetParameters`) | 0 | 6 ms | 70 ms |
| the metadata walk for hosts (`LoadAssemblyExtensionHosts`, all loaded assemblies) | 0 | 25 ms | 27 ms |
| `ExtensionMethodsOf` (the whole-scan read behind `error<T>`'s static constructor) | 0 | 0 | 46 ms |
| adapter registration (`GoDeferredRegistrations.Adapters.Run`) | 0 | 11 ms (15) | 30 ms (22) |
| `AdapterBinder.BuildFactory` (shells built) | 0 | 3 ms (1) | 55 ms (8) |

Every question comes from fmt's `handleMethods`: three assertions per printed value (`Formatter`, `Stringer`,
`error`). The sampled trace of `wide`'s main goroutine agrees with the probe: 256 ms in `main`, 202 ms of it under
`handleMethods`, 80 ms under `StructurallyImplements`, 69 ms in `ExtensionHost.Methods`
([`trace-wide-main-goroutine.txt`](data/startup-method-sets/trace-wide-main-goroutine.txt)).

**Per method, the read costs about 11 µs** (70 ms / 5,931), and the count is set by the size of the asked types'
packages, not by the types. That is why P4's box shows more: TypeScript's packages are large.

**P4's `hello` and `startup` cases were not re-measured here.** The TypeScript conversion is not on this box. P4's
committed profile of its `hello` (`cpu-hello.txt`) reads: 1,133 ms sampled in `main`, 586 ms under `handleMethods`,
462 ms under `StructurallyImplements`, 444 ms under `GetExtensionMethodsHomedIn`, 141 ms of that in
`WalkExtensionHosts`. The shape is the one measured here, at a larger scale. P4's `startup` case has no committed
start-up profile; §9 asks for the probe to be run on both.

## 3. Three ways to get one type's methods, priced

`alt-harness` loads `wide`'s built assemblies in a fresh process and reads the extension methods of the 11 asked
types three ways (cold, medians of 3 to 5 runs, **measured**):

| source | master build | method-holder build |
|:--|--:|--:|
| `scan`: reflection over every host of the asked homes (what golib does today) | 31 to 33 ms, 3,138 methods | 24 to 28 ms, 3,138 methods in 340 hosts |
| `holder`: reflection over only the hosts whose receiver is an asked type, plus hosts with no single receiver | same as `scan` (a package class holds every type) | **4.4 to 7.0 ms, 429 methods in 31 hosts** |
| `meta`: a per-method index from metadata, each asked method resolved by token | 15 to 31 ms (index 13 to 15 ms) | 23 to 45 ms (index 6 to 7 ms) |
| `names`: a per-type name list resolved with `GetMember` | 12 to 46 ms, 618 methods read | not run |

- A **name list** (the obvious generated table) is no better than the scan: a Go method name such as `String` is
  declared by many types in one package, so resolving by name reads other types' methods too, and reflection has no
  cheaper way to go from a name to a `MethodInfo`.
- A **per-method token index** resolves slowly (`Module.ResolveMethod`, about 85 µs per method in this prototype).
- **Reading only the asked types' host classes** is the cheapest, and it needs no new emitted shape: it needs to
  know which receivers a host class extends.

## 4. The design

### 4.1 One index, two sources (as L2 and L3 have)

**Under the JIT, golib only.** `WalkedExtensionHosts` (`TypeExtensions.ExtensionMethodRegistry.cs`) already walks each
assembly's TypeDef table, and for every extension method decodes the first parameter of its signature blob to find
the receiver's home assembly. The change: the decoder also returns the receiver's element (read through a by-ref,
through golib's `ж<X>` box to `X`, and a generic instance as its definition). A receiver defined in the walked
assembly is kept as its TypeDef token, with no type load; a receiver from another assembly is already resolved
there today, and is kept as its `Type`. Each `ExtensionHost` then holds `Receivers`, the set of receiver elements
its methods extend, and `AnyReceiver` for a receiver that names no type (a type parameter, an array, a pointer), as
it holds `Homes` and `AnyHome` today.

**Under Native AOT, the generator.** An assembly has no raw metadata there, and golib reads the build-time list.
`ExtensionHostListGenerator` (writer 1) and the templates that add host classes (writer 2) add
`Receivers = new[] { typeof(T) }` to each `GoExtensionHost`, read from the same symbols they read `Homes` from, and
the marker gains `ReceiversListed = true`. A host listed without the marker is read for every question in its
homes, which is today's behavior, so an assembly from an older generator stays correct.

**This is the per-assembly method-set table the brief asks for.** Per assembly it maps a host class to the types
whose methods it holds. Under the JIT it is the assembly's own metadata, which cannot disagree with the code it
describes (the reason `DESIGN-extension-scan-metadata.md` gave for not generating a list there). Under Native AOT it
is generated, and a guard holds it equal to the metadata (§6).

### 4.2 The question's receiver keys (arm A1)

The collector admits a method when its receiver element is the asked element, is the open definition of the asked
generic type, or is assignable from the asked element (`ReceiverElementMatches`). So a question about element `E`
can only match receivers among: `E` (its definition if generic), `E`'s base types, the interfaces `E` implements,
and `object`. Those are the question's **receiver keys**, computed beside `ExtensionHomesOf`. For the second
consumer (`GetExtensionMethods(Type)`, by assignability) the keys are the type's, its bases' and its interfaces'
definitions, and the pointee's when the type is a box.

`GetExtensionMethodsHomedIn(homes)` becomes `GetExtensionMethodsFor(homes, keys)`: a host is read when it extends
one of the homes **and** (`AnyReceiver` or its receivers meet the keys). The host's methods are read as today, in
`GetMethods` order, and hosts are visited in today's order. **Both consumers then run their unchanged filters on
what was read.** The index only decides which host classes are not read at all. It never decides an answer.

### 4.3 `error<T>` asks for its own type (arm A2)

`error<T>`'s static constructor calls `typeof(ж<T>).GetExtensionMethod("Error")`. For a box, `ExtensionMethodsOf`
matches by generic definition, so it reads the WHOLE scan (every host of every assembly) and returns every method
whose receiver is any box: 3,560 methods in `wide`, 46 ms, for one name on one type. The constructor then keeps the
method only if it binds as `ErrorByPtr` over `ж<T>`.

The change is in `error<T>` alone: it asks a new internal `GetBoxExtensionMethod(typeof(T), "Error")`, which reads the
hosts for `T`'s keys and applies the same name match and the same precedence comparer to the methods whose receiver
is a box. The public `GetExtensionMethods(Type)` keeps its answer for a box (finding F3).

### 4.4 The walk visits the asked homes (arm A3)

The first question walks every loaded converted assembly (27 ms for 87 here; 141 ms on P4's box) before it reads
one host. It must, because an assembly may declare a method on ANOTHER assembly's type (a `-tests` host's whitebox
methods; 35 of 10,205 methods in the crypto/tls host, none in any production assembly, per L3's census).

The generator writes `[assembly: GoExtensionReceiversLocal]` on a compilation none of whose extension methods has a
foreign receiver. golib then walks an assembly when a question's homes include it, or when it lacks the attribute.
One `Assembly.IsDefined` per loaded assembly replaces its walk. An assembly without the attribute (an older
generator, a hand-built assembly, a `-tests` host with bridges) is walked on the first question, as today.

### 4.5 Adapter registration per home (arm A4, the registration half)

`AdapterRegistry.TryGetAdapterFactory` runs EVERY loaded assembly's deferred `ᴛRegisterAdapters` on its first miss,
and a miss is the normal answer for fmt's assertions. It costs 11 ms (15 registrations) in `stringer` and 30 ms (22)
in `wide`; `go/ast`'s alone is 12 ms. The time is the JIT compiling each registration method and its generic
instantiations.

A registration for the pair (`T`, `I`) is written by an assembly that names both types, so that assembly is `T`'s
home or references it, and is `I`'s home or references it. On a miss for (`T`, `I`), golib runs only the pending
registrations of assemblies that satisfy both, read from `Assembly.GetReferencedAssemblies()` once per assembly.
A pending registration that is never reached is never compiled. `Defer` already receives the method, whose module
names the assembly, so the generated code does not change.

**A4 is the least certain arm**, and the design asks for it to be decided after A1 to A3 are measured: a program
whose printed types live in low packages (`runtime`, `time`) still runs most registrations.

## 5. How each case is covered

The collector, the signature comparison and the binder are not changed, so a case is covered when the hosts that
hold its methods are still read. For each case below: where its methods are, and why the keys reach them.

| case | where the methods are | why they are read |
|:--|:--|:--|
| value receiver | `M(this T)` in `T`'s host | receiver element `T` is a key |
| pointer receiver | `M(this ref T)` marked `[GoRecv]`, and RecvGenerator's `M(this ж<T>)` | both read through to element `T`; the pointer rule (`IsPointerSetByRefReceiver`, the value-method twin rule) runs unchanged afterwards |
| promoted, same package | forwarders in the struct's promoted-method class (`[GoPromotedHost(typeof(S))]`) | their receiver is the outer struct `S`, a key for a question about `S` |
| promoted, another package | forwarders in the sibling class (`…ᴛxpkg`), which carries no marker | the receiver is read from the signature, not from a marker; it is `S` |
| promoted through an embedded pointer | a `[GoCopyBound]` `this ref S` forwarder | receiver element `S`; the copy-bound rule runs unchanged |
| promoted through an embedded interface | forwarders on `S`, as above | receiver element `S` |
| run-time minted struct (`reflect.StructOf`) | the struct's own static methods | not from the scan at all; the collector's own arm, unchanged |
| generic instantiation | `M<K, V>(this Pair<K, V>)`, found open | the signature's generic instance is keyed by its definition, and the question's key is `E`'s definition; `CloseGoMethod` and `IGoMethodSource` run unchanged |
| alias | a Go alias is the same C# type as its target | the key is the runtime type, so there is one key |
| cross-assembly receiver | a `-tests` host's method on a production type | the walk already resolves a foreign receiver to its `Type`; the host is indexed under it, and A3 always walks an assembly without the local-receivers attribute |
| unexported method and package identity | unchanged candidates | `UnexportedMethodPackageMatches` reads the candidate's declaring class, and the candidates are the same `MethodInfo`s |
| hand-owned package, or a hand-owned type with methods in its package class | any host class the assembly declares | under the JIT the walk reads metadata, which every assembly has, whoever wrote it. Under AOT a host without `ReceiversListed` is read for every question |
| a receiver that is a base type, an interface or `object` | any host | the keys include the bases, the interfaces and `object` |
| a receiver that names no type (`this T`, an array) | any host | `AnyReceiver`: read for every question |
| an assembly loaded later | appended to the scan | the walk and the cache clearing on `AssemblyLoad` are unchanged |

## 6. What stays on reflection, and the equivalence proof

**Stays on reflection:** reading a selected host's methods (`GetMethods`, `IsDefined`, `GetParameters`), the
collector's attribute reads, the signature comparison, closing generic methods, and building the shell. These read
the code itself, per asked type, and are what keeps the answer identical. Hosts with `AnyReceiver`, assemblies
without metadata and without `ReceiversListed`, and dynamic assemblies keep today's path entirely.

**The proof is that the narrowed read loses nothing the filters would have kept.** Both consumers already take an
explicit `source` list (L3's guard uses it), so each answer can be computed from the whole scan and from the keyed
read and compared, as a list, in order.

1. **Fixture guard (GolibTests, red first).** `ExtensionReceiverScopeTests`' fixture gains a host per case in §5
   that it lacks. For every fixture type and both consumers: keyed answer == whole-scan answer. A count test reads
   `HostReadProbeForTest` to show a question reads only its hosts. Made to fail once by keying a promoted-method
   host under the embedded type instead of the outer struct.
2. **Reachable guard (GolibTests).** `EveryReachableAnswerIsTheFullScans` extended: every receiver type found in
   every assembly the test project references, value and pointer, both consumers, plus `error<T>`'s pair of methods
   old way and new way (A2).
3. **WHOLE-corpus guard (new, a gate leg).** A GolibTests test that takes a directory of built assemblies
   (`GO2CS_CORPUS_BIN`), loads every one into a collectible context, and for EVERY receiver type the whole scan finds,
   compares the keyed answer with the whole-scan answer for both consumers and both receiver forms, and the A2 pair.
   Run over the output of the `go2cs-stdlib.slnx` build for each of the three targets, and over the `-tests` hosts
   of the bridge packages (crypto/tls, runtime, sync, log/slog). It fails when the variable is unset under the gate
   script, and is Inconclusive otherwise. Made to fail once on the corpus by dropping one host's receivers.
4. **List guard (GolibTests and GenTests).** For every converted assembly, the generated `Receivers` of each listed
   host equal the walk's, and `GoExtensionReceiversLocal` is present exactly when the walk finds no foreign home.
5. **Live cross-check.** With `GO2CS_METHODSET_VERIFY=1` golib computes every asked answer both ways and throws on a
   difference. One full behavioral run and the `-tests` sample run with it set, so every question real programs ask
   is compared, not only every type.
6. **A4's guard.** With the same variable, after the gated run on a miss, golib runs every remaining registration
   and fails if the pair is then found.

## 7. Footprint and gating

- **Committed corpus `.cs`: 0 lines predicted.** A1 (JIT), A2 and A4 are golib only. The generator's output is not
  committed. CNR is predicted byte-identical.
- **Compiled output changes in every converted assembly** (the `Receivers` arguments and one assembly attribute).
  Gated by: the three-target `go2cs-stdlib.slnx` compile at 0 errors; `go2cs.slnx` built end to end once; the full
  behavioral runner; GolibTests Release and Debug; GenTests; `go test ./...`; the `-tests` sample; and guard 3 over
  the three targets' output. A Native AOT publish of one behavioral project and of the structural-assert probe
  checks the list source.
- **Public surface:** two named arguments and one attribute added to golib (`PublicSurface.golib.txt`), no removal.
- **Order:** after OQ2 lands on master. On master as it is, A1 changes no timing (a package class has every
  receiver), while A2, A3 and A4 do not depend on OQ2.

## 8. Predictions

Same box, same builds, medians as in §2, before = the method-holder build without B3.

| arm | quantity | before | predicted |
|:--|:--|--:|:--|
| A1 | `wide`: extension methods read | 5,931 | **under 1,200** |
| A1 | `wide`: host method reads | 70 ms | **under 20 ms**; wall **−40 to −60 ms** |
| A1 | `stringer`: wall | 457 ms | within noise (it reads 12 methods today) |
| A2 | `wide`: `ExtensionMethodsOf` | 46 ms | **under 6 ms**; wall **−30 to −45 ms** |
| A3 | `stringer` and `wide`: assemblies walked on the first question | 71 / 87 | **under 12 / under 20**; wall **−12 to −22 ms** each |
| A4 | `wide`: adapter registration | 30 ms | **−0 to −20 ms** (uncertain; see §4.5) |
| all | `wide` wall | 651 ms | **515 to 550 ms** |
| all | `hello` wall | 336 ms | **unchanged** (it asks nothing) |
| all | TypeScript `hello` on P4's box (inferred from P4's profile: 444 ms under `GetExtensionMethodsHomedIn`, 141 ms of it the walk) | 2.08 s | **1.68 to 1.80 s** |
| all | every answer, corpus-wide | | identical |

**Falsifiers:** `wide` improves by less than 70 ms with A1 to A3; `wide` still reads more than 2,000 methods; any
answer differs in any guard; CNR drifts; a `-tests` bridge package's counts move; TypeScript `hello` improves by less
than 0.2 s on P4's box.

## 9. Measurement plan

- **Here, per arm:** the probe and `t.py` on `hello`, `stringer` and `wide`, each arm alone on the method-holder
  base, then all together; interleaved before/after pairs, 15 runs.
- **P4's harness, before and after, same box, same arms** (`tools/bench.py`, cases `hello` and `startup`), once
  go.* 1.26.11.1 exists as P4's lane already plans. Before that, the probe patch run once on P4's box on both
  cases gives the counts this design could not measure (questions, types, methods read, the walk, registration),
  which scores the TypeScript prediction's inputs before any code. **Asked of COORD.**
- **Native AOT:** the structural-assert probe's first assert before and after (L3's instrument).

## 10. Stop lines

- **OQ2 does not land in this release:** A1 is not built. A2, A3 and A4 stand alone and are re-predicted
  (`wide` −45 to −85 ms).
- Any guard difference that is not a bug in the guard: stop and post it. An answer is never changed to match.
- A1 to A3 together under 70 ms on `wide`, or under 0.2 s on P4's `hello`: stop and report before A4.
- Any change wanted outside start-up method sets and registration (§11): posted, not built.

## 11. Findings outside the scope wall

- **F1. Package initialization is the fixed start-up cost.** In `hello`, 297 of 331 sampled ms are the chain of
  module initializers that runs before `main`: `go.runtime` 172 ms inclusive, `os` 81, `io/fs` 54,
  `internal/cpu` 40, `time` 38, `internal/godebug` 20, golib 14
  ([`trace-hello-startup-thread.txt`](data/startup-method-sets/trace-hello-startup-thread.txt)). About 2,900
  methods are compiled before `main`. Most of the time is the runtime's own type initialization and JIT, not
  converted code running. No method-set or registration work is in it. **This, not B3, is what a converted `hello`
  pays, and it is likely the larger half of P4's 2 s.** Not designed here.
- **F2. `AdapterBinder.BuildFactory` costs 6 to 7 ms per first shell** (55 ms for 8 in `wide`). It is the
  interface-shell backlog's ground, not this design's.
- **F3. `GetExtensionMethods(typeof(ж<X>))` returns every box-receiver method in the process**, for any `X`,
  because a box is matched by its generic definition. A2 stops `error<T>` asking it; the public answer is not
  changed here.
- **F4. The method-holder branch does not move start-up by itself**: `hello` 344 to 356 ms against 360 to 380 ms,
  and `wide` 610 to 625 ms against 612 to 643 ms, interleaved, within noise.

## 12. Questions for the review

1. **Order against OQ2.** Is B3 cut on top of `claude/p1-oq2-method-holder`, or held until OQ2 is on master?
2. **Is the metadata source acceptable as "the table" under the JIT**, with the generated list for AOT only? The
   alternative, reading a generated list under the JIT too, costs a type load per listed host, and
   `DESIGN-extension-scan-metadata.md` ruled it out because it replaces only the walk and can go stale.
3. **A3 and A4 in or out of this release.** A1 and A2 are the bulk on large packages; A3 matters most to small
   programs; A4 is uncertain.
4. **F1.** Whether package initialization gets its own scout.

— P5 (2026-11-13)
