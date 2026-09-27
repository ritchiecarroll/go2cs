# Nil and Zero Values
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#nil-and-zero-values)
In Go, `nil` is the equivalent of C# `null`. Where possible, converted code uses the golib [`NilType`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/NilType.cs) with a default instance called `nil` (defined in [`go.builtin`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs)). `NilType` provides comparison operators so `x == nil` / `x != nil` work across the runtime types (slices, maps, channels, pointers, interfaces), each of which defines what "nil" means for it (e.g. a `map<K,V>` whose backing dictionary is null is the nil map: reads return the zero value, `len` is 0, ranging yields nothing, and a write panics — matching Go).

The same null-safe-zero-value principle applies to value types whose backing store is a reference. A zero-value `string` converts to `@string s = default!`, which runs no constructor, so the backing `byte[]` is null. Rather than [NRE](../Glossary.md#nre) on the first read, `@string` treats a null backing as Go's empty string `""` for every read — length 0, no bytes to index/range, `== ""` is true, prints empty, and concatenation yields the other operand (`var s string; s += "x"` → `"x"`). Constructors still allocate, so only the `default(@string)` zero value relies on this. (Guarded by the `StringZeroValueConcat` behavioral test.)

## A NARROW-UNSIGNED target folds a constant only when nothing else can make it compile

`uint32(1<<32 - 1)`, `*_C_pw_uidp(&sp) = 1<<32 - 2`. Go evaluates an untyped constant expression
at arbitrary precision and requires only the RESULT to fit the target; C# evaluates the operands
themselves, and an operand past `uint32` forces the literal path to emit a bare `long` — which has
no implicit conversion to `uint`/`ushort`/`byte`, so the assignment fails **CS0266** even though
the value fits exactly. The fix folds the whole expression and casts the result:

```csharp
//  Go:   *_C_pw_uidp(&sp) = 1<<32 - 2          (_C_uid_t = uint32)
_C_pw_uidp(Ꮡsp).Value = unchecked((uint32)(4294967294UL));
```

**The FIVE conditions, and why each exists.** This arm is deliberately the narrowest in the fold
family, because every widening of it damaged readable emission somewhere else in the corpus. It
applies only when ALL of:

1. **the target is unsigned and narrower than `uint64`** — the wider arms already handle their own;
2. **the target is a plain basic type or an ALIAS to one**, never a NAMED type — `basic` at that
   point is the UNDERLYING type, so `io/fs.FileMode` (a named `uint32`) arrives looking plain, and
   folding it erased the type on every mode expression in the corpus
   (`(fs.FileMode)(ModeDevice | ModeCharDevice)` → a bare `unchecked((uint32)(69206016UL))`). A
   named target keeps the arm below, which carries its type in the fold; an alias has no distinct
   C# type to lose;
3. **no NAMED CONSTANT is referenced** — those render through their `Untyped*` wrappers, which is
   how every wider arm preserves them, and folding replaced `math/bits`' `x>>1 & (m0 & m)` with
   `unchecked((uint32)(1431655765UL))`: arithmetic no reader can trace back to `m0`;
4. **an UNTYPED subexpression exceeds `uint32`** — a typed conversion carries its own width and
   emits correctly unaided, so `runtime`'s `^uint32(0)/8 + 1` never needed the fold, and counting
   it flattened the entire `class_to_divmagic` table into 68 casts;
5. **the threshold is `uint32`, not the target's own width** — `(1<<16) - 1` exceeds a `uint16` but
   the emission already carries an explicit `(ushort)` cast and compiles (measured), so
   `regexp/syntax`'s `Range16` keeps its source form; only a bare `long` has nothing to rescue it.

**How the conditions were found — the method, not just the result.** Each was exposed by a
three-target corpus regeneration, never by the two packages the fix was aimed at: the local darwin
build was green at every step. The site count fell **754 → 46 → 12 → 10 → 2** across six
regenerations, and the two survivors are exactly the expressions that cannot compile otherwise.
The lesson generalizes past this arm: *a converter change is measured against the corpus, not
against the file that motivated it* — a fold that looks obviously correct at its motivating site
can rewrite hundreds of unrelated ones, and only a full regeneration shows it.

Guarded by the `ConstSubexprOverflow` behavioral test, which already covered the construct
(`u32 := []uint32{1<<32 - 1}`); its golden re-baselined to the folded form with the Output phase
passing unchanged — the value never moved, only the spelling.

## The THREE deref accessors of `ж<T>` — when each is needed, and how the converter picks

Establishing a local `ref` over a heap box (`ref var p = ref Ꮡp.<accessor>`) looks like one
operation but encodes different answers to one question: **is this access the Go DEREFERENCE, and
what does Go say happens on nil at exactly this point?** Consolidated here because the members
landed across separate arcs (their individual sections, linked below, carry the full derivations);
this is the map.

| Accessor | On nil | The Go semantics it encodes | How the converter KNOWS |
|:--|:--|:--|:--|
| `.Value` | **panics immediately** (Go's message, even on bind) | this access IS the deref, and Go panics here — the ordinary pointer USE site (`*p`, `~Ꮡp`, a read through the box) | the DEFAULT everywhere except a pointer's ENTRY alias; no special case applies |
| `.ValueSlot` | **no check** — the slot as-is | a read of the HELD value, never a deref: when the pointee is itself reference-like, `*p` legally yields nil (`*(&err)` of a nil `error` panics in neither language), so `.Value`'s null check would fire SPURIOUSLY on a legally-held null. Identical to `.Value`'s slot in every non-throwing case. Also where nil is structurally impossible (a freshly `make`-allocated box, `heap(out …)`) and in the reflection bridge's field paths. | by the POINTEE'S TYPE or by CONSTRUCTION — a box-of-pointer LOCAL, a named-result box, the bridge's field walk. NOT at a pointer's entry alias (see below) |
| `.DerefOrNull()` | **defers** — binds `Unsafe.NullRef<T>`, faults with Go's panic on first USE | Go defers the panic to the body's own deref point: passing a nil `*T` to a function, or calling a method through one, is legal; the body RUNS, a side effect before the deref must happen, and the panic lands where Go's would — after it, or never (delegated `checkValid`-style guards). | STRUCTURALLY — EVERY direct-ж pointer ENTRY alias, RECEIVER and PARAMETER alike, unconditionally (no analysis, because the accessor is faithful whether or not the body guards), plus the pointer-reassignment re-alias and go2cs-gen's `ReceiverMethodTemplate` bridge; see *A nil RECEIVER is nil-deferring, not nil-safe* and *A pointer PARAMETER is nil-deferring for exactly the reason a receiver is* |

Why three and not one: the ENTRY alias and the USE site are different questions, and `.Value`
answers the second. `.ValueSlot` is different in KIND rather than in timing — it marks accesses
that were never dereferences in Go's semantics at all, which no nil-policy accessor can express —
but it is not selected at an entry alias, where nothing can know whether the body will dereference
and the nil-policy question is the only one being asked.

**There used to be a fourth, `.DerefOrNil()` — a nil-SAFE accessor handing back a shared
`default(T)` slot — and its retirement (2026-08-02) is what collapsed the set.** It was admitted
by a body ANALYSIS: a pointer param the body nil-compares, one passed the untyped `nil` at a
same-package call site, or one whose first mentioning statement re-points it without dereferencing
(`l = l.get()` normalization). Wherever that analysis was RIGHT the silent zero was unobservable;
wherever it was wrong — and it could never be complete, because a body's guard may be DELEGATED to
a callee it merely hands the pointer to — a deref Go says must panic instead read a silent zero.
Unifying every pointer entry alias on `.DerefOrNull()` made the analysis unnecessary in the first
place, so the accessor, the three analyses that fed it (`collectNilSafePtrParams`,
`reassignedBeforeDerefParamName`, and the package-wide nil-argument pre-pass) and their vestigial
receiver arms were deleted together — 382 net lines of converter. The golib method survives with
its own unit coverage, but converted code no longer emits it.

## Canonical typed-nil pointer boxing
Go's typed nil is a real value: `any((*T)(nil))` is a **non-nil** interface carrying dynamic type
`*T`, `%T` prints `*T`, and the pervasive descriptor idiom `reflect.TypeOf((*T)(nil)).Elem()`
resolves the pointee type. A bare C# `null` erases all of that, so a nil→pointer **conversion**
renders in pointer context and yields the type's **canonical typed nil instance** — one shared,
write-protected instance per pointer type (`ж<T>.NilBox`; a generated named-pointer wrapper's
`NilInstance`), which the `NilType` implicit conversion returns:

```go
var errorType = reflectlite.TypeOf((*error)(nil)).Elem()
var x any = (*int)(nil)   // x != nil; %T prints *int
```
```csharp
internal static reflectliteꓸType errorType = reflectlite.TypeOf(((ж<error>)nil)).Elem();
any x = ((ж<nint>)nil);
```

Supporting semantics, all structural (`m_isNull` — never the value-peeking `IsNull`, which remains
the dereference guard):

- **Pointer identity**: a null reference and a nil box are the same Go nil pointer and compare
  equal; a heap box *holding* a nil value (`ж<ж<T>>` captured local) is a **non-nil pointer holding
  nil** — two such distinct addresses are unequal and neither equals nil (`&p1 != &p2` though
  `p1 == p2 == nil`), including when both slots share the canonical singleton.
- **Interface adapters** null-coalesce their receiver box to the canonical instance, so an
  interface holding a nil `*T` keeps its dynamic type, typed nils compare correctly across the
  `any`/interface boundary (`(*A)(nil)`-error ≠ `(*B)(nil)`-error), and `case *T:` matches with a
  nil pointee.
- The non-generic `INilPointer` surface exposes the structural predicate to runtime machinery
  holding a pointer only as `object` (equality tails, the reflection bridge's `IsNil`/`Elem`).

**Every consumer that asks "is this THE nil pointer" must ask the structural predicate.** The
managed-slot `atomic.Pointer<T>` (`core/sync/atomic/type.cs`) canonicalizes the nil pointer to a null
slot so a reference `CompareAndSwap` treats all nil `*T` values as equal — and its `nilCanon` helper
asked the value-peeking `IsNull`, so it collapsed a *pointer to a nil value* to nil as well. `sync.Map`
is built out of exactly that shape and lost both halves of it: `e.p.Store(&i)` with a nil `any` value
dropped the entry outright (`load()`'s `p == nil` then reported not-ok, so `Range` skipped it and
`CompareAndSwap` failed against it), and the `expunged = new(any)` sentinel — a real address holding a
nil interface — became indistinguishable from nil, so a *deleted* entry could not be told from an
*expunged* one and the whole dirty/expunge protocol degenerated. The predicate is now
`ж<T>.IsNilPointer`. The same conflation applied to `atomic.Pointer[error]`, `atomic.Pointer[func()]`
and any `**T` slot (`atomic.Pointer[*T]`), all present in the corpus. (Guarded by
`AtomicPointerToNil`: `Load`/`Store`/`Swap`/`CompareAndSwap` over a pointer to a nil `any`, two
distinct `new(any)` sentinels, a pointer to a nil `*int`, and the genuinely-nil slot, output-compared
vs `go run`. Before the fix the guard panics with a nil-pointer dereference on its second line.)

`(*T)(nil)` conversion **expressions** are where the canonical instance is *minted*, and pointer
locals, parameters and fields keep plain `null` — their statically-typed world never needs the
type carried. The type is carried where it becomes observable instead: at the **boundary into
interface space** (below).

A target written as a pointer to a composite **type literal** — `(*[]byte)(nil)`,
`(*map[string]int)(nil)`, `(*struct{ r int })(nil)` — reaches the same rendering by a different
route. `isTypeConversion` resolves a star target through its `types.Object`, which a type literal
does not have, so these shapes fell through to the regular call path and emitted a bare cast of
`default!` (`(ж<slice<byte>>)(default!)`) — a **null reference**, type erased. They are now claimed
as conversions, and the typed-nil interception renders the target through `convStarExpr` rather than
the resolved name, because an **anonymous** struct/interface element must be LIFTED to a named C#
type and `convStarExpr` is the site that performs that lift (the plain name path emits an
unresolvable raw `struct{…}` signature). `encoding/gob`'s `bootstrapType` table is the whole idiom
in one place, and it is a package-`init` NRE without this — every `reflect.TypeOf(…).Elem()` in it
saw a null descriptor:

```go
tBytes     = bootstrapType("bytes", (*[]byte)(nil))
tReserved7 = bootstrapType("_reserved1", (*struct{ r7 int })(nil))
```
```csharp
internal static typeId tBytes = bootstrapType("bytes"u8, ((ж<slice<byte>>)nil));
    [GoType("dyn")] partial struct Δtype {
        internal nint r7;
    }
internal static typeId tReserved7 = bootstrapType("_reserved1"u8, ((ж<Δtype>)nil));
```

Guarded by `TypedNilInterface` (extended with the slice/map/anonymous-struct type-literal targets),
`PointerToNilPointerIdentity`, and `NamedPointerReinterpret` (the canonical singleton keeps its
`object`-reference nil compare working); part of the reflection-bridge Phase-3 chip (see
`docs/phase4/DESIGN-reflection-bridge.md`).

**A nil converted to an unnamed MAP type is a cast, not an invocation.** The star form above is one
half of the type-literal problem; `map[string]int(nil)` — no pointer, the map type literal written
directly — is the other, and it did not merely erase a type, it failed to compile. The
composite-literal arm of `isTypeConversion` claims a target only when target and argument share an
underlying type, and untyped nil's underlying is *itself*, so the shape was never claimed as a
conversion and fell through to the regular CALL path. What that path emits for a call whose callee
is a type is the type followed by an argument list — `map<@string, nint>(default!)` — which is
**CS1955**, "non-invocable member `map<TKey, TValue>` cannot be used like a method", and the whole
package fails. An untyped-nil operand against a map-underlying target is therefore claimed
explicitly, and the ordinary conversion renderer emits the cast:

```go
fmt.Println(reflect.TypeOf(map[string]int(nil)))
```
```csharp
fmt.Println(reflect.TypeOf(((map<@string, nint>)default!)));
```

The NAMED twin `myMap(nil)` was always correct — `types.ConvertibleTo` holds for it, so the general
named path claimed it and cast — which is why only the type-LITERAL spelling ever broke. The two
sibling nil-able type literals are deliberately left on the routes they are already on, having no
defect: `[]byte(nil)` emits `slice<byte>(default!)`, which is not an invocation of a type but a call
to golib's real `builtin.slice<T>(T[])` conversion helper — the same helper `[]byte("…")` is emitted
against — and yields the nil slice; and `(chan T)(nil)` already renders as a cast. Claiming either
alongside the map would rewrite roughly twenty-five corpus sites to no effect.

The spelling matters, and it is why the corpus never showed this. The BARE `map[K]V(nil)` — the
broken one — appears in **no** standard-library production source; all thirteen of its GOROOT sites
are in `_test.go` files, and Go's own suites lean on it heavily (`fmt`'s
`{"%#v", map[int]byte(nil), …}` table row, one of that package's censused conversion roots;
`reflect`'s TypeOf/DeepEqual tables; `encoding/json`'s encode table; `internal/reflectlite`). The
PARENTHESIZED `(map[K]V)(nil)` reaches the conversion fork through `convParenExpr` instead and was
already emitting a cast, so its one production site — `reflect/type.go`'s
`var imap any = (map[unsafe.Pointer]unsafe.Pointer)(nil)` — compiled all along. Claiming the shape
in `isTypeConversion` now routes that site through the ordinary renderer too, which changes its
parentheses (`(map<…>)(default!)` → `((map<…>)default!)`) and nothing else. So this is a
converted-test and end-user fix whose only corpus footprint is one line of re-parenthesization.
(Guarded by the `UnnamedMapNilConversion` behavioral test — four unnamed-map nil conversions
including composite and struct-keyed element types, a nilness/length/absent-key read proving the
converted nil IS nil rather than merely typed, and the named-map, named-slice, `[]byte`, `chan` and
`*int` controls, output-compared vs Go.)

## A HAND-OWN's pointer parameter sees `NilBox`, never `null` — and the doctrine alone did not hold it

The rule above — *"every consumer that asks 'is this THE nil pointer' must ask the structural
predicate"* — was written, correct, and violated **24 times** on one platform before anything
measured it. That is worth recording, because the reason is a gap between two true sentences in this
same section rather than an author ignoring either of them.

Sentence one: `(*T)(nil)` conversion expressions **mint** the canonical instance, and "pointer locals,
parameters and fields keep plain `null`". Sentence two: `nil` reaches a `ж<T>` through
`implicit operator ж<T>(NilType) => NilBox`. Both hold. What follows from them together is the part
neither states: **a hand-own's `ж<T>` PARAMETER is on the receiving end of a caller's `nil`, so it
sees `NilBox` — a real `StandardBox<T>` whose `.Value` throws — and `Ꮡx is null` is FALSE for it.**
A guard written that way takes the wrong branch and the dereference behind it faults.

Neither half of the predicate is sufficient alone, which is why the corpus form is a pair:

```csharp
if (Ꮡrusage is not null && !Ꮡrusage.IsNilPointer) { ... }   // and its inverse
uintptr addr = Ꮡrusage is null || Ꮡrusage.IsNilPointer ? (uintptr)0 : (uintptr)(nint)(&native);
```

A C# `null` is reachable at the same sites (an uninitialised `ж<T>?`) and `.IsNilPointer` on a
genuine null would itself throw. **The ADDRESS arm needs the same predicate as the dereference**: with
only the deref fixed, a syscall wrapper hands the kernel a non-zero pointer where the caller meant
nil — a quietly wrong call rather than a crash.

Measured 2026-09-02, corpus-wide over every tracked `.cs` under `src/core`, and the split was total:
`syscall/linux/structclass_linux_impl.cs` 17 sites and
`syscall/linux/zsyscall_linux_amd64_impl.cs` 7 sites carried the one-sided form with **zero** using
the predicate, while all **four** `syscall/windows/*` sites used it — one hand-own family written
twice with the check correct on one platform only, invisible on Windows because those functions do
not exist there. Every one of the 24 was a PARAMETER (`Select`, `seedNativeFdSet`, `copyNativeFdSet`,
`FcntlFlock`, `Statfs`, `Fstatfs`, `Sysinfo`, `Adjtimex`, `Fstat`, `fstatat`, `wait4`, `Uname`). The
crash that surfaced it was `syscall.Wait4(pid, &status, 0, nil)` — Go's own `os/exec` wait shape.

**The remedy that makes it stick is a guard, not more prose.** `corpusNilPointerGuard_test.go` walks
every `.cs` under `src/core` in the converter's own `go test` and fails on a `Ꮡ`-prefixed identifier
tested with `is null`/`is not null` alone. It is corpus-wide rather than hand-own-only because the
converter never emits the form (generated code compares with `== nil`), so the walk needs no
exception list and catches the next hand-own wherever it lands; comment lines are skipped, and an
empty walk is a FAILURE rather than a pass so it cannot go green over a hole.

<a id="reflectvalueinterface-is-a-boundary-into-interface-space-so-it-packs-the-typed-nil-too"></a>Moved to [`reflect.Value.Interface()` is a boundary into interface space, so it packs the typed nil too](reflection/values.md#reflectvalueinterface-is-a-boundary-into-interface-space-so-it-packs-the-typed-nil-too).

## A pointer crossing into an interface carries its static type, however the pointer was produced
The rule the boxing above is a special case of:

> A Go POINTER entering INTERFACE space is represented by its pointer BOX, carrying its static
> pointee type — however the pointer was produced.

Go's interface value is a (dynamic type, value) pair, so a pointer in one is never merely an
address: `any((*T)(nil))` is a **non-nil** interface whose `%T` prints `*T` and whose `x.(*T)`
assert succeeds with a nil result. Two managed renderings break that, and both silently:

- a bare **`null`** boxes as nothing at all — the type is gone, `x == nil` answers Go's `false`
  with `true`, the assert fails, and the reflection bridge finds no descriptor; and
- a **deref-aliased** pointer (`ref var p = ref Ꮡp.DerefOrNull()`) rendered by its value alias
  boxes a **copy of the pointee** — dynamic type `T` where Go says `*T`, pointer identity gone,
  and a nil one panics at the box instead of crossing intact.

A **non-empty** interface target already satisfies the rule and takes nothing from this: its
conversion goes through a generated adapter that holds the box and null-coalesces it to the same
canonical instance (`AdapterImplTemplate`). The **empty** interface (`any`) has no adapter — C#
boxes the value directly — so the treatment is emitted, as the golib `ж<T>` extension
`OrTypedNil()` (`box ?? ж<T>.NilBox`; `TypedNilBoxAccessor`):

```go
var ip *int
var st struct{ P *AErr }
sl := make([]*BErr, 1)
fmt.Printf("%T %T %T\n", ip, st.P, sl[0])   // *int *main.AErr *main.BErr
```
```csharp
ж<nint> ip = default!;
main_st st = default!;
var sl = new slice<ж<BErr>>(1);
fmt.Printf("%T %T %T\n"u8, ip.OrTypedNil(), st.P.OrTypedNil(), sl[0].OrTypedNil());
```

**Why the BOUNDARY and not the producers.** Emitting the canonical instance for a pointer's zero
value instead would be incomplete by construction: `var p *T` is an emission the converter owns,
but a struct's zero value, a `make([]*T, n)` element and a map miss are not — they are C# `default`,
which no emission intercepts. The boundary is finite and enumerable; the producers are not. It is
the same set of slots [an untyped constant boxed as `any`](empty-interface.md#an-untyped-constant-boxed-as-any-boxes-at-gos-default-type)
already routes through, for the same reason (a value's Go dynamic type must survive being boxed):
call arguments including the variadic `...any` of the fmt family, composite-literal elements, keyed
**and positional** struct fields, map keys and values, `var` initializers, assignments, returns,
channel sends, and an explicit `any(p)` conversion. (The positional form is the one `encoding/gob`'s
`TestNilPointerPanics` table is written in — `[]struct{ value any; mustPanic bool }{{nilStringPtr,
true}, …}` — where every nil row has to arrive as a typed nil for gob to panic on it as Go does.)

Three BUILT-INs take an `any` slot without ever passing a declared parameter, so each applies the
boundary at its own emission arm rather than through the parameter loop: `panic`'s value (a
recovered typed nil must still answer `r.(*T)`), an `[]any` `append` element (the existing
element-to-`any` cast then wraps the result), and an `any`-keyed map's `delete` key — which has to
box to the same value the store did, or it matches no entry. None of the three has a corpus site
today (every `panic` argument in the standard library is an address-of composite, so the census is
zero), but they are the same boundary and the guard exercises all three.

A **FORWARDED multi-value return** is one more slot in that enumeration, and it was the one the
enumeration missed. `return f()` where `f` returns `(*T, error)` and the function's own results are
`(any, error)` cannot convert in place — C# tuple conversions do not consult user conversions
element-wise — so the call is deconstructed into temporaries and each element converts on its own.
Routing the `any` element through the ordinary interface-conversion machinery is wrong precisely
because `any` has **no adapter to hold the box**: with no arm to take, that route falls through to
its pointer-DEREF prefix and boxes a copy of the pointee. The element already *is* the box, so it
takes the boundary treatment directly:

```go
func parsePublicKey(…) (any, error) {
    …
    return ecdh.X25519().NewPublicKey(der)   // (*ecdh.PublicKey, error) into (any, error)
}
```
```csharp
var (ᴛ1, ᴛ2) = ecdh.X25519().NewPublicKey(der);
return (ᴛ1.OrTypedNil(), ᴛ2);               // NOT (~ᴛ1, ᴛ2) — that boxes a PublicKey VALUE
```

`crypto/x509`'s `parsePublicKey` and `parsePKCS8PrivateKey` are the corpus sites — the only two, and
the deref is what made `%T`, `reflect.TypeOf` and every `case *ecdh.PublicKey` type-switch arm on the
result disagree with Go, since `case ж<ecdhꓸPublicKey>` can never match a boxed value. The
ASSIGNMENT sibling of this arm (`c, err = sd.dialTCP(…)`) is not affected: its target list is
screened for *non-empty* interfaces only, so an `any` target never reaches the conversion machinery
there at all. (Guarded by the `MultiValueReturnOrder` behavioral test's identity half — a
`(*thing, error)` call forwarded into `(any, error)`, then read back through a type switch and both
assertions; before the fix the switch takes the `case thing` arm and the `*thing` assertion panics.)

The scope is a genuine `*T` (a `types.Pointer`). `unsafe.Pointer` is a Basic and renders as a
struct, and a NAMED pointer type renders as its generated wrapper struct — neither can be a null
reference, so neither has anything to carry. An address-of (`&x`), a `new(T)`, a `(*T)(nil)` and a
pointer CONVERSION of any of those (`(*Buffer)(&b)`, log/slog's buffer pool) render non-null by
construction and are left bare.

Corpus footprint: **465 sites across 145 files** (A/B of two seeded whole-stdlib reconverts), plus
one site where the deref-alias preamble became dead because the pointer now renders as its box —
`net/http`'s `http2h1ServerKeepAlivesDisabled(hs *Server)`, whose `var x any = hs` had been boxing a
`Server` VALUE and then interface-asserting it, so its `doKeepAlives` probe could never match the
pointer method set. Guarded by the extended `TypedNilInterface` (declared/field/element/map-miss
nils through every one of those slots, `%T`, `==`, and a type assert, output-compared vs `go run`;
before the fix it reports `dpi==nil true` where Go says `false` and panics dereferencing a nil box
on the return path).

<a id="pointer-to-interface-assignment-through-selector-fields"></a>Moved to [Pointer-to-interface assignment through selector fields](interfaces/adapters.md#pointer-to-interface-assignment-through-selector-fields).

## `new(T)` is Go's ZERO value — and for a container kind that means the NIL one

`p := new([]int)` in Go yields a pointer to the zero slice, which is **nil**; likewise
`new(map[K]V)` and `new(chan T)`. golib's container structs each declare a parameterless constructor
that ALLOCATES (`map<K,V>` makes its backing dictionary, `slice<T>` takes the empty array), and
`Activator.CreateInstance<T>` honors a declared parameterless constructor — so `new(T)` handed back a
pointer to a non-nil EMPTY container.

It went unnoticed because the two differ only under `== nil`: `len()` agrees at 0, a range over either
yields nothing, and `encoding/json` marshals them by the same branch. It surfaced where the two
zero-FABRICATION paths finally met. `reflect.Zero`/`New` build a zero through `GoReflect.ZeroValueOf`,
which has always answered the NIL container for these kinds, so

```go
reflect.DeepEqual(new([]any), reflect.New(typ).Interface())
```

compared a nil slice against an empty one and was false — and that comparison is the precondition
`encoding/json`'s whole `TestUnmarshal` table checks before every subtest, which is how one
constructor blocked forty-odd verdicts at once. `builtin.@new<T>` now takes `default(T)` for the
slice/map/chan kinds and keeps running the constructor for every other kind, because that is what
materializes a struct's fixed-size ARRAY fields from the initializers the converter emits into it. The
two rules are one classification now, asked of the same `KindOf`.

---

[← Floating-Point Formatting](floating-point-formatting.md) · [Index](README.md) · [Empty Interface (`any`) →](empty-interface.md)
<!-- {% endraw %} -->
