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
using static go.go.token_package;

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
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(FileSet))]
[assembly: GoFieldAccess(typeof(lineInfo))]
[assembly: GoFieldAccess(typeof(node))]
[assembly: GoFieldAccess(typeof(tree))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/token/position.go", "position.cs", "ACVEgAABFAAIAoKCgpSCgqaClAAkRJIAGC6SqJKokqiSqLKCgoKs0oKClK4ACQKClIKCgu6CoqjCgoKCAAIWAAsEgoKCyoKCgqrSgoKCgpSCgsqCkqwACAKClIKCgpQAEx6iAAIWAAoCgoKUqJKUgsaCxriCuAACGAAJAgACGAAJAqqixoKGppTMAAgCgoKAgqSUgoKClIKCyoKm7oKmsoKCggACEPKClKyyACRIku7CgoKCAAIkABUElIKCgpSClIKCpoKCpoKCgt4AHCaClIKCtAACEAAMAoKUlIKCtKjCgsqigoLkpMSCpoKUgqaClNzSgpSu4oKCpqqizAALFJKClIKUpg==", "334-336:1;559-563:1")]
[assembly: GoPositionMap("go/token/serialize.go", "serialize.cs", "ABwwsoKCpoKCggAHEIKUqLKUgoKCgoLulIKU", "", "", "66=Clone/1/2/4,Clone/2/2/5")]
[assembly: GoPositionMap("go/token/token.go", "token.cs", "AIUBjAIAZ9YB0oKClIKUAA8ispSkpKSklOqCgoK6koKUzrCYoqqQtpKCqKSCrLKClIKCpg==")]
[assembly: GoPositionMap("go/token/tree.go", "tree.cs", "ADdqggAEGAAJApSklMrSgoKClIKUgoK4goKCgpSClIKCAAUQAAgCkoKCgoKClKauwoKClIKCgqaClKYABhDClKSkpNiigoKUlIKClMaigoLYsoKCuLKCgriigpSmgoKCxqKUgpS0tLQABBQACAKCgoKUgpS2gpSkuJS81IKClIKUgoKUgoKq1IKClIKUgoKUgoKq0oKCgpSCAAUQ0riClIKUgoKospSCyoKUpoKUtoKCtraCgoKCgqrCgpSCgoKUgoKCgpSs4oKUgoKClA==", "132-151:1", "", "379=Name/1/2/1,Base/1/2/1,End/1/2/1,Name/2/2/2,Base/2/2/2,End/2/2/2")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("token")]
public static partial class token_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct key {}
    internal partial struct node {}
    internal partial struct serializedFileSet {}
    internal partial struct tree {}
    public partial struct File {}
    public partial struct FileSet {}
    public partial struct Pos {}
    public partial struct Position {}
    public partial struct Token {}
    public partial struct lineInfo {}
    public partial struct serializedFile {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
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
[GoMethodHost(typeof(token_package.File)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛFile;
[GoMethodHost(typeof(token_package.FileSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛFileSet;
[GoMethodHost(typeof(token_package.Pos)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛPos;
[GoMethodHost(typeof(token_package.Position)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛPosition;
[GoMethodHost(typeof(token_package.Token)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛToken;
[GoMethodHost(typeof(token_package.node)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛnode;
[GoMethodHost(typeof(token_package.tree)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tokenᴛtree;
// </MethodHolders>
