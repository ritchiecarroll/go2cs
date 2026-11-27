# DESIGN — cgo Phase 4, the preamble-only slice: a side-built library for the C a preamble defines

> **STATUS: DESIGN, posted before code (lane H, 2026-11-27; branch `claude/h-cgo-p4-sidebuild`, cut from the S6 seat
> `claude/h-cgo-s6-r12` at `8707e4d459`, which carries master `317b273e58`).** It designs the slice that
> [`RECON-cgo-phase3-target.md`](RECON-cgo-phase3-target.md) §7 ruling 4 names, inside
> [`PLAN-cgo-interop.md`](../PLAN-cgo-interop.md) Phase 4. **Placement:** the owner's 2026-11-20 placement puts Phases
> 4 and 5 after the Go 1.27 hop, and the 2026-11-21 pull into .5 named Phase 3 items only. So this note and its red test
> are written now, and the seat joins no .5 train without an owner ruling (COORD, 2026-11-27). Phase 5 (`//export`)
> waits for a ruling of its own.

## 1. What happens today

A C function that Go calls as `C.f` is bound by name to the shared library that exports it (Phase 2). For each such
symbol, the route links a probe against the package's `#cgo LDFLAGS` libraries (`cgoCapture` in `cgoRoute.go`). A
symbol that no library exports is refused at its use:

> C.f: no shared library the package links exports it (it resolved statically, or nowhere), and go2cs binds only a
> shared library's export; a function the preamble defines is compiled C, and go2cs does not compile C yet

`TestTheCgoRouteConvertsTheFixtureOnARealHost`'s subtest "a function the preamble defines is refused as compiled C"
holds that text over `static int add(int a, int b)`.

The plan names two gaps. A `static` helper exports no symbol at all. And much real-world cgo links C statically
(go-sqlite3's `sqlite3.c`). This slice takes the first gap only: the **preamble** is the only C compiled. A package
with C, C++, Objective-C or assembly source files of its own stays refused (§2.7).

## 2. The design

### 2.1 Which names are side-built

The route still links its probe first. A name the probe binds to a library is Phase 2's, unchanged. Each name left
unbound goes to one more question: **does the file's own preamble DEFINE it?**

- The route compiles each cgo file's side translation unit (§2.2) to an object file (`-c`), with the load's compiler and
  flags (§2.3).
- It reads that object's symbol table with Go's own readers (`debug/elf`, `debug/macho`, `debug/pe`), as the capture
  already reads the probe. No `nm` is involved.
- The TU takes the address of every candidate name (§2.2), so the compiler emits each one, `static` and `static inline`
  included. A name the preamble only DECLARES is an undefined symbol in the object.
  - **Defined** (local or global): the name is side-built.
  - **Undefined:** today's refusal stands, word for word. Statically resolved C runtime functions keep it too (mingw's
    `mktime` is the measured case). Linking them into the side library would give the package a second copy of C
    runtime state, so that is out of scope.

Only names Go uses are candidates. A helper the preamble defines but Go never names is never touched.

### 2.2 The side translation unit: an address table, not shims

There is one TU per cgo file, because a `static` helper in one file's preamble is visible only to that file. It is
generated text:

```c
#line 1 "add.go"
<the file's preamble, with its #cgo lines removed, as cmd/cgo removes them>
#line 1 "go2cs-sidebuild"
void *go2cs_cgo_table_<n>[] = { (void *)&add, (void *)&twice };
```

The table is the whole mechanism. It is an exported array holding the address of each side-built function, in a fixed
order the converter also writes into the C# bindings. Two alternatives were weighed and rejected:

- **Exported forwarding shims** (`int go2cs_add(int a, int b) { return add(a, b); }`). These need every parameter and
  result spelled as a C type. The converter knows Go's side of each type (`_Ctype_int`, a mirror), not the C spelling,
  and spelling it back (typedefs, `const`, struct tags, function pointers) is a C printer go2cs does not have.
- **`__attribute__((alias))`** to the static function. Mach-O has no aliases, so darwin is excluded.

