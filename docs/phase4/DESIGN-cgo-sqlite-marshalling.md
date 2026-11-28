# DESIGN — cgo: the go-sqlite3 marshalling slice (a `void *` struct member; a typedef'd pointer result)

> **STATUS: DESIGN, posted before code (lane H; branch `claude/h-cgo-sqlite-slice`, cut from cgo Phase 4b's tip
> be3d1bb56c, so `TestSqlite3ReadingOnTheSideBuild` re-reads go-sqlite3 after each step).**
>
> **Ruling:** the owner, 2026-11-28 (COORD 005043Z): "Can try to fit slice marshalling within .5, best effort. No dates
> tied to it." The order is Phase 4b's MUSTs, then this slice, then Phase 5 once R's F1 is on master.

## 1. What the reading says, read against the code

go-sqlite3 v1.14.53's -cgo conversion (4b at d1239a7146, every target alike) is refused 38 times before any `//export`.
COORD scoped this slice to three classes, 22 sites. Read against the C declarations and the converter, they come to
**11 distinct functions in two shapes, and one class that belongs to Phase 5**.

| Refusal (sites) | What it is | Slice? |
|---|---|---|
| "parameter N is a pointer to something other than a C struct or one C scalar" (14 sites, 7 functions) | Every one is a **C function pointer**: `sqlite3_commit_hook`'s `int(*)(void*)`, `sqlite3_rollback_hook`, `sqlite3_update_hook`, `sqlite3_set_authorizer`, `sqlite3_exec`'s callback, `sqlite3_create_collation`'s `xCompare`, and `_sqlite3_create_function`'s `xFunc` (passed as `(*[0]byte)(xFunc)`). go-sqlite3 hands its `//export` trampolines through them. | **No: Phase 5's E2**, with the 16 "C function's address used as a value". |
| "_Ctype_struct___0.ptr is not a scalar" (6 sites, 3 functions) | One struct, go-sqlite3's own `sqlite3_go_col` (`int typ; sqlite3_int64 i64; double f64; const void *ptr; int n;`), crossing as an array in `_sqlite3_column_values`, `_sqlite3_step_values` and `_sqlite3_bind_step_columns`. | **Yes: shape A.** |
| "result is not a scalar" (2 sites, 1 function) | `sqlite3_db_filename` returns `sqlite3_filename`, a typedef of `const char *`, which cmd/cgo declares as an ALIAS of `*_Ctype_char`. | **Yes: shape B.** |

## 2. Shape A: a `void *` member of a mirrored struct

**Today.** A struct crossing to C is mirrored member by member (cgoStructWrappers.go, `mirrorOf` and `copyMember`). A
TYPED pointer member already crosses, with cgo's own check (§5.2, `GoCgoArgs.PointerMember`): nil is 0, a native pointer
is its address, and a pointer into Go memory panics with cgo's message before the call. Copied back, the address it went
in as is the original pointer, and any other is C memory, minted native. A `void *` member is `unsafe.Pointer` in
cmd/cgo's output. It misses the `*types.Pointer` arm and falls to `cgoScalarOf`, which refuses an UnsafePointer.

**Go's rules** (cmd/cgo's "Passing pointers"; runtime cgocheck=1):
- Go may pass C a pointer to Go memory only if the memory it points to holds no Go pointers (unpinned).
- C may not keep a Go pointer after the call.
- cgocheck=1 checks the pointed-to memory's pointer words at the call.

go-sqlite3 is written to that rule: `marshalFusedArgs` copies text, blobs and times into the statement's C `malloc`
scratch buffer "so the records contain no Go pointers". `ptr` holds only C addresses, whether Go writes them
(`a.ptr = unsafe.Add(s.argBuf, off)`) or C does (`col->ptr = sqlite3_column_text(...)`).

**The cure.** An `unsafe.Pointer` member takes the pointer-member path with the same semantics, so the layout is one
address word and the check is the one cgo makes:
- nil is 0, and a native pointer is its address.
- A pointer into Go memory panics with cgo's message (cgocheck=1's). A runtime.Pinner-held one is refused too, which
  is the named limit of docs/Limitations.md (a void * member never carries Go memory here); this line said "a pinned
  one crosses (cgo S6)" until P3's NIT 3 on the slice's read (2026-11-28) corrected it to the code.
- Copied back, an address C left alone keeps the original pointer's identity, and a new one is C memory, minted native.
  The Go side then reads it with `unsafe.Slice((*byte)(col.ptr), col.n)`, as go-sqlite3 does.

**The array.** The wrappers take `*C.sqlite3_go_col` for an array of `ncol` records:
- from Go memory (`&cargs[0]`, a `[]C.sqlite3_go_col`), or
- from C memory.
The struct image door (DESIGN-cgo-phase3-marshaling 19.7, 19.9) images a view's ROOT, the slice's backing, so C sees
every element, and copies it back. A row reads that with ncol > 1 in both memories, since a one-record copy would be
silently wrong.

*Correction (2026-11-28; COORD's ruling on H's finding, inbox H 20261128T050225Z).* The paragraph above is wrong on
both counts, and the hosted cgo-e2e at adcad2f879 showed it on every OS:
- From Go memory, `&cargs[0]` is an element of a Go slice of structs. GoCgoArgs.StructPointer refuses that by design:
  "an element of a Go array or slice: cgo lets C touch the whole array, which one struct's mirror cannot hold
  (Phase 3)". The struct image door images only a reinterpret view (a native box over an order token), never an
  element box.
