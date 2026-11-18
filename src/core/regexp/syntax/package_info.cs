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
using static go.regexp.syntax_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6f6e63652073796e632e4f6e63653b206d206d61705b737472696e675d737472696e677d", "categoryAliasesᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<ranges, sort_package.Interface>]
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
[assembly: GoFieldAccess(typeof(Inst))]
[assembly: GoFieldAccess(typeof(Prog))]
[assembly: GoFieldAccess(typeof(Regexp))]
[assembly: GoFieldAccess(typeof(parser))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("regexp/syntax/compile.go", "compile.cs", "ABgugsaigoKCgoKUgsqigpSCpoKClJQADyCigoKCgoLGgoKCxoK0opSkpIKUgoKCgpSmpKSkpKSkpKSkpIKCgqSkpKSClIKCgpSmpIKClJSmlIKCpoKCgqaCpoKCgpSClKaUgsqCppSClIKmgoKCgoKCpoKCgoKClIKUggACENKCgoKClIKUgqaCppSmgqaCgoKCpoKCgoKCgpSUgqaUtLS2")]
[assembly: GoPositionMap("regexp/syntax/op_string.go", "op_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKCAAcStoKUgqSk")]
[assembly: GoPositionMap("regexp/syntax/parse.go", "parse.cs", "ABgsggAtOIIAVLIBgoKCgpSClIKmooKUgqaCgpSCpqLcgpSCgoKUgpSClKaC3IKCuIK4ooKCuIKUxrS0gsaClILGgoKClJSmtoKCpoKClIKCgqaCuKKCgqaCgoKCpoKs0oKUgpSCgoIAARKCuIKCpqaCggACGAAJAoKCpoKCgrimgoKCgqaCgqiSgoKClIKCyJKClIKCgpTKooKCrsKCgoKClLimgoKUgoKmgoKCgoKCgpSCpgAEGAAJAoKCgpSClIKUgqaCgqbIkqaCgpSCpoKmqLaCgpSCuILKgqbIspSCgoKClIKCgpSmAAcSwoKUgoKCgoKUpoKCgoKCpgACJAAPAoKokoKCgo7igoKCgoKClKaCAAkUlJSmgoKUgoKUlIKCuIKClAAJFIKCgozSgoKmAAcQlJSmgoKCgpSUgoK4gpSmgoLugsqUgriCgoKmlIKClIK4gpSUpoKCgoKUlJSqwoKUgpSqwqaCgoKCgqaCtIKCtIK2poKCgqaqwoKUgoKClJSs0oKClIKUgrSCgqSUgpTGgoKCgoKUgpSUAAIQ0qbCgqT4tMiUgpSozIKCgoKCgpSClLaUgpSUgoK0grSClLSClJS0gpSUtIKUlLSCxoKUtLSkgoKUgrSCgoKUgoKUlJSClIK0gpSCgrSCgrSCgsamkoKCgoKUgpS0goLIgqaCgoKUgoKCgsqAgoKCgqSmgpS0poKUlJSCgpQACAwACAKClIKCgpSClIKUgoKUgoKAgoKUtoKUgoKs0gAQIoKUlIKCuIKCgpSmgoKClIK4goKCgqiSgoKCgoKCgpSUyIK0grSCtILIgpSmgsiCgpSUlJSC2AAEENKClIKCpsjSgqaClIKClIKUgpSCgpSUyqKukpSkgoKmpKSUyJLcgt6y6oLYgpTYgpSCgu7WgoKClIKClIKCgqaCgoKCppSCgqaokoKUlJSCgpSCgoKCpoKUlIKCgpSq0oKClIKCpoK0yuyClMaCgoKUgpTIgpSClMqCgoKClIKUgpSCgpSCgpSUgpS4goKUgoKUAAkSpKSkpKSkqqKCyoKmAAsaAAkCgpSCgpSs8oKmgoKUgqKCgpSmooKClKaCgoKCgpSmxsq4ABAgkoKCAAMS8oKCgoKUtIKUtIK2gpSUgpSClJSClKyyuKSkpKSUgIKkgILsgoKClMwACAKCuIKClIKCgpSSlIKmgoKClJSCgoLKgoKmgoKUgqaCgpTcgoKCgoKClKaqwoKCgpSCgoK4griCgqaCgpSmgoKClIKCyoKClIKCuICCgsiCkoKUlIKCgpSCgqaClKamgoKClIKCytaUgoK4goKClIKUpoKCpqqikoKClIKUlKiSgpSo2oKCgoKCgpSClMoABxi0lJSUlJSClJSCuIKCgoKCpqqigpSokoKUqqKCgoKClJSClKiSgoKCgpSCpoKCgoKUgqaokoKCgoKClIKUgoKUpoKCgoKUgpSCgpSmgpSqooKCgoKCgoKUlIKmlAAMFoKCgoKmgqaCgoKCxoKCgoKUlKaigoKUpoKmgoKUgpSClA==", "892-903:1;1956-1965:1", "", "724=isCharClass/1/2/2,isCharClass/2/2/2", "", "960:21,961:13")]
[assembly: GoPositionMap("regexp/syntax/perl_groups.go", "perl_groups.cs", "AAgSuNzuAAcQ3Mq4ysq4uLi47sq47tw=")]
[assembly: GoPositionMap("regexp/syntax/prog.go", "prog.cs", "AChSAA8cgoKUAA4q4oKClLS0pJS0tKSClKzWAAwWgoKCqJKCgpTIkoKUpMyypoKokoKClKqigoKCgoKUtOikgrQACA6iAAIQ0pSUuIKClIKCgrimgpTKgoKUgqa4goKCgoKClJSmrLKUpKSkpKSUpoKCgsaigriigoKCgpSClIKCuIIAHAaClLS0tLS0tLSUlIKClLS0tA==")]
[assembly: GoPositionMap("regexp/syntax/regexp.go", "regexp.cs", "ADyAAcKClIKUpoLIpqaCyILIgrYADiKigpSCAAIQ0pTcgoKClLjKgoKCgoKCyqampqaClKas0oKCgoKCgoKUgoKClIKCgoKUgqaUlJSUAAsK0oKUlIKCgpSClIKUgoKClIKmlIKUgoKmlLS0tILGgoKUgoKCpoKCgoKCgpS4goKCgoKUuLS0tLS0gpTGtLSCgoKUlIKUtIKCgpSUlLS0tIKCgoKCpqSCxoKCgpTGgoKU1sSCgoKCgoKUgvqCgoKUgqaUtLS0tLS0goKCgpSClIKC6pKCgpSCgqaokoKCpoKClII=")]
[assembly: GoPositionMap("regexp/syntax/simplify.go", "simplify.cs", "AAscAAkCgpSmgoKClIKCgpSCpqaCyoK4ppSCuIK4goKClILcggAGEJKCgoKCyoKCgoKClIKUlILKpgAEJAAUBoKmgpSCpoKC")]
// </GoSourcePositionMaps>

