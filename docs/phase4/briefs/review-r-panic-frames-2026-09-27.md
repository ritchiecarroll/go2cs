# Review: R's panic-path frames cut B at a419543c14 (2026-09-27)

COORD reviewed claude/r-panic-frames a419543c14 (3 signed commits on the TRAIN G union e6fc210500) in P2's place, since P2 was out of the fleet. Four finder lenses read the diff: Go frame semantics, with go1.24.13 `go run` as ground truth; per-thread state and ownership; the no-panic hot-path cost; and the test arms against the prediction. Two skeptics then tried to refute each blocker and concern, one on the facts and one on whether it matters. 21 findings survived and 1 was refuted.

**VERDICT: NOT ACCEPTED as it stands.** The approved design (option B with P2's four changes) is sound, and the no-panic cost is small. But the ONE ADDITION, the Beneath chain, breaks the cut's own stated invariant ("a failed owner check anywhere still splices nothing"), and one class of runtime error splices a wrong list. Under the design's own rule, a missing splice is a known divergence and a WRONG splice is a silent lie.

| # | Defect | Findings | Severity |
|---|---|---|---|
| B1 | splicePanicInto follows Beneath on ANY owner mismatch, not only when the panic was first caught by THIS Run (managed_impl.cs:2074-2081), and Run sets `raised.Beneath ??= running` in whichever Run's catch receives the panic, rethrown ones included (GoFrame.cs:259-260, 286-287). A replacing panic first caught by an intermediate deferring frame therefore splices with frames silently missing. Shape A: `defer func(){ gDeferPanics() }()`. Shape B: `defer func(){ gNormalDeferPanics() }()`. Shape A drops A.func2; shape B drops gNormalDeferPanics and B.func2. Both skeptics reproduced Go's lists. Any deferred call whose callee has its own GoFrame (`defer mu.Unlock()`-like callees) reaches it. Suggested fix: set Beneath, and follow it, only for a panic ANCHORED at this Run: never rethrown by a nested Run's tail, and with its raw SiteTrace ending at GoFrame.Run (or at the delegate this Run just invoked, to keep `defer gDeferPanics()` right). | S01 S06 S12 S16 | blocker |
| B2 | A panic whose site has NO Go frame (a nil deferred func, or any golib-raised panic directly under Run) skips the owner check, or stops the chain, so the older panic's runtime.gopanic is missing: a wrong answer in the very shape the addition claims ("a panic raised by a deferred call with no frame of its own"). | S02 S07 S15 | blocker |
| C1 | The [P2-2] owner check compares MethodBase, so a RECURSIVE (or mutually recursive) deferring function passes it at a different activation. The inner activation, and any frames between the two activations, then disappear. Residual (i) says this case splices nothing. | S03 S08 S13 S17 | concern (a wrong splice; fix with activation identity, e.g. the Run's frame token) |
| C2 | Runtime errors other than nil dereference and integer divide: Go shows a runtime frame between gopanic and the site (goPanicIndex, panicdottypeE, mapassign_faststr, ...), and the cut splices gopanic directly over the site. The design says "named, NOT claimed"; the cut neither refuses these nor lists them as residuals. Refuse (no splice) unless modelled. | S05 S19 | concern (a wrong splice) |
| C3 | Missing but determinable splices: a deferred call that panics during a NORMAL return, after a completed recovery, the `defer panic(v)` shape, and during Goexit; the nil-thunk case in the same position does splice, so the two paths disagree. | S04 S09 S18 | concern (missing, not wrong) |
| C4 | The chain bound admits 65 links (depth 0..64), and at 66 or more it drops the WHOLE splice where Go truncates from the top. | S10 | concern |
| C5 | Missing arms: shapes A and B, recursion, the no-frame link, repanic depth > 1, the bound, a cycle, [P2-3]'s skip and capacity path, and a spliced frame's line (TestCallersPanic reads lines). The census reuse arm does not plant the two new thread-statics. | S11 S20 | concern |
| C6 | The prediction contradicts itself on parent verdicts: "every other verdict: 0 moved", while TestStackWrapperStackPanic may move and the intermediate parents /sigpanic and /panicwrap are not named. Restate it before the gates. | S21 | concern |
| COST | The no-panic hot path adds no allocation after the first per thread, no stack capture, no dictionary operation and no new EH region: 3 [ThreadStatic] reads, 2 writes, one covariant array-store helper and 2 bounds checks per deferring return, and a local null test per deferred call. Acceptable; PerfDefer before/after still owed. | S14 | info |

**Asks for the re-cut:** fix B1 and B2 (anchor the chain), C1 (activation identity) and C2 (refuse unmodelled runtime-error sites), each red first with an arm that expects Go's list or no splice; take C3 or state it as a residual; fix C4; add C5's arms; restate C6's prediction. Then the gates as planned.

## Appendix: the surviving findings

### S01 (blocker; lens: GO SEMANTICS OF THE FRAMES; skeptics refuting: 0 of 2)

**Claim.** The ONE ADDITION's Beneath chain splices WRONG frames. The problem case is a replacing panic first caught by a frame that defers inside the panicking deferred call. splicePanicInto follows Beneath on ANY owner mismatch, not only when the site ends at THIS Run. So the frames between the first catcher and the owner's Run are silently dropped. This contradicts the stated rule that a failed owner check anywhere still splices nothing.

**Evidence.** src/core/runtime/managed_impl.cs:2074-2081: `if (owner is not null && site[^1].GetMethod() == owner) ... else if (panic.Beneath is { } older) beneath = older; else return false;`. Nothing checks that the raw SiteTrace ended at GoFrame.Run, or at which Run. src/core/golib/GoFrame.cs:259-260 sets `raised.Beneath ??= running` in whichever Run's catch receives the panic, including one rethrown by a nested Run's tail (GoFrame.cs:286-287). Shape A, `defer func(){ gDeferPanics() }()`: p2's SiteTrace ends at gDeferPanics (its own catch caught it first). gDeferPanics' Run rethrows p2, and the owner's Run catch sets p2.Beneath = p1. The splice then emits gopanic, gDeferPanics, gopanic and misses the closure frame. Shape B, `defer func(){ gNormalDeferPanics() }()`: g's own deferred call panics during g's normal return. The site ends at Run(g), not the owner's Run. The splice emits gopanic, g.func1, gopanic and misses gNormalDeferPanics and the closure. Both are the 'deferred call that panics again' shape the cut claims. Go output from gosem/main.go shapes 7 and 7c.

**Go:**

```go
func gDeferPanics() { defer func(){}(); panic("p2") }
func gNormalDeferPanics() { defer func(){ panic("p2") }() }
func A() (got []string) {
    defer func(){ recover(); got = callersHere() }()
    defer func(){ gDeferPanics() }()
    panic("p1")
}
func B() (got []string) {
    defer func(){ recover(); got = callersHere() }()
    defer func(){ gNormalDeferPanics() }()
    panic("p1")
}
// Go 1.24.13, frames after callersHere:
// A: A.func1 | runtime.gopanic | gDeferPanics | A.func2 | runtime.gopanic | A | main
// B: B.func1 | runtime.gopanic | gNormalDeferPanics.func1 | gNormalDeferPanics | B.func2 | runtime.gopanic | B | main
// cut (by reading): A.func1 | gopanic | gDeferPanics | gopanic | A | main   (A.func2 missing)
//                   B.func1 | gopanic | gNormalDeferPanics.func1 | gopanic | B | main   (two frames missing)
```

**Suggested fix.** Make the splice depend on the panic's FIRST catcher, not on MethodBase coincidence. Mark a PanicException when a Run's tail rethrows it: a flag or counter set at GoFrame.cs:287 before `throw owned`. In Run's catch, set Beneath, and treat the panic as anchored at this Run, only when the panic was never rethrown and its raw SiteTrace's last frame is GoFrame.RunMethod. In splicePanicInto, follow Beneath only for an anchored panic; any other mismatch returns false. If `defer gDeferPanics()` (the delegate is itself the deferring frame, correct today) should stay spliced, allow exactly one rethrow, by the Run of the delegate this Run just invoked. Add red arms for shapes A and B that expect no splice (or Go's list, if modelled).

### S02 (blocker; lens: GO SEMANTICS OF THE FRAMES; skeptics refuting: 0 of 2)

**Claim.** A panic whose site has no Go frame never follows Beneath, so a nil deferred func called while the sequence runs a panic drops the replaced panic's gopanic and its frames. The ONE ADDITION's text covers exactly this: 'a panic raised by a deferred call with no frame of its own'.

**Evidence.** src/core/runtime/managed_impl.cs:2074: the owner-and-Beneath decision sits inside `if (site.Count > 0)`. With an empty site, `beneath` stays null and 2102 `return beneath is null || ...` completes the splice after the fault frames. The nil thunk is GoFrame.cs:64 `s_nilDeferredCall`, a golib lambda that isGoSourceFrame rejects, so its site has no Go frame. Run's catch still sets its Beneath = p1 (GoFrame.cs:259-260), but the splice never reads it. The same happens for any golib-thunk-raised panic in a panicking sequence. Go output from gosem shape 12.

**Go:**

```go
func N() (got []string) {
    defer func(){ recover(); got = callersHere() }()
    var fn func()
    defer fn()
    panic("p1")
}
// Go 1.24.13: N.func1 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | runtime.gopanic | N | main
// cut (by reading): N.func1 | gopanic | panicmem | sigpanic | N | main   (p1's gopanic missing)
```

**Suggested fix.** In splicePanicInto, when site.Count == 0 and the panic has an anchored Beneath (see the first finding), set `beneath = panic.Beneath` and continue. TestCallersDeferNilFuncPanic is unaffected because its thunk panics in a normal-return sequence, so Beneath is null. Add an arm: nil deferred func plus panic(p1), expecting gopanic, panicmem, sigpanic, gopanic, owner.

### S03 (concern; lens: GO SEMANTICS OF THE FRAMES; skeptics refuting: 0 of 2)

**Claim.** The [P2-2] owner check compares MethodBase, so a RECURSIVE deferring function defeats it. The intermediate frame that first caught the panic is another activation of the owner method, so the check passes. The 'intermediate deferring frame' residual then splices a wrong list: the inner activation (and anything between the two activations) is dropped, where R's residual says nothing should be spliced.

**Evidence.** src/core/runtime/managed_impl.cs:2076 `site[^1].GetMethod() == owner`. For rec(0) calling rec(1), where rec(1) panics: rec(1)'s catch captures SiteTrace [rec]. rec(1)'s Run rethrows, and rec(0)'s Run pushes the same panic. owner = rec, site[^1] = rec (the inner activation), so they compare equal and the frame is dropped. The same happens when rec(0)'s Beneath chain bottoms out on such a panic. Go output from gosem shape 6.

**Go:**

```go
var recGot []string
func rec(n int) {
    defer func() { if n == 0 { recover(); recGot = callersHere() } }()
    if n == 0 { rec(1) } else { panic("rec") }
}
// Go 1.24.13: rec.func1 | runtime.gopanic | rec | rec | main
// cut (by reading): rec.func1 | gopanic | rec | main   (inner rec missing: a wrong splice, not a missing one)
```

**Suggested fix.** Use the same rethrown-by-a-Run marker as the first finding: a claimed panic that any Run already rethrew fails the owner check. That closes recursion and keeps the MethodBase compare as a second guard. Add a recursive ownerMismatch arm that expects no splice.

### S04 (concern; lens: GO SEMANTICS OF THE FRAMES; skeptics refuting: 0 of 2)

**Claim.** A deferred call that panics in a NORMAL-RETURN sequence splices nothing, although its site ends at THIS Run and Go's frames are fully determined. The missed cases are: no earlier panic, after a recovery that completed, the DeferPanicArg shape `defer panic(v)`, and a panic during Goexit. The nil-thunk case in the same position does splice, so the two paths disagree.

**Evidence.** src/core/runtime/managed_impl.cs:2078-2081: site[^1] is the deferred closure (not the owner) and Beneath is null (GoFrame.cs:259 found a null entry), so it returns false and nothing is spliced. By contrast, the site.Count == 0 path (nil thunk) emits gopanic plus the fault frames in exactly this position (the ANilDeferredFunc arm). `defer panic(v)` emits `defer(ᴛ1 => throw panic(ᴛ1), v, ref ᒐ)` (src/tests/Behavioral/DeferPanicArg/main.cs:24); its site is [lambda, golib ladder closure, Run]. Go output from gosem shapes 8, 9b, 11 and 20.

**Go:**

```go
func S() (got []string) {
    defer func(){ recover(); got = callersHere() }()
    defer func(){ panic("p2") }()
    return
}
func P() (got []string) {
    defer func(){ recover(); got = callersHere() }()
    defer panic("p2")
    return
}
// Go 1.24.13: S.func1 | runtime.gopanic | S.func2 | S | main
//             P.func1 | runtime.gopanic | P.deferwrap1 | P | main
// after a completed recovery: R.func1 | runtime.gopanic | R.func2 | R | main
// cut (by reading): S.func1 | S | main ; P.func1 | P | main  (missing: safe, but an arm the claimed 'deferred call that panics' shape lacks)
```

**Suggested fix.** With the first-catcher marker from the first finding, a panic anchored at this Run with no Beneath completes after its site's Go frames: emit gopanic, then the site, then let the live walk continue at the owner. Open-coded defers put the deferred call directly on the owner. Defers inside a loop would still lack runtime.deferreturn, the same ruled residual as the WithLoop test. Add an arm for this shape.

### S05 (concern; lens: GO SEMANTICS OF THE FRAMES; skeptics refuting: 1 of 2)

**Claim.** Runtime-error panics other than nil-deref and divide splice a WRONG list. Go puts a runtime frame between gopanic and the site: goPanicIndex, panicdottypeE, mapassign_faststr, and so on. The cut emits gopanic directly above the site, because FaultKind is None for everything golib raises except NilPointerDereference and IntegerDivideByZero. By [P2-2]'s own rule this is a lie, not a gap. The design names it 'not claimed', but the code comment files it with explicit panic(v).

**Evidence.** src/core/golib/runtime/RuntimeErrorPanic.cs:23 and :171 are the only places that set FaultKind. IndexOutOfRange (RuntimeErrorPanic.cs:116-118) and the other factories set none. managed_impl.cs:2086-2098 emits no runtime frame for PanicFaultKind.None, and its comment says 'an explicit panic(v) adds none'. Go output from gosem shapes 17-19.

**Go:**

```go
func idx(s []int, i int) int { return s[i] }
func assertIt(x any) int { return x.(int) }
func mapWrite(m map[string]int) { m["a"] = 1 }
// each called under: defer func(){ recover(); got = callersHere() }()
// Go 1.24.13:
// indexPanic.func1  | runtime.gopanic | runtime.goPanicIndex      | idx      | indexPanic
// assertPanic.func1 | runtime.gopanic | runtime.panicdottypeE     | assertIt | assertPanic
// nilMapPanic.func1 | runtime.gopanic | runtime.mapassign_faststr | mapWrite | nilMapPanic
// cut (by reading): ... | gopanic | idx | indexPanic   (runtime frame missing)
```

**Suggested fix.** Choose one: (a) tag the RuntimeErrorPanic factories with the Go runtime frame they stand for (goPanicIndex/goPanicSlice* family, panicdottypeE/I, panicnildottype, mapassign_faststr, chansend1 or closechan, panicshift, panicmakeslicelen...) and splice it; or (b) have golib-raised runtime errors without a modelled frame fail the splice, so the answer is missing rather than wrong. Either way, disclose the choice beside the stated panicwrap residual.

### S06 (blocker; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** The claim 'a failed owner check anywhere still splices nothing' is false. splicePanicInto moves on to Beneath whenever the owner check fails, without checking that the failing link's site actually ends at this Run. A panic raised in a deferring CALLEE of a deferred call therefore splices with the deferred call itself silently missing.

**Evidence.** GoFrame.cs:259-260: Run's catch sets raised.Beneath ??= running for every panic it catches while its entry is non-null, however far below the Run the panic was first caught. PanicException.cs:268-274: SiteTrace is fixed at the FIRST catch. In the shape below that catch is G's own filter, so SiteTrace = [G]. G's Run pushes and pops its entry, then rethrows P2. D has no frame of its own. O's Run catch then sets P2.Beneath = P1 and SetSequence(0, P2). In O's next deferred call, splicePanicInto (managed_impl.cs:2074-2081) finds site[^1] = G, which is not the owner O. Instead of failing, it takes 'else if (panic.Beneath is { } older)' and adds gopanic and G. At the bottom link P1's site is [O], which matches, drops O, and adds gopanic. D appears in no trace: it lies between the first catcher G and the Run. The gate rows are unaffected, because TestCallersAbortedPanic's deferred call raises the panic directly.

**Go:**

```go
func G() { defer func() {}(); panic("p2") }
func D() { G() }
func O() {
	defer func() { recover(); fmt.Println(names(1, 64)) }() // names = Callers + CallersFrames
	defer D()
	panic("p1")
}
// Go 1.24.13: names | O.func1 | runtime.gopanic | G | D | runtime.gopanic | O | main.func1 | main | runtime.main | runtime.goexit
// cut (traced): names | O.func1 | runtime.gopanic | G | runtime.gopanic | O | ...   <- D missing: a wrong splice, not a missing one
```

**Suggested fix.** Follow Beneath only from a link whose site provably reaches this Run. Accept (a) a raw SiteTrace whose last frame is GoFrame.RunMethod, meaning the deferred call raised the panic with no deferring frame in between. Accept (b), one hop, when the deferred call itself was the first catcher and its Run rethrew straight into this Run. Check (b) at GoFrame.cs:259 by comparing the Go frames of the current exception trace with [SiteTrace's last Go frame]. In every other case leave Beneath unset, or mark the panic unsplicable, so the chain fails. Keep the own-frame case working and add it as an arm; Go gives O2.func1 | gopanic | D2 | gopanic | O2 for `func D2(){ defer func(){}(); panic("p2") }` deferred by O2 while p1 runs. Also add the G shape above as an arm.

### S07 (blocker; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** A panic whose site has no Go frame (the nil-deferred-func thunk, or any panic thrown from golib code that a deferred call invokes directly) skips the owner check entirely. When it propagates one deferring frame up, the fault frames are spliced over a missing frame, although the stated residual says a panic caught by an intermediate frame splices nothing.

**Evidence.** managed_impl.cs:2074 puts the whole owner check inside 'if (site.Count > 0)'. A site with no Go frame falls through and splices gopanic plus its fault frames at ANY Run whose entry holds it. The thunk s_nilDeferredCall (GoFrame.cs:64) is golib code, and A's Run catch (GoFrame.cs:198) catches it first, so SiteTrace = [thunk, Run] and site.Count == 0. A has no other defers, so A's Run rethrows. B's catch then Captures P, B's Run pushes P (GoFrame.cs:182), and B's deferred call's Callers splices gopanic | panicmem | sigpanic directly above B. The approved design text covers only the owning frame ('its site is the Run frame, so it yields gopanic, panicmem, sigpanic') and says nothing about propagation.

**Go:**

```go
func A() { var f func(); defer f() }
func B() { defer func() { recover(); fmt.Println(names(1, 64)) }(); A() }
// Go 1.24.13: names | B.func1 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | A | B | main.func1 | main | runtime.main | runtime.goexit
// cut (traced): names | B.func1 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | B | ...   <- A missing
```

**Suggested fix.** A site-less site is valid only at the Run that caught it. When a panic is first caught at Run's catch, record the owning Run activation on it. The minimum is the deferring function's MethodBase from new StackFrame(1).GetMethod(), which is paid on the panic path only; better is the activation id in the next finding. Require that record at the splice, and fail otherwise. Add the propagated nil-func shape as an arm.

### S08 (concern; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** The owner check compares MethodBase, so a different activation of the same function satisfies it. With recursion or mutual recursion, a panic first caught by an inner activation splices at the outer activation's Run, and the inner activation (plus any frames between the two) silently disappears. By the letter of the brief this is a blocker. I rate it concern only because it follows the approved [P2-2]/[P2-4] identity rule, so it needs a ruling rather than a patch.

**Evidence.** managed_impl.cs:2076: site[^1].GetMethod() == owner. SiteTrace ends at F(0), the first catcher (PanicException.cs:270-273). F(0)'s Run rethrows, F(1) Captures, and F(1)'s Run pushes P. The owner is F(1)'s MethodBase, which equals F(0)'s, so F(0) is dropped as if it were the owner and gopanic is spliced above F(1). Concurrent goroutines are NOT a hazard: t_sequences is per thread, and the owner and the site both come from the same thread's panic object. Shared generic code (F[string] and F[object] share a canonical MethodBase) collides the same way.

**Go:**

```go
func F(n int) {
	defer func() { if n == 1 { recover(); fmt.Println(names(1, 64)) } }()
	if n == 0 { panic("r") }
	F(n - 1)
}
// F(1), Go 1.24.13: names | F.func1 | runtime.gopanic | F | F | main.func2 | ...
// cut (traced):     names | F.func1 | runtime.gopanic | F | main.func2 | ...  <- F(0) dropped
// mutual F2(1) -> M -> F2(0) panics, Go: F2.func1 | runtime.gopanic | F2 | M | F2 | main.func3 ...; cut: F2.func1 | runtime.gopanic | F2 | main.func3 ...
```

**Suggested fix.** Identify the activation, not the method. Keep a per-thread monotonically increasing id, stamped at PushSequence into a parallel array; this is a new [ThreadStatic], so it needs a census entry. Store the id on the panic at its first owning Run: at ClaimPanic for a panic the frame caught, at Run's catch for a panic the Run caught. At the splice, require every link's id to equal the id of the Run frame being walked. This one mechanism also closes the two blockers above and the next finding. Keep the MethodBase comparison as a secondary assertion.

### S09 (concern; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** A panic the Run itself caught, with no Beneath, never splices. That covers a deferred call that panics during a normal return, and one that panics after a recovery has completed: the `defer panic(v)` shape. The addition's own reasoning (such a site 'ends at Run, so the owner check alone would never splice it') applies here too, but only the Beneath case was handled, and this gap is not among R's stated residuals.

**Evidence.** GoFrame.cs:259 sets Beneath only while the entry is non-null. A normal-return sequence pushes null (GoFrame.cs:182), and a completed recovery nulls the entry (GoFrame.cs:195-196). The later splice finds site [NR.func2], which is not the owner NR, and no Beneath, so it returns false (managed_impl.cs:2076-2081). This is a missing splice, the safe direction.

**Go:**

```go
func NR() {
	defer func() { recover(); fmt.Println(names(1, 64)) }()
	defer func() { panic("d") }()
}
// Go 1.24.13: names | NR.func1 | runtime.gopanic | NR.func2 | NR | main | runtime.main | runtime.goexit
// cut (traced): names | NR.func1 | NR | main ...
// after a completed recovery (defer recover(); then defer panic("p3")): Go AR.func1 | runtime.gopanic | AR.func2 | AR ...; cut AR.func1 | AR ...
// with the defer in a loop (not open-coded), Go shows NO deferreturn here: NRL.func1 | runtime.gopanic | NRL.func2 | NRL
```

**Suggested fix.** With activation identity, a site the Run caught whose id equals the walked Run completes the splice and keeps all its Go frames. Failing that, state it as a residual in the PREDICTION and the design, and add an arm.

### S10 (concern; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** The chain bound admits 65 links (depth 0..64), not 64 as stated. At 66 or more links it drops the WHOLE splice rather than truncating from the top as Go does. The answer therefore jumps from Go's roughly 2n+9 frames to the live stack, even when the pc buffer is smaller than the first links would fill.

**Evidence.** managed_impl.cs:2056: `if (depth > 64) return false;` and depth starts at 0 (managed_impl.cs:2043). The recursion's false propagates up, and splicePanic returns []. Chain(n) builds n+1 links: p0 in the body, then each deferred panic's Beneath points at the one before (GoFrame.cs:259). So Chain(64) splices and Chain(65) splices nothing. No cycle is reachable today (see the info finding), so the bound's only live effect is this divergence.

**Go:**

```go
func Chain(n int) {
	defer func() { recover(); fmt.Println(names(1, 256)) }()
	for i := 0; i < n; i++ { defer func() { panic(i) }() }
	panic("p0")
}
// Go 1.24.13: Chain(64) -> 137 frames; Chain(65) -> 139 frames; Chain(70) with a 64-slot buffer -> 64 frames: names | Chain.func1 | runtime.gopanic | Chain.func2 | runtime.gopanic | ... all from the top
// cut (traced): Chain(64) spliced; Chain(65) and Chain(70) -> names | Chain.func1 | Chain | ... (live stack only)
```

**Suggested fix.** Validate the chain iteratively, checking ownership per link, with a visited set or a far larger guard as the cycle backstop. Then emit through the existing skip/capacity path, so a full buffer truncates from the top as Go's does. Alternatively state '65 links' as a residual and add arms at 65 and 66.

### S11 (concern; lens: PER-THREAD STATE, OWNERSHIP AND LEAKS; skeptics refuting: 0 of 2)

**Claim.** Missing arms for the state and ownership shapes. PanicFramesTests covers none of the shapes above. Two shapes the cut currently gets right are unguarded. The census reuse arm does not plant the two new thread-statics.

**Evidence.** PanicFramesTests.cs:42-132 has no arm for: a Beneath chain through a deferring callee (the G shape), a replacing panic raised by a deferred call that has its own frame (correct today), recover() then panic() in one deferred call (correct today: the entry still holds P1 because Pop()() threw before GoFrame.cs:195), a site-less panic propagated one frame up, or recursion. ThreadStateCensusTests.cs:66-67 classifies t_sequences and t_sequenceDepth by proof string only. Slots() and Dirty() (ThreadStateCensusTests.cs:300-332) never dirty or read them, so no test exercises the ResetSequences wiring through GoFuncRoot.ResetThread (GoFuncRoot.cs:70). No arm pins the pairing during a second-pass unwind either, for example a Callers from an outer deferred call while a Goexit unwinds, after t.FailNow in an inner deferred call. That pairing relies on the CLR stack walker hiding Run frames that are already unwound.

**Go:**

```go
// correct today, unguarded:
func RP() {
	defer func() { recover(); fmt.Println(names(1, 64)) }()
	defer func() { recover(); panic("p2") }()
	panic("p1")
}
// Go 1.24.13: names | RP.func1 | runtime.gopanic | RP.func2 | runtime.gopanic | RP | main ...
// own-frame replacement: names | O2.func1 | runtime.gopanic | D2 | runtime.gopanic | O2 | main ...
```

**Suggested fix.** Add red-first arms with the Go lists above: the G shape, D2, RP, the propagated nil func, and F/F2 recursion. The last three should expect 'no splice' until ownership is fixed. In ThreadStateCensusTests, extend Dirty() to push a stale entry and a nonzero depth through reflection, and Slots() to assert that depth is 0 and the array is clear on the reused thread.

### S12 (blocker; lens: COST ON THE HOT PATH; skeptics refuting: 0 of 2)

**Claim.** Correctness, outside the cost lens. The Beneath continuation runs on ANY owner mismatch, not only when the site ends at Run. So a replacing panic first caught by an intermediate deferring frame gets a WRONG splice: the deferred call's frame is missing. The cut claims the opposite: 'A failed owner check anywhere still splices nothing', and R's residual says such a panic 'splices nothing'.

**Evidence.** managed_impl.cs:2076-2081 @a419543c14: `if (owner is not null && site[^1].GetMethod() == owner) site.RemoveAt(..); else if (panic.Beneath is { } older) beneath = older; else return false;`. Nothing checks that SiteTrace's catching frame is GoFrame.Run. GoFrame.cs:259-260 sets Beneath for EVERY panic that reaches the owner Run's inner catch. That includes one that intermediate frame h caught first and h's Run re-raised; its SiteTrace (PanicException.cs:273, captured at h's filter) ends at h. Traced for shape A below: the cut emits `main.frames <- main.fA.func2 <- runtime.gopanic <- main.hA <- runtime.gopanic <- main.fA <- main.main`. main.dA is missing because the deferred call that called h is in neither panic's SiteTrace. The base reports only the live stack, a divergence it already has; the cut turns that into the silent lie that [P2-2] ruled worse. No arm covers this shape. ownerMismatch (PanicFramesProbe.cs:251) has no Beneath. replacedPanic (:214) has a site that really ends at Run. Shapes B and C below come out right in the cut.

**Go:**

```go
func hA() { defer func() {}(); panic("p1") }
func dA() { hA() }
func fA() {
	defer func() { recover() }()
	defer func() { fmt.Println("A:", frames()) }() // frames(): runtime.Callers(1,..) + CallersFrames, joined
	defer dA()
	panic("p0")
}
// B: dB(){ defer func(){}(); panic("p1") }   C: dC(){ panic("p1") }
// go1.24.13 windows/amd64, with -gcflags=all=-l and without:
A: main.frames <- main.fA.func2 <- runtime.gopanic <- main.hA <- main.dA <- runtime.gopanic <- main.fA <- main.main
B: main.frames <- main.fB.func2 <- runtime.gopanic <- main.dB <- runtime.gopanic <- main.fB <- main.main
C: main.frames <- main.fC.func2 <- runtime.gopanic <- main.dC <- runtime.gopanic <- main.fC <- main.main
```

**Suggested fix.** Continue down Beneath only when the panic was verifiably first caught AT Run. Two options: (a) require the raw SiteTrace's last frame to be GoFrame.RunMethod; or (b) cost-free, stamp the fact in Run's inner catch, i.e. the panic never went through an emitted GoFrame.Capture. Any other mismatch returns false, which splices nothing. Shape B then becomes a stated residual instead of a match, which is the conservative choice. Add a shape-A arm that expects the owner only.

### S13 (concern; lens: COST ON THE HOT PATH; skeptics refuting: 0 of 2)

**Claim.** Correctness, outside the cost lens. The owner check compares MethodBase, so in recursion it matches a DIFFERENT activation of the same function. A panic first caught by an intermediate activation is then spliced wrongly (one frame missing), not left unspliced.

**Evidence.** managed_impl.cs:2036-2040 takes the owner as the next live Go frame's MethodBase, and :2076 drops site[^1] when it is the same method. For r(1) -> r(0), where r(0) defers and panics: r(0)'s own catch captures SiteTrace = [r], and r(1)'s Run claims the re-raised panic. Traced: [r] ends in the owner's MethodBase, so it is dropped and only gopanic is emitted. Cut output by reading: `main.frames <- main.r.func2 <- runtime.gopanic <- main.r <- main.main`. Go shows two main.r frames. This comes from [P2-2]'s identity rule, not from a coding slip, but it breaks the same 'never a wrong splice' contract.

**Go:**

```go
func r(n int) {
	if n == 0 { defer func() {}(); panic("p") }
	defer func() { fmt.Println(frames()); recover() }()
	r(n - 1)
}
func main() { r(1) }
// go1.24.13, -gcflags=all=-l:
main.frames <- main.r.func2 <- runtime.gopanic <- main.r <- main.r <- main.main
```

**Suggested fix.** Check who caught the panic first by a stamp, not by method identity. GoFrame.Capture increments a per-panic capture count. Run marks its entry owner-verified only when the panic it claimed has a count of 1, meaning this frame is the first catcher and SiteTrace ends here. This is one int per panic and adds nothing on the no-panic path. The same stamp (count 0 at Run's inner catch) is the 'site ends at Run' test the blocker above needs.

### S14 (concern; lens: COST ON THE HOT PATH; skeptics refuting: 0 of 2)

**Claim.** No-panic hot path, as measured by reading: no allocation (after the first on each thread), no stack capture, no dictionary operation and no new EH region. The added cost is 3 [ThreadStatic] reads, 2 [ThreadStatic] writes, one covariant array-store helper call and 2 bounds checks per deferring return, plus one local null test per deferred call. It adds to the base Run's 6 ThreadLocal<T>.Value accesses rather than replacing any of them. The approved design names a replacement the cut did not take. The design gate's PerfDefer before/after has not been run.

**Evidence.** GoFrame.cs:182 PushSequence (:309-324: t_sequenceDepth read, t_sequences read with `??=`, Length compare, `entries[index] = panic`, depth write). :195-196 run per deferred call (`handling is { Recovered: true }`; handling is null when nothing panics). :273 PopSequence (:328-332: t_sequences read, a constant-null store, depth write). Per panic: CaptureThrowSite (PanicException.cs:268-275) still takes ONE `new StackTrace(thrown, true)`, which SiteTrace and PanicTrace now share. So the capture count per panic is unchanged, and BenchmarkPanicRecover should not move. The allocation pin GoFrameTests.TheFrameItselfAllocatesNothing (GoFrameTests.cs:470-504) measures this exact path. By reading it stays 0 B: the one 152-B array per thread falls inside its 200-call warm-up. The base's no-panic accesses are ThreadLocal<T> (GoFuncRoot.cs:20-56), at GoFrame.cs:157 (ClaimPanic), 166, 168, 174, 274 and 276. Design §3.B.1: 'Seat (iii)'s RecoverablePanic is the innermost entry's panic, so the stack can replace that slot.' The cut keeps both. Design §5 owes 'seat (iii)'s PerfDefer benchmarks ... before and after'. PREDICTION.md item 3 calls it 'stated, not scored', and `git grep BenchmarkDefer|PerfDefer` over every r-/p2-/trainG branch finds no numbers. Expected delta, estimated by reading (inlined [ThreadStatic] access is about 1 ns, a shared-generic ThreadLocal<T>.Value about 3-5 ns): BenchmarkDefer +3 to 7 ns/op, which is +25-35% on Run's own TLS overhead and perhaps +5-10% of the op. Defer10 about +0.5 ns/op, because push/pop is amortized over 10 calls. DeferMany about 0, since it runs one Run in total. PanicRecover within noise, since exception dispatch plus StackTrace(true) dominate.

**Go:**

```go
Context only; this is not a behavioural claim. go1.24.13 on the i7 while the tG battery was loading it, so the numbers are inflated:
func defer1() { defer func(x, y, z int) { if recover() != nil || x != 1 || y != 2 || z != 3 { panic("bad recover") } }(1, 2, 3) }
func locked() { mu.Lock(); defer mu.Unlock(); n++ }
// testing.Benchmark output:
go BenchmarkDefer shape: 18.55 ns/op, 0 allocs/op
go lock+defer unlock:    21.71 ns/op, 0 allocs/op
```

**Suggested fix.** (1) Take the design's offset. recover() and Run read the innermost entry (SequenceFromTop(0), with the Recovered check) instead of GoFuncRoot.RecoverablePanic. That drops 2 ThreadLocal<T> accesses per deferring Run and 1 per recover(), so the cut should come out net-neutral or better on BenchmarkDefer. The semantics match: set at sequence start, updated on replacement, restored on exit. The entry being nulled after a completed recovery reads the same as Recovered==true. (2) Then run runtime's BenchmarkDefer / Defer10 / DeferMany / PanicRecover on base e6fc210500 and on the cut, on an idle box, and record ns/op with the box and tree named, as §5 requires.

### S15 (blocker; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 0 of 2)

**Claim.** The Beneath chain stops at a link whose site has no Go frame. A nil deferred func (or any golib-raised panic directly under Run) raised while another panic is running is spliced without the older panic's runtime.gopanic. The answer is wrong, not just missing, in the very shape the addition claims: a panic raised by a deferred call with no frame of its own.

**Evidence.** GoFrame.cs:259-260 sets raised.Beneath = running for every deferred-call panic. But managed_impl.cs:2072-2081 only reads panic.Beneath inside `if (site.Count > 0)`. The nil thunk s_nilDeferredCall (GoFrame.cs:64) lives on GoFrame, not a *_package class, so its site filters to zero Go frames. `beneath` stays null, and line 2102 completes the splice after gopanic, panicmem, sigpanic. No arm covers Beneath together with an empty site.

**Go:**

```go
func nilDeferWhilePanicking() {
	defer func() { recover() }()
	defer func() { dump() }() // runtime.Callers here
	var f func()
	defer f()
	panic("p1")
}
// Go (windows/amd64 go1.24.13): nilDeferWhilePanicking.func2 | runtime.gopanic | runtime.panicmem | runtime.sigpanic(signal_windows.go:401) | runtime.gopanic | nilDeferWhilePanicking
// cut (by trace):              nilDeferWhilePanicking.func2 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | nilDeferWhilePanicking
```

**Suggested fix.** When site.Count == 0 and panic.Beneath is set, follow Beneath. Do that only after the link is verified to belong to this Run (see the next finding's activation check). Add an arm for this exact shape.

### S16 (blocker; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 0 of 2)

**Claim.** The owner check never runs on the chain's non-final links. Suppose a deferred call's callee has its own GoFrame, for example any `defer mu.Unlock()`. A panic raised there is first caught by that intermediate frame, so its site ends there and not at the owner's Run. Run still records Beneath and the splice follows it, silently dropping every frame between the intermediate catcher and the Run. This falsifies 'A failed owner check anywhere still splices nothing' and residual (i).

**Evidence.** GoFrame.cs:259-260 sets Beneath whenever the sequence's entry is a different panic, without checking where the raised panic's SiteTrace ends. managed_impl.cs:2076-2079 treats any site whose last Go frame is not the owner as a link that ends at Run, whenever Beneath is non-null. SiteTrace is captured at the first IsPanic filter (PanicException.CaptureThrowSite via TryAsPanic), which is the intermediate frame's catch. No arm covers this.

**Go:**

```go
//go:noinline
func h() { defer func() {}(); panic("p2") }
func A() {
	defer func() { recover() }()
	defer func() { dump() }()
	defer func() { h() }()
	panic("p1")
}
// Go:  A.func2 | runtime.gopanic | h | A.func3 | runtime.gopanic | A
// cut: A.func2 | runtime.gopanic | h | runtime.gopanic | A        (A.func3 dropped)
//go:noinline
func k() { defer func() { panic("p2") }() }
// same shape with defer func(){ k() }():
// Go:  B.func2 | gopanic | k.func1 | k | B.func3 | gopanic | B
// cut: B.func2 | gopanic | k.func1 | gopanic | B
```

**Suggested fix.** Check each link by activation. When a panic is first claimed, record which sequence owns its site. Capture(p) sets p.SiteOwnerIndex ??= t_sequenceDepth, which is the index the frame's Run will push. Run's catch sets raised.SiteOwnerIndex ??= sequence and marks the site as ending at Run. In splicePanicInto, require SiteOwnerIndex == the paired Run's index on every link, otherwise fail the whole splice. Add both shapes as arms.

### S17 (blocker; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 1 of 2)

**Claim.** The owner check compares MethodBase identity, so a recursive activation of the deferring function passes it. The splice then drops a live Go frame that is not the owner. This contradicts residual (i), which says a panic first caught by an intermediate deferring frame 'fails the owner check and splices nothing'.

**Evidence.** managed_impl.cs:2076 checks `site[^1].GetMethod() == owner`. For rec(1)'s Run the owner is rec, and the site, first caught by rec(0), also ends at rec, so rec(0) is dropped. The approved [P2-2] text itself says 'MethodBase equals the next live Go frame', so the flaw is in the approved rule and the cut implements it faithfully. The same false match lets a chain stop early when a deferred call recurses into the owner method. No recursion arm exists.

**Go:**

```go
//go:noinline
func rec(n int) {
	defer func() { if n == 1 { recover(); dump() } }()
	if n == 0 { panic("x") }
	rec(n - 1)
}
// rec(1). Go:  rec.func1 | runtime.gopanic | rec (panic line) | rec (recursive-call line) | ...
// cut:         rec.func1 | runtime.gopanic | rec (recursive-call line) | ...
```

**Suggested fix.** Use the same activation identity (sequence index) as the previous finding instead of MethodBase for the owner test. Add a recursion arm. This needs a COORD/P2 ruling because it amends the approved [P2-2] wording.

### S18 (concern; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 0 of 2)

**Claim.** A panic raised by a deferred call with no Beneath is not spliced at all: either in a normal-return sequence, or after an earlier recovery in the same sequence completed. Go shows gopanic and the raising deferred call. The addition's own premise, 'its site ends at Run', holds here too, but only the Beneath case was taught to continue. The answer is missing, not wrong.

**Evidence.** In GoFrame.cs:259 `running` is null, because the entry is null on a normal return, or was nulled at :195-196 after a completed recovery, so no Beneath is set. managed_impl.cs:2080-2081 then returns false: the site [func3] is not the owner and Beneath is null. No arm covers either shape.

**Go:**

```go
func N() {
	defer func() { recover() }()
	defer func() { dump() }()
	defer func() { panic("p") }()
}
// Go: N.func2 | runtime.gopanic | N.func3 | N      cut: N.func2 | N
func R() {
	defer func() { recover() }()
	defer func() { dump() }()
	defer func() { panic("p2") }()
	defer func() { recover() }()
	panic("p1")
}
// Go: R.func2 | runtime.gopanic | R.func3 | R      cut: R.func2 | R
```

**Suggested fix.** With the per-link marker that a site ends at this Run, complete the splice with the site kept when Beneath is null. Add arms for both shapes.

### S19 (concern; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 1 of 2)

**Claim.** Any runtime error other than a nil dereference or an integer divide now gets a WRONG splice rather than none. Go shows a runtime frame between gopanic and the site; the cut splices gopanic directly over the site. Design §3.B.3 says 'named, NOT claimed', but the cut neither refuses these nor lists them among R's residuals.

**Evidence.** FaultKind is set only in RuntimeErrorPanic.NilPointerDereference and IntegerDivideByZero (RuntimeErrorPanic.cs:23,171). IndexOutOfRange (RuntimeErrorPanic.cs:116-118) and the other factories leave FaultKind at None, so managed_impl.cs:2084-2100 emits gopanic and then the site. Before the cut the answer was missing; after it the answer is off by one frame. No arm covers this.

**Go:**

```go
func indexPanic(i int) { defer func() { recover(); dump() }(); s := []int{1}; _ = s[i] } // i=5
// Go: indexPanic.func1 | runtime.gopanic | runtime.goPanicIndex(panic.go:115) | indexPanic
// cut: indexPanic.func1 | runtime.gopanic | indexPanic
// Go also measured: nil map write -> runtime.mapassign_faststr(runtime_faststr_swiss.go:265); close of closed chan -> runtime.closechan(chan.go:422); failed x.(int) -> runtime.panicdottypeE(iface.go:275)
```

**Suggested fix.** Either give golib runtime-error panics of an unmodelled kind a marker and splice nothing for them (a missing splice, per [P2-2]), or add the kinds: goPanicIndex, the goPanicSlice family, panicdottypeE/I, closechan, mapassign. Name whichever is chosen in the residuals and add one arm.

### S20 (concern; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 0 of 2)

**Claim.** Several claimed shapes have no arm. On the Beneath chain: no arm for repanic depth > 1, for the 64 bound, or for a cycle. [P2-3]'s skip and capacity path for spliced frames has no arm. No arm asserts a spliced frame's line, although the predicted TestCallersPanic reads those lines.

**Evidence.** PanicFramesTests.cs:93-104 (AReplacingPanic…) covers depth 1 only. Every arm calls Callers(0) with a 64-slot buffer (PanicFramesProbe.cs:19-20). I traced managed_impl.cs:2052-2102: depth 2+ gives Go's answer (see below). The bound at :2056 makes a legitimate chain of 66+ replacing defers splice nothing, which is missing rather than wrong. `!ReferenceEquals` at GoFrame.cs:259 prevents a self-cycle; a two-cycle needs an already-caught instance to be raised again, which emitted code cannot do as far as I can see. The runtime row's TestCallersPanic also calls testCallers, which requires f1:15, f2:19, f3:24 from the spliced frames (callers_test.cs, testCallers). callers() (managed_impl.cs:1877-1880) makes runtime.Caller splice too; the prediction does not mention it.

**Go:**

```go
func T() {
	defer func() { recover() }()
	defer func() { dump() }()
	defer func() { panic("p3") }()
	defer func() { panic("p2") }()
	panic("p1")
}
// Go: T.func2 | gopanic | T.func3 | gopanic | T.func4 | gopanic | T
// skip path: inside a panic's deferred call, runtime.Caller(1) -> runtime.gopanic (panic.go:792)
```

**Suggested fix.** Add these arms: a two-link chain with exact names (func3 over func4); runtime.Caller(1) and a Callers with skip >= 2 plus a 3-slot buffer inside a panic's deferred call; and a line assertion on the spliced site frames (the throw line and the call lines). Optionally add a bound arm that asserts no splice beyond 65 links.

### S21 (concern; lens: TEST ARMS AND PREDICTION VERSUS CLAIMS; skeptics refuting: 0 of 2)

**Claim.** The prediction contradicts itself on parent verdicts. Line 19 says 'Every other verdict: 0 moved'. Lines 21-22 say TestStackWrapperStackPanic moves if its /Stack subtests pass, and that those are already in the base. The intermediate parents TestStackWrapperStackPanic/sigpanic and /panicwrap are not named at all. Scored as written, the prediction fails as soon as the parents move.

**Evidence.** PREDICTION.md:9-22. The comparison records parents and intermediate subtests as separate verdicts (comparison-classifier/testdata/runtime-panic/go2cs_test_comparison.json keys TestStackWrapperStackPanic, …/sigpanic, …/panicwrap, …/sigpanic/CallersFrames). If the /Stack subtests pass at base, which the text implies (P2's F, renderStack appending PanicTrace), 10 verdicts move, not 7.

**Go:**

```go
n/a (prediction text). Go's TestStackWrapperStackPanic runs t.Run("sigpanic") and t.Run("panicwrap"), each with its own "CallersFrames" and "Stack" subtests.
```

**Suggested fix.** Read the /Stack verdicts from the base's record and restate the prediction as an exact count (7, or 10 including the three parents) before the gate runs.


---

## ADDENDUM 2026-09-27 (late): verification of the re-cut e266debe65

R re-cut B to e266debe65: 8e2dd0fa8a RED (17 arms from this review's Go lists; 10 red at a419543c14), 2ff712b522 FIX (activation identity; SiteOwner stamped at the first catch; every link must match or the whole splice is refused), and e266debe65, which restates the prediction (exactly 10 rows move). COORD verified it with one verifier per defect plus a regression finder, and a skeptic on each item not judged closed; 16 agents, go1.24.13 ground truth, and standalone net10 repros where the JIT mattered.

**VERDICT: better, still NOT ACCEPTED.** Shape B, the two-link chain, `defer D()`-as-catcher and the MethodBase recursion case are fixed. What remains:

| Item | Verifier | Skeptic | What remains |
|---|---|---|---|
| B1 | partial | partial | 1. The one accepted re-raise must stop inferring "no Go frame between" from missing frames. Run should keep the delegate it popped and accept the re-raise only when `delegate.Method` equals the catcher (site[^1].GetMethod()). For a golib ladder closure, it should compare the wrapped delegate's Method instead. If that is not done, drop the accepted re-raise entirely, so `defer D()` becomes a stated missing splice. 2. The arms must exercise the tail-call path in any tiering: - Mark the shapeA probe closure [MethodImpl(AggressiveOptimization)]. It is verified to lose its frame on its first call. - Add a forwarder arm whose forwarder is unmarked or AggressiveOptimization, never NoInlining. - Or gate PanicFramesReviewTests at Release+TC0 and say so on the gate line. As it stands, ShapeA should read RED at Release+TC0. 3. (Not B1.) t_lastActivation is per thread and starts at 0 on every thread |
| B2 | partial | partial | What is still open in B2's scope: - **Nil func with arguments (NA, NA2, NAR).** The golib `defer<T...>` closure throws the NullReferenceException with no Go frame in the site. The splice accepts it and drops Go's deferwrap1 frame in both the while-panicking form and the normal-return form. - **Suggested fix for that:** accept a site-less Memory link only when raw[0] is GoFrame's s_nilDeferredCall thunk, and refuse every other site-less link. Add red-first arms for NA and NAR that expect no splice. A related shape, outside the literal "site has no Go frame" wording: - **Nil named-func-type call with no args (NC).** The converter's `() => c()` lambda shows up as a spliced Go frame that Go does not have. It is wrong while panicking at both cuts. The normal-return form is newly wrong in the re-cut. - **Fix for that:** refuse, or have the converter stop wrapping the call, plus an arm. Neither |
| C1 | partial | partial | One activation number can stand for two threads. When range-over-func hands the same PanicException from the coro's pooled worker to the ranging goroutine, and the two per-thread counters happen to be equal, splicePanic accepts a site owned on the other thread. The two counters are deterministically both 1 for a fresh goroutine and the process's first coro worker. The result is a wrong splice: - Shape X: missing seqX. - Shape Y, recursion across the boundary: missing F(0) and F.func2. Any one of these fixes it: - (a) Store a per-thread identity token beside SiteOwner and compare both in splicePanic. This costs only on the panic path. - (b) Issue activation numbers from one counter for the whole process, with an interlocked increment. - (c) In YieldFunctionEnumerator.run's catch, clear SiteOwner on a PanicException before it crosses, keeping Catches at 1 or more so nothing re-stamps it. A |
| C2 | partial | open | 1) The fix must not infer the thrower from the stack. Tag unmodelled runtime errors where they are raised. For example: a PanicFaultKind.Unmodelled, or a flag on PanicException, set in: - RuntimeErrorPanic.IndexOutOfRange, the SliceBounds family and the other non-modelled factories; - golib's `new PanicException(...)` sites for nil-map write, closed or nil channel close and send, and interface conversion. splicePanic then refuses on the tag. The stack test can stay as a second guard. This costs nothing on the hot path, because it runs only on the throw path. 2) Make the arm independent of JIT tier: - mark probe indexPanic [MethodImpl(MethodImplOptions.AggressiveOptimization)], or run the arm under DOTNET_TieredCompilation=0; - add array and string index arms; - add a nil-map-write arm, a double-close arm and a failed `_<T>` arm, each expecting no splice. With the current code, a full-opt |
| C3 | partial | partial | 1. **The test host's Goexit.** t.Fatal, t.FailNow, t.Skip and t.SkipNow on the test's own thread throw TestAbortException, so GoexitException.Started is never set. A panic from a deferred call during that unwind then gets a partial splice, `X.func1 \| gopanic \| X.func2 \| X`, where Go shows `... \| runtime.Goexit \| testing.(*common).FailNow \| [Fatal] \| X`. This contradicts the stated Goexit residual ("splices NOTHING, never a partial list"), and it was a missing splice at a419543c14. Two ways to fix it: - Set the golib Goexit mark in TestExecution.FailNow and SkipNow before they throw TestAbortException, for example with an internal GoexitException.Mark(). - Or have Run refuse whenever it runs inside a non-panic unwind. Either way, add an arm with Go's list above that expects NO splice for t.Fatal and for SkipNow. 2. **`defer panic(v)`.** The converter thunk is spliced as `P.funcN`,  |
| C4 | partial | partial | 1. **Add a bound arm** in GolibTests. It needs only golib, not Go, and can be modeled on twoLinkChain. - The probe defers a recover-then-callersHere closure first, then n >= 66 loop closures that each throw panic(i), then throws p0. - Assert that below the recovering closure come exactly 2n+1 spliced frames, (gopanic, closure) x n then gopanic, followed by the owner. - Assert that a 64-slot buffer returns 64 frames, all from the top. - Go 1.24.13 confirms the shape: 2n+1 spliced frames, and 64 frames from the top with a 64-slot buffer. - This arm would red on a reintroduced `depth > 64` or `visited.Count > 64`. 2. **Optionally, add a cycle arm.** Set p1.Beneath = p2 and p2.Beneath = p1 through the internal setter, then assert there is no splice and no hang. A cycle can't be reached from emitted code, so this only guards the visited-set backstop. 3. **A design note to state, not a defect. |
| C5 and C6 | partial | partial | 1. **Add the bound arm, the one real hole.** Build a probe with 70 `defer(() => throw panic(i))` in a loop over panic("p0"), and read it from an outer deferred call through a 64-slot buffer. Assert 64 frames: runtime.Callers, callersHere, the reader, then alternating runtime.gopanic and the raiser's name all the way to the end. Also assert that Chain(65) through a 256-slot buffer ends at the owner. It is red at a419543c14 and green at the fix. 2. **Name or add the lesser gaps.** - A cycle arm. It is unreachable from emitted code, since Beneath is set only at a first catch or the accepted re-raise. It could be planted by reflection to guard the visited set, or waived by name. - An F2 -> M -> F2 mutual-recursion arm, which S11's suggested fix asks for. It is red at the first cut, but the rec arm already guards the same activation mechanism. - S11's second-pass pairing: a Callers from an ou |
| REGRESSION FINDER | open | open | Fixes owed, smallest first: (a) Make activation identity safe across threads. Options: - draw activation numbers from per-thread blocks reserved from a static Interlocked counter, or stamp the owning thread as well and require it to match in splicePanic; - in YieldFunctionEnumerator.rethrowFailure, mark t_started on the ranging thread when the rethrown exception is a GoexitException; - or clear SiteOwner/SiteEndsAtRun on any cross-thread rethrow. (b) Either let ReRaisedByTheDeferredDelegate accept a first catcher whose Run registered no defer (m_count 0: an unreached conditional defer, or all defers lowered into the finally), or state M0 and the all-lowered catchers as residuals. (c) Fix the t_lastActivation census rationale: numbers are unique only within one thread. Arms owed, each shown red first: - GX (deterministic): a range-over-func seq calls Goexit and the ranging goroutine's def |

**The load-bearing one (B1): the accepted re-raise infers "no Go frame between" from ABSENT frames in `new StackTrace(ex)`, and RyuJIT's implicit tail calls remove exactly those frames.** Both B1 agents reproduced it in standalone net10 programs. At DOTNET_TieredCompilation=0, the -tests pipeline's configuration of record, an unmarked closure `() => { G(); }` and an unmarked forwarder `D() => G()` leave the same trace as `defer G`. So shape A is accepted and splices B1's original wrong list. The converter marks NoInlining only on Callers users and their forwarders. The ShapeA arm is green only because GolibTests do not run at TC0 and the forwarder arm's forwarder is NoInlining. FIX: accept the re-raise only when the popped delegate's Method equals the catcher, or drop the acceptance, making `defer D()` a stated missing splice. Make the arms exercise tail calls (AggressiveOptimization on the probe closures, or run them at TC0).

**Also owed:** activation identity must be safe across threads, since range-over-func hands a PanicException between a coro worker and the ranging goroutine whose per-thread counters can both be 1 (use per-thread blocks from a static Interlocked counter, or stamp the thread). Tag unmodelled runtime errors where they are raised, not by stack inference. Mark the test host's Goexit path (TestAbortException). Handle a nil deferred func WITH arguments (the golib `defer<T...>` closure). Add the bound arm (70 raisers through a 64-slot buffer).

## Appendix B: the verification results

### B1: verifier partial

**Evidence.** WHAT IS FIXED (read at e266debe65; the three Go lists below are confirmed on go1.24.13 in <scratch>):
- The old mechanism is gone. GoFrame.Run's catch now sets SiteOwner, SiteEndsAtRun and Beneath only when this activation owns the new panic, in one of two cases: firstHere (SiteOwner 0 and Catches 1), or ReRaisedByTheDeferredDelegate.
- splicePanic (managed_impl.cs) requires link.SiteOwner == the walked Run's activation on EVERY link. It follows Beneath only from a SiteEndsAtRun link, and any failure returns [] for the whole splice.
- Shape B is refused in every JIT configuration. gNormalDeferPanics' Run stamps p2 {SiteOwner = its own activation, SiteEndsAtRun = true}. At shapeB's Run, firstHere is false (SiteOwner is not 0) and ReRaised returns false on SiteEndsAtRun, so there is no splice. Go: B.func1 | gopanic | gNormalDeferPanics.func1 | gNormalDeferPanics | B.func2 | gopanic | B.
- defer gDeferPanics() still splices Go's list. At the owner's Run, Catches == 2, and the re-raised trace is Run | gDeferPanics | Run. The panic is owned with Beneath = p1, giving gopanic | gDeferPanics | gopanic | owner, which matches Go (D2 above).
- The arms: ShapeA, ShapeB, deferACallerOfTheCatcher (expect no splice) and deferTheCatcher (expects Go's list).

WHAT IS STILL WRONG: ReRaisedByTheDeferredDelegate decides that "the deferred delegate IS the first catcher" from the ABSENCE of any other non-machinery frame in the re-raised exception's CLR trace. RyuJIT's implicit tail calls remove exactly those frames when the code is optimized and the caller is not NoInlining. Two shapes hit this:
- Shape A's closure `() => { gDeferPanics(); }`. builtin.defer(Action) pushes it directly, so no golib frame sits in between.
- A thin forwarder `func D(){ G() }`. The converter marks NoInlining only on Callers users and on forwarders to them (callerInliningAnalysis.go / litNoInliningPrefix).

Measured in a standalone net10 repro, NOT the suites. It lives in my scratchpad as tailrepro\Program.cs. Its shape is a first-catching frame whose finally re-raises, and a caller whose catch reads new StackTrace(ex):
- At DOTNET_TieredCompilation=0, the closure, the forwarder D and the direct delegate G all give the SAME trace: `Rerun | G | RunOne`. A NoInlining closure keeps its frame.
- Under default tiering the closure and D frames vanish after tier-up (about iteration 200 to 250). They vanish at iteration 0 if the lambda carries [MethodImpl(AggressiveOptimization)].
- So at Release+TC0, which the validation configuration of record, the re-cut ACCEPTS shape A as the one allowed re-raise. It splices gopanic | gDeferPanics | gopanic | shapeA. That is B1's exact wrong list: Go has shapeA.func2 between gDeferPanics and the second gopanic.
- The G shape with an unmarked forwarder splices gopanic | G | gopanic | O. Go: gopanic | G | D | gopanic | O.
- The same happens in any hot program after tier-up.

WHY THE ARMS MISS IT:
- The ShapeA arm's closure is unmarked, so it goes RED only when GolibTests runs at TC0 or after tier-up. A single default-tiering call runs at tier 0 and is green. R's "80/80 Release" does not state TC0, and by reading ShapeA should be red at Release+TC0.
- deferACallerOfTheCatcher uses dCallsG, which is marked NoInlining. The VM never tail-calls from a NoInlining caller, so that arm is green in every configuration and cannot see the real emission, where the forwarder is unmarked.

**Residual.** 1. The one accepted re-raise must stop inferring "no Go frame between" from missing frames. Suggested fix: in Run, keep the popped delegate. Accept the re-raise only when its Method equals the catcher, site[^1].GetMethod(). For a golib arity-ladder closure, use its inner delegate's Method. Otherwise drop the re-raise acceptance entirely: `defer D()` would become a stated missing splice, never a wrong one.

2. The arms need to exercise the tail-call path in any tiering:
- Mark the shapeA closure [MethodImpl(AggressiveOptimization)]. The repro shows it is tail-called on its first call.
- Add a forwarder arm with an unmarked forwarder, also AggressiveOptimization, not NoInlining.
- Or run PanicFramesReviewTests at Release+TC0 and state that configuration on the gate line.

3. Not B1, noted only: t_lastActivation is per-thread and starts at 0 on every thread. A PanicException re-raised on another thread could therefore match a foreign activation number. The by-method check still guards the site-ends-at-function case.

No repo or tG changes were made. The repro's bin and obj folders were purged. The Go program is at <scratch>

**Skeptic: partial.** Everything below was read at e266debe65, after a fetch. Go's lists were checked again on go1.24.13 in <scratch>
- A: shapeA.func1 | gopanic | gDeferPanics | shapeA.func2 | gopanic | shapeA
- B: shapeB.func1 | gopanic | gNormalDeferPanics.func1 | gNormalDeferPanics | shapeB.func2 | gopanic | shapeB
- D2: gopanic | gDeferPanics | gopanic | deferTheCatcher
- G: gopanic | gDeferPanics | dCallsG | gopanic | owner

FIXED:
- The old path, which followed Beneath whenever the owner check failed, is gone. splicePanic (managed_impl.cs) now requires `link.SiteOwner == activation` on every link. It follows Beneath only from a SiteEndsAtRun link and returns [] for the whole splice on any failure.
- Shape B is refused in every JIT configuration. At gNormalDeferPanics' Run catch, firstHere holds (Catches 1, SiteOwner 0), so p2 gets {SiteOwner = g's activation, SiteEndsAtRun = true}. At shapeB's Run, firstHere is false and ReRaisedByTheDeferredDelegate returns false on SiteEndsAtRun. The splice then fails because SiteOwner is not the walked activation.
- D2 (`defer gDeferPanics()`) still gives Go's list. Catches == 2, the re-raise trace holds only Run and gDeferPanics frames, so p2 is owned with Beneath = p1. The splice is gopanic | gDeferPanics | gopanic, then the owner is dropped.

STILL WRONG:
- ReRaisedByTheDeferredDelegate (GoFrame.cs) decides the delegate IS the first catcher from the ABSENCE of any other non-machinery frame in `new StackTrace(ex)`. RyuJIT's implicit tail calls remove exactly that frame.
- I checked this myself with a standalone net10 Release repro in my scratchpad (tc\Program.cs, bin/obj purged). It is a structural copy of Run's re-raise path: G defers and panics, its finally's NoInlining Run does `throw owned`, and the owner's Run catch prints the non-Run frames.
- At DOTNET_TieredCompilation=0, the trace reads `Pkg.G` for all three of: the unmarked closure `() => { G(); }`, the unmarked forwarder `D() => G()`, and `defer G` itself. They are identical. The closure or D survives only when it is NoInlining.
- At default tiering on the first call: `G | G | <>c.<ShapeA>b__4_1` and `G | G | D`. The frame is present, so the shape is refused.
- An AggressiveOptimization closure loses its frame on the first call, even under default tiering.

WHY IT MATTERS:
- The -tests pipeline runs Release with DOTNET_TieredCompilation=0 by default (testConversion.go:6550, main.go -test-tiered).
- The converter marks a closure or forwarder NoInlining only when it reaches runtime.Caller/Callers (litNoInliningPrefix, computeNoInliningClosure). Real `defer func(){ gDeferPanics() }()` is therefore unmarked.
- At TC0, all of ReRaised's gates pass for shape A: Catches == 2, SiteOwner set by gDeferPanics' Run, not SiteEndsAtRun, and the catcher is gDeferPanics. So p2 gets {SiteOwner = owner activation, SiteEndsAtRun, Beneath = p1}.
- The splice then emits gopanic | gDeferPanics | gopanic | shapeA. That is B1's exact wrong list, with shapeA.func2 missing.
- The unmarked `defer D()` gives gopanic | G | gopanic | O, where Go has D between G and the second gopanic.

WHY THE ARMS MISS IT:
- The ShapeA probe's closure is unmarked, and every arm calls it once. Under default tiering that call runs at tier 0, so the arm is green. It would go red only at TC0, and PREDICTION.md states GolibTests as "Release and Debug", with no TC0.
- deferACallerOfTheCatcher uses dCallsG, which is NoInlining. The VM never tail-calls from a NoInlining caller, so that arm is green in every configuration and never sees the real, unmarked emission.

**Skeptic residual.** 1. The one accepted re-raise must stop inferring "no Go frame between" from missing frames. Run should keep the delegate it popped and accept the re-raise only when `delegate.Method` equals the catcher (site[^1].GetMethod()). For a golib ladder closure, it should compare the wrapped delegate's Method instead. If that is not done, drop the accepted re-raise entirely, so `defer D()` becomes a stated missing splice.
2. The arms must exercise the tail-call path in any tiering:
   - Mark the shapeA probe closure [MethodImpl(AggressiveOptimization)]. It is verified to lose its frame on its first call.
   - Add a forwarder arm whose forwarder is unmarked or AggressiveOptimization, never NoInlining.
   - Or gate PanicFramesReviewTests at Release+TC0 and say so on the gate line.

As it stands, ShapeA should read RED at Release+TC0.
3. (Not B1.) t_lastActivation is per thread and starts at 0 on every thread, so a panic re-raised on another thread can match a foreign activation number.

### B2: verifier partial

**Evidence.** This is from reading e266debe65 against Go 1.24.13 output. I ran `go run` both plain and with `-gcflags=all=-l`, and the lists were the same. The programs are <scratch> and ...\rpf2\b2w\main.go.

**What is fixed (the named shapes):**
- In the re-cut, splicePanic walks the whole chain with a loop: `for (link = panic; link != null; link = link.Beneath)`, in managed_impl.cs from about line 2055. The site-less exemption at the old line 2074 (`if (site.Count > 0)`) is gone. Every link must have `SiteOwner == activation`.
- A site-less link has `SiteEndsAtRun = true`. It adds gopanic plus its fault frames, then moves on down Beneath.
- Run's inner catch (GoFrame.cs, the firstHere block) stamps `SiteOwner`, `SiteEndsAtRun` and `Beneath = running`. It does this only when `Catches == 1`, meaning this catch is the panic's first.

**S02/S15 (N shape) traced:**
- The body throws p1. N's own catch is the first catch (Catches=1), so Run stamps `p1.SiteOwner = A1`.
- The nil thunk `s_nilDeferredCall` throws p2 (FaultKind Memory, raw site `[thunk, Run]`, no Go frame). Run's catch is its first catch, so p2 gets `SiteOwner = A1`, `SiteEndsAtRun = true` and `Beneath = p1`.
- The splice gives gopanic, panicmem, sigpanic from p2, then gopanic from p1. p1's site ends at N, which is dropped because the live walk reports it.
- Result: `N.func1 | gopanic | panicmem | sigpanic | gopanic | N`. This is exactly Go's list.
- The 3-deep chain N2 (p1, then `defer func(){panic("p2")}()`, then a nil func) also matches Go exactly: `func1 | gopanic | panicmem | sigpanic | gopanic | N2.func2 | gopanic | N2`.

**S07 (propagated) traced:**
- A's Run stamps the thunk's panic with A's activation, then re-raises it.
- B's catch is its second catch, so nothing is re-stamped.
- At Run(B), `SiteOwner` is A's activation, not B's, so the splice is refused. That is a missing splice, not a wrong one, and it is one of the stated residuals.

**golib-raised panics with no fault kind** (for example `defer close(ch)`, where Go shows closechan): these are refused by the new `FaultKind == None && thrower is not Go source` rule.

**Arms:**
- `ANilDeferredFuncFaultingWhileAPanicRunsKeepsTheOlderGopanic` asserts Go's exact 5-element list. `ANilDeferredFuncFaultingInACalleeSplicesNothing` asserts no splice.
- Both are in the red commit 8e2dd0fa8a. By reading, both are red at a419543c14: the first-cut code gives `[gopanic, panicmem, sigpanic, owner]` in each case.
- If the defect came back, either arm would go red.

**Record note:** the red commit files nil-func-while-panicking under MISSING splices (C3). At a419543c14 it was actually a WRONG splice: the fault frames were present and the older gopanic was missing, which is how the review had it (a blocker). The arm's exact-list assert catches either kind of failure.

**Residual.** The site-less class is still open for one shape: a nil deferred func that takes arguments. That shape is in B2's own scope ("a nil deferred func, or any golib-raised panic directly under Run").

- **Emission:** `var fn func(int); defer fn(1)` is emitted as `defer(fn, 1, ref ᒐ)` (visitDeferStmt.go: parameter counts match, so the method-group form). golib's argument rung wraps it as `frame.Push(() => action(arg))` (builtin.DeferRegistrations.cs, around line 69).
- **What happens:** the golib closure throws the NullReferenceException directly under Run. TryAsPanic maps it to FaultKind Memory. The site has no Go frame, because `go.builtin` is not a `_package` class. So the splice accepts it and gives:
  - while p1 is running: `NA.func1 | gopanic | panicmem | sigpanic | gopanic | NA`
  - on a normal return: `NAR.func1 | gopanic | panicmem | sigpanic | NAR`
- **Go 1.24.13, measured, plain and with -l:**
  - `NA.func1 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | main.NA.deferwrap1 | runtime.gopanic | main.NA`
  - `NAR.func1 | ... | runtime.sigpanic | main.NAR.deferwrap1 | main.NAR`
  - Two arguments behave the same (NA2).
  - Go keeps the deferwrap1 wrapper here only because its callee is sigpanic. For a non-nil `defer g(1)` Go elides deferwrap1, and there the cut is right.
- **Why it matters:** the older gopanic is now present, but deferwrap1 is missing. That is a wrong splice, which [P2-2] counts as worse than a missing one. No arm covers it, and neither the PREDICTION nor the design lists it as a residual. The normal-return form went through the same site-less path at the first cut, so that part is older than the re-cut.
- **Suggested fix:** accept a site-less Memory link only when `raw[0]` is GoFrame's zero-argument `s_nilDeferredCall` thunk, and refuse every other site-less link. Add red-first arms for NA (while panicking) and NAR (normal return) that expect no splice.

Not checked: nothing C# was built or run, as instructed. Every go2cs answer above is by reading.

**Skeptic: partial.** I agree with the verifier: partial. I read e266debe65 and ran Go 1.24.13 (<scratch> plain and with -gcflags=all=-l, same output both times). Nothing C# was built or run.

What is fixed:
- In the re-cut, splicePanic (managed_impl.cs ~2052-2110) walks every link in a loop. Each link must have SiteOwner == the walked Run's activation. The old `if (site.Count > 0)` exemption is gone.
- In Run's inner catch (GoFrame.cs ~255-280), a raised panic with Catches:1 gets SiteOwner, SiteEndsAtRun, and Beneath = running.
- Zero-arg nil func while p1 runs (N, `defer(f!, ref ᒐ)`, the s_nilDeferredCall thunk). Traced result: gopanic | panicmem | sigpanic | gopanic | N. That is exactly Go's list (N: main.N.func1 | gopanic | panicmem | sigpanic | gopanic | main.N). Arm ANilDeferredFuncFaultingWhileAPanicRunsKeepsTheOlderGopanic asserts this exact list, so it would go red if the defect came back.
- Propagated shape (S07). The callee's Run stamps SiteOwner. The outer Run's activation differs, so the whole splice is refused. Arm ANilDeferredFuncFaultingInACalleeSplicesNothing covers it.
- Golib panics with FaultKind None (closechan and similar) are refused by the firstMethodOf rule.

Still wrong (the class B2 names, "a nil deferred func / any golib-thunk-raised panic"):
1. A nil func that takes arguments.
   - `var fn func(int); defer fn(1)`: in visitDeferStmt.go paramCount == getFunctionParamCount, so it emits `defer(fn, 1, ref ᒐ)` (the same form as CrossPkgUser's `defer(note, ..., ref ᒐ)`).
   - builtin.DeferRegistrations.cs:69 wraps it as `frame.Push(() => action(arg))`. The NullReferenceException is raised in that go.builtin closure. TryAsPanic maps it to NilPointerDereference (Memory), with Catches=1 at Run's catch. isGoSourceFrame rejects the closure (top-level type is builtin, not *_package), so the site is empty.
   - Cut result: NA.func1 | gopanic | panicmem | sigpanic | gopanic | NA.
   - Go (measured): main.NA.func1 | gopanic | panicmem | sigpanic | main.NA.deferwrap1 | gopanic | main.NA. NA2 (two args) is the same.
   - Normal-return form NAR: the cut gives ...sigpanic | NAR, Go gives ...sigpanic | main.NAR.deferwrap1 | main.NAR.
   - The older gopanic is now present, but a frame is missing, so this is a wrong splice, neither Go's list nor a refusal. The Func<T,TResult> rung behaves the same way.
   - For non-nil `defer g(1)` Go elides deferwrap1 (GA/GAR/TM measured), so the cut is right there. It keeps the wrapper only when the callee is sigpanic.
2. An extra shape the verifier did not find: a nil value of a named func type called with no args.
   - `type Cancel func(); var c Cancel; defer c()`: the namedFuncType branch emits `defer(() => cʗ1(), ref ᒐ)`, the same form as the corpus's context tests' `defer(() => cancelʗ1(), ref ᒐ)`.
   - That lambda lives in the package class, so isGoSourceFrame accepts it. The NullReferenceException is raised in it, so site = [lambda], and the lambda ends up in the splice.
   - Cut result: sigpanic | NC.funcK | gopanic | NC.
   - Go (measured): main.NC.func1 | gopanic | panicmem | sigpanic | gopanic | main.NC, with no frame between sigpanic and gopanic.
   - This splice has an extra frame. It was wrong at a419543c14 too. Its normal-return form is new in the re-cut: the first cut returned false there, and the re-cut takes the SiteEndsAtRun path.

No arm covers NA, NAR or NC. The restated PREDICTION lists residuals that "each splices NOTHING, never a partial list", and none of these shapes is listed.

**Skeptic residual.** What is still open in B2's scope:
- **Nil func with arguments (NA, NA2, NAR).** The golib `defer<T...>` closure throws the NullReferenceException with no Go frame in the site. The splice accepts it and drops Go's deferwrap1 frame in both the while-panicking form and the normal-return form.
- **Suggested fix for that:** accept a site-less Memory link only when raw[0] is GoFrame's s_nilDeferredCall thunk, and refuse every other site-less link. Add red-first arms for NA and NAR that expect no splice.

A related shape, outside the literal "site has no Go frame" wording:
- **Nil named-func-type call with no args (NC).** The converter's `() => c()` lambda shows up as a spliced Go frame that Go does not have. It is wrong while panicking at both cuts. The normal-return form is newly wrong in the re-cut.
- **Fix for that:** refuse, or have the converter stop wrapping the call, plus an arm.

Neither shape is covered by an arm or listed in the PREDICTION's residuals.

### C1: verifier partial

**Evidence.** WHAT IS CLOSED. Everything on one thread is fixed, and an arm would catch it coming back.
- How numbers are issued: GoFrame.PushSequence gives each deferring Run `++t_lastActivation`. That is a [ThreadStatic] long, so numbers start at 1 and 0 stays free to mean "unowned". ResetSequences and ResetThread never reset it; the census files it as KeptThreadResource. Wrap is not a concern: 2^63 deferring Runs, about 292 years at 1e9 per second.
- When a stamp can happen: SiteOwner is stamped only at a panic's first catch (Catches==1, GoFrame.cs:187 and :270). The one other path is ReRaisedByTheDeferredDelegate, which needs Catches==2 and a re-raise trace containing only the catcher's method.
- Recursion: every activation of a deferring method has the emitted IsPanic catch, because it is static per method. So rec(1)'s catch makes Catches=2 and is never re-stamped. rec(1)'s Run also gets a newer number than rec(0)'s. splicePanic (managed_impl.cs:2056) therefore refuses: SiteOwner is not equal to the walked Run's activation.
- Mutual recursion: F2(1)->M->F2(0) is refused the same way. M has no catch, and F2(1)'s catch makes Catches=2.
- A recursive activation of the catcher cannot sit inside a re-raise trace either: it would add a catch, making Catches=3.
- The arm: ARecursiveActivationIsNotTheOwner asserts no splice for the review's rec shape. It is red against the old MethodBase compare (a419543c14 gives gopanic|rec). It would also be red against slot-index identity, since rec(0)'s and rec(1)'s Runs both use slot 0 on the fresh goroutine. So it catches C1's return.
- Go ground truth: go1.24.13 run at <scratch> gives R: rec.func1|gopanic|rec|rec|main.

WHAT IS OPEN: the same numbers on two goroutines.
- Numbers repeat across threads by construction. Every `go` statement starts a new Thread (Goroutine.cs Start: `new Thread(...)`), so each goroutine's counter starts at 0. The design is safe only while panic objects stay on one thread, and range-over-func breaks that.
- How the object crosses: builtin.range builds a YieldFunctionEnumerable, which runs seq on a Coro. The Coro uses a pooled GoroutineThreadPool thread whose counter keeps growing. Its catch in run() is a plain `catch (Exception ex)` that stores ExceptionDispatchInfo.Capture(ex). rethrowFailure then throws the SAME PanicException object on the ranging goroutine, still carrying the SiteOwner and SiteEndsAtRun stamped on the coro's thread.
- On the ranging side: the ranger's catch makes Catches>=2, so nothing re-stamps. splicePanic then compares a SiteOwner from another thread with this thread's activation number. On equality the check passes.
- Shape X, `func seqX(yield func(int) bool){ defer func(){ panic("px") }() }` ranged by `rangerX` with a deferred recover plus Callers. The site ends at seqX's Run on the coro, so SiteEndsAtRun is true and no method check applies. The collision is deterministic for a fresh goroutine whose first deferring Run is rangerX's, with the process's first coro on a new worker: both numbers are 1.
  - Cut, by reading: rangerX.func1|gopanic|seqX.func1|rangerX
  - Go 1.24.13, confirmed by running it: rangerX.func1|runtime.gopanic|seqX.func1|seqX|rangerX|main.func1|runtime.goexit
  - seqX is missing, so this is a wrong splice.
- Shape Y, recursion across the boundary: F(1) ranges over `func(yield){ F(0) }`, and F(0) panics. site[^1]=F equals the owner F, so on a collision even the second guard, the method check, passes.
  - Cut: F.func1|gopanic|F
  - Go: F.func1|runtime.gopanic|F|F.func2|F|main
- No arm covers a panic that crosses threads. The census note ("never reused across goroutines") holds only between goroutines on the same thread.

Other paths I checked, all safe:
- sync.OnceFunc replays only foreign exceptions; Go panics are re-panicked by value, which makes a new object.
- iter.Pull re-panics by value.
- ActionExtensions throws an AggregateException, which is not a panic.
- builtin.panic always makes a new object, and InheritThrowSite copies only PanicTrace.
- The oncefunc filter that declines still bumps Catches. That over-counts, which only makes a splice missing, never wrong.

**Residual.** Open: one activation number can stand for two different threads. The counter is per thread, and range-over-func's YieldFunctionEnumerator rethrows the SAME PanicException object (via ExceptionDispatchInfo) from the Coro's pooled thread onto the ranging goroutine. When the two counters meet, which is deterministic for a fresh goroutine and a fresh coro worker, the splice drops frames: seqX in shape X; F(0) and the seq closure in shape Y.

Fix, any one of these:
- (a) Tie the stamp to its thread: store a per-thread token object (or ManagedThreadId plus a pool epoch) beside SiteOwner and compare both in splicePanic. The cost is on the panic path only.
- (b) Draw activation numbers from one counter shared by the whole process (an interlocked increment, paid on every deferring Run).
- (c) In YieldFunctionEnumerator.run's catch, mark a PanicException that is handed across threads as unsplicable (SiteOwner=0, with Catches kept at 1 or more so nothing re-stamps it).

Arm to add red first: shape X, plus the shape Y recursion across the boundary. Run each on a fresh goroutine, and force the ranging thread's t_lastActivation and the coro thread's t_lastActivation to the same value by reflection just before each deferring Run. Expect NO splice, or Go's lists:
- X: rangerX.func1|gopanic|seqX.func1|seqX|rangerX
- Y: F.func1|gopanic|F|F.func2|F

Also narrow the census note on t_lastActivation to "never reused on this thread". Nothing else in C1 needs work: recursion, mutual recursion, a pooled thread reusing its counter, and wrap are all closed.

**Skeptic: partial.** I checked this myself at e266debe65 by reading the code, and ran Go 1.24.13 for the frame lists. I agree with partial.

CLOSED: everything on one thread.
- GoFrame.cs:358 gives each Run `++t_lastActivation`. That is a [ThreadStatic] long that is never reset, so on one thread a number is never reused, and wrap would take 2^63 Runs.
- A panic's SiteOwner is stamped only when Catches==1 (GoFrame.cs:187 and :270). The one exception is ReRaisedByTheDeferredDelegate, which needs Catches==2 and a trace containing only the catcher.
- Recursion: the inner call's catch raises Catches to 2, so the outer Run never re-stamps, and its activation is newer. splicePanic (managed_impl.cs:2056) therefore refuses.
- Mutual recursion is refused the same way.
- The arm ARecursiveActivationIsNotTheOwner would go red if C1 came back, whether as a MethodBase compare or as a slot-index compare.

OPEN: a panic that crosses threads.
- Every `go` statement gets a new Thread (Goroutine.cs:830), so its counter starts at 0.
- range-over-func compiles to `foreach ... in range<T>(seq)`. I confirmed this in maps/iter_test.cs:20 and synctest_test.cs:449.
- That becomes YieldFunctionEnumerable, which runs seq on a Coro on a pooled GoroutineThreadPool worker (Coro.cs:128). That worker's counter is never reset.
- run() catches with a plain `catch (Exception)` and ExceptionDispatchInfo.Capture (YieldFunctionEnumerator.cs:98). rethrowFailure (:120-127) then throws the SAME PanicException object on the ranging thread, still carrying the coro thread's SiteOwner and SiteEndsAtRun.
- On the ranging side, the ranger's catch makes Catches 2, so nothing re-stamps. splicePanic then compares the coro thread's number with this thread's activation.

Shape X (seqX's own deferred call panics; rangerX recovers and calls Callers):
- The coro Run stamps SiteOwner=1 and SiteEndsAtRun=true on a fresh worker.
- rangerX's Run is also activation 1 on a fresh goroutine, so the numbers are equal.
- SiteEndsAtRun skips the method check, so the splice goes through.
- The cut, by my reading, gives rangerX.func1|gopanic|seqX.func1|rangerX|main.func1|goexit.

Shape Y (F(1) ranges over func(yield){F(0)}; F(0) panics):
- The site's last frame is F, which equals the owner F, so the method check passes too.
- The cut, by my reading, gives F.func1|gopanic|F|main.func2|goexit.

Go 1.24.13, which I ran at <scratch>
- X: rangerX.func1|runtime.gopanic|seqX.func1|seqX|rangerX|main.func1|runtime.goexit
- Y: F.func1|runtime.gopanic|F|F.func2|F|main.func2|runtime.goexit
- Z (seq panics in its body and has its own defer): rangerZ.func1|runtime.gopanic|seqZ|rangerZ|main.func3|runtime.goexit. The method check (seqZ is not rangerZ) refuses this one, so Z is safe.

So X and Y are WRONG splices, not missing ones:
- X drops seqX.
- Y drops F(0) and F.func2.

Without the collision the same shapes are simply refused, which is the accepted divergence.

No arm covers this:
- PanicFramesReviewTests' 17 arms all run on one goroutine each (onGoroutine), and none uses range or Coro.
- The census row (ThreadStateCensusTests.cs:68) says a number is "never reused across goroutines". That holds only between goroutines on the same thread.

**Skeptic residual.** One activation number can stand for two threads. When range-over-func hands the same PanicException from the coro's pooled worker to the ranging goroutine, and the two per-thread counters happen to be equal, splicePanic accepts a site owned on the other thread. The two counters are deterministically both 1 for a fresh goroutine and the process's first coro worker. The result is a wrong splice:
- Shape X: missing seqX.
- Shape Y, recursion across the boundary: missing F(0) and F.func2.

Any one of these fixes it:
- (a) Store a per-thread identity token beside SiteOwner and compare both in splicePanic. This costs only on the panic path.
- (b) Issue activation numbers from one counter for the whole process, with an interlocked increment.
- (c) In YieldFunctionEnumerator.run's catch, clear SiteOwner on a PanicException before it crosses, keeping Catches at 1 or more so nothing re-stamps it.

Add a red-first arm for X and for Y. Run each on a fresh goroutine and force both threads' t_lastActivation to the same value by reflection. Expect no splice, or Go's lists:
- X: rangerX.func1|gopanic|seqX.func1|seqX|rangerX
- Y: F.func1|gopanic|F|F.func2|F

Narrow the census note on t_lastActivation to "never reused on this thread".

Nothing else in C1 needs work: recursion, mutual recursion, a pooled thread reusing its counter, and wrap are all closed.

### C2: verifier partial

**Evidence.** Read at e266debe65 after a fetch. No C# was built or run.

WHAT THE RE-CUT DOES (correct as far as it goes):
- src/core/runtime/managed_impl.cs:2071, inside the per-link loop of splicePanic: `if (link.FaultKind == PanicFaultKind.None && firstMethodOf(raw) is { } thrower && !isGoSourceFrame(thrower, keepWrapper: true)) return [];`. raw is the link's SiteTrace, which is `new StackTrace(thrown, true)` (PanicException.CaptureThrowSite). The check runs on every link, Beneath links included, so an unmodelled error anywhere in a chain refuses the whole splice.
- Every raise site the review named throws from golib with FaultKind None:
  - slice.cs:129/520/572 (IndexOutOfRange)
  - array.cs:286/297
  - string.cs:236/247
  - builtin.cs:2575 (`_<T>`, interface conversion)
  - map.cs:310/373/389 (nil map write)
  - channel.cs:633/1378 (close of closed or nil channel)
  - the SliceBounds factories
  When golib's frame is the first frame of the trace, isGoSourceFrame rejects it (the type is not a `*_package` class), so these are refused.
- The modelled kinds are not refused. FaultKind is set only by RuntimeErrorPanic.NilPointerDereference (Memory) and IntegerDivideByZero (Divide). TryAsPanic maps NullReferenceException and DivideByZeroException through those two factories, and no golib code raises a nil-deref or divide message any other way (git grep finds none). So `FaultKind == None` short-circuits the check for both. The existing arms still expect those splices: PanicFramesTests ANilDereferenceAddsPanicmemAndSigpanic, AnIntegerDivideByZeroAddsPanicdivide and ANilDeferredFuncFaultsFromTheDeferringFunctionsExit, plus ANilDeferredFuncFaultingWhileAPanicRunsKeepsTheOlderGopanic. A refusal that swallowed a modelled kind would turn them red.
- There is an arm: PanicFramesReviewTests.cs:62 AnUnmodelledRuntimeErrorSplicesNothing. It runs probe indexPanic(5) (PanicFramesProbe.Review.cs:271, `slice<nint>` s[i]) and expects no splice. Removing the refusal line would turn it red.
- The residual is disclosed in PREDICTION.md's ranked uncertainties: "goPanicIndex, the goPanicSlice family, panicdottypeE/I, mapassign*, closechan".
- Go confirmed with go1.24.13 windows/amd64 (<scratch>). Go always puts a runtime frame between gopanic and the site: goPanicIndex for slice and string index, panicdottypeE, mapassign_faststr, closechan. It does so in a hot loop too (hot.func1 | gopanic | goPanicIndex | hot).

WHY IT IS NOT CLOSED:
- The refusal decides "thrown outside Go code" from the physical first frame of the exception trace, and JIT inlining can remove that frame. The index throwers are tiny getters with the `throw` written inline. slice.cs:520-527 records that the indexer is kept inlinable on purpose: PerfSieve got 30% slower when it stopped being "an inlinable array access". The array.cs and string.cs getters are even smaller.
- The validation configuration of record inlines from the first call: Release with DOTNET_TieredCompilation=0 (testConversion.go:6548 testHostRunEnv; main.go:259, where -test-config defaults to Release).
- The repo has already measured that this configuration drops inlined frames from stack walks (callerInliningAnalysis.go:18-25, flag.FlagSet.Set, 2026-08-30).
- So once the indexer is inlined into a Go function, the throw is reported in the Go frame and firstMethodOf returns a Go-source method. The refusal is skipped, and splicePanic emits gopanic directly over the site. That is exactly C2, in its leading example (slice, array and string index).
- The arm cannot see this. It calls indexPanic once under MSTest's default tiering, so the call is compiled at Tier0, where nothing is inlined. JIT tier is the axis that decides the verdict, and the arm pins only the safe side of it.
- Only the index shape has an arm. Type assertion, nil-map write, closechan and slice bounds have none. Those golib methods are larger and less likely to be inlined, but the map setter is moderate in size.
- The inlining claim is reasoned from the code and the repo's own measurement, not measured for this probe.

**Residual.** 1) Decisive check, owed before this can close: run AnUnmodelledRuntimeErrorSplicesNothing with DOTNET_TieredCompilation=0 (or mark probe indexPanic [MethodImpl(MethodImplOptions.AggressiveOptimization)], which compiles it fully optimised on the first call). If it goes red, C2 is open in the configuration of record. If it stays green, record the reading and close.
2) An inlining-proof fix, at no hot-path cost: tag unmodelled runtime errors when they are raised instead of inferring the thrower from the stack. For example, add a PanicFaultKind.Unmodelled or a bool on PanicException, set in the IndexOutOfRange, SliceBoundsOutOfRange, ArrayConversionLength, MakeSlice*, ComparingUncomparableType and RangeFunctionContinued factories and at golib's `new PanicException(...)` sites for nil-map write, send/close on a closed channel, close of a nil channel and interface conversion. splicePanic then refuses on the tag, and the stack test can stay as a second guard.
3) Make the arm independent of JIT tier (AggressiveOptimization on the probe, or a hot-loop variant), and add one arm each for a failed `_<T>` assertion, a nil-map write and a double close. Each expects no splice.

**Skeptic: open.** The previous verifier's inlining argument was untested. I tested it, and it holds: in the configuration of record, C2's lead case (index out of range) is still spliced wrong.

WHAT THE RE-CUT DOES
- The only refusal is managed_impl.cs:2071: `link.FaultKind == None && firstMethodOf(raw) is { } thrower && !isGoSourceFrame(thrower, keepWrapper: true)`.
- raw is `new StackTrace(thrown, true)`, captured at the first catch (PanicException.cs:300, via RuntimeErrorPanic.cs:239-256).
- So the refusal works only while golib's throwing method is still the first frame of the exception trace.

THE MEASUREMENT
- I wrote a standalone C# probe. It is not the suites, and it touches neither the repo nor tG. Location: <scratch>
- It references a copy of the built golib.dll from src/core/golib/bin/Release/net10.0, dated Sep 24 (sha256 7f4d2c85...).
- Its indexer bodies are the same as at e266debe65: `git log -L` puts their last change at slice 2026-08-26, string 2026-08-09 and array 2026-07-31.
- The probe copies the review probe indexPanic exactly: a NoInlining Go-shaped method in a `go.*_package` class, with GoFrame/defer try/catch/finally around `slice<nint> s = new(1); s[i]`. It snapshots `new StackTrace(ex, true)` in the IsPanic filter, as CaptureThrowSite does.

RESULTS (first frame of the exception trace)
- Default tiering, one call:
  - slice``1::get_Item, array``1::get_Item, string::get_Item, map``2::set_Item and builtin::_ are all first.
  - All would be refused. This is the tier the arm runs at.
- DOTNET_TieredCompilation=0 (the configuration of record: testHostRunEnv, Release), repeated twice:
  - slice, array and string index are inlined away. The first and only frame is the Go method (c2probe_package::indexPanic, arrayPanic, stringPanic).
  - The nil-map write is inlined too, but see the caveat under WHAT IS NOT MEASURED.
  - builtin::_ (failed type assertion) keeps its frame.
  - The Go helper `idx` of the review's example is also inlined away.
- Default tiering after warming: slice, array and string index are inlined the same way.

TRACED THROUGH THE RE-CUT, AT TC=0
- The index panic is caught first by indexPanic's own filter (Catches == 1). Run stamps SiteOwner = activation (GoFrame.cs:187-188), so the activation check passes.
- thrower is indexPanic, which is Go source, so the refusal is skipped.
- The splice emits gopanic. SiteEndsAtRun is false and site[^1] == owner, so the site frame is dropped.
- Result: indexPanic.func1 | runtime.gopanic | indexPanic. Go's list is indexPanic.func1 | gopanic | goPanicIndex | indexPanic. That is C2's exact wrong splice.

THE ARM
- AnUnmodelledRuntimeErrorSplicesNothing (PanicFramesReviewTests.cs:62) calls indexPanic(5) once, through onGoroutine.
- GolibTests.csproj sets no TieredCompilation option, so the call runs at Tier0, which inlines nothing. The arm passes only on the side that was already safe, and it would stay green if the inlined case regressed.

WHAT DOES HOLD
- The modelled kinds are not refused. FaultKind is set only at RuntimeErrorPanic.cs:23 (Memory) and :171 (Divide), so `FaultKind == None` short-circuits the check for nil-deref and divide.
- A failed `_<T>` is refused at both tiers.

**Skeptic residual.** 1) The fix must not infer the thrower from the stack. Tag unmodelled runtime errors where they are raised. For example: a PanicFaultKind.Unmodelled, or a flag on PanicException, set in:
   - RuntimeErrorPanic.IndexOutOfRange, the SliceBounds family and the other non-modelled factories;
   - golib's `new PanicException(...)` sites for nil-map write, closed or nil channel close and send, and interface conversion.
   splicePanic then refuses on the tag. The stack test can stay as a second guard. This costs nothing on the hot path, because it runs only on the throw path.
2) Make the arm independent of JIT tier:
   - mark probe indexPanic [MethodImpl(MethodImplOptions.AggressiveOptimization)], or run the arm under DOTNET_TieredCompilation=0;
   - add array and string index arms;
   - add a nil-map-write arm, a double-close arm and a failed `_<T>` arm, each expecting no splice.
   With the current code, a full-opt indexPanic arm should go red. That is the proof owed before C2 can close.
3) What is not measured:
   - The map setter at e266debe65 changed on 2026-09-27 (c0ab481227) and is larger than the Sep-24 build's, so whether it is inlined at the re-cut is unmeasured.
   - closechan and the slice-bounds factories were not probed.
   - The probe used master's Sep-24 golib, not the re-cut's. The re-cut changes no indexer body, but I did not build its golib.
4) Adjacent, outside C2: at TC=0 a thin Go helper such as `idx` is inlined out of the site trace as well. An explicit panic raised inside a one-statement helper would lose that frame in any splice.

### C3: verifier partial

**Evidence.** This was read only. Go ground truth comes from go1.24.13 windows/amd64, run with and without -gcflags=all=-l, from <scratch> c3wrap\main.go and c3fatal\fatal_test.go.

HOW THE RE-CUT HANDLES THESE SHAPES
- In GoFrame.Run's catch (GoFrame.cs:270-280 @e266debe65), when a deferred call raises a panic that is caught here first (Catches==1), the panic's SiteOwner is set to this Run's activation and SiteEndsAtRun is set. Beneath is set only when a panic is running. The one exception is `!(running is null && GoexitException.Started)`.
- In splicePanic (managed_impl.cs:2049-2110), a SiteEndsAtRun link emits gopanic and then every Go frame of its site, then follows Beneath. When Beneath is null, the live walk continues at the owner.

1. NORMAL RETURN: CLOSED.
- Go: `S.func1 | runtime.gopanic | S.func2 | S`. In the loop form Go gives `NRL.func1 | gopanic | NRL.func2 | NRL`, with no deferreturn.
- By reading, the re-cut gives the same list: running is null, firstHere is true, the site is [lambda], and the live walk continues at S.
- The arm is ADeferredCallPanickingOnANormalReturnIsSpliced (PanicFramesReviewTests.cs:78). It asserts [gopanic, normalReturnPanic.func*, owner]. It was red at a419543c14, where site[^1] != owner and there was no Beneath, so the splice was false. It goes red again if the SiteEndsAtRun continuation is reverted.

2. AFTER A COMPLETED RECOVERY: CLOSED.
- Go: `AR.func1 | gopanic | AR.func2 | AR`.
- By reading, the re-cut gives the same list: SetSequence(sequence, null) nulled the entry after the recovering call returned, so running is null.
- The arm is ADeferredCallPanickingAfterACompletedRecoveryIsSpliced (:88), which was red at the first cut.
- RecoverThenPanic, `RP.func1 | gopanic | RP.func2 | gopanic | RP`, also matches.

3. `defer panic(v)`: RIGHT SHAPE, WRONG FRAME NAME.
- Go: `P.func1 | gopanic | P.deferwrap1:48 | P`. Go keeps deferwrap1 because its callee is gopanic. In a loop it is still deferwrap1. Inside a literal it is `PL2.func1.deferwrap1`.
- The converter emits `defer(ᴛ1 => throw panic(ᴛ1), v, ref ᒐ)`. The thunk is a user-assembly lambda, so it counts as a Go source frame and passes the firstMethodOf check.
- The spliced frame therefore has the right line but is named `P.funcN`. No GoPositionMap literal span covers the defer line, so goFrameName falls back to Roslyn's lambda ordinal. By reading, in DeferPanicArg's valueCapture that is probably `func1`, which is Go's name for the recovering closure.
- The arm DeferPanicArgIsSpliced (:98) quotes Go's `P.deferwrap1` in its probe comment but asserts StartsWith(Pkg + "deferPanicArg.func"). It pins the go2cs name, and a fix to deferwrap1 would turn it red.
- Before the re-cut this answer was missing. Now it is a mis-named frame, which is the wrong direction under [P2-2], and PREDICTION.md does not list it as a residual.

4. GOEXIT: CLOSED FOR runtime.Goexit, OPEN FOR THE TEST HOST'S Goexit.
- For runtime.Goexit, Go gives `GX.func1 | gopanic | GX.func2 | runtime.Goexit:636 | GX`. The GoexitException constructor sets Started, and the re-cut refuses. The arm APanicADeferredCallRaisesDuringGoexitSplicesNothing (:66) goes red if the guard is removed, because the splice would then give gopanic | func2.
- For t.Fatal, t.FailNow, t.Skip and t.SkipNow on the test's own thread (InOwnersBubble is false), go2cs's Goexit is TestAbortException (TestExecution.cs:347 and :364). That class is `internal sealed class TestAbortException : Exception` (TestExecution.cs:1643), not a GoexitException, so Started stays false.
- A deferred call that panics during that unwind is then owned (running is null, firstHere is true) and spliced as `X.func1 | gopanic | X.func2 | X | testing.tRunner`.
- Go, from go test: `TestFatalThenDeferPanics.func1 | runtime.gopanic | .func2 | runtime.Goexit:636 | testing.(*common).FailNow:1041 | testing.(*common).Fatal:1118 | TestFatalThenDeferPanics | testing.tRunner`. The SkipNow form is the same with `testing.(*common).SkipNow:1156`.
- So the re-cut drops three frames in the middle: a WRONG splice. That contradicts the stated residual, "a panic a deferred call raises while a Goexit runs its sequence ... splices NOTHING". No arm covers it.

CHECKED AND FOUND FINE
- `defer g(x)`, `defer t.m()` and a method value (Go elides the wrapper, giving `gopanic | g | W`) match.
- Every link must match the walked Run's activation, so a Goexit-unowned link under a later owned panic refuses the whole splice.
- The thread mark is reset for each goroutine (ResetForReuse) and for each test (a dedicated thread).

**Residual.** (1) OPEN: the Goexit guard keys only on GoexitException, so t.Fatal, t.FailNow, t.Skip and t.SkipNow (TestAbortException, which is Go's runtime.Goexit) splice a wrong list: runtime.Goexit and testing.(*common).FailNow/Fatal (or SkipNow) are missing. Fix options: have TestExecution.FailNow and SkipNow set the golib Goexit mark before throwing, or have Run refuse whenever it is running inside a non-panic unwind. Add an arm with Go's list above that expects NO splice.
(2) `defer panic(v)` splices the converter thunk as `P.funcN` where Go shows `P.deferwrap1`. Name that frame deferwrapN, or refuse this thunk shape, or state it as a residual. Change DeferPanicArgIsSpliced to assert Go's name, or no splice, instead of the "deferPanicArg.func" prefix.
(3) Minor, and it predates the cut in the live walk: any deferred call the converter routes through a temp-param or arity-0 lambda can splice an extra `X.funcN` frame when its callee panics directly, where Go elides deferwrap (`VF.func1 | gopanic | vf | VF`, `R1.func1 | gopanic | r1 | R1`). Such calls are variadic callees, nullary callees that return results (`defer f.Close()`), named func types (`defer cancel()`), ref-lowered, sstring-twin and multi-value spread. The live walk already shows that thunk. The re-cut only extends it to normal-return and after-recovery splices, where the first cut answered nothing.
(4) A safe over-refusal: after any runtime.Goexit on a thread, the mark also refuses a nested normal-return splice that Go can determine, for example `FN.func1 | gopanic | FN.func2 | FN` under a Goexit deferred call. The answer is missing, not wrong.
None of these touch the 10 predicted runtime rows.

**Skeptic: partial.** I agree with the other verifier: partial. Two of the four shapes are closed. The other two now get a splice, and that splice does not match Go's list. At a419543c14 both of them got no splice at all.

This was read only against e266debe65 after a fetch. I did not build or run any C#. Go ground truth comes from my own probes, which do not reuse the other verifier's files. They ran on go1.24.13 windows/amd64, with and without -gcflags=all=-l, and both runs gave identical lists. The probes are <scratch> and c3sk2\tst\x_test.go.

**CLOSED**
1. **Normal return.** Go: `NR.func1 | gopanic:792 | NR.func2:28 | NR:29`.
   - GoFrame.cs Run catch: running is null and firstHere is true, so the panic gets SiteOwner = this activation and SiteEndsAtRun.
   - splicePanic then emits gopanic, then the site [func2], and the live walk continues at NR. That matches Go.
   - The arm is ADeferredCallPanickingOnANormalReturnIsSpliced. It asserts Count==3. By reading it is red at the first cut (site[^1] != owner and no Beneath, so nothing was spliced) and would go red again if the change were reverted.
2. **After a completed recovery.** Go: `AR.func1 | gopanic | AR.func2 | AR`.
   - SetSequence(sequence, null) after the recovering call returns makes running null. That gives the same path as the normal return, and the same list as Go.
   - The arm is ADeferredCallPanickingAfterACompletedRecoveryIsSpliced.
3. **runtime.Goexit.** Go: `GX.func1 | gopanic | GX.func2 | runtime.Goexit:636 | GX`.
   - The GoexitException constructor sets t_started, and the guard `!(running is null && GoexitException.Started)` refuses the splice.
   - The arm is APanicADeferredCallRaisesDuringGoexitSplicesNothing.

**NOT CLOSED**
4. **The test host's Goexit (t.Fatal, t.FailNow, t.Skip, t.SkipNow).** The re-cut gives a partial list here, where its own residual promises no splice. Go's lists (go test):
   - Fatal: `TestFatalThenDeferPanics.func1 | runtime.gopanic:792 | .func2:29 | runtime.Goexit:636 | testing.(*common).FailNow:1041 | testing.(*common).Fatal:1118 | TestFatalThenDeferPanics:30 | testing.tRunner`
   - SkipNow: the same, with `testing.(*common).SkipNow:1156`.
   - FailNow: the same, with `FailNow:1041` directly.

   In go2cs, on the test's own thread, TestExecution.FailNow and SkipNow throw `new TestAbortException()` (TestExecution.cs:347 and :364). That class is `internal sealed class TestAbortException : Exception` (:1643). It is not a GoexitException, so t_started is never set.

   How the splice goes wrong, by reading:
   - IsPanic refuses the TestAbortException. It only sets InFlightForeignException, and the foreign branch is skipped because px.State is not null.
   - The deferred call's panic is then caught first in Run: running is null, firstHere is true, and the guard passes. So it is owned with SiteEndsAtRun.
   - The spliced answer is `X.func1 | gopanic | X.func2 | X | ...`. runtime.Goexit and FailNow/Fatal (or SkipNow) are missing between func2 and X.

   PREDICTION.md:80-88 says the residuals each "splice NOTHING, never a partial list", including "A panic a deferred call raises while a Goexit runs its sequence". This case falsifies that.

   At a419543c14 the same shape spliced nothing (site[^1] != owner and no Beneath). So the re-cut moved it from missing to partial, which is the direction [P2-2] forbids. No arm covers it.
5. **`defer panic(v)`: a frame with the wrong name.** Go gives `P.func1 | gopanic | P.deferwrap1:40 | P:41`. Nested inside a literal it gives `PL.func1.1 | gopanic | PL.func1.deferwrap1 | PL.func1 | PL`.
   - The converter emits `defer(ᴛ1 => throw panic(ᴛ1), v, ref ᒐ)` (DeferPanicArg/main.cs:24). That thunk is a user-assembly lambda, so isGoSourceFrame accepts it and the splice keeps it.
   - goFrameName names it from the Roslyn ordinal fallback, because no recorded literal span covers the defer line (Go line 16 in valueCapture, where the literal on line 15 is one line long). So it becomes `valueCapture.funcN`. That is probably `func1`, the same name as the recovering closure.
   - There is no deferwrap naming anywhere in src/go2cs, golib, managed_impl.cs or GolibTests. The only mention is the probe comment.
   - The arm DeferPanicArgIsSpliced asserts `StartsWith(Pkg + "deferPanicArg.func")`. That pins the go2cs name, not Go's, and a fix to deferwrap1 would turn it red.
   - This case is not stated as a residual. At the first cut it was a missing splice.

None of this touches the 10 predicted runtime rows.

**Skeptic residual.** 1. **The test host's Goexit.** t.Fatal, t.FailNow, t.Skip and t.SkipNow on the test's own thread throw TestAbortException, so GoexitException.Started is never set. A panic from a deferred call during that unwind then gets a partial splice, `X.func1 | gopanic | X.func2 | X`, where Go shows `... | runtime.Goexit | testing.(*common).FailNow | [Fatal] | X`. This contradicts the stated Goexit residual ("splices NOTHING, never a partial list"), and it was a missing splice at a419543c14. Two ways to fix it:
   - Set the golib Goexit mark in TestExecution.FailNow and SkipNow before they throw TestAbortException, for example with an internal GoexitException.Mark().
   - Or have Run refuse whenever it runs inside a non-panic unwind.

   Either way, add an arm with Go's list above that expects NO splice for t.Fatal and for SkipNow.
2. **`defer panic(v)`.** The converter thunk is spliced as `P.funcN`, which can collide with the recovering closure's `P.func1`, where Go shows `P.deferwrap1`. Choose one:
   - name that frame deferwrapN;
   - refuse this thunk shape;
   - state it as a residual in PREDICTION.md.

   Then change DeferPanicArgIsSpliced from the "deferPanicArg.func" prefix to Go's name, or to no splice.

Minor, and safe, so not blocking: after any runtime.Goexit on a thread, the thread-wide mark also refuses a nested normal-return splice that Go can determine. That answer is missing, not wrong.

### C4: verifier partial

**Evidence.** THE CODE FIX IS CORRECT (checked by reading at e266debe65).
- The recursive splicePanicInto and its `if (depth > 64) return false` are gone. splicePanic (src/core/runtime/managed_impl.cs:2041-2110) is now a single `for (link = panic; link != null; link = link.Beneath)` loop with no depth cap.
- Cycles: a HashSet<PanicException> with ReferenceEqualityComparer (:2052). `if (!visited.Add(link) || link.SiteOwner != activation) return [];` (:2056) means a repeated link refuses the splice and cannot loop. Every step adds a distinct object, so the walk always ends.
- Emission: the list comes back to captureCallers, which runs every spliced pc through the same remainingSkip-- (:1929) and `if (count >= len(pc)) return count;` (:1933) as the live frames. So skip counts gopanic and the spliced site frames, and a full buffer stops the walk from the top with no goexit forced in. runtime.Caller goes through callers() -> captureCallers, the same path.
- Traced Chain(n) through GoFrame.Run (src/core/golib/GoFrame.cs):
  - p0 is stamped at Run entry (`owned is {Catches:1, SiteOwner:0}`).
  - Each deferred panic p_k is caught first by Run's inner catch: firstHere, so SiteOwner is this activation, SiteEndsAtRun=true and Beneath=p_{k-1}.
  - Every link passes, and the splice is [gopanic, Chain.func2] x n, then gopanic, with the owner dropped by method.
- Go ground truth (go1.24.13, <scratch>):
  - Chain(1/64/65/66) with a 256-slot buffer gives 9/135/137/139 frames, all in exactly that pattern, then Chain | main | runtime.main | runtime.goexit.
  - Chain(70) with 64 slots gives 64 frames from the top (names | Chain.func1 | gopanic | Chain.func2 | ...).
  - buf=3 gives names | Chain.func1 | runtime.gopanic.
  - skip=3/4 with buf=4 start at gopanic / Chain.func2, so skip counts spliced frames.
  - The cut's traced output matches every case.

NO ARM WOULD CATCH C4 RETURNING.
- PanicFramesReviewTests' 17 arms (src/tests/GolibTests/PanicFramesReviewTests.cs) have no chain longer than 3 links. The longest is ATwoLinkChainSplicesBothRaisersInOrder at :114.
- There is no arm at 65, 66 or 70 links, no long chain with a small buffer, and no cycle arm.
- ASmallBufferFillsFromTheTopAndSkipCountsGopanic (:147) covers capacity and skip on a SINGLE-link splice only.
- So a reintroduced cap would pass all 28 PanicFrames arms, for example `if (visited.Count > 64) return [];` or a return to a recursive depth guard.
- C5 in the review asked for "the bound, a cycle" arms, and the re-cut added neither.

**Residual.** 1. Add a bound arm (golib only, no Go needed): a probe that defers n>=66 loop closures that each panic(i), then panics p0, with a recover plus callersHere deferred first. It should assert the 2n+1 spliced frames below the recovering closure, i.e. gopanic/closure pairs n times and then gopanic, then the owner. It should also assert a 64-slot buffer's 64 frames all come from the top. Go's frames were confirmed on go1.24.13: Chain(66) gives 139 frames, and Chain(70) with 64 slots gives 64 frames from the top.
2. Optionally add a cycle arm that plants p1.Beneath=p2, p2.Beneath=p1 through the internal setter or reflection, and asserts no splice and no hang.
3. A design note, not a defect: the chain is validated in full before anything is emitted. A bad link deep below where the buffer would fill therefore refuses the whole splice, where Go would still fill the buffer from the top. That is a missing splice, not a wrong one, and it matches the stated all-or-nothing rule. It could be named under "Residuals" in PREDICTION.md.

**Skeptic: partial.** The code fix is correct. Nothing guards it: no arm would catch the defect coming back, so the verdict stays partial. I checked everything at e266debe65 after a git fetch, reading only.

**What the code does now**
- The recursive walk and its `if (depth > 64) return false;` are gone.
- splicePanic (src/core/runtime/managed_impl.cs:2041-2110) is a single loop, `for (link = panic; link != null; link = link.Beneath)`, with no depth cap.
- It keeps a `HashSet<PanicException>` with ReferenceEqualityComparer (:2052). The check `if (!visited.Add(link) || link.SiteOwner != activation) return [];` refuses a repeated link. Every pass adds a new distinct object, so the walk always ends and cannot loop on a cycle.
- The list goes back to captureCallers (:1921-1937). There every spliced pc goes through the same `remainingSkip--` and `if (count >= len(pc)) return count;` as the live frames. So skip counts gopanic and the spliced frames, and a full buffer stops the walk from the top with no goexit forced in.
- runtime.Caller also goes through callers() and then captureCallers.

**Tracing Chain(n) through GoFrame.cs**
- p0 is stamped at Run entry (:187, `owned is {Catches:1, SiteOwner:0}`). SiteEndsAtRun is false and its site is [Chain], which is dropped by method.
- Each deferred p_k is first caught by Run's inner catch, so firstHere holds (:270-279): SiteOwner is this activation, SiteEndsAtRun is true, and Beneath is the running entry, p_{k-1}.
- InheritThrowSite changes only PanicTrace, not SiteTrace (PanicException.cs:309-311).
- The splice is therefore (gopanic, func2) x n, then gopanic. That is 2n+1 frames with no bound.
- Sequence storage grows by doubling (GoFrame.cs:350-355), so there is no hidden cap there either.

**Go 1.24.13 ground truth, run myself** (<scratch> which adds a check() frame)
- With 256 slots, n = 1/64/65/66/70 gives 10/136/138/140/148 frames. Each is names | Chain.func1 | (gopanic | Chain.func2) x n | gopanic | Chain | check | main | runtime.main | runtime.goexit.
- n=70 with 64 slots gives 64 frames, all from the top, ending inside the func2/gopanic pairs.
- n=66 with 3 slots gives names | Chain.func1 | gopanic.
- skip=3 with 4 slots starts at gopanic; skip=4 starts at Chain.func2. So Go's skip counts spliced frames.
- The re-cut's traced output matches every case.

**Why this is not closed**
- The re-cut's arms never build a chain longer than 3 links. The longest is twoLinkChain (PanicFramesProbe.Review.cs:236, asserted at PanicFramesReviewTests.cs:114).
- ASmallBufferFillsFromTheTopAndSkipCountsGopanic (:147, probe capacityAndSkip at :307) checks capacity and skip on a single-link splice only.
- There is no arm at 65, 66 or 70 links, none for a long chain with a small buffer, and none for a cycle. `git grep Beneath -- src/tests` finds only test names.
- src/tests/Behavioral/RuntimeCallerFrames and FuncLiteralCallerNames have no panic chains. Go's TestCallers* rows loop at most once.
- So bringing back `depth > 64`, `visited.Count > 64`, or any cap of 3 or more would pass all 28 PanicFrames arms (11 plus 17).
- The review asked for these arms twice: C5 lists "the bound, a cycle", and C4's fallback asks for "arms at 65 and 66".

**Skeptic residual.** 1. **Add a bound arm** in GolibTests. It needs only golib, not Go, and can be modeled on twoLinkChain.
   - The probe defers a recover-then-callersHere closure first, then n >= 66 loop closures that each throw panic(i), then throws p0.
   - Assert that below the recovering closure come exactly 2n+1 spliced frames, (gopanic, closure) x n then gopanic, followed by the owner.
   - Assert that a 64-slot buffer returns 64 frames, all from the top.
   - Go 1.24.13 confirms the shape: 2n+1 spliced frames, and 64 frames from the top with a 64-slot buffer.
   - This arm would red on a reintroduced `depth > 64` or `visited.Count > 64`.
2. **Optionally, add a cycle arm.** Set p1.Beneath = p2 and p2.Beneath = p1 through the internal setter, then assert there is no splice and no hang. A cycle can't be reached from emitted code, so this only guards the visited-set backstop.
3. **A design note to state, not a defect.** The whole chain is checked before anything is emitted. So an unowned link deep down (for example p0 re-raised through a closure) refuses the whole splice, even where Go would still fill a small buffer from the top. That is a missing splice rather than a wrong one, and it follows the all-or-nothing rule. PREDICTION.md's "Residuals the re-cut states" does not name it yet.

### C5 and C6: verifier partial

**Evidence.** This was a read-only check. I read the arms and traced them against the first cut's code (the pre-fix splicePanicInto, and Run's `raised.Beneath ??= running`), and confirmed Go's lists with go1.24.13 `go run` under <scratch>

**C6: closed.**
- PREDICTION.md gains a dated block. The earlier text is kept and marked superseded.
- It predicts exactly 10 moves: TestCallersPanic, TestCallersDoublePanic, TestCallersNilPointerPanic, TestCallersDivZeroPanic and TestCallersDeferNilFuncPanic; both /CallersFrames subtests; and the parents TestStackWrapperStackPanic/sigpanic, TestStackWrapperStackPanic/panicwrap and TestStackWrapperStackPanic.
- The parent names match Go's t.Run names in stack_test.go:725/729.
- The count is consistent: the wrapper subtree has 7 verdict keys (the key layout is confirmed in comparison-classifier testdata). With both /Stack passing at base, 5 move, and 5 + 5 = 10.
- The prediction is coherent with the code:
  - TestCallersPanic and TestCallersDoublePanic are claimed at the frame's own catch (Catches == 1, stamped at Run entry), and they throw from Go frames, so the new unmodelled-error refusal does not fire. In DoublePanic, p1's entry stays live because func1 has not returned.
  - The NilPointer, DivZero and nil-thunk cases carry a FaultKind, so the refusal skips them.
  - The nil thunk takes the Run-first-catch path, and the Goexit mark is false there: TestExecution.Start gives every test a dedicated new thread, and FailNow on the owner thread throws TestAbortException, not a Goexit.
  - Both /CallersFrames subtests fault as a NullReferenceException through `(p0) => p0.M()` or `p0.Value`, which maps to FaultKind Memory. The refusal does not reach them, and the owner check matches the subtest lambda.
  - /Stack is untouched: the re-cut changes no Stack path.
  - The stays hold: TestCallersAbortedPanic and TestCallersAbortedPanic2 have null entries when they read, and WithLoop still lacks deferreturn.
- Beneath has no reader other than the splice.

**C5: mostly closed.**
- The 17 arms map to the review's shapes:
  - Asserting NO splice: shape A, shape B, S06's G shape, a nil deferred func propagated from a callee (S07), recursion (S03/S08/S17), indexPanic (C2), and a panic during Goexit.
  - Asserting Go's list: D2 `defer gDeferPanics()`, a normal-return panic, a panic after a completed recovery, `defer panic(v)`, a nil deferred func while a panic runs, a two-link chain (depth > 1, with exact thrower names), recover-then-panic, Caller(1) at panic.go:792, a 3-slot buffer and skip 2, and the spliced lines of f1/f2/f3.
- At a419543c14 exactly 10 are red, as the RED commit claims. Wrong splices: shape A and G give gopanic|gDeferPanics|gopanic|owner; shape B gives gopanic|lambda|gopanic|owner; the propagated nil thunk gives gopanic|panicmem|sigpanic|owner; recursion gives gopanic|rec; indexPanic gives gopanic|indexPanic. Missing splices: the normal-return, after-recovery and deferPanicArg shapes splice nothing, and the nil-thunk-while-panicking shape drops p1's gopanic. The other 7 are green at the first cut, which matches the review's "correct today".
- Every Go list the arms assert matched `go run`.
- The RED commit touches no src/core, and the FIX commit changes no arm (only the NoUncountedBacking allowlist text), so the fix did not weaken the arms.
- The census:
  - It lists t_lastActivation (KeptThreadResource; sound within a thread, because numbers only increase) and t_started (GolibReset, wired through GoFuncRoot.ResetThread).
  - Dirty() plants a stale Sequence entry, t_sequenceDepth = 1 and t_started = true. Slots() asserts depth 0, no live entry and false.
  - Both census tests are red at the first cut: stale census keys, and Dirty() throws on the old PanicException[] element type.

**Residual.** 1. **No arm for the bound (C4), which C5 names.** The fix drops the depth-64 cap and truncates through the capacity path, but no arm would catch the cap coming back. Go's answer, confirmed: chain(70) read through a 64-slot buffer gives 64 frames from the top: `runtime.Callers | chain.func1 | runtime.gopanic | chain.func2 | runtime.gopanic | ...`. Suggested arm: 70 replacing defers, and assert 64 frames of alternating gopanic and raiser.
2. **Other arms not added:**
   - A cycle arm: emitted code cannot build a cycle, but one could be planted by reflection to guard the visited set.
   - Mutual recursion (F2 -> M -> F2), which uses the same activation mechanism as the recursion arm.
   - S11's second-pass pairing: a Callers from an outer deferred call while a Goexit unwinds after FailNow in an inner deferred call. The goexitShape arm covers a different shape.
3. **Nits:**
   - The DeferPanicArg arm accepts any `deferPanicArg.func*`, where Go reports `deferPanicArg.deferwrap1` (confirmed). The naming divergence is pinned by the arm but not named in the residuals.
   - The normal-return and after-recovery arms check the name prefix only, not Go's func2.
4. **Outside my item, for the C2 and B1/C1 reviewers.** Every arm runs one-shot Tier0 code with NoInlining probes, so none can see these hazards in optimized code:
   - The C2 refusal keys on the first raw frame not being Go source. slice.this[nint] throws inline, so once Tier1 inlines it into a hot Go caller, the thrower reads as the Go frame and gopanic is spliced straight over the site again.
   - ReRaisedByTheDeferredDelegate accepts when no intermediate frame is visible. An implicit tail call at Tier1 from an unannotated closure or caller (shape A's `func(){ g() }`, S06's D) removes that frame, and the B1 wrong splice returns.
   - Activation numbers are per thread and start at 1 on every thread. A PanicException object carried to another thread could collide. I found no path that does this; OnceFunc replays a new panic.
5. **Unverified:** the base read (both /Stack subtests PASS at e6fc210500) rests on frames-evidence/gates/base-wrapper-comparison.json on R-LAPTOP, which is not in the tree. If either /Stack subtest fails at base, its parent and TestStackWrapperStackPanic do not move, and the count is below 10.

**Skeptic: partial.** This was a read-only check. I fetched origin, read the review's table and appendix (S01-S21), the diff a419543c14..e266debe65, the three commit messages, and the arms, probes, census and PREDICTION.md as they stand at e266debe65. I confirmed Go's lists with go1.24.13 `go run` under <scratch>

**C6 is closed.**
- The restated block is dated and the old text is kept and marked superseded. It predicts exactly 10 moves: the five TestCallers* rows, both /CallersFrames subtests, TestStackWrapperStackPanic/sigpanic, TestStackWrapperStackPanic/panicwrap, and TestStackWrapperStackPanic.
- The parent names match Go's t.Run names at stack_test.go:725 and :729. The testStackWrapperPanic subtests are CallersFrames (:739) and Stack (:762).
- The count holds. The wrapper subtree has 7 keys (comparison-classifier testdata lines 766-772). With both /Stack subtests passing at base, 5 of them move, and 5 + 5 = 10.
- It is coherent with the code:
  - TestCallersPanic and TestCallersDoublePanic are claimed at a first catch (Catches == 1, stamped at Run entry) and are thrown from Go frames, so the C2 refusal does not fire. In DoublePanic, p1's entry is still live because func1 never returns normally.
  - The NilPointer, DivZero and nil-thunk cases carry a FaultKind.
  - TestCallersAbortedPanic, AbortedPanic2 and AfterRecovery read a null entry, so they stay PASS.
- The arm counts it states match the tree: PanicFramesTests has 11 [TestMethod]s and PanicFramesReviewTests has 17.

**C5 is mostly done.**
- The 17 arms map to the review's shapes: A, B, the S06 G shape, D2, the S07 propagated nil thunk, recursion (S03/S08/S13/S17), indexPanic (C2), normal return, after recovery, `defer panic(v)`, the nil thunk while panicking, the two-link chain, recover-then-panic, Caller(1) at panic.go:792, the 3-slot buffer with skip 2, the lines of f1/f2/f3, and a panic during Goexit.
- I traced the arms against the first cut's splicePanicInto and its `Beneath ??= running`. Exactly 10 are red: shapes A and B, the G shape, the propagated nil thunk, recursion and indexPanic are wrong splices; normal return, after recovery, deferPanicArg and the nil thunk while panicking are missing ones. That matches the RED commit.
- The arms have teeth against the fix:
  - Removing the Goexit guard would redden goexitShape.
  - A MethodBase owner check would redden the recursion arm.
  - In shape A, the closure frame makes ReRaisedByTheDeferredDelegate return false, while D2 is accepted.
- The FIX commit changes no arm; the only test edit is the allowlist text in NoUncountedBacking.
- The census work is done. Dirty() plants a stale Sequence entry, t_sequenceDepth = 1 and t_started = true. Slots() asserts depth 0, no live entry and false. The proof strings match ResetSequences and GoexitException.ResetThread, and t_lastActivation is recorded as a KeptThreadResource.

**The gap: C5's own row names "the bound", and no arm tests it.**
- I searched every panic-frames test file and every GolibTests caller of Callers: no chain is longer than 2 links.
- The bound shape is genuinely red at a419543c14. There, `if (depth > 64) return false` drops the whole splice at 65 or more deferred panics.
- The fix drops the cap and claims "a long chain truncates from the top as Go's does". That holds by reading, since splicePanic builds the full list and captureCallers stops at len(pc). But if anyone re-added a cap, all 28 arms would stay green.
- Go's answer, confirmed: Chain(70) read through a 64-slot buffer gives 64 frames from the top (runtime.Callers | names | Chain.func1 | runtime.gopanic | Chain.func2 | runtime.gopanic | ...). Chain(65) through 256 slots gives the full list down to Chain | main.
- The row's "a cycle" also has no arm, but emitted code cannot build one, so that is waivable if it is named.

**Skeptic residual.** 1. **Add the bound arm, the one real hole.** Build a probe with 70 `defer(() => throw panic(i))` in a loop over panic("p0"), and read it from an outer deferred call through a 64-slot buffer. Assert 64 frames: runtime.Callers, callersHere, the reader, then alternating runtime.gopanic and the raiser's name all the way to the end. Also assert that Chain(65) through a 256-slot buffer ends at the owner. It is red at a419543c14 and green at the fix.
2. **Name or add the lesser gaps.**
   - A cycle arm. It is unreachable from emitted code, since Beneath is set only at a first catch or the accepted re-raise. It could be planted by reflection to guard the visited set, or waived by name.
   - An F2 -> M -> F2 mutual-recursion arm, which S11's suggested fix asks for. It is red at the first cut, but the rec arm already guards the same activation mechanism.
   - S11's second-pass pairing: a Callers from an outer deferred call while a Goexit unwinds after FailNow. No arm covers it.
3. **Nits.**
   - DeferPanicArgIsSpliced accepts `deferPanicArg.func*` where Go reports `P.deferwrap1` (confirmed). The divergence is pinned but not named among the residuals.
   - The normal-return and after-recovery arms check the name prefix only.
4. **Unverified from the tree.** The C6 count rests on both /Stack subtests passing at e6fc210500. The evidence for that is frames-evidence/gates/base-wrapper-comparison.json on R-LAPTOP, which is not committed. If either /Stack subtest fails at base, fewer than 10 rows move.
5. **Outside C5/C6, for the B1 and C2 reviewers.** Every arm runs one-shot Tier0 code, so two hazards are invisible to them:
   - An implicit Tier1 tail call from `() => gDeferPanics()` would remove the closure frame, ReRaisedByTheDeferredDelegate would then accept it, and the B1 wrong splice would come back.
   - Once the slice indexer is inlined into a hot Go caller, the thrower would read as Go source and defeat the C2 refusal.

### REGRESSION FINDER: verifier open

**Evidence.** I read the whole re-cut and everything around it at e266debe65. Five of the six axes you named are clean. The re-cut does open one new wrong-splice path, which the first cut did not have, and one smaller path where it now leaves out a splice the first cut got right.

CLEAN AXES
(1) No-panic hot path. There is no new allocation: the Sequence[16] array is still allocated once per thread. Each deferring Run now does one extra thread-static read and write (++t_lastActivation, GoFrame.cs:358) and stores a 16-byte struct entry. That struct store also drops the first cut's covariant array-store helper, so the cost is roughly neutral. On a normal return the entry check `owned is {Catches:1, SiteOwner:0}` (GoFrame.cs:187) is just a null test. The ReRaised gate, IsMachinery and the HashSet all run on the panic path only.
(2) Catches. It is incremented only in TryAsPanic, during the filter's first pass, once per dispatch. Every filter inside Go frames that calls it accepts the exception. The only filters that call it and decline are the goroutine-root pair CanContain/Observed (Goroutine.cs:1079/1091), which only an escaping panic reaches, and nothing runs after that. The typed `catch (PanicException){ throw; }` in channel.cs:1055/1077 does not count, but `throw;` keeps the whole trace. An abandoned second pass can only over-count, and over-counting only refuses a splice.
(3) Finally-funclet duplicates. They are handled only in the ReRaised gate. The splice is built from SiteTrace, so no real frame is ever dropped. Real recursion reaches Catches >= 3 and is refused.
(4) Same-thread cleanup. PopSequence runs in the finally on every exit: normal, panic, recover, Goexit and foreign exceptions. t_started is reset through ResetForReuse, and every test gets a fresh dedicated thread.
(5) Census. t_lastActivation is classed KeptThreadResource and t_started is GolibReset with the correct needle. The reuse arm plants the stale entry, the depth and t_started.

REGRESSION 1: a missing splice becomes a WRONG splice across threads. Range-over-func runs seq on a Coro pool thread, and YieldFunctionEnumerator.run/rethrowFailure rethrows the SAME exception object on the ranging thread through ExceptionDispatchInfo.
(a) Goexit (deterministic by reading). The GoexitException constructor (GoexitException.cs:37-43) marks the COROUTINE's thread, not the ranging goroutine's. On the ranging thread, the guard `!(running is null && GoexitException.Started)` (GoFrame.cs:272) therefore passes, and a panic from a deferred call during that Goexit is owned and spliced. Go 1.24.13, shape GX, which is a goroutine G2 that does `for range seqExit` where seqExit calls runtime.Goexit, then defers panic("px") and a recover+Callers: `G2.func1 | runtime.gopanic | G2.func2 | runtime.Goexit | seqExit | G2 | runtime.goexit`. The re-cut, by reading, gives `G2.func1 | gopanic | G2.func2 | G2 | goexit`. The first cut spliced nothing here (site[^1]=G2.func2 is not the owner, and there is no Beneath).
(b) Activation collision. Activation numbers come from a per-thread counter that starts at 0 on every new thread (GoFrame.cs:345). splicePanic compares them by number only (managed_impl.cs:2056), and a SiteEndsAtRun link skips the method check (2089-2104). When seq's own deferred call panics, seq's Run stamps the panic with a coroutine-thread number (GoFrame.cs:274). If that equals the ranging function's Run number, the re-cut splices it. Go, shape R1: `R1.func1 | runtime.gopanic | seq.func1 | seq | R1 | main`. The re-cut on a collision gives `R1.func1 | gopanic | seq.func1 | R1` (seq is missing). The first cut refused this. Fresh threads both start at 1, so a minimal program is a likely hit. I did not run the C#.
No arm covers either case: the only Goexit arm is the same-thread shape.

REGRESSION 2: a correct splice becomes a missing one (the safe direction). The accepted re-raise requires `SiteOwner != 0` (GoFrame.cs:379), but only a Run with m_count > 0 stamps a panic. So a deferred delegate that caught first but registered no defer at runtime is refused while its parent panics. That covers an unreached conditional defer, and every frame whose defers were all lowered into the finally, since its m_count is always 0. Go, shape M0: `M0.func1 | gopanic | Dc | gopanic | M0`. The first cut produced exactly this through Beneath; the re-cut splices nothing. The same happens for any converted-assembly forwarder that IsMachinery counts as a frame.

Go ground truth: <scratch> (main.go, goexit_test_shape.go), run with GOROOT=<profile>\sdk\go1.24.13 and GOTOOLCHAIN=local.

**Residual.** Suggested fixes, smallest first:
(a) Make activations unique across threads. Either reserve per-thread blocks from a static Interlocked counter (one compare on the hot path), or stamp the owning thread too and require it to match in splicePanic. Also mark a Goexit on the thread that rethrows it: YieldFunctionEnumerator.rethrowFailure, when it rethrows a GoexitException. Alternatively, clear SiteOwner/SiteEndsAtRun on that cross-thread rethrow.
(b) Relax ReRaisedByTheDeferredDelegate's `SiteOwner != 0` to `!SiteEndsAtRun && site[^1] != RunMethod && Catches == 2`, or state M0 and all-lowered catchers as residuals.

Arms owed, each red first:
- GX: a range-over-func seq calls Goexit, and the ranging goroutine's deferred call panics. Expect no splice. This is deterministic.
- R1: seq's deferred call panics. Plant equal t_lastActivation values on both threads by reflection, then expect no splice.
- M0: a deferred delegate with an unreached conditional defer panics while its parent panics. Expect Go's list, or record the residual.

The census rationale for t_lastActivation ("never reused across goroutines") is true only within one thread and should say so.

No predicted runtime row reaches these shapes, so the 10-row prediction is unaffected.

**Skeptic: open.** I read the re-cut myself (git diff a419543c14 e266debe65, plus the files at e266debe65) and re-ran the verifier's Go program under go1.24.13 (<scratch> copied from ..\regress). Go's lists match what the verifier reported:
- GX: G2.func1 | runtime.gopanic | G2.func2 | runtime.Goexit | seqExit | G2 | runtime.goexit
- R1: R1.func1 | runtime.gopanic | seq.func1 | seq | R1 | main
- M0: M0.func1 | runtime.gopanic | Dc | runtime.gopanic | M0 | main

GX is a new wrong splice. It happens every time, and I confirmed it by reading the code.
- range(seq) is built on YieldFunctionEnumerable (builtin.cs:793). Its seq runs through Coro.Start, then GoroutineThreadPool.Run, which puts it on a separate pool thread inside Goroutine.Run. So OnGoroutine is true there, and runtime.Goexit (managed_impl.cs:743) builds the GoexitException on the coro thread.
- The GoexitException constructor sets that thread's [ThreadStatic] t_started (GoexitException.cs:43).
- YieldFunctionEnumerator.run catches the exception, and rethrowFailure rethrows the same object on the ranging thread through ExceptionDispatchInfo.Throw, so the constructor does not run again there. On the ranging thread GoexitException.Started is false.
- In G2's Run: px is caught first there (Catches 1, SiteOwner 0), so firstHere is true. running is null. The guard !(running is null && GoexitException.Started) at GoFrame.cs:272 therefore passes. The Run stamps SiteOwner=activation and sets SiteEndsAtRun.
- splicePanic then emits gopanic | G2.func2, and the live walk goes on at G2. Result: G2.func1 | gopanic | G2.func2 | G2 | goexit, which is missing runtime.Goexit and seqExit.
- The first cut spliced nothing here: its owner was G2 and site[^1] was G2.func2, with no Beneath because running was null.
- This contradicts the re-cut's own stated residual in PREDICTION.md:88 ('a panic a deferred call raises while a Goexit runs its sequence ... splices NOTHING, never a partial list').
- The only Goexit arm, APanicADeferredCallRaisesDuringGoexitSplicesNothing, uses panicDuringGoexit, which calls Goexit on the same thread. Nothing tests the cross-thread path.

R1 can splice wrong when two activation numbers happen to match. It is a real hole, though not certain to trigger.
- t_lastActivation is a per-thread counter (GoFrame.cs:358), and the census classes it KeptThreadResource.
- seq's Run stamps p2 with a coro-thread activation, with SiteEndsAtRun set. The catch on the ranging thread only raises Catches to 2, and R1's Run does not restamp.
- splicePanic compares link.SiteOwner != activation by number only, and a SiteEndsAtRun link skips the method check.
- So if R1's ranging-thread activation equals seq's coro-thread activation, the output is R1.func1 | gopanic | seq.func1 | R1 | main, with seq missing.
- The first cut refused this, because site[^1] was seq.func1, not the owner R1, and there was no Beneath.
- The census rationale ('never reused across goroutines') holds only within one thread.
- The verifier said a minimal program is 'likely' to hit this. I think that overstates it: prior deferring Runs on the main thread shift its counter. Either way the hole exists.

M0 lost a splice the first cut got right. This is the safe direction, but it contradicts the re-cut's stated acceptance.
- Run stamps SiteOwner only inside if (m_count > 0) (GoFrame.cs:187). Dc has a GoFrame but reaches no defer, so it throws d unstamped.
- M0's Run catch then sees Catches 2 and SiteOwner 0, so firstHere is false. ReRaisedByTheDeferredDelegate returns false on SiteOwner == 0 (GoFrame.cs:379). splicePanic refuses the whole splice.
- The first cut chained d.Beneath=m and produced Go's exact list.
- The re-cut says 'the one accepted re-raise is the deferred delegate that is itself the first catcher', and Dc is exactly that, but it is refused.
- Frames whose defers are all lowered into the finally (deferFinallyLowering.go) have the same shape, since their m_count is always 0.

On the other axes I agree with the verifier:
- No new allocation. The extra work is one thread-static increment and a 16-byte struct store per deferring Run.
- Catches is incremented only in CaptureThrowSite, from the filters.
- The finally-funclet duplicate handling only gates the decision; the splice is built from SiteTrace, so no real frame is dropped.
- PopSequence runs in the finally on every exit path.
- t_started is GolibReset.

I did not build or run the C#.

**Skeptic residual.** Fixes owed, smallest first:
(a) Make activation identity safe across threads. Options:
- draw activation numbers from per-thread blocks reserved from a static Interlocked counter, or stamp the owning thread as well and require it to match in splicePanic;
- in YieldFunctionEnumerator.rethrowFailure, mark t_started on the ranging thread when the rethrown exception is a GoexitException;
- or clear SiteOwner/SiteEndsAtRun on any cross-thread rethrow.
(b) Either let ReRaisedByTheDeferredDelegate accept a first catcher whose Run registered no defer (m_count 0: an unreached conditional defer, or all defers lowered into the finally), or state M0 and the all-lowered catchers as residuals.
(c) Fix the t_lastActivation census rationale: numbers are unique only within one thread.

Arms owed, each shown red first:
- GX (deterministic): a range-over-func seq calls Goexit and the ranging goroutine's deferred call panics. Expect no splice.
- R1: plant equal t_lastActivation values on the ranging thread and the coro thread, for example by reflection inside seq and before the loop. Expect no splice.
- M0: a deferred delegate with an unreached conditional defer panics while its parent panics. Expect Go's list, or record the residual.

No predicted runtime row reaches these shapes, so the 10-row prediction is unaffected. I did not run the C# suites; every C# outcome above is by reading.



---

## ADDENDUM 2 (2026-09-28): the second verification, at 28fd1e3218

R's third revision: 59f54667e7 RED (19 arms red at e266debe65), 4597c097d8 FIX (the popped delegate's Method is the catcher; a SiteOwnerThread token beside the activation; the fault kind tagged at the raise site; MarkGoroutineExiting; an ends-at-Run site accepted only for the zero-arg nil thunk or the delegate re-raise; and a DESIGN CHANGE refusing every panic a deferred call raises directly, which COORD accepts as the safe direction), and 28fd1e3218 (the residuals amended). 9 agents; go1.24.13 ground truth and standalone net10 repros.

