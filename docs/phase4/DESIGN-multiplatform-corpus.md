<!-- {% raw %} — Jekyll/Liquid guard: this doc contains {{ sequences (Go template/composite syntax) that Liquid would otherwise parse or silently eat. Keep the matching endraw as the final line. -->
# DESIGN — one corpus, three platforms: what a multiplatform standard library costs, and how it ships

> **STATUS: ACCEPTED** (user ruling 2026-08-08: recommendations accepted as written — layout L3 + packaging option (a) RID assemblies; increments proceed in order).
> **Increment 1 LANDED 2026-08-08** — the converter now takes the census itself (`-platforms` list +
> `-platform-census`), and it reproduces every number below; see §12. It changes no
> emitter, no corpus, and no packaging.
> **Increment 2 LANDED 2026-08-08** — layout L3 is real for `internal/goos`: a `-platforms` LIST with
> `-stdlib` now merges the per-target emissions into one tree, and a single-target reconvert honors the
> result (§8, "layout adoption").
> **Increment 3 LANDED 2026-08-08** — L3 now covers all **37** platform-varying packages, with conditioned
> `<ProjectReference>` groups on the **21** whose imports differ and per-GOOS `package_info`/`package_init`;
> the solution is the union (307 projects). The Windows lane is proven unmoved semantically, §13.1's IL
> question is answered (identical), and **the Linux corpus compiled for the first time** — 57 packages, one
> failing package, 8 errors. See §12.
> Everything below is **measured** — four seeded full-standard-library
> conversions, three `go list` censuses and a `go/types` API-surface probe, run 2026-08-08 in lane
> `r47c-goosdesign` against `739f3606ad`. Where a measurement contradicts an earlier ruling
> ([`PLAN-linux-operation.md`](../PLAN-linux-operation.md) §A4 **N2**), it is flagged as such in §7 rather
> than quietly reversed; the user rules.
>
> **Prior art, read first:** `PLAN-linux-operation.md` §A2 (what `src/core` *is*), §A2.4 (corpus options
> 1–4, ruled: option 2), §A4 (packaging options N0–N3, ruled: N2 as destination). This document supplies
> the numbers those two sections were explicitly written *without* — §A2.4 says "do **not** plan the tree
> layout before that number exists". The number now exists.

---

## 1. The question

The standing goal is **"a single set of NuGet packages with binaries included that will work for all
target platforms"** — Windows, Linux, macOS (the big three, ruled 2026-08-06).

The corpus contradicts that goal today. `src/core` is one conversion, taken at `GOOS=windows/GOARCH=amd64`.
Go selects its platform sources at *build* time through filename suffixes and `//go:build` constraints, so
a conversion does not merely *target* a platform — it *is* that platform. `go.os` and `go.syscall` are
published under platform-neutral IDs while containing `kernel32.dll` P/Invokes.

Three questions have to be answered in order, and only the third is a matter of taste:

1. **How much of the corpus actually differs per platform?** (§3–§5 — it is far less than the Go source
   suggests, and the differing set is not the set you would predict.)
2. **What can a single compile-time reference surface truthfully carry?** (§6 — nearly everything.)
3. **Which packaging shape follows from 1 and 2?** (§7–§9.)

---

## 2. Method, and the control that makes it trustworthy

**Four seeded full-stdlib conversions**, each into its own scratch root, run strictly sequentially (never
two converter processes alive at once — the r41 corruption rule). Each root was seeded per the CLAUDE.md
reconvert discipline: `src/core` + `src/version.props` + `docs/validation`, mirroring the repository's
`src/` layout so the hand-own detector and the README badge composer both see what they expect.

| Run | `-platforms` | Wall | Packages queued | Failures |
|:--|:--|--:|--:|--:|
| control | `windows/amd64` | 197 s | 304 | **0** |
| A | `linux/amd64` | 184 s | 302 | **0** |
| B | `darwin/amd64` | 191 s | 303 | **0** |
| C | `darwin/arm64` | 196 s | 301 | **0** |

**The control validates the instrument.** `conv-windows/src/core` compared against the committed
`src/core`: every `.cs`, every `.csproj` and every `README.md` **byte-identical**; the converter rewrote
**0** `.csproj` files because none had changed. The only 12 differences in the entire tree are the
`.cs.auto` review siblings, which the overlay rule deliberately freezes and which CLAUDE.md already records
as stale (CleanupBacklog item 18 — 11 of 16 at r40, 12 of 16 now). So the diff instrument reads zero on a
null input, and every delta reported below is a genuine platform effect rather than converter
non-determinism.

**Finding zero, obtained for free: the converter already produces a Linux and a macOS corpus, cleanly.**
All four runs completed with **zero failures** in ~3 minutes each. `-platforms os/arch` is already threaded
into every `go/packages` load and into the converter's own filename/`//go:build` constraint evaluator
(`directiveOperations.go`'s `goodOSArchFile` reimplementation). **No converter change is required to
*generate* a non-Windows corpus.** Everything this design proposes is about *organising and shipping*
that output, not about producing it.

A companion census used `go list` under each `GOOS` with the corpus's own tags
(`-tags purego,math_big_pure_go`, `CGO_ENABLED=0`, `GO111MODULE=off`) and a `go/types` probe enumerating
package-scope exported names per target.

---

## 3. Census A — the Go source is 89 % platform-neutral

Comparing `GoFiles + SFiles` per package across `windows`/`linux`/`darwin` at `amd64`, over the
convert-eligible set (the union of all three, minus the `isNonConvertedStdLibPackage` skips):

| | Packages |
|:--|--:|
| Convert-eligible (union of W/L/D) | **307** |
| **Identical source set on all three** | **274 (89.3 %)** |
| Differing source set | 27 |
| Present on only some platforms | 6 |

The six that do not exist everywhere:

| Package | Exists on |
|:--|:--|
| `internal/syscall/windows`, `.../registry`, `.../sysdll` | Windows only |
| `internal/runtime/syscall` | Linux only |
| `crypto/x509/internal/macos`, `vendor/golang.org/x/net/route` | macOS only |

The 27 with differing file sets, by size of the platform-exclusive part:

| Package | W files | L files | D files | common | W-only | L-only | D-only |
|:--|--:|--:|--:|--:|--:|--:|--:|
| `runtime` | 161 | 175 | 167 | 138 | 17 | 23 | 15 |
| `syscall` | 13 | 29 | 32 | 3 | 10 | 16 | 19 |
| `net` | 54 | 59 | 62 | 38 | 14 | 11 | 12 |
| `os` | 29 | 36 | 35 | 17 | 9 | 10 | 8 |
| `internal/syscall/unix` | 1 | 19 | 15 | 0 | 0 | 14 | 9 |
| `internal/poll` | 13 | 22 | 19 | 7 | 6 | 8 | 5 |
| `os/user` | 3 | 5 | 6 | 2 | 1 | 3 | 4 |
| `runtime/pprof` | 11 | 11 | 13 | 9 | 2 | 1 | 3 |
| `crypto/x509` | 12 | 13 | 12 | 11 | 1 | 2 | 1 |
| `internal/goos` | 3 | 3 | 3 | 1 | 2 | 1 | 1 |
| `crypto/rand` | 3 | 4 | 4 | 2 | 1 | 1 | 1 |
| `internal/sysinfo` | 2 | 2 | 2 | 1 | 1 | 1 | 1 |
| `net/internal/socktest` | 4 | 5 | 4 | 2 | 2 | 1 | 0 |
| `time` | 11 | 10 | 10 | 8 | 3 | 0 | 0 |
| `archive/tar` | 5 | 7 | 7 | 5 | 0 | 1 | 1 |
| *(12 more with a 1–2 file delta)* | | | | | | | |

Whole-corpus source volume: **1,408** Go+asm files for Windows, **1,477** for Linux, **1,479** for macOS.
The 33 platform-varying packages hold **381 / 448 / 448** of them — roughly 27–30 % of the source, but
only 11 % of the packages.

**Closure.** Of the 274 identical-source packages, **58 are fully platform-free** — nothing in their
transitive import closure varies. The other **216 have identical source but import something that varies**.
That 216 is an *upper bound* on "might still emit differently", not a measurement. §4 measures it, and the
answer is far smaller.

---

## 4. Census B — the converted corpus is 96 % byte-identical (the number that matters)

Windows vs Linux, both `amd64`, comparing the `.cs` each run actually emitted:

| | Files |
|:--|--:|
| Emitted by the Windows run | 1,665 |
| Emitted by the Linux run | 1,734 |
| Emitted by **both** | 1,563 |
| → **byte-identical** | **1,498 (95.8 %)** |
| → content differs | **65** |
| Windows-only filenames | 102 |
| Linux-only filenames | 171 |

**34 of 302 packages carry any delta at all. 268 emit byte-identical C#.**

Three-way at fixed `amd64` (Windows / Linux / macOS):

| | Files |
|:--|--:|
| Union of emitted `.cs` names | 1,928 |
| Byte-identical on all three | **1,489** |
| Same name, **different content** | **69** |
| Emitted by exactly two (the shared `unix` variants) | 88 |
| Platform-exclusive (exactly one) | 282 |
| **Packages with any delta** | **37** |

Project files track the same shape. At fixed `amd64` the Linux run rewrote **21** `.csproj` (20 differing
from the Windows control, plus the one Linux-exclusive package), the macOS run **22** (20 + its two
exclusives), and the Windows control **0**. Those are the `<ProjectReference>` lists — **24 packages have a
different direct import set** across the three (21 emitted everywhere whose reference list differs, plus the
3 platform-exclusive packages), which is exactly what a conditioned reference block must express.

> **Corrected again on 2026-08-08 by increment 3's emission** (lane r49a), which is the first run to
> *build* the conditioned reference blocks rather than count `.csproj` rewrites. The measured decomposition,
> taken from the merged tree: **21** packages are emitted on every platform and have a differing direct
> import set — these get conditioned `<ProjectReference>` groups — plus **6** platform-exclusive packages
> (`internal/syscall/windows`, `.../registry`, `.../sysdll`; `internal/runtime/syscall`;
> `crypto/x509/internal/macos`, `vendor/golang.org/x/net/route`) whose entire source set is per-GOOS and
> which need no conditioning at all. Total packages whose direct import set is not identical across all
> three: **27**, not 24. The **21** is exactly what this section's own prose already said ("21 emitted
> everywhere whose reference list differs"); the 24 came from counting `.csproj` REWRITES against a
> Windows-seeded control, which by construction never rewrites a *Windows*-exclusive package's project file
> and so cannot see those three. No `.cs` number moves, and 37 packages-with-any-delta is reproduced exactly.

> **Two numbers in this paragraph were corrected on 2026-08-08 by increment 1's census** (lane r47d), which
> re-measures every table in §3–§5 from the converter's own emission. It reproduced all of them exactly
> except here: this paragraph read "the macOS run 26 … 22 packages". The **26** is `darwin/arm64`'s rewrite
> count — run C, not run B — which §5's own two-target census reproduces exactly (`darwin/amd64` 22,
> `darwin/arm64` 26); §4 is fixed at `amd64`, so 22 is its number. The **22 packages** is not reproducible as
> a set from any run: the measured union across the three `amd64` targets is 24. Nothing else moved, and no
> `.cs` number did.

**Linux and macOS are close to each other and far from Windows** — the fact that makes the third platform
cheap:

| Pair | Emitted by both | Byte-identical | Differ | Packages touched |
|:--|--:|--:|--:|--:|
| Windows ↔ Linux | 1,563 | 1,498 | 65 | 34 |
| Windows ↔ macOS | 1,566 | 1,499 | 67 | 35 |
| **Linux ↔ macOS** | **1,633** | **1,597** | **36** | **20** |

### 4.1 The finding the plan did not anticipate: one Go file, two C# files

`PLAN-linux-operation.md` §A2.4's option 2 describes "one `src/core/<pkg>` holding `file_windows.cs` **and**
`file_linux.cs`, with `<Compile Include>` conditioned on `$(GoTargetOS)`". That shape assumes the
platform split is expressed in *filenames*. It is not. **38 converted source files have one Go source and
two or three different C# emissions**, under a neutral filename that is identical on every platform.

Five distinct mechanisms produce this, each verbatim from the probe:

**(a) Constant folding of a platform constant** — `path/filepath/path.cs`:

```diff
-public static UntypedInt Separator     => /* os.PathSeparator */ 92;
-public static UntypedInt ListSeparator => /* os.PathListSeparator */ 59;
+public static UntypedInt Separator     => /* os.PathSeparator */ 47;
+public static UntypedInt ListSeparator => /* os.PathListSeparator */ 58;
```

**(b) A folded constant reaching a literal** — `go/build/build.cs`:

```diff
-    @string sep = "\\";
+    @string sep = "/";
```

**(c) Escape analysis** — `syscall/syscall.cs`. On Linux the unix wrappers take the address of `_zero`, so
the converter promotes it to a heap box; on Windows nothing does, and it stays a plain static:

```diff
-internal static uintptr _zero;
+internal static ж<uintptr> Ꮡ_zero = new(default(uintptr));
+internal static ref uintptr _zero => ref Ꮡ_zero.Value;
```

**(d) Cross-file name-collision renaming** — `os/proc.cs`. On Windows a second `init()` exists in
`exec_windows.go`, so this one is renamed; on Linux there is no collision:

```diff
-[GoInit] internal static void initΔ1() {
+[GoInit] internal static void init() {
```

**(e) A constant-driven dead branch** — `net/conf.cs`, where `!cgoAvailable` folds to a constant `true`
and the converter emits golib's non-foldable true marker:

```diff
-                case {} when !cgoAvailable: {
+                case {} when ᐧᐧ: {
```

**Consequence.** A flat union directory cannot hold "both platforms' files", because 38 file *names* would
need two or three contents. Any layout must give those files per-platform identities. (b), (c) and (d) also
kill any scheme based on rewriting filename suffixes: the divergence is produced by whole-package analysis,
not by the file's own name.

**Corollary — and it is good news.** Platform-*exclusive* files never collide with one another. Every
`GOOS` variant of a Go package lives in **one** GOROOT directory, so Go has already guaranteed that
`file_windows.go`, `file_unix.go` and `file_linux.go` have distinct names. Only the neutral-named shared
files can collide, and those are the 38.

### 4.2 The platform axis must be computed from the emission, not from the file sets

Four packages have **identical Go source on all three platforms but different emitted C#** — `go/build`,
`internal/buildcfg`, `os/signal`, `time/tzdata` — through mechanisms (a)/(b) above and through
`package_info.cs` closure changes.

Three packages have **differing Go source but identical Windows↔Linux emission** (their variance is
macOS-only): `crypto/x509/internal/macos`, `internal/testpty`, `vendor/golang.org/x/net/route`.

So the set of packages needing per-platform treatment is **not** derivable from `go list`. It must come
from the converter's own three-target emission. A converter that conditioned on filename suffixes or on
`GoFiles` deltas would be wrong in both directions.

### 4.3 The 69 same-name-different-content artifacts

| Kind | Count | Note |
|:--|--:|:--|
| Converted source `.cs` | 38 | the §4.1 set |
| `package_info.cs` | 27 | closure-derived: `ImportedTypeAliases`, `GoImplement` records, `TypeAccessibility` |
| `package_init.cs` | 4 | Go's `InitOrder` differs when the file set does |

The 38 source files, in full: `crypto/x509/verify.cs`, `go/build/build.cs`,
`internal/buildcfg/zbootstrap.cs`, `internal/sysinfo/sysinfo.cs`, `internal/testenv/exec.cs`, `net/conf.cs`,
`net/dial.cs`, `net/dnsclient_unix.cs`, `net/fd_posix.cs`, `net/internal/socktest/switch.cs`,
`net/iprawsock_posix.cs`, `net/ipsock_posix.cs`, `net/lookup.cs`, `net/net.cs`, `net/nss.cs`, `net/pipe.cs`,
`net/sock_posix.cs`, `net/sockaddr_posix.cs`, `net/tcpsock_posix.cs`, `net/udpsock_posix.cs`,
`net/unixsock_posix.cs`, `os/file.cs`, `os/proc.cs`, `path/filepath/path.cs`, `runtime/arena.cs`,
`runtime/extern.cs`, `runtime/malloc.cs`, `runtime/mcheckmark.cs`, `runtime/mgcscavenge.cs`,
`runtime/mheap.cs`, `runtime/proc.cs`, `runtime/runtime.cs`, `runtime/select.cs`, `runtime/stack.cs`,
`runtime/trace.cs`, `runtime/traceexp.cs`, `runtime/tracetime.cs`, `syscall/syscall.cs`.

A worked miniature, small enough to read whole — `internal/goos`, emitted per platform:

| Windows | Linux | macOS |
|:--|:--|:--|
| `goos.cs` | `goos.cs` | `goos.cs` *(byte-identical on all three)* |
| `nonunix.cs` | `unix.cs` | `unix.cs` |
| `zgoos_windows.cs` | `zgoos_linux.cs` | `zgoos_darwin.cs` |
| `package_info.cs` | `package_info.cs` | `package_info.cs` *(identical)* |

`unix.cs` and `nonunix.cs` are the whole of Go's `unix.go`/`nonunix.go`:

```csharp
// unix.cs  (linux, darwin)             // nonunix.cs  (windows)
partial class goos_package {            partial class goos_package {
public const bool IsUnix = true;        public const bool IsUnix = false;
} // end goos_package                   } // end goos_package
```

Keep that pair in mind for §7 option (d).

---

## 5. Census C — the GOARCH axis is real but an order of magnitude smaller

macOS at `amd64` vs `arm64`, everything else fixed:

| | Files |
|:--|--:|
| Emitted by both | 1,701 |
| Byte-identical | **1,687** |
| Content differs | **14** |
| `amd64`-only names | 32 |
| `arm64`-only names | 31 |
| **Packages touched** | **16** |

The 16: `hash/crc32`, `internal/abi`, `internal/buildcfg`, `internal/bytealg`, `internal/cpu`,
`internal/goarch`, `internal/runtime/atomic`, `math`, `runtime`, `runtime/internal/startlinetest`,
`runtime/internal/sys`, `runtime/pprof`, `runtime/race`, `runtime/race/internal/amd64v1`, `syscall`,
`vendor/golang.org/x/sys/cpu`.

**This matters for sizing.** A RID is `os-arch`, not `os`. If both architectures are shipped for each of
the big three, the runtime matrix is six (`win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`,
`osx-arm64`), not three. Proportionally the arch axis is about a fifth of the GOOS axis: **77 of 1,764**
artifact names are arch-sensitive (4.4 %), against **439 of 1,928** that are GOOS-sensitive (22.8 %). Cheap
to carry — but not free, and a design that silently assumes three RIDs will be wrong the first time someone
runs on an ARM box. §12's staging therefore proves one RID pair before generalising.

---

## 6. Census D — the exported API surface, and why it decides the packaging

The packaging question is not "do the *implementations* differ" (§4 says: for 37 packages, yes). It is
**"can one compile-time reference assembly truthfully describe all three platforms?"** That is a question
about *exported* surface only. Measured with a `go/types` probe over every platform-varying package,
counting package-scope exported names:

| Package | W | L | D | common | Verdict |
|:--|--:|--:|--:|--:|:--|
| `syscall` | 992 | 2,186 | 1,899 | **270** | **DIVERGENT** |
| `internal/syscall/unix` | 1 | 34 | 62 | 1 | DIVERGENT *(internal)* |
| `internal/syscall/windows` | 202 | 0 | 0 | 0 | DIVERGENT *(internal)* |
| `internal/syscall/windows/registry` | 38 | 0 | 0 | 0 | DIVERGENT *(internal)* |
| `internal/syscall/windows/sysdll` | 2 | 0 | 0 | 0 | DIVERGENT *(internal)* |
| `internal/runtime/syscall` | 0 | 25 | 0 | 0 | DIVERGENT *(internal)* |
| `internal/poll` | 18 | 19 | 16 | 15 | DIVERGENT *(internal)* |
| `log/syslog` | **0** | 33 | 33 | 0 | **DIVERGENT** *(absent on Windows)* |
| `os` | 115 | 115 | 115 | 115 | identical |
| `net` | 102 | 102 | 102 | 102 | identical |
| `crypto/x509` | 109 | 109 | 109 | 109 | identical |
| `time` | 73 | 73 | 73 | 73 | identical |
| `runtime` | 48 | 48 | 48 | 48 | identical |
| `internal/testenv` | 38 | 38 | 38 | 38 | identical |
| `archive/tar` | 31 | 31 | 31 | 31 | identical |
| `path/filepath` | 27 | 27 | 27 | 27 | identical |
| `internal/goos` | 20 | 20 | 20 | 20 | identical |
| `net/internal/socktest` | 14 | 14 | 14 | 14 | identical |
| `runtime/pprof` | 14 | 14 | 14 | 14 | identical |
| `os/user` | 11 | 11 | 11 | 11 | identical |
| `mime` | 10 | 10 | 10 | 10 | identical |
| `os/exec` | 9 | 9 | 9 | 9 | identical |
| `internal/fuzz` | 9 | 9 | 9 | 9 | identical |
| `crypto/rand` | 4 | 4 | 4 | 4 | identical |
| *(6 more, 1–15 names each)* | | | | | identical |

**Only 8 of 29 varying packages have a divergent exported surface, and only two of those are user-facing:
`syscall` and `log/syslog`.** Everything a normal converted program touches — `os`, `net`, `time`,
`path/filepath`, `runtime`, `os/exec`, `os/user`, `crypto/x509`, `archive/tar`, `mime` — presents a
**name-for-name identical** public API on Windows, Linux and macOS.

This is the single most consequential measurement in the document. It means a **neutral compile-time
reference assembly is truthful for 27 of 29 varying packages and for all 268 non-varying ones** — the
compile surface is not the problem. Only the *runtime* implementation needs to vary, which is precisely
the shape .NET's RID-specific asset mechanism exists to express.

`syscall` is the genuine exception, and it is genuine: 992 vs 2,186 vs 1,899 exported names with 270 in
common. No honest single reference assembly describes that. `log/syslog` is the trivial exception — the
package exists on Windows but exports nothing, because Go's build constraint excludes its whole
implementation there.

---

## 7. Census E — the hand-owned files are almost all platform-neutral

51 hand-owned files (41 carrying a line-anchored `[module: GoManualConversion]`, plus the `*_impl.cs`
companions that do not).

| Class | Count |
|:--|--:|
| In a platform-neutral package | 33 |
| In a platform-varying package | 18 |
| — of which the *Go file they replace* is Windows-only | **5** |
| Carrying any P/Invoke at all | **4** |

The five genuinely Windows-bound hand-owns: `os/dir_windows_impl.cs`, `os/file_windows_impl.cs`,
`syscall/dll_windows.cs`, `syscall/exec_windows.cs`, `syscall/zsyscall_windows_impl.cs`.

The four P/Invoke carriers: those first three, plus `time/time_impl.cs` — whose single
`CreateWaitableTimerExW` call already sits behind `if (!OperatingSystem.IsWindows()) return null;` with a
documented coarse-wait fallback. **`golib` and `go2cs-gen` remain provably clean** (plan §A3/F10).

Two specifics worth carrying forward:

- **`runtime/lock_sema_impl.cs` needs a sibling, not a port.** `lock_sema.go` is selected on Windows *and*
  macOS; Linux uses `lock_futex.go` instead. The Linux corpus needs its own hand-own at a different
  filename — which the layout in §8 accommodates naturally.
  **✅ WRITTEN (increment 3.5b, lane r51b), and the sibling is four lines.** The prediction was right and
  understated *why*: the two flavors differ in their OS primitive (semaphore vs futex) and their key
  encoding, and the OS primitive is exactly the part that does not survive conversion — so both collapse
  onto ONE managed model. The shared core moved out of the Windows companion into a flat, platform-neutral
  `runtime/lock_managed_impl.cs`, and each flavor keeps only the single declaration whose *signature*
  differs. Not a port, not even really a sibling: a shared core plus two four-line adapters.
  **Increment 3.5 found the GENERAL form of this, and it is a second registry, not a second file.**
  A whole-file hand-own is routed by layout (§12, increment 3.5); a hand-owned *function* is not routed by
  anything, because `manualConversionFuncs` is keyed by **name** and is platform-blind. Every entry in it
  turns its Go declaration into a placeholder on **every** platform, while the implementation exists only
  where somebody wrote one. Measured over the whole registry, exactly two clusters are affected — the ones
  whose Go declaration exists on more than one platform:

  | Cluster | Entries | Implemented in | Missing on |
  |:--|:--|:--|:--|
  | `runtime` mutex/note | `mutexContended`, `lock2`, `unlock2`, `notewakeup`, `notesleep`, `notetsleep_internal` | `runtime/{windows,darwin}/lock_sema_impl.cs` | **linux** |
  | `os` directory walk | `File.readdir` | `os/windows/dir_windows_impl.cs` | **linux, darwin** |

  **✅ BOTH CLOSED (increment 3.5b, lane r51b) — and the two rows wanted OPPOSITE answers, which is the
  finding.** The `runtime` row needed the missing implementation written, because both flavors genuinely
  need hand-owning. The `os` row needed the entry *narrowed*: `dir_unix.go`'s readdir is pure Go over
  `internal/poll` and converts faithfully, so the "missing on linux" was a gap the name-keyed registry had
  invented rather than one the Go source has. Scoping Linux out gives that package a real body and leaves
  nothing owed. Darwin stays in scope on evidence, not by omission: `dir_darwin.go` hands libc's
  `readdir_r` the ADDRESS of a Go `syscall.Dirent`, the same non-blittable-by-address seam as `syscall`'s
  wrappers, so its hand-own is owed with macOS at increment 5.

  Every other entry is safe for a reason worth stating: `syscall`'s five (`GetTimeZoneInformation`,
  `findFirstFile1`, `findNextFile1`, `Process32First`, `Process32Next`) and `os.readReparseLink` name
  functions whose Go declarations are **Windows-only**, so no other platform emits a placeholder to leave
  unimplemented; and the ~140 in `reflect`, `internal/abi`, `internal/reflectlite`, `sync` and
  `internal/cpu` sit in platform-neutral packages. ⚠ The Linux sibling cannot be a copy even in its
  *surface*: `notetsleep_internal` is **four** arguments in `lock_sema.go` (`n, ns, gp, deadline`) and
  **two** in `lock_futex.go` (`n, ns`) — the same Go name with a different signature per platform, against
  a registry that cannot express one. The registry's own comment already warned about this in writing; the
  measurement is what turned the warning into a bounded list.
- **`time` may need no Linux hand-own at all.** Go's Linux zone loading is `time/zoneinfo_read.go`, pure Go
  that converts cleanly; the Windows hand-own exists only because `GetTimeZoneInformation` is a syscall.
  This is an argument *for* per-platform emission and *against* hand-owning a dispatcher (plan §A7.1).

The remaining 33 neutral hand-owns (`sync/*`, `reflect/*`, `internal/abi/*`, `math/rand/*`, …) are go2cs
runtime machinery. They are unaffected by platform and ship once.

---

## 8. Layout options — what the corpus looks like on disk

All options below keep **one tree**. None resurrects the two-tree doctrine the 2026-08-01 consolidation
deleted; the invariant "a build references exactly one GOOS" is preserved by MSBuild conditioning rather
than by directory separation.

### L1 — flat union, conditioned `<Compile>` items *(the plan's option 2 as written)*

**Rejected by measurement.** §4.1: 38 file names would need two or three contents in one directory.

### L2 — every platform-varying package wholly duplicated into per-GOOS subfolders

Works, and is trivially explained. Cost: the 37 varying packages contain ~420 files that are *identical*
across platforms (`runtime` alone contributes ~138), and L2 triplicates all of them.

### L3 — shared files flat; platform-selected files in per-GOOS subfolders **(recommended)**

The converter, from a single run loading all three targets, classifies every emitted artifact:

- **byte-identical on all three** → written flat, `src/core/<pkg>/<file>.cs`
- **anything else** (content varies, or emitted for only some platforms) → written to
  `src/core/<pkg>/<goos>/<file>.cs`

Measured pricing:

| | Files |
|:--|--:|
| Flat shared `.cs` | 1,489 |
| Per-GOOS `.cs` (variants ×3, pair-shared ×2, exclusives ×1) | 665 |
| **Union tree total** | **2,154** |
| Today's single-platform tree | 1,665 |
| **Growth for full three-platform coverage** | **+29.4 %** |

The csproj change is **one line**. The existing block already removes subfolders and globs the flat
directory:

```xml
<Compile Remove="**/*.cs" />
<Compile Include="*.cs" />
<Compile Include="$(GoTargetOS)/*.cs" />   <!-- new -->
<Compile Remove="*_test.cs;package_test_info.cs;go2cs_test_host.cs" />
```

`package_info.cs` and `package_init.cs` fall out of the same rule with no special case: identical ones stay
flat, varying ones land in the per-GOOS folder (27 and 4 respectively). The `<ProjectReference>` block
gains conditioned `ItemGroup`s for the **24** packages whose direct imports differ (§4, as corrected).

**Converter work L3 implies** — this is the whole feature, and it is bounded. Increment 2 landed items
1–4 for the emission path (`platformLayout.go` + `platformEmit.go`); the annotations record what was
actually built:

1. `-platforms` accepts a **list** (`-platforms windows/amd64,linux/amd64,darwin/arm64`). The loader
   plumbing is already per-target; what is new is running the pipeline N times in one process and holding
   the N emissions. — **Landed in increment 1.** The N emissions are held on disk, in seeded staging
   roots, rather than in memory: the seeding is what the hand-own detector and the README badge composer
   need to see, and a root that is a real go2cs tree is also what each target's dependency
   `package_info.cs` reads are resolved against. Holding them in memory would have meant rewriting every
   writer to emit into a buffer, for no gain.
2. A **three-way comparison** at write time decides flat vs per-GOOS. §4.2 is the reason this must be a
   comparison of *emissions*, not of Go file sets. — **Landed as increment 1's classifier, reused
   verbatim.** `-platforms <list>` with `-stdlib` now runs the census's staging and
   `classifyPlatformEmissions`, then MERGES: `identical` → flat, `variant`/`partial`/`exclusive` → one
   copy per emitting target's `<goos>/` folder. The merge is additive plus targeted removal (only the
   other candidate location of an artifact it is placing), so nothing else in the corpus is touched.
