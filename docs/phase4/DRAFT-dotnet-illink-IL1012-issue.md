# DRAFT: a dotnet/runtime issue for the ILLink IL1012 crash

> **A draft for the owner to post from his own account.** Nothing here has been posted anywhere.
> Everything below the line "ISSUE TEXT" is the issue, ready to paste. The repro is the three projects on
> `claude/g-illink-repro` at `fda1793f62` (G's), which can be attached as a zip.
>
> **Measured twice.** G's readings are in that ref's `README.txt`. R ran the same command and the five
> control variants again on a second Windows 11 machine with the same SDK, 2026-10-10: the same error text
> and the same five outcomes. The Native AOT line is G's reading alone.
>
> **Repository:** `dotnet/runtime`. **Suggested label:** `area-Tools-ILLink`. **Template:** Bug report.

---

ISSUE TEXT

## Title

ILLink IL1012 (NullReferenceException in DocumentationSignatureParser) for a `[DynamicDependency]` signature naming a method whose parameter is a nested type from another assembly

## Body

### Description

`dotnet publish` with `PublishTrimmed` fails with `IL1012` when a `[DynamicDependency]` uses a
documentation-ID member signature that names a method with a parameter whose type is a **nested** type
declared in **another** assembly. The trimmer throws a `NullReferenceException` while it compares the
signature with the candidate method's parameters.

It needs all three conditions. A nested type from the same assembly as the method works, a non-nested
type from another assembly works, and the same method named without a parameter list works.

We met it in a source generator that emits one `[DynamicDependency]` per method it has to keep. The
repro below is three small projects with no other dependency.

### Reproduction Steps

Three projects, attached as a zip.

`LibA/Outer.cs` (`LibA.csproj`: `net10.0`, nothing else):

```csharp
namespace A;

public static class Outer
{
    public class Nested { }
}

public class Flat { }
```

`LibB/P.cs` (`LibB.csproj`: `net10.0`, references LibA):

```csharp
namespace B;

public static class Local
{
    public class Nested { }
}

public static class P
{
    public static int M(A.Outer.Nested n) => n is null ? 0 : 1;
    public static int F(A.Flat f) => f is null ? 0 : 1;
    public static int L(Local.Nested l) => l is null ? 0 : 1;
}
```

`App/Program.cs` (`App.csproj`: `Exe`, `net10.0`, `PublishTrimmed=true`, `TrimMode=partial`, references LibB):

```csharp
using System.Diagnostics.CodeAnalysis;

static class Program
{
    [DynamicDependency("M(A.Outer.Nested)", typeof(B.P))]
    static int Main() => 0;
}
```

Then, from the folder that holds the three projects:

```
dotnet publish App/App.csproj -c Release -r win-x64
```

### Expected behavior

The publish succeeds and `B.P.M(A.Outer.Nested)` is kept, as it is for the other parameter types below.

### Actual behavior

The publish fails with exit code 1:

```
ILLink : error IL1012: IL Trimmer has encountered an unexpected error. Please report the issue at https://aka.ms/report-illink [App/App.csproj]
  Fatal error in IL Linker
  Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
     at Mono.Linker.DocumentationSignatureGenerator.PartVisitor.VisitTypeReference(TypeReference typeReference, StringBuilder builder, ITryResolveMetadata resolver)
     at Mono.Linker.DocumentationSignatureGenerator.PartVisitor.VisitTypeReference(TypeReference typeReference, StringBuilder builder, ITryResolveMetadata resolver)
     at Mono.Linker.DocumentationSignatureParser.GetSignaturePart(TypeReference type, ITryResolveMetadata resolver)
     at Mono.Linker.DocumentationSignatureParser.AllParametersMatch(Collection`1 methodParameters, List`1 expectedParameters, ITryResolveMetadata resolver)
     at Mono.Linker.DocumentationSignatureParser.GetMatchingMethods(String id, Int32& index, TypeDefinition type, String memberName, Int32 arity, List`1 results, ITryResolveMetadata resolver, Boolean acceptName)
     at Mono.Linker.DocumentationSignatureParser.GetMatchingMembers(...)
     at Mono.Linker.DocumentationSignatureParser.GetMembersByDocumentationSignature(TypeDefinition type, String signature, ITryResolveMetadata resolver, Boolean acceptName)
     at Mono.Linker.Steps.MarkStep.MarkDynamicDependency(DynamicDependency dynamicDependency, IMemberDefinition context, MessageOrigin origin)
     at Mono.Linker.Steps.MarkStep.MarkCustomAttributes(ICustomAttributeProvider provider, DependencyInfo& reason, MessageOrigin origin)
     at Mono.Linker.Steps.MarkStep.ProcessMethod(MethodDefinition method, DependencyInfo& reason)
error NETSDK1144: Optimizing assemblies for size failed.
```

### Regression?

Unknown. Only the versions under Configuration were tried.

### Known Workarounds

Name the method without a parameter list: `[DynamicDependency("M", typeof(B.P))]`. The publish then
succeeds, at the cost of keeping every overload of `M`.

### Configuration

- .NET SDK 10.0.400
- `Microsoft.NET.ILLink.Tasks` 10.0.11 (implicit through `PublishTrimmed`, read from `obj/project.assets.json`)
- Windows 11 x64, RID `win-x64`
- Reproduced on two machines with these versions. No other OS or architecture was tried.

### Other information

Changing only the signature string in `App/Program.cs`, or only `TrimMode` in `App/App.csproj`:

| Signature | TrimMode | Result |
|---|---|---|
| `"M(A.Outer.Nested)"` | `partial` | IL1012 |
| `"M(A.Outer.Nested)"` | `full` | IL1012 |
| `"M"` | `partial` | succeeds |
| `"F(A.Flat)"` (a type from another assembly, not nested) | `partial` | succeeds |
| `"L(B.Local.Nested)"` (a nested type in the method's own assembly) | `partial` | succeeds |

Native AOT's compiler (`PublishAot`) accepts the failing attribute.

A guess from the stack, not checked against the source: the two `VisitTypeReference` frames look like the
nested-type branch visiting the declaring type of the parameter's type reference, and getting null for a
declaring type that lives in another assembly.