**CLOSED:** B1 (0 false accepts across every shape, at TC0 and after tier-up; generic instantiations can never alias), C1 (the thread token plus the activation make a foreign owner impossible), and B2.

**STILL WRONG (three narrow shapes, each with a fix verified in scratch):**
1. C2: sites raised through builtin.panic, and so tagged Explicit, where Go's panic is runtime-raised: R1's six sites, hashtriemap.cs:608 (ComparingUncomparableType also fixes Go's "runtime error: " text), math/bits Div64/Div (bits.cs:522/525, via a bits_impl.cs hand-own; a Divide tag would keep a frame Go's intrinsic lacks), and panicwrap on a nil box in the go2cs-gen adapter forwarders and the (*T).M wrapper. Raise them Unmodelled, with an arm each expecting no splice. The general rule: builtin.panic is Explicit only when Go's own frames are Go source too.
2. C3: a Goexit in a range-over-func loop body is not carried to the coro. Fix: YieldFunctionEnumerator.Dispose records GoexitException.Started before the switch, and yield() marks the goroutine on resume. A PANIC in the loop body splices on the unmarked coro; refuse any coro splice while m_stopped is set.
3. A zero-argument nil deferred func run after a COMPLETED recovery (no newer panic running) splices without Go's runtime.deferreturn frame. Leave that thunk's panic unstamped, or add deferreturn; DNILAR (a later panic calls the nil func) must stay spliced.

