# CLR type-load crash: a struct enumerating `KeyValuePair<K, Self>` that holds a generic struct doing the same

> **State: OPEN — for the owner to file upstream with dotnet/runtime (2026-10-05).** Everything
> between the two horizontal rules below is written to be pasted into an issue as it stands. It uses
> BCL types only, no go2cs code. go2cs works around the fault in go2cs-gen
> (`InheritedTypeTemplate.HoldsMapInHolder`: a self-containing named map holds its map in a
> `StrongBox`), guarded by `src/tests/GenTests/SelfContainingMapHolderTests.cs` and the behavioral
> fixture `src/tests/Behavioral/SelfContainingMapHolder`. Once the runtime is fixed, removing the
> holder is a separate decision. Measured by lane C2 on linux-x64; amended with dated blocks, never
> rewritten.

---

### Description

Loading a struct type crashes the process with SIGSEGV inside `libcoreclr.so` when all of these hold:

- the struct `Outer` implements `IEnumerable<KeyValuePair<long, Outer>>`, and
- it holds, **by value**, a field of a generic struct `Inner<Outer>`, and
- `Inner<V>` itself implements `IEnumerable<KeyValuePair<long, V>>`.

`Inner<V>` holds only a reference (a `List<>`), so there is no layout cycle. A struct with the same
field that does **not** implement the interface loads fine. No managed code of either type needs to
run: `typeof(Outer)` is enough. The process exits with SIGSEGV; no managed exception is raised.

### Reproduction

`clrrepro.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

`Program.cs`:

```csharp
using System.Collections;
using System.Runtime.CompilerServices;

// `dotnet run -c Release -- crash` exits 139 (SIGSEGV) on .NET 10.0.12 linux-x64 before printing "loaded".
// `dotnet run -c Release -- control` prints "loaded Control" and exits 0.
if (args[0] == "crash") Load.Crash(); else Load.Control();

static class Load
{
    [MethodImpl(MethodImplOptions.NoInlining)] public static void Crash() => Console.WriteLine("loaded " + typeof(Outer).Name);
    [MethodImpl(MethodImplOptions.NoInlining)] public static void Control() => Console.WriteLine("loaded " + typeof(Control).Name);
}

