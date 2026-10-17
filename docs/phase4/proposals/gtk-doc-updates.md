# PROPOSAL: docs/ConsumingGoFromCSharp.md text for the consumer-API lane (gtk)

Status: PROPOSED text, not applied. The page changes at release; this file holds the wording for each item the
lane `claude/coord-gtk-consumer-api` (cut from the hop tip `5adb2dfb58`) implements. Section names are the page's.
Each block says where it goes and what it replaces. Commit SHAs are in the lane's report.

Two things are true of every item below and should shape the wording:

- Items 4, 9, 10 and 11 are golib API, so they reach a consumer through **go.lib**, in the first release cut from
  the hop line. Until then the page must not describe them in the present tense.
- Items 1 and 5 are **package metadata** (`build/<PackageId>.props`, written at pack time). A package packed before
  this lane carries none, so the page should scope them to "packages from release X on", or wait for the release.

---

## Item 4: `append` takes C# int literals (Numbers; Good to know)

**Numbers, replace the last paragraph and its sample:**

> So `len` returns `nint`, and so does `strconv.Atoi`. Hold the result in `var` or `nint`, or cast it where you need
> an `int`. A C# integer literal appends to any Go integer slice as it is:
>
> ```csharp
> var size = len("héllo");            // nint, Go's int: 6 bytes
> int small = (int)len("héllo");      // C# int is Go's int32, so cast when you need one
> var (num, aerr) = strconv.Atoi("42");   // (nint, error)
> slice<nint> nums = new nint[] { 1, 2, 3 };
> nums = append(nums, 4, 5);              // C# int literals append to a slice<nint>
> ```

**Good to know: delete the item** "`append(nums, 4, 5)` is ambiguous on a `slice<nint>`".

---

## Item 10: `Must()` (Errors; Good to know)

**Errors, replace the closing paragraph ("Where the rest of a C# program expects exceptions...") and its sample:**

> Where the rest of a C# program expects exceptions, `Must()` turns a result into its value or an exception. It
> works on a bare `error` and on every `(value, error)` tuple a Go function returns, up to three values:
>
> ```csharp
> nint port = strconv.Atoi(text).Must();     // the value, or a GoErrorException
> ```
>
> The name is Go's, and so is the behaviour: like `template.Must` and `regexp.MustCompile`, a failed `Must` panics
> with the error. `GoErrorException` is a `PanicException` whose `State` is the error, and its `Err` property hands
> the original error back, so the Go matching still works:
>
> ```csharp
> try
> {
>     strconv.Atoi("eighty").Must();
> }
> catch (GoErrorException ex) when (errors.Is(ex.Err, strconv.ErrSyntax))
> {
>     // ex.Message is the error's text: strconv.Atoi: parsing "eighty": invalid syntax
> }
> ```
>
> An error is nil as Go defines it, so an interface that holds a nil pointer is not nil, and `Must` throws for it.
> To build your own exception instead, check `err != nil` and throw, as before.

**Panics and exceptions, add one sentence** after the description of `PanicException`:

> A failed `Must()` is a panic too ([Errors](#errors)), so `catch (PanicException)` catches it.

**Good to know, replace** "Errors are values, not exceptions" **with:**

> - **Errors are values, not exceptions.** Check `err != nil` after each call that can fail, or call `.Must()` on
>   the result to get its value or a `GoErrorException` ([Errors](#errors)).

---

## Item 9: `AsReadOnlySet()` (Maps; Generics: hashset; Good to know)

**Maps, append after the `IDictionary` sample:**

> `AsReadOnlySet()` gives a map's keys as an `IReadOnlySet<K>`, for C# code that takes a set. It suits Go's set idiom
> `map[K]struct{}` best, and works on any map, plain or named. The view is live and read-only: a later write to the
> map shows through it, and it never writes. It answers with Go's map semantics: a nil map is the empty set, `+0`
> and `-0` are one key, and a NaN key is never found.
>
> ```csharp
> var seen = new map<@string, EmptyStruct> { ["go"] = default };
> IReadOnlySet<@string> words = seen.AsReadOnlySet();
> // words.Contains("go") is true; words.IsSubsetOf(new @string[] { "go", "cs" }) is true
> ```

**Generics: hashset, add after the existing sample:**

> ```csharp
> IReadOnlySet<int> members = set.AsReadOnlySet();    // a live, read-only view of the keys
> // members.IsSubsetOf(new[] { 1, 2, 3, 4, 5 }) is true
> ```

**Good to know, replace** "A Go map type is not an `ISet<T>`" **with:**

> - **A Go map type is not an `ISet<T>`.** It is an `IDictionary<K,V>`; hashset's `HashSet<T>` is
>   `IDictionary<T, EmptyStruct>`. Where a set goes, pass `set.AsReadOnlySet()` ([Maps](#maps)).

Why the map is not itself an `IReadOnlySet<K>`, for the record (not page text): a Go map is already an
`IEnumerable<KeyValuePair<K,V>>`, and a second `IEnumerable<K>` on the same type, even implemented explicitly, makes
LINQ over the map fail with CS0411 (measured on a stand-in type: `m.Count(kv => ...)` and `m.Select(...)` compile
without it and fail with it). So the set is a separate view object.

---

## Item 11: `ReceiveAsync` / `SendAsync` (Goroutines and channels; Good to know)

**Goroutines and channels, replace the second bullet of "A few things behave as Go's runtime does":**

> - `Send`, `Receive` and `WaitGroup.Wait` block the calling thread until they can complete. From a UI thread or
>   any other code that must not block, `await` the channel instead: `ReceiveAsync` and `SendAsync` wait without
>   holding a thread, and take a `CancellationToken`. `WaitGroup.Wait` has no awaitable form; run it in
>   `Task.Run`.

**And add a sample after the `TrySend`/`TryReceive` sample:**

> ```csharp
> nint v = await results.ReceiveAsync(ct);   // waits without blocking the thread
> await results.SendAsync(7, ct);
> var (w, open) = await results.ReceiveAsync(ꟷ, ct);   // Go: w, open := <-results
> ```
>
> They are the same channel operations, so Go's rules hold: an unbuffered channel is a rendezvous, a closed channel
> drains its buffer and then reads as the zero value (with `open == false` in the comma-ok form), and a send on a
> closed channel panics, through the task. Waiters are served in order, alongside goroutines on the same channel.
> Cancellation is all or nothing: a canceled operation took or delivered nothing, and one that completed is never
> reported as canceled. A canceled task ends Canceled; catch `OperationCanceledException`, the .NET idiom (as for
> `SemaphoreSlim.WaitAsync`), since a token canceled before the call surfaces its subclass
> `TaskCanceledException`. A nil channel never becomes ready, so only cancellation ends a wait on one.

**Good to know, replace** "A channel call blocks its thread" **with:**

> - **A channel call blocks its thread.** Keep `Send`, `Receive` and `Wait` off a UI thread: `await` the channel's
>   `ReceiveAsync` and `SendAsync` instead, and run `WaitGroup.Wait` in `Task.Run`.

Notes for the reviewer (not page text): `ReceiveAsync(ꟷ, ct)` mirrors `Receive(ꟷ)`; any `bool` works as the
discriminator. CORRECTION to the lane's first report: `ReceiveAsync(default)` is NOT ambiguous. Measured against
the commit that added the methods, it compiles and binds the token form, because C# prefers the candidate that
fills every parameter over the comma-ok one that leaves its optional token to a default; no
`[OverloadResolutionPriority]` is needed, and GolibTests' ReceiveAsyncOfDefaultBindsTheTokenForm pins it (the
two-argument `ReceiveAsync(default, default)` is the comma-ok form). The methods are on `channel<T>` only: a NAMED
Go channel type (a generated wrapper) does not forward them yet (that is a generator change, not in this lane).

---

## Item 1: the package's own namespace using (Imports; A five-minute start; Good to know)

Wording for when packages that carry `build/<PackageId>.props` are published. Until then, keep the current text.

**Imports, replace the paragraph "Go methods come through the namespace, not the alias." through the end of the
"Packages from NuGet bring two of these for you" paragraph with:**

> **Go methods come through the namespace, not the alias.** An alias names the package class, which reaches the
> package's functions and types. Go methods are C# extension methods, and C# finds those only through a namespace
> import (or `using static` on the class). Every standard-library package whose import path has no slash lives in
> the namespace `go`; a package whose path has a slash lives in its own (`encoding/hex` in `go.encoding`,
> `github.com/google/uuid` in `go.github.com.google`).
>
> **Packages from NuGet bring these for you.** Every `go.*` and `nugetgo.*` package adds `using go;` and
> `using static go.builtin;` to your project, and each package you reference adds its own namespace, so a Go
> method such as `id.String()` compiles with no namespace `using` of your own. Only the packages you reference
> directly add theirs. Set the MSBuild property `GoConsumerUsings` to `false` to turn all of them off. A conversion
> you reference as projects adds none of them, so write them yourself, as `Program.cs` does; without the namespace,
> a method call does not compile, and the error names some other package's method, which can mislead.

**Good to know, replace** "A Go method does not compile" **with** (it now only applies to project references):

> - **A Go method does not compile.** In a conversion referenced as projects, import the package's namespace as well
>   as its alias: `using go.encoding;` for `encoding/hex` ([Imports](#imports)). Packages from NuGet add it for you.

**A five-minute start:** the uuid sample can drop `using go.github.com.google;` once the nugetgo package that
carries the props is published (see the module note below).

---

## Item 5: opt-in global package aliases (Imports)

**Imports, add after the "Packages from NuGet bring these for you" paragraph:**

> **Opt in to the aliases too.** Set `GoGlobalPackageAliases` to `true` and each package you reference also adds
> its alias as a global using, named for the Go package (`strings`, `hex`, `uuid`), so a file needs no `using` line
> at all:
>
> ```xml
> <GoGlobalPackageAliases>true</GoGlobalPackageAliases>
> ```
>
> It is off by default because a file that also declares the same alias, the form this page recommends, fails with
> CS1537 ("The using alias 'strings' appeared previously in this namespace"): choose one form per project. Two
> packages with one Go name (`math/rand` and `crypto/rand`) both add `rand`, and fail the same way; drop one with
> `<Using Remove="go.crypto.rand_package" />` and alias it yourself. A Go name that is a C# keyword is escaped
> (`@unsafe`).

---

## Measured on the full stdlib pack (not page text)

`dotnet pack go2cs-stdlib.slnx -c Debug --no-build` on the lane tip: 362 nupkgs, 359 carry
`build/<PackageId>.props`. The three without one have no package_info.cs: go.lib, go.gen, and the hand-owned
go.testing (so `testing`'s Go methods would still need `using go;`, which go.lib already adds: testing lives in
namespace `go`, so nothing is lost). Escaped aliases came out as `@unsafe` (go.unsafe) and `@internal`
(log/internal, log/slog/internal, net/http/internal). One namespace caveat worth a sentence if it ever bites:
math/rand/v2 lives in namespace `go.math.rand`, so referencing both math/rand (`go.math.rand_package`) and
math/rand/v2 (`go.math.rand.rand_package`) imports both `go.math` and `go.math.rand`; an UNQUALIFIED
`rand_package` is then ambiguous (CS0104). The aliases, and `go.math.rand_package` written in full, are not.

## The nugetgo (module) side of items 1 and 5

The module packages are packed in this repository, by `src/tools/nugetgo/nugetgo-pack.ps1`, from a generated module
pack project that bundles one assembly per Go package. That project is not a converted project, so the
`$(Product)=='go2cs'` recognition alone does not reach it. The lane's change to the script gives the generated
project `Product=go2cs`, one `GoPackageDirectory` item per packed package, and an import of this tree's
`go.lib.pack.targets`; the one `build/<PackageId>.props` then names every packed package's namespace (and, opt-in,
alias). The script could not be run end to end on the hop line (it packs only against a PUBLISHED go.* closure), so
the first module pack after the release that ships go.lib with the new targets should read the props back out of
the nupkg. Modules already published (hashset 1.0.0, uuid) carry no props until they are repacked in the post-hop
nugetgo wave.
