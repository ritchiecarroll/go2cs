# CENSUS: a run-time `runtime.GOARCH` for the linux and windows flavours on arm64 hosts
<!-- {% raw %} — Jekyll/Liquid guard: §9 quotes a go list template ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> **State: MEASURED (2026-11-09), P2.** A read-only census at master `fb885891b3`, with go1.26.10 for Go's own
> file selection. Nothing in `src/core` or `src/go2cs` was changed. Point-in-time: amend with dated blocks, never
> rewrite. Nothing here ran on an arm64 host; every reading of arm64 behaviour is from the tree and from Go's
> source, and says so.
>
> **Verdicts:**
> - **windows: YES.** A run-time `runtime.GOARCH` stays coherent in the windows flavour, with the same two
>   displacements darwin needed: `runtime.GOARCH` and `internal/buildcfg`'s `defaultGOARCH`.
> - **linux: NO.** Layer 2 alone reads the same as windows and darwin, but layer 1 does not hold: the linux flavour
>   reaches the kernel through glibc's `syscall(2)` with linux/amd64's call numbers and struct layouts, and on an
>   arm64 kernel those numbers name different calls (§6). The flavour cannot run on linux/arm64 whatever GOARCH says,
>   so the remedy there is a linux/arm64 flavour, not a displacement.
>
> One finding applies to every flavour, darwin included (§5, C): the managed heap dump's params record reads
> `goarch.GOARCH`, so on an arm64 host it writes `amd64` where Go writes `arm64`.

## 1. The question and the method

The darwin census (`DESIGN-comparator-arms.md` §5 on `claude/g-comparator-design`, and the commit message of
`claude/g-goarch-runtime` 28cfcf657e) asked whether `runtime.GOARCH` can read the process architecture while
`internal/goarch`, the amd64 file selection and every folded constant stay amd64. It answered yes for darwin, and the
BOARD row of 2026-11-09 leaves linux and windows open, pending the same census per flavour. This record is that
census. It follows the darwin method:

- **Layer 1:** Go's own file selection per GOARCH (`go list std`), with an amd64-against-amd64 control.
- **Layer 2:** the C# sites that read the architecture, in five categories, each with a positive control:
  - **A**, runtime's own unqualified `GOARCH`, with each amd64/arm64 comparison classified INERT, IMPROVE,
    EQUIVALENT or UNREACHABLE. UNREACHABLE is used only where a hand-own's own record says the path does not run.
  - **B**, `runtime.GOARCH` qualified, outside the runtime package.
  - **C**, `goarch.*` read live (it stays amd64).
  - **D**, constants folded at conversion.
  - **E**, constants Go computes from `runtime.GOARCH`, which the conversion folds (the `defaultGOARCH` class).

**Population, per flavour.** Every package `go list std` selects at that GOOS/amd64 (linux 360, windows 362), and
for each its flat `.cs` files plus its `<goos>/` subdirectory, under `src/core`. Production files are those not named
`*_test.cs` or `package_info*test.cs`: linux 2660 production and 1142 test files, windows 2609 and 1148. Comment text is
removed before matching (`//`, and `/* */` across lines), so a folded constant's comment counts in D and nowhere else.

**Predicates.** Python regular expressions with explicit character classes, since this git's ERE ignores `\b`:

| Category | Predicate on comment-free code |
|---|---|
| A | in package `runtime`: `GOARCH`, not preceded by a character in `[A-Za-z0-9_.ᵇ"]` and not followed by one in `[A-Za-z0-9_ᵇ]` |
| B | outside package `runtime`, `runtime.GOARCH` with the same boundaries |
| C | `goarch.` followed by an identifier |
| D | a line holding `/*` whose comment names `GOARCH` or `goarch.`, and which has no live A, B or C match |
| E | Go source, not C#: see §5, E |

A site "compares amd64/arm64" when its line names `"amd64"` or `"arm64"`, or when it copies GOARCH into a local that a
switch then compares with either (four such sites, read by hand).

**Darwin as a reference.** The same predicates over the darwin flavour give A 39 lines, 19 comparing amd64/arm64. The
darwin census reported 42 and 17; its predicate is not published, so the two counts are not comparable. Every site
the darwin census names is in this predicate's darwin set.

## 2. Layer 1: Go's file selection per GOARCH