A table needs no type spelling, works for `static` on every object format, and is one line per name. The C# side
already holds each function's exact signature from cmd/cgo's declarations, which is all it needs to call through the
address (§2.5).

The TU is deterministic text: the preamble's bytes, in the order cmd/cgo reads them, plus the table.

### 2.3 The build

The side build uses the compiler and flags cmd/cgo used:

- **Compiler and target:** the load's `go env CC`, resolved as `cgoCapture` resolves it, plus `cgoArchFlags` for the
  target. The route already refuses a compiler that targets another platform (`cgoHostTarget`, `-dumpmachine`), so a
  cross compiler named in CC (`zig cc -target <triple>`) builds the side library for its target with no new code.
- **Compile flags:** the package's `CgoCFLAGS` and `CgoCPPFLAGS` as `go list` evaluates them for the target, after
  cmd/go's own flag checks; then the environment's `CGO_CFLAGS` and `CGO_CPPFLAGS`, which the route already screens for
  a sysroot or a target flag; then `-fPIC` (except on windows). cmd/go's default `-O2 -g` is kept.
- **Link:** `-shared`, plus the package's `#cgo LDFLAGS` through the existing §11.15 grammar (`parseCgoLinkFlags`).
  The side library therefore records its own dependencies: `libsqlite3`, for go-sqlite3 under `libsqlite3`.

The library is named `go2cs-cgo-<package>-<8 hex of the import path's hash>`, with each OS's prefix and extension
(`lib….so`, `lib….dylib`, `….dll`).

### 2.4 Where the artifacts live

The library is a **build output, never corpus content** (PLAN §5.2, ⟨OQ-4⟩'s recommendation, taken as this slice's
answer):

- The side library is written beside the bindings, at `cgo/<goos>-<goarch>/` in the package's output directory.
- The converted project gains one item that copies that directory to the build output.
- The TUs are written beside it as `cgo/<goos>-<goarch>/<file>.side.c`. They are text and deterministic, so the
  converter's offline tests golden them. The binary is not deterministic and no byte gate reads it.
- The cgo route converts outside the corpus (the corpus stays cgo-off), so nothing enters `src/core`.

The single-file published host is Phase 6's, as PLAN Phase 6 says. This slice's exit gate is a build-output run.
§4 OQ-B asks where the published host finds the library.

### 2.5 The C# side

The emission changes in one place: the native call. The wrapper keeps its doors, pins, mirrors, the native-call
bracket (`syscallEntersyscall` / `syscallExitsyscall`) and the errno form. Only `native` is no longer a `[LibraryImport]`
partial. It becomes a call through the table slot:

```csharp
// C.add, defined in add.go's preamble: side-built (cgo Phase 4).
private static int addᴍnative(int p0, int p1) =>
    ((delegate* unmanaged[Cdecl]<int, int, int>)ᴍside_0[0])(p0, p1);
```

`ᴍside_0` is the file's table, read once from the side library. The calling convention is Cdecl, as the Phase 2
externs declare.

**The errno form.** On linux and darwin, the Phase 2 extern captures errno through `SetLastError = true`. The source
generator's stub for that is `Marshal.SetLastSystemError(0)`, then the call, then `Marshal.GetLastSystemError()`. The
side call emits exactly that sequence, so the two forms read the same slot at the same point. On windows, the errno
form reads the C runtime's slot through `cgoErrnoLocation`. For a side-built function, slot 0 of the file's table
becomes the errno location function its own C runtime exports (`_errno`). That keeps the read on the C runtime the
side library links, which may not be the bound library's. The window Phase 2 names (runtime code on the thread
between the return and the read) is unchanged. It is not claimed closed.

### 2.6 The runtime: loading a side library

`GoCgoLibraries` gains one door: `SideTable(Assembly, string file, string symbol)`. It returns the table's address.

- It loads the side library by an **absolute path** built from the assembly's own directory. So darwin's
  absolute-name rule holds, and the current directory is never searched, as for every Phase 2 binding.
