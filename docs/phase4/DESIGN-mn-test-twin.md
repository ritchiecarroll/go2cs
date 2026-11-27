# DESIGN — the test host enters a test's twin

**Lane R, 2026-11-27.** COORD 20261127T051825Z (G's row 16f precondition; the owner, 11-27: test a few real
concurrency-heavy modules before .5). Read at master `7d8af59808` (train 16). Nothing here is built or run yet.

## 1. The finding

Under `-tests`, a converted test binary runs every test through the hand-owned `testing` host. The host calls the
test function PLAIN:

- `TestExecution.Start` (src/core/testing/TestExecution.cs) starts a dedicated thread per test, enters a goroutine
  scope (`Goroutine.Enter(s_tRunCreator, creator)`), and calls `Execute(action)`.
- `Execute` calls `action(new StandardBox<testing_package.T>(t))`, the test's plain body.

A plain body writes `goǃ` for a `go` statement, which is the thread path, and parks on threads. So with twins opted
in and `GO2CS_MN=1`, a module's `go test` still runs every test goroutine on a thread. An M:N reading of a module's
tests measures nothing. A program's `main` does not have this gap: its generated entry calls `builtin.RunMainꞏtwin`,
which runs main's twin multiplexed as goroutine 1 (`Goroutine.RunMainTwin`).

## 2. The cut

**(a) golib: one public entry, `Goroutine.RunTwin(Action body, Func<GoTask> twin)`.** It is `RunMainTwin`'s body
generalized from goroutine 1 to the calling goroutine.
- When the scheduler is off, or the calling thread holds no goroutine, or that goroutine is locked to its thread, it
  calls `body()` and nothing else. That is exactly today's path.
- Otherwise the calling goroutine leaves its thread and runs `twin` multiplexed. The thread blocks until the twin
  ends, takes the goroutine back, and rethrows whatever left the twin, from the same place (`ExceptionDispatchInfo`).
- `RunMainTwin` keeps its `MainThread.Run` fallback and calls the shared core, so main's behaviour does not change.
- Called by hand-owned `testing`, not by generated code, so it joins the ordinary public surface
  (PublicSurface.golib.txt), not `[mn-frozen]`. That needs the decision this note asks for.

**(b) testing: look up the test's twin only when the scheduler is on.** In `TestExecution.Start`'s thread, if
multiplexing is enabled (a public read-only flag beside the entry) and the test's method has a sibling
`<Name>ꞏtwin` in its declaring type, taking the same parameters and returning `GoTask`, the host calls
`Goroutine.RunTwin(() => action(t), () => twin(t))` in place of `action(t)`.
- With the scheduler off, no reflection runs and no line of the host's path changes. That is G's T0 dormancy bar:
  default mode and `GO2CS_MN` unset stay byte-for-byte plain.

**(c) testing: the owner check follows the goroutine when a twin ran.** `TryEnsureOwner` (FailNow, SkipNow) and
`InOwnersBubble` compare `Environment.CurrentManagedThreadId` with `m_ownerThread`. A twin moves between carrier
threads, so a `t.Fatal` in a twinned test would be refused as "called from a goroutine other than the test
goroutine". On the twin path only, the check compares `Goroutine.Current` with `m_ownerGoroutine`, which is the same
object on every carrier. The plain path keeps the thread check unchanged.

**What leaves the twin.** `TestAbortException` (FailNow), `GoexitException` and `PanicException` are captured in the
twin and rethrown on the test thread, inside `Execute`'s `try`. So `Execute`'s handlers see exactly what they see
today. Defers run inside the twin, where the body's own defers are.

## 3. Scope, and what is left out

- **In:** top-level test functions (`TestX(t *testing.T)`) whose twin exists.
- **Out, named:** a subtest literal passed to `t.Run`, an example, a benchmark, and `TestMain`. A subtest literal
  arrives as a plain `Action<ж<T>>`. Whether the generator gives it a twin pair (`GoFuncPair`) reachable from the
  host is the first question of a follow-up, not this cut.
- `t.Parallel()` inside a twin parks on the host's gate, a managed wait on a carrier. N5's step 3a senses a managed
  wait and retakes the P, so it does not starve the scheduler; it is stated, not engineered here.

## 4. Red-first and the gates

