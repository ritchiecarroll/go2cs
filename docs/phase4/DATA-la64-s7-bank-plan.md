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

## 9. The entry checklist, read at master (dated block, 2026-11-11)

Batch 284 landed. Read on origin on 2026-11-11 after a fetch: master `50dd9a065c`. Sections 0 to 8 stand as
written; this block moves gate states and scores §8.4's predictions. **One hosted run was dispatched for it
(GolibTests on the arm leg, §9.3). No sweep shard has been dispatched.**

### 9.1 The gates

| Gate | State | Read at `50dd9a065c` |
|---|---|---|
| E1, site 9 on master | **CLEARED** | `a719a19546`, `9b706f4d82`, `ebe1b5ea34`, `a1d01fdfec` and `11171d1265` are each an ancestor of master (`git merge-base --is-ancestor`, five of five). `src/core/math/linux-arm64/exp2_asm_impl.cs` is in the tree. |
| E2, the triage of §4.1's names on a record | **CLEARED** (ruled 2026-11-10) | The triage is `DATA-la64-s3-census.md`, "Amendment, 2026-11-10", and it is on master. §9.2 reads it against the tree. |
| E3, the ruling | **CLEARED** | §8.1. |
| E4, S7-a landed | **CLEARED** | `f566323426` is an ancestor of master. Its fixups (`claude/p2-la64-s7a-fixups` at `fbb9169c42`) are second-read with no defect and are NOT on master. They land before the first combined bank; A-0 banks nothing. |
| E5, S7-b and the proof step landed | **CLEARED** | `356d2b7eb3` is an ancestor of master. The workflow at master sequences `.github/dump-suppression-proof.sh` on the linux `sweep-shard` legs in three steps (record, control and apply; prove; recheck under `always()`, which runs only when the prove step succeeded), and carries the `dumpproof=red` arm. |
| A-0's proof green and read | OPEN | Dispatched next, at master's tip, on both legs. No other shard before it. |
| Wave 1's arm64 banking | **HELD** (ruled 2026-11-11) | Until batch 285 lands with the netpoll guard lift (§9.3). Not one of §8.3's entry gates; a new hold. |

This record itself is not on master: neither `7608783cda` nor `a0c988af41` is an ancestor of `50dd9a065c`, and
master's workflow cites this file by path. The link resolves once the plan's branch lands.

### 9.2 The stub ceiling, re-measured (§8.4's first prediction)

§8.4 predicted 57 at the E1 tree, because `archExp2` is bodied. **The prediction is scored a MISS: its reason
was wrong, and 57 is the record's number for another reason.**

- **The plan's instrument does not move.** A grep for bodiless `partial` declarations in the 18 `linux-arm64`
  folders reads 58 at master, the same 58 names as at the plan's tree. A companion body does not remove the
  declaration, so this count cannot fall when a stub is bodied.
- **One of §4.1's 58 was never a stub.** `internal/cpu.libcGetauxval` is a `[LibraryImport]` declaration in a
  hand-owned companion; the platform's own generator bodies it. §4.1 counts it in internal/cpu's 4. The row
  reads 3, and the table's total 57.
- **The census's own predicates exclude it.** The S3 record's complete table has 57 declarations in 10 packages:
  42 bodied, 15 stubbed.
- **A second instrument agrees, at master.** For each of the 57 names, a body is searched in the `linux-arm64`
  folder and then in the package's other files: 8 are bodied in the arch folder, 34 in a flat or `linux/` file,
  and 15 have no body. The 15 are the record's 15, member for member (set difference empty).
- **Control.** The first cut of the body filter read `archExp2` as unbodied, because an expression body also
  ends in `);`. It was corrected before any number above was taken; `archExp2` is the positive control and
  `rawVforkSyscall` the negative.

So the ceiling on throwing stubs at linux/arm64 is **15**, not 58. Nine exist only at linux/arm64 (the record's
rows 7 to 15, classes (b) and (c)); six are each arch's own assembly and are stubbed at linux/amd64 too. P0
stands as written, against these 15 names.

### 9.3 GolibTests at linux/arm64 (§8.4's last line)

Run 42979136871 at `50dd9a065c`, `golib-tests`, the arm leg only:

