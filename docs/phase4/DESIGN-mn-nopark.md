# DESIGN — M:N N1 (row 5): edges that are not parks — weak members and cut sites

Row 5 of the .5 plan (owner, 2026-11-21: everything in .5). Design G; code P4 (3 lane-days); this text first.

## 1. The problem, as measured

The park rule marks a function a twin when any direct call in it can park (TwinGenerator `ComputeParks`: a site that
`ShouldAwait`, across assemblies "has a twin that binds": `TwinBinds`, a twin without `[GoParkForwarder]`). On the
TypeScript module the spike read 42.4% marked (65% of the checker, 73% of the parser) through three edges that do not
park in practice, and 1.9% with them cut (DATA-mn-spike-2.md 7.4, 7.5):

1. **Locks on static paths.** `ast.SourceFile` keeps lazy tables behind several locks; 60% of the checker reaches one.
2. **`fmt`'s method hook.** `(*fmt.pp).handleMethods` calls the operand's `Error`, `String`, `Format`, `GoString`
   through an interface; some `String` somewhere prints to an `*os.File`, so every formatter is marked.
3. **`fmt`'s rune reader.** `(*fmt.ss).ReadRune` and `(*fmt.readRune).ReadRune` read an `io.Reader`; through
   `math/big`'s scanner adapter the whole parser is marked.

Under L-min the stdlib is plain, so edges 1-3 do not mark anything today: a plain stdlib has no binding twins. **Row 7
(the stdlib twinned) brings them back**: `sync.Mutex.Lockꞏtwin` and fmt's twins would bind, and the spike's 42% would
return on any lock-heavy or formatting module. N1 must land with row 7, before row 10 reads sizes (T1).

## 2. Two forms, one meaning ("not a park for the generator's purposes")

**W. A weak member.** Its twin may park, but a call to it never makes its caller a twin. A caller that is a twin for
another reason awaits it. This is the spike's "sharper lock rule". Locks are the case: a goroutine contending a lock
in a function that parks anyway parks on the lock; one that does not holds its carrier while blocked, as any plain
blocking call does (the hand-off of 3.5, the monitor's retake).

**C. A cut site.** A named call site (caller + callee) is not a park at all. The call stays plain even inside a twin.
The behind-the-cut cost is the spike's: the plain body runs, the output is Go's, a carrier is held while that edge
blocks (7.5's table). fmt's hooks are the case: a `String` method that parks is rare and holds its carrier.
A cut's callee may be an interface member OR a concrete function (fmt's rune reader is cut at a call to `io.ReadFull`).

Neither form changes R1's population. Every public member still gets its twin or its forwarder; W and C move only
which members are state machines and which forwarders are marked. So they are not partial-set switches, O2
(GO2CS0018) does not apply, and P8's invariance holds by ruling (iii): a member that stops parking gains a mark
(its twin still exists), never loses a surface member.

## 3. Where the facts live

| form | read when | carried how | stdlib entries |
|---|---|---|---|
| W | building the CALLEE's assembly, and every caller's | the callee's twin carries **`[GoParkWeak]`** (new golib attribute, `[mn-frozen]` beside `[GoParkForwarder]`); a caller's generator reads it from metadata, as it reads `[GoParkForwarder]` | `sync.Mutex.Lock`, `sync.RWMutex.Lock`, `sync.RWMutex.RLock`, internal/poll `fdMutex`'s `rwlock` (the file-descriptor lock) |
| C | building the CALLER's assembly only | nothing crosses assemblies: the caller's own twins come out plain at that site, so its members' marks say the rest | six sites, section 3.1 |

### 3.1 The stdlib's cut sites, as they are at the corpus (amended 2026-11-21, P3's MUST 1)

Read in the converted source (`src/core/fmt`, go1.26.11 corpus), not in Go's: the first draft named two rune-reader
sites that do not exist (readRune.ReadRune never calls a reader directly; ss.ReadRune never calls `Reader.Read`).

| caller (Go) | callee (Go) | at the corpus |
|---|---|---|
| `(*fmt.pp).handleMethods` | `fmt.Formatter.Format` | print.cs:825 `formatter.Format(...)` |
| `(*fmt.pp).handleMethods` | `fmt.GoStringer.GoString` | print.cs:837 `stringer.GoString()` |
| `(*fmt.pp).handleMethods` | `error.Error` | print.cs:855 `v.Error()` |
| `(*fmt.pp).handleMethods` | `fmt.Stringer.String` | print.cs:861 `v.String()` |
| `(*fmt.ss).ReadRune` | `io.RuneScanner.ReadRune` | scan.cs:238 `s.rs.ReadRune()` |
| `(*fmt.readRune).readByte` | `io.ReadFull` (a concrete function) | scan.cs:393 `Δio.ReadFull(r.reader, ...)` |

`readRune.ReadRune` (scan.cs:413, :426) reaches the reader only through `readByte`, so cutting `readByte`'s call leaves
it unmarked with no entry of its own. NOT cut: `io.ReadAtLeast -> Reader.Read`. That would make every `io.ReadFull`
in every program plain, not only fmt's.

**Amended 2026-11-23 (P4, implementing): five rows, not six, and the table's keys bind the method body.** The first
whole-stdlib build with twins (`-p:GoStdlibTwins=true`) read GO2CS0019 on all six rows, "names no such call site".
The converter writes a library method's body as a plain static `<Name>⁔(/*this*/ recv, …)` (methodHolder.go,
GlyphMethodSuffix) beside a Go-named forwarder, so the keys matched only the forwarder, whose one call is the body. The
generator now keys a suffix-form body as its Go method (receiver from its first parameter, suffix dropped); GenTests
N1-3b holds that shape. The rebuilt fmt then bound five cuts and read the `error.Error` row DEAD: golib's `error` has no
interface twin (golib is not twinned), so `v.Error()` never parks and the cut does nothing. That row is removed from
the table, with a comment saying when it returns. The GenTests stdlib row binds a fake fmt and could not see either
fact; the twinned stdlib build's GO2CS0019 = 0 is the gate that does. The table above stays as written.

