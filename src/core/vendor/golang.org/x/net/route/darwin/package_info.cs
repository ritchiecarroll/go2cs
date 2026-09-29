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
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.route_package;

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
[assembly: GoTypeAlias("Sys", "ΔSys")]
[assembly: GoTypeAlias("SysType", "ΔSysType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<DefaultAddr, Addr>(Pointer = true)]
[assembly: GoImplement<Inet4Addr, Addr>(Pointer = true)]
[assembly: GoImplement<Inet6Addr, Addr>(Pointer = true)]
[assembly: GoImplement<InterfaceAddrMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceAnnounceMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceMessage, Message>(Pointer = true)]
[assembly: GoImplement<InterfaceMetrics, ΔSys>(Pointer = true)]
[assembly: GoImplement<InterfaceMulticastAddrMessage, Message>(Pointer = true)]
[assembly: GoImplement<LinkAddr, Addr>(Pointer = true)]
[assembly: GoImplement<RouteMessage, Message>(Pointer = true)]
[assembly: GoImplement<RouteMetrics, ΔSys>(Pointer = true)]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/address.go", "address.cs", "ABg4kKSCgqaCgoKUkoKUgoKClIKCgoKUgoKClKaCgpSCgpSCqgARIKKClIKUgpSCgpSCgoKCgpSCgpQABxKQpIKmgoKClIKCggAIFJCkgqaCgoKUgoKCgpSokpiUgpSCgoKClIKkgpSCgoKUgoKUgsqCgoKmpMwAFSiCpoKmlIKYyJSCgqSCgpSUpIKCpIKSlJQACxiQpIKCpoKCgpSClIKCpoKClIKmgoKClIK0grSCtIK2qqKCgpSCgpSCtIKClIK0goKUgrSCgpSCtqaCgoKCgpSClIKClIKCgpSoooKCgpSUgoKUpIKClIKCgpTIgoKUgoKClO4=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/binary.go", "binary.cs", "ABs8goKmgoKCpoKCpoKCgoKCpoKC3IKCpoKCgqaCgqaCgoKCgqaCgg==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/interface.go", "interface.cs", "AB5EkAANHpAADR6Q")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/interface_classic.go", "interface_classic.cs", "AAscgoKUgoKUgoKUAAgSgoKUgoKmgoKUgoKU3IKUlIKCgpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/interface_multicast.go", "interface_multicast.cs", "AAgSgoKUgoKU7oKCgpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/message.go", "message.cs", "ABpCooKUgpKCgoKClIKUgoKUgIKUgoKUgpS2poKU")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/route.go", "route.cs", "ABMsgoKCgoIAOHiSAAoqAAoCgoKCgpKAgqSClIKAiLKClKQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/route_classic.go", "route_classic.cs", "AAscgoKClIKmlIKCgpSUgoKCgoKCgpSClKaCgpSCgpQACRSCgpSCgoKU")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/sys.go", "sys.cs", "ABJEgoKU")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/sys_darwin.go", "sys_darwin.cs", "AAkSgpSkAAkUkKaSAAsckKaSAAYQgoKSgpKCkoKSgpKCkoKk")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/route/syscall.go", "syscall.cs", "AAoY")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net;

[GoPackage("route", ImportPath = "vendor/golang.org/x/net/route")]
public static partial class route_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface binaryByteOrder {}
    internal partial struct binaryBigEndian {}
    internal partial struct binaryLittleEndian {}
    internal partial struct wireFormat {}
    public partial interface Addr {}
    public partial interface Message {}
    public partial interface ΔSys {}
    public partial struct DefaultAddr {}
    [GoValueClone("IP")] public partial struct Inet4Addr {}
    [GoValueClone("IP")] public partial struct Inet6Addr {}
    public partial struct InterfaceAddrMessage {}
    public partial struct InterfaceAnnounceMessage {}
    public partial struct InterfaceMessage {}
    public partial struct InterfaceMetrics {}
    public partial struct InterfaceMulticastAddrMessage {}
    public partial struct LinkAddr {}
    public partial struct RIBType {}
    public partial struct RouteMessage {}
    public partial struct RouteMetrics {}
    public partial struct ΔSysType {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}
