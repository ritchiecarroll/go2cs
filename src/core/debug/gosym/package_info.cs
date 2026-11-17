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
using static go.debug.gosym_package;

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
[assembly: GoImplement<DecodingError, error>(Pointer = true)]
[assembly: GoImplement<UnknownFileError, error>]
[assembly: GoImplement<UnknownLineError, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(LineTable))]
[assembly: GoFieldAccess(typeof(Obj))]
[assembly: GoFieldAccess(typeof(Sym))]
[assembly: GoFieldAccess(typeof(go.debug.gosym_package.Func))]
[assembly: GoFieldAccess(typeof(lineFromAline_stackEnt))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/gosym/pclntab.go", "pclntab.cs", "AE+kAQAMEoKCgoKUgoKUgoK0tLSCpJSmgpKs0oKUgq7igpSCgqYABBYACAIABB4ADgKCqqKClKjSgoKCAAcQlIKUypamyoKUlLS0tLS0tLS0lKaClIaolIKCgoKCgoKCgoK0goKCgoKCgoKCtIKCgoKCgoKCgoK0xKbkgoK4goKCgoKCgoKCggAHEJTYsoKClJaCqLKSgoKCgoKCpoKokoKUgoKCqJKClIKCgqiSqJKClKiyAA0WkqiSgoKUqJKokoKUAAwUsoLIkqi2ppSmgoKCgpqygriCgpSCgsjCgoKUgpSUgoKCgqyylIKCgoKmAAIS4oKmgoKCgoKCgoKCgpTKgpSCgpSCpqaUqOKCgriCgpSCgujigoK4goKUgoKCgoKUpoKUgoKU6OKCgriCgoKssoKCgoKCgpSUlIKCpujigpSClJSCgoKmgoKCgqaiqvKCgriCgrQ=", "203-206:1;250-252:2;253-255:3;302-304:1;336-338:1;569-573:1;586-590:1;618-622:1;685-687:1", "", "367=nameOff/1/1/3,funcName/1/1/3", "", "227:25,229:17,352:25,353:17,650:21,651:13,676:21,677:13,717:21,718:13,800:25,801:17")]
[assembly: GoPositionMap("debug/gosym/symtab.go", "symtab.cs", "ACRGgJ7SgoKUgpSUAAgKotyCuIKmgoKmgpSssqaCgriUgpTKgqiSgoKCgsqmlAA7eoKCloKClIKCyoK0tIKkgoKClIKClJSCgoKClIKCgoKUlIKCgoKmgoKUgriCgoKCgpSClIKUgoKmgoKUgsqCgpSCgpSCgqiSgoKCgqaUgoKCgsiClIKClIKCpoKUlK7igoKClIKmgoKUgoKCgoKSgoKCgoKCgqaCgoKCgpSClLSClIKCgoKUgpS2lLSkgpSCpoKUgpSCgpTKgoKClJSmgpSmgoKWkoKCpoKmgoKCgpSClNqClIKoooKCgpS0tPqCgoKCgoKCgoK4goKClIKClLSCgrSCgrbIgpSClMqigoKCgpS0pLaq4oKUgoKUlKzygoKmgoKClKaCgpSCgoKCpqq0goKUgsiqooKCgqaokoKClILIABEcAAkCAAAQlIKCgpTIyIKUgsjogpSmgoKmlIK4goKCgoKClKaCgoKCyIKU1pQADhYADRSCAA4WgoKClII=", "339-342:1;356-402:2")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("gosym")]
public static partial class gosym_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct funcData {}
    internal partial struct funcTab {}
    [GoLocalName("stackEnt")] internal partial struct lineFromAline_stackEnt {}
    internal partial struct sym {}
    internal partial struct version {}
    public partial struct DecodingError {}
    public partial struct Func {}
    public partial struct LineTable {}
    public partial struct Obj {}
    public partial struct Sym {}
    public partial struct Table {}
    public partial struct UnknownFileError {}
    public partial struct UnknownLineError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gosym_package.DecodingError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛDecodingError;
[GoMethodHost(typeof(gosym_package.LineTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛLineTable;
[GoMethodHost(typeof(gosym_package.Obj)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛObj;
[GoMethodHost(typeof(gosym_package.Sym)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛSym;
[GoMethodHost(typeof(gosym_package.Table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛTable;
[GoMethodHost(typeof(gosym_package.UnknownFileError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛUnknownFileError;
[GoMethodHost(typeof(gosym_package.UnknownLineError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛUnknownLineError;
[GoMethodHost(typeof(gosym_package.funcData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛfuncData;
[GoMethodHost(typeof(gosym_package.funcTab)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gosymᴛfuncTab;
// </MethodHolders>