- **It compiles.** That is the claim S7-b's arm leg depends on, and it holds at the E1 tree.
- **It does not pass: 1845 passed, 2 failed, and the test host exited.**
  - The two failures are `LinuxRuntimeSyscallNumberTests`' flavour cross-checks. They compare the amd64 probe
    row with the converted flavour's constants, which are arm64's on this leg (172 against 39, 135 against 14).
    The runtime's numbers are right; the tests assume the flavour. The fix is on `claude/i9-s3-s2-sites` at
    `11a355e936` and is not on master.
  - The host exited while `RuntimeCPUProfilerTests.StartStopThenStartAgain` ran. **The cause is not the
    profiler.** i9 localised it (runs 42967529386, 42967662456, 42969401661, 42970423967):
    `runtime_pollServerInit` in `src/core/internal/poll/linux/runtime_netpoll_impl.cs` carries an admission
    guard, X64 only, and throws `PlatformNotSupportedException` at the first pollable `FD.Init`. pprof's writer
    opens `/proc/self/maps`, so the test died at its first open. Read here: the guard is in that function at
    master, and run 42970423967's log prints the exception by its message ("only X64 is admitted", 2 hits).
    Run 42979136871's own log does not print it (0 hits); it carries no first-chance trace.
- **What it means for S7.** At master, a converted program at linux/arm64 dies at its first `os.Open` or
  `os.Create`. A row that opens a file reads UNMEASURED with that reason (§5.3). It is not a divergence to
  disclose, and it is not a profiler limitation. The lift is on `claude/i9-s3-s2-sites` and rides batch 285,
  with a live arm64 epoll reading in the same commit.
- **RULED 2026-11-11: wave 1's arm64 banking waits for batch 285 to land with the guard lift.** No arm64 row
  banks on master as it stands. A-0, and preparation that banks nothing, proceed now.
- **A prediction for A-0, written before its dispatch.** `container/heap`'s tests open no file, so P0 stands on
  the arm leg at master. If the row's host does open one, the row reads UNMEASURED for the guard, and the proof
  and the calibration still read.

The linux-x64 GolibTests leg at the same SHA is green (run 42962618559).

## 10. A-0, read (dated block, 2026-11-11)

Two hosted runs at master `50dd9a065c`, each on both linux legs, each with `headSha` read equal to the tip:
the proof and calibration run 42982194260 (green), and its red arm 42982976703 (`container/heap;dumpproof=red`,
red by design). Sections 0 to 9 stand as written; this block moves one gate and scores §3's and §4.3's
predictions for A-0. **No other shard has been dispatched, and no row is banked by either run.**

### 10.1 The gates

| Gate | State | Read |
|---|---|---|
| A-0's proof green and read | **CLEARED** | §10.2, both legs; the proof was made to fail once on the same images, §10.3. |
| Wave 1's arm64 banking | **HELD** (ruled 2026-11-11) | Unchanged. Until batch 285 lands with the netpoll guard lift (§9.3). |

Two conditions on the first combined bank stand beside the hold: the S7-a fixups land first (§9.1, E4), and the
"not triaged" ledger rows of the nine floor-set packages are triaged before wave 1 banks.

### 10.2 The proof and the row (run 42982194260)

| Reading | linux-x64 | linux-arm64 |
|---|---|---|
| Runner image | `ubuntu-26.04` | `ubuntu-24.04-arm` |
| `ulimit -c`, soft / hard | unlimited / unlimited | 0 / unlimited |
| Stock `core_pattern` | a pipe to `systemd-coredump` | a pipe to `systemd-coredump` |
| apport / systemd-coredump socket | inactive / active | inactive / active |
| Free bytes on `/` | 92,443,172,864 | 112,946,515,968 |
| Dump files before the proof | 0 | 0 |
| Control: SIGSEGV of a one-thread process | 1 new file (2,039,808 bytes) | 1 new file (421,888 bytes) |
| Control: a 128 MiB write | seen, 134,217,728 bytes | seen, 134,217,728 bytes |
| Prove, SIGQUIT, 2,000 threads | status 131, 0 new files | status 131, 0 new files |
| Prove, FailFast, 2,000 threads | status 134, 0 new files | status 134, 0 new files |
| Largest free-space use in the prove step | 28,672 bytes | 32,768 bytes |
| Recheck after the sweep | 0 new files, PASS | 0 new files, PASS |
| `container/heap` | PASS, 7 `(linux)` | PASS, 7 `(linux (linux-arm64 pending))` |
| Tracked-rewrite diff after the row | 0 bytes | 0 bytes |

