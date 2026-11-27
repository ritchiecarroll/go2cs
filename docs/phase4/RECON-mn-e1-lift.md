# RECON: what lifting M:N rule E1 takes (generic functions and methods of generic types)

**Date:** 2026-11-27. **Lane:** C1. **Asked by:** COORD (from G), to size E1 in case P6's conc census reads mostly E1.
**Tree:** master `317b273e58` (train 17). **Instrument:** `src/go2cs/internal/e1census` on `claude/c1-mn-e1-recon`.
**Reviewer:** G. This is a read-only recon: no generator source was edited.

## 1. In short

- **The premise needs one correction.** E1 does not keep a parking generic from being twinned. `PreRefusal` has no
  generic row, `Twinned` is `Refusal is null && Parks`, and `TwinText` clones the method declaration, so a generic
  method the generator sees park already gets a generic twin (GenTests `P9_AGenericCalleeTakesItsBox` asserts
  `gpꞏtwin<TV>(global::go.ж<TV> Ꮡp)`). What `IsE1` gates is the **forwarder**, the population report's row, and the
  guard GO2CS0017. Seven more generic refusals live on the interface and call sides, and two of them decide whether a
  generic helper that takes a callback can carry a park at all (section 3, C-2 and C-3).
- **On the standard library E1 is small.** 109 exported generic functions (the C# E1 population before the
  hand-owned exclusion). 8 of them park when locks count, and every one of the 8 parks only through a lock. With
  locks not counted, as the generator does (N1), **0** park. The freeze's P2 predicted 130-330 public generic
  members with under 25 parkers.
- **x/sync v0.24.0 declares no generics** (4 files, 18 functions), so its E1 row is empty by construction.
- **conc v0.3.0 is not read yet.** It is not on this box, and the fetch waits for the owner's yes (C1's post
  `20261127T130435Z`). conc is the generic-heavy module of the set, so its row is the one that decides whether a lift is owed.

## 2. What E1 does today

E1 is one named rule (DESIGN-mn-twin-set.md 12.1): *a generic method, a method of a generic type, or a member of a
generic interface; excluded at the freeze; the lift is owed and additive.* In the generator it is the predicate
`IsE1` (`TwinGenerator.cs:687`), true for `IsGenericMethod` or any generic containing type.

How a Go generic reaches C# decides which half of E1 it meets:

| Go declaration | C# emission | in the population (`InPopulation`: public, converted file) |
|---|---|---|
| exported generic function `F[T]` | `public static R F<T>(…)` in the package class | yes: E1 when it does not park, twinned when it does |
| method of a generic type, `(n *Null[T]) Scan` | `internal static error Scan⁔<T>(/*this*/ ж<Null<T>> Ꮡn, …)` (glyph body) behind a generated public extension | no: the glyph body is internal and the extension is generated, not converted |
| generic interface `Point[P]` | `partial interface Point<P>` | its members meet the interface side's `generic-interface` |

So the population's E1 members are, in practice, the exported generic **functions**. A method of a generic type
reaches the twin set only as an internal candidate, where no forwarder is owed and E1 never applies: it is twinned
if it parks and plain if it does not.

## 3. Every consumer, and what each needs to lift

**The E1 rule itself (`IsE1`).**

