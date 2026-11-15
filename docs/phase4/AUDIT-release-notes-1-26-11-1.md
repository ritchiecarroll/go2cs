# AUDIT: the go.* 1.26.11.1 release notes against every change since 1.26.10.3 (R, 2026-11-14)

**What was audited.** Every commit on master's first-parent line from the `nuget-1.26.10.3` tag (`c35c0269b8`,
batch 263) to master `c2919b8b16` (batch 305): 271 commits. Each is classified USER-VISIBLE (behaviour, emission, a
flag, a refusal, the package surface, a shipped asset, tuning docs) or INTERNAL, and each user-visible one is matched
against `docs/DRAFT-news-1-26-10-4.md` and the held top entry of `docs/NEWS.md`. Every user-visible change with no
section there got one, appended to the draft in its house style with the owning seat in the heading. COORD queued
this as item 5. The 306 and 307 seats were not landed when the audit was read, so they are not in it.

**How.** The first-parent list was split into four topic groups (A-D below). Each was read by its own reader against
each merge's full commit message, the branch commits under it, and the records it cites, and each reader drafted the
sections for its gaps. R reviewed every fragment and assembled the sections, softening exact diagnostic figures per
the house style. R then classified the remaining commits, which no group named, below. A doubt raised by a reader is
carried into COORD's post, not resolved here.

**Result.** 26 sections were added to the draft. Two dated audit notes now sit beside H's existing cgo sections:
`GoCgoLibraries` is new in this release, and cgo Phase 3 increment 3.1 has landed. Two stale user pages were fixed:
KnownIssues.md's platform count, and ConsumingGoFromCSharp.md's break-floor version.

## The groups

### Group A: platform, packaging, public surface

| merge | what | USER-VISIBLE / INTERNAL | covered by, or GAP |
|:--|:--|:--|:--|
| 3d83f99d7b | SEAT 0 (C2): `GoRefuseUnshippedRid`, `GO2CS3003` error / `GO2CS3004` warning, run-time arch warning, Alpine and Ubuntu-SDK RID fixes | USER-VISIBLE (new diagnostics, build behavior) | GAP, section "A build for a platform the packages do not ship stops" |
| 00ee2dc292 | S1a (G): four-target layout rule in the converter's merge; corpus footprint empty | INTERNAL (enabling; no output change at three targets) | n/a (folded into the linux-arm64 section's provenance) |
| f9704076bb | S1b (G): `GoTargetArch` through `-tests`, the sweep and the roster tools | INTERNAL | n/a |
| 1dc059d34a | S2 (G): the linux-arm64 corpus apply (74 `linux-arm64/` files in 20 packages); fixes a pack regression where nine packages packed no build props | USER-VISIBLE as part of the flavour | GAP, section "The packages ship linux-arm64" |
| 93eb8c392d | S3 site 1 (i9): netpoll epoll numbers and event layout by arch | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| 85047f1165 | S3 sites 2-5 (i9): recvfrom/sendto, dieFromSignal, kill by the flavour's syscall numbers | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| a79e5af297 | S3 on S2 (i9): `internal/cpu` hwcap initializer, Fstat/fstatat for arm64 | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| b328dd71e7 | S3 sites 6, 8, 9 (i9): Select via pselect6, crc32 bodies via Arm.Crc32, stub triage | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| 9c95daacb6 | S3 complete (i9): netpoll admission guard lifted with a live arm64 epoll proof | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| 6c6ecc776c | S4 (C2): `osInitArm64` for linux, `getauxval(AT_HWCAP)` with Go's bit mapping | USER-VISIBLE as part of the flavour | GAP, linux-arm64 section |
| 898e6b775f | S5a (C2): opt-in os-matrix linux-arm64 leg | INTERNAL (CI) | n/a |
| c94cf818d8 | S5b (C2): leg binds `GoTargetArch`, flavour probe, pack guard | INTERNAL (CI) | n/a |
| 83789ecbcc | S6 (P2): `runtimes/linux-arm64` in every platform-split package, KnownIssues "15 members throw" | USER-VISIBLE (shipped asset) | GAP, linux-arm64 section |
| 0933af7792 | S7-b (C2): sweep-shard lifted on the arm leg, dump-suppression proof | INTERNAL (CI) | n/a |
| 51c6a3aa4f | S7-a (P2): two-RID roster grammar and guard | INTERNAL (records/guards) | n/a |
| e141a180ce | S7 wave 1 bank (P2) | INTERNAL (records), supports the validation claim | linux-arm64 section (provenance) |
| 7c7dc687f8 | S7 wave 2 bank (C1) | INTERNAL (records) | linux-arm64 section (provenance) |
| 6db3fc9dd3 | S7 wave 3 bank (C1): linux-arm64 complete, every roster row banked | INTERNAL (records), supports the validation claim | linux-arm64 section (provenance) |
| c150af6647 | P5: GolibTests linux-arm64 cgo golden and test-thread hardening (tests only) | INTERNAL | n/a |
| fe454ae807 | G: linux runtime bodies for `sched_getaffinity`, `osyield`, `exit` (linux-x64 AND linux-arm64) | USER-VISIBLE (`runtime.SetDefaultGOMAXPROCS` threw on Linux) | GAP, section "On Linux, `runtime.SetDefaultGOMAXPROCS` returns" |
| ffaafa144f | R: the go1.26.11 patch hop | USER-VISIBLE (converted net/http, net, crypto/rsa, FIPS module; the tool downloads Go 1.26.11; README badge) | PARTIAL: NEWS's lead says "the first built from Go 1.26.11, a Go patch release" and nothing more. GAP, section "Converted from Go 1.26.11" |
| 1e654296e5 | C1: the public-surface contract page, `[EditorBrowsable(Never)]` on golib tier 2 and generated plumbing, golib baseline, OQ5 guard, source-compat release-smoke arm | USER-VISIBLE (new doc page; IntelliSense) | GAP, section "What go2cs promises to C# code" |
| 9db360e89a | COORD: docs/PLAN-public-surface-stability.md, owner-ruled | INTERNAL (plan record; its visible outcomes are 1e654296e5 and the OQ4 rename, already covered) | n/a |
| 4167bfbcc2 | C2: `IByteSeq<T>.TryGetSlice/TryGetString` get default bodies, so a .3-built implementer still loads | INTERNAL (keeps binary compatibility; nothing for a user to do, and the break floor makes everyone rebuild anyway) | n/a |
| eef13193a9 | C2: 17 stale .3 compatibility suppressions deleted | INTERNAL | n/a |
| f7e140e982 | P2: the build-time break floor, `GoCheckBreakFloor`, `GO2CS3002`, `GoAllowBelowBreakFloor` | USER-VISIBLE (new build check and opt-out) | PARTIAL: NEWS's two Upgrading bullets name `GO2CS3002` as the refusal for their own breaks but never introduce the check, what it reads, or the opt-out. GAP, section "The break floor" |
| b6f3bbb44f | C2: I0-lib, `go.arrays.dll` (`array1<T>`..`array4096<T>`), packed in go.lib | USER-VISIBLE (new assembly in the go.lib package; no converter change at this merge) | GAP, section "A struct read from native memory holds its arrays as Go does" |
| 817b56e8b0 | C2: I0 part D, automatic admission, `-inline-arrays`, `GO2CS2009`/`GO2CS2010`, emission | USER-VISIBLE (emission, flag, diagnostics) | GAP, same section |
| 555e30c826 | C2: mixed reinterpret pairs kept out under the automatic rule | USER-VISIBLE only as part of the same feature: it fixes a refusal that 817b56e8b0 introduced in this same release window (see doubts) | same section |
| a713a73c06 | P2: the tool's Go download retries its final rename on Windows | USER-VISIBLE (a sporadic first-use failure on Windows) | GAP, section "The first-use Go download survives a virus scanner" |
| 742a3dbb24 | P2: CI pre-release packs versioned as the next build (`-ci.N`) | INTERNAL (CI packs only) | n/a |