The scores:

- **PD1** (the soft limit already reads 0): **HIT on arm64, MISS on x64**, where it reads unlimited.
- **PD2** (the stock pattern is a pipe to apport): **MISS as worded, on both legs.** The pipe is to
  `systemd-coredump` and apport is inactive. The part the step depends on holds: the handler is piped, so a limit
  of 0 does not stop it, and the step replaces the pattern.
- **PD3** (the control leaves exactly one file): **HIT** on both legs.
- **PD4** (both crashes leave 0 files and stay under 64 MiB): **HIT** on both legs.
- **P0** (`container/heap` reads its x64 reading): **HIT.** 7 verdicts on both legs, and the arm leg's roster
  note is the derived form of §2.4. It is the first converted test host started on the flavour.
- **§9.3's prediction** (the row's host opens no file, so the guard does not end it): **HIT.**

One difference from §3's text: step 4 names SIGSEGV as the outside crash. The step at master sends SIGQUIT, for
the reason its own header records, and the status read is 131 on both legs.

A green A-0 clears only the stubs its start path reaches (§4.3). It says nothing about a row that opens a file.

### 10.3 The proof made to fail (run 42982976703)

Four predictions were written to a file before the dispatch. All four **HIT**, on both legs:

- **R1.** Record and control read as in §10.2, and the apply step prints `RED ARM` and applies nothing.
- **R2.** The prove step fails by name at the first crash: "the sigquit crash of a 2000-thread .NET probe left 1
  dump file(s); the sweep does not run".
- **R3.** The sweep never starts and the recheck does not run: 0 verdict lines and 0 recheck lines in either log.
  The same two greps read 2 and 2 in each log of the green run, which is their control.
- **R4.** Both jobs and the run conclude `failure`.

| The dump the red arm left | linux-x64 | linux-arm64 |
|---|---|---|
| Apparent size | 33,712,996,352 bytes | 33,715,597,312 bytes |
| On disk | 47,390,720 bytes | 66,560,000 bytes |

Three readings follow from that table:

- **The file census is the detector that fired.** The step fails at the file count, before it evaluates free
  space. By the on-disk sizes above, the 64 MiB bar (67,108,864 bytes) would not have failed either leg on its
  own (inferred from the sizes; the bar was not evaluated). The bar still covers a dump that is not sparse and a
  run that leaves several. A tighter bar is possible, since the prove step used under 33 KiB when suppressed; it
  is a workflow change and is not made here.
- **The apparent size is the lane-box number.** 33.7 GB per crash is the order of §3's 211 GB over six aborts. On
  the runner's filesystem the file is sparse. A filesystem that stores it whole pays the apparent size (an
  inference from the two sizes, not a measurement of the lane box).
- **The red arm's scope.** It runs with the control's settings (a file pattern, the limit lifted), not with the
  stock handler. What the stock `systemd-coredump` pipe does with a 2,000-thread crash is not measured, and S7
  does not depend on it: every job replaces the pattern.

### 10.4 The calibration

| Seconds | linux-x64 | linux-arm64 | arm / x64 |
|---|---|---|---|
| Job wall | 228 | 274 | 1.20 |
| Before the sweep step (setup and the proof) | 39 | 42 | |
| The converter build (by subtraction) | 15 | 22 | |
| The row's wall, as the runner prints it | 168 | 203 | 1.21 |
| After the row | 6 | 7 | |

- **The ratio is 1.21, under 1.5, so §2.2's re-cut rule does not fire** and no ESTIMATE is recomputed.
- **The table is a loose upper bound.** A-0's own ESTIMATE was 35 minutes and the arm job took 4.6. The fixed
  term is 30 minutes per shard, 450 of the table's 1,298 minutes; here everything but the row took 71 seconds.
  The row's wall includes the first row's corpus build for `container/heap`'s closure, which is small. A larger
  closure builds more, and A-0 does not measure that. The first shard of wave 1 is the first reading of it.
