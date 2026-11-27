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
using io = go.io_package;
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.json_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoPositionMap("encoding/json/decode.go", "decode.cs", "ABfMAQBNCLKCgqaCABUsgoKkAA0aggAOEoKCpoKUpqKCgqaCpoKClN6AlpKokgAULJIACBCigoKCgpSUqqKCurKClIKCgpS2qLKigoKCgoKCgsyygoKUgrzCooKCgoKCgriCAAIWAAoCkoKUgpS0guiCyOi0tLSClJSs0pS2goKmlLaCgqaUyIKUgpLIAAUS4pS2graCpLYAAhAAEBiCyoKClKaCgoKCgriCpoLcgoKUgpSCgIKkgoLKgoKUpqrUgoKCgpSCgoKUpqSUgoKmpIKCtLaClIKCuIKClIK4lIK4gqamgpSClIK4goKCpqaClKaCmNSCgoKClIKCgpSCpoKCgqbcuMqCgoK2gsbGgoKmgoKCppSClJSCuIKCkoKCqJKUgoKClJSUgoKUgoKCgpSCgtyCpoKClJSUgtqUgoKCyoKUgpSUgqSAgtaAgua2gtyCgoKCgoKUlJSCtIKCgoKUgrSCgoKClIK0toLKgpS4gpSClIKmqqKClIKClKYACQ7klIKUgoKClIKCgoKUgpS0pIKUgoKClJSmlMiCgpSU6KaCgpSUgpTGtIKU6oKCgpSUlLSCgpSCgoKClLSCgpS0gpTqgoKUlJSmgpSClLSCgoKUgoKUtoKCgpS2goKClLaCgoKUxgACEAAIApS0grSCtKSosoKUgoKmpoKUgpSCpqiygpSClJSCuIKCkoKCuIKUgpSmpoKUgpSCpqzkgpSkpKamgoKUpoKUgoKUvKKClIKilLS0tJSUqtKCgqaygpTKgoKCgpSCgpSUgqaCgriCgoKUpIKClJSkgoK0goK0goK0goK0goK0goK0goKClIKCgpSCgqaU6riCgsiCgrY=", "", "", "804=Type/1/1/2,Field/1/1/2")]
[assembly: GoPositionMap("encoding/json/encode.go", "encode.cs", "AEGaAwCPAQKClIKClJTcsoKClIKCgpQADh6CAAkUggAMGoIADhaCgoKUrIAAFSqCgoKCgpSClAAJELKCgpKUuILokqaClKyUpoIADhyCgpSmgoIACBKGkpSCpoKCqIKa6oKUgpSClIKmlKSkpKSkpKSkpKSkpLiCpqKCgpSCgoKUgoKCgoKUgriigoKClIKCgoKCgpSCuKKCgpSCgoKUgoKUpqKCgoKUgoKClKaCgoKCgqaCgoKCgqaCgoKCgsqigoIABxCCgoKUgoKmgpSCgoKmgqiClqKCpoKUgpSCgoKCgpSCgpS4yoK4goKCypSmtoKC2oKCgtyCgoKCgqaCyqaCgoKUpqIADBqCgoKCpoKCgoKUlKaElIKCgpSUgrSClLiCgv7CgoKUgKaCgpSCpJa6goKUlJiCgpSCgpSCoqSCyoK2graigoKmgoKCgoIADBDCgoKUgMqIgpSCpIKipJSCgoKmgv6CgoKCgpSUpoKC/sKCgpSApoKClIKkgqKkgoL+goKUvKKCtoKClIL8pqaCgoKUlAAHEIKClIKClIKUlKSUpqKCgoKCgoKUgpS0tLS0tAAIDqSCgtyCgoKCgoKCAAgSgoKCgoKUlIKCABY0AAsElIKWxpampIKClIKClKaCgoKCgpSUuJSUgoKUgoKUgoKUgpS4goKc2oKCgpQACBKmgoKUgpSUpsqUxriClIKClNqCypS4goLcuICCpICCpIKClJQACBKCpoKCgoKCpoKClIKCuIKYgoKUgoKClIKmAAISAAkIgpTckoKUgqaCgpQ=", "334-342:1;399-401:1;402-404:2;794-796:1;1196-1202:1;1204-1207:2;1209-1211:3;1213-1221:4;1245-1262:5;1293-1295:6", "758=encoding/json.structEncoder.encode;821=encoding/json.mapEncoder.encode;876=encoding/json.sliceEncoder.encode;897=encoding/json.arrayEncoder.encode;925=encoding/json.ptrEncoder.encode;944=encoding/json.condAddrEncoder.encode", "299=String/1/1/1,Error/1/1/2;776=IsZero/1/1/1,Invoke/1/1/1;915=UnsafePointer/1/1/3,Len/1/1/3;1274=Contains/1/2/5,Contains/2/2/6;1298=Elem/1/2/1,Kind/1/1/1,Elem/2/2/1,IsNil/2/2/1,Interface/1/1/2,IsZero/1/1/2", "826:35,920:35,987:35", "349:62,350:13")]
[assembly: GoPositionMap("encoding/json/fold.go", "fold.cs", "AAwgpJKmgpSCgpSCgqaCgpSokoKCgpQ=")]
[assembly: GoPositionMap("encoding/json/indent.go", "indent.cs", "AAog8oKmpoKCgoKCpoKCgqaqwoKCgoKmooKCgoKCgoKUgqaCgpSClIKCgpSClKaClIKU1oKCgoKUAAosAA0CgoKCgqaigoKCgoKCgoKClIKUgoKCyoKCuKaCtIK0tJSUgpS0toKU")]
[assembly: GoPositionMap("encoding/json/scanner.go", "scanner.cs", "ABMwsoKC3NKCgoKCpoKUAAkUAB5GzIKUgoKmtIKUACVYooKCgqrCgpSClIKClIKU6qKCgpSqooKCgoKUuIKokoKUgpTosoKUlIKkgqSCpIKkgqSCpIKkgpSCgpSosoKUgoKClOiygpSCgpQACgrCgpSCgpSCgpSClIKCgpSkgoKClIKClKSCgpSCgpSU7NKUlOiygoKUgoKUgpTospSCpIKU6LKCgqaosoKCpqiygoKmqLKCgqbosoKClIKClKrCgoKUqLKCgpSCgpTqwoKClKrCgpSCgpSqwoKClOrCgoKUrLKClOiygoKU6LKCgpTosoKClOiygoKU6LKCgpTosoKClOiygoKU6LKCgpTosoKClKiygoKUqqKokoKCqKSClIK4gg==")]
[assembly: GoPositionMap("encoding/json/stream.go", "stream.cs", "ACRCwqqQmqCe8oKmgqaCuIKClILKppSqoqrClIKChsaCgoLKguqCgsaC3IKCgpSCpoKmgoK0pqaCgoKCqJKCgoK4gpSmgoKCpgAMHpIAAhIACgKCpoKUgoIACBKUgoKCgpSUgpTcsoIAAhTyAAcSkoKU6LKClIKmggALMsiUgoKUgpSCtIKClIKUgqSmgpSUpoKUtP6CAAIcAA0CgoKClJSClIKCgqaClIKCgoKmgpSCgoKmgpSCgoKCpoKUgoKmgoKClIKCgpSmgoKCgoKCgpSClKaClIKClAAQCoKClLS0tLS0pKqigqaCgoKCgoKUgqaClL6y")]
[assembly: GoPositionMap("encoding/json/tables.go", "tables.cs", "ABEiAGnUAQ==")]
[assembly: GoPositionMap("encoding/json/tags.go", "tags.cs", "AA4mooKssoKUgoKCgoKm")]
// </GoSourcePositionMaps>