`go list -tags purego,math_big_pure_go std` with `CGO_ENABLED=0`, go1.26.10, at GOOS=linux and GOOS=windows, each at
GOARCH amd64, amd64 again (the control) and arm64. **Control: the two amd64 lists are byte-identical on both
flavours.** The same command at darwin reproduces the darwin census's 14 packages.

| | linux | windows |
|---|---|---|
| Packages at amd64 / arm64 | 360 / 358 | 362 / 360 |
| Packages only at amd64 | `runtime/race/internal/amd64v1`, `internal/runtime/startlinetest` | the same two |
| Production packages selecting different files | 15 | 14 |
| Packages differing only in test files | 1 (`crypto/sha1`: `issue15617_test.go` is amd64-only) | 0 |

The 14 windows packages are darwin's 14 with `internal/runtime/syscall/windows` in place of the darwin one: `hash/crc32`,
`internal/abi`, `internal/bytealg`, `internal/cpu`, `internal/goarch`, `internal/runtime/atomic`,
`internal/runtime/gc/scan`, `internal/runtime/sys`, `internal/runtime/syscall/windows`, `math`, `runtime`, `runtime/race`,
`syscall` and `vendor/golang.org/x/sys/cpu`. Linux has the same set with `internal/runtime/syscall/linux`, plus
`internal/syscall/unix`.

What differs inside the system-call packages decides §6:

- **windows:** `syscall` differs only in `types_windows_{amd64,arm64}.go` and `internal/runtime/syscall/windows` only
  in `defs_windows_{amd64,arm64}.go`. Both carry the thread `CONTEXT` layout and `CONTEXT_CONTROL`. DLL calls are made
  by name, and no other type differs.
- **linux:** `syscall` differs in `zsysnum_linux_*` (the call numbers), `ztypes_linux_*` (kernel struct layouts),
  `zerrors_linux_*`, `zsyscall_linux_*` and `syscall_linux_*`; `internal/runtime/syscall/linux` in `defs_linux_*`
  (call numbers again); and `internal/syscall/unix` in `sysnum_linux_*` and the fstatat selection.

## 3. Layer 2, category A: runtime's own `GOARCH`

| | linux | windows |
|---|---|---|
| Production code lines naming `GOARCH` (the declaration in `extern.cs`:374 included) | 39 | 42 |
| Of those, comparing amd64/arm64 (inline, or through a switch over a copy) | 17 | 18 |
| The rest: comparisons naming only other architectures (wasm, mips, riscv64, 386, ppc64, arm), or the declaration | 22 | 24 |

**Positive control:** `alg.cs`:433 (`GOARCH == "arm64"u8 && cpu.ARM64.HasAES`) is matched on both flavours.

Every amd64/arm64 comparison, and its reading when GOARCH reads `arm64` while everything else stays amd64:

| Site | Flavours | Reading | Why |
|---|---|---|---|
| `runtime/alg.cs`:427 | both | UNREACHABLE | alginit's x86 arm. alginit's only caller is schedinit (`<goos>/proc.cs`:933), and `hash_impl.cs`:62 and :71 record that schedinit never runs; `initHashAlgorithm` is the live selection |
| `runtime/alg.cs`:433 | both | UNREACHABLE | alginit's arm64 arm; the same record |
| `runtime/hash_impl.cs`:181 | both | INERT | the live hash selection. Its x86 arm is false at arm64 either way, because `cpu.X86` is not filled on an arm64 process (`cpu_arm64_impl.cs` header). Its arm64 arm keys on the process (`cpu.GoIsArm64Process`), not on GOARCH |
| `runtime/cpuflags_impl.cs`:37 | both | INERT | cpuinit's copy half (a hand-own). The x86 flags read false at arm64 either way; `arm64HasATOMICS`, the only flag the arm64 arm writes, has no production reader |
| `runtime/<goos>/proc.cs`:799 | both | UNREACHABLE | cpuinit's switch; `cpuflags_impl.cs`:14 and :23 record that cpuinit stays unreachable with the rest of schedinit |
| `runtime/lfstack.cs`:40 | both | EQUIVALENT | a spin backoff count (128 at arm64, 0 at amd64); the result is the same |
| `runtime/mspanset.cs`:152 | both | EQUIVALENT | the same backoff count |
| `runtime/mbitmap.cs`:233 | both | EQUIVALENT | both arms clear the lowest set bit of `tp.mask` (BTC against `mask &= mask - 1`) |
| `runtime/mbitmap.cs`:1453 | both | INERT | the condition names both amd64 and arm64 |
| `runtime/stkframe.cs`:257 | both | INERT | the condition names both amd64 and arm64 |
| `runtime/tagptr_64bit.cs`:75 | both | EQUIVALENT below 2^48 | amd64's arm sign-extends from bit 48 of the packed address and arm64's does not; they agree while bit 48 is clear. The packers are lfstack's pinned boxes and netpoll's pinned `pollDesc` (`linux/netpoll_epoll.cs`:25, `windows/netpoll_windows.cs`:46). Linux/arm64 user space is 48-bit unless a mapping is hinted above it; Windows/arm64 user space is 2^47. `taggedPointerPack` checks the round trip and throws by name if it fails. Read, not run |
| `runtime/<goos>/malloc.cs`:650 | both | INERT | `GOARCH == "arm64" && GOOS == "ios"`, and GOOS is folded to linux or windows |
| `runtime/<goos>/malloc.cs`:654 | both | UNREACHABLE | mallocinit; `arena_impl.cs`:12 records that mallocinit never runs |
| `runtime/<goos>/proc.cs`:5087 | both | UNREACHABLE | newproc1's frame setup. `managed_impl.cs`:1291 records that a goroutine is a CLR thread with no Go stack; `newproc` has no caller in the corpus, since a `go` statement is lowered to `goǃ` |
| `runtime/<goos>/stack.cs`:870 | both | UNREACHABLE | adjustctxt, reached from copystack and newstack; `managed_impl.cs`:1291-1295 records that newstack is a scheduler path the managed host does not run |
| `runtime/linux/os_linux.cs`:502 | linux | EQUIVALENT | setsig fills `sa_restorer` only on 386 and amd64, then hands the struct to `sysSigaction`. `linux/signal_posix_impl.cs`:12-15 records that `rt_sigaction` is an unimplemented external stub on the CLR, and the three install paths are hand-owned over `PosixSignalRegistration`. Either arm ends in the same refusal |
| `runtime/linux/signal_unix.cs`:336 | linux | UNREACHABLE | sigFetchG, called only from sigtrampgo and sigfwdgo; `linux/signal_posix_impl.cs`:16 records that there is no native Go trampoline to install |
| `runtime/windows/signal_windows.cs`:57 | windows | UNREACHABLE | isAbort, on the vectored-exception path that `initExceptionHandler` arms from osinit. `windows/signal_windows_impl.cs`:26-27 records that osinit is emitted not-run and stdcall ends in `asmstdcall`, a throwing stub |
| `runtime/windows/signal_windows.cs`:365 | windows | UNREACHABLE | lastcontinuehandler; the same record |
| `runtime/windows/syscall_windows.cs`:249 | windows | UNREACHABLE | callbackasmAddr. Its callers sit inside runtime's `compileCallback`, after the check that panics on the inert nil eface; `syscall/windows/syscall_windows_callback_impl.cs`:16-20 records this, and `syscall.NewCallback` takes the managed seam instead |

| Reading | linux | windows |
|---|---|---|
| INERT | 5 | 5 |
| IMPROVE | 0 | 0 |
| EQUIVALENT | 5 | 4 |
| UNREACHABLE | 7 | 9 |

**No site improves, unlike darwin's three.** Darwin's improvements come from `cpu.ARM64` being filled on Apple
silicon. `cpu_arm64_impl.cs`'s `osInitArm64` has a body only on darwin (`darwin/cpu_arm64_darwin_impl.cs`), so on
linux and windows every `cpu.ARM64` flag stays false. For windows that matches Go: Go's windows/arm64 `osInit` is
empty (`internal/cpu/cpu_arm64_other.go`), so Go also takes the fallback hash there. For linux it does not: Go fills
`cpu.ARM64` from HWCAP (`cpu_arm64_hwcap.go`), so Go on linux/arm64 uses the AES hash. That gap is in the CPU-feature
fill, not in a GOARCH site, and it matters only once linux runs on arm64 at all (§6).

**A note for the darwin table, not measured here.** The darwin census reads `alg.cs`:427/433 as IMPROVE. At this
master, alginit's only caller is schedinit, which `hash_impl.cs` records never runs, so this record reads those two
lines UNREACHABLE; the live twin is `hash_impl.cs`:181.

