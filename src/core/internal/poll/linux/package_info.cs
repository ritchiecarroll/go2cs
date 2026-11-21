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

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using errors = errors_package;
using static poll_package;
using Δsyscall = syscall_package;
using atomic = go.sync.atomic_package;
using go.sync;
using sync = go.sync_package;
using time = time_package;
using go;
using io = io_package;
using strconv = go.@internal.strconv_package;
using unix = go.@internal.syscall.unix_package;
using go.@internal;
using go.@internal.syscall;
using Δruntime = runtime_package;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class pollᴛerrNetClosing {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this errNetClosing e) => poll_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this errNetClosing e) => poll_package.Timeoutᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this errNetClosing e) => poll_package.Temporaryᴍ(e);
}
partial class pollᴛDeadlineExceededError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref DeadlineExceededError e) => poll_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref DeadlineExceededError e) => poll_package.Timeoutᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Temporary(this ref DeadlineExceededError e) => poll_package.Temporaryᴍ(ref e);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fsync(this ж<FD> Ꮡfd) => poll_package.Fsyncᴍ(Ꮡfd);
}
partial class pollᴛfdMutex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool incref(this ж<fdMutex> Ꮡmu) => poll_package.increfᴍ(Ꮡmu);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool decref(this ж<fdMutex> Ꮡmu) => poll_package.decrefᴍ(Ꮡmu);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error incref(this ж<FD> Ꮡfd) => poll_package.increfᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error decref(this ж<FD> Ꮡfd) => poll_package.decrefᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readLock(this ж<FD> Ꮡfd) => poll_package.readLockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readUnlock(this ж<FD> Ꮡfd) => poll_package.readUnlockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeLock(this ж<FD> Ꮡfd) => poll_package.writeLockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeUnlock(this ж<FD> Ꮡfd) => poll_package.writeUnlockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readWriteLock(this ж<FD> Ꮡfd) => poll_package.readWriteLockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readWriteUnlock(this ж<FD> Ꮡfd) => poll_package.readWriteUnlockᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool closing(this ж<FD> Ꮡfd) => poll_package.closingᴍ(Ꮡfd);
}
partial class pollᴛpollDesc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error init(this ref pollDesc pd, ж<FD> Ꮡfd) => poll_package.initᴍ(ref pd, Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ref pollDesc pd) => poll_package.closeᴍ(ref pd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void evict(this ref pollDesc pd) => poll_package.evictᴍ(ref pd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error prepare(this ref pollDesc pd, nint mode, bool isFile) => poll_package.prepareᴍ(ref pd, mode, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error prepareRead(this ref pollDesc pd, bool isFile) => poll_package.prepareReadᴍ(ref pd, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error prepareWrite(this ref pollDesc pd, bool isFile) => poll_package.prepareWriteᴍ(ref pd, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error wait(this ref pollDesc pd, nint mode, bool isFile) => poll_package.waitᴍ(ref pd, mode, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error waitRead(this ref pollDesc pd, bool isFile) => poll_package.waitReadᴍ(ref pd, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error waitWrite(this ref pollDesc pd, bool isFile) => poll_package.waitWriteᴍ(ref pd, isFile);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void waitCanceled(this ref pollDesc pd, nint mode) => poll_package.waitCanceledᴍ(ref pd, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool pollable(this ref pollDesc pd) => poll_package.pollableᴍ(ref pd);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetReadDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetReadDeadlineᴍ(Ꮡfd, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetWriteDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetWriteDeadlineᴍ(Ꮡfd, t);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error eofError(this ref FD fd, nint n, error err) => poll_package.eofErrorᴍ(ref fd, n, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Shutdown(this ж<FD> Ꮡfd, nint how) => poll_package.Shutdownᴍ(Ꮡfd, how);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fchown(this ж<FD> Ꮡfd, nint uid, nint gid) => poll_package.Fchownᴍ(Ꮡfd, uid, gid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Ftruncate(this ж<FD> Ꮡfd, int64 size) => poll_package.Ftruncateᴍ(Ꮡfd, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error RawControl(this ж<FD> Ꮡfd, Action<uintptr> f) => poll_package.RawControlᴍ(Ꮡfd, f);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Init(this ж<FD> Ꮡfd, @string net, bool pollable) => poll_package.Initᴍ(Ꮡfd, net, pollable);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error destroy(this ж<FD> Ꮡfd) => poll_package.destroyᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<FD> Ꮡfd) => poll_package.Closeᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetBlocking(this ж<FD> Ꮡfd) => poll_package.SetBlockingᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Read(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.Readᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Pread(this ж<FD> Ꮡfd, slice<byte> p, int64 off) => poll_package.Preadᴍ(Ꮡfd, p, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, Δsyscall.Sockaddr, error) ReadFrom(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.ReadFromᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) ReadFromInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> from) => poll_package.ReadFromInet4ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) ReadFromInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> from) => poll_package.ReadFromInet6ᴍ(Ꮡfd, p, from);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, nint, Δsyscall.Sockaddr, error) ReadMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => poll_package.ReadMsgᴍ(Ꮡfd, p, oob, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, nint, error) ReadMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet4> sa4) => poll_package.ReadMsgInet4ᴍ(Ꮡfd, p, oob, flags, sa4);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, nint, error) ReadMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet6> sa6) => poll_package.ReadMsgInet6ᴍ(Ꮡfd, p, oob, flags, sa6);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Write(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.Writeᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Pwrite(this ж<FD> Ꮡfd, slice<byte> p, int64 off) => poll_package.Pwriteᴍ(Ꮡfd, p, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> sa) => poll_package.WriteToInet4ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteToInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> sa) => poll_package.WriteToInet6ᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteTo(this ж<FD> Ꮡfd, slice<byte> p, Δsyscall.Sockaddr sa) => poll_package.WriteToᴍ(Ꮡfd, p, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, error) WriteMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, Δsyscall.Sockaddr sa) => poll_package.WriteMsgᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, error) WriteMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet4> sa) => poll_package.WriteMsgInet4ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint, error) WriteMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet6> sa) => poll_package.WriteMsgInet6ᴍ(Ꮡfd, p, oob, sa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, Δsyscall.Sockaddr, @string, error) Accept(this ж<FD> Ꮡfd) => poll_package.Acceptᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fchmod(this ж<FD> Ꮡfd, uint32 mode) => poll_package.Fchmodᴍ(Ꮡfd, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fstat(this ж<FD> Ꮡfd, ж<Δsyscall.Stat_t> s) => poll_package.Fstatᴍ(Ꮡfd, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, @string, error) Dup(this ж<FD> Ꮡfd) => poll_package.Dupᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WaitWrite(this ref FD fd) => poll_package.WaitWriteᴍ(ref fd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteOnce(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.WriteOnceᴍ(Ꮡfd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error RawRead(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => poll_package.RawReadᴍ(Ꮡfd, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error RawWrite(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => poll_package.RawWriteᴍ(Ꮡfd, f);
}
partial class pollᴛSysFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref SysFile s) => poll_package.initᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error destroy(this ref SysFile s, nint fd) => poll_package.destroyᴍ(ref s, fd);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fchdir(this ж<FD> Ꮡfd) => poll_package.Fchdirᴍ(Ꮡfd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) ReadDirent(this ж<FD> Ꮡfd, slice<byte> buf) => poll_package.ReadDirentᴍ(Ꮡfd, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) Seek(this ж<FD> Ꮡfd, int64 offset, nint whence) => poll_package.Seekᴍ(Ꮡfd, offset, whence);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fstatat(this ж<FD> Ꮡfd, @string name, ж<Δsyscall.Stat_t> s, nint flags) => poll_package.Fstatatᴍ(Ꮡfd, name, s, flags);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name, nint arg) => poll_package.SetsockoptIntᴍ(Ꮡfd, level, name, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptInet4Addr(this ж<FD> Ꮡfd, nint level, nint name, /*[4]*/ array<byte> arg) => poll_package.SetsockoptInet4Addrᴍ(Ꮡfd, level, name, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptLinger(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.Linger> l) => poll_package.SetsockoptLingerᴍ(Ꮡfd, level, name, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) GetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name) => poll_package.GetsockoptIntᴍ(Ꮡfd, level, name);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptIPMreqn(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreqn> mreq) => poll_package.SetsockoptIPMreqnᴍ(Ꮡfd, level, name, mreq);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptByte(this ж<FD> Ꮡfd, nint level, nint name, byte arg) => poll_package.SetsockoptByteᴍ(Ꮡfd, level, name, arg);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptIPMreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreq> mreq) => poll_package.SetsockoptIPMreqᴍ(Ꮡfd, level, name, mreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetsockoptIPv6Mreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPv6Mreq> mreq) => poll_package.SetsockoptIPv6Mreqᴍ(Ꮡfd, level, name, mreq);
}
partial class pollᴛFD {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) Writev(this ж<FD> Ꮡfd, ж<slice<slice<byte>>> Ꮡv) => poll_package.Writevᴍ(Ꮡfd, Ꮡv);
}
// </GlyphForwarders>
