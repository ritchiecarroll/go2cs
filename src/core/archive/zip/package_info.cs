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
using fs = go.io.fs_package;
using io = go.io_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.archive.zip_package;
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
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<checksumReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<countWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dirReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<dirWriter, io_package.Writer>]
[assembly: GoImplement<fileListEntry, fileInfoDirEntry>(Pointer = true)]
[assembly: GoImplement<fileListEntry, go.io.fs_package.DirEntry>(Pointer = true)]
[assembly: GoImplement<fileListEntry, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fileWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<headerFileInfo, fileInfoDirEntry>]
[assembly: GoImplement<nopCloser, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<nopCloser, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<openDir, go.io.fs_package.File>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<pooledFlateReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<pooledFlateWriter, io_package.WriteCloser>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(FileHeader))]
[assembly: GoFieldAccess(typeof(ReadCloser))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(countWriter))]
[assembly: GoFieldAccess(typeof(directoryEnd))]
[assembly: GoFieldAccess(typeof(fileListEntry))]
[assembly: GoFieldAccess(typeof(fileWriter))]
[assembly: GoFieldAccess(typeof(header))]
[assembly: GoFieldAccess(typeof(pooledFlateReader))]
[assembly: GoFieldAccess(typeof(pooledFlateWriter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("archive/zip/reader.go", "reader.cs", "AB00loKCggAhXAAJAoKClIKCgpSCgoKUggAGGgAKAoKUgoKClKaigoKUgu6ClIKCgpTcgoKCgpSClIKUppSCgpS4goK4rLKClKaCgoKUqJIAAhAACAKCgpSqwoKClAAJFIKUpoKCgoKUgoKClNyqooKClILugqaCAAsYgqaygpSCgoKClIKUgoKUgoKClJSC3IK4gqaYooKClIKClIKCgqyygoCCpIKClIKCgoKCgoKCgoKCgoKCgoKCgoCCpIKCpoKCpsb8toKCmrKCgoKCgpSUlNyCgoKUlIKCgpSUgoKClMaClIKCgoKClIKCpoKCgoKCxoKUgoK0gpSC6IKCggAJFIIACxiUgqamggAKFoCCpIKCppSAgqSCggAIEuakkoKigpSCgpSCgoKUgsqCAAgSgoKUpoKCgoKUgriCgqamggAHEIKCgpK4rLKCgpSCgpSCgpSClIKClKqigoKmgoKmgoKCgoKClKaClJSCuJSmyoKCgqaCgoKmgoKCpoKCgqaCgoIAFSKigpSClKiQgKKCgoKClIKClKaUgqiSgpSUgqamgqa4griUgoKCgqaCgpSCgqaCgoKUlIKUpoLKgoKUpoKCgpTKyryCgoKClK7ilIKUgoKUgpSCgpSmooKCgpSmlIKCpoKCgoKClJSCgoKmpoKCgoKCppSCgoKmlAAIEoLUgraCgoKUgoKUlIKCgoKCguyUgg==", "807-881:1;878-880:1.1;935-941:1;953-960:1;961-968:2", "", "261=NewIEEE/1/1/2;636=uint16/1/5/1,uint16/2/5/2,uint16/3/5/3,uint16/4/5/4,uint32/1/2/5,uint32/2/2/6,uint16/5/5/7;1065=Errorf/1/1/3")]
[assembly: GoPositionMap("archive/zip/register.go", "register.cs", "AB48goKClJQACxDigoKClObSgoKCgoKClAAICoKCgpSUAAsQ4oKCgpTm0oKCgoKCgpQAChCCgpSCqqKCvKKCuIKCgpSmgoKClA==", "110-110:1;111-111:2")]
[assembly: GoPositionMap("archive/zip/struct.go", "struct.cs", "AJIByAKSAAcQgoKClJSCgoKUlIKClJSCAAIU8oK4goKClJQADSCiqoKClKyyAA4m4oKCrsKuwoKCABMuspS0pIKUqJKCpoKUgrqSpoKmooKUlIKUpoKClLS0tLS0tKSClIKUgpSmgoKUtLS0tPikgpSClIKU", "", "", "190=Name/1/1/1")]
[assembly: GoPositionMap("archive/zip/writer.go", "writer.cs", "ABYoggAUMJKuwoKUqqLqooKUguqigoKUlIKUpoKCgoKCgoKCgoKCgriClpKCgoKCgoKUgqaCgoKCgoKUlICCpICCpICCpICCtpSCgpSCpoKCpoKCgoKCgoKCgqaCgoKUgsqCgqiSgoKCgoKCgoKAgqSAgrYAAhoACgK4rNKCgu6CgpSm6qKCgqaUlAACFAAJAoCCABMmgoKUtLaCpgAIEgABEOKCgoKCgoKmmMrKgqaCgoKUlJTagoKUgoKClIKClIKAgraCpqKCgpSCpoKCgoKCgoK4goKCuIKClIKCgpSClIIAAhoADAKAgraClMqCgIK2goKmuIKqwoKCuIKCgpSCrLKClOyyooKUgpSCgpSClIKClIKClIKCgpSClIKClJKC6IKCgpQACQqCgpQAEBqCgpSClILmgoKUgoKUgriCgoKUgoKClIKmpoKCntKClJSCgoKCgpSClIIABxCCgoLugsqCgqaCgqaCgqaCgg==", "504-541:1", "", "393=NewIEEE/1/1/3", "590:25")]
// </GoSourcePositionMaps>

namespace go.archive;

using static go.archive.zip_package;
using fs = go.io.fs_package;
using io = go.io_package;
using time = go.time_package;

[GoPackage("zip")]
public static partial class zip_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface fileInfoDirEntry {}
    internal partial struct checksumReader {}
    internal partial struct countWriter {}
    internal partial struct dirReader {}
    internal partial struct dirWriter {}
    internal partial struct directoryEnd {}
    internal partial struct fileListEntry {}
    internal partial struct fileWriter {}
    internal partial struct header {}
    internal partial struct headerFileInfo {}
    internal partial struct nopCloser {}
    internal partial struct openDir {}
    internal partial struct pooledFlateReader {}
    internal partial struct pooledFlateWriter {}
    internal partial struct readBuf {}
    internal partial struct writeBuf {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct ReadCloser {}
    public partial struct Reader {}
    public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸflate() => builtin.initPackage(typeof(compress.flate_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(go.hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(zip_package.File)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛFile;
[GoMethodHost(typeof(zip_package.FileHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛFileHeader;
[GoMethodHost(typeof(zip_package.ReadCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛReadCloser;
[GoMethodHost(typeof(zip_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛReader;
[GoMethodHost(typeof(zip_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛWriter;
[GoMethodHost(typeof(zip_package.checksumReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛchecksumReader;
[GoMethodHost(typeof(zip_package.countWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛcountWriter;
[GoMethodHost(typeof(zip_package.dirReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛdirReader;
[GoMethodHost(typeof(zip_package.dirWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛdirWriter;
[GoMethodHost(typeof(zip_package.fileListEntry)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛfileListEntry;
[GoMethodHost(typeof(zip_package.fileWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛfileWriter;
[GoMethodHost(typeof(zip_package.headerFileInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛheaderFileInfo;
[GoMethodHost(typeof(zip_package.nopCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛnopCloser;
[GoMethodHost(typeof(zip_package.openDir)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛopenDir;
[GoMethodHost(typeof(zip_package.pooledFlateReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛpooledFlateReader;
[GoMethodHost(typeof(zip_package.pooledFlateWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛpooledFlateWriter;
[GoMethodHost(typeof(zip_package.readBuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛreadBuf;
[GoMethodHost(typeof(zip_package.writeBuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zipᴛwriteBuf;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class zipᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 offset, error err) DataOffset(this ref File f) => zip_package.DataOffset⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.ReadCloser, error) Open(this ж<File> Ꮡf) => zip_package.Open⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Reader, error) OpenRaw(this ref File f) => zip_package.OpenRaw⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) findBodyOffset(this ref File f) => zip_package.findBodyOffset⁔(ref f);
}
partial class zipᴛFileHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static fs.FileInfo FileInfo(this ж<FileHeader> Ꮡh) => zip_package.FileInfo⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static time.Time ModTime(this ref FileHeader h) => zip_package.ModTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetModTime(this ref FileHeader h, time.Time t) => zip_package.SetModTime⁔(ref h, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static fs.FileMode /*mode*/ Mode(this ref FileHeader h) => zip_package.Mode⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMode(this ref FileHeader h, fs.FileMode mode) => zip_package.SetMode⁔(ref h, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isZip64(this ref FileHeader h) => zip_package.isZip64⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasDataDescriptor(this ref FileHeader h) => zip_package.hasDataDescriptor⁔(ref h);
}
partial class zipᴛReadCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref ReadCloser rc) => zip_package.Close⁔(ref rc);
}
partial class zipᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error init(this ж<Reader> Ꮡr, io.ReaderAt rdr, int64 size) => zip_package.init⁔(Ꮡr, rdr, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RegisterDecompressor(this ref Reader r, uint16 method, Decompressor dcomp) => zip_package.RegisterDecompressor⁔(ref r, method, dcomp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Decompressor decompressor(this ref Reader r, uint16 method) => zip_package.decompressor⁔(ref r, method);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initFileList(this ж<Reader> Ꮡr) => zip_package.initFileList⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (fs.File, error) Open(this ж<Reader> Ꮡr, @string name) => zip_package.Open⁔(Ꮡr, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<fileListEntry> openLookup(this ref Reader r, @string name) => zip_package.openLookup⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<fileListEntry> openReadDir(this ref Reader r, @string dir) => zip_package.openReadDir⁔(ref r, dir);
}
partial class zipᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetOffset(this ref Writer w, int64 n) => zip_package.SetOffset⁔(ref w, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref Writer w) => zip_package.Flush⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetComment(this ref Writer w, @string comment) => zip_package.SetComment⁔(ref w, comment);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Writer w) => zip_package.Close⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) Create(this ref Writer w, @string name) => zip_package.Create⁔(ref w, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error prepare(this ref Writer w, ж<FileHeader> Ꮡfh) => zip_package.prepare⁔(ref w, Ꮡfh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) CreateHeader(this ref Writer w, ж<FileHeader> Ꮡfh) => zip_package.CreateHeader⁔(ref w, Ꮡfh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) CreateRaw(this ref Writer w, ж<FileHeader> Ꮡfh) => zip_package.CreateRaw⁔(ref w, Ꮡfh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Copy(this ref Writer w, ж<File> Ꮡf) => zip_package.Copy⁔(ref w, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RegisterCompressor(this ref Writer w, uint16 method, Compressor comp) => zip_package.RegisterCompressor⁔(ref w, method, comp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddFS(this ж<Writer> Ꮡw, fs.FS fsys) => zip_package.AddFS⁔(Ꮡw, fsys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Compressor compressor(this ref Writer w, uint16 method) => zip_package.compressor⁔(ref w, method);
}
partial class zipᴛchecksumReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ref checksumReader r) => zip_package.Stat⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref checksumReader r, slice<byte> b) => zip_package.Read⁔(ref r, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref checksumReader r) => zip_package.Close⁔(ref r);
}
partial class zipᴛcountWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref countWriter w, slice<byte> p) => zip_package.Write⁔(ref w, p);
}
partial class zipᴛdirReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref dirReader r, slice<byte> _) => zip_package.Read⁔(ref r, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref dirReader r) => zip_package.Close⁔(ref r);
}
partial class zipᴛdirWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this dirWriter _, slice<byte> b) => zip_package.Write⁔(_, b);
}
partial class zipᴛfileListEntry {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fileInfoDirEntry, error) stat(this ж<fileListEntry> Ꮡf) => zip_package.stat⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref fileListEntry f) => zip_package.Name⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Size(this ref fileListEntry f) => zip_package.Size⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Mode(this ref fileListEntry f) => zip_package.Mode⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Type(this ref fileListEntry f) => zip_package.Type⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsDir(this ref fileListEntry f) => zip_package.IsDir⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Sys(this ref fileListEntry f) => zip_package.Sys⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time ModTime(this ref fileListEntry f) => zip_package.ModTime⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this ж<fileListEntry> Ꮡf) => zip_package.Info⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<fileListEntry> Ꮡf) => zip_package.String⁔(Ꮡf);
}
partial class zipᴛfileWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref fileWriter w, slice<byte> p) => zip_package.Write⁔(ref w, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ref fileWriter w) => zip_package.close⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeDataDescriptor(this ref fileWriter w) => zip_package.writeDataDescriptor⁔(ref w);
}
partial class zipᴛheaderFileInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this headerFileInfo fi) => zip_package.Name⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Size(this headerFileInfo fi) => zip_package.Size⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsDir(this headerFileInfo fi) => zip_package.IsDir⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time ModTime(this headerFileInfo fi) => zip_package.ModTime⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Mode(this headerFileInfo fi) => zip_package.Mode⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Type(this headerFileInfo fi) => zip_package.Type⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Sys(this headerFileInfo fi) => zip_package.Sys⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this headerFileInfo fi) => zip_package.Info⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this headerFileInfo fi) => zip_package.String⁔(fi);
}
partial class zipᴛnopCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this nopCloser w) => zip_package.Close⁔(w);
}
partial class zipᴛopenDir {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref openDir d) => zip_package.Close⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ref openDir d) => zip_package.Stat⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref openDir d, slice<byte> _) => zip_package.Read⁔(ref d, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ref openDir d, nint count) => zip_package.ReadDir⁔(ref d, count);
}
partial class zipᴛpooledFlateReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<pooledFlateReader> Ꮡr, slice<byte> p) => zip_package.Read⁔(Ꮡr, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<pooledFlateReader> Ꮡr) => zip_package.Close⁔(Ꮡr);
}
partial class zipᴛpooledFlateWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) => zip_package.Write⁔(Ꮡw, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<pooledFlateWriter> Ꮡw) => zip_package.Close⁔(Ꮡw);
}
partial class zipᴛreadBuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 uint8(this ref readBuf b) => zip_package.uint8⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 uint16(this ref readBuf b) => zip_package.uint16⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32(this ref readBuf b) => zip_package.uint32⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uint64(this ref readBuf b) => zip_package.uint64⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static readBuf sub(this ref readBuf b, nint n) => zip_package.sub⁔(ref b, n);
}
partial class zipᴛwriteBuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint8(this ref writeBuf b, uint8 v) => zip_package.uint8⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint16(this ref writeBuf b, uint16 v) => zip_package.uint16⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint32(this ref writeBuf b, uint32 v) => zip_package.uint32⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint64(this ref writeBuf b, uint64 v) => zip_package.uint64⁔(ref b, v);
}
// </MethodForwarders>
