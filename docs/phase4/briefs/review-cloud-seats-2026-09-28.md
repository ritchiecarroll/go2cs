# Review: C2's narrow re-cut and MinInt/-1, and C1's runtime.Error factories (2026-09-28)

COORD's adversarial review, 47 agents, go1.24.13 ground truth; each probe converter and C# build ran in its own scratch root. Seats: C2's narrow re-cut (32da9c3826, on claude/c2-narrow-minint), C2's MinInt/-1 (6cb2f0ae0c..d4a9dfcae1 on the same stack), and C1's runtime.Error factories (claude/c1-runtime-error-factories db3868b4d0). Nothing surviving was refuted.

## Verdicts

- **Narrow re-cut: ACCEPT, with follow-ups.** D1, D2 and D5 are CLOSED, each proven by a mutation test (take the fix out and exactly the right arms go red). D3/D8, D4, D6 and D9 are partial: constant compound receivers of a guarded shift still bind the int32 GoShift overload (`(c+50)<<n`, `(-d)<<n`, `(c>>1)<<n`; pre-existing on master for `<<`, and a type-only difference for `>>`), D4's tag mirror is unreachable and the non-constant unary case label is a compile error (both pre-existing, identical on master), and several fixed shapes have no guarding arm (D6's `/=`/`%=` byte divisor, and D9's list).
- **MinInt/-1: RE-CUT needed.** BLOCKER: a compound `x /= b` or `x %= b` whose target is not "read twice without effect" (a map element, `a[f()]`, `*p`, `a[i+1]`) falls back to C#'s `/=`, so MinInt / -1 still throws an OverflowException that recover() cannot see. Evaluate the lvalue once (a ref local, or a temp for the index) and call quo/rem. CONCERNS: a generic division whose type parameter has a signed int core, instantiated with an UNNAMED type, is unguarded; the constant `% -1` fold drops a pointer-selector dividend's nil check (`p.v % -1` with nil p becomes 0); the builtin.quo spelling misses a local constant and a type-switch binding, which are compile errors; a constant -1 divisor is not folded in two shapes; and the helper is emitted at 18 of 108 sites per target where MinInt / -1 is impossible (a constant dividend other than MinValue, or len/cap), which costs readability and makes the docs' claim false.
- **runtime.Error factories: ACCEPT, with follow-ups.** CONCERNS, no blockers: reflect's hand-owned nil-map write (SetMapIndex) and the nil-channel Value.Close still panic with a string; makechan with a too-large size still escapes as a raw .NET exception, and the census's "truncates" is wrong since it throws; a huge unsigned index prints a text without the length, with %#v showing signed:true; when a program's import closure never reaches runtime, the hooks are never registered and all six classes recover as strings (a design limit, to state); a negative shift count does not panic (runtime.shiftError is missing); raw .NET exceptions still reach some bounds paths (via the ж.at accessor). PRE-EXISTING and SERIOUS, found by the slice probes: negative slice bounds, and string slicing past the end, throw CLR exceptions that escape recover() and end the process.

## Appendix: every result

### The narrow re-cut, per D item

#### D1: the mis-bound cast on a guarded variable-count '(a+b) >> n' (also '(h/m)>>n', '(-(a+b))>>n'), plus the same shapes with '<<': verifier closed

**Evidence.** I built both converters in <scratch> tip is 32da9c3826 and base is ff71ab6a9e. Each probe went to its own output root, and I built the C# with dotnet10 against tip golib/gen. Ground truth is `go run` on go1.24.13.

**The fix.** Line 477 of convBinaryExpr.go in emitGuardedShift gains one term: `|| v.narrowArithmeticRendersAsCast(binaryExpr.X, leftOperand)`. It parenthesizes the receiver when the operand, seen through its parens, rendered as its own narrowing cast. The false comment on narrowArithmeticParenSelfCast ("never the target of a member access") is corrected.

**Probe p1: the review's snippets, 37 rows, run at runtime.** Tip matches Go on all 37. Base diverges on 29.
- Tip `>>` emission, e.g. `int8 x = (int8)(((int8)(a + b)).Rsh(n));` and `return ((uint8)(u + v)).Rsh(n);`.
- The same pattern holds for `((int8)(h / m)).Rsh(n)`, `((int8)(-(a + b))).Rsh(n)`, `((int8)(a + b)).Rsh((uint64)(10))`, compound `k +=`, `int16 (s1+s2)>>n`, `((a+b))>>n`, `(h-a)>>n`, an index, nested and add consumers, and interface arguments.
- Tip against base, Go values first: typed -28 (base 100), any uint8 72 (base 200), comparison true (base false), `(h/m)>>n` -64 (base 64), `(-(a+b))>>n` 28 (base -100), `>>10` -1 (base 0), int16 -2768 (base 30000).
- `<<` with the same shapes now emits `((int8)(a + b)).Lsh(n)` and `((uint8)(u + v)).Lsh(n)`. Every row is Go-correct: int -112, any int8 -112, `/3` gives -37, int of `(h/m)<<n` is 0, `(-(a+b))<<n` any is int8 112, int16 widen -11072, `<<10` gives 0. At base the any consumer boxed int32 400 and the widen read 400, 256 and 204800. Those were D3 failures, not a mis-bind.
- The unparenthesized controls `a*b>>n` and `-h>>n` render the same way as the parenthesized forms, so the two Go spellings now agree.

**Probe p2 (adjacent shapes).** Correct at tip: define, assign, `(^(a+b))>>n`, `(a*b+a)>>n`, a guarded shift used as a count, generic `~int8`/`~uint8`, and named narrow types including `(t1+t2).M()` and a method value.

**Probe p3.** byte-alias and uint16 guarded shifts at typed, element, literal and any consumers are all Go-correct.

**Would an arm catch it coming back?** Yes.
- NarrowArithmeticSinks has new arms "guarded shr" and "guarded shl" covering exactly these shapes: `(a+a)>>n` typed, `(u+u)>>n` any, `==-28`, `(m8/n1)>>n`, `(-(a+a))>>n`, `(a+a)>>10`, and the `<<` counterparts.
- Go prints `guarded shr: -28 72 true -64 28 -1`. Base emits the mis-bound `(int8)((int8)(a + a).Rsh(n))` for these, which evaluates to 100 200 false 64 -100 0, so the arm is red at ff71ab6a9e.
- The tip conversion of the test is byte-identical to its main.cs.target (0 diff lines).
- Mutation check: I removed only the new term and rebuilt (go2cs-mut.exe). The golden diff then shows exactly 10 lines, all in the guarded shr and guarded shl arms, and nothing else. The mutant's p1 run is red on 18 `>>` rows (for example 100 against -28).

**Cleanup.** bin/obj/Generated and the NuGet caches are purged; probes, emissions and logs are kept. H:\Projects\go2cs is untouched, and tG was not accessed.

**Residual.** Nothing blocks D1. Three side notes, none a value defect:
1. For `<<`, the mis-bind would be value-safe even if it returned. Under the mutant, `(int8)(a + a).Lsh(n)` still narrows the low bits, so every `<<` row stayed Go-correct at runtime. Only the golden text catches a `<<` regression; the runtime arm does not.
2. Typed destinations still wrap an already-narrow Rsh/Lsh result in a redundant outer cast, for example `int8 x = (int8)(((int8)(a + b)).Rsh(n));` and `return (byte)(((byte)(bb + bc)).Rsh(n));`. This predates the re-cut: the bare `a*b>>n` control emits the same at ff71ab6a9e. It is a reads-like-Go nit next to D8, not part of D1.
3. The new narrowArithmeticRendersAsCast has the same body as narrowArithmeticParenSelfCast, so one could call the other. That is a small cleanup, not a correctness issue.
Not re-checked: I did not build or run the full NarrowArithmeticSinks project, because it needs the fmt dependency closure. Its arm output is inferred from the p1 runs of the same expression shapes plus the byte-identical golden.

#### D2: unary + and signed unary ^ over an overflowing narrow operand reaching value consumers (widening conversion, index, interface, comparison): verifier closed

**Evidence.** The fix is in narrowArithmeticProducer at 32da9c3826 (src/go2cs/narrowArithmeticOperations.go). A unary + or signed ^ now counts as wrapRisk when its unparenthesized operand is itself a wrapRisk producer. The check recurses, so chains like +^, ^^ and ^- carry the risk up. The markNarrowArithmeticContexts switch was merged into one condition (Typed, or Value && wrapRisk), so a Value consumer now casts the unary: int(^(a+b)) emits (nint)((int8)(~(a + b))), tbl[+(u+v)] emits tbl[(uint8)(+(u + v))], and (^(a+b))/2 emits (int8)((int8)(~(a + b)) / 2).

How I checked: I built the converter at 32da9c3826 and at ff71ab6a9e with GOROOT pinned. I converted four probe packages, one output root each, built them against each ref's own golib and gen with dotnet10 (all temp and NuGet folders on H:), ran them, and compared the output with go run.

- p1 has 55 lines: every snippet from S02, S09, S14 and S19, plus nested chains (+^, ^+, ^^, ++, ^-, ^(mn/n1), ^(s*t), +uint16) and other value consumers. Those are a shift count, make len and cap, a slice high bound, a 3-index max, an array index, divisor and remainder, /= and %=, range-over-int, float32, int16 and uint8 conversions, an interface (checked with %T through a type switch), comparisons, unary over a guarded <<, a native << and a guarded >>, and an untyped-constant guarded << used as a divisor. 32da9c3826 matches Go on 55/55. ff71ab6a9e is red on 40: xorVal -201 against 55, plusIdx panics, S14 cmp false against true, the switch takes default instead of case55, ^(-mn) gives -129 against 127, the shift count gives 0 against 1<<44.
- p3 has 13 lines: string(^(a+b)), +(-mn), ^(a+b) under | and &^ and &, max(), a named N int8, a generic T ~int8, a same-width conversion, and uint16/int16 widening. The new ref matches 13/13; the old ref is red on 5.
- p4 has 7 lines where a unary is the receiver of a guarded shift (this crosses D1 and D3): int((^(a+b))>>n), int(^(a+b)>>n), (+(u+v))<<n, tbl[+(u+v)>>n], a typed int8. The new ref matches 7/7 and emits ((int8)(~(a + b))).Rsh(n) correctly parenthesized; the old ref is red on 6.

Would an arm catch the defect coming back? Yes. As a negative control I built a converter at 32da9c3826 with only the D2 hunk removed and converted NarrowArithmeticSinks (from 32da9c3826). Its main.cs differs from main.cs.target on exactly the two lines of the 'unary xor plus' arm: int(^(a+a)), q, (^(a+a))>>1, tbl[+(u+u)] and int(+(a*a)) all lose their casts. The unmodified 32da9c3826 converter's output equals the golden byte for byte. The arm's Go output is '55 27 27 144 16'. p1 shows the reverted emission shapes giving -201, -100, -101, a panic and 10000, so the arm is red both on the golden compare and at run time. Scratch folder: <scratch> (bin/obj purged). H:\Projects\go2cs was not modified.

**Residual.** Nothing left in D2's scope. Four things came up during probing. All exist unchanged at ff71ab6a9e, none comes from this re-cut, and they are out of D2's scope:

(1) A non-constant UnaryExpr case label in a TAGGED switch is emitted as a C# constant pattern, which fails with CS9135. It happens for any integer type, not just narrow ones: 'switch k { case ^(a+b): }' emits 'exprᴛ1 is (int8)(~(a + b))', and a plain int 'case -x:' emits 'exprᴛ1 is -x'. The cause is canUsePatternMatch in src/go2cs/visitSwitchStmt.go: its syntactic screen covers Ident, Selector, Index and BinaryExpr but not UnaryExpr. That is a switch-emitter seat, not a narrow-arithmetic one.

