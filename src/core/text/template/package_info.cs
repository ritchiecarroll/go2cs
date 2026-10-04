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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using urlꓸError = go.net.url_package.ΔError;
using parse = go.text.template.parse_package;
// </ImportedTypeAliases>

using go;
using static go.text.template_package;

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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4f6e63653b2076206d61705b737472696e675d7265666c6563742e56616c75657d", "builtinFuncsOnceᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ExecError, error>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<parse.Tree, ж<parse.Tree>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("text/template/exec.go", "exec.cs", "ABgslIKClAATKpKokqiSqqKCgoKmqJKokoKCpoL+lJSCqJKqogAOHoKmgqiygoKUgpQADRyCzsKCgpS0tMTWAAISAAkCgoKUAAIaAAoCpuKCgoKUyoKUggAIDgAJAoKUgoKCgoKUgpSUlOqSmsKCmKKCxua0tILGtLSAgtbEzOKCgoKClIKClKSCtKqyprKUlJSkpKSkpKSkpKSm0oKCgIK2gpSCooKmgpTKpoKCuKaClICCtqKStoKClIKClJSClMSClIKUxIKUgoKUxIKUgoKUgoKCgpSUgpTExIKCgpSCgqaUgpSUgoKCgoLKpoKUlLSkgrSkooKCgpSCpoKSgpSCAAIWAAsCgpSCgoKUgqaCgpSmpoKCuKKClKSmtpKklIKClKSktKSUgt4ACQiClKqmgoKUprSmgqaCpqKCpqKCgpSCpoKmtIKCgoKUrLKCgqamooKCgoKUrLKCkpSUgoKmgrqCgpSAgqSUlIKCgoKUgqaClLiCkoKUgoK4pMbGgoKApraCxoKogoKcsoKUgoKClIKCgoKkgpSAgqaCgpSogoKCgoKmpu6UqJSCgqaCgoK4goKCprimuoKClJSUgpamgoKUqJKUpKSokoKUlJSUlIKUgoKCggAHEJSCgsa0xqaCgpSkgpS0pKSkpJSUpKSkpIK2grakpIKmgoKAgoKCpIKmgoKAgoKCpIKmgoKAgoKCpIKmgoKAgoKCpIKmgoKAgoKCpIKmgoCCgoKkgqaCgpSkpKSmtKSktJSCrLKCgqauwoKUgpSqooKCgpSCguyigpSCloKClJTI", "354-358:1;363-396:2;389-394:2.1;794-799:1")]
[assembly: go.GoPositionMap("text/template/funcs.go", "funcs.cs", "ABdOwgAlQKKClKiSgoKokoKClIKClICCpLyigrqkgKSkpKTakoKUgsaktqgACAKCgoKAgraAgqTqooKClJSClIKClKaClKSkqJKClKSkpKSClAACEPKCgpSigoKAgqSUgoKUpIKClICClMikxgACEgAIAoKClIKUgpSClKSktoKCgoKUpoKUgqaClKyygoKUlKSqsqrCgoKUgoKWgIKkgoKCgpSUgqaCopSCgpaCgIK2quKCgIKAgpTYgoKU6pKCqsKqwqiSAAQQgoIACx6ClKSkpKSkpKiSgpSUpKqigoKCpqiygoKUgqKCgoKUlLS0gtiUpKSkpKSkgpSClIKUyIKmqKSCqJKCgoKUgoKClIKUlLS0tpSkpKSkpLaopIKClKikgoKUqKSCgpS8goKCgoKYkoKCgpS0tLS0tLS0goKUqKSClIKCqsK8goSCgoKCgoKYkoKChJSUhKaUtLS0tLS0tIKCgtiCgpSUlJSopIKUgoKmgpSkqsKqwgACFPKClIKUgoKCgqaU", "73-75:1;366-374:1")]
[assembly: go.GoPositionMap("text/template/helper.go", "helper.cs", "ABEw8oKUAAIYAAsCAAIcAA4CgqrSlJSCgoKUjuKClIKUlIKCpgACGAAJAgACFgAKAoKokoKClIKUruKu8oKmgoKCgoKUgpSUpsKCgqaCsoKC", "173-177:1")]
[assembly: go.GoPositionMap("text/template/option.go", "option.cs", "ABVUABUCgoKUpoKCpoCClJSCpIKkgug=")]
[assembly: go.GoPositionMap("text/template/template.go", "template.cs", "ACVOkqaCqJIAAhTygtyokoKCgoKCAAMUAAwCgoKClIKCgoKCpoKUgoKCgtiSAAgaAAkCgoKCgoKmgpTY4oKmgoKCgpQABRDygoKCAAISAAsCgoKCgoLa8oKUgoIABRoADAKCgoKCgqaCgIK2rOKClICmpII=")]
// </GoSourcePositionMaps>

namespace go.text;

[GoPackage("template")]
public static partial class template_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct builtinFuncsOnceᴛ1 {}
    internal partial struct common {}
    internal partial struct kind {}
    internal partial struct missingKeyAction {}
    internal partial struct missingValType {}
    internal partial struct option {}
    internal partial struct state {}
    internal partial struct variable {}
    internal partial struct ΔwriteError {}
    public partial struct ExecError {}
    public partial struct FuncMap {}
    public partial struct Template {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfmtsort() => builtin.initPackage(typeof(@internal.fmtsort_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(go.text.template.parse_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
