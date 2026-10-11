# Consuming converted Go from C#

go2cs turns a Go package into an ordinary .NET assembly, so C# can call it like any other library. This page
shows what that surface looks like from the C# side: how Go's strings, slices, multiple results, errors, pointers,
maps, channels and goroutines appear to a C# caller.

The examples use two converted modules, [`github.com/ritchiecarroll/hashset`](https://github.com/ritchiecarroll/hashset)
(a generic set built on a Go map) and [`github.com/google/uuid`](https://github.com/google/uuid). Every snippet
comes from a small console app,
[`src/tests/CSharpConsumer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/tests/CSharpConsumer), which
converts both modules, builds against them, and checks that each example still behaves as shown here.

## Getting a converted package

There are two ways to get one:

- **Reference a published package.** The [nugetgo.net](https://nugetgo.net) registry lists Go modules that have been
  converted and published to NuGet. Add the package as a `PackageReference`, like any other. For example, the
  `hashset` module used on this page is published as `nugetgo.github.com.ritchiecarroll.hashset`:
  `dotnet add package nugetgo.github.com.ritchiecarroll.hashset`.
- **Convert it yourself.** `go2cs -recurse <module_dir> <output_root>` converts a module and its dependencies (see
  [Converting a real-world module](README.md#converting-a-real-world-module)). Each Go package becomes one C#
  project; add the ones you call as `ProjectReference` items.

The C# you write is the same either way.

## Names: packages, classes and namespaces

A Go package becomes a static class named `<package>_package`, in a namespace built from the rest of its import path.
`github.com/google/uuid` is the class `uuid_package` in the namespace `go.github.com.google`. An exported Go function
is a public static method on that class, and a Go method is a C# extension method on the type it belongs to. A Go type
is nested in the package class, so `uuid.UUID` is `uuid_package.UUID`.

These `using` directives cover the examples on this page:

```csharp
using go;                                   // golib: @string, slice<T>, map<K,V>, channel<T>, ж<T>, error
using go.github.com.google;                 // uuid_package (and its extension methods)
using go.github.com.ritchiecarroll;         // hashset_package (and its extension methods)
using static go.builtin;                    // nil, len, goǃ, Ꮡ
using errors = go.errors_package;
```

`go` is the namespace of golib, the runtime library that carries Go's semantics. `go.builtin` holds Go's built-in
functions and `nil`. When the converted code comes from NuGet packages, those two arrive with the packages as global
usings, so you write only the package namespaces; set the MSBuild property `GoConsumerUsings` to `false` to turn
them off. A local conversion that you reference as projects does not bring them, so add them yourself, as here.

A Go name that would clash with another name in the same C# class gets a `Δ` prefix: uuid's `Version` type is
`uuid_package.ΔVersion`, because the package class also holds the `Version` method.
Go's `int` is a native-sized integer, so it appears in C# as `nint`.

## Calling a function, and strings

```csharp
uuid_package.UUID id = uuid_package.New();
@string text = id.String();         // a Go string comes back as @string
string s = text;                    // and converts to System.String implicitly
```

A Go string is `@string`, a UTF-8 string like Go's. It converts to and from `System.String` implicitly, so a C#
string can be passed wherever Go takes a string:

```csharp
@string fromCSharp = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";   // System.String converts the other way
```

## Multiple results and errors

A Go function with several results returns a C# tuple. Go reports failure with an `error` value rather than an
exception, so check it against `nil`:

```csharp
var (parsed, err) = uuid_package.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
var (_, bad) = uuid_package.Parse("not-a-uuid");
// err == nil, and bad != nil with bad.Error() describing the problem
```

C# can make Go errors too, for Go code that takes one:

```csharp
error mine = errors.New("made in C#");
```

## Arrays

A Go array is a fixed-size value. `uuid.UUID` is a `[16]byte`, and it indexes like an array:

```csharp
// parsed[0] == 0x6b, and len(parsed) == 16
Span<byte> view = parsed.ToSpan();
```

## Slices

A Go slice is `slice<T>`. A C# array converts to one implicitly, and the slice **shares** the array, as a Go slice
shares its backing array: a write through either one shows in the other. Converting a slice back to `T[]` makes a
copy. `ToSpan()` gives a `Span<T>` over the slice's elements without copying.

```csharp
int[] numbers = [1, 2, 3];
slice<int> shared = numbers;        // wraps the same array: no copy
shared[0] = 10;                     // numbers[0] is now 10
int[] copy = shared;                // converting back to T[] copies
Span<int> span = shared.ToSpan();   // a view, no copy
```

## Maps and generics

hashset's `HashSet[T]` is a Go map type, and its generic functions and methods keep their type parameters:

```csharp
var set = hashset_package.NewHashSet<int>(new[] { 1, 2, 3 });
set.Add(4);                                 // true: 4 is new
var sum = 0;
foreach (var (key, _) in set) sum += key;   // range over a Go map: (key, value) pairs
slice<int> keys = set.Keys();
```

`len` works on a map, a slice, an array or a string, as in Go.

## Pointers

A Go `*T` is `ж<T>`, a reference to a value on the heap. A method with a pointer receiver takes either a C# `ref`
to a local or a `ж<T>`:

```csharp
var target = new uuid_package.UUID();
error uerr = target.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());

ж<uuid_package.UUID> boxed = Ꮡ(new uuid_package.UUID());
boxed.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
```

`Ꮡ(value)` makes a `ж<T>` holding a copy of the value, like Go's `&value` on a new variable. `boxed.Value` reads it.

## Channels and goroutines

A Go channel is `channel<T>`, with `Send` and `Receive`. `goǃ` starts a goroutine, from C# as from converted Go:

```csharp
var results = new channel<int>(1);
goǃ(() => results.Send(42));        // a Go goroutine, started from C#
// results.Receive() == 42
```

An unbuffered channel (`new channel<T>(0)`) makes the sender wait for a receiver, as in Go.

## Panics

A Go `panic` that is not recovered reaches C# as a `PanicException`:

```csharp
try
{
    uuid_package.MustParse("not-a-uuid");
}
catch (PanicException panic)
{
    // panic.Message describes it
}
```

## Rough edges

- Errors are values, not exceptions: C# code checks `err != nil` after each call that can fail.
- Some names carry glyphs a keyboard does not type easily (`goǃ`, `Ꮡ`, `ж<T>`, a `Δ` prefix). They come from golib and
  the converter's naming rules, and IntelliSense completes them.
- A Go map type such as `HashSet[T]` does not implement .NET collection interfaces like `ISet<T>`: use its Go methods,
  `len`, and `foreach`.

For how each Go construct is converted, and why, see [Conversion Strategies](ConversionStrategies.md).
