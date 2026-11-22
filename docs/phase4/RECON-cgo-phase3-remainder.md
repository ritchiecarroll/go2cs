# RECON — cgo Phase 3's remainder: Go memory handed to C without a copy (3.3), and a Go slice over C memory (3.4)

> **STATUS: DESIGN SCOUT, point-in-time record (P5, 2026-11-21, for COORD 080244Z). No code.** Read at master
> `ccca03abea` and against go1.26.11's sources. H reviews it for cgo fit and P3 reviews it as a design. It feeds H's
> .6 cgo plan; the plan, not this record, decides what is built. Amended by dated blocks, never rewritten.
>
> **Amended 2026-11-22 (section 7).** P3's design read is folded in there: 3.3-D's pin owner, 3.3-C(b) and Q4,
> A3's red reading, the order of R3 and Go's walk, the layout predicate, the runtime floor and the sizes.
>
> **Labels.** 3.3 and 3.4 are COORD's labels from the brief. In
> [`DESIGN-cgo-phase3-marshaling.md`](DESIGN-cgo-phase3-marshaling.md) the same numbers name other things: its 3.3 was
> the realloc view (section 5), which section 18 records as closed by the view doors, and its 3.4 is `net` and the
> harness (section 7). Question Q1 asks COORD to rule the names before either lands in that design.
>
> **How claims are marked.** READ means read at the tree or in Go's source, with the place named. NOT READ means
> inferred, and each NOT READ claim is the first thing its increment measures.

## 0. Summary

**3.3, Go memory handed to C without a copy.** H's 1b (in .6) gives C the address of an element of a Go array or slice,
or of a reinterpretation of one, pinned by a call-scoped `fixed`. Everything else that is Go memory still crosses as a
copy (a whole struct, by the Phase 2 mirror) or is refused. 3.3 separates two questions that `GoCgoArgs` answers as one
today:
- **Is it legal Go?** Go's own check answers, run by the converted runtime's `cgoCheckPointer`, which already honours
  `runtime.Pinner` and `GODEBUG=cgocheck`. The converter stops dropping cmd/cgo's check calls and routes them there.
- **Can the CLR do it?** `GoCgoArgs` decides how the memory crosses, on the box: PIN (no copy), IMAGE (a Go-layout copy
  where go2cs's layout is not Go's), or REFUSE. It refuses only what the CLR cannot do, and each refusal names that
  reason instead of a phase.
- Pin lifetime past the call comes from the Pinner: a long-lived CLR pin, taken the first time a Pinner-pinned referent
  crosses and released at its last `Unpin`.
- **Size: about 9.5 lane-days** in all; about 6 for a first seat without IMAGE (3.3-C(b)) and without Go pointers
  stored into C memory (3.3-E). It needs 1b landed.

**3.4, a Go slice over C memory.** Most of it is carried already: `unsafe.Slice` over C memory, the view doors' native
window, and door C's kept window all read and write C memory in place. Two shapes `net` uses are not carried, both on a
struct that lives in C memory: `(*[2]byte)(unsafe.Pointer(&sa.Port))` and `sa.Addr[:]`. A slice whose element type is a
C struct with pointer fields stays refused by name. **Size: about 3 lane-days.**

## 1. What this builds on (READ)

