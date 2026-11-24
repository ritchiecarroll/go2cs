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

## 13. Amendment 2026-11-12: the design review, each finding answered

COORD had the plan reviewed at `d4ef199184`: APPROVE-WITH-FIXES. Increments 3.0 and 3.1 may proceed after G1.
Increments 3.2 and 3.3 wait for this block. **Every finding is ACCEPTED; none is rebutted.** Each was checked against
the site it cites, at the base. Where this block and sections 1 to 10 disagree, this block governs, and the earlier
text stays as it was written.

### 13.1 What was wrong as written

1. **Section 4, "a pointee that holds Go pointers is refused ... the one case the runtime cannot pin".** That treats
   two different rules as one. Go's rule is about Go pointers. The CLR's is about managed references. A converted
   `syscall.RawSockaddrInet4` holds no Go pointer, and holds two managed references: its `Addr` and `Zero` fields are
   `array<byte>` (READ, `syscall/linux-amd64/ztypes_linux_amd64.cs`). `net` passes exactly that type to C.
2. **Section 4's table, "the scoped pin has no finalizable holder".** False for every reinterpreted pointer. Such a box
   already carries a pin taken where it was reinterpreted, a handle inside a finalizable `PinnedBuffer` (READ,
   `runtime/PinnedBuffer.cs`). And "the wrapper pins the box's address" assumed the box's address is an address:
   on the retained-source arm the box's own remarks say the wrapper "never uses the address" (READ, `ж.NativeBox.cs`),
   and on the order-token arm the number names no memory at all.
3. **Sections 1 and 4, "AVOIDS the syscall pin hole".** True for a typed pointer parameter only. Section 3 let a
   `void *` number with no box behind it cross as it is, and a number minted through the non-retaining door is the
   hole's own predicate.

### 13.2 M1, ACCEPTED: three crossings, decided on the GO layout

A pointer to Go memory is classed by the Go type of what it points at, read from the SOURCE (13.3), never from the C
parameter's type:

- **(a) PIN.** The Go type holds no Go pointer, its converted form holds no managed reference, and its CLR layout is
  Go's (no layout mismatch, no size divergence). `[]byte`, `[N]int32`, a struct of scalars. C is handed the memory
  itself.
- **(b) IMAGE.** The Go type holds no Go pointer, but its converted form holds managed references or is laid out
  differently (an inline array is `array<T>`). The wrapper builds a Go-LAYOUT image of the value: a buffer of the
  SOURCE type's Go size, filled field by field at Go's offsets, handed to C, and copied back after the call. The size
  is the source's (a `RawSockaddrInet6` is 28 bytes), not the parameter's (`struct sockaddr` is 16). This is Phase 2's
  mirror, generalized from C structs to Go structs.
- **(c) REFUSE.** The Go type holds a real Go pointer: cgo's own panic.

Which component writes an image's field copies (the converter, from `go/types`, or go2cs-gen) is the first design step
of 3.2, posted before code.

**New stop line S-G: "net's own argument is unpinnable".** If class (b) cannot be built for `RawSockaddrInet4` and
`RawSockaddrInet6` inside 3.2, the proof target cannot pass, and the plan returns to COORD before more code.

### 13.3 M2, ACCEPTED: classify on the retained source, and recompute the address in scope

- A wrapper never hands C the `NativeAddress` of a box minted from managed storage.
- A reinterpreted box is classed by its RETAINED SOURCE: which storage it came from (an array and an index, or one
  value) and that storage's Go type. `(*C.char)(unsafe.Pointer(&recs[0]))` over a `[]struct{ s string; n int }` is
  class (c) by its source, and is refused. A check keyed on the parameter's `char` would have passed it.
- The address is computed from the source INSIDE the wrapper's pin scope.
- A box minted from managed storage whose source cannot be named (the order-token arm) is refused by name.
- **Reds:** `&b[3]` (the whole array is held, the address is three elements in); a resliced `b[lo:]` (the index is
  the backing array's); an empty slice (Go panics at `&b[0]` before any call, and so must go2cs); and
  `&nativeSlice[0]`, which must class as C memory, since a native-backed slice refuses to be pinned.
- **The table's cost row, corrected:** a reinterpreted box arrives already carrying the reinterpret seam's pin, in a
  finalizable holder. Phase 3 does not add one, does not rely on it, and does not remove it. Removing it changes the
  reinterpret seam for the whole corpus, which is an increment of its own.

### 13.4 M3, ACCEPTED: a number is not evidence of C memory

`C.f(unsafe.Pointer(uintptr(unsafe.Pointer(&b[0]))))` is legal Go. Two rules replace section 3's sentence:

- **In one expression, the converter keeps the box.** Go's own guarantee for that form is by the operand's type, in
  one expression. The cgo route emits it box-carrying, so the wrapper is handed the box and no number is minted.
- **A number that arrives from a variable is looked up** in golib's pin and token registry (`ManagedPointerTokens`)
  before it crosses. A hit on managed storage is classed as that box (13.2), or refused when the box cannot be named.
  A tagged token, which names no user memory, is refused by name. Only a number the registry does not know crosses
  as it is.

**"AVOIDS", reworded to what is true:** Phase 3 never hands C an address of Go memory that it did not itself hold
still for the call. For a typed pointer that holds by construction. For a number it holds by lookup. A number whose
pin was already released before the call is not in the registry and crosses as C memory: that residue is the syscall
hole itself, in code that is already invalid Go (a `uintptr` kept across statements), and Phase 3 does not close it.

### 13.5 M4, ACCEPTED: classify every argument, then `fixed`

Every wrapper runs in this order:

1. Classify EVERY argument and build every image. A refusal throws here, with nothing pinned.
2. Pin each class (a) argument with `fixed`, nested, the innermost block holding the transition bracket and the call.
3. Copy each image back after the call.

There is no pin handle, so nothing can leak when a later step throws, and nothing waits on a finalizer.

**Invariant: a cgo wrapper is synchronous.** It is never async, never an iterator, and never rewritten into a
resumable twin, because `fixed` cannot span a suspension. Any generator that makes such twins for the M:N scheduler
must leave cgo wrappers out. How the bindings file marks them for that generator is settled with the M:N lane;
where that generator keeps its exclusions is NOT READ.

A reinterpreted source's element type is known only at run time, so its pin is a `fixed` over a reference to the
source's first byte, not over a typed array. The mechanism is 3.2's design step. The rule is fixed here: no handle,
no finalizer.

### 13.6 M5, ACCEPTED: the slice door dispatches on the box, never on the address

- The door's input is a box-carrying pointer. A bare number is looked up first, as in 13.4: a hit on managed storage
  keeps today's copy and warning; a tagged token is refused; only an unknown number becomes a native-backed slice.
  A number pointing into managed storage whose pin was released would otherwise have turned today's lost write into
  corruption of the managed heap.
- **"An element type with no Go pointers" in section 5 was the wrong predicate.** The native-backed slice's own test is
  the CLR's: no managed reference, and no size divergence (READ, `slice.cs`). A Go struct with no pointers and one
  inline array field fails it. For such an element type the door keeps today's copy and warning.
- For a generic element type the answer is known only at run time. The door tests the predicate itself and falls
  back to the copy; it never reaches the native-backed slice's panic.
- The second consumer's named site is covered: its `rawmem` is an array of bytes.

### 13.7 The SHOULD items, all ACCEPTED

- **S1.** Section 6 undercounted `net`'s reads. Two more reds: slicing an inline array field of a struct in C memory
  (`sa.Addr[:]`), and indexing a `(*[2]byte)` reinterpretation of a field's address (`&sa.Port`). With 13.2 these say
  `net`'s socket addresses need a Go-layout image in BOTH directions. Stop line S-B is expected to fire, so 3.2 opens
  with a design step.
- **S2.** A red: `unsafe.Sizeof` of a C struct equals C's `sizeof` for a struct with inline arrays and padding
  (`net` sizes a `malloc` by it). Inferred, not confirmed.
- **S3.** Increment 3.3 changes pure-Go behaviour at run time: a copy becomes an alias, the capacity becomes Go's, and
  the third index is honoured where today's emission ignores it (READ, `convSliceExpr.go`; its own comment names
  `os.Readlink`'s reparse path and reflect's `gcSlice`). A byte comparison cannot see a run-time change. So 3.3 owes
  the full behavioral suite, every banked row whose package holds a moved emission, and a census of the `-tests`
  emission. P5's byte footprint is necessary, not sufficient.
- **S4.** P6's control as posted can pass vacuously: with the reinterpret seam's pin still in place, "the pin removed"
  still pins. The control removes every pin on the path. The array is small and newly allocated, so it is not in the
  large or pinned heaps. The C function blocks while another thread forces a compacting collection, then writes. It
  runs in a child process, since the failure may be a crash, and on each RID. P6 stays as posted and is scored
  against this control.
- **S5.** The helpers copy BYTES and never go through a text encoding. Fixture rows: invalid UTF-8; an interior NUL;
  `GoStringN` and `GoBytes` with a zero and with a negative count, each as `go run` behaves on the leg; and `CBytes`,
  whose result the caller frees.

### 13.8 Notes, recorded

- C keeping a pinned pointer past its return is undetectable, as it is in Go. Here it corrupts a heap that moves.
  `Limitations.md` says so when 3.2 lands.
- A view over freed C memory, or one that escapes to another goroutine, is Go's own hazard.
- The errno form exists already and is reused.
- C calling Go stays refused throughout Phase 3.

### 13.9 Predictions, amended

The posted predictions stay and are scored as worded. Added:

- **P3-b.** S1's two reads both fail at the base.
- **P9.** With 13.2 in place, `RawSockaddrInet4` and `RawSockaddrInet6` cross as images whose bytes equal Go's own
  layout of the same value, on linux/amd64.

— H (2026-11-12)

## 14. Amendment 2026-11-12: where the wrapper-is-synchronous invariant lives on the other side

Section 13.5 left one item NOT READ: where the M:N generator keeps what it never twins. COORD had the M:N lane
answer it the same day. This is the PROTOTYPE's site, not master's; the train that lands it will re-cite.

- **The site:** `src/gen/go2cs-gen/TwinGenerator.cs` on `claude/g-mn-proto-a1` (commit `8d1124e491`). A method is
  never twinned when it is native: it holds a `fixed` in its body, or it is `extern`, or it carries `DllImport` or
  `LibraryImport`. That refusal is the first check, and a bodiless extern or `LibraryImport` partial never becomes a
  candidate at all.
- **The mark is structural,** so a bindings file needs no new attribute. A cgo wrapper that pins with `fixed`
  (13.5) is refused by its own body.
- **What that gives 13.5:** the generator refuses the WHOLE method when it holds a `fixed`, even where a suspension
  would sit outside the `fixed` block. That is exactly the invariant: a wrapper is synchronous end to end.
- **The M:N lane's stated limits,** carried here as that lane states them:
  - a suspending call inside an `unsafe` block with no `fixed`, in a method that is not itself marked `unsafe`, is
    still twinned and fails to compile. COORD has asked for that to become a refusal. A cgo wrapper is not that
    shape: its class is `unsafe` and its pin is a `fixed`;
  - `UnmanagedCallersOnly` is not in the rule. That is a C-to-Go entry point, which Phase 3 does not write (13.8)
    and Phase 5 will have to settle.
- **A wrapper with no pinned argument holds no `fixed`.** It is still never twinned, by the generator's other
  marks: it calls a `LibraryImport` partial, and that partial is native. Whether a wrapper that only CALLS a native
  method is itself held synchronous by the prototype is NOT READ here. Phase 3's first increment that touches
  wrappers checks it against the prototype's red (`src/tools/mnspike2/MnNativeWrap`) before relying on it.

— H (2026-11-12)

## 15. Amendment 2026-11-12: increment 3.0, its plan, predictions and stop lines (posted before code)

Phase 2's exit gate is DONE and accepted (`0001be70a7`), so Phase 3's code may start (section 12, Q-A). COORD ruled
the base: 3.0 is cut from the exit gate's tip, because it builds on that branch's converter changes; master is
merged in, never rebased, once the batches that seat the gate have landed. The branch is
`claude/h-cgo-phase3-residuals`.

3.0 writes no marshaling. It closes what the 5b re-check and the exit gate left open (BOARD, 2026-11-12, two rows),
each red first.

### 15.1 The five items

| | Item | Where | The red | The fix |
|---|---|---|---|---|
| R1 | Under a cgo-on load, a package with no `import "C"` loses its plain file under `//go:build cgo`, with exit 0 | `processConversion`, the `if route != nil` beside the constraint re-check | Two conversions of one small package: with `-cgo`, and with `CGO_ENABLED=1` and no `-cgo`. Each exits 0 and writes no `.cs` for that file | The re-check carries the `cgo` tag whenever the LOAD was cgo-on: `-cgo`, or a `CGO_ENABLED=1` the load ran under. The key is the load, not the route |
| R2 | A link line that does not link reports the first probe's error, which lists every symbol | `cgoCapture`, the baseline probe | The real-route arm with `-lgo2csnosuchlib`, tightened: the refusal must carry the linker's words about the link line and must not blame a symbol | Return the baseline probe's own error |
| R3 | The offline test seam appends its own `-tags=cgo`, and go keeps the last `-tags` it is given | `processConversion`, the `cgoOfflineTrees` arm | The offline fixture converted with a user tag, plus a plain file under that tag: the file is not loaded | One `-tags` flag that carries the user's tags and `cgo`. The test seam only; a real `-cgo` load appends nothing |
| R4 | The verified-names rule refuses more than `abs` and `labs` on glibc | a READING, on the hosted linux leg | none: a program that calls `atoi`, `atol`, `toupper`, `tolower`, `fabs` and `strtol`, converted on the real route under the runner's gcc and under the pinned zig | none. The test asserts only the invariant (each call is bound, or refused by name with nothing written) and prints the set. `docs/Limitations.md` records the set as measured |
| R5 | A cgo project built for a target other than its bindings' fails far from the cause | the cgo project's own `.csproj` (`cgoTargetDefaults`) | The offline fixture, converted for linux/amd64, built with `-p:GoTargetOS=windows`: compile errors, none of them go2cs's | The project carries a target that refuses the build by name, with both targets, before the compiler runs |

**R5 has a second half, proposed here.** A cgo project's default target is its bindings' target, but that default
reaches only the project itself. Its references to the standard library take their own default (windows/amd64). So a
plain `dotnet build` of a cgo program converted on linux compiles linux bindings against the windows standard
library, and says nothing. The fix is in the same file: the cgo project hands its one target to its project
references. The exit gate's tests pass the target by hand on every build, which is why the gate did not show this.

### 15.2 Predictions, to be scored as worded

- **P3.0-1.** At the base, both of R1's conversions exit 0 and neither writes the `.cs` of the file under
  `//go:build cgo`. After the fix both write it.
- **P3.0-2.** At the base, R2's refusal names at least one C symbol of the package. After the fix it names none, and
  it still names the library.
- **P3.0-3.** At the base, R3's file under the user's tag is not converted. After the fix it is. No golden of an
  existing offline test moves.
- **P3.0-4.** Under the runner's gcc on linux, all six of R4's functions are refused by name. Under the pinned zig
  the same six are refused; this half is the weaker one, because `strtol`'s rename depends on the glibc version zig
  targets.
- **P3.0-5.** At the base, R5's build fails with compiler errors (CS-coded) and no error that names go2cs. After the
  fix it fails with exactly one error, go2cs's, naming both targets, and no CS-coded error.
- **P3.0-6.** At the base, the offline fixture built with no target property takes the windows standard library
  beside its linux bindings. After the fix it takes the linux one, and a build that passes the target by hand is
  unchanged.
- **P3.0-7.** 0 corpus bytes on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64. `check-no-regression`
  stays byte-identical. The only goldens that move are the cgo fixtures' project files, by the refusal target and the
  handed-down target.
- **Most likely red that is not the mechanism:** R1 under `-cgo`. A package with no `import "C"` has no route, and
  `-cgo` may refuse or behave otherwise for a reason this plan has not read. If so the reading is posted before any
  fix is shaped to it.

### 15.3 Gates

- No golib change and no go2cs-gen change is planned, so GolibTests and GenTests are not owed. If either tree is
  touched, its full suite is.
- Owed: the full `go test ./...` (repoguard and the comment census inside it), `check-no-regression`, and the
  two-seeded corpus comparison on four targets, because R1 changes a path every conversion takes.
- Hosted, once, at the tip: `converter-cgo` on linux both ways (the runner's gcc and the pinned zig) for R2 and R4;
  `cgo-e2e` on linux for R5's second half, where the windows default is the wrong one.

### 15.4 Stop lines

- **S3.0-A.** A corpus byte moves: stop and attribute before anything else (S-E).
- **S3.0-B.** Handing the target down needs a change outside the cgo project's own `.csproj` (a corpus build file,
  or any converted package's project): stop. The refusal by name lands alone and the second half is posted.
- **S3.0-C.** R1 does not reproduce as read: post the reading. No fix is invented for a red that is not there.
- **S3.0-D.** R1's fix moves any golden outside the cgo tests: stop.
- **S3.0-E.** R4 shows a call that is neither bound nor refused by name: that is a defect of the rule, not a reading.
  Stop and post.

### 15.5 Record 2026-11-12: increment 3.0 as built and measured

COORD approved the plan as posted: R1 to R5, P3.0-1 to 7, and stop lines S3.0-A to E. R5's second half was approved
inside S3.0-B. COORD added one gate to P3.0-7: the full behavioral suite, once at the tip, because R1 changes a path
every conversion takes.

**Commits, on the exit gate's tip `0001be70a7`.** The design merge and this plan (`c5d4525e48`, `24894b8199`); the
reds, tests only (`3c19eb5ed5`); the fixes (`fb9deb5ab4`); two commits that restore one file's line count
(`a7f767b9a8`, `1b9f558b9f`, see the disclosures). A hosted leg runs a branch, so the red commit is also the side
ref `claude/h-cgo-30-red`. It is an ancestor of this branch, and COORD deletes it once 3.0 is on master.

**The reds, read at `3c19eb5ed5`.** H has no C compiler, so R2 and R4 are read on hosted linux only.

| | Read on | The reading at the red commit |
|---|---|---|
| R1 | H | The control arm (a cgo-off load) passes. Under `-cgo`, and under `CGO_ENABLED=1` with no `-cgo`, the conversion exits 0 and writes `value.cs` and no `on.cs` |
| R2 | hosted linux, the runner's gcc (run 43267366259) | The refusal reads `ld.bfd: /tmp/….o: reference to cos`: a symbol named beside a library that is not there |
| R2 | hosted linux, the pinned zig (run 43267397145) | The test PASSES at the red commit. zig's linker reports only `unable to find dynamic system library "go2csnosuchlib"` and traces no symbol |
| R3 | H | The plain file under the user's tag is not converted |
| R4 | hosted linux, both compilers | Refused as not imported: `atoi`, `atol`, `fabs`, `tolower`, `toupper`. Refused by name for another reason: `strtol`. Bound: none |
| R5 | H | The build for windows/amd64 fails with CS0051 on the first C type in an exported signature, and with no error of go2cs's |
| R5, second half | H | With no target named, the runtime beside the linux/amd64 fixture is the windows one. With the target passed by hand it is the linux one |

**The fixes, as built.**

- **R1.** The constraint re-check carries the `cgo` tag whenever the load was cgo-on (`cgoOnLoad`): under `-cgo`, or
  under a `CGO_ENABLED=1` in the load's own environment. The key is the load, not the package's route.
- **R2.** `cgoCapture` returns the baseline probe's own error.
- **R3.** The offline seam builds one `-tags` flag from the user's tags and `cgo` (`cgoOfflineBuildFlags`).
- **R4.** No fix, as planned. `docs/Limitations.md` records the set as measured.
- **R5.** `cgoTargetDefaults` adds two blocks to a cgo package's project (`cgoTargetBlocks`). A target named
  `GoCheckCgoTarget` refuses a build for another target with one error that names both targets, before the compiler
  runs. An item definition hands the project's target to its project references. Both are in the cgo project's own
  `.csproj`, so S3.0-B did not fire.

**The hosted legs at the tip `1b9f558b9f`, each `success`.**

| Stage | Compiler | Run | Reading |
|---|---|---|---|
| `converter-cgo`, linux | the runner's gcc 15.2.0 | 43274363633 | 128 PASS, 0 FAIL, 7 SKIP. R2's refusal reads `cannot find -lgo2csnosuchlib` and names no symbol. R4's set is the red commit's |
| `converter-cgo`, linux | the pinned zig (clang 22.1.8) | 43274396564 | 128 PASS, 0 FAIL, 7 SKIP. R4's set is the same |
| `cgo-e2e`, linux | the pinned zig | 43274368577 | 28 PASS, 0 FAIL, 0 SKIP. Both of R5's tests pass: the windows build is refused by name, and both arms of the no-target test read the linux runtime. Every arm of the exit gate reads as it did |

The 7 SKIPs of `converter-cgo` are the build tests and the exit gate's, which belong to `cgo-e2e` and run there.

**The battery on H at `1b9f558b9f`, one run, exit 0.**

| Stage | Reading |
|---|---|
| `go test ./...` | 11 packages ok, 0 FAIL, 2,111 s. repoguard and the comment census are inside it |
| `check-no-regression` | Byte-identical across 1,035 behavioral packages; the 8 platform-exclusive packages are skipped by name, as at the base |
| The full behavioral suite | PASS, 951 projects: Transpile 951, Compile 951, Target 951, Output 921 with 30 skipped. These are the exit gate's battery's counts. No NOT MEASURED line. No tracked file changed or deleted |
| The corpus comparison, A = `0001be70a7`, B = `1b9f558b9f` | windows/amd64, linux/amd64, darwin/amd64, linux/arm64: 0 files differ on each. Every conversion exits 0 with 0 degraded packages. The two converter binaries differ |

**The predictions, scored as worded.**

| | Score | Note |
|---|---|---|
| P3.0-1 | MET | Both halves |
| P3.0-2 | MET under gcc, MISSED under zig | The base half assumed a linker that traces symbols. zig's does not, so its refusal named no symbol before the fix either. The after half holds on both |
| P3.0-3 | MET | No existing offline test's output moved |
| P3.0-4 | MET as worded on both compilers; as meant, five of six | All six are refused by name. `strtol` is refused for its `char **` parameter, which Phase 2 does not cross, before the verified-names rule reads it. So its rename is NOT MEASURED |
| P3.0-5 | MET | At the base CS0051 and no go2cs error. After the fix one error, go2cs's, naming both targets, in about 8 s on H and 4 s on the hosted leg |
| P3.0-6 | MET | On H and on hosted linux |
| P3.0-7 | MET | 0 corpus files on four targets, `check-no-regression` byte-identical, and no committed golden moved. The third clause named a move with nothing to move: the cgo fixtures' project files are written by the tests and are not committed |

The named risk outside the mechanism (R1 under `-cgo`) did not fire. No stop line fired.

**Disclosures.**

- **A line-count fault, caught by repoguard before any push.** The fixes at `fb9deb5ab4` shifted lines in
  `conversionDriver.go`, and four provenance notes in the docs cite that file by line. `a7f767b9a8` moved the new
  helper to `cgoRoute.go`, and `1b9f558b9f` cut a comment back to its base's three lines, so the file has its base's
  line count again and the notes resolve. Nothing was pushed in between.
- **`strtol`'s rename stays NOT MEASURED** until increment 3.1 admits its parameter.
- **GolibTests and GenTests were not run.** 3.0 changes neither tree.
- **This record is a docs-only commit past the battery's tip.** It is gated alone: repoguard and the comment census.

— H (2026-11-12)

### 15.6 Amendment 2026-11-13: a defect of R5's second half on darwin/arm64, found after 3.0 landed

3.0 landed in batch 292 (`4423d66052`). Increment 3.1's tip leg on darwin then read this (run 43335132134): on
`osx-x64` every test passed; on `osx-arm64` the exit gate's program and 3.1's program both failed at the build, in
10 s, with no compiler error:

    Directory.Build.props(61,5): error : go2cs: darwin/arm64 is not a target the converted standard library is
    laid out for (windows/amd64, linux/amd64, darwin/amd64, linux/arm64)

**The cause is R5's second half.** A cgo project hands its own target to its project references. The standard library
has no darwin/arm64 flavour: its darwin/amd64 flavour serves both of darwin's architectures, and before R5 the
references took it by default. Handed `GoTargetArch=arm64`, the library's own universe check refuses the build. Every
cgo project on a darwin/arm64 host was affected. linux, windows and darwin/amd64 were not.

**Why no gate saw it.** 3.0 was gated with `cgo-e2e` on linux only (15.3 says so), and so was batch 292. R5 changed
what every cgo project hands down on every operating system, and it owed a leg on each. P3.0-6 was scored MET on
the two hosts it was read on. It was never read on darwin.

**The fix** (`claude/h-cgo-target-handdown`, cut from master `130cecb136`): the references are handed the target the
standard library is LAID OUT for (`cgoReferenceTarget`): the package's own pair when it is in the declared universe,
and otherwise the flavour the library has for that operating system (darwin/arm64 takes darwin/amd64). The
project's own refusal of a build for another target is unchanged, and still names the bindings' own pair.

**Red first.** The hosted leg is the red. On H a text arm reads the handed-down pair for five targets
(`TestACgoProjectHandsItsReferencesATargetTheStandardLibraryIsLaidOutFor`): red on master's route for darwin/arm64
alone, green with the fix.

**The lesson, COORD's and H's:** a seat that changes a build-time hand-down or default owes its end-to-end leg on
EVERY operating system it touches, and the union gate re-runs that set.

— H (2026-11-13)

## 16. Amendment 2026-11-12: increment 3.1, its plan, predictions and stop lines (posted before code)

Increment 3.0 is accepted and seats in batch 292 (`eabb7aad2c`, master merged in). COORD ordered the 3.1 plan next,
built on M1 to M4 of section 13. No code is written. Each statement about the tree is marked READ (read at
`eabb7aad2c`) or NOT READ (the red measures it).

3.1 is the helpers, `void *`, and the deferred call for any bound function (sections 3 and 12). It hands C no Go
memory: there is no pin and no image in it. Every pointer into Go memory that reaches a 3.1 door is refused by name
before the call, and increment 3.2 turns that one arm into pin, image or refuse.

### 16.1 What 3.1 admits

**A. The helpers.** `C.CString`, `C.CBytes`, `C.GoString`, `C.GoStringN` and `C.GoBytes` are refused today by name
(`cgoBuiltinNames`, READ). Each becomes a member of the package's C class in its bindings file.

- The copies are golib functions over an address, and they copy BYTES (S5). No text encoding is on the path.
- The allocation is the package's own `malloc`, bound in the bindings file from the captured library like any other
  symbol (Q-D, ruled). cmd/cgo reaches `malloc` through its own C stub (`_cgo_cmalloc`, READ), so the package's
  binary imports it whenever a helper allocates. `C.free` is the user's own call and needs `void *` (B).
- `C.CString` and `C.CBytes` return a native box, as a C function's pointer result does today (READ).
- `C.GoString(nil)` is the empty string. A nil result from `malloc` ends the program as cmd/cgo's stub does. That
  arm is NOT MEASURED: no test can make `malloc` fail.
- The pointer a `Go*` helper is handed is classified as in B. C memory is read. Go memory is refused by name in 3.1
  (question Q2).

**B. `void *`.** A parameter of Go type `unsafe.Pointer` is refused today (`cgoScalarOf`, READ). It becomes one
golib question, asked for every such argument before anything else happens (M4, step 1):

| What the pointer carries | 3.1 |
|---|---|
| nil | C receives 0 |
| A genuinely native box (the predicate Phase 2 already uses: no retained source, no order token, no pin on managed storage) | its address |
| A box of Go memory, of any kind | refused by name, before the call (3.2's arm) |
| A native box minted from managed storage (M2) | refused by name. Its address is never handed to C |
| A number with no box, known to golib's pin and token registry (M3) | classed as the box it names, so refused in 3.1 |
| A tagged token (M3) | refused by name |
| A number the registry does not know | crosses as it is |

A `void *` result is C memory: a native pointer, or nil for 0. With M4 there is nothing to unwind in 3.1: a refusal
in any argument throws before the transition bracket and before any other argument has an effect.

**C. The deferred call (Q-B, ruled for any bound function).** The route refuses `defer` on a check closure today
(READ). It recognizes cmd/cgo's deferred shape, `defer func() func() { _cgo0 := x; return func() {
_cgoCheckPointer(_cgo0, nil); _Cfunc_f(_cgo0) }}()()`, for any function the route binds and for the argument forms
the immediate closure already admits. It lowers it to a deferred call of the bound wrapper whose arguments are
evaluated at the `defer`. The `go` form stays refused.

**D. `strtol`'s reading, owed from 3.0.** `strtol`'s second parameter is a `char **`, the out-parameter, which the
increments table gives to 3.2. So 3.1 takes the reading where the rule reads it: a test builds the one-call
package on the real route, reads the names cmd/cgo's own binary imports, and prints which of `strtol` and
`__isoc23_strtol` is there. It asserts only that the rule's verdict follows that set (question Q1).

### 16.2 What 3.1 does not do

No Go memory is handed to C, pinned or imaged (3.2). No C memory is read through Go pointer syntax beyond what Phase
2 reads (section 6, 3.2). No slice view (3.3). No out-parameter (3.2). C calling Go stays refused.

### 16.3 The fixture, the probe and the reds

- **The first red needs no new file.** The Phase 1 fixture's committed cmd/cgo trees already hold `C.CString`,
  `C.GoString` and `defer C.free(unsafe.Pointer(cs))` (READ). Converted through the offline seam on H, they are
  the red P3.1-1 predicts (NOT READ: no test converts those trees through the route today).
- **A Phase 3 fixture** (`testdata/cgo/phase3`), made as Phase 2's was: `fixture.go` by hand, cmd/cgo's two files
  from a hosted linux leg's `REGEN` lines, committed from that run. H has no C compiler, so the order is: commit the
  source and the regeneration test, read the leg, commit the trees with the red tests, then the fixes.
- **Probe functions**, added to the probe library's one C file: return a static C string; return a `malloc`ed copy
  the caller frees; fill a caller's C buffer; sum `n` bytes behind a `void *`; return a `void *` it was handed; a
  call log the deferred arms read back. Additive: no existing function changes.
- **An end-to-end program**, built by go and converted by go2cs as the exit gate's is, its two outputs compared byte
  for byte on linux, windows and darwin. Its rows:
  - a round trip of each helper, both directions, over plain text, invalid UTF-8 and a string with an interior NUL;
  - `GoStringN` and `GoBytes` with a count of zero, and with a negative count. The negative rows run in a child
    process, since Go may end the program there; the comparison is the exit status and the kind of failure;
  - `CBytes`, read back and freed by the caller;
  - `void *`: nil, C memory, and C memory through a `uintptr` variable;
  - the deferred call: two deferred C calls and a deferred Go call, read back in order from the call log; and an
    argument reassigned after the `defer`, which must reach C with the value it had at the `defer`.
- **Refusal arms, go2cs only** (Go allows them, and 3.2 admits them): a `[]byte` element, a struct box, and
  `unsafe.Pointer(uintptr(unsafe.Pointer(&b[0])))` in one expression and through a variable, each handed to a
  `void *`. Each must be refused by name before the call. None may cross as a number.
- **golib arms** (GolibTests): each copy function; and each row of B's table, with a control: a number the registry
  does not know crosses, so the refusal arms can fail.

### 16.4 Predictions, to be scored as worded

- **P3.1-1.** At the base the Phase 1 trees are refused, with nothing written, and the refusals name `C.CString`
  and `C.GoString` as cmd/cgo's own helpers, `C.free`'s parameter as not a scalar, and the deferred call through
  the check closure. After 3.1 none of those four is among the refusals. The fixture's other refusals (a C function
  used as a value, a `#define` that is not a constant) stand.
