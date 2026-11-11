# SCOUT — Go methods out of the package class (OQ2)

> **State: SCOUT (P1, 2026-11-11), read-only. Nothing is cut and nothing here is implemented.** Ordered by COORD
> (2026-11-11 15:44Z) after the owner ruled OQ2 of `docs/PLAN-public-surface-stability.md` (branch
> `claude/coord-public-surface-plan`, `530fd3b1bc`): methods move out of the package class so that an exported Go
> name never takes a `Δ` because a method has the same name. This note gives the shape, the census, the
> interactions and what is left of the `Δ` rule, for COORD to rule before the move, which is its own batch.
> Every claim is marked **measured** (with its instrument) or **inferred**. Read at master `742a3dbb24`.

## 1. What the rule costs today (measured)

A package's types, constants, variables, functions and methods are all members of one C# class,
`<pkg>_package`. A method `Month` and a type `Month` cannot both be members (CS0102), so
`performNameCollisionAnalysis` renames the type, constant or variable to `ΔMonth`.

Instrument: a `go/types` census over `std`, one run per target, and over the behavioral workspace.

| | linux/amd64 | windows/amd64 | darwin/amd64 | linux/arm64 | behavioral (linux) |
|---|---|---|---|---|---|
| packages (declaring a method) | 360 (243) | 362 (243) | 360 (243) | 358 (243) | 1,022 (413) |
| methods / receiver types | 10,284 / 1,993 | 10,287 / 1,997 | 10,297 / 2,002 | 10,296 / 1,992 | 1,268 / 744 |
| renamed declarations: const / type / var | 32 / 178 / 15 | 32 / 180 / 15 | 32 / 178 / 15 | 32 / 178 / 15 | 3 / 40 / 2 |
| of them exported: const / type / var | 29 / 133 / 7 | 29 / 135 / 7 | 29 / 133 / 7 | 29 / 133 / 7 | 3 / 35 / 0 |
| Go references to them: same package + other packages | 5,298 + 2,593 | 5,449 + 2,838 | 5,284 + 2,594 | 5,309 + 2,593 | 147 + 64 |

- **The renamed set differs per GOOS**, which the plan listed as not measured: windows renames two names the
  other targets do not (`syscall.Handle`, `syscall.Sockaddr`). A consumer compiles against the Windows surface.
- **The plan's 161 / 114 / 63 reconcile.** A grep of `src/core` for a `Δ` type DECLARATION (non-test files, golib
  excluded) reads 174 distinct (package, name) pairs; the census reads 180 over the four targets; 8 census types
  are declared in a form the grep's pattern does not match (not checked one by one), and 2 emitted types are the
  lifted types of renamed variables (`runtime.Δtrace`, `runtime.Δscavenge`). No `Δ` type declaration in
  `src/core` comes from any other rule.
- What a reader sees today (`encoding/json`): `ᴛ == reflect.Array || ᴛ == reflect.Map || ᴛ == reflect.ΔSlice`.

## 2. The C# facts that fix the shape (measured: SDK 10.0.403, LangVersion 14, small compiled cases)

1. **A second class is unavoidable.** C# 14 `extension(T t) { … }` blocks in the SAME class as a nested type or
   constant of that name are still CS0102. Extension methods must sit in a top-level static class, so the second
   class is a sibling of the package class, never nested in it.
2. **From the package class everything resolves as Go wrote it**: the type, the constant, a call `t.Month()`.
   A method EXPRESSION must name the holder (`timeᴛTime.Month`).
3. **Inside the holder, simple names resolve differently**, and three cases are silent:
   - a name equal to one of the holder's methods binds the METHOD GROUP. For a constant or variable that is a
     compile error (CS0019, CS1656, CS0428). For a free function with another arity it is CS1501. **For a free
     function whose parameter list fits, the bare call compiles and runs the METHOD** (the probe printed
     `method norm` where Go calls the function).
   - the package's own names reach the holder through `using static <pkg>_package`, which ranks BELOW
     namespace-level names and MERGES with other static imports. A package type named like a namespace-level
     type bound the namespace's type, with no error. A package function `max(double, double)` called with two
     ints bound a generic `max<T>` from a second static import, with no error. A package variable named like a
     field of the second import is CS0229.
   - a type named like a method is fine wherever C# reads a type (`x is T`, `case T v:`, casts, `typeof`,
     generic arguments, `new T`), and an error as `case T:` (CS0150) or as the left of `T.member` (CS0119).
4. **Qualifying the reference repairs every case**: `time_package.Second`, `time_package.Unix(a, b)`.
5. **A consumer** with only `using time = go.time_package;`, or with `using static go.time_package;`, gets CS1061
   on `t.Month()`. With `using go;` (the namespace) it compiles; `time.Month`, `time.Second`, `time.Now()` read
   through the alias as in Go.

## 3. The shape proposed

```csharp
// package_info.cs, once per receiver type that declares a method
[GoMethodHost(typeof(Time))] public static partial class timeᴛTime;

// time.cs, in Go's declaration order
partial class time_package {

partial struct Month /*num:nint*/;
public static Month January => /* 1 + iota */ 1;

} partial class timeᴛMonth {

// String returns the English name of the month ("January", "February", ...).
public static @string String(this Month m) {
    …
}

} partial class time_package {
```

