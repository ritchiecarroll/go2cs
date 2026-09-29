# CENSUS — runtime's first semantic bill

**2026-09-02, i9.** First run of `src/tools/comparison-classifier` against a real corpus package
beyond its own fixtures and the `unicode/utf8` sanity check. Subject: `runtime`, on
`claude/i9-runtime-regen` @ `4df231e5a` (pre-merge; the branch COORD accepted and staged as train 8 —
this bill is stated as pre-merge per COORD's own instruction, since the ordering point — a runtime
whose init hooks are where the converter puts them — is what the regen exists to provide, and it
was available on this branch tip before train 8 lands).

This is the FIRST time `-test-action compare` has completed for `runtime` far enough to produce a
populated `go2cs_test_comparison.json` — every earlier attempt this session (the STOP-class
recheck, the commit-3 measurement, the regen's own control builds) stopped at `build`. The
init-hook and cast-drift regen is what got the host far enough to actually RUN tests instead of
dying at process init, and that is the reason this bill has 849 rows to classify instead of one.

**Run environment:** `CGO_ENABLED=0`, matching the corpus's own pinned convention (CLAUDE.md) — the
setting that makes the 8 cgo-callback rows in `unclassified` (below) a separate, already-understood
divergence class (cgo-conditional tests reached despite the pin) rather than 8 new findings.

## Bucket one: `TestCaller` / `textAddr` — a real nil-pointer panic, not an infrastructure gap

Per dispatch, this leads ahead of the other findings.

```
panic: runtime error: invalid memory address or nil pointer dereference

runtime.(*moduledata).textAddr()
	runtime/symtab.go:663
runtime.(*moduledata).textAddr()
	.../go2cs-gen/go2cs.RecvGenerator/go.runtime_package.textAddr.global__go.runtime_package.moduledata.g.cs:27
runtime.ΔfuncInfo.entry()
	runtime/symtab.go:851
runtime.(*Func).Entry()
	runtime/symtab.go:778
runtime_test.testCallerBar()   runtime/symtab_test.go:43
runtime_test.testCallerFoo()   runtime/symtab_test.go:35
runtime_test.TestCaller.func1() runtime/symtab_test.go:20
```

`Go="pass" C#="infrastructure-error"`, classified `go-panic-text` (Output contains `panic:`).
**Spot-checked against `go2cs_test_results.json` directly**: a `run` event, one `fail` event
carrying the real panic text and stack above, then three duplicate `infrastructure-error` events
from the same crash (the goroutine-panic reporter re-firing — see the file-lock note below). The
classifier's `classifyOneMismatch` takes the first `fail`/`panic` action it finds, which is the
real one; the duplicates don't change the bucket.

**The mechanism, read from the trace, not guessed:** `moduledata.textAddr()` dereferences a field
that is null at the point `Func.Entry()` calls it through the generated `RecvGenerator` shell
(`go.runtime_package.textAddr.global__go.runtime_package.moduledata.g.cs:27`). `TestCaller` builds
a `runtime.Frames` iterator via `runtime.Callers` inside a goroutine (`testCallerFoo` →
`testCallerBar` → the closure at `symtab_test.go:20`) and walks it with `Func.Entry()`.

**UPDATE, post-sizing (per dispatch, item 2):** the root is not goroutine-timing and not a
"managed-referent" descriptor gap as first guessed here — it is structural and unconditional.
`ΔfuncInfo.entry()` (`symtab.cs:722-724`) reads `f.datap.textAddr(...)`, and `f.datap` is nil because
`funcInfo(this ж<_func> Ꮡf)` (`symtab.cs:259-278`) resolves it by walking a linked list from
`Ꮡfirstmoduledata` looking for a module whose `pclntable` byte range contains the `_func` value's own
address — and `Ꮡfirstmoduledata` is declared exactly once in the whole package
(`symtab.cs:367`, `new StandardBox<moduledata>(new moduledata(nil))`) and never assigned again. Its
`pclntable` is permanently empty, the search's own guard skips it every time, and `mod` stays nil on
every call that reaches this path — not intermittently, not specific to goroutines, structurally
guaranteed. TestCaller is simply the test that happens to call `.Entry()` on a resolved `Func`. Full
trace and the predicted moved set are in the mailbox sizing post (i9, "item 2 SIZED") rather than
duplicated here in full; this update exists so the doc doesn't stand corrected only in a channel
that scrolls away.

**A second, separately-panicking test — NOT the same mechanism, corrected after sizing bucket one
properly:** `TestArenaCollision` (`go-panic-text`, `KeepNArenaHints` → nil pointer, `export_test.cs:620`).
This entry originally called it "the same mechanism class" as `TestCaller`; that was wrong and is
corrected here rather than left standing. `KeepNArenaHints` is a direct translation with no
generator shell involved at all — `hint = hint.Value.next` on a `ж<arenaHint>` chain seeded from
`mheap_.arenaHints`, faithfully mirroring Go's own `hint = hint.next; if hint == nil { return }`. If
it panics, `mheap_.arenaHints`'s chain has fewer real entries than Go's OS-facing allocator builds —
a DATA gap in the managed arena/heap subsystem, not a code-generation issue. It is also a different
severity: it ran on the main test thread inside `TestExecution.Execute`'s own try/catch (`FAIL ...
exit status 1`, caught and reported as an ordinary test failure) rather than on an goroutine outside
that scope, so it did NOT take the host down the way `TestCaller` did. Two unrelated defects, not
two instances of one — see the mailbox sizing post for the full trace of both.

## The whole-host crash's shadow: 833 `empty-unreached`, 1 `empty-in-progress-killed`

`TestCaller` runs in the middle of the alphabetically-sorted test order and its panic — thrown on
a **goroutine**, not the main test thread — brought the whole host process down (per the run log:
`go2cs test host: could not record the goroutine panic: ... file is being used by another process`,
repeated as multiple report attempts raced each other, then the process exited). Everything
scheduled after that point never got a turn:

- **833 `empty-unreached`** — no C# event at all (`TestVersion` spot-checked: 0 events in
  `go2cs_test_results.json`).
- **1 `empty-in-progress-killed`** — `TestBigGOMAXPROCS`, which HAD started (`run` event,
  spot-checked) but never reached a terminal before the host died with it.

This is the doc comment's documented phantom-finding risk realized exactly as described: these are
NOT 834 independent divergences, they are ONE root cause (`TestCaller`'s panic) with 834 shadows.
The tool does not currently collapse this automatically (the parallel-set-equality collapse is
explicitly unimplemented — see the tool's own doc comment) — flagging it here by hand instead.

## Everything else

```
assertion-mismatch      4
empty-in-progress-killed 1
empty-unreached        833
go-panic-text            2
unclassified             9
-------------------------
total non-matching rows 849
```

**`assertion-mismatch` (4)**, all real Go=pass/C#=fail divergences with captured Output, spot-checked
one (`TestArrayHash`, allocation-count mismatch — go2cs's own runtime allocation counter vs Go's
`MemStats.Mallocs`, a structural-mirror comparison rather than a semantic bug; the other three —
`TestBigItems`, `TestCPUMetricsSleep`, `TestCPUStats` — read the same way from their first-line
text (missing map key, zero CPU/idle time) but were not individually re-verified against raw JSON
this pass.

**`unclassified` (9)**: 8 are `Go="pass" C#="infrastructure-error"` with no captured Output
(`TestAddrRangesAdd`, `TestBlockingCallback`, `TestCallback`, `TestCallbackGC`,
`TestCallbackInAnotherThread`, `TestCallbackPanic`, `TestCallbackPanicLocked`,
`TestCallbackPanicLoop`) — every one of these is a cgo-callback test; runtime's `-tests` conversion
runs `CGO_ENABLED=0` per the corpus's own pinned convention (see CLAUDE.md), so these are almost
certainly a SEPARATE, already-understood divergence class (cgo-conditional tests reached despite
the pin) rather than 8 new findings — not re-classified as such here because the tool has no cgo
awareness yet, stated as a real gap rather than silently folded into `unclassified`'s catch-all.

**A classifier gap found by this run, not by inspection:** the 9th `unclassified` entry is the
`hostCrashAtInit` signature itself (`converted tests: ... failed: exit status N`) — but it landed
in `unclassified`, not the `PACKAGE-LEVEL: host-crash-at-init` short-circuit, because that
short-circuit only fires when `results.json` fails to load AT ALL (`resultsErr != nil`). Here
`results.json` loaded fine — 849 real per-test rows came from it — but the OVERALL host process
*also* exited non-zero (from `TestCaller`'s goroutine panic taking down the process), and the
`-tests` pipeline appended that whole-process failure line to `comparison.errors[]` alongside the
849 per-test lines rather than instead of them. `hostCrashAtInit` is checked in the wrong place for
this shape: it needs a check inside the per-line classification loop too (or an early separate
scan of `errors[]` for its signature before the `errorLinePattern` match), not just the
`resultsErr != nil` branch. Filed as a finding, not fixed here — outside this bill's scope.

## Freshness

The tool printed its staleness warning (`results.json is OLDER than comparison.json`) — both
timestamps read identically to the second (`2026-09-02T00:59:22-05:00`), so this reads as same-second
write ordering rather than a genuinely stale record from an earlier run; the 849-row count and the
specific test names (real runtime test names, sensibly distributed) are consistent with a single
fresh run, not a leftover. Noted rather than silently trusted, per the tool's own doctrine.

## Gates

Report-only — no code changed to produce this bill. The classifier itself (`bc092c9f3`) already
carries its own gates (build/vet/test). The `-tests` run that produced the input data used
`claude/i9-runtime-regen`'s own tip directly (no scratch-seed overlay), avoiding the seed-gap class
of false reading from the earlier 1,925-error measurement.

## Amendment 2026-09-28 -- Frame.Func reopened (census A2, D3)

runtime/managed_impl.cs cited this record for leaving `firstmoduledata` and `Frame.Func` unchanged when
FuncForPC's *Func gained Name/Entry/FileLine. COORD's ruling on census A2 (2026-09-28) REOPENED the Frame.Func
half: Go leaves `Frame.Func` nil only for an INLINED frame, go2cs keeps Go's function boundaries one-for-one so no
frame is inlined, and runtime's TestStartLine reads `frame.Func == nil` as "inlined". Frames.Next now sets
Frame.Func for every Go frame, one *Func interned per Go function name, and sets Frame.startLine (the function's
`func` keyword line, from the frame's GoPositionMap record, or a go1.24.13 constant for a root frame). The
`firstmoduledata` half of the original decision stands unchanged: there is still no pclntab, and a *Func is
still a record keyed by its box, not a window onto a symbol table. Seat: claude/i9-frame-func.