**OWED AS STATEMENTS (the safe direction):** the missing splices the design now makes, named in PREDICTION's residuals: `defer G(args)` through golib's defer<T...> rung, pointer-receiver method groups (the ж twin), value-receiver method values (emitted as lambdas), every generic catcher (StackFrame reports the open definition), and directly deferred named functions and methods. The PREDICTION's "never a partial list" is false for the ruled deferreturn residual; restate it. The tail-call arms guard B1 in the OPTIMIZED (Release) build only; say so on the gate line.

**FOUND IN PASSING, PRE-EXISTING (on master), its own task:** `defer runtime.Goexit()` skips the frame's earlier-registered deferred calls. Run's loop catches only IsPanic, so the GoexitException leaves with defers unrun. Go runs them.

## Appendix C: the second verification's results

### B1: verifier closed

**Evidence.** READ at 28fd1e3218 after a fetch.
- GoFrame.Run sets `deferred = Pop(); deferred();` inside the inner try. Its catch calls `ReRaisedByTheDeferredDelegate(deferred, raised)`, which is now `deferred is not null && Catches == 2 && !SiteEndsAtRun && catcher = SiteTrace[^1].GetMethod() && catcher != RunMethod && deferred.Method == catcher`.
- The old `new StackTrace(ex)` walk and IsMachinery are deleted, so nothing is inferred from the trace any more. `SiteOwner != 0` was dropped, which is what accepts M0.
- An accepted re-raise sets `SiteIsTheDeferredCall`. splicePanic accepts an ends-at-Run link only for that flag or for the zero-arg nil thunk (checked by method).
- The fix commit 4597c097d8 touches no test file. The arms were added red-first in 59f54667e7:
  - ShapeA's closure is now `[AggressiveOptimization]`.
  - New arm `ATailCallingForwarderDeferredIsNotTheCatcher` uses dForward, which is AO and calls the NoInlining gDeferPanics. It expects no splice.
  - New arm `ACatcherWhoseRunRegisteredNoDeferIsStillTheDeferredCall` (M0) expects Go's list.
  - D2 (deferTheCatcher) still expects Go's list.

