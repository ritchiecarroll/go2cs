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
using static go.@internal.runtime.atomic_package;
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
[assembly: GoFieldAccess(typeof(Bool))]
[assembly: GoFieldAccess(typeof(Float64))]
[assembly: GoFieldAccess(typeof(Pointer<>))]
[assembly: GoFieldAccess(typeof(Uint32))]
[assembly: GoFieldAccess(typeof(Uint64))]
[assembly: GoFieldAccess(typeof(Uint8))]
[assembly: GoFieldAccess(typeof(Uintptr))]
[assembly: GoFieldAccess(typeof(UnsafePointer))]
[assembly: GoFieldAccess(typeof(go.@internal.runtime.atomic_package.Int32))]
[assembly: GoFieldAccess(typeof(go.@internal.runtime.atomic_package.Int64))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/runtime/atomic/atomic_amd64.go", "atomic_amd64.cs", "ABpopqampqamAAQSpqampqampqamqsampqampqas")]
[assembly: GoPositionMap("internal/runtime/atomic/stubs.go", "stubs.cs", "AAoYpqampqampqamqsampqam")]
[assembly: GoPositionMap("internal/runtime/atomic/types.go", "types.cs", "ABIosqyyAAIQ0q7CAAIU8gANIrKssgACENKuwgACFPIAChyyrLIAAhTyAAIU8gAKHLKssoKClAAKHLIAAhgACQKssgACGAAJAgACENIAAhoACgKuwgACFPIAAhTyAAIU8gANIrKssgACENKuwgACFPIAChyyAAIYAAkCrLIAAhgACQIAAhDSrsIAAhTyAAwgsoKssgAQKLIAAhgACQKokqwABBwADAKssqYABhSyAAIYAAkCrLIAAh4ADAKuwgAKFpI=")]
[assembly: GoPositionMap("internal/runtime/atomic/types_64bit.go", "types_64bit.cs", "AAgkAAkCAAIYAAkC")]
[assembly: GoPositionMap("internal/runtime/atomic/unaligned.go", "unaligned.cs", "/oI=")]
[assembly: GoPositionMap("internal/runtime/atomic/xchg8.go", "xchg8.cs", "AAsapIKCgpSCgpSClJQ=")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

using static go.@internal.runtime.atomic_package;

[GoPackage("atomic")]
public static partial class atomic_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct align64 {}
    internal partial struct noCopy {}
    public partial struct Bool {}
    public partial struct Float64 {}
    public partial struct Int32 {}
    public partial struct Int64 {}
    public partial struct Pointer<T> {}
    public partial struct Uint32 {}
    public partial struct Uint64 {}
    public partial struct Uint8 {}
    public partial struct Uintptr {}
    public partial struct UnsafePointer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(atomic_package.Bool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛBool;
[GoMethodHost(typeof(atomic_package.Float64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛFloat64;
[GoMethodHost(typeof(atomic_package.Int32)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛInt32;
[GoMethodHost(typeof(atomic_package.Int64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛInt64;
[GoMethodHost(typeof(atomic_package.Pointer<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛPointer;
[GoMethodHost(typeof(atomic_package.Uint32)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛUint32;
[GoMethodHost(typeof(atomic_package.Uint64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛUint64;
[GoMethodHost(typeof(atomic_package.Uint8)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛUint8;
[GoMethodHost(typeof(atomic_package.Uintptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛUintptr;
[GoMethodHost(typeof(atomic_package.UnsafePointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛUnsafePointer;
[GoMethodHost(typeof(atomic_package.noCopy)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class atomicᴛnoCopy;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class atomicᴛBool {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Load(this ж<Bool> Ꮡb) => atomic_package.Load⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Bool> Ꮡb, bool value) => atomic_package.Store⁔(Ꮡb, value);
}
partial class atomicᴛFloat64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Load(this ж<Float64> Ꮡf) => atomic_package.Load⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Float64> Ꮡf, float64 value) => atomic_package.Store⁔(Ꮡf, value);
}
partial class atomicᴛInt32 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Load(this ж<Int32> Ꮡi) => atomic_package.Load⁔(Ꮡi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Int32> Ꮡi, int32 value) => atomic_package.Store⁔(Ꮡi, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<Int32> Ꮡi, int32 old, int32 @new) => atomic_package.CompareAndSwap⁔(Ꮡi, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Swap(this ж<Int32> Ꮡi, int32 @new) => atomic_package.Swap⁔(Ꮡi, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int32 Add(this ж<Int32> Ꮡi, int32 delta) => atomic_package.Add⁔(Ꮡi, delta);
}
partial class atomicᴛInt64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Load(this ж<Int64> Ꮡi) => atomic_package.Load⁔(Ꮡi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Int64> Ꮡi, int64 value) => atomic_package.Store⁔(Ꮡi, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<Int64> Ꮡi, int64 old, int64 @new) => atomic_package.CompareAndSwap⁔(Ꮡi, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Swap(this ж<Int64> Ꮡi, int64 @new) => atomic_package.Swap⁔(Ꮡi, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Add(this ж<Int64> Ꮡi, int64 delta) => atomic_package.Add⁔(Ꮡi, delta);
}
partial class atomicᴛPointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<T> Load<T>(this ж<Pointer<T>> Ꮡp) => atomic_package.Load⁔<T>(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreNoWB<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡvalue) => atomic_package.StoreNoWB⁔<T>(Ꮡp, Ꮡvalue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡvalue) => atomic_package.Store⁔<T>(Ꮡp, Ꮡvalue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwapNoWB<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡold, ж<T> Ꮡnew) => atomic_package.CompareAndSwapNoWB⁔<T>(Ꮡp, Ꮡold, Ꮡnew);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡold, ж<T> Ꮡnew) => atomic_package.CompareAndSwap⁔<T>(Ꮡp, Ꮡold, Ꮡnew);
}
partial class atomicᴛUint32 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Load(this ж<Uint32> Ꮡu) => atomic_package.Load⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 LoadAcquire(this ж<Uint32> Ꮡu) => atomic_package.LoadAcquire⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Uint32> Ꮡu, uint32 value) => atomic_package.Store⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreRelease(this ж<Uint32> Ꮡu, uint32 value) => atomic_package.StoreRelease⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<Uint32> Ꮡu, uint32 old, uint32 @new) => atomic_package.CompareAndSwap⁔(Ꮡu, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwapRelease(this ж<Uint32> Ꮡu, uint32 old, uint32 @new) => atomic_package.CompareAndSwapRelease⁔(Ꮡu, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Swap(this ж<Uint32> Ꮡu, uint32 value) => atomic_package.Swap⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void And(this ж<Uint32> Ꮡu, uint32 value) => atomic_package.And⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Or(this ж<Uint32> Ꮡu, uint32 value) => atomic_package.Or⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Add(this ж<Uint32> Ꮡu, int32 delta) => atomic_package.Add⁔(Ꮡu, delta);
}
partial class atomicᴛUint64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Load(this ж<Uint64> Ꮡu) => atomic_package.Load⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Uint64> Ꮡu, uint64 value) => atomic_package.Store⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<Uint64> Ꮡu, uint64 old, uint64 @new) => atomic_package.CompareAndSwap⁔(Ꮡu, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Swap(this ж<Uint64> Ꮡu, uint64 value) => atomic_package.Swap⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Add(this ж<Uint64> Ꮡu, int64 delta) => atomic_package.Add⁔(Ꮡu, delta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 LoadAcquire(this ж<Uint64> Ꮡu) => atomic_package.LoadAcquire⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreRelease(this ж<Uint64> Ꮡu, uint64 value) => atomic_package.StoreRelease⁔(Ꮡu, value);
}
partial class atomicᴛUint8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint8 Load(this ж<Uint8> Ꮡu) => atomic_package.Load⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Uint8> Ꮡu, uint8 value) => atomic_package.Store⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void And(this ж<Uint8> Ꮡu, uint8 value) => atomic_package.And⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Or(this ж<Uint8> Ꮡu, uint8 value) => atomic_package.Or⁔(Ꮡu, value);
}
partial class atomicᴛUintptr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uintptr Load(this ж<Uintptr> Ꮡu) => atomic_package.Load⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uintptr LoadAcquire(this ж<Uintptr> Ꮡu) => atomic_package.LoadAcquire⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Uintptr> Ꮡu, uintptr value) => atomic_package.Store⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreRelease(this ж<Uintptr> Ꮡu, uintptr value) => atomic_package.StoreRelease⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<Uintptr> Ꮡu, uintptr old, uintptr @new) => atomic_package.CompareAndSwap⁔(Ꮡu, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uintptr Swap(this ж<Uintptr> Ꮡu, uintptr value) => atomic_package.Swap⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uintptr Add(this ж<Uintptr> Ꮡu, uintptr delta) => atomic_package.Add⁔(Ꮡu, delta);
}
partial class atomicᴛUnsafePointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static unsafe_package.Pointer Load(this ж<UnsafePointer> Ꮡu) => atomic_package.Load⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreNoWB(this ж<UnsafePointer> Ꮡu, unsafe_package.Pointer value) => atomic_package.StoreNoWB⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<UnsafePointer> Ꮡu, unsafe_package.Pointer value) => atomic_package.Store⁔(Ꮡu, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwapNoWB(this ж<UnsafePointer> Ꮡu, unsafe_package.Pointer old, unsafe_package.Pointer @new) => atomic_package.CompareAndSwapNoWB⁔(Ꮡu, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CompareAndSwap(this ж<UnsafePointer> Ꮡu, unsafe_package.Pointer old, unsafe_package.Pointer @new) => atomic_package.CompareAndSwap⁔(Ꮡu, old, @new);
}
partial class atomicᴛnoCopy {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Lock(this ref noCopy _) => atomic_package.Lock⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Unlock(this ref noCopy _) => atomic_package.Unlock⁔(ref _);
}
// </MethodForwarders>
