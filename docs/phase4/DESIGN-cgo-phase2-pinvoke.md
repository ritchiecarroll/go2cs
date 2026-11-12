# DESIGN — cgo Phase 2: P/Invoke externs, blittable mirrors and the generated wrapper

> **STATUS: DESIGN, for adversarial review (lane H, 2026-11-10; branch `claude/h-cgo-phase2-design`, cut from the C
> toolchain scout's tip `0fe944e517`).** No code lands from this document. It designs
> [`PLAN-cgo-interop.md`](../PLAN-cgo-interop.md) §6 **Phase 2** with the ratifying review's binding corrections
> ([`REVIEW-cgo-interop.md`](REVIEW-cgo-interop.md), corrections 3 and 4) taken as requirements, not suggestions.
> Inputs, all read at master `9b8af3bd30` unless a branch is named:
>
> - Phase 1's recognizer and mapping (`cgoDeclarations.go`; increment 1 `5ff8b33435`, rides batch 280; increment 2
>   `6dfe400637`, ACCEPTED, batch 281);
> - the native-call seam ([`DESIGN-native-call-seam.md`](DESIGN-native-call-seam.md); golib `GoNativeCall`, on master);
> - the struct-passing ruling (`src/core/syscall/linux/structclass_linux_impl.cs`);
> - the C toolchain scout ([`SCOUT-cgo-c-toolchain.md`](SCOUT-cgo-c-toolchain.md));
> - cmd/cgo and cmd/go at go1.26.10.
>
> Every choice below is a recommendation with its alternative named; §10 lists the questions for review. Amended by
> dated blocks, never rewritten.
>
> **AMENDED 2026-11-10 (§11): the adversarial review is folded in.** The record is
> [`REVIEW-cgo-phase2-pinvoke.md`](REVIEW-cgo-phase2-pinvoke.md) (`claude/coord-cgo-phase2-review` `2e859d6c9b`); the
> verdict is APPROVE WITH CHANGES, with COORD's rulings. Where §11 and the text above disagree, **§11 governs**. Each
> superseded section carries a pointer to the §11 item that replaces it. Increments 0 and 1 are released; increment 2
> may be cut on rulings §11.2 and §11.7; 3, 4 and 5 wait on the items §11.14 names.

## 1. What Phase 2 is, in one paragraph

A cgo package loads (Phase 1) as cmd/cgo's rewrite: the user's file with `C.f(…)` turned into `_Cfunc_f(…)`, plus
`_cgo_gotypes.go`, which declares every `_Ctype_*` as a Go type laid out exactly like the C type and every
`_Cfunc_*` as a Go function whose body calls a C trampoline cmd/cgo compiled (`_cgo_runtime_cgocall(_cgo_<hash>_Cfunc_add,
&p0)`, fixture `cgo_gotypes.go:116-123`). go2cs has no such trampoline. Phase 2 replaces each `_Cfunc_*` **body** with
a generated C# wrapper that crosses the native-call seam and calls a `[LibraryImport]` extern bound to the real C
symbol in a shared library. Each C struct the wrapper hands across gets a blittable mirror. Everything else in the
cgo-generated files is either an ordinary Go declaration the converter already handles (the `_Ctype_*` types,
the `_Ci/_Cf/_Csconst_*` constants) or plumbing that is dropped. **Exit gate (PLAN):** an extern-function-only cgo
package's Go-callable surface compiles and resolves, with library resolution pinned. No live library is required.

## 2. Requirements carried in (binding)

| # | Requirement | From |
|:--|:--|:--|
| R1 | Externs and wrappers are written **by the converter into real `.cs` files**, never by go2cs-gen (source generators cannot see each other's output, so a go2cs-gen `[LibraryImport]` would never get its body). | PLAN Phase 2 |
| R2 | A mirror is **blittable** and mirrors the **native** layout. The generator **never** emits a call in which native code writes over storage holding managed references. | review correction 3; `structclass_linux_impl.cs` |
| R3 | A **size assertion at the boundary**, so a wrong mirror fails the call loudly. | review correction 3 |
| R4 | **Explicit two-way copy-back** for out and in-out structs, seeded from the caller before the call. | review correction 3 |
| R5 | The per-member-when-reached deferral is **ruled out**: every emitted struct crossing gets R2-R4, reached or not. | review correction 3 |
| R6 | Library resolution is **pinned** (`[DefaultDllImportSearchPaths]` or an explicit `NativeLibrary` resolver), stated here, not discovered in Phase 6. | PLAN §5.4 item 2; review correction 4 |
| R7 | No `#cgo` flag passes through beyond `-lfoo` unless it is checked against an allowlist modelled on Go's `cmd/go/internal/work/security.go`. | PLAN §5.4 item 1 |
| R8 | **Every generated wrapper crosses the seam**: `GoNativeCall.Enter(MayBlock)` before the call and `Exit()` in a `finally`. errno is read **between** the native return and `Exit()`. No `[SuppressGCTransition]` binding ever reaches the seam. ⟶ §11.4: through the runtime forwarders. | DESIGN-native-call-seam §3.5, §4.3, §7.1 |
| R9 | Generated bindings are **build outputs, not corpus content**: git-ignored, and outside every byte-identity gate. ⟶ §11.8: converter output, excluded by path. | PLAN §5.2 |
| R10 | The output stays reachable under **trimming and R2R**; the production template trims. | PLAN §11.4 |

## 3. What the converter reads, and what it already has

Phase 1 hands Phase 2 a type-checked package. In it:

- `cgoDeclarations(pkg.Types)` classifies every `C.` name.
- `cgoSourceFiles` maps each rewritten tree to its source.
- `pkg.GoFiles` separates the user's plain files.

What Phase 2 needs and where each piece is:

- **The C layout of every C type.** cmd/cgo derives it from the compiler's DWARF and writes it into the Go type,
  explicit padding included (`gcc.go`, `Struct`). So `types.Sizes` for the target GOARCH reproduces the C size and
  every field offset, and bitfields arrive as padding. The mirror (§5) is generated from `go/types`, not from a C
  parser. ⟨OQ-1⟩ needs none, as Phase 1 measured.
- **The signature of every C function**, as the `*types.Signature` of its `_Cfunc_*` object.
- **The link flags.** cmd/cgo writes each flag it was given with `-ldflags` into the rewritten package as
  `//go:cgo_ldflag "…"` (`cmd/cgo/out.go:49-51`). The fixture's own `cgo_gotypes.go:9-10` carries
  `//go:cgo_ldflag "-O2"` and `"-g"`. cmd/go builds that list in `Builder.CFlags` through `buildFlags(…,
  checkLinkerFlags)` (`cmd/go/internal/work/exec.go:2809-2823`). So by reading, a `-lfoo` that reaches the
  rewritten files has passed Go's own allowlist, pkg-config output included (`exec.go:1824`). **Whether
  `go list -compiled` takes that path is not yet measured.** It is the first arm in §9, and R7 does not rest on it
  until it is.

### 3.1 Recognition gaps found while designing this (Phase 1 follow-up, owed red-first)

cmd/cgo mangles a name as `_C<kind>_<name>` over **nine** kinds (`cmd/cgo/main.go:114`: `iconst`, `fconst`, `sconst`,
`type`, `var`, `fpvar`, `func`, `macro`, `not-type`; `gcc.go:805-814`). It also mangles the two-result form
`r, err := C.f()` as `_C2func_<name>` (`gcc.go:1644`). Phase 1's `cgoKindPrefixes` lists six prefixes. Three that a
real package can produce are **missing**, and today `cgoDeclarations` drops each of them **silently, as plumbing**:

- `_C2func_` (the errno form);
- `_Cfpvar_fp_` (a C function used as a value, `gcc.go:1660-1680`);
- `_Cmacro_` (a function-like macro cmd/cgo evaluates through a generated C function).

`not-type` never reaches Go code. Found by reading, not by a red: the Phase 1 fixture uses none of the three. The
fix belongs to Phase 1, needs a fixture extension and a hosted `converter-cgo` red, and should land **before** Phase 2
consumes the recognizer. Until then Phase 2 could not even refuse these by name.

## 4. The emitted shape

### 4.1 Files

> ⟶ **Amended 2026-11-10:** §11.8 (the bindings file is converter output, not git-ignored) and §11.9 (one per target, placed by the layout rule).
> ⟶ **Amended 2026-11-11:** §11.20 (the header records the toolchain's first `--version` line, never `go env CC`, and
> hashes the `import "C"` sources and converted C type declarations, never raw cmd/cgo output; Phase 2's single-target
> bindings file is homed at `<goos>-<arch>/`).

Per cgo package, the converter writes:

- **The user's files**, each from its rewritten tree and named for its source (Phase 1's mapping): `fixture.go` →
  `fixture.cs`. Call sites stay as cgo wrote them (§4.4).
- **One bindings file**, `<package>.cgo.g.cs`, in the package's output folder. It holds the `_Ctype_*` type
  declarations (ordinary converted Go types), the constants, the mirrors, the externs and the wrappers. cmd/cgo's
  own two files are **not** converted file-for-file. Their plumbing (`_Cgo_use`, `_Cgo_always_false`,
  `_Cgo_keepalive`, the `__cgofn_*` linknames, `//go:cgo_import_static`, the `runtime/cgo` blank import) has no C#
  meaning and is dropped. This is the same set Phase 1's recognizer already filters.

The bindings file is a build output (R9): the converter writes it, a `.gitignore` entry beside the package keeps it
out of git, and no CNR or corpus diff ever sees it, because the corpus is wholly `CGO_ENABLED=0` (PLAN §2 Finding 5).
Its header records the target (GOOS/GOARCH), the C toolchain that produced the layouts (`go env CC` and its
`--version` line), and a hash of the cgo-generated inputs. That way two bindings files can be told apart by
provenance, not only by content.

