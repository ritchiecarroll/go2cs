# DATA — the linux/arm64 bank (S7): the plan, written before any dispatch (G, 2026-11-10)

> **A point-in-time record, and a plan awaiting a ruling.** This is seat S7 of
> `DESIGN-linux-arm64-flavour.md` (§7, §9) as a plan, ordered by COORD on 2026-11-10 in five parts. It was read at
> `0e1e143a51` (the batch 284 union; master was `b9a6d261fc`). **Nothing was dispatched, built for the target or
> run.** Every count below is measured at that tree unless it is marked ESTIMATE or PREDICTION. The predictions of
> §4 are written here first so that the runs can score them.
>
> **Entry gates. No dispatch before all of them hold:** (E1) i9's site 9, the M2 wall, is on master; (E2) i9's
> arm64 stub triage is on a record, one line per name of §4.1; (E3) COORD has ruled this plan; (E4) the grammar
> seat of §2.4 has landed; (E5) the workflow seat of §2.3 and the proof step of §3 have landed.

## 0. Method, and how to re-derive it

- **The roster is read through its reader**, `Get-ValidatedRosterRows` in `src/_roster.ps1`, never by a text
  search: 238 rows, 236 with an applicable linux reading, 2 that are `linux: n/a` (internal/syscall/windows and
  internal/syscall/windows/registry).
- **Wall times** are the `wall_s` column of the seven evidence files in `rebank-1.26.10.3/linux/` (the ledger
  files excluded). Their 236 distinct rows are exactly the roster's 236 linux rows. They were measured on three
  lane boxes of different speed, not on a hosted runner, so they are an estimator and no more.
- **The shard lists** are the fifteen lists C1 ran on the hosted mac legs (`rebank-1.26.10.3/darwin/`, column
  `shard`). They cover the same 236 rows.
- **The arch file delta** comes from Go's own go1.26.10 source through `go list`, at linux/amd64 and at
  linux/arm64, under the corpus tags `purego,math_big_pure_go`. The instrument is
  [`data/la64-s7/filedelta.py`](data/la64-s7/filedelta.py); it prints `rows=236`, 4 rows with a test-file delta
  (14 top-level test names) and 10 rows with a source-file delta. Without the tags it reads 5 and 13: math/big,
  crypto/sha1's sources and the edwards25519 field leave the delta under the tags, which is why the corpus has no
  arch folder for them.
- **The stub ceiling** is a census of bodiless `partial` declarations in the 18 `linux-arm64` folders of
  `src/core`. It is a floor on declarations and a CEILING on throwing stubs, because a companion file may body
  some of them.

## 1. What banks

**Recommendation: all 236 linux rows get a linux-arm64 reading in .4, dispatched in the priority order of §2.2.**
A row that is dispatched and does not bank is named UNMEASURED with its reason (§5.3). No row is left undispatched.

Reasons:

1. **Every row loads the new flavour.** Each row's test host is built against the linux-arm64 selection of the
   packages it imports. Which rows REACH an arm64-only body at run time is not known without running them, so no
   row is arch-neutral by construction.
2. **Two flavours, two test sets.** The mac RIDs share one converted flavour and still differ on 4 of 234 rows
   (crypto/subtle, go/internal/gcimporter, net, runtime). linux has two flavours, so the converted tests differ
   too: §4.2 names 14 verdicts that a linux-arm64 reading lacks, in 4 rows.
3. **The cost is small.** §2.2 estimates 21.6 hours of arm-leg machine time, under one day elapsed.
4. **A subset costs more than it saves.** With a subset the header reports two populations until .5, and every
   reader of the roster must know which rows were never tried. "Tried, with a named reason" is a stronger record
   than "ruled out of scope".

The alternative, should COORD prefer a subset: wave 1 of §2.2 only (the 7 lists that hold the 13 arch-own rows,
116 rows), with the other 120 rows named `UNMEASURED: not dispatched in .4`. It saves about 10 machine-hours.

## 2. How

### 2.1 The leg, and one dispatch form

