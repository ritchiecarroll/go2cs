# CENSUS — Phase 5 exported-API reach: which throwing stubs an ordinary Go program can reach

> **State: MEASURED, 2026-10-10, at master `2ddabcc67a`.** Static reading by an extension of C2's inventory;
> runtime reading by eleven probe programs converted with go2cs on linux (local), windows and darwin (CI).
> Nothing in `src/core`, the converter or the generators was changed. Taken by C1 on COORD's dispatch of the
> owner's question, for the Phase 5 priority list. Go declarations are read at the pinned toolchain, go1.24.13.
>
> **The question.** [CENSUS-phase5-stub-inventory.md](CENSUS-phase5-stub-inventory.md) lists every member
> that compiles against a throwing stub (229 windows · 245 linux · 447 darwin production members) and found
> that no validated row's tests reach one. It does not say whether **user code** can. This record asks: for
> each stub, which exported members of importable packages reach it, and does a small Go program calling
> that API actually throw?
>
> **Headline.**
> - **windows/linux: two exported APIs reach a stub at runtime, on every OS:**
>   `runtime/coverage.ClearCounters` (stub `internal/coverage/cfile.getCovCounterList`) and
>   `runtime.Breakpoint` (stub `runtime.breakpoint`). Both are confirmed by probe on windows, linux and
>   darwin. Both are small fixes.
> - **darwin only: two more, confirmed on arm64 and x64:** `crypto/x509.(*Certificate).Verify` with no
>   `Roots` (system-root verification; stub `crypto/x509/internal/macos.syscall`) and
>   `runtime/pprof.StartCPUProfile` (stub `runtime/pprof.mach_vm_region`).
> - The static upper bound is **16 windows · 18 linux · 19 darwin** stubs of class (c). Of the 18 distinct
>   windows/linux entries, 2 are confirmed, 12 are refuted by probe (the reflect and `os.Exit` paths are
>   hand-owned and route around the stub) and 4 are refuted by reading (name collisions, and a package no
>   converted project imports).
> - The other four `runtime/coverage` functions already return Go's exact errors (measured on linux, §3.1);
>   only `ClearCounters` touches the stub.
> - **(a)** 8 windows · 6 linux · 179 darwin; **(b)** 7 · 9 · 6; **unreached** 198 · 212 · 243.
> - The darwin libc trampolines (168 of darwin's (a)) are not called: `FuncPCABI0` resolves each one to its
>   libSystem export. The `os.Lstat` and `crypto/rand.Read` probe exits 0 on both darwin legs.

## 1. The instrument

**The population is C2's, unchanged.** Appendix A of the inventory, saved as
`src/go2cs/zz_phase5_inventory_test.go` (uncommitted), was run at this master. It reproduces the
inventory's totals: 229 / 245 / 447 production rows plus one test row per flavour. Its TSV is the input to
the extension. This record never defines "stub": a member is in scope if and only if Appendix A emits it.

**The extension** (Appendix A of this record, saved as `src/go2cs/zz_phase5_reach_test.go`, uncommitted)
reuses Appendix A's file walk (`p5Walk`, `p5Compile`), its literal and comment stripper (`p5StripFile`) and
its member patterns. It changes only the graph. The inventory's §5 graph is per package and rooted at the
package's own tests. This one spans every package of a flavour's production compile and is rooted at
exported API.

**Roots.** An exported member of an importable package (no `internal/` or `testdata/` segment, not
`vendor/`, not `golib`) whose name is also an exported func or method of that package in Go's own source.
The second condition removes converter-made helpers such as `ꓸꓸꓸ` adapters and `_package` plumbing.

**Three textual graphs**, all ignoring control flow and platform branches, as §5's does:

| Graph | Edges |
|:--|:--|
| STRICT | a bare token to the same package's member of that name; `alias.Name` and `pkg_package.Name` to that package's member |
| MEDIUM | STRICT, plus every `x.Name` member access to the member named `Name` in the file's own package and in each package the file imports (its `using` aliases) |
| LOOSE | MEDIUM, plus every `x.Name` member access to every package's member named `Name` |

**LOOSE is the upper bound, and it saturates**: about 3,000 roots reach most stubs, because common names
(`Write`, `String`, `init`) join every package to the runtime. STRICT and MEDIUM are where a path is worth
reading. **All three are upper bounds**: a textual edge is a call the code might make, not one it makes.

**Classes** (COORD's three, plus unreached):

| Class | Predicate |
|:--|:--|
| unreached | no LOOSE path from any root |
| (a) | Go declares the member but no Go source in GOROOT calls it: `go/ast` call sites, counted by name over the stub's package (asm entry points, intrinsics, cgo trampolines, `main_main`) |
| (b) | every LOOSE path from a root passes through a runtime-internal node (an unexported member of `runtime` or `internal/runtime/*`), so it is reachable only through runtime internals the converted runtime does not execute (`mcall`, `gogo`, `systemstack`) |
| (c) | otherwise: some path from an exported root avoids runtime internals |

For each stub the extension reports the best root: the shortest path from a STRICT non-runtime root, else
MEDIUM, else LOOSE. That tier is the one shown below.

## 2. Counts

| | windows | linux | darwin |
|:--|--:|--:|--:|
| inventory production rows | 229 | 245 | 447 |
| unreached | 198 | 212 | 243 |
| (a) not callable from Go | 8 | 6 | 179 |
| (b) runtime internals only | 7 | 9 | 6 |
| (c) reachable from exported API | **16** | **18** | **19** |
| (c) by best tier: strict · medium · loose | 6 · 6 · 4 | 6 · 7 · 5 | 9 · 6 · 4 |
| reached from a package-load root (`init`, static init), LOOSE | 35 | 51 | 165 |

Unreached by inventory class: windows asm 102 · linkname 87 · vestigial 7 · cgo 1 · intrinsic 1; linux
asm 114 · linkname 90 · vestigial 7 · intrinsic 1; darwin asm 97 · linkname 89 · cgo 49 · vestigial 7 ·
intrinsic 1.

## 3. Class (c), in full

Each row: the stub, its inventory class, the best root and path, the root counts per tier, and the verdict.
**CONFIRMED** means a probe threw `NotImplementedException` naming the stub. **REFUTED (probe)** means the
probe called the root API and matched Go. **REFUTED (read)** means the path was read and does not exist in
the converted code. **STATIC** means not probed.

### 3.1 windows/linux (also present on darwin)

| Stub | Inv. class | Best root [tier] and path | Roots S / M / L | Verdict |
|:--|:--|:--|:--|:--|
| `internal/coverage/cfile.getCovCounterList` | linkname | `runtime/coverage.ClearCounters` [strict] → `cfile.ClearCounters` → stub | 3 / 3 / 3 | **CONFIRMED** windows, linux, darwin arm64 + x64 |
| `runtime.breakpoint` | asm | `runtime.Breakpoint` [strict] → stub | 1 / 1 / 1 | **CONFIRMED** windows, linux, darwin arm64 + x64 |
| `reflect.getStaticuint64s` | linkname | `reflect.Field` [strict] → stub (`structType.Field`, type.cs:951) | 26 / 1010 (windows 1001) / ~3000 | REFUTED (probe): `reflect.Type.Field` is hand-owned `rtype.Field` (value_impl.cs:2812) |
| `reflect.unsafe_New` | linkname | `reflect.ConvertibleTo` [strict] → `convertOp` → `cvtDirect` / `cvtT2I` → stub | 2 / 86 / ~3000 | REFUTED (probe): `Value.Convert` is hand-owned |
| `reflect.ifaceE2I` | linkname | `reflect.ConvertibleTo` [strict] → `convertOp` → `cvtT2I` → stub | 2 / 86 / ~3000 | REFUTED (probe), as above |
| `reflect.typedmemmove` | linkname | `reflect.ConvertibleTo` [strict] → `convertOp` → `cvtDirect` / `cvtSliceArray` → stub | 2 / 86 / ~3000 | REFUTED (probe), as above |
| `internal/cpu.cpuid` | asm | `crypto/tls.CipherSuiteName` [medium] → `internal/cpu.Name` → stub | 0 / 1 / ~3000 | REFUTED (probe): the edge is the name `Name`; in the converted tree `cpu.Name`'s only caller is `internal/sysinfo` |
| `reflect.call` | linkname | `net/rpc.ServeCodec` [medium] → stub (`.call` on rpc's own types) | 0 / 6 / ~3000 | REFUTED (probe): `Value.Call` is hand-owned |
| `reflect.memmove` | linkname | `net/rpc.ServeCodec` [medium] → `reflect.call` → `intFromReg` → stub | 0 / 6 / ~3000 | REFUTED (probe), behind `reflect.call` |
| `reflect.typedmemclr` | linkname | `net/rpc.ServeCodec` [medium] → `reflect.call` → stub | 0 / 6 / ~3000 | REFUTED (probe), behind `reflect.call` |
| `reflect.typedmemclrpartial` | linkname | `net/rpc.ServeCodec` [medium] → `reflect.call` → stub | 0 / 6 / ~3000 | REFUTED (probe), behind `reflect.call` |
| `runtime.abort` | asm | `net/http.RoundTrip` [medium] → `net/http.roundTrip` → stub (`.abort` on http's own types) | 0 / 172 / ~3000 | REFUTED (read): name collision |
| `runtime.exit` (linux) | asm | `runtime/debug.String` [medium] → `runtime.Main` → stub | 649 / 1872 / 3020 | REFUTED (probe): `os.Exit(3)` exits 3 on all three OS |
| `internal/runtime/maps.mapKeyError` | linkname | `encoding/gob.RegisterName` [loose] → `maps.Delete` → stub (`.Delete` on `sync.Map`) | 0 / 0 / ~3000 | REFUTED (probe): an unhashable map key panics with Go's message |
| `reflect.growslice` | linkname | `bytes.Grow` [loose] → `reflect.grow` → stub (`.grow` on `bytes.Buffer`) | 0 / 0 / ~3000 | REFUTED (probe): `Value.Grow` and `reflect.Append` are hand-owned |
| `vendor/golang.org/x/sys/cpu.cpuid` | asm | `archive/zip.NewReader` [loose] → `cpu.init` → `archInit` → stub (`.init` on zip's types) | 0 / 0 / ~3000 | REFUTED (read): no converted project references this package, so its `init` never runs |
| `vendor/golang.org/x/sys/cpu.xgetbv` | asm | as above | 0 / 0 / ~3000 | REFUTED (read), as above |
| `runtime.connect` (linux) | asm | `log/syslog.Dial` [loose] → stub (`.connect` on `syslog.Writer`) | 0 / 0 / 3020 | REFUTED (read): name collision |

16 rows on windows (all but `runtime.exit` and `runtime.connect`), 18 on linux, and the same 16 on darwin.

**The coverage siblings.** `runtime/coverage` has five exported functions, each forwarding to
`internal/coverage/cfile`. Only `ClearCounters` reaches the stub: it calls `getCovCounterList()` first
(apis.cs:72). `WriteCounters` calls it only after the covermode check (apis.cs:51-55), and
`WriteCountersDir`, `WriteMeta` and `WriteMetaDir` never call it. In a binary built without `-cover`,
`cmode` is invalid and `finalHashComputed` is false, so all four return an error before any stub.
**Measured on linux** (a five-call program, `ClearCounters` last; the converted build against this
master's corpus with `GoTargetOS=linux`):

| API | go1.24.13 prints | Converted |
|:--|:--|:--|
| `WriteCounters(io.Discard)` | `WriteCounters invoked for program built with -covermode=<invalid> (please use -covermode=atomic)` | equal |
| `WriteCountersDir(dir)` | `WriteCountersDir invoked for program built with -covermode=<invalid> (please use -covermode=atomic)` | equal |
| `WriteMeta(io.Discard)` | `error: no meta-data available (binary not built with -cover?)` | equal |
| `WriteMetaDir(dir)` | `error: no meta-data available (binary not built with -cover?)` | equal |
| `ClearCounters()` | `program not built with -cover` | throws `NotImplementedException: getCovCounterList`, exit 2 |

So the fix is the one stub: with `getCovCounterList` returning an empty list, `ClearCounters` returns
Go's error, and `WriteCounters` keeps returning the covermode error before it would reach the same call.

### 3.2 darwin only

| Stub | Inv. class | Best root [tier] and path | Roots S / M / L | Verdict |
|:--|:--|:--|:--|:--|
| `crypto/x509/internal/macos.syscall` | linkname | `crypto/x509.Verify` [strict] → `systemVerify` → `macos.SecTrustSetVerifyDate` → stub | 1 / 186 / 3002 | **CONFIRMED** arm64 + x64: thrown from `CFArrayCreateMutable` (corefoundation.cs:171) ← `systemVerify` (root_darwin.cs:22) ← `Verify` (verify.cs:812) |
| `runtime/pprof.mach_vm_region` | linkname | `runtime/pprof.StartCPUProfile` [strict] → `profileWriter` → `newProfileBuilder` → `readMapping` → `machVMInfo` → stub | 4 / 6 / 3002 | **CONFIRMED** arm64 + x64: thrown on the `profileWriter` goroutine |
| `runtime/pprof.proc_regionfilename` | linkname | as above → `machVMInfo` → `regionFilename` → stub | 4 / 6 / 3002 | STATIC: behind `mach_vm_region` on the same path, so the probe throws before reaching it |

`Verify` reaches `systemVerify` only when `VerifyOptions.Roots` is nil, which is the default for HTTPS
clients (`crypto/tls` verifies server certificates this way when `RootCAs` is unset).

## 4. Classes (a) and (b): counts and examples

### (a) — Go declarations ordinary Go code cannot call

windows 8 (best tier: medium 3, runtime-internal only 5) · linux 6 (medium 3, runtime-internal only 3) ·
darwin 179 (strict 144, medium 5, loose 4, runtime-internal only 26). By inventory class: windows asm 5 ·
intrinsic 2 · linkname 1; linux asm 3 · intrinsic 2 · linkname 1; darwin cgo 168 · asm 8 · intrinsic 2 ·
linkname 1.

| Stub | Why Go code cannot call it | Path the text finds |
|:--|:--|:--|
| `internal/runtime/sys.GetCallerPC` / `GetCallerSP` | compiler intrinsics | `bufio.Scan` → `runtime.start` → stub |
| `reflect.methodValueCall` | asm entry point, taken by address | `net/rpc.ServeCodec` → `reflect.call` → `assignTo` → `makeMethodValue` → `methodValueCallCodePtr` → stub |
| `runtime.mstart`, `runtime.tstart_stdcall` (windows) | asm entry points, taken by address | `runtime/debug.String` → `runtime.Main` → `newm` → `newm1` (→ `newosproc`) → stub |
| `runtime.goexit`, `runtime.asyncPreempt` (windows) | asm entry points, taken by address | `bufio.Scan` → `runtime.start` → … → `recovery` / `preemptM` → stub |
| `runtime.main_main` | linkname to the user's `main.main` | `runtime/debug.String` → `runtime.Main` → stub |
| darwin libc trampolines, e.g. `x509_CFDataCreate_trampoline` ← `crypto/x509.Verify`, `libc_readlinkat_trampoline` ← `os.Lstat`, `libc_arc4random_buf_trampoline` ← `crypto/rand.Read`, `libc_open_trampoline` ← `syscall.Open` | asm trampolines, passed to `syscall_syscall` by address only; `FuncPCABI0` (internal/abi/funcpc_impl.cs) resolves each through the `GoCgoDynamicImports` record to the libSystem export, so the stub body never runs | strict |

Runtime evidence for the trampoline reading: P5OsRand (`os.Mkdir`, `os.Lstat`, `crypto/rand.Read`) exits
0 with Go's output on darwin arm64 and x64.

### (b) — reachable only through runtime internals

windows 7 · linux 9 · darwin 6. All are asm except `runtime.time_now` (linkname).

| Stub | Flavours | Path (runtime-internal hop marked) |
|:--|:--|:--|
| `runtime.mcall` | all | `bufio.Scan` → *`runtime.start` → `deferreturn` → `nextDefer`* → stub |
| `runtime.gogo` | all | `text/template.Execute` → *`runtime.execute`* → stub |
| `runtime.publicationBarrier` | all | `bytes.Grow` → *`runtime.grow` → `alloc` → `userArenaNextFree`* → stub |
| `runtime.asmcgocall` | all | `runtime.SetCgoTraceback` → *`runtime.cgocall`* → stub |
| `runtime.checkASM` | all | `sync.Broadcast` → *`runtime.check`* → stub |

The remaining (b) rows: `runtime.time_now` (windows, linux), `runtime.getlasterror` (windows),
`runtime.callCgoMmap`, `runtime.callCgoMunmap` and `runtime.osyield` (linux), `runtime.asmcgocall_no_g`
(darwin). In four of the five examples the first hop is a name collision (`.start`, `.execute`, `.grow`,
`.check`) into a runtime function the converted runtime does not run; `SetCgoTraceback` → `cgocall` is
Go's own call (traceback.go:1540), taken only when cgo is linked.

## 5. Probes

Ref `claude/c1-phase5-probes` = `ae7517a953` (to be deleted after this record lands): eleven projects under
`src/tests/Behavioral/P5*`, each a self-checking Go program. **Exit 0 means Go-equal; exit 10 means a
mismatch.** P5OsExit expects exit 3. Run through the `behavioral-stderr` stage of `os-matrix.yml`, which
transpiles and compiles each project with the runner and then runs the executable.

| Probe | Calls | Targets (c) stub(s) |
|:--|:--|:--|
| P5Coverage | `coverage.ClearCounters()`; expects Go's error `program not built with -cover` | `getCovCounterList` |
| P5Breakpoint | `runtime.Breakpoint()` between two prints | `runtime.breakpoint` |
| P5ReflectField | `reflect.TypeOf(T{}).Field(1)` | `getStaticuint64s` |
| P5ReflectConvert | `Value.Convert` to `int64`, to `any`, slice to array; `Type.ConvertibleTo` | `unsafe_New`, `ifaceE2I`, `typedmemmove` |
| P5ReflectCall | `Value.Call(strings.ToUpper)`, `reflect.Append`, `Value.Grow` | `reflect.call`, `memmove`, `typedmemclr*`, `growslice` |
| P5MapUnhashable | `m[[]int{1}] = 1` on `map[any]int`, recovered | `mapKeyError` |
| P5OsExit | `os.Exit(3)` | `runtime.exit` |
| P5TlsCpu | `tls.CipherSuiteName(TLS_AES_128_GCM_SHA256)` | `internal/cpu.cpuid` |
| P5X509Verify | self-signed ECDSA cert, `Verify(VerifyOptions{})`; expects an error | `macos.syscall` (darwin) |
| P5PprofCPU | `pprof.StartCPUProfile(io.Discard)`, `StopCPUProfile` | `mach_vm_region` (darwin) |
| P5OsRand | `os.Mkdir`, `os.Lstat`, `crypto/rand.Read` | darwin trampolines (class a) |

**Runs.** linux: local, all eleven. windows: run 38066158902 (job 114254167060: the three reflect probes,
Coverage, Breakpoint) and run 38066174845 (job 114256408984: MapUnhashable, OsExit, OsRand, PprofCPU,
TlsCpu, X509Verify). darwin: run 38066166908 (arm64 job 114254177634, x64 job 114254177661: X509Verify,
PprofCPU, OsRand, Coverage, Breakpoint).

| Probe | linux | windows | darwin arm64 | darwin x64 |
|:--|:--|:--|:--|:--|
| P5Coverage | exit 2, `getCovCounterList` | exit 2, `getCovCounterList` | exit 2, `getCovCounterList` | exit 2, `getCovCounterList` |
| P5Breakpoint | exit 2, `breakpoint` | exit 2, `breakpoint` | exit 2, `breakpoint` | exit 2, `breakpoint` |
| P5ReflectField | 0 | 0 | — | — |
| P5ReflectConvert | 0 | 0 | — | — |
| P5ReflectCall | 0 | 0 | — | — |
| P5MapUnhashable | 0 | 0 | — | — |
| P5OsExit | 3 | 3 | — | — |
| P5TlsCpu | 0 | 0 | — | — |
| P5X509Verify | 0 | 0 | **exit 2, `syscall`** | **exit 2, `syscall`** |
| P5PprofCPU | 0 | 0 | **exit 2, `mach_vm_region`** | **exit 2, `mach_vm_region`** |
| P5OsRand | 0 | 0 | 0 | 0 |

Exit 0 is each program's own check against Go's value. On windows the stage's stdout diff against `go run`
was also empty for every exit-0 and exit-3 row.

**What Go does at the two confirmed windows/linux sites.** `coverage.ClearCounters` in a binary built
without `-cover` returns the error `program not built with -cover`. `runtime.Breakpoint`, measured with the
pinned toolchain on linux: prints `before`, then `SIGTRAP: trace trap` and a goroutine traceback on stderr,
and exits 2. The converted program prints `before`, throws `NotImplementedException: breakpoint` and also
exits 2, so stdout and the exit code match and only stderr differs.

`recover()` does not catch these exceptions: a stub's `NotImplementedException` is not a Go panic.

## 6. Summary for a visitor

| Package | API | OS | Status |
|:--|:--|:--|:--|
| `runtime/coverage` | `ClearCounters` | windows, linux, darwin | **confirmed** stub reach |
| `runtime` | `Breakpoint` | windows, linux, darwin | **confirmed** stub reach (exit code matches Go; message does not) |
| `crypto/x509` | `(*Certificate).Verify` with nil `Roots` (system roots) | darwin | **confirmed** stub reach |
| `runtime/pprof` | `StartCPUProfile` | darwin | **confirmed** stub reach |
| `reflect` | `Type.Field`, `Value.Convert`, `Type.ConvertibleTo`, `Value.Call`, `Append`, `Value.Grow` | windows, linux | static path only; probe matches Go |
| `crypto/tls` | `CipherSuiteName` | windows, linux | static path only; probe matches Go |
| `os` | `Exit` | windows, linux | static path only; probe matches Go |
| maps (language) | unhashable key | windows, linux | static path only; probe matches Go |
| `os`, `crypto/rand` | `Lstat`, `Mkdir`, `Read` | darwin | static path to trampolines; probe matches Go |
| `net/http`, `log/syslog`, `archive/zip` | `RoundTrip`, `Dial`, `NewReader` | windows, linux | static path only; refuted by reading (name collisions, unreferenced package) |

## 7. Priority split

**windows/linux — the Phase 5 priority list, easiest first.**

1. `runtime/coverage.ClearCounters` → have `getCovCounterList` return an empty list. `ClearCounters` then
   returns Go's `program not built with -cover`. `WriteCounters` and `WriteCountersDir` already return the
   covermode error before reaching the stub.
2. `runtime.Breakpoint` → answer as Go does: write `SIGTRAP: trace trap` and exit 2 (on linux; the windows
   Go output was not measured here). `Debugger.Break()` when a debugger is attached is an option to rule on.

**darwin only — the idle-lane backlog, by how common the path is. Never ahead of nugetgo or the train.**

1. x509 system-root verification (`crypto/x509/internal/macos.syscall`): any HTTPS client on macOS with
   default roots. Confirmed.
2. pprof CPU profiling (`runtime/pprof.mach_vm_region`, then `proc_regionfilename` behind it). Confirmed.

**PLAN (nothing built yet).**

- *x509 system roots.* The stub is the one linkname push `crypto/x509/internal/macos.syscall`, declared
  for `runtime/darwin/sys_darwin.cs`. The 28 `x509_*_trampoline` members beside it are class (a) (21) or
  unreached (7): they are only passed by address to `syscall`, as the `syscall` package's `libc_*_trampoline` members are. The
  `syscall` package's own darwin path already works (P5OsRand), with `FuncPCABI0` resolving each
  trampoline to its libSystem export through the `GoCgoDynamicImports` record. The work: (1) supply
  `macos.syscall` from a hand-owned `*_impl.cs` that forwards to the same native-call path `syscall.syscall`
  uses; (2) confirm the CoreFoundation and Security symbols resolve through the import record, since these
  live in framework dylibs rather than libSystem; (3) turn P5X509Verify into a darwin behavioral test that
  expects the self-signed certificate's `UnknownAuthorityError`, plus a positive case that verifies an
  embedded public chain against the system roots at a fixed `CurrentTime`. Red first: the probe throws
  today on both legs.
- *pprof mapping.* Both stubs are linkname pushes for `runtime/darwin/sys_darwin.cs`. `readMapping` is
  already hand-owned (`runtime/pprof/darwin/proto_darwin_impl.cs`) and falls back when `machVMInfo`
  returns false (line 40). The smallest Go-equal step is to supply `mach_vm_region` returning a failing
  `kern_return_t`, so `machVMInfo` returns false and the profile carries the fallback mapping, which is
  what Go writes when the call fails. The full step is a libSystem P/Invoke for `mach_vm_region` and
  `proc_regionfilename`, so the profile names the real executable mapping. Red first: P5PprofCPU throws
  today on both legs.

## 8. Controls

A planted bodyless partial `phase5Control` in `src/core/unicode` (never committed; the tree read clean after
each arm). Both arms re-ran Appendix A of the inventory and then the extension, end to end.

| Arm | Planted file | Inventory | Reach row, every flavour | Other rows |
|:--|:--|:--|:--|:--|
| A | the declaration only | one row per flavour | `unreached` (0 roots in every tier) | unchanged; unreached +1 |
| B | the declaration plus a `public static bool In(...)` whose body calls it | one row per flavour | class (a) (`go_calls` 0), best root `unicode.In` [strict], path `unicode.In -> unicode.phase5Control`; 38 strict roots | unchanged; (a) +1 |

So the graph finds a planted path, through a real Go API name, and finds nothing when there is none. The
(c) branch is shown by runtime evidence instead: `getCovCounterList` and `breakpoint` are class (c) and
throw in the probes.

⚠ **The instrument has a layout limit, found by the first B arm.** Appendix A's member pattern
(`p5MethodRe`) matches a declaration only at column 0, which is how the converter writes members. The
first B arm planted an indented file: the call was attributed to no member, and the row read `unreached`.
With column-0 layout it reads as above. Hand-owned files often indent members, and an indented member is
neither a node nor a root (its tokens are charged to the preceding column-0 member). **Measured at this
master:** 927 non-test files outside `golib` hold 7,141 indented declarations; inside their bodies, a call
token naming an inventory stub occurs 9 times, and every one resolves to a same-named member of a
different package (`syscall.connect` and `syscall.syscall`/`syscall6` in the `syscall` hand-owns, a local
`call` delegate in `runtime/managed_impl.cs`). **No indented member calls a stub of its own package**, so
the limit hides no stub edge here; it would if a hand-owned file ever did.

## 9. How to re-take this reading

```
git worktree add <dir> 2ddabcc67a
cd <dir>/src/go2cs
# the inventory's Appendix A saved as zz_phase5_inventory_test.go, this record's Appendix A as
# zz_phase5_reach_test.go (both uncommitted)
PHASE5_OUT=inventory.tsv PHASE5_GOROOT="$(go env GOROOT)" \
  go test -count=1 -run 'TestPhase5StubInventory$' .
PHASE5_INV=inventory.tsv PHASE5_REACH=reach.tsv PHASE5_GOROOT="$(go env GOROOT)" \
  go test -count=1 -timeout 60m -run 'TestPhase5ExportedReach$' -v .
awk -F'\t' 'NR>1{print $1, $14}' reach.tsv | sort | uniq -c       # §2
awk -F'\t' '$14=="c"' reach.tsv                                     # §3
```

`reach.tsv` columns: goos, pkg, member, inv_class, go_name, go_calls, strict_roots, loose_roots,
load_reach, nonrt_strict_roots, nonrt_loose_roots, medium_roots, nonrt_medium_roots, reach_class,
best_root, best_path, root_pkgs.

Probes: dispatch `os-matrix.yml` on `claude/c1-phase5-probes` with stage `behavioral-stderr` and the
project list in `STDERR_PROJECTS`, one run per GOOS.

## Appendix A — the extension (uncommitted; save as `src/go2cs/zz_phase5_reach_test.go`)

```go
// zz_phase5_reach_test.go - Phase 5 exported-API reach (C1, 2026-10-10; COORD dispatch, owner question).
//
// An EXTENSION of C2's Phase 5 inventory (docs/phase4/CENSUS-phase5-stub-inventory.md, Appendix A, saved
// beside this file as zz_phase5_inventory_test.go), kept out of the tree. It does NOT define "stub": the
// population is read from the inventory's own TSV (PHASE5_INV, the production rows Appendix A emits), so
// the predicate is C2's, unchanged. It reuses Appendix A's file walk (p5Walk, p5Compile), its literal and
// comment stripper (p5StripFile) and its member/field/token patterns, and changes only the GRAPH: §5's
// textual call graph is per package and rooted at the package's own tests; this one spans every package of
// a flavour's PRODUCTION compile and is rooted at the EXPORTED members of every importable package.
//
// Three graphs, all textual (they ignore control flow and platform branches, like §5's):
//   STRICT: a bare token edges to the same package's member of that name; `alias.Name` / `pkg_package.Name`
//           edges to that package's member. Value-receiver calls (`x.Name`) edge only within the package.
//   MEDIUM: STRICT plus every `x.Name` member access edges to the member named Name in the file's OWN
//           package and in each package the file imports (its `using` aliases) -- dispatch resolved to
//           packages the file can name.
//   LOOSE:  MEDIUM plus every `x.Name` member access edges to EVERY package's member named Name -- the
//           method and interface dispatch the text cannot resolve. This is the upper bound.
// A stub's class: unreached (no LOOSE path from any root); (a) Go declares it but no Go source calls it
// (go/ast over GOROOT, call sites counted by name); (b) every path passes through a runtime-internal node
// (package runtime or internal/runtime/*, unexported); (c) otherwise.
//
//	PHASE5_INV=<inventory.tsv> PHASE5_REACH=<out.tsv> PHASE5_GOROOT=<goroot> go test -run TestPhase5ExportedReach -v .
package main

import (
	"bufio"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
	"unicode"
	"unicode/utf8"
)

var (
	p5rNamespaceRe = regexp.MustCompile(`^namespace\s+([^\s;{]+)\s*[;{]?`)
	p5rUsingRe     = regexp.MustCompile(`^using\s+([\pL_@][\pL\pN_@]*)\s*=\s*([^;<]+_package)\s*;`)
	p5rClassRe     = regexp.MustCompile(`partial\s+class\s+([\pL_@][\pL\pN_@]*_package)\b`)
	p5rGoFuncRe    = regexp.MustCompile(`^func\s+(?:\([^)]*\)\s*)?([\pL_][\pL\pN_]*)`)
)

type p5rNode struct{ pkg, name string }

func (n p5rNode) String() string { return n.pkg + "." + n.name }

func p5rImportable(pkg string) bool {
	if pkg == "golib" || pkg == "go2cs" || strings.HasPrefix(pkg, "golib/") || strings.HasPrefix(pkg, "vendor/") {
		return false
	}
	for _, seg := range strings.Split(pkg, "/") {
		if seg == "internal" || seg == "testdata" {
			return false
		}
	}
	return true
}

func p5rExported(name string) bool {
	r, _ := utf8.DecodeRuneInString(strings.TrimPrefix(name, "@"))
	return unicode.IsUpper(r)
}

// runtime-internal: an unexported member of runtime or of an internal/runtime/* package.
func p5rRuntimeInternal(n p5rNode) bool {
	return (n.pkg == "runtime" || strings.HasPrefix(n.pkg, "internal/runtime/")) && !p5rExported(n.name)
}

func p5rNormFQN(rhs string) string {
	rhs = strings.TrimSpace(strings.TrimPrefix(strings.TrimSpace(rhs), "global::"))
	if !strings.HasPrefix(rhs, "go.") {
		rhs = "go." + rhs
	}
	return rhs
}

type p5rGraph struct {
	members map[string]map[string]bool // pkg -> member names declared at class level
	byName  map[string][]string        // member name -> pkgs declaring it
	strict  map[p5rNode]map[p5rNode]bool
	medium  map[p5rNode]map[p5rNode]bool
	loose   map[p5rNode]map[p5rNode]bool
	roots   map[p5rNode]bool // exported members of importable packages
	loads   map[p5rNode]bool // init members and static initializers of every package (load-time)
}

// p5rGoAPI is the set of EXPORTED func and method names a package declares in Go's own non-test source
// at the pin, any GOOS: a root must be a name a user's Go code can actually call.
var p5rAPICache = map[string]map[string]bool{}

func p5rGoAPI(goroot, pkg string) map[string]bool {
	if c, ok := p5rAPICache[pkg]; ok {
		return c
	}
	c := map[string]bool{}
	dir := filepath.Join(goroot, "src", filepath.FromSlash(pkg))
	entries, _ := os.ReadDir(dir)
	fset := token.NewFileSet()
	for _, e := range entries {
		n := e.Name()
		if !strings.HasSuffix(n, ".go") || strings.HasSuffix(n, "_test.go") {
			continue
		}
		f, err := parser.ParseFile(fset, filepath.Join(dir, n), nil, parser.SkipObjectResolution)
		if err != nil {
			continue
		}
		for _, d := range f.Decls {
			if fd, ok := d.(*ast.FuncDecl); ok && fd.Name.IsExported() {
				c[fd.Name.Name] = true
			}
		}
	}
	p5rAPICache[pkg] = c
	return c
}

func p5rBuild(files map[string][]p5File, goos, goroot string) *p5rGraph {
	g := &p5rGraph{members: map[string]map[string]bool{}, byName: map[string][]string{},
		strict: map[p5rNode]map[p5rNode]bool{}, medium: map[p5rNode]map[p5rNode]bool{}, loose: map[p5rNode]map[p5rNode]bool{},
		roots: map[p5rNode]bool{}, loads: map[p5rNode]bool{}}

	fqn := map[string]string{}       // namespace.Class_package -> pkg
	simple := map[string][]string{}  // Class_package -> pkgs
	sets := map[string][]p5File{}
	for pkg, all := range files {
		set := p5Compile(all, goos, false)
		sets[pkg] = set
		g.members[pkg] = map[string]bool{}
		for _, f := range set {
			ns := "go"
			for _, line := range f.lines {
				if m := p5rNamespaceRe.FindStringSubmatch(line); m != nil {
					ns = m[1]
				}
				if m := p5rClassRe.FindStringSubmatch(line); m != nil {
					key := ns + "." + m[1]
					if _, ok := fqn[key]; !ok {
						fqn[key] = pkg
						simple[m[1]] = append(simple[m[1]], pkg)
					}
				}
				if m := p5MethodRe.FindStringSubmatch(line); m != nil {
					g.members[pkg][m[1]] = true
				}
			}
		}
	}
	for pkg, ms := range g.members {
		for name := range ms {
			g.byName[name] = append(g.byName[name], pkg)
		}
	}

	add := func(m map[p5rNode]map[p5rNode]bool, from, to p5rNode) {
		if from == to {
			return
		}
		if m[from] == nil {
			m[from] = map[p5rNode]bool{}
		}
		m[from][to] = true
	}

	tokRe := regexp.MustCompile(`[\pL_@][\pL\pN_@]*`)
	for pkg, set := range sets {
		for _, f := range set {
			aliases := map[string]string{}
			for _, line := range f.lines {
				if m := p5rUsingRe.FindStringSubmatch(line); m != nil {
					if target, ok := fqn[p5rNormFQN(m[2])]; ok {
						aliases[m[1]] = target
					}
				}
			}
			code := p5StripFile(f.lines)
			current := p5rNode{pkg, "<static-init>"}
			for li, line := range f.lines {
				if strings.HasPrefix(strings.TrimSpace(line), "//") || strings.HasPrefix(line, "using ") {
					continue
				}
				if p5FieldRe.MatchString(line) {
					current = p5rNode{pkg, "<static-init>"}
				} else if m := p5MethodRe.FindStringSubmatch(line); m != nil {
					current = p5rNode{pkg, m[1]}
					if partialDeclRe.MatchString(line) {
						continue // a bodyless declaration names nothing
					}
				}
				if p5rImportable(pkg) && p5rExported(current.name) && p5rGoAPI(goroot, pkg)[strings.TrimPrefix(current.name, "@")] {
					g.roots[current] = true
				}
				if current.name == "<static-init>" || strings.HasPrefix(current.name, "init") {
					g.loads[current] = true
				}
				src := code[li]
				locs := tokRe.FindAllStringIndex(src, -1)
				consumed := map[int]bool{}
				for ti, loc := range locs {
					if consumed[ti] {
						continue
					}
					tok := src[loc[0]:loc[1]]
					after := strings.TrimLeft(src[loc[1]:], " \t")
					before := strings.TrimRight(src[:loc[0]], " \t")
					qualifierPkg := ""
					if strings.HasPrefix(after, ".") && ti+1 < len(locs) {
						if p, ok := aliases[tok]; ok {
							qualifierPkg = p
						} else if ps := simple[tok]; len(ps) > 0 {
							qualifierPkg = ps[0]
							if len(ps) > 1 { // disambiguate by the dotted chain written before it
								chain := before
								for _, p := range ps {
									for k, v := range fqn {
										if v == p && strings.HasSuffix(chain+tok, strings.TrimPrefix(strings.TrimSuffix(k, "."+tok), "go.")+"."+tok) {
											qualifierPkg = p
										}
									}
								}
							}
						}
					}
					if qualifierPkg != "" {
						next := src[locs[ti+1][0]:locs[ti+1][1]]
						consumed[ti+1] = true
						if g.members[qualifierPkg][next] {
							to := p5rNode{qualifierPkg, next}
							add(g.strict, current, to)
							add(g.medium, current, to)
							add(g.loose, current, to)
						}
						continue
					}
					if g.members[pkg][tok] {
						add(g.strict, current, p5rNode{pkg, tok})
						add(g.medium, current, p5rNode{pkg, tok})
						add(g.loose, current, p5rNode{pkg, tok})
					}
					if strings.HasSuffix(before, ".") {
						for _, p := range aliases {
							if g.members[p][tok] {
								add(g.medium, current, p5rNode{p, tok})
							}
						}
						for _, p := range g.byName[tok] {
							add(g.loose, current, p5rNode{p, tok})
						}
					}
				}
			}
		}
	}
	return g
}

// p5rReverse inverts an edge map.
func p5rReverse(m map[p5rNode]map[p5rNode]bool) map[p5rNode][]p5rNode {
	r := map[p5rNode][]p5rNode{}
	for from, tos := range m {
		for to := range tos {
			r[to] = append(r[to], from)
		}
	}
	return r
}

// p5rReach walks backward from a stub. avoid, when set, refuses to pass THROUGH a runtime-internal node
// (the stub itself may be one). It returns every root reached with its distance and, for each, the next
// hop toward the stub (to rebuild one shortest path).
func p5rReach(rev map[p5rNode][]p5rNode, stub p5rNode, roots map[p5rNode]bool, avoid bool) (map[p5rNode]int, map[p5rNode]p5rNode) {
	dist := map[p5rNode]int{stub: 0}
	next := map[p5rNode]p5rNode{}
	queue := []p5rNode{stub}
	hit := map[p5rNode]int{}
	for len(queue) > 0 {
		n := queue[0]
		queue = queue[1:]
		if roots[n] && n != stub {
			hit[n] = dist[n]
		}
		if avoid && n != stub && p5rRuntimeInternal(n) {
			continue
		}
		for _, p := range rev[n] {
			if _, seen := dist[p]; !seen {
				dist[p] = dist[n] + 1
				next[p] = n
				queue = append(queue, p)
			}
		}
	}
	return hit, next
}

func p5rPath(from p5rNode, next map[p5rNode]p5rNode, stub p5rNode) string {
	var parts []string
	for n := from; ; n = next[n] {
		parts = append(parts, n.String())
		if n == stub || len(parts) > 40 {
			break
		}
	}
	return strings.Join(parts, " -> ")
}

// p5rGoCalls counts, in Go's own non-test source for the package at the pin, the CALL sites of a name
// (an identifier in call position, or pkg.Name / x.Name in call position for a method), ignoring build
// constraints (an upper bound on "ordinary Go code calls it").
var p5rCallCache = map[string]map[string]int{}

func p5rGoCalls(goroot, pkg string) map[string]int {
	if c, ok := p5rCallCache[pkg]; ok {
		return c
	}
	c := map[string]int{}
	dir := filepath.Join(goroot, "src", filepath.FromSlash(pkg))
	entries, _ := os.ReadDir(dir)
	fset := token.NewFileSet()
	for _, e := range entries {
		n := e.Name()
		if !strings.HasSuffix(n, ".go") || strings.HasSuffix(n, "_test.go") {
			continue
		}
		f, err := parser.ParseFile(fset, filepath.Join(dir, n), nil, parser.SkipObjectResolution)
		if err != nil {
			continue
		}
		ast.Inspect(f, func(node ast.Node) bool {
			if call, ok := node.(*ast.CallExpr); ok {
				switch fn := call.Fun.(type) {
				case *ast.Ident:
					c[fn.Name]++
				case *ast.SelectorExpr:
					c[fn.Sel.Name]++
				case *ast.IndexExpr:
					if id, ok := fn.X.(*ast.Ident); ok {
						c[id.Name]++
					}
				}
			}
			return true
		})
	}
	p5rCallCache[pkg] = c
	return c
}

func TestPhase5ExportedReach(t *testing.T) {
	inv, out := os.Getenv("PHASE5_INV"), os.Getenv("PHASE5_REACH")
	if inv == "" || out == "" {
		t.Skip("PHASE5_INV / PHASE5_REACH not set: this is a measurement, not a gate")
	}
	goroot := strings.TrimSpace(os.Getenv("PHASE5_GOROOT"))

	type stubRow struct{ goos, pkg, name, class, goDecl string }
	var stubs []stubRow
	fh, err := os.Open(inv)
	if err != nil {
		t.Fatal(err)
	}
	sc := bufio.NewScanner(fh)
	sc.Buffer(make([]byte, 1<<20), 1<<24)
	for first := true; sc.Scan(); first = false {
		if first {
			continue
		}
		c := strings.Split(sc.Text(), "\t")
		if len(c) < 14 || c[1] != "prod" {
			continue
		}
		stubs = append(stubs, stubRow{goos: c[0], pkg: c[2], name: c[4], class: c[12], goDecl: c[10]})
	}
	fh.Close()

	files, err := p5Walk(filepath.Join("..", "core"))
	if err != nil {
		t.Fatal(err)
	}

	w, err := os.Create(out)
	if err != nil {
		t.Fatal(err)
	}
	defer w.Close()
	bw := bufio.NewWriter(w)
	fmt.Fprintln(bw, "goos\tpkg\tmember\tinv_class\tgo_name\tgo_calls\tstrict_roots\tloose_roots\tload_reach\tnonrt_strict_roots\tnonrt_loose_roots\tmedium_roots\tnonrt_medium_roots\treach_class\tbest_root\tbest_path\troot_pkgs")

	for _, goos := range []string{"windows", "linux", "darwin"} {
		g := p5rBuild(files, goos, goroot)
		revS, revM, revL := p5rReverse(g.strict), p5rReverse(g.medium), p5rReverse(g.loose)
		nodes := 0
		for _, ms := range g.members {
			nodes += len(ms)
		}
		t.Logf("%s: %d packages, %d member names, %d exported roots, strict edges from %d nodes, loose from %d", goos, len(g.members), nodes, len(g.roots), len(g.strict), len(g.loose))

		for _, s := range stubs {
			if s.goos != goos {
				continue
			}
			stub := p5rNode{s.pkg, s.name}
			goName := s.name
			if m := p5rGoFuncRe.FindStringSubmatch(s.goDecl); m != nil {
				goName = m[1]
			}
			calls := p5rGoCalls(goroot, s.pkg)[goName]

			hitS, _ := p5rReach(revS, stub, g.roots, false)
			hitL, nextL := p5rReach(revL, stub, g.roots, false)
			loadHit, _ := p5rReach(revL, stub, g.loads, false)
			nrS, nextNS := p5rReach(revS, stub, g.roots, true)
			nrL, nextNL := p5rReach(revL, stub, g.roots, true)
			hitM, _ := p5rReach(revM, stub, g.roots, false)
			nrM, nextNM := p5rReach(revM, stub, g.roots, true)

			class := "unreached"
			switch {
			case len(hitL) == 0:
				class = "unreached"
			case calls == 0:
				class = "a"
			case len(nrL) == 0:
				class = "b"
			default:
				class = "c"
			}

			// Best root: prefer a STRICT non-runtime-internal path, then loose; shortest, then by name.
			pick := func(h map[p5rNode]int) (p5rNode, bool) {
				var best p5rNode
				bd := -1
				for r, d := range h {
					if bd < 0 || d < bd || (d == bd && r.String() < best.String()) {
						best, bd = r, d
					}
				}
				return best, bd >= 0
			}
			bestRoot, bestPath := "", ""
			if r, ok := pick(nrS); ok {
				bestRoot, bestPath = r.String()+" [strict]", p5rPath(r, nextNS, stub)
			} else if r, ok := pick(nrM); ok {
				bestRoot, bestPath = r.String()+" [medium]", p5rPath(r, nextNM, stub)
			} else if r, ok := pick(nrL); ok {
				bestRoot, bestPath = r.String()+" [loose]", p5rPath(r, nextNL, stub)
			} else if r, ok := pick(hitL); ok {
				bestRoot, bestPath = r.String()+" [via runtime internals]", p5rPath(r, nextL, stub)
			}
			rootPkgs := map[string]int{}
			src := nrL
			if len(src) == 0 {
				src = hitL
			}
			for r := range src {
				rootPkgs[r.pkg]++
			}
			var rp []string
			for p, n := range rootPkgs {
				rp = append(rp, fmt.Sprintf("%s:%d", p, n))
			}
			sort.Strings(rp)
			fmt.Fprintf(bw, "%s\t%s\t%s\t%s\t%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%s\t%s\t%s\t%s\n", goos, s.pkg, s.name, s.class, goName, calls,
				len(hitS), len(hitL), len(loadHit), len(nrS), len(nrL), len(hitM), len(nrM), class, bestRoot, bestPath, strings.Join(rp, ","))
		}
	}
	bw.Flush()
}
```

## Appendix B — every reached stub, per flavour

From `reach.tsv` at this master: every row whose class is not `unreached`. Roots are counts of exported
roots reaching the stub per graph (strict / medium / loose); `Go calls` is the call-site count by name in
the stub's Go package. The best root's tier is in brackets; `[via runtime internals]` means only a path
through runtime internals exists.

### windows (31 rows)

| Class | Stub | Inv. class | Go calls | Roots S / M / L | Best root |
|:--|:--|:--|--:|:--|:--|
| (c) | `internal/coverage/cfile.getCovCounterList` | linkname | 4 | 3 / 3 / 3 | runtime/coverage.ClearCounters [strict] |
| (c) | `internal/cpu.cpuid` | asm | 9 | 0 / 1 / 2995 | crypto/tls.CipherSuiteName [medium] |
| (c) | `internal/runtime/maps.mapKeyError` | linkname | 3 | 0 / 0 / 2995 | encoding/gob.RegisterName [loose] |
| (c) | `reflect.call` | linkname | 4 | 0 / 6 / 2995 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.getStaticuint64s` | linkname | 1 | 26 / 1001 / 2995 | reflect.Field [strict] |
| (c) | `reflect.growslice` | linkname | 1 | 0 / 0 / 2995 | bytes.Grow [loose] |
| (c) | `reflect.ifaceE2I` | linkname | 2 | 2 / 86 / 2995 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.memmove` | linkname | 4 | 0 / 6 / 2995 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclr` | linkname | 4 | 0 / 6 / 2995 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclrpartial` | linkname | 1 | 0 / 6 / 2995 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemmove` | linkname | 15 | 2 / 86 / 2995 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.unsafe_New` | linkname | 20 | 2 / 86 / 2995 | reflect.ConvertibleTo [strict] |
| (c) | `runtime.abort` | asm | 7 | 0 / 172 / 2995 | net/http.RoundTrip [medium] |
| (c) | `runtime.breakpoint` | asm | 1 | 1 / 1 / 1 | runtime.Breakpoint [strict] |
| (c) | `vendor/golang.org/x/sys/cpu.cpuid` | asm | 4 | 0 / 0 / 2995 | archive/zip.NewReader [loose] |
| (c) | `vendor/golang.org/x/sys/cpu.xgetbv` | asm | 1 | 0 / 0 / 2995 | archive/zip.NewReader [loose] |
| (b) | `runtime.asmcgocall` | asm | 59 | 572 / 2033 / 2995 | runtime.SetCgoTraceback [via runtime internals] |
| (b) | `runtime.checkASM` | asm | 1 | 572 / 2033 / 2995 | sync.Broadcast [via runtime internals] |
| (b) | `runtime.getlasterror` | asm | 21 | 572 / 2033 / 2995 | runtime/debug.String [via runtime internals] |
| (b) | `runtime.gogo` | asm | 7 | 572 / 2033 / 2995 | text/template.Execute [via runtime internals] |
| (b) | `runtime.mcall` | asm | 14 | 572 / 2033 / 2995 | bufio.Scan [via runtime internals] |
| (b) | `runtime.publicationBarrier` | asm | 8 | 572 / 2033 / 2995 | bytes.Grow [via runtime internals] |
| (b) | `runtime.time_now` | linkname | 4 | 572 / 2033 / 2995 | crypto/cipher.XORKeyStream [via runtime internals] |
| (a) | `internal/runtime/sys.GetCallerPC` | intrinsic | 0 | 572 / 2033 / 2995 | bufio.Scan [via runtime internals] |
| (a) | `internal/runtime/sys.GetCallerSP` | intrinsic | 0 | 572 / 2033 / 2995 | runtime/debug.String [medium] |
| (a) | `reflect.methodValueCall` | asm | 0 | 0 / 6 / 2995 | net/rpc.ServeCodec [medium] |
| (a) | `runtime.asyncPreempt` | asm | 0 | 572 / 2033 / 2995 | bufio.Scan [via runtime internals] |
| (a) | `runtime.goexit` | asm | 0 | 572 / 2033 / 2995 | bufio.Scan [via runtime internals] |
| (a) | `runtime.main_main` | linkname | 0 | 0 / 1 / 2995 | runtime/debug.String [medium] |
| (a) | `runtime.mstart` | asm | 0 | 572 / 2033 / 2995 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.tstart_stdcall` | asm | 0 | 572 / 2033 / 2995 | runtime/debug.String [via runtime internals] |

### linux (33 rows)

| Class | Stub | Inv. class | Go calls | Roots S / M / L | Best root |
|:--|:--|:--|--:|:--|:--|
| (c) | `internal/coverage/cfile.getCovCounterList` | linkname | 4 | 3 / 3 / 3 | runtime/coverage.ClearCounters [strict] |
| (c) | `internal/cpu.cpuid` | asm | 9 | 0 / 1 / 3020 | crypto/tls.CipherSuiteName [medium] |
| (c) | `internal/runtime/maps.mapKeyError` | linkname | 3 | 0 / 0 / 3020 | encoding/gob.RegisterName [loose] |
| (c) | `reflect.call` | linkname | 4 | 0 / 6 / 3020 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.getStaticuint64s` | linkname | 1 | 26 / 1010 / 3020 | reflect.Field [strict] |
| (c) | `reflect.growslice` | linkname | 1 | 0 / 0 / 3020 | bytes.Grow [loose] |
| (c) | `reflect.ifaceE2I` | linkname | 2 | 2 / 86 / 3020 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.memmove` | linkname | 4 | 0 / 6 / 3020 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclr` | linkname | 4 | 0 / 6 / 3020 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclrpartial` | linkname | 1 | 0 / 6 / 3020 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemmove` | linkname | 15 | 2 / 86 / 3020 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.unsafe_New` | linkname | 20 | 2 / 86 / 3020 | reflect.ConvertibleTo [strict] |
| (c) | `runtime.abort` | asm | 7 | 0 / 172 / 3020 | net/http.RoundTrip [medium] |
| (c) | `runtime.breakpoint` | asm | 1 | 1 / 1 / 1 | runtime.Breakpoint [strict] |
| (c) | `runtime.connect` | asm | 1 | 0 / 0 / 3020 | log/syslog.Dial [loose] |
| (c) | `runtime.exit` | asm | 51 | 649 / 1872 / 3020 | runtime/debug.String [medium] |
| (c) | `vendor/golang.org/x/sys/cpu.cpuid` | asm | 4 | 0 / 0 / 3020 | archive/zip.NewReader [loose] |
| (c) | `vendor/golang.org/x/sys/cpu.xgetbv` | asm | 1 | 0 / 0 / 3020 | archive/zip.NewReader [loose] |
| (b) | `runtime.asmcgocall` | asm | 59 | 649 / 1872 / 3020 | runtime.SetCgoTraceback [via runtime internals] |
| (b) | `runtime.callCgoMmap` | asm | 1 | 649 / 1872 / 3020 | syscall.Mmap [via runtime internals] |
| (b) | `runtime.callCgoMunmap` | asm | 1 | 649 / 1872 / 3020 | syscall.Munmap [via runtime internals] |
| (b) | `runtime.checkASM` | asm | 1 | 649 / 1872 / 3020 | sync.Broadcast [via runtime internals] |
| (b) | `runtime.gogo` | asm | 7 | 649 / 1872 / 3020 | text/template.Execute [via runtime internals] |
| (b) | `runtime.mcall` | asm | 14 | 649 / 1872 / 3020 | bufio.Scan [via runtime internals] |
| (b) | `runtime.osyield` | asm | 32 | 649 / 1872 / 3020 | go/types.NewMethodSet [via runtime internals] |
| (b) | `runtime.publicationBarrier` | asm | 8 | 649 / 1872 / 3020 | bytes.Grow [via runtime internals] |
| (b) | `runtime.time_now` | linkname | 4 | 649 / 1872 / 3020 | crypto/cipher.XORKeyStream [via runtime internals] |
| (a) | `internal/runtime/sys.GetCallerPC` | intrinsic | 0 | 649 / 1872 / 3020 | bufio.Scan [via runtime internals] |
| (a) | `internal/runtime/sys.GetCallerSP` | intrinsic | 0 | 649 / 1872 / 3020 | runtime/debug.String [medium] |
| (a) | `reflect.methodValueCall` | asm | 0 | 0 / 6 / 3020 | net/rpc.ServeCodec [medium] |
| (a) | `runtime.goexit` | asm | 0 | 649 / 1872 / 3020 | bufio.Scan [via runtime internals] |
| (a) | `runtime.main_main` | linkname | 0 | 0 / 1 / 3020 | runtime/debug.String [medium] |
| (a) | `runtime.mstart` | asm | 0 | 649 / 1872 / 3020 | runtime/debug.String [via runtime internals] |

### darwin (204 rows)

| Class | Stub | Inv. class | Go calls | Roots S / M / L | Best root |
|:--|:--|:--|--:|:--|:--|
| (c) | `crypto/x509/internal/macos.syscall` | linkname | 28 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (c) | `internal/coverage/cfile.getCovCounterList` | linkname | 4 | 3 / 3 / 3 | runtime/coverage.ClearCounters [strict] |
| (c) | `internal/cpu.cpuid` | asm | 9 | 0 / 1 / 3002 | crypto/tls.CipherSuiteName [medium] |
| (c) | `internal/runtime/maps.mapKeyError` | linkname | 3 | 0 / 0 / 3002 | encoding/gob.RegisterName [loose] |
| (c) | `reflect.call` | linkname | 4 | 0 / 6 / 3002 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.getStaticuint64s` | linkname | 1 | 26 / 1010 / 3002 | reflect.Field [strict] |
| (c) | `reflect.growslice` | linkname | 1 | 0 / 0 / 3002 | bytes.Grow [loose] |
| (c) | `reflect.ifaceE2I` | linkname | 2 | 2 / 86 / 3002 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.memmove` | linkname | 4 | 0 / 6 / 3002 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclr` | linkname | 4 | 0 / 6 / 3002 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemclrpartial` | linkname | 1 | 0 / 6 / 3002 | net/rpc.ServeCodec [medium] |
| (c) | `reflect.typedmemmove` | linkname | 15 | 2 / 86 / 3002 | reflect.ConvertibleTo [strict] |
| (c) | `reflect.unsafe_New` | linkname | 20 | 2 / 86 / 3002 | reflect.ConvertibleTo [strict] |
| (c) | `runtime.abort` | asm | 7 | 0 / 172 / 3002 | net/http.RoundTrip [medium] |
| (c) | `runtime.breakpoint` | asm | 1 | 1 / 1 / 1 | runtime.Breakpoint [strict] |
| (c) | `runtime/pprof.mach_vm_region` | linkname | 1 | 4 / 6 / 3002 | runtime/pprof.StartCPUProfile [strict] |
| (c) | `runtime/pprof.proc_regionfilename` | linkname | 1 | 4 / 6 / 3002 | runtime/pprof.StartCPUProfile [strict] |
| (c) | `vendor/golang.org/x/sys/cpu.cpuid` | asm | 4 | 0 / 0 / 3002 | archive/zip.NewReader [loose] |
| (c) | `vendor/golang.org/x/sys/cpu.xgetbv` | asm | 1 | 0 / 0 / 3002 | archive/zip.NewReader [loose] |
| (b) | `runtime.asmcgocall` | asm | 59 | 637 / 1870 / 3002 | runtime.SetCgoTraceback [via runtime internals] |
| (b) | `runtime.asmcgocall_no_g` | asm | 4 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (b) | `runtime.checkASM` | asm | 1 | 637 / 1870 / 3002 | sync.Broadcast [via runtime internals] |
| (b) | `runtime.gogo` | asm | 7 | 637 / 1870 / 3002 | text/template.Execute [via runtime internals] |
| (b) | `runtime.mcall` | asm | 14 | 637 / 1870 / 3002 | bufio.Scan [via runtime internals] |
| (b) | `runtime.publicationBarrier` | asm | 8 | 637 / 1870 / 3002 | bytes.Grow [via runtime internals] |
| (a) | `crypto/x509/internal/macos.x509_CFArrayAppendValue_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFArrayCreateMutable_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFArrayGetCount_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFArrayGetValueAtIndex_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFDataCreate_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFDataGetBytePtr_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFDataGetLength_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFDateCreate_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFErrorCopyDescription_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFErrorGetCode_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFRelease_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFStringCreateExternalRepresentation_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_CFStringCreateWithBytes_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecCertificateCopyData_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecCertificateCreateWithData_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecPolicyCreateSSL_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecTrustCreateWithCertificates_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecTrustEvaluateWithError_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecTrustGetCertificateAtIndex_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecTrustGetCertificateCount_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `crypto/x509/internal/macos.x509_SecTrustSetVerifyDate_trampoline` | cgo | 0 | 1 / 186 / 3002 | crypto/x509.Verify [strict] |
| (a) | `internal/runtime/sys.GetCallerPC` | intrinsic | 0 | 637 / 1870 / 3002 | bufio.Scan [via runtime internals] |
| (a) | `internal/runtime/sys.GetCallerSP` | intrinsic | 0 | 637 / 1870 / 3002 | runtime/debug.String [medium] |
| (a) | `internal/syscall/unix.libc_arc4random_buf_trampoline` | cgo | 0 | 175 / 248 / 3002 | crypto/rand.Read [strict] |
| (a) | `internal/syscall/unix.libc_faccessat_trampoline` | cgo | 0 | 7 / 10 / 3002 | os/exec.LookPath [strict] |
| (a) | `internal/syscall/unix.libc_gai_strerror_trampoline` | cgo | 0 | 184 / 257 / 3002 | net.Error [strict] |
| (a) | `internal/syscall/unix.libc_getgrouplist_trampoline` | cgo | 0 | 1 / 1 / 1 | os/user.GroupIds [strict] |
| (a) | `internal/syscall/unix.libc_getnameinfo_trampoline` | cgo | 0 | 1 / 1 / 1 | net.LookupAddr [strict] |
| (a) | `internal/syscall/unix.libc_grantpt_trampoline` | cgo | 0 | 0 / 0 / 3002 | archive/tar.AddFS [loose] |
| (a) | `internal/syscall/unix.libc_mkdirat_trampoline` | cgo | 0 | 5 / 6 / 3002 | os.Mkdir [strict] |
| (a) | `internal/syscall/unix.libc_posix_openpt_trampoline` | cgo | 0 | 0 / 0 / 3002 | archive/tar.AddFS [loose] |
| (a) | `internal/syscall/unix.libc_ptsname_r_trampoline` | cgo | 0 | 0 / 0 / 3002 | archive/tar.AddFS [loose] |
| (a) | `internal/syscall/unix.libc_readlinkat_trampoline` | cgo | 0 | 266 / 319 / 3002 | os.Lstat [strict] |
| (a) | `internal/syscall/unix.libc_sysconf_trampoline` | cgo | 0 | 5 / 5 / 3002 | os/user.Lookup [strict] |
| (a) | `internal/syscall/unix.libc_unlockpt_trampoline` | cgo | 0 | 0 / 0 / 3002 | archive/tar.AddFS [loose] |
| (a) | `internal/syscall/unix.libresolv_res_9_nclose_trampoline` | cgo | 0 | 1 / 1 / 1 | net.LookupCNAME [strict] |
| (a) | `internal/syscall/unix.libresolv_res_9_ninit_trampoline` | cgo | 0 | 1 / 1 / 1 | net.LookupCNAME [strict] |
| (a) | `internal/syscall/unix.libresolv_res_9_nsearch_trampoline` | cgo | 0 | 1 / 1 / 1 | net.LookupCNAME [strict] |
| (a) | `reflect.methodValueCall` | asm | 0 | 0 / 6 / 3002 | net/rpc.ServeCodec [medium] |
| (a) | `runtime.asyncPreempt` | asm | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.cgoSigtramp` | asm | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.exit_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.goexit` | asm | 0 | 637 / 1870 / 3002 | bufio.Scan [via runtime internals] |
| (a) | `runtime.kevent_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.madvise_trampoline` | cgo | 0 | 637 / 1870 / 3002 | bytes.Grow [via runtime internals] |
| (a) | `runtime.main_main` | linkname | 0 | 0 / 1 / 3002 | runtime/debug.String [medium] |
| (a) | `runtime.mlock_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.mmap_trampoline` | cgo | 0 | 637 / 1870 / 3002 | syscall.Mmap [via runtime internals] |
| (a) | `runtime.mstart` | asm | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.mstart_stub` | asm | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.munmap_trampoline` | cgo | 0 | 637 / 1870 / 3002 | syscall.Munmap [via runtime internals] |
| (a) | `runtime.open_trampoline` | cgo | 0 | 0 / 0 / 3002 | archive/tar.AddFS [via runtime internals] |
| (a) | `runtime.pthread_attr_getstacksize_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.pthread_attr_init_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.pthread_attr_setdetachstate_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.pthread_cond_init_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.pthread_create_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.pthread_kill_trampoline` | cgo | 0 | 637 / 1870 / 3002 | bufio.Scan [via runtime internals] |
| (a) | `runtime.pthread_mutex_init_trampoline` | cgo | 0 | 637 / 1870 / 3002 | runtime/debug.String [via runtime internals] |
| (a) | `runtime.raise_trampoline` | cgo | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.raiseproc_trampoline` | cgo | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.sigpanic0` | asm | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.sigtramp` | asm | 0 | 637 / 1870 / 3002 | os.NewFile [via runtime internals] |
| (a) | `runtime.usleep_trampoline` | cgo | 0 | 637 / 1870 / 3002 | go/types.NewMethodSet [via runtime internals] |
| (a) | `runtime.walltime_trampoline` | cgo | 0 | 637 / 1870 / 3002 | crypto/cipher.XORKeyStream [via runtime internals] |
| (a) | `syscall.libc_accept_trampoline` | cgo | 0 | 0 / 228 / 3002 | net.Accept [medium] |
| (a) | `syscall.libc_access_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Access [strict] |
| (a) | `syscall.libc_adjtime_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Adjtime [strict] |
| (a) | `syscall.libc_chdir_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Chdir [strict] |
| (a) | `syscall.libc_chflags_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Chflags [strict] |
| (a) | `syscall.libc_chmod_trampoline` | cgo | 0 | 253 / 305 / 3002 | syscall.Chmod [strict] |
| (a) | `syscall.libc_chown_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Chown [strict] |
| (a) | `syscall.libc_chroot_trampoline` | cgo | 0 | 9 / 19 / 3002 | syscall.Chroot [strict] |
| (a) | `syscall.libc_close_trampoline` | cgo | 0 | 347 / 1199 / 3002 | syscall.Close [strict] |
| (a) | `syscall.libc_closedir_trampoline` | cgo | 0 | 21 / 205 / 3002 | syscall.Getdirentries [strict] |
| (a) | `syscall.libc_connect_trampoline` | cgo | 0 | 0 / 228 / 3002 | net/http.GetClientConn [medium] |
| (a) | `syscall.libc_dup2_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Dup2 [strict] |
| (a) | `syscall.libc_dup_trampoline` | cgo | 0 | 6 / 209 / 3002 | syscall.Dup [strict] |
| (a) | `syscall.libc_exchangedata_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Exchangedata [strict] |
| (a) | `syscall.libc_execve_trampoline` | cgo | 0 | 1 / 1 / 3 | syscall.Exec [strict] |
| (a) | `syscall.libc_fchdir_trampoline` | cgo | 0 | 1 / 2 / 2 | syscall.Fchdir [strict] |
| (a) | `syscall.libc_fchflags_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Fchflags [strict] |
| (a) | `syscall.libc_fchmod_trampoline` | cgo | 0 | 1 / 305 / 3002 | syscall.Fchmod [strict] |
| (a) | `syscall.libc_fchown_trampoline` | cgo | 0 | 1 / 2 / 2 | syscall.Fchown [strict] |
| (a) | `syscall.libc_fcntl_trampoline` | cgo | 0 | 264 / 321 / 3002 | syscall.CloseOnExec [strict] |
| (a) | `syscall.libc_fdopendir_trampoline` | cgo | 0 | 2 / 205 / 3002 | syscall.Getdirentries [strict] |
| (a) | `syscall.libc_flock_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Flock [strict] |
| (a) | `syscall.libc_fpathconf_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Fpathconf [strict] |
| (a) | `syscall.libc_fstat64_trampoline` | cgo | 0 | 258 / 324 / 3002 | syscall.Fstat [strict] |
| (a) | `syscall.libc_fstatat64_trampoline` | cgo | 0 | 264 / 317 / 3002 | os.Lstat [strict] |
| (a) | `syscall.libc_fstatfs64_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Fstatfs [strict] |
| (a) | `syscall.libc_fsync_trampoline` | cgo | 0 | 1 / 4 / 3002 | syscall.Fsync [strict] |
| (a) | `syscall.libc_ftruncate_trampoline` | cgo | 0 | 1 / 174 / 3002 | syscall.Ftruncate [strict] |
| (a) | `syscall.libc_futimes_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Futimes [strict] |
| (a) | `syscall.libc_getcwd_trampoline` | cgo | 0 | 14 / 20 / 3002 | syscall.Getwd [strict] |
| (a) | `syscall.libc_getdtablesize_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getdtablesize [strict] |
| (a) | `syscall.libc_getegid_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getegid [strict] |
| (a) | `syscall.libc_geteuid_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Geteuid [strict] |
| (a) | `syscall.libc_getfsstat_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getfsstat [strict] |
| (a) | `syscall.libc_getgid_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getgid [strict] |
| (a) | `syscall.libc_getgroups_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getgroups [strict] |
| (a) | `syscall.libc_getpgid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getpgid [strict] |
| (a) | `syscall.libc_getpgrp_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getpgrp [strict] |
| (a) | `syscall.libc_getpid_trampoline` | cgo | 0 | 17 / 19 / 3002 | syscall.Getpid [strict] |
| (a) | `syscall.libc_getppid_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getppid [strict] |
| (a) | `syscall.libc_getpriority_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getpriority [strict] |
| (a) | `syscall.libc_getrlimit_trampoline` | cgo | 0 | 184 / 310 / 3002 | syscall.Getrlimit [strict] |
| (a) | `syscall.libc_getrusage_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getrusage [strict] |
| (a) | `syscall.libc_getsid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Getsid [strict] |
| (a) | `syscall.libc_getsockopt_trampoline` | cgo | 0 | 11 / 11 / 3002 | syscall.GetsockoptByte [strict] |
| (a) | `syscall.libc_gettimeofday_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Gettimeofday [strict] |
| (a) | `syscall.libc_getuid_trampoline` | cgo | 0 | 5 / 5 / 3002 | syscall.Getuid [strict] |
| (a) | `syscall.libc_ioctl_trampoline` | cgo | 0 | 24 / 34 / 3002 | syscall.BpfBuflen [strict] |
| (a) | `syscall.libc_issetugid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Issetugid [strict] |
| (a) | `syscall.libc_kevent_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Kevent [strict] |
| (a) | `syscall.libc_kill_trampoline` | cgo | 0 | 15 / 1065 / 3002 | syscall.Kill [strict] |
| (a) | `syscall.libc_kqueue_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Kqueue [strict] |
| (a) | `syscall.libc_lchown_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Lchown [strict] |
| (a) | `syscall.libc_link_trampoline` | cgo | 0 | 2 / 19 / 3002 | syscall.Link [strict] |
| (a) | `syscall.libc_listen_trampoline` | cgo | 0 | 1 / 6 / 6 | syscall.Listen [strict] |
| (a) | `syscall.libc_lseek_trampoline` | cgo | 0 | 208 / 1124 / 3002 | syscall.Seek [strict] |
| (a) | `syscall.libc_lstat64_trampoline` | cgo | 0 | 28 / 206 / 3002 | syscall.Lstat [strict] |
| (a) | `syscall.libc_mkdir_trampoline` | cgo | 0 | 6 / 7 / 3002 | syscall.Mkdir [strict] |
| (a) | `syscall.libc_mkfifo_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Mkfifo [strict] |
| (a) | `syscall.libc_mknod_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Mknod [strict] |
| (a) | `syscall.libc_mlock_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Mlock [strict] |
| (a) | `syscall.libc_mlockall_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Mlockall [strict] |
| (a) | `syscall.libc_mmap_trampoline` | cgo | 0 | 15 / 21 / 3002 | syscall.Mmap [strict] |
| (a) | `syscall.libc_mprotect_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Mprotect [strict] |
| (a) | `syscall.libc_munlock_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Munlock [strict] |
| (a) | `syscall.libc_munlockall_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Munlockall [strict] |
| (a) | `syscall.libc_munmap_trampoline` | cgo | 0 | 15 / 21 / 3002 | syscall.Munmap [strict] |
| (a) | `syscall.libc_open_trampoline` | cgo | 0 | 320 / 1148 / 3002 | syscall.Open [strict] |
| (a) | `syscall.libc_openat_trampoline` | cgo | 0 | 266 / 319 / 3002 | syscall.Getdirentries [strict] |
| (a) | `syscall.libc_pathconf_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Pathconf [strict] |
| (a) | `syscall.libc_pread_trampoline` | cgo | 0 | 1 / 34 / 3002 | syscall.Pread [strict] |
| (a) | `syscall.libc_ptrace_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.PtraceAttach [strict] |
| (a) | `syscall.libc_pwrite_trampoline` | cgo | 0 | 1 / 2 / 3002 | syscall.Pwrite [strict] |
| (a) | `syscall.libc_read_trampoline` | cgo | 0 | 207 / 1192 / 3002 | syscall.Read [strict] |
| (a) | `syscall.libc_readdir_r_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Getdirentries [strict] |
| (a) | `syscall.libc_readlink_trampoline` | cgo | 0 | 7 / 7 / 3002 | syscall.Readlink [strict] |
| (a) | `syscall.libc_rename_trampoline` | cgo | 0 | 4 / 4 / 4 | syscall.Rename [strict] |
| (a) | `syscall.libc_revoke_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Revoke [strict] |
| (a) | `syscall.libc_rmdir_trampoline` | cgo | 0 | 10 / 183 / 3002 | syscall.Rmdir [strict] |
| (a) | `syscall.libc_select_trampoline` | cgo | 0 | 1 / 1 / 3002 | syscall.Select [strict] |
| (a) | `syscall.libc_sendfile_trampoline` | cgo | 0 | 6 / 191 / 3002 | syscall.Sendfile [strict] |
| (a) | `syscall.libc_setegid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setegid [strict] |
| (a) | `syscall.libc_seteuid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Seteuid [strict] |
| (a) | `syscall.libc_setgid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setgid [strict] |
| (a) | `syscall.libc_setgroups_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setgroups [strict] |
| (a) | `syscall.libc_setlogin_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setlogin [strict] |
| (a) | `syscall.libc_setpgid_trampoline` | cgo | 0 | 9 / 19 / 3002 | syscall.Setpgid [strict] |
| (a) | `syscall.libc_setpriority_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setpriority [strict] |
| (a) | `syscall.libc_setprivexec_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setprivexec [strict] |
| (a) | `syscall.libc_setregid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setregid [strict] |
| (a) | `syscall.libc_setreuid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setreuid [strict] |
| (a) | `syscall.libc_setrlimit_trampoline` | cgo | 0 | 2 / 311 / 3002 | syscall.Setrlimit [strict] |
| (a) | `syscall.libc_setsid_trampoline` | cgo | 0 | 9 / 19 / 3002 | syscall.Setsid [strict] |
| (a) | `syscall.libc_setsockopt_trampoline` | cgo | 0 | 192 / 244 / 3002 | syscall.SetsockoptByte [strict] |
| (a) | `syscall.libc_settimeofday_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Settimeofday [strict] |
| (a) | `syscall.libc_setuid_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Setuid [strict] |
| (a) | `syscall.libc_shutdown_trampoline` | cgo | 0 | 1 / 20 / 3002 | syscall.Shutdown [strict] |
| (a) | `syscall.libc_socket_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Socket [strict] |
| (a) | `syscall.libc_socketpair_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Socketpair [strict] |
| (a) | `syscall.libc_stat64_trampoline` | cgo | 0 | 265 / 318 / 3002 | syscall.Stat [strict] |
| (a) | `syscall.libc_statfs64_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Statfs [strict] |
| (a) | `syscall.libc_symlink_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.Symlink [strict] |
| (a) | `syscall.libc_sync_trampoline` | cgo | 0 | 1 / 1 / 3002 | syscall.Sync [strict] |
| (a) | `syscall.libc_sysctl_trampoline` | cgo | 0 | 187 / 313 / 3002 | syscall.RouteRIB [strict] |
| (a) | `syscall.libc_truncate_trampoline` | cgo | 0 | 2 / 174 / 3002 | syscall.Truncate [strict] |
| (a) | `syscall.libc_umask_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Umask [strict] |
| (a) | `syscall.libc_undelete_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Undelete [strict] |
| (a) | `syscall.libc_unlink_trampoline` | cgo | 0 | 199 / 298 / 3002 | syscall.Unlink [strict] |
| (a) | `syscall.libc_unlinkat_trampoline` | cgo | 0 | 264 / 317 / 3002 | os.Lstat [strict] |
| (a) | `syscall.libc_unmount_trampoline` | cgo | 0 | 1 / 1 / 1 | syscall.Unmount [strict] |
| (a) | `syscall.libc_utimensat_trampoline` | cgo | 0 | 2 / 2 / 2 | syscall.UtimesNano [strict] |
| (a) | `syscall.libc_utimes_trampoline` | cgo | 0 | 3 / 3 / 3 | syscall.Utimes [strict] |
| (a) | `syscall.libc_wait4_trampoline` | cgo | 0 | 10 / 288 / 3002 | syscall.Wait4 [strict] |
| (a) | `syscall.libc_write_trampoline` | cgo | 0 | 1 / 715 / 3002 | syscall.Write [strict] |
