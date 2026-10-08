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
using static go.main_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<sliceErr, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("main.go", "main.cs", "ADScAYCmooKAgoKklKIAAiKigoCCgqSUogA6BIKEgpCUgpCUopCUgpCUgpCUkpCUgpCUgpCUopCUgpCUgoSCkJSCkJSCkJSCkJSCioKQlIKQlqKEgoSCkJSChJCSkJKQkpCSkJSCoJKQlIKEgqCSoJKglIKEooSihJKEkoSihKKEsqKGkpKEgqKGkoaahIKIgpCSoA==", "82-88:1;108-114:1;122-122:1;125-125:2;127-127:3;128-128:4;131-131:5;134-134:6;136-136:7;137-137:8;140-140:9;143-143:10;145-145:11;146-146:12;149-149:13;155-155:14;158-158:15;161-161:16;164-164:17;173-173:18;176-176:19;186-186:20;191-191:21;192-192:22;193-193:23;194-194:24;195-195:25;198-198:26;199-199:27;205-205:28;206-206:29;207-207:30;261-261:31;262-262:32")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("nestedComparable")] [GoValueClone("A")] internal partial struct main_nestedComparable {}
    internal partial struct myMap {}
    internal partial struct mySlice {}
    internal partial struct outer {}
    internal partial struct point {}
    internal partial struct sliceErr {}
    internal partial struct withAny {}
    internal partial struct withFunc {}
    internal partial struct withMap {}
    internal partial struct withSlice {}
    public partial struct inner {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}
