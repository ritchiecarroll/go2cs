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

using static go.@internal.runtime.gc.@internal.gen_package;
using io = go.io_package;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind<T>(this Array<T> _) where T : Value => gen_package.kind⁔<T>(_);
}
partial class genᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddFunc(this ref File f, ж<Func> Ꮡfn) => gen_package.AddFunc⁔(ref f, Ꮡfn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddConst(this ref File f, @string name, any data) => gen_package.AddConst⁔(ref f, name, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Compile(this ж<File> Ꮡf) => gen_package.Compile⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void emitConst(this ref File f, @string name, any data) => gen_package.emitConst⁔(ref f, name, data);
}
partial class genᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void attach(this ж<Func> Ꮡfn, ж<op> Ꮡx) => gen_package.attach⁔(Ꮡfn, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref Func fn, io.Writer w) => gen_package.write⁔(ref fn, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool cse(this ref Func fn) => gen_package.cse⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool deadcode(this ref Func fn) => gen_package.deadcode⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addLoads(this ж<Func> Ꮡfn) => gen_package.addLoads⁔(Ꮡfn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static map<ж<op>, nint> opRefs(this ref Func fn) => gen_package.opRefs⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void schedule(this ref Func fn) => gen_package.schedule⁔(ref fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void emit(this ref Func fn, ж<File> Ꮡf, map<ж<op>, loc> locs) => gen_package.emit⁔(ref fn, Ꮡf, locs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static map<ж<op>, loc> assignLocs(this ж<Func> Ꮡfn) => gen_package.assignLocs⁔(Ꮡfn);
}
partial class genᴛMask64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this Mask64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Mask64 wrap(this Mask64 _, ж<op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64 /*z*/ ToUint64(this Mask64 x) => gen_package.ToUint64⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask64 /*z*/ Or(this Mask64 x, Mask64 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask64 /*z*/ ShiftLeft(this Mask64 x, uint8 c) => gen_package.ShiftLeft⁔(x, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask64 /*z*/ ShiftRight(this Mask64 x, uint8 c) => gen_package.ShiftRight⁔(x, c);
}
partial class genᴛMask8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this Mask8 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Mask8 wrap(this Mask8 _, ж<op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64 /*z*/ ToUint8(this Mask8 x) => gen_package.ToUint8⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask8 /*z*/ Or(this Mask8 x, Mask8 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask8 /*z*/ ShiftLeft(this Mask8 x, uint8 c) => gen_package.ShiftLeft⁔(x, c);
}
partial class genᴛPtr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind<T>(this Ptr<T> _Δp0) where T : Value => gen_package.kind⁔<T>(_Δp0);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Ptr<T> wrap<T>(this Ptr<T> _, ж<op> Ꮡx) where T : Value => gen_package.wrap⁔<T>(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Ptr<T> /*y*/ AddConst<T>(this Ptr<T> x, nint off) where T : Value => gen_package.AddConst⁔<T>(x, off);
}
partial class genᴛUint64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this Uint64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Uint64 wrap(this Uint64 _, ж<op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
}
partial class genᴛUint64x8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64x8 /*z*/ BroadcastMasked(this Uint64x8 x, Uint64 src, Mask8 mask) => gen_package.BroadcastMasked⁔(x, src, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this Uint64x8 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Uint64x8 wrap(this Uint64x8 _, ж<op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64x8 /*z*/ Or(this Uint64x8 x, Uint64x8 y) => gen_package.Or⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64x8 /*z*/ Sub(this Uint64x8 x, Uint64x8 y) => gen_package.Sub⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*z*/ ToUint8x64(this Uint64x8 x) => gen_package.ToUint8x64⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*z*/ GF2P8Affine(this Uint64x8 x, Uint8x64 y) => gen_package.GF2P8Affine⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask64 /*z*/ ShuffleBits(this Uint64x8 x, Uint8x64 y) => gen_package.ShuffleBits⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Mask64 /*z*/ ShuffleBitsMasked(this Uint64x8 x, Uint8x64 y, Mask64 mask) => gen_package.ShuffleBitsMasked⁔(x, y, mask);
}
partial class genᴛUint8x64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this Uint8x64 _) => gen_package.kind⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Uint8x64 wrap(this Uint8x64 _, ж<op> Ꮡx) => gen_package.wrap⁔(_, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint64x8 /*z*/ ToUint64x8(this Uint8x64 x) => gen_package.ToUint64x8⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*y*/ Shuffle(this Uint8x64 x, Uint8x64 shuf) => gen_package.Shuffle⁔(x, shuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*y*/ ShuffleZeroed(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) => gen_package.ShuffleZeroed⁔(x, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*y*/ ShuffleMasked(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) => gen_package.ShuffleMasked⁔(x, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf) => gen_package.Shuffle2⁔(x, y, shuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2Zeroed(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) => gen_package.Shuffle2Zeroed⁔(x, y, shuf, mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2Masked(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) => gen_package.Shuffle2Masked⁔(x, y, shuf, mask);
}
partial class genᴛlocMem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string LocString(this locMem l) => gen_package.LocString⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (loc, error) Deref(this locMem l, nint off) => gen_package.Deref⁔(l, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (locReg, bool) Reg(this locMem l) => gen_package.Reg⁔(l);
}
partial class genᴛlocReg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string LocString(this locReg l) => gen_package.LocString⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (loc, error) Deref(this locReg l, nint off) => gen_package.Deref⁔(l, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (locReg, bool) Reg(this locReg l) => gen_package.Reg⁔(l);
}
partial class genᴛop {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref op o) => gen_package.String⁔(ref o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool equalNoName(this ref op o, ж<op> Ꮡo2) => gen_package.equalNoName⁔(ref o, Ꮡo2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref op o, io.Writer w) => gen_package.write⁔(ref o, w);
}
partial class genᴛopHeap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this opHeap h) => gen_package.Len⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this opHeap h, nint i, nint j) => gen_package.Swap⁔(h, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this opHeap h, nint i, nint j) => gen_package.Less⁔(h, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ref opHeap h, any x) => gen_package.Push⁔(ref h, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Pop(this ref opHeap h) => gen_package.Pop⁔(ref h);
}
partial class genᴛregSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void used(this ref regSet s, ж<op> Ꮡo, loc l) => gen_package.used⁔(ref s, Ꮡo, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ref regSet s, loc l) => gen_package.free⁔(ref s, l);
}
partial class genᴛvalAny {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initOp(this ref valAny v, ж<op> Ꮡx) => gen_package.initOp⁔(ref v, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<op> getOp(this valAny v) => gen_package.getOp⁔(v);
}
partial class genᴛvalGP {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void isWord(this valGP _) => gen_package.isWord⁔(_);
}
partial class genᴛvoid {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<kind> kind(this @void _) => gen_package.kind⁔(_);
}
// </MethodForwarders>
