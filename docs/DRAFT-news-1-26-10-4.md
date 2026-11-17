# DRAFT — the go.* 1.26.11.1 release notes

<!-- 2026-11-13 (R, the 1.26.11 patch hop): the release this draft was started for as 1.26.10.4 ships as 1.26.11.1, built
     from Go 1.26.11 (the owner's ruling, relayed by COORD 2026-11-12). The file keeps its name because records cite it by
     path; the release it names below is 1.26.11.1. -->

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
non-self-referential method-set constraint (new in 1.26.11.1, alongside the conversions that now work for that shape)
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
assemblies by their OLD names, so it must be rebuilt against 1.26.11.1. The break floor already names any such
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

<!-- 2026-11-14 (H): the section "golib: `GoCgoLibraries.Register` gains a target (H)" is DROPPED, as R's note below
     suggests. GoCgoLibraries is new in this release (816009e491, batch 284; golib at nuget-1.26.10.3 has no such
     class), so an overload added to it changes nothing anyone compiled against, and the converter emits every call
     to it. The class is EditorBrowsable(Never). Its text was: "The method gains an overload that takes the target
     its bindings were converted for, Register(assembly, "linux/amd64", ...), and the original Register(assembly,
     ...) stays, so compiled code is unaffected. One call no longer compiles: Register(assembly, null) is ambiguous
     between the two overloads (CS0121). Converted code never writes it; the converter emits every call."
     Its provenance: docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20 (increment 5b; the review's N3). -->
<!-- 2026-11-14 (R, the release-notes audit), for H: GoCgoLibraries is new in this release. It was added on the
     branch merged as 816009e491 (batch 284), and golib at nuget-1.26.10.3 has no such class, so no compiled code
     can be "unaffected" by an overload of it. Suggest folding the CS0121 note into "cgo: `go2cs -cgo` converts a
     package that calls C" below, or dropping this section. -->

### cgo: C strings, `C.malloc` and `C.free`, `void *`, and `defer C.f(…)` (H)

**cgo converts more of what real programs write.** Under `-cgo`, `C.CString`, `C.CBytes`, `C.GoString`,
`C.GoStringN`, `C.GoBytes`, `C.malloc` and `C.free` now convert, a `void *` crosses when it is nil or points at C
memory, and `defer C.f(…)` converts. The helpers copy bytes, with no text encoding in between, and what `C.CString`
returns comes from the package's own `malloc`, so the package's own `C.free` frees it. go2cs still hands C no pointer
into Go memory: a pointer into a slice or an array is refused when the package is converted, and `unsafe.Pointer(&x)`
is refused when the call runs, each by name. Each of these was run against the Go build of the same program on
linux/amd64, darwin/amd64, darwin/arm64 and windows/amd64. Every arm printed what Go printed, or, where it hands C Go
memory, was refused by name. A `-cgo` conversion is one package, and it builds against a go2cs clone
(`dotnet build -p:go2csPath=<go2cs>/src/`); it does not use the `go.*` packages yet. Building a cgo package against
the `go.*` packages from the feed is planned for the next release.
<!-- src/go2cs/main.go:305 (`Not combinable with -stdlib, -tests or -recurse`); src/go2cs/csproj-template.xml:135 (`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj"`) and src/go2cs/csproj-template.xml:187 (`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`). COORD's ruling O2, 2026-11-14 (cgo 3.1's release-path seat). -->

**A pointer rebuilt from a number is refused too, with a narrow memory-safety gap left.** go2cs refuses, by name,
any address inside Go memory that it is holding still, even after the address has been through a `uintptr`:
`u := uintptr(unsafe.Pointer(p))` followed by `C.f(unsafe.Pointer(u + n))`, the reversed sum
`unsafe.Pointer(n + uintptr(unsafe.Pointer(p)))`, and Go's own one-expression form, which converts to `unsafe.Add`.
Three cases still cross, because go2cs is no longer holding the memory, or never was. In each, the call goes ahead,
and the memory may move while C is using it: a number kept after the last use of the pointer it came from, which Go's
own rules already forbid; a number taken inside a method with a pointer receiver, from a pointer of the receiver's own
type; and Go memory held still before the program's first cgo package was loaded. A program that needs this
memory-safety guarantee should avoid those three. See [Limitations](Limitations.md#cgo).
<!-- 2026-11-15 (H): narrowed by the pin-extent seat (docs/phase4/DESIGN-cgo-pin-extents.md, section 11) to the three
     rows its section 4 leaves open: 7 (a released pin), 8 (a number minted with no pin) and 11 (an early pin). The
     arms are named in the Limitations entry's notes. The text it replaces said that go2cs "cannot yet tell such a
     number from an address in C memory", for every number held in a variable. -->

<!-- docs/phase4/DESIGN-cgo-phase3-marshaling.md §16 (increment 3.1) and §16.9 (the residue, COORD's ruling C). cgo
     Phase 3 is a STRETCH for this release by the owner's ruling: this section ships with the release that carries
     increment 3.1, and moves to that release's notes if it is not this one.
     2026-11-14 (H), answering R's note below: CONFIRMED, the stretch condition is met. Increment 3.1 is on master
     (43ac9e33fa, batch 305), and its re-gate merge 932ed46c19 is seated in batch 306, so this section ships with this
     release. The four targets are the hosted cgo-e2e legs 43638330006 (linux-x64), 43638338397 (win-x64) and
     43638346986 (osx-arm64 and osx-x64), 50 of 50 arms each, at 54b84f9ff7.
     The gap example is RE-READ, as R asked. The one-expression form is emitted as unsafe.Add on master
     (src/go2cs/pointerArithmetic.go; TestPointerArithmeticFormEmitsUnsafeAdd's "sum" arm). unsafe.Add's result keeps
     the box it was given, so the door refuses it, as the goadd and goaddlocal arms read on all four legs. That
     test's controls "held" (u + n) and "reversed" (n + uintptr(p)) stay numbers, which is the gap as now worded.
     The residue's design is docs/phase4/DESIGN-cgo-pin-extents.md (claude/h-cgo-pin-registry). -->
<!-- 2026-11-14 (R, the release-notes audit), for H and COORD: increment 3.1 is on master. 43ac9e33fa ("cgo Phase 3
     increment 3.1: the helpers, void * and the deferred call (GREEN)") came in with batch 305's f4b803dfd8, and
     docs/Limitations.md already describes the helpers. The stretch condition reads as met: confirm, and drop the
     comment above. The "known memory-safety gap" example (the one-expression form) may also read differently after
     the interior-pointer root, which converts that form to unsafe.Add: worth H's re-read. -->

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

### A slice taken through an array-pointer conversion is a view of the memory (P5)

**`(*[N]T)(p)[lo:hi]` over an `unsafe.Pointer` is now a view of the memory `p` names, as in Go.** The slice was a
copy before, so a write through it never reached the memory. Code translated from C reaches memory this way
throughout: modernc.org/libc's `memcpy` is `copy((*RawMem)(unsafe.Pointer(dest))[:n:n], …)`, and with a copy as its
destination it copied nothing. The converted code now calls `@unsafe.Pointer.View`, and `copy` over an open form
(`copy((*RawMem)(p)[:], src)`) calls `@unsafe.Pointer.CopyInto` or `CopyFrom`. A view of C memory aliases it. So
does a view of a Go array or slice of the same element type, and a view of any other Go value that holds no
reference (a number, a struct of numbers, an array of another element type): the value is pinned and the view
keeps it in place. A Go value that holds a reference is still viewed as a copy. What a program can see change:

- a write through such a slice reaches the memory, and a later write to the memory shows through the slice;
- a two-index view has Go's capacity, the array type's length less the low bound, where it had its own length;
- a view or a copy that would pass the end of the Go array or the Go value it points into panics by name, where
  it read whatever lay past it;
- a length too large for a slice panics by name, where its low 32 bits were used.

In the standard library 13 lines in four files of `runtime` take the new form. See
[Pointers](ConversionStrategies-Reference/pointers.md#a-slice-taken-through-ntp-aliases-the-memory-p-names-the-view-doors).
<!-- docs/phase4/DESIGN-array-pointer-doors.md, sections 4, 10 and 11; golib's unsafe.Pointer.View, CopyInto and
     CopyFrom. Arms: the behavioral program ArrayPointerViewDoors (linux; native pages from syscall.Mmap and Go memory)
     and the goldens of PointerArrayViewForms and PointerCastSliceReinterpret, GolibTests ArrayPointerViewDoorTests,
     the converter's TestArrayPointerViewDoor, repoguard's TestArrayPointerViewDoors. The pinned case (a Go value
     of another type): design section 12, golib's IHeldStorage, GolibTests' stress arm and its control, 10,000
     rounds, 2026-11-15. FOOTPRINT, two-seeded, four
     targets, 2026-11-15 at go1.26.11: runtime/heapdump.cs 5 lines, iface.cs 2, mbitmap.cs 2,
     runtime/windows/os_windows.cs 4; the hand-owned internal/syscall/windows/registry value.cs is respelled by
     hand, 8 lines. -->

**An index, an element's address or the whole array through `(*A)(p)` reaches the memory as well.** `(*A)(p)[i]`
(a read, a store, a compound assignment or `++`), `&(*A)(p)[i]`, `*(*A)(p)` and `*(*A)(p) = v` now go to the
memory `p` names without making a box of the whole array, so an indexed loop over C memory allocates nothing.
modernc.org/libc writes a string's terminator this way (`(*RawMem)(unsafe.Pointer(p))[len(wd)] = 0`), and sqlite
indexes small unnamed arrays the same way. In the standard library 45 lines take the new form, nearly all of them in
`runtime`'s memory allocator. A named array type that holds references, read as a whole value from memory that cannot hold it, now panics by
name at that read. It used to store a managed reference into that memory, and nothing reported it. See
[Pointers](ConversionStrategies-Reference/pointers.md#an-index-an-elements-address-or-a-whole-array-through-ntp-reaches-the-memory-p-names-the-element-doors).
<!-- Door B: merge 0cbd1eae70 (P5, claude/p5-array-door-b 6946dcc33e): Element, ElementRef, LoadArray and StoreArray on
     @unsafe.Pointer; "0 B over C memory" (GolibTests ArrayPointerElementDoorTests.AnIndexedLoopOverNativeMemoryAllocatesNothing);
     regen 45 lines in 9 files (runtime malloc_generated.cs 30, the four per-target malloc.cs and malloc_stubs.cs 2
     each, alg.cs 1, lock_spinbit.cs 2, internal/runtime/maps runtime_faststr.cs 2; git diff --stat of the merge). The floor: merge a9dc1336a2 (P5,
     claude/p5-named-array-floor 4c8ee1cbd2): "a native box of a named-array wrapper (reference-holding, e.g. libc RawMem)
     is refused BY NAME at its first whole-value READ (ValueAddress), never at mint ... the silent managed-reference store
     into C or Go memory (arm M0) is gone". -->

### The bytes of one field through `(*[N]T)(unsafe.Pointer(&s.f))` (P5)

**A view of one field's bytes over an ordinary Go struct now reads and writes the field, as in Go.** Go code reads
the two bytes of a port, or the halves of a 64-bit word, with `(*[2]byte)(unsafe.Pointer(&h.port))[1]`. Over a Go
struct holding no reference, only `len` of such a view was right: an index panicked with "index out of range with
length 0" or a nil dereference, and a `range` by value over it read zeros where Go read the field, with no error.
Now the view indexes the field in place (a read, a store, a compound assignment and `++`), and the whole array
through it works as Go's `*p` does: `*p` read as a value is a copy, `*p = v` stores in place, `p[lo:hi]` is a
slice of the field, and a `range` over `p` reads each element when its round comes, while a `range` over `*p`
ranges a copy.

A limit: the view of a field of a struct that holds a string, a slice, a map, an interface or a pointer panics by
name. Every such site found in the TypeScript compiler and in the nine nugetgo modules is in `golang.org/x/sys`
(`unix` on linux and macOS, `windows` on Windows). A tuple assignment that stores the whole array through two
such views at once is also refused by name. See
[Pointers](ConversionStrategies-Reference/pointers.md#the-bytes-of-one-field-through-ntunsafepointerpf-the-field-view).
<!-- F1 parts A and B: merge 0d1f6f1f3f (P5, claude/p5-field-view-index d073be7130), BOARD 2026-11-15 "the field view
     answers over a Go struct, and is indexed in place where it is written" (the before-probe at 516586c4a9,
     linux/amd64 Release: over a Go struct with no reference, 9 forms panic "index out of range with length 0", 4 "nil
     pointer dereference", a range by value reads 0x0 where Go reads 0xf1). Part C: merge 0c1d295ea8 (claude/p5-field-view-whole
     da7cb0d29f), BOARD 2026-11-15 "the whole array through the field view is a load, a store or the view's window".
     Both footprint 0 on four targets. The reference-holding limit: BOARD 2026-11-15 "the field view of a struct that
     holds a reference is refused by name" and its census amendment (TypeScript closure: 17/17/4/7 sites on
     linux/amd64, linux/arm64, darwin/amd64, windows/amd64; nugetgo modules 17/17/4/0; all x/sys). The tuple store:
     BOARD 2026-11-15 "a tuple assignment that stores the whole array through two field views is refused by name"
     (proposed .5). REVISIT AT 310: P5's box-kind seat (a1a1d8534b) answers the reference-holding sites. -->

### A write at the address of a named array is no longer lost (P5)

**A write through the address of a named array type reaches the array, also before the program has used it.** For
`type small [8]byte`, `var local small; fill(unsafe.Pointer(&local), 41)` left `local[0]` at 0, where Go prints 41.
The same write was lost through a package variable, a struct field, `new(small)`, and the address converted to a
`uintptr` or to a raw pointer, and so was a `memcpy` from libc into a fresh named array. It reaches the array now,
and a first use of the array on one goroutine racing another's first use of its address no longer loses writes
either. No line of the converted standard library changes; the fix is in golib.
See [Pointers](ConversionStrategies-Reference/pointers.md).
<!-- BOARD 2026-11-15 "a write at the ADDRESS of a named array that has not been used yet is lost" (arms la-le lost,
     lf/lg controls right, linux/amd64). Option 1: merge 882e3da582 (P5, claude/p5-lazy-named-array-pin 70abf5dd5a;
     7 red + 2 controls -> 9 green; behavioral NamedArrayAddressWrite = go run; golib only). Option 2: merge 85b1c4181b
     (claude/p5-holder-publish c6479baf76; 5 racing arms lost 6,735-78,243 of 200,000 -> 0, the control still loses;
     golib only, no emitted line). The lazy backing (the go2cs-gen InheritedType template's interlocked publish) and both
     pin paths (golib's ж box, `ValueSlot is IArray`) are in the nuget-1.26.10.3 tree, read with git grep at the tag, so
     this is a fix to released behaviour. -->

<!-- 2026-11-14 (R, the release-notes audit, COORD item 5): every first-parent merge from the nuget-1.26.10.3 tag
     (c35c0269b8) to master c2919b8b16 was classified, and each user-visible change with no section here or in
     NEWS.md's held entry got one below, verified against its merge and records. The heading names the owning
     seat so its owner can correct it. Ordered with the headline first; at release they merge into the prose. -->

<!-- 2026-11-16 (P4, COORD's order: the draft brought current through batch 309, checked against master 0c1d295ea8).
     Every first-parent merge of batches 306-309 was classified. A line was added for each user-visible change to
     released behaviour: door B and the named-array floor (0cbd1eae70, a9dc1336a2), the field view (0d1f6f1f3f,
     0c1d295ea8), the lost write (882e3da582, 85b1c4181b), five sqlite-cs shapes (6992717d09, c09d5e04da,
     39b40c8c3a, b62c67b6d8, 4c101b8e57), the inline-array slice limit (6b78cef1a1) and the validated-modules page
     (65be87fd89, 64a5cc7e24). Already carried by their own seats' edits: door A (2d7ac32ffb), the pin-extent fixes
     (dbd872acad), cgo 3.1 and the cgo truth pass (d658f700cb, 3b202a6c56). HELD for P1: 055f4e6230 and d39b1ffe00
     (the Tuning section's note). LEFT OUT, each a fix to a feature new in this release, so no released build had
     the defect: the inline-array fixes 6b78cef1a1 (the foreign slice itself), b1c73ce844, d7f5a59f79, a6f526f31e
     and 63c2a339c7; the unsafe.Add box chain 18b36cb712, which came with batch 305's derived pointers. LEFT OUT as
     internal (CI, tests, records, design docs): 6f5def3cca (two darwin roster rows; the roster carries them),
     9b05e0dc4c, bd709b8206, 9c3f648b52, 1cfa713959, ec0a21ea2f, 3923dcda87, b4ac41960f, 5aa38c3ac2, 2c2c5b0ec0,
     b2214e00e4, 4415da65d3, 927441cd79, ec250198ff, d5a1f672fd, a0878172b2, be8c7a6859 (the zig pin for the
     linux-arm64 cgo smoke), 9615f7fc18 (proof-page pipe rendering; no committed page changed). -->

### Derived pointers keep what they point into (P5+H)

**A pointer made inside a Go value now keeps that value alive and in place.** Go lets a program point into the middle
of a variable, a struct or a slice's array with `unsafe`: `(*T)(unsafe.Add(unsafe.Pointer(p), off))`, or in one
expression, `(*T)(unsafe.Pointer(uintptr(unsafe.Pointer(p)) + off))`. In Go that pointer keeps the memory alive.
Converted code turned it into a number on its way to `*T`, and a number keeps nothing: the new pointer held no
reference to the value and did not stop the .NET garbage collector from moving it. Once nothing else referred to the
value, a write through the derived pointer went to memory the collector had already reused (a probe on linux/amd64
read 0 where Go reads 22), and after the collector compacted the heap a read returned the old bytes from the old
address. This is plain Go, with no cgo involved, and nothing reported it.

Now `(*T)(p)` over an `unsafe.Pointer` converts to `unsafe.Pointer.To<T>(p)`, which receives the pointer itself, and
pointer arithmetic written in one expression converts to `unsafe.Add(p, n)`, which moves the pointer and keeps what
it pointed into. A method that does this arithmetic on its own pointer receiver passes on the receiver itself. An array
field reached through such a pointer, and a slice over it, keep the value too. In the standard library the one reached
site of this shape is `syscall.UnixRights` and `UnixCredentials` on linux and macOS; three stress runs of 300,000
messages each on linux/amd64 never caught it failing, and it is closed by the same rule.

**A pointer returned from a function keeps it too.** A function that returns an `unsafe.Pointer`, such as
`return unsafe.Add(unsafe.Pointer(&b[0]), 4)`, had its result turned into a number at every call. On windows/amd64 a
program that wrote through such pointers after a garbage collection missed the slice, and the stray writes corrupted
the .NET heap: the program died with `Fatal error. Internal CLR error. (0x80131506)`. The call's result is now used
as it is. The standard library's calls of this kind, most of them in `runtime`,
`internal/runtime/maps` and `reflect`, changed with it.

**What you need to do.** Convert your own code again. The standard library in the 1.26.11.1 packages already carries
the new forms. Code converted by an earlier go2cs still builds against the new packages, but it keeps the old
conversion, and with it the hole.

**Limits.** A pointer rebuilt from a `uintptr` kept in a variable (`u := uintptr(p)`, later
`unsafe.Pointer(u + n)`) still keeps nothing, as Go's own rules say of it. So do a sum whose first term is not the
pointer and a rounded or masked address. A typed pointer made by arithmetic is not equal to the field's own address
(`(*int64)(unsafe.Add(unsafe.Pointer(&p), 8)) == &p.y` is false, where Go says true). One cost: a method that does
pointer arithmetic on its receiver pays for keeping the value at each call. On linux/amd64 that is 6% to 20% in the
standard library's own shape, and nearly twice the time when the receiver is a field of another struct, which no
standard-library caller does. See
[Pointers](ConversionStrategies-Reference/pointers.md#tp-over-an-unsafepointer-is-emitted-as-unsafepointertotp--the-typed-pointer-keeps-what-p-carries).
<!-- The interior-pointer root: docs/phase4/DESIGN-interior-pointer-root.md (on master since
     batch 307, merge 3923dcda87) sections 1.1 (the 12-shape program, shape 8:
     Go 2 then 22, converted 2 then 0, linux/amd64, 3 runs), 1.3 (CmsgStress, 3 runs x 300,000 messages, 0 wrong,
     recorded LATENT), 4.1-4.5, 10.1 (H's U2, ATypedPointerConvertedFromAnInteriorUnsafePointerKeepsItsStorageAlive: red
     3 of 3 after a compacting collection, the pointer reads the old bytes at the old address), 10.12 (leftmost-term
     rule, the node rule), 10.14 (S11: ctrlGroup get/set +6%/+12% and +17%/+20% in the package's own shape, +77%/+95%
     on a field receiver, linux/amd64, 20,000,000 calls, median of 5). Merge f4b803dfd8 (cut 837a413fdc): commits
     3527460c88 (golib To<T>; the old number operator kept, so old emission still compiles), ba226db89e (the door; probe:
     shape 8 reads 22 as Go does, it read 0), 58f547b592 (arithmetic form, receiver node rule), 86247389c0 (H 4.4/4.5,
     array-field windows), 837a413fdc (regen: 160 files, 938 out / 910 in; the 19 touched packages build on
     linux/amd64, windows, darwin and linux/arm64; no test host run on the regenerated corpus). Shape 9 still differs:
     bd7559f9bd ("the original repro differs from Go on line 9 only"); pointers.md, Equality and Limits paragraphs.
     The UnixRights site in the linux and darwin builds of syscall (sockcmsg_unix.cs) now reads
     `@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), ...)`.
     The returned pointer: docs/phase4/DESIGN-unsafe-pointer-call-result.md sections 2, 7 (UnsafePointerCallResultLifetime
     at master 3da19d0e2e: "Fatal error. Internal CLR error. (0x80131506)", 3 of 3 runs, windows/amd64) and 8 (108
     files, 758 lines each way, 17 packages; the windows/amd64 sweep of 57 rows all at their banked counts); merge
     a6cefba6ee. -->

### The packages ship linux-arm64 (G, with i9, C2, P2 and C1)

**The go.* packages now ship a linux-arm64 build of the standard library.** Through 1.26.10.3 they shipped
`win-x64`, `linux-x64`, `osx-x64` and `osx-arm64`, and a program built for linux-arm64 compiled against, and ran, the
Windows build of every package whose code differs by platform, with no error. Each such package now carries a fifth
folder, `runtimes/linux-arm64/`, converted from Go for `linux/arm64`. It calls the kernel with arm64's own
system-call numbers and structure layouts, and reads the processor's features from the kernel as Go does. A project
built with `-r linux-arm64`, or on an arm64 Linux host, uses it with no change. Go's own tests for every package on
the [validated roster](ValidatedTestPackages.md) that runs on Linux were run on GitHub's hosted Arm runner
(`ubuntu-24.04-arm`), and every one is validated there as well as on linux-x64. `go2cs -stdlib` accepts `linux/arm64` as a fourth
`-platforms` target.

A known limit: 15 internal standard-library members that Go writes in arm64 assembly have no managed body yet and
throw when called. A program reaches one only through a standard-library path. See
[Known issues](KnownIssues.md#on-linux-arm64-15-standard-library-members-throw-when-called).
<!-- The design and seats: docs/phase4/DESIGN-linux-arm64-flavour.md (S0-S7) and its review
     docs/phase4/REVIEW-linux-arm64-flavour.md. Packaging: S6, 83789ecbcc (P2, claude/p2-la64-s6-packaging),
     docs/phase4/DATA-la64-s6-packaging.md section 8 (release-smoke run 42983079137: every platform-split package
     carries five RID folders); the shipped-RID statement: docs/KnownIssues.md "Platforms the packages do not ship"
     (amended by 2e98774b35). Corpus: S2 1dc059d34a (G). Hand-owns: S3 93eb8c392d, 85047f1165, a79e5af297, b328dd71e7,
     9c95daacb6 (i9), census docs/phase4/DATA-la64-s3-census.md. hwcap: S4 6c6ecc776c (C2). Validation: S7 banks
     e141a180ce (P2), 7c7dc687f8 and 6db3fc9dd3 (C1); docs/ValidatedTestPackages.md, the Linux headline
     ("linux-arm64: 236 of 236 banked, 0 pending"). The 15 members: KnownIssues, from S6 66c13e7168. -->

### A build for a platform the packages do not ship stops (C2)

**A build for a platform the go.* packages do not ship now says so, instead of running the Windows build.** On
another operating system, or on Linux with another architecture (`linux-riscv64`, for one), the build stops with
the new error `GO2CS3003`, naming the platform and the first package that has no build for it. On `win-arm64` it
warns, `GO2CS3004`: the Windows build is that operating system's own, so the program runs. A build that only needs
to compile, and knowingly wants the Windows build, passes `-p:GoAllowReferenceFlavor=true` on the command line. A
portable app built on one platform and run on another, such as a linux-x64 build run on linux-arm64, prints a
warning on standard error at start-up.

**Two linux-x64 builds now pick the linux-x64 build.** Alpine (`linux-musl-x64`) compiled against the Windows build,
and failed to compile. A build with no RID on Ubuntu's packaged .NET SDK, which reports `ubuntu.24.04-x64` as its
own platform, also compiled against the Windows build, with no error. Both now resolve to linux-x64, as .NET does at
run time. Measured with SDK 10.0.403 and Ubuntu's 10.0.112 against go.* 1.26.10.3 from nuget.org. See
[Known issues](KnownIssues.md#platforms-the-packages-do-not-ship) and [diagnostic codes](DiagnosticCodes.md).
<!-- SEAT 0 of docs/phase4/DESIGN-linux-arm64-flavour.md (section 0, COORD ruling R2 with the review's F4), merge
     3d83f99d7b (C2, claude/c2-la64-seat0; branch commits 67b5578fa3 and 7346dc9015). The gate is
     src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1 (10 arms; master failed the UNSHIPPED, swap-off,
     project-file opt-out, ALPINE and WIN-ARM64 arms, the Alpine build with CS0426). The run-time half is
     GoHostOfRecord.Verify(GOOS, GOARCH), GolibTests HostOfRecordTests. GO2CS3003's example moved from linux-arm64 to
     linux-riscv64 when S6 shipped linux-arm64 (2e98774b35). -->

### The break floor (P2)

**The build now checks every go2cs-built package it uses against the break floor.** The break floor is the newest
go.* release that changed members that compiled code binds; at this release it is 1.26.11.1. The check runs before
the C# compiler. A `go.*` package counts as its own version. A `nugetgo.*` package counts as the release its
`go2cs/source-metadata.txt` names, and as below the floor when it ships no such file. Each package below the floor is
error `GO2CS3002`, naming the package and the release it was built on, and one more error stops the build before it
compiles anything, where such a package used to restore cleanly and then fail at run time with
`MissingMethodException`. To build anyway and accept that risk, set
`<GoAllowBelowBreakFloor>true</GoAllowBelowBreakFloor>`: each error becomes a warning. The check arrives with go.lib
1.26.11.1, so a project whose go.lib is 1.26.10.3 or older is not checked. A module packed some other way, under an ID
that does not start with `nugetgo.`, is not checked either. See
[The break floor](ConsumingGoFromCSharp.md#the-break-floor).
<!-- merge f7e140e982 (P2, claude/p2-break-floor, owner-ruled for .4): GoCheckBreakFloor in
     src/core/golib/buildTransitive/go.lib.targets (BeforeTargets=CoreCompile, skipped for design-time builds);
     the GoBreakFloor property set to 1.26.11.1 in the same file (moved from 1.26.10.3 by the promoted-forwarder seat
     and re-keyed by the 1.26.11 hop, 5f7b282396). The adversarial review and COORD's ruling:
     docs/phase4/REVIEW-go-break-floor.md (d81da0c9e2). Wording of the remedy and the unchecked cases:
     docs/ConsumingGoFromCSharp.md "The break floor" and docs/DiagnosticCodes.md GO2CS3002. If the owner prefers, this
     section can instead be a short paragraph ahead of NEWS's "Upgrading from 1.26.10.3" bullets, which already cite
     GO2CS3002. -->

### Converted from Go 1.26.11 (R)

**The standard library is converted from Go 1.26.11, a Go patch release with security fixes.** The converted
packages that change are `net/http` (its server and transfer code), `net` (its dialer), `crypto/rsa` and the FIPS
140 module, on every converted platform. Go 1.26.11's other fixes are in cgo, the go command, the compiler and the
linker, which have no converted counterpart. The go2cs tool, which downloads its own Go on first use, now downloads
Go 1.26.11. Every validated test package whose converted code moved was run again at Go 1.26.11 and validated
there. Each standard-library package's README now leads its badges with a "converted by go2cs" badge.
<!-- merge ffaafa144f (R, claude/r-hop-go12611), the 1.26.10 -> 1.26.11 patch hop under docs/GoCorpusMigration.md.
     The changed packages: the H5 regen 8a98787909 (predicted set MET: crypto/internal/fips140/*, crypto/rsa,
     internal/platform, net/{windows,linux,darwin}/dial.cs, net/http/server.cs and transfer.cs, on all four targets).
     The download: H2 b1e0cfb1b4, src/go2cs/toolchainPins.go (pinnedGoVersion "go1.26.11"). Re-banking: H10, the
     merge message (net/http, fips140test and net re-read at go1.26.11). The badge: 3ddde452d1. The security-fix list
     (net/http, cgo, the go command, compiler, linker) is the merge message's; no record in the tree itemizes it. -->

### What go2cs promises to C# code (C1)

**A new page says which names C# code can rely on.** [What go2cs promises to C# code that uses it](PublicSurface.md)
sorts every name a C# consumer sees into three tiers. Frozen: the exported surface of Go packages that are not
`internal` or `vendor`, the type mapping, golib's consumer API (`slice<T>`, `map<K, V>`, `@string`, `ж<T>`, `builtin`
and the rest), `PanicException` and `GoErrorException`, package IDs and the version scheme. Stable within a Go minor
release, and changed only when the break floor moves: generated plumbing and the golib API that converted code
calls. No promise: Go `internal` and `vendor` packages, tuple element names, and which thread runs a goroutine. The
page also lists the consumer MSBuild properties and the one path by which a promised name may change.

**IntelliSense hides the plumbing.** golib's types outside the consumer API, and the plumbing go2cs-gen generates
(interface adapters, `_package_asserts` classes, promoted-method host classes and the like), now carry
`[EditorBrowsable(Never)]`, so completion lists show the Go names. Nothing is renamed or removed, so code that names
one of them still compiles. Go `internal` packages are not marked yet, and no build warning names a use of one.
<!-- merge 1e654296e5 (C1, claude/c1-public-surface), the OQ3 seat and the freeze-and-document items of
     docs/PLAN-public-surface-stability.md (owner ruling 2026-11-11, merged as 9db360e89a). The page: 081ccaafaf and
     cb9f16c5b0 ("says only what the code does"). Hidden golib types and the baseline src/core/golib/PublicSurface.golib.txt:
     4883ac6bb0 ("No golib line moves"; GolibTests PublicSurfaceTests). Generated plumbing: 5a22ea46d5 and 5cba2d28a4
     (GenTests HiddenPlumbingTests). The OQ5 interface rule: 34582b3aef, d0caba1ca3 (GenTests GoInterfaceMemberGuardTests).
     OQ1 not done at this release: PublicSurface.md, Tier 3 ("At this release no package marks them"). -->

### cgo: `go2cs -cgo` converts a package that calls C (H)

**`go2cs -cgo` now converts a cgo package.** Through 1.26.10.3 the flag did nothing and a package that imports `"C"`
was refused. Now, on a machine with a C compiler, `-cgo` converts one package when every C use it makes is a call of
a C function that a shared library defines. Integers, floating-point numbers and pointers cross as arguments and
results. A struct of integers and pointers passes by value, and one struct or one scalar passes by pointer: C gets a
copy for the call, and the copy is written back after it. A function that sets errno can be called in Go's two-result
form, `n, err := C.f(…)`. The package's C types, typedefs and constants convert with it, and the converted C# reaches
them as `C.<name>`, as Go does. Anything else is refused by name, before anything is written, with the phase that
will cover it: C variables, a C function used as a value, a function the preamble itself defines, `//export`, and a
function the C compiler inlines or a header renames (`abs` and `labs` everywhere), among others. The cgo package must
be converted on its own: `-cgo` does not combine with `-recurse`, `-tests` or `-stdlib`. See
[Limitations](Limitations.md#what-wont-make-the-trip) and [`-cgo`](CommandLine.md).

**What you need, and where it runs.** go2cs does not provide a C compiler. It uses the one Go's own cgo build would
use, `go env CC`, and converts for that machine's own operating system and architecture only. On Windows use
llvm-mingw or MinGW-w64 gcc: under zig, Windows links C runtime functions such as `cos` and `exp` statically, so no
library exports them and go2cs refuses those calls by name. A program that uses the converted package needs no C
compiler, only the C library it was converted against. Each library loads by the name captured at conversion (for
example `libc.so.6` on Linux, `/usr/lib/libSystem.B.dylib` on macOS) from where Go's own loader would look, and
never from the current directory. A build of the package for another target is refused by name, and on another
operating system or architecture the converted package refuses to call C, naming both. A converted cgo program was
built and run against the Go build of the same program on linux/amd64, darwin/amd64, darwin/arm64 and
windows/amd64, converted with zig on Linux and macOS and with llvm-mingw on Windows. Every check printed what Go
printed, apart from the differences [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
lists, all of them in programs that break cgo's pointer rules.

Two fixes reach code that never uses `-cgo`. A package whose Go files all need cgo used to convert to nothing and
exit 0; it is now refused with `GO2CS2001`, naming the cause. And when a package loads with cgo on (under `-cgo`, or
with `CGO_ENABLED=1` set), a plain Go file under `//go:build cgo` now converts; it used to be dropped with exit 0.
<!-- docs/PLAN-cgo-interop.md section 11 and 5.1; docs/phase4/DESIGN-cgo-phase2-pinvoke.md section 11 (11.5, 11.12,
     11.20 increment 5, 11.22 G1 with its third hosted legs 43218540465 linux, 43218554704 darwin, 43218567730
     windows, GREEN on four rows; the windows zig override reading 43218579321 refused C.cos and C.exp);
     docs/phase4/DESIGN-cgo-phase3-marshaling.md 15.1 R1 and R5 (increment 3.0) and 15.6 (darwin/arm64 hand-down).
     Merges: d25c05efd2 (the gate), 18faa6d239 / b07fd14562 / 1fc911cbc3 (Phase 1), a7e19fafd9, 6075a10aeb,
     ddb57f9308, 2e9096000e, 44fad66dde, 86ba10b003, 816009e491, ea56e6ec40, 118cfbc77b (Phase 2 increments),
     b7f47fd8c6 (the real route: go env CC, host-target refusal), 72cb06355b (G1), 4423d66052 (3.0), 9a3a6e6aa0.
     Code: src/go2cs/cgoRoute.go (cgoHostTarget, cgoCapture, cgoTargetBlocks), src/core/golib/runtime/GoCgoLibraries.cs
     (TargetMismatch, Places, "the current directory is never searched"). The R1 half for CGO_ENABLED=1 without -cgo
     was read at nuget-1.26.10.3: its conversionDriver.go re-check (line 342) passed options.buildTags with no cgo tag.
     The flag at 1.26.10.3: CommandLine.md's row read "Not yet functional". -->

### A struct read from native memory holds its arrays as Go does (C2)

**A converted module that reads a struct out of memory laid out by Go, or reinterprets one through
`unsafe.Pointer`, now gets Go's bytes when the struct holds arrays.** go2cs held every Go array field as golib's
`array<T>`, a managed reference, so reading such a struct whole from Go-laid-out memory was refused by name at run
time. Code that ccgo translated from C, such as modernc.org/sqlite and its libc, does this throughout. Now a struct
that its own package converts whole through `unsafe.Pointer` is laid out at Go's offsets and size, with each array
field a fixed-size inline array (`array8<byte>` for a `[8]byte`), and so is every struct it holds by value. The
inline arrays come from `go.arrays.dll`, a new assembly in the go.lib package beside `golib.dll`, which a program
loads only when it uses one. Indexing, `len`, `range` and printing such a field work as in Go. The standard library
is never laid out this way, so its converted code is unchanged.

For a module, the rule is automatic, and `-inline-arrays <pattern>` forces packages on, or `-<pattern>` off. A type
the rule picks but cannot hold inline (an array or struct over 4096 bytes, an array field sliced or addressed, a
struct from another package) converts as before, with warning `GO2CS2009`. Error `GO2CS2010` refuses a
reinterpret between two structs whose arrays disagree, inline on one side and not on the other, when the packages
are forced or the inline side belongs to another package. A known limit: reflection over a struct that holds inline
arrays, `fmt.Printf("%v", s)` among it, panics by name. Print the fields one at a time. A slice of an array held
inline panics by name when the struct around it also holds a string, a slice, a map, an interface or a pointer;
a `copy` into or out of the array with constant bounds works, and so does slicing a local copy of the array. See
[`-inline-arrays`](CommandLine.md), [Limitations](Limitations.md) and [diagnostic codes](DiagnosticCodes.md).
<!-- docs/phase4/DESIGN-array-value-storage.md section 7 (the scout, 7.3 for the before-state:
     src/core/golib/runtime/RuntimeErrorPanic.cs refuses the whole-struct read) and its dated part-D and mixed-pair
     amendments. The library: merge b6f3bbb44f (C2, I0-lib; go.arrays packed by src/core/golib/golib.csproj:158-176#f8f2d9,
     a pack without it fails). The converter: merge 817b56e8b0 (C2, I0 part D; std footprint 0; the census P1 on
     linux/amd64 over modernc.org/sqlite v1.62.0 and libc v1.80.1). Mixed pairs: merge 555e30c826 (C2,
     claude/c2-inline-pairs, 3c25375a04). Emission shape: src/tests/Behavioral/InlineArrayFields. The reflection
     limit: docs/Limitations.md "Reflection cannot read a struct whose arrays are held inline". The slice limit:
     docs/Limitations.md "A slice of an array held inline panics when the struct around it also holds a string, …",
     merge 6b78cef1a1 (C2, go-winio Part 2, claude/c2-inline-foreign-slice 0333434616; COORD ruling 3b, disclosed for
     .4). -->

### A defined type over an instantiated generic converts (P1, H)

**`type Q pair[int]` and `type P[T any] pair[T]` now convert.** A defined type whose underlying type is an
instantiated generic struct used to make the converter drop the whole file that declared it, and the run still
exited 0, so the package compiled without that file's declarations or failed later for want of them. The type now
converts to a wrapper over the instantiated struct: its fields and methods are reachable, a generic defined type
declares its own type parameters, and a composite literal of it works inside and outside its package. The same
holds when the generic is reached through a type alias, in the same package or another. The code `GO2CS2004`, which
development builds briefly reported for this shape, is retired and stays reserved. A known limit: a second defined
type stacked on such a type, when the struct is declared in another package, does not yet get every member and
conversion generated, and fails at the C# build. See [diagnostic codes](DiagnosticCodes.md).
<!-- P1, batches 279 and 280: bf488d1296 (claude/p1-ts-go2cs2004, d0227c54b3), afbcc32974 (claude/p1-ts-go2cs2004-alias,
     9bd562223b); the interim refusal was H's C17, 60b13275fb; DiagnosticCodes.md's "Retired" row restored by 596a66cc89.
     The file-drop with exit 0: BOARD row from H's C11 seat (240a4dea0e). Std 0 sites. The known limit: BOARD
     2026-11-09 "a defined type whose chain reaches ANOTHER package's struct forwards no members past the first hop"
     (D1-D6) and 2026-11-10 (alias of an unnamed type), both OPEN, PARKED (0 std reach). -->

### `-trimpath`, on by default, and a fuller `-provenance` notice (R, P2)

**A converted module names no directory of your machine.** `-recurse` on a local module now records each of the
module's own source files the way `go build -trimpath` does, so a converted stack trace or `runtime.Caller` names the
file as `<module path>/<path in module>`, where it used to carry the absolute path of the converting machine. This is
the new `-trimpath` option, on by default; `-trimpath=false` keeps the absolute path, as an untrimmed `go build`
does. Standard-library and module-cache sources are recorded the same way either way. A module converted again
records its sources the new way. See [`-trimpath`](CommandLine.md).

**`-provenance` now says the file was changed.** The option writes two lines into each converted file, the Go source
file with its module and version, then `// Modified from the original by conversion to C# with go2cs <release>.`,
where it wrote one line naming the source file. The notice now follows the file's upstream copyright and license
comments, before the package doc comment, and still carries no timestamp. This is the notice Apache-2.0 asks of
modified files: convert a module under that license with `-provenance`. See [`-provenance`](CommandLine.md) and
[Converting a module](ConvertingAModule.md).
<!-- R, batch 283: 3362252ca8 (claude/r-trimpath, 0b365e34e8): src/go2cs/positionMapOperations.go,
     recurseTrimpath_test.go, src/core/runtime/managed_impl.cs; CNR 1004 identical; footprint 5567/5567 x3 (std
     unchanged). P2, batch 281: ae69e4ce74 (claude/p2-provenance-header, 93b0093adc), LICENSING.md and
     CommandLine.md; the nugetgo pack turns it on when -LicenseSpdx names Apache-2.0 and refuses a conversion
     without it (ConvertingAModule.md). Footprint 0 x3. The .3 text of -provenance: one line,
     `// Converted from Go source: "…"` (CommandLine.md at nuget-1.26.10.3). -->

### A conversion that loses a file now fails: `GO2CS2008` (C2)

**A conversion that loses a file now fails.** When go2cs faults on one file (a bug in go2cs itself), it recovers,
skips that file and converts the rest. The run used to print a warning and exit 0, so a conversion could lose a file,
with its functions, methods, `init` and interface records, and still read as success. Each skipped file is now
reported as `GO2CS2008`, every other file is still written, and the run ends with an error naming every skipped
file. This covers a hand-owned file's `.cs.auto` review sibling too. `-warn` does not accept `GO2CS2008`, since a
missing file is not loud. If you see it, report it as a go2cs issue naming the file; `-debug` re-raises the fault
with its stack. See [diagnostic codes](DiagnosticCodes.md).
<!-- C2, batch 295: a2bcf97a69 (claude/c2-dropped-file-exit, 068cdf1091): both recover sites (the per-file loop and
     emitAutoConversionSiblings); DiagnosticCodes.md GO2CS2008. Footprint 0 on 4 targets; CNR identical; -tests
     convert-only census over 242 packages, 0 dropped. One footprint run had dropped 61 files with rc 0 (the commit). -->

### More Go converts: TypeScript-Go readiness (TS readiness: P1, R, C1, C2, i9, P2, H, G)

**The Go implementation of TypeScript's compiler now converts, compiles and type-checks.** go2cs uses Microsoft's
TypeScript compiler in Go (microsoft/TypeScript's `tsc` module, at a fixed commit) as a large real-world test, and
each shape it found that did not convert or did not compile is fixed in this release. In go2cs's readiness runs on
Windows, every package that TypeScript's parser, binder, type-checker and emitter need compiles with no hand edit
to the converted C#, and the converted parser's output matches native Go's on every file of TypeScript's parser
test corpus. Type-checking TypeScript's single-file compiler tests one at a time matched native Go's output on all
but a few dozen of them, before fixes in this release for most of the rest. Five of its language-server packages
are still refused (`GO2CS2002`) and skipped, and xxh3 is used through the fork described below.

Among the shapes that now convert and run as in Go:

- a module that imports a path the standard library also vendors (golang.org/x/text, x/net, x/crypto and
  x/sys packages) now uses the module's package, not the standard library's private copy;
- a variadic method promoted from an embedded interface (a method value of it did not compile, and reflection
  counted one method too few);
- a method promoted through several levels of embedding, where a nearer level redeclares it;
- an interface that embeds two interfaces declaring the same method;
- a package that declares a type and a method of the same name (`type Kind` beside `func (t Token) Kind() Kind`),
  used from another package, including through an alias;
- a named string type used with `cmp.Ordered`, ranged over or converted from `[]byte` and `[]rune`, and a named
  `[]byte` satisfying `~[]byte | ~string`;
- named pointer types (`type P *T`): indexing, `len`, `range` and field access through them;
- `x == nil` on a type parameter constrained by pointer types (`[T *Node | *Symbol]`);
- a type parameter whose core type is `[]E` or `map[K]V`, returned or assigned as that type;
- a pointer type argument to a generic whose method-set constraint does not mention itself (`parse[O optionParser]`
  called with `&compilerOptionsParser{}`);
- two instantiations of another package's generic type converted to one interface;
- a pointer parameter re-pointed in a loop header, `for n = n.Parent; n != nil; n = n.Parent`;
- a `switch` on an array value with composite-literal cases, and a parenthesized type-switch case `case (*Node):`;
- `&^` with a constant on a `uintptr` alias, a `:=`-bound variadic func literal used as a value, a value passed to
  another package's anonymous interface parameter, and a package function and a method of the same name;

and others. Two of these answered wrong without any error before: the promoted variadic method's reflection count,
and a loop header that re-pointed a pointer parameter, whose condition read a stale value. The standard library
contains almost none of these shapes, so its converted code is unchanged by all but a handful of lines.
<!-- The TS readiness record docs/phase4/EXPERIMENT-ts-readiness-2026-11-07.md (catalogue C1-C29) and its data:
     rerun-preview-284p1-readings.txt (S4: closure rc 0, 0 errors, parse 26/26, bind 26/26, type-check 54/54, program
     and emit 63/63, "harness 68 of 68", "No hand-patch to any converted C#"), rerun-runahead-readings.txt (T5: 17,422
     of 17,422 parse units after C46; the full type-check pass 7,374 of 7,404 with the C45 xxh3 and C47 runtime
     patches; the 30 misses: C48 24, C49 5, C51 1), rerun-c48-c51-readings.txt (C48's 24 units match; C51 is
     ~110x per-call, correct output). All are Windows (i7) DIAGNOSTIC readings, not gates. TypeScript commit
     ba29a56299. The five refused packages: internal/ls/lsconv, internal/ls/autoimport, internal/ls,
     internal/project, internal/fourslash. Merges: C1 2663620296, C3 b5e99e005e, C4 f3ce8a0fc9, C5 ef137b6ae8,
     C6/C12 24973ed469 + 00bc75369f, C8 c021114195, C9 d0f9d63ca1, C10 248966d5e8, C11 240a4dea0e, C15 b6a3bc13ff,
     C16 c5a21d17c6, C17 60b13275fb, C18 d345469a7d, C19 e0468a5309, C22 eeecc94d3b, C22e d613a0c234, C24
     ee440ccf74, C28 7dcbfb74b4, C29 5b86e76ccb, C30 2563801e0f, C31 52cde97548 + 48f71f237c, C32 0804004099, C33
     15f1b979b2, C35 d770d97945, C37 b5ffdb166a, C38 8d19228a0c, C39 e240c5e815, C40 ad3fd922bf, C41 85c829ef0e,
     C42 08e00f6f9f. Std footprints: C28 6 files per target, C42 two files, C39 one line, C17 28 forwarders, the
     rest 0. C34, C43, C46 and C48 have their own sections. -->

### More Go converts: C translated to Go (sqlite-cs: P2, G, P1, C1)

**Code that ccgo translated from C converts further.** modernc.org/sqlite and modernc.org/libc are C libraries
translated to Go by ccgo, and converting them through go2cs found Go shapes that are rare in hand-written code but
everywhere in translated C. These now convert and run as in Go:

- a C function pointer carried as a `uintptr`: `__ccgo_fp(f)` and a call back through
  `*(*func(…))(unsafe.Pointer(&struct{ uintptr }{x}))`. Each top-level function gets one number, the same at every
  site, so C code that compares function pointers works, and a number that is not a function panics by name. These
  shapes used to stop the conversion or panic. See
  [Pointers](ConversionStrategies-Reference/pointers.md#a-func-value-carried-as-a-number--one-word-per-static-function-through-gofunccookie);
- a store through a cast pointer, `(*(*T)(p)).F = v` and `(*(*[N]T)(p))[i] = v`, which writes the memory `p`
  points at;
- every spelling of a slice over a pointer cast, `(*[1 << 30]T)(unsafe.Pointer(p))[:n:n]`, which views that memory
  instead of panicking, with Go's capacity for a three-index slice;
- `copy` into a struct that holds a fixed array, through a byte view of it
  (`copy((*[unsafe.Sizeof(d)]byte)(unsafe.Pointer(&d))[:], data)`, Go's record-decoding idiom), which panicked
  before; a struct holding a pointer, string, slice, map, channel, func or interface field is refused by name;
- a pointer conversion of a pointer conversion, `(*int8)(unsafe.Pointer((*int32)(p)))`;
- type aliases as C translations write them: an alias named like a C# keyword, an alias of an anonymous struct used
  across files and packages, and a declared type with a C name such as `float`, which keeps its own name;
- a defined type over a defined slice type (`type fermat nat` with `type nat []uint64`);
- a terminating `switch` whose guarded `default` falls through, a multiple assignment whose target is `*(*T)(x)`,
  and an integer converted to a named string type;
- a member that its package renames to avoid a name collision, reached through an import alias: `time` declares
  `Location` as `ΔLocation`, because `Time` has a method of that name, and libc imports `time` as `gotime`;
- the address of an element of an array whose element type is an alias of an alias of an anonymous struct that
  another package declares (`&X_iob[i]` over libc's `FILE`);
- an anonymous struct type declared inside a function and reinterpreted through `unsafe.Pointer`, and one
  declaration that names several variables of such a type;
- a pointer method called on a named result (`r.SetBigInt(bi)` in modernc.org/mathutil), which did not compile,
  and a pointer method called on a field of a named result, which compiled but wrote into a copy, so the write was lost;
- importing a converted package whose position map is one line longer than 64 KB, as modernc.org/sqlite's `lib`
  package's is. Its importers read none of its type aliases;
- one instantiation of a generic type used as a non-generic interface whose method names the type argument
  (`box[point]` as `interface{ val() point }`);

and others. Three uses of that last shape are not converted yet: the instantiation as a type argument the interface
constrains, a required method promoted from an embedded field, and a second instantiation of the same type behind
the same interface in one package. Each is named by the new warning `GO2CS2007`, and the C# build then fails at that
site.

**golang.org/x/sys/cpu reports the real CPU on amd64.** x/sys/cpu reads the processor's features with two assembly
functions, `cpuid` and `xgetbv`, from its `init`. They converted to methods that throw, so a converted program that
imported x/sys/cpu (modernc.org/libc does on windows and darwin) stopped before `main`. go2cs now forwards both to
managed bodies in golib: `cpuid` asks the processor through .NET, and `xgetbv`, which .NET does not expose, is
answered from what .NET reports about SSE, AVX and AVX-512. Measured on Windows x64, a converted module that imports
x/sys/cpu reports every one of its 29 feature flags as Go does. The forward applies to x/sys/cpu as a module
dependency; the standard library's private copy is not used by converted code. x/sys/cpu's arm64 feature reads are
not covered: they keep their throwing stubs. If a future x/sys/cpu declares either function with another signature,
go2cs refuses the package by name (`GO2CS2011`) rather than call a body that answers another question.

**go2cs names every function it cannot give a body.** A function in a third-party module that has no Go body
(written in assembly, or pulled from another package with `//go:linkname`) and that go2cs does not forward still
converts to a method that throws when called, and the conversion now names each one as warning `GO2CS2012`. A
program that never calls it is unaffected. If yours does, report it as a go2cs issue naming the function. See
[diagnostic codes](DiagnosticCodes.md).
<!-- sqlite-cs rows S1-S23 and the design docs/phase4/DESIGN-sqlite-cs-funcptr.md (S1, S21). Merges: S1 159f08e523
     (G; BOARD 2026-11-13 "a func value carried as a number takes one word per static function": r1/r2 stopped the
     transpile with "Unresolved dynamic struct type: struct{uintptr}", m1/m2/w1 panicked, w2 was silent; std 1 line,
     reflect funcName), S17/S20/S5 975aa3ac83, S13/S14/S6 adfcea58f6, S7/S4 811f3fdb3c, S18 a7e363527f, 3b 65404e5e63
     (P2); M1-M4 38d917d0be, S23/M5 2402236781 (P1); the closed carrier and GO2CS2007 3da19d0e2e (G; BOARD 2026-11-12
     "one instantiation of a generic type behind a non-generic interface now compiles and runs"); the reinterpret
     seam 71a08e8f9b (C1, docs/phase4/DESIGN-reinterpret-seam.md). S21: 839fd3634b (G; BOARD 2026-11-13
     "x/sys/cpu's cpuid and xgetbv take golib bodies by name": Windows x64, x/sys/cpu v0.49.0, exit 2 at master,
     29 flags equal Go's at the fix; gated on packages outside GOROOT; arm64 getisar0/getisar1/getpfr0/getzfr0 kept
     as stubs; darwin and linux conversions not run). GO2CS2012's reach on sqlite-cs: 8 stubs, none reached (1f2d0748ba).
     The sqlite-cs demo itself is NOT REACHED past start-up (BOARD S1 row, Q4: it stops at row S22).
     306-309 (P4, 2026-11-16, bringing the draft through batch 309): W2 6992717d09 (G; readPackageInfoLines' scanner
     stopped at its 64 KB default on lib's 123,448-byte GoPositionMap line; footprint 0/0 on 4 targets), W4/S10
     c09d5e04da (P2; CS0426 x5 fixture / x8 repro; footprint 0 x4), W5a 39b40c8c3a (G; CS0426 x4; footprint 0/0 on 4
     targets), S8c b62c67b6d8 (P2; CS1929 x6, and the silent write into a copy box on master; footprint 0 on 4 targets),
     L1 4c101b8e57 (G; 4 unresolved dynamic types at master; footprint 0 on 4 targets). -->

### An `unsafe`-free xxh3 for a `replace` (C1)

**github.com/zeebo/xxh3 converts through a copy go2cs carries.** xxh3 reads a string's or a slice's bytes through
its header and at computed offsets, which go2cs cannot yet convert: the converted program stops with a named panic.
The repository now holds xxh3 v1.1.0 with every use of `unsafe` removed, at
`src/forks/github.com/zeebo/xxh3@v1.1.0`; it computes the same hashes bit for bit. A module that depends on xxh3
converts with `replace github.com/zeebo/xxh3 => <go2cs clone>/src/forks/github.com/zeebo/xxh3@v1.1.0` in its
`go.mod`. The general fix is planned. See [Limitations](Limitations.md).
<!-- C1, batch 286: d386315daa (claude/c1-c45-xxh3-fork, 9516d0524e); the fork's FORK.md and identity/check.sh:
     16,632 results byte-identical to the original under Go 1.26.10 on linux/amd64 (AVX2 and AVX-512, so the
     original took its vector paths), with a negative control; xxh3's own tests pass on the fork. The general fix:
     docs/phase4/DESIGN-unsafe-header-views.md on claude/c1-c45-header-views, a post-1.26.11.1 train. -->

### `unsafe.Pointer` of a copy of the receiver writes where Go writes (P2)

**Inside a pointer-receiver method, `unsafe.Pointer` of a local copy of the receiver now points at the value.** In a
method on `*T`, `p := h` followed by `unsafe.Pointer(p)` converted to the address of the local variable `p` itself,
not of what `p` points to. A write through it landed on the local, the value never changed, and the program ran on
with a wrong answer. A field holding a `*T` and a local copied from another local did the same. They now point where
Go points; the receiver itself and a pointer parameter already did. Nothing in the standard library has this shape,
so only your own code changes when you convert it again.
<!-- Merge 269f475408 (claude/p2-fromref-local-box, e3e070dfe8). Behavioral UnsafePointerReceiverCopy: Go
     `7 8 0 9 10 0 11 12 13`, converted at the base `7 0 0 9 10 0 0 12 0`, fixed equal to Go (linux/amd64 box).
     Two-seeded -stdlib census: 0 changed lines on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64. -->

### Nil funcs and nil pointers behave as in Go (G, C2)

**A nil func of a named func type is one value in an interface.** Such a value read differently depending on how it
reached the interface: asserting it back to its own interface type failed, an assertion to `error` reported
"interface is nil", a panic message named an internal C# class, and `reflect`'s `Call`, `Method` and
`MethodByName` crashed with a .NET `InvalidCastException`. Each now answers as Go does, and `reflect.Value.Call` on a
nil func panics with Go's `call of nil function`.

**A type switch finds a nil func in its case.** `case F:`, with `F` a func type, missed a typed nil func, and the
switch went on to a later case or to `default` without a word. It now matches `F`'s case. The standard library has
seven such labels, in `net/http` and its tests.

**A nil func passed to a named func type stays nil.** Passing a nil func value where a parameter or a struct field
has a named func type threw `Delegate to an instance method cannot have null 'this'` where Go passes nil. The value
now arrives as nil.

**A nil pointer is one nil inside generic code.** A nil `*T` written as the literal `nil` and one that is a zero
value (`*new(T)`) are the same nil in Go. Inside a generic function they were two: `x != *new(T)` was true for a nil
`x`, a map with pointer keys could hold two nil keys, and an array of nil pointers compared unequal to another. They
are one nil now. A typed nil pointer in an interface is still distinct from a nil interface, as in Go.
<!-- N1: merge c78f162712 (d3dc97e466): behavioral NilFuncInterfaceReads died with InvalidCastException at the parent,
     its output equals Go's after; GolibTests NilFuncCarrierRuleTests. N2: merge 9c77232c27 (d4eb94fd60: 7 label lines
     in 4 net/http files, 3 of them test files); behavioral NilFuncTypeSwitch (90 silent lines at base). C47: merge
     847c629b31 (e2247291ee, 8a35b29c4f; behavioral NilFuncNamedDelegateArg; the TS shape core.IfElse(..., nil) to an
     ErrorReporter). C49: merge 0430770b00 (1821f05128: builtin.AreEqual<T>, map isNilKey, array<T> equality;
     behavioral NilPtrIdentity, linux, all 31 lines equal Go from 12 differing). No platform is named for N1, N2 or C47
     in their records. -->

### Reflection, generics and errors (G, C1, P5)

**reflect reads a value behind an interface as the value.** A named slice, map or other non-struct value held in an
interface through one of go2cs's adapters (a value converted to an interface it implements, or to `error`) was read
by `reflect` as the adapter: `Len` and `Cap` gave 0 or panicked, `Kind` reported `ptr`, `%v` and ranging over the
map printed nothing, and the program still exited 0. `sort.Slice` over such a slice panicked with
`call of Swapper on slice Value`. reflect now sees the value itself, and two `reflect.ValueOf` calls over one
interface value still compare equal.

**A float converts to an integer type parameter.** `T(f)` with `f` a `float64` and `T` constrained to an integer type
(`T ~int32`) did not compile after conversion. It now truncates toward zero, as Go does. An out-of-range value or NaN
is left to the implementation by Go; go2cs gives the answer Go's compiler gives on the machine the program runs on,
measured for all 11 integer kinds on amd64 and on arm64 (Apple silicon).

**A generic type whose `Error` method has a pointer receiver converts to `error` every time.** Such a conversion could
fail with "does not implement error.Error", depending on which packages had loaded and in what order. It now finds
the type's own method whatever the order.
<!-- reflect: merge c6fd06311c (6302266f1d): behavioral ReflectShellAccessors red at the parent (exit 2, "call of
     reflect.Value.Cap on slice Value"), green after; census 1028 reads; -tests reflect, fmt, errors, sort,
     encoding/json, encoding/binary, text/template: 0 tests moved. C36: merge c737e7760a (674f1c0bf8: CS1503 before;
     gc's amd64 table measured on go1.26.10 linux/amd64; 9413fb2f16: arm64 measured with go1.26.10 darwin/arm64 on a
     hosted osx-arm64 runner, run 42857184743; linux/arm64 not measured on a linux runner). error<T>: merge d9d52c0abd
     (df08b14978), docs/phase4/DATA-startup-method-sets-a2.md, "The difference" (GolibTests ErrorBoxLookupTests,
     both scan orders). -->

### Crash and deadlock reports print what Go prints (P2, C1)

**A deadlock report shows every goroutine, as Go's does.** After `fatal error: all goroutines are asleep -
deadlock!` Go prints one block per goroutine: its wait reason, its frames, and the `go` statement that started it.
A converted program printed the line alone, and when several goroutines saw the deadlock at once it could print it
more than once. It now prints Go's blocks, once, and exits with status 2. The report still appears only when every
goroutine waits on a nil channel or an empty `select {}`; any other deadlock hangs
([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)).

**Every fatal report reaches `debug.SetCrashOutput`.** Go copies whatever it prints while dying to the file set by
`runtime/debug.SetCrashOutput`. A converted program did so for a Go panic, but not when a .NET exception escaped a
goroutine, nor for the deadlock report: those went to stderr alone. They now reach the crash file too.
<!-- Deadlock: merge 01986cdd81 (4078bdc81b): red first at master 3da19d0e2e (the line alone; the three-goroutine case
     twice); behavioral DeadlockGoroutineBlocks; golib + hand-owned runtime only, 0 corpus lines; no platform named.
     Limitations.md:111-114#ab9ea8 and GoFeatureSupport.md:110#21160f already state it. Crash output: merge 456b550a46 (b60eef7b07):
     builtin.WriteFatalReport; GolibTests BackstopCrashOutputTests red with the crash file empty, green, linux. -->

### Runtime fixes (G, C1, i9)

**A profiled program no longer hangs.** With the block or mutex profiler on, the first event on a stack the profiler
had not seen could deadlock the thread that recorded it: building the profiler's own record was itself sampled by the
memory profiler, which waited on the lock its caller already held. At the default `runtime.MemProfileRate` that was a
rare draw; at `runtime.MemProfileRate = 1` it hung every time. The profiler's own records are now made outside the
memory profiler, as Go makes them.

**`runtime.GOARCH` is `arm64` on Apple silicon.** It reported `amd64` on every Mac, because the macOS packages are
built once, for `darwin/amd64`. It now reads the architecture of the running process, and the `GOARCH` setting that
`debug.ReadBuildInfo` returns and `go/build`'s default context follow it. On an Intel Mac and on `win-x64` it still
reads `amd64`. Everything decided by architecture when the packages are built still follows `amd64`
([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)). The tests of `debug/buildinfo`,
`net/http/pprof`, `go/build` and `go/internal/gcimporter` now match Go's on Apple silicon.

**`runtime.FuncForPC` returns one `*Func` per function.** It returned a different `*Func` for each call site of a
function, and not the one `runtime.Frames` reports for the same frame. Now `runtime.FuncForPC(pc) == frame.Func` holds,
two call sites of one function give the same `*Func`, and its `Entry()` is the function's, as in Go. Measured on
windows, linux and macOS.

**`os.Args` under `dotnet prog.dll` on Windows.** Started as `dotnet prog.dll a b`, a converted program read
`os.Args` as `[dotnet prog.dll a b]`, so every program that reads `os.Args[1:]` took the DLL as its first argument.
It now reads `[prog.dll a b]`, the shape the same launch gives on linux. Started through its own executable, the
program was already right, and it is still the launch to use for a program that runs itself again with
`os.Args[0]`.

**A zero-byte UDP datagram reads on macOS.** Reading a zero-length datagram could fail with
`address family not supported by protocol family`, where Go returns a zero source address. Measured on Apple silicon
and fixed for macOS and linux.

**Timers on macOS fire on time.** On the hosted macOS runners a 5 ms timer fired 9 to 13 ms late at the median, where
Go's own timers on the same machines are late by 0.7 to 1.3 ms. go2cs's timer service now waits as Go's runtime does
on macOS, and fires within the 1.5 ms its test allows, on Apple silicon and on Intel. This serves `time.Sleep`,
timers, tickers and I/O deadlines.

**CPU profiles on macOS name the program's own code.** The CPU profiler read each thread's identity and CPU time
through a .NET API that gives wrong answers on macOS, so nearly every sample was charged to `_ExternalCode`. It now
reads them from the kernel, as the samples themselves are keyed. One .NET limit remains on macOS: in a loop that
makes no calls, most samples are charged to the loop's caller.

**A nine-argument libc call on Apple silicon passes its ninth argument where Go 1.26 does.** go2cs still placed it
where Go 1.24 did. `internal/syscall/unix`'s `getnameinfo` is such a call.
<!-- Profiler: merge c64f8404c8 (ca23b88d6e: "at MemProfileRate = 1 the first block event on a new stack hangs every
     time"; GolibTests ProfilerOwnAllocationTests, child-process bound; found as an 84-minute hang of GolibTests Debug in
     batch 283's battery; no platform named). GOARCH: merge 26186c1739 (28cfcf657e darwin, 030725e30e windows: Windows on
     Arm UNMEASURED, no package ships for it; osx-arm64 debug/buildinfo 211, net/http/pprof 15, go/build 57, gcimporter
     634 validate per the merge subject); KnownIssues.md:15-33#b91ee1 already describes the behaviour; NEWS 1.26.10.3 (line
     229) called it planned work. FuncForPC: merge 40b021f8c4 (b20b20c5db; rows read as master on linux, darwin both
     RIDs and windows). os.Args: merge 813f55b5e8 (6a87a161f4: GolibTests WindowsCommandLineTests, windows/amd64; the
     self-re-exec limitation is disclosed in the hand-own's header). recvfrom: merge 872df69010 (9d31496d45:
     TestUDPZeroByteBuffer on osx-arm64; RecvfromUnspecifiedSourceTests 3/3 on both macs and on linux). macOS timers,
     CPU sampler and the nine-argument call: merge fb885891b3 (5c37f97453: median lateness 12.9 / 8.9 ms against a
     1.5 ms bar, Go's AfterFunc 1.34 / 0.70 ms, run 42595674585; 878610cb62: 17503 of 17703 samples to _ExternalCode;
     b1270b96dc: VariadicStackSlot arity 9; ruling (i) on the hog-as-leaf floor); darwin run 42617443400, 0 failed on
     both RIDs. -->

### On Linux, `runtime.SetDefaultGOMAXPROCS` returns (G)

**On Linux, `runtime.SetDefaultGOMAXPROCS` no longer throws.** Three runtime routines that Go writes only in
assembly, `sched_getaffinity`, `osyield` and `exit`, converted to methods that threw `NotImplementedException` on
linux-x64 and linux-arm64, and `SetDefaultGOMAXPROCS` always reaches the first. They now have bodies: the raw
`sched_getaffinity` system call, libc's `sched_yield`, and the `exit_group` system call. A known limit remains:
`SetDefaultGOMAXPROCS` returns without resetting the setting, so a converted program keeps the GOMAXPROCS it had,
where Go goes back to its default. Measured on linux/amd64.
<!-- merge fe454ae807 (G, claude/g-la64-r3-ledger, carrying g-linux-stub-bodies 0282fe03f4). Bodies:
     src/core/runtime/linux/stubs2_impl.cs (456c49f81f; measured under WSL, linux/amd64, SDK 10.0.403: 5 of 11 tests
     failed with the stub's NotImplementedException before, 12 of 12 pass after; the arm64 file is the hosted leg's).
     The remaining no-op: BOARD 2026-11-11, "runtime.SetDefaultGOMAXPROCS is a SILENT NO-OP on a public API", OPEN
     (34225f5b97). If the owner would rather not announce a fix that leaves a known no-op, drop this section. -->

### Faster locks, pointer compares and start-up (C2, P2, P4, P5)

**`sync.Mutex` takes a free lock at once.** An uncontended `Lock` and `Unlock` pair took 321.7 ns and allocated 64
bytes; it now takes 66.4 ns and allocates nothing. With four goroutines contending, a pair went from 2,191 ns to 105
ns. Measured on linux-x64. One difference from Go is now easier to see: a goroutine that unlocks and at once locks
again gets the lock back more often than a waiting goroutine does, because go2cs's mutex has no starvation mode,
Go's bound on that.

**Comparing pointers to struct fields is cheap.** `p == q`, for two pointers to a field reached through several
embedded structs, cost time that grew with the depth: at five levels an unequal compare took 717 ns, against Go's
3.4. It now takes about 6 ns at one level and at five (linux-x64). The same change fixes a rare wrong answer: pointers into two
different values could compare equal when their internal allocation numbers collided.

**Less copying and boxing.** A generated struct now implements `IEquatable<T>`, so a struct used as a map key is
compared without boxing, and its hash code is computed without allocating. A named map or channel compared with
`nil` no longer goes through reflection. `unicode/utf8` and `fmt` no longer copy a string argument on one internal
call each. Together these made the converted TypeScript compiler's checker about 8% faster on its first 25 test
units on linux.

**Faster start-up for programs that convert pointer-held types to `error`.** Finding a type's pointer-receiver
`Error` method read every extension method of every loaded package; it now reads only the type's own. A program that
loads many packages started about 40 to 65 ms faster under the JIT on linux/amd64; a small program sees no change.
<!-- Mutex: merge 6ea085d498 (bcea59d256: linux-x64 JIT, runtime 10.0.14, 4 cores, median of 9 rounds; Windows
     UNMEASURED; fairness: TestMutexFairness median 0.015 -> 0.14 s in context, all passing; BOARD row 5a7903f870, no
     starvation mode). C51: merge 2db66518be, docs/phase4/DATA-c51-field-view-equality.md sections 3 and 4 (716.52 ->
     6.03 ns at 5 hops, 28.73 -> 5.94 at 1 hop; Go 3.42-3.45; "the one intended difference", 29-bit allocation ids;
     FieldViewIdentityTests.FieldsOfDistinctRootsSharingAnIdAreDistinct red at base). S2-S4: merge 08944002b7
     (76b31d027c: HashCodeCombineAllocationTests 128,000 B in 1,000 calls -> 0; the TypeScript checker's first 25 perf
     units median 34.7 s against 37.6 s, 3 alternated runs, identical output, linux; corpus 2 lines on each of four
     targets). A2: merge d9d52c0abd, docs/phase4/DATA-startup-method-sets-a2.md (program "wide", paired medians -42 to
     -66 ms against an A/A control of -9 to +14 ms; hello and stringer within noise; one cloud linux/amd64 VM). -->

### Smaller converter fixes (R, P2)

**Converting for another platform reads that platform's Go settings.** With `-platforms` naming a target other than
the converting host, go2cs now selects files gated on Go's tool tags (`goexperiment.*`, `amd64.v1`, `arm64.v8.0`)
for the target, as `go build` does, where it used the host's. Go 1.26 turns the `dwarf5` experiment off for darwin
and on for windows and linux, so a darwin conversion on a windows host used to drop both halves of such a file pair.
In the standard library this adds one file to the darwin build of `internal/goexperiment`.

**A module's `package main` keeps its own `package_info.cs` when converted again.** A second conversion into the same
output used to warn, falsely, that the file belonged to another package and replaced it with the template, dropping
anything kept outside its marker sections.
<!-- R, batch 281: d914d8b360 (claude/r-tooltags, 14bea07f1e): src/go2cs/directiveOperations.go loaderToolTags per
     target; footprint darwin +1 file, windows and linux 0. P2, batch 282: e85dc60698 (claude/p2-pkginfo-doc-warning):
     BOARD 2026-11-10 "a main package in a module refused its own package_info.cs on every reconvert"; reach 0
     committed files (std has no main package). The guard (packageInfoBelongsHere) and the tool-tag lookup both ship
     in 1.26.10.3. -->

### The first-use Go download survives a virus scanner (P2)

**On Windows, the go2cs tool's first-use Go download no longer fails when another process briefly holds a file.**
The tool unpacks Go into a staging directory and renames it into place, and Windows denies that rename while a
virus scanner or the search indexer still has a file in it open. The install now retries the rename for up to two
seconds, as the go command does for the same errors, and when it still fails, names both directories. Measured on
Windows: the new held-handle test failed before the change and passes after it.
<!-- merge a713a73c06 (P2, claude/p2-toolchain-rename-retry): src/go2cs/toolchainProvision.go renameIntoPlace;
     TestToolchainInstallSurvivesAHandleHeldAfterUnpacking (Windows only), red at bbc0265c07 and green at 0afbc1b282
     on the i7. First seen on the i9 as "Access is denied" from TestToolchainProvisionFromModuleProxy. -->

### `go2cs -tests` stops cleanly on Ctrl+C (G)

**On linux and macOS, a `-tests` run handles an interrupt as `go test` does.** Ctrl+C now stops every process the
run started, the converted test host included, and go2cs exits with status 130. Every child starts with SIGINT and
SIGQUIT at their default handling, even when the run itself was started with them ignored (`cmd &`), so a converted
test of signal handling sees what it would see under `go test`. See [`-tests`](CommandLine.md).
<!-- Merge 58648c8d89 (claude/g-harness-sigint; red ec4605661e, green 2b491dc713); CommandLine.md's -test-action row.
     The os row run under an inherited ignore read DIVERGED at base and PASS 3831/2 at the fix; a normal run's
     comparison.json is byte-identical. -->

### Tuning: start-up against steady state, and server GC (G, P4)

**A short-lived converted program starts faster with one project setting.** Since 1.26.10.2 a converted program
optimizes its hot code from its first moment. That keeps a long-running program fast, and costs a short-lived one
part of its start. On `win-x64` (a 6-core laptop), published programs started 30 to 50 ms sooner with .NET's own
delay, and 100 to 170 ms sooner when pinned to one CPU, about a quarter of a small program's run. The same change
made a loop of TLS handshakes 41% slower, and 1.9 times slower on one CPU. So for a short-lived tool, a CLI, or a
container with one or two CPUs, set `<GoCallCountingDelayMs>runtime</GoCallCountingDelayMs>` in the converted project;
for a service, a server or a benchmark, keep the default. The setting applies under the JIT and ReadyToRun, not
Native AOT. The default does not change. See
[start-up or steady state](Tuning.md#start-up-or-steady-state-the-call-counting-delay).

**Server garbage collection is a documented option for an allocation-heavy, long-running program.** On `linux-x64`
(a 4-vCPU cloud VM), a large converted program, the TypeScript type checker, took about a third less time over a
slice of its compiler tests with server GC on and DATAS off, and used about 15% more peak memory. The memory cost
grows with the core count, so measure on the machine you deploy to. The settings take effect only in an executable.
Converted projects keep .NET's default, workstation GC. See [.NET runtime settings](Tuning.md#net-runtime-settings).
<!-- docs/phase4/DATA-call-counting-delay.md (G-LAPTOP, AMD Ryzen 5 PRO 6650U, 6 cores / 12 threads, .NET 10.0.11;
     tables 3.2 and 3.3: PerfStartup / PerfMidStart published, PerfTlsHandshake JIT +41% and x1.90), merge 6996aafb79;
     docs/phase4/SCOUT-ts-perf.md section 4 and docs/phase4/data/ts-perf/experiments.tsv (35.83 s -> 24.37 s, 1,252 ->
     1,447 MB peak RSS), merges fd2b6f5331 and c864b69655; owner ruling 2026-11-13 "an option, not the
     converted-project default". "Since 1.26.10.2": Tuning.md at nuget-1.26.10.3 attributed most of the 1.26.10.1 ->
     1.26.10.2 JIT speed-up to the delay of 0. -->

<!-- 2026-11-16 (P4): HELD for P1's AOT reading (COORD: "the AOT items still wait for P1"). Two user-visible Native AOT
     changes from batch 308 have no line yet, and get one when P1's reading lands:
     (1) merge 055f4e6230 (i9, lever G): golib's ж<T> no longer names ElemRefBox<T> in its view lookup, which ends a
     generic cycle; Native AOT IL3054 654 -> 0 on a golib-only consumer, image 19.7 -> 11.2 MB, PerfStartup -44%,
     parsedemo -34%, ILC peak -34..-41%; +2.7% PerfFieldView under the JIT.
     (2) merge d39b1ffe00 (P4): Tuning.md's "Native AOT method-body folding" (anchor native-aot-method-body-folding):
     IlcFoldIdenticalMethodBodies=false makes the AOT compiler 4.5x faster on PerfStartup and parsedemo 32m20s ->
     7m06s, for an image 11-15% larger; go2cs keeps the SDK default (the owner's ruling). -->

### Documentation (COORD)

**A new page, [Go feature support](GoFeatureSupport.md), says feature by feature what converts.** Each Go feature is
marked supported, supported with limits, or not supported yet, grouped by area. Each row links to how the feature
maps to C#, to a working example from the behavioral tests, or to the page that describes its limit. A pass over the
user-facing pages fixed what an audit found wrong or out of date, across the README, Limitations, Known issues, the
Roadmap and Consuming Go from C#, and [Command line](CommandLine.md) now lists every flag `go2cs -h` prints. To mark a
repository or a package page as converted by go2cs, use the badge served from go2cs.net
([how](ConvertingAModule.md#publishing-your-conversion-the-package-page)). The nugetgo pack now puts it first in the
block it writes into a package's README. The repository also has a code of conduct (the Contributor Covenant 2.1) and
a pull-request template. [Validated modules](ValidatedModules.md) is refreshed: nine Go modules whose own test
suites go2cs converts and runs, each read with Go 1.26.10 on the 1.26.10.3 packages, and the README links it
beside the standard-library roster.
<!-- merges 48665b310f and e02f6729d0 (docs/GoFeatureSupport.md, linked from docs/README.md and docs/Features.md);
     bce71f3ae9 (P2, the user-facing docs audit; the guard TestCommandLineDocListsEveryFlag); 6869ec2b54 and
     8792630525 (docs/images/go2cs-badge.svg, ConvertingAModule.md, src/tools/nugetgo/NugetgoReadme.psm1);
     c978595f0c (R: CODE_OF_CONDUCT.md, .github/PULL_REQUEST_TEMPLATE.md). Validated modules: merges 65be87fd89 (P4,
     batch 308; docs/ValidatedModules.md, 9 rows, read with Go 1.26.10 on go.* 1.26.10.3) and 64a5cc7e24 (P4, batch
     309; docs/README.md's Explore list). -->

<!-- 2026-11-15 (P4, docs audit Q2): the section below stood in NEWS.md as a held, visible "November XX" entry.
     Moved here verbatim (its heading demoted to ###) so the archive page shows no PENDING text; its items repeat
     some of this draft's (the assembly renaming, the break floor) and are merged at the release cut. -->

### Packages with very large method sets load (moved from NEWS.md; merge at the release cut)

<!-- Written ahead and HELD for release day (the .2 and .3 precedent): "November XX" becomes the publish date as the
     last change before the release tag (GoCorpusMigration.md, the release ritual, item 1). Section opened by the
     promoted-forwarder seat (P1, COORD ruling 2026-11-10 08:28Z); later seats for the release append to it. Every item is
     PENDING until 1.26.11.1 publishes. The release was prepared as 1.26.10.4; the owner ruled it ships as
     1.26.11.1, built from Go 1.26.11 (2026-11-12, the 1.26.11 patch hop). -->

**NuGet 1.26.11.1.** PENDING. The fourth release on the Go 1.26 line, and the first built from Go 1.26.11, a Go patch
release. Changes below break assemblies already compiled against 1.26.10.3, and C# source that reaches a promoted
method through `using static` alone; they are first under *Upgrading from 1.26.10.3*.

**Upgrading from 1.26.10.3.**

- **Rebuild what you built against 1.26.10.3.** PENDING. A Go struct that embeds another struct of its own package
  gets a forwarder for each method the embedding promotes. Through 1.26.10.3 those forwarders were members of the
  package class (`bufio_package` for `bufio`); from 1.26.11.1 each struct's forwarders are in a class of the
  struct's own, beside the package class in the same namespace. In the standard library 900 public forwarders moved,
  counted once for each assembly a package ships, in 20 packages; a program can import 13 of the 20, and the other
  7 are internal or vendored. An assembly or nugetgo package compiled against go.* 1.26.10.3 that calls such a
  promoted method fails at that call under 1.26.11.1 with `MissingMethodException`. The build refuses the
  combination first: a package built below the new break floor is error `GO2CS3002`, which names the package
  ([Diagnostic codes](DiagnosticCodes.md)). Rebuild against 1.26.11.1, and move each `nugetgo.*` reference to its
  1.26.11.1 revision.
  <!-- source: the promoted-forwarder seat (docs/phase4/DESIGN-promoted-forwarder-placement.md; owner ruling
       2026-11-10: "better breaking changes now than before we're deep with users"). The moved members are announced
       to the binary-compatibility guard as generated CP0002 entries, 900 entries in 20 suppression files
       (src/tools/pushnuget/New-GoApiCompatMoveSuppressions.ps1), and <GoBreakFloor> is 1.26.11.1. The nugetgo
       rebuild wave ships with this release (the owner's hands). -->
- **Standard-library assemblies are named after their packages.** PENDING. Package `go.fmt` now ships `go.fmt.dll`,
  not `fmt.dll`, and likewise every converted standard-library assembly (`go.net.dll`, `go.crypto.tls.dll`,
  `go.go.ast.dll` for package `go.go.ast`, `go.testing.dll`). The package IDs do not change, nor do `golib.dll`
  (package go.lib), the go2cs-gen analyzer, or a converted module's own assemblies. A library converted and built
  against go.* 1.26.10.3 or earlier binds the standard library's assemblies by their OLD names, so it must be rebuilt
  against 1.26.11.1; the same break floor names it at build time (`GO2CS3002`) rather than letting it fail as a
  missing assembly at run time. A C# project that referenced a converted assembly by file name (`fmt.dll`) or by an
  assembly-qualified type name (`go.fmt_package, fmt`) uses the new name.
  <!-- source: OQ4, docs/PLAN-public-surface-stability.md (ruled), the design docs/phase4/DESIGN-assembly-identity.md;
       src/go2cs/assemblyIdentity.go. The binary-compatibility guard compares each renamed assembly with its 1.26.10.3
       baseline through a rename map keyed to this release (src/tools/pushnuget/GoApiCompat.psm1). -->
- **`using static` alone no longer brings promoted methods.** PENDING. C# that calls into a converted package with
  only `using static go.bufio_package;` still sees the package's types, functions and the methods its types declare.
  A method a struct promotes from an embedded struct of the same package now needs the namespace as well:
  add `using go;` (or `using go.<path>;` for a package below the root). Converted code already has it.
  [Consuming Go from C#](ConsumingGoFromCSharp.md) has the details.
  <!-- source: the same seat; GenTests PromotedHostClassTests.UsingStaticAloneBringsDeclaredMethodsAndNoPromotedOne
       (CS1929 without the namespace) and TheNamespaceBringsThePromotedMethods. -->
- **The static members of `array<T>` need `go.` beside `using static go.builtin`.** PENDING. golib's `builtin`
  gains `array<T>(…)`, the function converted code now calls for an array literal (`[3]int{2, 3, 5}` is
  `array<nint>([2, 3, 5])`). In C# outside namespace `go` that has both `using go;` and `using static go.builtin;`,
  as every project has under the default `GoConsumerUsings`, the name `array<T>` before a `.` now finds that
  function first: `array<byte>.Alias(s, 4)` fails with CS0119. Write `go.array<byte>.Alias(s, 4)`. `slice<T>` has
  always worked this way because of its own `slice<T>(…)` function (`go.slice<nint>.Make(3)`). Declarations,
  `new array<T>(n)`, casts, `typeof`, `is` and `default` are unaffected, and so is converted code, which is in
  namespace `go`.
  <!-- source: Tour visual-parity item 4 (claude/p1-vp-array-literal). A probe project referencing golib compiled
       each shape in three arms: converted shape (namespace go, global using static), consumer shape (another
       namespace with using go; and using static go.builtin;), and qualified. Only the unqualified static member
       access fails, for array<T> and for slice<T> alike. golib's own tests needed 13 call sites qualified
       (GolibTests, b08b6087ad and e9ee58b76d). -->

**A package whose types promote very many methods now loads.** PENDING. The .NET runtime loads no type with more
than 65,521 methods. A converted package class held every promoted-method forwarder of every struct in the package,
so a package where many structs embed one method-rich base could pass that limit: it compiled, and its first use
threw `TypeLoadException`. With each struct's forwarders in a class of their own, the package class holds only what
the package declares. A type that would still exceed the limit is now a compile error, `GO2CS0012`, which names the
type and its method count.
<!-- source: the same seat. Measured shape: N methods on one base embedded by M structs put 2NM + 2N + 5 methods in
     the package class; N=250, M=131 (66,005) failed to load and M=130 (65,505) loaded. One converted package class
     held 116,102 methods, 107,835 of them forwarders. -->

**A promoted method that returns a string, a bool or several values can be called from another package.** PENDING.
A Go struct that embeds an exported struct promotes its methods, and code in another package may call them on the
outer struct. The C# forwarder for such a method was `internal` whenever its return type's C# spelling ended in a
lowercase name (`@string`, `bool`, `error`, any multi-value return), so a call that Go accepts did not compile in
the converted program (`CS1929`). The forwarder of an exported method is now public unless its signature names a
type that is not public. In the standard library 268 forwarders in 12 packages became public, `bufio.ReadWriter`'s
`Read` and `Peek` among them; none was removed, so nothing compiled against 1.26.10.3 breaks on this change.
<!-- source: C34 of the TypeScript-Go readiness run (repro c34promotedbuiltinreturn at 4505b95021 on
     claude/i7-ts-readiness-rerun), folded into the promoted-forwarder seat. GenTests PromotedForwarderScopeTests;
     behavioral PromotedValueEmbedLib / PromotedValueEmbedUser. The count is of forwarder methods in one target's
     packed assemblies, the same on windows, linux and darwin (docs/phase4/DESIGN-promoted-forwarder-placement.md,
     amendment section H). -->

**A method whose receiver type is written through an alias runs on a nil pointer, as in Go.** PENDING. Go accepts
`type AttrNode = Node` with `func (n *AttrNode) Mode() int`, and the method is `Node`'s: it can be called on a nil
pointer and test `n == nil` itself. The converter took such a method's receiver by reference, so the nil test read
the struct through the nil pointer and the call panicked where Go returns; a method that used the receiver as a
pointer in any other way did not compile. Such a method is now emitted as it is when the receiver is written with
the type's own name. The standard library has no such method, so its converted code is unchanged. A receiver that
is an alias of the pointer type itself (`type P = *T`) still does not compile after conversion.
<!-- source: C48 of the TypeScript-Go readiness run (one type-check unit panicked in
     ast.(*Node).GetResolutionModeOverride), P1; docs/phase4/DESIGN-alias-receiver-form.md. Behavioral
     AliasReceiverForms; converter tests TestAliasReceiverTakesTheNamedForm and
     TestAliasReceiverEmitsAsNamedReceiver. -->
