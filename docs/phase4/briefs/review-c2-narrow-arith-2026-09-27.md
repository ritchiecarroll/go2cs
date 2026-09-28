# Review: C2's narrow-arithmetic pre-pass at ff71ab6a9e (2026-09-27)

COORD ran an adversarial review of claude/bold-thompson-e4gztv at ff71ab6a9e (the converter is unchanged at 1caee7e1da, which adds only corpus hunks). Four finders read the diff, each through a different lens: consumer completeness, emission correctness, Go semantics of the classification, and test arms and docs against the claims. Each built the branch converter and the base converter (1aebd6a885) in its own scratch root and converted probe packages, and every Go value quoted was taken from `go run`. Two skeptics then tried to refute each blocker and concern, one by reproducing it and one by reasoning about the emitted C#. 24 findings survived; none was refuted.

**VERDICT: NOT ACCEPTED as it stands.** Five blockers and four other items, deduplicated below. The full surviving findings, with snippets, emissions and suggested fixes, are in the appendix.

| # | Defect | Findings | Severity |
|---|---|---|---|
| D1 | INTRODUCED by this change. convParenExpr's paren elision (narrowArithmeticParenSelfCast) drops the parens around a parenthesized narrow operand. emitGuardedShift trusts needsParentheses(ParenExpr) == false, so a variable-count `(a+b) >> n` emits `(int8)((int8)(a + b).Rsh(n))`. The cast binds to the int32 Rsh RESULT. This is silent and Go-divergent: a typed destination reads 100 against Go's -28, an interface uint8 200 against 72, a comparison false against true. `(h/m)>>n` and `(-(a+b))>>n` are affected too. With the elision off, the branch is correct. | S01 S06 S13 S18 | blocker |
| D2 | Unary `+` and signed unary `^` are classified as wrap-invariant consumers and wrapRisk=false. An overflowing operand (`^(a+b)`, `+(a*b)`) therefore reaches a value consumer (a widening conversion, an index, an interface) with no cast. | S02 S09 S14 S19 | blocker |
| D3 | `<<` is excluded from producers on the premise that its emission already narrows the whole result. That holds on the constant-count path only; the guarded variable-count path emits `(u + w).Lsh(s)`, which binds int32 Lsh and returns int32 unnarrowed. | S03 S07 S15 S20 | blocker |
| D4 | CaseClause values (and the tag) are always narrowConsumerValue. When the switch tag is an interface, Go compares the dynamic type, so a type-only producer (`%`, unsigned `/`, `>>`, unary `+`, signed `^`) boxes as int32 and never matches. | S04 S08 S21 | blocker |
| D5 | A map index through a type parameter with a map core (`M ~map[int8]string`) is classified Value, because TypeOf(X).Underlying() is the constraint interface. A type-only key is not cast, which is a compile error against IMap<int8,...>. | S05 S11 S17 | blocker |
| D6 | byte vs uint8 spelling: narrowDestinationType uses types.Unalias(TypeOf(Fun)).(*types.Signature), which is nil for a named func type such as `type F func(byte)`; so the "typed destinations do not change" and "compound double-cast fixed" claims fail in two shapes. | S10 | concern |
| D7 | PRE-EXISTING and NOT this seat's to fix: a unary minus or plus over a unary operand is emitted with no separator (convUnaryExpr.go, `return unaryExpr.Op.String() + v.convExpr(...)`), so `- -a` becomes `--a` and `+ +a` becomes `++a`. That is a C# pre-decrement or pre-increment that MUTATES the variable. The new hook now wraps it. It becomes its own seat. | S12 | concern (separate seat) |
| D8 | Unneeded self-casts on a guarded `>>` whose receiver is already narrow (GoShift's Rsh returns the narrow type): the NarrowShiftVarCount golden gains `(byte)(cb.Rsh(k))` and `(int8)(s8.Rsh(k))`. The behaviour is right, but it is against the reads-like-Go doctrine. | S16 S23 | concern |
| D9 | Coverage: the arm labelled "case value" is a tagless switch, which exercises a comparison, not CaseClause. Several classes the code changes have no arm: a map-literal KEY, a `make` length or capacity, a 3-index slice max, a divisor, and others (S24). There is also no variable-count arm for `>>` or `<<` (D1 and D3 would have been red). | S22 S24 | concern |

**Asks for the re-cut:** fix D1-D5 red first, each with a runtime or compile arm that is red at ff71ab6a9e; fix D6 and D8; add D9's missing arms; re-run the two-seeded 3-target footprint and CNR, scored against a restated prediction. D7 is queued separately.

## Appendix: the surviving findings

### S01 (blocker; lens: CONSUMER COMPLETENESS; skeptics refuting: 0 of 2)

**Claim.** The change introduces a mis-bound cast. The new convParenExpr elision (narrowArithmeticParenSelfCast) drops the parentheses around a parenthesized narrow operand of a variable-count shift. The guarded-shift emitter (emitGuardedShift) decides whether to parenthesize its receiver from the AST: needsParentheses(ParenExpr) is false, so it trusts the operand to arrive already parenthesized. It now gets `(int8)(a + b)` and appends `.Rsh(n)`. In C# a member access binds tighter than a cast, so the cast applies to the int32 Rsh RESULT and the operand stays unwrapped. The claimed 'shr operand' class is therefore still wrong for every variable count, in typed, interface and comparison consumers alike. The comment on narrowArithmeticParenSelfCast says 'a narrow integer is never the target of a member access, index or call', which is false for this emitter. The unparenthesized forms are fine: `-m8 >> n` emits `((int8)(-m8)).Rsh(n)`. The runtime arms only cover constant counts, e.g. `(a+a)>>1`.

**Go:**

```go
var a, b int8 = 100, 100
var u, v uint8 = 200, 200
var n uint = 1
func shrVarTyped() int8 { var x int8 = (a + b) >> n; return x } // Go -28
func shrVarAny() any     { return (u + v) >> n }               // Go 72 (uint8)
func shrVarCmp() bool    { return (a+b)>>n == -28 }            // Go true
```

**Emitted:**

```csharp
int8 x = (int8)((int8)(a + b).Rsh(n));        // = (int8)((a+b).Rsh(n)): int32 200>>1 = 100
return (uint8)((uint8)(u + v).Rsh(n));         // 400>>1 = 200
return (int8)((int8)(a + b).Rsh(n)) == -28;    // 100 == -28 -> false
// base emitted (int8)((a + b).Rsh(n)): the same wrong value, without the misleading inner cast
```

**Evidence.** Probe p4, branch vs base diff. The logic is emitGuardedShift in convBinaryExpr.go:477 (`needsParentheses(binaryExpr.X)`) and needsParentheses at convCallExpr.go:5454 (ParenExpr -> false). The C# grammar is cast_expression := '(' type ')' unary_expression, and `(a + b).Rsh(n)` is a single primary expression. Also affected: `(h / m) >> n` for signed `/` (MinInt8 / -1) and `(-(a+b)) >> n`.

**Suggested fix.** Remove the convParenExpr elision, or have emitGuardedShift parenthesize its receiver based on the RENDERED text (parenthesize unless isFullyParenthesized(leftOperand) or it is a bare identifier) rather than on needsParentheses(ast). Add runtime arms for `(a+a) >> k` and `(u+u) >> k` with a variable k to NarrowArithmeticSinks.

### S02 (blocker; lens: CONSUMER COMPLETENESS; skeptics refuting: 0 of 2)

**Claim.** Signed unary `^` and unary `+` are classified as wrap-invariant consumers (narrowConsumerOf's UnaryExpr case treats - ^ + alike). Unlike `-`, neither is a narrowing producer: both are wrapRisk=false, so they are cast only at typed consumers. An unwrapped operand therefore leaks through them into every value consumer (conversion, index, / % >> operand, switch). The model's claim that signed ^ and unary + give results that are 'merely int-typed' holds only when their operand is in range, and an invariant child is by definition not in range. Unsigned ^ is correct, because convUnaryExprCore truncates it.

**Go:**

```go
var a, b int8 = 100, 100
var u, v uint8 = 200, 200
var s = make([]int, 300)
func xorVal() int    { return int(^(a + b)) }   // Go 55
func plusVal() int   { return int(+(a + b)) }   // Go -56
func xorDiv() int8   { return (^(a + b)) / 2 }  // Go 27
func xorShr() int8   { return (^(a + b)) >> 1 } // Go 27
func plusIdx() int   { return s[+(u + v)] }     // Go s[144] = 0
func plusShr() uint8 { return (+(u + v)) >> 1 } // Go 72
```

**Emitted:**

```csharp
return (nint)(~(a + b));                       // -201
return (nint)(+(a + b));                       // 200
return (int8)((~(a + b)) / 2);                 // -201/2 = -100 -> -100
return (int8)(((~(a + b)) >> (int)(1)));       // -101
return s[+(u + v)];                            // index 400 -> panic (len 300)
return (uint8)(((+(u + v)) >> (int)(1)));      // 200
```

**Evidence.** Probe p1; the branch emission is identical to base, so this predates the change. The case is left unhandled in the nested-operation class that the pre-pass models. Typed consumers are correct: `^(a+b) == 55` emits `(int8)(~(a + b)) == 55`.

**Suggested fix.** Propagate wrap risk. When the operand (after unparen) of a signed unary `^` or of a unary `+` is a wrapRisk producer left unmarked because it sat under an invariant consumer, set wrapRisk=true on the ^/+ producer. Typed text then stays `(int8)(~(a + b))`, and value consumers become `(nint)(int8)(~(a + b))`. The alternative is to return narrowConsumerValue for the operand of a signed `^` or a `+`, so the operand itself is cast.

### S03 (blocker; lens: CONSUMER COMPLETENESS; skeptics refuting: 0 of 2)

**Claim.** The pre-pass assumes '`<<` emission already narrows the whole result', and on that basis treats the left operand of `<<` as invariant and leaves SHL out of the producers. That holds only on the constant-count path, which emits `(int8)(x << (int)k)`. On the guarded variable-count path, emitGuardedShift renders `(a + b).Lsh(n)`. The receiver is C# int32, so it binds GoShift's `int32 Lsh(this int32, uint64)` overload, which neither narrows nor uses the 8-bit width guard. Nothing then narrows the result for value or interface consumers. A plain narrow receiver is fine: `h << n` binds the int8 overload.

**Go:**

```go
var a, b int8 = 100, 100
var n uint = 1
func shlVarVal() int  { return int((a + b) << n) }   // Go -112
func shlVarAdd() any  { return (a + b) << n }        // Go -112 (int8)
func shlVarDiv() int8 { return ((a + b) << n) / 3 }  // Go -37
```

**Emitted:**

```csharp
return (nint)((a + b).Lsh(n));          // int32 400
return (a + b).Lsh(n);                  // boxes int32 400; %T int32
return (int8)(((a + b).Lsh(n)) / 3);    // 400/3 = 133 -> (int8) -123
```

**Evidence.** Probe p3; identical in base. GoShift.cs:84 is `int32 Lsh(this int32 x, uint64 n) => n >= 32 ? 0 : x << (int)n`. Typed destinations are correct: `(int8)((a + b).Lsh(n))` gives -112 from the low bits. A non-wrap-risk left operand fails the same way, e.g. `int((h>>1) << n)` with n=2 gives Go -56 against C# 200, as does a constant count at or above the width that also takes the guarded path.

**Suggested fix.** When the shift takes the guarded path (a count not provably within the width), classify the left operand of `<<` as narrowConsumerTyped. The receiver becomes `((int8)(a + b)).Lsh(n)`, which binds the narrow overload, and that overload narrows the result itself. Alternatively, treat a guarded-path SHL as a producer.

### S04 (blocker; lens: CONSUMER COMPLETENESS; skeptics refuting: 0 of 2)

**Claim.** Switch case values are always classified narrowConsumerValue. When the switch tag is an INTERFACE, Go converts each case value to that interface and compares dynamic types, so the case is a typed consumer. A type-only producer such as `>>`, `%` or unsigned `/` is left int32, gets boxed as int32, and never matches. The direct comparison `x == h>>1` is correct: it is classified Typed and cast.

**Go:**

```go
var h int8 = 101
var u uint8 = 200
func caseIface() string {
	var x any = int8(50)
	switch x {
	case h >> 1:
		return "match"
	}
	return "nomatch"
} // Go: match
// likewise: var y any = uint8(66); switch y { case u / 3: } -> Go: match
```

**Emitted:**

```csharp
any x = (int8)50;
var exprᴛ1 = x;
if (AreEqual(exprᴛ1, (h >> (int)(1)))) {   // boxed int32 50 vs sbyte 50 -> false: "nomatch"
...
if (AreEqual(exprᴛ1, u / 3)) {             // int32 vs byte -> false
// contrast: x == h>>1  ->  AreEqual(x, (int8)((h >> (int)(1))))
```

**Evidence.** Probe p3; identical in base. In golib builtin.cs:3355, AreEqual(object, object) returns false when `leftType != right.GetType()`. The CaseClause/SwitchStmt branch of narrowConsumerOf ignores the tag's type.

**Suggested fix.** In narrowConsumerOf, for a CaseClause, find the enclosing SwitchStmt (stack[i-2], through the BlockStmt). If its Tag's type has an interface underlying type, return narrowConsumerTyped. Otherwise keep Value. Add a runtime arm.

### S05 (blocker; lens: CONSUMER COMPLETENESS; skeptics refuting: 0 of 2)

**Claim.** A map key through a generic map type parameter compiles to an error. The IndexExpr branch tests `TypeOf(parent.X).Underlying().(*types.Map)`. For M ~map[int8]string the Underlying() is the constraint interface, so the key is classified Value and a type-only producer is not cast. The map-key compile sink therefore still fails whenever the map comes through a type parameter. A wrap-risk key (`m[a+b]`) happens to be cast anyway, through the value class.

**Go:**

```go
var h int8 = 101
func gmap[M ~map[int8]string](m M) string { return m[h>>1] }
// Go: gmap(map[int8]string{50: "ok"}) == "ok"
```

**Emitted:**

```csharp
internal static @string gmap<M>(M m)
    where M : /* ~map[int8]string */ IMap<int8, @string>, ISupportMake<M>, new()
{
    return m[(h >> (int)(1))];   // IDictionary<int8,@string> indexer, int argument -> CS1503
}
```

**Evidence.** Probe p2; identical in base. IMap<TKey,TValue> : IDictionary<TKey,TValue> (golib map.cs:63), and there is no implicit int -> sbyte conversion for a non-constant. The concrete-map case is correct: `m[(uint8)((u >> (int)(1)))]++`. An assignment `m[h>>1] = x` through a type parameter fails the same way.

**Suggested fix.** In the IndexExpr branch, when TypeOf(parent.X) is a *types.TypeParam, resolve its core type with the existing typeParamMapCore (convSliceExpr.go:512) before the map test. Make the same change in narrowDestinationType's IndexExpr branch so the cast takes the key's spelling.

### S06 (blocker; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** B1. The convParenExpr paren-drop breaks the guarded-shift receiver. For `(X) >> s` where X is a `+ - *`, unary `-` or signed `/` narrow expression and the count is not provably below the width, the new self-cast attaches to the `.Rsh(...)` result instead of X. The C# receiver is still the unwrapped int, so the shape the change tries to fix still gives Go-divergent results. The unparenthesized twin `a*b>>s` (same AST shape without the ParenExpr) comes out right, so the two Go spellings disagree.

**Go:**

```go
var a, b int8 = -100, 100
var s uint = 1
fmt.Println(a*b>>s, (a*b)>>s)   // Go: -8 -8
fmt.Println((a - b) >> 8)       // Go: 0
var u uint8 = 200
fmt.Println((-u) >> s)          // Go: 28
```

**Emitted:**

```csharp
fmt.Println((int8)(((int8)(a * b)).Rsh(s)), (int8)((int8)(a * b).Rsh(s)));   // C#: -8 120
fmt.Println((int8)((int8)(a - b).Rsh((uint64)(8))));                             // C#: -1
fmt.Println((uint8)((uint8)(((uint8)0 - u)).Rsh(s)));                            // C#: 156
(base emitted (a * b).Rsh(s) etc.; the values were already wrong there)
```

**Evidence.** C# reads `(int8)(a * b).Rsh(s)` as `(int8)((a * b).Rsh(s))` because member access and invocation bind tighter than a cast. So int32.Rsh(-10000, 1) = -5000, then (int8) gives 120. How it happens: convParenExpr.go drops the parens through narrowArithmeticParenSelfCast (narrowArithmeticOperations.go:436). Its doc says 'a narrow integer is never the target of a member access, index or call'. But emitGuardedShift (convBinaryExpr.go:477) turns the left operand into the receiver of `.Rsh/.Lsh`, and needsParentheses(*ast.ParenExpr) returns false (convCallExpr.go:5454), so the receiver goes out bare. Probes p1, p2, p4, p7 and p8. C2's 'shr operand' arm only uses constant counts ((a+a)>>1, native path), so its test never reaches the guarded route. A grep of the current corpus finds no `(x op y).Rsh(` receiver on a narrow type, so reach today is about zero; the defect is in the emission rule.

**Suggested fix.** In emitGuardedShift, parenthesize the receiver whenever X is a *ast.ParenExpr whose rendering is not one fully parenthesized group. For example, add `|| (isParen && !isFullyParenthesized(leftOperand))`, or check `v.narrowArithmeticParenSelfCast(paren, leftOperand)`. The result is `((int8)(a * b)).Rsh(s)`. Alternatively, drop the paren-drop arm entirely and accept `((int8)(a + b)) / 2`. Add a guarded-count arm to NarrowArithmeticSinks, for example `(a*b)>>s` with a variable s.

### S07 (blocker; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** B2. The pre-pass skips `<<` on the premise that its emission 'already narrows the whole result'. That holds on the native path but not on the guarded path when the receiver renders as int: a parenthesized or same-precedence arithmetic operand (`(u+w) << s`, `u*w << s`), a signed unary `^`, or `(u>>1)`. There `X.Lsh(s)` binds GoShift's int32 overload and returns an unnarrowed int32. Every non-invariant consumer (interface argument, index, widening conversion, `/ % >>` operand, shift count) then sees the wrong value. Typed destinations and comparisons are safe only because the older arms cast the whole shift.

**Go:**

```go
var u, w uint8 = 200, 100
var a int8 = -100
var s uint = 1
arr := make([]int, 1000)
fmt.Println((u + w) << s, u*w<<s, (^a) << s)  // Go: 88 64 -58
fmt.Println(arr[(u+w)<<s], int((u+w)<<s), (u+w)<<8) // Go index 88, 88, 0
```

**Emitted:**

```csharp
fmt.Println((u + w).Lsh(s), (u * w).Lsh(s), (~a).Lsh(s));   // C#: 600 40000 198
fmt.Println(arr[(u + w).Lsh(s)]);                          // index 600
fmt.Println((nint)((u + w).Lsh(s)));                       // 600
fmt.Println((u + w).Lsh((uint64)(8)));                     // 76800
```

**Evidence.** Probes p4 and p7 give identical output on base and branch. GoShift.cs defines `int32 Lsh(this int32 x, uint64 n)`; only a receiver that is actually typed uint8/int8 picks the narrow overload. The native-path narrow arm (convBinaryExpr.go:1635) sits after the guarded early return (convBinaryExpr.go:1565), so it never runs here. narrowArithmeticProducer rejects SHL, and narrowConsumerOf (narrowArithmeticOperations.go:182) classes the `<<` left operand as invariant, so nothing ever narrows this shape.

**Suggested fix.** In narrowConsumerOf's SHL case, when child == parent.X and the shift takes the guard (v.shiftCountGuarded(parent.Y, width) && !v.shiftLeftRendersAsUntypedWrapper(parent.X)), return narrowConsumerTyped: the receiver's C# TYPE picks the width-matched overload. Also make the guarded `<<` itself count as narrowing only when its receiver renders narrow. This depends on B1's receiver-paren fix so the result reads `((uint8)(u + w)).Lsh(s)`.

### S08 (blocker; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** B3. A switch case value (and a switch tag) is always classed narrowConsumerValue, even when the tag or the case is an interface. There Go compares dynamic type plus value, and the C# AreEqual compares boxed runtime types. So a case value with a type-only difference (`%`, unsigned `/`, `>>`, signed unary `^`, unary `+`) boxes as Int32 and never matches the tag's boxed int8/uint8. The same comparison written as `ia == a%51` is classed typed (BinaryExpr) and emits correctly, so switch and == disagree. The change's own doc lists 'an interface boxes the C# type' as the reason for the typed class.

**Go:**

```go
var a uint8 = 50
var ia any = uint8(50)
switch ia {
case a % 51:            // Go: matches, prints hit
	fmt.Println("hit")
}
var b int8 = 100
var ib any = int8(50)
switch ib { case b >> 1, +b, ^b: }
```

**Emitted:**

```csharp
var exprᴛ1 = ia;
if (AreEqual(exprᴛ1, a % 51)) {   // Int32 50 vs Byte 50 -> false, nothing printed
...
if (AreEqual(exprᴛ1, (a >> (int)(1)))) / AreEqual(exprᴛ1, +a) || AreEqual(exprᴛ1, ~a)
```

**Evidence.** Probes p1, p3 and p8. The AreEqual type-mismatch bail is documented at convBinaryExpr.go:1536-1539 ('an early leftType != right.GetType() bail'). The classification is at narrowArithmeticOperations.go:216. Base emits the same text, so this predates the change, but it sits in the typed class the change claims to cover. Related and also at base: `switch a >> 1 { case ia: }` emits `exprᴛ2 == ia` (int vs object), which should route through AreEqual.

**Suggested fix.** For a CaseClause, reach the enclosing SwitchStmt (stack[i-2]). Return narrowConsumerTyped when the tag's type is an interface, or when the tag is absent but some case expression in that clause list has interface type. Otherwise keep narrowConsumerValue. Apply the mirror rule to the SwitchStmt tag when any case value is an interface.

### S09 (blocker; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** B4. Unary `+` and signed unary `^` are marked wrapRisk=false ('only the promoted TYPE differs'), but their operand is classed narrowConsumerInvariant (narrowArithmeticOperations.go:197), so it is left unwrapped. `^(a+b)` and `+(a*b)` therefore carry out-of-range values into value consumers that get no cast: an index, a widening conversion, a `/ % >>` operand, a shift count. `/` and `%` do not preserve low bits, so even a typed destination gets the wrong value.

**Go:**

```go
var a, b int8 = 100, 100
var u, w uint8 = 200, 100
arr := make([]int, 1000)
fmt.Println(int(^(a + b)))           // Go: 55
var q int8 = ^(a + b) / 2            // Go: 27
fmt.Println(^(a+b)>>1, ^(a+b)%3)     // Go: 27 1
fmt.Println(arr[+(u+w)], int(+(a*b))) // Go: index 44, 16
var r int8 = +(a + b) / 3            // Go: -18
```

**Emitted:**

```csharp
fmt.Println((nint)(~(a + b)));                                   // C#: -201
int8 q = (int8)(~(a + b) / 2);                                     // C#: -100
fmt.Println(q, (int8)((~(a + b) >> (int)(1))), (int8)(~(a + b) % 3)); // -101 0
fmt.Println(arr[+(u + w)], (nint)(+(a * b)));                     // index 300, 10000
int8 r = (int8)(+(a + b) / 3);                                     // C#: 66
```

**Evidence.** Probe p9 on both converters: base gives identical wrong values and the branch only adds outer casts. The comment in convUnaryExprCore ('C#'s ~ of a sign-extended operand equals the sign-extended Go result') holds only for an in-range operand, and the invariant class for unary `^`/`+` operands breaks that premise. Typed comparison consumers are fine (narrowComparisonOperand casts the whole unary).

**Suggested fix.** In narrowArithmeticProducer, set wrapRisk=true for unary ADD and for signed unary XOR when ast.Unparen(e.X) is itself a wrap-risk narrow producer (or simply always for unary `+`, which is vanishingly rare). Equivalently, class a unary `^`/`+` parent's operand as narrowConsumerValue instead of invariant. Add `int(^(a+b))` and `^(a+b)/2` arms.

### S10 (concern; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** C1. The 'typed destinations do not change' and 'compound double-cast fixed' claims fail for byte-vs-uint8 spelling mismatches in two shapes. (a) A call through a NAMED func type: narrowDestinationType uses types.Unalias(TypeOf(Fun)).(*types.Signature), which is nil for `type F func(byte)`, so the self-cast keeps the operand spelling and the existing call-arg arm wraps it again. (b) A `/=` or `%=` divisor: a Value consumer never takes the destination spelling, and visitAssignStmt's compound arm then adds its LHS-spelled cast. Behaviour is correct; the text regresses from base.

**Go:**

```go
type F func(byte)
type Holder struct{ fn F }
var u, w uint8 = 200, 100
var f F = func(q byte) {}
f(u + w)
var h Holder; h.fn(u >> 1)
var x byte = 9
x /= u + w
x %= u * w
```

**Emitted:**

```csharp
f((byte)((uint8)(u + w)));                 // base: f((byte)(u + w));
h.fn((byte)((uint8)((u >> (int)(1)))));     // base: h.fn((byte)((u >> (int)(1))));
x /= (byte)((uint8)(u + w));               // base: x /= (byte)(u + w);
x %= (byte)((uint8)(u * w));               // base: x %= (byte)(u * w);
```

**Evidence.** Probes p1, p2 and p7, diffed against the base converter. With identical spelling (var x uint8; x /= u + w) a single `(uint8)(u + w)` is emitted, so only mixed byte/uint8 declarations are hit, which the stdlib does mix. See narrowArithmeticOperations.go:296 and the dest-spelling branch in markNarrowArithmeticContexts, which is gated to narrowConsumerTyped.

**Suggested fix.** Use `v.info.TypeOf(parent.Fun).Underlying().(*types.Signature)` (or coreType) in narrowDestinationType. In markNarrowArithmeticContexts, apply the destination spelling for narrowConsumerValue too whenever narrowDestinationType returns an identical type; the compound AssignStmt case already returns the LHS type.

### S11 (concern; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** C2. A map index through a type parameter with a map core (`M ~map[int8]string`) is classed narrowConsumerValue: TypeOf(X).Underlying() is the constraint interface, not *types.Map. A type-only key such as `a>>1` therefore gets no cast, and the int key does not bind IMap<int8,…>'s indexer. This is a compile error the change leaves in its map-key typed class. narrowDestinationType's IndexExpr case has the same blind spot.

**Go:**

```go
func mapGen[M ~map[int8]string](m M, a int8) string { return m[a>>1] }
```

**Emitted:**

```csharp
internal static @string mapGen<M>(M m, int8 a)
    where M : /* ~map[int8]string */ IMap<int8, @string>, ISupportMake<M>, new()
{
    return m[(a >> (int)(1))];   // int key -> CS1503 expected (no implicit int->sbyte)
}
```

**Evidence.** Probe p2; base and branch are identical. The classification is at narrowArithmeticOperations.go:210. The concrete map form `mi[a>>1]` is now fixed by the change (`mi[(int8)((a >> (int)(1)))]`). I did not compile this; I read the IMap indexer surface in golib map.cs.

**Suggested fix.** Resolve the container through its core type (typeParamMapCore / a coreType helper) in both narrowConsumerOf's IndexExpr case and narrowDestinationType.

### S12 (concern; lens: EMISSION CORRECTNESS; skeptics refuting: 0 of 2)

**Claim.** C3. A unary minus or plus over a unary operand is emitted with no separator, so C# reads it as a pre-decrement or pre-increment: `- -a` becomes `--a` and `+ +a` becomes `++a`, which mutates the variable. This predates the change (convUnaryExpr.go:1251 `return unaryExpr.Op.String() + v.convExpr(...)`). The new hook now wraps it as `(int8)(--a)`, which makes the defect look deliberate.

**Go:**

```go
var a int8 = 5
fmt.Println(- -a, + +a)   // Go: 5 5, a unchanged
```

**Emitted:**

```csharp
fmt.Println(..., (int8)(--a), (int8)(++a));   // C#: 4 5, and a is mutated
(base: fmt.Println(--a, ++a))
```

**Evidence.** Probes p1 and p5 on both converters. The inner `-a` is invariant-classed, so the change never inserts a cast that would separate the two tokens.

**Suggested fix.** Out of this change's scope. Emit a space (or parentheses) when the operand's rendering starts with the same `-` or `+` character. Worth a separate small cut.

### S13 (blocker; lens: GO SEMANTICS OF THE CLASSIFICATION; skeptics refuting: 0 of 2)

**Claim.** A guarded right shift (count not provably below the width) with a PARENTHESIZED operand puts the new narrowing cast in the wrong place. convParenExpr drops the parens because the inner expression rendered as a whole cast (narrowArithmeticParenSelfCast). emitGuardedShift does not re-parenthesize a ParenExpr, because needsParentheses(ParenExpr) is false. The output is `(int8)(a + b).Rsh(n)`, and C# parses that as `(int8)((a + b).Rsh(n))`. So the int32 overload of Rsh runs on the unwrapped sum. This is exactly the parenthesized-operand shape the ruling asked for.

**Go:**

```go
var a, b int8 = 100, 100
var u, w uint8 = 200, 200
var mn, neg1 int8 = -128, -1
var n uint = 1
fmt.Println((a + b) >> n)       // Go -28
fmt.Println((u + w) >> n)       // Go 72
fmt.Println((mn / neg1) >> n)   // Go -64
fmt.Println((a + b) >> 10)      // Go -1 (a constant count >= width is guarded too)
var k int8; k += (a + b) >> n   // Go -28
```

**Emitted:**

```csharp
fmt.Println((int8)((int8)(a + b).Rsh(n)));            // 200.Rsh(1)=100 -> 100
fmt.Println((uint8)((uint8)(u + w).Rsh(n)));          // 400.Rsh(1)=200 -> 200
fmt.Println((int8)((int8)(mn / neg1).Rsh(n)));        // 128.Rsh(1)=64 -> 64
fmt.Println((int8)((int8)(a + b).Rsh((uint64)(10)))); // 0
k += (int8)((int8)(a + b).Rsh(n));                    // k = 100
```

**Evidence.** In C#, a cast binds looser than member access and invocation. The converter already says so itself: shiftReceiverRendersAsCast exists because `(T)x.Lsh(…)` binds `.Lsh` to x. Here `(a + b)` has type int, so GoShift.cs:81 `int32 Rsh(this int32 x, uint64 n)` binds. The base 1aebd6a885 emits `(a + b).Rsh(n)`, which has the same wrong value. The branch adds a cast that makes it LOOK narrowed. The comment on narrowArithmeticParenSelfCast says "a narrow integer is never the target of a member access, index or call", and the guarded shift proves that false. The unparenthesized form is correct: `a*b>>n` emits `((int8)(a * b)).Rsh(n)`. The same failure appears with `(-mn) >> n`, `(s16+s16) >> n` and `int((u + w) >> n)`. The NarrowArithmeticSinks "shr operand" arm uses a constant count of 1, which takes the native path, so the test never exercises this. (probe p2, p3; diff2.txt)

**Suggested fix.** Make emitGuardedShift parenthesize the receiver when binaryExpr.X is a ParenExpr whose unparenthesized inner expression is in narrowArithmeticCasts, for example by widening shiftReceiverRendersAsCast. Alternatively, have narrowArithmeticParenSelfCast keep the parens when the ParenExpr is the X of a shift. Then add variable-count arms `(a+a)>>n` and `(u+u)>>n` to NarrowArithmeticSinks.

### S14 (blocker; lens: GO SEMANTICS OF THE CLASSIFICATION; skeptics refuting: 0 of 2)

**Claim.** Signed unary ^ and unary + are classified as "merely int-typed" (wrapRisk=false), but that only holds when their operand is in range. narrowConsumerOf marks a unary - ^ + parent as Invariant, so an overflowing operand such as a+b is deliberately left unwrapped. ~200 = -201 and +200 = 200 are out of range. They get narrowed only at Typed consumers. At Value consumers (widening or float conversion, index, switch tag, / % >> operands) the wrong value leaks. Once a non-low-bit-invariant operator (>> / %) sits above them, even the Typed cast produces a wrong value.

**Go:**

```go
var a, b int8 = 100, 100
var s, t int16 = 30000, 30000
var u, w uint8 = 200, 200
fmt.Println(int(^(a + b)))       // Go 55
fmt.Println(^(a+b)>>1 == 27)     // Go true
fmt.Println(int32(^(s + t)))     // Go 5535
fmt.Println((^(a * b)) % 5)      // Go -2
fmt.Println(int(+(a + b)))       // Go -56
fmt.Println((+(u + w)) / 3)      // Go 48
switch ^(a + b) { case 55: /* Go takes this */ }
```

**Emitted:**

```csharp
nint x = (nint)(~(a + b));                          // -201
fmt.Println((int8)((~(a + b) >> (int)(1))) == 27);  // -101 == 27 -> False
fmt.Println((int32)(~(s + t)));                     // -60001
fmt.Println((int8)((~(a * b)) % 5));                // -1
fmt.Println((nint)(+(a + b)));                      // 200
fmt.Println((uint8)((+(u + w)) / 3));               // 133
switch (~(a + b)) { case 55: ...                    // -201 -> default
```

**Evidence.** convUnaryExprCore's own comment states the premise: a SIGNED narrow ~ is value-correct only for an in-range, sign-extended operand. The Invariant rule for unary parents breaks that premise. Bitwise NOT and identity preserve only the low bits. That is enough when a narrowing cast follows directly, as in `fmt.Println(^(a+b))` → `(int8)(~(a + b))` = 55, which is correct. It is not enough under >>, /, % or a widening consumer. The base emits the same arithmetic. The branch adds casts such as `(int8)((~(a + b) >> 1))` and `(uint8)((+(u + w)) / 3)` that fix %T but not the value. The unsigned unary ^ is safe because it is always emitted as `((uint8)(~x))`. The same hole also affects `^(-mn)`, which should be 127 and emits `(nint)(~(-mn))` = -129. (probes p1, p2, p3)

**Suggested fix.** Propagate wrapRisk through the low-bit-preserving unary parents. In narrowArithmeticProducer, for signed ^ and unary +, set wrapRisk=true when ast.Unparen(e.X) is itself a wrapRisk narrow producer (one the Invariant rule left unmarked) or an un-narrowed guarded <<. The simpler alternative is to classify their operand as a Value consumer. Add the arms int(^(a+a)), ^(a+a)>>1 and int(+(a+a)).

### S15 (blocker; lens: GO SEMANTICS OF THE CLASSIFICATION; skeptics refuting: 0 of 2)

**Claim.** `<<` is excluded from producers as an operator "whose emission already narrows the whole result", and its left operand is classified Invariant. That premise holds only on the native path, `(int8)((x << 1))`. When the count is not provably in range, emitGuardedShift returns `receiver.Lsh(count)` before the narrow-SHL cast. An int-promoted receiver binds int32 Lsh: a 32-bit shift with an int result that nothing narrows. Receivers that do this include Invariant-consumed arithmetic, a unary -, or a merely int-typed >>.

**Go:**

```go
var a, b, mn int8 = 100, 100, -128
var u uint8 = 200
var n uint = 1
var i any = (a + b) << n            // Go -112 (int8)
fmt.Println((a + b) << n)           // Go -112
fmt.Println(int((u >> 1) << (n+1))) // Go 144
fmt.Println(int((-mn) << n))        // Go 0
fmt.Println((a*b) << n)             // Go 32
```

**Emitted:**

```csharp
any i = (a + b).Lsh(n);                              // boxes int32 400
fmt.Println((a + b).Lsh(n));                         // 400
fmt.Println((nint)(((u >> (int)(1))).Lsh((n + 1)))); // 400
fmt.Println((nint)((-mn).Lsh(n)));                   // 256
fmt.Println((a * b).Lsh(n));                         // 20000
```

**Evidence.** The overload comes from the receiver's C# type: GoShift.cs:84 `int32 Lsh(this int32 x, uint64 n)` versus :112 `int8 Lsh(this int8 …)`. A plain `a << n` binds the int8 overload and is correct. A typed narrow destination, such as `z = (a+b)<<n` or `x := (a+b)<<n`, is cast by the existing assignment arm, so its low bits come out right. The interface, fmt, widening and >> consumers, which are the class this change targets, see the unwrapped int32. This is pre-existing, but it sits inside the claimed class because the doc comment lists `<<` as already narrowed. (probes p1, p2, p4)

**Suggested fix.** When the shift takes the guarded path (shiftGuardWidth in scope && shiftCountGuarded), classify its LEFT operand as Typed rather than Invariant. Every narrow producer receiver, including merely int-typed ones, is then cast and binds the width-matching Lsh. This must go together with the paren fix in the first finding. Alternatively, treat a guarded `<<` as a producer. Add a variable-count `(a+a)<<n` arm with an `any` consumer.

### S16 (concern; lens: GO SEMANTICS OF THE CLASSIFICATION; skeptics refuting: 0 of 2)

**Claim.** On the guarded path, `>>` on a receiver that is already narrow-typed is not int-typed: GoShift's Rsh returns the narrow type. The classifier still treats every `>>` as "only the promoted TYPE differs" and adds an unneeded self-cast at Typed consumers. Behaviour is correct, but this goes against the doctrine of a cast only where needed. Some of these casts are in the moved goldens.

**Go:**

```go
var a int8 = 100; var u uint8 = 200; var n uint = 1
fmt.Println(a >> n, u >> n)
// NarrowShiftVarCount golden: fmt.Println(cb >> k); fmt.Println(s8 >> k)
```

**Emitted:**

```csharp
fmt.Println((int8)(a.Rsh(n)), (uint8)(u.Rsh(n)));
// golden (moved): fmt.Println((byte)(cb.Rsh(k)));  fmt.Println((int8)(s8.Rsh(k)));
```

**Evidence.** GoShift.cs:103 `uint8 Rsh(this uint8 x, …)` and :109 `int8 Rsh(this int8 x, …)` already return Go's width. The base emitted `a.Rsh(n)` with the correct type and value. The claim that every one of the 138/135/135 footprint lines is a narrow-cast insertion still holds, but some of those casts are not needed.

**Suggested fix.** Skip the self-cast for a SHR whose emission is guarded and whose receiver is not an int-promoted narrow producer. The Rsh result already has the Go type in that case.

### S17 (concern; lens: GO SEMANTICS OF THE CLASSIFICATION; skeptics refuting: 0 of 2)

**Claim.** An index on a type-parameter map is classified as a Value consumer instead of Typed. The IndexExpr arms in narrowConsumerOf and narrowDestinationType test `TypeOf(X).Underlying().(*types.Map)`, but a TypeParam's Underlying is its constraint interface. A merely int-typed key therefore stays a C# int against the uint8-keyed indexer.

**Go:**

```go
var u uint8 = 200
func genIdx[M ~map[uint8]int](m M) int { return m[u>>1] }
```

**Emitted:**

```csharp
internal static nint genIdx<M>(M m) where M : IMap<uint8, nint>, ISupportMake<M>, new()
{
    return m[(u >> (int)(1))];
}
```

**Evidence.** IMap<TKey,TValue> : IDictionary<TKey,TValue> (golib map.cs:63) exposes `this[TKey]`. A non-constant int has no implicit conversion to byte, so this should be CS1503. I did not compile it (the C# cannot be run here). A wrapRisk key such as `m[u+w]` IS cast (Value + wrapRisk), so only >>, %, unsigned /, signed ^ and unary + keys are affected. The base emits the same text, so this is pre-existing and exotic.

**Suggested fix.** Resolve a TypeParam container to its core type before the *types.Map test, in both narrowConsumerOf and narrowDestinationType. Or treat an IndexExpr on a TypeParam container as Typed.

### S18 (blocker; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** When a narrow sum in parentheses is the left operand of a `>>` whose count is a variable, the tip still computes the wrong value, and the emitted text looks as if it were fixed. The new paren-drop in narrowArithmeticParenSelfCast removes the Go parentheses, so the self-cast attaches to the `.Rsh(k)` call instead of to the operand. In C#, `(T)(x).M()` is a cast of the call's result. So the shift runs on the int-promoted value and binds `int32 Rsh`. The function's own comment says 'a narrow integer is never the target of a member access, index or call'. That is false here, because the converter emits every variable-count shift as a `.Rsh(k)`/`.Lsh(k)` extension call on the operand. Without the paren-drop the tip would be correct: an unparenthesized `-m8>>k` renders `((int8)(-m8)).Rsh(k)` and prints Go's -64. Master was also wrong here (plain `(u + u).Rsh(k)`), so this is a fix that did not take effect in a consumer class the change claims (`>>` operand), not a regression.

**Go:**

```go
var u uint8 = 200; var k uint = 1
fmt.Println((u+u)>>k)        // Go 72
fmt.Println(int((u-u-1)>>k))  // Go 127
var z uint8 = (u + u) >> k    // Go 72
```

**Emitted:**

```csharp
fmt.Println((uint8)((uint8)(u + u).Rsh(k)));      // (u+u)=400 int -> int32.Rsh -> 200
fmt.Println((nint)((uint8)(u - u - 1).Rsh(k)));   // -1.Rsh(1) = -1 -> (uint8) 255
uint8 z = (uint8)((uint8)(u + u).Rsh(k));         // 200
```

**Evidence.** Probes p2/p3 converted with the tip binary. golib GoShift.cs:81 `int32 Rsh(this int32 x, uint64 n)` is the overload that binds for the promoted receiver. `go run` prints 72 / 127 / 72. The C# above yields 200 / 255 / 200. The same happens for int8 `(a+a)>>k` (C# 100, Go -28) and int16 `(w+w)>>n` (C# 30000, Go -2768). Every shift in the test arms uses a constant count (`>> (int)(1)`, `>> (int)(4)`), which is emitted as a C# operator, so no arm reaches this shape.

**Suggested fix.** In narrowArithmeticParenSelfCast, keep the parentheses when the ParenExpr is the X of a SHL/SHR BinaryExpr, since a variable-count shift renders as a member call on it. Keeping them always would also work. Correct the comment. Add a variable-count arm, e.g. `(u+u)>>k` and `int((u-u-1)>>k)`, to NarrowArithmeticSinks.

### S19 (blocker; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** A unary `+`, or a signed unary `^`, applied to a narrow result that can overflow gets no cast at all when it reaches a value consumer (widening conversion, index, and so on). The unary parent is classified wrap-invariant, so the inner `a + a` is not cast. The unary itself is classified 'type-only' (wrapRisk=false), so a value consumer does not cast it either. 'Type-only' holds only when the operand is already in range. Here the operand is the un-narrowed sum, so the wrap risk passes straight through the unary.

**Go:**

```go
var a int8 = 100; var u uint8 = 200; var w int16 = 30000
fmt.Println(int(^(a + a)), int(+(a + a)), float64(^(a + a)), int64(+(w + w)))  // Go: 55 -56 55 -5536
fmt.Println(tbl[+(u + u)])  // tbl := make([]int,256) with tbl[i]=i; Go: 144
```

**Emitted:**

```csharp
fmt.Println((nint)(~(a + a)), (nint)(+(a + a)), (float64)(~(a + a)), (int64)(+(w + w)));  // -201 200 -201 60000
fmt.Println(tbl[+(u + u)]);  // index 400 -> panic
```

**Evidence.** Probe p1, tip binary; these lines are identical to master's emission. In narrowArithmeticOperations.go, narrowConsumerOf returns narrowConsumerInvariant for a UnaryExpr parent with SUB/XOR/ADD, while narrowArithmeticProducer leaves wrapRisk=false for token.ADD and for a signed token.XOR. Widening conversions and indexes are both documented Value consumers. By contrast `fmt.Println(^(a+a))` (a typed consumer) and `int(-(a+a))` (unary minus is wrap-risk) are correct.

**Suggested fix.** Carry the wrap risk through: mark a unary `+` or signed `^` producer as wrapRisk when its unparenthesized operand is itself a wrap-risk producer that is left unmarked. Alternatively, classify the child of unary `+`/`^` by the unary's own consumer. Add arms `int(^(a+a))` and `tbl[+(u+u)]`.

### S20 (blocker; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** The docs and the pre-pass comment state that `<<`'s emission 'already narrows its whole result' and that the left operand of `<<` is wrap-invariant. That is false for a variable-count shift whose left operand is narrow arithmetic. The emission `(u + u).Lsh(k)` binds `int32 Lsh` on the promoted receiver and returns int32, and no cast is added because `<<` is excluded as a producer. This is interface-sink divergence, the headline class of the change. It predates the branch and is unchanged by it, but it contradicts the rule as documented.

**Go:**

```go
var u uint8 = 200; var a int8 = 100; var m8 int8 = -128; var k uint = 1
fmt.Println((u+u)<<k, (a+a)<<k)       // Go: 32 -112
fmt.Printf("%T\n", (u+u)<<k)          // Go: uint8
fmt.Println(int((u+u)<<k), -m8<<k)    // Go: 32 0
```

**Emitted:**

```csharp
fmt.Println((u + u).Lsh(k), (a + a).Lsh(k));   // 800 400
fmt.Printf("%T\n"u8, (u + u).Lsh(k));         // int32
fmt.Println((nint)((u + u).Lsh(k)), (-m8).Lsh(k));  // 800 256
```

**Evidence.** Probes p2/p3, identical on master and tip. golib GoShift.cs:84 `int32 Lsh(this int32 x, uint64 n)`. A constant count is fine: `(uint8)((u + u) << (int)(1))` gives 32. Only a typed destination of the same type is correct today (`uint8 y = (uint8)((u + u).Lsh(k))`). native-and-narrow-integers.md, section 'Not covered here', lists `<<` among 'operators whose emission already narrows its whole result'.

**Suggested fix.** For a non-constant count, classify the X of `<<` as a value consumer with wrap risk, so the operand is cast and binds the narrow overload: `((uint8)(u + u)).Lsh(k)`, keeping the parentheses as in the first finding. Alternatively, treat a variable-count `<<` as a producer. Correct the docs sentence either way and add an arm.

### S21 (blocker; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** CaseClause is classified as a value consumer. But a case value is compared at the tag's type, and when the tag is an interface that comparison is typed. A type-only case value (`%`, `>>`, unsigned `/`, unary `+`, signed `^`) is therefore boxed as int32, and `AreEqual(object, object)` returns false on the dynamic-type mismatch. The tip's own comparison path does cast the equivalent `t == a%7`, so the switch and the `==` disagree.

**Go:**

```go
var a int8 = 100; var u uint8 = 200
var t any = int8(2)
switch t { case a % 7: fmt.Println("hit") }   // Go: hit
var t3 any = uint8(100)
switch t3 { case u >> 1: fmt.Println("hit") } // Go: hit
```

**Emitted:**

```csharp
if (AreEqual(exprᴛ1, a % 7)) {...}            // sbyte(2) vs int(2): builtin.cs:3356 type mismatch -> false -> miss
if (AreEqual(exprᴛ3, (u >> (int)(1)))) {...}  // byte vs int -> miss
// while the == form is cast: AreEqual(t, (int8)(a % 7))
```

**Evidence.** Probe p4, tip binary: `go run` prints hit/hit. The emitted switches take the default branch, because golib builtin.cs:3355-3357 returns false when the dynamic types differ. The wrap-risk case `case a + a:` is fixed on the tip (`AreEqual(exprᴛ2, (int8)(a + a))`). The same Value classification applies to a SwitchStmt tag compared against interface case values.

**Suggested fix.** In narrowConsumerOf, return narrowConsumerTyped for a CaseClause (and a SwitchStmt tag) when the tag's type, or the other side's type, is an interface. Keep narrowConsumerValue otherwise. Add an interface-tag arm.

### S22 (concern; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** The arm labelled "case value" does not exercise the CaseClause value path. `switch { case u+u < 150: }` is a tagless switch, so `u+u` sits inside a comparison and is classified typed. The arm is genuinely red at master (the `is < 150` pattern emission bypassed narrowComparisonOperand), but it guards a different path. A real case value (`switch v { case u + u: }`) is fixed on the tip and has no arm; an arm with an interface tag would have caught the previous finding.

**Go:**

```go
v := uint8(144)
switch v { case u + u: fmt.Println("case hit") default: fmt.Println("case miss") }  // Go: case hit
```

**Emitted:**

```csharp
master: if (exprᴛ1 == u + u)            // 400 -> case miss
tip:    if (exprᴛ1 == (uint8)(u + u))   // case hit, but no arm guards it
```

**Evidence.** NarrowArithmeticSinks/main.go 'case value' arm; probe p1 diff between master and tip. The narrowConsumerOf CaseClause branch is reached by no arm in either project.

**Suggested fix.** Rename the existing arm to "case comparison". Add a true tagged-switch case-value arm, plus an interface-tag variant.

### S23 (concern; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 1 of 2)

**Claim.** The NarrowShiftVarCount golden gains two casts that are not needed: `(byte)(cb.Rsh(k))` and `(int8)(s8.Rsh(k))`. With a narrow variable as receiver, `.Rsh` already binds the narrow overload and returns byte/int8, and master printed the correct value and type. The pre-pass treats every `>>` as int-promoted without knowing whether the emission will be the C# operator (constant count) or the narrow extension (variable count). This breaks the 'cast only where needed' doctrine. The golden's own header still says 'Right shifts of a narrow operand ... take no cast'. The corpus footprint probably contains more hunks of the same kind wherever a narrow variable is shifted right by a variable count and reaches a typed consumer.

**Go:**

```go
var cb byte = 200; var s8 int8 = -100; var k uint = 1
fmt.Println(cb >> k)
fmt.Println(s8 >> k)
```

**Emitted:**

```csharp
master: fmt.Println(cb.Rsh(k));        fmt.Println(s8.Rsh(k));
tip:    fmt.Println((byte)(cb.Rsh(k))); fmt.Println((int8)(s8.Rsh(k)));
```

**Evidence.** git diff of src/tests/Behavioral/NarrowShiftVarCount/main.cs; golib GoShift.cs:103/109 (`uint8 Rsh(this uint8 ...)`, `int8 Rsh(this int8 ...)`). By contrast `cb << k` stays `cb.Lsh(k)` with no cast, so the two shift directions are handled inconsistently.

**Suggested fix.** Do not mark a `>>` producer whose count is non-constant and whose unparenthesized X is not itself a marked or int-promoting expression, because its emission is already narrow. Alternatively, have convBinaryExpr skip the self-cast when the rendering is a narrow-receiver `.Rsh(` call. Update the NarrowShiftVarCount header either way.

### S24 (concern; lens: TEST ARMS AND DOCS VERSUS CLAIMS (claude/bold-thompson-e4gztv @ ff71ab6a9e over master 1aebd6a885); skeptics refuting: 0 of 2)

**Claim.** Several consumer classes that the code changes, some of which the docs name, have no arm in either project. The tip fixes each of these relative to master, so each is unguarded behaviour: a map-literal KEY (master emits a compile-error shape); a `make` length or capacity; a 3-index slice max; a divisor, i.e. the right operand of `/` or `%`; unary minus at a widening conversion; the RHS of `/=` and `%=`; a return inside a func literal; explicit generic instantiation; and tuple assignment. Beyond these, no arm shows that the invariant consumers (same-width `+ - *`, a narrowing conversion) still get no cast, apart from incidental target text.

**Go:**

```go
mk := map[uint8]string{u + u: "wrapped"}
sl := make([]int, u+u)
_ = cap(tbl[:10:u+u]); _ = 100/(a+a)
fmt.Println(int(-m8))  // Go: -128
```

**Emitted:**

```csharp
master -> tip:
new map<uint8, @string>{[u + u] = ...}  ->  {[(uint8)(u + u)] = ...}
new slice<nint>(u + u)                  ->  new slice<nint>((uint8)(u + u))       // len 400 vs 144
tbl.slice(-1, 10, u + u)                ->  tbl.slice(-1, 10, (uint8)(u + u))
100 / (a + a)                           ->  (int8)(100 / (int8)(a + a))           // 0 vs -1
(nint)(-m8)                             ->  (nint)((int8)(-m8))                   // 128 vs -128
```

**Evidence.** Probe p1, diff between master and tip emission, with `go run` output for comparison. None of these shapes appears in NarrowArithmeticSinks/main.go or NarrowArithmeticCompileSinks/main.go.

**Suggested fix.** Add a compile arm for the map-literal key and runtime arms for make, the 3-index max, the divisor and `int(-m8)`. Optionally add a negative-control arm that asserts the no-cast text for `int8(u+u)` and `a+a+a`.
