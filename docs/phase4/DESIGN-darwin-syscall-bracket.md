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

## 7. Amendment, 2026-11-26 (COORD 20261126T071719Z, adopting P3's F1-F7)

Sections 1-6 stand as written above; this block corrects and extends them.

- **F1, section 5 item 3 is wrong.** "No behaviour change outside M:N" does not hold: `entersyscall` runs in
  default mode too (it moves the goroutine to `_Gsyscall` and `GoNativeCall.Enter` opens the transition whatever the
  scheduler), so every darwin cooked syscall now changes status in default mode, as linux's have since 2026-11-09.
  The default-mode arm is measured by row, not assumed: `AGoroutineBlockedInTheConvertedReadReadsGsyscall` is
  linux's F-1 row run on darwin (a goroutine blocked in `syscall.Read` on a pipe reads `_Gsyscall`, depth 1, with
  `GO2CS_MN` unset), and `AGoroutineBlockedInARawReadStaysGrunning` is its raw near-miss.
- **F2, the default-mode sweep joins the gate.** A hosted darwin default-mode sweep of `os`, `os/exec`, `net`,
  `internal/poll`, `syscall` and `os/signal` on osx-arm64 and osx-x64, verdict for verdict against the banked rows.
- **F3/F4, the rows.** GolibTests' `DarwinSyscallBracketTests` carries, beside the direct probe rows of section 5
  item 1, the real-wrapper rows (the converted `syscall6X` and `syscallPtr` through `GoSyscall6X`/`GoSyscallPtr`, a
  blocked `syscall.Read`, and `RawSyscall` on libc's `read`) and throw rows for all four bracketed entries (a null
  function pointer through each; ten arguments through `syscalln`), each asserting the status, the seam depth and
  that golib's original `ArgumentException` surfaces.
- **F5, attribution.** The file carries two notices: `syscalln` DERIVED FROM `src/runtime/sys_darwin.go`
  `syscall_syscalln` (Copyright 2018); `Syscall`, `Syscall6` and `Syscall9` INSPIRED BY
  `src/syscall/asm_darwin_amd64.s` (Copyright 2009); the rest original. Both old-reason statements in the header are
  replaced.
- **F6, the window.** The bracketed window is wider than Go's by golib's managed marshalling and copy-back
  (`NativeStructMarshal`) around the libc call. That is safe: the thread keeps running managed code it owns, and
  errno is read inside the call, before the exit.
- **F7.** The pre-existing order in `enterSyscall` (the status moves to `_Gsyscall` before `GoNativeCall.Enter`,
  which can throw) is recorded on the BOARD; this cut does not change it.

**The gate**: darwin golib-tests, go-test and mn-platform on osx-arm64 and osx-x64; MnInvisible's C-exec row on both
macs (predicted: retakes above 0, injections near 0); the F2 sweep; linux and windows CNR plus golib; repoguard.

### 7.1 After P3's code read, 2026-11-26 (P3 101328Z, COORD 103317Z)

- **The gate as run, and mn-platform.** The line above lists mn-platform on osx-arm64 and osx-x64. The workflow's
  mn-platform stage has one darwin leg, osx-arm64, so **osx-x64 mn-platform is NOT MEASURED**. It read PASS on
  osx-arm64 (run 45779434937). What stands in for it on osx-x64 is the MnInvisible C-exec read under M:N on the probe
  ref with the bracket (claude/r-dsb-mn-b, run 45786435129): retakes 15-16, injections 0. The same ref without the
  bracket (claude/r-dsb-mn-a, run 45786429427) read retakes 0, injections 14-15 there.
- **The throw rows made to fail (P3 finding 1).** They were green before the bracket existed, so nothing had shown
  they can catch a lost exit. Probe ref claude/r-dsb-throw-probe (d8183a7e3f, measurement only, never merges)
  removes the finally from Syscall9 and makes enter, call, exit a straight line. Hosted golib-tests on darwin, run
  45804048284, filtered to DarwinSyscallBracketTests: on both macs 15 passed and 1 failed, the predicted row
  `Syscall9OnANullPointerThrowsAndLeavesGrunning`. Its assert reads `_Gsyscall` (3) after the throw, where
  `_Grunning` (2) is wanted, because the exit never ran. No other row moved, and the host did not go down.