## 4. Layer 2, category B: `runtime.GOARCH` outside the runtime

| | linux | windows |
|---|---|---|
| Production lines | 35 | 31 |
| Test lines | 62 | 62 |

**Positive control:** `vendor/golang.org/x/sys/cpu/byteorder.cs`:44 (a switch over `runtime.GOARCH`) is matched on both.

Production, by what the line does:

| What the line does | linux | windows | Reading at arm64 |
|---|---|---|---|
| Reports the architecture in a string (`fips140.cs`:73, `go/build/gc.cs`:15, coverage meta `emit.cs`:607, testenv's skip and error messages, `nettest.cs`:235/282) | 15 | 15 | now the true arch, as Go reports |
| Compares only with other architectures or other OSes (s390x, wasm, ppc64, arm, netbsd/arm64, and testenv's `CPUIsSlow` list) | 12 | 8 | inert |
| A switch whose amd64 and arm64 cases agree (`byteorder.cs`:44, little-endian) | 1 | 1 | inert |
| Asks `internal/platform` about the running platform (`testenv.cs`:105, 363, 398, 408) | 4 | 4 | now asks about arm64, as Go's own test binary does |
| `nettest/conntest.cs`:609 (`GOOS == "windows" && GOARCH == "arm64"`) | 1 | 1 | windows: skips the subtest Go skips; linux: inert |
| `x/sys/cpu/cpu_x86.cs`:61 (sse2 `Required` only on amd64) | 1 | 1 | changes only whether `GODEBUG=cpu.sse2=off` is refused; no behavioural flip |
| `go/build/<goos>/build.cs`:405 (`runtime.GOARCH == c.GOARCH`) | 1 | 1 | **incoherent unless `defaultGOARCH` moves too** (§5, E) |

Test: the 62 lines are the same 62 on both flavours. Twelve name amd64 or arm64: `debug/gosym/pclntab_test.cs`:40,
`net/http/client_test.cs`:1633/1662/1720, `net/sendfile_test.cs`:120, `unicode/letter_test.cs`:472, and one line each in
`internal/trace`, `net` (two), `os/exec`, `runtime/pprof` and `time`, those six also guarded by GOOS or a builder check. Each follows the arm64 Go oracle's own branch, so a skip or a check that Go makes at arm64
the converted test now makes too.

## 5. Layer 2, categories C, D and E

**C: `goarch.*` read live (stays amd64).**

| | linux | windows |
|---|---|---|
| Production lines reading `goarch.<name>` | 555 | 560 |
| Of those, a member whose value differs between amd64 and arm64 (`GOARCH`, `IsAmd64`, `IsArm64`, `ArchFamily`, `DefaultPhysPageSize`, `PCQuantum`, `MinFrameSize`, `StackAlign`) | 13 | 13 |

**Positive control:** `runtime/heapdump.cs`:553 (`dumpstr(goarch.GOARCH)`) is matched on both. Almost every line reads
`PtrSize` (linux 514, windows 521), which is 8 at both architectures. The 13 arch-dependent reads stay amd64, coherent
with the amd64 file selection they sit in:
- runtime frame and stack code: `stkframe.cs`:201, `<goos>/stack.cs`:745/765/1219, `panic.cs`:1467, `tracestack.cs`:272;
- `iface.cs`:534/639 (the interface-switch cache gate), `rand.cs`:245, and `<goos>/malloc.cs`:599 (in mallocinit);
- two that report the architecture: `heapdump.cs`:553 (off the live path: the managed `WriteHeapDump`,
  `managed_impl.cs`:1191, writes its own params record) and `crypto/internal/cryptotest/implementations.cs`:76 (which lists the implementations the amd64
  selection has);
- linux only: the `pread` switch in `internal/runtime/syscall/linux/linux/syscall_linux.cs`:88, which picks amd64's
  `SYS_PREAD64`, coherent with amd64's call numbers (§6);
- windows only: `defs_windows_amd64.cs`:80, the `CONTEXT` stack alignment, on the stdcall path that throws.

**Finding, every flavour.** The predicate cannot see a fully qualified read in a hand-own, so these were searched
separately (`goarch_package.` and `runtime_package.GOARCH` across `src/core`, golib included): three lines.
`runtime/debug/stubs_impl.cs`:199 builds the module info from `runtime_package.GOARCH`, which is right. But
`runtime/managed_impl.cs`:1212, the managed `WriteHeapDump`'s params record (live on every flavour), writes
`goarch_package.GOARCH`. Go's `dumpparams` writes `goarch.GOARCH` too, but Go's `goarch.GOARCH` is the binary's
architecture, so Go at arm64 writes `arm64`. Once `runtime.GOARCH` is run-time, that line should read
`runtime.GOARCH` to stay equal to Go on an arm64 host. This applies to darwin's seat already; the darwin census did
not name it.

**D: constants folded at conversion.**

| | linux | windows |
|---|---|---|
| Production fold comments naming `GOARCH` or `goarch.` | 67 | 68 |
| Of those, naming `runtime.GOARCH` or runtime's `GOARCH` | 4 | 4 |

**Positive control:** `crypto/internal/fips140/subtle/xor_generic.cs`:43 (`supportsUnaligned`'s fold) is matched on both.
The 63 and 64 folds over `goarch.*` stay amd64 by design, coherent with C. The 4 over GOARCH are E's four commented
folds.

**E: constants Go computes from `runtime.GOARCH`.** A Go program over each flavour's amd64 file selection
(production files from §2's lists, parsed with `go/parser`): a package-level const is counted when its value names
`runtime.GOARCH` through the file's own import of `runtime` (or, in package `runtime`, the identifier `GOARCH`), or
names another counted const of the same package, repeated until nothing changes. Implicitly repeated values in a
const group are followed. **Positive control:** `internal/buildcfg`'s `defaultGOARCH` is found; the same program at
darwin reproduces the darwin census's five.

The result is the same five on all three flavours:

| Const | Go source | linux and windows emission | Reading at arm64 |
|---|---|---|---|
| `internal/buildcfg.defaultGOARCH` | `zbootstrap.go`:20 | `internal/buildcfg/<goos>/zbootstrap.cs`:35, `defaultGOARCHᵇ = "amd64"`, no comment | **INCOHERENT: displace it.** `buildcfg.GOARCH` defaults to it (`cfg.cs`:30), go/build's `Default.GOARCH` copies that (`go/build/<goos>/build.cs`:365), and `build.cs`:405 then sees `runtime.GOARCH != c.GOARCH` and turns `Default.CgoEnabled` off |
| `runtime.framepointer_enabled` | `runtime2.go`:1505 | `runtime2.cs`:1395, folded `true` | INERT: true at amd64 and arm64 |
| `runtime.haveSysmon` | `proc.go`:6481 | `<goos>/proc.cs`:6218, folded `true` | INERT: true unless wasm |
| `vendor/golang.org/x/crypto/chacha20.unaligned` | `xor.go`:10 | `xor.cs`:11, folded `true` | INERT: the list names amd64 and arm64 |
| `crypto/internal/fips140/subtle.supportsUnaligned` | `xor_generic.go`:16 | `xor_generic.cs`:43, folded `true` | EQUIVALENT: false at arm64 in Go, which selects an aligned XOR with the same result; the C# body needs no alignment test (its own comment, lines 69-71) |

Plus `runtime.GOARCH` itself, the const being displaced.

## 6. Why linux is NO: the system-call layer is amd64's

The linux flavour reaches the kernel in one place: `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs` binds
glibc's `syscall(long number, ...)` and passes the call number through unchanged. The numbers are linux/amd64's:
`syscall/linux/zsysnum_linux_amd64.cs` (for example `SYS_READ` 0, `SYS_WRITE` 1, `SYS_GETRLIMIT` 97) and
`internal/runtime/syscall/linux/linux/defs_linux_amd64.cs`. The hand-own's header says its binding was measured on
linux/amd64 only. On an arm64 kernel the same numbers name other calls. From Go's own tables (`zsysnum_linux_amd64.go`
against `zsysnum_linux_arm64.go`):

| Call | amd64 number | arm64 number | What the amd64 number is on arm64 |
|---|---|---|---|
| read | 0 | 63 | io_setup |
| write | 1 | 64 | io_destroy |
| openat | 257 | 56 | unassigned |
| fcntl | 72 | 25 | pselect6 |
| uname | 63 | 160 | read |

So `fmt.Println` on linux/arm64 would issue io_destroy, not write. Kernel struct layouts differ too: `Stat_t` is
144 bytes at amd64 with `Nlink uint64` before `Mode`, and 128 bytes at arm64 with `Mode` before a 32-bit `Nlink`
(`ztypes_linux_*.go`), and the flavour marshals amd64's (`syscall/linux/zsyscall_linux_amd64_impl.cs` header). The one
place in the tree that already chooses a linux call number by architecture is golib's `ThreadCpuTime.cs`:145 (gettid:
186 on x64, 178 on arm64), and it is hand-written for that reason.

This is read from the tree and from Go's tables; no linux/arm64 host was available to run it. It does not depend on
GOARCH: today's flavour already issues these numbers on an arm64 host. A run-time `runtime.GOARCH` would make that
process report `arm64` without making it work. The remedy is a linux/arm64 flavour (layer 1's 15 packages, the
system-call tables first), and with it the HWCAP fill of `cpu.ARM64` that §3 notes.

Darwin and windows do not have this problem. Darwin calls libSystem functions through trampolines, not raw numbers,
and its system-call types agree between darwin/amd64 and darwin/arm64 (layer 1 for darwin differs in the same
packages, and the osx-arm64 rows bank today on the amd64 selection). Windows calls DLL procedures by name, and its only
arch-specific system-call type, `CONTEXT`, is used on stdcall paths that end in the throwing `asmstdcall`
(`runtime/windows/os_windows.cs`:1091 and :1240).

## 7. Verdicts

| Flavour | A run-time `runtime.GOARCH` coherent? | Consts to displace | Also needed |
|---|---|---|---|
| windows | **YES** | `runtime.GOARCH`, `internal/buildcfg.defaultGOARCH` | the heap dump's params line reads `runtime.GOARCH` (§5, C; shared code, so darwin gains it too) |
| linux | **NO** (layer 1, §6) | if a linux/arm64 flavour is built: none, since its `goarch` and selection would be arm64's own; if not, a displacement would only relabel a process that cannot make system calls | a linux/arm64 flavour; `cpu.ARM64` filled from HWCAP |

For windows the darwin seat's mechanism applies as it stands: `manualConversionConsts` entries for windows, and two
hand-owns beside the darwin pair (`runtime/windows/` reading `RuntimeInformation.ProcessArchitecture`, and
`internal/buildcfg/windows/` deriving `defaultGOARCH` from it). The darwin seat's footprint prediction (windows 0 files)
holds for that seat; this one would move two windows files the way the seat moved two darwin files. Nothing ships for
windows/arm64 today (the four RIDs are win-x64, linux-x64, osx-x64 and osx-arm64), so the change is correctness for a
host no package targets yet. An x64 build running under emulation on Windows on Arm reports X64 from
`ProcessArchitecture`, which matches Go's amd64 binary under the same emulation.

## 8. Not measured

- No arm64 host was used. Every arm64 reading is from the tree and from Go's source, as each row says.
- The tagged-pointer reading (§3) assumes the CLR's pinned objects sit below 2^48 on linux/arm64. That holds for the
  default 48-bit layout; a kernel with 52-bit user addresses gives addresses above it only on request.
- Layer 1's non-system-call packages (`math`, `hash/crc32`, `internal/bytealg`, `internal/runtime/atomic`,
  `internal/runtime/gc/scan`, `internal/runtime/sys`) were not re-read for windows; this record takes the darwin
  census's reading that keeping the amd64 selection changes nothing, which the osx-arm64 rows support on darwin only.
- Test sites in B were counted and the twelve that name amd64 or arm64 were read; the other fifty were not read one
  by one.

## 9. Commands

Toolchain: go1.26.10 with `GOTOOLCHAIN=local GOFLAGS= CGO_ENABLED=0`, on linux-x64.

- Layer 1, for each GOOS in linux, windows, darwin and each GOARCH in amd64, amd64 (control), arm64:
  `GOOS=<goos> GOARCH=<goarch> go list -tags purego,math_big_pure_go -f '{{.ImportPath}} {{join .GoFiles ","}} | {{join .TestGoFiles ","}} | {{join .XTestGoFiles ","}}' std`,
  then the two amd64 outputs compared with `cmp`.
- Layer 2, A to D: a Python script over the population and predicates of §1, run per flavour.
- E: a Go program over each flavour's amd64 list, as §5 describes.
- Supporting reads: `grep -rn 'goarch_package\.[A-Z]\|runtime_package\.GOARCH' src/core`, and the hand-own headers
  cited in each row.
<!-- {% endraw %} -->