### 4.2 One function, end to end

> ⟶ **Amended 2026-11-10:** §11.4 (the seam bracket goes through the runtime forwarders) and §11.3 (errno).

For `int add(int, int);` declared in the preamble and `#cgo LDFLAGS: -lfoo`:

```csharp
// <package>.cgo.g.cs (generated; build output, not corpus)
partial class cgofixture_package
{
    [LibraryImport("foo", EntryPoint = "add")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial int add_ꓸnative(int a, int b);

    // The `_Cfunc_add` cmd/cgo declared, its trampoline body replaced.
    public static _Ctype_int _Cfunc_add(_Ctype_int p0, _Ctype_int p1)
    {
        int r;
        GoNativeCall.Enter();
        try { r = add_ꓸnative((int)p0, (int)p1); }
        finally { GoNativeCall.Exit(); }
        return (_Ctype_int)r;
    }
}
```

- **The extern takes primitives only**, chosen from `types.Sizes`: a 4-byte signed integer is `int`. The named
  `_Ctype_*` structs never appear in an extern. They are blittable today, but a signature that depends on go2cs-gen's
  layout of a named type is one generator change away from a marshalling surprise. C `long` needs no special case:
  cmd/cgo already gives `_Ctype_long` the target's width, 8 bytes on linux and darwin, 4 on windows, so the extern
  follows `go/types`.
- **The calling convention is stated, never defaulted.** `LibraryImport`'s default on windows/x86 is `Winapi`
  (stdcall), and C functions are cdecl. The attribute costs nothing on 64-bit targets, where there is one convention.
- **The seam** (R8): `Enter()` defaults to `MayBlock`, because every wrapper is the `cgocall` of its call site.
- **The extern's name** carries a mark (shown as `ꓸnative`), so it cannot collide with a Go identifier. The exact
  glyph follows go2cs's existing collision-suffix convention.

### 4.3 The errno form (`_C2func_`, once §3.1 lands)

> ⟶ **Amended 2026-11-10:** §11.3.

`r, err := C.f(x)` binds the same symbol with `SetLastError = true`. The wrapper reads
`Marshal.GetLastSystemError()` **after the extern returns and before `GoNativeCall.Exit()`**, which is the seam's
§4.3 rule. It then returns `(r, err)` with `err` nil when the value is 0, and `syscall.Errno(value)` otherwise, as
cgo does.

### 4.4 What converted user code shows

> ⟶ **Amended 2026-11-10:** §11.17 (RULED: the visible `C.add(C.int(a))` form is the target).

Recommendation: user code reads **as cgo rewrote it**, `_Cfunc_add(_Ctype_int(a), _Ctype_int(b))`. That is the text
the type checker resolved, and it is what Go's own toolchain compiles.

The alternative is to print the source's `C.add(C.int(a), C.int(b))` back, through a nested static class `C` that
declares the C-named members. That reads more like the Go the user wrote. It has a cost: the `_Ctype_*` types would
become nested types (`C.@int`), and go2cs-gen's handling of `[GoType]` structs nested inside a static class is
unmeasured. ⟨Q1⟩ for review, with the plain form as the safe default.

## 5. Mirrors: the struct-passing discipline at generated scale

### 5.1 The mirror

> ⟶ **Amended 2026-11-10:** §11.2 (the assertions), §11.9 (per target) and §11.13 (flexible arrays, bitfields, `_Bool`).

For each `_Ctype_struct_*` (and `_Ctype_union_*`, which cmd/cgo gives as a byte array) that any wrapper hands
across, the generator emits:

```csharp
[StructLayout(LayoutKind.Explicit, Size = 40)]         // types.Sizes: Sizeof(_Ctype_struct_x)
internal unsafe struct _Ctype_struct_x__ꓸmirror
{
    [FieldOffset(0)]  public int a;                     // Offsetsof, field by field
    [FieldOffset(8)]  public double b;
    [FieldOffset(16)] public _InlineArray8_sbyte name;  // [8]_Ctype_char
    [FieldOffset(24)] public byte flag;
    [FieldOffset(32)] public nint next;                 // *_Ctype_struct_x: a native address
}
```

- **Explicit layout, with every offset and the total size from `go/types`.** cmd/cgo already made those offsets the
  C offsets (§3), so the mirror is the C layout **by construction**, whatever C#'s sequential packing would do.
  `structclass_linux_impl.cs` uses `Sequential` with hand-checked fields. A generator has no hand to check with, so it
  takes the offsets from the same source cmd/cgo used.
- **Inline arrays are `[InlineArray(N)]`, never `fixed`** (PLAN §9): an array of structs or of pointers must work too.
  One `_InlineArrayN_T` helper per (N, T) the package uses, emitted once.
- **Every pointer member is `nint`**: a native address, never a managed reference (R2). Nested structs nest their
  mirrors. A union is the byte array cmd/cgo gave it, at its offset, with the containing struct's size keeping its
  alignment honest.
- **No managed reference anywhere in a mirror.** A generator assertion refuses to emit one that would hold one, and
  the struct then has no Phase 2 crossing.

### 5.2 The crossing

> ⟶ **Amended 2026-11-10:** §11.1 (BLOCKER: one element wide) and §11.7 (by value, RULED).

Which parameter shapes Phase 2 covers:

| Parameter or result | Wrapper does |
|:--|:--|
| struct by value | copy the Go struct into a mirror local; pass the mirror by value |
| struct result | receive a mirror; copy it into a new Go struct |
| pointer to a struct the Go side holds **in managed memory** | copy into a mirror **local** (stack, so it cannot move during a blocking call); pass its address; **copy back unconditionally** after the call (R4) |
| pointer to a struct in **native** memory (a `NativeBox`, e.g. from `C.malloc`) | pass its address. No mirror: native memory already has the C layout. The address comes through the keystone contract (PLAN §4): the wrapper receives the box, never a pre-extracted number, and keeps it alive across the call. |
| nil pointer | pass 0 |

**Why unconditional copy-back is correct here.** C carries no direction annotation (cmd/cgo drops `const`), so the
wrapper cannot know whether `p` is in, out or in-out. A mirror seeded from the caller and copied back is an identity
copy when C wrote nothing. That is exactly `structclass_linux_impl.cs`'s argument for its read-modify-write members.
It does not depend on modelling the callee.

**Why the temporary is legal, and why it is cgo's rule rather than ours.** A mirror local's address dies when the
call returns. cgo's pointer-passing rules already forbid C from keeping a Go pointer after the call returns
(`cmd/cgo/doc.go`, "Passing pointers"). So C's view through the temporary is exactly what Go permits. Native memory
(the `NativeBox` row) is passed through untouched, because C *may* keep it.

**The copy-in rule for pointer members** is where Go's own runtime check becomes structural. cgo forbids passing Go
memory that contains a Go pointer to unpinned Go memory: `cgocheck` panics with `argument of cgo function has Go
pointer to unpinned Go <kind>` (`runtime/cgocall.go`, `cgoFormatErr`). So copying a struct into its mirror maps
each pointer member like this:

- nil → 0;
- a pointer into **native** memory → its address;
- a pointer into **managed** memory → the wrapper panics **with cgo's own message**, before any native code runs.

What Go checks at run time under `GODEBUG=cgocheck=1` (the default), the generated wrapper checks always. That
cannot be turned off, and the cost is one box-kind test per pointer member.

**Copy-out of pointer members** keeps identity where it can. Address 0 → nil. The same address that member was
copied in from → the **original** `ж<T>`, untouched. Any other address is C memory → a native-backed `ж<T>` over it,
through the existing native mint.

**Aliasing.** If the same `ж<T>` is passed twice in one call, the wrapper builds **one** mirror and passes its address
twice, so C sees one object, as it does under Go. A wrapper that built two would let C write one copy and the
copy-back of the other overwrite it.

