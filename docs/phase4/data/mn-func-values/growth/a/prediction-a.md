# Shape (a): predictions (written before the build ran any arm)

(a) = propagating caller rule + the hoisted pair test + an opt-out for callback-only callers (COORD 2026-11-14): a
func-value PARAMETER only ever called, or passed on to a callback-only parameter of a static callee (fixpoint), is
callback-only; a function whose every func-value call goes through such parameters takes no caller-rule twin, and
those sites fall back, warned once per function. Tool: census3 (-opt-out-callbacks), same reach and cuts as section
10, fix 1 over the rule's own set.

Read from the sources before predicting: sort.Slice wraps less in a lessSwap STRUCT (stored) and pdqsort_func calls
data.Less through a field, so sort.Slice is NOT opted out; slices.SortFunc passes cmp straight down
(pdqsortCmpFunc ...) and only calls it, so it is; strings.Map calls mapping only; (*sync.Once).Do passes f to doSlow,
which only calls it, so Once.Do is opted out.
- A1 net/http closure: twins 2,400..3,100 (between leaf+fix1 1,820 and propagating 3,371); awaited 24..28 of 31;
  rule-own holes 0 (an opted-out site is a fallback, not a hole).
- A2 TypeScript: twins 4,000..8,000 of 16,881 (propagating 9,307); awaited 12..16 of 18.
- A3 the opt-out's real cost: at least 5 opted-out sites in net/http's closure are reached by a REALLY parking value,
  through (*sync.Once).Do (a Once whose function parks).
- A4 sortbench under (a): slices.SortFunc is plain by construction (x1.00); sort.Slice stays twinned (it stores
  less), so it keeps the hoisted x1.17-x1.25 -- AT the line, not back to plain.

## Scored (census3, the S1-merged tree's tool state; net/http and TypeScript)
| | propagating + fix 1 | shape (a) |
|---|--:|--:|
| net/http closure: twinned (of 8,647) | 3,371 (39.0%) | 2,919 (33.8%) |
| net/http itself (of 1,144) | 647 | 561 |
| net/http: awaited, transitive (of 31) | 28 | 11 |
| net/http: rule-own holes | 0 | 12 |
| net/http: fallback (opt-out) sites among the 31 | - | 6 |
| TS: twinned (of 16,881) | 9,307 (55.1%) | 8,315 (49.3%) |
| TS: awaited, transitive (of 18) | 16 | 5 |
| TS: rule-own holes | 0 | 7 |
| TS: fallback sites among the 18 | - | 8 |
Opted out: net/http closure 235 functions wholly + 14 partly, 379 fallback sites; TS 649 + 18, 1,010 sites.
Opt-out's real cost (opted-out sites a REALLY parking value reaches): net/http closure 128, TS 94. Top carriers:
cryptobyte.(*Builder).callContinuation 64 (VTA-imprecise), (*log.Logger).output 12, (*sync.Once).doSlow 11,
fips140.WithoutEnforcement 6, http2serverConn.runHandler 5, ... and (net/http.HandlerFunc).ServeHTTP 2: the
MOTIVATING site is itself opted out (its receiver f is a parameter only called). TS: Once.doSlow 10,
GetDiagnosticsOfAnyProgram$1 8, GetOutputFileNames$1 8, strings.splitSeq$1 6, ...
- A1 twins TRUE (2,919 in 2,400..3,100); awaited FALSE (11, not 24..28); holes FALSE (12, not 0: a fallback runs
  the callback's plain body, so a twinned callback reached through an opted-out caller is holed).
- A2 FALSE (8,315 above 4,000..8,000; awaited 5, not 12..16).
- A3 TRUE (128, far over 5).
- A4 TRUE: sort.Slice stays twinned (WHY: sort.Slice -> pdqsort_func, a func-value seed through lessSwap's field),
  so it keeps the hoisted x1.17-x1.25; slices.pdqsortCmpFunc[...] is opted out, so SortFunc is plain (x1.00).