// A generic struct that enumerates KeyValuePair<long, V> (it holds only a reference).
public readonly struct Inner<V> : IEnumerable<KeyValuePair<long, V>>
{
    private readonly List<KeyValuePair<long, V>> items;
    public IEnumerator<KeyValuePair<long, V>> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Crashes: a struct that enumerates KeyValuePair<long, Outer> and holds Inner<Outer> BY VALUE.
public struct Outer : IEnumerable<KeyValuePair<long, Outer>>
{
    private readonly Inner<Outer> inner;
    public IEnumerator<KeyValuePair<long, Outer>> GetEnumerator() => inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Loads: the same field, without the interface.
public struct Control
{
    private readonly Inner<Control> inner;
}
```

Commands and results:

```
$ dotnet build -c Release -o out
$ dotnet out/clrrepro.dll crash;   echo "exit $?"
exit 139
$ dotnet out/clrrepro.dll control; echo "exit $?"
loaded Control
exit 0
```

### Expected behavior

`loaded Outer`, exit 0 — or, if the shape were unsupported, a `TypeLoadException`. Never a native crash.

### Actual behavior

SIGSEGV in `libcoreclr.so` while the JIT resolves `typeof(Outer)` for `Load.Crash`. Without symbols,
as offsets into `libcoreclr.so` (frame #9 is re-entered at #19, after the ten frames #9–#18):

```
#0      +0x389691                                               <- SIGSEGV
#1-#8   +0x3daff4 +0x3d9216 +0x3d929f +0x4bf0bf +0x4b68ab +0x4c982a +0x3604ca +0x3617dd
#9      +0x35d7d1                                               <- cycle start
#10-#18 +0x35d568 +0x35ebad +0x40982a +0x4096bc +0x35eea1 +0x4c5a09 +0x354dca +0x354ff8 +0x36182b
#19     +0x35d7d1                                               <- same frame as #9
#20-#22 +0x35e4f1 +0x35f2a2 +0x3a65dd, then libclrjit.so (JIT of Load.Crash)
```

### Variations measured (one type per non-inlined method, one process per variant)

- **Key type is not the trigger:** `nint`, `long`, `string`, `object`, and a struct key (with or
  without `IEquatable<>` / `IEnumerable<>`) all crash.
- **Load order does not help:** touching `Inner<Outer>`, `KeyValuePair<long, Outer>` or
  `IEnumerable<KeyValuePair<long, Outer>>` first still crashes, and the crash happens while loading
  that first type.
- **Debug and Release** both crash.
- **Holding `Inner<Outer>` through a reference avoids it:** a class holder
  (`sealed class Holder<T> { T Value; }` with a `Holder<Inner<Outer>>` field) or an `object` field
  loads cleanly with the interface kept.
- **Removing the interface from `Outer`** (the `Control` type) avoids it.

### Which shapes crash, which throw, which load

The same fault, reached through the wrappers a Go-to-C# transpiler generates (see *Other
information*). Each wrapper `M` is a struct implementing `IDictionary<long, V>` that holds, by value,
a dictionary struct `Map<long, V>` (itself implementing `IDictionary<long, V>` over a `Dictionary`
reference). Only the value type `V` varies. `Array<T>`, `Slice<T>`, `Map<K, V>` and `Channel<T>` are
structs; `Ptr<T>` is a class. One variant per process; the Go program behind each runs correctly.

| `V` (the map's value type)                         | Result on .NET 10.0.12      |
|:---------------------------------------------------|:----------------------------|
| `M` itself                                         | SIGSEGV (exit 139)          |
| `Array<M>`                                         | SIGSEGV (exit 139)          |
| `Slice<M>`                                         | SIGSEGV (exit 139)          |
| `Map<long, M>`                                     | SIGSEGV (exit 139)          |
| `struct S { M m; }`                                | `TypeLoadException` ("Could not load type '…M'") |
| `struct A { B b; }`, `struct B { M m; }`            | `TypeLoadException`         |
| `Array<S>` with `struct S { M m; }`                | `TypeLoadException`         |
| `Ptr<M>` (a class)                                 | loads                       |
| `Channel<M>` (a struct holding a reference)        | loads                       |
| `Func<M>`                                          | loads                       |
| an interface whose method returns `M`              | loads                       |

So: a path to `M` that runs only through value types (struct fields, or the type arguments of the
`Array`/`Slice`/`Map` structs) fails, and a path through a class, a delegate or an interface loads.
`Channel<M>` is the exception: it is a struct, yet its wrapper loads. Holding `Map<long, V>` through
a class field (a `StrongBox`) made every failing row we re-ran load: the first six (the `Array<S>` row
was not re-run with the holder).

### Configuration

- .NET runtime 10.0.12 (`Microsoft.NETCore.App`, `@(#)Version 10.0.1226.42308 @Commit: 95017c711e6afc1085133d440e42b4bd78155701`), SDK 10.0.112
- Ubuntu 24.04 LTS, x86_64
- Not checked on other runtime versions, operating systems or architectures.

### Other information

Found through a Go-to-C# transpiler: Go's `type M map[int]M` (a map whose values are the map
itself) is emitted as a struct wrapping a generic dictionary struct and implementing
`IDictionary<K, M>`, which reduces to the shape above.

---

## Provenance

<!-- 2026-10-05, lane C2, linux-x64, runtime 10.0.12 (libcoreclr sha256 prefix df5cfe7ba5abf793), SDK 10.0.112.
     Route: go-cmp class O. The converted cmp test host died with SIGSEGV before its first test; a JIT
     trace placed the fault in golib's extension-method scan reflecting the cmp test assembly, and the
     type probe crashed loading cmp_test_package+cycleTests_M (go-cmp compare_test.go:2394,
     `type M map[int]M`). The go2cs wrapper alone reproduced it; dropping IMap<K,V> fixed it; the BCL
     reduction above crashed with control exit 0.
     Containment matrix through go2cs at 446d2c8ba0 (Go exits 0 on every variant): SIGSEGV for direct,
     [2]M, []M and map[int]M values; TypeLoadException for struct{m M}, two struct levels and
     [1]struct{m M}; loads for *M, chan M, func() M and an interface value. Key matrix through go2cs:
     integer, bool, float, rune, pointer and `any` keys crash; string, named string and [2]int keys
     load, though the generated wrappers are byte-identical apart from names; in the BCL reduction every
     key crashes, so that discriminator is unrooted.
     Census on that predicate: std 1 site on each of linux/windows/darwin (encoding/gob encoder_test.go:378
     recursiveMap, string key; banked green), go-cmp 1 (int key; the crash), behavioral 1 of 696 dirs
     (LocalNamedTypeDecls, string key; green). -->
