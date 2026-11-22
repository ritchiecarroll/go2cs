# TypeScript reading: predictions (written before the runs; clock above in the post)

Setup: microsoft/TypeScript at ba29a562991c, tsc/ minus testdata, P4's kit go/cmd (56c5ec548f) copied in, xxh3
replaced with go2cs's fork (B5's labelled replace), go mod tidy; load ./cmd/checkdemo; GOOS=windows, go1.26.10;
B5's two cuts: (*fmt.pp).handleMethods, (*fmt.ss).ReadRune, (*fmt.readRune).ReadRune. go list -deps reads 199
packages here against B5's 200.
- T1 (control): the base report reads B5's counts within 1%: 16,923 functions in 173 packages (TS module 9,395).
- T2 (control): the base creation census counts about 124 can-park func-value call sites in the TS module (the
  reviewer's "124 of 2,197", generator reach).
- T3: the propagating rule + fix 1 (arm set) grows twins in the closure by +80..+150% (net/http's closure: +125%).
- T4: the leaf rule + fix 1 leaves at least a third of the TS sites holed (transitive), the propagating one under a
  tenth (net/http: 15 of 31 against 0).

## First read (2026-11-14 00:12 CST), and a tool bug found in it
- T1 TRUE: 16,881 functions (B5 16,923, -0.25%); 45 direct parkers (the record's "45 functions").
- T2 FALSE as worded, a SCOPE difference: the creation census counts sites INSIDE the TS module, 18; the record's
  124 of 2,197 (DATA-mn-spike-2.md, line 1643) is the WHOLE closure's func-value call sites.
- T3 FALSE: twins 806 -> 9,307 (4.8% -> 55.1% of declared), x11.5; SSA instructions 41,773 -> 360,360 (x8.6).
- T4 first half TRUE (leaf: 8 of 18 holed), second FALSE (propagating: 7 of 18 holed) -- but 6 of those 7 were
  rooted in (*internal/poll.pollDesc).init, a METHOD named init that creationKind took for a package initializer
  (it matched the name only). Fixed: a package initializer has no receiver. net/http's paths also crossed a method
  named init ((*net.nsswitchConfig).init), so every arm, net/http and TS, re-runs on the fixed tool.
- T5 (re-run): TS propagating + fix 1 has at most 1 rule-own hole; net/http's rule-own holes stay 0 and its
  awaited counts move by at most 2 in any arm.

## Re-run on the init fix (00:14 CST): identical numbers; diagnosis and a second rule
The init fix changed no count (T5 FALSE for TS: still 7 holed). The no-initializer walk's own paths show every one
of the 7 rooted at (*fmt.pp).handleMethods -- B5's CUT 1 -- through VTA's Error() edge (json.SemanticError.Error
-> sync.OnceValue closure -> Once.Do -> checkerPool.createCheckers$1 -> ...). A cut asserts that reach is spurious,
so a cut function is no plain root either (tool rule added; net/http has no cuts, so its arms are unaffected).
- T6: TS propagating + fix 1 reads 0 rule-own holes; leaf + fix 1 keeps at least 5.
Re-run with cut roots excluded: still 7 (T6 not yet readable). The walk reached the cut function (*fmt.readRune).
ReadRune from elsewhere and expanded THROUGH it: a non-twinned cut function looks like a plain body mid-path. Same
reasoning: the walk no longer expands out of a cut function. T6 stands as predicted for the next run.
