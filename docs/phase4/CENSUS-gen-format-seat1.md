# CENSUS — generated-code format seat 1: the interceptor's namespace body and one statement per line
<!-- {% raw %} — Jekyll/Liquid guard: this page quotes generated C#; the HTML comment hides the tag on GitHub. -->

> **State: MEASURED, 2026-11-05, P2.** Branch `claude/p2-gen-format-1` off master `ce5072e657`, linux, .NET SDK
> 10.0.403, `GoTargetOS=linux`. Seat 1 of [`PLAN-generated-code-format.md`](../PLAN-generated-code-format.md),
> approved by the owner with the plan ("Clear P2 scout work on generated-code formatting IMO", COORD relay,
> 2026-11-05). Class C is out of scope by the owner's ruling ("Fine on attributes on one line.").
>
> **Headline:** the brace defect the owner saw in the tour is fixed. All 8 interceptor files in the scout's sample move
> from 73 brace lines at the wrong depth and 317 lines at the wrong indent to 0. 1,980 lines that held two
> statements now hold one each. `dotnet format` diagnostics over the same 2,578 files fall from 8,844 to 6,344, and
> every one of the 2,500 removed is a class A or B diagnostic. Only whitespace and line breaks moved: 72 assemblies,
> 57,856 methods, `ildiff` `differing=0`.

## 1. What changed

| file | change |
|---|---|
| `src/gen/go2cs-gen/LiteralInterceptorGenerator.cs` | class A: the body of `namespace go2cs.Interceptors { … }` is written one level in, so its two classes, their members and statements sit at 4, 8 and 12 columns |
| `src/gen/go2cs-gen/MethodSet.cs` | class B: a bound arm's binding and its `return` each take their own line, at the case block's indent |
| `src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs` | class B: the field-wise constructor's `if (f.Source is not null)` and `if (f != nil)` put `this.f = f;` on its own indented line |
| `src/tests/GenTests/GeneratedLayout.cs` | the guard's checker, ported from the scout's `genfmt`, plus two arms: two statements on one line, and an embedded statement on its header's line |
| `src/tests/GenTests/GeneratedLayoutTests.cs` | runs every `[Generator]` (found by reflection) over one fixture that reaches the three writers, reads every file they write, and has a positive control per arm |

The new constructor line break is `"\r\n"`. The rest of that constructor body still comes from `AppendLine`, which is
seat 2's class G.

## 2. Red first

At `ce5072e657` with the new tests added and no generator change, `EveryGeneratedFileHoldsTheProjectLayout` failed
with 23 findings, all in the three writers' files and none elsewhere:

- `ltest.….Interceptors.g.cs`: `depth` on the 4 class braces and `indent` on 14 member and statement lines;
- `go.ltest_package.Plain.MethodSet.g.cs`: `two statements` on 3 bound arms;
- `go.ltest_package.Buf.g.cs`: `embedded` on `if (b.Source is not null) this.b = b;` and `if (c != nil) this.c = c;`.

The 11 control rows passed at the same time: 6 planted defects each read by their arm, and 5 admitted shapes read
clean (attribute lists on one line, switch sections, embedded statements and `else if`, a braced block on one line, a
`using` nested in a `using`).

## 3. Gates

| gate | base `ce5072e657` | seat |
|---|---|---|
| GenTests | 376 / 376 | 388 / 388 (the 12 new rows included) |
| `ildiff`, reflect's closure + fmt + 10 behavioral projects, built at both trees | — | `assemblies=72 methods=57856 differing=0 only-in-base=0 only-in-seat=0 missing-in-seat=0` |
| CNR (`check-no-regression.ps1`) | 8 `.csproj` changed | the same 8 `.csproj` changed |

