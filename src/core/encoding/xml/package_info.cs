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
using encoding = go.encoding_package;
using io = go.io_package;
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.xml_package;
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
[assembly: GoPositionMap("encoding/xml/marshal.go", "marshal.cs", "ABekAQA4AoKCgIKkgIKkACVSsoKCgoCCpICCpAAHEpKCgqyyggACEgAIAoKClAACFAAKAoKClKiCggABHgAMBIKkgILWgILWtIKUgoKCyIKUgpSClIKCgoKUtIKUgoLElqqiqoKUgoLqgti0gpTGgpS2qqKssgAZLMKC7oK4goLKgoKUgtyClJSCgoLKgpSCgoKClJSokoKmgqaCgoKCgpS84oKmgpSC3IKClKaCpoKCpoKCgoLcgoKmgoKCgtyCgoKmpoKCAAIS9IKCgoKCgpSCgriClIKClJSClLiCgoKUlIKmgqaCgsqGlIKmgpSCgoKClKaCpoKmqLKCgoKClIKUuIKCgoKCgpSClMqCgoKClIK4goKCgoKClILclIKUyIKCgoKmpoKCgqaCgpSClIKq0qaCgoKCgoKCuJSotoKUgoK4gpSCqMKClIKClILowoKmgpSCgpSCgoK4goKClIKCgpSCgoKUgqaCgpSClIKClJSUgoKCgoKCpoKUpKSkpKSClpKClIKUpIKmlKacwoKClJSmsoKCgoKUgqamlIKClICCpIKCgoKUgpSmgoKCgoKClIKUyoKClICC1oCC1oCC1oCC1oCC1oKCyLaAgqSCgoKUgpSCgoKClIKCgoLGgoKCgsakgpSUlIK2goKUgrSC2ICCpIKCgtqAgraC6LKClIKUqLKClIKUqJKClIKUrLKClIKClIKUqJKCpoKClIKCgoKUlIKUlIKUgoKmgoIACBiygoKCpoKCpoKokoKCpoIACBKCpoKUrJQ=")]
[assembly: GoPositionMap("encoding/xml/read.go", "read.cs", "ABGKAgBzAqqiAAgOwoKCpoKU3AAgRJKCgpSqAAgElIKCgKSCgoKmgqbcsoKCgoKClJSCxrS2qJKCgpSUpoKmgoKCgtymgqaCgoKCyqaCgqaCgpSmgoKmps6WwoKmgoKCgpSSgu6CgoK4goKUpqaCuIKCgoLKgoK4goKCgsoACxyk3KaClILKgoKmgoKUpraCgoKmgoKCuIKCgpSCgoKUlJSCgsqCgoKCgpSCgoKUyILIgoKCgtyCgpSCyILIgsiCgoKClAAKEoKCgoKUgoKUpIKCgoKUgoKCuIKC2oKCgqa2gsiC+oKCgpS4goKCgoKUyoKmgKS0xoCktILYpoKUgoKUuNikgoKUgoKUtIKClIKClLSCgpSCgpS0goKUgoKUtLSUlKQAAhAACgKCgoKCgpSCgqaUlLi4gsaUyoKCgoKUpIKClIKC2AAEFNKCgoKClJS0gpQ=", "212-212:1", "", "", "", "223:69,224:13")]
[assembly: GoPositionMap("encoding/xml/typeinfo.go", "typeinfo.cs", "AC9imKKCpoKCgoKCgriCgoKUgoKClIKUooKCpriCgqaCgriAgtqCqJKmgoK4goKUgoKUtLS0tLS02oKktILYpIKUgpSC3IK4uIKmuIKUlLiCgpSClIKCgpTcgoKCgrissoKUgpSCgoKUgoK4lAACFAAKAoKEkoKClIKUgoKCpoKClIKCpoLqgoLKgoLKgoKCgoLcgoKClIIAChaCAAUa0oKCgoKCgpSUppQ=", "", "", "189=Get/1/1/1;196=Get/1/1/1")]
[assembly: GoPositionMap("encoding/xml/xml.go", "xml.cs", "ACM+ggAXOpKCgoKokgAMHIDegAAHEpKCAAYQgJaSlKSkpKSUAFfAAbLcgqikgpTuAAY6ABoCgoKClIKClIKClLiUgoKCpuyCgoKClJSCgriCgoKUxoKUpAAJGNKUpKS0lIKCgrjKgpQAEiqCgoKUlIKCgpSCpoKCgoKUgoKUrOiCgriClIKClJSCgqqigpSCqJKCqqKCgoKokgACEgAIAoKClIKkgoKCgpSCpIKClISmuIKCgpS4qqKClIKUgoKUpqaasoKUABwGgoKUgpS4gqaCgqaUgoKClKaClJaSgoKUlIKClIKClKiSgoKUlIKCgoKClIKClJSClIKCgoKClIKCgoKUgoKClIKUpriClKaClIKCpoKSgoKUgoKClJSUlIKCuIKClIKCuIKClNyCgoKCgoKUgpSCgpT8tsiCgoKUgoKUgsqWooKClIKU7ra4lKqCgpSmgoKCgpSCgoKUgoKUlIKUlIKCgpSUgoKUgoKClIKUgoKClJSUgoKU5oKCgqaCpoKCpoKCgoKCpoSUgqaokoKCgpTGggAEEuKClIKClIKClIKmgoKUgqyyrLKqooKClK7ygoKmqJKClIKmABEYwpKCgoKCgoKCgpSU3IKCgpSCuIKCgpSClIKU3IKCgoKCgpSCgoKUgoKCgoKmgoaCgqaClIKCgoKCuIKCgqaClIKUgoKCgoKCgoLcgoKCgpSCgpSCgpSCuIKCpqa0gqaCgoKCgpSCgoK4rLIAAhTSgoKUgoKClIKUrLKCgriCgoKUrNKCgpSCgpSUgoKUgoKUlKaCrIKClIKClIKUgoKCgpSCpqaCgpSCgpSClIKCgoKUgqYABhAAwgGGAwB48gGkAIwClgSUABMqgoKCgoKCgoKaoqyygoKCgoKUtLS0tLS0gpS0tIKClKSClIKUlIKqooKCgoKClLS0tLS0tLS0goKUpIKClKyyqIKCmqKClIKmgoKCpoKUgpSmgqaCqsaCgoKCgoKCgpSCgoKmgpSCgpQ=")]
// </GoSourcePositionMaps>

