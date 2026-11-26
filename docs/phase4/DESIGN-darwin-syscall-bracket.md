# DESIGN — darwin's syscall keystones enter a system call the way Go's do

**Lane R, 2026-11-26.** The darwin os/exec Wait note owed after D2/T6 (cc P5). It roots the BOARD row "os/exec Wait
blocks invisibly on darwin under the M:N scheduler" (R, 2026-11-25, on claude/r-mn-n5-sleep, not yet landed) and
proposes the cut. Read at master `d6805be2e8` and go1.26.11's source. Nothing here is built or run yet.

## 1. The finding

MnInvisible's C-exec census row puts K goroutines in `exec.Cmd.Wait` under `GO2CS_MN=1`:
- hosted osx-x64 and osx-arm64 (run 45576163996) read handoffs 0, retakes 0 and injections 10-21 in each of three runs;
- linux reads retakes 15-16 and 0 injections;
- windows reads handoffs 576, retakes 16 and 0 injections.

So on darwin the scheduler never learns that the goroutine is in a system call. Each waiter keeps its carrier, and the
monitor answers with injected threads. Nothing hangs (released 3/3).

## 2. The root (read, not yet measured)

The path from `Cmd.Wait` on darwin:

```
os/darwin/exec_unix.cs:66        (*Process).pidWait -> syscall.Wait4
syscall/darwin/syscall_bsd.cs:142 Wait4 -> wait4
syscall/darwin/zsyscall_darwin_amd64.cs:48  wait4 -> syscall6(libc_wait4_trampoline, ...)
syscall/darwin/syscall_darwin.cs:354         syscall6 -> syscalln   (converted Go body)
syscall/darwin/syscall_darwin_impl.cs:96     syscalln -> calln -> GoLibcCall.Call   (hand-owned)
golib/GoLibcCall.cs                          an unmanaged call: no transition seam
```

In Go, `syscall.syscalln` is the runtime's `syscall_syscalln`, pushed by `//go:linkname` (runtime/sys_darwin.go:37-42):
`entersyscall(); r1, r2, err = syscall_rawsyscalln(fn, args...); exitsyscall()`. Go brackets four entries and leaves
three raw:

| entry | Go | converted (`syscall_darwin_impl.cs`) |
|---|---|---|
| `syscalln` | entersyscall / exitsyscall (runtime/sys_darwin.go:38-40) | no bracket |
| `Syscall`, `Syscall6`, `Syscall9` | entersyscall / exitsyscall (syscall/asm_darwin_amd64.s:16-92, asm_darwin_arm64.s) | no bracket |
| `rawsyscalln`, `RawSyscall`, `RawSyscall6` | raw | raw |

- The file's header says why: "the cooked and raw forms differ only by entersyscall/exitsyscall, which the managed host
  already discharges, so both are the same call here".
- That was true before the native-call seam existed. Under M:N, `entersyscall` (runtime/syscall_managed_impl.cs, S-b)
  moves the goroutine to `_Gsyscall` and opens `GoNativeCall`'s transition, which is what lets the scheduler hand off
  or retake a blocked goroutine's carrier.
- linux had the same gap and closed it on 2026-11-09: its `runtime_entersyscall`/`runtime_exitsyscall` pair now
  forwards (syscall/linux/syscall_linux_impl.cs:70-85; GolibTests' LinuxSyscallTransitionTests, red first).

**Forwarding is not available here.** The runtime's converted `syscall_syscalln` (runtime/darwin/sys_darwin.cs) does
bracket, but it calls `syscall_rawsyscalln`, which reads errno through `libc_error_addr`, and that refuses by name.
runtime/darwin/libccall_impl.cs:228-247 says so: "syscall bodies its own syscalln and rawsyscalln ... so the pushed body
is never called". So the cut brackets in place, in the shape Go's runtime function has.

**A premise to correct.** DarwinNativeTransitionTests' header says "Every libcCall caller is already inside the window:
the converted darwin runtime's syscall_syscalln calls entersyscall". That holds for the runtime's own libcCall callers.
It does not hold for the syscall package, which never reaches that function. The cut amends the header.

## 3. Reach

- `zsyscall_darwin_amd64.cs` has 123 libc wrappers through the keystones. 33 use `rawSyscall`/`rawSyscall6` (raw in Go,
  and they stay raw). About 90 gain the bracket, `wait4`, `read`, `write`, `accept` and `kevent` among them.
- Both Mac RIDs run this one darwin build.
- `internal/syscall/unix`'s darwin pulls (`GoSyscall6X`, `GoSyscallPtr`, and the exported `Syscall*`) reach the same
  entries, so they are covered.
- `golang.org/x/sys/unix` is not vendored in the corpus (only `x/sys/cpu`), so there is no second path.
- The runtime's own libcCall callers are already bracketed (DarwinNativeTransitionTests).

## 4. The cut

In `syscall/darwin/syscall_darwin_impl.cs`:

```csharp
internal static partial (uintptr r1, uintptr r2, Errno err) syscalln(uintptr fn, params variadic<uintptr> argsʗp) {
    runtime_package.syscallEntersyscall();
    try {
        return calln(fn, argsʗp);           // rawsyscalln's body, unchanged
    }
    finally {
        runtime_package.syscallExitsyscall();
    }
}
```

- `Syscall`, `Syscall6` and `Syscall9` take the same bracket. `rawsyscalln`, `RawSyscall` and `RawSyscall6` keep none.
- `try/finally` rather than Go's straight-line pair: Go's libc call cannot throw, but GoLibcCall can (a null function
  pointer, more than nine arguments). A throw that left the goroutine in `_Gsyscall` would corrupt the scheduler's
  count.
- The header paragraph above is replaced with the new reason.
- Not changed: golib, the converter, the generator, the runtime. Hand-owned file edits only, so no corpus footprint.

**Attribution** (the owner's 2026-11-25 rule): the bracket follows runtime/sys_darwin.go's `syscall_syscalln` and
syscall/asm_darwin_amd64.s's `Syscall`, `Syscall6` and `Syscall9`. The file gains a provenance line naming them
("INSPIRED BY": the same bracket around golib's call).

**Cost without M:N** (the default): one `GoNativeCall.Enter`/`Exit` pair per darwin syscall, as linux already pays.

## 5. Red-first and the near-miss

1. **GolibTests, darwin-only** (the hosted golib darwin leg): a probe `[UnmanagedCallersOnly]` function stands in for
   the libc entry, as DarwinNativeTransitionTests does, and records the goroutine's status and the seam's depth from
   inside the call.
   - Through `syscalln` and through `Syscall`, `Syscall6` and `Syscall9`: `_Gsyscall`, depth + 1. RED at master
     (`_Grunning`, no depth change).
   - Through `rawsyscalln`, `RawSyscall` and `RawSyscall6`: `_Grunning`, depth unchanged, in both trees (the near-miss
     that keeps raw raw).
   - A throwing call (a null function pointer) leaves the status `_Grunning` afterwards (the `finally`).
2. **The outcome**: MnInvisible's C-exec row on hosted osx-x64 and osx-arm64. Predicted: retakes above 0 and injections
   near 0, the linux shape. Measured by the cut's run, not before it.
3. **No behaviour change outside M:N**: the darwin legs of release-smoke and the behavioral smoke.

## 6. Questions for P5 and COORD

- **Owner.** The cut is small (one hand-owned file and one test file). Q3's default puts it with the lane on row 13
  (R). P5 owns the darwin run layer; say if it is yours.
- **Train.** It changes every darwin syscall's status transition. Behaviour without M:N is unchanged, but it is
  darwin-wide, so it rides a train with the darwin hosted legs in its gate.
