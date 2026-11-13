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
>
> **Revised 2026-11-13 after the Fable review (inbox C1 20261113T071712Z).** Section 3 is the revision. Where it
> disagrees with the Summary or sections 1–2, section 3 holds. Sections 1–2 stay as first posted.

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
  - A minted function called under another signature (a C cast):
    `"function pointer 0x… holds <type> and is called as <type>"`.
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

## 3. Revision after the Fable review (dated block, 2026-11-13, C1)

> Read at master `3da19d0e2e`, the same base as sections 1–2. The review is COORD's inbox post C1 20261113T071712Z,
> items 1–11. Every new census and probe is in `docs/phase4/data/sqlite-cs-funcptr/s1/rev/`.

### 3.0 The review's items, and where each is answered

| item | finding | answer |
|:--|:--|:--|
| 1 | golib already has `GoFuncCookie`, a recovery that rebinds, and `GoSyntheticPC` | 3.1: one func-word story. GoFuncCookie is extended and `GoFuncPtr` is withdrawn. |
| 2 | the band sits inside GoSyntheticPC's; the 32-bit fallback was already refused; data use panics rather than faults | 3.1: GoFuncCookie's band, unchanged; 32-bit throws |
| 3 | linux/amd64 only, and shapes missed | 3.2: all five shipped targets, six classes |
| 4 | C# delegate identity is not Go type identity | 3.1 (key on the method, rebind on recovery) and 3.3 (the one by-value struct site) |
| 5 | the prototype raced | 3.4: a locked prototype, race-tested, timed under JIT and AOT |
| 6 | 33 recoveries came from mints outside `__ccgo_fp`/`cFuncPointer` | 3.2: classes m2 and w2 |
| 7 | each park holds a carrier | 3.5 |
| 8 | `asmTrampolines.go` already forwards the x/sys/unix trampolines | 3.7: those rows are dropped |
| 9 | the `*_trampoline_addr` DATA symbols were not counted | 3.7 |
| 10 | X2.2 trips by construction | 3.7 |
| 11 | a name table only for the cpu rows; a signature change stays an error | 3.7 |

### 3.1 S1: one func-word story

**What exists at the base.** The Summary's "go2cs has no number for a delegate" is false:
- `GoFuncCookie` (golib, 2026-09-29) mints a word per delegate INSTANCE, held weakly, in the band
  `0xC000_8000_0000_0000` with a 16-byte stride. Its header (GoFuncCookie.cs:31-40) proves the band disjoint from:
  - order tokens (bit 47);
  - caller spans (bit 62);
  - synthetic PCs (bits 61..48);
  - real addresses (a cookie is not canonical).
- The converter mints a cookie for `*(*unsafe.Pointer)(unsafe.Pointer(&f))`, where f is a func: it emits
  `unsafe.Pointer.OfFunc` (convCallExpr.go:5493-5529).
- golib recovers it for `*(*F)(unsafe.Pointer(&w))`, where w is a `uintptr` (ж.PointerExtensions.cs:203-214, and
  `FuncCookieReinterpret` at :382-420).
  - On a delegate-type mismatch it REBINDS, with `Delegate.CreateDelegate(F, target, method)`.
  - When that cannot bind, it panics by name.
- `GoSyntheticPC` keys one token per FUNCTION on `RuntimeMethodHandle`, under a lock.

**The revision extends GoFuncCookie; it adds no new type.** `GoFuncPtr` (1.3) is withdrawn, and with it 1.3's band
and 32-bit fallback.

| a func value that is | gets | held | why |
|:--|:--|:--|:--|
| a static method, with no target (every converted top-level Go func) | one word per METHOD, keyed on `RuntimeMethodHandle` as GoSyntheticPC keys its tokens | strongly, for the process | Go's static funcval: `__ccgo_fp(f)` answers the same pointer at every site, and the funcval is never freed |
| anything else (a closure, a bound method value) | one word per delegate INSTANCE: today's behaviour, unchanged | weakly | Go's heap funcval: one per closure value, and collectable |

- **One band, one counter, one lock.** Both kinds come from GoFuncCookie's existing band and counter, so the proof at
  GoFuncCookie.cs:31-40 covers both unchanged.
  - A word is never 0 and never all ones (bits 61..48 are clear), so it cannot collide with C's sentinels
    (`SQLITE_TRANSIENT` is `^uintptr(0)`).
