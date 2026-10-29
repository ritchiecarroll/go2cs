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
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ranges, sort_package.Interface>]
[assembly: GoImplement<ΔError, error>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: go.GoFieldAccess(typeof(Inst))]
[assembly: go.GoFieldAccess(typeof(Prog))]
[assembly: go.GoFieldAccess(typeof(Regexp))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("regexp/syntax/compile.go", "compile.cs", "ABcugqaigoKCgoKUgsqigpSCpoKClJQADSCigoKCgoKmgoKCppKUopSkpIKUgoKCgpSmpKSkpKSkpKSkpIKCgqSkpKSClIKCgpSmpIKClJSmlIKCpoKCgqaCpoKCgpSClKaUgsqCppSClIKmgoKCgoKCpoKCgoKClIKUggACENKCgoKClIKUgqaCppSmgqaCgoKCpoKCgoKCgpSUgqaUtLS2")]
[assembly: go.GoPositionMap("regexp/syntax/op_string.go", "op_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCggAFEpaClIKkpA==")]
[assembly: go.GoPositionMap("regexp/syntax/parse.go", "parse.cs", "ABUsggAZOIIAULIBgoKCgpSClIKmooKUgqaCgpSCpqLcgpSCgoKUgpSClKaC3IKCuIK4ooKAgsiClMa0tILGgpSCxoKCgpSUpraCgqaCgpSCgoKmgriigoCCtoKCgoKmgqzSgpSClIKCggABEoK4goKmpoKCAAIYAAkCgoKmgoKCuKaCgoKCpoKCqJKCgoKUgoKokoKUgoKClKqigoKuwoKCgoKUuKaCgpSCgqaCgoKCgoKClIKmAAIYAAkCgoKClIKUgpSCpoKCpqiSpoKClIKmgqaotoKClIK4gsqCpqiylIKCgoKUgoKClKYABRLCgpSCgoKCgpSmgoKCgoKmAAIkAA8CgqiSgoKCjuKCgoKCgoKUpoIACRSUlKaCgpSCgpSUgoK4goKUAAkUgoKCjNKCgqYABxCUlKaCgoKClJSCgriClKaCgu6CypSCuIKCgqaUgoKUgriClJSmgoKCgpSUlKrCgpSClKrCpoKCgoKCpoK0goK0gramgoKCpqrCgpSCgoKUlKzSgoKUgpSCtIKCpJSClKaCgoKCgpSClJQAAhDSpsKCgKT4tNiUgIKkqMyCgoKCgoKUgIKktpSAgqSUgoK0grSAgqS0gpSUtIKUlLSClJS0gILWgpS0tKSCgIKkgrSCgoKUgoKUlJSAgqSCtIKUgoK0goK0goLGppKCgoKClIKUtIKCyIKmgoKClIKCgoLKgIKCgoKkpoCCpLSmgpSUlIKClOwACAKClIKCgIKkgpSClIKClIKCgIKClLaClIKCrNIAECKClJSCgriCgoCCpKaCgoCCpIK4goKCgqiSgoKCgoKCgIKklMiCtIK0grSCyIKUpoLIgoKUlJSUgtgAAhDSgpSCgqao0oKmgpSCgpSClIKUgoKUlKqirpKUpIKCpqSklKiS3IK+suqC2IKU2IKUgoLO1oKCgpSCgpSCgoKmgoKCgqaUgoKmqJKClJSUgoKUgoKCgqaClJSCgoKUqtKCgpSCgqaCtMrsgpTGgoKClIKUyIKUgIKkyoKCgoKUgIKkgpSCgpSCgpSUgpS4goCCpIKClAAJEqSkpKSkpKqigsqCpgAHGgAJAoKUgoKUrPKCpoKClIKigoKUpqKCgpSmgoKCgoKUpqbKuAAQIJKCggADEvKCgoKClLSClLSCtoKUlIKUgpSUgpSssrikpKSklICCpICC7IKAgoKkrAAIAoK4goKUgoKClJKUgqaCgoCCpJSCgoCC2oKCpoKClIKmgoKU3IKCgoKCgpSmqsKCgoKUgoKCuIK4goKmgoKUpoKCgpSCgsqCgpSCgriAgoLIgpKAgqSUgoKAgqSCgqaClKamgoKClIKCqtaUgoK4goKClIKUpoKCpqqikoKClIKUlKiSgpSo2oKCgoKCgpSClMoABxi0lJSUlJSClJSCuIKCgoKCpqqigpSokoKUqqKCgoKClJSClKiSgoKCgpSCpoKCgoKUgqaokoKCgoKClIKUgoKUpoKCgoKUgpSCgpSmgpSqooKCgoKCgoKUlIKmlAAKFoKCgoKmgqaCgoKCpoKCgoKUlKaigoKUpoKmgoKUgpSClA==", "892-903:1;1956-1965:1", "", "689=isCharClass/1/2/2,isCharClass/2/2/2")]
[assembly: go.GoPositionMap("regexp/syntax/perl_groups.go", "perl_groups.cs", "AAgSuNzuAAgQ3Mq4ysq4uLi47sq47tw=")]
[assembly: go.GoPositionMap("regexp/syntax/prog.go", "prog.cs", "ACdSAA0cgoKUAAwq4oKClLS0pJS0tKSClKzWAAoWgoKCqJKCgpSokoKUpKyypoKokoKClKqigoKCgoKUtOikgrTOogACENKUlLiCgpSCgoK4poKUyoKClIKmuIKCgoKAgoKUlLasspSkpKSkpJSmgoKCpqKCuKKCgoKClIKUgoK4ggAQBoKUtLS0tLS0tJSUgoKUtLS0")]
[assembly: go.GoPositionMap("regexp/syntax/regexp.go", "regexp.cs", "ADmAAcKClIKUpoLIpqaCyILIgrYADCKigpSCAAIQ0pTcgoKClLjKgoKCgoKCyqampqaClKas0oKCgoKCgoKUgoKClIKCgoKUgqaUlJSUAAgK4oKUlIKCgpSClIKUgoKClIKmlIKUgoKmlLS0tILGgoKUgoKCpoKCgoKCgpS4goKCgoKUuLS0tLS0gpTGtLSCgoKUlIKUtIKCgpSUlLS0tIKCgoKCpqSCxoKCgpTGgoKU1qSCgoKCgoKUgsqCgoKUgqaUtLS0tLS0goKCgpSClIKCypKCgpSCgIK2qJKCgqaCgpSC")]
[assembly: go.GoPositionMap("regexp/syntax/simplify.go", "simplify.cs", "AAccAAkCgpSmgoKClIKCgpSCpqaCyoK4ppSCuIK4goKClILcggAGEJKCgoKCyoKCgoKClIKUlILKpgACJAAUBoKmgpSCpoKC")]
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
    public partial struct ErrorCode {}
    public partial struct Flags {}
    public partial struct Inst {}
    public partial struct InstOp {}
    public partial struct Op {}
    public partial struct Prog {}
    [GoValueClone("Sub0", "Rune0")] public partial struct Regexp {}
    public partial struct ΔError {}
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
