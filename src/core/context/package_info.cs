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
using static go.context_package;

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
[assembly: GoPositionMap("context/context.go", "context.cs", "AKgBzgK4AAoIgoIACgyypoKmgqaCAA4KggAOCoLOwq7CAAgg8oKQAA80AAsCgpC2goKUgoIAAhLigoKUgoKCgoLuAAIoABECpoKSgoKUgpQADxyigoKUggAVMOKCgpSCgpSCgpSokoKClIKClIKClAAKFpSCABMeooKUptKCgpSCgoKCgpTWlJSClKrClIKCpraC2ICUgpSUgpSUgraAlIKClLiCtoKCtAAOFIKClMaCrNKClIKUgoKClIKCgoKUlJSUgpSCAAUQwoKUAAgOsqaCpoKmgqaCAAQYAAkCrNKClJSUpoKCgoKQpIKSgpKmkAARGoKmgqqigpSUgoKClAAEGgAKAqyyAAIgAA0CgpSClIKUAB0aspSkpJTGgqqCgpTGgoKUgpS0gpS0ppS0gpTUtA==", "242-242:1;270-270:1;330-339:1;332-334:1.1;357-359:1;510-512:1;522-528:2;647-647:1;652-654:2;656-656:3", "", "786=String/1/2/1,Until/1/1/2,String/2/2/2;901=stringify/1/2/1,stringify/2/2/2")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using errors = errors_package;
using reflectlite = @internal.reflectlite_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using static context_package;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(context_package.afterFuncCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛafterFuncCtx;
[GoMethodHost(typeof(context_package.backgroundCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛbackgroundCtx;
[GoMethodHost(typeof(context_package.cancelCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛcancelCtx;
[GoMethodHost(typeof(context_package.deadlineExceededError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛdeadlineExceededError;
[GoMethodHost(typeof(context_package.emptyCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛemptyCtx;
[GoMethodHost(typeof(context_package.timerCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛtimerCtx;
[GoMethodHost(typeof(context_package.todoCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛtodoCtx;
[GoMethodHost(typeof(context_package.valueCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛvalueCtx;
[GoMethodHost(typeof(context_package.withoutCancelCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class contextᴛwithoutCancelCtx;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class contextᴛdeadlineExceededError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this deadlineExceededError _) => context_package.Errorᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Timeout(this deadlineExceededError _) => context_package.Timeoutᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Temporary(this deadlineExceededError _) => context_package.Temporaryᴍ(_);
}
partial class contextᴛemptyCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this emptyCtx _) => context_package.Deadlineᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this emptyCtx _) => context_package.Doneᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this emptyCtx _) => context_package.Errᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Value(this emptyCtx _, any key) => context_package.Valueᴍ(_, key);
}
partial class contextᴛbackgroundCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this backgroundCtx _) => context_package.Stringᴍ(_);
}
partial class contextᴛtodoCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this todoCtx _) => context_package.Stringᴍ(_);
}
partial class contextᴛafterFuncCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cancel(this ж<afterFuncCtx> Ꮡa, bool removeFromParent, error err, error cause) => context_package.cancelᴍ(Ꮡa, removeFromParent, err, cause);
}
partial class contextᴛcancelCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time, bool) Deadline(this cancelCtx recvᴛ) => context_package.Deadlineᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Value(this ж<cancelCtx> Ꮡc, any key) => context_package.Valueᴍ(Ꮡc, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this ж<cancelCtx> Ꮡc) => context_package.Doneᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this ж<cancelCtx> Ꮡc) => context_package.Errᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void propagateCancel(this ж<cancelCtx> Ꮡc, Context parent, canceler child) => context_package.propagateCancelᴍ(Ꮡc, parent, child);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref cancelCtx c) => context_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cancel(this ж<cancelCtx> Ꮡc, bool removeFromParent, error err, error cause) => context_package.cancelᴍ(Ꮡc, removeFromParent, err, cause);
}
partial class contextᴛwithoutCancelCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this withoutCancelCtx _) => context_package.Deadlineᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this withoutCancelCtx _) => context_package.Doneᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this withoutCancelCtx _) => context_package.Errᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Value(this withoutCancelCtx c, any key) => context_package.Valueᴍ(c, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this withoutCancelCtx c) => context_package.Stringᴍ(c);
}
partial class contextᴛtimerCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time deadline, bool ok) Deadline(this ref timerCtx c) => context_package.Deadlineᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref timerCtx c) => context_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cancel(this ж<timerCtx> Ꮡc, bool removeFromParent, error err, error cause) => context_package.cancelᴍ(Ꮡc, removeFromParent, err, cause);
}
partial class contextᴛvalueCtx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Time, bool) Deadline(this valueCtx recvᴛ) => context_package.Deadlineᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this valueCtx recvᴛ) => context_package.Doneᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this valueCtx recvᴛ) => context_package.Errᴍ(recvᴛ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref valueCtx c) => context_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Value(this ref valueCtx c, any key) => context_package.Valueᴍ(ref c, key);
}
// </GlyphForwarders>
