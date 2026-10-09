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
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jwtꓸVerificationKey = object;
// </ImportedTypeAliases>

using go;
using static go.github.com.golang_jwt.jwt.v5.request_package;

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
[assembly: GoImplement<ArgumentExtractor, Extractor>(Pointer = true)]
[assembly: GoImplement<ArgumentExtractor, Extractor>]
[assembly: GoImplement<BearerExtractor, Extractor>(Pointer = true)]
[assembly: GoImplement<BearerExtractor, Extractor>]
[assembly: GoImplement<HeaderExtractor, Extractor>(Pointer = true)]
[assembly: GoImplement<HeaderExtractor, Extractor>]
[assembly: GoImplement<MultiExtractor, Extractor>(Pointer = true)]
[assembly: GoImplement<MultiExtractor, Extractor>]
[assembly: GoImplement<PostExtractionFilter, Extractor>(Pointer = true)]
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
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/request/extractor.go", "extractor.cs", "AAkWAAccpIKAgrYABBCkhoKAgrjMhIKAgpKCtgAHFIKAgpQACxKihoKU")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/request/oauth2.go", "oauth2.cs", "AAcQhIKUut4=")]
[assembly: go.GoPositionMap("github.com/golang-jwt/jwt/v5@v5.3.1/request/request.go", "request.cs", "AAketIaCmIKUgpiCgpisggALGoKCuqKC", "60-62:1;67-69:1")]
// </GoSourcePositionMaps>

namespace go.github.com.golang_jwt.jwt.v5;

[GoPackage("request", ImportPath = "github.com/golang-jwt/jwt/v5/request")]
public static partial class request_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial interface Extractor {}
    public partial struct ArgumentExtractor {}
    public partial struct BearerExtractor {}
    public partial struct HeaderExtractor {}
    public partial struct MultiExtractor {}
    public partial struct PostExtractionFilter {}
    public partial struct fromRequestParser {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸgolang_jwtꓸjwtꓸv5() => builtin.initPackage(typeof(go.github.com.golang_jwt.jwt.jwt_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}