- It reads the table with `NativeLibrary.GetExport`.
- A missing file or symbol is refused by name: `DllNotFoundException` or `EntryPointNotFoundException`, with the
  `docs/Limitations.md#cgo` pointer the other refusals carry.
- The target check of `Register` (A6) runs first, unchanged.

The door is golib public surface at the tier of the other `GoCgoLibraries` members (`EditorBrowsable(Never)`, data
the converter emits). The golib seat gates on unfiltered GolibTests at both configurations and on TestLicensing.

### 2.7 Refusals, by name

| Shape | Refusal |
|---|---|
| the package has `.c`, `.cc`, `.cpp`, `.m` or `.S` files of its own | "C source files of its own: go2cs side-builds the preamble only (cgo Phase 4's preamble-only slice)" |
| a side-built name is a VARIABLE the preamble defines | today's variable refusal, unchanged; §4 OQ-A |
| a side-built function's signature passes a Go string (`_GoString_`) | refused by name: a Go-ABI string has no C-side table form here |
| the side compile or link fails | refused naming the file, with the compiler's first diagnostic line, sanitized as `cgoToolchainLine` sanitizes |
| a file with `//export` | unchanged: Phase 5's, and cgo already restricts that preamble to declarations |

**cmd/cgo's own imports (§11.20, M3).** Today, a name cmd/cgo's own binary does not import is refused
(`cgoImportedByCmdCgo`), because nothing shows that name is what C calls. A side-built name never was an import: cmd/cgo
compiles it into its own objects. The check exempts side-built names, and only those. The table is compiled from the
same preamble text cmd/cgo compiled, so a header that renames the name renames it in both, and the address is the one
C's call would reach.

### 2.8 Security (PLAN §5.4)

cmd/cgo already compiles the preamble at conversion time: `go list` runs cmd/cgo, and cmd/cgo runs the compiler.
The side build compiles the same text with the same compiler and flags, behind cmd/go's flag checks and the route's
own. What is new is that the converted program loads the compiled preamble, as a Go binary contains it. The library is
loaded only from the assembly's directory, never from the current directory.

## 3. Red first, then the exit gate

**The red (1b) lands as its own commit before any code.** It is a new real-route test,
`TestCgoPhase4APreambleDefinedFunctionConvertsAndRunsAsGoDoes`, over one file:

```go
package main

/*
#include <errno.h>
static int add_ints(int a, int b) { return a + b; }
static int halve(int v) { if (v % 2 != 0) { errno = EINVAL; return -1; } return v / 2; }
int twice(int v) { return 2 * v; }
*/
import "C"
```

`main` prints `C.add_ints(2, 3)`, `C.twice(21)`, both results of `C.halve(8)`, and the error of `C.halve(7)`. The
fixture covers three shapes:

- the PLAN §2 `static`;
- a non-static definition, which the probe cannot bind either, since it links no preamble;
- an errno form.

Its arms:

1. **It converts** (converter-cgo legs). Red today: the conversion is refused naming `C.add_ints` with "does not
   compile C yet".
2. **It builds and runs as `go run` does** (cgo-e2e legs, `GO2CS_TESTS_E2E=1`). Red today, at arm 1's refusal.

The existing subtest "a function the preamble defines is refused as compiled C" is RETIRED in the cure commit. Its
fixture becomes arm 1's. Until then it passes, so the red commit carries only the new failure.

**Predictions for the red, scored as worded:**
- On every converter-cgo and cgo-e2e leg (win-x64, linux-x64, osx-arm64, osx-x64), the new test FAILS, naming
  `C.add_ints` and "does not compile C yet". No other test changes.
- On H (no C compiler), the unfiltered `go test` stays green: the new test skips exactly as the other real-route tests
  do. The skip is counted, not assumed.

**The cure seat's gates:**
- the converter's unfiltered `go test`, with offline goldens for the TU text, the table order and the side-call
  emission;
- unfiltered GolibTests at both configurations, and TestLicensing, for the golib door;
- hosted converter-cgo and cgo-e2e on the four legs above. H compiles no C, so every arm that compiles C runs hosted.

