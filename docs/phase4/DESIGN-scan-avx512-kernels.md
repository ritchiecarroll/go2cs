# DESIGN — internal/runtime/gc/scan's AVX-512 kernels: read the hardware as Go does, and run them

**Lane R, 2026-11-25.** COORD GO 20261125T165433Z, (a) and (b) as one cut. P3 reads this note before the cut. The finding
is P2's 2026-10-16 BOARD row ("three AVX512 tests skip in C# where Go runs them — FEATURE GAP"), re-met by D2/T6 as its
finding 5. That row's root and retirement path are the ones below. Read at master `1c556c35a5` and go1.26.11's source.

## 1. What Go does, and what the converted package does

- Go: `CanAVX512()` returns `avx512ScanPackedReqsMet` (scan_amd64.go:37), which is the AND of `HasAVX512VL`,
  `HasAVX512BW`, `HasGFNI`, `HasAVX512BITALG`, `HasAVX512DQ` and `HasAVX512VBMI`. On a host with all six, the three
  tests `TestExpandAVX512`, `TestFilterNilAVX512` and `TestScanSpanPackedAVX512` run the assembly kernels against the
  package's references: 6,166 subtests on today's hosted AVX-512 runners.
- Converted, two layers:
  1. `internal/cpu/amd64/cpu_x86_impl.cs` fills `cpu.X86` from System.Runtime.Intrinsics in a module initializer.
     .NET 10 has no `Avx512Bitalg`, so `HasAVX512BITALG` stays false and `CanAVX512()` is false on every host.
  2. The kernels are Go assembly (scan_amd64.s, filter_amd64.s, expand_amd64.s; 29 TEXT routines, 2,817 lines) and
     emit as bodiless partials, whose generated stub throws. Layer 1 alone would turn the three skips into crashes.

## 2. The cut

**(a) The flag, BITALG only.** In `initX86FeatureDetection`, after the mapped flags:
`X86.HasAVX512BITALG = X86.HasAVX512F && isSet(ecx7, cpuid_AVX512BITALG)`, where `ecx7` comes from `cpuid(7, 0)`.
- That is doinit's own line (cpu_x86.go:178-185), under the same `HasAVX512F` gate. Here `HasAVX512F` is
  `Avx512F.IsSupported`, which already folds in the OS's AVX-512 state support (Go's `osSupportsAVX512`).
- `cpuid()` already has a managed body (`X86Base.CpuId`).
- BITALG is not a GODEBUG option in Go's table, so the option list is untouched.
- The other nine unmapped flags stay false. Each is named on the BOARD with the asm path it gates (section 5).

**(b) Managed bodies for the three kernels,** in one new hand-owned companion,
`src/core/internal/runtime/gc/scan/amd64/scan_amd64_impl.cs` (the `cpu_x86_impl.cs` pattern). Each body returns what
the assembly returns, read from the assembly itself:

| kernel | contract (from the .s) | body |
|---|---|---|
| `ExpandAVX512(sizeClass, packed, unpacked)` | OVERWRITES all 16 words of `*unpacked` (two 64-byte stores) with the expansion of `packed` for the size class | zero `*unpacked`, then `ExpandReference`'s loop |
| `scanSpanPackedAVX512(mem, bufp, objMarks, sizeClass, ptrMask)` | in 64-byte frames, in order: for each word whose bit is set in expand(objMarks) AND ptrMask, copy `mem[word]` to `buf`, contiguously and zeros included; return the count | one loop over the span's `PageWords` words with those two tests. No nil test: `FilterNilAVX512` removes the zeros after |
| `FilterNilAVX512(bufp, n)` | an ORDER-PRESERVING compaction of the non-zero words (VPCOMPRESSQ, then a scalar tail); return the new length | one stable compaction loop |

- `ScanSpanPackedAVX512 = FilterNilAVX512(scanSpanPackedAVX512(...))` is emitted Go and stays as emitted.
- The scan test needs the reference's exact ORDER (`slices.Equal`), and these bodies give it. The filter test compares
  as a multiset, so stable compaction meets it with room to spare.
