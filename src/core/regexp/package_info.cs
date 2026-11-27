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
using syntax = go.regexp.syntax_package;
// </ImportedTypeAliases>

using go;
using static go.regexp_package;
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
[assembly: GoImplement<inputBytes, input>(Pointer = true)]
[assembly: GoImplement<inputReader, input>(Pointer = true)]
[assembly: GoImplement<inputString, input>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Regexp))]
[assembly: GoFieldAccess(typeof(bitState))]
[assembly: GoFieldAccess(typeof(entry))]
[assembly: GoFieldAccess(typeof(go.regexp.syntax_package.Inst))]
[assembly: GoFieldAccess(typeof(go.regexp.syntax_package.Prog))]
[assembly: GoFieldAccess(typeof(inputs))]
[assembly: GoFieldAccess(typeof(machine))]
[assembly: GoFieldAccess(typeof(onePassInst))]
[assembly: GoFieldAccess(typeof(onePassMachine))]
[assembly: GoFieldAccess(typeof(onePassProg))]
[assembly: GoFieldAccess(typeof(queue))]
[assembly: GoFieldAccess(typeof(thread))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("regexp/backtrack.go", "backtrack.cs", "AC5igoKClKaigqqigpSqoqzSlIKUpoKClIKmgpSUgqaClJSCvKKCgpSCquaCusKUgoKUgoKCAAgSgoKClISUlLQACxSUgoKUgoLapoKCgraCgraCgpSCgraCgpSCgraCgpSCgraCgpSCgraUgpSUgpSCyIKClIK2gtqC3IKUgriCuIK4yKiygoKUlKaCgqaCgpSCggAIFIKClIKCgpSmgpSUlJSCpoKCgg==")]
[assembly: GoPositionMap("regexp/exec.go", "exec.cs", "ADZwooKmooKmooKCgqamgoKClLiCgpSClKaCgpSqooKCgpSClIIACRSCpoKClIKCgpSUgoKUlIKUgoKClJSCgpSUgpSClJSs0oKClIKClIKCkoKClIKClJSCgpSUlJSUgoKUgoKmgoKUlIKCgpSmlIKCgpSUgqiygoKmAAIQ8oKCgoKClIKClIKClLaCgpSUgoKmlLa0tLSkgpSCpq7ygoKUgqaCgoKCgpSClPiCgrSCgsaCtIKCgoKUgsaClJSClIKkAAoUgoKClKaigqiygoKmgoKUpoKCppSCkoKClIKClJSClJaClIKCgoKUgoKClLSCgoKUtILGggAICoLYgrS0tIKUtIKUpIKUgoKCgriCgoKmgoKokq7ilKaCpoKUgqaClIKCgqaCgg==", "", "", "475=canCheckPrefix/1/1/1")]
[assembly: GoPositionMap("regexp/onepass.go", "onepass.cs", "ACRSAAkCgoKUgoKCgqaCqJKCgpSGlK7igoKUgpSmgoKUpAAJFIKmooKCpoKCpoKClKaCgriCgpSCgqaCAAwcgpbSgoKClJaCgoKCgoK4goKClIKCgoKmgpS0tLSkgqbYkoLGtILcksqCAAcQgpTGgpSCgoKCgqaUlLiCgoKCgoKClILKgsimgp4ACASCpgAGEsKCgoKUgpSUgoKCgqaCgpSCgriUgoLGgpSCgoLGgoKCgoLGtIKClIKCgoKUgoKCgoKUlJSCgoKUtIKClIKUgoKCgpSUlIKCgpS0goKUgoK0goKUgoKCgramgoKCgoKCgoKmgoKmruKCpoSUgoKCgsqCgpSCxoLGgoKU7KaUgpQ=", "170-175:1;178-187:2;308-434:1", "", "", "", "180:53,181:13")]
[assembly: GoPositionMap("regexp/regexp.go", "regexp.cs", "AGbOAZIAAhYACAKCAAIaAAoCAAIsABMCAAIS4qaCgoKUgpSCgoKUgoKUAAoWgoKUlKaCpoKCgpSUAAgUvtKCgpSCgoKCgtyCgpSCgpSosoKCgqiSlKSkgoKClKakpLSCgpSkgoKCgoKmvrKCgpSssoKClKaCgpSokgACENIAAhTygoKCuAATKIKClKaCpqKmoqaClIKmgpQABxCCgpSmgqaipqKmgpSCpoKUAAkUgoKWgoKClIKmgqaCpoKmgqyyqqKqoqqirNKCgpSs0oKClKzSgoKUrNKCgpSGrLIAAhLChqaigoKCgoKUlIKmgoKCgriClO6ClJaSgpSUgoKmlMqClKas0oKClIKigpSUrLIAAxLCAAYSkqaCgr60koKCuIKmgoKCgoKClIKUAAIQ0pSUgoKUrNKCgpSmgoKCpoKUppSCgoKUgpSClKaUlIKCzqKCgoKUrsKCgpQAAhDSgoKClK7CgoKUAAIQ0oKClAACEPKCgoKUgoKCpgACKAARAqyypoKCgoKUgoKUgoKUgpSClIKCgoKUuIKCgpSUyoKsAAgCgpSCgoKUgoKCgpSUlJSCgpSUuIKCgoKUpoKmgoIAAhDyAAIQ8oKCgpSCgoKmAAIQ8gACEPIABBLCgpSCkoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCkoKUgoKCppSuwoKUgoKClJSuwoKUgoKClIKCgqaUAAIQ0oKUgoKClJQAAiYAEgSCpoKmgpSCgoKCpoKClKaCpgACEuKuwqqigoKUgg==", "557-559:1;567-569:1;577-579:1;657-662:1;670-672:1;680-682:1;1062-1067:1;1080-1085:1;1098-1103:1;1116-1121:1;1134-1145:1;1158-1163:1;1176-1187:1;1201-1206:1", "", "185=compileOnePass/1/1/3,StartCond/1/1/6,minInputLen/1/1/9")]
// </GoSourcePositionMaps>

