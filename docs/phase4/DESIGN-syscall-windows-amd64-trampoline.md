# DESIGN — the windows/amd64 syscall trampoline: Go's register shape, and `r2` as XMM0

> **Status: PROPOSED (2026-11-27), for P3's design read and a second read. No code lands before both clear.**
> Red row: behavioral `WindowsSyscallFloatReturn` on `claude/p1-syscall-xmm0-r2` (`755aedae3b`), compared with
> `go run`. Ordered by COORD 222529Z; design read required by COORD 224618Z because the remedy maps executable
> memory.

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

**arm64 is not affected.** Go's `asm_windows_arm64.s` passes integer registers only and never writes `R2` (a TODO in
its source), so `r2` is 0 there in Go, as in go2cs today. This design changes amd64 only. (windows/386, where Go's
`r2` is `EDX`, is not a go2cs target.)

## 2. The remedy

Run Go's call shape, in machine code: a 126-byte transcription of `asmstdcall` for the Windows x64 convention,
called through `delegate* unmanaged<StdCallInfo*, void>`. `StdCallInfo` is Go's struct of the same name, in the same
order (`Fn`, `N`, `Args`, `R1`, `R2`, `Err`, 8 bytes each).

```
push rsi; push rdi; push rbx; sub rsp, 160          ; prolog: 160 >= 18 words, RSP 16-byte aligned at the call
mov rbx, rcx                                        ; rbx = the StdCallInfo
mov rcx, [rbx+8]; mov rsi, [rbx+16]; mov rdi, rsp; cld; rep movsq   ; the N words to [rsp] on
mov rcx, [rsp]; movq xmm0, rcx   ... mov r9, [rsp+24]; movq xmm3, r9 ; first four: both register files
mov r10, gs:[0x30]; mov dword ptr [r10+0x68], 0     ; SetLastError(0) on the TEB, as Go's asm does
mov rax, [rbx]; call rax
mov [rbx+24], rax; movq [rbx+32], xmm0              ; r1, r2
mov r10, gs:[0x30]; mov eax, [r10+0x68]; mov [rbx+40], rax           ; GetLastError()
add rsp, 160; pop rbx; pop rdi; pop rsi; ret        ; the epilog the unwinder recognizes
```

Differences from Go's asm, each forced by the convention rather than chosen: Go's frame is ABI0 (it saves the old
SP in its own frame and aligns by masking); this one is a Windows x64 leaf-shaped function with a fixed allocation,
so the unwinder can describe it. Reading words 0-3 when `N < 4` reads the stub's own reserved stack, which the callee
ignores, as Go's fast path does not need to; it saves a branch per arity and changes no result.

