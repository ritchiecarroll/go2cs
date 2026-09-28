# The position map — one record per converted file, carrying a whole position

> **STATUS: DESIGN — IMPLEMENTED AND MEASURED on lane `claude/position-map-arc` (2026-08-21); §3.1
> records the USER DESIGN DIRECTIVE (relayed via the mailbox the same day) that moved the records
> from the converted files into the package-info files before the merge.** The design is written first and committed first, so a
> veto costs rework rather than archaeology; §10 is this document's adversarial pass against its own
> first draft, and §11 lists what the mechanism forced that the ruling did not fix — written up
> rather than self-ruled.
>
> **Commissioned by the ruling** *"the position map is INDIVISIBLE and build-shape-faithful; the host
> never claims `testing/testing.go`"* (coordinator, 2026-08-21,
> [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md)), which fixed the
> CONTENT of both halves and left the MECHANISM to this design: *"its MECHANISM (per-file line table,
> frame-record side channel, or otherwise) is the arc's design to make, reviewed per charter §7
> before implementation."*
>
> **Companions:** `src/go2cs/positionMapOperations.go` (the emission),
> `src/core/golib/GoPositionMapAttribute.cs` (the record), `src/core/runtime/managed_impl.cs`
> (`goFramePosition`, the reader, beside `goFrameName`'s function half),
> `src/tests/Behavioral/RuntimeCallerFrames` (the guard the ruling queued strengthening, §9.3).

---

## 1. What the ruling fixed, and what it left here

The governing principle is **no fabricated positions**: a reported `file:line` must exist in the tree
the file names, and every identity a frame reports must be a conversion-time FACT rather than a
plausible composite. Three consequences the ruling fixed outright:

1. **Indivisible.** The file half alone mints `log/log_test.go:69` — Go's file, C#'s line, a position
   in neither tree. File and line ship together or not at all.
2. **The reported line is the GO source line**, derived from conversion-time facts.
3. **The identity is build-shape-faithful.** The stdlib reports the trimpath/import-path form; a
   `-recurse` user module reports what Go would have baked for the same build. `main/main.go` where
   Go answers `C:/…/main.go` is a divergence and does not land.

What was left open is the mechanism, and one thing the mechanism turns out to decide for itself: how
the identity is CARRIED. §5 and §11.1 are that.

---

## 2. The mechanism space, priced

Four shapes were considered. The two axes that decide it are **corpus-size cost** (the emitted tree
is committed, reviewed, and packed into NuGet) and **runtime cost** (paid by every program that reads
a frame, and by nobody else). Blast radius is the third, because a corpus-wide emission change costs
a regen and a golden re-baseline whichever shape wins.

| | corpus cost | runtime cost | other |
|:--|:--|:--|:--|
| **A. `#line` directives (PDB transport)** | **+28–47% lines** — one directive per statement | **zero** — the PDB answers directly | **disqualifying:** CS diagnostics relocate onto `.go` files that are not in the project, and the debugger steps into sources that may not exist on the machine. The repo's whole method is root-causing against the emitted `.cs`; this takes that away |
| **B. Side-car data file + `EmbeddedResource`** | one new data file per package, plus a `.csproj` item in every emitted project | one resource read per assembly | a new artifact TYPE, per-GOOS partitioning under layout L3, and an explicit exclusion rule for hand-owned files — three places for a rule to go wrong |
| **C. Per-package generated `.cs` of records** | one new `.cs` per package (~306 core + ~570 behavioral + the test hosts) | one reflection read per assembly | picked up by the existing `<Compile Include="*.cs" />`, so no csproj change — but same hand-own and L3 partitioning rules as B |
| **D. ⭐ One `[assembly: GoPositionMap]` per converted file, emitted INTO that file** | **+2 lines and one string per file** (measured: §4.2) | one reflection read per assembly, one lazy decode per file | **no new artifact, no csproj change, and the hand-own and L3 rules fall out for free** (§2.1). ⚠ Superseded on PLACEMENT by the user directive — the records moved to the info files (§3.1), which kept D's record shape and cost while trading the hand-own free rule for one explicit line |

**D wins on the third column, not the first two.** B and C are within noise of D on size, and all
three are equivalent at run time. What separates D is that two rules other shapes have to STATE, it
gets by construction:

### 2.1 The two rules D does not have to state

- **Hand-owned files.** A `[module: GoManualConversion]` whole-file hand-own is never re-emitted, so
  under D it simply carries no record — and reports its `.cs` position, which is the honest answer,
  since its C# was written rather than converted and no line of it corresponds to a line of Go. Under
  B or C the package-level artifact is regenerated wholesale and must be told to EXCLUDE the
  hand-owns, in a place that has nothing to do with them. That is exactly the shape of rule that
  rots: the marker census in `CLAUDE.md` moves in both directions (32 → 39 → 40 → 44 → 53) and a
  second place that has to track it is a second place that can fall behind.
- **Layout L3.** A package whose emitted C# varies by `GOOS` keeps the varying files in per-GOOS
  folders. Under D each variant carries its own record, because the record is IN the variant. Under
  B or C the package-level artifact has to be partitioned by target and merged by `platformEmit`.

### 2.2 What was NOT a candidate

A **frame-record side channel** — the runtime recording positions as it walks — cannot work, and it
is worth saying why so it is not re-proposed: the runtime has no access to the Go source, and the
mapping it would need is precisely the conversion-time fact this design records. The board's own
phrasing of the alternative ("frame-record side channel") is about where the data LIVES, and D is
that: the record is the side channel, it just lives in the file it describes.

---

## 3. The record

One per converted file, emitted into the package-info file of the compilation that compiles it
(§3.1 — the placement is a user directive; the record itself is unchanged from the shape priced
here):

```csharp
[assembly: go.GoPositionMap("log/log.go", "log.cs", "hYS4hLiEuIS4hLg…")]
```

- **`goFile`** — the Go source's identity, build-shape-faithful (§5).
- **`csFile`** — the emitted file name, which is the key the runtime matches a frame's PDB file name
  against. Recorded rather than derived from the Go stem: "the `.cs` stem IS the Go stem by
  construction" is true, and it is exactly the kind of true-by-construction derivation this arc is
  replacing with a fact.
- **`table`** — the C#-line → Go-line map (§4).

**Both halves in one record is the design's whole point.** Indivisibility is not enforced by a rule
that says "do not report one without the other"; there is no code path that could. A frame either
finds a record and reports the Go position it carries, or finds none and reports the converted `.cs`
position it always did.

`AttributeTargets.Assembly` rather than `Module`: `GoManualConversionAttribute`'s own header records
that module-scoped metadata serializes only one instance per assembly regardless of `AllowMultiple`,
which is fine for a marker and fatal for a per-file record.

### 3.1 Where the record LIVES — the user directive, and what it traded

As first implemented, each record was emitted at the top of its own converted file. **The user ruled
that placement against the project's primary objective** ("reads like Go; extra machinery hidden in
generated files") — a per-file base64 attribute is visible plumbing in exactly the surface the
project promises to keep clean, and `flag.cs`'s own file-top blob was the exhibit. The records now
land in a delimited expository block — `// <GoSourcePositionMaps>` … `// </GoSourcePositionMaps>`,
always emitted, one record per converted source file — in the package-info file of the COMPILATION
that compiles the mapped source: `package_info.cs` for production files, and the test-info anchors
(`package_test_info.cs` / `package_info_internal_test.cs`, per which anchor owns the file, exactly as
the `GoImplement` records route) for the `-tests` variants. The same-compilation rule is what keeps
the lookup working: an assembly attribute is visible only to its own assembly, and the tests assembly
compiles the test sources, so `log_test.go:69` stays resolvable inside the test host.

The relocation is semantics-free by construction — the record shape is unchanged, so indivisibility
is untouched, and the runtime cannot tell where an assembly attribute was declared — but it did
trade away two of §2.1's for-free properties, and honesty requires saying so:

- **The hand-own rule needed one explicit line.** In-file, a hand-own's `.cs.auto` review sibling
  carried its (meaningless) record harmlessly, because the sibling is never compiled. Centralized,
  that record would have landed in the COMPILED `package_info.cs` — a table mapping lines of a
  hand-written file that does not contain them, the exact fabrication the ruling forbids.
  `finalizePositionMap` now strips the sibling's sentinels and records nothing for a
  `manualConversion` visit.
- **Merge semantics became load-bearing.** The section follows `writePackageInfoFile`'s
  `mergeExisting` contract: whole-package conversions rebuild it, merging writes keep existing
  records for files this conversion did not re-emit. That is not politeness — the recompile-model
  test assembly excludes `package_info.cs` from its compile items in favor of the seeded
  `package_test_info.cs`, so the seed is the only route production records have into that assembly.

What it did NOT trade: layout L3 still works unchanged, because `package_info.cs` is itself a
closure-derived, per-GOOS-routed artifact — the records land in whichever variant of it the target's
emission owns. And the `stdlib-metadata` extractor was verified (not assumed) to ignore the new
block: `TestStdLibMetadataExtractIgnoresPositionMaps` pins it against a synthetic info file carrying
all three record families.

---

## 4. The line table

### 4.1 The encoding

Base64 over a delta stream, one record per mapped C# line, ordered by ascending C# line.

- A byte with the **high bit set** packs one record: bits 6–4 are `ΔcsLine - 1`, bits 3–0 are the
  zig-zag `ΔgoLine`. This is the step that dominates a converted file — the next C# line for the next
  Go line — and it costs **one byte**.
- A **`0x00`** byte introduces the extended form: unsigned varint `ΔcsLine - 1`, then unsigned varint
  zig-zag `ΔgoLine`.
- No other value below `0x80` is ever produced, so a decoder can REJECT a corrupt stream rather than
  read plausible line numbers out of it.

Deliberately not compressed. A deflate stream would be roughly four times smaller, and it would make
the emitted bytes depend on a compressor's implementation details — a Go toolchain hop could churn
every file in the corpus for no semantic change. The encoder and decoder are round-tripped under the
plain `go test ./...` (`positionMap_test.go`), including the backward `ΔgoLine` a hoisted declaration
produces and both wide-gap forms.

### 4.2 Measured cost

Whole-corpus, from a seeded `-stdlib -comments` reconvert of `src/core` (Go 1.23.1, windows target):

| | |
|:--|--:|
| files carrying a record | 1,339 |
| attribute text, total | 370,644 bytes (0.35 MB) |
| of which table payload | 269,692 bytes (0.26 MB) |
| corpus `.cs`, total | 38.73 MB |
| **attribute share of the corpus** | **0.91%** |
| table characters per emitted C# line | 0.294 |
| largest single table | 7,168 chars (`crypto/internal/nistec/fiat/p521_fiat64.cs`) |

Against option A's +28–47% of LINES, that is the whole argument. The largest table in the corpus is
one 7 KB line in a 5,540-line file.

### 4.3 Lookup is a predecessor search

The greatest recorded C# line **≤** the queried one wins, and its Go line is the answer — so a query
inside a multi-line emission answers the Go statement it was emitted FOR, rather than interpolating
into a line that statement never occupied. This is the same model Go's own `pclntab` uses: within a
function there are no holes, and any PC maps to some line of that function.

Below a file's first mapped construct there is no Go line, and the answer is the `.cs` position —
half a position is the one answer this design does not give.

---

## 5. The identity — build-shape-faithful, in three forms

Go bakes the source path at COMPILE time; go2cs bakes it at CONVERSION time, which is the same
decision point. `cmd/go` builds the standard library with `-trimpath`, so a GOROOT package's frames
name `runtime/debug/stack.go`; an ordinary user build bakes the absolute source path.

| the Go source is… | recorded as | why |
|:--|:--|:--|
| under `GOROOT/src` | GOROOT/src-relative (`runtime/debug/stack.go`) | exactly the `-trimpath` form `cmd/go` applies to std — the identity `runtime/debug`'s own `TestStack` asserts |
| **beside** its emitted `.cs` | the **bare file name** (`main.go`), rooted at run time against the `.cs` file's own compile-time directory | §5.1 |
| anywhere else (`-recurse` into a separate output root) | the absolute conversion-time path, forward-slashed | exactly what Go bakes for an ordinary untrimmed build |

The three are distinguishable without a discriminator field, and deliberately so: the GOROOT form
always carries a separator (it is `<import path>/<stem>.go`), the beside-the-C# form never does, and
the absolute form is rooted.

> **Amendment 2026-09-28 (C2, C5 SOURCE PATHS, re-cut after COORD's review of `ff6b555e00`).** The
> premise that `cmd/go` builds std with `-trimpath` is false by measurement at go1.24.13: default
> `go test` and `go build` bake the ABSOLUTE `GOROOT/src/...` path into std frames, and only a
> `-trimpath` build answers the relative form. `TestStack` does not assert the relative form either:
> it re-runs itself with `GOROOT=` and requires each frame to start with the child's
> `runtime.GOROOT()` + `/src/`, which is empty (so the relative form) only when no link-time root
> exists. The RECORD is unchanged: std is still recorded GOROOT/src-relative, since no host path may
> go into a committed artifact (§5.1). What changes is the run-time READ: the runtime roots a
> GOROOT-relative record against the LINK-TIME root, `runtime.defaultGOROOT`, which the `-tests`
> pipeline hands the host through its own variable, `GO2CS_DEFAULT_GOROOT`, copied once at start-up
> (`goenvs_impl.cs`). It never roots against the ambient `GOROOT`: Go's frames do not move with it,
> and it can name another Go install whose files do not carry the recorded lines, which would be a
> position in neither tree. With no link-time root (a program run outside the pipeline), the recorded
> form is answered, which is Go's `-trimpath` form. The first cut rooted against `runtime.GOROOT()`
> and brought back four `TestStack` assertion failures in the banked `runtime/debug` row.

The **`GOROOT/src` test is the discriminator, not the conversion MODE.** That matters: the `-tests`
pipeline converts GOROOT packages through a single-package run, not `-stdlib`, and it is the run that
produces `log`, `flag` and `runtime/debug`. A mode-keyed rule would give those three absolute GOROOT
paths and fail `TestStack` outright.

### 5.1 Why the beside-the-C# form is FORCED, not preferred

An absolute path baked into a COMMITTED artifact names a directory that does not exist on the next
clone. Two consequences, and the second is disqualifying on its own:

1. It is a **fabricated position** on every machine but the converting one — the precise thing the
   ruling's governing principle forbids.
2. Every behavioral and performance golden would carry this machine's path. `check-no-regression`
   would report the entire behavioral corpus as drifted in every other clone and every sibling
   worktree, on the first run, before any work started.

Recording the bare name instead keeps the artifact machine-independent while letting the runtime root
it against a path it can prove — the `.cs` file's own compile-time directory, straight out of the
PDB. The result is the rooted, absolute path Go answers, naming a file that genuinely exists, derived
from **two recorded facts** rather than composed from a namespace. §9.3 measures it against a Go
control: same tail, same rootedness, same line numbers.

### 5.2 The `main/main.go` regression is structurally impossible

Not avoided — impossible. There is no code path, in the converter or the runtime, that composes a
file name from a package name, a namespace or a class name. The runtime reads a recorded string and,
in one case, prefixes it with a directory the PDB gave it. `positionMap_test.go` pins this directly.

---

## 6. The line half's mechanism — sentinels, and the binding rule

### 6.1 Why a sentinel and not a line count

The emitted line a statement lands on is **not knowable while the statement is being emitted**:
`visitBlockStmt` swaps in a fresh builder for a nested block and splices it back later, hoisted
declarations are spliced AHEAD of the statement that produced them, and the using directives and type
aliases are markers resolved only once the whole file has been visited. So the walk writes an
invisible sentinel carrying the GO line into the text itself, where every one of those movements
carries it along; `finalizePositionMap` reads the finished text once — the only point at which C#
line numbers exist at all — and strips them.

NUL is the sentinel delimiter because it is the one byte that cannot occur in emitted C#: the Go
compiler disallows it in source text, so a sentinel can never collide with converted content, and a
sentinel that somehow survived stripping is a hard compile error rather than silent corruption.

Sentinels are written at the two places that have a Go position and can hold a frame: every statement
(`visitStmt`) and every function declaration (`visitFuncDecl`).

### 6.2 The binding rule, and the off-by-one that made it necessary

**Which line a sentinel marks is not the line it sits on.** A statement is emitted as *newline,
indent, text*, so the sentinel written immediately before that emission lands at the END of the
PRECEDING line — a line its own construct has not started yet.

The rule that reads this correctly is positional rather than syntactic:

> A sentinel with content still to come on its own line marks THAT line; a sentinel with nothing
> after it marks the line that follows. The FIRST binding of an emitted line wins, and bindings only
> ever advance.

Measured before the rule existed, on `RuntimeCallerFrames`: every statement bound one construct too
early — a function's first statement was swallowed by its signature line, and every later statement
inherited its SUCCESSOR's Go line. The table looked entirely plausible, which is the point of
recording it here.

"First binding wins" is what makes a `for` header report the `for` statement rather than its own init
clause — the init clause is a statement emitted mid-header, so both resolve to the same emitted line,
and the outer one is earlier in the text. That is the frame Go reports.

### 6.3 The sentinel is text-neutral to CONSUMERS, not to the converter's own reads

A sentinel costs nothing to any consumer of the finished file. The converter itself, though, inspects
emitted text in places — a captured block is read back as a string and rewritten before it lands —
and those reads see it. Measured on the first whole-corpus reconvert: `convFuncLit` decides whether a
single-return literal collapses to an expression-bodied lambda by testing that nothing but the
block's opening brace precedes the `return`, and an un-stripped sentinel made that test fail for
**every such literal in the corpus — 110 files emitted a block body instead**. Nothing was wrong with
the map; the text simply was not neutral.

The rule is therefore explicit: a site that INSPECTS or REWRITES captured block text reads it through
`stripPositionSentinels`; a site that merely APPENDS it must not, or the position is lost. The
standing guard is the corpus itself — a seeded reconvert must be byte-identical to the committed tree
except for each file's own attribute line (§9.1), and the behavioral goldens pin the collapsed form
directly — so a future non-neutral site surfaces as drift rather than as a wrong line number.

A collapsed body drops its sentinels rather than relocating them, which is correct: it has no emitted
line of its own, and the enclosing statement's line already carries that statement's position.

### 6.4 A MERGE of a mapped file invalidates its map — re-emission is owed, not optional

The map is a DERIVED artifact of the emitted text, so anything that changes that text without
re-deriving the map leaves a plausible-but-wrong one — the exact class the ruling forbids, and silent,
because no gate reads a line number against its source.

A textual merge is such a thing. The `claude/union-157` merge did NOT trigger it, and why not is
worth recording: the one file it changes substantially, `src/core/sync/atomic/type.cs` (+69/−20), is
a whole-file `[module: GoManualConversion]` hand-own and therefore carries no record to invalidate —
§2.1's rule holding at the moment it would have mattered. What that merge leaves instead is the
complementary state: converted files arriving from a side that predates this change are UNMAPPED
rather than mis-mapped, which re-emission also fixes.

**The rule: a merge that changes any converted `.cs` owes a re-emission of that file's package before
the gates.** It is cheap — a filtered `-stdlib`, or the package's own `-tests -test-action all`,
which re-emits and re-validates in one step — and it is mechanical, because the affected set is
exactly `git diff --name-only <base> <theirs> -- 'src/core/**/*.cs'` minus the files that carry no
record anyway (golib, `*_impl.cs` hand-owns, `package_info.cs`/`package_init.cs`).

Same shape as the standing "never convert twice into one root" rule: not a diagnosis to re-derive
each time, a step in the ritual.

### 6.5 The one coupling this leaves

`rewriteDeferredMarkers` rewrites `«DYNTYPE:…»` / `«ADAPTER:…»` markers AFTER the file is written, so
it is the one rewrite the map cannot see. Its replacements are type NAMES and cannot span lines, and
the map's line numbers are valid only while that stays true. Rather than rely on that argument
holding, the pass now WARNS when a resolution changes a file's line count — loud, and pointing at
this design.

---

## 7. What the runtime does

`goFramePosition` is a single funnel both consumers read (`appendGoFrames` for tracebacks,
`internCallerFrame` for `Callers`/`Caller`/`CallersFrames`), so a traceback and a `runtime.Caller` on
the same frame cannot disagree about where it is. It:

1. takes the frame's `.cs` path and line from the PDB, as before;
2. reads the frame's assembly's records once, cached, keyed by emitted file name;
3. decodes that file's table lazily, on first use;
4. answers the recorded Go file and the predecessor-searched Go line — or, in every case where any of
   those is absent, the `.cs` position unchanged.

Only a program that actually inspects frames pays for any of it, and it pays once per assembly.
Reflection failure and an undecodable table both answer "no records", because a traceback is
diagnostic output and must never be the thing that takes a program down.

---

## 8. Consequences worth stating

- **The two suffix rules retire from the FILE half.** The lane's measured rules — strip
  `_test`/`_internal_test` from the class name to reach the package-under-test's directory — existed
  to DERIVE a file path from a namespace. With the path recorded there is nothing to derive:
  `runtime/debug/stack_test.go` is what the converter saw. They remain necessary and unchanged for
  the FUNCTION half (`goFrameName`), where Go genuinely does keep `_test`
  (`runtime/debug_test.T.method`). §11.1 flags this as a departure from the ruling's phrasing.

  > **Amendment, 2026-08-26 (lane `claude/slog-callerinfo`).** "Remain necessary and unchanged for
  > the FUNCTION half" was true as a REQUIREMENT and false as a description of the code: only the
  > `_test` half was ever satisfied there, and it was satisfied by accident — `goFrameName` derives
  > the import path straight off the class name, so an external suite's `<pkg>_test_package` came
  > out right without any rule, and an internal suite's `<pkg>_internal_test_package` came out
  > WRONG for the same reason. The sentence above reads as if both were handled because both are
  > phrased as one pair of rules; they are not a pair. Go strips nothing and keeps nothing — it
  > compiles an internal test file INTO the package under test (so the frame is
  > `callerprobe.TestInternalCallerName`, no suffix) and leaves an external one a separate package
  > (`callerprobe_test.TestExternalCallerName`), measured against go1.23.12. The internal-test
  > strip landed in `goFrameName` on this date; the effect was systemic rather than
  > `log/slog`-specific — any converted suite reading caller info reported
  > `<import path>_internal_test.<func>` — and it is now pinned in all three emitted shapes by
  > `GolibTests/CallerFrameTestVariantNamingTests`. The design's POSITION is unchanged and was
  > right; what this amendment records is that a stated-as-existing rule was never landed, which is
  > exactly the class of gap a design document can create by describing intent in the present
  > tense.
- **`goImportPath` stays** — the function half still derives the import path at run time, and that is
  correct: a function name IS a property of the package, and Go's own traceback spells it from the
  package, not from the file.

  > **Amendment, 2026-08-29 (lane `claude/func-literal-ordinals`, coordinator-ruled).** One piece
  > of the function half moves from derivation to the record: the FUNCTION-LITERAL counter. Go
  > names a literal `Outer.funcN` — a per-enclosing-function counter over that function's direct
  > literals, source order, from 1 — and a nested literal appends its own per-parent counter
  > (`Outer.funcN.M`; measured against go1.23.12 with inlining disabled, since gc renames a
  > closure whose enclosing function is inlined and go2cs performs no inlining). The runtime had
  > derived the ordinal from Roslyn's `<Outer>b__X_Y`, whose closure-group numbering matches Go's
  > counter only by coincidence — two sibling literals answered `func0`/`func1` for Go's
  > `func1`/`func2`, and nesting was unrepresentable. The remedy is this design's own: the record
  > gains an optional fourth argument, `funcLits` — `<startLine>-<endLine>:<suffix>` per literal,
  > semicolon-joined, in GO line space — collected from the AST at declaration-visit time
  > (`collectFuncLitNames`), and `goFrameName` maps a literal frame's C# line through the SAME
  > record's line table to its Go line, then answers the innermost recorded span containing it.
  > Both halves of the answer are conversion-time facts out of one record; a three-argument record
  > is unchanged, and a frame with no record (older artifact, hand-written lambda, no PDB, or a
  > package-level initializer literal — cmd/compile numbers those with a package-global `glob..`
  > counter that is a compile-schedule fact, so they are deliberately unrecorded) keeps the
  > derived fallback exactly as before. The import path itself stays derived — this amendment
  > moves only the counter. Guarded three ways: `positionMap_test.go` pins the collection against
  > Go's naming, `GolibTests/CallerFrameTestVariantNamingTests` pins the recorded, nested and
  > fallback shapes red-first, and the `FuncLiteralCallerNames` behavioral guard compares fourteen
  > frame names against `go run` (measured red before the fix: every literal off-by-origin, the
  > exact defect net/http's TestWriteHeaderNoCodeCheck and TestTimeoutHandlerSuperfluousLogs read).
  > Known edge, stated rather than guarded: line granularity cannot split two literals sharing one
  > Go line, and a lambda inside a converted METHOD still lacks Go's receiver qualifier in its
  > prefix (`main.m.func1` where Go spells `main.T.m.func1`) — the receiver half is a separate,
  > out-of-scope gap.
- **Whole-file hand-owns report their `.cs` position.** `crypto/subtle/xor_generic.cs` will not name
  `crypto/subtle/xor_generic.go`. Under a file-half-only design it would have; under indivisibility
  it must not, because there is no line of that C# that corresponds to a line of that Go.
- **The hand-owned test host reports `.cs`**, which is the ruling's point 2 restated in mechanism: it
  is not that the host declines to claim `testing/testing.go`, it is that no conversion ever recorded
  a position for it.

---

## 9. Measurements

### 9.1 The reconvert is byte-identical except for each file's own record

§4.2 carries the size. The correctness half of the same reconvert: with the hand-own clobber gate at
**0 violations**, no sentinel and no marker leaked into emitted source, no file carrying two records,
and every recorded identity in the GOROOT-relative form (1,339 of 1,339 — the corpus is std, and std
is what `cmd/go` trimpaths).

**Per-GOOS coverage is target-scoped, and stated rather than glossed:** a windows-target reconvert
records maps for the flat files and the `windows/` variants; the `linux/` and `darwin/` variants keep
whatever they had. They are not re-emitted, so they carry no record and their frames report the `.cs`
position — honest under indivisibility, and levelled by the next multi-platform emission, exactly as
any other L3 emission change is.

### 9.2 The table is exact

Decoded from the emitted attribute and printed against both sources, every mapped construct in
`RuntimeCallerFrames/main.cs` names its true Go statement — 109 of 109, including one-line function
bodies (where the signature and the body correctly share one Go line) and multi-line emissions.

### 9.3 The guard, strengthened, against a Go control

`RuntimeCallerFrames` **passed all four phases** with the file-half change in, because its five
file-related assertions were separator booleans and an equality — every one invariant under a
wholesale change of what the file names. It now asserts the property: the last two path segments, the
rootedness, and two line numbers, each printed as a VALUE so the stdout comparison against `go run .`
is the assertion and no constant in the guard has to be kept in step with the source it names.

```
                                   go run .                     converted C#
  caller file tail:                RuntimeCallerFrames/main.go  RuntimeCallerFrames/main.go
  caller file rooted:              true                         true
  caller line:                     27                           27
  caller line two frames up:       107                          107
  traceback names a go file:       true                         true
```

Output phase: **1 compared, 0 failed.** The same five lines under the file-half-only change would
have read `main/main.go` and `false`.

---

### 9.4 All three identity forms, measured

| conversion | recorded identity | reported at run time | Go's answer |
|:--|:--|:--|:--|
| `-stdlib` / `-tests` over GOROOT | `runtime/debug/stack.go` | same | same — `cmd/go` trimpaths std |
| single-package, source beside the `.cs` | `main.go` | `C:/…/RuntimeCallerFrames/main.go` | the same absolute path |
| `-recurse` into a separate output root | `C:/…/mod/app/main.go` | same | the same absolute path |

The third row is a probe module converted into its own output root: Go prints
`C:/…/mod/app/main.go 9` for its `runtime.Caller(0)`, the recorded identity is that same absolute
path, and the decoded table maps the emitted C# line to Go line 9.

---

## 10. Adversarial review (charter §7)

This section is the pass against this document's own first draft. Each item is an attempt to REFUTE
the design, with what the attempt found.

**10.1 "The PDB line is not always a statement start, so the predecessor search will drift."**
Partly true and harmless. C# emits sequence points per statement, but a lambda or a conditional
expression can carry extra ones inside a multi-line statement. A predecessor search maps those to the
nearest preceding recorded construct — which is the Go statement they belong to. The failure mode
this would need is a sequence point BEFORE the statement's own line, which the compiler does not
produce.

**10.2 "A frame in generated plumbing will answer a neighbouring statement's Go line."**
True, and it is the same answer Go gives. A `GoFrame` declaration, a hoisted capture, a `finally`
that runs the defer list — none has a Go line of its own, and the predecessor search attributes them
to the construct they were emitted for. Go's own line table has no holes inside a function either.
The alternative — explicit end-of-region markers, so such a line answers "unknown" — buys a more
honest answer for lines no consumer reads, at the cost of doubling the table.

**10.3 "The map is only as good as the PDB, so Release and AOT lose it."**
True, and unchanged from today: `frame.GetFileName()` already returns nothing without a PDB, and a
frame with no `.cs` path has never had a position. The map degrades to exactly the current behaviour
rather than to a wrong one.

**10.4 "Base names are not unique within an assembly, so the runtime key is unsound."**
Checked. One converted package is one assembly, Go forbids duplicate file names in a directory, and
layout L3 compiles exactly one per-GOOS variant. The `-tests` host is a SEPARATE assembly
(`<pkg>.tests`, `InternalsVisibleTo`), so a package's production and test files never share one
record set. A collision would be a last-write-wins overwrite, not corruption — but there is no
construction that produces one.

**10.5 "Recording the identity per file makes the record disagree with `goFrameName`'s package."**
This is the concern that made the lane EXTRACT `goImportPath` in the first place — two derivations
that could disagree while each looked right alone. Recording removes one of the two derivations
entirely, so they cannot disagree; what remains is a recorded file and a derived function, which is
exactly Go's own pairing (an external test's file is `runtime/debug/stack_test.go` and its function is
`runtime/debug_test.T.method` — they name different things ON PURPOSE, and the lane measured that).

**10.6 "The corpus cost is real and recurring: every converter change that shifts a line rewrites
every table."** True, and unavoidable in any shape that records positions — including `#line`. The
mitigation is that the tables are DERIVED, so a rebank regenerates them wholesale and no one ever
hand-edits one. The review cost is one opaque line per file; the review BENEFIT is that the line sits
in the file it describes, so a diff that touches a file touches its map and nothing else.

**10.7 "A behavioral golden now contains a Base64 blob, so a golden mismatch is unreadable."**
True and mitigated: the mismatch NAMES the file, and the decoder is a dozen lines
(`positionMap_test.go` carries one). A skewed table is also loud rather than silent — every line in
the file moves at once, not one.

**10.8 "The beside-the-C# form lets the runtime compose a path, which is the thing the ruling
forbids."** The sharpest objection, and it is about what "composed" means. The ruling forbids a
position *fabricated* from a plausible source — a path invented from a package name, naming a file
that need not exist. Here the runtime joins two RECORDED facts: a directory the compiler wrote into
the PDB, and a file name the converter read off disk. The result names a file that exists, on the
machine that built it, exactly as Go's baked absolute path does. If the coordinator reads the ruling
more strictly than that, §11.2 is the alternative and its price.

**10.9 "`-recurse` is untested."** It was when this section was first written, and the objection is
kept because it is what sent the lane to measure it rather than argue it. A `-recurse` conversion of a
probe module into a separate output root now records the absolute conversion-time path and maps its
lines exactly (§9.4).

**10.10 "An in-text sentinel changes the converter's own behaviour, so the emission is no longer the
emission."** The most damaging objection, and it was TRUE on the first whole-corpus reconvert: 110
files emitted a block body where they had emitted an expression-bodied lambda, because the collapse
test reads the block back as a string (§6.3). The mechanism survives it — the fix is that inspecting
sites read through `stripPositionSentinels` while appending sites do not — but the objection is the
reason the corpus-wide byte-identity check in §9.1 is a GATE and not a formality. The measurement
found this; no amount of reading the code would have.

---

## 11. What the mechanism forced that the ruling did not fix

Written up rather than self-ruled, per the lane's charge.

### 11.1 The stdlib identity is RECORDED, not derived — a departure from the ruling's phrasing

The ruling says *"stdlib → trimpath form via your `goFrameName` derivation + the two suffix rules"*.
This design records it instead, for three reasons: the record already has to exist for the line half,
so the derivation buys nothing; recording is strictly closer to the ruling's own principle (*"a
conversion-time FACT, never a plausible composite"*); and it retires the two suffix rules from the
file half, which is two fewer things to be right about (§8).

The lane implemented the recorded form. **Reverting to the derivation for GOROOT sources is a
localized change** — drop the GOROOT branch of `goSourceIdentity`, restore the suffix rules in
`managed_impl.cs` — at the cost of reintroducing a derivation the file half no longer needs, and of
the file and the line then coming from different places for stdlib frames while coming from one
record everywhere else.

### 11.2 If "no composition at run time" is read strictly

§10.8's objection, priced. The alternative to the beside-the-C# form is to record NOTHING for a
non-GOROOT source that ships as committed C# — behavioral tests included — so those frames report
their `.cs` position. That is unambiguously honest and it costs: the arc's own guard could no longer
assert Go agreement (it would be asserting `.cs`), converted user programs converted in place would
keep diverging from Go, and the arc would deliver the stdlib half only. Baking the absolute path is
NOT an available third option (§5.1).

### 11.3 What this design does not touch

`FuncForPC` and `Frame.Func` stay nil, `getcallersp` stays a stub, and the `+0x<offset>` PC deltas
stay omitted — all unchanged, and all for the reasons already recorded in `managed_impl.cs`.
