# Twin growth under the caller-local 1' rule: prediction (written 2026-11-13 23:23 CST, before any change or run)

The rule measured: a function whose body has a direct call through a func value (not a `go` statement, not a
`defer`) gets a twin; its callers then twin by the generator's existing rule (a direct call of something that can
park). The instrument: the N3 census tool (`canpark` + creation pass) with a new flag that seeds every such function
as parking, then the existing reverse closure. Reach: the generator's (`-no-func-values -long-only
-exclude-runtime`), windows, no cuts. Program: `net/http` and its closure (every loaded package), declared functions.

- G1 (posted to COORD 23:20): the replacement adds +5..15% twinned functions in net/http's closure, relative to the
  functions twinned today.
- G2 (positive control): `(net/http.HandlerFunc).ServeHTTP` is twinned with the seed and not without it.
- G3: the seed's direct set (functions with a direct func-value call) is under 10% of the closure's declared
  functions; the growth comes more from propagation than from the seeds themselves.
- G4: the higher-order helpers (`sort.Slice`, `slices.SortFunc`, `strings.Map`, `sync.Once.Do`) are seeds; their
  callers then twin, which is where any explosion would come from.
- G5 (twin code size, the proxy for compiled size): the SSA instruction count of twinned functions grows in the same
  band as the function count, +5..15%.
Not measured by this tool: build time and compiled size of net/http's twin assembly (they need the generator, in the
code stage); G5 is the proxy offered instead.

## Scored after the first arm (2026-11-13 23:27 CST), and the re-review's arms predicted

First arm read (propagating seeds, net/http's closure): twinned 1,496 -> 3,317 (+122%); instructions +72%; net/http
482 -> 637 (+32%); new seed functions 341 declared. G1 FALSE (far over +5..15%); G2 TRUE; G3 TRUE (seeds 3.9% of
declared, the growth is propagation); G4 TRUE (sort.Slice, strings.Map, Once.Do now twinned); G5 FALSE (+72%).

The re-review's arms (tool reworked: base parks decide which sites matter; seeds and factories decide twins; each
site's caller classified twin / twin-holed / plain; AWAITED = every creator a twin AND the caller entered as twin):
- H1 (control): the base arm reproduces the posted net/http census, 18 covered / 2 partial / 11 uncovered.
- H2: leaf-only seeds add +15..25% twinned functions in the closure (about the seed count, no propagation).
- H3: leaf-only leaves most seeded sites HOLED: twin-holed outnumbers twin among their callers; AWAITED gains at most
  5 sites over base.
- H4: propagating seeds + factories: AWAITED at least 27 of 31 (the reviewer reads 29 after fix 1; init rows stay).
- H5: base + factories (fix 1 alone): covered rises to at least 27, but AWAITED stays under 20 (ServeHTTP is plain).

## Scored, one-level entry (the six arms, net/http)

| arm | twinned (closure) | net/http | covered/partial/uncovered | awaited (one level) |
|---|--:|--:|--:|--:|
| base | 1,496 | 482 | 18/2/11 | 13 |
| fix1 | 1,526 | 496 | 28/0/3 | 19 |
| leaf | 1,748 | 521 | 18/2/11 | 17 |
| leaf-fix1 | 1,776 | 535 | 28/0/3 | 27 |
| prop | 3,317 | 637 | 21/2/8 | 21 |
| prop-fix1 | 3,329 | 646 | 28/0/3 | 28 |

H1 TRUE (18/2/11). H2 TRUE (+16.8%, leaf). H3 FALSE as worded (holed 3 of 31 callers one level up, awaited +4).
H4 TRUE (28). H5 TRUE (covered 28, awaited 19).
The one-level test cannot see a hole two levels up, so it flatters the leaf rule. Next: a TRANSITIVE entry: a
function is holed when any plain function reaches it by a non-go call (VTA edges, an upper bound on holes; package
initializers run plain, so what they reach is holed by right).
- H6: leaf-fix1's transitive awaited falls well below its one-level 27 (predicted 10..20).
- H7: prop-fix1's transitive awaited stays at least 24 (its losses are init-reached sites).

## Scored, transitive entry; the factory rule corrected (2026-11-13 23:31 CST)

Transitive awaited (one-level in brackets): base 8 (13), fix1 11 (19), leaf 9 (17), leaf-fix1 13 (27), prop 10
(21), prop-fix1 14 (28). H6 TRUE (13, in 10..20). H7 FALSE (14, under 24). Diagnosed, not guessed: every one of
prop-fix1's 15 holed callers is reached from ONE plain root, a sync.OnceFunc / sync.OnceValue closure. That closure
calls a func value (so the caller-local rule twins it) but is CREATED in plain sync.OnceFunc, because fix 1's factory
test counted only values that park in the base set. Under the caller-local rule a closure that calls a func value is
itself can-park, so fix 1 must key on the rule's own set, to a fixpoint (flag -factories-arm). The tool also now
treats package initializers as plain roots (they run plain by section 8) and adds a STRICT awaited: every creator
of the value is also entered as a twin, transitively. The VTA graph over-approximates interface edges
(io.ReadAtLeast reaches every Reader), so every holed count is an UPPER bound.
- H8: prop-fix1a transitive awaited at least 25 of 31.
- H9: leaf-fix1a transitive awaited stays at most 18 (the leaf rule leaves callers plain).
- H10: strict awaited is within 3 of transitive awaited in every arm.

## Scored, nine arms (fix1a = factories over the rule's own set)

Transitive awaited: base 8, fix1 11, fix1a 11, leaf 9, leaf-fix1 13, leaf-fix1a 14, prop 10, prop-fix1 14,
prop-fix1a 14. H8 FALSE (14, not at least 25). H9 TRUE (14). H10 TRUE (strict 11 vs 14 at worst).
Diagnosed: the sync.OnceFunc root was only the walk's first find. In prop-fix1a every hole's root is a PACKAGE
INITIALIZER (crypto/ecdh.init$5, net.init$1), reaching the sites through VTA's over-approximated Reader edges
(drbg.ReadWithReader -> io.ReadFull -> (*crypto/tls.Conn).Read), which no initializer really calls. The tool now
reports the transitive count both with and without initializers as roots.
- H11: without initializer roots, prop-fix1a awaits at least 26 of 31; leaf-fix1a stays at most 20.

## Scored, no-initializer roots
H11 TRUE both ways: prop-fix1a awaits 28 of 31 with NO hole of the rule's own (all 31 callers entered as twins);
leaf-fix1a awaits 14 with 15 rule-own holes. Every hole left in prop-fix1a's with-initializer column is reached from
a package initializer, through VTA's over-approximated Reader edges.