**Where it lives.** In `dll_windows.cs` (syscall's hand-owned file), as a nested `StdCallTrampoline` class. Not in
golib: nothing else needs it, and golib's public surface does not change. `callNative` uses it when it exists, and
keeps the current function-pointer switch otherwise. `NativeStructMarshal.CallDeep` passes through unchanged (its
result type widens to carry `r2`).

## 3. W^X, and one allocation per process (COORD point 1)

- `VirtualAlloc(MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE)` for one page; write the code, the unwind info and the
  `RUNTIME_FUNCTION`; `VirtualProtect(PAGE_EXECUTE_READ)`; `FlushInstructionCache`; `RtlAddFunctionTable`. The page is
  **never** writable and executable at once: no `PAGE_EXECUTE_READWRITE` anywhere, and after the protect call nothing
  writes it again.
- Once per process, lazily, thread-safe: the function pointer is a `static readonly` field of the nested class, so
  the CLR's type initializer builds it exactly once, on the first windows syscall, and every thread sees the
  finished value. The page is never freed (one page per process, like Go's own code).
- If any step fails, the page is released if it was allocated (`VirtualFree`), and the field holds null: the
  fallback in section 6.

## 4. Where the bytes come from, and how they are checked (COORD point 2)

- **Derived source.** `asmstdcall`, `src/internal/runtime/syscall/windows/asm_windows_amd64.s`, Go 1.26.11. The
  file's PROVENANCE block gains: "StdCallTrampoline: DERIVED FROM src/internal/runtime/syscall/windows/
  asm_windows_amd64.s, Go 1.26.11, TEXT asmstdcall; rewritten for the Windows x64 calling convention (a fixed
  frame the unwinder can describe)". The file already carries Go's BSD notice and names LICENSES/BSD-3-Clause.txt.
- **In the file.** The byte array, and beside it, in a comment, the exact GNU `as` input (Intel syntax) that
  produces it, line for line, so a reader reviews the assembly and not the hex.
- **The check**, a repoguard test under the plain `go test ./...` (`syscallTrampolineBytes_test.go`):
  1. It extracts the assembly comment and the byte array from `dll_windows.cs`.
  2. Always: the array's SHA-256 equals a pin in the test, so a byte edit without a review of the pin fails on every
     host.
  3. Where `as` and `objcopy` are on PATH (every linux runner and this lane's box): it assembles the comment with
     `as --64` and compares the `.text` bytes with the array. Where they are not, it logs NOT RUN for that half
     (Inconclusive, not a pass), and the pin still holds.
  4. Made to fail: one byte flipped in the array fails both halves, restored byte-identical.
  The bytes in this proposal were produced exactly that way (`as --64` then `objcopy -O binary`), 126 bytes, equal.

## 5. Unwind data (COORD point 4)

`UNWIND_INFO` version 1, no handler, no frame register, prolog size 10, five code slots padded to six. In reverse
prolog order: at offset 10, `UWOP_ALLOC_LARGE` with op info 0 and a slot of 20 (160 / 8); at 3, 2 and 1,
`UWOP_PUSH_NONVOL` of `rbx` (3), `rdi` (7) and `rsi` (6). The `RUNTIME_FUNCTION` is (0, 126, 128) relative to the
page; the unwind info sits at 128 (DWORD-aligned) and the function entry at 144.

Why it is needed: a native function called through the stub may call back into managed code
(`syscall.NewCallback`), and a Go panic in that callback unwinds through the stub's frame. Without a function-table
entry, the OS unwinder treats the frame as a leaf whose return address is at `[rsp]`, which is wrong after the
pushes and the allocation, and the unwind would land on garbage.

**Covered by a row already:** `WindowsNewCallback` line 5 (`callback-panic-unwinds:`) panics inside a callback that
`EnumTimeFormatsEx` makes, called through `LazyProc.Call`, so through `syscalln` and, after this change, through the
stub. That line is the unwind proof; the hosted reading of the fix runs `WindowsNewCallback` with the new row.

## 6. Hardened processes, and a loud fallback (COORD point 3)

- **Arbitrary Code Guard** (dynamic code prohibited): `VirtualProtect` to an executable protection fails. A JIT
  process cannot run under ACG at all (the JIT needs the same thing), so this is a Native AOT process with the
  mitigation set. Result: the fallback.
- **CET shadow stacks** (.NET opts applications into CET compatibility by default): the stub's one `call` is paired
  with the callee's `ret`, and the stub's own `ret` returns to its caller, so the shadow stack stays balanced. Windows
  CET enforces shadow stacks only, not indirect-branch tracking, so no `endbr64` is needed. An exception unwinding
  through the frame is checked against the shadow stack by the OS unwinder, which the correct unwind data in
  section 5 satisfies. No EH continuation metadata is needed: the stub has no handler and no continuation target.
- **Control Flow Guard**: memory made executable through `VirtualProtect` is marked as valid call targets by
  default (unless `PAGE_TARGETS_INVALID` is passed, which this design does not), so a CFG-instrumented caller
  (Native AOT with `ControlFlowGuard` set) may call the page. No `SetProcessValidCallTargets` call is needed. The
  stub's own `call rax` is uninstrumented, as the current function-pointer call into the DLL is in an uninstrumented
  build.

**The fallback is loud.** When the stub cannot be built on amd64, `syscalln` keeps the current call (exact for
every call that passes and returns integers only) and writes ONE line to stderr, on the first call that takes the
fallback: `go2cs: syscall: the amd64 call trampoline is unavailable (<step> failed, error <n>); floating-point
arguments and results of Proc.Call and SyscallN are not passed (r2 reads 0)`. A refusal when a float is read is not
possible: `r2` is a plain `uintptr` and a float argument is an ordinary word, so the seam cannot tell a float call
from an integer one. Refusing every call would break the integer calls that do work. The warning is the loud form
that is available.

## 7. Native AOT and single-file (COORD point 5)

Nothing in the stub is reflective: a byte array, six source-generated `[LibraryImport]`s (`VirtualAlloc`,
`VirtualProtect`, `VirtualFree`, `FlushInstructionCache`, `GetCurrentProcess` and `RtlAddFunctionTable`), and a
function-pointer call. It works the same under JIT, ReadyToRun, single-file and Native AOT. The hosted reading of the
fix is JIT (behavioral-smoke); the AOT seat that follows in this lane (`syscall.NewCallback` under Native AOT)
publishes an AOT program through the same funnel and so reads the stub under AOT too.

## 8. What is not changed

- The runtime package's own native calls (`stdcall` in `runtime/windows`) never pass or return floats; they are
  out of scope.
- arm64 keeps the current call (section 1).
- The deep-copy door (`NativeStructMarshal.CallDeep`) and the token refusals are unchanged.

## 9. Gates for the code

The red row then green (behavioral-smoke windows, filter `Windows`, so `WindowsNewCallback` reads in the same run);
the bytes test, made to fail; a windows `go2cs-stdlib` build of `syscall`; GolibTests R/D and `go test` (unchanged
code paths, run as the standard battery); repoguard. No converter change, so no CNR.
