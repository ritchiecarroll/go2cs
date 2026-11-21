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
using static go.vendor.golang.org.x.text.unicode.norm_package;

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
[assembly: GoImplement<normReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<normWriter, io_package.WriteCloser>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Iter))]
[assembly: GoFieldAccess(typeof(formInfo))]
[assembly: GoFieldAccess(typeof(normReader))]
[assembly: GoFieldAccess(typeof(normWriter))]
[assembly: GoFieldAccess(typeof(reorderBuffer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/composition.go", "composition.cs", "ACRMoqqigpSCgoIACBKCgpSuwoKUgoKUgoKUpoIAHTaCgoKCpoKCgoKmgoKokoKmooKUgoLIsoKCgpTIkoKCgpSCqqKCgoKUgqyygoKClIKClKaCgoKCAA4i4oKClIKUgq7igpSUlL7SuIKCgpSClKqigqiSqJKCgoKCqJKCgqiSgoKqooIAH0CCgpSCgpSClKSkpJSmgoKUgoKUgpSkpKSUqKSmgoKCqqKCgoKCgoKCgpTMsoKCgoKCgryigoKCgoKClJSClIKCqMy0gsis1u6CgpSCgoKmgpTKgoKCgoKUlIKCgoK4gpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/forminfo.go", "forminfo.cs", "ADt6ABhEAAgCgsqqtAARHIKUgoKUgqaCpoKmgqq0gpSCgoKokqiSgpSqoqqixoKCgoKCgoIAAx4ACwKCgpSmgoKmgoLIkoKkuJKCpNyygoKC3IKUpoKCgoKCgoKCgoKUgoKUpg==")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/input.go", "input.cs", "ABAcgqaCxoKCpoKCpoKClKaCgqampoKCpqamgoKUgpSmgoKUpoKClKaCgpSmooKCgpSUgpSUgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/iter.go", "iter.cs", "ACBAkoKCgoKUgoKCgoKokoKCgoKUgoKCgoKssoKUtLS0lIKUgoKUgoKCgoKuwoKUqJKmgoKokgACEPLGooKCgoKUgoKClIKCpqKCgoKClIKCgpSCgqaigoKCgoKCgoKUgqaCqsKClJSCgoKClKaCqsKCgoKCgoKCgoKUgpSCgqiygpKCgoKCgoKCgoKClILKgoKmgpSmgoKCpoKClIKCgpSUpIKkgoKUlIKCgoKUgoKCgoKCgoKCgqaCgpSClIKClIKCgoKCgpSCpoKCgpSChqKCpqKCgoKClIKClIKCuKaigoKCgoKospKCgoKUgoKClIKClIKCgoKCgoKClIKCgoKClIKmgoSSgoKCgoKCgoKUgoKmtIKCgpSCgoKCgpSUgoKmooKCuIKC")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/normalize.go", "normalize.cs", "AC9akoKCgoKUgoKCqJKCgoKClIKCgqiSgoKCgpSCgoKCgpSUxqKCgoKClIKCgoKUpsiSgoKCgpSCgoKCgpSCgoKClKaUgoKUlMzSgoKUgoKmgoKCgoKClIKCgoKCgoKCgpSCpqKClIKCysKmgoKUlIKCgoKUgpSCxqKCkoKUgoKUgoKCgoKCgpSmgpSUgqaCpqKCgpTKoqqigqrSgoKClKaq0oKCgpSmzuKCgoKCgoKCgoKUgoKUlLiUtKSCtoKCpoKmgpSCgpSUyqKCqqKmgoKClIK4goKClIKUgoKClM6irLKssqaCgoKUlIKCgoKUlIKUgoKCgpS4gqaClKqixoKCgoKUgoKUgoKCpoKUgpSCgoKCgpSCgpSmrOSCgpSAlIKUgoKkgIKkgoKCgpSUgoKClJSCgoKClIKmgoKUqqKClIKUqsKCgpSUgpSCgoKCgoKCppSCgoKUgoKmhJKCgoKCgg==", "120-136:1")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/readwriter.go", "readwriter.cs", "ABIk9KSUgoKUgoKCgriCgpSCgpSCpqiSgoKCps7CgoIAEByygoKCgoKUlIKUgoKUgoKCgpSClIKCAAcQooKCgoI=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/tables15.0.0.go", "tables15.0.0.cs", "ABMoABMsALEV5iqygpSkpIKUgoKClKSClIKCgpSCgoKClKSClIKCgpSCgoKClIKCgoKUpqqigoKUgoKUgoKUgoKUrLKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpQACAyCyJKUpIL8AIYEjggAatYBpgCBB4YOsoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKaqooKClIKClIKClIKClKyygpSkpIKUgoKClKSClIKCgpSCgoKClKSClIKCgpSCgoKClIKCgoKUpqqigoKUgoKUgoKUgoKUAAgMgsiSlKSC/ACkCMoQAHLmAaY=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/transform.go", "transform.cs", "AA8crAAJBIKCgoKClIKCgoKmgpTGtIKUgqa49IKClIKCgpSCpoKCgoKClIKCgoKCgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/trie.go", "trie.cs", "ABMi2tyCvsKCgoKCgoKCgpSClKY=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static norm_package;
using binary = encoding.binary_package;
using encoding;
using fmt = fmt_package;
using transform = go.vendor.golang.org.x.text.transform_package;
using go.vendor.golang.org.x.text;
using ꓸꓸꓸbyte = Span<byte>;
using norm = norm_package;
using io = io_package;
using sync = sync_package;
// </GlyphForwarderUsings>

[GoPackage("norm", ImportPath = "vendor/golang.org/x/text/unicode/norm")]
public static partial class norm_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct formInfo {}
    internal partial struct input {}
    internal partial struct insertErr {}
    internal partial struct nfcTrie {}
    internal partial struct nfkcTrie {}
    [GoValueClone("rb")] internal partial struct normReader {}
    [GoValueClone("rb")] internal partial struct normWriter {}
    internal partial struct qcInfo {}
    [GoValueClone("rune", "@byte")] internal partial struct reorderBuffer {}
    internal partial struct sparseBlocks {}
    internal partial struct ssState {}
    internal partial struct streamSafe {}
    internal partial struct valueRange {}
    public partial struct Form {}
    [GoValueClone("rb", "buf")] public partial struct Iter {}
    public partial struct Properties {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸtransform() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.transform_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(norm_package.Form)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛForm;
[GoMethodHost(typeof(norm_package.Iter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛIter;
[GoMethodHost(typeof(norm_package.Properties)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛProperties;
[GoMethodHost(typeof(norm_package.formInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛformInfo;
[GoMethodHost(typeof(norm_package.input)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛinput;
[GoMethodHost(typeof(norm_package.nfcTrie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛnfcTrie;
[GoMethodHost(typeof(norm_package.nfkcTrie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛnfkcTrie;
[GoMethodHost(typeof(norm_package.normReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛnormReader;
[GoMethodHost(typeof(norm_package.normWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛnormWriter;
[GoMethodHost(typeof(norm_package.reorderBuffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛreorderBuffer;
[GoMethodHost(typeof(norm_package.sparseBlocks)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛsparseBlocks;
[GoMethodHost(typeof(norm_package.streamSafe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class normᴛstreamSafe;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class normᴛstreamSafe {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void first(this ref streamSafe ss, Properties p) => norm_package.firstᴍ(ref ss, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ssState next(this ref streamSafe ss, Properties p) => norm_package.nextᴍ(ref ss, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ssState backwards(this ref streamSafe ss, Properties p) => norm_package.backwardsᴍ(ref ss, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isMax(this streamSafe ss) => norm_package.isMaxᴍ(ss);
}
partial class normᴛreorderBuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref reorderBuffer rb, Form f, slice<byte> src) => norm_package.initᴍ(ref rb, f, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initString(this ref reorderBuffer rb, Form f, @string src) => norm_package.initStringᴍ(ref rb, f, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setFlusher(this ref reorderBuffer rb, slice<byte> @out, Func<ж<reorderBuffer>, bool> f) => norm_package.setFlusherᴍ(ref rb, @out, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref reorderBuffer rb) => norm_package.resetᴍ(ref rb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool doFlush(this ж<reorderBuffer> Ꮡrb) => norm_package.doFlushᴍ(Ꮡrb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> flush(this ref reorderBuffer rb, slice<byte> @out) => norm_package.flushᴍ(ref rb, @out);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint flushCopy(this ref reorderBuffer rb, slice<byte> buf) => norm_package.flushCopyᴍ(ref rb, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertOrdered(this ref reorderBuffer rb, Properties info) => norm_package.insertOrderedᴍ(ref rb, info);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static insertErr insertFlush(this ж<reorderBuffer> Ꮡrb, input src, nint i, Properties info) => norm_package.insertFlushᴍ(Ꮡrb, src, i, info);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertUnsafe(this ж<reorderBuffer> Ꮡrb, input src, nint i, Properties info) => norm_package.insertUnsafeᴍ(Ꮡrb, src, i, info);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static insertErr insertDecomposed(this ж<reorderBuffer> Ꮡrb, slice<byte> dcomp) => norm_package.insertDecomposedᴍ(Ꮡrb, dcomp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertSingle(this ref reorderBuffer rb, input src, nint i, Properties info) => norm_package.insertSingleᴍ(ref rb, src, i, info);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertCGJ(this ref reorderBuffer rb) => norm_package.insertCGJᴍ(ref rb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendRune(this ref reorderBuffer rb, rune r) => norm_package.appendRuneᴍ(ref rb, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assignRune(this ref reorderBuffer rb, nint pos, rune r) => norm_package.assignRuneᴍ(ref rb, pos, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune runeAt(this ref reorderBuffer rb, nint n) => norm_package.runeAtᴍ(ref rb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> bytesAt(this ref reorderBuffer rb, nint n) => norm_package.bytesAtᴍ(ref rb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decomposeHangul(this ref reorderBuffer rb, rune r) => norm_package.decomposeHangulᴍ(ref rb, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void combineHangul(this ref reorderBuffer rb, nint s, nint i, nint k) => norm_package.combineHangulᴍ(ref rb, s, i, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compose(this ref reorderBuffer rb) => norm_package.composeᴍ(ref rb);
}
partial class normᴛProperties {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool BoundaryBefore(this Properties p) => norm_package.BoundaryBeforeᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool BoundaryAfter(this Properties p) => norm_package.BoundaryAfterᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isYesC(this Properties p) => norm_package.isYesCᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isYesD(this Properties p) => norm_package.isYesDᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool combinesForward(this Properties p) => norm_package.combinesForwardᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool combinesBackward(this Properties p) => norm_package.combinesBackwardᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasDecomposition(this Properties p) => norm_package.hasDecompositionᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isInert(this Properties p) => norm_package.isInertᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool multiSegment(this Properties p) => norm_package.multiSegmentᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint8 nLeadingNonStarters(this Properties p) => norm_package.nLeadingNonStartersᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint8 nTrailingNonStarters(this Properties p) => norm_package.nTrailingNonStartersᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Decomposition(this Properties p) => norm_package.Decompositionᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Size(this Properties p) => norm_package.Sizeᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint8 CCC(this Properties p) => norm_package.CCCᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint8 LeadCCC(this Properties p) => norm_package.LeadCCCᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint8 TrailCCC(this Properties p) => norm_package.TrailCCCᴍ(p);
}
partial class normᴛForm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Properties Properties(this Form f, slice<byte> s) => norm_package.Propertiesᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Properties PropertiesString(this Form f, @string s) => norm_package.PropertiesStringᴍ(f, s);
}
partial class normᴛinput {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setBytes(this ref input @in, slice<byte> str) => norm_package.setBytesᴍ(ref @in, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setString(this ref input @in, @string str) => norm_package.setStringᴍ(ref @in, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte _byte(this ref input @in, nint p) => norm_package._byteᴍ(ref @in, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint skipASCII(this ref input @in, nint p, nint max) => norm_package.skipASCIIᴍ(ref @in, p, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint skipContinuationBytes(this ref input @in, nint p) => norm_package.skipContinuationBytesᴍ(ref @in, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendSlice(this ref input @in, slice<byte> buf, nint b, nint e) => norm_package.appendSliceᴍ(ref @in, buf, b, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint copySlice(this ref input @in, slice<byte> buf, nint b, nint e) => norm_package.copySliceᴍ(ref @in, buf, b, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16, nint) charinfoNFC(this ref input @in, nint p) => norm_package.charinfoNFCᴍ(ref @in, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16, nint) charinfoNFKC(this ref input @in, nint p) => norm_package.charinfoNFKCᴍ(ref @in, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune /*r*/ hangul(this ref input @in, nint p) => norm_package.hangulᴍ(ref @in, p);
}
partial class normᴛIter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Init(this ref Iter i, Form f, slice<byte> src) => norm_package.Initᴍ(ref i, f, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void InitString(this ref Iter i, Form f, @string src) => norm_package.InitStringᴍ(ref i, f, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) Seek(this ref Iter i, int64 offset, nint whence) => norm_package.Seekᴍ(ref i, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> returnSlice(this ref Iter i, nint a, nint b) => norm_package.returnSliceᴍ(ref i, a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Pos(this ref Iter i) => norm_package.Posᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setDone(this ref Iter i) => norm_package.setDoneᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Done(this ref Iter i) => norm_package.Doneᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Next(this ж<Iter> Ꮡi) => norm_package.Nextᴍ(Ꮡi);
}
partial class normᴛForm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Bytes(this Form f, slice<byte> b) => norm_package.Bytesᴍ(f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Form f, @string s) => norm_package.Stringᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsNormal(this Form f, slice<byte> b) => norm_package.IsNormalᴍ(f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsNormalString(this Form f, @string s) => norm_package.IsNormalStringᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Append(this Form f, slice<byte> @out, params ꓸꓸꓸbyte srcʗp) => norm_package.Appendᴍ(f, @out, srcʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> doAppend(this Form f, slice<byte> @out, input src, nint n) => norm_package.doAppendᴍ(f, @out, src, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> AppendString(this Form f, slice<byte> @out, @string src) => norm_package.AppendStringᴍ(f, @out, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint QuickSpan(this Form f, slice<byte> b) => norm_package.QuickSpanᴍ(f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Span(this Form f, slice<byte> b, bool atEOF) => norm_package.Spanᴍ(f, b, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) SpanString(this Form f, @string s, bool atEOF) => norm_package.SpanStringᴍ(f, s, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint QuickSpanString(this Form f, @string s) => norm_package.QuickSpanStringᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint FirstBoundary(this Form f, slice<byte> b) => norm_package.FirstBoundaryᴍ(f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint firstBoundary(this Form f, input src, nint nsrc) => norm_package.firstBoundaryᴍ(f, src, nsrc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint FirstBoundaryInString(this Form f, @string s) => norm_package.FirstBoundaryInStringᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NextBoundary(this Form f, slice<byte> b, bool atEOF) => norm_package.NextBoundaryᴍ(f, b, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NextBoundaryInString(this Form f, @string s, bool atEOF) => norm_package.NextBoundaryInStringᴍ(f, s, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint nextBoundary(this Form f, input src, nint nsrc, bool atEOF) => norm_package.nextBoundaryᴍ(f, src, nsrc, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint LastBoundary(this Form f, slice<byte> b) => norm_package.LastBoundaryᴍ(f, b);
}
partial class normᴛformInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, bool ok) quickSpan(this ref formInfo f, input src, nint i, nint end, bool atEOF) => norm_package.quickSpanᴍ(ref f, src, i, end, atEOF);
}
partial class normᴛnormWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<normWriter> Ꮡw, slice<byte> data) => norm_package.Writeᴍ(Ꮡw, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref normWriter w) => norm_package.Closeᴍ(ref w);
}
partial class normᴛForm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static io.WriteCloser Writer(this Form f, io.Writer w) => norm_package.Writerᴍ(f, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static io.Reader Reader(this Form f, io.Reader r) => norm_package.Readerᴍ(f, r);
}
partial class normᴛnormReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ж<normReader> Ꮡr, slice<byte> p) => norm_package.Readᴍ(Ꮡr, p);
}
partial class normᴛnfcTrie {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookup(this ref nfcTrie t, slice<byte> s) => norm_package.lookupᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupUnsafe(this ref nfcTrie t, slice<byte> s) => norm_package.lookupUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookupString(this ref nfcTrie t, @string s) => norm_package.lookupStringᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupStringUnsafe(this ref nfcTrie t, @string s) => norm_package.lookupStringUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupValue(this ref nfcTrie t, uint32 n, byte b) => norm_package.lookupValueᴍ(ref t, n, b);
}
partial class normᴛnfkcTrie {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookup(this ref nfkcTrie t, slice<byte> s) => norm_package.lookupᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupUnsafe(this ref nfkcTrie t, slice<byte> s) => norm_package.lookupUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookupString(this ref nfkcTrie t, @string s) => norm_package.lookupStringᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupStringUnsafe(this ref nfkcTrie t, @string s) => norm_package.lookupStringUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupValue(this ref nfkcTrie t, uint32 n, byte b) => norm_package.lookupValueᴍ(ref t, n, b);
}
partial class normᴛForm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this Form _) => norm_package.Resetᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint nDst, nint nSrc, error err) Transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) => norm_package.Transformᴍ(f, dst, src, atEOF);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint nDst, nint nSrc, error err) transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) => norm_package.transformᴍ(f, dst, src, atEOF);
}
partial class normᴛsparseBlocks {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookup(this ref sparseBlocks t, uint32 n, byte b) => norm_package.lookupᴍ(ref t, n, b);
}
// </GlyphForwarders>
