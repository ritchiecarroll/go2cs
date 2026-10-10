# CENSUS — Phase 5 inventory: every declaration that compiles today against a throwing stub

> **State: MEASURED, 2026-10-10, at master `4c42cc7d3a`.** Read-only; nothing in `src/core`, the converter or
> the generators was changed. Taken by C2 on COORD's dispatch of the owner's request (the i9 offline), for
> R's Roadmap refresh. All paths repo-relative; Go declarations are read at the pinned toolchain,
> go1.24.13.
>
> **The question.** go2cs-gen's `PartialStubGenerator` gives every bodyless partial method that has no
> implementing part in its compilation a throwing body: `throw new NotImplementedException("<name>: no
> implementation reached this compilation (assembly, cgo, or a linkname whose push did not arrive)")`.
> Such a package compiles clean and throws at the first call. Phase 5 needs the whole population, by
> package, by source class and by platform, and needs to know which of them a validated row's tests reach.
>
> **Headline.** **490 production members compile against a throwing stub in at least one flavour — 229 on windows, 245 on linux, 447 on darwin — in 17 packages, plus one member of a test compile (`internal/abi.FuncPCTestFn`).** By class: windows 118 asm · 100 linkname · 7 vestigial · 3 intrinsic · 1 cgo; linux 132 asm · 103 linkname · 7 vestigial · 3 intrinsic; darwin 217 cgo (its libc trampolines) · 116 asm · 104 linkname · 7 vestigial · 3 intrinsic. `runtime` holds 147 / 159 / 185 of them, `reflect` 64 in every flavour, and `syscall` 129 on darwin. **The generator's own output equals this inventory member for member** in linux, darwin, windows (each flavour built and read, §2). **No stub is reached by a validated row's tests at this master, by runtime evidence** (§5); a static upper bound of 48 members on 7 rows is the watch list. The q82 guard overstates its population by one false member, `internal/trace.totalUtilOf` (§1).

## 1. The instrument, and what it read

**The q82 census guard is the instrument** (`src/go2cs/declaredNotImplemented_test.go`). It is the
generator's predicate ported to text: a partial definition with no implementing part, minus the `-tests`
init hook and minus a `[LibraryImport]` declaration, with positive controls (`mapKeyError`,
`getCovCounterList`) and negative controls checked against an earlier build (`time.runtimeNano`,
`time.runtimeNow`, the four `internal/sync` mutex hooks). At this master it reads, unchanged:

```
declarations 793 · stubbed 505 · packages 40
by reason (the consumer's OWN marker): linkname 75 · cgo 18 · assembly-or-other 412
of the 505, a //go:linkname PUSH exists in this corpus for 61
declared 58 · measured 58 · appeared 0 · vanished 0
```

That count folds a package's per-GOOS flavours into one population and counts per FILE. Phase 5 needs
per platform, so this record EXTENDS the guard rather than writing a second instrument: an uncommitted
test beside it (Appendix A) reuses its predicate (`partialDeclRe`, `partialDefnRe`, `leadingTrivia`,
`leadsABody`, both exclusions) and changes only the unit:

- **Per flavour.** For each of `windows`, `linux` and `darwin`, a package's compilation is what its csproj
  compiles for `-p:GoTargetOS=<goos>`: the package root's `.cs` files plus `<goos>/*.cs` (layout L3). It
  READS THE COMMITTED PER-GOOS FILES, so it needs no build per OS.
- **Production and test.** The production compile excludes `*_test.cs`, `package_test_info.cs` and
  `go2cs_test_host.cs`; a `.tests.csproj` recompiles the production sources with them, so the test compile
  adds only the stubs declared in test files.
- **Source class**, read from Go's own source at the pin (§3), and **test reach** (§5).

**One defect of the guard, found by this extension and excluded here.** Its declaration pattern also
matches an ASSIGNMENT to a local variable named `partial`: `internal/trace/gc.cs:908` reads
`partial = totalUtilOf(...);`, and the guard reports `internal/trace.totalUtilOf` as a stub in every
flavour, although `totalUtilOf` has a body (`gc.cs:319`) and the generator mints nothing for it (§2).
The guard overstates its population by one member. The extension excludes a line that assigns to
`partial`; the guard itself is unchanged by this docs-only record, and the one-line fix is offered to its
owner.

## 2. Cross-check against the generator's actual output

