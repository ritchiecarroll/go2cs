# DESIGN: C function pointers as numbers (sqlite-cs S1), and assembly bodies in third-party modules (S21)

> **Status: DESIGN, for a ruling; no code.** COORD's order of 2026-11-13 (inbox C1 20261113T064136Z).
>
> - **Read at:** master `3da19d0e2e`.
> - **Modules:** modernc.org/sqlite v1.62.0 and modernc.org/libc v1.80.1 (sqlite-cs's pins); Go 1.26.10.
> - **The C# side:** .NET SDK 10.0.403 and ILCompiler 10.0.14, linux-x64, Release.
> - **Evidence:** every census and probe is in `docs/phase4/data/sqlite-cs-funcptr/`. Each probe builds with
>   `probe.csproj`. `$GOMODCACHE` and `$GOROOT` replace local paths.
> - **Not read:** sqlite-cs's own tree. This session cannot reach that repository, so the forms below are read from
>   the Go source go2cs converts.
> - **G's read of S1 is asked before COORD rules** (section 1.7).

## Summary (for review)

**S1: ccgo carries every C function pointer as a `uintptr`.**
- **Mint:** `__ccgo_fp(f)` reads the data word of the interface its func value is boxed in.
- **Recovery:** the func value is rebuilt by reinterpreting `&struct{ f uintptr }{n}` as a pointer to a func type.

go2cs has no number for a delegate. **The design is a golib registry, `GoFuncPtr`, with these properties:**
- one stable number per function, in a range no data pointer can occupy, so a number used as data faults;
- recovery returns the very delegate minted, which keeps G's func-value twins (N3) open;
- 0 is nil, and an unknown number or a mismatched signature fails loudly by name;
- no reflection, so Native AOT is clean (measured).

The converter recognises three shapes, and nothing else changes.

**The census** (lib, the sqlite package and libc):

| form | sites |
|:--|--:|
| mints (`__ccgo_fp`) | 1,360 |
| mints on the Go side (`cFuncPointer`) | 46 |
| recoveries | 509 |
| comparisons against a mint | 16 |

- The mints cover 989 distinct functions, and every one is a package-level function. There are no closures, method
  values or func variables, so no number ever goes stale.
- Every recovery is ccgo's form, of an unnamed func type, called at once.

**S21: what remains is narrow.** go2cs already converts modules under `safe,purego,noasm`. Under those tags, every
assembly function in x/crypto and klauspost/compress is gone. What remains, and runs, in the sqlite closure is:
- x/sys/cpu's CPU-feature reads (on windows and darwin, from its package init);
- x/sys/unix's syscall entry points, which in Go are jumps into std's `syscall`.

The design is a converter-side table of known assembly functions, checked by signature, whose bodies forward to golib
or to std. An unlisted assembly function keeps today's throwing stub, and the converter names it.

## 1. S1: C function pointers carried as `uintptr`

### 1.1 The two shapes ccgo writes

```go
// mint (libc ccgo_linux_amd64.go:150485; every ccgo package declares its own copy)
func __ccgo_fp(f interface{}) uintptr {
	type iface [2]uintptr
	return (*iface)(unsafe.Pointer(&f))[1] // the interface's data word: a pointer to the func's funcval
}

// recovery, at each call through a C function pointer
(*(*func(*libc.TLS, uintptr) int32)(unsafe.Pointer(&struct{ f uintptr }{p.FxClose})))(tls, p)
```

The sqlite package mints on the Go side with `cFuncPointer[T](f T) uintptr`
(`*(*uintptr)(unsafe.Pointer(&struct{ f T }{f}))`, sqlite.go:1212).

### 1.2 Census

`s1/fpcensus-main.go` on linux/amd64, over the packages modernc.org/sqlite/lib, modernc.org/sqlite and
modernc.org/libc. Output in `s1/fpcensus-output.txt`.

| form | lib | sqlite | libc |
|:--|--:|--:|--:|
| `__ccgo_fp(f)` mint sites | 1,307 | — | 53 |
| … of a package-level func of the same package | 1,265 | — | 53 |
| … of another package's func (libc's, minted in lib) | 42 | — | 0 |
| … of a closure, method value, func variable or literal | 0 | — | 0 |
| … inside a composite literal (a callback table) | 16 | — | 5 |
| `cFuncPointer(f)`, the Go-side mint | — | 46 | — |
| a mint compared with `==` or `!=` (`busy.go:36`: `h == cFuncPointer(busyHandler)`) | 15 | 1 | — |
| recoveries, ccgo's `&struct{ f uintptr }{x}` form | 454 | 0 | 55 |
| … of another recovery shape | 0 | 0 | 0 |
| … to a NAMED func type, or a Go-variadic one | 0 | 0 | 0 |
| … called at once (`(*(*F)(…))(args)`) | 454 | 0 | 55 |

