# DESIGN: unsafe string/slice-header views over managed memory (C45, post-1.26.10.4 train)

C1, 2026-11-11. COORD's rulings, after an independent adversarial review of the first design: option A is a
post-1.26.10.4 TRAIN of its own, with the review's minimum bar; option D (an unsafe-free fork of zeebo/xxh3,
branch `claude/c1-c45-xxh3-fork`, `480a7a90c2`) is the 1.26.10.4 route if the TypeScript proof ships with it. This
record states the defect as measured, the census, the review's findings and the revised design that meets the bar.
Branch `claude/c1-c45-header-views`, cut from master `50dd9a065c`. Its first commit parks the train's red.

## The defect, measured

github.com/zeebo/xxh3 v1.1.0 reaches typescript-go's checker through tsc's hashing. It writes Go's header idioms
directly:

```go
type str struct {
	p unsafe.Pointer
	l uint
}

func HashString(s string) uint64 { return hashAny(*(*str)(unsafe.Pointer(&s))) } // hash64.go:12

func readU64(p unsafe.Pointer, o uintptr) uint64 {                                  // utils.go
	b := (*[8]byte)(unsafe.Pointer(uintptr(p) + o))
	return binary.LittleEndian.Uint64(b[:])
}

var key = unsafe.Pointer(&[...]uint8{0xb8, 0xfe /* ... 192 bytes */}) // consts.go:18
```

Behavioral `UnsafeHeaderViewFamily` carries the family as nine recovered arms. Go prints every arm. Converted at
master `50dd9a065c` and run on linux, the C# fails on eight of them:

| Arm | Go | C# at the base |
|:--|:--|:--|
| `*(*str)(ptr(&s))`, then reads through `h.p` | `16 97 112 8101815670912281193` | panic: cannot read native memory as `main.str`: its CLR layout is not its Go layout (field `p` is a managed reference) |
| the same over a `[]byte` | `16 66 5786930140093827657` | the same panic |
| `*(*string)(ptr(&b))` | `ABCDEFGHIJKLMNOP` | panic: dereference of a managed pointer with no address (arm 2a) |
| `*(*ptr)(ptr(&s))`, the data word | `99` | panic: unsafe pointer arithmetic on a managed pointer with no address |
| pointer stepping `ptr(ui(q)+4)` | `105` | the header panic |
| reads of the package `key` array at an offset | `1 16 1157159078456920585` | panic: cannot view native memory as `array<Byte>`: no managed element storage behind it |
| `len((*[4]byte)(key))` | `4 4` | `16 4`: the conversion drops N |
| writes into `ptr(&[16]byte{})` | `8 24 1230066625199609624` | the array-view panic |
| writes into a struct field's array, `ptr(&h.buf)` | `42 8` | the array-view panic |

The causes, read at the tree:

- **Header shape.** `Reinterpret<@string, str>` admits only the {`unsafe.Pointer`, `nint`} header
  (`ж.StringHeaderBox.cs`, `Applies`). xxh3's `l uint` is `nuint`. The two-field header over a slice has no box,
  since `SliceHeaderBox` needs three fields. `slice<byte> -> @string` has no arm.
- **The data word.** The pointer-word arm reads the stored word only for func, pointer and `unsafe.Pointer` sources.
  A string source gives the variable box's order token, and arithmetic on a token is refused by name.
- **Interior addresses.** `ManagedPointerTokens.Resolve` matches a pinned box only at its exact base address. An
  address inside a pinned array resolves to nothing, and an array view over nothing is refused by name
  (`NativeArrayViewWithoutElementStorage`).
- **N.** `(*[8]byte)(p)` is emitted `(ж<array<byte>>)(uintptr)(...)`; `array<byte>` carries no length, so the view
  can only be the whole array.

## Census

go/types over parsed syntax, with four predicates:

