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
using static go.@internal.poll_package;

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
[assembly: GoImplement<DeadlineExceededError, error>(Pointer = true)]
[assembly: GoImplement<errNetClosing, error>]
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
[assembly: go.GoFieldAccess(typeof(splicePipe))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/poll/copy_file_range_linux.go", "copy_file_range_linux.cs", "AA0auAAKFoIACRQAFy6k3IL+")]
[assembly: go.GoPositionMap("internal/poll/copy_file_range_unix.go", "copy_file_range_unix.cs", "AAsa4oKmgoKCgoKUgoK4qAAYKICCpIKAgqSCgoI=", "63-66:1")]
[assembly: go.GoPositionMap("internal/poll/errno_unix.go", "errno_unix.cs", "ABAqopSkpKSU")]
[assembly: go.GoPositionMap("internal/poll/fd.go", "fd.cs", "ABcwwKSAooDIyLiWkoKUygAIEsCigKKAyJaSgoKCgpSCgso=")]
[assembly: go.GoPositionMap("internal/poll/fd_fsync_posix.go", "fd_fsync_posix.cs", "AAoYwoCCpII=", "17-19:1")]
[assembly: go.GoPositionMap("internal/poll/fd_mutex.go", "fd_mutex.cs", "ACZqAA4CgoKClIKClIIABkyigoKClIKCAAisAaKYwoKUrLKClKrCgpSs0oK8woKUrNKCvMKClKzSgoK6kg==")]
[assembly: go.GoPositionMap("internal/poll/fd_poll_runtime.go", "fd_poll_runtime.cs", "AA8oxJKSkpKSkpKSAAgQooKCgpSCpoKClIKokoKUpoKClIKmgqaC1oKClIKmgqaCpoKClKaCAAkYgpSkpKSUgqiSqJKokqbCgoKCgqaAgqSUgpSCAAUeAAwC", "", "", "", "177:21")]
[assembly: go.GoPositionMap("internal/poll/fd_posix.go", "fd_posix.cs", "AAsgooKUqOKAgqSC2MKAgqSC3MKAgqSC3vKAgqSCggAFFPKCgoLMkoKCgg==", "38-40:1;49-51:1")]
[assembly: go.GoPositionMap("internal/poll/fd_unix.go", "fd_unix.cs", "ACpu8qaClIKClIKmlKrmlJSCgqrCggAHELgABxCCpqjigIKkuIIADBbigIKkgtyUgIKkgpSCgoKCgoCCyILq6ICCpIKUloKUgoKo4oCCpIKAgqSCgoKClIKCgILIgurigIKkgoCCpIKCgoKUgoKAgsiC6uKAgqSCgIKkgoKCgpSCgoCCyILq4oCCpIKAgqSCgoKCpoKAgsiC6uKAgqSCgIKkgoKCgqaCgILIgurigIKkgoCCpIKCgoKmgoCCyILq4oCCpIKAgqSCgoKClIKC3JSUgpSCgIK2gpSC/AAJCICCpIKCgoKClIKClIKUgpSClIL84oCCpIKAgqSCgoKUgoCCtoKU6uKAgqSCgIKkgoKClIKAgraClOrigIKkgoCCpIKCgpSCgIK2gpTq4oCCpIKAgqSCgoKUgoCCtoKU6uKAgqSCgIKkgoKClIKAgraClOrigIKkgoCCpIKCgpSCgIK2gpTq4oCCpJSAgqSCgoKUlLSCgIIACQ6k6sKAgqSC3MKAgqSCAAwSkoKCgpTKtKao4oCCpILcsqjigIKkgtjigIKkgoCCpIKClICCAAgM4oCCpIKAgqSCgpSAggAIDJKCgoI=", "186-188:1;633-635:1;644-646:1")]
[assembly: go.GoPositionMap("internal/poll/fd_unixjs.go", "fd_unixjs.cs", "ABAgpNzawoKCgoKUgtjigIKkgtwACAKAgqSCgoKCgoKAgtrq4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/fstatat_unix.go", "fstatat_unix.cs", "AAwcsoCCpII=", "19-21:1")]
[assembly: go.GoPositionMap("internal/poll/hook_cloexec.go", "hook_cloexec.cs", "AAsY")]
[assembly: go.GoPositionMap("internal/poll/hook_unix.go", "hook_unix.cs", "AAsYtg==")]
[assembly: go.GoPositionMap("internal/poll/iovec_unix.go", "iovec_unix.cs", "AAoWgg==")]
[assembly: go.GoPositionMap("internal/poll/sendfile.go", "sendfile.cs", "/g==")]
[assembly: go.GoPositionMap("internal/poll/sendfile_unix.go", "sendfile_unix.cs", "AAw8ABMCgLjshoKmgoKCmKgACAKClICCpJSAgraCyoKClIKCgpQABxCCAAgOgpSAggAOFOyCuAAICsK2xoKCAAwagsaCgsq2", "41-43:1;51-53:2;60-62:1")]
[assembly: go.GoPositionMap("internal/poll/sock_cloexec.go", "sock_cloexec.cs", "AA8gooKClA==")]
[assembly: go.GoPositionMap("internal/poll/sockopt.go", "sockopt.cs", "AAoY4oCCpILY8oCCpILY4oCCpILY4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/sockopt_linux.go", "sockopt_linux.cs", "AAkU4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/sockopt_unix.go", "sockopt_unix.cs", "AAoY4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/sockoptip.go", "sockoptip.cs", "AAoY4oCCpILY4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/splice_linux.go", "splice_linux.cs", "ABpCAAoCgoKUgpKCgoKUAAsYgoKUlIKCgoKmgpQABhoADgKAgqSCgIKkyoKClIKUgoCCAAkmABECgIKkgoCCpILKgoK4goKClIKUgoCCyN7CggASIpSmgoKmgqSokoKClKbGgoKClKiSgoCCAAgQlKiSgg==", "197-199:1")]
[assembly: go.GoPositionMap("internal/poll/writev.go", "writev.cs", "AAwg8oCCpIKAgraCgsqClKaCgoKCgoKUgoKClIKCpoKUgpSUgoKClIKCgoKCgpSCgIK2lIKCpg==")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("poll")]
public static partial class poll_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct fdMutex {}
    internal partial struct pollDesc {}
    internal partial struct splicePipe {}
    internal partial struct splicePipeFields {}
    public partial struct DeadlineExceededError {}
    public partial struct FD {}
    public partial struct String {}
    public partial struct SysFile {}
    public partial struct errNetClosing {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(go.@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸunix() => builtin.initPackage(typeof(go.@internal.syscall.unix_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