WHY IT IS SOUND:
- Equal methods plus Catches==2 imply the delegate's own activation caught first. If a deeper activation of the same method had caught first, the delegate target's own emitted filter, or its Run's inner catch, would count a third catch.
- delegate.Method is fixed when the delegate is created, so a tail call cannot change it. A catcher always contains a try region, so the JIT cannot tail-call its frame out of the trace.

MEASURED. A standalone net10 (10.0.11) Release repro in <scratch> MiniGolib\GoFrame.cs copies Run's catch decision and ReRaised verbatim from 28fd1e3218, in a separate "golib" assembly with the ladder rung `frame.Push(() => action(arg))`. The probes use the emission shape (try, `catch when IsPanic`, `finally ᒐ.Run()`) and are unmarked unless named. Outputs are the out-*.txt files beside it.
- At DOTNET_TieredCompilation=0, one call per shape:
  - Refused: the closure `func(){G()}` (unmarked and AO), the forwarder D (unmarked, AO and NI), the M0 variants, and R's exact shapes (AO closure or forwarder calling a NoInlining catcher).
  - The old trace rule would have accepted every one of those except D_NI, because the closure and D frames vanish from the re-raise trace.
  - Accepted, which is Go's list: the direct `defer G()`, G_NI, Gm0 (M0), and the non-generic closure-catcher.
