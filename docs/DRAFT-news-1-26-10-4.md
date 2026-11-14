# DRAFT — the go.* 1.26.10.4 release notes

<!-- DRAFT for owner and COORD review, started 2026-11-11 by seat C43 (P2) from master 50dd9a065c, on the .3 precedent
     (docs/DRAFT-news-1-26-10-3.md, b3ca4ef593). Nothing here is published. At release the entries below move to the top
     of docs/NEWS.md (dated), and the same text is the GitHub Release body. Each item names the committed record its
     facts are read from in an HTML comment beside it. -->

---

## <DATE>, 2026 — <TITLE>

<!-- One section per item, each owned by the seat that writes it, so the seats' merges touch separate lines (COORD,
     2026-11-11). At release the sections are merged into the entry's prose. -->

### Converting a module: default build tags (C43)

**Converting a module picks a library's pure-Go code over its assembly.** `go2cs -recurse`, in every form (local
references, `-recurse=module` and `-recurse=nuget`), now builds a module with the build tags `safe`, `purego` and
`noasm`, where it used `safe` alone, and prints them at the start of the run. A library that ships hand-written
assembly behind `!purego` or `!noasm` now converts its pure-Go version instead of methods that throw when called.
github.com/klauspost/cpuid/v2 is the case that prompted it: it ran its assembly CPU detection as the package
loaded, so every converted program that imported it, github.com/zeebo/xxh3 among them, stopped at startup. With
`noasm` it uses its pure-Go detection. An explicit `-tags` still replaces the default entirely, and
`-module-safe-tag=false` drops all three tags. None of the nine published nugetgo packages selects a different
file under the new tags, so none needs a rebuild for this change. See
[Build tags for a module](ConvertingAModule.md#build-tags-for-a-module).

A known limit: no tag removes assembly that a library gates on the architecture alone. Its declarations still
convert to methods that throw, and they stay reachable through code that forces the fast path on. xxh3's
vector routines are the case: hashing takes the portable path, because the converted cpuid reports no CPU
features, but xxh3's own tests switch the vector paths on and reach the throwing methods.
<!-- seat C43 (COORD, 2026-11-11), branch claude/p2-c43-recurse-tags. The default: src/go2cs/commandLineOptions.go
     defaultModuleBuildTags. The nine-package census, at both tag sets on windows/amd64, linux/amd64, linux/arm64,
     darwin/amd64 and darwin/arm64, and the cpuid/xxh3 control: docs/phase4/DATA-c43-recurse-tags.md. -->

### `main` keeps its name, and runs with Go's stack (C46)

**A converted program's `main` now has the stack Go gives it.** Go's main goroutine grows its stack to 1 GB, so
deep recursion in `main` is ordinary Go. A converted program used to run `main` on the process's startup thread,
whose stack is the operating system's default (1 MiB on Windows, usually 8 MiB on linux and macOS), and a deep
recursion there ended the process with `Stack overflow.` where Go printed its result. `main` now runs as the main
goroutine on a thread that reserves 1 GiB. The converted-test host runs `TestMain` the same way, as Go runs it on
the main goroutine. The cost is one extra thread at start-up: under half a millisecond on linux and Windows, and a
few milliseconds on macOS hosted runners, where thread creation is slower and more variable.

**Visible change: `func main` keeps its name in converted code.** It used to become C#'s `Main`; it is now
`main`, like every other Go function, and the C# entry point `Main` is generated beside it. Every converted
program's `main.cs` changes in that one line when it is converted again. A program converted by an earlier
release still builds: the generator adds nothing when a `Main` is already declared. One Go rule wins over the
stack: when an `init` function calls `runtime.LockOSThread`, `main` runs on the startup thread, as Go runs it
there, and keeps that thread's stack. The main package's own `init` functions also still run on the startup
thread. See [Limitations](Limitations.md).
<!-- seat C46 (COORD ruling 2026-11-11, option A), branch claude/c1-c46-main-stack. Design and measurements:
     docs/ConversionStrategies-Reference/package-conversion.md, "`main` keeps its name, and the entry point is
     generated"; start-up probe runs 42987722709, 42987726907, 42987730438. Confirmed on typescript-go: the two parse
     units that overflowed at 1 MB (binderBinaryExpressionStress.ts/Js) exit 0 with dumps byte-identical to native. -->

### Some code instead of none: `go2cs -warn` (R)

**Some code instead of none: `go2cs -warn`.** A package that holds a site the converter refuses used to produce no
code at all. `-warn <code>` now downgrades the listed refusal to a warning, so the package converts and everything but
the refused site runs. It is opt-in per code and never a default, and a code is allowed only when every site it covers
fails loud when reached: the converted program panics there with the refusal text, rather than running on with a
silently wrong result. Today that is `GO2CS2002`, a generic instantiation whose self-referential constraint has a
method that cannot be forwarded (for example `Tree.Children` returning `[]N`). Each downgraded site is printed as
`warning GO2CS2002`, and every C# build of the project reports it again as the new `GO2CS0011`, so a project that is
never converted again still shows that it carries a known-broken path. See [`-warn`](CommandLine.md) and
[diagnostic codes](DiagnosticCodes.md).
<!-- the -warn seat (owner's .4, COORD 2026-11-10/11), branch claude/r-warn-option: src/go2cs/warnOption.go (the
     allow-list), src/core/golib/GoRefusalDowngradedAttribute.cs, src/gen/go2cs-gen/RefusalDowngradedAnalyzer.cs,
     behavioral RefusalDowngradeProxy. -->

### `-recurse` converts the rest of a module when one package is refused (R)

**`-recurse` converts the rest of a module when one package is refused.** One package the converter refused (a
`GO2CS2001`, `GO2CS2002`, `GO2CS2003`, `GO2CS2005` or `GO2CS2006` error) used to end a `-recurse` run, so every package queued after it
went unconverted. Now the refused package is skipped: its diagnostics still print as errors and it gets no project,
but every other package converts. The run still ends with exit 1, naming each refused package and each converted
package that imports one, since those will not build until the refused package converts. A package that fails to
convert (it does not load, or the converter panics) was already skipped, but the run used to exit 0; it now exits 1
too, so a script that checks the exit code sees it. A single-package conversion still stops at a refusal and writes
nothing. See [`-recurse`](CommandLine.md).
<!-- COORD rulings 20261111T053529Z and 20261111T060642Z, branch claude/r-warn-option: src/go2cs/recurseRefusal.go,
     moduleConverter.go convertAll / ConvertModule; the caller census in R's post 20261111T062125Z. -->

### A refusal is renumbered: `GO2CS2005` (R)

**A refusal is renumbered: `GO2CS2005`.** The refusals for a pointer type argument to a generic with a
non-self-referential method-set constraint (new in 1.26.10.4, alongside the conversions that now work for that shape)
are reported as `GO2CS2005` instead of `GO2CS2002`. Their sites cannot fail loud if downgraded, so `-warn` does not
accept `GO2CS2005`. A build script that matched `GO2CS2002` for those sites should match `GO2CS2005`.
<!-- branch claude/r-warn-option: src/go2cs/diagnosticOutput.go diagnosticNonSelfProxyRefused; docs/DiagnosticCodes.md. -->

### Standard-library assemblies are named after their packages (R)

**Package go.fmt now ships go.fmt.dll.** Every converted standard-library assembly is named after its NuGet package
ID: `go.fmt.dll`, `go.net.dll`, `go.crypto.tls.dll` (and `go.go.ast.dll` for package `go.go.ast`), where 1.26.10.3
and earlier shipped `fmt.dll`, `net.dll` and `crypto.tls.dll`. Short names like `net.dll` and `log.dll` could clash
with another library's in a consumer's bin folder. The package IDs themselves do not change, nor do golib
(`golib.dll`, package go.lib), the go2cs-gen analyzer, or a converted module's own assemblies.

**Upgrading.** A library converted and built against go.* 1.26.10.3 or earlier binds the standard library's
assemblies by their OLD names, so it must be rebuilt against 1.26.10.4. The break floor already names any such
package at build time (`GO2CS3002`) rather than letting it fail as a missing assembly at run time.
<!-- OQ4 (docs/PLAN-public-surface-stability.md, ruled), branch claude/r-oq4-assembly-rename:
     src/go2cs/assemblyIdentity.go; the design docs/phase4/DESIGN-assembly-identity.md and its census. -->

### A new refusal: `GO2CS2006` (C2)

**A new refusal: `GO2CS2006`.** A generic call that passes a `[]*T` where the callee declares `[]N` (`N` constrained
by a method-set interface) is converted by copying the slice element by element, so a callee that writes the slice
(assigns an element, appends into it, sorts it, or hands it to something that does) wrote the copy, and the caller
never saw the change, with no diagnostic. go2cs now refuses such a call by name, naming the callee's first write, and
converts it as before when the callee only reads the slice. No standard-library package is refused. `-warn` does not
accept `GO2CS2006`, since a downgraded site would run on silently wrong. See [diagnostic codes](DiagnosticCodes.md).
<!-- branch claude/c2-widening-write-refusal: src/go2cs/sliceWideningWriteRefusals.go; docs/DiagnosticCodes.md;
     the design record on claude/c2-widening-write-design (std census: 8 sites per target, 0 refused). -->

### golib: `GoCgoLibraries.Register` gains a target (H)

**A source-level note for anyone who calls `GoCgoLibraries.Register` by hand.** The method gains an overload that takes
the target its bindings were converted for, `Register(assembly, "linux/amd64", ...)`, and the original
`Register(assembly, ...)` stays, so compiled code is unaffected. One call no longer compiles: `Register(assembly, null)`
is ambiguous between the two overloads (CS0121). Converted code never writes it; the converter emits every call.
<!-- docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20 (increment 5b; the review's N3). -->

### cgo: C strings, `C.malloc` and `C.free`, `void *`, and `defer C.f(…)` (H)

**cgo converts more of what real programs write.** Under `-cgo`, `C.CString`, `C.CBytes`, `C.GoString`,
`C.GoStringN`, `C.GoBytes`, `C.malloc` and `C.free` now convert, a `void *` crosses when it is nil or points at C
memory, and `defer C.f(…)` converts. The helpers copy bytes, with no text encoding in between, and what `C.CString`
returns comes from the package's own `malloc`, so the package's own `C.free` frees it. go2cs still hands C no pointer
into Go memory: a pointer into a slice or an array is refused when the package is converted, and `unsafe.Pointer(&x)`
is refused when the call runs, each by name.

**One known memory-safety gap.** Do not pass C a pointer you rebuilt from a number when it points into Go memory:
`unsafe.Pointer(uintptr(unsafe.Pointer(p)) + n)`, or a `uintptr` kept in a variable and converted back. go2cs cannot
yet tell that number from an address in C memory, so the call goes ahead, and the memory may move while C is using
it. Pass `unsafe.Pointer(p)` itself, which go2cs refuses by name. See [Limitations](Limitations.md).
<!-- docs/phase4/DESIGN-cgo-phase3-marshaling.md §16 (increment 3.1) and §16.9 (the residue, COORD's ruling C). cgo
     Phase 3 is a STRETCH for this release by the owner's ruling: this section ships with the release that carries
     increment 3.1, and moves to that release's notes if it is not this one. -->

### A func stored through an `unsafe.Pointer` (P5)

**`*(*F)(e) = fn`, a func stored through an `unsafe.Pointer`, now stores what the target can hold.** Code translated
from C by ccgo fills a table of C function pointers this way, at a computed address
(`*(*func(…) int32)(unsafe.Pointer(uintptr(unsafe.Pointer(&table)) + 8)) = fn`). The store wrote a managed
reference into the table's memory before, and the number read back from the table named no func. It now stores the
func's word where the target is a word, and the func itself where the target is a func variable or a func field.
A target that is neither panics by name. The standard library has no such store. See
[Pointers](ConversionStrategies-Reference/pointers.md#a-func-value-carried-as-a-number--one-word-per-static-function-through-gofunccookie).
<!-- docs/phase4/DESIGN-interior-pointer-root.md 4.8 B1 and 10.4; golib's unsafe.Pointer.StoreFunc. Arms: the
     behavioral programs FuncWordShapes (the w3 rows) and InteriorPointerShapes (shape 1), GolibTests
     FuncStoreThroughPointerTests, the converter's TestFuncWordShapes. CENSUS, read 2026-11-14 at go1.26.10, tests
     included: 0 such stores in the standard library on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64;
     9 in modernc.org/sqlite v1.62.0 (vfs/patches64.go:18-26), all of top-level functions. -->

### `unsafe.Add` over a nil pointer answers the offset (P5)

**`unsafe.Add` over a nil pointer now answers the offset, as Go does.** `unsafe.Add(nil, n)` is the address `n`; it
answered nil before. Pointer arithmetic written in one expression, `unsafe.Pointer(uintptr(p) + n)`, converts to the
same call, so it gives the same answer over a nil `p`. A program that compared such a result with nil sees a non-nil
pointer now. No call site in the standard library does. See
[Pointers](ConversionStrategies-Reference/pointers.md#unsafepointeruintptrp--n-is-emitted-as-unsafeaddp-n--the-moved-pointer-keeps-what-p-carries).
<!-- docs/phase4/DESIGN-interior-pointer-root.md 10.7 (COORD's ruling of 2026-11-14 07:52Z); the arm is GolibTests
     InteriorPointerRootTests.AddOverANilPointerAnswersTheOffset, and the behavioral program UintptrUnsafePointerIdiom
     prints the same under Go. CENSUS, read 2026-11-14 at go1.26.10 (non-test sources outside cmd): 17 call sites of
     unsafe.Add. syscall/env_windows.go:88, 93; runtime/mgcmark_greenteagc.go:753, list.go:58, list_manual.go:65,
     malloc.go:1181; reflect/type.go:1205; internal/syscall/execenv/execenv_windows.go:37, 39, 44;
     internal/runtime/maps/group.go:131, 133, 139, 142; internal/runtime/gc/scan/scan_reference.go:32, scan_go.go:37,
     81. None passes a base that its own code allows to be nil together with an offset that is not 0: runtime/list.go
     and list_manual.go test `p == nil` before the call, and the others add to the address of a variable, a receiver
     or an allocation. -->