namespace go.regexp;

[GoPackage("syntax")]
public static partial class syntax_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct categoryAliasesᴛ1 {}
    internal partial struct charGroup {}
    internal partial struct compiler {}
    internal partial struct frag {}
    internal partial struct parser {}
    internal partial struct patchList {}
    internal partial struct printFlags {}
    internal partial struct ranges {}
    public partial struct EmptyOp {}
    public partial struct Error {}
    public partial struct ErrorCode {}
    public partial struct Flags {}
    public partial struct Inst {}
    public partial struct InstOp {}
    public partial struct Op {}
    public partial struct Prog {}
    [GoValueClone("Sub0", "Rune0")] public partial struct Regexp {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
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
[GoMethodHost(typeof(syntax_package.Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛError;
[GoMethodHost(typeof(syntax_package.ErrorCode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛErrorCode;
[GoMethodHost(typeof(syntax_package.Inst)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛInst;
[GoMethodHost(typeof(syntax_package.InstOp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛInstOp;
[GoMethodHost(typeof(syntax_package.Op)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛOp;
[GoMethodHost(typeof(syntax_package.Prog)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛProg;
[GoMethodHost(typeof(syntax_package.Regexp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛRegexp;
[GoMethodHost(typeof(syntax_package.compiler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛcompiler;
[GoMethodHost(typeof(syntax_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛparser;
[GoMethodHost(typeof(syntax_package.patchList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛpatchList;
[GoMethodHost(typeof(syntax_package.ranges)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛranges;
// </MethodHolders>
