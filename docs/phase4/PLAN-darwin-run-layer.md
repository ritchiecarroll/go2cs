# PLAN — the darwin run layer at Go 1.24.13, and the path to darwin row validation

> Lane C1, 2026-10-02, written at master `172d437e66` (the 1.24.13.3 release's announcement commit).
> Requested by COORD (mailbox `54cc3b61a0`), re-scoped by COORD's ruling of 2026-10-02 00:40Z after the
> premise correction in §0. **A plan, not a cut: COORD rules before any seat is opened.**
> Companions: [`FINDING-darwin-run-layer.md`](FINDING-darwin-run-layer.md) (why a darwin program died
> before `Main`, and §7–§8: the run layer that fixed it),
> [`DESIGN-darwin-run-layer.md`](DESIGN-darwin-run-layer.md) (the keystone's ABI reading),
> [`FINDING-linux-run-layer.md`](FINDING-linux-run-layer.md) (the template, found by running).
> C2's sizing, `SIZING-darwin-option2.md` on `claude/c2-darwin-option2-sizing` (`43e0dff04c`), was
> never merged; §0 says why this plan does not build on its premise.

**⚠ THE LIMIT THAT BOUNDS EVERYTHING BELOW.** There is no Apple hardware in the fleet. Every darwin
*run* reading comes from GitHub's two hosted mac runners (`macos-15`, arm64; `macos-15-intel`,
x64) through `.github/workflows/os-matrix.yml`. A lane can dispatch that workflow and read its
result (§4), so the loop exists, but it is paced in tens of minutes per reading, and nothing here
can be single-stepped. What a container can do is read the committed corpus and run red-first tests
on linux for mechanisms that are portable. §3's seats are chosen on that line.

---

## 0. The premise, corrected

The request and C2's sizing both read darwin as *"compiles, does not run"*. Measured at this base,
that is no longer true, and has not been since 2026-09-03:

| fact | evidence |
|:--|:--|
| the run layer exists | the keystone and a real `FuncPCABI0` landed as `88f01638c` (train 19, 2026-09-03): `runtime/darwin/libccall_impl.cs`, golib `GoLibcCall` (arity 0–9 over Cdecl), `GoCgoDynamicImports.SymbolOf`; `FuncPCABI0` is `FuncPC` (`internal/abi/funcpc_impl.cs:50`) |
| the sizing is stale on its own base | `88f01638c` is an ancestor of `a02ac3df3`, the commit the sizing measured on 2026-09-13. The sizing still says `FuncPCABI0` "returns 0" and the ten keystones are throwing stubs |
| darwin runs | FINDING §7–§8: 618/632 (arm64) and 619/633 (x64) behavioral programs Go-identical at Output, 2026-09-03 |
| the latest full reading | BOARD, *THE TRAIN-25 DARWIN CENSUS* (2026-09-05, `db9e95841`, Go 1.23.12): measurable 670 arm64 / 671 x64, the same twelve failing on each leg, every door named |
| what had never been measured (until §2.1) | darwin increments 7–12 (sockaddr twin, `forkExec` over `posix_spawn`, ptrout and gostring, `Getaddrinfo`), the libc funnel's order-token marshal (2026-09-27), and the whole 1.23 → 1.24 hop |
| what still runs daily | the darwin **compile** census, green on both legs through 2026-10-01 (`f819887fa3`, a 1.24.13 tree) |

`docs/CIMatrix.md:222` still quotes the pre-keystone *"failed all twenty"* reading as the darwin
state. That is a documentation-currency item, seat S5 below.

---

## 1. The sizing's structural counts, re-derived at `172d437e66`

**Method.** Each predicate was first CALIBRATED against the sizing's own base `a02ac3df3`, and only
used at `172d437e66` once it reproduced the sizing's published reading there. Counted from the git
tree (`git ls-tree` / `git cat-file` / `git grep`), never from a working copy.

| reading | sizing (`a02ac3df3`) | calibration (`a02ac3df3`) | **now (`172d437e66`)** | delta |
|:--|--:|--:|--:|:--|
| bodyless partials, darwin / linux / windows flavor | 288 / 95 / 53 | 288 / 95 / 53 exact | **280 / 84 / 43** | −8 / −11 / −10, every member named below |
| darwin per package: `syscall` / `runtime` / `internal/syscall/unix` / `internal/poll` / `os` | 147 / 55 / 37 / 12 / 4 | exact | **147 / 56 / 39 / 2 / 3** | 0 / +1 / +2 / −10 / −1 |
| `abi.FuncPCABI0(` sites, darwin flavor | 263 | 263 exact | **266** | +3 |
| distinct `*_trampoline` whose address is taken | 208 | 208 exact | **211** | +3, named below |
| `libc_*_trampoline` declarations, `syscall/darwin` / whole flavor | 126 / 142 | 126 / 142 exact | **126 / 144** | 0 / +2 |
| `cgo_import_dynamic` lines in `syscall/zsyscall_darwin_amd64.cs`, name/symbol mismatches | 123, 0 | 123, 0 | **123, 0** | none |
| `_impl.cs` companions, darwin flavor / linux flavor | 15 / 17 | 15 / 17 exact | **18 / 21** | +3 / +4 |
| files naming `GoManualConversion`, darwin `runtime` / `syscall` / `internal/syscall/unix` | 15 / 7 / 4 | exact | **24 / 7 / 4** | +9 / 0 / 0 |
| `.cs` files under `runtime/**/darwin` / `runtime/**/linux` | 57 / 62 | exact (the count includes runtime's nested packages) | **62 / 68** | +5 / +6 |
| keystone lines (every line naming one of the ten, minus GoInit initializers and comments) | 215 | **217** | **217** | 0 |
| declarations still filled by a throwing stub, darwin / linux | 243 / 62 | 244 / 57 | **247 / 48** | +3 / −9 |
| darwin-folder files named `*amd64*` / `*arm64*` | 9 / 0 | 9 / 0 | **9 / 0** | none |

**Two readings do not reproduce exactly, and the plan says so rather than carrying them.** The keystone
breakdown (111 / 62 / 20 / 5 / 4 / 4 / 3 / 2 / 2 / 2) cannot be recovered from the sizing's prose: its
predicate excludes some nullary declarations (`syscall6X();`, `syscallPtr();`) and counts others. The
stated predicate above reads 217 at both bases, so the keystone family is UNCHANGED by the hop. And
the stub-filled count depends on a "bodied" definition the sizing does not state. These two readings
are kept as old against new under ONE stated predicate, where the delta is what a plan needs.

**Every bodyless delta, by name.** The same twelve leave all three flavors: Go 1.24 gave
`internal/poll`'s ten runtime hooks (`runtime_pollServerInit`, `runtime_pollOpen`, `runtime_pollClose`,
`runtime_pollReset`, `runtime_pollWait`, `runtime_pollWaitCanceled`, `runtime_pollSetDeadline`,
`runtime_pollUnblock`, `runtime_isPollServerDescriptor`, `runtimeNano`), `net.runtime_rand` and
`os.sigpipe` bodies in Go source. Each flavor then adds its own 1.24 members:

| flavor | added | what it is |
|:--|:--|:--|
| darwin | `internal/syscall/unix::libc_mkdirat_trampoline`, `libc_readlinkat_trampoline` | 1.24's `*at` family behind `os.Root` |
| darwin | `runtime::arc4random_buf_trampoline` | 1.24's `runtime.readRandom` source on darwin |
| darwin | `vendor/golang.org/x/sys/cpu::syscall_syscall6` | x/sys/cpu's darwin AVX-512 probe, `//go:linkname`d to `syscall.syscall6` |
| linux | `runtime::vgetrandom1` | the vDSO getrandom |
| windows | `runtime::GetErrorModeNative`, `SetErrorModeNative`; −1 `internal/syscall::tests` | — |

**What each 1.24 darwin delta should do, read from the tree:**

- **`arc4random_buf`, `mkdirat`, `readlinkat` resolve.** Each carries a `GoCgoImportDynamic` record
  (`runtime/darwin/package_info.cs:295`; `internal/syscall/unix/darwin/package_info.cs:84, 96, 99`). So
  `FuncPCABI0` resolves them by the same path that held across all 659 programs in FINDING §8.
- **x/sys/cpu's `syscall_syscall6` is DORMANT.** The package's only importer in GOROOT is
  `chacha20poly1305_amd64.go`, built `gc && !purego`. `purego` is the corpus's tag, the corpus emits no
  amd64 chacha20poly1305 file, and no corpus project references `vendor.golang.org.x.sys.cpu`. So its
  `init` never runs on any OS. The stub is unreachable, as are its own `cpuid` and `xgetbv`, which are
  also bodyless. It becomes LIVE the day `purego` stops applying to that package, and is recorded as
  the condition that would wake it.

**The keystone family and the libc map did not move with the hop.** The +3 trampolines are all new
1.24 libc entries with records, so the run layer's structural surface is essentially what it was when
train 25 measured it.

---

## 2. STEP 2 FIRST — the 1.24.13 re-baseline (dispatched 2026-10-02)

**Why it goes first.** Everything §3 would cut is aimed at doors last measured on Go 1.23.12, before
six darwin increments and the hop. A seat aimed at a door that has since moved is wasted, and a
reading of where each door stands now costs one dispatch and about an hour.

**The dispatch.** `os-matrix.yml`, `goos=darwin stage=behavioral-full dotnet=10.0.x`, on the pinned ref
`claude/c1-darwin-baseline` = `172d437e66` exactly (a branch, because `workflow_dispatch` takes a ref,
and master may move with the release record). Run
[36947612442](https://github.com/ritchiecarroll/go2cs/actions/runs/36947612442), head SHA read back from
the run. The prediction was posted to COORD before the trigger (mailbox `ce8eabce7`). In summary:

- **Bands:** failing x64 6–14, arm64 7–15; measurable about 723 / 722 (the set grew 687 → 741 projects,
  platform-exclusive 14 → 16).
- **Expected to stay red:** `SignalPrimitives` (the `sigtramp` design door), `IpAdapterAddresses` (`sysctl`)
  and `LongPathRoundTrip`.
- **Expected to move:** the `runtime_BeforeFork` pair (increment 10 routes `forkExec` over `posix_spawn`)
  and at least two of the loopback five (the sockaddr twin, and the funnel's marshal fix).
- **Expected to cause no deaths:** the four 1.24 deltas.
- **Falsifiers:** any death in the pure class; any death naming `arc4random_buf`, `mkdirat`, `readlinkat` or
  x/sys/cpu's `syscall_syscall6`; x64 Compile below 100 %; more than three `FuncPCABI0 … no program
  counter` deaths outside `SignalPrimitives`.

### 2.1 THE READING (2026-10-02, both legs complete)

Recorded in full as the BOARD block *THE 1.24.13 DARWIN RE-BASELINE* (2026-10-02). In summary:

| leg | measurable | Output compared / pass / **fail** | not measured (120 s budget) |
|:--|--:|:--|:--|
| osx-arm64 | 722 | 690 / 686 / **4** | 4 (`NetDeadlineMatrix`, `PipeCloseUnblocksRead`, `StdoutCloseEofBarrier`, `TcpLoopbackRoundTrip`) |
| osx-x64 | 723 | 694 / 690 / **4** | 1 (`StdoutCloseEofBarrier`) |

**The same four names fail on both legs**, against train 25's twelve:

- **class L, three projects:** `IpAdapterAddresses` (`sysctl`), `LongPathRoundTrip` (`fdopendir`), and
  `StatLayoutTruth` (`fdopendir` on x64, `unlinkat` on arm64). Each is a darwin `//go:linkname` PULL to a
  body that exists in `syscall`, with no forward in the corpus.
- **class N, one project:** `LookupServicePort`, a native-array-view refusal.

`SignalPrimitives`, `LinuxSpawnBasics`, all five loopback programs on x64, and the new
`SyscallKeystonePulls` pass. The four Go 1.24 deltas caused zero deaths, and no falsifier fired.

The prediction scored 9 hits, 2 falsified and 1 missed, each row named in the BOARD block. The largest
miss: **`SignalPrimitives` passes.** The Q52 os/signal bridge (`554620235`, `sigenable` over .NET
`PosixSignalRegistration`) landed after train 25, and this plan's first version proposed it as a future
design (S4, now retired).

The pinned ref is deleted once this reading is recorded.

---

## 3. STEP 1 — the no-Mac seats, each with a red-first test that runs on linux

Ordered by what can be cut before the re-baseline returns (S1–S5), then by what waits on it (S6).
Every seat is a normal TRAIN seat: one worktree, red first, the converter suite plus GolibTests (or
repoguard) green, CNR where the converter moves, announce then push.

**S1 — the unresolvable-trampoline census (a repoguard arm).** At `172d437e66` the darwin flavor takes
the address of 225 distinct identifiers through `FuncPCABI0`. Of those, 212 have a
`GoCgoImportDynamic` record and **18 do not**: `asyncPreempt`, `cgoSigtramp`, `goexit`, `mstart`,
`mstart_stub`, `sigpanic0`, `sigtramp`, `osinit_hack_trampoline`, `raiseproc_trampoline`,
`walltime_trampoline`, `syscall_x509`, `libc_x_trampoline`, and the keystones `syscall`, `syscall6`,
`syscall6X`, `syscall9`, `syscallPtr`, `syscallX`. An identifier with neither a record nor a managed
body is exactly the train-25 `SignalPrimitives` death (`panic: FuncPCABI0: no program counter exists for
runtime.sigtramp`). The class is decidable from the committed tree, and today nothing states it.
- **The seat:** a repoguard test that lists every such identifier with a disposition. Each is a
  hand-own body, a libc record, or *dormant-by-design*, with the reason (scheduler and GC entries the
  managed model never runs, as C2's class-C read did for 43 runtime trampolines on 2026-09-03). A new
  undeclared member fails by name.
- **Red first:** a synthetic flavor with one taken address that has no record and no body, which must
  be reported; and the real tree's list before the dispositions are written.
- **No Mac needed.** It also turns the next "no program counter" death from a runner surprise into a
  diff in review.

**S2 — a GOARCH axis for platform-exclusive behavioral projects.** ⚠ **Re-read against §2.1: largely
DONE.** The arm64 leg now skips `StdLibInternalAbi [amd64]` by name before Transpile, and the runner
already classifies a best-effort conversion as NOT MEASURED. What remains, if anything, is a
red-first test pinning both behaviors, a small seat at most. Original text, kept for the record:
FINDING-darwin §8 item 4.
`StdLibInternalAbi` is native to an ARCHITECTURE (its `*_amd64.go` files), `[GoPlatformExclusive("<goos>")]`
cannot say so, and on the arm64 leg the runner's Transpile phase reported `ok` over a best-effort
conversion that only Compile caught. The seat has two parts:
- (a) the behavioral runner refuses a Transpile whose conversion was best-effort, as CNR already
  does, so the next GOARCH hole fails one phase earlier and by name;
- (b) a GOARCH dimension on the exclusive marker.

Red first, on linux: convert the project with `-platforms darwin/arm64` (the converter's own target
flag; no Mac involved) and assert the runner now refuses it at Transpile.

**S3 — the darwin `syscall` struct seams, priced against the linux six.** FINDING-darwin §8 item 3: the
linux `syscall` flavor carries six hand-owns for by-address struct seams (`sockaddr_linux_impl.cs`,
`structclass_linux_impl.cs`, `syscall_linux_impl.cs`, `syscall_linux_amd64_impl.cs`,
`zsyscall_linux_amd64_impl.cs`, `cgocaller_linux_impl.cs`). Darwin now carries three
(`exec_libc2_impl.cs`, `sockaddr_darwin_impl.cs`, `syscall_darwin_impl.cs`). The seat is a READ first:
a per-seam table of which linux seam has a darwin twin, which is not needed on darwin and why, and
which is missing. Repoguard's `nativeCallGateDarwin_test.go` already censuses the fourteen live
`libcCall` sites that pass a reference-bearing record by address; the table names which of them the
loopback five and `StatLayoutTruth` cross. Red first, for each missing twin that is cut: a GolibTests
case on linux that drives the SAME layout mechanism (a reference-bearing record handed to the libc
dispatcher by address). The dispatcher is platform-neutral, so the layout bug reproduces off darwin.

**S4 — RETIRED by §2.1.** The design this seat proposed already exists: the Q52 os/signal bridge
(`554620235`, 2026-09-05; increment 9 `d185e28b8` made Go's Ignore the kernel disposition) routes
`sigenable` over .NET `PosixSignalRegistration`, and `SignalPrimitives` passes on both legs. Kept below
as written, because it was proposed in error and the record should show it. Train 25 ruled this "sized for
COORD, not cut here". Every darwin increment so far gave a libc call a managed body, while a signal
trampoline runs the other way: the kernel calls into the process on any thread. And the CLR keeps its
own handler chain (SIGSEGV and SIGBUS become managed exceptions; `PosixSignalRegistration` handles
SIGINT and SIGTERM) that a raw `sigaction` would displace. The linux run layer has the same question
queued. The deliverable is a design document for converted `os/signal` delivery on both unixes,
probably over `PosixSignalRegistration` for the signals it supports. It states which Go signals have
no CLR equivalent, and it is reviewed before anything is cut. No Mac needed for the design; its
acceptance reading is `SignalPrimitives` on both legs.

**S5 — documentation currency.**
- `docs/CIMatrix.md` gets the run layer's actual state, replacing the pre-keystone *"failed all
  twenty"* line, with a pointer to the re-baseline reading.
- C2's unmerged sizing gets a note on its branch, or a disposition from COORD, saying its premise is
  superseded, so nobody builds on it again.

Docs only, repoguard's docs guards as the gate.

**S6 — per-door seats, ordered by the re-baseline.** For each door still red after §2: first a
`behavioral-stderr` dispatch on that project, which keeps the WHOLE stderr and the macOS crash report
the summary line cannot carry. Then a root cause from the stack. Then a red-first test on linux where
the mechanism is portable: layout, marshal, errno and pointer-token classes all reproduce through the
platform-neutral dispatcher. Where it is not portable, a darwin-gated test whose red is read on the
runner. Candidates as of train 25, to be re-read against §2: `IpAdapterAddresses` (`sysctl`),
`LongPathRoundTrip` (a stdout mismatch, never rooted), `StatLayoutTruth` (`fdopendir` on x64 against
`unlinkat` on arm64, an architecture split that is itself a finding), the loopback five (exit 134), the
`runtime_BeforeFork` pair if it did not move, and `LookupServicePort`. Each seat names its door and
its predicted move before its own acceptance dispatch, as every darwin increment so far has.

**S7 — NEW from §2.1: the darwin linkname-pull class.** The darwin flavor at `172d437e66` carries 24
bodyless declarations with a `//go:linkname` pull. A companion `*_impl.cs` fills 14, the way
`internal/syscall/unix/darwin/net_darwin_impl.cs` fills the `Sendto*`/`Recv*` family. **10 are
unfilled:**

| member → target | state |
|:--|:--|
| `vendor/golang.org/x/net/route.sysctl → syscall.sysctl` | **LIVE**, `IpAdapterAddresses` dies here |
| `internal/poll.fdopendir → syscall.fdopendir` | **LIVE**, `LongPathRoundTrip` (both legs) and `StatLayoutTruth` (x64) |
| `internal/syscall/unix.unlinkat → syscall.unlinkat` | **LIVE**, `StatLayoutTruth` (arm64) |
| `internal/syscall/unix.fstatat`, `openat` → `syscall.*` | latent, on the `os.RemoveAll` and `os.Root` paths, next behind `unlinkat` |
| `os.closedir → syscall.closedir` | latent, the twin of `fdopendir` |
| `internal/poll.writev`, `internal/syscall/unix.ioctlPtr` → `syscall.*` | latent |
| `runtime.main_main → main.main` | dormant: the managed entry point is not reached this way |
| `vendor/golang.org/x/sys/cpu.syscall_syscall6 → syscall.syscall6` | dormant under `purego` (§1) |

- **The seat:** companion bodies forwarding the eight live and latent members to their `syscall`
  bodies, following the darwin precedent rather than linux's registry rows. Plus a repoguard census arm
  listing every bodyless darwin linkname pull with its disposition (filled, or declared dormant with a
  reason). A new unfilled pull fails by name.
- **Red first, on linux:** the census arm reads 10 unfilled before the bodies and 2 (the declared
  dormant pair) after. The forwarding itself compiles in the darwin flavor, so the converter suite and
  the daily census gate it.
- **Acceptance on the runner:** a `behavioral-stderr` dispatch on the three LIVE projects, with the
  prediction posted first: each moves off class L, to a pass or to a newly named door, on both legs.

**S8 — NEW from §2.1: a run-budget input for the behavioral stages.** Five not-measured results (four
on arm64, one on both legs) sit at the fixed 120 s run budget (`GO2CS_RUN_TIMEOUT` is hard-coded in the
workflow's env). Nothing can tell slowness on the 3-core arm64 runner from a hang until a run can wait
longer. The seat adds an optional `run_timeout` input to `os-matrix.yml`, defaulting to today's 120.
Red first: a dispatch with the input set reaches the step's env, read back from the job log.

### 3.1 The proposed cut order, each with its predicted door movement

| order | seat | predicted movement on the runner |
|--:|:--|:--|
| 1 | **S7**, the linkname pulls | `IpAdapterAddresses`, `LongPathRoundTrip` and `StatLayoutTruth` leave class L on both legs. At least two pass outright; any that do not name a new door in the `os` RemoveAll path |
| 2 | **S8**, the run-budget input | none by itself. It makes the five NOT MEASURED results readable |
| 3 | **S6a**, `behavioral-stderr` on `LookupServicePort`, `StdoutCloseEofBarrier`, `NetDeadlineMatrix`, `TcpLoopbackRoundTrip`, `PipeCloseUnblocksRead` (budget raised by S8) | the stacks, from which S6b seats are cut. My expectation: the arm64-only three pass with a longer budget, and `StdoutCloseEofBarrier` is a real hang on both legs |
| 4 | **S1**, the record-less `FuncPCABI0` census | none (a static guard). It pins the class that produced train 25's `sigtramp` death |
| 5 | **S5**, documentation currency | none |
| — | **S2, S3** | S2 is largely done (§2.1); S3 is subsumed by §2.1, since all five loopback programs pass on x64 and no remaining failure is in the struct-seam class |

**In parallel, no seat needed:** the first darwin ROW dispatch (§5), `sweep-shard` with
`filter=unicode/utf8` on both legs. Nothing in §3 blocks it, and its reading starts the row ledger.

---

## 4. The CI mac run leg — what exists, and how a lane drives it

**It already exists.** `os-matrix.yml` takes `workflow_dispatch` with `goos=darwin`, which fans out to
BOTH mac runners in one dispatch, and these stages:

| stage | what it runs on the mac legs | budget (×1.5 on macOS) |
|:--|:--|:--|
| `census` | compiles the whole darwin corpus flavor (also the daily 04:41 UTC schedule) | — |
| `behavioral-smoke` | converter build, then a filtered behavioral run, compared against `go run` | — |
| `behavioral-full` | every behavioral project, in index slices with a purge between | — |
| `behavioral-stderr` | named projects built and launched directly, keeping the WHOLE stdout/stderr and any `.ips` crash report written after the launch | 60 min |
| `sweep-shard` | `run-validated-sweep.ps1 -Filter <substring>`, the `-tests` rows (§5) | 210 min |

So the "one converted program, read its exception" leg COORD asked for is `behavioral-stderr` with
`filter=<project names>`. The hello and os+time pair the linux layer started with are long past;
darwin's first-program reading was FINDING §7's.

**How a lane triggers it without Apple hardware (proven 2026-10-02).** The GitHub MCP tool
`actions_run_trigger` (method `run_workflow`, workflow `os-matrix.yml`, a ref, the four inputs). This
session's token was accepted (HTTP 204 for run 36947612442), so a lane with the same connector can
dispatch. Pin the measured commit by pushing a throwaway branch at it and dispatching on that branch,
then read `head_sha` back from the run. Post the prediction BEFORE the trigger.

**How it reads the result.** `actions_list` (`list_workflow_jobs`) gives each leg's step conclusions,
and `get_job_logs` (`tail_lines`) gives the step's own tail. Every stage also writes its summary as
check-run ANNOTATIONS (`.github/annotate-summary.ps1`). That matters because the job summary and the
artifacts are served from blob storage that some lane hosts cannot reach, while annotations come back
through the API itself.

**What is unbounded, stated plainly.** How many doors stand behind each one is discovered by running,
one door per iteration, as linux's four were. The re-baseline narrows the start; it does not size
the end. The runner pace is the real constraint: a full darwin behavioral reading is about an hour
(train 25: arm64 26 min, x64 54 min), and an x64 leg costs about twice an arm64 leg.

---

## 5. The path to darwin ROW validation (the owner's parity measure)

The owner's goal is measured on ROWS, Go's own test suites per OS (the roster's Windows columns and
`linux:` annotations), not on behavioral programs. A behavioral pass is the precondition; a row is
the measure.

**What a darwin row needs, and what exists.**

- **The leg:** `sweep-shard` with `goos=darwin` already runs `run-validated-sweep.ps1` on both mac
  runners. That means `go test -json` natively on darwin against the converted host, which `publishTestHost`
  publishes self-contained single-file for the host's own RID (`osx-arm64` / `osx-x64`; no RID is
  hard-coded). Not yet done: a darwin `sweep-shard` has never been dispatched.
- **How verdicts come back:** the stage collects every swept row's `go2cs_test_comparison.json` (and
  the results files) into the artifact, and writes the sweep's tail as annotations. A row with no
  darwin annotation reports *comparison-validated-at-count* and exits non-zero BY DESIGN. That is a
  reading, not a regression.
- **How a row banks:** the roster's per-OS annotation, `· darwin: N + D`, the same rule as `linux:`
  (`docs/ValidatedTestPackages.md`, *Reading the table*: the key is a GOOS name, windows refused).
  `check-roster-format` and the sweep already parse the key generically. The darwin Linux-line
  equivalent (a "Darwin: n of m applicable rows" headline) is a roster-format change for its own seat.

**THE OPEN RULING, which this plan cannot settle and COORD/the owner must: one darwin annotation or
two.** The annotation key is a GOOS, and darwin runs on two architectures whose results can differ.
Train 25's `StatLayoutTruth` dies at a different symbol on each, and the committed darwin flavor is
amd64-only, so arm64 compiles amd64 tables. Options:
- (a) bank only when both legs agree on N + D;
- (b) bank arm64 as primary and record x64 differences as disclosures;
- (c) a per-arch key.

My recommendation is (a): it needs no format change, and a disagreement is a finding rather than a
number to choose between.

**The order rows would follow.**

1. **`unicode/utf8` first**, as COORD named it: pure computation and the linux L1 sanity gate.
   `filter=unicode/utf8` selects exactly that row. It proves the leg end to end: convert, publish for
   osx, `go test -json` native, compare, artifact and annotation readable from a lane.
2. **The pure-computation tier**, batched by substring families (`filter=encoding/`, `strings`,
   `strconv`, `container/`, `math`, `sort`, `unicode`, `hash/`, `regexp`, `text/`, …). These are the
   classes the behavioral census passed 100 % on darwin, so a red here is new information about rows
   specifically.
3. **`time`, then `os`, `io/fs`, `path/filepath`.** This is where `StatLayoutTruth` and
   `LongPathRoundTrip` sit, so it waits on their S6 seats.
4. **`os/exec`, `net` and its family, `crypto/tls`.** These wait on the loopback and spawn doors.
5. **`os/signal`.** It waits on S4's design.
6. **`runtime`, `runtime/pprof`, `runtime/debug` last**, as on linux.

**Cost, stated so it can be planned against.** A `sweep-shard` budget is 210 minutes, 315 on the mac
legs, and every darwin dispatch runs BOTH legs. 225 rows in substring batches is tens of dispatches.
Hosted-runner minutes, not lane time, are the constraint, and the batch size is the lever.

---

## 6. Honest limits

- **Nothing in this plan has been run on a Mac by this lane except the §2 dispatch**, read in §2.1.
  Every door named in §3 is now a 1.24.13 reading, but each S6 seat still owes a stack before it is cut.
- **The unbounded part is unbounded.** Linux needed four iterations found only by running; darwin's
  remaining twelve doors may hide more behind them. §2 narrows where the loop starts and nothing
  sizes where it ends.
- **arm64 compiles amd64 tables.** Nine darwin-folder files are amd64-specific and zero arm64. A green
  arm64 run on a path that touches those tables is not proof of arm64 correctness. That layout ruling
  is priced separately and is not part of this plan.
- **Two of the sizing's numbers are not recoverable exactly** (§1). They are carried as old against
  new under one stated predicate, never as the sizing's figures.
- **The x/sys/cpu stub is dormant only while `purego` holds** (§1). Dropping the tag for performance
  would wake it, and its own `cpuid` and `xgetbv` stubs, at package init.
- **Runner availability and pace** are GitHub's. A darwin regression is caught by the daily compile
  census, but a darwin RUN regression is caught only when someone dispatches a run. The plan does not
  add a scheduled run; that is a cost question for COORD.

<!-- Provenance. Counts in §1 are from scratch census scripts run against the git tree at a02ac3df3
     (calibration) and 172d437e66; per-declaration deltas from a set difference of the parsed
     bodyless declaration names. The 18 record-less FuncPCABI0 targets in §3 S1 are the set
     difference between the identifiers taken by `abi.FuncPCABI0(` in */darwin/*.cs (225) and the
     GoCgoImportDynamic record names in */darwin/package_info.cs (212) at 172d437e66. The dormancy of
     x/sys/cpu is read from GOROOT go1.24.13 (chacha20poly1305_amd64.go's `gc && !purego`) and the
     absence of any corpus project reference. The run 36947612442 head SHA was read back from the
     GitHub API. -->
