# C# Coding Style

Hand-authored C# in this repo follows Visual Studio defaults, adapted from
[dotnet/runtime](https://github.com/dotnet/runtime)'s conventions.

**Scope.** These rules govern hand-authored code: `src/core/golib`, `src/gen/go2cs-gen`,
`src/core/testing`, `*_impl.cs` companions and other `[module: GoManualConversion]` whole-file
hand-owns, and the utility/runner projects under `src/tests` and `src/utilities`. They do **not**
govern `src/core/<pkg>` — the converter-generated standard library — which follows the converter's
own emission rules, not hand-authoring style.

For non-code files (XML, JSON, …), match the existing style in that file or component.

1. [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces: each brace begins on its own line. A single-line statement block can skip braces but must still be properly indented and not nested inside a braced block. Exception: a `using` statement may nest directly inside another `using` at the same indentation, even when the nested `using` controls a block.
2. Four spaces of indentation, no tabs.
3. `m_camelCase` for private/internal instance fields, `s_` for static fields, `t_` for thread-static fields; `readonly` after `static` (`static readonly`, not `readonly static`). Public fields are rare and use PascalCase with no prefix.
4. Avoid `this.` unless required.
5. Always specify visibility, even the default (`private string m_foo`, not `string m_foo`); visibility is the first modifier (`public abstract`, not `abstract public`).
6. `using` imports go at the top of the file, outside any `namespace` block, sorted alphabetically — except `System.*`, which sorts first.
7. Never more than one blank line in a row.
8. No spurious spaces (`if (someVar == 0)`, not `if ( someVar == 0 )`).
9. If an existing hand-authored file already differs from these rules in a consistent way (e.g. `_camelCase` private fields instead of `m_camelCase`), match that file's style rather than mixing conventions within it. This is not license to invent a new style in a new file.
10. Always use actual type names; use `var` only when the type is unknown or resolved at run time.
11. Use language keywords, not BCL type names, for both type references and static calls: `int`/`string`/`float`, not `Int32`/`String`/`Single`; `int.Parse`, not `Int32.Parse`.
12. PascalCase for constant locals and fields, except interop code whose constant must match the name/value of what it calls.
13. Prefer `nameof(...)` over a string literal wherever it applies.
14. Fields go at the top of a type declaration.
15. Never hardcode a go2cs symbol glyph (`Δ`, `Ꮡ`, `ж`, `ᴛ`, …) as a literal character or a raw `\uXXXX` escape. Each is defined once in `src/core/go2cs/symbols.json` and projected into `src/go2cs/symbols.go` (Go) and `src/core/go2cs/Symbols.cs` (C#, class `go2cs.Symbols`) by `src/go2cs/internal/gensymbols`; consume them via `using static go2cs.Symbols` (`AddressPrefix`, `PointerPrefix`, `ShadowVarMarker`, `TempVarMarker`, …). `src/check-symbol-sync.ps1` fails if a projection drifts from the table. For any other non-ASCII character, use a `\uXXXX` escape — literal characters occasionally get garbled by a tool or editor.
16. Indent `#region` sections and `goto` labels one level less than the surrounding code.
17. One type per file, named after the type — that is the default and it stays the default. Split a type across `partial` files only when a large, *mechanically shaped* cluster of members (an arity ladder, a generated overload set) is crowding out the members a reader actually comes to the file to find. Name each additional file `<Type>.<Cluster>.cs` (`builtin.GoroutineLaunchers.cs`, `builtin.DeferRegistrations.cs`), and open it with a banner comment stating what the cluster covers, *why* the members take that shape, and the rule for adding to it — so the next contributor can tell at a glance whether their new member belongs there or in the primary file. The second admissible case is a utility type that is not one large thing but several *independent concerns* co-located under one name — `TypeExtensions` (an extension-method registry, Go method sets, scalar conversions) and `GoReflect` (naming, layout, value marshalling, field access) are the worked examples. Split those by concern rather than by shape, name each file for its concern (`GoReflect.FieldAccess.cs`), and hold it to the same banner requirement plus one more: say how the concern relates to the siblings, because a coupling that crosses files — a cache cleared from one file and declared in another — is invisible from either side alone. What is *not* admissible is splitting a type whose size is one cohesive thing; that is a design problem, not a file-layout one. The test between the two: if you cannot write each file's banner without describing the others' internals, the concerns are not independent and the split is hiding the real problem.
18. Never name a local, parameter, pattern variable or range variable `field` inside a property accessor — including inside a local function or lambda nested in one. C# 14 makes `field` a *keyword* in that scope (it names the property's synthesized backing field), so the declaration is CS9273 and every use rebinds away from your variable. Projects pin `LangVersion` (`src/Directory.Build.props`) at a release where `field` is already a keyword, and raising the pin can turn other names into keywords the same way. That is how `go2cs-gen` once stopped building on a newer SDK, back when projects followed `latest` ([issue #34](https://github.com/ritchiecarroll/go2cs/issues/34)); an older SDK will not warn you. Use a qualified name (`fieldSymbol`, `structField`); the `@field` escape compiles but reads as a workaround. Apply the same care to any future contextual keyword. This rule governs hand-authored code only — the escapes that keep *generated* identifiers legal (`file`, `required`, `scoped`, …) live in the converter's keyword set, `src/go2cs/identifierNaming.go`.

`src/.editorconfig` enables C# auto-formatting for the projects under `src/`.