> **AMENDED 2026-11-10 (H's increments 3b2a and 3b2b; COORD, RULED): sameness is the storage's, and a returned argument
> IS the argument.**
>
> - **Aliasing is decided by STORAGE, not by box.** Two boxes can reach one struct (a `FieldRefBox` and the box of
>   the struct it is a field of, or two field boxes over one field), so a later pointer parameter of the same type
>   shares an earlier one's mirror when `Unsafe.AreSame` holds over their `Value` refs, and the struct is copied back
>   once. The same rule covers a pointer to one scalar. Arm: windows' `UnionRect(&r, &r, &o)` writes the union into r
>   through the shared mirror; two mirrors would copy r's stale second mirror back over it.
> - **A pointer RESULT that equals a pointer argument's address is that argument's box** (RULED). `gmtime_r(&t, &tm)`
>   returns `&tm` in Go. Here the address C returns is the wrapper's stack mirror, which dies when the wrapper
>   returns, so minting it native would hand back a use-after-return. The wrapper compares the result with each
>   same-typed pointer argument's address and returns that argument's own box; any other result is C memory, minted
>   native. Red (3b2b, measured on hosted linux and darwin at `3dad6ea07b`): with every result minted, "gmtime_r
>   returns the pointer it was handed" failed.

### 5.3 The size assertion, and what it can and cannot catch (R3)

> ⟶ **Amended 2026-11-10:** §11.2 (the assertion above is tautological).
> ⟶ **Amended 2026-11-11:** §11.20 (the run-time target check reads `ProcessArchitecture`, not `OSArchitecture`,
> and lands in increment 5b).

Each mirror carries a static check that runs once, before its first crossing:

```csharp
Unsafe.SizeOf<_Ctype_struct_x__ꓸmirror>() == 40
```

40 is the converter's `types.Sizes` reading. On a mismatch it throws by name: the struct, both sizes, and the target
and toolchain recorded in the bindings file header.

- **It catches** a generator defect: a wrong `InlineArray` length, a member emitted at a wider type, a missed union.
- **It cannot catch** a library compiled against **different headers** from the ones the converting host's C
  toolchain read. That is PLAN §5.2's boundary, measured by the scout: recognition follows the target's C headers,
  and the data model already splits `long`.

So the bindings file's static constructor also compares the running OS and architecture
(`RuntimeInformation.OSArchitecture` and `OperatingSystem.Is*`) with the target its header recorded, and refuses by
name when they differ. A linux binding run on windows would otherwise give silently wrong layouts. Pinning the C
toolchain (the scout's `-cc pinned`) is what makes the recorded headers reproducible. Nothing makes them match a
library someone built elsewhere, and the note says so rather than implying a check that does not exist.

## 6. Library resolution, pinned (R6) and allowlisted (R7)

### 6.1 Which library: the load-time name is captured at convert time

> ⟶ **Amended 2026-11-10:** §11.5 (per symbol, not per library) and §11.12 (darwin install names).

`-lfoo` is a **link-time** name. The name the loader needs at run time is different on every system:

- **ELF:** the library's `SONAME`, e.g. `libfoo.so.1`. Only a `-dev` package installs the bare `libfoo.so` link a
  `dlopen("foo")` would find.
- **Mach-O:** the dylib's install name.
- **Windows:** `-lfoo` names an **import library** (`libfoo.dll.a`), and the DLL it records can be named anything.
  mingw's `-lm` resolves into the UCRT.

A Go cgo binary gets this right because its linker writes the load-time name into the binary (`DT_NEEDED`,
`LC_LOAD_DYLIB`, the import table). go2cs links nothing.

**Recommendation:** capture the load-time name **at convert time with the same C toolchain** (§5.1's precondition,
the scout's pin). Link a one-line shared object against each `-l` the allowlist admits, with the package's `-L`
paths, and read the name its dynamic section, load commands or import table records. Write that name into the
`[LibraryImport]` and the resolver. The converter already needs the toolchain to load the package, and this asks it
one more question, the same one Go's linker answers.

The alternative is to resolve `foo` at run time by searching for `libfoo.so*` or `*foo*.dll`. That is a glob over
directories, which is the opposite of pinned. ⟨Q2⟩.

### 6.2 Where it is loaded from

> ⟶ **Amended 2026-11-10:** §11.11 (a resolver miss throws; assembly-level search paths).

Each converted package is its own assembly, so it can carry its own resolver. It is registered from a
`[ModuleInitializer]` with `NativeLibrary.SetDllImportResolver`. A module initializer is rooted, so trimming keeps it
(R10), and `LibraryImport` is source-generated, so AOT needs nothing more.

The resolver loads the captured name only:

- **Windows:** `NativeLibrary.TryLoad(name, assembly, DllImportSearchPath.SafeDirectories |
  DllImportSearchPath.AssemblyDirectory)`. That covers the application directory, System32 and directories added with
  `AddDllDirectory`. **Never the current directory or `PATH`**, the DLL-planting surface §5.4 names.
- **Linux and darwin:** the system loader with the captured `SONAME` or install name. That is exactly what a Go cgo
  binary's `DT_NEEDED` or `LC_LOAD_DYLIB` asks the same loader for, with the same `rpath`, `LD_LIBRARY_PATH` and
  cache rules, so the trust model is the Go binary's and no wider. The current directory is never searched.
- **A miss** throws `DllNotFoundException` naming the `-l` flag, the captured name and the places tried.

### 6.3 What passes through (R7)

> ⟶ **Amended 2026-11-10:** §11.15 (a plain token, defined; CGO_LDFLAGS is unchecked upstream).

Phase 2 reads `-l<name>` and `-L<dir>` from the `//go:cgo_ldflag` lines (§3), and nothing else. Every other flag is
**refused by name**, until a later phase has a use for it and an allowlist to check it against. Two things keep that
from being a false comfort:

- the flags arrived through cmd/go's `checkLinkerFlags`, which §9 arm 1 is to confirm for `go list -compiled`;
- Phase 2 checks `-l`/`-L` values itself: a name that is a plain token, and a directory that exists. Whatever the
  first point turns out to be, Phase 2 never hands an unchecked flag to a tool.

## 7. What Phase 2 refuses, by name

> ⟶ **Amended 2026-11-10:** §11.6 (`_Cvar_` and `_Cfpvar_fp_` refused) and §11.13.

> **AMENDED 2026-11-10 (COORD, RULED, for increment 3b2b): single scalar pointee; element pointers remain Phase 3.** A
> pointer to ONE scalar crosses under §11.1's rule, with a stack copy-in and an unconditional copy-back, exactly as one
> whole struct does. That covers a `StandardBox<scalar>` (`&t`) and a `FieldRefBox` whose pointee is a scalar field
> (`&s.f`), since cgo scopes that memory to the field alone. An element pointer (`&xs[i]`, an `ElemRefBox`) is still
> refused to Phase 3, for scalars as for structs, with its own red. The table row below therefore narrows to element
> pointers and `unsafe.Pointer` into managed memory.

Phase 1 increment 2 refuses every cgo package and says what it recognized. Phase 2 **narrows** that refusal: a
package converts only when every recognized declaration is in Phase 2's set. Otherwise the refusal lists each
declaration that is not, with the phase that owns it:

| Declaration or shape | Owner |
|:--|:--|
| `C.CString`, `C.GoString(N)`, `C.GoBytes`, `C.CBytes`, `C.malloc`/`C.free` used for Go memory (cmd/cgo's `builtinDefs`, `out.go:1810`) | Phase 3 |
| a pointer parameter into managed **non-struct** Go memory (`&b[0]`, `unsafe.Pointer`) | Phase 3 (the pin scope, under §4's FromBox contract) |
| a function **defined** in the preamble, `static` or not (no exported symbol to bind; the Phase 1 fixture's `add` and `scale` are both `static`) | Phase 4 |
| `_Cmacro_` | Phase 4 (needs compiled C) |
| `//export` | Phase 5 |
| `complex`, `long double`, `__int128` parameters (ABI rules unverified per target) | Phase 2 later, measured first |
| variadic C functions | never: cgo itself refuses them (`cmd/cgo/doc.go:257`) |

**Telling "defined in the preamble" from "declared, provided by a library"** is ⟨Q3⟩. Recommendation: compile the
preamble once with the provisioned toolchain (`-c`, as cmd/cgo already does) and read the object's symbol table. A
symbol **defined** there, local or global, is Phase 4. One **undefined** there is what Phase 2 binds. It needs no C
parser, and the toolchain is already present.

## 8. Under M:N, and what Phase 2 does not change

The wrapper's only scheduler contact is the seam bracket (R8; ⟶ §11.4, through the runtime forwarders), so M:N changes nothing here. The seam's
contract already covers a goroutine that resumes on another worker. The rest also stands as designed:

- errno is read inside the transition (§4.3);
- a thread-affine C library needs `runtime.LockOSThread`, as in Go (seam §7.3);
- `#cgo nocallback` adds only a debug assertion that no `ReenterManaged` occurs (seam §7.2);
- `#cgo noescape` is Phase 3's pin-scope hint;
- no binding is ever `[SuppressGCTransition]` (seam §7.1, and its guard).

## 9. Increments, each a seat, red-first

> ⟶ **Amended 2026-11-10:** §11.10 (the exit gate's arms) and §11.14 (sequencing).

0. **Phase 1 follow-up (§3.1):** recognize `_C2func_`, `_Cfpvar_` and `_Cmacro_`. Extend the fixture; the red runs on
   hosted `converter-cgo`.
1. **Arm, no code:** does `go list -compiled` run cmd/go's `checkLinkerFlags`? Predicted YES, by §3's reading. Plant
   `#cgo LDFLAGS: -fplugin=x` in a hosted fixture. If the load refuses it, R7 rests on Go. If not, R7's own check
   (§6.3) carries it alone, and the record says so.
2. **Mirrors (§5):** generated from `go/types`, with the size assertion and the target check. A compile-only test over
   every struct shape in an extended fixture: inline arrays, a nested struct, a union, pointer members and padding.
   The red is a mirror whose size disagrees, refused by name.
3. **Externs and wrappers (§4):** scalar and struct-by-value or by-pointer parameters through the seam. The red is a
   managed pointer member copied in, which must panic with cgo's own message before the extern runs.
4. **Resolution (§6):** load-time names captured at convert time; the module-initializer resolver. This is the
   PLAN's exit gate: an extern-only package against a system library every host has (libm/UCRT `cos`; `div` for a
   struct by value) compiles and resolves on hosted linux, darwin and windows. A planted copy of the library in the
   **current directory** must NOT be the one loaded.
5. **The narrowed refusal (§7)** replaces Phase 1's blanket refusal once 2-4 hold.

## 10. Questions for review

> ⟶ **Amended 2026-11-10:** §11.17 (Q1 RULED), §11.8 (Q4 answered) and §11.7.

- **⟨Q1⟩ Visible call sites.** cgo's rewritten names (`_Cfunc_add`, `_Ctype_int`), or the source's `C.add` through a
  nested static class `C`? The plain form is the safe default; `C.` reads more like the Go but depends on unmeasured
  go2cs-gen behaviour.
- **⟨Q2⟩ Load-time names.** Captured at convert time by a link probe (recommended), or searched at run time?
- **⟨Q3⟩ Defined or provided.** The preamble object's symbol table (recommended), or a narrower rule such as
  "anything with a body in the preamble"? That would need a C parser, which ⟨OQ-1⟩ retired.
- **⟨Q4⟩ The bindings file's place.** `<package>.cgo.g.cs` beside the converted files, git-ignored (recommended), or a
  separate generated project the package references?
- **⟨Q5⟩ Unconditional copy-back.** It is correct for every direction, and for a large struct it copies twice per
  call. Is a later `const`-preserving hint worth a DWARF read? Not in Phase 2.
- **⟨Q6⟩ The target check (§5.3).** Refuse on an OS/architecture mismatch at run time (recommended), or trust the
  layout L3 flavour the package was built under?

The review this note asks for is adversarial: §5.2's copy-in rule, §6.1's capture and §7's discriminator are the
claims most likely to be wrong, and each names the arm that would show it.

## 11. Amendment 2026-11-10 — the adversarial review, folded in

The review ([`REVIEW-cgo-phase2-pinvoke.md`](REVIEW-cgo-phase2-pinvoke.md), `claude/coord-cgo-phase2-review`
`2e859d6c9b`) approved with changes. COORD ruled findings 2, 7, 9 and 18. Each item below names the review finding it
answers, and where it disagrees with §1-§10, it governs. The text above stays as written, so the record shows what
the review corrected.

### 11.1 BLOCKER: the stack mirror is one element wide (finding 1)

§5.2's managed-struct row is wrong for a pointer to an element. Under cgo's pointer rules, when Go passes a pointer to
an element of an array or slice, the Go memory in question is the entire array. So `C.f(&xs[0], C.int(len(xs)))` is
legal Go, and C may read and **write** `n` consecutive structs. A one-struct stack mirror turns that native write into
a stack smash.

**Revised:** the mirror row admits only a pointer whose pointee is **one whole struct**: a `StandardBox<T>`, or a
`FieldRefBox` whose field is the whole struct. An `ElemRefBox` of a struct element is **refused by name** to Phase 3,
where the pin scope covers the whole backing array. **Red** (increment 3): a wrapper handed `&xs[1]` refuses before
the extern runs.

**The native row has a second exception.** A `NativeBox<T>` minted FROM MANAGED STORAGE by a reinterpret-fallback arm
carries a managed address or token, not a native address, and its retained-source slot names the box it came from
(`src/core/golib/ж.NativeBox.cs`, the class remarks: "the wrapper copies from the retained source and never uses the
address"). "Pass its address" would hand C a token to write through. **Revised:** a `NativeBox` whose retained source
is set, or which aliases an order token, is never passed by address. It is refused by name, and the retained source is
not silently copied either, because §11.1's one-struct rule cannot tell whether C writes past it. Only a `NativeBox`
with no retained source, a genuinely native address, passes through. **Red** (increment 3): a wrapper handed a
reinterpret-minted box refuses before the extern runs.

### 11.2 The size assertion, made able to fail (finding 2; RULED)

§5.3's check cannot fail: under `LayoutKind.Explicit` with `Size = N`, `Unsafe.SizeOf` returns N by construction, and
`[FieldOffset]` makes the offsets true by definition. **Ruled:** keep explicit layout, and assert what the generator
could get wrong:

- for every mirror field, `sizeof(clrType) == types.Sizes.Sizeof(goType)`;
- for every `[InlineArray(N)]`, its length is N and its element size is the Go element's size.

The assertions run once per mirror, before its first crossing, and refuse by name: the struct, the field, both sizes.
**Red** (increment 2): a planted UNDERFLOW, an `int` member where the Go type is 8 bytes, is refused naming that
field. It is planted in increment 2's own test, not in any converter path: the mirror for a test fixture struct with a
C `long` member (8 bytes on the LP64 hosts the test reads) is generated with that one member's CLR type forced to
`int` through a test-only override, and the emitted check must refuse it naming the struct and the member. §5.3's second half stands: no assertion can see a library built against other headers. The run-time OS and
architecture check stays.

### 11.3 errno (finding 3)

§4.3 named the wrong API. The `_C2func_` binding is `[LibraryImport(…, SetLastError = true)]`, whose generated stub
clears the error before the call and captures it after. The wrapper reads `Marshal.GetLastPInvokeError()` between the
extern's return and the seam's exit. That reproduces cgo's own trampoline: errno set to 0, the call, the capture.
`GetLastSystemError()` after the stub returns could read a value the GC transition clobbered. Plain `_Cfunc_` bindings
stay `SetLastError = false`.

A C function returning `void` also has a two-result form. cmd/cgo gives it the result `(_Ctype_void, error)`, so the
wrapper returns the zero `_Ctype_void` and the error.

> **AMENDED 2026-11-10 (COORD, RULED, on H's increment 3b1 finding): the capture above holds on linux and darwin ONLY.**
> On windows `SetLastError` captures the Win32 `GetLastError`, while C's `errno` is the C runtime's own thread slot, a
> different one. Measured on H against `ucrtbase.dll`: through a `SetLastError` binding, `exp(1000.0)` left
> `GetLastPInvokeError()` at 0; with the C runtime's errno zeroed first and read right after, the same call read 34
> (`ERANGE`). A read made after a further P/Invoke read 0, so intervening runtime work can clobber the slot.
>
> **Ruled, for windows targets:** the errno form binds the function WITHOUT `SetLastError`, plus the LIBRARY'S C runtime
> `_errno()` (an `nint` slot). Inside the bracket, the wrapper:
>
> 1. takes the slot's ADDRESS with one `_errno()` call BEFORE the native call;
> 2. writes 0 through it;
> 3. calls the function;
> 4. reads through the same address as the very next statement, with no P/Invoke between the call's return and the read.
>
> That is cgo's own trampoline (zero errno, call, capture) in managed code. The slot is the calling thread's, and one
> synchronous wrapper does not change threads; a GolibTests arm holds the address equal before and after a wrapped call.
> The C runtime is per library: `ucrtbase.dll` for UCRT builds (llvm-mingw ucrt, zig), `msvcrt.dll` for msvcrt-built
> DLLs. Increment 4's probe records it from the import table. Linux and darwin keep `SetLastError` plus
> `GetLastPInvokeError()`. The generator takes the target GOOS (§11.9). **Remaining caveat:** runtime code on the same
> thread between the stub's return and the read could still touch the C runtime's errno; Phase 4's native shim closes
> it.

### 11.4 The seam bracket goes through the runtime forwarders (finding 4)

§4.2 called `GoNativeCall` directly, and that is not the ruled shape. DESIGN-native-call-seam §12.2 Q2 ruled Go's
shape: the `g`-status move happens in the runtime's entersyscall family, which then calls `GoNativeCall` exactly once
per transition. A wrapper that skipped the runtime would show `_Grunning` where Go shows `_Gsyscall` (F-2,
reintroduced).

**Revised:** the wrapper brackets with `runtime_package.syscallEntersyscall()` and `syscallExitsyscall()`
(`src/core/runtime/syscall_managed_impl.cs:85/89` at master). **Red** (increment 3): an `[UnmanagedCallersOnly]` probe
reads `GoCurrentGStatus()` (`:165`) and `GoNativeCall.TransitionDepth` from inside the call, and must see the syscall
status and depth 1.

**The red's route (RULED).** A `[LibraryImport]`-bound wrapper calls a C symbol, and a C symbol cannot reach a managed
probe unless it is handed one. S-b's tests worked because `cgocaller` takes a function pointer. So the instrument is a
**probe library** exporting a call-through, `void call_through(void (*fn)(void *), void *arg)`, which calls `fn(arg)`.
The wrapper under test binds it, and the probe is the function pointer. The library lands IN increment 3, built by the
`converter-cgo` leg's own C compiler (gcc, MinGW gcc or Apple clang), without waiting for the pinned toolchain, which is
still the owner's decision.

> **AMENDED 2026-11-10 (COORD, RULED, on H's proposal):** the seam-status red uses **libc's `qsort`**, not a built
> probe library. A `[LibraryImport]`-bound `qsort`, called in the generated wrapper's shape, calls back an
> `[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]` comparator from inside the bracket. There is no C build
> step, and the red runs in GolibTests on every `golib-tests` leg and on H. Three conditions:
>
> - **No "exactly once".** glibc's `qsort` is a mergesort with a malloc'd buffer and may change its comparison count. The
>   red asserts at least one comparator call, and reads `GoCurrentGStatus()` and `TransitionDepth` on **every** call.
> - **A negative control, so the red can fail:** the same `qsort` bracketed by `GoNativeCall` DIRECTLY, with no
>   `syscallEntersyscall`, must read `_Grunning`.
> - **The library, per OS:** `libc.so.6` on linux, `libSystem.B.dylib` on darwin, `ucrtbase.dll` on windows. It is a
>   test binding in GolibTests, not a generated one, so §11.11's owned-name resolver rule does not apply to it.
>
> The probe library stays only for increment 4's windows two-`int` by-value struct (§11.7).

### 11.5 Load-time names are captured per SYMBOL (finding 5)

§6.1 captured a name per `-l`, and that is wrong in two ways:

- **Windows:** mingw's `-lm` resolves `cos` to `api-ms-win-crt-math-*` and `div` to `api-ms-win-crt-utility-*`, both
  API-set forwarders. Some libm symbols are static in `libmingwex.a` and import from nothing.
- **ELF:** `--as-needed` drops a library no symbol references, so a probe that references nothing records nothing.

**Revised:** the probe links one shared object that **references every symbol Phase 2 binds**. It then reads the
symbol-to-library map from the import table (PE), or `LC_LOAD_DYLIB` with the two-level namespace (Mach-O). On ELF,
`DT_NEEDED` plus `.dynsym` versioning maps only VERSIONED symbols to a library, so the probe asks the linker instead:
`-Wl,--trace-symbol=<sym>` for each bound symbol (or a `-Map` file), falling back to one probe per `-l` where a
linker lacks it. It writes one `[LibraryImport]` per symbol, naming that symbol's library. A symbol
resolved statically, or not at all, is **refused by name**.

> **AMENDED 2026-11-10 (H's increment 4b, measured on hosted converter-cgo legs; COORD, RULED):**
>
> - **A windows API set is recorded AS LINKED** (Q3). For mingw's `cos` the probe's import table names
>   `api-ms-win-crt-math-l1-1-0.dll`, which is what a Go binary built by the same toolchain imports; the OS loader
>   resolves the set. Recording today's host DLL instead would bake in a mapping Windows is free to change.
> - **The ELF fallback is one probe PER SYMBOL, not per `-l`.** COORD's earlier "per -l fallback" is superseded: each
>   bound symbol gets its own probe, referencing that symbol alone and linked `--as-needed` against every `-l`. The
>   `DT_NEEDED` entry it adds over a probe that references nothing is its library, recorded exactly as a Go binary's
>   linker records it. It needs no file lookup and no tracing. zig refuses `-Wl,--trace-symbol` ("unsupported linker
>   arg") and cannot be relied on to locate its stub libraries, so for zig this IS the route. Under GNU ld, the
>   two-library fixture holds the trace route and this fallback to one answer.
> - **Measured:** GNU ld prefixes its trace lines with its own name, and zig links `--as-needed`, so a probe that
>   references nothing records nothing (§6.1's one-line probe, the red). Under zig, windows' `cos` resolves
>   STATICALLY (zig's mingw libm), and is refused by name, this section's own example; the host mingw gcc resolves it
>   to `api-ms-win-crt-math-l1-1-0.dll`.

### 11.6 `_Cvar_` and `_Cfpvar_fp_` are refused (finding 6)

§7 gave both no owner. Phase 1 has recognized `_Cvar_` from the start, and increment 0 (`d20526ee99`, seated in
batch 281) added `_Cfpvar_fp_`, so neither is dropped silently. Phase 2 **refuses each by name** until Phase 2-later:

- `_Cvar_` (a C global) would be `NativeLibrary.GetExport` into a `NativeBox<T>`;
- `_Cfpvar_fp_` would be the export's address as an `unsafe.Pointer`.

A converted managed zero standing in for a C global is worse than a refusal.

### 11.7 Structs by value: integer- and pointer-only (finding 7; RULED)

The CLR classifies a by-value struct for the native ABI from the **mirror's** field types, so it can disagree with
the C compiler: a union carried as bytes, or a `float` inline array (an HFA on arm64). **Ruled:** Phase 2 passes and
returns by value only structs whose members are integers and pointers, with no union and no inline array. Floats,
unions and inline arrays by value are "Phase 2 later, measured per RID", and refused by name until then. The exit
gate gains `inet_ntoa(struct in_addr)` as the by-value **argument** arm on linux and darwin, beside `div` for the
by-value result. **On windows `in_addr` is a union** (`S_un`), which this ruling refuses, so windows takes the probe
library's (§11.4) exported two-`int` struct as its by-value argument arm instead (RULED).

> **AMENDED 2026-11-10 (COORD, RULED, on P2's question; §11.19):** a struct of 16 bytes or less with an eightbyte that
> no member covers (dropped bitfield bytes, or tail padding) is also refused by value. Under SysV x64 the CLR may
> classify that eightbyte NO_CLASS where the C compiler says INTEGER. Conservative, until an exit-gate arm lifts it.

### 11.8 The bindings file is converter output (finding 8; answers Q4)

§4.1 and R9 called it a git-ignored build output. It is not: the pinned toolchain makes it at convert time, and a
checkout cannot regenerate it without that toolchain. **Revised:**

- `<package>.cgo.g.cs` is converter output beside the converted files, committed with them by whoever converts;
- byte-identity gates exclude `*.cgo.g.cs` **by path**;
- the converter never writes `.gitignore` entries into a user's tree.

R9's substance stands: the corpus is wholly `CGO_ENABLED=0`, so no corpus gate ever sees one.

> **AMENDED 2026-11-10 (COORD, RULED, on H's increment 3b1 note): the bindings file is `<package>.cgo.cs`, NOT
> `<package>.cgo.g.cs`.** Committed converter output cannot carry a suffix that common `.gitignore` files drop, and
> `*.g.cs` is one: this repository's own `.gitignore` ignores it at line 348, which is why increment 3b1's GolibTests
> goldens are named `GoCgoWrappers.<os>.golden.cs`. The converter never writes `.gitignore` entries into a user's
> tree, so the name itself has to survive. Byte-identity gates exclude the bindings file by its `.cgo.cs` suffix.

### 11.9 Mirrors vary per (GOOS, GOARCH): placed by the layout rule (finding 9; RULED)

`_Ctype_char` is `int8` on x64 and `uint8` on linux/arm64, so a mirror set is a property of the target, not of GOOS
alone. **Ruled:** bindings follow the four-target LAYOUT RULE from the linux/arm64 work. The converter emits bindings
per target and homes each file at its coarsest cover, tried in the rule's order over its four keys:

- **flat** (the package root), when the mirror set is identical on all four targets, as a set with no `long`, no
  `char` and no pointer-width difference is;
- **`<goos>/`**, when it is identical across a GOOS's targets (`linux/` covers linux/amd64 and linux/arm64);
- **`<arch>/`**, when it is identical across one GOARCH's targets (`amd64/`, while it has two or more);
- **`<goos>-<arch>/`**, one target, when it differs from every other (`linux-arm64/` for a `char` that is unsigned
  there).

A set shared by targets no single key covers is split as the rule splits any file. The header's target record (§4.1) names the cover, and the
run-time check (§5.3) compares the running OS and architecture against it.

### 11.10 The exit gate exercises more than scalars (finding 10)

§9 increment 4's gate gains four arms:

- **copy-back:** `uname` on linux and darwin, `GetSystemTime` on windows, each through a struct-pointer mirror;
- **errno:** `strtol` of an out-of-range string, reading `ERANGE` through the two-result form. Not `sqrt(-1)`: darwin's
  libm is `MATH_ERREXCEPT`, so it may leave errno 0;
- **a REAL current-directory arm:** a library that exists ONLY in the current directory must throw
  `DllNotFoundException`, while the same library in the application directory loads;
- **a pinned-zig probe library**, built per target by the scout's provisioned toolchain, as the instrument for
  aliasing, unions and HFA classification.

> **AMENDED 2026-11-10 (COORD, RULED, on H's increment 4b reading): G1's windows arm is NOT `cos`.** Under the pinned zig
> (the owner's default toolchain, PLAN §5.1), mingw's libm links `cos` statically, so a cgo package calling `C.cos` on
> windows is refused by name (§11.5). The windows arm takes a symbol that BOTH the host mingw gcc and the pinned zig
> resolve dynamically (a UCRT string or stdio function, or a kernel32 one), confirmed by increment 4b's own capture on
> both toolchains before G1 relies on it.
>
> **RULED 2026-11-10 (COORD, on H's capture readings, windows converter-cgo jobs 42924514691 gcc and 42924536206 zig):
> G1's windows arm is `puts`.** It resolves to `api-ms-win-crt-stdio-l1-1-0.dll` under both the host mingw gcc and
> the pinned zig, and its output is checkable for G2. `GetCurrentProcessId` (`KERNEL32.dll` under both) is the named
> kernel32 alternative. Rejected, as static under zig: `cos` (libm) and `strlen`, which the host gcc resolves to
> `api-ms-win-crt-string-l1-1-0.dll`. A cold zig cache cost 140 s per probe target on the hosted windows runner, which
> goes into G1's leg budget; a pinned cache-warming step on the leg is allowed.

### 11.11 A resolver miss throws (finding 11)

§6.2's resolver could return `IntPtr.Zero`, and the runtime then falls back to default probing. That is legacy search
order, current directory included, on windows. **Revised:**

- the assembly carries `[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories |
  DllImportSearchPath.AssemblyDirectory)]`, so even a fallback is pinned;
- for a name it owns, the resolver throws `DllNotFoundException` on a miss, and never returns 0;
- it returns 0 only for names it does not own (hand-owned imports in the same assembly).

**Red** (increment 4): a deleted library throws, naming the `-l`, the captured name and the places tried.

### 11.12 darwin install names (finding 12)

An install name of `@rpath/…`, `@loader_path/…` or `@executable_path/…` cannot be resolved by the CLR's loader as Go's
binary resolves it. **Revised:** record the absolute path the probe's linker resolved, or refuse by name. `-framework
X` is admitted, with its install name captured. As with a Go binary, a library outside the system loader's path needs
`LD_LIBRARY_PATH` on linux; the note says so to users and does not paper over it.

> **MANDATORY 2026-11-10 (increment 4d, a SECURITY finding caught before any release shipped it; COORD ruled the fix
> into 4d itself):** the absolute-name rule binds EVERY darwin load, not only the capture's preference. dyld's search
> for a BARE name reached the current directory on both hosted darwin RIDs: golib-tests 42936395011 planted a copy of
> the probe library in the current directory, asked the resolver for it by its bare name, and dyld loaded it (handles
> 1913759472 on osx-arm64 and 1108903152 on osx-x64), where the arm required a refusal. Both halves now refuse by name
> any darwin name that is not absolute: the converter's capture (`cgoDarwinInstallName`, red 8dfe7398ee, green
> 3c4875b988) and golib's resolver, which refuses before dyld sees the name. `ALibraryOnlyInTheCurrentDirectoryIsNotLoaded`
> stays as the rule's guard. The probe library is built at the fixed absolute install name
> `/tmp/go2cs-cgo-probe/libgo2csprobe.dylib`.
>
> **The portability consequence, and it is the intended trade:** a captured absolute install name is baked into the
> bindings, as it is into a Go binary's load commands. A library installed at a different path on the consumer's
> machine, say Homebrew's `/opt/homebrew/lib` on Apple silicon against `/usr/local/lib` on Intel, fails by name, with a
> DllNotFoundException naming the `-l`, the captured name and the rule. It does not load whatever a search would find.
> A package that must load from more than one location is converted on, or for, the layout it targets.

### 11.13 Flexible arrays, bitfields and `_Bool` (findings 13 and 14)

- A **flexible array member** (`T[]`, which cmd/cgo gives as `[0]T`) is **refused by name**: such a struct only ever
  lives in native memory, and a mirror cannot hold its tail.
- **Bitfields** are dropped by cmd/cgo, not padded with named fields, so those bytes round-trip as opaque bytes in the
  mirror and the Go side cannot read them. That is Go's behaviour too.
- **C `_Bool`** maps to `byte` in mirrors and externs. A C# `bool` marshals as a 4-byte `BOOL`.

### 11.14 Sequencing (COORD)

- Increments 0 and 1 are released now.
- Increment 2 (mirrors) is cut on §11.2 and §11.7.
- Increment 3 (wrappers) waits on §11.1 and §11.4.
- Increment 4 (resolution, the exit gate) waits on §11.5, §11.10, §11.11 and §11.12.
- Increment 5 (the narrowed refusal) waits on §11.6, §11.8 and §11.9.

### 11.15 `-l` and `-L` as plain tokens; CGO_LDFLAGS is unchecked upstream (note 15)

§6.3's reliance on cmd/go's allowlist is narrower than it read. `CGO_LDFLAGS` from the **environment** reaches
`//go:cgo_ldflag` without `checkLinkerFlags`, so Phase 2's own check is load-bearing, not a backstop. A "plain token"
is defined:

- `-l<name>`: the name matches `[A-Za-z0-9_.+-]+`, does not start with `-` or `@` (as cmd/go's own check refuses),
  and has no path separator; the GNU `-l:file` form is refused;
- `-L<dir>`: an absolute path, or one relative to the package directory, that exists;
- `-framework <name>`, **darwin only** (RULED; cmd/go's allowlist admits it): the name a plain token as above, its
  install name captured as §11.12 says. On any other GOOS it is refused by name.

> **AMENDED 2026-11-10 (COORD, RULED, on H's increment 4b): `-O2` and `-g` are admitted, token-exact, and set aside.**
> They are cmd/go's default `CGO_LDFLAGS`, so every cgo package's `//go:cgo_ldflag` lines carry them (the Phase 1
> fixture shows exactly those two), and neither names a library. The admission is token-exact: `-O3`, `-g3` and every
> other flag beyond `-l`/`-L`/`-framework` stay refused by name.

Everything else is refused by name, whatever increment 1 finds about `go list -compiled`.

### 11.16 Divergences, stated (notes 16 and 17)

- A struct member pinned with `runtime.Pinner`, and a struct that points at itself, are refused by name. That is
  **narrower than Go**, which accepts a pinned Go pointer.
- A `uintptr` member that carries a managed-pointer token is invisible to the copy-in check. Go's cgocheck is equally
  blind to it.
- A union holding pointers is bytes, as cmd/cgo gives it.
- The cost of the seam bracket is on par with Go's `cgocall` bracket. §8 on blocking, affinity and signals stands as
  the review read it.

> **AMENDED 2026-11-11 (COORD, RULED, on the review of increment 5's design, M1; §11.20):** five divergences of
> cmd/cgo's pointer check, four of them with a function of their own in `src/go2cs/testdata/cgo/phase2/fixture.go`:
>
> - **Go panics, Phase 2 proceeds:** a pointer into package-level memory through the variable form
>   (`PackageLevel`; `runtime/cgocall.go:772-777`), and a pointer to a field whose heap object holds another Go pointer
>   (`Sibling`; `:751-768`). C is handed a stack mirror of the pointee alone, so the memory Go's scan guards is
>   unreachable from C.
> - **Go proceeds, Phase 2 refuses:** a member pointing at `runtime.Pinner`-pinned memory (`Pinned`; `:738`, the first
>   line above); every check under `GODEBUG=cgocheck=0` (`:565`), which Phase 2 has no switch for (no fixture function:
>   it is an environment setting); and a by-value struct whose pointer member points into pointer-free Go memory
>   (`ByValue`; Go passes `top` through struct fields, `:714-726`, and Phase 2's copy-in refuses a managed pointer).

### 11.17 Q1 RULED: the visible `C.` form is the target (note 18)

Converted call sites read `C.add(C.@int(a), C.@int(b))`, through a nested static class `C` that declares the C-named
members and types, per the owner's readability and parity rulings. The mangled form (`_Cfunc_add`, `_Ctype_int`) is a
fallback only if the arm fails. The arm measures go2cs-gen on `[GoType]` structs nested in a static class, and it
lands with increment 3. Its red: a converted call site that prints a mangled name.

> **AMENDED 2026-11-10 (COORD, RULED, on H's increment 3c measurement): the `C.` form is a PSEUDO-PACKAGE, not a nested
> class.** Measured with scratch projects against go2cs-gen, each arm read from the compiler's emitted sources:
>
> - a type nested in a static class `C` inside the package class is never generated. The converted-type rule
>   (go2cs-gen `Common.cs`, `IsConvertedGoTypeDeclaration`) requires the declaration's DIRECT parent to be the
>   `*_package` class;
> - with an explicit `[GoType]` it is generated into the WRONG type: `GetParentClassName` (`TypeGenerator.cs`) names
>   `C`, and the code lands in a namespace-level `C_package`;
> - a sibling top-level package class, with `using C = <class>;` in the converted file, builds with NO go2cs-gen change.
>   That is the alias form converted imports already use (`using errors = errors_package;`).
>
> **Ruled:** in Go, `C` IS a pseudo-package, so the bindings are a package class of their own, named
> **`<pkg>ꓸC_package`**. It uses U+A4F8, the converter's own imported-alias separator. The name ends in `_package`, as
> the converted-type rule needs, and it can never collide with a real Go package's class: `ꓸ` is not a legal Go
> identifier character, and Go reserves `import "C"`. The emitter's comment states both facts. Two conditions:
>
> - **accessibility** follows `package_info`'s rule. cgo forbids a C type in an exported Go signature, so expect
>   `internal` throughout, and the emitter says so;
> - **each file that imports `C` gets the alias**, and a file that does not never sees the class.
>
> Nested containment plus a go2cs-gen fix is REJECTED: it is bigger, not needed, and every template's package-class
> wrapper would have to learn it. Increment 3b emits this shape, with the arm committed as its test. §11.17's red is
> unchanged: a converted call site that prints a mangled name.

### 11.18 Amendment 2026-11-10 (second): the check of §11

COORD's Fable check of §11 (a dated amendment to `REVIEW-cgo-phase2-pinvoke.md`, `claude/coord-cgo-phase2-review`
`8a30cab3e6`) read 13 findings MET, 5 PARTIAL, sequencing MET and nothing contradicted. Increment 2 is released. The
fixes, made in place above, each gate the increment named:

- **RULED, increment 4:** §11.15's grammar admits `-framework <name>`, darwin only.
- **RULED, increment 3:** §11.4's red needed a route. It is a probe library exporting a call-through, built by the
  `converter-cgo` leg's own compiler, landing in increment 3.
- **RULED, increment 4:** §11.10's `inet_ntoa` arm runs on linux and darwin. Windows' `in_addr` is a union, so windows
  takes the probe library's two-`int` struct instead.
- **PROMOTED, increment 3:** §11.1 refuses a reinterpret-minted `NativeBox` (retained source set, or an order token),
  with its own red.
- **SHOULD, increment 4:** §11.5 traces ELF symbols through the linker, since `.dynsym` maps only versioned ones.
  §11.10's errno arm is `strtol` and `ERANGE`, since darwin's `sqrt(-1)` may leave errno 0.
- **SHOULD, increment 5:** §11.9 names all four covers; an all-target-identical mirror set lands flat.
- **SHOULD:** §11.15 refuses a leading `-` or `@` after `-l`. §2's R8 and R9 and §8's first sentence point here.
  §11.6 states that `_Cvar_` was recognized from the start. §11.2 says where its underflow is planted.

### 11.19 Amendment 2026-11-10 (third): P2's second read of increment 2

P2 second-read increment 2 (`claude/h-cgo-phase2-mirrors` `16c71c2886`) against §11.2 and §11.7, with H's own dotnet
test. COORD ruled each item; each was fixed red-first on the same branch:

- **MATERIAL, §11.2: a nested mirror's members were never checked.** A nested mirror's size is its own explicit
  `Size`, so the outer member's size check and every inline-array element check over a struct are tautological:
  finding 2's class, one level down. A nested mirror never crosses on its own, and the outer check never called the
  nested one. P2 measured it: `inner.b`, a C `int`, generated as `short`, read ACCEPTED through `shapes`. **Fixed:**
  each mirror's check now calls the check of every mirror a member is, or holds through inline arrays, once each.
- **RULED: the generated methods carry the `ᴍ` marker.** A C member named `CheckLayout` or `Hold` collided with the
  generated methods (CS0102). They are now `CheckLayoutᴍ` and `Holdᴍ`. As with the type suffix, a C-derived name is
  ASCII, so it can never be a marked name.
- **RULED, §11.7: an all-padding eightbyte is refused by value** (the dated line under §11.7).

### 11.20 Amendment 2026-11-11: increment 5, as reviewed

H's design delta for increment 5 (§9 item 5, the narrowed refusal) was reviewed adversarially before any red. COORD
ruled on the review: no BLOCKER, and four MUST-FIX items, all inside the seat. This section is the increment's design
as ruled. Evidence cites are go1.26.10 file:line.

**What 5 is.** Under `-cgo`, a package converts when every C use it makes is in Phase 2's set. Otherwise the refusal
lists each use that is not, with the phase that owns it, writes nothing and exits non-zero. Increment 5 is two seats.
5a is the converter route, gated offline over checked-in cmd/cgo output. 5b is the real route: the capture through
`go env CC`, the link-flag check and §5.3's target check, gated on the hosted legs. Building and running the converted
output against `go run` is G1's.

**The check closure, and what unwrapping it claims (M1).** cmd/cgo rewrites a call whose pointer argument's pointee
holds a pointer into a closure that calls `_cgoCheckPointer` before the call (cmd/cgo `gcc.go` `rewriteCall`). Phase 2
unwraps the non-deferred closure to the direct call `C.f(…)`. The claim is NOT that the wrapper checks what Go
checks. It is: **the mirror hands C only the pointee, so Go's wider scan guards memory C cannot reach.** The two
forms differ:

- the ADDRESS form, `_cgoBaseN := &x; …; _cgoCheckPointer(_cgoBaseN, 0 == 0)`, checks the pointee's members only
  (`runtime/cgocall.go:580-590`). That is what `GoCgoArgs.PointerMember` checks (`GoCgoArgs.cs:80-88`).
- the VARIABLE form, `_cgoN := p; _cgoCheckPointer(_cgoN, nil)`, scans the whole heap object (`cgocall.go:618`,
  `:728-768`) and panics for any pointer into data or bss (`:772-777`).

**RULED (COORD, 2026-11-11, on H's measured proposal): the variable form is admitted, and its two Go-panics cases are
DISCLOSED divergences** (§11.16, `Limitations.md`). `TestCgoCheckClosureFormsAcrossGorootsCgoPackages`
counted each form in the generated trees of GOROOT's 29 cgo packages (hosted linux, go1.26.10, run 42973745743):

| Form | Count |
|---|---|
| address | 31 |
| variable | 50 |
| index | 22 |
| slice | 0 |
| defer | 9 |
| go | 0 |

The variable form is the majority among the closures that do not take element memory: 50 of 81. `plugin` reaches
the address form only (2 sites). `net` reaches the variable form only (4 sites), because its resolver passes its
pointers through as parameters (`cgo_unix_cgo.go:76`, `:84`), and that is ordinary cgo style. Most of `net`'s sites
also pass a `void *` or a `T **`, which Phase 2 refuses anyway (N2), so the case for admitting the variable form rests
on the census, not on `net`. The rest are `cmd/cgo/internal/test`.

In both cases where Go's wider scan panics and Phase 2 proceeds, C is handed a stack mirror of the pointee alone,
never the Go object. The memory Go's check guards (the rest of the heap object, or a data/bss variable C might retain)
is unreachable from C. Only a program that already breaks cgo's pointer rules can observe the difference, and Go
itself proceeds under `cgocheck=0`. Refusing the form would refuse correct programs to catch incorrect ones.

**NAMED DEFERRAL, not a promise:** a run-time equivalent of the variable form's scan is Phase 2 later. Nothing
schedules it, and it opens only if a program is found that depends on the panic.

**A correction to the review, measured.** cmd/cgo DOES write a closure for a struct passed by value with a pointer
member. The fixture's `ByValue` gets the variable form, `_cgo0 := link; _cgoCheckPointer(_cgo0, nil)`
(`fixture.cgo1.go`, generated at go1.26.10), so the census's variable count includes by-value arguments. The divergence
itself stands: Go accepts a pointer member into pointer-free Go memory, and Phase 2's copy-in refuses a managed
pointer member.

**Fail-closed is a census (M2, S4).** `_cgoN`, `_cgoBaseN` and `_cgoCheckPointer` are ordinary identifiers in a
type-checked tree. A shape matcher that misses a closure would convert it to a C# lambda calling a dropped function,
which is a build error AFTER files are written. So the refusal is a census over `types.Info.Uses`: every use of an
object declared in cmd/cgo's types file must be consumed by a recognised shape. Those shapes are a call-site rename,
a C type or constant reference, or a part of an unwrapped closure. Any other use refuses BEFORE any write, naming its
site. Its negative controls (safety floor 13) mutate the checked-in output, and each must refuse naming its site:

- an extra statement in a closure;
- a check on the wrong `_cgoN`;
- final-call arguments that are not exactly `_cgo0`…`_cgoN-1`, in order.

The corpus stays byte-identical only because the `/*line*/` drop and the parenthesis drop are gated on cgo-tree
identity, and the prediction says so.

**Shapes (M3, N2).** RECOGNISED:

- the non-deferred closure, in its address and variable forms;
- `var _cgoN T = arg` for an unchecked argument (`gcc.go:988-995`), emitted as `T`'s conversion of `arg`, never the bare
  initializer;
- a closure nested in another's initializer at any depth (`gcc.go:1189-1203`), unwrapped recursively;
- the two-result closures `func() (R, error)` and `func() (_Ctype_void, error)` (`gcc.go:939-969`);
- a result-less closure as a statement;
- an unparenthesised callee inside a closure, `/*line*/` anywhere, including between `return` and the callee, and
  after `:=`;
- `import _cgo_unsafe "unsafe"` (`gcc.go:1461-1468`);
- cmd/cgo's parentheses around EVERY non-type reference (`gcc.go:1583-1586`). A parenthesis is dropped only when its
  operand resolves to a cgo object.

REFUSED by name:

- the deferred closure, `defer func() func() {…}()()`: Phase 2 later;
- the goroutine closure: `Deferred` is set only for `ctxDefer` (`ast.go:276`), so cmd/cgo writes `go func() {…}()`
  and the arguments are evaluated INSIDE the new goroutine: Phase 2 later;
- the index and slice forms (`_cgoIndexN`, `_cgoSliceN`): Phase 3.

`_cgoBaseN` under conversions (`gcc.go:1287-1305`), such as `(*C.T)(unsafe.Pointer(&goStruct))`, unwraps and reaches
`GoCgoArgs.StructPointer`'s refusal of a reinterpret-minted box (§11.1). That refusal has an arm.

`void *` and `T **` always get a closure (`gcc.go:1129-1131`), and the wrapper already refuses both
(`cgoStructWrappers.go:227`). So the closures inside Phase 2's set are exactly those whose argument is a pointer to a
struct with a pointer member.

`/*line*/` comments are line directives and shift ADJUSTED positions. Anything placed by position in a cgo tree uses
`PositionFor(pos, false)` (N3). A refusal names the source site, which is the adjusted position.

**File roles (S2).** On the real route a tree's role is read from content AND provenance:

- the package has an `import "C"` GoFile;
- the tree opens with cmd/cgo's header (`out.go:82`, `:707`);
- the tree is in CompiledGoFiles and not in GoFiles;
- two trees mapping to one source refuse.

The tree whose `//line` names an `import "C"` source converts under that source's name. cmd/cgo's types file hosts the
bindings file. `_cgo_import.go` is dropped before the dynamic-import collector sees it, and any other cmd/cgo tree
refuses. Content-only recognition is allowed only for the offline tests, where checked-in output is loaded as plain
files.

**The header (M4; §4.1 amended).** The bindings file's header records:

- the target;
- the C toolchain's FIRST `--version` line, and NEVER `go env CC`'s value, because an absolute path would break the
  nicknames-only rule;
- a sha256 over the `import "C"` SOURCES and the C type declarations converted, normalised to LF.

It hashes no path, no link flag and nothing from `_cgo_import.go`. Raw cmd/cgo output is machine-dependent: its
`//line <srcfile>:1:1` is absolute (`out.go:713`), its `//go:cgo_ldflag` carries the environment's `CGO_LDFLAGS`,
`-L` paths included (`out.go:50`), and `_cgo_import.go` names the host libc's symbol versions.

**Targets (S1, S3 RULED; answers §11.18's SHOULD for Phase 2).** `-cgo` is single-target in Phase 2, and multi-target
`-cgo` is Phase 2 later.

- The bindings file is homed at `<goos>-<arch>/`, which is always truthful, never flat: flat means identical on all
  four targets (§11.9). The project takes the layout rule's conditioned include for that folder (`platformLayout.go`).
- "Host" is the load's `go env GOOS`/`GOARCH`, never go2cs's own `runtime.GOOS`. A `-cgo` target other than the host
  refuses by name. linux/arm64 bindings need an arm64 host (char signedness, §11.9).
- §5.3's run-time check reads `RuntimeInformation.ProcessArchitecture`, not `OSArchitecture`. Otherwise a Rosetta or
  Windows-on-Arm x64 process falsely refuses (`26186c1739` already chose ProcessArchitecture for GOARCH). It lands in
  5b: `GoCgoLibraries.Register` takes the target, and `Resolve` throws by name on a mismatch.

**Arms.** `testdata/cgo/phase2/fixture.go` gives cmd/cgo every shape above, from the system C library and the probe
library (§11.10). Its generated files are cmd/cgo's own output, made on a hosted linux/amd64 leg with the pinned zig
toolchain and committed as Phase 1's were.

- N1: `Ordered` passes a by-value struct and then an argument that changes it. Go copies the struct when the argument
  is evaluated, and the converted call must too.
- Four of §11.16's five divergences have a fixture function each. The fifth, `GODEBUG=cgocheck=0`, is an environment
  setting.

**Seam coverage, a standing property (COORD, for M:N; increment 5b).** Every native call a binding makes is inside
runtime's `syscallEntersyscall`/`syscallExitsyscall` bracket (§11.4), so a scheduler that watches the bracket sees every C
call. Each extern belongs to one wrapper with one bracket. The single exception is windows' `_errno` slot, which only
the errno form calls, inside its own bracket. golib's resolver loads a library only when a bracketed call first binds
its import, and the module initializer makes no native call. `TestCgoBindingsCrossTheSeamOnEveryNativeCall` holds this
over the goldens: the externs equal the brackets plus the errno slot (linux 15/15, darwin 15/15, windows 17 = 16 + 1 at
increment 5b), and no extern call sits outside a bracket. Its own negative control removes one bracket's opening and
must name the call.

**The host rule (increment 5b; S3).** `-cgo` converts for the host's own target, read before the load as `go env
GOHOSTOS`/`GOHOSTARCH` in the load's environment, and any other target refuses naming both sides. golib's resolver
holds converted bindings to the target they record (`GoCgoLibraries.Register`'s additive target overload;
`TargetMismatch` compares the running GOOS and the PROCESS architecture). So the GolibTests goldens are per
architecture where a leg's process differs: darwin's arm64 leg compiles `GoCgoWrappers.darwin-arm64.golden.cs`, which
the generator writes for darwin/arm64 like the others.

**A class 5b's legs found: an offline seam whose fixture names differ from a real host's (COORD named it).** The
offline route loads checked-in cmd/cgo output as files named `.go`. A real host's `go list` hands back the same trees
under build-cache names with no `.go` suffix, and the driver's build-constraint re-check refuses such a name
(`isFileNameCompatible`). So every tree of the real route was dropped silently, with exit 0 and no bindings file,
which no offline gate could see by construction. Fixed in 5b (3d4c3236b1): the route's trees are not re-checked, since
go list selected their source, and a cgo package whose bindings file was not written fails the conversion by name.
The general rule: where an offline seam stands in for a real route, any property of the real inputs the seam changes
(here, a file's name) is a gap only the real route closes, so the real-route legs are part of the gate.

A second member of the class, from the review below (M1): the offline seam loads cgo-off, so `go list` never selected a
plain file under `//go:build cgo`, and no offline test could see the driver drop one. The seam's load now carries
`-tags=cgo`, which selects such a file as a cgo-on load does and still leaves an `import "C"` source out.

**The first real-route readings (converter-cgo, increment 5b).**
- The Phase 2 fixture's own refusal (its deferred and goroutine calls) passed on real cmd/cgo output first at
  43147447241 (linux, zig).
- With those two calls removed, the fixture converts on linux (zig), darwin (zig, both RIDs) and windows
  (llvm-mingw), at 43151538896, 43151546974 and 43151555690. The captured tables are:
  - linux: `libc.so.6`, `libm.so.6` (`-lm`) and `libgo2csprobe.so`;
  - darwin: `/usr/lib/libSystem.B.dylib` and `/tmp/go2cs-cgo-probe/libgo2csprobe.dylib`;
  - windows: `api-ms-win-crt-math-l1-1-0.dll`, `api-ms-win-crt-utility-l1-1-0.dll` and `go2csprobe.dll`.
- llvm-mingw's C runtime resolves `mktime` statically, so windows refuses it by name at its use.
- No build-machine path appears beyond the captured library names (a darwin install name is absolute by rule).
- With the probe linked, `go list` hands back `_cgo_import.go` on all three. That answers §11.20's UNMEASURED cause:
  `-dynimport` reads a binary linked from the preamble, which links now that the probe library defines the four
  probes.

Each divergence is a tested decision, not an accident (COORD's condition on the M1 ruling). In 5a, each of the four
fixture functions converts, and the converted call is asserted. Their run-time behaviour is G1's, and goes on **G1's
checklist** as these run-time arms, each asserting go2cs's actual behaviour beside `go run`'s:

- `PackageLevel` and `Sibling` PROCEED in go2cs (Go panics);
- `Pinned` REFUSES in go2cs with the copy-in message (Go proceeds), and the same arm is run again under
  `GODEBUG=cgocheck=0`, where Go proceeds and go2cs still refuses;
- `ByValue` REFUSES in go2cs with the copy-in message (Go proceeds);
- `Reinterpreted` REFUSES in go2cs at `GoCgoArgs.StructPointer` (§11.1);
- `Ordered` passes the pair as it was before `mutate` ran;
- `Nested`, `TwoResults` and `Statement` each match `go run`.

**`_cgo_import.go` is missing from the fixture, cause UNMEASURED.** `go list -compiled` handed back none for
`fixture.go` (run 42973745743), where it does for Phase 1's. The likely cause is that `-dynimport` reads a binary
linked from the preamble, and nothing here defines the four probe functions. 5b PREDICTS that once `probe.c` defines
them and the fixture links the probe library, `-dynimport` writes `_cgo_import.go` again. If it does not, that is a
finding.

### 11.21 Amendment 2026-11-12: the adversarial review of increment 5b

COORD had 5b reviewed adversarially at `568fde8424` and held it from batch 287 until three MUST-FIX items closed, each
red first (`c1ec22f56a`). This section records each item as fixed.

**M1: a plain file under `//go:build cgo`.** The driver's build-constraint re-check skipped only cmd/cgo's trees, so a
plain sibling the cgo tag selects was dropped with exit 0 and its declarations dangled. The re-check now carries the
`cgo` tag for every plain file of a cgo package, as `refuseCgoOnlyPackage` already did.

**M2: a stale bindings file.** The "bindings file was not written" gate was a bare `os.Stat`, which an earlier
conversion's file satisfies when this conversion's write fails. The gate now reads a flag that only this conversion's
own successful write sets.

**M3: a name a header renames (RULED: the verified-names rule).** The capture's probe declares each symbol with no
header, so it finds a renamed function under its plain name: darwin/amd64's `fstat` is `fstat$INODE64`, a different
function over a different `struct stat`, and glibc may make `strtol` the symbol `__isoc23_strtol`. A binding to the
plain name would call a function C never calls, silently.

- **The rule:** a symbol binds only when cmd/cgo's own binary imports it under its plain C name. The names are the
  local names of `_cgo_import.go`'s `//go:cgo_import_dynamic` lines (`cgoImportedByCmdCgo`). Any other symbol is
  refused at its use, naming what the binary imports that contains its name.
- A package with symbols to bind and no `_cgo_import.go` is refused too: its C does not link as go builds it, so
  nothing verifies a name.
- The capture stays the library oracle and runs first, so the static and Phase 4 refusals keep their text.
- **The cost, accepted for .4:** cmd/cgo compiles its wrappers at `-O2`, so a call the C compiler inlines as a builtin
  imports nothing and is refused like a rename. `abs` and `labs` are the fixture's. The refusal a user sees:
  `C.abs: cmd/cgo's own binary does not import a symbol named abs, so nothing shows that name is the function C calls
  here: a header renames it, or the C compiler inlined the call as a builtin. Binding the plain name could call a
  different function, so it is refused; Phase 2 later`. A refusal by name is a disclosed limit; a silent wrong binding
  is a defect. The real-route test reads which calls each host refuses this way and fails on any outside
  `{abs, labs}`.
- **NAMED DEFERRAL (post-.4, a BOARD row):** a probe compiled WITH the package's preamble, reading each reference's real
  symbol from the object. It lifts the builtin refusals and binds a renamed name soundly. It needs the package's
  `CgoCFLAGS` and three object readers (ELF, Mach-O, COFF).

**SHOULD items, all fixed in 5b.**

- **S1:** when the probe does not link, a probe that references nothing is linked first. If that fails too, the fault
  is the link line or the compiler, and the linker's own error is the refusal. No symbol is blamed as Phase 4's.
- **S2:** the probe carries the target's architecture flags as cmd/go hands them to CC (`gccArchArgs`, mirrored in
  `cgoArchFlags` for amd64 and arm64), so a compiler whose default architecture differs cannot answer for another.
- **S4:** the header's toolchain line keeps printable ASCII only and is capped at 120 characters
  (`cgoToolchainLine`). A bare CR, U+0085, U+2028 and U+2029 each end a C# line, and the text is whatever CC printed.
- **S5:** golib's resolver holds the target for every import the assembly makes, before the table is searched, so a
  hand-owned import beside bindings for another target is refused too.
- **S6:** the real-route test reads `GO2CS_REQUIRE_CGO_PROBE` first and checks the compiler go itself would use
  (`go env CC`), so a leg that loses its compiler fails rather than skips
  (`TestTheRealRouteTestFailsWhenARequiredCompilerIsMissing`, shown red against the old ordering).

**Notes.**

- **N1:** CC is split by a port of cmd/go's `quoted.Split` (`cgoQuotedSplit`), held by quoted's own test table. The
  earlier splitter treated a quote inside a field as a quote, which cmd/go does not.
- **N2:** CC is resolved once to one program before its `--version` and the probe's link: a relative path against the
  package directory, a bare name on PATH.
- **N3:** `GoCgoLibraries.Register`'s target overload is additive, so compiled callers are unaffected. At source,
  `Register(assembly, null)` is now ambiguous (CS0121). No hand caller exists; the release notes name it.
- **N5:** each bindings golden's header names its architecture.
- **N6, INTENDED:** the host rule fires on every `-cgo` run, a package with no C in it included. `-cgo` is a cgo-on
  load, and a cgo-on load for another target needs that target's C toolchain. The help text says so.
