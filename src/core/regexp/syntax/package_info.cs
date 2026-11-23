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
using static go.regexp.syntax_package;
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
[assembly: GoDynamicTypeLift("7374727563747b6f6e63652073796e632e4f6e63653b206d206d61705b737472696e675d737472696e677d", "categoryAliasesᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<ranges, sort_package.Interface>]
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
[assembly: GoFieldAccess(typeof(Inst))]
[assembly: GoFieldAccess(typeof(Prog))]
[assembly: GoFieldAccess(typeof(Regexp))]
[assembly: GoFieldAccess(typeof(parser))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("regexp/syntax/compile.go", "compile.cs", "ABcugqaigoKCgoKUgsqigpSCpoKClJQADSCigoKCgoKmgoKCpoKUopSkpIKUgoKCgpSmpKSkpKSkpKSkpIKCgqSkpKSClIKCgpSmpIKClJSmlIKCpoKCgqaCpoKCgpSClKaUgsqCppSClIKmgoKCgoKCpoKCgoKClIKUggACENKCgoKClIKUgqaCppSmgqaCgoKCpoKCgoKCgpSUgqaUtLS2")]
[assembly: GoPositionMap("regexp/syntax/op_string.go", "op_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCggAHEpaClIKkpA==")]
[assembly: GoPositionMap("regexp/syntax/parse.go", "parse.cs", "ABUsggApOIIAULIBgoKCgpSClIKmooKUgqaCgpSCpqLcgpSCgoKUgpSClKaC3IKCuIK4ooKCuIKUxrS0gsaClILGgoKClJSmtoKCpoKClIKCgqaCuKKCgqaCgoKCpoKs0oKUgpSCgoIAARKCuIKCpqaCggACGAAJAoKCpoKCgrimgoKCgqaCgqiSgoKClIKCqJKClIKCgpSqooKCrsKCgoKClLimgoKUgoKmgoKCgoKCgpSCpgACGAAJAoKCgpSClIKUgqaCgqaokqaCgpSCpoKmqLaCgpSCuILKgqaospSCgoKClIKCgpSmAAUSwoKUgoKCgoKUpoKCgoKCpgACJAAPAoKokoKCgo7igoKCgoKClKaCAAkUlJSmgoKUgoKUlIKCuIKClAAJFIKCgozSgoKmAAcQlJSmgoKCgpSUgoK4gpSmgoLugsqUgriCgoKmlIKClIK4gpSUpoKCgoKUlJSqwoKUgpSqwqaCgoKCgqaCtIKCtIK2poKCgqaqwoKUgoKClJSs0oKClIKUgrSCgqSUgpSmgoKCgoKUgpSUAAIQ0qbCgqT4tMiUgpSozIKCgoKCgpSClLaUgpSUgoK0grSClLSClJS0gpSUtIKUlLSCxoKUtLSkgoKUgrSCgoKUgoKUlJSClIK0gpSCgrSCgrSCgsamkoKCgoKUgpS0goLIgqaCgoKUgoKCgsqAgoKCgqSmgpS0poKUlJSCgpTsAAgCgpSCgoKUgpSClIKClIKCgIKClLaClIKCrNIAECKClJSCgriCgoKUpoKCgpSCuIKCgoKokoKCgoKCgoKUlMiCtIK0grSCyIKUpoLIgoKUlJSUgtgAAhDSgpSCgqao0oKmgpSCgpSClIKUgoKUlKqirpKUpIKCpqSklKiS3IK+suqC2IKU2IKUgoLO1oKCgpSCgpSCgoKmgoKCgqaUgoKmqJKClJSUgoKUgoKCgqaClJSCgoKUqtKCgpSCgqaCtMrsgpTGgoKClIKUyIKUgpTKgoKCgpSClIKUgoKUgoKUlIKUuIKClIKClAAJEqSkpKSkpKqigsqCpgAHGgAJAoKUgoKUrPKCpoKClIKigoKUpqKCgpSmgoKCgoKUpqbKuAAQIJKCggADEvKCgoKClLSClLSCtoKUlIKUgpSUgpSssriUpKSklICCpICC7IKCgpSsAAgCgriCgpSCgoKUkpSCpoKCgpSUgoKCyoKCpoKClIKmgoKU3IKCgoKCgpSmqsKCgoKUgoKCuIK4goKmgoKUpoKCgpSCgsqCgpSCgriAgoLIgpKClJSCgoKUgoKmgpSmpoKCgpSCgqrWlIKCuIKCgpSClKaCgqaqopKCgpSClJSokoKUqNqCgoKCgoKUgpTKAAcYtJSUlJSUgpSUgriCgoKCgqaqooKUqJKClKqigoKCgpSUgpSokoKCgoKUgqaCgoKClIKmqJKCgoKCgpSClIKClKaCgoKClIKUgoKUpoKUqqKCgoKCgoKClJSCppQAChaCgoKCpoKmgoKCgqaCgoKClJSmooKClKaCpoKClIKUgpQ=", "892-903:1;1956-1965:1", "", "701=isCharClass/1/2/2,isCharClass/2/2/2", "", "935:45,936:13")]
[assembly: GoPositionMap("regexp/syntax/perl_groups.go", "perl_groups.cs", "AAgSuNzuAAcQ3Mq4ysq4uLi47sq47tw=")]
[assembly: GoPositionMap("regexp/syntax/prog.go", "prog.cs", "ACdSAA0cgoKUAAwq4oKClLS0pJS0tKSClKzWAAoWgoKCqJKCgpSokoKUpKyypoKokoKClKqigoKCgoKUtOikgrTOogACENKUlLiCgpSCgoK4poKUyoKClIKmuIKCgoKCgpSUpqyylKSkpKSklKaCgoKmooK4ooKCgoKUgpSCgriCABwGgpS0tLS0tLS0lJSCgpS0tLQ=")]
[assembly: GoPositionMap("regexp/syntax/regexp.go", "regexp.cs", "ADmAAcKClIKUpoLIpqaCyILIgrYADCKigpSCAAIQ0pTcgoKClLjKgoKCgoKCyqampqaClKas0oKCgoKCgoKUgoKClIKCgoKUgqaUlJSUAAsK0oKUlIKCgpSClIKUgoKClIKmlIKUgoKmlLS0tILGgoKUgoKCpoKCgoKCgpS4goKCgoKUuLS0tLS0gpTGtLSCgoKUlIKUtIKCgpSUlLS0tIKCgoKCpqSCxoKCgpTGgoKU1qSCgoKCgoKUgtqCgoKUgqaUtLS0tLS0goKCgpSClIKCypKCgpSCgqaokoKCpoKClII=")]
[assembly: GoPositionMap("regexp/syntax/simplify.go", "simplify.cs", "AAccAAkCgpSmgoKClIKCgpSCpqaCyoK4ppSCuIK4goKClILcggAGEJKCgoKCyoKCgoKClIKUlILKpgACJAAUBoKmgpSCpoKC")]
// </GoSourcePositionMaps>

namespace go.regexp;

using static go.regexp.syntax_package;

[GoPackage("syntax")]
public static partial class syntax_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct categoryAliasesᴛ1 {}
    internal partial struct charGroup {}
    internal partial struct compiler {}
    internal partial struct frag {}
    internal partial struct parser {}
    internal partial struct patchList {}
    internal partial struct printFlags {}
    internal partial struct ranges {}
    public partial struct EmptyOp {}
    public partial struct Error {}
    public partial struct ErrorCode {}
    public partial struct Flags {}
    public partial struct Inst {}
    public partial struct InstOp {}
    public partial struct Op {}
    public partial struct Prog {}
    [GoValueClone("Sub0", "Rune0")] public partial struct Regexp {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(syntax_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛError;
[GoMethodHost(typeof(syntax_package.ErrorCode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛErrorCode;
[GoMethodHost(typeof(syntax_package.Inst)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛInst;
[GoMethodHost(typeof(syntax_package.InstOp)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛInstOp;
[GoMethodHost(typeof(syntax_package.Op)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛOp;
[GoMethodHost(typeof(syntax_package.Prog)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛProg;
[GoMethodHost(typeof(syntax_package.Regexp)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛRegexp;
[GoMethodHost(typeof(syntax_package.compiler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛcompiler;
[GoMethodHost(typeof(syntax_package.parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛparser;
[GoMethodHost(typeof(syntax_package.patchList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛpatchList;
[GoMethodHost(typeof(syntax_package.ranges)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class syntaxᴛranges;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class syntaxᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref Error e) => syntax_package.Error⁔(ref e);
}
partial class syntaxᴛErrorCode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ErrorCode e) => syntax_package.String⁔(e);
}
partial class syntaxᴛInst {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static InstOp op(this ref Inst i) => syntax_package.op⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MatchRune(this ref Inst i, rune r) => syntax_package.MatchRune⁔(ref i, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint MatchRunePos(this ref Inst i, rune r) => syntax_package.MatchRunePos⁔(ref i, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MatchEmptyWidth(this ref Inst i, rune before, rune after) => syntax_package.MatchEmptyWidth⁔(ref i, before, after);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Inst> Ꮡi) => syntax_package.String⁔(Ꮡi);
}
partial class syntaxᴛInstOp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this InstOp i) => syntax_package.String⁔(i);
}
partial class syntaxᴛOp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Op i) => syntax_package.String⁔(i);
}
partial class syntaxᴛProg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Prog> Ꮡp) => syntax_package.String⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Inst> skipNop(this ref Prog p, uint32 pc) => syntax_package.skipNop⁔(ref p, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string prefix, bool complete) Prefix(this ref Prog p) => syntax_package.Prefix⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static EmptyOp StartCond(this ref Prog p) => syntax_package.StartCond⁔(ref p);
}
partial class syntaxᴛRegexp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this ж<Regexp> Ꮡx, ж<Regexp> Ꮡy) => syntax_package.Equal⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Regexp> Ꮡre) => syntax_package.String⁔(Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint MaxCap(this ref Regexp re) => syntax_package.MaxCap⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> CapNames(this ref Regexp re) => syntax_package.CapNames⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void capNames(this ref Regexp re, slice<@string> names) => syntax_package.capNames⁔(ref re, names);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Regexp> Simplify(this ж<Regexp> Ꮡre) => syntax_package.Simplify⁔(Ꮡre);
}
partial class syntaxᴛcompiler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref compiler c) => syntax_package.init⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag compile(this ref compiler c, ж<Regexp> Ꮡre) => syntax_package.compile⁔(ref c, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag inst(this ref compiler c, InstOp op) => syntax_package.inst⁔(ref c, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag nop(this ref compiler c) => syntax_package.nop⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag fail(this ref compiler c) => syntax_package.fail⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag cap(this ref compiler c, uint32 arg) => syntax_package.cap⁔(ref c, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag cat(this ref compiler c, frag f1, frag f2) => syntax_package.cat⁔(ref c, f1, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag alt(this ref compiler c, frag f1, frag f2) => syntax_package.alt⁔(ref c, f1, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag quest(this ref compiler c, frag f1, bool nongreedy) => syntax_package.quest⁔(ref c, f1, nongreedy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag loop(this ref compiler c, frag f1, bool nongreedy) => syntax_package.loop⁔(ref c, f1, nongreedy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag star(this ref compiler c, frag f1, bool nongreedy) => syntax_package.star⁔(ref c, f1, nongreedy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag plus(this ref compiler c, frag f1, bool nongreedy) => syntax_package.plus⁔(ref c, f1, nongreedy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag empty(this ref compiler c, EmptyOp op) => syntax_package.empty⁔(ref c, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static frag rune(this ref compiler c, slice<rune> r, Flags flags) => syntax_package.rune⁔(ref c, r, flags);
}
partial class syntaxᴛparser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> newRegexp(this ref parser p, Op op) => syntax_package.newRegexp⁔(ref p, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reuse(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.reuse⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkLimits(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.checkLimits⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkSize(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.checkSize⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 calcSize(this ref parser p, ж<Regexp> Ꮡre, bool force) => syntax_package.calcSize⁔(ref p, Ꮡre, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkHeight(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.checkHeight⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint calcHeight(this ref parser p, ж<Regexp> Ꮡre, bool force) => syntax_package.calcHeight⁔(ref p, Ꮡre, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> push(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.push⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool maybeConcat(this ref parser p, rune r, Flags flags) => syntax_package.maybeConcat⁔(ref p, r, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void literal(this ref parser p, rune r) => syntax_package.literal⁔(ref p, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> op(this ref parser p, Op op) => syntax_package.op⁔(ref p, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) repeat(this ref parser p, Op op, nint min, nint max, @string before, @string after, @string lastRepeat) => syntax_package.repeat⁔(ref p, op, min, max, before, after, lastRepeat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> concat(this ref parser p) => syntax_package.concat⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> alternate(this ref parser p) => syntax_package.alternate⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> collapse(this ref parser p, slice<ж<Regexp>> subs, Op op) => syntax_package.collapse⁔(ref p, subs, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<Regexp>> factor(this ref parser p, slice<ж<Regexp>> sub) => syntax_package.factor⁔(ref p, sub);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<rune>, Flags) leadingString(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.leadingString⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> removeLeadingString(this ref parser p, ж<Regexp> Ꮡre, nint n) => syntax_package.removeLeadingString⁔(ref p, Ꮡre, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> leadingRegexp(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.leadingRegexp⁔(ref p, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Regexp> removeLeadingRegexp(this ref parser p, ж<Regexp> Ꮡre, bool reuse) => syntax_package.removeLeadingRegexp⁔(ref p, Ꮡre, reuse);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint min, nint max, @string rest, bool ok) parseRepeat(this ref parser p, @string s) => syntax_package.parseRepeat⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string rest, error err) parsePerlFlags(this ref parser p, @string s) => syntax_package.parsePerlFlags⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, @string rest, bool ok) parseInt(this ref parser p, @string s) => syntax_package.parseInt⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseVerticalBar(this ref parser p) => syntax_package.parseVerticalBar⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool swapVerticalBar(this ref parser p) => syntax_package.swapVerticalBar⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseRightParen(this ref parser p) => syntax_package.parseRightParen⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune r, @string rest, error err) parseEscape(this ref parser p, @string s) => syntax_package.parseEscape⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune r, @string rest, error err) parseClassChar(this ref parser p, @string s, @string wholeClass) => syntax_package.parseClassChar⁔(ref p, s, wholeClass);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<rune> @out, @string rest) parsePerlClassEscape(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parsePerlClassEscape⁔(Ꮡp, s, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<rune> @out, @string rest, error err) parseNamedClass(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parseNamedClass⁔(Ꮡp, s, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<rune> appendGroup(this ж<parser> Ꮡp, slice<rune> r, charGroup g) => syntax_package.appendGroup⁔(Ꮡp, r, g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<rune> @out, @string rest, error err) parseUnicodeClass(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parseUnicodeClass⁔(Ꮡp, s, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string rest, error err) parseClass(this ж<parser> Ꮡp, @string s) => syntax_package.parseClass⁔(Ꮡp, s);
}
partial class syntaxᴛpatchList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void patch(this patchList l, ж<Prog> Ꮡp, uint32 val) => syntax_package.patch⁔(l, Ꮡp, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static patchList append(this patchList l1, ж<Prog> Ꮡp, patchList l2) => syntax_package.append⁔(l1, Ꮡp, l2);
}
partial class syntaxᴛranges {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this ranges ra, nint i, nint j) => syntax_package.Less⁔(ra, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ranges ra) => syntax_package.Len⁔(ra);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this ranges ra, nint i, nint j) => syntax_package.Swap⁔(ra, i, j);
}
// </MethodForwarders>