namespace go.encoding;

using static go.encoding.xml_package;
using encoding = go.encoding_package;
using io = go.io_package;
using reflect = go.reflect_package;

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
[GoMethodHost(typeof(xml_package.CharData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛCharData;
[GoMethodHost(typeof(xml_package.Comment)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛComment;
[GoMethodHost(typeof(xml_package.Decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛDecoder;
[GoMethodHost(typeof(xml_package.Directive)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛDirective;
[GoMethodHost(typeof(xml_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛEncoder;
[GoMethodHost(typeof(xml_package.ProcInst)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛProcInst;
[GoMethodHost(typeof(xml_package.StartElement)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛStartElement;
[GoMethodHost(typeof(xml_package.SyntaxError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛSyntaxError;
[GoMethodHost(typeof(xml_package.TagPathError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛTagPathError;
[GoMethodHost(typeof(xml_package.UnmarshalError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛUnmarshalError;
[GoMethodHost(typeof(xml_package.UnsupportedTypeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛUnsupportedTypeError;
[GoMethodHost(typeof(xml_package.fieldInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛfieldInfo;
[GoMethodHost(typeof(xml_package.parentStack)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛparentStack;
[GoMethodHost(typeof(xml_package.printer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class xmlᴛprinter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class xmlᴛCharData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static CharData Copy(this CharData c) => xml_package.Copy⁔(c);
}
partial class xmlᴛComment {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Comment Copy(this Comment c) => xml_package.Copy⁔(c);
}
partial class xmlᴛDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Decode(this ж<Decoder> Ꮡd, any v) => xml_package.Decode⁔(Ꮡd, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error DecodeElement(this ж<Decoder> Ꮡd, any v, ж<StartElement> Ꮡstart) => xml_package.DecodeElement⁔(Ꮡd, v, Ꮡstart);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unmarshalInterface(this ж<Decoder> Ꮡd, Unmarshaler val, ж<StartElement> Ꮡstart) => xml_package.unmarshalInterface⁔(Ꮡd, val, Ꮡstart);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unmarshalTextInterface(this ref Decoder d, encoding.TextUnmarshaler val) => xml_package.unmarshalTextInterface⁔(ref d, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unmarshalAttr(this ref Decoder d, reflect.Value val, Attr attr) => xml_package.unmarshalAttr⁔(ref d, val, attr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error unmarshal(this ж<Decoder> Ꮡd, reflect.Value val, ж<StartElement> Ꮡstart) => xml_package.unmarshal⁔(Ꮡd, val, Ꮡstart);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool consumed, error err) unmarshalPath(this ж<Decoder> Ꮡd, ж<typeInfo> Ꮡtinfo, reflect.Value sv, slice<@string> parents, ж<StartElement> Ꮡstart) => xml_package.unmarshalPath⁔(Ꮡd, Ꮡtinfo, sv, parents, Ꮡstart);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Skip(this ref Decoder d) => xml_package.Skip⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Token, error) Token(this ref Decoder d) => xml_package.Token⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void translate(this ref Decoder d, ж<Name> Ꮡn, bool isElementName) => xml_package.translate⁔(ref d, Ꮡn, isElementName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void switchToReader(this ref Decoder d, io.Reader r) => xml_package.switchToReader⁔(ref d, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<stack> push(this ref Decoder d, nint kind) => xml_package.push⁔(ref d, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<stack> pop(this ref Decoder d) => xml_package.pop⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushEOF(this ref Decoder d) => xml_package.pushEOF⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool popEOF(this ref Decoder d) => xml_package.popEOF⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushElement(this ref Decoder d, Name name) => xml_package.pushElement⁔(ref d, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushNs(this ref Decoder d, @string local, @string url, bool ok) => xml_package.pushNs⁔(ref d, local, url, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error syntaxError(this ref Decoder d, @string msg) => xml_package.syntaxError⁔(ref d, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool popElement(this ref Decoder d, ж<EndElement> Ꮡt) => xml_package.popElement⁔(ref d, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Token, bool) autoClose(this ref Decoder d, Token t) => xml_package.autoClose⁔(ref d, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Token, error) RawToken(this ref Decoder d) => xml_package.RawToken⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Token, error) rawToken(this ref Decoder d) => xml_package.rawToken⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> attrval(this ref Decoder d) => xml_package.attrval⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void space(this ref Decoder d) => xml_package.space⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte b, bool ok) getc(this ref Decoder d) => xml_package.getc⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 InputOffset(this ref Decoder d) => xml_package.InputOffset⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint line, nint column) InputPos(this ref Decoder d) => xml_package.InputPos⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint savedOffset(this ref Decoder d) => xml_package.savedOffset⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte b, bool ok) mustgetc(this ref Decoder d) => xml_package.mustgetc⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ungetc(this ref Decoder d, byte b) => xml_package.ungetc⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> text(this ref Decoder d, nint quote, bool cdata) => xml_package.text⁔(ref d, quote, cdata);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Name name, bool ok) nsname(this ref Decoder d) => xml_package.nsname⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string s, bool ok) name(this ref Decoder d) => xml_package.name⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*ok*/ readName(this ref Decoder d) => xml_package.readName⁔(ref d);
}
partial class xmlᴛDirective {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Directive Copy(this Directive d) => xml_package.Copy⁔(d);
}
partial class xmlᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Indent(this ref Encoder enc, @string prefix, @string indent) => xml_package.Indent⁔(ref enc, prefix, indent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Encode(this ж<Encoder> Ꮡenc, any v) => xml_package.Encode⁔(Ꮡenc, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error EncodeElement(this ж<Encoder> Ꮡenc, any v, StartElement startʗp) => xml_package.EncodeElement⁔(Ꮡenc, v, startʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error EncodeToken(this ж<Encoder> Ꮡenc, Token t) => xml_package.EncodeToken⁔(Ꮡenc, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref Encoder enc) => xml_package.Flush⁔(ref enc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Encoder enc) => xml_package.Close⁔(ref enc);
}
partial class xmlᴛProcInst {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ProcInst Copy(this ProcInst p) => xml_package.Copy⁔(p);
}
partial class xmlᴛStartElement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static StartElement Copy(this StartElement e) => xml_package.Copy⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static EndElement End(this StartElement e) => xml_package.End⁔(e);
}
partial class xmlᴛSyntaxError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref SyntaxError e) => xml_package.Error⁔(ref e);
}
partial class xmlᴛTagPathError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref TagPathError e) => xml_package.Error⁔(ref e);
}
partial class xmlᴛUnmarshalError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this UnmarshalError e) => xml_package.Error⁔(e);
}
partial class xmlᴛUnsupportedTypeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnsupportedTypeError e) => xml_package.Error⁔(ref e);
}
partial class xmlᴛfieldInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static reflect.Value value(this ref fieldInfo finfo, reflect.Value v, bool shouldInitNilPointers) => xml_package.value⁔(ref finfo, v, shouldInitNilPointers);
}
partial class xmlᴛparentStack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error trim(this ref parentStack s, slice<@string> parents) => xml_package.trim⁔(ref s, parents);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error push(this ref parentStack s, slice<@string> parents) => xml_package.push⁔(ref s, parents);
}
partial class xmlᴛprinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string createAttrPrefix(this ж<printer> Ꮡp, @string url) => xml_package.createAttrPrefix⁔(Ꮡp, url);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteAttrPrefix(this ref printer p, @string prefix) => xml_package.deleteAttrPrefix⁔(ref p, prefix);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markPrefix(this ref printer p) => xml_package.markPrefix⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void popPrefix(this ref printer p) => xml_package.popPrefix⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error marshalValue(this ж<printer> Ꮡp, reflect.Value val, ж<fieldInfo> Ꮡfinfo, ж<StartElement> ᏑstartTemplate) => xml_package.marshalValue⁔(Ꮡp, val, Ꮡfinfo, ᏑstartTemplate);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error marshalAttr(this ref printer p, ж<StartElement> Ꮡstart, Name name, reflect.Value val) => xml_package.marshalAttr⁔(ref p, Ꮡstart, name, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error marshalInterface(this ref printer p, Marshaler val, StartElement start) => xml_package.marshalInterface⁔(ref p, val, start);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error marshalTextInterface(this ж<printer> Ꮡp, encoding.TextMarshaler val, StartElement startʗp) => xml_package.marshalTextInterface⁔(Ꮡp, val, startʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeStart(this ж<printer> Ꮡp, ж<StartElement> Ꮡstart) => xml_package.writeStart⁔(Ꮡp, Ꮡstart);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeEnd(this ref printer p, Name name) => xml_package.writeEnd⁔(ref p, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, slice<byte>, error) marshalSimple(this ref printer p, reflect.Type typ, reflect.Value val) => xml_package.marshalSimple⁔(ref p, typ, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error marshalStruct(this ж<printer> Ꮡp, ж<typeInfo> Ꮡtinfo, reflect.Value val) => xml_package.marshalStruct⁔(Ꮡp, Ꮡtinfo, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref printer p, slice<byte> b) => xml_package.Write⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref printer p, @string s) => xml_package.WriteString⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteByte(this ref printer p, byte c) => xml_package.WriteByte⁔(ref p, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref printer p) => xml_package.Close⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error cachedWriteError(this ref printer p) => xml_package.cachedWriteError⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeIndent(this ref printer p, nint depthDelta) => xml_package.writeIndent⁔(ref p, depthDelta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void EscapeString(this ref printer p, @string s) => xml_package.EscapeString⁔(ref p, s);
}
// </MethodForwarders>