- Default tiering, 3000 calls per shape with sleeps for tier-up: the new rule had 0 false accepts at every call. The old rule accepted unmarked closures and forwarders about 1600 to 1800 of 3000 times after tier-up, and AO shapes 3000 of 3000, including the first call.
- G's frame was never lost in any run: its EH and filter keep it from being inlined, even with PGO.
- So both B1 arms go red if the trace rule comes back, at TC0 and on MSTest's default one-shot tier, because AO is tail-called on the first call.

LADDER: the delegate's Method is golib's `builtin+<>c__DisplayClass2_0<IntPtr>::<defer>b__0`. It never equals the catcher, so `defer G1(1)` is always REFUSED. The wrapped delegate is not unwrapped. This is safe, but it is a missing splice: Go 1.24.13 shows `Ladder.func1 | gopanic | G1 | gopanic | Ladder`.

METHOD VALUES:
- A class-instance delegate or a boxed-struct delegate compares EQUAL, and is correctly accepted.
- The converter does not emit those forms:
  - Value-receiver method values are emitted as lambdas (`() => aʗ1.label()`, MethodValueReceiverSnapshot golden).
  - Pointer-receiver groups bind RecvGenerator's ж twin forwarder (`defer(Ꮡs.release, ref ᒐ)`, DeferCallOrder golden). The repro shows the twin and the ref body, both named M, correctly UNEQUAL.
