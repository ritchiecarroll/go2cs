// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using Δdoc = global::go.go.doc_package;
using token = global::go.go.token_package;
using buildcfg = @internal.buildcfg_package;
using godebug = @internal.godebug_package;
using goroot = @internal.goroot_package;
using goversion = @internal.goversion_package;
using platform = @internal.platform_package;
using syslist = @internal.syslist_package;
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
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using global::go.go;
using global::go.go.build;
using global::go.io;
using global::go.os;
using global::go.path;
using global::go.unicode;
using ꓸꓸꓸstring = Span<@string>;

partial class build_package {

// A Context specifies the supporting context for a build.
[GoType] partial struct Context {
    public @string GOARCH; // target architecture
    public @string GOOS; // target operating system
    public @string GOROOT; // Go root
    public @string GOPATH; // Go paths
    // Dir is the caller's working directory, or the empty string to use
    // the current directory of the running process. In module mode, this is used
    // to locate the main module.
    //
    // If Dir is non-empty, directories passed to Import and ImportDir must
    // be absolute.
    public @string Dir;
    public bool CgoEnabled;   // whether cgo files are included
    public bool UseAllFiles;   // use files regardless of go:build lines, file names
    public @string Compiler; // compiler to assume when computing target paths
    // The build, tool, and release tags specify build constraints
    // that should be considered satisfied when processing go:build lines.
    // Clients creating a new context may customize BuildTags, which
    // defaults to empty, but it is usually an error to customize ToolTags or ReleaseTags.
    // ToolTags defaults to build tags appropriate to the current Go toolchain configuration.
    // ReleaseTags defaults to the list of Go releases the current release is compatible with.
    // BuildTags is not set for the Default build Context.
    // In addition to the BuildTags, ToolTags, and ReleaseTags, build constraints
    // consider the values of GOARCH and GOOS as satisfied tags.
    // The last element in ReleaseTags is assumed to be the current release.
    public slice<@string> BuildTags;
    public slice<@string> ToolTags;
    public slice<@string> ReleaseTags;
    // The install suffix specifies a suffix to use in the name of the installation
    // directory. By default it is empty, but custom builds that need to keep
    // their outputs separate can set InstallSuffix to do so. For example, when
    // using the race detector, the go command uses InstallSuffix = "race", so
    // that on a Linux/386 system, packages are written to a directory named
    // "linux_386_race" instead of the usual "linux_386".
    public @string InstallSuffix;
// By default, Import uses the operating system's file system calls
// to read directories and files. To read from other sources,
// callers can set the following functions. They all have default
// behaviors that use the local file system, so clients need only set
// the functions whose behaviors they wish to change.

    // JoinPath joins the sequence of path fragments into a single path.
    // If JoinPath is nil, Import uses filepath.Join.
    public Funcꓸꓸꓸ<@string, @string> JoinPath;
    // SplitPathList splits the path list into a slice of individual paths.
    // If SplitPathList is nil, Import uses filepath.SplitList.
    public Func<@string, slice<@string>> SplitPathList;
    // IsAbsPath reports whether path is an absolute path.
    // If IsAbsPath is nil, Import uses filepath.IsAbs.
    public Func<@string, bool> IsAbsPath;
    // IsDir reports whether the path names a directory.
    // If IsDir is nil, Import calls os.Stat and uses the result's IsDir method.
    public Func<@string, bool> IsDir;
    // HasSubdir reports whether dir is lexically a subdirectory of
    // root, perhaps multiple levels below. It does not try to check
    // whether dir exists.
    // If so, HasSubdir sets rel to a slash-separated path that
    // can be joined to root to produce a path equivalent to dir.
    // If HasSubdir is nil, Import uses an implementation built on
    // filepath.EvalSymlinks.
    public Func<@string, @string, (@string rel, bool ok)> HasSubdir;
    // ReadDir returns a slice of fs.FileInfo, sorted by Name,
    // describing the content of the named directory.
    // If ReadDir is nil, Import uses os.ReadDir.
    public Func<@string, (slice<fs.FileInfo>, error)> ReadDir;
    // OpenFile opens a file (not a directory) for reading.
    // If OpenFile is nil, Import uses os.Open.
    public Func<@string, (io.ReadCloser, error)> OpenFile;
}

// joinPath calls ctxt.JoinPath (if not nil) or else filepath.Join.
[GoRecv] internal static @string joinPath(this ref Context ctxt, params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.sslice();

    {
        var f = ctxt.JoinPath; if (f != default!) {
            return f(elem.ꓸꓸꓸ);
        }
    }
    return filepath.Join(elem.ꓸꓸꓸ);
}

// splitPathList calls ctxt.SplitPathList (if not nil) or else filepath.SplitList.
[GoRecv] internal static slice<@string> splitPathList(this ref Context ctxt, @string s) {
    {
        var f = ctxt.SplitPathList; if (f != default!) {
            return f(s);
        }
    }
    return filepath.SplitList(s);
}

// isAbsPath calls ctxt.IsAbsPath (if not nil) or else filepath.IsAbs.
[GoRecv] internal static bool isAbsPath(this ref Context ctxt, @string path) {
    {
        var f = ctxt.IsAbsPath; if (f != default!) {
            return f(path);
        }
    }
    return filepath.IsAbs(path);
}

// isDir calls ctxt.IsDir (if not nil) or else uses os.Stat.
[GoRecv] internal static bool isDir(this ref Context ctxt, @string path) {
    {
        var f = ctxt.IsDir; if (f != default!) {
            return f(path);
        }
    }
    var (fi, err) = os.Stat(path);
    return err == default! && fi.IsDir();
}

// hasSubdir calls ctxt.HasSubdir (if not nil) or else uses
// the local file system to answer the question.
[GoRecv] internal static (@string rel, bool ok) hasSubdir(this ref Context ctxt, @string root, @string dir) {
    @string rel = default!;
    bool ok = default!;

    {
        var f = ctxt.HasSubdir; if (f != default!) {
            return f(root, dir);
        }
    }
    // Try using paths we received.
    {
        (rel, ok) = hasSubdir(root, dir); if (ok) {
            return (rel, ok);
        }
    }
    // Try expanding symlinks and comparing
    // expanded against unexpanded and
    // expanded against expanded.
    var (rootSym, _) = filepath.EvalSymlinks(root);
    var (dirSym, _) = filepath.EvalSymlinks(dir);
    {
        (rel, ok) = hasSubdir(rootSym, dir); if (ok) {
            return (rel, ok);
        }
    }
    {
        (rel, ok) = hasSubdir(root, dirSym); if (ok) {
            return (rel, ok);
        }
    }
    return hasSubdir(rootSym, dirSym);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sepᶜ = "/";

// hasSubdir reports if dir is within root by performing lexical analysis only.
internal static (@string rel, bool ok) hasSubdir(@string root, @string dir) {
    @string sep = sepᶜ;
    root = filepath.Clean(root);
    if (!strings.HasSuffix(root, sep)) {
        root += sep;
    }
    dir = filepath.Clean(dir);
    var (after, found) = strings.CutPrefix(dir, root);
    if (!found) {
        return ("", false);
    }
    return (filepath.ToSlash(after), true);
}

// readDir calls ctxt.ReadDir (if not nil) or else os.ReadDir.
[GoRecv] internal static (slice<fs.DirEntry>, error) readDir(this ref Context ctxt, @string path) {
    // TODO: add a fs.DirEntry version of Context.ReadDir
    {
        var f = ctxt.ReadDir; if (f != default!) {
            var (fis, err) = f(path);
            if (err != default!) {
                return (default!, err);
            }
            var des = new slice<fs.DirEntry>(len(fis));
            foreach (var (i, fi) in fis) {
                des[i] = fs.FileInfoToDirEntry(fi);
            }
            return (des, default!);
        }
    }
    return os.ReadDir(path);
}

// openFile calls ctxt.OpenFile (if not nil) or else os.Open.
[GoRecv] internal static (io.ReadCloser, error) openFile(this ref Context ctxt, @string path) {
    {
        var fn = ctxt.OpenFile; if (fn != default!) {
            return fn(path);
        }
    }
    var (f, err) = os.Open(path);
    if (err != default!) {
        return (default!, err); // nil interface
    }
    return (new os_FileжReadCloser(f), default!);
}

// isFile determines whether path is a file by trying to open it.
// It reuses openFile instead of adding another function to the
// list in Context.
[GoRecv] internal static bool isFile(this ref Context ctxt, @string path) {
    var (f, err) = ctxt.openFile(path);
    if (err != default!) {
        return false;
    }
    f.Close();
    return true;
}

// gopath returns the list of Go path directories.
[GoRecv] internal static slice<@string> gopath(this ref Context ctxt) {
    slice<@string> all = default!;
    foreach (var (_, p) in ctxt.splitPathList(ctxt.GOPATH)) {
        if (p == ""u8 || p == ctxt.GOROOT) {
            // Empty paths are uninteresting.
            // If the path is the GOROOT, ignore it.
            // People sometimes set GOPATH=$GOROOT.
            // Do not get confused by this common mistake.
            continue;
        }
        if (strings.HasPrefix(p, "~"u8)) {
            // Path segments starting with ~ on Unix are almost always
            // users who have incorrectly quoted ~ while setting GOPATH,
            // preventing it from expanding to $HOME.
            // The situation is made more confusing by the fact that
            // bash allows quoted ~ in $PATH (most shells do not).
            // Do not get confused by this, and do not try to use the path.
            // It does not exist, and printing errors about it confuses
            // those users even more, because they think "sure ~ exists!".
            // The go command diagnoses this situation and prints a
            // useful error.
            // On Windows, ~ is used in short names, such as c:\progra~1
            // for c:\program files.
            continue;
        }
        all = append(all, p);
    }
    return all;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcˢ = "src"u8;

// SrcDirs returns a list of package source root directories.
// It draws from the current Go root and Go path but omits directories
// that do not exist.
[GoRecv] public static slice<@string> SrcDirs(this ref Context ctxt) {
    slice<@string> all = default!;
    if (ctxt.GOROOT != ""u8 && ctxt.Compiler != "gccgo"u8) {
        @string dir = ctxt.joinPath(ctxt.GOROOT, srcˢ);
        if (ctxt.isDir(dir)) {
            all = append(all, dir);
        }
    }
    foreach (var (_, p) in ctxt.gopath()) {
        @string dir = ctxt.joinPath(p, srcˢ);
        if (ctxt.isDir(dir)) {
            all = append(all, dir);
        }
    }
    return all;
}

// Default is the default Context for builds.
// It uses the GOARCH, GOOS, GOROOT, and GOPATH environment variables
// if set, or else the compiled code's GOARCH, GOOS, and GOROOT.
public static ж<Context> ᏑDefault = new StandardBox<Context>(default(Context));
public static ref Context Default => ref ᏑDefault.Value;
internal static void initᴛDefault() { Default = defaultContext(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string homeˢ = "HOME"u8;
internal static readonly @string userprofileˢ = "USERPROFILE"u8;
internal static readonly @string homeˢ2 = "home"u8;

// Keep consistent with cmd/go/internal/cfg.defaultGOPATH.
internal static @string defaultGOPATH() {
    @string env = homeˢ;
    if (runtime.GOOS == "windows"u8){
        env = userprofileˢ;
    } else 
    if (runtime.GOOS == "plan9"u8) {
        env = homeˢ2;
    }
    {
        @string home = os.Getenv(env); if (home != ""u8) {
            @string def = filepath.Join(home, "go");
            if (filepath.Clean(def) == filepath.Clean(runtime.GOROOT())) {
                // Don't set the default GOPATH to GOROOT,
                // as that will trigger warnings from the go tool.
                return ""u8;
            }
            return def;
        }
    }
    return ""u8;
}

// defaultToolTags should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gopherjs/gopherjs
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultToolTags
public static slice<@string> defaultToolTags;

// defaultReleaseTags should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/gopherjs/gopherjs
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultReleaseTags
public static slice<@string> defaultReleaseTags;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopathˢ = "GOPATH"u8;
internal static readonly @string cgoEnabledˢ = "CGO_ENABLED"u8;

internal static Context defaultContext() {
    Context c = default!;
    c.GOARCH = buildcfg.GOARCH;
    c.GOOS = buildcfg.GOOS;
    {
        @string goroot = runtime.GOROOT(); if (goroot != ""u8) {
            c.GOROOT = filepath.Clean(goroot);
        }
    }
    c.GOPATH = envOr(gopathˢ, defaultGOPATH());
    c.Compiler = runtime.Compiler;
    c.ToolTags = appendꓸꓸꓸ(c.ToolTags, buildcfg.ToolTags);
    defaultToolTags = appendꓸꓸꓸ(new @string[]{}.slice(), c.ToolTags); // our own private copy
    // Each major Go release in the Go 1.x series adds a new
    // "go1.x" release tag. That is, the go1.x tag is present in
    // all releases >= Go 1.x. Code that requires Go 1.x or later
    // should say "go:build go1.x", and code that should only be
    // built before Go 1.x (perhaps it is the stub to use in that
    // case) should say "go:build !go1.x".
    // The last element in ReleaseTags is the current release.
    for (nint i = 1; i <= goversion.Version; i++) {
        c.ReleaseTags = append(c.ReleaseTags, "go1."u8 + strconv.Itoa(i));
    }
    defaultReleaseTags = appendꓸꓸꓸ(new @string[]{}.slice(), c.ReleaseTags); // our own private copy
    @string env = os.Getenv(cgoEnabledˢ);
    if (env == ""u8) {
        env = defaultCGO_ENABLED;
    }
    var exprᴛ1 = env;
    if (exprᴛ1 == "1"u8) {
        c.CgoEnabled = true;
    }
    else if (exprᴛ1 == "0"u8) {
        c.CgoEnabled = false;
    }
    else { /* default: */
        do {
            if (runtime.GOARCH == c.GOARCH && runtime.GOOS == c.GOOS) {
                // cgo must be explicitly enabled for cross compilation builds
                c.CgoEnabled = platform.CgoSupported(c.GOOS, c.GOARCH);
                break;
            }
            c.CgoEnabled = false;
        } while (false);
    }

    return c;
}

internal static @string envOr(@string name, @string def) {
    @string s = os.Getenv(name);
    if (s == ""u8) {
        return def;
    }
    return s;
}

[GoType("num:nuint")] partial struct ImportMode;

public static ImportMode FindOnly => /* 1 << iota */ 1;
public static ImportMode AllowBinary => 2;
public static ImportMode ImportComment => 4;
public static ImportMode IgnoreVendor => 8;

// A Package describes the Go package found in a directory.
[GoType] partial struct Package {
    public @string Dir;  // directory containing package sources
    public @string Name;  // package name
    public @string ImportComment;  // path in import comment on package statement
    public @string Doc;  // documentation synopsis
    public @string ImportPath;  // import path of package ("" if unknown)
    public @string Root;  // root of Go tree where this package lives
    public @string SrcRoot;  // package source root directory ("" if unknown)
    public @string PkgRoot;  // package install root directory ("" if unknown)
    public @string PkgTargetRoot;  // architecture dependent install root directory ("" if unknown)
    public @string BinDir;  // command install directory ("" if unknown)
    public bool Goroot;     // package found in Go root
    public @string PkgObj;  // installed .a file
    public slice<@string> AllTags; // tags that can influence file selection in this directory
    public @string ConflictDir;  // this directory shadows Dir in $GOPATH
    public bool BinaryOnly;     // cannot be rebuilt from source (has //go:binary-only-package comment)
    // Source files
    public slice<@string> GoFiles; // .go source files (excluding CgoFiles, TestGoFiles, XTestGoFiles)
    public slice<@string> CgoFiles; // .go source files that import "C"
    public slice<@string> IgnoredGoFiles; // .go source files ignored for this build (including ignored _test.go files)
    public slice<@string> InvalidGoFiles; // .go source files with detected problems (parse error, wrong package name, and so on)
    public slice<@string> IgnoredOtherFiles; // non-.go source files ignored for this build
    public slice<@string> CFiles; // .c source files
    public slice<@string> CXXFiles; // .cc, .cpp and .cxx source files
    public slice<@string> MFiles; // .m (Objective-C) source files
    public slice<@string> HFiles; // .h, .hh, .hpp and .hxx source files
    public slice<@string> FFiles; // .f, .F, .for and .f90 Fortran source files
    public slice<@string> SFiles; // .s source files
    public slice<@string> SwigFiles; // .swig files
    public slice<@string> SwigCXXFiles; // .swigcxx files
    public slice<@string> SysoFiles; // .syso system object files to add to archive
    // Cgo directives
    public slice<@string> CgoCFLAGS; // Cgo CFLAGS directives
    public slice<@string> CgoCPPFLAGS; // Cgo CPPFLAGS directives
    public slice<@string> CgoCXXFLAGS; // Cgo CXXFLAGS directives
    public slice<@string> CgoFFLAGS; // Cgo FFLAGS directives
    public slice<@string> CgoLDFLAGS; // Cgo LDFLAGS directives
    public slice<@string> CgoPkgConfig; // Cgo pkg-config directives
    // Test information
    public slice<@string> TestGoFiles; // _test.go files in package
    public slice<@string> XTestGoFiles; // _test.go files outside package
    // Go directive comments (//go:zzz...) found in source files.
    public slice<Directive> Directives;
    public slice<Directive> TestDirectives;
    public slice<Directive> XTestDirectives;
    // Dependency information
    public slice<@string> Imports;              // import paths from GoFiles, CgoFiles
    public map<@string, slice<tokenꓸPosition>> ImportPos; // line information for Imports
    public slice<@string> TestImports;              // import paths from TestGoFiles
    public map<@string, slice<tokenꓸPosition>> TestImportPos; // line information for TestImports
    public slice<@string> XTestImports;              // import paths from XTestGoFiles
    public map<@string, slice<tokenꓸPosition>> XTestImportPos; // line information for XTestImports
    // //go:embed patterns found in Go source files
    // For example, if a source file says
    //	//go:embed a* b.c
    // then the list will contain those two strings as separate entries.
    // (See package embed for more details about //go:embed.)
    public slice<@string> EmbedPatterns;              // patterns from GoFiles, CgoFiles
    public map<@string, slice<tokenꓸPosition>> EmbedPatternPos; // line information for EmbedPatterns
    public slice<@string> TestEmbedPatterns;              // patterns from TestGoFiles
    public map<@string, slice<tokenꓸPosition>> TestEmbedPatternPos; // line information for TestEmbedPatterns
    public slice<@string> XTestEmbedPatterns;              // patterns from XTestGoFiles
    public map<@string, slice<tokenꓸPosition>> XTestEmbedPatternPos; // line information for XTestEmbedPatternPos
}

// A Directive is a Go directive comment (//go:zzz...) found in a source file.
[GoType] partial struct Directive {
    public @string Text;        // full line comment including leading slashes
    public tokenꓸPosition Pos; // position of comment
}

// IsCommand reports whether the package is considered a
// command to be installed (not just a library).
// Packages named "main" are treated as commands.
[GoRecv] public static bool IsCommand(this ref Package p) {
    return p.Name == "main"u8;
}

// ImportDir is like [Import] but processes the Go package found in
// the named directory.
public static (ж<Package>, error) ImportDir(this ж<Context> Ꮡctxt, @string dir, ImportMode mode) {
    return Ꮡctxt.Import("."u8, dir, mode);
}

// NoGoError is the error used by [Import] to describe a directory
// containing no buildable Go source files. (It may still contain
// test files, files hidden by build tags, and so on.)
[GoType] partial struct NoGoError {
    public @string Dir;
}

[GoRecv] public static @string Error(this ref NoGoError e) {
    return "no buildable Go source files in "u8 + e.Dir;
}

// MultiplePackageError describes a directory containing
// multiple buildable Go source files for multiple packages.
[GoType] partial struct MultiplePackageError {
    public @string Dir;  // directory containing files
    public slice<@string> Packages; // package names found
    public slice<@string> Files; // corresponding files: Files[i] declares package Packages[i]
}

[GoRecv] public static @string Error(this ref MultiplePackageError e) {
    // Error string limited to two entries for compatibility.
    return fmt.Sprintf("found packages %s (%s) and %s (%s) in %s"u8, e.Packages[0], e.Files[0], e.Packages[1], e.Files[1], e.Dir);
}

internal static @string nameExt(@string name) {
    nint i = strings.LastIndex(name, "."u8);
    if (i < 0) {
        return ""u8;
    }
    return name.slice(i);
}

internal static ж<godebug.Setting> installgoroot = godebug.New("installgoroot"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "/testdata/"u8;
internal static readonly @string testdataˢ2 = "/testdata"u8;
internal static readonly @string testdataˢ3 = "testdata/"u8;
internal static readonly @string srcˢ2 = "src/"u8;
internal static readonly @string vendorˢ = "vendor"u8;
internal static readonly @string vendorˢ2 = "vendor/"u8;
internal static readonly @string sVendorTreeˢ = "\t%s (vendor tree)"u8;
internal static readonly @string sFromGopathˢ = "\t%s (from $GOPATH)"u8;
internal static readonly @string pkgˢ = "pkg"u8;
internal static readonly @string binˢ = "bin"u8;
internal static readonly @string testGoˢ = "_test.go"u8;
internal static readonly @string testˢ = "_test"u8;
internal static readonly @string cgoˢ = "cgo"u8;

[GoType("dyn")] internal partial struct Import_tried {
    internal slice<@string> vendor;
    internal @string goroot;
    internal slice<@string> gopath;
}

// Import returns details about the Go package named by the import path,
// interpreting local import paths relative to the srcDir directory.
// If the path is a local import path naming a package that can be imported
// using a standard import path, the returned package will set p.ImportPath
// to that path.
//
// In the directory containing the package, .go, .c, .h, and .s files are
// considered part of the package except for:
//
//   - .go files in package documentation
//   - files starting with _ or . (likely editor temporary files)
//   - files with build constraints not satisfied by the context
//
// If an error occurs, Import returns a non-nil error and a non-nil
// *[Package] containing partial information.
public static (ж<Package>, error) Import(this ж<Context> Ꮡctxt, @string path, @string srcDir, ImportMode mode) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();

    var p = Ꮡ(new Package(
        ImportPath: path
    ));
    if (path == ""u8) {
        return (p, fmt.Errorf("import %q: invalid import path"u8, path));
    }
    @string pkgtargetroot = default!;
    @string pkga = default!;
    error pkgerr = default!;
    @string suffix = ""u8;
    if (ctxt.InstallSuffix != ""u8) {
        suffix = "_"u8 + ctxt.InstallSuffix;
    }
    var exprᴛ1 = ctxt.Compiler;
    if (exprᴛ1 == "gccgo"u8) {
        pkgtargetroot = "pkg/gccgo_"u8 + ctxt.GOOS + "_"u8 + ctxt.GOARCH + suffix;
    }
    else if (exprᴛ1 == "gc"u8) {
        pkgtargetroot = "pkg/"u8 + ctxt.GOOS + "_"u8 + ctxt.GOARCH + suffix;
    }
    else { /* default: */
        pkgerr = fmt.Errorf("import %q: unknown compiler %q"u8, // Save error for end of function.
 path, ctxt.Compiler);
    }

    var pʗ1 = p;
    void setPkga() {
        var exprᴛ2 = Ꮡctxt.Value.Compiler;
        if (exprᴛ2 == "gccgo"u8) {
            var (dir, elem) = pathpkg.Split((~pʗ1).ImportPath);
            pkga = pkgtargetroot + "/"u8 + dir + "lib"u8 + elem + ".a"u8;
        }
        else if (exprᴛ2 == "gc"u8) {
            pkga = pkgtargetroot + "/"u8 + (~pʗ1).ImportPath + ".a"u8;
        }

    }
    setPkga();
    var binaryOnly = false;
    if (IsLocalImport(path)){
        pkga = ""u8; // local imports have no installed path
        if (srcDir == ""u8) {
            return (p, fmt.Errorf("import %q: import relative to unknown directory"u8, path));
        }
        if (!ctxt.isAbsPath(path)) {
            p.Value.Dir = ctxt.joinPath(srcDir, path);
        }
        // p.Dir directory may or may not exist. Gather partial information first, check if it exists later.
        // Determine canonical import path, if any.
        // Exclude results where the import path would include /testdata/.
        bool inTestdata(@string sub) => strings.Contains(sub, testdataˢ) || strings.HasSuffix(sub, testdataˢ2) || strings.HasPrefix(sub, testdataˢ3) || sub == "testdata"u8;
        if (ctxt.GOROOT != ""u8) {
            @string root = ctxt.joinPath(ctxt.GOROOT, srcˢ);
            {
                var (sub, ok) = ctxt.hasSubdir(root, (~p).Dir); if (ok && !inTestdata(sub)) {
                    p.Value.Goroot = true;
                    p.Value.ImportPath = sub;
                    p.Value.Root = ctxt.GOROOT;
                    setPkga(); // p.ImportPath changed
                    goto Found;
                }
            }
        }
        var all = ctxt.gopath();
        foreach (var (i, root) in all) {
            @string rootsrc = ctxt.joinPath(root, srcˢ);
            {
                var (sub, ok) = ctxt.hasSubdir(rootsrc, (~p).Dir); if (ok && !inTestdata(sub)) {
                    // We found a potential import path for dir,
                    // but check that using it wouldn't find something
                    // else first.
                    if (ctxt.GOROOT != ""u8 && ctxt.Compiler != "gccgo"u8) {
                        {
                            @string dir = ctxt.joinPath(ctxt.GOROOT, srcˢ, sub); if (ctxt.isDir(dir)) {
                                p.Value.ConflictDir = dir;
                                goto Found;
                            }
                        }
                    }
                    foreach (var (_, earlyRoot) in all.slice(0, i)) {
                        {
                            @string dir = ctxt.joinPath(earlyRoot, srcˢ, sub); if (ctxt.isDir(dir)) {
                                p.Value.ConflictDir = dir;
                                goto Found;
                            }
                        }
                    }
                    // sub would not name some other directory instead of this one.
                    // Record it.
                    p.Value.ImportPath = sub;
                    p.Value.Root = root;
                    setPkga(); // p.ImportPath changed
                    goto Found;
                }
            }
        }
    } else {
        // It's okay that we didn't find a root containing dir.
        // Keep going with the information we have.
        if (strings.HasPrefix(path, "/"u8)) {
            return (p, fmt.Errorf("import %q: cannot import absolute path"u8, path));
        }
        {
            var errΔ1 = ctxt.importGo(p, path, srcDir, mode); if (errΔ1 == default!){
                goto Found;
            } else 
            if (!AreEqual(errΔ1, errNoModules)) {
                return (p, errΔ1);
            }
        }
        var gopath = ctxt.gopath(); // needed twice below; avoid computing many times
        // tried records the location of unsuccessful package lookups
        ref var tried = ref heap(new Import_tried(), out var Ꮡtried);
        // Vendor directories get first chance to satisfy import.
        if ((ImportMode)(mode & IgnoreVendor) == 0 && srcDir != ""u8) {
            var pʗ2 = p;
            var setPkgaʗ1 = setPkga;
            bool searchVendor(@string root, bool isGoroot) {
                var (sub, ok) = Ꮡctxt.Value.hasSubdir(root, srcDir);
                if (!ok || !strings.HasPrefix(sub, srcˢ2) || strings.Contains(sub, testdataˢ)) {
                    return false;
                }
                while (ᐧ) {
                    @string vendor = Ꮡctxt.Value.joinPath(root, sub, vendorˢ);
                    if (Ꮡctxt.Value.isDir(vendor)) {
                        @string dir = Ꮡctxt.Value.joinPath(vendor, path);
                        if (Ꮡctxt.Value.isDir(dir) && hasGoFiles(Ꮡctxt, dir)) {
                            pʗ2.Value.Dir = dir;
                            pʗ2.Value.ImportPath = strings.TrimPrefix(pathpkg.Join(sub, vendorˢ, path), srcˢ2);
                            pʗ2.Value.Goroot = isGoroot;
                            pʗ2.Value.Root = root;
                            setPkgaʗ1(); // p.ImportPath changed
                            return true;
                        }
                        Ꮡtried.Value.vendor = append(Ꮡtried.Value.vendor, dir);
                    }
                    nint i = strings.LastIndex(sub, "/"u8);
                    if (i < 0) {
                        break;
                    }
                    sub = sub.slice(0, i);
                }
                return false;
            }
            if (ctxt.Compiler != "gccgo"u8 && ctxt.GOROOT != ""u8 && searchVendor(ctxt.GOROOT, true)) {
                goto Found;
            }
            foreach (var (_, root) in gopath) {
                if (searchVendor(root, false)) {
                    goto Found;
                }
            }
        }
        // Determine directory from import path.
        if (ctxt.GOROOT != ""u8) {
            // If the package path starts with "vendor/", only search GOROOT before
            // GOPATH if the importer is also within GOROOT. That way, if the user has
            // vendored in a package that is subsequently included in the standard
            // distribution, they'll continue to pick up their own vendored copy.
            var gorootFirst = srcDir == ""u8 || !strings.HasPrefix(path, vendorˢ2);
            if (!gorootFirst) {
                (_, gorootFirst) = ctxt.hasSubdir(ctxt.GOROOT, srcDir);
            }
            if (gorootFirst) {
                @string dir = ctxt.joinPath(ctxt.GOROOT, srcˢ, path);
                if (ctxt.Compiler != "gccgo"u8) {
                    var isDir = ctxt.isDir(dir);
                    binaryOnly = !isDir && (ImportMode)(mode & AllowBinary) != 0 && pkga != ""u8 && ctxt.isFile(ctxt.joinPath(ctxt.GOROOT, pkga));
                    if (isDir || binaryOnly) {
                        p.Value.Dir = dir;
                        p.Value.Goroot = true;
                        p.Value.Root = ctxt.GOROOT;
                        goto Found;
                    }
                }
                tried.goroot = dir;
            }
            if (ctxt.Compiler == "gccgo"u8 && goroot.IsStandardPackage(ctxt.GOROOT, ctxt.Compiler, path)) {
                // TODO(bcmills): Setting p.Dir here is misleading, because gccgo
                // doesn't actually load its standard-library packages from this
                // directory. See if we can leave it unset.
                p.Value.Dir = ctxt.joinPath(ctxt.GOROOT, srcˢ, path);
                p.Value.Goroot = true;
                p.Value.Root = ctxt.GOROOT;
                goto Found;
            }
        }
        foreach (var (_, root) in gopath) {
            @string dir = ctxt.joinPath(root, srcˢ, path);
            var isDir = ctxt.isDir(dir);
            binaryOnly = !isDir && (ImportMode)(mode & AllowBinary) != 0 && pkga != ""u8 && ctxt.isFile(ctxt.joinPath(root, pkga));
            if (isDir || binaryOnly) {
                p.Value.Dir = dir;
                p.Value.Root = root;
                goto Found;
            }
            tried.gopath = append(tried.gopath, dir);
        }
        // If we tried GOPATH first due to a "vendor/" prefix, fall back to GOPATH.
        // That way, the user can still get useful results from 'go list' for
        // standard-vendored paths passed on the command line.
        if (ctxt.GOROOT != ""u8 && tried.goroot == ""u8) {
            @string dir = ctxt.joinPath(ctxt.GOROOT, srcˢ, path);
            if (ctxt.Compiler != "gccgo"u8) {
                var isDir = ctxt.isDir(dir);
                binaryOnly = !isDir && (ImportMode)(mode & AllowBinary) != 0 && pkga != ""u8 && ctxt.isFile(ctxt.joinPath(ctxt.GOROOT, pkga));
                if (isDir || binaryOnly) {
                    p.Value.Dir = dir;
                    p.Value.Goroot = true;
                    p.Value.Root = ctxt.GOROOT;
                    goto Found;
                }
            }
            tried.goroot = dir;
        }
        // package was not found
        slice<@string> paths = default!;
        @string format = sVendorTreeˢ;
        foreach (var (_, dir) in tried.vendor) {
            paths = append(paths, fmt.Sprintf(format, dir));
            format = "\t%s"u8;
        }
        if (tried.goroot != ""u8){
            paths = append(paths, fmt.Sprintf("\t%s (from $GOROOT)"u8, tried.goroot));
        } else {
            paths = append(paths, "\t($GOROOT not set)"u8);
        }
        format = sFromGopathˢ;
        foreach (var (_, dir) in tried.gopath) {
            paths = append(paths, fmt.Sprintf(format, dir));
            format = "\t%s"u8;
        }
        if (len(tried.gopath) == 0) {
            paths = append(paths, "\t($GOPATH not set. For more details see: 'go help gopath')"u8);
        }
        return (p, fmt.Errorf("cannot find package %q in any of:\n%s"u8, path, strings.Join(paths, "\n"u8)));
    }
Found:
    if ((~p).Root != ""u8) {
        p.Value.SrcRoot = ctxt.joinPath((~p).Root, srcˢ);
        p.Value.PkgRoot = ctxt.joinPath((~p).Root, pkgˢ);
        p.Value.BinDir = ctxt.joinPath((~p).Root, binˢ);
        if (pkga != ""u8) {
            // Always set PkgTargetRoot. It might be used when building in shared
            // mode.
            p.Value.PkgTargetRoot = ctxt.joinPath((~p).Root, pkgtargetroot);
            // Set the install target if applicable.
            if (!(~p).Goroot || (installgoroot.Value() == "all"u8 && (~p).ImportPath != "unsafe"u8 && (~p).ImportPath != "builtin"u8)) {
                if ((~p).Goroot) {
                    installgoroot.IncNonDefault();
                }
                p.Value.PkgObj = ctxt.joinPath((~p).Root, pkga);
            }
        }
    }
    // If it's a local import path, by the time we get here, we still haven't checked
    // that p.Dir directory exists. This is the right time to do that check.
    // We can't do it earlier, because we want to gather partial information for the
    // non-nil *Package returned when an error occurs.
    // We need to do this before we return early on FindOnly flag.
    if (IsLocalImport(path) && !ctxt.isDir((~p).Dir)) {
        if (ctxt.Compiler == "gccgo"u8 && (~p).Goroot) {
            // gccgo has no sources for GOROOT packages.
            return (p, default!);
        }
        // package was not found
        return (p, fmt.Errorf("cannot find package %q in:\n\t%s"u8, (~p).ImportPath, (~p).Dir));
    }
    if ((ImportMode)(mode & FindOnly) != 0) {
        return (p, pkgerr);
    }
    if (binaryOnly && ((ImportMode)(mode & AllowBinary)) != 0) {
        return (p, pkgerr);
    }
    if (ctxt.Compiler == "gccgo"u8 && (~p).Goroot) {
        // gccgo has no sources for GOROOT packages.
        return (p, default!);
    }
    var (dirs, err) = ctxt.readDir((~p).Dir);
    if (err != default!) {
        return (p, err);
    }
    ref var badGoError = ref heap<error>(out var ᏑbadGoError);
    var badGoFiles = new map<@string, bool>();
    var badGoFilesʗ1 = badGoFiles;
    var pʗ3 = p;
    void badGoFile(@string name, error errΔ2) {
        if (ᏑbadGoError.ValueSlot == default!) {
            ᏑbadGoError.ValueSlot = errΔ2;
        }
        if (!badGoFilesʗ1[name]) {
            pʗ3.Value.InvalidGoFiles = append((~pʗ3).InvalidGoFiles, name);
            badGoFilesʗ1[name] = true;
        }
    }
    slice<@string> Sfiles = default!;            // files with ".S"(capital S)/.sx(capital s equivalent for case insensitive filesystems)
    @string firstFile = default!;
    @string firstCommentFile = default!;
    var embedPos = new map<@string, slice<tokenꓸPosition>>();
    var testEmbedPos = new map<@string, slice<tokenꓸPosition>>();
    var xTestEmbedPos = new map<@string, slice<tokenꓸPosition>>();
    var importPos = new map<@string, slice<tokenꓸPosition>>();
    var testImportPos = new map<@string, slice<tokenꓸPosition>>();
    var xTestImportPos = new map<@string, slice<tokenꓸPosition>>();
    var allTags = new map<@string, bool>();
    var fset = token.NewFileSet();
    foreach (var (_, d) in dirs) {
        if (d.IsDir()) {
            continue;
        }
        if (d.Type() == fs.ModeSymlink) {
            if (ctxt.isDir(ctxt.joinPath((~p).Dir, d.Name()))) {
                // Symlinks to directories are not source files.
                continue;
            }
        }
        @string name = d.Name();
        @string ext = nameExt(name);
        var (info, errΔ3) = Ꮡctxt.matchFile((~p).Dir, name, allTags, p.of(Package.ᏑBinaryOnly), fset);
        if (errΔ3 != default! && strings.HasSuffix(name, ".go"u8)) {
            badGoFile(name, errΔ3);
            continue;
        }
        if (info == nil) {
            if (strings.HasPrefix(name, "_"u8) || strings.HasPrefix(name, "."u8)){
            } else 
            if (ext == ".go"u8){
                // not due to build constraints - don't report
                p.Value.IgnoredGoFiles = append((~p).IgnoredGoFiles, name);
            } else 
            if (fileListForExt(p, ext) != nil) {
                p.Value.IgnoredOtherFiles = append((~p).IgnoredOtherFiles, name);
            }
            continue;
        }
        // Going to save the file. For non-Go files, can stop here.
        var exprᴛ3 = ext;
        if (exprᴛ3 == ".go"u8) {
        }
        else if (exprᴛ3 == ".S"u8 || exprᴛ3 == ".sx"u8) {
            Sfiles = append(Sfiles, // keep going
 // special case for cgo, handled at end
 name);
            continue;
        }
        else { /* default: */
            {
                var list = fileListForExt(p, ext); if (list != nil) {
                    list.ValueSlot = append(list.ValueSlot, name);
                }
            }
            continue;
        }

        var data = info.Value.header;
        @string filename = info.Value.name;
        if ((~info).parseErr != default!) {
            badGoFile(name, (~info).parseErr);
        }
        // Fall through: we might still have a partial AST in info.parsed,
        // and we want to list files with parse errors anyway.
        @string pkg = default!;
        if ((~info).parsed != nil) {
            pkg = info.Value.parsed.Value.Name.Value.Name;
            if (pkg == "documentation"u8) {
                p.Value.IgnoredGoFiles = append((~p).IgnoredGoFiles, name);
                continue;
            }
        }
        var isTest = strings.HasSuffix(name, testGoˢ);
        var isXTest = false;
        if (isTest && strings.HasSuffix(pkg, testˢ) && (~p).Name != pkg) {
            isXTest = true;
            pkg = pkg.slice(0, len(pkg) - len("_test"));
        }
        if ((~p).Name == ""u8){
            p.Value.Name = pkg;
            firstFile = name;
        } else 
        if (pkg != (~p).Name) {
            // TODO(#45999): The choice of p.Name is arbitrary based on file iteration
            // order. Instead of resolving p.Name arbitrarily, we should clear out the
            // existing name and mark the existing files as also invalid.
            badGoFile(name, new MultiplePackageErrorжerror(Ꮡ(new MultiplePackageError(
                Dir: (~p).Dir,
                Packages: new @string[]{(~p).Name, pkg}.slice(),
                Files: new @string[]{firstFile, name}.slice()
            ))));
        }
        // Grab the first package comment as docs, provided it is not from a test file.
        if ((~info).parsed != nil && (~(~info).parsed).Doc != nil && (~p).Doc == ""u8 && !isTest && !isXTest) {
            p.Value.Doc = Δdoc.Synopsis((~(~info).parsed).Doc.Text());
        }
        if ((ImportMode)(mode & ImportComment) != 0) {
            var (qcom, line) = findImportComment(data);
            if (line != 0) {
                var (com, errΔ4) = strconv.Unquote(qcom);
                if (errΔ4 != default!){
                    badGoFile(name, fmt.Errorf("%s:%d: cannot parse import comment"u8, filename, line));
                } else 
                if ((~p).ImportComment == ""u8){
                    p.Value.ImportComment = com;
                    firstCommentFile = name;
                } else 
                if ((~p).ImportComment != com) {
                    badGoFile(name, fmt.Errorf("found import comments %q (%s) and %q (%s) in %s"u8, (~p).ImportComment, firstCommentFile, com, name, (~p).Dir));
                }
            }
        }
        // Record imports and information about cgo.
        var isCgo = false;
        foreach (var (_, imp) in (~info).imports) {
            if (imp.path == "C"u8) {
                if (isTest) {
                    badGoFile(name, fmt.Errorf("use of cgo in test %s not supported"u8, filename));
                    continue;
                }
                isCgo = true;
                if (imp.doc != nil) {
                    {
                        var errΔ5 = Ꮡctxt.saveCgo(filename, p, imp.doc); if (errΔ5 != default!) {
                            badGoFile(name, errΔ5);
                        }
                    }
                }
            }
        }
        ж<slice<@string>> fileList = default!;
        map<@string, slice<tokenꓸPosition>> importMap = default!;
        map<@string, slice<tokenꓸPosition>> embedMap = default!;
        ж<slice<Directive>> directives = default!;
        switch (ᐧ) {
        case {} when isCgo: {
            allTags[cgoˢ] = true;
            if (ctxt.CgoEnabled){
                fileList = p.of(Package.ᏑCgoFiles);
                importMap = importPos;
                embedMap = embedPos;
                directives = p.of(Package.ᏑDirectives);
            } else {
                // Ignore imports and embeds from cgo files if cgo is disabled.
                fileList = p.of(Package.ᏑIgnoredGoFiles);
            }
            break;
        }
        case {} when isXTest: {
            fileList = p.of(Package.ᏑXTestGoFiles);
            importMap = xTestImportPos;
            embedMap = xTestEmbedPos;
            directives = p.of(Package.ᏑXTestDirectives);
            break;
        }
        case {} when isTest: {
            fileList = p.of(Package.ᏑTestGoFiles);
            importMap = testImportPos;
            embedMap = testEmbedPos;
            directives = p.of(Package.ᏑTestDirectives);
            break;
        }
        default: {
            fileList = p.of(Package.ᏑGoFiles);
            importMap = importPos;
            embedMap = embedPos;
            directives = p.of(Package.ᏑDirectives);
            break;
        }}

        fileList.ValueSlot = append(fileList.ValueSlot, name);
        if (importMap != default!) {
            foreach (var (_, imp) in (~info).imports) {
                importMap[imp.path] = append(importMap[imp.path], fset.Position(imp.pos));
            }
        }
        if (embedMap != default!) {
            foreach (var (_, emb) in (~info).embeds) {
                embedMap[emb.pattern] = append(embedMap[emb.pattern], emb.pos);
            }
        }
        if (directives != nil) {
            directives.ValueSlot = appendꓸꓸꓸ(directives.ValueSlot, (~info).directives);
        }
    }
    foreach (var (tag, _) in allTags) {
        p.Value.AllTags = append((~p).AllTags, tag);
    }
    slices.Sort<slice<@string>, @string>((~p).AllTags);
    (p.Value.EmbedPatterns, p.Value.EmbedPatternPos) = cleanDecls(embedPos);
    (p.Value.TestEmbedPatterns, p.Value.TestEmbedPatternPos) = cleanDecls(testEmbedPos);
    (p.Value.XTestEmbedPatterns, p.Value.XTestEmbedPatternPos) = cleanDecls(xTestEmbedPos);
    (p.Value.Imports, p.Value.ImportPos) = cleanDecls(importPos);
    (p.Value.TestImports, p.Value.TestImportPos) = cleanDecls(testImportPos);
    (p.Value.XTestImports, p.Value.XTestImportPos) = cleanDecls(xTestImportPos);
    // add the .S/.sx files only if we are using cgo
    // (which means gcc will compile them).
    // The standard assemblers expect .s files.
    if (len((~p).CgoFiles) > 0){
        p.Value.SFiles = appendꓸꓸꓸ((~p).SFiles, Sfiles);
        slices.Sort<slice<@string>, @string>((~p).SFiles);
    } else {
        p.Value.IgnoredOtherFiles = appendꓸꓸꓸ((~p).IgnoredOtherFiles, Sfiles);
        slices.Sort<slice<@string>, @string>((~p).IgnoredOtherFiles);
    }
    if (badGoError != default!) {
        return (p, badGoError);
    }
    if (len((~p).GoFiles) + len((~p).CgoFiles) + len((~p).TestGoFiles) + len((~p).XTestGoFiles) == 0) {
        return (p, new NoGoErrorжerror(Ꮡ(new NoGoError((~p).Dir))));
    }
    return (p, pkgerr);
}

internal static ж<slice<@string>> fileListForExt(ж<Package> Ꮡp, @string ext) {
    var exprᴛ1 = ext;
    if (exprᴛ1 == ".c"u8) {
        return Ꮡp.of(Package.ᏑCFiles);
    }
    if (exprᴛ1 == ".cc"u8 || exprᴛ1 == ".cpp"u8 || exprᴛ1 == ".cxx"u8) {
        return Ꮡp.of(Package.ᏑCXXFiles);
    }
    if (exprᴛ1 == ".m"u8) {
        return Ꮡp.of(Package.ᏑMFiles);
    }
    if (exprᴛ1 == ".h"u8 || exprᴛ1 == ".hh"u8 || exprᴛ1 == ".hpp"u8 || exprᴛ1 == ".hxx"u8) {
        return Ꮡp.of(Package.ᏑHFiles);
    }
    if (exprᴛ1 == ".f"u8 || exprᴛ1 == ".F"u8 || exprᴛ1 == ".for"u8 || exprᴛ1 == ".f90"u8) {
        return Ꮡp.of(Package.ᏑFFiles);
    }
    if (exprᴛ1 == ".s"u8 || exprᴛ1 == ".S"u8 || exprᴛ1 == ".sx"u8) {
        return Ꮡp.of(Package.ᏑSFiles);
    }
    if (exprᴛ1 == ".swig"u8) {
        return Ꮡp.of(Package.ᏑSwigFiles);
    }
    if (exprᴛ1 == ".swigcxx"u8) {
        return Ꮡp.of(Package.ᏑSwigCXXFiles);
    }
    if (exprᴛ1 == ".syso"u8) {
        return Ꮡp.of(Package.ᏑSysoFiles);
    }

    return default!;
}

internal static slice<@string> uniq(slice<@string> list) {
    if (list == default!) {
        return default!;
    }
    var @out = new slice<@string>(len(list));
    copy(@out, list);
    slices.Sort<slice<@string>, @string>(@out);
    var uniq = @out[..0];
    foreach (var (_, x) in @out) {
        if (len(uniq) == 0 || uniq[len(uniq) - 1] != x) {
            uniq = append(uniq, x);
        }
    }
    return uniq;
}

internal static error errNoModules = errors.New("not using modules"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string go111moduleˢ = "GO111MODULE"u8;
internal static readonly @string goModˢ = "go.mod"u8;
internal static readonly @string listˢ = "list"u8;
internal static readonly @string fDirImportPathRootGorootˢ = "-f={{.Dir}}\n{{.ImportPath}}\n{{.Root}}\n{{.Goroot}}\n{{if .Error}}{{.Error}}{{end}}\n"u8;

// importGo checks whether it can use the go command to find the directory for path.
// If using the go command is not appropriate, importGo returns errNoModules.
// Otherwise, importGo tries using the go command and reports whether that succeeded.
// Using the go command lets build.Import and build.Context.Import find code
// in Go modules. In the long term we want tools to use go/packages (currently golang.org/x/tools/go/packages),
// which will also use the go command.
// Invoking the go command here is not very efficient in that it computes information
// about the requested package and all dependencies and then only reports about the requested package.
// Then we reinvoke it for every dependency. But this is still better than not working at all.
// See golang.org/issue/26504.
[GoRecv] internal static error importGo(this ref Context ctxt, ж<Package> Ꮡp, @string path, @string srcDir, ImportMode mode) {
    ref var p = ref Ꮡp.DerefOrNull();

    // To invoke the go command,
    // we must not being doing special things like AllowBinary or IgnoreVendor,
    // and all the file system callbacks must be nil (we're meant to use the local file system).
    if ((ImportMode)(mode & AllowBinary) != 0 || (ImportMode)(mode & IgnoreVendor) != 0 || ctxt.JoinPath != default! || ctxt.SplitPathList != default! || ctxt.IsAbsPath != default! || ctxt.IsDir != default! || ctxt.HasSubdir != default! || ctxt.ReadDir != default! || ctxt.OpenFile != default! || !equal(ctxt.ToolTags, defaultToolTags) || !equal(ctxt.ReleaseTags, defaultReleaseTags)) {
        return errNoModules;
    }
    // If ctxt.GOROOT is not set, we don't know which go command to invoke,
    // and even if we did we might return packages in GOROOT that we wouldn't otherwise find
    // (because we don't know to search in 'go env GOROOT' otherwise).
    if (ctxt.GOROOT == ""u8) {
        return errNoModules;
    }
    // Predict whether module aware mode is enabled by checking the value of
    // GO111MODULE and looking for a go.mod file in the source directory or
    // one of its parents. Running 'go env GOMOD' in the source directory would
    // give a canonical answer, but we'd prefer not to execute another command.
    @string go111Module = os.Getenv(go111moduleˢ);
    var exprᴛ1 = go111Module;
    if (exprᴛ1 == "off"u8) {
        return errNoModules;
    }
    { /* default: */
    }

    // "", "on", "auto", anything else
    // Maybe use modules.
    if (srcDir != ""u8) {
        @string absSrcDir = default!;
        if (filepath.IsAbs(srcDir)){
            absSrcDir = srcDir;
        } else 
        if (ctxt.Dir != ""u8){
            return fmt.Errorf("go/build: Dir is non-empty, so relative srcDir is not allowed: %v"u8, srcDir);
        } else {
            // Find the absolute source directory. hasSubdir does not handle
            // relative paths (and can't because the callbacks don't support this).
            error err = default!;
            (absSrcDir, err) = filepath.Abs(srcDir);
            if (err != default!) {
                return errNoModules;
            }
        }
        // If the source directory is in GOROOT, then the in-process code works fine
        // and we should keep using it. Moreover, the 'go list' approach below doesn't
        // take standard-library vendoring into account and will fail.
        {
            var (_, ok) = ctxt.hasSubdir(filepath.Join(ctxt.GOROOT, srcˢ), absSrcDir); if (ok) {
                return errNoModules;
            }
        }
    }
    // For efficiency, if path is a standard library package, let the usual lookup code handle it.
    {
        @string dirΔ1 = ctxt.joinPath(ctxt.GOROOT, srcˢ, path); if (ctxt.isDir(dirΔ1)) {
            return errNoModules;
        }
    }
    // If GO111MODULE=auto, look to see if there is a go.mod.
    // Since go1.13, it doesn't matter if we're inside GOPATH.
    if (go111Module == "auto"u8) {
        @string parent = default!;
        error err = default!;
        if (ctxt.Dir == ""u8){
            (parent, err) = os.Getwd();
            if (err != default!) {
                // A nonexistent working directory can't be in a module.
                return errNoModules;
            }
        } else {
            (parent, err) = filepath.Abs(ctxt.Dir);
            if (err != default!) {
                // If the caller passed a bogus Dir explicitly, that's materially
                // different from not having modules enabled.
                return err;
            }
        }
        while (ᐧ) {
            {
                var (fΔ1, errΔ1) = ctxt.openFile(ctxt.joinPath(parent, goModˢ)); if (errΔ1 == default!) {
                    var buf = new slice<byte>(100);
                    var (_, errΔ2) = fΔ1.Read(buf);
                    fΔ1.Close();
                    if (errΔ2 == default! || AreEqual(errΔ2, io.EOF)) {
                        // go.mod exists and is readable (is a file, not a directory).
                        break;
                    }
                }
            }
            @string d = filepath.Dir(parent);
            if (len(d) >= len(parent)) {
                return errNoModules; // reached top of file system, no go.mod
            }
            parent = d;
        }
    }
    @string goCmd = filepath.Join(ctxt.GOROOT, binˢ, "go");
    var cmd = exec.Command(goCmd, listˢ, "-e", "-compiler=" + ctxt.Compiler, "-tags=" + strings.Join(ctxt.BuildTags, ","u8), "-installsuffix=" + ctxt.InstallSuffix, fDirImportPathRootGorootˢ, "--", path);
    if (ctxt.Dir != ""u8) {
        cmd.Value.Dir = ctxt.Dir;
    }
    ref var stdout = ref heap(new strings.Builder(), out var Ꮡstdout);
    ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
    cmd.Value.Stdout = new strings_BuilderжWriter(Ꮡstdout);
    cmd.Value.Stderr = new strings_BuilderжWriter(Ꮡstderr);
    @string cgo = "0"u8;
    if (ctxt.CgoEnabled) {
        cgo = "1"u8;
    }
    cmd.Value.Env = append(cmd.Environ(),
        "GOOS="u8 + ctxt.GOOS,
        "GOARCH=" + ctxt.GOARCH,
        "GOROOT=" + ctxt.GOROOT,
        "GOPATH=" + ctxt.GOPATH,
        "CGO_ENABLED=" + cgo);
    {
        var err = cmd.Run(); if (err != default!) {
            return fmt.Errorf("go/build: go list %s: %v\n%s\n"u8, path, err, stderr.String());
        }
    }
    var f = strings.SplitN(stdout.String(), "\n"u8, 5);
    if (len(f) != 5) {
        return fmt.Errorf("go/build: importGo %s: unexpected output:\n%s\n"u8, path, stdout.String());
    }
    @string dir = f[0];
    @string errStr = strings.TrimSpace(f[4]);
    if (errStr != ""u8 && dir == ""u8) {
        // If 'go list' could not locate the package (dir is empty),
        // return the same error that 'go list' reported.
        return errors.New(errStr);
    }
    // If 'go list' did locate the package, ignore the error.
    // It was probably related to loading source files, and we'll
    // encounter it ourselves shortly if the FindOnly flag isn't set.
    p.Dir = dir;
    p.ImportPath = f[1];
    p.Root = f[2];
    p.Goroot = f[3] == "true";
    return default!;
}

internal static bool equal(slice<@string> x, slice<@string> y) {
    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, xi) in x) {
        if (xi != y[i]) {
            return false;
        }
    }
    return true;
}

// hasGoFiles reports whether dir contains any files with names ending in .go.
// For a vendor check we must exclude directories that contain no .go files.
// Otherwise it is not possible to vendor just a/b/c and still import the
// non-vendored a/b. See golang.org/issue/13832.
internal static bool hasGoFiles(ж<Context> Ꮡctxt, @string dir) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();

    var (ents, _) = ctxt.readDir(dir);
    foreach (var (_, ent) in ents) {
        if (!ent.IsDir() && strings.HasSuffix(ent.Name(), ".go"u8)) {
            return true;
        }
    }
    return false;
}

internal static (@string s, nint line) findImportComment(slice<byte> data) {
    nint line = default!;

    // expect keyword package
    (var word, data) = parseWord(data);
    if (((sstring)word) != "package"u8) {
        return ("", 0);
    }
    // expect package name
    (_, data) = parseWord(data);
    // now ready for import comment, a // or /* */ comment
    // beginning and ending on the current line.
    while (len(data) > 0 && (data[0] == (rune)' ' || data[0] == (rune)'\t' || data[0] == (rune)'\r')) {
        data = data[1..];
    }
    slice<byte> comment = default!;
    switch (ᐧ) {
    case {} when bytes.HasPrefix(data, slashSlash): {
        (comment, _, _) = bytes.Cut(data[2..], newline);
        break;
    }
    case {} when bytes.HasPrefix(data, slashStar): {
        bool ok = default!;
        (comment, _, ok) = bytes.Cut(data[2..], starSlash);
        if (!ok) {
            // malformed comment
            return ("", 0);
        }
        if (bytes.Contains(comment, newline)) {
            return ("", 0);
        }
        break;
    }}

    comment = bytes.TrimSpace(comment);
    // split comment into `import`, `"pkg"`
    (word, var arg) = parseWord(comment);
    if (((sstring)word) != "import"u8) {
        return ("", 0);
    }
    line = 1 + bytes.Count(data.slice(0, cap(data) - cap(arg)), newline);
    return (strings.TrimSpace(((@string)arg)), line);
}

internal static slice<byte> slashSlash = slice<byte>("//"u8);
internal static slice<byte> slashStar = slice<byte>("/*"u8);
internal static slice<byte> starSlash = slice<byte>("*/"u8);
internal static slice<byte> newline = slice<byte>("\n"u8);

// skipSpaceOrComment returns data with any leading spaces or comments removed.
internal static slice<byte> skipSpaceOrComment(slice<byte> data) {
    while (len(data) > 0) {
        switch (data[0]) {
        case (rune)' ' or (rune)'\t' or (rune)'\r' or (rune)'\n': {
            data = data[1..];
            continue;
            break;
        }
        case (rune)'/': {
            if (bytes.HasPrefix(data, slashSlash)) {
                nint i = bytes.Index(data, newline);
                if (i < 0) {
                    return default!;
                }
                data = data.slice(i + 1);
                continue;
            }
            if (bytes.HasPrefix(data, slashStar)) {
                data = data[2..];
                nint i = bytes.Index(data, starSlash);
                if (i < 0) {
                    return default!;
                }
                data = data.slice(i + 2);
                continue;
            }
            break;
        }}

        break;
    }
    return data;
}

// parseWord skips any leading spaces or comments in data
// and then parses the beginning of data as an identifier or keyword,
// returning that word and what remains after the word.
internal static (slice<byte> word, slice<byte> rest) parseWord(slice<byte> data) {
    slice<byte> word = default!;
    slice<byte> rest = default!;

    data = skipSpaceOrComment(data);
    // Parse past leading word characters.
    rest = data;
    while (ᐧ) {
        var (r, size) = utf8.DecodeRune(rest);
        if (unicode.IsLetter(r) || (rune)'0' <= r && r <= (rune)'9' || r == (rune)'_') {
            rest = rest.slice(size);
            continue;
        }
        break;
    }
    word = data.slice(0, len(data) - len(rest));
    if (len(word) == 0) {
        return (default!, default!);
    }
    return (word, rest);
}

// MatchFile reports whether the file with the given name in the given directory
// matches the context and would be included in a [Package] created by [ImportDir]
// of that directory.
//
// MatchFile considers the name of the file and may use ctxt.OpenFile to
// read some or all of the file's content.
public static (bool match, error err) MatchFile(this ж<Context> Ꮡctxt, @string dir, @string name) {
    error err = default!;

    (var info, err) = Ꮡctxt.matchFile(dir, name, default!, nil, nil);
    return (info != nil, err);
}

internal static ж<Package> ᏑdummyPkg = new StandardBox<Package>(default(Package));
internal static ref Package dummyPkg => ref ᏑdummyPkg.Value;

// fileInfo records information learned about a file included in a build.
[GoType] partial struct fileInfo {
    internal @string name; // full name including dir
    internal slice<byte> header;
    internal ж<token.FileSet> fset;
    internal ж<ast.File> parsed;
    internal error parseErr;
    internal slice<fileImport> imports;
    internal slice<fileEmbed> embeds;
    internal slice<Directive> directives;
}

[GoType] partial struct fileImport {
    internal @string path;
    internal tokenꓸPos pos;
    internal ж<ast.CommentGroup> doc;
}

[GoType] partial struct fileEmbed {
    internal @string pattern;
    internal tokenꓸPosition pos;
}

// matchFile determines whether the file with the given name in the given directory
// should be included in the package being constructed.
// If the file should be included, matchFile returns a non-nil *fileInfo (and a nil error).
// Non-nil errors are reserved for unexpected problems.
//
// If name denotes a Go program, matchFile reads until the end of the
// imports and returns that section of the file in the fileInfo's header field,
// even though it only considers text until the first non-comment
// for go:build lines.
//
// If allTags is non-nil, matchFile records any encountered build tag
// by setting allTags[tag] = true.
internal static (ж<fileInfo>, error) matchFile(this ж<Context> Ꮡctxt, @string dir, @string name, map<@string, bool> allTags, ж<bool> ᏑbinaryOnly, ж<token.FileSet> Ꮡfset) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();
    ref var binaryOnly = ref ᏑbinaryOnly.DerefOrNull();

    if (strings.HasPrefix(name, "_"u8) || strings.HasPrefix(name, "."u8)) {
        return (default!, default!);
    }
    nint i = strings.LastIndex(name, "."u8);
    if (i < 0) {
        i = len(name);
    }
    @string ext = name.slice(i);
    if (ext != ".go"u8 && fileListForExt(ᏑdummyPkg, ext) == nil) {
        // skip
        return (default!, default!);
    }
    if (!ctxt.goodOSArchFile(name, allTags) && !ctxt.UseAllFiles) {
        return (default!, default!);
    }
    var info = Ꮡ(new fileInfo(name: ctxt.joinPath(dir, name), fset: Ꮡfset));
    if (ext == ".syso"u8) {
        // binary, no reading
        return (info, default!);
    }
    var (f, err) = ctxt.openFile((~info).name);
    if (err != default!) {
        return (default!, err);
    }
    if (strings.HasSuffix(name, ".go"u8)){
        err = readGoInfo(f, ref (info).DerefOrNull());
        if (strings.HasSuffix(name, testGoˢ)) {
            ᏑbinaryOnly = default!; binaryOnly = ref ᏑbinaryOnly.DerefOrNull(); // ignore //go:binary-only-package comments in _test.go files
        }
    } else {
        ᏑbinaryOnly = default!; binaryOnly = ref ᏑbinaryOnly.DerefOrNull(); // ignore //go:binary-only-package comments in non-Go sources
        (info.Value.header, err) = readComments(f);
    }
    f.Close();
    if (err != default!) {
        return (info, fmt.Errorf("read %s: %v"u8, (~info).name, err));
    }
    // Look for go:build comments to accept or reject the file.
    (var ok, var sawBinaryOnly, err) = Ꮡctxt.shouldBuild((~info).header, allTags);
    if (err != default!) {
        return (default!, fmt.Errorf("%s: %v"u8, name, err));
    }
    if (!ok && !ctxt.UseAllFiles) {
        return (default!, default!);
    }
    if (ᏑbinaryOnly != nil && sawBinaryOnly) {
        binaryOnly = true;
    }
    return (info, default!);
}

internal static (slice<@string>, map<@string, slice<tokenꓸPosition>>) cleanDecls(map<@string, slice<tokenꓸPosition>> m) {
    var all = new slice<@string>(0, len(m));
    foreach (var (path, _) in m) {
        all = append(all, path);
    }
    slices.Sort<slice<@string>, @string>(all);
    return (all, m);
}

// Import is shorthand for Default.Import.
public static (ж<Package>, error) Import(@string path, @string srcDir, ImportMode mode) {
    return ᏑDefault.Import(path, srcDir, mode);
}

// ImportDir is shorthand for Default.ImportDir.
public static (ж<Package>, error) ImportDir(@string dir, ImportMode mode) {
    return ᏑDefault.ImportDir(dir, mode);
}

internal static slice<byte> plusBuild = slice<byte>("+build"u8);
internal static slice<byte> goBuildComment = slice<byte>("//go:build"u8);
internal static error errMultipleGoBuild = errors.New("multiple //go:build comments"u8);

internal static bool isGoBuildComment(slice<byte> line) {
    if (!bytes.HasPrefix(line, goBuildComment)) {
        return false;
    }
    line = bytes.TrimSpace(line);
    var rest = line.slice(len(goBuildComment));
    return len(rest) == 0 || len(bytes.TrimSpace(rest)) < len(rest);
}

// Special comment denoting a binary-only package.
// See https://golang.org/design/2775-binary-only-packages
// for more about the design of binary-only packages.
internal static slice<byte> binaryOnlyComment = slice<byte>("//go:binary-only-package"u8);

// shouldBuild reports whether it is okay to use this file,
// The rule is that in the file's leading run of // comments
// and blank lines, which must be followed by a blank line
// (to avoid including a Go package clause doc comment),
// lines beginning with '//go:build' are taken as build directives.
//
// The file is accepted only if each such line lists something
// matching the file. For example:
//
//	//go:build windows linux
//
// marks the file as applicable only on Windows and Linux.
//
// For each build tag it consults, shouldBuild sets allTags[tag] = true.
//
// shouldBuild reports whether the file should be built
// and whether a //go:binary-only-package comment was found.
internal static (bool shouldBuild, bool binaryOnly, error err) shouldBuild(this ж<Context> Ꮡctxt, slice<byte> content, map<@string, bool> allTags) {
    bool shouldBuild = default!;
    error err = default!;

    // Identify leading run of // comments and blank lines,
    // which must be followed by a blank line.
    // Also identify any //go:build comments.
    (content, var goBuild, var sawBinaryOnly, err) = parseFileHeader(content);
    if (err != default!) {
        return (false, false, err);
    }
    // If //go:build line is present, it controls.
    // Otherwise fall back to +build processing.
    switch (ᐧ) {
    case {} when goBuild != default!: {
        var (x, errΔ3) = constraint.Parse(((@string)goBuild));
        if (errΔ3 != default!) {
            return (false, false, fmt.Errorf("parsing //go:build line: %v"u8, errΔ3));
        }
        shouldBuild = Ꮡctxt.eval(x, allTags);
        break;
    }
    default: {
        shouldBuild = true;
        var p = content;
        while (len(p) > 0) {
            var line = p;
            {
                nint i = bytes.IndexByte(line, (rune)'\n'); if (i >= 0){
                    (line, p) = (line.slice(0, i), p.slice(i + 1));
                } else {
                    p = p.slice(len(p));
                }
            }
            line = bytes.TrimSpace(line);
            if (!bytes.HasPrefix(line, slashSlash) || !bytes.Contains(line, plusBuild)) {
                continue;
            }
            @string text = ((@string)line);
            if (!constraint.IsPlusBuild(text)) {
                continue;
            }
            {
                var (x, errΔ4) = constraint.Parse(text); if (errΔ4 == default!) {
                    if (!Ꮡctxt.eval(x, allTags)) {
                        shouldBuild = false;
                    }
                }
            }
        }
        break;
    }}

    return (shouldBuild, sawBinaryOnly, default!);
}

// parseFileHeader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bazelbuild/bazel-gazelle
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname parseFileHeader
internal static (slice<byte> trimmed, slice<byte> goBuild, bool sawBinaryOnly, error err) parseFileHeader(slice<byte> content) {
    slice<byte> goBuild = default!;
    bool sawBinaryOnly = default!;

    nint end = 0;
    var p = content;
    var ended = false; // found non-blank, non-// line, so stopped accepting //go:build lines
    var inSlashStar = false; // in /* */ comment
Lines:
    while (len(p) > 0) {
        var line = p;
        {
            nint i = bytes.IndexByte(line, (rune)'\n'); if (i >= 0){
                (line, p) = (line.slice(0, i), p.slice(i + 1));
            } else {
                p = p.slice(len(p));
            }
        }
        line = bytes.TrimSpace(line);
        if (len(line) == 0 && !ended) {
            // Blank line
            // Remember position of most recent blank line.
            // When we find the first non-blank, non-// line,
            // this "end" position marks the latest file position
            // where a //go:build line can appear.
            // (It must appear _before_ a blank line before the non-blank, non-// line.
            // Yes, that's confusing, which is part of why we moved to //go:build lines.)
            // Note that ended==false here means that inSlashStar==false,
            // since seeing a /* would have set ended==true.
            end = len(content) - len(p);
            goto continue_Lines;
        }
        if (!bytes.HasPrefix(line, slashSlash)) {
            // Not comment line
            ended = true;
        }
        if (!inSlashStar && isGoBuildComment(line)) {
            if (goBuild != default!) {
                return (default!, default!, false, errMultipleGoBuild);
            }
            goBuild = line;
        }
        if (!inSlashStar && bytes.Equal(line, binaryOnlyComment)) {
            sawBinaryOnly = true;
        }
Comments:
        while (len(line) > 0) {
            if (inSlashStar) {
                {
                    nint i = bytes.Index(line, starSlash); if (i >= 0) {
                        inSlashStar = false;
                        line = bytes.TrimSpace(line.slice(i + len(starSlash)));
                        goto continue_Comments;
                    }
                }
                goto continue_Lines;
            }
            if (bytes.HasPrefix(line, slashSlash)) {
                goto continue_Lines;
            }
            if (bytes.HasPrefix(line, slashStar)) {
                inSlashStar = true;
                line = bytes.TrimSpace(line.slice(len(slashStar)));
                goto continue_Comments;
            }
            // Found non-comment text.
            goto break_Lines;
continue_Comments:;
        }
break_Comments:;
continue_Lines:;
    }
break_Lines:;
    return (content.slice(0, end), goBuild, sawBinaryOnly, default!);
}

// saveCgo saves the information from the #cgo lines in the import "C" comment.
// These lines set CFLAGS, CPPFLAGS, CXXFLAGS and LDFLAGS and pkg-config directives
// that affect the way cgo's C code is built.
internal static error saveCgo(this ж<Context> Ꮡctxt, @string filename, ж<Package> Ꮡdi, ж<ast.CommentGroup> Ꮡcg) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();
    ref var di = ref Ꮡdi.DerefOrNull();

    @string text = Ꮡcg.Text();
    foreach (var (_, vᴛ1) in strings.Split(text, "\n"u8)) {
        var line = vᴛ1;

        @string orig = line;
        // Line is
        //	#cgo [GOOS/GOARCH...] LDFLAGS: stuff
        //
        line = strings.TrimSpace(line);
        if (len(line) < 5 || line[..4] != "#cgo" || (line[4] != (rune)' ' && line[4] != (rune)'\t')) {
            continue;
        }
        // #cgo (nocallback|noescape) <function name>
        {
            var fields = strings.Fields(line); if (len(fields) == 3 && (fields[1] == "nocallback" || fields[1] == "noescape")) {
                continue;
            }
        }
        // Split at colon.
        var (lineΔ1, argstr, ok) = strings.Cut(strings.TrimSpace(line[4..]), ":"u8);
        if (!ok) {
            return fmt.Errorf("%s: invalid #cgo line: %s"u8, filename, orig);
        }
        // Parse GOOS/GOARCH stuff.
        var f = strings.Fields(lineΔ1);
        if (len(f) < 1) {
            return fmt.Errorf("%s: invalid #cgo line: %s"u8, filename, orig);
        }
        var cond = f.slice(0, len(f) - 1);
        @string verb = f[len(f) - 1];
        if (len(cond) > 0) {
            var okΔ1 = false;
            foreach (var (_, c) in cond) {
                if (Ꮡctxt.matchAuto(c, default!)) {
                    okΔ1 = true;
                    break;
                }
            }
            if (!okΔ1) {
                continue;
            }
        }
        var (args, err) = splitQuoted(argstr);
        if (err != default!) {
            return fmt.Errorf("%s: invalid #cgo line: %s"u8, filename, orig);
        }
        foreach (var (i, vᴛ2) in args) {
            var arg = vᴛ2;

            {
                (arg, ok) = expandSrcDir(arg, di.Dir); if (!ok) {
                    return fmt.Errorf("%s: malformed #cgo argument: %s"u8, filename, arg);
                }
            }
            args[i] = arg;
        }
        var exprᴛ1 = verb;
        if (exprᴛ1 == "CFLAGS"u8 || exprᴛ1 == "CPPFLAGS"u8 || exprᴛ1 == "CXXFLAGS"u8 || exprᴛ1 == "FFLAGS"u8 || exprᴛ1 == "LDFLAGS"u8) {
            ctxt.makePathsAbsolute(args, // Change relative paths to absolute.
 di.Dir);
        }

        var exprᴛ2 = verb;
        if (exprᴛ2 == "CFLAGS"u8) {
            di.CgoCFLAGS = appendꓸꓸꓸ(di.CgoCFLAGS, args);
        }
        else if (exprᴛ2 == "CPPFLAGS"u8) {
            di.CgoCPPFLAGS = appendꓸꓸꓸ(di.CgoCPPFLAGS, args);
        }
        else if (exprᴛ2 == "CXXFLAGS"u8) {
            di.CgoCXXFLAGS = appendꓸꓸꓸ(di.CgoCXXFLAGS, args);
        }
        else if (exprᴛ2 == "FFLAGS"u8) {
            di.CgoFFLAGS = appendꓸꓸꓸ(di.CgoFFLAGS, args);
        }
        else if (exprᴛ2 == "LDFLAGS"u8) {
            di.CgoLDFLAGS = appendꓸꓸꓸ(di.CgoLDFLAGS, args);
        }
        else if (exprᴛ2 == "pkg-config"u8) {
            di.CgoPkgConfig = appendꓸꓸꓸ(di.CgoPkgConfig, args);
        }
        else { /* default: */
            return fmt.Errorf("%s: invalid #cgo verb: %s"u8, filename, orig);
        }

    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcdirˢ = "${SRCDIR}"u8;

// expandSrcDir expands any occurrence of ${SRCDIR}, making sure
// the result is safe for the shell.
internal static (@string, bool) expandSrcDir(@string str, @string srcdir) {
    // "\" delimited paths cause safeCgoName to fail
    // so convert native paths with a different delimiter
    // to "/" before starting (eg: on windows).
    srcdir = filepath.ToSlash(srcdir);
    var chunks = strings.Split(str, srcdirˢ);
    if (len(chunks) < 2) {
        return (str, safeCgoName(str));
    }
    var ok = true;
    foreach (var (_, chunk) in chunks) {
        ok = ok && (chunk == ""u8 || safeCgoName(chunk));
    }
    ok = ok && (srcdir == ""u8 || safeCgoName(srcdir));
    @string res = strings.Join(chunks, srcdir);
    return (res, ok && res != ""u8);
}

// makePathsAbsolute looks for compiler options that take paths and
// makes them absolute. We do this because through the 1.8 release we
// ran the compiler in the package directory, so any relative -I or -L
// options would be relative to that directory. In 1.9 we changed to
// running the compiler in the build directory, to get consistent
// build results (issue #19964). To keep builds working, we change any
// relative -I or -L options to be absolute.
//
// Using filepath.IsAbs and filepath.Join here means the results will be
// different on different systems, but that's OK: -I and -L options are
// inherently system-dependent.
[GoRecv] internal static void makePathsAbsolute(this ref Context ctxt, slice<@string> args, @string srcDir) {
    var nextPath = false;
    foreach (var (i, arg) in args) {
        if (nextPath){
            if (!filepath.IsAbs(arg)) {
                args[i] = filepath.Join(srcDir, arg);
            }
            nextPath = false;
        } else 
        if (strings.HasPrefix(arg, "-I"u8) || strings.HasPrefix(arg, "-L"u8)) {
            if (len(arg) == 2){
                nextPath = true;
            } else {
                if (!filepath.IsAbs(arg[2..])) {
                    args[i] = arg[..2] + filepath.Join(srcDir, arg[2..]);
                }
            }
        }
    }
}

// NOTE: $ is not safe for the shell, but it is allowed here because of linker options like -Wl,$ORIGIN.
// We never pass these arguments to a shell (just to programs we construct argv for), so this should be okay.
// See golang.org/issue/6038.
// The @ is for OS X. See golang.org/issue/13720.
// The % is for Jenkins. See golang.org/issue/16959.
// The ! is because module paths may use them. See golang.org/issue/26716.
// The ~ and ^ are for sr.ht. See golang.org/issue/32260.
internal static readonly @string safeString = "+-.,/0123456789=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz:$@%! ~^"u8;

internal static bool safeCgoName(@string s) {
    if (s == ""u8) {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        {
            var c = s[i]; if (c < utf8.RuneSelf && strings.IndexByte(safeString, c) < 0) {
                return false;
            }
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unclosedQuoteˢ = "unclosed quote"u8;
internal static readonly @string unfinishedEscapingˢ = "unfinished escaping"u8;

// splitQuoted splits the string s around each instance of one or more consecutive
// white space characters while taking into account quotes and escaping, and
// returns an array of substrings of s or an empty list if s contains only white space.
// Single quotes and double quotes are recognized to prevent splitting within the
// quoted region, and are removed from the resulting substrings. If a quote in s
// isn't closed err will be set and r will have the unclosed argument as the
// last element. The backslash is used for escaping.
//
// For example, the following string:
//
//	a b:"c d" 'e''f'  "g\""
//
// Would be parsed as:
//
//	[]string{"a", "b:c d", "ef", `g"`}
internal static (slice<@string> r, error err) splitQuoted(@string s) {
    error err = default!;

    slice<@string> args = default!;
    var arg = new slice<rune>(len(s));
    var escaped = false;
    var quoted = false;
    var quote = (rune)'\x00';
    nint i = 0;
    foreach (var (_, rune) in s) {
        switch (ᐧ) {
        case {} when escaped: {
            escaped = false;
            break;
        }
        case {} when rune is (rune)'\\': {
            escaped = true;
            continue;
            break;
        }
        case {} when quote is not (rune)'\x00': {
            if (rune == quote) {
                quote = (rune)'\x00';
                continue;
            }
            break;
        }
        case {} when rune == (rune)'"' || rune == (rune)'\'': {
            quoted = true;
            quote = rune;
            continue;
            break;
        }
        case {} when unicode.IsSpace(rune): {
            if (quoted || i > 0) {
                quoted = false;
                args = append(args, ((@string)(arg.slice(0, i))));
                i = 0;
            }
            continue;
            break;
        }}

        arg[i] = rune;
        i++;
    }
    if (quoted || i > 0) {
        args = append(args, ((@string)(arg.slice(0, i))));
    }
    if (quote != 0){
        err = errors.New(unclosedQuoteˢ);
    } else 
    if (escaped) {
        err = errors.New(unfinishedEscapingˢ);
    }
    return (args, err);
}

// matchAuto interprets text as either a +build or //go:build expression (whichever works),
// reporting whether the expression matches the build context.
//
// matchAuto is only used for testing of tag evaluation
// and in #cgo lines, which accept either syntax.
internal static bool matchAuto(this ж<Context> Ꮡctxt, @string text, map<@string, bool> allTags) {
    if (strings.ContainsAny(text, "&|()"u8)){
        text = "//go:build "u8 + text;
    } else {
        text = "// +build "u8 + text;
    }
    var (x, err) = constraint.Parse(text);
    if (err != default!) {
        return false;
    }
    return Ꮡctxt.eval(x, allTags);
}

internal static bool eval(this ж<Context> Ꮡctxt, constraint.Expr x, map<@string, bool> allTags) {
    var allTagsʗ1 = allTags;
    return x.Eval((@string tag) => Ꮡctxt.Value.matchTag(tag, allTagsʗ1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goexperimentBoringcryptoˢ = "goexperiment.boringcrypto"u8;

// matchTag reports whether the name is one of:
//
//	cgo (if cgo is enabled)
//	$GOOS
//	$GOARCH
//	ctxt.Compiler
//	linux (if GOOS = android)
//	solaris (if GOOS = illumos)
//	darwin (if GOOS = ios)
//	unix (if this is a Unix GOOS)
//	boringcrypto (if GOEXPERIMENT=boringcrypto is enabled)
//	tag (if tag is listed in ctxt.BuildTags, ctxt.ToolTags, or ctxt.ReleaseTags)
//
// It records all consulted tags in allTags.
[GoRecv] internal static bool matchTag(this ref Context ctxt, @string name, map<@string, bool> allTags) {
    if (allTags != default!) {
        allTags[name] = true;
    }
    // special tags
    if (ctxt.CgoEnabled && name == "cgo"u8) {
        return true;
    }
    if (name == ctxt.GOOS || name == ctxt.GOARCH || name == ctxt.Compiler) {
        return true;
    }
    if (ctxt.GOOS == "android"u8 && name == "linux"u8) {
        return true;
    }
    if (ctxt.GOOS == "illumos"u8 && name == "solaris"u8) {
        return true;
    }
    if (ctxt.GOOS == "ios"u8 && name == "darwin"u8) {
        return true;
    }
    if (name == "unix"u8 && syslist.UnixOS[ctxt.GOOS]) {
        return true;
    }
    if (name == "boringcrypto"u8) {
        name = goexperimentBoringcryptoˢ; // boringcrypto is an old name for goexperiment.boringcrypto
    }
    // other tags
    foreach (var (_, tag) in ctxt.BuildTags) {
        if (tag == name) {
            return true;
        }
    }
    foreach (var (_, tag) in ctxt.ToolTags) {
        if (tag == name) {
            return true;
        }
    }
    foreach (var (_, tag) in ctxt.ReleaseTags) {
        if (tag == name) {
            return true;
        }
    }
    return false;
}

// goodOSArchFile returns false if the name contains a $GOOS or $GOARCH
// suffix which does not match the current system.
// The recognized name formats are:
//
//	name_$(GOOS).*
//	name_$(GOARCH).*
//	name_$(GOOS)_$(GOARCH).*
//	name_$(GOOS)_test.*
//	name_$(GOARCH)_test.*
//	name_$(GOOS)_$(GOARCH)_test.*
//
// Exceptions:
// if GOOS=android, then files with GOOS=linux are also matched.
// if GOOS=illumos, then files with GOOS=solaris are also matched.
// if GOOS=ios, then files with GOOS=darwin are also matched.
[GoRecv] internal static bool goodOSArchFile(this ref Context ctxt, @string name, map<@string, bool> allTags) {
    (name, _, _) = strings.Cut(name, "."u8);
    // Before Go 1.4, a file called "linux.go" would be equivalent to having a
    // build tag "linux" in that file. For Go 1.4 and beyond, we require this
    // auto-tagging to apply only to files with a non-empty prefix, so
    // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
    // systems, such as android, to arrive without breaking existing code with
    // innocuous source code in "android.go". The easiest fix: cut everything
    // in the name before the initial _.
    nint i = strings.Index(name, "_"u8);
    if (i < 0) {
        return true;
    }
    name = name.slice(i); // ignore everything before first _
    var l = strings.Split(name, "_"u8);
    {
        nint nΔ1 = len(l); if (nΔ1 > 0 && l[nΔ1 - 1] == "test") {
            l = l.slice(0, nΔ1 - 1);
        }
    }
    nint n = len(l);
    if (n >= 2 && syslist.KnownOS[l[n - 2]] && syslist.KnownArch[l[n - 1]]) {
        if (allTags != default!) {
            // In case we short-circuit on l[n-1].
            allTags[l[n - 2]] = true;
        }
        return ctxt.matchTag(l[n - 1], allTags) && ctxt.matchTag(l[n - 2], allTags);
    }
    if (n >= 1 && (syslist.KnownOS[l[n - 1]] || syslist.KnownArch[l[n - 1]])) {
        return ctxt.matchTag(l[n - 1], allTags);
    }
    return true;
}

// ToolDir is the directory containing build tools.
public static @string ToolDir = getToolDir();

// IsLocalImport reports whether the import path is
// a local import path, like ".", "..", "./foo", or "../foo".
public static bool IsLocalImport(@string path) {
    return path == "."u8 || path == ".."u8 || strings.HasPrefix(path, "./"u8) || strings.HasPrefix(path, "../"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string architectureLetterNoˢ = "architecture letter no longer used"u8;

// ArchChar returns "?" and an error.
// In earlier versions of Go, the returned string was used to derive
// the compiler and linker tool names, the default object file suffix,
// and the default linker output name. As of Go 1.5, those strings
// no longer vary by architecture; they are compile, link, .o, and a.out, respectively.
public static (@string, error) ArchChar(@string goarch) {
    return ("?", errors.New(architectureLetterNoˢ));
}

} // end build_package
