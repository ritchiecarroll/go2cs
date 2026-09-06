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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static go.@internal.syscall.unix_package;

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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/syscall/unix/arc4random_darwin.go", "arc4random_darwin.cs", "ABIcpqSClA==")]
[assembly: go.GoPositionMap("internal/syscall/unix/at_libc2.go", "at_libc2.cs", "ABEcgqaCpoKopqY=")]
[assembly: go.GoPositionMap("internal/syscall/unix/eaccess_darwin.go", "eaccess_darwin.cs", "AAwamJKCgpSCgpSmgg==")]
[assembly: go.GoPositionMap("internal/syscall/unix/fcntl_unix.go", "fcntl_unix.cs", "AAsi9IKCgpQ=")]
[assembly: go.GoPositionMap("internal/syscall/unix/kernel_version_other.go", "kernel_version_other.cs", "AAgSgg==")]
[assembly: go.GoPositionMap("internal/syscall/unix/net.go", "net.cs", "AAsguLi4uLi4uA==")]
[assembly: go.GoPositionMap("internal/syscall/unix/net_darwin.go", "net_darwin.cs", "ACNWAAYkAAQSpIIACRSCgpSopIKmqKSCqKampqbupIKmgpSopILMpILugpQ=")]
[assembly: go.GoPositionMap("internal/syscall/unix/nonblocking_unix.go", "nonblocking_unix.cs", "AAoWgoKClKaC")]
[assembly: go.GoPositionMap("internal/syscall/unix/pty_darwin.go", "pty_darwin.cs", "AAwapIKCgpSopIKCgpSopIKCyoKUgoKCpqikgoKClA==")]
[assembly: go.GoPositionMap("internal/syscall/unix/tcsetpgrp_bsd.go", "tcsetpgrp_bsd.cs", "AAseqsI=")]
[assembly: go.GoPositionMap("internal/syscall/unix/user_darwin.go", "user_darwin.cs", "AAwcpIKmgpQAGToABB4ABB4ABB4ABB6kgpSClA==")]
// </GoSourcePositionMaps>

// Dynamically imported C entry points are recorded here, one `GoCgoImportDynamic` attribute
// per `//go:cgo_import_dynamic` pragma this package binds to a trampoline declaration, so
// that `abi.FuncPCABI0` of that trampoline resolves to the REAL address of the exported
// symbol rather than to a token. The value is dereferenced by design - the trampoline's
// caller jumps to it - which is why a stub carrying no record here is left a loud throw
// instead: an address that is merely plausible is fatal at the first call.

// <CgoDynamicImports>
[assembly: go.GoCgoImportDynamic("libc_arc4random_buf_trampoline", "arc4random_buf", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_faccessat_trampoline", "faccessat", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_freeaddrinfo_trampoline", "freeaddrinfo", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_gai_strerror_trampoline", "gai_strerror", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getaddrinfo_trampoline", "getaddrinfo", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getgrgid_r_trampoline", "getgrgid_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getgrnam_r_trampoline", "getgrnam_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getgrouplist_trampoline", "getgrouplist", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getnameinfo_trampoline", "getnameinfo", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpwnam_r_trampoline", "getpwnam_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpwuid_r_trampoline", "getpwuid_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_grantpt_trampoline", "grantpt", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_posix_openpt_trampoline", "posix_openpt", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_ptsname_r_trampoline", "ptsname_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sysconf_trampoline", "sysconf", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_unlockpt_trampoline", "unlockpt", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libresolv_res_9_nclose_trampoline", "res_9_nclose", "/usr/lib/libresolv.9.dylib")]
[assembly: go.GoCgoImportDynamic("libresolv_res_9_ninit_trampoline", "res_9_ninit", "/usr/lib/libresolv.9.dylib")]
[assembly: go.GoCgoImportDynamic("libresolv_res_9_nsearch_trampoline", "res_9_nsearch", "/usr/lib/libresolv.9.dylib")]
// </CgoDynamicImports>

namespace go.@internal.syscall;

[GoPackage("unix")]
public static partial class unix_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Addrinfo {}
    public partial struct Group {}
    public partial struct Passwd {}
    [GoValueClone("unexported")] public partial struct ResState {}
    // </TypeAccessibility>
}