Each shard is ONE `os-matrix` dispatch: `goos: linux`, `stage: sweep-shard`, `arch: both`, the list as the exact
row filter, at one tip. The arm leg is S5b's `ubuntu-24.04-arm`; the x64 leg of the same dispatch is not overhead:

- **It is the partner reading.** The arch-count guard needs the verdict NAMES the shorter RID lacks. They are the
  set difference of the two legs' verdict names at one tip, and nothing else supplies them.
- **It is the one-axis control.** The roster's linux segments were banked on lane boxes. A hosted linux-x64
  reading that differs from the roster is a HOST difference. It is recorded by name and is not charged to arm64.
  S7 does not rewrite a linux-x64 reading (Q4).

"A run whose headSha is not the tip is not evidence" carries over from the mac bank.

### 2.2 The shard plan

A-0 is the proof and calibration dispatch: the workflow's default filter, `container/heap`. It needs the lift of
§2.3 like every other shard, because the arm leg refuses `sweep-shard` whole now that the flavour's folders
exist. A-1 to A-15 are C1's fifteen lists, unchanged. `box` is the summed row wall on
the lane boxes; `mac` is the hosted osx-arm64 row wall of the same list; the ESTIMATE is `1.5 x box + 30` minutes
(1.5 is the workflow's own arm factor; 30 is setup, the converter build and the first row's corpus build).

| Shard | Rows | Wave | Arch-own and named rows | box min | mac min | ESTIMATE min |
|---|---|---|---|---|---|---|
| A-0 | 1 | 0 | container/heap: proof (§3), calibration, first start | - | - | 35 |
| A-13 | 1 | 1 | runtime | 62 | 42 | 123 |
| A-9 | 19 | 1 | math; math/big | 41 | 14 | 92 |
| A-12 | 19 | 1 | syscall; time | 35 | 15 | 83 |
| A-7 | 19 | 1 | internal/cpu, internal/runtime/atomic; internal/poll | 55 | 13 | 113 |
| A-8 | 19 | 1 | internal/runtime/gc/scan, internal/runtime/sys, internal/syscall/unix | 43 | 20 | 95 |
| A-6 | 19 | 1 | hash/crc32, internal/abi, internal/buildcfg, internal/chacha8rand | 36 | 21 | 84 |
| A-2 | 20 | 1 | crypto/internal/fips140/subtle | 32 | 16 | 78 |
| A-15 | 1 | 2 | net | 6 | 17 | 39 |
| A-11 | 19 | 2 | os, os/exec, os/signal, reflect, runtime/pprof | 35 | 23 | 83 |
| A-14 | 1 | 2 | sync/atomic | 11 | 11 | 47 |
| A-3 | 20 | 2 | crypto/sha1, crypto/subtle, crypto/tls, debug/buildinfo | 52 | 78 | 108 |
| A-5 | 20 | 2 | go/internal/gcimporter, go/parser | 46 | 27 | 99 |
| A-10 | 19 | 2 | net/http and its packages | 48 | 26 | 102 |
| A-1 | 20 | 3 | none named | 28 | 14 | 72 |
| A-4 | 20 | 3 | none named | 33 | 16 | 80 |

- A-1 to A-15 sum to 236 rows, 563 box minutes and an ESTIMATE of 1,298 arm minutes (21.6 hours). The largest
  ESTIMATE, 123 minutes, is 39% of the arm leg's 315-minute job limit.
- **A-0 re-cuts the table.** It measures the fixed cost and, against its own x64 leg, the arm ratio. If the ratio
  is above 1.5, every ESTIMATE is recomputed and posted before wave 1; a shard whose new ESTIMATE passes 240
  minutes is split in two.
- **Waves.** Wave 0 alone, then stop and read. Wave 1 is the flavour's own code (7 lists, 116 rows, all 13
  arch-own rows). Wave 2 is the OS surface over the hand-owns. Wave 3 is the tail. A wave is dispatched only after
  the previous wave's rows are READ, not merely finished.