- 3.0 and 3.1 landed (batches 292 and 305). S-B1, the word doors for `unsafe.Pointer`, is built (design 18.8).
- H's 3.2 is split three ways (H's 18.9 and 18.10, relayed 2026-11-21 14:14Z). 1a, the accessor for a C struct's
  pointer field, is in .5. 1b (the pinned class, out-parameter shape (i), B-10, B-12) and 1c (the `**C.T` load and
  store, which `net`'s `getaddrinfo` walk needs) are in .6.
- **The mirror.** `GoCgoArgs.StructPointer` crosses a whole struct in managed memory as a stack copy with a copy back
  (`src/core/golib/runtime/GoCgoArgs.cs:42`, the `StandardBox` and `FieldRefBox` arms). C never sees Go memory.
- **The always-on check.** `PointerMember` refuses every Go pointer member with cgo's panic, pinned or not
  (`GoCgoArgs.cs:80-88`). `VoidPointer` refuses Go memory "until cgo Phase 3's increment 3.2" (`:123-127`).
- **Go's check exists, but no wrapper calls it.** `runtime/pinner_impl.cs:347` is Go's `cgoCheckPointer`: a level-1
  and level-2 walk in which a pinned word passes, turned off by `GODEBUG=cgocheck=0` (`:343`). The converter lowers
  cmd/cgo's check closures to the direct call and drops the check. The closure census over GOROOT's 29 cgo packages
  reads address 31, variable 50, index 22, slice 0, defer 9 and go 0 (hosted run 42973745743; the forms are
  `src/go2cs/cgoCheckClosureForms_test.go:32-37`).
- **The Pinner keeps a count and takes no CLR pin.** The pin bit is a count in a `ConditionalWeakTable` keyed by the
  referent (`pinner_impl.cs:290`), and the Pinner holds the referents. The file's header says why it takes no CLR pin:
  an address is minted only by the box's address-take, which already pins for the box's life. That holds for an
  address Go mints. It does not hold for an address the wrapper hands C through `fixed`, which 1b releases at return.
- **Pin extents.** The page-keyed index (`DESIGN-cgo-pin-extents.md`, built in .4) answers a bare number. A number
  stays refused (design 13.4, M3), and design 18 keeps it refused in 3.2.

## 2. Go's rules, and go2cs's answer to each

The rules are cmd/cgo's "Passing pointers" (go1.26.11 `src/cmd/cgo/doc.go:346-421`, READ).

| # | Go's rule | go2cs at master, plus 1b | 3.3 |
|:--|:--|:--|:--|
| G1 | A Go pointer passed to C is pinned implicitly for the call. | An element or reinterpretation: pinned by `fixed` (1b). A whole struct: copied, not pinned. | A whole value with Go's layout is pinned too (3.3-C(a)). |
| G2 | `runtime.Pinner` pins past the call. | The pin bit only. No CLR pin is taken. | A CLR pin at the first crossing, held to the last `Unpin` (3.3-D). |
| G3 | The pointee holds no Go pointer to unpinned memory. Extent: a field means the field's memory; an element means the whole backing array. | Every Go pointer member is refused, pinned or not, so Go proceeds where go2cs refuses. The variable form's whole-object check is never run, so Go panics where go2cs proceeds. | Go's walk decides (3.3-A). The extent is Go's, already ported in the walk ("check the slice rather than the pointer"). |
| G4 | C keeps a Go pointer only while it is pinned; after return, only under a Pinner. | The mirror's address is dead at return, and 1b's pin ends at return. | A Pinner-pinned referent crosses at its long-lived address (3.3-D). |
| G5 | Go code may store a Go pointer into C memory only when it points to pinned memory. | Refused: S-B1's W2(a), 1a's store refusal, and deviation 8(b). | Admitted for a Pinner-pinned referent (3.3-E), or kept as the named deviation (Q2). |
| G6 | `cgocheck=1` by default, `0` turns the checks off, and the `cgocheck2` experiment checks writes. | `GoCgoArgs` ignores `GODEBUG`. | The check is Go's, so `cgocheck=0` turns it off. What the CLR cannot do stays refused, under its own text (R3). |
| G7 | `runtime/cgo.Handle` passes Go values as integers. | An integer crosses as a number no Go value owns. NOT READ by a run. | One arm, expected green (A11). |

## 3. Increment 3.3: Go memory handed to C without a copy

### 3.3-A Go's check, run by Go's own code

- The converter lowers each `_cgoCheckPointer(x, y)` to a call of the runtime's check before the C call, instead of
  dropping it. The runtime gets a public forwarder, `runtime_package.CgoCheckPointer(any, any)`, marked
  `[EditorBrowsable(Never)]`, in `pinner_impl.cs` beside the internal body.
- All four argument forms map one to one: the address form's `0 == 0` is `true`, the variable form's `nil` is `nil`, and
  the index and slice forms pass the container. The `defer` form checks when the `defer` statement runs, as Go does,
  because Go evaluates the arguments there.
- What it closes:
  - The variable form (50 of the 81 census sites) gets Go's whole-object panic. That is Limitations' first divergence,
    where Go panics and go2cs proceeds.
  - A member pointing at Pinner-pinned memory passes, and so does every pointer under `cgocheck=0`. That is
    Limitations' second divergence, as far as the CLR allows (3.3-C(c)).
- **NOT READ:** whether a `-cgo` package's project already references `go.runtime`. The cut reads it first.

### 3.3-B The pin table moves into golib

`GoCgoArgs` is in golib and the pin table is in the runtime package, which golib cannot see. The table, a
`ConditionalWeakTable` with no runtime dependency, moves to `golib/runtime/GoPinTable.cs`. The Pinner's hand-owned
bodies call it, and runtime's own Pinner tests are unchanged. This is a golib member, so it needs route #7's consumer
gates.

### 3.3-C Three crossings, decided on the box

This extends design 13.2 (M1) and 13.3 (M2) from elements to whole values:

- **(a) PIN.** A whole value whose CLR layout is Go's and that holds no managed reference: a standard box, or a field
  reference over managed storage. The wrapper takes `fixed` over the value's own storage and hands C that address, with
  no copy. For a field reference that is the field's memory, which is Go's extent for a field. A `fixed` over a
  reference into an object pins that object as a pinned local; unlike `GCHandle.Alloc(Pinned)`, it does not require the
  object to be blittable. The mirror is no longer used for this class.
- **(b) IMAGE.** A value with no Go pointer whose CLR layout is not Go's, because it holds an `array<T>` (a reference
  in go2cs) or a struct that holds one. The wrapper writes an image of the value's Go size, at Go offsets, before the
  call and reads it back after: the mirror, made Go-shaped. The shape `net` needs is `syscall.RawSockaddrInet4` and
  `RawSockaddrInet6` (`Addr [4]byte` and `[16]byte`), handed to C as `*C.struct_sockaddr` (go1.26.11
  `net/cgo_sockold.go:23-29` and `cgo_socknew.go`). It needs Go-offset accessors on a Go struct, where 1a's accessor
  covers only a C struct. That is a go2cs-gen change, so it is stop line S-G, designed before code.
