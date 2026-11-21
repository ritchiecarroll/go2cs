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
using static go.sync_package;
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
[assembly: GoImplement<Mutex, Locker>(Pointer = true)]
[assembly: GoImplement<RWMutex, Locker>(Pointer = true)]
[assembly: GoImplement<rlocker, Locker>(Pointer = true)]
// </InterfaceImplementations>

// An exported pointer-receiver method that carries a `ref`-receiver primary beside its
// pointer-box twin is recorded here as a `GoRefPrimary` attribute, so a package in another
// assembly can bind the primary at a ref-addressable call site instead of allocating a box.
// Both names are the Go spellings. The section exists only while there is a record to hold.
// <RefVerdicts>
[assembly: GoRefPrimary("Mutex", "Lock")]
[assembly: GoRefPrimary("Mutex", "TryLock")]
[assembly: GoRefPrimary("Mutex", "Unlock")]
// </RefVerdicts>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Cond))]
[assembly: GoFieldAccess(typeof(Map))]
[assembly: GoFieldAccess(typeof(RWMutex))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("sync/cond.go", "cond.cs", "ACxgkgACJAARAoKCgoIAAhTygq7CggAROJI=")]
[assembly: GoPositionMap("sync/map.go", "map.cs", "ACtesqiSqJKssqqiqqKqoqyyAAIQ0gACHAALAg==")]
[assembly: GoPositionMap("sync/runtime.go", "runtime.cs", "AAkc5gACFAAIAgABEgAIBqampqaSgoKmgg==")]
[assembly: GoPositionMap("sync/rwmutex.go", "rwmutex.cs", "ADGGAQASAoKClJSUgoIAAxLSgoKUgoKCgpSUgoKClAAEEsKCgoKUlJSC+IKCgqaUvtKCgqaUlIKUgoKCAAMS8oKClIKClJSCgoKUlIKCgpQABhIACAKCgoK4goKCpoKmgoIAAxgACAKCqqLKgg==")]
// </GoSourcePositionMaps>

namespace go;

using static go.sync_package;

[GoPackage("sync")]
public static partial class sync_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct copyChecker {}
    internal partial struct noCopy {}
    internal partial struct notifyList {}
    internal partial struct rlocker {}
    public partial struct Cond {}
    public partial struct Map {}
    public partial struct RWMutex {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsync() => builtin.initPackage(typeof(@internal.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsynctest() => builtin.initPackage(typeof(@internal.synctest_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(sync_package.Cond)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛCond;
[GoMethodHost(typeof(sync_package.Map)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛMap;
[GoMethodHost(typeof(sync_package.Mutex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛMutex;
[GoMethodHost(typeof(sync_package.Once)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛOnce;
[GoMethodHost(typeof(sync_package.Pool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛPool;
[GoMethodHost(typeof(sync_package.RWMutex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛRWMutex;
[GoMethodHost(typeof(sync_package.WaitGroup)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛWaitGroup;
[GoMethodHost(typeof(sync_package.copyChecker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛcopyChecker;
[GoMethodHost(typeof(sync_package.noCopy)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛnoCopy;
[GoMethodHost(typeof(sync_package.poolChain)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛpoolChain;
[GoMethodHost(typeof(sync_package.poolDequeue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛpoolDequeue;
[GoMethodHost(typeof(sync_package.rlocker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syncᴛrlocker;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class syncᴛCond {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Wait(this ж<Cond> Ꮡc) => sync_package.Wait⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Signal(this ж<Cond> Ꮡc) => sync_package.Signal⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Broadcast(this ж<Cond> Ꮡc) => sync_package.Broadcast⁔(Ꮡc);
}
partial class syncᴛMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (any value, bool ok) Load(this ж<Map> Ꮡm, any key) => sync_package.Load⁔(Ꮡm, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Store(this ж<Map> Ꮡm, any key, any value) => sync_package.Store⁔(Ꮡm, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Clear(this ж<Map> Ꮡm) => sync_package.Clear⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (any actual, bool loaded) LoadOrStore(this ж<Map> Ꮡm, any key, any value) => sync_package.LoadOrStore⁔(Ꮡm, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (any value, bool loaded) LoadAndDelete(this ж<Map> Ꮡm, any key) => sync_package.LoadAndDelete⁔(Ꮡm, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Delete(this ж<Map> Ꮡm, any key) => sync_package.Delete⁔(Ꮡm, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (any previous, bool loaded) Swap(this ж<Map> Ꮡm, any key, any value) => sync_package.Swap⁔(Ꮡm, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool /*swapped*/ CompareAndSwap(this ж<Map> Ꮡm, any key, any old, any @new) => sync_package.CompareAndSwap⁔(Ꮡm, key, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool /*deleted*/ CompareAndDelete(this ж<Map> Ꮡm, any key, any old) => sync_package.CompareAndDelete⁔(Ꮡm, key, old);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Range(this ж<Map> Ꮡm, Func<any, any, bool> f) => sync_package.Range⁔(Ꮡm, f);
}
partial class syncᴛRWMutex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RLock(this ж<RWMutex> Ꮡrw) => sync_package.RLock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool TryRLock(this ж<RWMutex> Ꮡrw) => sync_package.TryRLock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RUnlock(this ж<RWMutex> Ꮡrw) => sync_package.RUnlock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rUnlockSlow(this ж<RWMutex> Ꮡrw, int32 r) => sync_package.rUnlockSlow⁔(Ꮡrw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Lock(this ж<RWMutex> Ꮡrw) => sync_package.Lock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool TryLock(this ж<RWMutex> Ꮡrw) => sync_package.TryLock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Unlock(this ж<RWMutex> Ꮡrw) => sync_package.Unlock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Locker RLocker(this ж<RWMutex> Ꮡrw) => sync_package.RLocker⁔(Ꮡrw);
}
partial class syncᴛnoCopy {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Lock(this ref noCopy _) => sync_package.Lock⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Unlock(this ref noCopy _) => sync_package.Unlock⁔(ref _);
}
partial class syncᴛrlocker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Lock(this ж<rlocker> Ꮡr) => sync_package.Lock⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Unlock(this ж<rlocker> Ꮡr) => sync_package.Unlock⁔(Ꮡr);
}
// </MethodForwarders>
