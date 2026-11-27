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
using static go.os.signal_package;
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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b206d206d61705b6368616e3c2d206f732e5369676e616c5d2a6f732f7369676e616c2e68616e646c65723b20726566205b36355d696e7436343b2073746f7070696e67205b5d6f732f7369676e616c2e73746f7070696e677d", "handlersᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<signalCtx, context_package.Context>(Pointer = true)]
[assembly: GoImplement<signalCtx, context_package.Context>(Promoted = true)]
[assembly: GoImplement<signalError, error>]
[assembly: GoImplement<syscall_package.Signal, os_package.Signal>]
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
[assembly: GoFieldAccess(typeof(handler))]
[assembly: GoFieldAccess(typeof(signalCtx))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("os/signal/signal.go", "signal.cs", "ACdKgqaCpoKs0oKUgpSCgoKCgsqmgoKmgsas4qiSggAKOAAUAoKmgpSCgoKUgqaSgpSCgoK4goK4uIKCpoLGqtKsspSCgoKUlIKCgoIADyCUlJSUgoKCuKrUooKCpoKUgpQACxCCggALDAACKAAVAoLKgoKCkrQACAoACRaCgu6ipoKCgoKCgoKCppSCyoKmgoKU", "56-70:1;139-158:1;150-154:1.1;291-297:1")]
[assembly: GoPositionMap("os/signal/signal_unix.go", "signal_unix.cs", "AAsesqKiorSCgriCzoKUgoKUtLiCpoKmgqaC")]
// </GoSourcePositionMaps>

namespace go.os;

using static go.os.signal_package;

[GoPackage("signal")]
public static partial class signal_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface stringer {}
    [GoValueClone("mask")] internal partial struct handler {}
    [GoValueClone("@ref")] internal partial struct handlersᴛ1 {}
    internal partial struct signalCtx {}
    internal partial struct signalError {}
    internal partial struct stopping {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
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
[GoMethodHost(typeof(signal_package.handler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class signalᴛhandler;
[GoMethodHost(typeof(signal_package.signalCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class signalᴛsignalCtx;
[GoMethodHost(typeof(signal_package.signalError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class signalᴛsignalError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class signalᴛhandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool want(this ref handler h, nint sig) => signal_package.want⁔(ref h, sig);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref handler h, nint sig) => signal_package.set⁔(ref h, sig);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref handler h, nint sig) => signal_package.clear⁔(ref h, sig);
}
partial class signalᴛsignalCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stop(this ref signalCtx c) => signal_package.stop⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref signalCtx c) => signal_package.String⁔(ref c);
}
partial class signalᴛsignalError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this signalError s) => signal_package.Error⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Is(this signalError s, error target) => signal_package.Is⁔(s, target);
}
// </MethodForwarders>
