# sortbench: prediction and stop line (written 2026-11-13 23:34 CST, before the build)

What it measures: the caller-local rule's pure overhead on a hot callback path. main runs as a twin, so under the
rule it calls the sort's TWIN; every recursion is a twin-to-twin GoTask call, and every less/cmp call first tests
the delegate's target for a pair (non-generic `is GoFuncPair`), then calls plain (a comparison is never a pair).
100,000 random ints; ns per element; floor of 7 passes per process; 5 processes; JIT build and Native AOT publish.

STOP LINE (stated before the run): the rule is not acceptable as the DEFAULT if the twin sort costs more than 1.25x
the plain sort on either runtime, for either shape. Over that, the rule needs an opt-out for callback-only callers
(a func-value parameter that is only ever called, never stored or sent), or a different rule.

Predictions:
- S1: sort.Slice shape, twins over plain: x1.05..x1.25 on the JIT (the test is ~1 ns against a ~3-5 ns less call;
  the recursion is shallow, about 2 * N / 12 twin calls).
- S2: the same under AOT: x1.10..x1.35.
- S3: slices.SortFunc shape (generic): within 10 points of S1/S2 on each runtime.
- S4: the non-generic pair test alone: under 1.5 ns on both runtimes (the generic test read 8.3 ns AOT).

## Run 1 read (2026-11-13, after the build; five processes each)
AOT (stable): sort.Slice shape plain 85.9-98.9 ns/elem, twins 245.7-259.3 (x2.58-x3.02); SortFunc shape plain
77.2-80.0, twins 231.6-308.5 (x3.00-x3.99). Pair test (non-generic) 8.2-8.7 ns/call.
JIT (UNSTABLE, not usable as is): sort.Slice plain swings 62-309 ns/elem between processes, SortFunc plain ~180
against twins ~88; pair test 4.0 ns. Tiering/OSR order effects; re-run needs TieredCompilation pinned (or
TC_QuickJitForLoops off) and arms interleaved.
Scored: S2 FALSE (x2.6-3.0, far over x1.10-1.35); S3 FALSE under AOT (generic x3.0-4.0); S4 FALSE (non-generic test
8.2 ns AOT, 4.0 JIT, over 1.5); S1 unreadable until the JIT re-run. STOP LINE TRIPPED under AOT: the caller-local
rule is not acceptable as the default for callback-only callers as measured.
Caveat to check: the twin arm's cost includes GoTask state machines for EVERY recursion and partition call; a
generated twin of a body with no await of its own may compile differently (the generator decides). Read the real
emission before ruling.

## JIT re-run, tiering off (DOTNET_TieredCompilation=0), five processes
sort.Slice shape x1.96-x2.02 (plain 86.2-89.3, twins 172.6-176.7 ns/elem); SortFunc x2.02-x2.11; pair test 4.2 ns.
S1 FALSE (x2.0, over x1.05-1.25). STOP LINE TRIPPED on both runtimes.

## Decomposition arm (predicted before the build)
TN / TNG: the same twins with NO pair test (the dynamic call is plain). This is also the HOISTED shape (the test
made once per func-value variable that is never reassigned, at the twin's entry), short of one test per sort.
- D1: TN over plain x1.05..x1.30 on both runtimes (GoTask state machines on ~2N/12 calls only), so the per-call test
  is most of the measured x2-x3.

## Run 2 read (decomposition; five processes per runtime; JIT with tiering off)
| arm | JIT (TC off) | AOT |
|---|--:|--:|
| T, twins + per-call pair test, sort.Slice shape | x2.04-x2.11 | x2.87-x2.89 |
| TG, the same, SortFunc shape | x2.07-x2.09 | x3.14-x3.18 |
| TN, twins, NO per-call test (= hoisted), sort.Slice | x1.22-x1.23 | x1.22-x1.25 |
| TNG, the same, SortFunc | x1.17-x1.18 | x1.19-x1.23 |
D1 TRUE (x1.17-x1.25, in x1.05-x1.30). The per-call pair test is most of the cost (4.2 ns JIT, 8.2 ns AOT, about
17 comparisons per element); the GoTask state machines on the ~2N/12 twin-to-twin calls are the other x1.2, which is
the M:N cost of ANY twinned callee, not this rule's. With the test HOISTED (once per func-value variable that is
never reassigned, at the twin's entry), the rule sits AT the stop line (x1.25 at worst), not under it.