**The stdlib's entries** are a table in go2cs-gen (`ParkCuts.cs`), keyed by Go package path and Go declaration, read
when the generator builds that package. One table, reviewed like code, versioned with the generator that reads it.

**A consumer's entries**: two MSBuild properties, the same shape as `GoTwinsIfaceUnmark` (stage (c)), passed as
`CompilerVisibleProperty`: `GoTwinsWeak` (members of THIS assembly, `Namespace.Type.Member`, `;`-separated) and
`GoTwinsCut` (`Namespace.Type.Caller->Namespace.Type.Callee` pairs in this assembly). A consumer cannot weaken
another assembly's member: that member's own build decides it (its `[GoParkWeak]`), so one assembly is never read
two ways.

**A stale entry** is reported, never ignored: **GO2CS0019** (Warning, go2cs-gen), naming the entry and which arm it
hit. For the stdlib table the GenTests row reads it as an error, so the table cannot rot. Its predicate (amended,
COORD's ruling 3) has four arms:
- the entry names no member;
- it names no such call site;
- DEAD, a cut site whose callee no longer parks (the cut does nothing);
- DEAD, a W member whose own twin no longer parks (it would be marked `[GoParkForwarder]` anyway).
The two dead arms are what keep a list honest across a Go hop: after the hop an edge can stop parking, and the entry
would otherwise read as live.

### 3.2 `[GoParkWeak]` in the freeze (amended, COORD's ruling 4)

Admitted to `[mn-frozen]` by COORD's ruling of 2026-11-21 (inbox/G/20261121T230123Z-COORD.md), cited beside the lines
as the section's header requires. The lines, in the writer's spelling, as `GoParkForwarderAttribute` has:

    go.golib.GoParkWeakAttribute :: ctor public ()
    go.golib.GoParkWeakAttribute :: shape sealed class; base System.Attribute; usage Method, multiple False, inherited False

Two rules:
- **A member never carries both.** `[GoParkForwarder]` says the member cannot park; `[GoParkWeak]` says it can. A
  W-listed member whose twin cannot park is a dead entry (GO2CS0019) and carries the forwarder mark only.
- **W is version-safe both ways, so changing it later is no break.** A caller built against W (it did not twin itself
  for that call, and calls plain) runs correctly against a callee without W: the plain call blocks its thread where
  it would have parked. A caller built without W (twinned, it awaits the twin) runs correctly against a callee with W:
  the twin is still there. Either way the output is Go's; only the carrier held while blocked differs.

### 3.3 Promoted forwarders and interface members (COORD's rulings 5 and 6)

- **A promoted forwarder of a weak member is itself weak.** `type T struct{ sync.Mutex }` gives `T` a generated
  `Lock` forwarder whose only park is the weak call. By N1-1 alone it would not be twinned, so a twin caller of
  `t.Lock()` would block its carrier where `t.mu.Lock()` parks. Instead the forwarder is twinned and carries
  `[GoParkWeak]`, so `t.Lock()` and `t.mu.Lock()` behave the same in a twin and in a plain caller. Embedding a mutex
  is common Go; parity wins over the count.
- **An interface member stays a park, and is never weak.** `sync.Locker.Lock` is a different member from
  `sync.Mutex.Lock`, and its implementers are open-ended across assemblies, so "weak when every parking implementer
  is weak" cannot be closed at build time. NAMED COST: a function that locks through `sync.Locker` (or another
  interface over a weak member) is twinned for that call, as today, where the same call on the concrete type would
  not twin it: size and state machines, never a wrong result. No real program has been measured paying it; a row-10
  reading that finds one brings the count.

