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
using static go.go.build_package;

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
[assembly: GoPositionMap("go/build/build.go", "build.cs", "AHbuAbKClKiSgpSokoKUqJKClIKq0oK4gtyClIKUgpQACAiSgoKClIKCgpTIpIKCgpSCgpSUqJKCpoKClKyygoKUgqiygoLKlAAMGpSU/LKCgoKCpoKCgqYACQwACQaSgoKCgpSCgqaUlAAeMoKUgoKClIKClAAIEoKmlIKClJS0xoKClLamgoKClAB5hgKyyqIADRSCAA4WlMaCgoKUpgAlIgATAqaCpoKCgoKClKS0xqSSpIK0tpSCgoKClILKhoKCgoKCgoKmgoKCuIKCgqaCgoLcgoKC3IKmgoKCppa+gqKCgpSCgoKCgoKCgoKClJSCgpSUlIKUgoLcyoKClIKCgoKCgoKCgqaUuIKCgqaCgoKCgoKClNyCgoKCgoKCgoKmqJKCgoKUgpSUgoKClIKUpoKCgoKCpqaCgpQACRSClLimgpSCppSmgoKmgoKigpSCgriCkoKCgoKCgoKCgoKUgpS4gpSCgoKUgpSCgoKUuOqCtIKUtqSCyoKCgoKCuIKCgoKmgoKCuO6CpoKCgoKCgoKCgoLcgoKCgoKUgoKC3IKSgpSCgoKCgqbGgoKCtIKCgrSCgoKkgoKCpoKCpoK4gpSUgoKUgoLKgoKUgqaClIKUxoKUpKSkpKSkpKSUpoKClIKCgoKCgqamAA0YAA8IhNyC7oKU6oKCgoKCmKKCgu6CyoLKgpiCgpSmgqamgoKCgoKUpoKClLiClIKmkoKUgoKUAAYQgIK2goKUgoKm3IKCgoLO4oKCgqamtIKCuLiCpoKUtIKClJSCtqaCgqaCqIKCgpiSgpSCtIKCgpSClIKCgoKUgraUrOKmgoKCgoKUpoKCpgAEEgAIAoIAIFAADwKEpoKClJSUpoKmgpSmgoKmgoKCpoKUgoK4goKUgqaCpsaCgoKUgqiSqJKolJSWgoKUgoLcAAMmABcIgoLKlIKClLaCgoKCgpSUgoKUgoKUgoLsAAQYAAwCgoKClIKCgoCClKSCAAgSgpSCpoKClJSCpoKCgoKCgpSUgpSCgoKm+MzigpLKgoK4griCgriCgqaSgoKCgoKmgriCgpSigpSmpraUtLS0tLS0pgAICtiUgoKUgoKUgoIABBwACwKCgoKClIKCgpSCABIggoKUgoKmAAgkABECgoKCgoKCgpS0grSCgsaCgrSCgoKUpIKUgpSCgoKUAAQQ0oKUlIKClKaCAAciAA4CgriClIKUgpSClIKUgpSCuAACJgAPAgAIEoKClJSCgpSCgpSUlIKU2KiiAAYS0g==", "600-608:1;623-625:2;688-714:3;866-874:4;1930-1930:1", "", "1279=Equal/1/2/1,Equal/2/2/1;1615=HasPrefix/2/2/1;2214=Contains/3/3/1;2275=HasPrefix/1/2/1,HasPrefix/2/2/1")]
[assembly: GoPositionMap("go/build/gc.go", "gc.cs", "AAwekg==")]
[assembly: GoPositionMap("go/build/read.go", "read.cs", "ACNElILKgpQACRSCqIK4koK8ooKCgoKmgoKCgpSUqJKClJSCgoKCgoKUzqKCgpTcgoKUgqaUgraCgoKUgoKCgpSmlIK2lIKokoKCqqKCgoKCpoK8ooKCgpSCvKKUgoKUgtiCgoKUgpSC2MyigoKCgpQABB4ADAKCgpSUAAoU8pSCgoKCgoKClJS4uILKgoKClIKmgriCgqaCgoKClIKCgpSCgoKUpoKClIKmgoKUyoKClIKCAAwagoKCgoKCgoKClKaCgtzuwoKCgqassoKClIKClIKClA==")]
// </GoSourcePositionMaps>

