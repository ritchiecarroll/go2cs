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
[assembly: GoImplement<CommonType, gobType>(Pointer = true)]
[assembly: GoImplement<arrayType, gobType>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<decBuffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<encBuffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gobEncoderType, gobType>(Pointer = true)]
[assembly: GoImplement<mapType, gobType>(Pointer = true)]
[assembly: GoImplement<sliceType, gobType>(Pointer = true)]
[assembly: GoImplement<structType, gobType>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Decoder))]
[assembly: GoFieldAccess(typeof(Encoder))]
[assembly: GoFieldAccess(typeof(arrayType))]
[assembly: GoFieldAccess(typeof(decEngine))]
[assembly: GoFieldAccess(typeof(decInstr))]
[assembly: GoFieldAccess(typeof(decoderState))]
[assembly: GoFieldAccess(typeof(encBuffer))]
[assembly: GoFieldAccess(typeof(encEngine))]
[assembly: GoFieldAccess(typeof(encInstr))]
[assembly: GoFieldAccess(typeof(encoderState))]
[assembly: GoFieldAccess(typeof(gobEncoderType))]
[assembly: GoFieldAccess(typeof(mapType))]
[assembly: GoFieldAccess(typeof(sliceType))]
[assembly: GoFieldAccess(typeof(structType))]
[assembly: GoFieldAccess(typeof(typeInfo))]
[assembly: GoFieldAccess(typeof(userTypeInfo))]
[assembly: GoFieldAccess(typeof(wireType))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("encoding/gob/dec_helpers.go", "dec_helpers.cs", "AAscABImABImlIKUpqKSlJSCgpSUlJSmlIKUpqKSlJSCgpSUlIKClKaUgpSmopKUlIKClJSUgoKUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSUgpSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSUppSClKaikpSUgoKUlJSCgpSUppSClKaikpSUgoKUlJSCgoKUgqaCgpSClKaUgpSmopKUlIKClJSUuJSmlIKUpqKSlJSCgpSUlIKClJSmlIKUpqKSlJSCgpSUlIKClJSmlIKUpqKSlJSCgpSUlJSmlIKUpqKSlJSCgpSUlIKClJSqooKCgoKClIKC")]
[assembly: GoPositionMap("encoding/gob/decode.go", "decode.cs", "ABMogoIAFy6CgoKUgqaCgpSmgoKUgoKmgqaCqJKCpoKCysKCgoKUlIKmooLGgqrigoKClIKClIKCgpSCgoKUpoKUgsrCgoKUgpSCgpSCgriClIKqooKClKyygoKUAA8esqrCggACHAALAoKClJSosqjCgoKUqMKCgpSowoKClKjCgoKUqMKCgpSowoKClKiygqiyggACENKCrsKCgoKmgpSq0qrCrOKCgqzSgoKs0oKClIKCgpSCgoKC3LiCgpSCgoKUpoKCAAQQ0oKCpoKClIKCqLKCgpSCgpQADR7ygoKCgpSCoq4ACQKCgoKCgoKUgpSClIKCgpSCgqaCtAAICtKCgoKCgoKUgpSCgpSCgrSo4oKCgoKClIKiprKClIKCgoKClKaCgoKUgpSCgpS+0oKUyJKCgqaCzuKCgpSCgoKCgoKCgoKCgvqygoKClLqygpSosoKCgoKCvMKCgoKClKaUgoKClJSUqLKs9IKClIKUgoKUlIKUgqaCgpSmgoK4lIKCgriCpqjUgoKUgoKUgoKCpoKClKrkgoKUgoKUgoKUlIK0grSCpIK6xIKClIKClNgAEiQACxjClILKgpSCgoKClIKUgKSCgoKCgsLIgoKCgoLCyIKCgpSCgpSUgoKCstqCgoKUpMaS2IKUAAwM5IKAkoK4gpSSgoKmgpSmgpS0goKiyIKCgoKiyIKCktqCgpSkyILIgpTawoKCgoKCpoKkgpSUrsKClIKC7oaUgpS2pKSkpKSkpKSClIKkgpSCtoKWkoKCgpSCpLrSgoKUlNrygoKCgoKUgpSUgoKCgqiSgoKCgoKCqgAKAoKCgoKUpoKUgoKUlIKUgoKUgoKClJSUgoKClIKUgoKU2OKCgoKClJSCgoKCpgAIDLbSgpSCgoKClJSCpqjSlIKCpoKCgoKClIKCgoKEgpSUtKaygoKClIKClAAIEoKSlIK0grSkgqaUtLSkrsI=", "858-860:1;868-870:2;887-889:3;898-901:4;903-905:5;920-920:1;935-937:2;948-950:3;957-959:4;964-966:5;974-977:6;980-982:7;1003-1009:1", "", "", "1364:21,1481:21", "1098:21,1099:13")]
[assembly: GoPositionMap("encoding/gob/decoder.go", "decoder.cs", "ACpaspSClIKCgoKUAAgIxIKCuIKCgqbGuLSCgoKUgoKUgqiSlJaSgoKC2pKCgpTGooKClKaigoKUAAYYAAsCgoKCyoKUuIKUpoKC3IKCgpSUlAAGFAAJAoKUpoKClAAGEgALAoKUgriClIKCgoKU")]
[assembly: GoPositionMap("encoding/gob/enc_helpers.go", "enc_helpers.cs", "AAoaABImABImlIKUpqKClJSCgoKUuKaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgqamlIKUpqKClJSCgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpg==")]
[assembly: GoPositionMap("encoding/gob/encode.go", "encode.cs", "ACxUlIKC6IKmgoKmgqaCpoKmgoKU2LKCgoKUlIKCgoKUpqKCAAQS4oKCpoKClKyygoKUlAARIMKCggAFHAAKAoKClJSosoKCgoKUzLKCgoK6soKCggADFOKCqLKCgoKCvNKCgoKCgoK8woKCgoK8woKCgoK8wgAMHqKUpJTI0oKCpoKCgpSCtKbSgpSCgoKCgpSClIKClIKmtKaygoKCgoKClIKCgpSCprTGkoKUgpTIsoKCgoKCgoKUAAIQAAoGgoKUgoKCgoKmgoKClKaClJSmgoKCgoKUgoKCgoKUqgAIBoKUlIK0grSCpIKUgoKCgsYAEyyilIK4gpSCgoKCgpSClICkgoKmgoKigpSC2IKCooLGgoLGgpSC2IKCkpSCxoKClILYgpSosoKCgoKCpoKSlIKUlIKUgpSokoKCgoKClIKCgoKUgoKUgpSUgoKUqJKCgpSCgpSm1IKUgoKCgoKUgoKU9sKCgoKClIKUgpS0", "514-520:1;525-528:2;532-540:3;545-550:4;552-558:5;578-591:1", "", "", "776:21")]
[assembly: GoPositionMap("encoding/gob/encoder.go", "encoder.cs", "AB4+lpKCgoKCyJKokqiSpoKC+uiClIKCpoKUgpSUgoKCvPKClIKCgriUgoKCuIKCpqSCgti0gqSosoKmuLa2gqbGxsbGpqyyrAAIBoKClJSCgsqCgoKClMzU7AAIAoKUgsqCppSCgqaCgoKUgoKCuIKCpoI=")]
[assembly: GoPositionMap("encoding/gob/error.go", "error.cs", "ABYuwqiSquKCgoKUtA==")]
[assembly: GoPositionMap("encoding/gob/type.go", "type.cs", "ACtasoCC/oKC3IKCgoKUgpSUgpSmgIKCggAJEoCCgoIACBCCqIKCgoKClJ7ygpSmgoKUgoKClIKUppSCpqqigoKUABImgqiCgpSmgoKUppSClIKCxoKClOiSgpSokoKUAA8YlJSUgqYACRKCgoKCgoKCxLKysrKysrimlIKCgoKCgpS4gpSCggAJFIKCxrSCgqaCgpSCpgAIDoKCgsaCpgAKEoKCxrSCgqaCgpSCgoKmAAkQgoLGtKaClKaCgpSCpgAUGqKClIKUgoKClIKmtIKmggACEAAIBIKUgpKCgsq2pqampqampoKCgoIACRSCpoKCgoKUgoKUgriClIKCgoKUgqaCgoKCgoKUgoKCgpSCgtyClJSm6pKCrLKCuIKClIKmqqKCAAIQ0oKClIKClKaCgoK8ooKCgpSCgoIAHTCigoKUlKSkpKSkpJQAGCyUgoKUgqaCgpSUgpSqwoKUgqaCgpSUgoKClIKUtLSklIKktMaCxsiCgriCgoKC2JKCgpQAIkqilKbcgILIgIKCAAQU9IK4goKCABAipoKClLimgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKmgoI=", "460-464:1", "", "", "", "535:21,536:13")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("gob")]
public static partial class gob_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface gobType {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gob_package.CommonType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛCommonType;
[GoMethodHost(typeof(gob_package.Decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛDecoder;
[GoMethodHost(typeof(gob_package.Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛEncoder;
[GoMethodHost(typeof(gob_package.arrayType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛarrayType;
[GoMethodHost(typeof(gob_package.decBuffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛdecBuffer;
[GoMethodHost(typeof(gob_package.decoderState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛdecoderState;
[GoMethodHost(typeof(gob_package.encBuffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛencBuffer;
[GoMethodHost(typeof(gob_package.encoderState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛencoderState;
[GoMethodHost(typeof(gob_package.gobEncoderType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛgobEncoderType;
[GoMethodHost(typeof(gob_package.mapType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛmapType;
[GoMethodHost(typeof(gob_package.sliceType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛsliceType;
[GoMethodHost(typeof(gob_package.structType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛstructType;
[GoMethodHost(typeof(gob_package.typeId)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛtypeId;
[GoMethodHost(typeof(gob_package.wireType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gobᴛwireType;
// </MethodHolders>