| # | site | today | to lift |
|---|---|---|---|
| E1-a | `IsE1`, `TwinGenerator.cs:685-701` | the predicate | keep it as the name for what is left out after the lift, or delete it with E1-b to E1-e |
| E1-b | the forwarder gate, `TwinGenerator.cs:347` (`!IsE1(candidate.Symbol)`) | a public generic that does not park, or is refused, gets no forwarder | `ForwarderText` (`TwinGenerator.Emit.cs:263`) writes neither the method's `TypeParameterList` nor its `ConstraintClauses`, and calls `M(args)` with inferred type arguments. It needs both copied, and the call written `M<T1, …>(args)`, because inference fails when a type parameter appears only in the result. The host chain already reopens generic partial types with their parameters (`Emit.cs:70`), so a member of a generic host needs nothing more there. Converter-written constraint clauses (an elided `where P : /* … */ new()` or a constraint proxy) are copied as written. They bind in the same compilation, but a proxy that is `internal` cannot appear in a public forwarder's signature; that is the CS0050 class the 1.24 hop met on fips140, so it gets a row |
| E1-c | `Classify`, `TwinGenerator.cs:733-740` | the report writes `E1` | becomes `forwarder:<reason>` once E1-b forwards; no code beyond E1-b |
| E1-d | the guard GO2CS0017, `TwinGenerator.cs:350-360`; message `GeneratorDiagnostics.cs:231` | an E1 member is exempt | after the lift a generic the forwarder cannot carry is a GUARD row. Give `NotForwardable` a named reason for each generic shape it refuses (for example an inexpressible constraint), and drop "or excluded by E1" from the message |
| E1-e | GenTests `TwinSurfaceTests.F8_AGenericFunctionIsE1ByName` (and `F9`, which passes the same `Generic` source) | asserts no twin and no forwarder for `ident<T>` | flips to assert the generic forwarder, `identꞏtwin<T>(T v) => new GoTask<T>(ident<T>(v))`; that is the lift's red-first row |

