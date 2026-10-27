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
global using commentꓸText = go.go.doc.comment_package.ΔText;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.doc_package;

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
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: global::go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/doc/comment.go", "comment.cs", "AAtAABQCgoKCggACNAAXAoLK")]
[assembly: global::go.GoPositionMap("go/doc/doc.go", "doc.cs", "AG/yAeKCgoKCABAigoKClKaCgoLKgoKUlIKCgoKCgriCgoKCgIKklAAEHAALAoKCgpSCgpSCgtyigoKClIKClIKCAAg4ABsEgpSC6IKClLS4uoKCgpSApLSCtAAKFIKCggACFPKClAACFPKAgoKUpIKUrsIABRTmAAIQ8gACEPIAAhQACQI=", "", "", "123=sortedKeys/1/1/4,noteBodies/1/1/7,sortedValues/1/2/8,sortedTypes/1/1/9,sortedValues/2/2/10,sortedFuncs/1/1/11")]
[assembly: global::go.GoPositionMap("go/doc/example.go", "example.cs", "ACtkABICgoKCgoKCgIKCpIKClIKCgoKUgpSAgqSAgqSClIKClKIACxi4gpSmhqaWsoCUgoCCgpSUgoKUxqyygpSClILqwpSmuIKUgpSClIKCgtiClLSCAAkQmrLKgpKCgqamgoKClKaUgoKClIK2ppSSkoKCgsqCgtyCqJKCgILagoLcpoKUgsaC7Mqm3IKCgpSGqgAGHAAMDIKCgpSCwpSCkoCEgoKCttqC2oKUpoKCgtymgqaUgqaCxoKUgoK0gpSCAAgeAAgCgpS0hJKClILGgu6CgpSSgoKCgoKCuILIlIKmkoKUlNqmgqaAgraqooKCgpSoxIKUiqKCgoKUlIKm6tSCgqiSooCUkoKCpLiSgpKCqtSCgrjKgoKCqNKClJKCgpSClJQAAhwADQKCpoKCgoKUlIKClIKCgpSUgoKUyu6CgoKUgoKUgoLKggADFtKCgpSClJQAAhIACQKClIKUgqaCgqzSgoKC", "110-112:1;207-214:1;315-317:2;318-320:3;349-377:1;379-385:2;523-525:1;680-682:1", "", "108=playExample/1/1/4;349=NewIdent/1/1/1;364=NewIdent/1/1/1;663=Pos/1/1/3")]
[assembly: global::go.GoPositionMap("go/doc/exports.go", "exports.cs", "AAwgooKCgoKmppSCgoKCuIKCgpS0gIKkgILGgpSqwoKClKaokoKCpqiSgoKCgoCUgILGgoKmgpSu8oKUgoKCgoCUgoKCkriCAAcQpoKClIK2goKCpoKUgqiygoIABBCy2LS0gsaCgsa0gsaCgrSCxoK0yIKmpILugoKmgoKC/ICCgpKCgpTGrLKUpICU/KaChqKCgpSUlJTKgoKCgqamgpSC7JSosoKCgoKm", "", "", "310=NewIdent/1/1/1")]
[assembly: global::go.GoPositionMap("go/doc/filter.go", "filter.cs", "AAwWooKCgoLKpoKClIKC2IK4lILGguqmgoKCgoKmpoKCgoKCpqaCgoKCgqaCgoKClIKCpqqigoKCgg==")]
[assembly: global::go.GoPositionMap("go/doc/reader.go", "reader.cs", "ACJAopSktraCgoKCgoKClIK21oKAgqSu4pKA3LaSgpSAgqSU7oK+0oKCgpSUAAYY0pSkpKSAptaklAAycoKuwoKUgIK23IKu8oKClICCgoKkpqaCgoKUpoKClKaCgpSCpqgACAqCgoKCgoKClIKmgIL6pJSmgpSClIK4griCgpSAgsjcgtyowoKUgrSkgpSowoKCyqaClJSCgpSasoKCgsySqMSCuJSmlIKmlICC/sqCgoKCgoCmpICCppSAgoKCggAIDoKCyqrCgpSCgoK4uoKawoKCgoKClIKCuICCpKiSgoDKkoKCAAoe0oKCgoKCgpSmgszUgoKCyoKUpoKAgoCCgoKCgoKmgoKUgoKSggAKEMbcgIKklIKAygAKFgAKEIKCuMSCgoKCgoK4goKClKaCgoKUpoKCyoKCgIIABhLCgriSkoKCgoKClKaSppKmkoKUlKiygtyClIKmgqaokpSUAAgSgoIABBLCgoKU7pSmpoKUgILsggAEEKKCgoKClIKokoKAgramgoKCgoKCppSCgoKUpqaCgoKCAAgSppimgoKUpoKCgpT8graChqqigoKUrsKmABgyABQqAAYUsp6CgoCCgoLIgoCCpA==", "852-858:1;879-881:1;909-911:1;990-995:1", "", "112=Text/1/1/1;373=Text/1/1/1,specNames/1/1/2;596=Pos/1/1/1,End/1/1/2;708=Pos/1/1/7;969=sortedValues/1/2/4,sortedValues/2/2/5,sortedFuncs/1/2/6,sortedFuncs/2/2/7")]
[assembly: global::go.GoPositionMap("go/doc/synopsis.go", "synopsis.cs", "AAwgsqKigpSClIKUlK7CggAHEAAGGAAIAoKCgoKmgoKCgpSAgqSC")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("doc")]
public static partial class doc_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct embeddedSet {}
    internal partial struct methodSet {}
    internal partial struct namedType {}
    internal partial struct reader {}
    public partial struct Example {}
    public partial struct Func {}
    public partial struct Mode {}
    public partial struct Note {}
    public partial struct Package {}
    public partial struct Type {}
    public partial struct Value {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdocꓸcomment() => builtin.initPackage(typeof(global::go.go.doc.comment_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸlazyregexp() => builtin.initPackage(typeof(global::go.@internal.lazyregexp_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
