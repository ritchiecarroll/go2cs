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
using ast = go.go.ast_package;
using constraint = go.go.build.constraint_package;
using fs = go.io.fs_package;
using io = go.io_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.build_package;
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
[assembly: GoImplement<MultiplePackageError, error>(Pointer = true)]
[assembly: GoImplement<NoGoError, error>(Pointer = true)]
[assembly: GoImplement<go.go.scanner_package.Error, error>]
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Package))]
[assembly: GoFieldAccess(typeof(fileInfo))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Comment))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommentGroup))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(importReader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/build/build.go", "build.cs", "AHLuAbKClKiSgpSokoKUqJKClIKq0oK4gtyClIKUgpTokoKCgpSCgoKUqKSCgoKUgoKUlKiSgqaCgpSssoKClIKosoKCypQADBqUlPyygoKCgqaCgoKm/AAJBpKCgoKClIKCppSUAB4ygpSCgoKUgoKUAAgSgqaUgoKUlLTGgoKUtqaCgoKUAHeGArKqogAJFIIAChaUpoKCgpSmACMiABMCpoKmgoKCgoKUpLTGpJKkgrS2lIKCgoKUgsqGgoKCgoKCgqaCgoK4goKCpoKCgtyCgoLcgqaCgoKmlr6CooKClIKCgoKCgoKCgoKUlIKClJSUgpSCgtzKgoKUgoKCgoKCgoKCppS4goKCpoKCgoKCgoKU3IKCgoKCgoKCgqaokoKCgpSClJSCgoKUgpSmgoKCgoKmpoKClAAJFIKUuKaClIKmlKaCgqaCgqKClIKCuIKSgoKCgoKCgoKCgpSClLiClIKCgpSClIKCgpS46oK0gpS2pILKgoKCgoK4goKCgqaCgoK47oKmgoKCgoKCgoKCgtyCgoKCgpSCgoLcgpKClIKCgoKCpsaCgoK0goKCtIKCgqSCgoKmgoKmgriClJSCgpSCgsqCgpSCpoKUgpSmgpSkpKSkpKSkpJSmgoKUgoKCgoKCpqYACxgADwiE3ILugpTqgoKCgoKYooKC7oLKgsqCmIKClKaCpqaCgoKCgpSmgoKUuIKUgqaSgpSCgpQABhCAgraCgpSCgqbcgoKCgq7igoKCpqa0goK4uIKmgpS0goKUlIK2poKCpoKogoKCmJKClIK0goKClIKUgoKCgpSCtpSs4qaCgoKCgpSmgoKmAAISAAgCggAcUAAPAoSmgoKUlJSmgqaClKaCgqaCgoKmgpSCgriCgpSCpoKmpoKCgpSCqJKokqiUlJaCgpSCgtwAASYAFwiCgsqUgoKUtoKCgoKClJSCgpSCgpSCguwAAhgADAKCgoKUgoKCgIKUpIIACBKClIKmgoKUlIKmgoKCgoKClJSClIKCgqb4rOKCksqCgriCuIKCuIKCppKCgoKCgqaCuIKClKKClKamtpS0tLS0tLSm6tiUgoKUgoKUgoIAAhwACwKCgoKClIKCgpSCABAggoKUgoKmAAgkABECgoKCgoKCgpS0grSCgsaCgrSCgoKUpIKUgpSCgoKUAAIQ0oKUlIKClKaCAAciAA4CgriClIKUgpSClIKUgpSCuAACJgAPAgAIEoKClJSCgpSCgpSUlIKUuKiiAAYS0g==", "600-608:1;623-625:2;688-714:3;866-874:4;1930-1930:1", "", "1249=Equal/1/2/1,Equal/2/2/1;1577=HasPrefix/2/2/1;2160=Contains/3/3/1;2219=HasPrefix/1/2/1,HasPrefix/2/2/1")]
[assembly: GoPositionMap("go/build/gc.go", "gc.cs", "AAwekg==")]
[assembly: GoPositionMap("go/build/read.go", "read.cs", "ACJElILKgpQACRSCqIKYkoK8ooKCgoKmgoKCgpSUqJKClJSCgoKCgoKUzqKCgpTcgoKUgqaUgraCgoKUgoKCgpSmlIK2lIKokoKCqqKCgoKCpoK8ooKCgpSCvKKUgoKUgtiCgoKUgpSC2MyigoKCgpQAAh4ADAKCgpSUAAoU8pSCgoKCgoKClJS4uILKgoKClIKmgriCgqaCgoKClIKCgpSCgoKUpoKClIKmgoKUyoKClIKCAAwagoKCgoKCgoKClKaCgtzuwoKCgqassoKClIKClIKClA==")]
// </GoSourcePositionMaps>

namespace go.go;

using static global::go.go.build_package;
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using fs = global::go.io.fs_package;
using io = global::go.io_package;
using token = global::go.go.token_package;
using ꓸꓸꓸstring = Span<@string>;

[GoPackage("build")]
public static partial class build_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct Import_tried {}
    internal partial struct fileEmbed {}
    internal partial struct fileImport {}
    internal partial struct fileInfo {}
    internal partial struct importReader {}
    public partial struct Context {}
    public partial struct Directive {}
    public partial struct ImportMode {}
    public partial struct MultiplePackageError {}
    public partial struct NoGoError {}
    public partial struct Package {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdoc() => builtin.initPackage(typeof(global::go.go.doc_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbuildcfg() => builtin.initPackage(typeof(global::go.@internal.buildcfg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgoroot() => builtin.initPackage(typeof(global::go.@internal.goroot_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸplatform() => builtin.initPackage(typeof(global::go.@internal.platform_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyslist() => builtin.initPackage(typeof(global::go.@internal.syslist_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
[GoMethodHost(typeof(build_package.Context)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildᴛContext;
[GoMethodHost(typeof(build_package.MultiplePackageError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildᴛMultiplePackageError;
[GoMethodHost(typeof(build_package.NoGoError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildᴛNoGoError;
[GoMethodHost(typeof(build_package.Package)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildᴛPackage;
[GoMethodHost(typeof(build_package.importReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class buildᴛimportReader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class buildᴛContext {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string joinPath(this ref Context ctxt, params ꓸꓸꓸstring elemʗp) => build_package.joinPath⁔(ref ctxt, elemʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> splitPathList(this ref Context ctxt, @string s) => build_package.splitPathList⁔(ref ctxt, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isAbsPath(this ref Context ctxt, @string path) => build_package.isAbsPath⁔(ref ctxt, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDir(this ref Context ctxt, @string path) => build_package.isDir⁔(ref ctxt, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string rel, bool ok) hasSubdir(this ref Context ctxt, @string root, @string dir) => build_package.hasSubdir⁔(ref ctxt, root, dir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) readDir(this ref Context ctxt, @string path) => build_package.readDir⁔(ref ctxt, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (io.ReadCloser, error) openFile(this ref Context ctxt, @string path) => build_package.openFile⁔(ref ctxt, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isFile(this ref Context ctxt, @string path) => build_package.isFile⁔(ref ctxt, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> gopath(this ref Context ctxt) => build_package.gopath⁔(ref ctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> SrcDirs(this ref Context ctxt) => build_package.SrcDirs⁔(ref ctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Package>, error) ImportDir(this ж<Context> Ꮡctxt, @string dir, ImportMode mode) => build_package.ImportDir⁔(Ꮡctxt, dir, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Package>, error) Import(this ж<Context> Ꮡctxt, @string path, @string srcDir, ImportMode mode) => build_package.Import⁔(Ꮡctxt, path, srcDir, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error importGo(this ref Context ctxt, ж<Package> Ꮡp, @string path, @string srcDir, ImportMode mode) => build_package.importGo⁔(ref ctxt, Ꮡp, path, srcDir, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool match, error err) MatchFile(this ж<Context> Ꮡctxt, @string dir, @string name) => build_package.MatchFile⁔(Ꮡctxt, dir, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<fileInfo>, error) matchFile(this ж<Context> Ꮡctxt, @string dir, @string name, map<@string, bool> allTags, ж<bool> ᏑbinaryOnly, ж<token.FileSet> Ꮡfset) => build_package.matchFile⁔(Ꮡctxt, dir, name, allTags, ᏑbinaryOnly, Ꮡfset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool shouldBuild, bool binaryOnly, error err) shouldBuild(this ж<Context> Ꮡctxt, slice<byte> content, map<@string, bool> allTags) => build_package.shouldBuild⁔(Ꮡctxt, content, allTags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error saveCgo(this ж<Context> Ꮡctxt, @string filename, ж<Package> Ꮡdi, ж<ast.CommentGroup> Ꮡcg) => build_package.saveCgo⁔(Ꮡctxt, filename, Ꮡdi, Ꮡcg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void makePathsAbsolute(this ref Context ctxt, slice<@string> args, @string srcDir) => build_package.makePathsAbsolute⁔(ref ctxt, args, srcDir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool matchAuto(this ж<Context> Ꮡctxt, @string text, map<@string, bool> allTags) => build_package.matchAuto⁔(Ꮡctxt, text, allTags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool eval(this ж<Context> Ꮡctxt, constraint.Expr x, map<@string, bool> allTags) => build_package.eval⁔(Ꮡctxt, x, allTags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool matchTag(this ref Context ctxt, @string name, map<@string, bool> allTags) => build_package.matchTag⁔(ref ctxt, name, allTags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool goodOSArchFile(this ref Context ctxt, @string name, map<@string, bool> allTags) => build_package.goodOSArchFile⁔(ref ctxt, name, allTags);
}
partial class buildᴛMultiplePackageError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref MultiplePackageError e) => build_package.Error⁔(ref e);
}
partial class buildᴛNoGoError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref NoGoError e) => build_package.Error⁔(ref e);
}
partial class buildᴛPackage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsCommand(this ref Package p) => build_package.IsCommand⁔(ref p);
}
partial class buildᴛimportReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void syntaxError(this ref importReader r) => build_package.syntaxError⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte readByte(this ref importReader r) => build_package.readByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readRest(this ref importReader r) => build_package.readRest⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte peekByte(this ref importReader r, bool skipSpace) => build_package.peekByte⁔(ref r, skipSpace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte nextByte(this ref importReader r, bool skipSpace) => build_package.nextByte⁔(ref r, skipSpace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readKeyword(this ref importReader r, @string kw) => build_package.readKeyword⁔(ref r, kw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readIdent(this ref importReader r) => build_package.readIdent⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readString(this ref importReader r) => build_package.readString⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readImport(this ref importReader r) => build_package.readImport⁔(ref r);
}
// </MethodForwarders>
