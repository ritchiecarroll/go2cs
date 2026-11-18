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
[assembly: GoPositionMap("archive/tar/reader.go", "reader.cs", "ACZOkgAEHAANAoKUgoKCgoKmpqKC/oKUgIKkgpSUgoKUgIKkppSCgoKUgoIABxC0goKCpoKUtKTqgpSClIKUgoKU3ILKgriClIIABBDSgoKUgqaCgqrCgoKClMqCgpSClK7kkpKUtLSktJSmgpSCgpSCgoKUuIKUyLKCgpSCpLS0tLSCtIK0tLS0tIKClLaCpoKqwoKClJrEgoKCgpSUtoaUtLaClAAEFgALBICCpIKClIKUuIKCpoKmgoKCgoKCgoKmgoKCgoKUgpSCgriCgpSChMaCgoK0goKCgIKkgIIAGDCCgoKClLaCpgACFgAOCIKUlIKCgpSCgoKClIKUgoKClIKCgriUgpSCgpSUAAQaAAoCAAYSkoKCgpSClIKCgrjKgoKCyoKUgoLcgpSCgoKCgpSCgqaqxoKCgriCgpSCyoKCgoKClIKClJSmgoKUAAQaAAoCgpSCgpQAAhoACgKClIKClAAMErKClIKClJSkpAALCIKokqiSAA0UwoKCpoKCgoKCgoKUgpSCgoK4gpSkpKSkuMKCgoKmgqaCgoKCgoKClIKCgpSUgoLcgoKmgpSkpKS4gpSCAAkKgoLKooKClKrSgoKClIKUqqKCgpSomsLKgpSCgpS4goKU", "407-407:1;411-411:2;546-563:1;567-571:2", "", "393=Contains/1/1/2;481=modTime/1/1/1,Invoke/5/7/1,devMajor/1/1/1,Invoke/6/7/1,devMinor/1/1/1,Invoke/7/7/1")]
[assembly: GoPositionMap("archive/tar/stat_actime1.go", "stat_actime1.cs", "AAscorai")]
[assembly: GoPositionMap("archive/tar/stat_unix.go", "stat_unix.cs", "ABIkggAJDqKCgpSCgriCgoKClIKCgoKmgqaCgqSSgoLGgoKCgsaCgsaCgsaCgsaCgoLGgoKC+g==")]
[assembly: GoPositionMap("archive/tar/strconv.go", "strconv.cs", "AA4gkqiSgoKmqsKClIKCgqYADBqigpTIkoKUgoLcgoIABRbygszojNKCpoKigoKUgoKUlIKClIKUuMqigoKmgoKClIKmgsbclIKUgoKUxoKCgqaUgpTKooKssqamgoKUgsqCgqSktoKClKqikoK4goKCgpSs9IKCuIKClIKCuKKCuIKCpoKUqqKCpoKCgqaCgpQAAhoACgKClKSk")]
[assembly: GoPositionMap("archive/tar/writer.go", "writer.cs", "ACNIkgALHtKClIKUgpSCrvKClLiCgpQACBKCgoKmgpSCpIKkgqS4tJKCqJKCgoKClAAIBrIAJEimgoSkgoKClKiSgoKCgpSUgoKUgoKUgILKkoCSgoKAggAPHua0koKCgqaCgoK6koKCgoKUggAnUIKAgsiUgpSUAAocAAkClIKCpoKCgoKCgoKClIKCgoKUrNKmgoKUlIKCgoKCgoKCgoKCuIKUgqzSgIKkgIKkgpSCguyyooKUgpSCgpSCgoKCgpSClIKClIKClIKUgpSCgpSSggAIDKKCgoKCgoKmgoKCgpQABBTygpSCgpQAAhoACgKClIKClKyygpSCuIKCuIIADBKygoKUgoKUlKSkAAsIgqiSqJIADRTCgoKmgoKCgoKCgpSClIKCgriClKSkpKS4woKCgqaCpoKCgoKCgoKCgpSUgpSCgtyCgqaClKSkpKS4gqaCAAoMgoKCpsiSgpSkpA==", "209-209:1;407-449:1", "", "", "504:25")]
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
