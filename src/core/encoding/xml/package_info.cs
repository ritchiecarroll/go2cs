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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.xml_package;

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
[assembly: GoTypeAlias("Token", "ΔToken")]
[assembly: GoTypeAlias("ΔToken", "object")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Decoder, TokenReader>(Pointer = true)]
[assembly: GoImplement<SyntaxError, error>(Pointer = true)]
[assembly: GoImplement<TagPathError, error>(Pointer = true)]
[assembly: GoImplement<UnmarshalError, error>]
[assembly: GoImplement<UnsupportedTypeError, error>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, io_package.ByteReader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<printer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<StartElement, ж<StartElement>>(Indirect = true)]
[assembly: GoImplicitConv<StartElement, ж<StartElement>>]
[assembly: GoImplicitConv<fieldInfo, ж<fieldInfo>>]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/xml/marshal.go", "marshal.cs", "ABOkAQA4AoKCgIKkgIKkACVSsoKCgoCCpICCpAAHEpKCgqyyggACEgAIAoKClAACFAAKAoKClKiCggABHgAMBIKkgILWgILWtIKUgoKCqKKUgpSClIKCgoKUtIKUgoLElqqiqoKUgoCC6pLItJKUxoKUxqqirLIAFyzCgILugqiCgrqCgIKkgtyClJSCgIKCyoKEgoKCgoSEqJKCpoKmgoKCgoKUuoKCmuKCloKUgsyCgpSWgpaClIKCgrqClIKCgrqCgoCCtpaCggABEuSCgpKCgoKUgoCCyIKUgoKAlKSClKiCgoKUhIKWgpaCgILKhpSAgqaClIKCkoKUpoKWgIKmqLKCgoKUgpSWgoKCgoKUgpSogoKClIKWgoKCgoKUgrqUgpS4goKCgIK2loKCloKClIKUgqrSpoKCkoKCkoK4lKi2goSCgqiClIKowoCCpIKClILYwoKWgoSCgoSCgoKogoKClIKCgpSCgoKUgqaCgpSClICCgpSkhIKCgoKCgqaClKSkpKS0gpaSgpSClMSClsSmnMKCgpSUprKCgoKClIKmlpSCgpSAgqSCgoKUgIKklIKCgoKClICCpKiCgpSAgsaAgsaAgsaAgsaAgsaAgoCC6KaAgqSCgoKUgpSCgoKClIKCgoK2goKCgrakgpSUlIKmgoKUgrSCyICCpIKCgILqgIK2gtiygpSClKiygpSClKiSgpSClKyygpSCgIKkgpSokoKmgoKUgoKCgpSUgpSUgpSCgqaCggAIGLKCgoKmgoCCtoKokoKAgraCAAgSgqaClKyk")]
[assembly: go.GoPositionMap("encoding/xml/read.go", "read.cs", "ABGKAgBzAqqi7sKCgpaClMyAACFEkoKClKrkhIKCgoKCloKWrLKCgoKCgpSUgsa0tqiSgoKUlKaUgoKCuqaUgoKCqKaCgpaAgoKkloKClqiCgoIABBCWwoKmgoKCgpSAkoLugoKCqIKClJamloKCgqiCloKCgqgAChyAAAIQtoLEgpqCgoaAsoKktraCgoKWgoKCmJKCgpSCgoKUlJSCgIK6koKCgoKUgoKAgqS4gsiCgoKAgsySgpSCuIK4griCgoKClAAI9QEABYgCgoKCgpSCgpSkgqaCgpSCgoCCyIKAguqCgoKmtoLIguqCgIKkloKCgoCCpKiAgqaApKS2gKSkgsimgoSCgpSovJKClIKClKSCgpSCgpSkgoKUgoKUpIKClIKClKSklJQAAl8AAmQAAhAACgKCgoKCgpSCgqaUlLiogsaUyoKCgoKUqKKClIKAgugABBTSgoKCgpSUtIKU")]
[assembly: go.GoPositionMap("encoding/xml/typeinfo.go", "typeinfo.cs", "ACtimKKAgqaCgoKCgoKogoKClIKCgpSClKKCgIK2qIKCloKCqICCyoKokpaCgIK4goKUgoKUpKSkpKSkyoKApKSCuMSClIKUgsyCqLiClriAgpSkqIKClIKUgoKClMyCgoKCuKyygpSClIKCgpSCgriUAAIUAAkCgoSSgoKUgpSCgoKmgoKkgoKmguqCgrqCgrqCgoKCgsyCgoKUggAKFoIABRrSgoKCgoKClJSmlA==", "", "", "176=Get/1/1/1;182=Get/1/1/1")]
[assembly: go.GoPositionMap("encoding/xml/xml.go", "xml.cs", "ACA+ggAXOpKCgoKokgAJHJDOkAAIEpKCAAQQkKaSlKSkpKSUAFa+AbLcgqikgIKk7gAFOgAaAoKCgpSCgpSAgoKUyJSCgIKCtqzCgoKClJSCgqiCgoKUxoKUpAAGGNKUpKS0pICCkoLIyoCClAATKoKCgpSUgoKCpoKCgoKClKzogoK4gpSCgpSUgoKqooKUgqiSgqqigoKCqJIAAhIACAKCgpSCpIKCgoKUgqSCgpSEpqiCgoKUqKqigpSClIKClKammrKClAAQBoKClIKUuIKWgoKWlIKCgpSWgIKklpKAgoKUpIKAgqSCgpSokoCCgpSkgoKCgoCCpIKClJSChIKCgoKClIKCgoKUgoKClIKUpqiAkqSWgKKkgoKWgqKCgIKkgoKClJSUlIKCqKKAgqSCgqiSgpSsgoKCgrKAgqSClIKClOy2uIKigIKkgoKUgqqGwoKAgqSClJ7G+ISqgIKClKaCgoKAgqSCgoCCpIKClJSClISCgIKClKSCgIKkgoKClIKUgoKClJSUgoKU1oKCgqaCpoKCpoKCgoKCpoSUgqaokoKCgpTGggAFEuKClIKClIKClIKmgoKUgqyyrLKqooKClK7ygIKCtqiSgpSCpgANGMKSgoKCgoKCgoKUlMyCgoKUgqiCgoKUgpSClNyCgoKCgoCCpIKCgIKkgoKCgoCCtoKGgoCCtoKUgoKCgoK4goKCpoCCpIKUgoKCgoCCgpKC3IKCgoKUgoKUgoKUgqiCkqaWtIKWgoKCgoKUgoKCqKyyAAIU0oKClIKSgIKUgqSssoKCqIKCgpSs0oKAgqSCgpSEgoCCpIKClJSmgqyCgpSCgpSClIKCgoKUgqamgoKUgoKUgpSCgoKClIKmAAYQAMIBhgMAefIBAAkEAIMClgTUAA8qgoKCgoKCgoKaoqyygoKCgoKUtLS0tLS0gpS0tIKClLSAgqSAgqSUgqqigoKCgoKUtLS0tLS0tLSCgpS0goKUrLKogoKaooKUgIKmgoKCpoCCpICCpJaAgqaCqsaCgoKCgoKCgpSCgIKCtoKUgoKU")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("xml")]
public static partial class xml_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct fieldFlags {}
    internal partial struct fieldInfo {}
    internal partial struct parentStack {}
    internal partial struct printer {}
    internal partial struct stack {}
    internal partial struct typeInfo {}
    public partial interface Marshaler {}
    public partial interface MarshalerAttr {}
    public partial interface TokenReader {}
    public partial interface Unmarshaler {}
    public partial interface UnmarshalerAttr {}
    [GoLocalName("Token")] public partial interface ΔTokenᴅ {}
    public partial struct Attr {}
    public partial struct CharData {}
    public partial struct Comment {}
    [GoMemberRecord("nextToken", GoMemberFact.Descriptor, typeof(ΔTokenᴅ))] public partial struct Decoder {}
    public partial struct Directive {}
    public partial struct Encoder {}
    public partial struct EndElement {}
    public partial struct Name {}
    public partial struct ProcInst {}
    public partial struct StartElement {}
    public partial struct SyntaxError {}
    public partial struct TagPathError {}
    public partial struct UnmarshalError {}
    public partial struct UnsupportedTypeError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
