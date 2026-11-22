# N3 bench reading, 2026-11-13 18:31 CST

Host: Windows x64, SDK 10.0.400 (runtime 10.0.11). `Program.cs`, `n3bench.csproj` (PublishAot in the project, so the
JIT run also reports dynamic code off), `run.sh`. JIT build and Native AOT publish (win-x64, 0 errors; vswhere's
directory had to be on PATH for the link step). Five processes each; each arm's floor of 7 passes of 2,000,000 ops,
then the floor over the five processes; bytes per op from the thread's allocation counter.

| arm | what | JIT ns | JIT B | AOT ns | AOT B |
|---|---|--:|--:|--:|--:|
| A0 | a closure literal created and called (today) | 10.40 | 88 | 11.81 | 72 |
| D3c | the same literal created as a pair, then called | 26.36 | 248 | 30.70 | 200 |
| A0call | a call through a closure delegate | 0.39 | 0 | 1.60 | 0 |
| D3call | a call through the pair-bound delegate | 0.74 | 0 | 1.83 | 0 |
| D3s | a call through a static function's cached pair | 2.53 | 0 | 2.06 | 0 |
| D3t | the twin-side test: is the delegate's Target the pair type | 0.81 | 0 | 8.30 | 0 |
| D1hit | ConditionalWeakTable lookup of a known delegate | 12.44 | 0 | 11.74 | 0 |
| M | Delegate.Method read on a fresh delegate | 81.29 | 184 | 498.15 | 296 |
| D1reg | fresh delegate: CWT miss, Method read, method table, registration | 541.21 | 213 | 1049.89 | 319 |

Predictions (posted 18:17):
- D3 creation +10..30 ns over A0: TRUE (JIT +16, AOT +19). Bytes per creation: +160 JIT, +128 AOT (COORD's arm).
- D3's call-side test under 3 ns: TRUE on the JIT (0.8), FALSE under AOT (8.3): the generic type test costs more there.
- Delegate.Method under AOT over 200 ns: TRUE (498), so D1 pays about 1 us at each delegate's first twin call.
- One unwrap rule shared with S1's wrapper rule: the bench shows why it is needed. The handed-out delegate's Method is
  `Invoke` and its Target is the pair, so every golib reader of Delegate.Method/Target must see through a pair:
  GoFuncCookie (S1), GoReflect.TypeLayout's func-name reader (which already unwraps a `new F(g)` wrapper), GoSyntheticPC,
  ж.PointerTokens, builtin.DelegateConversions (a rebind over a pair keeps the pair, which is right).

Stop lines: D3 creation over 100 ns: not tripped (31 ns). reflect/FuncForPC names: they WOULD change under a raw D3
(Method reads `Invoke`), so the design must carry the unwrap rule; no arm here measures it with golib.

## Run 2: the bound-method arms (2026-11-13 23:06 CST, review fix 2)

Same host, same method; four arms added (`prediction-bound.md`, written 22:58 before the build). Run 1's files are in
`run1/`; run 2's in `run2/`. Run 2 re-reads run 1's arms within noise (JIT A0 11.4 vs 10.4 ns, D3c 29.7 vs 26.4;
AOT M 448 vs 498).

| arm | what | JIT ns (floor / median of 5) | JIT B | AOT ns (floor) | AOT B |
|---|---|--:|--:|--:|--:|
| B0p | a bound method value, pointer receiver, created and called (today) | 10.74 / 10.95 | 64 | 9.25 | 48 |
| D3bp | the same as a pair: receiver read once, both halves bind it | 25.89 / 31.73 | 224 | 28.18 | 176 |
| B0v | a bound method value, value receiver: one copy in a closure (today) | 12.48 / 13.25 | 88 | 11.70 | 72 |
| D3bv | the same as a pair: ONE copy, shared by both halves | 26.86 / 35.51 | 248 | 30.76 | 200 |

Predictions scored:
- P-b1 (+10..30 ns over today): TRUE. Floors: +15.2 / +14.4 JIT, +18.9 / +19.1 AOT; JIT medians +20.8 / +22.3 (the
  JIT pair arms spread 26 to 43 ns over the five processes; AOT within 1 ns).
- P-b2 (+160 B, pointer receiver): TRUE on the JIT (+160); +128 under AOT, where a delegate is 48 B, not 64.
- P-b3 (+160 B, value receiver, the copy shared): TRUE the same way (+160 JIT, +128 AOT): the copy is not doubled.
- P-b4 (no arm over 100 ns): TRUE (the largest pair arm, 42.8 ns, one JIT process).
So the bound case costs what a literal does: S4's 160 B delta holds for both receiver kinds.
