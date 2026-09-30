# The OSR opt-in's corpus edit (TRAIN K fixup)

**What changes.** `src/go2cs/csproj-template.xml` gains three lines in its executable-only group, after
`<EnableCompressionInSingleFile>`: a two-line note and the commented
`<!-- <TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops> -->`
(owner ruling 2026-09-30: opt-in). Every committed project the template produced must carry the same three lines,
or the next re-emit reads as drift.

**The set.** Every tracked `*.csproj` carrying the template's own comment "Enable native compiled output
optimizations", except `src/core/golib/golib.csproj`. That file is hand-owned: it carries the comment, has no
executable-only group, and the converter never writes it. At master `f819887fa3` the set is **1129** (773
behavioral, 341 `src/core`, 15 performance); the 228 test hosts come from `test-csproj-template.xml` and get
nothing.

**How K changes the count.** `optin.py` finds the set by the signature, not from a list, so it counts whatever the
union holds.
- Each behavioral or performance project a K seat adds is one more descendant.
- If the union's converter (which carries this template) emitted the project, it already has the lines and
  `insert` skips it.
- If a seat cut before this template emitted it, `insert` adds them.

## Order in the fixup

1. Run S1's csproj regeneration with the UNION converter. It re-emits from this template, so the three lines
   arrive by emission.
2. Run `python3 docs/phase4/recipes/quickjit-optin/optin.py insert .`. It is idempotent: after step 1 it should
   print `0 edited`, and any non-zero count names projects S1 did not reach.
3. Run `python3 docs/phase4/recipes/quickjit-optin/optin.py check .`. It must print `CHECK PASS`. The check
   requires:
   - every descendant carries the opt-in exactly once, inside the executable-only group;
   - no test host mentions it;
   - the hand-owned `golib.csproj` is untouched.

## Measured (C2, 2026-09-30, at `f819887fa3`)

- **`check` before `insert`:** FAIL, 1129 problems. The check sees every file.
- **`insert`:** 1129 files edited, each `+3/-0`, line endings kept (all CRLF). `check` then PASSes, and a second
  `insert` edits 0.
- **Byte-equality** for three sampled projects, each emitted into a seeded scratch root by master's converter,
  plus `insert`, and separately by this seat's converter: identical bytes for the library
  `unicode/utf8/unicode.utf8.csproj`, the behavioral `AliasImport.csproj` (with its sibling module
  `AliasImportLib`) and the performance `PerfSieve.csproj`.
  - Negative control: without `insert` the two emissions differ by exactly the 3 lines.
  - Each master emission equals its committed file (CR-stripped), so the samples carry no unrelated drift.
