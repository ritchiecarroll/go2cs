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
using static go.archive.zip_package;

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
[assembly: GoPositionMap("archive/zip/reader.go", "reader.cs", "AB80loKCggAhXAAJAoKClIKCgpSCgoKUggAGGgAKAoKUgoKClMaigoKUgu6ClIKCgpTcgoKCgpSClIKUppSCgpS4goK4rLKClKaCgoKUyJIABBAACAKCgpSqwoKClAAJFIKUpoKCgoKUgoKClNyqooKClIIACg6CpoIADxiCprKClIKCgoKUgpSCgpSCgoKUlILcgriCpriigoKUgoKUgoKCzLKCgIKkgoKUgoKCgoKCgoKCgoKCgoKCgoKCgIKkgoKmgoKmxvy2goKasoKCgoKClJSU3IKCgpSUgoKClJSCgoKUxoKUgoKCgoKUgoKmgoKCgoLGgpSCgrSClILogoKCAAkUggALGJSCpqaCAAoWgIKkgoKmlICCpIKCAAgS5qSSgqKClIKClIKCgpSCyoIACBKCgpSmgoKCgpSCuIKCpqaCAAcQgoKCkrissoKClIKClIKClIKUgoKUqqKCgqaCgqaCgoKCgoKUpoKUlIK4lKbqgoKCpoKCgqaCgoKmgoKCpoKCggAZIqKClIKUqJCAooKCgoKUgoKUppSCyJKClJSCpsaCpriCuJSCgoKCpoKClIKCpoKCgpSUgpSmgsqCgpSmgoKClMrK3IKCgoKUzuKUgpSCgpSClIKClMaigoKClKa0goKmgoKCgoKUlIKCgqamgoKCgoKmlIKCgqaUAAwSgtSCpoKCgpSCgpSUgoKCgoKC3JSC", "807-881:1;878-880:1.1;935-941:1;953-960:1;961-968:2", "", "269=NewIEEE/1/1/2;656=uint16/1/5/1,uint16/2/5/2,uint16/3/5/3,uint16/4/5/4,uint32/1/2/5,uint32/2/2/6,uint16/5/5/7;1106=Errorf/1/1/3")]
[assembly: GoPositionMap("archive/zip/register.go", "register.cs", "AB88goKClJQADRDigoKClObSgoKCgoKClAAKCoKCgpSUAA0Q4oKCgpTm0oKCgoKCgpQADBCCgpSCqqKCvKKCuIKCgpSmgoKClA==", "110-110:1;111-111:2")]
[assembly: GoPositionMap("archive/zip/struct.go", "struct.cs", "AJUByAKSAAsQgoKClJSCgoKUlIKClJSCAAQU8oK4goKClJQADSCiqoKClKyyAA4m4oKCzsKuwoKCABcuspS0pIKUqJKCpoKUgrqSpoLGooKUlIKUpoKClLS0tLS0tKSClIKUgpSmgoKUtLS0tPikgpSClIKU", "", "", "199=Name/1/1/1")]
[assembly: GoPositionMap("archive/zip/writer.go", "writer.cs", "ABgoggAUMJLOwoKUqqLqooKUguqigoKUlIKUpoKCgoKCgoKCgoKCgriClpKCgoKCgoKUgqaCgoKCgoKUlICCpICCpICCpICCtpSCgpSCpoKCpoKCgoKCgoKCgqaCgoKUgsqCgqiSgoKCgoKCgoKAgqSAgrYAAhoACgK4zNKCgu6CgpSmAAgKooKCppSUAAIUAAkCgIIAEyaCgpS0toKmAAgSAAEQ4oKCgoKCgqaYysqCpoKCgpSUlMqCgpSCgoKUgoKUgoCCtoLGooKClIKmgoKCgoKCgriCgoK4goKUgoKClIKUggAEGgAMAoCCtoKUyoKAgraCgqa4gqrCgoK4goKClIKssoKU7LKigpSClIKClIKUgoKUgoKUgoKClIKUgoKUkoLogoKClAANCoKClAAUGoKClIKUguaCgpSCgpSCuIKCgpSCgoKUgqamgoKe0oKUlIKCgoKClIKUggALEIKCggAKDoIACAqCgqaCgqaCgqaCgg==", "504-541:1", "", "401=NewIEEE/1/1/3", "601:25")]
// </GoSourcePositionMaps>

