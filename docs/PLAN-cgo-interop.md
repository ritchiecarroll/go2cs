# PLAN — cgo interop for go2cs: from `import "C"` to a working P/Invoke bridge

> **STATUS: RATIFIED WITH BINDING CORRECTIONS (Fable adversarial review, 2026-08-29,
> `phase4/REVIEW-cgo-interop.md`, branch `claude/cgo-review`) — corrections folded 2026-08-29.**
> The frame is ruled; the ladder staffs per §1's sequencing statement, not on ratification. A
> **strategy plan** ([`Glossary.md`](Glossary.md), *Plan*): it fixes a ruled frame — which targets,
> in which order — and holds its OQ rulings until its ladder completes. It supplies **no
> procedure**; procedure stays in the runbooks. §10's ⟨OQ-1⟩ through ⟨OQ-5⟩ carry recommendations
> awaiting ruling; ⟨OQ-6⟩ is ruled.
>
> Written against `be58eb4aa` (2026-08-25), Go 1.23.12, .NET 10; **line cites re-resolved against
> master `6fcf4be7b` at fold time** (content verified identical in each case — the numbers moved, the
> code did not). **Revised twice on 2026-08-28 under adversarial review, then corrected once on
> 2026-08-29 under the ratifying review**; §9 records every claim those reviews falsified — four of
> the first draft's that were simply wrong, one of the second draft's, and one the ratifying review
> found wrong at this document's own base and still wrong at master. That
> review also re-grounded the plan against the twelve merge windows since its fork — the keystone
> tether (§4), the struct-passing ruling (Phase 2), the single-file host (Phase 6) — and supplied
> the two absences it names: security posture (§5.4) and campaign sequencing (§1). The §5.3 finding
> against [`phase4/DESIGN-cooperative-scheduler.md`](phase4/DESIGN-cooperative-scheduler.md) was
> **actioned at master** (`6fcf4be7b`, 2026-08-28); that document's status block now carries the
> dated amendment and names this plan's blocking-call section as the trap it retires.
>
> Scopes the `cgo` slice of [`Roadmap.md`](Roadmap.md) Phase 5 ("replace the `PartialStubGenerator`'s
> throwing implementations for Go declarations backed by assembler, cgo, runtime/compiler intrinsics,
> or platform services") — specifically the part Phase 5A/5B's inventory-and-companion machinery does
> not reach: a Go file that itself declares `import "C"`.
> [`ToDo.md`](../src/go2cs/ToDo.md) item 48 has tracked this, unimplemented, since `87465f5f5`
> (2025-01-12) — the "go2cs now based on Go" restructuring that began this generation of the project.
>
> Companions: [`TestingInfrastructureRequirements.md`](TestingInfrastructureRequirements.md) (Phase 4 —
> the validation pipeline Phase 6 extends, and the determinism principle §5.2 answers to),
> [`phase4/DESIGN-pointer-provenance.md`](phase4/DESIGN-pointer-provenance.md) (RATIFIED) and
> [`phase4/DESIGN-native-backed-slice.md`](phase4/DESIGN-native-backed-slice.md) (RATIFIED, LANDED) —
> the pointer/slice foundations Phase 3 builds on.
>
> **AMENDED 2026-11-08 (lane H) — re-grounded at master `30ad20e17e` (Go 1.26.10, x/tools v0.52.0).** The
> owner opened cgo foundation work for the `.4` queue. §11 carries the whole re-ground: every cite re-resolved,
> §2 re-measured with its predictions scored, §4 re-checked against what landed since 2026-08-29, and §5.3 marked
> **STALE-BY-DESIGN** against the scheduled M:N scheduler. Read §2, §4, §5.3, Phase 1 and Phase 6 **through §11**.
> Two premises it falsifies: §2 Finding 3's "the `-cgo` flag is inert" (it was a bypass) and Phase 6's "`-tests`
> and `-recurse` cannot be combined" (they can). Phase 1 step 1 is **landed** (`948b9e8998`, 2026-08-29), so its
> "severable" exception is spent. The native-call transition seam §5.3 now needs is designed in
> [`phase4/DESIGN-native-call-seam.md`](phase4/DESIGN-native-call-seam.md). ⟨OQ-1⟩ is **answered YES**, measured
> by lane P1 on a cgo-capable host (§11.7): go/packages delivers typed declarations for all five kinds, so Phase 1
> needs no C parser. It is recommended RESOLVED.

---

## 1. Verdict and scope

cgo is not a fundamental platform mismatch for go2cs. Its hard parts — goroutine-stack switching,
P-detach around a blocking call, GC-visible-pointer rules, reverse-call thread registration — are
runtime-transition mechanics the CLR has mature, explicit answers for: `LibraryImport`, blittable
marshaling, `GCHandle`/`fixed` pinning, and `[UnmanagedCallersOnly]`. There is no clean, portable
"cgo API" separable from those mechanics to target instead — `cmd/cgo`'s `C.foo()` surface is thin,
but every call beneath it is wired into `runtime.cgocall`/`cgocallback`.

Go's side of that bargain is *cheaper* than the CLR's, not weaker: Go's heap is **non-moving**, which
is exactly what lets `&b[0]` be handed to C with no pinning at all. (Go does move goroutine **stacks**,
which is why `cgocall` switches to the system stack.) .NET compacts, so pinning is the price of
admission here — a cost to pay, not an advantage to claim.

It is a real subproject, not a flag flip, and §2's measurement shows the starting line is further back
than assumed. §5–6 stage the work in six phases sized to the common shape of cgo usage: declared
extern functions, simple structs, basic callbacks. §7 is deliberately speculative and unscoped — the
long tail is named, not estimated, per the no-frozen-figures discipline
([`Glossary.md`](Glossary.md), *Runbook*).

| # | Sub-problem | Weight in practice | .NET-side primitive | Phase |
|---|---|---|---|---|
| 1 | Declared extern C functions | the bulk of cgo usage | `LibraryImport` + blittable structs | 1–2 |
| 2 | Pointer / string / slice marshaling | underlies every call in row 1 | `PinnedBuffer` (Go→C), native-backed `slice<T>` (C→Go), `ж<T>` provenance | 3 |
| 3 | Inline C bodies in the preamble | common in small libraries | native side-build, then P/Invoke — never C→C# transpilation | 4 |
| 4 | Reverse callbacks (`//export`) | less common, load-bearing where used | `[UnmanagedCallersOnly]` | 5 |

**Sequencing — this ladder staffs after the validation campaign's platform-parity goal, and gates
nothing before it.** §2 Finding 5 supplies the argument: the corpus is wholly `CGO_ENABLED=0`
output, so no roster row and no toolchain hop depends on cgo. Ratification fixes the frame; it does
not claim a slot. The one deliberate exception is **severable**: Phase 1 step 1's file-list zip fix
(§2 Finding 4) is a latent correctness bug independent of cgo and may staff on its own merits
without staffing the ladder. Read Phase 1's "worth doing on its own merits" as applying to that
step alone.

## 2. Measured: what happens today

**Instrument.** A minimal cgo fixture plus a `packages.Load` probe using `packages.LoadAllSyntax` —
the exact mode `conversionDriver.go:89`, `stdLibConverter.go` and `moduleConverter.go:156` all use.
Run on the coordinator machine, 2026-08-28, against `be58eb4aa`.

**Finding 1 — the converting machine cannot run cgo at all.** `go env CGO_ENABLED` → `0`; no `gcc`,
no `clang`, no `cl.exe` on `PATH`. Go's cgo on Windows requires gcc or clang and **does not support
MSVC**, so the MSVC toolchain the Native-AOT perf path already depends on does not satisfy cgo.
Toolchain provisioning is a day-one problem, not a Phase-7 one — and it is why ⟨OQ-1⟩ below could not
be closed, and why every cgo-enabled reading in this section is bounded by "no C compiler present."

**Finding 2 — a cgo package under `CGO_ENABLED=0` loads SILENTLY EMPTY.** `packages.Load` returns one
package with `GoFiles=[]`, `CompiledGoFiles=[]`, a zero-name type scope, and a single *soft* error,
`build constraints exclude all Go files`. Not a hard failure — an absence.

**Finding 3 — the fatal gate is effectively dead code, and go2cs reports SUCCESS on a cgo package.**
The first draft claimed `import "C"` is "a hard stop" at
[`visitImportSpec.go:212-214`](../src/go2cs/visitImportSpec.go). Measured, that gate **never fires**:
with cgo disabled, build constraints exclude the file before any visitor runs; with cgo *enabled*, the ASTs
reaching the visitor are cgo-**rewritten** and no longer contain `import "C"` at all. Actual behavior,
identical with and without `-cgo=true` — **the flag is inert**:

```text
WARNING: cgorevfixture did not fully type-check; converting best-effort — code depending on
         the following is emitted untyped: [-: build constraints exclude all Go files in ...]
INFO: Skipping conversion: no target Go source files found for conversion in input path "..."
exit code: 0
```

A cgo package converts to **nothing** and the converter **exits 0**, having emitted only `go2cs.ico`.
This is the repo's catalogued false-green pathology — the shape CLAUDE.md names for the GOROOT
forward-slash trap, *"a path the converter half-recognizes is worse than one it rejects."* The
`did not fully type-check` warning means CNR would report such a package **NOT MEASURED**, so the gate
is not fooled; a hand-invoked conversion is.

**Finding 4 — `pkg.Syntax` is zipped against the WRONG file list in the production path.**
[`conversionDriver.go:227-228`](../src/go2cs/conversionDriver.go):

```go
for i, file := range pkg.Syntax {
    path := pkg.GoFiles[i]
```

`golang.org/x/tools@v0.36.0/go/packages/packages.go` — the version `src/go2cs/go.mod` pins — is
explicit that this is the wrong pairing. `:512` *"Syntax is the package's syntax trees, **for the
files listed in CompiledGoFiles**"*; `:518-519` *"kept in the same order as CompiledGoFiles… If
parsing returned nil, Syntax **may be shorter** than CompiledGoFiles"*; `:447-451` *"GoFiles… may
include files that should not be compiled… or are **subject to cgo preprocessing**"*.

For every package in the corpus today the two lists coincide, so the zip is correct *by accident*.
Under cgo they diverge — `CompiledGoFiles` holds cgo's generated output in a build cache — and the
pairing either overruns or, worse, **silently misaligns**, binding an AST to another file's path. That
path decides the emitted `.cs` name, the hand-own marker probe (`conversionDriver.go:244-246`), and the
`CheckBuildConstraints` target. The nils caveat makes the zip fragile even without cgo. The `-tests`
path already does it correctly ([`testConversion.go:1183-1187`](../src/go2cs/testConversion.go)):
zipped against `CompiledGoFiles`, bounds-guarded. *This is Phase 1's first change, and a latent
correctness bug independent of cgo.*

**Finding 5 — zero regression risk to the existing corpus.** No tracked `.go` file imports `"C"`. The
corpus is wholly `CGO_ENABLED=0` output, so cgo support cannot move an existing golden or CNR verdict.

**Finding 6 — three existing "C" accommodations, all avoidance, none a binding.** The fatal gate
(`visitImportSpec.go:212-214`), the `classSkip` bucket (`moduleConverter.go:226`), and a type-alias
preload skip (`importOperations.go:1027`). Nothing parses a preamble; nothing generates anything.

## 3. Where this sits in the existing roadmap

[`Roadmap.md`](Roadmap.md) Phase 5 already names cgo alongside assembler and runtime intrinsics, but
its machinery was built for a different shape of the word:

- **Bodyless stdlib declarations backed by asm/cgo *linkage*** already compile — `PartialStubGenerator`
  emits a throwing `partial`, and Phase 5A/5B replace each with a hand-owned `*_impl.cs` companion, the
  `sync/atomic` `doc.cs` + `doc_impl.cs` pattern ([`Roadmap.md:770-784`](Roadmap.md)). **Bounded and
  curated** — the stdlib's own declarations, known in advance.
- **A Go source file's own `import "C"`** is missing entirely (§2). **Unbounded** — user- or
  library-supplied.

This document designs the second. It reuses the first's *shape* — a converter-owned declaration paired
with an implementing companion — but the companion must be **generated**. That is Phases 1–4. The
Windows `syscall` interop already proved the hand-written approach works and where it stops:
`zsyscall_windows_impl.cs` and `interface_windows_impl.cs` found and fixed exactly this bug class —
non-blittable types needing blittable mirrors, `ж<T> → uintptr` answering 0 for a nil boxed pointer on
`**T` out-parameters, raw kernel buffers needing manual transcription — one hand-owned wrapper at a
time, over an enumerable Win32 surface. The **Linux** family since joined it and carries the harder
lesson: `syscall/linux/structclass_linux_impl.cs` (2026-08-28) closes the struct-passing class
*proactively* because a native write over managed-reference storage is heap corruption, not a wrong
answer (Phase 2 absorbs its discipline). cgo raises the same classes against a surface nobody can
enumerate in advance.

## 4. Foundations already in place

- **Pointer provenance** ([`phase4/DESIGN-pointer-provenance.md`](phase4/DESIGN-pointer-provenance.md),
  RATIFIED 2026-08-23) teaches `ж<T>` which kind of address it holds — pinned-managed, pointer-shaped,
  or native. That is the classification a generated wrapper needs to decide whether a value handed to C
  must be pinned or is already stable. Since `3174009a8` it is realized as the **kind split** — `ж<T>`
  abstract over `StandardBox`/`FieldRefBox`/`ElemRefBox`/`NativeBox` — so the classification is a type,
  not a flag.
- **Native-backed `slice<T>`** ([`phase4/DESIGN-native-backed-slice.md`](phase4/DESIGN-native-backed-slice.md),
  RATIFIED 2026-08-22, landed) covers the **C→Go** direction: wrapping a C-returned buffer as a slice
  that reads and writes through to the real memory.
- **Native-box source retention** — `NativeBox<T>`'s `m_retainedSource` slot (`ж.NativeBox.cs:31/47/79`)
  and the corpus-wide `unsafe.Pointer.FromBox` mint (`core/unsafe/unsafe.cs:368-380`, merge `a7c964d80`): a number
  that carries the box it was minted from. This is the **callee-retains** mechanism — the machinery
  Phase 3's `C.CString`-class helpers and any C-keeps-the-pointer scenario need, and it landed after
  this document's fork.

**The Go→C direction is a different mechanism, and the second draft of this document got this wrong.**
A native-backed slice cannot be pinned — `slice.cs:431` *throws* (*"pinning is meaningless for native
memory — take an element address instead"*), and `OverNativeMemory` refuses any `T` carrying managed
references. Handing a **managed** Go slice to C uses the pre-existing `PinnedBuffer`
(`slice.cs:430`, and its siblings in `string.cs`/`ж.cs`). So Phase 3 is *partly* reuse: the C→Go half
is ratified work, the Go→C half is existing golib machinery, and only the glue is new.

**The keystone tether re-grounds what "the wrapper owns the pin scope" has to mean.** The ж→uintptr
lifetime gap was measured post-fork (os/exec's GC-mark SIGSEGV, rooted 2026-08-26) and closed at
`src/core/internal/runtime/syscall/linux/syscall_linux_impl.cs:105-119`: a `(uintptr)Ꮡbuf` argument's
pin lives **on the box**, the token registry holds the box **weakly**, so the JIT may retire the
caller's box local the moment the address is extracted — *before the call runs*. The remedy re-roots
each argument's box for the call's duration (registry `Resolve`, strong locals, `GC.KeepAlive` past the
return). Linux syscalls have one funnel to fix; **cgo has none — every generated extern is its own
crossing.** So the wrapper-owns-the-pin-scope answer is structurally correct only under a stated
contract, which Phase 3 carries: *the generated wrapper receives the pinnable object or a box-carrying
`Pointer` (`unsafe.Pointer.FromBox`), never a bare pre-extracted address; where Go hands it
`unsafe.Pointer(&b[0])`, the emission is FromBox-shaped so the wrapper can pin or re-root.* A wrapper
handed a number has already lost the ability to keep the storage alive. (`PinnedBuffer.Clone` re-pins
since `1d01200a9` — Phase 3 inherits that fix for free.)

## 5. Constraints the design must carry

### 5.1 A C toolchain, on the converting machine, per target

cgo preprocessing and Phase 4's inline-C build both require a working C compiler for the *target*
platform; §2 Finding 1 shows this project's own machine has none. Two consequences: cgo support must
**assert** `CGO_ENABLED=1` and a resolvable compiler, failing loudly (never inheriting §2's
silent-empty path); and `-platforms` multi-target emission needs a provisioned **cross** C toolchain
per target — materially harder than the pure-Go layout-L3 emission already solved. §7 keeps the
cross-compilation matrix speculative for this reason.

### 5.2 Determinism, and what that forbids

[`TestingInfrastructureRequirements.md`](TestingInfrastructureRequirements.md) §2 principle 8 requires
that equivalent inputs — *"Go version, converter version, and target platform"* — produce byte-stable
output; CNR gates emitted `.cs`/`.csproj` on byte identity. **The C toolchain is not in that
equivalence class**, and cgo threatens the principle from two directions:

1. **Generated C# derived from local system headers.** `#include <stdlib.h>` resolves against the
   converting machine's headers. `C.size_t`, `C.long`, `time_t` width and struct layout vary by libc
   (glibc vs musl), SDK version, and flags like `_FILE_OFFSET_BITS`. A generated `[StructLayout]` mirror
   is therefore a function of the machine. Layout L3 models per-**GOOS** variance only; it has no slot
   for per-libc or per-SDK variance.
2. **A compiled native artifact** (Phase 4) is machine- and architecture-specific by construction, and
   is a **binary** — a class the corpus neither tracks nor gates, and which fleet doctrine explicitly
   excludes ("never git-committed binaries").

**The `.s`-assembly precedent does not license this.** [`ToDo.md:49-51`](../src/go2cs/ToDo.md) proposes
compiling assembly to object code and wrapping it — but that is *Go's own assembler*: hermetic,
versioned with the toolchain, inside principle 8's equivalence class. An external gcc/clang is not. The
precedent has exactly the property Phase 4 lacks.

**Recommendation:** generated bindings and any native artifact are **build outputs, not corpus
content** — git-ignored as the `-tests` pipeline's regenerated inputs already are
(`src/core/.gitignore`), and excluded from byte-identity gates by construction. ⟨OQ-4⟩ carries the
open half, and the word "deterministic" is deliberately *not* claimed for the convert-time option.

### 5.3 Blocking calls — the pool-starvation risk no longer exists

**The first two drafts of this document built this section on a premise that was already false at
their own pinned commit, and the correction changes the conclusion.** Goroutines have not been
`ThreadPool` work items since `4f06d78ae` (2026-08-13, *"SCHED-S1: goroutines get their own threads —
the runtime owns capacity, and the pool floor retires"*), which is an **ancestor of `be58eb4aa`**.
`Goroutine.Start` (`golib/runtime/Goroutine.cs:202-209`) is now `new Thread(() => Run(body),
s_stackReserve)`, one dedicated thread per goroutine; the source says so directly at `:26` —
*"Goroutines **used to be** `ThreadPool.QueueUserWorkItem` work items."* The 28.7-minute
`internal/singleflight` ladder that
[`phase4/DESIGN-cooperative-scheduler.md`](phase4/DESIGN-cooperative-scheduler.md) measured is cited by
`Goroutine.cs:26-38` as the historical motivation **for the fix that removed it**.
(The flag this section raised against that document — present-tense
`QueueUserWorkItem` in §1/§2 under a `PROPOSED` header, after `0b8287f07` chartered it and
`4f06d78ae` landed SCHED-S1 — was **actioned at master** in `6fcf4be7b` (2026-08-28): the status
block now reads RATIFIED AND LANDED and its dated amendment marks §1/§2 as the pre-landing measured
bill, naming *this* section as the trap the stale present tense produced. Read that document through
its amendment. CLAUDE.md was clean on this point throughout; it carries no `QueueUserWorkItem` claim.)

So a blocking cgo call from a goroutine occupies **that goroutine's own dedicated thread** and starves
nothing. Capacity is thread-bound (~10⁴ threads), not pool-heuristic-bound. cgo inherits no
starvation pathology, and the design owes no mitigation for one. The resulting position:

1. **Default: a direct P/Invoke on the calling thread.** Correct, fast, and already the goroutine's own
   thread. This is also what a naive P/Invoke does, so it is the zero-work default.
2. **`[SuppressGCTransition]` only for short, non-blocking, non-callback-taking calls.** The thread
   stays in cooperative mode, so a long call blocks **GC suspension process-wide** — a deadlock, not a
   stall. It must be excluded for any symbol reachable by a reverse callback (Phase 5's shape), and for
   anything that can throw, block, or take a lock. Opt-in and evidence-backed, never a default.
3. **Thread-hopping is not a general answer, and thread-*per-call* is actively wrong.** Beyond costing
   ~10³–10⁴× the call it wraps, it breaks correctness: `errno` — returned by cgo's two-value form
   `n, err := C.f()` — is thread-local, and thread-affine C libraries (OpenSSL's error queue,
   `setlocale`, GTK, SQLite under `SQLITE_CONFIG_SINGLETHREAD`) require call-to-call thread identity.
   If a future case needs it, the shape is a **persistent, reused affinity thread per cgo context**,
   never a thread per call.

For reference, real Go does not mint a thread per call either: `cgocall` enters the syscall state, the
goroutine keeps its M, and `sysmon` **detaches the P** so other goroutines proceed — P-detach with M
reuse. The .NET analogue of the useful half is the GC transition a P/Invoke already performs.

### 5.4 Security posture — cgo compiles and loads attacker-authored input

Every other constraint here is a correctness constraint. This one is a trust constraint, and the
document owes it plainly: **a loaded native library is full-trust in-process code; the trust decision
is made when the module is chosen, and no generated wrapper confines it.** Three surfaces, each with
a precedent this design adopts rather than invents.

1. **Build-time flag injection.** `#cgo CFLAGS`/`LDFLAGS` in a third-party module are *input to the C
   toolchain on the converting machine*. Real Go isolates an entire file to the problem —
   `$GOROOT/src/cmd/go/internal/work/security.go`: *"We must avoid flags like -fplugin=, which can
   allow arbitrary code execution during the build."* Phase 2's `-lfoo`-only scoping accidentally
   approximates that allowlist; §7's "arbitrary `LDFLAGS`" line, taken **without** the allowlist
   doctrine, is a build-time RCE on the converting machine, and `-recurse` conversion of an untrusted
   module is exactly the exposed path. Any pass-through beyond `-lfoo` is gated on an allowlist
   modelled on Go's, not on a permissive default.
2. **Load-time library resolution.** "Per-OS naming is .NET's own native-library resolution" (Phase 2)
   delegates the search-order question, and the Windows DLL-planting surface is what it delegates.
   Generated bindings **pin** resolution — `[DefaultDllImportSearchPaths]` or an explicit
   `NativeLibrary` resolver — stated in Phase 2, not discovered in Phase 6.
3. **The boundary itself.** Nothing above confines the library once loaded; it constrains only how it
   is *chosen* and *found*. There is no sandbox in this design and none is promised.

**Recommendation:** carry 1 and 2 as Phase 2 acceptance criteria rather than Phase 7 items — both are
cheap while the emitter is being written and expensive to retrofit onto generated output. The trust
statement is documentation, and belongs wherever cgo support is announced to users.

## 6. The phased plan — base operations

### Phase 1 — Recognition, file mapping, and declaration extraction

**Precondition, not a footnote:** `CGO_ENABLED=1` with a provisioned C compiler (§5.1). Everything
below is unreachable without one.

1. **Fix the file-list zip** (§2 Finding 4) — `conversionDriver.go` adopts `testConversion.go`'s
   bounds-guarded `CompiledGoFiles` pattern. Worth doing on its own merits.
2. **Fail loudly instead of silently empty** (§2 Findings 2–3) — a package whose files were all excluded
   by build constraints, or that needs a C toolchain that is absent, must be an error naming the cause,
   not `exit 0` with `INFO: Skipping conversion`.
3. **Establish the source ↔ generated file mapping.** For a cgo package the ASTs go2cs type-checks are
   cgo's *generated* files in a build cache, not the files on disk. Everything keyed off a filename needs
   a defined answer: the emitted `.cs` name, the hand-own marker probe, and comment/licence-header
   preservation — `-comments` is mandatory for derivative works, and a generated file's comments are not
   the source's. Map each rewritten AST back to its originating source entry rather than index-zipping.

**On preamble parsing (⟨OQ-1⟩).** No go2cs `packages.Config` call site overrides `CGO_ENABLED`, and
`LoadAllSyntax` includes `NeedCompiledGoFiles` (the probe confirms the field is live). When a C
toolchain is present, `cmd/cgo` rewrites `C.foo`/`C.sometype` into typed synthetic Go declarations
(`_Cfunc_foo`, `_Ctype_sometype`, `_Cvar_x`) before `go/types` runs — the mechanism `gopls` and
`go vet` rely on to type-check cgo code without embedding a C front end. If it holds, Phase 1 needs
**no C parser** — only recognition and routing, *on top of* the driver change in step 1, which is
required regardless. **Not verifiable on the coordinator machine** (§2 Finding 1); confirm on a
cgo-capable host before scoping parser work. The fallback is a bounded parser for the common preamble
subset only, explicitly not a general C grammar.

**Exit gate:** on a cgo-capable host, a fixture loads with every `C.*` reference resolved to a
classified declaration, and each converted AST maps to the correct source filename. No C# emitted.

### Phase 2 — P/Invoke and blittable struct generation

**Goal:** generate a `LibraryImport` extern and blittable mirrors per declaration. Natural home: a new
`cgoOperations.go`, beside `directiveOperations.go`/`importOperations.go`.

| C form | Emitted C# |
|---|---|
| integer/float scalar types | fixed-width C# equivalents |
| `struct { … }` | `[StructLayout(LayoutKind.Sequential)]` blittable mirror |
| fixed-size C array member | **`[InlineArray(N)]`** (.NET 8+) — *not* a C# `fixed` buffer, which accepts only primitive element types and so cannot express `struct sockaddr addrs[4]` |
| function pointer | `delegate*<…>` |
| `char *`, `void *`, sized buffers | a blittable `void*`/`nint` extern **plus** a generated wrapper that owns the pin scope (below) |
| `#cgo LDFLAGS: -lfoo` | resolves the shared-library name; per-OS naming is .NET's own native-library resolution, **with the search order pinned** (`[DefaultDllImportSearchPaths]` or an explicit `NativeLibrary` resolver — §5.4). **Scoped to the `-lfoo` case** — `LDFLAGS` is arbitrary linker flags, and static linking is a Phase 4 problem |

**A generated mirror carries a discipline, not just a layout.** The struct-passing class was closed
proactively on the Linux `syscall` surface (`syscall/linux/structclass_linux_impl.cs`, 2026-08-28)
after the measured Uname root: a converted struct holds its `array<>` fields as **managed
references** in a GC-tracked slot, so a native write over them is heap corruption that surfaces
arbitrarily far away inside the collector — verifyheap on the crashed host reported a zeroed
MethodTable and text where pointers belong. The ruling that closed it binds this generator, because
what was a hand-owned handful of members there is *every emitted struct* here:

- The mirror is blittable and mirrors the **native** layout; the generator **never** emits a call in
  which native code writes over storage holding managed references.
- A **size assertion at the boundary**, so a wrong mirror fails the *call* loudly instead of
  producing a quiet wrong answer or a delayed corruption.
- **Explicit two-way copy-back** for out and in-out structs — seeded from the caller before the call,
  copied back after it — not a one-way marshal that silently drops kernel- or callee-written fields.
- The per-member-**when-reached** deferral that the hand-written surface could afford is **ruled out**
  for this class: "no case reached it" is a statement about coverage, not about execution.

**Two layers, not one.** `LibraryImportGenerator` marshals only types it knows; `ж<T>` and `slice<T>`
are not among them and each would need a full `CustomMarshaller`. So the generated surface is a
blittable extern taking `void*`/`nint`, wrapped by a generated method that owns the pin scope — which
is exactly what Phase 3 describes. `LibraryImport` also cannot express **varargs** at all (§7).

**One architectural constraint, stated so it is not moved later:** these declarations must be written
by **the converter, into real `.cs` files on disk** — never emitted from the `go2cs-gen` analyzer.
Roslyn source generators do not observe each other's output, so a `[LibraryImport]` declaration produced
by go2cs-gen would be invisible to the SDK's `LibraryImportGenerator` and never receive an
implementation. Converter-written files are ordinary compilation inputs. (`DllImport` remains the
fallback for any signature the generator rejects.)

**Exit gate:** an extern-function-only cgo package's Go-callable surface compiles and resolves, with
resolution **pinned** per §5.4 rather than left to the platform's default search order. Signature
generation only — no live native library required.

### Phase 3 — Pointer, string, and slice marshaling

**Goal:** `C.CString`, `C.GoString`, `C.GoStringN`, `C.GoBytes`, `C.CBytes` and `C.free` become golib
helpers over §4's mechanisms, split by direction.

Three distinct cases the first draft conflated:

1. **Go→C copy** (`C.CString`). Exists because Go strings are **not NUL-terminated and are immutable**,
   and because it `malloc`s so C *may* retain the result — not because a Go value's layout is unreadable.
2. **C→Go copy** (`C.GoString`, `C.GoBytes`). The opposite direction; copies because the Go GC cannot own
   C memory.
3. **Go→C no-copy** (`C.f((*C.char)(unsafe.Pointer(&b[0])), C.size_t(len(b)))`). The standard idiom for
   handing a `[]byte` to C. cgo's rule constrains **retention past the call**, not the handing over. This
   is a first-class case, served by `PinnedBuffer` (§4).

Real cgo's pointer rules are a programmer discipline, optionally checked at runtime (`cgocheck`). Here
the discipline can be **structural**: the generated wrapper owns the pin scope, opening it immediately
before the call and closing it immediately after, so a caller cannot violate the rule by omission.

**That only works under §4's contract, restated here because it constrains the emission, not just the
wrapper:** *the generated wrapper receives the pinnable object or a box-carrying `Pointer`
(`unsafe.Pointer.FromBox`), never a bare pre-extracted address; where Go hands it
`unsafe.Pointer(&b[0])` — case 3 above — the emission is FromBox-shaped so the wrapper can pin or
re-root.* A wrapper handed a number has nothing left to pin, and the keystone tether measured that
window closing on a live GC mark.

**Exit gate:** byte-identical round-trip in both directions through a real C function, including the
no-copy `&b[0]` idiom, with the no-copy case proven under GC pressure rather than on a quiet heap.

### Phase 4 — Inline C bodies (native side-build)

**Goal:** compile the preamble's C **definitions** with a real C compiler into a native shared library,
then bind it via Phase 2/3.

Explicitly not C→C# transpilation. §5.2 governs where the artifact may live; ⟨OQ-4⟩ carries timing.

**Two gaps that make this harder than the phase ordering suggests, both named rather than assumed:**

- **Idiomatic inline helpers are `static`, and a `static` C function exports no symbol.** The §2 fixture
  is itself an example — `static int add_ints(...)` is un-bindable by a mechanism that P/Invokes exported
  symbols. Phase 4 therefore needs a de-static-ing or shim step (generate a non-static wrapper per
  referenced `static` helper), not merely a compile step.
- **Much real-world cgo links C statically into the package** — `mattn/go-sqlite3` ships `sqlite3.c` —
  so there is **no shared library** to bind at all. Handling that means producing one, which is the same
  shim machinery pointed at a larger input, and it interacts directly with ⟨OQ-2⟩'s choice of validation
  target.

**Exit gate:** a cgo package whose preamble contains real C definitions builds and matches `go run`.

### Phase 5 — Reverse callbacks (`//export`)

**Goal:** a `//export`-marked Go function becomes an `[UnmanagedCallersOnly]` static entry point.

**Phase 4 and Phase 5 are mutually exclusive for a given file**, and the ordering must not imply
otherwise. `$GOROOT/src/cmd/cgo/doc.go:324-329`: *"Using //export in a file places a restriction on the
preamble: since it is copied into two different C output files, it must not contain any definitions,
only declarations."* Phase 5 targets declaration-only preambles; definitions live in other files.

The base mapping is favorable: cgo already restricts `//export` to top-level, receiver-less functions
with cgo-safe signatures — close to what `UnmanagedCallersOnly` independently requires. Three things
need real work:

- **Per-goroutine state.** A thread entering through a callback has none of golib's `[ThreadStatic]`
  state — the root is `t_current` (`Goroutine.cs:83-84`), read through the `OnGoroutine` property
  (`:138`), plus the defer/panic locals and the high-resolution sleep timer; `t_procId` is **not**
  golib's, it lives in `src/core/sync/runtime_impl.cs:273`. The entry point must establish that state
  as `Goroutine.Run` does. This *is* the registration work; the CLR's automatic thread attach on
  reverse P/Invoke covers the runtime's bookkeeping, not golib's. It is not invented here, though:
  **`Goroutine.Enter()` (`Goroutine.cs:176`) is an existing host-attach scope**, already used by the
  test host to run foreign threads as goroutines — the thunk composes with it and adds the panic
  boundary below.
- **Panics at the boundary.** An exception escaping an `[UnmanagedCallersOnly]` thunk is a fatal rude
  abort. golib models Go panics as `PanicException` with a Go-faithful traceback; a panicking `//export`
  function would instead abort with none. The thunk must catch and map to cgo's own crash behavior.
- **Stack divergence.** An attached foreign thread's stack is whatever native allocated, so golib's
  `DefaultStackReserve` (256 MB, `Goroutine.cs:68`) does not apply — deep recursion inside a callback
  fails differently than inside a goroutine.

**Exit gate:** a Go→C→Go round trip matches `go run`, including from a thread the C library created
itself, **and** a panic inside the callback produces Go-faithful output rather than a bare abort.

### Phase 6 — Validation

**Goal:** earn "base operations work" as a measured claim by routing a cgo package through the Phase-4
`-tests` pipeline and comparing against `go test -json`, as
[`ValidatedTestPackages.md`](ValidatedTestPackages.md) holds every other package.

`-tests` and `-recurse` cannot be combined in one invocation —
[`main.go:344`](../src/go2cs/main.go) is a hard `log.Fatalln`. A third-party cgo library needs
`-recurse`, so it cannot be converted *and* test-validated in one command. The flag error names the
intended flow — *"convert the module first, then convert its package tests individually"* — so the real
question is whether that two-step path holds for a **cgo** dependency, where step two must re-resolve
the C toolchain and preamble for a package in a converted output tree. Unproven. The roster is also
stdlib-scoped today and has no shape for a non-stdlib row. ⟨OQ-2⟩ carries the fork.

**And the host the pipeline runs is now a moving part.** The converted test host publishes
**self-contained, single-file** (`test-csproj-template.xml:81-85`: `SelfContained`,
`PublishSingleFile`, RID-pinned; trimming stays off). A cgo package's native artifact (§5.2, Phase 4)
must therefore *travel with* that host and be resolvable from it — a single-file bundle's extraction
and probing paths are not the ordinary output directory, and a native dependency that resolves during
a plain `dotnet build` can fail only when published. This is the placement half of ⟨OQ-4⟩ meeting the
pipeline: the artifact is a build output, and Phase 6 must say where the published host finds it.

**Exit gate:** at least one real cgo package validated end-to-end against `go test`, **from the
published single-file host**, not only from a build-output run.

## 7. Phase 7 (speculative) — toward 100%

Named, not estimated.

- **Full C semantic fidelity** — macros, **varargs** (which `LibraryImport` cannot express at all),
  bitfields, unions, nested anonymous structs, and `#cgo` conditional-directive syntax beyond the common
  subset.
- **`#cgo pkg-config` resolution** against arbitrary system package managers on every target. (Phase 2
  is scoped to `-lfoo` precisely so this stays here.)
- **Arbitrary `LDFLAGS`** — `-L`, `-rpath`, `-framework`, `.a` archives, `-static`. **Gated on §5.4's
  allowlist**: this bullet is a widening of what reaches the C toolchain, and widening it without the
  allowlist doctrine is the build-time RCE that section names.
- **Signal-handler coexistence.** A loaded C library that installs its own handlers — SIGSEGV for its
  own purposes, SIGCHLD, `sigaltstack` — fights both the CLR's own use of those signals and this
  corpus's live bridge, `src/core/runtime/linux/signal_posix_impl.cs` (`PosixSignalRegistration`, the
  os/signal arc, merge `9b4699ff1`). Real cgo documents this contract at length; unlike the rest of
  this section it is nameable today and it touches a banked row, so it is a *known* interaction
  awaiting scope rather than a speculative one.
- **A `cgocheck`-equivalent runtime verifier** — a debug-mode check that a generated wrapper's
  pin/lifetime discipline actually held. New machinery; nothing in golib is a starting point.
- **C++ interop** — a stretch even in real Go, which officially supports C only.
- **A cross-compilation host-toolchain matrix** (§5.1) — a provisioned cross C compiler per target
  before `-platforms` can emit cgo packages at all.

## 8. Non-goals

- **Transpiling arbitrary C into C#.** Native compile plus P/Invoke, never a second front end.
- **100% cgo fidelity as an entry criterion for Phases 1–6.** Staged on purpose, as the stdlib
  conversion itself staged.
- **Reopening goroutine scheduling.** §5.3 measured it as already solved for this design's purposes.

## 9. Adversarial pass — what review falsified

Recorded rather than silently corrected, per this repo's practice of keeping the wrongs on the record
(`DESIGN-pointer-provenance.md`'s "three recorded wrongs"). Three review rounds: the second round
falsified a claim the first round's revision had *introduced*, and the third — the ratifying review —
found one that had been wrong since the first draft and survived both revisions.

| Claim | Verdict |
|---|---|
| "`import "C"` is a hard stop — `visitImportSpec.go:211` calls `log.Fatalf`" | **WRONG, measured.** The gate never fires in either configuration; conversion silently yields nothing and **exits 0** (§2 Finding 3). A false green, which is worse than a hard stop. |
| "Goroutines run as `ThreadPool.QueueUserWorkItem`; a blocking cgo call starves the pool" | **WRONG at this document's own pinned commit** — and it survived the first revision, which fixed the recommendation while keeping the false premise. `4f06d78ae` (2026-08-13) gave every goroutine a dedicated thread and is an ancestor of `be58eb4aa`. §5.3 rewritten; the mitigation it argued for is not owed at all. |
| "Route every cgo call through a dedicated `Thread.Start`" | **WRONG on cost and on correctness.** ~10³–10⁴× the call it wraps, and it breaks `errno` (thread-local) and every thread-affine C library. Withdrawn. |
| "Real Go dedicates an OS thread to a blocking cgo call" | **WRONG mechanism.** Go does P-detach with M reuse; Ms come from a free list, never one per call. |
| "`C.CString`/`C.GoBytes` exist because a raw `string`/`[]T` header was never meant to be handed to C" | **Wrong rationale, and it conflated directions.** cgo permits passing `&b[0]`; the rule constrains *retention past the call*. `CString` exists for NUL-termination and immutability; `GoBytes` runs the other way. |
| ".NET's interop story is stronger *because* its GC compacts" | **Backwards.** Compaction is why pinning exists; Go's non-moving heap is the cheaper side. |
| "Fixed-size C array member → C# `fixed` buffer" | **Wrong for non-primitives.** `[InlineArray]` is the general answer on net10.0. |
| "The native-backed slice is the mechanism for **both directions** of buffer exchange" | **WRONG, and the first revision made it broader.** `slice.cs:431` *throws* for pinning a native-backed slice; Go→C uses the pre-existing `PinnedBuffer`. §4 now splits by direction. |
| Phase 6 validates via `-tests`; ⟨OQ-2⟩ suggests a third-party library | **Internally inconsistent** — `main.go:344` forbids the combination. Re-scoped around the two-step flow. |
| Phase 4 compiles C at convert time, "deterministic" | **Asserts the opposite of what it delivers.** §5.2 states the constraint; the `.s` precedent is invalid because Go's assembler is hermetic and an external gcc is not. |
| Phase 4 → Phase 5 ordering implies composition | **Forbidden by cgo.** `//export` restricts the preamble to declarations only (`cmd/cgo/doc.go:324-329`). |
| Phase 4 binds "exported symbols" | **Cannot bind its own fixture.** `static` helpers export no symbol; static linking leaves no library. Both now named as Phase 4 sub-problems. |
| Phase 5: "no scheduler-registration dance to replicate" | **Self-contradicted** two paragraphs later. The `[ThreadStatic]` establishment *is* the work; plus a panic-boundary and stack-reserve divergence the draft never mentioned. |
| "`SuppressGCTransition` … a way to stall the GC" | **Understated.** It blocks GC suspension *process-wide* (deadlock) and must also exclude callback-taking symbols. |
| "Nothing handles `C` today" | **Imprecise.** Three accommodations exist, all avoidance (§2 Finding 6). |
| Phase 5: golib's per-goroutine `[ThreadStatic]` state is `t_onGoroutine` and `t_procId` | **WRONG at this document's base and at master** — the ratifying review's one naming defect, wrong since the first draft. No `t_onGoroutine` exists: the root is `t_current` (`Goroutine.cs:83-84`), read via `OnGoroutine` (`:138`); `t_procId` is not golib's (`sync/runtime_impl.cs:273`). And `Goroutine.Enter()` is an **existing** attach API, so Phase 5 composes with it rather than inventing registration. |

Review also **added** findings the drafts lacked entirely: the `pkg.Syntax`/`GoFiles` zip defect (§2
Finding 4 — a latent bug independent of cgo), the source ↔ generated file mapping problem (Phase 1),
and the two-layer `LibraryImport` reality (Phase 2). The ratifying round added two whole absences —
§5.4's security posture and §1's sequencing — plus the post-fork foundations §4, Phase 2 and Phase 6
now name; those are re-groundings against a moved tree rather than falsifications, which is why they
appear as text above and not as rows here.

**Two reviewer claims were not adopted, both re-derived against source before declining them.** First,
that `packages.Package` exposes `CgoFiles` showing `GoFiles=[]` for a cgo package: that field does not
exist on `packages.Package` in the pinned x/tools v0.36.0 (it belongs to `go list`/`go/build`); the
divergence in §2 Finding 4 rests on the API's own doc comments instead, which is the stronger citation
and does not depend on a cgo-capable host. Second, the ratifying review's line-cite refresh listed the
hand-own marker probe as drifting `conversionDriver.go:244-246 → 245-249`; it has not moved.
`conversionDriver.go` is byte-identical between this document's base and master — which is precisely
why §2 Finding 4 is still live there — so the probe cite stands as written, and the other three
refreshes (`slice.cs`, `visitImportSpec.go`, `testConversion.go`) were verified individually rather
than applied as a set.

## 10. Open questions

- **⟨OQ-1⟩ Does `go/packages` already deliver typed cgo declarations?** Unresolved — **not verifiable
  on the coordinator machine** (§2 Finding 1). *Recommendation:* run the §2 probe on a cgo-capable host
  before scoping C-parser work. Note it can only ever remove the *parser*, never the Phase 1 driver fix.
- **⟨OQ-2⟩ First validation target, and does Phase 6 own the two-step flow?** Interacts with Phase 4's
  static-linking gap: the obvious third-party candidates are the statically-linked shape.
  *Recommendation:* prefer a target the `-tests` path reaches directly; if only a third-party library
  will do, scope both the two-step flow and the static-link shim into Phase 6.
- **⟨OQ-3⟩ Is any thread-hopping mechanism owed at all?** §5.3 says no for goroutines. The residual case
  is a blocking call on the *main* thread — where Go blocks too. *Recommendation:* ship nothing; revisit
  only against a measured workload.
- **⟨OQ-4⟩ Native side-build timing and artifact home.** *Recommendation:* whichever is chosen, the
  artifact is a git-ignored build output, never corpus content (§5.2). Needs a ruling, since it is the
  point where a binary would otherwise enter the tree.
- **⟨OQ-5⟩ Future of the `-cgo` flag.** Measured inert today (§2 Finding 3). *Recommendation:* keep it
  opt-in through Phase 6; revisit at Phase 6 exit.
- **⟨OQ-6⟩ Document placement — RULED (owner, 2026-08-28).** Filed as a **strategy plan** at
  `docs/PLAN-cgo-interop.md`, beside the five existing `PLAN-*.md`, rather than as a
  `phase4/DESIGN-*.md`. The `PLAN-` prefix is the operative part: per [`Glossary.md`](Glossary.md) a
  strategy plan "fixes a ruled frame — which targets, in which order — and holds its OQ rulings; it
  stays live until its ladder completes," which is exactly this document's shape, and it supplies no
  procedure (that stays in the runbooks). Question closed.

## 11. Amendment 2026-11-08 — the re-ground at master `30ad20e17e` (lane H)

*A dated block, not a rewrite. The sections above stand as written at `be58eb4aa`/`6fcf4be7b`; where this block
contradicts them, this block is the reading at `30ad20e17e`. Campaign context: the owner opened cgo foundation work
for the `.4` work queue, explicitly "laying the foundation … so that when the 1.27 hop happens, this is an item
that can be well accommodated for common use cases", and named M:N as an interacting arc. Step 0 of that campaign
is this block; step 1 is [`phase4/DESIGN-native-call-seam.md`](phase4/DESIGN-native-call-seam.md); step 2 is §11.3's
cut.*

### 11.1 §2 re-measured

**Instrument.** The same shape as §2: two fixtures (`fixonly`, one `import "C"` file with a `static int add_ints`
preamble; `fixmixed`, that file plus one plain `.go` file), a `packages.Load(LoadAllSyntax)` probe on x/tools
v0.52.0, and go2cs built from `30ad20e17e`, eight arms, each into its own output directory. The host is H, Windows,
`go version go1.26.10 windows/amd64`, with no gcc, clang or cl on `PATH` and `go env CGO_ENABLED` = `0`: §2
Finding 1, unchanged. **Predictions were written before any arm ran**, and are scored here as worded.

| Arm | Predicted | Measured | Score |
|---|---|---|---|
| P1 `fixonly`, `CGO_ENABLED` unset (go2cs pins 0) | `GoFiles=[]`, soft "build constraints exclude all Go files", WARNING + "Skipping conversion", exit 0; `-cgo=true` identical | exactly that, plus a NEW canonical line `go2cs: error GO2CS1001: build constraints exclude all Go files …`; emits only `go2cs.ico` | **MET.** An ERROR diagnostic beside exit 0: Finding 3's false green **still lives** on this route |
| P2 `fixmixed`, unset | the cgo file in `IgnoredFiles`, `plain.cs` converts, exit 0, nothing names the dropped file | exactly that | **MET** (S3's ruled "deselected stays quiet" case) |
| P3 `fixonly`/`fixmixed`, `CGO_ENABLED=1`, no compiler | S3 refuses by name, exit 1, nothing written | go list does **not** run cgo without a compiler: `CompiledGoFiles` is the RAW `add.go`, `Syntax` carries it, plus `could not import C (no metadata for C)` (GO2CS1003). The file IS paired, so S3 stays quiet, and **the visitImportSpec gate Finding 3 called dead code fires**: GO2CS2001, `log.Fatalf`, exit 1, in `fixmixed` **after `plain.cs` is written** | **MISSED on mechanism** (right exit, wrong door, and output written) |
| P3′ the same, `-cgo=true` | exit 1 too: the driver's refusal does not read the flag | **exit 0**. It emits `add.cs` + csproj + `package_info.cs`, and `add.cs` reads `using C = C_package;` / `C.add_ints(C.@int(a), C.@int(b))`: a package that does not exist, with the preamble dropped | **FALSIFIED.** "The flag is inert" (Finding 3, OQ-5) was false: `-cgo` was a **bypass** turning the gate's exit 1 into an exit-0 untyped emission |

**Finding 4: FIXED at master.** `948b9e8998` (2026-08-29, red-first in `conversionDriver_test.go`) replaced the
`pkg.GoFiles[i]` walk with `syntaxSourceFiles` (`conversionDriver.go:782-830`), which derives each tree's path
from its own UNADJUSTED token position and filters by `GoFiles` membership. It removes index pairing rather than
bounds-patching it. The one residual index pairing is `testFileEntries` (`testConversion.go:2055-2066`), against
`CompiledGoFiles` and bounds-guarded. That pairing is correct by the API's own contract, and is exposed only to the
nil-drop caveat (`packages.go:524-525`), for which no red is constructible. It is recorded and not proposed.

**Phase 1 step 2 ("fail loudly"): half landed.** S3 (`80ea656006`, 2026-09-30; owner ruling 578 item 7) refuses a
SELECTED cgo source by name before any output (`refuseSelectedCgoSources`, `conversionDriver.go:870-905`,
GO2CS2001). `d3071b1fe7` (2026-11-05) pins every go/packages load to `CGO_ENABLED=0` unless the environment SETS
it (`loaderEnvironment.go`). **That falsifies Phase 1's "No go2cs `packages.Config` call site overrides
`CGO_ENABLED`".** A cgo-capable probe through go2cs must set `CGO_ENABLED=1` explicitly, or it silently measures
cgo off. Relayed to P1's OQ-1 run.

**Finding 5 HOLDS:** no tracked `.go` file imports `"C"` (count 0 at `30ad20e17e`). **Finding 6 is now eight
accommodations, not three:** the gate (`visitImportSpec.go:270-274`), `classSkip` (`moduleConverter.go:318-319`),
the preload skip (`importOperations.go:1317-1318`), `noInitPseudoPackages` (`visitImportSpec.go:396`), S3, the
loader pin, `cgoDynamicImports.go` (`//go:cgo_import_dynamic` binding for darwin trampolines, golib
`GoCgoDynamicImports`), and `cgoUnsafeArgsLift.go` (Q56's `//go:cgo_unsafe_args` block lift). The last two are
**bindings**, not avoidance: the first time anything in the tree consumes a cgo directive rather than skipping it.

### 11.2 Every cite, re-resolved (old → `30ad20e17e`)

Content was verified at each new site. "GONE" means the cited code no longer exists.

| Cite in the text above | At `30ad20e17e` |
|---|---|
| `conversionDriver.go:89` LoadAllSyntax · `moduleConverter.go:156` · `stdLibConverter.go` | `:113` · `:247-251` · `stdLibLoadConfig` `:282` |
| `visitImportSpec.go:212-214` | `:270-274` (now also emits GO2CS2001 before `Fatalf`) |
| `conversionDriver.go:227-228` (the zip) · `:244-246` (marker probe) | **GONE** (`948b9e8998`, §11.1) · `:349-366` |
| x/tools v0.36.0 `packages.go` `:512` / `:518-519` / `:447-451` | v0.52.0 `:518` / `:524-525` / `:453-456`, text unchanged |
| `testConversion.go:1183-1187` | `testFileEntries`, `:2055-2066` |
| `moduleConverter.go:226` · `importOperations.go:1027` | `:318-319` · `:1317-1318` |
| `Roadmap.md:770-784` | the 5A/5B text is `Roadmap.md:402/421`; the completed half moved to `RoadmapHistory.md:307` |
| `ToDo.md` item 48 · `ToDo.md:49-51` | `ToDo.md:63` (an `xx)` line pointing here) · `:64` |
| `ж.NativeBox.cs:31/47/79` · `core/unsafe/unsafe.cs:368-380` | `:31/47/63` (+ `RetainedSource` `:202`) · `:361-367`, `FromBox<T>` `:500` |
| `slice.cs:430-431` | `:660-661`, the same throw text |
| `internal/runtime/syscall/linux/syscall_linux_impl.cs:105-119` | **moved under layout L3** to `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs:116-124`; it now cites the converter's own funnel hold (`convSyscallFunnelCall`, `syscallKeepAliveAnalysis.go`) |
| `PinnedBuffer.Clone` (`1d01200a9`) | `golib/runtime/PinnedBuffer.cs:174` |
| `Goroutine.cs:202-209` `new Thread(…)` | **GONE**: `Start` (`:1126-1172`) hands off to `GoroutineThreadPool` (`ded04dbdca`, 2026-10-23: threads REUSED, thread-statics reset per body under the `GoroutineThreadState` census, never multiplexed) |
| `Goroutine.cs:26` · `:83-84` · `:138` · `:176` · `:68` | `:38` · `t_current` `:122` · `OnGoroutine` `:465` · `Enter()` `:516` · `DefaultStackReserve` `:85` (plus `BoundedStackReserve`, 16 MiB, `b8587f92c5`) |
| `sync/runtime_impl.cs:273` `t_procId` | `:250` |
| `test-csproj-template.xml:81-85` | `:101-103`, plus a `TEST_READY_TO_RUN` marker. NEW: the production `csproj-template.xml:58-59` sets `PublishReadyToRun` **and `PublishTrimmed=True`** |
| `main.go:344`: `-tests` with `-recurse` is a hard `log.Fatalln` | **FALSE now**: `main.go:636-641`. `-tests -recurse` is the whole-module driver (`moduleTestsDriver.go`), plain `-recurse` only |
| `interface_windows_impl.cs` · `structclass_linux_impl.cs` · `signal_posix_impl.cs` | `net/windows/` (L3) · unchanged · unchanged path, now with a darwin flavour (`DESIGN-signal-posix-bridge-darwin.md`) and batch 257's inherited-`SIG_IGN` fix |
| `security.go` `-fplugin=` · `cmd/cgo/doc.go:324-329` · TIR §2 principle 8 | `:6` · `:339` · `TestingInfrastructureRequirements.md:40` |
| SHAs `4f06d78ae` `6fcf4be7b` `3174009a8` `a7c964d80` `1d01200a9` `9b4699ff1` `87465f5f5` `0b8287f07` | all resolve |

### 11.3 Phase 1 step 1 is spent, and the replacement cut (ruled)

Step 1 landed (`948b9e8998`), so §1's "one deliberate exception is severable" now names nothing. The re-measure
supplies three severable correctness bugs in its place, **ruled by COORD as one red-first seat landing after the
`.3` publish** on branch `claude/h-cgo-gate`:

- **(a)** a package whose every file is excluded, and whose excluded file imports `"C"` and would be selected with
  cgo on, **exits non-zero naming the cause**. This closes P1's false green. A GOROOT package is exempt: the stdlib
  converts at `CGO_ENABLED=0` by ruling. Census over std at go1.26.10, three GOOS × two GOARCH: the population is
  exactly `net/internal/cgotest` (`resstate.go`, `!netgo && cgo && darwin`) on every target, and without the
  exemption darwin would refuse it and abort a darwin `-stdlib` run;
- **(b)** a selected file paired with its OWN tree that imports `"C"` (P3's no-compiler route) is refused with S3,
  **before any file is written**;
- **(c)** the visitImportSpec gate fires **whatever `-cgo` says**. That honours OQ-5's "keep opt-in" literally: the
  flag goes back to being inert until a phase gives it something typed to emit. Retiring it stays OQ-5's later
  ruling, at Phase 6 exit.

### 11.4 §4 re-checked against what has landed since 2026-08-29

- **Layout L3: LANDED and load-bearing.** Flavour folders exist; the keystone file itself moved. §5.2 item 1
  **STANDS**: L3 models per-GOOS variance and still has no per-libc or per-SDK slot.
- **Native-backed slice: LANDED, and the family grew** (`DESIGN-native-array-view.md` RATIFIED; Q58's native-backed
  array pointer designed). The C→Go half is stronger than §4 says.
- **Pin / lifetime: now doctrine.** `.claude/rules/golib-gen.md` §"The native boundary, class 1: LAYOUT" and
  "class 2: LIFETIME", plus its pointer-identity / ordering-token section. New since the fork:
  - the converter's syscall-funnel KeepAlive (`syscallKeepAliveAnalysis.go`: Go's `//go:uintptrkeepalive` set plus
    two members the moving CLR heap adds);
  - order TOKENS for reference-bearing boxes (Q44, `ж.PointerTokens.cs`);
  - `runtime.Pinner` over the CLR heap (Q45, `a38e638fe`);
  - the native field-view door.

  §4's "Linux syscalls have one funnel to fix; **cgo has none**" is now the problem statement of
  `DESIGN-native-call-seam.md`: the tree has **four** native-call funnels, and generated cgo wrappers would be a
  fifth producer into them.
- **Single-file / R2R: the placement question STANDS and widens.** The test host is self-contained, single-file,
  RID-pinned and R2R by marker. The production template is R2R **and trimmed**, so Phase 6's native artifact must
  also keep a generated `[LibraryImport]` / resolver reachable under trimming.
- **The golib signal bridge: LANDED on linux and darwin** (`PosixSignalRegistration`), with batch 257 on top. §7's
  signal-coexistence bullet is now a two-flavour interaction.
- **Landed and load-bearing, but absent from §4:**
  - the darwin `libcCall` dispatcher (golib `GoLibcCall.cs` + `runtime/darwin/libccall_impl.cs`, a native-call
    funnel through function pointers);
  - the cgocaller keystone (`DESIGN-cgocaller-keystone.md`, `syscall/linux/cgocaller_linux_impl.cs`,
    `[UnmanagedCallersOnly]` shims);
  - a **managed `syscall.NewCallback`** (`DESIGN-managed-newcallback.md`, `syscall_windows_callback_impl.cs`). That
    is native→managed re-entry that exists today, so Phase 5's thunk has a working precedent.

### 11.5 §5.3: STALE-BY-DESIGN

§5.3's premise, "one dedicated thread per goroutine, so a blocking cgo call starves nothing", is **still true at
`30ad20e17e`**: the pool reuses threads and never multiplexes. **But it is now scheduled to become false.** The
owner has scheduled M:N after `.3`, and lane R's [`DESIGN-mn-scheduler-sizing.md`](DESIGN-mn-scheduler-sizing.md)
(PROPOSED) names native calls as its §3.7: "a goroutine in a blocking native call holds its worker", syscall handoff
is in its S3 scope, and its §4.E observes that "a physical thread blocked in a native call still counts against
`MaxThreadCount`, and nothing starts a replacement". That is Go's `entersyscall` / P-detach problem.

So §5.3 item 1 ("the zero-work default") expires with M:N, as does §8's non-goal "reopening goroutine scheduling".
Items 2 and 3 **survive and tighten**:

- under M:N a goroutine may resume on a different worker after any park, so errno must be captured on the calling
  thread **inside** the transition, before exit-native can hand control anywhere;
- thread-affine C state needs `LockOSThread`, exactly as in Go;
- a `[SuppressGCTransition]` call must never take the transition's slow path.

All three are designed in `DESIGN-native-call-seam.md` §7. OQ-3's recommendation ("ship nothing") stands for
thread-hopping and does not extend to the transition seam, which M:N owes regardless.

### 11.6 Phase 6 and OQ-2, re-opened

`-tests -recurse` is now the whole-module driver (§11.2), so a third-party cgo library can be converted **and**
test-validated in one command, plain `-recurse` only. Phase 6's "two-step flow" fork is therefore no longer forced.
OQ-2's choice of target is reopened on that basis; Phase 4's static-link gap still binds the obvious third-party
candidates.

### 11.7 ⟨OQ-1⟩

**ANSWERED 2026-11-08 by lane P1: YES, measured; recommended RESOLVED, Phase 1 needs no C parser.** Host: a linux
container, go1.26.10, gcc 13.3.0, x/tools v0.52.0, at `30ad20e17e`, with `CGO_ENABLED` set explicitly per arm (the
§11.1 loader pin would otherwise force it off). Fixture and probe: scratch ref `claude/p1-cgo-oq1-probe` @
`3d4df404b8`, `docs/phase4/data/cgo-oq1-probe/` (evidence, not for merge).

- **`CGO_ENABLED=1`** (both a `Need*` mode and `LoadAllSyntax`): `GoFiles` is `[fixture.go]`, and `CompiledGoFiles`
  is three build-cache files: `_cgo_gotypes.go`, `fixture.cgo1.go` (whose `//line` directives point back to
  `fixture.go`), and `_cgo_import.go`. No compiled file keeps `import "C"`, and every `C.x` is rewritten to a `_C`
  name. Typed declarations arrive for all five kinds:
  - a function: `C.add` becomes `_Cfunc_add func(_Ctype_int, _Ctype_int) _Ctype_int`, and `CString`, `free` and
    `GoString` are typed too;
  - a struct: `_Ctype_struct_x` with Go-typed fields, a self pointer included, and C base types as named Go types;
  - a `#define`: `_Ciconst_`, `_Cfconst_` or `_Csconst_`, as an untyped constant;
  - an enum: `_Ciconst_` values, with `_Ctype_enum_color = uint32`;
  - a typedef: `_Ctype_xptr = *_Ctype_struct_x`.
- **`CGO_ENABLED=0`**: the empty package with "build constraints exclude all Go files". §2 Finding 2 is reproduced.

**Routing caveats Phase 1's design inherits** (P1's, recorded as constraints):

- recognize by the `_C` name prefix together with the `//line` positions;
- FILTER cgo's plumbing rather than convert it: `_cgo_<hash>_Cfunc_*`, `__cgofn_*`, `_Cgo_use`/`_ptr`/`_keepalive`,
  `_cgoCheckPointer`, `_cgo_runtime_cgocall`, `runtime_throw`;
- a cgo package's syntax pairs with `CompiledGoFiles` (one `GoFiles` entry against three `Syntax` trees). Today
  `syntaxSourceFiles` deliberately skips those intermediates by UNADJUSTED path and S3 refuses the package. Phase 1
  step 3's source mapping is exactly the `//line`-ADJUSTED position that function declines to pair on, now used on
  purpose;
- `/*line*/` comments and `//go:cgo_*` directives would leak into a `-comments` emission;
- the declarations are sized to the TARGET ABI, per GOOS/GOARCH, which is the same axis as layout L3 (and §5.2
  item 1's per-libc variance still has no slot);
- a `static` C body arrives only as a typed `_Cfunc_` declaration, so its code still needs Phase 4's native
  side-build;
- the converting host needs `CGO_ENABLED=1` plus a C compiler. §2 Finding 1 stands.

### 11.8 Phase 1, increments 1 and 2 (2026-11-09, lane H)

*A dated block. The exit gate below reads live only on a host with a C compiler.*

- **Increment 1** (`cgoDeclarations.go`, over P1's checked-in fixture `src/go2cs/testdata/cgo/phase1`): recognition
  reads cmd/cgo's synthetic names off the type-checked package scope and classifies nine kinds. It filters the plumbing
  and every C name reserved for the implementation. The `//line`-ADJUSTED position of each tree's package clause maps it
  back to its source. The fixture type-checks at `CGO_ENABLED=0` with one Overlay declaration, runtime/cgo's
  `Incomplete`, which lives in a file that itself imports `"C"`.
- **Increment 2**: `-cgo` is no longer inert. It makes the single-package driver's OWN load cgo-on (`CGO_ENABLED=1`,
  last, over the loader pin) and nothing else. It is refused beside `-stdlib`, `-tests` and `-recurse`, whose loads it
  does not reach, and beside an explicit `CGO_ENABLED` other than 1. The package is still refused, with GO2CS2001 and
  nothing written, but the refusal now says what Phase 1 read: the declarations counted by kind, then each tree by
  ROLE, sorted by source: how many generated files map to each source (by base name), which trees are plain Go source,
  and how many are cmd/cgo's own. It names no generated file: the first hosted `converter-cgo` run (linux, 2026-11-10)
  read every one under a 64-hex build-cache name, so the first wording, which named them, was unreadable on any real
  host. Where cmd/cgo never ran (no C compiler, §11.1 arm P3) it says that instead. §11.3(c)'s ruling that the
  gate fires "whatever `-cgo` says" stands: the flag reaches no emission.
- **The exit gate** (`TestTheCgoFlagRecognizesTheFixtureAndStillRefuses`) loads the fixture's own source through the
  real driver under `-cgo`. It reads Inconclusive (skipped) without a C compiler, and is read live by the opt-in
  `converter-cgo` stage of the OS matrix.
