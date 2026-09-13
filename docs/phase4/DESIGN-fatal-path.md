# DESIGN — the Go-format fatal report, and severing `fatalthrow` from `getcallerpc`

> **Status: SPECIFIED, NOT BUILT.** Ruled by COORD (mailbox `84efb3a`): *"fatal increment = ONE
> shape (sever fatalthrow/fatalpanic/traceback), acceptance by stderr SHAPE on both hosts, guard =
> renderer arm in GolibTests + the probe."* This record is the first of the three deliverables.

> **Scope, and its sibling.** What a converted program prints, and where, when the runtime takes a
> **fatal** — `runtime.throw`, not a panic. Its sibling
> [`DESIGN-crash-report.md`](DESIGN-crash-report.md) owns the **panic** side and has been
> IMPLEMENTED since 2026-08-21. The two paths diverge inside `runtime/panic.cs`: an unrecovered
> panic reaches golib's `CrashReport`, which this runtime already fills with a Go-spelled
> traceback; a `throw` reaches `fatalthrow`, whose **first statement** is a bodyless stub. Nothing
> in the sibling record mentions `fatalthrow`, `fatalpanic`, `getcallerpc` or `fatal error` — I
> grepped it — so the fatal path is genuinely unowned rather than covered elsewhere.

## 1. What Go prints — MEASURED AT THE CORPUS PIN, not described

The probe (`docs/phase4/probes/c1-fatal-path/`) declares one limit in its own Limits section: its
oracle row was taken at **go1.24.13**, because go1.23.12 was not on the probe author's box. **That
limit is now closed.** The oracle was re-taken today at **go1.23.12** — the corpus pin, and the
release the comparison oracle actually runs — with the resolved binary asserted under the pinned
root and `go env GOROOT` asserted equal to it.

```
EXIT = 2
stdout, 1 line   PROBE-MARK-1: reached main
stderr, 66 lines PROBE-MARK-1E: reached main (fd 2)
                 fatal error: runtime.SetFinalizer: first argument is nil
                 <blank>
                 goroutine 1 gp=0xADDR m=0 mp=0xADDR [running]:
                 …
leading frames   runtime.throw → runtime.SetFinalizer → main.main → runtime.main → runtime.goexit
goroutine hdrs   5
"fatal error:"   exactly 1
PROBE-MARK-2     absent from both streams
```

**It reproduces the go1.24.13 reading in every particular** — same exit, same 66 lines, same text
once, same leading frame order. That is a second derivation agreeing rather than a new fact, and it
is worth exactly that: it says the shape this increment must reproduce is not version-specific
across the hop, so the acceptance written here survives the go1.24 migration.

**One datum the earlier row did not record: FIVE goroutine headers, not one.** Go's fatal dumps
**every** goroutine, not just the failing one. That is load-bearing for §4 and it was invisible
until somebody counted.

## 2. What we print today — measured on both hosts, and one prediction of mine was wrong

From the probe's RESULTS block (i7 on windows, R on linux, both arms read):

| | predicted | MEASURED |
|:--|:--|:--|
| windows | text once, then a stub naming `getcallerpc` | ✅ confirmed exactly |
| linux | **nothing of Go's text**, stub naming `write1` | ❌ **FALSIFIED — identical to windows, frame for frame** |
| exit | NOT 2 | ❌ **it IS 2** — but as golib's unhandled-exception backstop |

**Why the linux prediction failed, and why the increment SHRANK because of it.** `runtime`'s
`@throw` (`panic.cs:1090`) calls plain `print(…)`, and `print` resolves to **golib's builtin**
(`builtin.cs:2270`, a `Console.Error.Write`). So does `printindented`. The whole fatal text
therefore goes to .NET's stderr and runtime's own `gwrite → writeErr → write → write1` chain is
**never entered**. `write1` *is* a bodyless partial on linux exactly as the sizing read it; it is
simply **UNREACHED**. **A converted call graph is not Go's call graph.**

So: **`write1` is OFF the remedy and the fatal path is ONE SHAPE on all three flavours** — no
per-flavour arm. That is a strictly smaller increment than the sizing proposed, and it is smaller
because a prediction was wrong.

**The exit code cannot discriminate.** 2 appears on both sides, ours from golib's backstop rather
than from Go's `exit(2)`. Recorded and not leaned on — which is why the ruling keys the acceptance
on the stderr **shape**.