1. **The red row:** a twinned test whose body parks (on a channel, and in a select) reads parks > 0 under
   `GO2CS_MN=1` after the cut, and 0 before. It is a converted test package built with twins, run by the `-tests`
   host. The open question for G: whether that lives as a behavioral-style project with a `_test.go` (the host
   built twinned, read through the scheduler's reading), or in GolibTests through a separate process, since
   `GoScheduler.Enabled` is read once per process.
2. **The near-miss:** with `GO2CS_MN` unset, the same package reads parks 0 and the host takes no twin path
   (byte-for-byte: the default-mode subset and a CNR).
3. **A FailNow row in a twinned test:** it ends the test as failed, not as an infrastructure error. That is the (c) guard.
4. **Gates:** GolibTests; the behavioral `--mn` subset and the default subset; hosted mn-platform; repoguard (the
   public-surface line).

## 5. Questions for G and COORD

- **The entry's shape and home:** `Goroutine.RunTwin` plus a public `Goroutine.MultiplexingEnabled`, or a
  `builtin.RunTestꞏtwin` beside `RunMainꞏtwin`.
- **The red row's home** (4.1).
- **Order:** the train 18 aim stands if this note is ruled on today.

## 6. Rulings, and the cut as built (amended 2026-11-27)

G ruled the same day: "The cut is right; cut it now, for train 18." COORD set the order (the twin cut first, then
16c's legs, then F2's V8 row). What the rulings changed against sections 2 to 5:

- **The entry's home.** `Goroutine.RunTwin`, hidden (`EditorBrowsable Never`), in PublicSurface's `[mn-frozen]` set
  (not the ordinary surface that 2(a) proposed): every converted test host compiles `testing`, which calls it. No
  public `MultiplexingEnabled` flag: `testing` reads the existing `GoScheduler.Enabled`.
- **The lookup is by stamp, not by name alone.** `TwinOf` takes `<Name>ꞏtwin` in the test method's declaring type
  with the same parameters, returning `GoTask`, and requires its `[GoTwinOf]` to name that type and that method. It
  is cached per `MethodInfo`, and runs only when `GoScheduler.Enabled`. A closure (`action.Target` not null) has none.
- **A fourth fallback.** `RunTwin` is also `body()` when the goroutine is already multiplexed: there is no thread to
  leave.
- **The startup rule.** A twinned `Exe` starts through `go.GoTwinEntry`, which go2cs-gen emits only for a Go
  `main`. A converted test host has none, so a twinned host failed to build (CS1555). `go.lib.twins.targets` sets
  `GoTwinsTestHost` for a project named `*.tests` that has `go2cs_test_host.cs`, and that project keeps its own
  `Main`. P6 met the same CS1555 in four module arms (x/sync, conc, pubsub, go-cache; COORD 062609Z).
- **Scope, narrowed.** Top-level tests only (`m_parent` null), and not a synctest child. Native AOT is not this
  cut's target: the lookup is reflection, and an AOT-published twinned host was not built or run here.

**As built** (branch `claude/r-mn-test-twin`):

- golib: `RunTwin` and `RunMainTwin` share one core, `RunOnScheduler`. `RunTwin` carries the goroutine's logical
  state (panic and defer state, `GoThreadState`) with it, as `Unmount` does. Main still starts from the defaults.
- testing: `Execute` takes the twin path under the conditions above, and sets `m_twinPath`. On that path,
  `TryEnsureOwner` and `InOwnersBubble` compare `Goroutine.Current` with `m_ownerGoroutine`; otherwise they keep the
  thread check.
- The red row lives in `src/tests/PackageTests/TwinTestHost` (`test-twin-test-host.ps1`, six arms at the cut, fourteen
  after the review folds in 6.1). The fallbacks
  are GolibTests rows in a child process (`GoroutineRunTwinTests`).

**Measured on R-LAPTOP:**

- The red row before the cut: arms A and F failed (no READING line: the scheduler never started). B, C, D and E
  held.
- After the cut, every arm holds:
  - A, TestParks: parks=101.
  - B, `GO2CS_MN` unset: the scheduler never starts.
  - C, a test with no twin: runs plain.
  - D, `t.Fatal` inside the twin: a test FAIL, not an infrastructure error.
  - E, `t.Fatal` from a started goroutine: still refused.
  - F, `t.Parallel` and then a park: parks=22.
- `sensed=` reads n/a until N5's step 3a merges. At the merged tree it read 0 on every arm. That is the design, not a
  short gate: see 6.1.
- Each gate was made to fail once, then restored byte-identical:

  | Gate | Plant | Red |
  |---|---|---|
  | Build | Pre-cut targets | CS1555 "Could not find 'go.GoTwinEntry'" |
  | PublicSurface | Drop the RunTwin line | A violation that names it |
  | RunTwin rows | Drop the locked-thread fallback | Exactly the `locked` row |

### 6.1 Review folds (amended 2026-11-27, after `5015b12085`)

The account A reviewer read `5015b12085` as clear with nits (via COORD). G approved it for train 18 and corrected one
reading. The folds:

- **Q54 on the twin path: measured, and it holds as cut.** The reviewer read the held-lock list as the carrier's
  thread state, so a twin panicking between `lock2` and `unlock2` would leave the lock held while `Execute` abandoned
  on the test's thread.
  - Under M:N the list is the goroutine's (`runtime/lock_managed_impl.cs`, `RegisterMounted`, row 11). It moves with
    the goroutine, and `RunTwin`'s return mounts it back on the test's thread, where `Execute`'s existing catch
    abandons it.
  - I wrote a carrier-side filter first. With it removed, the new row stayed green, so I dropped it.
  - The new rows, in `GoroutineRunTwinTests`:
    - `host-twin-abandons`: a twinned test, run through the real `TestRunner`, takes runtime lock probe 0 on a carrier
      and panics. The next locker must die by name.
    - `host-plain-abandons`: the plain control.
  - A comment at `Execute`'s twin call says why the abandon holds there.
- **Three more arms.** They are named H, I and J because G took the letter G.
  - H: a panic in the twin is a test FAIL with "panic: twin panic", not an infrastructure error.
  - I: `runtime.Goexit` in the twin is a FAIL with "test executed panic(nil) or runtime.Goexit".
  - J: `t.Skip` in the twin is a SKIP.
  - Each also runs plain (H0, I0, J0). The plain verdict is the one its twin arm must give.
- **`m_twinPath`** is reworded: it says the twin was OFFERED to `RunTwin`, not that it ran. On `RunTwin`'s fallback
  the body runs on the test's thread as the owner goroutine, where the goroutine compare answers as the thread compare
  does.
- **`sensed=0` is the design, not a short gate.** This is G's correction and G's measurement, on `5015b12085`.
  - N5 3a senses only when work waits for a P (a run queue that is not empty, or a P waiter). At 12 procs nothing
    waits, so nothing is sensed however long the gate is held.
  - G's control: the fixture plus `TestSerialParks` (`Sum(200)`), run after `TestParallelParks` with
    `-test.timeout=60s`, one run each:

    | Run | Setting | Result |
    |---|---|---|
    | G1 | `GOMAXPROCS=1` | both pass, sensed=1, injections=0, 775 ms |
    | G2 | `GOMAXPROCS=1`, `GO2CS_MN_SENSE=0` | both pass, sensed=0, injections=203, 3772 ms (3c rescues it, about 5x slower) |
    | G3 | 12 procs | both pass, sensed=0, injections=0 |

  - G's trap: tests run in file-name order. A serial test that sorts first finds the gate not yet held.
  - Added as arm G (G1) and G0 (G2), with the serial test in `zserial_test.go`.
  - R-LAPTOP reads G: parks=125, sensed=1. G0: parks=125, sensed=0.
  - P6's arm M runs at `GOMAXPROCS=1`, so `sensed > 0` is expected on modules that use `t.Parallel` heavily.
- **Arm D asserted nothing about the twin.** It held on the plain path before the cut, and it held again under the
  "no twin path" plant below. It now requires parks > 0, as H-J do. In the green run it reads parks=3.
- **A trap in the fixture.** The script does not remove the C# emitted for a Go file that no longer exists. A renamed
  test file leaves its old `.cs` building beside the new one; clean the directory with `git clean -fdX`.

All 14 arms hold on R-LAPTOP. Each new gate was made to fail, then restored byte-identical:

| Gate | Plant | Red |
|---|---|---|
| `host-twin-abandons` | the mounted held-lock swap made a no-op | exactly that row: "the next locker is still polling" (the plain control stays green) |
| Arm G | the serial test in `aserial_test.go`, which sorts first | exactly G: sensed=0 |
| The twin conjunct of A, D, F, G, G0, H, I, J | `Execute` never takes the twin path | A, F, G, G0, H, I, J. D passed, which is how its gap was found; B, C, E, H0, I0 and J0 hold |
