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
