# COORD verification: C2's O1 survival census (2026-09-24)

**What was verified.** The first step of the owner's sstring-first ruling:
- C2's `claude/c2-literal-cache-draft` `3b53fec4f2`, probe `docs/phase4/probes/c2-o1-survival` (RESULTS.md is the record);
- C2's announcement, `claude/mailbox` `docs/phase4/inbox/COORD/20260924T172333Z-C2.md`.

**Method.** Three verifier dimensions (reproduce, predicates, decision framing), two adversarial refuters per finding,
and a completeness critic. The windows numbers were REPRODUCED on the i7 by running C2's own probe against a fresh
`go build -gcflags=-m std` (go1.24.13, CGO off). The probe's controls were made to fail once and then restored.

## Verdict

**The census reproduces.** On windows, every headline number matches byte-for-byte:

| | Sites | Format position |
|:--|--:|--:|
| Population | 4,835 | 2,491 (non-format 2,344) |
| S0, today | 476 | 81 |
| S1, sstring gaps closed | 528 | |
| S2, with value-site adapters | 2,766 = 57% | 2,233 = 90% of format |

The 2,069-site failure split matches row for row, as do the fmt entry counts (Errorf 920, Sprintf 446, Fprintf 286,
Printf 39, Appendf 2, Sscanf 1 = 1,694) and the 8 value sites. The residual (about 19,900 stored, returned or composite
literal occurrences) also matches.

Three corrections change what the pilot is:

1. **The flip list does not compile as published.**
   - `fmt.(pp).catchPanic #2` is on it, but its only calls are lowered into golib's generic defer. An `sstring`
     parameter cannot ride that, so the result is CS0306 at `print.cs:813`.
   - The census has no class for "callee of a defer/go statement". Add one.
   - The list becomes 32 on windows and S2 becomes 2,762, with no count loss: those 4 literals were already hoisted.
2. **Sites are not allocations.** Tier C already hoists most non-format literal arguments to static fields, which
   allocate once per process rather than per evaluation. The per-evaluation value of O1 is about **2,233 format sites
   plus about 230 inline non-format sites**, not 2,766.
   - The `runtime.throw` lever (726 sites) is worth **about zero** per evaluation: those are fatal paths. Its only gain
     is start-up, and a Tier C hoist exclusion gets that more cheaply.
   - The `bytealg` lever needs BOTH a non-generic `IndexRabinKarp` form AND the bodiless `IndexString` overloads. It
     also flips exported strings/bytes parameters.
3. **A cheaper public shape exists: the sstring TWIN.**
   - Keep the Go-shaped `@string` member, and add an `sstring` overload marked `[OverloadResolutionPriority(1)]`. The
     repo already uses that attribute in go2cs-gen's StructTypeTemplate.
   - Direct calls bind the twin, so literals go zero-copy.
   - Typed delegate conversions (`Funcꓸꓸꓸ<@string, any, @string> = fmt.Sprintf`) and reflection keep binding the
     `@string` member.
   - The result: NO adapter lambdas; NO function-identity change (with adapters, `reflect.ValueOf(fmt.Sprintf).Pointer()`
     and `%p` of a func would differ between value sites, where Go's are equal); NO cross-package "which parameters
     flipped" contract for consumers to learn; call sites byte-identical.
   - UNVERIFIED: whether Roslyn's method-group conversion honours the priority over an incompatible sstring overload.
     One compile probe settles it.

## Findings for the census record (C2)

- **Determinism.** The cascade-root ATTRIBUTION is nondeterministic (map order in `fixedPoint` and in the root key).
  Over 300 re-runs, 248 / 97 / 66 and the 522 total are stable, while the per-root splits (68 / 4, 45 / 21) move.
  Iterate in sorted order and sort the reasons in the key.
- **The hand-owned predicate over-matches.** It marks every function of a converted file that contains a per-function
  placeholder: 18 parameters and 133 sites; S2 rises about 75 when corrected. `throw` is blocked at function level,
  not by a whole-file hand-own.
- **The CGO axis.** The linux column was produced with cgo ON, and windows/darwin with cgo OFF. With CGO_ENABLED=0,
  linux reads a population of 4,797 and S2 of 2,755. Pin CGO in the README command, or state it per GOOS.
- **Value sites.** "The functions of 4 of those parameters have a production value site" reproduces as 2. The 26
  value sites count strings.EqualFold's 5 twice.
- **The announcement's sum.** "Errorf 920, Sprintf 446, Fprintf 286, Printf 39 = 1,694": those four sum to 1,691. The
  record, which includes Appendf and Sscanf, is right.
- **Minor.** The probe's `rel()` prints `<outside GOROOT>` on a Windows host (display only). The residual table omits
  42 literal arguments bound to non-string parameters. Tuple literals from parallel assignment are not modelled (a
  possible under-count; a three-line compile probe settles it).
- **Rank levers by per-evaluation value, not by sites.** The order is: the fmt format chain (with the utf8 pair) far
  ahead; then bytealg (both pieces); then Builder/Buffer.WriteString (it needs a proof that io.StringWriter goes
  through static adapters); everything else is about zero.

## What the pilot is (ruled by COORD within the owner's rulings)

The owner has ruled that sstring-first is primary and that exported signatures may flip. The pilot therefore
proceeds, in this order:

1. **(C2, read-only, cheap)** The census fixes above: the defer/go class, deterministic attribution, the
   function-level hand-own predicate, the pinned CGO axis, and per-evaluation (inline-today) columns. Then re-derive
   the pilot list.
2. **(A compile probe of about 50 lines, .NET)**
   - the implicit `u8 -> sstring` operator (sstring.cs:185 explicit -> implicit) with §8.2R.2's negative tests
     (CS0121/CS0034);
   - twin-plus-priority binding for u8, `@string`, C# `string` and `sstring` arguments;
   - method-group -> `Funcꓸꓸꓸ<@string,…>` binding the `@string` member, and the two bare natural-type method groups;
   - go2cs-gen's RecvGenerator accepting an sstring parameter on `[GoRecv]` methods.
3. **(The pilot seat)** A converter rule driven by an EXPLICIT approved list, not a mechanical "every noescape
   exported parameter". Its SHAPE: the TWIN if the probe binds; otherwise the owner-approved flip with converter-emitted
   adapters at value sites (plus published cross-package records, the GoRefPrimary pattern). Its member-row prediction:
   fmt TestCountMallocs `Sprintf("xxx")` 2 -> 1, since the callee-side flip reaches the test-assembly call site.
   log TestDiscard is NOT reached, because log.Printf captures `format` in a closure.

## Provenance

- The workflow run was `wf_65670c78-5d6` (48 agents). The per-agent outputs, including the defer scanner and the
  300-run determinism check, are in COORD's session scratch. This file is the record.