- **The functions:** 989 distinct functions minted, with 122 distinct signatures.
- **The recoveries:** 85 distinct signatures. C variadics arrive as a trailing `va uintptr`.
- **Six recovery signatures match no mint in the corpus.** They are callbacks the sqlite package mints through
  `cFuncPointer` (its trampolines), or a cast in C.
- **One thing a number must never be:** C's sentinels compare numbers without calling them. `SQLITE_TRANSIENT` is
  `^uintptr(0)` (`lib/defs.go:9`), so a minted number must never be 0 or all ones.

### 1.3 The registry (golib)

```csharp
public static class GoFuncPtr
{
    public static nuint Of(Delegate? f);                 // mint: 0 for nil, else the function's one number
    public static F? Get<F>(nuint n) where F : Delegate; // recovery: null for 0, else the minted delegate
}
```

The prototype is `s1/registry-probe-Program.cs`. It reads 0 failed under the JIT and under Native AOT, and the AOT
publish has 0 IL or AOT warnings.

- **MINT, one number per function.**
  - Two tables: a reference-identity table in front of a `Delegate.Equals` table (same delegate type, method and
    target).
  - C# 11 caches the delegate for a conversion from a static method group, so each mint site passes the same
    instance every time. The front table answers it in O(1), and the second table makes two sites, or two packages,
    agree on one function (measured, `s1/bucket-two-level-output.txt`).
- **Why two tables.** `Delegate.GetHashCode` is the delegate TYPE's hash for a static method: two functions of one
  signature hash alike (`s1/delegate-hash-output.txt`). So a one-table registry scans a bucket of every function of
  that signature. Measured with 300 such functions (`s1/bucket-uncached-output.txt`):
  - one table: 75 ns to 13 µs per mint, depending on where the function sits in its bucket;
  - two tables: 48–51 ns once warm, for the worst-placed function.
- **The numbers.** `0xFFFF_F000_0000_0000 + 16·i` on 64-bit:
  - never 0, never all ones, and 16-aligned;
  - in the kernel half, where a user-mode data access faults: a non-canonical-for-user address on x64; on arm64,
    bit 55 set selects TTBR1 even under top-byte-ignore.

  So a number dereferenced as data crashes loudly rather than reading memory. A 32-bit target would take
  `0xF000_0000 + 16·i`. NOT MEASURED: whether go2cs ships a 32-bit target that needs it.
- **RECOVERY.** `Get<F>` indexes an array by `(n − base) / 16`, and returns the MINTED INSTANCE, never a re-created
  delegate. It costs 12 ns AOT and 34 ns JIT, including the call.
- **Its failures, each loud and named** (all measured):
  - `0` → `null`, Go's nil func.
  - An unknown number, or a data pointer: `"0x… is not a function pointer go2cs minted"`.
  - A minted function called under another signature (a C cast): `"function pointer 0x… holds <type> and is called as
    <type>"`.
- **SIGNATURE.** `F` is the C# delegate the converter emits for the recovery's Go func type. A mint is typed by its
  function's OWN Go signature, so a mint and a recovery of one Go signature meet as one C# delegate type.
  - The control in the probe: the same function minted under another delegate type gets another number. That is
    why the type must come from the Go signature, never from the call site.
  - There is no reflection and no emit: 85 `Get<F>` instantiations at compile time.
- **LIFETIME.** Every mint in the census is a package-level function: a static method with no target, so a number lives
  for the process and the table is bounded by the program's functions (989 ccgo functions plus at most 46 Go-side ones for sqlite).
  - A closure minted through the Go-side shape would be held for the process too: a leak, never a stale number.
    No census site does it.
- **EQUALITY.**
  - Numbers compare as numbers, and two mints of one function are equal: C's `h == cFuncPointer(busyHandler)` holds
    (measured).
  - A func value never minted has no number until its first mint, which assigns one, so a comparison stays
    consistent.
  - A number never equals a data pointer, because the ranges are disjoint.

### 1.4 What the converter recognizes, and what golib does at run time

The converter recognizes three shapes, by STRUCTURE and never by name:

| shape | converted to |
|:--|:--|
| C1 (mint, ccgo): a function whose body returns `(*[2]uintptr)(unsafe.Pointer(&f))[1]` with `f` an interface parameter | the body `return GoFuncPtr.Of((Delegate?)f);`. Call sites keep `__ccgo_fp(F)`, with `F` converted to the delegate of its own Go signature as today. |
| C2 (mint, Go side): `*(*uintptr)(unsafe.Pointer(&struct{ f T }{x}))` with the one field a func type or a type parameter | `GoFuncPtr.Of(x)`. A type parameter is checked to be a delegate at run time. |
| C3 (recovery): `*(*F)(unsafe.Pointer(&struct{ f uintptr }{n}))` with `F` a func type | `GoFuncPtr.Get<F>(n)` |

