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
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.WSAMsg))]
[assembly: GoFieldAccess(typeof(operation))]
[assembly: GoFieldAccess(typeof(syscall_package.WSABuf))]
[assembly: GoFieldAccess(typeof(wsaRsa))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/poll/errno_windows.go", "errno_windows.cs", "AA4oopSkyg==")]
[assembly: GoPositionMap("internal/poll/fd.go", "fd.cs", "ABswsJSC2Mi4lpKClMoACxKwgoLYlpKCgoKClIKCyg==")]
[assembly: GoPositionMap("internal/poll/fd_fsync_windows.go", "fd_fsync_windows.cs", "AAwU4oKUgg==")]
[assembly: GoPositionMap("internal/poll/fd_mutex.go", "fd_mutex.cs", "ACpqAA4CgoKClIKClIIABkyigoKClIKCAAqsAZK4woKUrLKClKrCgpSs0oK8woKUrNKCvMKClKzSgoK6kg==")]
[assembly: GoPositionMap("internal/poll/fd_poll_runtime.go", "fd_poll_runtime.cs", "ABAoxIKCgoKCgoKCAAoQooKCgpSCpoKClIKokoKUpoKClIKmgqaC5oKClIKmgqaCpoKClKaCAAsYgpSkpKSUgsiSqJKoksbCgoKCgqaClJSClIIABR4ADAI=", "", "", "", "177:21")]
[assembly: GoPositionMap("internal/poll/fd_posix.go", "fd_posix.cs", "AA4gooKUqOKClILYwoKUgtzCgpSC3vKClIKCAAcU8oKCgsySgoKC", "38-40:1;49-51:1")]
[assembly: GoPositionMap("internal/poll/fd_windows.go", "fd_windows.cs", "ACRMwoKClIKCgoKClIKCptyCgoKClAAPHILKlMaCppSCyIKCgqKCgpSCgpSCpqa0goIABxCClLgABxYACg6CgoKCyoKCgpSmtIKCgoKCggALGMyCgoKCpu7CgpS4gqaC2KSowoKmlJSUqqKClIKUvOKClKaCgpSCggAIEriClKaClIKSpqaClLiClIKmypSCuNiUtgA0kgEADQKokqrCAA8oAAgCgqaktLS0xqSClIKmhqLcgpTcgoKUlJaUAAQ2ooK4goKmtKSCgqrCgqaCpoKmggALEvKCgpSUgpSmgqaCpoKClLSygpSClMaC6LKCgpSCtoKU9sqygqa4gqaCgoKUgoKClIKCgoKCgoKUgoKUlIKCuJSCgoK4goKCgoKClJSUgqjSlKaClJSCpoKmgoKClILugpSCsoKUgpSClNjSgpSClIKUlJSCgsKCgpSCgpSC2NKClIKUgpSUlIKCwoKClIKClILY0oKUgpSClJSUgoLCgoKUgoKUgtjygoKUlIKUpoKUgoKCgpSCgoKUtLKClLSClLKCtoKClIL+woKCgoKWgoKClIKCmrKCgoKUgoKCgoKCgpSmqNKUpoCCpJSCpoKCgpSC7oKmgoKCgpSCsoKUgpSClIL80oKUgpSCgpSCgrKClIKC2OKAgqSUlKKClKaUgoKCgpSigpSCgpSU2OKAgqSUlKKClKaUgoKCgpSigpSCgpSU2OKAgqSUlKKClKaUgoKCgpSigpSCgpSU3LKGAAgGlIKygpaCgriCgoKmqtKAgqSUgoKCgqaCggAHEIKClNj84oKUgpSUhqKUtLSCgpS0lIKUgpSCgtjigIKklIKAgqSCgpSUgqaCgtjigpSC2OKClILY4oKUgtjigIKkgoKCyqKCgpSClJSC/OKClJSCuAAPUoKUgqSCtNrSgpSUgqaCgoKCsoKUgoKClNjSgpSUgqaCgoKCsoKUgoKU2NKClJSCpoKCgoKygpSCgpQACQiygqaClJSCgoKCgoKCpoKCsoKU2LKCpoKUlIKCgoKUgoKygpTYsoKmgpSUgoKCgpSCgrKClAANBoKCgqaCgoKU", "605-608:1;620-624:2;746-749:1;776-780:1;806-810:1;836-840:1;879-882:1;888-891:2;986-989:1;1016-1019:1;1034-1037:1;1049-1052:2;1071-1074:1;1086-1089:2;1108-1111:1;1123-1126:2;1140-1142:1;1149-1153:1;1312-1319:1;1409-1412:1;1436-1439:1;1462-1465:1;1496-1499:1;1522-1525:1;1548-1551:1")]
[assembly: GoPositionMap("internal/poll/hook_windows.go", "hook_windows.cs", "AAoUtrY=")]
[assembly: GoPositionMap("internal/poll/sendfile.go", "sendfile.cs", "/g==")]
[assembly: GoPositionMap("internal/poll/sendfile_windows.go", "sendfile_windows.cs", "AAoaAAkCgpSUlIKCpoKUlpKClIKCgpSCgpSmgriCggAEEMSCgoKmgoKCgoKClJSCpoKm", "14-16:1;48-58:2;71-80:3", "", "", "", "21:21,22:13,55:21,56:13")]
[assembly: GoPositionMap("internal/poll/sockopt.go", "sockopt.cs", "AA0Y4oKUgtjygpSC2OKClILY4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockopt_windows.go", "sockopt_windows.cs", "AAwU4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockoptip.go", "sockoptip.cs", "AA0Y4oKUgtjigpSC")]
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
    internal partial struct fileKind {}
    internal partial struct operation {}
    internal partial struct pollDesc {}
    [GoValueClone("name")] internal partial struct wsaRsa {}
    public partial struct DeadlineExceededError {}
    public partial struct FD {}
    public partial struct String {}
    public partial struct errNetClosing {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(go.@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
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
[GoMethodHost(typeof(poll_package.errNetClosing)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛerrNetClosing;
[GoMethodHost(typeof(poll_package.fdMutex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛfdMutex;
[GoMethodHost(typeof(poll_package.pollDesc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pollᴛpollDesc;
// </MethodHolders>
