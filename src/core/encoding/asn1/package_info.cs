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
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.encoding.asn1_package;

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
[assembly: GoImplement<StructuralError, error>]
[assembly: GoImplement<SyntaxError, error>]
[assembly: GoImplement<bitStringEncoder, encoder>]
[assembly: GoImplement<byteEncoder, encoder>]
[assembly: GoImplement<bytesEncoder, encoder>]
[assembly: GoImplement<int64Encoder, encoder>]
[assembly: GoImplement<invalidUnmarshalError, error>(Pointer = true)]
[assembly: GoImplement<multiEncoder, encoder>]
[assembly: GoImplement<oidEncoder, encoder>]
[assembly: GoImplement<setEncoder, encoder>]
[assembly: GoImplement<stringEncoder, encoder>]
[assembly: GoImplement<taggedEncoder, encoder>(Pointer = true)]
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
[assembly: go.GoPositionMap("encoding/asn1/asn1.go", "asn1.cs", "ACxWgP6ArNKCgsyUtLS2rsKClIKUgpSq0oKClJSClIKCqIKCqqKAgqSCgpSClKaYooCCpIKUgoKUgoKClIIADB6igpSCgqqigoKWgoKCgpaowoKClIKGgpSCgtymAAMQsqaCgoSCgoKUlqzigoK6zIKClIKClIKWgoKCgpSUggAGHgAJAoKCpoKClIKmgoKUgoKClIKUpoKqwoSCgoKClIKWgIKCppSW2tKChICCpoCCpq7CgoKmqJIAAhDygoKCpoIACyKyAAIwAA4CgoKCpoKuwt7CgpTewoKogIKmgoKClgAPLAAKAqaCgpSCgoKCqIKCgqaCgqaCgpSCgpSmgoKClIKCgoKUgoKmgpSCgpSCuIKCqKzigoKCuoKCgoKClJqm5oKClIKClIKUgoKCgpSCgoKCgoKCpqiCgoKCgoKCmqLc4oKWgoKUqICCgoKClIKClIKCgpSkpKSkpKSkpKSkpKTqgoKUgpSmgoKUgoKClIKClIKUkoKCpoKClIK4goKUlKiCgoLegoKUtILMgpaCloKChIKCgpaCgoKWgoKCqJaCgpSUlIKClIKWlIKkgqSCpIKClIKkgoKUgqSCpIKClIKUgKSCgpSCpIKCgpSUgoKUlKaEkoKCqISCloKCgoKUgoKspIKygoKUgoKUgqSClKSkpKSspKbkgpS0gqqilKas0oKUgoKUgpQAApABAEUCAAsSgoKWgpSqwoKClIKClA==", "", "", "969=Field/1/1/1")]
[assembly: go.GoPositionMap("encoding/asn1/common.go", "common.cs", "AGm8AQAIAoKCgpS0goLGtLS0tLS0goKCxoKCgsa0goLGgoLGxtqilKSkpKSkpJSkpKSClIKUpKQ=")]
[assembly: go.GoPositionMap("encoding/asn1/marshal.go", "marshal.cs", "AB9CgqaCyoKmgoLcgqaCgtyCgoKUpoKCgoLcgoKClKYACBKCgoIABxKEgoKCAAsYgqaCgsqChIKCloKClqaChIK4goKWgoKWpoKEgoKCgpaWpqKClsqCgoKClIKUkpSUgqaUuIKEgpamooKCgpSmgoKClIKCgpSCloKCgpSWyoKmgoKC3IKCgpSmgoKCuIKClqaC7oKopoKCgqimgoKCqKaCpoKmgu6CgqaihIKClqaihIKClqaChJS0tKamgoKCloSmgoSChISCgoSElKS0toKCloKEpoKCgpSmopSkgoKUpKSkpoCkgpSkpISCgqiEgoKaooKCypaWgKSkpIKCgoKo1oKCloSApKSkhIKCgqiClNaUpKSk2NaigqaCloKWgoKEgt6CgqiCgoKWhIKEloKCloKWgpaUuIKCgpSCuLaCuIKClAAGEIKWhIKCloSCgoKSgpSWgoSEhN6oloQAAhwACwKqooKClIKC", "", "", "686=Len/1/1/3")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("asn1")]
public static partial class asn1_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface encoder {}
    internal partial struct ampersandFlag {}
    internal partial struct asteriskFlag {}
    internal partial struct bitStringEncoder {}
    internal partial struct byteEncoder {}
    internal partial struct bytesEncoder {}
    internal partial struct fieldParameters {}
    internal partial struct int64Encoder {}
    internal partial struct invalidUnmarshalError {}
    internal partial struct multiEncoder {}
    internal partial struct oidEncoder {}
    internal partial struct setEncoder {}
    internal partial struct stringEncoder {}
    internal partial struct tagAndLength {}
    [GoValueClone("scratch")] internal partial struct taggedEncoder {}
    public partial struct BitString {}
    public partial struct Enumerated {}
    public partial struct Flag {}
    public partial struct ObjectIdentifier {}
    public partial struct RawContent {}
    public partial struct RawValue {}
    public partial struct StructuralError {}
    public partial struct SyntaxError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}
