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

## COORD's ruling on the scope (2026-11-23): the Go-order move plus inline re-derivation

RB-b and RB-c stand **MISSED**, as scored above. The cure is (1) + (2). Conditions:
- (a) each operand class admitted to the move gets a go1.26.11 probe showing gc evaluates it AFTER the call, run three
  ways (Go, plain, twinned); a class whose probe disagrees with gc leaves the move set; a conversion that can panic
  (slice to array or array pointer) stays out unless its probe says otherwise;
- (b) every program in the MN CNR set or the full behavioral suite whose output differs plain against twinned must
  read twinned == Go, each one named; KnownIssues discloses the plain path's C# order; plain-path order parity with
  gc is a BOARD row of its own;
- (c) the residual named by class with counts;
- (d) a prediction for the post-cure census first, then the gates of pieces 4-6.

**Where the probe lives.** `src/tools/mnspike2/MnGoOrder` (Go source and its conversion), not the behavioral suite.
Its plain run differs from Go BY DESIGN: that is the disclosed plain-path gap, and in the suite its plain output
comparison would fail. It moves into the suite with the BOARD item that gives the plain path gc's order. Each case is
written in the census's dominant blocked shape: a `Printf` whose u8 format is ref-like, then the operand class under
test, then a call that parks and writes that operand. So the twinned run before the cure prints the plain (C#) order,
and after it gc's.

## The cure (design, stated before code)

1. **The Go-order move.** In RefLikeBefore, an operand evaluated before the call stops blocking the move when it is a
   pure read: an identifier naming a local, a parameter, a ref local or a field of the receiver; a member access chain
   over such reads (a field of a value, or a field through a pointer or box); a literal or constant; a cast (a
   numeric, string or interface conversion) of a pure read; a unary operator (`-`, `!`, `~`, `+`) on a pure read. A
   binary operator's LEFT operand that is a pure read no longer blocks a call in its right operand. Calls, indexers,
   element accesses, object creations, slice-to-array conversions and anything else still block. The moved call
   runs ahead of its statement as a spill already does, and Rule S derives the live refs again after it.
2. **Inline re-derivation.** Where the call still cannot move (an if, else-if or while header, a case guard), it is
   awaited in place, and each ref local read after it within the same evaluation is replaced by its re-derivation
   source (`Ꮡx.DerefOrNull()`, `Ꮡv.ValueSlot`, the box parameter's deref). The ref local itself is not touched
   there; Rule R derives it again at the next statement as before.

## Predictions (before the cure's code; windows, the whole standard library twinned)

- RB-d: the probe reads gc evaluating each of the 7 admitted classes AFTER the call (local, parameter, field of a
  value, field through a pointer, unary, binary left operand, cast); in the nil-pointer case, the call's side effect
  is visible before the panic. The slice-to-array conversion is probed and kept out either way.
- RB-e: the probe's twinned run equals Go on every admitted class after the cure, and equals the plain run (C#
  order) before it.
- RB-f: the post-cure census reads at most 90 ref-blocked calls (from 341), so at least 250 are unblocked. The
  residual is mostly an object creation evaluated first, a call evaluated first, and calls inside lambdas.
- RB-g: in the MN CNR set and the behavioral suite run twinned, a plain-vs-twinned output difference appears in at
  most 2 programs, and each reads twinned == Go.

## The cure, read (2026-11-23, working tree on 7f77fb387e)

**The probe** (`src/tools/mnspike2/MnGoOrder`, go1.26.11). gc reads every admitted class AFTER the call: local,
parameter, field of a value, field through a pointer, unary, binary left operand, cast. In the nil-pointer case the
call's side effect is visible before the panic. The slice-to-array conversion panics BEFORE the call (gc converts
first), so it stays out of the move set, as ruled. **RB-d met.**
Run three ways: Go; plain (plain program, plain standard library); twinned (GoTwinsFor, the whole standard library
twinned) with GO2CS_MN unset and set. Before the cure all 9 calls are ref-blocked, and plain and twinned both print
the C# order on 8 lines (each one value stale; the nil pointer panics before the call). After the cure, the twinned
run under GO2CS_MN=1 is **EQUAL to Go on all 9 lines**. 8 calls now move; the slice-to-array call stays ref-blocked
by design. The plain run still prints the C# order: that is the disclosed plain-path gap. **RB-e met.** (With
GO2CS_MN unset the twinned build runs its plain bodies, so it prints the plain order, as it must.) The probe's first
form put the nil-pointer and slice-to-array cases in function literals called in place. A literal invoked directly
runs its plain half (a func-value pair whose call site the generator does not test), so the twin never ran there. The
cases moved into named functions, and the record keeps the reason.

**GenTests**, red first. `TwinOrderTests`, 6 rows, each executing the twin against Go's value: a local, a unary
operand, a binary left operand and a numeric cast each read after the call; a call evaluated first still pins the
parking call (the residual control); an if header awaits in place and reads the ref through its box. Without the cure
the 5 cure rows fail and the control passes. Two existing rows change with the behavior: `P28` (a stack ref's later
parking call now moves ahead instead of staying plain) and piece 6's else-if row (the call now awaits, and p is read
through its box). GenTests 802/802.

**The post-cure census** (windows, the whole standard library twinned, every project rebuilt): rc=0,
GO2CS0017/0018/0021 = 0, 358 reports, rows equal the count in every one. **120 ref-blocked calls in 35 packages, from
341: 221 unblocked (65%).** 40 by inline re-derivation; the rest move ahead (ref spills 995 to 1,189). **RB-f
MISSED**: at most 90 were predicted to remain, and 120 do (221 unblocked against the at-least-250 predicted). A first
read counted 118 over 336 reports: projects the probe had already rebuilt were skipped as up to date and wrote no
fresh report. That read is void, and the census is now run with --no-incremental.

