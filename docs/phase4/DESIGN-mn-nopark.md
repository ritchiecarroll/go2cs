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

Neither form changes R1's population. Every public member still gets its twin or its forwarder; W and C move only
which members are state machines and which forwarders are marked. So they are not partial-set switches, O2
(GO2CS0018) does not apply, and P8's invariance holds by ruling (iii): a member that stops parking gains a mark
(its twin still exists), never loses a surface member.

## 3. Where the facts live

| form | read when | carried how | stdlib entries |
|---|---|---|---|
| W | building the CALLEE's assembly, and every caller's | the callee's twin carries **`[GoParkWeak]`** (new golib attribute, `[mn-frozen]` beside `[GoParkForwarder]`); a caller's generator reads it from metadata, as it reads `[GoParkForwarder]` | `sync.Mutex.Lock`, `sync.RWMutex.Lock`, `sync.RWMutex.RLock`, internal/poll `fdMutex`'s `rwlock` (the file-descriptor lock) |
| C | building the CALLER's assembly only | nothing crosses assemblies: the caller's own twins come out plain at that site, so its members' marks say the rest | fmt `pp.handleMethods` -> `error.Error`, `Stringer.String`, `Formatter.Format`, `GoStringer.GoString`; fmt `ss.ReadRune` and `readRune.ReadRune` -> `io.Reader.Read` and `io.RuneScanner.ReadRune` |

**The stdlib's entries** are a table in go2cs-gen (`ParkCuts.cs`), keyed by Go package path and Go declaration, read
when the generator builds that package. One table, reviewed like code, versioned with the generator that reads it.

**A consumer's entries**: two MSBuild properties, the same shape as `GoTwinsIfaceUnmark` (stage (c)), passed as
`CompilerVisibleProperty`: `GoTwinsWeak` (members of THIS assembly, `Namespace.Type.Member`, `;`-separated) and
`GoTwinsCut` (`Namespace.Type.Caller->Namespace.Type.Callee` pairs in this assembly). A consumer cannot weaken
another assembly's member: that member's own build decides it (its `[GoParkWeak]`), so one assembly is never read
two ways.

**A stale entry** (it names no member, or no such call site) is reported, never ignored: **GO2CS0019** (Warning,
go2cs-gen), naming the entry. For the stdlib table the GenTests row reads it as an error, so the table cannot rot.

## 4. The generator's change (P4's code)

1. `ParkCuts`: parse the table and the two properties into a set of weak members and a set of cut sites, per build.
2. `ComputeParks`: a site whose target is weak (`[GoParkWeak]` in metadata, or W-listed in this assembly) or whose
   (caller, target) is cut does not make the candidate park.
3. `ShouldAwait`: a cut site is never awaited. A weak target IS awaited when the caller is a twin anyway (the
   rewrite runs only for a twin, so this is the existing path).
4. Emission: a W-listed member's twin (or forwarder) carries `[GoParkWeak]`. The forwarder mark rule (iii) is
   unchanged: `[GoParkForwarder]` is set from `Parks`, which W does not change for the member itself (Lock can
   park), only for its callers.
5. The report counts `weak-sites=N cut-sites=M`, and `twinned` before and after on the same build reads the effect.
6. GO2CS0019 for a stale entry; DiagnosticCodes.md row.

## 5. Predictions and GenTests rows (each red first)

| # | row / prediction | stop line |
|---|---|---|
| N1-1 | a caller whose ONLY park is a call to a weak member is not twinned (red: it is today) | - |
| N1-2 | a caller twinned for another reason awaits the weak member (the twin's body awaits `Lockꞏtwin`) | - |
| N1-3 | a cut site stays a plain call inside a twin; a caller whose only park was that site is not twinned | - |
| N1-4 | cross-assembly: a library's weak member carries `[GoParkWeak]`; the other assembly's caller is not twinned by it | - |
| N1-5 | a stale entry reports GO2CS0019 naming it; the stdlib table's row fails on one | - |
| N1-6 | O2 does not fire for GoTwinsWeak / GoTwinsCut; R1's population is unchanged (GO2CS0017 = 0) | - |
| N1-7 | row 7's twinned stdlib, the TypeScript closure: marked share at most 3% (the spike read 1.9% with the 3 cuts) | over 5%: read the next `-why` witness, a design read |
| N1-8 | the closure (MnHttpClosure) under `GO2CS_MN=1`: output EQUAL to Go; parks > 0 | any difference |

## 6. What it costs, stated with it

Behind W, a goroutine that is not otherwise a twin and finds the lock held blocks its thread (the P is handed off).
Behind C, a hook that parks holds its carrier. Both are the spike's measured shape for an untwinned blocking call, and
N5 (blocking the scheduler cannot see) is the policy that governs them. S2's contended-path arm reads the lock case.