(2) Unary + over a type parameter (T ~uint8) emits '+(x + y)' on T and fails with CS0023, because the constraint has no IUnaryPlusOperators. Type parameters are outside the narrow pass.

(3) Cosmetic (reads-like-Go): the existing typed-destination arm still adds an outer cast. 'var x int8 = ^(a+b) >> n' emits '(int8)(((int8)(~(a + b))).Rsh(n))', although Rsh already returns int8.

(4) Arm coverage is thin but enough. The 'unary xor plus' arm covers a widening conversion, an index, a / operand and a >> operand. It does not cover a unary reaching a shift count, a slice bound, a switch tag or float/string conversions. All of those go through the same Value && wrapRisk path, and a revert of the hunk turns the existing arm red.

#### D3 + D8: variable-count << narrowed on the guarded path ('(u + w) << s', 'u*w << s', '(u>>1) << s'), and no unneeded self-casts on a guarded >> whose receiver is already narrow (NarrowShiftVarCount golden should read as master): verifier partial

**Evidence.** Verdict: the three named shapes and D8 as worded are CLOSED, and each has an arm. I call the item PARTIAL because one kind of receiver still binds the int32 overload. That hole includes a `>>` case which ff71ab6a9e emitted correctly and the re-cut drops.

Method: I built converters at 32da9c3826 (tip), ff71ab6a9e, d4a9dfcae1 and 1aebd6a885 (master merge-base) in <scratch> GOROOT was the backslash spelling of 1.24.13. I converted probe packages (probe\p1..p4) with each converter, one output root per conversion. The C# was built against a golib and gen root staged from the same ref (dotnet10, TEMP/NUGET inside scratch), and its stdout was diffed against `go run`. bin/obj are purged and the main checkout is unchanged. The ff71ab6a9e and d4a9dfcae1 emissions are byte-identical on these probes.

1) D3 is fixed. The tip emits `((uint8)(u + w)).Lsh(s)`, `((uint8)(u * w)).Lsh(s)`, `(nint)(((uint8)((u >> (int)(1)))).Lsh((n + 1)))`, `((int8)(~aa)).Lsh(k)` and `((int8)(-mn)).Lsh(n)`. ff71ab6a9e and master emit `(u + w).Lsh(s)` etc.
- probe p2 has 62 lines: every S03, S07, S15 and S20 snippet, 16-bit forms, & | ^ &^ receivers, unsigned-^ receivers, native-shift receivers, chains, % and / receivers, counts at or above the width, a map key, a generic argument, typed destinations, comparisons, a shift count and a compound assignment.
- Tip C# output == Go on all 62 lines.
- The ff71ab6a9e control is red on 36 lines at run time (S03val 400 vs -112, S03any int32 400, S03div -123 vs -37, S07add int32 600 vs 88, S20u int32 800 vs 32, Xc9l int32 102400 vs 0, and so on). It also has one compile error: `m[(u + w).Lsh(k)]` is CS1503.

2) An arm would catch D3's return. The 'guarded shl' arm in NarrowArithmeticSinks uses exactly the S03 shapes (`int((a+a)<<n)`, an `any`, `/3`, `(h>>1)<<(n+1)`, `(u+w8)<<8`), and each is red at ff71ab6a9e in my control. Its golden also moves: ff71ab6a9e regenerates `(a + a).Lsh(n)`. The tip's regeneration of NarrowArithmeticSinks and NarrowShiftVarCount is byte-identical to the committed main.cs.target. The unparenthesized `u*w << s` has no dedicated arm, but it goes through the same classification.

3) D8 as worded is fixed. The NarrowShiftVarCount Rsh lines read as master again: `fmt.Println(cb.Rsh(k)); fmt.Println(s8.Rsh(k));`. The only remaining differences from master in that golden are the three wrap-risk `+` casts (`(byte)(b + cb.Lsh(k))`), which the pre-pass intends. In probe p2, S16, S23 and `gen(cb>>k)` all emit bare `.Rsh`, with the right value and %T.

RESIDUAL A, Go-divergent: a typed narrow CONSTANT compound receiver. narrowArithmeticProducer rejects any expression with tv.Value != nil. emitGuardedShift casts only a bare untyped or tightened const. So `const c uint8 = 100; (c+50)<<n` emits `(c + 50).Lsh(n)`, where C# `c + 50` is a const int. Probe p4, tip run against Go:
- `(c+50)<<n`: int32 300 vs uint8 44
- `(d+20)<<n` (const d int8 = 100): int32 240 vs int8 -16
- `(-d)<<n`: int32 -200 vs int8 56
- `int((c+50)<<n)`: 300 vs 44
- `(c+50)>>n`: int32 75 vs uint8 75 (value right, type wrong)
This is pre-existing: master == tip on p4. The `>>` form, however, was `(uint8)((c + 50).Rsh(n))` (correct type) at ff71ab6a9e. The re-cut removes it, because its premise that a guarded `>>` always renders on a receiver of the operand's own narrow type is false for this receiver.

RESIDUAL B, reads-like-Go: the existing typed-destination and comparison arm (narrowArithmeticCastTypeFor, visitAssignStmt.go:251) still casts the whole guarded shift. Now that the receiver is also cast, the result is a double cast: `int8 x = (int8)(((int8)(a + a)).Rsh(n));` and `(int8)(((int8)(a + a)).Rsh(n)) == -28` (NarrowArithmeticSinks main.cs.target:205/207), and `uint8 t8 = (uint8)(((uint8)(u + w)).Lsh(k));`. The new docs section in native-and-narrow-integers.md, lines 107-119, says a guarded shift "needs no cast of its own", yet its example is that double-cast line. The single outer cast on a narrow-variable receiver (`int8 x8 = (int8)(s8.Rsh(k))`) is master's own text.

Aside, outside this item and pre-existing on origin/master: shiftGuardWidth (convBinaryExpr.go:295) reads tv.Type without Unalias, while compoundShiftGuarded does unalias. So `type W = uint32; y << m` with m=40 stays native and masks the count. Probe p3 on the tip: 51200 vs Go 0; `type B = uint8; x<<33` gives 144 vs Go 0, and x>>33 gives 100 vs Go 0.

**Residual.** 1. Suggested fix for Residual A, the constant receivers. In emitGuardedShift, extend the receiver-cast branch to any receiver that is a typed Go constant of a narrow basic type, e.g. `((uint8)(c + 50)).Lsh(n)`. The cast is legal because a Go constant cannot overflow its type, so the C# constant is in range. Alternatively, let narrowArithmeticProducer admit a constant when its consumer is a guarded-shift receiver. Add an arm to NarrowArithmeticSinks with `const c uint8 = 100; const d int8 = 100` and `(c+50)<<n`, `(-d)<<n` and `(c+50)>>n` reaching an `any` or %T. That arm is red at the tip, at ff71ab6a9e and at master.

2. Residual B, the redundant outer cast, is a nit and is optional. narrowArithmeticCastTypeFor could skip a narrowShiftGuarded shift, since its receiver now always carries the width. That would also improve master's own `int8 x8 = (int8)(s8.Rsh(k))` and `var x = (byte)(cb.Lsh(k))`, at the cost of golden churn. At minimum, correct the docs sentence "needs no cast of its own" or its example at native-and-narrow-integers.md:107-119.

3. Stale comments in narrowArithmeticOperations.go at the tip:
- narrowConsumerInvariant (~line 21) still names "the left operand of `<<`".
- narrowConsumerValue (~26) names "`>>` operand".
- markNarrowArithmeticContexts' doc (~58) still lists `<<` as already narrowing its whole result.
- narrowArithmeticProducer's doc (~98) lists `>>` as type-only.
All of these should say "native".

4. Out of scope, flag for a separate seat: the binary shift guard skips alias-typed operands (shiftGuardWidth has no Unalias), which gives Go-divergent masked counts. Pre-existing on master.

**Skeptic: partial.** I reproduced this independently. Converters were built at 32da9c3826 (the tip), ff71ab6a9e and 1aebd6a885 (the master merge-base) in <scratch>, with GOROOT spelled as the backslash path to 1.24.13. I wrote seven fmt-free probes (d3, d8, ext, cst, cst2, cst3, alias). Each conversion had its own output root. Each probe was built against a golib and gen root staged from the same ref, and its output was diffed against `go run`. The main checkout is unchanged, and bin/obj, nuget and temp have been purged.

CLOSED: the named D3 shapes, with an arm that catches them.
- Tip emission: `((uint8)(u + w)).Lsh(s)`, `((uint8)(u * w)).Lsh(s)`, `((uint8)((u >> (int)(1)))).Lsh(s)`, `((int8)(~a2)).Lsh(s)` and `((int8)(-mn)).Lsh(n)`.
- Probe d3 holds every S03, S07, S15 and S20 snippet from the review, plus the item's three shapes and destination/compare/div/rem/shr consumers. The tip matches Go on 26 of 26 lines.
- ff71ab6a9e is red on 23 of them: S03 400 vs -112, S03div -123 vs -37, S07add int32 600 vs uint8 88, S07mul int32 40000 vs 64, S07c8 int32 76800 vs 0, S15neg 256 vs 0, S20u int32 800 vs 32, div 200 vs 29.
- Probe ext (& | ^ &^ receivers, unsigned ^, min, conversions, 16-bit, a field, nested guarded shifts, unary + and -, % and /, untyped constants, map keys, counts at or above the width) matches Go on 25 of 25 at the tip. At ff71ab6a9e and at master the same probe fails to compile: `m[(u + w).Lsh(s)]` gives CS1503.
- Regenerating NarrowArithmeticSinks and NarrowShiftVarCount with the tip converter is byte-identical to the committed main.cs.target. With ff71ab6a9e, the 'guarded shl' arm regenerates `(a + a).Lsh(n)` and `(u + w8).Lsh((uint64)(8))`. Those shapes are runtime-red in probe d3, so both the golden and the run would catch D3's return.

CLOSED: D8 as worded.
- S16 and S23 emit bare `a.Rsh(n)`, `u.Rsh(n)`, `cb.Rsh(k)` and `s8.Rsh(k)`, with correct values and %T.
- The NarrowShiftVarCount Rsh lines read as master. ff71ab6a9e regenerates `(byte)(cb.Rsh(k))` and `(int8)(s8.Rsh(k))`, a 4-line diff against the tip target, so the golden would catch their return.
- Guarded >> with an int-promoted receiver is right on all nine rows of probe d8 at the tip. ff71ab6a9e was wrong on some of them: (a+a)>>n gave 100 vs -28, and (m8/n1)>>n gave 64 vs -64.

WHY PARTIAL: the D3 defect class is still open for CONSTANT compound receivers.
The review defines D3 as "the guarded path binds int32 Lsh on an int-rendered receiver, unnarrowed". C2 claims "an int-promoted receiver is cast". narrowArithmeticProducer rejects tv.Value != nil. emitGuardedShift casts only an untyped literal or a tightened constant reference. So a Go constant expression of narrow type goes out bare and binds int32.