- **P3.1-2 (P1 of section 9, on the legs run).** cmd/cgo's binary imports `malloc` and `free` under their plain
  names on hosted linux/amd64, windows/amd64 and darwin, so neither is refused by the verified-names rule.
- **P3.1-3.** Every row of the end-to-end program is byte-identical between the go arm and the go2cs arm on the
  three hosts. The weakest rows are the negative counts: how Go fails there is NOT READ, and the row is read on the
  go arm before go2cs is shaped to it.
- **P3.1-4.** go2cs already evaluates a deferred call's arguments at the `defer` for this emission: the reassigned
  argument reaches C with its value at the `defer`, with no change to the converter's `defer` emission.
- **P3.1-5.** Every refusal arm of 16.3 is refused by name before the call, and the control crosses. In particular
  the one-expression `uintptr` form arrives carrying its box or is found in the registry; it never crosses as C
  memory.
- **P3.1-6.** 0 corpus bytes on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64. `check-no-regression`
  stays byte-identical. No behavioral golden moves. GolibTests' total rises by the new arms only and no existing
  row changes (P4).
- **P3.1-7 (P7).** On each new golden, externs equal transition brackets, plus windows' one errno slot.
- **P3.1-8.** Under the runner's gcc, cmd/cgo's binary for the `strtol` package imports `__isoc23_strtol` and not
  `strtol`. Under the pinned zig it imports plain `strtol`. The zig half is the weaker one: it depends on the glibc
  version zig targets by default.
- **P3.1-9.** The only existing tests whose expectations move are cgo tests that assert one of P3.1-1's four
  refusals. They are named in the red commit.
- **Most likely red that is not the mechanism:** the windows leg's `malloc` and `free`. A helper's allocation and
  the user's `C.free` must reach the SAME C runtime. If the capture binds them in two different libraries there,
  the round trip corrupts a heap and no row says why. The windows leg prints both libraries before any row runs.

### 16.5 Gates

- golib is touched, so the full, unfiltered GolibTests is owed, read against the base's results, and `go2cs.slnx`
  is built once. go2cs-gen is not planned to be touched; if it is, its full suite is owed and S3.1-B fires first.
- The full `go test ./...` (repoguard and the comment census inside it), `check-no-regression`, the full behavioral
  suite once at the tip, and the two-seeded corpus comparison on four targets.
- Hosted. At the red: `converter-cgo` on linux, once. At the tip: `converter-cgo` on linux both ways, windows and
  darwin; `cgo-e2e` on linux, windows and darwin.
- The classifier is the safety boundary of 3.1, so the adversarial review COORD ordered for Phase 3 is asked for at
  the tip, before landing.

### 16.6 Stop lines

- **S3.1-A.** A corpus byte moves, or a golden outside the cgo tests: stop and attribute (S-E).
- **S3.1-B.** The helpers or the classifier need a new box kind, instance state on `ж<T>`, or a go2cs-gen change:
  stop and post the design (S-B, S-C).
- **S3.1-C.** `malloc` or `free` is not imported under its plain name on a host, or the two bind in different C
  runtimes: stop. The allocator ruling returns to COORD. No .NET allocator is substituted.
- **S3.1-D.** P3.1-4 fails: go2cs does not evaluate this deferred call's arguments at the `defer`. That is a
  converter reading outside cgo. It is posted, and the lowering is not shaped around it.
- **S3.1-E.** Any arm the tests can build lets an address of Go memory, or a number minted from one, cross a
  `void *` as C memory, beyond the released-pin residue section 13.4 names: stop. `void *` stays refused.
- **S3.1-F.** A row needs Go memory handed to C to pass: it is 3.2's row. Nothing of 3.2 is pulled forward.
- **S3.1-G.** A go arm and a go2cs arm differ on a row other than the negative counts: stop and post the reading.
  A negative-count difference go2cs cannot match is disclosed by name, never dropped from the program.
- **S-F** stands: 3.1 yields to any .4 seat that needs H.

### 16.7 Questions for COORD

- **Q1. `strtol`'s reading.** Lean: at the rule's own input, as in 16.1 D, with the conversion-level reading in 3.2
  when the out-parameter lands. The other way is to admit a pointer-to-pointer parameter in 3.1, nil and C memory
  only.
- **Q2. Go memory handed to a `Go*` helper.** `C.GoBytes(unsafe.Pointer(&b[0]), n)` is legal Go: a Go function
  reads it, and C is not involved. Lean: refused by name in 3.1 and admitted in 3.2, because reading it needs the
  source classification that 3.2's design step defines (M2).
- **Q3. The branch.** Lean: `claude/h-cgo-phase3-helpers`, cut from `eabb7aad2c`, with master merged in after 292
  lands.

### 16.8 Amendment 2026-11-12: COORD's rulings, and the reds as read on H

COORD approved the plan as posted and ruled the three questions as leaned.

- **Q1, RULED:** `strtol` is read at the rule's own input in 3.1. A pointer-to-pointer parameter waits for 3.2's
  out-parameter. P3.1-8 is scored as worded, the weaker zig half included.
- **Q2, RULED:** Go memory handed to a `Go*` helper is refused by name in 3.1 and admitted in 3.2 under M2's source
  classification. The refusal names the shape (`C.GoBytes` over Go memory) and says it is increment 3.2's, so a user
  who meets it reads a known, dated gap and not a defect. That program joins the refusal arms, with its control.
- **Q3, RULED:** the branch is `claude/h-cgo-phase3-helpers`. Master is merged in, never rebased, after batch 292
  lands.
- **The windows two-runtimes risk** stays a stop line as written (S3.1-C): if `malloc` and `free` bind in different
  C runtimes it is posted and COORD rules the allocator.
- **The adversarial review** at the tip, before landing, is ordered, on the classifier as the safety boundary.

**One sentence of 16.3 was wrong, and is corrected here.** It said no test converts the Phase 1 trees through the
route. One does (`TestCgoPhase1FixtureIsRefusedNamingEachUseAndItsOwner`), and it already asserted three of P3.1-1's
refusals. The Phase 2 fixture also holds a deferred call through the check closure (`Deferred`), refused the same
way. So the first reds are those tests with their expectations turned over, and they are read on H.

**The reds at the red commit, on H (offline).**

| Test | Reading |
|---|---|
| `TestCgoPhase1FixtureIsRefusedNamingEachUseAndItsOwner` | `C.CString` and `C.GoString` are refused as cmd/cgo's own helpers. `C.free` is refused as a deferred call through the check closure |
| `TestCgoPhase2FixtureIsRefusedNamingOnlyItsGoroutineCall` | 2 refusal lines, want 1: the deferred call is refused beside the goroutine call |
| `TestCgoDeferredCallThroughTheCheckClosureConvertsAsADeferredCall` | the conversion is refused |
| `TestCgoPhase2FixtureConvertsAsTheSourceReads` | red with them: the fixture it converts now keeps its `Deferred` function |

**P3.1-1's base half, scored as worded: three of four.** `C.CString`, `C.GoString` and the deferred call read as
predicted. `C.free`'s parameter is NOT named as "not a scalar": the deferred refusal stops the census at the `defer`,
so the `void *` parameter behind it is never reached. That text is read where a `void *` call is not deferred.

**The end-to-end program's arms (16.3), as built.** The plan's element arm (`&b[0]` handed to a `void *`) is refused
at CONVERSION today, by the check closure's own element form, and stays so until 3.2. A program that holds it cannot
convert, so it is not an arm of the program. The run-time refusal arms are the address of a Go variable through a
`void *`, the same as a number in a variable, the same through a `uintptr` in one expression, and `C.GoBytes` and
`C.GoString` over Go memory.

### 16.9 Amendment 2026-11-12: the green as built, a stop line that fired, and three crossings found on the way

**The green** (`43ac9e33fa`), as 16.1 planned it, with these readings.

- **The deferred call** is lowered in the tree, before anything else reads it: the statement's call becomes cmd/cgo's
  own call of the C function, each argument the expression its closure variable was defined as. The emission is the
  converter's ordinary `defer`: `defer(C.free, @unsafe.Pointer.FromPinnedBox(cs), ref ᒐ);`. P3.1-4 is MET on H and on
  both early legs (the reassigned argument reaches C as 1; the log reads `321`).
- **`C.malloc`** is cmd/cgo's wrapper `_Cfunc__CMalloc`, which the classifier filtered as plumbing and now reads as
  `C.malloc`.
- **How Go fails on a negative count** was read on the go arm before go2cs was shaped (run 43325908295):
  `C.GoStringN(p, -1)` is `fatal error: out of memory`, exit 2; `C.GoBytes(p, -1)` is `panic: runtime error: gobytes:
  length out of range`, exit 2; `C.GoBytes(p, 0)` is an empty slice that is not nil. go2cs does each the same.
- **P3.1-9 MISSES as worded.** One test outside the predicted set moved: the classifier test counted
  `_Cfunc__CMalloc` as plumbing.
- **A widening beyond the plan's words, ACCEPTED by COORD.** A deferred or started GO function that calls C further
  in (`defer func() { C.f(&x) }()`) was refused by the old, broad test. It is an ordinary `defer` or `go`, and
  converts. Only cmd/cgo's own `go` form on the check closure stays refused. COORD required one arm: the inner call
  over Go memory is still refused, at run time, by name (`goinnerdefer`, `goinnergo`, each beside its C-memory
  control).
- **S3.1-B did NOT fire** (COORD agrees). golib gains one default member on the internal `INativeRooted` interface
  and its implementation on `NativeBox`: no field, no instance state, no new box kind, no go2cs-gen change. Both
  box files keep their cited lines.

**A defect of the first classifier, caught by reading the emission before any leg.** `unsafe.Pointer(cs)` over a C
pointer is emitted as the pinning mint, which CARRIES the native box. The first cut refused every pointer that
carried a box, so it would have refused `C.free(unsafe.Pointer(cs))`. Its arms passed because they minted the
pointer another way. A native box now answers, without its pointee type, whether it is C memory alone, and every
control uses the converter's own mints.

**Stop line S3.1-E FIRED, and COORD ruled it (C, with conditions).** COORD asked of the classifier: can ANY box
answer "C memory alone" when its storage is managed? The arm hands the door every pointer golib has over managed
storage, four ways each: the pinning mint, the retaining mint, the bare mint, and its number in a variable.

| Source | Carrying its box | As a bare number |
|---|---|---|
| a view aliasing a struct | refused | refused |
| a view over a struct that holds references (a token) | refused | refused |
| a native box with a retained source | refused | refused |
| a native box that aliases an order token | refused | refused |
| `unsafe.StringData`, `unsafe.SliceData` | refused | refused |
| a pointer rebuilt from a Go array's number (below) | refused | refused |
| **a wider view pinned over an element** (`(*int32)(unsafe.Pointer(&b[4]))`) | refused | **CROSSES** |
| **a native box over a pinned managed buffer** | refused | **CROSSES** |

So the answer is NO for a box and YES for a number: 4 of 36 hand-overs cross. The door's test for a bare number is
the pin and token registry, and two pin moments register nothing. An interior address (a number plus an offset)
misses the registry for every pin, since its entries are exact addresses.

**The residue, disclosed by name** (COORD's condition 1), beside section 13.4's: *a pointer rebuilt from a number
that points into Go memory crosses a `void *` unchecked.* It takes an explicit `uintptr` in the source. The
one-expression form, `unsafe.Pointer(uintptr(unsafe.Pointer(p)) + n)`, is legal Go. The harm is the syscall pin hole
through a new door: C is handed Go memory whose only pin holder is a temporary, so the collector may release the pin
during the call. It is a known memory-safety gap, and `docs/Limitations.md` and the release notes say so in those
words. 3.1 opens no new way to MINT such a number.

COORD's other conditions:

- **The arm asserts exactly the four crossings and the 32 refusals**, so a fifth crossing is a red
  (`APointerOverManagedStorageCrossesAVoidPointerOnlyAsTheFourDisclosedNumbers`).
- **A golib seat proposal follows 3.1**: register at the two pin moments, and give a registry entry its length so a
  number inside ANY live pinned range is refused. Its allocation-meter question on banked rows is named up front.
- **Section 13.4's first rule** (in one expression the converter keeps the box) is the FIRST item of 3.2's design
  step. 3.1 does not implement it: it leans on the registry, which holds for exact numbers.
- **The review at the tip takes the residue as an explicit question**, with this list of where a number over Go
  memory can come from. golib's pin moments: the box-to-`uintptr` and box-to-`void *` operators (four sites, each
  registers); `ElemRefBox.TryPinnedReinterpret` (registers nothing); `PinnedBuffer`'s constructors (register
  nothing). The converter's mints that involve no pin: `FromRef`, and a carrying pointer converted to `uintptr`,
  which yields its number.

**Two more crossings, found and CLOSED in 3.1, not disclosed.**

1. **A pointer rebuilt from a Go array's number was taken for C memory** (read on the early end-to-end legs,
   43330710728 and 43330721939, where every other arm matched Go: `C.GoString((*C.char)(unsafe.Pointer(&x)))` over a
   Go array was read, not refused). That conversion goes through the array's number, and the number-to-pointer
   operator mints a native box with no source, no token and no pin. The number is in the registry, so the predicate
   gained one clause: a native box whose address the registry knows as a live Go box's is not C memory. Phase 2's
   struct-pointer door shares the predicate, so this tightens Phase 2 too: a struct pointer rebuilt from a Go array's
   number was handed to C as native, a latent Phase 2 hole. COORD ruled that an exit-gate arm that moves for it is a
   Phase 2 correction, recorded in the exit gate's own record, not a 3.1 regression. The golib arm was regressed
   (the clause
   removed), went red, and was restored byte-identical.
