# CENSUS — generated-code format seat 2: one door for every generated file, and CRLF on every host
<!-- {% raw %} — Jekyll/Liquid guard: this page quotes generated C#; the HTML comment hides the tag on GitHub. -->

> **State: MEASURED, 2026-11-05, P2.** Branch `claude/p2-gen-format-2`, stacked on seat 1's
> `claude/p2-gen-format-1` at `f04b9034ec` (itself off master `ce5072e657`), linux, .NET SDK 10.0.403,
> `GoTargetOS=linux`. Seat 2 of [`PLAN-generated-code-format.md`](../PLAN-generated-code-format.md), classes D to G,
> approved with the plan (COORD relay, 2026-11-05). The base of every A/B below is seat 1's tip, so each reading
> isolates this seat.
>
> **Headline:** every one of the 2,578 generated files in the scout's corpus now reads clean on trailing whitespace,
> mixed line endings and blank runs (base 4,444 / 12,965 / 825 findings), and every file ends with one line break.
> `dotnet format` diagnostics fall from 6,344 in 1,162 files to 1,117 in 849, and those 1,117 are exactly the class C
> attribute lists the owner ruled may stay. Outside blank lines and trailing whitespace no file changed. 72
> assemblies, 57,856 methods, `ildiff` `differing=0`.

## 1. What changed

| file | change |
|---|---|
| `src/gen/go2cs-gen/GeneratedSource.cs` (new) | `AddGeneratedSource`, overloads for `GeneratorExecutionContext` and `SourceProductionContext`, and `Normalize`: CRLF only, no trailing space or tab, blank runs collapsed to one, no leading or trailing blank line, exactly one final line break; a text holding `@"`, `@$"` or `"""` is added unchanged |
| 15 generator files | the 28 `context.AddSource(` / `productionContext.AddSource(` calls become `AddGeneratedSource(`, each on its own line |
| `StructTypeTemplate.cs`, `InterfaceTypeTemplate.cs` | class G: the 22 `AppendLine` calls become `Append` with `"\r\n"` folded into the literal, or `.Append(x).Append("\r\n")` for a computed argument |
| `src/tests/GenTests/GeneratedWhitespace.cs` (new) | the guard's whitespace checker: LF without CR, a line ending in a space or tab, two blank lines in a row, a blank first line, a missing final line break, a trailing blank line |
| `docs/ConversionStrategies.md:541`, `docs/ConversionStrategies-Reference/struct-embedding.md:50` | three provenance notes on `StructTypeTemplate.cs` re-cited: two fingerprints taken again from the same lines, and one quote of an `AppendLine` call updated to its `Append` form |
| `src/tests/GenTests/GeneratedLayoutTests.cs` | the whitespace sweep over every file every `[Generator]` writes, a source scan for writes around the door, and controls for both |

Two departures from the plan, both small. The helper lives in its own file, not `Common.cs`, because
LiteralInterceptor is an incremental generator and needs the `SourceProductionContext` overload beside the other.
The plan's "fixed point of the helper" check is the whitespace checker itself: its six arms are the helper's six
properties, so a file clean on them is unchanged by the helper.

