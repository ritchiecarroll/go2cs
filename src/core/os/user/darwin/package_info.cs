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
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static go.os.user_package;

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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4f6e63653b2075202a6f732f757365722e557365723b20657272206572726f727d", "cacheᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<UnknownGroupError, error>]
[assembly: GoImplement<UnknownGroupIdError, error>]
[assembly: GoImplement<UnknownUserError, error>]
[assembly: GoImplement<UnknownUserIdError, error>]
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
[assembly: go.GoPositionMap("os/user/cgo_listgroups_unix.go", "cgo_listgroups_unix.cs", "AA4igoKClIKChJKSgqaAgraCgoKUquSClIKCgpQ=")]
[assembly: go.GoPositionMap("os/user/cgo_lookup_syscall.go", "cgo_lookup_syscall.cs", "ABUugKKAooCigKKAooCigKSAooCkgKSygoKmsoKCprKCgqaygoIABRCA")]
[assembly: go.GoPositionMap("os/user/cgo_lookup_unix.go", "cgo_lookup_unix.cs", "AA8kgqaCgoKChJKClJSClIKUpoKCgpSmgoKEgoKUlIKUgpSmggAKFoKmgoKEgoSSgpSUgpSClKaCgoKUpoKChIKClJSClIKUpoK4AAcUgoK4lJSUrLKCgoKC6pSCgpTcgqiSkoKC", "28-33:1;55-60:1;93-98:1;120-125:1")]
[assembly: go.GoPositionMap("os/user/getgrouplist_syscall.go", "getgrouplist_syscall.cs", "AAsagoKClA==")]
[assembly: go.GoPositionMap("os/user/lookup.go", "lookup.cs", "AAwcntKAkoKUkgAMGKKAgqSqooCCpKqiqqKokg==", "22-22:1")]
[assembly: go.GoPositionMap("os/user/user.go", "user.cs", "AEOKAYLOgs6CzoI=")]
// </GoSourcePositionMaps>

namespace go.os;

[GoPackage("user")]
public static partial class user_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bufferKind {}
    internal partial struct cacheᴛ1 {}
    public partial struct Group {}
    public partial struct UnknownGroupError {}
    public partial struct UnknownGroupIdError {}
    public partial struct UnknownUserError {}
    public partial struct UnknownUserIdError {}
    public partial struct User {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸunix() => builtin.initPackage(typeof(@internal.syscall.unix_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}
