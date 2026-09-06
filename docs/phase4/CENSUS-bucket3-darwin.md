# CENSUS — bucket 3 on **darwin**: generated stubs whose body exists in `runtime` and does not arrive

> **State: MEASURED.** Read-only census of the darwin corpus flavour at master `69136ef1ae`.
> Nothing in `src/core` was modified. All paths repo-relative.
>
> **Headline: of 458 generated stub files, 52 have a `//go:linkname` push aimed at them, and for
> 49 of those the pushing function HAS A CONVERTED BODY IN THE TREE.** Every one of the 49 is
> pushed from `runtime`.
>
> This is the darwin half of the class G censused on windows
> (`CENSUS-bucket3-unreachable-bodies.md`). The funnel is deliberately theirs so the two numbers
> mean the same thing. **It also corrects two things in the shared record — see §5 — and both
> corrections were found by controls rather than by looking for them.**

## 1. The funnel

```
 458   generated stub (pkg,name) pairs        the population
       |-- 406  no push entry at all             NOT bucket 3
       +--  52  a push entry exists              candidates
              |-- 49  push source HAS a body    <- THE FINDING
              +--  3  push source has none      a PULL, not a defect
```

`49 + 3 = 52` partitions the join exactly, and the assertion is in the instrument rather than in the prose.

**The population is nearly double windows' 232, and the whole difference is one family.** 214 of the
458 stubs are `libc_*_trampoline` — darwin's libc shim layer, whose PC is taken and never called.
**Zero of them are bucket-3 members**, so the family inflates the population and contributes nothing
to the finding. Windows has no equivalent layer.

| population by package | stubs |
|---|---:|
| `runtime` | 175 |
| `syscall` | 131 |
| `reflect` | 67 |
| `crypto/x509/internal/macos` | 29 |
| `internal/syscall/unix` | 24 |
| `runtime/pprof` | 9 |
| `runtime/trace` | 4 |
| `os` | 3 |
| `vendor/golang.org/x/sys/cpu` | 3 |
| `internal/bytealg` | 2 |
| *(8 more packages)* | 11 |

## 2. The 49, by owning package

| package | count |
|---|---:|
| `reflect` | **33** |
| `syscall` | **5** |
| `runtime/trace` | **4** |
| `runtime/pprof` | **3** |
| `crypto/x509/internal/macos` | **1** |
| `internal/coverage/cfile` | **1** |
| `internal/syscall/unix` | **1** |
| `os` | **1** |

**Every push source is in `runtime`.** That is the structural statement the count is really making:
the darwin corpus has 49 symbols that `runtime` implements, that go2cs converted, and that the
destination assembly fills with a throw because the push does not cross the assembly boundary.

