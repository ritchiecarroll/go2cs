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
using iter = go.iter_package;
// </ImportedTypeAliases>

using go;
using static go.go.token_package;
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
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(FileSet))]
[assembly: GoFieldAccess(typeof(lineInfo))]
[assembly: GoFieldAccess(typeof(node))]
[assembly: GoFieldAccess(typeof(tree))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/token/position.go", "position.cs", "ACJEgAABFAAIAoKCgpSCgqaClAAgRJIAFC6SqJKokqiSqLKCgoKs0oKClK4ACQKClIKCgu6CoqjCgoKCAAIWAAsEgoKCyoKCgqrSgoKCgpSCgsqCkqwACAKClIKCgpQADx6iAAIWAAoCgoKUqJKUgsaCxriCuAACGAAJAgACGAAJAqqipoKGppSsAAgCgoKAgqSUgoKClIKCyoKm7oKmsoKCggACEPKClKyyACJIks7CgoKCAAIkABUElIKCgpSClIKCpoKCpoKCgt4AHCaClIKCtAACEAAMAoKUlIKCtKjCgsqigoLkpMSCpoKUgqaClNzSgpSu4oKCpqqirAALFJKClIKUpg==", "334-336:1;559-563:1")]
[assembly: GoPositionMap("go/token/serialize.go", "serialize.cs", "ABkwsoKCpoKCggAHEIKUqLKUgoKCgoLulIKU", "", "", "63=Clone/1/2/4,Clone/2/2/5")]
[assembly: GoPositionMap("go/token/token.go", "token.cs", "AIQBjAIAZdYB0oKClIKUAAsispSkpKSklMqCgoK6koKUrrCYoqqQlpKCqKSCrLKClIKCpg==")]
[assembly: GoPositionMap("go/token/tree.go", "tree.cs", "ADRqggACGAAJApSklKrSgoKClIKUgoK4goKCgpSClIKCAAMQAAgCkoKCgoKClKauwoKClIKCgqaClKYABhDClKSkpLiigoKUlIKClKaigoK4soKCuLKCgriigpSmgoKCpqKUgpS0tLQABBQACAKCgoKUgpS2gpSkuJS81IKClIKUgoKUgoKq1IKClIKUgoKUgoKq0oKCgpSCAAUQ0riClIKUgoKospSCyoKUpoKUtoKCtraCgoKCgqrCgpSCgoKUgoKCgpSs4oKUgoKClA==", "132-151:1", "", "362=Name/1/2/1,Base/1/2/1,End/1/2/1,Name/2/2/2,Base/2/2/2,End/2/2/2")]
// </GoSourcePositionMaps>

namespace go.go;

using static global::go.go.token_package;
using iter = global::go.iter_package;
using ꓸꓸꓸжFile = Span<ж<token_package.File>>;

[GoPackage("token")]
public static partial class token_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct key {}
    internal partial struct node {}
    internal partial struct serializedFileSet {}
    internal partial struct tree {}
    public partial struct File {}
    public partial struct FileSet {}
    public partial struct Pos {}
    public partial struct Position {}
    public partial struct Token {}
    public partial struct lineInfo {}
    public partial struct serializedFile {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(token_package.File)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛFile;
[GoMethodHost(typeof(token_package.FileSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛFileSet;
[GoMethodHost(typeof(token_package.Pos)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛPos;
[GoMethodHost(typeof(token_package.Position)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛPosition;
[GoMethodHost(typeof(token_package.Token)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛToken;
[GoMethodHost(typeof(token_package.node)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛnode;
[GoMethodHost(typeof(token_package.tree)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tokenᴛtree;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class tokenᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref File f) => token_package.Name⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Base(this ref File f) => token_package.Base⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref File f) => token_package.Size⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Pos End(this ref File f) => token_package.End⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint LineCount(this ж<File> Ꮡf) => token_package.LineCount⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddLine(this ж<File> Ꮡf, nint offset) => token_package.AddLine⁔(Ꮡf, offset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MergeLine(this ж<File> Ꮡf, nint line) => token_package.MergeLine⁔(Ꮡf, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> Lines(this ж<File> Ꮡf) => token_package.Lines⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool SetLines(this ж<File> Ꮡf, slice<nint> lines) => token_package.SetLines⁔(Ꮡf, lines);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetLinesForContent(this ж<File> Ꮡf, slice<byte> content) => token_package.SetLinesForContent⁔(Ꮡf, content);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Pos LineStart(this ж<File> Ꮡf, nint line) => token_package.LineStart⁔(Ꮡf, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddLineInfo(this ж<File> Ꮡf, nint offset, @string filename, nint line) => token_package.AddLineInfo⁔(Ꮡf, offset, filename, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddLineColumnInfo(this ж<File> Ꮡf, nint offset, @string filename, nint line, nint column) => token_package.AddLineColumnInfo⁔(Ꮡf, offset, filename, line, column);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint fixOffset(this ref File f, nint offset) => token_package.fixOffset⁔(ref f, offset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Pos Pos(this ref File f, nint offset) => token_package.Pos⁔(ref f, offset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Offset(this ref File f, Pos p) => token_package.Offset⁔(ref f, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Line(this ж<File> Ꮡf, Pos p) => token_package.Line⁔(Ꮡf, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string filename, nint line, nint column) unpack(this ж<File> Ꮡf, nint offset, bool adjusted) => token_package.unpack⁔(Ꮡf, offset, adjusted);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Position /*pos*/ position(this ж<File> Ꮡf, Pos p, bool adjusted) => token_package.position⁔(Ꮡf, p, adjusted);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Position /*pos*/ PositionFor(this ж<File> Ꮡf, Pos p, bool adjusted) => token_package.PositionFor⁔(Ꮡf, p, adjusted);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Position /*pos*/ Position(this ж<File> Ꮡf, Pos p) => token_package.Position⁔(Ꮡf, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static key key(this ref File f) => token_package.key⁔(ref f);
}
partial class tokenᴛFileSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Base(this ж<FileSet> Ꮡs) => token_package.Base⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<File> AddFile(this ж<FileSet> Ꮡs, @string filename, nint @base, nint size) => token_package.AddFile⁔(Ꮡs, filename, @base, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddExistingFiles(this ж<FileSet> Ꮡs, params ꓸꓸꓸжFile filesʗp) => token_package.AddExistingFiles⁔(Ꮡs, filesʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RemoveFile(this ж<FileSet> Ꮡs, ж<File> Ꮡfile) => token_package.RemoveFile⁔(Ꮡs, Ꮡfile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Iterate(this ж<FileSet> Ꮡs, Func<ж<File>, bool> yield) => token_package.Iterate⁔(Ꮡs, yield);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<File> @file(this ж<FileSet> Ꮡs, Pos p) => token_package.file⁔(Ꮡs, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<File> /*f*/ File(this ж<FileSet> Ꮡs, Pos p) => token_package.File⁔(Ꮡs, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Position /*pos*/ PositionFor(this ж<FileSet> Ꮡs, Pos p, bool adjusted) => token_package.PositionFor⁔(Ꮡs, p, adjusted);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Position /*pos*/ Position(this ж<FileSet> Ꮡs, Pos p) => token_package.Position⁔(Ꮡs, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Read(this ж<FileSet> Ꮡs, Func<any, error> decode) => token_package.Read⁔(Ꮡs, decode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ж<FileSet> Ꮡs, Func<any, error> encode) => token_package.Write⁔(Ꮡs, encode);
}
partial class tokenᴛPos {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this Pos p) => token_package.IsValid⁔(p);
}
partial class tokenᴛPosition {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this ref Position pos) => token_package.IsValid⁔(ref pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Position pos) => token_package.String⁔(pos);
}
partial class tokenᴛToken {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Token tok) => token_package.String⁔(tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Precedence(this Token op) => token_package.Precedence⁔(op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsLiteral(this Token tok) => token_package.IsLiteral⁔(tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsOperator(this Token tok) => token_package.IsOperator⁔(tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsKeyword(this Token tok) => token_package.IsKeyword⁔(tok);
}
partial class tokenᴛnode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void check(this ж<node> Ꮡn, ж<node> Ꮡparent) => token_package.check⁔(Ꮡn, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkBalance(this ref node n) => token_package.checkBalance⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node> next(this ж<node> Ꮡx) => token_package.next⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setLeft(this ж<node> Ꮡx, ж<node> Ꮡy) => token_package.setLeft⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setRight(this ж<node> Ꮡx, ж<node> Ꮡy) => token_package.setRight⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 safeHeight(this ж<node> Ꮡn) => token_package.safeHeight⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ref node n) => token_package.update⁔(ref n);
}
partial class tokenᴛtree {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ж<node>> pos, ж<node> parent) locate(this ж<tree> Ꮡt, key k) => token_package.locate⁔(Ꮡt, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static iter.Seq<ж<File>> all(this ж<tree> Ꮡt) => token_package.all⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node> nextAfter(this ref tree t, ж<ж<node>> Ꮡpos, ж<node> Ꮡparent) => token_package.nextAfter⁔(ref t, Ꮡpos, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setRoot(this ref tree t, ж<node> Ꮡx) => token_package.setRoot⁔(ref t, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void replaceChild(this ref tree t, ж<node> Ꮡparent, ж<node> Ꮡold, ж<node> Ꮡnew) => token_package.replaceChild⁔(ref t, Ꮡparent, Ꮡold, Ꮡnew);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rebalanceUp(this ref tree t, ж<node> Ꮡx) => token_package.rebalanceUp⁔(ref t, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node> rotateRight(this ref tree t, ж<node> Ꮡy) => token_package.rotateRight⁔(ref t, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node> rotateLeft(this ref tree t, ж<node> Ꮡx) => token_package.rotateLeft⁔(ref t, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ж<tree> Ꮡt, ж<File> Ꮡfile) => token_package.add⁔(Ꮡt, Ꮡfile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref tree t, ж<File> Ꮡfile, ж<ж<node>> Ꮡpos, ж<node> Ꮡparent) => token_package.set⁔(ref t, Ꮡfile, Ꮡpos, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void delete(this ref tree t, ж<ж<node>> Ꮡpos) => token_package.delete⁔(ref t, Ꮡpos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteSwap(this ref tree t, ж<ж<node>> Ꮡpos) => token_package.deleteSwap⁔(ref t, Ꮡpos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<node> /*z*/ deleteMin(this ref tree t, ж<ж<node>> Ꮡzpos) => token_package.deleteMin⁔(ref t, Ꮡzpos);
}
// </MethodForwarders>