namespace go.archive;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bufio = bufio_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using crc32 = go.hash.crc32_package;
using godebug = @internal.godebug_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using @internal;
using encoding;
using go.hash;
using go.io;
using go.path;
using zip = zip_package;
using static zip_package;
using flate = compress.flate_package;
using compress;
using utf8 = unicode.utf8_package;
using unicode;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(zip_package.File)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛFile;
[GoMethodHost(typeof(zip_package.FileHeader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛFileHeader;
[GoMethodHost(typeof(zip_package.ReadCloser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛReadCloser;
[GoMethodHost(typeof(zip_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛReader;
[GoMethodHost(typeof(zip_package.Writer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛWriter;
[GoMethodHost(typeof(zip_package.checksumReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛchecksumReader;
[GoMethodHost(typeof(zip_package.countWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛcountWriter;
[GoMethodHost(typeof(zip_package.dirReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛdirReader;
[GoMethodHost(typeof(zip_package.dirWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛdirWriter;
[GoMethodHost(typeof(zip_package.fileListEntry)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛfileListEntry;
[GoMethodHost(typeof(zip_package.fileWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛfileWriter;
[GoMethodHost(typeof(zip_package.headerFileInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛheaderFileInfo;
[GoMethodHost(typeof(zip_package.nopCloser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛnopCloser;
[GoMethodHost(typeof(zip_package.openDir)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛopenDir;
[GoMethodHost(typeof(zip_package.pooledFlateReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛpooledFlateReader;
[GoMethodHost(typeof(zip_package.pooledFlateWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛpooledFlateWriter;
[GoMethodHost(typeof(zip_package.readBuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛreadBuf;
[GoMethodHost(typeof(zip_package.writeBuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zipᴛwriteBuf;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class zipᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error init(this ж<Reader> Ꮡr, io.ReaderAt rdr, int64 size) => zip_package.initᴍ(Ꮡr, rdr, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RegisterDecompressor(this ref Reader r, uint16 method, Func<io.Reader, io.ReadCloser> dcomp) => zip_package.RegisterDecompressorᴍ(ref r, method, dcomp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<io.Reader, io.ReadCloser> decompressor(this ref Reader r, uint16 method) => zip_package.decompressorᴍ(ref r, method);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initFileList(this ж<Reader> Ꮡr) => zip_package.initFileListᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (fs.File, error) Open(this ж<Reader> Ꮡr, @string name) => zip_package.Openᴍ(Ꮡr, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<fileListEntry> openLookup(this ref Reader r, @string name) => zip_package.openLookupᴍ(ref r, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<fileListEntry> openReadDir(this ref Reader r, @string dir) => zip_package.openReadDirᴍ(ref r, dir);
}
partial class zipᴛReadCloser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref ReadCloser rc) => zip_package.Closeᴍ(ref rc);
}
partial class zipᴛFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 offset, error err) DataOffset(this ref File f) => zip_package.DataOffsetᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (io.ReadCloser, error) Open(this ж<File> Ꮡf) => zip_package.Openᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (io.Reader, error) OpenRaw(this ref File f) => zip_package.OpenRawᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) findBodyOffset(this ref File f) => zip_package.findBodyOffsetᴍ(ref f);
}
partial class zipᴛdirReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref dirReader r, slice<byte> _) => zip_package.Readᴍ(ref r, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref dirReader r) => zip_package.Closeᴍ(ref r);
}
partial class zipᴛchecksumReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ref checksumReader r) => zip_package.Statᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref checksumReader r, slice<byte> b) => zip_package.Readᴍ(ref r, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref checksumReader r) => zip_package.Closeᴍ(ref r);
}
partial class zipᴛreadBuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint8 uint8(this ref readBuf b) => zip_package.uint8ᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 uint16(this ref readBuf b) => zip_package.uint16ᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 uint32(this ref readBuf b) => zip_package.uint32ᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 uint64(this ref readBuf b) => zip_package.uint64ᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static readBuf sub(this ref readBuf b, nint n) => zip_package.subᴍ(ref b, n);
}
partial class zipᴛfileListEntry {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fileInfoDirEntry, error) stat(this ж<fileListEntry> Ꮡf) => zip_package.statᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref fileListEntry f) => zip_package.Nameᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this ref fileListEntry f) => zip_package.Sizeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Mode(this ref fileListEntry f) => zip_package.Modeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Type(this ref fileListEntry f) => zip_package.Typeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this ref fileListEntry f) => zip_package.IsDirᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this ref fileListEntry f) => zip_package.Sysᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this ref fileListEntry f) => zip_package.ModTimeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this ж<fileListEntry> Ꮡf) => zip_package.Infoᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<fileListEntry> Ꮡf) => zip_package.Stringᴍ(Ꮡf);
}
partial class zipᴛopenDir {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref openDir d) => zip_package.Closeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ref openDir d) => zip_package.Statᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref openDir d, slice<byte> _) => zip_package.Readᴍ(ref d, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ref openDir d, nint count) => zip_package.ReadDirᴍ(ref d, count);
}
partial class zipᴛpooledFlateWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) => zip_package.Writeᴍ(Ꮡw, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<pooledFlateWriter> Ꮡw) => zip_package.Closeᴍ(Ꮡw);
}
partial class zipᴛpooledFlateReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<pooledFlateReader> Ꮡr, slice<byte> p) => zip_package.Readᴍ(Ꮡr, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<pooledFlateReader> Ꮡr) => zip_package.Closeᴍ(Ꮡr);
}
partial class zipᴛFileHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static fs.FileInfo FileInfo(this ж<FileHeader> Ꮡh) => zip_package.FileInfoᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Time ModTime(this ref FileHeader h) => zip_package.ModTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetModTime(this ref FileHeader h, time.Time t) => zip_package.SetModTimeᴍ(ref h, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static fs.FileMode /*mode*/ Mode(this ref FileHeader h) => zip_package.Modeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMode(this ref FileHeader h, fs.FileMode mode) => zip_package.SetModeᴍ(ref h, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isZip64(this ref FileHeader h) => zip_package.isZip64ᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasDataDescriptor(this ref FileHeader h) => zip_package.hasDataDescriptorᴍ(ref h);
}
partial class zipᴛheaderFileInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this headerFileInfo fi) => zip_package.Nameᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this headerFileInfo fi) => zip_package.Sizeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this headerFileInfo fi) => zip_package.IsDirᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this headerFileInfo fi) => zip_package.ModTimeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Mode(this headerFileInfo fi) => zip_package.Modeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Type(this headerFileInfo fi) => zip_package.Typeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this headerFileInfo fi) => zip_package.Sysᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this headerFileInfo fi) => zip_package.Infoᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this headerFileInfo fi) => zip_package.Stringᴍ(fi);
}
partial class zipᴛWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetOffset(this ref Writer w, int64 n) => zip_package.SetOffsetᴍ(ref w, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Flush(this ref Writer w) => zip_package.Flushᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetComment(this ref Writer w, @string comment) => zip_package.SetCommentᴍ(ref w, comment);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Writer w) => zip_package.Closeᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (io.Writer, error) Create(this ref Writer w, @string name) => zip_package.Createᴍ(ref w, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error prepare(this ref Writer w, ж<FileHeader> fh) => zip_package.prepareᴍ(ref w, fh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (io.Writer, error) CreateHeader(this ref Writer w, ж<FileHeader> Ꮡfh) => zip_package.CreateHeaderᴍ(ref w, Ꮡfh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (io.Writer, error) CreateRaw(this ref Writer w, ж<FileHeader> Ꮡfh) => zip_package.CreateRawᴍ(ref w, Ꮡfh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Copy(this ref Writer w, ж<File> Ꮡf) => zip_package.Copyᴍ(ref w, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RegisterCompressor(this ref Writer w, uint16 method, Func<io.Writer, (io.WriteCloser, error)> comp) => zip_package.RegisterCompressorᴍ(ref w, method, comp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error AddFS(this ж<Writer> Ꮡw, fs.FS fsys) => zip_package.AddFSᴍ(Ꮡw, fsys);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<io.Writer, (io.WriteCloser, error)> compressor(this ref Writer w, uint16 method) => zip_package.compressorᴍ(ref w, method);
}
partial class zipᴛdirWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this dirWriter _, slice<byte> b) => zip_package.Writeᴍ(_, b);
}
partial class zipᴛfileWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref fileWriter w, slice<byte> p) => zip_package.Writeᴍ(ref w, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ref fileWriter w) => zip_package.closeᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeDataDescriptor(this ref fileWriter w) => zip_package.writeDataDescriptorᴍ(ref w);
}
partial class zipᴛcountWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref countWriter w, slice<byte> p) => zip_package.Writeᴍ(ref w, p);
}
partial class zipᴛnopCloser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this nopCloser w) => zip_package.Closeᴍ(w);
}
partial class zipᴛwriteBuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint8(this ref writeBuf b, uint8 v) => zip_package.uint8ᴍ(ref b, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint16(this ref writeBuf b, uint16 v) => zip_package.uint16ᴍ(ref b, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint32(this ref writeBuf b, uint32 v) => zip_package.uint32ᴍ(ref b, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint64(this ref writeBuf b, uint64 v) => zip_package.uint64ᴍ(ref b, v);
}
// </GlyphForwarders>