## 3. The three functions to sever, read at the landed master

| site | first statement | consequence |
|:--|:--|:--|
| `runtime/panic.cs:1265` `fatalthrow(throwType)` | `var pc = getcallerpc();` | dies before `startpanic_m` |
| `runtime/panic.cs:1296` `fatalpanic(ж<_panic>)` | `var pc = getcallerpc();` | same, on the panic-with-messages path |
| `runtime/traceback.cs:775` `traceback(pc, sp, lr, gp)` | delegates to `traceback1` | the PC/SP-driven walk itself |

`getcallerpc`/`getcallersp` are bodyless partials declared beside `publicationBarrier()` in
`runtime/stubs.cs`, and they are **not** implementable: they are compiler intrinsics returning a
caller's PC and SP, which the CLR does not expose and which this runtime's synthetic-PC scheme
deliberately does not mint for arbitrary frames. **The remedy is therefore not to implement them.
It is to stop the fatal path needing them**, which is what "sever onto the managed walk" means.

## 4. What the managed walk ALREADY gives us — and what it refuses

This is the part that makes the increment small, and it was read at the code rather than assumed.
`runtime/managed_impl.cs` already owns:

* **`Stack(slice<byte> buf, bool all)`** — the calling goroutine's real frames, the in-flight
  panic's snapshot below them, and under `all` **every other goroutine**: a real header (the id the
  registry minted, the wait reason the park accounting recorded), a `created by …` line from
  `Goroutine.Creator`, and an **honest placeholder** where the frames would be. It filters system
  goroutines exactly as Go's `tracebackothers` does.
* **`appendGoroutineHeader`** — `goroutine N [status]:`.
* **`appendGoFrames`** / `goFrameName` / `goFramePosition` — `<pkg>.<Func>()` with the Go position
  beneath, mapped back through the emitted position maps.
* **`crashTraceback`** — the panic side's renderer, already wired into golib's `CrashReport`.

**So the renderer is not this increment's work.** The increment is a *call*, not a new printer.

⚠ **A PRECISION CORRECTION OWED TO THE SIBLING RECORD, and it is a correction rather than a
contradiction.** `DESIGN-crash-report.md` §8 lists as out of scope: *"`all goroutines` dumps.
`runtime.Stack(buf, true)` cannot honestly answer them under the CLR (no supported cross-thread
stack walk); that refusal is recorded at `runtime.Stack` and stands."* That was true when written.
It is now **half true, and the half matters**: the **headers** are answered truthfully today —
`Goroutine.Snapshot()`, real ids, real wait reasons, `created by` — and only the **frames** are
still refused. *Absent from the dump* and *absent from the frames* are different claims, and the
fatal path needs exactly the half that landed. **Not editing their record from here**: the owning
arc should carry a dated amendment. Stated here so this increment's §5 does not read as
contradicting a live doc.

## 5. The shape decisions, each with its divergence stated

**(a) The extended header is REFUSED, and the plain one is what we print.** Go's fatal prints
`goroutine 1 gp=0xADDR m=0 mp=0xADDR [running]:` — the runtime-pointer form a `throw` raises the
traceback level to get. We hold no `g`, `m` or `mp` addresses that mean anything, and inventing
three plausible hex numbers would be **fabrication in the one artifact an operator reads when
things have already gone wrong**. We print `goroutine N [status]:`, the form the panic side already
prints and the form Go's own consumers match on. **This is a stated divergence, not an oversight**,
and it is the reason the acceptance in §6 is a shape predicate rather than a byte compare.

**(b) All goroutines, per §1's five headers** — `Stack`'s `all: true` branch, headers plus
`created by` plus the honest frame placeholder. Go dumps them; we dump what we can answer and say
so where we cannot.

**(c) `exit(2)` becomes the fatal path's OWN exit.** Today 2 arrives from golib's backstop after an
unhandled stub exception. After the sever it is Go's own exit, taken deliberately after the report
is written. The observable number does not move; **what moves is whether it can be trusted**, and
that is precisely why the probe records it as non-discriminating.

**(d) `fatal` keeps its own reduced form.** Go's `fatal` (as distinct from `throw`) omits runtime
frames and system goroutines unless `GOTRACEBACK=system`; the existing system-goroutine filter in
`Stack` already expresses that axis and should be reused rather than duplicated.

## 6. Acceptance — the stderr SHAPE, as ruled

