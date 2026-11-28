# Door 1: an element of a Go array or slice of C structs, handed to C

Status: REVISION 2, for P3's re-read. Owner: H. Branch `claude/h-cgo-door1-struct-element`, cut from the go-sqlite3
slice (`claude/h-cgo-sqlite-slice` at 0db617eaf9), whose `void *` member crossing it builds on.

BOARD row: 2026-11-28, "an element of a Go slice of C structs handed to C is refused". COORD's rulings of that day:
- Door 1 is H's next seat, design note first.
- OQ-1 and OQ-2 are settled (inbox H 20261128T051407Z).
- P3's review (APPROVE WITH CHANGES, inbox COORD 20261128T051829Z) folds in before code (inbox H 20261128T051934Z).

Revision 2 folds P3's F1 to F4, NITs 5 and 6, and COORD's p[-1] arm. Each change is marked [F1] and so on.

## 1. The site and the refusal

go-sqlite3 v1.14.53 hands C an array of its column records from Go memory:

    var argp *C.sqlite3_go_col
    if len(cargs) > 0 {
        argp = &cargs[0]                    // cargs []C.sqlite3_go_col
    }
    C._sqlite3_bind_step_columns(s.s, argp, C.int(len(cargs)), ...)

go2cs refuses this today, in `GoCgoArgs.StructPointer`: "an element of a Go array or slice: cgo lets C touch the whole
array, which one struct's mirror cannot hold (Phase 3)". The e2e arm `slicecolsgo` (testdata/cgo/p3) is that refusal,
with go run's three lines kept in its comment as the reference.

## 2. Go's rule, in two halves

**The memory C may touch.** cmd/cgo's "Passing pointers" (doc.go:385-387): when Go passes a pointer to an element of
an array or slice, "the Go memory in question is the entire array or the entire backing array of the slice". So C may
legally reach elements BEFORE the one it was handed (`p[-1]` inside the same array). The MIRROR therefore covers the
whole backing array from index 0 (OQ-1). A re-slice mirrors the whole backing array too (OQ-2). Both follow from Go's
semantics, not from convenience.

**The check Go makes, and its text, depend on the call's syntax** [F3]:
- **The literal form `C.f(&a[i])`**, as in the e2e arms. cmd/cgo rewrites the argument (gcc.go:1228-1269) to
  `_cgoIndexN := &a; _cgoN := &(*_cgoIndexN)[i]; _cgoCheckPointer(_cgoN, *_cgoIndexN)`. The runtime checks `a` from its
  first element through its capacity (cgocall.go:591-594, 700-703), not the whole backing array. It names the
  offending member's kind: "Go pointer to unpinned Go unsafe pointer" for a `void *` member, "pointer" for a typed one
  (cgocall.go:738-739, cgoFormatErr).
- **The variable form `C.f(p)`**, as in go-sqlite3. cmd/cgo writes `_cgoCheckPointer(_cgoN, nil)` (gcc.go:1013). The
  runtime reaches cgoCheckUnknownPointer, checks the whole heap object, and panics "... unpinned Go pointer"
  (cgocall.go:750-768), always "pointer".

## 3. The door

**Classify (golib).** `StructPointer<T>(..., ref GoCgoImages images, ...)` gains an element answer before the plain
door:
- The element of a native window (`ElemRefBox<T>.NativeElementAddress != 0`) crosses by its address (`Native`), as
  argv's does. This is the case P5's C-memory door makes reachable from a `(*[N]T)(p)` view; nothing here overlaps
  P5's work beyond this one answer.
- An element of managed storage with a backing `T[]` and an absolute index answers a new door kind,
  `GoCgoStructPointer.Elements`, with the backing and the index.
- A runtime.Pinner-held element keeps today's answer (`TryPinnerHeld`: in place, S6).
- Anything else keeps today's refusal.

**The Elements claim [F2].** Today `ClaimOther` refuses only against an IMAGE (GoCgoImages.cs:88-94); two Other claims on
one root both pass, so `C.f(&cols[0], &cols[2])` would get two mirrors and two copy-backs, the last winning. Elements
is a THIRD claim kind in `GoCgoImages`, keyed by the backing array. It is refused by name against any other claim on
the same keys: an image, a mirror (Other), a pinned argument, or a second Elements claim. There is one golib unit row per
pair. Go accepts every one of these calls, so Limitations names each refusal.

**Mirror (wrapper).** The struct's mirror type and its copy functions are the wrapper file's (`ᴍin_M`, `ᴍout_M`,
generated per struct), so the array loop is emitted in the wrapper:

    M[]? e0 = null, s0 = null;                         // only on the Elements kind
    if (k0 == Elements)
    {
        e0 = new M[b0.Length];
        for (nint i = 0; i < b0.Length; i++) ᴍin_M(ref e0[i], b0[i]);    // the member rules, per element
        s0 = (M[])e0.Clone();                                          // the copied-in snapshot [F1]
    }
    fixed (M* f0 = e0) { if (k0 == Elements) a0 = (nint)(f0 + x0); ... call ... pin checks ... }
    if (k0 == Elements)
        for (nint i = 0; i < b0.Length; i++)
            if (!MirrorBytesEqual(e0[i], s0[i]))                         // only elements C changed [F1]
                ᴍout_M(ref b0[i], e0[i] /*, copies, function when pointer-bearing */);

- **Copy-back only what C changed [F1].** Copying back every element would write a stale value over another
  goroutine's write made during the call. Examples: one goroutine calls C on `&pool[0]` while another writes
  `pool[5].n`; two goroutines call C on disjoint ranges of one backing array. Both are legal Go. The word mirror copies
  back only the words C changed (GoCgoPin.cs:188-189), and the image door decodes only an image C changed (B6). Door 1
  does the same: an element whose mirror bytes equal its copied-in snapshot is not written back. The comparison is
  over the blittable mirror's bytes.
