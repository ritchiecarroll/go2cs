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

## Amendment 2026-11-11 — the rulings, and the cut plan for the move

> Appended, not rewritten: sections 1 to 9 above stand as posted. COORD ruled section 9 on 2026-11-11 (20:35Z,
> 20:43Z, 21:06Z). This block records the rulings, measures what the owner's self-alias amendment asked for, and
> gives the cut plan with its predictions. Nothing is cut. Read at master `742a3dbb24`.

### 10. The rulings

1. One holder per RECEIVER TYPE (`timeᴛTime`), declared once in `package_info.cs` with the receiver-type
   attribute.
2. Switched in place, so Go's declaration order stays.
3. **The qualifier is a per-file self-alias** (the owner's amendment): where a holder must qualify a
   package-level name of its OWN package, the file declares `using time = time_package;` and writes `time.Now()`.
   It falls back to `time_package.X` exactly where the alias name is shadowed at the use, decided from Go's
   scopes and never from a compile error. The alias goes through the existing CS0576 rename pre-pass
   (`computeImportAliasRenames`), and only a file that needs it declares it.
4. The analyzer that makes the silent cases loud is cut red first, and understands the alias form.
5. The hand-owned leg and the `RecvGenerator` class fix ride the same batch.
6. Each holder TYPE carries `[EditorBrowsable(Never)]`; its methods never do (C1's measurement).
7. The move is its own batch. The `-tests` variants and the white-box bridge are measured inside it.

### 11. The self-alias, measured

Instrument: the scout's typed census, extended (`go/packages`, `go/types`; one run per target over `std`,
and one over the behavioral workspace of 968 modules). For each package-level name a method body uses, it reads
whether the holder must qualify it and why, then looks up the package's own name at the use
(`Scope.LookupParent`), and applies the CS0576 test of `computeImportAliasRenames` to the package's own name.

| `std` | linux/amd64 | windows/amd64 | darwin/amd64 | linux/arm64 |
|---|---|---|---|---|
| references a holder must qualify | 356 | 358 | 356 | 356 |
| … because the name is a method of the holder (exact) | 260 | 262 | 260 | 260 |
| … because it is a `go.builtin` member (bound) | 71 | 71 | 71 | 71 |
| … because it is a type of namespace `go` (bound) | 25 | 25 | 25 | 25 |
| … because it is a child namespace in view | 0 | 0 | 0 | 0 |
| written through the alias (`time.X`) | 330 | 332 | 330 | 330 |
| written through a RENAMED alias (`Δsync.X`) | 18 | 18 | 18 | 18 |
| fall back to `<pkg>_package.X` | 8 | 8 | 8 | 8 |
| methods touched | 230 | 232 | 230 | 230 |
| files that gain the alias line (of them renamed) | 65 (7) | 67 (7) | 65 (7) | 65 (7) |
| packages that gain it (of them renamed) | 35 (5) | 36 (5) | 35 (5) | 35 (5) |

- **No reference falls back because of a local, a parameter, a result or a receiver.** A free function keeps
  the package class and qualifies nothing, so `func Clean(path string)` never meets the alias; only method
  bodies do. 18 methods in `std` declare a variable named like their package (`flag`, `net`, `url`, `types`,
  `doc`), and one reference to qualify sits in such a method (`go/types`, `Checker.builtin`), outside the
  variable's scope.
- **The 8 fallbacks**, the same on every target:
  - 7 in `vendor/golang.org/x/crypto/chacha20poly1305`: the package declares a TYPE named like itself, so the
    alias would take the type's name in every holder of the file;
  - 1 in `html/template`: `template.go` imports `text/template`, whose alias already owns the name.
- **The renamed alias** is 18 references in 7 files of 5 packages whose own closure holds a child namespace of
  the same name: `sync` (5), `text/template` (4), `go/build` (4), `crypto/tls` (3), `net/http` (2). There the
  pre-pass gives `using Δsync = sync_package;`, the spelling an importer of `sync` in namespace `go` reads today.
- **Behavioral tree**: 422 of 1,038 packages declare a method. 4 references must be qualified (1 exact, 3
  bound), all in `package main`. So the corpus exercises none of this: the move brings its own fixtures.
- The bound rows are upper bounds, as in section 3: arity clears most `go`-type matches. The exact row is the
  Go source's; the emitter adds references Go did not write (section 6's named arm).

