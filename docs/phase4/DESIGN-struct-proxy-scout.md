# DESIGN (scout): the non-self constraint proxy as a one-field struct, reinterpreted in place

> **Record**, 2026-11-16, R. COORD's ruling (20:14Z): .5, no .4 cut; scout the struct proxy of
> [DESIGN-constraint-proxy-nonself](DESIGN-constraint-proxy-nonself.md) section 10 as a design record, NON-SELF route
> only, with no converter code. First answer the question that decides whether the route exists. If the answer is
> "not safe", say so plainly, and the design stops there.

## 1. Verdict

**NOT SAFE.** Reinterpreting a `slice<ж<E>>`'s backing array as a `slice<EжIᴾ>` (a one-field `readonly struct` over the
box) is undefined behaviour on the CLR. The optimizing JIT already exploits that. The design stops here, as ruled.

- **GC safety and data aliasing held in every configuration measured.** That is observed behaviour, not a guarantee.
- **Type safety does not hold, by contract or by measurement.** The same type test on the punned array answers
  differently in tier-0 code and in optimized code.
- A store through the view also bypasses the covariance check of a reference-element array.

## 2. The mechanism, and what the runtime guarantees

`slice<T>` (golib `slice.cs`) is a `readonly struct` that owns a `T[] m_array` plus a window. A native base exists only
for unmanaged `T`. A `slice<Proxy>` therefore needs a `Proxy[]` OBJECT, and there are two candidate mechanisms.

- **`MemoryMarshal.Cast<TFrom, TTo>`** is the runtime's safe reinterpretation. It does not even compile for a reference
  element: `TFrom` is constrained `struct` (CS0453 for `ж<E>`). Closed.
- **A span view**, `MemoryMarshal.CreateSpan(ref Unsafe.As<ж<E>, Proxy>(ref a[0]), n)`, compiles and aliases. But a span
  is stack-only, and `slice<T>` stores its backing on the heap. Closed for `slice<T>`.
- **The object pun**, `Unsafe.As<Proxy[]>(a)`, is the only route that yields a storable `Proxy[]`. Its contract requires
  the object to BE a `TTo`; otherwise the behaviour is undefined. An `ж<E>[]` is not a `Proxy[]`: its method table says
  `ж<E>[]`.

golib already uses this pun once, `slice<T>.AliasOfElement`, and its remarks state the conditions under which it is
tolerated:
- both elements are one-byte, reference-free value types;
- the result is an internal bridge that nothing type-tests.

The proxy case meets neither condition. Its elements carry a GC reference, and the result would flow into arbitrary
converted callee bodies.

## 3. The probe (`data/struct-proxy-scout/`)

A standalone program (`Program.cs`, `proxyprobe.csproj`), not golib. It has an abstract `Box<E>` with two sealed
subclasses, standing in for `ж<E>` and its box kinds, and a one-field `readonly struct ItemProxy` implementing an
interface. Readings are for each runtime (10.0.11 and 10.0.15) under default tiering and `DOTNET_TieredCompilation=0`,
varying one axis at a time.

| Arm | default tiering | tiering off |
|---|---|---|
| layout: `SizeOf<ItemProxy>` == `IntPtr.Size` (8 == 8) | PASS | PASS |
| by-ref span view aliases | PASS | PASS |
| reads, a constrained generic consumer, a swap, a store, `Array.Sort<T>`, a span copy: all alias | PASS | PASS |
| `GetType()` on the punned array is `Box<Item>[]` | PASS | **FAIL** (folded to the static type) |
| `o is ItemProxy[]` on it is false | PASS | **FAIL** (true) |
| `(ItemProxy[])view.Clone()` throws | PASS | **FAIL** (succeeds) |
| `Array.Copy` from the view throws `ArrayTypeMismatchException` | PASS | PASS |
| a store through the view into a covariant `StandardBox[]` is NOT refused (the type hole) | PASS | **FAIL**: the JIT, trusting the allocation's exact type, then reads the stored `OtherBox` as a `StandardBox` |
| an exact-type gate (`a.GetType() == typeof(ж<E>[])`) separates the covariant array | PASS | PASS |
| GC card marking: 200 rounds x 4096 gen0 boxes written through the view into a gen2 array, with gen0 and compacting full collections; 0 lost or wrong | PASS | PASS |

The runtime version changes nothing; the JIT tier alone flips the same four arms on both runtimes. Default tiering ran
the probe's `Main` at tier 0, where the true method table answers. Hot code in a real program reaches tier 1 and folds.
So a type observation on the punned array depends on how hot the code is: undefined behaviour the JIT already acts on.

## 4. What this means for the route

- golib's own exposure would be small today. Its slice paths address elements from the static type and the array
  length: the indexer, `ToSpan`, `copy` (identical elements take a span copy), `append`'s growth copy and `Clone`
  (memberwise, on the struct). GoReflect uses the backing only as an identity key. But "nothing type-tests it today" is
  a property of golib's current code, not a contract, and every future access to `m_array` would inherit tier-dependent
  answers.
- A covariant `ж<E>[]` needs an exact-type gate at the boundary, with a fallback, or a store through the view breaks the
  array's element type.
- The two later cuts of the census (20:13Z) both rest on this pun:
  - (1) results such as `[]MappedSpan[T]` reinterpreted in place;
  - (2) a named type carrying the proxy at a reinterpreting boundary.
  They are therefore not sketched further.

## 5. A type-safe alternative, NOT sized, for a later ruling

If the tsc language-service packages must convert, the type-safe direction is to keep `T` as the box and stop needing a
proxy at all. A method-set constraint would be emitted as a **shape witness**: a second type parameter
`TShape : struct, IShape<T>` whose static abstract members forward each method (`TShape.Name(x)` for `x.Name()`). The
call site passes `itemжNamedShape`.
- No proxy objects, no copies, no puns. Slices, results and named types keep Go's exact element type, so ruling 1 and
  section 5's refusals would lift by construction.
- Its cost is the opposite of this scout's: every generic body under a method-set constraint changes its emission, so the
  stdlib footprint is large rather than zero (crypto's self-referential constraints included).
- It would need its own census and design before any ruling.
