# PROPOSAL: the "Good to know" items in ConsumingGoFromCSharp.md

Status: PROPOSAL, nothing implemented. Reviewed 2026-10-16 at master `8af95e28b0` (equal to origin/master,
checked by `git ls-remote`). Branch tips read: `claude/c1-linq-slice` `2d635fad3f` and
`claude/c1-tostring-forward` `e752628faf`, both cut on `claude/hop-go126`.

## What was probed and what was only read

Two throwaway probes ran under `H:\go2cs-tmp-coord\consumer-probe` (bin/obj and the package cache are
deleted now):

- **`real/`** is a net10.0 console app that references the **published 1.24.13.7** `go.fmt`, `go.strings`,
  `go.strconv`, `go.time`, `go.errors`, `go.encoding.hex` and `go.crypto.sha256` from nuget.org. It has
  one build arm per item. Every compiler diagnostic quoted below as "probed" comes from this app.
- **`lang/`** copies golib's `append` overload set (`builtin.cs:368-509`, `:1367`, `:1450`) onto stand-in
  types. It tests the fix proposed for item 4 without editing golib.

Everything else comes from reading the code at the cited `file:line`. The proposals' own costs and risks
are estimates, and they say so.

**A finding outside the items:** the published 1.24.13.7 packages **still raise CS8981**. The probe
printed six `warning CS8981` lines, one per alias. The NoWarn landed on master after that release
(`ec014aac22`), so the page describes behaviour that no published package has yet (item 2).

**The rewrites already on branches.** Two items are already rewritten on branches the owner approved on
2026-10-15. Neither branch is on master.

- **Item 6 (LINQ):** `claude/c1-linq-slice` deletes the item. `IArray<T>` stops deriving from
  `IEnumerable<(nint, T)>` (`array.cs` on that branch), so LINQ sees the values. The item is replaced by
  "`foreach (var v in s)` gives a tuple".
- **Item 7 (Duration in nanoseconds):** `claude/c1-tostring-forward` makes `ToString()` return the Go
  `%v` text (`golib/GoMethodText.cs`, MethodSetKeep). The item is replaced by "a Go func value prints the
  delegate's type name".

This review covers both the master text and those replacement items.

## Classes

- (a) golib alone.
- (b) source-generator change.
- (c) converter emission change: the visible converted code changes, so the owner must approve it as
  reads-like-Go.
- (d) docs only.
- (e) inherent to Go and C# semantics; stays documented.

