# Door 1: an element of a Go array or slice of C structs, handed to C

Status: REVISION 3, reds started (COORD inbox H 20261128T054307Z; P3's delta review, inbox COORD
20261128T054244Z, folded here and checked at the seat read). Owner: H. Branch `claude/h-cgo-door1-struct-element`, cut from the go-sqlite3
slice (`claude/h-cgo-sqlite-slice` at 0db617eaf9), whose `void *` member crossing it builds on.

BOARD row: 2026-11-28, "an element of a Go slice of C structs handed to C is refused". COORD's rulings of that day:
- Door 1 is H's next seat, design note first.
- OQ-1 and OQ-2 are settled (inbox H 20261128T051407Z).
- P3's review (APPROVE WITH CHANGES, inbox COORD 20261128T051829Z) folds in before code (inbox H 20261128T051934Z).

Revision 2 folds P3's F1 to F4, NITs 5 and 6, and COORD's p[-1] arm. Each change is marked [F1] and so on.
Revision 3 folds P3's delta: [d1] the copy-back compares pointer words with the snapshot first; [d2] the check window
travels beside the pointer, and ElemRefBox<T> stays sealed; [d3]-[d6] the NITs.

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
pair. Go accepts every one of these calls, so Limitations names each refusal. Two element pointers into one
array (`swap(&a[i], &a[j])`) are a common C shape and are refused here; the later cure is ONE shared mirror per
backing array, as the image door shares one buffer per root [d6].

**Mirror (wrapper).** The struct's mirror type and its copy functions are the wrapper file's (`ᴍin_M`, `ᴍout_M`,
generated per struct), so the array loop is emitted in the wrapper:

    M[]? e0 = null, s0 = null;                         // only on the Elements kind
    if (k0 == Elements)
    {
        e0 = new M[b0.Length];
        for (nint i = 0; i < b0.Length; i++)
            ᴍin_M(ref e0[i], b0[i], w0.Check(i));                     // the window decides each element's check [d5]
        s0 = (M[])e0.Clone();                                          // the copied-in snapshot [F1]
    }
    fixed (M* f0 = e0) { if (k0 == Elements) a0 = (nint)(f0 + x0); ... call ... pin checks ... }
    if (k0 == Elements)
        for (nint i = 0; i < b0.Length; i++)
            if (!MirrorBytesEqual(e0[i], s0[i]))                         // only elements C changed [F1]
                ᴍout_M(ref b0[i], e0[i], s0[i] /*, copies, function when pointer-bearing */);

`w0.Check(i)` is the element's check under section 4's window [d5]:
- `InWindow`: a Go pointer in a pointer member panics with the member's kind in the text, "unsafe pointer" for a
  `void *` member and "pointer" for a typed one (the literal form).
- `Whole`: the same panic with the text "pointer" (the variable form, no window).
- `OutsideWindow`: a Go pointer crosses as 0 (an element before the literal form's slice).

`ᴍout_M(ref v, in m, in s)` takes the snapshot element `s` [d1]. For each pointer member it compares the mirror's word
with the snapshot's word BEFORE running any member function: an equal word keeps the original member, whatever it
holds, and no member function runs again on the original. Only a word C changed goes through the member's copy-back
rule. So `p[-1].n = 7`, legal for C, comes back as 7 while the element's Go pointer stays intact. Without this, a
`void *` member whose word crossed as 0 would come back nil, and a typed pointer member would panic after the call,
recomputing its copied-in word.

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
an element, or the address one past the end, is refused by name. Every mapped result is a PLAIN element box, the
passed element's included [d3]: the window describes one call and never outlives it, so a later `C.g(p)` checks as
the variable form, as Go does.

## 4. The check: Go's range and text, per call form [F3]

The converter already folds cmd/cgo's check closure (cgoRoute.go:455-456 recognises `_cgoCheckPointer(_cgoN,
*_cgoIndexN)`), so at each folded call it knows which form it is converting and, for the literal form, which slice
Go checks.

**PROPOSED (P3's preferred path).** Size: one golib type, one emission change at the fold, the rows below.
- For the literal form, the fold passes Go's check window BESIDE the pointer [d2]. Each struct-pointer parameter
  of a wrapper gains an optional `GoCgoWindow` parameter, defaulting to none (the variable form). The fold fills it
  with `GoCgoWindow.Of(a)`: the slice's start in its backing array and its capacity. `ElemRefBox<T>` stays sealed. It
  is on every element pointer in every program, and a subtype would turn each exact type test there into a hierarchy
  cast for a window only cgo's literal form reads (COORD's ruling). The element box the wrapper receives is the
  program's own `&a[i]`, so equality and every door are unchanged.
- `StructPointer` reads the window when present:
  - The copy-in member check panics only for a Go pointer in an element inside the window, with the member's kind:
    "unsafe pointer" for `void *`, "pointer" for a typed member, as cgoFormatErr names it.
  - An element outside the window (before the slice's start) is still mirrored, since C may read it. A Go pointer in
    one of its pointer words crosses as 0, never as a Go address, because a mirror cannot carry a Go pointer.
    Copy-back leaves the element untouched unless C changed its bytes (F1).
- For the variable form (no window), the whole backing array is checked, with the text "pointer", as
  cgoCheckUnknownPointer says it. That equals Go's check only when the backing array is its own heap object [d4]. For
  an array field, Go checks the whole enclosing object (cgocall.go:752-760). For a package-level variable holding
  pointers, Go panics outright (cgocall.go:771-776), where go2cs runs. Limitations carries both, and slicecolsfield's
  comment keeps go run's variable-form result.

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
- **An element before the literal form's window** [d1]: C reads NULL in a pointer member that holds a Go pointer in Go,
  and a NULL that C stores there is not seen (the word equals the snapshot's, so the original pointer is kept).
- **The variable form's check range** for an array field and a package-level variable [d4].
- **Under the fallback only:** the two check differences of section 4.
- A struct the mirror cannot pass by pointer today (a union, a float or an inline array member it refuses) is refused
  as it is for one record.

## 6. Red first, then the gates

1. **Reds** (one tests-only commit, each red today; every arm's comment keeps go run's lines):
   - `slicecolsgo` flips from a known refusal to an equal run.
   - `slicecolsgoptr2`, in BOTH forms: a Go pointer in element 2's `void *` member, as `C.f(&cols[0])` (Go: "unsafe
     pointer") and as `p := &cols[0]; C.f(p)` (Go: "pointer").
   - `slicecolsreslice`: the literal form over `cols[1:]` with a Go pointer in `cols[0]`. Go runs it to its end. C
     also writes a scalar of `cols[0]` (`p[-1].n`); Go then reads that scalar changed and the pointer member intact
     [d1].
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
2. **The cure**: golib's classification, claim kind and `GoCgoWindow`; the wrapper's window parameter, loop,
   snapshot and snapshot-compared copy-back; the fold's window for the literal form; and the Limitations lines.
3. **Gates**: go test; GolibTests R and D; CNR; hosted cgo-e2e x3; and TestSqlite3Reading x3, which stays at 15 ways (all
   E2), since door 1 changes run time only.

## 7. Settled questions

- OQ-1: the mirror runs from index 0, by Go's rule (section 2).
- OQ-2: a re-slice mirrors the whole backing array, by the same rule. The cost is section 5's.
