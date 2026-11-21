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
using static go.archive.tar_package;

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
[assembly: GoPositionMap("archive/tar/common.go", "common.cs", "ACA+loKCgoKCgoKCAA0KooKCgoKmgpQAY5wBAEygAQAnUsaClIKClKSkpJSUAAIU8oKCgoKClIKmAAIWAAgCgoKCgpSCgpSUggAvKAAMAoKUooLYgoKCgpSCgoKClIKClKaAgraigoKUgoKCgoKUpoKmooKUgoKUgoKCgpSCgoKCgpSmgrqSgoKCgoKCgoKCgoKCgoKClqKmgsakgoKUgqSCuIKClIKUgoKktLS2gpSCggAbOIKClJSClLS0tLaokgALEIKCgpaSgpSoxKaClIKUgriktLS0tIK0tpS0grS0tLamggAbRgALAoKUgsqUgrSCtIK0gpTGtKSUgpSClIK4poKCgoKCgoKUgoKUlIKCgoKCgpSCgqaClAAOIKKUpA==", "350-376:1;377-395:2;396-423:3", "", "787=Name/1/1/1,ModTime/1/1/2,Perm/1/1/3")]
[assembly: GoPositionMap("archive/tar/format.go", "format.cs", "AG3YAYKCgrQACQiigoKCppSkpAAgPKIACRCwoqCioKKgoprUkoKCgriCgoKUpKSkvNTYgrSCgrSCtLqigoKCrvKigpSClKiSAAkKgoKCgoKCgoIACAigooKCgoKCgoKCggAICKCigoKCgoKCgoKCAAgIoKKCgoKCgoL4goL4gg==")]
[assembly: GoPositionMap("archive/tar/reader.go", "reader.cs", "ACZOkgAEHAANAoKUgoKCgoKmpqKC/oKUgIKkgpSUgoKUgIKkppSCgoKUgoIABxC0goKCpoKUtKTqgpSClIKUgoKU3ILKgriClIIABBDSgoKUgqaCgqrCgoKClMqCgpSClK7kkpKUtLSktJSmgpSCgpSCgoKUuIKUyLKCgpSClLS0tLSCtIK0tLS0tIKClLaCpoKqwoKClJrEgoKCgpSUpoaUtLaClAAEFgALBICCpIKClIKUuIKCpoKmgoKCgoKCgoKmgoKCgoKUgpSCgriCgpSChMaCgoK0goKCgIKkgIIAGDCCgoKClLaCpgACFgAOCIKUlIKCgpSCgoKClIKUgoKClIKCgriUgpSCgpSUAAQaAAoCAAYSkoKCgpSClIKCgrjKgoKCyoKUgoLcgpSCgoKCgpSCgqaqxoKCgriCgpSCyoKCgoKClIKClJSmgoKUAAQaAAoCgpSCgpQAAhoACgKClIKClAAMErKClIKClJSkpAALCIKokqiSAA0UwoKCpoKCgoKCgoKUgpSCgoK4gpSkpKSkuMKCgoKmgqaCgoKCgoKClIKCgpSUgoLcgoKmgpSkpKS4gpSCAAkKgoLKooKClKrSgoKClIKUqqKCgpSomsLKgpSCgpS4goKU", "407-407:1;411-411:2;546-563:1;567-571:2", "", "391=Contains/1/1/2;479=modTime/1/1/1,Invoke/5/7/1,devMajor/1/1/1,Invoke/6/7/1,devMinor/1/1/1,Invoke/7/7/1")]
[assembly: GoPositionMap("archive/tar/stat_actime2.go", "stat_actime2.cs", "AAscorai")]
[assembly: GoPositionMap("archive/tar/stat_unix.go", "stat_unix.cs", "ABIkggAJDqKCgpSCgriCgoKClIKCgoKmgqaCgqSSgoLGgoKCgsaCgsaCgsaCgsaCgoLGgoKC+g==")]
[assembly: GoPositionMap("archive/tar/strconv.go", "strconv.cs", "AA4gkqiSgoKmqsKClIKCgqYADBqigpTIkoKUgoLcgoIABRbygszojNKCpoKigoKUgoKUlIKClIKUuMqigoKmgoKClIKmgsbclIKUgoKUxoKCgqaUgpTKooKssqamgoKUgsqCgqSktoKClKqikoK4goKCgpSs9IKCuIKClIKCuKKCuIKCpoKUqqKCpoKCgqaCgpQAAhoACgKClJSk")]
[assembly: GoPositionMap("archive/tar/writer.go", "writer.cs", "ACNIkgALHtKClIKUgpSCrvKClLiCgpQACBKCgoKmgpSCpIKkgqS4tJKCqJKCgoKClAAIBrIAJEimgoSkgoKClKiSgoKCgpSUgoKUgoKUgILKkoCSgoKAggAPHua0koKCgqaCgoK6koKCgoKUggAnUIKAgsiUgpSUAAocAAkClIKCpoKCgoKCgoKClIKCgoKUrNKmgoKUlIKCgoKCgoKCgoKCuIKUgqzSgIKkgIKkgpSCguyyooKUgpSCgpSCgoKCgpSClIKClIKClIKUgpSCgpSSggAIDKKCgoKCgoKmgoKCgpQABBTygpSCgpQAAhoACgKClIKClKyygpSCuIKCuIIADBKygoKUgoKUlKSkAAsIgqiSqJIADRTCgoKmgoKCgoKCgpSClIKCgriClKSkpKS4woKCgqaCpoKCgoKCgoKCgpSUgpSCgtyCgqaClKSkpKS4gqaCAAoMgoKCpsiSgpSkpA==", "209-209:1;407-449:1", "", "", "504:25")]
// </GoSourcePositionMaps>

