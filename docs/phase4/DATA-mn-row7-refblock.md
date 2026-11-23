# Row 7: the ref-blocked calls, unblocked (record)

**G, 2026-11-23.** COORD's ruling: the ref-blocked call sites (295 in 55 packages on windows when pieces 4-6 were
gated, DATA-mn-row7-r5.md) are a .5 item, not a follow-up. Each is a call to a twin that stays PLAIN in its caller's
twin, because the call cannot move ahead of its statement and awaiting it in place would hold a ref across the await
(CS9217); so it holds its carrier when it parks, the cut the owner ruled out. The proposed cure: await in place and
derive the ref again INLINE, through its box, where the statement reads it after the call. Branch `claude/g-mn-refblock`
= train 5's master 1079ca371f with claude/g-mn-twins-p45 (f8790b5d60) merged, as COORD ruled (4a355f34a5); it seats
after p1 and p45 in train 6. Prediction first: how many the cure unblocks, and the residual by named class.

## The census instrument (committed with this record, stated before the count prints)

The twin generator's report gains one row per ref-blocked call: `// B file|line|caller|callee|reasons|blocker`.
- **reasons**, as RefLikeBefore decides the spill: `ref-like-before` (a ref struct value, or a by-ref argument or
  receiver, is evaluated before the call), `store-through-ref` (the call's value is stored through a ref), and
  `ref-read-after` (a ref local is read later in the same evaluation), the last one split by whether every such ref
  has a live re-derivation (`:rederivable`, Rule S could derive it) or not (`:unrederivable(names)`). The three are
  read in that order, as the spill rule reads them, so a row names the first that holds.
- **blocker**: the FIRST thing that made the call unmovable: `stmt:<kind>` (the statement is not an expression,
  declaration or return), `nested:<kind>` (an else-if or other nested statement), `in:<kind>` (a lambda, a member, or
  an expression the walk does not see through), `earlier:<kind>` (a non-inert value evaluated before the call),
  `element-index`, `store-left:<kind>`, `multi-declarator`, `inside-moving-call`.

## Controls (stated before the count prints)

- **Row count equals the count**: in every report the `// B` rows equal `ref-blocked=`; any gap is an instrument fault.
- **Known positive**: archive/zip's `checksumReader.Read` row (piece 6's shape 3) reads
  `ref-read-after:rederivable|nested:IfStatement`.
- **The instrument changes nothing else**: the twinned standard library's twins (every `*.twins.g.cs`) are
  byte-identical with and without it, except the report's new rows.

## Predictions (before the count prints; windows, the whole standard library twinned)

- RB-a: the total is between 295 and 380. This tip carries row 7 pieces 2 and 3 (Lock and Sleep twins), so more
  callees park than at pieces 4-6's gate.
- RB-b: `ref-read-after` is the largest reason (over half), and `ref-like-before` is second, mostly u8 format
  literals (`fmt.Fprintf(w, "…"u8, …)`) and spans evaluated before the call.
- RB-c: the inline cure can unblock exactly the rows whose ONLY reason is `ref-read-after:rederivable` and whose
  blocker is not inside a lambda or another member (`in:` an anonymous function or member declaration). Predicted:
  at least half of the total. The residual, by class: `ref-like-before` (a ref struct value cannot outlive an await
  at all), `store-through-ref` (the target's reference is taken before the await), `ref-read-after:unrederivable`,
  and calls inside lambdas.

## Census reading (2026-11-23, tip a029d9475a, windows)

The whole standard library twinned: rc=0, 0 errors, GO2CS0017/0018/0021 = 0, 358 reports.

Controls: every report's `// B` rows equal its `ref-blocked=` (0 mismatches); the known positive is there,
`reader.cs|333|Read⁔|…|ref-read-after:rederivable|nested:IfStatement`. The third control (the twins otherwise
byte-identical) is read with the cure's gates.

**341 ref-blocked calls in 59 packages** (the 295 was read at pieces 4-6's tip, without row 7 pieces 2 and 3; more
callees park here).

| reason | rows |
|---|---|
| ref-like-before | 214 |
| ref-read-after:rederivable | 60 |
| store-through-ref | 52 |
| ref-like-before + store-through-ref | 10 |
| ref-read-after:unrederivable | 5 |

The first blocker: `earlier:IdentifierName` 129, `earlier:SimpleMemberAccessExpression` 59, `in:AddExpression` 40,
`earlier:CastExpression` 33, `earlier:ObjectCreationExpression` 19, `earlier:InvocationExpression` 13,
`stmt:IfStatement` 7, `in:LogicalNotExpression` 6, `in:ParenthesizedLambdaExpression` 5, and 32 more in 17 kinds.

**Scored:** RB-a **met** (341, inside 295-380). RB-b **missed**: `ref-like-before` is the largest reason (214, 63%),
not `ref-read-after` (65). RB-c **missed**: the inline re-derivation alone can unblock **60 of 341** (18%), not half.

**What the reading shows instead.** The ref-like value is evaluated before the call only because something else is,
and that something is usually a plain read. Crypto/x509's `s.addCertFunc(sha256.Sum224(cert.Raw), …)` reads the
receiver `s` before the parking Sum224. Archive/zip's `f.Modified = modified.In(timeZone(…))` reads `modified` before
the parking timeZone. In both, the parking call cannot move ahead of its statement, because the walk treats a plain
read before it as an evaluation that must stay first. Go does not order those: "all function calls, method calls,
receive operations, and binary logical operations are evaluated in lexical left-to-right order", and a variable's read
is not ordered against a call. (gc itself loads variables after the calls in an expression.) So the call may move
ahead, Rule S derives the refs again after it, and the ref-like values are then evaluated after the await.
By first blocker, 267 of the 341 are blocked first by a plain read, a member read, a cast or an arithmetic or logical
operator. That is the candidate population for that relaxation, an upper bound (a later operand may still block);
30 more need the inline re-derivation alone. The residual, 44: an object creation evaluated first (16), a call
evaluated first (10), calls inside lambdas (5), and 13 singles.

Go probe (go1.26.11, scratch `row7/goorder`): `a := 1; f := func() int { a = 2; return 3 }; fmt.Println(a, f())`
prints `2 3`; a member read before a call prints the member as written by the call (`10 5`); a value receiver read
before a call in its argument sees the call's write (`107`). So gc evaluates the calls first and reads afterwards, and
a twin that moves the call ahead matches gc. The plain conversion reads first (C# order), so for code that depends on
this unspecified order, plain and twin would differ while both stay Go-legal. That is a CNR exposure the gates must
read.