**A text census is not the generator, so each flavour was also BUILT and the generator's own output read.**
`dotnet build src/go2cs-stdlib.slnx -c Release -p:GoTargetOS=<goos>` (all 345 projects; the template
emits generated files under each project's `Generated/`), then every `*.stub.g.cs` under `src/core` keyed by
its project's package and the member its `NotImplementedException` names, and compared with the inventory's
production rows for that flavour, both directions. Build output was purged between flavours (a
`GoTargetOS` switch poisons `obj/`), and after each purge no tracked file was missing.

| Flavour | Stub files the generator emitted | Inventory production rows | In the output, not the inventory | In the inventory, not the output |
|:--|--:|--:|--:|--:|
| linux | 245 | 245 | 0 | 0 |
| darwin | 447 | 447 | 0 | 0 |
| windows | 229 | 229 | 0 | 0 |

The two sets are EQUAL in every flavour built: no member the generator stubs is missing from the
inventory, and the inventory names nothing the generator does not stub. `internal/trace.totalUtilOf` (§1)
has no stub file in any build, which is the guard's false member confirmed by the generator. The
generator runs only in the production compile here; the one test-compile member (§4) is read from the
committed test sources.

## 3. The classes

Each member is classified from its Go declaration at go1.24.13, in this order (the first that applies):

| Class | Measured how |
|:--|:--|
| **linkname** | A `//go:linkname` PUSH to it exists in the corpus and its body does not reach this compilation (the guard's push discriminator), or a push exists only in Go's own source, or its Go declaration carries a `//go:linkname` PULL whose producer has no body here. |
| **cgo** | A dynamic import: the consumer's `//go:cgo_import` marker, or a darwin `libc_*_trampoline`. |
| **asm** | A `TEXT` for it in an assembly file Go selects for `<goos>/amd64`: written `·name` in its own package or `runtime·name` in another (as `internal/bytealg` defines `runtime·memequal_varlen`), or defined by an assembly macro (`CALLFN(·call16, 16)`), or assembly for another arch only (windows `sigFetchGSafe`, 386 only, by Go's own comment). |
| **intrinsic** | A compiler intrinsic: listed in `cmd/compile/internal/ssagen/intrinsics.go`. |
| **vestigial** | Declared in Go with no definition anywhere in Go's source: `runtime`'s `gcWriteBarrierCX` … `R9`, "Called from compiled code; declared for vet; do NOT call from Go" (`stubs_amd64.go:9`). |

Every member resolved: no member is left unclassified, and every one has a bodyless Go declaration at the
pin (the test-compile member's is in `internal/abi/export_test.go`).

## 4. Summary

### Totals per OS

| | windows | linux | darwin |
|:--|--:|--:|--:|
| production stubs | 229 | 245 | 447 |
| test-compile-only stubs | 1 | 1 | 1 |
| packages with a production stub | 12 | 13 | 16 |

Distinct members across the three flavours (a member stubbed in several flavours counted once): **491** (490 production, 1 test-only), in 18 packages.

### Per class per OS (production)

| Class | windows | linux | darwin | What it is |
|:--|--:|--:|--:|:--|
| asm | 118 | 132 | 116 | Go implements it in assembly (a `TEXT` in a `.s` Go selects for the flavour on amd64, a macro-defined symbol, or assembly for another arch only). |
| cgo | 1 | 0 | 217 | A dynamic import: a darwin libc trampoline or a `//go:cgo_import` declaration. |
| linkname | 100 | 103 | 104 | A `//go:linkname` push or pull whose producer body does not reach this compilation. |
| intrinsic | 3 | 3 | 3 | A compiler intrinsic (`cmd/compile/internal/ssagen/intrinsics.go`). |
| vestigial | 7 | 7 | 7 | Declared in Go with no definition anywhere in its source (declared for vet; called only from compiled code). |

### Linkname detail (production)

| | windows | linux | darwin |
|:--|--:|--:|--:|
| push in the corpus, not arriving | 50 | 53 | 54 |
| push in Go only (producer not bodied in the corpus) | 0 | 0 | 0 |
| pull | 50 | 50 | 50 |

### Per package (production stubs per OS; top 10 first, then every other package)

| # | Package | windows | linux | darwin | Classes (all flavours) |
|--:|:--|--:|--:|--:|:--|
| 1 | `runtime` | 147 | 159 | 185 | asm 347, linkname 80, cgo 43, vestigial 21 |
| 2 | `syscall` |  | 4 | 129 | cgo 126, linkname 6, asm 1 |
| 3 | `reflect` | 64 | 64 | 64 | linkname 186, asm 6 |
| 4 | `crypto/x509/internal/macos` |  |  | 29 | cgo 28, linkname 1 |
| 5 | `internal/syscall/unix` |  |  | 21 | cgo 21 |
| 6 | `internal/runtime/sys` | 3 | 3 | 3 | intrinsic 9 |
| 7 | `vendor/golang.org/x/sys/cpu` | 3 | 3 | 3 | asm 6, linkname 3 |
| 8 | `internal/bytealg` | 2 | 2 | 2 | linkname 6 |
| 9 | `internal/cpu` | 2 | 2 | 2 | asm 6 |
| 10 | `internal/syscall/windows` | 2 |  |  | linkname 2 |
| | **below: the rest** | | | | |
| 11 | `net/http` | 2 | 2 | 2 | linkname 6 |
| 12 | `os` |  | 2 | 1 | linkname 3 |
| 13 | `runtime/pprof` |  |  | 2 | linkname 2 |
| 14 | `go/types` | 1 | 1 | 1 | linkname 3 |
| 15 | `internal/coverage/cfile` | 1 | 1 | 1 | linkname 3 |
| 16 | `internal/runtime/maps` | 1 | 1 | 1 | linkname 3 |
| 17 | `net/url` | 1 | 1 | 1 | linkname 3 |

## 5. Reach by the package's own converted tests

**Method.** Per flavour, a TEXTUAL call graph over the package's test compile: every class-level member is
a node; a line inside it names every identifier on it (a call, a method group, a delegate); roots are
every `Test`/`Example`/`Benchmark`/`Fuzz` member of a test file, every `init` member and every static
field initializer. A stub is **reached** if a root reaches it, **named by a test** if a test file names it
directly, and **no tests** if the package has no `.tests.csproj`. The graph over-approximates: it ignores
control flow, platform branches and skips, and it counts a method group (`FuncPCABI0(FuncPCTestFn)`, whose
address is taken and which is never called) as a reach. So "reached" here is an UPPER BOUND, and "no" is
the strong reading: nothing in the package's own tests leads to it at all.

**The measured lower bound is runtime evidence, and it is zero at this master.** A NotImplementedException
from a stub is classified by the test host as an infrastructure-error, a verdict no disclosure can absorb
(stated in the runtime disclosures below), so a banked row's tests cannot be throwing on a stub. The only
committed evidence of a stub reached by a validated row is five runtime disclosures, and every one records
the stub being REPLACED by a body that refuses by name, so none is reached at this master:

| Disclosure (runtime row) | The stub it reached | At this master |
|:--|:--|:--|
| `TestNewOSProc0` (linux) | `clone` (asm) | bodied: refuses by name (`runtime/linux/os_linux_impl.cs`); not in the population |
| `TestSignalM` (linux) | `tgkill`, `getpid` (asm) | bodied (`getpid` answers `Environment.ProcessId`; `tgkill` refuses by name); not in the population |
| `TestStartLineAsm` | `runtime/internal/startlinetest.AsmFunc` (asm) | bodied: refuses by name (`asmfunc_impl.cs`); not in the population |
| `TestTracebackSystemstack` | `internal/runtime/sys.GetCallerPC`, `GetCallerSP` (intrinsic) | still stubs; the refusal now sits AHEAD of them (`export_impl_test.cs`), so the test no longer reaches them |
| `TestG0StackOverflow` | `GetCallerSP` (intrinsic) | as above |

**The static upper bound, by roster row** (the validated rows that hold a stub; a row is read only on the
OSes it is banked for):

Roster rows read: 225 packages. Packages that are roster rows AND hold a stub: `go/types`, `internal/abi`, `internal/coverage/cfile`, `internal/cpu`, `internal/runtime/maps`, `internal/runtime/sys`, `internal/syscall/windows`, `net/http`, `net/url`, `os`, `reflect`, `runtime`, `runtime/pprof`, `syscall`.

| Roster row | OS | Stubs in its compile | Named directly by a test file | Reached in the textual call graph (an upper bound) |
|:--|:--|--:|--:|--:|
| `go/types` | windows | 1 | 0 | 0 |
| `go/types` | linux | 1 | 0 | 0 |
| `internal/abi` | windows | 1 | 1 | 1 |
| `internal/abi` | linux | 1 | 1 | 1 |
| `internal/coverage/cfile` | windows | 1 | 0 | 1 |
| `internal/coverage/cfile` | linux | 1 | 0 | 1 |
| `internal/cpu` | windows | 2 | 0 | 1 |
| `internal/cpu` | linux | 2 | 0 | 1 |
| `internal/runtime/maps` | windows | 1 | 0 | 1 |
| `internal/runtime/maps` | linux | 1 | 0 | 1 |
| `internal/runtime/maps` | darwin | 1 | 0 | 1 |
| `internal/runtime/sys` | windows | 3 | 0 | 0 |
| `internal/runtime/sys` | linux | 3 | 0 | 0 |
| `internal/runtime/sys` | darwin | 3 | 0 | 0 |
| `internal/syscall/windows` | windows | 2 | 0 | 0 |
| `net/http` | windows | 2 | 0 | 0 |
| `net/http` | linux | 2 | 0 | 0 |
| `net/url` | windows | 1 | 0 | 0 |
| `net/url` | linux | 1 | 0 | 0 |
| `net/url` | darwin | 1 | 0 | 0 |
| `os` | linux | 2 | 0 | 2 |
| `reflect` | windows | 64 | 1 | 9 |
| `reflect` | linux | 64 | 1 | 9 |
| `reflect` | darwin | 64 | 1 | 9 |
| `runtime` | windows | 147 | 0 | 16 |
| `runtime` | linux | 159 | 1 | 28 |
| `syscall` | linux | 4 | 0 | 0 |

**By name** — every member the textual graph lets a banked row's tests reach:

| Roster row | Member | Class | OS (banked) | Named by a test file |
|:--|:--|:--|:--|:--|
| `internal/abi` | `FuncPCTestFn` | asm | windows, linux | yes |
| `internal/coverage/cfile` | `getCovCounterList` | linkname | windows, linux | no |
| `internal/cpu` | `cpuid` | asm | windows, linux | no |
| `internal/runtime/maps` | `mapKeyError` | linkname | windows, linux, darwin | no |
| `os` | `ignoreSIGSYS` | linkname | linux | no |
| `os` | `restoreSIGSYS` | linkname | linux | no |
| `reflect` | `call` | linkname | windows, linux, darwin | yes |
| `reflect` | `getStaticuint64s` | linkname | windows, linux, darwin | no |
| `reflect` | `ifaceE2I` | linkname | windows, linux, darwin | no |
| `reflect` | `memmove` | linkname | windows, linux, darwin | no |
| `reflect` | `methodValueCall` | asm | windows, linux, darwin | no |
| `reflect` | `typedmemclr` | linkname | windows, linux, darwin | no |
| `reflect` | `typedmemclrpartial` | linkname | windows, linux, darwin | no |
| `reflect` | `typedmemmove` | linkname | windows, linux, darwin | no |
| `reflect` | `unsafe_New` | linkname | windows, linux, darwin | no |
| `runtime` | `osyield` | asm | linux | yes |
| `runtime` | `asmcgocall` | asm | windows, linux | no |
| `runtime` | `asminit` | asm | linux | no |
| `runtime` | `asyncPreempt` | asm | windows, linux | no |
| `runtime` | `callCgoMmap` | asm | linux | no |
| `runtime` | `callCgoMunmap` | asm | linux | no |
| `runtime` | `callCgoSigaction` | asm | linux | no |
| `runtime` | `cgoSigtramp` | asm | linux | no |
| `runtime` | `checkASM` | asm | windows, linux | no |
| `runtime` | `exceptiontramp` | asm | windows | no |
| `runtime` | `exit` | asm | linux | no |
| `runtime` | `firstcontinuetramp` | asm | windows | no |
| `runtime` | `getlasterror` | asm | windows | no |
| `runtime` | `gettid` | asm | linux | no |
| `runtime` | `goexit` | asm | windows, linux | no |
| `runtime` | `gogo` | asm | windows, linux | no |
| `runtime` | `lastcontinuetramp` | asm | windows | no |
| `runtime` | `main_main` | linkname | windows, linux | no |
| `runtime` | `mcall` | asm | windows, linux | no |
| `runtime` | `mstart` | asm | windows, linux | no |
| `runtime` | `publicationBarrier` | asm | windows, linux | no |
| `runtime` | `raiseproc` | asm | linux | no |
| `runtime` | `reflectcall` | asm | windows, linux | no |
| `runtime` | `rt_sigaction` | asm | linux | no |
| `runtime` | `setg` | asm | linux | no |
| `runtime` | `setitimer` | asm | linux | no |
| `runtime` | `sigaltstack` | asm | linux | no |
| `runtime` | `sigpanic0` | asm | linux | no |
| `runtime` | `sigreturn__sigaction` | asm | linux | no |
| `runtime` | `sigtramp` | asm | linux | no |
| `runtime` | `time_now` | linkname | windows, linux | no |
| `runtime` | `timer_create` | asm | linux | no |
| `runtime` | `tstart_stdcall` | asm | windows | no |

**The three a test file names, read by hand.** None is a call an executed test makes:

- `internal/abi.FuncPCTestFn` (test compile): `abi_test.cs:28` passes it as a METHOD GROUP to
  `abi.FuncPCABI0`, which takes its address; nothing calls it.
- `runtime.osyield` (linux): `export_test.cs:1277` assigns it to the exported `OSYield`, whose only caller is
  `BenchmarkOSYield` (`runtime_test.cs:649`), and validation executes no benchmark.
- `reflect.call`: the token is a LOCAL function of the same name declared inside a test
  (`all_test.cs:4930`), not the stub.

**FINDING, stated as COORD asked:** measured by runtime evidence, ZERO stubs are reached by a validated
row's tests at this master. The static upper bound is 48 members on 7 rows (runtime 33, reflect 9, and one
each in internal/abi, internal/coverage/cfile, internal/cpu, internal/runtime/maps, plus os's two linux
members). They are the watch list: each is one converted code path away from a banked test, so a change that
makes a test reach one turns a banked row into an infrastructure error, which no disclosure absorbs.

## 6. How to re-take this reading

```
git worktree add <dir> 4c42cc7d3a
cd <dir>/src/go2cs
go test -count=1 -run 'TestDeclaredNotImplementedCensus$' -v .        # the guard, unchanged (§1)
# Appendix A saved as src/go2cs/zz_phase5_inventory_test.go (uncommitted):
PHASE5_OUT=inventory.tsv PHASE5_GOROOT="$(go env GOROOT)" \
  go test -count=1 -run 'TestPhase5StubInventory$' -v .                # one row per (flavour, compile, stub)
cd <dir>/src
dotnet build go2cs-stdlib.slnx -c Release -p:GoTargetOS=linux          # the cross-check (§2)
find core -path '*Generated*' -name '*.stub.g.cs'
```

## 7. Controls

| Control | Planted (a bodyless partial `phase5Control` in `unicode`, never committed) | Body supplied (an implementing part beside it) |
|:--|:--|:--|
| The q82 guard, unchanged | `declarations 794 · stubbed 506 · packages 41` (from 793 · 505 · 40) | `stubbed 505`: stops counting it |
| The extension (Appendix A) | one row per flavour: windows, linux, darwin | no row |
| The GENERATOR (`dotnet build src/core/unicode/unicode.csproj -p:GoTargetOS=linux`) | `go.unicode_package.phase5Control.0.stub.g.cs` emitted | no stub file, after a CLEAN rebuild |

⚠ **The generator half has a trap, measured here.** The first bodied rebuild still showed the stub file:
`Generated/` is not cleared between builds, so the previous build's output lingers. The build itself said
otherwise: it exited 0, and a stub beside a body is two implementing parts (CS0757). With `Generated/`,
`obj/` and `bin/` removed first, the bodied build emits no stub. **A reading of `Generated/` is a reading
of the LAST build that wrote each file, not of the current one** — the §2 cross-checks each read a tree
whose build output had been purged before the build.

The guard's own controls held throughout: the positive controls (`internal/coverage/cfile.getCovCounterList`,
`internal/runtime/maps.mapKeyError`) are in the population, and none of its negative controls
(`time.runtimeNano`, `time.runtimeNow`, the four `internal/sync` mutex hooks) is.

## 8. Every member

One line per member, its flavours folded; the Go declaration is read at go1.24.13 (file:line in the
package's own directory). "Tests reach it" is §5's reading per flavour.

| Package | Member | Go declaration | Class | OS | Tests reach it |
|:--|:--|:--|:--|:--|:--|
| `crypto/x509/internal/macos` | `syscall` | `func syscall(fn, a1, a2, a3, a4, a5 uintptr, f1 float64) uintptr` corefoundation.go:208 | linkname: push exists in the corpus (runtime/darwin/sys_darwin.cs) and does not arrive | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFArrayAppendValue_trampoline` | `func x509_CFArrayAppendValue_trampoline()` corefoundation.go:171 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFArrayCreateMutable_trampoline` | `func x509_CFArrayCreateMutable_trampoline()` corefoundation.go:164 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFArrayGetCount_trampoline` | `func x509_CFArrayGetCount_trampoline()` corefoundation.go:133 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFArrayGetValueAtIndex_trampoline` | `func x509_CFArrayGetValueAtIndex_trampoline()` corefoundation.go:141 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFDataCreate_trampoline` | `func x509_CFDataCreate_trampoline()` corefoundation.go:70 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFDataGetBytePtr_trampoline` | `func x509_CFDataGetBytePtr_trampoline()` corefoundation.go:125 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFDataGetLength_trampoline` | `func x509_CFDataGetLength_trampoline()` corefoundation.go:117 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFDateCreate_trampoline` | `func x509_CFDateCreate_trampoline()` corefoundation.go:179 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFDictionaryGetValueIfPresent_trampoline` | `func x509_CFDictionaryGetValueIfPresent_trampoline()` corefoundation.go:94 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFEqual_trampoline` | `func x509_CFEqual_trampoline()` corefoundation.go:149 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFErrorCopyDescription_trampoline` | `func x509_CFErrorCopyDescription_trampoline()` corefoundation.go:187 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFErrorGetCode_trampoline` | `func x509_CFErrorGetCode_trampoline()` corefoundation.go:194 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFNumberGetValue_trampoline` | `func x509_CFNumberGetValue_trampoline()` corefoundation.go:109 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFRelease_trampoline` | `func x509_CFRelease_trampoline()` corefoundation.go:156 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFStringCreateExternalRepresentation_trampoline` | `func x509_CFStringCreateExternalRepresentation_trampoline()` corefoundation.go:205 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_CFStringCreateWithBytes_trampoline` | `func x509_CFStringCreateWithBytes_trampoline()` corefoundation.go:82 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecCertificateCopyData_trampoline` | `func x509_SecCertificateCopyData_trampoline()` security.go:247 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecCertificateCreateWithData_trampoline` | `func x509_SecCertificateCreateWithData_trampoline()` security.go:146 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecPolicyCreateSSL_trampoline` | `func x509_SecPolicyCreateSSL_trampoline()` security.go:162 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustCreateWithCertificates_trampoline` | `func x509_SecTrustCreateWithCertificates_trampoline()` security.go:131 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustEvaluate_trampoline` | `func x509_SecTrustEvaluate_trampoline()` security.go:185 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustEvaluateWithError_trampoline` | `func x509_SecTrustEvaluateWithError_trampoline()` security.go:215 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustGetCertificateAtIndex_trampoline` | `func x509_SecTrustGetCertificateAtIndex_trampoline()` security.go:234 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustGetCertificateCount_trampoline` | `func x509_SecTrustGetCertificateCount_trampoline()` security.go:223 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustGetResult_trampoline` | `func x509_SecTrustGetResult_trampoline()` security.go:198 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustSettingsCopyCertificates_trampoline` | `func x509_SecTrustSettingsCopyCertificates_trampoline()` security.go:102 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustSettingsCopyTrustSettings_trampoline` | `func x509_SecTrustSettingsCopyTrustSettings_trampoline()` security.go:118 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `crypto/x509/internal/macos` | `x509_SecTrustSetVerifyDate_trampoline` | `func x509_SecTrustSetVerifyDate_trampoline()` security.go:173 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `go/types` | `badlinkname_Checker_infer` | `func badlinkname_Checker_infer(*Checker, positioner, []*TypeParam, []Type, *Tuple, []*operand, bool, *error_) []Type` badlinkname.go:20 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `internal/abi` | `FuncPCTestFn (test compile)` | `func FuncPCTestFn()` export_test.go:7 | asm: internal/abi/abi_test.s | all three | yes, named by a test |
| `internal/bytealg` | `abigen_runtime_memequal` | `func abigen_runtime_memequal(a, b unsafe.Pointer, size uintptr) bool` equal_native.go:18 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no tests |
| `internal/bytealg` | `abigen_runtime_memequal_varlen` | `func abigen_runtime_memequal_varlen(a, b unsafe.Pointer) bool` equal_native.go:21 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no tests |
| `internal/coverage/cfile` | `getCovCounterList` | `func getCovCounterList() []rtcov.CovCounterBlob` emit.go:37 | linkname: push exists in the corpus (runtime/covercounter.cs) and does not arrive | all three | yes |
| `internal/cpu` | `cpuid` | `func cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)` cpu_x86.go:12 | asm: internal/cpu/cpu_x86.s | all three | yes |
| `internal/cpu` | `xgetbv` | `func xgetbv() (eax, edx uint32)` cpu_x86.go:15 | asm: internal/cpu/cpu_x86.s | all three | no |
| `internal/runtime/maps` | `mapKeyError` | `func mapKeyError(typ *abi.SwissMapType, p unsafe.Pointer) error` runtime_swiss.go:21 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | yes |
| `internal/runtime/sys` | `GetCallerPC` | `func GetCallerPC() uintptr` intrinsics.go:233 | intrinsic: compiler intrinsic (cmd/compile/internal/ssagen/intrinsics.go) | all three | no |
| `internal/runtime/sys` | `GetCallerSP` | `func GetCallerSP() uintptr` intrinsics.go:235 | intrinsic: compiler intrinsic (cmd/compile/internal/ssagen/intrinsics.go) | all three | no |
| `internal/runtime/sys` | `GetClosurePtr` | `func GetClosurePtr() uintptr` intrinsics.go:256 | intrinsic: compiler intrinsic (cmd/compile/internal/ssagen/intrinsics.go) | all three | no |
| `internal/syscall/unix` | `libc_arc4random_buf_trampoline` | `func libc_arc4random_buf_trampoline()` arc4random_darwin.go:14 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_faccessat_trampoline` | `func libc_faccessat_trampoline()` faccessat_darwin.go:13 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_freeaddrinfo_trampoline` | `func libc_freeaddrinfo_trampoline()` net_darwin.go:62 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_gai_strerror_trampoline` | `func libc_gai_strerror_trampoline()` net_darwin.go:92 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getaddrinfo_trampoline` | `func libc_getaddrinfo_trampoline()` net_darwin.go:44 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getgrgid_r_trampoline` | `func libc_getgrgid_r_trampoline()` user_darwin.go:97 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getgrnam_r_trampoline` | `func libc_getgrnam_r_trampoline()` user_darwin.go:82 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getgrouplist_trampoline` | `func libc_getgrouplist_trampoline()` user_darwin.go:14 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getnameinfo_trampoline` | `func libc_getnameinfo_trampoline()` net_darwin.go:71 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getpwnam_r_trampoline` | `func libc_getpwnam_r_trampoline()` user_darwin.go:52 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_getpwuid_r_trampoline` | `func libc_getpwuid_r_trampoline()` user_darwin.go:67 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_grantpt_trampoline` | `func libc_grantpt_trampoline()` pty_darwin.go:13 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_mkdirat_trampoline` | `func libc_mkdirat_trampoline()` at_darwin.go:43 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_posix_openpt_trampoline` | `func libc_posix_openpt_trampoline()` pty_darwin.go:57 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_ptsname_r_trampoline` | `func libc_ptsname_r_trampoline()` pty_darwin.go:35 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_readlinkat_trampoline` | `func libc_readlinkat_trampoline()` at_darwin.go:15 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_sysconf_trampoline` | `func libc_sysconf_trampoline()` user_darwin.go:112 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libc_unlockpt_trampoline` | `func libc_unlockpt_trampoline()` pty_darwin.go:24 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libresolv_res_9_nclose_trampoline` | `func libresolv_res_9_nclose_trampoline()` net_darwin.go:141 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libresolv_res_9_ninit_trampoline` | `func libresolv_res_9_ninit_trampoline()` net_darwin.go:128 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/unix` | `libresolv_res_9_nsearch_trampoline` | `func libresolv_res_9_nsearch_trampoline()` net_darwin.go:150 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no tests |
| `internal/syscall/windows` | `QueryPerformanceCounter` | `func QueryPerformanceCounter() int64` syscall_windows.go:497 | linkname: push exists in the corpus (runtime/windows/os_windows.cs) and does not arrive | windows | no |
| `internal/syscall/windows` | `QueryPerformanceFrequency` | `func QueryPerformanceFrequency() int64` syscall_windows.go:503 | linkname: push exists in the corpus (runtime/windows/os_windows.cs) and does not arrive | windows | no |
| `net/http` | `badRoundTrip` | `func badRoundTrip(*Transport, *Request) (*Response, error)` roundtrip.go:20 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `net/http` | `badServeHTTP` | `func badServeHTTP(serverHandler, ResponseWriter, *Request)` server.go:3304 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `net/url` | `badSetPath` | `func badSetPath(*URL, string) error` url.go:731 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `os` | `ignoreSIGSYS` | `func ignoreSIGSYS()` pidfd_linux.go:197 | linkname: push exists in the corpus (runtime/linux/signal_unix.cs) and does not arrive | linux | yes |
| `os` | `readdir_r` | `func readdir_r(dir uintptr, entry *syscall.Dirent, result **syscall.Dirent) (res syscall.Errno)` dir_darwin.go:148 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | darwin | no |
| `os` | `restoreSIGSYS` | `func restoreSIGSYS()` pidfd_linux.go:200 | linkname: push exists in the corpus (runtime/linux/signal_unix.cs) and does not arrive | linux | yes |
| `reflect` | `badlinkname_rtype_Align` | `func badlinkname_rtype_Align(*rtype) int` badlinkname.go:40 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_AssignableTo` | `func badlinkname_rtype_AssignableTo(*rtype, Type) bool` badlinkname.go:43 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Bits` | `func badlinkname_rtype_Bits(*rtype) int` badlinkname.go:46 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_ChanDir` | `func badlinkname_rtype_ChanDir(*rtype) ChanDir` badlinkname.go:49 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Comparable` | `func badlinkname_rtype_Comparable(*rtype) bool` badlinkname.go:52 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_ConvertibleTo` | `func badlinkname_rtype_ConvertibleTo(*rtype, Type) bool` badlinkname.go:55 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Elem` | `func badlinkname_rtype_Elem(*rtype) Type` badlinkname.go:58 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Field` | `func badlinkname_rtype_Field(*rtype, int) StructField` badlinkname.go:61 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_FieldAlign` | `func badlinkname_rtype_FieldAlign(*rtype) int` badlinkname.go:64 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_FieldByIndex` | `func badlinkname_rtype_FieldByIndex(*rtype, []int) StructField` badlinkname.go:67 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_FieldByName` | `func badlinkname_rtype_FieldByName(*rtype, string) (StructField, bool)` badlinkname.go:70 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_FieldByNameFunc` | `func badlinkname_rtype_FieldByNameFunc(*rtype, func(string) bool) (StructField, bool)` badlinkname.go:73 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Implements` | `func badlinkname_rtype_Implements(*rtype, Type) bool` badlinkname.go:76 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_In` | `func badlinkname_rtype_In(*rtype, int) Type` badlinkname.go:79 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_IsVariadic` | `func badlinkname_rtype_IsVariadic(*rtype) bool` badlinkname.go:82 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Key` | `func badlinkname_rtype_Key(*rtype) Type` badlinkname.go:85 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Kind` | `func badlinkname_rtype_Kind(*rtype) Kind` badlinkname.go:88 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Len` | `func badlinkname_rtype_Len(*rtype) int` badlinkname.go:91 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Method` | `func badlinkname_rtype_Method(*rtype, int) Method` badlinkname.go:94 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_MethodByName` | `func badlinkname_rtype_MethodByName(*rtype, string) (Method, bool)` badlinkname.go:97 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Name` | `func badlinkname_rtype_Name(*rtype) string` badlinkname.go:100 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_NumField` | `func badlinkname_rtype_NumField(*rtype) int` badlinkname.go:103 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_NumIn` | `func badlinkname_rtype_NumIn(*rtype) int` badlinkname.go:106 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_NumMethod` | `func badlinkname_rtype_NumMethod(*rtype) int` badlinkname.go:109 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_NumOut` | `func badlinkname_rtype_NumOut(*rtype) int` badlinkname.go:112 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Out` | `func badlinkname_rtype_Out(*rtype, int) Type` badlinkname.go:115 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_PkgPath` | `func badlinkname_rtype_PkgPath(*rtype) string` badlinkname.go:118 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_ptrTo` | `func badlinkname_rtype_ptrTo(*rtype) *abi.Type` badlinkname.go:127 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_Size` | `func badlinkname_rtype_Size(*rtype) uintptr` badlinkname.go:121 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_rtype_String` | `func badlinkname_rtype_String(*rtype) string` badlinkname.go:124 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `badlinkname_Value_pointer` | `func badlinkname_Value_pointer(Value) unsafe.Pointer` badlinkname.go:130 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `reflect` | `call` | `func call(stackArgsType *abi.Type, f, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` value.go:3652 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | yes, named by a test |
| `reflect` | `chancap` | `func chancap(ch unsafe.Pointer) int` value.go:3552 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `chanclose` | `func chanclose(ch unsafe.Pointer)` value.go:3555 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `chanlen` | `func chanlen(ch unsafe.Pointer) int` value.go:3558 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `chanrecv` | `func chanrecv(ch unsafe.Pointer, nb bool, val unsafe.Pointer) (selected, received bool)` value.go:3568 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `chansend0` | `func chansend0(ch unsafe.Pointer, val unsafe.Pointer, nb bool) bool` value.go:3571 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `getStaticuint64s` | `func getStaticuint64s() *[256]uint64` type.go:1145 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | yes |
| `reflect` | `growslice` | `func growslice(t *abi.Type, old unsafeheader.Slice, num int) unsafeheader.Slice` value.go:3695 | linkname: push exists in the corpus (runtime/slice.cs) and does not arrive | all three | no |
| `reflect` | `ifaceE2I` | `func ifaceE2I(t *abi.Type, src any, dst unsafe.Pointer)` value.go:3654 | linkname: push exists in the corpus (runtime/iface.cs) and does not arrive | all three | yes |
| `reflect` | `makechan` | `func makechan(typ *abi.Type, size int) (ch unsafe.Pointer)` value.go:3578 | linkname: push exists in the corpus (runtime/chan.cs) and does not arrive | all three | no |
| `reflect` | `makeFuncStub` | `func makeFuncStub()` makefunc.go:78 | asm: reflect/asm_amd64.s | all three | no |
| `reflect` | `makemap` | `func makemap(t *abi.Type, cap int) (m unsafe.Pointer)` value.go:3579 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapaccess` | `func mapaccess(t *abi.Type, m unsafe.Pointer, key unsafe.Pointer) (val unsafe.Pointer)` value.go:3582 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapaccess_faststr` | `func mapaccess_faststr(t *abi.Type, m unsafe.Pointer, key string) (val unsafe.Pointer)` value.go:3585 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapassign0` | `func mapassign0(t *abi.Type, m unsafe.Pointer, key, val unsafe.Pointer)` value.go:3588 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapassign_faststr0` | `func mapassign_faststr0(t *abi.Type, m unsafe.Pointer, key string, val unsafe.Pointer)` value.go:3607 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapclear` | `func mapclear(t *abi.Type, m unsafe.Pointer)` value.go:3624 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapdelete` | `func mapdelete(t *abi.Type, m unsafe.Pointer, key unsafe.Pointer)` value.go:3616 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `mapdelete_faststr` | `func mapdelete_faststr(t *abi.Type, m unsafe.Pointer, key string)` value.go:3619 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `maplen` | `func maplen(m unsafe.Pointer) int` value.go:3622 | linkname: push exists in the corpus (runtime/map_swiss.cs) and does not arrive | all three | no |
| `reflect` | `memmove` | `func memmove(dst, src unsafe.Pointer, size uintptr)` value.go:3659 | linkname: push exists in the corpus (runtime/stubs.cs) and does not arrive | all three | yes |
| `reflect` | `methodValueCall` | `func methodValueCall()` makefunc.go:141 | asm: reflect/asm_amd64.s | all three | yes |
| `reflect` | `rselect` | `func rselect([]runtimeSelect) (chosen int, recvOK bool)` value.go:2747 | linkname: push exists in the corpus (runtime/windows/select.cs) and does not arrive | all three | no |
| `reflect` | `typedarrayclear` | `func typedarrayclear(elemType *abi.Type, ptr unsafe.Pointer, len int)` value.go:3687 | linkname: push exists in the corpus (runtime/mbarrier.cs) and does not arrive | all three | no |
| `reflect` | `typedmemclr` | `func typedmemclr(t *abi.Type, ptr unsafe.Pointer)` value.go:3669 | linkname: push exists in the corpus (runtime/mbarrier.cs) and does not arrive | all three | yes |
| `reflect` | `typedmemclrpartial` | `func typedmemclrpartial(t *abi.Type, ptr unsafe.Pointer, off, size uintptr)` value.go:3675 | linkname: push exists in the corpus (runtime/mbarrier.cs) and does not arrive | all three | yes |
| `reflect` | `typedmemmove` | `func typedmemmove(t *abi.Type, dst, src unsafe.Pointer)` value.go:3664 | linkname: push exists in the corpus (runtime/mbarrier.cs) and does not arrive | all three | yes |
| `reflect` | `typedslicecopy` | `func typedslicecopy(t *abi.Type, dst, src unsafeheader.Slice) int` value.go:3681 | linkname: push exists in the corpus (runtime/mbarrier.cs) and does not arrive | all three | no |
| `reflect` | `typehash` | `func typehash(t *abi.Type, p unsafe.Pointer, h uintptr) uintptr` value.go:3690 | linkname: push exists in the corpus (runtime/alg.cs) and does not arrive | all three | no |
| `reflect` | `unsafe_New` | `func unsafe_New(*abi.Type) unsafe.Pointer` value.go:2897 | linkname: push exists in the corpus (runtime/windows/malloc.cs) and does not arrive | all three | yes |
| `reflect` | `unsafe_NewArray` | `func unsafe_NewArray(*abi.Type, int) unsafe.Pointer` value.go:2900 | linkname: push exists in the corpus (runtime/windows/malloc.cs) and does not arrive | all three | no |
| `reflect` | `unsafeslice` | `func unsafeslice(t *abi.Type, ptr unsafe.Pointer, len int)` value.go:3698 | linkname: push exists in the corpus (runtime/unsafe.cs) and does not arrive | all three | no |
| `reflect` | `verifyNotInHeapPtr` | `func verifyNotInHeapPtr(p uintptr) bool` value.go:3692 | linkname: push exists in the corpus (runtime/mbitmap.cs) and does not arrive | all three | no |
| `runtime` | `abort` | `func abort()` stubs.go:409 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `access` | `func access(name *byte, mode int32) int32` stubs_linux.go:18 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `addmoduledata` | `func addmoduledata()` stubs.go:435 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `arc4random_buf_trampoline` | `func arc4random_buf_trampoline()` sys_darwin.go:581 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `asmcgocall` | `func asmcgocall(fn, arg unsafe.Pointer) int32` stubs.go:311 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `asmcgocall_landingpad` | `func asmcgocall_landingpad()` stubs_amd64.go:45 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `asmcgocall_no_g` | `func asmcgocall_no_g(fn, arg unsafe.Pointer)` stubs_amd64.go:42 | asm: runtime/asm_amd64.s | all three | no; yes |
| `runtime` | `asminit` | `func asminit()` stubs.go:216 | asm: runtime/asm_amd64.s | all three | no; yes |
| `runtime` | `asmstdcall` | `func asmstdcall(fn unsafe.Pointer)` os_windows.go:216 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `asmstdcall_trampoline` | `func asmstdcall_trampoline(args unsafe.Pointer)` os_windows.go:939 | cgo: dynamic import (cgo or a libc trampoline) | windows | no |
| `runtime` | `asyncPreempt` | `func asyncPreempt()` preempt.go:299 | asm: runtime/preempt_amd64.s | all three | yes |
| `runtime` | `badFuncInfoEntry` | `func badFuncInfoEntry(funcInfo) uintptr` symtab.go:877 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `badSrcFunc` | `func badSrcFunc(*inlineUnwinder, inlineFrame) srcFunc` symtabinl.go:130 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `badSrcFuncName` | `func badSrcFuncName(srcFunc) string` symtab.go:954 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `breakpoint` | `func breakpoint()` stubs.go:218 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `call1024` | `func call1024(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:345 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call1048576` | `func call1048576(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:355 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call1073741824` | `func call1073741824(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:365 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call128` | `func call128(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:342 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call131072` | `func call131072(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:352 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call134217728` | `func call134217728(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:362 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call16` | `func call16(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:339 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call16384` | `func call16384(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:349 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call16777216` | `func call16777216(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:359 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call2048` | `func call2048(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:346 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call2097152` | `func call2097152(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:356 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call256` | `func call256(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:343 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call262144` | `func call262144(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:353 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call268435456` | `func call268435456(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:363 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call32` | `func call32(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:340 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call32768` | `func call32768(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:350 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call33554432` | `func call33554432(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:360 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call4096` | `func call4096(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:347 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call4194304` | `func call4194304(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:357 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call512` | `func call512(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:344 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call524288` | `func call524288(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:354 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call536870912` | `func call536870912(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:364 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call64` | `func call64(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:341 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call65536` | `func call65536(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:351 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call67108864` | `func call67108864(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:361 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call8192` | `func call8192(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:348 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `call8388608` | `func call8388608(typ, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:358 | asm: defined by an assembly macro in runtime/asm_amd64.s | all three | no |
| `runtime` | `callbackasm` | `func callbackasm()` syscall_windows.go:230 | asm: runtime/zcallback_windows.s | windows | no |
| `runtime` | `callbackasm1` | `func callbackasm1()` os_windows.go:1126 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `callCgoMmap` | `func callCgoMmap(addr unsafe.Pointer, n uintptr, prot, flags, fd int32, off uint32) uintptr` cgo_mmap.go:63 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `callCgoMunmap` | `func callCgoMunmap(addr unsafe.Pointer, n uintptr)` cgo_mmap.go:70 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `callCgoSigaction` | `func callCgoSigaction(sig uintptr, new, old *sigactiont) int32` cgo_sigaction.go:94 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `cgocallback` | `func cgocallback(fn, frame, ctxt uintptr)` stubs.go:212 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `cgoSigtramp` | `func cgoSigtramp()` os_linux.go:429 | asm: runtime/sys_linux_amd64.s | linux, darwin | yes |
| `runtime` | `checkASM` | `func checkASM() bool` stubs.go:393 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `close_trampoline` | `func close_trampoline()` sys_darwin.go:318 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `connect` | `func connect(fd int32, addr unsafe.Pointer, len int32) int32` stubs_linux.go:19 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `debugCallPanicked` | `func debugCallPanicked(val any)` debugcall.go:26 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `debugCallV2` | `func debugCallV2()` debugcall.go:25 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `duffcopy` | `func duffcopy()` stubs.go:432 | asm: runtime/duff_amd64.s | all three | no |
| `runtime` | `duffzero` | `func duffzero()` stubs.go:431 | asm: runtime/duff_amd64.s | all three | no |
| `runtime` | `exceptiontramp` | `func exceptiontramp()` signal_windows.go:44 | asm: runtime/sys_windows_amd64.s | windows | yes |
| `runtime` | `exit` | `func exit(code int32)` stubs2.go:20 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `exit_trampoline` | `func exit_trampoline()` sys_darwin.go:328 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `exitThread` | `func exitThread(wait *atomic.Uint32)` stubs2.go:44 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `fcntl_trampoline` | `func fcntl_trampoline()` sys_darwin.go:485 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `firstcontinuetramp` | `func firstcontinuetramp()` signal_windows.go:45 | asm: runtime/sys_windows_amd64.s | windows | yes |
| `runtime` | `futex` | `func futex(addr unsafe.Pointer, op int32, val uint32, ts, addr2 unsafe.Pointer, val3 uint32) int32` os_linux.go:43 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `gcWriteBarrier1` | `func gcWriteBarrier1()` stubs.go:412 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier2` | `func gcWriteBarrier2()` stubs.go:423 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `gcWriteBarrier3` | `func gcWriteBarrier3()` stubs.go:425 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier4` | `func gcWriteBarrier4()` stubs.go:426 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier5` | `func gcWriteBarrier5()` stubs.go:427 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier6` | `func gcWriteBarrier6()` stubs.go:428 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier7` | `func gcWriteBarrier7()` stubs.go:429 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrier8` | `func gcWriteBarrier8()` stubs.go:430 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `gcWriteBarrierBP` | `func gcWriteBarrierBP()` stubs_amd64.go:13 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierBX` | `func gcWriteBarrierBX()` stubs_amd64.go:12 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierCX` | `func gcWriteBarrierCX()` stubs_amd64.go:10 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierDX` | `func gcWriteBarrierDX()` stubs_amd64.go:11 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierR8` | `func gcWriteBarrierR8()` stubs_amd64.go:15 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierR9` | `func gcWriteBarrierR9()` stubs_amd64.go:16 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `gcWriteBarrierSI` | `func gcWriteBarrierSI()` stubs_amd64.go:14 | vestigial: no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code) | all three | no |
| `runtime` | `getlasterror` | `func getlasterror() uint32` os_windows.go:359 | asm: runtime/sys_windows_amd64.s | windows | yes |
| `runtime` | `gettid` | `func gettid() uint32` os_linux.go:394 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `goexit` | `func goexit(neverCallThisFunction)` stubs.go:291 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `gogo` | `func gogo(buf *gobuf)` stubs.go:214 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `issetugid_trampoline` | `func issetugid_trampoline()` sys_darwin.go:599 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `kevent_trampoline` | `func kevent_trampoline()` sys_darwin.go:504 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `kqueue_trampoline` | `func kqueue_trampoline()` sys_darwin.go:493 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `lastcontinuetramp` | `func lastcontinuetramp()` signal_windows.go:46 | asm: runtime/sys_windows_amd64.s | windows | yes |
| `runtime` | `mach_vm_region_trampoline` | `func mach_vm_region_trampoline()` sys_darwin.go:634 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `madvise_trampoline` | `func madvise_trampoline()` sys_darwin.go:287 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `main_main` | `func main_main()` proc.go:135 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | yes |
| `runtime` | `mapaccess1` | `func mapaccess1(t *abi.SwissMapType, m *maps.Map, key unsafe.Pointer) unsafe.Pointer` map_swiss.go:89 | linkname: push exists in the corpus (internal/runtime/maps/runtime_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess1_fast32` | `func mapaccess1_fast32(t *abi.SwissMapType, m *maps.Map, key uint32) unsafe.Pointer` map_fast32_swiss.go:18 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast32_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess1_fast64` | `func mapaccess1_fast64(t *abi.SwissMapType, m *maps.Map, key uint64) unsafe.Pointer` map_fast64_swiss.go:18 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast64_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess1_faststr` | `func mapaccess1_faststr(t *abi.SwissMapType, m *maps.Map, ky string) unsafe.Pointer` map_faststr_swiss.go:18 | linkname: push exists in the corpus (internal/runtime/maps/runtime_faststr_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess2` | `func mapaccess2(t *abi.SwissMapType, m *maps.Map, key unsafe.Pointer) (unsafe.Pointer, bool)` map_swiss.go:100 | linkname: push exists in the corpus (internal/runtime/maps/runtime_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess2_fast32` | `func mapaccess2_fast32(t *abi.SwissMapType, m *maps.Map, key uint32) (unsafe.Pointer, bool)` map_fast32_swiss.go:29 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast32_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess2_fast64` | `func mapaccess2_fast64(t *abi.SwissMapType, m *maps.Map, key uint64) (unsafe.Pointer, bool)` map_fast64_swiss.go:29 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast64_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapaccess2_faststr` | `func mapaccess2_faststr(t *abi.SwissMapType, m *maps.Map, ky string) (unsafe.Pointer, bool)` map_faststr_swiss.go:29 | linkname: push exists in the corpus (internal/runtime/maps/runtime_faststr_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign` | `func mapassign(t *abi.SwissMapType, m *maps.Map, key unsafe.Pointer) unsafe.Pointer` map_swiss.go:133 | linkname: push exists in the corpus (internal/runtime/maps/runtime_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign_fast32` | `func mapassign_fast32(t *abi.SwissMapType, m *maps.Map, key uint32) unsafe.Pointer` map_fast32_swiss.go:41 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast32_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign_fast32ptr` | `func mapassign_fast32ptr(t *abi.SwissMapType, m *maps.Map, key unsafe.Pointer) unsafe.Pointer` map_fast32_swiss.go:52 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast32_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign_fast64` | `func mapassign_fast64(t *abi.SwissMapType, m *maps.Map, key uint64) unsafe.Pointer` map_fast64_swiss.go:41 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast64_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign_fast64ptr` | `func mapassign_fast64ptr(t *abi.SwissMapType, m *maps.Map, key unsafe.Pointer) unsafe.Pointer` map_fast64_swiss.go:53 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast64_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapassign_faststr` | `func mapassign_faststr(t *abi.SwissMapType, m *maps.Map, s string) unsafe.Pointer` map_faststr_swiss.go:41 | linkname: push exists in the corpus (internal/runtime/maps/runtime_faststr_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapdelete_fast32` | `func mapdelete_fast32(t *abi.SwissMapType, m *maps.Map, key uint32)` map_fast32_swiss.go:55 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast32_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapdelete_fast64` | `func mapdelete_fast64(t *abi.SwissMapType, m *maps.Map, key uint64)` map_fast64_swiss.go:56 | linkname: push exists in the corpus (internal/runtime/maps/runtime_fast64_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapdelete_faststr` | `func mapdelete_faststr(t *abi.SwissMapType, m *maps.Map, ky string)` map_faststr_swiss.go:44 | linkname: push exists in the corpus (internal/runtime/maps/runtime_faststr_swiss.cs) and does not arrive | all three | no |
| `runtime` | `mapinitnoop` | `func mapinitnoop()` map_swiss.go:326 | asm: runtime/asm.s | all three | no |
| `runtime` | `mcall` | `func mcall(fn func(*g))` stubs.go:47 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `memequal_varlen` | `func memequal_varlen(a, b unsafe.Pointer) bool` stubs.go:395 | asm: internal/bytealg/equal_amd64.s | all three | no |
| `runtime` | `mlock_trampoline` | `func mlock_trampoline()` sys_darwin.go:295 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `mmap_trampoline` | `func mmap_trampoline()` sys_darwin.go:271 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `morestack` | `func morestack()` stubs.go:313 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `morestack_noctxt` | `func morestack_noctxt()` stubs.go:325 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `mstart` | `func mstart()` proc.go:1803 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `mstart_stub` | `func mstart_stub()` os_darwin.go:252 | asm: runtime/sys_darwin_amd64.s | darwin | yes |
| `runtime` | `munmap_trampoline` | `func munmap_trampoline()` sys_darwin.go:279 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `nanotime_trampoline` | `func nanotime_trampoline()` sys_darwin.go:381 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `open_trampoline` | `func open_trampoline()` sys_darwin.go:359 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `osinit_hack_trampoline` | `func osinit_hack_trampoline()` sys_darwin.go:252 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `osSetupTLS` | `func osSetupTLS(mp *m)` tls_windows_amd64.go:10 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `osyield` | `func osyield()` os_linux.go:460 | asm: runtime/sys_linux_amd64.s | linux | yes, named by a test |
| `runtime` | `panicIndex` | `func panicIndex(x int, y int)` panic.go:210 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicIndexU` | `func panicIndexU(x uint, y int)` panic.go:211 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3Acap` | `func panicSlice3Acap(x int, y int)` panic.go:220 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3AcapU` | `func panicSlice3AcapU(x uint, y int)` panic.go:221 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3Alen` | `func panicSlice3Alen(x int, y int)` panic.go:218 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3AlenU` | `func panicSlice3AlenU(x uint, y int)` panic.go:219 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3B` | `func panicSlice3B(x int, y int)` panic.go:222 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3BU` | `func panicSlice3BU(x uint, y int)` panic.go:223 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3C` | `func panicSlice3C(x int, y int)` panic.go:224 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSlice3CU` | `func panicSlice3CU(x uint, y int)` panic.go:225 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceAcap` | `func panicSliceAcap(x int, y int)` panic.go:214 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceAcapU` | `func panicSliceAcapU(x uint, y int)` panic.go:215 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceAlen` | `func panicSliceAlen(x int, y int)` panic.go:212 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceAlenU` | `func panicSliceAlenU(x uint, y int)` panic.go:213 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceB` | `func panicSliceB(x int, y int)` panic.go:216 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceBU` | `func panicSliceBU(x uint, y int)` panic.go:217 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `panicSliceConvert` | `func panicSliceConvert(x int, y int)` panic.go:226 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `pipe_trampoline` | `func pipe_trampoline()` sys_darwin.go:311 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `proc_regionfilename_trampoline` | `func proc_regionfilename_trampoline()` sys_darwin.go:651 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_attr_getstacksize_trampoline` | `func pthread_attr_getstacksize_trampoline()` sys_darwin.go:169 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_attr_init_trampoline` | `func pthread_attr_init_trampoline()` sys_darwin.go:159 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_attr_setdetachstate_trampoline` | `func pthread_attr_setdetachstate_trampoline()` sys_darwin.go:178 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_cond_init_trampoline` | `func pthread_cond_init_trampoline()` sys_darwin.go:542 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_cond_signal_trampoline` | `func pthread_cond_signal_trampoline()` sys_darwin.go:572 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_cond_timedwait_relative_np_trampoline` | `func pthread_cond_timedwait_relative_np_trampoline()` sys_darwin.go:563 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_cond_wait_trampoline` | `func pthread_cond_wait_trampoline()` sys_darwin.go:552 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_create_trampoline` | `func pthread_create_trampoline()` sys_darwin.go:188 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_kill_trampoline` | `func pthread_kill_trampoline()` sys_darwin.go:211 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_mutex_init_trampoline` | `func pthread_mutex_init_trampoline()` sys_darwin.go:514 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `pthread_mutex_lock_trampoline` | `func pthread_mutex_lock_trampoline()` sys_darwin.go:523 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_mutex_unlock_trampoline` | `func pthread_mutex_unlock_trampoline()` sys_darwin.go:532 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `pthread_self_trampoline` | `func pthread_self_trampoline()` sys_darwin.go:203 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `publicationBarrier` | `func publicationBarrier()` stubs.go:308 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `raise` | `func raise(sig uint32)` os_linux.go:455 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `raise_trampoline` | `func raise_trampoline()` sys_darwin.go:195 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `raiseproc` | `func raiseproc(sig uint32)` os_linux.go:456 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `raiseproc_trampoline` | `func raiseproc_trampoline()` sys_darwin.go:440 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `read_trampoline` | `func read_trampoline()` sys_darwin.go:304 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `reflectcall` | `func reflectcall(stackArgsType *_type, fn, stackArgs unsafe.Pointer, stackArgsSize, stackRetOffset, frameSize uint32, regArgs *abi.RegArgs)` stubs.go:264 | asm: runtime/asm_amd64.s | all three | yes |
| `runtime` | `retpolineAX` | `func retpolineAX()` stubs_amd64.go:25 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineBP` | `func retpolineBP()` stubs_amd64.go:29 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineBX` | `func retpolineBX()` stubs_amd64.go:28 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineCX` | `func retpolineCX()` stubs_amd64.go:26 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineDI` | `func retpolineDI()` stubs_amd64.go:31 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineDX` | `func retpolineDX()` stubs_amd64.go:27 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR10` | `func retpolineR10()` stubs_amd64.go:34 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR11` | `func retpolineR11()` stubs_amd64.go:35 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR12` | `func retpolineR12()` stubs_amd64.go:36 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR13` | `func retpolineR13()` stubs_amd64.go:37 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR14` | `func retpolineR14()` stubs_amd64.go:38 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR15` | `func retpolineR15()` stubs_amd64.go:39 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR8` | `func retpolineR8()` stubs_amd64.go:32 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineR9` | `func retpolineR9()` stubs_amd64.go:33 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `retpolineSI` | `func retpolineSI()` stubs_amd64.go:30 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `return0` | `func return0()` stubs.go:334 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `rt0_go` | `func rt0_go()` stubs.go:327 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `rt_sigaction` | `func rt_sigaction(sig uintptr, new, old *sigactiont, size uintptr) int32` os_linux.go:562 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `sbrk0` | `func sbrk0() uintptr` stubs_linux.go:11 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `sched_getaffinity` | `func sched_getaffinity(pid, len uintptr, buf *byte) int32` os_linux.go:459 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `sehtramp` | `func sehtramp()` signal_windows.go:47 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `setg` | `func setg(gg *g)` stubs.go:217 | asm: runtime/asm_amd64.s | all three | no; yes |
| `runtime` | `setitimer` | `func setitimer(mode int32, new, old *itimerval)` os_linux.go:435 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `setitimer_trampoline` | `func setitimer_trampoline()` sys_darwin.go:449 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `settls` | `func settls()` stubs_amd64.go:22 | asm: runtime/sys_windows_amd64.s | all three | no |
| `runtime` | `sigaction_trampoline` | `func sigaction_trampoline()` sys_darwin.go:408 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `sigaltstack` | `func sigaltstack(new, old *stackt)` os_linux.go:432 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `sigaltstack_trampoline` | `func sigaltstack_trampoline()` sys_darwin.go:433 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `sigFetchGSafe` | `func sigFetchGSafe() *g` signal_windows.go:122 | asm: assembly for another arch only (runtime/sys_windows_386.s,runtime/sys_windows_386.s); no amd64 body | windows | no |
| `runtime` | `sigfwd` | `func sigfwd(fn uintptr, sig uint32, info *siginfo, ctx unsafe.Pointer)` signal_unix.go:1153 | asm: runtime/sys_linux_amd64.s | linux, darwin | no |
| `runtime` | `sigpanic0` | `func sigpanic0()` stubs.go:441 | asm: runtime/asm.s … | all three | no; yes |
| `runtime` | `sigprocmask_trampoline` | `func sigprocmask_trampoline()` sys_darwin.go:417 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `sigresume` | `func sigresume()` signal_windows.go:48 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `sigreturn__sigaction` | `func sigreturn__sigaction()` os_linux.go:427 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `sigtramp` | `func sigtramp()` os_linux.go:428 | asm: runtime/sys_linux_amd64.s | linux, darwin | yes |
| `runtime` | `socket` | `func socket(domain int32, typ int32, prot int32) int32` stubs_linux.go:20 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `spillArgs` | `func spillArgs()` stubs_amd64.go:51 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `stackcheck` | `func stackcheck()` stubs_amd64.go:19 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `switchToCrashStack0` | `func switchToCrashStack0(fn func())` proc.go:628 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `sync_atomic_CompareAndSwapUintptr` | `func sync_atomic_CompareAndSwapUintptr(ptr *uintptr, old, new uintptr) bool` atomic_pointer.go:112 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `sync_atomic_StoreUintptr` | `func sync_atomic_StoreUintptr(ptr *uintptr, new uintptr)` atomic_pointer.go:81 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `sync_atomic_SwapUintptr` | `func sync_atomic_SwapUintptr(ptr *uintptr, new uintptr) uintptr` atomic_pointer.go:96 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no |
| `runtime` | `syscall` | `func syscall()` sys_darwin.go:32 | asm: runtime/sys_darwin_amd64.s | darwin | yes, named by a test |
| `runtime` | `syscall6` | `func syscall6()` sys_darwin.go:65 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `syscall6X` | `func syscall6X()` sys_darwin.go:91 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `syscall9` | `func syscall9()` sys_darwin.go:80 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `syscall_x509` | `func syscall_x509()` sys_darwin.go:147 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `syscallPtr` | `func syscallPtr()` sys_darwin.go:106 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `syscallX` | `func syscallX()` sys_darwin.go:43 | asm: runtime/sys_darwin_amd64.s | darwin | no |
| `runtime` | `sysctl_trampoline` | `func sysctl_trampoline()` sys_darwin.go:461 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `sysctlbyname_trampoline` | `func sysctlbyname_trampoline()` sys_darwin.go:473 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime` | `systemstack_switch` | `func systemstack_switch()` stubs.go:367 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `time_now` | `func time_now() (sec int64, nsec int32, mono int64)` timeasm.go:14 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | windows, linux | yes |
| `runtime` | `timer_create` | `func timer_create(clockid int32, sevp *sigevent, timerid *int32) int32` os_linux.go:438 | asm: runtime/sys_linux_amd64.s | linux | yes |
| `runtime` | `timer_delete` | `func timer_delete(timerid int32) int32` os_linux.go:444 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `timer_settime` | `func timer_settime(timerid int32, flags int32, new, old *itimerspec) int32` os_linux.go:441 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `tstart_stdcall` | `func tstart_stdcall(newm *m)` os_windows.go:158 | asm: runtime/sys_windows_amd64.s | windows | yes |
| `runtime` | `unspillArgs` | `func unspillArgs()` stubs_amd64.go:52 | asm: runtime/asm_amd64.s | all three | no |
| `runtime` | `usleep_trampoline` | `func usleep_trampoline()` sys_darwin.go:335 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `vgetrandom1` | `func vgetrandom1(buf *byte, length uintptr, flags uint32, state uintptr, stateSize uintptr) int` vgetrandom_linux.go:15 | asm: runtime/sys_linux_amd64.s | linux | no |
| `runtime` | `walltime_trampoline` | `func walltime_trampoline()` sys_darwin.go:399 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `runtime` | `wintls` | `func wintls()` os_windows.go:161 | asm: runtime/sys_windows_amd64.s | windows | no |
| `runtime` | `write_trampoline` | `func write_trampoline()` sys_darwin.go:350 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `runtime/pprof` | `mach_vm_region` | `func mach_vm_region(address, region_size *uint64, info unsafe.Pointer) int32` vminfo_darwin.go:73 | linkname: push exists in the corpus (runtime/darwin/sys_darwin.cs) and does not arrive | darwin | yes |
| `runtime/pprof` | `proc_regionfilename` | `func proc_regionfilename(pid int, address uint64, buf *byte, buflen int64) int32` vminfo_darwin.go:76 | linkname: push exists in the corpus (runtime/darwin/sys_darwin.cs) and does not arrive | darwin | yes |
| `syscall` | `libc_accept_trampoline` | `func libc_accept_trampoline()` zsyscall_darwin_amd64.go:67 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_access_trampoline` | `func libc_access_trampoline()` zsyscall_darwin_amd64.go:429 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_adjtime_trampoline` | `func libc_adjtime_trampoline()` zsyscall_darwin_amd64.go:443 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_bind_trampoline` | `func libc_bind_trampoline()` zsyscall_darwin_amd64.go:81 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_chdir_trampoline` | `func libc_chdir_trampoline()` zsyscall_darwin_amd64.go:462 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_chflags_trampoline` | `func libc_chflags_trampoline()` zsyscall_darwin_amd64.go:481 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_chmod_trampoline` | `func libc_chmod_trampoline()` zsyscall_darwin_amd64.go:500 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_chown_trampoline` | `func libc_chown_trampoline()` zsyscall_darwin_amd64.go:519 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_chroot_trampoline` | `func libc_chroot_trampoline()` zsyscall_darwin_amd64.go:538 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_close_trampoline` | `func libc_close_trampoline()` zsyscall_darwin_amd64.go:552 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_closedir_trampoline` | `func libc_closedir_trampoline()` zsyscall_darwin_amd64.go:566 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_connect_trampoline` | `func libc_connect_trampoline()` zsyscall_darwin_amd64.go:95 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_dup2_trampoline` | `func libc_dup2_trampoline()` zsyscall_darwin_amd64.go:595 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_dup_trampoline` | `func libc_dup_trampoline()` zsyscall_darwin_amd64.go:581 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_exchangedata_trampoline` | `func libc_exchangedata_trampoline()` zsyscall_darwin_amd64.go:619 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_execve_trampoline` | `func libc_execve_trampoline()` zsyscall_darwin_amd64.go:1796 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_exit_trampoline` | `func libc_exit_trampoline()` zsyscall_darwin_amd64.go:1810 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fchdir_trampoline` | `func libc_fchdir_trampoline()` zsyscall_darwin_amd64.go:633 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fchflags_trampoline` | `func libc_fchflags_trampoline()` zsyscall_darwin_amd64.go:647 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fchmod_trampoline` | `func libc_fchmod_trampoline()` zsyscall_darwin_amd64.go:661 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fchown_trampoline` | `func libc_fchown_trampoline()` zsyscall_darwin_amd64.go:675 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fcntl_trampoline` | `func libc_fcntl_trampoline()` zsyscall_darwin_amd64.go:328 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fdopendir_trampoline` | `func libc_fdopendir_trampoline()` syscall_darwin.go:243 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_flock_trampoline` | `func libc_flock_trampoline()` zsyscall_darwin_amd64.go:689 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fork_trampoline` | `func libc_fork_trampoline()` zsyscall_darwin_amd64.go:1782 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fpathconf_trampoline` | `func libc_fpathconf_trampoline()` zsyscall_darwin_amd64.go:704 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fstat64_trampoline` | `func libc_fstat64_trampoline()` zsyscall_darwin_amd64.go:1904 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fstatat64_trampoline` | `func libc_fstatat64_trampoline()` zsyscall_darwin_amd64.go:2008 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fstatfs64_trampoline` | `func libc_fstatfs64_trampoline()` zsyscall_darwin_amd64.go:1918 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_fsync_trampoline` | `func libc_fsync_trampoline()` zsyscall_darwin_amd64.go:718 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_ftruncate_trampoline` | `func libc_ftruncate_trampoline()` zsyscall_darwin_amd64.go:732 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_futimes_trampoline` | `func libc_futimes_trampoline()` zsyscall_darwin_amd64.go:313 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getcwd_trampoline` | `func libc_getcwd_trampoline()` zsyscall_darwin_amd64.go:1890 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_getdtablesize_trampoline` | `func libc_getdtablesize_trampoline()` zsyscall_darwin_amd64.go:744 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getegid_trampoline` | `func libc_getegid_trampoline()` zsyscall_darwin_amd64.go:756 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_geteuid_trampoline` | `func libc_geteuid_trampoline()` zsyscall_darwin_amd64.go:768 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getfsstat_trampoline` | `func libc_getfsstat_trampoline()` syscall_darwin.go:112 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getgid_trampoline` | `func libc_getgid_trampoline()` zsyscall_darwin_amd64.go:780 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getgroups_trampoline` | `func libc_getgroups_trampoline()` zsyscall_darwin_amd64.go:23 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getpeername_trampoline` | `func libc_getpeername_trampoline()` zsyscall_darwin_amd64.go:152 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_getpgid_trampoline` | `func libc_getpgid_trampoline()` zsyscall_darwin_amd64.go:795 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getpgrp_trampoline` | `func libc_getpgrp_trampoline()` zsyscall_darwin_amd64.go:807 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getpid_trampoline` | `func libc_getpid_trampoline()` zsyscall_darwin_amd64.go:819 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getppid_trampoline` | `func libc_getppid_trampoline()` zsyscall_darwin_amd64.go:831 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getpriority_trampoline` | `func libc_getpriority_trampoline()` zsyscall_darwin_amd64.go:846 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getrlimit_trampoline` | `func libc_getrlimit_trampoline()` zsyscall_darwin_amd64.go:860 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_getrusage_trampoline` | `func libc_getrusage_trampoline()` zsyscall_darwin_amd64.go:874 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getsid_trampoline` | `func libc_getsid_trampoline()` zsyscall_darwin_amd64.go:889 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_getsockname_trampoline` | `func libc_getsockname_trampoline()` zsyscall_darwin_amd64.go:166 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_getsockopt_trampoline` | `func libc_getsockopt_trampoline()` zsyscall_darwin_amd64.go:124 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_gettimeofday_trampoline` | `func libc_gettimeofday_trampoline()` zsyscall_darwin_amd64.go:1932 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_getuid_trampoline` | `func libc_getuid_trampoline()` zsyscall_darwin_amd64.go:901 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_ioctl_trampoline` | `func libc_ioctl_trampoline()` zsyscall_darwin_amd64.go:353 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_issetugid_trampoline` | `func libc_issetugid_trampoline()` zsyscall_darwin_amd64.go:913 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_kevent_trampoline` | `func libc_kevent_trampoline()` zsyscall_darwin_amd64.go:280 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_kill_trampoline` | `func libc_kill_trampoline()` zsyscall_darwin_amd64.go:410 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_kqueue_trampoline` | `func libc_kqueue_trampoline()` zsyscall_darwin_amd64.go:928 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_lchown_trampoline` | `func libc_lchown_trampoline()` zsyscall_darwin_amd64.go:947 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_link_trampoline` | `func libc_link_trampoline()` zsyscall_darwin_amd64.go:971 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_listen_trampoline` | `func libc_listen_trampoline()` zsyscall_darwin_amd64.go:985 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_lseek_trampoline` | `func libc_lseek_trampoline()` zsyscall_darwin_amd64.go:1368 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_lstat64_trampoline` | `func libc_lstat64_trampoline()` zsyscall_darwin_amd64.go:1951 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mkdir_trampoline` | `func libc_mkdir_trampoline()` zsyscall_darwin_amd64.go:1004 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mkfifo_trampoline` | `func libc_mkfifo_trampoline()` zsyscall_darwin_amd64.go:1023 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mknod_trampoline` | `func libc_mknod_trampoline()` zsyscall_darwin_amd64.go:1042 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mlock_trampoline` | `func libc_mlock_trampoline()` zsyscall_darwin_amd64.go:1062 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mlockall_trampoline` | `func libc_mlockall_trampoline()` zsyscall_darwin_amd64.go:1076 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_mmap_trampoline` | `func libc_mmap_trampoline()` zsyscall_darwin_amd64.go:1753 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_mprotect_trampoline` | `func libc_mprotect_trampoline()` zsyscall_darwin_amd64.go:1096 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_msync_trampoline` | `func libc_msync_trampoline()` zsyscall_darwin_amd64.go:1116 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_munlock_trampoline` | `func libc_munlock_trampoline()` zsyscall_darwin_amd64.go:1136 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_munlockall_trampoline` | `func libc_munlockall_trampoline()` zsyscall_darwin_amd64.go:1150 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_munmap_trampoline` | `func libc_munmap_trampoline()` zsyscall_darwin_amd64.go:1767 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_open_trampoline` | `func libc_open_trampoline()` zsyscall_darwin_amd64.go:1170 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_openat_trampoline` | `func libc_openat_trampoline()` zsyscall_darwin_amd64.go:1869 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_pathconf_trampoline` | `func libc_pathconf_trampoline()` zsyscall_darwin_amd64.go:1190 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_pipe_trampoline` | `func libc_pipe_trampoline()` zsyscall_darwin_amd64.go:377 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_pread_trampoline` | `func libc_pread_trampoline()` zsyscall_darwin_amd64.go:1211 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_ptrace_trampoline` | `func libc_ptrace_trampoline()` zsyscall_darwin_amd64.go:2026 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_pwrite_trampoline` | `func libc_pwrite_trampoline()` zsyscall_darwin_amd64.go:1232 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_read_trampoline` | `func libc_read_trampoline()` zsyscall_darwin_amd64.go:1253 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_readdir_r_trampoline` | `func libc_readdir_r_trampoline()` zsyscall_darwin_amd64.go:1265 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_readlink_trampoline` | `func libc_readlink_trampoline()` zsyscall_darwin_amd64.go:1291 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_recvfrom_trampoline` | `func libc_recvfrom_trampoline()` zsyscall_darwin_amd64.go:215 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_recvmsg_trampoline` | `func libc_recvmsg_trampoline()` zsyscall_darwin_amd64.go:250 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_rename_trampoline` | `func libc_rename_trampoline()` zsyscall_darwin_amd64.go:1315 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_revoke_trampoline` | `func libc_revoke_trampoline()` zsyscall_darwin_amd64.go:1334 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_rmdir_trampoline` | `func libc_rmdir_trampoline()` zsyscall_darwin_amd64.go:1353 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_select_trampoline` | `func libc_select_trampoline()` zsyscall_darwin_amd64.go:1382 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_sendfile_trampoline` | `func libc_sendfile_trampoline()` syscall_darwin_amd64.go:60 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_sendmsg_trampoline` | `func libc_sendmsg_trampoline()` zsyscall_darwin_amd64.go:265 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_sendto_trampoline` | `func libc_sendto_trampoline()` zsyscall_darwin_amd64.go:235 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_setegid_trampoline` | `func libc_setegid_trampoline()` zsyscall_darwin_amd64.go:1396 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_seteuid_trampoline` | `func libc_seteuid_trampoline()` zsyscall_darwin_amd64.go:1410 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setgid_trampoline` | `func libc_setgid_trampoline()` zsyscall_darwin_amd64.go:1424 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setgroups_trampoline` | `func libc_setgroups_trampoline()` zsyscall_darwin_amd64.go:37 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setlogin_trampoline` | `func libc_setlogin_trampoline()` zsyscall_darwin_amd64.go:1443 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setpgid_trampoline` | `func libc_setpgid_trampoline()` zsyscall_darwin_amd64.go:1457 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_setpriority_trampoline` | `func libc_setpriority_trampoline()` zsyscall_darwin_amd64.go:1471 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setprivexec_trampoline` | `func libc_setprivexec_trampoline()` zsyscall_darwin_amd64.go:1485 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setregid_trampoline` | `func libc_setregid_trampoline()` zsyscall_darwin_amd64.go:1499 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setreuid_trampoline` | `func libc_setreuid_trampoline()` zsyscall_darwin_amd64.go:1513 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setrlimit_trampoline` | `func libc_setrlimit_trampoline()` zsyscall_darwin_amd64.go:1527 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_setsid_trampoline` | `func libc_setsid_trampoline()` zsyscall_darwin_amd64.go:1542 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_setsockopt_trampoline` | `func libc_setsockopt_trampoline()` zsyscall_darwin_amd64.go:138 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_settimeofday_trampoline` | `func libc_settimeofday_trampoline()` zsyscall_darwin_amd64.go:1556 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_setuid_trampoline` | `func libc_setuid_trampoline()` zsyscall_darwin_amd64.go:1570 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_shutdown_trampoline` | `func libc_shutdown_trampoline()` zsyscall_darwin_amd64.go:180 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_socket_trampoline` | `func libc_socket_trampoline()` zsyscall_darwin_amd64.go:110 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_socketpair_trampoline` | `func libc_socketpair_trampoline()` zsyscall_darwin_amd64.go:194 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_stat64_trampoline` | `func libc_stat64_trampoline()` zsyscall_darwin_amd64.go:1970 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_statfs64_trampoline` | `func libc_statfs64_trampoline()` zsyscall_darwin_amd64.go:1989 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_symlink_trampoline` | `func libc_symlink_trampoline()` zsyscall_darwin_amd64.go:1594 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_sync_trampoline` | `func libc_sync_trampoline()` zsyscall_darwin_amd64.go:1608 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_sysctl_trampoline` | `func libc_sysctl_trampoline()` zsyscall_darwin_amd64.go:1830 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_truncate_trampoline` | `func libc_truncate_trampoline()` zsyscall_darwin_amd64.go:1627 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_umask_trampoline` | `func libc_umask_trampoline()` zsyscall_darwin_amd64.go:1639 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_undelete_trampoline` | `func libc_undelete_trampoline()` zsyscall_darwin_amd64.go:1658 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_unlink_trampoline` | `func libc_unlink_trampoline()` zsyscall_darwin_amd64.go:1677 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_unlinkat_trampoline` | `func libc_unlinkat_trampoline()` zsyscall_darwin_amd64.go:1849 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_unmount_trampoline` | `func libc_unmount_trampoline()` zsyscall_darwin_amd64.go:1696 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_utimensat_trampoline` | `func libc_utimensat_trampoline()` zsyscall_darwin_amd64.go:396 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_utimes_trampoline` | `func libc_utimes_trampoline()` zsyscall_darwin_amd64.go:299 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `libc_wait4_trampoline` | `func libc_wait4_trampoline()` zsyscall_darwin_amd64.go:52 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_write_trampoline` | `func libc_write_trampoline()` zsyscall_darwin_amd64.go:1717 | cgo: dynamic import (cgo or a libc trampoline) | darwin | yes |
| `syscall` | `libc_writev_trampoline` | `func libc_writev_trampoline()` zsyscall_darwin_amd64.go:1738 | cgo: dynamic import (cgo or a libc trampoline) | darwin | no |
| `syscall` | `rawVforkSyscall` | `func rawVforkSyscall(trap, a1, a2, a3 uintptr) (r1 uintptr, err Errno)` syscall_linux.go:102 | asm: syscall/asm_linux_amd64.s | linux | no |
| `syscall` | `runtime_AfterFork` | `func runtime_AfterFork()` exec_linux.go:124 | linkname: push exists in the corpus (runtime/linux/proc.cs) and does not arrive | linux, darwin | no |
| `syscall` | `runtime_AfterForkInChild` | `func runtime_AfterForkInChild()` exec_linux.go:125 | linkname: push exists in the corpus (runtime/linux/proc.cs) and does not arrive | linux, darwin | no |
| `syscall` | `runtime_BeforeFork` | `func runtime_BeforeFork()` exec_linux.go:123 | linkname: push exists in the corpus (runtime/linux/proc.cs) and does not arrive | linux, darwin | no |
| `vendor/golang.org/x/sys/cpu` | `cpuid` | `func cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)` cpu_gc_x86.go:11 | asm: vendor/golang.org/x/sys/cpu/cpu_gc_x86.s | all three | no tests |
| `vendor/golang.org/x/sys/cpu` | `runtime_getAuxv` | `func runtime_getAuxv() []uintptr` runtime_auxv_go121.go:14 | linkname: pull: the Go declaration names its producer, whose body does not reach this compilation | all three | no tests |
| `vendor/golang.org/x/sys/cpu` | `xgetbv` | `func xgetbv() (eax, edx uint32)` cpu_gc_x86.go:15 | asm: vendor/golang.org/x/sys/cpu/cpu_gc_x86.s | all three | no tests |

## Appendix A — the extension (uncommitted; save as `src/go2cs/zz_phase5_inventory_test.go`)

```go
// zz_phase5_inventory_test.go - the Phase 5 stub inventory (C2, 2026-10-10; COORD dispatch, owner request).
//
// An EXTENSION of the q82 census in declaredNotImplemented_test.go, kept out of the tree: it reuses that
// file's predicate (partialDeclRe, partialDefnRe, leadingTrivia, leadsABody, the [LibraryImport] and
// test-init-hook exclusions) and changes only the UNIT. The q82 census folds a package's per-GOOS flavours
// into one population; this evaluates each flavour as the compilation its csproj makes for
// -p:GoTargetOS=<goos> (the package root's .cs files plus <goos>/*.cs), separately for the PRODUCTION
// compile and the TEST compile (production sources recompiled with the converted *_test.cs files).
// It then classifies each member from the Go source at the pinned GOROOT and traces whether the
// package's own converted tests reach it.
//
//	PHASE5_OUT=<file.tsv> go test -run TestPhase5StubInventory -v .
package main

import (
	"bufio"
	"fmt"
	"go/ast"
	"go/build"
	"go/parser"
	"go/printer"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

type p5File struct {
	rel, flavour string
	test         bool
	lines        []string
}

type p5Row struct {
	goos, compile, pkg, file, name, marker, pushedBy string
	prodMentions                                     int
	testDirect, testReach                            string
	goDecl, goSig, class, classDetail                string
}

var (
	// A member declaration at class level: converted code puts class members at column 0.
	p5MethodRe = regexp.MustCompile(`^(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\s+(?:static\s+)?(?:unsafe\s+)?(?:partial\s+)?[^=;]*?([\pL_@][\pL\pN_@]*)\s*(?:<[^()]*>)?\s*\(`)
	p5FieldRe  = regexp.MustCompile(`^(?:public|internal|private|protected)\s+static\s+[^(]*\s=`)
	p5TokenRe  = regexp.MustCompile(`[\pL_@][\pL\pN_@]*`)
	// Test functions only: validation executes no Example, Benchmark or Fuzz declaration (each proof page
	// lists them as excluded, "deferred to Phase 4D").
	p5TestRoot        = regexp.MustCompile(`^Test`)
	p5PartialAssignRe = regexp.MustCompile(`^\s*partial\s*[-+*/]?=`)
)

func p5Walk(coreDir string) (map[string][]p5File, error) {
	files := map[string][]p5File{}

	err := filepath.Walk(coreDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			switch info.Name() {
			case "bin", "obj", "Generated", ".vs":
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(path, ".cs") || strings.HasSuffix(path, ".cs.auto") {
			return nil
		}
		content, readErr := os.ReadFile(path)
		if readErr != nil {
			return nil
		}
		rel := filepath.ToSlash(mustRel(coreDir, path))
		dir := filepath.ToSlash(filepath.Dir(rel))
		pkg, flavour := dir, ""
		switch filepath.Base(dir) {
		case "windows", "linux", "darwin":
			pkg, flavour = filepath.ToSlash(filepath.Dir(dir)), filepath.Base(dir)
		}
		base := filepath.Base(rel)
		test := strings.HasSuffix(base, "_test.cs") || base == "package_test_info.cs" || base == "go2cs_test_host.cs"
		files[pkg] = append(files[pkg], p5File{rel: rel, flavour: flavour, test: test,
			lines: strings.Split(strings.ReplaceAll(string(content), "\r\n", "\n"), "\n")})
		return nil
	})

	return files, err
}

// p5Compile is one compilation's file set: the package root's files plus <goos>/, test files only for the
// test compile.
func p5Compile(all []p5File, goos string, test bool) []p5File {
	var set []p5File
	for _, f := range all {
		if f.flavour != "" && f.flavour != goos {
			continue
		}
		if f.test && !test {
			continue
		}
		set = append(set, f)
	}
	return set
}

func TestPhase5StubInventory(t *testing.T) {
	out := os.Getenv("PHASE5_OUT")
	if out == "" {
		t.Skip("PHASE5_OUT is not set: this is a measurement, not a gate")
	}
	goroot := strings.TrimSpace(os.Getenv("PHASE5_GOROOT"))
	coreDir := filepath.Join("..", "core")

	files, err := p5Walk(coreDir)
	if err != nil {
		t.Fatal(err)
	}

	var rows []p5Row

	for _, goos := range []string{"windows", "linux", "darwin"} {
		// Pushes are proven per flavour: a //go:linkname whose local name leads a BODY, in a production
		// file of this flavour's compile of any package.
		pushes := map[string]string{}
		for _, pkgFiles := range files {
			for _, f := range p5Compile(pkgFiles, goos, false) {
				for i, line := range f.lines {
					if m := linknameRe.FindStringSubmatch(line); m != nil && m[2] != "" && leadsABody(f.lines, i) {
						if _, seen := pushes[m[2]]; !seen {
							pushes[m[2]] = f.rel
						}
					}
				}
			}
		}

		pkgs := make([]string, 0, len(files))
		for pkg := range files {
			pkgs = append(pkgs, pkg)
		}
		sort.Strings(pkgs)

		for _, pkg := range pkgs {
			hasTests := false
			if entries, err := os.ReadDir(filepath.Join(coreDir, pkg)); err == nil {
				for _, e := range entries {
					if strings.HasSuffix(e.Name(), ".tests.csproj") {
						hasTests = true
					}
				}
			}

			for _, compile := range []string{"prod", "test"} {
				set := p5Compile(files[pkg], goos, compile == "test")

				type decl struct{ name, file, marker string }
				var decls []decl
				defns := map[string]bool{}

				for _, f := range set {
					for i, line := range f.lines {
						// ⚠ q82's declaration pattern also matches an ASSIGNMENT to a local named `partial`
						// (`partial = totalUtilOf(...);`, internal/trace/gc.cs): excluded here, and reported.
						if m := partialDeclRe.FindStringSubmatch(line); m != nil && !strings.Contains(line, "=>") && !p5PartialAssignRe.MatchString(line) {
							comments, attributes := leadingTrivia(f.lines, i)
							if strings.Contains(attributes, "LibraryImport") || m[1] == PackageTestInitHookMethod {
								continue
							}
							marker := "other"
							switch {
							case linknameMark.MatchString(comments):
								marker = "linkname"
							case cgoMark.MatchString(comments):
								marker = "cgo"
							}
							decls = append(decls, decl{m[1], f.rel, marker})
							continue
						}
						if m := partialDefnRe.FindStringSubmatch(line); m != nil {
							defns[m[1]] = true
						}
					}
				}

				// The test compile reports only what it adds: a production stub is already a row.
				var stubs []decl
				for _, d := range decls {
					if defns[d.name] {
						continue
					}
					if compile == "test" && !strings.HasSuffix(d.file, "_test.cs") && !strings.HasSuffix(d.file, "package_test_info.cs") && !strings.HasSuffix(d.file, "go2cs_test_host.cs") {
						continue
					}
					stubs = append(stubs, d)
				}
				if len(stubs) == 0 {
					continue
				}

				reached, direct, mentions := p5Reach(files[pkg], goos, stubs2names(stubs, func(d decl) string { return d.name }))

				for _, d := range stubs {
					row := p5Row{goos: goos, compile: compile, pkg: pkg, file: d.file, name: d.name, marker: d.marker,
						pushedBy: pushes[pkg+"."+d.name], prodMentions: mentions[d.name]}
					switch {
					case !hasTests:
						row.testDirect, row.testReach = "-", "no tests"
					default:
						row.testDirect = map[bool]string{true: "yes", false: "no"}[direct[d.name]]
						row.testReach = map[bool]string{true: "yes", false: "no"}[reached[d.name]]
					}
					row.goDecl, row.goSig, row.class, row.classDetail = p5Classify(goroot, pkg, d.name, goos, d.marker, row.pushedBy, compile == "test")
					rows = append(rows, row)
				}
			}
		}
	}

	w, err := os.Create(out)
	if err != nil {
		t.Fatal(err)
	}
	defer w.Close()
	bw := bufio.NewWriter(w)
	fmt.Fprintln(bw, "goos\tcompile\tpkg\tfile\tmember\tmarker\tpushed_by\tprod_mentions\ttest_direct\ttest_reach\tgo_decl\tgo_sig\tclass\tclass_detail")
	for _, r := range rows {
		fmt.Fprintf(bw, "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%d\t%s\t%s\t%s\t%s\t%s\t%s\n", r.goos, r.compile, r.pkg, r.file, r.name,
			r.marker, r.pushedBy, r.prodMentions, r.testDirect, r.testReach, r.goDecl, r.goSig, r.class, r.classDetail)
	}
	bw.Flush()
	t.Logf("rows %d -> %s", len(rows), out)
}

func stubs2names[T any](items []T, name func(T) string) map[string]bool {
	m := map[string]bool{}
	for _, it := range items {
		m[name(it)] = true
	}
	return m
}

// p5Reach traces, over the package's own test compile for goos, a TEXTUAL call graph: every class-level
// member is a node, and a line inside it MENTIONS every identifier token on it (a call, a method group, a
// delegate), which over-approximates reach. Roots: every Test/Example/Benchmark/Fuzz member declared in a
// test file, every init member, and every static field initializer (they run when the class loads).
// It returns the stubs reached, the stubs a test file names directly, and the production mention count.
func p5Reach(all []p5File, goos string, stubs map[string]bool) (reached, direct map[string]bool, mentions map[string]int) {
	reached, direct, mentions = map[string]bool{}, map[string]bool{}, map[string]int{}
	edges := map[string]map[string]bool{}
	roots := map[string]bool{"<static-init>": true}

	for _, f := range p5Compile(all, goos, true) {
		current := "<static-init>"
		code := p5StripFile(f.lines)
		for li, line := range f.lines {
			trimmed := strings.TrimSpace(line)
			if strings.HasPrefix(trimmed, "//") {
				continue
			}
			if p5FieldRe.MatchString(line) {
				current = "<static-init>"
			} else if m := p5MethodRe.FindStringSubmatch(line); m != nil {
				current = m[1]
				if (f.test && p5TestRoot.MatchString(current)) || strings.HasPrefix(current, "init") {
					roots[current] = true
				}
				if partialDeclRe.MatchString(line) {
					continue
				}
			}
			if edges[current] == nil {
				edges[current] = map[string]bool{}
			}
			// Identifiers only: a string literal ("runtime.abort"u8) or a trailing comment names nothing.
			for _, tok := range p5TokenRe.FindAllString(code[li], -1) {
				if tok == current {
					continue
				}
				edges[current][tok] = true
				if stubs[tok] {
					if f.test {
						direct[tok] = true
					} else {
						mentions[tok]++
					}
				}
			}
		}
	}

	seen := map[string]bool{}
	queue := []string{}
	for r := range roots {
		queue = append(queue, r)
		seen[r] = true
	}
	for len(queue) > 0 {
		n := queue[0]
		queue = queue[1:]
		for next := range edges[n] {
			if stubs[next] {
				reached[next] = true
			}
			if !seen[next] {
				if _, isNode := edges[next]; isNode {
					seen[next] = true
					queue = append(queue, next)
				}
			}
		}
	}
	return
}

var p5Intrinsics map[string]bool

// p5StripFile blanks a file's comments and its string and char literals, multi-line raw ("""…""") and
// verbatim (@"…") strings included, keeping its line structure, so a reach reads identifiers only.
func p5StripFile(lines []string) []string {
	src := []rune(strings.Join(lines, "\n"))
	out := make([]rune, len(src))
	for i := range src {
		out[i] = ' '
		if src[i] == '\n' {
			out[i] = '\n'
		}
	}
	quotes := func(i int) int {
		n := 0
		for i+n < len(src) && src[i+n] == '"' {
			n++
		}
		return n
	}
	for i := 0; i < len(src); i++ {
		c := src[i]
		switch {
		case c == '/' && i+1 < len(src) && src[i+1] == '/':
			for i < len(src) && src[i] != '\n' {
				i++
			}
			i--
		case c == '/' && i+1 < len(src) && src[i+1] == '*':
			for i+1 < len(src) && !(src[i] == '*' && src[i+1] == '/') {
				i++
			}
			i++
		case c == '"' && quotes(i) >= 3:
			n := quotes(i)
			for i += n; i < len(src) && quotes(i) < n; i++ {
			}
			i += n - 1
		case c == '"' && i > 0 && src[i-1] == '@':
			for i++; i < len(src); i++ {
				if src[i] == '"' {
					if i+1 < len(src) && src[i+1] == '"' {
						i++
						continue
					}
					break
				}
			}
		case c == '"':
			for i++; i < len(src) && src[i] != '"' && src[i] != '\n'; i++ {
				if src[i] == '\\' {
					i++
				}
			}
		case c == '\'':
			for i++; i < len(src) && src[i] != '\'' && src[i] != '\n'; i++ {
				if src[i] == '\\' {
					i++
				}
			}
		default:
			out[i] = c
		}
	}
	return strings.Split(string(out), "\n")
}

type p5Asm struct{ text, mention map[string][]string }

var p5AsmCache = map[string]*p5Asm{}

// p5AsmIndex indexes, for goos/amd64, every assembly file Go's source selects: each TEXT definition keyed
// by its package (`·name` in a file of that directory) or by its written prefix (`runtime·name`, which
// another directory's file may define, as internal/bytealg defines runtime·memequal_varlen), and every
// `·name` mentioned in a file of a directory (a macro such as CALLFN(·call16, 16) defines through one).
func p5AsmIndex(goroot, goos string) *p5Asm {
	if idx := p5AsmCache[goos]; idx != nil {
		return idx
	}
	idx := &p5Asm{text: map[string][]string{}, mention: map[string][]string{}}
	ctx := build.Default
	ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = goos, "amd64", false
	root := filepath.Join(goroot, "src")
	// goos "*" indexes every assembly file regardless of its constraints.
	textRe := regexp.MustCompile(`(?m)^TEXT\s+([^\s(·]*)·([\pL\pN_]+)(?:<[^>]*>)?\(SB\)`)
	mentionRe := regexp.MustCompile(`·([\pL\pN_]+)\b`)
	filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return nil
		}
		if info.IsDir() {
			if n := info.Name(); n == "testdata" || (n == "cmd" && filepath.Dir(path) == root) {
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(path, ".s") {
			return nil
		}
		dir, base := filepath.Dir(path), filepath.Base(path)
		if ok, _ := ctx.MatchFile(dir, base); !ok && goos != "*" {
			return nil
		}
		data, err := os.ReadFile(path)
		if err != nil {
			return nil
		}
		relDir, _ := filepath.Rel(root, dir)
		pkg := filepath.ToSlash(relDir)
		file := filepath.ToSlash(filepath.Join(relDir, base))
		for _, m := range textRe.FindAllStringSubmatch(string(data), -1) {
			key := pkg + "." + m[2]
			if m[1] != "" {
				key = strings.ReplaceAll(m[1], "∕", "/") + "." + m[2]
			}
			idx.text[key] = append(idx.text[key], file)
		}
		for _, m := range mentionRe.FindAllStringSubmatch(string(data), -1) {
			key := pkg + "." + m[1]
			if n := len(idx.mention[key]); n == 0 || idx.mention[key][n-1] != file {
				idx.mention[key] = append(idx.mention[key], file)
			}
		}
		return nil
	})
	p5AsmCache[goos] = idx
	return idx
}

// p5GoPushes maps a push TARGET (pkg.name) to the Go file that pushes it: a two-argument //go:linkname in
// Go's own source whose local name leads a body. A consumer need carry no marker, so this is the only way
// to see a push whose producer the corpus does not body.
var p5GoPushes map[string]string

func p5LoadGoPushes(goroot string) {
	p5GoPushes = map[string]string{}
	re := regexp.MustCompile(`^//go:linkname\s+(\S+)\s+(\S+)\s*$`)
	root := filepath.Join(goroot, "src")
	filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return nil
		}
		if info.IsDir() {
			if n := info.Name(); n == "testdata" || (n == "cmd" && filepath.Dir(path) == root) {
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(path, ".go") || strings.HasSuffix(path, "_test.go") {
			return nil
		}
		data, err := os.ReadFile(path)
		if err != nil {
			return nil
		}
		lines := strings.Split(string(data), "\n")
		for i, line := range lines {
			m := re.FindStringSubmatch(strings.TrimSpace(line))
			if m == nil {
				continue
			}
			// The body discriminator, in Go: the next func declaration names the local and opens a brace.
			for j := i + 1; j < len(lines) && j <= i+8; j++ {
				l := strings.TrimSpace(lines[j])
				if l == "" || strings.HasPrefix(l, "//") {
					continue
				}
				if strings.HasPrefix(l, "func ") && strings.Contains(l, " "+m[1]+"(") && strings.HasSuffix(l, "{") {
					if _, seen := p5GoPushes[m[2]]; !seen {
						rel, _ := filepath.Rel(root, path)
						p5GoPushes[m[2]] = filepath.ToSlash(rel)
					}
				}
				break
			}
		}
		return nil
	})
}

// p5Classify reads the member's Go declaration at the pinned GOROOT and names its source class.
func p5Classify(goroot, pkg, csName, goos, marker, pushedBy string, test bool) (decl, sig, class, detail string) {
	name := strings.TrimPrefix(csName, "@")
	dir := filepath.Join(goroot, "src", filepath.FromSlash(pkg))
	ctx := build.Default
	ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = goos, "amd64", false

	if p5GoPushes == nil {
		p5LoadGoPushes(goroot)
	}
	if p5Intrinsics == nil {
		p5Intrinsics = map[string]bool{}
		if data, err := os.ReadFile(filepath.Join(goroot, "src", "cmd", "compile", "internal", "ssagen", "intrinsics.go")); err == nil {
			for _, m := range regexp.MustCompile(`"([a-z0-9/_]+)",\s*"([A-Za-z0-9_]+)"`).FindAllStringSubmatch(string(data), -1) {
				p5Intrinsics[m[1]+"."+m[2]] = true
			}
		}
	}

	entries, err := os.ReadDir(dir)
	if err != nil {
		return "-", "-", "unresolved", "no Go package directory at the pin"
	}

	fset := token.NewFileSet()
	var found *ast.FuncDecl
	var foundFile string
	var foundMatch bool
	var linknamed bool
	for _, e := range entries {
		n := e.Name()
		if !strings.HasSuffix(n, ".go") || (strings.HasSuffix(n, "_test.go") && !test) {
			continue
		}
		f, err := parser.ParseFile(fset, filepath.Join(dir, n), nil, parser.ParseComments)
		if err != nil {
			continue
		}
		match, _ := ctx.MatchFile(dir, n)
		for _, d := range f.Decls {
			fd, ok := d.(*ast.FuncDecl)
			if !ok || fd.Name.Name != name || fd.Body != nil {
				continue
			}
			if found == nil || (match && !foundMatch) {
				found, foundFile, foundMatch = fd, n, match
				linknamed = false
				for _, cg := range f.Comments {
					for _, c := range cg.List {
						if strings.HasPrefix(c.Text, "//go:linkname "+name+" ") || c.Text == "//go:linkname "+name {
							linknamed = true
						}
					}
				}
			}
		}
	}
	if found == nil {
		return "-", "-", "unresolved", "no bodyless Go declaration of this name at the pin"
	}

	var b strings.Builder
	printer.Fprint(&b, token.NewFileSet(), &ast.FuncDecl{Recv: found.Recv, Name: found.Name, Type: found.Type})
	sig = strings.ReplaceAll(b.String(), "\n", " ")
	decl = fmt.Sprintf("%s:%d", foundFile, fset.Position(found.Pos()).Line)
	if !foundMatch {
		decl += " (file not selected for " + goos + "/amd64)"
	}

	asm := p5AsmIndex(goroot, goos)
	last := pkg[strings.LastIndex(pkg, "/")+1:]
	var asmFiles []string
	asmFiles = append(asmFiles, asm.text[pkg+"."+name]...)
	if len(asmFiles) == 0 {
		asmFiles = append(asmFiles, asm.text[last+"."+name]...)
	}
	macro := asm.mention[pkg+"."+name]

	switch {
	case pushedBy != "":
		return decl, sig, "linkname", "push exists in the corpus (" + pushedBy + ") and does not arrive"
	case p5GoPushes[pkg+"."+name] != "":
		return decl, sig, "linkname", "push exists in Go (" + p5GoPushes[pkg+"."+name] + "); its producer has no body in the corpus"
	case linknamed || marker == "linkname":
		return decl, sig, "linkname", "pull: the Go declaration names its producer, whose body does not reach this compilation"
	case marker == "cgo" || strings.Contains(name, "trampoline") || strings.HasPrefix(name, "libc_"):
		return decl, sig, "cgo", "dynamic import (cgo or a libc trampoline)"
	case len(asmFiles) > 0:
		return decl, sig, "asm", strings.Join(asmFiles, ",")
	case len(macro) > 0:
		return decl, sig, "asm", "defined by an assembly macro in " + strings.Join(macro, ",")
	case p5Intrinsics[pkg+"."+name]:
		return decl, sig, "intrinsic", "compiler intrinsic (cmd/compile/internal/ssagen/intrinsics.go)"
	default:
		// Not amd64 assembly, not an intrinsic: assembly for another arch only, or no definition at all.
		any := p5AsmIndex(goroot, "*")
		if files := append(any.text[pkg+"."+name], any.text[last+"."+name]...); len(files) > 0 {
			return decl, sig, "asm", "assembly for another arch only (" + strings.Join(files, ",") + "); no amd64 body"
		}
		return decl, sig, "vestigial", "no definition anywhere in GOROOT/src at the pin (declared for vet or for compiled code)"
	}
}
```

