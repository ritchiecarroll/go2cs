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
using static go.regexp_package;

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
[assembly: GoPositionMap("regexp/backtrack.go", "backtrack.cs", "AC9igoKClKaigqqigpSqoszSlIKUpoKClIKmgpSUgqaClJSCvKKCgpSCquaC2sKUgoKUgoKCAAgSgoKClISUlLQACxSUgoKUgoLapoKCgraCgraCgpSCgraCgpSCgraCgpSCgraCgpSCgraUgpSUgpSCyIKClIK2gtqC3IKUgriCuIK4yKiygoKUlKaCgqaCgpSCggAIFIKClIKCgpSmgpSUlJSCpoKCgg==")]
[assembly: GoPositionMap("regexp/exec.go", "exec.cs", "ADlwooKmooKmooKCgqamgoKClLiCgpSClMaCgpSqooKCgpSClIIACxSCxoKClIKCgpSUgoKUlIKUgoKClJSCgpSUgpSClJTM0oKClIKClIKCkoKClIKClJSCgpSUlJSUgoKUgoKmgoKUlIKCgpSmlIKCgpSUgqiygoKmAAIQ8oKCgoKClIKClIKClLaCgpSUgoKmlLa0tLSkgpSCpq7ygoKUgqaCgoKCgpSClPiCgrSCgsaCtIKCgoKUgsaClJSClIKkAAwUgoKClKaigsiygoKmgoKUpoKCppSCkoKClIKClJSClJaClIKCgoKUgoKClLSCgoKUtILGggAICoLYgrS0tIKUtIKUpIKUgoKCgriCgoKmgoKokq7ilKaCpoKUgqaClIKCgqaCgg==", "", "", "490=canCheckPrefix/1/1/1")]
[assembly: GoPositionMap("regexp/onepass.go", "onepass.cs", "ACVSAAkCgoKUgoKCgqaCqJKCgpSGlK7igoKUgpSmgoKUpAALFIKmooKCpoKCpoKClKaCgriCgpSCgsaCAAwcgpbSgoKClJaCgoKCgoK4goKClIKCgoKmgpS0tLSkgqbYkoLGtILcksqCAAcQgpTGgpSCgoKCgqaUlLiCgoKCgoKClILKgsimgp4ACASCpgAGEsKCgoKUgpSUgoKCgqaCgpSCgriUgoLGgpSCgoLGgoKCgoLGtIKClIKCgoKUgoKCgoKUlJSCgoKUtIKClIKUgoKCgpSUlIKCgpS0goKUgoK0goKUgoKCgramgoKCgoKCgoKmgoKmruKCpoSUgoKCgsqCgpSCxoLGgoKU7KaUgpQ=", "170-175:1;178-187:2;308-434:1", "", "", "", "185:21,186:13")]
[assembly: GoPositionMap("regexp/regexp.go", "regexp.cs", "AGnOAZIAAhYACAKCAAQaAAoCAAIsABMCAAQS4saCgoKUgpSCgoKUgoKUAAoWgoKUlKaCpoKCgpSUAAgU3tKCgpSCgoKCgtyCgpSCgpSosoKCgsiSlKSkgoKClKakpKSCgpSkgoKCgoKmvrKCgpSssoKClKaCgpTIkgACENIAAhTygoKCuAAXKIKClKaCpqKmoqaClIKmgpQACxCCgpSmgqaipqKmgpSCpoKUAA0UgoKWgoKClIKmgqaCpoKmgsyyqqKqoqqizNKCgpSs0oKClKzSgoKUzNKCgpSGrLIAAhLChqaigoKCgoKUlIKmgoKCgriClO6ClJaSgpSUgoKmlMqClKas0oKClIKigpSUrLIAAxLCAAgSkqaCgr60koKCuIKmgoKCgoKClIKUAAQQ0pSUgoKUrNKCgpSmgoKCpoKUppSCgoKUgpSClKaUlIKCzqKCgoKUrsKCgpQAAhDSgoKClK7CgoKUAAIQ0oKClAACEPKCgoKUgoKCpgACKAARAqyypoKCgoKUgoKUgoKUgpSClIKCgoKUuIKCgpSUyoLMAAgCgpSCgoKUgoKCgpSUlJSCgpSUuIKCgoKUpoKmgoIABBDyAAIQ8oKCgpSCgoKmAAIQ8gACEPIACBLCgpSCkoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCgoKUlK7CgpSCkoKUgoKCppSuwoKUgoKClJSuwoKUgoKClIKCgqaUAAIQ0oKUgoKClJQAAiYAEgSCpoKmgpSCgoKCpoKClKaCpgACEuKuwqqigoKUgg==", "557-559:1;567-569:1;577-579:1;657-662:1;670-672:1;680-682:1;1062-1067:1;1080-1085:1;1098-1103:1;1116-1121:1;1134-1145:1;1158-1163:1;1176-1187:1;1201-1206:1", "", "194=compileOnePass/1/1/3,StartCond/1/1/6,minInputLen/1/1/9")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using syntax = regexp.syntax_package;
using io = io_package;
using regexp;
using static regexp_package;
using go.unicode;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(regexp_package.Regexp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛRegexp;
[GoMethodHost(typeof(regexp_package.bitState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛbitState;
[GoMethodHost(typeof(regexp_package.inputBytes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputBytes;
[GoMethodHost(typeof(regexp_package.inputReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputReader;
[GoMethodHost(typeof(regexp_package.inputString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputString;
[GoMethodHost(typeof(regexp_package.inputs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛinputs;
[GoMethodHost(typeof(regexp_package.lazyFlag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛlazyFlag;
[GoMethodHost(typeof(regexp_package.machine)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛmachine;
[GoMethodHost(typeof(regexp_package.queueOnePass)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class regexpᴛqueueOnePass;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class regexpᴛbitState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref bitState b, ж<syntax.Prog> Ꮡprog, nint end, nint ncap) => regexp_package.resetᴍ(ref b, Ꮡprog, end, ncap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldVisit(this ref bitState b, uint32 pc, nint pos) => regexp_package.shouldVisitᴍ(ref b, pc, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref bitState b, ж<Regexp> Ꮡre, uint32 pc, nint pos, bool arg) => regexp_package.pushᴍ(ref b, Ꮡre, pc, pos, arg);
}
partial class regexpᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryBacktrack(this ж<Regexp> Ꮡre, ж<bitState> Ꮡb, input i, uint32 pc, nint pos) => regexp_package.tryBacktrackᴍ(Ꮡre, Ꮡb, i, pc, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<nint> backtrack(this ж<Regexp> Ꮡre, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.backtrackᴍ(Ꮡre, ib, @is, pos, ncap, dstCap);
}
partial class regexpᴛinputs {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static input newBytes(this ж<inputs> Ꮡi, slice<byte> b) => regexp_package.newBytesᴍ(Ꮡi, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static input newString(this ж<inputs> Ꮡi, @string s) => regexp_package.newStringᴍ(Ꮡi, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static input newReader(this ж<inputs> Ꮡi, io.RuneReader r) => regexp_package.newReaderᴍ(Ꮡi, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref inputs i) => regexp_package.clearᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (input, nint) init(this ж<inputs> Ꮡi, io.RuneReader r, slice<byte> b, @string s) => regexp_package.initᴍ(Ꮡi, r, b, s);
}
partial class regexpᴛmachine {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref machine m, nint ncap) => regexp_package.initᴍ(ref m, ncap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<thread> alloc(this ref machine m, ж<syntax.Inst> i) => regexp_package.allocᴍ(ref m, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool match(this ж<machine> Ꮡm, input i, nint pos) => regexp_package.matchᴍ(Ꮡm, i, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref machine m, ж<queue> Ꮡq) => regexp_package.clearᴍ(ref m, Ꮡq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void step(this ref machine m, ж<queue> Ꮡrunq, ж<queue> nextq, nint pos, nint nextPos, rune c, ж<lazyFlag> nextCond) => regexp_package.stepᴍ(ref m, Ꮡrunq, nextq, pos, nextPos, c, nextCond);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<thread> add(this ref machine m, ж<queue> Ꮡq, uint32 pc, nint pos, slice<nint> cap, ж<lazyFlag> cond, ж<thread> Ꮡt) => regexp_package.addᴍ(ref m, Ꮡq, pc, pos, cap, cond, Ꮡt);
}
partial class regexpᴛlazyFlag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool match(this lazyFlag f, syntax.EmptyOp op) => regexp_package.matchᴍ(f, op);
}
partial class regexpᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<nint> doOnePass(this ж<Regexp> Ꮡre, io.RuneReader ir, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.doOnePassᴍ(Ꮡre, ir, ib, @is, pos, ncap, dstCap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool doMatch(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s) => regexp_package.doMatchᴍ(Ꮡre, r, b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<nint> doExecute(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s, nint pos, nint ncap, slice<nint> dstCap) => regexp_package.doExecuteᴍ(Ꮡre, r, b, s, pos, ncap, dstCap);
}
partial class regexpᴛqueueOnePass {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref queueOnePass q) => regexp_package.emptyᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 /*n*/ next(this ref queueOnePass q) => regexp_package.nextᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref queueOnePass q) => regexp_package.clearᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool contains(this ref queueOnePass q, uint32 u) => regexp_package.containsᴍ(ref q, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insert(this ref queueOnePass q, uint32 u) => regexp_package.insertᴍ(ref q, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertNew(this ref queueOnePass q, uint32 u) => regexp_package.insertNewᴍ(ref q, u);
}
partial class regexpᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Regexp re) => regexp_package.Stringᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Regexp> Copy(this ref Regexp re) => regexp_package.Copyᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Longest(this ref Regexp re) => regexp_package.Longestᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<machine> get(this ж<Regexp> Ꮡre) => regexp_package.getᴍ(Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void put(this ref Regexp re, ж<machine> Ꮡm) => regexp_package.putᴍ(ref re, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumSubexp(this ref Regexp re) => regexp_package.NumSubexpᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> SubexpNames(this ref Regexp re) => regexp_package.SubexpNamesᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint SubexpIndex(this ref Regexp re, @string name) => regexp_package.SubexpIndexᴍ(ref re, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string prefix, bool complete) LiteralPrefix(this ref Regexp re) => regexp_package.LiteralPrefixᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MatchReader(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.MatchReaderᴍ(Ꮡre, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MatchString(this ж<Regexp> Ꮡre, @string s) => regexp_package.MatchStringᴍ(Ꮡre, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Match(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.Matchᴍ(Ꮡre, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string ReplaceAllString(this ж<Regexp> Ꮡre, @string src, @string repl) => regexp_package.ReplaceAllStringᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string ReplaceAllLiteralString(this ж<Regexp> Ꮡre, @string src, @string repl) => regexp_package.ReplaceAllLiteralStringᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string ReplaceAllStringFunc(this ж<Regexp> Ꮡre, @string src, Func<@string, @string> repl) => regexp_package.ReplaceAllStringFuncᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> replaceAll(this ж<Regexp> Ꮡre, slice<byte> bsrc, @string src, nint nmatch, Func<slice<byte>, slice<nint>, slice<byte>> repl) => regexp_package.replaceAllᴍ(Ꮡre, bsrc, src, nmatch, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> ReplaceAll(this ж<Regexp> Ꮡre, slice<byte> src, slice<byte> repl) => regexp_package.ReplaceAllᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> ReplaceAllLiteral(this ж<Regexp> Ꮡre, slice<byte> src, slice<byte> repl) => regexp_package.ReplaceAllLiteralᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> ReplaceAllFunc(this ж<Regexp> Ꮡre, slice<byte> src, Func<slice<byte>, slice<byte>> repl) => regexp_package.ReplaceAllFuncᴍ(Ꮡre, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<nint> pad(this ref Regexp re, slice<nint> a) => regexp_package.padᴍ(ref re, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allMatches(this ж<Regexp> Ꮡre, @string s, slice<byte> b, nint n, Action<slice<nint>> deliver) => regexp_package.allMatchesᴍ(Ꮡre, s, b, n, deliver);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Find(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.Findᴍ(Ꮡre, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> /*loc*/ FindIndex(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindIndexᴍ(Ꮡre, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string FindString(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringᴍ(Ꮡre, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> /*loc*/ FindStringIndex(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringIndexᴍ(Ꮡre, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> /*loc*/ FindReaderIndex(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.FindReaderIndexᴍ(Ꮡre, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<byte>> FindSubmatch(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindSubmatchᴍ(Ꮡre, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Expand(this ref Regexp re, slice<byte> dst, slice<byte> template, slice<byte> src, slice<nint> match) => regexp_package.Expandᴍ(ref re, dst, template, src, match);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> ExpandString(this ref Regexp re, slice<byte> dst, @string template, @string src, slice<nint> match) => regexp_package.ExpandStringᴍ(ref re, dst, template, src, match);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> expand(this ref Regexp re, slice<byte> dst, @string template, slice<byte> bsrc, @string src, slice<nint> match) => regexp_package.expandᴍ(ref re, dst, template, bsrc, src, match);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> FindSubmatchIndex(this ж<Regexp> Ꮡre, slice<byte> b) => regexp_package.FindSubmatchIndexᴍ(Ꮡre, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> FindStringSubmatch(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringSubmatchᴍ(Ꮡre, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> FindStringSubmatchIndex(this ж<Regexp> Ꮡre, @string s) => regexp_package.FindStringSubmatchIndexᴍ(Ꮡre, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> FindReaderSubmatchIndex(this ж<Regexp> Ꮡre, io.RuneReader r) => regexp_package.FindReaderSubmatchIndexᴍ(Ꮡre, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<byte>> FindAll(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllᴍ(Ꮡre, b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<nint>> FindAllIndex(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllIndexᴍ(Ꮡre, b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> FindAllString(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringᴍ(Ꮡre, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<nint>> FindAllStringIndex(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringIndexᴍ(Ꮡre, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<slice<byte>>> FindAllSubmatch(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllSubmatchᴍ(Ꮡre, b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<nint>> FindAllSubmatchIndex(this ж<Regexp> Ꮡre, slice<byte> b, nint n) => regexp_package.FindAllSubmatchIndexᴍ(Ꮡre, b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<@string>> FindAllStringSubmatch(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringSubmatchᴍ(Ꮡre, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<nint>> FindAllStringSubmatchIndex(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.FindAllStringSubmatchIndexᴍ(Ꮡre, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Split(this ж<Regexp> Ꮡre, @string s, nint n) => regexp_package.Splitᴍ(Ꮡre, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ref Regexp re, slice<byte> b) => regexp_package.AppendTextᴍ(ref re, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this ref Regexp re) => regexp_package.MarshalTextᴍ(ref re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ref Regexp re, slice<byte> text) => regexp_package.UnmarshalTextᴍ(ref re, text);
}
partial class regexpᴛinputString {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune, nint) step(this ref inputString i, nint pos) => regexp_package.stepᴍ(ref i, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputString i) => regexp_package.canCheckPrefixᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasPrefix(this ref inputString i, ж<Regexp> Ꮡre) => regexp_package.hasPrefixᴍ(ref i, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint index(this ref inputString i, ж<Regexp> Ꮡre, nint pos) => regexp_package.indexᴍ(ref i, Ꮡre, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static lazyFlag context(this ref inputString i, nint pos) => regexp_package.contextᴍ(ref i, pos);
}
partial class regexpᴛinputBytes {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune, nint) step(this ref inputBytes i, nint pos) => regexp_package.stepᴍ(ref i, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputBytes i) => regexp_package.canCheckPrefixᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasPrefix(this ref inputBytes i, ж<Regexp> Ꮡre) => regexp_package.hasPrefixᴍ(ref i, Ꮡre);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint index(this ref inputBytes i, ж<Regexp> Ꮡre, nint pos) => regexp_package.indexᴍ(ref i, Ꮡre, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static lazyFlag context(this ref inputBytes i, nint pos) => regexp_package.contextᴍ(ref i, pos);
}
partial class regexpᴛinputReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune, nint) step(this ref inputReader i, nint pos) => regexp_package.stepᴍ(ref i, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canCheckPrefix(this ref inputReader i) => regexp_package.canCheckPrefixᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasPrefix(this ref inputReader i, ж<Regexp> re) => regexp_package.hasPrefixᴍ(ref i, re);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint index(this ref inputReader i, ж<Regexp> re, nint pos) => regexp_package.indexᴍ(ref i, re, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static lazyFlag context(this ref inputReader i, nint pos) => regexp_package.contextᴍ(ref i, pos);
}
// </GlyphForwarders>
