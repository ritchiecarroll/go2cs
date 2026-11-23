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
using bytes = go.bytes_package;
using strings = go.strings_package;
// </ImportedTypeAliases>

using go;
using static go.go.doc.comment_package;
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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Code, Block>(Pointer = true)]
[assembly: GoImplement<DocLink, Text>(Pointer = true)]
[assembly: GoImplement<Heading, Block>(Pointer = true)]
[assembly: GoImplement<Italic, Text>(Pointer = true)]
[assembly: GoImplement<Italic, Text>]
[assembly: GoImplement<Link, Text>(Pointer = true)]
[assembly: GoImplement<List, Block>(Pointer = true)]
[assembly: GoImplement<Paragraph, Block>(Pointer = true)]
[assembly: GoImplement<Plain, Text>(Pointer = true)]
[assembly: GoImplement<Plain, Text>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Code))]
[assembly: GoFieldAccess(typeof(DocLink))]
[assembly: GoFieldAccess(typeof(Heading))]
[assembly: GoFieldAccess(typeof(Link))]
[assembly: GoFieldAccess(typeof(LinkDef))]
[assembly: GoFieldAccess(typeof(List))]
[assembly: GoFieldAccess(typeof(ListItem))]
[assembly: GoFieldAccess(typeof(Paragraph))]
[assembly: GoFieldAccess(typeof(mdPrinter))]
[assembly: GoFieldAccess(typeof(parseDoc))]
[assembly: GoFieldAccess(typeof(textPrinter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/doc/comment/html.go", "html.cs", "ABIqwoKCgpQACgiypLaClIK2goKCgoKClIKCgoK2goK2goKUgoKCgoKCgoKCgpSUgoKClJSCzKKCgoKClJTosoKUtIKCtIKCgoK0goKCgpSCggAOEtKCgpSCgrSCgrSCgrSCgrSCgrY=")]
[assembly: GoPositionMap("go/doc/comment/markdown.go", "markdown.cs", "ABMs0sqCgoKUlOjCpLaCtoKCgoKClLaCgoKCgoKUyIKCgpSCgoKUlIKCgpSC7sKCgoKClKa0goKUlIKCtqzSgpS0goK0goKCgrSCgpSCgoKCgoIABhLSgoLugoKCAAkQgoKCtg==", "", "", "26=headingLevel/1/1/2,Repeat/1/1/2")]
[assembly: GoPositionMap("go/doc/comment/parse.go", "parse.cs", "ACdSABo6AAES8gACEuKClILKpgASKAAJFAAKFsoACBIAESYAOIwBAAsCgoKUlJSCpqaCggACFgAIAoKUqsKC7oKqooKClLS0tLS0pIKUuIKUtIKCguwAFC6iAAgSlIKCgpSClIKUgqaCgoLugoKCgpS4goIADyCCpoKUuIKClAAKFoLcgoKCAAgQgraCgsqCgoKUuILGuqKu1IKUgpSCuIKCgriCooKClJSClIKUqJKokoKClKiSgoKUqqKCpqaCgriCgriCuIKCgpSCyoKCgpSCuKiSqJKukqiygoKqxJKCgoKUlJKCgqaChAACENKClIKCpoKCgoLuqsKCqoKCgqamoqSUgoKUgoKCgpSUgq7ygoK4goKCgoKUgpSUpoKmrLKCAAIaAA0CgoKCgoK4goKCgoKUlLSCgoKCuIKCgoKCpoKkgriCAAISAAkCgoKCpoKCgqaCooKClIKCpoKmyqzigoKClIKUAAIS4oKCgoKUkoKCgqaCgoKAktyCgoKkgIKSgoKUgoKUlIKCtpSUgoKUlIKCgrSCgoK0toIAAhD4spSktLS0tJSCuILKgoKUgoKCgpSUAAgSgoKCgoKUgpSUtLS0gpSkgtissgABEJTqqgAQGKyqAAsQ6poAHDCqkoKClIIAAhDkgoKCgoKUlIKCgpSU6JoACgysooKUgpSClIKUgpSCgoKClKamgoKUgoKm5ooADhQ=", "291-291:1;703-710:1;787-792:1;899-902:1;903-909:2", "", "640=TrimSpace/1/1/3;837=parseText/1/1/1")]
[assembly: GoPositionMap("go/doc/comment/print.go", "print.cs", "ADN0goKUpqKClKaigpQAAioAEgKCgoKUlJSkpKaClAAGFPaigoKCgpSCgoKClKYABhLCgoKCgpQABxCCgoKCgoKClIKCgoLKqqKClOiypLaCtoKCtoKCgoKCgpTIgoKClIKClIKUgoKClILusoKUtLSClIKCxoKC3sKCgoKCgpQ=")]
[assembly: GoPositionMap("go/doc/comment/std.go", "std.cs", "AAkU")]
[assembly: GoPositionMap("go/doc/comment/text.go", "text.cs", "ABg00tyClIKmgoKCgpSUgoKCgqaCgoKCuKrUgoKClIKU6MKktoK2goK2goKCgoKClMiCgoKClIKCgpSClIKCgoKClO7CgoKUgoKUlIKCgpSCgpSUvrKClLS0tAAMKAAvPoqClKSkpJTKgoKCuLSCgu6CgoKU3IK0sqKClIKUuIKCgoKCgoKClIKUgriCuKaCgpSCgoKUqJKUlA==", "231-231:1;232-244:2;255-272:3;278-278:4;280-289:5;281-284:5.1")]
// </GoSourcePositionMaps>

namespace go.go.doc;

using static global::go.go.doc.comment_package;
using bytes = global::go.bytes_package;
using strings = global::go.strings_package;

[GoPackage("comment")]
public static partial class comment_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct commentPrinter {}
    internal partial struct htmlPrinter {}
    internal partial struct mdPrinter {}
    internal partial struct parseDoc {}
    internal partial struct span {}
    internal partial struct spanKind {}
    internal partial struct textPrinter {}
    [GoLocalName("score")] internal partial struct wrap_score {}
    public partial interface Block {}
    public partial interface Text {}
    public partial struct Code {}
    public partial struct Doc {}
    public partial struct DocLink {}
    public partial struct Heading {}
    public partial struct Italic {}
    public partial struct Link {}
    public partial struct LinkDef {}
    public partial struct List {}
    public partial struct ListItem {}
    public partial struct Paragraph {}
    public partial struct Parser {}
    public partial struct Plain {}
    public partial struct Printer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(comment_package.Code)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛCode;
[GoMethodHost(typeof(comment_package.DocLink)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛDocLink;
[GoMethodHost(typeof(comment_package.Heading)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛHeading;
[GoMethodHost(typeof(comment_package.Italic)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛItalic;
[GoMethodHost(typeof(comment_package.Link)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛLink;
[GoMethodHost(typeof(comment_package.List)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛList;
[GoMethodHost(typeof(comment_package.Paragraph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛParagraph;
[GoMethodHost(typeof(comment_package.Parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛParser;
[GoMethodHost(typeof(comment_package.Plain)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛPlain;
[GoMethodHost(typeof(comment_package.Printer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛPrinter;
[GoMethodHost(typeof(comment_package.commentPrinter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛcommentPrinter;
[GoMethodHost(typeof(comment_package.htmlPrinter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛhtmlPrinter;
[GoMethodHost(typeof(comment_package.mdPrinter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛmdPrinter;
[GoMethodHost(typeof(comment_package.parseDoc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛparseDoc;
[GoMethodHost(typeof(comment_package.textPrinter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class commentᴛtextPrinter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class commentᴛCode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref Code _) => comment_package.block⁔(ref _);
}
partial class commentᴛDocLink {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ref DocLink _) => comment_package.text⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string DefaultURL(this ref DocLink l, @string baseURL) => comment_package.DefaultURL⁔(ref l, baseURL);
}
partial class commentᴛHeading {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref Heading _) => comment_package.block⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string DefaultID(this ref Heading h) => comment_package.DefaultID⁔(ref h);
}
partial class commentᴛItalic {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this Italic _) => comment_package.text⁔(_);
}
partial class commentᴛLink {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ref Link _) => comment_package.text⁔(ref _);
}
partial class commentᴛList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref List _) => comment_package.block⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool BlankBefore(this ref List l) => comment_package.BlankBefore⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool BlankBetween(this ref List l) => comment_package.BlankBetween⁔(ref l);
}
partial class commentᴛParagraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref Paragraph _) => comment_package.block⁔(ref _);
}
partial class commentᴛParser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Doc> Parse(this ж<Parser> Ꮡp, @string text) => comment_package.Parse⁔(Ꮡp, text);
}
partial class commentᴛPlain {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this Plain _) => comment_package.text⁔(_);
}
partial class commentᴛPrinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> HTML(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) => comment_package.HTML⁔(Ꮡp, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Markdown(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) => comment_package.Markdown⁔(Ꮡp, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint headingLevel(this ref Printer p) => comment_package.headingLevel⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string headingID(this ref Printer p, ж<Heading> Ꮡh) => comment_package.headingID⁔(ref p, Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string docLinkURL(this ref Printer p, ж<DocLink> Ꮡlink) => comment_package.docLinkURL⁔(ref p, Ꮡlink);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Comment(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) => comment_package.Comment⁔(Ꮡp, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Text(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) => comment_package.Text⁔(Ꮡp, Ꮡd);
}
partial class commentᴛcommentPrinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, Block x) => comment_package.block⁔(ref p, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, slice<Text> x) => comment_package.text⁔(ref p, Ꮡout, indent, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void indent(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, @string s) => comment_package.indent⁔(ref p, Ꮡout, indent, s);
}
partial class commentᴛhtmlPrinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, Block x) => comment_package.block⁔(ref p, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, slice<Text> x) => comment_package.text⁔(ref p, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void escape(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, @string s) => comment_package.escape⁔(ref p, Ꮡout, s);
}
partial class commentᴛmdPrinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ж<mdPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, Block x) => comment_package.block⁔(Ꮡp, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ж<mdPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, slice<Text> x) => comment_package.text⁔(Ꮡp, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rawText(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, slice<Text> x) => comment_package.rawText⁔(ref p, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void escape(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, @string s) => comment_package.escape⁔(ref p, Ꮡout, s);
}
partial class commentᴛparseDoc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string importPath, bool ok) lookupPkg(this ref parseDoc d, @string pkg) => comment_package.lookupPkg⁔(ref d, pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Block oldHeading(this ref parseDoc d, @string line) => comment_package.oldHeading⁔(ref d, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Block heading(this ref parseDoc d, @string line) => comment_package.heading⁔(ref d, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Code> code(this ref parseDoc d, slice<@string> lines) => comment_package.code⁔(ref d, lines);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Block paragraph(this ref parseDoc d, slice<@string> lines) => comment_package.paragraph⁔(ref d, lines);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<List> list(this ж<parseDoc> Ꮡd, slice<@string> lines, bool forceBlankBefore) => comment_package.list⁔(Ꮡd, lines, forceBlankBefore);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Text> parseLinkedText(this ж<parseDoc> Ꮡd, @string text) => comment_package.parseLinkedText⁔(Ꮡd, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<DocLink> link, bool ok) docLink(this ref parseDoc d, @string text, @string before, @string after) => comment_package.docLink⁔(ref d, text, before, after);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Text> parseText(this ref parseDoc d, slice<Text> @out, @string s, bool autoLink) => comment_package.parseText⁔(ref d, @out, s, autoLink);
}
partial class commentᴛtextPrinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void block(this ж<textPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, Block x) => comment_package.block⁔(Ꮡp, Ꮡout, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void text(this ж<textPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, @string indent, slice<Text> x) => comment_package.text⁔(Ꮡp, Ꮡout, indent, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void oneLongLine(this ref textPrinter p, ж<strings.Builder> Ꮡout, slice<Text> x) => comment_package.oneLongLine⁔(ref p, Ꮡout, x);
}
// </MethodForwarders>
