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

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using encoding = encoding_package;
using errors = errors_package;
using saferio = @internal.saferio_package;
using io = io_package;
using math = math_package;
using bits = go.math.bits_package;
using reflect = reflect_package;
using @internal;
using go.math;
using static gob_package;
using bufio = bufio_package;
using sync = sync_package;
using binary = go.encoding.binary_package;
using go.encoding;
using go.sync;
using fmt = fmt_package;
using maps = maps_package;
using os = os_package;
using atomic = go.sync.atomic_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class gobᴛdecBuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref decBuffer d, slice<byte> p) => gob_package.Readᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Drop(this ref decBuffer d, nint n) => gob_package.Dropᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte, error) ReadByte(this ref decBuffer d) => gob_package.ReadByteᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref decBuffer d) => gob_package.Lenᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> Bytes(this ref decBuffer d) => gob_package.Bytesᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SetBytes(this ref decBuffer d, slice<byte> data) => gob_package.SetBytesᴍ(ref d, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Reset(this ref decBuffer d) => gob_package.Resetᴍ(ref d);
}
partial class gobᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<decoderState> newDecoderState(this ж<Decoder> Ꮡdec, ж<decBuffer> buf) => gob_package.newDecoderStateᴍ(Ꮡdec, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeDecoderState(this ref Decoder dec, ж<decoderState> Ꮡd) => gob_package.freeDecoderStateᴍ(ref dec, Ꮡd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeSingle(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine, reflect.Value value) => gob_package.decodeSingleᴍ(Ꮡdec, Ꮡengine, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeStruct(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine, reflect.Value value) => gob_package.decodeStructᴍ(Ꮡdec, Ꮡengine, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreStruct(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine) => gob_package.ignoreStructᴍ(Ꮡdec, Ꮡengine);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreSingle(this ж<Decoder> Ꮡdec, ж<decEngine> Ꮡengine) => gob_package.ignoreSingleᴍ(Ꮡdec, Ꮡengine);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeArrayHelper(this ref Decoder dec, ж<decoderState> Ꮡstate, reflect.Value value, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, nint length, error ovfl, Func<ж<decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeArrayHelperᴍ(ref dec, Ꮡstate, value, elemOp, length, ovfl, helper);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeArray(this ref Decoder dec, ж<decoderState> Ꮡstate, reflect.Value value, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, nint length, error ovfl, Func<ж<decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeArrayᴍ(ref dec, Ꮡstate, value, elemOp, length, ovfl, helper);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeMap(this ref Decoder dec, reflect.Type mtyp, ж<decoderState> Ꮡstate, reflect.Value value, Action<ж<decInstr>, ж<decoderState>, reflect.Value> keyOp, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, error ovfl) => gob_package.decodeMapᴍ(ref dec, mtyp, Ꮡstate, value, keyOp, elemOp, ovfl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreArrayHelper(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, nint length) => gob_package.ignoreArrayHelperᴍ(ref dec, Ꮡstate, elemOp, length);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreArray(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, nint length) => gob_package.ignoreArrayᴍ(ref dec, Ꮡstate, elemOp, length);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreMap(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflect.Value> keyOp, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp) => gob_package.ignoreMapᴍ(ref dec, Ꮡstate, keyOp, elemOp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeSlice(this ref Decoder dec, ж<decoderState> Ꮡstate, reflect.Value value, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp, error ovfl, Func<ж<decoderState>, reflect.Value, nint, error, bool> helper) => gob_package.decodeSliceᴍ(ref dec, Ꮡstate, value, elemOp, ovfl, helper);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreSlice(this ref Decoder dec, ж<decoderState> Ꮡstate, Action<ж<decInstr>, ж<decoderState>, reflect.Value> elemOp) => gob_package.ignoreSliceᴍ(ref dec, Ꮡstate, elemOp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeInterface(this ж<Decoder> Ꮡdec, reflect.Type ityp, ж<decoderState> Ꮡstate, reflect.Value value) => gob_package.decodeInterfaceᴍ(Ꮡdec, ityp, Ꮡstate, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreInterface(this ж<Decoder> Ꮡdec, ж<decoderState> Ꮡstate) => gob_package.ignoreInterfaceᴍ(Ꮡdec, Ꮡstate);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeGobDecoder(this ref Decoder dec, ж<userTypeInfo> Ꮡut, ж<decoderState> Ꮡstate, reflect.Value value) => gob_package.decodeGobDecoderᴍ(ref dec, Ꮡut, Ꮡstate, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignoreGobDecoder(this ref Decoder dec, ж<decoderState> Ꮡstate) => gob_package.ignoreGobDecoderᴍ(ref dec, Ꮡstate);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Action<ж<decInstr>, ж<decoderState>, reflect.Value>> decOpFor(this ж<Decoder> Ꮡdec, typeId wireId, reflect.Type rt, @string name, map<reflect.Type, ж<Action<ж<decInstr>, ж<decoderState>, reflect.Value>>> inProgress) => gob_package.decOpForᴍ(Ꮡdec, wireId, rt, name, inProgress);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Action<ж<decInstr>, ж<decoderState>, reflect.Value>> decIgnoreOpFor(this ж<Decoder> Ꮡdec, typeId wireId, map<typeId, ж<Action<ж<decInstr>, ж<decoderState>, reflect.Value>>> inProgress) => gob_package.decIgnoreOpForᴍ(Ꮡdec, wireId, inProgress);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Action<ж<decInstr>, ж<decoderState>, reflect.Value>> gobDecodeOpFor(this ref Decoder dec, ж<userTypeInfo> Ꮡut) => gob_package.gobDecodeOpForᴍ(ref dec, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool compatibleType(this ref Decoder dec, reflect.Type fr, typeId fw, map<reflect.Type, typeId> inProgress) => gob_package.compatibleTypeᴍ(ref dec, fr, fw, inProgress);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string typeString(this ж<Decoder> Ꮡdec, typeId remoteId) => gob_package.typeStringᴍ(Ꮡdec, remoteId);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<decEngine> engine, error err) compileSingle(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) => gob_package.compileSingleᴍ(Ꮡdec, remoteId, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<decEngine> compileIgnoreSingle(this ж<Decoder> Ꮡdec, typeId remoteId) => gob_package.compileIgnoreSingleᴍ(Ꮡdec, remoteId);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<decEngine> engine, error err) compileDec(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) => gob_package.compileDecᴍ(Ꮡdec, remoteId, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ж<decEngine>> enginePtr, error err) getDecEnginePtr(this ж<Decoder> Ꮡdec, typeId remoteId, ж<userTypeInfo> Ꮡut) => gob_package.getDecEnginePtrᴍ(Ꮡdec, remoteId, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ж<decEngine>> enginePtr, error err) getIgnoreEnginePtr(this ж<Decoder> Ꮡdec, typeId wireId) => gob_package.getIgnoreEnginePtrᴍ(Ꮡdec, wireId);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeValue(this ж<Decoder> Ꮡdec, typeId wireId, reflect.Value value) => gob_package.decodeValueᴍ(Ꮡdec, wireId, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decodeIgnoredValue(this ж<Decoder> Ꮡdec, typeId wireId) => gob_package.decodeIgnoredValueᴍ(Ꮡdec, wireId);
}
partial class gobᴛdecoderState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 /*x*/ decodeUint(this ref decoderState state) => gob_package.decodeUintᴍ(ref state);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 decodeInt(this ref decoderState state) => gob_package.decodeIntᴍ(ref state);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, bool) getLength(this ref decoderState state) => gob_package.getLengthᴍ(ref state);
}
partial class gobᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recvType(this ж<Decoder> Ꮡdec, typeId id) => gob_package.recvTypeᴍ(Ꮡdec, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool recvMessage(this ref Decoder dec) => gob_package.recvMessageᴍ(ref dec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readMessage(this ref Decoder dec, nint nbytes) => gob_package.readMessageᴍ(ref dec, nbytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 nextInt(this ж<Decoder> Ꮡdec) => gob_package.nextIntᴍ(Ꮡdec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 nextUint(this ж<Decoder> Ꮡdec) => gob_package.nextUintᴍ(Ꮡdec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typeId decodeTypeSequence(this ж<Decoder> Ꮡdec, bool isInterface) => gob_package.decodeTypeSequenceᴍ(Ꮡdec, isInterface);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Decode(this ж<Decoder> Ꮡdec, any e) => gob_package.Decodeᴍ(Ꮡdec, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error DecodeValue(this ж<Decoder> Ꮡdec, reflect.Value v) => gob_package.DecodeValueᴍ(Ꮡdec, v);
}
partial class gobᴛencBuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeByte(this ref encBuffer e, byte c) => gob_package.writeByteᴍ(ref e, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref encBuffer e, slice<byte> p) => gob_package.Writeᴍ(ref e, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void WriteString(this ref encBuffer e, @string s) => gob_package.WriteStringᴍ(ref e, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref encBuffer e) => gob_package.Lenᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> Bytes(this ref encBuffer e) => gob_package.Bytesᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Reset(this ref encBuffer e) => gob_package.Resetᴍ(ref e);
}
partial class gobᴛEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<encoderState> newEncoderState(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb) => gob_package.newEncoderStateᴍ(Ꮡenc, Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeEncoderState(this ref Encoder enc, ж<encoderState> Ꮡe) => gob_package.freeEncoderStateᴍ(ref enc, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeSingle(this ж<Encoder> Ꮡenc, ж<encBuffer> b, ж<encEngine> Ꮡengine, reflect.Value value) => gob_package.encodeSingleᴍ(Ꮡenc, b, Ꮡengine, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeStruct(this ж<Encoder> Ꮡenc, ж<encBuffer> b, ж<encEngine> Ꮡengine, reflect.Value value) => gob_package.encodeStructᴍ(Ꮡenc, b, Ꮡengine, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeArray(this ж<Encoder> Ꮡenc, ж<encBuffer> b, reflect.Value value, Action<ж<encInstr>, ж<encoderState>, reflect.Value> op, nint elemIndir, nint length, Func<ж<encoderState>, reflect.Value, bool> helper) => gob_package.encodeArrayᴍ(Ꮡenc, b, value, op, elemIndir, length, helper);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeMap(this ж<Encoder> Ꮡenc, ж<encBuffer> b, reflect.Value mv, Action<ж<encInstr>, ж<encoderState>, reflect.Value> keyOp, Action<ж<encInstr>, ж<encoderState>, reflect.Value> elemOp, nint keyIndir, nint elemIndir) => gob_package.encodeMapᴍ(Ꮡenc, b, mv, keyOp, elemOp, keyIndir, elemIndir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeInterface(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, reflect.Value iv) => gob_package.encodeInterfaceᴍ(Ꮡenc, Ꮡb, iv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeGobEncoder(this ж<Encoder> Ꮡenc, ж<encBuffer> Ꮡb, ж<userTypeInfo> Ꮡut, reflect.Value v) => gob_package.encodeGobEncoderᴍ(Ꮡenc, Ꮡb, Ꮡut, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ж<Encoder> Ꮡenc, ж<encBuffer> b, reflect.Value value, ж<userTypeInfo> Ꮡut) => gob_package.encodeᴍ(Ꮡenc, b, value, Ꮡut);
}
partial class gobᴛencoderState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeUint(this ref encoderState state, uint64 x) => gob_package.encodeUintᴍ(ref state, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encodeInt(this ref encoderState state, int64 i) => gob_package.encodeIntᴍ(ref state, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ref encoderState state, ж<encInstr> Ꮡinstr) => gob_package.updateᴍ(ref state, Ꮡinstr);
}
partial class gobᴛEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static io.Writer writer(this ref Encoder enc) => gob_package.writerᴍ(ref enc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushWriter(this ref Encoder enc, io.Writer w) => gob_package.pushWriterᴍ(ref enc, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void popWriter(this ref Encoder enc) => gob_package.popWriterᴍ(ref enc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setError(this ref Encoder enc, error err) => gob_package.setErrorᴍ(ref enc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeMessage(this ref Encoder enc, io.Writer w, ж<encBuffer> Ꮡb) => gob_package.writeMessageᴍ(ref enc, w, Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*sent*/ sendActualType(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut, reflect.Type actual) => gob_package.sendActualTypeᴍ(Ꮡenc, w, Ꮡstate, Ꮡut, actual);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*sent*/ sendType(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> state, reflect.Type origt) => gob_package.sendTypeᴍ(Ꮡenc, w, state, origt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Encode(this ж<Encoder> Ꮡenc, any e) => gob_package.Encodeᴍ(Ꮡenc, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendTypeDescriptor(this ж<Encoder> Ꮡenc, io.Writer w, ж<encoderState> state, ж<userTypeInfo> Ꮡut) => gob_package.sendTypeDescriptorᴍ(Ꮡenc, w, state, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sendTypeId(this ref Encoder enc, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut) => gob_package.sendTypeIdᴍ(ref enc, Ꮡstate, Ꮡut);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error EncodeValue(this ж<Encoder> Ꮡenc, reflect.Value value) => gob_package.EncodeValueᴍ(Ꮡenc, value);
}
partial class gobᴛtypeId {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static gobType gobType(this typeId t) => gob_package.gobTypeᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this typeId t) => gob_package.@stringᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this typeId t) => gob_package.nameᴍ(t);
}
partial class gobᴛCommonType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typeId id(this ref CommonType t) => gob_package.idᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setId(this ref CommonType t, typeId id) => gob_package.setIdᴍ(ref t, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref CommonType t) => gob_package.@stringᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ref CommonType t, map<typeId, bool> seen) => gob_package.safeStringᴍ(ref t, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this ref CommonType t) => gob_package.nameᴍ(ref t);
}
partial class gobᴛarrayType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<arrayType> Ꮡa, gobType elem, nint len) => gob_package.initᴍ(Ꮡa, elem, len);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ref arrayType a, map<typeId, bool> seen) => gob_package.safeStringᴍ(ref a, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref arrayType a) => gob_package.@stringᴍ(ref a);
}
partial class gobᴛgobEncoderType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ref gobEncoderType g, map<typeId, bool> seen) => gob_package.safeStringᴍ(ref g, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref gobEncoderType g) => gob_package.@stringᴍ(ref g);
}
partial class gobᴛmapType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<mapType> Ꮡm, gobType key, gobType elem) => gob_package.initᴍ(Ꮡm, key, elem);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ref mapType m, map<typeId, bool> seen) => gob_package.safeStringᴍ(ref m, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref mapType m) => gob_package.@stringᴍ(ref m);
}
partial class gobᴛsliceType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<sliceType> Ꮡs, gobType elem) => gob_package.initᴍ(Ꮡs, elem);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ref sliceType s, map<typeId, bool> seen) => gob_package.safeStringᴍ(ref s, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref sliceType s) => gob_package.@stringᴍ(ref s);
}
partial class gobᴛstructType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string safeString(this ж<structType> Ꮡs, map<typeId, bool> seen) => gob_package.safeStringᴍ(Ꮡs, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ж<structType> Ꮡs) => gob_package.@stringᴍ(Ꮡs);
}
partial class gobᴛwireType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ж<wireType> Ꮡw) => gob_package.@stringᴍ(Ꮡw);
}
// </GlyphForwarders>