- **(c) REFUSE.** A pointee that holds a Go pointer to unpinned memory gets Go's panic from 3.3-A. Under `cgocheck=0` Go
  passes it, but a managed reference cannot be handed to C as an address, so go2cs refuses it by name (R3).

### 3.3-D Pin lifetime past the call: the Pinner

- When the classifier finds that `GoPinTable` reports the referent pinned, the call-scoped `fixed` is not enough,
  because C may keep the address after return (G4).
- The wrapper asks the pin record for a long-lived CLR pin. The pin is taken the first time the referent crosses, and
  released at the last `Unpin`:
  - for a box, `EnsureStableAddress` (`src/core/golib/ж.cs:774`) moves the value into pinned storage once;
  - for a backing array, `PinnedBuffer.PinOnly` (`ж.ElemRefBox.cs:458`, the pinned-reinterpret path).
  The wrapper hands C that stable address, which stays the same on every call.
- Runtime's own suite never crosses a pinned referent, so it pays nothing. The Pinner design refused that per-pin cost
  for runtime's tests, and this keeps the refusal.
- A Pinner-pinned value of class (b) is refused (R2): C may keep the address, and a copy cannot honour that.
- The Pinner's leak finalizer already unpins (`pinnerFinalizer`), so a leaked Pinner releases its CLR pins as well.

### 3.3-E A pinned Go pointer stored into C memory (separable)

- Go allows the store when the pointee is pinned (G5). Today S-B1's W2(a), 1a's store and deviation 8(b) refuse it.
- The design:
  - The store writes the stable address from 3.3-D.
  - A side table records the C address against the box, so W1 and 1a's load return the original box instead of
    refusing.
  - An entry is dropped when its word is overwritten or cleared through a door, or at `Unpin`.
- A word stored by C itself is never in the table and is read as today.
- If COORD prefers, this stays the named deviation and 3.3 ships without it (Q2).

### 3.3-F `GODEBUG`

The forwarder in 3.3-A reads `debug.cgocheck` itself, as the body does today. golib needs the level only to choose R3's
wording, so runtime's `parsedebugvars` (`goenvs_impl.cs`) also writes it to a golib static.

### 3.3-G Numbers

A number stays refused, Pinner or not (M3). Go lets C receive a number inside pinned memory. Admitting that would make
pin-extent row 1 binding first (the O(log L) index or the 16-page threshold). It is disclosed as a place where Go
proceeds and go2cs refuses (Q3).

### 3.3 Refusals, by name (proposed texts)

- **R1**, Go's own: `argument of cgo function has Go pointer to unpinned Go pointer`. It comes from 3.3-A, with the
  Go 1.26 kind that the converted `cgoFormatErr` composes.
- **R2**: `cgo: the <T> pointer handed to C.<f> is Go memory a runtime.Pinner holds, whose layout in go2cs is not Go's:
  C may keep the address, which a copy cannot honour, so go2cs refuses the call`.
- **R3**: `cgo: the <T> pointer handed to C.<f> points at Go memory that holds a managed reference, which go2cs cannot
  hand to C as an address, so it refuses the call (GODEBUG=cgocheck=0 does not change this)`.
