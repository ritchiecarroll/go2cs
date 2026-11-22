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
[assembly: GoPositionMap("archive/tar/strconv.go", "strconv.cs", "AA0gkqiSgoKmqsKClIKCgqYAChqigpSokoKUgoLcgoIAAxbygqzojNKCpoKigoKUgoKUlIKClIKUuKqigoKmgoKClIKmgqbclIKUgoKUpoKCgqaUgpSqooKssqamgoKUgsqCgqSktoKClKqikoK4goKCgpSs9IKCuIKClIKCuKKCuIKCpoKUqqKCpoKCgqaCgpQAAhoACgKClJSk")]
[assembly: GoPositionMap("archive/tar/writer.go", "writer.cs", "ACJIkgAJHtKClIKUgpSCrvKClLiCgpQACBKCgoKmgpSCpIKkgqS4tJKCqJKCgoKClAAIBrIAJEimgoSkgoKClKiSgoKCgpSUgoKUgoKUgILKkoCSgoKAggAPHua0koKCgqaCgoK6koKCgoKUggAnUIKAgsiUgpSUAAYcAAkClIKCpoKCgoKCgoKClIKCgoKUrNKmgoKUlIKCgoKCgoKCgoKCuIKUgqzSgIKkgIKkgpSCguyyooKUgpSCgpSCgoKCgpSClIKClIKClIKUgpSCgpSSguyigoKCgoKCpoKCgoKUAAIU8oKUgoKUAAIaAAoCgpSCgpSssoKUgriCgriCAAgSsoKClIKClJSkpPiCqJKokgAJFMKCgqaCgoKCgoKClIKUgoKCuIKUpKSkpLjCgoKCpoKmgoKCgoKCgoKClJSClIKC3IKCpoKUpKSkpLiCpoLsgoKCpqiSgpSkpA==", "209-209:1;407-449:1", "", "", "497:25")]
// </GoSourcePositionMaps>

namespace go.archive;

