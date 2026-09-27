# Package Conversion: Per-GOOS Layout

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers how a package's platform-specific sources are laid out and selected at build time (layout L3 and `$(GoTargetOS)`), and in which platform builds a hand-owned file takes part.

## Layout L3

### Per-GOOS sources: layout L3 and `$(GoTargetOS)`

Go selects its platform sources at *build* time — filename suffixes and `//go:build` constraints — so a
conversion does not merely target a platform, it **is** that platform. A converted package whose emission
varies across `GOOS` therefore keeps the varying files in per-GOOS subfolders, and the `.csproj` compiles
exactly one of them; files that are byte-identical on every platform stay flat. This is layout **L3**
([`phase4/DESIGN-multiplatform-corpus.md`](../../phase4/DESIGN-multiplatform-corpus.md) §8, accepted 2026-08-08).
`internal/goos` is the first package to carry it:

```
src/core/internal/goos/goos.cs                    shared by windows, linux and darwin
src/core/internal/goos/package_info.cs            shared
src/core/internal/goos/windows/nonunix.cs         public const bool IsUnix = false;
src/core/internal/goos/windows/zgoos_windows.cs   public static readonly @string GOOS = @"windows"u8;
src/core/internal/goos/linux/unix.cs              public const bool IsUnix = true;
src/core/internal/goos/linux/zgoos_linux.cs       public static readonly @string GOOS = @"linux"u8;
src/core/internal/goos/darwin/…
```

Such a package's `.csproj` gains exactly two blocks — the selector, and the include that reads it:

```xml
<PropertyGroup Condition="'$(GoTargetOS)'==''">
  <GoTargetOS>windows</GoTargetOS>
</PropertyGroup>
…
  <Compile Remove="**/*.cs" />
  <Compile Include="*.cs" />
  <Compile Include="$(GoTargetOS)/*.cs" />
```

The include must follow `<Compile Remove="**/*.cs" />`, which would otherwise remove it — MSBuild evaluates
items in document order. The property may sit anywhere (properties are evaluated in an earlier pass) but is
declared above the item that reads it. `windows` is the default because the corpus that exists today is the
Windows emission, so a plain `dotnet build` reproduces the single-platform package this layout replaced:
verified byte-identical, and `-p:GoTargetOS=windows` produces the same assembly as the property absent.

**Only a package that varies gets the blocks.** The design measures the varying set at 37 of 304; the rest
emit the same C# on every platform and keep exactly the project file they always had.

**Which files are per-GOOS cannot be decided by one conversion.** The platform axis is a comparison of
several targets' *emissions*, and it is emphatically not derivable from Go file sets: four packages emit
different C# from identical Go source (constant folding, escape analysis, cross-file collision renaming,
dead-branch folding), and three emit identical C# from differing Go source. So the layout is produced by a
multi-target run — `go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64` — which
converts once per target into a seeded staging root, classifies every emitted artifact (shared / variant /
partial / exclusive), and merges the result into one tree.

A **single**-target conversion instead *honors* a layout the output tree already carries: if the package
directory holds `<goos>/<name>.cs`, that is where `<name>.cs` is written, and a package directory holding
any per-GOOS source folder gets the two blocks. That is what makes an ordinary seeded reconvert reproduce
an L3 package file for file, instead of laying a flat duplicate beside the copy the project is already
compiling — a duplicate-member break that would otherwise arrive silently. A per-GOOS folder is
distinguished from a nested package by the project file every converted package directory holds and a
source folder never does: `internal/syscall/windows` is a real package whose own directory name is a GOOS.

Guarded by `platformLayout_test.go` (`src/go2cs`), that negative case included.

**A HAND-OWNED file has a platform too, and the classifier cannot see it.** L3 decides placement by
comparing *emissions* — and a hand-owned file is never emitted, so it is never classified, so it silently
keeps whatever placement it had while the file it belongs to moves per-GOOS. That is invisible on Windows,
where the compile item set is identical either way, and it took the entire Linux corpus down at increment 3:
`runtime/lock_sema_impl.cs` supplements `lock_sema.cs`, which Go selects on Windows *and* macOS but never on
Linux, so the Linux build compiled the flat companion against a principal that was not in its build.

The rule is stated as a platform **set**, not as a folder: *a hand-owned file belongs in exactly the platform
builds its principal takes part in*, after which L3's ordinary placement rule applies unchanged — every
platform ⟹ flat, a subset ⟹ one copy per platform in the subset. Read literally as "inherit the principal's
folder", `os/proc_impl.cs` and `syscall/syscall_impl.cs` would be triplicated: their principals are per-GOOS
*variants* present on all three platforms, so three copies of one hand-written file would have to be
maintained in lockstep for no compile benefit. L3 duplicates only what cannot be shared.

A principal comes in two shapes, and both are already recorded in the tree by the emission itself:

