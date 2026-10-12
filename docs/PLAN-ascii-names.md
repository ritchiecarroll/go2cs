# PLAN — an opt-in ASCII spelling for go2cs glyph names

> **STATUS: CONSIDERED, NOT SCHEDULED (owner, 2026-10-12).** This page describes a possible direction, not a
> commitment. Nothing here is built, and the default output does not change.

## What the glyphs are for

go2cs marks the names and helpers it adds with Unicode letters such as `ж`, `Ꮡ`, `Δ`, `ᴛ` and `ˢ`. C# accepts them
in identifiers, and they do two jobs:

1. **No clashes.** Ordinary Go code does not use these letters, so a name the converter makes up does not collide
   with a name the Go author wrote.
2. **Go-like reading.** The scaffolding is small and visually out of the way, so the converted C# reads close to the
   Go original.

A few real examples, from [Reading Converted Code: Names and Glyphs](ConversionStrategies.md#reading-converted-code-names-and-glyphs):

| Go | Converted C# | Glyph |
|---|---|---|
| `func bindAdd(a *accum)` | `void bindAdd(ж<accum> Ꮡa)` | `ж<T>` is golib's pointer box; `Ꮡ` marks a name that holds one |
| `acc := &accum{}` | `var acc = Ꮡ(new accum(nil));` | `Ꮡ(…)` is Go's `&` |
| `"bound add:"` | `boundAddˢ` | `ˢ` is a string literal stored once in a static field |
| an inner `hour` shadowing an outer one | `hourΔ1` | `Δ1` is a second declaration of the same name |
| `go fmt.Println("First")` | `goǃ(ᴛ1 => fmt.Println(ᴛ1), firstˢ)` | `goǃ` is the `go` statement; `ᴛ` marks a made-up name |

Go itself accepts most of these letters in identifiers, so the first job is done by convention, not by grammar. The
guard proposed below would cover the glyph spelling as well.

## The idea

An opt-in converter option, for example `-ascii-names`, gives each glyph role an ASCII spelling: `ж<T>` might read
`Ptr<T>`, `Ꮡa` might read `ptr_a`, `boundAddˢ` might read `boundAdd_str`. The default stays glyphs. The option is for
C# developers who maintain converted code by hand and find the glyphs hard to type or to read. Most glyph constants
already have one home, the converter's symbol table, so much of the option is a second set of values for that table.

## The collision problem, and how a converter would guard it

An ASCII spelling such as `ptr_a` or `tmp1` is a name a Go author could easily have written. The converter can check
for that: it already has the full `go/types` view of the package. With the option on, it would check every
generated ASCII name against the package's own identifiers in every scope (package level, functions, fields and
methods), the imported package names, golib's public names, C# keywords, and every other name it generates.

On a collision it can either refuse with a named error, or disambiguate deterministically (append a counter).
**Recommended: refuse.** The error names the Go identifier, the generated name, the glyph role and the source
position, and suggests a different prefix or suffix. Disambiguation is quieter, but it makes a generated name depend
on unrelated code elsewhere in the package, so adding one Go identifier can rename scaffolding in a file nobody
touched. A refusal keeps the output stable and predictable.

## Customizable?

**Recommended: a built-in default mapping, plus an optional user mapping file** that sets a prefix or suffix per glyph
role (for example `temp = "tmp_"`, `hoistedString = "_str"`). The collision guard checks every mapping, default or
custom, on every conversion. Customization is safe only because of that guard: without it, a user's chosen spelling
could silently shadow a Go name and change what the program does. The mapping file also gets its own checks: each
spelling is a valid C# identifier fragment, no two roles share a spelling, and none is empty.

Some names stay non-configurable:

- **golib's public API.** `ж<T>`, `Ꮡ(…)`, `goǃ`, `ᐸꟷ`, `ꟷ`, `ᐧ` and `appendꓸꓸꓸ` are members of the shared runtime
  library. Each needs at most one shipped ASCII alias, chosen once, never a per-user name, because every package
  compiles against the same golib.
- **Names other packages reference.** Some generated names are public and are used across packages, such as the
  field references behind `&b.off` (`Buffer.Ꮡoff`) and the source generators' adapter classes. A package converted
  with one mapping must link against a package converted with another, so these follow the shipped default only.
- **The source generators.** They emit glyph names too, so they would read the chosen spelling from the project
  rather than from their own constants.

## Effect on published packages

The go.* NuGet packages are built with glyphs and would stay that way. An ASCII option affects only code a user
converts, plus any ASCII aliases golib offers. The tension is real: a user's ASCII code still meets glyph names when
it reaches into a published package's generated members, and C# cannot alias an open generic type, so an ASCII
`ж<T>` is either a second type (which breaks pointer identity) or a rename of the real one with the glyph kept as
the alias. That one decision is the hardest part of the plan.

## Rough size

A guess, not a measurement: **about 8 to 15 lane-days** (focused working days). Roughly: routing every generated name
through the symbol table and adding the option and mapping file, 3 to 5; the collision guard and its errors, 2 to 3;
source-generator support, 2 to 3; golib aliases and the `ж<T>` decision, 1 to 4.

A real implementation would need these guards:

- With the option off, the output is byte-identical: no existing golden changes.
- The behavioral suite converted with the option on behaves exactly as with glyphs.
- No non-ASCII identifier remains in option-on output, outside string literals and comments.
- The collision guard fails on a planted Go identifier equal to each mapped name, one test per glyph role.
- A malformed mapping file (invalid fragment, duplicate spelling, empty value) is refused with a named error.

## Open questions

1. Is the option name `-ascii-names`, and is the chosen mapping recorded in the converted project so a reconvert
   always uses the same spelling?
2. Refuse or disambiguate on a collision? This page recommends refuse.
3. Does golib ship ASCII aliases at all, and how is `ж<T>` spelled: an alias type, or a rename of the real type?
4. Should the option be refused with `-stdlib`, so published packages can only ever be built with glyphs?
5. Is the user mapping file worth having in a first cut, or should the first cut ship only the default mapping?
