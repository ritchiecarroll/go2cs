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
using time = go.time_package;
using Δsyscall = go.syscall_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.poll_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/poll/errno_unix.go", "errno_unix.cs", "ABAqopSkpKSU")]
[assembly: GoPositionMap("internal/poll/fd.go", "fd.cs", "ABgwsJSCuMi4lpKClMoACRKwgoK4lpKCgoKClIKCyg==")]
[assembly: GoPositionMap("internal/poll/fd_fsync_darwin.go", "fd_fsync_darwin.cs", "AAwg4oCCpIKCyoKU", "21-31:1")]
[assembly: GoPositionMap("internal/poll/fd_mutex.go", "fd_mutex.cs", "ACdqAA4CgoKClIKClIIABkyigoKClIKCAAisAZKYwoKUrLKClKrCgpSs0oK8woKUrNKCvMKClKzSgoK6kg==")]
[assembly: GoPositionMap("internal/poll/fd_opendir_darwin.go", "fd_opendir_darwin.cs", "AA4e1oKClIKCgoKmgoKUrA==")]
[assembly: GoPositionMap("internal/poll/fd_poll_runtime.go", "fd_poll_runtime.cs", "AA8oxIKCgoKCgoKCAAgQooKCgpSCpoKClIKokoKUpoKClIKmgqaC5oKClIKmgqaCpoKClKaCAAkYgpSkpKSUgqiSqJKokqbCgoKCgqaClJSClIIABR4ADAI=", "", "", "", "168:21")]
[assembly: GoPositionMap("internal/poll/fd_posix.go", "fd_posix.cs", "AAsgooKUqOKClILYwoKUgtzCgpSC3vKClIKCAAUU8oKCgsySgoKC", "38-40:1;49-51:1")]
[assembly: GoPositionMap("internal/poll/fd_unix.go", "fd_unix.cs", "ACpu8qaClIKClIKmlKrmlJSCgqrCggAHELgABxCCpqjigpS4ggAMFuKAgqSC3JSAgqSClIKCgoKCgriC6uiClIKUloKUgoKo4oCCpIKAgqSCgoKClIKCgriC6uKAgqSCgIKkgoKCgpSCgoK4gurigIKkgoCCpIKCgoKUgoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKCpoKCuILq4oCCpIKAgqSCgoKClIKC3JSUgpSCgqaClIL8AAkIgIKkgoKCgoKUgoKUgpSClIKUgvzigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKkgoCCpIKCgpSCgqaClOrigIKklICCpIKCgpSUtIKCAAgOpOrCgpSC3MKClIIADRKSgoKClMq0pqjigpSC3LKo4oKUgtjigIKkgoCCpIKClIL84oCCpIKAgqSCgpSC/JKCgoI=", "186-188:1;633-635:1;644-646:1")]
[assembly: GoPositionMap("internal/poll/fd_unixjs.go", "fd_unixjs.cs", "ABAglNzqwoKCgoKUgtjigpSC3AAIAoCCpIKCgoKCgoLK6uKClII=")]
[assembly: GoPositionMap("internal/poll/fd_writev_libc.go", "fd_writev_libc.cs", "AAse")]
[assembly: GoPositionMap("internal/poll/fstatat_unix.go", "fstatat_unix.cs", "AAwcsoKUgg==", "19-21:1")]
[assembly: GoPositionMap("internal/poll/hook_unix.go", "hook_unix.cs", "AAsYtg==")]
[assembly: GoPositionMap("internal/poll/iovec_unix.go", "iovec_unix.cs", "AAoWgg==")]
[assembly: GoPositionMap("internal/poll/sendfile.go", "sendfile.cs", "/g==")]
[assembly: GoPositionMap("internal/poll/sendfile_unix.go", "sendfile_unix.cs", "AAw8ABMCuNyGgqaCgoKYqAAIAoKUgpSUgqaCyoKClIKCgpQABxCCAAgOgpSCAA0U7IK4AAgKwrbGgoIADBqCxoKCyrY=", "41-43:1;51-53:2;60-62:1", "", "", "", "65:48,66:13")]
[assembly: GoPositionMap("internal/poll/sockopt.go", "sockopt.cs", "AAoY4oKUgtjygpSC2OKClILY4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockopt_unix.go", "sockopt_unix.cs", "AAoY4oKUgg==")]
[assembly: GoPositionMap("internal/poll/sockoptip.go", "sockoptip.cs", "AAoY4oKUgtjigpSC")]
[assembly: GoPositionMap("internal/poll/sys_cloexec.go", "sys_cloexec.cs", "ABIk/IKClIKUgoKU")]
[assembly: GoPositionMap("internal/poll/writev.go", "writev.cs", "AAwg8oKUgoKmgoLKgpSmgoKCgoKClIKCgpSCgqaClIKUlIKCgpSCgoKCgoKUgoKmlIKCpg==")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.poll_package;
using time = go.time_package;
using Δsyscall = go.syscall_package;

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
[GoMethodHost(typeof(poll_package.DeadlineExceededError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛDeadlineExceededError;
[GoMethodHost(typeof(poll_package.FD)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛFD;
[GoMethodHost(typeof(poll_package.SysFile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛSysFile;
[GoMethodHost(typeof(poll_package.errNetClosing)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛerrNetClosing;
[GoMethodHost(typeof(poll_package.fdMutex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛfdMutex;
[GoMethodHost(typeof(poll_package.pollDesc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pollᴛpollDesc;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class pollᴛDeadlineExceededError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref DeadlineExceededError e) => poll_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref DeadlineExceededError e) => poll_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref DeadlineExceededError e) => poll_package.Temporary⁔(ref e);
}
partial class pollᴛFD {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fsync(this ж<FD> Ꮡfd) => poll_package.Fsync⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error incref(this ж<FD> Ꮡfd) => poll_package.incref⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error decref(this ж<FD> Ꮡfd) => poll_package.decref⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readLock(this ж<FD> Ꮡfd) => poll_package.readLock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readUnlock(this ж<FD> Ꮡfd) => poll_package.readUnlock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeLock(this ж<FD> Ꮡfd) => poll_package.writeLock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeUnlock(this ж<FD> Ꮡfd) => poll_package.writeUnlock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readWriteLock(this ж<FD> Ꮡfd) => poll_package.readWriteLock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readWriteUnlock(this ж<FD> Ꮡfd) => poll_package.readWriteUnlock⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool closing(this ж<FD> Ꮡfd) => poll_package.closing⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (uintptr, @string, error) OpenDir(this ж<FD> Ꮡfd) => poll_package.OpenDir⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetReadDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetReadDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetWriteDeadline(this ж<FD> Ꮡfd, time.Time t) => poll_package.SetWriteDeadline⁔(Ꮡfd, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error eofError(this ref FD fd, nint n, error err) => poll_package.eofError⁔(ref fd, n, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Shutdown(this ж<FD> Ꮡfd, nint how) => poll_package.Shutdown⁔(Ꮡfd, how);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fchown(this ж<FD> Ꮡfd, nint uid, nint gid) => poll_package.Fchown⁔(Ꮡfd, uid, gid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Ftruncate(this ж<FD> Ꮡfd, int64 size) => poll_package.Ftruncate⁔(Ꮡfd, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RawControl(this ж<FD> Ꮡfd, Action<uintptr> f) => poll_package.RawControl⁔(Ꮡfd, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Init(this ж<FD> Ꮡfd, @string net, bool pollable) => poll_package.Init⁔(Ꮡfd, net, pollable);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error destroy(this ж<FD> Ꮡfd) => poll_package.destroy⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<FD> Ꮡfd) => poll_package.Close⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetBlocking(this ж<FD> Ꮡfd) => poll_package.SetBlocking⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.Read⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Pread(this ж<FD> Ꮡfd, slice<byte> p, int64 off) => poll_package.Pread⁔(Ꮡfd, p, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, Δsyscall.Sockaddr, error) ReadFrom(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.ReadFrom⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) ReadFromInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> Ꮡfrom) => poll_package.ReadFromInet4⁔(Ꮡfd, p, Ꮡfrom);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) ReadFromInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> Ꮡfrom) => poll_package.ReadFromInet6⁔(Ꮡfd, p, Ꮡfrom);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, nint, Δsyscall.Sockaddr, error) ReadMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => poll_package.ReadMsg⁔(Ꮡfd, p, oob, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, nint, error) ReadMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet4> Ꮡsa4) => poll_package.ReadMsgInet4⁔(Ꮡfd, p, oob, flags, Ꮡsa4);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, nint, error) ReadMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet6> Ꮡsa6) => poll_package.ReadMsgInet6⁔(Ꮡfd, p, oob, flags, Ꮡsa6);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.Write⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Pwrite(this ж<FD> Ꮡfd, slice<byte> p, int64 off) => poll_package.Pwrite⁔(Ꮡfd, p, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToInet4(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet4> Ꮡsa) => poll_package.WriteToInet4⁔(Ꮡfd, p, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteToInet6(this ж<FD> Ꮡfd, slice<byte> p, ж<Δsyscall.SockaddrInet6> Ꮡsa) => poll_package.WriteToInet6⁔(Ꮡfd, p, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteTo(this ж<FD> Ꮡfd, slice<byte> p, Δsyscall.Sockaddr sa) => poll_package.WriteTo⁔(Ꮡfd, p, sa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, error) WriteMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, Δsyscall.Sockaddr sa) => poll_package.WriteMsg⁔(Ꮡfd, p, oob, sa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, error) WriteMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet4> Ꮡsa) => poll_package.WriteMsgInet4⁔(Ꮡfd, p, oob, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, nint, error) WriteMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet6> Ꮡsa) => poll_package.WriteMsgInet6⁔(Ꮡfd, p, oob, Ꮡsa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, Δsyscall.Sockaddr, @string, error) Accept(this ж<FD> Ꮡfd) => poll_package.Accept⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fchmod(this ж<FD> Ꮡfd, uint32 mode) => poll_package.Fchmod⁔(Ꮡfd, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fstat(this ж<FD> Ꮡfd, ж<Δsyscall.Stat_t> Ꮡs) => poll_package.Fstat⁔(Ꮡfd, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, @string, error) Dup(this ж<FD> Ꮡfd) => poll_package.Dup⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WaitWrite(this ref FD fd) => poll_package.WaitWrite⁔(ref fd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteOnce(this ж<FD> Ꮡfd, slice<byte> p) => poll_package.WriteOnce⁔(Ꮡfd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RawRead(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => poll_package.RawRead⁔(Ꮡfd, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RawWrite(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => poll_package.RawWrite⁔(Ꮡfd, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fchdir(this ж<FD> Ꮡfd) => poll_package.Fchdir⁔(Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) ReadDirent(this ж<FD> Ꮡfd, slice<byte> buf) => poll_package.ReadDirent⁔(Ꮡfd, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ж<FD> Ꮡfd, int64 offset, nint whence) => poll_package.Seek⁔(Ꮡfd, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Fstatat(this ж<FD> Ꮡfd, @string name, ж<Δsyscall.Stat_t> Ꮡs, nint flags) => poll_package.Fstatat⁔(Ꮡfd, name, Ꮡs, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name, nint arg) => poll_package.SetsockoptInt⁔(Ꮡfd, level, name, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptInet4Addr(this ж<FD> Ꮡfd, nint level, nint name, /*[4]*/ array<byte> arg) => poll_package.SetsockoptInet4Addr⁔(Ꮡfd, level, name, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptLinger(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.Linger> Ꮡl) => poll_package.SetsockoptLinger⁔(Ꮡfd, level, name, Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) GetsockoptInt(this ж<FD> Ꮡfd, nint level, nint name) => poll_package.GetsockoptInt⁔(Ꮡfd, level, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptByte(this ж<FD> Ꮡfd, nint level, nint name, byte arg) => poll_package.SetsockoptByte⁔(Ꮡfd, level, name, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptIPMreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPMreq> Ꮡmreq) => poll_package.SetsockoptIPMreq⁔(Ꮡfd, level, name, Ꮡmreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetsockoptIPv6Mreq(this ж<FD> Ꮡfd, nint level, nint name, ж<Δsyscall.IPv6Mreq> Ꮡmreq) => poll_package.SetsockoptIPv6Mreq⁔(Ꮡfd, level, name, Ꮡmreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Writev(this ж<FD> Ꮡfd, ж<slice<slice<byte>>> Ꮡv) => poll_package.Writev⁔(Ꮡfd, Ꮡv);
}
partial class pollᴛSysFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref SysFile s) => poll_package.init⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error destroy(this ref SysFile s, nint fd) => poll_package.destroy⁔(ref s, fd);
}
partial class pollᴛerrNetClosing {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this errNetClosing e) => poll_package.Error⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this errNetClosing e) => poll_package.Timeout⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this errNetClosing e) => poll_package.Temporary⁔(e);
}
partial class pollᴛfdMutex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool incref(this ж<fdMutex> Ꮡmu) => poll_package.incref⁔(Ꮡmu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool decref(this ж<fdMutex> Ꮡmu) => poll_package.decref⁔(Ꮡmu);
}
partial class pollᴛpollDesc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error init(this ref pollDesc pd, ж<FD> Ꮡfd) => poll_package.init⁔(ref pd, Ꮡfd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref pollDesc pd) => poll_package.close⁔(ref pd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void evict(this ref pollDesc pd) => poll_package.evict⁔(ref pd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error prepare(this ref pollDesc pd, nint mode, bool isFile) => poll_package.prepare⁔(ref pd, mode, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error prepareRead(this ref pollDesc pd, bool isFile) => poll_package.prepareRead⁔(ref pd, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error prepareWrite(this ref pollDesc pd, bool isFile) => poll_package.prepareWrite⁔(ref pd, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error wait(this ref pollDesc pd, nint mode, bool isFile) => poll_package.wait⁔(ref pd, mode, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error waitRead(this ref pollDesc pd, bool isFile) => poll_package.waitRead⁔(ref pd, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error waitWrite(this ref pollDesc pd, bool isFile) => poll_package.waitWrite⁔(ref pd, isFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void waitCanceled(this ref pollDesc pd, nint mode) => poll_package.waitCanceled⁔(ref pd, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool pollable(this ref pollDesc pd) => poll_package.pollable⁔(ref pd);
}
// </MethodForwarders>