**C# facts for the alias, measured** (SDK 10.0.403, LangVersion 14; three compiled cases):

- With `using main = main_package;` and `using static main_package;` in one file, a holder's `main.Sort(3)` and
  `main.Limit` bind the package class, and a bare call `main()` still binds the function. A FUNCTION named like
  its package does not shadow the alias.
- A TYPE named like its package does: with the alias, `this chacha c` is CS0721, because the alias names the
  static class. The fallback form compiles and runs.
- A parameter named like the alias binds first, with no diagnostic when its type has a member of that name:
  the probe printed the parameter's field (2) where the package's variable holds 7. This is the silent case.
- C# scopes a local over its whole block. The alias used before a later declaration of that name in the same
  block is CS0841; a nested block may declare the name for itself. So the shadow test reads C#'s block, not
  Go's point of declaration. Neither `std` nor the behavioral tree has such a reference.
- Not measured: a package-level VARIABLE or CONSTANT named like its package. No package that qualifies a
  reference declares one, so the plan sends that shape to the fallback.

### 12. The analyzer (`GO2CS0013`, proposed number)

`GO2CS0011` is taken by the `-warn` work and the internal-package warning takes its own, so this takes
`GO2CS0013` unless COORD assigns another. It reports, as an error, inside a class marked `[GoMethodHost]`:

- a simple name that binds a method of the holder, a `go.builtin` member or a type of namespace `go` while the
  holder's package class declares a member of that name (the three silent cases of section 2);
- **the alias arm**: in a file that declares the self-alias, a member access `time.X` where `time` binds a
  local or a parameter and the package class declares `X`. That is the one shape in which a shadowed alias
  compiles and reads another member.

The alias arm has a known false-positive shape in correct Go: a selector through a variable named like the
package, of a name the package also declares. `std` has 7, in two files: `flag.Value` (a field of a `*Flag`
named `flag`; the package has the type `Value`) four times in `flag/flag.go`, and `url.User` three times in
`net/url/url.go` (the package has the function `User`). **Proposed rule, for COORD to confirm**: a file that
holds such a selector in a method does not take the self-alias and falls back. In `std` that is `url.go` alone
(`flag.go` qualifies nothing), which moves 2 references from the alias to the fallback (8 becomes 10). The
analyzer then has no false positive on converted code, and stays exact for hand-owned bodies.

### 13. The cut, in order

A new worktree from the master that carries batch 286. Each step is its own commit; a red commit comes first
wherever a step changes behavior or emission.

1. **Analyzer, inert on the tree**: `GO2CS0013` in `go2cs-gen` with its GenTests (the silent cases compile
   without a diagnostic at the red commit), its registry entry and its row in `docs/DiagnosticCodes.md`. No
   class carries `[GoMethodHost]` yet, so no existing project changes.
2. **Generators and run time, behind the attribute**: `GoMethodHostAttribute` in golib;
   `RecvGenerator`, `TemplateBase.TemplateHeader` and `StrGenerator` place output in the method's own class;
   `GoMethodHosts` and `MethodSetKeep` find methods through the attribute; frame and function names
   (`goImportPathOf`, `classifyGoSourceFrame`, `GoSyntheticPC.GoNameOf`, `GoReflect.goPackageNameOf`,
   `RuntimePanicCheck`) map a holder to its package. GenTests and GolibTests red first.
3. **Red for the emission**: behavioral projects for the forms the corpus lacks (a constant, a variable, a type
   and a function named like a method; each reason to qualify; the alias, the renamed alias and each fallback;
   method expressions and method values; a dot import; generic, pointer, value and alias-typed receivers;
   promoted methods beside declared ones), and converter tests that state the new emission.
4. **Converter**: the switch lines and the holder declarations; `using static` of the package class in a file
   with a holder; qualification by rule with the self-alias; method expressions name the holder; a dot import
   brings the namespace; the element rename and its exported aliases are removed, and each rule of section 5
   that exists only because methods share the class is re-derived, not deleted blind.
5. **Hand-owned leg**: the 115 files that declare extension methods, by script and then by the analyzer;
   static-form calls of methods and reflection on a method by name (section 5).
