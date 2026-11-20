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
using static go.modernc.org.libc.sys.types_package;

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
[assembly: GoDynamicTypeLift("7374727563747b4669745f696e74657276616c207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f7379732f74797065732e54696d655f743b204674765f6e73656320696e7433323b20465f5f6363676f5f70616431205b345d627974657d3b204669745f76616c7565207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f7379732f74797065732e54696d655f743b204674765f6e73656320696e7433323b20465f5f6363676f5f70616431205b345d627974657d7d", "Itimerspecᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f20696e7436343b2046686920696e7436347d", "X__int128_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f2075696e7436343b204668692075696e7436347d", "X__uint128_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f63616c652075696e747074723b2046776c6f63616c652075696e747074723b2046726566636f756e742075696e747074723b204677726566636f756e742075696e747074727d", "Threadlocaleinfostructᴛ1_Flc_category")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f63696e666f206d6f6465726e632e6f72672f6c6962632f7379732f74797065732e507468726561646c6f63696e666f3b20466d6263696e666f206d6f6465726e632e6f72672f6c6962632f7379732f74797065732e507468726561646d6263696e666f7d", "Localeinfo_structᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b46726566636f756e7420696e7433323b20466c635f636f6465706167652075696e7433323b20466c635f636f6c6c6174655f63702075696e7433323b20466c635f68616e646c65205b365d75696e7433323b20466c635f6964205b365d6d6f6465726e632e6f72672f6c6962632f7379732f74797065732e4c435f49443b20466c635f63617465676f7279205b365d7374727563747b466c6f63616c652075696e747074723b2046776c6f63616c652075696e747074723b2046726566636f756e742075696e747074723b204677726566636f756e742075696e747074727d3b20466c635f636c696b6520696e7433323b20466d625f6375725f6d617820696e7433323b20466c636f6e765f696e746c5f726566636f756e742075696e747074723b20466c636f6e765f6e756d5f726566636f756e742075696e747074723b20466c636f6e765f6d6f6e5f726566636f756e742075696e747074723b20466c636f6e762075696e747074723b20466374797065315f726566636f756e742075696e747074723b20466374797065312075696e747074723b20467063747970652075696e747074723b204670636c6d61702075696e747074723b20467063756d61702075696e747074723b20466c635f74696d655f637572722075696e747074727d", "Threadlocaleinfostructᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f7379732f74797065732e54696d655f743b204674765f6e73656320696e7433323b20465f5f6363676f5f70616431205b345d627974657d", "Timespecᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b46774c616e67756167652075696e7431363b204677436f756e7472792075696e7431363b204677436f6465506167652075696e7431367d", "TagLC_IDᴛ1")]
[assembly: GoTypeAlias("Dev_t", "uint")]
[assembly: GoTypeAlias("Errno_t", "int")]
[assembly: GoTypeAlias("Ino_t", "ushort")]
[assembly: GoTypeAlias("Intptr_t", "long")]
[assembly: GoTypeAlias("Itimerspec", "go.modernc.org.libc.sys.types_package.Itimerspecᴛ1")]
[assembly: GoTypeAlias("LC_ID", "go.modernc.org.libc.sys.types_package.TagLC_IDᴛ1")]
[assembly: GoTypeAlias("LPLC_ID", "go.uintptr")]
[assembly: GoTypeAlias("Localeinfo_struct", "go.modernc.org.libc.sys.types_package.Localeinfo_structᴛ1")]
[assembly: GoTypeAlias("Mode_t", "ushort")]
[assembly: GoTypeAlias("Off32_t", "int")]
[assembly: GoTypeAlias("Off64_t", "long")]
[assembly: GoTypeAlias("Off_t", "long")]
[assembly: GoTypeAlias("Pid_t", "long")]
[assembly: GoTypeAlias("Pthreadlocinfo", "go.uintptr")]
[assembly: GoTypeAlias("Pthreadmbcinfo", "go.uintptr")]
[assembly: GoTypeAlias("Ptrdiff_t", "long")]
[assembly: GoTypeAlias("Rsize_t", "ulong")]
[assembly: GoTypeAlias("Size_t", "ulong")]
[assembly: GoTypeAlias("Ssize_t", "long")]
[assembly: GoTypeAlias("TagLC_ID", "go.modernc.org.libc.sys.types_package.TagLC_IDᴛ1")]
[assembly: GoTypeAlias("Threadlocaleinfostruct", "go.modernc.org.libc.sys.types_package.Threadlocaleinfostructᴛ1")]
[assembly: GoTypeAlias("Threadlocinfo", "go.modernc.org.libc.sys.types_package.Threadlocaleinfostructᴛ1")]
[assembly: GoTypeAlias("Time_t", "long")]
[assembly: GoTypeAlias("Timespec", "go.modernc.org.libc.sys.types_package.Timespecᴛ1")]
[assembly: GoTypeAlias("Uintptr_t", "ulong")]
[assembly: GoTypeAlias("Useconds_t", "uint")]
[assembly: GoTypeAlias("Va_list", "go.uintptr")]
[assembly: GoTypeAlias("Wchar_t", "ushort")]
[assembly: GoTypeAlias("Wctype_t", "ushort")]
[assembly: GoTypeAlias("Wint_t", "ushort")]
[assembly: GoTypeAlias("X__builtin_va_list", "go.uintptr")]
[assembly: GoTypeAlias("X__float128", "double")]
[assembly: GoTypeAlias("X__gnuc_va_list", "go.uintptr")]
[assembly: GoTypeAlias("X__int128_t", "go.modernc.org.libc.sys.types_package.X__int128_tᴛ1")]
[assembly: GoTypeAlias("X__time32_t", "int")]
[assembly: GoTypeAlias("X__time64_t", "long")]
[assembly: GoTypeAlias("X__uint128_t", "go.modernc.org.libc.sys.types_package.X__uint128_tᴛ1")]
[assembly: GoTypeAlias("X_dev_t", "uint")]
[assembly: GoTypeAlias("X_ino_t", "ushort")]
[assembly: GoTypeAlias("X_locale_t", "go.uintptr")]
[assembly: GoTypeAlias("X_locale_tstruct", "go.modernc.org.libc.sys.types_package.Localeinfo_structᴛ1")]
[assembly: GoTypeAlias("X_mode_t", "ushort")]
[assembly: GoTypeAlias("X_off64_t", "long")]
[assembly: GoTypeAlias("X_off_t", "int")]
[assembly: GoTypeAlias("X_pid_t", "long")]
[assembly: GoTypeAlias("X_sigset_t", "ulong")]
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("modernc.org/libc@v1.80.1/sys/types/capi_windows_amd64.go", "capi_windows_amd64.cs", "yg==")]
// </GoSourcePositionMaps>

namespace go.modernc.org.libc.sys;

[GoPackage("types", ImportPath = "modernc.org/libc/sys/types")]
public static partial class types_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("Fit_interval", "Fit_value")] public partial struct Itimerspecᴛ1 {}
    public partial struct Localeinfo_structᴛ1 {}
    public partial struct TagLC_IDᴛ1 {}
    [GoValueClone("Flc_handle", "Flc_id", "Flc_category")] public partial struct Threadlocaleinfostructᴛ1 {}
    public partial struct Threadlocaleinfostructᴛ1_Flc_category {}
    [GoValueClone("F__ccgo_pad1")] public partial struct Timespecᴛ1 {}
    public partial struct X__int128_tᴛ1 {}
    public partial struct X__uint128_tᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(sync.atomic_package));
    // </ImportInitializers>
}