- **32-bit throws.** GoFuncCookie has no such guard today: on a 32-bit runtime the band would truncate to a small
  number. The revision adds a `PlatformNotSupportedException` at the mint, as GoSyntheticPC has under the 2026-09-03
  ruling.
- **Data use.** `ManagedPointerTokens.NamesNoUserMemory` (ж.PointerTokens.cs:377) refuses a word dereferenced as data,
  because bit 63 is set. The refusal is a caught nil-dereference panic.
  - This corrects 1.3, which said a user-mode access faults.
  - `GoFuncCookieTests.ACookieIsStableNonZeroAndInItsOwnBand` pins it.
- **Equality.** The method key gives one word per static function, across sites, packages and C# delegate types.
  - So `h == cFuncPointer(busyHandler)` (busy.go:36) holds even when the two sides convert the function to different
    delegate types.
  - Measured in 3.4: `Func<…>` and a named delegate over one method answer one word.
- **Recovery: the rebind is kept, and settled.**
  - Recovery returns the minted delegate when its type is the recovery's type.
  - Otherwise it rebinds over the same target and method. That answers a C# type mismatch between two spellings of
    one Go signature, such as `Func<…>` against a named delegate.
  - When `CreateDelegate` cannot bind, recovery panics by name, as today.
  - 1.3's "refuse on any mismatch" is withdrawn. The reviewer's dynamic run of sqlite's -short suite on Windows
    exercised 60 of 83 signatures and found 0 Go-type mismatches. A refusal would only ever fire on a C#-only
    mismatch, and that is exactly what the rebind answers.
- **A rebind cache.** It is keyed per (word, delegate type), for static functions only, and keeps a rebound recovery
  at 112 ns under AOT instead of 8.9–9.9 µs (3.4). A closure's rebind is not cached, because a cache would keep the
  closure alive.
- **Non-capturing lambdas.** C# compiles one to an instance method on a cached singleton, so it takes the
  per-instance row.
  - C# also caches its delegate per site, so one site still answers one word.
  - Go gives a non-capturing func literal one static funcval too.
  - No census site mints a closure through `__ccgo_fp` (1.2). This was not re-checked for m2 and w2 in 3.2.

**Migration.**
- **Unchanged signatures and callers:** `GoFuncCookie.Of`, `IsCookie`, `Resolve` and `unsafe.Pointer.OfFunc`. The
  callers are runtime's callback and nestedCall, debugcall.cs:269, the three cgocall.cs, and
  syscall_windows_test.cs:222.
- **The eight existing `GoFuncCookieTests`** mint lambdas, which have a target, so they stay on the per-instance path
  unchanged. That includes the weak-lifetime test, whose lambda captures a local.
- **New:** `GoFuncCookie.Func<F>(nuint word)`, the recovery the converter emits for the shapes in 3.3.
  - It is public, with `EditorBrowsable(Never)`, and takes one line in `PublicSurface.golib.txt`.
  - `FuncCookieReinterpret.Resolve` calls it, so the runtime route and the converter route share one resolve and one
    rebind.
- **New GolibTests:**
  - one word for a static function across two delegate types;
  - a 64-thread mint race;
  - a rebound recovery;
  - the 32-bit refusal, as a unit over the guard, since no 32-bit runtime runs here.

### 3.2 Census per shipped target

**The targets.** The shipped targets are win-x64, linux-x64, linux-arm64, osx-x64 and osx-arm64 (push-nuget.ps1:17).

**The tool.** `rev/fpcensus3-main.go` reads modernc.org/sqlite, sqlite/lib, sqlite/vfs, sqlite/vec, sqlite/vtab and
modernc.org/libc/... It runs once per GOOS/GOARCH, with CGO_ENABLED=0. The output is `rev/fpcensus3-<target>.txt`.

The classes are structural:

| class | shape |
|:--|:--|
| m1 | `__ccgo_fp`'s shape, a function returning an `interface{}` parameter's data word (`(*[2]uintptr)(unsafe.Pointer(&f))[1]`), and its call sites |
| m2 | a func value read as a word: `*(*uintptr)(unsafe.Pointer(&x))`, where x is a func, or a struct whose only field is a func or a type parameter |
| w2 | a func written INTO a word slot: `*(*F)(unsafe.Pointer(e)) = fn`. This is a mint. |
| r1 | `*(*F)(unsafe.Pointer(&struct{ uintptr }{x}))`. The field is EMBEDDED at every site; 1.1's `struct{ f uintptr }` was wrong. |
| r2 | `(*struct{ f F })(unsafe.Pointer(&struct{ uintptr }{x})).f` |
| w1 | a word written THROUGH a func variable: `*(*uintptr)(unsafe.Pointer(&f)) = x`. This is a recovery. |

Sites:

| class | windows/amd64 | linux/amd64 | linux/arm64 | darwin/amd64 | darwin/arm64 |
|:--|--:|--:|--:|--:|--:|
| m1 call sites | 1,410 | 1,467 | 1,468 | 1,559 | 1,559 |
| m2 | 22 | 32 | 13 | 24 | 24 |
| w2 | 13 | 13 | 13 | 13 | 13 |
| **mints** | **1,445** | **1,512** | **1,494** | **1,596** | **1,596** |
| r1 | 529 | 542 | 542 | 519 | 519 |
| r2 | 7 | 1 | 1 | 7 | 7 |
| w1 | 0 | 0 | 0 | 1 | 1 |
| **recoveries** | **536** | **543** | **543** | **527** | **527** |
| … stored rather than called at once | 5 | 1 | 1 | 2 | 2 |

- **Per package.** lib's recoveries are 496, 454 and 485 on windows, linux and darwin. vec has 33 everywhere. libc
  has 7, 56 and 8. On darwin, vfs has 1 r2.
- **What first missed.** windows libc has no m1 and no r1: its hand code uses r2. lib's nine `lib/mutex.go:43` mints
  exist only on windows and darwin.
- **The sites the reviewer named are all in the table:**
  - libc_windows.go:3024 (r2, stored);
  - libc.go:2376 (r2);
  - pthread.go:708 (r2, stored);
  - libc_unix.go:63-64 (w1);
  - lib/mutex.go:43 (m2, nine, package-level);
  - conn.go:482 (m2);
  - sqlite.go:1223 (m2, of a type parameter).
- **A misclassification, found while writing this block.** The v2 tool counted 13 vfs sites as STORED recoveries:
  - `*(*F)(unsafe.Pointer(&vfsio.xWrite)) = vfsWriteReadOnly` (vfs/patches.go:122);
  - the nine offset stores at vfs/patches64.go:18.

  Each writes a func INTO a word slot, so it is a mint (w2). v3 classifies assignment targets separately. Only those
  13 moved, on all five targets, and no r3 or r4 recovery remains.

### 3.3 What the converter emits, and what golib does

One rule, applied in four directions: **a func value's storage is its word.** Each shape is recognised by its type,
never by a name:

| direction | Go | converted |
|:--|:--|:--|
| read a func as a word (m1, m2) | `(*[2]uintptr)(unsafe.Pointer(&f))[1]`, where f is an `interface{}` holding a func; or `*(*uintptr)(unsafe.Pointer(&x))`, where x is a func or a one-func-field struct | `GoFuncCookie.Of(…)` of the func, as a `uintptr`. An interface that holds no func panics by name. |
| write a func into a word slot (w2) | `*(*F)(unsafe.Pointer(e)) = fn` | a store of `GoFuncCookie.Of(fn)` through `*(*uintptr)(unsafe.Pointer(e))`, which the existing unsafe word store already converts |
| read a word as a func (r1, r2) | `*(*F)(unsafe.Pointer(e))`, where F is a func type. r2's `(*struct{ f F })(unsafe.Pointer(e)).f` folds into it, because the field is at offset 0. | `GoFuncCookie.Func<F>(word)`, with the word read from e as a `uintptr`. For `&struct{ uintptr }{x}`, the word is x itself. |
| write a word into a func variable (w1) | `*(*uintptr)(unsafe.Pointer(&f)) = x` | `f = GoFuncCookie.Func<F>(x)`, a statement-level emission |

- **The result is a delegate value,** so a stored recovery needs nothing more than one called at once.
- **golib's existing runtime route stays** for any `Reinterpret<uintptr, F>` the converter does not see, such as
  runtime's `callback`. The revision changes it in one way:
  - a word read as a delegate that is neither 0 nor a cookie panics by name ("0x… is not a func word");
  - today it falls through to `FieldRefBox`, which would read a number as a reference.
