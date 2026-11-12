# cgo Phase 3 scout: which real cgo package can be the proof of operation

> **State: SCOUT, for COORD's review (2026-11-12, lane H).** A point-in-time record, read-only: nothing was converted
> or built. It answers the owner's question for the queued cgo Phase 3 work
> ([`../PLAN-cgo-interop.md`](../PLAN-cgo-interop.md), Phase 3): which REAL cgo package can Phase 2 plus Phase 3 take end
> to end, validated against its own `go test`. Amended with dated blocks, never rewritten.

## The short answer

- **No package read here runs its own `go test` on Phase 2 plus Phase 3 as the plan words Phase 3 today.** Three
  shapes that every real package uses are outside both phases as written (section 3): an out-parameter that is a
  pointer to a C pointer, `defer C.free(...)`, and a `void *` parameter. Phase 3's scope has to take them in, or its
  exit gate proves helpers no real package can reach.
- **With those three in Phase 3, one candidate is reachable with no Phase 4 and no Phase 5: Go's own `net` cgo
  resolver.** It has no C in its preamble and no `//export`, and it has seven tests of its own.
- **`github.com/mattn/go-sqlite3` is not reachable by Phase 2 plus Phase 3, in either build.** Under the `libsqlite3`
  tag the SQLite API itself is all Phase 2 and Phase 3, but opening a database, preparing, binding and stepping all
  go through C functions the package's preamble defines (Phase 4), and nine `//export` callbacks are compiled into
  the package (Phase 5). Section 5 sizes the smallest slice of each that would make it run its own tests.
- **Recommended ladder:** `net`'s cgo resolver as Phase 3's proof; then go-sqlite3 under `libsqlite3` as the flagship,
  once a preamble-only Phase 4 slice and a minimum of Phase 5 exist.

## 1. Method, and what it cannot see

The lane box has no C compiler, so cmd/cgo cannot run there. This is a census of SOURCE TEXT, by a script kept
outside the repository (the readings below are its output, with corrections made by reading the files):

- a file is selected by its `//go:build` line and file-name suffix for linux/amd64 with `cgo` set (darwin where
  said), without test files;
- the preamble is the comment before `import "C"`. A function is **preamble-defined** when the preamble holds a body
  for it;
- each `C.name` in Go code is classed by spelling and use. A name is a type by spelling (`int`, `struct_x`, `x_t`) or
  by use (`*C.x`, `[]C.x`). A name called with `(` is a function. An all-capitals name that is never called is a
  constant.

Limits, each of which a hosted cmd/cgo run would close for a chosen target:

- a typedef used as a conversion (`C.sqlite3_int64(n)`) reads as a call. Those found by reading are corrected below;
- arguments are classed by text, so "Phase 3 traffic" counts the obvious spellings only (`&b[0]`,
  `unsafe.Pointer(&x)`, `C.CString(...)` inline);
- whether a constant is a real constant or a macro cmd/cgo must evaluate in C is not visible.

Sources read: Go's own packages at go1.26.10; `mattn/go-sqlite3` at `22b75c54a3` (2026-11-11; latest tag v1.14.53);
`jmhodges/levigo` at `ed89ec741d`; `coreos/pkg` at `bbd7aa9bf6`; `miekg/pkcs11` at `e0ca385070`.

## 2. The classes

| Class | What it is | Where it stands |
|---|---|---|
| Phase 2 | C types and constants; a call of a C function a shared library defines, passing scalars, one struct or one scalar by pointer, or a struct of integers and pointers by value; the two-result errno form with a scalar result | Landed through increment 5 |
| Phase 3 | `C.CString`, `C.GoString`, `C.GoStringN`, `C.GoBytes`, `C.CBytes`, `C.free`; Go memory handed to C without a copy (`&b[0]`) | Queued |
| Phase 4 | A function the preamble defines, or C source files in the package: compiled C | Not started |
| Phase 5 | `//export`: C calling Go | Not started |
| Later in Phase 2 | A C variable, a C function used as a value, a deferred or `go` call through cmd/cgo's pointer check, a function Go's cgo build does not import by its C name | Refused by name |

## 3. Three shapes every real package uses, which no phase names yet

Found in nearly every candidate, and refused today with "Phase 3's" or "Phase 2 later" as the owner:

