# REVIEW — DESIGN-cgo-phase2-pinvoke (H, claude/h-cgo-phase2-design 3b9142ae23), adversarial

> **Status: RECORD.** COORD requested this adversarial review before any Phase 2 increment is cut (owner-provided
> Fable 5.2, 2026-11-09). It read the design, PLAN-cgo-interop (§4, §5.1-5.4, Phase 2, §11),
> DESIGN-native-call-seam (§4, §12-13), REVIEW-cgo-interop corrections 3-4, the C-toolchain scout, Phase 1's
> cgoDeclarations.go, golib's GoNativeCall / GoCgoDynamicImports / NativeBox, and the golib-gen native-boundary rules.
> Go source was read at a local go1.23.1 (cmd/cgo, cmd/go security/exec, runtime/cgocall); the semantics checked
> are stated as unchanged at go1.26.10 as far as the reviewer knows. **Verdict: approve with changes.**

## Must change before the wrapper increment (3) is cut

1. **BLOCKER: the stack mirror is one element wide.** cgo's rule makes `C.f(&xs[0], C.int(len(xs)))` legal: when a
   pointer to an element of an array or slice is passed, the Go memory in question is the entire array. C then
   reads and WRITES n consecutive structs, which is a stack smash on a native write. The mirror row covers
   StandardBox<T> (and a FieldRefBox whose pointee is a whole struct) ONLY. An ElemRefBox of a struct element is
   refused by name to Phase 3, the pin scope over the backing array. Red: a wrapper handed `&xs[1]` must refuse before
   the extern runs.
2. **R3's size assertion is tautological under LayoutKind.Explicit with Size = N.** SizeOf returns N by
   construction, and FieldOffset makes offset checks vacuous. Assert something that can fail: either Sequential with
   a derived Pack, asserting SizeOf and Marshal.OffsetOf per field against go/types, or Explicit plus a per-field
   `sizeof(clrType) == Sizeof(goType)` check and a per-InlineArray length check. Increment 2's red plants an
   UNDERFLOW (int where long belongs), refused by name.
4. **The wrapper must use the ruled seam shape.** DESIGN-native-call-seam §12.2 Q2 ruled Go's shape: the g-status
   move happens in the runtime's entersyscall family, which then calls GoNativeCall. R8 brackets with the runtime
   forwarders (syscallEntersyscall/syscallExitsyscall), and its red reads GoCurrentGStatus and TransitionDepth from
   an [UnmanagedCallersOnly] probe, as S-b's tests do. Without that, a cgo call shows `_Grunning` where Go shows
   syscall (F-2 reintroduced).

## Design-text corrections each increment carries

3. **errno names the wrong API.** Use `SetLastError = true` on the `_C2func_` binding and read
   `Marshal.GetLastPInvokeError()` before Exit(), which reproduces cgo's errno=0 / call / capture trampoline.
   `GetLastSystemError()` after the stub returns can read an errno clobbered by the GC transition. Plain `_Cfunc_`
   bindings stay `SetLastError = false`. State the void two-result form.
5. **Windows load-time capture is per SYMBOL.** mingw `-lm` resolves to API-set forwarders (`cos` in
   api-ms-win-crt-math, `div` in api-ms-win-crt-utility), and some libm symbols are static in libmingwex.a; on ELF,
   `--as-needed` drops unreferenced libraries. The probe references every bound symbol, reads the symbol-to-library map
   from the import table, DT_NEEDED+dynsym or LC_LOAD_DYLIB, writes one LibraryImport per symbol, and refuses by name
   any symbol resolved statically or not at all.
6. **`_Cvar_` (a C global) and `_Cfpvar_fp_` have no §7 owner.** Refuse both by name until Phase 2-later
   (`_Cvar_` via NativeLibrary.GetExport into a NativeBox<T>). A silently converted managed zero is worse than a
   refusal.
7. **Struct BY VALUE is ABI-classified by the CLR from the mirror's field types.** A union-as-bytes or a float
   inline array (an HFA on arm64) can classify differently from C. Phase 2 admits only integer- and pointer-only
   structs by value, with no unions and no inline arrays; floats, unions and inline arrays by value are "Phase 2
   later, measured per RID". Add `inet_ntoa(struct in_addr)` as the by-value ARGUMENT arm.
8. **The bindings file is converter output, not git-ignored build output.** It is produced at convert time by the
   pinned toolchain, and a checkout cannot regenerate it. Gates exclude `*.cgo.g.cs` by path; the converter never
   writes .gitignore entries into a user's tree.
9. **Mirrors vary per (GOOS, GOARCH)**: `_Ctype_char` is int8 on x64 and uint8 on linux/arm64. State it, and
   place bindings by the four-target layout rule (see the ruling).
10. **The exit gate exercises scalars only.** Add a copy-back arm (`uname` on linux/darwin, `GetSystemTime` on
    windows), an errno arm (`sqrt(-1)` reading EDOM through the two-result form), a REAL current-directory arm (a
    library that exists ONLY in CWD must throw DllNotFoundException, while a copy in the app directory loads), and a
    pinned-zig probe library for aliasing, unions and HFA as an instrument.
11. **A resolver returning IntPtr.Zero falls back to default probing**, which is legacy order with CWD on Windows.
    A miss throws, with a red; add assembly-level `[DefaultDllImportSearchPaths(SafeDirectories | AssemblyDirectory)]`;
    the resolver returns 0 for names it does not own (hand-owned imports in the same assembly).
