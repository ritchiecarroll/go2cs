# DESIGN (scout): the NARROW shape witness

> **Record**, 2026-11-16, R. COORD's ruling (20:56Z): the full witness rewrite is rejected. Scout the NARROW variant as a
> design record plus a standalone probe, with no converter code, and prove three things: (a) no copy, no pun and no
> proxy, with Go's aliasing intact; (b) readability, side by side with the Go; (c) the masked lsconv sites come out
> correct. A Fable review (P3) comes before the owner rules on readability. It follows the struct-proxy scout record
> (`docs/phase4/DESIGN-struct-proxy-scout.md` on `claude/r-struct-proxy-scout` 0f24c170c1, seated separately), which
> found the struct-proxy pun NOT SAFE.
>
> **Revised 2026-11-18 after P3's review (APPROVE WITH CHANGES): see section 7. It supersedes sections 2, 5 and 6 where
> they differ; the earlier text stays visible.**

## 1. Verdict

(a) and (c) are **PROVEN** by the probe, on two runtimes at both JIT tiers. (b) is shown in section 4 for the owner's
ruling; it has a real cost. One design rule is new and owed by any cut: a witness that appears in a named type's
identity must be canonical (section 5).

## 2. The emission

- **Selection, from the declaration alone**, so a callee and its callers always agree. A generic declaration takes the
  witness when one of its type parameters is under a NON-self method-set constraint, its clause is kept
  (methodSetConstraintNeeded), and its own signature puts T where today's routes refuse it:
  - a generic named type (ruling 1);
  - or T inside a result, a map, a chan, a pointer, a nested slice, or a generic named type (section 5).
  A plain `[]T` parameter and a func-typed parameter stay on today's routes (class B). The self route, the constraint
  route and class B are untouched.
- **The witness interface** `IΔshape<T>` is emitted beside the constraint interface I. It has one
  `static abstract R M(T self, ...)` per method of I.
- **The witness type.** For a value or interface T, which implements the converted I directly, one generic
  `IΔshapeOf<T> where T : I` beside I serves every such T. For a pointer T, a per-pair `EжIΔshape` forwards to E's
  methods through the box, like today's adapters.
- **A function** gains `TI` with `where TI : struct, IΔshape<T>`, and each constrained call `x.M()` becomes `TI.M(x)`.
  Its call sites pass the witness as an explicit type argument.
- **A named type** gains the witness parameter ONLY when its own methods call T's methods. MappedSpan and MappedPosition
  have no methods, so they gain nothing and keep Go's identity exactly. Index, gen's Ptr and Array do call T's methods,
  so they gain it.
- T stays the BOX. There is no proxy object, no projected copy and no reinterpretation. `any(x)`, a type switch and
  reflect see `*E` exactly as in Go: the fidelity the proxy routes give up.

## 3. The probe (`data/shape-witness-scout/`)