- From C memory, go-sqlite3 reads `colvals` as `(*[(math.MaxInt32-1)/unsafe.Sizeof(C.sqlite3_go_col{})]C.sqlite3_go_col)(unsafe.Pointer(rc.colvals))[:n:n]`.
  The whole-array box over native memory is refused ("cannot view native memory as array<...>",
  DESIGN-native-array-view), and door B (DESIGN-array-pointer-doors §16) refuses by name an element that holds a
  reference, which this record's `unsafe.Pointer` member is.

Neither is the `void *` member's crossing, which works for one record (arm slicecolgoptr, and the C-writes row). The
slice seats at its conversion goal. Arms slicecolsgo and slicecolsc are KNOWN REFUSALS, asserting go2cs's refusal text
by name, with go run's output kept in the arm's comment. The two doors are BOARD rows of 2026-11-28: door 1 (H, next
seat) and door 2 (P5). The go-sqlite3 reading stays at 15 ways, all E2, until they land.

## 3. Shape B: a typedef'd pointer result (and parameter)

`resultType.(*types.Pointer)` (cgoStructWrappers.go, the result read) sees only a pointer type literal. cmd/cgo
declares a C pointer typedef as a DEFINED type over the pointer (`type _Ctype_sqlite3_filename *_Ctype_char`; out.go
writes `=` only where the printed type is itself a `_Ctype_` name, or an enum or union), so `sqlite3_filename` reaches
the scalar wrapper and is refused. `param()` reads a parameter with the same `t.(*types.Pointer)`, so a typedef'd
pointer PARAMETER is refused as well (go-sqlite3: `sqlite3_filename` is also the parameter of `sqlite3_uri_parameter`
and friends, which the package does not call today).

**The cure.** Read the pointer from `Underlying()` at both reads. A typedef'd pointer is then the pointer it names, with
no new behaviour: go2cs renders the defined type as a wrapper class with implicit conversions to and from `ж<T>` both
ways, so the wrapper's `ж<_Ctype_char>` result and parameter meet the Go code's named type without a cast.

*Correction (2026-11-27).* The first cure applied `types.Unalias`, on the reading that cmd/cgo declares the typedef as
an alias; the offline row's hand-written shape carried that reading and passed, and the hosted cgo-e2e with real
cmd/cgo refused `C.go2cs_probe_fname` on every OS. The row now carries cmd/cgo's real shape.

## 4. Red first, then the gates

1. **Reds**, one tests-only commit, each red today:
   - **A, offline golden**: a fixture struct with a `const void *ptr` member crossing as a pointer parameter emits a
     mirror whose member is copied in through `PointerMember` and back through the pointer copy-back.
   - **A, e2e arms** (p3): an array of three such records written by C (`ptr` set to C memory, read back through
     `unsafe.Slice`) from Go memory, and the same from C memory. Plus an arm where Go puts a Go pointer in `ptr`, which
     panics with cgo's message, as go's binary does under cgocheck=1.
   - **B, offline golden**: a function returning a typedef of `const char *` emits the pointer-result wrapper. A
     function taking one emits the pointer parameter.
   - **B, e2e arm**: a typedef'd `const char *` result read back with `C.GoString`, equal to go run.
2. **Gates**, Phase 4's: unfiltered go test and repoguard; hosted converter-cgo and cgo-e2e ×3, the default compilers
   and the pins.
3. **The reading after each step** (COORD's ask): TestSqlite3ReadingOnTheSideBuild on the hosted legs.
   - Predicted: 38 → 32 after A (the 6 struct refusals go) → 30 after B (the 2 result refusals go).
   - What stays is the 30 C-function-pointer refusals, Phase 5's E2 (16 used as a value, 14 as parameters).
   - A count other than 30 is a finding: a site the reading did not show, or a cure that does not reach every site.

## 5. Open questions

- **OQ-1:** the 14 function-pointer parameter refusals. I read them as Phase 5's E2 (exported callbacks), not this
  slice's, since no C-function-pointer value can cross until a Go function can be one. Agree?
- **OQ-2:** a `void *` member that C writes with a pointer into memory C does not own (a pointer into a Go value C was
  handed). The copy-back mints any new address as C memory, as for a typed pointer member today. Same rule, no new
  case. Confirm that is acceptable for `void *`, where no element type bounds what the address may point at.

## 6. Rulings, 2026-11-28 (COORD 031803Z)

- **OQ-1: agreed.** The 14 C-function-pointer parameter sites (7 functions: the callback slots of
  `sqlite3_commit_hook`, `sqlite3_rollback_hook`, `sqlite3_update_hook`, `sqlite3_set_authorizer`, `sqlite3_exec`,
  `sqlite3_create_collation`, and `_sqlite3_create_function`'s `xFunc`) belong to Phase 5's E2, with the 16 "C
  function's address used as a value". This slice does not touch them. BOARD entry of 2026-11-28.
- **OQ-2: acceptable.** A `void *` member's copy-back mints any address C wrote there as C memory, the rule typed
  pointer members follow. It is Go's own contract. cmd/cgo's "Passing pointers": "C code may not keep a copy of a Go
  pointer after the call returns", and Go memory passed to C "must not contain any Go pointers". So after the call,
  anything C has written into a pointer word of memory Go passed it is a pointer C owns, never one into Go memory.
- **Shapes A and B: GO**, reds first, with one more e2e arm: **C writes a C pointer into `ptr`** (into memory C
  allocated) and Go reads the bytes back through it, equal to go run.
