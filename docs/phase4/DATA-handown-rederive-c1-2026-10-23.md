# DATA — the hop's hand-own re-derive: 32 `.cs.auto` siblings at `1ffa0f708f` (C1, 2026-10-23)

> **A point-in-time record.** G's freeze fixed-point regen at hop tip `1ffa0f708f` found the committed `.cs.auto`
> review siblings stale: they still carried go1.24.13 source and older emission. This record answers COORD's question
> per sibling. It asks whether the HAND-OWN beside each sibling carries its Go source's go1.24.13 → go1.26.10 change,
> and it refreshes the stale siblings. **Three hand-owns are MISSING a change (§3). They are findings for COORD to
> route as their own fixes, and no hand-owned `.cs` changes in this commit.**

## 1. Method

- **Emission:** the `.cs.auto` siblings were refreshed from a seeded `-stdlib -comments` emission at `1ffa0f708f`.
  - Each target had its own root, `-platforms <goos>/amd64`, and the three ran serially.
  - Each root was seeded from ONE `git archive` of the tip: `src/core`, `src/version.props`, `docs/validation` and the
    roster.
  - The converter was built once with `-trimpath -buildvcs=false` (sha256 `db164d7614387740…`) by go1.26.10.

| Target | Exit | Wall | Seeded `.cs` / `.cs.auto` | Written `.cs` / `.cs.auto` |
|:--|:--:|:--:|:--:|:--:|
| windows | 0 | 414 s | 4452 / 32 | 9 / 25 |
| linux | 0 | 252 s | 4452 / 32 | 10 / 21 |
| darwin | 0 | 242 s | 4452 / 32 | 9 / 20 |

- **Why the counts differ:** `.cs` writes are skipped when the bytes are unchanged, so their count measures only
  whether the bytes changed. `.cs.auto` is `os.Create` every time, so its count is the true emission set.
- **Comparison:** each committed sibling against its own target's emission, CR-stripped.
- **Go source diffs:** go1.24.13 against go1.26.10 GOROOTs, per owning `.go` file.
- **Hand-own review:** the behavioral hunks were reviewed against the hand-owned `.cs` with read-only file:line
  evidence. The key citations below were re-checked by hand.

## 2. Which siblings were stale, and the refresh

**26 stale**, exactly COORD's count: 20 flat, 5 windows-folder, 1 linux-folder, 0 darwin. Every one differs from its
own target's emission at the tip. All 26 are overlaid wholesale from that emission. The flat siblings are taken from
the windows emission, the corpus reference target. `syscall/linux/exec_unix.cs.auto` is taken from linux. The total
is +2082/−667 over 26 files, and no file is a CR-only phantom.

**Six were not written by any `-stdlib` arm**, so they are not refreshed here:

- `internal/godebug/godebug.cs.auto`;
- the linux-folder `internal/poll/linux/fd_writev_unix`, `internal/syscall/unix/linux/siginfo_linux` and
  `os/linux/wait_waitid`. Their Go sources are byte-identical across the hop;
- the two windows TEST siblings, `internal/syscall/windows/exec_windows_test` and `.../registry/registry_test`, which
  only a `-tests` emission writes.

COORD's 26 likewise leaves these six out.

**One flat sibling varies by target.** For `runtime/runtime2.cs.auto` the three emissions differ from one another,
and each is 591 diff lines from the committed copy. The windows form is committed. The others differ only as
follows:

- linux: `public partial struct g`, and `sigmask` heads `m`'s `[GoValueClone]` list;
- darwin: `mPadded`'s `unsafe.Sizeof(m{})` is 1936 against windows' 1912.

This is the standing flat-sibling-of-a-per-GOOS-emission class.

## 3. Per hand-own: does it carry the go1.24.13 → go1.26.10 change?

**Go source unchanged across the hop (nothing owed by the hand-own):** fd_writev_unix, siginfo_linux, wait_waitid,
crypto/internal/boring/bcache/cache, runtime/metrics/sample, sync/mutex, sync/poolqueue, sync/rwmutex, time/tick and
syscall/security_windows. Seven of these siblings were still stale, on emission drift alone.

**Go source changed: 22 files.** 19 are YES (every behavioral change is reflected or does not apply) and 3 are
MISSING:

