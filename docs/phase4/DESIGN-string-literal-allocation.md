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

## 8.1 DRAFT amendment, 2026-09-24 (C2) -- sizing an INVISIBLE alternative to arms A and B, and the annotation-level flag

**DRAFT, UNMERGED** (branch `claude/c2-literal-cache-draft`, off `47e088d3d7`). C1 amends the record of
record post-hop; this block is its input and rewrites nothing above it. It is triggered by the owner's
ruling (ledger 2026-09-23 10:10 (2)(3)): arm C is APPROVED; arms A and B are HELD while an alternative
that leaves the visible code as Go wrote it is sized. The owner's axis is readability vs performance vs
behavioral parity, because strings are among the first things a user meets. Measurements come from the
probe [`probes/c2-literal-cache/`](probes/c2-literal-cache/README.md): the real golib `@string` and
`AllocationCounter` at `47e088d3d7`, on .NET 10.0.12, Release, tiering off, on a shared 4-core linux
VM. Read the ratios, not the absolute numbers.

*(Revised 2026-09-24 by §8.1R and again by §8.1R2 below, per COORD's review and verification; 8.1.1-8.1.5 are kept as reviewed so its citations
resolve. §8.1R supersedes 8.1.2-8.1.4's figures and recommendation; §8.1.x, §8.2 and §8.3 follow it.)*

### 8.1.1 The problem an invisible form has to solve: recognising the literal from inside golib

The per-call object is minted in golib: `(@string)"n"u8` and every implicit `"a"u8` at an `@string`
parameter bind `implicit operator @string(ReadOnlySpan<byte>)` (string.cs:460-463). That operator
calls the copying constructor (:92), whose `CopyOf` is the counted object §8's arms remove. To reuse
one `@string` per literal with NO emission change, golib must recognise "the same literal again", and
all it is handed is a `ReadOnlySpan<byte>`. There is no per-call-site hook without an emission change:
- a source generator cannot rewrite an existing site;
- C# interceptors intercept method invocations, not conversion operators;
- `@string` is not a ref struct, so it cannot hold the span.

That leaves three possible keys: the bytes (content), the span's address (a u8 literal is RVA data),
or, if the converter drops `u8`, the interned UTF-16 literal's reference.

### 8.1.2 Mechanisms, measured

| mechanism | hit ns | B/op | counted/op | miss path (NON-literal span through the same entry) | visible delta |
|:--|--:|--:|--:|:--|:--|
| today: copy per evaluation | 9.5–10.0 | 32 | 1 | 13.7 ns (the baseline) | — |
| hoisted field (Tier C; arms A/B/C) | 2.2–2.3 | 0 | 0 | n/a | `strˢN`/`sˢ` names (A/B); own name (C) |
| golib cache, content-hashed, direct-mapped, `byte[]` slots | 7.3–8.7 | 0 | 0 | 27.5 ns (+14 ns, 2×) | **none** |
| golib cache, address-keyed, direct-mapped | 7.1 | 0 | 0 | (same shape as content) | **none** |
| golib cache, insert-only open addressing | 10.9–11.5 | 0 | 0 | 47.8 ns once it is full | **none** |
| golib cache, `ConcurrentDictionary` + span alternate lookup | 16.9 | 0 | 0 | 50.7 ns | **none** |
| UTF-16 literal, reference-keyed (converter drops `u8`) | 6.1 | 0 | 0 | +1 entry object on every miss | `(@string)"n"` for `(@string)"n"u8` |

The same measurements by shape:

| shape | today | hoisted field | content cache |
|:--|:--|:--|:--|
| arm B, `Printf("%s", v)` | 9.9 ns / 1 counted | 2.3 / 0 | 7.8 / 0 |
| arm C, `@string fnAtoi = "Atoi"u8` | 9.5 / 1 | 2.3 / 0 | 8.7 / 0 |
| **log/slog `...any` pack, three one-letter keys (arm A's actual members)** | 62.9 ns, 264 B, 3 counted | pre-boxed fields: 16.5 ns, 72 B, 0 | **63.4 ns, 168 B, 0 counted** |

A forced two-literal slot collision measured 2 counted per call. The concurrency stress (8 threads ×
2M mixed hits and misses, every result content-checked) found **0 mismatches in 32,000,000 results**.

### 8.1.3 What the numbers say

1. **A golib cache buys COUNT parity, not speed.** At every `@string`-typed site (arms A, B and C) it
   takes `counted` from 1 to 0, which is exactly what `testing.AllocsPerRun` reads, and so exactly what
   the arms remove. It recovers only ~25% of the time, where hoisting recovers ~78%: on this JIT a
   32-byte allocation costs about what a lookup costs.
2. **At any-typed sites the box stays.** log/slog's key/value packs are arm A's members. The cache
   removes the counted copy (3 → 0 counted) and nothing else: the C# box at the interface slot is
   compiler-emitted, and `AllocationCounter` excludes boxing by design (AllocationCounter.cs:53).
   Bytes fall only to 168 (pre-boxed hoisting reaches 72), and time does not move. §8's COUNT
   predictions for the members survive; the bytes and time that pre-boxing would buy do not.
3. **The cache taxes every non-literal conversion through the same entry** (+14 ns, 2×). So it must
   sit where literals arrive and nowhere wider:
   - the `@string` span OPERATOR (string.cs:460), plus the one generator line that builds a named
     string type from a span (InheritedTypeTemplate.cs:390, which calls the constructor directly today);
   - NEVER the constructor (:92), which golib's own 22 `new @string(` sites use for runtime bytes;
   - behind a length gate. A span over the gate pays one compare (16.4 → 18.6 ns, inside this VM's
     spread).
4. **Address keys are refused.** An RVA address moves with the image base (ASLR), so which literals
   collide would change from RUN to run, and a banked `AllocsPerRun` row would flake. A content hash
   is deterministic: a collision is a property of two literals' bytes and reproduces exactly.
   Collisions matter only inside one measured function's literal set:
   - direct-mapped, 4096 slots: P ≈ 1.1% for 10 literals and 25.8% for 50;
   - **2-way set-associative, 2048 sets: ≈ 0 for 10 and 0.5% for 50.**

   The §8 member set (15 distinct literals) maps to 15 distinct slots under FNV-1a/4096.
5. **Thread safety is structural, not locked.** Each slot holds ONE reference (the `byte[]` itself),
   so a racing reader sees either the old array or the new one, and every hit is content-verified. A
   torn or stale slot can only produce a miss, never a wrong string. Because no entry object exists,
   the miss path allocates exactly what today allocates. The UTF-16-keyed variant cannot do this: it
   needs a (key, value) pair per slot, so it costs +1 object on every miss. It is also the fastest hit
   (6.1 ns) but a visible delta, reintroduces a transcode at first use, and taxes every runtime .NET
   string through `operator @string(string)`.
6. **Parity hazards, against the hoisted form:**
   - *Identity.* Two evaluations of one literal share a backing array, so `unsafe.StringData` returns
     EQUAL pointers, which is Go's behaviour (RODATA) and not today's C#. The cache shares this
     improvement with Tier C.
   - *Mutation through `unsafe.Slice(unsafe.StringData(s), …)`.* Go faults on a literal. Today's C#
     corrupts one copy, a hoisted field corrupts that field, and the cache corrupts every
     equal-content `@string` that hits the slot, **including a non-literal with the same bytes**. That
     is the ONE hazard class the cache adds over Tier C. It is bounded by the length gate, and it is
     Go UB in every variant.
   - *`[]byte(s)` / `slice<byte>(s)`.* These still copy (§1's preconditions), so nothing changes.
   - *Initialization order.* NONE: the cache is consulted at evaluation, so §4.4's and §4.9's
     relocation machinery (items 6 and 9) has nothing to guard, unlike every hoisted form.
   - *Retention.* Bounded: 4096 × 16 B ≈ 64 KiB plus array headers.
7. **Not built: a pointer-backed `@string`** (the RVA pointer held directly; zero allocation and zero
   lookup). It adds a field and a branch to every `@string` read, and it needs a literal-only
   constructor the converter would have to emit, which is a visible delta. A write through
   `unsafe.StringData` would then fault on read-only image memory, which is exactly Go's behaviour.
   It is too wide for this arm; recorded so the option is not rediscovered.
8. **An instrument trap:** a capped `ConcurrentDictionary` cache that reads `.Count` takes every lock.
   The probe's first run stalled there, so the cap is kept in a separate counter.

### 8.1.4 Recommendation

- **Arm C: land as approved.** It is the best time (2.3 ns), it hoists under the const's own name, and
  it reopens no naming decision.
- **Arms A and B: retire the hoisting forms** and replace them with the golib literal cache:
  - **Design:** content-hashed, 2-way set-associative, one `byte[]` per slot, content-verified,
    length-gated at ≤ 16 bytes. That covers every degenerate-slug literal, the verb formats, and all
    §8 members, which are ≤ 9 bytes.
  - **Where it is wired:** at exactly two entries, the `@string` span operator and
    InheritedTypeTemplate.cs:390. The constructor stays copying.
  - **What it gives:** zero visible delta, and count parity at every member row. It adds no
    init-order machinery, and its only new hazard is 8.1.3 (6)'s, which is bounded and Go UB.
- **What it does not buy, stated rather than taken:** speed (about a quarter of what hoisting buys),
  and at any-typed sites the box and its bytes. If the owner wants those at log/slog's call sites,
  the price is the visible positional names the ruling has already held.
- **Before it lands:**
  - an instrumented corpus sweep counting the operator's NON-literal callers, which sizes the 8.1.3
    (3) tax on real code rather than on this probe;
  - C1's member rows before and after, at Release with tiering off;
  - `DESIGN-allocation-counting.md`'s site census gains the cache's miss path (the same `CopyOf`
    charge);
  - a golib test pinning the content-verify, the length gate, and the two-entries-only wiring.
- **Predictions (UNMEASURED on the members):** §8's counts hold unchanged, because the cache removes
  the same `CopyOf` the arms remove: log/slog 2_pairs 10 → 8, 2_pairs_disabled_inline 4 → 2,
  9_kvs 27 → 18, attrs1 7 → 6, attrs3 12 → 9, attrs3_disabled 9 → 6, attrs6 21 → 15, attrs9 28 → 19;
  log TestDiscard 2 → 1. Arm C's rows are unaffected, since it hoists regardless.

### 8.1.5 The annotation-level flag -- the hoisted-literal comment goes OFF by default

**`-annotations=quiet|normal|verbose`, default `normal`.** It governs only comments go2cs AUTHORS about
its own mechanism. It is orthogonal to `-comments` (Go's own comments: the user's content,
main.go:274) and to `-provenance` (its own opt-in, :273), and it is named apart from `-comments` so the
two cannot be confused.

Census of the converter-authored comment families in the committed corpus at `47e088d3d7` (3,925
tracked converted `.cs`, excluding golib and `*_impl.cs`):

| family | emitted at | lines | files | quiet | normal | verbose |
|:--|:--|--:|--:|:--:|:--:|:--:|
| `// Hoisted @string literals (single allocation; Go keeps these in RODATA)` | hoistedLiteralOperations.go:960 | 5,322 | 1,257 | off | **off** (the ruling) | on |
| `// Hoisted Go big-integer constant (…)` | visitValueSpec.go:990 | 13 | 6 | off | off | on |
| `} // end <PackageClass>` | visitFile.go:142, initOrderOperations.go:490/:547 | 2,936 | 2,936 | off | on | on |
| `// blank import: <path> (side effects only; …)` | visitImportSpec.go:323 | 162 | 147 | off | on | on |
| `// type <T> is a methodless func type — rendered inline …` | visitFuncType.go:36 | 69 | 58 | off | on | on |
| package_init.cs ordering headers (production and test variant) | initOrderOperations.go | 97 | 97 | off | off | on |
| metadata-anchor prose (`// go2cs metadata anchor for …`) | testConversion.go | 333 | 333 | off | off | on |
| package_info.cs explanatory prose (the paragraphs between the region markers) | packageInfoWriter.go, positionMapOperations.go, typeAccessibilityOperations.go, importInitSection.go (and the alias-section prose, which is template text) | ≈15K | ≈733 | off | off | on |
| arm A's proposed per-field literal echo | (§8, arm A) | — | — | off | off | on (moot if A retires) |

The ≈15K figure is 16 prose lines repeated 733 times plus 8 repeated 401 times, READ from the
repeated-line census, not counted per file. Hoisted-literal comments come off by default. Relocation
notices and end markers stay: they tell a reader where Go code went or where a scope ends, which
serves readability. Mechanism narration moves to `verbose`.

**NEVER GOVERNED, because each is load-bearing:**
- the package_info.cs `// <Region>` markers: the converter reads `ImportedTypeAliases` back out of them;
- `// go2cs generated this placeholder — …` (409 lines, 89 files): platformHandOwn.go:384 scans for
  `funcPlaceholderLead` as the hand-own witness;
- `// Code generated by go2cs. DO NOT EDIT.` (97 files): the generated-code tooling contract;
- `GoPositionMap` records, which are attributes, not comments.

**Footprint of the default flip:** at least the 5,322 hoist headers plus the moved prose, as a
corpus-wide re-baseline with goldens. Every removed line shifts C# line numbers, so the `GoPositionMap`
tables regenerate (mechanically). This is its own seat post-hop: CNR one class, two-seeded corpus
reconvert, and a reader check that nothing parses the prose lines between package_info.cs's markers.

## 8.1R REVISION, 2026-09-24 (C2) -- the 22 review fixes, the recommended cache measured, and a table that replaces it

**DRAFT, UNMERGED**, same branch. Input: COORD's adversarial review of `951c916403`
(`docs/phase4/reviews/literal-cache-draft-review-2026-09-23.md` on `claude/coord-handover` at `a456cecaf7`;
"fix N" below is its Part 2 item N). This block supersedes 8.1.2-8.1.4's figures and recommendation.
8.1.1-8.1.5 stay as reviewed, so every review citation still resolves. Citations are at `47e088d3d7`, this
branch's base. The new rows are round 2 of the same probe (`Caches2.cs`, `Round2.cs`; outputs in
`output-round2-linux.txt`), on .NET 10.0.12, linux-x64, the shared 4-core VM, in three modes:
- JIT with tiering off (the csproj's setting, and the mode C1's rows run in);
- JIT with `DOTNET_TieredCompilation=1` (an environment override; the mode was not independently verified);
- NativeAOT.

**Legs NOT run, in any mode:** Windows, ARM64, the Performance suite, the banked operational sweep. Every
probe build is local, unofficial, and not a gate.

### 8.1R.1 Coverage: the ≤ 16 B gate misses most of arm B's population (fix 1)

The draft's claim that the gate "covers every degenerate-slug literal, the verb formats" (:627-628) is
false. Census over the Go source of `std` plus its tests (GOOS=windows, go1.24.13, 2,749 files; a literal
counted only where it is a DIRECT call argument), with the format position decided by the converter's own
predicate (hoistedLiteralOperations.go:595) and "degenerate" by its own slug floor
(`len(literalSlug(v)) < minHoistSlugLength`, :103):

| population | ≤ 8 B | 9-16 B | 17-32 B | > 32 B | total | ≤ 16 B |
|:--|--:|--:|--:|--:|--:|--:|
| format-position literals, production | 269 | 282 | 837 | 1,211 | 2,599 | 21.2% |
| format-position literals, tests | 1,267 | 1,825 | 7,193 | 6,565 | 16,850 | 18.4% |
| degenerate literal → `@string` param, production | 1,565 | 19 | 27 | 20 | 1,631 | 97.1% |
| degenerate literal → `@string` param, tests | 3,296 | 298 | 119 | 208 | 3,921 | 91.7% |
| degenerate literal → `any` param, production | 69 | 0 | 0 | 0 | 69 | 100% |
| degenerate literal → `any` param, tests | 191 | 4 | 3 | 5 | 203 | 96.6% |

Overall, 18.7% of format-position literals are ≤ 16 B and 7.9% are ≤ 8 B. COORD's 14-17% was measured over
the C# tree with a different predicate, so the two figures agree on the conclusion rather than the digits.

**The leftover set under a 16 B gate:**
- 15,806 format-position literals over 16 B (2,048 of them production);
- 374 degenerate literals over 16 B (47 production), e.g. COORD's crypto/cipher/gcm_test.cs:562.

Each keeps copying on every call if arms A and B retire in favour of the cache.

**The owner's options, as the review framed them:**
- (a) a hybrid: the cache under the gate, hoisting above it;
- (b) a raised gate: the hit cost grows with length, see 8.1R.2;
- (c) accept the gap.

8.1.x adds (d), which has no gate at all.

### 8.1R.2 The recommended 2-way form, measured (fixes 2, 3, 4, 5, 16, 17)

**Replacement policy (fix 3):**
- no write on a hit;
- a miss fills way 0 if it is empty, otherwise it overwrites way 1. That is one store either way.

A literal that lands in way 0 is never evicted; one that lands in way 1 can be. Which way it lands in
depends on what touched its set first. That dependence is the root of 8.1R.3.

**The variants:**
- V8 = FNV-1a over the bytes, then XOR the length, then AND 2047 (sets), two ways.
- V9 = the same table with a word hash: two overlapping 64-bit reads cover 1-16 B, then a multiply-xor fold
  (`Lit2.WordHash`).
- V10 = the registration table of 8.1.x.
- The round-1 V2 was FNV-1a, XOR length, AND 4095, direct-mapped (fix 17's hash spec).

Hit cost, ns per conversion, `counted` 0 unless marked:

| length | today (V0) | V8 2-way FNV | V9 2-way word | V10 table | mode |
|:--|--:|--:|--:|--:|:--|
| 1 B | 11.89 | 5.91 | 6.80 | 4.38 | JIT, tiering off |
| 8 B | 11.92 | 8.39 | 7.15 | 4.15 | |
| 12 B | 13.24 | 11.10 | 6.37 | 4.17 | |
| 16 B | 13.30 | **14.75** | 6.23 | 4.58 | |
| 32 B | 15.13 | 19.82, 1 counted | 19.39, 1 counted | 4.14 | |
| 16 B | 12.88 | 14.45 | 5.21 | 3.84 | JIT, tiered |
| 16 B | 8.47 | 14.46 | 6.55 | 4.38 | NativeAOT |
| hoisted field | 1.33 / 2.58 / 1.62 | | | | tiering off / tiered / AOT |

The noise floor is the baseline measured twice: 12.10 / 11.80 ns with tiering off, 11.65 / 11.52 tiered,
7.37 / 7.59 AOT (≤ 3%).

**Fix 4's extrapolation holds:** a byte-serial FNV hit at 16 B costs MORE than today's copy in every mode.
Under NativeAOT, where the copy is cheapest, V8 is at or above today from 8 B up. The word hash fixes the
16 B hit, but as specified it stops at 16 B.

**Misses and the gate (fix 5 replaces the V6 evidence):**
- A non-literal 8 B span that differs every call: today 16.17 ns, V8 32.68 ns (2×, and it inserts),
  V10 17.68 ns. Tiered: 13.46 / 38.64 / 21.02. AOT: 10.58 / 26.34 / 12.52.
- A 24 B span over V8's gate pays 17.56 → 19.65 ns with tiering off (+12%), 15.56 → 20.94 tiered, and
  14.00 → 14.82 AOT.
- The gate is NOT inside the noise floor under either JIT mode. The draft's "inside this VM's spread" is
  withdrawn.

**Three contents thrashing one set (fix 2)**, found by search, so the collision is a deterministic function
of the bytes: 85.38 ns and 3 counted per round of three conversions (28.5 ns and 1 counted each; tiered 36.6,
AOT 20.3). That is 2.4× today's time with tiering off (3.2× tiered, 2.7× AOT) at today's count.

**Round 1's wording, corrected (fix 16):**
- "2 counted per call" was per two conversions: about 21.4 ns each, 2.2× today at the same count.
- "0 mismatches in 32,000,000" was 16M content-cache results plus 16M address-keyed ones, and the
  content-cache half could not fail by construction.
- Round 2's stress was 8 threads × 2M mixed hits and misses over V8, every result content-checked:
  **0 mismatches in 16,000,000, x64 only.** A way is published by `Volatile.Write` after its bytes are
  written, and x64's store order hides the reordering this check could catch. ARM64 is the leg that could
  show it, and it was not run.

**PerfStringMatch's `"// "u8` shape, 16M evaluations (fix 9):**

| mode | today | V8 2-way | V10 table | hoisted |
|:--|--:|--:|--:|--:|
| JIT, tiering off | 192.8 ms | 128.8 | 71.7 | 22.5 |
| JIT, tiered | 194.5 | 128.7 | 57.4 | 41.1 |
| NativeAOT | 120.9 | **137.9** | 69.4 | 25.9 |

That is the probe's loop, not the Performance suite, which stays unmeasured. Under NativeAOT, the leg the
suite publishes, the 2-way cache is slower than doing nothing.

**Share of the removable cost recovered (fix 17, corrected):** round 1's cache recovered 32% (arm A's shape)
and 28% (arm B's). Round 2's V8 at arm A's 1 B hit recovers 56% with tiering off. The table recovers
71-80% at every length with tiering off, and 54-66% under NativeAOT, where today's copy is cheapest.

### 8.1R.3 Determinism: a process-global cache under a per-thread counter (fix 6)

These points hold at once, each cited by the review:
- a miss inserts any span under the gate;
- the table is process-global while `AllocationCounter` is per-thread (AllocationCounter.cs:72-78);
- go2cs's `AllocsPerRun` floors a nonzero result at 1 (testing.cs:743, :755), where Go divides with no
  floor;
- the literal set that a measured function touches includes its callees' literals (slog handler.cs:389's
  `attrSep`).

**Measured with `--eviction`:** thread A converts one literal 2M times while thread B converts non-literals
that collide with its set.

| literal's way | V8 counted misses on thread A, per 1M | V10 table |
|:--|--:|--:|
| way 0 (the literal touched the set first) | 0.0, 0.0, 0.0 (three runs) | 0.0 |
| way 1 (a non-literal touched it first) | 91,439 / 92,639 / 91,594 (three runs) | 0.0 |
| way 1, the full round-2 run, tiered | 141,040 and 156,781 | 0.0 |
| way 1, the full round-2 run, NativeAOT | 168,433 and 168,005 | 0.0 |

**The first round-2 run labelled its arms "way 0" and "way 1".** Instrumenting with `Lit2.WayOf` showed
that BOTH literals were in way 1, because earlier rows of the same process had already filled way 0 of
their sets. The labels in `round2-tc0` are wrong and are kept as run. Every later run prints the way it
measured.

**An instrument trap, found and fixed:** the eviction-only arm first read 1.0 per 1M in way 0. That was 2
counted objects at iteration 0, which was Round2's own static constructor minting its two hoisted
`@string` fields on the first `Use`. It now runs outside the window.

**What this means:**
- A literal in way 1 misses about 9-17% of the time while another thread churns its set. Under the floor
  at 1, ONE such miss inside a window turns a row that wants 0 into a fail.
- Whether a literal is in way 0 depends on process history, which is not a property of the test.
- **Any row whose zero reading depends on the cache is statistical.** The draft's refusal of address keys
  on determinism grounds now weighs against the content cache too.
- The table reads 0 structurally: it takes no write after module init, so there is nothing to evict. That
  0 is a property of the design and was not stressed under contention.

### 8.1R.4 The count-to-bytes cliff (fix 7)

`AllocsPerRun` trusts the count only while it is nonzero (testing.cs:750-755: `countUsable = … counted > 0`).
A leg whose last COUNTED object goes away while uncounted bytes remain switches unit. It reports bytes per
run, floored at 1, and the reading RISES.

fmt TestCountMallocs's `Fprintf(buf, "%x")` legs (fmt_test.cs:1603-1607; go2cs_test_disclosures.json:9-10)
show it:
- Today a leg reads 2 against a want of 0: the `...any` pack's `slice()` copy plus the `"%x"u8` copy.
- The disclosure's planned non-copying pack view removes the first; any literal mechanism removes the
  second. Counted then falls to 0.
- The box of `(nint)65536` into `any` is still allocated. Boxing is uncounted by design
  (AllocationCounter.cs:53), and Go allocates nothing there for a constant.
- So the leg reads about 24 (bytes) against a want of 0 and fails louder than today.

The cliff is **common to every literal mechanism** (hoisting, the cache, the table, an `sstring`
parameter), because it is a property of the instrument. It is not a reason to prefer one mechanism. It is a
reason that each predicted row names what stays counted:
- log TestDiscard 2 → 1: log.cs:289's `...any` params copy stays counted, and it is Go's own allocation.
- log/slog 2_pairs … attrs9: the objects that stay counted are **not attributed in this draft.** They are
  C1's rows, and they are read before a prediction is scored.

### 8.1R.5 Footprint, callers, and the gate list (fixes 8, 9, 14, 15)

A golib or generator mechanism reaches literals the arms never touch:
- package-level lambdas and composites (fmt_test.cs:1566-1625);
- callee literals (slog handler.cs:385-389, which feeds TestTextHandlerAlloc);
- every package of every converted project, not only the corpus.

The two-seeded reconvert hunk gate reads **zero** for such a change. So before landing, on every OS lane:
- the full banked operational sweep;
- a re-read of every `AllocsPerRun` row and every disclosure that cites a count;
- the Performance suite under JIT AND NativeAOT (Performance/Directory.Build.targets:11);
- Windows and ARM64 legs;
- one `go2cs.slnx` build, the floor's rule after any golib API change.

That is §4.6's bar for Tier C, and it applies to the table (8.1.x) as much as to the cache.

**Entries the "two entries only" wiring does not cover (fix 14):**
- the 16 UTF-16 tuple-return sites that bind `operator @string(string)` (string.cs:450), e.g.
  path/filepath/windows/path.cs:193 and runtime/symtab.cs:959;
- `sstring`'s escape to `@string`, which goes through the copying constructor (sstring.cs:177-180).

The table's miss cost is small enough that the constructor could consult it too. That choice waits for the
sweep's measurement of the tiered miss (+7.6 ns, the one mode where it is not small).

**Guard the callers, not only the wiring (fix 15):**
- The operator is a public implicit conversion, so a golib test pins that a NON-literal span with a
  registered literal's bytes still copies. Under the table this is structural (the key is an image address)
  but still worth pinning. Under the cache it is the undercount hazard of 8.1R.6.
- DESIGN-string-byte-window stage 4 must not build its one-rune string through `(@string)span`.

### 8.1R.6 Hazards and corrections (fixes 11, 12, 13, 17, 18)

**Content-cache hazards the draft omitted (fix 13):**
- A NON-literal whose bytes repeat hits and is undercounted, which is a false pass (AllocationCounter.cs:64-69).
- After an eviction, two evaluations of one literal give DIFFERENT `unsafe.StringData` pointers.
- Public writable views (string.cs:251-281) widen what a mutation reaches. Making `ToSpan` read-only is a
  separate cleanup.
- Retaining non-literal inserts contradicts unsafe.cs:1088-1099's rationale.

The table has none of the four: non-literals never enter it, nothing evicts, and one literal's pointer never
changes.

**Corrections:**
- **Retention (fix 17):** about 128-160 KiB of arrays plus a 32 KiB table for 4,096 ways, not "≈ 64 KiB".
- **The draft's "15 distinct literals":** 12 go through the operator per call (`n s d a b c e f %s Atoi
  ParseInt ParseUint`), plus `""`, `hello` and `two`.
- **Fix 18:** only three sites bind the :92 span constructor: string.cs:151, sstring.cs:179, and string.cs:462
  (the operator). The `IArray<byte>` constructor (:162) chains to it. The format-position test is at
  hoistedLiteralOperations.go:595.
- **Fix 11:** `Bytes` already branches on a null backing (string.cs:66), so a pointer-backed `@string` adds
  no new branch. It still adds a field (16 → 24 B per `@string`), and 8.1.3 (7)'s conclusion stands.
- **Fix 12:**
  - Arm A would not pre-box `n`, `s` or `d`: `preBoxed` requires `valueUses == 0`
    (hoistedLiteralOperations.go:720), and log/slog has value uses (text_handler_test.cs:194,
    value_test.cs:269, handler_test.cs:560).
  - The probe's 72 B pre-boxed figure used a `params object[]`, where real code passes `Span<any>`
    (logger.cs:241-242).
  - Arm A's real any-slot cost is unmeasured, and its advantage over the cache there is smaller than the
    draft said.

### 8.1R.7 The -annotations census, corrected (fixes 19-22)

The draft's 8.1.5 table counted HEADER lines where COORD counted BLOCK lines. COORD's figures are cited here
as COORD's, and they replace the draft's:
- denominator 3,912 files, not 3,925 (the draft did not record the predicate behind its figure);
- package_init prose 375 lines, not 97;
- metadata anchors 1,890 lines, not 333;
- package_info prose about 21.8K lines, not about 15K;
- a default flip moves about 29K lines.

The emitter list gains testConversion.go:1604, cgoDynamicImports.go:217 and refVerdictPublication.go:69.

**Churn outside src/core (fix 20, COORD's figures):**
- 399 `.cs.target` goldens (775 lines) and 15 Performance `.cs` files;
- docs/README.md:334, ConversionStrategies.md:851 and ConversionStrategies-Reference.md:6968;
- the hand-owned runtime/mranges_impl.cs, which a flip does not touch;
- drift in every disclosure that cites a C# `path:line`.

The flag must also reach the `-tests` seed and `check-no-regression.ps1`.

**The prose-reader check already fails (fix 21).** packageInfoWriter.go:236-242 copies existing lines
through verbatim, and `migrateProseBlock` finds a block by its first prose line (:36, :117-150). So:
- changing the emitters alone leaves the committed prose in place, and the seat needs a strip pass;
- once stripped, `verbose` cannot restore the prose in files already written unless the writer re-inserts it
  on every emission. That is a design decision for the seat, not a flag default.

**Never governed, extended (fix 22):**
- the 225 `go2cs_test_host.cs` `DO NOT EDIT` headers (testConversion.go:4275);
- both `funcPlaceholderLead` readers: platformHandOwn.go:384 and manualConversionDestination_test.go.

The `/* expr */` const-echo family (convBinaryExpr.go:135, :961; convCallExpr.go:6170; about 1.8-2.1K lines,
COORD's count) is Go's own source text shown beside the folded C# value. It is **on at `normal`, off at
`quiet`**, the same class as the end markers.

## 8.1.x The module-init LITERAL REGISTRATION TABLE, sized (fix 10)

**The mechanism.** The go2cs generator already runs on every converted project, and already emits a
`[ModuleInitializer]` (AdapterImplTemplate.cs:108-113). It would emit one more, listing every distinct u8
literal of the compilation:
- `Register("n"u8); Register("%s: %v"u8); …`
- golib keeps one process-wide table keyed by (address, length).
- The operator (string.cs:460) looks the span up. A hit returns the literal's one `@string`; a miss copies
  exactly as today.

**The first probe: does Roslyn store identical u8 data once per module? YES.** `probes/c2-rva-dedup/` gives
the same results under JIT and under NativeAOT:
- the same address from two methods, from a nested type, and from `Generic<int>` and `Generic<string>`;
- the same address for 1 B and for 32 B literals;
- stable across calls;
- no prefix sharing: `"abc"` and `"abcd"` start at different addresses, and `"xabc"u8[1..]` is not
  `"abc"`.

So the one `Register("n"u8)` in the initializer names the very address that every `"n"u8` in the module
hands the operator. The C# specification does not promise this; Roslyn does it. A golib test pins it, so a
compiler change fails loudly instead of silently turning every hit into a miss.

**Measured (V10):**
- Hit: 3.7-4.6 ns at every length from 1 to 32 B in all three modes, and 0 counted. That is 2.7-3.6× cheaper
  than today under JIT and 1.7-2.2× under NativeAOT, and 1.2-3.3 ns above a hoisted field.
- Miss on a non-literal: +1.5 ns with tiering off, +7.6 tiered, +1.9 AOT.
- It never inserts, so a miss allocates exactly today's one object.
- Eviction and thrash cannot occur.
- It has no length gate.

**Why an address key is acceptable here when 8.1.3 (4) refused one.** That refusal was about COLLISIONS in
a lossy cache, where ASLR would change which literals collide from run to run. This table is exact: it
compares the full key and never evicts. The address differs per run, but it is registered in the same run,
so every registered literal is found in every run. Membership is run-invariant.

**How big it gets.** Measured at this branch's tree: the regex `"…"u8` over 4,045 tracked `src/core` `.cs`
files (golib excluded), grouped by directory, which approximates one assembly per directory:

| scope | directories | occurrences | distinct per directory, summed | median | p90 | max |
|:--|--:|--:|--:|--:|--:|--:|
| production | 443 | 27,809 | 20,584 | 6 | 103 | 2,230 (html) |
| tests | 225 | 86,887 | 50,102 | 63 | 607 | 3,805 (net/http) |

A process registers only the modules it loads.

**Module-init cost at the largest module**: `--regcost`, 3,805 distinct literals, 1-40 B, one generated
method, eager values:

| mode | first call | work alone (second call) |
|:--|--:|--:|
| JIT, tiering off | 46.5-69.3 ms | 0.15-0.24 ms |
| JIT, tiered | 46.0-47.1 ms | 0.26-0.27 ms |
| NativeAOT | 1.18-1.48 ms | 0.10-0.13 ms |

Under JIT the cost is almost all the JIT of one 3,805-call method, paid once per test-assembly process.
The p90 production module (103 literals) scales to about 1-2 ms. The shape that would cut the JIT cost is to
register only the literals the semantic model shows reaching an `@string` conversion, which the generator
can see. That filter is unmeasured.

**Eager or lazy values:**
- Eager (the probe): one `byte[]` per literal at init, uncounted and outside every window.
- Lazy: the entry holds only the key, and the first hit publishes the array with a CAS. The first
  evaluation then costs one counted copy. AllocsPerRun's warm-up run absorbs it (testing.cs:725), but a
  single-shot measurement would not.
- Eager is recommended: memory is bounded by the module's literal bytes, and the count never moves.

**Concurrency:**
- Registration takes a lock. Modules initialise on whichever thread first touches them.
- A registration that grows the table publishes a new table; that is the only write.
- Reads take no lock.

**Hazards:**
- A non-literal never matches, because its address is heap or stack memory, never the module image. So it
  is never undercounted.
- A write through `unsafe.StringData` corrupts that literal for every later evaluation. The hoisted field
  has the same hazard; the content cache has a wider one (non-literals too). It is Go UB, and Go faults.
- Requires non-collectible modules, which is the default; go2cs uses no collectible load context.

**Gates:**
- 8.1R.5's full list;
- a generator test that the initializer lists every distinct literal;
- the golib dedup pin above;
- NativeAOT's module-initializer order checked against the first literal use.

## 8.2 THE sstring-FIRST MODEL -- a view where Go would not allocate

`sstring` is a ref struct view over UTF-8 bytes (sstring.cs:19-46). C# enforces most of Go's escape rules
on it at compile time: it cannot be boxed, stored in a field, array or map, captured by a lambda, or used as
a type argument. A literal viewed as an `sstring` is Go's RODATA string: zero allocations and zero lookup
at ANY length. That is why the owner asked for this model first.

### 8.2.1 The parity target: Go 1.24.13's no-copy rules, read in cmd/compile

| # | idiom | Go 1.24.13 | where |
|:--|:--|:--|:--|
| G0 | a string literal | never allocates (RODATA) | — |
| G1 | `m[string(b)]` read, and `m[T{…, string(b), …}]` with a nonempty literal field | zero-copy view, any length | walk/order.go:321-345 |
| G2 | `string(b)` as an operand of `== != < <= > >=` | zero-copy view, any length | walk/order.go:1410-1425 |
| G3 | `switch string(b)` with side-effect-free cases | zero-copy view, any length | walk/switch.go:55-68 |
| G4 | a non-escaping `string(b)` (a local, or `range string(b)`) | 32 B stack buffer: free ≤ 32 B, heap above | walk/convert.go:229-243 |
| G5 | a non-escaping concatenation (e.g. inside a comparison) | 32 B stack buffer for the result | walk/expr.go:483-495 |
| G6 | `range []byte(s)` (the reverse direction) | zero-copy | walk/order.go:870-880 |
| — | `m[string(b)] = v` | copies (the key is stored) | — |

`range string(b)` is G4, not a zero-copy site. `m[string(b)] = v` copies in Go too, so a write is never an
opportunity.

### 8.2.2 Where go2cs emits sstring today, and the census of where it could

**Today, at this branch's tree:**
- 381 `((sstring)x)` sites and 3 `sstring` locals, in 154 files (54 production);
- emitted by three passes plus a hoist: `markSStringEligible` (escapeAnalysisOperations.go:1352),
  `markSStringBinaryOperandConversions` (:1568), `markSStringSwitchConversions` (:1599), and
  `planSStringHoists` (sstringHoistOperations.go:56).
- All three require an unnamed `[]byte` source that is never written while the view is alive.
- No literal is ever an `sstring` today.

**Census of the idioms** (Go source of `std` plus tests, the 8.1R.1 walk; a site is a `string([]byte)`
conversion):

| idiom | production | tests | go2cs today | gap to Go |
|:--|--:|--:|:--|:--|
| G1 `m[string(b)]` read | 25 | 5 | copies | **full**: golib `map` has no span lookup |
| (`m[string(b)] = v`) | 11 | 4 | copies | none: Go copies too |
| G2 `string(b)` compared | 53 | 280 | view (both operands safe) | residual: operands the safety test refuses |
| G5 concatenation inside a comparison | 1 | 30 | operand viewed, result allocates | result ≤ 32 B |
| G4 `range string(b)` | 0 | 0 | — | — |
| G3 `switch string(b)` | 7 | 1 | view | none |
| `string(b)` as a call argument | 111 | 379 | copies | needs an `sstring` parameter (8.2.3) |
| `string(b)` bound to a name | 99 | 225 | view only if every use is a safe read | G4 (≤ 32 B) and beyond |
| `string(b)` returned | 135 | 42 | copies | none: it escapes |
| elsewhere | 14 | 26 | copies | case by case |

The biggest population is not in this table: **literals passed to string PARAMETERS**. Go's escape
analysis is the oracle for those. The measurement is `go build -a -gcflags=-m std` (GOOS=windows,
go1.24.13; 136,781 lines), joined by declaration position to every string parameter and to every literal
argument:

| | noescape | leaks to heap | leaks to result | no verdict |
|:--|--:|--:|--:|--:|
| production string PARAMETERS (919 functions have ≥ 1 noescape string param) | 1,115 (40%) | 1,379 | 249 | 19 |
| literal args → string params, production | 4,835 (2,491 format-position) | 3,155 | 73 | 47 |
| literal args → string params, tests | 18,711 (16,655 format-position) | 6,080 | 884 | 3,248 |

So 60% of production literal arguments, and 65% of test ones, flow into a parameter Go proves does not
escape. That is the reach of 8.2.3.

### 8.2.3 The opportunities, with the proof each needs and its visible delta

**O1 -- `sstring` PARAMETERS for proven non-escaping, non-materializing string parameters.**
- **The change:** the callee's `@string format` becomes `sstring format`.
  - A literal argument binds `"%s"u8` with no copy at any length, which is Go's G0 exactly.
  - An `@string` argument converts implicitly and free (sstring.cs:172).
  - An `sstring` argument passes through.
- **Go's verdict is NECESSARY, NOT SUFFICIENT.** C# forbids four things Go's analysis allows for a
  non-escaping value:
  - capture by a closure: `log.Printf`'s `format` is noescape in Go but captured by the closure it hands
    `l.output`;
  - conversion to `any`: Go's analysis tracks the interface value, while C# boxes;
  - use as a generic type argument;
  - a store into a struct local's field.
  
  Each is a compile error, not a silent bug, which is the property that makes this safe to attempt.
- **The cost that decides it:** every use that must MATERIALIZE, meaning an `sstring` passed onward to an
  `@string` parameter or into `any`, copies. Today an `@string` parameter is passed on for free. So O1 is a
  win only where no materializing use runs on the hot path. The predicate is a FIXED POINT over the call
  graph, with the same shape as Go's own analysis.
  - Example: fmt's format chain is noescape at every link: `Sprintf` print.go:237, `Fprintf` :222,
    `doPrintf` :1019, `buffer.writeString` :107. Its `doPrintf` also passes `format` to its parse helpers
    and `utf8.DecodeRuneInString`, and each of those needs the same verdict.
  - Across packages, the verdicts are published the way `refVerdictPublication.go` already publishes ref
    verdicts.
- **The one golib obstacle:** `ReadOnlySpan<byte> → sstring` is EXPLICIT (sstring.cs:182-188), because an
  implicit form makes `"…"u8 == x` ambiguous against `@string`'s implicit span operator (CS0034).
  - With it explicit, every literal call site would read `(sstring)"%s"u8`. That visible cast per site is
    rejected.
  - **Proof needed first:** flip the operator to implicit, with exact-match comparison operators for every
    pair that turns ambiguous (Tier A's span operators are the precedent), and build the corpus clean.
- **Visible delta, if that proof holds:** the callee's parameter type (`sstring format` for
  `@string format`). The vocabulary is already in 154 files, and call sites do not change.
- **Reach:** up to 4,835 production literal arguments, 2,491 of them in the format position arm B holds.
  The share that survives the C# filters and the materialization fixed point is **unmeasured**. It is the
  first census to run.

**O2 -- G1, the map read.**
- `map<@string, V>` gains `this[sstring key]` over .NET's alternate lookup (`Dictionary.GetAlternateLookup`,
  whose comparer takes `IAlternateEqualityComparer<ReadOnlySpan<byte>, @string>`; golib's map is a
  `Dictionary` with its own comparer, map.cs:157).
- The converter marks `m[string(b)]` reads as it already marks comparison operands. The key is never
  stored, so no further proof is needed.
- **Visible delta:** `m[((sstring)b)]`, the shape already emitted at comparisons.
- **Reach:** 25 production, 5 test.
- **Parity:** exact, any length.

**O3 -- a literal consumed only by non-escaping uses**, e.g. `s := "abc"` used only by reads, or a
function-local const.
- `sstring s = (sstring)"abc"u8;`. It needs O1's implicit operator to drop the cast.
- **Visible delta:** the local's type.
- It overlaps Tier C's hoisting, which already reaches these sites at 1.3-2.6 ns with no type change, so
  its gain is parity at ANY length without a field. It ranks below the table.

**O4 -- G4, widening `markSStringEligible` past "safe reads"** to the oracle's `string(b) does not escape`.
- Go stops at 32 B, and an sstring view has no limit, so above 32 B go2cs would allocate LESS than Go. That
  is harmless for rows that assert upper bounds, and it is stated here so it is not mistaken for parity.
- **Reach:** up to 99 production / 225 test bindings, before the oracle filters them.
- Most of the call-argument sites (111 / 379) need O1 to be useful.

**O5 -- G5, the concatenation result inside a comparison.**
- A golib comparison over the parts (`a + b == c` without building `a + b`) would need an emitted helper,
  which is a visible delta, for 1 production site.
- Ranked last.

### 8.2.4 Arms A, B and C against an sstring view

| arm | member | served by a view? | what remains at the boundary |
|:--|:--|:--|:--|
| A | log/slog `...any` keys `n s d` | **no**: each key is boxed into `Span<any>` | the box, plus today's copy unless the table removes it |
| A | log/slog LogAttrs keys `a`-`f` | **no**: `slog.String(key, …)` stores `key` in `Attr.Key` (Go: leaks) | the copy; the table removes it |
| B | log TestDiscard `Printf("%s", …)` | **no, without restructuring**: Go-noescape, but captured by log.go's closure | the copy; the table removes it |
| B | fmt TestCountMallocs's format literals | **yes, through O1** (the fmt chain above) | the pack and the box (8.1R.4's cliff) |
| C | strconv `fnAtoi` / `fnParseInt` / `fnParseUint` | on the success path; the error path materialises into `NumError` (Go stores the literal pointer there for free) | the error path's copy; the approved hoist has none |

**The cost at the `any` / `@string` boundary**, where a view must materialize, is exactly today's: one
counted copy (about 11-15 ns) plus, at `any`, the uncounted box. Round 1 measured the log/slog pack at
86 ns / 264 B / 3 counted today, against 78 ns / 168 B / 0 counted with the copy removed.

### 8.2.5 Ranking: parity gain × corpus reach

| rank | opportunity | parity gain | reach (production / tests) | visible delta | proof still owed |
|:--|:--|:--|:--|:--|:--|
| 1 | O1 `sstring` parameters | exact G0 at any length on the hot path | ≤ 4,835 / 18,711 literal args | callee param type | implicit-operator build; C#-filter census; materialization fixed point; cross-package verdicts |
| 2 | O2 map read | exact G1 | 25 / 5 | `((sstring)b)` at the index | none beyond the golib overload |
| 3 | O4 wider local views | G4, and beyond it at > 32 B | ≤ 99 / 225 | local's type | oracle join per site |
| 4 | O3 literal locals | G0, where hoisting already gives count parity | overlaps Tier C | local's type | O1's operator |
| 5 | O5 concat in comparison | G5 | 1 / 30 | emitted helper | — |

## 8.3 RECOMMENDATION -- three tiers, in order

**Tier 1: sstring where it is provable.**
- Land O2 first (small, exact, and the cast shape already exists).
- Then take O1 in three steps, each on its own seat:
  - (i) the implicit-operator build proof;
  - (ii) the census joining Go's `-m` verdicts to the C# filters and the materialization fixed point;
  - (iii) the converter change on the population (ii) proves, with its two-seeded footprint.
- *Readability:* the parameter type reads `sstring`, a Go-string word, and call sites stay as Go wrote them.
- *Performance:* no allocation, no lookup.
- *Parity:* Go's own rule, at any length.

**Tier 2: the registration table (8.1.x) for everything a view cannot reach.** That covers literals stored,
boxed, returned, or passed to leaking parameters.
- **The content cache is retired**: the table beats it on every axis measured:
  - hit at 16 B: 3.8-4.6 ns against 14.5-14.8;
  - miss tax: +1.5-7.6 ns against +15.8-25.2;
  - exactness: never evicts, never undercounts;
  - coverage: no gate, so all 15,806 long format literals and 374 long degenerate literals are in.
- It keeps the cache's one virtue: zero visible delta, with no converter change at all.
- Its costs are:
  - a generated initializer, about 47 ms under JIT at the largest test assembly and 1.2-1.5 ms under
    NativeAOT;
  - eager memory bounded by the module's literal bytes;
  - a dependency on Roslyn's u8 deduplication, pinned by a test.
- *Readability:* unchanged.
- *Performance:* 1.7-3.6× faster than today by mode, 1.2-3.3 ns short of a field.
- *Parity:* COUNT parity at every `@string`-typed site at any length (the `any` box remains).
- **Predictions (UNMEASURED on the members):** §8's counts hold: log/slog 2_pairs 10 → 8, … attrs9 28 → 19;
  log TestDiscard 2 → 1. Its sweep footprint is wider than §8's members, and 8.1R.4's cliff is named in
  advance at fmt's `%x` legs.

**Tier 3: hoisting, where it is already ruled or where time matters.**
- Arm C lands as approved: its own name, 1.3-2.6 ns, and no naming decision reopened.
- Tier C as landed stays.
- Arms A and B are **not needed for count parity** once Tier 2 lands. Their positional names would buy only
  the 1.2-3.3 ns between a table hit and a field.
- *Readability:* a hoisted name per literal.
- *Performance:* the fastest form measured.
- *Parity:* count parity (identity parity too: one backing per literal).

**Ordering, and what each step costs the owner's three axes:**
- Tier 2 alone gives count parity everywhere at once with no visible change, so it is the step to take
  first if only one is taken.
- Tier 1 is the step that makes the converted code BEHAVE like Go's compiler rather than compensate at
  runtime, and it is the only tier that is free at run time. It is a converter campaign sized by its own
  census, not a draft-sized change.
- Tier 3 is the performance tier and stays where the owner has already accepted its names.

## 8.1R2 REVISION 3, 2026-09-24 (C2) -- the verification's 23 fixes, the start-up A/B, and §8.2 rewritten

**DRAFT, UNMERGED**, same branch. Input: COORD's verification of revision 2
(`docs/phase4/reviews/literal-revision-2-verification-2026-09-24.md` on `claude/coord-handover` at
`545eacef7f`; "V-fix N" below is its Part 2 item N). This block supersedes, where it says so, 8.1R,
§8.1.x's start-up and memory paragraphs, §8.2 and §8.3. Nothing above is rewritten.

**Legs.** Everything here ran on the shared 4-core linux VM, locally, unofficial, not a gate. Not run:
- Windows and ARM64;
- the Performance suite and the operational sweep;
- the net/http TEST process (8.1R2.1 says why, and what stood in for it).

### 8.1R2.1 Start-up, measured as a sum over the forced closure (V-fixes 2, 3, 4, 5)

**The mechanism, confirmed by measurement.** A program's import hooks force every module of its import
closure at start (COORD's reading: fmt/package_info.cs:113-122, builtin.cs:239-242). The probe's arm B
registered 3,092 of the 3,095 literals it generated for an fmt hello world on its first run. So every
module initializer in the closure ran, and the cost is a sum over the closure, not "the largest module".

**Method** ([`probes/c2-startup-ab/`](probes/c2-startup-ab/README.md)). Two programs, converted by this
tree's converter and built for linux (`-p:GoTargetOS=linux`):
- an fmt hello world;
- a program that imports net/http and calls two of its functions.

**Arms:**
- A is today's golib and corpus.
- B adds golib's registration table (`LiteralTable.cs`), makes the span operator consult it, and gives
  every closure module a generated `[ModuleInitializer]` that registers that module's distinct regular
  `"…"u8` literals (`genreg.py`).
- Boff is B with `Register` returning at once. The initializers still run, so B − Boff is the table's own
  work and Boff − A is the generated initializers' cost.

**Timing:** wall time from exec to exit, 31 runs per arm after 3 warm-ups, arms interleaved in alternating
order, load average about 1.0 at the start. ReadyToRun is the template's own publish (csproj
`PublishReadyToRun`/`PublishTrimmed`), verified by the images' ManagedNativeHeader.

| program (literals registered) | regime | A | Boff | B | B − A | initializers (Boff − A) | table (B − Boff) |
|:--|:--|--:|--:|--:|--:|--:|--:|
| fmt hello world (3,092, 55 modules) | tiered JIT | 375.4 | 420.5 | 416.2 | **+40.8 ms** (+10.9%) | +45.0 | −4.2 |
| | TC=0 (the test host's regime) | 629.7 | 668.5 | 687.7 | **+58.0 ms** (+9.2%) | +38.8 | +19.2 |
| | ReadyToRun publish | 199.7 | 196.7 | 199.8 | **+0.2 ms** (min +4.9) | −3.0 | +3.1 |
| net/http program (6,323, 168 modules) | tiered JIT | 1,037.7 | 1,092.8 | 1,101.1 | **+63.4 ms** (+6.1%) | +55.1 | +8.3 |
| | TC=0 | 1,719.5 | 1,799.7 | 1,821.8 | **+102.3 ms** (+5.9%) | +80.2 | +22.1 |
| | ReadyToRun publish | 438.6 | 432.1 | 440.1 | **+1.6 ms** (min +4.0) | −6.4 | +8.0 |

**What the numbers say:**
- **The cost is the generated initializers, not the table.** `Register`'s own time, stopwatched inside
  it, is 1.1-6.9 ms per process. The rest is compiling one initializer method per module, whose IL
  resolves one data token per literal: about 9-15 µs per literal under JIT (Boff − A over the
  literal count).
- **ReadyToRun removes it:** +0.2 ms and +1.6 ms median.
- Under TC=0, the test host's regime, the tax is +58 ms for a hello world and +102 ms for a net/http
  program.
- **COORD's inference (20-60 ms under JIT) holds** for the hello world and understates a larger closure.

**The net/http TEST process was not measured, and here is why.** Its test project does not build for
linux at this tree:
- its Windows-generated `package_test_info.cs` aliases two Windows-only syscall types;
- after dropping those two unused aliases in the scratch copy, `http_test.cs` calls `testenv.HasSrc`,
  which no flavour of internal/testenv defines here.

The program above has net/http's production closure, the dominant part of the test process's forced
closure. The test process itself, with its test-only modules, is owed on a Windows lane.

**V-fix 4:** §8.1.x's "work alone (second call) 0.15-0.24 ms" is the EARLY RETURN for literals already
registered (Caches2.cs's dedup test), not registration work. NativeAOT's first call, 1.18-1.48 ms for
3,805 literals, is the cost without the JIT.

**Mitigations, sized (V-fix 5):**
- **ReadyToRun or NativeAOT:** measured above; this is the one that removes the cost.
- **Registering only literals that reach an `@string` conversion:** at most about 3.5% fewer (COORD's
  figure: 121 of 3,419 closure literals are used only in comparisons). Not worth a semantic-model pass.
- **A lazy per-module-range variant** (no per-literal registration; a span inside a module's read-only
  image data is shared on first use):
  - It removes the initializers entirely.
  - A path first taken inside an `AllocsPerRun` window counts 1 (testing.cs:725-755). The warm-up call
    absorbs it, but a single-shot measurement would not.
  - Discovering the image range under single-file and AOT builds is unmeasured.
  - Named, not recommended.

### 8.1R2.2 The table as it would ship (V-fixes 8, 9, 15-21, 23)

**Growth (V-fix 20):**
- The round-2 table was a fixed 1<<16 slots and spins forever when full.
- The probe's table now GROWS: it starts at 16 slots, doubles, and publishes the grown table with one
  volatile store. A replaced table is never written again, so a reader holding it sees a consistent
  older index.
- **Load factor is the knob.** One axis only: the lookup is inlined in both arms (`C2_TABLE_LOAD`), with
  two runs each at tiering off. The spread within a run is per-literal probe length, from address
  collisions, not literal length.

| load | hit ns (1-32 B) | miss on a non-literal, over today | index bytes per literal |
|:--|--:|--:|--:|
| at most half full | 4.8-8.5 | +4.0 to +6.1 ns | 40 |
| at most a quarter full | 4.9-6.2 | +2.5 to +2.7 ns | 80 |

- **Concurrent growth** (`--stress10`, x64): four writers register 50,000 keys through 13 growths while
  four readers look up. In 369K-713K hit lookups there were 0 wrong bytes and 0 counted objects. In as
  many miss lookups there were 0 wrong bytes. ARM64 is not run.

**Memory, honestly (V-fix 20).** A 64-bit `byte[]` costs 24 B plus its data rounded up to 8.
- **fmt hello world:** 3,094 literals, 52 KiB of data in 136 KiB of arrays. The index is 20 B per slot:
  160 KiB at half load, 320 KiB at quarter load. Total 296-456 KiB.
- **net/http program:** 6,325 literals in 303 KiB of arrays, plus 320-640 KiB of index. Total 623-943 KiB.

**Inlining (V-fix 19):**
- At TC=0 the span operator AND the table lookup inline into the caller. The probe loop (`Find`) stays a
  call.
- One hoisted-literal static constructor grew from 161 to 283 bytes of code.
- The shipped form decides: `NoInlining` on the lookup keeps every conversion site small and costs one
  call.

**Initializer order (V-fix 15).** Generator initializers run AFTER package_info.cs's import hooks and after
the package's own `init()` (COORD: os.csproj:147-151, fmt.csproj:140-153). A literal evaluated during init
therefore misses and copies, counted exactly as today, and later evaluations share the table's backing.
- **Design:** package_info.cs's first-position hook calls a generated `partial` method that registers the
  module's literals. An empty body applies when the generator is absent.
- **Gate:** the order, under JIT and NativeAOT.
- **Withdrawn until then:** 8.1R.6's "one literal's pointer never changes".

**The deduplication dependency, hardened (V-fix 16).** `probes/c2-rva-dedup/` now also pins a literal in a
SECOND source file. It holds in Debug, in Release and in a ReadyToRun publish (SDK 10.0.112, runtime
10.0.12), in addition to the earlier NativeAOT leg.
- **Still owed:**
  - a literal emitted by a source generator, against one in a user file;
  - Windows.
- **Design:** a per-module sentinel self-check. The initializer registers one sentinel literal and looks
  it up from a method in another file. On a mismatch it drops that module's registrations, once, and
  counts fall back to today's.

**Collectible load contexts (V-fix 17).** Skip registration when the module's `AssemblyLoadContext` is
collectible, or purge its keys on `Unloading`. A stale (address, length) key after an unload could
otherwise return a wrong string.

**Writable views (V-fix 18).** 8.1R.6 said the table has "none of the four" hazards. That is reconciled
with public `Slice()`/`ToSpan()` (string.cs:251-281), which hand out WRITABLE views over a shared
backing. Making them read-only is a precondition of Tier 2. Until then, a write through one corrupts that
literal for every later evaluation, which is the same hazard a hoisted field carries.

**Wired entries (V-fixes 8, 9):**
- the span operator (string.cs:460);
- `sstring`'s escape to `@string` (sstring.cs:177-180), which otherwise undoes the table for a literal
  that passes through a view;
- InheritedTypeTemplate.cs:390, routed through `(@string)value` instead of the copying constructor (a
  live bypass: reflect/all_test.cs:6683, `Tag: "s"u8`).

§8.3's "everything a view cannot reach" holds only with the second of these.

**Predictions are upper bounds (V-fix 21).** Every §8 count prediction reads "≤". At rollout Tier 2 can
flip a PASSING row to a byte reading: its last counted object goes, uncounted bytes remain
(testing.cs:743-755), and the reading rises.

**Interning by content (V-fix 23, optional).** In the hello-world closure, 114 literal contents appear in
more than one module, 184 extra copies in all. Interning the eager values by content at registration
gives Go's identity across modules for about that many lookups at start-up. Recorded, not recommended
until the order gate (above) exists.

### 8.1R2.3 Corrections (V-fixes 4, 22)

- **§8.2.4's log/slog pack figures:** 86 ns / 78 ns were an uncommitted run. The committed output reads
  62.94 ns / 264 B / 3 counted today and 63.36 ns / 168 B / 0 counted with the copy removed
  (output-linux.txt:23, :25).
- **Ratios:**
  - The JIT hit ratio reaches 3.79× (tiered: 14.16 / 3.74 at 32 B).
  - NativeAOT's recovery at 8 B is 53%, not 54-66%.
- **The tiered tax:** +7.56 ns on the miss row and +7.76 ns on the gate row, not "+7.6".
- **`sstring` locals:** there are 4, not 3 (serve_test.cs:6405, platform_test.cs:31, string_test.cs:120,
  nettest.cs:73).
- **G1's condition:** `mapKeyReplaceStrConv` (order.go:337-363) recurses into struct and array literal
  keys with NO nonempty-literal rule, and its read-only gate is `!n.Assigned` (order.go:1209-1214). The
  `haslit && hasbyte` test belongs to a different rule, concatenation (order.go:1185-1197, row G7 below).

## 8.2R THE sstring-FIRST MODEL, REWRITTEN AROUND THE VERIFIED GAPS (supersedes §8.2)

**The census is committed.** [`probes/c2-escape-join/`](probes/c2-escape-join/main.go) (V-fix 13) holds:
- the program;
- its outputs for windows, linux and darwin;
- a per-package digest of the three `-gcflags=-m` outputs (about 137K lines each).

**Exclusions are counted, not dropped** (production, windows):
- 2,747 literal arguments with no callee declaration (func values, conversions, generic instantiation
  calls);
- 30 to interface methods;
- 249 in variadic tails.

**Caveats:**
- Test sites carry no expression verdict: `-m` over `std` does not compile test files.
- A verdict describes Go's function, not a hand-owned go2cs file.
- linux and darwin differ from windows by under 1% in every row.

### 8.2R.1 Go 1.24.13's no-copy rules, the complete list read in the runtime and compiler

| # | rule | Go | go2cs today | production reach (windows) |
|:--|:--|:--|:--|--:|
| G0 | a string literal | RODATA, never allocates | Tier C hoist, or one copy per evaluation | §8's census |
| G1 | `m[string(b)]` READ, recursing into struct/array literal keys | zero-copy (order.go:337-363, :1209-1214) | **covered**: `mapReadTmpStringKey` emits `tmpstring(b)` (convIndexExpr.go:388-437; since 2026-08-12) | 25 of 25 reads covered; composite keys 0 (2 in tests) |
| G2 | `string(b)` compared | zero-copy (order.go:1410-1425) | covered: sstring operand view | 53 |
| G3 | `switch string(b)` with side-effect-free cases | zero-copy (switch.go:55-68) | covered | 7 |
| G4 | a non-escaping `string(b)` | 32 B stack buffer (convert.go:229-243) | a local view only if every use is a safe read | ≤ 99 bindings |
| G5 | a non-escaping concatenation | 32 B stack buffer for the result (expr.go:483-495) | the result allocates | 61 two-operand + 33 multi-operand non-escaping sites |
| G6 | `range []byte(s)` | zero-copy (order.go:870-880) | — | — |
| G7 | `string(b)` operand of a concatenation that has a nonempty literal | zero-copy operand (order.go:1185-1197) | covered: sstring operand view | — |
| **G8** | a concatenation with ONE non-empty operand | returns that operand, no allocation (runtime/string.go:46-51) | **allocates** (string.cs:718-755) | dynamic |
| **G9** | a multi-operand concatenation | ONE allocation for the whole chain (concatstrings) | one per `+` (e.g. time/time.cs:346) | **369 sites** (334 escaping) |
| **G10** | a one-byte `string(b)` | `staticuint64s`, no allocation (runtime/string.go:144-146) | allocates | dynamic |
| **G11** | `string(r)` of an integer, non-escaping | 4-byte stack buffer (runtime/string.go:291-298; convert.go:260-266) | allocates (string.cs:502-505) | 16 of 38 non-escaping; `string(byte)` 1 of 3 |
| **G12** | read-only `[]byte("lit")` | zero-copy of the literal (escape.go:336-346) | a copy per evaluation | **216 sites** (36 proven non-escaping) |

### 8.2R.2 The opportunities, ranked by parity gain × reach, each with its visible delta

**1. G8, the empty-operand concatenation. golib only, no visible delta.**
- `operator +` returns the other operand when one side is empty. golib's `@string` is always immutable
  heap or literal data, so Go's "not on the stack" condition always holds.
- **Gain:** one object per such evaluation, Go-exact.
- **Reach:** dynamic, so unmeasurable statically. The cost is one length test per concatenation.
- **Ranked first on cost and exactness.**

**2. G10, one-byte strings. golib only, no visible delta.**
- A 256-entry static table of one-byte `@string`s, returned by every byte-slice or one-byte conversion
  of length 1.
- **Gain:** one object per evaluation, Go-exact.
- **Reach:** dynamic.
- Extending it to `string(r)` for r < 0x80 would allocate LESS than Go for an escaping `string(r)`.
  That is over-parity, harmless for the upper-bound asserts the rows use, and stated so it is not
  mistaken for parity.

**3. G9, multi-operand concatenation. Converter, VISIBLE.**
- **Gain:** k − 2 objects per evaluation of a k-operand chain, at every one of **369 production sites**.
  That is the largest reach×gain in this list.
- **Visible delta:** `a + b + c` must become one call (e.g. `concat(a, b, c)`). C# evaluates `+`
  left-associatively, and golib cannot accumulate without changing the expression's type, which `var`
  would then leak.
- It is the owner's readability call. Its members are rows that assert a count on a concatenating path.

**4. G12, read-only `[]byte("lit")`. Converter, VISIBLE, needs a proof.**
- A hoisted `static readonly` backing per literal, shared by every evaluation.
- It is sound only where Go's read-only analysis holds: no write through the slice, no escape to a
  writer. That is a converter proof of the same class as V-fix 10's.
- **Reach:** 216 production sites, 36 of them proven non-escaping by `-m`. The read-only half is not in
  `-m`'s output and is unmeasured.
- **Visible delta:** a hoisted field name, Tier C's shape.

**5. G11, `string(r)`.**
- A non-escaping `string(r)` would need a 4-byte stack view: an `sstring` over a `stackalloc`, a visible
  type.
- **Reach:** 17 non-escaping production sites.
- Opportunity 2's ASCII extension covers most of the value.

**6. G4 widened, and O1 (sstring parameters): a PERFORMANCE campaign that follows Tier 2 (V-fixes 6, 7,
10, 11, 14).**
- **Gain after Tier 2:** for LITERAL arguments O1 gains about 4 ns (a table hit against a free view) and
  no count. Its count gain is only for `string(b)` arguments (111 production call-argument sites).
- **A `string(b)` argument needs a no-mutation proof across callees.** `objectIsWritten` works within one
  function (escapeAnalysisOperations.go:1479-1530) and is alias-blind. Go's G4 is itself a copy, just a
  stack one. Rows to watch: bufio_test.go:603, io/multi_test.go:115, gob/timing_test.go:132 and
  slices_test.go:956.
- **The safety basis is rewritten.** Revision 2 said every breaker is a compile error, and it is not:
  - **silent counted copies:** an `@string` field store, an explicit `F<@string>`, `copy()`
    (builtin.cs:985/1085/1109), local `[]string` elements, and passing onward to `...string`;
  - **a silent run-time miss:** the binder demands exact parameter-type identity
    (TypeExtensions.GoMethodSets.cs:515-545) and fails soft (AdapterBinder.cs:70-76);
  - **compile errors:** func values (text/template/funcs.cs:40);
  - the generator's static adapters DO bridge `@string` to `sstring` (InterfaceImplTemplate.cs:294).
  
  So the filter is "reachable by neither a binder fallback nor reflection", not "satisfies no interface".
- **Preconditions before any widening:**
  - `sstring`'s own allocations route through `AllocationCounter` (sstring.cs:76, 111, 204, 214, 445,
    450, 453-459; encode.cs:1279 makes 2 objects and counts 1);
  - the `NoUncountedBackingAllocations` guard that AllocationCounter.cs:158 cites does not exist, so it
    is added;
  - `sstring` gains a spread, an enumerator, `copy` and `append`, which it lacks (fmt/print.cs:125-127).
- **The explicit span → `sstring` operator.** Its rationale at sstring.cs:182-188 (CS0034) is stale: the
  exact comparison operators landed later the same day (`c5398fcb64`). Before flipping it, add these
  negative tests:
  - `object o = sstr` stays an error;
  - a `Span<byte>` does not bind an `sstring` parameter;
  - no new overload set yields CS0121 (an `sstring` map indexer would expose about 3,548 map-literal
    lines).
- **Pilot:** fmt's unexported helpers (`doPrintf`, `parsenum`, `argNumber`, `writeString`), or a
  `ReadOnlySpan<byte>` twin overload. There are no public signatures there, and the `-m` verdicts are
  noescape at every link.

**7. G1's remainder:** named string keys, named-byte elements, and composite keys. It has 0 production
sites (2 composite-key reads in tests). It is not worth a stage.

## 8.3R RECOMMENDATION, REVISED (supersedes §8.3)

**1. Tier 2, the registration table, is the base.** The verification found it sound, and this revision
measured what it costs.
- **Gain:** count parity at every `@string`-typed site, at any length, with no visible delta.
- **Start-up:** +41/+58 ms (tiered/TC=0) for a hello world and +63/+102 ms for a net/http program. About
  2 ms or less under a ReadyToRun or NativeAOT publish.
- **Memory:** 0.3-0.9 MB for those closures.
- **Preconditions:** the wiring, order, sentinel, collectible and read-only preconditions of 8.1R2.2.
- *The owner's trade:*
  - **readability:** unchanged;
  - **performance:** literal hits 2.0-3.0× faster than today with tiering off (the growable table at
    quarter load), a miss 2.5-2.7 ns dearer, and a start-up tax under JIT only;
  - **parity:** Go's counts, and Go's identity within a module.

**2. The golib-only Go rules, G8 and G10.** Empty-operand concatenation and one-byte strings: no visible
delta, Go-exact, one length test each. They are the cheapest parity in this document.

**3. Tier 3 as ruled.** Arm C lands. Tier C stays. Arms A and B are not needed for count parity once
Tier 2 lands.

**4. The visible converter campaigns, each the owner's readability call, in order of reach×gain:**
- **G9, multi-operand concatenation:** 369 production sites, k − 2 objects each.
- **G12, read-only `[]byte("lit")`:** 216 sites, with a read-only proof.
- **O1, the `sstring` parameter pilot:** performance only after Tier 2, on unexported fmt helpers first.

**Not recommended:**
- the content cache (8.1R);
- O2 as a stage (already done);
- widening sstring before V-fix 11's counting and V-fix 10's proof.