**The interface side** (DESIGN-mn-twin-set.md row E1 names these refusals as E1's).

| # | site | today | to lift |
|---|---|---|---|
| I-1 | `IfaceRefusal` `generic-interface`, `TwinGenerator.Iface.cs:123-126` | a member of a generic interface gets no twin slot, so a call through it runs plain | a twin interface carrying the same type parameters (`Iꞏtwin<P> : I<P>`), with its implementers forwarded per instantiation. That is an arm, not a forwarder. Population: the stdlib declares 4 generic interfaces, each a self-referential constraint (`nistPoint[T]`, two `Point[P]`, `wrap[T]`), and none of their members park |
| I-2 | `IfaceRefusal` `generic-method`, `Iface.cs:132-133` | a generic interface METHOD | Go interfaces have no generic methods, so converted code never meets it. Keep it as a refusal |
| I-3 | `ForwarderSkip` `generic-host`, `Iface.cs:160-161` | an explicit implementation inside a generic host is skipped | follows from I-1: the slot exists only once the generic interface has a twin |
| I-4 | `ForwarderSkip` `target-shape` for a generic target, `Iface.cs:184` | the slot's target extension may not be generic | the same as E1-b's explicit type arguments, applied to the slot's call |

**The call side.**

| # | site | today | to lift |
|---|---|---|---|
| C-1 | `generic-unfollowed`, `TwinGenerator.cs:392-399` and `TwinGenerator.BoxTwin.cs:346-372` | a box-overload call to a GENERIC method is not followed to its box twin; the plain call is named | `BoxOverloadTarget` already constructs the definition over the overload's type parameters to detect the case. Follow that construction instead of only naming it (the "substituted matching" the comment defers) |
| C-2 | func-value pairs, `TwinGenerator.Pairs.cs:56-62` (`ContainsTypeParameter(delegateType)`) | a delegate whose type mentions a type parameter (`Func<T>`, `Action<ж<T>>`) has no twin delegate, so inside a generic body a callback is called plain and holds its carrier | twin delegates and pair classes generic over the enclosing method's type parameters: `go2cs.twinpairs.g.cs` today emits one non-generic class per delegate shape. **This is the consumer a conc-shaped module needs**: a generic pool or iterator that calls its `func(T)` argument parks only through the callback |
| C-3 | method-group pairing, `TwinGenerator.Rewriter.cs:1563` and `:1597` (`IsGenericMethod: false`) | a generic method passed as a func value is not paired | pair the constructed method; it depends on C-2's generic pair classes |

Not consumers of E1: `TwinGenerator.Emit.cs:338` (the entry point is never generic), and `TwinGenerator.Rewriter.cs:192-216`
(type-name spelling, which already handles generic hosts).

## 4. Counts (step 1)

Rows over the packages each pattern matches (tests excluded). linux/amd64, `CGO_ENABLED=1`, go1.26.11. The default
does not follow calls into package `runtime` and `internal/runtime/...` (section 6). The `-runtime` rows follow them.

| module | funcs | parkers | generic | generic exported | parks: direct | parks: transitive | parkers exported | no park: dynamic calls | no park: static | parks, no locks | parks, no locks, exported | exported generic FUNCS | of them parking | of them parking, no locks |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| std (default) | 18,867 | 5,158 | 303 | 139 | 9 | 37 | 22 | 79 | 178 | 1 | 0 | 109 | 8 | 0 |
| std (`-runtime`) | 18,867 | 5,450 | 303 | 139 | 9 | 40 | 24 | 79 | 175 | 22 | 9 | 109 | 9 | 6 |
| golang.org/x/sync v0.24.0 | 18 | 14 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| github.com/sourcegraph/conc v0.3.0 | not read: awaiting the owner's yes to fetch | | | | | | | | | | | | | |

**The stdlib's 8 parking exported generic functions (default reading), all locks only:**

| function | the lock it reaches | C# home |
|---|---|---|
| `sync.OnceValue`, `sync.OnceValues` | `sync.(*Once).Do`'s mutex | `sync/oncefunc.cs`, hand-owned whole file: outside the population |
| `reflect.TypeAssert` | `reflect.makeMethodValue` → `sync.(*Map).Load` → `internal/sync` HashTrieMap's mutex | `reflect/value_impl.cs`, a hand-owned companion |
| `unique.Make` | `internal/sync.(*HashTrieMap).Load` → its mutex | `unique/handle.cs`, converted |
| `crypto/internal/fips140/ecdh.GenerateKey`, `crypto/internal/fips140/ecdsa.GenerateKey`, `crypto/internal/fips140/ecdsa.Sign` | `drbg.ReadWithReader` → `drbg.Read` → `sync.(*Pool).Get`'s slow-path mutex | `ecdh.cs`, `ecdsa.cs`, converted |
| `internal/trace.MakeEvent` | `internal/trace.Stack.Frames` → `fmt.Sprintf` → `sync.(*Pool).Get`'s slow-path mutex | `internal/trace/event.cs`, converted |

The other 14 of the 22 exported parkers are methods of generic types, so none reaches the population as an E1
member: 11 `internal/sync.(*HashTrieMap)` methods (14 of its methods park in all, every one through its mutex, in a
hand-owned whole file, `internal/sync/hashtriemap.cs`), `crypto/internal/fips140cache.(*Cache).Get` (`sync.(*Map).Load`),
and `database/sql.(*Null).Scan` and `Null.Value` (first paths found: `Scan` through `convertAssignRows` to
`(*Rows).close`'s RWMutex, `Value` through `reflect`'s method-value cache, a `sync.Map`). The one generic that
parks without locks is `net.doBlockingWithCtx` (unexported; its `go` statement). It is emitted only in the cgo resolver
file `net/darwin/cgo_unix.cs`.

**The dynamic bucket.** 79 stdlib generics have no static path to a park but call something the census cannot
resolve: a func value, an interface method, or a method of a type parameter's constraint. These are the members whose
park, if any, arrives through C-2 or I-1 rather than through E1-b.

**What the `-runtime` rows add.** Following the runtime adds 3 stdlib generic parkers and, without locks, 21. Every
added path enters runtime machinery converted code never calls: allocation reaching `runtime.gcStart` and
`gcBgMarkStartWorkers`' `go` statement, `os.newFile` reaching `runtime.SetFinalizer` and `createfing`, and
`runtime.AddCleanup` reaching `(*cleanupQueue).createGs`. Read with `-why`, for example
`crypto/x509.checkConstraints -> fmt.Errorf -> … -> reflect.valueMethodName -> runtime.(*Frames).Next -> … ->
runtime.mallocgc -> runtime.gcStart -> runtime.gcBgMarkStartWorkers -> go statement`.

## 5. The reading for G

- **The stdlib does not need the lift.** No converted exported generic function parks outside a lock, and the
  generator already treats a lock as not making its caller a twin (N1: Mutex.Lock's twin is weak, RWMutex has none).
  The forwarder half (E1-b to E1-e) is small and mechanical, and it buys population completeness, not carriers.
- **If conc reads "mostly E1", the work is C-2 (and C-3), not E1-b.** A generic helper that parks only by calling a
  `func(T)` argument is twinned today only if the generator can see the park, and it cannot through an open generic
  delegate. The census's dynamic bucket is where that population shows up. The conc row (pending) splits it.
- **I-1 has no stdlib population that parks.** Its four generic interfaces are self-referential constraints over
  curve and value types.

## 6. Method and limits

- **The predicate.** A function parks DIRECTLY when its body, function literals included, holds a channel send or
  receive, a select, a range over a channel, a go statement, or a call to a wait primitive. The primitives are
  `sync.(*Mutex).Lock`, `sync.(*RWMutex).Lock`/`RLock`, `sync.(*WaitGroup).Wait`, `sync.(*Cond).Wait`,
  `internal/sync.(*Mutex).Lock` and `time.Sleep`; the census stops at them, because the bodies below reach the runtime
  through bodyless linkname declarations. A function parks TRANSITIVELY when a static callee parks, to a fixpoint over
  every declaration with a body in the load.
- **Two readings.** COORD's predicate counts every primitive. The generator-aligned reading does not count the four
  lock primitives.
- **Not followed.** Calls through func values, interface methods, methods of type parameters, and fields of func
  type are counted in the dynamic bucket. Bodyless functions (assembly, linkname) park only if they are primitives.
- **The runtime boundary.** By default the census does not follow calls into `runtime` or `internal/runtime/...`;
  golib stands in for that machinery in converted code. `-runtime` crosses it.
- **Go-side, not C#-side.** The census counts Go declarations. The C# population's two further exclusions, hand-owned
  files and the glyph-body mapping of generic-type methods, are applied above by reading, member by member.
- **Platform.** linux/amd64 with `CGO_ENABLED=1`. A file selected only on another GOOS is not in these rows.
- **Cross-check.** A grep of the same 1,680 std files gives 217 generic functions (`^func Name[`) and 86 methods of
  generic types (`^func (recv[...]) `): 303 together, the census's `generic`. The function count by grep (18,776
  single-line signatures) is within 0.5% of the census's 18,867.

**Controls** (`TestE1CensusControls`, over `testdata/fixture`). Each named function must also be SEEN, so no control
passes on an empty load:

| fixture function | counted (COORD's predicate) | counted, no locks |
|---|---|---|
| `NonGenericParker` (direct chan send, not generic) | no | |
| `GenericParker[T]` (parks only through `sendHelper`) | yes | yes |
| `(*Box[T]).Get` (a generic-type method, `sync.Mutex.Lock`) | yes | no |
| `GenericQuiet[T]` (only calls a non-parking function) | no | |
| `GenericDynamic[T]` (only calls a func value) | no (dynamic bucket) | |
| `GenericFinalizer[T]` (parks only inside the runtime) | no; yes with `-runtime` | |

The test also asserts the fixture's row (5 generic; direct 1, transitive 1, dynamic 1, static 2) and its exported
generic functions (4; parking 1; parking without locks 1).

**Red first.** Seven predicate breaks, each run against the test and then restored byte-identically (SHA-256 checked):

| # | break | the control that failed |
|---|---|---|
| 1 | the transitive step removed | `GenericParker: counted=false, want true` |
| 2 | the generic filter widened to every function | `NonGenericParker: counted=true, want false` |
| 3 | every static call made a park | `GenericQuiet: counted=true, want false` |
| 4 | locks counted in the no-locks reading | `(*Box).Get: counted without locks` |
| 5 | the runtime boundary never applied | `GenericFinalizer: counted=true, want false` |
| 6 | the runtime boundary applied under `-runtime` | `GenericFinalizer: not counted with the runtime followed` |
| 7 | methods counted as exported generic functions | `exported generic funcs: 5, parkers 2 …; want 4, 1, 1` |

## 7. Reproduce

From `src/go2cs`, with the Go toolchain at the corpus pin:

```
go run ./internal/e1census -label std -list std
go run ./internal/e1census -runtime -label std -list std
GOFLAGS=-mod=mod GOPROXY=off go run ./internal/e1census -label x/sync -list -dir <module cache>/golang.org/x/sync@v0.24.0 ./...
go run ./internal/e1census -why crypto/x509.checkConstraints std
go test -count=1 ./internal/e1census
```

## Amendment 2026-11-27: the wait-twin reach census (COORD, from G)

G settled the conc question above: conc's gap is the hand-owned `sync.WaitGroup.Wait`, not E1. This block sizes the
payoff of a hand-written `sync.WaitGroup.Waitꞏtwin` and `sync.Cond.Waitꞏtwin` with the same tool (`-wait`,
`internal/e1census/wait.go`). Every function is classified by the kinds of park point its static closure reaches: W
(`WaitGroup.Wait`), C (`Cond.Wait`), L (a mutex or RWMutex lock) and O (a channel op, a select, a range over a channel,
a go statement, `time.Sleep`). It reads W when it reaches W and nothing else, C likewise, and WC when it reaches only
both. These are the functions those twins would newly twin.

Two readings. In the LITERAL one a lock is another park point. The LOCKS-ASIDE one sets locks aside, as the generator
does (a lock never makes its caller a twin). Every `Cond.Wait` caller holds its lock, so only the second reading can see
a `Cond.Waitꞏtwin`'s reach.

| module (version) | funcs | call WaitGroup.Wait / Cond.Wait directly | literal W / C / WC | locks-aside W / C / WC | exported among locks-aside W, C |
|---|---:|---:|---:|---:|---:|
| std (go1.26.11) | 18,867 | 14 / 5 | 0 / 0 / 0 | 1 / 6 / 0 | 1, 0 |
| golang.org/x/sync v0.24.0 | 18 | 2 / 0 | 1 / 0 / 0 | 1 / 0 / 0 | 1, 0 |
| github.com/sourcegraph/conc v0.3.0 | 81 | 2 / 0 | 2 / 0 / 0 | 8 / 0 / 0 | 8, 0 |
| github.com/cskr/pubsub/v2 v2.0.2 | 19 | 0 / 0 | 0 / 0 / 0 | 0 / 0 / 0 | 0, 0 |
| github.com/patrickmn/go-cache v2.1.0+incompatible | 75 | 0 / 0 | 0 / 0 / 0 | 0 / 0 / 0 | 0, 0 |

**The functions** (locks-aside; callers = distinct static callers across the load, so an entry point's own callers
outside the module read 0):
- std: W `net/http.(*Server).Close`. C `net/http.(*connReader).abortPendingRead` (3 callers), `(*http2pipe).Read` (2),
  `(*response).finishRequest` (2), `(*conn).hijackLocked` (1), `(*response).sendExpectationFailed` (1),
  `(*response).Hijack` (0). Every other direct caller of either wait in std also reaches an O park point (a channel op, a
  select, a range over a channel, a go statement or `time.Sleep`); that is what keeps it out of W, C and WC with locks
  aside.
- x/sync: W `errgroup.(*Group).Wait`, the same in both readings.
- conc: W `conc.(*WaitGroup).Wait` (3 callers) and `WaitAndRecover`, both W in the literal reading too. With locks
  aside, also `pool.(*Pool).Wait` (3), `(*ErrorPool).Wait` (2), `(*ContextPool).Wait` (1), and `(*ResultPool).Wait`,
  `(*ResultErrorPool).Wait`, `(*ResultContextPool).Wait`; 3 of the 8 are generic.
- pubsub and go-cache: none. Every park point they reach is a channel op, a select, a go statement or a lock.

**Controls** (`TestWaitTwinControls`):
- a function that parks through a channel is not W;
- `WaitOnly`, and `WaitViaHelper` one call down, are W;
- `CondOnly` is C, with its `c.L` an interface so its Lock is dynamic;
- `WaitAndSend` is neither;
- the converter module's own `golang.org/x/sync/errgroup.(*Group).Wait` is W, since its `g.cancel` is a func-field call;
- `CondLocked` and `WaitLocked` (a concrete `*sync.Mutex` held) are neither in the literal reading and C and W in the
  locks-aside one.

Red first: six breaks, each failing its own control, and every restore was byte-identical:
1. a channel op counted as W;
2. `WaitGroup.Wait` counted as other;
3. `Cond.Wait` counted as other;
4. a dynamic call counted as a park;
5. kinds not carried through calls;
6. locks not set aside in the second reading.

**The modules** came from the Go module proxy (`proxy.golang.org`), with every hash checked against `sum.golang.org`.
`go mod verify` reported all modules verified (owner's ruling 2026-11-27, relayed by COORD). x/sync is the converter
module's own requirement, the same version as P6's pin.