6. **Goldens**: every behavioral project that declares a method or names a renamed element.
7. **`src/core`**: the seeded reconvert per target, as its own commits.
8. **Compatibility and docs**: the generated API-compat suppressions and the `GoBreakFloor` move (section 5);
   `ConversionStrategies` and its reference; NEWS; the contract line for C1 (`using static <pkg>` no longer
   brings methods; a consumer uses the namespace).

### 14. Predictions, stated before any reconvert

- **`std`, two-seeded, one target per run**: 952 / 958 / 957 / 951 converted files differ, in 253 / 254 / 253 /
  253 packages, plus the `package_info.cs` of each such package (243 of them on each target declare holders).
  To be read again at the cut's own base.
- **Line kinds, linux/amd64**: 6,000 switch lines; 848 `using static` lines; 65 alias lines; 225 declarations
  and their references lose the `Δ`; 356 qualified references at most (330 alias, 18 renamed alias, 8
  fallback; 10 fallback and 328 alias if section 12's rule is confirmed); 81 method expressions name a holder;
  1,993 holder declarations in `package_info.cs`. Any other kind is a miss.
- **Behavioral**: 429 packages (435 files) move with their goldens; `check-no-regression.ps1` reads every other
  golden unchanged apart from the new projects.
- **Go-visible names** (`runtime.Caller`, `FuncForPC`, panics, `reflect`): none changes.
- **Named arm most likely to miss**: the qualification counts, for section 6's reason, and the hand-owned leg,
  which no census of Go source can size (the `reflect` prototype read 110 errors in 4 files).
- **Not predicted**: the `-tests` variants and the white-box bridge. They are measured inside the move, before
  its goldens are minted.

### 15. For COORD

1. Confirm `GO2CS0013`, or assign the number.
2. Confirm section 12's file rule for the analyzer's alias arm.
3. **`package main`**: Go's `func main` is a package-level name equal to the alias, so by Go's scopes every
   `package main` would fall back. By section 11's measurement the function does not shadow the alias in C#.
   Recommended: a function named like its package does not force the fallback (`main.Sort(x)`); a type does.
   The behavioral tree has 4 such references and `std` none.
4. Who regenerates `src/core` in the batch (step 7), and where the batch sits against the linux/arm64 bank
   waves, whose rows are read on the corpus this move rewrites.

## Amendment 2026-11-12 — corrections found while writing the emission, before any reconvert

> Appended, not rewritten: sections 1 to 15 above stand as posted, and where this block corrects one it names
> it. Nothing below is measured on `std`: the corrections come from three C# measurements against the emitted
> file's real shape and from re-reading the census rows of section 11. Read at master `742a3dbb24`, with the
> converter of the cut in progress (uncommitted beyond the analyzer commit).

### 16. Three facts that change the qualification rule

Measured with SDK 10.0.403, LangVersion 14, each case compiled and run.

1. **The reason "`go.builtin` member" (sections 2 and 11) does not apply to converted files.** A converted
   file declares its usings INSIDE the file-scoped namespace, and `go.builtin` arrives by a global using from
   the project. A `using static <pkg>_package;` inside the namespace is nearer than a global one, so inside a
   holder the package's `max`, `zero` or `cap` binds before builtin's. The probe printed the package's
   `max(double, double)` and its `zero`. The experiment behind section 2 declared both usings at one level,
   which is not the emission's shape. The analyzer keeps its builtin arm: a hand-owned file may place its
   usings differently.
2. **The reason "type of namespace `go`" is exact on arity and on the namespace.** A generic-only type
   (`go.Func<T>`) does not hide a constant or function named `Func`; a non-generic one hides it. A package
   declared in a child namespace (`go.encoding`) reads its own usings before the types of `go`, so the reason
   applies only to a package declared in namespace `go` itself. The 25 references of section 11's row are
   `reflect`'s constant `Func` (20) and `runtime`'s type `Func` (1), which golib's generic-only `Func<…>`
   does not hide, and `runtime`'s type `slice` (4), which is already renamed as a reserved name. The
   expected count in `std` is 0.
3. **A local named like the package is tested per function.** Section 11 read C#'s block scope per use. The
   converter asks whether the function declares a variable of that name anywhere, and falls back for the
   whole function. It is never wrong and costs one reference in `std` (`go/types`, `Checker.builtin`).

### 17. Decisions the emission needed

- A type lifted out of a method's body stays in the package class, where every other type is; the method
  switches back to the package class for it. A literal or constant hoisted out of a method sits in the holder
  beside the method.
- A hoisted string literal field is `private`, so it is kept once per CLASS that uses it, under one name.
- A switch line always stands between two blank lines.
- The holder declarations are a section at the END of `package_info.cs` (`<MethodHolders>`), written only for
  a package that has holders.
- **The self-alias is blocked for the whole package** when the package declares a type, variable or constant
  of its own name, when another package of the same name is in its import closure (`html/template`), or when
  the package is in namespace `go` and golib declares that name. It is blocked for a FILE by an import of the
  same name and by section 12's selector rule; for a FUNCTION by a variable of that name; for a HOLDER that
  declares a method of that name.
- A method expression names the holder: `timeᴛTime.Add` in the type's own package,
  `global::go.net.httpᴛRequest.Write` from another.
- **A method a `_test.go` file declares on a production type gets a second holder**, `<pkg>ᴛ<Type>ᴛtest`,
  wherever the test file and the production file emit into one package class. The production holder then
  never gains a member from a test file, so production emission does not depend on whether tests are compiled.
- A name a dot import brings is written through its package class where a holder hides it. That replaces the
  rename of the test-declared method (B9).
- Retired with the move: the element-versus-method rename and its exported aliases, the function rename for
  a same-signature method (C40), the test-variant renames B2 and B9, and the receiver-parameter collision
  rename. Each existed only because methods shared the package class.

### 18. The predictions of section 14, corrected

`std`, linux/amd64 | windows/amd64 | darwin/amd64 | linux/arm64, from the census rows of section 11 with the
rules above applied. No new run.

| | linux/amd64 | windows/amd64 | darwin/amd64 | linux/arm64 |
|---|---|---|---|---|
| qualified references (section 14: 356 at most) | 260 | 262 | 260 | 260 |
| written through the alias | 240 | 242 | 240 | 240 |
| written through a renamed alias | 9 | 9 | 9 | 9 |
| fall back to `<pkg>_package.X` | 11 | 11 | 11 | 11 |
| files that gain the alias line (of them renamed) | 46 (4) | 48 (4) | 46 (4) | 46 (4) |
| packages that gain it (of them renamed) | 29 (3) | 30 (3) | 29 (3) | 29 (3) |

- The 11 fallbacks: 7 in `chacha20poly1305`, 1 in `html/template`, 2 in `net/url` `url.go`, 1 in `go/types`.
- Unchanged: the files that differ (952 / 958 / 957 / 951 in 253 / 254 / 253 / 253 packages), the holder
  declarations, the method expressions, the 225 declarations that lose the `Δ`, the behavioral count.
- **Line kinds section 14 did not name**, each following from a rule above and none sized by the census:
  one hoisted literal field per class; per-type .NET alias lines in a file with a holder, in place of
  `using System.Runtime.CompilerServices;` (the existing rule for a file that imports static members); the
  `<MethodHolders>` section; the `GoPositionMap` record of every file whose lines move; two more switch lines
  around a method that lifts a type; a dot-imported name written through its class.
- **Falsifiers**: a `builtin.` or golib-type qualification the move adds; a fallback outside the 11; a file
  outside the predicted set whose difference is not one of the kinds named here.

## Amendment 2026-11-12 (second) — four rules re-derived, five more line kinds, before any reconvert

> Appended, not rewritten. Posted to the coordinator before any reconvert ran. Read at master `742a3dbb24`
> with the converter of the cut in progress.

### 19. The rules of section 13, step 4, re-derived

Step 4 says each rule that exists only because methods share the package class is re-derived, not deleted
blind. Four are done. Go's own scoping decides each one:

- **A built-in named like a method.** A package-level FUNCTION named `len` hides the built-in for the whole
  package in Go, so no built-in call of that name can stand beside it. Only a METHOD could shadow, and a
  method now stands in its holder. The call is written through `builtin` only inside that holder.
- **An import named like a method.** Go refuses a package-level function of an import's name in a file that
  imports it, so again only a method could shadow. The reference is written through the import's package
  class only inside the holder that declares the method. A method a `_test.go` file declares stands in a
  holder of its own (section 17), so the production pass no longer reads the test files' declarators for
  this, and the comment that explained such a qualification is no longer written.
- **A function value named like a method.** The function is the only member of its name in the package
  class, so its method group has a natural type again. The delegate cast and the delegate-typed `:=` local
  are no longer written.
- **A call of a function whose argument's type declares a method of the function's name.** The method is no
  member of the class the call names, so C# binds the function as Go does, and the interface cast on the
  argument is no longer written.

Each has a converter test that states the narrowed emission, and the behavioral fixture
`MethodHolderNames` runs all four shapes against Go's output.

### 20. Line kinds g to k, with counts

A typed census of the `go/packages` load of `std`, one build configuration per target:
linux/amd64 | windows/amd64 | darwin/amd64 | linux/arm64.

| kind | what changes | old rule's sites | stay | change |
|---|---|---|---|---|
| g | `builtin.<name>(` returns to `<name>(` outside the declaring holder | 1,097 \| 1,106 \| 1,098 \| 1,097 | 63 | at most 1,034 \| 1,043 \| 1,035 \| 1,034 |
| h | an import written through `<path>_package` returns to its alias outside the declaring holder | 132 in 35 files | 17 | 115 in 32 files |
| i | a function value loses its delegate cast; a delegate-typed `:=` local returns to `var` | at most 11 in 6 files | 0 | at most 11 |
| j | the interface cast on a same-name call's first argument is removed | 3, all in `sort/sort.go` | 0 | 3 |
| k | the white-box bridge's qualification of a production name holds only inside the test method's holder | not counted | | |

- g excludes `make` and `new`, which were never written through `builtin`. "At most" for g and i: some call
  forms never wrote the qualifier, and the cast was written only where the site supplied no delegate type.
- h: the comment "Fully qualified to avoid alias shadowing by the same-package test declaration" disappears.
  The census finds no site of it; the tree has one, `hash/maphash/maphash_purego.cs`, a file outside the
  load.
- One more consequence of kind a (a declaration loses its `Δ`): a field named like its struct type carried two
  markers where the type was renamed for a method (`ΔΔLabel` in `ΔLabel`). The type keeps its name, so the
  field keeps one marker, which C# needs for a member named like its type. Seven files of the tree carry
  `ΔΔ` (`net`, `internal/trace`, `internal/trace/testtrace`).
- **Falsifiers added**: a `builtin.` or `_package` qualification removed INSIDE the declaring holder (the 63
  and the 17); a cast of kind i or j that remains; a g or h change in a file the census does not list.
- **Limit**: the census reads one build configuration per target. A file the corpus converts under other
  tags is outside it and is named in the footprint.

### 21. Open at this amendment

`src/core` commits 1,148 `_test.cs` files and 242 `package_test_info.cs`. `-stdlib` does not emit them, and
after the move a `-tests` pass writes a test file's methods into holders too. Asked of the coordinator:
whether the batch regenerates production only (a row's test files then take the new shape at its next bank)
or re-emits every banked row's test files. The generators and the run-time library read both shapes: the
behavioral projects of the cut run in the new shape against a `src/core` still in the old one.

## Amendment 2026-11-12 (third) — the cut's first commits, a fifth rule retired, two more line kinds

> Appended, not rewritten. Written while the first two-arm reconvert of the cut (linux/amd64) runs and
> before its diff is read. The cut is `claude/p1-oq2-method-holder` at `c07e0b7671` on master `742a3dbb24`.

### 22. A fifth rule re-derived: the rename of a name a method shares (line kind a)

Section 19 re-derived four rules. The fifth is the one section 14 predicts as line kind a. Go keeps a
method in the scope of its receiver type, so a package-level type, constant or variable may share a
method's name. While every method stood in the package class, C# saw two members of one name there, and
the converter renamed the package-level declaration with `Δ`. A method now stands in its holder, so the
package class holds one member of that name and the rename has no cause.

- Removed with it: the export of each rename as a `GoTypeAlias` entry, the derivation of the same rename
  for an unconverted dependency (`foreignNameCollisions.go`), the generic and aliased spellings of a
  foreign renamed type, and the second marker on a field named like a renamed foreign type.
- What the rename table still holds: a type named `any`, `rune`, `nint` or `nuint`, and a declaration
  named `closed`, `union` or `safe`, which C# reads as a keyword in a type position. All seven are
  lower case, so no exported Go name is renamed by the table.
- The test-variant arms that follow a renamed production variable or constant into a `_test.go` file are
  kept. Only an unexported production name can reach them now (`closed`, `union`, `safe`), and the type arm
  is still reached where the emission writes out an inferred type.
- **Prediction added**: runtime's `type slice` stands beside the method `(*userArena).slice` and is also a
  golib reserved word. It was written `Δsliceᴛ`; it is predicted to take the reserved-word spelling
  `Δslice`. Falsifier: any other spelling of it, or a `ᴛ` type mark that remains on a name of the 228-row
  list of kind a.

### 23. Line kinds l and m: a type's name in an expression position inside a holder

C# reads a simple name in an expression position as a member of the enclosing class before it reads it as
a type. Inside a holder that declares a method named like a type of the package, two emitted forms put
that type's name in an expression position, and C# binds the method group (CS8917):

| kind | form | emitted outside such a holder | emitted inside it |
|---|---|---|---|
| l | the box accessor of a field | `T.Ꮡf` | `<pkg>_package.T.Ꮡf` |
| m | a type-switch label with no binding | `case T:` | `case T _:` |

- Kind l names the package class and not the file's alias of its own package. Every box accessor that
  qualifies its type already names the package class, and the alias exists only in files whose holder
  qualifies a value.
- Kind m uses the discard pattern because a pattern's type position reads only types.
- Counts: not taken; the typed census of section 20 does not read these two forms. **Falsifier**: a line
  of kind l or m outside a holder that declares a method of that type's name, or a CS8917 at such a site
  in the compile arm.

### 24. Found while writing the cut (measured 2026-11-12 at `c07e0b7671`)

- Gates at the tip: full `go test` in `src/go2cs` ok; GenTests 518 of 518; GolibTests 2,574 pass, 61
  skipped. Each red commit was read red first: 6 generator tests and 3 run-time tests at the first, 12 of
  12 converter tests at the second (on an archive copy of that commit).
- `MemberRecordGenerator` writes its trimming keep-alive once per OUTERMOST class. A holder is an outermost
  class, so a package with a recorded method in a holder gets two keep-alives. That is correct: golib finds
  a recorded method through its declaring type. The GenTests test that required exactly one tree now
  requires one per class.
- The behavioral project `FuncValueMethodClash` does not build on the cut before the `src/core` regen:
  `time.After` is read beside the method `Time.After` while `core/time` is in the old layout (CS8917). It is
  expected to build once `time`'s methods stand in holders. The same holds for any hand-owned package: where
  a function and a method share a name, the method has to move for a caller's method group to be unambiguous.
- Two gaps that predate the cut and are not chased in it (BOARD candidates): a foreign UNEXPORTED renamed
  type (`union`) spelled through inference in an external test package, and the descriptor carrier's name
  for such a type.
- A consequence of the cut still to be measured: a `private` member of the package class is not reachable
  from a holder (CS0122). The converter's own private members (hoisted literals) go with the method that
  uses them; a hand-owned file may hold one that a holder reaches. The compile arm will show each.
- Open in the generators: `GoMethodHosts` reads a type's holders from the type's own assembly. A holder a
  `_test.go` file declares for a production type is in the test assembly. The `-tests` run of this cut
  measures it.

## Amendment 2026-11-12 (fourth) — the first footprint scored, seven defects of the cut, eight more line kinds

> Appended, not rewritten. Written after the linux/amd64 footprint of the cut at `c07e0b7671` and the full
> behavioral runner at that commit, and BEFORE the footprint of the fixed cut is run. The fixes and the merge
> of master `06610aaa65` (batch 286) are local commits on `claude/p1-oq2-method-holder`.

### 25. The first footprint, linux/amd64, scored (cut `c07e0b7671` against master `742a3dbb24`)

Two seeded `-stdlib` reconverts, one per converter, both seeded from the cut tree, both exit 0. 965 converted
`.cs`, 19 `.cs.auto` and 251 `package_info.cs` differ: 1,235 files.

| | predicted | measured | score |
|---|---|---|---|
| converted files + `package_info.cs` | 952 + 253 | 984 + 251 | MISSED, 30 more files |
| switch lines | 6,000 | 6,048 | MISSED, 48 more |
| `using static` lines | 848 | 843 | MET in attribution |
| files with the self-alias (renamed) | 46 (4) | 47 (4) | MISSED by 1 |
| holder declarations | 1,993 | 1,975 | MISSED, 18 fewer |
| method expressions that name a holder | 81 | 81 | MET |
| g, `builtin.` removed | at most 1,034 | 1,028 | MET |
| h, import alias restored | 115 in 32 files | 79 in 27 | MISSED, low; re-read in section 28 |
| j, cast removed | 3 | 3 | MET |
| references through the alias | 249 | 189 | MISSED, low; re-read in section 28 |
| fallbacks | 11 | 12 | the falsifier FIRED (section 26, C2) |
| `Δsliceᴛ` becomes `Δslice` (section 22) | | | MET |

- The 30 extra files hold only named kinds, mostly a consumer's spelling of a foreign type that lost its `Δ`.
- `using static`: the census read 9 files the corpus does not convert under its tags or owns by hand (5 of
  `testing`) and missed 4 it converts.
- Every changed line was attributed to a kind; 24 lines were read by eye.

### 26. Seven defects of the cut (none is in master), each fixed red-first

The full behavioral runner ran at `c07e0b7671` AFTER that commit was pushed. A seat that moves generated
members owes the runner before its first push; here it came second. It read 943 projects: 21 did not
compile, and no project that compiled printed anything other than Go's output.

| class | where | what was wrong | projects |
|---|---|---|---|
| C1 | converter | inside a holder, a package TYPE named like a direct child namespace of the package's namespace, or like a non-generic golib root type, binds that namespace or type (CS0118) | 1 |
| C2 | converter | the holder state was not cleared when its method ended, so a later package-level declaration was qualified as if it stood in that holder | 0; found by section 25's fallback falsifier in `flag/flag.go` |
| C3 | converter | a defined type over the empty interface is a `global using` in its package; a file that reaches it only through another package's alias spelled it as a member of the package class (CS0426) | 1 |
| G1 | `ImplementGenerator`, value adapter | called a wrapped type's method on the package class | 10 |
| G2 | `ImplementGenerator`, value embed hop | named a foreign embed's package class for a promoted method | 2 |
| G3 | `RecvGenerator` | the twin it writes into a holder could not bind a type another generator declares in the package class (CS0246) | 1 |
| G4 | `BoundedGoroutineInterceptorGenerator` | its name lookup read one class and missed methods of other holders (GO2CS0010) | 1 |

- The fixes: the holder state ends with its method (C2); a type the namespace hides is written through the
  package class inside a holder, decided from the names in view, never from a compile error (C1); the type
  takes its target's spelling where no alias of it is loaded (C3; master covered only the form that
  collided with a method, through the rename section 22 retired); the four generator readers ask
  `GoMethodHosts` for the class that holds the method.
- C1 applies only where the package's own namespace has such a child. A behavioral project is in the root
  namespace `go`, where every top-level package is a child namespace; a package under a dotted module path
  is not. A read of `std`'s Go source (every non-test file, every build tag) finds no package type named
  like a child namespace of its package's namespace and none named like a golib root type, so the fix is
  predicted to change no line of `src/core`.
- Of the other five projects, four failed only because a consumer was converted before its dependency
  (section 30), and `FuncValueMethodClash` waits on the `src/core` regen (section 24).
- After the fixes 20 of the 21 convert, build and print Go's output.

### 27. Line kinds n to u: what the first footprint held that no section named

Counts are linux/amd64 at `c07e0b7671`.

| kind | what changes | count |
|---|---|---|
| n | the closing `} // end <class>` names the holder where a file ends inside one | 413 files |
| o | in `package_info.cs`, a list sorted by name reorders once a name has no `Δ` | 217 lines change position in 57 files |
| p | in `package_info.cs`, a name that would equal a .NET type in view without its `Δ` is written through its package class | 38 sites in 6 files |
| q | where master spelled a foreign renamed type by its exported alias (`abiꓸName`), the package's own Go alias of that type is written (`name`) | 13 sites |
| r | a foreign method expression names the holder from the global namespace (`global::go.net.httpᴛRequest.Write`) | 2 sites |
| s | the rename's exports go: `GoTypeAlias` records and `global using pkgꓸName` lines | 167 and 188 lines |
| t | a `GoBoundedGoroutine` record carries the moved line number of its `go` statement | 1 line |
| u | the comment column of a declaration block moves when a name in it loses its `Δ` | 40 lines in 17 files |

- **Named in sections 17, 18 and 22 without a count, now counted**: the `GoPositionMap` records of files
  whose lines move (798 re-encoded in 234 files, 35 added, 35 removed); the per-type .NET alias lines (28
  added and 20 replacing a using, in 20 files); the `<MethodHolders>` section (1,975 declarations and 1,694
  other lines in 242 files); a lifted type's lines standing ahead of its method's switch line (44 lines in
  13 files); one hoisted literal field per class (569 lines added and 1 removed, in 90 files); a consumer's
  spelling of a foreign type that lost its `Δ` (`timeꓸMonth` becomes `time.Month`: 2,555 lines in 202
  files).
- One line fits no kind and is correct: in `go/types/format.go` a hoisted literal changes its field type,
  because the class that now holds it uses it only as an interface value.
- The comment "Fully qualified to avoid alias shadowing…" in `hash/maphash/maphash_purego.cs` is gone, as
  section 20 expected.

### 28. The two low counts, read again on the same two arms (no reconvert)

- **h, 79 of 115.** The classifier's miss, not the cut's and not the census's. Where master wrote an import
  through its package class by a DOTTED path (`text.template_package.HTMLEscape`), the classifier filed the
  change under the consumer side of kind a. Counted as h, the arm reads 116 sites in 32 files against 115
  in 32. Five files differ by one or two in either direction; one of them is `maphash_purego.cs`, the file
  outside the census's load. Scored against the original prediction: MISSED as posted (79), MET within one
  as re-read.
- **References through the alias, 189 of 249.** 63 of the census's sites stand in methods the corpus
  implements by hand (`reflect` `value.go` 59, `map.go` 4). The converter writes a placeholder for such a
  method, so the site is not emitted. They belong to the hand-owned leg, which section 14 named as the arm
  most likely to miss. Every other file agrees with the census, file for file. Scored against the original
  prediction: MISSED; the census counted Go source and not what the converter emits.
- **C2's reach in `std`**, by a text read of the cut arm: one line, `flag.go`'s `Usage`. The stale state
  could reach any package-level declaration that follows a method; no kept `builtin.` or import
  qualification was found in such a declaration.

### 29. Predictions for the fixed cut

- **linux/amd64 again, on merged master `06610aaa65`**: h 116; references through the alias 189; fallbacks
  11; g 1,028; `using static` 843; switch lines 6,048; holder declarations 1,975; 1,235 files. Kinds n to u
  keep their counts. **Falsifier**: a count that moves by more than the one `flag.go` line, or a line of no
  named kind.
- **windows/amd64, darwin/amd64, linux/arm64**, one run each, in that order: each count is the fixed
  linux/amd64 count plus the difference between that target's census row and linux/amd64's (sections 11,
  18, 20). The census differs only by target-specific files: files with a holder 848 | 838 | 854 | 847,
  methods 10,284 | 10,287 | 10,297 | 10,296, method expressions 81 on each, g sites 1,097 | 1,106 | 1,098 |
  1,097, h and j equal on each. The numbers are written into this record before each run. **Falsifier**: a
  kind outside a to u, or a changed line in a file common to two targets that differs between them.

### 30. Procedure: a golden re-baseline across a rename converts dependencies first

A converted package records the names it exports (`GoTypeAlias`, `global using`) in its own output, and a
consumer reads them from the dependency's emitted files. A consumer converted BEFORE its dependency reads
the dependency's old records and keeps a name that no longer exists (CS0426 on `CrossPkgBox`). Any
re-baseline across a change to exported names therefore converts each dependency before its consumers,
or runs the whole set twice. Depth of directory is not the order: two sibling packages can depend on each
other's output (`RenamedTypeAliasReexport`). Four behavioral projects failed the first runner for this
reason alone and built clean in dependency order.