- **One holder per RECEIVER TYPE, named `{pkg}ᴛ{Type}`**, recommended over one per package (`time_packageᴛ`):

  | | per receiver type | per package |
  |---|---|---|
  | references that must be qualified inside method bodies (linux/amd64) | 260 in 169 methods | 656 in 385 methods |
  | of them calls to free functions, which read plainly today | 72 | 357 (`runtime` 163) |
  | of them constants / variables, which carry a `Δ` today | 174 / 14 | 242 / 57 |
  | holder classes | 1,993 | 243 |
  | class-switch lines if switched in place | 6,000 | 5,314 |

  It is Go's own shape (a method belongs to its type; linker symbols are per type), the name states the receiver
  to a reader, the class is found by an attribute that carries the receiver type exactly as `[GoPromotedHost]`
  is, and no class approaches the method limit. The three qualification rows agree within 2 across the four
  targets; the class and switch rows are linux/amd64 (1,992 to 2,002 classes and 5,999 to 6,024 lines elsewhere).
- **Switched in place, not reordered.** A line `} partial class timeᴛMonth {` before each run of one type's
  methods, and `} partial class time_package {` after it, keeps Go's declaration order, the comments and the
  position maps. 848 of 1,674 files declare a method (linux/amd64). The alternative, the package block first
  and the methods after it, would reorder 663 files.
- **The visual cost, linux/amd64**: 6,000 switch lines, 848 `using static` lines and 260 qualified references
  (plus the bounded cases below) are added; the `Δ` leaves 225 declarations and the 7,891 Go references to them.
- **The holder is separate from the promoted-method class** (`{pkg}ᴛ{Struct}ᴛpkg`). Sharing one class would add
  every promoted name to the names that bind first inside the type's own method bodies.
- **Qualification is by rule, never by compile error**, because three cases are silent (§2.3). Inside a method
  body a package-level simple name is written `<pkg>_package.X` when its name is (a) a method of that holder,
  (b) a public static member of `go.builtin`, (c) a public type of namespace `go`, (d) a child namespace in
  view. Upper bounds for (b), (c), (d) on each target: 75, 86 and 168 references (golib's names read by
  reflection: 215 public types in `go`, 82 public static members of `builtin`); (c) and (d) are bounds because
  arity and the reference closure clear most of them.
- **A guard that makes the silent cases loud** (proposed, to be red first): an analyzer in go2cs-gen that
  reports a simple name inside a holder that binds a holder method, a `builtin` member or a namespace type while
  the package class declares that name. It covers hand-owned bodies, which no census of Go source can see.
- **`using static <pkg>_package` then brings** the types, constants, variables and functions, and no method.
  Converted code already finds methods by namespace (`addMethodPackageNamespaceUsing`), as do the promoted
  classes. A Go DOT IMPORT is emitted as `using static` of the package class alone today, so it needs the
  namespace as well (inferred from `visitImportSpec.go:327`; not probed).

## 4. Prototype (measured; a text transform of the emitted package, built with the unchanged generators)

| Package (linux, production files) | `Δ` occurrences removed | methods moved | switch lines | qualified | errors in converted files | errors in hand-owned files |
|---|---|---|---|---|---|---|
| `time`, per receiver (16 classes) | 161 | 101 | 72 | 8 | 0 | 0 |
| `time`, per package | 161 | 101 | 60 | 15 | 0 | 0 |
| `reflect`, per receiver (16 classes) | 844 | 182 | 120 | 36 | 0 | 110 |
| `reflect`, per package | 844 | 182 | 114 | 47 | 0 | 119 |

The untouched copies build with rc 0. `reflect` is the worst case on purpose: it holds `ΔType`, `ΔValue` and
`ΔKind`, 99 of the 174 constants of the per-receiver column, and 10 of its 16 production files are hand-owned.

- **Hand-owned files do not follow a reconvert.** Their errors: 42 × CS0122 (a `private static` helper of the
  package class is out of reach of a sibling class), names the Go census cannot see in hand-written bodies, and
  `Type` meaning System's type where the file wrote `ΔType` for Go's. In `src/core`, 115 of 379 hand-owned
  files declare 1,253 extension methods. This is a hand leg of the move, done by script plus the analyzer.
- **`RecvGenerator` writes its `ж<T>` twin into `<containing class>_package`** (`timeᴛTime_package`). It
  compiles, because any top-level static class can host an extension, and it is the wrong class.
- **`ExtensionHostListGenerator` listed every holder unchanged**, so the Native AOT host list follows.
- The first `reflect` build showed 3 errors and the second 126 to 137: an error in a `global using` line hid
  the errors in method bodies. Two of the three were the transform's own (it stripped another package's `Δ`).

## 5. What the move changes (read at the tree; path and line in the scratch inventory, on request)

