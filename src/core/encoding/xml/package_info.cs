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
[assembly: GoTypeAlias("Token", "object")]
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
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Attr))]
[assembly: GoFieldAccess(typeof(Encoder))]
[assembly: GoFieldAccess(typeof(EndElement))]
[assembly: GoFieldAccess(typeof(StartElement))]
[assembly: GoFieldAccess(typeof(fieldInfo))]
[assembly: GoFieldAccess(typeof(printer))]
[assembly: GoFieldAccess(typeof(stack))]
[assembly: GoFieldAccess(typeof(typeInfo))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("encoding/xml/marshal.go", "marshal.cs", "ABikAQA4AoKCgIKkgIKkACVSsoKCgoCCpICCpAAHEpKCgsyyggACEgAIAoKClAACFAAKAoKClMiCggADHgAMBIKkgILWgILWtIKUgoKCyIKUgpSClIKCgoKUtIKUgoLElsqiqoKUgoLqgti0gpTGgpS2yqKssgAdLMKC7oK4goLKgoKUgtyClJSCgoLKgpSCgoKClJSokoKmgqaCgoKCgpS84oKmgpSC3IKClKaCpoKCpoKCgoLcgoKmgoKCgtyCgoKmpoKCAAIS9IKCgoKCgpSCgriClIKClJSClLiCgoKUlIKmgqaCgsqGlIKmgpSCgoKClKaCpoKmqLKCgoKClIKUuIKCgoKCgpSClMqCgoKClIK4goKCgoKClILclIKUyIKCgoKmpoKCgqaCgpSClILK0qaCgoKCgoKCuJTItoKUgoK4gpSCqMKClIKClILowoKmgpSCgpSCgoK4goKClIKCgpSCgoKUgqaCgpSClIKClJSUgoKCgoKCpoKUpKSkpKSClpKClIKUpIKmlMacwoKClJTGsoKCgoKUgqamlIKClICCpIKCgoKUgpSmgoKCgoKClIKUyoKClICC1oCC1oCC1oCC1oCC1oKCyLaAgqSCgoKUgpSCgoKClIKCgoLGgoKCgsakgpSUlIK2goKUgrSC2ICCpIKCgtqAgraC6LKClIKUqLKClIKUqJKClIKUrLKClIKClIKUqJKCpoKClIKCgoKUlIKUlIKUgoKmgoIADBiygoKCpoKCpoKokoKCpoIADBKCxoKUrJQ=")]
[assembly: GoPositionMap("encoding/xml/read.go", "read.cs", "ABKKAgBzAsqiAAgOwoKCpoKUAAkMACJEkoKClMoACASUgoKApIKCgqaCptyygoKCgoKUlILGtLaokoKClJSmgqaCgoKC3KaCpoKCgoLKpoKCpoKClKaCgqbGzrbCgqaCgoKClJKC7oKCgriCgpSmpoK4goKCgsqCgriCgoKCygALHKTcpoKUgsqCgqaCgpSmtoKCgqaCgoK4goKClIKCgpSUlIKCyoKCgoKClIKCgpTIgsiCgoKC3IKClILIgsiCyIKCgoKUAAoSgoKCgpSCgpSkgoKCgpSCgoK4goLagoKCpraCyIL6goKClLiCgoKCgpTKgqaApLTGgKS0gtjGgpSCgpS42KSCgpSCgpS0goKUgoKUtIKClIKClLSCgpSCgpS0tJSUpAAEEAAKAoKCgoKClIKCppSUuLiCxpTKgoKCgpSkgoKUgoLYAAQU0oKCgoKUlLSClA==", "212-212:1", "", "", "", "234:21,235:13")]
[assembly: GoPositionMap("encoding/xml/typeinfo.go", "typeinfo.cs", "ADBi2KKCpoKCgoKCgriCgoKUgoKClIKUooKCpriCgqaCgriAgtqCqJKmgoK4goKUgoKktLS0tLS02oKktILYpIKUgpSC3IK4uIKmuIKUlLiCgpSClIKCgpTcgoKCgrissoKUgpSCgoKUgoK4lAACFAAKAoKEkoKClIKUgoKCpoKClIKCpoLqgoLKgoLKgoKCgoLcgoKClIIADBaCAAka0oKCgoKCgpSUppQ=", "", "", "195=Get/1/1/1;202=Get/1/1/1")]
[assembly: GoPositionMap("encoding/xml/xml.go", "xml.cs", "ACY+ggAbOpKCgoKokgAQHIAACQ6AAAsSkoIAChCAtpKUpKSkpJQAV8ABstyCqKSClO4ACDoAGgKCgoKUgoKUgoKUuJSCgoKm7IKCgoKUlIKCuIKCgpTGgpSkAA0Y0pSkpLSUgoKCuMqClAAWKoKCgpSUgoKClIKmgoKCgpSCgpSs6IKCuIKUgoKUlIKCqqKClIKokoKqooKCgqiSAAISAAgCgoKUgqSCgoKClIKkgoKUhKa4goKClLiqooKUgpSCgpSmxrqygpQAHAaCgpSClLiCpoKCppSCgoKUpoKUlpKCgpSUgoKUgoKUqJKCgpSUgoKCgoKUgoKUlIKUgoKCgoKUgoKCgpSCgoKUgpSmuIKUpoKUgoKmgpKCgpSCgoKUlJSUgoK4goKUgoK4goKU3IKCgoKCgpSClIKClPy2yIKCgpSCgpSCypaigoKUgpTutriUqoKClKaCgoKClIKCgpSCgpSUgpSUgoKClJSCgpSCgoKUgpSCgoKUlJSCgpTmgoKCpoKmgoKmgoKCgoKmhJSCpqiSgoKClMaCAAQS4oKUgoKUgoKUgqaCgpSCrLKssqqigoKUrvKCgqaokoKUgsYAExjCkoKCgoKCgoKClJTcgoKClIK4goKClIKUgpTcgoKCgoKClIKCgpSCgoKCgqaChoKCpoKUgoKCgoK4goKCpoKUgpSCgoKCgoKCgtyCgoKClIKClIKClIK4goKmprSCpoKCgoKClIKCgrjMsgAEFNKCgpSCgoKUgpSssoKCuIKCgpSs0oKClIKClJSCgpSCgpSUxoKsgoKUgoKUgpSCgoKClIKmpoKClIKClIKUgoKCgpSCpgAGEADCAYYDAHjyAaQAjAKWBJQAEyqCgoKCgoKCgpqirLKCgoKCgpS0tLS0tLSClLS0goKUpIKUgpSUgsqigoKCgoKUtLS0tLS0tLSCgpSkgoKUzLKogoKaooKUgqaCgoKmgpSClKaCpoKqxoKCgoKCgoKClIKCgqaClIKClA==")]
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
    [GoLocalName("Token")] public partial interface Tokenᴅ {}
    public partial interface Unmarshaler {}
    public partial interface UnmarshalerAttr {}
    public partial struct Attr {}
    public partial struct CharData {}
    public partial struct Comment {}
    [GoMemberRecord("nextToken", GoMemberFact.Descriptor, typeof(Tokenᴅ))] public partial struct Decoder {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(xml_package.CharData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛCharData;
[GoMethodHost(typeof(xml_package.Comment)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛComment;
[GoMethodHost(typeof(xml_package.Decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛDecoder;
[GoMethodHost(typeof(xml_package.Directive)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛDirective;
[GoMethodHost(typeof(xml_package.Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛEncoder;
[GoMethodHost(typeof(xml_package.ProcInst)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛProcInst;
[GoMethodHost(typeof(xml_package.StartElement)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛStartElement;
[GoMethodHost(typeof(xml_package.SyntaxError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛSyntaxError;
[GoMethodHost(typeof(xml_package.TagPathError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛTagPathError;
[GoMethodHost(typeof(xml_package.UnmarshalError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛUnmarshalError;
[GoMethodHost(typeof(xml_package.UnsupportedTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛUnsupportedTypeError;
[GoMethodHost(typeof(xml_package.fieldInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛfieldInfo;
[GoMethodHost(typeof(xml_package.parentStack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛparentStack;
[GoMethodHost(typeof(xml_package.printer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class xmlᴛprinter;
// </MethodHolders>
