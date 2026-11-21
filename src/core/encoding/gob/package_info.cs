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
using io = go.io_package;
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.encoding.gob_package;
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
[assembly: GoPositionMap("encoding/gob/decode.go", "decode.cs", "ABIogoIAFS6CgoKUgqaCgpSmgoKUgoKmgqaCqJKCpoKCqsKCgoKUlIKmooKmgqrigoKClIKClIKCgpSCgoKUpoKUgqrCgoKUgpSCgpSCgriClIKqooKClKyygoKUAA0esqrCggACHAALAoKClJSosqjCgoKUqMKCgpSowoKClKjCgoKUqMKCgpSowoKClKiygqiyggACENKCrsKCgoKmgpSq0qrCrOKCgqzSgoKs0oKClIKCgpSCgoKC3LiCgpSCgoKUpoKCAAQQ0oKCpoKClIKCqLKCgpSCgpQACx7ygoKCgpSCoq4ACQKCgoKCgoKUgpSClIKCgpSCgqaCtMrSgoKCgoKClIKUgoKUgoK0qOKCgoKCgpSCoqaygpSCgoKCgpSmgoKClIKUgoKUvtKClKiSgoKmgq7igoKUgoKCgoKCgoKCgoL6soKCgpS6soKUqLKCgoKCgrzCgoKCgpSmlIKCgpSUlKiyrPSCgpSClIKClJSClIKmgoKUpoKCuJSCgoK4gqao1IKClIKClIKCgqaCgpSq5IKClIKClIKClJSCtIK0gqSCusSCgpSCgpS4ABIkAAkYwpSCyoKUgoKCgpSClICkgoKCgoLCyIKCgoKCwsiCgoKUgoKUlIKCgrLagoKClKTGktiClAAIDOSCgJKCuIKUkoKCpoKUpoKUtIKCosiCgoKCosiCgpLagoKUpMiCyIKU2sKCgoKCgqaCpIKUlK7CgpSCgu6GlIKUtqSkpKSkpKSkgpSCpIKUgraClpKCgoKUgqS60oKClJTa8oKCgoKClIKUlIKCgoKokoKCgoKCgqoACgKCgoKClKaClIKClJSClIKClIKCgpSUlIKCgpSClIKClNjigoKCgpSUgoKCgqbsltKClIKCgoKUlIKmqNKUgoKmgoKCgoKUgoKCgoSClJS0prKCgoKUgoKUAAYSgpKUgrSCtKSCppS0tKSuwg==", "858-860:1;868-870:2;887-889:3;898-901:4;903-905:5;920-920:1;935-937:2;948-950:3;957-959:4;964-966:5;974-977:6;980-982:7;1003-1009:1", "", "", "1335:21,1448:21", "1069:21,1070:13")]
[assembly: GoPositionMap("encoding/gob/decoder.go", "decoder.cs", "AClaspSClIKCgoKU6MSCgriCgoKmppi0goKClIKClIKokpSWkoKCgrqSgoKUpqKCgpSmooKClAAGGAALAoKCgsqClLiClKaCgtyCgoKUlJQABhQACQKClKaCgpQABhIACwKClIK4gpSCgoKClA==")]
[assembly: GoPositionMap("encoding/gob/enc_helpers.go", "enc_helpers.cs", "AAoaABImABImlIKUpqKClJSCgoKUuKaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgoKCpqaUgpSmooKUlIKCgqamlIKUpqKClJSCgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKmppSClKaigpSUgoKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpqaUgpSmooKUlIKCpg==")]
[assembly: GoPositionMap("encoding/gob/encode.go", "encode.cs", "ACtUlIKCyIKmgoKmgqaCpoKmgoKUuLKCgoKUlIKCgoKUpqKCAAIS4oKCpoKClKyygoKUlAANIMKCggADHAAKAoKClJSosoKCgoKUzLKCgoK6soKCggADFOKCqLKCgoKCvNKCgoKCgoK8woKCgoK8woKCgoK8wgAMHqKUpJSo0oKCpoKCgpSCtKbSgpSCgoKCgpSClIKClIKmtKaygoKCgoKClIKCgpSCprSmkoKUgpSosoKCgoKCgoKUAAIQAAoGgoKUgoKCgoKmgoKClKaClJSmgoKCgoKUgoKCgoKUqgAIBoKUlIK0grSCpIKUgoKCgqYAEyyilIK4gpSCgoKCgpSClICkgoKmgoKigpSC2IKCooLGgoLGgpSC2IKCkpSCxoKClILYgpSosoKCgoKCpoKSlIKUlIKUgpSokoKCgoKClIKCgoKUgoKUgpSUgoKUqJKCgpSCgpSm1IKUgoKCgoKUgoKU1sKCgoKClIKUgpS0", "514-520:1;525-528:2;532-540:3;545-550:4;552-558:5;578-591:1", "", "", "753:21")]
[assembly: GoPositionMap("encoding/gob/encoder.go", "encoder.cs", "AB0+lpKCgoKCqJKokqiSpoKC+uiClIKCpoKUgpSUgoKCvPKClIKCgriUgoKCuIKCpqSCgti0gqSosoKmuLa2gqbGxsbGpqyyrAAIBoKClJSCgsqCgoKClMzU7AAIAoKUgsqCppSCgqaCgoKUgoKCuIKCpoI=")]
[assembly: GoPositionMap("encoding/gob/error.go", "error.cs", "ABYuwqiSquKCgoKUtA==")]
[assembly: GoPositionMap("encoding/gob/type.go", "type.cs", "ACpasoCC/oKC3IKCgoKUgpSUgpSmgIKCggAJEoCCgoIACBCCqIKCgoKClJ7ygpSmgoKUgoKClIKUppSCpqqigoKUABImgqiCgpSmgoKUppSClIKCpoKClOiSgpSokoKUAAsYlJSUgqYABxKCgoKCgoKCxLKysrKysrimlIKCgoKCgpS4gpSCggAJFIKCprSCgqaCgpSCpu6CgoKmgqYACBKCgqa0goKmgoKUgoKCpgAHEIKCprSmgpSmgoKUgqYADBqigpSClIKCgpSCppSCpoIAAhAACASClIKSgoLKtqampqampqaCgoKCAAkUgqaCgoKClIKClIK4gpSCgoKClIKmgoKCgoKClIKCgoKUgoLcgpSUpuqSgqyygriCgpSCpqqiggACENKCgpSCgpSmgoKCvKKCgoKUgoKCABswooKClJSkpKSkpKSUABYslIKClIKmgoKUlIKUqsKClIKmgoKUlIKCgpSClLS0pJSCpLTGgsbIgoK4goKCgtiSgoKUACJKopSm3ICCyICCggAEFPSCuIKCggAQIqaCgpS4poKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCpoKC", "460-464:1", "", "", "", "502:21,503:13")]
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
[GoMethodHost(typeof(gob_package.CommonType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛCommonType;
[GoMethodHost(typeof(gob_package.Decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛDecoder;
[GoMethodHost(typeof(gob_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛEncoder;
[GoMethodHost(typeof(gob_package.arrayType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛarrayType;
[GoMethodHost(typeof(gob_package.decBuffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛdecBuffer;
[GoMethodHost(typeof(gob_package.decoderState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛdecoderState;
[GoMethodHost(typeof(gob_package.encBuffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛencBuffer;
[GoMethodHost(typeof(gob_package.encoderState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛencoderState;
[GoMethodHost(typeof(gob_package.gobEncoderType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛgobEncoderType;
[GoMethodHost(typeof(gob_package.mapType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛmapType;
[GoMethodHost(typeof(gob_package.sliceType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛsliceType;
[GoMethodHost(typeof(gob_package.structType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛstructType;
[GoMethodHost(typeof(gob_package.typeId)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛtypeId;
[GoMethodHost(typeof(gob_package.wireType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gobᴛwireType;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class gobᴛCommonType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gob_package.typeId id(this ref gob_package.CommonType t) => gob_package.id⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setId(this ref gob_package.CommonType t, gob_package.typeId id) => gob_package.setId⁔(ref t, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref gob_package.CommonType t) => gob_package.string⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ref gob_package.CommonType t, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(ref t, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this ref gob_package.CommonType t) => gob_package.name⁔(ref t);
}
partial class gobᴛDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gob_package.decoderState> newDecoderState(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decBuffer> Ꮡbuf) => gob_package.newDecoderState⁔(Ꮡdec, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeDecoderState(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡd) => gob_package.freeDecoderState⁔(ref dec, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeSingle(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decEngine> Ꮡengine, reflect.Value value) => gob_package.decodeSingle⁔(Ꮡdec, Ꮡengine, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeStruct(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decEngine> Ꮡengine, reflect.Value value) => gob_package.decodeStruct⁔(Ꮡdec, Ꮡengine, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreStruct(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decEngine> Ꮡengine) => gob_package.ignoreStruct⁔(Ꮡdec, Ꮡengine);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreSingle(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decEngine> Ꮡengine) => gob_package.ignoreSingle⁔(Ꮡdec, Ꮡengine);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeArrayHelper(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, reflect.Value value, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, nint length, error ovfl, Func<ж<gob_package.decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeArrayHelper⁔(ref dec, Ꮡstate, value, elemOp, length, ovfl, helper);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeArray(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, reflect.Value value, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, nint length, error ovfl, Func<ж<gob_package.decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeArray⁔(ref dec, Ꮡstate, value, elemOp, length, ovfl, helper);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeMap(this ref gob_package.Decoder dec, reflect.Type mtyp, ж<gob_package.decoderState> Ꮡstate, reflect.Value value, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> keyOp, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, error ovfl) => gob_package.decodeMap⁔(ref dec, mtyp, Ꮡstate, value, keyOp, elemOp, ovfl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreArrayHelper(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, nint length) => gob_package.ignoreArrayHelper⁔(ref dec, Ꮡstate, elemOp, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreArray(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, nint length) => gob_package.ignoreArray⁔(ref dec, Ꮡstate, elemOp, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreMap(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> keyOp, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp) => gob_package.ignoreMap⁔(ref dec, Ꮡstate, keyOp, elemOp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeSlice(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, reflect.Value value, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp, error ovfl, Func<ж<gob_package.decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeSlice⁔(ref dec, Ꮡstate, value, elemOp, ovfl, helper);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreSlice(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate, Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value> elemOp) => gob_package.ignoreSlice⁔(ref dec, Ꮡstate, elemOp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeInterface(this ж<gob_package.Decoder> Ꮡdec, reflect.Type ityp, ж<gob_package.decoderState> Ꮡstate, reflect.Value value) => gob_package.decodeInterface⁔(Ꮡdec, ityp, Ꮡstate, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreInterface(this ж<gob_package.Decoder> Ꮡdec, ж<gob_package.decoderState> Ꮡstate) => gob_package.ignoreInterface⁔(Ꮡdec, Ꮡstate);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeGobDecoder(this ref gob_package.Decoder dec, ж<gob_package.userTypeInfo> Ꮡut, ж<gob_package.decoderState> Ꮡstate, reflect.Value value) => gob_package.decodeGobDecoder⁔(ref dec, Ꮡut, Ꮡstate, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignoreGobDecoder(this ref gob_package.Decoder dec, ж<gob_package.decoderState> Ꮡstate) => gob_package.ignoreGobDecoder⁔(ref dec, Ꮡstate);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value>> decOpFor(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId wireId, reflect.Type rt, @string name, map<reflect.Type, ж<Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value>>> inProgress) => gob_package.decOpFor⁔(Ꮡdec, wireId, rt, name, inProgress);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value>> decIgnoreOpFor(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId wireId, map<gob_package.typeId, ж<Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value>>> inProgress) => gob_package.decIgnoreOpFor⁔(Ꮡdec, wireId, inProgress);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Action<ж<gob_package.decInstr>, ж<gob_package.decoderState>, reflect.Value>> gobDecodeOpFor(this ref gob_package.Decoder dec, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.gobDecodeOpFor⁔(ref dec, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool compatibleType(this ref gob_package.Decoder dec, reflect.Type fr, gob_package.typeId fw, map<reflect.Type, gob_package.typeId> inProgress) => gob_package.compatibleType⁔(ref dec, fr, fw, inProgress);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string typeString(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId remoteId) => gob_package.typeString⁔(Ꮡdec, remoteId);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<gob_package.decEngine> engine, error err) compileSingle(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId remoteId, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.compileSingle⁔(Ꮡdec, remoteId, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gob_package.decEngine> compileIgnoreSingle(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId remoteId) => gob_package.compileIgnoreSingle⁔(Ꮡdec, remoteId);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<gob_package.decEngine> engine, error err) compileDec(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId remoteId, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.compileDec⁔(Ꮡdec, remoteId, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ж<gob_package.decEngine>> enginePtr, error err) getDecEnginePtr(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId remoteId, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.getDecEnginePtr⁔(Ꮡdec, remoteId, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ж<gob_package.decEngine>> enginePtr, error err) getIgnoreEnginePtr(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId wireId) => gob_package.getIgnoreEnginePtr⁔(Ꮡdec, wireId);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeValue(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId wireId, reflect.Value value) => gob_package.decodeValue⁔(Ꮡdec, wireId, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decodeIgnoredValue(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId wireId) => gob_package.decodeIgnoredValue⁔(Ꮡdec, wireId);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recvType(this ж<gob_package.Decoder> Ꮡdec, gob_package.typeId id) => gob_package.recvType⁔(Ꮡdec, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool recvMessage(this ref gob_package.Decoder dec) => gob_package.recvMessage⁔(ref dec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readMessage(this ref gob_package.Decoder dec, nint nbytes) => gob_package.readMessage⁔(ref dec, nbytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 nextInt(this ж<gob_package.Decoder> Ꮡdec) => gob_package.nextInt⁔(Ꮡdec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 nextUint(this ж<gob_package.Decoder> Ꮡdec) => gob_package.nextUint⁔(Ꮡdec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gob_package.typeId decodeTypeSequence(this ж<gob_package.Decoder> Ꮡdec, bool isInterface) => gob_package.decodeTypeSequence⁔(Ꮡdec, isInterface);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Decode(this ж<gob_package.Decoder> Ꮡdec, any e) => gob_package.Decode⁔(Ꮡdec, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error DecodeValue(this ж<gob_package.Decoder> Ꮡdec, reflect.Value v) => gob_package.DecodeValue⁔(Ꮡdec, v);
}
partial class gobᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gob_package.encoderState> newEncoderState(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb) => gob_package.newEncoderState⁔(Ꮡenc, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeEncoderState(this ref gob_package.Encoder enc, ж<gob_package.encoderState> Ꮡe) => gob_package.freeEncoderState⁔(ref enc, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeSingle(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, ж<gob_package.encEngine> Ꮡengine, reflect.Value value) => gob_package.encodeSingle⁔(Ꮡenc, Ꮡb, Ꮡengine, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeStruct(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, ж<gob_package.encEngine> Ꮡengine, reflect.Value value) => gob_package.encodeStruct⁔(Ꮡenc, Ꮡb, Ꮡengine, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeArray(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, reflect.Value value, Action<ж<gob_package.encInstr>, ж<gob_package.encoderState>, reflect.Value> op, nint elemIndir, nint length, Func<ж<gob_package.encoderState>, reflect.Value, bool> helper) => gob_package.encodeArray⁔(Ꮡenc, Ꮡb, value, op, elemIndir, length, helper);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeMap(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, reflect.Value mv, Action<ж<gob_package.encInstr>, ж<gob_package.encoderState>, reflect.Value> keyOp, Action<ж<gob_package.encInstr>, ж<gob_package.encoderState>, reflect.Value> elemOp, nint keyIndir, nint elemIndir) => gob_package.encodeMap⁔(Ꮡenc, Ꮡb, mv, keyOp, elemOp, keyIndir, elemIndir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeInterface(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, reflect.Value iv) => gob_package.encodeInterface⁔(Ꮡenc, Ꮡb, iv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeGobEncoder(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, ж<gob_package.userTypeInfo> Ꮡut, reflect.Value v) => gob_package.encodeGobEncoder⁔(Ꮡenc, Ꮡb, Ꮡut, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ж<gob_package.Encoder> Ꮡenc, ж<gob_package.encBuffer> Ꮡb, reflect.Value value, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.encode⁔(Ꮡenc, Ꮡb, value, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static io.Writer writer(this ref gob_package.Encoder enc) => gob_package.writer⁔(ref enc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushWriter(this ref gob_package.Encoder enc, io.Writer w) => gob_package.pushWriter⁔(ref enc, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void popWriter(this ref gob_package.Encoder enc) => gob_package.popWriter⁔(ref enc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setError(this ref gob_package.Encoder enc, error err) => gob_package.setError⁔(ref enc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeMessage(this ref gob_package.Encoder enc, io.Writer w, ж<gob_package.encBuffer> Ꮡb) => gob_package.writeMessage⁔(ref enc, w, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*sent*/ sendActualType(this ж<gob_package.Encoder> Ꮡenc, io.Writer w, ж<gob_package.encoderState> Ꮡstate, ж<gob_package.userTypeInfo> Ꮡut, reflect.Type actual) => gob_package.sendActualType⁔(Ꮡenc, w, Ꮡstate, Ꮡut, actual);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*sent*/ sendType(this ж<gob_package.Encoder> Ꮡenc, io.Writer w, ж<gob_package.encoderState> Ꮡstate, reflect.Type origt) => gob_package.sendType⁔(Ꮡenc, w, Ꮡstate, origt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Encode(this ж<gob_package.Encoder> Ꮡenc, any e) => gob_package.Encode⁔(Ꮡenc, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendTypeDescriptor(this ж<gob_package.Encoder> Ꮡenc, io.Writer w, ж<gob_package.encoderState> Ꮡstate, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.sendTypeDescriptor⁔(Ꮡenc, w, Ꮡstate, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendTypeId(this ref gob_package.Encoder enc, ж<gob_package.encoderState> Ꮡstate, ж<gob_package.userTypeInfo> Ꮡut) => gob_package.sendTypeId⁔(ref enc, Ꮡstate, Ꮡut);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error EncodeValue(this ж<gob_package.Encoder> Ꮡenc, reflect.Value value) => gob_package.EncodeValue⁔(Ꮡenc, value);
}
partial class gobᴛarrayType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<gob_package.arrayType> Ꮡa, gob_package.gobType elem, nint len) => gob_package.init⁔(Ꮡa, elem, len);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ref gob_package.arrayType a, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(ref a, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref gob_package.arrayType a) => gob_package.string⁔(ref a);
}
partial class gobᴛdecBuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref gob_package.decBuffer d, slice<byte> p) => gob_package.Read⁔(ref d, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Drop(this ref gob_package.decBuffer d, nint n) => gob_package.Drop⁔(ref d, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this ref gob_package.decBuffer d) => gob_package.ReadByte⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref gob_package.decBuffer d) => gob_package.Len⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref gob_package.decBuffer d) => gob_package.Bytes⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SetBytes(this ref gob_package.decBuffer d, slice<byte> data) => gob_package.SetBytes⁔(ref d, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Reset(this ref gob_package.decBuffer d) => gob_package.Reset⁔(ref d);
}
partial class gobᴛdecoderState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 /*x*/ decodeUint(this ref gob_package.decoderState state) => gob_package.decodeUint⁔(ref state);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 decodeInt(this ref gob_package.decoderState state) => gob_package.decodeInt⁔(ref state);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, bool) getLength(this ref gob_package.decoderState state) => gob_package.getLength⁔(ref state);
}
partial class gobᴛencBuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeByte(this ref gob_package.encBuffer e, byte c) => gob_package.writeByte⁔(ref e, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref gob_package.encBuffer e, slice<byte> p) => gob_package.Write⁔(ref e, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteString(this ref gob_package.encBuffer e, @string s) => gob_package.WriteString⁔(ref e, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref gob_package.encBuffer e) => gob_package.Len⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref gob_package.encBuffer e) => gob_package.Bytes⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Reset(this ref gob_package.encBuffer e) => gob_package.Reset⁔(ref e);
}
partial class gobᴛencoderState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeUint(this ref gob_package.encoderState state, uint64 x) => gob_package.encodeUint⁔(ref state, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encodeInt(this ref gob_package.encoderState state, int64 i) => gob_package.encodeInt⁔(ref state, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ref gob_package.encoderState state, ж<gob_package.encInstr> Ꮡinstr) => gob_package.update⁔(ref state, Ꮡinstr);
}
partial class gobᴛgobEncoderType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ref gob_package.gobEncoderType g, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(ref g, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref gob_package.gobEncoderType g) => gob_package.string⁔(ref g);
}
partial class gobᴛmapType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<gob_package.mapType> Ꮡm, gob_package.gobType key, gob_package.gobType elem) => gob_package.init⁔(Ꮡm, key, elem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ref gob_package.mapType m, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(ref m, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref gob_package.mapType m) => gob_package.string⁔(ref m);
}
partial class gobᴛsliceType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<gob_package.sliceType> Ꮡs, gob_package.gobType elem) => gob_package.init⁔(Ꮡs, elem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ref gob_package.sliceType s, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(ref s, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref gob_package.sliceType s) => gob_package.string⁔(ref s);
}
partial class gobᴛstructType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string safeString(this ж<gob_package.structType> Ꮡs, map<gob_package.typeId, bool> seen) => gob_package.safeString⁔(Ꮡs, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ж<gob_package.structType> Ꮡs) => gob_package.string⁔(Ꮡs);
}
partial class gobᴛtypeId {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gob_package.gobType gobType(this gob_package.typeId t) => gob_package.gobType⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this gob_package.typeId t) => gob_package.string⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this gob_package.typeId t) => gob_package.name⁔(t);
}
partial class gobᴛwireType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ж<gob_package.wireType> Ꮡw) => gob_package.string⁔(Ꮡw);
}
// </MethodForwarders>