- Not reproduced: the scan's full 64-byte store, and the filter's merge-masked store, each write unspecified lanes past
  the returned count. No caller reads them, and no test does. Stated, not emulated.
- Plain loops, not `Vector512`. .NET has no BITALG (VPOPCNTB). `ExpandAVX512` is "Test-only" in Go's own source.
  `scanSpanPackedAVX512` is reached only from Go's garbage collector, which the converted runtime does not run.
  So readability wins at no cost a program can see.
- The emitted `gcExpandersAVX512` table (asm addresses) stays as it is, unused.

**Not changed:** the converter, go2cs-gen and golib. It is one hand-owned file added and one hand-owned file edited.
Re-emission is untouched, so no corpus footprint and no CNR movement. Only the amd64 flavours compile the new file;
linux-arm64 has no amd64 folder there.

## 2a. Attribution: the owner's direction of 2026-11-25 (COORD 20261125T170034Z, standing rule 170054Z)

Every file in the cut that takes code or behavior from Go source opens with three parts, in this order:
1. Go's notice, verbatim from the source file.
2. A provenance block that says "derived from" (a port of the algorithm) or "inspired by" (the same behavior in its own
   shape), naming each Go file by its GOROOT-relative path, the Go version (go1.26.11) and the functions followed.
3. go2cs's own header, which keeps the file's license and authorship for the parts that are original.

LICENSES/BSD-3-Clause.txt is the license of record. Where a file mixes ported and original code, the block says which
part is which.

**`scan/amd64/scan_amd64_impl.cs` (new).** Go's notice is `Copyright 2025 The Go Authors. All rights reserved.` plus
the BSD line, the same in all four source files below.

| body | relation | Go source, go1.26.11 |
|---|---|---|
| `ExpandAVX512` | DERIVED FROM, the loop | `src/internal/runtime/gc/scan/expand_reference.go`, `ExpandReference` |
| `ExpandAVX512` | INSPIRED BY, the overwrite-all-16-words contract | `src/internal/runtime/gc/scan/scan_amd64.s`, `ExpandAVX512` |
| `scanSpanPackedAVX512` | DERIVED FROM, the per-word selection | `src/internal/runtime/gc/scan/scan_reference.go`, `ScanSpanPackedReference` |
| `scanSpanPackedAVX512` | INSPIRED BY, in-order and zeros-included with no nil test | `src/internal/runtime/gc/scan/scan_amd64.s`, `scanSpanPackedAVX512` |
| `FilterNilAVX512` | INSPIRED BY: the same order-preserving result, as a scalar loop | `src/internal/runtime/gc/scan/filter_amd64.s`, `FilterNilAVX512` |

`FilterNilAVX512` is not derived from `filter.go`'s `FilterNil`, which swaps from the end and does not keep order.