| package | name | pushed from | site |
|---|---|---|---|
| `crypto/x509/internal/macos` | `syscall` | `crypto_x509_syscall` | `src/core/runtime/darwin/sys_darwin.cs:228` |
| `internal/coverage/cfile` | `getCovCounterList` | `coverage_getCovCounterList` | `src/core/runtime/covercounter.cs:12` |
| `internal/syscall/unix` | `gostring` | `internal_syscall_gostring` | `src/core/runtime/string.cs:359` |
| `os` | `sigpipe` | `os_sigpipe` | `src/core/runtime/darwin/signal_unix.cs:27` |
| `reflect` | `chancap` | `reflect_chancap` | `src/core/runtime/chan.cs:851` |
| `reflect` | `chanclose` | `reflect_chanclose` | `src/core/runtime/chan.cs:856` |
| `reflect` | `chanlen` | `reflect_chanlen` | `src/core/runtime/chan.cs:841` |
| `reflect` | `chanrecv` | `reflect_chanrecv` | `src/core/runtime/chan.cs:798` |
| `reflect` | `chansend0` | `reflect_chansend` | `src/core/runtime/chan.cs:793` |
| `reflect` | `growslice` | `reflect_growslice` | `src/core/runtime/slice.cs:341` |
| `reflect` | `ifaceE2I` | `reflect_ifaceE2I` | `src/core/runtime/iface.cs:697` |
| `reflect` | `makechan` | `reflect_makechan` | `src/core/runtime/chan.cs:57` |
| `reflect` | `makemap` | `reflect_makemap` | `src/core/runtime/map.cs:1448` |
| `reflect` | `mapaccess` | `reflect_mapaccess` | `src/core/runtime/map.cs:1496` |
| `reflect` | `mapaccess_faststr` | `reflect_mapaccess_faststr` | `src/core/runtime/map.cs:1506` |
| `reflect` | `mapassign0` | `reflect_mapassign` | `src/core/runtime/map.cs:1524` |
| `reflect` | `mapassign_faststr0` | `reflect_mapassign_faststr` | `src/core/runtime/map.cs:1532` |
| `reflect` | `mapclear` | `reflect_mapclear` | `src/core/runtime/map.cs:1634` |
| `reflect` | `mapdelete` | `reflect_mapdelete` | `src/core/runtime/map.cs:1540` |
| `reflect` | `mapdelete_faststr` | `reflect_mapdelete_faststr` | `src/core/runtime/map.cs:1545` |
| `reflect` | `mapiterelem` | `reflect_mapiterelem` | `src/core/runtime/map.cs:1606` |
| `reflect` | `mapiterinit` | `reflect_mapiterinit` | `src/core/runtime/map.cs:1561` |
| `reflect` | `mapiterkey` | `reflect_mapiterkey` | `src/core/runtime/map.cs:1592` |
| `reflect` | `mapiternext` | `reflect_mapiternext` | `src/core/runtime/map.cs:1578` |
| `reflect` | `maplen` | `reflect_maplen` | `src/core/runtime/map.cs:1620` |
| `reflect` | `memmove` | `reflect_memmove` | `src/core/runtime/stubs.cs:152` |
| `reflect` | `rselect` | `reflect_rselect` | `src/core/runtime/darwin/select.cs:532` |
| `reflect` | `typedarrayclear` | `reflect_typedarrayclear` | `src/core/runtime/mbarrier.cs:416` |
| `reflect` | `typedmemclr` | `reflect_typedmemclr` | `src/core/runtime/mbarrier.cs:397` |
| `reflect` | `typedmemclrpartial` | `reflect_typedmemclrpartial` | `src/core/runtime/mbarrier.cs:402` |
| `reflect` | `typedmemmove` | `reflect_typedmemmove` | `src/core/runtime/mbarrier.cs:229` |
| `reflect` | `typedslicecopy` | `reflect_typedslicecopy` | `src/core/runtime/mbarrier.cs:356` |
| `reflect` | `typehash` | `reflect_typehash` | `src/core/runtime/alg.cs:372` |
| `reflect` | `unsafe_New` | `reflect_unsafe_New` | `src/core/runtime/darwin/malloc.cs:1238` |
| `reflect` | `unsafe_NewArray` | `reflect_unsafe_NewArray` | `src/core/runtime/darwin/malloc.cs:1292` |
| `reflect` | `unsafeslice` | `reflect_unsafeslice` | `src/core/runtime/unsafe.cs:129` |
| `reflect` | `verifyNotInHeapPtr` | `reflect_verifyNotInHeapPtr` | `src/core/runtime/mbitmap.cs:1330` |
| `runtime/pprof` | `mach_vm_region` | `mach_vm_region` | `src/core/runtime/darwin/sys_darwin.cs:896` |
| `runtime/pprof` | `proc_regionfilename` | `proc_regionfilename` | `src/core/runtime/darwin/sys_darwin.cs:932` |
| `runtime/pprof` | `readProfile` | `runtime_pprof_readProfile` | `src/core/runtime/cpuprof.cs:224` |
| `runtime/trace` | `userLog` | `trace_userLog` | `src/core/runtime/traceruntime.cs:696` |
| `runtime/trace` | `userRegion` | `trace_userRegion` | `src/core/runtime/traceruntime.cs:669` |
| `runtime/trace` | `userTaskCreate` | `trace_userTaskCreate` | `src/core/runtime/traceruntime.cs:640` |
| `runtime/trace` | `userTaskEnd` | `trace_userTaskEnd` | `src/core/runtime/traceruntime.cs:653` |
| `syscall` | `runtime_AfterExec` | `syscall_runtime_AfterExec` | `src/core/runtime/darwin/proc.cs:4997` |
| `syscall` | `runtime_AfterFork` | `syscall_runtime_AfterFork` | `src/core/runtime/darwin/proc.cs:4927` |
| `syscall` | `runtime_AfterForkInChild` | `syscall_runtime_AfterForkInChild` | `src/core/runtime/darwin/proc.cs:4958` |
| `syscall` | `runtime_BeforeExec` | `syscall_runtime_BeforeExec` | `src/core/runtime/darwin/proc.cs:4982` |
| `syscall` | `runtime_BeforeFork` | `syscall_runtime_BeforeFork` | `src/core/runtime/darwin/proc.cs:4899` |

## 3. Method

