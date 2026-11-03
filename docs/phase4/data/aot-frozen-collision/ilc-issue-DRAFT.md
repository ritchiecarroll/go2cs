# DRAFT ONLY (not posted; the owner decides whether anything is filed)

## NativeAOT: preinitialized (frozen) object symbol names collide when a type name ends in digits

### Description

`dotnet publish -p:PublishAot=true` fails inside ILC with
`System.ArgumentException: An item with the same key has already been added. Key: __FrozenObj_<type><n>`
when two types in one namespace are named `X` and `X<digits>` and both have static constructors that ILC
preinitializes with allocations.

`SerializedFrozenObjectNode.AppendMangledName` builds the symbol as the compilation-unit prefix, `__FrozenObj_`,
the mangled owning type name and then the allocation site id, with no separator:

```csharp
sb.Append(nameMangler.CompilationUnitPrefix).Append("__FrozenObj_"u8)
    .Append(nameMangler.GetMangledTypeName(_owningType))
    .Append(_allocationSiteId.ToStringInvariant());
```

The allocation site id is the interpreted instruction counter in `TypePreinit`, which is unique only within one owning
type. So type `A`'s allocation at instruction 23 and type `A2`'s allocation at instruction 3 produce the same name,
`__FrozenObj_<prefix>_A23`.

### Reproduction

```csharp
using System;

static class A
{
    static int M() => 1;
    internal static readonly Func<int> F1 = M;
    internal static readonly Func<int> F2 = M;
    internal static readonly Func<int> F3 = M;
    internal static readonly Func<int> F4 = M;
    internal static readonly Func<int> F5 = M;
    internal static readonly Func<int> F6 = M;   // newobj at instruction 23
}

static class A2
{
    static int M() => 2;
    internal static readonly Func<int> F1 = M;   // newobj at instruction 3
}

static class Program
{
    static int Main() => A.F1() + A.F2() + A.F3() + A.F4() + A.F5() + A.F6() + A2.F1();
}
```

Project: `net10.0`, `<PublishAot>true</PublishAot>`. Command: `dotnet publish -c Release -r win-x64`.

**Actual:** `EXEC : error : An item with the same key has already been added. Key: __FrozenObj_FrozenRepro_A23`.

**Expected:** the program publishes and returns 8, as it does when `A2` is renamed `AB`.

### Configuration

.NET SDK 10.0.400, Microsoft.DotNet.ILCompiler 10.0.11, win-x64 host and target. The same failure occurs on
osx-arm64 and osx-x64 hosted runners in a larger application. The code is target-independent.

### Suggested fix

Put a separator that cannot occur in a mangled type name between the type and the id (for example `__FrozenObj_` +
type + `_` + id, or a `@`), or key the node on the (type, id) pair rather than on the string.
