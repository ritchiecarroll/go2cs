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
using syscall = go.syscall_package;
// </ImportedTypeAliases>

using go;
using static go.net.@internal.socktest_package;

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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Stat))]
[assembly: GoFieldAccess(typeof(Status))]
[assembly: GoFieldAccess(typeof(Switch))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/internal/socktest/switch.go", "switch.cs", "ABc0goKCqLKCgoKClIKosoKCgpSCAAYQgJaAloCUggAJFIIAEyyCyoKCgoKUABc0goKUAAsYgoKUqLKCgoI=")]
[assembly: GoPositionMap("net/internal/socktest/switch_posix.go", "switch_posix.cs", "ABMcgpSkpKQADQiCgpS0tLS0pIKUAAoGgpSkpKQ=")]
[assembly: GoPositionMap("net/internal/socktest/switch_unix.go", "switch_unix.cs", "AAsY0oKCkoKU2sKCgoI=")]
[assembly: GoPositionMap("net/internal/socktest/sys_unix.go", "sys_unix.cs", "AAoY8pSCgoKUgoKUgoKClKaCgoKClIKC6OKCgpSCgpSCgpSCgqaCgoKClIKC6OKCgpSCgpSCgpSCgqaCgoKClILo4oKClIKClIKClIKCpoKCgoKUgugACAKCgpSCgpSCgpSCgoKUpoKCgoKUgoLo0oKClIKClIKClIKCgqaClIKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.net.@internal;

using static go.net.@internal.socktest_package;
using syscall = go.syscall_package;

[GoPackage("socktest")]
public static partial class socktest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct stats {}
    public partial struct Cookie {}
    public partial struct FilterType {}
    public partial struct Sockets {}
    public partial struct Stat {}
    public partial struct Status {}
    public partial struct Switch {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(socktest_package.AfterFilter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛAfterFilter;
[GoMethodHost(typeof(socktest_package.Cookie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛCookie;
[GoMethodHost(typeof(socktest_package.Filter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛFilter;
[GoMethodHost(typeof(socktest_package.Stat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛStat;
[GoMethodHost(typeof(socktest_package.Status)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛStatus;
[GoMethodHost(typeof(socktest_package.Switch)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛSwitch;
[GoMethodHost(typeof(socktest_package.stats)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class socktestᴛstats;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class socktestᴛAfterFilter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error apply(this AfterFilter f, ж<Status> Ꮡst) => socktest_package.apply⁔(f, Ꮡst);
}
partial class socktestᴛCookie {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Family(this Cookie c) => socktest_package.Family⁔(c);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Type(this Cookie c) => socktest_package.Type⁔(c);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Protocol(this Cookie c) => socktest_package.Protocol⁔(c);
}
partial class socktestᴛFilter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (AfterFilter, error) apply(this Filter f, ж<Status> Ꮡst) => socktest_package.apply⁔(f, Ꮡst);
}
partial class socktestᴛStat {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Stat st) => socktest_package.String⁔(st);
}
partial class socktestᴛStatus {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Status so) => socktest_package.String⁔(so);
}
partial class socktestᴛSwitch {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref Switch sw) => socktest_package.init⁔(ref sw);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<Stat> Stats(this ж<Switch> Ꮡsw) => socktest_package.Stats⁔(Ꮡsw);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Sockets Sockets(this ж<Switch> Ꮡsw) => socktest_package.Sockets⁔(Ꮡsw);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ж<Switch> Ꮡsw, FilterType t, Filter f) => socktest_package.Set⁔(Ꮡsw, t, f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Status> sockso(this ж<Switch> Ꮡsw, nint s) => socktest_package.sockso⁔(Ꮡsw, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Status> addLocked(this ж<Switch> Ꮡsw, nint s, nint family, nint sotype, nint proto) => socktest_package.addLocked⁔(Ꮡsw, s, family, sotype, proto);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint s, error err) Socket(this ж<Switch> Ꮡsw, nint family, nint sotype, nint proto) => socktest_package.Socket⁔(Ꮡsw, family, sotype, proto);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ Close(this ж<Switch> Ꮡsw, nint s) => socktest_package.Close⁔(Ꮡsw, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ Connect(this ж<Switch> Ꮡsw, nint s, syscall.Sockaddr sa) => socktest_package.Connect⁔(Ꮡsw, s, sa);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ Listen(this ж<Switch> Ꮡsw, nint s, nint backlog) => socktest_package.Listen⁔(Ꮡsw, s, backlog);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint ns, syscall.Sockaddr sa, error err) Accept(this ж<Switch> Ꮡsw, nint s) => socktest_package.Accept⁔(Ꮡsw, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint soerr, error err) GetsockoptInt(this ж<Switch> Ꮡsw, nint s, nint level, nint opt) => socktest_package.GetsockoptInt⁔(Ꮡsw, s, level, opt);
}
partial class socktestᴛstats {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Stat> getLocked(this stats st, Cookie c) => socktest_package.getLocked⁔(st, c);
}
// </MethodForwarders>
