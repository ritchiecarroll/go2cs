# A whole-array assignment copies into the destination's storage

**Status: PLAN, for P5's review before the cut (R, 2026-11-15).** The census is
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
  a self-assignment or an overlapping window is right. The lengths are equal by Go's type rules.
- When an element is itself an array or a value-clone struct (`s_elementNeedsDeepCopy`), it assigns each element in
  place, so the inner backings stay put too.
- When `dest` has no backing (`default(array<T>)`: a struct made by `default`, which skips field initializers), it
  takes `source.Clone()`. Nothing can alias storage that does not exist.

The generated named-array wrapper (go2cs-gen) forwards the same `Assign` to its inner `array<T>`.

**The converter.** For such an assignment it emits `L.Assign(R)` (the in-place copy) where it emitted
`L = R.Clone()`, and `L.Assign(new array<T>(…){…})` for a literal. A zero literal, `L = [N]T{}`, is emitted
`L.Clear()`: the same effect, no allocation, and it reads like the reset it is. The predicate is
`typeIsArrayValue` on the left side's type, plus "the statement is a `=`, not a `:=`", and the left side is not
`_`. Tuple assignments (`a, b = b, a`) need their right sides evaluated first: they assign through temporaries
(`var t1 = b.Clone(); var t2 = a.Clone(); a.Assign(t1); b.Assign(t2)`), and the census sizes them.

**Structs holding arrays (the second half).** go2cs-gen gives a `[GoValueClone]` struct a generated
`AssignFrom(ref this T dest, in T source)`. It assigns array fields with `Assign`, nested value-clone structs with
`AssignFrom`, and every other field by value. The converter emits `L.AssignFrom(R)` for a struct `=` on an existing
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
  the sweep, and none is re-baselined.
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