Tip results against Go, all on a guarded path:
- Typed constants `const c uint8 = 100` and `const d int8 = 100`:
  - `(c+50)<<n`: int32 300 vs uint8 44
  - `(d+20)<<n`: 240 vs int8 -16
  - `(-d)<<n`: -200 vs 56
  - `int((c+50)<<n)`: 300 vs 44
  - `(c*2)<<n`: 400 vs 144
  - `(c<<1)<<n`: 400 vs 144
  - `(^d)<<n`: -202 vs 54
  - `(c>>1)<<nn` with nn=9: 25600 vs 0
  - `var x any = (c+50)<<n`: int32 300
  - `arr[(c+50)<<n]`: 300 vs 44
- Untyped constant receivers typed narrow by context are also affected (probe cst3). `v/((100+50)<<n)` gives 0 vs 4, `v%(...)` gives 200 vs 24, `((100+50)<<n)/v` gives 1 vs 0, and `i8/((60+60)<<n)` gives 0 vs -6. The outer `(uint8)` from the `/` producer cannot help, because division does not preserve low bits.
- A typed destination or comparison is still correct: `(uint8)((c + 50).Lsh(n))` gives 44.
- The << forms are pre-existing: master equals the tip on cst and cst3.
- For >>, D8's new rule ("a guarded >> is not a producer, because its receiver has the operand's narrow type") is false for these receivers. `(c+50)>>n`, `(-d)>>n` and `(d+20)>>n` box int32 at the tip. ff71ab6a9e emitted `(uint8)((c + 50).Rsh(n))`, which had the right type. That is a regression against ff71ab6a9e, not against master, and it affects the type only.
- The new docs section (native-and-narrow-integers.md) says the guarded shift "needs no cast of its own". That statement is false for this class.

**Skeptic residual.** 1. (Behaviour, Go-divergent; pre-existing on master for <<, and a type-only regression against ff71ab6a9e for >>.) Constant compound receivers of a guarded shift still bind the int32 GoShift overload. Examples: `(c+50)<<n`, `(-d)<<n`, `(c<<1)<<n`, `(^d)<<n` and `(c>>1)<<nn`, plus untyped `(100+50)<<n` in a narrow context.
   - Suggested minimal fix: in emitGuardedShift, cast the receiver to the shift's basic type, `((uint8)(c + 50)).Lsh(n)`, whenever unparen(X) has a constant value (info.Types[X].Value != nil) and is not a bare identifier.
   - The cast is legal because a valid Go constant is representable in its type, and C#'s int32 evaluation of these small operands is exact. The unsigned `^` case already renders `unchecked((uint8)(~c))`.
   - Add an arm to NarrowArithmeticSinks: `const c uint8 = 100; const d int8 = 100`, with `(c+50)<<n`, `(-d)<<n` and `(c+50)>>n` reaching an `any` or %T, and `v/((c+50)<<n)` as a divisor. It is red at the tip, at ff71ab6a9e and at master.
   - Correct the docs sentence "a guarded shift needs no cast of its own" at the same time.
2. (Reads-like-Go nit; optional.) The typed-destination and comparison arms still wrap the guarded shift, so the tip emits a double cast: `int8 x = (int8)(((int8)(a + a)).Rsh(n));` (NarrowArithmeticSinks main.cs.target:205 and 207) and `uint8 t8 = (uint8)(((uint8)(u + w)).Lsh(s));`. The outer cast pattern is master's own (`int8 z8 = (int8)(s8.Rsh(kk))`), so this is not a D8 regression. However, the docs use that double-cast line to illustrate "needs no cast of its own".
3. (Comment staleness in narrowArithmeticOperations.go at the tip.) All of these should say "native `<<`" / "native `>>`":
   - line 23 still lists "the left operand of `<<`" as invariant
   - line 27 lists a "`>>` operand" without "native"
   - line 55 says `<<` "already narrows the whole result"
   - line 99 lists `>>` as a type-only producer
4. (Out of scope; separate seat; pre-existing.) shiftGuardWidth does not Unalias, so alias-typed operands keep the native, count-masking form. Probe alias at the tip: `type W = uint32; y<<40` gives 25600 vs Go 0; `type B = uint8; x<<33` gives 200 vs 0; `x>>33` gives 50 vs 0.

#### D4: switch case values and tags under an INTERFACE tag (type-only producers %, unsigned /, >>, unary +, signed ^ must match Go): verifier partial

**Evidence.** I built converters at 32da9c3826 (re-cut), ff71ab6a9e (reviewed) and d4a9dfcae1 (MinInt base) with the pinned Go 1.24.13, then converted println-only probes and compiled and ran the C# against golib from 32da9c3826. Everything is in <scratch> One note: my first `git show` redirect briefly wrote review.md into coord-scratch\ itself; I moved it into d4\ within a minute. The main checkout is untouched and bin/obj are purged.

Fix (narrowArithmeticOperations.go at 32da9c3826): a CaseClause whose SwitchStmt, found at stack[i-2], has an interface Tag now gives narrowConsumerTyped. A SwitchStmt Tag gives Typed when switchHasInterfaceCase is true.

1. The silent defect D4 describes is CLOSED wherever the case is emitted through AreEqual. Probe p2 (p2\main.go) has 25 shapes. They cover the review's own snippets (S04: h>>1 and u/3; S08: a%51, b>>1 and the list `b>>1, +b, ^b`; S21: a%7 and u>>1) plus: variable divisors (the golib rem/quo path), a named `type I any` tag, a labelled switch with an init statement, a struct-field tag, int16/uint16, guarded `h>>n` and `(a+a)>>n`, a generic `any(x)` tag, a closure, a nested switch, and unary +/^ (plain and wrap-carrying) and a parenthesized label reached through AreEqual.
   - Go prints hit for all 25.
   - The 32da9c3826 run is IDENTICAL to Go.
   - The ff71ab6a9e run MISSES 23 of the 25; for example it emits `AreEqual(exprᴛ1, a % 7)` where the re-cut emits `AreEqual(exprᴛ1, (int8)(a % 7))`.
   - d4a9dfcae1 emits byte-identically to ff71ab6a9e, so the fix is in the re-cut commit.

2. An arm would catch a regression. The new "interface case" arm in NarrowArithmeticSinks (h>>1 and u/3) is the same code as my probes s04a and s04b:
   - Go prints "match match".
   - With ff71ab6a9e the runtime result is miss/miss.
   - Converting the arm's main.go with the 32da9c3826 converter reproduces main.cs.target exactly, after normalizing line endings.
   - The ff71ab6a9e converter differs from the golden on exactly those two AreEqual lines.
   %, unary + and signed ^ have no interface-tag arm of their own, but they go through the same CaseClause branch, so undoing it turns the arm red.

**Residual.** Two compile errors remain, both present at all three refs (probe p1\main.go; errors in out-recut and out-orig):

(a) OPEN, the TAG MIRROR, which C2's D4 claim covers ("a case value or tag compared through an interface is typed"). For `var t any = int8(2); switch a % 7 { case t: }`, which Go runs as a hit, the re-cut now casts the tag (`var exprᴛ1 = (int8)(a % 7);`). But the case is still emitted as `if (exprᴛ1 == t)`, and C# rejects it with CS0019 (Operator '==' cannot be applied to 'sbyte' and 'object'); ff71ab6a9e fails the same way on 'int' and 'object'. The same happens for `switch u >> 1 { case t3: }` and for a tag with a constant case followed by an interface case. The review's S08 said this comparison should route through AreEqual. Until it does, the tag half of the Typed rule cannot be exercised, and it has no arm.

(b) PRE-EXISTING and outside the narrow classification: when every label in a clause passes canUsePatternMatch, a non-constant UnaryExpr or ParenExpr label gets the `is` pattern form. Examples: `case +b:`, `case ^b:`, `case (a % 7):`, `case +(a+a):`, `case ^(a+a):`, and lists such as `case +b, 0:`. Under an interface tag these emit `exprᴛ1 is (int8)(+b)`, which fails with CS0150; ff71ab6a9e emits `is +b`, which fails the same way. A concrete tag hits it too: `switch v { case -b: }` becomes `exprᴛ1 is (int8)(-b)` (CS9135), and `case +b:` gives CS0266 plus CS9135 (probe p3). visitSwitchStmt.go is identical at 32da9c3826, ff71ab6a9e and origin/master. The fix is for canUsePatternMatch to also turn away non-constant UnaryExpr and ParenExpr labels; that is its own seat, not C2's.

Both residuals fail to compile rather than giving a wrong value silently. Also missing: interface-tag arms for %, unary + and signed ^ (they only share the CaseClause branch), and any arm for the mirror.

**Skeptic: partial.** I reproduced this myself in <scratch> I did not reuse the earlier verifier's binaries. I built my own converters at 32da9c3826 (re-cut) and ff71ab6a9e (reviewed) with GOROOT set to <profile>\sdk\go1.24.13 and GOTOOLCHAIN=local. I also built two single-site reverts of the re-cut:
- nocase: the CaseClause interface-tag branch is disabled with `&& false`.
- notag: the SwitchStmt switchHasInterfaceCase branch is disabled the same way.
All C# was built with dotnet10 against golib from 32da9c3826, with TEMP, TMP and NUGET_PACKAGES inside my folder. bin/obj and caches are purged, and the main checkout is untouched.

1. The case-value half is FIXED. Probe pa\main.go has 18 lines. It covers the review's own snippets: S04 (h>>1 and u/3), S08 (ua%51, and `b>>1, +b, ^b` over 3 tag values) and S21 (a%7 and u>>1). It adds a constant+producer list with a negative %, a named `type I any` tag with int16 %, uint16 >>, a call tag with a variable divisor, a variable unsigned divisor, guarded h>>n and (a+a)>>n, an init statement, and default-first with fallthrough.
   - Go prints hit for every line. The re-cut C# output is IDENTICAL to Go.
   - ff71ab6a9e misses 16 of the 18. For example it emits `AreEqual(exprᴛ5, a % 7)` where the re-cut emits `AreEqual(exprᴛ5, (int8)(a % 7))`.
   - nocase alone misses 15 of the 18: the same lines as ff71 except X7, a guarded shift that D1/D3 fix. So the CaseClause branch is what closes D4.

2. The arm catches a regression of the case half. I converted NarrowArithmeticSinks/main.go with each converter:
   - The re-cut output equals main.cs.target (line endings normalized).
   - nocase differs from the golden on exactly the two "interface case" lines: `(int8)((h >> (int)(1)))` becomes bare, and `(uint8)(u / 3)` becomes bare.
   - Go prints "match match" for this arm, so the behavioral runner would go red too.
   - %, unary + and signed ^ have no interface-tag arm of their own, but they are classified through the same branch.

3. The tag half has NO arm. notag reproduces both the NarrowArithmeticSinks and the NarrowArithmeticCompileSinks goldens byte-for-byte.

**Skeptic residual.** Two gaps remain. Both are compile errors that were already there at ff71ab6a9e (not silent wrong values). visitSwitchStmt.go is identical at ff71ab6a9e, 32da9c3826 and origin/master.

(a) The TAG MIRROR can never be reached as emitted, and it has no arm.
- The re-cut now casts the tag: `var exprᴛ1 = (int8)(a % 7);`.
- But tagNeedsAreEqual in visitSwitchStmt.go is set only when the TAG is an interface, so an interface CASE is emitted as `exprᴛ1 == t`. C# rejects that with CS0019 ('sbyte' and 'object'; ff71 fails the same way with 'int' and 'object').
- Probe pc covers the mirror for %, >> in a list with a constant, unsigned /, unary + and signed ^. Go prints hit for all five.
- I hand-routed those comparisons through AreEqual and compiled the result. The re-cut then prints hit on all 5, while ff71 and notag print miss on all 5. So C2's tag classification is correct and needed, but no compilable program reaches it until the emitter uses AreEqual for an interface case. That is the fix S08 asked for ("should route through AreEqual"). Until then the half can have no arm, and a revert of it would go unnoticed.