- **Every other func value through `unsafe.Pointer`** is a convert-time warning that names the site. It keeps today's
  path, which refuses at run time.
- **The C3 form must be dereferenced at once.** It is in 509 of 509 sites. A C3 form NOT dereferenced at once (0
  sites) is a convert-time error.
- **golib's run-time part** is the table, the number range and the four refusals above.
- **golib needs no knowledge of ccgo.**

### 1.5 The M:N collision (G's N3, func values as async twins)

N3 (`DATA-mn-spike-2.md`, its section N3) asks for "a way from a func value to its twin that does not change the
visible type of a Go func value". It is not designed yet. The registry keeps every N3 option open:
- **No change to the func value's type.** A recovered value is the original delegate instance, so any twin lookup
  keyed on the instance, or on its method, finds the same twin through a number as without one (probe:
  `ReferenceEquals` holds).
- **A row has room for a twin.** If N3 keeps a table from function to twin, the registry's function table can be that
  table: one row per function, holding its number, its plain delegate and, later, its twin. A twinned call site would
  read the twin from the same row (`GetTwin<FAsync>(n)`).
- **Thread safety:** a concurrent dictionary and an append under a lock. Recovery reads an array, safe on any carrier.

The ask to G: does N3 intend a per-FUNCTION identity (one row per function, the same two-level key), or a
per-INSTANCE one? With the first, the two tables should be one table; with the second, they stay separate and still
compose.

### 1.6 Predictions and stop lines

**Predictions:**
- **P1.1** With C1–C3 and the registry, sqlite-cs's start-up passes the S1 wall (503 recovery sites by its count; 509
  in this census).
- **P1.2** sqlite's own suite reaches no "is called as" refusal. If it does, each one is a real C cast and is named.
- **P1.3** A converted run mints at most 1,035 numbers (989 + 46).
- **P1.4** Native AOT publishes with 0 new warnings.

**Stop lines:**
- **X1.1** A recovery returns a delegate that is not the minted instance.
- **X1.2** A number is 0, all ones, or in a user-mode range.
- **X1.3** A cast refusal is silenced instead of named.
- **X1.4** Reflection or emit enters the registry.

### 1.7 For G

This section and 1.5 are posted to G for a read before COORD rules, as asked.

## 2. S21: managed bodies for assembly functions in third-party modules

### 2.1 Census

`s21/asmcensus-main.go`: every bodyless Go function in the NON-std packages of a closure, classified as assembly (the
package has `.s` files) or `//go:linkname`.

| sqlite closure | assembly | which | called from Go? |
|:--|--:|:--|:--|
| linux/amd64 | 1,886 | x/sys/unix 7; libc 1,879 = 1,875 `Y…` trampolines (`abi0_linux_amd64.s`, `CALL ·X…`, generated by qbecc) and 4 `TLS…` | x/sys/unix: yes (libc calls `unix.Syscall` 261 times, `Syscall6` 60 and `RawSyscall6` 2, counted over all its OS files). The `Y…` and `TLS…` are ABI0 entry points for compiled C, with no Go caller in the closure. |
| linux/arm64 | 6 | x/sys/unix 6 | yes |
| windows/amd64 | 2 | x/sys/cpu `cpuid`, `xgetbv` | yes: x/sys/cpu's init runs them in every program importing libc. libc's `pthread.go` imports x/sys/cpu on every target except linux, which is why the wall shows on windows and darwin. |
| darwin/amd64 | 7 | x/sys/cpu `cpuid`, `xgetbv`; x/sys/unix 5 | yes |
| darwin/arm64 | 9 | x/sys/cpu `getisar0`, `getisar1`, `getpfr0`, `getzfr0`; x/sys/unix 5 | x/sys/unix: yes. The four ID-register reads: NO, because x/sys/cpu's `archInit` reads them only on freebsd (`cpu_arm64.go:46`). darwin's `doinit` asks sysctl through the linkname pull `syscall_syscall6` instead. |

- **The `purego,noasm` tags change nothing in the sqlite closure** (`s21/sqlite-linux-amd64-purego-noasm.txt`).
- **Linkname pulls:** 10–17 per target. bigfft pulls 7 of `math/big`'s internal arithmetic, and x/sys pulls
  `runtime.getAuxv` and, on windows, `syscall.loadlibrary` and `syscall.getprocaddress`.
  - These are not assembly. They are the `linknameForwardTargets` class (`visitFuncDecl.go:2423`).
  - bigfft's targets are internal to the converted `math/big` assembly, so a forward needs them visible. NOT
    MEASURED: whether a sqlite path reaches bigfft.

