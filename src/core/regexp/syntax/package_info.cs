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
[assembly: GoPositionMap("regexp/syntax/compile.go", "compile.cs", "ABgugsaigoKCgoKUgsqigpSCpoKClJQADyCigoKCgoLGgoKCxoK0opSkpIKUgoKCgpSmpKSkpKSkpKSkpIKCgqSkpKSClIKCgpSmpIKClJSmlIKCpoKCgqaCpoKCgpSClKaUgsqCppSClIKmgoKCgoKCpoKCgoKClIKUggACENKCgoKClIKUgqaCppSmgqaCgoKCpoKCgoKCgpSUgqaUtLS2")]
[assembly: GoPositionMap("regexp/syntax/op_string.go", "op_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKCAAcStoKUgqSk")]
[assembly: GoPositionMap("regexp/syntax/parse.go", "parse.cs", "ABgsggAtOIIAVLIBgoKCgpSClIKmooKUgqaCgpSCpqLcgpSCgoKUgpSClKaC3IKCuIK4ooKCuIKUxrS0gsaClILGgoKClJSmtoKCpoKClIKCgqaCuKKCgqaCgoKCpoKs0oKUgpSCgoIAARKCuIKCpqaCggACGAAJAoKCpoKCgrimgoKCgqaCgqiSgoKClIKCyJKClIKCgpTKooKCrsKCgoKClLimgoKUgoKmgoKCgoKCgpSCpgAEGAAJAoKCgpSClIKUgqaCgqbIkqaCgpSCpoKmqLaCgpSCuILKgqbIspSCgoKClIKCgpSmAAcSwoKUgoKCgoKUpoKCgoKCpgACJAAPAoKokoKCgo7igoKCgoKClKaCAAkUlJSmgoKUgoKUlIKCuIKClAAJFIKCgozSgoKmAAcQlJSmgoKCgpSUgoK4gpSmgoLugsqUgriCgoKmlIKClIK4gpSUpoKCgoKUlJSqwoKUgpSqwqaCgoKCgqaCtIKCtIK2poKCgqaqwoKUgoKClJSs0oKClIKUgrSCgqSUgpTGgoKCgoKUgpSUAAIQ0qbCgqT4tMiUgpSozIKCgoKCgpSClLaUgpSUgoK0grSClLSClJS0gpSUtIKUlLSCxoKUtLSkgoKUgrSCgoKUgoKUlJSClIK0gpSCgrSCgrSCgsamkoKCgoKUgpS0goLIgqaCgoKUgoKCgsqAgoKCgqSmgpS0poKUlJSCgpQACAwACAKClIKCgpSClIKUgoKUgoKAgoKUtoKUgoKs0gAQIoKUlIKCuIKCgpSmgoKClIK4goKCgqiSgoKCgoKCgpSUyIK0grSCtILIgpSmgsiCgpSUlJSC2AAEENKClIKCpsjSgqaClIKClIKUgpSCgpSUyqKukpSkgoKmpKSUyJLcgt6y6oLYgpTYgpSCgu7WgoKClIKClIKCgqaCgoKCppSCgqaokoKUlJSCgpSCgoKCpoKUlIKCgpSq0oKClIKCpoK0yuyClMaCgoKUgpTIgpSClMqCgoKClIKUgpSCgpSCgpSUgpS4goKUgoKUAAkSpKSkpKSkqqKCyoKmAAsaAAkCgpSCgpSs8oKmgoKUgqKCgpSmooKClKaCgoKCgpSmxsq4ABAgkoKCAAMS8oKCgoKUtIKUtIK2gpSUgpSClJSClKyyuJSkpKSUgIKkgILsgoKClMwACAKCuIKClIKCgpSSlIKmgoKClJSCgoLKgoKmgoKUgqaCgpTcgoKCgoKClKaqwoKCgpSCgoK4griCgqaCgpSmgoKClIKCyoKClIKCuICCgsiCkoKUlIKCgpSCgqaClKamgoKClIKCytaUgoK4goKClIKUpoKCpqqikoKClIKUlKiSgpSo2oKCgoKCgpSClMoABxi0lJSUlJSClJSCuIKCgoKCpqqigpSokoKUqqKCgoKClJSClKiSgoKCgpSCpoKCgoKUgqaokoKCgoKClIKUgoKUpoKCgoKUgpSCgpSmgpSqooKCgoKCgoKUlIKmlAAMFoKCgoKmgqaCgoKCxoKCgoKUlKaigoKUpoKmgoKUgpSClA==", "892-903:1;1956-1965:1", "", "724=isCharClass/1/2/2,isCharClass/2/2/2", "", "960:21,961:13")]
[assembly: GoPositionMap("regexp/syntax/perl_groups.go", "perl_groups.cs", "AAgSuNzuAAcQ3Mq4ysq4uLi47sq47tw=")]
[assembly: GoPositionMap("regexp/syntax/prog.go", "prog.cs", "AChSAA8cgoKUAA4q4oKClLS0pJS0tKSClKzWAAwWgoKCqJKCgpTIkoKUpMyypoKokoKClKqigoKCgoKUtOikgrQACA6iAAIQ0pSUuIKClIKCgrimgpTKgoKUgqa4goKCgoKClJSmrLKUpKSkpKSUpoKCgsaigriigoKCgpSClIKCuIIAHAaClLS0tLS0tLSUlIKClLS0tA==")]
[assembly: GoPositionMap("regexp/syntax/regexp.go", "regexp.cs", "ADyAAcKClIKUpoLIpqaCyILIgrYADiKigpSCAAIQ0pTcgoKClLjKgoKCgoKCyqampqaClKas0oKCgoKCgoKUgoKClIKCgoKUgqaUlJSUAAsK0oKUlIKCgpSClIKUgoKClIKmlIKUgoKmlLS0tILGgoKUgoKCpoKCgoKCgpS4goKCgoKUuLS0tLS0gpTGtLSCgoKUlIKUtIKCgpSUlLS0tIKCgoKCpqSCxoKCgpTGgoKU1sSCgoKCgoKUgvqCgoKUgqaUtLS0tLS0goKCgpSClIKC6pKCgpSCgqaokoKCpoKClII=")]
[assembly: GoPositionMap("regexp/syntax/simplify.go", "simplify.cs", "AAscAAkCgpSmgoKClIKCgpSCpqaCyoK4ppSCuIK4goKClILcggAGEJKCgoKCyoKCgoKClIKUlILKpgAEJAAUBoKmgpSCpoKC")]
// </GoSourcePositionMaps>