- **A:** `(*T)(unsafe.Pointer(&x))` with x a string or slice and T a different type.
- **C:** `unsafe.Pointer(<uintptr +/- expr>)`.
- **D1:** a package-level `unsafe.Pointer` var.
- **D2:** a write through a `(*T)(p)` conversion, directly or through a local defined by one.

Positive control xxh3 hits all four (A 14, C 19, D1 1, D2 8); negative control `container/list` 0. The first
build read 0 on xxh3, which spells the types through aliases; the instrument unwraps `types.Alias`.

| Population | A | C | D1 | D2 | Where |
|:--|--:|--:|--:|--:|:--|
| std, tests included, linux (975 packages) | 28 | 98 | 71 | 461 | A only in runtime, reflect, internal/unsafeheader, internal/runtime/maps; C and D in runtime, internal/runtime/maps, reflect, syscall, internal/chacha8rand |
| typescript-go's own 105 packages | 0 | 0 | 0 | 0 | |
| typescript-go's dependencies | 21 | 26 | 1 | 58 | zeebo/xxh3 A 14 + C 19 + D1 1 + D2 8; go-toml/v2 A 4 + C 1; pborman/indent A 3 + D2 3; klauspost/compress D2 3; golang.org/x/sys C 5 + D2 43; golang.org/x/tools D2 1; go-cmp C 1 |
| the nine published nugetgo modules | 1 | 0 | 0 | 0 | google/uuid's uuid_test.go:660 |

typescript-go was read at `v0.0.0-20260820064610-89d5d5b2849a` (no version is pinned in this repository). std's
sites sit in code go2cs hand-owns or runs on its managed runtime, which is why std never forced a general mapping.

**Scope, in xxh3.** Every string or slice header view is call-scoped: `hashAny(*(*str)(ptr(&s)))` and the
Hasher's data-word read are consumed within the call that takes them. The stored pointers are `Hasher.key`, either
the package `key` array or an array the Hasher allocates (`ptr(&[192]byte{})`), and neither views a string or a
caller's slice. One further shape: `ptr(&h.buf)` / `ptr(&h.buf[0])`, a view of the Hasher's own field array,
call-scoped.

## The first design, and what the review found

The first design (2026-11-11, inbox `2baa84f4e7`) proposed:

- **A1:** widen the header shapes.
- **A2:** pin the backing array at the header view and register its byte range, the pin owned by the array's
  registration.
- **A3:** resolve interior addresses to array windows, with the converter carrying N.
- **A4:** read the data word for string and slice sources.

The review found, against golib:

- **B1, the pin lifetime is wrong.** Today's pins are owned by the POINTER BOX (`ж.cs` `m_pin`; `PinnedBuffer`'s
  finalizer frees the GCHandle, `runtime/PinnedBuffer.cs`). The registry is weak and validate-on-read
  (`ж.PointerTokens.cs`), and `FromPinnedBox` retains the box (`unsafe.cs`). That design is leak-free. A table keyed
  on the array that holds the pin owner never frees, because the pinned handle roots its own key, so every
  header-viewed backing becomes immortal. A latent twin is `s_arrayElementBoxes` (`ж.ElemRefBox.cs`) if such a box
  ever takes `m_pin`.
- **B2, the cost model is wrong.** A pin per header view is a GCHandle, a finalizable object, a WeakReference and a
  dictionary insert on every hash call. Pin lazily at the `uintptr` conversion (`ж.cs`, operator `uintptr`), or
  carry (box, offset) through Go's rule-3 idiom `unsafe.Pointer(uintptr(p) + o)`. Today `unsafe.Add` rebuilds from
  the number and drops retention. The pinned object heap does not apply, and a scoped pin cannot be derived in
  general.
- **M3, a resolve race.** In `ptr(uintptr(p) + o)` the JIT may retire `p`, and the new Pointer retains nothing (the
  "pin unheld" class). It needs `GC.KeepAlive` or the offset carry, plus a forced-GC control.