1. **Purge, build the darwin flavour, gate on completeness.** `bin`/`obj`/`Generated` removed
   depth-unlimited, then
   `dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=darwin --no-incremental -m -p:UseSharedCompilation=false`.
   **The purge is not hygiene: `Generated/` accumulates and nothing prunes it** — the tree held **462**
   stub files before the purge, from builds of more than one flavour — so a census over an unpurged
   tree measures a union of builds nobody ran. The surviving count is asserted to be **zero** before
   the build starts, which is a stronger guarantee than an mtime check: nothing present afterwards
   can predate the build.
2. **A completeness gate, because a SHORT build and a complete one look identical in the output.**
   307 projects in the solution, **306 assemblies**, and the one project producing none is named:
   `gen/go2cs-gen`, the Roslyn analyzer, which targets `netstandard2.0` and has its assembly there.
   0 strict errors (`error (CS|MSB|NETSDK)[0-9]+` — a loose `grep -c 'error '` scores hits on the
   `errors` package name). Wall 1,042 s.
3. **The push map re-derived, then restricted to the flavour.** See §5 for what the re-derivation
   found. The map is then cut to the files this flavour actually compiles — flat files plus
   `*/darwin/*`, excluding `*/windows/*`, `*/linux/*` and `_test.cs` — because a push declared only
   in `runtime/windows/proc.cs` does not exist in a darwin build.

**`//go:linkname` is not one relation but two, and only a body test separates them.** The
two-argument form `//go:linkname localname importpath.name` is a **push** when `localname` has a
body in this package (the body is installed as `importpath.name`) and a **pull** when it does not
(the symbol is provided elsewhere). Same syntax, opposite directions.

## 4. The three PULL rows, and they cross-check against G's

The three candidates whose source has no body are `runtime.memequal`, `runtime.memequal_varlen` and
`runtime.reflectcall`. G's windows record names three with no body: `runtime.memequal_varlen`,
`runtime.reflectcall`, `syscall.compileCallback`. **Two are shared; each difference has a reason
rather than being noise** — `syscall.compileCallback` is a Windows callback API with no darwin
counterpart, and `runtime.memequal` is one of the five G dropped at the join (§5).

## 5. Two corrections to the shared record

**(a) The push map is 254, not 259, and five of its entries are Go source TEXT.** Deriving the map
independently reproduced G's **259 unique `(local,dest)` pairs** exactly — two lanes, two machines,
two parsers. Then five entries turned out to sit inside a C# raw string literal in
`go/types/issues_test.cs` (opening line 1005, closing 1037) holding a cgo-generated Go program used
as **test input data**: `runtime.cgoAlwaysFalse`, `runtime.cgoUse`, `runtime.cgocall`,
`runtime.cgoCheckPointer`, `runtime.cgoCheckResult`. Cross-checked a third way against GOROOT's own
`src/go/types/issues_test.go`, lines 886–904, inside a backticked Go raw string named `cgoTypes`
headed `// Code generated by cmd/cgo; DO NOT EDIT.`

**Every line-anchored predicate matches them, and the pattern is not at fault: inside the literal
the lines genuinely do begin with `//go:linkname`.** `^` is true. They were found only because the
classifier had to look each local name up and **reported that it could not find one** — an
instrument that states its own failures pays for itself.

**(b) G's five join-drops resolve, four of them into the finding.** G's §3 records five candidates
whose pushed name did not resolve on the first pass and which are therefore *not* in the windows 37:
`reflect.mapaccess`, `reflect.mapdelete`, `reflect.typedmemclr`, `reflect.unsafe_New`,
`runtime.memequal`. Carrying `(file, line)` provenance through the map instead of re-joining on the
pushed name resolves all five: **the four `reflect` members are bucket-3 members** (their sources
are `reflect_mapaccess`, `reflect_mapdelete`, `reflect_typedmemclr`, `reflect_unsafe_New` in
`runtime/map.cs`, `runtime/mbarrier.cs` and `runtime/darwin/malloc.cs`, all bodied) and
**`runtime.memequal` is a pull**, its source `abigen_runtime_memequal` bodyless in
`internal/bytealg/equal_native.cs:17`.

**These four are target-independent `reflect` intrinsics, so the windows count is very likely 41,
not 37** — and the arithmetic agrees from the other side: this census reads `reflect` **33**, which
is G's 29 plus exactly these four. **That is a prediction about G's artifact, not a measurement of
it**; `g-stubs.txt` settles it in one grep and the number stays G's to publish.

## 6. What the 49 does NOT mean

**It is not 49 defects and it is certainly not 49 pieces of work.** Three of the eight families are
already spoken for, in opposite directions:

- **`reflect`'s 33 are the unreached intrinsic family G measured on windows.** Their reachability is
  not re-measured here; G's dampener stands — those intrinsics appear in **0 of `reflect`'s 59
  disclosures**, so connecting them likely moves that row by zero.