**Exit gate (PLAN Phase 4):** a cgo package whose preamble contains real C definitions builds and matches `go run`.
That is arm 2 on all four legs. The flagship (go-sqlite3 under `libsqlite3`, 17 helpers) follows the gate as its own
increment. Its library-on-host questions are RECON §6's.

## 4. Open questions

- **OQ-A: preamble-defined variables.** The same table carries a variable's address (`(void *)&counter`), and
  `GoCgoLibraries.Variable` could read it from the table rather than by name. *Recommendation:* in this slice, since
  it is one table entry and one door overload. Decided before code, with a row of its own.
- **OQ-B: the published single-file host.** Does the side library travel in the bundle
  (`IncludeNativeLibrariesForSelfExtract`) and still load by the assembly-directory path? *Recommendation:* Phase 6's,
  as PLAN Phase 6 places it. This slice states it UNMEASURED.
- **OQ-C: placement.** The owner's ruling (§ STATUS). COORD puts it to the owner with this note.

## 5. Amendment, 2026-11-27: the owner's ruling, and what the cure settles

**The ruling** (the owner, relayed by COORD on 2026-11-27): Phase 4 is GO now, on this branch.
- **OQ-C:** the seat joins .5 if it is finished and fully gated before the M:N seat freeze (around 12-05). Otherwise it
  rides .6.
- **OQ-A is IN this slice.** A variable the preamble defines rides the same table (row below).
- **OQ-B** stays recorded as UNMEASURED, for Phase 6.
- Phase 5 (`//export`) still waits for its own ruling.

**What writing the cure settled.** These refine §2. They do not change its mechanism.

1. **Slot 0 of every table is the side library's own errno location.** It is a function the TU defines,
   `static int *go2cs_cgo_errno_<n>(void) { return &errno; }`, after the preamble and an `#include <errno.h>`. That is
   portable C: `errno` is `*_errno()` on windows, `*__errno_location()` on glibc and `*__error()` on darwin. It replaces
   §2.5's "the errno location function its own C runtime exports (`_errno`)". Taking the address of a dllimport function
   in a static initializer is not a constant on every windows compiler, and the helper reads the same slot. The slot is
   written on every OS, so a table's layout does not depend on the target. Only the windows errno form reads it. Linux
   and darwin keep §2.5's `SetLastSystemError` sequence.
2. **The TU carries cmd/cgo's prolog include.** cmd/cgo compiles every preamble after its builtin prolog, which
   includes `<stddef.h>`. The TU opens with `#include <stddef.h>` under `#line 1 "cgo-builtin-prolog"`, so a preamble
   that uses `size_t` unincluded compiles as it does under cmd/cgo. The rest of the prolog (the `_GoString_` typedefs)
   is not written, because no side-built signature may carry them (row below).
3. **Each name is built in the TU of the first file that uses it.** cmd/cgo writes each C wrapper once, in the first
   file that uses the name (`out.go`, `p.Written`). The route takes that file from the name's first recorded use.
4. **The compile and link take `-I <package directory>`,** as cmd/cgo's compile does, so a preamble's
   `#include "local.h"` resolves. On darwin the link sets the install name to the library's own file name, so no
   conversion path is written into it.
5. **The project copies the library to the build output flat** (`Link`), beside the assembly, which is where
   `SideTable` loads it from.

| Refusal row (§2.7), as cured | Text |
|---|---|
| unbound names in a package with C, C++, Objective-C or assembly files of its own | each name, at its use: "…and the package has C source files of its own (`x.c`): go2cs side-builds the preamble only (cgo Phase 4's preamble-only slice)" |
| a VARIABLE the preamble defines (OQ-A, IN) | not refused: `GoCgoLibraries.SideVariable<T>` boxes its table slot, as `Variable<T>` boxes an export |
| a Go string in a side-built signature | refused before any C runs, by the wrapper's existing scalar check ("is not a scalar"). No name reaches the side build with one |
| a declared-only name (undefined in the object) | today's text, word for word |
| the side compile or link fails | names the file, with the compiler's first diagnostic line through `cgoToolchainLine` |