namespace go.archive;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using fs = go.io.fs_package;
using time = time_package;
using @internal;
using go.io;
using static tar_package;
using io = io_package;
using go.path;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(tar_package.Format)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛFormat;
[GoMethodHost(typeof(tar_package.Header)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛHeader;
[GoMethodHost(typeof(tar_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛReader;
[GoMethodHost(typeof(tar_package.Writer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛWriter;
[GoMethodHost(typeof(tar_package.block)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛblock;
[GoMethodHost(typeof(tar_package.formatter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛformatter;
[GoMethodHost(typeof(tar_package.headerError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderError;
[GoMethodHost(typeof(tar_package.headerFileInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderFileInfo;
[GoMethodHost(typeof(tar_package.headerGNU)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderGNU;
[GoMethodHost(typeof(tar_package.headerSTAR)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderSTAR;
[GoMethodHost(typeof(tar_package.headerUSTAR)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderUSTAR;
[GoMethodHost(typeof(tar_package.headerV7)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛheaderV7;
[GoMethodHost(typeof(tar_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛparser;
[GoMethodHost(typeof(tar_package.regFileReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛregFileReader;
[GoMethodHost(typeof(tar_package.regFileWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛregFileWriter;
[GoMethodHost(typeof(tar_package.sparseArray)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛsparseArray;
[GoMethodHost(typeof(tar_package.sparseElem)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛsparseElem;
[GoMethodHost(typeof(tar_package.sparseEntry)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛsparseEntry;
[GoMethodHost(typeof(tar_package.sparseFileReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛsparseFileReader;
[GoMethodHost(typeof(tar_package.sparseFileWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛsparseFileWriter;
[GoMethodHost(typeof(tar_package.zeroReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛzeroReader;
[GoMethodHost(typeof(tar_package.zeroWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tarᴛzeroWriter;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class tarᴛheaderError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this headerError he) => tar_package.Errorᴍ(he);
}
partial class tarᴛsparseEntry {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 endOffset(this sparseEntry s) => tar_package.endOffsetᴍ(s);
}
partial class tarᴛHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Format format, map<@string, @string> paxHdrs, error err) allowedFormats(this Header h) => tar_package.allowedFormatsᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static fs.FileInfo FileInfo(this ж<Header> Ꮡh) => tar_package.FileInfoᴍ(Ꮡh);
}
partial class tarᴛheaderFileInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this headerFileInfo fi) => tar_package.Sizeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this headerFileInfo fi) => tar_package.IsDirᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this headerFileInfo fi) => tar_package.ModTimeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this headerFileInfo fi) => tar_package.Sysᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this headerFileInfo fi) => tar_package.Nameᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode /*mode*/ Mode(this headerFileInfo fi) => tar_package.Modeᴍ(fi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this headerFileInfo fi) => tar_package.Stringᴍ(fi);
}
partial class tarᴛFormat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool has(this Format f, Format f2) => tar_package.hasᴍ(f, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mayBe(this ref Format f, Format f2) => tar_package.mayBeᴍ(ref f, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mayOnlyBe(this ref Format f, Format f2) => tar_package.mayOnlyBeᴍ(ref f, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mustNotBe(this ref Format f, Format f2) => tar_package.mustNotBeᴍ(ref f, f2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Format f) => tar_package.Stringᴍ(f);
}
partial class tarᴛblock {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerV7> toV7(this ж<block> Ꮡb) => tar_package.toV7ᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerGNU> toGNU(this ж<block> Ꮡb) => tar_package.toGNUᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerSTAR> toSTAR(this ж<block> Ꮡb) => tar_package.toSTARᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerUSTAR> toUSTAR(this ж<block> Ꮡb) => tar_package.toUSTARᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sparseArray toSparse(this ref block b) => tar_package.toSparseᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Format getFormat(this ж<block> Ꮡb) => tar_package.getFormatᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setFormat(this ж<block> Ꮡb, Format format) => tar_package.setFormatᴍ(Ꮡb, format);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 unsigned, int64 signed) computeChecksum(this ref block b) => tar_package.computeChecksumᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref block b) => tar_package.resetᴍ(ref b);
}
partial class tarᴛheaderV7 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> name(this ref headerV7 h) => tar_package.nameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> mode(this ref headerV7 h) => tar_package.modeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> uid(this ref headerV7 h) => tar_package.uidᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> gid(this ref headerV7 h) => tar_package.gidᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> size(this ref headerV7 h) => tar_package.sizeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> modTime(this ref headerV7 h) => tar_package.modTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> chksum(this ref headerV7 h) => tar_package.chksumᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> typeFlag(this ref headerV7 h) => tar_package.typeFlagᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> linkName(this ref headerV7 h) => tar_package.linkNameᴍ(ref h);
}
partial class tarᴛheaderGNU {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerGNU> Ꮡh) => tar_package.v7ᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> magic(this ref headerGNU h) => tar_package.magicᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> version(this ref headerGNU h) => tar_package.versionᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> userName(this ref headerGNU h) => tar_package.userNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> groupName(this ref headerGNU h) => tar_package.groupNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerGNU h) => tar_package.devMajorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerGNU h) => tar_package.devMinorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> accessTime(this ref headerGNU h) => tar_package.accessTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> changeTime(this ref headerGNU h) => tar_package.changeTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sparseArray sparse(this ref headerGNU h) => tar_package.sparseᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> realSize(this ref headerGNU h) => tar_package.realSizeᴍ(ref h);
}
partial class tarᴛheaderSTAR {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerSTAR> Ꮡh) => tar_package.v7ᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> magic(this ref headerSTAR h) => tar_package.magicᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> version(this ref headerSTAR h) => tar_package.versionᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> userName(this ref headerSTAR h) => tar_package.userNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> groupName(this ref headerSTAR h) => tar_package.groupNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerSTAR h) => tar_package.devMajorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerSTAR h) => tar_package.devMinorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> prefix(this ref headerSTAR h) => tar_package.prefixᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> accessTime(this ref headerSTAR h) => tar_package.accessTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> changeTime(this ref headerSTAR h) => tar_package.changeTimeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> trailer(this ref headerSTAR h) => tar_package.trailerᴍ(ref h);
}
partial class tarᴛheaderUSTAR {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<headerV7> v7(this ж<headerUSTAR> Ꮡh) => tar_package.v7ᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> magic(this ref headerUSTAR h) => tar_package.magicᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> version(this ref headerUSTAR h) => tar_package.versionᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> userName(this ref headerUSTAR h) => tar_package.userNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> groupName(this ref headerUSTAR h) => tar_package.groupNameᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMajor(this ref headerUSTAR h) => tar_package.devMajorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> devMinor(this ref headerUSTAR h) => tar_package.devMinorᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> prefix(this ref headerUSTAR h) => tar_package.prefixᴍ(ref h);
}
partial class tarᴛsparseArray {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sparseElem entry(this sparseArray s, nint i) => tar_package.entryᴍ(s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> isExtended(this sparseArray s) => tar_package.isExtendedᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxEntries(this sparseArray s) => tar_package.maxEntriesᴍ(s);
}
partial class tarᴛsparseElem {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> offset(this sparseElem s) => tar_package.offsetᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> length(this sparseElem s) => tar_package.lengthᴍ(s);
}
partial class tarᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Header>, error) Next(this ж<Reader> Ꮡtr) => tar_package.Nextᴍ(Ꮡtr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Header>, error) next(this ж<Reader> Ꮡtr) => tar_package.nextᴍ(Ꮡtr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error handleRegularFile(this ref Reader tr, ж<Header> Ꮡhdr) => tar_package.handleRegularFileᴍ(ref tr, Ꮡhdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error handleSparseFile(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> rawHdr) => tar_package.handleSparseFileᴍ(ref tr, Ꮡhdr, rawHdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (sparseDatas, error) readGNUSparsePAXHeaders(this ref Reader tr, ж<Header> Ꮡhdr) => tar_package.readGNUSparsePAXHeadersᴍ(ref tr, Ꮡhdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Header>, ж<block>, error) readHeader(this ж<Reader> Ꮡtr) => tar_package.readHeaderᴍ(Ꮡtr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (sparseDatas, error) readOldGNUSparseMap(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> Ꮡblk) => tar_package.readOldGNUSparseMapᴍ(ref tr, Ꮡhdr, Ꮡblk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Read(this ref Reader tr, slice<byte> b) => tar_package.Readᴍ(ref tr, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) writeTo(this ref Reader tr, io.Writer w) => tar_package.writeToᴍ(ref tr, w);
}
partial class tarᴛregFileReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref regFileReader fr, slice<byte> b) => tar_package.Readᴍ(ref fr, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) WriteTo(this ж<regFileReader> Ꮡfr, io.Writer w) => tar_package.WriteToᴍ(Ꮡfr, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 logicalRemaining(this regFileReader fr) => tar_package.logicalRemainingᴍ(fr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 physicalRemaining(this regFileReader fr) => tar_package.physicalRemainingᴍ(fr);
}
partial class tarᴛsparseFileReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<sparseFileReader> Ꮡsr, slice<byte> b) => tar_package.Readᴍ(Ꮡsr, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) WriteTo(this ж<sparseFileReader> Ꮡsr, io.Writer w) => tar_package.WriteToᴍ(Ꮡsr, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 logicalRemaining(this sparseFileReader sr) => tar_package.logicalRemainingᴍ(sr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 physicalRemaining(this sparseFileReader sr) => tar_package.physicalRemainingᴍ(sr);
}
partial class tarᴛzeroReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this zeroReader _, slice<byte> b) => tar_package.Readᴍ(_, b);
}
partial class tarᴛparser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string parseString(this ref parser _, slice<byte> b) => tar_package.parseStringᴍ(ref _, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 parseNumeric(this ref parser p, slice<byte> b) => tar_package.parseNumericᴍ(ref p, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 parseOctal(this ref parser p, slice<byte> b) => tar_package.parseOctalᴍ(ref p, b);
}
partial class tarᴛformatter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void formatString(this ref formatter f, slice<byte> b, @string s) => tar_package.formatStringᴍ(ref f, b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void formatNumeric(this ref formatter f, slice<byte> b, int64 x) => tar_package.formatNumericᴍ(ref f, b, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void formatOctal(this ref formatter f, slice<byte> b, int64 x) => tar_package.formatOctalᴍ(ref f, b, x);
}
partial class tarᴛWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Flush(this ref Writer tw) => tar_package.Flushᴍ(ref tw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.WriteHeaderᴍ(Ꮡtw, Ꮡhdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeUSTARHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.writeUSTARHeaderᴍ(Ꮡtw, Ꮡhdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writePAXHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr, map<@string, @string> paxHdrs) => tar_package.writePAXHeaderᴍ(Ꮡtw, Ꮡhdr, paxHdrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeGNUHeader(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr) => tar_package.writeGNUHeaderᴍ(Ꮡtw, Ꮡhdr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<block> templateV7Plus(this ж<Writer> Ꮡtw, ж<Header> Ꮡhdr, Action<slice<byte>, @string> fmtStr, Action<slice<byte>, int64> fmtNum) => tar_package.templateV7Plusᴍ(Ꮡtw, Ꮡhdr, fmtStr, fmtNum);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeRawFile(this ж<Writer> Ꮡtw, @string name, @string data, byte flag, Format format) => tar_package.writeRawFileᴍ(Ꮡtw, name, data, flag, format);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeRawHeader(this ref Writer tw, ж<block> Ꮡblk, int64 size, byte flag) => tar_package.writeRawHeaderᴍ(ref tw, Ꮡblk, size, flag);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error AddFS(this ж<Writer> Ꮡtw, fs.FS fsys) => tar_package.AddFSᴍ(Ꮡtw, fsys);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Write(this ref Writer tw, slice<byte> b) => tar_package.Writeᴍ(ref tw, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) readFrom(this ref Writer tw, io.Reader r) => tar_package.readFromᴍ(ref tw, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Writer tw) => tar_package.Closeᴍ(ref tw);
}
partial class tarᴛregFileWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref regFileWriter fw, slice<byte> b) => tar_package.Writeᴍ(ref fw, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) ReadFrom(this ж<regFileWriter> Ꮡfw, io.Reader r) => tar_package.ReadFromᴍ(Ꮡfw, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 logicalRemaining(this regFileWriter fw) => tar_package.logicalRemainingᴍ(fw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 physicalRemaining(this regFileWriter fw) => tar_package.physicalRemainingᴍ(fw);
}
partial class tarᴛsparseFileWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<sparseFileWriter> Ꮡsw, slice<byte> b) => tar_package.Writeᴍ(Ꮡsw, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this ж<sparseFileWriter> Ꮡsw, io.Reader r) => tar_package.ReadFromᴍ(Ꮡsw, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 logicalRemaining(this sparseFileWriter sw) => tar_package.logicalRemainingᴍ(sw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 physicalRemaining(this sparseFileWriter sw) => tar_package.physicalRemainingᴍ(sw);
}
partial class tarᴛzeroWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this zeroWriter _, slice<byte> b) => tar_package.Writeᴍ(_, b);
}
// </GlyphForwarders>