1. **An out-parameter that is a pointer to a C pointer.** `sqlite3_open_v2(name, &db, ...)`, `sqlite3_prepare_v2(...,
   &stmt, &tail)`, `getaddrinfo(host, service, hints, &res)`, levigo's `char **errptr` on nearly every call. Phase 2
   crosses a pointer to ONE C scalar by a stack copy and a copy back, and refuses a pointee that is itself a pointer
   (`cgoStructWrappers.go`, "a pointer to something other than a C struct or one C scalar, which is Phase 3's").
   The plan's Phase 3 section does not mention it.
2. **`defer C.free(unsafe.Pointer(cs))`.** It is the idiom that follows every `C.CString`. cmd/cgo writes the deferred
   form of its pointer-check closure for it (the Phase 1 fixture's line 58 is exactly this), and increment 5 refuses
   the deferred form as "Phase 2 later". go-sqlite3 calls `C.free` at 23 sites, and all 3 of coreos/pkg/dlopen's
   are deferred.
3. **A `void *` parameter.** `free` takes one, and so do `sqlite3_file_control` and `sqlite3_bind_blob`. cmd/cgo
   always wraps a `void *` argument in its check closure, and Phase 2's wrapper refuses the parameter.

One more, found in `net`: **reading C memory through a pointer C returned.** `net` walks `getaddrinfo`'s list
(`r.ai_next`), reinterprets `ai_addr` as a `syscall.RawSockaddrInet4`, and takes `unsafe.Slice` over a `malloc`ed
buffer. Phase 2 already mints a native box for a pointer result, so this may hold as it is; it is the first thing a
Phase 3 target would measure.

**Consequence for Phase 3's plan:** its exit gate as written (a byte-identical round trip through a real C function, the
no-copy case under GC pressure) can pass while no real package converts. The plan and its predictions should name
these three shapes as in scope, each with its own red.

## 4. The candidates, ranked by "reachable with Phase 3 alone"

Counts are names, then call sites. "Phase 3 (+)" means Phase 3 with section 3's three shapes included.

