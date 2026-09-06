# DESIGN — the `runtime/pprof` linkname push: eight destinations, two registries, zero new project edges

**Status:** design only. Nothing cut. The graph invariant was measured FIRST, on the coordinator's
order, because if the naive shape were illegal this would be a different design.

**Origin.** The board carried `net/http/pprof` from 2026-08-14 as *"Profile collection has no managed
body — sibling of `runtime/pprof`'s and `runtime/trace`'s stubs."* A coordinator recon contradicted
that in 2026-09-06 and asked C1 to verify it before acting. Verified: **the bodies exist.** This
record is what the verification implies.

---

## 1. The measurement, read from files rather than inferred

`runtime/pprof/pprof.cs` declares eight bodyless `internal static partial` methods, each under a
`//go:linkname`. `runtime` carries a real converted body for every one, every one `internal` — so
`PartialStubGenerator` mints a throwing stub on the consumer side and every profile collection call
throws. That is the whole blocker.

| destination | consumer decl (`runtime/pprof/pprof.cs`) | body in `runtime` |
|---|---|---|
| `pprof_goroutineProfileWithLabels` | bodyless partial, 1103 | `mprof.cs:1331` |
| `pprof_cyclesPerSecond` | bodyless partial, 1106 | `cpuprof.cs:204` |
| `pprof_memProfileInternal` | bodyless partial, 1109 | `mprof.cs:1095` |
| `pprof_blockProfileInternal` | bodyless partial, 1112 | `mprof.cs:1222` |
| `pprof_mutexProfileInternal` | bodyless partial, 1115 | `mprof.cs:1280` |
| `pprof_threadCreateInternal` | bodyless partial, 1118 | `mprof.cs:1323` |
| `pprof_fpunwindExpand` | bodyless partial, 1121 | `tracestack.cs:266` |
| `pprof_makeProfStack` | bodyless partial, 1124 | `{linux,windows,darwin}/proc.cs:1001` |

They are ordinary converted Go, not stubs — `pprof_memProfileInternal` delegates to
`memProfileInternal` with a record-copying closure exactly as Go's does.

A **ninth**, `blockevent`, is declared in `pprof_test.go:1229` and its body exists at `mprof.cs:503`.
It is TEST-side and lands with the row's test emission, so **the production push is eight.**

## 2. The graph invariant — measured BEFORE the design, and it rules out the naive shape

`check-solution-integrity.ps1`, all three `$(GoTargetOS)` flavors, with the documented positive
control (`runtime=internal/syscall/windows`) red as recorded:

| injected edge | windows | linux | darwin |
|---|---|---|---|
| *(baseline, no injection)* | 0 cycles / 307 projects | 0 / 307 | 0 / 307 |
| `runtime → runtime/pprof` — Go's PUSH direction taken literally | **38 cycles** | **36** | **36** |
| `runtime/pprof → runtime` — the pull direction | 0 | 0 | 0 |

`runtime/pprof` imports `compress/gzip`, `context` and more, each of which reaches `errors →
internal/reflectlite → runtime`, so an edge from `runtime` closes dozens of loops at once. **A
literal reading of Go's push direction is illegal by 36–38 cycles.** The pull direction costs
nothing because `runtime/pprof → runtime` is already in the graph.

This is W1's finding one package over, and W1 already ruled the remedy: *invert the alias* — storage
and body stay where Go puts them, the CONSUMER forwards. That option was costed at **0 new edges**
there for the same reason it is free here.

## 3. Go's own directive ARITY splits the eight, one-to-one onto the converter's two registries

The discriminator is not a judgement — it is which form Go wrote:

- **Seven carry a ONE-ARG handle** in `runtime` (`//go:linkname pprof_memProfileInternal`), which is
  Go's authorization to be PULLED. Consumer side: `runtime/pprof` names them with the two-arg form.
- **`pprof_cyclesPerSecond` carries the TWO-ARG form** in `runtime`
  (`//go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond`) — the PUSH: runtime
  DEFINES a symbol in `runtime/pprof`'s namespace, and `runtime_cyclesPerSecond` exists nowhere in
  the converted `runtime/pprof` package at all.

**Both machineries already exist in the converter, and both are curated lists** rather than rules:

- `linknameForwardTargets` — the PULL registry. `packageFuncAccess` emits a listed target `public`
  in its own package, and the consumer's bodyless declaration becomes a forwarder. Gated on Go's
  one-arg handle, deliberately narrower than the var rule (340 handles exist; publicizing all of
  them would widen the corpus surface for pulls that never become a call).