- **The two legs are not one image.** x64 runs `ubuntu-26.04` and arm64 runs `ubuntu-24.04-arm`, so the ratio
  compares image and arch together. One row on one run is one sample; 1.5 stays the planning factor.
- **Disk.** Both legs start with more than 92 GB free, against the sweep's 25 GB floor.

## 11. The floor-set ledger rows, triaged (dated block, 2026-11-11)

**Why.** The flavour census guard's ledger (`declaredFlavourOnlyStubs` in
`src/go2cs/declaredNotImplementedFlavour_test.go`, on `claude/g-flavour-census-guard` at `d740c1a31a`) carries 76
rows whose disposition reads "not triaged: stubbed before this guard (2026-11-10)". Each row is a bodyless
`partial` that PartialStubGenerator bodies with a throw: the project compiles and the member dies at its first
call. A standing ruling (§10.1) orders the rows in the nine floor-set packages triaged before wave 1 banks. This
block is that triage. It is a reading. It changes no code and no ledger row.

**Read at.** `34d303270c` (`claude/g-la64-s7-e1`). Its `src/core` is byte-equal to master `50dd9a065c`
(`git diff --quiet` between the two over `src/core` exits 0). Go's side is read in GOROOT at go1.26.10.

**The population.** 76 untriaged rows. 69 sit in floor-set packages: runtime 66, syscall 1, os 1, internal/cpu 1.
The other five floor-set packages (net, internal/poll, internal/runtime/atomic, sync/atomic, math) hold no
untriaged row. 27 of the 69 are stubs at linux/arm64: the ledger's `atLinux` 24 and `atUnix` 3. The 7 rows outside
the floor set are the post-hop reading list and are not read here: internal/runtime/gc/scan `ExpandAVX512`,
`FilterNilAVX512`, `scanSpanPackedAVX512`; vendor/golang.org/x/sys/cpu `cpuid`, `xgetbv`;
internal/runtime/syscall/windows `StdCall`, `asmstdcall`.

**The method.** Five instruments, all static.

1. A reference census: every line in the package's flat files and key folders that names the member, classed as
   declaration, definition, reference, qualified reference or comment.
2. A reverse call closure over names, per target view (the flat files plus that target's goos, arch and goos-arch
   folders, the set its project compiles). Two modes. "all": any unqualified occurrence of a function's name in a
   body is an edge. "calls": only a call spelling is an edge. Roots are public functions, public functions in
   `*_impl.cs`, functions in test files, and each file's top-level initialisers.
3. A hand read of the emitted lines wherever the closure does not settle a row.
4. A cross-package check: every closure of 60 names or fewer, intersected with the names other packages call as
   `<package>_package.NAME(` across `src/core` and `src/gen`.
5. Go's own declaration and Go's own call sites for each member.

**Controls.** Closure: `emptyfunc` (no emitted caller) reads 0 direct callers; `walltime` reads reached, through
`time_now`, at linux/arm64; a name that does not exist reads ABSENT. Cross-package: runtime has 40 distinct names
called from other packages, in 76 calls over 4,180 files, and exactly one of the 28 small closures is entered:
`vgetrandom`, from `internal/syscall/unix/linux/getrandom.cs:17`, which is the entry already known to be live. The
other 27 read no entry. Syscall has 4 such names in 5 calls, and `rawVforkSyscall`'s closure is not entered.

**The limit.** The graph holds names, not receivers. Method names shared across types (`start`, `find`, `init`,
`write`, `alloc`, `push`, `next`, `schedule`) weld most of runtime into one component: at linux/arm64, 2,411
names with 1,002 roots in "calls" mode and 3,894 names with 1,770 roots in "all" mode. So "no root reaches it" is
the strong reading, and "a root reaches it" is only a lead, which a hand read then settles or does not. "calls"
mode misses a function taken as a value; "all" mode covers that and over-counts: it takes a string literal, a
local or a tuple element of the same name as an edge. Each such edge met below was read by hand and is named in
its row. Nothing here was executed. No row was run to its stub.

**The classes.**

