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
using static go.encoding.json_package;

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
[assembly: GoTypeAlias("Token", "object")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<InvalidUnmarshalError, error>(Pointer = true)]
[assembly: GoImplement<MarshalerError, error>(Pointer = true)]
[assembly: GoImplement<RawMessage, Marshaler>(Pointer = true)]
[assembly: GoImplement<RawMessage, Marshaler>]
[assembly: GoImplement<RawMessage, Unmarshaler>(Pointer = true)]
[assembly: GoImplement<SyntaxError, error>(Pointer = true)]
[assembly: GoImplement<UnmarshalTypeError, error>(Pointer = true)]
[assembly: GoImplement<UnsupportedTypeError, error>(Pointer = true)]
[assembly: GoImplement<UnsupportedValueError, error>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<jsonError, error>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Decoder))]
[assembly: GoFieldAccess(typeof(UnmarshalTypeError))]
[assembly: GoFieldAccess(typeof(decodeState))]
[assembly: GoFieldAccess(typeof(encodeState))]
[assembly: GoFieldAccess(typeof(errorContext))]
[assembly: GoFieldAccess(typeof(field))]
[assembly: GoFieldAccess(typeof(scanner))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("encoding/json/decode.go", "decode.cs", "ABjMAQBNCLKCgqaCABcsgoKUABAaggAQEoKCpoKUxqKCgqaCpoKClAAJDoCWkqiSABgskgAMEKKCgoKClJSqooK6soKUgoKClLaosqKCgoKCgoKCzLKCgpSCvMKigoKCgoKCuIIAAhYACgKSgpSClLSC6ILI6LS0tIKUlKzSlLaCgqaUtoKCppTIgpSCgsgACRLilLaCtoKktgAEEAAQGILKgoKUpoKCgoKCuIKmgtyCgpSClIKAgqSCgsqCgpSmytSCgoKClIKCgpSmpJSCgqakgoK0toKUgoK4goKUgriUgriCpqaClIKUgriCgoKmpoKUxoK41IKCgoKUgoKClIKmgoKCpty4yoKCgraCxsaCgqaCgoKmlIKUlIK4goKCgoKokpSCgoKUlJSCgpSCgoKClIKC3IKmgoKUlJSCupSCgoLKgpSClJSCpICC1oCC5raC3IKCgoKCgpSUlIK0goKCgpSCtIKCgoKUgrS2gsqClLiClIKUgqaqooKUgoKUxgALDuSUgpSCgoKUgoKCgpSClLSkgpSCgoKUlKaUyIKClJTopoKClJSClMa0gpTqgoKClJSUtIKClIKCgoKUtIKClLSClOqCgpSUlKaClIKUtIKCgpSCgpS2goKClLaCgoKUtoKCgpTGAAIQAAgClLSCtIK0pKiygpSCgqamgpSClIKmqLKClIKUlIK4goKCgoK4gpSClKamgpSClIKmrOSClJSkpqaCgpSmgpSCgpTcooKUgqKUtLS0lJSq0oKCprKClMqCgoKClIKClJSCpoKCuIKCgpSkgoKUlKSCgrSCgrSCgrSCgrSCgrSCgrSCgoKUgoKClIKCppTquIKCyIKCtg==", "", "", "835=Type/1/1/2,Field/1/1/2")]
[assembly: GoPositionMap("encoding/json/encode.go", "encode.cs", "AEKaAwCPAQKClIKClJTcsoKClIKCgpQAEB6CAA0UggAQGoIAEhaCgoKUrIAAFyqCgoKCgpSClAALELKCgpKUuILoksaClKyUxoIAEByCgpSmgoIACBKGkpSCpoKCqIKa6oKUgpSClIKmlKSkpKSkpKSkpKSkpPiCpqKCgpSCgoKUgoKCgoKUgriigoKClIKCgoKCgpSCuKKCgpSCgoKUgoKUpqKCgoKUgoKClKaCgoKCgqaCgoKCgqaCgoKCguqigoIABxCCgoKUgoKmgpSCgoKmgsiClqKCpoKUgpSCgoKCgpSCgpS4yoK4goKCypSmtoKC2oKCgtyCgoKCgqaCyqaCgoKUpqIADhqCgoKCpoKCgoKUlKaElIKCgpSUgrSClNiCggANDsKCgpSApoKClIKklrqCgpSUmIKClIKClIKixILKgraCtqKCgqaCgoKCggASEMKCgpSAyoiClIKkgqLElIKCgqaCAAkOgoKCgoKUlMaCggANDsKCgpSApoKClIKkgqLEgoIACQ6CgpTcooK2goKUgvympoKCgpSUAAcQgoKUgoKUgpSUpJSmooKCgoKCgpSClLS0tLS0AAgOpIKC3IKCgoKCgoIACBKCgoKCgpSUgoIAFjQACwSUgpbGlqakgoKUgoKUpoKCgoKClJS4lJSCgpSCgpSCgpSClLiCgpzagoKClAAIEqaCgpSClJSmypTGuIKUgoKU2oLKlLiCgty4gIKkgIKkgoKUlAAIEoKmgoKCgoKmgoKUgoK4gpiCgpSCgoKUgqYAAhIACQiClNySgpSCpoKClA==", "334-342:1;399-401:1;402-404:2;794-796:1;1196-1202:1;1204-1207:2;1209-1211:3;1213-1221:4;1245-1262:5;1293-1295:6", "758=encoding/json.structEncoder.encode;821=encoding/json.mapEncoder.encode;876=encoding/json.sliceEncoder.encode;897=encoding/json.arrayEncoder.encode;925=encoding/json.ptrEncoder.encode;944=encoding/json.condAddrEncoder.encode", "314=String/1/1/1,Error/1/1/2;811=IsZero/1/1/1,Invoke/1/1/1;966=UnsafePointer/1/1/3,Len/1/1/3;1343=Contains/1/2/5,Contains/2/2/6;1367=Elem/1/2/1,Kind/1/1/1,Elem/2/2/1,IsNil/2/2/1,Interface/1/1/2,IsZero/1/1/2", "869:35,971:35,1050:35", "368:21,369:13")]
[assembly: GoPositionMap("encoding/json/fold.go", "fold.cs", "AAwgpJKmgpSCgpSCgqaCgpSokoKCgpQ=")]
[assembly: GoPositionMap("encoding/json/indent.go", "indent.cs", "AAog8oKmpoKCgoKCpoKCgqaqwoKCgoKmooKCgoKCgoKUgqaCgpSClIKCgpSClKaClIKU1oKCgoKUAAosAA0CgoKCgqaigoKCgoKCgoKClIKUgoKCyoKCuKaCtIK0tJSUgpS0toKU")]
[assembly: GoPositionMap("encoding/json/scanner.go", "scanner.cs", "ABQwsoKC3NKCgoKCpoKUAAsUACBGzIKUgoKmtIKUACdYooKCgqrCgpSClIKClIKU6qKCgpSqooKCgoKU2IKokoKUgpTosoKUlIKkgqSCpIKkgqSCpIKkgpSCgpSosoKUgoKClOiygpSCgpQACgrCgpSCgpSCgpSClIKCgpSkgoKClIKClKSCgpSCgpSU7NKUlOiygoKUgoKUgpTospSCpIKU6LKCgqaosoKCpqiygoKmqLKCgqbosoKClIKClKrCgoKUqLKCgpSCgpTqwoKClKrCgpSCgpSqwoKClOrCgoKUrLKClOiygoKU6LKCgpTosoKClOiygoKU6LKCgpTosoKClOiygoKU6LKCgpTosoKClKiygoKUqqLIkoKCyKSClIK4gg==")]
[assembly: GoPositionMap("encoding/json/stream.go", "stream.cs", "ACVCwsqQmqCe8oKmgqaCuIKClILKppSqoqrClIKChsaCgoLKguqCgsaC3IKCgpSCpoKmgoK0pqaCgoKCqJKCgoK4gpTGgoKCpgAMHpIABBIACgKCpoKUgoIACBKUgoKCgpSUgpTcsoIAAhTyAAsSkoKU6LKClILGggANMsiUgoKUgpSCtIKClIKUgqSmgpSUpoKUtAALDoIABBwADQKCgoKUpIKUgoKCpoKUgoKCgqaClIKCgqaClIKCgoKmgpSCgqaCgoKUgoKClKaCgoKCgoKClIKUpoKUgoKUABAKgoKUtLS0tLSkqqKCpoKCgoKCgpSCpoKUvrI=")]
[assembly: GoPositionMap("encoding/json/tables.go", "tables.cs", "ABEiAGnUAQ==")]
[assembly: GoPositionMap("encoding/json/tags.go", "tags.cs", "AA8mooLMsoKUgoKCgoKm")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("json")]
public static partial class json_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct arrayEncoder {}
    internal partial struct condAddrEncoder {}
    internal partial struct decodeState {}
    internal partial struct encOpts {}
    internal partial struct encodeState {}
    internal partial struct encode_ptr {}
    internal partial struct errorContext {}
    internal partial struct field {}
    internal partial struct floatEncoder {}
    internal partial struct jsonError {}
    internal partial struct mapEncoder {}
    internal partial struct ptrEncoder {}
    internal partial struct reflectWithString {}
    internal partial struct scanner {}
    internal partial struct sliceEncoder {}
    internal partial struct structEncoder {}
    internal partial struct structFields {}
    internal partial struct tagOptions {}
    internal partial struct unquotedValue {}
    public partial interface Marshaler {}
    [GoLocalName("Token")] public partial interface Tokenᴅ {}
    public partial interface Unmarshaler {}
    public partial interface isZeroer {}
    public partial struct Decoder {}
    public partial struct Delim {}
    public partial struct Encoder {}
    public partial struct InvalidUTF8Error {}
    public partial struct InvalidUnmarshalError {}
    public partial struct MarshalerError {}
    public partial struct Number {}
    public partial struct RawMessage {}
    public partial struct SyntaxError {}
    public partial struct UnmarshalFieldError {}
    public partial struct UnmarshalTypeError {}
    public partial struct UnsupportedTypeError {}
    public partial struct UnsupportedValueError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(go.encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(json_package.Decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛDecoder;
[GoMethodHost(typeof(json_package.Delim)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛDelim;
[GoMethodHost(typeof(json_package.Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛEncoder;
[GoMethodHost(typeof(json_package.InvalidUTF8Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛInvalidUTF8Error;
[GoMethodHost(typeof(json_package.InvalidUnmarshalError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛInvalidUnmarshalError;
[GoMethodHost(typeof(json_package.MarshalerError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛMarshalerError;
[GoMethodHost(typeof(json_package.Number)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛNumber;
[GoMethodHost(typeof(json_package.RawMessage)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛRawMessage;
[GoMethodHost(typeof(json_package.SyntaxError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛSyntaxError;
[GoMethodHost(typeof(json_package.UnmarshalFieldError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛUnmarshalFieldError;
[GoMethodHost(typeof(json_package.UnmarshalTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛUnmarshalTypeError;
[GoMethodHost(typeof(json_package.UnsupportedTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛUnsupportedTypeError;
[GoMethodHost(typeof(json_package.UnsupportedValueError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛUnsupportedValueError;
[GoMethodHost(typeof(json_package.arrayEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛarrayEncoder;
[GoMethodHost(typeof(json_package.condAddrEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛcondAddrEncoder;
[GoMethodHost(typeof(json_package.decodeState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛdecodeState;
[GoMethodHost(typeof(json_package.encodeState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛencodeState;
[GoMethodHost(typeof(json_package.floatEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛfloatEncoder;
[GoMethodHost(typeof(json_package.mapEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛmapEncoder;
[GoMethodHost(typeof(json_package.ptrEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛptrEncoder;
[GoMethodHost(typeof(json_package.scanner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛscanner;
[GoMethodHost(typeof(json_package.sliceEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛsliceEncoder;
[GoMethodHost(typeof(json_package.structEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛstructEncoder;
[GoMethodHost(typeof(json_package.tagOptions)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jsonᴛtagOptions;
// </MethodHolders>