namespace go.go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using Δdoc = global::go.go.doc_package;
using token = global::go.go.token_package;
using buildcfg = global::go.@internal.buildcfg_package;
using godebug = global::go.@internal.godebug_package;
using goroot = global::go.@internal.goroot_package;
using goversion = global::go.@internal.goversion_package;
using platform = global::go.@internal.platform_package;
using syslist = global::go.@internal.syslist_package;
using io = io_package;
using fs = global::go.io.fs_package;
using os = os_package;
using exec = global::go.os.exec_package;
using pathpkg = path_package;
using filepath = global::go.path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.@internal;
using global::go.go;
using global::go.go.build;
using global::go.io;
using global::go.os;
using global::go.path;
using global::go.unicode;
using iter = iter_package;
using ꓸꓸꓸstring = Span<@string>;
using Δbuild = build_package;
using static build_package;
using bufio = bufio_package;
using parser = global::go.go.parser_package;
using scanner = global::go.go.scanner_package;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(build_package.Context)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildᴛContext;
[GoMethodHost(typeof(build_package.MultiplePackageError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildᴛMultiplePackageError;
[GoMethodHost(typeof(build_package.NoGoError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildᴛNoGoError;
[GoMethodHost(typeof(build_package.Package)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildᴛPackage;
[GoMethodHost(typeof(build_package.importReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildᴛimportReader;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class buildᴛContext {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string joinPath(this ref Context ctxt, params ꓸꓸꓸstring elemʗp) => build_package.joinPathᴍ(ref ctxt, elemʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<@string> splitPathList(this ref Context ctxt, @string s) => build_package.splitPathListᴍ(ref ctxt, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isAbsPath(this ref Context ctxt, @string path) => build_package.isAbsPathᴍ(ref ctxt, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDir(this ref Context ctxt, @string path) => build_package.isDirᴍ(ref ctxt, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string rel, bool ok) hasSubdir(this ref Context ctxt, @string root, @string dir) => build_package.hasSubdirᴍ(ref ctxt, root, dir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) readDir(this ref Context ctxt, @string path) => build_package.readDirᴍ(ref ctxt, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (io.ReadCloser, error) openFile(this ref Context ctxt, @string path) => build_package.openFileᴍ(ref ctxt, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isFile(this ref Context ctxt, @string path) => build_package.isFileᴍ(ref ctxt, path);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<@string> gopath(this ref Context ctxt) => build_package.gopathᴍ(ref ctxt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> SrcDirs(this ref Context ctxt) => build_package.SrcDirsᴍ(ref ctxt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Package>, error) ImportDir(this ж<Context> Ꮡctxt, @string dir, ImportMode mode) => build_package.ImportDirᴍ(Ꮡctxt, dir, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Package>, error) Import(this ж<Context> Ꮡctxt, @string path, @string srcDir, ImportMode mode) => build_package.Importᴍ(Ꮡctxt, path, srcDir, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error importGo(this ref Context ctxt, ж<Package> Ꮡp, @string path, @string srcDir, ImportMode mode) => build_package.importGoᴍ(ref ctxt, Ꮡp, path, srcDir, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (bool match, error err) MatchFile(this ж<Context> Ꮡctxt, @string dir, @string name) => build_package.MatchFileᴍ(Ꮡctxt, dir, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<fileInfo>, error) matchFile(this ж<Context> Ꮡctxt, @string dir, @string name, map<@string, bool> allTags, ж<bool> ᏑbinaryOnly, ж<token.FileSet> fset) => build_package.matchFileᴍ(Ꮡctxt, dir, name, allTags, ᏑbinaryOnly, fset);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool shouldBuild, bool binaryOnly, error err) shouldBuild(this ж<Context> Ꮡctxt, slice<byte> content, map<@string, bool> allTags) => build_package.shouldBuildᴍ(Ꮡctxt, content, allTags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error saveCgo(this ж<Context> Ꮡctxt, @string filename, ж<Package> Ꮡdi, ж<ast.CommentGroup> cg) => build_package.saveCgoᴍ(Ꮡctxt, filename, Ꮡdi, cg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void makePathsAbsolute(this ref Context ctxt, slice<@string> args, @string srcDir) => build_package.makePathsAbsoluteᴍ(ref ctxt, args, srcDir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool matchAuto(this ж<Context> Ꮡctxt, @string text, map<@string, bool> allTags) => build_package.matchAutoᴍ(Ꮡctxt, text, allTags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool eval(this ж<Context> Ꮡctxt, constraint.Expr x, map<@string, bool> allTags) => build_package.evalᴍ(Ꮡctxt, x, allTags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool matchTag(this ref Context ctxt, @string name, map<@string, bool> allTags) => build_package.matchTagᴍ(ref ctxt, name, allTags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool goodOSArchFile(this ref Context ctxt, @string name, map<@string, bool> allTags) => build_package.goodOSArchFileᴍ(ref ctxt, name, allTags);
}
partial class buildᴛPackage {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsCommand(this ref Package p) => build_package.IsCommandᴍ(ref p);
}
partial class buildᴛNoGoError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref NoGoError e) => build_package.Errorᴍ(ref e);
}
partial class buildᴛMultiplePackageError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref MultiplePackageError e) => build_package.Errorᴍ(ref e);
}
partial class buildᴛimportReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void syntaxError(this ref importReader r) => build_package.syntaxErrorᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte readByte(this ref importReader r) => build_package.readByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readRest(this ref importReader r) => build_package.readRestᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte peekByte(this ref importReader r, bool skipSpace) => build_package.peekByteᴍ(ref r, skipSpace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte nextByte(this ref importReader r, bool skipSpace) => build_package.nextByteᴍ(ref r, skipSpace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readKeyword(this ref importReader r, @string kw) => build_package.readKeywordᴍ(ref r, kw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readIdent(this ref importReader r) => build_package.readIdentᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readString(this ref importReader r) => build_package.readStringᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readImport(this ref importReader r) => build_package.readImportᴍ(ref r);
}
// </GlyphForwarders>