| Rank | Package | Phase 2 | Phase 3 | Phase 4 | Phase 5 | Own tests | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | `net`, the cgo resolver (linux; go1.26.10) | 8 types, 18 constants, 5 functions at 5 sites; 2 of them errno forms with a scalar result | `GoString` 1; `malloc`/`free`; `getnameinfo` into a Go buffer (`&b[0]`); `getaddrinfo(..., &res)` | none | none | 7 (`cgo_unix_test.go`, `TestCgoLookup*`) | **Reachable with Phase 3 (+).** Go's own code, no preamble C |
| 2 | `internal/testpty` (linux) | 5 functions at 6 sites, 3 of them errno forms; 1 constant | `GoString` 1 | none | none | none of its own | Reachable today plus `GoString`, but nothing of its own to validate against |
| 3 | `coreos/pkg/dlopen` | `dlopen`, `dlsym`, `dlclose`, `dlerror` at 7 sites | `CString` 3, `GoString` 2, `free` 3 (deferred) | 1 (`my_strlen`, in the example file its test goes through) | none | 1 (`TestDlopen`) | The library half is Phase 3 (+). Its one test calls a preamble-defined C function that calls through a function pointer |
| 4 | `jmhodges/levigo` | 17 types, 59 functions at 67 sites | `CString` 6, `GoString` 9, `GoBytes` 3, `free` 6, `&key[0]` 2; `char **errptr` throughout | 1 (`levigo_leveldb_approximate_sizes`, 1 site) | none | 3 test functions | Phase 3 (+) plus one Phase 4 function. Needs `libleveldb` installed on every host |
| 5 | `os/user`, the cgo path (linux) | 8 types, 2 constants, `sysconf` | `GoString` 1 | 5 static helpers, 5 sites (`mygetpwuid_r` and kin): every lookup goes through one | none | yes | Needs the Phase 4 preamble slice first |
| 6 | `mattn/go-sqlite3`, `libsqlite3` tag | about 54 functions at 98 sites, 12 types, 68 constants | 51 helper sites, 26 `malloc`/`free`, 9 no-copy sites | 17 helpers at 29 sites, on the core path | 9 `//export`, always compiled | 103 test functions | Section 5 |
| 7 | `miekg/pkcs11` | types and constants | `GoBytes` 30, `CString` 7, `free` 29 | 74 preamble-defined functions at 75 sites | none | yes | Phase 4 dominates: the whole API is called through preamble C |
| 8 | `mattn/go-sqlite3`, default build | as rank 6 | as rank 6 | as rank 6, plus the bundled `sqlite3-binding.c` (271,135 lines) | 9 | 103 | Phase 4 in full. Section 5 |
| - | `plugin`, `runtime/cgo`, `crypto/internal/sysrand/internal/seccomp`, `internal/runtime/cgobench` | | | preamble C in each (or the package's own `.c` files) | 1 in cgobench | | Not targets: each is preamble C, or cgo's own runtime support |
| - | `cmd/cgo/internal/test` (Go's cgo conformance suite) | 54 types, 57 functions | 28 helper sites | 92 preamble-defined at 127 sites | 56 `//export` | the suite itself | A conformance target for after Phase 5, not a first proof |

Two corrections to the script's raw output, made by reading: the errno forms it lists under "later" for `net` and
`internal/testpty` all have scalar results, which Phase 2 binds since increment 4d; and `sqlite3_int64` and
`sqlite3_uint64` are typedef conversions, not calls (11 sites taken off go-sqlite3's function count).

**Why `net` ranks first.** It is the only candidate with its own tests and no compiled C: its five cgo files hold
`#include`s, `#define`s and `#cgo` flags, and no function body. Its C surface is small (`getaddrinfo`, `freeaddrinfo`, `gai_strerror`,
`getnameinfo`, `res_search`, `malloc`, `free`) and exercises each Phase 3 direction once: a Go string to C
(a NUL-terminated byte slice by `&b[0]`), a C string to Go (`GoString`), a Go buffer filled by C (`getnameinfo`),
and an out-parameter (`&res`). The seven `TestCgoLookup*` tests need only `localhost` to resolve.

**What `net` costs.** It is a standard-library package, and the corpus converts it with cgo off by ruling. Its cgo
flavour would be a separate conversion of one package under `-cgo`, and `-cgo` cannot be combined with `-tests`
today, so validating it against `go test` is harness work that Phase 3's plan has to size. Its link line has
`-lresolv`, a second library for the capture.

## 5. go-sqlite3, in both builds

The owner asked for this one explicitly. Read at `22b75c54a3`.

### (a) The default build: the bundled amalgamation

The package compiles `sqlite3-binding.c`, SQLite's whole source in one file of 271,135 lines, into itself. There is no
shared library: every `sqlite3_*` call resolves inside the package's own object. Phase 2's capture would refuse each
one as "resolved statically". This build is Phase 4 in full, the case the plan's Phase 4 names, and nothing before
Phase 4 reaches it.

### (b) The `libsqlite3` tag: the system's shared library

Under the tag `sqlite3-binding.c` compiles to nothing (it is wrapped in `#ifndef USE_LIBSQLITE3`), and
`sqlite3_libsqlite3.go` adds `-lsqlite3`. Ten cgo files are selected. The census:

| Class | Count | What |
|---|---|---|
| Phase 2: SQLite API calls | about 54 functions at 98 sites | `sqlite3_errmsg`, `sqlite3_value_type`, `sqlite3_column_decltype`, `sqlite3_close_v2`, `sqlite3_bind_int`, and so on |
| Phase 2: types and constants | 12 types at 177 sites; 68 constants at 128 sites | `sqlite3`, `sqlite3_stmt`, `sqlite3_context`, `SQLITE_OK`, ... |
| Phase 3: string and blob helpers | 51 sites | `GoString` 25, `CString` 18, `GoStringN` 5, `GoBytes` 3 |
| Phase 3: `malloc` and `free` | 26 sites | `free` 23 (deferred after `CString`), `malloc` 3 |
| Phase 3: Go memory with no copy | 9 sites | `_sqlite3_bind_text` and `_sqlite3_bind_blob` with `&b[0]`; `_sqlite3_result_blob`; `sqlite3_file_control(..., unsafe.Pointer(&cArg))`; `&cargs[0]` |
| Section 3's out-parameters | 8 call sites | `&db`; `&s, &tail`; `&rowid, &changes, &tail, &nParams`; `&errMsg` |
| Phase 4: preamble-defined functions | 19 defined (17 `static`); 17 named from Go at 29 sites | below |
| Phase 5: `//export` | 9, always compiled; each also passed to C as a function value (8 sites) | `callbackTrampoline`, `stepTrampoline`, `doneTrampoline`, `compareTrampoline`, the commit, rollback, update and pre-update hooks, `authorizerTrampoline` |

More `//export`s sit behind optional tags and are not selected: 13 for virtual tables, 2 for unlock-notify, 1 for
tracing.

**The Phase 4 helpers are the core path.** They are not conveniences at the edge:

- `_sqlite3_open_v2` is how a database is opened;
- `_sqlite3_prepare_v2_internal` is how a statement is prepared;
- `_sqlite3_step_internal`, `_sqlite3_step_row_internal`, `_sqlite3_step_columns`, `_sqlite3_bind_step_columns` and
  `_sqlite3_column_values` are how a row is stepped and read;
- `_sqlite3_bind_text` and `_sqlite3_bind_blob` are how a parameter is bound;
- `_sqlite3_exec_no_args`, `_sqlite3_reset_clear` and `_sqlite3_limit` cover exec and reset;
- `my_result_text`, `_sqlite3_result_text`, `_sqlite3_result_blob` and `_sqlite3_create_function` serve user
  functions.

**How far Phase 2 plus Phase 3 gets under (b), stated plainly: no test runs.** Phase 2 and Phase 3 cover the
package's types, its constants, about 54 direct API calls and all of its string traffic. But `sql.Open` followed by
any statement goes through the preamble's C at every step, and the package does not convert at all while a
`//export` refuses it.

**The smallest slice that would make (b) run its own tests** has two parts:

1. **A preamble-only Phase 4 slice.** Compile ONE C translation unit, the package's preamble with `static` removed
   from the 17 helpers Go names, into a small shared library beside the bindings, linked with `-lsqlite3`, and bind
   those 17 as Phase 2 externs of that library. Under (b) the package has no other C to compile. This is Phase 4's
   machinery (a native side-build with the pinned toolchain) at its smallest: one generated file, no package `.c`
   sources, no C parser beyond finding the function names.
2. **A minimum for the nine `//export`s.** Two ways, and the choice is a ruling:
   - Phase 5 proper for these nine. SQLite calls them back on the calling thread, during `sqlite3_step`, so the
     plan's foreign-thread gap does not arise here;
   - or a narrower rule: the package converts with each `//export` present but unreachable from C, and passing one
     to C as a function value (`C.callbackTrampoline`) is refused at that call. Open, exec and query would then run,
     and the tests that register a function, a collation or a hook would fail by name.

**Its tests.** 103 test functions in 24 files. Under the `libsqlite3` tag alone, the files that build hold 90 of
them, 51 in `sqlite3_test.go`. By a text search of the nine default-tag core files, 13 test functions call a registration or
hook API and would need the callbacks; the rest exercise open, exec, query, types and errors. That 13 is a lower
bound read from text, not a run.

**The library on each host, NOT MEASURED.** linux needs the development symlink (`libsqlite3.so`) for cmd/cgo's
link, where a stock runner may hold only `libsqlite3.so.0`. On darwin, go-sqlite3's own `#cgo` lines name Homebrew's
directories for the link, so the captured install name would be Homebrew's absolute path, and the absolute-name
trade of the Phase 2 design's section 11.12 applies. A stock windows runner has no `sqlite3.dll` with an import
library.

### A side reading: `modernc.org/sqlite`, SQLite with no C

One paragraph, as asked, and no conversion. `modernc.org/sqlite` (v1.62.0, 2026-11-07, read from the module proxy:
its `go.mod` only) is SQLite's C translated to Go by a C-to-Go compiler, with no cgo. Its `go.mod` asks for go 1.26.0
and depends on `modernc.org/libc`, a libc written in Go, plus `modernc.org/memory` and `golang.org/x/sys`. It would
be a different and strong proof: SQLite running on .NET through go2cs with no C toolchain anywhere. What follows is
from general knowledge of the project, NOT read here, and is the reason it is a separate question. The translated
source is one generated file per platform, hundreds of thousands of lines long, written in C's memory model: memory
from its own allocator, `uintptr` arithmetic throughout, and C function pointers carried as `uintptr` and called
through a reinterpreting cast. That stresses go2cs's `unsafe` handling and compile-size limits far more than any
package in the validated roster, and the function-pointer idiom has no measured go2cs conversion. It needs its own
scout, with the source downloaded and censused, before anyone sizes it.

## 6. Recommendation

1. **Phase 3's plan takes in section 3's three shapes**, each with a red: the pointer-to-pointer out-parameter,
   the deferred `C.free`, and the `void *` parameter.
2. **Phase 3's proof of operation is `net`'s cgo resolver**, validated by its seven `TestCgoLookup*` tests on linux.
   It needs no Phase 4 and no Phase 5. The plan sizes the harness work: a cgo-on conversion of one standard-library
   package, with its tests.
3. **go-sqlite3 under `libsqlite3` is the flagship that follows**, gated on the preamble-only Phase 4 slice (17
   functions) and a ruling on the nine `//export`s. Its default build waits for Phase 4 in full.
4. **`coreos/pkg/dlopen` is the smallest third-party proof** if one is wanted before go-sqlite3: four `libdl`
   functions and strings both ways, with one preamble function standing between it and its own test.
5. **`modernc.org/sqlite` gets its own scout** if the owner wants the no-C proof weighed.

**Not verified here:** every count above is from source text, not from cmd/cgo's output. For the chosen target, one
hosted `converter-cgo` leg can replace the reading with cmd/cgo's own declarations, as Phase 1's recognition test
does for its fixture.

## 7. Amendment 2026-11-12: the rulings on this scout

COORD ruled on the recommendation the day it was posted, and the owner ruled on the one item left to them. Section 6
stands as written; this block records what was decided. The Phase 3 plan carries these as its inputs.

1. **Phase 3 takes in all three shapes of section 3, each with a red:** the out-parameter that is a pointer to a C
   pointer (`&db`, `&stmt`, `&res`, `char **errptr`); `defer C.free(unsafe.Pointer(cs))`, so cmd/cgo's deferred check
   closure is accepted for that case in place of "Phase 2 later"; and `void*` parameters.
2. **Phase 3's exit gate is amended:** "a real package's own cgo tests pass", beside the byte-identical round trip and
   the no-copy case under GC pressure as written. A fixture round trip alone no longer closes the phase.
3. **The proof target is `net`'s cgo resolver on linux**, with its seven `TestCgoLookup*` tests. The harness work
   (`-cgo` with `-tests`, and a cgo-on conversion of one standard-library package) is its own increment of Phase 3
   with its own red. The corpus ruling stays cgo-off: this is a separate validation lane, never the default build.
4. **go-sqlite3 under `libsqlite3` is the flagship after Phase 3, and is not for the .4 release.** It is gated on
   two things:
   - the preamble-only Phase 4 slice: the 17 preamble helpers lose `static` and build into one side-built shared
     library linked `-lsqlite3`;
   - its nine `//export`s. **OWNER RULING:** Phase 5 proper for the same-thread callback case, where SQLite calls
     back on the calling thread during a step. The foreign-thread case is left for later. The narrower rule, refusing
     a callback only where it is passed to C, is not taken.
5. **`coreos/pkg/dlopen` is not needed** as a separate proof: `net`'s resolver is the better one.
6. **`modernc.org/sqlite` gets a separate scout later**, if the owner wants it.

**A second consumer of the no-copy view, for the Phase 3 design.** A separate proof project converts
`modernc.org/sqlite`, and found that a slice of a pointer-to-array cast over native memory copies, so a write through
it is lost. The converter warns at 20 such sites: libc 12, `modernc.org/sqlite` 7, `modernc.org/memory` 1. The
sharpest is the allocator's realloc (`memory.go:502`), a `copy` whose destination is such a slice. That memory is
native, so a real view over the address is possible, and it is the same primitive as Phase 3's no-copy `&b[0]` case
over pinned Go memory. The Phase 3 plan designs one view for both, or says why the two must differ. Reported by
COORD; the sites are not read here.

— H (2026-11-12)
