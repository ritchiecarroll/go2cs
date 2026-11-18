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
using static go.regexp_package;

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
[assembly: GoImplement<inputBytes, input>(Pointer = true)]
[assembly: GoImplement<inputReader, input>(Pointer = true)]
[assembly: GoImplement<inputString, input>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Regexp))]
[assembly: GoFieldAccess(typeof(bitState))]
[assembly: GoFieldAccess(typeof(entry))]
[assembly: GoFieldAccess(typeof(go.regexp.syntax_package.Inst))]
[assembly: GoFieldAccess(typeof(go.regexp.syntax_package.Prog))]
[assembly: GoFieldAccess(typeof(inputs))]
[assembly: GoFieldAccess(typeof(machine))]
[assembly: GoFieldAccess(typeof(onePassInst))]
[assembly: GoFieldAccess(typeof(onePassMachine))]
[assembly: GoFieldAccess(typeof(onePassProg))]
[assembly: GoFieldAccess(typeof(queue))]
[assembly: GoFieldAccess(typeof(thread))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("regexp/backtrack.go", "backtrack.cs", "AC9igoKClKaigqqigpSqoszSlIKUpoKClIKmgpSUgqaClJSCvKKCgpSCquaC2sKUgoKUgoKCAAgSgoKClISUlLQACxSUgoKUgoLapoKCgraCgraCgpSCgraCgpSCgraCgpSCgraCgpSCgraUgpSUgpSCyIKClIK2gtqC3IKUgriCuIK4yKiygoKUlKaCgqaCgpSCggAIFIKClIKCgpSmgpSUlJSCpoKCgg==")]
[assembly: GoPositionMap("regexp/exec.go", "exec.cs", "ADlwooKmooKmooKCgqamgoKClLiCgpSClMaCgpSqooKCgpSClIIACxSCxoKClIKCgpSUgoKUlIKUgoKClJSCgpSUgpSClJTM0oKClIKClIKCkoKClIKClJSCgpSUlJSUgoKUgoKmgoKUlIKCgpSmlIKCgpSUgqiygoKmAAIQ8oKCgoKClIKClIKClLaCgpSUgoKmlLa0tLSkgpSCpq7ygoKUgqaCgoKCgpSClPiCgrSCgsaCtIKCgoKUgsaClJSClIKkAAwUgoKClKaigsiygoKmgoKUpoKCppSCkoKClIKClJSClJaClIKCgoKUgoKClLSCgoKUtILGggAICoLYgrS0tIKUtIKUpIKUgoKCgriCgoKmgoKokq7ilKaCpoKUgqaClIKCgqaCgg==", "", "", "490=canCheckPrefix/1/1/1")]
[assembly: GoPositionMap("regexp/onepass.go", "onepass.cs", "ACVSAAkCgoKUgoKCgqaCqJKCgpSGlK7igoKUgpSmgoKUpAALFIKmooKCpoKCpoKClKaCgriCgpSCgsaCAAwcgpbSgoKClJaCgoKCgoK4goKClIKCgoKmgpS0tLSkgqbYkoLGtILcksqCAAcQgpTGgpSCgoKCgqaUlLiCgoKCgoKClILKgsimgp4ACASCpgAGEsKCgoKUgpSUgoKCgqaCgpSCgriUgoLGgpSCgoLGgoKCgoLGtIKClIKCgoKUgoKCgoKUlJSCgoKUtIKClIKUgoKCgpSUlIKCgpS0goKUgoK0goKUgoKCgramgoKCgoKCgoKmgoKmruKCpoSUgoKCgsqCgpSCxoLGgoKU7KaUgpQ=", "170-175:1;178-187:2;308-434:1", "", "", "", "185:21,186:13")]
[assembly: GoPositionMap("regexp/regexp.go", "regexp.cs", "AGnOAZIAAhYACAKCAAQaAAoCAAIsABMCAAQS4saCgoKUgpSCgoKUgoKUAAoWgoKUlKaCpoKCgpSUAAgU3tKCgpSCgoKCgtyCgpSCgpSosoKCgsiSlKSkgoKClKakpKSCgpSkgoKCgoKmvrKCgpSssoKClKaCgpTIkgACENIAAhTygoKCuAAXKIKClKaCpqKmoqaClIKmgpQACxCCgpSmgqaipqKmgpSCpoKUAA0UgoKWgoKClIKmgqaCpoKmgsyyqqKqoqqizNKCgpSs0oKClKzSgoKUzNKCgpSGrLIAAhLChqaigoKCgoKUlIKmgoKCgriClO6ClJaSgpSUgoKmlMqClKas0oKClIKigpSUrLIAAxLCAAgSkqaCgr60koKCuIKmgoKCgoKClIKUAAQQ0pSUgoKUrNKCgpSmgoKCpoKUppSCgoKUgpSClKaUlIKCzqKCgoKUrsKCgpQAAhDSgoKClK7CgoKUAAIQ0oKClAACEPKCgoKUgoKCpgACKAARAqyypoKCgoKUgoKUgoKUgpSClIKCgoKUuIKCgpSUyoLMAAgCgpSCgoKUgoKCgpSUlJSCgpSUuIKCgoKUpoKmgoIABBDyAAIQ8oKCgpSCgoKmAAIQ8gACEPIACBLCgpSCkoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCkoKUgoKCppSuwoKUgoKClJSuwoKUgoKClIKCgqaUAAIQ0oKUgoKClJQAAiYAEgSCpoKmgpSCgoKCpoKClKaCpgACEuKuwqqigoKUgg==", "557-559:1;567-569:1;577-579:1;657-662:1;670-672:1;680-682:1;1062-1067:1;1080-1085:1;1098-1103:1;1116-1121:1;1134-1145:1;1158-1163:1;1176-1187:1;1201-1206:1", "", "194=compileOnePass/1/1/3,StartCond/1/1/6,minInputLen/1/1/9")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("regexp")]
public static partial class regexp_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface input {}
    internal partial struct bitState {}
    internal partial struct entry {}
    internal partial struct inputBytes {}
    internal partial struct inputReader {}
    internal partial struct inputString {}
    internal partial struct inputs {}
    internal partial struct job {}
    internal partial struct lazyFlag {}
    internal partial struct machine {}
    internal partial struct onePassMachine {}
    internal partial struct onePassProg {}
    internal partial struct queue {}
    internal partial struct queueOnePass {}
    internal partial struct thread {}
    public partial struct Regexp {}
    public partial struct onePassInst {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸregexpꓸsyntax() => builtin.initPackage(typeof(regexp.syntax_package));
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
[GoMethodHost(typeof(regexp_package.Regexp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛRegexp;
[GoMethodHost(typeof(regexp_package.bitState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛbitState;
[GoMethodHost(typeof(regexp_package.inputBytes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputBytes;
[GoMethodHost(typeof(regexp_package.inputReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputReader;
[GoMethodHost(typeof(regexp_package.inputString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputString;
[GoMethodHost(typeof(regexp_package.inputs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputs;
[GoMethodHost(typeof(regexp_package.lazyFlag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛlazyFlag;
[GoMethodHost(typeof(regexp_package.machine)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛmachine;
[GoMethodHost(typeof(regexp_package.queueOnePass)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛqueueOnePass;
// </MethodHolders>