using static go.archive.tar_package;
using fs = go.io.fs_package;
using io = go.io_package;
using time = go.time_package;

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
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool has(this Format f, Format f2) => tar_package.has⁔(f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mayBe(this ref Format f, Format f2) => tar_package.mayBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mayOnlyBe(this ref Format f, Format f2) => tar_package.mayOnlyBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mustNotBe(this ref Format f, Format f2) => tar_package.mustNotBe⁔(ref f, f2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Format f) => tar_package.String⁔(f);
}
partial class tarᴛHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Format format, map<@string, @string> paxHdrs, error err) allowedFormats(this Header h) => tar_package.allowedFormats⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static fs.FileInfo FileInfo(this ж<Header> Ꮡh) => tar_package.FileInfo⁔(Ꮡh);
}
partial class tarᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Header>, error) Next(this ж<Reader> Ꮡtr) => tar_package.Next⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Header>, error) next(this ж<Reader> Ꮡtr) => tar_package.next⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleRegularFile(this ref Reader tr, ж<Header> Ꮡhdr) => tar_package.handleRegularFile⁔(ref tr, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleSparseFile(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> ᏑrawHdr) => tar_package.handleSparseFile⁔(ref tr, Ꮡhdr, ᏑrawHdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (sparseDatas, error) readGNUSparsePAXHeaders(this ref Reader tr, ж<Header> Ꮡhdr) => tar_package.readGNUSparsePAXHeaders⁔(ref tr, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Header>, ж<block>, error) readHeader(this ж<Reader> Ꮡtr) => tar_package.readHeader⁔(Ꮡtr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (sparseDatas, error) readOldGNUSparseMap(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> Ꮡblk) => tar_package.readOldGNUSparseMap⁔(ref tr, Ꮡhdr, Ꮡblk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this ref Reader tr, slice<byte> b) => tar_package.Read⁔(ref tr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) writeTo(this ref Reader tr, io.Writer w) => tar_package.writeTo⁔(ref tr, w);
}
partial class tarᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref Writer tw) => tar_package.Flush⁔(ref tw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.WriteHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeUSTARHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.writeUSTARHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writePAXHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr, map<@string, @string> paxHdrs) => tar_package.writePAXHeader⁔(Ꮡtw, Ꮡhdr, paxHdrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeGNUHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.writeGNUHeader⁔(Ꮡtw, Ꮡhdr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<block> templateV7Plus(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr, Action<slice<byte>, @string> fmtStr, Action<slice<byte>, int64> fmtNum) => tar_package.templateV7Plus⁔(Ꮡtw, Ꮡhdr, fmtStr, fmtNum);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeRawFile(this ж<Writer> Ꮡtw, @string name, @string data, byte flag, Format format) => tar_package.writeRawFile⁔(Ꮡtw, name, data, flag, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeRawHeader(this ref Writer tw, ж<block> Ꮡblk, int64 size, byte flag) => tar_package.writeRawHeader⁔(ref tw, Ꮡblk, size, flag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddFS(this ж<Writer> Ꮡtw, fs.FS fsys) => tar_package.AddFS⁔(Ꮡtw, fsys);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ref Writer tw, slice<byte> b) => tar_package.Write⁔(ref tw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) readFrom(this ref Writer tw, io.Reader r) => tar_package.readFrom⁔(ref tw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Writer tw) => tar_package.Close⁔(ref tw);
}
partial class tarᴛblock {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerV7> toV7(this ж<block> Ꮡb) => tar_package.toV7⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerGNU> toGNU(this ж<block> Ꮡb) => tar_package.toGNU⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerSTAR> toSTAR(this ж<block> Ꮡb) => tar_package.toSTAR⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerUSTAR> toUSTAR(this ж<block> Ꮡb) => tar_package.toUSTAR⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sparseArray toSparse(this ref block b) => tar_package.toSparse⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Format getFormat(this ж<block> Ꮡb) => tar_package.getFormat⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setFormat(this ж<block> Ꮡb, Format format) => tar_package.setFormat⁔(Ꮡb, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 unsigned, int64 signed) computeChecksum(this ref block b) => tar_package.computeChecksum⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref block b) => tar_package.reset⁔(ref b);
}
partial class tarᴛformatter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatString(this ref formatter f, slice<byte> b, @string s) => tar_package.formatString⁔(ref f, b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatNumeric(this ref formatter f, slice<byte> b, int64 x) => tar_package.formatNumeric⁔(ref f, b, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void formatOctal(this ref formatter f, slice<byte> b, int64 x) => tar_package.formatOctal⁔(ref f, b, x);
}
partial class tarᴛheaderError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this headerError he) => tar_package.Error⁔(he);
}
partial class tarᴛheaderFileInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Size(this headerFileInfo fi) => tar_package.Size⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsDir(this headerFileInfo fi) => tar_package.IsDir⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time ModTime(this headerFileInfo fi) => tar_package.ModTime⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Sys(this headerFileInfo fi) => tar_package.Sys⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this headerFileInfo fi) => tar_package.Name⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode /*mode*/ Mode(this headerFileInfo fi) => tar_package.Mode⁔(fi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this headerFileInfo fi) => tar_package.String⁔(fi);
}
partial class tarᴛheaderGNU {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerGNU> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref headerGNU h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref headerGNU h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref headerGNU h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref headerGNU h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerGNU h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerGNU h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> accessTime(this ref headerGNU h) => tar_package.accessTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> changeTime(this ref headerGNU h) => tar_package.changeTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sparseArray sparse(this ref headerGNU h) => tar_package.sparse⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> realSize(this ref headerGNU h) => tar_package.realSize⁔(ref h);
}
partial class tarᴛheaderSTAR {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerSTAR> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref headerSTAR h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref headerSTAR h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref headerSTAR h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref headerSTAR h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerSTAR h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerSTAR h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> prefix(this ref headerSTAR h) => tar_package.prefix⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> accessTime(this ref headerSTAR h) => tar_package.accessTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> changeTime(this ref headerSTAR h) => tar_package.changeTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> trailer(this ref headerSTAR h) => tar_package.trailer⁔(ref h);
}
partial class tarᴛheaderUSTAR {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerUSTAR> Ꮡh) => tar_package.v7⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> magic(this ref headerUSTAR h) => tar_package.magic⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> version(this ref headerUSTAR h) => tar_package.version⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> userName(this ref headerUSTAR h) => tar_package.userName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> groupName(this ref headerUSTAR h) => tar_package.groupName⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerUSTAR h) => tar_package.devMajor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerUSTAR h) => tar_package.devMinor⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> prefix(this ref headerUSTAR h) => tar_package.prefix⁔(ref h);
}
partial class tarᴛheaderV7 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> name(this ref headerV7 h) => tar_package.name⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> mode(this ref headerV7 h) => tar_package.mode⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> uid(this ref headerV7 h) => tar_package.uid⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> gid(this ref headerV7 h) => tar_package.gid⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> size(this ref headerV7 h) => tar_package.size⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> modTime(this ref headerV7 h) => tar_package.modTime⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> chksum(this ref headerV7 h) => tar_package.chksum⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> typeFlag(this ref headerV7 h) => tar_package.typeFlag⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> linkName(this ref headerV7 h) => tar_package.linkName⁔(ref h);
}
partial class tarᴛparser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string parseString(this ref parser _, slice<byte> b) => tar_package.parseString⁔(ref _, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 parseNumeric(this ref parser p, slice<byte> b) => tar_package.parseNumeric⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 parseOctal(this ref parser p, slice<byte> b) => tar_package.parseOctal⁔(ref p, b);
}
partial class tarᴛregFileReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref regFileReader fr, slice<byte> b) => tar_package.Read⁔(ref fr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) WriteTo(this ж<regFileReader> Ꮡfr, io.Writer w) => tar_package.WriteTo⁔(Ꮡfr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this regFileReader fr) => tar_package.logicalRemaining⁔(fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this regFileReader fr) => tar_package.physicalRemaining⁔(fr);
}
partial class tarᴛregFileWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref regFileWriter fw, slice<byte> b) => tar_package.Write⁔(ref fw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) ReadFrom(this ж<regFileWriter> Ꮡfw, io.Reader r) => tar_package.ReadFrom⁔(Ꮡfw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this regFileWriter fw) => tar_package.logicalRemaining⁔(fw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this regFileWriter fw) => tar_package.physicalRemaining⁔(fw);
}
partial class tarᴛsparseArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sparseElem entry(this sparseArray s, nint i) => tar_package.entry⁔(s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> isExtended(this sparseArray s) => tar_package.isExtended⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxEntries(this sparseArray s) => tar_package.maxEntries⁔(s);
}
partial class tarᴛsparseElem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> offset(this sparseElem s) => tar_package.offset⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> length(this sparseElem s) => tar_package.length⁔(s);
}
partial class tarᴛsparseEntry {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 endOffset(this sparseEntry s) => tar_package.endOffset⁔(s);
}
partial class tarᴛsparseFileReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<sparseFileReader> Ꮡsr, slice<byte> b) => tar_package.Read⁔(Ꮡsr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) WriteTo(this ж<sparseFileReader> Ꮡsr, io.Writer w) => tar_package.WriteTo⁔(Ꮡsr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this sparseFileReader sr) => tar_package.logicalRemaining⁔(sr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this sparseFileReader sr) => tar_package.physicalRemaining⁔(sr);
}
partial class tarᴛsparseFileWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<sparseFileWriter> Ꮡsw, slice<byte> b) => tar_package.Write⁔(Ꮡsw, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this ж<sparseFileWriter> Ꮡsw, io.Reader r) => tar_package.ReadFrom⁔(Ꮡsw, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 logicalRemaining(this sparseFileWriter sw) => tar_package.logicalRemaining⁔(sw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 physicalRemaining(this sparseFileWriter sw) => tar_package.physicalRemaining⁔(sw);
}
partial class tarᴛzeroReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this zeroReader _, slice<byte> b) => tar_package.Read⁔(_, b);
}
partial class tarᴛzeroWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this zeroWriter _, slice<byte> b) => tar_package.Write⁔(_, b);
}
// </MethodForwarders>
