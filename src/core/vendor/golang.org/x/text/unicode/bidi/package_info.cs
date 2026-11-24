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
using static go.vendor.golang.org.x.text.unicode.bidi_package;
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
[assembly: GoImplement<bracketPairs, sort_package.Interface>]
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
[assembly: GoFieldAccess(typeof(Paragraph))]
[assembly: GoFieldAccess(typeof(options))]
[assembly: GoFieldAccess(typeof(paragraph))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bidi.go", "bidi.cs", "ADeKAQALAoIADzIACQKClIKClIKCgoKClIKCgoKmgpSCpgACEPKCggACEPKCgqyyrLIAAhDSgoKCgoKmpoKUgpSmgoKUlIKCgoKUgqaCgoKosoKmgpSCgpSCgqaUgqqigoKClIKCAAsesqiSqJLKABAkkqiSAAIS4qqirLKCgpSCgoK4gpSUrLKCgoKCgoKUpg==", "70-72:1")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bracket.go", "bracket.cs", "ADVwgtyCggABFAAIAtyCgpSCABxKogAEFPaWlJSmgoKmyIKCgoKUlIKUpgAHfAA3AqakpAADFvKCgoKClIKUpqqigoK4qKTcgqaUgpQABxCmgoKUgoKClKaCgoKUupKC", "", "", "74=New/1/1/2")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/core.go", "core.cs", "ADt6soKCpgAXSPKCgpSClIKUgqYACBKCppjC3IK4grgACRSmuLi4ptwAAiQADwKClIKmgpSCgoKCgoKCgoK4ggAIFtKmgoKCgoKCgsqmpKQADh6CgpSCgoKCpoKmgqaCqJKCxpSUlIKmgpSCgoK4gpSmpoKC3JS0tLaCyoKUgv6CgqaCgpSClNqUlIKCggAIEIKCgoK2goIADyKYwoKCgriCgpSCgqaCgoKmgpaCgqaCAAkc1riCgoLK3IKCgoKClO6CggAQIoKCgoKCgoKC3KKUgqaCgpSCgoKmgqbKgoLKopSCgoKCgqaC3qaUoqaCltiClIKCpoKUgoK4gpTKpqbagoK4goK6ppSClIKUlIKUuJSmAAUQooKCvtKCgoKCgqa0qsKCgoKCpgAFFOKCgpSCgpSCgqaUuIKUqMKmgoKCuJSUooKClJSUgoKClKamvsKCgoLugpSCggAGIgAVGJTKgpSmgoKU7oKCgoKUpqYAAiQADwKUqqKUgoKClIKUlKyylILcgoKCgpSCuIKClIKCgqaCpsqqopSUqJKUlKiSgpSmgoKUgoKmpoKGlKaCgoKClJSClKaCgpSCxqamgoKUgpQ=", "", "", "480=typeForLevel/1/2/5,typeForLevel/2/2/6")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/prop.go", "prop.cs", "ABAeAAEaAAsCgoKUqICYkJaSpgALGpKCggACKgASAoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKassoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKY=")]
[assembly: GoPositionMap("vendor/golang.org/x/text/unicode/bidi/tables15.0.0.go", "tables15.0.0.cs", "AAwYvrKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpSssoKUpKSClIKCgpSkgpSCgoKUgoKCgpSkgpSCgoKUgoKCgpSCgoKClKaqooKClIKClIKClIKClOyCqJKU3ACwDeIa")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

using static go.vendor.golang.org.x.text.unicode.bidi_package;
using ꓸꓸꓸClass = Span<bidi_package.Class>;
using ꓸꓸꓸOption = Span<bidi_package.Option>;

[GoPackage("bidi", ImportPath = "vendor/golang.org/x/text/unicode/bidi")]
public static partial class bidi_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bidiTrie {}
    internal partial struct bracketPair {}
    internal partial struct bracketPairer {}
    internal partial struct bracketPairs {}
    internal partial struct bracketType {}
    [GoValueClone("embeddingLevelStack", "overrideStatusStack", "isolateStatusStack")] internal partial struct directionalStatusStack {}
    internal partial struct isolatingRunSequence {}
    internal partial struct level {}
    internal partial struct paragraph {}
    public partial struct Class {}
    public partial struct Direction {}
    public partial struct Ordering {}
    public partial struct Paragraph {}
    public partial struct Properties {}
    public partial struct Run {}
    public partial struct options {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bidi_package.Class)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛClass;
[GoMethodHost(typeof(bidi_package.Ordering)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛOrdering;
[GoMethodHost(typeof(bidi_package.Paragraph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛParagraph;
[GoMethodHost(typeof(bidi_package.Properties)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛProperties;
[GoMethodHost(typeof(bidi_package.Run)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛRun;
[GoMethodHost(typeof(bidi_package.bidiTrie)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛbidiTrie;
[GoMethodHost(typeof(bidi_package.bracketPair)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛbracketPair;
[GoMethodHost(typeof(bidi_package.bracketPairer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛbracketPairer;
[GoMethodHost(typeof(bidi_package.bracketPairs)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛbracketPairs;
[GoMethodHost(typeof(bidi_package.directionalStatusStack)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛdirectionalStatusStack;
[GoMethodHost(typeof(bidi_package.isolatingRunSequence)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛisolatingRunSequence;
[GoMethodHost(typeof(bidi_package.paragraph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bidiᴛparagraph;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class bidiᴛClass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @in(this Class c, params ꓸꓸꓸClass setʗp) => bidi_package.in⁔(c, setʗp);
}
partial class bidiᴛOrdering {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Direction Direction(this ref Ordering o) => bidi_package.Direction⁔(ref o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumRuns(this ref Ordering o) => bidi_package.NumRuns⁔(ref o);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Run Run(this ref Ordering o, nint i) => bidi_package.Run⁔(ref o, i);
}
partial class bidiᴛParagraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) prepareInput(this ref Paragraph p) => bidi_package.prepareInput⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) SetBytes(this ref Paragraph p, slice<byte> b, params ꓸꓸꓸOption optsʗp) => bidi_package.SetBytes⁔(ref p, b, optsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) SetString(this ref Paragraph p, @string s, params ꓸꓸꓸOption optsʗp) => bidi_package.SetString⁔(ref p, s, optsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLeftToRight(this ref Paragraph p) => bidi_package.IsLeftToRight⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Direction Direction(this ref Paragraph p) => bidi_package.Direction⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Run RunAt(this ref Paragraph p, nint pos) => bidi_package.RunAt⁔(ref p, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Ordering, error) Order(this ж<Paragraph> Ꮡp) => bidi_package.Order⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Ordering, error) Line(this ref Paragraph p, nint start, nint end) => bidi_package.Line⁔(ref p, start, end);
}
partial class bidiᴛProperties {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Class Class(this Properties p) => bidi_package.Class⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsBracket(this Properties p) => bidi_package.IsBracket⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsOpeningBracket(this Properties p) => bidi_package.IsOpeningBracket⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune reverseBracket(this Properties p, rune r) => bidi_package.reverseBracket⁔(p, r);
}
partial class bidiᴛRun {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Run r) => bidi_package.String⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Run r) => bidi_package.Bytes⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Direction Direction(this ref Run r) => bidi_package.Direction⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint start, nint end) Pos(this ref Run r) => bidi_package.Pos⁔(ref r);
}
partial class bidiᴛbidiTrie {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint8 v, nint sz) lookup(this ref bidiTrie t, slice<byte> s) => bidi_package.lookup⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 lookupUnsafe(this ref bidiTrie t, slice<byte> s) => bidi_package.lookupUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint8 v, nint sz) lookupString(this ref bidiTrie t, @string s) => bidi_package.lookupString⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 lookupStringUnsafe(this ref bidiTrie t, @string s) => bidi_package.lookupStringUnsafe⁔(ref t, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 lookupValue(this ref bidiTrie t, uint32 n, byte b) => bidi_package.lookupValue⁔(ref t, n, b);
}
partial class bidiᴛbracketPair {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref bracketPair b) => bidi_package.String⁔(ref b);
}
partial class bidiᴛbracketPairer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool matchOpener(this ref bracketPairer p, slice<rune> pairValues, nint opener, nint closer) => bidi_package.matchOpener⁔(ref p, pairValues, opener, closer);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void locateBrackets(this ref bracketPairer p, slice<bracketType> pairTypes, slice<rune> pairValues) => bidi_package.locateBrackets⁔(ref p, pairTypes, pairValues);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Class getStrongTypeN0(this ref bracketPairer p, nint index) => bidi_package.getStrongTypeN0⁔(ref p, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Class classifyPairContent(this ref bracketPairer p, bracketPair loc, Class dirEmbed) => bidi_package.classifyPairContent⁔(ref p, loc, dirEmbed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Class classBeforePair(this ref bracketPairer p, bracketPair loc) => bidi_package.classBeforePair⁔(ref p, loc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignBracketType(this ref bracketPairer p, bracketPair loc, Class dirEmbed, slice<Class> initialTypes) => bidi_package.assignBracketType⁔(ref p, loc, dirEmbed, initialTypes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBracketsToType(this ref bracketPairer p, bracketPair loc, Class dirPair, slice<Class> initialTypes) => bidi_package.setBracketsToType⁔(ref p, loc, dirPair, initialTypes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveBrackets(this ref bracketPairer p, Class dirEmbed, slice<Class> initialTypes) => bidi_package.resolveBrackets⁔(ref p, dirEmbed, initialTypes);
}
partial class bidiᴛbracketPairs {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this bracketPairs b) => bidi_package.Len⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this bracketPairs b, nint i, nint j) => bidi_package.Swap⁔(b, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this bracketPairs b, nint i, nint j) => bidi_package.Less⁔(b, i, j);
}
partial class bidiᴛdirectionalStatusStack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void empty(this ref directionalStatusStack s) => bidi_package.empty⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pop(this ref directionalStatusStack s) => bidi_package.pop⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint depth(this ref directionalStatusStack s) => bidi_package.depth⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref directionalStatusStack s, level level, Class overrideStatus, bool isolateStatus) => bidi_package.push⁔(ref s, level, overrideStatus, isolateStatus);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static level lastEmbeddingLevel(this ref directionalStatusStack s) => bidi_package.lastEmbeddingLevel⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Class lastDirectionalOverrideStatus(this ref directionalStatusStack s) => bidi_package.lastDirectionalOverrideStatus⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool lastDirectionalIsolateStatus(this ref directionalStatusStack s) => bidi_package.lastDirectionalIsolateStatus⁔(ref s);
}
partial class bidiᴛisolatingRunSequence {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref isolatingRunSequence i) => bidi_package.Len⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveWeakTypes(this ref isolatingRunSequence s) => bidi_package.resolveWeakTypes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveNeutralTypes(this ref isolatingRunSequence s) => bidi_package.resolveNeutralTypes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveImplicitLevels(this ref isolatingRunSequence s) => bidi_package.resolveImplicitLevels⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void applyLevelsAndTypes(this ref isolatingRunSequence s) => bidi_package.applyLevelsAndTypes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint findRunLimit(this ref isolatingRunSequence s, nint index, params ꓸꓸꓸClass validSetʗp) => bidi_package.findRunLimit⁔(ref s, index, validSetʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assertOnly(this ref isolatingRunSequence s, params ꓸꓸꓸClass codesʗp) => bidi_package.assertOnly⁔(ref s, codesʗp);
}
partial class bidiᴛparagraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref paragraph p) => bidi_package.Len⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void run(this ж<paragraph> Ꮡp) => bidi_package.run⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void determineMatchingIsolates(this ref paragraph p) => bidi_package.determineMatchingIsolates⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static level determineParagraphEmbeddingLevel(this ref paragraph p, nint start, nint end) => bidi_package.determineParagraphEmbeddingLevel⁔(ref p, start, end);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void determineExplicitEmbeddingLevels(this ref paragraph p) => bidi_package.determineExplicitEmbeddingLevels⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<isolatingRunSequence> isolatingRunSequence(this ж<paragraph> Ꮡp, slice<nint> indexes) => bidi_package.isolatingRunSequence⁔(Ꮡp, indexes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<slice<nint>> determineLevelRuns(this ref paragraph p) => bidi_package.determineLevelRuns⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<isolatingRunSequence>> determineIsolatingRunSequences(this ж<paragraph> Ꮡp) => bidi_package.determineIsolatingRunSequences⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignLevelsToCharactersRemovedByX9(this ref paragraph p) => bidi_package.assignLevelsToCharactersRemovedByX9⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<level> getLevels(this ref paragraph p, slice<nint> linebreaks) => bidi_package.getLevels⁔(ref p, linebreaks);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<nint> getReordering(this ref paragraph p, slice<nint> linebreaks) => bidi_package.getReordering⁔(ref p, linebreaks);
}
// </MethodForwarders>