### Group B: runtime correctness and performance

| merge | what | USER-VISIBLE / INTERNAL | covered by, or GAP |
|:--|:--|:--|:--|
| f4b803dfd8 | P5+H: the interior-pointer root. `(*T)(p)` goes through `unsafe.Pointer.To<T>`, one-expression `uintptr` arithmetic becomes `unsafe.Add`, a receiver's box at a use known at the node, array-field windows keep their root; plus `StoreFunc`, `Add(nil, n) = n`, `Pointer.Equals`/`GetHashCode` in token space; 160 corpus files | USER-VISIBLE (GC-safety hole in plain Go closed) | Partly: the draft's two P5 sections cover `StoreFunc` and `Add(nil, n)`. The root itself is a **GAP**: section 2.1 |
| a6cefba6ee | H: a call returning `unsafe.Pointer` keeps its box (the `(uintptr)` prefix dropped; `unsafe.Add` carries); 108 corpus files | USER-VISIBLE (silent heap corruption fixed) | **GAP**: folded into 2.1 |
| 269f475408 | P2: `unsafe.Pointer` of a box-holding operand (a local copy of the receiver, a field, a local of a local) inside a pointer-receiver method wrote to the local | USER-VISIBLE (silent wrong answer) | **GAP**: 2.2 |
| 4fe1cb992c | H: five hand-owned crypt32 calls (Windows cert chain) hold their box with `GC.KeepAlive` | USER-VISIBLE only on invalid input (the record: reachable only where crypt32 rejects the input under Go too) | Not drafted: too narrow to announce; see doubts |
| 03902a7f0f | H: the converted testing package's 64 KiB per-record cap keeps a 4 KiB tail with a counted marker | INTERNAL (the `-tests` results record) | n/a |
| c6fd06311c | G: reflect through an interface shell (value adapter, object shell, error): crashes, silent wrong answers (`Len` 0, empty `%v`/`MapKeys`/`MapRange`), spurious panics; `sort.Slice` Swapper | USER-VISIBLE | **GAP**: 2.4 |
| c64f8404c8 | G: the runtime profiler self-deadlock (block/mutex event on a new stack re-entered `stkbucket` under its own lock) | USER-VISIBLE (a profiled program could hang) | **GAP**: 2.6 |
| 01986cdd81 | P2: after `all goroutines are asleep - deadlock!`, each goroutine's block (reason, frames, created-by), once, exit 2 | USER-VISIBLE (output text) | **GAP**: 2.5. Limitations.md and GoFeatureSupport.md already say it; no release text does |
| 456b550a46 | C1: every fatal report also reaches `debug.SetCrashOutput`'s file (a non-Go exception escaping a goroutine, its fallback, the deadlock report) | USER-VISIBLE (output) | **GAP**: 2.5 |
| 40b021f8c4 | G: `runtime.FuncForPC` answers one `*Func` per Go function, the one `Frames.Next` sets; `Entry()` per function | USER-VISIBLE (small) | **GAP**: 2.6 |
| 26186c1739 | G: `runtime.GOARCH` (and `internal/buildcfg`'s default) read the process architecture on the darwin and windows flavours | USER-VISIBLE (Apple silicon reads `arm64`) | **GAP**: 2.6. The 1.26.10.3 notes called it planned work; KnownIssues.md is already updated |
| 813f55b5e8 | i9 (C44): on Windows `os.Args` under `dotnet prog.dll` no longer includes the muxer's prefix | USER-VISIBLE | **GAP**: 2.6 |
| 872df69010 | C1: darwin/linux `recvfrom`/`recvmsg`: a zero-length datagram with an unspecified source reads as Go's zero address, not `EAFNOSUPPORT` | USER-VISIBLE | **GAP**: 2.6 |
| 58ef70b456 | H: `TestDecorateMappings`: `setVMANameSupported` probes once, as Go does (linux) | INTERNAL (the record: Go's check only logs; no user effect stated) | n/a |
| 6ea085d498 | C2: `sync.Mutex.Lock` tries the lock first (uncontended 321.7 -> 66.4 ns, 64 -> 0 B) | USER-VISIBLE (performance) | **GAP**: 2.7 |
| 0e589cd686 | C2: `internal/runtime/atomic`'s 8-bit family lock-free; a `Store8` can no longer be lost | INTERNAL (callers are the converted runtime's own heap code; no user symptom recorded) | n/a; see doubts |
| 2db66518be | P2 (C51): `==` on pointers to struct fields, 5 hops 716 -> 6.0 ns; plus distinct allocations whose ids collide no longer compare equal | USER-VISIBLE (performance, one rare correctness fix) | **GAP**: 2.7 |
| 08944002b7 | P4 S2-S4: utf8/fmt twin copies, named map/chan `== nil` without reflection, struct equality and hashing without boxing (generated structs implement `IEquatable<T>`) | USER-VISIBLE (performance; one C# surface addition) | **GAP**: 2.7 |
| d9d52c0abd | P5 B3 A2: `error<T>` looks up its pointer `Error` in its own type's homes (start-up), and binds a generic type's pointer `Error` in every load order | USER-VISIBLE (start-up time; one correctness fix) | **GAP**: 2.4 and 2.7 |
| c78f162712 | G N1: a typed nil func in interface space is one value (type assertion, dynamic type, panic text, reflect `Call`) | USER-VISIBLE | **GAP**: 2.3 |
| 9c77232c27 | G N2: a type switch case with a func type matches a typed nil func instead of falling to `default` | USER-VISIBLE (silent wrong answer) | **GAP**: 2.3 |
| 0430770b00 | C2 (C49): one nil per pointer type through a type argument (generic `==`, map nil keys, array equality) | USER-VISIBLE | **GAP**: 2.3 |
| 847c629b31 | C2 (C47): a nil func passed to a slot of a named func type stays nil (it threw) | USER-VISIBLE (crash fixed) | **GAP**: 2.3 |
| c737e7760a | C1 (C36): a float converted to an integer type parameter (`T(f)`, `T ~int32`) compiles and answers as gc does per architecture | USER-VISIBLE (did not compile) | **GAP**: 2.4 |
| 33faf4bba8 | H: the native-call seam S-a..S-d (Windows native calls enter the runtime's entersyscall family; NewCallback re-entry; foreign-thread callback identity; guard) | INTERNAL (the design: behaviour-neutral under today's thread-per-goroutine model; groundwork for M:N; cost at most +4.2% of a bare P/Invoke) | n/a; see doubts |
| 83169c3f10 | G F-1: linux `runtime_entersyscall`/`exitsyscall` forward to the seam | INTERNAL (seam) | n/a |
| 772e2a7679 | G S-b2 linux: `cgocaller` brackets its shim call; `Setgroups` goes through it | INTERNAL (seam) | n/a |
| f3d21fdbf0 | H S-b2 darwin: `posix_spawn`, `kill`, `readdir_r` bracketed | INTERNAL (seam) | n/a |
| 58648c8d89 | G: the `-tests` harness ports cmd/go's `StartSigHandlers` (unix): children start SIGINT/SIGQUIT at default; Ctrl+C kills every child's process group; exit 130 | USER-VISIBLE (converter CLI, `-tests` users on unix) | **GAP**: 2.8. Tooling, not runtime: move it to the tooling fragment if one exists |
| fb885891b3 | C1: darwin GolibTests rooted. Two Go-parity fixes (darwin/arm64 nine-argument libc call; macOS timer wait through kqueue) plus the macOS CPU sampler's per-thread CPU through mach | USER-VISIBLE (macOS timer precision, macOS CPU profiles; one arm64 call) | **GAP**: 2.6 |

### Group C: converter, command line, refusals, emission

| merge | what | USER-VISIBLE / INTERNAL | covered by, or GAP |
|:--|:--|:--|:--|
| bf488d1296 | P1: a defined type over an instantiated generic struct (`type Q pair[int]`, `type P[T any] pair[T]`) converts; go2cs-gen substitutes the type arguments in forwarded members | USER-VISIBLE | GAP -> 2.1 |
| afbcc32974 | P1: the same through a local or cross-package alias chain | USER-VISIBLE | GAP -> 2.1 |
| 596a66cc89 | docs: restores DiagnosticCodes.md's GO2CS2004 "Retired" row a merge dropped | INTERNAL | n/a (2.1 cites the row) |
| 3362252ca8 | R: new `-trimpath`, on by default: `-recurse` of a local module records `<module>@/<path>`, stack traces print `<module>/<path>` | USER-VISIBLE (new flag, emission changes) | GAP -> 2.2 |
| d914d8b360 | R: tool tags (`goexperiment.*`, `amd64.v1`) resolved for the `-platforms` target, not the host; darwin corpus gains `internal/goexperiment/exp_dwarf5_off.cs` | USER-VISIBLE (minor: cross-target conversions) | GAP -> 2.4 |
| ae69e4ce74 | P2: `-provenance` (existing flag) now writes a two-line notice with module@version and a "Modified from the original" line, after the upstream license comments; nugetgo-pack turns it on for Apache-2.0 | USER-VISIBLE (changed flag output) | GAP -> 2.2 |
| e85dc60698 | P2: a module's `package main` keeps its own `package_info.cs` on reconvert (was replaced by the template, with a false warning) | USER-VISIBLE (minor) | GAP -> 2.4 |
| a2bcf97a69 | C2: a file the converter faults on fails the run (`GO2CS2008`), where it was dropped with exit 0 | USER-VISIBLE (exit code, new code) | GAP -> 2.3 |
| 4336f0e2aa | P2: `go2cs -h` text of the experimental `-dual-recv` flags; CI comments | INTERNAL (help text only) | no note needed |
| 2663620296 | i9 C1: a value passed to another package's anonymous-interface parameter | USER-VISIBLE | GAP -> 2.5 |
| b5e99e005e | i9 C3: variadic method promoted from an embedded interface gets its forwarder (silent wrong reflect answer fixed) | USER-VISIBLE | GAP -> 2.5 |
| f3ce8a0fc9 | i9 C4: `x == nil` on a pointer-union type parameter | USER-VISIBLE | GAP -> 2.5 |
| ef137b6ae8 | P1 C5: a module's import of a path std vendors resolves to the module | USER-VISIBLE | GAP -> 2.5 |
| c021114195 | P2 C8: value receiver calling a boxed-receiver pointer method | USER-VISIBLE | GAP -> 2.5 ("and others") |
| d0f9d63ca1 | P2 C9: `:=` variadic func literal used as a value | USER-VISIBLE | GAP -> 2.5 |
| 248966d5e8 | P1 C10: named pointer type converted at a call | USER-VISIBLE | GAP -> 2.5 |
| 240a4dea0e | H C11: keyed literal of a named struct defined over a foreign struct | USER-VISIBLE | GAP -> 2.5 ("and others") |
| b6a3bc13ff | i9 C15: type-switch binding rename collision | USER-VISIBLE | GAP -> 2.5 ("and others") |
| 60b13275fb | H C17: promotion behind a pointer at any depth; GO2CS2004 introduced (later retired) | USER-VISIBLE | GAP -> 2.5 / 2.1 |
| 24973ed469, 00bc75369f | R C6/C12: a named string type is ordered, rangeable, convertible from `[]byte`/`[]rune`, compares with literals | USER-VISIBLE | GAP -> 2.5 |
| c5a21d17c6 | R C16: a named `[]byte` satisfies `~[]byte \| ~string` | USER-VISIBLE | GAP -> 2.5 |
| d345469a7d | P1 C18: named pointer locals/fields index, len, range, field select | USER-VISIBLE | GAP -> 2.5 |
| e0468a5309 | C1 C19: `&^` with a constant on a `uintptr` alias | USER-VISIBLE | GAP -> 2.5 |
| eeecc94d3b | R C22: a type renamed for a same-name method, referenced package-qualified from another package | USER-VISIBLE | GAP -> 2.5 |
| d613a0c234 | R C22e: the same through another package's alias re-export | USER-VISIBLE | GAP -> 2.5 |
| ee440ccf74 | C1 C24: type parameter with core type `[]E` / `map[K]V` used as that type | USER-VISIBLE | GAP -> 2.5 |
| 7dcbfb74b4 | P1 C28: `for` init/post re-pointing a pointer parameter (two silent wrong results fixed) | USER-VISIBLE | GAP -> 2.5 |
| 5b86e76ccb | P1 C29: method promoted through more than one embedding level | USER-VISIBLE | GAP -> 2.5 |
| 2563801e0f | C1 C30: `switch` on an array value with composite-literal cases | USER-VISIBLE | GAP -> 2.5 |
| 52cde97548, 48f71f237c | C2 C31: `map[K]V` type argument rendered once (was `map<@string, @string, >`) | USER-VISIBLE | GAP -> 2.5 ("and others") |
| 0804004099 | R C32: conversion to a named `[]byte` through an alias | USER-VISIBLE | GAP -> 2.5 ("and others") |
| 15f1b979b2 | C2 C33: two instantiations of a foreign generic behind one interface | USER-VISIBLE | GAP -> 2.5 |
| d770d97945 | R C35: pointer type argument to a non-self-referential method-set constraint | USER-VISIBLE | partly: the draft's GO2CS2005 section mentions "the conversions that now work for that shape"; listed in 2.5 |
| b5ffdb166a | C2 C37: lifted anonymous variadic element package-qualified | USER-VISIBLE | GAP -> 2.5 ("and others") |
| 8d19228a0c | C1 C38: parenthesized type-switch case `case (*ast.Node):` | USER-VISIBLE | GAP -> 2.5 |
| e240c5e815 | R C39: a func passed to a type-parameter argument takes no `any` carrier | USER-VISIBLE | GAP -> 2.5 ("and others") |
| ad3fd922bf | C2 C40: a package func and a method with the same name | USER-VISIBLE | GAP -> 2.5 |
| 85c829ef0e | C1 C41: typed `var` initialized from a ref-aliased pointer parameter | USER-VISIBLE | GAP -> 2.5 ("and others") |
| 08e00f6f9f | G C42: interface taking one method from two embedded interfaces (CS0121) | USER-VISIBLE | GAP -> 2.5 |
| d386315daa | C1 C45: unsafe-free fork of zeebo/xxh3 v1.1.0 at `src/forks/`, for a `replace` | USER-VISIBLE (a user action) | GAP -> 2.6 (Limitations.md already says it; the draft's C43 section does not) |
| 159bc9e9bc | NOT a first-parent merge and not S1: a GolibTests change (the Parked predicate, mutex-event test) | INTERNAL | n/a; S1's merge is 159f08e523 |
| 159f08e523 | G S1: a C function pointer carried as a `uintptr` (ccgo's `__ccgo_fp` and its recovery) takes one word per static function | USER-VISIBLE | GAP -> 2.7 (the draft's P5 section covers only the w3 store at a computed address) |
| 811f3fdb3c | P2 S7 (a declared type named like a C# builtin, `float`, keeps its name) + S4 (`type fermat nat; type nat []uint64`) | USER-VISIBLE | GAP -> 2.7 |
| adfcea58f6 | P2 S13 (terminating switch, CS0161), S14 (tuple target `*(*T)(x)`, CS0131), S6 (integer to named string, CS0030) | USER-VISIBLE | GAP -> 2.7 |
| 975aa3ac83 | P2 S17 (store through `(*(*T)(p)).F`, `(*(*[N]T)(p))[i]`), S20+S5 (pointer-cast slice view), three-index cap | USER-VISIBLE | GAP -> 2.7 |
| a7e363527f | P2 S18: pointer-cast slice with a non-ASCII element type name | USER-VISIBLE | GAP -> 2.7 ("and others") |
| 65404e5e63 | P2 3b: `(*int8)(unsafe.Pointer((*int32)(up)))` (CS1929) | USER-VISIBLE | GAP -> 2.7 |
| 839fd3634b | G S21: x/sys/cpu `cpuid`/`xgetbv` forward to golib `GoCpu`; new GO2CS2011 (refusal), GO2CS2012 (warning) | USER-VISIBLE | GAP -> 2.7 (bold paragraph) |
| 314e1d8b60 | G: GoCpu CPUID test masks the APIC ID | INTERNAL (test only) | n/a |
| 1f2d0748ba | G: BOARD row, sqlite-cs demo reaches none of GO2CS2012's 8 stubs | INTERNAL (docs) | n/a |
| 2402236781 | P1 S23/M5: anonymous struct identical to an imported package's public lift reuses it | USER-VISIBLE | GAP -> 2.7 ("and others") |
| 38d917d0be | P1 M1-M4: keyword-named alias, foreign alias spelling, anonymous-struct alias across files/packages | USER-VISIBLE | GAP -> 2.7 |
| 3da19d0e2e | G: one instantiation of a same-package generic as a non-generic interface naming the type argument (CS0029); GO2CS2007 warning for three uses; reflect `Value.Field` through an interface shell | USER-VISIBLE | GAP -> 2.7 |
| 71a08e8f9b | C1: `copy` through a byte view of a struct holding a fixed array (syscall's dirent idiom) | USER-VISIBLE | GAP -> 2.7 |
| 556ffd2c1d | C2: `-tests` alias matcher on a namespace segment boundary (inert test ProjectReference lines dropped) | INTERNAL | n/a |
| a97aa209ff | C2: fixes a strings DeltaReplacer regression introduced after .3 by C22e (never shipped) | INTERNAL | n/a |

### Group D: cgo Phases 1-2, docs and tuning

| merge | what | USER-VISIBLE / INTERNAL | covered by, or GAP |
|---|---|---|---|
| `d25c05efd2` | cgo gate: a package whose every Go file needs cgo is refused (`GO2CS2001`) instead of converting to nothing with exit 0 | USER-VISIBLE | GAP, in "cgo: `go2cs -cgo` converts a package that calls C (H)" |
| `80c18e6562` | PLAN-cgo-interop section 11 re-ground, native-call seam design (docs only) | INTERNAL | none needed |
| `18faa6d239` | Phase 1 inc. 1: recognizer of cmd/cgo's synthetic names, wired into nothing | INTERNAL | base of the cgo GAP section |
| `b07fd14562` | Phase 1 inc. 2: `-cgo` loads cgo-on and recognizes, still refuses; converter-cgo CI stage | INTERNAL (superseded by 5a/5b) | base of the cgo GAP section |
| `1fc911cbc3` | Phase 1 inc. 0: the three remaining synthetic-name kinds | INTERNAL | base of the cgo GAP section |
| `9a85aea9c6` | Phase 2 inc. 1, test only: `go list` checks `#cgo LDFLAGS` but not the environment's `CGO_LDFLAGS` | INTERNAL | none needed |
| `a7e19fafd9` | Phase 2 inc. 2: C struct mirrors from go/types, layout checked; flexible arrays and complex refused by name | USER-VISIBLE (through the route) | GAP, cgo section |
| `6075a10aeb` | Phase 2 inc. 3a: golib `GoCgoArgs`, pointer classification; `&xs[1]` and reinterpreted boxes refused | USER-VISIBLE (through the route) | GAP, cgo section |
| `ddb57f9308` | Phase 2 inc. 3b1: scalar wrappers (`[LibraryImport]`, cdecl), the errno form | USER-VISIBLE (through the route) | GAP, cgo section |
| `692b9d6f84` | test harness: the mirror dotnet arm skips/fails on a missing .NET 10 SDK | INTERNAL | none needed |
| `2e9096000e` | Phase 2 inc. 3b2a: structs by value and by pointer (stack mirror, copied back) | USER-VISIBLE (through the route) | GAP, cgo section |
| `0c3ea9fb42` | correction to 3b2a (class modifiers, `AllowUnsafeBlocks` per package) | INTERNAL (fix inside unreleased work) | none needed |
| `44fad66dde` | Phase 2 inc. 3d: C types, typedefs, constants as `C.<name>`; a declaration mixing C and Go names refused | USER-VISIBLE | GAP, cgo section |
| `86ba10b003` | Phase 2 inc. 3b2b: pointer members, a single scalar pointee | USER-VISIBLE (through the route) | GAP, cgo section |
| `816009e491` | Phase 2 inc. 4a/4b: golib `GoCgoLibraries.Register` (new class), per-symbol load-name capture; static symbols refused | USER-VISIBLE | GAP, cgo section. Note: the draft's "golib: `GoCgoLibraries.Register` gains a target (H)" assumes `Register` shipped before; it did not (see doubts 2) |
| `a7510bfadf` | 4b follow-up, test only: `-O3`/`-g3` refused; windows CRT capture under gcc and zig | INTERNAL | none needed |
| `ea56e6ec40` | Phase 2 inc. 4c: emitted resolver table and `[ModuleInitializer]` registration | USER-VISIBLE (through the route) | GAP, cgo section |
| `118cfbc77b` | Phase 2 inc. 4d: probe library and exit-gate arms; darwin bare-name dyld load reached the CWD, fixed before any release | USER-VISIBLE (the never-the-CWD property) | GAP, cgo section (no separate security note: no release shipped the hole) |
| `b7f47fd8c6` | Phase 2 real route (5a driver, 5b capture via `go env CC`, host-target refusal, golib target check) | USER-VISIBLE: this is what makes `-cgo` work | GAP, cgo section |
| `4490a52244` | real-route test resolves a bare CC as the route does, test only | INTERNAL | none needed |
| `72cb06355b` | G1 gate: a converted cgo program builds, binds and runs like go on linux-x64, osx-x64, osx-arm64, win-x64; two converter/gen fixes | USER-VISIBLE (the platform claim) | GAP, cgo section |
| `4423d66052` | Phase 3 step 3.0: a plain `//go:build cgo` file kept under any cgo-on load; a clean link-error message; MSBuild refuses a cgo build for another target; target hand-down | USER-VISIBLE | GAP, cgo section |
| `9a3a6e6aa0` | the target hand-down fixed for darwin/arm64 (red on master 292 to 294, never released) | INTERNAL (regression fix inside unreleased work) | covered by the cgo section's darwin/arm64 claim |
| `c150af6647` | GolibTests linux-arm64 cgo golden + thread hardening | INTERNAL | none needed (no linux/arm64 end-to-end claim made) |
| `95c6348154`, `c66714e422`, `c4b47c4f61` | PLAN 5.1 owner rulings: auto-provision a pinned C toolchain; zig default; then per OS (zig linux/darwin, llvm-mingw windows) | INTERNAL today: the pins live only in CI (`.github/cgo-toolchain.ps1`); the converter provisions no C compiler | not announced (see doubts 5) |
| `89f98dc37f` | SCOUT-cgo-c-toolchain; converter-cgo `;cc=zig\|llvm-mingw` CI option | INTERNAL | none needed |
| `fd2b6f5331` | docs/Tuning.md: server GC and DATAS-off as a documented option | USER-VISIBLE (advice) | GAP, in "Tuning: start-up against steady state, and server GC (G, P4)" |
| `6996aafb79` | call-counting delay measured; Tuning.md's start-up-or-steady-state guide; the default stays 0 | USER-VISIBLE (advice) | GAP, Tuning section |
| `48665b310f` | docs/GoFeatureSupport.md, the Go feature support matrix (new page) | USER-VISIBLE | GAP, in "Documentation (COORD)" |
| `e02f6729d0` | GoFeatureSupport status icons, non-wrapping headers | USER-VISIBLE (cosmetic) | folded into the Documentation section's page line |
| `c978595f0c` | CODE_OF_CONDUCT.md (Contributor Covenant 2.1) and a pull-request template | USER-VISIBLE (contributors) | GAP, Documentation section (one clause) |
| `6869ec2b54` | the converted-by-go2cs badge on go2cs.net, usage in ConvertingAModule.md | USER-VISIBLE | GAP, Documentation section |
| `8792630525` | the nugetgo README template opens its generated block with the badge | USER-VISIBLE (anyone packing with the nugetgo tooling) | GAP, Documentation section |
| `bce71f3ae9` | docs audit fixes across the user pages; CommandLine.md lists every flag `-h` prints (guarded) | USER-VISIBLE | GAP, Documentation section (one sentence, no counts) |
| `0bbe3ab6e5` | docs/README.md badge link-text wrap fix | INTERNAL (cosmetic) | none needed |
| `67b2954b01` | docs/README.md drops the Explore bullet for Tuning | INTERNAL (cosmetic) | none needed |
| `c864b69655` | SCOUT-ts-perf.md, docs/phase4 only | INTERNAL (it is the source of the server-GC measurement) | none needed |
| `b1c020c19c` | mousetrap gets a repack row; the driver converts it with `-provenance` | INTERNAL (tooling for the nugetgo rebuild wave; mousetrap is already published, not a new package) | the wave is covered by NEWS's "Rebuild what you built against 1.26.10.3" bullet |

## The remaining 114 commits (no group named them)

| commit | subject | verdict |
|:--|:--|:--|
| c2919b8b16 | batch 305: re-cite three provenance notes the union moved (conversionDriver.go +1 from C2's seat; slice.cs +16 from the interior-pointer seat) | INTERNAL: tree maintenance (re-cites, union fixes) |
| 651c4ad147 | batch 300: merge claude/c1-i0-partd-review (48dd358744) -- C1 (docs/data): I0 part D review evidence (three read-only go/packages censuses with contro… | INTERNAL: records, designs, BOARD rows, banks |
| 6dc4bae09b | batch 299 union fix (COORD): DESIGN-sqlite-cs-funcptr.md:132-133 rewrapped so the code span starts its line | INTERNAL: tree maintenance (re-cites, union fixes) |
| dc1635b435 | batch 299: merge claude/c1-sqlite-funcptr-design (99fa87d969) -- C1 (docs): DESIGN-sqlite-cs-funcptr.md -- sqlite-cs S1 (C function pointers as number… | INTERNAL: records, designs, BOARD rows, banks |
| 93db197fac | batch 298: merge claude/c1-multivalue-spread-scout (9fbcab452e) -- C1 (docs): SCOUT-tour-visual-parity.md -- the owner's four Tour visual-parity items… | INTERNAL: records, designs, BOARD rows, banks |
| fb55c8d872 | batch 297: merge claude/c1-inlinearray-scout (0f00910159) -- C1 (docs): DESIGN-array-value-storage.md 7.7 dated lines -- the 04:00Z auto-detect ruling… | INTERNAL: records, designs, BOARD rows, banks |
| b18a2501c9 | batch 294: merge claude/c1-inlinearray-scout (edb10de59f) -- C1 (docs): the [InlineArray] design, DESIGN-array-value-storage.md section 7 (scout, the … | INTERNAL: records, designs, BOARD rows, banks |
| bd59ee0003 | batch 293: merge claude/p1-tests-host-name (6a557445ae) -- P1: the -tests pipeline finds the published test host by its declared AssemblyName (one hel… | INTERNAL: -tests pipeline tooling (contributor validation) |
| 130cecb136 | batch 292: merge claude/g-generic-inst-iface-board-row (d2afbe6551) -- G (docs): BOARD row (OPEN): one instantiation of a local generic type assigned … | INTERNAL: records, designs, BOARD rows, banks |
| 77d7e99ba6 | batch 291: merge claude/g-defer-callers-line (7dfd0e396e) -- G: BOARD row, runtime.Callers from a deferred function with a panic in flight reads the d… | INTERNAL: records, designs, BOARD rows, banks |
| c7d55b055d | batch 289: merge claude/g-runner-notcompared-board-row (393b88b47b) -- G: BOARD row, the behavioral runner prints PASS for a program without [GoTestMa… | INTERNAL: records, designs, BOARD rows, banks |
| 9c1e3f78cc | batch 289: merge claude/g-deadlock-board-row (3bbb78bfbf) -- G: BOARD row, a deadlock exit prints the fatal line but not Go's goroutine blocks (measur… | INTERNAL: records, designs, BOARD rows, banks |
| 01de383545 | batch 289: merge claude/r-oq4-assembly-rename (04bde47ae9) -- R, OQ4: std assemblies renamed to go.<pkg>.dll (package IDs frozen); converter linkname … | covered by draft: "Standard-library assemblies are named after their packages (R)"; NEWS held entry |
| 80631fa9f7 | batch 288: merge claude/h-cgo-phase3-scout (ec0a5b43a0) -- H: RECON-cgo-phase3-target.md, the Phase 3 proof-target scout with the COORD and owner ruli… | INTERNAL: records, designs, BOARD rows, banks |
| 9cc459f618 | batch 288 union fix (COORD): re-cite five ConversionStrategies provenance notes after the public-surface merge | INTERNAL: tree maintenance (re-cites, union fixes) |
| 3e1c9b2b77 | batch 288: merge claude/g-mn-spike2-review2 (b58df67d4a) -- G: M:N spike record (sections 8-10: final answer, review folds) + public-surface plan date… | INTERNAL: records, designs, BOARD rows, banks |
| 4efe149858 | batch 287 union fix (COORD): two Windows-checkout reds from merged seats | INTERNAL: tree maintenance (re-cites, union fixes) |
| 1432a8b9dd | batch 287: merge claude/c1-subtle-dit-linux (76fc096ed3) -- C1: crypto/subtle DIT disclosures extended darwin -> darwin+linux (linux-arm64 clears HasD… | INTERNAL: validation disclosures (records) |
| f7abff0d7a | batch 287: merge claude/i7-ts-readiness-rerun (05be9bc2fd) -- TS readiness records (docs/phase4 only): the full type-check pass and P4 handoff | INTERNAL: records, designs, BOARD rows, banks |
| c309a29daf | batch 287: merge claude/i9-darwin-timing-bars (c77070d5ac) -- i9: darwin timing rows on paired host-relative bars (the hosted mac slow mode) | INTERNAL: test-only (darwin timing bars) |
| 4c03b7779e | batch 287: merge claude/h-cs0219-fixed-point (6de7a99fe4) -- H: behavioral .editorconfig warning entries are every target's fixed point (CgoPseudoPack… | INTERNAL: CI, guards, instruments, hygiene |
| 7f529b104d | batch 287: merge claude/p2-s7-sweep-timeout (a31cb64783) -- P2: sweep-shard per-row timeout=Nm filter option; internal/zstd TestAlloc pin darwin+linux | INTERNAL: CI, guards, instruments, hygiene |
| 63722d3e17 | batch 287: merge claude/r-gofmt-sweep (ca0713e5bf) -- R: gofmt sweep (30 files) + TestConverterSourcesAreGofmtClean; CNR byte-identical | INTERNAL: CI, guards, instruments, hygiene |
| 06610aaa65 | batch 286: merge claude/c1-b286-main-entry (0716cfe76a) -- C1: MainEntryGenerator finds the main package in any go namespace. Dotted module paths emit… | covered by draft: the C46 section (the entry generator finds a main package in any namespace) |
| b84248ee91 | go2cs: aliasReceiverForm_test tolerates CRLF checkouts (batch 286 union fix) | INTERNAL: tree maintenance (re-cites, union fixes) |
| 0894a4aa68 | behavioral: regenerate four projects added before C46 with the batch 286 union's converter (Main() -> main()) | covered by the C46 section (behavioral goldens regenerated) |
| c7cb617d5b | docs: re-cite 36 unfingerprinted line cites moved by the batch 286 union | INTERNAL: tree maintenance (re-cites, union fixes) |
| 4c77723ed1 | batch 286: merge claude/i7-ts-readiness-rerun (a101ea060e) -- TS readiness records (docs/phase4 only): the run-ahead readings, the full type-check pas… | INTERNAL: records, designs, BOARD rows, banks |
| a023a0d54c | batch 286: merge claude/p1-c48-alias-receiver (47069bdd0e) -- P1, TS readiness C48 part A: a pointer method whose receiver is written through a type a… | covered by NEWS held entry: C48, alias receivers |
| 28f475c260 | batch 286: merge claude/g-s7-instrument (1d1f329eb0) -- G: the S7 wave 1 instrument (wave1.sh, the seven shard filters, filters.py, README) for P2 | INTERNAL: CI, guards, instruments, hygiene |
| 93eaea5247 | batch 286: merge claude/i9-gosched-measure (5cf8435159) -- i9: on macOS EffectiveGoschedYieldsStayCheap bars Gosched against the host's own raw Thread… | INTERNAL: records, designs, BOARD rows, banks |
| b9bfffc665 | batch 286: merge claude/c2-widening-write-refusal (b334a29fa9) -- C2 + R, one seat: GO2CS2006 refuses an element-wise []N widening the callee may writ… | covered by draft: "A new refusal: GO2CS2006 (C2)" |
| fe1b17797c | batch 286: merge claude/c1-c46-main-stack (7ade528472) -- C1, TS readiness C46: Go's func main is emitted as main() and go2cs-gen generates the entry,… | covered by draft: "main keeps its name, and runs with Go's stack (C46)" |
| 113c6d69a9 | batch 286: merge claude/p1-method-cap (466c074ad6) -- P1: the method-cap seat's footprint record (0 files on four targets) and Native AOT on the merge… | covered by NEWS held entry: the method cap (its footprint record) |
| fecfd5b220 | docs: correct the batch 285 re-cite (2cfd155b72), which shifted 20 already-current cites a second time | INTERNAL: tree maintenance (re-cites, union fixes) |
| 2cfd155b72 | docs: re-cite 82 unfingerprinted line cites moved by the batch 285 union (16 files) | INTERNAL: tree maintenance (re-cites, union fixes) |
| fb282cf1ea | batch 285: merge claude/g-la64-s7-r3-triage (0d3cb6b72b) -- G, linux/arm64 S7: the E1 entry checklist, A-0 (the dump-suppression proof green and made … | INTERNAL: the linux-arm64 bank process (records and instruments) |
| e1f47dcb77 | batch 285: merge claude/i7-ts-readiness-rerun (e5c5ac5992) -- TS readiness (docs/phase4 record): the 284 + P1 preview readings. The whole type-check c… | INTERNAL: records, designs, BOARD rows, banks |
| 443822b10d | batch 285: merge claude/p2-c43-recurse-tags (fb769dab94) -- P2, TS readiness C43: -recurse's default third-party tags become safe,purego,noasm (printe… | covered by draft: "Converting a module: default build tags (C43)" |
| 29b18dc3ab | batch 285: merge claude/p2-la64-s7a-fixups (fbb9169c42) -- P2: S7-a roster grammar fixups (G's three follow-ups and the smaller items; 8 host-differen… | INTERNAL: roster grammar and guards |
| f496cc2316 | batch 285: merge claude/r-board-widening (b52d585d31) -- R: BOARD row, the []N element-wise widening silently copies a written slice (reverse() prints… | INTERNAL: records, designs, BOARD rows, banks |
| 3f6d5d3db3 | batch 285: merge claude/c2-keepalive-census-nativeidentity (6dd0c29b11) -- C2: the KeepAlive census guard's arm 2 reads nativeIdentityOf(Ex) and a bar… | INTERNAL: records, designs, BOARD rows, banks |
| f9574d2385 | batch 285: merge claude/g-timer-paired-bar (c0ebfffe10) -- G, remedy A for the darwin/osx-arm64 timer red: on macOS AFiringIsCloseToItsDueTime bars Go… | INTERNAL: test-only (darwin timer bar) |
| e92f2ebffb | batch 285: merge claude/g-classic-partial-erase (20a3b6d6fe) -- G: the per-flavour census guard (TestFlavourOnlyStubCensus; found two masked throwing … | INTERNAL: generator (classic partials left to the compiler; 4 stub outputs gone, corpus unchanged) and a census guard |
| a2d59a5392 | batch 285: merge claude/p1-method-cap (c05dd26ae1) -- P1, TS readiness: a package class holds at most 65,521 methods; promoted forwarders over the cap… | covered by NEWS held entry: the 65,521 method cap and GO2CS0012 |
| 50dd9a065c | runtime (linux): keep GoSysMmapProbe's 1.26.10.3 signature; the page-size form is GoSysMmapPagedProbe (batch 284 union fix) | INTERNAL: tree maintenance (re-cites, union fixes) |
| 35cb1fd80e | converter: rename C35's mentionsTypeParam to mentionsTypeParamOutsideInterface (batch 284 union fix) | INTERNAL: tree maintenance (re-cites, union fixes) |
| d6bf1960c9 | batch 284 (docs): merge claude/h-cgo-phase2-design (e160c7932a) -- H: the dated 11.12 MANDATORY line (dyld's bare-name search reached the CWD on both … | INTERNAL: records, designs, BOARD rows, banks |
| 1ac12080d7 | batch 284 (docs): merge claude/i9-s3-s2-sites (cbd2a61a05) -- i9: the S3 census amendment carries the complete per-flavour table: all 57 bodyless part… | INTERNAL: records, designs, BOARD rows, banks |
| bed6916024 | batch 284 (docs): merge claude/h-cgo-phase2-design (a60aefa884) -- H: the dated 11.10 line, G1's windows arm is puts (dynamic under both mingw gcc and… | INTERNAL: records, designs, BOARD rows, banks |
| 0e1e143a51 | batch 284 (docs): merge claude/h-cgo-phase2-design (0ed7bb4489) -- H: dated design amendments: 5.2 storage-keyed sameness and a returned argument IS t… | INTERNAL: records, designs, BOARD rows, banks |
| da6cc03c51 | batch 284: merge claude/g-xor-generic-sibling (500224d4ec) -- G, S2's residual R1e: mergeReviewSiblings splits a differing .cs.auto review sibling int… | INTERNAL: records, designs, BOARD rows, banks |
| 2821105ec7 | goldens: re-baseline CgoPseudoPackageTypes on the merged converter | INTERNAL: a behavioral golden re-baselined |
| 4384472449 | batch 284: merge claude/c2-darwin-callerline-hang (98973cdb3f) -- C2: the darwin CallerLine hang, rooted by measurement. EveryShapeEqualsGoInADebugBui… | INTERNAL: test-only (a GolibTests Debug hang, rooted) |
| daaf3de871 | batch 284 (docs): merge claude/c1-board-float-cast (1cc9f40b39) -- C1: a BOARD row disclosing that a direct non-constant float-to-integer conversion f… | INTERNAL: records, designs, BOARD rows, banks |
| 339994562e | batch 284: merge claude/p2-layout-key-guards (10c6f7067f) -- P2: the layout-key guard sweep. Every remaining reader that enumerated a fixed folder set… | INTERNAL: records, designs, BOARD rows, banks |
| 09b760e117 | batch 284 (docs): merge claude/g-c42-bounds-board (d8acc7f8f0) -- G: C42's Bounds corrected (R1 is wider: declaring the method is no workaround) and t… | INTERNAL: records, designs, BOARD rows, banks |
| b9a6d261fc | docs/ConversionStrategies.md: re-cite moduleConverter_integration_test.go:230-232 -> 231-233 (batch 283 union) | INTERNAL: tree maintenance (re-cites, union fixes) |
| bff1d1dd3e | batch 283 (docs): merge claude/g-la64-design-r4 (7210d949fa) -- G: DESIGN-linux-arm64-flavour.md revision 4 (11.6 the single-target residual, 11.7) | INTERNAL: records, designs, BOARD rows, banks |
| 4daf1c6b06 | batch 283 (docs): merge claude/g-board-la64-routes (539946866d) -- G: BOARD rows: a single-target conversion rewrites six attribution files LF and a w… | INTERNAL: records, designs, BOARD rows, banks |
| c85d3b9d0e | batch 283: merge claude/g-corpus-regen-281-282 (235e21dc98) -- G: the stdlib regeneration two converter seats stated but never committed: batch 281's … | covered by the corpus regen of the tool-tags seat (d914d8b360, "Smaller converter fixes") |
| 46cc2c551c | batch 283 (docs): merge claude/coord-mtime-restore-rule (87441d1a5a) -- COORD: gate-forensics gains two instrument traps: a restore by move keeps the … | INTERNAL: records, designs, BOARD rows, banks |
| 66a2da59fb | batch 283 (docs): merge claude/h-cgo-phase2-design (a0ec9d3218) -- H: the dated section 7 line, a single scalar pointee crosses under 11.1 and element… | INTERNAL: records, designs, BOARD rows, banks |
| 76b8448c7c | batch 283 (docs): merge claude/h-cgo-phase2-design (c01de71708) -- H: DESIGN-cgo-phase2-pinvoke.md dated lines: 11.17 (the C pseudo-package <pkg>ꓸC_pa… | INTERNAL: records, designs, BOARD rows, banks |
| 38caf3558a | batch 282 (docs): merge claude/h-cgo-phase2-design (9ff0fc6664) -- H: DESIGN-cgo-phase2-pinvoke.md dated lines -- section 11.4 (the libc qsort seam re… | INTERNAL: records, designs, BOARD rows, banks |
| f6dd06a8da | batch 282 (docs): merge claude/c1-arch-count (5c1f8a956f) -- C1: the arch-count bank form (guard Get-ArchCountReading and annotate_darwin.py arch_coun… | INTERNAL: records, designs, BOARD rows, banks |
| 488aef2c2d | batch 282 (docs): merge claude/coord-layout-gate-rule (59831df570) -- COORD: .claude/rules/corpus.md, a change that moves the corpus layout runs every… | INTERNAL: test-only |
| 5ed131f51d | batch 282 (docs): merge claude/p2-nugetgo-wave-1264 (dfb102c631) -- P2: PLAN-nugetgo-wave-1-26-10-4.md, the nugetgo rebuild wave for the .4 break, wit… | INTERNAL: records, designs, BOARD rows, banks |
| 0420fb8bb7 | batch 282 (docs): merge claude/c2-nugetgo-sheets-1264 (41a2f5a297) -- C2: the four .4 nugetgo run sheets as TEMPLATES (block 1 throws while a placehol… | INTERNAL: records, designs, BOARD rows, banks |
| d739a42b6a | batch 282: merge claude/p2-netuserinfo-rid (b32903bb26) -- P2: WindowsNetUserInfoTests resolve the built-in administrator and guest by RID (500/501, N… | INTERNAL: test-only |
| 1bc62b4b6b | batch 282 (docs): merge claude/p1-board-gen-foreign-chains (7528e06e43) -- P1: the BOARD row for the seven alias shapes of the C29 arc | INTERNAL: records, designs, BOARD rows, banks |
| a8a895b2e3 | batch 281 (docs): merge claude/c1-darwin-bank-279 (e40c0eb8f7) -- C1: the batch 279 darwin bank re-read at 9b8af3bd30 (42740799606, both RIDs, cells f… | INTERNAL: records, designs, BOARD rows, banks |
| fb46a2a5df | batch 281 (docs): merge claude/c1-os-linux-rebank (5b8c4d3f7a) -- C1: os's linux row re-banked 3831 + 2 -> 3840 + 2, ruled HOST: the hosted runner rea… | INTERNAL: records, designs, BOARD rows, banks |
| 8995944018 | batch 281 (docs): merge claude/h-cgo-phase2-design (55aac64e6b) -- H: DESIGN-cgo-phase2-pinvoke.md, the cgo Phase 2 P/Invoke design, with section 11 f… | INTERNAL: records, designs, BOARD rows, banks |
| 3e67bc8d35 | batch 281 (docs): merge claude/coord-cgo-phase2-review (8a30cab3e6) -- the review record's dated amendment: the conformance check of H's section 11 an… | INTERNAL: records, designs, BOARD rows, banks |
| a4f1bae7d4 | batch 281 (docs): merge claude/coord-golib-apicompat-rule (56774f13ae) -- COORD: .claude/rules/golib-gen.md, a golib public-surface change runs the bi… | INTERNAL: records, designs, BOARD rows, banks |
| bdb1b83bd5 | batch 281 (docs): merge claude/coord-cgo-phase2-review (2e859d6c9b) -- REVIEW-cgo-phase2-pinvoke.md, the adversarial review of H's cgo Phase 2 design … | INTERNAL: records, designs, BOARD rows, banks |
| e8fc141a32 | batch 281 (docs): merge claude/i9-la64-s3-census (07a66bfea8) -- i9: DATA-la64-s3-census.md, the linux/arm64 S3 census of the hand-owns, plus a dated … | INTERNAL: records, designs, BOARD rows, banks |
| 429e24c092 | batch 281: merge claude/i9-flake-cpu (8e4f967c9a) -- i9: the idle-class sleep test asserts only quiet sleeps, never a scheduler-dependent reading | INTERNAL: test-only |
| 6481da92e4 | batch 281: merge claude/i9-flake-gpf (741ee49346) -- i9: the goroutine-profile fill test counts only its own children, so a goroutine from another tes… | INTERNAL: test-only |
| 66a2e023ca | batch 280 (docs): merge claude/c1-syscall-darwin-bank (aa06c2774f) -- C1: syscall banks darwin at 32 + 1 on both RIDs (the reinterpret seam), by rulin… | INTERNAL: records, designs, BOARD rows, banks |
| afb7032070 | batch 280 (docs): merge claude/g-la64-design-r3 (e0e994973a) -- G: DESIGN-linux-arm64-flavour.md revisions 1-3 (the four-target rule R1a-e, approved a… | INTERNAL: records, designs, BOARD rows, banks |
| 104c2dfcf4 | batch 280 (docs): merge claude/p1-board-gen-foreign-chains (23a68159f3) -- P1: a BOARD row on go2cs-gen's gaps over cross-package wrapper chains (D1-D… | INTERNAL: records, designs, BOARD rows, banks |
| d81da0c9e2 | batch 280 (docs): merge claude/coord-breakfloor-review (96e6313cad) -- REVIEW-go-break-floor.md, the adversarial review of the break floor and COORD's… | INTERNAL: records, designs, BOARD rows, banks |
| 378a2f8a2e | batch 280 (docs): merge claude/i7-ts-readiness-rerun (a03f45add5) -- the TypeScript readiness re-run record: dated amendments to EXPERIMENT-ts-readine… | INTERNAL: records, designs, BOARD rows, banks |
| cfded634b8 | batch 280 (docs): merge claude/r-mn-r6-record (4cd5e310c9) -- R, M:N R6: the design doc's O1 amendment (at stock vm.max_map_count, Linux aborts at abo… | INTERNAL: records, designs, BOARD rows, banks |
| 08a3d9c526 | batch 280: merge claude/i9-flake-smp (159bc9e9bc) -- i9: SyncMutexProfileTests assert what the code guarantees, never the scheduler. A deterministic E… | INTERNAL: test-only |
| 46dc5ce623 | batch 280: merge claude/c1-c21-guard (298d3646d1) -- C1: NamedSliceGoClone, an extra guard on R's C16(b)/C21 fix (the GoImplement shim for a Go Clone … | INTERNAL: CI, guards, instruments, hygiene |
| 9b8af3bd30 | batch 279: merge claude/i9-flake-acr (152a2fe2c5) -- i9, the AsyncChannelTests.CancellationRacingDeliveryNeverLosesAValue flake: the PRECONDITION only… | INTERNAL: test-only |
| 123dc8f57a | batch 279 (docs): merge claude/coord-la64-review (11d081c637) -- COORD: REVIEW-linux-arm64-flavour.md, the adversarial review of G's linux/arm64 desig… | INTERNAL: records, designs, BOARD rows, banks |
| b6faffde00 | batch 279 (docs): merge claude/p2-pin-corrections (93bac15ab9) -- P2: dated corrections owed by the NU1107 record. A nuget.org deprecation is NOT show… | INTERNAL: records, designs, BOARD rows, banks |
| 0c553534df | batch 278 (docs): merge claude/i9-goexit-amend (00070bcc64) -- i9: DESIGN-goexit-main.md step 4's re-reads, one dated amendment. sync PASS 48=48, iter… | INTERNAL: records, designs, BOARD rows, banks |
| e903def6b7 | NEWS 1.26.10.3: the two publish-triggered PENDING items flipped -- gojq for .NET ships (nugetgo .3 wave published), and the 1.26.10.1/.2-built nugetgo… | INTERNAL: records, designs, BOARD rows, banks |
| f09e762659 | BOARD: the CLR heap carries none of Go's [anon: Go: ...] region labels on a naming kernel -- a fidelity gap TestDecorateMappings cannot see (validateM… | INTERNAL: records, designs, BOARD rows, banks |
| 02cc6d9d85 | batch 277 (docs): merge claude/p1-board-generic-namedptr (1a097edc58) -- P1: BOARD row, a GENERIC named pointer type is broken at the base (found whil… | INTERNAL: records, designs, BOARD rows, banks |
| 3ed27f75b9 | batch 277 (docs): merge claude/p2-nu1107-pins (b6ebe12773) -- P2: RECORD-nuget-sibling-pins.md. go.* sibling pins are exact by the OQ-2 ruling (4e4c98… | INTERNAL: records, designs, BOARD rows, banks |
| f7828efce3 | batch 277 (docs): merge claude/p2-goarch-census (45bdf35814) -- P2: CENSUS-goarch-linux-windows.md. Windows: YES, the darwin displacement (runtime.GOA… | INTERNAL: records, designs, BOARD rows, banks |
| df98138b54 | batch 276 (1/2): merge claude/c2-nugetgo-1261003-smoke (c2f6b294b9) -- C2: Test-PostPublishRestore.ps1, the nugetgo 1.26.10.3 wave's post-publish smok… | INTERNAL: CI, guards, instruments, hygiene |
| d9a7dcd344 | go2cs.CpuProfiler/DarwinThreadCpu.cs: the BSD-3-Clause header every project-owned src/core source carries, not MIT (TestLicensingPackageBoundaries; ma… | INTERNAL: CI, guards, instruments, hygiene |
| 911b363e25 | docs: restore master's two variableAnalysisOperations.go re-cites on ConversionStrategies.md:10534 that the TS train 2 union subtracted (2872-2881#4ff… | INTERNAL: tree maintenance (re-cites, union fixes) |
| d994700549 | TS train 2 (docs rider): merge claude/c1-net-darwin-bank (0acfd66ec6) -- C1: net banks darwin, osx-x64 551 + 4 and osx-arm64 552 + 3, at e45fd4feb5 wi… | INTERNAL: records, designs, BOARD rows, banks |
| 908b803338 | TS train 2: merge claude/g-comparator-design (9b729e327e) -- G: two dated §5 amendments to DESIGN-comparator-arms.md on top of the c22a158a07 landed i… | INTERNAL: records, designs, BOARD rows, banks |
| 84ed9400b6 | batch 273 (4/4): merge claude/p2-docs-audit (a516840890) -- P2: AUDIT-user-facing-docs.md, .4 scope item (4): 25 visitor pages by link-walk, 189 findi… | INTERNAL: the docs audit record; its fixes (bce71f3ae9) are in "Documentation" |
| b8b68fff04 | batch 273 (3/4): merge claude/c2-census-modver (9f779e5314) -- C2: the identifier census's ipv4 arm gains rule 8 (module-version): a four-part version… | INTERNAL: records, designs, BOARD rows, banks |
| 817bd0668e | batch 273 (2/4): merge claude/g-comparator-design (c22a158a07) -- G: DESIGN-comparator-arms.md, RULED by COORD 2026-11-09: R1 arm (a) goSkipSignature … | INTERNAL: records, designs, BOARD rows, banks |
| 9d862cd0fc | batch 273 (1/4): merge claude/g-mn-spike-record (376be99a4f) -- G: DATA-mn-spike.md, the M:N R5 spike record. Verdict GO for hidden async twins (O6+O7… | INTERNAL: records, designs, BOARD rows, banks |
| b1664b5552 | docs: re-cite four provenance notes the TS train's union moved (visitFuncDecl.go :2961-2969 / :2045 / :2046 after C10; variableAnalysisOperations.go :… | INTERNAL: tree maintenance (re-cites, union fixes) |
| e45fd4feb5 | C1 darwin batch: merge claude/c1-darwin-net-dns (cd0a6413ee) -- C1: os-matrix sweep-shard gains an opt-in darwin-only dns=<resolvers> option (probe, s… | INTERNAL: CI, guards, instruments, hygiene |
| b331e696fb | C1 darwin batch: merge claude/c1-escalation-floor (51c9c60390) -- C1: GoschedBackoffEscalationLatencyTests: the sample floor extends the yield count o… | INTERNAL: test-only (Gosched escalation floor) |
| 58cfeb3d0f | batch 269: merge claude/g-mn-design (1e736b2e69) -- G, DOCS ONLY: docs/phase4/DESIGN-mn-scheduler.md (PROPOSED, R1-R6 RULED 2026-11-09 by the owner: r… | INTERNAL: records, designs, BOARD rows, banks |
| cd97240e9f | batch 267: merge claude/c2-nugetgo-1261003-sheets (d694c8785d) -- C2: the nugetgo .3 wave's four PIN sheets filled for the owner's paste (docs/phase4/… | INTERNAL: records, designs, BOARD rows, banks |
| be5ce695d2 | batch 266 (3/3): merge claude/h-b266-runtime-page (efdb24a8d7) -- H: runtime's proof page re-rendered by the -Exact sweep at the gate tip 33faf4bba8, … | INTERNAL: records, designs, BOARD rows, banks |
| 0904811dcf | batch 264 (3/3): merge claude/h-b264-pprof-page (9bfa07d1bc) -- H: runtime/pprof's proof page re-rendered by the -Exact sweep at the batch tip 4fe1cb9… | INTERNAL: records, designs, BOARD rows, banks |
| e1761e9a17 | release: go2cs converted stdlib 1.26.10.3 | INTERNAL: the 1.26.10.3 release commit itself |
