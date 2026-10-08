# DATA: the face-lift docs seat's inventory (2026-10-07)

A point-in-time record. Measured at master `541766413e`, over every tracked `*.md` outside `docs/phase4` and
`docs/doctrine`. It counts mentions of the attributes the face lift takes out of converted code: `GoRecv`,
`GoType` (plain, and with an argument), `GoArrayDims`, `GoParamDims`, `GoTag`, `GoEmbedded`, `GoStr`.

## Method

One whole-word match per mention, per line. Each mention gets one kind, the first that applies:

| Kind | Rule |
|---|---|
| SAMPLE | inside a fenced code block |
| COMMENT | on a line inside an HTML comment (a visitor never sees it) |
| INLINE-SAMPLE | in a code span that holds converted code beside the attribute, such as a declaration |
| EXPLAINS | in a heading, or the attribute is the subject or object of an explaining verb (marks, records, driven by, ...) |
| PASSING | everything else |

EXPLAINS against PASSING is a pattern, not a reading, so the split between those two is approximate; their sum is
exact. Scopes: `docs-pages` is the living pages under `docs/`; `docs-history` is `docs/phase3`,
`docs/RoadmapHistory.md` and `docs/CleanupBacklog.md`; `plan` is `docs/PLAN-*.md`; `other` is markdown outside
`docs/` and `.claude/`. A fenced sample is "sourced" when a `<!-- source: ... -->` comment stands between it and
the heading or the C# sample before it.

The same count was taken twice, once from the blobs at the ref and once from a worktree at the ref: 741 mentions
both times, with the kinds of section 11 of the plan included (`GoLocalName`, `GoValueClone`, `GoChanDir`,
`GoMapKeyDims`, `GoDescriptorType`, `StackTraceHidden`, `MethodImpl`). Those kinds are 83 mentions in 14 living
pages and 10 in 4 rules and skills files, and are not in the tables below.

## Output