Byte equality is unavailable by construction (§5a), so the acceptance is a predicate over the
converted side's stderr, measured against the pin's shape in §1:

1. exactly **one** `fatal error: <text>` line, and the text is Go's;
2. **at least one** goroutine header matching `goroutine <N> [<status>]:`;
3. the leading frames, in order: `runtime.throw` → the throwing function → `main.main`;
4. **zero** .NET exception dump lines (`   at <Namespace>.<Type>.<Method>`) — this is the predicate
   that currently FAILS, and the one that carries the increment;
5. exit **2**;
6. `PROBE-MARK-2` absent — the fatal must not be recoverable.

Predicate 4 is the discriminator. Today's stderr carries Go's text *and then* a .NET stack trace
naming `getcallerpc`; a pass is that second block being replaced by a Go-shaped traceback.

## 7. Gates

`golib`/`runtime` change class, so per this repo's standing rules:

| Gate | Why |
|:--|:--|
| **GolibTests renderer arm**, proven **failing-first** | the ruled guard: the shape predicates of §6, asserted against a rendered fatal |
| **The probe**, both hosts | the ruled second half; it is the only instrument that exercises a *real* fatal end to end |
| **Full behavioral suite**, Output phase included | route #7's behavioral twin — a golib/runtime change that emits byte-identical `.cs` is invisible to CNR and to a compile-only gate |
| `go2cs.slnx --no-incremental`, 0 errors | the only gate compiling the non-generated solution members after a runtime API change |
| CNR | transpile-only; expected byte-identical, since this is a hand-own and not a converter change |

## 8. Deliberately not in scope

* Implementing `getcallerpc`/`getcallersp`. They are compiler intrinsics; §3 says why the remedy is
  to stop needing them.