- **R4**: `VoidPointer`'s text drops "until cgo Phase 3's increment 3.2" and names the reason it keeps: `a number inside
  Go memory, which go2cs never takes as C memory`.
- **R5**, only if 3.3-E is not taken: 1a's store refusal stays as written, and so does deviation 8(b).
- `PointerMember`'s always-on panic retires. Its rule moves to 3.3-A. It then copies in a pinned member's stable
  address, and refuses with R3 under `cgocheck=0` a member it cannot hand over.

### 3.3 Red-first arms

Each arm is run before code, at 1b's tip, and its red is read and named. An arm predicted red whose red reason has not
been read is marked NOT READ.

- **A1, alias pair.** C is handed `&s` and `&s.n`, writes through one and reads through the other. Go reads its own
  write. The mirror makes two copies, and the later copy-back wins, so the write is lost. Predicted red at master.
- **A2, identity.** `C.ident(&s)` returns its argument, and Go compares the result with `&s`. Go: equal. go2cs: the
  mirror's stack address comes back as a new native box, unequal and dangling. Predicted red at master.
- **A3, retention under a Pinner.** Pin a value, `C.keep(p)` stores the pointer, compacting collections are forced
  (`GC.Collect(2, Forced, blocking: true, compacting: true)` from another thread), then `C.writeKept(42)` writes and Go
  reads 42. Predicted red: no CLR pin is held after return.
  - Its unpinned control can let a C write into a moved object take the process down. It runs in a child process,
    the handling wall D (the sqlite-cs manifest) gave a host-fatal test. It is read as Inconclusive or a crash, and is
    never asserted green.
- **A4, the variable form.** `p := &x`, where `x` holds a Go pointer, and `C.f(p)`. Go panics; go2cs proceeds today.
- **A5, a Pinner-pinned member.** A struct whose pointer member points at Pinner-pinned memory. Go proceeds; go2cs
  refuses today.
- **A6, `GODEBUG=cgocheck=0`.** A member pointing into pointer-free Go memory: Go proceeds; go2cs refuses today. A
  member holding a managed reference: R3's text, asserted.
- **A7, IMAGE.** `net`'s sockaddr shape, `(*C.struct_sockaddr)(unsafe.Pointer(&sa))` with `sa` a
  `syscall.RawSockaddrInet4`, handed to a C function that prints the family, the port and the address bytes. NOT READ
  at 1b's tip; 1b may refuse it as a reference-bearing pointee.
- **A8, pressure.** Design 18's I-4, extended to class (a): C writes the pinned value while another thread forces
  compacting collections, and Go reads C's write in N of N rounds.
- **A9, a pinned store into C memory (3.3-E).** Store a pinned `&x` into a C struct's pointer field. C reads through it,
  and Go loads it back as the same box. Refused today.
- **A10, the control for numbers.** `uintptr(unsafe.Pointer(&pinned))` passed as `void *` is refused by name, with a
  Pinner and without.
- **A11, `cgo.Handle`.** A handle passed through C and back resolves to its value. Expected green at master.
- GolibTests: one arm per classifier result, each with a near-miss control (a struct one field away from the class).

### 3.3 Hosted legs

- os-matrix `cgo-e2e`, `converter-cgo` and `golib-tests` on linux-x64, win-x64, osx-arm64 and osx-x64.
- linux-arm64 for A3 and A8, which is B-12's leg in 1b.
- `release-smoke-cgo` on all five RIDs runs the Phase 3 program, so the new e2e arms run there as well.
- A3's child-process control needs its exit read per OS. Windows reports an access violation as `0xC0000005`, where
  the others report a signal.

### 3.3 Size

| Part | Lane-days |
|:--|--:|
| 3.3-A: the converter lowering of four forms, the forwarder, and the cgo goldens and closure-form test | 1.5 |
| 3.3-B: the pin table into golib | 0.5 |
| 3.3-C(a): PIN for a whole value | 1 |
| 3.3-C(b): IMAGE, including the S-G design | 2 |
| 3.3-D: Pinner lifetime | 1.5 |
| 3.3-E: a pinned store into C memory | 1.5 |
| 3.3-F: `GODEBUG` | 0.25 |
| Red arms, GolibTests, and the hosted reading | 1.25 |
| **Total** | **9.5** |

A first seat without (b) and E is about 6. The corpus footprint is predicted at 0 files on four targets, because all of
it sits behind `-cgo` plus golib, and the standard library converts with `CGO_ENABLED=0`. The two-seeded comparison
proves it, as in design 18's I-1.

## 4. Increment 3.4: a Go slice over C memory

### 4.1 What is carried today (READ)

| Shape | Route | What happens |
|:--|:--|:--|
| `unsafe.Slice((*T)(p), n)`, with `p` C memory | `unsafe.cs:2030`, then `slice<T>.OverNativeMemory` (`slice.cs:327`) | Reads and writes land in C memory. A reslice keeps the base. `append` within capacity writes in place. Beyond capacity it moves to managed memory, which is Go's behaviour (new Go memory). |
| `unsafe.Slice` of a pointer element | `builtin.NativeArrayPointer` (words with a shadow) | Words read and write in place. |
| `(*[N]T)(p)[lo:hi]` over C memory | view door V3 (`unsafe.cs:710-736`) | A native window. |
| `copy` into or out of that view | `CopyInto` and `CopyFrom` (`unsafe.cs:744`, `:777`) | The realloc shape. Also `crypto/internal/boring/rsa.go:166`, `copy((*[1 << 30]byte)(unsafe.Pointer(clabel))[:len(label)], label)`. |
| `(*[N]E)(p)` kept in a local | door C's `ToArray` (`unsafe.cs:920`; doors section 20.1) | An N-element native window, bounded by N. Proven on sqlite-cs's wall C (modernc's page cache over `libc.Xcalloc` memory; 9 sqlite tests). |
| `(*C.T)(unsafe.Pointer(&memSlice[0]))` over a native slice | design section 6, shape 4 | 1a's D-2 predicts s6slice stays green. |
| `unsafe.String`, `C.GoStringN`, `C.GoBytes` over C memory | copies | As Go does. |

Refused today, by name:
- an element type with a managed reference: `native-backed slice: element type X contains managed references and cannot
  alias native memory` (`slice.cs:330`), and an element whose C# size is not its Go size (`:336`);
- a window longer than a span (`slice.cs:352`);
- an index through a pointer to a door C box that the door did not bind, which stays on `.Value` and refuses (doors
  section 20.1).

### 4.2 The population in Go's own cgo packages (READ, a grep of go1.26.11)

The grep covers the packages that import `"C"` and their other files: `net` reaches C through `_C_` wrappers in files
that do not import it.

- `net/cgo_unix.go`:
  - `:326` `unsafe.Slice((*byte)(mem), resStateSize)` over `_C_malloc`, then `clear`;
  - `:327` that slice's element reinterpreted as `*_C_struct___res_state` and passed back to C;
  - `:372` `unsafe.Slice((*byte)(unsafe.Pointer(buf)), size)` handed to `dnsmessage.Parser`;
  - `:146` and `:150` `(*[2]byte)(unsafe.Pointer(&sa.Port))`, where `sa` reinterprets `*_C_ai_addr(r)`, which is C
    memory;
  - `:216` and `:220` `sa.Addr[:]` over the same C memory;
  - `:321` `unsafe.Sizeof(_C_struct___res_state{})` sizes a `malloc` that C then writes.
- `crypto/internal/boring/rsa.go:166`: the `copy` above, boringcrypto only.
- `plugin/plugin_dlopen.go:114`: `(*[2]unsafe.Pointer)(unsafe.Pointer(&sym))`, over Go memory, not C.
- `os/user`: `C.GoString` only.

No site in this population slices C memory as a C struct type with pointer fields.

### 4.3 The design, by gap

- **3.4-A, `(*[2]byte)(unsafe.Pointer(&sa.Port))` with `sa` in C memory.**
  - The address of a scalar field of a struct in native memory must classify as C memory, at the base plus the
    field's Go offset (`GoFieldOffsetOf`, the offset s6field reads). Then door C or the element door gives a native
    window.
  - This is the same assumption as 1a's item 2 (`Classify` over a native field-address box), so one GolibTests arm
    answers both.
  - Design 13.7 S1 predicts this shape red; that has not been measured. **About 0.75 lane-days.**
- **3.4-B, `sa.Addr[:]`, an array field of a struct in C memory.**
  - The field accessor over a native receiver refuses a field that holds a managed reference, and `array<byte>` is
    one. golib already maps an element in native memory to `builtin.NativeArrayPointer<T>(address, N)`
    (`array.cs:323-328`, the `&buf.PathBuffer[0]` case).
  - The design takes the open slice of the field through its ADDRESS: the doors section 18 field view, with a native
    receiver. The window is at the base plus the Go offset, has length N, and is anchored on the receiver.
  - Trigger, recommended: only where the receiver is a local bound once from `(*T)(unsafe.Pointer(x))`, door C's
    kept-local rule. This keeps the corpus footprint at the reinterpretations.
  - Not taken: dispatching every `p.f[:]` on the box at run time, which moves lines all over the corpus.
  - **About 1.5 lane-days**, including the two-seeded footprint on four targets.
- **3.4-C, `unsafe.Sizeof` of a C struct.**
  - It must equal C's `sizeof` on every target (design 13.7 S2). go/types folds it from cmd/cgo's padded declaration,
    so the converter emits Go's constant (NOT READ by a run).
  - An arm only. **0.25 lane-days.**
- **3.4-D, a slice of C structs with pointer fields over C memory.**
  - Refused by name today (`slice.cs:330`). There is no site in the population above.
  - Element boxes at the base plus i times C's size, through 1a's accessor, are the design if a target needs one.
    Deferred, **about 2 to 3 lane-days** when it is taken.
- **3.4-E, the limits stay Go's.** No window is checked against the size of the C allocation, and a window kept past
  `C.free` is a use after free on both sides (doors 20.2).

### 4.4 3.4 Red-first arms

- **B1.** A port read through `(*[2]byte)(unsafe.Pointer(&sa.Port))` over a `sockaddr_in` that C allocated and filled.
  Go prints the port. go2cs: a refusal is expected (NOT READ).
- **B2.** `copy(dst, sa.Addr[:])` over C memory, and a write through `sa.Addr[:]` that C reads back.
- **B3.** `unsafe.Sizeof` of three C structs against a C function that returns their `sizeof`, on each target.
- **B4, controls.**
  - Bytes from `unsafe.Slice` over `malloc` memory that Go writes and C reads.
  - `append` within capacity, which C sees.
  - `append` beyond capacity, which C does not see, as in Go.
- **B5, `net`'s `res_state` shape end to end.** `malloc`, `unsafe.Slice`, `clear`, the `&memSlice[0]` reinterpret, and
  a C call that writes the struct. This depends on 1a and 1b.

### 4.5 3.4 Hosted legs and size

- Hosted legs: `cgo-e2e` on the four RIDs plus linux-arm64 (struct offsets per target), and `golib-tests` on three OSes.
- Size: 3.4-A 0.75, 3.4-B 1.5, 3.4-C 0.25, and arms with the hosted reading 0.5. **Total about 3 lane-days.**
- 3.4 does not depend on 3.3. Its end-to-end proof is the `net` resolver (the DESIGN's own 3.4), which needs 1a, 1b,
  1c and 3.3-C(b).

## 5. Questions for COORD

- **Q1.** The names. COORD's 3.3 and 3.4 here, against the design record's 3.3 (closed) and 3.4 (`net` and the
  harness). Proposal: the design names these two 3.5 and 3.6 when they are folded in.
- **Q2.** 3.3-E: admit a Pinner-pinned Go pointer stored into C memory, or keep deviation 8(b) as named?
- **Q3.** A number inside Pinner-pinned memory: keep it refused and disclosed (this record's proposal), or rule that
  it crosses, which makes pin-extent row 1 binding first?
- **Q4.** 3.3-C(b) IMAGE needs Go-offset accessors on a converted Go struct (stop line S-G). Does it ride 1a's
  generator work, or get its own design?

## 6. What this record does not show

- No arm here has been run. Every "predicted red" is read from the code, not measured.
- The reasoning in 3.3-A, that Go's walk is already faithful, rests on `pinner_impl.cs` and runtime's banked Pinner
  tests (`TestPinnerCgoCheck*`). It has not been run against a `-cgo` program.
- Sizes are estimates from comparable seats (door C, the view doors, S-B1's footprint), not measurements.

## 7. Amendment, 2026-11-22: P3's design read folded (three MUSTs, two SHOULDs, one NIT)

P3 read this record at `baf94a1c4e` and returned APPROVE WITH CHANGES (mailbox, P3 to COORD, 2026-11-21 17:23Z). This
block folds every point in. The sections above stand as written; where this block corrects one, it names the lines.
Every golib site below is READ at master `1f8d62ddb3`, and its line numbers are that tree's. P3's answers to the
questions: Q1 agreed (3.5 and 3.6), Q2 keep deviation 8(b) for the first seat, Q3 keep the number refused.

### 7.1 MUST 1: who owns the long-lived pin (corrects 3.3-D, lines 118-122)

**What the record got wrong.** Line 120 says `EnsureStableAddress` "moves the value into pinned storage once", and
lines 118-119 say the pin is released at the last `Unpin`. Neither is the tree:
- `EnsureStableAddress` (`ж.cs:774-781`) pins IN PLACE: `m_pin = PinnedBuffer.PinOnly(storage)`, when
  `PinnableStorage` is not null. Nothing moves.
- `m_pin` (`ж.cs:117`) is never cleared. The pin lasts for the box's life and is released by the `PinnedBuffer`
  finalizer once the box is unreachable (`ж.cs:829-830`). Nothing releases it at `Unpin`.
- It is a pin on the BOX that took the address. For a standard box that box is the referent, and the Pinner holds
  it, so the pin does last while the Pinner holds it (the reason `pinner_impl.cs`'s header gives for taking no CLR
  pin). For a field reference or an element reference it is not: the Pinner holds the referent, which is the source
  allocation or the canonical backing (`pinner_impl.cs:138-139`, `goReferentOfBox`), and the pin sits on a per-access
  view box that nothing holds. When that view box is collected, its finalizer releases the pin while the Pinner still
  holds the allocation.
- `PinOnly` returns null when the CLR refuses the pin (`runtime/PinnedBuffer.cs:138-148`; `GCHandle` pins only a
  blittable object). A field reference's storage is its root's (`ж.FieldRefBox.cs:434-435`), so a reference-free field
  of a reference-bearing root reads `PointerStorage.Unpinnable` (`ж.FieldRefBox.cs:468-470`; the element reference has
  the same arm, `ж.ElemRefBox.cs:394-396`). For that shape no pin can be taken that outlives the call.

**The design, restated.**
- **The pin record owns the pin.** `PinRecord` (`pinner_impl.cs:292`, a class keyed by the referent, moving to
  golib's `GoPinTable` under 3.3-B) gains one field, `PinnedBuffer? Hold`.
- The first time the cgo wrapper hands C a pointer whose referent the table reports pinned, it sets
  `Hold = PinnedBuffer.PinOnly(view.PinnableStorage)` under the table's lock, if `Hold` is still null. Every view into
  the same allocation reaches the same storage object, so one hold covers all of them, and the address handed to C
  stays the same on every call.
- `GoPinTable.Unpin` disposes `Hold` (`PinnedBuffer.Dispose`, `runtime/PinnedBuffer.cs:82`) where the count reaches
  zero (`pinner_impl.cs:321`). That is the last `Unpin` across every Pinner that pinned the referent, which is Go's
  rule: the object stays pinned while any Pinner pins it.
- The leak finalizer reaches the same line through `unpinManaged`, so a leaked Pinner releases its holds as well.
- A standard box keeps its box-life pin from the address-take as well. That pin outlives `Unpin` until the box is
  collected. It costs a pinned slot for longer than Go would pin it and changes no value, and it is that way at
  master already.
- Runtime's own suite never crosses a pinned referent, so `Hold` stays null there and Pin and Unpin cost what they
  cost today.

**A new refusal, R6.** A Pinner-pinned pointee whose storage reads `PointerStorage.Unpinnable`, or for which `PinOnly`
returns null, gets:
`cgo: the <T> pointer handed to C.<f> is Go memory a runtime.Pinner holds, inside an allocation that holds managed
references, which go2cs cannot hold still past the call, so it refuses the call`.
Without a Pinner the same pointee is class (a) and crosses under the call-scoped `fixed`, which pins any object as a
pinned local (P3 agrees). Go allows the Pinner case (G4), so R6 is a place where Go proceeds and go2cs refuses, and it
is added to the disclosure list with R2.

**A new arm, A3b, the twin of A3 on that shape.** `s` holds a string and a reference-free `hdr`. `p.Pin(&s.hdr)`,
then `C.keep(&s.hdr)`, compacting collections forced, then `C.writeKept(42)`. Target: R6's text, asserted at
`C.keep`. At 1b's tip the field reference crosses by the mirror and C keeps a stack address, so the red reading runs in
a child process, as 7.3 says for A3.

### 7.2 MUST 2: IMAGE exists in golib (corrects 3.3-C(b), lines 105-110, the size table, and Q4)

**What the record got wrong.** 3.3-C(b) says IMAGE needs "Go-offset accessors on a Go struct ... a go2cs-gen change,
so it is stop line S-G". That was not read at the tree. golib already builds a Go-layout image of a converted Go struct
at run time, with no generator change:
- `NativeStructMarshal` (`NativeStructMarshal.cs`): `classify` (`:410`) decides a struct type's shape (scalars,
  fixed arrays of scalars and nested structs of the same are marshallable; a pointer, string, slice, map, channel,
  func, interface or `unsafe.Pointer` is refused by name), `MarshalledSizeOf` (`:397`) gives Go's size, `encode`
  (`:543`) writes the image at Go's offsets and `decode` (`:618`) reads it back into the box. This is the linux
  keystone's marshal and the darwin libc funnel's (`CallLibc`, `:125`). It is keyed by `Type` at run time.
- `NativeStructMarshal.ImagePlan<S>` (`GoImage.cs:189`): the same image for a struct type known at compile time,
  with no boxing per call. `GoImage<S>` uses it for `copy` over a reference-bearing struct's bytes.
- `NativeBox<T>.FieldRef` (`ж.NativeBox.cs:272-284`) already reads a Go struct's scalar field at its Go offset over
  native memory.

**What is actually missing.**
1. **Finding the source value.** `net` hands C `(*C.struct_sockaddr)(unsafe.Pointer(&sa))`. `sa`'s array field makes
   its box reference-bearing, so `unsafe.Pointer(&sa)` is its order token. `GoCgoArgs.StructPointer` refuses a
   native box minted from managed storage, naming the reinterpret view and the order token among its forms
   (`runtime/GoCgoArgs.cs:51-53`, through `IsGenuinelyNative`, `:302-303`). Which box kind the reinterpret reaches
   the wrapper as is NOT READ by a run; A7 reads it. The source is reachable: the native box
   carries `RetainedSource` (`ж.NativeBox.cs:321`) or `AliasesAnOrderToken` (`:298`), and an order token resolves
   through `ManagedPointerTokens.Resolve`, as the keystone resolves it. The new arm takes the source box, not the
   reinterpreted one.
2. **The image's size is the source's.** `RawSockaddrInet6` is 28 bytes (`SizeofSockaddrInet6 = 0x1c`, go1.26.11
   `syscall/ztypes_linux_amd64.go:310`, READ) and `struct sockaddr` declares 16. C reads `salen` bytes, so the image
   is built at the SOURCE's Go type and size (`MarshalledSizeOf(sourceType)`), never at `T`'s, and is decoded back
   into the source box after the call.
3. **A door from golib's cgo class into the marshal.** `classify`, `encode` and `decode` are private. The arm needs
   one internal member that encodes a box into a caller's buffer and decodes it back. Both classes are in golib, so
   this is a golib member, under route #7's consumer gates, not a generator change.

**Restated (b).** For a pointee with no Go pointer whose CLR layout is not Go's (the predicate is 7.5), the wrapper
reuses the keystone's plan: resolve the source box, size and encode its image at the source's Go layout, hand C the
image's address, decode back after the call. Both sockaddr shapes are within `classify`'s accepted set by their
declarations (`uint16`, `uint32` and fixed byte arrays; go1.26.11 `syscall/ztypes_linux_amd64.go:155-168`, READ).
Whether `classify` returns marshallable for them at the tree is NOT READ by a run; A7 measures it. Whether the
`Type`-keyed path is clean under Native AOT for the `release-smoke-cgo` legs is NOT READ either; A7's AOT leg answers
it.

**Re-sized.** 3.3-C(b) goes from 2 lane-days to **0.75** (the source resolution, the internal door, the size rule and
A7). **Q4 is withdrawn**: there is no stop line S-G for this increment. 3.4-A goes from 0.75 to **0.5**, because the
read side exists and the remaining question is the one 1a's item 2 already asks (whether `unsafe.Pointer(&sa.Port)`
over a native receiver yields base plus the Go offset); it is mostly its arm.

### 7.3 MUST 3: A3's red reading is host-unsafe too (corrects lines 173-178)

At 1b's tip a whole struct crosses by the mirror, so `C.keep` stores the mirror's stack address and `C.writeKept(42)`
writes through a dead stack slot of the test host. The red run can corrupt the host, not only its control. So:
- A3 and A3b run their red readings in a child process, the handling wall D gave a host-fatal test.
- Each red is read as "42 not observed, or the child crashed". Neither is asserted.
- Only the green reading, after 3.3-D, runs in-process, and asserts 42.
- The other arms do not keep an address past the call (A1, A2 and A8 write or compare within it; A9 is refused at
  1b's tip), so they stay in-process.

### 7.4 SHOULD 4: the order of R3 and Go's walk (adds to 3.3-A, lines 82-86)

`pinner_impl.cs:58-61` states two divergences of the managed walk: a string literal's bytes are a heap `byte[]`, and
a boxed scalar inside an interface is a heap copy, so both read "unpinned Go pointer" where Go reads "not a Go
pointer". Routed as written, a struct with a literal string field handed to C would panic with Go's R1 where Go
proceeds. The CLR cannot hand a string or interface field to C under any `cgocheck` level, so the outcome is a refusal
either way; the text must be R3's.

**The order, stated.** The wrapper classifies first. A pointee whose type holds a string or interface word at level 1
is refused with R3 before the walk runs. The walk runs for pointer words only.

**The residue, disclosed.** A string field whose bytes are heap memory Go would reject with R1 now reads R3: both are
refusals, and only the text differs. It goes into Limitations with one arm, **A12**: a struct with a literal string
field and one with a heap-built string field, each handed to C, each asserting R3's text.

### 7.5 SHOULD 5: the layout predicate, by name (adds to 3.3-C, lines 100 and 105)

"CLR layout is Go's" is the predicate `NativeBox<T>.FieldRef` already uses (`ж.NativeBox.cs:291`):
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` false and `GoReflect.ClrLayoutMismatch(typeof(T))` null. The
three results split on it and on the keystone's shape:
- **(a) PIN**: the predicate holds.
- **(b) IMAGE**: the predicate fails and `NativeStructMarshal`'s shape is marshallable.
- **(c) REFUSE**: the shape names a pointer field, or the type holds a string or interface word (R3, 7.4), or Go's
  walk panics (R1).