(b) Pattern-form labels (probe pb). When every label in a clause passes canUsePatternMatch, the case is emitted in the `is` form, and a non-constant label there fails to compile with CS0150. Under an interface tag this hits D4's own operators:
- `case +b:` emits `exprᴛ1 is (int8)(+b)`.
- `case ^b:` emits `exprᴛ1 is (int8)(~b)`.
- `case (a % 7):` emits `exprᴛ1 is (int8)(a % 7)`.
- The lists `case +b, 0:` and `case ^(a + a), 1:` fail the same way.
Go runs all of these as hit. ff71 emits `is +b` and fails identically. The fix belongs in canUsePatternMatch: turn away non-constant UnaryExpr and ParenExpr labels. That is a separate seat, outside the narrow pre-pass.

My verdict: the silent int32-boxing blocker D4 described is closed and armed for the case half. The item stays partial because the tag half is unarmed and unreachable behind (a), and D4's own unary + and signed ^ shapes do not compile as single labels because of (b).

#### D5: a map index, and a map-literal key, through a type parameter with a map core (M ~map[int8]string): verifier closed

**Evidence.** The fix, from git diff d4a9dfcae1 32da9c3826: a new helper, narrowMapType(t), resolves a *types.TypeParam container through the existing typeParamMapCore before looking for *types.Map. Both places the review named now use it: narrowConsumerOf's IndexExpr case and narrowDestinationType's IndexExpr case.

How I tested: I built both converters (32da9c3826 and ff71ab6a9e) with GOROOT <profile>\sdk\go1.24.13, GOTOOLCHAIN=local and CGO_ENABLED=0. Each probe went into its own output root under <scratch> I compiled with dotnet10 (TEMP, TMP and NuGet caches all inside the scratch folder), using golib and go2cs-gen archived at 32da9c3826. Go's own output came from `go run`.

1. The review's own snippets (probe p3), plus the assignment twin:
- The snippets are S05 gmap m[h>>1], S11 mapGen m[a>>1] and S17 genIdx over ~map[uint8]int with m[u>>1]. The twin is m[h>>1] = x.
- At ff71ab6a9e: 4 errors, all CS1503 (int to sbyte / int to byte), at main.cs lines 12, 18, 24 and 33.
- At 32da9c3826 it emits `return m[(int8)((h >> (int)(1)))];` and `m[(uint8)((u >> (int)(1)))]`. It builds, and the output matches Go byte for byte (ok / ok2 / 7 / set 1).

2. Wider shapes (probe p4):
- Covered: a ~map[byte]int key u%7, which is emitted `(byte)(u % 7)` (the destination spelling now reaches through the core); a ~map[int16]int key w>>1; an assignment, a `+=`, a comma-ok read and delete(m, h>>1); a wrap-risk key a+a; and the keys ^h, +(h>>1) and h%7.
- At ff71ab6a9e: 8 CS1503 errors plus 2 CS8130.
- At 32da9c3826: builds, and matches Go (44 / 16 / set! true 0 / wrapped / xyz).

3. The arm:
- NarrowArithmeticCompileSinks' genericKey is the same shape as S05.
- Converting the test's main.go with 32da9c3826 gives output byte-identical to main.cs.target (ignoring line endings).
- With ff71ab6a9e the only difference from the golden is exactly the D5 line (`return m[(h >> (int)(1))];`), and that line is the CS1503 shown in step 1. So a return of D5 would be red twice: in the golden and at compile time.
- The map-literal-key arm in that test uses a concrete map, not a type parameter.

4. A map-literal key through a type parameter:
- The key itself is correct at both refs. The KeyValueExpr parent falls to the Typed default, so `M{h>>1: "lit"}` emits the key as `(int8)((h >> (int)(1)))`.
- The literal as a whole does not compile at either ref. It emits `new M{(int8)(...): "lit"u8}`, which is not C#.
- Control probe p5, `M{50: "lit"}` with constant keys and no narrow arithmetic at all, emits `new M{50: "lit"u8}` byte-identically at both refs and fails with CS1003.
- So this is an existing gap in composite literals of a type-parameter type, not a narrow-pass defect.

**Residual.** None of the following is a D5 defect:

(a) EXISTING GAP, outside this seat: a composite literal whose type is a type parameter with a map core is emitted as `new M{k: v}`. That is CS1003 even with constant keys, and it is the same at both refs (control probe p5). So "a map-literal key through a type parameter compiles and matches Go" cannot be shown at any ref until that is fixed. Candidate for its own seat.

(b) Cosmetic: the KeyValueExpr branch of narrowDestinationType still reads TypeOf(composite).Underlying().(*types.Map) and does not use narrowMapType. A byte-keyed type-parameter literal therefore spells its key cast `(uint8)(u / 3)` instead of `(byte)`. It doesn't matter until (a) is fixed.

(c) EXISTING GAP, outside this seat: a named constraint (`type I8 interface{ ~map[int8]string }; [M I8]`) is emitted as `where M : /* I8 */ new()` with no IMap constraint. Any index on it is CS0021, even with a constant key, and the emission is the same at both refs (control probe p6). A constraint that embeds another named interface would also make typeParamMapCore return nil. That case is moot here because of the same missing constraint.

(d) Housekeeping: my first diff redirect landed briefly in cloudrev\ itself. I moved it into cloudrev\d5\ straight away, so nothing of mine is left outside d5.

All bin/obj folders, TEMP and the NuGet caches under d5 are purged. The converter binaries, the emitted .cs files and the build logs remain in <scratch> and \armout. H:\Projects\go2cs is untouched: git status still shows only the stackdump that was there before.

#### D6: byte vs uint8 spelling through a named func type ('type F func(byte)') and the compound double-cast claim: verifier partial

**Evidence.** Setup: I built the converter at four refs in <scratch> 32da9c3826 (re-cut), ff71ab6a9e (reviewed), d4a9dfcae1 (MinInt top) and base 1aebd6a885. Base's src/go2cs matches origin/master except for repoguard tests, so base is master's emission. I converted the review's S10 snippet plus variants (probe/p1/main.go) and took ground truth from `go run` (f 44 / f 100 / f 32; x1 0, x2 9, y1 4, y2 8).

Both S10 shapes are fixed in the emission:
- ff71ab6a9e reproduces the review exactly: `f((byte)((uint8)(u + w)))`, `h.fn((byte)((uint8)((u >> (int)(1)))))`, `x /= (byte)((uint8)(u + w))`, `x %= (byte)((uint8)(u * w))`.
- 32da9c3826 emits `f((byte)(u + w))`, `h.fn((byte)((u >> (int)(1))))`, `x /= (byte)(u + w)`, `x %= (byte)(u * w)`. That is byte-identical to base on these lines.
- The variants are also single-cast at the re-cut, where ff71ab6a9e double-cast them: a pointer field `(~hp).fn`, a generic named func type `G[byte]`, a variadic named func type `vv((byte)(u + w), (byte)(u * w))`, the reverse spelling `type F8 func(uint8)` with byte operands (`f8((uint8)(b1 + b2))`), a func with a result, `y /= b1 + b2` over a uint8 y, and `arr[0] /= u + w`.
- Against base, the re-cut adds text only at sites where base had no cast at all: `f((u + w))`, the second `vv` argument, `x /= (u + w)`, and the shift-assign counts. The typed destinations that base already cast are unchanged. d4a9dfcae1 and ff71ab6a9e emit identically on p1.
- Mechanism: narrowDestinationType now reads `TypeOf(Fun).Underlying().(*types.Signature)`, and markNarrowArithmeticContexts gives a wrap-risk value consumer the destination spelling when the types are identical.

Does an arm catch a return? I tested this by mutation (safety-floor rule 13), converting both test projects at 32da9c3826 and diffing against the goldens:
- The unmutated re-cut matches both goldens.
- Mutant m1 restores `types.Unalias(TypeOf(Fun)).(*types.Signature)`. NarrowArithmeticSinks goes red at main.cs.target:229 (the "named func param" arm: `f((byte)((uint8)(u + w8)))`). Shape (a) is guarded.
- Mutant m2 reverts only the value-consumer destination spelling. Both goldens stay byte-identical, while the same m2 converter reproduces `x /= (byte)((uint8)(u + w))` on p1 (the positive control). Shape (b) has no arm. The only compound divisor in any Behavioral golden is the "divisor" arm's `q /= (uint8)(u + u)`, where q is `q := d` (uint8) and the operands are uint8, so the spellings match and it cannot tell the fixed text from the regressed one.
- The corpus gives no guard either. The re-cut footprint was 0/0/0, and a git grep for `(byte)((uint8)(` and `(uint8)((byte)(` at 32da9c3826 finds only 10 lines. All of them already exist at base, from an unrelated `&`/`|` result arm, for example jpeg/scan.cs:130.

**Residual.** 1. The shape (b) fix (a compound `/=` or `%=` divisor over a byte LHS with uint8 operands) has no guarding arm. Reverting it leaves every golden green. Suggested arm: in NarrowArithmeticSinks "divisor", declare the LHS as byte with uint8 operands, for example `var q byte = d; q /= u + u; q %= u * w8`, so the golden pins `q /= (byte)(u + u)` and `q %= (byte)(u * w8)`.

2. A new cosmetic side effect of the same fix. narrowDestinationType's AssignStmt case returns the LHS type for SHL_ASSIGN and SHR_ASSIGN too, although a shift count is not assigned to the LHS. So `x <<= u + w` over a byte x now spells its count `x.LshAssign((uint64)((byte)(u + w)))`, where ff71ab6a9e emitted `(uint8)`, the operands' declared spelling. Base emitted no cast there, which is Go-divergent for sums that wrap below 256. The cast is an identity cast (byte and uint8 are both System.Byte), so only the spelling changes. It fires only when the count's type is identical to the LHS type, and it has no corpus sites. Suggested fix: return nil for SHL_ASSIGN and SHR_ASSIGN in that case.

3. Trivial: Underlying() does not see through a type parameter's func core (`func call[FN ~func(byte)](f FN) { f(u + w) }`). The emission is a single `(uint8)` cast in the operand's own spelling, with no double cast. It is the same at ff71ab6a9e, and base emits no cast there at all. Whether the generated C# for calling a func-typed type parameter compiles at all was not checked.

I did no C# build: every cast involved is between byte and uint8, which are both System.Byte, so behaviour cannot change. Nothing was written to H:\Projects\go2cs or tG, and no bin/obj folders were created.

