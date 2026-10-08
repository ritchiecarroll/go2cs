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
using static go.go.doc.comment_package;

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
[assembly: GoTypeAlias("Text", "ΔText")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Code, Block>(Pointer = true)]
[assembly: GoImplement<DocLink, ΔText>(Pointer = true)]
[assembly: GoImplement<Heading, Block>(Pointer = true)]
[assembly: GoImplement<Italic, ΔText>(Pointer = true)]
[assembly: GoImplement<Italic, ΔText>]
[assembly: GoImplement<Link, ΔText>(Pointer = true)]
[assembly: GoImplement<List, Block>(Pointer = true)]
[assembly: GoImplement<Paragraph, Block>(Pointer = true)]
[assembly: GoImplement<Plain, ΔText>(Pointer = true)]
[assembly: GoImplement<Plain, ΔText>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("go/doc/comment/html.go", "html.cs", "ABIqwoKCgpT4sqS2gpSCtoKCgoCCgoKkgoKCgraCgraCgpSCgoKCgoCCgoKCgpSkgoKClJSCzKKCgoKClJTYsoKUtIKCtIKCgoK0goKCgpSCggALEtKCgpSCgrSCgrSCgrSCgrSCgsY=")]
[assembly: global::go.GoPositionMap("go/doc/comment/markdown.go", "markdown.cs", "ABMs0rqCgoKUlNjCpLaCtoKCgIKCgqS2goKCgoKClMiCgoKUgIKCgpSkgoKClILuwoKCgoKUlrSCkpSUgoLGrNKClLSCgrSCgoKCtIKClIKCgoKCggAGEtKCgp6CgoIAAxCCgoIADwY=", "", "", "26=headingLevel/1/1/2,Repeat/1/1/2")]
[assembly: global::go.GoPositionMap("go/doc/comment/parse.go", "parse.cs", "ACdSABs6AAIS8gACEuKClILKpgASKAAKFAAKFsoACRIAEiYAOYwBAAsCkoKUlJSAgramgoIAAhYACAKClKrCgu6CmqKCgpikpKSkAAITAAIYgpSogpS0goKC3AATLoIABxKEgoKClIKUgpSAgqaCgoLugoKCgpSogoIADiCCloKUuIKClAAJFoKcgoLCAAQQgvaCgrqCkoKUqIK2qqKu1IKUgpSCqIKCgqiCooKClJSClIKUqJKokoKClKiSgoKUqqKClpaCgqiCgqiCqIKCgIKkgrqCgoCCpIKoqJKokq6SqJKCgqqkkoKCgpSUgoKCpoKEAAIQ0oKUgoKWkpKCgt6qooKqgoKAgraWooCkhIKCpIKCgoKUlIKu8oKCqICCkoKCgpSClJSmgpassoIAAhoADAKCgoKCgqiCgoKCgpSUtIKAgoKCuJKAgoKCgsaCtIKoggACEgAJAoKCgqaCgoKmgqKCgpSCgIK2gIK2yqzigoKClIKUAAIS4oKCgoKUkoKCgqaCgoKAktyCgoKkgIKSgoKUgoKUlIKCtpSUgoKUlIKCgrSCgoK0xoIAAhDYspSktLS0tKSCqIK6goKUgoKCgpSUAAcSgoKCgoKUgpSUtLS0gpS0gsissgABEKTaqgAPGKyqAAoQ2poAGzCqkoKClIIAAhDkgoKAgoKClKSCgoKUlNiaAAkMrKKClIKUgpSClIKUgoKCgpSmpoKClIKCptaKAA0U", "291-291:1;703-710:1;787-792:1;899-902:1;903-909:2", "", "625=TrimSpace/1/1/3;816=parseText/1/1/1")]
[assembly: global::go.GoPositionMap("go/doc/comment/print.go", "print.cs", "ADN0goKUpqKClKaigpQAAioAEgKCgoKUlJSkpLaClAAFFPaigoKCgpSCgoKClKYABhLCgoKCgpQABhCCgoKCgoKClIKCgoK6qqKAgqTYsqS2graCgraCgoKCgoKUyIKCgpSCgpSClIKCgpSC7rKClLS0gpSCgsaCgt7CgoKCgoKU")]
[assembly: global::go.GoPositionMap("go/doc/comment/std.go", "std.cs", "AAkU")]
[assembly: global::go.GoPositionMap("go/doc/comment/text.go", "text.cs", "ABg00tyClIKWgoKCgpSUgoKCgqaCgoKCuKrUgoKClIKU2MKktoK2goK2goKCgoKClMiCgoKClIKCgpSClIKCgoKClO7CgoKEgoKUlIKCgpSCgpSUvrKClLS0tAAMKAAsPoqClKSkpKS6goKCqLSCgt6CgoKUzJKksqKClIKUqIKCgoKCgoKClIKUgriCuJaCgpSCgoKUqJKUpA==", "231-231:1;232-244:2;255-272:3;278-278:4;280-289:5;281-284:5.1")]
// </GoSourcePositionMaps>

namespace go.go.doc;

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
    public partial interface ΔText {}
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