- **M4, ranges.** A window straddling the end panics by name. The range is the string's WINDOW
  [base+off, +len), not the whole backing, and needs an ordered range structure: `s_table` is exact-key, and
  `IsTokenArithmetic`'s 4 GiB mask means nothing for real addresses. Overlapping and stale ranges are validated.
- **M5, string immutability.** `@string` shares backing (`string.cs`, `AliasOf`, `s_oneByteStrings`), so a
  writable window corrupts unrelated strings. String-origin ranges are READ-ONLY and panic on a store, including
  through an alias.
- **S6, size.** 4 to 6+ days. The 93-site respelling needs a full reconvert and banked-row reruns. The hot
  `uintptr -> ж<T>` operator (875 sites) has a flip-flop history. `IsHeaderShaped` carries false-positive risk, since
  `StringHeaderBox.Applies` requires `nint`.

## The train's design (revised to the review's bar)

1. **Header shapes (A1).** The {`unsafe.Pointer`, int|uint|uintptr} header over a `@string` or a `slice<T>`, and
   `slice<byte> -> @string` as a read alias. The header's `p` is a Pointer that RETAINS (backing box, byte offset,
   window length, read-only for a string), and mints no address.
2. **The (box, offset) carry.** `unsafe.Pointer(uintptr(p) + o)`, the rule-3 idiom, keeps the retention when the
   converter can see the round trip in one expression, as Go requires it to be written. The converter emits a carry
   form for that expression shape, and `unsafe.Add` keeps retention too. Arithmetic on a retaining Pointer moves its
   offset, never its box.
3. **A lazy pin, box-owned.** A pin is taken only where a number must be a real address: a `uintptr` that escapes
   the carry (stored, passed, compared, or handed to native code). The pin is owned by the Pointer box, as today's
   `m_pin` is, and registered weakly. So nothing outlives the Pointers that reach it, and the leak class B1 found
   cannot arise.
4. **Ranges.** A pinned retaining Pointer registers its WINDOW in an ordered range structure, weak and
   validate-on-read. An interior address resolves to (box, offset). A view straddling the window's end panics by
   name.
5. **Array views (A3).** `(*[N]T)(p)` on a retaining Pointer, or on a number that resolves into a window, is a
   window of that managed array, [off, off+N), aliasing reads and writes. The converter carries N in the emission
   (one line at each of the 93 raw-address array views in the corpus), so `len` is Go's.
6. **Read-only strings (M5).** A string-origin window refuses a store by name, on every path: scalar, array view
   and alias.
7. **The data word (A4).** For a string or slice source the pointer-word read yields the same retaining Pointer as
   the header's `p`.

## The bar (each a gate that has been made to fail once)

- The 9-arm `UnsafeHeaderViewFamily` red goes green, matching Go.
- A leak gate: hash 1,000,000 distinct strings through xxh3; the GC handle count and the range table stay flat. Fired
  red once against a deliberately leaking variant.
- The resolve race: a forced-GC control between the `uintptr` and the conversion back, red without the carry or
  `KeepAlive`.
- Read-only string ranges: a store through a string-origin window panics by name; an unrelated string sharing the
  backing is unchanged.
- A footprint: the 93 respelled sites, two-seeded on every target, with the banked rows that reach them re-swept.
- A cost read on the hot `uintptr -> ж<T>` operator, against its history.

## 1.26.10.4: option D

`src/forks/github.com/zeebo/xxh3@v1.1.0` on `claude/c1-c45-xxh3-fork` is xxh3 with no `unsafe`, consumed by
`replace github.com/zeebo/xxh3 => ...`. Its `FORK.md` lists every change. `identity/check.sh` proves it
byte-identical to the original under real Go:

- 16,632 result lines over every length branch, the seeded forms and the streaming hashers.
- A negative control (150 lines moved by one changed constant).
- xxh3's own compatibility tests pass.
- go2cs's conversion of it prints the original's output byte for byte.

If it ships, Limitations gains an entry: unsafe string/slice header views over managed memory are not supported, and
this train is the planned fix. The TypeScript proof's own docs name the replace.