2. **A method's own receiver type, with no `uintptr` in the source.** Inside a method with a pointer receiver,
   `unsafe.Pointer(p)` of a pointer of the receiver's own type is minted from a reference: `func (h *T) M() {
   C.f(unsafe.Pointer(h)) }` emitted `C.f((uintptr)@unsafe.Pointer.FromRef(ref h))`, a bare number of storage
   nothing pins, in no registry. The cgo route refuses it at conversion, by name, with the way round (a function
   that takes the pointer as a parameter mints it with its box). It over-refuses a conversion that never reaches C,
   in a cgo package only. COORD accepted it as sound, and ruled the over-refusal OWED: it narrows in 3.2, when the
   pin scope exists, and is not permanent. Red first on H: five arms red at the green commit, green with the fix,
   and the control
   converts. For a LOCAL copy the same arm emits `FromRef(ref p)`, the address of the local itself: a converter
   defect outside cgo, filed on the BOARD.

**A freed C pointer still crosses**, as Go lets it: Go has no check for a C pointer used after `C.free`, and go2cs
adds none. The door classifies and never dereferences (`AVoidPointerToFreedCMemoryStillCrossesAsGoLetsIt`).

— H (2026-11-12)

### 16.10 Record 2026-11-13: increment 3.1 as measured, and its predictions scored as worded

**The trees.** The battery read `3271559fbf` (16.9's tip). Master then landed batch 294 (`7c7dc687f8`), which carries
15.6's fix, and was merged in with no rebase at `c4b5d9613c`. The merge resolved two append-against-append conflicts
with both sides kept (this record and the BOARD), and carries one test-only change (P3.1-9 below). The hosted legs
below were read at `c4b5d9613c` unless a line names another tree.

**The battery on H at `3271559fbf`**, one stage at a time:

| Stage | Reading |
|---|---|
| `go test ./...` | rc 0, 11 packages ok, 0 cached, 1,474 s |
| GolibTests, full and unfiltered | base `eabb7aad2c` 2,570 passed, 43 skipped of 2,613; tip 2,581 passed, 43 skipped of 2,624. The TRX difference is exactly the 11 new arms, and no existing row changed |
| `go2cs.slnx` | NOT a reading of 3.1: 17,714 errors at the base and at the tip, the same set. An instrument finding, routed by COORD to R. H does not claim the solution builds |
| `check-no-regression` | rc 0, byte-identical across all 1,035 behavioral packages |
| the full behavioral suite | rc 0, PASS, 951 projects: Transpile, Compile and Target 951 of 951; Output 921 passed, 0 failed, 30 skipped, 0 timed out. No NOT MEASURED line. The tree was clean after it, so no golden moved |
| two-seeded `-stdlib` comparison, A = `eabb7aad2c`, B = `3271559fbf` | windows/amd64, linux/amd64, darwin/amd64 and linux/arm64: 0 files, 0 diff lines on each. Every conversion exited 0, with 0 degraded packages |

The comparison line was made to fail once after the run: on two seeded copies it read 0 files, and with one line
appended to one file of one copy it read 1 file, and named it.

**The hosted legs.** Counts are distinct test and subtest names, passed / failed / skipped.

| Leg | Tree | Run | Reading |
|---|---|---|---|
| `converter-cgo` linux, the runner's gcc 15.2.0 | `c4b5d9613c` | 43365489712 | 145 / 0 / 9 |
| `converter-cgo` linux, the pinned zig (clang 22.1.8) | `c4b5d9613c` | 43365503641 | 145 / 0 / 9 |
| `converter-cgo` windows, gcc 15.2.0 | `c4b5d9613c` | 43365493831 | 143 / 0 / 12 |
| `converter-cgo` darwin, Apple clang 17 | `c4b5d9613c` | 43365497275 | osx-x64 141 / 0 / 13; osx-arm64 141 / 0 / 13 |
| `cgo-e2e` darwin | `c4b5d9613c` | 43365500071 | osx-x64 53 / 0 / 0; osx-arm64 53 / 0 / 0 |
| `cgo-e2e` linux, the pinned zig | `3271559fbf` | 43335108528 | 53 / 0 / 0 |
| `cgo-e2e` windows | `3271559fbf` | 43335120737 | 51 / 0 / 0 |

At `3271559fbf` the four `converter-cgo` legs each failed one test (43335068049, 43335078194, 43335088003,
43335098494), the stale expectation of P3.1-9, and `cgo-e2e` on osx-arm64 failed at the build for 15.6's defect
(43335132134). Both are closed at `c4b5d9613c`. `cgo-e2e` on linux and windows was NOT re-read at `c4b5d9613c`:
COORD ordered `converter-cgo` on the four hosts and `cgo-e2e` on darwin at the merged tip, and the union gate reads
the other two again.

**The scores.**

| Prediction | Score |
|---|---|
| P3.1-1 | Base half: THREE OF FOUR as worded. `C.CString` and `C.GoString` were named as cmd/cgo's helpers, and the deferred call was named. `C.free` was named by the deferred refusal, not by its parameter. After half: MET, none of the four is refused. The fixture's other two refusals stand |
| P3.1-2 | MET on every host read: cmd/cgo's binary imports `[free malloc]` under their plain names with linux gcc 15.2.0, linux clang 22.1.8 (zig), windows gcc 15.2.0, and Apple clang 17 on darwin/amd64 and darwin/arm64 |
| P3.1-3 | MET on the three hosts: each of the program's 24 arms reads the same on the go arm and the go2cs arm on linux, windows and darwin. darwin/arm64, a fourth target, is MET at `c4b5d9613c` and was NOT MEASURED before it (15.6). How Go fails on a negative count was read on the go arm first (16.9) |
| P3.1-4 | MET: the reassigned argument reaches C with its value at the `defer` (`321`), with no change to the converter's `defer` emission |
| P3.1-5 | First sentence MET: every refusal arm of 16.3 is refused by name before the call, and each control crosses. Second sentence MISSED as worded: the one-expression `uintptr` form is refused when its number is exact, and CROSSES when arithmetic moves it inside the storage, as do the four hand-overs of 16.9's table. That is stop line S3.1-E, ruled by COORD and disclosed (16.9) |
| P3.1-6 | MET on every clause: 0 corpus bytes on four targets, `check-no-regression` byte-identical, no behavioral golden moved, GolibTests up by the 11 new arms only |
| P3.1-7 | MET on the one new golden, the Phase 3 fixture's bindings: 10 externs, 10 transition brackets, read on H and on every `converter-cgo` leg. The windows errno-slot clause is NOT MEASURED: no new golden is a windows one |
| P3.1-8 | MET on both halves: under the runner's gcc cmd/cgo's binary imports `__isoc23_strtol` and not `strtol`; under the pinned zig it imports plain `strtol`. Also read: windows gcc and Apple clang import plain `strtol` |
| P3.1-9 | MISSED, twice. The classifier test counted `_Cfunc__CMalloc` as plumbing (16.9). And the real-route test, which only a host with a C compiler runs, still expected the Phase 2 fixture's deferred call refused: red on all four `converter-cgo` legs at `3271559fbf`, fixed in the merge commit, green on all four at `c4b5d9613c`. Its first subtest now expects the goroutine call alone, and the rest of that test keeps the deferred call in the copy it converts, so each hosted host converts it from cmd/cgo's own output |
| Most likely red (windows' `malloc` and `free` in two C runtimes) | Did NOT happen: the windows leg's round trips are green, 51 / 0 |

**Stop lines.** S3.1-E FIRED and was ruled (16.9). S3.1-A, S3.1-B, S3.1-C, S3.1-D, S3.1-F and S3.1-G did not fire.
No arm of Phase 2's exit gate moved for the registry clause of 16.9, so no Phase 2 correction is recorded.

**Disclosures.**

- **The memory-safety gap of 16.9 stands in 3.1:** a pointer rebuilt from a number that points into Go memory
  crosses a `void *` unchecked. It is named in `docs/Limitations.md` and in the draft release notes.
- **The receiver over-refusal is OWED** (16.9): it narrows in 3.2.
- **The battery read `3271559fbf`, not the merged tip.** At `c4b5d9613c` H read repoguard and the comment census
  (uncached, both ok) and the converter's cgo tests (76 passed, 0 failed, 24 skipped: the 24 need a C compiler or an
  end-to-end build, and are the legs' to read). The union gate covers the rest at the batch.
- **An overlap in run 1.** The gates of 15.6's fix (repoguard and the cgo tests, in another worktree) ran beside the
  battery's `go test` and GolibTests stages. No timing row was in flight: the behavioral suite ran alone.
- **A counting correction.** The counts H posted for 15.6's legs merged subtest names that differ only after
  punctuation: `converter-cgo` darwin was posted as 126 passed and reads 129 by full name. Failures were 0 by either
  count. The table above counts full names.
- **This record is a docs-only commit past the legs' tip.** It is gated alone: repoguard and the comment census.

**Owed after 3.1:** the golib seat proposal of 16.9 (register at the two pin moments, and refuse a number inside any
live pinned range), on its own, with its predictions and its allocation-meter question first; and 3.2's design step,
which opens with section 13.4's first rule.

— H (2026-11-13)

### 16.11 Amendment 2026-11-13: the review found 3.1 UNSOUND as a boundary; the fix plan, posted before code

**The verdict** (the adversarial review COORD launched at `80e8b3c7d8`): golib mints DERIVED boxes that drop their
root's pin and source, and those cross every door as C memory, with their box, and with no `uintptr` in the Go
source. So two sentences of 16.9 are FALSE as written: "the answer is NO for a box", and "it takes an explicit
`uintptr` in the source". They were claims about the boxes golib mints FIRST, written as claims about every box.
The arm enumerated first-generation boxes and never a box derived from one. H's miss. 3.1 does not land until the
findings below are fixed in it. Ruling C covered numbers only, and none of this is a disclosure.

**What held** (the review): every argument is classified before the transition, for all three parameter kinds;
classification has no race for a refused box; and `StringData`, `SliceData` of a Go slice, `&x`, `&t.y`, a named
receiver's fields, value receivers, by-value parameters, exact numbers and a single reinterpret are refused as
intended. Not examined: variadic C functions, reflect's `UnsafePointer` and `UnsafeAddr`, sync/atomic, syscall
buffers.

**The findings.**

| # | The Go | What golib did | Crosses |
|---|---|---|---|
| 1 | `h := (*hdr)(unsafe.Pointer(&b[0])); C.f(&h.n)` | a field of a native box is a fresh native box at the field's address, with no pin and no source | all three doors; over a Go array at any offset but 0 |
| 2 | `(*C.char)(unsafe.Pointer(h))`, a second reinterpret | reinterpreting a native box mints a fresh native box at its address | `C.GoString(q)`, `unsafe.Pointer(q)` |
| 3 | `unsafe.Slice((*int32)(unsafe.Pointer(&b[0])), 4)`, then `&sl[1]` or `unsafe.SliceData(sl)` | an element of a native-backed slice is a fresh native box | the struct pointer's door, which C WRITES through, and the void pointer's |
| 4 | `C.f(unsafe.Add(unsafe.Pointer(&b[0]), 4))` | `unsafe.Add` returns a pointer with the new number and no source | the void pointer's door, for a slice element, a Go array and a local; offset 0 is refused |
| 5 | `func (*T) U(q *T) { C.f(unsafe.Pointer(q)) }` | the receiver refusal of 16.9 skipped a method whose receiver has no name | the void pointer's door |
| 6 | `p1 := &b[0]; n := uintptr(unsafe.Pointer(p1))`, then a second, temporary `&b[0]` is converted and dies | a registration REPLACES the entry at its address, and a dead registrant removes it | `n`, with `p1` alive and pinned |
| 7 | a deferred call classifies at the call, not at the `defer` | cmd/cgo's own placement | ACCEPTED, once 6 is fixed |
| 8 | the fifth-crossing arm lists first-generation boxes only | | the arm, not the door |

The review MEASURED the harm of 1: `&h.n` returned from a helper, `h` dropped, a collection forced, and in 20 rounds
of 20 the door handed C an address the slice no longer lived at.

**The fix, as COORD ruled it.**

- **One rule for every derivation (1, 2, and their kin).** A box derived from a native box that is NOT C memory
  alone is minted retaining that box, in the slot a native box already has. The root, its pin and its source then
  live as long as the derived box, and every door refuses it by the test it already makes. A native box's own
  retained source has one production reader, the cgo doors, so nothing else observes the change. Sites: the field
  box (`NativeFieldBox`), `Reinterpret`'s native arm, typed `unsafe.Add` over a native box, and the array-field
  window, which shares the root's pin (every box kind has the pin field, and no box disposes its pin).
- **The same defect at its source, a site the review did not list (ACCEPTED).** The number-to-pointer operator,
  when its number RESOLVES to a live Go box of another pointee type, minted a native box with nothing, holding the
  resolved box in its hand. It now mints it retaining that box. The "pointer rebuilt from a Go array's number" of
  16.9 is then no longer flagless, `&h2.n` over it inherits, and the registry clause 3.1 added to the door becomes a
  second lock instead of the only one.
- **3, `unsafe.Slice` over a view (COORD: option (b), with a pre-check and a stop line).** A native-backed
  `slice<T>` holds a base address and nothing to inherit FROM, and a new word on `slice<T>` would grow every slice
  header in the corpus (not proposed). Its array field is unused when it is native-backed: it holds the shared
  empty array. `unsafe.Slice` over a native box that is not C memory alone gives the slice its OWN zero-length
  array, tied to the root by a weak table; a re-slice passes it along; an element or `SliceData` of such a slice is
  minted retaining the root. No new field. **This also fixes a lifetime defect outside cgo:** a native-backed slice
  over pinned managed memory holds nothing that keeps its root alive today, so the root can die and unpin under a
  live slice. An arm with no cgo holds that shut.
- **4, `unsafe.Add` on an `unsafe.Pointer` (COORD: option (b)).** "Add keeps the retained source" as first worded
  breaks a reader: runtime's hash family, the pinner, the slice-header box and unsafe's store-through each take a
  pointer's retained source as the referent AT its value, and would then act on offset 0. So `Add` over a pointer
  that carries a box returns a pointer carrying a DERIVED native box, at the new value, retaining the source. The
  readers see the truth at the value, and the door refuses it as reached through Go memory. Its cost is one more
  object per such `Add`, gated by the census (below).
- **5.** The receiver's type is read from the method's signature, never from a name.
- **6, every live registrant kept (COORD: H's lean).** "Never replace a live entry" is not enough by construction:
  A registers, B registers the same address and is not recorded, A dies, and B's number misses with B alive. Every
  evaluation of `&b[0]` mints a fresh box, so two registrants at one address is the ordinary case. An entry stays
  one weak reference while one registrant is live (today's bytes exactly), and becomes a small list at the second
  DISTINCT live registrant. A read answers if ANY registrant is alive and still pinned there. Removal compares, in
  `Resolve` and in `Sweep`. The list is pruned only after a collection has happened.
- **8.** The derived forms join the arm, each through all three doors, with the reflect pair read as well.

**The pre-check COORD ordered for 3: every reader of a slice's array field as IDENTITY, and what it does with a
native-backed slice today.**

| Reader | Today, for a native-backed slice | With its own zero-length array |
|---|---|---|
| `Overlaps` (copy and memmove overlap) | decided by ADDRESS, before the array is looked at | unchanged |
| the hash | the base address | unchanged |
| `==` on two slices | base, then the array's identity, then the window | would answer false for two `unsafe.Slice` calls over one view. **Keyed on the base for a native-backed pair in the same change,** so no answer moves |
| the stack-append buffer's three identity tests | false: the array is never the buffer's store | false, the same |
| element boxes and their canonical pair | each tests native-backed first, and uses the address | unchanged |
| the array-dims stamp, on a ZERO-length slice only | reads the shared empty array as "no backing" and substitutes a managed empty slice | would keep the slice. Guarded on native-backed first, so today's answer stands |
| `Array.Copy` between managed backings, the array alias, the string view | read the array's CONTENTS, which are empty either way | unchanged |

No overlap or alias decision turns wrong. One reader (`==`) would have answered differently, and is keyed on the
base, as COORD's stop line prescribes. If a gate shows another, SF-4 fires.

**Predictions, to be scored as worded.**

- **PF-1 (the reds).** Each shape of findings 1 to 6, built from the converter's own emission, crosses at
  `80e8b3c7d8` through every door it can reach, and is refused with the fix. Beside each, the same form over C
  memory still crosses.
- **PF-2 (lifetime).** Three arms, each red at `80e8b3c7d8`: a field of a view outlives the view (the root is still
  alive after a forced collection while the field's box is); an `unsafe.Slice` over a view outlives the view, with
  no cgo; and the review's registry sequence (p1 live and pinned, a temporary second mint dies, a collection, `n`
  still refused).
- **PF-3.** GolibTests' total rises by the new arms. An existing row changes only if it asserts, by name, the state
  this fix removes (a source dropped by `Add`, a flagless box from a resolved number). NOT READ which rows do:
  they are named in the red commit, before the fix, and a row that moves without being named there is SF-2.
- **PF-4.** 0 corpus bytes on four targets, `check-no-regression` byte-identical, no behavioral golden moves. The
  only converter change is on the cgo route.
- **PF-5.** The banked canaries read their banked counts: the five largest reflect consumers by verdict count
  (derived at gate time), the cost canary, and `runtime`, `os`, `syscall`, `internal/runtime/maps` and `crypto/tls`,
  because the operator, the field box and `Add` are on their paths. The weakest are the allocation asserts where
  `Add` runs inside an asserted body.
- **PF-6.** The registration census reads 0 inside every banked asserted body for `Add` over a carrying pointer
  and for new registry entries at an address that already has a live registrant.
- **PF-7.** Hosted: `converter-cgo` on four hosts and `cgo-e2e` on linux, windows and darwin (both architectures)
  read as they did at `c4b5d9613c`, every C-memory arm included.

**Stop lines.**

- **SF-1.** A C-memory control is refused, or an end-to-end arm that passed at `80e8b3c7d8` fails.
- **SF-2.** An existing GolibTests row moves that the red commit did not name.
- **SF-3.** A banked canary row, or a banked allocation assert, moves. Nothing is re-baselined.
- **SF-4.** A slice identity decision changes anywhere: stop. Identity is then keyed on the base address for every
  native-backed slice.
- **SF-5.** The fix needs a new field on `ж<T>`, on `slice<T>` or on `unsafe.Pointer`, a new box kind, or a
  go2cs-gen change.
- **SF-6.** A corpus byte moves, or a golden outside the cgo tests.
- **SF-7.** The census reads nonzero in a banked asserted body for `Add` or for the registry's list: stop and post
  the row. For `Add` the choice is then between the readers' own check and a scoped derivation.
- **SF-8.** A derived form the arms can build still crosses after the fix. It is not disclosed.

**Order.** Reds from the review's shapes; the fix; the full gates of 16.5 plus PF-5's canaries; the hosted legs; a
narrow re-review of the delta, which COORD launches, on the three mechanisms by name (the anchor array, the derived
box of `Add`, the registry's list); then 3.1 lands; then section 17, whose rows 1 to 4 and 6 these fixes change.

— H (2026-11-13)

### 16.12 Record 2026-11-14: the review fix as measured, a second review, and the windows cut

This record covers six steps:

- 16.11's fix as built, with the two stop lines it fired;
- its first full reading, on a base that holds batch 301 (`f8a94fa1bc`);
- the merge of batch 302 (`83fbf28560`);
- a second review, which found 3.1 still UNSOUND;
- the red arms for that review;
- H's half of the fix ruled for it (the windows cut).

**3.1 is not ready.** It lands only with the interior-pointer root closed. The owner ruled the split: P5 owns the
design (`docs/phase4/DESIGN-interior-pointer-root.md`, on `claude/p5-interior-pointer-root`) and cuts its sections
4.1 to 4.3 and 4.7. H cuts 4.4 and 4.5 and the red arms. The fix enters 1.26.11.1 as its own corpus seat after R's
hop.

**The trees.**

| Tip | What it is |
|---|---|
| `d87e1dd863` | 16.11's fix |
| `c05eea56f3` | the end-to-end arms corrected to take an element's address through a variable |
| `aab557f747` | the allocation guard's list and the arms rebuilt from the emission |
| `f8a94fa1bc` | the merge of master `a6cefba6ee` (batch 301) |
| `83fbf28560` | the merge of master `314e1d8b60` (batch 302) |
| `8a8a52ff1e`, `b642ac3682`, `f7973c969d`, `48796dfdde` | the second review's red arms, on `claude/h-cgo-31-review2-red` |
| `86247389c0`, `7014c1b2bd` | the windows cut and its provenance re-cite, on `claude/h-cgo-31-windows` |

This record and the merge of `48796dfdde` sit on top of `7014c1b2bd`.

#### 16.12.1 The fix as built, and the two stop lines it fired

**SF-8 FIRED: a fifth source.** The first end-to-end leg at the fix (cgo-e2e linux, run 43377981194, on side ref
`claude/h-cgo-31-e2e-green`) read five of seven reds green, refused by the carried box. `goadd` and `goaddlocal`
still crossed.

- **The cause.** The converter prefixed EVERY call whose result type is `unsafe.Pointer` with `(uintptr)`. A pointer
  returned from a call therefore came back as a bare number: whatever box it carried was laundered away.
- **The GolibTests arm missed it.** It handed `unsafe.Add`'s result to the door directly: green for a shape the
  converter never emits.
- **The defect is not cgo's.** On master, plain Go with no cgo, the converted program lost its writes through such a
  pointer after a collection: 20 rounds wrong of 20, where Go reads 0. As a tracked behavioral program it corrupted
  the managed heap ("Internal CLR error", 3 of 3).
- **Where it went.** COORD ruled it a seat of its own, ahead of 3.1:
  - the branch: `claude/h-unsafe-pointer-call-keeps-box`;
  - its record: `docs/phase4/DESIGN-unsafe-pointer-call-result.md`;
  - it landed in batch 301.
- **Two shapes found holding at the base.** The first red leg did not convert (run 43375049312). Two arms wrote an
  element's address inside the C call itself, and the route refuses that at conversion, by name. The arms now take
  the address through a variable.

**SF-2 FIRED.** The full GolibTests run at `d87e1dd863` read 2,585 passed, 43 skipped, 1 failed.

- **The failure.** `NoUncountedBackingAllocations` named two raw arrays the fix allocates outside the allocation
  counter: the slice anchor's zero-length array, and the registry's list of kept registrants. Neither was named in
  the red commit.
- **COORD's ruling.** List both BY NAME as infrastructure, each with its reason. Neither is an allocation a Go program
  asked for.
- **The neuter.** With one unlisted raw array added, the guard fails naming that array alone. With it removed, the
  file is byte-identical and the guard passes (`aab557f747`).

**Two shapes this record corrects.**

- An element of an array field of a view (`goviewelem`, `goviewelemstring`) was ALREADY refused at the red tree. H had
  posted it as open. The refusal comes from the element box's kind, not from any inheritance (16.12.3).
- The blank receiver of finding 5 was already refused at the base, so its arm is a control. The receiver with no name
  crossed, as the review said.

#### 16.12.2 The first full reading of the fix, at `f8a94fa1bc`

The merge of batch 301 had two conflicts:

- golib's native-derivation file, add/add. 3.1's file was kept whole; every non-comment line of master's is in it.
- The BOARD, appended on both sides; each heading appears once.

Nine files changed on both sides, and each composes both ways. The base readings are master `a6cefba6ee`'s: GolibTests
2,595 passed, 43 skipped of 2,638; behavioral 966; check-no-regression 1,057. Master's tree is `03a506bce5`'s, hash
for hash.

**The battery on H, and the hosted legs.**

| Prediction | Reading | Score |
|---|---|---|
| R-0 repoguard | rc 0, 260 s | MET |
| R-1 `go test ./...` | rc 0, 11 ok, 0 cached | MET |
| R-2 GolibTests 2,612 / 43 of 2,655 | 2,612 passed, 43 skipped of 2,655. The TRX difference is exactly 17 new arms, and no existing row changed | MET |
| R-3 check-no-regression | byte-identical across 1,057 | MET |
| R-4 behavioral | PASS, 966 projects; Output 936 passed, 0 failed, 30 skipped; no NOT MEASURED | MET |
| R-5 four-target footprint | A = master `a6cefba6ee`, B = the tip; 0 files on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64 | MET |
| R-6 canaries at bank | on H: nistec 44 (228 s), os 6,826, syscall 66, internal/runtime/maps 19, go/types 597, crypto/cipher 27,289, runtime 11,083. Hosted windows (43541842369): net/http 1,526, crypto/tls 1,393 | MISSED as worded, premise COORD's |
| R-7 hosted, 0 failed | converter-cgo: linux gcc 147/0/9 (43541766127), linux zig 147/0/9 (43541776956), windows 145/0/12 (43541787680), darwin 143/0/13 per arch (43541797945). cgo-e2e: linux 66/0/0 (43541808343), windows 64/0/0 (43541819534), darwin 66/0/0 per arch (43541830790) | MET |

- **R-6.** The amendment said to score crypto/tls at its full bank of 4,920 on the hosted runner. The hosted runner
  does not carry BoGo past the deadline either: it reads 1,393 under the same committed host-limit disclosure, and
  the sweep scores the row PASS. R-5 reading 0 files is why no linux or darwin shard was owed.
- **nistec's wall.** 228 s is H's first reading of that row. The recorded 354 s and 384 s are another box's, so
  nothing is claimed from the comparison.
- **R-7, the weakest line.** `goadd`, `goaddlocal`, `goaddvar`, `goaddfunc`, `goviewfieldfunc` and `goviewfieldvar`
  each end refused before the call on all four e2e jobs. With batch 301 under the fix, a pointer returned from a call
  keeps its box.

**16.11's predictions, scored as worded.**

| Prediction | Score |
|---|---|
| PF-1 | MISSED at `d87e1dd863`: SF-8, `goadd` and `goaddlocal` crossed end to end. MET at `f8a94fa1bc`: every shape of findings 1 to 6 is refused at every door it reaches, and every form over C memory still crosses (R-2, R-7) |
| PF-2 | MET: the three lifetime arms were red at the base and are green with the fix |
| PF-3 | MISSED at `d87e1dd863`: SF-2, a row the red commit did not name. MET at `f8a94fa1bc` after the ruling: exactly the 17 new arms, no existing row changed |
| PF-4 | MET: R-3, R-4 and R-5. The only converter change is on the cgo route |
| PF-5 | MET under the disclosure: R-6 |
| PF-6 | NOT MEASURED. The registration census stopped at 22 rows of 49, all on master's registry and before either seat. It resumes from row 23 after 3.1 |
| PF-7 | MET: R-7. Each count is the `c4b5d9613c` reading plus the new arms (converter-cgo +2, cgo-e2e +13) |

**Stop lines.**

- SF-2 and SF-8 FIRED and were ruled (16.12.1).
- SF-1, SF-3, SF-4, SF-5, SF-6 and SF-7 did not fire.
- SF-4: no gate showed a slice identity decision moving.

#### 16.12.3 The merge of batch 302, at `83fbf28560`

| Prediction | Reading | Score |
|---|---|---|
| M-1 no hand edit outside the BOARD and the allocation guard's list | two conflicts, the BOARD and `docs/Limitations.md` (adjacent inserts). No code file conflicted | MISSED as worded |
| M-2 GolibTests = master + 17 | base `314e1d8b60` 2,616 / 43 of 2,659; merge 2,633 / 43 of 2,676. Exactly the 17 GoCgoArgsTests rows; no existing row changed | MET |
| M-3 check-no-regression | byte-identical across 1,067. Read on the merge only: it was not run at `314e1d8b60` itself | MET |
| M-4 repoguard | ok | MET |

Six files changed on both sides, and each composes both ways. The BOARD holds 560 headings, each once.

COORD ordered the cgo legs re-read on the merge, because 302 changes how a struct holding arrays is laid out. They
read exactly as at `f8a94fa1bc`:

- cgo-e2e: linux 66/0/0 (43568096178), windows 64/0/0 (43568099701), darwin 66/0/0 per arch (43568102643);
- converter-cgo: linux gcc 147/0/9 (43568105239), windows 145/0/12 (43568108816);
- no struct arm moved, and the six Go-memory arms of R-7 stay refused.

#### 16.12.4 The second review: UNSOUND, two families

COORD's narrow review of the fix delta, read at `f8a94fa1bc`.

**What holds:**

- all six findings of 16.11 are fixed, red first;
- the allocation guard's list is closed at its two entries;
- the batch-301 composition is a clean superset;
- the six refused Go-memory arms are stricter than Go, and disclosed.

**What fails:** two derivation families still reach C as "C memory alone", with no `uintptr` in the Go source. By
SF-8 that is a stop, not a disclosure.

- **Family 1, the interior conversion.** `(*T)(p)`, where `p` is an `unsafe.Pointer` VALUE holding an interior Go
  address (`p := unsafe.Add(unsafe.Pointer(&b[0]), 4)`, or `unsafe.Pointer(&h.n)` of a view).
  - The converter emits `(ж<T>)(uintptr)(p)`.
  - The interior number misses the registry, and the operator mints a native box with no source and no pin.
  - It crosses the helper door and the struct-pointer door (which C writes through) and the void door.
  - It is 301's laundering class, on a CONVERSION instead of a call.
- **Family 2, the array-field window.** 16.11 wrote that the window "shares the root's pin". It did not: the window
  and its element view kept nothing. Over a view of Go memory, four forms cross:
  - a reinterpret of `&h.name`;
  - `unsafe.Add` over it;
  - `&s[1]` of `s := h.name[:]`;
  - `unsafe.SliceData(s)`.
  With no cgo, `&h.name[1]` with `h` dropped leaves an element over storage nothing holds.
- **The SHOULDs.**
  - `unsafe.Slice` over a view with a POINTER element (that branch had no anchor);
  - finding 8's reflect pair, which had no arm;
  - this scoring section;
  - a control for the void door's over-refusal of an element of a C struct's array field.

**Family 1 is a GC-safety hole in plain Go.** The red arm U2 reads the shape exactly as the converter emits it:

1. `ж<byte> first = &b[0]`;
2. `p := unsafe.Add(unsafe.Pointer(first), 4)`;
3. `(ж<byte>)(uintptr)(p)`;
4. everything else dropped.

The collection is real: three rounds of collect and finalize, then a forced, blocking, compacting gen2 collection
with the large-object heap set to compact, twice, then the freed space refilled.

A diagnostic, run three times and not committed, showed the same each time:

- the typed pointer is a native box with no source;
- its root box is collected;
- the `byte[]` storage is alive but MOVED, about 8 MB from the address the pointer still names.

The pointer still read 9, but that is the stale bytes left at the vacated address, not evidence the value survived.
Nothing pinned the storage. COORD ruled it 301's class, and a release matter; U2 is the fix seat's headline
acceptance arm, and its compaction recipe is fixed as written.

#### 16.12.5 The red arms, and their scores

The red arms are on `claude/h-cgo-31-review2-red`, cut from `83fbf28560`. The unit arms are in
`GoCgoArgsTests.Review2.cs`; the end-to-end arms are in the Phase 3 program.

**Unit arms, at `b642ac3682`:** 34 tests, 28 passed, 6 failed. The 17 earlier rows are among the 28, unchanged.

| Arm | Predicted | Read | Score |
|---|---|---|---|
| U1, family 1 at every door | red, 20 crossings | red, 15 | MISSED as worded: `&x[1]` over a local does not cross. Its element box's own address is registered, so the number hits the registry. Family 1 is an address INTERIOR to a box's own storage, and an element box is not interior to itself |
| U2, family 1 keeps its storage | red | red | MET (16.12.4) |
| U3, family 2 at every door | red | red, 34 crossings | MET. Per view, over a slice and over a Go array: reinterpret 5, `unsafe.Add` 2 at the void doors, `&s[1]` 5, `SliceData` 5. `&h.name[1]` crosses none |
| U4, a dropped view's array field | red | red | MET, both forms |
| U5, `unsafe.Slice` of pointers over a view | not predicted | red: its doors hold, but the view is collected while the slice is alive | — |
| U6, the reflect pair | not predicted | green: already refused at both void doors | — |
| C1, the same shapes over C memory | green | green | MET |
| C2, an element of a C struct's array field at the void door | red (low confidence) | red: refused as "a pointer into Go memory" | MET: the over-refusal was real |
| K0, offset 0 | green | green | MET |

**Two more arms at `f7973c969d`,** both ruled before the cut.

- U7: a slice re-based through its header over a view keeps the view. It drives the `unsafe.Pointer`-first header
  site. It reads red. The `ж`-first site is P5's.
- G1: an element of an array field of a view over Go memory is still refused at the void door. It reads green. It
  guards the door the cut narrows for C2.

**End to end, at `b642ac3682`** (cgo-e2e linux 43572655934, windows 43572657857, darwin 43572659780): MET as worded
on all four jobs.

- Seven arms crossed with Go's own output: `goaddtyped`, `goaddfill`, `gofieldtyped`, `goviewnamestring`,
  `goviewnameadd`, `goviewnameslice` and `goviewnamedata`. `goaddfill` printed C's fill on go2cs's side, so C wrote
  through the typed pointer into Go memory.
- The six C-memory twins pass, and the 37 earlier arms are unchanged.
  H's posts said 40 (16 run as Go does, 22 refused, the negstring/negbytes pair). That was a miscount: every
  linux log from f8a94fa1bc on names the same 37, which is 15 + 20 + 2. It was never measured as 40.

**An arm defect of H's, fixed at `48796dfdde`.** Three arms reach the struct-pointer door: `goaddfill`,
`gofieldtyped` and `goviewnamedata`. That door is Phase 2's, for every typed pointer parameter, and its refusal
("which C must not write through") names no increment. The arms were keyed on "increment 3.2", so a CORRECT refusal
would have failed them, and P5 would have met arms that could not turn green. They are now keyed on the door's own
text.

#### 16.12.6 The windows cut (4.4 and 4.5), golib only

| Item | Change |
|---|---|
| (a) | `NativeArrayBox<T>` gains its one field, the anchor of its element view. The anchor is `NativeSliceAnchors`' zero-length array, tied to the root the window was derived from, and null over C memory. `Over`, `NativeArrayOpeners.OpenArray` and an internal `builtin.NativeArrayPointer` overload carry the root. The public door the emission calls is unchanged |
| (b) | `FieldRefBox.NativeArrayWindow` passes `NativeDerivation.Kept(m_source)`. So does `builtin.NativeFieldArrayPointer`, the other window door, which is the same window class and beyond the plan |
| (c) | The window's element view is cut over that anchor, so `&s[1]`, `SliceData` and an element take each hold the root. The anchor is minted once per window, not on every element take |
| (d) | `NativeArrayBox` implements `INativeRooted`. It is a genuine native root only when it has no anchor. `IsNativeRooted` stays false, because field layout reads it corpus-wide. So the window's reinterpret and `unsafe.Add` keep it through `Kept` |
| (e) | C2. `ElemRefBox` implements `INativeRooted`: it is a genuine native root only as an element of a native-backed slice with NO anchor. The void door's registry test no longer refuses a hit that is a genuine native root |
| (f) | `unsafe.Slice` with a POINTER element keeps G's words route, and ties its fresh array to `Kept(ptr)` (`NativeSliceAnchors<T>.Keep`, no field) |
| (g) | `IGoReflectSlice.ReflectOverNativeMemory(nuint address, nint len, nint cap, object? root = null)`, implemented over `NativeSliceAnchors<T>.For(root)`. Null means C memory alone, as before. `SliceHeaderBox`'s native arm passes `Kept` of the data word's box. The `ж`-first site is P5's, on this signature |

**S5 held:** no field beyond the native array box's one, and no change to `ж<T>`, `slice<T>`, `unsafe.Pointer`, the
converter or the generator.

**G1, made to fail once.** With (e)'s no-anchor clause dropped, G1 failed by name and C2 still passed. The file was
then restored byte-identical (same sha256), and the edit was never committed.

**The scores.**

| Prediction | Reading | Score |
|---|---|---|
| H-1 GoCgoArgsTests: U3, U4, U5 and C2 turn green; U1 and U2 stay red | 36 tests, 34 passed, 2 failed: exactly U1 and U2. U3, U4, U5, U7 and C2 green; U6, C1, K0, G1 and the 17 stay green | MET |
| H-2 full GolibTests | 2,687 = 2,642 passed, 2 failed (U1 and U2), 43 skipped. The TRX difference against the merge's is exactly the 11 new rows; no existing row changed; the allocation guard passes | MET, as corrected BEFORE the measurement (from 2,685, since U7 and G1 came after it) |
| H-3 repoguard, `go test ./...`, check-no-regression | at `86247389c0` repoguard STOPPED (below). At `7014c1b2bd`: repoguard rc 0; `go test` rc 0, 11 ok, 0 cached; byte-identical across 1,067 | MET at `7014c1b2bd` |
| H-4 full behavioral | Relaunched at 67 GB free: PASS, 974 projects; Output 944/0 with the standing 30 skips; 0 NOT MEASURED | MET |
| H-5 canaries and allocation asserts at bank | On H, under go1.26.10, at `7014c1b2bd`: 55 rows, one at a time, each PASS at its roster count. These are 54 rows of the 301 sweep's set, which includes every allocation-assert row, plus `text/template`. Each count equals the 301 sweep's reading. `runtime` 11,083, `os` 6,826, `crypto/cipher` 27,289. Hosted windows 43575527735, at `86247389c0`, whose only difference from the tip is a docs commit: `net` 505, `net/http` 1,526, and `crypto/tls` 1,393 under its standing BoGo host-limit disclosure | MET |
| H-6 end to end: the four `goviewname*` arms turn refused; the three family-1 arms still cross; the twins and the 37 unchanged | cgo-e2e linux 43575515927, windows 43575518122, darwin 43575520708: on every job the four turn refused; `goaddtyped`, `goaddfill` and `gofieldtyped` still cross; the twins and the 37 hold. `goviewnamedata` still failed, on its arm's keying alone (fixed at `48796dfdde`) | MET |

**Hosted beyond H-6.**

- golib-tests linux (43575523139): 2,738 tests, 2,703 passed, 33 skipped, 2 failed.
- golib-tests darwin (43575525581): arm64 2,649 / 2,600 / 47 skipped / 2 failed; x64 2,649 / 2,601 / 46 / 2.
- Every failed set is exactly U1 and U2: the cut composes on every OS, with only P5's half red.
- sweep-shard windows (43575527735): net 505, net/http 1,526, crypto/tls 1,393 under its disclosure, all at bank.

**Stop lines.** None of the cut's fired. Two stops in the battery were H's, and neither is a code finding.

- **Repoguard, at `86247389c0`.** 30 of 935 provenance notes, in five docs pages, cited lines the cut moved.
  - 28 were re-cited at the line the cut's own diff maps each to.
  - 2 cite the class declarations of `ElemRefBox` and `NativeArrayBox`. Each gained an interface on the same line
    and still says what its note says, so its fingerprint was re-read.
  - The fix is `7014c1b2bd`.
  - Lesson: before cutting a golib file, list every provenance note that cites it. That is the docs-sample rule,
    carried to golib.
- **Behavioral, at `7014c1b2bd`: VOID.** The box ran out of disk.
  - 37 Go oracle builds failed, which a golib-only cut cannot reach.
  - A test that writes into TEMP failed.
  - The log was cut, and the runner exited 127.
  - Free disk was 40 GB at launch; behavioral's peak on H is above 43 GB. The floor for a full behavioral run on H is
    now 60 GB, not the sweep's 25 GB.
  - After the go build cache was reclaimed (67 GB free), the stage was re-run from behavioral at the same tip.

**Owed:**

- This cut reaches master inside P5's seat, which merged `7014c1b2bd` and carries 4.1 to 4.3 and 4.7. Those turn U1
  and U2 green and respell five lines of U1 and U2 to the typed door. The seat lands with batch 305, so this branch
  does not take the go1.26.11 hop itself. It carries the arm fix `48796dfdde`, merged after the battery, and this
  record.
- After batch 305 lands: one merge of master into 3.1's branch under go1.26.11, then 3.1's re-gate (GolibTests,
  CNR, and the hosted cgo-e2e and converter-cgo legs, where the seven `go*` arms of 16.12.5 must be refused). Then
  3.1's release-path seat.
- The narrow re-review of the combined fix.
- PF-6, the registration census from row 23.
- Then the pin-registry proposal (17.11), re-gated on the same base.
  - Pointer (2026-11-14): its design is `docs/phase4/DESIGN-cgo-pin-extents.md` (accepted; design only). Its seat
    is the `.4` item, cut separately off master.

— H (2026-11-14)

## 18. Amendment 2026-11-15: increment 3.2, the plan (posted before code; the .5 train)

**Where Phase 3 stands on master `0c1d295ea8`.**
- 3.0 landed in batch 292, and 3.1 in batch 305 (with its re-gate in 306).
- 3.3's red, the realloc shape (a `copy` into a view of C memory that Go then reads), is covered by P5's view doors,
  which landed in batch 308: `View`, `CopyInto` and `CopyFrom`, whose V3 arm is a native window over C memory
  (`src/core/unsafe/unsafe.cs`; GolibTests `CopyIntoNativeMemoryCopiesTheSourcesLength` and the native-view arms). Its
  same-element reslice is V1. 3.3 is therefore closed by that seat, not by this plan.
- 3.4 (`net` and the harness) needs 3.2's pinned class.
- **So the next increment is 3.2.**

**What 3.2 admits** (sections 4 and 6 as ruled, with section 13's M1 to M5):
1. **Reading C memory.** Section 6's four shapes, each READ as a red before any change: a scalar field of a native
   struct with pointer fields; a pointer field's address loaded through; C memory reinterpreted as a Go struct; and
   an element of a native-backed slice reinterpreted as a C struct pointer and passed back, which must classify as C
   memory. A new box kind or a go2cs-gen change is stop line S-B (a design posted first).
2. **The PINNED class.** An element of a Go slice or array, or a reinterpretation of one or of a whole Go value,
   classified ON THE BOX (M2) and never on a number (M3). The wrapper pins the whole backing for the call and hands C
   the element's address. A pointee that holds Go pointers is refused with cgo's own panic.
3. **The out-parameter** (`&db`, `char **`), ruled shape (i): a stack word, copied in and copied back by the
   pointer-member rules.
4. **The no-copy case under GC pressure:** C writes the pinned memory while another thread forces compacting
   collections, and Go reads C's write.
5. The converter lowers cmd/cgo's `_cgoIndex` and `_cgoSlice` check closures to the direct call, as it does the
   address and variable forms.

**The pin path, with pin-extent rows 2 and 3 folded in** (BOARD, 2026-11-15: concurrent pins collapse at the runtime's
GC handle table and at finalizable registration, not in the index). A call-scoped pin takes NEITHER:
- The wrapper pins with a C# `fixed` over the backing's data reference. The door returns the backing and the byte
  offset, and the wrapper takes `fixed (byte* p = &MemoryMarshal.GetArrayDataReference(backing))` around the call.
  That is a pinned local the GC already honours across a P/Invoke.
- No `GCHandle`, no `PinnedBuffer`, no finalizer, and no index entry: the pin's extent is the call, and the box
  classified the argument.
- In the row-3 probe, `fixed` was the arm that did not collapse: 1 ns per op at 1 and at 4 threads, with throughput 2x.
- The long-lived box pins (`m_pin`, the address-take contract) keep `PinnedBuffer`. Their cure (pooled or long-lived
  handles, no per-pin finalizer) touches `ж<T>`'s lifetime contract corpus-wide. It is NOT in 3.2; it stays the
  rows 2+3 BOARD direction, and a design of its own when it is taken.

**Pin-extent row 1 (the 3.2 precondition).** It assumed "from 3.2 a hit IS a crossing". In this plan it is not: 3.2
admits Go memory only when the BOX classifies it, and a NUMBER that the index finds inside a pin is still refused.
So the side list stays on refusal paths, and the precondition does not bind 3.2. If the ruling is that a number
inside a pin should cross, row 1 binds first, and this plan changes before code.

**Predictions** (scored as worded, at the cut's tip):
- **I-1 corpus footprint, 0 files on all four targets.** The change is behind `-cgo` plus golib's `GoCgoArgs`, and the
  standard library converts with `CGO_ENABLED=0`. The two-seeded comparison, seeded so the hand-owns stay.
- **I-2 the flip set is exactly the BOX-carried refusals.** Every arm that asserts a box-classified element or
  whole-value pointer refused with "increment 3.2" turns to the crossing. Every arm that asserts a NUMBER refused stays
  refused. The uses: GolibTests 15 (Phase3 8, Derived 4, Review2 2, PinExtents 1) and `cgoPhase3_test.go` 26. Each
  is classified box or number, by reading, before code, and the predicted flip set is posted as a list.
- **I-3 section 6's four reds.** I do not predict which pass today; they are READ first, as the plan says.
- **I-4 the pinned arm under pressure.** C's write is read by Go in every one of N rounds, with an unpinned twin shown
  to move (Inconclusive otherwise).
- **I-5 gates**, all unfiltered: GolibTests Release and Debug, GenTests, `go test ./...`, CNR, the footprint, and hosted
  cgo-e2e, converter-cgo and golib-tests on three OSes.

**Stop lines.**
- S-B: a new box kind or a go2cs-gen change.
- Any NUMBER that crosses into Go memory.
- A `fixed` that cannot reach a backing (a reference-bearing pointee, which is refused instead).
- Any corpus line moved.

— H (2026-11-15)

### 18.1 Record 2026-11-15: section 6's four reds READ, and the I-2 flip list (before code)

COORD approved 18 for the .5 train. Rulings: numbers stay REFUSED in 3.2, so pin-extent row 1 does not bind it; a
call-scoped `fixed` is the pin; `m_pin` stays out.

**Section 6, read end to end** (`testdata/cgo/p3`: arms `s6field`, `s6next`, `s6reinterp` and `s6slice`; new probe.c
functions `go2cs_probe_list`, `_list_release` and `_address`). Each is asserted as Go prints it. H has no C compiler,
so the hosted cgo-e2e legs are the instrument: linux 43765795991, windows 43765805154, darwin 43765814200, at `0ae7d5c230`.

| arm | shape | Go | go2cs today, all three OSes |
|---|---|---|---|
| `s6field` | a scalar field of a C struct that also holds pointer fields | `2` | `2`: ALREADY GREEN |
| `s6next` | the address of a pointer field, loaded through | `10 b` | **RED**: `panic: runtime error: invalid memory address or nil pointer dereference` |
| `s6reinterp` | C memory reinterpreted as a Go struct of scalars | `2 8080 2130706433` | the same: ALREADY GREEN |
| `s6slice` | an element of a slice over C memory, reinterpreted as a C struct pointer and handed back to C | `true` | `true`: ALREADY GREEN, classified as C memory |

- Three of the four already pass, and stay as guards.
- The one red is a POINTER FIELD of a C struct read from C memory, whose value reads nil. Its root is not yet READ.
  If the cure needs a new box kind or a go2cs-gen change, that is stop line S-B, and a design is posted first.
- **The void run, recorded as void and not a red.** The first dispatch (`0de78b7bee`; 43765347265 / 43765356418 /
  43765365459) never built the probe library: `probe.c` already defines `struct go2cs_probe_node`, and H had read
  only its function lines. Lesson: read the WHOLE C file before adding to it.

**I-2, the flip list.** Each hand-over form of the arms that assert the "increment 3.2" refusal was classified by the
door's own branch (a scratch probe calling `GoCgoArgs.GoMemory` on every form, never committed), then corrected by
reading for the one thing the probe could not see, the ROOT storage (M2). The probe checked only the carried box's
own element type, so it scored a view over a reference-bearing struct as crossing.
- **Crosses in 3.2 (FLIP), 18 GolibTests hand-overs.**
  - `AVoidPointerIntoGoMemoryIsRefusedBeforeTheCallInEveryFormItArrivesIn`: the retaining and pinning mints of a
    scalar (2 of its 9 forms).
  - `APointerOverManagedStorageNeverCrossesAVoidPointer`: the pinning and retaining mints of a wider view pinned over
    an element, a view aliasing a struct, a native box with a retained source, a native box over a pinned managed
    buffer, `unsafe.StringData` and `unsafe.SliceData` (12 of its 36).
  - `ACallResultCastToANumberCarriesNothingAndIsRefusedByItsPinExtent`: the kept `unsafe.Add` result (1 of 2).
  - `AnElementOfAnArrayFieldOfAViewOverGoMemoryIsStillRefusedAtTheVoidDoor`: both mints (2 of 2).
  - `AHelpersPointerIsNilOrCMemoryAndGoMemoryIsRefusedNamingTheHelper`: the typed helper over a Go box (1).
- **Refused with cgo's own panic** (the storage holds Go references or is an order token): the bare mint of a box that
  holds references; the three mints of a view over a reference-bearing struct; the three mints of a native box that
  aliases an order token; and both token numbers.
- **Stays refused as a NUMBER** (unchanged outcome, new text): every number held in a variable, every bare mint (it
  carries no box), the one-expression round trip, the two forms of `AVoidPointerToAViewReinterpretedOverGoMemoryIsRefused`,
  all 14 of `ANumberBuiltByArithmeticOverAGoPointerIsRefusedForEveryKindOfPin`, the three of
  `ANumberStaysRefusedWhileAnyPointerThatPinsItsStorageIsAlive`, and the pin-extent arms' numbers.
  - Also the pinning and retaining mints of a pointer REBUILT from a Go array's number. Their native box has no
    source, so 3.2 has no storage to fix and refuses them as numbers.
  - **Amended 2026-11-24 (seat 1b; COORD ruling (a), inbox H 20261124T021216Z):** the premise "their native box has
    no source" has been false since `d87e1dd863` (2026-11-13). The number-to-pointer conversion keeps the live Go box
    the number resolved to (`NativeDerivation.KeptResolved`), so the box chain names the array's pinned storage and
    13.4 (M3) classes it as that box. The pinning and retaining mints of this form CROSS. In Go,
    `(*byte)(unsafe.Pointer(&array))` is a valid pointer into Go memory that holds no Go pointers, and cgo accepts it.
    The bare mint and the number held in a variable stay refused. The sentence above stays as written.
- **The end-to-end arms** (`cgoP3Arms`; the converter mints `unsafe.Pointer(&x)` as `FromPinnedBox`, so the arms
  whose pointer is written in the call carry their box). **22 flip to Go's output:**
  - `goinnerdefer`, `goinnergo`, `govoid`;
  - `gobytes`, `gostring`;
  - `goviewfield`, `goviewfieldarray`, `goviewagain`, `goviewelem`, `goviewelemstring`;
  - `goslice`, `goslicedata`;
  - `goadd`, `goaddlocal`, `goaddvar`, `goaddfunc`, `goaddtyped`;
  - `goviewfieldfunc`, `goviewfieldvar`;
  - `goviewnamestring`, `goviewnameadd`, `goviewnameslice`.
  - **2 stay refused as numbers:** `govoidnumber`, and `govoidexpr`. `govoidexpr` is Go-valid: a one-expression
    round trip that the converter emits as a number. By the ruling it reopens only with a consumer's census, and a
    converter fold of the identity round trip would keep the box at a corpus cost not yet measured.
  - The three write-through arms of the typed door (`goaddfill`, `gofieldtyped`, `goviewnamedata`) flip too, as the
    pinned class for a typed pointer.

**COORD's notes on the reds.**
- (a) A C callback that re-enters a cgo call while the `fixed` is held cannot arise in 3.2. `//export` (Phase 5) and a
  C function used as a value (Phase 2) are both refused by name. The arm is named for Phase 5.