A packaging/MSBuild change (golib's `buildTransitive`, the pack targets) is counted as (a) when golib
owns the file. Where it also needs the converter's generated build files, the item says so.

---

## Item 1: "A Go method does not compile" (without the namespace import)

**Root cause.** A Go package is a static class whose Go methods are extension methods. For example,
`partial class hex_package` (`src/core/encoding/hex/hex.cs:14`) sits in `namespace go.encoding`
(`hex.cs:6`). A C# `using X = Y;` alias never brings extension methods into scope; only a namespace
import or `using static` does. golib's packages add only `go` and `go.builtin`
(`src/core/golib/buildTransitive/go.lib.targets:89-90`). So every package whose import path has a slash
needs a second, manual `using`.

The error is misleading as well. Probed on 1.24.13.7:

> `CS1929: 'hex_package.InvalidByteError' does not contain a definition for 'Error' and the best
> extension method overload 'os_package.Error(ref os_package.SyscallError)' requires a receiver of type
> 'ref go.os_package.SyscallError'`

It names `os`, a package the probe never mentions.

**Class: (a)**, with a packaging hook in two places. The visible C# does not change.

**Proposal.** Each converted package's nupkg ships `build/<PackageId>.props` holding one line:

```xml
<ItemGroup Condition="'$(GoConsumerUsings)' != 'false'">
  <Using Include="go.encoding" />   <!-- the package's own namespace -->
</ItemGroup>
```

NuGet imports `build/<id>.props` for a **direct** reference only, which is the right scope: a consumer
gets the methods of the packages it added, never those of `internal/*` packages that arrive
transitively.

The namespace is already known at pack time with **no csproj change**. Every converted csproj carries
`<RootNamespace>` equal to its package class's namespace: `encoding.hex.csproj:6` reads `go.encoding`,
`go.ast.csproj:6` reads `go.go`, and the vendor `norm` csproj reads
`go.vendor.golang.org.x.text.unicode`. So one pack-time target writes the file from `$(RootNamespace)`
and adds it as `None Pack="true" PackagePath="build/$(PackageId).props"`. When `$(RootNamespace)` is
`go`, the target can skip, because go.lib already adds that using. The SDK also emits identical `Using`
items once, as `go.lib.targets:84-86` records was measured.

Where the target lives:

- **stdlib:** `src/Directory.Build.targets`, which already imports a golib targets file for every in-tree
  project (`go.lib.symbols.targets`). Put the target in a new
  `src/core/golib/buildTransitive/go.lib.pack.targets` and import it there.
- **nugetgo modules (`-recurse=nuget`):** the converted project takes go.lib as a package, so
  `go.lib.targets` can import the same file. It must be gated so that a hand-written library that merely
  references go.* packages does not get one. A zero-drift gate is `'$(Product)' == 'go2cs'`, which the
  template sets (`src/go2cs/csproj-template.xml:8`). An explicit property would be cleaner but costs a
  csproj change across the corpus. The owner chooses.

Consumer view, before and after:

```csharp
// before                                    // after
using go.encoding;          // required      // (nothing: the package adds it)
using hex = go.encoding.hex_package;         using hex = go.encoding.hex_package;
@string s = err.Error();                     @string s = err.Error();
```

Probed: adding exactly `<Using Include="go.encoding" />` to the probe's csproj turns the CS1929 arm into
rc 0.

**Effort: M.** The code is S. The checking is M: pack one stdlib package and one nugetgo package, read
the nupkg, and run a consumer smoke. Check also that push-nuget's flavour merge carries a non-`lib/` entry
from the reference flavour, and that the release-smoke arms accept it.

**Risks.** A namespace import also brings that namespace's top-level types into scope. Today such a
namespace holds `*_package` classes and child namespaces, so the risk is low, but a census is owed.

**Breaking:** no. A consumer who already writes `using go.encoding;` gets CS0105 or a hidden duplicate at
most. Measure it; the SDK's global using deduplicates.

**Optional, owner decision: global package aliases.** The same props could add
`<Using Include="go.strings_package" Alias="strings" />`, so the five-minute start would need no
`using` lines at all. Probed: this works when the file declares no alias of its own. But a file that also
writes `using strings = go.strings_package;`, which is the page's recommended form and CSharpConsumer's
own code, fails with `CS1537: The using alias 'strings' appeared previously in this namespace`. So it can
only be **opt-in** (`GoPackageAliases=true`). On by default it would break every existing consumer.

---

## Item 2: "Warning CS8981 on each alias"

**Root cause.** C# warns on an all-lowercase type or alias name. The NoWarn is in
`go.lib.targets:101-103` on master, but **not in any published package** (probed on 1.24.13.7: six
warnings). A project-reference consumer never imports `buildTransitive`, as the file's own comment says.

**Class: (d)**, plus (a) for project references if wanted.

**Proposal.**
1. **(d), now:** while the next release is unpublished, the page should not say "NuGet packages switch
   it off for you". The present-tense policy says to state what IS, so either hold the sentence until the
   release ships or scope it to the release that carries it. Owner decision.
2. **(a), optional:** let a project-reference consumer import golib's consumer settings with one line
   instead of hand-copying them:
   `<Import Project="$(go2csPath)core/golib/buildTransitive/go.lib.targets" />`. That file is safe there:
   its RID swap acts only on package items, and its trim and symbol defaults are condition-guarded. The
   page could document that one line in place of the NoWarn. Effort S, and nothing breaks.

---

## Item 3: "`int n = len(s)` does not compile"

**Root cause.** Go's `int` is native-sized, so `len` returns `nint` (`builtin.cs`, by design). Probed:
`CS0266: Cannot implicitly convert type 'nint' to 'int'. An explicit conversion exists (are you missing a
cast?)`.

**Class: (e).** C# cannot overload on return type. A user-defined `nint`-to-`int` conversion is
impossible, because both are primitives. Returning `int` would break Go's 64-bit `len` semantics.

**Proposal:** no code change. The compiler's own message already points to the cast. Leave the item
as it is.

---

## Item 4: "`append(nums, 4, 5)` is ambiguous on a `slice<nint>`"

**Root cause, measured.** The legacy overload `append<T>(ISlice slice, params T[] elems)`
(`builtin.cs:390`, unchanged since the C# 10 port) infers `T = int` from the literals and accepts the
slice through a boxing conversion. Each candidate wins on one argument: the `slice<T>` overloads on the
slice (identity against boxing), the `ISlice` overload on the literals (identity `int`). Neither is
better.

Probed on 1.24.13.7:

> `CS0121: The call is ambiguous between ... 'go.builtin.append<T>(go.ISlice, params T[])' and
> 'go.builtin.append<T>(go.slice<T>, params System.ReadOnlySpan<T>)'`

**Class: (a).**

**Proposal.** Put `[OverloadResolutionPriority(-1)]` on that one overload. golib already uses this
attribute for the `GoStr` forwarders (`GoStrAttribute.cs:14`, `sstring.cs:269`). Before and after:

```csharp
nums = append(nums, (nint)4, (nint)5);   // today
nums = append(nums, 4, 5);               // after: binds append<nint>(slice<nint>, params ReadOnlySpan<nint>)
```

The stand-in probe (`lang/`) reproduces CS0121 with golib's overload set. With the attribute, the same
call compiles and binds `slice<T>, ReadOnlySpan<T> T=IntPtr`. Removing the overload instead gives the
same result, but removal can break a call whose first argument is typed `ISlice`, where it is the only
candidate.

**Why it is safe, argued and not yet measured on the corpus.** The priority attribute removes a candidate
only when a higher-priority candidate is applicable. It can never unseat a call's unique best candidate.
On every currently compiling call, the `ISlice` overload is either the only applicable candidate (the
attribute changes nothing) or not the best (removing a non-best candidate cannot change a unique best).
Its first argument is always the worse conversion: boxing against identity, or a worse conversion target
than `slice<T>`. So the change can alter only calls that fail with CS0121 today.

The proof still owes a build:

- a full `go2cs-stdlib.slnx` build;
- the `go2cs.slnx` build (golib API rule);
- CSharpConsumer, with the cast dropped and one new check.

The emitted C# does not change, so CNR has nothing to see.

**Effort: S.** **Risks:** none beyond the build. **Breaking:** no. Docs: the item and the Numbers
paragraph drop the cast advice.

---

## Item 5: "`error e = nil;` does not compile"

**Root cause.** `nil` is `public static readonly NilType nil` (`builtin.cs:153`), a class instance
(`NilType.cs`). C# forbids a user-defined conversion to an interface type (CS0552), so `NilType` can never
convert implicitly to `error` or any other Go interface. Probed:

> `CS0266: Cannot implicitly convert type 'go.NilType' to 'go.error'. An explicit conversion exists (are
> you missing a cast?)`

The "explicit conversion" is C#'s built-in reference conversion from a non-sealed class to an interface,
so `(error)nil` compiles and yields null. The converter itself emits `default!` for a nil interface
(`src/core/errors/wrap.cs:25`).

**Class: (e).**

**Rejected alternative.** Making `NilType : error` would let `error e = nil` compile, because `nil`'s
value is null. It would cover only `error`, not `io.Reader` or any other interface. It would add an
implicit conversion to every overload set in the corpus that takes `error`. And it would make `NilType`
carry an `Error()` method. That is high risk for one spelling.

**Proposal (d):** keep the item, and add that the compiler's suggested cast `(error)nil` also works but
`null` or `default` is the plain form. Effort S.

---

## Item 6: LINQ (master) / "`foreach (var v in s)` gives a tuple" (LINQ branch)

**Master text.** This is resolved on `claude/c1-linq-slice`, approved and not yet landed. Probed on
1.24.13.7: `words.Count(w => ...)` fails with
`CS0411: The type arguments for method 'MemoryExtensions.Count<T>(Span<T>, T)' cannot be inferred`,
which is a second misleading message. **Action: land the branch.** That is (a)+(b), already built.

**Replacement item.** `slice<T>.GetEnumerator()` (`slice.cs:997`) yields `(index, value)` pairs on
purpose, so `foreach` reads like Go's `range` and converted code depends on it. Probed:
`CS0029: Cannot implicitly convert type '(nint, go.@string)' to 'go.@string'`.

**Class: (e)** for the pairs, plus a (d) tweak. The values-only loop already exists:
`foreach (var v in s.ToSpan())` uses the span's enumerator, which allocates nothing (`ToSpan` is public on
`slice<T>` and on named arrays). Its one limit is that it cannot cross an `await`. After the LINQ branch,
`s.AsEnumerable()` works too, and it boxes. Add one clause to the item: "or `foreach (var v in
s.ToSpan())`". A golib `Values()` view struct would add little over `ToSpan()`, so it is not proposed.

---

## Item 7: Duration in nanoseconds (master) / func value prints the delegate type (ToString branch)

**Master text.** This is resolved on `claude/c1-tostring-forward`, approved and not yet landed. Probed on
1.24.13.7: `$"{d}"` prints `1500000000`, and `$"{errors.New("disk full")}"` prints an address
(`0x166411bea20`). **Action: land the branch.**

**Replacement item.** A Go func value is a .NET delegate, and `Delegate.ToString()` cannot be overridden
or forwarded. **Class: (e).** The branch's wording ("print it through `fmt`") is right. No proposal.

---

## Item 8: "A copy of a Go array value shares its bytes"

**Root cause.** `array<T>` is a `readonly struct` over a heap `T[]` (`array.cs:115`, field
`m_array` at `:124`). A named array type such as `uuid.UUID` is a generated struct around it. A C#
assignment copies the struct, not the array. Converted code emits `.Clone()` at each copy site; the
template generates it at `src/gen/go2cs-gen/Templates/InheritedType/IArrayTypeTemplate.cs:67`, and
`GoValueClone` is the deep-copy member. Probed on `sha256.Sum256`'s `array<byte>`: after `var b = a;
b[0] = ...`, `a[0] == b[0]` is True.

**Class: (e)** as things stand. No C# construct can run code on struct assignment.

**Options, none recommended for the first batch:**
1. **(b)+(c), L:** give named arrays inline value storage, a generator-emitted `[InlineArray(N)]` field
   (the template already receives `targetTypeSize`). This would make the C# assignment the Go copy. But it
   touches spans over readonly copies, `ж<T>` boxing of interior storage, reflect layout, AOT, and every
   converted copy site. It is a design arc, not a fix.
2. **New consumer analyzer, M:** a warning with a code fix on a plain assignment of a Go array value that
   is not followed by `.Clone()`. See the cross-cutting note at the end.
3. **(d), now:** keep the item; it is accurate.

---

## Item 9: "A Go map type is not an `ISet<T>`"

**Root cause.** A named Go map type gets the base list `IMap<K,V>, ISupportMake<…>` and nothing else
(`src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:119`). The members come from
`IMapTypeTemplate.cs`. hashset's `HashSet[T]` is `map[T]struct{}`, so its value type is golib's
`EmptyStruct`, and nothing marks it as a set.

**Class: (b)** for named types, plus (a) for plain maps. The visible C# does not change, because
generator output is hidden.

**Proposal.**
- **(b):** when the generator emits a named map whose value type is `EmptyStruct`, it also implements
  `IReadOnlySet<K>`, **explicitly**. Then the Go methods (`Add`, `Contains`, `Remove` on hashset, which
  are extension methods) still bind at call sites, the same rule `IMapTypeTemplate.cs:24-28` already
  follows for `Add`, `Remove`, `TryGetValue` and `ContainsKey`. The members are `Contains` plus the
  set-relation queries, each forwarding to the map's keys.
- **(a):** golib adds `AsReadOnlySet()` on `IMap<K, EmptyStruct>`, a struct view for an unnamed
  `map[K]struct{}`.

```csharp
IReadOnlySet<int> s = set;                 // after: a hashset.HashSet<int> passes where a set goes
bool sub = s.IsSubsetOf(new[] { 1, 2, 3, 4, 5 });
```

**Effort: S-M.** It needs a GenTests template test and one CSharpConsumer check against hashset.

**Risks.** The reflect method-set scan and the full-trim registries must ignore the extra BCL
interface; read `GoReflect.MethodSets.cs` before cutting. `ISet<K>` (mutable) is deliberately not
proposed: its `Add` returns `bool` and would compete with a Go `Add` of the same shape. **Breaking:**
no; it is additive.

---

## Item 10: "Errors are values, not exceptions"

**Root cause.** This is Go's model, kept on purpose. golib has `PanicException` (`builtin.cs:282`) but
no bridge from an error to an exception (grep: no `OrThrow`, `ThrowIfError` or `ErrorException` in
golib).

**Class: (e)** for the model, plus an **(a)** convenience.

**Proposal.** golib adds a small, purely C#-side bridge in namespace `go`, so `using go;` (already global
for package consumers) brings it in:

```csharp
public sealed class GoErrorException(error err) : Exception(err.Error()) { public error Err => err; }
public static T OrThrow<T>(this (T value, error err) r);              // throws GoErrorException when err != nil
public static (T1, T2) OrThrow<T1, T2>(this (T1, T2, error) r);
public static void OrThrow(this error err);
```

```csharp
// before
var (port, err) = strconv.Atoi(text);
if (err != nil) throw new FormatException(err.Error());
// after
nint port = strconv.Atoi(text).OrThrow();
catch (GoErrorException ex) when (errors.Is(ex.Err, strconv.ErrSyntax)) { … }
```

**Naming is an owner decision.** `Must()` mirrors Go's `template.Must` and `regexp.MustCompile`, but Go's
`Must` panics, and this throws a non-panic exception. `OrThrow()` says what it does in C# terms; I lean
that way.

**Parity:** none affected, because Go code never calls it. **Effort: S.** It needs one GolibTests file and
two CSharpConsumer checks. **Breaking:** no.

---

## Item 11: "A channel call blocks its thread"

**Root cause.** A blocked `Send` or `Receive` parks the calling thread on a `SemaphoreSlim`. A plain
waiter parks on `Waiter.Park` (`channel.cs:237`) and a select parks on `SelectState.Park` (`:212`).
`Send` is at `:1531` and `Receive` at `:1667`. Goroutine identity is taken from the parking thread
(`:247`).

**Class: (e)** for Go's blocking semantics, plus an **(a)** convenience.

**Proposal, choosing the cheap form deliberately.** `channel<T>` gains:

```csharp
Task<T> ReceiveAsync(CancellationToken ct = default);
Task SendAsync(T value, CancellationToken ct = default);
```

Each runs the existing blocking operation on a dedicated thread
(`TaskCreationOptions.LongRunning`, so it never starves the pool). Cancellation is a Go `select` between
the operation and a private channel that the token closes. Built that way, the delivery and fairness
rules stay golib's existing ones.

A truly thread-free async waiter (`Park.WaitAsync`) would need a second park path through
`Recv`/`Send`/`select` and the goroutine accounting. That is L and not proposed.

`sync.WaitGroup` lives in the converted `sync` package, which golib cannot reference. For it, the page
documents `await Task.Run(() => wg.Wait())`. That is (d).

```csharp
nint v = await results.ReceiveAsync(ct);   // UI-thread safe
```

**Effort: M**, for the cancellation arm and its tests. **Breaking:** no.

---

## Item 12: "Glyphs"

**Root cause.**
- `goǃ` cannot be `go`: a method named `go` would lose name lookup to namespace `go`.
- `Δ` comes from a real name clash inside one package class, such as `time.Second` and `Time.Second()`.
- `ꟷ` is `public const bool ꟷ = false;` (`builtin.cs:187`), an overload discriminator.

**Class: (e)** for `goǃ` and `Δ` short of a converter rename, which would be (c), corpus-wide, and not
worth it. **(d)** for `ꟷ`. Probed: `ages["ann", false]` and `ages.TryGetValue("ann", out nint age)`
(public on `map<K,V>`, `map.cs:568`) both return `(31, true)`.

**Proposal (d):** add one sentence: the comma-ok read is also `TryGetValue`, as on any `IDictionary`.
A C# reader then types no glyph for maps. ASCII spellings for `goǃ` and the others are already queued as
the rainy-day "ASCII consumer scout"; this review does not reopen it.

---

## Cross-cutting, owner decision: a consumer analyzer

go.lib reaches every consumer, so it could ship a small Roslyn analyzer under `analyzers/dotnet/cs`.
go.gen is a `DevelopmentDependency` (`src/gen/go2cs-gen/go2cs-gen.csproj:52`) and never reaches a
consumer. The analyzer would provide:

- the item 8 warning and its `Clone()` fix;
- code fixes for the compiler's own CS1929 (add the package's namespace; moot once item 1 lands) and
  CS0266 on `nil` (use `null`).

It must skip converted code, for example types carrying `[GoPackage]`, because nugetgo packages build
against go.lib too. **Effort: M-L.** It is listed for completeness, not for the first batch.

---

## Ranking (value to effort)

| Rank | Item | Change | Class | Effort | Breaking |
|---|---|---|---|---|---|
| 1 | 6, 7 | Land `c1-linq-slice` and `c1-tostring-forward` (already built, approved) | a+b | done; landing only | no |
| 2 | 4 | `[OverloadResolutionPriority(-1)]` on `append<T>(ISlice, params T[])` | a | S | no |
| 3 | 2 | Page accuracy: CS8981 is not off in any published package yet | d | S | no |
| 4 | 1 | `build/<PackageId>.props` adds the package's namespace `using` | a (+ the generated build hook for nugetgo) | M | no |
| 5 | 3, 5, 6′, 11, 12 | Docs: `(error)nil` note, `foreach … ToSpan()`, `Task.Run` for `Wait`, `TryGetValue` | d | S | no |
| 6 | 10 | `OrThrow()` / `GoErrorException` | a | S | no |
| 7 | 9 | `IReadOnlySet<K>` for `map[K]struct{}` named types, `AsReadOnlySet()` for plain maps | b+a | S-M | no |
| 8 | 11 | `ReceiveAsync` / `SendAsync` with cancellation | a | M | no |
| 9 | 2 | Project-reference consumers import `go.lib.targets` by path (docs one-liner) | d (a) | S | no |
| 10 | 8 | Consumer analyzer, or inline-array named arrays | new / b+c | M-L / L | analyzer no; inline arrays yes |

**Recommended first batch:** ranks 1 to 5. That means the two approved branches; the `append` priority
(one attribute, proof by build); the CS8981 accuracy fix; the namespace props; and a single docs pass. The
docs pass rewrites the Good-to-know list against what then compiles. Items 1 and 4 then disappear from
the list, item 2 shrinks to the project-reference case, and items 6 and 7 take the branches' wording.

## Owner decisions needed

1. Item 2: how the page states the CS8981 NoWarn before the release that ships it.
2. Item 1: the pack-time gate for nugetgo packages (`$(Product) == 'go2cs'` with zero drift, or an
   explicit property with corpus csproj drift), and whether opt-in global aliases (`GoPackageAliases`)
   are wanted.
3. Item 10: the name, `OrThrow()` or `Must()`.
4. Whether items 9 and 11 (new golib/generator API surface) are wanted before the public announcement.
   Both are additive.
5. Whether a consumer analyzer is in scope at all.