- **The one by-value aggregate (what remains of item 4).** `rev/fpaggr-main.go` (output `rev/fpaggr-<target>.txt`)
  counts func-word signatures that pass a struct or an array by value.
  - Exactly one recovery and one mint do, both on windows/amd64. The other four targets have none.
    - The mint is `__ccgo_fp(libc.XSetFilePointerEx)`, whose parameter is libc's `largeInteger`.
    - The recovery is lib's winSeekFile,
      `(*(*func(*libc.TLS, THANDLE, TLARGE_INTEGER, TPLARGE_INTEGER, TDWORD) TBOOL)(…))(…)`
      (lib/sqlite_g_00000000000c0000.go:117332).
  - Both types are aliases of one anonymous struct, so Go calls them identical.
  - go2cs lifts an anonymous struct to its own C# type in each package (anonStructNamedConversion_test.go:20).
    The two delegates therefore differ in a parameter TYPE, `CreateDelegate` cannot bind them, and that recovery
    would panic by name on a Windows seek.
- **Recommendation, for a ruling.** When an alias's anonymous struct is identical to one that an imported package
  already lifted, the converter reuses the imported lift (a using alias) instead of lifting a second type.
  - C# type identity then follows Go's, here and for every other cross-package use of such a struct.
  - It is a converter change with its own corpus footprint, so it is a separate seat.
  - Until it lands, this one site is a named red on windows/amd64, and P1.2 names it.

### 3.4 The prototype: sound, then measured

**The code.** `rev/funcword-v1-Program.cs` and `rev/funcword-v2-Program.cs` implement 3.1's table as a standalone
class in GoFuncCookie's band, with no golib reference. Each has:
- one lock;
- a `RuntimeMethodHandle` dictionary for static functions;
- a `ConditionalWeakTable`, plus a weak index, for instances;
- the rebind.

**What v2 adds:**
- a fast path keyed on the delegate instance, ahead of the method lookup. It is a lock-free `ConditionalWeakTable`
  read, so a repeated mint of one delegate instance never reads `Delegate.Method`;
- the rebind cache.

**Both versions pass every check** under the JIT and under Native AOT (linux-x64, Release; the AOT publish has 0
IL/AOT warnings):
- one word per static function, across `Func<object, nuint, int>` and a named delegate `Cmp` over one method;
- the word is in GoFuncCookie's band;
- recovery as the minted type, and as the other delegate type through the rebind;
- a signature that cannot bind is refused by name;
- two closures get two words, and one closure gets one word;
- **the race.** 300 rounds × 64 threads × 200 functions, each round on a fresh registry. The threads mint in
  different orders and alternate the two delegate types. 0 functions were split across two words, and in 0 rounds
  did two functions share one.

**Timings.** Single thread, warm, 200,000 calls, ns per call, second pass (`rev/funcword-v{1,2}-{jit,aot}.txt`):

| | v1 JIT | v1 AOT | v2 JIT | v2 AOT |
|:--|--:|--:|--:|--:|
| mint (one delegate instance, repeated) | 27 | 541 | 19 | 24 |
| recover as the minted type, and call | 106 | 30 | 136 | 39 |
| recover through a rebind, and call | 564 | 9,888 | 413 | 112 |

- **Under AOT, `Delegate.Method` and `Delegate.CreateDelegate` work with 0 warnings, but they are slow.** In v1 a mint
  costs 541 ns and a rebind about 9.9 µs. v2's fast path and cache are what make them cheap.
- **A FIRST mint** of a static function still reads `Delegate.Method` under the lock: about 0.5 µs under AOT, once per
  delegate instance.
- **The JIT columns were not controlled for tiering (OSR),** so they are reported, not compared. The AOT columns are
  what sqlite-cs ships.
- **For comparison,** the reviewer's sound single-table prototype measured 253 ns per mint. v2's 24 ns comes from the
  fast path.

### 3.5 M:N (revises 1.5 and 1.7, for G)

- **The rows.**
  - A static function has one row for the process: its word, its plain delegate and its cached rebinds.
  - A closure has a weak row per instance.
  - If N3 keeps a twin per function, the static row is where it belongs, keyed on the METHOD.
