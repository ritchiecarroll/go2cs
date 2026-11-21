# DESIGN — M:N: the twin-set rule as text, with the closed definition of "signature admits a twin"

> **STATUS: PROPOSED (lane G, 2026-11-20; branch `claude/g-mn-proto-b1`).** Design only: the rule text to freeze,
> the decisions it asks, and the census that gates it. No code lands from this document. It answers item 3 of the
> re-sized M:N plan (record 10.4's SHOULD-1, written last per 10.9) on top of the pin design
> ([DESIGN-mn-pin](DESIGN-mn-pin.md)), N2 ([DESIGN-mn-box-twin](DESIGN-mn-box-twin.md)) and stage (c)
> ([DATA-mn-stage-c](DATA-mn-stage-c.md)). Amended by dated blocks, never rewritten.
>
> **2026-11-20: P3 CLEARED section 7 for build; COORD approved the asks of section 10; section 11 carries the text
> changes owed before the freeze.** The build waits on the owner's M:N landing shape.

## 1. What the text must hold

From [DATA-mn-spike-2](DATA-mn-spike-2.md), as ruled:

- **M1**: "the twin surface of a package is a function of its exported Go API and never of the park analysis."
  Every exported function and method whose signature admits a twin has one, always; where the body cannot park it
  is a forwarder with a mark. Refusals that read the BODY give a forwarder, not an absence.
- **10.3**: the SURFACE and the PARK SET are two sets; only the surface is frozen. The park mark is data beside it.
- **10.4**: the definition is a closed list of exclusions, **each stated on the signature alone**, with one rule:
  the list can only get shorter (lifting adds twins, which is safe; narrowing removes them, which is M1's break).
  Generic functions are decided before the freeze.
- **10.8**: for a hand-owned file the generator emits the forwarder; an author-written twin wins.

## 2. The finding that shapes the text: three lowerings

The generator decides on the **C#** signature. The C# signature of a converted Go function is not a function of its
Go signature alone. Three lowerings move it, read on this branch:

| Go form | plain C# lowering | decided by | today's refusal |
|---|---|---|---|
| `*T` receiver | `this ref T`, or `this ж<T>` where the body needs the pointer itself (a nil check, an escape); RecvGenerator adds the `ж<T>` overload beside a `ref` form | the BODY (`bytes.Buffer.Bytes` is `this ref Buffer`, `Buffer.String` is `this ж<Buffer>`) | `byref-recv` (the box twin lifts it, N2) |
| `string` parameter | an `sstring` member (a `ref struct` view) carrying the body, beside a generated `@string` member at `[OverloadResolutionPriority(-1)]` | an EXPLICIT converter list, the sstring-twin pilot (`sstringTwinOperations.go`); `fmt.Printf` is on it | `reflike`, on the `sstring` member |
| `...T` parameter | `params ꓸꓸꓸT` = `params Span<T>` (`params Span<T>` for a generic) | always | `reflike` |

A `*T` PARAMETER of an exported function is always `ж<T>`: the ref-lowering pass lowers only unexported functions
(DESIGN-mn-box-twin 12), which R6 leaves to the analysis anyway.

The first row flips with a body edit (a Go patch that adds `if b == nil` turns `this ref T` into `this ж<T>`), and
the second with a converter list that can change between releases. A definition read off the C# signature would
let either add or remove a twin, which is exactly what M1 forbids. The third refuses **every exported variadic
function**: `fmt.Printf`, `fmt.Fprintf`, `log.Printf`, `errors.Join`, `filepath.Join` have no twin today, by
signature.

Source read of the converted (not hand-owned, not golib, not test) public methods on this branch, one-line
declarations only, so each is a floor (positive control `fmt.Printf` counted, negative `strings.Contains` not):

| lowering on a public method | count (of 7,653 read) |
|---|---|
| `this ref T` receiver | 1,832 (beside 1,598 `this ж<T>`) |
| variadic `params Span` | 217 |
| `ref T` parameter | 0 from a Go declaration (the regex's 24: bodies on the same line, synthesized native helpers, hand-written helpers) |
| `sstring` parameter | 9: the pilot's `sstring` members, each beside a generated `@string` member |
| generic method | 132 (1 of them variadic) |
| `unsafe` modifier (a body property) | 124 |
| a C# pointer type in the signature | 20, all converter-SYNTHESIZED native helpers (`GoWriteNativeSockaddrInet4`), no Go declaration |

**So the text is stated on the Go signature, and the twin's own C# signature is a function of the Go signature.**

## 3. The rule (the text to freeze)

> **R1. Population.** The twin set of a package built with twins is every Go function and method whose declaration
> the conversion emits as a public C# member: package functions, methods (on any receiver the conversion makes
> public), promoted-method forwarders, and the members of every public interface. One Go declaration is ONE
> member of the population however many C# overloads it has (a `ref`/`ж` receiver pair, an `sstring`/`@string`
> pair). A member the converter synthesizes with no Go declaration (native helpers, adapters) is not in it.
>
> **R2. One twin, Go-shaped.** Each member of the population has exactly one twin, `<name>ꞏtwin`, whose parameters
> are the **canonical lowering** of its Go parameters (section 4) and whose result is `GoTask` for no result,
> `GoTask<T>` for one, `GoTask<(T1, …)>` for several. The twin's signature depends on the Go signature only.
>
> **R3. Body.** The twin is a state machine where the generator can rewrite the body for the park set, and
> otherwise a **forwarder**: it calls the plain member, converting each canonical parameter to the plain lowering,
> and returns a completed task. A forwarder carries `[GoParkForwarder]` (golib's `GoTwinForwarder` name is taken,
> by the sstring twin's value delegate), and a caller's build may bind the plain
> member through it. Every refusal that reads the body, the plain lowering or the park analysis produces a
> forwarder. None produces an absence.
>
> **R4. Exclusions.** A member is outside the twin set only by an entry of the closed list E (section 5). Each
> entry is stated on the Go signature alone. E can only get shorter.
>
> **R5. Hand-owned.** For a hand-owned member that stands for a Go declaration the generator emits the forwarder
> (10.8). An author-written `<name>ꞏtwin` replaces it; the generator then emits nothing for that member.
>
> **R6. Unexported.** Unexported members stay analysis-driven: twinned where the park set needs them, never frozen.
>
> **R7. Interfaces.** Every public interface has a hidden twin interface with one slot per member in the twin set,
> and every implementer the conversion records implements it (a forwarder where the implementer does not park).
> Whether a call through the interface AWAITS the slot is the park mark of 10.3, data beside the surface.
>
> **Contract sentence.** Within a Go line (from the first release that carries twins), a twin member never
> disappears and never changes shape; E only gets shorter.

## 4. The canonical lowering

The twin takes each parameter in the form that holds across an await and that the plain lowering converts from:

| Go parameter | plain lowering (any of) | canonical in the twin | forwarder's conversion |
|---|---|---|---|
| `*T` receiver | `this ref T`, `this ж<T>` | `this ж<T>` | `ref Ꮡt.DerefOrNull()` (N2's prologue) |
| `*T` parameter (exported) | `ж<T>` | `ж<T>` | none |
| `string` | `@string`; for the pilot list also `sstring` | `@string` | none: it calls the `@string` member |
| `...T` | `params Span<T>` | `slice<T>` (Go's own view: `...T` is a `[]T` in the body) | `args.ꓸꓸꓸ` (shares storage, as Go's `f(s...)`) |
| value receiver `T` | `this T`; a `[GoCopyBound]` `this ref T` forwarder | `T` | none |
| any other type | unchanged | unchanged | none |

Results: Go results always escape, so they are never lowered; the twin's result types are the plain ones.

A nil receiver through the `*T` row keeps Go's behaviour: `DerefOrNull` yields a null ref that faults at the first
dereference, as the plain method does, so a method that never dereferences its receiver runs (DESIGN-mn-box-twin
section 4, item 6).

## 5. The closed list E, decided

| entry | stated on the Go signature | decision asked | why |
|---|---|---|---|
| E1 | a generic function, a method of a generic type, or a member of a generic interface | EXCLUDE at the freeze; lifting is owed and additive | no generic arm was built (4.10); the generator refuses `generic-interface`, `generic-method`, `generic-host`, and leaves `generic-unfollowed` box calls plain. A forwarder is easy; a real generic twin is an arm. Source read: 132 public generic methods; few park (census, P2) |

Nothing else. Each remaining generator refusal maps to R3 (a forwarder), to a corpus decision, or to a fix, as
section 6 shows. In particular **variadic functions are in** (canonical `slice<T>`): excluding them would put
`fmt.Fprintf` and `log.Printf` outside the surface for a whole Go line, and a forwarder costs one method.

## 6. Every generator refusal, mapped

| generator reason | reads | under the rule |
|---|---|---|
| `byref-recv` | the plain lowering (the body chose `this ref T`) | box twin where N2 built it; else a canonical forwarder (R3) |
| `byref-param`, `byref-mixed`, `byref-out` | the ref-lowering of UNEXPORTED functions, and hand-owned `out` | outside the frozen surface (R6); an `out` in a hand-owned file is R5's author decision |
| `reflike` | the plain lowering (the pilot's `sstring` member, variadic `Span`) | `sstring`: the twin hangs on the `@string` member (R1), a forwarder to it until the rewriter twins the body; variadic: a canonical forwarder; a real twin needs the rewriter to retype the parameter (owed, park-set work) |
| `unsafe` | the `unsafe` modifier (body); pointer types only on synthesized members | forwarder; synthesized members are outside R1 |
| `native`, `iterator`, `await-in-lock`, `ref-unrederivable`, `ref-shared-member`, refuse rows | the body | forwarder |
| `host-not-partial` | the hand-owned file, not the signature | **decision asked**: make those hosts `partial` (stage (c) counted 94 methods and 2 interface members on the closure); otherwise they need a file-level exclusion, which the text cannot state on a signature |
| `async-shaped` | a result type NAMED `Task`, `ValueTask` or `GoTask` | **fix before the freeze**: key it on the BCL and golib symbols. Keyed on a name it excludes a Go type that happens to be called `Task`, a signature exclusion nobody chose. Narrowing it adds twins |
| `instance`, `interface-member`, `explicit-impl`, `ref-return`, iface `static` | C# shapes no Go declaration converts to | outside R1 (hand-written C# only); the census asserts zero in converted files |
| iface `byref-param` for `params` | a variadic interface member | canonical `slice<T>` slot (generator gap to close) |
| `generic-*` | the Go signature | E1 |
| `has-twin` | an author-written twin | R5 |

## 7. What the freeze needs built (item 7a's work, priced here)

| piece | lane-days |
|---|---|
| canonical forwarders: the three lowerings, body refusals emitting a forwarder instead of nothing, `[GoParkForwarder]` and the caller's plain bind | 2-3 |
| variadic interface slots; `async-shaped` keyed on symbols; E1 as one named rule | 0.5 |
| the guard (section 8) | 0.5-1 |
| the census arms (section 9) | 0.5-1 |
| **total** | **3.5-5.5**, beside this text's 1-1.5 |

Hosts made `partial` is a hand-owned corpus edit for its owners, not counted here. Whether the published stdlib
packages are built with twins at all is the owner's open landing shape (L-min opt-in twins vs L-on); the rule is
the same either way, and applies to a package when it is built with twins.

## 8. The guard

A generator check over the compilation: every member in R1's population is twinned, forwarded, or excluded by an
E entry; any other outcome is a build error naming the member and the reason (a new GO2CS diagnostic). A GenTests
unit per row of section 6. Made to fail before it is trusted (floor 13): drop the forwarder for one body refusal
and the guard must name that member; key `async-shaped` on the name again and the guard must name a Go type called
`Task`. The binary-compatibility guard already compares every public member, so it covers twin members (M1).

## 9. Census arms and predictions (written before any arm runs)

Arms: MnHttpClosure (stage (c)'s closure, arm B's rule) and the whole stdlib conversion, each with a report row
per public member: `twin | forwarder:<reason> | E1 | GUARD`.

| # | prediction | stop line |
|---|---|---|
| P1 | rows reading GUARD after section 7's build: 0 in converted files | any: the rule misses a case; the text waits |
| P2 | E1 on the stdlib: 130-330 public generic members; of them parkers (the generator's analysis, arm B) under 25 | over 50 parkers: E1 needs a generic arm before the freeze, a ruling |
| P3 | forwarders on the closure: 60-75% of R1's population (the park set is 22.2% of candidates in arm B) | none: a reading |
| P4 | closure go.*.dll bytes over arm B's +6.1%: a further +2% to +5% | over +10%: a ruling (for example, forwarders only where a park is possible in any Go version) |
| P5 | build wall over arm B: within 1.2x | over 1.5x: a ruling |
| P6 | variadic members gain twins or forwarders: 217 or more converted public methods (the source floor) | fewer than 217 |
| P7 | `instance`, `interface-member`, `explicit-impl`, `ref-return` on converted public members: 0 | any |

## 10. Asked of COORD

1. Rule R1-R7 and the canonical lowering (section 4) as the twin-set text.
2. E = {E1} at the freeze: generics excluded, lift owed and additive.
3. `host-not-partial`: the hosts are made `partial` (recommended), or a file-level exclusion is accepted outside
   the signature rule.
4. `async-shaped` keyed on symbols before the freeze.
5. Section 7's build as item 7a's scope, and section 9's stop lines as written.

## 11. Before the freeze: P3's read and COORD's ruling (dated block, 2026-11-20)

P3 read `e508700c5a` read-only, with P2's wall E at `claude/p2-wall-e-spread-alias` `4c16756345`: CLEAR to build
section 7. Four changes to the TEXT are owed before it freezes, plus two nits; none changes the build. COORD answered
section 10: all five asks are yes. The hosts are made `partial`, `async-shaped` is keyed on symbols, and
E = {E1}. Where this block and sections 3-9 differ, this block holds.

### 11.1 R1 and R6 read one predicate: C# accessibility (amends R1, R6)

R1 said "emitted as a public C# member" and R6 said "unexported", and the two disagree both ways. A linkname-opened
unexported function is public in C# (`linknameOperations.go`), so R1 froze it while R6 said it never freezes. An
exported method the converter narrows to `internal` (its receiver type is unexported) fell outside R1 and was not
"unexported" either.

> **R1 (amended).** The predicate is C# accessibility: the population is every Go declaration whose plain member the
> conversion emits as `public`, whatever its Go spelling.
>
> **R6 (amended).** Members the conversion does NOT emit as public stay analysis-driven: twinned where the park set
> needs them, never frozen.

### 11.2 R7 names the call helper and the fallback (amends R7)

The hidden twin interface freezes more than its slots. A caller in another assembly binds the static helper
`Iꞏtwin.Mꞏcall(I recv, …)` (`TwinGenerator.IfaceEmit.cs`), found by name (`TwinGenerator.Iface.cs`).

> **R7 (amended).** Every public interface `I` has its hidden twin interface `Iꞏtwin`, with one slot `Mꞏtwin` per
> member in the twin set and, beside each slot, the static helper `Mꞏcall(I recv, …)` that callers bind across
> assemblies. When `recv` implements `Iꞏtwin`, the helper returns its slot's task. Otherwise it calls the plain
> member and returns a completed task. Under opt-in (11.3) that fallback is the common case: an implementer in an
> assembly built without twins. Helper and slot freeze together.

### 11.3 The opt-in rule (new section of the text)

R1 is the set GIVEN that an assembly is built with twins. Opt-in needs its own rule. It holds whichever M:N shape
the owner picks (L-min or L-on).

> **O1. The unit is the assembly.** `GoTwinsFor` selects projects, so an assembly is built with twins or without
> them.
>
> **O2. No partial sets.** An assembly built with twins carries all of R1 in the one shape. The spike's switches
> that change which twins an opted-in assembly carries (`GoTwinsIface`, `GoTwinsIfaceAll`, `GoTwinsGo`,
> `GoTwinsRefuse`) do not ship. A build that sets one with twins on is refused by the guard (section 8), naming
> the property.
>
> **O3. Direction.** An assembly that has shipped twins keeps shipping them, and one package identity has one
> variant (never a with-twins and a without-twins build under the same identity and version). The other direction
> is safe: a caller bound plain keeps working when its dependency gains twins.

The contract's clock is "the first release that carries twins". Under L-min no stdlib package starts that clock in
.5, so O3 binds a stdlib package from the first release that builds it with twins.

### 11.4 P8, the invariance arm (new; section 9)

Nothing tested section 2's central claim, that the twin's signature does not move with the lowerings.

| # | prediction | stop line |
|---|---|---|
| P8 | rebuild one package twice, (a) with the sstring pilot list emptied, and (b) with one `*T` receiver flipped from `this ref T` to `this ж<T>` by a body edit (an added nil check); the twin members' metadata (names, parameter and result types, attributes) is identical to the unmodified build in both | any difference |

Made to fail first: in a scratch generator build, derive one twin's parameter from the plain lowering instead of
the canonical one; P8 must then name that member under (b). Then restore the generator, byte-identical.

### 11.5 Nits

- **R5.** The guard checks an author-written twin's signature against the canonical lowering. A hand-written
  `Printfꞏtwin(@string, params Span<any>)` would otherwise pass as twinned while breaking R2.
- **The contract sentence**, amended: "a twin member never disappears, and **changes shape only where its plain
  member does**". Generated lift names are ruled not stable, and a twin whose signature carries one moves with its
  plain member.

### 11.6 Wall E (P2's spread alias), as P3 read it

R2's `...T` to `slice<T>` holds under wall E. There, `slice<T>.ꓸꓸꓸ` builds a spread pack that carries the
caller's header, and the plain body's `.slice()` returns that header. So section 4's forwarder is exact: a spread
call shares storage, and a literal call arrives as the fresh slice the twin's caller built. The variadic interface
slot holds likewise, and its `ꞏcall` fallback passes `.ꓸꓸꓸ` the same way. Two things to carry:

- Wall E removes the slice-to-pack conversion on purpose (`variadicPack.cs`). A forwarder that passes `args`
  WITHOUT `.ꓸꓸꓸ` to a `...any` member compiles and passes ONE element, the slice itself. Section 8 gains a
  GenTests row: a `...any` forwarder and a `...any` `ꞏcall` fallback each deliver every element, made to fail by
  dropping the `.ꓸꓸꓸ`.
- Sections 2 and 4 say `params Span<T>`. After wall E the plain lowering is `params variadic<T>`, still ref-like.
  The generator's variadic test keys on the parameter being `params` with a ref-like type, not on `Span<T>` by
  name: the same symbols-not-names rule COORD ruled for `async-shaped`.

## 12. 7a as cut: scope, shapes and predictions, before any code (dated block, 2026-11-21)

Row 2 of the ruled .6 plan: the surface build behind sections 3-6 and 11. It changes which PUBLIC members of an
opted-in assembly get a twin, and in what shape. It changes nothing for an assembly built without twins.

### 12.1 Scope

- **The population (R1, as built):** every public method declared in a CONVERTED file (the generator's existing
  predicate: not a `*_impl.cs` companion, not `[module: GoManualConversion]`, not a metadata file, not generated
  output) of an assembly that is not `[GoHandOwnedPackage]`, outside E1. Hand-owned members (R5) are NOT in 7a's
  guard: they need a per-package list of the Go declarations they stand for, and only the stdlib has them in number,
  so they move with row 7 (the stdlib-twins arm). Named, not dropped.
- **Shapes, as section 4:** a pointer receiver's twin takes `this ж<T>`; an sstring-pilot function's twin hangs on its
  `@string` member; `...T` becomes `slice<T>`, and a forwarder passes `args.ꓸꓸꓸ` (11.6).
- **Forwarders (R3):** every member of the population that the generator does not twin as a state machine (it cannot
  park, a body refusal, or the plain lowering refused) gets `[GoParkForwarder] Mꞏtwin(<canonical>)`. Its body calls
  the plain member and returns a completed task (`default`, or `new GoTask<R>(...)`). `GoParkForwarderAttribute`
  is new in golib and goes into `[mn-frozen]`. A caller's twin binds the PLAIN member when the callee's twin
  carries the mark, so a forwarder is never awaited.
- **Interfaces (R7):** the hidden twin interface gets a slot for every member in the population, variadic members
  included (canonical `slice<T>`); the `ꞏcall` fallback passes `.ꓸꓸꓸ`.
- **async-shaped** keys on the symbols (`System.Threading.Tasks.Task`, `ValueTask`, `go.golib.GoTask`), not the name.
- **E1:** one named rule: a generic method, a method of a generic type, or a member of a generic interface.
- **The guard, GO2CS0017 (Error):** a member of the population with no twin, no forwarder, and no E1 exclusion.
  **GO2CS0018 (Error), O2:** a partial-set switch (`GoTwinsIface`, `GoTwinsIfaceAll`, `GoTwinsGo`, `GoTwinsRefuse`)
  set in a build with twins on. The prototype's own arms set `GoTwinsArm=true`, which this refusal admits. That
  property is an arm marker, and the docs never mention it.

### 12.2 GenTests rows, each red first

| row | red before the code | green after |
|---|---|---|
| F1 a public function that cannot park | no `ꞏtwin` member | a `[GoParkForwarder]` twin returning a completed task |
| F2 a public method refused by its body (`unsafe` modifier, `native`) | absent | a forwarder |
| F3 a pointer receiver whose box twin is refused | absent | a forwarder taking `this ж<T>`, calling through `DerefOrNull` |
| F4 a variadic `...any` function | refused `reflike` | a forwarder taking `slice<object>` and delivering EVERY element (dropping `.ꓸꓸꓸ` makes it fail: one element arrives) |
| F5 an sstring-pilot pair | refused on the `sstring` member | one twin, on the `@string` member |
| F6 a caller of F1 | (n/a before) | calls the plain member, no await |
| F7 a Go type named `Task` | refused `async-shaped` | twinned or forwarded |
| F8 a generic function | refused under several names | refused as E1, by name |
| F9 the guard | (no guard) | a population member with its forwarder suppressed (a test seam) fails GO2CS0017, naming it |
| F10 O2 | (no refusal) | GoTwinsIface set with twins on fails GO2CS0018; with GoTwinsArm=true it builds |
| F11 a variadic interface member | no slot | a `slice<T>` slot; `ꞏcall` delivers every element |
| P8 invariance (11.4) | (no check) | the sstring list emptied, and one receiver flipped from ref to ж: the twin members' metadata is identical |

### 12.3 Predictions on the programs and the closure

| # | prediction | stop line |
|---|---|---|
| Q1 | the 20 twin builds of the 7b kit stay green; each twin run prints its plain output | any difference |
| Q2 | MnHttpClosure, arm B's rule: GO2CS0017 reports 0 in converted files | any (section 9's P1) |
| Q3 | forwarders are 60-75% of the population on the closure (section 9's P3) | none: a reading |
| Q4 | the closure's go.*.dll bytes: a further +2% to +5% over arm B (P4) | over +10%: a ruling |
| Q5 | the build wall: within 1.2x of arm B (P5) | over 1.5x: a ruling |
| Q6 | P7: `instance`, `interface-member`, `explicit-impl`, `ref-return` on converted public members: 0 | any |
| Q7 | GenTests: every row of 12.2 green, with the 693 already there unchanged | any red that was green |

Size: 3.5-5.5 lane-days, as section 7 priced it. Order: `GoParkForwarderAttribute` and its `[mn-frozen]` lines; F1,
F2, F6; F3, F5; F4, F11; F7, F8; the guard (F9) and O2 (F10); P8; then the closure arms.

### 12.4 Readings, step 1 (dated block, 2026-11-20)

- **Step 1 built:** `GoParkForwarderAttribute` (golib, `[mn-frozen]`: its ctor and a SHAPE line that now records
  `AttributeUsage`; made to fail by setting `AllowMultiple = true`, restored byte-identical). F1, F2 and both F6 rows
  went red before the code (the same-assembly F6 was green before, as the table says) and are green after.
- **Q7 MISSED as written.** Four rows that were green went red:
  `AFunctionThatParksOnlyThroughAnUnboxedCallIsStillNamed`,
  `GoTwinsPairsOffEmitsTheCoreFreeTwins`, `AnAwaitInsideALockRefusesTheTwinByName` and
  `AnUnrederivableRefLiveAcrossAnAwaitRefusesTheTwinByName`. Each asserted that a PUBLIC function refused a twin (or
  parking nowhere) has no `ꞏtwin` member at all. R3 gives it a forwarder by design, so the prediction should have
  named them. Each row now asserts what it guards: exactly one `ꞏtwin`, a forwarder, with no state machine and no
  await of it (`TwinSurfaceTests.AssertForwarderOnly`). GenTests: 698 of 698 (693 + 5 new).
- **COORD's ruling on the four rows (2026-11-20): ACCEPTED**, on condition that each still guards its reason. Read
  at 57de4a48b2: each row keeps every assertion it had except the replaced "no `ꞏtwin`" line. The two "by name"
  rows still assert their report refusal row (`|locked|await-in-lock`, `|stale|ref-unrederivable`). They never
  asserted a Roslyn diagnostic: the refusal is named in the report, and a test for one would be a new row. The
  unboxed row still asserts `|sPark|v`. Pairs-off still asserts `callerꞏtwin(`, `mainꞏtwin(` and no `GoFuncPair`.
  The replacement is stronger than the line it replaced: exactly one line naming `Mꞏtwin(` also rules out an await
  of it. P3 reads these four rows at 7a READY.
- **repoguard at 57de4a48b2:** 13 provenance notes are red. They cite lines that the M:N delta moved on this branch,
  and they are item 13's landing re-cite job, not 7a's: manual-conversions.md:3830 (managed_impl.cs :5112, :5120),
  source-generators.md:485 (ImplementGenerator.cs:1608), ConversionStrategies.md:8718 (Goroutine.cs:1345) and :11879
  (managed_impl.cs:5129), DESIGN-goexit-main.md:45, 87, 88, 89 (Goroutine.cs :540, :1496, :1096, :1162),
  Limitations.md:101, 107, 115, 116 (Goroutine.cs :1345 twice, :1089, :1162). The two nuget suites were red only
  because the shell running them had no .NET on PATH.
- **Step 2, F3 and F5:** a forwarder gives a plain lowering its Go shape back. A pointer receiver (`this ref T`) takes
  its box, `Mꞏtwin(this ж<T> Ꮡt, ...)`, and calls `M(ref Ꮡt.DerefOrNull(), ...)`. An sstring-pilot parameter takes
  `@string`, and the call binds the body member through the implicit view. Both rows went red before the code.
  GenTests 700 of 700, with no earlier row changed.