namespace go;

using static go.regexp_package;
using io = go.io_package;
using syntax = go.regexp.syntax_package;

[GoPackage("regexp")]
public static partial class regexp_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface input {}
    internal partial struct bitState {}
    internal partial struct entry {}
    internal partial struct inputBytes {}
    internal partial struct inputReader {}
    internal partial struct inputString {}
    internal partial struct inputs {}
    internal partial struct job {}
    internal partial struct lazyFlag {}
    internal partial struct machine {}
    internal partial struct onePassMachine {}
    internal partial struct onePassProg {}
    internal partial struct queue {}
    internal partial struct queueOnePass {}
    internal partial struct thread {}
    public partial struct Regexp {}
    public partial struct onePassInst {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸregexpꓸsyntax() => builtin.initPackage(typeof(regexp.syntax_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
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
[GoMethodHost(typeof(regexp_package.Regexp)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛRegexp;
[GoMethodHost(typeof(regexp_package.bitState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛbitState;
[GoMethodHost(typeof(regexp_package.inputBytes)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛinputBytes;
[GoMethodHost(typeof(regexp_package.inputReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛinputReader;
[GoMethodHost(typeof(regexp_package.inputString)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛinputString;
[GoMethodHost(typeof(regexp_package.inputs)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛinputs;
[GoMethodHost(typeof(regexp_package.lazyFlag)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛlazyFlag;
[GoMethodHost(typeof(regexp_package.machine)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛmachine;
[GoMethodHost(typeof(regexp_package.queueOnePass)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class regexpᴛqueueOnePass;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class regexpᴛRegexp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryBacktrack(this ж<Regexp> Ꮡre, ж<bitState> Ꮡb, input i, uint32 pc, nint pos) => regexp_package.tryBacktrack⁔(Ꮡre, Ꮡb, i, pc, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<nint> backtrack(this ж<Regexp> Ꮡre, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.backtrack⁔(Ꮡre, ib, @is, pos, ncap, dstCap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<nint> doOnePass(this ж<Regexp> Ꮡre, io.RuneReader ir, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.doOnePass⁔(Ꮡre, ir, ib, @is, pos, ncap, dstCap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool doMatch(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s) => regexp_package.doMatch⁔(Ꮡre, r, b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<nint> doExecute(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.doExecute⁔(Ꮡre, r, b, s, pos, ncap, dstCap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Regexp re) => regexp_package.String⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Regexp> Copy(this ref Regexp re) => regexp_package.Copy⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Longest(this ref Regexp re) => regexp_package.Longest⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<machine> get(this ж<Regexp> Ꮡre) => regexp_package.get⁔(Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void put(this ref Regexp re, ж<machine> Ꮡm) => regexp_package.put⁔(ref re, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumSubexp(this ref Regexp re) => regexp_package.NumSubexp⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> SubexpNames(this ref Regexp re) => regexp_package.SubexpNames⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint SubexpIndex(this ref Regexp re, @string name) => regexp_package.SubexpIndex⁔(ref re, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string prefix, bool complete) LiteralPrefix(this ref Regexp re) => regexp_package.LiteralPrefix⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MatchReader(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.MatchReader⁔(Ꮡre, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool MatchString(this ж<Regexp> Ꮡre, @string s) => regexp_package.MatchString⁔(Ꮡre, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Match(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.Match⁔(Ꮡre, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ReplaceAllString(this ж<Regexp> Ꮡre, @string src, @string repl) => regexp_package.ReplaceAllString⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ReplaceAllLiteralString(this ж<Regexp> Ꮡre, @string src, @string repl) => regexp_package.ReplaceAllLiteralString⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ReplaceAllStringFunc(this ж<Regexp> Ꮡre, @string src, Func<@string, @string> repl) => regexp_package.ReplaceAllStringFunc⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> replaceAll(this ж<Regexp> Ꮡre, slice<byte> bsrc, @string src, nint nmatch, Func<slice<byte>, slice<nint>, slice<byte>> repl) => regexp_package.replaceAll⁔(Ꮡre, bsrc, src, nmatch, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> ReplaceAll(this ж<Regexp> Ꮡre, slice<byte> src, slice<byte> repl) => regexp_package.ReplaceAll⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> ReplaceAllLiteral(this ж<Regexp> Ꮡre, slice<byte> src, slice<byte> repl) => regexp_package.ReplaceAllLiteral⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> ReplaceAllFunc(this ж<Regexp> Ꮡre, slice<byte> src, Func<slice<byte>, slice<byte>> repl) => regexp_package.ReplaceAllFunc⁔(Ꮡre, src, repl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<nint> pad(this ref Regexp re, slice<nint> a) => regexp_package.pad⁔(ref re, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allMatches(this ж<Regexp> Ꮡre, @string s, slice<byte> b, nint n, Action<slice<nint>> deliver) => regexp_package.allMatches⁔(Ꮡre, s, b, n, deliver);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Find(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.Find⁔(Ꮡre, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> /*loc*/ FindIndex(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindIndex⁔(Ꮡre, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FindString(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindString⁔(Ꮡre, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> /*loc*/ FindStringIndex(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringIndex⁔(Ꮡre, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> /*loc*/ FindReaderIndex(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.FindReaderIndex⁔(Ꮡre, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<byte>> FindSubmatch(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindSubmatch⁔(Ꮡre, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Expand(this ref Regexp re, slice<byte> dst, slice<byte> template, slice<byte> src, slice<nint> match) => regexp_package.Expand⁔(ref re, dst, template, src, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> ExpandString(this ref Regexp re, slice<byte> dst, @string template, @string src, slice<nint> match) => regexp_package.ExpandString⁔(ref re, dst, template, src, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> expand(this ref Regexp re, slice<byte> dst, @string template, slice<byte> bsrc, @string src, slice<nint> match) => regexp_package.expand⁔(ref re, dst, template, bsrc, src, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> FindSubmatchIndex(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindSubmatchIndex⁔(Ꮡre, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> FindStringSubmatch(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringSubmatch⁔(Ꮡre, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> FindStringSubmatchIndex(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringSubmatchIndex⁔(Ꮡre, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> FindReaderSubmatchIndex(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.FindReaderSubmatchIndex⁔(Ꮡre, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<byte>> FindAll(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAll⁔(Ꮡre, b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<nint>> FindAllIndex(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllIndex⁔(Ꮡre, b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> FindAllString(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllString⁔(Ꮡre, s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<nint>> FindAllStringIndex(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringIndex⁔(Ꮡre, s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<slice<byte>>> FindAllSubmatch(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllSubmatch⁔(Ꮡre, b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<nint>> FindAllSubmatchIndex(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllSubmatchIndex⁔(Ꮡre, b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<@string>> FindAllStringSubmatch(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringSubmatch⁔(Ꮡre, s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<nint>> FindAllStringSubmatchIndex(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringSubmatchIndex⁔(Ꮡre, s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Split(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.Split⁔(Ꮡre, s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ref Regexp re, slice<byte> b) => regexp_package.AppendText⁔(ref re, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this ref Regexp re) => regexp_package.MarshalText⁔(ref re);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref Regexp re, slice<byte> text) => regexp_package.UnmarshalText⁔(ref re, text);
}
partial class regexpᴛbitState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref bitState b, ж<syntax.Prog> Ꮡprog, nint end, nint ncap) => regexp_package.reset⁔(ref b, Ꮡprog, end, ncap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldVisit(this ref bitState b, uint32 pc, nint pos) => regexp_package.shouldVisit⁔(ref b, pc, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref bitState b, ж<Regexp> Ꮡre, uint32 pc, nint pos, bool arg) => regexp_package.push⁔(ref b, Ꮡre, pc, pos, arg);
}
partial class regexpᴛinputBytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune, nint) step(this ref inputBytes i, nint pos) => regexp_package.step⁔(ref i, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputBytes i) => regexp_package.canCheckPrefix⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasPrefix(this ref inputBytes i, ж<Regexp> Ꮡre) => regexp_package.hasPrefix⁔(ref i, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint index(this ref inputBytes i, ж<Regexp> Ꮡre, nint pos) => regexp_package.index⁔(ref i, Ꮡre, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static lazyFlag context(this ref inputBytes i, nint pos) => regexp_package.context⁔(ref i, pos);
}
partial class regexpᴛinputReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune, nint) step(this ref inputReader i, nint pos) => regexp_package.step⁔(ref i, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputReader i) => regexp_package.canCheckPrefix⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasPrefix(this ref inputReader i, ж<Regexp> Ꮡre) => regexp_package.hasPrefix⁔(ref i, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint index(this ref inputReader i, ж<Regexp> Ꮡre, nint pos) => regexp_package.index⁔(ref i, Ꮡre, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static lazyFlag context(this ref inputReader i, nint pos) => regexp_package.context⁔(ref i, pos);
}
partial class regexpᴛinputString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune, nint) step(this ref inputString i, nint pos) => regexp_package.step⁔(ref i, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputString i) => regexp_package.canCheckPrefix⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasPrefix(this ref inputString i, ж<Regexp> Ꮡre) => regexp_package.hasPrefix⁔(ref i, Ꮡre);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint index(this ref inputString i, ж<Regexp> Ꮡre, nint pos) => regexp_package.index⁔(ref i, Ꮡre, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static lazyFlag context(this ref inputString i, nint pos) => regexp_package.context⁔(ref i, pos);
}
partial class regexpᴛinputs {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static input newBytes(this ж<inputs> Ꮡi, slice<byte> b) => regexp_package.newBytes⁔(Ꮡi, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static input newString(this ж<inputs> Ꮡi, @string s) => regexp_package.newString⁔(Ꮡi, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static input newReader(this ж<inputs> Ꮡi, io.RuneReader r) => regexp_package.newReader⁔(Ꮡi, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref inputs i) => regexp_package.clear⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (input, nint) init(this ж<inputs> Ꮡi, io.RuneReader r, slice<byte> b, @string s) => regexp_package.init⁔(Ꮡi, r, b, s);
}
partial class regexpᴛlazyFlag {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this lazyFlag f, syntax.EmptyOp op) => regexp_package.match⁔(f, op);
}
partial class regexpᴛmachine {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref machine m, nint ncap) => regexp_package.init⁔(ref m, ncap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<thread> alloc(this ref machine m, ж<syntax.Inst> Ꮡi) => regexp_package.alloc⁔(ref m, Ꮡi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this ж<machine> Ꮡm, input i, nint pos) => regexp_package.match⁔(Ꮡm, i, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref machine m, ж<queue> Ꮡq) => regexp_package.clear⁔(ref m, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void step(this ref machine m, ж<queue> Ꮡrunq, ж<queue> Ꮡnextq, nint pos, nint nextPos, rune c, ж<lazyFlag> ᏑnextCond) => regexp_package.step⁔(ref m, Ꮡrunq, Ꮡnextq, pos, nextPos, c, ᏑnextCond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<thread> add(this ref machine m, ж<queue> Ꮡq, uint32 pc, nint pos, slice<nint> cap, ж<lazyFlag> Ꮡcond, ж<thread> Ꮡt) => regexp_package.add⁔(ref m, Ꮡq, pc, pos, cap, Ꮡcond, Ꮡt);
}
partial class regexpᴛqueueOnePass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref queueOnePass q) => regexp_package.empty⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 /*n*/ next(this ref queueOnePass q) => regexp_package.next⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref queueOnePass q) => regexp_package.clear⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool contains(this ref queueOnePass q, uint32 u) => regexp_package.contains⁔(ref q, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insert(this ref queueOnePass q, uint32 u) => regexp_package.insert⁔(ref q, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertNew(this ref queueOnePass q, uint32 u) => regexp_package.insertNew⁔(ref q, u);
}
// </MethodForwarders>