3.4 uses the same test, so a struct reaches the same class on both increments. GolibTests carries one arm per result,
each with a near-miss control one field away: (a) `{a, b uint32}` against (b) `{a uint32; b [4]byte}`; (b) against
(c) `{a uint32; b *uint32}`; (a) against a reference-free struct whose CLR layout is not Go's, if `ClrLayoutMismatch`
names one at the tree (NOT READ which types it names).

### 7.6 NIT 6: the runtime floor for C2's seat (adds to 3.3-A, line 77)

The public door already exists. `pinner_impl.cs:831` is `public static void GoCgoCheckPointer(any ptr, any arg)`, a
GolibTests seam over the same body. It is in the published `nuget-1.26.11.1` (READ: `git show
nuget-1.26.11.1:src/core/runtime/pinner_impl.cs`). Two choices for C2's seat:
- **Recommended:** the converter emits a call to `GoCgoCheckPointer`. A .5-built cgo package then runs against
  go.runtime 1.26.11.1 or later, and the runtime floor does not move. The seam becomes production API, and its
  comment says so.
- A new `[EditorBrowsable(Never)]` forwarder, as line 77 proposed: a .5-built cgo package then needs go.runtime .5
  or later, and C2's seat states that floor.

### 7.7 The size, re-totalled (supersedes the table at lines 206-216 and 4.5's total)