12. **darwin: `@rpath`/`@loader_path`/`@executable_path` install names cannot load from the CLR.** Record the
    absolute path the probe's linker resolved, or refuse by name. Admit `-framework X` with its install name
    captured. State that, as with Go, a library outside the loader path needs LD_LIBRARY_PATH on linux.
13. **Flexible array members** (`T[]` arrives as `[0]T`) are refused by name: native memory only. Bitfields are
    DROPPED by cmd/cgo, not padded by name, so those bytes round-trip opaque.

## Notes

14. Map C `_Bool` to byte in mirrors and externs; a C# bool marshals as a 4-byte BOOL.
15. CGO_LDFLAGS from the ENVIRONMENT reaches //go:cgo_ldflag UNCHECKED (cmd/go exec.go), so Phase 2's own -l/-L
    check is load-bearing. Define "plain token" (refuse `-l:` and path separators).
16. State the divergences: runtime.Pinner-pinned members and self-pointers are refused (a narrowing); a uintptr
    member carrying a managed-pointer token is invisible (cgocheck is equally blind); a union with pointers is bytes.
17. The cost of Enter/Exit is on par with Go's cgocall bracket; blocking, affinity and signals are stated correctly.
18. Q1: prefer the visible `C.add(C.int(a))` form (readability and parity rulings), with the mangled form only as a
    fallback if the arm fails.

## COORD rulings (2026-11-09)

- **Approve with changes.** Increments 0 and 1 may be cut NOW.
- **Finding 2:** Explicit layout plus per-field CLR-size checks and InlineArray length checks (no Sequential/Pack
  re-derivation). The red plants an underflow.
- **Finding 7:** the by-value admission set is integer- and pointer-only structs, with no union and no inline array.
  Increment 2 may be cut on these two rulings.
- **Finding 9:** place bindings by the four-target LAYOUT RULE (the linux/arm64 design): emit per target and home each
  file at its coarsest cover, so a GOARCH-invariant mirror set lands in `<goos>/` and a differing one in
  `<goos>-<arch>/`. That uses the axis the linux/arm64 work just built, rather than refusing arm64.
- **Finding 18:** the `C.` form is the target, as the owner's readability rulings favour.
- Increment 3 waits on findings 1 and 4; increment 4 on 5, 10, 11 and 12; increment 5 on 6, 8 and 9.

## Amendment 2026-11-10: the §11 conformance check

> A second Fable 5.2 pass read H's amendment (claude/h-cgo-phase2-design 6ae0285d8c, §11.1-11.17, +212/-0) against
> findings 1-18 and the four rulings above. **MET: 1, 2, 3, 6, 7, 8, 11, 13-18, and the sequencing (§11.14).
> PARTIAL: 4, 5, 9, 10, 12.** Nothing was contradicted outright; three internal contradictions were found inside §11.

### Must fix before the increment each gates

1. **§11.12 against §11.15, `-framework`.** §11.12 admits `-framework X`; §11.15 refuses everything but `-l`/`-L`.
   **RULED:** extend §11.15's grammar with `-framework <name>` on darwin only (cmd/go's allowlist admits it). Gates
   increment 4.
2. **§11.4's red has no route at increment 3.** A `[LibraryImport]`-bound wrapper cannot reach a managed
   `[UnmanagedCallersOnly]` probe: S-b's shape worked because `cgocaller` takes a function pointer. **RULED:** the
   instrument is a probe library exporting a call-through that takes a function pointer and an argument. It lands in
   increment 3, built by the converter-cgo leg's own compiler (gcc, mingw or clang); it does not wait for the pinned
   toolchain, which is still the owner's call. Gates increment 3.
3. **§11.10's `inet_ntoa` by-value arm cannot run on windows under §11.7**: windows' `in_addr` is a union (`S_un`),
   which by-value refuses. **RULED:** the arm runs on linux and darwin. Windows gets the probe library's export taking a
   two-int struct. Gates increment 4.
4. **Promoted from should-fix: a reinterpret-minted `NativeBox`** carries a managed token with its retained source
   set (`ж.NativeBox.cs:31-39`). "Pass its address" must refuse that arm, or C writes at a token. Gates increment 3,
   with a red.

### Should fix (each with its increment)

- §11.5 (inc 4): ELF `DT_NEEDED` plus `.dynsym` maps only VERSIONED symbols to a library. Derive the rest with
  `ld --trace-symbol` / `-Map`, or a per-`-l` probe.
- §11.10 (inc 4): darwin's libm sets `math_errhandling == MATH_ERREXCEPT`, so `sqrt(-1)` may leave errno 0. Use a
  `strtol` overflow (ERANGE) or an `open` miss (ENOENT), which all three CRTs set.
- §11.9 (inc 5): name all four covers (flat, `<goos>/`, `<arch>/`, `<goos>-<arch>/`). An all-target-identical mirror set
  lands flat.
- §11.15: align the `-l` name rule with cmd/go's leading-character refusal (`-l-foo` and `-l@x` are refused).
- Pointers to §11 from §2's R8/R9 rows and §8's first sentence. §11.6's tense: "increment 0 recognizes them", which
  is true at master once claude/h-cgo-phase1-kinds lands in batch 281. §11.2: say where the underflow is planted (the
  type map or the emission).

**Increment 2 (findings 2 and 7, both MET) may be cut now.**
