# DESIGN — the windows/amd64 syscall trampoline: Go's register shape, and `r2` as XMM0

> **Status: PROPOSED (2026-11-28), revision 4, for P3's re-read.** P3's design read of revision 2 (`5e3cef4e76`) was
> APPROVE WITH CHANGES: one MUST on the Native AOT + CFG arm, two SHOULDs, three NITs. COORD 002011Z and 002455Z ruled
> the MUST: option (d), the stub checks `Fn` with the process's own CFG routine. P3's re-read of revision 3 found the
> stub took that routine from the caller's block (MUST); revision 4 keeps it on the stub's own read-only page.
> Sections 11 and 12 list the folds.
> Red row: behavioral `WindowsSyscallFloatReturn` on `claude/p1-syscall-xmm0-r2` (`755aedae3b`), compared with
> `go run`: read red on windows/amd64 (behavioral-stderr 46079966559), lines 1-4 reading 0 where Go prints 2.5, 1.5,
> 1024 and 12, line 5 equal. Ordered by COORD 222529Z; design read required by COORD 224618Z because the remedy maps
> executable memory. Revision 2 folds the second read (COORD 230743Z, CLEAR WITH NITS, points 1-4 and the nits) and
> its CFG addendum (COORD 230844Z), the .NET caller question (COORD 233938Z) and its ruling (COORD 235045Z);
> section 10 lists each fold and where it landed.

## 1. The defect

Every Windows call a converted program makes through `syscall` (`SyscallN`, the fixed-arity `Syscall*`,
`Proc.Call`, `LazyProc.Call`, and the `golang.org/x/sys/windows` wrappers that route through them) ends in one
hand-owned function, `syscalln` in `src/core/syscall/windows/dll_windows.cs`, which calls the native function
through a C# function pointer typed over `nuint`.

Go's end of the same funnel is assembly, `asmstdcall` in `src/internal/runtime/syscall/windows/asm_windows_amd64.s`
(Go 1.26.11). Its contract has a floating-point half that the C# call cannot reproduce:

- **Arguments.** It loads each of the first four argument words into both its integer register (`RCX`, `RDX`, `R8`,
  `R9`) and its XMM register (`XMM0`–`XMM3`), because the Windows x64 convention passes a floating-point argument in
  the XMM register of its position (asm lines 51-62).
- **Result.** It stores `XMM0`, where a native function leaves a `double` or `float` result, as `r2` (asm line 77).

So a Go program calls a C function that takes or returns a double with `math.Float64bits` words in and
`math.Float64frombits(r2)` out. modernc.org/libc's windows `strtod` does this, which is why sqlite-vec reads zero
vectors on windows. At master `r2` is always 0 (`dll_windows.cs:201`), and a double argument arrives as whatever
the XMM register held. No single C# call can return both `RAX` and `XMM0`, and no C# signature puts one word in both
register files.

