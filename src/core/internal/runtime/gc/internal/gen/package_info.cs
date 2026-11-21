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
// </ImportedTypeAliases>

using go;
using static go.@internal.runtime.gc.@internal.gen_package;
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
[assembly: GoImplement<@void, Value>]
[assembly: GoImplement<Mask64, Value>(Pointer = true)]
[assembly: GoImplement<Mask64, Value>]
[assembly: GoImplement<Mask8, Value>(Pointer = true)]
[assembly: GoImplement<Mask8, Value>]
[assembly: GoImplement<Ptr<@void>, Word>]
[assembly: GoImplement<Uint64x8, Value>(Pointer = true)]
[assembly: GoImplement<Uint64x8, Value>]
[assembly: GoImplement<Uint8x64, Value>(Pointer = true)]
[assembly: GoImplement<Uint8x64, Value>]
[assembly: GoImplement<locMem, loc>]
[assembly: GoImplement<locReg, loc>]
[assembly: GoImplement<opHeap, go.container.heap_package.Interface>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(go.@internal.runtime.gc.@internal.gen_package.Func))]
[assembly: GoFieldAccess(typeof(kind))]
[assembly: GoFieldAccess(typeof(op))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/runtime/gc/internal/gen/gen.go", "gen.cs", "ABgqogAIEoKmggAHELgACRSCgqrkgoKCgoK4goKCptKCgoKCgqaigoKUggARKIKmgqaigpSCgqamgoKClIKUgpSClKaCgoIADwjG6oKmgoKUgqaClIKCgpSCgpSUgqaClJS6pIKCgpSClIKClIKUpoKigoKCpoK4goKCgoKCuKiSgoKSgpSCgriCpMiClIKCgqaCygAJFvqCgoKC3JSUgoKCgpTugoKCgqYACAaigpS0tOzKgoKCgpSUlJSUkpSmgqaCgoKCggAICP6CgoKCgpSUgqaUgoKClIKCgoKCgriCgoKCgoKCpuaipoKCgrimgrakpLamlIK4lIKClJSClKa4lJSCpoKUgoKCgpSmgriCgoKCgoKClKY=", "173-179:1;230-239:1;259-267:1;366-376:1")]
[assembly: GoPositionMap("internal/runtime/gc/internal/gen/gp.go", "gp.cs", "AAsWlKKCpoKmgoKC")]
[assembly: GoPositionMap("internal/runtime/gc/internal/gen/regalloc.go", "regalloc.cs", "AB9EgpSkpKSUpoKmggAIEoKClIKUgpSmgqaCgpQACBIADCCC3KKClIKClIKUgpSmgoKUgoKUgpSClKa0poKCAAgSgoKUgpTKgoKUgoKU7oKCgsqCgriClKaUgtKCgpSUgqa4goKkgpS0gpSCgoKCgpS0gpSUkoKUtIK0gujagoKmlKaCgoLKgqaUgriCuIKCgriClLSCxoLGgraCpg==", "215-227:1")]
[assembly: GoPositionMap("internal/runtime/gc/internal/gen/simd.go", "simd.cs", "AAsWlLKCpoKmgoKCpqKCpqLclILKgoKmooKCpqKCgq7kgoKmtIKCprSCgu6UxIKmooKmooKmgqaCgoKmooKmtIKmooKmtIKmooKmtILulKKCgoKmgqaCgoKmooKmooKmooKUlO6UooKCgqaCpoKCgqaigqaigqaigpSUpqKClJQ=")]
[assembly: GoPositionMap("internal/runtime/gc/internal/gen/val.go", "val.cs", "ACpMlAAIDpSkooKCgoKClKaigoKmwoKCgpSCprKCgoKCpoIACA7mgqakku7cooKUgpSmgoKCyoI=")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime.gc.@internal;

[GoPackage("gen")]
public static partial class gen_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface loc {}
    internal partial struct @void {}
    internal partial struct fileConst {}
    internal partial struct locMem {}
    internal partial struct locReg {}
    internal partial struct opHeap {}
    internal partial struct regClass {}
    [GoValueClone("inUse")] internal partial struct regSet {}
    internal partial struct valAny {}
    internal partial struct valGP {}
    public partial interface Value {}
    public partial interface Word {}
    public partial interface wrap<T> {}
    public partial struct Array<T> {}
    public partial struct File {}
    public partial struct Func {}
    public partial struct Mask64 {}
    public partial struct Mask8 {}
    public partial struct Ptr<T> {}
    public partial struct Uint64 {}
    public partial struct Uint64x8 {}
    public partial struct Uint8x64 {}
    public partial struct kind {}
    public partial struct op {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontainerꓸheap() => builtin.initPackage(typeof(container.heap_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸmaphash() => builtin.initPackage(typeof(hash.maphash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gen_package.Array<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛArray;
[GoMethodHost(typeof(gen_package.File)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛFile;
[GoMethodHost(typeof(gen_package.Func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛFunc;
[GoMethodHost(typeof(gen_package.Mask64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛMask64;
[GoMethodHost(typeof(gen_package.Mask8)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛMask8;
[GoMethodHost(typeof(gen_package.Ptr<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛPtr;
[GoMethodHost(typeof(gen_package.Uint64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛUint64;
[GoMethodHost(typeof(gen_package.Uint64x8)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛUint64x8;
[GoMethodHost(typeof(gen_package.Uint8x64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛUint8x64;
[GoMethodHost(typeof(gen_package.locMem)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛlocMem;
[GoMethodHost(typeof(gen_package.locReg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛlocReg;
[GoMethodHost(typeof(gen_package.op)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛop;
[GoMethodHost(typeof(gen_package.opHeap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛopHeap;
[GoMethodHost(typeof(gen_package.regSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛregSet;
[GoMethodHost(typeof(gen_package.valAny)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛvalAny;
[GoMethodHost(typeof(gen_package.valGP)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛvalGP;
[GoMethodHost(typeof(gen_package.@void)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class genᴛvoid;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class genᴛArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind<T>(this gen_package.Array<T> _) where T : gen_package.Value => gen_package.kind⁔<T>(_);
}
partial class genᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.void AddFunc(this ref gen_package.File f, ж<gen_package.Func> Ꮡfn) => gen_package.AddFunc⁔(ref f, Ꮡfn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.void AddConst(this ref gen_package.File f, @string name, any data) => gen_package.AddConst⁔(ref f, name, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.void Compile(this ж<gen_package.File> Ꮡf) => gen_package.Compile⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void emitConst(this ref gen_package.File f, @string name, any data) => gen_package.emitConst⁔(ref f, name, data);
}
partial class genᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void attach(this ж<gen_package.Func> Ꮡfn, ж<gen_package.op> Ꮡx) => gen_package.attach⁔(Ꮡfn, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void write(this ref gen_package.Func fn, io.Writer w) => gen_package.write⁔(ref fn, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool cse(this ref gen_package.Func fn) => gen_package.cse⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool deadcode(this ref gen_package.Func fn) => gen_package.deadcode⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void addLoads(this ж<gen_package.Func> Ꮡfn) => gen_package.addLoads⁔(Ꮡfn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static map<ж<gen_package.op>, nint> opRefs(this ref gen_package.Func fn) => gen_package.opRefs⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void schedule(this ref gen_package.Func fn) => gen_package.schedule⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void emit(this ref gen_package.Func fn, ж<gen_package.File> Ꮡf, map<ж<gen_package.op>, gen_package.loc> locs) => gen_package.emit⁔(ref fn, Ꮡf, locs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static map<ж<gen_package.op>, gen_package.loc> assignLocs(this ж<gen_package.Func> Ꮡfn) => gen_package.assignLocs⁔(Ꮡfn);
}
partial class genᴛMask64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.Mask64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Mask64 wrap(this gen_package.Mask64 _, ж<gen_package.op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64 /*z*/ ToUint64(this gen_package.Mask64 x) => gen_package.ToUint64⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask64 /*z*/ Or(this gen_package.Mask64 x, gen_package.Mask64 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask64 /*z*/ ShiftLeft(this gen_package.Mask64 x, uint8 c) => gen_package.ShiftLeft⁔(x, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask64 /*z*/ ShiftRight(this gen_package.Mask64 x, uint8 c) => gen_package.ShiftRight⁔(x, c);
}
partial class genᴛMask8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.Mask8 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Mask8 wrap(this gen_package.Mask8 _, ж<gen_package.op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64 /*z*/ ToUint8(this gen_package.Mask8 x) => gen_package.ToUint8⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask8 /*z*/ Or(this gen_package.Mask8 x, gen_package.Mask8 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask8 /*z*/ ShiftLeft(this gen_package.Mask8 x, uint8 c) => gen_package.ShiftLeft⁔(x, c);
}
partial class genᴛPtr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind<T>(this gen_package.Ptr<T> _Δp0) where T : gen_package.Value => gen_package.kind⁔<T>(_Δp0);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Ptr<T> wrap<T>(this gen_package.Ptr<T> _, ж<gen_package.op> Ꮡx) where T : gen_package.Value => gen_package.wrap⁔<T>(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Ptr<T> /*y*/ AddConst<T>(this gen_package.Ptr<T> x, nint off) where T : gen_package.Value => gen_package.AddConst⁔<T>(x, off);
}
partial class genᴛUint64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.Uint64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Uint64 wrap(this gen_package.Uint64 _, ж<gen_package.op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
}
partial class genᴛUint64x8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64x8 /*z*/ BroadcastMasked(this gen_package.Uint64x8 x, gen_package.Uint64 src, gen_package.Mask8 mask) => gen_package.BroadcastMasked⁔(x, src, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.Uint64x8 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Uint64x8 wrap(this gen_package.Uint64x8 _, ж<gen_package.op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64x8 /*z*/ Or(this gen_package.Uint64x8 x, gen_package.Uint64x8 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64x8 /*z*/ Sub(this gen_package.Uint64x8 x, gen_package.Uint64x8 y) => gen_package.Sub⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*z*/ ToUint8x64(this gen_package.Uint64x8 x) => gen_package.ToUint8x64⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*z*/ GF2P8Affine(this gen_package.Uint64x8 x, gen_package.Uint8x64 y) => gen_package.GF2P8Affine⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask64 /*z*/ ShuffleBits(this gen_package.Uint64x8 x, gen_package.Uint8x64 y) => gen_package.ShuffleBits⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Mask64 /*z*/ ShuffleBitsMasked(this gen_package.Uint64x8 x, gen_package.Uint8x64 y, gen_package.Mask64 mask) => gen_package.ShuffleBitsMasked⁔(x, y, mask);
}
partial class genᴛUint8x64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.Uint8x64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.Uint8x64 wrap(this gen_package.Uint8x64 _, ж<gen_package.op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint64x8 /*z*/ ToUint64x8(this gen_package.Uint8x64 x) => gen_package.ToUint64x8⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*y*/ Shuffle(this gen_package.Uint8x64 x, gen_package.Uint8x64 shuf) => gen_package.Shuffle⁔(x, shuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*y*/ ShuffleZeroed(this gen_package.Uint8x64 x, gen_package.Uint8x64 shuf, gen_package.Mask64 mask) => gen_package.ShuffleZeroed⁔(x, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*y*/ ShuffleMasked(this gen_package.Uint8x64 x, gen_package.Uint8x64 shuf, gen_package.Mask64 mask) => gen_package.ShuffleMasked⁔(x, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*z*/ Shuffle2(this gen_package.Uint8x64 x, gen_package.Uint8x64 y, gen_package.Uint8x64 shuf) => gen_package.Shuffle2⁔(x, y, shuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*z*/ Shuffle2Zeroed(this gen_package.Uint8x64 x, gen_package.Uint8x64 y, gen_package.Uint8x64 shuf, gen_package.Mask64 mask) => gen_package.Shuffle2Zeroed⁔(x, y, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static gen_package.Uint8x64 /*z*/ Shuffle2Masked(this gen_package.Uint8x64 x, gen_package.Uint8x64 y, gen_package.Uint8x64 shuf, gen_package.Mask64 mask) => gen_package.Shuffle2Masked⁔(x, y, shuf, mask);
}
partial class genᴛlocMem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string LocString(this gen_package.locMem l) => gen_package.LocString⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gen_package.loc, error) Deref(this gen_package.locMem l, nint off) => gen_package.Deref⁔(l, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gen_package.locReg, bool) Reg(this gen_package.locMem l) => gen_package.Reg⁔(l);
}
partial class genᴛlocReg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string LocString(this gen_package.locReg l) => gen_package.LocString⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gen_package.loc, error) Deref(this gen_package.locReg l, nint off) => gen_package.Deref⁔(l, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gen_package.locReg, bool) Reg(this gen_package.locReg l) => gen_package.Reg⁔(l);
}
partial class genᴛop {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref gen_package.op o) => gen_package.String⁔(ref o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool equalNoName(this ref gen_package.op o, ж<gen_package.op> Ꮡo2) => gen_package.equalNoName⁔(ref o, Ꮡo2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void write(this ref gen_package.op o, io.Writer w) => gen_package.write⁔(ref o, w);
}
partial class genᴛopHeap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this gen_package.opHeap h) => gen_package.Len⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void Swap(this gen_package.opHeap h, nint i, nint j) => gen_package.Swap⁔(h, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this gen_package.opHeap h, nint i, nint j) => gen_package.Less⁔(h, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void Push(this ref gen_package.opHeap h, any x) => gen_package.Push⁔(ref h, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Pop(this ref gen_package.opHeap h) => gen_package.Pop⁔(ref h);
}
partial class genᴛregSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void used(this ref gen_package.regSet s, ж<gen_package.op> Ꮡo, gen_package.loc l) => gen_package.used⁔(ref s, Ꮡo, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void free(this ref gen_package.regSet s, gen_package.loc l) => gen_package.free⁔(ref s, l);
}
partial class genᴛvalAny {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void initOp(this ref gen_package.valAny v, ж<gen_package.op> Ꮡx) => gen_package.initOp⁔(ref v, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.op> getOp(this gen_package.valAny v) => gen_package.getOp⁔(v);
}
partial class genᴛvalGP {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gen_package.void isWord(this gen_package.valGP _) => gen_package.isWord⁔(_);
}
partial class genᴛvoid {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gen_package.kind> kind(this gen_package.@void _) => gen_package.kind⁔(_);
}
// </MethodForwarders>
