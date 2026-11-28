# Door 1: an element of a Go array or slice of C structs, handed to C

Status: DRAFT for COORD and P3. Owner: H. Branch `claude/h-cgo-door1-struct-element`, cut from the go-sqlite3 slice
(`claude/h-cgo-sqlite-slice` at 0db617eaf9), whose `void *` member crossing it builds on.

BOARD row: 2026-11-28, "an element of a Go slice of C structs handed to C is refused". COORD's ruling of that day: door
1 is H's next seat, design note first, ahead of Phase 5's code unless F1 lands on master first.

## 1. The site and the refusal

go-sqlite3 v1.14.53 hands C an array of its column records from Go memory:

    var argp *C.sqlite3_go_col
    if len(cargs) > 0 {
        argp = &cargs[0]                    // cargs []C.sqlite3_go_col
    }
    C._sqlite3_bind_step_columns(s.s, argp, C.int(len(cargs)), ...)

cgo's rule ("Passing pointers"): a pointer to an element lets C touch the whole array. go2cs refuses it today, in
`GoCgoArgs.StructPointer`: "an element of a Go array or slice: cgo lets C touch the whole array, which one struct's
mirror cannot hold (Phase 3)". The e2e arm `slicecolsgo` (testdata/cgo/p3) is this refusal, with go run's three lines
kept in its comment as the reference.

## 2. The precedent: argv's word mirror

An element of a Go array of C POINTERS already crosses (Phase 3 .5 item 2, the argv shape,
DESIGN-cgo-phase3-marshaling 18.12.3). `GoCgoArgs.OutParameter` takes the element box's backing array
(`ElemRefBox<T>.TryGetElementStorage`) and builds a `GoCgoWordMirror` of the WHOLE backing array. It pins the mirror at
the element's word (`GoCgoStructPointer.Pinned`), checks every word C wrote against the call's pins, and copies the words
back. An element of a native window over C memory crosses by its own address, uncopied (`Native`). The documented cost:
the mirror is the backing array's length on every call, not the count C uses, and a re-slice keeps its whole backing
array (docs/Limitations.md#cgo).

Door 1 is the same shape, with the element a C struct instead of a pointer word.

## 3. The door

**Classify (golib).** `StructPointer<T>(..., ref GoCgoImages images, ...)` gains an element answer before the plain
door:
- The element of a native window (`ElemRefBox<T>.NativeElementAddress != 0`) crosses by its address: `Native`, as
  argv's does.
- An element of managed storage with a backing `T[]` and index answers a new kind, `GoCgoStructPointer.Elements`, with
  the backing and the index out. The root is claimed in `images` like a mirror's (`ClaimOther`), so one Go array handed
  C twice in a call, as an image and as elements, or twice as elements, is refused by name as B5 refuses today.
- A runtime.Pinner-held element keeps today's answer (`TryPinnerHeld`: in place, S6).
- Anything else keeps today's refusal.

**Mirror (wrapper).** The struct's mirror type and its copy functions are the wrapper file's (`ᴍin_M`, `ᴍout_M`,
generated per struct), so the array loop is emitted in the wrapper, not in golib:

    M[]? e0 = null;                                    // only on the Elements kind
    if (k0 == Elements)
    {
        e0 = new M[b0.Length];
        for (nint i = 0; i < b0.Length; i++) ᴍin_M(ref e0[i], b0[i]);   // the pointer-member rules, per element
    }
    fixed (M* f0 = e0) { if (k0 == Elements) a0 = (nint)(f0 + x0); ... call ... }
    if (k0 == Elements)
        for (nint i = 0; i < b0.Length; i++) ᴍout_M(ref b0[i], e0[i] /*, copies, function when pointer-bearing */);

- Copy-in runs every element through the member rules, so a Go pointer in ANY element's pointer member panics with
  cgocheck's text (for a `void *` member, "Go pointer to unpinned Go unsafe pointer"), as cgo checks the whole array.
- The pin check after the call (`RequireWordOutsidePin`) runs over every element's pointer words, as the word mirror
  checks every word.
- Copy-back runs every element. A pointer member C left alone keeps its original pointer, and one C wrote is C memory,
  minted native (OQ-2's rule, already the typed and `void *` members' rule).

**Identity of results.** A pointer result equal to `a0` (C returning the pointer it was handed) maps back to the
element's own box. One that lands elsewhere inside the mirror array is refused by name, as an address inside a stack copy
is refused today. A struct array has no per-element identity the wrapper could re-mint without a new box kind.

## 4. Cost and limits (Limitations, cgo)

- Like argv: the WHOLE backing array is mirrored and copied back on every call, from index 0, not from the element and
  not by the count C uses. Go lets C touch the whole array, so a mirror from the element to the end would miss elements
  before it only if C walked backwards, which C cannot do legally from `&a[i]` (OQ-1). The array is allocated per call;
  go-sqlite3's records are a handful of words per column.
- A struct the mirror cannot pass by pointer today (a union, float or inline array member the mirror refuses) is refused
  as it is for one record.

## 5. Red first, then the gates

1. **Reds** (one tests-only commit, each red today):
   - The e2e arm `slicecolsgo` flips from a known refusal to an equal run (go run's three lines).
   - A new arm, `slicecolsgoptr2`: a Go pointer in element 2's `void *` member panics with cgocheck's text on both
     sides, before any C runs.
   - A new arm, `slicecolsmid`: `&cols[1]` handed with ncol 2, so C fills elements 1 and 2; element 0 is untouched,
     and Go reads all three as go run does.
   - A golib unit row: `StructPointer` over an `ElemRefBox` answers `Elements` with the backing and the index; over a
     native window's element it answers `Native`; the same root claimed twice in one call is refused by name.
2. **The cure**: golib's classification, the wrapper's loop and copy-back, and the Limitations line.
3. **Gates**: go test; GolibTests R and D; CNR; hosted cgo-e2e x3; and TestSqlite3Reading x3, which stays at 15 ways (all
   E2), since door 1 changes run time only.

## 6. Open questions

- **OQ-1, from index 0 or from the element.** argv's mirror covers the whole backing array from index 0. I propose the
  same for door 1, for one rule and one code path, at the cost of the elements before the pointer. The alternative,
  from the element to the end, mirrors less and needs its own copy-back bounds.
- **OQ-2, a re-slice.** `&cargs[2:][0]` hands C the element at backing index 2; `len(cargs[2:])` is not visible to the
  wrapper, so the mirror is the whole backing array, as argv's is. Stated as a limit, not refused.
