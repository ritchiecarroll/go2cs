# DATA — hop N (.NET 9 → 10) performance measurements

> **State: EXECUTED / partially open.** Point-in-time measurement DATA for one .NET migration,
> labeled by leg, host and date. It exists because [`../DotNetMigration.md`](../DotNetMigration.md)
> carries **no frozen figures** — the runbook keeps the protocol and the traps, and an instance's
> numbers live here, where a later migration can compare against them without mistaking them for
> rules. Sections are **appended and superseded, never overwritten**, on the model of
> [`DATA-sweep-row-walltimes.md`](DATA-sweep-row-walltimes.md).
>
> The **N-stage ids** used throughout (N0…N8) are the hop's own ladder, defined in
> [`../PLAN-hop-campaign.md`](../PLAN-hop-campaign.md) §3.2: N0 provisioning, N1 the SDK alone,
> N2 the .NET 9 baseline capture, N3 the TFM, N4 the .NET 10 CPU measurement, N5 the AOT/ILC
> verification, N6 golib trim-safety, N7 the test-host publish shape, N8 the deployment-shape ruling.
>
> Provenance: §1 was `DotNetMigration.md` §6.1's scouting block, §2 was its §6.2, both moved here
> 2026-08-24 when the runbook's authority was consolidated. The protocol they were folded into —
> the named control row, and gating allocation claims by COUNT rather than by time — stays in the
> runbook as §6.1, because it is a rule rather than a reading.

---

## 1. The pre-hop scouting legs (2026-08-23, SDK 10.0.400, perf-canon laptop)

Folded in as protocol facts at the time, and retained here as the measurements behind them:

1. **The ILC binds to the TFM, not the SDK** — measured, not just cataloged
   ([`../DotNetMigration.md`](../DotNetMigration.md) §3 trap 1): SDK 10.0.400 publishing `net9.0`
   resolves `ILCompiler 9.0.19`, and its "10-AOT" Fib is **identical** to the 9-AOT Fib
   (**177.1 vs 178.2 ms**). Corollary: *there is no AOT measurement worth taking between N1 and N3* —
   the AOT column cannot move until the TFM does, so scheduling one spends hours measuring the null
   hypothesis of a variable that has not moved.
2. **A 51-second "publish" is the trap-2 tell on this corpus.** A real per-benchmark publish was
   **964–1,138 s** on the perf-canon laptop and **~25 min** on the i7-5820K. Any AOT leg whose
   publish came in orders of magnitude under that re-measured a stale binary; purge and disbelieve.
3. **Roslyn 10's CS7022** on the runner's top-level-statements shape is benign and expected at N1.
4. **`net9.0` under the 10 SDK still executes on the 9 runtime** unless explicitly selected — the
   `FrameworkDescription` probe (trap 4) is what makes any leg's identity a fact rather than an
   assumption, and it runs on *both* legs, every time.

**The JIT leg's own reading** (board 2026-08-22, lane G, `claude/dotnet10-perf-scout`; both legs
executing identical IL, same day, same silicon, quiet machine, median-of-5, Go columns reproducing
across legs as the same-day control): *"a solid single-digit-to-20 % improvement across most of the
corpus with a >2× win on string-heavy code, financed by two narrow regressions to re-measure at hop
time."* The instance detail — the per-benchmark wins and the three named regressions — is carried in
[`../PLAN-hop-campaign.md`](../PLAN-hop-campaign.md) §3.1, which is that hop's own record.

---

## 2. N5 — the AOT leg's falsifiable prediction, and its resolution

[`../DotNetMigration.md`](../DotNetMigration.md) §7 demands the AOT stage state its expectation
before running. This is the prediction as stated, so the outcome below is information rather than
narrative.

**Background:** the bflat exploration's one CPU anomaly was Fib under bflat's .NET-10-preview
codegen — unattributable to bflat itself (same ILC/RyuJIT family), and left standing as *"an argument
for measuring the hop itself"*
([`../PLAN-bflat-perf-exploration.md`](../PLAN-bflat-perf-exploration.md)). The scouting then showed
the 10-SDK-on-`net9.0` leg is byte-for-byte the 9-ILC (§1 lesson 1), so the anomaly's candidate cause
narrowed to exactly one untested thing: **the real .NET 10 ILC/framework pair behind a `net10.0`
TFM.**