- **Concurrency.** The workflow's group is keyed by ref, goos and stage, and it holds one running and one pending
  run. The mac bank's answer is reused: several tip refs at one SHA (`claude/g-tip-<sha>-1` to `-5`), with no
  change to the group (Q2).

### 2.3 The workflow seat (S7-b)

- Lift `sweep-shard` in `$armLiftedBy`. This is the lift the map names "S3, then the bank of section 7".
- Add the proof step of §3 before any step that runs a converted program on linux, and its post-run twin.
- Emit evidence in the mac bank's form, one line per row per RID: `shard row RID verdicts matched disclosed
  divergent new status agree ... run wall_s cells absent`. **Not yet read: whether C1's collector is already
  RID-generic.** If it is mac-only, generalising it is part of this seat.

### 2.4 The grammar seat (S7-a), and the derived segment

§7 of the design is unbuilt at this tree: `annotate_linux.py` is a single-RID tool and has no `pending`. The seat:
the note group of `$RosterOsPattern` admits `linux-x64|linux-arm64` and `pending`; the darwin-only refusal becomes
a per-key RID set; the Linux header derivation reports one population per RID; `annotate_linux.py` gains the
two-RID derivation; and ONE mechanical commit writes `(linux-x64; linux-arm64: pending)` on all 236 rows. From
that commit a linux segment with no per-RID note fails the format check.

Each row's segment is then DERIVED from its two evidence lines. It is never typed.

| Case | Condition on the two lines | Segment |
|---|---|---|
| Equal | same verdicts, same matched and disclosed-divergent | `N + d (linux-x64; linux-arm64: N + d)` |
| Arch-strict | equal verdicts, counts differ, `new` 0 on both | the RID with more disclosed-divergent, the other as the note |
| Arch-count | unequal verdicts, equal disclosed-divergent, `new` 0, matched + divergent = verdicts on each | the LONGER RID, the shorter as the note |
| Not banked | anything else, or a non-PASS word | `(linux-x64; linux-arm64: pending)`, with a ledger line (§5.3) |

The first form writes both RIDs out because, once the bank is open, a bare segment is refused. Arch-strict is the
rule of `Get-DarwinArchStrictReading`, which this seat makes RID-generic. Arch-count is C1's `Get-ArchCountReading`
as it stands; its self-test already asserts `636 (linux-x64; linux-arm64: 634)`.

**Named missing verdicts are mandatory.** The shorter RID's `absent` column names exactly (longer - shorter)
distinct verdicts and the longer names none. Otherwise the guard returns a Refusal by name, the row stays pending,
and nothing is edited by hand to make it pass.

## 3. Safety: dump suppression, proven per job

The fleet rule (COORD, 2026-11-09, after six aborts wrote 211 GB on a lane box): no many-thread Linux run without
dump suppression PROVEN on that box first, and a failed proof STOPS the run. No suppression exists in the
workflows, the harness or golib at this tree. A hosted job is a fresh machine and an image update can change its
dump handler without notice, so **the proof is a step of every job**, not a property of a runner label.

**The proof step**, before the first converted program runs:

1. **Record.** `ulimit -c` soft and hard, `kernel.core_pattern`, `core_uses_pid`, the state of apport and of
   systemd-coredump, and free bytes per filesystem. Take a file census of the dump locations: the systemd-coredump
   and apport folders, `/var/crash`, the workspace, the temp folder and a scratch folder.
2. **Positive control: the detector must see a dump.** With a file `core_pattern` into the scratch folder and
   `ulimit -c unlimited`, kill a one-thread process with SIGSEGV. The file census must report one new file. Then
   write a 128 MiB file; the free-space check must report it. A detector that sees neither proves nothing.
3. **Apply and read back.** `kernel.core_pattern=|/bin/false`, `ulimit -c 0` and `DOTNET_DbgEnableMiniDump=0`
   exported to the later steps. Each value is read back and compared.
4. **Negative proof.** Under suppression, a .NET probe starts 2,000 threads and is crashed twice: by SIGSEGV from
   outside, and by `Environment.FailFast`. After each: 0 new files in every location, free space within 64 MiB of
   the recorded value on every filesystem, and no new systemd-coredump entry.

**The failure rule.** A blind detector in (2), a readback mismatch in (3), or any new file or a delta over the
threshold in (4) fails the job BEFORE the sweep step. Further:

- A-0 runs the proof on both legs. **No other shard is dispatched until A-0's proof is green and read.**
- A post-run step under `always()` repeats the census. A new dump file there HOLDS that shard's evidence: its
  rows do not bank and no further shard is dispatched until the cause is named.
- No S7 row runs on a lane box, WSL or otherwise, without the same four steps on that box, with the Windows-side
  dump folder in the census.

**PREDICTIONS for the proof** (the kernel statements are expectations for the step to measure, not findings):
PD1 the soft limit already reads 0 on the runner; PD2 the stock `core_pattern` is a pipe to apport, and a piped
handler is not stopped by a limit of 0, which is why (3) replaces the pattern; PD3 the control produces
exactly one file; PD4 both crashes leave 0 files and under 64 MiB. A-0 also records the arm runner's free space,
which the sweep's disk floor does not check on a hosted leg.

## 4. Predictions, before any dispatch

Three populations: **13 arch-own rows** (a `linux-arm64` folder in `src/core`), **1 test-delta row** with no
folder (crypto/sha1), and **222 plain rows**.

### 4.1 The stub ceiling (the input E2 triages)

A bodiless partial compiles and throws at its first call. There are 58 such declarations in the arm64 folders:

| Package | Row | Declarations | Names |
|---|---|---|---|
| internal/runtime/atomic | yes | 26 | the Xadd, Xchg, And, Or, Cas and Store families |
| runtime | yes | 10 | panicBounds, panicExtend, load_g, save_g, asmcgocall_no_g, emptyfunc, spillArgs, unspillArgs, getfp, walltime |
| syscall | yes | 6 | runtime_entersyscall, runtime_exitsyscall, rawSyscallNoError, rawVforkSyscall, runtime_doAllThreadsSyscall, cgocaller |
| internal/cpu | yes | 4 | getisar0, getpfr0, getMIDR, libcGetauxval |
| internal/runtime/sys | yes | 3 | EnableDIT, DITEnabled, DisableDIT |
| hash/crc32 | yes | 2 | castagnoliUpdate, ieeeUpdate |
| math | yes | 1 | archExp2 |
| internal/chacha8rand | yes | 1 | block |
| vendor x/sys/cpu, internal/bytealg | no | 5 | getisar0, getisar1, getpfr0, getzfr0; MakeNoZero |

**The two failure classes read differently, and the plan depends on telling them apart.** A stub that still
throws ends a test or the host: the row's word is not PASS and the stub's name is in the log. A hand-own that is
bodied but wrong passes the build and the startup and reads as a DIVERGENCE (Go passes, C# fails). So:

> **The rule for an arm64-only divergence in an arch-own row:** it is read against the hand-own list first. No
> disclosure is minted for it until the hand-own it reaches is named or ruled out.

### 4.2 The four arch-count rows (PREDICTION P1)

The linux-arm64 reading lacks exactly these top-level verdicts, and each row banks through the arch-count guard:

| Row | linux-x64 | linux-arm64 | Absent at arm64 |
|---|---|---|---|
| crypto/sha1 | 14 + 1 | 13 + 1 | TestOutOfBoundsRead |
| internal/cpu | 9 | 2 | TestX86ifAVX2hasAVX, TestX86ifAVX512FhasAVX2, TestX86ifAVX512BWhasAVX512F, TestX86ifAVX512VLhasAVX512F, TestDisableSSE3, TestSSE3DebugOption, TestDataCacheSizes |
| internal/runtime/gc/scan | 3095 | 3092 | TestExpandAVX512, TestFilterNilAVX512, TestScanSpanPackedAVX512 |
| runtime | 11094 + 67 | see P4 | TestHasAVX, TestMemmoveOverflow, TestStartLineAsm (a floor: subtests are not counted) |

The mac flavour, which is converted at arm64, already reads 13 + 1, 2 and 3092 on the first three. That is
corroboration from another OS, not a measurement of this one.

### 4.3 Per shard

- **P0, A-0.** Green on both legs: `container/heap` reads its x64 reading. It is the first converted test host
  ever started on the flavour, so a throwing stub on the start path fails here and not in wave 1. Which stubs the
  start path reaches is not known; a green A-0 clears only those.
- **P2, the 222 plain rows (all shards).** Each reads its same-dispatch linux-x64 reading, verdict for verdict.
  Exceptions: 0 to 6 (the mac base rate is 4 of 234). More than 6 falsifies "plain" as a class, and banking stops
  for a re-read. Named candidates: crypto/subtle, go/internal/gcimporter, net, os, internal/poll.
- **P3, A-9, math.** At this tree `math.Exp2` throws at arm64: `haveArchExp2` is true there and `archExp2` has no
  body, while `math_impl.cs` bodies the other arch functions. Before triage the row does not bank, and TestExp2 is
  the named casualty. After triage: `76 + 0`, equal to x64. A bodied-but-wrong `archExp2` reads as exactly one
  divergence, TestExp2. The M2 wall (archLog and archHypot bodied twice) is a compile error and must be gone at
  E1. Owed at E2: a census of `math.Exp2` callers outside the math row.
- **P4, A-13, runtime.** After E1 and E2 the verdict set is x64's less the three names of P1. The disclosed set
  will NOT be equal: the two mac RIDs differ by one (11062 + 58 against 11063 + 57), and predicted here is a
  difference of 1 to 3. **Neither guard covers a row whose verdicts AND disclosed-divergent both differ, so
  runtime is predicted to return a Refusal** (Q1). A divergence here is read against the 10 names of §4.1 and
  sites 1, 3, 4 and 9 of S3 before anything else.
- **P5, A-12, syscall.** The same verdict set as x64 (no test-file delta). The row carries 17 disclosures on
  linux-x64; membership may move by 0 to 2 at arm64, which is the arch-strict case. A divergence is read against
  the 6 names of §4.1, the zsyscall twin (site 7) and `Select` over pselect6 (site 6).
- **P6, A-7.** internal/cpu reads 2 (P1). internal/runtime/atomic reads 16 and, in A-14, sync/atomic reads 108,
  both equal to x64: these two rows are where a wrong atomic body reads as a divergence. internal/poll equals x64 once sites 1 and 2 land; it is the slowest row of the list.
- **P7, A-6.** hash/crc32 reads 17. With HWCAP read (S4), the arch path is taken on the runner, so `ieeeUpdate`
  and `castagnoliUpdate` must be bodied or their have-flags forced false; a wrong body shows in the tests that
  check the arch result against the table result. internal/chacha8rand reads 4; its golden-output test catches a
  wrong `block`. internal/abi and internal/buildcfg equal x64.
- **P8, A-8.** gc/scan reads 3092 (P1). internal/runtime/sys reads 4; the three DIT names are reached only where
  the CPU reports DIT. internal/syscall/unix reads 9: the fstatat and generic syscall-number files are the arm64
  selection, and a wrong number there fails a test outright.
- **P9, A-3.** crypto/sha1 reads 13 + 1 (P1). crypto/subtle: if the runner's HWCAP reports DIT, the row reads
  the mac arm64 form, `8 + 3` against x64's `11 + 0`, through arch-strict, with the same three DIT disclosures;
  if a DIT stub throws uncaught, the row does not bank and names it. debug/buildinfo and crypto/tls equal x64.
- **P10, A-5.** go/internal/gcimporter equals x64 (636). The design says it should match by construction; the mac
  arm64 leg differed by 2, so this row is the test of that claim.
- **P11, A-15, A-11, A-10.** net, os, os/exec, os/signal, runtime/pprof and net/http equal x64. They ride sites 1
  to 5 and 7; a difference is charged to a site by name before any disclosure.
- **P12, the control.** The hosted linux-x64 leg equals the roster's banked linux reading on all but a few rows
  (predicted 0 to 4, around cgo-gated oracle tests). Each difference is listed as a host difference.

## 5. The .4 release gate

### 5.1 What .4 needs from S7 (recommended)

1. **The grammar seat and the pending commit** (§2.4), so that no linux row reads as banked on a RID it never
   ran on. This holds even if nothing else of S7 lands.
2. **A-0 green on the arm runner**, proof included.
3. **All 236 rows dispatched once and READ.** Every row is banked, or pending with a ledger line.
4. **The floor set BANKED:** runtime, syscall, os, net, internal/poll, internal/runtime/atomic, sync/atomic,
   math, internal/cpu. These are the rows a consumer's first program stands on. If one of them is not banked at
   the freeze, the recommendation is that linux-arm64 does NOT ship as supported in .4: SEAT 0's refusal stays,
   and `KnownIssues.md` keeps its "compiles there and does not run yet" wording (Q3).
5. **No unread death.** Every row that died at a throwing stub is listed with the stub's name.

### 5.2 What may be named UNMEASURED and follow in .5

- A non-floor row that did not bank for a named reason: a Refusal from a guard, an oracle-unstable row on the
  runner, or a host difference still being read.
- A row that needs a NEW disclosure ruling for an arm64-only divergence. A disclosure minted under freeze
  pressure is the risk this list exists to avoid.
- runtime's segment form, if Q1 is not ruled in time. Then runtime is banked by count in the evidence and stays
  pending on the roster, which fails item 4 above unless COORD rules otherwise.

### 5.3 The ledger, consistent with P2's S6 rule

P2's rule for the package pages (`unmeasured: <reason>`: a blank reason refused, and a file that also records run
fields refused) is applied to the roster's pending rows. One file, `rebank-<release>/linux-arm64/UNMEASURED.tsv`,
holds `row`, `reason`, `shard` and `run`. Once the .4 bank closes, the format check refuses a pending row with no
ledger line, a ledger line with a blank reason, and a ledger line for a banked row. A package page reads
`UNMEASURED: <reason>` at linux-arm64 exactly while one of its rows is in the ledger.

## 6. Questions for the ruling

- **Q1.** A row whose verdicts and disclosed-divergent BOTH differ between the RIDs fits neither guard. Predicted
  for runtime. Recommended: a combined reading that banks only when the arch-count naming holds and both lines
  carry `new` 0, cut by C1 as a third guarded form with its own self-test.
- **Q2.** Parallel shards through several tip refs (recommended: proven, no workflow change), or a group that
  gains the shard.
- **Q3.** The floor set of §5.1 item 4: its membership, and whether a miss withholds the RID from .4.
- **Q4.** A hosted linux-x64 reading that differs from the roster: recommended recorded as a host difference,
  with no roster edit by S7.
- **Q5.** All 236 rows (recommended), or wave 1 with the other 119 rows named UNMEASURED.

## 7. Seats and size (ESTIMATE, lane-days)

| Seat | Work | Days |
|---|---|---|
| S7-a | The grammar, the two-RID `annotate_linux.py`, the pending commit, the ledger check | 1-1.5 |
| S7-b | The workflow lift, the proof step and its twin, RID-generic evidence | 0.5-1 |
| S7-c | A-0, then the three waves, each read against §4 | 1 |
| S7-d | The bank: derived segments, the ledger, the scored predictions as a dated block here | 0.5-1 |

The total is 3 to 4.5 days, inside the design's 3 to 5. S7-a and S7-b touch no arm64 code and can be cut before
E1; S7-c cannot.

## 8. The rulings and the entry checklist (dated block, 2026-11-10)

COORD accepted this plan at `7608783cda` on 2026-11-10. Sections 0 to 7 stand as written; this block adds the
rulings, the seats' owners and the state of the entry gates. **Still nothing dispatched, built for the target or
run.**

### 8.1 Rulings

| Asked | Ruled |
|---|---|
| Q5 | **All 236 rows**, in the three waves of §2.2, wave 1 first. A row that does not bank is UNMEASURED with a reason, never undispatched. |
| Q3 | **The floor set is the nine rows of §5.1** (runtime, syscall, os, net, internal/poll, internal/runtime/atomic, sync/atomic, math, internal/cpu). The RID ships as supported in .4 only if the floor banks. If it cannot bank before the release date, COORD takes the choice to the owner (slip .4, or ship with linux-arm64 withheld); the default is the slip, never a quiet ship. |
| Q1 | **A combined form is approved as a named form**, for a row whose verdicts AND disclosed-divergent both differ. It is built in S7-a with mandatory naming, as arch-count has. A row that fits no form stays PENDING and is never banked. |
| Q2 | **Tip refs** for the parallel shards. |
| Q4 | A hosted linux-x64 reading that differs from the roster is **recorded as a finding** and never written into the roster in S7. A re-bank is its own ruled seat. |
| §4.1's rule | Ruled as written: an arm64-only divergence in an arch-own row is read against the hand-own list before any disclosure. |

A correction to §6: Q5 names "the other 119 rows". The count is 120 (236 less wave 1's 116), as §1 has it.

### 8.2 Seats

| Seat | Owner | State |
|---|---|---|
| S7-a (the §7 grammar, the two-RID `annotate_linux.py`, the combined form of Q1) | P2, after S6 | cuttable now |
| S7-b (the `sweep-shard` lift on the arm leg, the per-job proof step with its positive control) | C2 | cuttable now |
| S7-c and S7-d (the dispatches, the bank, this record) | G | gated on §8.3 |

### 8.3 The entry checklist

Read on origin on 2026-11-10 after a fetch: master `b9a6d261fc`, the batch 284 union `c4b47c4f61`. A line is
CLEARED only by a fact read at master.

| Gate | State | Read |
|---|---|---|
| E1, site 9 on master | OPEN | Site 9 is on `claude/i9-s3-s2-sites` at `a719a19546` (`9b706f4d82`, `ebe1b5ea34`, `a1d01fdfec`, `11171d1265`). That tip is an ancestor of neither master nor the union. |
| E2, the triage of §4.1's 58 names on a record | OPEN | The S3 census at `a719a19546` carries no triage of them, and site 9 adds no other record (its `docs/` changes are that census and one re-cite). |
| E3, the ruling | **CLEARED** | §8.1. |
| E4, S7-a landed | OPEN | Assigned to P2, after S6. |
| E5, S7-b and the proof step landed | OPEN | Assigned to C2. |
| A-0's proof green and read | OPEN | Dispatched only after E1 to E5. No shard before it. |

### 8.4 What site 9 changes in §4 (read at `a719a19546`; nothing built or run)

The predictions of §4 were written at `0e1e143a51`. Site 9 moves two of them, and the move is written here before
any dispatch, as §4 requires.

- **§4.1, math: 1 to 0.** `ebe1b5ea34` adds `src/core/math/linux-arm64/exp2_asm_impl.cs`, which bodies
  `archExp2` as a call to Go's own pure-Go `exp2`. The stub ceiling at the E1 tree is PREDICTED at 57 and is
  re-measured there before A-0.
- **P3, math.** The "before triage" clause is spent once E1 clears: `math.Exp2` no longer throws at arm64. The
  prediction is `76 + 0`, equal to the x64 leg. linux-amd64 reaches the same `exp2` directly (`haveArchExp2` is
  false there), so the two legs run one body. `archExp2` is now the first arm64 hand-own of the
  bodied-but-wrong class: a wrong body reads as exactly one divergence, TestExp2. The census of `math.Exp2`
  callers outside the math row, owed at E2, is owed again only if TestExp2 diverges.
- **The M2 wall.** `9b706f4d82` and `a1d01fdfec` move the bodies of `testSPWrite`, `archLog` and `archHypot` to
  `amd64/`, beside their only bodyless principals. That is the compile error P3 said must be gone at E1.
- **GolibTests.** `11171d1265` states that GolibTests compiles at linux/arm64. S7-b's arm leg depends on it; it
  is read again at the E1 tree, not taken from the commit's subject.
