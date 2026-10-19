# p1-reflect-method-arms: do the generated reflect method arms stay out of a program that never asks for them?

Seat `claude/p1-reflect-method-arms`, first increment (G's ruling, 2026-10-19). go2cs-gen writes golib's
`IGoMethodSet` on every value type with exported Go methods: per method, the unbound func reflect's
`Type.Method(i).Func` hands out and the binder behind `Value.Method(i)`, as compiled delegates. The arms are reached
only through that interface slot, which only reflect's method paths call, through the type's zero value. A program
that reaches no reflect method access should therefore compile none of them under Native AOT.

Two programs, identical except for one line:

- `nocall/`: types with value and pointer methods (a plain struct, a generic struct, a named float with `String`),
  printed through `fmt`. `fmt` reaches reflect, but nothing calls `Type.Method`, `Value.Method` or `MethodByName`.
- `onecall/`: the same, plus one `reflect.ValueOf(p).MethodByName("Describe").Call(...)`.

Each is published four ways in total: Native AOT default publish, linux-x64, with the arms ON (the seat) and OFF (the
generator's one emission line switched off, restored after).

## Prediction (written before any measurement)

- `nocall`, ON minus OFF: within 16 KB. Only the interface type and its interface-map entries on the constructed types
  may appear; no arm body. FALSIFIER: more than 16 KB means something roots the arms. The suspects, in order: the
  zero-instance lookup (`GoTypeOps.Uninitialized`), then `ΔValue.live`'s `LazyMethodFunc` test, which every reflect
  read passes through (it should fold away, because `LazyMethodFunc` is sealed and only `rtype.Method` constructs one).
- `onecall`, ON minus OFF: between +100 KB and +2 MB. The interface call is now reachable, so every CONSTRUCTED value
  type with exported methods compiles its arms: this program's own types, and every standard-library value type the
  program constructs (the reflect bridge's own types among them). The +127 KB the generic method source measured for
  13 closed methods is the scale for this program's own types alone; the standard library's share is the unknown, and
  that is why the range is wide.
- Output: both programs print what Go prints, ON and OFF (`onecall`'s `y[p=9]` included).

## Measurement (2026-10-19, linux-x64, Microsoft .NET 10.0.12, seat at e90f602d1c)

| Program | Arms OFF | Arms ON | ON minus OFF | Output equals Go |
|---|---|---|---|---|
| `nocall` | 83,960,544 B | 86,547,360 B | +2,586,816 B | yes, ON and OFF |
| `onecall` | 83,960,544 B | 86,547,360 B | +2,586,816 B | yes, ON and OFF |

Scored against the prediction above, which stays as written:

- `nocall`: MISSED. +2,586,816 B against "within 16 KB". The falsifier fired: something roots the arms.
- `onecall`: MISSED. +2,586,816 B against "+100 KB to +2 MB", above the top of the range.
- Output: MET. All four binaries print what Go prints.
- A finding the prediction did not ask about: `onecall` and `nocall` are the same size with the arms OFF too. One
  `MethodByName(...).Call(...)` costs zero bytes, because a program that reaches `fmt` already keeps the whole reflect
  method path.

### What roots the arms

Not the zero-instance lookup and not `ΔValue.live`. `nocall` ON was published again with ILC's dependency graph
(`-p:IlcGenerateDgmlFile=true`). Every arm node in the scan graph is a "Reflectable method", and its one incoming
edge is `Reason="Partial trimming and assembly not trimmable"`, from the type's own boxed method table: for `Plain`,
from the probe assembly; for `os.File`, from `_ZTV27Boxed_os_go_os_package_File with metadata`.

Converted programs publish with `TrimMode=partial` (the converted `.csproj`: "Trim the framework only: a full trim
removes members golib and the converted code reach by reflection"), and no converted assembly is marked trimmable. So
ILC keeps every method of every type the program constructs, arms included, whether or not anything calls them. The
graph holds 184 such `ᴛMethod` nodes (5 in the probe, 179 in standard-library assemblies, `runtime`
23, `os` 15, `io` 13, `reflect` 11 the largest) and 2,388 arm lambdas: about 1 KB per method kept.

The same rule is why the OFF arms are equal: golib and `reflect` are not trimmable either, so `GoMethodValue`,
`BindMethodFunc` and the `IGoMethodSet` call in `GeneratedMethod` are always compiled. Under this trim mode no slot
shape can keep the arms out of a program that does not ask for them; only a trimmable converted assembly could.

### What the arms buy

The `GenericMethodReflect` behavioral program, published with Native AOT at this seat, prints output byte-identical
to `go run` at the same tree: 91 of 91 lines, against 62 with the lazy `Method(i).Func` alone. The 29 lines that
moved are 22 uses of the unbound `Method(i).Func`, 5 bound method values (`Box[[]int]` and `Pair`, through `T` and
`*T`), and 2 lines after the pointer calls that depended on them.

## Increment 2: promoted methods, variadic tails, the 17..24 parameter ladder

COORD's ruling (2026-10-19 19:36Z) accepted increment 1's cost and asked for the three gaps to be covered. The
generator now writes arms for a method with a variadic tail (golib's `Funcꓸꓸꓸ`/`Actionꓸꓸꓸ` family), for one with more
than 16 parameters (golib's `Func`/`Action` ladder through 24), and for the promoted methods it forwards into a type's
generated part. A third program, `promoted/`, reaches promoted methods through a value embed (`Outer`) and a pointer
embed (`Wrap`), and a variadic method, through `MethodByName`, `Method(i)`, `Type.Method(i).Func` and `Call`.

Census before the change, type-checked over the converted standard library: 1,213 exported promoted (type, method)
pairs in 203 types (276 through a pointer embed); 105 variadic methods on linux and darwin and 107 on windows, none
with more than 8 fixed parameters; no method with 16 or more parameters.

### Prediction (written before any measurement)

- `promoted`, Native AOT, arms ON: output equals Go, all 6 lines.
- `promoted`, Native AOT, arms OFF: line 1 equals Go (names only, no bind), and the program fails at line 2, where
  `Twice` is bound through `*Outer`, a value method of a struct reached through a pointer, which needs code Native AOT
  does not have. FALSIFIER: OFF printing line 2 equal to Go means increment 1's run-time binder already covered the
  promoted value method, and the promoted arms are not what fixes it.
- `nocall`, Native AOT, arms ON at this increment minus arms ON at increment 1 (86,547,360 B): between +250 KB and
  +900 KB, central +500 KB. The promoted pairs are about a fifth of all std pairs and the variadic ones under 2%, and
  only the types `fmt`'s closure constructs pay. FALSIFIER: outside that range.
- The standard library's assemblies (`go2cs-stdlib.slnx`, Release, every core package), arms ON minus OFF, summed
  over the DLLs: between +2.5 MB and +6 MB. Increment 1's JIT measure was about 430 B of IL per arm, and the std has
  about 8,000 (type, method, T or *T) cases.