- **(a) reached.** A path from a root to the stub was read line by line. The guard's own rule then applies: a
  member that is reached at its target is bodied, not rowed.
- **(b) Go-side.** Go has no Go call of the member. Assembly owns the symbol, or Go names it only to take its
  address. An address take does not run the stub. This is the ledger's "Go-side" disposition.
- **(c) refused.** Go calls the member and this emission does not reach the call. Three reasons, named per row:
  *no call* (the emission holds no call site at the row's targets), *guard* (the call sits behind a test that no
  code lifts), *no root* (callers exist and no root reaches them). This is the ledger's "refused" disposition.
- **open.** The reading does not exclude a reach.

| | (a) reached | (b) Go-side | (c) refused | open | rows |
|---|---|---|---|---|---|
| All floor-set rows | 2 | 38 | 26 | 3 | 69 |
| The rows at linux/arm64 | 2 | 3 | 21 | 1 | 27 |

Of the 26 refused: 7 *no call*, 4 *guard*, 15 *no root*. Every linux row reads the same at linux/amd64 and at
linux/arm64.

### 11.1 The two reached rows

**`sched_getaffinity` is reached from a public function, with no branch before the stub.**
`runtime.SetDefaultGOMAXPROCS()` (`src/core/runtime/debug.cs:23`) calls `defaultGOMAXPROCS(0)` at `:28`, before
it takes the scheduler lock. `defaultGOMAXPROCS` (`linux/cgroup_linux.cs:83`) calls `getCPUCount()` at `:96`
whenever its argument is not positive. `getCPUCount` (`linux/os_linux.cs:97`) calls the stub at `:107`. So a call
of `runtime.SetDefaultGOMAXPROCS()` at either linux target throws.

- Who calls it: nothing in `src/core`. The name occurs once, at its definition. Only user code reaches it. Go's
  own tests of it are `runtime/cgroup_linux_test.go` and `testdata/testprog/gomaxprocs.go`.
- The other callers of `getCPUCount` are `osinit` (`linux/os_linux.cs:369`) and `defaultGOMAXPROCSInit`
  (`linux/cgroup_linux.cs:61`); no root reaches either.
- For the fix seat: Go's raw syscall returns the number of bytes written, and `getCPUCount` slices its buffer by
  that count. The C library's wrapper returns 0 on success. A body that forwards the wrapper's return reads zero
  CPUs. The seat owes a guard that is red first.

**`exit` is reached on a failure path only.** `sysAllocOS` (`linux/mem_linux.cs:19`) calls `exit(2)` at `:24` and
`:28`, when `mmap` fails with EACCES or EAGAIN. `sysAllocOS` is live: the hand-owned persistent allocator calls
`sysAlloc` (`mem_persistent_impl.cs:96`, `:108`), and `linux/mem_linux_impl.cs:101` calls `sysAllocOS` directly.
In that state the process throws from the stub after printing Go's message, where Go exits with status 2. The
other 11 functions that call `exit` at linux are not reached: `Main`, `syscall_Exit`, `fatalthrow`, `fatalpanic`,
`startpanic_m`, `cgocallbackg`, `newosproc0`, `needm`, `sighandler`, `fatalsignal`, `badsignal`. windows and
darwin carry converted bodies of `exit`; linux is the only target where it is a stub.

### 11.2 The three open rows

**`osyield` (linux/amd64, linux/arm64).** 12 functions call it. Read and excluded:

- `addNonGo`, `osyield_no_g`, `traceCPUSample`, the profiler's `add` and `syscall_runtime_BeforeExec` have no
  reached caller.
- `setcpuprofilerate` is live, and its spin (`linux/proc.cs:5575`) waits on `prof.signalLock`, which the opt-in
  sampler never takes.
- `runqgrab` (`linux/proc.cs:7463`) calls it only in the `else` of `if (!osHasLowResTimer)` (`:7457`), and
  `osHasLowResTimer` is the constant `false` at linux (`linux/proc.cs:1173`).
- `ensureSwept` through `addfinalizer`: `addfinalizer` has no caller, because `SetFinalizer` and `AddCleanup` are
  hand-owned whole files (`mfinal.cs`, `mcleanup.cs`) that bind the host's own finalizers.

