# Consuming converted Go from C#

go2cs turns a Go package into an ordinary .NET assembly, so C# calls it like any other library. Go's standard
library is converted and published on NuGet, and so are Go modules listed on [nugetgo.net](https://nugetgo.net).
This page shows what calling them looks like: imports, numbers, strings, errors, slices, maps, pointers, goroutines
and channels. Most of it reads the way the Go code does.

Every snippet comes from a small console app,
[`src/tests/CSharpConsumer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/tests/CSharpConsumer). Its
runner converts two Go modules, [`github.com/google/uuid`](https://github.com/google/uuid) and
[`github.com/ritchiecarroll/hashset`](https://github.com/ritchiecarroll/hashset), builds the app against them and
the converted standard library, and checks that each example behaves as shown here.

## A five-minute start

Each Go standard-library package is the NuGet package `go.` plus its import path, with dots for slashes: `strings`
is `go.strings`, and `unicode/utf8` is `go.unicode.utf8`. Each one brings `go.lib`, the runtime library that
carries Go's semantics. In a .NET 10 console project:

```shell
dotnet add package go.strings
dotnet add package go.fmt
```

Then, in `Program.cs`:

```csharp
using strings = go.strings_package;
using fmt = go.fmt_package;

@string shout = strings.ToUpper("hello, gopher");
fmt.Println(shout, len(shout));     // HELLO, GOPHER 13
```

`dotnet run` prints `HELLO, GOPHER 13`. That is the whole pattern: one `using` alias per Go package, and the calls
read like Go. `@string` is Go's string type ([Strings](#strings)), and the packages bring `@string` and Go's
built-ins such as `len` into scope for you ([Imports](#imports)). To quiet the compiler's warning about lowercase
names, add `<NoWarn>$(NoWarn);CS8981</NoWarn>` to the project.

A published Go module works the same way. `github.com/google/uuid` is `nugetgo.github.com.google.uuid`:

```shell
dotnet add package nugetgo.github.com.google.uuid
```

```csharp
using go.github.com.google;                 // the methods of github.com/google/uuid
using uuid = go.github.com.google.uuid_package;

uuid.UUID id = uuid.New();
@string text = id.String();
```

## Getting a converted package

There are two ways to get one:

- **Reference a published package.** The Go standard library is published as the `go.*` packages, and the
  [nugetgo.net](https://nugetgo.net) registry lists Go modules converted and published to NuGet, such as
  `nugetgo.github.com.ritchiecarroll.hashset`. Add them as a `PackageReference`, like any other package.
- **Convert it yourself.** `go2cs -recurse=nuget <module_dir> <output_root>` converts a module and its dependencies
  (see [Converting a real-world module](README.md#converting-a-real-world-module)). Each Go package becomes one C#
  project; add the ones you call as `ProjectReference` items.

The C# you write is the same either way. The `go.*` packages are built and versioned together, so use one version
of them in a project.

If you convert and publish a module of your own, its nuget.org page is covered in
[Publishing your conversion: the package page](ConvertingAModule.md#publishing-your-conversion-the-package-page).

## Imports

A Go package becomes a static class named `<name>_package`, in a namespace made of `go` and the rest of its import
path: `strings` is `go.strings_package`, `unicode/utf8` is `go.unicode.utf8_package`, and `github.com/google/uuid`
is `go.github.com.google.uuid_package`. An exported Go function is a static method on that class, and a Go type is
nested in it, so `strings.Builder` is `strings_package.Builder`. A Go method is a C# extension method.

**The recommended form is one alias per package**, so call sites read like Go. These are the imports behind every
example on this page:

```csharp
using go;                                   // golib, and the methods of every Go package whose import path has no slash
using static go.builtin;                    // Go's built-ins: nil, len, append, delete, @new, goǃ, Ꮡ
using go.github.com.google;                 // the methods of github.com/google/uuid
using go.github.com.ritchiecarroll;         // the methods of github.com/ritchiecarroll/hashset

using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using uuid = go.github.com.google.uuid_package;
using hashset = go.github.com.ritchiecarroll.hashset_package;
```

`go` is the namespace of golib, the runtime library: `@string`, `slice<T>`, `map<K,V>`, `channel<T>`, `ж<T>` and
`error` live there. `go.builtin` holds Go's built-in functions and `nil`.

**Go methods come through the namespace, not the alias.** An alias names the package class, which reaches the
package's functions and types. Go methods are C# extension methods, and C# finds those only through a namespace
import (or `using static` on the class). Every standard-library package whose import path has no slash (`strings`,
`fmt`, `time`, `bytes`, `sync`, `strconv`, `errors` and the rest) lives in the namespace `go`, so `using go;` brings
all of their methods. A package whose path has a slash needs its own namespace as well: `encoding/hex` needs
`using go.encoding;`, `io/fs` needs `using go.io;`, and `github.com/google/uuid` needs `using go.github.com.google;`.
Without it, a method call such as `id.String()` does not compile, and the error names some other package's
`String` method, which can mislead.

```csharp
time.Duration timeout = 90 * time.ΔSecond;
@string shown = timeout.String();   // a Go method: found through `using go;`
```

**Packages from NuGet bring the usings for you.** Every `go.*` and `nugetgo.*` package adds `using go;` and
`using static go.builtin;` to your project as global usings, and each package you reference directly also adds its
own namespace, such as `go.encoding` for `go.encoding.hex`, so its Go methods are in scope. Set the MSBuild property
`GoConsumerUsings` to `false` to turn them off. Writing the same usings in a file as well does no harm. Set
`GoGlobalPackageAliases` to `true` to get each directly referenced package's alias as well, named for the Go
package (`hex` for `go.encoding.hex_package`); it is off by default, because a file that also declares that alias
fails with CS1537. A conversion you reference as projects adds none of these, so write them yourself, as
`Program.cs` does.

**Lowercase aliases raise warning CS8981**, which notes that an all-lowercase name may become a C# keyword one day.
The aliases are deliberate, so turn the warning off in the project:

```xml
<NoWarn>$(NoWarn);CS8981</NoWarn>
```

**Two other forms work too.** `using static` on a package class gives bare names, and brings the package's methods
as well. Several `using static` lines can collide, though: `Contains` is in `strings`, `bytes` and `slices`, and
`Reader` is a type in both `strings` and `bytes`. It suits a file that leans on one package. The package class
itself, through `using go;`, always works and reads a little noisier:

```csharp
@string bare = ToUpper("gopher");                       // using static go.strings_package;
@string qualified = strings_package.ToUpper("gopher");  // using go;
```

### How a Go declaration appears in C#

| Go | C# |
|---|---|
| `func Atoi(s string) (int, error)` | `public static (nint, error) Atoi(@string s)` |
| `func (d Duration) String() string` | `public static @string String(this Duration d)` |
| `func (b *Builder) WriteString(s string) (int, error)` | `public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s)` |
| `func (uuid *UUID) UnmarshalText(data []byte) error` | `public static error UnmarshalText(this ref UUID uuid, slice<byte> data)` |
| `type Duration int64` | `partial struct Duration` |
| `const Second Duration = 1000 * Millisecond` | `public static Duration ΔSecond` |
| `var ErrSyntax = errors.New("invalid syntax")` | `public static error ErrSyntax` |
| `type Writer interface { Write(p []byte) (n int, err error) }` | `partial interface Writer { (nint n, error err) Write(slice<byte> p); }` |

Each sits in its package class, so the C# caller writes `strconv.Atoi(..)`, `time.ΔSecond` and `io.Writer`. A
value method extends `T`; a pointer method extends `ж<T>` or `ref T` ([Pointers](#pointers)).

A name that would clash with another in the same class gets a `Δ` prefix. `time.Second` is `time.ΔSecond`,
because `time_package` also holds `Time`'s `Second()` method; `time.Millisecond`, with no such clash, keeps its
name.

A Go interface is a C# interface, so a C# class can implement one and hand it to Go:

```csharp
sealed class CollectingWriter : io.Writer
{
    public readonly StringBuilder Text = new();

    public (nint n, error err) Write(slice<byte> p)
    {
        string chunk = (@string)p;      // Go: string(p)
        Text.Append(chunk);
        return (len(p), null);
    }
}
```

```csharp
var log = new CollectingWriter();
fmt.Fprintf(log, "%d gophers\n", 3);    // Go calls the C# Write
// log.Text.ToString() == "3 gophers\n"
```

## Numbers

Go's `int` is a native-sized integer, so it is C# `nint`, and C# `int` is Go's `int32`. The rest match by size:

| Go | C# |
|---|---|
| `int`, `uint` | `nint`, `nuint` |
| `int8`, `int16`, `int32`, `int64` | `sbyte`, `short`, `int`, `long` |
| `uint8` (`byte`), `uint16`, `uint32`, `uint64` | `byte`, `ushort`, `uint`, `ulong` |
| `rune` | `int` |
| `float32`, `float64` | `float`, `double` |

So `len` returns `nint`, and so does `strconv.Atoi`. Hold the result in `var` or `nint`, or cast it where you need
an `int`. C# integer literals `append` to a `slice<nint>` as they are:

```csharp
var size = len("héllo");            // nint, Go's int: 6 bytes
int small = (int)len("héllo");      // C# int is Go's int32, so cast when you need one
var (num, aerr) = strconv.Atoi("42");   // (nint, error)
slice<nint> nums = new nint[] { 1, 2, 3 };
nums = append(nums, 4, 5);              // C# int literals append to a slice<nint>
```

## Strings

A Go string is `@string`: a UTF-8 string, like Go's. (`string` is a C# keyword, and `@` lets the name be used
anyway.) It converts to and from `System.String` implicitly, transcoding between UTF-16 and UTF-8, so a C# string
goes wherever Go takes a string, and a `"..."u8` literal goes in as its UTF-8 bytes, with no transcoding. An
`@string` compares with a C# string literal directly.

```csharp
@string hello = "héllo, 世界";      // System.String converts to @string (UTF-16 to UTF-8)
string back = hello;                // and back (UTF-8 to UTF-16)
// hello == "héllo, 世界", and strings.HasSuffix(hello, "世界"u8) is true
```

`len` counts UTF-8 bytes, as in Go, even on a C# string: `len("héllo")` is 6. To count characters, count runes:
`len(hello)` is 14 bytes, and `utf8.RuneCountInString(hello)` counts its 9 runes.

## Printing Go values

Print a Go value through `fmt`, or call its `String()` or `Error()` method, and you get the text Go prints:

```csharp
error diskFull = errors.New("disk full");
time.Duration wait = 1500 * time.Millisecond;
fmt.Println(diskFull, wait);        // disk full 1.5s
Console.WriteLine($"{diskFull.Error()} after {wait.String()}");    // disk full after 1.5s
```

`Console.WriteLine(x)`, `$"{x}"` and a debugger call .NET's `ToString()`, and it gives the same text. A converted
value's `ToString()` returns its `Error()`, else its `String()`, chosen the way `fmt`'s `%v` chooses: for a pointer,
`Error()` wins over `String()` whichever of them has the pointer receiver. A value with neither method prints its
fields or its underlying value, and a pointer with neither prints its address.

```csharp
Console.WriteLine($"{diskFull} after {wait}");                    // disk full after 1.5s
```

A method that panics prints Go's `%!v(PANIC=String method: …)` text instead of throwing. A Go func value (a .NET
delegate) keeps its .NET text. A value of a generic Go type prints its fields or its underlying value, not its
`String()` or `Error()`, because its methods are generic extension methods, which `ToString()` does not call. A
`*big.Float` prints its `String()`, 10 significant digits, where `%v` calls its `Format` method and prints the
shortest exact text.
`fmt.Sprintf("%v", x)` gives the Go text of any value as an `@string`. Converted Go code's `print` and `println`
still print a named value's raw number and a pointer's address, as in Go. One gap: in converted generic code,
`println` of a type-parameter value prints its `String()`, where Go prints the raw value, because the converter can
pass the raw value only for a concrete named type. No converted standard-library code does this.

A debugger display also calls `ToString()`, so it runs the type's `String()`. A `String()` that blocks or runs long
hits the debugger's evaluation timeout.

`fmt` formats Go values, so give it a `slice<T>` rather than a C# array:

```csharp
slice<nint> scores = new nint[] { 7, 8, 9 };
// fmt.Sprintf("%v", scores) == "[7 8 9]"
```

## Errors

Go reports failure with an `error` value rather than an exception, and you check it exactly as in Go. A function
with several results returns a C# tuple:

```go
n, err := strconv.Atoi("42")
if err != nil {
    fmt.Println("not a number:", err)
}
```

```csharp
var (n, err) = strconv.Atoi("42");
if (err != nil)
{
    Console.WriteLine($"not a number: {err.Error()}");
}
```

Wrapping and matching work as in Go:

```csharp
var (_, bad) = strconv.Atoi("forty-two");
error wrapped = fmt.Errorf("reading config: %w", bad);
// wrapped.Error() == "reading config: strconv.Atoi: parsing \"forty-two\": invalid syntax"
// errors.Is(wrapped, strconv.ErrSyntax) is true
```

Compare an error with `nil`. To produce a nil error from C#, write `null` (or `default`), and make a Go error with
`errors.New`:

```csharp
static error CheckName(@string name)
{
    if (len(name) == 0) return errors.New("empty name");
    return null;                        // a nil error: write null (or default)
}
```

Where the rest of a C# program expects exceptions, turn the error into one at the boundary:

```csharp
static nint ParsePort(@string text)
{
    var (port, err) = strconv.Atoi(text);
    if (err != nil) throw new FormatException(err.Error());
    return port;
}
```

`Must()` does this in one call, on a bare `error` result or on up to three values before the `error`: it returns the
values when the error is nil, and otherwise throws `GoErrorException`. That is a `PanicException`, as a failed Go
`Must` panics, and its `Err` is the original error, so `errors.Is` still works:

```csharp
nint port = strconv.Atoi("8080").Must();    // the value, or a GoErrorException
try
{
    strconv.Atoi("eighty").Must();
}
catch (GoErrorException ex) when (errors.Is(ex.Err, strconv.ErrSyntax))
{
    // ex.Message is the error's text: strconv.Atoi: parsing "eighty": invalid syntax
}
```

## Panics and exceptions

A Go `panic` that is not recovered reaches C# as a `PanicException`. Its `State` is the value Go passed to
`panic`, and its `Message` is that value's text, as Go prints it:

```csharp
try
{
    uuid.MustParse("not-a-uuid");
}
catch (PanicException panic)
{
    // panic.Message starts "uuid: Parse(not-a-uuid)"
}
```

The other way round, a C# exception thrown from C# code that Go calls (a C# `String()` method that `fmt` calls,
say) reaches your C# caller unchanged. Go's deferred calls still run on the way out, and Go's `recover()` does not
catch it, so Go code cannot swallow it:

```csharp
sealed class NotReady : fmt.Stringer
{
    public @string String() => throw new InvalidOperationException("not ready");
}
```

```csharp
fmt.Sprint(new NotReady());     // fmt calls the C# String method, under a Go recover()
// throws the C# InvalidOperationException from NotReady.String()
```

Two exceptions are Go's own: `NullReferenceException` and `DivideByZeroException` are how .NET reports a nil
dereference and an integer division by zero, so Go code treats them as Go runtime panics, which `recover()` can
catch.

## Slices

A Go slice is `slice<T>`. `foreach` over a slice gives `(index, value)` pairs, so it reads like Go's `range`; take
`(_, v)` for the values alone:

```csharp
slice<@string> words = strings.Fields("  the quick  brown fox ");
var lengths = new StringBuilder();
foreach (var (i, w) in words)       // Go: for i, w := range words
{
    lengths.Append($"{i}:{len(w)} ");
}
nint letters = 0;
foreach (var (_, w) in words) letters += len(w);    // Go: for _, w := range words
```

`foreach` gives a slice's `(index, value)` pairs, as Go's `range` does, and LINQ sees the slice as its values. The
same holds for a Go array and for a named slice or array type:

```csharp
// words.Count(w => len(w) == 5) == 2
List<@string> list = new(words);
@string[] array = words.ToArray();
```

A C# array converts to a slice implicitly, and the slice **shares** the array, as a Go slice shares its backing
array: a write through either one shows in the other. Converting a slice back to `T[]` makes a copy. `ToSpan()`
gives a `Span<T>` over the slice's elements without copying.

```csharp
int[] numbers = [1, 2, 3];
slice<int> shared = numbers;        // wraps the same array: no copy
shared[0] = 10;
int[] copy = shared;                // converting back to T[] copies
Span<int> span = shared.ToSpan();   // a view, no copy
```

`len` works on a slice, an array, a map, a channel or a string, as in Go.

## Maps

A Go map is `map<K,V>`. Reading a missing key gives the zero value, the comma-ok form tells you whether the key is
there, and `delete` removes one:

```go
ages := map[string]int{"ann": 31, "bob": 42}
age, ok := ages["ann"]
delete(ages, "bob")
```

```csharp
var ages = new map<@string, nint> { ["ann"] = 31, ["bob"] = 42 };
nint unknown = ages["zed"];         // a missing key gives the zero value
var (age, ok) = ages["ann", ꟷ];     // Go: age, ok := ages["ann"]
delete(ages, "bob");
nint total = 0;
foreach (var (name, years) in ages) total += years;   // Go: for name, years := range ages
```

The `ꟷ` in the comma-ok read comes from `go.builtin`; it picks the form that returns `(value, ok)`. The order of
a `foreach` over a map is randomised, as in Go, so do not depend on it. A Go map is also an `IDictionary<K,V>`, so
it goes to C# code that takes one:

```csharp
IDictionary<@string, nint> dict = ages;   // a Go map is an IDictionary<K, V>
```

## Pointers

A Go `*T` is `ж<T>`, a box holding a value on the heap. Go's `new(T)` is `@new<T>()`, and `var` holds the box
without your writing its type:

```csharp
var sb = @new<strings.Builder>();   // Go: sb := new(strings.Builder)
sb.WriteString("go");
sb.WriteString("pher");
// sb.String() == "gopher"
```

Every pointer method takes the box. Some, such as uuid's `UnmarshalText`, also take a plain local, passed by
reference, so the method updates it:

```csharp
var target = new uuid.UUID();
error uerr = target.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
```

`Ꮡ(value)` makes a new box holding a copy of the value, like Go's `&T{...}`, and `.Value` reads or writes what a
box holds. For a value method, call it on `.Value`.

```csharp
ж<uuid.UUID> boxed = Ꮡ(new uuid.UUID());    // Go: boxed := &uuid.UUID{}
boxed.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
nint count = 1;
ж<nint> counter = Ꮡ(count);         // a new box holding a copy of count
counter.Value = 2;
// count is still 1
```

## Goroutines and channels

`goǃ` starts a goroutine, from C# as from converted Go, and a Go channel is `channel<T>`, with `Send` and
`Receive`:

```csharp
var results = new channel<nint>(1);
goǃ(() => results.Send(42));        // Go: go func() { results <- 42 }()
// results.Receive() == 42
```

`sync.WaitGroup` works as in Go. `Close` closes a channel, and `foreach` ranges over it until it is closed and
drained:

```csharp
var wg = @new<sync.WaitGroup>();
var squares = new channel<nint>(3);
for (nint i = 1; i <= 3; i++)
{
    wg.Add(1);
    goǃ(k => { squares.Send(k * k); wg.Done(); }, i);   // i is passed now, as in Go's go f(i)
}
wg.Wait();
squares.Close();
nint sum = 0;
foreach (var sq in squares) sum += sq;  // Go: for sq := range squares, until it is closed
```

`TrySend` and `TryReceive` never wait: they return `false` when the operation would block. An unbuffered channel
(`new channel<T>(0)`) makes the sender wait for a receiver, as in Go, and Go's `select` is available too
([Channels and select](ConversionStrategies.md#channels-and-select)).

```csharp
var mailbox = new channel<@string>(1);
// mailbox.TrySend("hi") is true, and a second TrySend is false: the buffer is full
// mailbox.TryReceive(out var got) is true, with got == "hi"
```

`ReceiveAsync` and `SendAsync` are the awaitable forms of `Receive` and `Send`, for C# code that must not block a
thread. They are the same channel operations, waiting in the channel's own queue with Go's rules, and they take a
`CancellationToken`: a canceled receive takes no value, and a canceled send delivers nothing.

```csharp
var replies = new channel<nint>(0);
Task<nint> reply = replies.ReceiveAsync();      // awaitable: no thread waits for the sender
goǃ(() => replies.Send(99));
// reply completes with 99
```

A few things behave as Go's runtime does, so they are worth knowing from the C# side:

- Each goroutine runs on its own thread.
- `Send`, `Receive` and `WaitGroup.Wait` block the calling thread until they can complete. Keep them off a UI
  thread: await `ReceiveAsync` or `SendAsync` instead, or run them inside `Task.Run`.
- A panic that a goroutine does not recover ends the process, as it does in Go. The goroutine runs on its own
  thread, so a `try`/`catch` around `goǃ` cannot see it; catch it inside the goroutine's own body instead, or
  let it end the program as Go would.

## A real third-party module: uuid

[`github.com/google/uuid`](https://github.com/google/uuid) shows most of these shapes at once. `uuid.UUID` is a Go
`[16]byte`, a fixed-size array, and it indexes like one:

```csharp
uuid.UUID id = uuid.New();
@string text = id.String();
var (parsed, perr) = uuid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
var (_, badId) = uuid.Parse("not-a-uuid");
// perr == nil and badId != nil; parsed[0] == 0x6b, and len(parsed) == 16
Span<byte> view = parsed.ToSpan();
```

A Go array is a value: `copied := parsed` copies its 16 bytes. A C# assignment copies only the struct around
them, so both variables would share the bytes; `Clone()` makes the copy Go makes:

```csharp
uuid.UUID copied = parsed.Clone();  // Go: copied := parsed
```

uuid's `Version` type is `uuid.ΔVersion`, because the package class also holds the `Version` method:

```csharp
uuid.ΔVersion version = parsed.Version();   // Go: uuid.Version
// version.String() == "VERSION_1"
```

`MustParse`, like other Go `Must` functions, panics on bad input; see [Panics and exceptions](#panics-and-exceptions).

## Generics: hashset

[`github.com/ritchiecarroll/hashset`](https://github.com/ritchiecarroll/hashset)'s `HashSet[T]` is a Go map type,
and its generic functions and methods keep their type parameters. `hashset.NewHashSet<int>` is Go's
`hashset.NewHashSet[int32]`:

```csharp
var set = hashset.NewHashSet<int>(new[] { 1, 2, 3 });   // Go: hashset.NewHashSet[int32]
// set.Add(2) is false: 2 is already there; set.Add(4) is true
var keySum = 0;
foreach (var (key, _) in set) keySum += key;   // a Go map: (key, value) pairs
slice<int> keys = set.Keys();
```

`AsReadOnlySet()` hands any Go map's keys, a plain `map<K,V>` or a named map type like `HashSet<T>`, to C# code
that takes an `IReadOnlySet<K>`. The view is live and read-only, and a nil map is the empty set:

```csharp
IReadOnlySet<int> members = set.AsReadOnlySet();    // a live, read-only view of the keys
// members.Contains(4) is true
```

<a id="rough-edges"></a>

## Good to know

Each item is a first surprise, and what to write instead.

- **A Go method does not compile.** Import the package's namespace as well as its alias: `using go;` for a
  standard-library package without a slash, `using go.encoding;` for `encoding/hex`, `using go.github.com.google;`
  for uuid ([Imports](#imports)).
- **Warning CS8981 on each alias.** Add `<NoWarn>$(NoWarn);CS8981</NoWarn>` to the project.
- **`int n = len(s)` does not compile.** Go's `int` is `nint`: use `var`, `nint`, or a cast ([Numbers](#numbers)).
- **`error e = nil;` does not compile.** Compare with `nil`, but produce a nil error with `null` or `default`.
- **`foreach (var v in s)` gives a tuple, not a value.** `foreach` over a slice gives `(index, value)` pairs: write
  `foreach (var (_, v) in s)` for the values, or use LINQ ([Slices](#slices)).
- **`$"{f}"` on a Go func value prints the delegate's .NET type name.** Print it through `fmt` instead
  ([Printing Go values](#printing-go-values)).
- **A copy of a Go array value shares its bytes.** `var b = a;` on a `uuid.UUID` copies the C# struct, not the
  array inside it: write `a.Clone()`.
- **A Go map type is not an `ISet<T>`.** It is an `IDictionary<K,V>`; hashset's `HashSet<T>` is
  `IDictionary<T, EmptyStruct>`. Use its Go methods, `len` and `foreach`, pass it where an `IDictionary` goes, or
  call `AsReadOnlySet()` for an `IReadOnlySet<T>` of its keys ([Generics: hashset](#generics-hashset)).
- **Errors are values, not exceptions.** Check `err != nil` after each call that can fail, and throw where your C#
  wants an exception, or call `Must()` ([Errors](#errors)).
- **A channel call blocks its thread.** Keep `Send`, `Receive` and `Wait` off a UI thread: await `ReceiveAsync`
  or `SendAsync`, or wrap them in `Task.Run`.
- **Glyphs.** Consumer code rarely types one: `var` hides `ж<T>`, `@new<T>()` stands in for `Ꮡ`, channels have
  `Send` and `Receive`, and aliases keep call sites Go-shaped. What remains is `goǃ`, which completion lists when
  you type `go`, `ꟷ` in a map's comma-ok read, and the occasional `Δ` name such as `time.ΔSecond`. Every glyph
  is in the table at
  [Reading Converted Code: Names and Glyphs](ConversionStrategies.md#reading-converted-code-names-and-glyphs).

## Further reading

- [Limitations](Limitations.md): where converted Go differs from Go, and what to do about it.
- [Frequently asked questions](FAQ.md): short answers for Go and C# developers meeting go2cs.
- [Conversion Strategies](ConversionStrategies.md): how each Go construct is converted, and why.
