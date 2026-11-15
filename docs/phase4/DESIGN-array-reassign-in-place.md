# A whole-array assignment copies into the destination's storage
<!-- {% raw %} — Jekyll/Liquid guard: this doc quotes Go composite-literal syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

**Status: PLAN, for P5's review before the cut (R, 2026-11-15); amended after that review in §8.** The census is
`docs/phase4/data/array-reassign-census/`; the finding is P5's (2026-11-15, BOARD row and
DESIGN-array-pointer-doors.md 13.1).

## 1. The defect

In Go an array variable is its storage, and `a = b` copies `b`'s elements into it. Converted code holds an array as
`array<T>`, a struct over a shared backing (`m_array`, with a window `m_low`/`m_length`), and emits the assignment
as a swap: `a = b.Clone()`, or `a = new array<T>(…){…}` for a literal. A slice, an element pointer, a pin or a
number taken from `a` before the assignment keeps the old backing. A struct holding arrays (`[GoValueClone]`) is
replaced whole, so its arrays swap the same way. P5 measured it on linux/amd64 with two kits: 7 of 9 lines differ
without `unsafe`, 8 of 10 with it.

## 2. The rule

An assignment whose left side is an EXISTING location (a variable, a field, an element, `*p`) of array type, or of a
struct type that holds an array by value, copies into the location's own storage. A declaration, an argument, a
return, a map value, a channel send and a composite-literal element stay as they are: each is new storage in Go too,
so the clone is right there.

## 3. The change

**golib.** `array<T>` gains an extension with a `ref this` receiver, so the location is evaluated once:

```csharp
public static void Assign<T>(ref this array<T> dest, in array<T> source)
```

- When `dest` has a backing, it copies `source`'s window into `dest`'s window with span copy semantics (memmove), so
  a self-assignment or an overlapping window is right. The lengths are equal by Go's type rules. *(Not always: §8 C1.)*
- When an element is itself an array or a value-clone struct (`s_elementNeedsDeepCopy`), it assigns each element in
  place, so the inner backings stay put too. *(How: §8 C3, C4.)*
- When `dest` has no backing (`default(array<T>)`: a struct made by `default`, which skips field initializers), it
  takes `source.Clone()`. Nothing can alias storage that does not exist.

The generated named-array wrapper (go2cs-gen) forwards the same `Assign` to its inner `array<T>`.

**The converter.** For such an assignment it emits `L.Assign(R)` (the in-place copy) where it emitted
`L = R.Clone()`, and `L.Assign(new array<T>(…){…})` for a literal. A zero literal, `L = [N]T{}`, is emitted
`L.Clear()`: the same effect, no allocation, and it reads like the reset it is. *(Withdrawn: §8 C2.)* The predicate is
`typeIsArrayValue` on the left side's type, plus "the statement is a `=`, not a `:=`", and the left side is not
`_`. Tuple assignments (`a, b = b, a`) need their right sides evaluated first: they assign through temporaries
(`var t1 = b.Clone(); var t2 = a.Clone(); a.Assign(t1); b.Assign(t2)`), and the census sizes them. *(Superseded by
§8 C5.)*

**Structs holding arrays (the second half).** go2cs-gen gives a `[GoValueClone]` struct a generated
`AssignFrom(ref this T dest, in T source)` *(an instance member: §8)*. It assigns array fields with `Assign`, nested
value-clone structs with `AssignFrom`, and every other field by value. The converter emits `L.AssignFrom(R)` for a struct `=` on an existing
location. Embedded members follow the existing value-clone walk, and `arrayCloneOperations.go`'s note about arrays
reached only through an embed applies here unchanged.

**Pins and unsafe.** None needed. With the backing never replaced, a pin taken from the array's elements
(`pinnedArrayData`, `m_pin ??=`) stays on the storage the variable names, and fix 1's test in door A becomes a check.

## 4. Reach and footprint (from the census)

The standard library, production: 228 sites on windows/amd64, 230 on linux/amd64, 228 on darwin/amd64, 230 on
linux/arm64; about 145 more in the test emissions. One per target is in a hand-owned file (`runtime/mfinal.cs`,
`runFinalizers`, unreachable), so the converter respells 227-229 lines per target. No standard-library local holds an
alias across one; 162-164 per target are fields, elements, derefs or package variables, which local analysis cannot
classify. The demo closures (sqlite-cs, TypeScript) are their owners' runs.

## 5. Predictions, to be scored as worded

- **RA-1 (red first).** P5's two kits, made behavioral programs, fail their output comparison at master: 7 of 9 and
  8 of 10 lines differ from Go.
