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
[assembly: GoPositionMap("encoding/xml/typeinfo.go", "typeinfo.cs", "ADBi2KKCpoKCgoKCgriCgoKUgoKClIKUooKCpriCgqaCgriAgtqCqJKmgoK4goKUgoKUtLS0tLS02oKktILYpIKUgpSC3IK4uIKmuIKUlLiCgpSClIKCgpTcgoKCgrissoKUgpSCgoKUgoK4lAACFAAKAoKEkoKClIKUgoKCpoKClIKCpoLqgoLKgoLKgoKCgoLcgoKClIIADBaCAAka0oKCgoKCgpSUppQ=", "", "", "194=Get/1/1/1;201=Get/1/1/1")]
[assembly: GoPositionMap("encoding/xml/xml.go", "xml.cs", "ACY+ggAbOpKCgoKokgAQHIAACQ6AAAsSkoIAChCAtpKUpKSkpJQAV8ABstyCqKSClO4ACDoAGgKCgoKUgoKUgoKUuJSCgoKm7IKCgoKUlIKCuIKCgpTGgpSkAA0Y0pSkpLSUgoKCuMqClAAWKoKCgpSUgoKClIKmgoKCgpSCgpSs6IKCuIKUgoKUlIKCqqKClIKokoKqooKCgqiSAAISAAgCgoKUgqSCgoKClIKkgoKUhKa4goKClLiqooKUgpSCgpSmxrqygpQAHAaCgpSClLiCpoKCppSCgoKUpoKUlpKCgpSUgoKUgoKUqJKCgpSUgoKCgoKUgoKUlIKUgoKCgoKUgoKCgpSCgoKUgpSmuIKUpoKUgoKmgpKCgpSCgoKUlJSUgoK4goKUgoK4goKU3IKCgoKCgpSClIKClPy2yIKCgpSCgpSCypaigoKUgpTutriUqoKClKaCgoKClIKCgpSCgpSUgpSUgoKClJSCgpSCgoKUgpSCgoKUlJSCgpTmgoKCpoKmgoKmgoKCgoKmhJSCpqiSgoKClMaCAAQS4oKUgoKUgoKUgqaCgpSCrLKssqqigoKUrvKCgqaokoKUgsYAExjCkoKCgoKCgoKClJTcgoKClIK4goKClIKUgpTcgoKCgoKClIKCgpSCgoKCgqaChoKCpoKUgoKCgoK4goKCpoKUgpSCgoKCgoKCgtyCgoKClIKClIKClIK4goKmprSCpoKCgoKClIKCgrjMsgAEFNKCgpSCgoKUgpSssoKCuIKCgpSs0oKClIKClJSCgpSCgpSUxoKsgoKUgoKUgpSCgoKClIKmpoKClIKClIKUgoKCgpSCpgAGEADCAYYDAHjyAaQAjAKWBJQAEyqCgoKCgoKCgpqirLKCgoKCgpS0tLS0tLSClLS0goKUpIKUgpSUgsqigoKCgoKUtLS0tLS0tLSCgpSkgoKUzLKogoKaooKUgqaCgoKmgpSClKaCpoKqxoKCgoKCgoKClIKCgqaClIKClA==")]
// </GoSourcePositionMaps>