- **Key twins on the method, not the delegate instance.**
  - A rebound recovery returns a NEW delegate over the same target and method (3.1), so a twin lookup keyed on the
    delegate instance misses after a rebind.
  - A lookup keyed on (method, target) finds it.
  - 1.5's "ReferenceEquals holds" is true only when no rebind happened. It is withdrawn as a guarantee.
- **What recovery returns: the plain body (item 7).** Each of these calls goes through a recovery:
  - every sqlite mutex call (lib/mutex.go's nine mints);
  - every VFS method call (vfs's 24 mints);
  - the busy handler.

  So under N3 as it stands, each park inside one of those calls holds a carrier thread.
- **Thread safety.**
  - Mint and recovery each take GoFuncCookie's one lock for a dictionary lookup; v2's mint fast path takes none.
  - A recovery costs about 39 ns under AOT, including the call.
  - NOT MEASURED: contention under many goroutines.
- **Questions for G:**
  - **G1.** Does N3 intend twins keyed per FUNCTION (the method, plus the target for a bound method), as above?
  - **G2.** Until N3 lands, is it acceptable for sqlite-cs that each park in sqlite's mutex, VFS and busy-handler calls
    holds a carrier? Or must the mutex path get twins first?

### 3.6 Predictions and stop lines (replace 1.6)

**Predictions:**
- **P1.1** With 3.1 and 3.3, sqlite-cs's start-up passes the S1 wall on all five targets.
- **P1.2** sqlite's suite reaches no rebind refusal on any target, with one exception until 3.3's lift seat lands:
  winSeekFile's `TLARGE_INTEGER` recovery on windows/amd64. If that site is reached, it is the only refusal, and the
  refusal names the lifted type.
- **P1.3** A converted sqlite run mints at most one word for each distinct static function, whatever the delegate
  types or sites.
- **P1.4** The eight existing GoFuncCookieTests pass unchanged.
- **P1.5** Native AOT publishes with 0 new warnings. The prototype publishes with 0.

**Stop lines:**
- **X1.1** Two words for one static function.
- **X1.2** A word outside GoFuncCookie's band, or a 32-bit mint that does not throw.
- **X1.3** A rebind failure that does not panic by name, or a non-cookie word read as a func that does not panic by
  name.
- **X1.4** `Reflection.Emit` or `DynamicInvoke` in the path.
  - `Delegate.Method` and `CreateDelegate` are reflection reads. GoFuncCookie already uses them, and they publish with
    0 AOT warnings.
  - 1.6's X1.4 ("no reflection") is narrowed to this.
- **X1.5** A closure's word held strongly, or a static function's word dropped.

### 3.7 S21 revised

- **The x/sys/unix rows are dropped (item 8).**
  - `src/go2cs/asmTrampolines.go` already forwards third-party pure-JMP trampolines structurally
    (`TEXT ·Syscall … JMP syscall·Syscall(SB)`). Each forward is gated on `types.Identical` or on a proven layout
    bridge (`layoutIdenticalStructs`, :321), which is how the gettimeofday bridge works.
  - It also covers the `NoError` entries and unexported targets (`asmJumpForwardTargets`, :91).
  - 2.3's x/sys/unix rows duplicated it.
- **The trampoline-address DATA symbols (item 9).** 2.1 counted only bodyless functions.
  - **darwin x/sys/unix** (v0.49.0) defines 159 `*_trampoline_addr` DATA symbols per arch: `zsyscall_darwin_amd64.s`
    and `zsyscall_darwin_arm64.s` each hold 318 GLOBL and DATA lines.
  - **x/sys/cpu** defines 2 on darwin/amd64 (`libc_sysctl`, `libc_sysctlbyname`) and 1 on darwin/arm64
    (`libc_sysctlbyname`).
  - **The existing index covers them.** `parseAsmTrampolineAddrs` (:681, with `asmAddrDataRE` at :666) reads each
    package directory's selected `.s` files. `asmTrampolineAddrIndex` (:459) keys them per (directory, platform,
    tags). Nothing in either is specific to x/sys/unix.
  - **Tests:** `TestXSysDarwinLibcPullsForwardToTwins` (xsysDarwinLibc_test.go:120), `TestParseAsmTrampolineAddrs`
    (:176) and `TestXSysDarwinLibcEmission` (:216).
  - **NOT MEASURED:** an x/sys/cpu darwin conversion through that index. The implementing seat adds an x/sys/cpu case
    to `TestParseAsmTrampolineAddrs`.
- **The name table holds only bodies with real machine code (items 8 and 11).**
  - Its rows are x/sys/cpu's `cpuid`, `xgetbv`, and the four arm64 ID-register reads. The ID-register reads are
    reached only on freebsd (`cpu_arm64.go:46`).
  - A listed function whose declared signature differs is a convert-time error.
- **darwin/arm64's sysctl path** is the linkname pull `syscall_syscall6`. It is a `linknameForwardTargets` entry, not
  a name-table row (2.3's row stands).
- **X2.2, reworded (item 10).**
  - std's `internal/cpu` sets each flag with a CLR equivalent from `.IsSupported` and leaves the rest zero
    (`initX86FeatureDetection`, cpu_x86_impl.cs:123-151). So comparing x/sys/cpu with it flag by flag trips by
    construction.
  - x/sys/cpu keeps Go's behaviour: its flags come from the cpuid bits its own `doinit` decodes, where the golib body
    runs `X86Base.CpuId`. XCR0 is synthesised from `.IsSupported`, because .NET has no XGETBV.
  - **The new X2.2** trips on either of two things:
    - a cpuid-derived x/sys/cpu flag that disagrees with the same bit of `X86Base.CpuId` on the same host;
    - an XCR0-gated flag (AVX, AVX2, AVX-512) that is set where .NET's `.IsSupported` says the OS does not allow it.
- **P2.2, reworded to match.** Under .NET's default configuration, x/sys/cpu's `X86.HasAVX2` equals
  `Avx2.IsSupported` on the same host.

### 3.8 Amendment after the re-review (dated block, 2026-11-13, C1)

> The re-review is COORD's inbox post C1 20261113T074423Z, items N1–N5. It rules S1 APPROVE WITH CHANGES and S21
> APPROVE; G implements both, after its nil-func seats.
>
> Read again at master `b6f3bbb44f`. GoFuncCookie.cs, ж.PointerExtensions.cs, convCallExpr.go and reflect/value.cs
> are unchanged since `3da19d0e2e` (`git diff` is empty), so every reading above still describes this tree.
>
> The new census is `rev/fpcensus4-main.go`. Its outputs are `rev/fpcensus4-<target>.txt` (sqlite) and
> `rev/fpcensus4-std-<target>.txt` (std).

**N1: the rules, narrowed, and their stdlib footprint.** This table replaces 3.3's "Go" column for which sites match.
3.3's "converted" column stands.

| rule | matches only | left on today's emission |
|:--|:--|:--|
| m1 | a function with one interface parameter and a `uintptr` result whose body returns `(*[2]uintptr)(unsafe.Pointer(&f))[1]` (ccgo's `__ccgo_fp`, in libc, lib and vec) | sync/pool.go:92 `poolRaceAddr`, which reads `[2]unsafe.Pointer` of a pool item and returns `unsafe.Pointer` |
| m2 | `*(*uintptr)(unsafe.Pointer(&x))` READ, where x is a func or a one-func-field struct, and not the left side of an assignment (N3) | the `*(*unsafe.Pointer)` read, already `unsafe.Pointer.OfFunc` (convCallExpr.go:5501-5545); it answers the same word |
| w2 | `*(*F)(unsafe.Pointer(e)) = fn` | — |
| r1, r2 | the source is `&S{x}`, where S is a struct whose only field is a `uintptr` (embedded or named) | `&v` with v a `uintptr` (r3), which is golib's existing `Reinterpret` route and already resolves a cookie; a pointer-typed source (runtime/mcleanup.go:203, `&fn` with fn a `*funcval`); memory at an address (runtime/panic.go:969, a defer slot) |
| w1 | `*(*uintptr)(unsafe.Pointer(&f)) = x` only | `*(*unsafe.Pointer)(unsafe.Pointer(&f)) = p`: runtime/cgocall.go:465 and debugcall.go:266 (emitted at cgocall.cs:482 in all three flavours, and at debugcall.cs:269). These build a funcval by hand. They keep today's emission, a store into a temporary box: unchanged, named here, and outside this design. |

**std census** (fpcensus4, `std` with `_test.go` files, CGO_ENABLED=0, windows/amd64, linux/amd64 and darwin/amd64;
each site counted once across test variants). Each site it finds and what the narrowed rules do with it:

| site | shape | under the narrowed rules |
|:--|:--|:--|
| sync/pool.go:92 | m1-like | not matched (above). pool.cs is hand-owned too. |
| **reflect/value.go:1145** `funcName` | m2, `*(*uintptr)(unsafe.Pointer(&f))` | **MATCHED**: one hunk |
| runtime/mcleanup.go:203 | a recovery from a `*funcval` | not matched. mcleanup.cs is hand-owned too. |
| runtime/panic.go:969 | a recovery from memory | not matched |
| runtime/cgocall.go:465, debugcall.go:266 | `*unsafe.Pointer` store | not matched (not w1) |
| runtime/syscall_windows_test.go:174 (windows) | m2' (`*unsafe.Pointer` read) | already OfFunc; unchanged |
| runtime/syscall_windows_test.go:164 (windows) | r3 | the existing Reinterpret route; unchanged. It passes a cookie, so 3.3's non-cookie panic does not fire. |
| any w2 | — | 0 sites |

**Prediction for the implementing seat's two-seeded `-stdlib` diff,** on each of the three flavours:
- exactly one changed line, in reflect/value.cs (`funcName`, today `var pc = ~Ꮡf.Reinterpret<Func<slice<ΔValue>,
  slice<ΔValue>>, uintptr>();`), becoming the cookie word;
- 0 `GoPositionMap` lines, because no line is added or removed;
- 0 changes to any hand-own's `.cs.auto` sibling.

**The falsifiers:**
- any other file;
- any map line;
- the `heap(fʗp, out var Ꮡf)` line moving. Go still takes `&f`, so the address-taken decision does not change.

**The behaviour this changes:**
- `runtime.FuncForPC` of a cookie is nil, so `funcName` answers "closure".
- Go answers the same, for the same reason: Go's `*(*uintptr)(&f)` is the funcval's ADDRESS, not a code PC.
- Today's answer is NOT MEASURED.
- It is reached only from MakeFunc's panic messages.

**N2: the mint's delegate form is a stop line.** v2's 24 ns mint depends on C# caching the delegate per site. C# does
that for a conversion of a method group (`(Func<…>)F`, or F passed where a delegate is expected), which is what the
corpus emits today. `new Func<…>(F)` is not cached: the reviewer measured a new instance at every evaluation and
155–541 ns per mint.
- **X1.6** An m1 call site, or an m2/w2 source, that creates its delegate with `new F(M)` instead of a
  method-group conversion. The implementing seat adds a converter test that asserts the conversion form at an m1
  call site.

**N3: darwin counted the w1's left side as an m2.**
- libc_unix.go:64 is the left side of the w1 store, not a read. darwin m2 is therefore **23**, and darwin mints are
  **1,595**, on both darwin targets.
- fpcensus4 matches w1 first. That moved exactly this one site on both darwin targets and nothing on the other three
  (`diff` against fpcensus3).
- The converter likewise matches w1 before m2.

**N4: lift reuse is ruled YES in direction and routed to P1,** as part of the naming family.
- **The spec P1 writes must say:**
  - that the lift is publicly referenceable across assemblies, since libc's `largeInteger` is unexported;
  - what happens when NEITHER package imports the other. My suggestion: nothing. Both lifts stay, and a func word
    passed between them is refused by name by the rebind. The census finds no such case: the reviewer's deeper scan
    finds `largeInteger`/`TLARGE_INTEGER` the only member of the class.
- **P1.1 is amended:** sqlite-cs start-up passes the S1 wall on all five targets, except winSeekFile's
  `TLARGE_INTEGER` recovery on windows/amd64 until P1's lift seat lands. This is the same exception as P1.2. If the
  lift seat lands first, it is a windows/amd64 prerequisite instead.

**N5: Resolve's pruning is kept.**
- GoFuncCookie.Resolve (GoFuncCookie.cs:104-120) removes a dead weak entry when it resolves one. The implementation
  keeps that for the closure path; the prototype in 3.4 lacked it.
- Dead closures' entries that are never resolved stay in the index, as they do today. This is not changed.
