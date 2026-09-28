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