```
docs-history  EXPLAINS       5
docs-history  INLINE-SAMPLE  10
docs-history  PASSING        26
docs-pages    COMMENT        11
docs-pages    EXPLAINS       39
docs-pages    INLINE-SAMPLE  183
docs-pages    PASSING        60
docs-pages    SAMPLE         205
other         INLINE-SAMPLE  3
other         PASSING        6
plan          EXPLAINS       8
plan          INLINE-SAMPLE  15
plan          PASSING        32
plan          SAMPLE         7
rules         COMMENT        1
skills        COMMENT        9
skills        PASSING        3
TOTAL docs-history 41 mentions in 5 files
TOTAL docs-pages 498 mentions in 30 files
TOTAL other 9 mentions in 3 files
TOTAL plan 62 mentions in 1 files
TOTAL rules 1 mentions in 1 files
TOTAL skills 12 mentions in 4 files
--- csharp fences holding a leaving attribute:
  ('docs-pages', 'UNSOURCED') 44
  ('docs-pages', 'sourced:core') 7
  ('docs-pages', 'sourced:golden') 69
  ('plan', 'UNSOURCED') 7
--- by attribute (docs-pages + rules + skills)
  GoArrayDims   {'COMMENT': 2, 'EXPLAINS': 4, 'INLINE-SAMPLE': 6, 'PASSING': 8, 'SAMPLE': 6}
  GoEmbedded    {'EXPLAINS': 1, 'PASSING': 3, 'SAMPLE': 4}
  GoRecv        {'COMMENT': 10, 'EXPLAINS': 7, 'INLINE-SAMPLE': 60, 'PASSING': 7, 'SAMPLE': 33}
  GoStr         {'EXPLAINS': 2, 'INLINE-SAMPLE': 7, 'SAMPLE': 2}
  GoTag         {'INLINE-SAMPLE': 1, 'PASSING': 5, 'SAMPLE': 5}
  GoType        {'COMMENT': 3, 'EXPLAINS': 19, 'INLINE-SAMPLE': 70, 'PASSING': 20, 'SAMPLE': 101}
  GoType(arg)   {'COMMENT': 6, 'EXPLAINS': 6, 'INLINE-SAMPLE': 39, 'PASSING': 20, 'SAMPLE': 54}
--- per file
  docs-history docs/CleanupBacklog.md  9  {'INLINE-SAMPLE': 4, 'EXPLAINS': 2, 'PASSING': 3}
  docs-history docs/RoadmapHistory.md  4  {'EXPLAINS': 1, 'INLINE-SAMPLE': 2, 'PASSING': 1}
  docs-history docs/phase3/DESIGN-pointer-core-typeparam.md  2  {'PASSING': 2}
  docs-history docs/phase3/Phase3-Handoff.md  25  {'PASSING': 19, 'EXPLAINS': 2, 'INLINE-SAMPLE': 4}
  docs-history docs/phase3/Reflect-Census.md  1  {'PASSING': 1}
  docs-pages   docs/Architecture.md  4  {'INLINE-SAMPLE': 4}
  docs-pages   docs/ConversionStrategies-Reference.md  2  {'PASSING': 2}
  docs-pages   docs/ConversionStrategies-Reference/README.md  1  {'PASSING': 1}
  docs-pages   docs/ConversionStrategies-Reference/constants.md  6  {'INLINE-SAMPLE': 6}
  docs-pages   docs/ConversionStrategies-Reference/defer-panic-recover.md  2  {'PASSING': 1, 'INLINE-SAMPLE': 1}
  docs-pages   docs/ConversionStrategies-Reference/empty-interface.md  2  {'SAMPLE': 1, 'EXPLAINS': 1}
  docs-pages   docs/ConversionStrategies-Reference/expression-switch.md  2  {'INLINE-SAMPLE': 2}
  docs-pages   docs/ConversionStrategies-Reference/floating-point-formatting.md  1  {'INLINE-SAMPLE': 1}
  docs-pages   docs/ConversionStrategies-Reference/generic-constraints.md  13  {'INLINE-SAMPLE': 6, 'PASSING': 7}
  docs-pages   docs/ConversionStrategies-Reference/interfaces.md  34  {'EXPLAINS': 3, 'SAMPLE': 13, 'INLINE-SAMPLE': 12, 'PASSING': 6}
  docs-pages   docs/ConversionStrategies-Reference/labels-and-loop-variables.md  2  {'INLINE-SAMPLE': 2}
  docs-pages   docs/ConversionStrategies-Reference/manual-conversions.md  42  {'INLINE-SAMPLE': 13, 'PASSING': 11, 'SAMPLE': 16, 'EXPLAINS': 2}
  docs-pages   docs/ConversionStrategies-Reference/maps-and-channels.md  14  {'EXPLAINS': 1, 'INLINE-SAMPLE': 4, 'PASSING': 5, 'SAMPLE': 4}
  docs-pages   docs/ConversionStrategies-Reference/multi-assignment.md  2  {'SAMPLE': 1, 'INLINE-SAMPLE': 1}
  docs-pages   docs/ConversionStrategies-Reference/multi-result-and-comma-ok.md  2  {'INLINE-SAMPLE': 2}
  docs-pages   docs/ConversionStrategies-Reference/named-numeric-types.md  19  {'INLINE-SAMPLE': 14, 'SAMPLE': 3, 'PASSING': 1, 'EXPLAINS': 1}
  docs-pages   docs/ConversionStrategies-Reference/native-and-narrow-integers.md  2  {'INLINE-SAMPLE': 2}
  docs-pages   docs/ConversionStrategies-Reference/nil-and-zero-values.md  1  {'SAMPLE': 1}
  docs-pages   docs/ConversionStrategies-Reference/package-conversion.md  5  {'INLINE-SAMPLE': 3, 'SAMPLE': 1, 'PASSING': 1}
  docs-pages   docs/ConversionStrategies-Reference/pointers.md  17  {'INLINE-SAMPLE': 15, 'PASSING': 1, 'SAMPLE': 1}
  docs-pages   docs/ConversionStrategies-Reference/shadowing.md  7  {'INLINE-SAMPLE': 3, 'SAMPLE': 3, 'PASSING': 1}
  docs-pages   docs/ConversionStrategies-Reference/slices-and-arrays.md  14  {'EXPLAINS': 1, 'SAMPLE': 3, 'INLINE-SAMPLE': 6, 'PASSING': 4}
  docs-pages   docs/ConversionStrategies-Reference/source-generators.md  29  {'INLINE-SAMPLE': 24, 'PASSING': 5}
  docs-pages   docs/ConversionStrategies-Reference/strings.md  6  {'INLINE-SAMPLE': 3, 'SAMPLE': 2, 'EXPLAINS': 1}
  docs-pages   docs/ConversionStrategies-Reference/struct-embedding.md  18  {'INLINE-SAMPLE': 7, 'SAMPLE': 8, 'EXPLAINS': 2, 'PASSING': 1}
  docs-pages   docs/ConversionStrategies-Reference/struct-types.md  20  {'EXPLAINS': 1, 'SAMPLE': 13, 'INLINE-SAMPLE': 3, 'PASSING': 3}
  docs-pages   docs/ConversionStrategies-Reference/value-receiver-delegates.md  6  {'INLINE-SAMPLE': 5, 'EXPLAINS': 1}
  docs-pages   docs/ConversionStrategies-Reference/variable-initialization-order.md  1  {'INLINE-SAMPLE': 1}
  docs-pages   docs/ConversionStrategies.md  223  {'SAMPLE': 134, 'EXPLAINS': 25, 'INLINE-SAMPLE': 43, 'COMMENT': 11, 'PASSING': 10}
  docs-pages   docs/README.md  1  {'SAMPLE': 1}
  other        src/archived/Baseline-vs-FullConversion.md  1  {'INLINE-SAMPLE': 1}
  other        src/go2cs/ToDo.md  7  {'PASSING': 6, 'INLINE-SAMPLE': 1}
  other        src/tools/crosspkg-census/README.md  1  {'INLINE-SAMPLE': 1}
  plan         docs/PLAN-marker-comment-parity.md  62  {'PASSING': 32, 'INLINE-SAMPLE': 15, 'EXPLAINS': 8, 'SAMPLE': 7}
  rules        .claude/rules/converter.md  1  {'COMMENT': 1}
  skills       .claude/skills/corpus-reconvert/SKILL.md  1  {'COMMENT': 1}
  skills       .claude/skills/gate-forensics/SKILL.md  3  {'COMMENT': 3}
  skills       .claude/skills/train-assembly/SKILL.md  1  {'COMMENT': 1}
  skills       .claude/skills/validation-bank/SKILL.md  7  {'COMMENT': 4, 'PASSING': 3}
```
