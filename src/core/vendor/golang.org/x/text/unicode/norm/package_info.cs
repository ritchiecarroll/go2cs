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
using static go.vendor.golang.org.x.text.unicode.norm_package;
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
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/composition.go", "composition.cs", "ACFMoqqigpSCgoIACBKCgpSuwoKUgoKUgoKUpoIAGTaCgoKCpoKCgoKmgoKokoKmooKUgoKosoKCgpSokoKCgpSCqqKCgoKUgqyygoKClIKClKaCgoKCAAoi4oKClIKUgq7igpSUlL7SuIKCgpSClKqigqiSqJKCgoKCqJKCgqiSgoKqooIAHUCCgpSCgpSClKSkpJSmgoKUgoKUgpSkpKSUqKSmgoKCqqKCgoKCgoKCgpSssoKCgoKCgryigoKCgoKClJSClIKCqMy0gsis1u6CgpSCgoKmgpTKgoKCgoKUlIKCgoK4gpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/forminfo.go", "forminfo.cs", "ADp6ABZEAAgCgsqqtAANHIKUgoKUgqaCpoKmgqq0gpSCgoKokqiSgpSqoqqipoKCgoKCgoIAAx4ACwKCgpSmgoKmgoKokoKkuJKCpLyygoKC3IKUpoKCgoKCgoKCgoKUgoKUpg==")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/input.go", "input.cs", "AA8cgqaCpoKCpoKCpoKClKaCgqampoKCpqamgoKUgpSmgoKUpoKClKaCgpSmooKCgpSUgpSUgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/iter.go", "iter.cs", "AB1AkoKCgoKUgoKCgoKokoKCgoKUgoKCgoKssoKUtLS0lIKUgoKUgoKCgoKuwoKUqJKmgoKokgACEPKmooKCgoKUgoKClIKCpqKCgoKClIKCgpSCgqaigoKCgoKCgoKUgqaCqsKClJSCgoKClKaCqsKCgoKCgoKCgoKUgpSCgqiygpKCgoKCgoKCgoKClILKgoKmgpSmgoKCpoKClIKCgpSUpIKkgoKUlIKCgoKUgoKCgoKCgoKCgqaCgpSClIKClIKCgoKCgpSCpoKCgpSChqKCpqKCgoKClIKClIKCuKaigoKCgoKospKCgoKUgoKClIKClIKCgoKCgoKClIKCgoKClIKmgoSSgoKCgoKCgoKUgoKmtIKCgpSCgoKCgpSUgoKmooKCuIKC")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/normalize.go", "normalize.cs", "ACtakoKCgoKUgoKCqJKCgoKClIKCgqiSgoKCgpSCgoKCgpSUpqKCgoKClIKCgoKUpqiSgoKCgpSCgoKCgpSCgoKClKaUgoKUlKzSgoKUgoKmgoKCgoKClIKCgoKCgoKCgpSCpqKClIKCqsKmgoKUlIKCgoKUgpSCpqKCkoKUgoKUgoKCgoKCgpSmgpSUgqaCpqKCgpSqoqqigqrSgoKClKaq0oKCgpSmruKCgoKCgoKCgoKUgoKUlLiUtKSCtoKCpoKmgpSCgpSUqqKCqqKmgoKClIK4goKClIKUgoKClM6irLKssqaCgoKUlIKCgoKUlIKUgoKCgpS4gqaClKqipoKCgoKUgoKUgoKCpoKUgpSCgoKCgpSCgpSmrOSCgpSAlIKUgoKkgIKkgoKCgpSUgoKClJSCgoKClIKmgoKUqqKClIKUqsKCgpSUgpSCgoKCgoKCppSCgoKUgoKmhJKCgoKCgg==", "120-136:1")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/readwriter.go", "readwriter.cs", "AA8k9KSUgoKUgoKCgriCgpSCgpSCpqiSgoKCpq7CgoIADByygoKCgoKUlIKUgoKUgoKCgpSClIKCAAUQooKCgoI=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/tables15.0.0.go", "tables15.0.0.cs", "ABIoABMsAK8V5iqygpSkpIKUgoKClKSClIKCgpSCgoKClKSClIKCgpSCgoKClIKCgoKUpqqigoKUgoKUgoKUgoKUrLKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpTsgqiSlKSC3ACGBI4IAGrWAaYA/waGDrKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpSssoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKaqooKClIKClIKClIKClOyCqJKUpILcAKQIyhAAcuYBpg==")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/transform.go", "transform.cs", "AAwcrAAJBIKCgoKClIKCgoKmgpSmtIKUgqaY9IKClIKCgpSCpoKCgoKClIKCgoKCgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/norm/trie.go", "trie.cs", "ABEi2tyCnsKCgoKCgoKCgpSClKY=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