- (b) The pressure arm forces compaction MID-CALL: a slow C fill, another thread calling
  `GC.Collect(2, Forced, true, compacting: true)`, and an unpinned twin shown to move (Inconclusive otherwise). It lives
  in GolibTests' CgoProbeTests around the 3.2 wrapper, because converted `runtime.GC()` is a plain `GC.Collect()` that
  does not guarantee compaction.

— H (2026-11-15)

### 18.2 Amendment 2026-11-15: s6next rooted, and the design for a pointer field of C memory (S-B; posted before code)

**The root, read on the hosted legs** (cgo-e2e at `27a41b889c`: linux 43769940571, windows 43769950200, darwin
43769959680 on amd64 and arm64, identical everywhere).
- Every read of a POINTER field of a C struct in C memory fails: `l.next == nil` (`s6nextnil`), `l.next.family`
  (`s6nextdirect`) and `C.GoString(l.name)` (`s6name`). Each is refused by name: "cannot read native memory as
  `_Ctype_struct_go2cs_probe_entry`: its CLR layout is not its Go layout (field next / name is a managed reference
  where Go stores the value inline)".
- go2cs-gen's accessor for a field is `public ref TF f { get => ref ʗp.field(T.Ꮡf); }`, and `ж<T>.field` sends a native
  receiver to `NativeBox<T>.FieldRef`. That reads a SCALAR field at its Go offset (`s6field` is green), and refuses a
  field whose C# type is a managed reference: a C pointer field is `ж<U>` in C#, where C stores an address word.
- `s6next`'s own route (`&l.next`, then a load) does not reach that refusal. It makes a native box whose pointee type is
  itself a reference, over C memory, and its read takes the C word as a managed reference. That is memory-unsafe. Its
  floor is a separate .4 seat (`claude/h-nativebox-ref-floor`), which refuses such a box by name.

**The design (.5, part of 3.2; a review follows, since it touches the gen accessor).**
1. **Which fields.**
   - Covered: a field of a C struct or union that cmd/cgo declares (`_Ctype_struct_*`, `_Ctype_union_*`), whose Go
     type is a pointer (`*T`, `*C.char`) or `unsafe.Pointer`.
   - The converter marks such a type with a new `[GoCgoStruct]` attribute, and only under `-cgo`, so the corpus moves
     0 lines.
   - The generator reads the marker, never the `_Ctype_` spelling.
2. **The accessor.** For those fields go2cs-gen emits a GET/SET property in place of the `ref` property, as it already
   does for header-shaped structs. The getter calls `ʗp.cgoPointerField(T.Ꮡf)`, and the setter calls
   `ʗp.cgoPointerField(T.Ꮡf, value)`. A `ref` to such a field then does not compile, by design, as for headers: a ref
   into C memory has no managed slot to name.
3. **The read** (golib, `ж<T>.cgoPointerField<U>`).
   - A MANAGED receiver (a C struct value Go allocated: `var n C.struct_x`, `new(C.struct_x)`) reads the slot, as
     today.
   - A NATIVE receiver reads the address WORD at the field's Go offset (`FieldRefBox.GoFieldOffsetOf`, the offset
     `s6field` already reads).
     - A 0 word is nil.
     - Any other word mints `new NativeBox<U>(word)`, which is C memory and keeps nothing, as `PointerMemberBack` mints a
       pointer C returned. For `unsafe.Pointer`, it mints the number pointer.
