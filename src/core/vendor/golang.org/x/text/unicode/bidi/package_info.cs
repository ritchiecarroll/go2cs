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
using static go.vendor.golang.org.x.text.unicode.bidi_package;

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
[assembly: GoImplement<bracketPairs, sort_package.Interface>]
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
[assembly: GoFieldAccess(typeof(Paragraph))]
[assembly: GoFieldAccess(typeof(options))]
[assembly: GoFieldAccess(typeof(paragraph))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bidi.go", "bidi.cs", "ADeKAQALAoIAETIACQKClIKClIKCgoKClIKCgoKmgpSCpgACEPKCggACEPKCgqyyrLIAAhDSgoKCgoKmxoKUgpSmgoKUlIKCgoKUgqaCgoLIsoKmgpSCgpSCgqaUgqqigoKClIKCAA8esqiSqJLKABQkkqiSAAIS4qqizLKCgpSCgoK4gpSUrLKCgoKCgoKUpg==", "70-72:1")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bracket.go", "bracket.cs", "ADhwggAJDIKCAAMUAAgC3IKClIIAHkqiAAgU9paUlKaCgqbIgoKCgpSUgpSmAAd8ADcCpqSkAAMW8oKCgoKUgpSmqqKCgriopNyCppSClAAHEKaCgpSCgoKUpoKCgpS6koI=", "", "", "83=New/1/1/2")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/core.go", "core.cs", "AD56soKCpgAZSPKCgpSClIKUgqYACBKCxpjC3IK4grgACRSmuLi4ptwAAiQADwKClIKmgpSCgoKCgoKCgoK4ggAIFtKmgoKCgoKCgsqmpKQAEh6CgpSCgoKCpoKmgqaCyJKCxpSUlIKmgpSCgoK4gpSmpoKC3JS0tLaCyoKUgv6CgqaCgpSClNqUlIKCggAIEIKCgoK2goIAEyK4woKCgriCgpSCgqaCgoKmgpaCgqaCAAsc1riCgoLK3IKCgoKClO6CggAQIoKCgoKCgoKC3KKUgqaCgpSCgoKmgqbKgoLKopSCgoKCgqaC3qaUoqaCltiClIKCpoKUgoK4gpTKpqb6goK4goLappSClIKUlIKUuJSmAAUQooKCvtKCgoKCgqa0qsKCgoKCpgAHFOKCgpSCgpSCgqaUuIKUqMKmgoKCuJSUooKClJSUgoKClKamvsKCgoLugpSCggAGIgAVGJTKgpSmgoKU7oKCgoKUpqYAAiQADwKUyqKUgoKClIKUlKyylILcgoKCgpSCuIKClIKCgqaCpsqqopSUqJKUlKiSgpSmgoKUgoKmpoKGlKaCgoKClJSClKaCgpSCxqamgoKUgpQ=", "", "", "499=typeForLevel/1/2/5,typeForLevel/2/2/6")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/prop.go", "prop.cs", "ABEeAAMaAAsCgoKUqICYkJaSxgALGpKCggACKgASAoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKassoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKY=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/tables15.0.0.go", "tables15.0.0.cs", "AA4Y3rKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpSssoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKaqooKClIKClIKClIKClAAIDILIkpT8ALAN4ho=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

[GoPackage("bidi", ImportPath = "vendor/golang.org/x/text/unicode/bidi")]
public static partial class bidi_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bidiTrie {}
    internal partial struct bracketPair {}
    internal partial struct bracketPairer {}
    internal partial struct bracketPairs {}
    internal partial struct bracketType {}
    [GoValueClone("embeddingLevelStack", "overrideStatusStack", "isolateStatusStack")] internal partial struct directionalStatusStack {}
    internal partial struct isolatingRunSequence {}
    internal partial struct level {}
    internal partial struct paragraph {}
    public partial struct Class {}
    public partial struct Direction {}
    public partial struct Ordering {}
    public partial struct Paragraph {}
    public partial struct Properties {}
    public partial struct Run {}
    public partial struct options {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bidi_package.Class)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛClass;
[GoMethodHost(typeof(bidi_package.Ordering)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛOrdering;
[GoMethodHost(typeof(bidi_package.Paragraph)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛParagraph;
[GoMethodHost(typeof(bidi_package.Properties)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛProperties;
[GoMethodHost(typeof(bidi_package.Run)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛRun;
[GoMethodHost(typeof(bidi_package.bidiTrie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛbidiTrie;
[GoMethodHost(typeof(bidi_package.bracketPair)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛbracketPair;
[GoMethodHost(typeof(bidi_package.bracketPairer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛbracketPairer;
[GoMethodHost(typeof(bidi_package.bracketPairs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛbracketPairs;
[GoMethodHost(typeof(bidi_package.directionalStatusStack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛdirectionalStatusStack;
[GoMethodHost(typeof(bidi_package.isolatingRunSequence)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛisolatingRunSequence;
[GoMethodHost(typeof(bidi_package.paragraph)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bidiᴛparagraph;
// </MethodHolders>