3. `writeProjectFile` emits the conditioned `Compile` line and per-GOOS `ProjectReference` groups. —
   **The `Compile` line landed; the `ProjectReference` groups are increment 3.** An emission run whose
   targets disagree about a package's project file reports those packages by name rather than silently
   merging the first target's import set.
4. `$(GoTargetOS)` needs a default. Proposal: default to the *host* OS in a plain `dotnet build`, so a
   Windows developer's `go2cs.slnx` build is unchanged, and set it explicitly per pack pass. —
   **Landed as `windows`, declared in the L3 package's own `.csproj`.** Host defaulting is the right
   destination but not yet a truthful one: it would make a plain `dotnet build` on Linux select a corpus
   nothing has ever compiled. It becomes a one-line change to that `PropertyGroup` at increment 3, where
   the Linux corpus's error buckets are first measurable. Declaring it in the package rather than in
   `src/core/Directory.Build.props` keeps the package self-describing — the same `.csproj` is correct in
   the repository, in a `deploy-core` staging root, and in a pack pass.
5. `solutionGenerator.go` is unaffected — the project set is the union, which it already computes. —
   **Confirmed:** the emission run regenerated a 304-project `go2cs-stdlib.slnx`, unchanged.

**The mechanism this section did not specify: how a SINGLE-target conversion behaves against an L3 tree
(increment 2's one design choice).** A conversion emits for one target, so it cannot compute the platform
axis — that axis is the comparison above. Left there, the documented single-target reconvert ritual would
lay a flat `zgoos_windows.cs` beside the `windows/zgoos_windows.cs` the `.csproj` is already compiling: a
duplicate-member build break, arrived at silently, and one that grows to 37 packages at increment 3. The
resolution is **layout adoption**: what one target cannot compute, it can *honor*. If the package
directory already holds `<goos>/<name>.cs`, that is where this target's `<name>.cs` is written; and a
package directory that holds any per-GOOS source folder gets the conditioned `<Compile Include>`. Both are
pure functions of the output tree — precise (the file must already be there), idempotent, and the same
class of rule as the `[module: GoManualConversion]` hand-own detector that sits directly above them in the
conversion driver. A per-GOOS folder is told from a nested package by the project file every converted
package directory holds and a source folder never does, which is what keeps `internal/syscall/windows` —
a real package whose own name is a GOOS — from being read as `internal/syscall`'s Windows variants.

The measured consequence is the one worth having: a seeded single-target `-stdlib` reconvert reproduces
the L3 corpus **file for file**, so the ritual in CLAUDE.md keeps working unchanged and the "Windows lane
must not move" gate reads empty rather than "empty except the L3 packages".

### L4 — per-GOOS **filename suffixes** instead of subfolders

Rejected as fragile: a Go filename may legally contain a dot (`foo.bar.go`), so no suffix scheme is
provably collision-free against the source language, and `<Compile Remove="*.linux.cs">` patterns multiply
where L3 needs one include.

---

## 9. Packaging options — real NuGet mechanics

Three mechanical facts govern everything here:

- `lib/{tfm}/` is a **compile-time and runtime** asset, RID-agnostic. `ref/{tfm}/` is compile-time only.
  `runtimes/{rid}/lib/{tfm}/` is a **runtime-only, RID-selected** managed asset — the same mechanism used
  for native shims, applied to IL. It requires a compile-time asset to exist elsewhere in the package.
- A framework-dependent portable app *does* resolve `runtimes/{rid}/lib/{tfm}` — the assets are recorded in
  `deps.json` under `runtimeTargets` and the host selects by RID at startup. RID-specific assets also
  survive `PublishAot` and self-contained publish, where the RID is explicit.
- **NuGet `<dependencies>` are declared per target framework only — never per RID.** Any package whose
  dependency graph varies by platform must declare the **union**, and accept that some restored assemblies
  go unused on some platforms.

### (a) RID-specific assemblies in one nupkg — **recommended**

```
go.os/
  lib/net9.0/os.dll                        ← compile-time reference (one designated flavor)
  runtimes/win-x64/lib/net9.0/os.dll
  runtimes/linux-x64/lib/net9.0/os.dll
  runtimes/osx-arm64/lib/net9.0/os.dll
```

- **Package IDs unchanged** — ~304, exactly today's set. Nothing new appears on nuget.org.
- **Consumer changes: none.** `dotnet add package go.os` then `dotnet run` works on every platform, and
  `-recurse=nuget` emits the same `PackageReference` it emits today.
- **Sized by measurement: only 37 of 302 packages need RID-specific assets at all** (§4). The other 265
  ship a single `lib/net9.0/` assembly, because their C# is byte-identical *and* every assembly they
  reference presents an identical exported surface (§6) — so one IL image satisfies all platforms.
- **The compile surface is truthful for all but two packages** (§6). `go.syscall` and `go.log.syslog` are
  the named exceptions; §11 lists the options for them as an explicitly open seam.
- **Dependency union.** `go.os` must declare `go.internal.syscall.windows` *and* `go.internal.syscall.unix`
  as dependencies, because the graph cannot vary by RID. Both restore everywhere; only the RID-matched
  assemblies load. A platform-exclusive package still needs a `lib/net9.0/` compile asset — shipping its
  single real flavor there is harmless, since nothing on the other platforms binds it at runtime.
- **Pack cost:** `push-nuget.ps1` grows from one build+pack pass to N build passes (one per RID, differing
  only by `-p:GoTargetOS=`) plus an asset-merge step before `dotnet pack`.

**Built in increment 4** (win-x64 / linux-x64; §12). Four things the sketch above left open, each decided
against NuGet's documented asset selection rather than by preference:

1. **`lib/` carries a flavor; there is no `ref/`.** `lib/{tfm}/` supplies both the `compile` and the
   `runtime` asset role, `ref/{tfm}/` only `compile`, and `runtimes/{rid}/lib/{tfm}/` only `runtime` —
   and the last *requires* a compile asset elsewhere in the package. So this is always a two-part shape
   and the only real question is what the compile half is. `ref/` carrying a neutral surface would turn
   an unshipped RID's silent wrong-flavor fallback into a loud missing-assembly failure, which is
   better — but **there is no neutral surface to put there**: §6 measures `syscall` at 270 names in
   common out of 992/2,186/1,899, and `log/syslog` exports nothing at all on Windows, so `ref/` would
   have to hold a synthesised intersection nothing in this repository produces, or one flavor again,
   which is `lib/` minus the fallback. **The cost of the choice, stated: on a RID this release does not
   ship (linux-arm64, say) the `lib/` assembly is what loads, so such a consumer silently gets Windows
   behaviour.** That is why the shipped RID set is declared in this document rather than inferred, and
   it is the argument for `runtimes/linux/…` (arch-agnostic RID) when §5's arch axis is taken up.
2. **The reference flavor is Windows,** so the compile surface a consumer binds against is exactly the
   one today's single-platform packages present. This is §11 seam 1's option (i) — "let
   `lib/net9.0/syscall.dll` carry the host-of-record flavor" — arrived at as a consequence rather than a
   separate decision; it still wants the ruling §11 asks for, because it is a public promise.
3. **Every shipped RID is stated explicitly**, including the reference one: the reference flavor's
   assembly is written to `runtimes/win-x64/` *as well as* `lib/`, rather than relying on "no RID
   matched, fall back to `lib/`". Which assembly a RID gets then never depends on which flavor `lib/`
   happens to carry — which is what makes changing the reference flavor, or introducing a `ref/`, a
   local edit later.
4. **The RID-split set is derived from the corpus, never listed.** L3's own rule read back off the
   tree: a directory named after a GOOS holding no project file is a per-GOOS source folder, and its
   parent is a platform-varying package. The "no project file" test is what keeps
   `internal/syscall/windows` — a real package whose own name is a GOOS — from being read as
   `internal/syscall`'s Windows variants. It reports **37**, which is §4's measured count.

And one correction to the sizing above: a package varying only on **darwin** (`crypto/x509/internal/macos`,
`vendor/golang.org/x/net/route`) produces identical win/linux assemblies, yet still ships RID assets here.
Shipping by *structure* rather than by a byte compare keeps the package shape a predictable function of the
corpus — and §12's increment-4 record explains why a byte compare cannot be the decision procedure at all.

### (b) TFM per OS (`net9.0-windows` + `net9.0`) — *the current N2 ruling; the measurement contradicts it*

This is what `PLAN-linux-operation.md` §A4 records as the user-ruled destination, on the reasoning that
"`net9.0-windows` is a real, first-class TFM" and "NuGet/MSBuild resolve it automatically". Both statements
are true. Two others are also true and were not available when the ruling was made:

1. **There is no `net9.0-linux`,** and `net9.0-macos` is a workload TFM (the macOS/AppKit bindings, as
   `net9.0-maccatalyst` is Catalyst's), not a console-library target that a `dotnet pack` of this corpus
   could produce. The TFM axis can express *Windows vs not-Windows* and nothing more — it cannot
   separate Linux from macOS, which §4 measures as 36 differing files across 20 packages. The plan's §A4
   already noticed this for the third flavor and proposed falling back to RID assets for linux and darwin;
   that hybrid uses two mechanisms where (a) uses one.
2. **TFM selection happens at compile time against the consumer's TFM, not at runtime against the host.**
   A portable app targeting plain `net9.0` and running on Windows would bind the *neutral* (non-Windows)
   assembly — the wrong one. To get the Windows flavor the app must itself target `net9.0-windows`, and a
   `net9.0` project cannot reference a `net9.0-windows` library at all (NU1201). So (b) requires every
   consumer to multi-target and build once per OS, which is the opposite of "a single set of packages that
   works for all target platforms".

**This is the one place where this design asks the user to revisit a ruling.** It is not a disagreement
about the goal — (b) and (a) target the same two-binary outcome — but the TFM mechanism cannot carry it for
three platforms, and it changes the consumer contract. Recommendation: replace N2's *mechanism* with (a),
keeping N2's *intent* (one package ID set, consumer picks nothing) intact.

### (c) Three package sets (`go.os.windows` / `go.os.linux` / `go.os.darwin`)

The plan rejected this as permanent public ID sprawl, and that judgment stands. The measurement does
improve the arithmetic — only 37 packages vary, so a hybrid would be 265 neutral IDs + 37 × 3 = **376 IDs**
rather than ~900 — but it still forces `-recurse=nuget` to rewrite references by target, still means a
user's project file names a platform, and still publishes an irreversible naming scheme. Recorded as
measured, not recommended.

### (d) One assembly, runtime OS dispatch — **rejected**

All three platforms' code compiled together under namespaced classes, selected at runtime. The smallest
counter-example is the §4.3 miniature:

```csharp
// unix.cs   → public const bool IsUnix = true;
// nonunix.cs → public const bool IsUnix = false;
```

Both are members of `goos_package`. Compiled together they are a duplicate-member error; and they are
`const`, so they cannot be converted to a runtime-selected property without breaking every downstream
`const` context. The same objection scales: `path/filepath.Separator` is a compile-time constant in both
languages, and `syscall` would need 992 + 2,186 + 1,899 names disambiguated in one assembly.

Beyond the collisions, the dispatch boundary is in the wrong place. Go's platform seam is *internal* —
`os` reaches its platform behaviour through `internal/syscall/windows` vs `internal/syscall/unix`, two
different assemblies with different surfaces (§6) — so a dispatcher at each package's *public* API would
not be sufficient; every internal cross-package call would need triplication.

**And Go itself does not do this.** Go selects files at build time via build constraints and compiles
exactly one platform; `GOOS` is a compiled-in constant, not a runtime query. Option (d) has no precedent in
the source language, contradicts the faithful-conversion rule, and is the shortcut the nothing-throwaway
principle exists to refuse.

---

## 10. Validation implications

- **The differential oracle needs a matching host.** `runCommandWithTimeout` exports `GOOS`/`GOARCH` to
  both sides, including the `go test -json` baseline; `go test` for a foreign `GOOS` builds a binary it
  cannot execute. A Linux corpus is validated on Linux (WSL is sufficient and already proven for the F2/F3
  work); macOS needs real hardware or CI. This is plan §A2.3 and it is unchanged by anything here.
- **The existing roster mostly transfers.** Of the **110** validated packages, only **6** are
  platform-varying (`crypto/rand`, `internal/sysinfo`, `mime`, `os/exec/internal/fdtest`, `path/filepath`,
  `time`); **15** are fully closure-clean. The converted C# *test sources* for the other 104 are
  platform-neutral and transfer unchanged.
- **Verdicts, however, must be re-earned per platform.** A package with identical test source still runs
  against a different closure (a different `os`, a different `runtime`), so a Windows pass is not evidence
  of a Linux pass. The honest accounting is one roster *per platform*, sharing sources.
- **Proof pages gain a platform dimension.** `docs/validation/<version>/<pkg>.<os>.md`, as §A4 already
  proposes. The measurement supports it for a reason worth stating: a proof page describes the binary being
  shipped, and (a) ships three binaries per varying package.
- **Two behavioral tests are Windows-semantic by construction** (`LocalTimeZone`, `FindFirstFileData`) and
  need a platform marker in the runners' enumeration rather than deletion — plan §A2.5.

---

## 11. Recommendation

**Layout L3 + packaging (a).** One tree, one solution, one package-ID set; 37 packages carry per-GOOS
sources and RID-specific assemblies; 265 ship exactly as they do today.

Honest trade-offs:

| | L3 + (a) |
|:--|:--|
| Corpus size | +29.4 % files (1,665 → 2,154) |
| Package IDs | unchanged (~304) |
| Consumer change | **none** — same `PackageReference`, same `dotnet run` |
| Converter work | `-platforms` list, three-way emission compare, conditioned csproj blocks |
| Pack work | N build passes + asset merge in `push-nuget.ps1` |
| Truthfulness of the compile surface | exact for 300 of 302 packages; `syscall` and `log/syslog` open |
| Rebank cost | one regeneration, but every subsequent CNR compares three emissions |
| Reversibility | high — L3 is additive; a single-platform build is `-platforms` with one value |

**The two seams this design does not close**, stated plainly rather than hidden in the recommendation:

1. **`syscall`'s compile surface.** 270 names in common out of 992/2,186/1,899. Candidate resolutions, in
   the order this lane would try them: (i) let `lib/net9.0/syscall.dll` carry the *host-of-record* flavor
   and document that portable code must not reference `go.syscall` directly — which is already true of Go,
   where `syscall` is frozen and `os` is the portable API; (ii) per-GOOS package IDs for `go.syscall`
   *alone*, the one place where the sprawl is honest; (iii) a unix-intersection reference assembly. Option
   (i) is cheapest and matches Go's own guidance; it needs a user ruling because it is a public promise.
2. **`log/syslog`** exports nothing on Windows. A neutral `lib/net9.0` reference carrying the Unix surface
   would let Windows code compile against APIs that cannot run. Simplest honest answer: ship the empty
   Windows surface as the compile asset, so the package is unusable at compile time on Windows exactly as
   it is in Go.

---

## 12. Staged migration — the first increment is small and provable

Each stage lands green on its own and is gated by an instrument that already exists.

**Increment 1 — a three-target census the converter itself produces. No emission change. — ✅ LANDED
2026-08-08 (lane r47d, against `223e4ffd3`).**
`-platforms` now accepts a comma-separated **list**, and the new `-platform-census <dir>` runs the pipeline
once per target into its own staging root, classifies every emitted artifact (shared / variant / partial /
exclusive) and writes `<dir>/platform-manifest.json`. Nothing is written into `src/core`: `-go2cspath` is
read as the SEED each staging root is copied from and never as an output, and no emitter was touched — CNR
is byte-identical across all 574 behavioral packages. A `-platforms` list *without* `-platform-census` is
rejected rather than silently converting the first target; multi-platform **emission** is increment 3.

Three mechanisms carry this document's own reasoning into the code:

- the classification compares **emissions**, never Go file sets (§4.2 is the reason);
- every staging root is **seeded** — `core` + `version.props` + `docs/validation`, mirroring `src/` — and is
  wiped and re-seeded per run, so CLAUDE.md's reconvert ritual and r41's never-convert-twice-into-one-root
  rule are mechanical rather than remembered. The manifest carries the **marker gate** per target (the
  hand-owned files the seed held, and any the run emitted as a plain `.cs` — which must be zero), so a
  seeding that did not take cannot be mistaken for a platform finding;
- emitted-vs-seeded is decided by a sentinel **modification time**, not by a content diff: the control
  target's emission is *supposed* to reproduce the seed byte for byte, so a content discriminator would
  report the control as having emitted nothing.

*Proof — the manifest reproduces this document's numbers.* Two censuses, five conversions (859 s + 522 s),
`-platforms windows/amd64,linux/amd64,darwin/amd64` and `-platforms darwin/amd64,darwin/arm64`:

| Measured here | § | Manifest | |
|:--|:--|:--|:--|
| Union of emitted `.cs` names 1,928 | §4 | 1,928 | ✔ |
| Byte-identical on all three 1,489 | §4 | 1,489 | ✔ |
| Same name, different content 69 | §4 | 69 | ✔ |
| Emitted by exactly two 88 | §4 | 88 | ✔ |
| Platform-exclusive 282 | §4 | 282 | ✔ |
| Packages with any delta 37 | §4 | 37 | ✔ |
| The 69 split 38 source / 27 `package_info.cs` / 4 `package_init.cs` | §4.3 | 38 / 27 / 4 | ✔ |
| Windows emitted 1,665 · Linux emitted 1,734 | §4 | 1,665 · 1,734 | ✔ |
| W↔L 1,563 both / 1,498 identical / 65 differ / 102 W-only / 171 L-only / 34 packages | §4 | all six | ✔ |
| W↔D 1,566 / 1,499 / 67 / 35 packages | §4 | all four | ✔ |
| L↔D 1,633 / 1,597 / 36 / 20 packages | §4 | all four | ✔ |
| L3 pricing: flat 1,489 + per-GOOS 665 = 2,154, against 1,665 (+29.4 %) | §8 | all five | ✔ |
| Packages queued 304 / 302 / 303 / 301, **zero** failures in every run | §2 | all four | ✔ |
| Arch axis 1,701 both / 1,687 identical / 14 differ / 32 + 31 exclusive / 16 packages | §5 | all six | ✔ |
| `.csproj` rewritten: Windows 0, Linux 21 | §4 | 0, 21 | ✔ |
| `.csproj` rewritten: "the macOS run 26" | §4 | **22** at `darwin/amd64`; 26 is `darwin/arm64` | corrected |
| "22 packages have a different direct import set" | §4 | **24** | corrected |

The two corrections are recorded in §4 with their evidence; both are `.csproj` counts and neither touches a
`.cs` number or any conclusion. One further disagreement is worth naming: §7 counts **41** files carrying a
line-anchored `[module: GoManualConversion]`, while the census — which asks the question with the
converter's *own* predicate, the one that actually decides whether a file's emission is diverted — counts
**40**. The odd file is `runtime/runtime2_impl.cs`, whose marker the predicate cannot see because an earlier
`//` comment line contains `*g/*p`, and `/*` opens a phantom block comment the file never closes. It is
inert today (an `*_impl.cs` companion has no Go counterpart, so that path is never probed) but it is the
exact shape that would silently clobber a whole-file hand-own; filed as its own fix rather than smuggled
into a census increment.

Cost, for the record: two new converter files and three edited lines of flag plumbing — no emitter, no
corpus file, no golden.

**Increment 2 — L3 for one package: `internal/goos`. — ✅ LANDED 2026-08-08 (lane r48a).**
Four files, no dependents' *surface* change (§6: 20 exported names, identical on all three), and it is the
clearest possible illustration. `src/core/internal/goos/{goos.cs, package_info.cs}` stay flat;
`{windows,linux,darwin}/` gain `zgoos_*.cs` and `unix.cs`/`nonunix.cs`. The conditioned `<Compile Include>`
line is exercised for real.

The corpus change is produced by the converter, not by hand:

```powershell
go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 `
      -go2cspath <repo>\src internal/goos          # 156 s; -platform-stage <dir> keeps the staging roots
```

That run seeded three staging roots from the corpus, converted the package once per target (marker gate
0/0/0, and the Windows control's 4 emitted `.cs` **all four reproduced the seed** — the same null reading
that certifies increment 1's instrument), classified the emissions, and merged: 6 artifacts written into
`{windows,linux,darwin}/`, 2 left flat unchanged, 2 stale flat copies removed, 1 project file given the L3
block. It reported no per-target `.csproj` disagreement, which §4's own census predicts — `internal/goos`
imports nothing.

*Proofs.* Method and blind spots stated, because "identical IL" is not a thing a byte compare can say on
its own:

| Proof | Method | Result |
|:--|:--|:--|
| `-p:GoTargetOS=windows` produces what today's package does | Symmetric A/B: the flat package with its pre-change `.csproj` and the L3 package built from equal-depth scratch roots, then a sorted reflection dump of each assembly — every type, every member, every **static member's runtime value**, and **every method body's IL bytes** | **Identical.** 56/56 dump lines match, IL included |
| …at the binary level | SHA-256 of the two `.dll` | **74 of 324,096 bytes differ (0.023 %)**, in 6 runs confined to the deterministic-identity fields — PE stamp, two 16-byte GUIDs (MVID and PDB), and the 32-byte PDB content checksum. Those hash the compilation inputs *including each source document's path*, and the L3 path carries a `windows\` segment. No IL, metadata or member ordering moved — the dump above is what proves that, and it would have caught a token shift |
| the property ABSENT is the same build | SHA-256 of the `.dll` | **byte-identical** to `-p:GoTargetOS=windows`. The default is exactly `windows`, not merely equivalent |
| `-p:GoTargetOS=linux` compiles and `GOOS` reads `"linux"` | Build, then read the built assembly at RUNTIME through the reflection probe (the field is `static readonly`, so the value comes from the executed `.cctor`, not from the source) | **`GOOS = linux`, `IsUnix = True`, `IsLinux = 1`, `IsWindows = 0`** |
| the Windows lane did not move | Seeded full single-target reconvert per CLAUDE.md's ritual (304/304 packages, 276 s), classified path-precisely against the committed corpus | **4,063 files both sides, 0 new, 0 absent, 12 content differences — all 12 the `.cs.auto` review siblings the overlay rule freezes** (CleanupBacklog item 18; the same 12 §2's control reports). Marker gate: 41 line-anchored hand-owns, **0** violations. `internal/goos` does not appear: layout adoption reproduced it file for file |
| the corpus still builds | `dotnet build src/go2cs-stdlib.slnx -c Debug` with `$(GoTargetOS)` unset | **304/304, 0 errors, 176 s** |
| nothing else in the converter moved | `check-no-regression.ps1` (574 behavioral packages, 493 s) plus a `git status` of the behavioral tree for `.csproj` | **byte-identical**, and no project-file drift |

*Blind spot, named:* the reflection dump reads a loaded assembly, so it proves surface, values and IL
bodies but not the metadata table ORDER those bodies are laid out in. Here the order provably did not
move — a reordering shifts metadata tokens, and the tokens are inside the IL bytes the dump compares.

One measurement artifact to expect from here on: a package in layout L3 is a package a single-target run
reproduces only *because* of adoption, so the census's `.csproj`-rewritten column stays at zero (the
seeded project file already carries the block, and `writeProjectFile` re-adds it) — but a census taken
against a root that was NOT seeded would now differ for that package. Seed, as the ritual already says.

**Increment 3 — L3 for the 37 measured packages, plus the conditioned `ProjectReference` blocks, plus
per-GOOS `package_info`/`package_init`. — ✅ LANDED 2026-08-08 (lane r49a, against `ae8c07f2d`).**

One command, three real conversions, 555 s:

```powershell
go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -go2cspath <repo>\src
```

**37** packages carry per-GOOS sources, **21** carry conditioned `<ProjectReference>` groups (not 22 — see
§4's correction), 36 project files gain the `$(GoTargetOS)` block (`internal/goos` already had it), and the
solution grows **304 → 307**: the three platform-exclusive packages no single-target run could ever list.
Nothing in the bank is a content change to converted C# — 174 flat `.cs` MOVE into `<pkg>/<goos>/`, 94 files
are new, 33 `.csproj` gain their blocks, and that is the entire set of real hunks. Each of the 174 was
checked against its committed bytes at its new `<pkg>/windows/` path: **174 preserved, 0 mismatched**.

Four mechanisms this increment had to add that §8's list did not name, each because the corpus reads itself:

1. **`package_info.cs` has READERS.** It is closure-derived, so L3 routes it per-GOOS in **33** packages —
   the **27** whose content varies (§4.3) plus the **6** platform-exclusive ones, whose every artifact is
   per-GOOS by definition; `package_init.cs` lands per-GOOS in **7**. And the converter
   reads its dependencies' copies to mint `<ImportedTypeAliases>` and to learn their `[assembly: GoImplement]`
   records. Asking flat would have found nothing and fallen through to the derived-alias path: no error, no
   warning, a quietly different closure in every dependent. `platformPackageInfoPath` mirrors
   `platformLayoutDir` for readers (flat wins; the other 275 pay one `os.Stat`).
2. **`stdlibmeta.Collect`** keys sections by directory, so per-GOOS copies would have arrived as `os.windows`
   with no `os` at all, silently emptying the record `-recurse=nuget` depends on. It now folds the GOOS
   segment away and keeps the `ReferenceGOOS` flavor, since that asset describes a *published* assembly and a
   published assembly presents one compile surface (§9(a)). Proven by the existing sync test, which
   regenerates from the L3 tree and still matches the committed asset byte for byte.
3. **The union solution**, regenerated from the merged corpus — without it `-p:GoTargetOS=linux` cannot
   resolve a Linux-only reference at all.
4. **Facts that are not references still need reconciling**, because one `.csproj` serves every platform:
   `<AllowUnsafeBlocks>` is the **union** (it differs in `os/user` and `syscall`, permissive on Windows in
   both, so the rule is byte-neutral here — and it is the polarity the corpus does *not* have that would
   break a Linux build), and every other companion is taken from the **first target in `-platforms` order**
   rather than the first that re-emitted, which was silently landing `os/exec/internal/fdtest/README.md` in
   its Linux flavor.

*Proofs.*

| Proof | Method | Result |
|:--|:--|:--|
| the property ABSENT is the same build | SHA-256 of every assembly | **All 306 byte-identical** to `-p:GoTargetOS=windows`, generator output included |
| **the Windows lane did not move** | The pre-L3 corpus restored IN PLACE (same paths — a different root would change the PDB/MVID path hashes and prove nothing), built, and compared to the L3 `-p:GoTargetOS=windows` build by a semantic digest: assembly identity + assembly-level attributes + every AssemblyRef + every type, field, method, signature, **constant** and **method-body IL**, with MVID/PDB/PE-stamp excluded | **286 of 303 semantically identical.** The 17 that differ are all L3 packages, and for every one the compile inputs are the identical file set with identical bytes and the generated code is identical as a multiset — 15 byte-for-byte, and `os`/`runtime` identical as sorted content, the same promotions landing in differently-named generated files. What moved is **declaration ORDER** — see the seam below |
| the corpus still builds | `dotnet build src/go2cs-stdlib.slnx -c Debug`, property unset | **307/307, 0 errors, 149 s** |
| §13.1, the IL question | see §13.1 | **54 of 54 measurable shared-source packages identical** |
| **the first Linux corpus compile** | `dotnet build … -p:GoTargetOS=linux` | **8 errors, 2 buckets, ONE package** — see below |
| the Windows lane reproduces from a reconvert | Seeded single-target `-stdlib` per CLAUDE.md's ritual (307 projects, 202 s), compared path-precisely | **3,576 files both sides, 0 new, 0 absent, 0 content differences.** Layout adoption reproduced all 37 L3 packages file for file. Marker gate: **41** line-anchored hand-owns, **0** violations |
| nothing else in the converter moved | `check-no-regression.ps1` (574 behavioral packages, 419 s) | **byte-identical**; solution integrity 576/576, path casing 4,142/4,142 |
| the behavioral corpus still passes | `run-behavioral.ps1` (649 s) | **549/549** transpile+compile+golden, **523/523** stdout vs `go run`, 26 skipped |
| the converter's own suite | `go test ./...` | **green**, 52 s |

**The Linux buckets, honestly.** The build reaches **57** packages and stops at exactly one: `runtime`.

| Bucket | Count | Where |
|:--|--:|:--|
| `CS0103` — `The name 'locked' does not exist` | 6 | `runtime/lock_sema_impl.cs` |
| `CS7036` — missing argument for `Ꮡgp` of `notetsleep_internal` | 2 | `runtime/linux/lock_futex.cs` |

The remaining 249 packages are **skipped as dependents**, not errored — the standard bucketing reading. Both
buckets are ONE root cause, and it is the one **§7 predicted in writing**: `lock_sema.go` is selected on
Windows *and* macOS while Linux uses `lock_futex.go`, so "`runtime/lock_sema_impl.cs` needs a sibling, not a
port." The measurement adds the mechanism §7 could not know: **a hand-owned `*_impl.cs` companion of a
per-GOOS file is not routed by L3 at all.** The classifier works on *emissions*, and an `*_impl.cs` has no Go
counterpart, so it is never emitted, never classified, and stays flat — where Linux compiles it against a
`lock_sema.cs` that is not in its build. That is increment 3.5's first item, and it is a layout rule
(hand-owned companions inherit their principal's folder), not a Go-conversion defect.

**Two seams this increment names rather than closes.**

1. **L3 changes compile-item ORDER, and metadata order is observable.** One alphabetical `*.cs` glob becomes
   `*.cs` followed by `$(GoTargetOS)/*.cs`, so a package's platform files now sort after its shared ones
   instead of interleaving. No code changes — but the source-generator hint-name disambiguation suffix can
   attach to the other member of a case-colliding pair, and the metadata table order shifts, which shifts the
   tokens inside IL. 17 packages are affected; 20 of the 37 L3 packages are unaffected. This is **not fixable
   within L3** — no MSBuild glob can interleave two directories alphabetically — so the honest acceptance
   standard for a layout change is *semantic content* identity, not byte identity. Worth a ruling: member
   order is unspecified in C# but is observable through reflection enumeration and through the relative order
   of `[GoInit]` module initializers, so if any of that is load-bearing, an explicit sorted `<Compile>` list
   is the only remedy — at the cost of every project file naming every file.
   **RULED (user, 2026-08-08): semantic-content identity IS the acceptance standard for layout changes** —
   types, members, signatures, constants and method-body IL, as increment 3's dump measures them — with the
   full 110-package validated sweep run against the L3 corpus as the empirical check that nothing
   order-sensitive (package init sequencing above all) actually moved. The explicit sorted `<Compile>`-list
   remedy is rejected as machinery no observable behavior demands; if a future defect is ever traced to
   member-enumeration or `[GoInit]` relative order, that evidence reopens this ruling, not silently.
   **The empirical check has since run: 110/110 packages, 13,628 verdicts, 0 FAIL against the L3 corpus**
   (2026-08-08, sweep at the increment-3 train head) — `os` and every other ordering-affected package's
   suite passed at banked counts, so init sequencing and member enumeration provably did not move for any
   behavior the validation net observes.
2. **`log/syslog`'s `InternalsVisibleTo`.** Its Go source is entirely excluded on Windows, so there are no
   sibling internal test files and the block is absent there while present on the unix side. Taking the union
   would add an assembly-level attribute to a shipped Windows assembly, so the merge keeps the Windows
   remainder and reports it. It costs the Linux `-tests` path for one package nothing validates. The general
   question — should L3 grow a *conditioned property* axis alongside its conditioned references? — is a
   design decision, not a measurement.

**Increment 3.5 — the hand-owned files get a platform, and the Linux corpus gets measured. — ✅ LANDED
2026-08-08 (lane r50a, against `850a85faa`).**

*The layout half.* L3 classifies **emissions**; a hand-owned file is never emitted, so it was never
classified, so it kept whatever placement it had while its principal moved per-GOOS. The rule L3 was
missing: **a hand-owned file belongs in exactly the platform builds its PRINCIPAL takes part in**, and then
L3's own placement rule applies to that set unchanged — every platform ⟹ flat, a subset ⟹ one copy per
platform in the subset. Stating it as a platform SET rather than "the folder the principal is in" is what
keeps `os/proc_impl.cs` and `syscall/syscall_impl.cs` flat: their principals are per-GOOS *variants*
present on all three platforms, so folder-inheritance would triplicate one hand-written file into three
copies to maintain in lockstep, for no compile benefit. A principal comes in two shapes, both already
recorded in the tree by the emission itself — an `*_impl.cs` supplements `<name>.cs`; a marked whole-file
hand-own's principal is its own `<name>.cs.auto` review sibling, which is emitted by exactly the platforms
that compile the Go file the hand-own replaces.

Census, measured: **33** `*_impl.cs` and **41** line-anchored `[module: GoManualConversion]` files (the two
sets overlap — a marked file can also be an `_impl` companion). **19** distinct hand-owns sit in one of the
37 L3 packages: **6 were misplaced**, and the other 13 were already correct, for the platform-set reason
above. The same census re-taken *after* the move counts **20**, because `runtime/lock_sema_impl.cs` is now
two files — the same file-versus-hand-own distinction that takes the marker gate from 41 to 42.

| Hand-own | Principal | Emitted by | Moved to |
|:--|:--|:--|:--|
| `os/dir_windows_impl.cs` | `os/dir_windows.cs` | windows | `os/windows/` |
| `os/file_windows_impl.cs` | `os/file_windows.cs` | windows | `os/windows/` |
| `runtime/lock_sema_impl.cs` | `runtime/lock_sema.cs` | windows, **darwin** | `runtime/windows/` **and** `runtime/darwin/` |
| `syscall/zsyscall_windows_impl.cs` | `syscall/zsyscall_windows.cs` | windows | `syscall/windows/` |
| `syscall/dll_windows.cs` (+`.cs.auto`) | `syscall/dll_windows.cs.auto` | windows | `syscall/windows/` |
| `syscall/exec_windows.cs` (+`.cs.auto`) | `syscall/exec_windows.cs.auto` | windows | `syscall/windows/` |

The last two are the ones the `.cs.auto` binding buys: whole-file hand-owns of Windows-**only** Go files,
flat in the corpus and therefore latently in every Linux build for the whole of increment 3 — unreached
only because the build stopped at `runtime` two layers below. Every moved file keeps its committed bytes
(8 of 9 are git-detected `R100` renames; the ninth is the second `lock_sema_impl` copy, hash-verified equal
to the other two). The reconvert side needed no change at all: `conversionDriver` already probes the marker
and writes the `.cs.auto` through `platformLayoutPath`.

*Guarded by a walk of the REAL corpus*, because the next offender will be a file somebody adds by hand and
no synthetic tree can see it. Three structural rules: an `*_impl.cs` whose principal is in SOME but not all
of its package's per-GOOS folders must be in exactly those; a `.cs.auto` lives beside the `.cs` it reviews;
and a source carrying Go's own GOOS filename constraint is never flat in an L3 package — that third rule is
the one that sees a *marked* hand-own, whose principal a static walk cannot otherwise find. Neutered-fix
control: copying `exec_windows.cs` and `lock_sema_impl.cs` back to flat fires rules 3 and 1 respectively.

*The measurement half.* With the layout fixed, `runtime` fails **differently** — and correctly. The
companion routing removes the Windows note/lock implementation from the Linux build without supplying a
Linux one, which is exactly §7's "needs a sibling, not a port", now demonstrated rather than predicted.
So the honest unscaffolded reading is **58 compile, 1 fails, 248 skipped** — barely past increment 3.

To measure the packages *behind* that wall the build was walked forward behind **five throwaway
scaffolds**, in the same spirit as this document's other throwaway probes (census D, the §13.1 walker).
None is committed; all five were removed before any gate ran, and every scaffolded body throws. They are
listed because a number measured behind a scaffold must say so:

| # | Scaffold | Stands in for |
|--:|:--|:--|
| 1 | `runtime/linux/lock_futex_impl.cs` (throwing) | the missing Linux mutex/note hand-own (§7) — **REAL since increment 3.5b; no longer a scaffold** |
| 2 | `syscall/linux` `_Socklen` made public | bucket **L2** below |
| 3 | `syscall/linux` `GoImplicitConv` `ValueType` corrected | bucket **L3** below |
| 4 | `os/linux` duplicate `GoImplement` record removed | bucket **L4** below |
| 5 | `os/linux/dir_unix_impl.cs` (throwing) | the missing Linux `readdir` hand-own (§7) — **RETIRED at increment 3.5b: the registry entry was narrowed and Linux needs no hand-own** |

**The Linux buckets, with classifications.** Each row is the leaf-most failure of one build; peeling it
revealed the next. `(a)` = L3-mechanical, `(b)` = hand-own flavor gap, `(c)` = converted-code or generator
Linux-flavor defect.

| # | Package | Bucket | Class | Root cause |
|--:|:--|:--|:--:|:--|
| L0 | `runtime` | 6× `CS0103` `locked`, 2× `CS7036` | **(a)** | The companion-routing gap itself. **FIXED — this increment.** |
| L1 | `runtime` | 54× `CS0103` (`notewakeup`, `notesleep`, `notetsleep_internal`, `lock2`, `unlock2`) | **(b)** | `manualConversionFuncs` is name-keyed and platform-blind; the only implementation is `lock_sema_impl.cs`. Needs `runtime/linux/lock_futex_impl.cs`, with `notetsleep_internal` at **two** arguments. §7 |
| L2 | `syscall` | 1× `CS0050` | **(c)** | `Sockaddr.sockaddr()` returns unexported `_Socklen`. `collectMethodSignatureUnexportedTypes` **does** walk a named interface's underlying methods, but `collectSignatureUnexportedTypes` early-returns on `!method.Exported()` — and a C# interface member is implicitly **public** regardless. One gate, wrong for interfaces. |
| L3 | `syscall` | 1× `CS0030` | **(c)** | `ImplicitConvGenerator` emits `new WaitStatus((WaitStatus)src.Value)` for the inverse of `WaitStatus`↔`ΔSignal`. The `ValueType` the converter records is the target NAMED type instead of its underlying primitive (`uint32`); harmless while both sides share an underlying, fatal when they do not. Windows never sees it — `WaitStatus` is a struct there, so the pair is never registered. |
| L4 | `os` | 20× `CS8130`, 12× `CS8183`, 10× `CS0111`/`CS0102`, 4× `CS8646`, `CS0246` | **(c)** | `os/linux/package_info.cs` records `unixDirent → DirEntry` **twice** — once through `os`'s local alias (Go `type DirEntry = fs.DirEntry`) and once through canonical `io/fs.DirEntry` — and both derive the SAME adapter name, so `ImplementGenerator` emits it twice. Type-alias canonicalization is missing from the implement-record path. Windows records only `dirEntry → fs.DirEntry` once, so it never fires. Related and not identical: the converted call site in `file_unix.cs` names a **third** spelling, `unixDirentжfs_DirEntry`, that neither record produces — so converter and generator disagree on adapter naming for an aliased interface. |
| L5 | `os` | 1× `CS0246` `unixDirentжfs_DirEntry` | **(c)** | The naming disagreement above, isolated once the duplicate is gone. |
| L6 | `os` | 3× `CS0029`, 3× `CS8716` | **(c)** | `zero_copy_linux.cs` returns `(default!, "")` where the tuple element is the **named string type** `poll.String`; a bare `""` has no conversion to it, and the failed element leaves `default` with no target type. The Go zero value of a named string type is emitted untyped. |
| — | 150 packages | — | — | still blocked above `os` (`fmt`, `net`, `log`, `testing`, `go/*`, `crypto/*` …) |

**Nothing in the surface is L3-mechanical beyond L0.** That is the finding worth carrying: the layout is
now correct for every package the build can reach, and everything remaining is either a hand-own that was
never written for Linux **(b)** or a converted-code/generator defect that Windows structurally cannot
exhibit **(c)**. None is a *layout* question, so increment 4 is not blocked on this document.

**Standing.** 58 packages compile unscaffolded (was 57); **156** compile behind the five scaffolds,
against 305 of 306 on Windows with zero errors. *(Superseded by increment 3.5b below: 143 compile
unscaffolded, and 307/307 behind four class-(c) scaffolds.)*

> **All four class-(c) defects are FIXED — lane r51a, 2026-08-08.** L2 and L3 in `ad6378b88`, L4/L5
> and L6 in `eef90b3f1`. Two findings reshape the table above. **L5 was never independent:** the
> duplicate record of L4 is what made `adapterNameCollisionSet` see a FALSE collision and qualify one
> cast site to the third spelling, so removing the duplicate removes the third name — one root cause,
> two rows. And **L3's root was wider than syscall:** every one of the corpus's 49 `ValueType`
> records named the constructed type instead of its backing primitive, so the form was never right,
> only never yet fatal — `int`/`uint` native-width wrappers are exactly the case the generator's
> compensating override deliberately declined, which is why `WaitStatus`(uint32)←`Signal`(nint) was
> where it finally bit. Correcting the record required moving one consumer with it: the generator's
> `uintptr` hop read `ValueType` as the type to CONSTRUCT.
>
> With the four fixes plus scaffolds for the two remaining class-(b) hand-own gaps (L1's futex
> note/mutex and `os.File.readdir`, both lane r51b's), **the whole corpus compiles on Linux: 307
> projects, ZERO errors, 154 s at `--no-incremental`** — so the class-(c) defects were the only
> converter-layer blockers in the entire Linux surface, and the 150-package tail above was blocked
> solely by what sat under it. Control that this is a real Linux compile rather than a mis-targeted
> one: injecting a syntax error into `os/linux/zero_copy_linux.cs` fails the build, and `log/syslog`
> — whose Go source is excluded on Windows in its entirety — produces a 346 KB assembly.
>
> Windows moved, deliberately and by classification: of the 22 files a three-target seeded reconvert
> changed, 14 are the `ValueType` correction (which spans every flavor, `runtime/windows` included)
> and one is `go/types/object.cs`, where the sealed-interface accessibility rule publicizes `Δcolor`
> on purpose — it had only ever compiled because its `Δ` collision-rename made the generator's
> name-based scope rule read it as exported by accident. Corpus build property-absent: **307
> projects, 0 errors**.

*Proofs.*

| Proof | Method | Result |
|:--|:--|:--|
| the Windows lane did not move | `-p:GoTargetOS=windows` over the whole solution | **305/306, 0 errors, 149 s** (the 306th is `crypto/x509/internal/macos`, darwin-exclusive) |
| the property ABSENT is the same build | SHA-256 of every assembly, property unset vs `-p:GoTargetOS=windows`, **both `--no-incremental`** | **305/305 byte-identical** |
| …and the byte compare is a real instrument | Same flags, two full recompiles | **305/305 byte-identical** — the build IS byte-reproducible, so the row above is a measurement rather than a tautology |
| the Windows lane reproduces from a reconvert | Seeded single-target `-stdlib` per CLAUDE.md's ritual (304 packages, 192 s), compared path-precisely | **4,540 files both sides, 0 new, 0 absent, 0 content differences.** Layout adoption reproduced all 37 L3 packages *including the newly routed hand-owns*, and re-emitted `dll_windows.cs.auto` / `exec_windows.cs.auto` into `syscall/windows/` on its own |
| the hand-owns were not clobbered | Marker gate, line-anchored | **42** marked files, **0** re-emitted as a plain `.cs` |
| §13.1, the IL question, re-taken at full width | see §13.1 | **141 of 141 measurable shared-source packages identical** (was 54) |
| nothing else in the converter moved | `check-no-regression.ps1` (574 behavioral packages, 439 s) | **byte-identical**, `.csproj` included; solution integrity 576/576, path casing 4,142/4,142 |
| the behavioral corpus still passes | `run-behavioral.ps1` (613 s) | **549/549** transpile+compile+golden, **523/523** stdout vs `go run`, 26 skipped |
| the converter's own suite | `go test ./...` | **green**, 56 s |

⚠ **Two measurement traps this increment walked into, both worth inheriting.** *First:* the marker census
reads **42**, not 41, and that is correct — `runtime/lock_sema_impl.cs` now exists in **two** folders, so the
count of marked FILES exceeds the count of distinct hand-owns for the first time. CLAUDE.md's standing rule
(re-measure, never assert last session's number, explain a move in either direction) covers it exactly.
*Second:* `--no-incremental` is **not byte-neutral**. Comparing an incremental build against a
`--no-incremental` one reported all 305 assemblies as differing, which reads exactly like a broken
determinism guarantee; holding the flag constant reports 305/305 identical, and the same-flags control above
is what tells those two readings apart. **An A/B byte compare must hold `--no-incremental` constant**, and a
byte compare that skips the control can only ever be trusted downward.

**`log/syslog`'s `InternalsVisibleTo` — a PROPOSAL, deliberately not an implementation.** Increment 3 left
this as the one project-file difference layout L3 cannot express, and increment 3.5 confirms the shape:
every emitted `.csproj` carries `<InternalsVisibleTo Include="$(AssemblyName).tests" />`, but `log/syslog`'s
Go source is excluded on Windows in its entirety, so the Windows emission has no internal-test surface and
no such item, while the unix emissions do. The merge keeps the Windows remainder and reports it.

The general question — should L3 grow a **conditioned PROPERTY/ITEM axis** beside its conditioned
references? — has a clean answer in shape:

```xml
<ItemGroup Condition="'$(GoTargetOS)'=='linux'">
  <InternalsVisibleTo Include="$(AssemblyName).tests" />
</ItemGroup>
```

built by the same `splitPlatformReferenceSets` decomposition the reference axis already uses: intersect the
targets' item sets, emit the intersection unconditionally, emit one conditioned group per platform for the
remainder, **and write an EMPTY group for a platform with no delta** — for exactly the reason §4 records
about references, that a platform which loses its recorded membership takes the next reconvert's
intersection down to two and promotes a one-platform item into the shared list.

**It is proposed rather than implemented because it is not mechanically trivial, and the bar was that it
be both trivial and provably Windows-unmoved.** Adding the block to `log/syslog`'s project file is inert
for the Windows *build* (the condition is false), but it is **not** inert for the Windows *corpus*: a
single-target Windows reconvert re-emits that `.csproj` from the Windows emission, which has no such item,
and would strip the block — so the axis only works once `platformProject.go` can round-trip it the way it
round-trips references, including the empty-group rule. That is a real piece of machinery, not an edit.
Its cost today is one package's Linux `-tests` path, which nothing validates; its value arrives with
increment 4's second build pass. Two facts to carry into that work: `<AllowUnsafeBlocks>` is *already*
reconciled by a different rule (union, §12 increment 3 note 4), so the axis must not re-open it; and the
merge's residual report is the instrument that will say when a THIRD such fact appears.

**Increment 3.5b — the class-(b) hand-own flavor gaps close, and the registry grows a platform axis. —
✅ LANDED 2026-08-08 (lane r51b, against `88c4d082a`).**

Increment 3.5 classified the Linux surface into (a) layout, (b) hand-own flavor gaps and (c) converted-code
or generator defects, and closed (a). This closes **(b)**, both rows of §7's table — and the two rows wanted
**opposite** answers, which is the finding worth carrying.

*L1, the `runtime` mutex/note cluster (27 unresolved call sites across `notewakeup`, `notesleep`,
`notetsleep_internal`, `lock2`, `unlock2`).* §7 said "a sibling, not a port". True, and understated: the two
flavors differ in their OS primitive (semaphore vs futex) and their key encoding (`{0, locked|*m}` vs
`{0,1,2}`), and **the OS primitive is exactly the part that does not survive conversion**. Both therefore
collapse onto the identical managed model — a `{0, keyLocked}` latch driven by `Interlocked` with `SpinWait`
escalation. So the shared core moved OUT of the Windows companion into a flat, platform-neutral
`runtime/lock_managed_impl.cs`, and each flavor keeps only the one declaration whose signature differs:

| File | Contents |
|:--|:--|
| `runtime/lock_managed_impl.cs` | NEW, flat — `mutexContended`, `lock2`, `unlock2`, `notewakeup`, `notesleep`, `noteSleepDeadline`, `keyLocked` |
| `runtime/{windows,darwin}/lock_sema_impl.cs` | 4-arg `notetsleep_internal`, delegating (byte-identical copies) |
| `runtime/linux/lock_futex_impl.cs` | NEW, 2-arg `notetsleep_internal`, delegating |

Flat rather than tripled is increment 3.5's own platform-SET rule applied to a hand-own that *can* be
shared. The 4-vs-2 arity §7 flagged as inexpressible turns out to need no expression: the registry says
whether a declaration is hand-owned, the hand-owned file says what it looks like.

*L-os, `File.readdir`.* The opposite. `dir_windows.go` reinterprets the buffer
`GetFileInformationByHandleEx` fills as a Go struct and must be hand-owned; `dir_unix.go`'s readdir is
**pure Go** over `internal/poll`'s `ReadDirent` and converts faithfully. The name-keyed entry deleted that
body too — so the Linux gap was **invented by the registry, not by the Go source**, and the right fix is to
narrow the entry rather than write a stub. Linux now emits a real 126-line converted `readdir` and owes
nothing. Darwin stays scoped IN on evidence: `dir_darwin.go` hands libc's `readdir_r` the address of a Go
`syscall.Dirent` — the same non-blittable-by-address seam — so its hand-own is owed with macOS at
increment 5.

*The registry's platform axis.* `manualConversionFuncs` entries now carry a `goosScope`, resolved against
the conversion's target GOOS; the empty scope (`goosAny`) means every target and is what ~120 of the ~126
entries use. `syscall`'s five generated wrappers and `os.readReparseLink` are scoped to windows too — inert
there already, so no emission moves, but a future same-named unix declaration can no longer silently
inherit a Windows hand-own the way `readdir` did. Guarded by `manualConversionScope_test.go`: the
`lock_sema`/`lock_futex` pair at real arity, the readdir three-way split, and a typo guard, since a scope
naming an unknown GOOS matches nothing and would turn a hand-own off everywhere — otherwise unreportable,
because "not hand-owned" is a legitimate answer for every other declaration.

**The bucket table, re-read.** L0 and L1 are gone; every remaining row is class (c) and belongs to the
converter/generator, not to this document.

| # | Package | Class | State |
|--:|:--|:--:|:--|
| L0 | `runtime` companion routing | (a) | **FIXED** increment 3.5 |
| L1 | `runtime` mutex/note | **(b)** | **FIXED** — this increment, no scaffold |
| L2 | `syscall` `CS0050` `_Socklen` | (c) | open (lane r51a) |
| L3 | `syscall` `CS0030` `WaitStatus`↔`ΔSignal` | (c) | open (lane r51a) |
| L4 | `os` duplicate `GoImplement` record | (c) | open (lane r51a) |
| L5 | `os` `unixDirentжfs_DirEntry` naming | (c) | open (lane r51a) |
| L6 | `os` `zero_copy_linux.cs` typed zero string | (c) | open (lane r51a) |
| — | `os` missing Linux `readdir` | **(b)** | **FIXED** — the entry was narrowed; no hand-own owed |

**Standing.** Scaffolds drop from **five to four**, and all four are class (c). Unscaffolded, the Linux
build's leaf-most failure moves from `runtime` to `syscall` (L2): **143 of 307 projects compile**, up from
58, with the other 164 sitting behind `syscall` in the reference graph. Behind r51a's four class-(c)
scaffolds — **and no hand-own scaffold at all** — the corpus compiles **307 of 307, zero errors** (108 s).
That is the number this increment exists to produce: with (a) and (b) both closed, *nothing else* in the
entire Linux corpus is a layout or hand-own question.

*Proofs.*

| Proof | Method | Result |
|:--|:--|:--|
| the Linux `runtime` compiles | `-p:GoTargetOS=linux`, no scaffolds | **0 errors** (from 27 `CS0103`) |
| the Linux `os` compiles | `-p:GoTargetOS=linux`, behind r51a's four class-(c) scaffolds only | **0 errors** — no `readdir` scaffold |
| the whole Linux corpus | full solution, same four scaffolds | **307/307, 0 errors, 108 s** |
| …and the flag is really applying | scaffolds reverted, same command | fails at exactly L2, 1 error — a control, not a tautology |
| the Windows lane did not move | full solution, property ABSENT, `--no-incremental` | **307 projects, 0 errors, 181 s** |
| `runtime.dll`'s Windows semantics did not move | order-insensitive surface digest (type/member/accessibility/arity/IL-size/locals/EH), `-p:GoTargetOS=windows`, `--no-incremental` both sides | **diff is exactly 4 lines** (below) |
| the corpus reproduces from a reconvert | seeded 3-target `-stdlib` merge (559 s), path-precise | 6,137 files both sides, **0 new, 0 absent, 1 real content difference** — `os/linux/dir_unix.cs`, the intended one; the other 55 are CR-strip-equal |
| the hand-owns were not clobbered | marker gate, line-anchored | **44** marked files (was 42), **0** re-emitted as plain `.cs` |

The `runtime.dll` digest is worth quoting, because it is what makes "the body moved" a measurement:

```
compiler-generated closure classes  1653 -> 1653     (ordinal names shift; count is the test)
go-derived surface lines          17258 -> 17260     (+2, the two intended additions)

<= METHOD notetsleep_internal  p4  il196
=> METHOD notetsleep_internal  p4  il13      now a delegation
=> METHOD noteSleepDeadline    p2  il196     the SAME 196-byte body, relocated
=> METHOD get_keyLocked        p0  il7
```

`mutexContended`, `lock2`, `unlock2`, `notewakeup` and `notesleep` do not appear in the diff at all — same
IL size, locals flag and exception regions after changing file.

⚠ **A raw IL/signature-blob digest cannot serve as the instrument here, and the first attempt proved it.**
Both signature blobs and IL bodies encode type and member references as *coded indices into the metadata
tables*, so relocating a member rewrites every one of them without changing any meaning: a byte-hash
comparison reported **15,408** differing lines for a change that moved five method bodies. That is the same
effect §12's increment-3 note predicted ("the metadata table order shifts, which shifts the tokens inside
IL"), and it is the concrete reason the ruled acceptance standard for a layout move is semantic-content
identity rather than byte identity. An order-insensitive surface digest reports 4.

⚠ **The 55 CR-only differences after the reconvert are the documented mixed-CRLF/LF phantom**, not drift:
CR-stripped equality holds for every one of them. Worth stating because the count is much larger than a
single-target reconvert's, and because it is exactly the shape a real regression would hide in — classify
by CR-stripped comparison, never by file count.

**Increment 4 — packaging (a) for the RID pair `win-x64` / `linux-x64` only. — ✅ LANDED 2026-08-08
(lane r52a, against `a3fb17170`).**

`push-nuget.ps1` builds and packs `go2cs-stdlib.slnx` once per RID and merges the flavors into a single
nupkg per package. **307 packages packed: 37 merged with RID-specific assets, 270 copied verbatim from the
reference pass.** Package IDs are unchanged and so is everything a consumer writes. The layout decisions and
their justification against NuGet's asset selection are recorded in §9(a) above; this section is what was
measured. **No corpus file moves — the whole increment is the script layer.**

```
go.os/
  lib/net9.0/os.dll                       459,776 bytes   compile asset (Windows flavor)
  runtimes/win-x64/lib/net9.0/os.dll      459,776 bytes
  runtimes/linux-x64/lib/net9.0/os.dll    464,896 bytes
```

*The two platforms, run.* A portable framework-dependent console app — no `RuntimeIdentifier` anywhere —
with a single `<PackageReference Include="go.fmt" />`, restored from a folder feed with nuget.org cleared and
`NUGET_PACKAGES` redirected (version 1.23.1.4 is already published, so without both a restore would
silently satisfy `go.fmt` from the public feed or the machine cache and prove nothing):

| | Windows | Linux (WSL, Ubuntu 22.04, SDK 9.0.316) |
|:--|:--|:--|
| restore | ✔ 59 packages | ✔ 59 packages |
| build | ✔ 0 warnings, 0 errors | ✔ 0 warnings, 0 errors |
| **run** | **✔ byte-exact vs `go run`** — `48 65 6C 6C 6F 2C 20 57 6F 72 6C 64 21 0A` on both sides | **✘ throws in a module initializer** — see below |

**The RID mechanism itself is proven, and by measurement rather than by inference.** Under `COREHOST_TRACE`
the Linux host resolved **13 of 13** RID-split assemblies in `fmt`'s closure — `os`, `syscall`, `runtime`,
`internal.poll`, `internal.goos`, `internal.filepathlite`, `internal.runtime.syscall` and the rest — from
`runtimes/linux-x64/lib/net9.0/`, overriding the RID-agnostic Windows assembly sitting in the application
root. So option (a) does what §9 says it does on a consumer that names no platform.

**The Linux run failure is the opening data point of the run-layer campaign, and it is a clean one.**

```
System.TypeInitializationException: The type initializer for 'go.syscall_package' threw an exception.
 ---> System.NotImplementedException: runtime_envs: external (assembly or cgo) function is not implemented
   at go.syscall_package.runtime_envs()
   at go.syscall_package..cctor()      <- syscall.init()
   at go.os_package..cctor()           <- os.init()
   at go.fmt_package.os_FileжReader.ᴛRegisterAdapter()
   at go.Program.Main()
```

- **Bottommost frame's package: `syscall`, Linux flavor** — and that is a second, independent proof that the
  RID-matched assembly loaded, because `runtime_envs` is declared *only* in `syscall/{linux,darwin}/env_unix.cs`.
  Windows has no such declaration at all, so this failure is unreachable from the Windows flavor.
- **Not a missing implementation — a missing wiring.** `syscall/linux/env_unix.cs` declares
  `internal static partial slice<@string> runtime_envs();` and `PartialStubGenerator` fills the bodyless
  partial with a throw, while `runtime/linux/runtime.cs:120` already carries the implementation
  `syscall_runtime_envs()` under `//go:linkname syscall_runtime_envs syscall.runtime_envs`. What is absent is
  the cross-package `//go:linkname` binding between them.
- **It is the first thing any program touching `os` or `fmt` does** — `envs` is a package-level variable of
  `syscall`, so this fires during package init, before any user code runs. It is a gate, not a leaf.
- The failure is **not** any of the shapes the increment was told to watch for: no `DllNotFoundException`, no
  `PlatformNotSupportedException`, no hang. The Linux corpus loads, initializes, and stops at one named
  declaration.

*Proofs.*

| Proof | Method | Result |
|:--|:--|:--|
| **the Windows lane did not move** | Pre-change single-pass pack (property absent) vs the merged feed, **at the same commit**, compared entry-by-entry by content hash over all 307 packages | **270 platform-neutral packages differ in `_rels/.rels` and nothing else**; the 37 L3 packages gain `runtimes/<rid>/…` and 15 gain nuspec dependencies. **`lib/` assemblies changed: 0.** README/VALIDATION/icons changed: 0 |
| …and `_rels/.rels` is not a payload difference | It is the OPC part reference naming `…/core-properties/<guid>.psmdcp`, whose file name is a fresh GUID on every pack | Differs between *any* two pack runs, including two runs of the unmodified script |
| the corpus still builds | `dotnet build src/go2cs-stdlib.slnx -c Release`, `$(GoTargetOS)` unset | **307 projects, 0 errors, 164 s** |
| the derived RID-split set is the measured one | L3 tree scan (per-GOOS folder without a project file) | **37**, matching §4 |
| the dependency union is real | `go.os.nuspec`, merged | 19 shared + `internal.byteorder`, `internal.goarch`, `internal.stringslite`, `internal.syscall.unix` — **both `internal/syscall` flavors in one nuspec**, as §9(a) requires |
| …and the union count is explained | 21 packages carry conditioned `<ProjectReference>` groups (§12 increment 3); 15 of them have at least one Linux-only reference | **15 nuspecs change**; the other 6 (`internal/filepathlite`, `internal/syscall/execenv`, `internal/testpty`, `net/internal/socktest`, `runtime/pprof`, `time`) have Linux groups that are subsets of their Windows set |
| §11 seam 2 is visible in the artifact | `go.log.syslog` | `lib/net9.0` and `runtimes/win-x64` carry the 322,560-byte Windows assembly with no Go source in it; `runtimes/linux-x64` carries the real 346,112-byte one |
| the merge does not corrupt the package | `.nuspec` re-read after rewrite | well-formed XML, UTF-8 BOM preserved byte for byte |

⚠ **Two measurement traps, both new, both worth inheriting.**

*First: a BYTE COMPARE CANNOT TEST FOR A PLATFORM DIFFERENCE, and the first implementation of the
neutral-package fail-safe proved it by calling **216 of 270** platform-neutral packages "different".* Roslyn's
deterministic identity fields — PE timestamp, MVID, PDB id, PDB content checksum — are hashes of the
compilation, and they **cascade**: an assembly whose identity moved shifts every dependent's identity too,
with no source file and no semantic byte changed. `internal/oserror`, whose only converted dependency is
`errors`, moves because `errors` moved. Measured: **~72 bytes of a ~340 KB assembly, in four runs, with the
assembly LENGTH unchanged** — the same signature §12's increment-3 proof recorded for a path-hash shift.
Meanwhile two *same-flavor* packs at the same commit (property absent vs `-p:GoTargetOS=windows`) agree on
**every `lib/` assembly of all 307 packages**, so the build is reproducible and the cascade is the whole
explanation. The sound discriminator is therefore **the entry set plus the assembly
length**: identity fields are fixed-size, so a pure identity difference can never move it, and the test has no
false positives — which is what a release gate needs. It is a smoke alarm, not a semantic digest; §13.1's
digest remains the instrument that answers the IL question in full, and it is a per-increment measurement
rather than a per-release one.

*Second: THE PACK EMBEDS THE GIT COMMIT, so a packed-output byte A/B is only valid WITHIN ONE COMMIT.* The
SDK's built-in SourceLink writes `<repository … commit="…"/>` into every `.nuspec` and the commit into every
assembly's debug data, so an A/B taken across the increment's own commit reported all 307 packages' `lib/`
assemblies as differing — indistinguishable at a glance from a real regression. The Windows-lane proof above
was re-taken with both sides built at the same HEAD. Same lesson as increment 3.5's `--no-incremental` trap,
one layer out: **hold everything that feeds the hash constant, including the commit.**

⚠ **A release-time cost worth knowing: `-p:UseSharedCompilation=false` is a MEASURED necessity, not
concurrency hygiene.** `go2cs-gen` runs inside the compiler, and with the shared VBCSCompiler every project's
generator work funnels through one server process: the same clean Release build of this solution measures
**~165 s** with csc per project and **had not finished after 14 minutes** without it (one core busy, 24
MSBuild nodes idle). Two RID passes make that the difference between a 6-minute release and an hour-long one.
`--no-incremental` joins it on every pass — both so pass N cannot inherit pass N-1's assembly from the shared
`obj\` (what differs between passes is the `<Compile>` ITEM SET, not any source timestamp) and because that
flag is not byte-neutral. The passes also run in **reverse** order so the reference flavor is built last and a
developer's tree is left holding the same assemblies a plain property-absent build produces.

`-SkipBuild` is now rejected with an explanation rather than silently packing one flavor: there is no single
on-disk build that holds them all.

**Owed to increment 5, beyond adding `osx-arm64` to the RID map** (which is a one-line change, and only
there): the unshipped-RID fallback in §9(a) point 1 — a `runtimes/linux/…` arch-agnostic RID would cover
linux-arm64, and is the natural place to take it up alongside §5's arch axis.

**Increment 4.5 — the run layer, opened and walled. — 📄 RECORDED 2026-08-08 (lane r52b)**
Increment 4's first Linux run got as far as `NotImplementedException: runtime_envs` inside `syscall`'s type
initializer. That was **not** a per-GOOS-layout defect, which is what it looked like from here: it was a
converter shape gap in the `//go:linkname` PUSH matcher, which accepted only the modern one-arg-handle
consumer and not the bare bodyless shape `syscall` and `os` still use. Fixed, guarded, and A/B'd at five
emitted files. The loop then advanced one step and hit its real wall — `internal/runtime/syscall.Syscall6`,
the raw kernel boundary every Linux syscall funnels through, and the single implementation-class item
between this corpus and a running Linux program. Full measurement log, the catalog for that one
declaration, and the harness that makes the loop cheap to resume:
**[`FINDING-linux-run-layer.md`](FINDING-linux-run-layer.md)**. Read its §1 warning before iterating — a
single-package conversion into an L3 corpus regenerates its `.csproj` from a single-platform view and must
not be banked.

**Increment 5 — macOS, then the second architecture.**
§4 measures macOS as 20 packages' distance from Linux, and §5 measures the arch axis at 16 packages; both
are additions *by value* to a mechanism already proven, exactly as the big-three ruling intends.

#### Amendment 2026-09-24 — the compile surface follows the conversion target (the 1.24.13.2 Linux train)

§9(a) chose `lib/{tfm}` as a package's compile asset and filled it with ONE designated flavor (windows),
and it named the consequence: "the compile surface is truthful for all but two packages". The README
walkthrough measured what that exception costs a consumer on Linux (COORD FINDING 086853edd5): a
`-recurse=nuget` conversion of `fatih/color` reaches `golang.org/x/sys/unix`, which calls
`syscall.Rlimit`, and compiled against `go.syscall`'s `lib/` (byte-identical to the win-x64 flavor, no
`Rlimit`) while the linux-x64 flavor that would LOAD had the type all along. NuGet has no RID-selected
compile group: `project.assets.json` resolves compile = `lib/` in the RID-less graph and in
`<tfm>/linux-x64` alike. §9(a)'s layout is unchanged; four pieces now close the seam around it:

1. **A RID-selected compile asset** (`src/core/golib/buildTransitive/go.lib.targets`). After
   `ResolvePackageAssets`, each go.* compile item whose package ships a `runtimes/<rid>/lib/<tfm>/` twin
   is swapped to that twin with its `HintPath`. Every converted package depends on go.lib, so it reaches
   every consumer, and on win-x64 the twin IS `lib/` (a no-op, byte-identical). Gate:
   `src/tests/PackageTests/RidCompileAsset`.
2. **Per-GOOS metadata records** (`stdlib-metadata.txt`, `##<name>@<goos>`). The converter's embedded
   record had carried only the reference flavor, on §9(a)'s own premise; a linux conversion now reads the
   linux flavor's exported aliases and GoImplement records.
3. **One platform for both halves.** `-recurse=nuget` writes the conversion's target into the output root's
   `Directory.Build.props` as a conditioned `GoCompileRuntimeIdentifier` default, which item 1 honors first,
   so the compile flavor is the one the metadata was read for rather than the build host's.
4. **Third-party assembly trampolines.** A pure-JMP `.s` block outside GOROOT with a `types.Identical`
   target becomes a forwarder (`asmTrampolines.go`), so x/sys/unix's `Syscall` family runs instead of
   throwing; `syscall.prlimit` joins the linkname forward targets.

§9(a)'s "consumer changes: none" still holds. What changes is that the compile surface is now the
conversion target's own flavor wherever the release ships one; a platform with no shipped twin
(linux-arm64, osx) still compiles the reference flavor, as before.

---

## 13. What this design does **not** answer

Recorded so nobody mistakes a measurement for a guarantee.

1. ~~**IL identity is assumed, not measured.**~~ **MEASURED 2026-08-08 (increment 3, lane r49a): identical,
   for every package the Linux build can currently reach.** §4 measures the *converter's* output; the worry
   was that `go2cs-gen` might generate differently against a Windows- vs Linux-flavored dependency, since the
   generators read `[assembly: GoImplement]` records from referenced `package_info` assemblies and 27 of
   those vary.

   Method: build the whole solution at `-p:GoTargetOS=windows` and at `-p:GoTargetOS=linux`, then compare
   each package's assembly with the semantic digest described in §12 (types, members, signatures, constants
   and method-body IL bytes, plus the AssemblyRef set — deliberately included, since a conditioned
   `<ProjectReference>` block is exactly what changes it — with MVID, PDB id and PE stamp excluded).

   **RE-TAKEN AT FULL WIDTH 2026-08-08 (increment 3.5, lane r50a). The answer did not move; the evidence
   behind it nearly tripled.**

   | | Increment 3 | **Increment 3.5** |
   |:--|--:|--:|
   | Compiled under **both** flavors | 57 | **156** |
   | Semantically identical across flavors | 54 | **142** |
   | Differing | 3 | 14 |
   | of those, **shared-source** packages | 54 of 54 | **141 of 141** |

   The decomposition is what makes the row exact. Of the 156 packages that compile under both flavors, **15
   are L3** — packages whose own source varies by platform — and 141 are shared-source. **All 14 differing
   packages are L3**, and every one of the 141 shared-source packages is identical. Not one exception.
   The 15th L3 package, `crypto/x509/internal/macos`, is *identical* for a reason worth stating rather than
   rounding away: it is darwin-**exclusive**, so neither the Windows nor the Linux build compiles any of its
   sources and both produce the same assembly — a null reading that behaves exactly as it should.

   So **no package with shared source produced different IL against a differently-flavored closure**, now
   over 141 packages instead of 54: §9(a)'s "265 packages ship one `lib/{tfm}` assembly" holds for
   everything measurable, and increment 4 owes RID-specific assets only to the packages whose source
   already varies.

   **The honest limit, restated:** 141 of ~265. The Linux build stops above `os` (§12), so ~124 remain
   unmeasurable, and `runtime`/`os`/`syscall` were reached only behind the throwaway scaffolds §12 lists —
   all three are source-differs packages, so the scaffolds cannot have manufactured any of the 141
   identical verdicts. The instrument is again a small `System.Reflection.Metadata` walker, throwaway like
   census D's probe and not committed.
2. ~~**Whether the Linux corpus compiles is still unmeasured — and cannot be measured today.**~~ **ASKED AND
   ANSWERED 2026-08-08 (increment 3): 57 packages compile, one package fails with 8 errors in 2 buckets, 249
   are skipped as its dependents.** The single failing package is `runtime`, from a single root cause §7 had
   already predicted (`lock_sema` is Windows+macOS; Linux uses `lock_futex`), and the mechanism is a layout
   rule L3 does not yet carry — a hand-owned `*_impl.cs` companion is never *emitted*, so the
   emission-based classifier never routes it into its principal's per-GOOS folder. Detail and buckets in §12.
   **Increment 3.5 closed that layout gap and re-asked the question: 58 compile unscaffolded, 156 behind five
   throwaway scaffolds, and the full bucket table with (a)/(b)/(c) classifications is in §12. Nothing left in
   the surface is a layout question.**
   The prediction below was exactly right about *why* the question needed L3 first, and this is what it looked
   like when it could finally be asked. The original note follows.

   A seeded
   reconvert produces a *union* tree (the Linux emission laid over a seeded Windows tree), and the emitted
   csproj globs `*.cs` flat, so building it would compile both platforms' files and fail on duplicate
   definitions. Pruning it correctly *is* the layout question this document answers. That is why the
   compile number lives in Increment 3 and not here: **L3 is a prerequisite for asking it, not a
   consequence of knowing it.** Plan Arc 4's "one afternoon" estimate assumed option 1's separate tree; L3
   reaches the same number without one.
3. **`syscall`'s public contract** (§11 seam 1) needs a user ruling, not a measurement.
   **RULED (user, 2026-08-08): option (i) — the neutral `lib/net9.0/` slot carrying the Windows
   host-of-record assembly is an acceptable default.** Increment 4's de-facto layout is ratified as the
   design; the silent-fallback consequence for unshipped RIDs remains increment 5's arch-agnostic
   `runtimes/linux` item, softened rather than reopened by this ruling.
   **Refined same day: the fallback must be LOUD.** A one-time stderr warning fires when the loaded
   flavor's baked `GOOS` constant disagrees with the actual host OS — which is true exactly and only in
   the unshipped-RID case (a resolved RID asset always matches its host; Windows-on-Windows always
   matches). Mechanism: a golib helper taking the baked constant, called from a flat platform-neutral
   file in `runtime`'s init path so every flavor carries the identical check and the neutral `lib/` slot
   stays byte-identical to `runtimes/win-x64`. Implementation queued directly behind the in-flight
   Syscall6 lane (same package — sequenced, not raced). **Same day, the run-layer ruling:
   `internal/runtime/syscall.Syscall6` is implemented as the libc `syscall(2)` keystone P/Invoke**
   (FINDING-linux-run-layer.md option A) — one gate lighting the whole generated surface, mirroring Go's
   own single-funnel asm architecture, with the errno-convention conversion inside the shim.
4. **cgo.** Every census here ran `CGO_ENABLED=0`. Real Go on Linux resolves `os/user` and `net` through
   cgo by default; the pure-Go paths measured here are the same class of decision as the standing
   `-tags purego` ruling, and should be confirmed as such rather than assumed.
5. **The Windows-semantic behavioral tests and the runners' platform gating** are scoped in plan §A2.5/§A5
   and are not re-derived here.

---

## Appendix — reproducing the measurements

**Censuses B and C are now one command each** (increment 1). The converter does the seeding, runs the
targets sequentially into isolated staging roots, and writes the manifest every number in §4, §5 and §8
above is read from:

```powershell
go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 `
      -go2cspath <repo>\src -platform-census <scratch>\three-way   # census B (§4)

go2cs -stdlib -comments -platforms darwin/amd64,darwin/arm64 `
      -go2cspath <repo>\src -platform-census <scratch>\arch        # census C (§5)
```

`-go2cspath` is the SEED and is never written to; the manifest lands at
`<scratch>\<name>\platform-manifest.json`. The hand-rolled form the original measurements used is kept
below, since census A still needs it:

```powershell
# Four seeded conversions (sequential; never two converter processes at once).
# Seed each root with src/core + src/version.props + docs/validation, mirroring src/.
go2cs -stdlib -comments -platforms windows/amd64 -go2cspath <root-w>/src
go2cs -stdlib -comments -platforms linux/amd64   -go2cspath <root-l>/src
go2cs -stdlib -comments -platforms darwin/amd64  -go2cspath <root-d>/src
go2cs -stdlib -comments -platforms darwin/arm64  -go2cspath <root-a>/src
```

```bash
# Census A — per-GOOS source file sets.
GOOS=$OS GOARCH=amd64 CGO_ENABLED=0 GO111MODULE=off \
  go list -e -tags purego,math_big_pure_go \
  -f '{{.ImportPath}}|{{join .GoFiles ","}}|{{join .SFiles ","}}|{{join .CgoFiles ","}}' std
```

Censuses B/C compare the emitted `.cs` of two roots, discriminating emitted from seeded files by
modification time — that comparison is no longer a throwaway probe but `src/go2cs/platformCensus.go` +
`platformManifest.go`, which stamp the seed with a sentinel time so "emitted" is exact rather than
inferred. Census D enumerates `types.Package.Scope()` exported names via `go/packages` under each `GOOS`;
its probe source was throwaway and was not committed. The commands above are sufficient to regenerate every
number in this document.

<!-- {% endraw %} -->