`go/main.go` is the Go reference, and its output is `go/expected.txt`. `cs/Program.cs` is the witness emission written by
hand, compiled against the real golib (`ж<T>`, `slice<T>`, `append`, `Ꮡ`). It models lsconv's MappedSpan, FromLSPRange
and lspRangeToVirtual, the masked non-generic caller (converters.go:136's shape), and gen's Ptr, Array and ConstArray.

| Arm | Result |
|---|---|
| (a) `FromLSPRange` returns `[]MappedSpan[*SourceFile]` holding the caller's pointer (`spans[0].Script == a`) | equal to Go |
| (a) a write through the result (`spans[0].Script.text = ...`) is seen through the caller's pointer | equal to Go |
| (a) a callee writing its `[]T` parameter writes the caller's slice: with no witness (box route) and WITH the witness (`renameFirstNamed`) | equal to Go |
| (c) the masked site: `FromLSPRangeForSourceFile([]*SourceFile)` calls `lspRangeToVirtual` with the pointer and returns `[]MappedSpan[*SourceFile]` with no widening; it compiles and is correct | equal to Go |
| gen: `Ptr[u8].kind()` and `ConstArray` over a value type, through the canonical generic witness | equal to Go |
| the whole output against Go, runtimes 10.0.11 and 10.0.15, default tiering and `TieredCompilation=0` | EQUAL in all 4 |

Static abstract members on a `struct` type argument are ordinary, fully type-checked generics. The JIT specializes each
witness instantiation, so there is no undefined behaviour to gate and no tier dependence: the struct-proxy scout's
failure mode does not exist here.

## 4. Readability, side by side (for the owner's ruling)

Go, as written (lsconv):

```go
type MappedSpan[T Script] struct {
	Script T
	Start  int
}

func FromLSPRange[T Script](script T, start int) []MappedSpan[T] {
	return lspRangeToVirtual([]T{script}, start)
}

func lspRangeToVirtual[T Script](scripts []T, start int) []MappedSpan[T] {
	var out []MappedSpan[T]
	for i, s := range scripts {
		out = append(out, MappedSpan[T]{s, start + i + len(s.Text())})
	}
	return out
}

spans := FromLSPRange(a, 10)
```

C#, narrow witness:

```csharp
public partial struct MappedSpan<T>
{
    public T Script;
    public nint Start;
}

public static slice<MappedSpan<T>> FromLSPRange<T, TScript>(T script, nint start)
    where TScript : struct, ScriptΔshape<T>
{
    return lspRangeToVirtual<T, TScript>(new slice<T>(new[] { script }), start);
}

internal static slice<MappedSpan<T>> lspRangeToVirtual<T, TScript>(slice<T> scripts, nint start)
    where TScript : struct, ScriptΔshape<T>
{
    slice<MappedSpan<T>> @out = default!;
    foreach (var (i, s) in range(scripts)) {
        @out = append(@out, new MappedSpan<T>(s, start + i + len(TScript.Text(s))));
    }
    return @out;
}

var spans = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 10);
```

Go, gen (the type's own method calls T's method):

```go
type Ptr[T Value] struct{ off int }

func (Ptr[T]) kind() string { var x T; return "Ptr[" + x.kind() + "]" }

p := Ptr[u8]{3}
```

C#, narrow witness:

```csharp
public partial struct Ptr<T, TValue> where TValue : struct, ValueΔshape<T>
{
    internal nint off;
    public @string kind() { T x = default!; return "Ptr["u8 + TValue.kind(x) + "]"u8; }
}

var p = new Ptr<u8, ValueΔshapeOf<u8>>(3);
```

What a Go reader meets, honestly:
- **The good:** the data types read exactly as Go: `MappedSpan<T>` has the same fields and no added parameter, and every
  slice and result carries Go's own element type (`slice<MappedSpan<ж<SourceFile>>>`). Bodies keep their shape line for
  line.
- **The cost, three visible things:**
  - (1) each affected generic has one extra type parameter and a `where TScript : struct, ScriptΔshape<T>` clause;
  - (2) a constrained call reads `TScript.Text(s)` where Go says `s.Text()`;
  - (3) call sites, and the names of witness-carrying types, spell the witness: `Ptr<u8, ValueΔshapeOf<u8>>` for Go's
    `Ptr[u8]`.
- **Scope of the cost:** only the narrow declarations pay it (stdlib: 6; tsc: 12), and everything else keeps today's
  emission.

## 5. Witness identity across assemblies (owed by any cut)

- **Functions:** the witness is a METHOD type argument, never part of a value's type, so any witness for the pair is
  correct, wherever it is emitted. Nothing to rule.
- **Named types that carry a witness parameter** (Index, gen's Ptr and Array) take the witness into their C# identity,
  so it must be canonical:
  - for a value or interface T, the generic `IΔshapeOf<T>` beside I is canonical by construction;
  - for a pointer T, the per-pair witness must be emitted in exactly ONE assembly. Proposed rule: in the assembly of the
    package that declares E, when it can see I; else in I's, when it can see E; otherwise refuse by name.
- The narrow populations already meet it:
  - gen's types are instantiated with value types;
  - tsc's Index[*Export] has E and I both in autoimport;
  - MappedSpan and MappedPosition carry no witness.
- **Reflection** must drop a witness parameter from the Go name (`%T` of `Ptr<u8, ValueΔshapeOf<u8>>` is `Ptr[u8]`). The
  probe's name printer does exactly that. A cut owes a marker on witness parameters that golib's type naming reads.

## 6. Predictions for the eventual cut (scored only when it runs)

- **stdlib footprint:**
  - production: 3-4 .cs in internal/runtime/gc/internal/gen (gen, regalloc, val, plus its package_info), the same on
    all 4 targets;
  - test emission: +1, crypto/mlkem_test.decapsulationKey.
- **CNR:** moves ONE behavioral golden, InlineConstraintLift (3-4 generic types: Box, pair, slot, and Ord if its
  `Less(T)` constraint is classed non-self), and no other. This CORRECTS the suggested "only the NonSelf* goldens": the
  narrow census over all 107 generic behavioral modules found InlineConstraintLift alone, and neither NonSelf test has a
  narrow declaration. It also means working code moves: those types convert today on the constraint route (value
  instantiations), and the declaration-level selection cannot see that.
- **Public surface:** 0. Every narrow stdlib declaration is unexported or in an internal or test package, and every
  witness can be internal.
- **tsc:** the 12 narrow declarations convert. lsconv, autoimport, ls, project and fourslash leave the refused list,
  provided nothing else in them refuses. The 10 dependent packages then build or show their own defects.

## 7. Revision 2026-11-18: P3's review, answered by number

P3 reviewed 288eb48178 as APPROVE WITH CHANGES (COORD inbox, 20261117T011022Z-P3.md), and COORD accepted every finding
(01:11Z). Each answer below is measured, not argued: the probe (`data/shape-witness-scout/cs`, now with CONTROL arms on
stderr) and a census over the converter's own predicates (`data/shape-witness-scout/census`).

### MUST-FIX 1: selection is closed over bodies, to a fixed point

- **Rule.** A generic FUNCTION that instantiates a selected declaration with one of its OWN type parameters, at a
  selected position, is selected too, repeated until nothing changes. Otherwise today's route binds a pointer T to a
  proxy, and the proxy reaches a witness type's identity.
- **Probe.** `dump[T Script](x T) any { return FromLSPRange(x, 0) }` is selected by the closure, so
  `dump(a).([]MappedSpan[*SourceFile])` succeeds, as in Go.
- **CONTROL.** The UNCLOSED form (dump on the non-self proxy route) makes that assertion **false** where Go says true.
- **Census.** The closure adds **0** declarations in std on 4 targets, **0** in the tsc closure and **0** in the 107
  generic behavioral modules. P3's open question ("whether any of the 12 tsc declarations is reached through a
  non-selected forwarder") reads 0. The rule still binds every future cut.

### MUST-FIX 2: one canonical witness for every pair, functions included

Section 5's "Functions: nothing to rule" is withdrawn. A function's witness can flow into a witness-carrying type (gen's
ConstArray returns `Array<T, TValue>`), so a caller-named witness becomes the RESULT'S identity.
- **CONTROL.** `ConstArray<u8, u8ValueΔshapeRogue>` gives the same Go type, `Array[u8]`, a different C# identity:
  `is Array<u8, ValueΔshapeOf<u8>>` reads **false**.
- **Rule.** Every call site and every type mention names THE canonical witness for its pair; no assembly mints a local
  one.

### MUST-FIX 3: canonical location, eager emission, refusal by name

The canonical witness for a pair (T, I) is:
- **for a T that implements I nominally in C#** (a value or interface type whose assembly sees I): the generic
  `IΔshapeOf<T> where T : I`, emitted beside I;
- **for any other T** (a pointer, a value type of an assembly that cannot see I, a structural interface J): a per-pair
  witness emitted EAGERLY by the assembly of E's package, for every narrow interface I that package can see and E
  satisfies. If E's package cannot see I, the instantiation is refused by name, never satisfied by a witness the
  instantiating assembly mints.

A witness-carrying TYPE therefore never takes a witness emitted where it is instantiated. The eager set, re-predicted:
- **std:** the two witness-carrying kept types (gen's Ptr and Array) both take Value, whose 7 implementers are gen's
  own value types (void, Uint64, Uint8x64, Uint64x8, Mask8, Mask64 and the rest, value receivers through valAny). All
  are nominal, so `ValueΔshapeOf<T>` covers them: **0 per-pair witnesses**, and the stdlib footprint is unchanged.
- **tsc:** autoimport.Index takes Named, and its owner's import closure holds **38** types satisfying `Name() string`
  (counted as pairs). That is tsc-only, and an upper bound: some are nominal.
- **behavioral:** 0, because no behavioral declaration is kept (SHOULD-FIX 6).

### SHOULD-FIX 4: wrap and Deref

Both are in the probe now. `Deref[W wrap[T], T Value](ptr Ptr[W]) T` becomes
`Deref<W, T, WValue, WWrap>(Ptr<W, WValue> ptr)`: Ptr[W] needs W's Value witness and `w.wrap` needs W's wrap witness, so
there are two witness parameters for W. T needs none, because its constraint is only carried by wrap's, which the C#
interface `Δwrap<T> : Value` states without a nominal clause. Called as Go's `Deref[Ptr[u8], Ptr[u8]]`, it prints
`5 Ptr[u8] main.Ptr[main.u8]`, equal to Go.

### SHOULD-FIX 5: renameFirstNamed relabelled

It is now a PROPERTY CHECK, not a selection arm. A plain `[]T` parameter is class B's position and is not narrow-selected;
given a witness anyway, the callee still writes the caller's slice.

### SHOULD-FIX 6: P3's predicate, adopted

- **The predicate.** An EXPORTED declaration outside package main stays declaration-level. Any other is kept only if an
  in-package instantiation (tests included) binds T to a non-nominal implementer, or a kept forwarder forwards into it
  (fixed point).
- **Re-scored.**
  - std: 4 kept of 4, all exported in the internal gen package.
  - tsc: 12 of 12. The unexported ls, lsconv and lsp ones each have an in-package pointer instantiation, such as
    lspRangeToVirtual with `*ast.SourceFile`.
  - behavioral: **0 kept** across all 107 generic modules plus class B's NonSelfConstraintSliceArg.
- **InlineConstraintLift is UNMOVED.** Box, pair and slot are instantiated with package-main value types only. Ord's
  `Less(T)` constraint mentions T, so it is self-referential and on the self route, not narrow. The first census
  classed it non-self because the converter's typeMentionsTypeParam does not look inside interface method signatures;
  the revised census does.
- **Correction to section 6:** "CNR moves ONE behavioral golden, InlineConstraintLift" is withdrawn. **CNR: 0 lines.**

### SHOULD-FIX 7: reflection derives what it drops

- Witness parameters carry a `[GoShapeWitness]` marker. The probe's Go-name printer reads it from the generic
  definition's formals; it is no longer told a count.
- It drops markers at every nesting depth: `[]Ptr[u8]` prints `[]main.Ptr[main.u8]`, equal to Go.
- A generic function prints `main.FromLSPRange[...]`, which is Go's own `runtime.FuncForPC` form, with no witness name
  leaking.
- A cut owes the same marker read in golib's type naming and in function naming (FuncForPC, tracebacks).

### NIT 8

The struct-proxy link is now plain text naming its branch, because the record is not at this ref.

### READABILITY additions, for the owner

The heaviest shapes, as a Go reader meets them.

Go:

```go
func Deref[W wrap[T], T Value](ptr Ptr[W]) T

v := Deref[Ptr[u8], Ptr[u8]](Ptr[Ptr[u8]]{4})

type registry struct{ exports Index[*Export] }
```

C#:

```csharp
public static T Deref<W, T, [GoShapeWitness] WValue, [GoShapeWitness] WWrap>(Ptr<W, WValue> ptr)
    where WValue : struct, ValueΔshape<W>
    where WWrap : struct, ΔwrapΔshape<W, T>

var v = Deref<Ptr<u8, ValueΔshapeOf<u8>>, Ptr<u8, ValueΔshapeOf<u8>>,
              ValueΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>>,
              ΔwrapΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>, Ptr<u8, ValueΔshapeOf<u8>>>>(
    new Ptr<Ptr<u8, ValueΔshapeOf<u8>>, ValueΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>>>(4));

public partial struct registry { internal Index<ж<Export>, ExportжNamedΔshape> exports; }
```

- A field or signature holding a witness-carrying type spells its pair (`Index<ж<Export>, ExportжNamedΔshape>`).
- An explicit instantiation of a doubly-witnessed generic, as in Deref, is long. The converter could emit it through a
  `using` alias per instantiation, but that is not designed here.
- **Use sites, not just declarations:** std 4 declarations, 10 instantiation sites in 1 file (all in gen); tsc 12
  declarations, 116 instantiation sites in 11 files.

### Predictions for the eventual cut (scored only when it runs), REVISED

- **stdlib:** gen's 3-4 .cs on 4 targets, plus `ValueΔshape` / `ΔwrapΔshape` beside Value and wrap (in the same files),
  and 0 per-pair witnesses.
- **CNR:** 0 behavioral lines (section 6's InlineConstraintLift line is withdrawn above).
- **Public surface:** 0.
- **tsc:** the 12 kept declarations convert. autoimport gains up to 38 eager per-pair witnesses. lsconv -> fourslash
  leave the refused list unless something else in them refuses.