**Skeptic: partial.** I reproduced this myself in <scratch> I built the converter at 32da9c3826 (re-cut), ff71ab6a9e (reviewed) and 1aebd6a885 (base, which is master's emission), plus two mutants of the re-cut. Ground truth came from `go run`: f 44, h 100, x1 0, x2 9.

What the S10 snippet emits:
- ff71ab6a9e reproduces the review exactly: `f((byte)((uint8)(u + w)))`, `h.fn((byte)((uint8)((u >> (int)(1)))))`, `x /= (byte)((uint8)(u + w))`, `x %= (byte)((uint8)(u * w))`.
- 32da9c3826 emits `f((byte)(u + w))`, `h.fn((byte)((u >> (int)(1))))`, `x /= (byte)(u + w)`, `x %= (byte)(u * w)`. These lines are byte-identical to base.

Variants, all single-cast at the re-cut where ff71ab6a9e double-casts them:
- a pointer field, `(~hp).fn((byte)(u + w))`
- `type F8 func(uint8)` with byte operands, `f8((uint8)(b1 + b2))`
- a named func type with a result (FR)
- a variadic named func type, `vv((byte)(u + w), (byte)(u * w))`
- a generic named func type, G[byte]
- `y /= b1 + b2` over a uint8 y, and `arr[0] /= u + w`
- local aliases, which I added: `type B = uint8` / `type BY = byte`. `x /= (B)(b1 + b2)`, `y %= (BY)(u * w)`, `fb((B)(b1 + b2))` and `fby((BY)(u + w))` are all identical to base.
- a call on a returned func, `mk()((byte)(u + w))`

Against base, the re-cut adds a cast only where base had none. These are Go-correctness gains, since base passed an int-promoted value.

The mechanism: narrowDestinationType now reads `TypeOf(Fun).Underlying().(*types.Signature)`. markNarrowArithmeticContexts also gives the destination spelling to a Value consumer with wrap risk when the types are identical.

Arm check by mutation (safety-floor rule 13). I converted the two test projects, taken from 32da9c3826, and diffed against their .cs.target files:
- The unmutated re-cut matches both goldens (0 diff lines).
- m1 restores `types.Unalias(TypeOf(Fun)).(*types.Signature)`. NarrowArithmeticSinks goes red at main.cs.target:229, the "named func param" arm, now `f((byte)((uint8)(u + w8)))`. Shape (a) is guarded.
- m2 gates the destination spelling back to Typed consumers only. Both goldens stay byte-identical. The same m2 converter on S10 gives back `x /= (byte)((uint8)(u + w))` and `x %= (byte)((uint8)(u * w))`, which is the positive control. Shape (b) has no arm.

Why nothing else catches m2:
- A git grep of every src/tests .cs.target file at 32da9c3826 for compound `/=`, `%=`, LshAssign, RshAssign, `<<=` or `>>=` lines with a byte or uint8 cast finds only two:
  - `q /= (uint8)(u + u)`, where q is `q := d`, so it is uint8 over uint8 operands and the spelling already matches.
  - `y.LshAssign((uint64)((uint8)(d + 10)))`, where y is uint32, so the types are not identical.
- The corpus gives no guard either. The double-cast count (`(byte)((uint8)(` plus `(uint8)((byte)(`) in src/core is 10 at base, 1caee7e1da, d4a9dfcae1 and 32da9c3826, all from an unrelated `&`/`|` arm. There are no compound byte-divisor sites, and the corpus shift-assign sites have int64 left-hand sides.

No C# build was needed: golib.csproj:80 declares `<Using Include="System.Byte" Alias="uint8" />`, so every cast involved is an identity cast. Nothing was written to H:\Projects\go2cs (git status unchanged) or to tG, and no bin/obj folders were created.

**Skeptic residual.** 1. Shape (b) is fixed but has no guarding arm: a compound `/=` or `%=` divisor over a byte left-hand side with uint8 operands. Reverting the value-consumer destination spelling (mutant m2) leaves every golden green. Suggested arm, in NarrowArithmeticSinks "divisor": declare the left-hand side with the other spelling, e.g. `var q byte = d; q /= u + u; q %= u * w8`. The golden then pins `q /= (byte)(u + u)` and `q %= (byte)(u * w8)`, and m2 should turn it red.

2. A new cosmetic side effect of the same fix. narrowDestinationType's AssignStmt case returns the left-hand side's type for SHL_ASSIGN and SHR_ASSIGN too, even though a shift count is not assigned to that side. So `z <<= u + 60` over a byte z emits `z.LshAssign((uint64)((byte)(u + 60)))`, where ff71ab6a9e wrote `(uint8)`, the operands' declared spelling. It is an identity cast and there are no corpus or golden sites. The cast itself, which base lacked, is a correctness gain; only the spelling is misattributed. Suggested fix: return nil for SHL_ASSIGN and SHR_ASSIGN in narrowDestinationType.

3. Out of D6's scope. A func-typed type parameter (`FN ~func(byte)`) is not seen by Underlying(). The re-cut emits `f((uint8)(u + w))`, the same as ff71ab6a9e, with no double cast. But the emitted constraint `where FN : /* ~func(byte) */ /* ~func(byte) */, new()` shows this shape cannot compile at any ref, so it is not a D6 regression.

#### D9 (coverage arms) and completeness of the D table's arms at origin/claude/c2-narrow-minint 32da9c3826, plus an S01-S24 consumer sweep: verifier partial

**Evidence.** Method: I built the converter at 32da9c3826, ff71ab6a9e and 1aebd6a885, all in <scratch> 1aebd6a885 stands in for master: since then origin/master has changed only projitems and the repoguard tests under src/go2cs. I converted the NarrowArithmeticSinks and NarrowArithmeticCompileSinks sources from 32da9c3826 with each converter, each into its own output root. I built them against golib, gen and core from 32da9c3826 using dotnet10 and ran them. `go run` gave ground truth.

THE NEW ARMS ARE REAL.
- At 32da9c3826 the emission is byte-identical to the committed main.cs and main.cs.target in both projects. Both runs match Go exactly.
- At ff71ab6a9e exactly the four claimed runtime arms are red:
  - interface case: `nomatch`, Go `match match`
  - guarded shr: `100 200 false 64 -100 0`, Go `-28 72 true -64 28 -1`
  - guarded shl: `400 400 -123 200 76800`, Go `-112 -112 -37 -56 0`
  - unary xor plus: panics with index 400, Go `55 27 27 144 16`
- CompileSinks at ff71ab6a9e fails with CS1503 at genericKey's `m[(h >> (int)(1))]`, main.cs(12,15). That is D5.
- D8: NarrowShiftVarCount at 32da9c3826 matches its target. ff71ab6a9e emits `(byte)(cb.Rsh(k))` and `(int8)(s8.Rsh(k))`, so the golden compare is red.
- D6a: ff71ab6a9e emits `f((byte)((uint8)(u + w8)))`; the target is `f((byte)(u + w8))`. Only the golden text catches this, since the values agree.
- The coverage arms are red at master:
  - case value and case comparison print `unwrapped`
  - make and bounds panics with `[::400]`
  - divisor prints `0 250 1`, Go `1 106 1`
  - negated minimum prints `128 128`
  - invariant prints `a+a+a` as 300
  - CompileSinks gives CS1503 on the map-literal key and on the type-parameter key
- A mutation shows the D1 arm catches its own return. In a copy of 32da9c3826 I dropped `|| v.narrowArithmeticRendersAsCast(binaryExpr.X, leftOperand)` from emitGuardedShift. The emission went back to `(int8)((int8)(a + a).Rsh(n))`: the golden goes red on the guarded shr and shl lines, and the run goes red on guarded shr (x=100). Guarded shl's values stay right under the mis-bound cast because only the low bits matter there, so only the golden text catches D1 on that line.

CONSUMER SWEEP at 32da9c3826. probe\sweep\main.go holds 52 labelled lines covering every S01-S24 snippet plus extra classes. All match Go except S12: `- -a` prints `4 5` against Go's `5 5`. That is D7, which predates this seat and is queued as its own seat. The same probe at ff71ab6a9e diverges on 37 lines and at base on 41.

**Residual.** These D-table items are still without an arm:
1. **D4's mirror half.** This is a tag compared against an interface-typed case value, handled by switchHasInterfaceCase, and it has no arm. It cannot have a runtime one yet. At all three refs the switch emitter renders `if (exprᴛ5 == ia)`, which fails with CS0019. That is also true for a plain int tag (`nint == object`, probe\swtag), so it is an older switch-emitter defect, not this seat's. The narrow cast on the tag is correct text, but nobody can see it until the switch routes through AreEqual. It should be its own seat, like D7.
2. **D6b is fixed but has no arm.** A byte x with `x /= u + w` or `x %= u * w` gives `(byte)(u7 + w7)` at 32da9c3826 against `(byte)((uint8)(u7 + w7))` at ff71ab6a9e. The divisor arm's `q /= u + u` has uint8 on both sides. Its text, `q /= (uint8)(u + u)`, and its value are the same at base, ff71ab6a9e and 32da9c3826, so it guards nothing. There is also no `%=` arm.
3. **S24's other classes are fixed but have no arm.**
   - A return inside a func literal: base prints `400 int32` and `-201`.
   - Explicit instantiation `gen[any](a+a)`: base prints `200 int32`.
   - Tuple assignment: base prints `200 int32 100 int32`.
   The review's own suggested fix did not ask for these, but S24 lists them.
4. **One imprecise claim.** The commit says the coverage arms are red at master. "named func param" is not red at master: text and value are the same there. It is correctly red at ff71ab6a9e as a D6 regression arm. The no-cast half of the "invariant" arm is checked only by the golden text.

None of these gaps is a behavioural divergence at 32da9c3826. Probes and outputs are in <scratch> (probe\sweep, probe\sweepold, probe\swtag, go-*.txt, cs-*.txt); bin and obj were purged.

**Skeptic: partial.** I reproduced this on my own, in <scratch>
- I built the converter at 32da9c3826 (recut), ff71ab6a9e (orig) and 1aebd6a885 (base). origin/master differs from base under src/go2cs only in projitems and repoguard tests, so base stands in for master.
- I converted NarrowArithmeticSinks, NarrowArithmeticCompileSinks and NarrowShiftVarCount from 32da9c3826 with each converter, each into its own output root.
- C# was built with dotnet10 against golib, gen and core from 32da9c3826. TEMP, TMP and NUGET_PACKAGES were all inside my scratch folder. Ground truth came from `go run`.

1) THE NEW ARMS ARE REAL.
- At recut the main.cs emission is byte-identical to the committed main.cs and main.cs.target in all 3 projects. package_info differs only by the probe path in GoPositionMap. All 3 runs match Go exactly.
- At orig, exactly 4 lines of the runtime arms are red:
  - interface case: `nomatch`, Go `match match`
  - guarded shr: `100 200 false 64 -100 0`, Go `-28 72 true -64 28 -1`
  - guarded shl: `400 400 -123 200 76800`, Go `-112 -112 -37 -56 0`
  - unary xor plus: `panic: index out of range [400] with length 256`
- CompileSinks at orig gives exactly one CS1503, at main.cs(12,15), genericKey's `m[(h >> (int)(1))]` (D5).
- NarrowShiftVarCount at orig: its values match Go, but the golden is red on `(byte)(cb.Rsh(k))` and `(int8)(s8.Rsh(k))` (D8).
- Named func param at orig emits `f((byte)((uint8)(u + w8)))` against the target `f((byte)(u + w8))`. Only the golden catches this (D6a).
- At base, these arms are red:
  - case comparison and case value print `unwrapped`
  - make and bounds panics `[::400]`
  - divisor prints `0 250 1`, Go `1 106 1`
  - negated minimum prints `128 128`
  - invariant prints 300 for a+a+a
  - CompileSinks gives 8 errors, including CS1503 at (46,40) on the map-literal key and at (12,15) on the type-parameter key
- The D1 mutation catches its own return. I dropped `|| v.narrowArithmeticRendersAsCast(binaryExpr.X, leftOperand)` in a copy of 32da9c3826. The emission went back to `(int8)((int8)(a + a).Rsh(n))`. The golden goes red on both the guarded shr and guarded shl lines. The run goes red only on guarded shr (`100 200 false 64 -100 0`); guarded shl's values stay right, so only the golden text catches that line.

