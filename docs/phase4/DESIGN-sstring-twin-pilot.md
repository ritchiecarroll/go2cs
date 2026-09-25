# DESIGN — the sstring TWIN pilot: the converter rule and the registry shape

> **Status: DRAFT, not ruled. No code exists.** Written by C2 while COORD's CS0121 census (the corpus
> build under the implicit u8→sstring operator) runs on the i9. It drafts the converter rule and the
> registry shape for the pilot COORD ruled on the i9 twin probe (`claude/i9-sstring-twin-probe`
> `b919919c96`). Every emitted-C# fact below was READ at `origin/master` `9a5f63041b`, and every Go fact
> at go1.24.13. Nothing here was built. The census's site list may change §3 and §6. A code cut waits on
> that list and on post-TRAIN-B master.

## 1. What the pilot is

A **twin** keeps a function's existing `@string` member and adds an `sstring` overload of the same name
carrying `[OverloadResolutionPriority(1)]`. The ruled shape (COORD, after the probe) has four parts:

- **The call sites.** TWIN plus priority, and golib's `explicit operator sstring(ReadOnlySpan<byte>)`
  (sstring.cs:185) made `implicit`.
- **The value sites.** ONE published canonical value delegate per twinned function. It is a
  `static readonly Funcꓸꓸꓸ<@string, …>` field whose lambda forwards, and every func-value site refers to
  it.
- **The names.** `GoNameOf` / `FuncForPC` answer the Go name (`fmt.Sprintf`).
- **The scope.** A converter rule over an EXPLICIT approved list, publishing twin and delegate records
  the way `GoRefPrimary` records are published.

The member prediction is fmt's `TestCountMallocs` row `Sprintf("xxx")` (fmt_test.go:1490, Go's
expected count 1): 2 today, 1 after.

## 2. The facts it rests on

**From the probe** (`probes/sstring-twin/RESULTS.md` on `b919919c96`):