| Part | Lane-days |
|:--|--:|
| 3.3-A: the converter lowering of four forms, the call to the existing seam, the cgo goldens and closure-form test | 1.5 |
| 3.3-B: the pin table into golib | 0.5 |
| 3.3-C(a): PIN for a whole value | 1 |
| 3.3-C(b): IMAGE over the keystone's marshal | 0.75 |
| 3.3-D: the pin record's hold, R6 and A3b | 1.75 |
| 3.3-E: a pinned store into C memory | 1.5 |
| 3.3-F: `GODEBUG` | 0.25 |
| Red arms (A3 and A3b in a child process, A12), GolibTests, and the hosted reading | 1.5 |
| **Total** | **8.75** |

- A first seat without 3.3-E (P3's Q2 answer) is about **7.25**. IMAGE is now cheap enough to ride the first seat,
  and `net`'s sockaddr shape needs it.
- 3.4: 3.4-A 0.5, 3.4-B 1.5, 3.4-C 0.25, arms and the hosted reading 0.5. **About 2.75.**
- The corpus footprint prediction is unchanged: 0 files on four targets.

### 7.8 What this block does not show

- No arm has been run and nothing has been built. Every claim marked READ is a reading of source at `1f8d62ddb3` or of
  go1.26.11's sources.
- `classify`'s acceptance of the sockaddr shapes, the AOT cleanliness of the `Type`-keyed marshal, and which types
  `ClrLayoutMismatch` names are NOT READ by a run, as marked above.
- H's review of this record is still owed, and H's plan, not this record, decides what is built.
