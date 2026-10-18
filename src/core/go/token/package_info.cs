// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.token_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoTypeAlias("File", "ΔFile")]
[assembly: GoTypeAlias("Pos", "ΔPos")]
[assembly: GoTypeAlias("Position", "ΔPosition")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<node, ж<node>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/token/position.go", "position.cs", "ACJEkAACFAAIAoKCgpSCgqaClAAgRJIAFC6SqJKokqiSqLKCgoKs0oKAgqSuAAkCgpSCgoLugqKowoKCggACFgALBIKCgsqCgoKqwoKCgoKUgoLKgoKsAAgCgpSCgoKUAA8eogACFgAKAoKAgqSokpSCxoLGuIK4AAIYAAkCAAIYAAkCqqKmgoamlKwACAKCgoCCpJSAgoKCgJSCgsqSpgAIDoKmsoKCggACEPKClKyyACJIks7CgoKCAAIkABUElIKCgpSClIKCpoKCpoKCgt4AGyaClIKCtAACEAAMAoKUlIKCtKjCgsqigoLkpMSAgraClIKApoKk3NKClK7igoCCtqqirAALFJKClIKUpg==", "334-336:1;559-563:1")]
[assembly: global::go.GoPositionMap("go/token/serialize.go", "serialize.cs", "ABkwsoKAgraCgoIABxCClKiylIKCgoKC7pSClA==", "", "", "65=Clone/1/2/4,Clone/2/2/5")]
[assembly: global::go.GoPositionMap("go/token/token.go", "token.cs", "AIQBjAIAVNYB0oKClIKUAAsispSkpKSklMqCgoK6koCCpK7AqKKqoKaSgqikgqyygpSCgqY=")]
[assembly: global::go.GoPositionMap("go/token/tree.go", "tree.cs", "ADRqggACGAAJApSklKrSgoKClIKUgoK4goKCgpSClIKCAAMQAAgCkoKCgpKClKauwoKClIKCgqaClKYABhDClKSkpLiigoKUlIKClKaigoK4soKCuLKCgriigpSmgoKCpqKUgpS0tLQABBQACAKCgoKUgpSmgpSUuJS81IKClIKUgoKUgoKq1IKClIKUgoKUgoKq0oKCgpSAggAGENKAuIKUgqSCgqiylILKgpSmgIKktoKCtraCgoKCgqrCgpSCgoKUgoKCgpSs4oKUgoKClA==", "132-151:1", "", "362=Name/1/2/1,Base/1/2/1,End/1/2/1,Name/2/2/2,Base/2/2/2,End/2/2/2")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("token")]
public static partial class token_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct node {}
    internal partial struct serializedFileSet {}
    internal partial struct tree {}
    internal partial struct Δkey {}
    public partial struct FileSet {}
    public partial struct Token {}
    public partial struct lineInfo {}
    public partial struct serializedFile {}
    public partial struct ΔFile {}
    public partial struct ΔPos {}
    public partial struct ΔPosition {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