| probe form | what it shows | consequence here |
|:--|:--|:--|
| a, b, c | with the priority attribute, a u8 literal (under the implicit operator), an `@string` and a C# `string` argument all bind the **sstring** member | every direct call site binds the twin **with no call-site change**. The sstring member is the hot path, so it carries the body (§3.1) |
| e, e4 | a method-group conversion to `Funcꓸꓸꓸ<@string, …>`, implicit or cast, is **CS0123** | no func-value site may name a twinned function as a method group (§3.3) |
| e3 | a lambda `(@string f, Span<any> a) => F.Sprintf(f, a)` compiles, and its body binds sstring | the canonical delegate and the defer form are lambdas |
| f | `var g = F.Sprintf` is **CS8917** | the converter never emits a natural-type var for a twin |
| g | a lambda capturing an sstring parameter is **CS9108** | a twin body must not capture a twinned parameter (the census's capture class, §5) |
| n1 | without the priority attribute, a u8 literal against the pair is **CS0121** under the implicit operator | every member of a twin pair, **generated overloads included**, carries the priority (§3.2) |
| ARM 2 | RecvGenerator accepts an sstring parameter and forwards it unchanged | a `[GoRecv]` twin is possible, but see §3.2 on the attribute |

**From the census** (`probes/c2-o1-survival` r2 `34b60c14da`, PILOT3, windows): **33 parameters in 30
functions**, all surviving at S3. They split into three groups:

- **13 package-level functions:** Appendf, Errorf, Fprintf, Fscanf, Printf, Sprintf, Sscanf, hasX,
  indexRune, parseArgNumber, parsenum, `utf8.DecodeRuneInString`, `utf8.RuneCountInString`.
- **13 `[GoRecv] this ref T` methods:** padString, fmtInteger (on `fmt`), fmtSbx, fmtSx, fmtBx,
  writeString, argNumber, consume, peek, accept, okVerb, scanNumber, advance.
- **4 methods with a direct `this ж<T>` receiver:** doPrintf, fmtBytes, doScanf, catchPanic (the
  signatures are at fmt/print.cs, format.cs and scan.cs, and unicode/utf8/utf8.cs).

**From the tree:**

- **Value sites**, the only ones in GOROOT for these functions:

  | Go site | shape | master emits | twin outcome |
  |:--|:--|:--|:--|
  | text/template/funcs.go:51 `"printf": fmt.Sprintf` | map entry | `((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintf))` (funcs.cs:40) | CS0123 (e4) |
  | text/template/parse/parse_test.go:333 | the same shape | parse_test.cs:341 | CS0123 (e4) |
  | runtime `FmtSprintf = fmt.Sprintf` | typed field assignment | import_test.cs:34 | CS0123 (e) |
  | fmt/errors_test.go:17 `noVetErrorf := fmt.Errorf` | typed local | `Funcꓸꓸꓸ<@string, any, error> noVetErrorf = fmt.Errorf;` (errors_test.cs:31) | CS0123 (e) |
  | fmt/export_test.go:8 `var Parsenum = parsenum` | typed package var | export_test.cs:13 | CS0123 (e) |
  | reflect/all_test.go:3703, 3709 `V(fmt.Fprintf)` | an `any` argument | all_test.cs:4414, 4419 | no natural type (f) |
  | crypto/internal/fips140test/check_test.go:117 `abi.FuncPCABIInternal(fmt.Printf)` | an `any` argument | check_test.cs:142 | no natural type (f) |

- **Defer and go.** A deferred call whose arity matches the callee is emitted as a METHOD GROUP into
  golib's generic `defer` (`defer(Ꮡp.catchPanic, Ꮡp.Value.arg, verb, formatˢ, ref ᒐ)`, print.cs:813).
  That becomes CS0123 against a twin, and CS0306 if an sstring ever reached a type argument. A deferred
  variadic call already takes the lambda form (`defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), …)`,
  Behavioral/ClosureDefer), which binds the twin as in e3.
- **Names.** `GoSyntheticPC.Of(Delegate)` keys on `target.Method` (GoSyntheticPC.cs:82), and
  `FuncForPC` names a func value by `goFrameName(d.Method, null)` (runtime/managed_impl.cs,
  `managedFuncName`). A lambda held in a static field is a compiler-generated `<.cctor>b__X_Y` method,
  which `goFrameName` would spell `fmt..cctor.funcY`. The canonical delegate therefore needs a name
  record.
- **Hidden frames.** `isGoSourceFrame` already hides a method carrying
  `[GeneratedCode("go2cs-gen", …)]` (RecvGenerator's ж forwarders). A converter-emitted forwarder needs
  the same treatment, or `runtime.Callers` depths shift on the paths that go through it (§3.4).
- **Generated overloads.** RecvGenerator's ж overload carries `[GeneratedCode]` and, when the source method has it,
  `NoInlining` (`ForwarderAttributes`, ReceiverMethodTemplate.cs:30-32). **It copies no other attribute.** A twin pair of `[GoRecv]` methods
  would therefore generate two ж overloads with no priority, which is n1's CS0121 for a u8 argument
  reaching the method through a pointer.

## 3. The rule

### 3.1 The registry (the explicit list)

The list is a curated Go map in the converter, beside `refPrimaryHandOwns`
(refVerdictPublication.go). It is keyed `"<pkgPath>.<Func>"` or `"<pkgPath>.<Recv>.<method>"`, and its
value is the set of twinned parameter indices:

```go
var sstringTwins = map[string][]int{
	"fmt.Sprintf": {0},
	"fmt.fmt.fmtSbx": {0, 2}, // s and digits; b []byte is untouched
	…
}
```

The list is the pilot's census population, not a predicate. The converter REFUSES a registered key
(conversion error, naming the key) when:
- it finds no declaration;
- the declaration is hand-owned or has no Go body;
- the parameter's type is not `string`;
- a registered parameter is captured by a closure in the body.

The last refusal turns the probe's CS9108 into a converter error at the declaration, instead of a build
error far from the cause (the `refPrimaryHandOwns` guard stance).

### 3.2 The declaring side

For each registered function, the converter emits:

1. **The sstring member**: the same name, each registered parameter typed `sstring`,
   `[OverloadResolutionPriority(1)]`, and **the converted body**. It carries the body because every
   direct call binds it (probe a/b/c).
2. **The `@string` member**: today's signature, unchanged, with a one-line forwarding body
   (`=> Sprintf((sstring)format, a);`) and `[GoTwinForwarder]` (§3.4).
3. **For a `[GoRecv]` method, both members keep `[GoRecv]`.** go2cs-gen must copy
   `[OverloadResolutionPriority(n)]` from the source method onto the ж overload it generates. This is a
   generator change to ReceiverMethodTemplate, the one place it emits method attributes, and it falls
   under `.claude/rules/golib-gen.md`. The 4 direct-ж methods need no generator change.

**The alias hazard inside the body.** `sstring → @string` is an IMPLICIT conversion that COPIES
(sstring.cs:176). A twin body that keeps today's local declarations would therefore allocate silently
where today's body does not. The case is `@string s0 = s;` (strconv's shape, and the census's alias
class): typed `@string`, the local materialises the view on every call. The rule: every local the
census's alias tracking binds to a registered parameter is emitted `sstring` in the twin body. Any other
`sstring → @string` conversion left in a twin body is an escape the census should have refused, and the
member-row arms in §4 measure it. Compiling proves nothing here, because the conversion is implicit.

**Where Go's `string(b)` meets a twin.** Today's `@string(b)` copy stays: form b binds the twin through
a zero-copy view of the copy. Passing the byte slice's own view is the O1 `string(b)` population's
change, not this one.

### 3.3 Value sites (any package, including the declaring one)

1. **The canonical delegate.** Every registered PACKAGE-LEVEL function gets one field in its package
   class, whether or not a value site exists. The `-tests` path adds value sites (export_test.go) that
   the production emission cannot see, and the field must already exist:

   ```csharp
   // The canonical func value of Sprintf. A twinned function has no single method group (CS0123).
   public static readonly Funcꓸꓸꓸ<@string, any, @string> Sprintfᶠ =
       [GoTwinForwarder("Sprintf")] static (@string format, Span<any> a) => Sprintf(format, a);
   ```

   - Access follows the function (internal when unexported, per the test-friend rule).
   - The suffix is a new marker, `FuncValueMarker = "ᶠ"` (ᶠ), in the family of `ˢ` (U+02E2) and
     `ᶜ` (U+1D9C). No converter source or golib file uses it at master.
   - The initializer depends on nothing, so it needs no relocation (§4.4 of
     DESIGN-string-literal-allocation.md).
   - A reader in another package triggers the declaring class's type initializer first, as for any
     static field.
   - The pilot population gets **13 delegates**.
2. **The value-site rewrite.** A twinned package-level function referenced anywhere other than as a
   call's `Fun` renders as `<qualifier>.<Name>ᶠ`, and the delegate cast is dropped because the field is
   already typed:
   - funcs.cs:40 becomes `["printf"u8] = fmt.Sprintfᶠ`;
   - errors_test.cs:31 becomes `… noVetErrorf = fmt.Errorfᶠ;`;
   - reflect's `V(fmt.Fprintfᶠ)` and the fips test's `FuncPCABIInternal(fmt.Printfᶠ)` follow the same
     rule.

   The hook sits beside `refLoweredFuncValueWrapper`, the converter's existing "adapt at the
   reference" arm, at convIdent.go:386. The qualified path (`fmt.Sprintf`) reaches the same decision
   through convSelectorExpr, and the pilot's red-first arm proves that it does.
3. **A target type that is not the canonical delegate's type** (a Go named func type rendered as a
   distinct C# delegate) falls back to a per-site lambda. Its identity is then per site, as every
   value site's is today. The record says so rather than guessing.
4. **A twinned METHOD referenced as a value** (a method value or method expression) renders as a
   per-site lambda, since a canonical delegate cannot bind a receiver. The pilot population has none,
   and the census's value-site column is empty for all 17 methods.

### 3.4 Names and frames: one golib attribute

`GoTwinForwarderAttribute(string? goName = null)` (AttributeTargets.Method) marks a converter-emitted
forwarder: the `@string` member of §3.2 and the canonical lambda of §3.3. Two readers consult it:

- **`isGoSourceFrame`** (runtime/managed_impl.cs) skips it exactly as it skips go2cs-gen output. A call
  through the `@string` member or the delegate then shows the same Go frames as a direct call.
- **`GoSyntheticPC.GoNameOf`** and **`goFrameName`** spell a method carrying a `goName` as that name.
  So `runtime.FuncForPC(reflect.ValueOf(fmt.Sprintf).Pointer()).Name()` reads `fmt.Sprintf`, and so
  does the `FuncPCABIInternal` path.

**Identity.** One delegate object per function means `reflect.ValueOf(fmt.Sprintf).Pointer()` is EQUAL
across value sites, as in Go. Today each site allocates a fresh delegate and gets its own token. This is
a stated side-improvement, and the red-first arm in §4 tests it.

### 3.5 Defer and go

A twinned callee forces the temp-parameter lambda form: one more rung in visitDeferStmt.go beside the
builtin and ref-lowered rungs (around lines 86-104), and the same rung in visitGoStmt.go. The eager
arguments stay `@string` type arguments of golib's generic `defer`, and the lambda's call binds the twin
(e3).

### 3.6 Records (the GoRefPrimary way)

A package publishes each EXPORTED twinned package-level function in a `package_info.cs` section that is
omitted when empty, exactly as `<RefVerdicts>` is:

```csharp
// <SStringTwins>
// A twinned function has a @string member and a prioritized sstring member, so it has no single method
// group; a func value names its canonical delegate `<Name>ᶠ` instead. Go spellings.
[assembly: GoSStringTwin("Sprintf")]
// </SStringTwins>
```

- **Readers.** The reader joins `loadPackageImplementLines`. internal/stdlibmeta keeps the prefix, as
  it keeps `GoRefPrimary`'s. A consuming conversion needs only the function's name to apply §3.3 and
  §3.5, because call sites bind by priority.
- **Guard.** A `TestPublishedTwinsMatchEmitted` analogue checks that a record exists exactly when the
  assembly declares both members and the `ᶠ` field.
- **Unexported functions and methods** are same-package only and are not published. The pilot twins no
  exported method, so no method record exists yet (§6).
- **Size.** For the pilot this is **9 records**: 7 in fmt, 2 in unicode/utf8.

## 4. Red-first arms (each RED at master, then GREEN, then the site regressed and restored byte-identical)

**Call forms** (the probe's a-g, in converter terms). The unit-test emission read comes first, then one
behavioral project whose Go output is the oracle:

| form | Go | arm |
|:--|:--|:--|
| a | `fmt.Sprintf("xxx")` | the emission binds the twin, and the allocation guard reads 0 for the argument |
| b | `fmt.Sprintf(s)`, s a string variable | binds the twin through a view, with no copy |
| c | a hoisted `ˢ` field argument | binds the twin |
| d | a twin calling a twin (doPrintf → argNumber) | the onward edge stays sstring, and no `@string` local appears in the body |
| e | a value site of each §2 shape | emits `Nameᶠ`, and never a method group |
| f | `f := fmt.Sprintf` | emits the typed local over `Sprintfᶠ` |
| g | a registered parameter captured in a body | the converter refuses the registration (§3.1) |

**Value identity and names** (a behavioral project; Go prints `true` and `fmt.Sprintf`):
- `reflect.ValueOf(fmt.Sprintf).Pointer() == reflect.ValueOf(fmt.Sprintf).Pointer()` is RED at master
  (per-site delegates) and GREEN after;
- `runtime.FuncForPC(…).Name()` is GREEN at master and must stay GREEN; `GoTwinForwarder`'s name arm is
  what keeps it GREEN;
- a `defer fmt.Printf(…)` and a `go` call to a twin;
- a panic raised inside a twin reached through the delegate, whose traceback must match a direct
  call's.

**Member row:** fmt `TestCountMallocs` `Sprintf("xxx")` goes 2 → 1. It needs the implicit operator:
under ARM 0 the u8 literal binds `@string` and the row stays 2.

## 5. Prerequisites and what is owed

1. **COORD's CS0121 census under the implicit operator** (dispatched to the i9). It decides whether the
   operator flip lands. Without the flip, form a needs the converter to emit `(sstring)"…"u8` in a
   twinned slot. That is a second rule, which this note does not draft.
2. **Two golib gaps.** S3 was computed with the gaps closed (S1). The census's first-reason column
   names exactly four gap ROOTS in the pilot:
   - `range` over an sstring (no enumerator): `okVerb #1`, `indexRune`, `RuneCountInString`;
   - `append(b, s...)` (no spread): `writeString`.

   padString, fmtBytes, accept, consume, peek and scanNumber fail at S0 only by cascade from those
   roots. `copy` is not needed. Either the gaps close first, or the list shrinks to what survives with
   the twin and no gap closure. That mode ({twin, gaps open}) was not computed, and is owed if needed.
3. **The go2cs-gen priority propagation** (§3.2) for the 13 `[GoRecv]` methods.
4. **catchPanic.** Its parameter survives only at S3, and all 4 of its literal sites are deferred
   (`lit_deferred` = 4). A deferred call binds `@string` (§3.5), so twinning it saves nothing and adds
   the only defer site in the population. The recommendation is to drop it: **32 parameters in 29
   functions**. This is COORD's call, since the ruling names 33.

## 6. Boundaries (stated, not built)

- **No exported METHOD is twinned.** Twinning one would reach three things this note does not handle:
  - the reflect method sets (`GetGoMethodSetEntries`, which sort by name and would see two candidates);
  - the extension-method registry, which scans every extension method;
  - interface binding by exact signature (ImplementGenerator, TypeExtensions.GoMethodSets.cs:515-545).
- **Hand-owned C# that names a pilot function as a method group** would become CS0123. The converter
  does not rewrite hand-owns. The i9 build is the census that finds any.
- **In the pilot population, the `@string` member is reached only through method-group conversions**,
  which §3.3 replaces with the delegate. It stays as ruled: it keeps the signature every existing
  binder and consumer compiled against, and removing it is the S2 flip, which the owner did not choose.