2) SWEEP. probe\sweep\main.go has 84 labelled lines: every S01-S24 snippet plus extra classes (gmapSet, genB with a byte key, several interface-tag variants, uint16).
- At recut all 84 match Go except S12, D7's `- -a + +a`, which prints `4 5 5` against Go's `5 5 5`. D7 is pre-existing and queued as its own seat.
- orig does not compile the generic-map lines, so I ran the same probe without them (79 lines): 62 diverge at orig. At base, with the map-literal key also removed (78 lines), 74 diverge.
- d6b probe: recut `x /= (byte)(u7 + w7)` and `y %= (byte)(u7 * w7)`, which is master's text. Orig emits `(byte)((uint8)(...))`. Values match Go (`4 8 1 106`) at all three refs.

3) A NEW PRE-EXISTING DEFECT, not this seat's. A case list made only of non-constant unary values becomes a C# constant pattern:
- `if (exprᴛ1 is -y)` for a plain int, and `is (int8)(+b) or (int8)(~b)` for int8.
- Both fail with CS0150, at base and at recut, so there cannot be a single-value `case +b:` or `case ^b:` interface arm yet.
- Mixed lists do route through AreEqual, and those match Go at recut.

**Skeptic residual.** No behavioural divergence is left at 32da9c3826 other than D7 (S12). These D-table items still have no arm:

(a) D4's mirror half, the switchHasInterfaceCase branch.
- It cannot be seen today. Every shape of a narrow tag against an interface case emits `exprᴛ == ia` and fails with CS0019. I tried a single case, `case ia, 7`, a separate `case ia` clause, and a uint8 `u+u` tag; a plain int tag fails the same way (`nint == object`).
- This is an older switch-emitter defect. It needs its own seat, routing through AreEqual, before this branch can get an arm.

(b) D6b is fixed but has no arm.
- The divisor arm's `q /= u + u` emits `q /= (uint8)(u + u)`, identical at base, orig and recut, so it guards nothing.
- There is no `%=` arm at all.
- Only a byte left side fed uint8 arithmetic catches this. That branch is the re-cut's new `consumer == narrowConsumerValue && wrapRisk` destination-spelling path.

(c) D5's narrowDestinationType half has no arm. It is spelling only: with a `~map[byte]int` key, recut emits `(byte)`. genericKey uses int8 on both sides, so it cannot tell the two spellings apart.

(d) Three S24 classes are fixed but have no arm with their exact shape:
- a return in a func literal (base prints `400 int32` and `-201`)
- `gen[any](a+a)` (base prints `200 int32`)
- tuple assignment (base prints `200 int32 66 int32 200 int32 50 int32`)
They go through the same default-Typed classification as the existing any return, generic and paren assign arms. The FuncLit branch in narrowDestinationType affects only spelling, so the gap is low risk.

(e) One imprecise claim in the commit. "Coverage arms are red at master" is not true of "named func param": its text and value are the same at base. It is red at orig, through the golden only. The no-cast half of "invariant" is checked only by the golden text.

(f) The pre-existing CS0150 on a case list made only of unary values should be queued as its own seat, like D7.

bin and obj were purged from my scratch folder, along with the nuget, tmp and dotnethome caches. The probes, outputs and logs remain (241 MB).

### Review: minint-semantics

#### (blocker; skeptics refuting 0 of 2) When a compound `x /= b` or `x %= b` on a signed int, int32 or int64 has a target that is not "read twice without effect", the converter falls back to the plain C# operator. At MinInt / -1 that still throws OverflowException, which recover() cannot catch, so the program dies. This is exactly the bug the seat exists to close, and it remains open for ordinary Go shapes.

**Evidence.** Every one of these probes prints the correct value under Go and exits 2 with an unhandled System.OverflowException in C#: rptr `*p /= int64(m1)`, ridx `sl[i+1] /= int32(m1)`, rlen `sl[len(sl)-1] %= m1`, rcall `m[key()] /= int64(m1)`, rfret `f().x /= int64(m1)`, rparen `(*p).x %= int64(m1)`. isSideEffectFreeLvalue (signedDivisionOperations.go:147) accepts only an identifier, a selector chain, or an index whose base and index are identifiers, selectors or literals. It rejects StarExpr, and it rejects any index that is itself an expression (i+1, len(s)-1). visitAssignStmt.go:1019 then leaves `operator = " /= "`. Census with a go/types scan of Go 1.24.13 std on windows: 0 such sites in production code (22 governed compounds, all simple targets) and 0 in _test files (32, all simple). So the corpus is unaffected today, but user code (-recurse) is not.

**Snippet.** rptr emitted:  p.Value /= (int64)m1;        // Go: -9223372036854775808; C#: System.OverflowException
ridx emitted:  sl[i + 1] /= (int32)m1;
rlen emitted:  sl[len(sl) - 1] %= m1;

**Suggested fix.** Extend isSideEffectFreeLvalue to a StarExpr whose operand is simple (`*p`, `(*p).x`, which emit as p.Value). Extend isSideEffectFreeOperand, for index positions only, to pure arithmetic: Binary or Unary expressions (never `<-`) over simple operands, plus builtin len/cap of a simple operand. Reading such a target twice can only repeat the same first panic. For targets that contain a call or a receive, evaluate once into a temporary: `ref var t = ref <lvalue>; t = quo(t, b);` for a slice, array, pointer or field; for a map, hoist the key: `var k = key(); m[k] = quo(m[k], b);`. Never fall back to the bare C# /= or %= on a governed type. Add rptr, ridx and rcall arms to MinIntDivide.

#### (concern; skeptics refuting 0 of 2) A generic division whose type parameter has a signed core type (~int64, ~int32, ~int), when instantiated with an unnamed type, still throws OverflowException at MinInt / -1 and kills the process. Instantiating with a named type is fine because the wrapper operator carries the -1 check.

**Evidence.** rgen64: `func div[T ~int64](a, b T) T { return a / b }` emits `return a / b;` under IDivisionOperators<T,T,T>. `div(I64(mi64), I64(m1))` prints MinInt64 correctly. `div(mi64, int64(m1))` dies with System.OverflowException at System.Int64...IDivisionOperators...op_Division (Go prints -9223372036854775808). The same happens in pmain for divAny[~int8|~int16|~int32] instantiated with int32. The int8 and int16 instantiations are correct, because .NET narrows the result back. signedDivisionGuardKind returns false for a *types.TypeParam. The ruling names only the basic int, int32 and int64 types, so this is a scope gap to rule on, not a deviation from the ruling. Census: 0 generic signed division or compound sites in std (production or tests).

**Snippet.** internal static T div<T>(T a, T b) where T : ... IDivisionOperators<T, T, T>, IDecrementOperators<T>, IComparisonOperators<T, T, bool>, IUnaryNegationOperators<T, T> ...
{ return a / b; }   // T=int64: OverflowException

**Suggested fix.** Ask COORD or the owner for a ruling on extending the governed set to type parameters whose type set includes int, int32 or int64. If extended, a golib generic helper needs only interfaces the converter already emits, for example `static T quo<T>(T a, T b) where T : IDivisionOperators<T,T,T>, IUnaryNegationOperators<T,T>, IDecrementOperators<T>, IComparisonOperators<T,T,bool>, new() { T m1 = new(); m1--; return m1 < new T() && b == m1 ? -a : a / b; }`, and similarly for rem, returning new T(). For an unsigned T, m1 wraps to MaxValue and fails the `< 0` test, so unsigned division is untouched.

#### (concern; skeptics refuting 0 of 2) The folded `x % -1` drops the evaluation of a selector dividend that dereferences a pointer, so Go's nil-pointer panic disappears. isSideEffectFreeOperand treats any selector chain as free of side effects, but every hop through a pointer (explicit, or promoted through an embedded pointer) is a nil check.

**Evidence.** pmain arms 'H rem fold nil sel' and 'H rem fold nil sel2'. With `var ps *S`, Go panics 'runtime error: invalid memory address or nil pointer dereference' for both `r := ps.x % -1` and `ps.n%-1 == 0`. The emitted C# is `var r = (int64)0;` and `(nint)0 == 0`, and prints 'no panic 0' and 'no panic true'. The quotient fold keeps the evaluation (`unchecked(-(~ps).x)`) and matches Go. signedDivisionOperations.go:102 (the `minusOne && isSideEffectFreeOperand(dividend)` case) and :133. Census: 0 std sites of a selector `% -1` on a governed type.

**Snippet.** Go:  r := ps.x % -1      // panics: nil pointer dereference
C#:  var r = (int64)0;   // no panic

**Suggested fix.** Fold to (T)0 only when the dividend is an identifier, a literal, a package-qualified identifier, or a selector chain in which no hop's X has pointer type (types.Pointer after Unalias/Underlying) and no hop is promoted through an embedded pointer (check the selection's Indirect()). Otherwise emit rem(x, -1), which evaluates x as the index-dividend path already does (`rem(sl[0], -1)` keeps the bounds check).

#### (concern; skeptics refuting 0 of 2) The rule that switches to `builtin.quo` / `builtin.rem` misses two Go bindings that shadow the helper in C#: a local constant, and a type-switch binding. Both produce a C# compile error.

**Evidence.** s2: `const quo = 3; return a/b + quo` emits `const nint quo = 3; return quo(a, b) + quo;` giving CS0149 'Method name expected'. s3: `r := a % b; const rem = 10` gives CS0841 'Cannot use local variable rem before it is declared'. s5: `switch quo := any(a).(type) { case int: return quo / b }` emits `case nint quo: { return quo(quo, b); }` giving CS0149 twice. funcScopeVarNames (variableAnalysisOperations.go:377) records only *types.Var from info.Defs. Local constants are *types.Const, and a type-switch symbol's per-clause objects live in info.Implicits (its Defs entry is nil). Every other case I probed spells correctly: a local declared after the division, a local only inside a nested closure, an outer local captured by a closure that divides, named results, a package-level func literal's local, a for-init local, and a receiver. A local type is renamed (f5_rem), so it does not collide. Census: 0 local const or type-switch bindings named quo or rem in std. Because this is a compile error, it is loud, not silent.

**Snippet.** const nint quo = 3;
return quo(a, b) + quo;   // CS0149
...
case nint quo: { return quo(quo, b); }   // CS0149

**Suggested fix.** In signedDivisionHelperName, qualify when the enclosing function declares any object named quo or rem: every info.Defs object of any kind (Var, Const, TypeName) plus info.Implicits[*ast.CaseClause] objects, all within the funcDecl's extent. The simplest route is a seat-local pre-pass per function that collects the names, instead of widening funcScopeVarNames, which boxAccessorType also consumes.

#### (concern; skeptics refuting 0 of 2) A constant -1 divisor is not folded in two shapes, and golib's helper is called with a constant divisor. That goes against the ruling ("fold a constant -1 divisor at conversion"; quo/rem ONLY where the divisor is not constant) and against quo/rem's own XML remarks ("a constant -1 divisor is folded when converting"). The results themselves are correct.

**Evidence.** pmain 'B fold compound': `a /= -1; c %= -1` emits `a = quo(a, -1); c = rem(c, -1);` because visitAssignStmt.go:1021 takes `guard || minusOne` into the helper. The `minusOne && !isSideEffectFreeOperand` case (signedDivisionOperations.go:104) emits `rem(f(), -1)` / `rem(sl[0], -1)`. All of these match Go at run time, so behavior is fine, but the docs overstate what happens and golib appears at constant sites.

**Snippet.** q /= -1   ->  q = quo(q, -1);
m %= -1   ->  m = rem(m, -1);
sl[0] % -1 -> rem(sl[0], -1)

