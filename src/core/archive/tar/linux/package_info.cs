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
using static go.archive.tar_package;
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
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465727d", "WriteTo_src")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5772697465727d", "ReadFrom_dst")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ReadFrom_dst, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<ReadFrom_dst, io_package.Writer>]
[assembly: GoImplement<Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<WriteTo_src, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<WriteTo_src, io_package.Reader>]
[assembly: GoImplement<Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<headerError, error>]
[assembly: GoImplement<headerFileInfo, go.io.fs_package.FileInfo>]
[assembly: GoImplement<regFileReader, fileReader>(Pointer = true)]
[assembly: GoImplement<regFileReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<regFileWriter, fileWriter>(Pointer = true)]
[assembly: GoImplement<regFileWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<sparseFileReader, fileReader>(Pointer = true)]
[assembly: GoImplement<sparseFileReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<sparseFileWriter, fileWriter>(Pointer = true)]
[assembly: GoImplement<sparseFileWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<zeroReader, io_package.Reader>]
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
[assembly: GoFieldAccess(typeof(Header))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(Writer))]
[assembly: GoFieldAccess(typeof(go.os.user_package.Group))]
[assembly: GoFieldAccess(typeof(go.os.user_package.User))]
[assembly: GoFieldAccess(typeof(syscall_package.Stat_t))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("archive/tar/common.go", "common.cs", "AB8+loKCgoKCgoKCAAsKooKCgoKmgpQAYZwBAEqgAQAlUsaClIKClKSkpJSUAAIU8oKCgoKClIKmAAIWAAgCgoKCgpSCgpSUggAtKAAMAoKUooLYgoKCgpSCgoKClIKClKaAgraigoKUgoKCgoKUpoKmooKUgoKUgoKCgpSCgoKCgpSmgrqSgoKCgoKCgoKCgoKCgoKClqKmgsakgoKUgqSCuIKClIKUgoKktLS2gpSCggAbOIKClJSClLS0tLaokgAHEIKCgpaSgpSoxKaClIKUgriktLS0tIK0tpS0grS0tLamggAZRgALAoKUgsqUgrSCtIK0gpTGtKSUgpSClIK4poKCgoKCgoKUgoKUlIKCgoKCgpSCgqaClAAOIKKUpA==", "350-376:1;377-395:2;396-423:3", "", "770=Name/1/1/1,ModTime/1/1/2,Perm/1/1/3")]
[assembly: GoPositionMap("archive/tar/format.go", "format.cs", "AGrYAYKCgpT4ooKCgqaUpKQAHjyiAAcQsKKgoqCioKKa1JKCgoK4goKClKSkpLzU2IK0goK0grS6ooKCgq7yooKUgpSoktqCgoKCgoKCgsigooKCgoKCgoKCgsigooKCgoKCgoKCgsigooKCgoKCgriCgriC")]
[assembly: GoPositionMap("archive/tar/reader.go", "reader.cs", "ACVOkgACHAANAoKUgoKCgoKmpqKC/oKUgIKkgpSUgoKUgIKkppSCgoKUgoIABxC0goKCpoKUtKTqgpSClIKUgoKU3ILKgriClIIABBDSgoKUgqaCgqrCgoKClMqCgpSClK7kkpKUtLSktJSmgpSCgpSCgoKUuIKUqLKCgpSClLS0tLSCtIK0tLS0tIKClLaCpoKqwoKClJrEgoKCgpSUpoaUtLaClAACFgALBICCpIKClIKUuIKCpoKmgoKCgoKCgoKmgoKCgoKUgpSCgriCgpSChMaCgoK0goKCgIKkgIIAGDCCgoKClLaCpgACFgAOCIKUlIKCgpSCgoKClIKUgoKClIKCgriUgpSCgpSUAAIaAAoCAAYSkoKCgpSClIKCgrjKgoKCyoKUgoLcgpSCgoKCgpSCgqaqxoKCgriCgpSCyoKCgoKClIKClJSmgoKUAAIaAAoCgpSCgpQAAhoACgKClIKClAAIErKClIKClJSkpPiCqJKokgAJFMKCgqaCgoKCgoKClIKUgoKCuIKUpKSkpLjCgoKCpoKmgoKCgoKCgpSCgoKUlIKC3IKCpoKUpKSkuIKUgtqCgqqigoKUqtKCgoKUgpSqooKClKiawsqClIKClLiCgpQ=", "407-407:1;411-411:2;546-563:1;567-571:2", "", "386=Contains/1/1/2;472=modTime/1/1/1,Invoke/5/7/1,devMajor/1/1/1,Invoke/6/7/1,devMinor/1/1/1,Invoke/7/7/1")]
[assembly: GoPositionMap("archive/tar/stat_actime1.go", "stat_actime1.cs", "AAscorai")]
[assembly: GoPositionMap("archive/tar/stat_unix.go", "stat_unix.cs", "ABIkggAJDqKCgpSCgriCgoKClIKCgoKmgqaCgqSSgoLGgoKCgsaCgsaCgsaCgsaCgoLGgoKC+g==")]
[assembly: GoPositionMap("archive/tar/strconv.go", "strconv.cs", "AA0gkqiSgoKmqsKClIKCgqYAChqigpSokoKUgoLcgoIAAxbygqzojNKCpoKigoKUgoKUlIKClIKUuKqigoKmgoKClIKmgqbclIKUgoKUpoKCgqaUgpSqooKssqamgoKUgsqCgqSktoKClKqikoK4goKCgpSs9IKCuIKClIKCuKKCuIKCpoKUqqKCpoKCgqaCgpQAAhoACgKClJSk")]
[assembly: GoPositionMap("archive/tar/writer.go", "writer.cs", "ACJIkgAJHtKClIKUgpSCrvKClLiCgpQACBKCgoKmgpSCpIKkgqS4tJKCqJKCgoKClAAIBrIAJEimgoSkgoKClKiSgoKCgpSUgoKUgoKUgILKkoCSgoKAggAPHua0koKCgqaCgoK6koKCgoKUggAnUIKAgsiUgpSUAAYcAAkClIKCpoKCgoKCgoKClIKCgoKUrNKmgoKUlIKCgoKCgoKCgoKCuIKUgqzSgIKkgIKkgpSCguyyooKUgpSCgpSCgoKCgpSClIKClIKClIKUgpSCgpSSguyigoKCgoKCpoKCgoKUAAIU8oKUgoKUAAIaAAoCgpSCgpSssoKUgriCgriCAAgSsoKClIKClJSkpPiCqJKokgAJFMKCgqaCgoKCgoKClIKUgoKCuIKUpKSkpLjCgoKCpoKmgoKCgoKCgoKClJSClIKC3IKCpoKUpKSkpLiCpoLsgoKCpqiSgpSkpA==", "209-209:1;407-449:1", "", "", "497:25")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("tar")]
public static partial class tar_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface fileReader {}
    internal partial interface fileState {}
    internal partial interface fileWriter {}
    internal partial struct ReadFrom_dst {}
    internal partial struct WriteTo_src {}
    internal partial struct block {}
    internal partial struct formatter {}
    internal partial struct headerError {}
    internal partial struct headerFileInfo {}
    internal partial struct headerGNU {}
    internal partial struct headerSTAR {}
    internal partial struct headerUSTAR {}
    internal partial struct headerV7 {}
    internal partial struct parser {}
    internal partial struct regFileReader {}
    internal partial struct regFileWriter {}
    internal partial struct sparseArray {}
    internal partial struct sparseDatas {}
    internal partial struct sparseElem {}
    internal partial struct sparseEntry {}
    internal partial struct sparseFileReader {}
    internal partial struct sparseFileWriter {}
    internal partial struct sparseHoles {}
    internal partial struct zeroReader {}
    internal partial struct zeroWriter {}
    public partial interface FileInfoNames {}
    public partial struct Format {}
    public partial struct Header {}
    [GoValueClone("blk")] public partial struct Reader {}
    [GoValueClone("blk")] public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸuser() => builtin.initPackage(typeof(os.user_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(tar_package.Format)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛFormat;
[GoMethodHost(typeof(tar_package.Header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛHeader;
[GoMethodHost(typeof(tar_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛReader;
[GoMethodHost(typeof(tar_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛWriter;
[GoMethodHost(typeof(tar_package.block)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛblock;
[GoMethodHost(typeof(tar_package.formatter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛformatter;
[GoMethodHost(typeof(tar_package.headerError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderError;
[GoMethodHost(typeof(tar_package.headerFileInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderFileInfo;
[GoMethodHost(typeof(tar_package.headerGNU)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderGNU;
[GoMethodHost(typeof(tar_package.headerSTAR)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderSTAR;
[GoMethodHost(typeof(tar_package.headerUSTAR)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderUSTAR;
[GoMethodHost(typeof(tar_package.headerV7)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛheaderV7;
[GoMethodHost(typeof(tar_package.parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛparser;
[GoMethodHost(typeof(tar_package.regFileReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛregFileReader;
[GoMethodHost(typeof(tar_package.regFileWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛregFileWriter;
[GoMethodHost(typeof(tar_package.sparseArray)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛsparseArray;
[GoMethodHost(typeof(tar_package.sparseElem)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛsparseElem;
[GoMethodHost(typeof(tar_package.sparseEntry)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛsparseEntry;
[GoMethodHost(typeof(tar_package.sparseFileReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛsparseFileReader;
[GoMethodHost(typeof(tar_package.sparseFileWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛsparseFileWriter;
[GoMethodHost(typeof(tar_package.zeroReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛzeroReader;
[GoMethodHost(typeof(tar_package.zeroWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class tarᴛzeroWriter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class tarᴛFormat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool has(this tar_package.Format f, tar_package.Format f2) => tar_package.has⁔(f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mayBe(this ref tar_package.Format f, tar_package.Format f2) => tar_package.mayBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mayOnlyBe(this ref tar_package.Format f, tar_package.Format f2) => tar_package.mayOnlyBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mustNotBe(this ref tar_package.Format f, tar_package.Format f2) => tar_package.mustNotBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this tar_package.Format f) => tar_package.String⁔(f);
}
partial class tarᴛHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (tar_package.Format format, map<@string, @string> paxHdrs, error err) allowedFormats(this tar_package.Header h) => tar_package.allowedFormats⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static fs.FileInfo FileInfo(this ж<tar_package.Header> Ꮡh) => tar_package.FileInfo⁔(Ꮡh);
}
partial class tarᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<tar_package.Header>, error) Next(this ж<tar_package.Reader> Ꮡtr) => tar_package.Next⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<tar_package.Header>, error) next(this ж<tar_package.Reader> Ꮡtr) => tar_package.next⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleRegularFile(this ref tar_package.Reader tr, ж<tar_package.Header> Ꮡhdr) => tar_package.handleRegularFile⁔(ref tr, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleSparseFile(this ref tar_package.Reader tr, ж<tar_package.Header> Ꮡhdr, ж<tar_package.block> ᏑrawHdr) => tar_package.handleSparseFile⁔(ref tr, Ꮡhdr, ᏑrawHdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (tar_package.sparseDatas, error) readGNUSparsePAXHeaders(this ref tar_package.Reader tr, ж<tar_package.Header> Ꮡhdr) => tar_package.readGNUSparsePAXHeaders⁔(ref tr, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<tar_package.Header>, ж<tar_package.block>, error) readHeader(this ж<tar_package.Reader> Ꮡtr) => tar_package.readHeader⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (tar_package.sparseDatas, error) readOldGNUSparseMap(this ref tar_package.Reader tr, ж<tar_package.Header> Ꮡhdr, ж<tar_package.block> Ꮡblk) => tar_package.readOldGNUSparseMap⁔(ref tr, Ꮡhdr, Ꮡblk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this ref tar_package.Reader tr, slice<byte> b) => tar_package.Read⁔(ref tr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) writeTo(this ref tar_package.Reader tr, io.Writer w) => tar_package.writeTo⁔(ref tr, w);
}
partial class tarᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref tar_package.Writer tw) => tar_package.Flush⁔(ref tw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteHeader(this ж<tar_package.Writer> Ꮡtw, ж<tar_package.Header> Ꮡhdr) => tar_package.WriteHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeUSTARHeader(this ж<tar_package.Writer> Ꮡtw, ж<tar_package.Header> Ꮡhdr) => tar_package.writeUSTARHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writePAXHeader(this ж<tar_package.Writer> Ꮡtw, ж<tar_package.Header> Ꮡhdr, map<@string, @string> paxHdrs) => tar_package.writePAXHeader⁔(Ꮡtw, Ꮡhdr, paxHdrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeGNUHeader(this ж<tar_package.Writer> Ꮡtw, ж<tar_package.Header> Ꮡhdr) => tar_package.writeGNUHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.block> templateV7Plus(this ж<tar_package.Writer> Ꮡtw, ж<tar_package.Header> Ꮡhdr, Action<slice<byte>, @string> fmtStr, Action<slice<byte>, int64> fmtNum) => tar_package.templateV7Plus⁔(Ꮡtw, Ꮡhdr, fmtStr, fmtNum);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeRawFile(this ж<tar_package.Writer> Ꮡtw, @string name, @string data, byte flag, tar_package.Format format) => tar_package.writeRawFile⁔(Ꮡtw, name, data, flag, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeRawHeader(this ref tar_package.Writer tw, ж<tar_package.block> Ꮡblk, int64 size, byte flag) => tar_package.writeRawHeader⁔(ref tw, Ꮡblk, size, flag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddFS(this ж<tar_package.Writer> Ꮡtw, fs.FS fsys) => tar_package.AddFS⁔(Ꮡtw, fsys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ref tar_package.Writer tw, slice<byte> b) => tar_package.Write⁔(ref tw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) readFrom(this ref tar_package.Writer tw, io.Reader r) => tar_package.readFrom⁔(ref tw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref tar_package.Writer tw) => tar_package.Close⁔(ref tw);
}
partial class tarᴛblock {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerV7> toV7(this ж<tar_package.block> Ꮡb) => tar_package.toV7⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerGNU> toGNU(this ж<tar_package.block> Ꮡb) => tar_package.toGNU⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerSTAR> toSTAR(this ж<tar_package.block> Ꮡb) => tar_package.toSTAR⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerUSTAR> toUSTAR(this ж<tar_package.block> Ꮡb) => tar_package.toUSTAR⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static tar_package.sparseArray toSparse(this ref tar_package.block b) => tar_package.toSparse⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static tar_package.Format getFormat(this ж<tar_package.block> Ꮡb) => tar_package.getFormat⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setFormat(this ж<tar_package.block> Ꮡb, tar_package.Format format) => tar_package.setFormat⁔(Ꮡb, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 unsigned, int64 signed) computeChecksum(this ref tar_package.block b) => tar_package.computeChecksum⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref tar_package.block b) => tar_package.reset⁔(ref b);
}
partial class tarᴛformatter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatString(this ref tar_package.formatter f, slice<byte> b, @string s) => tar_package.formatString⁔(ref f, b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatNumeric(this ref tar_package.formatter f, slice<byte> b, int64 x) => tar_package.formatNumeric⁔(ref f, b, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatOctal(this ref tar_package.formatter f, slice<byte> b, int64 x) => tar_package.formatOctal⁔(ref f, b, x);
}
partial class tarᴛheaderError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this tar_package.headerError he) => tar_package.Error⁔(he);
}
partial class tarᴛheaderFileInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Size(this tar_package.headerFileInfo fi) => tar_package.Size⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsDir(this tar_package.headerFileInfo fi) => tar_package.IsDir⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time ModTime(this tar_package.headerFileInfo fi) => tar_package.ModTime⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Sys(this tar_package.headerFileInfo fi) => tar_package.Sys⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this tar_package.headerFileInfo fi) => tar_package.Name⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode /*mode*/ Mode(this tar_package.headerFileInfo fi) => tar_package.Mode⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this tar_package.headerFileInfo fi) => tar_package.String⁔(fi);
}
partial class tarᴛheaderGNU {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerV7> v7(this ж<tar_package.headerGNU> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref tar_package.headerGNU h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref tar_package.headerGNU h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref tar_package.headerGNU h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref tar_package.headerGNU h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref tar_package.headerGNU h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref tar_package.headerGNU h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> accessTime(this ref tar_package.headerGNU h) => tar_package.accessTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> changeTime(this ref tar_package.headerGNU h) => tar_package.changeTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static tar_package.sparseArray sparse(this ref tar_package.headerGNU h) => tar_package.sparse⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> realSize(this ref tar_package.headerGNU h) => tar_package.realSize⁔(ref h);
}
partial class tarᴛheaderSTAR {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerV7> v7(this ж<tar_package.headerSTAR> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref tar_package.headerSTAR h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref tar_package.headerSTAR h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref tar_package.headerSTAR h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref tar_package.headerSTAR h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref tar_package.headerSTAR h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref tar_package.headerSTAR h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> prefix(this ref tar_package.headerSTAR h) => tar_package.prefix⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> accessTime(this ref tar_package.headerSTAR h) => tar_package.accessTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> changeTime(this ref tar_package.headerSTAR h) => tar_package.changeTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> trailer(this ref tar_package.headerSTAR h) => tar_package.trailer⁔(ref h);
}
partial class tarᴛheaderUSTAR {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tar_package.headerV7> v7(this ж<tar_package.headerUSTAR> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref tar_package.headerUSTAR h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref tar_package.headerUSTAR h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref tar_package.headerUSTAR h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref tar_package.headerUSTAR h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref tar_package.headerUSTAR h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref tar_package.headerUSTAR h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> prefix(this ref tar_package.headerUSTAR h) => tar_package.prefix⁔(ref h);
}
partial class tarᴛheaderV7 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> name(this ref tar_package.headerV7 h) => tar_package.name⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> mode(this ref tar_package.headerV7 h) => tar_package.mode⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> uid(this ref tar_package.headerV7 h) => tar_package.uid⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> gid(this ref tar_package.headerV7 h) => tar_package.gid⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> size(this ref tar_package.headerV7 h) => tar_package.size⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> modTime(this ref tar_package.headerV7 h) => tar_package.modTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> chksum(this ref tar_package.headerV7 h) => tar_package.chksum⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> typeFlag(this ref tar_package.headerV7 h) => tar_package.typeFlag⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> linkName(this ref tar_package.headerV7 h) => tar_package.linkName⁔(ref h);
}
partial class tarᴛparser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string parseString(this ref tar_package.parser _, slice<byte> b) => tar_package.parseString⁔(ref _, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 parseNumeric(this ref tar_package.parser p, slice<byte> b) => tar_package.parseNumeric⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 parseOctal(this ref tar_package.parser p, slice<byte> b) => tar_package.parseOctal⁔(ref p, b);
}
partial class tarᴛregFileReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref tar_package.regFileReader fr, slice<byte> b) => tar_package.Read⁔(ref fr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) WriteTo(this ж<tar_package.regFileReader> Ꮡfr, io.Writer w) => tar_package.WriteTo⁔(Ꮡfr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this tar_package.regFileReader fr) => tar_package.logicalRemaining⁔(fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this tar_package.regFileReader fr) => tar_package.physicalRemaining⁔(fr);
}
partial class tarᴛregFileWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref tar_package.regFileWriter fw, slice<byte> b) => tar_package.Write⁔(ref fw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) ReadFrom(this ж<tar_package.regFileWriter> Ꮡfw, io.Reader r) => tar_package.ReadFrom⁔(Ꮡfw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this tar_package.regFileWriter fw) => tar_package.logicalRemaining⁔(fw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this tar_package.regFileWriter fw) => tar_package.physicalRemaining⁔(fw);
}
partial class tarᴛsparseArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static tar_package.sparseElem entry(this tar_package.sparseArray s, nint i) => tar_package.entry⁔(s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> isExtended(this tar_package.sparseArray s) => tar_package.isExtended⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxEntries(this tar_package.sparseArray s) => tar_package.maxEntries⁔(s);
}
partial class tarᴛsparseElem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> offset(this tar_package.sparseElem s) => tar_package.offset⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> length(this tar_package.sparseElem s) => tar_package.length⁔(s);
}
partial class tarᴛsparseEntry {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 endOffset(this tar_package.sparseEntry s) => tar_package.endOffset⁔(s);
}
partial class tarᴛsparseFileReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<tar_package.sparseFileReader> Ꮡsr, slice<byte> b) => tar_package.Read⁔(Ꮡsr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) WriteTo(this ж<tar_package.sparseFileReader> Ꮡsr, io.Writer w) => tar_package.WriteTo⁔(Ꮡsr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this tar_package.sparseFileReader sr) => tar_package.logicalRemaining⁔(sr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this tar_package.sparseFileReader sr) => tar_package.physicalRemaining⁔(sr);
}
partial class tarᴛsparseFileWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<tar_package.sparseFileWriter> Ꮡsw, slice<byte> b) => tar_package.Write⁔(Ꮡsw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this ж<tar_package.sparseFileWriter> Ꮡsw, io.Reader r) => tar_package.ReadFrom⁔(Ꮡsw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this tar_package.sparseFileWriter sw) => tar_package.logicalRemaining⁔(sw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this tar_package.sparseFileWriter sw) => tar_package.physicalRemaining⁔(sw);
}
partial class tarᴛzeroReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this tar_package.zeroReader _, slice<byte> b) => tar_package.Read⁔(_, b);
}
partial class tarᴛzeroWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this tar_package.zeroWriter _, slice<byte> b) => tar_package.Write⁔(_, b);
}
// </MethodForwarders>
