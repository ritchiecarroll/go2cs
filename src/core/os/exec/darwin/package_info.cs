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
using io = go.io_package;
using os = go.os_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.os.exec_package;
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
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "Cmd_cachedLookExtensions")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<ExitError, error>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<prefixSuffixSaver, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<wrappedError, error>]
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
[assembly: GoFieldAccess(typeof(Cmd))]
[assembly: GoFieldAccess(typeof(ExitError))]
[assembly: GoFieldAccess(typeof(go.io.fs_package.PathError))]
[assembly: GoFieldAccess(typeof(prefixSuffixSaver))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("os/exec/exec.go", "exec.cs", "AHzuAYKmygAHEIKmggDaAdwDggAHMAAYAsqAgqaCgoKCgpSmgpSmgoKCgpSCgoKU2oKCuJSClAAIEoKUpgACGAALAoKUgoKWrsKUpoKCgoKUqsKClNaCgpSmooKCgpSCpoCCtoKCpoKCkoKClIKUlKaCpoKClK7CgoKClIKmgIK2goKmgoKSgoKUpoKCAAMiAA0CgpQADxIADQiCpoKCgpSCgoK4gpSCgpSUgoK4AAEgAA4CgoK4gpSCpOqCgoKUgoKClIKCgpSClIKCptyClKaCgoSKgoKilIKClIKClLgACBKCgoKmAAUU8rTYgoK4gsrcgoKmgtjYgsqCgtyCyIIADRimgoLspgASKIIACCgAEwKClIKmgoKUlIKCgriCuLiUgpQAAhQADAKCgpSmgqaCgqa2/Ka0poKmAAsS8oKUgpSCgqaCgoKm6sKClIKUgoKCggAIEuKClIKUgoKUgoKCAAYWAAgCgpSClIKClIKCggAGFgAIAoKUgpSCgpSCgoIADSjygqaCgpSmgoKCgoKCpqrCgoKClAAIBoKClIKUgoKCgoKCgoKsspSCgoKCuIIADxqCgoLsgoKUqrSCAAIQ0uy2ooKCgriCgqaCuJSCuJSUgoKUgqaCuIKCpuyygpSCgoKUlKYACRSYopSU", "430-443:1;490-492:1;518-520:1;552-561:1;601-605:1;650-659:1;756-769:2;968-973:1", "", "", "", "518:54,519:13,678:52,679:13,1023:62,1024:13")]
[assembly: GoPositionMap("os/exec/exec_unix.go", "exec_unix.cs", "AAwg2II=")]
[assembly: GoPositionMap("os/exec/lookpath.go", "lookpath.cs", "AAc4ABUC")]
[assembly: GoPositionMap("os/exec/lp_unix.go", "lp_unix.cs", "ABUolIKCgpSCgpS4gpSClObKgIK2goKClJSCopSUgoKCgpSUpqqi")]
// </GoSourcePositionMaps>

namespace go.os;

using static go.os.exec_package;
using io = go.io_package;
using os = go.os_package;
using time = go.time_package;

[GoPackage("exec")]
public static partial class exec_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("goroutineStatus")] internal partial struct Start_goroutineStatus {}
    internal partial struct ctxResult {}
    internal partial struct prefixSuffixSaver {}
    internal partial struct wrappedError {}
    public partial struct Cmd {}
    public partial struct Cmd_cachedLookExtensions {}
    public partial struct Error {}
    public partial struct ExitError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸexecenv() => builtin.initPackage(typeof(@internal.syscall.execenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸunix() => builtin.initPackage(typeof(@internal.syscall.unix_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(sync.atomic_package));
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
[GoMethodHost(typeof(exec_package.Cmd)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class execᴛCmd;
[GoMethodHost(typeof(exec_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class execᴛError;
[GoMethodHost(typeof(exec_package.ExitError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class execᴛExitError;
[GoMethodHost(typeof(exec_package.prefixSuffixSaver)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class execᴛprefixSuffixSaver;
[GoMethodHost(typeof(exec_package.wrappedError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class execᴛwrappedError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class execᴛCmd {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Cmd c) => exec_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> argv(this ref Cmd c) => exec_package.argv⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) childStdin(this ж<Cmd> Ꮡc) => exec_package.childStdin⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) childStdout(this ref Cmd c) => exec_package.childStdout⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) childStderr(this ref Cmd c, ж<os.File> ᏑchildStdout) => exec_package.childStderr⁔(ref c, ᏑchildStdout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<os.File>, error) writerDescriptor(this ref Cmd c, io.Writer w) => exec_package.writerDescriptor⁔(ref c, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Run(this ж<Cmd> Ꮡc) => exec_package.Run⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Start(this ж<Cmd> Ꮡc) => exec_package.Start⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void watchCtx(this ref Cmd c, channel/*<-*/<ctxResult> resultc) => exec_package.watchCtx⁔(ref c, resultc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Wait(this ж<Cmd> Ꮡc) => exec_package.Wait⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error awaitGoroutines(this ж<Cmd> Ꮡc, ж<time.Timer> Ꮡtimer) => exec_package.awaitGoroutines⁔(Ꮡc, Ꮡtimer);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Output(this ж<Cmd> Ꮡc) => exec_package.Output⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) CombinedOutput(this ж<Cmd> Ꮡc) => exec_package.CombinedOutput⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.WriteCloser, error) StdinPipe(this ref Cmd c) => exec_package.StdinPipe⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.ReadCloser, error) StdoutPipe(this ref Cmd c) => exec_package.StdoutPipe⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.ReadCloser, error) StderrPipe(this ref Cmd c) => exec_package.StderrPipe⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<@string>, error) environ(this ref Cmd c) => exec_package.environ⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Environ(this ref Cmd c) => exec_package.Environ⁔(ref c);
}
partial class execᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref Error e) => exec_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref Error e) => exec_package.Unwrap⁔(ref e);
}
partial class execᴛExitError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ExitError e) => exec_package.Error⁔(ref e);
}
partial class execᴛprefixSuffixSaver {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<prefixSuffixSaver> Ꮡw, slice<byte> p) => exec_package.Write⁔(Ꮡw, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> /*pRemain*/ fill(this ref prefixSuffixSaver w, ж<slice<byte>> Ꮡdst, slice<byte> p) => exec_package.fill⁔(ref w, Ꮡdst, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref prefixSuffixSaver w) => exec_package.Bytes⁔(ref w);
}
partial class execᴛwrappedError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this wrappedError w) => exec_package.Error⁔(w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this wrappedError w) => exec_package.Unwrap⁔(w);
}
// </MethodForwarders>
