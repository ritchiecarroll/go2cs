# DESIGN — retiring per-evaluation `@string` literal allocation (Tiers A / A′ / B / C)

> **Status: COMPLETE — ALL TIERS LANDED, 2026-07-26 (rev 4).** Tiers A / A′ / B landed 2026-07-25 as
> three independently-gated commits (`b5164da58` golib operators + both slice-route closures;
> `fecf02e5f` TypeGenerator span operators; `b99cf4419` combined rendering + concat decoupling),
> preceded by the §4.8 quiet-machine baseline (`2cde35fac`). The Tier C session then landed the
> queued §3 opener (`e88443b23` — typed composite string-literal elements and any-ELEMENT composites
> render `u8`) followed by **Tier C itself**: the whole-package hoisting pre-pass, per §4.1–§4.6, with
> the as-built record in §4.9 (`08680a751`, plus `7f485f167` for the `-tests` init-order arm the
> operational sweep exposed). Measured end to end: `PerfStringMatch` **11.75× → 9.19× JIT** and
> **12.18× → 8.76× Native AOT**, with StringView flat and String/Map non-regressing (§4.9).
> Underlying approval unchanged: rev 2 accepted by the user with **all five §6 decisions as
> recommended**, implementation order A → A′ → B → C, each independently revertible (§4.7).
> History: rev 1 went through a three-lens adversarial panel (semantics / converter mechanics /
> scope+fidelity; all three verdicts **sound-with-fixes**); every confirmed finding is integrated
> and the four blockers are resolved by design changes (§7 records them). Scope blessed by the user
> ("the always-allocating `@string` is THE performance bottleneck in the system"; hoisting proposed
> by the user with the "close to usage" placement requirement). Evidence: the `u8bench` experiment
> (scratchpad; `Mockup.cs` compiles the before/after forms against live golib) and the committed
> `PerfStringMatch` benchmark.

## 1. Problem

Go string literals live in RODATA: `return "true"`, `s == "true"`, `HasPrefix(line, "//go:build")`
allocate **nothing** in a Go binary. The converted C# pays a heap allocation — and usually a
UTF-16→UTF-8 transcode — at **every evaluation** of the same sites, because each literal→`@string`
conversion materializes a fresh backing `byte[]`:

| Site shape (real corpus code) | Today | Cost per evaluation |
|:--|:--|:--|
| `exprᴛ1 == "true"u8` (strconv `ParseBool`, 12 operands) | span → `@string.ToArray()` per operand | 408 B / call |
| `return "true"u8` (strconv `FormatBool`) | span → `ToArray()` | 32 B / call |
| `HasPrefix(line, "//go:build"u8)` (go/build scan loop) | span → `ToArray()` | 40 B / line scanned |
| `Ꮡt.Fatal((@string)"…")` (any-target diagnostics) | UTF-16 string → `Encoding.UTF8.GetBytes` + box | 88 B / call |
| `new StructuralError("empty integer")` (named string types, 86 sites) | UTF-16 string → `GetBytes` | worst form — no u8, no cast |

Measured (u8bench, Release, net9.0, best-of-5 × 10M):

| Shape | current | fixed | speedup |
|:--|:--:|:--:|:--:|
| ParseBool("False") — 12 literal compares, **real span operator** | 36.8 ns, 408 B | 7.5 ns, 0 B | **4.9×** |
| FormatBool value return (hoisted) | 7.0 ns, 32 B | 5.3 ns, 0 B | 1.3× |
| t.Fatal any-target (hoisted, pre-boxed) | 20.8 ns, 88 B | 6.5 ns, 0 B | **3.2×** |
| 16-line HasPrefix scan (hoisted prefix) | 185 ns, 640 B | 158 ns, 0 B | 1.2× + GC pressure |
| `(@string)""u8` (empty literal) | — | 3.5 ns, **0 B already** | excluded from hoisting |

Corpus instrument: **`PerfStringMatch`** (committed `d2af4a59c`) exercises all of these shapes in one
hot loop — pre-arc indicative gap **Go 147 ms vs C# JIT 1.69 s (~11×, worst ratio in the suite)**.

The end state: **a literal costs at most one allocation per program run** (Tier C), comparisons cost
zero ever (Tiers A/A′) — Go's own cost model, restored.

### Safety preconditions (verified, panel-hardened)

Sharing one `@string` across evaluations is sound because nothing may mutate its backing array:

- `[]byte(s)` **copies** — `implicit operator byte[](@string)` copies precisely because the wrapping
  form once let utf8's TestDecodeRune corrupt the package's `utf8map` table (incident recorded in the
  operator's comment). Emitted `slice<byte>(s)` binds this copying route **because of the explicit
  `<byte>` type argument**; two latent backing-SHARING routes exist beside it — `builtin.slice(@string)`
  (non-generic, zero emitted callers) and `implicit operator slice<byte>(@string)` — and the **Tier A
  landing closes both** (make them copy / carry the incident comment) since it already touches this file.
- Pre-boxed shared `object` fields are not observable by reference identity: golib interface equality
  is value-based end to end (`builtin.AreEqual` unwraps adapters and compares by value;
  `@string.Equals`/`GetHashCode` are byte-value). A shared box changes nothing for `any` equality,
  map-of-any keys, or `DeepEqual`.
- Compile-time u8 encoding and runtime `GetBytes` encode the **same UTF-16 literal**, so they can
  diverge only on an unpaired surrogate — where u8 is a compile error (CS9026) while `GetBytes`
  silently substitutes U+FFFD. Surrogates cannot arise (Go rejects surrogate escapes; `\xHH`
  raw-byte literals are diverted to the byte-array-backed path before this rendering). u8 is the
  *stricter* form.
- Package-level Go string vars already live as long-lived shared `@string` statics corpus-wide —
  Tier C adds no new exposure class. Residual writable aliases (`ToSpan()`, `ꓸꓸꓸ`) are read-only by
  every converter-emitted consumer (`append` copies in).
- Tracked separately (pre-existing, NOT a regression of this arc): Go octal escapes (`"\377"` = one
  raw byte 0xFF) are rewritten by `replaceOctalChars` to `ÿ` (two UTF-8 bytes) — `stringLiteralNeedsByteArray`
  scans only `\x`. No corpus instance found; chipped for an independent fix. **FIXED** — the scan now
  also diverts octal escapes ≥ `\200` to the byte-array path (sub-0x80 stays ASCII-safe and readable);
  CNR byte-identical corpus-wide, confirming the no-corpus-instance finding. See
  [`ConversionStrategies-Reference.md`](../ConversionStrategies-Reference.md) *A string literal with
  high raw-byte escapes*, guarded by `HexByteStringLiteral` + `convBasicLit_test.go`.

---

## 2. Tier A — golib span comparison operators (zero visual change)

**Change (golib, ~40 lines + the two route closures above):** `@string` gains the full comparison
set against `ReadOnlySpan<byte>`, both operand orders — `==`, `!=`, `<`, `<=`, `>`, `>=` — on the
null-safe `Bytes` view via `SequenceEqual` / `SequenceCompareTo`.

**Effect:** every `x == "…"u8` / lowered `switch string(y)` chain / relational literal compare
becomes allocation-free with the emitted source text **byte-identical**. Largest single win at the
lowest cost; lands first.

**Evidence (panel-hardened):** binding is *proven*, not argued — a proof type carrying `@string`'s
exact competing surface (`==(T,T)`, implicit `ReadOnlySpan<byte>→T`, implicit `string→T`) plus the
span operators binds `t == "…"u8` to the **span operator** (sentinel counter = 1: identity match
beats user-defined conversion), and the ParseBool row re-measured through the real operator holds at
7.5 ns / 0 B.

**Ambiguity audit:** `sstring` interactions are clean *by construction* — sstring's span conversion
is deliberately **explicit**, so the new operators are not even candidates for an sstring operand;
`slice<byte>` has no span conversion. The one real vector is a **`byte[]`/`Span<byte>` operand**
(`str == someByteArray`: both `==(@string,@string)` and `==(@string,ReadOnlySpan<byte>)` become
applicable with neither better → CS0121 where it compiles today). Go's type system makes the shape
unlikely in emitted code (string ≠ []byte comparisons are illegal Go); the corpus build is the
oracle, and the resolution is pre-committed: **add exact-match `==(@string, byte[])` overloads if it
fires — never withdraw the span operators.**

**Gates:** full behavioral suite (Output 0-fail); full corpus build 0 errors (ambiguity oracle);
banked canaries; PerfStringView flat (sstring paths untouched); `--filter StringMatch` re-measure.

## 2′. Tier A′ — the same operators for named string types (generator)

54 corpus types are `[GoType("@string")] partial struct` (goVersion, html/template's CSS/HTML/JS/URL,
bzip2/flate readers, …). They define only `==(T,T)` plus an implicit span→T conversion, so
`v != ""u8` (real site: go/types version checks) **allocates through the conversion** and stays
allocating after Tier A — comparisons against literals on these types are outside golib. Fix at the
generator: **`TypeGenerator` emits the identical span comparison set** on every `[GoType("@string")]`
struct. (r10-sync already fixed the *const* arm for these types — `const opLoad = mapOp("Load")` now
renders u8 — this tier covers the operators; Tier B covers the conversion-call sites.)

**Gates (gen-class change):** full suite + seeded corpus reconvert/overlay/build per the standing
generator rule; canaries.

---

## 3. Tier B — the combined `(@string)"…"u8` rendering

**Change (converter):** render the cast (`castToGoString`) and the u8 suffix (`u8StringOK`) from
independent flags where the only reason u8 was forced off is that a *bare* span has no conversion to
the target slot. **The rev-1 site table was wrong at 6 of 12 rows** (panel, mechanics lens — three
rows were ValueTuple ref-struct guards, one the panic special case, one keyed on all interfaces, one
the consumer instead of the producer); the corrected table:

| Site | Action |
|:--|:--|
| convKeyValueExpr.go:25 — `anyBoxedStringLitContext()` helper | flip on — **one edit also covers convIndexExpr.go:93/116** (any-typed map-index keys), which consume the same helper |
| convCompositeLit.go:1016 — `markAnyFieldLits` (positional struct-literal element with `any` field) | flip on (independent site the rev-1 table missed) |
| visitAssignStmt.go:118 (interface-target assignment) | flip on |
| visitAssignStmt.go:1124 | flip **only the `emptyIfaceTarget` arm**; the `rhsLen > 1` tuple arm stays off (a span cannot be a ValueTuple element) |
| visitSendStmt.go:72 (send into `channel<any>`) | flip on |
| visitReturnStmt.go:335 (empty-interface element form) | flip on |
| visitValueSpec.go:170 (`var v any = "x"`) | becomes `u8StringOK = !isInterfaceType \|\| isAnyType` — the any arm gets u8+cast; non-empty-interface targets keep today's form |
| convCallExpr.go:1345 (producer: `u8StringArgOK[j] = true` beside `useGoStringArg[j] = true`, inside the `isEmptyInterfaceTarget` gate) | flip on — the one-line producer edit; rev 1 wrongly pointed at the convExprList.go:95 *consumer* |
| **Typed struct-composite element sites** (`new StructuralError("…")`, 83 converter-emitted sites / 10 types — rev 2 mislabelled these "named-string-type conversions"; they are POSITIONAL elements of a typed struct composite whose field is `@string`, a different path from the named-string conversion that already emitted `((errorString)(@string)"…"u8)`; 3 of the original 86 are hand-written `_impl.cs` lines) | flip on — renders the element `"…"u8`, binding the `@string` field through one implicit conversion |
| **Two classes remain bare-UTF-16 after Tier B** (rev 3): `new @string[]{"…"}` slice/array composite elements (**385 corpus sites**, re-counted at landing) and `new any[]{(@string)"a"}` any-ELEMENT composites (convCompositeLit's `isEmptyInterfaceTarget(elementType)` arm sets `useGoStringArg` but not `u8StringArgOK`) | **LANDED** as the Tier C session's opener — one arm keyed on the element type's underlying `string` kind (the element twin of `markStringFieldLits`) plus `u8StringArgOK` beside the existing `useGoStringArg`. CNR one class, 29 lines, zero `u8` removed; 18 goldens re-baselined; suite 494/494 Output 0-fail; seeded corpus 28/0/14, 0 errors |
| visitStructType.go:162 (struct tags) | **stays off** — attribute arguments must be compile-time constants |
| visitReturnStmt.go:238, visitAssignStmt.go:1387 (ValueTuple guards) | **stay off** — same ref-struct class as tags; relabeled, not flipped |
| convCallExpr.go:1691 (`panic("…")`) | **stays off, resolved differently**: r10-sync's golib fix normalizes any C# string reaching `builtin.panic` to a boxed `@string`, so the dynamic type is already Go-correct with **no** emission change — and the bare interned literal is *zero-alloc until a panic actually fires*, which is optimal for a cold path |
| convBinaryExpr.go:957 (concat suppression) | **decoupled, not flipped** — and the landed form needed two corrections rev 2 didn't anticipate (both corpus-only, invisible to behavioral CNR): the signal (`basicLitContext.spanTargetUnsupported = !basicLitContext.u8StringOK`) must be **re-derived at each binary node from that node's own operand u8 decision** — inheriting it changed syscall/exec_windows's `FullPath(d + "\\" + p[2:])`, computing it once changed net/dnsclient's `fqdn == name + "."` in the *reverse* direction. Mechanical proof of purity: across behavioral + corpus, changed lines with a `u8` REMOVED = 0. Guard: the `print("\n" + "\t")` shape |

Measured effect: 2.1–2.4× (ASCII) / 4.2× (non-ASCII), allocation unchanged (transcode eliminated).

**Relationship to Tier C:** C replaces most per-evaluation sites with field references, so B's
standalone footprint shrinks to one-time contexts and the C-excluded classes (format strings,
degenerate-slug literals — see §4.2) — no longer marginal, since those exclusions are now permanent.
B's combined rendering is also exactly what C's field initializers emit. B stays independently
complete and revertible.

**Gates:** converter go test; CNR **classified** (complete site list above makes the one-class claim
checkable); goldens re-baselined after inspection; full suite Output 0-fail; seeded corpus
reconvert + overlay + 0-err build (the concat coupling makes the corpus build load-bearing here).

---

## 4. Tier C — hoisting literals to `static readonly` fields "close to usage"

### 4.1 The form

One field per unique literal per package, placed immediately above the first consuming function,
initialized with the Tier-B rendering; call sites reference the field:

```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string trueˢ = "true"u8;
private static readonly @string falseˢ = "false"u8;

// FormatBool returns "true" or "false" according to the value of b.
public static @string FormatBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}
```

A literal whose **every** package use is an `any`/interface target (the typical unique diagnostic)
is emitted **pre-boxed** (`static readonly object … = (@string)"…"u8;`) so call sites allocate
nothing at all. Mixed-use literals get one `@string` field; any-uses box per call. Never two fields
for one literal. Fields are `private` to the package class, so the beforefieldinit question never
becomes cross-type.

### 4.2 Hoist set — value-materializing contexts, with the panel's exclusions

| Context | Hoist? | Why |
|:--|:--:|:--|
| `return "…"u8`, `@string` local/param assignment, **struct-field assignment** | ✅ | value materializes per evaluation |
| argument to an `@string` parameter, **incl. variadic `@string` element args** | ✅ | the broadest hot footprint |
| `any`/interface targets (args, returns, sends incl. **non-any `chan string` sends**, assignments, KeyValue) | ✅ | pre-boxed when exclusively any-target |
| standalone map-index keys (`counts["build"]++`) | ✅ | rebuilt per evaluation |
| **named-string-type conversions** (`MyStr("…")` in function bodies) | ✅ | same value shape through the wrapper ctor |
| **fmt/log/testing `*f` format-position literals** | ❌ **(changed in rev 2)** | 2,985 corpus sites; 372 are verb-only (`"%v"`, `"%d:%d"`) sluggging to `vˢ`/`dDˢ` — the least allocation saved per unit of readability lost; format calls' cost is dominated by formatting itself. Tier B covers them (2–4×) |
| **degenerate-slug literals** — degenerate = **empty slug OR slug ≤ 2 chars** (§4.10 amendment, user-accepted 2026-07-26; was ≤ 3 in rev 3 — the tighter floor kept `"MD4"u8` inline beside fifteen hoisted siblings in crypto's `Hash.String()` table) | ❌ | names like `strˢ7`, `dˢ`, or `okˢ` carry no information; they stay inline in Tier-B form. Three-char slugs (`md4ˢ`) hoist |
| **the empty literal `""`** | ❌ **(rev 2)** | measured 0 B already (`ToArray()` of an empty span returns `Array.Empty`) — hoisting buys nothing |
| **composite-literal elements and keys** (in-function) | ❌ **(rev 2 — decided with data)** | uniform hoisting would emit **2,229 fields** above html's `populateMaps()` and move those allocations out from under its `sync.Once` guard into the type initializer. A composite materializes its whole table per evaluation anyway; revisit only if a profiled hot composite appears |
| **literals inside `func init()` bodies** | ❌ **(rev 2)** | run once by construction; deterministic AST-level filter, not a hotness heuristic |
| comparison operands (incl. lowered switch chains, relational forms) | ❌ | Tiers A/A′ make them zero-alloc with the literal inline |
| concat operands (`x + "…"u8`) | ❌ | `operator+(@string, ReadOnlySpan<byte>)` already consumes the span |
| `[]byte("…")` / `[]rune("…")` sources | ❌ | result must be freshly mutable; `slice<byte>("…"u8)` is already the single mandatory alloc |
| sstring-elided sites, struct tags, `\xHH` byte-array literals | ❌ | already zero-alloc / attribute constants / diverted |
| package-level var/const initializers and package-level composites — **decided on the GO AST position, not the emitted C# shape** (package-level tables are emitted into `initᴛ*` method bodies and must stay excluded) | ❌ | one-time by nature; Go-named constants are already hoisted by the source |
| func literals **outside** function declarations (package-level `var f = func(){…}`) | ❌ (v1) | no `FunctionPrefixMarker` anchor exists there |

### 4.3 Naming

- **Marker:** new symbols.json entry `HoistedLiteralMarker = "ˢ"` (U+02E2, category Lm, zero corpus
  occurrences today), suffix position: `trueˢ`, `goBuildˢ`. Regenerate both projections via
  gensymbols; never hardcode.
- **Slug:** identifier-safe words, camelCase-joined, truncated at a word boundary ≤ 24 chars.
  Literals whose slug is degenerate are **not hoisted at all** (§4.2) — the `strˢN` fallback now
  exists only for collision ordinals among healthy slugs, not as a naming dump.
- **Collisions:** distinct literals with the same slug → deterministic ordinal by package-wide
  first-occurrence order. The check runs **in the hoist registry against the package's declared
  names plus already-claimed hoist names** — rev 1's claim that `performNameCollisionAnalysis`
  handles synthetic names was wrong (it walks Go declarations only).
- **What is genuinely new here (the decision under review):** every synthetic identifier go2cs emits
  today is either derived from a real Go identifier or initialized on the immediately preceding line
  (`exprᴛ1`, `selᴛ2`). Hoisted names are the first identifiers derived from a *value's content*
  whose definition may be a file away. The `ˢ` suffix is consistent with the marker family; the
  **derivation-from-content is the new category**, deliberately trading the locality property for
  the allocation win — with the §4.2 exclusions ensuring the trade is only made where the name can
  actually carry the meaning.

### 4.4 Placement, dedupe, initialization order, and determinism

- **Package-scoped dedupe, first-use placement** via the converter's existing declaration-injection
  mechanism — `FunctionPrefixMarker`/`currentFuncPrefix`, back-patched above the function's doc
  comment, the same path lifted anonymous struct/interface declarations already ride (rev 1 cited
  the sstring *statement* pre-pass; wrong precedent). Receiver methods emit into the package class
  and are covered.
- **Initialization order (rev-1 blocker, closed):** C# runs field initializers in textual order
  within a class *part* but **unspecified order across parts** — and a package-level var initializer
  that calls a function reading a hoisted field would silently see `default(@string)` (""). The
  converter already owns the defense (`initOrderOperations` relocates ordered initializers into the
  generated static ctor, which runs after ALL field initializers), but its graph is keyed on Go
  variables and cannot see hoisted fields. **Rule: every hoisted field is registered in that
  dependency graph, so any package-level initializer that transitively reads one is relocated into
  the ordered static ctor.** A corpus scan (434 function-calling top-level initializers, transitive
  to depth 3) found zero live instances today — the rule closes the class, not an instance, and a
  guard test pins it.
- **Partially hand-owned packages (rev-1 blocker, closed):** a `[module: GoManualConversion]` file's
  emission is redirected to a non-compiled `.cs.auto`, so it must **never claim first-use** (it may
  reference already-claimed fields; its own literals stay inline). The reconvert gate asserts no
  hoisted field is declared in any `.cs.auto`.
- **`-tests` conversions — two invariants:** (1) the test pass's registry is pre-seeded with the
  production literal→field map, and a test file may only *reference*, never claim, a seeded literal —
  internal test files emit into the production package class and can sort **before** production
  files, so this is what prevents CS0102, not name luck; (2) the registry lives beside the other
  package-scoped state in `resetPackageState`, seed applied after the reset on the `-tests` path.
  Production output stays byte-identical whether or not tests are converted.
- **Determinism:** file conversion is sequential in sorted-filename order (the converter removed
  concurrency for exactly this reason); names and placement derive only from literal content +
  source order.

### 4.5 What Tier C does *not* do (v1)

No hotness heuristics or caps beyond the deterministic §4.2 filters; no golib intern cache for
1-byte strings (noted as a possible refinement); no changes to sstring elision or the byte-array
path. The known worst remaining case is accepted deliberately and shown for review: `http.StatusText`
gains ~63 fields above it (its returns are genuinely per-call and it *is* hot); `testing.Init`'s
52 flag-description literals are format/degenerate-filtered down but its remaining healthy-slug
literals still hoist despite the run-once guard — an `initRan`-pattern filter would be a heuristic,
and v1 refuses those. If the corpus A/B shows this class is common, that becomes a rev-3 decision
with data.

### 4.6 Gates (heaviest — a corpus-wide re-baseline)

Converter go test → CNR **classified** → goldens re-baselined after inspection → full suite
**Output 0-fail** → seeded corpus reconvert + overlay + 0-err build → **full banked-package
operational sweep** at exact counts → performance suite re-run (protocol §4.8). New behavioral
guard project **`StringLiteralHoisting`**: value return; @string arg; variadic element; struct-field
assignment; any-target pre-boxed; mixed-use single-field; standalone map key; named-string-type
conversion; comparison NOT hoisted; concat NOT hoisted; format-position NOT hoisted; degenerate slug
NOT hoisted; `""` NOT hoisted; composite element NOT hoisted; package-level init NOT hoisted (by GO
AST position); `[]byte` source NOT hoisted; slug collision ordinals; cross-file dedupe; **init-order
case** (package var = f() where f reads a hoisted literal declared in a later file — asserts the
non-empty value); **cross-mode case** (internal `_test.go` file sorting before its production
first-use file); **manual-conversion case** (marked file consuming, unmarked file claiming). Docs in
the same landing: ConversionStrategies.md + Reference with real corpus before/after; Symbols docs.

### 4.7 Interactions with landed r10 work

This design builds on three r10-sync/gobprobe landings that reach master via train r11 *before* this
arc: the golib panic-value normalization (§3's panic row), the named-string **const** u8 fix (§2′),
and the package-level func-literal escape-analysis fix (whose emission paths Tier C's registry will
traverse). The arc's baseline corpus is therefore post-r11.

### 4.8 Measurement instrument — `PerfStringMatch` (committed `d2af4a59c`)

20M-iteration hot loop of exactly these shapes; verified byte-identical Go↔C# output; indicative
pre-arc gap ~11× (worst in suite). Protocol (quiet machine): full-suite `--update-readme` baseline
**before** Tier A (pre-arc table → README History); `--filter StringMatch` after each tier
(attribution per tier, recorded in the landing commits); full suite after C with StringView flat and
String/Map non-regressing as the oracle.

---

### 4.9 Tier C as built (rev 4, 2026-07-26)

The rev-3 handoff's load-bearing architecture decision held exactly as recorded: the hoist decision
is a **whole-package PRE-pass keyed per `*ast.BasicLit` node** (`hoistedLiteralOperations.go`), run
immediately before `collectMovedInitVars`, and emission is a **pure substitution at the single
`convExpr` `*ast.BasicLit` arm**. §4.1–§4.6 landed as designed. Eight things the draft did not
anticipate, all found by the gates rather than by reading:

1. **Non-BMP letters cannot be slugged.** `unicode.IsLetter('𝓤')` is true (U+1D4E4, Lu), but a C#
   identifier is lexed over UTF-16 code units, so a surrogate pair is never valid in one:
   `go/types`' universe type set `"𝓤"` produced `𝓤ˢ` and a CS1056/CS1519 cascade in `typeset.cs` /
   `typeterm.cs` (the corpus build was the oracle; 9 errors, one root cause). The slug alphabet is
   now **ASCII letters and digits**, which also closes combining marks, format characters, and RTL
   content, and is what keeps a content-derived name readable in the first place.
2. **An ALL-CAPS word must fold whole.** Lower-casing only the first character left `tESTINGKEYˢ`
   for `"TESTING KEY"` — 9% of the corpus' hoisted names on the first cut (ALL-CAPS constants, HTTP
   verbs and header names, DNS record types). §4.3's "camelCase-joined" is now implemented as such:
   `testingKeyˢ`, `contentTypeˢ`, `getˢ`. A word that already mixes case keeps its interior.
3. **`applyUntypedConstBoxCast` re-boxes a hoisted name.** Its already-cast guard is a
   `HasPrefix(rendered, "(@string)")` text test, which a bare field name fails — so every any-slot
   site emitted `(@string)(fooˢ)`, unboxing a PRE-BOXED `object` field and allocating a fresh box per
   evaluation, i.e. defeating the hoist at exactly the sites §4.1 targets. It now skips a hoisted
   literal outright.
4. **A manual-conversion FUNCTION never renders a prefix marker.** §4.4 closed the marked-FILE trap;
   `visitFuncDecl`'s `isManualFuncDecl` early return is a second one — such a declaration emits only
   a placeholder comment, so a field claimed there would simply vanish. `isManualFuncDecl`'s core is
   now a package-path-keyed free function the pre-pass calls.
5. **Universe builtins must be excluded explicitly.** `go/types` records a call-site-specific
   *signature* for `panic`/`print`/`copy`/`unsafe.Slice`, so the draft's "a builtin has no signature,
   so it falls out" reasoning was wrong: `panic("…")` reads as an `any` parameter and would have
   hoisted, contradicting §3's ruling that the panic literal stays bare (zero-cost until a panic
   fires). Detected via `info.Uses[ident].(*types.Builtin)`.
6. **The init-order rule guards live instances, not just a class.** §4.4 recorded a corpus scan
   finding zero; with hoisting actually on, three surfaced immediately —
   `net/http/internal/testcert` (`LocalhostKey = testingKey(…)` reads two fields declared later in
   the same file; without the rule it would run `strings.ReplaceAll(s, "", "")`), `internal/profile`,
   and `runtime/pprof`. Each gains a `package_init.cs`.
7. **Two dead deref-alias prologues disappear.** `bodyReferencesIdentAsValue` is a text test whose
   own comment names "a string" as a source of spurious matches; the words *node* and *call* inside
   the message literals of `runtime`'s `lfnodeValidate` and `go/types`' `suspendedCall` were the only
   textual occurrences keeping those aliases alive. Moving the literal out of the body drops the dead
   local. Zero aliases were ADDED, and a genuinely live alias is still never dropped.
8. **Diff classification needs a canonicalizer, not eyeballs.** The CNR diff is ~2,300 lines across
   208 projects; it was proven to be exactly two classes by rewriting both sides to a slot form
   (field name ⇄ its initializer rendering, with or without the `(@string)` cast and `u8` suffix a
   substituted site may shed) and comparing multisets per directory: **1,140 fields declared, 1,145
   call-site substitutions matched, 0 residual on both sides.**
9. **§4.4's init-order rule needs a second arm where the relocation is unavailable — and only the
   OPERATIONAL sweep could find it.** `collectMovedInitVars` deliberately does not run on the
   `-tests` path (no `package_init.cs` emission there, and an internal variant shares the production
   class, which may already own a static ctor — a second one is CS0111). So a *test-file* package
   var reading a test-file hoisted field had no defense: `encoding/pem`'s
   `var pemData = testingKey(…)` is declared ~300 lines ABOVE the `testingKey` whose two hoisted
   fields it depends on, ran `strings.ReplaceAll(s, "", "")`, and left every `"TESTING KEY"` in
   place — TestDecode and TestEncode failed and the package dropped out of the sweep (42/43). The
   fix is a second arm of the same rule rather than a site patch: `collectHoistedLiterals` takes
   `initOrderRelocated`, and where the driver cannot relocate, **no literal inside a function a
   package-level initializer can reach is hoisted at all** (those sites keep the inline Tier-B
   rendering; the same literal still hoists from any other use). The `-tests` SEED run passes true —
   it simulates `processConversion` and must reproduce the production `.cs` exactly — so production
   is untouched, proven by CNR byte-identity across 495 behavioral projects *and* a full corpus
   reconvert A/B with zero production `.cs`/`.csproj` differences. Note what this says about the
   gate stack: the corpus **compiled clean** with the bug present. Only running the tests found it.

**`PerfStringMatch`, the arc's instrument (§4.8 protocol, quiet machine).** Full progression:
baseline **1,699.5 ms (11.75×)** → Tier A **1,471.9 (9.93×)** → Tier B **1,488.3 (9.87×)** →
**Tier C 1,386.1 (9.19×)** JIT, and **1,762.5 (12.18×) → 1,321.4 (8.76×)** Native AOT. Whole arc:
**−18.4% JIT / −25.0% AOT** wall time against Go, i.e. 11.75× → 9.19× and 12.18× → 8.76×. Tier C's
own share is the −6.9% JIT / −6.8% AOT step, from exactly the three shapes it targets in that
benchmark — the `HasPrefix` prefix argument, `kindName`'s four literal returns, and the two literal
map keys (its `switch` chains were already free after Tier A, and `"// "` slugs to nothing and stays
inline by the degenerate rule). Oracles held: **StringView flat** (3.05×→3.05× JIT, 1.97×→1.95× AOT
— its sstring paths are untouched) and **String/Map non-regressing** (String 10.79×→10.75× JIT and
11.11×→10.72× AOT against the PRE-ARC baseline; its only Tier C change is the two closing `Println`s
outside the timed loop, and the intermediate post-Tier-B reading of 9.68× was a fast outlier, not a
level. Map 0.86×→0.86× JIT, 0.31×→0.34× AOT).

**Measured (Go 1.23.1, 302 packages):** 3,253 hoisted fields across 467 files, 62 pre-boxed.
Per-function blocks: 1,634 blocks, median **1**, p90 **4**, p99 **11**, max **61** —
`net/http`'s `StatusText`, exactly the §4.5 worst case accepted under decision 4, with nothing
beyond it. Per file: median **3**, p90 16, p99 76, max **127** (the 20k-line bundled
`net/http/h2_bundle.cs`, i.e. the same density spread over many functions, not a single block).

### 4.10 Accepted naming amendments (2026-07-26 — user-approved; **IMPLEMENTED in the r15 train**)

Two §4.3 refinements accepted during the user's corpus review of the landed Tier C output (the
crypto `Hash.String()` table made both visible):

1. **Degenerate threshold ≤ 3 → ≤ 2.** Three-character slugs (`mD4ˢ`, `mD5ˢ`) become hoistable,
   unifying tables where the rule boundary currently lands mid-function (`"MD4"u8` inline beside
   fifteen hoisted siblings). Admits names for the ~8.7% of distinct literals in the 3-char-slug
   class; the empty and ≤2 classes stay inline.
2. **Mixed-case leading-word fold.** `"BLAKE2s-256"` currently slugs to `bLAKE2s256ˢ` — the
   camelCase first-character lowering applied to a mixed-case word. The fold must produce a natural
   reading (e.g. `blake2s256ˢ`), deterministic, keeping the existing ALL-CAPS whole-word fold
   (`SHA` → `sha`); exact rule proposed by the implementer with corpus A/B examples.

Both are one-constant/one-function converter changes with a mechanical corpus-wide re-baseline
(goldens + corpus regen; collision ordinals may shift) through the standard gate stack; the
`StringLiteralHoisting` guard updates its degenerate case to the new rule and gains a 3-char-hoist
and a mixed-case-fold case.

## 5. Sequencing (one session) and rollback

| Order | Landing | Risk | Revert story |
|:--:|:--|:--|:--|
| 1 | **A** golib span operators + slice-route closures | byte[] CS0121 vector (pre-committed resolution) | revert one golib commit |
| 2 | **A′** TypeGenerator span operators | gen-class blast radius (full gates) | revert one gen commit |
| 3 | **B** combined rendering (corrected site table) | concat coupling (decoupled signal + guard) | revert converter commit + re-baseline |
| 4 | **C** hoisting | emission complexity; corpus-wide churn | revert converter commit + re-baseline; A/A′/B stand alone |

Corpus regenerated per-tier for gating, **committed once** post-C (with the banked test-artifact
refresh). The session runs after train r11 lands (§4.7).

## 6. Decisions (all five ACCEPTED as recommended — user, 2026-07-25)

1. **Marker `ˢ` suffix** — and, explicitly, the *derivation-from-content naming category* it
   introduces (§4.3 last bullet). *Recommended: accept; the §4.2 exclusions confine it to literals
   whose names read well.*
2. **Pre-boxed `object` fields for exclusively-any literals** — *recommended: yes* (identity-safety
   verified; deterministic from the registry's own use-set).
3. **Slug budget 24 chars, word-boundary** — *recommended: keep; degenerate cases are now excluded
   rather than badly named.*
4. **The StatusText class** (§4.5): accept ~63-field blocks above genuinely hot literal-heavy
   functions, or add a deterministic size cap (e.g. a function contributing > N fields keeps its
   literals inline)? *Recommended: accept in v1; a cap is a knob the corpus A/B should justify.*
5. **Tier B's standalone landing** — *recommended: keep* (now permanently owns the format-string and
   degenerate-slug classes, no longer just transitional).

## 7a. Implementation round record (rev 3, 2026-07-25)

Tiers 0/A/A′/B landed per spec; per-tier gates all green (suite PASS 494/494 ×3; corpus 0 errors ×3;
seeded gates 28/0/14; canaries io/fs 18, errors 61, bytes 81 at banked counts; Tier A′'s overlay
produced **zero content diff**, proving the reconvert byte-exact against the banked tree).
`PerfStringMatch` quiet-machine progression: **baseline 1,699.5 ms (11.75×) → Tier A 1,471.9 ms
(9.93×) → Tier B 1,488.3 ms (9.87×, flat as predicted — this benchmark's only any-slot literals are
its two closing Printlns)**; the remaining ~10× is exactly Tier C's target. Findings folded into
this rev: the degenerate-slug wording fix (§4.2), the §3 row relabel + the two remaining composite
classes, the concat-decoupling re-derivation rule (§3), and two doctrine corrections recorded in
CLAUDE.md (the seed gate's marker scan must anchor `^\s*\[module:` — two reflect files *mention* the
marker in placeholder comments; the overlay csproj exception is two EXACT paths, never the
`core\testing\` prefix, because `core\testing\iotest` is a relocated package). Tier A's
pre-committed CS0121 vector never fired — no `byte[]` overloads were needed.

## 7. Panel record (round 1)

Three lenses (semantics / converter mechanics / scope+fidelity), all **sound-with-fixes**. Blockers,
all closed by design changes above: **B1** hoisted-field init-order hazard → registry feeds the
`initOrderOperations` graph (§4.4); **B2** rev-1 Tier B site table wrong at 6/12 rows → corrected
table with producer-side edits and stay-off relabels (§3); **B3** concat suppression coupled to the
flipped flag → dedicated signal + compile-break guard (§3); **B4** GoManualConversion first-use trap
→ marked files never claim (§4.4). Major re-scopes: format-position/degenerate/empty/composite/init()
exclusions (§4.2, with corpus counts); Tier A′ for named string types (§2′); binding + operator-form
+ empty-literal evidence closed by measurement (§1, §2). The panic row resolved itself via r10-sync's
golib normalization (§3). Full lens reports in the session task output; corpus counts therein.

---

## 8. Dated amendment, 2026-09-23 (C1, from the H10 relabel reads) -- three literal classes that allocate per call

Written so the entries whose counted objects include a per-call literal cite a record, as the H10
relabel ruling's plan bar requires (ledger 2026-09-23 03:37, X(1)). Read at `bb54ff0920`. Owner: C1.
Full design: phase-4D kickoff. Nothing above this block is rewritten.

**This block REOPENS owner-accepted decisions, and the owner's ruling is its precondition.** Arms A and
B would change decision 3 (§6, :430), decision 5 (:435-436), §4.2's degenerate-slug and format-position
rows (:203-204), §4.3's naming rule (:221-222) and the user-approved §4.10 amendment (:392). None of
them lands until the owner rules; COORD surfaces the question. Arm C reopens nothing (below).
*(Fixed up 2026-09-23 per COORD's ACCEPT-WITH-FIXES, ledger 3942e083ad, items 7 and 8 and COORD's
ruling 3: the first version of this block said it did not reopen §6's decisions; it does.)*

**Arm A -- the degenerate-slug literal.** §4.10's floor keeps a literal whose slug is two characters or
fewer inline (`minHoistSlugLength = 3`, src/go2cs/hoistedLiteralOperations.go:94-103), where §3's
Tier-B rendering `(@string)"n"u8` materialises it through `new @string(value)`
(src/core/golib/string.cs:460-463, a counted `CopyOf`) on EVERY evaluation. log/slog's call-site keys
pay it: the explicit `(@string)"n"u8`, `"s"u8`, `"d"u8` casts in the `...any` packs
(src/core/log/slog/logger_test.cs:319-320, :332-333, :359-361), and the `"a"u8`..`"f"u8` keys of the
LogAttrs calls, which are IMPLICIT span-to-`@string` conversions at the `@string` parameter -- the same
operator, string.cs:460-463 (:377, :391, :400, :410-411, :421-423). *Stage:* hoist a degenerate-slug
literal evaluated inside a function body under a positional name (the design's `strˢN` form, today only
a collision ordinal among healthy slugs), with the literal in a comment beside the hoisted field.
*Removes:* one counted object per evaluation. *Predictions (UNMEASURED):* log/slog 2_pairs 10 -> 8,
2_pairs_disabled_inline 4 -> 2, 9_kvs 27 -> 18, attrs1 7 -> 6, attrs3 12 -> 9, attrs3_disabled 9 -> 6,
attrs6 21 -> 15, attrs9 28 -> 19.

**Arm B -- the format-position literal.** A literal in a formatting call's format position is excluded
STRUCTURALLY, independent of its slug (hoistedLiteralOperations.go:590-592; §4.2's format-position row,
:203; decision 5, :435-436). log TestDiscard's `"%s"u8` passed to `Printf` (src/core/log/log_test.cs:239)
is one: the entry's only excess, since log.cs:289's params copy is Go's own allocation. *Stage:* lift
the format-position exclusion for a literal evaluated per call inside a function body, hoisting it like
arm A. *Removes:* one counted object per evaluation. *Prediction (UNMEASURED):* log TestDiscard 2 -> 1
(want at most 1).

**Arm C -- the function-local CONST.** Tier C skips every CONST spec (hoistedLiteralOperations.go:478-487)
on the premise that a const is "emitted as the constant's own `static readonly` field". That holds at
PACKAGE level (compare strconv's `static readonly fnParseFloat`, atof.cs:609) but not inside a function,
where `const fnAtoi = "Atoi"` is emitted as a local `@string fnAtoi = "Atoi"u8;` (strconv/atoi.cs:255)
and materialised on every call. *Stage:* narrow the CONST exclusion to package-level specs, so a
function-local const hoists under its OWN name -- no naming decision is reopened. *Removes:* one counted
object per const per call. *Members:* strconv TestAllocationsFromBytes/Atoi (`fnAtoi`), /ParseInt
(`fnParseInt` at :214 and, through ParseUint, `fnParseUint` at :75) and /ParseUint (`fnParseUint`).
*Predictions (UNMEASURED), after DESIGN-string-byte-window.md §7's stages 2 and 3:* Atoi 1 -> 0, ParseInt
2 -> 0, ParseUint 1 -> 0.

**Refusals (all arms):** a literal in a constant context outside a function (nothing is materialised);
a literal whose `u8` span is consumed as a span (no `@string` is minted); a literal evaluated once.

**Gate:** each arm's two-seeded corpus reconvert hunk count, then its members' rows before and after at
Release with tiering off.

## 9. Dated amendment, 2026-10-19 (C2, owner ruling 2026-10-17) -- plain literals are the default; the standard library keeps Tier C

Tier C is now the standard library's mode and an opt-in for everything else. Outside the standard
library, a literal whose slot is exactly Go `string` renders as a bare C# literal (`F("hello")`), and
golib's `implicit operator @string(string)` converts each C# literal once through `StringLiteralCache`
(golib StringLiteralCache.cs; a hit costs about 0.5 ns over a hoisted field, measured there). A package
whose source is under GOROOT/src keeps the hoist under `-stdlib` and `-tests` alike (`literalsHoistedFor`);
`-hoist-literals` opts any other package in, and is the recommended opt-out for performance-critical code.
Every slot where a bare C# string would bind the wrong C# type keeps today's form: `any` slots (still
hoisted pre-boxed), named string types, arguments whose callee parameter mentions a type parameter, go and
defer arguments, sstring twins, multi-line raw strings and raw-byte literals. The fail-closed guard is the
analyzer `GO2CS0004` (go2cs-gen StringLiteralBindingAnalyzer.cs; one marker, `[GoTakesCSharpString]`).

**As landed:** seat claude/c2-plain-literals d4bee28445, merged on hop 9c2a551adf as e1f98819c4; windows
goldens 904fc6aed1 (COORD). **Measured at the merge 130fbd495b (linux):** three-target `-stdlib` corpus
0 of 7530 files differ, and the negative control (GOROOT key forced off) moves 1219 files and removes 4502
hoisted fields, so the comparison can see a change and the zero is a real reading. The behavioral CNR moved 474 of the
476 predicted goldens, plus DeferVariadicCallee and the new InlineConstraintLift, and retired three
generated package_init.cs files that only hoisted fields had needed (initOrderOperations.go
`retireStalePackageInitFile`). The two linux misses, EnvironBlockWalk and LocalTimeZone, are windows-only
fixtures, measured by the windows regeneration. Interceptors, which would remove the cache lookup, are
later work and not part of this amendment.

## 10. Dated amendment, 2026-10-19 (C2, owner ruling 2026-10-17, part 2) -- `any` slots go plain, boxed by interceptors

Outside the standard library, a literal in an `any` slot of a call go2cs-gen can intercept now renders bare too
(`fmt.Println("hello")`). go2cs-gen's `LiteralInterceptorGenerator` emits, per source file, interceptors that write a
cached, pre-boxed `@string` into the slot and forward, so the call allocates nothing and the callee never sees a
`System.String`. A call the generator cannot intercept is `GO2CS0005`, and an overload that captured the literal is
`GO2CS0006`. `GO2CS0004` accepts a boxed literal only when its call is intercepted. Every build route admits
`go2cs.Interceptors` and raises a go2cs-gen load failure (`CS8032`, `CS8034`, `CS8784`, `CS8785`, `CS9057`) to an
error, and `go.lib.targets` refuses a compiler below Roslyn 4.14 (`GO2CS3001`). The user-facing rules are in
strings.md, *A literal in an `any` slot is a plain C# literal too*.

**The boundary is generator visibility.** Every source generator reads the same input compilation, so the interceptor
generator cannot see what go2cs-gen's other generators add. The first full go2cs.slnx build at the seat measured both
consequences:
- A call whose OTHER argument uses a generated member does not bind for it. DefinedArrayChainSlice's
  `fmt.Println(..., a.Value[1])` names TypeGenerator's `Value`. The generator resolves that call from its one-member
  method group.
- A call whose RECEIVER or bound overload is generated (RecvGenerator's `ж<T>` forwarders) cannot be resolved at all.
  That was 5 GO2CS0004 sites, all fail-closed. The converter keeps the pre-boxed field there.

**Measured at the merge c2b3bd0bc0 (linux, base = hop tip 66d41328b5).** The CNR moved 404 `.cs`, 906 `.csproj` (the
template block) and 372 `package_info.cs` (position maps only), against a revised prediction of 402 / 904 / 372. The +2
are the two PointerConversion fixtures new at the tip. The base showed 15 files of pre-existing linux drift,
subtracted by name. The go2cs.slnx Debug build had 0 errors and 0 GO2CS0004/0005/0006 sites.