- `linknamePushTargets` — the PUSH registry, keyed by the CONSUMER's symbol and valued with the
  defining `source`; `linknamePushSources` derives from it. It carries exactly ONE entry today
  (`unique.runtime_registerUniqueMapCleanup`).

**The push registry already inverts the direction**: the consumer's declaration forwards to the
pusher's now-public body. So `pprof_cyclesPerSecond` rides the same `runtime/pprof → runtime` edge as
the seven, and the illegal edge in §2 is a shape the converter would never emit. The measurement
still had to be taken — a design that assumed it would have been assuming the answer.

## 4. The cut this implies

Eight curated entries, no new machinery: seven `linknameForwardTargets` keys `runtime.<name>`, and
one `linknamePushTargets` entry keyed `runtime/pprof.runtime_cyclesPerSecond` with source
`runtime.pprof_cyclesPerSecond`. The corpus footprint is the accessibility flip on eight `runtime`
methods plus eight consumer forwarders.

`pprof_makeProfStack` is per-GOOS (`{linux,windows,darwin}/proc.cs`), so its emission moves on all
three targets and the two-seeded diff must be three-target.

## 5. Acceptance, stated BEFORE the work — the coordinator's ruling, recorded so nobody scores it afterwards

- **`net/http/pprof` (downstream) MAY BANK on the push alone**, since its handlers need collection
  only to function.
- **`runtime/pprof` does NOT bank.** Success is the failure mode **MOVING** — from an unreachable
  symbol to the four capability rungs measured 2026-08-29 (what the runtime can OBSERVE: wait-state
  names, spawning threads under a profile, walking another thread's stack). That is a named frontier,
  not a wall of the same kind.
- **If `runtime/pprof` banks**, the coordinator's reading of those rungs was wrong and that is the
  finding.
- **If the failure mode does not move at all**, the push was not the blocker and §1's measurement
  needs re-reading.
- A row can be blocked twice; clearing the mechanical blocker is progress even while the capability
  one stands.

## 6. Blast radius — a prediction, to be MEASURED by a two-seeded three-target `-stdlib` diff

**Predicted:** the differing set is `runtime/{mprof,cpuprof,tracestack}.cs`, `runtime/<goos>/proc.cs`
on all three targets, and `runtime/pprof/pprof.cs` — accessibility keywords on eight declarations and
eight consumer bodies where bodyless partials were. **Zero `GoPositionMap` lines** if the emission is
purely additive at the forwarders; a **removed** bodyless declaration re-encodes its file's map, so
map lines on `pprof.cs` alone would be expected and on the runtime files would not.

**Falsifiers:** any file outside that set; any project-reference line in the diff (§2 says there must
be none); any change to the ONE existing `linknamePushTargets` entry's emission, which is the
three-site regression surface W1 records for the var form and its function analogue here.

## 7. What is NOT settled

Whether `linknameForwardTargets`' gate composes with a per-GOOS body (`pprof_makeProfStack` is the
only member with one) is unverified — the registry's existing members are flat files. That is a
question for the cut's first conversion, not an argument against the shape.

-- C1, 2026-09-06

---

## Amendment, 2026-09-06 (at the cut): FIVE, not eight — the cut met three things this record did not know, and one of them inverts a claim above

Nothing above is rewritten. Every prediction stays visible beside what it was worth.

### 1. §7's sentence is INVERTED, and its arithmetic was wrong too

§7 says the per-GOOS composition is unverified *"since every existing member is a flat file."* The
registry has **EIGHT** members (I counted six from a grep window, not from the map), and **FIVE of the
eight emit PER-GOOS**, every one `public`:

| member | emitted body | shape |
|---|---|---|
| `syscall.loadlibrary` / `loadsystemlibrary` / `getprocaddress` | `syscall/windows/dll_windows.cs:154,158,172` | per-GOOS |
| `time.registerLoadFromEmbeddedTZData` | `time/{darwin,linux}/zoneinfo_read.cs:29` | per-GOOS |
| `runtime.fcntl` | `runtime/darwin/sys_darwin.cs:589`, `runtime/linux/os_linux.cs:482` | per-GOOS |
| `runtime.blockUntilEmptyFinalizerQueue` | `runtime/mfinal.cs:326` | flat |
| `net/textproto.readMIMEHeader` | `net/textproto/reader.cs:579` | flat |
| `go/types.srcimporter_setUsesCgo` | `go/types/api.cs:211` | flat |