**Suggested fix.** In the compound path, when minusOne is true (the target is already known to be free), emit `x = unchecked(-x)` for /= and `x = (T)0` for %=. Writing 0 through a nil pointer still panics, exactly as Go does. For the effectful-dividend remainder, either keep rem(x, -1) and amend the ruling and the XML remarks to say so, or emit an expression that evaluates x and discards it. Whichever is chosen, make the remarks in builtin.cs and NumericTypeTemplate match what the converter actually emits.

**Info:**
- Verified correct against Go ground truth (pmain, pnarrow and pshift all MATCH byte for byte, apart from the nil-fold arms listed above).

### Review: minint-emission

#### (concern; skeptics refuting 0 of 2) The helper is used where MinInt / -1 cannot happen: at 18 of the 108 sites per target the dividend is a constant other than MinValue, or len/cap. The XML docs and the reference page claim this is exactly where Go itself checks, and that is false. The doc's own headline example is one of these sites.

**Evidence.** The overflow needs BOTH dividend == MinValue AND divisor == -1, but signedDivisionExpr only looks at the divisor. A go/types census of std (GOOS windows, linux and darwin) finds 18 of the 108 applied sites per target with a const-dividend or len/cap dividend: bytes.go:572 and strings.go:493 (maxInt / ...), cbc.go:80 and :159, shake.go:30, pem_decrypt.go:155 and :212, tls conn.go:393, elf file.go:1824 and :1872, regexp parse.go:187 and :193, gcsizes.go:130, sizes.go:203, jpeg writer.go:599 (5000/quality), png writer.go:424 (8/bitsPerPixel), pprof proto.go:289, plus netpoll_windows (len of an array is a constant) or signal_unix (1000000/hz). Go's own amd64 codegen (go build -gcflags=-S, asm\a.go) emits a bare IDIVQ for `len(s) % b` and `5000 / b`, and CMPQ BX,$-1 / NEGQ / IDIVQ only for `a % b`. So the builtin.cs remarks (lines 1551 and 1573, 'only where Go itself must check at run time, a signed division whose divisor is not a constant') and native-and-narrow-integers.md:151 ('Go itself checks for -1 at run time only where the divisor is a variable') are wrong. ConversionStrategies.md shows cbc.go:80 `rem(len(src), x.blockSize)` as its showcase, and that site needs no check at all. A divisor converted from a narrower unsigned type (probe1: `quo(a, (nint)u8)`, `quo(d, (int64)u32)`) is also provably not -1 but still gets the helper (0 corpus sites).

**Snippet.** guard, minusOne := v.signedDivisorClass(divisor)   // dividend never consulted
...
case minusOne || guard:
    return fmt.Sprintf("%s(%s, %s)", v.signedDivisionHelperName(name), left, right), true
// corpus: if (rem(len(src), x.blockSize) != 0)   nint pixelsPerByte = quo(8, bitsPerPixel);

**Suggested fix.** In signedDivisionExpr (signedDivisionOperations.go:92), keep the plain operator when the dividend is a constant whose value is not the result type's MinValue, or a builtin len/cap call. Optionally also keep it when the divisor is a conversion from uint8, uint16, or uint32-to-64-bit. This removes 18 of 108 helper calls per target. Then fix the builtin.cs remarks and the two doc paragraphs to say 'where the divisor may be -1 and the dividend may be MinInt', and re-pick the ConversionStrategies.md example (e.g. image/geom's `quo(p.X, k)`).

#### (concern; skeptics refuting 0 of 2) A compound `x /= b` whose target fails isSideEffectFreeLvalue silently keeps C#'s `/=`. MinInt / -1 then kills the process with OverflowException. The predicate also rejects targets that have no side effect (`*p`, `a[i+1]`), so the doc's Limits paragraph understates the gap.

