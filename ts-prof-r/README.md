# TypeScript checkdemo profiles for P3's perf scout (R, 2026-11-20)

Scratch, not for master. These are raw tables for P3. R has not analysed them.

## The run

- **Program:** typescript-go-cs `6ba3f6b`, `checkdemo.exe`, Release JIT. Built here with the README's line against the
  published go.* 1.26.11.1 packages (`-p:GoStdLibVersion=1.26.11.1`). The artifact directory `538abe619327` is G's.
- **GC:** checkdemo's own runtimeconfig, server GC with DATAS off. `DOTNET_ROLL_FORWARD`, `GOFLAGS` and `DOTNET_gcServer`
  are unset.
- **Inputs:** G's kit `claude/g-tsbench-kit` `d5678617c9`, units-101 (108/108 SHA256SUMS verified). The 101 paths are
  passed as arguments, the working dir is empty, and the output hash is the same on every run (`3EB1134A0D5BFF02`, 405
  lines).
- **Launch:** the runner's way, a .NET `Process` with stdout redirected to a pipe the parent drains (`ts-profile.ps1`).
  Never an MSYS pipe (BOARD mm).
- **Box:** R-LAPTOP, Windows 11 x64, .NET SDK 10.0.400, runtime 10.0.11 (G's versions). Untraced wall about 94-113 s here
  (see `control.txt`).

## The profiler and the trims

- **Tool:** dotnet-trace 10.0.750501 (EventPipe; this shell is not elevated, so no ETW kernel sampling). Profiles:
  `dotnet-sampled-thread-time,gc-verbose`, run `tr1` (wall 100.9 s).
- **Work phase only.** dotnet-trace ATTACHES by PID right after `Process.Start`, so runtime start-up is not traced. Then
  every table is filtered by STACK, not by timestamp:
  - `tr1-work.txt`: samples and allocation ticks whose stack holds `main_package.main`, the whole work phase;
  - `tr1-check.txt`: stacks holding `GetSemanticDiagnostics` or `GetGlobalDiagnostics`, the type-check phase. This
    includes the binder, which runs lazily under `GetGlobalDiagnostics` (`Once.doSlow`).
  - The GC line in both files is the WHOLE trace; GC events carry no work-thread stack.
- **The sampler** is the runtime's thread-time sampler (about 100 Hz per thread). It samples a thread whether or not it is
  running. So `Thread.PollGC` (39% exclusive in the type-check phase) is the work thread SUSPENDED for a garbage
  collection, not CPU spent in PollGC. The work thread is the only thread with `main` on its stack, because checkdemo is
  `SingleThreaded`.
- **Allocations** are GCAllocationTick samples, one per ~100 KB allocated. MB figures are estimates, and the tick counts
  are samples, not objects.
- **Frame names:** `go.?\`1` is golib's `ж\`1`; the glyph is lost in the trace's module text.

## Files against P3's cuts

1. **FieldRefBox by (parent, TElem) with the converted caller:** the `--alloc-type` table in both files, as `type | golib:
   <first golib frame> | app: <nearest converted frame>`. The parent is the `ж\`1[Parent].viewOfSlow` frame. The CPU of
   viewOfSlow, publishView, CWT.GetOrCreateValue and viewsHead is in the QUERY blocks.
2. **Retention against churn:** `gcd1-live-families.txt`, live counts at 90 s from a heap snapshot (run `gcd1`, untraced);
   `gcd1-report.txt` is the full per-type report. dotnet-gcdump fell back to lossy buffering (it needs .NET 11 for
   non-lossy), so a large heap may be under-counted.
3. **ElemRefBox by T and caller:** the same `--alloc-type` table (ElemRefBox rows).
4. **Hash and equality:** QUERY blocks `ValueType.GetHashCode`, `ValueType.Equals`, `EqualityComparer`, `HashCode.Combine`,
   `GoEqualityComparer`, `builtin.AreEqual`. Each is split by the nearest `map\`2[K,...]` frame's K, and by the nearest
   converted caller.
5. **Coroutines:** QUERY `YieldFunctionEnumerator.*MoveNext`, inclusive samples and top converted callers. NOT a switch
   COUNT: EventPipe has no event for a Coro hand-off, so the count is not measured here.
6. **GC:** the `## GC` line. Run `tr1` (traced): gen0 30, gen1 17, gen2 16; pause 17.2 s = 17.2% of the trace; peak heap
   1,665 MB (sum of GCHeapStats generation sizes). In the gc-collect-only control run `srv-gc`: 18/11/10, pause 12.8%,
   peak 3,854 MB. The two peaks disagree by more than tracing load explains: read peak heap as unreliable from this
   instrument.
7. **CPU exclusive top 40** and **inclusive top 40, GOLIB frames only:** both files.
8. **Workstation control** (`control.txt`, 3 untraced runs each, alternating):
   - server: median 99.4 s (93.8 / 99.4 / 103.7);
   - workstation concurrent: median 158.6 s (151.0 / 158.7 / 158.6);
   - gc-collect traces: server pause 12.8%, gen 18/11/10; workstation pause 36.4%, gen 41/**1,929**/97.

## Scripts

`ts-profile.ps1` (launch, attach, optional gcdump), `ts-control.sh` (cut 8), `traceagg-Program.cs` + `traceagg.csproj`
(the aggregator, Microsoft.Diagnostics.Tracing.TraceEvent 3.1). Local paths are replaced by placeholders.

## Round 2 (2026-11-20, P3's cuts 9-12) and a CORRECTION to round 1

**CORRECTION, scored against round 1.** Round 1 read `Thread.PollGC` (39% exclusive in the type-check phase) as the work
thread suspended for GC, and gave tr1's GC pause as 17.2% of the trace. Both were wrong.
- All 6,551 type-check PollGC samples fall inside runtime suspensions with reason `SuspendOther`: 44,463 of them in tr1,
  16.8 s in total. Those are the sample profiler's own stack-walk suspensions, not GCs. A thread in a loop is caught at its
  poll, so the PollGC split by holder is ordinary CPU by method (P3's reading, as P3 predicted).
- The old pause figure summed every suspension, the sampler's included. Counting only GC-reason suspensions, tr1 shows
  332 ms, but tr1 cannot separate GC pause by reason: it has 63 collections and only 19 GC-reason suspensions, so
  collections ran inside the sampler's suspensions (the longest SuspendOther is 647 ms).
- The reliable GC figures are the sampler-free gc-collect runs (`control-gc-suspensions.txt`):
  - server: 13.2 s SuspendForGC + 0.3 s prep = 12.8% of the run, 39 collections;
  - workstation: 56.4 s + 0.8 s = 36.4%.
- The aggregator now counts only `SuspendForGC*` suspensions as GC pause, and prints every suspension reason.

**9 and 10, PollGC split** (`tr1-check-r2.txt`, `tr1-work-r2.txt`): by the first frame above the poll, and by the nearest
converted caller. Placement against suspensions: type-check 6,551 of 6,551 inside SuspendOther. Work phase: 25,198
inside SuspendOther, 2 inside SuspendForGCPrep, 6 outside any suspension.
- Type-check holders: `Dictionary<go.string,__Canon>.Resize` 46.2%, `Buffer.BulkMoveWithWriteBarrier` 42.0%,
  `Array.Copy` 2.7%.

**11, callee splits, type-check phase** (`tr1-check-r2.txt`; inclusive, top 15 children, plus own exclusive):
`Dictionary<go.string,__Canon>.Resize` 19.9% of the phase; `slice.Append(ref, ReadOnlySpan, bool)` 11.9%;
`Binder.declareSymbolEx` 44.8% (43% of it in `Dictionary.TryInsert`, 38% in `addDeclarationToSymbol`);
`FieldRefBox<Node>.Equals` 3.7%; `builtin.TryTypeAssert` 2.7%. The callers of `Monitor.Enter_Slowpath` (1.4%): 83% are
`ManagedPointerTokens.IdentityOf`.

**12, gen0 budget** (`gen0.txt`; untraced, 3 each, alternating, base first):
- server GC as published: 87.8 / 82.7 / 83.9 s, median 83.9;
- `DOTNET_GCgen0size=0x10000000`: 81.6 / 81.5 / 82.8 s, median 81.6.

That is -2.7% on the median wall, with the ranges overlapping (base 82.7-87.8, arm 81.5-82.8). CPU time fell from
200 / 192 / 193 s to 178 / 181 / 183 s, about -7%. P3's prediction of a 5-12% wall fall MISSED as worded; by P3's own
falsifier the change sits inside the base's spread. The box was faster than in round 1's control (base median then
99.4 s), so compare only within this pair.
