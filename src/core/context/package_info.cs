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
// </ImportedTypeAliases>

using go;
using static go.context_package;
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
[assembly: GoImplement<afterFuncCtx, canceler>(Pointer = true)]
[assembly: GoImplement<backgroundCtx, Context>]
[assembly: GoImplement<cancelCtx, Context>(Pointer = true)]
[assembly: GoImplement<cancelCtx, canceler>(Pointer = true)]
[assembly: GoImplement<deadlineExceededError, error>]
[assembly: GoImplement<emptyCtx, Context>]
[assembly: GoImplement<stopCtx, Context>(Promoted = true)]
[assembly: GoImplement<stopCtx, Context>]
[assembly: GoImplement<timerCtx, Context>(Pointer = true)]
[assembly: GoImplement<timerCtx, canceler>(Pointer = true)]
[assembly: GoImplement<todoCtx, Context>]
[assembly: GoImplement<valueCtx, Context>(Pointer = true)]
[assembly: GoImplement<withoutCancelCtx, Context>]
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
[assembly: GoFieldAccess(typeof(afterFuncCtx))]
[assembly: GoFieldAccess(typeof(cancelCtx))]
[assembly: GoFieldAccess(typeof(timerCtx))]
[assembly: GoFieldAccess(typeof(valueCtx))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("context/context.go", "context.cs", "AKYBzgK4AAgIgoLssqaCpoKmggAKCoIACgqCrsKuwgAIIPKCkAAPNAALAoKQtoKClIKCAAIS4oKClIKCgoKC7gACKAARAqaCkoKClIKUAA0cooKClIIAEzDigoKUgoKUgoKUqJKCgpSCgpSCgpQAChaUggARHqKClKbSgoKUgoKCgoKU1pSUgpSqwpSCgqa2gtiAlIKUlIKUlIK2gJSCgpS4graCgrQADBSCgpSmgqzSgpSClIKCgpSCgoKClJSUlIKUggADEMKClO6ypoKmgqaCpoIAAhgACQKs0oKUlJSmgoKCgpCkgpKCkqaQAA8agqaCqqKClJSCgoKUAAIaAAoCrLIAAiAADQKClIKUgpQAGRqylKSklKaCqoKClKaCgpSClLSClLSmlLSClNS0", "242-242:1;270-270:1;330-339:1;332-334:1.1;357-359:1;510-512:1;522-528:2;647-647:1;652-654:2;656-656:3", "", "750=String/1/2/1,Until/1/1/2,String/2/2/2;857=stringify/1/2/1,stringify/2/2/2")]
// </GoSourcePositionMaps>

namespace go;

using static go.context_package;
using time = go.time_package;

[GoPackage("context")]
public static partial class context_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface afterFuncer {}
    internal partial interface canceler {}
    internal partial interface stringer {}
    internal partial struct afterFuncCtx {}
    internal partial struct backgroundCtx {}
    internal partial struct cancelCtx {}
    internal partial struct deadlineExceededError {}
    internal partial struct emptyCtx {}
    internal partial struct stopCtx {}
    internal partial struct timerCtx {}
    internal partial struct todoCtx {}
    internal partial struct valueCtx {}
    internal partial struct withoutCancelCtx {}
    public partial interface Context {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() => builtin.initPackage(typeof(@internal.reflectlite_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(context_package.afterFuncCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛafterFuncCtx;
[GoMethodHost(typeof(context_package.backgroundCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛbackgroundCtx;
[GoMethodHost(typeof(context_package.cancelCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛcancelCtx;
[GoMethodHost(typeof(context_package.deadlineExceededError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛdeadlineExceededError;
[GoMethodHost(typeof(context_package.emptyCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛemptyCtx;
[GoMethodHost(typeof(context_package.timerCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛtimerCtx;
[GoMethodHost(typeof(context_package.todoCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛtodoCtx;
[GoMethodHost(typeof(context_package.valueCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛvalueCtx;
[GoMethodHost(typeof(context_package.withoutCancelCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class contextᴛwithoutCancelCtx;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class contextᴛafterFuncCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cancel(this ж<afterFuncCtx> Ꮡa, bool removeFromParent, error err, error cause) => context_package.cancel⁔(Ꮡa, removeFromParent, err, cause);
}
partial class contextᴛbackgroundCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this backgroundCtx _) => context_package.String⁔(_);
}
partial class contextᴛcancelCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time, bool) Deadline(this cancelCtx recvᴛ) => context_package.Deadline⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Value(this ж<cancelCtx> Ꮡc, any key) => context_package.Value⁔(Ꮡc, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this ж<cancelCtx> Ꮡc) => context_package.Done⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this ж<cancelCtx> Ꮡc) => context_package.Err⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void propagateCancel(this ж<cancelCtx> Ꮡc, Context parent, canceler child) => context_package.propagateCancel⁔(Ꮡc, parent, child);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref cancelCtx c) => context_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cancel(this ж<cancelCtx> Ꮡc, bool removeFromParent, error err, error cause) => context_package.cancel⁔(Ꮡc, removeFromParent, err, cause);
}
partial class contextᴛdeadlineExceededError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this deadlineExceededError _) => context_package.Error⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this deadlineExceededError _) => context_package.Timeout⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this deadlineExceededError _) => context_package.Temporary⁔(_);
}
partial class contextᴛemptyCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this emptyCtx _) => context_package.Deadline⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this emptyCtx _) => context_package.Done⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this emptyCtx _) => context_package.Err⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Value(this emptyCtx _, any key) => context_package.Value⁔(_, key);
}
partial class contextᴛtimerCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this ref timerCtx c) => context_package.Deadline⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref timerCtx c) => context_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cancel(this ж<timerCtx> Ꮡc, bool removeFromParent, error err, error cause) => context_package.cancel⁔(Ꮡc, removeFromParent, err, cause);
}
partial class contextᴛtodoCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this todoCtx _) => context_package.String⁔(_);
}
partial class contextᴛvalueCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time, bool) Deadline(this valueCtx recvᴛ) => context_package.Deadline⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this valueCtx recvᴛ) => context_package.Done⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this valueCtx recvᴛ) => context_package.Err⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref valueCtx c) => context_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Value(this ref valueCtx c, any key) => context_package.Value⁔(ref c, key);
}
partial class contextᴛwithoutCancelCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this withoutCancelCtx _) => context_package.Deadline⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this withoutCancelCtx _) => context_package.Done⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this withoutCancelCtx _) => context_package.Err⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Value(this withoutCancelCtx c, any key) => context_package.Value⁔(c, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this withoutCancelCtx c) => context_package.String⁔(c);
}
// </MethodForwarders>