**The Target Atlas modules that ship assembly.** Only those in the local module cache were read: x/crypto v0.53.0
and klauspost/compress v1.20.0, through a probe module (`s21/atlas-*`).
- **Without tags:** 63 assembly functions on amd64 and 50 on arm64 (s2, zstd, huff0, xxhash, chacha20poly1305,
  poly1305, blake2b, argon2, cpuinfo, and x/sys/cpu).
- **Under `purego,noasm`:** only x/sys/cpu's remain, 2 on amd64 and 4 on arm64.
- **go2cs already applies `safe,purego,noasm`** to every `-recurse` module conversion (`defaultModuleBuildTags`,
  `commandLineOptions.go:309`), so these modules need nothing more.

### 2.2 The options

| option | what it is | scales? |
|:--|:--|:--|
| (a) a golib overlay | golib ships managed bodies for well-known third-party assembly functions | the bodies are CPU reads that already live in golib's own territory; versions do not matter |
| (b) a converter-side table | the converter maps (import path, function, signature) to a forwarding body, as `linknameForwardTargets` does for std | yes: a small table, checked by signature at convert time, version-independent while signatures hold (x/sys's have held for years) |
| (c) a nugetgo companion package | a converted x/sys published with hand-owned impl files | poorly: every x/sys version needs its own, and a consumer pinning another version gets none |
| (d) a hand-owned file per consumer module | each converted program writes C# for x/sys | no |

### 2.3 Recommendation: (b) with (a)'s bodies

**The table, `asmForwardBodies`:**

| Go function | body |
|:--|:--|
| x/sys/cpu `cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)` | `go.golib.Cpu.Cpuid(eaxArg, ecxArg)`: `X86Base.CpuId`, zeros where X86Base is unsupported. The same logic as std's `internal/cpu` impl (`internal/cpu/amd64/cpu_x86_impl.cs`), which should move to golib so there is one body. |
| x/sys/cpu `xgetbv() (eax, edx uint32)` | `go.golib.Cpu.Xgetbv()`. .NET has no XGETBV intrinsic, so XCR0 is SYNTHESISED from `Avx.IsSupported` (bits 1 and 2) and `Avx512F.IsSupported` (bits 5–7). Those already include the OS-support check xgetbv exists to make. |
| x/sys/cpu `getisar0`, `getisar1`, `getpfr0`, `getzfr0` | `go.golib.Cpu.Arm64IdRegister(…)`. The fields x/sys/cpu reads are synthesised from `AdvSimd`, `Aes`, `Sha1`, `Sha256`, `Crc32`, `Atomics`, `Dp` and `Rdm` `.IsSupported`. A managed process cannot execute MRS. These are reached only on freebsd, so they are lowest priority. |
| x/sys/cpu's linkname `syscall_syscall6` (darwin's sysctl path) | a `linknameForwardTargets` entry to std's `syscall.syscall6`. It is internal to the converted `syscall` assembly, so the forward needs it visible, as bigfft's pulls do. NOT MEASURED. |
| x/sys/unix `Syscall`, `Syscall6`, `RawSyscall`, `RawSyscall6` | the same-named std `syscall` function: the assembly is `JMP syscall·Syscall(SB)` (`unix/asm_linux_amd64.s:16`) |
| x/sys/unix `SyscallNoError`, `RawSyscallNoError` | `syscall.RawSyscall`, keeping `r1, r2` |
| x/sys/unix `gettimeofday` | std's `syscall.Gettimeofday`, adapted to the `(tv *Timeval) Errno` shape |

**How the table applies:**
- **The signature is checked.** A listed function whose declared signature differs is a convert-time error. It never
  forwards silently.
- **An unlisted assembly function** keeps today's PartialStubGenerator stub, which throws, and the converter prints a
  WARNING naming it. A program that never calls it is unaffected, as libc's 1,879 are today.
- **The synthesised bodies are named as such** in their doc comments: a register image built from .NET's view of
  the CPU, not read from the hardware.

**Predictions:**
- **P2.1** A sqlite-cs program on windows/amd64 and darwin/amd64 passes x/sys/cpu's init. darwin/arm64 also needs the `syscall_syscall6` linkname forward.
- **P2.2** Its `cpu.X86.HasAVX2` (or ARM64's `HasAES`) equals std's `internal/cpu` answer on the same host.
- **P2.3** libc's syscalls on linux run through `syscall.Syscall`, with no stub reached in sqlite's suite.

**Stop lines:**
- **X2.1** A forwarder whose signature was not checked.
- **X2.2** A synthesised register that disagrees with std's `internal/cpu` on the same host.
- **X2.3** A stub reached and not named in the conversion's warnings.