| Hand-own | Principal | Why |
|:--|:--|:--|
| `<name>_impl.cs` | `<pkg>/<name>.cs` | a companion with no Go counterpart of its own; it *supplements* the converted file |
| `<name>.cs` carrying `[module: GoManualConversion]` | `<pkg>/<name>.cs.auto` | the conversion still runs and only its EMISSION diverts, to the review sibling — which is therefore emitted by exactly the platforms that compile the Go file this hand-own replaces |

The second binding is the one that catches a whole-file hand-own of a platform-exclusive Go file:
`syscall/dll_windows.cs` and `syscall/exec_windows.cs` replace Windows-**only** sources and now live in
`syscall/windows/`. The `.cs.auto` sibling moves with its `.cs` so the pair cannot separate — which is also
what a single-target reconvert already does, since `conversionDriver` routes both through
`platformLayoutPath`. A hand-own whose principal no target emitted (`runtime/managed_impl.cs`,
`internal/poll/runtime_sema_impl.cs` — go2cs machinery with no Go file behind it) has no placement evidence
and is left alone. Two existing copies of one hand-own that disagree are an **error**, never a first-wins
choice: a duplicated hand-own is hand-maintained in each folder, so propagating one over the other is how a
fix applied to a single flavor would disappear.

⚠ **A hand-owned FUNCTION is a different problem with no layout answer.** `manualConversionFuncs` is keyed by
NAME and is platform-blind, so an entry turns its Go declaration into a placeholder on *every* platform while
the implementation exists only where one was written — and `notetsleep_internal` is even four arguments in
`lock_sema.go` against two in `lock_futex.go`. Census and remedy in
[`phase4/DESIGN-multiplatform-corpus.md`](../../phase4/DESIGN-multiplatform-corpus.md) §7.

Guarded by `platformHandOwn_test.go`, which walks the **real** `src/core` rather than a synthetic tree —
the next offender will be a file somebody adds by hand. Three structural rules: an `*_impl.cs` whose
principal is in some but not all of its package's per-GOOS folders must be in exactly those; a `.cs.auto`
lives beside the `.cs` it reviews; and a source carrying Go's own GOOS filename constraint (`*_windows.cs`,
`*_linux.cs`, `*_darwin.cs`, with an `_impl` suffix stripped first) is never flat in an L3 package. That
third rule exists because a static walk cannot find a *marked* hand-own's principal — it is what would have
caught `dll_windows.cs`/`exec_windows.cs` a whole increment earlier.

**References are conditioned by the same rule, one level up.** A package's *direct imports* can differ by
`GOOS` too — measured at **21** packages, `os` being the clearest: it imports `internal/syscall/windows` on
Windows and `internal/syscall/unix` on Linux and macOS. One flat reference list cannot say that, so the
references common to every platform stay unconditioned and only the differences are selected:

```xml
  <ItemGroup>
    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
    …the 16 references os has on every platform…
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='darwin'">
    <ProjectReference Include="$(go2csPath)core/internal/syscall/unix/internal.syscall.unix.csproj" />
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='windows'">
    <ProjectReference Include="$(go2csPath)core/internal/godebug/internal.godebug.csproj" />
    <ProjectReference Include="$(go2csPath)core/internal/syscall/windows/internal.syscall.windows.csproj" />
  </ItemGroup>
```

**A platform whose delta is empty still gets a group**, written self-closing
(`<ItemGroup Condition="'$(GoTargetOS)'=='linux'" />`). That is not noise, and it is the one detail the whole
mechanism rests on: the shared list is an *intersection*, and a single-target reconvert recovers the other
platforms' sets from the file it is about to overwrite. Forget that linux takes part and the next reconvert
computes the intersection over two platforms instead of three — promoting a Windows-only reference into the
shared list, where it would land in the Linux build. The empty group records membership so the axis survives.

Both producers — the multi-target merge and a single-target reconvert — go through one renderer, which is
what makes the reconvert reproduce the merge's bytes rather than something merely equivalent. When every
platform's imports agree again the block disappears and the project file returns to its plain form.

Two facts that are NOT references still have to be reconciled, because one `.csproj` serves every platform:
`<AllowUnsafeBlocks>` is emitted as the **union** across targets (it differs in `os/user` and `syscall`; the
property grants a capability rather than using one, so raising it is inert where unused), and every other
companion artifact — README, icons, `.cs.auto` — is taken from the **first target in `-platforms` order**,
the reference flavor, so the choice is deterministic instead of depending on which target happened to have
something to rewrite.

Guarded by `platformProject_test.go`, whose central test is the invariant itself: a single-target reconvert
of every platform must reproduce the merged project file byte for byte.

## Hand-owned files

### Where a hand-owned FILE is built

* **Where a hand-owned FILE is built** is layout L3's question, answered by the file's *principal*: an `*_impl.cs` takes part in exactly the platform builds the `<name>.cs` it supplements does; a marked whole-file hand-own follows its `.cs.auto` review sibling. Every platform ⟹ flat; a subset ⟹ one copy per platform in the subset. Routed automatically by the reconvert merge and guarded by a walk of the real corpus (`platformHandOwn_test.go`).

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