**The door scan.** `EveryGeneratorWritesThroughTheOneDoorWithItsOwnLineBreaks` reads every `.cs` under
`src/gen/go2cs-gen` (found from the test's own path, `obj/` and `bin/` excluded, `GeneratedSource.cs` exempt) and
names any non-comment line that calls `.AddSource(`, `AppendLine(` or `Environment.NewLine`. It covers generators
the fixture does not reach, such as PartialStub and ImplicitConv, so a sixteenth generator, or a seventeenth
`AppendLine`, fails the build of GenTests' run rather than the corpus.

No existing GenTests pin changed: the 388 rows from seat 1 pass unedited.

## 2. Red first

At `f04b9034ec` with the new tests added and no generator change (log kept outside the repo), 2 of 29 rows failed:

- `EveryGeneratedFileHoldsTheProjectWhitespace`: 38 findings in the fixture's two struct files,
  `go.ltest_package.Plain.g.cs` and `go.ltest_package.Buf.g.cs` (26 `lf` in the constructors block, 10 `trailing` on
  the whitespace-only lines between the equality members, 2 `blank run`);
- `EveryGeneratorWritesThroughTheOneDoorWithItsOwnLineBreaks`: 50 writes around the door, the 28 `AddSource` calls
  and the 22 `AppendLine` calls, each named `file:line`.

The 15 control rows passed at the same time: 8 `Normalize` rows (including a verbatim and a raw literal left alone)
and 7 planted defects each read by its arm.

## 3. Gates

| gate | base `f04b9034ec` | seat |
|---|---|---|
| GenTests | 388 / 388 | 405 / 405 (the 17 new rows included) |
| `ildiff`, reflect's closure + 8 behavioral projects, built at both trees | — | `assemblies=72 methods=57856 differing=0 only-in-base=0 only-in-seat=0 missing-in-seat=0` |
| CNR (`check-no-regression.ps1`) | 8 `.csproj` changed | the same 8 `.csproj` changed, byte-identical diff |

**`ildiff`.** Both sides were built Release with `-p:OutDir` per side and compared with `--roots` naming both trees,
with `go2cs-gen.dll` itself set aside (its source is what changed). The builds compile every generated file of the
72 assemblies, so each still compiles. The plan's `[CallerLineNumber]` caveat is settled by the same reading: no
method's IL moved, though 829 blank lines went.

**CNR.** The seat run changes the same 8 linux-only behavioral `.csproj` files as seat 1 and master, and its
diff hashes the same as seat 1's base run (`sha256 3bd803e295…`): the project template's blocks that their committed
copies lack. The generator writes no `.csproj`. The tree was restored after the run.

**Not run: the full behavioral suite**, for the reason seat 1 gave: `run-behavioral.ps1`'s 25 GB disk floor, on a box
reading 23 GB. Generated files are not checked in and the converter is untouched, so the `ildiff` builds above are
the stronger reading for this seat.

## 4. The scout instrument, re-read

The scout's corpus (2,578 files from the same 64 projects, built as `collect.sh` builds them) was rebuilt at both
trees and read by the same three arms. The base reading equals seat 1's §4 seat column.

| reading | base (seat 1) | seat 2 |
|---|---|---|
| lines | 186,495 | 185,958 (−537) |
| `dotnet format` files / diagnostics | 1,162 / 6,344 | 849 / 1,117 |
| trailing whitespace (lines/files) | 4,444 / 862 | 0 / 0 |
| LF in CRLF text (lines/files) | 12,965 / 698 | 0 / 0 |
| blank runs (lines/files) | 825 / 633 | 0 / 0 |
| files with no final line break | 292 (221 PartialStub, 71 interface) | 0 |
| arm (c) `kr` / `misalign` / `depth` / `body` | 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 |

`dotnet format whitespace --verify-no-changes` still exits 2, on the 1,117 class C diagnostics alone: 849 TypeGenerator
files and nothing from any other generator. The plan's seat 2 gate asked for exit 0; the owner's ruling on class C
came after the plan and keeps those lines as they are.

**What the −537 lines are.** Compared file by file (interceptor hints matched without their path hash), every file's
non-blank lines are identical once trailing whitespace is trimmed, in all 2,578 files. The bytes changed in 1,154
files: 907 TypeGenerator, 221 PartialStub, 21 ImplicitConv, 4 Implement and 1 Recv. 829 blank lines went (821 from
TypeGenerator's blank runs, 8 from 4 PartialStub files with no usings), and 292 files gained a final line break (221
PartialStub, 71 TypeGenerator interface files). The instrument's line count is Roslyn's `SourceText.Lines`, which
counts the empty line after a final break, so it reads those 292 as +1 each: −829 + 292 = −537. The other files changed
only in trailing whitespace or line endings. No seat file holds an LF without its CR.

The scout's arm (b), Roslyn `NormalizeWhitespace`, is documented noise and not a gate; it reads 1,416 files against
the base's 1,437, the 21 ImplicitConv files whose `}····` was its only difference.

## 5. Sample (real seat output)

`core/runtime/…/go.runtime_package.dlogPerM.g.cs`, before (`·` marks a trailing space) and after:

```csharp
        public bool Equals(dlogPerM other) =>
            true /* empty */;
········
        public override bool Equals(object? obj) => obj is dlogPerM other && Equals(other);
```

```csharp
        public bool Equals(dlogPerM other) =>
            true /* empty */;

        public override bool Equals(object? obj) => obj is dlogPerM other && Equals(other);
```

The same file's constructors block, which `AppendLine` wrote with LF on Linux, now ends each line in CRLF like the
rest of the file, so the same input writes the same bytes on Windows and Linux.

— P2 (measured 2026-11-05)

<!-- {% endraw %} -->