**Amended 2026-11-23 (P4, implementing; COORD's ruling of 2026-11-23 22:50Z): the first bullet is DROPPED from N1.**
The converter does not call a promoted method through the forwarder. It writes `t.Lock()` on `type T struct{ sync.Mutex }`
as a call through the embedded field (`Ꮡt.of(T.ᏑMutex).Lock()`; convSelectorExpr.go, the same-package and
cross-package forms alike; crypto/tls conn.cs shows the shape at the corpus). So a converted call reaches
`Mutex.Lock` itself, which is weak, and `t.Lock()` and `t.mu.Lock()` already behave the same in a twin and in a
plain caller. Parity holds by construction for converted calls, and N1-5b asserts that shape. The `[GoPromoted]`
forwarder is reached only by a method value (`f := t.Lock`), a method expression (`T.Lock`), a generated interface
adapter, and hand-written C#. It is generated by a sibling generator, so it is never a twin candidate, with or without
N1: such a call blocks its carrier where `Mutex.Lock` would park in a twin (a missed weak park, never a wrong result).
That residue is a BOARD row (OPEN, candidate post-.5), not part of this seat. The bullet above stays as written.

## 4. The generator's change (P4's code)

1. `ParkCuts`: parse the table and the two properties into a set of weak members and a set of cut sites, per build.
2. `ComputeParks`: a site whose target is weak (`[GoParkWeak]` in metadata, or W-listed in this assembly) or whose
   (caller, target) is cut does not make the candidate park.
3. `ShouldAwait`: a cut site is never awaited. A weak target IS awaited when the caller is a twin anyway (the
   rewrite runs only for a twin, so this is the existing path).
4. Emission: a W-listed member's twin (or forwarder) carries `[GoParkWeak]`. The forwarder mark rule (iii) is
   unchanged: `[GoParkForwarder]` is set from `Parks`, which W does not change for the member itself (Lock can
   park), only for its callers. Never both on one member (3.2).
5. Promoted forwarders (3.3): a promoted-method forwarder whose body is the one call to a weak member is twinned as
   that member's twin is (its await of the weak member is its park), and carries `[GoParkWeak]` itself.
   **Amended 2026-11-23: dropped** (3.3's amendment). Converted calls reach the weak member through the embedded
   field, never through the forwarder; the forwarder's own callers are the BOARD row named there.
6. The report counts `weak-sites=N cut-sites=M`, and `twinned` before and after on the same build reads the effect.
7. GO2CS0019 for a stale entry, all four arms of 3's predicate; DiagnosticCodes.md row.

## 5. Predictions and GenTests rows (each red first)

| # | row / prediction | stop line |
|---|---|---|
| N1-1 | a caller whose ONLY park is a call to a weak member is not twinned (red: it is today) | - |
| N1-2 | a caller twinned for another reason awaits the weak member (the twin's body awaits `Lockꞏtwin`) | - |
| N1-3 | a cut site stays a plain call inside a twin; a caller whose only park was that site is not twinned | - |
| N1-4 | cross-assembly: a library's weak member carries `[GoParkWeak]`; the other assembly's caller is not twinned by it. CONTROL: the same member WITHOUT the attribute twins that caller (COORD's ruling 7) | - |
| N1-5 | a stale entry reports GO2CS0019 naming it, one row per arm of the predicate (no member, no site, dead cut, dead W); the stdlib table's row fails on one | - |
| N1-5b | a promoted forwarder of a weak member (`struct{ sync.Mutex }`): a twin caller of `t.Lock()` awaits it exactly as `t.mu.Lock()`; a plain caller is not twinned by it; the forwarder's twin carries `[GoParkWeak]` | - |
| N1-5c | a call through an interface over a weak member (`sync.Locker.Lock`) still twins its caller (3.3's named cost) | - |
| N1-6 | O2 does not fire for GoTwinsWeak / GoTwinsCut; R1's population is unchanged (GO2CS0017 = 0) | - |
| N1-7 | row 7's twinned stdlib, the TypeScript closure: marked share at most 3% (the spike read 1.9% with the 3 cuts) | over 5%: read the next `-why` witness, a design read |
| N1-8 | the closure (MnHttpClosure) under `GO2CS_MN=1`: output EQUAL to Go; parks > 0 | any difference |

### 5.1 The hang rows, at ONE P (amended, P3's MUST 2)

W and C cannot give a wrong answer: each replaces an await with the plain call every untwinned caller already makes.
The one wrong RESULT available is a hang: a carrier blocked behind a cut or weak edge whose release needs another
goroutine to run, with no P left to run it. That rests on the hand-off of a blocking plain call (DATA-mn-spike-2.md
3.5) and the monitor's retake (5.5), and no row above makes it happen. Three behavioural rows do, each under
`GO2CS_MN=1` with `runtime.GOMAXPROCS(1)`, output EQUAL to Go, and a bounded run time (a watchdog that reds the row
by name, as MnLockPin's does):

| # | the shape | what must not hang |
|---|---|---|
| N1-9 | a twin goroutine formats a value whose `String` does `<-ch` (cut C at handleMethods); a second goroutine sends on `ch` | the formatting carrier blocks in plain `String`; the sender must still run on the one P |
| N1-10 | `fmt.Fscan` over an `io.Pipe` reader (cut C at the rune reader); the writer writes from a second goroutine | the scanning carrier blocks in the plain read; the writer must still run |
| N1-11 | a NON-twin function calls `Lock` on a mutex held by a twin goroutine that is parked on a channel (weak W); a third goroutine sends, the holder wakes and unlocks | the non-twin's carrier blocks on the lock; the holder and the sender must still run |

P5: if one of these is already an S2 arm (arm 2, contended paths, is the nearest to N1-11), cite it, and it becomes
an N1 gate as it stands.

## 6. What it costs, stated with it

Behind W, a goroutine that is not otherwise a twin and finds the lock held blocks its thread (the P is handed off).
Behind C, a hook that parks holds its carrier. Both are the spike's measured shape for an untwinned blocking call, and
N5 (blocking the scheduler cannot see) is the policy that governs them. S2's contended-path arm reads the lock case.