Not excluded: `casgstatus`, `getGCMaskOnDemand`, `suspendG`, `traceAdvance`, and `ensureSwept`'s other callers.
All sit in the welded component. Each call is a spin taken under contention, so a quiet run does not show it.
windows and darwin carry converted bodies of `osyield`; linux is the outlier. Go's body is one `sched_yield`
system call. Bodying it is cheaper than proving that it is never reached.

**`time_now` (windows/amd64, linux/amd64).** Three callers: `time_runtimeNow` (no caller; `time.now` is
hand-bodied in `time/time_impl.cs:80`), `gcMarkTermination` and `traceSyncBatch`. The last two sit in the welded
component, behind `gcStart` and `traceAdvance`. `GC`, `StartTrace` and `StopTrace` are replaced by hand-owned
bodies, which removes the public entries, and the remaining callers of `gcStart` are the allocator's. Not
excluded. It is not a wave 1 question: at linux/arm64 the member is bodied (`linux-arm64/timestub2_impl.cs`).

**`getlasterror` (windows/amd64).** One call is on a normal path: the poller's timeout test
(`windows/netpoll_windows.cs:201`). It sits behind the `iocphandle` guard at `:153-156`, and `iocphandle` starts
invalid (`:97`) with one writer (`:107`), in `netpollinit`, which has no caller. Every other call prints the
error number on a branch that then throws. Of the functions holding those, `initHighResTimer`, `minit`,
`netpollinit`, `netpollopen`, `profileLoop`, `semasleep` and `semawakeup` are not reached; `newosproc`, `preemptM`
and `semacreate` sit in the welded component. So the open part is a failure path whose next statement is a throw.

### 11.3 The table

Paths in the last column are under `src/core/runtime/` unless the row names another package. "Closure N" is the
"all"-mode reverse closure at the row's targets.