- Both forms are refused. Go shows `gopanic | main.(*T).PM | gopanic | owner` and `main.T.VM`, the same for a stored method value. Measured plain and with -gcflags=all=-l.

GENERICS: in .NET 10, StackFrame.GetMethod() returns the generic method DEFINITION, `Gg<T>` (IsGenericMethodDefinition=true). For a closure inside a generic function it returns the OPEN type, `<>c__DisplayClass19_0`1[T]`.
- This holds for nint, string, object and class type arguments alike, for exception traces and live walks, at TC0 and after tier-up.
- delegate.Method is always the closed instantiation, even when the delegate is built in shared code (`Gg<String>`).
- So two different instantiations can never compare equal. The matrix was all unequal, and Gg<string> vs Gg<object> is False.
- But one instantiation ALWAYS compares unequal to itself. Every generic catcher, and every closure-catcher inside a generic function, is refused. Go shows `gopanic | main.Gg[...] | gopanic | GenInt` and `OG[...].func2`.
- Across every shape: 0 false accepts.

**Residual.** No correctness hole remains in B1. What is owed is disclosure plus one caveat about the gate leg.
1. These shapes are now missing splices, the safe direction. Go shows `gopanic | catcher | gopanic | owner` for each (measured). None is named in PREDICTION.md's AMENDED residual list; they are only implied by "the deferred delegate that is itself the first catcher":
   - `defer G(args)` through golib's defer<T...> rung. The addendum asked to compare the wrapped delegate's Method, and that was not done.
   - Pointer-receiver method groups, which bind the ж twin forwarder.
   - Value-receiver method values, which are emitted as lambdas.
   - Every generic catcher, because StackFrame reports the open generic definition while delegate.Method is closed.
   The fix is to state them as residuals, or unwrap the ladder closure. Note that unwrapping alone would still refuse generics.
2. The tail-call arms (ShapeA's AO closure and dForward) guard B1 only in an OPTIMIZED build. In a Debug build of the same repro, AggressiveOptimization is ignored (debuggable code), and the old trace rule also refuses, so both arms stay green whichever rule is in place. PREDICTION states GolibTests as "Release and Debug". Only the Release leg carries the B1 guard, and the gate line should say so.
3. Not measured: the real golib at 28fd1e3218, since the repro is a verbatim copy of the decision logic in a mimic, and non-x64 JITs. No C# suite was built or run. Scratch bin/obj folders and caches were purged; sources and outputs are kept in <scratch>

### C1: verifier closed

**Evidence.** Checked by reading the code at 28fd1e3218, then confirmed with Go 1.24.13 ground truth and a tiny net10 repro. The C# suites were not built or run.

1. STAMP AND CHECK
- The owner fields are written in exactly two places: GoFrame.cs:187-191 (the first catch) and :283-286 (the Run loop's catch). Each sets SiteOwner = t_sequences[sequence].Activation and SiteOwnerThread = ThreadToken together.
- A git grep over src finds no other writer of SiteOwner, SiteOwnerThread or t_lastActivation.
- splicePanic (managed_impl.cs:2065) refuses any link unless both of these hold:
  - SiteOwner equals the activation of the Run being walked, read from this thread's t_sequences;
  - ReferenceEquals(SiteOwnerThread, GoFrame.ThreadToken).
- So a link passes only when the Run that stamped it is on this thread and has this number.

2. NUMBERS AND TOKENS ARE UNIQUE
- t_lastActivation has one writer, `++` in PushSequence (:369).
- The pool's ResetForReuse calls GoFuncRoot.ResetThread, which calls ResetSequences. That clears the entries and the depth only. The counter and t_threadToken are never reset, and the census lists both as KeptThreadResource.
- The token is an object, so it is distinct per thread. Any exception that holds it keeps it alive, so it cannot be recycled the way a ManagedThreadId can.
- The net10 repro confirms this: a worker's token differs from the main thread's, and from the token of a new thread started after the worker ended.

3. RANGE-OVER-FUNC HANDOFF
- Only one path hands the same PanicException object to another thread: YieldFunctionEnumerator. Its run() has a plain catch, captures with ExceptionDispatchInfo, and rethrowFailure throws on the ranging goroutine.
- The other rethrow sites are safe:
  - GoFrame's foreignRethrow, and the TargetInvocation unwrap in PanicException.cs:162 and reflect, stay on one thread;
  - sync.oncefunc replays only foreign exceptions;
  - ActionExtensions throws an AggregateException, which is not a panic.
- The handoff runs one way only, coro to ranger. A panic in the loop body disposes the enumerator and never crosses to the coro.
- A link stamped on the coro carries the worker's token, so the ranger refuses it.
- The ranger cannot re-stamp it:
  - firstHere needs Catches==1, but the coro side has already counted one catch;
  - reRaised needs Catches==2 and a popped delegate whose Method is the first catcher. That delegate's body is the catcher's own method, and its emitted catch sits between rethrowFailure and the Run loop, so Catches is at least 3.
- Go 1.24.13, run in my scratch folder, gives these lists:
  - R1: rangerR1.func1|gopanic|panicmem|sigpanic|seqR1|rangerR1
  - Y: F.func1|gopanic|F|F.func2|F
- On a collision, the cut without the token would drop seqR1 from R1, and F plus F.func2 from Y. At the fix both are refused, and PREDICTION.md's amended residuals state that.

4. POOLED-THREAD REUSE
- Yes: the next goroutine on the same worker reuses the same token object. That is by design.
- It cannot match an old panic, because that goroutine's Runs get numbers from the same never-reset counter, so they are all larger than the old stamp. splicePanic needs a live Run with exactly the stamped number, and the stamping Run has already returned.
- The old object also never gets back to its worker. The handoff is one way, and every thread in a range chain is live and blocked while the object is in transit, so none of them can be reused meanwhile.

5. THE PLANTED-COLLISION ARM
- The arm is ASiteOwnedOnAnotherThreadSplicesNothingEvenWhenActivationsCollide, with probes crossThreadOwner and seqDeferringNil.
- Each thread plants t_lastActivation=1_000_000 inside its own deferring function. After that, each pushes exactly one sequence, so both activations are 1_000_001:
  - golib itself has no GoFrame.Run;
  - the park path (casgstatus, readgstatus, getg) has no defers.
- The net10 repro confirms that reflection's SetValue on a [ThreadStatic] field writes only the calling thread's slot.
- The raiser is the zero-argument nil-func thunk, the one ends-at-Run site the fix still accepts (Memory kind, no Go frames, raw[0] == NilDeferredCallMethod). With the token check removed, every other check passes and the arm would read gopanic|panicmem|sigpanic|crossThreadOwner, which is RED.
- The same class's longThunkChain 64 and 65 arms are the positive control: they show the nil-thunk splice path is live.
- R reports the arm red at e266debe65, which is consistent: the collision works.

6. A CROSS-THREAD SITE UNDER A LOCAL STAMP IS CORRECT (Z0)
- Z0 is a seq that panics with no catch of its own. The ranger's own Run on the ranger's thread stamps it.
- The repro shows that new StackTrace(ex) after an ExceptionDispatchInfo rethrow on another thread includes the worker's frames.
- So the splice reads gopanic|seqZ0|rangerZ0, which matches Go. The yield-first variant Z0b matches Go too.

Scratch files: <scratch> and ...\netrepro\Program.cs. bin, obj and caches are purged, and the repo tree is unchanged.

**Residual.** Nothing blocking on C1. Three notes:

1. Minor arm gap. There is no cross-thread arm for a site that ends at the deferring function (the review's shape Y). The token check is one condition in the guard every link shares, so R1 alone goes red if it is removed. A future change that moved the check into one branch would not be caught.

2. No arm resets t_lastActivation on pooled reuse. That case cannot be reached anyway, because the handoff runs one way.

3. Outside C1, already present at every cut, found by reading. A panic raised in a range-over-func LOOP BODY gives a partial splice, which is a wrong splice.
   - Setup: the body is inline in the ranger's foreach, and Callers is read from the ranger's deferred call.
   - The cut gives rangerL.func1|gopanic|rangerL.
   - Go 1.24.13 (run) gives rangerL.func1|runtime.gopanic|main.rangerL-range1|main.seqL|main.rangerL, so rangerL-range1 and seqL are missing.
   - Nothing refuses it: the kind is Explicit, the thrower rangerL is Go source, and site=[rangerL] equals the owner.
   - This contradicts the stated "never a partial list". I found no predicted row that uses it.
   - Fix: refuse the splice when the site's owner is inside a range-over-func loop body (converter or golib marker), or state it as a residual, and add a red-first arm expecting no splice.

### C2: verifier partial

**Evidence.** Read-only against 28fd1e3218 (git diff e266debe65..28fd1e3218, git show, git grep). No C# build. Go lists were measured with go1.24.13 using the probes in <scratch>,iface,refl,rclose}; the Go cache was purged afterwards.

WHERE THE TAG IS SET. FaultKind is an internal init property. Its enum default (value 0) is Unmodelled (PanicException.cs:339). A git grep of all src finds exactly three places that set it:
- builtin.panic sets Explicit (builtin.cs:271).
- RuntimeErrorPanic.NilPointerDereference sets Memory (RuntimeErrorPanic.cs:23).
- RuntimeErrorPanic.IntegerDivideByZero sets Divide (:171).
TryAsPanic maps NullReferenceException and DivideByZeroException through those two factories. It is the only place a CLR exception becomes a panic, and no other catch maps IndexOutOfRange, InvalidCast or Overflow exceptions.

splicePanic:
- refuses Unmodelled for every link, stack-free (managed_impl.cs:2068);
- keeps the stack test only for Explicit (:2083);
- adds panicmem+sigpanic for Memory and panicdivide for Divide.

GOLIB RAISE SITES, all correct:
- Every `new PanicException(...)` keeps the default Unmodelled, so all are refused. This covers:
  - index (array/slice/sslice/string/sstring), SliceBounds (x9), ArrayConversionLength, MakeSlice len/cap, growslice, zero-size ceiling;
  - map nil write (map.cs:310/373/389);
  - channel send-on-closed, close of closed and nil, synctest and timer sites (17);
  - type assertion (builtin.cs:2576, error.cs:284);
  - ComparingUncomparableType and hash-of-unhashable (GoEqualityComparer:91/235);
  - RangeFunctionContinued;
  - the unsafe, native and header refusals, and Goroutine/SyncTestBubble internals;
  - the 18 `new PanicException` sites in runtime *_impl.cs.
- golib's seven builtin.panic callers (SyncTestBubble x6, NativeStructMarshal:207) are Explicit but thrown from non-`_package` golib methods that are large or hold try/lock, so the second guard refuses them. testing's TestExecution sites are refused the same way.
- NilPointerDereference call sites (ж operator~/NilBox Value, nonnil, NativeBox, GoFrame thunk, reflection writeSlot) are genuine sigpanic shapes, and none is refused on kind.

CONVERTER: it emits `throw panic(v)` only for Go's own panic(v) (convCallExpr.go:3004/3007) and for go2cs stubs. Type assertion goes through `._<T>()`, which is Unmodelled.

ARMS:
- PanicFramesVerificationTests.AnUnmodelledRuntimeErrorSplicesNothing has 6 rows (array, string, nil map, double close, failed _<T>, slice bounds). The probe is AggressiveOptimization|NoInlining, so it does not depend on JIT tier. Deleting line 2068 turns all 6 red; reverting to stack inference turns the inlined rows red. The old indexPanic arm is also still there.
- The positive arms plainPanic, nilPointerPanic and divZeroPanic (PanicFramesTests:43/61/66) go red if any modelled tag is lost.

WHAT IS NOT CLOSED (both are wrong splices, not missing ones):
- R1: hand-owned C# inside a `<pkg>_package` class raises Go runtime-frame panics through builtin.panic, so they are tagged Explicit. isGoSourceFrame counts every `_package` class as Go source, so the second guard passes them. Sites: unsafe.cs:923/930 (unsafe.Slice), 1031/1046 (unsafe.String), reflect/value_impl.cs:1703 (Value.Close on a nil chan), 1787 (SetMapIndex on a nil map). go2cs splices `gopanic | unsafe.Slice | owner`; Go gives `gopanic | runtime.panicunsafeslicelen1 | runtime.panicunsafeslicelen | owner`. For SetMapIndex Go gives `gopanic | runtime.mapassign_faststr | reflect.mapassign_faststr0 | reflect.mapassign_faststr | reflect.Value.SetMapIndex | owner`, and for Close `gopanic | runtime.closechan | reflect.chanclose | reflect.Value.Close | owner`; go2cs keeps only the reflect method.
- R2: panicwrap. A nil *T called through a non-empty interface goes through the adapter's `m_box.Value` (NilBox, a StandardBox, throws NilPointerDereference). Through the `(*T).M` method-expression wrapper it goes through `p0.Value` (convSelectorExpr.go:925). Both are tagged Memory. go2cs splices `gopanic | panicmem | sigpanic [| main.(*T).M] | owner`; Go gives `gopanic | runtime.panicwrap | main.(*T).M | owner`, and its panic message is "value method main.T.M called using nil *T pointer".
- Both existed before the fix: at e266debe65 these sites also had no kind and a Go-source thrower, so they spliced the same way. Neither is on the 10-row path.

**Residual.** 1) R1, six sites: raise them as Unmodelled, by `new PanicException(...)` or a RuntimeErrorPanic factory, not builtin.panic. The sites are unsafe.cs:923/930/1031/1046 and reflect/value_impl.cs:1703/1787. Go's unsafe texts are "unsafe.Slice: len out of range" / "unsafe.Slice: ptr is nil and len is not zero" and the unsafe.String pair; its values satisfy runtime.Error; go2cs's "len is negative" diverges separately. Add one arm, say unsafe.Slice with len -1 in a deferring probe, expecting NO splice. The general hazard: the Explicit second guard cannot see hand-owned `_package` code. Audit any future hand-own that reproduces a runtime-raised panic text.
2) R2: raise panicwrap as its own Unmodelled panic, with Go's message, when the box is nil. This applies in the go2cs-gen adapter forwarders that use `m_box.Value` and in the `(*T).M` method-expression wrapper. Add an arm with a nil *T through an interface, value-receiver method, expecting NO splice. Go's list was measured: `gopanic | runtime.panicwrap | main.(*T).M | owner`.
3) Minor: no arm exercises the Explicit second guard (a golib or host explicit panic refused). Deleting managed_impl.cs:2083 would stay green. It is load-bearing for testing's TestExecution panics and SyncTestBubble.
Nothing modelled is refused on kind. Closure-raised Memory/Divide sites are refused only by the announced design change.

**Skeptic: partial.** Read-only at 28fd1e3218 after a fetch: git diff from e266debe65, git show and git grep. No C# was built. Go lists were measured with go1.24.13 windows/amd64 using my own probes in <scratch>,probe2}\main.go. GOCACHE and TEMP pointed at that folder and were purged afterwards; nothing was written to C:.

THE FIXED PART HOLDS, AND AN ARM WOULD CATCH ITS RETURN.
- FaultKind is init-only, and its default is Unmodelled (PanicException.cs:339). A git grep of all src finds exactly three places that set it: builtin.cs:271 (Explicit), RuntimeErrorPanic.cs:23 (Memory) and RuntimeErrorPanic.cs:171 (Divide).
- There is no subclass and no copy of a PanicException. The range-over-func rethrow re-raises the same instance, so it keeps its kind.
- TryAsPanic is the only place a CLR exception becomes a panic: NullReferenceException becomes Memory and DivideByZeroException becomes Divide. The only other CLR catch in golib is Goroutine.cs:1026, which parses a number and raises no panic.
- Every `new PanicException` keeps the default kind, so it is refused at managed_impl.cs:2068 before the trace is read. That is 57 sites in golib and 17 in runtime's *_impl.cs (the verifier said 18).
- golib's 7 builtin.panic callers are SyncTestBubble x6 and NativeStructMarshal:207. testing's TestExecution is in go.testing_runtime, not a `_package` class. The second guard at :2083 refuses all of them.
- The Memory sites are genuine sigpanic shapes: the ж operator~ and nil-box Value, nonnil, NativeBox, slice RefuseNoUserMemory, the GoFrame thunk, GoReflect writeSlot, and runtime memmove/memequal/memclr.
- No regression: at e266 both builtin.panic and `new PanicException` had kind None. The refusal is now strictly wider for unmodelled panics, the same for builtin.panic, and unchanged for Memory and Divide.
- Arms, by reading:
  - With line 2068 deleted, nothing else checks for an Unmodelled panic. All 6 rows of PanicFramesVerificationTests.AnUnmodelledRuntimeErrorSplicesNothing would then splice gopanic, so they go red whatever the JIT tier.
  - plainPanic (an Explicit arm), nilPointerPanic and divZeroPanic go red if a modelled tag is lost.
  - The old indexPanic arm is still there.

NOT CLOSED. Each of these is a wrong splice, not a missing one: a panic whose Go form passes through an unmodelled frame is tagged Explicit or Memory.
- R1 confirmed at all six sites (unsafe.cs:923/930/1031/1046, reflect/value_impl.cs:1703/1787). unsafe_package and reflect_package are `_package` classes in namespace go. My own Go lists:
  - unsafe.Slice(&b, -1): `gopanic | runtime.panicunsafeslicelen1 | runtime.panicunsafeslicelen | owner`
  - unsafe.String(nil, 3): `gopanic | runtime.panicunsafestringnilptr | owner`
  - Value.Close on a nil chan: `gopanic | runtime.closechan | reflect.chanclose | reflect.Value.Close | owner`
  - SetMapIndex on a nil map: `gopanic | runtime.mapassign_faststr | reflect.mapassign_faststr0 | reflect.mapassign_faststr | reflect.Value.SetMapIndex | owner`
- R2 confirmed, with one caveat:
  - The method expression `(*T).M` always gives `gopanic | runtime.panicwrap | main.(*T).M | owner`, with the message "value method main.T.M called using nil *T pointer". go2cs goes through p0.Value in the wrapper (convSelectorExpr.go:925) and tags it Memory.
  - Through an interface, Go's answer depends on its compiler. With `var i I = p; i.M()` Go devirtualizes the call and gives `gopanic | panicmem | sigpanic | owner`, which is exactly go2cs's answer. With the interface returned from a noinline function it gives the panicwrap list. The adapter's `m_box.Value` (ImplementGenerator.cs:559) is wrong for the usual case.
- NEW, same class, missing from the verifier's list:
  (a) internal/sync/hashtriemap.cs:608 (mustBeComparable). It is hand-owned in `go.internal.sync_package` and raises `throw panic("comparing uncomparable type ...")`, so it is tagged Explicit and accepted. User code reaches it through sync.Map.CompareAndSwap or CompareAndDelete when the stored value is a []int.
    - Go, CompareAndSwap: `gopanic | runtime.efaceeq | runtime.nilinterequal | internal/sync.(*entry[...]).lookupWithValue | internal/sync.(*HashTrieMap[...]).find | internal/sync.(*HashTrieMap[...]).CompareAndSwap | sync.(*Map).CompareAndSwap | owner`
    - Go, CompareAndDelete: `... | runtime.efaceeq | runtime.nilinterequal | internal/sync.(*entry[...]).compareAndDelete | internal/sync.(*HashTrieMap[...]).CompareAndDelete | sync.(*Map).CompareAndDelete | owner`
    - The go2cs message also lacks Go's "runtime error: " prefix, and its value is not a runtime.Error.
  (b) Converted math/bits: bits.cs:522/525 (`panic(divideError)` and `panic(overflowError)` in Div64, which Div reaches too). Both are Explicit Go-source panics and are accepted. But Go on amd64 replaces Div64 with an intrinsic (intrinsics.go:1191-1201, and Div is an alias on ArchAMD64).
    - Measured: Div64(0,1,0) gives `gopanic | runtime.panicdivide | owner`; Div64(2,0,1) gives `gopanic | runtime.panicoverflow | owner`; Div(0,1,0) gives `gopanic | runtime.panicdivide | owner`.
    - go2cs splices `gopanic | math/bits.Div64 [| math/bits.Div] | owner`.
    - Div32's overflow is not an intrinsic, and it matches Go: `gopanic | math/bits.Div32 | owner`.
    - This is the converted-code path the item asked about.
  (c) Refusal paths in hand-owned code that Go treats as a fault, raised as Explicit: unsafe.cs:587 (Pointer.LoadThrough "nil pointer dereference") and runtime/hash_impl.cs:266 (a nil pointer with a non-zero size). Go takes sigpanic in both. Few callers can reach them.
- Converter emission is otherwise clean:
  - `throw panic(v)` only for Go's own panic (convCallExpr.go:3004/3007) and the "go2cs:" stubs (visitFuncDecl.go:2433).
  - It never emits RuntimeErrorPanic or `new PanicException` directly.
  - `._<T>()` is Unmodelled.
- Minor: no arm reaches the second guard. No PanicFrames* test raises a golib or host explicit panic, so deleting :2083 would stay green.

**Skeptic residual.** 1) Raise these as Unmodelled (a RuntimeErrorPanic factory or `new PanicException`), not through builtin.panic:
   - R1's six sites.
   - hashtriemap.cs:608. RuntimeErrorPanic.ComparingUncomparableType also fixes Go's "runtime error: " text.
   - Optionally unsafe.cs:587, hash_impl.cs:266, and the go2cs-only refusal texts raised through builtin.panic in hand-owned `_package` code (unsafe.cs:546/573/584/595 and hash_impl.cs's refusals). Go has no such panic, so tagging them "Go's panic(v)" makes a false claim.
2) math/bits Div64 and Div (bits.cs:522/525): raise Unmodelled, through a hand-own in bits_impl.cs. A Divide tag would still be wrong: the site would keep a math/bits.Div64 frame that Go's amd64 intrinsic does not have.
3) R2: raise panicwrap as its own Unmodelled panic, with Go's message, when the box is nil. This applies in the go2cs-gen adapter forwarders that use `m_box.Value` and in the `(*T).M` method-expression wrapper.
4) Arms, each in a deferring probe expecting NO splice:
   - unsafe.Slice(&b, -1)
   - sync.Map CompareAndSwap on a stored []int
   - bits.Div64(0,1,0)
   - a nil *T through the method expression `(*T).M`
   - plus one arm for the Explicit second guard, for example a golib explicit raiser such as SyncTestBubble.Wait outside a bubble.
5) The general rule these break: "builtin.panic = Explicit" is right only when Go's own frames are Go source too. Hand-owned `_package` code and compiler-intrinsic converted Go both break it, and the second guard sees neither. Audit any hand-own that reproduces a runtime-raised panic.
None of this is on the 10-row path. Every item existed at e266debe65 as well. Nothing modelled is refused on kind; closure-raised Memory and Divide sites are refused only by the announced design change.

### C3: verifier partial

**Evidence.** Method: I read git diff e266debe65 28fd1e3218 and the files at the tip. I ran Go ground truth on go1.24.13 windows/amd64, with and without -gcflags=all=-l; both gave identical lists. I built a standalone net10 repro against a golib.dll compiled from a git archive of the tip, with the assembly named GolibTests so it gets golib's InternalsVisibleTo access. Inside the recoverer it evaluates splicePanic's per-link clauses (managed_impl.cs:2062-2091) against the live state: SequenceFromTop(0), ThreadToken, FaultKind, the raw SiteTrace and the ends-at-Run clause. Results were identical at the default tiering and at DOTNET_TieredCompilation=0. I did not run the C# suites. All scratch bin/obj folders and caches have been purged, and the repo is untouched.

WHAT IS CLOSED
1. **runtime.Goexit.** managed_impl.cs:743 throws `new GoexitException()`, and the constructor calls MarkGoroutineExiting (GoexitException.cs:43). iter.Pull is converted Go and re-raises seq's Goexit through runtime.Goexit() in next and stop (iter.cs:326 and :345), so the caller's thread is marked too. The arm is ANilDeferredFuncFaultingDuringGoexitSplicesNothing.
2. **The test host.**
   - TestExecution.FailNow sets the mark at :349 and SkipNow at :367, immediately before `throw new TestAbortException()`.
   - It is set only after TryEnsureOwner passes. It is not set if Fail() panics first, or when a non-owner call returns without unwinding.
   - The InOwnersBubble branches (:341 and :360) throw GoexitException, so the constructor marks.
   - T.Fatal, Fatalf, Skip, Skipf and SkipNow all call execution.FailNow or SkipNow (testing.cs:285-333). The TB adapter forwards to the same calls, and B and F are no-ops.
   - The arm is ANilDeferredFuncFaultingDuringTheTestHostsGoexitSplicesNothing, with FailNow and SkipNow rows run through a real child TestExecution thread.
3. **The range-over-func rethrow.** rethrowFailure marks the ranging thread when SourceException is a GoexitException (YieldFunctionEnumerator.cs:129-130). Both re-raise points go through it: MoveNext (:84) and Dispose (:149). A seq cannot carry a TestAbortException across, because t.FailNow on the coro thread fails TryEnsureOwner and does not throw.
   - The arm is ANilDeferredFuncFaultingDuringARangeFuncsGoexitSplicesNothing.
   - I confirmed it would go red: FWD-GX refuses (Started=True, SiteOwner=0). A mutant that clears the mark before the nil thunk runs gives Started=False, SiteOwner=activation, EndsAtRun, ACCEPTS.
4. **Goexit inside a deferred call.** The constructor marks whichever thread runs the deferred call, since GoFrame.Run invokes defers inline. If the call belongs to a seq, rethrowFailure carries the mark across. Run's catch is the only place the mark is read (GoFrame.cs:281, `running is null && Started`). A link raised unstamped under the Goexit refuses any chain above it at splicePanic:2065.
5. **Scoping.**
   - Pooled coro workers reset after every body: GoroutineThreadPool.cs:140, then ResetForReuse, then GoFuncRoot.ResetThread (:71), then GoexitException.ResetThread.
   - Goroutines get a new Thread each (Goroutine.cs:830), and so does each test or subtest (TestExecution.cs:201-232).
   - The census arm plants t_started=true after a body (ThreadStateCensusTests.cs:317 and :347).
   - The repro confirmed it: a Goexiting seq's worker was reused for the next seq and read Started=False.
   - All four marking sites set the mark immediately before a throw. The only Go code that then runs marked is Goexit-time code: defers, and the test cleanups after FailNow, which Go also runs under Goexit. So any extra refusal is safe.

WHAT IS STILL OPEN: A GOEXIT IN THE LOOP BODY IS NEVER MARKED ON THE CORO THREAD
This is the mirror of the path R fixed. R's own doc comment on MarkGoroutineExiting says every Goexit that unwinds "on a different thread than the one that raised it" must mark.
- runtime.Goexit, or t.FailNow / SkipNow / Fatal / Skip, can be raised in a range-over-func loop body on the ranging goroutine.
- The foreach's Dispose (YieldFunctionEnumerator.cs:140-150) then sets m_stopped and switches. seq's yield returns false (:117), and seq's deferred calls run as a normal return on the coro worker, which is not marked.
- Both ends-at-Run shapes the splice still accepts are then spliced: the nil-func thunk, and the `defer D()` delegate re-raise.

Go 1.24.13 (gorev/main.go):
- RB-Goexit/nil: `seqNil.func1 | runtime.gopanic | runtime.panicmem | runtime.sigpanic | runtime.Goexit:636 | RBGoexit-range1 | seqNil | RBGoexit | runtime.goexit`
- RB-Goexit/named: `seqNamed.func1 | gopanic | dNamed | runtime.Goexit | RBGoexit-range1 | seqNamed | ...`

The repro at the tip:
- RB-Goexit/nil, RB-FailNow/nil (the body does exactly what FailNow does at lines 349-350), RB-Goexit/named and RB-FailNow/named all give: coro thread, Goexit.Started=False, SiteOwner=activation, thread token matches, EndsAtRun=True, Memory with first frame == NilDeferredCallMethod (or IsTheDeferredCall=True for named), result ACCEPTS.
- So by reading, go2cs gives `seqNil.func1 | gopanic | panicmem | sigpanic | seqNil | ...`. That drops runtime.Goexit and the range body between sigpanic and seqNil: a partial list, which the stated residual promises never happens.
- For the named shape, the repro did not evaluate the Explicit second guard, because isGoSourceFrame lives in runtime. In the corpus dNamed is a Go-source frame, so that guard passes.
- Control: RB-Break gives the identical state. There Go really does splice straight onto seq (`sigpanic | seqNil:33 | RBBreak`), so the coro side currently cannot tell a break from a Goexit.
- No arm covers this. The shape was already spliced this way at e266debe65, so it is not a regression from R's fix, but C3 is not closed.
- None of the 10 predicted runtime rows reaches this shape.

**Residual.** 1. OPEN (C3): Dispose needs to carry the Goexit to the coro side. Suggested fix:
   - In YieldFunctionEnumerator.Dispose, read GoexitException.Started on the ranging thread into a field before m_coro.Switch(). A body Goexit, or the host's FailNow/SkipNow, has already set it at this point.
   - In yield(), when it resumes with m_stopped set and that field true, call GoexitException.MarkGoroutineExiting() on the coro thread.
   - The worker is reset after the body, so scoping still holds.
   Arms, red first:
   - RB-Goexit/nil: the body throws GoexitException; seq defers a recoverer and then a nil func. Expect NO splice.
   - The same with the body doing MarkGoroutineExiting + throw, or a real child TestExecution calling FailNow and SkipNow.
   - RB-Break/nil as the control: expect Go's `gopanic | panicmem | sigpanic | seqNil`, so the fix cannot pass by refusing everything.
   Either fix it, or add it to PREDICTION.md's Goexit residual together with its arm.
2. OUT OF C3's SCOPE, same mechanism: a PANIC in the loop body.
   - Go: `seqNil.func1 | gopanic | panicmem | sigpanic | runtime.gopanic | RBPanic-range1 | seqNil | RBPanic | main`.
   - The repro also gives ACCEPTS on the unmarked coro thread, with running null. By reading, go2cs gives `... sigpanic | seqNil`, dropping `gopanic | RBPanic-range1`.
   - The body's panic is never handed to seq's Runs as the panic they are running. This belongs with the cross-thread items (C1) and should be stated or refused the same way.
3. Minor, safe: Fatal, Fatalf, Skip and Skipf have no arm of their own. They are one-line wrappers over the armed FailNow/SkipNow (testing.cs:289-330). The earlier safe over-refusal (a nested normal-return splice under a Goexit comes back missing, never wrong) still stands.
Files: <scratch> (Go ground truth) and <scratch> (net10 repro; bin/obj purged).

**Skeptic: partial.** Method. I read the addendum on origin/claude/coord-handover 886eb16694, git diff e266debe65 28fd1e3218, and the files at 28fd1e3218, after a fetch. I ran my own Go programs on go1.24.13 windows/amd64, with and without -gcflags=all=-l (same lists both ways), plus go test for the test-host forms. I built golib from a git archive of the tip, and a separate net10 console repro (assembly named GolibTests, so it gets golib's internals). Its Probe.Callers repeats captureCallers' walk and splicePanic's per-link rules (managed_impl.cs:1900-1979 and 2050-2125) exactly, over the live stack, so each shape prints the list go2cs would give. I ran it at default tiering and at DOTNET_TieredCompilation=0; the results were identical. I did not build or run the C# suites. The repo is untouched and all bin/obj/build/tmp folders are purged.

CLOSED (checked by reading, and by the repro where marked):
1. runtime.Goexit. The GoexitException constructor calls MarkGoroutineExiting (GoexitException.cs:43). Repro: the same-thread Goexit with a nil deferred func is REFUSED, with Started=True.
2. The test host. TestExecution.FailNow sets the mark at :349 and SkipNow at :367, just before `throw new TestAbortException()`. The InOwnersBubble branches throw GoexitException, so the constructor marks there. T.Fatal, Fatalf, Skip and Skipf all go through execution.FailNow or SkipNow (testing.cs:285-333). Arm: ANilDeferredFuncFaultingDuringTheTestHostsGoexitSplicesNothing.
3. seq's Goexit carried to the ranging goroutine. rethrowFailure marks when SourceException is a GoexitException (YieldFunctionEnumerator.cs:129-130). Both MoveNext and Dispose go through it. Repro: FWD GX is REFUSED, with Started=True on the ranging thread. Arm: ANilDeferredFuncFaultingDuringARangeFuncsGoexitSplicesNothing.
4. Goexit inside a deferred call. The constructor marks the thread that runs the deferred call, which is the goroutine's own thread, because Run calls defers inline.
5. Scoping.
   - Pooled workers reset after every body: GoroutineThreadPool.cs:140, then ResetForReuse, then GoFuncRoot.ResetThread:71, then GoexitException.ResetThread.
   - Goroutines get a new Thread each (Goroutine.cs:830), and so does each test.
   - Repro: a worker that ran a seq which called Goexit was reused for the next seq and read Started=False.

OPEN, confirmed independently: a Goexit raised in a range-over-func LOOP BODY is never marked on the coro thread. The body runs on the ranging thread, so the mark lands there. The foreach's Dispose (YieldFunctionEnumerator.cs:140-150) then sets m_stopped and switches to the coro. There seq's yield returns false (:117), and seq's defers run as an ordinary normal return on an unmarked worker. Both site shapes the splice still accepts at a Run are then spliced: the zero-arg nil thunk, and the `defer D()` delegate re-raise.

Go 1.24.13 (my gotruth/main.go and body_test.go):
- A goexit: seqA.func1 | gopanic | panicmem | sigpanic | runtime.Goexit | goexitBody-range1 | seqA | goexitBody
- B goexit: seqB.func1 | gopanic | dB | runtime.Goexit | goexitBody-range1 | seqB
- FailNow: ... sigpanic | runtime.Goexit | testing.(*common).FailNow | TestFailNowBody-range1 | seqA
- SkipNow: the same shape with SkipNow.
- Fatal on B: ... dB | runtime.Goexit | FailNow | Fatal | range1 | seqB
- Control, A break: ... sigpanic | seqA | breakBody

go2cs at the tip (repro, both tierings):
- A goexit, A failnow, A skipnow: `seqA.func | gopanic | panicmem | sigpanic | seqA`, on the coro thread with Goexit.Started=False, spliced.
- B goexit, B failnow: `seqB.func | gopanic | dB | seqB`, spliced.
- A break and B break give exactly the same lists.

So the splice drops runtime.Goexit, the test host's frame and the range body between the fault frames and seq: a partial list. That breaks R's amended PREDICTION line, "A deferred call's panic during ANY Goexit ... splices NOTHING, never a partial list". No arm covers this direction; every Goexit probe raises the Goexit on the ranging side or inside seq, never in the body. It is not a regression from R's fix: e266debe65 accepted the same two site shapes. None of the 10 predicted rows reaches it.

The fix works in a scratch experiment. I patched a copy of golib: Dispose records GoexitException.Started into a field before m_coro.Switch(), and yield calls MarkGoroutineExiting when it resumes with m_stopped set and that field true. With that patch:
- A goexit, A failnow, A skipnow, B goexit and B failnow are all REFUSED.
- A break and B break still splice Go's lists.
- The scoping check still reads Started=False.
So an arm built this way would read red at the tip and green with the fix.

Scratch files: <scratch> (main.go, body_test.go, gd\main.go), repro\Program.cs (tip) and reprofix\Program.cs (the same source, built against the patched golib).

**Skeptic residual.** 1. OPEN, C3's own scope: a Goexit in a range-over-func loop body is not carried to the coro. This covers runtime.Goexit, and t.FailNow, SkipNow, Fatal and Skip on the test's thread.
   - Fix, verified in scratch: in YieldFunctionEnumerator.Dispose, record GoexitException.Started into a field before m_coro.Switch(). In yield(), when it resumes with m_stopped set and that field true, call GoexitException.MarkGoroutineExiting(). The pooled worker's reset keeps this scoped.
   - Arms, red first. All run with seq deferring a recoverer that reads Callers, and then either a nil func or `defer dB()` (dB defers and panics):
     - Body throws GoexitException: expect no splice.
     - Body does MarkGoroutineExiting and then throws, or a real child TestExecution calls FailNow and then SkipNow: expect no splice.
     - Control, body breaks: expect Go's `gopanic | panicmem | sigpanic | seqA` and `gopanic | dB | seqB`, so the fix cannot pass by refusing everything.
   - If R does not fix it, state it in PREDICTION.md's Goexit residual with an arm that pins the refusal.
2. Same mechanism, outside C3's wording: a PANIC in the loop body.
   - Go: `seqA.func1 | gopanic | panicmem | sigpanic | runtime.gopanic | panicBody-range1 | seqA`.
   - The tip gives `... sigpanic | seqA`, spliced on the unmarked coro. The body's panic is never handed to seq's Run as the panic it is running.
   - Dispose cannot tell this from a break, so it needs either a stated residual or a refusal: for example, refuse any splice on the coro while m_stopped is set. That would also make the break case a missing splice.
3. Pre-existing, found in passing (it is on origin/master too), and a Go-semantics gap, not a splice one: `defer runtime.Goexit()` skips the frame's earlier-registered deferred calls.
   - Run's loop catches only IsPanic, so the GoexitException leaves the loop with defers still unrun.
   - Go runs them: `defer func(){ got = "ran" }(); defer runtime.Goexit()` gives "ran" in Go, and in go2cs the earlier defer never runs.
   - The mark is still right for the outer frames, which refuse safely. This needs its own task, not this seat.
4. Minor and safe: Fatal, Fatalf, Skip and Skipf have no arm of their own. They are one-line forwarders to the armed FailNow and SkipNow.

### B2: verifier closed

**Evidence.** Read at 28fd1e3218 after a fetch (git diff e266debe65 28fd1e3218, git show of GoFrame.cs, PanicException.cs, managed_impl.cs, builtin.DeferRegistrations.cs, visitDeferStmt.go, the arms and PREDICTION.md). Nothing in the repo or in tG was touched.

THE GATE, BY READING
- managed_impl.cs:2087-2089 (splicePanic, in the per-link loop) refuses the whole splice when `link.SiteEndsAtRun && !link.SiteIsTheDeferredCall && !(site.Count == 0 && FaultKind == Memory && firstMethodOf(raw) == GoFrame.NilDeferredCallMethod)`.
- It returns [] for the whole splice, never a partial list.
- There is exactly one writer of either flag: GoFrame.cs:285-286 in Run's catch, which sets SiteEndsAtRun = true and SiteIsTheDeferredCall = reRaised.
- reRaised is computed only when !firstHere. It requires `deferred.Method == site[^1].GetMethod()`, where `deferred` is the delegate Run just popped.
- A stamped link is never stamped again: firstHere needs SiteOwner 0, and ReRaised returns false once SiteEndsAtRun is set.
- So an ends-at-Run site passes only in two cases: when the thunk's method is the site's first raw frame, or when the popped delegate's own method is the first catcher.
- NilDeferredCallMethod is s_nilDeferredCall.Method (GoFrame.cs:406). Push(null) is the only route to that thunk, and the ladder's only direct `frame.Push(action)` is the nullary rung (DeferRegistrations:57). Every argument rung pushes a golib closure.

MEASURED
- I built the real golib from `git archive 28fd1e3218` under <scratch>
- The net10 console repro is an assembly named GolibTests (so golib's InternalsVisibleTo applies) at ...\b2wrap\repro\Program.cs.
- It copies captureCallers, splicePanic, firstMethodOf and isGoSourceFrame VERBATIM from the tip. The only change is that PC interning is replaced by names.
- A negative control, GateOff, removes only the ends-at-Run gate.
- Configurations: default tiering, DOTNET_TieredCompilation=0, and default tiering warmed 3000x.
- All three configurations gave identical verdicts. Outputs are in ...\b2wrap\out\{default,tc0,warm3000,go}.txt.
- Go ground truth is go1.24.13, run plain and with -gcflags=all=-l (identical), from ...\b2wrap\go\main.go.

Nil thunk (still Go's list, in every tier):
- In the thunk's trace, raw=[GoFrame+<>c.<.cctor>b__32_0, GoFrame.Run], and first==NilDeferredCallMethod is True.
- N gives gopanic|panicmem|sigpanic|gopanic|N. That matches Go: N.func1|gopanic|panicmem|sigpanic|gopanic|N.
- NR (TestCallersDeferNilFuncPanic's shape) gives gopanic|panicmem|sigpanic|NR, which matches Go.
- N3, three thunks, gives (gopanic|panicmem|sigpanic)x3|gopanic|N3, which matches Go.
- The converted callers_test.cs emits `defer(fʗ1, ref ᒐ)`, the same path as NR, so the predicted row keeps its mechanism.

Nil func with arguments (now refused, in every tier):
- NA, NAR, NA2 and NAF (the Func<T,TResult> rung) all have raw[0] = builtin+<>c__DisplayClass*.<defer>b__0, and the tip splices nothing.
- With the gate off they splice e266's wrong list: NA gives gopanic|panicmem|sigpanic|gopanic|NA, where Go has sigpanic|NA.deferwrap1|gopanic|NA.
- Converter lambdas are refused too:
  - NT is the temp-param `ᴛ1 => fn(ᴛ1)`. At TC0 the JIT tail-called the golib closure out of its trace, and it is refused anyway.
  - NC and NCR are `() => c()`.
  - NF is `() => f()` for a func() int. Go shows NF.deferwrap1 there.
- With the gate off, each of these splices the lambda as an extra frame.

WOULD AN ARM CATCH ITS RETURN? Yes.
- AssertNoSplice requires below == [owner].
- Removing the gate reproduces the wrong lists above. That reddens ANilFuncWithArgumentsWhilePanickingSplicesNothing, ...OnANormalReturnSplicesNothing and ANilNamedFuncTypeThroughTheConvertersLambdaSplicesNothing.
- Relaxing the thunk check to "any site-less Memory link" re-accepts NA and NAR, and those arms go red.
- The thunk's acceptance is pinned with exact lists by these arms, all unchanged: ANilDeferredFuncFaultsFromTheDeferringFunctionsExit, ANilDeferredFuncFaultingWhileAPanicRunsKeepsTheOlderGopanic, ALongChainThroughASmallBufferFillsFromTheTop and A65LinkChainIsSplicedWhole.
- Every one of these gate-off readings agrees by reading with the e266 splice, which confirms the RED commit's claim for NA, NAR and NC.

**Residual.** None of these is a wrong splice, and none touches the 10 predicted rows.

(1) The PREDICTION wording is narrower than the code. The 2026-09-28 amendment says "EVERY panic a deferred call raises from a closure". The gate actually refuses every panic a deferred call raises directly, including a named function or method value deferred as a method group: `defer g()`, `defer g1(1)`, `defer t.m()`.
- Go 1.24.13 (measured): DG gives DG.func1|gopanic|g|gopanic|DG; DGR gives ...|gopanic|g|DGR; DG1R gives ...|gopanic|g1|DG1R.
- e266 matched these. My gate-off control reproduces Go's list exactly, and the C3 verifier had marked them "checked and found fine".
- The tip splices nothing for all three, in every tier. That is a safe-direction missing splice, but it should be named in the residuals.

(2) NC is a determinable missing splice. For a nil named-func-type value called with no arguments, Go's list is the SAME as the thunk's: NC.func1|gopanic|panicmem|sigpanic|gopanic|NC, with no deferwrap. go2cs refuses only because the converter wraps the call in `() => c()`. This fits the stated "until the converter marks its wrappers", and emitting a delegate conversion instead would recover it.

(3) There is no dedicated arm for the Func<T,TResult> rung, the temp-param lambda or the `() => f()` results lambda. All three go through the same single gate that the NA, NAR and NC arms cover.

(4) As instructed, the real GolibTests arms were not built or run. The repro runs real tip golib, but its splice is a verbatim copy of the tip's managed_impl.cs, not the runtime assembly itself. It also names frames by CLR name rather than by goFrameName. Scratch bin/obj and caches were purged.

### THE: verifier partial

**Evidence.** Everything was read at 28fd1e3218 after a fetch, with `git diff e266debe65 28fd1e3218` and `git show`. No C# was built or run. Go lists come from go1.24.13 windows/amd64, run both plain and with `-gcflags=all=-l`, and the two runs gave the same lists. The probes are <scratch> and dc\b\main.go (the Go cache was purged, no bin/obj was created, and the repo is untouched).

(1) THE REFUSAL ONLY EVER MAKES A SPLICE MISSING.
- The whole change in splicePanic (managed_impl.cs ~2087) is one added `return []`: `if (link.SiteEndsAtRun && !link.SiteIsTheDeferredCall && !(site.Count==0 && FaultKind==Memory && firstMethodOf(raw)==GoFrame.NilDeferredCallMethod)) return [];`.
- `git grep` finds no reader of SiteEndsAtRun, SiteIsTheDeferredCall, Beneath or SiteOwnerThread other than splicePanic.
- splicePanic has one caller, captureCallers, reached from Callers and from callers() (Caller). On `[]` it simply continues the live walk. Its Run-to-sequence pairing (sequencesMet++) does not depend on the splice result.
- Beneath is set only together with SiteEndsAtRun, in Run's inner catch. Every link is checked before any frame is emitted, so refusing one link refuses the whole chain and can never leave a partial list.
- The accepted set is a subset of e266's: the nil thunk was already accepted as site-less, and the re-raise is now decided by method.
- An arm would catch its removal. Six arms now expect no splice: normalReturnPanic, panicAfterRecovery, deferPanicArg, twoLinkChain, recoverThenPanic and replacedPanic. So do the NA, NAR and NC arms.

WHAT KEEPS THIS PARTIAL: the design keeps the nil-thunk exception, and it still splices a WRONG list. It was already wrong at a419543c14 and e266debe65; the design change did not introduce it.
- NILAR: `var f func(); defer f()` runs after a completed recovery, in an open-coded function.
  - Go: `NILAR.func1 | gopanic | panicmem | sigpanic | runtime.deferreturn | NILAR`
  - The cut, by reading: SetSequence(null) makes running null, so firstHere holds, the exception accepts the link, and the splice is `gopanic | panicmem | sigpanic | NILAR`.
  - InheritThrowSite touches only PanicTrace, so this is exactly the path that ANilDeferredFuncFaultsFromTheDeferringFunctionsExit asserts.
- NILAR2 (two nil funcs after a recovery) and NILARL (the loop form) also lack deferreturn.
- NILNR, the same function without the recovery, shows no deferreturn. So the recovery causes it, not open-coding. The ruled open-coding hint (DESIGN-panic-stack-frames: no loop, 8 defers or fewer, returns×defers 15 or fewer) cannot close it. It is not stated anywhere, and no arm covers it.
- The ruled normal-return shapes also splice a partial list: NILL (WithLoop), NIL9 (more than 8 defers) and NILRET (returns×defers over 15). PREDICTION says the residuals "splice NOTHING, never a partial list", which is wrong for these.
- The re-raise exception is right everywhere I measured, because Go shows deferreturn only when the nil call itself faults:
  - DDNR, DDL, DDAR: `gopanic | D | owner`
  - DDP: `gopanic | D | gopanic | DDP`
  - DCDP: `gopanic | DCDP.func2 | gopanic | DCDP`
  - DCDNR: `gopanic | DCDNR.func2 | DCDNR`

(2) NONE OF THE 10 ROWS NEEDS A REFUSED LINK. Read from the converted callers_test.cs and stack_test.cs:
- TestCallersPanic, NilPointerPanic, DivZeroPanic: the body raises the panic (f1→f2→f3, `p.Value`, `5/n`). The test's own emitted catch catches it first, so it is stamped at Run entry with SiteEndsAtRun false.
- TestCallersDoublePanic: a deferred closure (func1) raises p2, but func1 defers, so its own catch is first (Catches 1) and func1's Run stamps p2 at entry. p1 stays the test's entry because func1 has not returned. That gives two ends-at-owner links: `func1.1 | gopanic | func1 | gopanic | Test`.
- TestCallersDeferNilFuncPanic: the emission is `defer(fʗ1, ref ᒐ)` with a null Action. The nullary rung calls `frame.Push(action)`, which becomes s_nilDeferredCall. The site is empty, the kind is Memory and raw[0] is the thunk, so the exception accepts it. The test checks only up to sigpanic.
- Both /CallersFrames subtests: the NRE comes from `(p0) => p0.M()` or `(p0) => nop(p0.Value)` under cb(), and the t.Run literal's own catch catches it first. /Stack uses runtime.Stack, which does not go through captureCallers.
- The rows predicted to stay still stay:
  - AfterRecovery and AbortedPanic2 read a null entry.
  - AbortedPanic's p2 is refusable, but func3 recovers it and SetSequence(null) runs before the reader.
  - FromWrapper has no panic.
  - WithLoop stays FAIL (the partial list without deferreturn).
- The wording "every panic a deferred CLOSURE raises" is broader than the code. The code refuses only sites that end at the Run, and DoublePanic survives because of that.

(3) SPLICES GIVEN UP (they matched Go at e266; Go's lists as measured):
- NR and NRL: `NR.func1 | gopanic | NR.func2 | NR`. The loop form is the same, with no deferreturn.
- AR: `AR.func1 | gopanic | AR.func2 | AR`
- RP: `RP.func1 | gopanic | RP.func2 | gopanic | RP`
- REPL (the replaced panic): `REPL.func2 | gopanic | REPL.func3 | gopanic | REPL`
- CH2 and any closure chain of n links: `gopanic | CH2.func2 | gopanic | CH2.func3 | gopanic | CH2`
- DERR (a fault inside a deferred closure): `gopanic | panicmem | sigpanic | DERR.func2 | DERR`
- DCALLEE: `gopanic | hNI | DCALLEE.func2 | DCALLEE`. This one was right at e266 only when no TC0 tail call removed the closure frame.
- Not only closures: a named function, method or method value deferred directly that panics itself.
  - DG0: `gopanic | g0 | DG0`
  - DG0P: `gopanic | g0 | gopanic | DG0P`
  - DG1: `gopanic | g1 | DG1`
  - DM and DMV: `gopanic | T.m | DM`
  - A non-nil `defer f()`, and for example `defer wg.Done()` panicking on a negative counter.

Wrong at e266 and missing now (gains):
- `defer panic(v)` (named funcN where Go shows deferwrap1).
- NC, the converter's `() => c()`.
- NA, NA2 and NAR, which lacked deferwrap1.
- Converter wrappers around a panicking callee, such as `() => fʗ1.Close()`.
- Closure frames lost to TC0 tail calls.

**Residual.** 1. A nil deferred func after a COMPLETED RECOVERY still splices a WRONG list, missing `runtime.deferreturn`, in any function, including open-coded ones.
   - Go: `... | sigpanic | runtime.deferreturn | owner`. The cut: `... | sigpanic | owner`.
   - It is not stated, no arm covers it, and the open-coding hint the ruling names cannot close it.
   - It is the run-time fact "a recovery completed in this sequence", which golib already knows.
   - Fix: in Run, keep a local bool set where `SetSequence(sequence, null)` runs. When running is null and that bool is set, leave the nil-thunk panic unstamped, so it splices nothing. Or model the frame: after a recovery, Go always shows runtime.deferreturn beneath the bottom link.
   - Arm, red first: `defer reader; var f func(); defer f(); defer func(){ recover() }(); panic("p")`, expecting no splice (or Go's list with deferreturn).
2. PREDICTION wording.
   - The deferreturn residual is a PARTIAL list, not "splices NOTHING": the loop form, more than 8 defers, returns×defers over 15, and after a recovery.
   - The closure residual should say what the code does: it refuses every panic the Run catches first, whether the deferred call is a closure, a named function, a method value or a golib/converter wrapper, with no deferring frame between. The exceptions are the zero-arg nil thunk and a directly deferred first catcher.
   - It should name the given-up direct named-function shapes (DG0, DG1, DM).
3. Out of scope, noted: at TC0, frames inlined or tail-called out of a SiteTrace are lost from every accepted site, including the re-raise exception. This predates the cut; it is the C2 skeptic's item 4.

None of this touches the 10-row runtime prediction, which stands.

**Skeptic: partial.** Everything was read at 28fd1e3218 after a fetch, using `git diff e266debe65 28fd1e3218` and `git show` of managed_impl.cs, GoFrame.cs, PanicException.cs, builtin.DeferRegistrations.cs, the GolibTests arms, and the converted runtime callers_test.cs and stack_test.cs. No C# was built or run. The Go ground truth is go1.24.13 windows/amd64, run plain and with -gcflags=all=-l; both runs gave identical lists. My probe is <scratch> (output in plain.txt and l.txt). I also re-ran the verifier's two probes as dcsk\v1 and dcsk\v2, and they reproduced every list the verifier quoted. The Go cache and temp folders were purged, no bin/obj was created, and the repo is untouched.

(1) THE REFUSAL ONLY EVER REMOVES A SPLICE; IT NEVER MAKES ONE WRONG.
- The change is one added early return, `return []`, in splicePanic (managed_impl.cs:2087-2089). It fires before any frame is emitted, and it discards the whole list built so far, so it cannot leave a partial list.
- splicePanic is only called from captureCallers (:1925). The Run-to-sequence pairing (sequencesMet++, :1923) happens before the splice and does not depend on its result. runtime.Stack goes through renderStack, not captureCallers.
- git grep finds SiteEndsAtRun, SiteIsTheDeferredCall, Beneath, SiteOwnerThread and NilDeferredCallMethod read only in splicePanic.
- The base e6fc210500 did no splicing at all. So a refused Run gives exactly the base's output for that Run: the refusal cannot cause a row to move that was not predicted.
- Arms pin both sides:
  - Removing the refusal turns six arms red: normalReturnPanic, panicAfterRecovery, deferPanicArg, twoLinkChain, recoverThenPanic and replacedPanic, plus the NA, NAR and NC arms.
  - Widening it to refuse the two accepted shapes also turns arms red:
    - the nil thunk: ANilDeferredFuncFaultsFromTheDeferringFunctionsExit, nilDeferWhilePanicking, longThunkChain(65);
    - the re-raise: deferTheCatcher, deferACatcherWithNoDefer.
- I also looked for a wrong splice in the re-raise exception (SiteIsTheDeferredCall) and found none by reading:
  - the same D deferred twice;
  - D replaced by its own deferred panic (SiteEndsAtRun refuses);
  - nested direct re-raises (Catches 3 refuses);
  - D recovering and then panicking;
  - a promoted method.
- Go agrees wherever I measured:
  - DDAR: gopanic | D | DDAR
  - DDL: gopanic | D | DDL
  - DNILAR: gopanic | panicmem | sigpanic | gopanic | D | DNILAR
  - DDP, DCDP, DCDNR as the verifier listed.
  - Go's runtime.deferreturn is a wrapper function (cmd/internal/objabi/funcid.go:36, "deferreturn": abi.FuncIDWrapper). Tracebacks drop it unless it called gopanic, sigpanic or panicwrap, so it never shows under D.

WHY PARTIAL: the design keeps the nil-thunk exception, and that exception still produces a WRONG list after a completed recovery. The change did not introduce this (it was already there at e266debe65 and at a419543c14), but it contradicts the design's claim that every residual "splices NOTHING, never a partial list". It is not stated anywhere and no arm covers it.
- NILAR (`var f func(); defer f(); defer func(){recover()}(); panic`), traced by reading:
  - Run calls SetSequence(null) after the recovering call returns, so `running` is null.
  - The thunk's panic is its first catch, so it is stamped with SiteEndsAtRun and no Beneath.
  - :2088 accepts it: the site is empty, the kind is Memory, and raw[0] is the thunk.
  - go2cs splices: gopanic | panicmem | sigpanic | NILAR
  - Go:            gopanic | panicmem | sigpanic | runtime.deferreturn | NILAR
- New instances of the same class, measured in Go:
  - NILARC: a closure's panic on a normal return is recovered, then the nil func runs. Go shows ... | sigpanic | runtime.deferreturn | NILARC.
  - NILINCAR: the same inside a deferred closure's own sequence. Go shows ... | sigpanic | runtime.deferreturn | NILINCAR.func1 | NILINCAR.
  - NILAR3: two nil funcs after a recovery. Go shows the chain, then runtime.deferreturn.
  - NILAR2 and NILARL (the verifier's) reproduce.
- Control shapes that match go2cs today:
  - RAR (TestCallersAfterRecovery's shape): no deferreturn.
  - NILNR (no recovery): no deferreturn.
  - NILINC (nil func in a closure's own sequence while the owner panics): no deferreturn.
- The ruled normal-return residuals also splice a PARTIAL list, not nothing: NILL (TestCallersDeferNilFuncPanicWithLoop, predicted to stay FAIL), NIL9 and NILRET all show deferreturn in Go.

(2) THE 10 PREDICTED ROWS STAND. None of them needs a refused link. From the converted sources:
- TestCallersPanic: the panic is thrown in f3 (NoInlining) and first caught by the test's own catch, so its site ends at the function.
- TestCallersNilPointerPanic: p.Value on a null box raises NullReferenceException in the test body (Memory).
- TestCallersDivZeroPanic: 5/n in the body (Divide).
- TestCallersDoublePanic: func1 has its own GoFrame. Its catch catches p2 first, so func1's Run owns p2 with its site ending at func1. p1 is still Test's running panic, because SetSequence(null) happens only after func1 returns. Traced result: Callers | func1.1 | gopanic | func1 | gopanic | Test, which is want.
- TestCallersDeferNilFuncPanic: the emission is `defer(fʗ1, ref ᒐ)` with a null Action. The nullary rung calls frame.Push, which substitutes s_nilDeferredCall, and :2088 accepts it. The test checks only up to sigpanic. R's 106/106 green, which includes longThunkChain, shows that the method-equality test holds at run time.
- Both /CallersFrames subtests: the fault comes from cb() in the t.Run literal's body, and that literal's own catch catches it first, so the site ends at the function. /Stack does not go through captureCallers.
- The two intermediate parents and TestStackWrapperStackPanic follow from their subtests.
- The rows predicted to stay:
  - AfterRecovery and AbortedPanic2 read a null sequence entry.
  - AbortedPanic's p2 is an ends-at-Run closure link, but func3 recovers it and SetSequence(null) runs before the reader.
  - FromWrapper has no panic.
  - WithLoop stays FAIL.

(3) SPLICES GIVEN UP. Each matched Go at e266 and is refused now; Go's lists are re-measured:
- Closures raising:
  - NR and NRL: gopanic | NR.func2 | NR
  - AR: gopanic | AR.func2 | AR
  - RP: gopanic | RP.func2 | gopanic | RP
  - REPL: gopanic | REPL.func3 | gopanic | REPL
  - CH2: gopanic | CH2.func2 | gopanic | CH2.func3 | gopanic | CH2, and any chain of n closure raisers
  - DERR: gopanic | panicmem | sigpanic | DERR.func2 | DERR
  - DCALLEE: gopanic | hNI | DCALLEE.func2 | DCALLEE (it was right at e266 only when the JIT did not tail-call the closure away)
- Not only closures: named functions and methods deferred directly that panic themselves:
  - DG0: gopanic | g0 | DG0
  - DG0P: gopanic | g0 | gopanic | DG0P
  - DG1: gopanic | g1 | DG1
  - DM and DMV: gopanic | T.m | DM
- Any nil thunk or re-raise stacked on one of the shapes above, because the whole chain is refused.

Wrong at e266 and missing now (gains): `defer panic(v)` (Go shows deferwrap1), NC's `() => c()`, NA, NA2 and NAR (missing deferwrap1), converter wrappers around a panicking callee (`() => fʗ1.Close()`), and closure frames lost to TC0 tail calls.

Wording: "every panic a deferred CLOSURE raises" is broader than the code in one direction and narrower in another.
- Broader: closures that defer and then panic are still spliced, correctly (DoublePanic, DCDP, DCDNR).
- Narrower: it leaves out the named-function, method-value and golib-ladder shapes that are refused.

**Skeptic residual.** 1. Nil func after a completed recovery. The nil-thunk exception still produces a WRONG list, missing runtime.deferreturn: Go shows `... | sigpanic | runtime.deferreturn | owner`, go2cs `... | sigpanic | owner`.
   - It happens whenever a zero-argument nil deferred func runs after a recovery has completed in the same Run sequence and no newer panic is running, in open-coded functions too.
   - Measured shapes: NILAR, NILAR2, NILAR3, NILARL, NILARC, NILINCAR.
   - Fix: in Run, keep a local bool that is set where `SetSequence(sequence, null)` runs after a recovery. When that bool is set and `running` is null, leave the thunk's panic unstamped, so the chain splices nothing. Or add the runtime.deferreturn frame: Go always shows it there.
   - Do NOT refuse when `running` is not null. DNILAR, where the nil func is called by a later panic after the recovery, has no deferreturn in Go and splices correctly today.
   - Arms, red first, each expecting no splice (or Go's list with deferreturn):
     - NILAR: `defer reader; var f func(); defer f(); defer func(){recover()}(); panic("p")`
     - NILARC: the recovered panic is a closure's, on a normal return
     - DNILAR as a control that must stay spliced
   - Then ANilDeferredFuncFaultsFromTheDeferringFunctionsExit (normal return, no recovery) must stay green.

2. PREDICTION wording.
   - The heading "each still splices NOTHING, never a partial list" is false for the deferreturn residual. The ruled normal-return shapes (WithLoop/NILL, more than 8 defers/NIL9, returns×defers over 15/NILRET) and the after-recovery shapes in item 1 all splice a partial list. So does panicwrap, whose fault frames are named but not modelled.
   - The closure residual should say what the code does:
     - It refuses every panic whose first catch is the Run itself: a closure, a named function or method deferred directly (DG0, DG0P, DM, DMV), or a golib/converter wrapper (DG1, NA, NC, `defer panic(v)`).
     - The only exceptions are the zero-argument nil thunk and a directly deferred first catcher.
     - Closures that defer and then panic are still spliced, correctly (DoublePanic, DCDP, DCDNR).

3. Optional, not owed. The directly deferred named-function shapes could be recovered without the converter's help: accept an ends-at-Run site when the popped delegate's Method is a non-compiler-generated method equal to the site's last Go frame (DG0, DG0P, method-group DM). No converter wrapper is a named method, and go2cs-gen forwarders are already excluded from the site.

4. Out of scope, noted. At TC0, frames inlined or tail-called out of a SiteTrace are lost from every accepted site. This predates the cut; it is the C2 skeptic's item 4.

None of this changes the 10-row runtime prediction, which stands.