* Frame argument words and PC offsets — the banked frame rendering omits both and `TestStack`
  agrees with Go without them (the sibling record's ruling, inherited).
* The `gp=/m=/mp=` header fields — §5a.
* Cross-thread frame walking for foreign goroutines. Still refused; §4's correction is about
  headers, not frames.
* `GOTRACEBACK` parsing beyond the system-goroutine axis already present.

## 9. Limits of this record

**Uncompiled and unrun by its author.** This box has no C# toolchain of any kind, so every claim
above about the corpus is a **read at the code with file and line**, and every claim about Go is a
**measurement at the corpus pin**. The converted-side rows in §2 are the runners' measurements, not
mine. The body and the GolibTests arm follow as separate deliverables, in the ruled order.

---

## 10. AMENDMENT — 2026-09-08, written at the body, three corrections and one ruling absorbed

Appended rather than rewritten: §§1–9 stand as they were when the record was announced at
`b0c6bff33`, and everything below is what building the increment measured.

### 10.1 §3's remedy SHRANK again, and the census is why

§3 tabled three functions to sever — `fatalthrow`, `fatalpanic`, `traceback`. A census of the
corpus says the cut is one frame HIGHER and covers two functions instead:

| callee | callers in the whole corpus |
|:--|:--|
| `fatalthrow` | exactly **two** — `throw` (`panic.cs:1098`) and `fatal` (`panic.cs:1118`) |
| `fatalpanic` | exactly **one** — `gopanic` (`panic.cs:851`), itself already dead at its own `getcallerpc()`, and a path no panic in this runtime takes (a panic is a golib `PanicException` reported by `CrashReport`) |

So displacing `throw` and `fatal` leaves `fatalthrow`, `fatalpanic`, `getcallerpc` and
`getcallersp` all **UNREACHED rather than unimplemented** — `write1`'s shape from §2, arrived at a
second time. §3 said the remedy was "to stop the fatal path needing them"; that is what this is,
and it is two registry entries rather than three severs.

### 10.2 §4 was HALF right: the renderer exists, the ANCHOR did not

§4 said "the renderer is not this increment's work — the increment is a *call*, not a new printer."
The helpers (`appendGoroutineHeader`, `appendGoFrames`, `appendCreatedBy`) are indeed reusable. What
§4 missed is that `callerFrames()` located its boundary by identity against **`Stack`'s own method
handle**, hard-coded, so a SECOND entry point into the walk could not reuse it: it would have fallen
to the count-based fallback, which is the exact failure the identity boundary replaced in 2026-09-04.
The anchor is a parameter now, each entry point passing its own handle, and `Stack`'s body is split
so the fatal path and the panic path render through **one** `renderStack` rather than two copies.

### 10.3 COORD's ruling absorbed: the primitive lives in golib, and it has three consumers

Mailbox `4e9b115` (correcting `e9447240b` after R's count correction in `33a99c6`): *"the
internal/sync @throw/fatal pair is C1 fatal-path class with a second site at 1.24; both sites become
one-line forwards to the managed fatal primitive C1 increment defines."*

That decides the primitive's HOME, and not by preference. The shims are declared in three packages —
`runtime`, `sync`, and at Go 1.24 the new `internal/sync` — and golib is the only assembly below all
three. A primitive in `runtime` could not serve the other two (neither references it), and
`internal/sync` referencing `sync` is the project-reference **cycle** `check-solution-integrity`'s
per-GOOS assertion exists to catch. So: `go.golib.FatalReport`, shaped like `CrashReport` and
registered from `runtime`'s own module initializer, exactly as the crash traceback and the
divide-by-zero panic value already invert that dependency.

`sync`'s two forwards land with this increment. `internal/sync`'s are R's seat B and land when the
1.24 package exists in the corpus.

### 10.4 The measured footprint, three targets

Two-seeded, both arms built from the same toolchain (`go version <binary>` = go1.24.13 on each),
all six roots seeded before any arm converted, each arm asserting it WROTE (1,660 windows /
1,733 linux / 1,733 darwin `.cs`) and that a hand-own no converter may write was byte-unchanged:

| path | delta | note |
|:--|:--|:--|
| `runtime/panic.cs` | −53 / +2 | two bodies out, two placeholders in; the diff is **byte-identical on all three targets** |
| `runtime/{windows,linux,darwin}/package_info.cs` | −1 / +1 each | one re-encoded `GoPositionMap` line for `panic.go`, whose range list loses exactly the two displaced functions' ranges |

Nothing else, on any target. Zero `GoImplement`/`GoInit` lines in the delta; no hoisted literal
moved (both bodies spelled their strings inline, so there was none to carry or relocate).

The map line is applied SURGICALLY rather than by three-way merge, and the reason is recorded
because it looks like the hazard it is not: `git merge-file` **conflicted** on all three
`package_info.cs`, and the conflict is the adjacent-insert class rather than a stale target — the
committed `panic.go` map line is byte-identical to the base emission on every target (asserted),
while other map lines in the same region carry the corpus's standing position-map drift. The
residual against the emission is the IDENTICAL SET before and after the application on every file
(0 / 64 / 72 / 60 lines).

### 10.5 §5a is UNCHANGED and still owed a ruling

The plain `goroutine N [status]:` header is what this increment prints; the `gp=/m=/mp=` fields Go's
throw-level traceback carries are still refused, for §5a's reason. Nothing measured while building
the body changed that, and the body is shaped so a ruling the other way costs one renderer, not a
re-cut.

### 10.6 The divergence a sweep must measure, named before it is measured

Today a `runtime.throw` inside the Phase-4 test host raises a stub exception that `TestHost.Run`'s
outer catch can see. After this increment the primitive calls `Environment.Exit(2)` and the host
dies where Go's test binary dies. That is strictly more faithful — and it is exactly the shape that
can trade a MEASURABLE row for an UNMEASURABLE one, so it is named here rather than discovered in a
sweep. The prediction: no banked row moves, because the oracle side dies on a fatal too, and the
common case is already a dead process (the stub exception reaches golib's backstop, which exits 2);
what changes is stderr. The gate that can falsify it is the roster sweep, owed and not run here.

### 10.7 §5a is RULED — 2026-09-08, COORD `133e138`

*"section 5a RULED plain goroutine header (synthetic pointer fields would be fabricated)."*

So §10.5's closing sentence — *"§5a is UNCHANGED and still owed a ruling"* — was true when the body
was cut and is now spent. It is left standing above rather than rewritten, because a record is
amended in dated blocks: what the increment shipped is what was ruled, and the plain
`goroutine N [status]:` header is the form of record. The `gp=/m=/mp=` fields are refused
permanently, on the ground the record argued and the ruling adopted: we hold no `g`, `m` or `mp`
addresses that mean anything, and three plausible hex numbers in the one artifact an operator reads
when things have already gone wrong would be fabrication.

**Consequence for §6:** the acceptance stays a SHAPE predicate rather than a byte compare, and it is
now that by ruling rather than by the author's choice. Nothing in the body or the guard moves.
