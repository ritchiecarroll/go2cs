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

## 17. Proposal 2026-11-13: a golib seat that closes the number residue of 16.9 (design only, no code)

**Status: PROPOSED.** COORD rules it together with the findings of the review of 3.1 (`80e8b3c7d8`). It is condition
3 of ruling C. It is its own seat, after 3.1 lands. Nothing here is built.

### 17.1 What is open

3.1's door tells a bare number that is Go memory from C memory by an EXACT lookup in the pin and token registry.
Two things get past it (16.9):

- **A number whose pin was never registered.** Two pin moments register nothing: the pinned reinterpret
  (`ElemRefBox.TryPinnedReinterpret`) and a native box built over a pinned buffer. Those are the four hand-overs the
  arm holds open.
- **An interior number.** A registered address plus an offset is not a key, for every kind of pin.

### 17.2 What was read in golib before this was written

- **Every number leaves a box through one exit:** the box-to-`uintptr` operator, and its `void *` twin. The bare
  mint takes the same route (`Pointer(box)` asks `StableAddress`, which is that operator). The exit has four arms.
  Three register what they hand out: the fixed array's data, the order token, the pinned value slot. The FIRST arm
  answers a native box's own address and registers nothing. Both open crossings leave through that arm.
- **The pinned reinterpret holds the WHOLE backing array still**, not the one element: it pins the array and mints a
  native box over the element's address, holding the pin and its source.
- **An entry is one weak reference, keyed by the exact address.** A read validates it: the box is alive, and its
  current token is the number or it is still pinned there. A native box's token is its address, so an entry for a
  pin-holding native box validates with no new arm. To be confirmed by the red, not assumed.
- **A token plus an offset is refused today,** by the tag (`IsTaggedToken` reads the tag alone). Interior numbers are
  open for pinned storage only.
- **Costs the registry already measured** (its own remarks): about 163 bytes per distinct registration and about 0
  for a repeat; 0.19 to 0.21 microseconds per fresh registration; the heaviest pin row on the roster (`crypto/tls`,
  47.5 thousand pins) holds about 500 resident entries.
