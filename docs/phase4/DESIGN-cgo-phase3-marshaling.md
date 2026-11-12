# cgo Phase 3: pointers, strings and slices across the C boundary (plan)

**Status: PLAN for review, 2026-11-12, lane H. No code is written.** Base: `db5e34a189`, the tip of increment 5b,
seated in batch 288. Phase 3 is a stretch for the .4 release by the owner's ruling: it ships in .4 only if it is
ready when .4 is otherwise done, and .4 never waits on it.

This plan answers [PLAN-cgo-interop.md](../PLAN-cgo-interop.md)'s Phase 3 section, as amended by the rulings recorded
in [RECON-cgo-phase3-target.md](RECON-cgo-phase3-target.md) section 7. Each statement about the tree below is marked
READ (read at the base) or NOT READ (to be measured by the increment's red).

## 1. The short answer

- **Phase 3 is five increments**, each red first: the 5b re-check's residuals; the string and byte helpers with
  `void *` and the deferred call; Go memory handed to C without a copy, with the out-parameter; the slice view over
  an address; and the harness that runs `net`'s own cgo tests.
- **The pin scope is the wrapper's, and it avoids the syscall pin hole instead of closing it.** A generated wrapper
  is handed the box, never a number, so it can hold the box and pin the array for exactly the call (section 4).
- **The view question has one answer in two parts.** Handing Go memory to C and viewing an address as a Go slice are
  opposite directions with different owners, so they are two operations. They share one rule for telling whose
  memory an address is, and the slice direction is one door for cgo and for the second consumer (section 5).
- **The largest unknown is reading C memory, not writing it.** `net` walks a list C allocated, through the address of
  a pointer field inside it. Whether that works today is NOT READ, and it is the first red (section 6).

## 2. What is ruled, and what this plan adds

Ruled (RECON section 7): the three shapes are in scope; the exit gate gains "a real package's own cgo tests pass";
the proof target is `net`'s cgo resolver on linux with its seven `TestCgoLookup*` tests; the harness is its own
increment; go-sqlite3 follows Phase 3 and is not for .4.

Added by this plan, from reading `net` at go1.26.10 (READ):

- `net` reaches C only through small Go functions it declares itself (`_C_getaddrinfo`, `_C_free`, `_C_ai_next`).
  So its `defer` statements defer Go functions, and it does not use cmd/cgo's deferred closure at all. The deferred
  shape stays in scope by ruling, but `net` does not prove it; the fixture does.
- `net` uses both no-copy forms. `getnameinfo` WRITES into a Go slice through `(*C.char)(unsafe.Pointer(&b[0]))`,
  and reads a Go struct through `(*C.struct_sockaddr)(unsafe.Pointer(&sa))`.
- `net` reads C memory four ways: a field of a C struct through a returned pointer (`ai_family`); the ADDRESS of a
  pointer field of that struct (`&ai.ai_next`, `&ai.ai_addr`); a reinterpretation of C memory as a Go struct
  (`*syscall.RawSockaddrInet4`); and `unsafe.Slice` over a `malloc`ed buffer.

## 3. The helpers (increment 3.1)

**What cmd/cgo writes (READ, the Phase 1 fixture).** `C.CString` is a call of `_Cfunc_CString`, a Go function in
cmd/cgo's types file that calls `_cgo_cmalloc` and copies. `C.GoString`, `C.GoStringN` and `C.GoBytes` are Go
functions over runtime linknames. `C.free` is an ordinary C call, `_Cfunc_free`, with one `void *` parameter. The
converter refuses all of these today by name (`cgoBuiltinNames`).

**The design.**

- The copies are golib functions over an address: copy a Go string or byte slice to an address, and copy a
  NUL-terminated or counted run at an address into a Go string or byte slice. They allocate nothing in C.
- The allocation is the PACKAGE's own `malloc`, bound in its bindings file like any other symbol, and `C.free` is
  the package's own `free`. So a string `C.CString` returns is freed by the allocator that made it, by
  construction. A .NET allocator is not used: nothing shows it is the C library's on every host.
- `C.CString` and `C.CBytes` return a pointer into C memory, a native box, as a C function's pointer result does
  today. `C.malloc` panics when C returns nil, as cmd/cgo's does.
- `C.GoString(nil)` is the empty string, as Go's is.

**`void *` (ruled shape iii).** The parameter is an `unsafe.Pointer`. The wrapper asks golib what it carries: nil;
a number with no box behind it, which crosses as it is (C memory, or the programmer's own arithmetic, as in Go); or
a box, which is classified exactly as a typed pointer is in section 4.

**The deferred call (ruled shape ii).** cmd/cgo writes `defer func() func() { _cgo0 := x; return func() {
_cgoCheckPointer(_cgo0, nil); _Cfunc_free(_cgo0) }}()()`: the arguments are evaluated at the `defer`, the call runs
at return (READ). The route recognizes that shape and lowers it to `defer C.free(x)`, which has the same two times
provided go2cs evaluates a deferred call's arguments at the `defer` (NOT READ for this emission; the red measures
it with an argument reassigned after the `defer`). The `go` form stays refused.

## 4. Go memory handed to C without a copy (increment 3.2)

**What exists (READ).** A wrapper's pointer parameter is the box (`ж<T>`). `GoCgoArgs.StructPointer` classifies it:
nil; one whole value in managed memory, crossed by a stack copy and a copy back; or C memory, crossed by its
address. It refuses an element of a Go slice or array by name as Phase 3's, and refuses a native box minted from
managed storage. The converter refuses cmd/cgo's index and slice check closures (`_cgoIndex`, `_cgoSlice`) as
Phase 3's.

**The design.**

- The classifier gains one result: PINNED. It applies to an element of a Go slice or array, and to a
  reinterpretation of one (`(*C.char)(unsafe.Pointer(&b[0]))`) or of a whole Go value
  (`(*C.struct_sockaddr)(unsafe.Pointer(&sa))`).
- For a pinned argument the wrapper pins the WHOLE backing array, or the value's own storage, hands C the element's
  address, and releases the pin after the call returns. cgo lets C touch the whole array, so the whole array is
  what is held still.
- The pin is scoped to the call and owned by the wrapper: taken before `syscallEntersyscall`, released in the
  `finally` after `syscallExitsyscall`. The wrapper keeps the box alive past the release.
- **A pointee that holds Go pointers is refused with cgo's own panic** (`argument of cgo function has Go pointer to
  unpinned Go pointer`). This is Go's rule, and it is also the one case the runtime cannot pin (below).
- The converter recognizes the index and slice check closures and lowers each to the direct call with the user's
  own argument expression, as it does the address and variable forms.

**The pin-lifetime classes, and what Phase 3 does about each** (`.claude/rules/golib-gen.md`, "class 2: LIFETIME",
READ):

| Class | Phase 3 |
|---|---|
| The syscall buffer pin hole: an address minted through the non-retaining `uintptr` door, its pin on a box that is garbage at once (77 sites) | **AVOIDS it, does not close it.** A cgo wrapper is never handed a number for Go memory. It is handed the box, holds it in a parameter for the call, and pins in its own scope. The 77 syscall sites are unchanged |
| Retention and pinning are two properties | Both are the wrapper's: the parameter and a keep-alive retain, the scoped pin pins |
| The reference-bearing pin miss is structural: no pinned buffer is ever constructed for such a box | **SHARES the limit and turns it into the refusal above.** Phase 3 never asks for a pin the runtime cannot give |
| A pin's cost is invisible to a byte census (a handle, a finalizable holder) | The scoped pin has no finalizable holder: it is released by the wrapper's `finally`, not by a finalizer. Its cost is priced from the type in the increment, and stated |
| A box cannot observe a write through a `ref` it handed out | Not reached: C writes the pinned memory itself, and no copy is written back |

**Route #7.** The classifier is golib and the wrappers are converter output, so the increment owes both gate
families: full GolibTests, GenTests, and the generator's goldens on the hosted legs.

**The out-parameter (ruled shape i).** `&db`, `&res` and `char **errptr` are pointers to one pointer. The wrapper
crosses it as it crosses one scalar today: a stack word, copied in and copied back. Copy-in and copy-back are the
existing rules for a pointer MEMBER of a struct (`PointerMember`, `PointerMemberBack`, READ): nil is 0, a C pointer
is its address, a Go pointer is cgo's panic, and an address C left alone keeps its box.

## 5. The slice view over an address (increment 3.3), and the second consumer

**The question (COORD, 2026-11-12).** A separate proof project converting `modernc.org/sqlite` found that a slice
of a pointer-to-array cast over native memory copies, so a write through it is lost; the sharpest site is its
allocator's realloc. Design Phase 3's view to cover that and the no-copy `&b[0]` case, or say why they differ.

**What exists (READ).** `convSliceExpr` lowers `(*[N]T)(p)[lo:hi]`, where the element type differs or `p` is a raw
pointer, to a slice built from a span, and that constructor copies. `copyingSliceViewNote.go` warns when the copy is
written through, and says no aliasing door exists there. golib has a native-backed slice
(`slice<T>.OverNativeMemory`), which reads and writes the real memory and takes only an element type with no Go
pointers in it. The 20 sites and the project's repros are NOT READ by H.

**The answer: two operations, one rule.**

- **They must differ in lifetime, so they are not one type.** Handing `&b[0]` to C produces an address C uses for
  one call; its owner is Go, and the wrapper bounds it. A slice over an address produces a Go value with no bound;
  its owner is whoever owns the memory. One view type would have to carry either an unbounded pin or a bounded
  slice, and each is wrong for the other.
- **They share the rule that decides whose memory an address is**, which today lives in `GoCgoArgs` (genuinely
  native: no retained source, no order token, no pin of managed storage). It moves to one golib classifier both
  sides call.
- **The slice direction is ONE door for both consumers.** cgo needs it too: `net` takes `unsafe.Slice` over a
  `malloc`ed buffer, and go-sqlite3 slices a `(*[1 << 30]byte)` cast over a C buffer. The door dispatches on the
  address:
  - C memory and an element type with no Go pointers: the native-backed slice. Writes land. This is the realloc.
  - Go memory of the SAME element type: a reslice of the Go array itself. Writes land, with no pin.
  - Go memory REINTERPRETED as another element type: the copy and the warning stay, as today. A true view there
    needs a pin with no bound, which is the cost class the table above avoids. Named as NOT covered.
  - An element type that holds Go pointers over C memory: refused by name, as the native-backed slice refuses it.

**This increment is not behind `-cgo`.** It changes an emission every package can reach, so it moves corpus bytes
and owes the two-seeded comparison on every target. The BOARD counted 14 such emissions in `src/core` on
2026-10-02; the count at this base is NOT READ. It is separable from the rest of Phase 3 (question Q-C).

## 6. Reading C memory (the first red of increment 3.2)

A pointer C returns is a native box today (READ, the `gmtime_r` wrapper). NOT READ, each a red before any design:

1. reading a scalar field through a native box of a struct that has pointer fields (`ai.ai_family`);
2. taking the address of a pointer field inside it and loading through that address (`*_C_ai_next(r)`);
3. reinterpreting C memory as a Go struct and reading a field (`sa.Port`);
4. reinterpreting an element of a native-backed slice as a C struct pointer and passing it back to C
   (`(*_C_struct___res_state)(unsafe.Pointer(&memSlice[0]))`), which must classify as C memory.

If any of these needs a new box kind or a change to go2cs-gen, that is stop line S-B: the design is posted before
code.

## 7. The harness (increment 3.4)

`-cgo` converts one package and is not combinable with `-tests` or `-stdlib` (READ). The corpus converts `net` with
cgo off, by ruling, and that stays.

- The lane converts `net` alone with cgo on into a scratch root seeded from `src/core`, never into `src/core`.
  The rest of the standard library is the corpus's own cgo-off conversion.
- That mix is not a build Go itself makes. It is sound only if no package `net` imports changes its API with cgo
  on. NOT READ; the increment's first measurement.
- The seven tests run through the `-tests` pipeline on a linux hosted leg. H has no linux host, so H measures none
  of them locally.
- `net`'s link line adds `-lresolv`, a second library for the capture.

## 8. Increments

| | What | Red | Behind `-cgo` |
|---|---|---|---|
| 3.0 | The 5b re-check's residuals (BOARD, 2026-11-12): the dropped `//go:build cgo` file with no `import "C"`, the baseline link error, the test seam's `-tags`, a linux reading of the wider refusal set | one per item | yes |
| 3.1 | Helpers, `void *`, the deferred call. A Phase 3 fixture and probe functions that take, return and fill C strings | byte-identical round trips, both directions, three hosts | yes |
| 3.2 | Reading C memory (section 6); the pinned class; the out-parameter; the no-copy case under GC pressure | section 6's four; a pinned write C makes that Go reads; the pressure arm with its control | yes, plus golib |
| 3.3 | The slice door (section 5) | the realloc shape: a `copy` into a view of C memory that Go then reads back | **no** |
| 3.4 | The harness and `net` | the seven tests fail before, pass after, on the linux leg | yes |

## 9. Predictions, to be scored as worded

- **P1.** cmd/cgo's binary imports `malloc` and `free` under their plain names on linux/amd64, darwin/amd64,
  darwin/arm64 and windows/amd64, so neither is refused by the verified-names rule.
- **P2.** At the base, `(*C.char)(unsafe.Pointer(&b[0]))` reaches the wrapper as a native box minted from managed
  storage and is refused with that text, not with the slice-element text.
- **P3.** Of section 6's four readings, 1 passes at the base and at least one of 2, 3 and 4 fails.
- **P4.** Increments 3.0, 3.1, 3.2 and 3.4 move 0 corpus bytes on every target. GolibTests' total rises by the new
  arms only, and no existing row changes.
- **P5.** Increment 3.3 moves corpus bytes at exactly the copying-view emissions counted at its base before the
  change, and nowhere else.
- **P6.** The pressure arm's negative control, the same call with the pin removed, FAILS on H within the arm's
  budget. If it cannot be made to fail, the arm proves nothing (stop line S-A).
- **P7.** Every new binding's native call sits inside the transition bracket: externs equal brackets on each
  golden, plus windows' one errno slot.
- **P8.** The first cgo-on conversion of `net` with 3.1 and 3.2 in refuses at least one use this plan does not
  name.
- **Most likely red that is not the mechanism:** the `net` tests need `localhost` and a reverse lookup to resolve on
  the hosted runner, and a `...WithCancel` test races a cancelled context against a blocking C call.

## 10. Stop lines

- **S-A.** The pressure arm's control cannot be made to fail: stop and report. The GC-pressure gate is not claimed.
- **S-B.** Reading C memory needs a new box kind or a go2cs-gen change: post the design first.
- **S-C.** The pin scope needs instance state on `ж<T>`, a byte cost for the whole corpus: stop.
- **S-D.** The harness needs a corpus file changed, or the cgo-off ruling touched: stop.
- **S-E.** A corpus byte moves in an increment predicted 0: stop and attribute before anything else.
- **S-F.** Phase 3 yields to G1 and to any .4 seat that needs H.

## 11. Questions for COORD

- **Q-A. Order.** G1, Phase 2's exit gate, is dated and is .4's. Lean: G1 first, with this plan under review
  meanwhile, then Phase 3's code.
- **Q-B. The deferred call's scope.** The ruling accepts it "for this case" (`C.free`). The recognition is by shape,
  so it serves any function the route already binds. Lean: any bound function; `go` stays refused.
- **Q-C. The slice door's seat.** It moves corpus bytes while the rest of Phase 3 does not. Lean: its own seat,
  early, so the second consumer is not tied to Phase 3's schedule. H has not read that project's sites or repros,
  so whether H or that project's lane cuts it is yours to rule.
- **Q-D. The allocator.** Lean: the package's own `malloc` and `free` (section 3).
- **Q-E. The linux leg.** Lean: a new on-demand stage on the existing workflow, linux only, run once per red and
  once per green.

— H (2026-11-12)

## 12. Amendment 2026-11-12: COORD's rulings on section 11

COORD ruled on the five questions the day the plan was posted. A design review of the plan is running, and code
waits for its verdict. Sections 1 to 11 stand as written; where a ruling changes one, this block governs.

- **Q-A, RULED: G1 first.** G1 is dated and is the .4 release's. Phase 3's code follows it.
- **Q-B, RULED, and WIDENED from the scout's ruling (RECON section 7, item 1):** cmd/cgo's deferred check closure is
  accepted for ANY function the route already binds, not only `C.free`. Recognition is by shape, and deferring a
  bound C function is the same closure as `defer C.free`. A `go` statement with a checked pointer stays refused.
- **Q-C, RULED: the slice door is its own seat, early, and H cuts it.** It is a converter seat: a seeded footprint
  on all four targets, and the no-regression check over the behavioral corpus. The second consumer's repros, once
  that project pushes them, are reds beside H's own, with the allocator's realloc as the named one.
  **Sequencing:** seats that move corpus bytes land one at a time. Two other lanes' seats land first, then the slice
  door, so no two large regenerations meet.
- **Q-D, RULED: the package's own bound `malloc` and `free`.**
- **Q-E, RULED: an on-demand linux stage on the existing workflow.**

— H (2026-11-12)