**arm64 is not affected.** Go's `asm_windows_arm64.s` passes integer registers only and never writes `R2`: its line
82 reads `// TODO(rsc) floating point like amd64 in StdCallInfo_R2?`. So `r2` is 0 there in Go, as in go2cs today.
This design changes amd64 only. (windows/386, where Go's `r2` is `EDX`, is not a go2cs target.)

## 2. The remedy

Run Go's call shape, in machine code: a 158-byte transcription of `asmstdcall` for the Windows x64 convention,
called through `delegate* unmanaged<StdCallInfo*, void>`. `StdCallInfo` is Go's struct of the same name, in the same
order (`Fn`, `N`, `Args`, `R1`, `R2`, `Err`, 8 bytes each). The page also holds `guard_check`, the process's CFG check
routine or 0, written once before the page is made executable (section 6).

```
push rsi; push rdi; push rbx; sub rsp, 336          ; prolog: 336 = Go's MaxArgs (42) words, RSP 16-aligned at the call
mov rbx, rcx                                        ; rbx = the StdCallInfo
mov rcx, [rbx+8]; cmp rcx, 42; ja too_many          ; Go's argument-count check, in the stub
mov rax, [rip+guard_check]; test rax, rax; jz checked ; under CFG (section 6): the check routine on Fn,
mov rcx, [rbx]; call rax; mov rcx, [rbx+8]          ;   which returns for a valid target and fast-fails otherwise
checked:
mov rsi, [rbx+16]; mov rdi, rsp; cld; rep movsq     ; the N words to [rsp] on
mov rcx, [rsp]; movq xmm0, rcx   ... mov r9, [rsp+24]; movq xmm3, r9 ; first four: both register files
mov r10, gs:[0x30]; mov dword ptr [r10+0x68], 0     ; SetLastError(0) on the TEB, as Go's asm does
mov rax, [rbx]; call rax
mov [rbx+24], rax; movq [rbx+32], xmm0              ; r1, r2
mov r10, gs:[0x30]; mov eax, [r10+0x68]; mov [rbx+40], rax           ; GetLastError()
add rsp, 336; pop rbx; pop rdi; pop rsi; ret        ; the epilog the unwinder recognizes
too_many: int3                                      ; Go's `INT $3`: not enough room -> crash
```

Differences from Go's asm, each forced by the convention rather than chosen: Go's frame is ABI0 (it saves the old
SP in its own frame and aligns by masking); this one is a Windows x64 function with a fixed allocation, so the
unwinder can describe it. Go checks the count only on its copying path (more than four words); the stub checks it
before every copy, which reads the same for every count. Reading words 0-3 when `N < 4` reads the stub's own reserved
stack, which the callee ignores, as Go's fast path does not need to; it saves a branch per arity and changes no
result.

**The argument limit.** The frame holds Go's `MaxArgs = 42` words (`internal/runtime/syscall/windows/
syscall_windows.go:18`), so with the stub `SyscallN` takes up to 42 arguments and refuses a 43rd with Go's text,
`runtime: SyscallN has too many arguments` (Go's `runtime/syscall_windows.go:418-419`), before the stub is entered;
the stub's own check is the backstop Go also keeps. Without the stub (section 6) and on arm64, the C# function-pointer
switch has cases up to 18 words, so there a 19th to 42nd argument is a SEPARATE go2cs limit, refused by name
(`syscall: N arguments, more than the 18 this port's C# call passes`), never Go's text and never a silent truncation.

**Last-error.** The stub clears and reads `LastErrorValue` in the TEB (`gs:[0x30]`, offset 0x68) around the call,
as Go's asm does, in place of `Marshal.GetLastSystemError`. Behavioral `WindowsSyscallFloatReturn` line 10 reads it:
`GetFileAttributesW` on a missing file through `Proc.Call` returns `err` 2, equal to `go run`.

**Where it lives.** In `dll_windows.cs` (syscall's hand-owned file), as a nested `StdCallTrampoline` class. Not in
golib: nothing else needs it, and golib's public surface does not change. `callNative` uses it when it exists, and
keeps the current function-pointer switch otherwise. `NativeStructMarshal.CallDeep` passes through unchanged (its
result type widens to carry `r2`).

## 3. W^X, and one allocation per process (COORD point 1)

- `VirtualAlloc` one page (the protection is section 6's CFG choice; it is never writable and executable at once);
  write the image (section 4); `VirtualProtect(PAGE_EXECUTE_READ, ...)`; **compare the mapped page with the image**,
  byte for byte, and on any difference free the page and take the fallback; under CFG, mark offset 0 (section 6);
  `FlushInstructionCache`;
  `RtlAddFunctionTable`; and only then publish the function pointer. No `PAGE_EXECUTE_READWRITE` anywhere, and after
  the protect call nothing writes the page again.
- Once per process, lazily, thread-safe: the function pointer is a `static readonly` field of the nested class, so
  the CLR's type initializer builds it exactly once, on the first windows syscall, and every thread sees the
  finished value. The page is never freed (one page per process, like Go's own code).
- **A normal transition, no `SuppressGCTransition`.** The function-pointer call to the stub is an ordinary unmanaged
  call: the thread leaves cooperative mode, so the native function may block, and may call back into managed code
  (`syscall.NewCallback`) while the GC runs.
- **The type initializer never throws.** Every step is inside one `try`; a failed step, or any exception, releases
  the page if it was allocated (`VirtualFree`), leaves the field null, and records the step for the loud line in
  section 6. A type initializer that threw would make every later syscall throw `TypeInitializationException`.

## 4. Where the bytes come from, and how they are checked (COORD point 2)

- **Derived source.** `asmstdcall`, `src/internal/runtime/syscall/windows/asm_windows_amd64.s`, Go 1.26.11. The
  file's PROVENANCE block gains: "StdCallTrampoline: DERIVED FROM src/internal/runtime/syscall/windows/
  asm_windows_amd64.s, Go 1.26.11, TEXT asmstdcall; rewritten for the Windows x64 calling convention (a fixed
  frame the unwinder can describe)". The file already carries Go's BSD notice and names LICENSES/BSD-3-Clause.txt.
- **One image, one source, one pin.** The page's whole content is one GNU `as` source (Intel syntax), held in the
  file's comment between `ASM-BEGIN` and `ASM-END`: the code, `.balign 4, 0xcc`, the `UNWIND_INFO` as `.byte` and
  `.short` directives, and the `RUNTIME_FUNCTION` as `.long stub - stub, stub_end - stub, unwind_info - stub`, so the
  assembler computes every offset. The array beside it, a `ReadOnlySpan<byte>` property (`Image`, data in the image's
  read-only section, never a writable `byte[]` field), holds exactly the bytes that source assembles to: 200 bytes,
  the code at 0-157, `int3` padding at 158-159, `UNWIND_INFO` at 160-175, the `RUNTIME_FUNCTION` at 176-187, `int3`
  padding at 188-191, and `guard_check` at 192-199, 0 in the image (so the pin and the re-assembly see 0) and written
  on the page before it is made executable; the post-protect compare covers it.
  `RtlAddFunctionTable` takes a pointer to that last record.
- **The check**, a repoguard test under the plain `go test ./...` (`syscallTrampolineBytes_test.go`):
  1. It extracts the `ASM` comment and the `Image` array from `dll_windows.cs`.
  2. Always: the array's SHA-256 equals one pin in the test, so a byte edit without a review of the pin fails on every
     host.
  3. Where `as` and `objcopy` run: it first assembles a one-line probe (`nop`) with `as --64`. If that fails (an `as`
     that is not GNU x86-64, or none), the re-assembly half logs NOT RUN (Inconclusive, never a pass) and the pin
     still holds. Otherwise it assembles the comment and compares `.text` with the array, and a failure there is red.
  4. Made to fail: one byte flipped in the array fails both halves, restored byte-identical.
  The bytes in this revision were produced exactly that way (`as --64` then `objcopy -O binary`), 200 bytes, SHA-256
  `83f78a2b893abcc9762ea0ca793290afd0d7bff3f93fe1b70efd5d9717cef554`. The `rip`-relative load resolves inside the
  section (no relocation).

## 5. Unwind data (COORD point 4)

`UNWIND_INFO` version 1, no handler, no frame register, prolog size 10, five code slots padded to six. In reverse
prolog order: at offset 10, `UWOP_ALLOC_LARGE` with op info 0 and a slot of 42 (336 / 8); at 3, 2 and 1,
`UWOP_PUSH_NONVOL` of `rbx` (3), `rdi` (7) and `rsi` (6). The `RUNTIME_FUNCTION` record, at page offset 176, is
(begin 0, end 158, unwind info 160), each relative to the page base given to `RtlAddFunctionTable`. The `int3` at 157
lies after the prolog, inside the described range, so a crash there unwinds like any other body instruction. The CFG
check is an ordinary call from the body; its fast fail ends the process without unwinding.

Why it is needed: a native function called through the stub may call back into managed code
(`syscall.NewCallback`), and a Go panic in that callback unwinds through the stub's frame. Without a function-table
entry, the OS unwinder treats the frame as a leaf whose return address is at `[rsp]`, which is wrong after the
pushes and the allocation, and the unwind would land on garbage.

**Covered by a row already:** `WindowsNewCallback` line 5 (`callback-panic-unwinds:`) panics inside a callback that
`EnumTimeFormatsEx` makes, called through `LazyProc.Call`, so through `syscalln` and, after this change, through the
stub. That line is the unwind proof; the hosted reading of the fix runs `WindowsNewCallback` with the new row.

**Made to fail.** On a measure-only branch that is never merged (`claude/p1-syscall-xmm0-plant`, `2fc1102646`), the
build skips `RtlAddFunctionTable`, and `WindowsNewCallback` line 5 must read red (behavioral-smoke windows, filter
`WindowsNewCallback`). The same commit carries section 6's two plants; each reds a different arm. Restored by
dropping the branch; the fix branch never carries the skip.

## 6. Hardened processes, and a loud fallback (COORD point 3)

- **Arbitrary Code Guard** (dynamic code prohibited): `VirtualProtect` to an executable protection fails. A JIT
  process cannot run under ACG at all (the JIT needs the same thing), so this is a Native AOT process with the
  mitigation set. Result: the fallback. (A shipped native shim, which ACG allows, is the follow-up after .5.)
- **CET shadow stacks** (.NET opts applications into CET compatibility by default): the stub's one `call` is paired
  with the callee's `ret`, and the stub's own `ret` returns to its caller, so the shadow stack stays balanced. Windows
  CET enforces shadow stacks only, not indirect-branch tracking, so no `endbr64` is needed. An exception unwinding
  through the frame is checked against the shadow stack by the OS unwinder, which the correct unwind data in
  section 5 satisfies. No EH continuation metadata is needed: the stub has no handler and no continuation target.
- **Control Flow Guard.** Plainly: memory that `VirtualProtect` makes executable is marked by default as valid call
  targets at EVERY location of the page (unless the call carries `PAGE_TARGETS_NO_UPDATE`), so with the default a
  CFG-checked indirect call may land anywhere in the stub, for example straight on its `call rax`. And the stub is an
  unchecked forwarder: it calls `StdCallInfo.Fn` without a CFG check, so whoever can reach its entry with a chosen
  `RCX` gets an arbitrary call. **Ruling (COORD 230743Z): harden it.** When the process runs with CFG on
  (`GetProcessMitigationPolicy(ProcessControlFlowGuardPolicy)`), the page is allocated `PAGE_EXECUTE_READ |
  PAGE_TARGETS_INVALID` (every location invalid), protected `PAGE_READWRITE` to write the image, protected
  `PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE` (the bits stay invalid), and then `SetProcessValidCallTargets` marks
  offset 0 ONLY; if any step fails, the page is freed and the fallback is taken. A process without CFG keeps the
  default. That leaves one valid target, the entry, which a CFG-checked caller must be able to reach. Narrowing alone
  does not make the forward through the entry checked; revision 3's check of `Fn` does (below).
  **Measured, not assumed** (COORD 230844Z). `TestCgoSyscallTrampolinePageCallTargets` (converter-cgo, windows,
  llvm-mingw, built `-mguard=cf`, process `cfg: 1`) calls page+0 and page+16 under each mapping. Reading 4
  (`46083273402`):
  - default protection (the control): page+0 returns 7 and page+16 returns 16;
  - `PAGE_TARGETS_INVALID`, offset 0 marked: page+0 returns 7, and the call to page+16 ends the process with the CFG
    fast fail (`0xC0000409`). This is the arm the trampoline takes;
  - `PAGE_READWRITE` allocation, protected `RX | NO_UPDATE`: `SetProcessValidCallTargets` fails with error 998
    (`ERROR_NOACCESS`), so a read-write allocation cannot be narrowed;
  - default protection with offset 0 marked: the call succeeds and page+16 stays callable, so marking alone narrows
    nothing.
  The row asserts the control, the invalid arm, and the invalid mapping with offsets 0 and 16 marked (both callable),
  which shows the mark takes effect; the other mappings are recorded only.
  **The .NET caller, measured** (COORD 233938Z). The C row also found that a call to `SetProcessValidCallTargets`
  through a pointer from `GetProcAddress` ends a CFG process with a fast fail before it returns, while a direct call
  through the import table succeeds; the C# `[LibraryImport]` calls through a resolved pointer.
  `TestSyscallTrampolineCfgDotnetCaller` (converter-cgo, windows) builds a probe with syscall's own declarations and
  page sequence and runs it four ways. Reading 1 (`46084158304`):
  - the JIT, through the program's apphost and through `dotnet.exe`: CFG on, the marking call returns, page+0
    returns 7 and page+16 returns 16 (managed code makes no CFG check, so the narrowing guards native callers);
  - Native AOT, default settings: CFG off, the default mapping, both callable;
  - Native AOT with `<ControlFlowGuard>Guard</ControlFlowGuard>`: CFG on, and the marking call ends the process with
    a fast fail.
  So every windows JIT process runs with CFG on and takes the narrowed arm safely, and a Native AOT process built with
  CFG must not make the call. Reading 2 (`46084609091`), on that Native AOT + CFG binary: with the invalid mapping and
  the call skipped, page+0 ends the process (no location is valid, and that code checks the call), so skipping the
  call alone is not a choice; mapped as without CFG, page+0 returns 7 and page+16 returns 16.
  **Ruling (COORD 235045Z): under Native AOT with CFG on, the marking call is skipped and the page is mapped with the
  default protection** (allocated `PAGE_READWRITE`, written, made `PAGE_EXECUTE_READ`: every location valid, as in a
  process without CFG, still never writable and executable at once). Correct results beat narrowing: narrowing is
  defence in depth, and the fallback would bring back `r2 = 0`. JIT processes keep the narrowed arm. The case is
  detected as CFG on (`GetProcessMitigationPolicy`) with `RuntimeFeature.IsDynamicCodeSupported` false.
  `GetProcessMitigationPolicy` is reached by the same `[LibraryImport]` and returns in a Native AOT + CFG process
  (both readings print `cfg: 1` there before anything else). The row now asserts, in each of the four callers, that
  this choice returns 7 from page+0 and exits 0: the two JIT callers narrowed, with the marking call returning; Native
  AOT not narrowed, with CFG off; Native AOT + CFG not narrowed, with the detector's `cfg: 1`. The marked and
  unmarked Native AOT + CFG arms stay recorded.
  **What that ruling left open (P3's design read, MUST 1).** A Native AOT program built with CFG checks its own
  indirect calls, so at master `syscalln`'s function-pointer call to `Fn` is CFG-checked in that process. With the
  stub as revision 2 had it, only the call to the stub is: the default protection makes every location of the page a
  valid target, and the stub forwards to `[rcx]` with the arguments at `[rcx+16]` unchecked. A hijacked indirect call
  that reaches the entry with `RCX` on attacker-written memory gets an arbitrary call. Narrowing would not change
  that, since the entry is the gadget. So revision 2 removed a check the process makes today, and its line "the same
  unchecked call ... already makes in an uninstrumented build" was true under the JIT only.
  **The fix (COORD 002455Z, option (d)): the stub checks `Fn` itself.** When the process runs with CFG, the type
  initializer reads the process image's own check routine (`IMAGE_LOAD_CONFIG_DIRECTORY64.GuardCFCheckFunctionPointer`,
  the slot the loader fills, through `GetModuleHandleW(NULL)` and the PE headers; no call is made) and writes it into
  the page's `guard_check` slot with the image, before the page is made executable; the stub loads it `rip`-relative.
  When it is not 0 the stub calls it with `Fn` in `RCX` before anything else: the routine
  returns for a valid call target and ends the process with the CFG fast fail otherwise, which is what the
  instrumented caller's own indirect call does at master. So every CFG process, JIT or Native AOT, checks `Fn`, and the
  stub is not an unchecked forwarder in any of them. The routine's home space is the stub's reserved stack, written
  afterwards; `rbx`, `rsi` and `rdi` survive it, and `N` is reloaded. A process without CFG reads 0 and skips it. A
  process with CFG whose image names no check routine takes the loud fallback (`the process runs with Control Flow
  Guard and its image names no check routine`); no known process reaches it.
  **Where the routine lives (P3's re-read of revision 3, MUST).** Revision 3 had the stub read the routine from the
  caller's block (`StdCallInfo.Check`, `[rbx+48]`), filled from a static in writable memory. A hijacked call that
  reaches the entry with `RCX` on attacker-written memory writes 0 there and skips the check, or writes an address and
  has it called unchecked; a write to the static turns the check off for every call. Revision 4 reads it from the
  stub's own page, which is executable and never writable after the protect, so no write a caller or the process can
  make outside `VirtualProtect` turns it off. `StdCallInfo.Check` and the static are gone. The shipped row's `block`
  run enters the stub with a block the program builds (`Fn` an invalid target, every other word 0): it returned 7 on
  revision 3 and must end with `0xC0000409` on revision 4.
  **What stays after the check (stated, P3 SHOULD 2).** Two residuals remain where an attacker already controls an
  indirect call target and `RCX`:
  - Past the check: where the page is not narrowed (Native AOT + CFG, COORD 235045Z), the stub's own `mov rax, [rbx];
    call rax` at offset 115 is a valid call target that follows the check. Reaching it usefully needs `RBX` on attacker memory as
    well. This is the cost of keeping the default protection there; narrowing (option (c)) would remove it.
  - Argument control: the checked entry lets a hijacked call that controls `RCX` alone choose every argument word, in
    both register files, of any VALID target (`VirtualProtect` is one). Master's Native AOT + CFG process has no such
    target. A cheap narrowing, proposed for COORD to rule: the stub refuses a block outside the current thread's stack
    (`gs:[0x08]` and `gs:[0x10]` in the TEB); `syscalln`'s block is always a local.
  Measured before the code (`46087694121`, `aot-cfg-guardcheck` on the .NET caller row, Native AOT + CFG): the slot is
  present, the check of `GetTickCount` returns, and the check of an address on a `PAGE_TARGETS_INVALID` page ends the
  process with `0xC0000409`.
  **Option (c), a direct import of `SetProcessValidCallTargets`, is not shipped.** It would let Native AOT + CFG narrow
  the page too, but it needs the publishing project to carry a `DirectPInvoke` item and `mincore.lib`, which `syscall`
  cannot add to a converted program's publish through a project reference. Its arm stays recorded on the .NET caller
  row (`aot-cfg-direct`; the first reading was an instrument fault, CS0579 from a second intermediate directory inside
  the project, fixed in `bb1f3e8f5a`). The `aot-cfg-apiset` arm, the API-set name, fast-fails like the plain import.
  **The detector.** `RuntimeFeature.IsDynamicCodeSupported` stands in for "this caller's indirect calls are not
  checked", the condition under which the marking call is safe. The asymmetry is why: a wrong "narrow" ends the
  process at its first syscall, while a wrong "default" only leaves the page wider, and `Fn` is checked either way.
  **The shipped class, measured** (P3 SHOULD 2). `TestSyscallTrampolineShipped` and `TestSyscallTrampolineShippedCfg`
  (cgo-e2e, windows; one Native AOT publish each, since one takes 15 to 25 minutes there) publish a program
  against the repository's own `golib` and `syscall`, not a copy of their choices, and run it in the four callers.
  Each must print `strtod` 2.5 through `LazyProc.Call`, `labs` 7, 19 and 42 words through `_snprintf`, errno 2, and
  under the JIT a `NewCallback` pointer called through `SyscallN`; and a second run of each calls, through `SyscallN`,
  a function on a page whose every location is an invalid target: under CFG it must end with `0xC0000409` before the
  call returns, and in plain Native AOT it returns 7. Under the JIT a third run enters the stub with the program's own
  block (above), which must also end with `0xC0000409`. Made to fail on the plant branch: `narrow = cfg` must end the
  Native AOT + CFG caller at its first syscall, and a stub without the check must let the CFG callers' unmarked call
  return 7.
- **Never RWX, in either arm.** The CFG arm allocates `PAGE_EXECUTE_READ | PAGE_TARGETS_INVALID`
  (nothing written yet), protects `PAGE_READWRITE` to write the image, then
  `PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE`.

**The fallback is loud.** When the stub cannot be built on amd64, `syscalln` keeps the current call (exact for
every call that passes and returns integers only) and writes ONE line to stderr, on the first call that takes the
fallback: `go2cs: syscall: the amd64 call trampoline is unavailable (<step> failed, error <n>); floating-point
arguments and results of Proc.Call and SyscallN are not passed (r2 reads 0)`. A refusal when a float is read is not
possible: `r2` is a plain `uintptr` and a float argument is an ordinary word, so the seam cannot tell a float call
from an integer one. Refusing every call would break the integer calls that do work. The warning is the loud form
that is available.

**The fallback is exercised on every windows host, not only a hardened one.** The environment variable
`GO2CS_SYSCALL_TRAMPOLINE=off`, read once by the type initializer, takes the fallback with the step `disabled by
GO2CS_SYSCALL_TRAMPOLINE=off`. `TestSyscallTrampolineShipped` runs its JIT program a third time with the variable set:
`strtod` reads 0, `labs` 7, exactly one stderr line names the switch, and 19 words are refused by name (`syscall: 19
arguments, more than the 18 this port's C# call passes`). At master a 19th word throws `ArgumentOutOfRangeException`.

## 7. Native AOT and single-file (COORD point 5)

Nothing in the stub is reflective: a `ReadOnlySpan<byte>` image, source-generated `[LibraryImport]`s
(`VirtualAlloc`, `VirtualProtect`, `VirtualFree`, `FlushInstructionCache`, `GetCurrentProcess`, `GetModuleHandleW`,
`RtlAddFunctionTable`, `GetProcessMitigationPolicy` and `SetProcessValidCallTargets`), and a function-pointer call.
The call shape is the same under JIT, ReadyToRun, single-file and Native AOT. **The CFG arm differs:** a JIT process
runs with CFG and narrows the page to its entry; a Native AOT process runs without CFG unless built with
`<ControlFlowGuard>Guard</ControlFlowGuard>`, and then keeps the default protection (section 6). Every CFG process
checks `Fn`. `TestSyscallTrampolineShipped` reads the stub in all four callers.

## 8. What is not changed

- The runtime package's own native calls (`stdcall` in `runtime/windows`) never pass or return floats; they are
  out of scope.
- arm64 keeps the current call (section 1).
- The deep-copy door (`NativeStructMarshal.CallDeep`) and the token refusals are unchanged.

## 9. Gates for the code

The red row then green (behavioral-smoke windows, filter `Windows`, so `WindowsNewCallback` reads in the same run),
with the row's lines 6-10: `_snprintf` with six arguments and doubles at positions 4, 5 and 6 (a guard, green at
master too); 19 arguments, the first count the C# switch cannot pass; 42, Go's MaxArgs; 43, recovered with Go's text;
and errno 2 from `Proc.Call`. Each line equals `go run`. Then: `TestSyscallTrampolineShipped` (section 6), and its
two plants plus the unwind plant (section 5), on the measure-only branch; the CFG row (green since reading 5,
`46083694102`); the .NET caller row; the bytes test, made to fail; a windows `go2cs-stdlib` build of `syscall`;
GolibTests R/D and `go test` (the standard battery); repoguard. No converter change, so no CNR.

Both shipped rows are opt-in on `GO2CS_TESTS_E2E=1`, as the other end-to-end rows are, because each makes a Native
AOT publish that a plain `go test ./...` deadline does not hold; the cgo-e2e stage sets the switch. The row's `block`
run takes the stub by reflection, so it runs under the JIT only: Native AOT + CFG does not run it. The stub's bytes
are the same in every arm, so the JIT reading covers the check those bytes make.

## 10. Revision 2: the second read's folds

| Point (COORD 230743Z / 230844Z) | Where it landed |
|---|---|
| 1. Count check in the stub; frame sized for Go's 42 words; 18 as a separate limit | §2 listing and "The argument limit"; §5 slot 42 |
| 2. CFG stated plainly; harden with `NO_UPDATE` + offset 0 only when CFG is on; fallback on failure | §6 Control Flow Guard |
| Addendum: prove page+16 invalid and page+0 valid, or say the hardening is not achieved | §6: proved on the `PAGE_TARGETS_INVALID` mapping (reading 4); the read-write mapping cannot be narrowed |
| COORD 233938Z / 235045Z: the .NET P/Invoke under CFG, JIT and Native AOT | §6: safe under the JIT; Native AOT + CFG skips the call and maps the page with the default protection; the row asserts all four callers |
| 3. One `as`-sourced image (code + `UNWIND_INFO` + `RUNTIME_FUNCTION`), one pin; unwind made-to-fail | §4, §5 |
| 4. Compare the mapped page with the image before publishing; image as a `ReadOnlySpan<byte>` property | §3, §4 |
| NIT: the type initializer never throws | §3 |
| NIT: `as --64` probe reports NOT RUN where unusable | §4 step 3 |
| NIT: a row with 5+ arguments and a double past position 4 | §9, row line 6 |
| NIT: a forced-fallback switch so the fallback arm runs on every host | §6, last paragraph |
| NIT: the "function entry at 144" wording | §4, §5: the `RUNTIME_FUNCTION` record, at 152 in this layout |
| arm64: cite the line | §1, `asm_windows_arm64.s:82` |

## 11. Revision 3: P3's design read of revision 2 (COORD 002011Z, 002455Z)

| Finding | Where it landed |
|---|---|
| MUST 1. Under Native AOT + CFG the stub removed master's check on `Fn`; the text said the opposite | §6: the text corrected, and option (d) checks `Fn` in every CFG process; §2 listing, `Check` at 48 |
| SHOULD 2. No gate ran the shipped class's Native AOT + CFG branch | §6 and §9: `TestSyscallTrampolineShipped`, four callers, made to fail by `narrow = cfg` and by removing the check; §7 says the CFG arm differs |
| SHOULD 3. Rows for 19, 42 and 43 arguments, and 19 refused under the off switch | §9, row lines 7-9; §6, the off switch run |
| NIT 4. Last-error now comes from the TEB read | §2 "Last-error", row line 10 |
| NIT 5. What `IsDynamicCodeSupported` stands in for | §6 "The detector" |
| NIT 6. No `SuppressGCTransition` | §3 |
| COORD 002011Z (c): a direct import of `SetProcessValidCallTargets` | §6: not shipped (needs publish items `syscall` cannot add); arm recorded |
| Layout | §4, §5: 180 bytes, code 0-150, `UNWIND_INFO` at 152, `RUNTIME_FUNCTION` at 168 |

## 12. Revision 4: P3's re-read of revision 3 (COORD 021145Z)

| Finding | Where it landed |
|---|---|
| MUST 1. The stub read its check routine from the caller's block, filled from a writable static | §2, §4, §6: `guard_check` on the stub's own page, `rip`-relative, written before the protect and compared; `StdCallInfo.Check` and the static removed; the row's `block` run, red on revision 3 |
| SHOULD 2. Two residuals not stated | §6 "What stays after the check"; the thread-stack check proposed for COORD |
| SHOULD 3. Under the JIT a call to a target CFG calls invalid now fast-fails where Go and master call it | docs/Limitations.md, one entry |
| NIT 4. CFG on with no routine leaves the stub unchecked | §6: the loud fallback |
| Layout | §4, §5: 200 bytes, `RUNTIME_FUNCTION` at 176, `guard_check` at 192 |