using static go.vendor.golang.org.x.text.unicode.norm_package;
using io = go.io_package;
using ꓸꓸꓸbyte = variadic<byte>;

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
[GoMethodHost(typeof(norm_package.Form)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛForm;
[GoMethodHost(typeof(norm_package.Iter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛIter;
[GoMethodHost(typeof(norm_package.Properties)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛProperties;
[GoMethodHost(typeof(norm_package.formInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛformInfo;
[GoMethodHost(typeof(norm_package.input)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛinput;
[GoMethodHost(typeof(norm_package.nfcTrie)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛnfcTrie;
[GoMethodHost(typeof(norm_package.nfkcTrie)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛnfkcTrie;
[GoMethodHost(typeof(norm_package.normReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛnormReader;
[GoMethodHost(typeof(norm_package.normWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛnormWriter;
[GoMethodHost(typeof(norm_package.reorderBuffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛreorderBuffer;
[GoMethodHost(typeof(norm_package.sparseBlocks)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛsparseBlocks;
[GoMethodHost(typeof(norm_package.streamSafe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class normᴛstreamSafe;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class normᴛForm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Properties Properties(this Form f, slice<byte> s) => norm_package.Properties⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Properties PropertiesString(this Form f, @string s) => norm_package.PropertiesString⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this Form f, slice<byte> b) => norm_package.Bytes⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Form f, @string s) => norm_package.String⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsNormal(this Form f, slice<byte> b) => norm_package.IsNormal⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsNormalString(this Form f, @string s) => norm_package.IsNormalString⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Append(this Form f, slice<byte> @out, params ꓸꓸꓸbyte srcʗp) => norm_package.Append⁔(f, @out, srcʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> doAppend(this Form f, slice<byte> @out, input src, nint n) => norm_package.doAppend⁔(f, @out, src, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendString(this Form f, slice<byte> @out, @string src) => norm_package.AppendString⁔(f, @out, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint QuickSpan(this Form f, slice<byte> b) => norm_package.QuickSpan⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Span(this Form f, slice<byte> b, bool atEOF) => norm_package.Span⁔(f, b, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) SpanString(this Form f, @string s, bool atEOF) => norm_package.SpanString⁔(f, s, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint QuickSpanString(this Form f, @string s) => norm_package.QuickSpanString⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint FirstBoundary(this Form f, slice<byte> b) => norm_package.FirstBoundary⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint firstBoundary(this Form f, input src, nint nsrc) => norm_package.firstBoundary⁔(f, src, nsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint FirstBoundaryInString(this Form f, @string s) => norm_package.FirstBoundaryInString⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NextBoundary(this Form f, slice<byte> b, bool atEOF) => norm_package.NextBoundary⁔(f, b, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NextBoundaryInString(this Form f, @string s, bool atEOF) => norm_package.NextBoundaryInString⁔(f, s, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint nextBoundary(this Form f, input src, nint nsrc, bool atEOF) => norm_package.nextBoundary⁔(f, src, nsrc, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint LastBoundary(this Form f, slice<byte> b) => norm_package.LastBoundary⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.WriteCloser Writer(this Form f, io.Writer w) => norm_package.Writer⁔(f, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.Reader Reader(this Form f, io.Reader r) => norm_package.Reader⁔(f, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this Form _) => norm_package.Reset⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint nDst, nint nSrc, error err) Transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) => norm_package.Transform⁔(f, dst, src, atEOF);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint nDst, nint nSrc, error err) transform(this Form f, slice<byte> dst, slice<byte> src, bool atEOF) => norm_package.transform⁔(f, dst, src, atEOF);
}
partial class normᴛIter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Init(this ref Iter i, Form f, slice<byte> src) => norm_package.Init⁔(ref i, f, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void InitString(this ref Iter i, Form f, @string src) => norm_package.InitString⁔(ref i, f, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ref Iter i, int64 offset, nint whence) => norm_package.Seek⁔(ref i, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> returnSlice(this ref Iter i, nint a, nint b) => norm_package.returnSlice⁔(ref i, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Pos(this ref Iter i) => norm_package.Pos⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setDone(this ref Iter i) => norm_package.setDone⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Done(this ref Iter i) => norm_package.Done⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Next(this ж<Iter> Ꮡi) => norm_package.Next⁔(Ꮡi);
}
partial class normᴛProperties {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool BoundaryBefore(this Properties p) => norm_package.BoundaryBefore⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool BoundaryAfter(this Properties p) => norm_package.BoundaryAfter⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isYesC(this Properties p) => norm_package.isYesC⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isYesD(this Properties p) => norm_package.isYesD⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool combinesForward(this Properties p) => norm_package.combinesForward⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool combinesBackward(this Properties p) => norm_package.combinesBackward⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasDecomposition(this Properties p) => norm_package.hasDecomposition⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isInert(this Properties p) => norm_package.isInert⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool multiSegment(this Properties p) => norm_package.multiSegment⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 nLeadingNonStarters(this Properties p) => norm_package.nLeadingNonStarters⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 nTrailingNonStarters(this Properties p) => norm_package.nTrailingNonStarters⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Decomposition(this Properties p) => norm_package.Decomposition⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this Properties p) => norm_package.Size⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint8 CCC(this Properties p) => norm_package.CCC⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint8 LeadCCC(this Properties p) => norm_package.LeadCCC⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint8 TrailCCC(this Properties p) => norm_package.TrailCCC⁔(p);
}
partial class normᴛformInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, bool ok) quickSpan(this ref formInfo f, input src, nint i, nint end, bool atEOF) => norm_package.quickSpan⁔(ref f, src, i, end, atEOF);
}
partial class normᴛinput {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBytes(this ref input @in, slice<byte> str) => norm_package.setBytes⁔(ref @in, str);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setString(this ref input @in, @string str) => norm_package.setString⁔(ref @in, str);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte _byte(this ref input @in, nint p) => norm_package._byte⁔(ref @in, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint skipASCII(this ref input @in, nint p, nint max) => norm_package.skipASCII⁔(ref @in, p, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint skipContinuationBytes(this ref input @in, nint p) => norm_package.skipContinuationBytes⁔(ref @in, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendSlice(this ref input @in, slice<byte> buf, nint b, nint e) => norm_package.appendSlice⁔(ref @in, buf, b, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint copySlice(this ref input @in, slice<byte> buf, nint b, nint e) => norm_package.copySlice⁔(ref @in, buf, b, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16, nint) charinfoNFC(this ref input @in, nint p) => norm_package.charinfoNFC⁔(ref @in, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16, nint) charinfoNFKC(this ref input @in, nint p) => norm_package.charinfoNFKC⁔(ref @in, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune /*r*/ hangul(this ref input @in, nint p) => norm_package.hangul⁔(ref @in, p);
}
partial class normᴛnfcTrie {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16 v, nint sz) lookup(this ref nfcTrie t, slice<byte> s) => norm_package.lookup⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupUnsafe(this ref nfcTrie t, slice<byte> s) => norm_package.lookupUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16 v, nint sz) lookupString(this ref nfcTrie t, @string s) => norm_package.lookupString⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupStringUnsafe(this ref nfcTrie t, @string s) => norm_package.lookupStringUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupValue(this ref nfcTrie t, uint32 n, byte b) => norm_package.lookupValue⁔(ref t, n, b);
}
partial class normᴛnfkcTrie {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16 v, nint sz) lookup(this ref nfkcTrie t, slice<byte> s) => norm_package.lookup⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupUnsafe(this ref nfkcTrie t, slice<byte> s) => norm_package.lookupUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16 v, nint sz) lookupString(this ref nfkcTrie t, @string s) => norm_package.lookupString⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupStringUnsafe(this ref nfkcTrie t, @string s) => norm_package.lookupStringUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookupValue(this ref nfkcTrie t, uint32 n, byte b) => norm_package.lookupValue⁔(ref t, n, b);
}
partial class normᴛnormReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<normReader> Ꮡr, slice<byte> p) => norm_package.Read⁔(Ꮡr, p);
}
partial class normᴛnormWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<normWriter> Ꮡw, slice<byte> data) => norm_package.Write⁔(Ꮡw, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref normWriter w) => norm_package.Close⁔(ref w);
}
partial class normᴛreorderBuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref reorderBuffer rb, Form f, slice<byte> src) => norm_package.init⁔(ref rb, f, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initString(this ref reorderBuffer rb, Form f, @string src) => norm_package.initString⁔(ref rb, f, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setFlusher(this ref reorderBuffer rb, slice<byte> @out, Func<ж<reorderBuffer>, bool> f) => norm_package.setFlusher⁔(ref rb, @out, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref reorderBuffer rb) => norm_package.reset⁔(ref rb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool doFlush(this ж<reorderBuffer> Ꮡrb) => norm_package.doFlush⁔(Ꮡrb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> flush(this ref reorderBuffer rb, slice<byte> @out) => norm_package.flush⁔(ref rb, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint flushCopy(this ref reorderBuffer rb, slice<byte> buf) => norm_package.flushCopy⁔(ref rb, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertOrdered(this ref reorderBuffer rb, Properties info) => norm_package.insertOrdered⁔(ref rb, info);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static insertErr insertFlush(this ж<reorderBuffer> Ꮡrb, input src, nint i, Properties info) => norm_package.insertFlush⁔(Ꮡrb, src, i, info);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertUnsafe(this ж<reorderBuffer> Ꮡrb, input src, nint i, Properties info) => norm_package.insertUnsafe⁔(Ꮡrb, src, i, info);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static insertErr insertDecomposed(this ж<reorderBuffer> Ꮡrb, slice<byte> dcomp) => norm_package.insertDecomposed⁔(Ꮡrb, dcomp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertSingle(this ref reorderBuffer rb, input src, nint i, Properties info) => norm_package.insertSingle⁔(ref rb, src, i, info);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertCGJ(this ref reorderBuffer rb) => norm_package.insertCGJ⁔(ref rb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendRune(this ref reorderBuffer rb, rune r) => norm_package.appendRune⁔(ref rb, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignRune(this ref reorderBuffer rb, nint pos, rune r) => norm_package.assignRune⁔(ref rb, pos, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune runeAt(this ref reorderBuffer rb, nint n) => norm_package.runeAt⁔(ref rb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesAt(this ref reorderBuffer rb, nint n) => norm_package.bytesAt⁔(ref rb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decomposeHangul(this ref reorderBuffer rb, rune r) => norm_package.decomposeHangul⁔(ref rb, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void combineHangul(this ref reorderBuffer rb, nint s, nint i, nint k) => norm_package.combineHangul⁔(ref rb, s, i, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compose(this ref reorderBuffer rb) => norm_package.compose⁔(ref rb);
}
partial class normᴛsparseBlocks {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 lookup(this ref sparseBlocks t, uint32 n, byte b) => norm_package.lookup⁔(ref t, n, b);
}
partial class normᴛstreamSafe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void first(this ref streamSafe ss, Properties p) => norm_package.first⁔(ref ss, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ssState next(this ref streamSafe ss, Properties p) => norm_package.next⁔(ref ss, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ssState backwards(this ref streamSafe ss, Properties p) => norm_package.backwards⁔(ref ss, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isMax(this streamSafe ss) => norm_package.isMax⁔(ss);
}
// </MethodForwarders>