**The residual (COORD's (c)), by class, with counts.** Each one stays a plain call that holds its carrier when it
parks, and each is a real ordering constraint the move cannot cross:
| class (first blocker) | calls |
|---|---|
| a cast of a call evaluated first (e.g. elf's `(ProgFlag)bo.Uint32(…)`, an interface call that does not spill) | 33 |
| an object creation evaluated first | 18 |
| an element access or indexer evaluated first (or the call is an index) | 18 |
| a call evaluated first | 10 |
| a tuple element (`return (x, f())`-shaped, the earlier element not a pure read) | 8 |
| an identifier that is not a pure read (a property or method group) | 8 |
| a call inside a lambda | 5 |
| a throw statement | 4 |
| an if header whose reason is ref-like (not a ref read) | 3 |
| inside a call that already moves | 3 |
| a compound assignment | 3 |
| a collection-expression element | 2 |
| a declaration expression target | 1 |
| a unary operand, a member read or an equality that is not a pure read (one each), a `default` literal | 4 |
| **total** | **120** |
By reason: ref-like-before 75, store-through-ref 40, ref-read-after:unrederivable 5.

## (b): the whole behavioral suite in M:N mode, with the standard library twinned (2026-11-23)

`BehavioralRunner --mn` (every project built with twins, every program run with GO2CS_MN=1, output compared with Go)
over all 1,040 projects, with `GoStdlibTwins=true` in the environment (so every child build twins the standard library
too). This is the first full-suite `--mn` run; the battery's `--mn` leg runs the 87 projects of its filter
`Mn|Select|Channel|GoCall|Defer`. Result: **1,012 compared, 4 failing**, 8 not compared (no
GoTestMatchingConsoleOutput, as always). Attributed in four arms on the four projects: the cure on and off, the
standard library twinned and plain. **All four fail in every arm**, so none is this seat's, and none comes from
twinning the standard library; they are at this seat's base:
- MnIfaceTwin [Compile]: piece 1's GO2CS0021 on a consumer's hand-written instance method, once train 5 made
  time.Sleep's twin public. COORD ruled it a train 6 union fix of its own: claude/g-t6-union-fix 4b5c8d96e2
  (host-not-partial checked after the structural refusals). This seat merges or rebases over it and carries no copy.
- GoroutineStackReserve [Output, stack overflow], GoStmtValueReturn [Compile, CS8030], PromotedMethodFrames [Output]:
  pre-existing `--mn` gaps outside the battery's filter. A BOARD row each, OPEN (they ride the union-fix commit).

**No program shows a plain-against-twinned difference from the order change.** The plain suite passes against Go (train
5's battery), and every compared program reads twinned == Go except the two pre-existing output failures above, neither
an operand-order case. **RB-g met** (at most 2 predicted; 0 from the order).
The run cost about 40 GB of build output (free disk fell to 7 GB), purged after.

## Gates (2026-11-23, working tree on 7f77fb387e)

Kit `C:\go2cs\scratch\mn2\proto\row7`: rb-gates.sh, rb-probe.sh, rb-tally.sh, p6-probe.sh, rb-behav.sh, rb-attr.sh.
- **Made to fail, each restored byte-identical.** N7a (a pure read no longer counts) reds 5 rows: P28 and the local,
  unary, binary-left and cast rows. N7b (no inline re-derivation) reds the if-header and else-if rows. N7c (no binary
  right-operand case) reds the binary-left row alone.
- **The whole standard library twinned**, every project rebuilt, rc=0 with GO2CS0017/0018/0021 = 0 on windows (120
  ref-blocked), linux (117) and darwin (117); `// B` rows equal the count in every report.
- **CNR both ways**, 8 programs: MnSpinCap, MnContended, MnForeignThread, MnMixedStorm (n=1,000), MnLockPin,
  MnMutexTwin, MnBlockedIO and MnGoOrder. Every run of the first seven is identical to its baseline, and EQUAL to Go
  where a Go source exists (MnLockPin has none: pins=11, off-carrier-locks=0, 210,121 parks). MnGoOrder: plain and
  twinned-with-MN-unset are identical to the plain baseline (the C# order); twinned under GO2CS_MN=1 is EQUAL to Go
  and differs from that baseline, which is the cure and the disclosed plain-path gap. MnSpinCap's parks under the
  scheduler rose from 80,345 to 92,953 with the cure: more calls park instead of holding their carrier.
- **Plain build unchanged**: the 13 R5 packages, fmt, runtime and sync built plain with the seat's generator and with
  the generator at 7f77fb387e. 0 differing lines in each (types, fields, method IL).
- GenTests 802/802; GolibTests Release 3,135 passed (53 skipped), Debug 3,109 passed (79 skipped).
- The Q1 kit: 0 differing over the twin runs it made (12). Three of its builds fail at this base and not from the
  cure: MnIfaceTwin's twins and ifhall builds (the GO2CS0021 the train 6 union fix cures); MnStormTwin's twins build
  (its hand-written main.cs calls `Lockꞏtwin` on a `Mutex` value, and train 5's row 7 piece 2 made it take the box);
  and MnLaterArg, whose converted project is not on this base (go.mod and main.go only).

**Scored:** RB-d met, RB-e met, RB-f MISSED (120 remain, against at most 90 predicted; 221 unblocked), RB-g met.