**`internal/cpu/amd64/cpu_x86_impl.cs` (edited).** Go's notice is `Copyright 2017 The Go Authors. All rights
reserved.` plus the BSD line, from `src/internal/cpu/cpu_x86.go`. The file already mixes, and its block names the ported
parts:
- the new BITALG line: DERIVED FROM `doinit`'s `X86.HasAVX512BITALG = isSet(ecx7, cpuid_AVX512BITALG)` under
  `HasAVX512F` (cpu_x86.go:178-185);
- `registerX86Options`: DERIVED FROM `doinit`'s option table;
- `deriveX86Features`: DERIVED FROM `doinit`'s `doDerived`;
- `cpuid`'s managed body: INSPIRED BY `cpu_x86.s` `cpuid`.

The rest is original: the intrinsics mapping, the module-initializer stand-in for `schedinit`, `GoCpuAppliesGodebug`
and the GolibTests probe. The rule applies to any file a seat touches this way, so the block covers the whole file, not
only the new line.

**GolibTests' new class.** It calls the kernels and Go's references. Its object-mask shapes follow
`src/internal/runtime/gc/scan/scan_test.go`'s `testObjs`, so it carries the same three parts, with `testObjs` named as
INSPIRED BY.

P3: please check each header against its source file: the notice text and year, the path, and the function names.

## 3. Red-first, and the near-miss

1. **Kernels, locally.** This box has no AVX-512 (Ryzen 7 PRO 6850U), and the managed bodies need none. GolibTests gains
   one class, which needs a ProjectReference to `internal.runtime.gc.scan`. For every packed size class (size up to
   `MinSizeForMallocHeader`), over the same object-mask shapes as `testObjs`, it calls `ExpandAVX512`,
   `ScanSpanPackedAVX512` and `FilterNilAVX512` directly against `ExpandReference`, `ScanSpanPackedReference` and
   `FilterNil`'s multiset. RED at master: each call throws in its stub. GREEN with (b).
2. **The flag, locally.** A row asserts `CanAVX512() == (Avx512F.IsSupported && cpuid BITALG && GFNI && VBMI && VL &&
   BW && DQ)`. Here that reads false: the near-miss.
3. **The near-miss in Go's own terms:** the sweep row on this box (windows) and on a non-AVX-512 hosted linux runner.
   The three tests still SKIP on both sides, by Go's own condition.
4. **Hosted AVX-512:** sweep-shard `internal/runtime/gc/scan` on linux, dispatched until a runner draws AVX-512 (2 of 3
   did today). Read when Go and C# both run the 6,166 subtests and every one matches. A draw without AVX-512 is
   reported as the near-miss reading, never as the proof.

## 4. Roster: the linux count becomes host-dependent

The row banks `linux: 3095` (linux-x64), the non-AVX-512 floor. After the cut an AVX-512 runner reads 9,261, and the
sweep today calls that COUNT drift. The existing `host-conditional (<why>): `Name`, ...` absorption is the right idea,
a banked floor plus named extra verdicts, but it does not reach this row as it stands:
- names match exactly, and `k` may not exceed the number of names (6,166 subtests);
- its proof reads the committed WINDOWS proof page, and on an OS-annotated row it rejects by design (run-validated-sweep.ps1,
  Get-HostConditionalVerdict: proof pages gain the OS dimension later).

Options (P3/COORD to rule; the validation-bank discipline applies either way, and nothing is hand-set):
- **R1 (recommended): a family form in the annotation.** In ``host-conditional (AVX-512 BITALG GFNI VBMI): `TestExpandAVX512/…`, …``,
  a name ending `/…` means the parent and its subtests. The sweep accepts floor+k only when every extra row is inside a named family,
  agrees on both runtimes, and no row outside the families changed or vanished, read from the live record. On linux
  the floor set comes from the row's banked linux evidence. Tooling: `_roster.ps1` (pattern), `run-validated-sweep.ps1`
  (family match and a linux floor source) and `check-roster-format.ps1` fixtures, red-first. A separate tools seat, so
  this cut stays small.
- **R2: two derived counts.** `linux: 3095 (linux-x64 AVX-512: 9261)`, with the sweep choosing by the Go oracle's own
  evidence (did Go run `TestExpandAVX512`'s subtests). This is a new annotation form and a new classifier bucket:
  more machinery than R1 for one row.
- Until either lands, the row's AVX-512 reading is recorded and not banked, as it is today (2026-11-12, P1-2).

## 5. BOARD, with the cut

The nine flags that stay unmapped, each with the asm path it gates in the corpus:
- `HasERMS` and `HasFSRM`: runtime's memmove choice (`cpuflags_amd64`, `useREPMOV`).
- `HasADX` and `HasSHA`: `crypto/internal/fips140deps/cpu` (the bignum and SHA assembly paths; the corpus is the purego
  flavour, so each needs its own consumer census).
- `HasRDTSCP`: runtime's `cputicks` in asm_amd64.s, through `offsetX86HasRDTSCP`.
- `HasVAES`, `HasAVX512VAES`, `HasAVX512VNNI` and `HasAVX512VPOPCNTDQ`: no reader outside `internal/cpu` in the corpus
  today. Vendored x/sys/cpu keeps its own flags, filled by its own `cpuid`.

Each one turned on is its own census, of what reads it and whether a managed body exists behind it.

## 6. Gates for the cut

- GolibTests, the new class red-first.
- The `internal/runtime/gc/scan` and `internal/cpu` rows under `run-validated-sweep.ps1 -Filter` on windows.
- The stdlib build, all four flavours, since the new file compiles in three of them.
- The hosted linux reading (section 3.4).
- Repoguard, at the cut's own tip.
- `go test ./...` is not owed: no converter change.

## 7. Folded from P3's read (APPROVE WITH CHANGES, COORD/20261125T172539Z-P3) and COORD's ruling (20261125T172751Z)

These folds bind the cut. Where sections 2-4 above say otherwise, this section wins.

1. **MUST 1: no window, and every amd64 cell.** R1 and the cut ride the SAME train, with R1 merged first, so no
   hosted sweep ever reads the row red between them.
   - They are two seats on one branch line: R1 (`claude/r-scan-avx512-r1`), then the kernels on top of it
     (`claude/r-scan-avx512`).
   - R1 covers EVERY amd64 OS cell of the row, not only linux. The banked windows count (19) moves on an AVX-512
     runner too, since every amd64 flavour compiles the bodies. So does darwin's, if an AVX-512 Intel runner draws.
2. **SHOULD 2: the family's exact size.** The annotation carries each family's size, derived from the evidence, e.g.
   ``host-conditional (AVX-512 BITALG GFNI VBMI): `TestExpandAVX512/…` (+N), …``.
   - The sweep accepts the floor, or the floor plus EXACTLY the sum of the family sizes, never any other k. Go runs
     none of a family or all of it, so an in-between count is a host that ran part of one, and it fails.
   - The parent row's only allowed change is skip to pass, on both sides.
3. **SHOULD 3: a zero-word arm.** Go's test fills the span with non-zero words, so its 6,166 subtests cannot tell
   "zeros included, then filtered" from "a nil test in the scan". GolibTests carries arms over a span with zero words
   under set mask bits:
   - the wrapped `ScanSpanPackedAVX512` equals `ScanSpanPackedReference` in order;
   - the raw `scanSpanPackedAVX512` count equals the popcount of expand(objMarks) AND ptrMask, with the zeros in place
     and in order. That reaches an internal method, so the scan test project's `InternalsVisibleTo` gains GolibTests,
     or the arm goes through reflection. Whichever is smaller, stated in the cut.
4. **NIT 4: which tail rule the scan body follows.** It follows the ASSEMBLY. Its word mask is objMarks EXPANDED (the
   same expansion `ExpandAVX512` gives, bounded by nObj) and AND-ed with ptrMask. The reference's per-word `objI`
   test is not bounded: for a mark bit at or past nObj, where the size does not divide the page (size 48: words
   1020-1023, object 170), the reference would scan words the kernel does not. No test sets such a bit. The header's
   "DERIVED FROM ScanSpanPackedReference" therefore names only the walk and the two-mask test; the mask itself is
   INSPIRED BY scan_amd64.s.
5. **SHOULD 5: getGOAMD64level.** `cpu_x86_impl.cs`'s block also names `getGOAMD64level`, INSPIRED BY `cpu_x86.s`
   `getGOAMD64level`: its body is that TEXT's fall-through constant, which the file's own comment quotes. One line
   says the flag set follows `doinit`'s.

P3's "could not confirm": the second non-test reader, `runtime/mgcmark_greenteagc.cs:964`
(`scan.HasFastScanSpanPacked()` in `scanSpan`).
- It is reached only through Go's own collector: `mallocgc` calls `gcStart`, which leads through the mark workers to
  `gcDrain` and then `scanSpan`.
- The converted runtime does not run that collector. `runtime/managed_impl.cs` says of mallocgc "which this host does
  not run (there is no Go heap)".
- So the flag changes nothing there today. If that path ever becomes live, it reaches the managed bodies, which
  return what Go's kernels return.
