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
using static go.bytes_package;

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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("bytes/buffer.go", "buffer.cs", "AClYkgADEtCswNzilJSokKigqKCmkKqygoKUgoKUrLKCgqyygIKCpKyylIKmgIKkgoKUgsqkpqaCggACENKClIKssoKCgpSssoKCgpQACBrigoKCgoKCloKCgpSCzsKCggAKFoKmlIKC3uKCgIKCgpSCgoK4gsiCrsKCgoKUgq7UgoKUgoKClIKu4oKUgoKUlIKCgpSuwoKCgpSCgoKUqqKUgpSCgoIAAhDylIKUgoKCgpSCgoIABRDSgpSClIKmnMKClIKClAACEgAJAqaCqMKCgoKClIKCggACEgAIAoIAAhgACQAAAhDi", "230-234:1")]
[assembly: go.GoPositionMap("bytes/bytes.go", "bytes.cs", "ABMoxKyyqqKClIKCgoKCgoKUgoKClKq0gpSClIKCgoKUgrqSqJKokqiSqJKmgoKCpqiSgpSkpIKUpKSokgACENKClKSCgoKUlKSqsoKCgoKCgoKClKaCgqaCgoKChKaEhqKAgtqCgoKCgoKCptgABBDClJSCgpSigqaUgpSUgoKClJSCgIKCgqa2goKCgoKUgpSClIKClKaCgpSmgoK4rsKUlIKAgoKCpraCgoKigqaUgpSUgoKClIKCgoKmlIKCgoKUgpSCgpSCgpSmgoKUpoKCuKqigpSClIKUgpaCgoKCgoKUgoKUggACGAAJAAACEvIAAhLgrMKmnOaClIKCgoKCgpaUqIKCgpSClIKCgoKUgoKUgpSUkpQACRT2AAQUgoKCgoKUgoKCpoKmqIKogoKWqqKClJSWgoKClJSCgpSWgoKCgpSokqiSrviCgoKCgpSCgpSUAAImABACgsyClIKClISCAAEaAAoCgoKCgqaCgoKClKqigoKCgoKUlpKUlIKCgoKUlJSqooKCgoKClJaSgpSCgoKClJSUqJCoorqiuqK6ooKCgoKCgoKClIKCgoKClJSCgpSqtIKUpKSkpKaCpgACEAAICIKUgoKUgs6igoKUqqKCgoKUlKqiqqKClKqigpSssqyyrLKCgoKCgpSClJSssoKSgpSCgqYABB7CgoKClJSokqyygoKmqqKUlIKUgpSAkqSqopSUgpSClICSpKaCgpSUlKaigoKUlJSUpoKCkoKUgpSUlJSqooKUgpSAkqSmgoKUpqKCgpSUpoKCkoKUgpSUqrSCgoKmlIK6goKCgpSC3qaUqqKCgoKCgoKUAAIS4oKUlJSUgqiCgoKCgoKCgqaUgoKUggACENKsxIKCgoKCqIKogqaClKaEgoKChKKClIKUgpSCvIKogqaUgpS6goKUgpSoqJKClKSkgpSkppKUgoKCgoKCpoKClJSClIKUgoKClKakgoKCgoKCgoKClJSClIKCAAgSgoKUpgACEuKAgqSssoKUAAIS4oKUAAIS4oKU", "838-845:1", "758=unicode.SpecialCase.ToUpper;764=unicode.SpecialCase.ToLower;770=unicode.SpecialCase.ToTitle")]
[assembly: go.GoPositionMap("bytes/iter.go", "iter.cs", "AAwk0oKCgoCClKSCprqSgoKCgpTOooKUkoKCgpSCgpSUAAMQwq7CrsKSgoKCgoKCgpSCgoKUtpSUggAEEsKSgoKCgoKUgoKClLaUlII=", "19-32:1;37-45:1;54-67:1;91-116:1;124-147:1")]
[assembly: go.GoPositionMap("bytes/reader.go", "reader.cs", "ABc0ooKUrLCmwoKUgoKC2NSClIKUgoKUqJKCgpSCgtiSgpSCgqjSgoKUgoCCgqSCguiSgpSClIKC6JKCgpSkpKSkgpSCqMKCgpSCgoKUgoKClKiQppA=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("bytes")]
public static partial class bytes_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("span")] internal partial struct FieldsFunc_span {}
    internal partial struct asciiSet {}
    internal partial struct readOp {}
    public partial struct Buffer {}
    public partial struct Reader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