- **Converter.** `visitFile.go:175` opens the one class block; the switch lines are new. The only static-form
  method references the converter emits are method expressions (`convSelectorExpr.go:1584-1628`): 81 in `std`,
  44 in the behavioral tree. Rules that exist only because methods share the class, each to be re-derived and
  not merely deleted: the element rename and its exported aliases, `packageBuiltinShadows`,
  `packageFuncMethodNames`, B2, B9, `resolveReceiverParameterCollisions`, `funcRenamedForMethodCollision`,
  `applyMethodGroupClashCast`, the white-box bridge's shadow list. Each has a mirror inside the holder (§2.3).
- **Generators.** `GoMethodHosts` (`PackageMethods`, `StaticClass`) is the single lookup the promoted move left;
  11 files reach methods through it or through `ContainingType.GetMembers`, among them `MethodSetKeep`'s
  `[DynamicDependency(name, typeof(package))]` trimming roots. `RecvGenerator`, `TemplateBase.TemplateHeader`
  and `StrGenerator` place output by a class name ending `_package`.
- **Run time.** Go function names for frames, `runtime.Caller` and `FuncForPC` are built from the declaring
  class: `goImportPathOf` and `classifyGoSourceFrame` in `runtime/managed_impl.cs` require a name ending
  `_package`, as do `GoSyntheticPC.GoNameOf`, `GoReflect.goPackageNameOf` and `RuntimePanicCheck`. Closure
  names walk out to a class ending `_package`. **Predicted with the attribute mapping: no Go-visible name
  changes** (`time.Time.Month`, `time.(*Location).String`, `….func1`). The frame tests are the instrument.
- **Hand-owned static-form calls of methods**: `abi.Kind(Ꮡt)`, `abi.Elem(x)` and others in
  `reflect/value_impl.cs`, `runtime/arena_impl.cs`, `runtime/managed_impl.cs`; reflection on a method by name in
  `testing/TestExecution.cs` and `testing/TestFlagBridge.cs`. Not scanned: the other hand-owned files.
- **The break is owned the way the promoted move owned it**: generated API-compat suppressions that accept a
  removed member only where a `[GoMethodHost]` class declares it for the same receiver, a `GoBreakFloor` move,
  release notes. Every published nugetgo package is rebuilt.

## 6. Predictions for the move, stated before any reconvert

- **Standard library, two-seeded, one target per run**: converted files that differ = Go files that declare a
  method or reference a renamed name: 952 / 958 / 957 / 951 of 1,674 / 1,600 / 1,669 / 1,669 (linux/amd64,
  windows/amd64, darwin/amd64, linux/arm64), in 253 / 254 / 253 / 253 packages, plus one `package_info.cs` per
  such package. Line kinds: the two switch lines, a name losing its `Δ`, a qualified reference, a method
  expression naming the holder, the `using static` line. Any other kind is a miss.
- **Behavioral tree**: 420 of 1,022 packages (426 files) move, with their goldens. `check-no-regression.ps1`
  must read every other golden unchanged.
- **Tests**: not measured. The `-tests` variants and the white-box bridge are the least-read part of this note.
- Named arm most likely to miss, and why: the qualification counts, because the census reads Go bodies and the
  emitter adds references Go did not write (field-address tokens `T.of(…)`, lifted declarations beside a method).

## 7. Interactions

- **Method limit (GO2CS0012).** The count is per type. The package class loses every method; the largest
  holder is a type's own method set. The promoted classes are untouched.
- **C1's tier-2 `[EditorBrowsable]` pass.** A holder's NAME is plumbing a consumer never writes, but it is part
  of each method's binary identity, and methods are tier 1. So the naming rule `{pkg}ᴛ{Type}` is frozen with
  tier 1; it is a function of two names Go already keeps stable. Whether `[EditorBrowsable(Never)]` on the
  class hides its extension methods from completion is **not measured** (no IDE on this box): C1 should measure
  it before marking a holder.

## 8. What remains of the `Δ` rule afterwards (read from the converter and from the emitted tokens)

- a type named `any`, `rune`, `nint` or `nuint` (`emitterSpelledTypeNames`), or after a C# 15 reserved type word;
- golib's reserved names (`identifierNaming.go:171-181`), a name ending `_package`, and `Main`;
- a struct field named like its own struct (CS0542: `l.ΔLinger`, `a.ΔAddress`);
- an import alias that equals a namespace in view (`using Δsync = sync_package;`: 262 lines, 18 aliases);
- shadowed locals and parameters (`xΔ1`, `Δp0`), the largest group by count; golib's own `ΔClone`.

None of these depends on a method name. An exported package-level Go name then takes a `Δ` only from the fixed
lists of the first two lines, which no Go release can extend.

## 9. For COORD to rule

1. Holder per receiver type (`timeᴛTime`, recommended) or per package.
2. Switched in place (recommended) or the package block first and the methods after.
3. The qualifier spelling inside method bodies: `time_package.Second` (recommended: it is what the emitter
   already writes for a shadowed import alias) or a per-file alias.
4. The analyzer of §3 as the move's red-first guard, and its diagnostic number.
5. Whether the hand-owned leg rides the same batch (recommended: the tree does not build without it).
