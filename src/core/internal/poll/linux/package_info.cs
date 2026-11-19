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
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(FD))]
[assembly: GoFieldAccess(typeof(fdMutex))]
[assembly: GoFieldAccess(typeof(splicePipe))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/poll/copy_file_range_linux.go", "copy_file_range_linux.cs", "AA0auAAKFoIACRQAFy6k3IL+")]
[assembly: GoPositionMap("internal/poll/copy_file_range_unix.go", "copy_file_range_unix.cs", "AAsa4oKmgoKCgoKUgoK4qAAYKIKUgoKUgoKC", "63-66:1")]
[assembly: GoPositionMap("internal/poll/errno_unix.go", "errno_unix.cs", "ABAqopSkpKSU")]
[assembly: GoPositionMap("internal/poll/fd.go", "fd.cs", "ABswsJSC2Mi4lpKClMoACxKwgoLYlpKCgoKClIKCyg==")]
[assembly: GoPositionMap("internal/poll/fd_fsync_posix.go", "fd_fsync_posix.cs", "AA0YwoKUgg==", "17-19:1")]
[assembly: GoPositionMap("internal/poll/fd_mutex.go", "fd_mutex.cs", "ACpqAA4CgoKClIKClIIABkyigoKClIKCAAqsAZK4woKUrLKClKrCgpSs0oK8woKUrNKCvMKClKzSgoK6kg==")]
[assembly: GoPositionMap("internal/poll/fd_poll_runtime.go", "fd_poll_runtime.cs", "ABAoxIKCgoKCgoKCAAoQooKCgpSCpoKClIKokoKUpoKClIKmgqaC5oKClIKmgqaCpoKClKaCAAsYgpSkpKSUgsiSqJKoksbCgoKCgqaClJSClIIABR4ADAI=", "", "", "", "177:21")]
[assembly: GoPositionMap("internal/poll/fd_posix.go", "fd_posix.cs", "AA4gooKUqOKClILYwoKUgtzCgpSC3vKClIKCAAcU8oKCgsySgoKC", "38-40:1;49-51:1")]
[assembly: GoPositionMap("internal/poll/fd_unix.go", "fd_unix.cs", "AC1u8qaClIKClIKmlKrmlJSCgqrCggAHELgABxCCpqjigpS4ggAQFuKAgqSC3JSAgqSClIKCgoKCgriC6uiClIKUloKUgoKo4oCCpIKAgqSCgoKClIKCgriC6uKAgqSCgIKkgoKCgpSCgoK4gurigIKkgoCCpIKCgoKUgoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKClIKC3JSUgpSCgqaClIL8AAkIgIKkgoKCgoKUgoKUgpSClIKUgvzigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKklICCpIKCgpSUtIKCAAgOpOrCgpSC3MKClIIADxKSgoKClMq0psjigpSC3LKo4oKUgtjigIKkgoCCpIKClIL84oCCpIKAgqSCgpSCAAkMkoKCgg==", "186-188:1;633-635:1;644-646:1")]
[assembly: GoPositionMap("internal/poll/fd_unixjs.go", "fd_unixjs.cs", "ABMglNwACArCgoKCgpSC+OKClILcAAgCgIKkgoKCgoKCgsrq4oKUgg==")]
[assembly: GoPositionMap("internal/poll/fstatat_unix.go", "fstatat_unix.cs", "AA8csoKUgg==", "19-21:1")]
[assembly: GoPositionMap("internal/poll/hook_cloexec.go", "hook_cloexec.cs", "AAsY")]
[assembly: GoPositionMap("internal/poll/hook_unix.go", "hook_unix.cs", "AAsYtg==")]
[assembly: GoPositionMap("internal/poll/iovec_unix.go", "iovec_unix.cs", "AAoWgg==")]
[assembly: GoPositionMap("internal/poll/sendfile.go", "sendfile.cs", "/g==")]
[assembly: GoPositionMap("internal/poll/sendfile_unix.go", "sendfile_unix.cs", "AAw8ABMCuNyGgqaCgoKYqAAIAoKUgpSUgqaCyoKClIKCgpQABxCCAAgOgpSCAA0U7IK4AAgKwrbGgoIADBqCxoKCyrY=", "41-43:1;51-53:2;60-62:1", "", "", "", "65:21,66:13")]
[assembly: GoPositionMap("internal/poll/sock_cloexec.go", "sock_cloexec.cs", "ABAgooKClA==")]
[assembly: GoPositionMap("internal/poll/sockopt.go", "sockopt.cs", "AA0Y4oKUgtjygpSC2OKClILY4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockopt_linux.go", "sockopt_linux.cs", "AAwU4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockopt_unix.go", "sockopt_unix.cs", "AA0Y4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockoptip.go", "sockoptip.cs", "AA0Y4oKUgtjigpSC")]
[assembly: GoPositionMap("internal/poll/splice_linux.go", "splice_linux.cs", "ABpCAAoCgoKUgpKCgoKUAAsYgoKUlIKCgoKmgpQABhoADgKAgqSCgIKkyoKClIKUgoIACCYAEQKAgqSCgIKkgsqCgriCgoKUgpSCgrjewoIAEiKUpoKCpoKkqJKCgpSmxoKCgpSokoKCAAcQlKiSgg==", "197-199:1")]
[assembly: GoPositionMap("internal/poll/writev.go", "writev.cs", "AA8g8oKUgoKmgoLKgpSmgoKCgoKClIKCgpSCgqaClIKUlIKCgpSCgoKCgoKUgoKmlIKCpg==")]
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(poll_package.DeadlineExceededError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛDeadlineExceededError;
[GoMethodHost(typeof(poll_package.FD)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛFD;
[GoMethodHost(typeof(poll_package.SysFile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛSysFile;
[GoMethodHost(typeof(poll_package.errNetClosing)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛerrNetClosing;
[GoMethodHost(typeof(poll_package.fdMutex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛfdMutex;
[GoMethodHost(typeof(poll_package.pollDesc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛpollDesc;
// </MethodHolders>
