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
| R8 | **Every generated wrapper crosses the seam**: `GoNativeCall.Enter(MayBlock)` before the call and `Exit()` in a `finally`. errno is read **between** the native return and `Exit()`. No `[SuppressGCTransition]` binding ever reaches the seam. | DESIGN-native-call-seam §3.5, §4.3, §7.1 |
| R9 | Generated bindings are **build outputs, not corpus content**: git-ignored, and outside every byte-identity gate. | PLAN §5.2 |
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

`r, err := C.f(x)` binds the same symbol with `SetLastError = true`. The wrapper reads
`Marshal.GetLastSystemError()` **after the extern returns and before `GoNativeCall.Exit()`**, which is the seam's
§4.3 rule. It then returns `(r, err)` with `err` nil when the value is 0, and `syscall.Errno(value)` otherwise, as
cgo does.

### 4.4 What converted user code shows

Recommendation: user code reads **as cgo rewrote it**, `_Cfunc_add(_Ctype_int(a), _Ctype_int(b))`. That is the text
the type checker resolved, and it is what Go's own toolchain compiles.

The alternative is to print the source's `C.add(C.int(a), C.int(b))` back, through a nested static class `C` that
declares the C-named members. That reads more like the Go the user wrote. It has a cost: the `_Ctype_*` types would
become nested types (`C.@int`), and go2cs-gen's handling of `[GoType]` structs nested inside a static class is
unmeasured. ⟨Q1⟩ for review, with the plain form as the safe default.

## 5. Mirrors: the struct-passing discipline at generated scale

### 5.1 The mirror

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

### 5.3 The size assertion, and what it can and cannot catch (R3)

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

Phase 2 reads `-l<name>` and `-L<dir>` from the `//go:cgo_ldflag` lines (§3), and nothing else. Every other flag is
**refused by name**, until a later phase has a use for it and an allowlist to check it against. Two things keep that
from being a false comfort:

- the flags arrived through cmd/go's `checkLinkerFlags`, which §9 arm 1 is to confirm for `go list -compiled`;
- Phase 2 checks `-l`/`-L` values itself: a name that is a plain token, and a directory that exists. Whatever the
  first point turns out to be, Phase 2 never hands an unchecked flag to a tool.

## 7. What Phase 2 refuses, by name

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

The wrapper's only scheduler contact is `GoNativeCall.Enter`/`Exit` (R8), so M:N changes nothing here. The seam's
contract already covers a goroutine that resumes on another worker. The rest also stands as designed:

- errno is read inside the transition (§4.3);
- a thread-affine C library needs `runtime.LockOSThread`, as in Go (seam §7.3);
- `#cgo nocallback` adds only a debug assertion that no `ReenterManaged` occurs (seam §7.2);
- `#cgo noescape` is Phase 3's pin-scope hint;
- no binding is ever `[SuppressGCTransition]` (seam §7.1, and its guard).

## 9. Increments, each a seat, red-first

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