namespace go.regexp;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using static syntax_package;
using go.unicode;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(syntax_package.Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛError;
[GoMethodHost(typeof(syntax_package.ErrorCode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛErrorCode;
[GoMethodHost(typeof(syntax_package.Inst)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛInst;
[GoMethodHost(typeof(syntax_package.InstOp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛInstOp;
[GoMethodHost(typeof(syntax_package.Op)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛOp;
[GoMethodHost(typeof(syntax_package.Prog)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛProg;
[GoMethodHost(typeof(syntax_package.Regexp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛRegexp;
[GoMethodHost(typeof(syntax_package.compiler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛcompiler;
[GoMethodHost(typeof(syntax_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛparser;
[GoMethodHost(typeof(syntax_package.patchList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛpatchList;
[GoMethodHost(typeof(syntax_package.ranges)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class syntaxᴛranges;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class syntaxᴛpatchList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void patch(this patchList l, ж<Prog> Ꮡp, uint32 val) => syntax_package.patchᴍ(l, Ꮡp, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static patchList append(this patchList l1, ж<Prog> Ꮡp, patchList l2) => syntax_package.appendᴍ(l1, Ꮡp, l2);
}
partial class syntaxᴛcompiler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref compiler c) => syntax_package.initᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag compile(this ref compiler c, ж<Regexp> Ꮡre) => syntax_package.compileᴍ(ref c, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag inst(this ref compiler c, InstOp op) => syntax_package.instᴍ(ref c, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag nop(this ref compiler c) => syntax_package.nopᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag fail(this ref compiler c) => syntax_package.failᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag cap(this ref compiler c, uint32 arg) => syntax_package.capᴍ(ref c, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag cat(this ref compiler c, frag f1, frag f2) => syntax_package.catᴍ(ref c, f1, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag alt(this ref compiler c, frag f1, frag f2) => syntax_package.altᴍ(ref c, f1, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag quest(this ref compiler c, frag f1, bool nongreedy) => syntax_package.questᴍ(ref c, f1, nongreedy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag loop(this ref compiler c, frag f1, bool nongreedy) => syntax_package.loopᴍ(ref c, f1, nongreedy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag star(this ref compiler c, frag f1, bool nongreedy) => syntax_package.starᴍ(ref c, f1, nongreedy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag plus(this ref compiler c, frag f1, bool nongreedy) => syntax_package.plusᴍ(ref c, f1, nongreedy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag empty(this ref compiler c, EmptyOp op) => syntax_package.emptyᴍ(ref c, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static frag rune(this ref compiler c, slice<rune> r, Flags flags) => syntax_package.runeᴍ(ref c, r, flags);
}
partial class syntaxᴛOp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Op i) => syntax_package.Stringᴍ(i);
}
partial class syntaxᴛError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref Error e) => syntax_package.Errorᴍ(ref e);
}
partial class syntaxᴛErrorCode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ErrorCode e) => syntax_package.Stringᴍ(e);
}
partial class syntaxᴛparser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> newRegexp(this ref parser p, Op op) => syntax_package.newRegexpᴍ(ref p, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reuse(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.reuseᴍ(ref p, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkLimits(this ref parser p, ж<Regexp> re) => syntax_package.checkLimitsᴍ(ref p, re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkSize(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.checkSizeᴍ(ref p, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 calcSize(this ref parser p, ж<Regexp> Ꮡre, bool force) => syntax_package.calcSizeᴍ(ref p, Ꮡre, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkHeight(this ref parser p, ж<Regexp> re) => syntax_package.checkHeightᴍ(ref p, re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint calcHeight(this ref parser p, ж<Regexp> Ꮡre, bool force) => syntax_package.calcHeightᴍ(ref p, Ꮡre, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> push(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.pushᴍ(ref p, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool maybeConcat(this ref parser p, rune r, Flags flags) => syntax_package.maybeConcatᴍ(ref p, r, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void literal(this ref parser p, rune r) => syntax_package.literalᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> op(this ref parser p, Op op) => syntax_package.opᴍ(ref p, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) repeat(this ref parser p, Op op, nint min, nint max, @string before, @string after, @string lastRepeat) => syntax_package.repeatᴍ(ref p, op, min, max, before, after, lastRepeat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> concat(this ref parser p) => syntax_package.concatᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> alternate(this ref parser p) => syntax_package.alternateᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> collapse(this ref parser p, slice<ж<Regexp>> subs, Op op) => syntax_package.collapseᴍ(ref p, subs, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<Regexp>> factor(this ref parser p, slice<ж<Regexp>> sub) => syntax_package.factorᴍ(ref p, sub);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<rune>, Flags) leadingString(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.leadingStringᴍ(ref p, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> removeLeadingString(this ref parser p, ж<Regexp> Ꮡre, nint n) => syntax_package.removeLeadingStringᴍ(ref p, Ꮡre, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> leadingRegexp(this ref parser p, ж<Regexp> Ꮡre) => syntax_package.leadingRegexpᴍ(ref p, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Regexp> removeLeadingRegexp(this ref parser p, ж<Regexp> Ꮡre, bool reuse) => syntax_package.removeLeadingRegexpᴍ(ref p, Ꮡre, reuse);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint min, nint max, @string rest, bool ok) parseRepeat(this ref parser p, @string s) => syntax_package.parseRepeatᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string rest, error err) parsePerlFlags(this ref parser p, @string s) => syntax_package.parsePerlFlagsᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, @string rest, bool ok) parseInt(this ref parser p, @string s) => syntax_package.parseIntᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseVerticalBar(this ref parser p) => syntax_package.parseVerticalBarᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool swapVerticalBar(this ref parser p) => syntax_package.swapVerticalBarᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseRightParen(this ref parser p) => syntax_package.parseRightParenᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune r, @string rest, error err) parseEscape(this ref parser p, @string s) => syntax_package.parseEscapeᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune r, @string rest, error err) parseClassChar(this ref parser p, @string s, @string wholeClass) => syntax_package.parseClassCharᴍ(ref p, s, wholeClass);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<rune> @out, @string rest) parsePerlClassEscape(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parsePerlClassEscapeᴍ(Ꮡp, s, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<rune> @out, @string rest, error err) parseNamedClass(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parseNamedClassᴍ(Ꮡp, s, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<rune> appendGroup(this ж<parser> Ꮡp, slice<rune> r, charGroup g) => syntax_package.appendGroupᴍ(Ꮡp, r, g);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<rune> @out, @string rest, error err) parseUnicodeClass(this ж<parser> Ꮡp, @string s, slice<rune> r) => syntax_package.parseUnicodeClassᴍ(Ꮡp, s, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string rest, error err) parseClass(this ж<parser> Ꮡp, @string s) => syntax_package.parseClassᴍ(Ꮡp, s);
}
partial class syntaxᴛranges {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this ranges ra, nint i, nint j) => syntax_package.Lessᴍ(ra, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ranges ra) => syntax_package.Lenᴍ(ra);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this ranges ra, nint i, nint j) => syntax_package.Swapᴍ(ra, i, j);
}
partial class syntaxᴛInstOp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this InstOp i) => syntax_package.Stringᴍ(i);
}
partial class syntaxᴛProg {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Prog> Ꮡp) => syntax_package.Stringᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Inst> skipNop(this ref Prog p, uint32 pc) => syntax_package.skipNopᴍ(ref p, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string prefix, bool complete) Prefix(this ref Prog p) => syntax_package.Prefixᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static EmptyOp StartCond(this ref Prog p) => syntax_package.StartCondᴍ(ref p);
}
partial class syntaxᴛInst {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static InstOp op(this ref Inst i) => syntax_package.opᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MatchRune(this ref Inst i, rune r) => syntax_package.MatchRuneᴍ(ref i, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint MatchRunePos(this ref Inst i, rune r) => syntax_package.MatchRunePosᴍ(ref i, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MatchEmptyWidth(this ref Inst i, rune before, rune after) => syntax_package.MatchEmptyWidthᴍ(ref i, before, after);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Inst> Ꮡi) => syntax_package.Stringᴍ(Ꮡi);
}
partial class syntaxᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this ж<Regexp> Ꮡx, ж<Regexp> Ꮡy) => syntax_package.Equalᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Regexp> Ꮡre) => syntax_package.Stringᴍ(Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint MaxCap(this ref Regexp re) => syntax_package.MaxCapᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> CapNames(this ref Regexp re) => syntax_package.CapNamesᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void capNames(this ref Regexp re, slice<@string> names) => syntax_package.capNamesᴍ(ref re, names);
}
partial class syntaxᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Regexp> Simplify(this ж<Regexp> Ꮡre) => syntax_package.Simplifyᴍ(Ꮡre);
}
// </GlyphForwarders>