- **`syscall`'s five are a KNOWN HAZARD and giving them bodies is the documented wrong move.**
  `runtime_BeforeExec`, `runtime_AfterExec`, `runtime_BeforeFork`, `runtime_AfterFork`,
  `runtime_AfterForkInChild` — CLAUDE.md records that empty bodies for the first two were argued
  correctly from `execLock`'s readers and **fork-bombed the `syscall` row**, 96 children in seven
  minutes, because the throwing stub was acting as a brake. Membership in this census says the body
  exists and does not arrive; it says nothing about whether connecting it is safe.
- **`internal/syscall/unix.gostring` is already answered by queued work** — `claude/c2-darwin-ptrout`
  gives it a body at `internal/syscall/unix/darwin/net_darwin_impl.cs:264`. It is the **only** one of
  the 49 that any unlanded darwin seat removes; `claude/c2-darwin-inc10`'s registry entries
  (`forkExec`, `Exec`, `pipe`, `Accept`, `Bind`, `Connect`) intersect the 49 **not at all**.

The genuinely darwin-shaped members are small in number and specific: `runtime/pprof`'s
`mach_vm_region` and `proc_regionfilename` (Mach APIs with no windows counterpart, which is why that
family reads 3 here against 1 on windows) and `crypto/x509/internal/macos.syscall`.

## 7. Instrument failures, recorded because each looked like a result

Four, all caught by controls rather than by inspection, and each would have shipped a plausible number:

1. **A `"""`-counting state machine desynced** on `net/http/server.cs:2458` (`@"""", "&#34;",` — a
   verbatim string holding an escaped quote, one non-overlapping `"""`), flipped itself into
   "inside a literal" for the rest of the file, and silently swallowed the **real** push at line
   3323. Six exclusions, one false, nothing in the output saying which. Caught by controlling the
   filter in **both** directions — a known-good directive must be KEPT, not merely a known-bad one
   excluded. The fence rule is edge-anchored now (opens at end-of-line, closes at start-of-line).
2. **A by-name classifier matched the wrong declaration.** For `//go:linkname call
   runtime.reflectcall` it found `reflect.Value.call` — a different method sharing a generic name,
   2,267 lines away — and reported a body. **Neither pure method is sound**: by-name over-matches a
   generic name; by-window breaks on a *block* of ten directives (`internal/fuzz/trace.cs`) and on a
   placeholder local (`badServeHTTP`, Go's hall-of-shame idiom, where the local name never existed as
   a function). The rule is nearest-declaration-by-name **with the distance reported**, so a far
   match is read rather than trusted. It moved exactly one row — and moved it into agreement with G.
3. **The completeness gate reported 307 of 307 projects missing**, because the solution spells paths
   relative to `src/` and the probe looked under the worktree root. It failed **loud**, which is the
   direction a broken gate should fail in; a subtler prefix error would have passed quietly.
4. **The funnel crashed** rather than silently reading five columns of a six-column file after the
   classifier gained a distance field.

## 8. Boundaries

- **Darwin only.** The stub population is per-target; windows is G's record, linux is unmeasured.
- **Built on a Linux host at `GoTargetOS=darwin`.** That property selects the `<Compile>` item set,
  which is what the generator reads, so the population is a function of the flavour rather than the
  host — but CI's darwin legs run on macOS runners with a RID and **this census has not been
  reconciled against one.** The completeness gate stands in for that until it is. One reassurance
  from inside the data: restricting the push map from all flavours (381 rows) to darwin only (243)
  changed the funnel by **nothing** — 458/52/49/3 either way — because every affected destination is
  pushed from the darwin file too, at identical line numbers. The contamination was in the
  attribution, not the membership.
- **Production assemblies only.** Two push rows live in `_test.cs` and are excluded, since the
  production build does not compile them: `runtime.blockUntilEmptyFinalizerQueue` (from
  `sync/oncefunc_test.cs`) and `runtime.haveHighResSleep` (from `time/sleep_test.cs`).
- **Membership is not reachability**, and this census does not measure reachability for any family.
- **The 406 without a push entry were not further split.** They are "not bucket 3" and nothing more.
- **The count is at master.** §6 names the one member queued work already removes, so the 49 is not
  read as work still owed when part of it is not.

## 9. Provenance

Master `69136ef1ae`, darwin flavour, net10 SDK 10.0.111, Go toolchain pinned to `go1.23.12` and
printed by the build script before it ran. Artifacts: the stub population (458), the push map before
and after the flavour restriction, the per-row source classification with distances, and the 49 —
all re-derivable from the repository by §3 alone, which is the point of writing it down.