| # | member | Go's declaration | class | reach and disposition |
|---|---|---|---|---|
| 1 | `access` | stubs_linux.go:18 | (c) no call | Go's one caller is write_err_android.go, which no target compiles. The emission holds the declaration (`linux/stubs_linux.cs:18`) and no call. The two "all" hits are the word inside string literals. |
| 2 | `callCgoMmap` | cgo_mmap.go:63 | (c) guard | One caller, `mmap`, behind `_cgo_mmap != nil` (`linux/cgo_mmap.cs:31`). `_cgo_mmap` is declared at `:16`; nothing in `src/core` or `src/gen` writes it. |
| 3 | `callCgoMunmap` | cgo_mmap.go:70 | (c) guard | The same for `munmap` and `_cgo_munmap` (declared `:22`, guard `:50`). |
| 4 | `callCgoSigaction` | cgo_sigaction.go:98 | (c) guard | One caller, `sigaction`, which returns early while `_cgo_sigaction == nil` (`linux/cgo_sigaction.cs:37`); nothing writes it. `sigaction` is called only from `setsig`, `setsigstack` and `getsig` (`linux/os_linux.cs:513-532`), and no root reaches those (closures 12, 5, 6). |
| 5 | `connect` | stubs_linux.go:19 | (c) no call | As row 1. No reference at all in the emission. |
| 6 | `exit` | stubs2.go:20 | (a) reached | On a failure path only. §11.1. |
| 7 | `exitThread` | stubs2.go:44 | (c) no root | One call, in `mexit` (`linux/proc.cs:2149`). Closure 2, no root. |
| 8 | `futex` | os_linux64.go:16 | (c) no root | Callers `futexsleep` and `futexwakeup`. Closure 4, no root: the lock and note primitives above them are hand-owned. |
| 9 | `gettid` | os_linux.go:392 | (c) no root | One call, in `minit` (`linux/os_linux.cs:420`). `minit`'s closure is 7, no root. The other "all" hits are a tuple element of the same name (`linux/signal_posix_impl.cs:706,717`). |
| 10 | `osyield` | os_linux.go:456 | open | §11.2. |
| 11 | `raise` | os_linux.go:451 | (c) no root | One call, in `raisebadsignal` (`linux/signal_unix.cs:981`). Closure 3, no root. |
| 12 | `raiseproc` | os_linux.go:452 | (c) no root | One call, in `sighandler` (`linux/signal_unix.cs:756`). Closure 14, no root. |
| 13 | `rt_sigaction` | os_linux.go:559 | (c) no root | One caller, `sysSigaction`, called only by `sigaction`; row 4 names `sigaction`'s three callers. The "all" mode joins the welded component through the string `"sigaction"` in two native entry-point attributes. |
| 14 | `sbrk0` | stubs_linux.go:11 | (c) no root | One call, in `mallocinit` (`linux-arm64/malloc.cs:717`, and its amd64 twin). Closure 2, no root. |
| 15 | `sched_getaffinity` | os_linux.go:455 | (a) reached | From `runtime.SetDefaultGOMAXPROCS()`. §11.1. |
| 16 | `setitimer` | os_linux.go:434 | (c) no root | One caller, `setProcessCPUProfilerTimer`. Closure 16, no root. The second "all" hit is a local of the same name. |
| 17 | `sigaltstack` | os_linux.go:431 | (c) no root | Closure 17, no root. |
| 18 | `sigreturn__sigaction` | os_linux.go:426 | (b) Go-side | Go has no call; `setsig` takes its address. The emission does the same, and no root reaches `setsig` (closure 13). |
| 19 | `socket` | stubs_linux.go:20 | (c) no call | As row 1. The one "all" hit is the word inside a signal-table string. |
| 20 | `timer_create` | os_linux.go:437 | (c) no call | Go's callers are all in `setThreadCPUProfiler` (os_linux.go:648-697). Its converted body is replaced by `linux/cpuprof_linux_impl.cs:47`, which holds no call. The "all" hits are a local of the same name in `validSIGPROF` (`linux/os_linux.cs:618`). |
| 21 | `timer_delete` | os_linux.go:440 | (c) no call | As row 20. No reference at all in the emission. |
| 22 | `timer_settime` | os_linux64.go:19 | (c) no call | As row 20. No reference at all in the emission. |
| 23 | `vgetrandom1` | vgetrandom_linux.go:16 | (c) guard | `vgetrandom` is live across packages and returns at `vgetrandomAlloc.stateSize == 0` (`linux/vgetrandom_linux.cs:102`) before the call. The one writer of `stateSize` (`:43`) is in `vgetrandomInit`, called from `osinit`, which has no caller. |
| 24 | syscall `rawVforkSyscall` | syscall_linux.go:103 | (c) no root | Calls in `forkAndExecInChild1` (`src/core/syscall/linux/exec_linux.cs:345,351,353`) and `doCheckClonePidfd` (`:931,933`). `forkAndExecInChild` has no caller. `os.checkClonePidfd` is hand-bodied (`src/core/os/linux/pidfd_linux_impl.cs:50`) and returns without the probe. No cross-package call enters. |
| 25 | `cgoSigtramp` | os_linux.go:428, os_darwin.go:418 | (b) Go-side | Go has no call; `setsig` takes its address. The emission does the same at linux and at darwin. |
| 26 | `sigtramp` | os_linux.go:427, os_darwin.go:417 | (b) Go-side | As row 25. |
| 27 | `sigfwd` | signal_unix.go:1159 | (c) no root | One caller, `sigfwdgo`. Closure 2, no root, at linux and at darwin. |
| 28 | internal/cpu `xgetbv` | cpu_x86.go:15 | (c) no root | One call, in `doinit` (`src/core/internal/cpu/amd64/cpu_x86.cs:141`), under `Initialize` (`src/core/internal/cpu/cpu.cs:202`), under runtime's `cpuinit` (`proc.cs:795` in each goos folder), under `schedinit`, which has no caller at any amd64 target. `amd64/cpu_x86_impl.cs:33-36` fills `X86` natively instead. |
| 29 | `asmcgocall_landingpad` | stubs_amd64.go:45 | (b) Go-side | No Go call in Go. No reference in the emission. |
| 30 | `gcWriteBarrierBP`, `BX`, `CX`, `DX`, `R8`, `R9`, `SI` (7 rows) | stubs_amd64.go:10-16 | (b) Go-side | The compiler's write-barrier entries; assembly only. The reference census reads 0 references for all 7. |
| 31 | `retpolineAX`, `BP`, `BX`, `CX`, `DI`, `DX`, `R8` to `R15`, `SI` (15 rows) | stubs_amd64.go:25-39 | (b) Go-side | The compiler's retpoline thunks; assembly only. The reference census reads 0 references for all 15. |
| 32 | `settls` | stubs_amd64.go:22 | (b) Go-side | Go's own comment says it is called from assembly only. No reference in the emission. |
| 33 | `stackcheck` | stubs_amd64.go:19 | (c) no root | One caller, at windows only: `minit`. Closure 5, no root. No call at linux or darwin. |
| 34 | `time_now` | timeasm.go:14 | open | windows/amd64 and linux/amd64 only. §11.2. |
| 35 | `callbackasm` | syscall_windows.go:226 | (b) Go-side | Go takes its address in `callbackasmAddr`; its only call spellings are text printed by the generator wincallback.go. The emission holds the one take. Closure 8; the one "all" root is the word inside a position-map attribute string. |
| 36 | `callbackasm1` | os_windows.go:1040 | (b) Go-side | No reference in the emission. |
| 37 | `exceptiontramp` | signal_windows.go:36 | (b) Go-side | One address take. Closure 2, no root. |
| 38 | `firstcontinuetramp` | signal_windows.go:37 | (b) Go-side | As row 37. |
| 39 | `lastcontinuetramp` | signal_windows.go:38 | (b) Go-side | As row 37. |
| 40 | `getlasterror` | os_windows.go:352 | open | Failure paths only. §11.2. |
| 41 | `osSetupTLS` | tls_windows_amd64.go:10 | (c) no root | One caller, `needm`. Closure 2, no root. |
| 42 | `sehtramp` | signal_windows.go:39 | (b) Go-side | No reference in the emission. |
| 43 | `sigFetchGSafe` | signal_windows.go:113 | (c) no root | One caller, `sigFetchG`. Closure 2, no root. |
| 44 | `sigresume` | signal_windows.go:40 | (b) Go-side | One address take. Closure 1, no root. |
| 45 | `tstart_stdcall` | os_windows.go:156 | (b) Go-side | One address take, in `newosproc`. |
| 46 | `wintls` | os_windows.go:159 | (b) Go-side | No reference in the emission. |
| 47 | os `readdir_r` | dir_darwin.go:148 | (c) no call | The hand-owned `readdir` binds the C library's `readdir_r` natively (`src/core/os/darwin/dir_darwin_impl.cs:103-107`). The converted declaration (`src/core/os/darwin/dir_darwin.cs:63`) has no caller; the one "all" hit is the name inside that binding's string. |
| 48 | `mstart_stub` | os_darwin.go:268 | (b) Go-side | One address take, in `newosproc`. |
| 49 | `syscall_x509` | sys_darwin.go:88 | (b) Go-side | One address take, in `crypto_x509_syscall`, which has no caller in the package or across packages. |

Rows 1 to 27 are the 27 at linux/arm64 (1 to 24 `atLinux`, 25 to 27 `atUnix`). Rows 28 to 33 are `atAmd64`, 34
is `atWindowsLinuxAmd64`, 35 to 46 are `atWindows`, 47 to 49 are `atDarwin`. 49 table rows hold 69 ledger rows.

### 11.4 What this block does not do

- **It bodies nothing.** The two reached rows and `osyield` need a fix seat. Until one lands, the ledger's
  "not triaged" text is still the true description of those three.
- **It edits no ledger row.** The guard's branch takes no more commits, so the 64 dispositions that are settled
  here (38 Go-side, 26 refused) reach the ledger in a later seat, after the guard lands.
- **It does not read the 7 rows outside the floor set.**
- **It proves no absence by running.** A row classed (c) *no root* rests on the "all" closure and the hand reads
  named in its row. A change that gives one of those islands a caller moves the row; the guard does not see that,
  because it reads declarations, not reach.

One instrument note for the next reader: whole-file hand-owned files carry `[module: go.GoManualConversion]`, with
the `go.` prefix. A search anchored on the attribute name without the prefix misses them, and missed `mfinal.cs`
and `mcleanup.cs` here until their headers were read.