**Evidence.** probe1 emission: `a[f()] /= m;`, `*p /= m` -> `p /= m;`, `a[i+0] %= m` -> `a[i + 0] %= m;` (all unguarded), next to `s.v = quo(s.v, m)`. At runtime, probe2 (out2, built against d4a9dfcae1's golib and gen) prints 'about to compound a[f()] /= m1' and then 'System.OverflowException ... at go.main_package.Main() main.cs:line 47', exit 2. Go prints -9223372036854775808 and exits 0. native-and-narrow-integers.md:175 says only a target 'that has a side effect when read' keeps `/=`, and does not say that the program dies. The codebase already has the single-evaluation answer: compound shifts use GoShift.cs's `ref this` twins (`x.RshAssign(n)`, 'the target is evaluated ONCE: a field, a local, and golib's ref-returning element indexers all bind'), with map index as the only exclusion. Census: 0 such sites in std, in production or tests, so this is latent.

**Snippet.** if len(assignStmt.Lhs) == 1 && len(assignStmt.Rhs) == 1 && isSideEffectFreeLvalue(assignStmt.Lhs[0]) &&
    signedDivisionGuardKind(v.info.TypeOf(assignStmt.Lhs[0])) {   // else: operator stays " /= "
// emitted: a[f()] /= m;   p /= m;   a[i + 0] %= m;

**Suggested fix.** Add golib `QuoAssign`/`RemAssign(ref this nint|int32|int64 x, T b) => x = quo(x, b)` twins, next to LshAssign/RshAssign. Keep `x = quo(x, b)` for the simple targets, which read better, and emit `target.QuoAssign(b)` for every other non-map target, reusing compoundShiftGuarded's map exclusion. At minimum, accept `*ident` in isSideEffectFreeLvalue and reword Limits to name the actual predicate and the OverflowException consequence.

#### (concern; skeptics refuting 0 of 2) A division over a generic type parameter whose type set includes int, int32 or int64 is not guarded. With an unnamed instantiation it throws OverflowException that recover() cannot see. The docs do not mention generics.

**Evidence.** probe3: `func div[T ~int | ~int64](a, b T) T { return a / b }` emits `return a / b;` under an IDivisionOperators<T,T,T> constraint. out3 run: 'generic named: -9223372036854775808' (a named instantiation works through the wrapper arm), then 'System.OverflowException at System.IntPtr.System.Numerics.IDivisionOperators<nint,nint,nint>.op_Division ... at go.main_package.div[T]', exit 2. Go prints -9223372036854775808. signedDivisionGuardKind only accepts *types.Basic. Census: 0 generic signed divisions in std (production and tests), so this is latent.

**Snippet.** basic, ok := types.Unalias(t).(*types.Basic)
if !ok {
    return false   // *types.TypeParam falls through to plain a / b
}

**Suggested fix.** When the division's type is a *types.TypeParam whose core type set contains int, int32 or int64, emit a generic golib overload, e.g. `quo<T>(T a, T b) where T : IBinaryInteger<T>, ISignedNumber<T> => b == T.NegativeOne ? unchecked(-a) : a / b` (and rem<T>). Or at least add generics to the documented limits.

#### (concern; skeptics refuting 0 of 2) The constant -1 remainder fold drops Go's nil-pointer panic. isSideEffectFreeOperand treats a selector through a pointer as effect-free, so `p.v % -1` with a nil p becomes `(nint)0`.

**Evidence.** probe5: `var p *S; r := p.v % -1` emits `nint r = (nint)0;`. Go prints 'recovered: true' because evaluating p.v panics. The C# build (out5) prints 'no panic, r = 0' and 'recovered: false'. The quotient fold keeps the evaluation (`unchecked(-p.v)`); only the remainder arm drops it. 0 corpus sites.

**Snippet.** case minusOne && isSideEffectFreeOperand(dividend):
    return fmt.Sprintf("(%s)0", ...), true
...
case *ast.SelectorExpr:
    return isSideEffectFreeOperand(e.X)   // p.v through a nil pointer can panic

**Suggested fix.** Fold only when the dividend is an Ident, a BasicLit, or a package-qualified identifier, or a selector chain with no pointer indirection (v.info.Selections[sel].Indirect() is false and no link is pointer-typed). Otherwise emit `rem(x, -1)`, which is still correct.

**Info:**
- The emitted form contradicts the docs' fold rule: a compound assignment with a constant -1 divisor is not folded. It emits the helper with a constant divisor, including in the seat's own golden.
- Go parentheses around an operand are kept inside the helper's argument list, where they are redundant. This happens at 28 of the 108 governed sites per target.
- A compound target behind a pointer is written with two different spellings in one statement.
- The shadow check that decides between bare `quo`/`rem` and `builtin.quo`/`builtin.rem` misses two cases: function-local constants or types named quo/rem, and package-level vars or consts declared only in a _test.go file. std has no such names today.
- The corpus hunks, the 3 moved goldens, golib's quo/rem and the NumericTypeTemplate change all check out, measured independently.

### Review: factories-semantics

#### (concern; skeptics refuting 0 of 2) reflect's hand-owned nil-map write still panics with a string, so the plainError family is split by route: `m[k]=v` recovers as runtime.plainError, but `reflect.Value.SetMapIndex` on a nil map recovers as `string`. The sizing census section 4 missed this site. The nil-channel fallback in reflect's Value.Close is the same shape.

**Evidence.** Measured with probea2, probea5 and the Go originals. Go gives `nilmap-reflect|T=runtime.plainError|...|rt=true err=true str=false as=true`. The seat gives `nilmap-reflect|T=string|...|rt=false err=false str=true as=false`. setmap-zero-reflect (reflect.Zero(map).SetMapIndex) and setmap-new-elem-reflect (reflect.New(map).Elem().SetMapIndex) diverge the same way. The text is identical, but %T, r.(runtime.Error), r.(error) and errors.As all differ. The reflect Close and Send doors already match Go, because a boxed nil channel<T> is still an IChannel and reaches golib's moved Close. The `throw panic("close of nil channel")` fallback at value_impl.cs:1703 was not reached by any probe, but it raises a string too.

**Snippet.** src/core/reflect/value_impl.cs:1787
    if (nilMap) {
        throw panic("assignment to entry in nil map");
    }
src/core/reflect/value_impl.cs:1703
    throw panic("close of nil channel");

**Suggested fix.** In both hand-owned reflect sites, raise the same value golib now raises: `throw RuntimeErrorPanic.PlainError("assignment to entry in nil map");` and `throw RuntimeErrorPanic.PlainError("close of nil channel");` (go.golib is already referenced by reflect). Add a GolibTests or behavioral arm for reflect SetMapIndex on a nil map. This is a small hand-own edit with no converter change.

#### (concern; skeptics refuting 0 of 2) makechan with a too-large size still escapes recover() as a raw .NET exception and ends the process. This is the same failure class the seat fixes for negative sizes. Sizes that Go accepts crash the same way. The census section 6 says a size above int.MaxValue 'truncates'. That is wrong: it throws.

**Evidence.** probea2, run with each arg separately. `n := 1 << 62; make(chan int, n)`: Go recovers `runtime.plainError` with text `makechan: size out of range`. The seat prints `System.OverflowException: Arithmetic operation resulted in an overflow. at go.ChanCore`1..ctor(IntPtr size) channel.cs:line 445 at go.channel`1..ctor(IntPtr size) channel.cs:line 1221` and exits 2, and the probe's deferred recover never runs. `make(chan struct{}, 1<<31)` and `make(chan struct{}, 1<<40)` are VALID in Go (it prints `made cap 2147483648` and `made cap 1099511627776`), and both throw the same OverflowException here. The mechanism: base(size) narrows Dataqsiz = (int)size, then AllocationCounter.NewArray<T>(size) throws. TryAsPanic does not map OverflowException. The negative cases (int, int64 -1, uint64 1<<63, chan chan) all match Go.

**Snippet.** golib/channel.cs (seat)
        if (size < 0)
            throw RuntimeErrorPanic.MakeChanSizeOutOfRange();
        m_core = new ChanCore<T>(size);   // ChanCore: m_buf = size > 0 ? AllocationCounter.NewArray<T>(size) : null;

**Suggested fix.** Make the three constructors apply Go's full makechan predicate: panic MakeChanSizeOutOfRange() when size < 0, or when elemSize*size overflows or exceeds maxAlloc-hchanSize (1<<48 minus the header on 64-bit). For sizes Go accepts but the CLR cannot allocate (above Array.MaxLength, including zero-size elements), either use a count-only buffer for zero-size T or name it as a disclosed platform bound. It must never be an escaping OverflowException. At minimum, correct the seat comment and the census section 6 wording ('truncates' should read 'throws OverflowException, escapes recover()').

#### (concern; skeptics refuting 0 of 2) The seat's text claim is wrong for unsigned indices. The commit and panicvalues_impl.cs say 'The panic TEXT is unchanged for every non-negative index', but a huge unsigned index (a non-negative Go index) now prints a different wrong text that drops the length Go prints. %#v also shows signed:true where Go has signed:false for every unsigned index.

**Evidence.** probea2. `u := ^uint(0); _ = s[u]` (len 2): Go prints `runtime error: index out of range [18446744073709551615] with length 2`. The seat prints `runtime error: index out of range [-1]`. The base printed `... [-1] with length 2`, derived from the unchanged fallback format. `var u uint64 = 1<<63`: Go prints `[9223372036854775808] with length 2`, the seat prints `[-9223372036854775808]`. Small unsigned indices keep the right text, but `index-uint|#v=runtime.boundsError{x:5, y:2, signed:true, ...}` differs from Go's `signed:false`. The cause is the emission `_ = s[(nint)(u)];`, which casts before golib sees the index, and the `this[ulong] => ref this[(nint)index]` overloads, which discard signedness anyway. So the new boundsNeg format fires. No golden quotes these texts (git grep: none).

**Snippet.** emitted: _ = s[(nint)(u)];
golib/slice.cs:572  public ref T this[ulong index] => ref this[(nint)index];
RuntimeErrorPanic.IndexOutOfRange: BoundsErrorValue?.Invoke(index, length, true, BoundsIndex)

**Suggested fix.** Either (a) amend the commit/comment claim to 'unchanged for every signed non-negative index; an unsigned index >= 2^63 moves from [-N] with length L to [-N]' and bank the named follow-up the census section 6 already lists, or (b) do the durable fix now. For (b), the converter stops casting unsigned index expressions to nint. The ulong overloads bounds-check `index >= (ulong)m_length`, and IndexOutOfRange gets a `bool signed` parameter so they can pass signed:false, which the hook already accepts.

#### (concern; skeptics refuting 0 of 2) Registration bound, measured. When a program's import closure never reaches runtime, the hooks are never registered, because runtime.dll is not even in the output. All six classes then recover as strings, and a negative index prints a different text from Go, both when recovered and in the uncaught `panic:` line. When the closure does reach runtime, registration comes before user code.

**Evidence.** probeb (no imports): the output folder holds only golib.dll and probeb.dll. The C# run prints `nilmap string: ...`, `closenil string: ...`, `index string: ...`, `negindex string: runtime error: index out of range [-1] with length 2`, `makechan string: ...`, `divide string: ...`, then uncaught `panic: runtime error: index out of range [-1] with length 2`. Go prints `error:` for every case and `panic: runtime error: index out of range [-1]`. probee is the same program plus `import "errors"`, and it matches Go exactly, including the uncaught line. probef imports fmt and also matches. The chain is: every package_info forces its imports as [GoInit] module initializers, and errors -> internal/reflectlite -> runtime (also os, sync). So any program importing errors, strings, io, strconv, sort, fmt, os or sync registers before main's own inits. Only closures made entirely of leaf packages (math, math/bits, unicode, unicode/utf8, cmp, unsafe) stay unregistered. By reasoning, not measured: a user package with no runtime-reaching imports whose init recovers a runtime panic, and which main's import hooks order before its first runtime-reaching import, would also see the fallback. The divide hook shares the same bound (divide also printed `string:` in probeb).

**Snippet.** RuntimeErrorPanic.IndexOutOfRange fallback:
    ?? string.Format(IndexOutOfRangeMessage, index, length)   // "...index out of range [{0}] with length {1}" even for index < 0

**Suggested fix.** Make the text independent of registration. When index < 0, the fallback formats Go's boundsNeg shape, `runtime error: index out of range [-1]` without the length (a one-line change in IndexOutOfRange). Record the type-only bound (the value is a string, not a runtime.Error, when nothing links runtime) in the RuntimeErrorPanic remarks, next to the divide hook's identical note. Optionally add a GolibTests arm for the unregistered negative-index text, since UnregisteredHooksFallBackToThePlainMessage covers only index 2.

#### (concern; skeptics refuting 0 of 2) PRE-EXISTING and not caused by this seat, but found by the slice probes the lens asked for: whole classes of Go slice-bounds panics are not Go panics at all. Negative slice bounds, and string slicing past the end, throw CLR exceptions that escape recover() and end the process.

**Evidence.** probea3 and probea5, each case run separately. `s := make([]int,3); j := -1; _ = s[:j]` gives `System.ArgumentOutOfRangeException: Non-negative number required. (Parameter 'value') at System.Index.ThrowValueArgumentOutOfRange_NeedNonNegNumException()`. The same happens for `s[i:]` with i=-1, for string `s[:j]` with j=-1 and for array `a[:j]` with j=-1. Go recovers a runtime.boundsError for each (`slice bounds out of range [:-1]` / `[-1:]`). `s := "abc"; _ = s[:5]` and `_ = s[5:]` give `System.ArgumentException: Indices low and high represent a range outside bounds of the string. (Parameter 'range') at go.string.get_Item(Range range) string.cs:line 273`. Go gives `slice bounds out of range [:5] with length 3` and `[5:3]`. The emitted form is `_ = s[..(int)(j)];` and `_ = s[(int)(i)..];`: C# Index rejects negatives before golib runs, and the (int) cast also truncates 64-bit bounds.

**Snippet.** emitted: _ = s[..(int)(j)];   // System.Index(int) throws on j < 0 before slice<T> sees it
golib/string.cs:270-273  throw new ArgumentOutOfRangeException(nameof(range), ...) / ArgumentException

**Suggested fix.** This is not a gate on this seat. Open a follow-up (the census's 'SliceBoundsOutOfRange hook needs a code mapping' seat is the natural owner). Route slice expressions through a golib slicing entry that takes the raw (low, high, max) as nint instead of System.Range, check Go's order (max vs cap, high vs max, low vs high, including negatives), and raise RuntimeErrorPanic.SliceBoundsOutOfRange for string, array and slice alike. Alternatively, map the CLR exceptions in TryAsPanic as a stopgap.

**Info:**
- The boundsError family is now mixed. Index panics recover as runtime.boundsError (runtime.Error, error), but slice-expression and slice-to-array-conversion panics still recover as `string`. The census names this as a sibling. The text agrees for most shapes; two shapes print differently from Go, both pre-existing.
- The other siblings the census lists still recover as strings, as the seat intends. These probes also found an unrelated reflect text bug.
- Positive verification: every door the seat moves matches Go byte for byte on every observable I probed. That covers type, text, the fmt verbs, errors.As and recover, as well as the uncaught panic line. The seat's GolibTests pass on the CLR.

### Review: factories-coverage

#### (concern; skeptics refuting 0 of 2) Raw .NET exceptions that recover() cannot see still reach Go bounds panics. The sizing did not list them, and one of them sits in the exact class this seat converts (index out of range via ж.at).

**Evidence.** Converted probe, the same at the seat and the parent (run-st.txt, run-pt.txt). In Go, every case below recovers a boundsError:
- c30 `s[1:n]` on a string, n > len, emitted as `s[1..(int)(n)]`: ESCAPED as System.ArgumentException from @string's Range indexer (golib/string.cs:270/273).
- c31/c32 `s[lo:]` with lo=-1 on a string or slice, emitted as `s[(int)(lo)..]`: ESCAPED as ArgumentOutOfRangeException ('Non-negative number required', raised by the C# Index conversion at the call site, before golib runs).
- c33 `"abc"[i]`, emitted as `"abc"u8[(int)(i)]`: ESCAPED as the CLR's IndexOutOfRangeException.
- c34 `&a[i]` through a *[3]int, emitted as `a.at<nint>(i)`: ESCAPED as IndexOutOfRangeException from golib/ж.cs:416.
RuntimeErrorPanic.TryAsPanic maps only PanicException, DivideByZeroException and NullReferenceException.

**Snippet.** ж.cs:415-416
        if (!array.IndexIsValid(index))
            throw new IndexOutOfRangeException("Index is out of range for array or slice.");

**Suggested fix.** Not a blocker for this seat. Queue them as the next sibling seat:
(1) ж.at: `throw RuntimeErrorPanic.IndexOutOfRange(index, <length>)`, a one-line change in golib that picks up boundsError automatically.
(2) @string's this[Range]: raise RuntimeErrorPanic.SliceBoundsOutOfRange instead of ArgumentException or ArgumentOutOfRangeException.
(3) A negative 2-index bound and string-literal indexing are converter emission shapes: route the bound through an nint golib path instead of the C# Index conversion, and route the literal index through a checked accessor.
Do NOT broaden TryAsPanic to ArgumentException or IndexOutOfRangeException globally.

#### (concern; skeptics refuting 0 of 2) A negative shift count does not panic at all. Go raises runtime.Error "negative shift amount" (runtime.shiftError), and this sibling is missing from the sizing's list.

**Evidence.** Probe c29, `n := -1; sink = 1 << n`. Go: 'runtime.Error: true | runtime error: negative shift amount'. go2cs, at both the seat and the parent: '<no panic>'. The emission is `((nint)1).Lsh((uint64)(n))`, and golib GoShift.cs:62 `Lsh(this nint x, uint64 n) => n >= 64 ? 0 : x << (int)n`. The sign is discarded at the emitted cast, so the program silently computes 0.

**Snippet.** sink = ((nint)1).Lsh((uint64)(n));   // Go: panic(shiftError)

**Suggested fix.** Separate from this seat. For a signed shift count, the converter or the golib Lsh/Rsh overloads need a signed path that panics with a shiftError hook of the same inverted shape (the runtime already has shiftError = error(errorString("negative shift amount"))). Add it to the named-sibling list beside the errorString family.

**Info:**
- The 17 red arms are genuinely red at the parent, but the fix commit's tally is wrong: 13 recovered System.String, not 14.
- Mutation shows every throw site has its own red arm. The file header's "one arm per THROW SITE" is slightly off: two arms are extra doors onto a site that is already armed.
- Census of the remaining siblings that still recover as a string, measured. None has a stdlib or banked-row consumer that depends on its type.
- Consumers checked. No banked-row verdict is predicted to move. The only behaviour changes are toward Go, and the panic text is unchanged except for the negative index.
- The registration-timing probe the sizing asked for reads green, but the seat did not record it: a plain program that imports runtime gets the hook values before main.
- Two small panic-text divergences on paths that were already wrong: an unsigned index at or above 2^63 now drops its length, and the unregistered fallback still prints the old form for a negative index.
