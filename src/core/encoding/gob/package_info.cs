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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static go.encoding.gob_package;

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
[assembly: GoDynamicTypeLift("7374727563747b723120696e747d", "eᴛ7")]
[assembly: GoDynamicTypeLift("7374727563747b723220696e747d", "eᴛ6")]
[assembly: GoDynamicTypeLift("7374727563747b723320696e747d", "eᴛ5")]
[assembly: GoDynamicTypeLift("7374727563747b723420696e747d", "eᴛ4")]
[assembly: GoDynamicTypeLift("7374727563747b723520696e747d", "eᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b723620696e747d", "eᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b723720696e747d", "eᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CommonType, ΔgobType>(Pointer = true)]
[assembly: GoImplement<arrayType, ΔgobType>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<decBuffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<encBuffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gobEncoderType, ΔgobType>(Pointer = true)]
[assembly: GoImplement<mapType, ΔgobType>(Pointer = true)]
[assembly: GoImplement<sliceType, ΔgobType>(Pointer = true)]
[assembly: GoImplement<structType, ΔgobType>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<decInstr, ж<decInstr>>(Indirect = true)]
[assembly: GoImplicitConv<decoderState, ж<decoderState>>(Indirect = true)]
[assembly: GoImplicitConv<encEngine, ж<encEngine>>(Indirect = true)]
[assembly: GoImplicitConv<userTypeInfo, ж<userTypeInfo>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/gob/dec_helpers.go", "dec_helpers.cs", "AAwcABMmABImlIKUpqKSlJSCgpSUlJSmlIKUpqKSlJSCgpSUlIKClKaUgpSmopKUlIKClJSUgoKUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSUgpSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSCgoKUgqaCgpSClKaUgpSmopKUlIKClJSUuJSmlIKUpqKSlJSCgpSUlIKClJSmlIKUpqKSlJSCgpSUlIKClJSmlIKUpqKSlJSCgpSUlJSmlIKUpqKSlJSCgpSUlIKClJSqooKCgoKClIKC")]
[assembly: go.GoPositionMap("encoding/gob/decode.go", "decode.cs", "ABIogoIAFS6CgoKUgqaCgpSmgoKUgoKmgqaCqJKCpoKCqsKCgoKUlIKmooKmgqrigoKClIKClIKCgpSCgoKUpoKUgqrCgoKUgpSCgpSCgriClIKqooKClKyygoKUAAwesqrCggACHAALAoKClJSosqjCgoKUqMKCgpSowoKClKjCgoKUqMKCgpSowoKClKiygqiyggACENKCrsKCgoKmgpSq0qrCrOKCgqzSgoKs0oKClIKCgpSCgoKC3LiCgpSCgoCCpKaCgIIABRDSgoKmgoKUgoKosoKClIKClAALHvKCgoKClIKirgAJAoKCgoKCgpSSlJKUgoKClIKCpoK0ytKCgoKCgoKUkpSCgpSCgrSo4oKCgoKClIKiprKClIKCgoKClKaCgoKUgpSCgpS+0oCCpKiSgoKWgq7igoKUgoKCgoKCgoKCgoLqsoKCgpS6soCCpKiygoKCgoK8woKCgoKUppSCgoKUlJSosqz0gpKUgpSCgpSUgpSCpoKClJaCgriUgoKCuIKmqNSCgpSCgpSCgoKmgoKUquSCgpSCgpSCgpSUpKSkgrrEgoKUgoKUuAASJAAJGMKUgrqAgqSCgoKClIKUgKSCgoKCgsK4goKCgoLCyIKCgpSCgIKUpIKCgrLKgpKClKS2ktiClPzkgoCSgriAgqSSgoKmgpSmgpS0goKiyIKCgoKiyIKCksqCkpSkyILYgpTawoKCpIKmgqSClJSuwoCCpIKC7oaUkpSAqqSkpKSkpKSSlIKkgpSCppKWkoCCpKSCpAACUwAFXtKCgoCUpNrygoKCgoKUgpSUgoKCgqiSgoKCgoKCqgAKAoKCgoKUpoCClIKClKSClIKClIKCgpSUlIKCgpSClIKClNjigoKCgpSAlIKCgoK27JbSgoCUgoKCgpSUgrao0pSCgqaCgoKCgpSCgoCCgoSClJTEprKCgoKUgoKUAAYSgpKUgqSCpKSClpSkpKSuwg==", "855-857:1;865-867:2;884-886:3;895-898:4;900-902:5;917-917:1;932-934:2;945-947:3;954-956:4;961-963:5;971-974:6;977-979:7;1000-1006:1")]
[assembly: go.GoPositionMap("encoding/gob/decoder.go", "decoder.cs", "AClaspSAgqSCgoKChNjEgoKogoKCpqaYtIKCgpSCgpSCqJKUlpKCgoK6koKClKaigoKUpqKCgpQABRgACwKCgoLKgpS4gpSmgoLcgoKClJSUAAUUAAkCgpSmgoKUAAUSAAsCgra4goSCgoKClA==")]
[assembly: go.GoPositionMap("encoding/gob/enc_helpers.go", "enc_helpers.cs", "AAoaABImABImlIKUpqKClJSCgoKUuKaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgqamlIKUpqKClJSCgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpg==")]
[assembly: go.GoPositionMap("encoding/gob/encode.go", "encode.cs", "ACtUlIKCyIKmgoKmgqaCpoKmgoKUuLKCgoKUlIKCgoKUpqKCAAIS4oKCloKChKyygoKUlAAMIMKCggADHAAKAoKClJSosoKCgoKUzLKCgoK6soKCggADFOKCqLKCgoKCvNKCgoKCgoK8woKCgoK8woKCgoK8wgAMHqKUpKSo0oKCpoKCgpSCtKbSgpSCgoKCgpSClIKClIKmtKaygoKCgoKClIKCgpSCprSmkoKUgpSosoKCgoKCgoKUAAIQAAoGgoKUgoKCgoKWgoKClJaClJSmgoKCgoKUgoKCgoKUqvaClJSkpKSClIKCgoKmABMsopSCuICCpIKCgoKClIKUgLSCgpaSgqKClILIkoKigraCgsaClIK4koKSlIK2goKUgtiClKiygoKkgqaCkpSClJSClIKUqJKCgoKCgpSCgoKClIKClIKUlIKClKiSgoKUgoKUptSClIKCgoKClIKClNbCgoKCgpSClIKUtA==", "511-517:1;522-525:2;529-537:3;542-547:4;549-555:5;575-588:1")]
[assembly: go.GoPositionMap("encoding/gob/encoder.go", "encoder.cs", "AB4+lpKCgoKCqJKokqiSpoKS6uiClIKCpoKUgpSUgoKCvPKAgqSCgoK4lIKCgqiCgqaApIKCyKSCtKiygqaogLyilsbGxrYAAiUACCyssqwACAaCgpSAlIKCyoKCgoKU3NTcAAgCgpSCuoKWhIKCloKCgoSCgoKogoKWgg==")]
[assembly: go.GoPositionMap("encoding/gob/error.go", "error.cs", "ABYuwqiSqsKAgoKClMQ=")]
[assembly: go.GoPositionMap("encoding/gob/type.go", "type.cs", "ACxasoCC3oKC3IKCgoKUgqSUgpSWgIKiggAIEoCCooIABxCCqIKCgoKChJ7ygpSmgoKUgIKCkpSCpKaUgqaqooKClAARJoKogoKUpoKClKaUgpSCgqaCgpTYkoKUqJKClAALGICkgKSApIKmgAAEEoKCgoKCgoLEwsLCwsLCuLaUgoKCgoKChKiAgqSCggAJFIKCprSCgqaCgpSCpoD+goKCpoKmgAAJEoKCprSCgqaCgpSCgoKmgAAIEIKCprSmgpSmgoKUgqaAAA0aooKUgIKkgoKClIKmgKSCpoIAAhAACASClIKSgoLKgKampqampqamgoKSggABFAAIAqaCgoKClIKClIKokpSCgoKClIKmgoKCgoKClIKCgoKUgoLcgpSUpgAICpKCrLKCuIKClIKWqqKCAAIQ0oKClIKClKaCgoK8ooKCgpSCgoIAGjCigoKUlKSkpKSkpKQAFiyUgoCCpIKmgoKUlICCpKrCgoSAgqaCgpSEgoKClIKUpKSklIKApKSmkrbIgIKCuIKCgoLYkoKClAAiSqKUlryAgriAgoIABBT0gqiCgoCCABAitoKClKimgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKmgoI=", "460-464:1")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("gob")]
public static partial class gob_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface ΔgobType {}
    internal partial struct decBuffer {}
    internal partial struct decEngine {}
    internal partial struct decInstr {}
    internal partial struct decoderState {}
    internal partial struct emptyStruct {}
    [GoValueClone("scratch")] internal partial struct encBuffer {}
    internal partial struct encEngine {}
    internal partial struct encInstr {}
    [GoValueClone("buf")] internal partial struct encoderState {}
    internal partial struct eᴛ1 {}
    internal partial struct eᴛ2 {}
    internal partial struct eᴛ3 {}
    internal partial struct eᴛ4 {}
    internal partial struct eᴛ5 {}
    internal partial struct eᴛ6 {}
    internal partial struct eᴛ7 {}
    internal partial struct gobError {}
    internal partial struct typeInfo {}
    internal partial struct userTypeInfo {}
    internal partial struct wireType {}
    public partial interface GobDecoder {}
    public partial interface GobEncoder {}
    public partial struct CommonType {}
    public partial struct Decoder {}
    [GoValueClone("byteBuf")] public partial struct Encoder {}
    public partial struct arrayType {}
    public partial struct fieldType {}
    public partial struct gobEncoderType {}
    public partial struct mapType {}
    public partial struct sliceType {}
    public partial struct structType {}
    public partial struct typeId {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(go.encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