**The composition question is CLOSED and needed no probe run:** `runtime.fcntl` is the exact analogue
of `pprof_makeProfStack` — same package, same per-GOOS shape, two platform bodies, both carrying the
`public` flip. The answer was in the corpus and §7 asserted "unverified" without opening a file.

### 2. `pprof_cyclesPerSecond` is a THIRD shape that NEITHER registry serves

§3 called it a push and §4 costed it as one `linknamePushTargets` entry. Measured: the entry **emits
nothing**, twice — first because I keyed the linkname SYMBOL where `funcLinknamePush` keys the
consumer's DECLARED NAME (`currentPackagePath+"."+funcDecl.Name.Name`; for `unique` the two coincide,
so the precedent does not disambiguate), and then, with the key corrected, because
`linknamePushDeclMatches` **rejects a consumer carrying a two-arg directive by design** — its own
comment says *"that is a PULL, a different mechanism entirely."*

And it is right to. Go's shape here is a push whose consumer then pulls the pushed symbol locally:
`runtime` defines `runtime/pprof.runtime_cyclesPerSecond`, and `runtime/pprof` declares
`pprof_cyclesPerSecond` under a two-arg directive naming that symbol. **The registry entry is removed
rather than left dead.** Serving this shape is a converter widening, not a curated row — so §4's "eight
curated entries and no new machinery" is false for the eighth.

### 3. THE CUT IS FIVE, because two of the seven are already hand-owned — and forwarding them would be a REGRESSION

`runtime/pprof/pprof_impl.cs` is a `[module: GoManualConversion]` companion answering
`pprof_memProfileInternal` and `pprof_goroutineProfileWithLabels`. The compiler said so first: CS0111
and CS0759 against the forwarders. Reading it changes the design rather than just the count.

- **The goroutine body deliberately WITHHOLDS the label slice.** A label pointer goes stale under GC,
  `printCountProfile` then sizes a slice from a corrupt map, and the process dies with
  `OutOfMemoryException` — the row becomes an `infrastructure-error`, i.e. **not a verdict at all**.
  The file records a refuted first attempt (filtering on `IsNative`, which dropped all 91 labels
  because native is the NORMAL state for a `FromPinnedBox` pointer) and concludes there is no cheap
  consumer-side test. Forwarding to runtime's real body would trade a measurable wrong answer for an
  unmeasurable one.
- **The memory body returns an honest `(0, true)`** and its header states that a row which STARTS
  PASSING there has laundered a false green, naming that as the assertion to re-run before believing
  any increment in the file.

Both are judgements a curated pull registry cannot make. **They stay hand-owned; the five that were
throwing become forwarders.**

### 4. …and that companion's stated premise is FALSE, measured

Its header says the push *"is an edge runtime -> runtime/pprof … so the forwarder would close a
project-reference CYCLE … no forwarder can exist, and the destination has to answer for itself."*
**The forwarder is on the CONSUMER side**, across the `runtime/pprof → runtime` edge that already
exists: injected, it reads **0 cycles on all three targets**. Only the other direction costs 38/36/36.
So five destinations were hand-waved as impossible when they were merely unperformed — the same class
COORD ruled on the same night: a comment claiming a behaviour the code does not have reads as the
census to the next reader.

### 5. The measured footprint, for five

5 files per target, **11 removed / 24 added**, 0 only-in, 0 project-reference lines. Applied: the three
runtime files wholesale (each byte-identical to the base emission, so the new emission IS committed +
this change), `pprof/pprof.cs` by 3-way merge (applied delta 23 = emission delta 23, zero standing-drift
lines carried), and **the two `GoPositionMap` lines deliberately NOT applied**: the committed
`pprof.cs` carries unbanked forced-init drift, so the fresh map would describe neither tree. That
belongs to the deliberate regen, per the standing rule.

### 6. What §5's acceptance becomes

Unchanged in kind and narrower in reach: five destinations move, not eight. `pprof_cyclesPerSecond`
still throws, so the CPU-profile paths are untouched; the memory and goroutine profiles keep their
hand-owned answers, so any movement there would be a REGRESSION and the companion's own
laundering assertion is the check.

-- C1, 2026-09-06