**Prediction N5, falsifiable in both directions (stated before the run):** *running the suite's AOT
column at N5 (`net10.0`, ILC 10.x), the Fib row moves materially in the direction the bflat preview
showed — closing the anomaly's attribution as "the 10 codegen" — or it lands within the named control
row's envelope of the N2 9-AOT baseline, and the anomaly is attributed to the preview/bflat packaging
and CLOSED as not-a-hop-question.* The comparison base is N2's 9-AOT numbers, minted on the same
host and banked in the performance README's History section; the control row is the Go column.

### RESOLVED 2026-08-24, branch two

**174.7 vs 175.3 ms (−0.3 %), control 0.0 % — attribution: preview/bflat packaging.** Corroborated
independently by the route-#6 bflat-arm quarantine (the un-guarded arm could report `ok` for
benchmarks it never compiled). **The anomaly is closed as not-a-hop-question.**

Two riders the resolution run established, both fleet-relevant:

| Reading | Value | Consequence |
|:--|:--|:--|
| First completed 10-ILC publish | **11,862 s wall / ≥12,754 CPU-s** on the perf-canon host, against the 9-era **894–953 s** | **~10.6× CPU work**, near-serial (a multi-hour ~1.0-core tail), uniform across benchmarks at the comparable wall point — so ladder arithmetic scales linearly |
| Peak working set during the publish | **14.9 GB** in the final phase | **A 16 GB machine cannot run a 10-ILC publish of this closure without swapping.** A provisioning floor, not a tuning knob |
| ⚠ SUPERSEDED (2026-08-25): publish WS peak re-measured HIGHER | **17.662 GB** — the i9 farm probe's Sieve publish, `ilc.exe` sampled every 10 s, peak in the final seconds of compile | The floor moves with the corpus: budget **~18 GB per concurrent publish** and re-measure each hop. On the 63.7 GB i9 that means **three concurrent lanes, not four** (3 × 17.7 = 53 GB; 4 × 17.7 = 70.8 GB does not fit) |
| 10-AOT working set on the resolved row | 75.8 → **15.5 MB**, below the unchanged JIT floor | The 9-era *"AOT trades memory for startup"* reading does **not** survive on that row. Whether the collapse generalizes is the full ladder's question, not this section's claim |
| ⚠ SUPERSEDES the 17.662 GB row (2026-10-13): publish peak re-measured HIGHER again | **23.8 GB private bytes (22.2 GB working set)** — the i9's one Native AOT publish of `PerfIfaceShell` (the IfaceShell P3 reading), peak in the final seconds of compile. The same publish died on a 15 GB linux VM with no swap after 7 h 54 min: ILC's resident set sat flat at 8.4–8.6 GB for six hours, read 9.74 GB with 5.1 GB available 18 s before the exit, then "Insufficient memory to continue the execution of the program" | **Plan on 32 GB or more for one publish, with nothing else running.** The peak has grown at each re-measure (14.9 → 17.7 → 22.2 GB working set); the flat phase is not the peak, so a reading taken before the final seconds understates it. *Derived:* on the 63.7 GB i9 that is two concurrent publishes, not three (3 × 22.2 = 66.6 GB) |

---

## Sources

- [`../DotNetMigration.md`](../DotNetMigration.md) — the protocol these numbers were measured under
  (§6 the paired same-session A/B and the named control row, §7 the verify-then-bank order, §3 the
  trap catalog)
- [`../PLAN-hop-campaign.md`](../PLAN-hop-campaign.md) — the hop instance: §3.1 the measured bill,
  §3.2 the N-stage ladder, §3.3 the bflat breadcrumb as the plan stated it
- [`../PLAN-bflat-perf-exploration.md`](../PLAN-bflat-perf-exploration.md) — the concluded floor
  exploration the anomaly came from
- [`DATA-sweep-row-walltimes.md`](DATA-sweep-row-walltimes.md) — the append-and-supersede contract
  this record follows
