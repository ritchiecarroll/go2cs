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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("archive/tar/common.go", "common.cs", "AB8+loKCgoKCgoKCAAkKgoKCgoKmgpQAVJwBAEqgAYAABlLGgpSCgpSkpKSklAACFPKCgoKCgpSCpgACFgAIAoKCgoKUgoKUlIIAHSgADAKChKKC2IKCgoKUgoKAgoKkgoKUpoCCtqKCgpSCgoKCgpSmgIK2ooKUgoKUgoKCgpSCgoKCgpSmgIK6koKCgoKCgoKCgoKCgoKCgoailpK2pIKClIKkgqiCgpSClIKCgKS0tNaClIKCABk4gIKClKSClKSkpLaokgAHEICigKKAooCmkoKUqMSWgpSClIKogKSkpKSkgqS2lKSCpKSkpqaCABNGAAsCgpSC2pSCtIK0grSClMa0pKSClIKUgriApoKCgoKCgoKUgoKUpIKAgoKCgoKUgoK2gpQADiCilKQ=", "350-376:1;377-395:2;396-423:3")]
[assembly: go.GoPositionMap("archive/tar/format.go", "format.cs", "ABzYAYCigKKAooCk6IKCgoKmlKSkABYwogAHELCioKKgoqCigKrUkoKCgqiCgoKUpKSkzNTIkrSCgrSCtLqigoKCrvKigpSClKiS2oCigKKAooCigKKAooCigKKA2KCigKKAooCigKKAooCigKKAooCigNigooCigKKAooCigKKAooCigKKAooDYoKKAooCigKKAooCigKKAyICigKKAyICigA==")]
[assembly: go.GoPositionMap("archive/tar/reader.go", "reader.cs", "ACVOkgACHAANAoKUgoKCgoKmpqKC7oKUgIKkgIKkhIKClICCpJaUgoKClIKCAAcQpIKCgpaClKSkqoCipIKUgpSCgpSsgKKqgKKokpSCAAUQ0pKClIKWgoKqwoKCgpS6goKUgpSu5JKSlLS0pLSkloCCpIKClIKCgpSogpSosoKClIKUpKSkpIKkgqSkpKSkgoKUtoKmgqqigoKUirSCgoKClISWlpSktoKUAAIWAAsEgIKkgoCCpIKUqIKCloKWgoKCgoKCgoKWgoKCgoKEgpSCgoigkoKUgoTGgoKCtIKCgoCCpICCAAIwABUCgoKAgqTGgqYAAhYADgiClISCgoKUgoKClIKUgoKClJaUgIKkgpQAAxwACgIABRKSgoKClICCpIKCgri6goKCuoCCpIKCzICCpIKCgoKClJSqxoKCgqiCgpSCuoKCgoKClIKUAAIaAAoCgpSCgpQAAhoACgKClIKClAAIErKClIKClJSkpAAICIKokqiSAAkUsoKCloKCgoKCkoKkgpSCgoKogpSkpKSkyMKCgoCCtoKWgoKCgoKSgqSCgoKUlIKCzIKCloKUpKSkyIKkgtqCgqqigoKUqtKCgoKUgpSqooKClKiawoDKgpSCgpS4goKU", "407-407:1;411-411:2;539-556:1;560-564:2")]
[assembly: go.GoPositionMap("archive/tar/strconv.go", "strconv.cs", "AA0gkqiSgoKmqqKClIKCgqYAChqigIKkqJKClIKCzIKCAAMW8oKs6IzSgpaCooKClIKClJSCgpSClKiqooKCloKCgpSCloKm3ISClIKClKaCgoKWlICCpKqigqyylpaCgpSCqIKUgpSUgoKUqqKSgqiCgoKClKz0goKogoKUgoKoooKogoKWgpSqooKWgoKCloKClAACGgAKAoKUlKQ=")]
[assembly: go.GoPositionMap("archive/tar/writer.go", "writer.cs", "ACJIkgAJHtKClICCpICCpIKu8oCCpKiCgpQABxKCgoKWgpSCpIKkgqTItJKAgqiSgoKCgpTmsgAjSJaChKSCgoKUmJKCgoKClJSCgpSCgpSAgrqSgJKCgoCCAA4e1rSSgoKAgraCgoCCupKCgoKClIIAJ1CCgIK4lICCpJQABhwACQKEgoKWgoKCgoKCgoKEgoKCgoSs0paCgpSEgoKCgoKCgoKCgoKogIKkgqzSgIKkgIKkgpSCgtyyooKUgpSCgqaClIKClIKClICCpIKUgoKUkoLsooKCpKSWgoKCgpQAAhTygpSCgpQAAhoACgKClIKClKyygpSCqIKCqIIACBKygoKUgoKUlKSkAAgIgqiSqJIACRSygoKWgoKCgoKSgqSClIKCgqiClKSkpKTIwoKCgIK2gpaCgoKCgpKCgoKUpIKUgoLMgoKWgpSkpKSkyIKmguyCgoKmqJKClKSk", "209-209:1;407-443:1")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("tar")]
public static partial class tar_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