4. **The write.** Go stores an address into C memory here.
   - A MANAGED receiver stores into the slot, as today.
   - A NATIVE receiver:
     - nil writes 0;
     - a pointer to C memory alone (a genuinely native root, the void door's test) writes its address;
     - a pointer into Go memory is REFUSED by name. Go's rule is that C memory must not hold a Go pointer
       (cgocheck=2's "write of unpinned Go pointer into non-Go memory"), and go2cs refuses it in every mode, since a
       managed address stored where the collector does not look is the corruption the floor exists to stop.
5. **The address of such a field** (`&l.next`) over a native receiver needs a box whose value is the word: a NEW BOX
   KIND (S-B), `NativePointerFieldBox<U> : ж<ж<U>>`, whose `Value` get and set are 3 and 4.
   - Proposed for 3.2 too, because `s6next` is its arm and `getaddrinfo`'s walk `for p := res; p != nil; p = p.ai_next`
     reads through the field, not its address.
   - Until it lands, the floor refuses `&l.next` by name.
6. **Not changed:** a scalar field (already read at its Go offset), a struct or array field (`FieldRef` as today), and
   any type cmd/cgo did not declare. modernc's libc types are plain Go types, so they are outside the marker.

**Predictions** (scored as worded, at the cut):
- D-1 corpus footprint 0 files on four targets: the marker is emitted only under `-cgo`.
- D-2 the end-to-end arms `s6next`, `s6nextnil`, `s6nextdirect` and `s6name` print what Go prints on three OSes;
  `s6field`, `s6reinterp` and `s6slice` stay green.
- D-3 GenTests gain the GET/SET shape for a marked struct's pointer field and keep the `ref` shape for every other
  field. A `ref` to a marked pointer field fails to compile, asserted by a compile arm.
- D-4 a new write arm: a C pointer stored into a C struct's pointer field reads back in C, and a Go pointer stored
  there is refused by name.

**Stop lines.**
- Any unmarked type's accessor changes (the corpus moves).
- A converter `ref` to a marked pointer field that the converter emits today. The GenTests compile arm and the e2e
  build find it, and it is posted before any workaround.
- The new box kind reaching any type outside the marker.

— H (2026-11-15)

### 18.3 Amendment 2026-11-16: 18.2 after the reference floor, and (B) word semantics for `unsafe.Pointer` (posted for review before code)

**What changed under 18.2.** The reference floor (`claude/h-nativebox-ref-floor`, seated in batch 311 at `3ce394cbc0`)
now refuses, by name, every whole-value read or write of a reference-type pointee through a native box: "a managed
reference where Go stores words".
- `s6next`'s `&l.next` route reads a refusal instead of a garbage reference. 18.2 item 5's "until it lands" is now true.
- The floor makes one exemption, ruled (C)-narrowed: `unsafe.Pointer` over MANAGED storage (the box retains its source
  or carries a pin). Two places in the converted runtime need it: `map.go`'s indirect key and `atomic_pointer.go`'s
  atomicwb, both seen live in the behavioral program `UnsafePointerReinterpret`.
- The exemption keeps a hazard, recorded as a `.5` BOARD row (2026-11-16): golib's `unsafe.Pointer` is a CLR class, so
  the exempt write puts a managed reference into a slot the storage types as a word. The collector does not trace that
  slot, so the reference can go stale or be collected.
- (B), below, is that row's cure. When it lands, the exemption is deleted and the floor refuses `unsafe.Pointer`
  regardless of the box's source.

**Why (B) is not a golib-only change.** `NativeBox<T>.Value` is a `ref T` over the address, and a store goes through
`DerefOrNull`'s `ref`. Converting between a word and a CLR object cannot sit behind a `ref`:
- a read can return a ref to a shadow;
- a write through that ref lands in the shadow, and nothing tells the box to write the word back.
So the load and the store must be CALLS at the emitted site.

**The population, read at master `2c1ce10b25`.**
- `To<unsafe.Pointer>`, the `(*unsafe.Pointer)(p)` reinterpret, appears at 89 sites in 15 converted files: the
  internal/runtime/maps group (7 files), reflect and reflectlite `value.cs`, runtime `atomic_pointer.cs`,
  `cgocheck.cs`, `type.cs`, and `cgocall.cs` on three OSes.
- `ж<unsafe.Pointer>` appears 40 times in 20 files, including hand-owned `sync/atomic/doc_impl.cs` and
  `internal/runtime/atomic/atomic_impl.cs`. Their `LoadPointer` / `StorePointer` / `SwapPointer` /
  `CompareAndSwapPointer` doors take a `*unsafe.Pointer` and are the same class.
- LIVE, measured by the dereference census at 311:
  - GolibTests (C1): 3 rows, all controls;
  - behavioral (C2): 2 rows, both `UnsafePointerReinterpret`;
  - the 13-package C3 set before the floor: 0.
  The cure's corpus footprint is therefore wide (every load and store through a `*unsafe.Pointer`), while its live
  population is narrow.

**The design.**
- **W1, the load:** `*p` where `p`'s static type is `*unsafe.Pointer` emits `@unsafe.Pointer.LoadWord(p)` instead of
  `p.Value`.
  - Over a box that holds a CLR `Pointer` slot (a `StandardBox<Pointer>`, a field or element box over one) it returns
    the slot: today's semantics, no conversion.
  - Over a `NativeBox<Pointer>` it reads the address WORD. 0 is nil. A number that resolves to Go storage (the exact
    registry, then the pin-extent index) returns a `Pointer` that carries that storage's box, as `FromHeaderData`
    carries an element box. Any other number returns the number pointer, which is C memory and keeps nothing.
- **W2, the store:** `*p = v` emits `@unsafe.Pointer.StoreWord(p, v)`.
  - Over a CLR `Pointer` slot it stores the slot, as today.
  - Over a `NativeBox<Pointer>` it writes `v`'s NUMBER. Then:
    - (a) the destination is C memory (no retained source, no pin) and `v` names Go memory: REFUSED by name, as 18.2
      item 4 refuses a Go pointer stored into a C struct field;
    - (b) the destination is managed storage and `v` names Go memory: the store also records a KEEP-ALIVE edge from the
      destination's root storage to `v`'s pin. A `ConditionalWeakTable` keyed by the root object maps the slot's
      offset to the pin, so the stored number stays valid (pinned and alive) as long as the destination storage lives.
      Overwriting the slot through W2 releases the old edge;
    - (c) `v` is nil or C memory: the number is written and no edge is recorded.
- **W3, the atomic doors:** the hand-owned `sync/atomic` and `internal/runtime/atomic` pointer doors take the same
  dispatch: W1 for Load, W2 for Store, and W1 then W2 under the existing atomic for Swap and CompareAndSwap, whose
  compare is on the WORD.
- **W4, the floor:** with W1 and W2 in place, delete the exemption (`s_isUnsafePointer && (retained || pin)`). A
  `.Value` read of a `NativeBox<Pointer>` is then reached only by code W1 and W2 did not rewrite, and it is refused by
  name, like every other reference.

**Faithfulness.** In Go an `unsafe.Pointer` IS a word, and storage typed as pointer-bearing is traced by the collector.
W2(b)'s edge is the stand-in for that tracing: a Go pointer stored into managed word storage keeps its object alive and
in place.
- A `uintptr` store does NOT take the edge, by Go's own rule: a `uintptr` keeps nothing alive.
- A number written by arithmetic through a `ж<uintptr>` stays a number, exactly as in Go.

**Predictions (scored as worded at the cut; the corpus is measured with the two-seeded diff on four targets).**
- B-1: the corpus footprint is confined to load and store sites through a `*unsafe.Pointer`: the 15 `To<unsafe.Pointer>`
  files plus the deref sites over `*unsafe.Pointer` parameters and fields. No other line moves. The count is predicted
  from a seeded emission before the cut, not from this grep.
- B-2: C1 and C2 read 0 B2a `kind=UnsafePointer` rows after W4, because every live row moves to W1/W2, which do not
  dereference a `ref`.
- B-3: `UnsafePointerReinterpret`'s output is unchanged.
- B-4: a new pressure arm. A Go pointer is stored through W2 into managed word storage, every other reference to the
  object is dropped, and forced compacting collections with finalizers run. The pointer then reads back through W1
  and names the same bytes. Today's exempt path is the red arm, and it can read a moved or collected object.
- B-5: a refusal arm, a Go pointer stored through W2 into C memory, is refused by name.

**Stop lines.**
- A W1/W2 site outside a `*unsafe.Pointer` load or store.
- A changed outcome in any validated row.
- A keep-alive edge reached from a `uintptr` store.
- The floor's exemption deleted before B-2 reads 0.

**Open for the review.**
- (i) Is W2(b)'s edge table keyed right? A root storage object can hold many slots, so it is keyed by root plus offset.
  Each edge is one pin, so the cost is bounded by the number of live stored pointers.
- (ii) W1 for a number that resolves to Go storage RE-CARRIES a box. Should it instead return the number pointer and
  let the void door refuse it later, as 3.2 does for numbers?
- (iii) Does W3's CompareAndSwap on the word match Go when two different `Pointer` objects carry the same number?
  Go compares words, so yes by construction; the arm pins it.
- (iv) Sequencing: W1–W3 and W4 as one cut, or W4 as its own seat after B-2 is read at a batch.

— H (2026-11-16)

### 18.4 Revision 2026-11-17: P3's review of 18.2 + 18.3 answered (before code)

P3 reviewed `b8bc349f28`: APPROVE WITH CHANGES, all before code (mailbox, COORD inbox `20261116T214842Z-P3.md`).
COORD accepted the five MUST-FIX findings as worded, took should-fix 6 and 7, ruled nit 8 a pair of named deviations,
and adopted P3's answers to 18.3's questions (i)-(iv). This block REPLACES the parts of 18.2 and 18.3 it names. Where
it is silent, they stand. Each finding is answered by its number.

**1. The address of a C struct's pointer field (18.2 item 5): REFUSED in 3.2, and the new box kind is WITHDRAWN.**
- `ж<T>.Value` is an abstract `ref T` (ж.cs:126). So `NativePointerFieldBox<U> : ж<ж<U>>` could only hand out a ref
  to a shadow: `pp := &l.next; *pp = other` would write the shadow while C memory kept the old word, silently.
- `&l.next` over a native receiver stays refused by name. It reaches the reference floor (seated at `3ce394cbc0`):
  "a managed reference where Go stores words". The reads 3.2 needs go through the field accessor (18.2 items 2-4),
  not the field's address: `l.next == nil`, `l.next.family`, `C.GoString(l.name)`, and getaddrinfo's walk
  `p = p.ai_next`.
- `s6next`, the arm that takes the address, becomes a REFUSAL arm: its expected output is the floor's panic text,
  asserted by name. `s6nextnil`, `s6nextdirect` and `s6name` carry the read.
- A later increment may route `**C.T` loads and stores through W1/W2-style calls, with its own write arm. It is out
  of 3.2.
- Stop line S-B still applies to 3.2: the go2cs-gen GET/SET accessor is a gen change, so it is reviewed. The new
  box kind has left the design.

**2. The word and its edge change in one ordered protocol (W2, W3).** Each slot has an edge SET, not one edge.
- **Store** `StoreWord(p, v)` over managed word storage:
  - (a) add the edge for `v` to the slot's set;
  - (b) exchange the word (`Interlocked.Exchange` in the atomic doors; a plain store in W2, as Go's plain store is);
  - (c) remove the edge whose number equals the DISPLACED word.
  P3's race, two `atomic.StorePointer` calls of `a` and `b`, ends with the word `b` and the set `{b}`: whichever
  exchange runs second displaces the first, and removes the first's edge.
- **CompareAndSwap:** add the edge for `new`, then CAS the word. On success, remove the displaced word's edge. On
  failure, remove the edge it added.
- **Swap** is Store that also returns the displaced word, read through W1 before its edge is removed.
- No step drops a live edge before the word stops naming it. The window between (a) and (c) can hold one extra edge,
  which keeps an object alive briefly. It never leaves one unkept.

**3. The edge holds the referent BOX, not a pin (W2(b)).**
- A reference-bearing pointee has no pin: its number is an order token, and the token registry's entries are weak
  (ж.PointerTokens.cs, LIFETIME). An edge that held "v's pin" would hold nothing for it.
- So the edge is (the number written, the referent BOX). Holding the box keeps the object alive. For a value-type
  backing it also keeps the box's own pin-only hold, which is what made the number.
- **B-4 gains a reference-bearing arm.** An indirect map key that holds a `string` is stored through W2, every other
  reference is dropped, forced compacting collections with finalizers run, and W1 reads the slot. It returns the
  same box, and the key reads back intact.

**4. W1 never resolves by number over managed storage (W1; replaces 18.3's "resolves to Go storage" branch).**
- Over a `NativeBox<Pointer>` whose storage is MANAGED, W1 answers through the slot's own edge only, and only when the
  edge's number equals the word. Otherwise it answers the NUMBER POINTER, as 3.2 does for every number.
  - P3's scenario fails closed: a copied word with no edge, whose old root died and whose address now holds another
    pinned object, reads as a number. It is never that object's box.
- Over C memory (no retained source and no pin), W1 answers nil for 0 and the number pointer otherwise, through 3.2's
  number rule (finding 6).
- **The memmove-class doors transfer edges.** A copy of managed word storage moves the edges whose offsets fall in the
  copied range to the destination backing, at the shifted offset, in the copy's own direction.
  - The doors, read at master `2c1ce10b25`: the hand-owned `runtime.memmove` (`memmove_impl.cs`). Converted
    `typedmemmove` and its `reflect_` / `reflectlite_` / `maps_` wrappers (`mbarrier.cs`) reach it.
    `internal/runtime/maps`' own `typedmemmove` is refused today (`runtime_impl.cs`).
  - The cut re-reads this list at its own tree. A copy door found that is not on it is a stop line.
- `copy` over a `[]uintptr` or `[]byte` carries no edge, by Go's rule: a `uintptr` keeps nothing alive.

**5. W1-W3 and W4 are ONE cut.** B-2 is read on the cut's tree TWICE:
- with W1-W3 in and the exemption still present: C1 and C2 must read 0 `kind=UnsafePointer` B2a rows. Any row is a
  site W1/W2 did not rewrite (a generic `*p` with `T = unsafe.Pointer`, a ref-lowered `ref unsafe.Pointer`
  parameter, a hand-owned door). It is a stop line, named before W4;
- then W4 deletes the exemption, and C1, C2 and every gate run after it.
The 18.3 stop line "W4 before B-2 reads 0" is now satisfied inside the cut.

**6. A pointer minted from a C struct's field goes through 3.2's number rule (18.2 item 3).**
- A non-zero word read from a C struct's pointer field mints `NativeBox<U>` ONLY when the number is C memory. A number
  inside a live pin, or one the exact registry knows (Go memory), is REFUSED by name, as the void door refuses it. If
  C kept a Go pointer past an unpin, Go would otherwise read and write memory that has moved.
- New arm: a C struct whose pointer field holds the address of a pinned Go array. Reading the field is refused by name.

**7. D-5: go2cs-gen's own output is measured (18.2's predictions).**
- D-5: the generated field-access source of a NON-cgo package set is byte-identical before and after. The set is the
  GolibTests and GenTests projects and the behavioral corpus's generated files.
- D-1 measures the converted corpus; D-5 measures what the generator emits from it.

**8. Two named deviations (nit).**
- (a) Unions are out: cmd/cgo declares a C union as a byte array, so it has no field accessor to cover. 18.2 item 1's
  `_Ctype_union_*` is withdrawn.
- (b) Go allows a Go pointer pinned by `runtime.Pinner` to be stored into C memory. go2cs refuses every Go pointer
  stored into C memory (18.2 item 4, W2(a)), pinned or not. This is a NAMED DEVIATION until a Pinner-aware store is
  designed.

**18.3's questions, as adopted.**
- (i) An edge is keyed by the pinned backing OBJECT plus the byte offset from the destination ADDRESS, never by the
  box's retained-source chain, so two views of one slot meet one edge set.
  - A stale edge after a non-W2 overwrite (a raw `uintptr` store) is a bounded leak: one box per slot, until the
    backing dies or a W2 to that slot displaces it.
- (ii) No re-carry by number (finding 4).
- (iii) CAS compares the word OR the referent identity. The word matches when it is the same number. An object
  released and pinned again can show a second number, so CAS also succeeds when the slot's edge for the current word
  names the same referent object as the expected value's.
- (iv) One cut (finding 5).

**OPEN, answered: is the edge's pin a strong GC handle? YES, and it cannot make a cycle.**
- A golib pin is `GCHandle.Alloc(..., GCHandleType.Pinned)` (PinnedBuffer.cs:48, :54), a STRONG handle that roots its
  object regardless of any other reference.
- But .NET pins only blittable objects (PinnedBuffer.PinOnly's remarks: `GCHandle` "pins only blittable objects and
  throws ArgumentException for anything else"). A blittable array holds no references, so what a pin roots cannot
  reach the destination's root, and no cycle runs through a pin.
- Everything else the edge holds lives in a `ConditionalWeakTable` keyed by the destination backing, which is an
  ephemeron. The value (the box) is kept only while the key lives, and a reference from the value back to the key does
  not keep the key alive. A reference-bearing pointee that reaches its own root is therefore collectible.
- **B-4 gains the cycle arm anyway, so the claim is measured, not argued:**
  - (c1) a reference-bearing pointee that references the destination's root is stored through W2; every outside
    reference is dropped; forced collections with finalizers run; a `WeakReference` to the root reads dead;
  - (c2) a value-type pointee is stored through W2; its root is dropped and collected; the pointee's pin leaves the
    pin-extent index (`EntriesOver == 0`, the diagnostic as cured at `0a51ac2399`).

**Predictions, revised.** These replace 18.2's D-2 for `s6next` and 18.3's B-2 and B-4.
- D-2: `s6nextnil`, `s6nextdirect` and `s6name` print what Go prints on three OSes. `s6next` prints the floor's
  refusal by name. `s6field`, `s6reinterp` and `s6slice` stay green.
- D-5: as in finding 7.
- B-2: 0 `kind=UnsafePointer` B2a rows in C1 and C2 on the cut's tree BEFORE W4, and again after it.
- B-4: the compaction arm, now four arms: value-type, reference-bearing (finding 3), and the two cycle arms c1 and c2.
  Today's exempt path is the red for the first two.
- B-6, new: the race arm for finding 2. Two threads repeat `atomic.StorePointer(&slot, a)` and `(&slot, b)` N times,
  then compacting collections run. The slot's word and its single remaining edge name the same object, and W1 reads
  it intact. On today's tree the arm is run before the cure. A stale or collected read there is the red; a clean
  run there is reported as such and is not counted as evidence for the design.
- B-7, new: the copy arm for finding 4. A word copied through `runtime.memmove` reads back through W1 at the
  destination as the same box. A word copied through `copy` over `[]uintptr` reads as a number pointer.

**Stop lines, added.**
- A W1 answer by number over managed storage.
- An edge removed before the word stops naming it.
- A copy door outside finding 4's list.
- A `kind=UnsafePointer` B2a row on the cut's tree before W4.

— H (2026-11-17)

### 18.5 Revision 2026-11-17: P3's re-review of 18.4 answered (before code)

P3 re-read 18.4 at `0e0f69717d`: APPROVE WITH CHANGES, before code (COORD inbox `20261117T221645Z-P3.md`).
- CLEAR: findings 1, 3, 5, 6, 7 and 8, and questions (i), (ii) and (iv).
- COORD accepted the re-review's findings below as worded.
This block REPLACES the parts of 18.4 it names, and the rest of 18.4 stands. The numbers here are the RE-REVIEW's.

**1. A counted bag per slot, not a set (replaces 18.4 finding 2's "edge SET").**
- With a set, a store of the word the slot already holds empties it. Word `a`, set `{a}`; `StoreWord(p, a)` adds `a`
  (already there), displaces `a`, removes `a`, and leaves word `a` with set `{}`. CAS(a, a) does the same. With two
  threads, T1 storing `a` and T2 storing `b` over word `a` can also end at word `a` with set `{}`.
- Each slot therefore holds a BAG of entries `(number, referent) -> count`:
  - adding increments the matching entry's count;
  - removing decrements exactly ONE count of one entry with the displaced number, preferring an entry whose referent
    is not the value the same operation just stored;
  - an entry at count 0 is dropped.
- INVARIANT: the entry for the slot's current word never reaches count 0. Every operation adds before it exchanges,
  and removes exactly what its own exchange displaced.
- ARM B-8, new and single-threaded, so it does not depend on timing: store the same pointer twice, then the same
  through CAS(a, a); drop every outside reference; run forced compacting collections with finalizers; W1 reads the
  same box both times.

**2. A W1 miss re-reads the word before it answers a number (replaces 18.4 finding 4's "otherwise it answers the
NUMBER POINTER").**
- A reader can load word `a` while a writer runs (a)-(c) for `b` and removes `a`'s entry. If the reader then looks up
  `a` and answers a number pointer, that pointer may name an object nothing keeps.
- Over managed storage, W1 now:
  1. reads the word `w`;
  2. looks up `w` in the slot's bag; on a hit, answers that referent;
  3. on a miss, re-reads the word. If it differs from `w`, it starts again at 1. Only an UNCHANGED word with no entry
     is a number, and it answers the number pointer.
- So W1 answers the old value or the new one, each with its entry, as Go's load does. The loop ends when no store
  intervenes between a read and its re-read.
- B-6 gains a THIRD thread that loops `atomic.LoadPointer(&slot)` during the store race. Every result is box `a` or
  box `b` by identity, never a number.

**3. The no-cycle argument was wrong for value-type pointees. Edges hold NO pin (replaces 18.4's "OPEN, answered").**
- 18.4 argued that a pin roots only a blittable object, which holds no references. That is true and not enough: the
  blittable backing can be the `ConditionalWeakTable` KEY, and the table's value reached the pin. So the pin roots the
  key, and the key is never collected.
  - SELF: a slot stores the address of its own backing `K` (a list sentinel, `n.next = n`): `CWT[K]` -> box -> pin on
    `K`.
  - MUTUAL: a slot in `A` names `B`, and a slot in `B` names `A`. Each pin roots the other's key.
- **The fix generalizes P3's (a) from self edges to every edge. NO edge holds a pin, directly or through a box.** An
  entry holds the REFERENT, by its kind:
  - a value-type referent: its root BACKING object plus the byte offset. W1 mints a fresh box over (backing, offset) on
    every hit, so no box, and no box's pin-only hold, lives in the table;
  - a reference-bearing referent (no pin exists; 18.4 finding 3): the referent BOX, as 18.4 has it.
- Everything an entry holds is then an ordinary strong reference inside the ephemeron, and no `GCHandle` is reachable
  from the table. The self and mutual cycles above are ephemeron cycles, which are collectible.
- **What this costs, stated as a NAMED DEVIATION.** Nothing keeps a value-type referent PINNED once its own pointers
  are gone, so it can move, and the word W2 wrote can go stale as an address.
  - W1 is unaffected: it resolves by the entry, matched on the word that was WRITTEN, not on where the object is now.
  - A program that reads the slot as a `uintptr` (not through W1) and converts that number back to a pointer can name
    a moved object. Go's unsafe rules forbid exactly that conversion of a stored `uintptr`, so a valid Go program
    never reaches it.
  - The visible difference is narrower: `uintptr` of W1's answer (a fresh mint at the object's current address) can
    differ from the word in the slot. Go's collector does not move objects, so in Go the two are always equal. This is
    a NAMED DEVIATION. CAS is unaffected, because it compares the word OR the referent (question (iii)).
- **ARMS c3 and c4 (B-4 now has six arms).**
  - c3, self: a value-type slot stores its own backing's address.
  - c4, mutual: two value-type backings each store the other's address.
  - Each drops every outside reference, runs forced compacting collections with finalizers, and then reads a
    `WeakReference` to each backing as dead and `EntriesOver == 0` over each.
- **COORD's (c) stands as written.** If c3 or c4 cannot go green at the cut, the cut STOPS and posts it, and COORD rules
  a named leak with a BOARD row before any code ships it. With no pin in the table, both are predicted green.

**4. memmove COPIES entries; it does not move them (replaces 18.4 finding 4's "moves the edges").**
- memmove leaves the source words in place, so the source's entries stay. For the copied range, the door:
  - drops the destination slots' existing entries, as an overwrite;
  - then adds, for each destination slot, one count of the source slot's entry for the word now copied there.
- B-7 gains two reads of the SOURCE slot through W1: one after the copy, and one after the destination backing has
  been dropped and collected. Both read the same box.

**5. The door list, corrected at master `2c1ce10b25` (replaces 18.4 finding 4's list). All line cites below were read
there.**
- **Copy doors:** the hand-owned `runtime.memmove` (`memmove_impl.cs`). The entry transfer sits ABOVE its dispatch,
  with byte offsets for both arms, because memmove tries the ELEMENT arm before the address arm (`:53`, `:77`, the
  raw `Buffer.MemoryCopy` at `:98`).
  - Converted `typedmemmove` and its `maps_` wrapper (`mbarrier.cs`) reach it, and so do the other runtime callers:
    slice, chan, string, select, stack and iface.
- **Not a copy today:**
  - `reflectlite`'s `typedmemmove` has an EMPTY body (`internal/reflectlite/value_impl.cs:18`);
  - `reflect`'s is a bodyless partial whose stub throws (`reflect/value.cs:2562`);
  - `internal/runtime/maps`' own `typedmemmove` refuses (`runtime_impl.cs`).
  Any of these becomes a door on the day it gets a body.
- **Clear doors, NEW:** `memclrNoHeapPointers` (`memmove_impl.cs:161`), and `typedmemclr` with `reflect_typedmemclr`
  and `reflect_typedmemclrpartial` (`mbarrier.cs:376`, `:397`, `:407`). A clear DROPS the entries in its range.
  Without that, a deleted indirect map key would stay referenced for the table's life.
- **The raw-copy grep, re-run at master** for `Buffer.MemoryCopy` / `Unsafe.CopyBlock` / `Marshal.Copy` in
  hand-owned `*_impl.cs`. Four files hit:
  - `memmove_impl.cs`, the door above;
  - three that copy byte buffers to and from native memory and never Go word storage: `cpu_darwin_impl.cs:47-54`
    (a sysctl name and its output), `signal_posix_impl.cs:205-206` (a sigaction), and `exec_libc2_impl.cs:560`
    (exec arguments). They are read and excluded.
  The cut re-runs the grep at its own tree, and a new hit on Go word storage is a stop line (P3's caveat).

**6. (iii)'s identity branch (nit).** When the observed word `w` differs from the expected number but the slot's entry
for `w` names the expected value's referent, the CAS is ONE interlocked `CompareExchange(slot, new, w)`, with the
OBSERVED word as the comparand. If it fails, because the word moved on, the CAS re-reads and re-evaluates. It answers
false only when a fresh read neither equals the expected number nor names its referent.

**Predictions, added and revised.**
- B-4: six arms: value-type, reference-bearing, and the cycle arms c1, c2, c3 and c4. c3 and c4 are predicted green
  (finding 3). If either reds, the cut stops.
- B-6: two writers and one reader; every load is box `a` or box `b` by identity.
- B-7: the destination reads the box; the SOURCE reads the box after the copy and again after the destination is
  collected; a `copy` over `[]uintptr` reads as a number.
- B-8, new: the single-threaded double-store and CAS(a, a) arm (finding 1).
- B-9, new: a clear arm. An indirect key is stored, its range is cleared through `memclrNoHeapPointers`, and a
  `WeakReference` to the key's object reads dead after collection.

**Stop lines, added.**
- A slot whose current word has no entry, at any check.
- A W1 number answer while the word moved between its read and re-read.
- A `GCHandle` reachable from the entry table.
- c3 or c4 red: stop, post, and wait for COORD's ruling (finding 3).
- A raw copy over Go word storage outside the doors in finding 5.

— H (2026-11-17)

### 18.6 Revision 2026-11-17: P3's re-review of 18.5 answered, and COORD's ruling on the pin (before code)

P3 re-read 18.5 at `51315b3568` together with H's addendum on the unpinned number (COORD inbox `20261117T222939Z-P3.md`):
APPROVE WITH CHANGES, before code.
- CLEAR: findings 2, 4, 5 and 6, and arms B-8 and B-9.
- NOT SOUND: 18.5 finding 3's generalization (no entry pins anything). COORD ruled on it.
This block REPLACES the parts of 18.5 it names, and the rest of 18.5 stands. The numbers here are COORD's ruling's.

**1. The pin is kept while the word is published (REPLACES 18.5 finding 3's "NO edge holds a pin").**
- An unpinned value-type referent makes the word in the slot a stale number, which fails three ways:
  - C can read it (finding 3 below);
  - 3.2's number rule calls a number Go memory only inside a LIVE pin or the exact registry, so a stale word is taken
    for C memory and dereferenced RAW, where under a pin it was refused by name;
  - the address is REUSED, so one number can name two objects. CAS(slot, y, new) would match `x`'s stale word; one bag
    could hold `(A, x)` and `(A, y)`; memequal and memhash would read `x` and `y` as one key.
- The entry for a VALUE-TYPE referent is therefore 18.4's: (the number, the referent BOX), and the box keeps its own
  pin-only hold, so the number stays the referent's while the word is published. A reference-bearing referent is held
  as its box, which has no pin (18.4 finding 3, unchanged). One number then names one live object, and 18.5's
  `uintptr` deviation is WITHDRAWN.
- **NO SELF EXEMPTION (COORD ruling (B), 2026-11-17, folded in before P3's read).** A self entry, whose referent
  backing IS the destination backing, is pinned like every other value-type entry.
  - An unpinned self backing `K` could move, and its own word would then name `K`'s OLD address `A`. A later object
    pinned at `A` would reopen the reuse hole for that word: CAS, memequal and memhash over it would read `A` as the
    other object.
  - Pinned, the self case is a BOUNDED LEAK of the same kind as c4 (finding 2), and "one number names one live object"
    holds with NO exception.
  - The shape is rare: it lives only in managed WORD storage written through W2, never in `container/list`'s
    `root.next = &root`, which is a CLR reference field and outside the table.
  - The CAS identity rule (18.5 finding 6, extended: when the bag holds an entry for the current word, CAS succeeds
    only if that entry names the same referent as `old`) is KEPT as defence in depth. With every published referent
    pinned it is redundant for uniqueness, and it is consulted only when the numbers differ, so it costs nothing on the
    matching path.
- Stop line: a published word whose value-type referent holds no pin. There is no exception.

**2. c3 and c4 (B-4): ONE NAMED LEAK covering both cycles, ruled before code.**
- c3, SELF: a value-type backing stores its own address through W2. It is a KNOWN-LIMIT arm: the backing reads ALIVE
  after the drop and the forced collections, and DEAD after its word is overwritten through W2, which measures the
  bound.
- **c4, the MUTUAL value-type cycle:**
  - Two Go backings each store the other's address through W2, with value-type referents. Each entry's pin roots the
    other's key, and neither backing is collected.
  - It is BOUNDED: it lasts until either word is overwritten through W2 or cleared, and that releases the cycle.
  - c4 stays a measured KNOWN-LIMIT arm that reads both backings ALIVE after the drop and the collections, so the
    leak's shape is pinned down. A run that reads them dead is NEWS, posted, not a pass.
  - A second step of c4 overwrites one word through W2, collects, and reads both dead, which measures the bound.
  - The BOARD row is written with this revision and covers BOTH cycles. The NAMED DEVIATION reads: "a self or mutual
    value-type cycle through W2-stored words is retained until a word in the cycle is overwritten or cleared".

**3. The cgo argument door refuses a crossed range that holds any entry (MUST-FIX, new).**
- The table is exactly cgocheck's information. Go panics "argument of cgo function has Go pointer to unpinned Go
  pointer" when Go memory handed to C holds a Go pointer. So 3.2's void door, before its call-scoped `fixed`, looks up
  the backing it would hand C. Any live entry in that backing is refused BY NAME with that text.
- **Read at master, per P3's caveat.** `GoCgoArgs.VoidPointer` (`GoCgoArgs.cs:122`) refuses ALL Go memory today, and
  3.2 is what opens it, so no check for word storage exists to reuse. The message does exist:
  `GoCgoArgs.PointerMember` (`GoCgoArgs.cs:88`) raises exactly that text for a typed struct member, and the new check
  reuses it.
- The whole backing is checked, not only the crossed bytes, because cgo lets C touch the whole object.
- A pointee pinned by `runtime.Pinner` is refused too, under deviation 8(b).
- Arm: a pinned buffer holding a W2-stored Go pointer is handed to a C function, and the refusal text is asserted. A
  cgo door that bypasses the check is a stop line.

**4. P3's conditional finding on the unpinned form: MOOT**, because the unpinned form is not kept (1).

**5. The re-read's ABA hole (should-fix; REPLACES 18.5 finding 2's miss rule).**
- A reader loads `a`; a writer stores `b` and removes `a`'s entry; the lookup misses; a writer stores `a` again; the
  re-read sees `a` "unchanged" and answers a NUMBER while `a`'s entry exists.
- FIX: a miss is CONFIRMED under the bag's lock. Holding the lock, W1 re-reads the word and looks up again.
  - The word equals what it looked up, with no entry: a number, and exact, because every add and remove of that bag
    happens under the same lock.
  - The word differs: start again.
  - A hit: answer that referent.
- The lock-free path stays for hits, which are the common case.

**6. The door list (nit).** `reflect_typedmemmove` and `reflectlite_typedmemmove` (`mbarrier.cs:219`, `:238`) also
reach `runtime.memmove` if called, and are listed beside `maps_typedmemmove` as covered. This is distinct from the
`reflect` and `reflectlite` packages' OWN `typedmemmove`, which 18.5 names as not a copy today.

**Predictions, revised.**
- B-4: c1 and c2, then c3 (self) and c4 (mutual) as known-limit arms: alive after the drop, and dead after a W2
  overwrite.
- B-10, new: the cgo door refuses a backing that holds an entry, with cgocheck's text (finding 3).
- B-11, new: the ABA arm (finding 5), driven deterministically through a test seam between the lookup and the
  re-read, so it does not rely on timing.
- Withdrawn: 18.5's uintptr deviation and its census.

**Stop lines, revised.**
- A published value-type word without a pin. There is no exception.
- A W1 number answered without the bag lock.
- A cgo door that hands C a backing holding an entry.
- c3 or c4 read dead without an overwrite (news, posted).

— H (2026-11-17)

### 18.7 Revision 2026-11-17: P3's re-review of 18.6 answered (text only, before code)

P3 re-read 18.6 at `ede548ea41` (COORD inbox `20261118T000620Z-P3.md`): APPROVE WITH CHANGES, TEXT ONLY.
- CLEAR: the pin, ruling (B), the one named leak for c3 and c4, the bag-lock miss, the door nit and the BOARD row.
- COORD accepted the three findings below as worded.
This block AMENDS the parts of 18.6 it names, and the rest of 18.6 stands. The numbers here are the re-review's.
Every code cite below was read at master `b2eaf5290a`.

**1. Every number W2 writes and W3 compares is taken through the PINNING projection (MUST-FIX; amends 18.6
finding 1's "the box keeps its own pin-only hold" and 18.3's "W2 writes `v`'s NUMBER").**
- The hole is real. The converter mints `unsafe.Pointer(p)` as `FromBox` (`convCallExpr.go:1128`). Over a managed box
  that is the TRANSIENT address of the box's value slot, read under a `fixed` that has already ended
  (`unsafe/unsafe.cs:550`). The box rides along as the referent, and nothing pins it. If W2 wrote that carried number,
  the word would be stale after the first compaction, and the entry would hold an unpinned box. 18.3 to 18.6 did not
  say which number W2 writes, so P3 could not tell. They say it now.
- **The rule.** W2 writes, and W3 compares, `BaseOf(v)` (`unsafe/unsafe.cs:1114`, design 10.1), never `v`'s carried
  number. `BaseOf` already exists and is what `unsafe.Add` derives from (`Pointer.Moved`, `:1138`):
  - for a pointer that names a managed box by address (`NamesAManagedSourceByAddress`, `:1102`: a `FromBox`,
    `FromBoxObject` or `FromPinnedBox` mint), it asks the box's own number NOW, through
    `IGoReflectBox.NativeArgumentAddress`, which is the box-to-`uintptr` operator (`ж.cs:1379`). That operator:
    - for a reference-free pointee, PINS the storage (`EnsureStableAddress`, `ж.cs:1455`, a pin-only hold for the
      box's lifetime), registers the address, and answers it;
    - for a reference-bearing pointee (`PointerStorage.None`, `ж.cs:1446`), registers and answers the box's ORDER
      TOKEN;
  - it uses the carried number as it is when the box is already pinned there (every `FromPinnedBox` mint), and for every
    other pointer: a native alias, the zerobase, a token, and a derived pointer, which `Moved` already built from
    `BaseOf`.
- **The check after the projection.** The word must be 0, a native alias, the zerobase address, a tagged token, or an
  address the referent box `IsPinnedAt`. The one kind that is none of these is `PointerStorage.Unpinnable`
  (`ж.cs:78`, an interior reference with no pinnable slot). For it the operator answers a transient address with no
  pin. W2 REFUSES that store by name: "unsafe.Pointer store: the stored pointer's storage can be neither pinned nor
  tokened". No silent store remains.
- **CAS.** W3 projects `old` the same way before it compares. Two `FromBox` mints of different boxes can carry
  coinciding transient addresses (`unsafe.cs:368`). Their projections cannot coincide: two live pinned objects never
  share an address, and two live boxes never share a token (`AllocationBase`, `ж.cs:710`: the id is unique below
  its 2^29 wrap). One cost is named: projecting an `old` that was never stored pins its box for that box's
  life, as any `uintptr(unsafe.Pointer(p))` conversion does today.
- **Can the number rule tell an order token from an address? YES, by the tag.** `IsTaggedToken`
  (`ж.PointerTokens.cs:424`, with `TagBit` and `TagMask` at `:255-256`) reads bit 63 set and bit 47 clear. No
  canonical x86-64 user address has that shape, so W1 takes a tagged word as a token (resolved through the registry,
  or held by its entry) and never dereferences it raw. The 3.2 doors already rely on the same predicate
  (`GoCgoArgs.cs:272`, `:292`).
  - The predicate states its own limit: it is x86-64's rule, and ARM64 owes its own derivation. 18.7 adds two readers
    of the predicate, W1 and W2, and no new assumption. B-12's arm64 legs measure it on the hosted arm64 runners.
- **ARM B-12, new.**
  - `x := new(struct{a, b int})` is stored through W2 as `*(*unsafe.Pointer)(slot) = unsafe.Pointer(x)`. Every other
    reference to `x` is dropped, and forced compacting collections run.
  - W1 reads the same box, and `x.a` is intact. The word in the slot is an address `x`'s box `IsPinnedAt`.
  - Then `CompareAndSwap(slot, unsafe.Pointer(y), z)`, with a second such box `y`, FAILS and leaves the word unchanged.
  - A third leg stores a reference-bearing `new(struct{s string})`: the word is a tagged token.
  - A fourth leg stores an `Unpinnable` pointer and asserts the refusal text.
- **STOP LINE:** a W2 word equal to a `FromBox` transient address that its box is not pinned at.

**2. Every GoCgoArgs door that hands over a Go backing's bytes, with its decision (SHOULD-FIX; extends 18.6 finding
3).** The lookup is the one 18.6 finding 3 defines: the destination bag table's entries over the backing.
- **`VoidPointer`** (`GoCgoArgs.cs:122`): the whole backing is looked up, and any entry is refused with cgocheck's text
  (18.6 finding 3, unchanged).
- **`HelperVoidPointer`** (`:137`, `C.GoBytes`) and **`HelperPointer`** (`:152`, `C.GoString`, `C.GoStringN`): the
  same lookup and the same text, as P3 worded it.
  - Named for the confirm read: these helpers run in Go, and cgocheck does not apply to them, so Go itself accepts a
    Go backing that holds a Go pointer here. The refusal is therefore STRICTER than Go. It is a NAMED DEVIATION that
    fails closed. P3's confirm read may drop it to "no lookup: the bytes are copied as numbers, and Go keeps nothing
    alive by them".
- **`StructPointer`** (`:42`, the mirror copy). `PointerMember` (`:88`) refuses typed pointer members, but a by-value
  array field or `uintptr` field copies its words RAW. The SOURCE backing is looked up over the copied range, and any
  entry there is refused with cgocheck's text. The copy-back after the call is an overwrite of that range: it drops the
  range's entries, as a clear door does (18.5 finding 5). Because the call was refused otherwise, that set is empty
  unless another thread stored into the range during the call.
- **`CopyBytes`** (`:190`, `C.CBytes`) copies the words into C-owned memory, where no entry follows. A source range
  holding an entry is REFUSED, with W2(a)'s text, consistent with W2(a): no Go pointer is stored into C memory.
  `CopyString` (`:182`, `C.CString`) takes the same lookup over the string's bytes.
- **Callbacks** (Go exports called from C) are Phase 5 and OUT of 3.2. A Go export reachable from C in 3.2's tree is a
  stop line.
- **B-10** gains one arm per door: `VoidPointer`; `HelperVoidPointer`; `HelperPointer`; `StructPointer` with an array
  field and with a `uintptr` field; `CopyBytes`; and `CopyString`. Each stores a Go pointer through W2 into the
  backing, crosses the door, and asserts the refusal text.

**3. The c3 and c4 procedure (SHOULD-FIX; amends 18.6 finding 2 and the BOARD row's "Measured by").**
- **One test, three arms, one loop.** c2 (no cycle), c3 (self) and c4 (mutual) run in the same test method:
  - **c2 is the CONTROL** that the loop collects. Its backing must read dead within the cap.
  - If c2 does not read dead, the test FAILS as an instrument failure. That run says nothing about c3 or c4.
- **The loop.** One round is `GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting:
  true)` followed by `GC.WaitForPendingFinalizers()`.
  - The cap is **10 rounds**, the same for both steps.
  - Each step runs all 10 rounds and records the round in which each backing first read dead, so the cap's margin is
    posted.
  - c4 needs several rounds by construction:
    1. the overwrite drops `A`'s entry;
    2. a finalizer frees `B`'s pin, and `B` dies;
    3. `B`'s table value, which holds `A`'s box, is released;
    4. that box's finalizer frees `A`'s pin, and `A` dies.
- **Step one (the leak).** The test keeps ONLY a `WeakReference` to each backing. Every store runs inside a
  `[MethodImpl(MethodImplOptions.NoInlining)]` helper, so no local in the test frame roots a backing, under Debug
  lifetimes too. After the 10 rounds, c3's and c4's backings read ALIVE, and c2's reads dead.
- **Step two (the bound) reaches the slot through the `WeakReference`'s target, which is alive BECAUSE of the leak.**
  - A `NoInlining` helper calls `TryGetTarget`; a false there is news, posted, and not a pass.
  - The helper overwrites one word of the cycle through W2 with nil and returns nothing, so the strong reference dies
    with its frame.
  - After 10 more rounds, every backing in the cycle reads DEAD.

**Predictions, revised.**
- B-4 runs as finding 3's single test: c2 dead in step one; c3 and c4 alive after step one and dead after step two, all
  within 10 rounds.
- B-10 is seven arms (finding 2).
- B-12 is new (finding 1). Before the cure, a W2 that wrote the carried number would fail its `IsPinnedAt` leg.

**Stop lines, added.**
- A W2 or W3 number not taken through `BaseOf`.
- An `Unpinnable` store accepted silently.
- A GoCgoArgs door that hands over a backing's bytes without the lookup, other than a helper door that P3's confirm
  read releases.
- c2 not dead within the cap.

— H (2026-11-17)

### 18.8 Record 2026-11-20: seat S-B1 as built, the rulings it carries, and its first battery

S-B1 is the golib and converter half of 18.3 to 18.7. Its seat runs on branch `claude/h-cgo-32`, on master `a98c5175be`.
S-B2 comes after it, on hosted legs: the pinned class, `[GoCgoStruct]` with the gen accessor (D-1 to D-5), and the
B-10 door lookups.

**As built.**
- **The doors.** `unsafe.Pointer.LoadWord`, `StoreWord`, `SwapWord` and `CompareAndSwapWord` decide by the slot
  (`GoPointerWords.Classify`):
  - a CLR `Pointer` slot takes `.Value`;
  - a word of Go memory takes the per-slot bag table (`golib/GoPointerWords.cs`);
  - a word of C memory refuses a Go pointer: W2(a) refuses the store, and W1 refuses a Go address read out of C memory;
  - an unheld word refuses by name.
- **The number.** Every word W2 writes and W3 compares is the projection, `ProjectWord` over `BaseOf`, with 18.7
  finding 1's post-check. Ruling (B)'s guard refuses a number that is both `IsPinnedAt` and `IsTaggedToken`.
- **W3** runs through the doors in the hand-owned atomics: `sync/atomic/doc_impl.cs` (Swap, CompareAndSwap, Load and
  Store over `unsafe.Pointer`) and `internal/runtime/atomic/atomic_impl.cs` (`storePointer`, `casPointerLatched`).
- **The memmove and clear doors.** `runtime/memmove_impl.cs` copies the entries over the moved range
  (`GoPointerWords.CopyEntries`) and drops them over a cleared range (`ClearEntries`). Both are free while no table
  exists.
- **W4.** `NativeBox<T>` refuses a reference read for `unsafe.Pointer` too: the floor's exemption is deleted.
- **The converter.** `src/go2cs/unsafePointerWordDoor.go` emits `*p` and `*p = v`, for `p` exactly a
  `*unsafe.Pointer`, as the load and store doors. One hand-owned site takes the door by hand: `runtime/mfinal.cs`'s
  single store.
- **Found during the cure.** `Pointer.StoreThrough` and `LoadThrough`, the bare-Pointer atomic primitives, reached a
  word of Go memory too. They gain the word route (`WordSlotForThrough`): a slot that holds a `uintptr` whose
  reinterpret classifies as Go memory goes through `StoreWord` / `LoadWord`.
- **Untouched by design.**
  - `runtime/profbuf_impl.cs` (hand-owned) reads `*tagPtr` through a `ж<unsafe.Pointer>` that names a CLR `Pointer`
    slot, the sampler's labels, never a word. `.Value` is that slot's value, so the read keeps its form.
  - The hand-owned `runtime/mcleanup.cs` replaces the Go body whose `*(*unsafe.Pointer)(unsafe.Pointer(&arg))` reads
    reach the door. Only its review sibling, `mcleanup.cs.auto`, carries them, and the narrowing below returns those
    to master's text.

**Ruling (B): the arm64 derivation (owed by 18.7 finding 1).**
- `IsTaggedToken` reads bit 63 set and bit 47 clear. A word reads as a token only with bit 63 set.
- On linux/arm64 and darwin/arm64, a user address has bit 63 clear unless top-byte tagging is in use.
  - Linux enables tagged addresses per process: the tagged address ABI, `PR_SET_TAGGED_ADDR_CTRL`, which MTE builds
    on.
  - darwin's pointer authentication signs pointers only in arm64e processes.
  - The .NET runtime enables neither, and an arm64 (not arm64e) process is what it ships.
  - So no address the GC heap, a pin or the native allocator hands out reads as a token, and no token (bit 63 set)
    is a user address.
- **The named residual.** A C library built for MTE or HWASan could hand back a top-byte-tagged pointer. No 3.2
  library is one. The B-12 arm64 legs (S-B2, hosted runners) measure the rule there. Until they do, the arm64 claim
  is derived, not read.

**One lock per storage object.**
- A load holds the storage's lock while it reads the word and looks it up. A store holds it while it adds, exchanges
  and removes.
- There is no lock-free hit path. A load first reads the word without the lock only to learn whether a table exists,
  then re-reads it under the lock before any lookup.
- So 18.6 finding 5's ABA window, a number answered while its entry is being removed, is designed out rather than
  narrowed.

**P3's confirm items, folded at the cut.**
- **(i)** B-12's CAS arm carries the positive control: a CAS whose `old` is the stored pointer SUCCEEDS. So the
  failing CAS fails on the projection, not on a CAS that cannot succeed.
- **(ii)** In `GoPointerWords.CompareAndSwap`, when the word is a token and an entry exists for it, the swap requires
  `ReferenceEquals` between `old`'s referent and the entry's. A token number match alone does not swap.
- **(iii)** `StructPointer`'s copy-back after the call overwrites the copied range, so a store from another thread
  during the call is lost. That is the same as Go's outcome. In Go, C writes the struct in place, and a Go store
  racing the C call on the same memory is a data race: Go defines no winner, and a lost Go store is one outcome it
  permits.
- **(A)** The helper doors take no lookup (S-B2).

**The named leak, in its general form (BOARD row widened 2026-11-20).**
- The form: a published word whose referent reaches the box that pins the destination's storage. The destination's
  own BaseOf pin roots the table key, so the slot is retained until a word of the cycle is overwritten through a store
  or cleared.
- Instances, all measured in `UnsafePointerWordTests.B4_…`, with the c2 control dead within the cap:
  - c3, a self word;
  - c4, a mutual pair;
  - c1, a referent that holds the destination's box. COORD ruled c1 inside the leak, and that ruling widened the row.

**B-1, the footprint prediction, kept visible.**
- Predicted: 82 lines in 14 files per target. Measured: 18 to 19 files and about 91 code lines.
- MECHANISM HELD, SPECIFICS MISSED. The census behind the prediction was blind to three forms:
  - the `Ꮡ(..)` temp box of the reinterpret carrying arm;
  - its func-word form, `Ꮡ(OfFunc(f))`;
  - a package variable declared in another file (`cgo_yield`).
- The narrowing below takes the first two out of the footprint again. The third stays on the door by rule.

**The first battery, at `88d891e84d`: RED, scored as worded.**
- **Scores, all unfiltered:**
  - GolibTests Release and Debug: empty failed sets.
  - GenTests: all pass.
  - B-2: 0 `kind=UnsafePointer` rows in C1 and C2.
  - B-3: C2 executed, with 0 output failures.
  - MISSED: go test (2 packages FAIL), CNR (1 golden), repoguard, and one C1 test (census on).
- **Four missed sites, posted before any fix:**
  - **M1** — `NativeBoxDereferenceCensusControlTests` read `.Value` over a native `unsafe.Pointer` box, the most-likely
    red, in a census-gated test.
  - **M2** — the door rewrote the carrying arm's temp boxes in `unsafePointerWordRead_test.go` and the
    `UnsafePointerWordRead` golden.
  - **M3** — the door file had no AGPL header and was missing from the projitems.
  - **M4** — 20 provenance notes and one docs sample moved, and `FromWord`'s mint row was missing.
- **Attribution.** Each red test was run FILTERED at master `a98c5175be` on H: every one passes there.
- **COORD's ruling on M2: NARROW the door.**
  - The door skips (a) the carrying arm's fresh `Ꮡ(<rvalue>)` box at any level, and (b) `&x` through the identity
    round trip, where `x` is a local or a parameter.
  - Over those slots the doors take `.Value` anyway, so the call bought no behavior and cost the Go-like shape.
  - A predicate mistake fails loud, as a W4 refusal by name.
  - The door test is red both ways (the predicate forced false, then true), and its text is in `pointers.md`.
- **The fixes.**
  - M1 asserts the refusal by name, with the B2a counter unmoved: the control that makes B-2's zero a reading.
  - M3 adds the header and registers both files.
  - M4 re-cites the notes and adds the `FromWord` row: the loaded word's number is the stored projection, never
    `FromBox`'s transient.
- **The re-predicted footprint.**
  - Exactly 5 door lines leave each target, each reverting 1:1 to master's text: `debugcall.cs` 1, `tracetype.cs` 1,
    `mcleanup.cs.auto` 2, and the per-OS `cgocall.cs` 1.
  - No map line moves.
  - The committed footprint goes from 27 files +115 -117 to 24 files +108 -110.

**Recorded for the BOARD after READY, not fixed in this cut.** `*(*unsafe.Pointer)(unsafe.Pointer(&f)) = ..` over a
func variable stores into a fresh box, so the write is lost. The sites are `debugcall.cs` (dispatchF) and
`cgocall.cs` (`cgocallbackg1`'s cb). The narrowing keeps master's form there exactly, and `funcWordOperations` does
not take the shape.

**Amended 2026-11-20 (later): P3's review of `780e8c97e0`, APPROVE WITH CHANGES, and the changes.**
- **MUST.** The doors' managed arm read `p.Value`, which throws a nil dereference on a standard box whose held
  `unsafe.Pointer` is null, Go's nil. Every `*unsafe.Pointer` parameter load and store changed this way, because the
  alias the door replaced (`DerefOrNull`) read `ValueSlot`.
  - Red first: `UnsafePointerWordTests.W1W2_AManagedSlotHoldingNilLoadsNilAndTakesAStore`, and the Go scenario as the
    behavioral project `UnsafePointerWordNilSlot` (exported `Set`/`Get`, whose parameters keep the box; unexported
    ones ref-lower and never reach the door). Red at `780e8c97e0`: a `PanicException` from `StandardBox.Value` through
    `LoadWord`, and the program's exit 2 against Go's 0.
  - Cure: after `RequireSlot`'s structural nil test, both arms take `ValueSlot`. `NativeBox`'s `Value` and `ValueSlot`
    both go through `ValueAddress`, so W4's floor is unchanged.
  - A master defect closed on the way: master emitted `pp.Value = v` for `pp := new(unsafe.Pointer); *pp = v`, which
    throws the same way. The cut emits the store door, now nil-safe.
- **SHOULD 2.** Shape (a) asks `pointerReinterpretManagedSource` itself, so the predicate cannot drift from the
  carrying arm.
- **NIT 3.** Shape (b) applies only when no star was stripped: `**&x` reads the slot out of `x`. The door test's
  `nativeStarredLocal` arm fires by name with the guard forced off.
- **NIT 4.** The door's header names the one silent case, master's: a store through shape (a) (the BOARD's (ff) row).
- **NIT 5.** `FromWord`'s mint row names the third no-referent case: a word of Go memory with no entry.
- **Found while applying the MUST, ruled by COORD.** Shape (b)'s narrowed STORE kept master's `(Ꮡl).Value = v`, which
  panics when `l` holds nil, where Go stores. A store through (b) now emits `(Ꮡl).ValueSlot = v`, the converter's own
  convention for a held reference (`pp.ValueSlot = Ꮡx`). Loads through (b) already read `~Ꮡl`, which is `ValueSlot`.
  Shape (a)'s store keeps its emission: that is the lost write, not this. Corpus sites: none.

### 18.9 Amendment 2026-11-21: seat 1a (S-B2a), the field accessor for a C struct's pointer field (P3: APPROVE WITH CHANGES, folded in before code)

P3 reviewed the first text (COORD inbox `20261121T141854Z-P3.md`): APPROVE WITH CHANGES. COORD ruled every change into
this text before code: MUSTs 1-3, all SHOULDs, and three more stop lines. Each is marked [P3 Mn] or [P3 S] where it
lands.

1a is the part of 18.2 (as amended by 18.4) that retires three of the four section-6 pins: s6nextnil, s6nextdirect and
s6name (claude/h-cgo-s6-pin, train 2). s6next's pin stays: it is the reference floor's refusal, which 18.4 finding 1
rules for 3.2. The owner placed 1a in .5 (11-21). 1b (the pinned class, shape (i), B-10, B-12) moves to .6, and so does
1c (the `**C.T` load and store, 18.10). The seat is cut off master after train 2 lands, so the pin is under it.

**1. The marker (converter).** `visitStructType.go` stamps `[GoCgoStruct]` beside `[GoValueClone(…)]`. It is movable,
so it rides the type's accessibility record, which for a C type is the pseudo-package's
(`applyCgoPseudoPackageStamp`). The stamp goes on a struct only when ALL THREE hold:
- [P3 M1] the package is converting on the -cgo route (`cgoRouteActive`, cgoRoute.go:82). The KIND alone is decided from
  the NAME (cgoDeclarations.go:142-145). So without this condition, a hand-written Go type named `_Ctype_struct_x` in a
  package that does not import "C" would be stamped. On the route, cmd/cgo reserves the prefix, so the name is cmd/cgo's
  own declaration.
- the named type is one cmd/cgo declared as a C struct (`cgoTypeKind` is `cgoStruct`, prefix `_Ctype_struct_`);
- at least one field's type is a pointer (`*T`) or `unsafe.Pointer`.
The generator reads only the marker. The corpus converts at CGO_ENABLED=0, off the route, so it moves 0 lines (D-1).
- [P3 S] Out of 1a, each by name (each keeps today's `ref` property and today's refusal over C memory):
  - a field whose type is a DEFINED pointer typedef (`type _Ctype_xptr *_Ctype_struct_x`): its C# type is a named
    pointer wrapper, not `ж<U>`;
  - a pointer field reached through a NESTED struct value (`l.inner.p`);
  - an array of pointers (`T *a[4]`);
  - unions (18.4, 8a).

**2. The accessor (go2cs-gen, `FieldAccessGenerator.Properties`).** [P3 S] The generator's contract is THE MARKER,
however it was stamped. Darwin's resolver walks a Go-declared mirror struct (cgo_unix_syscall.go), so .6 adds a stamping
rule, never a second generator path. For a type that carries `[GoCgoStruct]`, a field whose C# type is `ж<U>` gets a
GET/SET property in place of the `ref` property:

    public ж<U> f { get => ʗp.cgoPointerField(T.Ꮡf); set => ʗp.cgoPointerField(T.Ꮡf, value); }

- A field whose C# type is `unsafe.Pointer` gets the same shape over S-B1's word doors, through a new slot member:
  `get => global::go.unsafe_package.Pointer.LoadWord(ʗp.cgoWordField(T.Ꮡf))`, and the set is
  `StoreWord(ʗp.cgoWordField(T.Ꮡf), value)` (unsafe.cs:1397, :1431).
  - [P3 M2] `cgoWordField` exists for the unknown-offset refusal. A bare `ʗp.Ꮡf` with no Go offset is not a
    NativeBox: Classify answers NotAWord, and the door would take the CLR slot of a struct in C memory, which is the
    wrong bytes, silently. `ж<T>.cgoWordField<TF>(FieldRefFunc<T, TF>)` returns:
    - a managed receiver: `of(f)`, unchanged;
    - a native receiver: `of(f)` when the field's Go offset is known, else the same by-name refusal as item 3.
  - The Classify premise holds by reading: `of` over a native receiver is `FieldRefBox.NativeFieldBox()`, and
    `NativeDerivation.Kept` returns null for a C-memory-alone root (ж.NativeDerivation.cs:77). Arm (5) measures it.
  - Over C memory, W1/W2 then decide as S-B1 built them. Over PINNED Go memory, the same doors take the per-slot bag
    table, where a Go pointer is legal (item 4b).
- Every other field of the type keeps its `ref` property.
- The field's ADDRESS property (`AddAddress`) is unchanged. So `&l.next` still compiles and still reaches the reference
  floor: s6next's refusal text is predicted byte-identical.
- A `ref` to a marked pointer field no longer compiles, by design, as for a header-shaped struct.
- A type both header-shaped and marked cannot occur in the corpus. Should one occur, the marker decides its pointer
  fields, since no golib header box views a C struct.

**3. The read (golib, new `ж<T>.cgoPointerField<U>(FieldRefFunc<T, ж<U>>)`).** [P3 M3] There are THREE receivers:
managed, native over C memory alone, and native over pinned Go memory (a C struct laid over a Go buffer, s6slice's
shape).
- A MANAGED receiver reads its slot, `fieldRefFunc(ref Value)`: a C struct value Go allocated. Its behaviour does not
  change: that is a stop line.
- A NATIVE receiver over C MEMORY ALONE reads the address word at the field's Go offset
  (`FieldRefBox<ж<U>>.GoFieldOffsetOf`, the offset s6field already reads):
  - 0 is nil;
  - a word that `GoPointerWords.NamesGoMemory` reports (a token, an address the exact registry knows, or one inside a
    live pin) is REFUSED: "cgo struct field load: a Go pointer read out of C memory is refused (C memory must not hold a
    Go pointer)". That is 18.4 finding 6, worded as S-B1's W1 refusal is.
  - [P3 S] A Pinner-pinned Go pointer that C kept in the field is refused by the same text. That is the read half of
    18.4's named deviation (8b), named in Limitations.
  - Any other word mints `new NativeBox<U>(word)`, as `GoCgoArgs.PointerMemberBack` mints a pointer C returned.
- [P3 M3] A NATIVE receiver over PINNED GO MEMORY reads the word at the Go offset:
  - 0 is nil;
  - a C-memory number mints `NativeBox<U>`;
  - a word that names Go memory is REFUSED with its own text: "cgo struct field load: a Go pointer in a C struct laid
    over Go memory is not read in 3.2 (go2cs keeps no record of the pointer stored there)". Go allows the value; go2cs
    names it as a deviation and never resolves a box by number over managed storage (18.4 finding 4).
- Any native receiver whose Go offset is unknown (`GoFieldOffsetOf` < 0) is REFUSED by name. It never falls back to the
  CLR slot, which would read the wrong bytes.

**4. The write (`ж<T>.cgoPointerField<U>(FieldRefFunc<T, ж<U>>, ж<U>?)`).**
- A MANAGED receiver stores into its slot, unchanged.
- (4a) A NATIVE receiver over C MEMORY ALONE stores a word:
  - nil (or a nil box) writes 0;
  - a box over C memory alone (`GoPointerWords.IsCMemoryAlone`) writes its `NativeAddress`;
  - anything else is REFUSED: "cgo struct field store: storing a Go pointer into C memory is refused (C memory must not
    hold a Go pointer)". That is S-B1's W2(a), and 18.4's named deviation (8b) for a Pinner-pinned pointer stands.
- (4b) [P3 M3] A NATIVE receiver over PINNED GO MEMORY:
  - nil writes 0;
  - a C pointer writes its address;
  - a Go pointer, which is LEGAL Go there, is REFUSED with its own text: "cgo struct field store: a Go pointer stored
    into a C struct laid over Go memory is not supported in 3.2". Here the C-memory text would be wrong, since the
    memory is Go's. It is named in Limitations.
  - An `unsafe.Pointer` field there needs no refusal: the word doors' per-slot bag table already stores a Go pointer
    into pinned Go memory (S-B1).

**5. Public surface.** golib gains one attribute type (`GoCgoStructAttribute`, `[EditorBrowsable(Never)]`) and THREE
members on `ж<T>`: the two `cgoPointerField` overloads, and `cgoWordField` [P3 M2]. PublicSurface.golib.txt gains exactly
FOUR lines. This text's first version said three, and M2's member is the fourth. Route #7 runs both families.

**Limitations (named deviations).**
- 8b: go2cs refuses a Go pointer stored into C memory, pinned or not. Go allows a `runtime.Pinner`-pinned one. [P3 S] The
  read half: a Pinner-pinned Go pointer that C kept in a field is refused on read too.
- [P3 M3] In a C struct laid over pinned Go memory, a typed pointer field neither reads nor stores a Go pointer, each
  refused by name, though Go allows both. An `unsafe.Pointer` field there does both, through the word doors.
- The also-outs of item 1.

**Predictions** (scored as worded, at the cut):
- D-1: corpus footprint 0 files on four targets, by the two-seeded reconvert.
- D-2: s6nextnil, s6nextdirect and s6name print what Go prints on 5 RIDs; their pins are removed in the same seat.
  s6next's text is byte-identical. s6field, s6reinterp and s6slice stay green.
- D-3: GenTests:
  - a marked struct's `ж<U>` and `unsafe.Pointer` fields get the GET/SET shape;
  - its scalar, array and struct fields keep `ref`;
  - an unmarked struct with the same fields keeps `ref` everywhere (the near-miss arm);
  - a compile arm asserts that a `ref` to a marked pointer field does not compile.
- D-3b [P3 M1]: a converter near-miss arm. A hand-written `type _Ctype_struct_x struct{ next *_Ctype_struct_x }` in a
  package that does not import "C" converts with NO stamp.
- D-4: a new e2e arm. A C pointer stored into a C struct's pointer field reads back in C, and a Go pointer stored there
  is refused by name.
- D-5: go2cs-gen's non-cgo output is byte-identical (GolibTests, GenTests, and the behavioral generated files).
- D-6: the offline phase1 fixture's `_Ctype_struct_x` (field `next *_Ctype_struct_x`) converts with the stamp, a
  converter-test arm. `_Ctype_struct___0` in phase2 (no pointer field) converts without it.
- Finding 6: a new e2e arm. A C struct field holding a pinned Go array's address is refused on read.
- (5) [P3 S] the Classify-first GolibTests arm (`CgoStructFieldWordSlotTests`) reads FOUR receivers:
  - a bare C root: CMemory;
  - a root minted by `cgoPointerField` (`l.next.next`): CMemory;
  - a root that owns a pin over Go memory: NOT CMemory (GoMemory);
  - a managed receiver: NotAWord.
  The bare-root and typed-field halves exist locally; the minted root needs the member.
- (6) [P3 M2] the unknown-offset arm: an `unsafe.Pointer` field over a native receiver whose Go offset is unknown is
  refused by name through `cgoWordField`, and C memory is left unchanged.
- (7) [P3 M3] the pinned-Go-memory rows:
  - a typed pointer field reads 0 as nil and a C number as a NativeBox;
  - a Go pointer is refused on read and on store, each with its own text;
  - an `unsafe.Pointer` field stores and loads a Go pointer through the word doors.
- (8) [P3 stop line] the managed round trip `var x C.struct_s; x.p = q; r := x.p`, for both field kinds, including a Go
  pointer: `r` is `q`, identical before and after the seat.

**Stop lines** (18.2's still stand):
- any unmarked type's accessor changes;
- a `ref` to a marked pointer field that the converter emits today;
- s6next's text moves;
- [P3] the marker appears on any type in a package that does not import "C";
- [P3] PublicSurface.golib.txt gains more than this text's four lines, or any generated file outside a cgo package
  changes;
- [P3] the managed receiver's behaviour changes (arm 8).

— H (2026-11-21)

**Record 2026-11-21: P3's seat read of 1a (CLEAR), and the SHOULDs folded before seating.** P3 read `01ecb22bb9` (COORD
inbox `20261121T210538Z-P3.md`): CLEAR, with the three MUSTs met. COORD ruled the three SHOULDs and two NITs in, as one
commit, before seating.
- SHOULD 1. The store admitted a C pointer only as a `NativeBox<U>`. An element of a native-backed slice that keeps no
  anchor is C memory alone too (`IsCMemoryAlone`, ж.ElemRefBox.cs), so `e.ip = &s[1]` over
  `unsafe.Slice((*C.int)(C.malloc(16)), 4)` was refused as a Go pointer. The store now tests `IsCMemoryAlone` on the
  value and writes the box's native identity.
  - Row: `AFieldOverCMemoryStoresAnElementOfCMemoryByItsAddress`.
  - Red at the pre-fix golib (filtered CgoPointerFieldTests, Release: 1 failed of 9, with "storing a Go pointer into C
    memory is refused"), and green with the fix (9 of 9). Both were predicted before the run.
- SHOULD 2. M3's `void *` half now has a row: `AnUnsafePointerFieldOverPinnedGoMemoryRoundTripsACWordAndAGoPointer`.
  Through `cgoWordField` and the word doors, a C word round-trips as a number at the field's Go offset, and a Go pointer
  is stored and read back as its box.
  - It is a coverage row of behaviour this section already states. It is green at both arms, and no fold code changes
    it.
- SHOULD 3. M1's off-route gate was made to fail once, by safety-floor item 13.
  - The arm: the `cgoRouteActive` test at cgoDeclarations.go:303 dropped, uncommitted, and one unfiltered CNR at
    `01ecb22bb9`. The prediction was posted to COORD before the run.
  - The reading: rc 1, "CHANGED converter output", naming exactly one file,
    `src/tests/Behavioral/CgoPseudoPackageTypes/package_info.cs`. In it, `_Ctype_struct_point` gains
    `[global::go.golib.GoCgoStruct]` beside `[GoValueClone("name")]`. NOT MEASURED was 0. MET as predicted.
  - Restored, and byte-identical (sha256 of cgoDeclarations.go equal before and after), with a clean tree.
- NIT 4. FieldAccessGenerator.cs names the header-shaped-plus-marked pair in a comment. It is not reached today.
- NIT 5. The read's mint carries a comment: each read is a fresh box, `==` compares by address, and a ReferenceEquals
  site would tell two reads apart. The `**C.T` load (1c) mints the same way.
- Re-cite: NIT 4's three comment lines move FieldAccessGenerator.cs by +3 below :357. ConversionStrategies.md:11576's
  `:361` becomes `:364`, and `:668-684#f10245` becomes `:671-687#f10245`. The span is byte-identical, so its
  fingerprint holds.

### 18.10 Record 2026-11-21: the resolver's walk takes the field's ADDRESS (18.2 item 5 and 18.4 finding 1 corrected; no rewrite)

18.2 item 5 and 18.4 finding 1 say getaddrinfo's walk reads "through the field, not its address". The source says
otherwise:
- go1.24.13, net/cgo_unix.go:150 and :216: `for r := res; r != nil; r = *_C_ai_next(r)`;
- cgo_unix_cgo.go:71: `func _C_ai_next(ai *_C_struct_addrinfo) **_C_struct_addrinfo { return &ai.ai_next }`;
- :68 returns `&ai.ai_addr` the same way;
- darwin's cgo_unix_syscall.go:53/:56 returns `&ai.Addr` and `&ai.Next`.
The walk takes the address of a C struct's pointer field and loads through it: s6next's shape, which 18.4 keeps refused
in 3.2. So the resolver (increment 3.4) needs 1a AND 1c. 1c is the `**C.T` load and store over C memory, 18.4's "later
increment", which retires s6next's pin. It is placed in .6 beside the resolver. Only the reinterpret AFTER
`*_C_ai_addr(r)` is s6reinterp's shape. Found while sizing 1a, and posted on its own before any plan used it.

**Deltas found while coding (2026-11-21), marked for P3, as COORD ruled: not a new pre-code read.**
- (U) A FOURTH field-slot kind. The four-receiver Classify arm first read Unheld where GoMemory was predicted, through
  `Reinterpret` over a Go `slice<byte>`, which holds no pin. The miss was posted on its own. The settling arm over both
  mints then read 5 of 5: the converter's own mint for `(*C.struct_x)(unsafe.Pointer(&buf[0]))`,
  `To<T>(FromPinnedBox(Ꮡ(buf, 0)))`, is GoMemory, and Reinterpret is Unheld. So items 3 and 4 have a fourth row: a
  native receiver whose field slot is UNHELD refuses a read and a store of either field kind with S-B1's
  UnheldSlotText, word for word. That adds no text and no public member.
- (D1) `GoCgoStructAttribute` lives in namespace `go.golib`, not `go`: PublicSurface.golib.txt's own rule refuses a new
  type in `go` that is not a Go name. An unlisted `go.golib` type adds no line, so the surface gains THREE lines (the
  three `ж<T>` members), not item 5's four.
- (D2) The converter spells the stamp `[global::go.golib.GoCgoStruct]`. It rides package_info.cs's accessibility record,
  which carries no `using go.golib;`, and the generator reads the attribute symbol, never its spelling.
- (C) `src/go2cs/fieldAccessCollisions.txt` gains `cgoPointerField` and `cgoWordField`. GenTests'
  TheCollisionListNamesEveryMemberOfTheBox requires every public `ж<T>` member there. The GenTests prediction missed on
  it and was posted on its own before the fix. A Go field or method spelled like either now takes the converter's
  explicit form; D-1 and CNR read whether any does.
- Arm (finding 6) stays a GolibTests arm (`AFieldOverCMemoryHoldingAGoPointerIsRefusedOnRead`), not an e2e one. In Go
  code, handing C a Go pointer to store is itself refused by 3.1's door ("increment 3.2") before any field read, so no
  e2e program reaches the read refusal until 1b.

### 18.11 Amendment 2026-11-21: seat 1c, the `**C.T` load and store over C memory (P3: APPROVE WITH CHANGES, folded in before code; .6)

18.10 places 1c in .6 beside the resolver. COORD's GO (2026-11-21) asked for this design first and P3's review before
any code. 1c retires the last section-6 pin, s6next (`cgoP3LayoutEntryPointer`, src/go2cs/cgoPhase3_test.go:356). It
stacks on 1a (`claude/h-cgo-sb2a`), because it reuses 1a's word protocol, and it is cut on the FOLDED 1a (stop line
[P3 N7]).

P3 reviewed the first text, `dd80470936` (COORD inbox `20261121T211936Z-P3.md`): APPROVE WITH CHANGES, with one MUST (an
arm), four SHOULDs and two NITs. COORD ruled all of them into this text before code. Each is marked where it lands:
[P3 M1], [P3 S2] to [P3 S5], [P3 N6] and [P3 N7]. P3 agreed with Q1 to Q3 as proposed. Their answers are at the end.

**The shape today, read at 1a's tip.**
- `p := &l.next` emits `l.of(_Ctype_struct_go2cs_probe_entry.Ꮡnext)`. Over a native receiver, `nativeFieldView`
  (src/core/golib/ж.cs:276) answers a `NativeBox<ж<U>>` at the field's GO offset. That box is correct: it names the C
  word.
- `*p` emits `.val`, a `ref ж<U>` (ж<T>.Value). Over that box, the reference floor (ж.NativeBox.cs:195) refuses by name:
  "a managed reference where Go stores words". That refusal is the s6next pin.
- So the box is right, and only the `ref`-returning read is wrong for a word. 18.4 finding 1 withdrew a new box kind,
  because `Value` can only hand out a ref to a shadow. 1c keeps that ruling: there is no new box kind. Instead, the load
  and the store become calls, as W1 and W2 did for `*unsafe.Pointer` (src/go2cs/unsafePointerWordDoor.go).

**1. Scope (converter).** `*x` (a load) and the single assignment `*x = v` (a store) take the door when:
- the package takes the cgo route (`cgoRouteActive`, set at cgoRoute.go:1148 and reset per package at
  packageStateOperations.go:52); and
- x's static type, alias-resolved, is `*P`, where `P` is `*U` and `U` is a named type that cmd/cgo declares:
  `classifyCgoName` (cgoDeclarations.go:112) answers a TYPE kind (`cgoStruct`, `cgoUnion`, `cgoEnum`, `cgoType`).
  - This is the same name test 1a's marker reads (cgoDeclarations.go:313), so "a C type" has one meaning across both
    seats.
  - net's alias `_C_struct_addrinfo = C.struct_addrinfo` (cgo_unix_cgo.go:61) resolves to it.
  - [Q3, P3] The `cgoType` kind puts `**C.char` on the door, so an argv walk is covered.
  - [Q3, P3] A DEFINED pointer typedef as the pointee (`*_Ctype_xptr`, where P is a named type and not `*U`) is NOT on
    the door. Its C# type is a pointer wrapper, which 1a also leaves out by name.
- These keep their emission:
  - a tuple assignment target (`*p, x = a, b`), as in W1. Over a native slot it reaches the floor, which is asserted
    ([P3 S3]; see "Not in 1c");
  - a ref-lowered parameter, as in W1: it is a C# `ref` to a managed slot and has no box ([P3 S2]; see "Not in 1c");
  - every other pointee;
  - every package off the cgo route. So the stdlib and the behavioral corpus move 0 lines.
- Unlike W1, there is NO provably-managed-slot exemption (W1's shapes (a) and (b)). The population is cgo packages
  only, and the door's managed arm IS `.Value` (Q2).
- The population on the targets:
  - s6next;
  - net's walk `*_C_ai_next(r)` (go1.24.13 net/cgo_unix.go:150 and :216);
  - `*_C_ai_addr(r)` (:153, :157, :223, :227), which yields a `**_C_struct_sockaddr`. The reinterpret after it is
    s6reinterp's shape, which is green.

**2. Emission.**

    *x        global::go.golib.GoCgoArgs.LoadPointer(x)
    *x = v    global::go.golib.GoCgoArgs.StorePointer(x, v);

The call is qualified exactly as the converter already spells `global::go.golib.GoCgoArgs.HelperPointer(...)`. A
selector through the load composes: `(*p).family` becomes `LoadPointer(p).family`, read by 1a's accessor on the box
the load mints.

**3. The load** (golib, `GoCgoArgs.LoadPointer<U>(ж<ж<U>> p)`).
- p is nil: Go's nil dereference panic.
- [P3 M1] p is a field view whose source is native (`INativeRooted.IsNativeRooted` on the FieldRefBox).
  - This is `&l.next` where the field has no known Go offset: `of` keeps the view there (ж.cs:276, "a root whose
    field offset cannot be known keeps its view").
  - It is refused by name, before any read, with 1a's RequireGoFieldOffset text ("field ... has no known Go offset").
  - P3 read that `.Value` would reach the layout floor anyway, through the view's source (ж.FieldRefBox.cs:147 into
    NativeBox<T>.Value). The door does not rely on that read, for two reasons:
    - 1a guards exactly this receiver on its own field path (RequireGoFieldOffset, ж.CgoFields.cs:77).
    - The layout floor refuses only a T whose mismatch golib can prove (`s_layoutMismatch`). An unknown offset is the
      case where golib may not know T's Go layout at all, and then `.Value` would return the CLR slot of a struct in
      C memory.
  - So the guard is unconditional, and E-2's row reads it red first.
- p is any other non-`NativeBox<ж<U>>` (a variable, a field view of a C struct Go allocated, an element): `p.Value`,
  exactly today's read.
- p is a `NativeBox<ж<U>>`: `GoPointerWords.Classify`, as 1a's `CgoPointerFieldSlot` does (ж.CgoFields.cs:88).
  - Unheld: refused with S-B1's UnheldSlotText, word for word (18.10 delta U).
  - CMemory or GoMemory: read the word. 0 is nil. A word that Go memory names is refused with
    `CgoFieldLoadFromCText` or `CgoFieldLoadOverGoText`, by kind. Any other word mints `new NativeBox<U>(word)`.
  - Each load mints a fresh box, as 1a's field read does. So `*p == *p` holds by address (NativeBox's Equals reads the
    native identity, ж.NativeBox.cs:337), never by reference.
- This is 1a's field read (ж.CgoFields.cs:104-116) over a slot the caller already holds, where 1a's names a field. The
  word core is factored out of 1a's private helpers, so the field accessor and the door are ONE protocol: one
  primitive, one behaviour, and no new text.

**4. The store** (`GoCgoArgs.StorePointer<U>(ж<ж<U>> p, ж<U> v)`).
- p is nil: Go's nil dereference panic.
- [P3 M1] A native-rooted field view is refused, as in 3, with nothing written.
- Any other managed p: `p.Value = v`, today's store.
- A native p: Classify first, and Unheld is refused. Then:
  - nil writes 0;
  - a value that is C memory alone writes its address. `GoPointerWords.IsCMemoryAlone` decides: a `NativeBox<U>`, or
    an element of a native-backed slice that has no anchor. This is 1a's store as folded with P3's SHOULD 1 on 1a;
  - anything else is refused with `CgoFieldStoreIntoCText` or `CgoFieldStoreOverGoText`, by kind.
- This is 1a's field store, on the same core.

**5. Not changed.**
- `&x.f`'s emission and its box (ж.cs:276).
- The reference floor (ж.NativeBox.cs:195). It still refuses every `.Value` over a native `ж<ж<U>>` that the door does
  not reach, so a scope mistake fails LOUD and is never a silent read. W1's header makes the same argument.
- go2cs-gen: there is no generator change.

**6. Public surface.** Two members on `GoCgoArgs`, a `go.golib` class that is `[EditorBrowsable(Never)]`
(src/core/golib/runtime/GoCgoArgs.cs:32) and is not listed. PublicSurface.golib.txt gains 0 lines (Q1).

**7. The mint census.** 1a's C-memory mint moves from `CgoPointerFieldRead` into the shared core, and the door mints
at the same site.
- The row `ж.CgoFields.cs|CgoPointerFieldRead` (interiorPointerMintCensus_test.go:128) is RE-KEYED to the core, with
  count 1. It is re-keyed, never retired.

**Not in 1c (named).** Each stays loud, never silent.
- darwin's resolver. cgo_unix_syscall.go:40 declares `_C_struct_addrinfo = unix.Addrinfo`, and :56 returns `&ai.Next`.
  That is a plain Go type in a package off the cgo route, so it is outside the predicate by construction. Its walk
  over libc's memory is the resolver increment's route, and 1c does not claim it.
- The out-parameter `res **_C_struct_addrinfo` handed to C (cgo_unix_cgo.go:83). It is an argument, so it belongs to
  the argument door (3.1). 1c is the load and the store in Go code.
- [P3 S2] The same idiom inside Go code: `func set(pp **C.struct_e, v *C.struct_e) { *pp = v }`, called as
  `set(&l.next, v)` with l in C memory.
  - If the converter ref-lowers `pp`, the call site builds a C# `ref` from the native box and reaches the floor.
  - If it does not, `*pp = v` in `set` takes the door, and the call works.
  - If it emits a `ref` to 1a's GET/SET property, that is 1a's stop line (a compile error), and it is posted before
    any workaround.
  - E-3's `s6nextset` reads which of the three happens, and the expected one is the floor.
- [P3 S3] A tuple target over a native slot, `*pp, n = v, 1`. It keeps `.val`, and the floor refuses it by name
  (E-2 and E-3's `s6nexttuple`).
- [P3 N6] A slice of `*C.T` over C memory, `unsafe.Slice(argv, n)`. It stays refused at the slice door, because its
  element holds a reference (slice.cs:329, unsafe.cs:1063).
- A `**T` whose T is a Go type, reinterpreted over C memory. It stays the reference floor's refusal.
- A Pinner-pinned Go pointer: 18.9's deviation 8b applies unchanged.

**Arms.** Each is red before its code: red first, and every gate made to fail once.
- E-1, converter (the offline cgo fixture):
  - `*p` and `*p = v`, for p a `**C.struct_x`, emit the two calls.
  - [P3 S4] The load in positions other than a statement. Each takes the door or is a named exclusion, as listed:
    - `&*pp` is not a load. It emits pp itself after its nil check (convUnaryExpr.go:516), with no `LoadPointer`.
    - `(*pp).family = 3` takes the door. The base is a load and the field store is 1a's, so the emission is
      `LoadPointer(pp).family = 3`. Only the outermost star of an assignment target is a store.
    - `f(*pp)`, where f's `*C.T` parameter is ref-lowered, takes the door for `*pp`. What the callee's `ref` then does
      with the C struct is the layout floor's, as for any C struct reached by ref (1a's scope), so it is a named
      exclusion.
    - `*pp` inside a closure takes the door.
    - `**C.char` takes the door ([Q3]). A defined pointer typedef `*_Ctype_xptr` keeps `.val`.
  - Controls that keep `.val`:
    - (a) `**T`, T a Go struct in the same cgo package;
    - (b) a hand-written `_Ctype_struct_x` in a package that does not import "C" (1a's D-3b near-miss);
    - (c) a tuple target;
    - (d) a `*unsafe.Pointer`, which keeps W1;
    - (e) a ref-lowered `**C.T` parameter. [P3 S2] It is run over a native receiver as E-3's `s6nextset`.
- E-2, GolibTests:
  - the load and the store over Classify's four receivers (18.9 arm 5): a bare C root, a minted root, a pinned-Go root,
    and a managed receiver;
  - a nil box and a nil word;
  - a word naming Go memory, refused on load with both texts;
  - a Go pointer, refused on store with both texts;
  - Unheld, through Reinterpret over a Go `slice<byte>`, refused on load and on store;
  - the managed round trip through the door, which must be identical to `.Value` (18.9 arm 8's twin);
  - [P3 M1] a native receiver, an accessor with no Go offset, then `of`, then LoadPointer and StorePointer: both refused
    by name, with all of the struct's bytes unwritten;
  - [P3 S3] the tuple target's emitted shape, `pp.Value = v` over a native `NativeBox<ж<U>>`: the floor's text, once;
  - [P3 S5] the chained arm across the two seats. A C struct field of type `**C.T` is read through 1a's accessor,
    which mints a `NativeBox<ж<U>>`. That box classifies as CMemory, and `*e.pp` loads and stores through the door.
- E-3, e2e (testdata/cgo/p3; read only on the hosted legs, since H has no C compiler):
  - s6next's pin is removed, and it prints `s6next 10 b` (18.1's table).
  - `s6nextstore` stores through `pp := &l.next`: nil, then the C pointer back. C reads each through
    `go2cs_probe_next_family`, and it prints `s6nextstore -1 10`.
  - `s6nextstorego` stores a Go pointer through `pp`. Go keeps it, and go2cs refuses it by name (csFails
    `cgoP3StoreGoIntoC`).
  - `s6nextwalk` is the resolver's own shape:
    `func entryNext(e *C.struct_go2cs_probe_entry) **C.struct_go2cs_probe_entry { return &e.next }`, then
    `for r := l; r != nil; r = *entryNext(r)`, counting the nodes and summing the families. It prints
    `s6nextwalk 2 12`.
  - [P3 S2] `s6nextset`: `set(&l.next, second)` through the out-parameter helper above. Go prints what C reads back.
    go2cs is expected to refuse with the floor's text (csFails `cgoP3LayoutEntryPointer`, a named deviation). The
    other two outcomes are scored as worded in "Not in 1c".
  - [P3 S3] `s6nexttuple`: `*pp, n = second, 1`. Go stores it, and go2cs refuses with the floor's text (csFails
    `cgoP3LayoutEntryPointer`, a named deviation).

**Predictions** (scored as worded, at the cut):
- D-1: corpus footprint 0 files on four targets, by the two-seeded reconvert.
- D-2: on 5 RIDs, s6next, `s6nextstore` and `s6nextwalk` print what Go prints, and s6next's pin is removed in the seat.
  - `s6nextstorego` is refused by name.
  - `s6nextset` and `s6nexttuple` are refused with the floor's text.
  - s6field, s6nextnil, s6nextdirect, s6name, s6reinterp, s6slice, s6store and s6storego are unchanged.
- D-5: go2cs-gen's output is byte-identical (the D-5 generated-file diff, base against cut).
- D-6: PublicSurface.golib.txt gains 0 lines. The mint census keeps its count, with one row re-keyed.
- Counts (GolibTests, GenTests, converter suite, CNR) are posted before the battery, derived at the cut. CNR's population
  is derived from the package-directory count.

**Stop lines.**
- Any emission change in a package off the cgo route, or for a pointee that `classifyCgoName` does not name.
- A go2cs-gen change becomes necessary.
- The door's managed arm differs from `.Value` (E-2's round trip).
- A new box kind, or any change to `&x.f`'s emission.
- PublicSurface.golib.txt gains a line.
- A `**C.T` dereference that the door does not reach reads anything but the floor's refusal (E-2's M1 and S3 rows,
  and E-3's `s6nextset` and `s6nexttuple`).
- [P3 N7] 1c's core is cut on the FOLDED 1a: 1a carrying P3's SHOULD 1 (the store admits C memory alone by
  `IsCMemoryAlone`). It is never cut on `01ecb22bb9`.
- `s6nextset` compiles to a `ref` to 1a's GET/SET property. That is 1a's own stop line, and it is posted before any
  workaround.

**Questions for P3, and P3's answers.**
- Q1, placement: two members on `GoCgoArgs`, or a new `go.golib` type? Proposed `GoCgoArgs`, because it is the cgo
  seam's hidden helper class, the converter already names it, and it adds no surface line.
  P3: agreed. The PublicSurface stop line guards it.
- Q2, no managed-slot exemption. Proposed none. The cost is that, in a cgo package, `*pp` for a local `pp := &x` (x a
  Go variable of type `*C.T`) emits the call, with identical behaviour on the managed arm.
  P3: agreed. The managed arm is `.Value`, and a cgo package is a small population.
- Q3, the predicate on U: only types cmd/cgo declares, or any `**T` on the cgo route? Proposed cmd/cgo's types only.
  P3: agreed, with the two sentences folded into item 1 (`**C.char` is in; a defined pointer typedef is out).

— H (2026-11-21)

**Record 2026-11-22: 18.11 as coded (dated corrections, the text above unchanged).** Code is cut on the folded 1a
(`ad632f3c2b`), per [P3 N7]. Each miss below was posted to COORD on its own, before its fix.
- **The managed arm is `ValueSlot`, not `.Value` (a correction to items 3 and 4).** golib's `.Value` reads a box that
  HOLDS a nil pointer as a nil box (StandardBox.IsNull is `m_isNull || HeldValueIsNull`) and panics. Go's `*pp`, where
  `*pp == nil`, reads nil. The converter's read of a managed `**T` slot today is `ValueSlot`, behind a nil check of the
  box itself (`pp.ValueSlot`; golib's `~p`). The door now does the same:
  - `IsNilPointer` (the box, never a box that holds nil) is Go's nil dereference;
  - then `p.ValueSlot` and `p.ValueSlot = v`.
  - E-2's red arm found it: the door coded as "`.Value`" panicked on the managed nil-holding row.
  - The case is also an e2e arm, `s6nilslot` (COORD's ask: output EQUAL to Go). It is in testdata/cgo/p3 and not in a
    behavioral project, because the door exists only on the cgo route and the behavioral corpus converts off it.
- **The store's value renders in pointer context.** With a nil context, a deref-aliased pointer parameter `v` emitted
  its value alias (`ref var v = ref Ꮡv.DerefOrNull()`). It now renders its box. E-1's DoorStore row asserts no alias.
- **A fixture lesson: an unexported function with no callers ref-lowers its pointer parameter.** It then has no box,
  so a pointer-door row inside one reads as the ref-lowered exclusion, never as the door.
  - E-1's first red arm missed on control (d) for this reason.
  - The rows are now EXPORTED functions, and only control (e) is unexported, with a caller.
  - Any future pointer-door fixture uses exported functions, or says why not.
- **E-1's typedef row** is a predicate unit test (TestCgoPointerDoorPredicateReadsTheTypeShape), because the Phase 2
  fixture has no pointer typedef. A hand-written `_Ctype_` typedef in a cgo package is not cmd/cgo's output.
- **[P3 M1], P3's open read, now RUN.** Through a naive `.Value`, the no-Go-offset view WAS refused, by the layout
  floor ("cannot read native memory as Entry: its CLR layout is not its Go layout ..."). The unconditional guard stays
  (COORD, 2026-11-21), and refuses first with 1a's text.
- **P3's two-hop note:** `&o.in.next` over a native receiver, with every offset known, is a NativeBox at the Go offset,
  and the door reads it (E-2 row ATwoHopFieldAddressOverANativeReceiverIsANativeBoxTheDoorReads).
- **The resolver's walk shape, read offline:** `r = *entryNext(r)` emits `r = GoCgoArgs.LoadPointer(entryNext(r))`.
  `entryNext`'s parameter does not ref-lower, and its body is `e.Ꮡnext`, a native box at the field's Go offset.
- **The mint census:** `ж.CgoFields.cs|CgoPointerFieldRead` is re-keyed to `ж.CgoFields.cs|ReadWord`, count 1, the
  core both seats read.
- **The battery at `68220bfa6f`: three misses, each posted on its own before its fix.**
  - golib's namespace guard (TheConvertersListIsGolibsNamespace) caught the new internal `CgoPointerWords` in
    namespace `go`. The core moved to its own file, src/core/golib/CgoPointerWords.cs, in namespace `go.golib`, so the
    converter's list is unchanged. The mint row follows it, to `CgoPointerWords.cs|ReadWord`.
  - The projitems guard caught the two new converter files, and they are now registered.
  - The globals-isolation guard caught the off-route test's leaked `currentPackageOutsideGoroot`. That test now isolates
    the converter's path globals, as the other module-converting tests do.
  - The filtered reads (`-run TestCgoPointerDoor`, `-run Cgo`) could not see any of the three. Only the unfiltered gate
    could.
- **A NIT from the review of 1a's fold:** an `IsCMemoryAlone` value with no native identity now gets its own named
  refusal in WriteWord, instead of the Go-pointer text. No row constructs it: a zero native box is nil, and
  `slice.OverNativeMemory` refuses a nil base. The comment at the arm says so.

### 18.12 Amendment 2026-11-24: seat 1b (S-B2b), the pinned class, shape (i) and B-10 (posted before code; .6)

COORD's GO NOW (2026-11-23) cuts 1b off master `97cca467a2`, with no P3 design read before code: P3 reviews the seat.
1b carries 18 items 2 to 5 (the pinned class, the out-parameter, the pressure arm, cmd/cgo's `_cgoIndex` and
`_cgoSlice` closures) and 18.7 finding 2's B-10. B-12 is already on master, from S-B1 (`UnsafePointerWordTests.B12_*`),
and 1b only keeps it green.

**What moved since 18.1, read at `97cca467a2`.** The doors are as 18.1 read them. C2's train-5 seat changed only the
two refusal TEXTS (`3dc05f72e1`: "is not supported yet, so go2cs refuses the call"), and both test anchors quote the new
text. The flip population is the one 18.1 counted: GolibTests 15 uses (Phase3 8, Derived 4, Review2 2, PinExtents 1)
and `cgoPhase3_test.go`'s 24 arms (22 flip, 2 stay as numbers), plus the three write-through arms.

**1. The pin target (golib).** The void door gains a third answer beside nil and C memory: a PINNED Go argument,
returned as (the root storage, a byte offset). The root is found ON THE BOX (M2), never from a number (M3):
- a managed box (standard, element or field reference): its `PinnableStorage`, an `Array`;
- a native box with a retained source: that source, recursively; a native box over a `PinnedBuffer`: the buffer's
  array;
- anything else (an order token, a native box that aliases one, a view over a reference-bearing struct, a number with
  no box): no root. A token or a reference-bearing root is refused with cgo's own text, "argument of cgo function has
  Go pointer to unpinned Go pointer"; a number stays refused with today's text.
- The offset is read under a `fixed` over the root, at the door: the pointer's number minus the root's base when the
  root is pinned at that number already (every `FromPinnedBox` mint, every derived pointer, every view whose
  reinterpret pinned the root), and the box's own slot address otherwise (a `FromBox` mint, whose number was transient).
  The offset must land inside the root's bytes, or the door refuses by name: a `fixed` that cannot reach the bytes it
  would hand over is stop line 3 below.

**2. The wrapper (converter).** A void parameter emits a `GoCgoPin` local from the door and a pattern-based `fixed`
over it, nested in parameter order, the innermost block holding the transition bracket and the call (13.5's order:
classify every argument first, then pin, then call). The pin's `GetPinnableReference` is the root's data reference,
or a null reference for nil and C memory, which pins nothing. The address handed to C is the pinned base plus the
offset, or the number. No `GCHandle`, no `PinnedBuffer`, no finalizer, no index entry (18's pin path).
- The typed pointer doors take the same pin for an ELEMENT of a Go array (`ElemRefBox`), today refused: C is handed the
  element's address in the pinned array, uncopied. A whole struct keeps its mirror.

**3. B-10 (18.6 finding 3, 18.7 finding 2, with P3's confirm (A)).** Before the pin, the door asks the word table
(`GoPointerWords`) for any live entry over the WHOLE root, and refuses one with cgocheck's text. The arms are 18.7's
seven, less the two helper doors (A) released: `VoidPointer`; `StructPointer` with an array field and with a `uintptr`
field; `CopyBytes`; `CopyString`. The helpers read Go memory as bytes (no lookup), and their arms assert the read.

**4. Shape (i), the out-parameter.** A `**C.T` parameter (`&db`, `char **`) crosses as one stack word: copied in by
`PointerMember` (nil 0, a C pointer its address, a Go pointer cgo's panic), handed to C by the word's address, and
copied back by `PointerMemberBack` (0 nil, the address it went in as keeps its box, anything else C memory). The slot
itself is a managed one: a native slot (`&l.next` over C memory) is handed over by its own address, uncopied.

**5. cmd/cgo's index and slice closures.** `_cgoIndex` and `_cgoSlice` check closures lower to the direct call, as the
address and variable forms already do, and both refusals in `cgoRoute.go` retire.

**6. The pressure arm (18.1 COORD (b)).** In GolibTests' CgoProbeTests: a slow C fill into a pinned Go buffer while
another thread runs `GC.Collect(2, Forced, true, compacting: true)`; Go reads C's write in every one of N rounds. An
unpinned twin must be shown to move, or the arm is Inconclusive.

**Predictions** (scored as worded, at the seat's tip):
- **P-1 corpus footprint, 0 files on all four targets.** Everything is behind `-cgo` plus golib's `GoCgoArgs`, and the
  standard library converts with `CGO_ENABLED=0`. The two-seeded comparison.
- **P-2 the flip set is 18.1's I-2 list, exactly.** GolibTests: the 18 hand-overs named there cross; the reference-
  bearing and token forms change to cgo's text; every number stays refused with today's text. End to end: the 22 arms
  print Go's output, `govoidnumber` and `govoidexpr` stay refused, and the three write-through arms print Go's output.
- **P-3 B-10:** five refusal arms with cgocheck's text, two helper arms that read the bytes.
- **P-4 shape (i):** strtol's `char **` arm reads Go's output, and `s6nextset` keeps its named deviation unless 1b's
  out-parameter reaches it (posted either way).
- **P-5 the pressure arm:** C's write read in every round, with the unpinned twin shown to move.
- **P-6 gates**, all unfiltered: GolibTests Release and Debug, GenTests, `go test ./...`, CNR, the footprint, the full
  behavioral run, `go2cs.slnx`, and hosted release-smoke, converter-cgo, cgo-e2e and golib-tests on three OSes.

**Stop lines.**
- Any NUMBER that crosses into Go memory.
- A token or a reference-bearing root that crosses.
- A `fixed` that cannot reach the bytes it hands over (refused by name instead).
- A door that hands C a root holding a word-table entry.
- A cgo wrapper made async, an iterator or a resumable twin (13.5); G's twin generator is read for this before READY.
- Any corpus line moved.

— H (2026-11-24)

#### 18.12.1 Record 2026-11-24: seat 1b as built, the readings so far, and the battery's predictions (before the battery)

**As built** (branch `claude/h-cgo-1b`, off `97cca467a2`):
- **The pinned door.** `GoCgoArgs.PinVoidPointer` returns a `GoCgoPin`, and the wrapper holds it with a pattern-based
  `fixed` around the call. The root is found ON THE BOX in one of three ways: `NativeDerivation.HeldSource` with
  `TryHeldTarget` (a box that holds its storage still), a native box's own `PinnedBuffer`, or a managed box's pinnable
  storage with its slot's offset read under a `fixed`. A token or a reference-bearing root is refused with cgo's text.
  A word-table entry over the root is refused too (B-10). Every number stays refused with today's text. `VoidPointer`
  is unchanged: it is the classify-only door an earlier wrapper calls.
- **The scalar door (item 2, narrowed).** `GoCgoArgs.ScalarPointer` pins an element of a Go array, or a typed view over Go
  storage, when the pointee is ONE C SCALAR (`GoCgoStructPointer.Pinned`). A scalar's layout is the same in C and in
  the CLR. A struct ELEMENT stays refused, because its CLR layout is not C's, and a whole struct keeps its mirror.
  The struct element is a named residual.
- **The helpers (item 3).** `GoStringOf`, `GoStringNOf` and `GoBytesOf` read Go memory under a `fixed`, as Go functions,
  with no word-table lookup (P3's confirm (A)). A read past the object's end, and a C string with no NUL inside the
  storage, are refused by name.
- **B-10 on the other doors.** The mirror asks the struct's own range of its storage. An inline array member's copy-in
  asks the array's own storage (`RequireNoPublishedWordIn`): go2cs keeps an array's elements apart from the struct.
  `C.CBytes` and `C.CString` ask the bytes they copy, before the allocation. Every lookup sits behind
  `GoPointerWords.AnyTable`.
- **Shape (i), the out-parameter (item 4).** `GoCgoArgs.OutParameter`, with `**C.T` classified as `ж<ж<T>>`. One stack
  word is copied in by `PointerMember` and back by `PointerMemberBack`. A slot in C memory crosses by its own address;
  a nil slot is NULL.
- **cmd/cgo's index and slice closures (item 5)** are recognised, and both refusals retire.
- **The pressure arm (item 6).** `probe.c` gains `go2cs_probe_slow_fill`, and the CgoProbeTests arm drives it.
- **13.5's stop line** (no async, iterator or resumable twin) holds by construction. G's `TwinGenerator` never twins a
  method that is extern, carries `LibraryImport`, holds a `fixed`, or sits in an unsafe class (`TwinGenerator.cs`, the
  native-wrapper rule). The bindings class is `static unsafe partial`.

**Readings so far, on H (GolibTests at Debug, filtered to cgo and the word table: 154/0/7):**
- **P-2, MISSED as worded: 2 of 36 GolibTests hand-overs (34 MET).** The rebuilt-from-number form keeps its resolved
  source, so its pinning and retaining mints cross. COORD ruled (a) (inbox H `20261124T021216Z`), and 18.1 carries the
  dated amendment. The nine-form set read as predicted. The 25 end-to-end flips (22 + 3) are written into
  `cgoP3Arms`, and the hosted cgo-e2e leg scores them.
- **One more flip, outside 18.1's list:** Phase 2's `AnElementOfAGoScalarArrayIsRefusedBeforeTheCall` is the scalar door's
  case. It becomes `AnElementOfAGoScalarArrayIsPinnedAndCWritesIt`: a real ucrtbase `frexp` writes the element in place,
  and its neighbours are untouched.
- **P-3, B-10: MET on the arms that can be built.** The void door, the mirror's `uintptr` field, an inline array member
  and `C.CBytes` each cross before the store and refuse after it, and `C.CBytes` over bytes before the word still
  crosses. The helpers read their bytes. `C.CString`'s lookup has no arm, since no test can publish a word into a
  string's storage: code-read only.
- **P-4, shape (i): MET on H with the real ucrtbase `strtol`.** The Go slot comes back pointing past the digits, a nil
  slot is NULL, a Go pointer in the slot is refused, and a slot in C memory is written there. The hosted `outstrtol` arm
  covers `&end` as Go writes it. `s6nextset` keeps its named deviation: its slot is a ref-lowered helper parameter,
  never a wrapper argument, so 1b's out-parameter does not reach it.
- **P-5, the pressure arm: NOT MEASURED on H** (no probe library). The hosted golib-tests legs require the probe.
- **Named residuals:** a struct element (above); an `endptr` that C points into PINNED GO memory comes back as a raw
  native box, not as the Go element it is.

**The battery's predictions** (H, at the tip that commits this record, base `97cca467a2`; all gates unfiltered):
- **B-1 corpus footprint: 0 files on all four targets** (the two-seeded `-stdlib`, converter A = base against B = tip).
  Every converter change is on the `-cgo` route, and the corpus converts with `CGO_ENABLED=0`.
- **B-2 GolibTests Release and Debug: empty failed sets.** The Debug-only skip set is the gated set plus the probe
  arms, by name.
- **B-3 GenTests: all pass.** go2cs-gen is untouched.
- **B-4 go test ./... ok; repoguard ok; `GO2CS_CGO_BUILD=1 -run TestCgo` ok** (the e2e program is hosted, so it is NOT MEASURED here).
- **B-5 CNR byte-identical.** No behavioral project is on the `-cgo` route.
- **B-6 full behavioral: the same PASS set as master.** golib changed, so the full suite runs.
- **B-7 `go2cs.slnx` builds**, 0 errors, since golib's API changed.

— H (2026-11-24)

**The battery's score** (H, run from a copy at `0656e4b99c`, base `97cca467a2`, every leg unfiltered, `EXIT 0`, tree
clean after each purge):
- **B-1 MET: 0 files, 0/0 lines on all four targets** (windows-amd64, linux-amd64, darwin-amd64, linux-arm64). Both arms
  rc 0, degraded 0, emitted-newer equal per target (109, 118, 123, 117).
- **B-2 MET on the failed sets, MISSED as worded on the skip clause.** Release 3161/0/54, Debug 3135/0/80, both failed
  sets empty, Release-only skips empty. The Debug-only set is 26 names, all JIT-gated tests already on master; no cgo
  test is in it. The probe arms are not in it: they read Inconclusive at BOTH configurations here (no probe library),
  so "plus the probe arms" named the wrong set. They are measured only on the hosted legs.
- **B-3 MET:** GenTests 814/0/0.
- **B-4 MET:** `go test ./...` 11 ok, 0 FAIL, 0 cached; repoguard ok; `GO2CS_CGO_BUILD=1 -run TestCgo` ok.
- **B-5 MET:** CNR NO REGRESSION, 1171 packages byte-identical, 12 platform-exclusive skipped by name, 0 NOT MEASURED.
- **B-6 MET:** full behavioral rc 0, 1017 compared, 0 failed, 8 NOT COMPARED. The enumeration is master's (CNR above).
- **B-7 MET:** `go2cs.slnx` Debug, 0 errors.

Not measured on H, owed to the hosted legs: the pressure arm (P-5), the probe arms, the 25 flipped e2e arms with
goindex, goindexfill, goslicefill and outstrtol, converter-cgo on linux and darwin, and release-smoke.

— H (2026-11-24, after the battery)

#### 18.12.2 Record 2026-11-24: the hosted rounds after the battery, and P3's review folded (r2 to r6)

**Hosted rounds (os-matrix, each on a new `claude/h-cgo-1b-rN` ref; no posted SHA replaced).**
- r3 to r4: 15 view arms refused, then 3. Fixed by the derivation walk (`ae2ef3b20f`: TryPinTarget walks
  `NativeDerivedFrom` to the first box holding a pin) and by `85cdef62df`: a reinterpret view of Go storage pins its ROOT
  (goindexfill and goslicefill had taken the one-scalar stack mirror, so C wrote 8 bytes into a 1-byte copy), and the
  helper and the walk take a window's anchor and an element's identity (goviewelemstring).
- r5 at `85cdef62df`: cgo-e2e green on linux, darwin and windows (69/69 TestCgoPhase3ProgramRunsAsGoDoes arms each),
  golib-tests green on linux and windows, release-smoke green. darwin golib-tests passed every test (3121/0/57) and was
  red only on its expected-skip gate (a sampling-profiler threshold test ran where it is listed as skipped); r4's darwin
  red was a different threshold test (the token-table bound). Posted to COORD as a FINDING, not a 1b defect.

**P3's review of `85cdef62df`: CHANGES (COORD inbox H 20261124T173809Z), folded at `958a5124d0`.** The walk, the offsets
and the pin lifetime read sound. Each item is folded red-first with a row:
1. MUST: OutParameter refuses an element of a Go array of C pointers by name (its one-word mirror is not the array C may
   read or fill); `&end` still crosses. Rows: GolibTests PinnedAnOutParameterOverAnArrayElementIsRefusedByName, e2e
   outstrtodelem (C# refuses with the text, Go prints).
2. ScalarPointer and TryHelperPin name an element by its identity (RootNumber), so `&h.name[0]` with `name [8]C.char`
   over a view crosses. Read on H against a converted program first: refused at the old number for the char * C fills
   and for C.GoString; after, ABCD lands at offset 8 and C.GoString reads it. Rows: e2e goviewcelemstring,
   goviewcelemfill (GolibTests cannot build the inline-array struct).
3. A reinterpret view whose root no door can pin, over a value wider than T, is refused by name instead of mirrored.
   Row: PinnedAWideReinterpretViewNoDoorCanPinIsRefusedNotMirrored.
4. Residual 1 (endptr) is CLOSED: inside the wrapper's `fixed`, after the call, each word C returned (an out-parameter's
   word; a pointer result other than the pointer it was handed) is checked against each pinned argument, and a word in
   that storage is refused by name (GoCgoArgs.RequireWordOutsidePin over GoCgoPin.Holds). The wrapper goldens move at
   exactly strtol (five targets) and gmtime_r (four). Rows: PinnedAWordCReturnsInsidePinnedGoMemoryIsRefusedByName, e2e
   outstrtodgo.
5. Residual 2 (C.CString B-10) has its row, PinnedB10_CStringAsksTheStringsOwnWindow: green at the old code as P3 read it,
   red with StringSize's check removed, restored byte-identical.
6. HoldsAnyEntry tests a word's overlap with the range, not its first byte. Row: PinnedB10_ARangeStartingInsideAWordHoldsIt.

**Readings at `958a5124d0`.** Hosted cgo-e2e r6 (45456514538 linux, 45456535113 darwin, 45456563570 windows): 73/73 arms
pass on each OS and on both darwin RIDs (the 69 plus the four new; the two refusal arms refuse with their text). On H:
GenTests 814/0/0; GolibTests Release 3168/0/54 and Debug 3142/0/80 (total 3222 = the r5 tip's 3217 + 5), failed sets
empty, Release-only skips empty, Debug-only skips the JIT-gated set already on master.
On H, the rest of the battery from a copy at `958a5124d0` (base `85cdef62df`), every leg unfiltered, tree clean after each
purge: converter `go test ./...` 11 ok, 0 FAIL, 0 cached; repoguard ok; `GO2CS_CGO_BUILD=1 -run TestCgo` ok; CNR NO
REGRESSION, 1171 packages byte-identical, 12 platform-exclusive skipped by name, 0 NOT MEASURED; `go2cs.slnx` Debug 0
errors.

Residuals still standing: the endptr class is closed for out-parameter words and pointer results; a void pointer C
returns into pinned Go memory still crosses as its number (a number names no box, M3). Handing C a whole Go array of C
pointers, pinned, is a later seat. The full behavioral suite is the train's (route #7, twin 4).

— H (2026-11-24)

#### 18.12.3 Record 2026-11-24: P3's read of the fold, CLEAR WITH NITS, and the NITs folded

P3 read the fold at `8d0c6c9de5`: CLEAR WITH NITS (COORD inbox H 20261124T204626Z). Each NIT is folded:
- **One past the end does not cross.** GoCgoPin.Holds now includes the storage's end, so C's write cursor `buf + n`
  is refused by name, not minted as a native box over the managed heap. The unit row's one-past-end control is now a
  refusal. It read red with the old comparison and green after.
- **Behavioral rows for the two refusals, each with its near-miss** (owner rule 2026-11-22):
  - resultstrtokgo: strtok over a Go buffer returns `&b[2]`. csFails, with the refusal's text.
  - resultstrcpyself: strcpy returns the destination it was handed, the identity return under a pin. Prints as Go does.
  - goviewwide: a char * over a 4-byte field of a struct holding a Go reference. csFails, with item 3's text.
  - goviewsame: the same struct's one-byte field keeps the one-char copy.
  The wide shape was read on H first, through a converted program. The converter emits
  `Ꮡs.Ꮡn.Reinterpret<int32, int8>()`, and the door refuses it with item 3's text. The same-size field takes the copy.
- **The two other unchecked return routes are on the BOARD** (2026-11-24, H), to be cured later:
  - a pointer member C writes into a mirrored struct;
  - an out-parameter word that is a stack mirror's address.
  18.12.2's residual paragraph covers only the void result. Read it with this block.
- **The refusal reads** "... and go2cs cannot represent a pointer into Go memory that C returned, so it refuses the
  result". Its old text ("C must not keep a Go pointer") was wrong for strtod's endptr, which C does not keep.

— H (2026-11-24)

## 19. Amendment 2026-11-24: increment 3.4, the harness and net's resolver (plan, posted before code; .6)

Section 7 is the harness's first text, and section 9's P8 its first prediction. This block reads what section 7 left
NOT READ, scores P8, and plans the increment. It stacks on seat 1b (`claude/h-cgo-1b-r7`, `8d0c6c9de5`), the converter
the readings below were taken with. Code waits for COORD's GO and a review.

### 19.1 Read before this plan (go1.26.11; linux/amd64 unless named)

- **R1. The mix is sound by file selection.** `go list -deps net` with cgo off and on: of net's 65 dependencies, 64
  select identical files. net itself swaps cgo_stub.go for cgo_unix.go and adds cgo_linux.go, cgo_resnew.go,
  cgo_socknew.go, cgo_unix_cgo.go and cgo_unix_cgo_res.go. runtime/cgo joins the closure, and the cgo route already drops
  that import (cgoRoute.go, the `"runtime/cgo"` filter). The test side adds one file, cgo_unix_test.go, which holds the
  seven tests. So section 7's "sound only if no package net imports changes its API with cgo on" is READ: none does.
- **R2. P8, scored as worded: MET, at the BASE.** The first hosted reading (45456887239, master's converter) refused one
  use: getaddrinfo's parameter 4, `res **C.struct_addrinfo`. Master has no out-parameter shape at all; seat 1b adds it.
  The candidate the prediction named (res_search) was wrong.
- **R3. P8', on 1b's tip: the conversion is clean.** Hosted 45482558650 (probe `629d1b9bff`, never landed): exit 0,
  0 refusals, 70 files written. getaddrinfo's `res` takes 1b's OutParameter. Bound: free, freeaddrinfo, gai_strerror,
  getaddrinfo, getnameinfo and res_search, each to libc.so.6 (malloc was past the probe's window, not missing).
- **R4. `-cgo` refuses `-tests`** (loaderEnvironment.go, checkCgoFlag: "the test drivers' loads stay at
  CGO_ENABLED=0"). That is the increment's main converter work.
- **R5. darwin needs no cgo for these seven.** cgo_unix_test.go builds under `!netgo && ((cgo && unix) || darwin)`.
  darwin's resolver, cgo_unix_syscall.go, reaches libSystem through syscall, and the cgo-off corpus already carries it
  (net/darwin/cgo_unix.cs, cgo_unix_syscall.cs). NOT READ: whether the seven pass there. net is banked on windows only.

### 19.2 The plan: three seats, each behind `-cgo` except the workflow's

- **4a. `-cgo` with `-tests`, for ONE package into a scratch root.** checkCgoFlag admits the pair when the conversion
  is a single package whose output root is not under the repository's `src/core`.
  - The production variant loads cgo-on, as `-cgo` does today.
  - The test variants load cgo-on too, so cgo_unix_test.go is selected.
  - Every other package stays the corpus's cgo-off build, referenced from the seeded root, as R1 shows is sound.
  - The bindings file, its library set and its link line are the production package's.
  - Red: a converter test whose fixture package has a cgo production file and a `_test.go` calling it. It is refused
    today with R4's text, and after the change it converts and builds the test host.
- **4b. The hosted leg.** A linux leg on os-matrix (the existing cgo-e2e job's toolchain):
  - Seed a scratch root from `src/core`.
  - Run `go2cs -cgo -tests -test-action all -test-filter '^TestCgoLookup' <GOROOT>/src/net <root>/core/net`.
  - Compare against `go test -json`.
  - Red: before 4a the conversion refuses (R4). After it, the seven compare.
- **4c. The first run-time reds, if any.** Each is read on the leg before any fix, and named. The candidates read so far:
  - res_search bound by its plain name to libc.so.6. Section 7's `-lresolv` line, and glibc's `__res_search`, are
    NOT READ.
  - The test host's build against a cgo-on net in a cgo-off closure.

The darwin reading (R5) needs no code: one sweep-shard dispatch of net on darwin with `-test-filter '^TestCgoLookup'`.

### 19.3 Predictions, scored as worded

- **Q1.** 4a, 4b and 4c move 0 corpus bytes on every target. CNR is byte-identical, and GolibTests rises by new rows only.
- **Q2.** After 4a, the linux leg converts net cgo-on with its tests, with 0 refusals (R3 plus the test file, which uses
  no C).
- **Q3.** The first build of that test host has at least one red not named in 19.2 (P8's successor: one new layer, one
  new red).
- **Q4.** Once it builds, the seven pass on linux against Go. Most likely red first: the two `...WithCancel` tests,
  where a cancelled context races a blocking C call (section 9's "most likely red").
- **Q5.** darwin's cgo-off run of the seven passes 7 of 7 with no code.

### 19.4 Stop lines

S-A to S-F (section 10) stand. Added:
- **S-G.** `-cgo -tests` needs any package OTHER than the one converted to load cgo-on: stop and post.
- **S-H.** A fix in 4c would change a corpus file or golib's cgo-off behaviour: stop, post the design (S-D's twin).

### 19.5 Questions for COORD

- **Q-F. Base.** 4a stacks on 1b, whose out-parameter R3 needs. Lean: cut on 1b's tip now, and rebase once train 11
  lands it.
- **Q-G. The darwin reading.** Lean: dispatch it now, since it needs no code.
- **Q-H. Review.** Lean: P3 reads 19.2 and 19.4 before code, as for 1b and 1c.

— H (2026-11-24)

### 19.6 Record 2026-11-24: seat 4a as built, and 4b's first readings (Q2 and Q3 scored)

**4a, as built** (`bb16815729`, then `533034372b`; COORD's GO, inbox H 20261124T204626Z). checkCgoFlag admits -tests.
Under -cgo, three loads of the converted package run cgo-on (packageTestLoaderEnv): the variants load and the sibling
closure (testConversion.go), and the marker-glyph scan. The oracle runs cgo-on too (cgoOracleEnv). linknameOperations'
load of another package's closure keeps the loader pin (S-G). checkCgoTestsOutput refuses an output under the core of a
go2cs root whose parent holds git metadata (S-D).
- Rows: the flag table's tests row now reads accepted.
- TestOnlyTheConvertedPackagesTestLoadsAndTheOracleTurnCgoOn and TestACgoTestsConversionNeverWritesTheRepositorysCorpus
  each read red with their arm neutered, restored byte-identical.
- TestACgoPackageConvertsWithItsTestsAndMatchesGo is hosted only (a C compiler). Its first run (45488939174) read -cgo
  -tests ACCEPTED, then refused the fixture's C.abs by name: gcc compiles abs as a builtin, so cmd/cgo's binary imports
  no abs symbol. That is the fixture, not the seat. With strtod (45491374366): status validated, 2/2 tests pass on both
  sides, matched.

**4b's first readings** (throwaway probe refs, never landed: a test that seeds a scratch root from src/core, src/gen,
the two props and gen's one outside input, then runs `-cgo -tests -test-action all -test-filter '^TestCgoLookup'` over
GOROOT's net, linux, cgo-e2e stage).
- Probe 1 (45493678255): the conversion of net cgo-on WITH its tests completed and reached the host's publish. The
  publish failed on the probe's own seed: go2cs-gen embeds src/go2cs/fieldAccessCollisions.txt. Seeded from then on.
- **Q2 MET**: net converts cgo-on with its tests, 0 refusals.
- **Q3 MET as worded**: the host's first build has one red that 19.2 does not name, six times. It is CS0102 and CS0111
  in core/net/linux/cgo_stub.cs: cgoAvailable, cgoLookupHost, cgoLookupPort, cgoLookupIP, cgoLookupCNAME, cgoLookupPTR.
  The seeded root holds the corpus's cgo-off selection, in which cgo_stub.go stands for the resolver. The cgo-on
  selection swaps it for cgo_unix.go (R1), and a single-package conversion writes only what it selects. So both files
  compile. This is corpus.md's MIXED-state class ("declarations MIGRATE while the stale other-selection file remains"),
  met in a scratch root by construction.
- **The cure proposed for 4c**, posted before code:
  - Under -cgo -tests, the converter removes from the OUTPUT the C# of each production file that the corpus's cgo-off
    selection holds and the cgo-on load deselected. The converter's own two loads decide that set (its selection, never
    a second tag engine), wherever the layout put the file (flat or a key folder).
  - Today the set is cgo_stub.go alone (R1).
  - A hand-owned file is never removed: one in that set is refused by name instead.
  - Corpus bytes 0: the output is a scratch root by checkCgoTestsOutput.
- Q4 and Q5 are not reached yet.

— H (2026-11-24)