| Go source (diff lines) | Hand-own | Verdict | Evidence |
|:--|:--|:--|:--|
| runtime/mcleanup.go (675) | runtime/mcleanup.cs | **MISSING (message)** | Go 1.26 keeps "ptr is equal to arg" for an IDENTICAL pointer and adds `panic("runtime.AddCleanup: ptr is within arg, cleanup will never run")` for an `arg` pointing INSIDE `ptr`'s object (non-tiny spans). The C# guard compares REFERENTS (mcleanup.cs:179-180), so an interior pointer (`&obj.f`) does panic, but with the "equal to arg" text. The check fires; the message differs, and `recover()` can see it. No Go test asserts the text. Everything else is reflected (closes-over-ptr panic :194-208, queue stats, the unique/sync test hooks :331-340) or does not apply (`debug.checkfinalizers`: no Go mark phase under the CLR). |
| runtime/mfinal.go (99) | runtime/mfinal.cs | yes | queued/executed counters and `finReadQueueStats` (:113-114, :205, :260-269, :365); `runfinq` → `runFinalizers`; new throw texts. |
| runtime/runtime2.go (462) | runtime/runtime2.cs | yes | every new, removed or renamed constant, field and type (G states, `maybeTraceablePtr`/`Chan`, `mWeakPointer`, `ncpu` → `numCPUStartup`). The `waitReason` renumbering is safe because golib maps wait reasons by name. `mPadded`'s padding is omitted by design (:686-690). |
| sync/waitgroup.go (167) | sync/waitgroup.cs | yes | `WaitGroup.Go` (:234-248): Done runs on return and on Goexit, but not on a panic, as in 1.26. The synctest-bubble checks are present (:103-112, :128-163, :200-212). |
| sync/oncefunc.go (109) | sync/oncefunc.cs | yes | `f` is cleared even when it panics (`finally` at :71-73, :114-116, :159-161). |
| sync/once.go (10) | sync/once.cs | **MISSING (representation)** | Go 1.26 changed `done` from `atomic.Uint32` to `atomic.Bool` (`!Load()` / `Store(true)`). once.cs:37 is still `atomic.Uint32`, with integer `Load`/`Store` at :77, :88-89. `Do` behaves identically; only reflection or `%#v` of the unexported field sees the old type. |
| sync/pool.go (2) | sync/pool.cs | yes | `runtime_StoreReluintptr` dropped its result; the native pool never calls it. |
| sync/atomic/type.go (14), value.go (6) | sync/atomic/type.cs, value.cs | yes | documentation only. |
| internal/sync/hashtriemap.go (6) | internal/sync/hashtriemap.cs | yes | the longer "ran out of hash bits" text cannot occur, because the C# map has no hash-bit walk. A parameter rename has no effect. |
| weak/pointer.go (10) | weak/pointer.cs | yes | documentation only ("if and only if"). |
| unique/clone.go (2) | unique/clone.cs | yes | `abi.EscapeToResultNonString` is a compiler escape hint with no CLR counterpart. |
| syscall/dll_windows.go (113) | syscall/windows/dll_windows.cs | yes | Syscall*/SyscallN bodies (:292-324); `syscalln` via the managed trampoline (:150-176); loadlibrary/loadsystemlibrary/getprocaddress over `LoadLibraryExW` (:263-290); the `__LoadLibraryExW` vars are replaced by `[LibraryImport]` (:92, :110). |
| syscall/exec_windows.go (61) | syscall/windows/exec_windows.cs | yes | `envSorted` (:167-208), used by `createEnvBlock` (:220-223). The 1.25 attribute-list rework is behavior-neutral over a natively allocated list (:630-672, :722-726). |
| net/lookup_windows.go (76) | net/windows/lookup_windows.cs | yes | all 14 hunks: the buffered channel in `lookupProtocol` (:113, :128); `newDNSError` at every new site (:148, :199, :290, :378, :418, :455, :486, :516, :559); the UTF16 error returns (:321-327, :396-401, :588-591). |
| internal/syscall/windows/registry/value.go (6) | registry/windows/value.cs | yes | `DeleteValue` returns the `UTF16PtrFromString` error (:357-363). |
| registry/registry_test.go (18) | registry/registry_test.cs | yes | `slices.Equal` against the file's own `equalStringSlice` compares identically. The Test function set is unchanged. |
| internal/syscall/windows/exec_windows_test.go (5) | exec_windows_test.cs | **MISSING** | `TestRunAtLowIntegrity` launches `os.Args[0]` (:58) where 1.26 uses `testenv.Executable(t)`. Under go2cs those come from the command line and the host module path respectively, and can differ under `dotnet <dll>`. Whether the verdict moves is not measured. The `SetTokenInformation` `uintptr` → `unsafe.Pointer` argument does not apply (:183 compiles through the implicit conversion). |
| syscall/exec_unix.go (23) | syscall/linux/exec_unix.cs | yes | 1.26 merges three execve hooks into `execveLibc`. On linux that is the default arm, which the hand-own already takes (`RawSyscall(SYS_EXECVE)`, :385). Lines 315-321 still declare the three 1.24-shaped hooks, which nothing references (cosmetic). The darwin `exec_unix.cs` is auto-converted, not a hand-own, and the darwin hand-own `exec_libc2_impl.cs` calls libc `execve` in both releases. |
| crypto/internal/fips140/subtle/xor_generic.go (2) | xor_generic.cs | yes | `!riscv64` is build-tag only. go2cs converts under `purego` (src/go2cs/commandLineOptions.go:255-272), whose `\|\| purego` arm selects this file on every target before and after. |
| hash/crc32/crc32_amd64.go (5) | crc32_amd64.cs | yes | the new constant is consumed only by assembly. The AVX-512 path computes the same CRC. |
| internal/godebug/godebug.go (8) | internal/godebug/godebug.cs | yes | Immutable settings are locked in `update()` (:284-296; `info` filled at :224). |

## 4. Noticed beside the hand-owns (not this seat's to fix)

- `golib/runtime/CrashReport.cs` never prints Go 1.26's `panic: X [recovered, repanicked]` suffix. The new
  `TestIssue76126` checks only for "panic: test", so it passes either way.
- Go 1.26's oncefunc test bounds a `OnceFunc`/`OnceValue`/`OnceValues` construction at 2 allocations. The C# version
  allocates about 4, which likely needs a disclosure once the sync tests are reconverted at 1.26.
- The committed `-tests` emissions are still 1.24-era in at least two places:
  - syscall's windows test files lack `EnvSorted` and `TestEnvBlockSorted`;
  - `runtime/mcleanup_test.cs` (2026-10-15) predates the converter's `AddCleanup` capture argument (2026-10-17).
    Its `TestCleanupUnreachableClosure` call at :451 uses the three-argument form.
  Both are for the test-source refresh at the re-bank.