- **The check, per form [F3].** See section 4.
- **The pin check** after the call (`RequireWordOutsidePin`) runs over every element's pointer words, as the word
  mirror checks every word [NIT 6]. So a pointer MEMBER that C sets to another element (`cols[0].next = &cols[1]`, an
  address inside the mirror) is refused by name, as the word mirror's remarks say for argv.
- **A pointer member C wrote** is C memory, minted native. A member C left alone keeps its original pointer (OQ-2's
  rule, already the typed and `void *` members' rule).

**Identity of results [F4].** An element box's identity is the pair (backing array, absolute index) (ж.ElemRefBox.cs:
318-343, 494-499). So a pointer result that lands on ANY element boundary inside the mirror, element k and not only
the passed element, maps back to a box over (b0, k), which equals the program's `&cols[k]`. Only an interior word of
an element, or the address one past the end, is refused by name.

## 4. The check: Go's range and text, per call form [F3]

The converter already folds cmd/cgo's check closure (cgoRoute.go:455-456 recognises `_cgoCheckPointer(_cgoN,
*_cgoIndexN)`), so at each folded call it knows which form it is converting and, for the literal form, which slice
Go checks.

**PROPOSED (P3's preferred path).** Size: one golib type, one emission change at the fold, the rows below.
- For the literal form, the fold hands the wrapper the element through a golib constructor that records Go's check
  window: `GoCgoArgs.Indexed(a, i)` returns an `ElemRefBox<T>` subtype carrying the slice's start in its backing and
  its capacity. Its identity is the plain element box's, (backing, absolute index), so `==` against `&a[i]` and every
  door that reads an element box are unchanged.
- `StructPointer` reads the window when present:
  - The copy-in member check panics only for a Go pointer in an element inside the window, with the member's kind:
    "unsafe pointer" for `void *`, "pointer" for a typed member, as cgoFormatErr names it.
  - An element outside the window (before the slice's start) is still mirrored, since C may read it. A Go pointer in
    one of its pointer words crosses as 0, never as a Go address, because a mirror cannot carry a Go pointer.
    Copy-back leaves the element untouched unless C changed its bytes (F1).
- For the variable form (no window), the whole backing array is checked, with the text "pointer", as
  cgoCheckUnknownPointer says it.

**Fallback, if the fold change is not small in practice.** The whole backing array is checked for both forms, with
the member's kind in the text. Section 5 and Limitations then state both differences:
- (i) the literal form over a re-slice panics where go run does not, for a Go pointer before the slice's start;
- (ii) the variable form's text says the member's kind where go run says "pointer".

I will measure which path holds when the fold is written, and say which one I took in the seat post.

## 5. Cost and limits (Limitations, cgo)

- **Cost.** The whole backing array is mirrored and snapshotted on every call, and compared after it: about three
  passes of len(backing) x sizeof(M) per call (copy-in, snapshot, compare), plus a copy-back of the changed elements.
  This is by Go's rule (section 2), not by the count C uses. For go-sqlite3 that is one record of 40 bytes per bound
  argument per step.
- **Refusals Go does not make:** the double claims of section 3 (F2), and an interior or one-past-the-end result (F4).
  Each is named in Limitations.
- **Under the fallback only:** the two check differences of section 4.
- A struct the mirror cannot pass by pointer today (a union, a float or an inline array member it refuses) is refused
  as it is for one record.

## 6. Red first, then the gates

1. **Reds** (one tests-only commit, each red today; every arm's comment keeps go run's lines):
   - `slicecolsgo` flips from a known refusal to an equal run.
   - `slicecolsgoptr2`, in BOTH forms: a Go pointer in element 2's `void *` member, as `C.f(&cols[0])` (Go: "unsafe
     pointer") and as `p := &cols[0]; C.f(p)` (Go: "pointer").
   - `slicecolsreslice`: the literal form over `cols[1:]` with a Go pointer in `cols[0]`. Go runs it to its end.
   - `slicecolsmid`: `&cols[1]` with ncol 2; C fills elements 1 and 2, element 0 is untouched.
   - `slicecolsback` (COORD): `&cols[1]`, and C reads `p[-1]`, to pin the from-index-0 mirror.
   - `slicecolsresult` [F4]: C returns `p + 1`, and Go compares it with `&cols[2]` (true under go run).
   - [NIT 5] `slicecolsarray` (a Go array, `&arr[1]`), `slicecolsfield` (an array field, `&s.cols[0]`), and
     `slicecolscap` (a Go pointer in an element between len and cap: `cargs[:0]` reused; Go checks through cap).
   - [F1] a concurrent-write row: an element C does not write keeps a write another goroutine made during the call,
     in default mode and under --mn (D4).
   - golib unit rows: `StructPointer` over an element box answers Elements with the backing and index; over a native
     window's element it answers Native; one row per refused claim pair [F2]; and the result mapping, for a boundary,
     an interior word and one past the end [F4].
2. **The cure**: golib's classification and claim kind, the window-carrying element box, the fold's emission for the
   literal form, the wrapper's loop, snapshot and copy-back, and the Limitations lines.
3. **Gates**: go test; GolibTests R and D; CNR; hosted cgo-e2e x3; and TestSqlite3Reading x3, which stays at 15 ways (all
   E2), since door 1 changes run time only.

## 7. Settled questions

- OQ-1: the mirror runs from index 0, by Go's rule (section 2).
- OQ-2: a re-slice mirrors the whole backing array, by the same rule. The cost is section 5's.