namespace go.encoding;

using static go.encoding.json_package;
using io = go.io_package;
using reflect = go.reflect_package;

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
[GoMethodHost(typeof(json_package.Decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛDecoder;
[GoMethodHost(typeof(json_package.Delim)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛDelim;
[GoMethodHost(typeof(json_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛEncoder;
[GoMethodHost(typeof(json_package.InvalidUTF8Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛInvalidUTF8Error;
[GoMethodHost(typeof(json_package.InvalidUnmarshalError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛInvalidUnmarshalError;
[GoMethodHost(typeof(json_package.MarshalerError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛMarshalerError;
[GoMethodHost(typeof(json_package.Number)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛNumber;
[GoMethodHost(typeof(json_package.RawMessage)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛRawMessage;
[GoMethodHost(typeof(json_package.SyntaxError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛSyntaxError;
[GoMethodHost(typeof(json_package.UnmarshalFieldError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛUnmarshalFieldError;
[GoMethodHost(typeof(json_package.UnmarshalTypeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛUnmarshalTypeError;
[GoMethodHost(typeof(json_package.UnsupportedTypeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛUnsupportedTypeError;
[GoMethodHost(typeof(json_package.UnsupportedValueError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛUnsupportedValueError;
[GoMethodHost(typeof(json_package.arrayEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛarrayEncoder;
[GoMethodHost(typeof(json_package.condAddrEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛcondAddrEncoder;
[GoMethodHost(typeof(json_package.decodeState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛdecodeState;
[GoMethodHost(typeof(json_package.encodeState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛencodeState;
[GoMethodHost(typeof(json_package.floatEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛfloatEncoder;
[GoMethodHost(typeof(json_package.mapEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛmapEncoder;
[GoMethodHost(typeof(json_package.ptrEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛptrEncoder;
[GoMethodHost(typeof(json_package.scanner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛscanner;
[GoMethodHost(typeof(json_package.sliceEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛsliceEncoder;
[GoMethodHost(typeof(json_package.structEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛstructEncoder;
[GoMethodHost(typeof(json_package.tagOptions)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class jsonᴛtagOptions;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class jsonᴛDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void UseNumber(this ref Decoder dec) => json_package.UseNumber⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void DisallowUnknownFields(this ref Decoder dec) => json_package.DisallowUnknownFields⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Decode(this ж<Decoder> Ꮡdec, any v) => json_package.Decode⁔(Ꮡdec, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.Reader Buffered(this ref Decoder dec) => json_package.Buffered⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) readValue(this ж<Decoder> Ꮡdec) => json_package.readValue⁔(Ꮡdec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error refill(this ref Decoder dec) => json_package.refill⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error tokenPrepareForDecode(this ref Decoder dec) => json_package.tokenPrepareForDecode⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tokenValueAllowed(this ref Decoder dec) => json_package.tokenValueAllowed⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tokenValueEnd(this ref Decoder dec) => json_package.tokenValueEnd⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Token, error) Token(this ж<Decoder> Ꮡdec) => json_package.Token⁔(Ꮡdec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Token, error) tokenError(this ref Decoder dec, byte c) => json_package.tokenError⁔(ref dec, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool More(this ref Decoder dec) => json_package.More⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) peek(this ref Decoder dec) => json_package.peek⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 InputOffset(this ref Decoder dec) => json_package.InputOffset⁔(ref dec);
}
partial class jsonᴛDelim {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Delim d) => json_package.String⁔(d);
}
partial class jsonᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Encode(this ж<Encoder> Ꮡenc, any v) => json_package.Encode⁔(Ꮡenc, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetIndent(this ref Encoder enc, @string prefix, @string indent) => json_package.SetIndent⁔(ref enc, prefix, indent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetEscapeHTML(this ref Encoder enc, bool on) => json_package.SetEscapeHTML⁔(ref enc, on);
}
partial class jsonᴛInvalidUTF8Error {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref InvalidUTF8Error e) => json_package.Error⁔(ref e);
}
partial class jsonᴛInvalidUnmarshalError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref InvalidUnmarshalError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛMarshalerError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref MarshalerError e) => json_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref MarshalerError e) => json_package.Unwrap⁔(ref e);
}
partial class jsonᴛNumber {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Number n) => json_package.String⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64, error) Float64(this Number n) => json_package.Float64⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Int64(this Number n) => json_package.Int64⁔(n);
}
partial class jsonᴛRawMessage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this RawMessage m) => json_package.MarshalJSON⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalJSON(this ж<RawMessage> Ꮡm, slice<byte> data) => json_package.UnmarshalJSON⁔(Ꮡm, data);
}
partial class jsonᴛSyntaxError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref SyntaxError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛUnmarshalFieldError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnmarshalFieldError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛUnmarshalTypeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnmarshalTypeError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛUnsupportedTypeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnsupportedTypeError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛUnsupportedValueError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnsupportedValueError e) => json_package.Error⁔(ref e);
}
partial class jsonᴛarrayEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this arrayEncoder ae, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(ae, Ꮡe, v, opts);
}
partial class jsonᴛcondAddrEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this condAddrEncoder ce, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(ce, Ꮡe, v, opts);
}
partial class jsonᴛdecodeState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unmarshal(this ж<decodeState> Ꮡd, any v) => json_package.unmarshal⁔(Ꮡd, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint readIndex(this ref decodeState d) => json_package.readIndex⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<decodeState> init(this ж<decodeState> Ꮡd, slice<byte> data) => json_package.init⁔(Ꮡd, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void saveError(this ref decodeState d, error err) => json_package.saveError⁔(ref d, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error addErrorContext(this ref decodeState d, error err) => json_package.addErrorContext⁔(ref d, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skip(this ж<decodeState> Ꮡd) => json_package.skip⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanNext(this ж<decodeState> Ꮡd) => json_package.scanNext⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanWhile(this ж<decodeState> Ꮡd, nint op) => json_package.scanWhile⁔(Ꮡd, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rescanLiteral(this ж<decodeState> Ꮡd) => json_package.rescanLiteral⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error value(this ж<decodeState> Ꮡd, reflect.Value v) => json_package.value⁔(Ꮡd, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any valueQuoted(this ж<decodeState> Ꮡd) => json_package.valueQuoted⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Δarray(this ж<decodeState> Ꮡd, reflect.Value v) => json_package.Δarray⁔(Ꮡd, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error @object(this ж<decodeState> Ꮡd, reflect.Value v) => json_package.object⁔(Ꮡd, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (any, error) convertNumber(this ref decodeState d, @string s) => json_package.convertNumber⁔(ref d, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error literalStore(this ref decodeState d, slice<byte> item, reflect.Value v, bool fromQuoted) => json_package.literalStore⁔(ref d, item, v, fromQuoted);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any /*val*/ valueInterface(this ж<decodeState> Ꮡd) => json_package.valueInterface⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<any> arrayInterface(this ж<decodeState> Ꮡd) => json_package.arrayInterface⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static map<@string, any> objectInterface(this ж<decodeState> Ꮡd) => json_package.objectInterface⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any literalInterface(this ж<decodeState> Ꮡd) => json_package.literalInterface⁔(Ꮡd);
}
partial class jsonᴛencodeState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ marshal(this ж<encodeState> Ꮡe, any v, encOpts opts) => json_package.marshal⁔(Ꮡe, v, opts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref encodeState e, error err) => json_package.error⁔(ref e, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reflectValue(this ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.reflectValue⁔(Ꮡe, v, opts);
}
partial class jsonᴛfloatEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this floatEncoder bits, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(bits, Ꮡe, v, opts);
}
partial class jsonᴛmapEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this mapEncoder me, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(me, Ꮡe, v, opts);
}
partial class jsonᴛptrEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ptrEncoder pe, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(pe, Ꮡe, v, opts);
}
partial class jsonᴛscanner {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref scanner s) => json_package.reset⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint eof(this ж<scanner> Ꮡs) => json_package.eof⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint pushParseState(this ref scanner s, byte c, nint newParseState, nint successState) => json_package.pushParseState⁔(ref s, c, newParseState, successState);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void popParseState(this ref scanner s) => json_package.popParseState⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint error(this ref scanner s, byte c, @string context) => json_package.error⁔(ref s, c, context);
}
partial class jsonᴛsliceEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this sliceEncoder se, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(se, Ꮡe, v, opts);
}
partial class jsonᴛstructEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this structEncoder se, ж<encodeState> Ꮡe, reflect.Value v, encOpts opts) => json_package.encode⁔(se, Ꮡe, v, opts);
}
partial class jsonᴛtagOptions {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Contains(this tagOptions o, @string optionName) => json_package.Contains⁔(o, optionName);
}
// </MethodForwarders>