namespace go.encoding;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bufio = bufio_package;
using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using static xml_package;
using runtime = runtime_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class xmlᴛEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Indent(this ref Encoder enc, @string prefix, @string indent) => xml_package.Indentᴍ(ref enc, prefix, indent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Encode(this ж<Encoder> Ꮡenc, any v) => xml_package.Encodeᴍ(Ꮡenc, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error EncodeElement(this ж<Encoder> Ꮡenc, any v, StartElement startʗp) => xml_package.EncodeElementᴍ(Ꮡenc, v, startʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error EncodeToken(this ж<Encoder> Ꮡenc, Token t) => xml_package.EncodeTokenᴍ(Ꮡenc, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Flush(this ref Encoder enc) => xml_package.Flushᴍ(ref enc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Encoder enc) => xml_package.Closeᴍ(ref enc);
}
partial class xmlᴛprinter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string createAttrPrefix(this ж<printer> Ꮡp, @string url) => xml_package.createAttrPrefixᴍ(Ꮡp, url);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void deleteAttrPrefix(this ref printer p, @string prefix) => xml_package.deleteAttrPrefixᴍ(ref p, prefix);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markPrefix(this ref printer p) => xml_package.markPrefixᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void popPrefix(this ref printer p) => xml_package.popPrefixᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error marshalValue(this ж<printer> Ꮡp, reflect.Value val, ж<fieldInfo> Ꮡfinfo, ж<StartElement> ᏑstartTemplate) => xml_package.marshalValueᴍ(Ꮡp, val, Ꮡfinfo, ᏑstartTemplate);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error marshalAttr(this ref printer p, ж<StartElement> Ꮡstart, Name name, reflect.Value val) => xml_package.marshalAttrᴍ(ref p, Ꮡstart, name, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error marshalInterface(this ref printer p, Marshaler val, StartElement start) => xml_package.marshalInterfaceᴍ(ref p, val, start);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error marshalTextInterface(this ж<printer> Ꮡp, encoding.TextMarshaler val, StartElement startʗp) => xml_package.marshalTextInterfaceᴍ(Ꮡp, val, startʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeStart(this ж<printer> Ꮡp, ж<StartElement> Ꮡstart) => xml_package.writeStartᴍ(Ꮡp, Ꮡstart);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeEnd(this ref printer p, Name name) => xml_package.writeEndᴍ(ref p, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, slice<byte>, error) marshalSimple(this ref printer p, reflect.Type typ, reflect.Value val) => xml_package.marshalSimpleᴍ(ref p, typ, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error marshalStruct(this ж<printer> Ꮡp, ж<typeInfo> Ꮡtinfo, reflect.Value val) => xml_package.marshalStructᴍ(Ꮡp, Ꮡtinfo, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref printer p, slice<byte> b) => xml_package.Writeᴍ(ref p, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref printer p, @string s) => xml_package.WriteStringᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error WriteByte(this ref printer p, byte c) => xml_package.WriteByteᴍ(ref p, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref printer p) => xml_package.Closeᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error cachedWriteError(this ref printer p) => xml_package.cachedWriteErrorᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeIndent(this ref printer p, nint depthDelta) => xml_package.writeIndentᴍ(ref p, depthDelta);
}
partial class xmlᴛparentStack {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error trim(this ref parentStack s, slice<@string> parents) => xml_package.trimᴍ(ref s, parents);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error push(this ref parentStack s, slice<@string> parents) => xml_package.pushᴍ(ref s, parents);
}
partial class xmlᴛUnsupportedTypeError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref UnsupportedTypeError e) => xml_package.Errorᴍ(ref e);
}
partial class xmlᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Decode(this ж<Decoder> Ꮡd, any v) => xml_package.Decodeᴍ(Ꮡd, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error DecodeElement(this ж<Decoder> Ꮡd, any v, ж<StartElement> start) => xml_package.DecodeElementᴍ(Ꮡd, v, start);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error unmarshalInterface(this ж<Decoder> Ꮡd, Unmarshaler val, ж<StartElement> Ꮡstart) => xml_package.unmarshalInterfaceᴍ(Ꮡd, val, Ꮡstart);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error unmarshalTextInterface(this ref Decoder d, encoding.TextUnmarshaler val) => xml_package.unmarshalTextInterfaceᴍ(ref d, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error unmarshalAttr(this ref Decoder d, reflect.Value val, Attr attr) => xml_package.unmarshalAttrᴍ(ref d, val, attr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error unmarshal(this ж<Decoder> Ꮡd, reflect.Value val, ж<StartElement> Ꮡstart) => xml_package.unmarshalᴍ(Ꮡd, val, Ꮡstart);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool consumed, error err) unmarshalPath(this ж<Decoder> Ꮡd, ж<typeInfo> Ꮡtinfo, reflect.Value sv, slice<@string> parents, ж<StartElement> Ꮡstart) => xml_package.unmarshalPathᴍ(Ꮡd, Ꮡtinfo, sv, parents, Ꮡstart);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Skip(this ref Decoder d) => xml_package.Skipᴍ(ref d);
}
partial class xmlᴛUnmarshalError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this UnmarshalError e) => xml_package.Errorᴍ(e);
}
partial class xmlᴛTagPathError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref TagPathError e) => xml_package.Errorᴍ(ref e);
}
partial class xmlᴛfieldInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value value(this ref fieldInfo finfo, reflect.Value v, bool shouldInitNilPointers) => xml_package.valueᴍ(ref finfo, v, shouldInitNilPointers);
}
partial class xmlᴛSyntaxError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref SyntaxError e) => xml_package.Errorᴍ(ref e);
}
partial class xmlᴛStartElement {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static StartElement Copy(this StartElement e) => xml_package.Copyᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static EndElement End(this StartElement e) => xml_package.Endᴍ(e);
}
partial class xmlᴛCharData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static CharData Copy(this CharData c) => xml_package.Copyᴍ(c);
}
partial class xmlᴛComment {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Comment Copy(this Comment c) => xml_package.Copyᴍ(c);
}
partial class xmlᴛProcInst {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ProcInst Copy(this ProcInst p) => xml_package.Copyᴍ(p);
}
partial class xmlᴛDirective {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Directive Copy(this Directive d) => xml_package.Copyᴍ(d);
}
partial class xmlᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Token, error) Token(this ref Decoder d) => xml_package.Tokenᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void translate(this ref Decoder d, ж<Name> Ꮡn, bool isElementName) => xml_package.translateᴍ(ref d, Ꮡn, isElementName);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void switchToReader(this ref Decoder d, io.Reader r) => xml_package.switchToReaderᴍ(ref d, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<stack> push(this ref Decoder d, nint kind) => xml_package.pushᴍ(ref d, kind);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<stack> pop(this ref Decoder d) => xml_package.popᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushEOF(this ref Decoder d) => xml_package.pushEOFᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool popEOF(this ref Decoder d) => xml_package.popEOFᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushElement(this ref Decoder d, Name name) => xml_package.pushElementᴍ(ref d, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushNs(this ref Decoder d, @string local, @string url, bool ok) => xml_package.pushNsᴍ(ref d, local, url, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error syntaxError(this ref Decoder d, @string msg) => xml_package.syntaxErrorᴍ(ref d, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool popElement(this ref Decoder d, ж<EndElement> Ꮡt) => xml_package.popElementᴍ(ref d, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Token, bool) autoClose(this ref Decoder d, Token t) => xml_package.autoCloseᴍ(ref d, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Token, error) RawToken(this ref Decoder d) => xml_package.RawTokenᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Token, error) rawToken(this ref Decoder d) => xml_package.rawTokenᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> attrval(this ref Decoder d) => xml_package.attrvalᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void space(this ref Decoder d) => xml_package.spaceᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte b, bool ok) getc(this ref Decoder d) => xml_package.getcᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 InputOffset(this ref Decoder d) => xml_package.InputOffsetᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint line, nint column) InputPos(this ref Decoder d) => xml_package.InputPosᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint savedOffset(this ref Decoder d) => xml_package.savedOffsetᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte b, bool ok) mustgetc(this ref Decoder d) => xml_package.mustgetcᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ungetc(this ref Decoder d, byte b) => xml_package.ungetcᴍ(ref d, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> text(this ref Decoder d, nint quote, bool cdata) => xml_package.textᴍ(ref d, quote, cdata);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Name name, bool ok) nsname(this ref Decoder d) => xml_package.nsnameᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string s, bool ok) name(this ref Decoder d) => xml_package.nameᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*ok*/ readName(this ref Decoder d) => xml_package.readNameᴍ(ref d);
}
partial class xmlᴛprinter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void EscapeString(this ref printer p, @string s) => xml_package.EscapeStringᴍ(ref p, s);
}
// </GlyphForwarders>