- **RA-2.** With the cut, both print Go's lines, in 3 runs of 3.
- **RA-3.** The four-target footprint is the census's sites minus the hand-owned one, as line rewrites in place, with
  position-map lines 0, since the line count is unchanged. Tuple-assignment sites add lines; their number is read
  before the run.
- **RA-4.** Allocation goes DOWN at each site whose right side was cloned (no new backing), and at a zero literal.
  An alloc-assert row reached by such a site can move cheaper, which is still a move: such rows are named before
  the sweep, and none is re-baselined. *(Reworded in §8.)*
- **RA-5.** CNR moves only the behavioral goldens a census of the behavioral programs names, listed before it runs.

## 6. Stop lines

A converted `.cs` moves outside the census's sites; a banked count moves; a golden moves that was not named; a
package stops compiling on any target. Any of these: stop and post.

## 7. Questions for P5

1. `Assign` over a window (`Alias`, `AliasPointer`) copies into the window. Is that the door semantics you want for
   an array that IS a view (door A's `(*[N]T)(p)` result assigned whole)?
2. A destination with no backing takes a clone. Is there any path where a `default(array<T>)` is aliased (a door
   over a null backing) that this would get wrong?
3. Tuple temporaries: clone first, then `Assign`. Any objection to the extra allocation there, since those sites are
   rare?

## 8. Amendment after P5's review (R, 2026-11-15)

P5's review (GO with six conditions, mailbox 2026-11-15T05:24:46Z) answered §7: Q1 yes, a window is the storage `*p`
names; Q2 no path reads wrong data from the clone; Q3 no objection. COORD ruled that each condition goes in here, with
its red arm, before the corpus apply. Where this section and §3 or §5 disagree, this section holds; the lines it
replaces carry a pointer. The census behind C5 and C6 is v2 (`data/array-reassign-census/`, its 2026-11-15
amendment). "Red arm" means the check that fails when the condition is not met. Each one is run red first.

**One shape for an element in place (it serves C1, C3 and N2).** golib gains a public interface
`IGoValueAssign<TSelf>` with one member, `TSelf AssignedFrom(in TSelf source)`. It returns the receiver's own
location after an in-place copy, for the caller to write back. `array<T>` implements it as `var d = this;
d.Assign(source); return d;`. `d` shares the backing, so the copy lands there, and a null-backed `d` takes the clone that
the write-back installs. go2cs-gen implements it on a named-array wrapper and on a `[GoValueClone]` struct. A new
interface, not a member on `IGoValueClone`, so it is no abstract member on a public interface (ApiCompat CP0006).

**C1, unequal lengths.** `Assign` copies `min(len(dest), len(source))` elements, then CLEARS the rest of the
destination. A null-backed source reads length 0, and Go's source is zeros there. A destination shorter than its source
(a window that `AliasPointer` clamped) copies its own length. No path reaches `Span.CopyTo` with a longer source. For an
element in place (C3), "clear" assigns each remaining element from `default(T)`, so an inner backing is zeroed where it
is and never dropped. A null-backed destination takes `source.Clone()`, as §3 says. When both are null-backed it stays
null-backed, as it is today. That edge is the existing null-backing class, which this cut neither causes nor widens.
- *Red arm (GolibTests):* (a) a 4-element destination holding 1..4, assigned from `default(array<int>)`, reads four
  zeros; it is red under a plain min-copy, which leaves 1..4. (b) a 2-element window assigned from a 4-element source
  copies two and throws nothing; it is red under `source.ToSpan().CopyTo(dest.ToSpan())`, which throws
  ArgumentException.

**C2, a zero literal.** Ruled: no `Clear()` form. A zero literal takes `Assign` like every other literal,
`L.Assign(new nint[]{}.array(4))`, as the converter already emits it. The literal carries N, so a null-backed
destination is repaired as it is today, and the special case is gone. The cost: a zero-literal site still allocates its
literal, so RA-4 no longer claims it (see RA-4 below).
- *Red arm (GolibTests):* a null-backed destination assigned a 4-element zero literal reads length 4, and its element 2
  takes a write. It is red under the withdrawn `Clear()`, which leaves length 0 and panics on that index. The kit
  golden pins the emitted `.Assign(` form.

**C3, the per-element path.** When the element type implements `IGoValueAssign<T>` (a nested array, a named array, a
value-clone struct), `Assign` walks the elements: `d[i] = ((IGoValueAssign<T>)d[i]).AssignedFrom(s[i])`. That is one box
per element, with the write-back P5 asked for. No reflection-built delegate is involved (the Native AOT note above
`s_publishArrayBacking`). An element type that has no `IGoValueAssign<T>` keeps today's deep clone. This replaces the
"assigns each element in place" bullet in §3, and the cut-A draft that cloned the source and span-copied it (that draft
swapped the inner backings).
- *Red arm (behavioral, ArrayReassignAliases extended):* `var a [2][2]int; p := &a[1]; a = [2][2]int{{1, 2}, {3, 4}};
  fmt.Println(*p)` prints `[3 4]`. With a struct element: `type E struct{ v [2]int; n int }`, `p := &a[1].v`, a whole
  assignment, then `*p` and `a[1].n` print the new values. GolibTests: an `array<array<int>>` with null-backed elements,
  assigned whole, reads every inner element (red without the write-back: length 0).

**C4, overlap in the per-element path.** When both sides share one backing (`ReferenceEquals` on `m_array`) and the
destination starts above the source, the walk runs from the top down; otherwise it runs from the bottom up. The span path
(memmove) needs nothing.
- *Red arm (behavioral, no unsafe):* `s := [][2]int{{1, 1}, {2, 2}, {3, 3}}; p := (*[2][2]int)(s[1:]);
  q := (*[2][2]int)(s); *p = *q` prints `[[1 1] [1 1] [2 2]]` (red under a bottom-up walk). The reverse, `*q = *p` on a
  fresh `s`, prints `[[2 2] [3 3] [3 3]]` (red under a top-down walk).

**C5, the predicate is "existing location", per operand.** An operand takes the in-place copy when it is not `_`, its
type is an array (or a struct holding one, for the second half), and either the statement is `=` or it is a `:=` whose
operand redeclares (`info.Defs[ident] == nil`). Census v2, production std per target: SINGLE 196-198, TUPLE-CALL 24,
TUPLE-N 8, and RECV, SELECT, TUPLE-COMMAOK, DEFINE-REDECL and RANGE 0 each (RANGE is 1 in the tests). Every form is
converted, at 0 too. Emission:
- SINGLE and RECV: `a.Assign(R)`, as in cut A.
- A tuple: each in-place operand becomes a temporary in the C# deconstruction, and its `Assign` follows, left to right.
  `a, b = b, a` emits `var (aᴛ1, bᴛ1) = (b.Clone(), a.Clone()); a.Assign(aᴛ1); b.Assign(bᴛ1);`. The right sides are
  cloned, so a later Assign cannot read an earlier one's write; that allocation is today's. `x, arrs[i] = f()` emits
  `(x, var arrsᴛ1) = f(); …`. The other operands stay deconstruction targets.
- Go evaluates the left side's index and pointer operands before any assignment. So an in-place operand that is not a
  plain identifier is taken first as a `ref` local: `i, arrs[i] = 2, other` emits `ref var arrsᴛ1 = ref arrs[i];
  (i, var otherᴛ1) = (2, other.Clone()); arrsᴛ1.Assign(otherᴛ1);`. C#'s deconstruction already evaluates a ref
  target first, so a nil or out-of-range location panics no earlier than it does today.
- TUPLE-COMMAOK and DEFINE-REDECL take the same temporaries: `(var aᴛ1, ok) = m[k, …]; a.Assign(aᴛ1);`.
- RANGE: the loop body's `a = vᴛ1.Clone();` becomes `a.Assign(vᴛ1);`. SELECT: `ꟷᐳ(out var aᴛ1)` in the case guard,
  and `a.Assign(aᴛ1);` first in its body.
- The temporaries use the converter's own marker (ᴛ), the marker its other tuple temporaries use.
- *Red arm (behavioral, a new kit ArrayReassignForms):* one line per form. Each takes a slice of the destination
  before the assignment and prints that slice after it. Every line is red at cut A (each form still swaps there), and the
  tuple-order line `i, arrs[i] = 2, other` prints Go's `arrs[old i]`.

**C6, a pointer over native memory.** The census is not 0. sqlite's closure has 69 UNSAFE-CONV derefs on linux/amd64
(68 unnamed `*(*[N]T)(…)`, 1 named) and 6 on windows/amd64, all writing libc's heap. std has 1, in the hand-owned
`mfinal.cs`. So the route is at the box. golib gains `Assign<T>(this ж<array<T>> p, in array<T> source)`. On a box
that stores to native memory it copies N elements through the box's element door; that is the loop
`NativeArrayBox.WriteBoxSlot` already runs for reflect, which an internal `INativeArrayStore<T>` on `NativeArrayBox<T>`
and `NativePointerArrayBox<E>` exposes. On every other box it is `p.Value.Assign(source)`. The converter emits `*p = R`
for a pointer to an UNNAMED array as `p.Assign(R)`, where cut A wrote `p.Value.Assign(R)`. A pointer to a NAMED array
keeps `p.Value.Assign(R)`, the wrapper's own member. Its native door (1 site, `uuid.Uuid_t`, linux) is door B's: I
take P5's offer for that override, in door B's seat. The unnamed override is in this cut, so that P5 reviews it rather
than writes it.
- *Red arm (GolibTests):* a `NativeArrayBox<int>` over unmanaged memory, `p.Assign(source)`, reads N new values at the
  address. It is red under cut A's `p.Value.Assign(source)`, which panics with `noMaterializableArray`.

**N1, a conversion hands the backing over (closed in this cut).** `x := [4]int(named)` emits
`var x = (array<nint>)n;` and `n2 := N(y)` emits `var n2 = (N)y;`, so the two names share one backing. That is a
defect today, before this cut: `x[0] = 9` writes `n[0]`. After the cut, `x.Assign(other)` would write it too. The fix:
`exprReadsValueNeedingClone` sees through an array-to-array conversion to its operand, and the clone is appended to the
parenthesized conversion: `((array<nint>)n).Clone()`.
- *Red arm (behavioral, in ArrayReassignForms):* `var n N; x := [4]int(n); x[0] = 9; fmt.Println(n[0])` prints `0`, and
  `n2 := N(y); n2[1] = 7; fmt.Println(y[1])` prints `0`. Both are red at master.

**N2, `reflect.Value.Set` swaps too (closed in this cut).** `Set` ends in `ж<T>.WriteBoxSlot`, whose body is
`ValueSlot = (T)value!`: the same swap. When `T` implements `IGoValueAssign<T>`, the store becomes
`ValueSlot = ((IGoValueAssign<T>)ValueSlot).AssignedFrom((T)value!)`. The native boxes already copy element by element.
- *Red arm (behavioral, in ArrayReassignForms):* `a := [4]int{}; s := a[:];
  reflect.ValueOf(&a).Elem().Set(reflect.ValueOf([4]int{1, 2, 3, 4})); fmt.Println(s)` prints `[1 2 3 4]`.

**The struct half, restated.** `AssignFrom` is an instance member that go2cs-gen gives a `[GoValueClone]` struct,
`public void AssignFrom(in S source)`, not a `ref this` extension. A generated struct is not readonly, and every
in-place location is a variable or a `ref` (indexers and `Value` return `ref`; a map element is never in-place). It
assigns array fields with `Assign`, nested value-clone structs with `AssignFrom`, and every other field by value. It
also implements `IGoValueAssign<S>`. A named-array wrapper's `Assign` forwards through its materializing accessor
(Q2), and so does the array-VIEW wrapper (`type pallocBits pageBits`).

**Predictions, amended.**
- **RA-2, scored at cut A** (claude/r-array-reassign's uncommitted working tree before C1-C6: the converter's
  SINGLE path, golib's first `Assign`, and the named-wrapper `Assign`):
  ArrayReassignAliases prints Go's 9 lines. ArrayReassignUnsafe prints Go's lines except line 9 (`byte view`: Go 48,
  C# 1). Line 9 is not this mechanism. `(*[16]byte)(v)[:4]` is emitted as
  `new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(v), 4))`, a COPY of the bytes, so the write never reaches
  `g`. The line is identical at master. It goes to P5 as a door-A byte-view finding, and RA-2 is re-scored with that line
  named as outside the cut. One run so far; the 3-of-3 runs come after C1-C6.
- **RA-3.** SINGLE sites stay one-line respells with no position-map lines. A tuple statement gains lines: the
  temporaries and one `Assign` per in-place operand, plus a `ref` line per non-identifier operand. That is 28 statements
  per target in seven packages, and their files take map re-encodes. The per-file prediction is written from the
  two-seeded emission before the apply, never from a probe.
- **RA-4, reworded.** Allocation goes down by one backing at each SINGLE site whose right side was cloned. It is
  unchanged at a literal site (the literal is still built, zero literals included) and at a tuple site (the right side
  is still cloned). At an element-in-place site the bytes go down, not to zero: one box per element replaces a clone
  per element. Alloc-assert rows that such a site reaches are named before the sweep, and none is re-baselined.
- **RA-6 (new).** Every red arm above fails before its change and passes after it, and each run is recorded.

**Stop lines, added.** A tuple statement's emission outside the shapes above; a form that census v2 counts and the
converter leaves on the swap; any ApiCompat finding other than the new interface and the new extension.
<!-- {% endraw %} — keep this the FINAL line, so every later amendment lands inside the raw guard. -->
