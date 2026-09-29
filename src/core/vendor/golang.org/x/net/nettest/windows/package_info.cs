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
global using execꓸError = go.os.exec_package.ΔError;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.nettest_package;

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
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465727d", "chunkedCopy_src")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5772697465727d", "chunkedCopy_dst")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<chunkedCopy_dst, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<chunkedCopy_dst, io_package.Writer>]
[assembly: GoImplement<chunkedCopy_src, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<chunkedCopy_src, io_package.Reader>]
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/net/nettest/conntest.go", "conntest.cs", "ACI80oCSgJKAkoCSgJKAkoCSgJKAkoCSgAAICqKCgoKUgpCSkpKCppKi1pKChIKSgoCCpICCuJKCgIKkgIKkloCCzMKChKKCgoKCgIKClKaCgoKUgoKWgIKCtoCCxJSCkqaAgsSowoSChIKCgqKEgoKCgoKCgsbEqMKEgoSCgoKihIKCgoKCgoLGxKaShIKCgoCCypKEgoKCgILMooSEgoKClIKCgpSE6sKCgoSCsoKCgoKikrKCgoKUgoK0krKCgoKUgoK0sqiigoSCooKCopKigoKClKKShIKCqsKEgoKWgqKCgqKSooKCgoK0kqKCgoKCtLLYooKUiKKCgqKCopKigqKSooKikqKCopKigqKSooKikqKCoqSEgqqigoCCgoKUuPyigoCCpoKCgIKkgIKkgr6ygoKCkoKUgoKCgpSCgqaAggAMEMKCgg==", "31-31:1;32-32:2;33-33:3;34-34:4;35-35:5;36-36:6;37-37:7;38-38:8;39-39:9;40-40:10;41-41:11;53-53:1;54-59:2;55-58:2.1;70-78:1;80-89:2;102-132:1;155-168:1;183-196:1;254-260:1;261-271:2;272-282:3;292-296:1;297-304:2;323-327:1;328-335:2;336-343:3;359-362:1;363-366:2;367-370:3;371-374:4;375-378:5;379-382:6;383-386:7;439-442:1")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/nettest/nettest.go", "nettest.cs", "AB08goSCAAgGgoCCpICCgqSAgqSAgoKkgpiikoKCtsiCgqqigqqigqqigq7CgpiUxpSkotiUpMaUxpSkpKqigKaSxgACENKClIKAgraCtoK2graCgpSk7sKClIKAgraCtoK2graCgpSk6qKCgpSCgpSCgoKu4rakgpSCgpSqooKClLKCpq7CtqSCgpSygpSAgqSUpoKCgpSClICC1oCC2KaCgpSUgILGkpSAgsaAgqSAgsY=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/nettest/nettest_windows.go", "nettest_windows.cs", "AAkSAAcQgoKClIKU")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net;

[GoPackage("nettest", ImportPath = "vendor/golang.org/x/net/nettest")]
public static partial class nettest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct chunkedCopy_dst {}
    internal partial struct chunkedCopy_src {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
