# An allocation-free element-address compare (1.26.10.2 seat #10)

> **STATUS: PROPOSED (lane P2, 2026-10-27; branch `claude/p2-seat10-elem-compare`, cut from master `e6b7f24f81`).**
> Step 1 of seat #10: the design note COORD asked for before any code. G reviews it and COORD rules. It
> designs increment D of [the stack-allocation design](../DESIGN-stack-allocation.md#the-two-non-storage-increments):
> when both operands of `==` or `!=` are element address-ofs, compare the backing storage and the absolute
> index, and take no box. Counts are read at `e6b7f24f81` against go1.26.10, per amd64 target.

## What happens today

Go's `&x[i]` is a plain interior pointer and costs nothing. go2cs mints a `ж<T>` for every take, even when the
only use of the pointer is one comparison. What a take costs today depends on the form the converter emits
(read from the golib source at `e6b7f24f81`, not measured):

| Operand | Emitted as | Objects per take |
|:--|:--|:--|
| `[]T` | `Ꮡ(s, i)` | 1, the `ElemRefBox` |
| `[N]T` value | `Ꮡ(a, i)` | 1, or 0 after the first take for an element type that has had a field view (the persistent table) |
| named slice or array type (`nat`, `color.Palette`) | `Ꮡ(x, i)` | 2: the header is boxed to bind the `IArray<T>` overload, then the `ElemRefBox` |
| pointer to array, or an addressed array local | `p.at<E>(i)` | 2: `arrayView` boxes the array header, then the `ElemRefBox` |

So math/big's `alias()` (`&x[0:cap(x)][cap(x)-1] == &y[0:cap(y)][cap(y)-1]`, both `nat`) costs 4 objects per call,
and each `&z[0] != &x[0]` in `addVW` and `subVW` (both `[]Word`) costs 2.

The comparison itself is already right. `ElemRefBox.Equals` compares the canonical pair: the backing `T[]` and the
absolute index (the slice's `Low` plus `i`). This seat keeps that rule and removes the boxes.

## (a) The census

A `go/packages` walk over std, with `GOOS` set per target, `GOARCH=amd64` and `CGO_ENABLED=0`. It counts each
`==`/`!=` whose operand (parentheses removed) is `&x[i]` over a slice, an array or a pointer to an array.

**Production code, both operands element address-ofs:**

| Package | linux | windows | darwin | Sites |
|:--|--:|--:|--:|:--|
| math/big | 6 | 6 | 6 | arith.go:114, 123, 168, 177 (`&z[0] != &x[0]`, `[]Word`); nat.go:227 (`alias`), nat.go:379 (`same`), both `nat` |
| crypto/internal/fips140/alias | 1 | 1 | 1 | alias.go:26, `InexactOverlap` |
| vendor/golang.org/x/crypto/internal/alias | 1 | 1 | 1 | alias.go:27, `InexactOverlap` |
| vendor/golang.org/x/crypto/cryptobyte | 1 | 1 | 1 | builder.go:287, `&b.result[0] != &child.result[0]` |
| image/gif | 1 | 1 | 1 | writer.go:294, `&gp[0] == &pm.Palette[0]`, both `color.Palette` |
| regexp | 1 | 1 | 1 | exec.go:367, `&t.cap[0] != &cap[0]` |
| syscall | 1 | 0 | 1 | syscall_unix.go:82, `&b[0] != &data[0]` |
| **all** | **12** | **11** | **12** | |

**Production code, one operand only:** 2 on each target, and both keep today's emission: runtime proc.go:695
(`&allgs[0] != allgptr`, the other side a `**g`) and slices.go:472 (`p == &haystack[i]`, the other side a `*E`).

**Test files** (converted by `-tests`, not part of the corpus): 12 sites with both operands on each target (bytes 2,
database/sql 2, encoding/base32 1, encoding/base64 1, fmt 3, reflect 3), and 8 with one operand (crypto/tls 4,
encoding/gob 2, internal/fmtsort 1, runtime 1).

**Behavioral corpus:** 14 sites in SlicePointerIdentity, 2 each in AppendOfMake, ArrayPointerElementAlias and
SliceToArrayPointerAlias, the same on all three targets. UnsafeSliceDataAliasing has 2 one-operand sites
(`unsafe.SliceData(items) == &items[0]`), which keep today's emission.

Every std production site is emitted in the two-argument `Ꮡ(x, i)` form today. The `p.at<E>(i)` form appears only
in the behavioral corpus (7 of its 20 sites).

## (b) The golib API

One value type and the takes that make it. Nothing allocates.

```csharp
// The identity of one element: the backing storage and the absolute index. Only == and != are meant to use it.
public readonly struct ElementAddress<T> : IEquatable<ElementAddress<T>>
{
    private readonly object? m_storage;  // the backing T[]; the zerobase sentinel; null for native memory
    private readonly nint m_index;       // the absolute index; the native address when m_storage is null

    public static bool operator ==(ElementAddress<T> x, ElementAddress<T> y) =>
        ReferenceEquals(x.m_storage, y.m_storage) && x.m_index == y.m_index;
    public static bool operator !=(ElementAddress<T> x, ElementAddress<T> y) => !(x == y);
}

// builtin: Go's &x[i] for a comparison. The same index check as Ꮡ(x, i), then the pair.
public static ElementAddress<T> Ꮡelem<T>(slice<T> target, int index);   // and nint, ulong
public static ElementAddress<T> Ꮡelem<T>(array<T> target, int index);   // and nint, ulong

// ж<T>: &p[i] through a pointer to array<E>, reading the box's storage by ref (no header box).
public static ElementAddress<E> Ꮡelem<E>(this ж<array<E>> p, nint index);   // and int, ulong
```

The emission changes only the take's name, so the line still reads like the Go:

```csharp
if (Ꮡelem(z, 0) != Ꮡelem(x, 0)) {                                                    // arith.go:114
return cap(x) > 0 && cap(y) > 0 && Ꮡelem<Word>(x.slice(0, cap(x)), cap(x) - 1) == Ꮡelem<Word>(y.slice(0, cap(y)), cap(y) - 1);   // nat.go:227
fmt.Println("element identity:", p.Ꮡelem(0) == Ꮡelem(dst, 2), …);                     // ArrayPointerElementAlias
```

**How a take reaches the backing and the absolute offset.**

- `slice<T>`: the backing is `m_array` and the offset is `Low + i`, exactly the pair `ElemRefBox`'s fast arm
  stores. `s[lo:hi]` and `s[lo:hi:max]` add `lo` to `Low`, so they move the offset. `s[:hi]`, `s[:hi:max]`, an
  `append` that fits in the capacity, and a conversion between a named slice type and `[]T` keep `Low` and the
  backing, so they do not. An `append` past the capacity makes a new backing, which compares unequal.
- `array<T>`: the backing is `Source` and the offset is `Low + i`. `Low` is 0 except for the window that Go's
  slice-to-array-pointer conversion makes (`(*[N]T)(s[k:])`), where it is `k`. That window names the same
  absolute element as a take through the slice.
- A named slice or array wrapper: the converter writes the element type, `Ꮡelem<Word>(x, i)`. With `T` explicit,
  C# binds the `slice<T>` overload through the wrapper's implicit conversion to `slice<T>`, which copies the
  header and boxes nothing. When `T` is inferred, C# picks the `IArray<T>` overload instead, which boxes. I checked
  this on a stand-in (`inferred: IArray`, `explicit: slice`). There is deliberately no `IArray<T>` overload, so a
  missed type argument fails to compile instead of allocating.
- `ж<array<E>>`: reads `ref p.Value` and takes its `Source` and `Low`. A box over native memory answers with
  the native element address, as `at` does through `TryGetNativeArrayView`.

**Scope.** The converter rewrites a comparison only when it can render BOTH operands in one of the forms above. A
compare where either operand takes another arm keeps today's emission whole. Those arms are a named array behind a
box (`ж<NamedArr>`, whose backing is created lazily and must be published through the box first), an array field
reached through a box (`base.at(field, i)`), a nested `.at` chain, and the receiver copy form. No std site and no
behavioral site uses them. Ordering compares (`<`, `<=`) do not exist for Go pointers, and a pointer kept for any
other use, such as a map key, keeps its box.

The analysis that boxes an addressed local array is not changed. SlicePointerIdentity's `var a [4]int` stays
`heap(new array<nint>(4), out var Ꮡa)`, because `a[:]` also takes its address. The seat removes the element boxes,
not the array's box.

## (c) The cases where identity is subtle

Each case gets a line in the red-first behavioral test, output-compared with Go. A GolibTests guard also checks
that `Ꮡelem(a, i) == Ꮡelem(b, j)` gives the same answer as `Ꮡ(a, i) == Ꮡ(b, j)` for every case below.

1. **Two slices over one array at different offsets.** `t := s[1:]`, so `&t[0] == &s[1]` is true and
   `&t[0] == &s[0]` is false. `u := t[1:]` gives `&u[0] == &s[2]`. A three-index slice `s[1:2:3]` behaves the same.
   An array and a slice of it compare as in Go: `v := a[:]`, so `&v[2] == &a[2]`.
2. **An array that is a field or an element.** `&x.f[i] == &y.f[j]` with `f` a `[N]T` field of a value is the
   `Ꮡ(x.f, i)` form and is covered. The `array<T>` copy shares `f`'s `T[]`, so the identity is that field's
   storage. An element of an array that is itself an element (`&m[i][j]` over `[][4]T`) is the same:
   `m[i]`'s copy shares the inner `T[]`. When the outer value sits behind a box, the operand takes the
   `base.at(field, i)` arm, and the compare keeps today's emission (see Scope). The test reads today's emission
   for each of these before the change.
3. **A nil slice.** `&s[0]` on a nil or empty slice panics with Go's index error (`index out of range [0] with
   length 0`) at the take. `Ꮡelem` runs the same `CheckElementIndex` before it reads anything.
4. **An out-of-range index.** The take checks the full value of the index first (`goPanicIndex`, and
   `goPanicIndexU` for an unsigned index), so `recover()` sees the same `runtime.boundsError` as today. Each
   operand is a separate call that returns a value, so the left take is checked, and can panic, before the right
   operand is evaluated, the same order as today's `Ꮡ(x, i) == Ꮡ(y, j)`. The spec orders only calls, so a right
   operand that calls a function could legally run that call before the left check in Go. Today's order is kept,
   and the test does not assert on that order.
5. **A zero-size element type.** Go leaves the result unspecified. Today's boxed compare gives two answers, and
   the seat keeps both:
   - an element of a zero-size slice over the shared per-type storage (`make([]struct{}, n)`, or any slice that
     has no array of its own) names zerobase. Every such pointer compares equal to every other, across slices,
     because `NamesZeroBase` decides before the pair does. `Ꮡelem` gives these the zerobase sentinel at index 0.
   - an element of a zero-size slice over a real array (`a[:]` of `[3]struct{}`) keeps its own backing and index.
     `&a[0] != &a[1]`, and neither equals a zerobase pointer.
6. **Native memory.** A slice over native memory (an `mmap` result) is taken today as a `NativeBox`, whose identity
   is the address. `Ꮡelem` stores the address with null storage, so two takes of one native element are equal, and
   a native element never equals a managed one, as `NativeBox.Equals` and `ElemRefBox.Equals` answer today.
7. **A nil pointer to array.** Go panics with a nil dereference before the index check. An extension method
   can be called on a null reference, so `p.Ꮡelem(i)` must raise that panic itself. The test reads how today's
   `p.at<E>(i)` fails on a nil `p`, and the new take must fail the same way.

## (d) The predicted footprint

The two-seeded three-target diff, cut against `e6b7f24f81`. Each site changes one line, the line count of every
file stays the same, and no file gains an import.

| Target | Files | Lines |
|:--|--:|:--|
| linux | 8 | +12/-12 |
| windows | 7 | +11/-11 |
| darwin | 8 | +12/-12 |

The files are math/big `arith.cs` (4 lines) and `nat.cs` (2), `crypto/internal/fips140/alias/alias.cs`,
`vendor/golang.org/x/crypto/internal/alias/alias_purego.cs`, `vendor/golang.org/x/crypto/cryptobyte/builder.cs`,
`image/gif/writer.cs`, `regexp/exec.cs`, and `syscall/<goos>/syscall_unix.cs` on linux and darwin. The two alias
files are converted files whose `AnyOverlap` is hand-owned in an `_impl.cs`. The changed line is the converted
`InexactOverlap`, and the hand-owned function does not change. The prediction is 0 `GoPositionMap` lines, 0
`package_info.cs` changes and 0 `.cs.auto` siblings, and the three targets carry the same line in every flat file.
A falsifier for each: any map line, an import alias, or a file outside the list.

**Behavioral goldens:** 4 change, by 17 lines (+17/-17): SlicePointerIdentity 13, AppendOfMake 2,
ArrayPointerElementAlias 1 and SliceToArrayPointerAlias 1. One golden is added, the new test. CNR shows only those.

**What it should do to the entry.** math/big's `alias()` drops from 4 objects per call to 0, and each
`addVW`/`subVW` compare from 2 to 0. The design's prediction for TestMulUnbalanced (a ratio of 14, falling to
about 5) is G's, and step 2 re-reads it on this box before and after the change.

## Step 2, after the ruling

1. Red first: the behavioral test with the cases above plus an `AllocsPerRun` line that Go prints as 0, and the
   GolibTests guard that agrees with the boxed compare. Both run red on the base.
2. The golib struct and takes, then the converter arm in `convBinaryExpr` that renders both operands through one
   function mirroring `convUnaryExpr`'s element arms. An unrecognised arm returns "not covered" and leaves the
   compare as it is.
3. The two-seeded three-target footprint scored against (d), CNR, the converter suite, GolibTests, and a BOARD row.

## Rulings asked

1. The names: `Ꮡelem` and `ElementAddress<T>`. The `Ꮡ` prefix keeps the take out of the Go identifier space, since
   a local named `addr` would hide a plain `addr(...)` method.
2. The scope: the forms above only, with the boxed and lazy array arms left as they are until a site needs them.

<!-- Provenance. Census: a go/packages walk (golang.org/x/tools v0.52.0) over `std` with Mode
NeedSyntax|NeedTypes|NeedTypesInfo, run three times with GOOS=linux, windows, darwin, GOARCH=amd64, CGO_ENABLED=0 and
GOROOT go1.26.10, once with Tests=false for production and once with Tests=true for _test.go files. The behavioral
walk loaded each test module whose Go source a loose text filter flagged (six modules), on all three targets. The
emitted forms are read at e6b7f24f81: src/core/math/big/arith.cs:115 (`if (Ꮡ(z, 0) != Ꮡ(x, 0)) {`), src/core/math/big/nat.cs:240
(`Ꮡ(x.slice(0, cap(x)), cap(x) - 1) == Ꮡ(y.slice(0, cap(y)), cap(y) - 1)`), nat.cs:411, image/gif/writer.cs:323,
regexp/exec.cs:394, syscall/linux/syscall_unix.cs:97, syscall/darwin/syscall_unix.cs:97,
vendor/golang.org/x/crypto/cryptobyte/builder.cs:308, crypto/internal/fips140/alias/alias.cs:22,
vendor/golang.org/x/crypto/internal/alias/alias_purego.cs:24; src/tests/Behavioral/SlicePointerIdentity/main.cs.target:34
(`Ꮡa.at<nint>(1) == Ꮡa.at<nint>(1)`), ArrayPointerElementAlias/main.cs.target:47 (`p.at<uint16>(0) == Ꮡ(dst, 2)`).
Take costs: src/core/golib/builtin.cs (`Ꮡ<T>(IArray<T> target, int index)`: `AllocationCounter.Count(); return new
ElemRefBox<T>(target, index);`, the header temp charged), `Ꮡ<T>(slice<T> target, int index)` (one object),
`Ꮡ<T>(array<T> target, int index)` -> `ElemRefBox<T>.OfArrayElement` (the persistent table, scoped to
BoxShape<T>.Slotted); src/core/golib/ж.cs `at<Telem>(nint index)` over `arrayView`, whose non-publishing arm reads
`Value is IArray storageView` (a boxed header). Identity: src/core/golib/ж.ElemRefBox.cs `Equals` (zerobase first,
then the native address, then the canonical pair), `NamesZeroBase`, `FastIndex`; ж.NativeBox.cs `Equals`
(`other is NativeBox<T> nb` -> address; anything else false). Overload binding: a stand-in C# program (net10.0) with
`E<T>(slice<T>)`, `E<T>(array<T>)`, `E<T>(IArray<T>)` and a wrapper struct implementing ISlice<ulong> with an
implicit conversion to slice<ulong>: `E(n, 0)` bound IArray, `E<ulong>(n, 0)` bound slice. -->