- **`PinnedBuffer`** is constructed at 5 sites in golib and through `PinOnly` at 2, and is named by 3 hand-owned
  files outside golib (two of windows' syscall files, and `unsafe`). No converted package can name it.

### 17.3 The design, in three parts

**Part 1: register the two unregistered numbers.** Two placements close the same four hand-overs.

| | 1-EXIT (H's lean) | 1-PIN (condition 3 as worded) |
|---|---|---|
| Where | the exit's native arm: a native box that HOLDS A PIN (`PinsManagedStorage`) registers its address as it is handed out, and only where the exact read misses, so a live entry is never displaced | `TryPinnedReinterpret`, as it mints its box, and `PinnedBuffer`'s constructors |
| Registers | only a pin-holding native box whose number is actually taken | every pinned reinterpret and every pinned buffer, numbered or not |
| A `PinnedBuffer`'s address read directly by hand-owned code | not registered | registered, and a read then needs a new validation arm (a buffer is not a box) |

They differ in cost (17.5) and in the last row. No cgo package's own code can read a `PinnedBuffer`'s address, so
for the door of 3.1 the two close the same set.

**Part 2: the pinned range.** Each registered pin answers the byte range it holds still: a value slot is its
pointee's size; a fixed array's data is the array's bytes; a pinned reinterpret is the WHOLE backing array. Two ways
to hold it:

- **2-ENTRY (condition 3 as worded):** the entry carries the range's base and length. Every entry grows, for every
  registration the registry makes today.
- **2-BOX (H's lean):** the entry is unchanged, and the box answers its range when asked, from the pin it already
  holds (one member beside `IsPinnedAt`, with no state).

**Part 3: the interior question, asked at the door only.** `GoCgoArgs.IsKnownGoAddress` gains one step: on an exact
miss, is the number inside the range of a LIVE entry? A hit is valid only if that entry's own key still resolves,
so a dead box and a released pin answer MISS exactly as an exact read does. The number-to-pointer operator and
`Resolve` are NOT changed: no conversion resolves an interior number to a box. Phase 2's struct-pointer door shares
the predicate, so it gains the step too.

How the door finds the entry:

- **3-SCAN (H's lean):** walk the live entries. Nothing is added to a registration. A crossing that misses pays one
  walk: a handful of entries in an ordinary process, about 500 on the heaviest row read so far.
- **3-INDEX:** an index by page, kept at registration. A crossing is one lookup, and every registration pays for
  each page its range covers (17 writes for a 64 KiB buffer). It puts cost on the path every row takes, so it is a
  design of its own if the walk fails its bound (SA-5).

### 17.4 What it closes

One row per source of a number over Go memory. A source the review finds is one more row.

| # | Source of the number | Exact number | Interior number | Closed by |
|---|---|---|---|---|
| 1 | a pinned value slot (a box, an element, a field), through the exit | refused today | **closes** | parts 2 and 3 |
| 2 | a fixed array's pinned data, through the exit | refused today | **closes** | parts 2 and 3 |
| 3 | a wider view pinned over an element (the pinned reinterpret) | **closes** | **closes**, over the whole backing array | parts 1, 2 and 3 |
| 4 | a native box over a pinned managed buffer | **closes** | **closes** | parts 1, 2 and 3 |
| 5 | a pointee that holds references (an order token) | refused today | refused today, by the tag | nothing needed; an arm holds it |
| 6 | a pinned buffer's address read directly by hand-owned code | open under 1-EXIT, closes under 1-PIN | the same | part 1's placement; not reachable from a cgo package |
| 7 | a number whose pin has been RELEASED (section 13.4's residue) | NOT closed | NOT closed | 3.2's first item (13.4's rule 1): the entry reads MISS by design |
| 8 | a number minted from a reference with no pin (`FromRef`) | NOT closed | NOT closed | the converter (the BOARD row routed to P2). There is no pin, so there is no entry. A cgo package is refused it for the receiver's type (3.1) |
| 9 | reserved: a fifth source from the review | | | |

After this seat the disclosed gap narrows to rows 7 and 8, and `docs/Limitations.md` and the release notes are
reworded in the same seat to say exactly that.

### 17.5 The registry's cost per pinned reinterpret

| | At the pin moment | When its number is taken | At the door |
|---|---|---|---|
| Today | 0 | 0 | one exact lookup |
| 1-EXIT with 2-BOX and 3-SCAN | **0** | one test for a pin on every native conversion, and one exact read where there is one; for the FIRST number of a pin-holding native box, one entry (one allocation, about 163 bytes, about 0.2 microseconds); a repeat, 0 | on an exact miss, one walk of the live entries |
| 1-PIN with 2-ENTRY | **one entry per pinned reinterpret, numbered or not** (one allocation, about 179 bytes, about 0.2 microseconds), and the same for every pinned buffer constructed, the pin-only holds every box takes included | 0 | the same walk, comparing two fields instead of asking the box |

Every figure in this table is DERIVED from the registry's recorded costs. None is measured for this design.

### 17.6 The allocation-meter question, named before any code

The banked allocation asserts are denominated in BYTES. So the question is: **does any banked row's asserted body
run a path this seat makes allocate, or allocate more?**

- **2-ENTRY moves every existing registration** by the range's two words. A row whose asserted body registers a pin
  today pays more bytes there, with no change to what it does.
- **1-PIN adds a registration at every pin.** A row whose asserted body pins and never takes the number allocates
  where it allocates nothing today.
- **1-EXIT with 2-BOX** adds bytes only where an asserted body takes the number of a pin-holding native box for the
  first time.

**NOT MEASURED:** which banked rows, if any, do each of the three. It is measured first, before the seat is built,
by a registration census (off by default, after the registry's existing census) that counts new entries per arm on:
every banked row that holds an allocation assert, derived from the roster at that time; `crypto/tls`, the heaviest
pin row; and `os`, the registry's own neutrality row. H's lean is the combination with the smallest exposure. If the
census reads 0 on those rows for 1-PIN with 2-ENTRY as well, the wording of condition 3 costs nothing and H's lean
has no case.

### 17.7 Predictions, to be scored as worded

- **PA-1 (red first).** The two arms that hold the gap open are rewritten to hold it shut, and are red at the base.
  With the seat: all 36 hand-overs of 16.9's table are refused, 0 cross; a number built by arithmetic over a Go
  pointer is refused for every kind of pin; and beside each, a number inside a block from C's `malloc`, exact and
  interior, still crosses.
- **PA-2.** GolibTests' total rises by the new arms only. No existing row changes, the pointer-token conversion
  rows, the reinterpret-source retention rows and the registry's growth guard included.
- **PA-3 (the weakest).** The number-to-pointer operator answers differently in exactly ONE case: the number of a
  pin-holding native box, where no entry answered before, converted back to that box's own pointee type, is now that
  same box, and keeps its pin. Today it is a fresh native box with no pin. A number an entry already answers for
  keeps that entry. Every other case takes the arm it takes today. Not traced arm by arm: the
  operator has many, and this is read on the red before it is trusted.
- **PA-4.** 0 corpus bytes on four targets (golib only, no converter change), `check-no-regression` byte-identical,
  no behavioral golden moves.
- **PA-5.** Under H's lean no banked allocation assert moves, and the census of 17.6 reads 0 new entries inside an
  asserted body. Under the wording of condition 3: NOT PREDICTED, because the rows are not yet read.
- **PA-6.** The cost rows hold: `crypto/internal/nistec`'s wall time inside its recorded spread, and the `os` row
  with no direction between base and seat, each read twice at each tree, alone on the box.
- **PA-7.** The door's walk costs under 0.5 microseconds at 16 live entries and about 10 at 500. A derived figure
  (a weak read and one call per entry), stated so that the measurement can miss it.
- **PA-8.** Every arm of both end-to-end programs reads as it does at 3.1's tip, on linux, windows and darwin (both
  architectures), a loop of `C.CString` and `C.free` included.
- **Most likely red that is not the mechanism:** a reinterpret-source retention row, through PA-3's one case; or
  the growth guard, which counts entries.

### 17.8 Gates

- golib is touched: the full, unfiltered GolibTests against the base's results. `go2cs.slnx` stays R's routed
  finding, and is read at base and tip the same way.
- The registry is the reflect bridge's: the five largest banked reflect consumers by verdict count, derived from the
  roster at gate time, plus the cost canary, plus `crypto/tls` and `os`.
- The full `go test ./...`, `check-no-regression`, the full behavioral suite once at the tip, and the two-seeded
  corpus comparison on four targets (predicted 0: it is the proof that nothing but golib moved).
- Hosted: `cgo-e2e` on linux, windows and darwin, because a door every cgo call takes is changed on every OS, and the
  golib tests' C-memory controls where a C compiler exists.
- An adversarial review at the tip before landing. This is collector-safety work.

### 17.9 Stop lines

- **SA-1.** An existing GolibTests row, a behavioral golden or a corpus byte moves: stop and attribute.
- **SA-2.** A banked allocation assert moves: stop and post the row and the bytes. Nothing is re-baselined.
- **SA-3.** The seat needs instance state on `ж<T>`, a new box kind, or a go2cs-gen change: stop and post the design.
- **SA-4.** The number-to-pointer operator answers differently anywhere beyond PA-3's one case: stop.
- **SA-5.** The door's walk misses its bound by more than five times, or the design needs a lock or a write on the
  registration or resolution path: stop. The index is then its own design.
- **SA-6.** Any C-memory control is refused: stop. A range that refuses real C memory is worse than the gap.
- **SA-7.** The review's fifth source does not fit as a row of 17.4, and needs a mechanism of its own: stop and post
  it. It is not folded in.
- **S-F** stands: the seat yields to any .4 seat that needs H.

### 17.10 Questions for COORD

- **QA-1. Where part 1 registers.** Lean: the exit (1-EXIT). It deviates from the words of condition 3, which name
  the pin moments. It closes the same four hand-overs and costs nothing at a pin.
- **QA-2. Where the range lives.** Lean: the box answers it (2-BOX). It deviates from "an entry carries its length",
  and it leaves every existing registration's bytes as they are.
- **QA-3. The door's lookup.** Lean: the walk, with PA-7 as its bound and SA-5 behind it.
- **QA-4. The census first.** Lean: yes, as its own small step, read-only against banked rows, before either
  placement is chosen for good. It can make QA-1 and QA-2 moot.
- **QA-5. Order.** Lean: this seat after 3.1 lands and before 3.2's design step, so that 3.2 is designed over a door
  whose only open rows are 7 and 8.

— H (2026-11-13)