**`ildiff`.** Both sides were built Release with `-p:OutDir` per side and compared with `--roots` naming both trees.
The first reading also paired `go2cs-gen.dll`, the generator itself, which differs because its source changed (7
methods, plus 35 compiler-generated iterator members renamed on each side). With that one assembly set aside the
reading above is clean. The builds compile every generated file of 72 assemblies, so it also proves each one still
compiles.

**CNR.** Both runs list the same 8 linux-only behavioral projects
(MulticastGroupJoin, NativeFieldPortAlias, NativeViewPointerEquality, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam,
UnixAbstractAddrName, WritevIovecSeam). Each `.csproj` gains the project template's call-counting and ReadyToRun
blocks, which its committed copy lacks. The generator writes no `.csproj`, so this is master's own stale state on
linux, not this seat's. Both trees were restored after each run.

**Not run: the full behavioral suite.** `run-behavioral.ps1` refused at its disk preflight (17.9 GB free against its
25 GB floor). After clearing this lane's own caches and a finished worktree the box reads 23 GB, still under the
floor, so the suite was not forced with `-IgnoreDiskPreflight`. The `ildiff` builds compiled the 10 behavioral
projects of the scout's sample, and their IL is identical to master's.

## 4. The scout instrument, re-read

The scout's `collect.sh` corpus (2,578 files from the same 64 projects) was rebuilt at both trees with
`EmitCompilerGeneratedFiles=true` and read by the same three arms. The base reading equals the scout's at
`33589333b2` file for file (184,515 lines, 8,844 diagnostics; `src/gen` did not change between the two masters).

| reading | base | seat |
|---|---|---|
| lines | 184,515 | 186,495 (+1,980) |
| `dotnet format` files / diagnostics | 1,376 / 8,844 | 1,162 / 6,344 |
| LiteralInterceptor diagnostics | 520 | 0 |
| arm (c) `depth` / `indent` (lines/files) | 73/8 / 317/8 | 0 / 0 |
| two statements on one line (MethodSet bound arms) | 1,741 | 0 |
| an `if` and its statement on one line (struct constructors) | 239 | 0 |

The +1,980 lines are exactly 1,741 + 239: every split line, and nothing else. The scout's plan counted 1,739 MethodSet
lines in 205 files; the right count is 1,741 in 206, because one method set's hint carries a uniqueness suffix
(`go.os_package.root.MethodSet.1.g.cs`), which the scout's template split filed under StructType. Its 2 diagnostics
were inside StructType's 3,995 "whitespace" figure, which is 3,993.

What remains is seat 2's (classes D to G) and the ruled-out class C: 1,117 attribute-list diagnostics, 71 missing final
newlines in interface files, 221 in PartialStub files, and the whitespace-only lines.

## 5. Samples (real seat output)

`MapCloneLinkname/…/main.….Interceptors.g.cs` (before: §3 A of the plan):

```csharp
namespace go2cs.Interceptors
{
    file static class ˢLiterals
    {
        internal static readonly object ˢ0 = (global::go.@string)"orig len:"u8;
        …
    }

    file static class ˢInterceptors
    {
        [global::System.Runtime.CompilerServices.InterceptsLocation(1, "…")]
        public static (nint n, global::go.error err) ˢI0(params global::System.Span<object> aʗp)
        {
            aʗp[0] = ˢLiterals.ˢ0;
            aʗp[2] = ˢLiterals.ˢ1;
            return global::go.fmt_package.Println(aʗp);
        }
```

`core/internal/abi/…/go.internal.abi_package.ChanType.MethodSet.g.cs`:

```csharp
                    {
                        global::go.ж<…ChanType> ᴛp = (global::go.ж<…ChanType>)receiver!;
                        return (global::System.Func<…ΔKind>)(() => …Kind(ᴛp));
                    }
```

`core/syscall/…/go.syscall_package.RawSockaddrLinklayer.g.cs`, a field-wise constructor:

```csharp
            if (Addr.Source is not null)
                this.Addr = Addr;
```

— P2 (measured 2026-11-05)

<!-- {% endraw %} -->
