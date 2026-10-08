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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
// </ImportedTypeAliases>

using go;
using static go.strings_package;

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
[assembly: GoImplement<appendSliceWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<byteReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<byteStringReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<genericReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<singleStringReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
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
[assembly: go.GoPositionMap("strings/builder.go", "builder.cs", "ABk0otySgrqSqJCqsKaSgqqigoKs0oKClIK8woKCqsKCgqrCgoKCqsKCgg==")]
[assembly: go.GoPositionMap("strings/clone.go", "clone.cs", "AAoqAAoC")]
[assembly: go.GoPositionMap("strings/compare.go", "compare.cs", "AAoe4g==")]
[assembly: go.GoPositionMap("strings/iter.go", "iter.cs", "AAwk0oKCgoCClKSCprqSgoKCgpTOooKUgoKCgpSCgpSUAAMQwq7CrsKCgoKCgoKCgpSCgoKUpIKUlIIABBLCgoKCgoKClIKCgpSkgpSUgg==", "19-32:1;37-45:1;54-67:1;91-116:1;124-147:1")]
[assembly: go.GoPositionMap("strings/reader.go", "reader.cs", "ABYyooKUrsCmwoKUgoKC2NSClIKUgoKUqJKCgpSCgtiSgpSCgqjSgoKUgoCCgqSCguiSgpSClIKC6JKCgpSkpKSkgpSCqMKCgpSCgoKUgoKClKiQqKA=")]
[assembly: go.GoPositionMap("strings/replace.go", "replace.cs", "ABlAAAgCgpSmgoKmgoKCloKCgpSCqIKSgriCgoKUlraCgpS4lJaosoKosoIANG6igoKClJaEkoKCpoKSiLKClMqCgoKCgoKmuIKCpJSCgpSUgoK49oKCgoKCgoKCloKUgoKClIKCkoKCgpSmAA0cgpSCgoKogpaCgoKUgriEgpTMkoKokoLugqaCgoKUpoKSgqbCgpKClIKCgoK6goKCgoKClIKCgpSCgpSUgoKUAAoWgqaCgpKCgoKUgoKCgpSClIKmsoKSgoKClIKCgpSCgoKUlIKCAAQQgoKCgoKClKaClKaigoKCgoKUgoKCgqaCgoKCpoKCgoKmABcwgoKUgoKAlILKgoKUgriClIKCgoKClIKmprKCgoKCgpSCgoKCpoKCgoKmgoKClA==")]
[assembly: go.GoPositionMap("strings/search.go", "search.cs", "AC5ggsqogsqCzIKCgqamgoKUqKaigoKmqqKClIKCgpSClJQ=")]
[assembly: go.GoPositionMap("strings/strings.go", "strings.cs", "ABYusoKClIKCgoKUgpSqtIKUgpSCgoKClIK6kqiSqJKokqiSgpSkpIKUpLaCgoKClIKUgoKCgoKmqJKuwoKUpIKCpqSqgoKCgrKCgoKUpoKCpoKCgoKEpoSEkoCCtoKCgoKCgoKm2MyilJSUgoKUlIKAgoKCpraCgqasspSUgoKClIKUlIKAgoKCpraCgoKUgoKCgqaUgoKCgqaokqqigpSClIKWgpSCgoKCgoKUgoKUggACHgAMAAACGAAKAgACHgAMAAACGAAKAqaa1oKUgoKCgoKClpSmgoKClIKUgoKCgpSCgpSClJSSlAAJEuYABBSCgoKCuKaCzIKogoKWqqKUpKaCgoKUlIKClJaCgoKCgpSokqiSrO6kooKCloKCgoKmloKCgpaCqJKWgoS4gqa6ABA4wpSk3IKUgoKUhIKolJSkpKSkAAUcAAoCgoKCgqiCgoKCgpSokoKCgoKClJaSgpSYgoKCgoKClIKmgpSUqJKCgoKCgpSWkoKUmIKCgoKCgpSCpoKUlKqgqKK6orqiuqKEgoKWgoKCgoK6kpaCgoKCgoKClIKCgoKClJSCgpaqtIKUpKSkpKaCpgACEAAICIKUgoKUgs6igoKUqqKCgoKUlKqiqqKqoqyygoKmrLKCgoKCpgAEHsKCgoKUlKiSqqKClIKUgJKkrsKClIKUgJKkpoKClKaigoKUlKaCgpKClIKUlK7CgpSClICSpKaCgpSmooKClJSmgoKSgpSClJSqtIKCgqaUgrqCgoKUlILeqqKqogACEuKCqICCkoKokoKCgoKCgoKmlIKClIIAAhDSrMSCgoKCgqiCqIKmgpSmhIKCgrSCmJKClIK8gqiCppSClLqCgpSClKiokq7CrsKuwg==", "870-877:1", "769=unicode.SpecialCase.ToUpper;775=unicode.SpecialCase.ToLower;781=unicode.SpecialCase.ToTitle")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strings")]
public static partial class strings_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface replacer {}
    [GoLocalName("span")] internal partial struct FieldsFunc_span {}
    internal partial struct appendSliceWriter {}
    internal partial struct asciiSet {}
    internal partial struct byteReplacer {}
    [GoValueClone("replacements")] internal partial struct byteStringReplacer {}
    [GoValueClone("mapping")] internal partial struct genericReplacer {}
    internal partial struct singleStringReplacer {}
    [GoValueClone("badCharSkip")] internal partial struct stringFinder {}
    internal partial struct stringWriter {}
    internal partial struct trieNode {}
    public partial struct Builder {}
    public partial struct Reader {}
    public partial struct Replacer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
