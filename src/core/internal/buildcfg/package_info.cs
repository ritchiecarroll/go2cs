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
using static go.@internal.buildcfg_package;
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
[assembly: GoFieldAccess(typeof(ExperimentFlags))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/buildcfg/cfg.go", "cfg.cs", "ABkusqKioqKioqKioqKi1AAHEJKCkriCgpTmgqSkpKSUguaCgpSUgpSCqqKCgpSClIKUgpSClIKClIKUqqKCgpQABxCCgoKUlAAOBqKYgpSUlIKCgoKUgoKmlLS0tILIgpQADh6CgoKUgpQACgayqoKUgoKCgqaCgoKmppS2lLSUtuaigqq0gqaCpoamgpSCgpSU+IKklILmgqSUguaCpKSklILmgqSkpJSCgoaCAAUSsoLmooIADRC25oKmgoKCpqLcgpT6oqSkpKSkpKSkpJSmwqSkgoKUtIKClLSCgoKCpoKCpqSkpIKClLSCgpSClKSUlIKU", "326-328:1")]
[assembly: GoPositionMap("internal/buildcfg/exp.go", "exp.cs", "ACA+goKCgpQAEBze2tKUgrQADBiU7gAHEJSCgoKCgv6SgriSgpS4gpSCgpSCgpS4goKmgoKmgpSqoq7CgoKCgpSCgoKCgoKUgoKUuKqiqqI=", "112-115:1", "105=reflect.Value.SetBool", "33=Invoke/1/1/7")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.buildcfg_package;

[GoPackage("buildcfg")]
public static partial class buildcfg_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct ExperimentFlags {}
    public partial struct Goarm64Features {}
    public partial struct GoarmFeatures {}
    public partial struct gowasmFeatures {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(buildcfg_package.ExperimentFlags)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildcfgᴛExperimentFlags;
[GoMethodHost(typeof(buildcfg_package.Goarm64Features)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildcfgᴛGoarm64Features;
[GoMethodHost(typeof(buildcfg_package.GoarmFeatures)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildcfgᴛGoarmFeatures;
[GoMethodHost(typeof(buildcfg_package.gowasmFeatures)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildcfgᴛgowasmFeatures;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class buildcfgᴛExperimentFlags {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<ExperimentFlags> Ꮡexp) => buildcfg_package.String⁔(Ꮡexp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Enabled(this ж<ExperimentFlags> Ꮡexp) => buildcfg_package.Enabled⁔(Ꮡexp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> All(this ж<ExperimentFlags> Ꮡexp) => buildcfg_package.All⁔(Ꮡexp);
}
partial class buildcfgᴛGoarm64Features {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Goarm64Features g) => buildcfg_package.String⁔(g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Supports(this Goarm64Features g, @string s) => buildcfg_package.Supports⁔(g, s);
}
partial class buildcfgᴛGoarmFeatures {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this GoarmFeatures g) => buildcfg_package.String⁔(g);
}
partial class buildcfgᴛgowasmFeatures {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this gowasmFeatures f) => buildcfg_package.String⁔(f);
}
// </MethodForwarders>
