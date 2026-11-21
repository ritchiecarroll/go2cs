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
using static go.encoding.asn1_package;

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
[assembly: GoImplement<StructuralError, error>]
[assembly: GoImplement<SyntaxError, error>]
[assembly: GoImplement<bitStringEncoder, encoder>]
[assembly: GoImplement<byteEncoder, encoder>]
[assembly: GoImplement<bytesEncoder, encoder>]
[assembly: GoImplement<int64Encoder, encoder>]
[assembly: GoImplement<invalidUnmarshalError, error>(Pointer = true)]
[assembly: GoImplement<multiEncoder, encoder>]
[assembly: GoImplement<oidEncoder, encoder>]
[assembly: GoImplement<setEncoder, encoder>]
[assembly: GoImplement<stringEncoder, encoder>]
[assembly: GoImplement<taggedEncoder, encoder>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(taggedEncoder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("encoding/asn1/asn1.go", "asn1.cs", "ADBYAAoOvNKCgtyUtLS2rsKClIKUgpSq0oKClJSClIKCuIKCqqKClIKClIKUppiigpSClIKClIKCgpSCAA4eooKUgoKqooKCpoKCgoKmyMKCgpSChoKUgoLctgAJEJKmgoKUgoKClKbM4oKCytyCgpSCgpSCpoKCgoKUlIIADB7igoKmgoKUgqaCgpSCgoKUgpSmgqrClIKCgoKUgqaCgqaUpurSgpSCpoKmrsKCgqaokgACEPKCgoKmggALIrIACzDygoKCpoKuAA0UgpSU7sKClO4ADhKCuIKmgoK4hpSCpgATLAAKAqaCgpSCgoKCuIKCgqaCgqaCgpSCgpSmgoKClIKCgoKUgoKmgpSCgpSCuIKCuKzigoKCyoKCgoKClMrGtoKClIKClIKUgoKCgpSCgoKCgoKCpqiCgoKCgoKCmqLs4oKYgpSCpoKClLiCgoKClIKClIKCgpS0tLS0tLS0tLS0tLT6goKUgpSmgoKUgoKClIKClIKUgoKCpoKClIK4goKUlLiCgoLugoKUpIIACBKCgoKUgriCpoKClIKCgqaCgoKmgoKCuJaCgpSUlIKClIKmlIKkgqSCpIKClIKkgoKUgqSCpIKClIKUpIKClIKkgoKClJSCgpSUtpSCgoK4hIKmgoKCgpSCgtykgoKCgpSCgpSCpIKUtLS0tPy0tqSClJSCqqKUpqzSgpSCgpSClAACngEATAIADhKCgqaClMrCgoKUgoKU", "", "", "1080=Field/1/1/1")]
[assembly: GoPositionMap("encoding/asn1/common.go", "common.cs", "AFe8AQAIAoKCgpS0goLGtLS0tLS0goKCxoKCgsa0goLGgoLGtuqilKSkpKSklJSkpKSClIKUpJQ=")]
[assembly: GoPositionMap("encoding/asn1/marshal.go", "marshal.cs", "ACJCgqaCAAgKgqaCggAJDIKmgoIACQyCgoKUpoKCgoIACQyCgoKUpgAIEoKCggAIEpSCgoIADxiCpoKCAAgKgpSCgqaCgqamgpSC2IKCpoKCpqaClIKCgoKmpqaigqbKgoKCgpSClIKUlIKmlLiClIKmpqKCgoKUpoKCgpSCgoKUgqaCgoKUpuqCpoKCggAJDIKCgpSmgoKC2IKCpqaC7oK4poKCgrimgoKCuKaCpoKmgu6CgqailIKCpqailIKCpqaClJS0tKamgoKCppSmgpSClJSCgpSUlKS0toKCpoKUpoKCgpSmopSkgoKUpIKkgqSCpoCkgpSkpJSCgriUgoLKgoKCyqampKSkgoKCgrjGgoKmlKSkpJSCgoK4gpTGlKSkpNjmooKmgqaCpoKClILugoK4goKCppSClKaCgqaCpoKmlLiCgoKUgrjGgoLIgoKUAAcQgqaUgoKmlIKCgoKClKaClJSU7rimlAACHgAMAqqigoKUgoI=", "", "", "790=Len/1/1/3")]
// </GoSourcePositionMaps>

namespace go.encoding;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using math = math_package;
using big = go.math.big_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @internal;
using go.math;
using unicode;
using static asn1_package;
using bytes = bytes_package;
// </GlyphForwarderUsings>

[GoPackage("asn1")]
public static partial class asn1_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface encoder {}
    internal partial struct ampersandFlag {}
    internal partial struct asteriskFlag {}
    internal partial struct bitStringEncoder {}
    internal partial struct byteEncoder {}
    internal partial struct bytesEncoder {}
    internal partial struct fieldParameters {}
    internal partial struct int64Encoder {}
    internal partial struct invalidUnmarshalError {}
    internal partial struct multiEncoder {}
    internal partial struct oidEncoder {}
    internal partial struct setEncoder {}
    internal partial struct stringEncoder {}
    internal partial struct tagAndLength {}
    [GoValueClone("scratch")] internal partial struct taggedEncoder {}
    public partial struct BitString {}
    public partial struct Enumerated {}
    public partial struct Flag {}
    public partial struct ObjectIdentifier {}
    public partial struct RawContent {}
    public partial struct RawValue {}
    public partial struct StructuralError {}
    public partial struct SyntaxError {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
[GoMethodHost(typeof(asn1_package.BitString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛBitString;
[GoMethodHost(typeof(asn1_package.ObjectIdentifier)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛObjectIdentifier;
[GoMethodHost(typeof(asn1_package.StructuralError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛStructuralError;
[GoMethodHost(typeof(asn1_package.SyntaxError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛSyntaxError;
[GoMethodHost(typeof(asn1_package.bitStringEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛbitStringEncoder;
[GoMethodHost(typeof(asn1_package.byteEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛbyteEncoder;
[GoMethodHost(typeof(asn1_package.bytesEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛbytesEncoder;
[GoMethodHost(typeof(asn1_package.int64Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛint64Encoder;
[GoMethodHost(typeof(asn1_package.invalidUnmarshalError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛinvalidUnmarshalError;
[GoMethodHost(typeof(asn1_package.multiEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛmultiEncoder;
[GoMethodHost(typeof(asn1_package.oidEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛoidEncoder;
[GoMethodHost(typeof(asn1_package.setEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛsetEncoder;
[GoMethodHost(typeof(asn1_package.stringEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛstringEncoder;
[GoMethodHost(typeof(asn1_package.taggedEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class asn1ᴛtaggedEncoder;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class asn1ᴛStructuralError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this StructuralError e) => asn1_package.Errorᴍ(e);
}
partial class asn1ᴛSyntaxError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this SyntaxError e) => asn1_package.Errorᴍ(e);
}
partial class asn1ᴛBitString {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint At(this BitString b, nint i) => asn1_package.Atᴍ(b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> RightAlign(this BitString b) => asn1_package.RightAlignᴍ(b);
}
partial class asn1ᴛObjectIdentifier {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this ObjectIdentifier oi, ObjectIdentifier other) => asn1_package.Equalᴍ(oi, other);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ObjectIdentifier oi) => asn1_package.Stringᴍ(oi);
}
partial class asn1ᴛinvalidUnmarshalError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref invalidUnmarshalError e) => asn1_package.Errorᴍ(ref e);
}
partial class asn1ᴛbyteEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this byteEncoder c) => asn1_package.Lenᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this byteEncoder c, slice<byte> dst) => asn1_package.Encodeᴍ(c, dst);
}
partial class asn1ᴛbytesEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this bytesEncoder b) => asn1_package.Lenᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this bytesEncoder b, slice<byte> dst) => asn1_package.Encodeᴍ(b, dst);
}
partial class asn1ᴛstringEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this stringEncoder s) => asn1_package.Lenᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this stringEncoder s, slice<byte> dst) => asn1_package.Encodeᴍ(s, dst);
}
partial class asn1ᴛmultiEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this multiEncoder m) => asn1_package.Lenᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this multiEncoder m, slice<byte> dst) => asn1_package.Encodeᴍ(m, dst);
}
partial class asn1ᴛsetEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this setEncoder s) => asn1_package.Lenᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this setEncoder s, slice<byte> dst) => asn1_package.Encodeᴍ(s, dst);
}
partial class asn1ᴛtaggedEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref taggedEncoder t) => asn1_package.Lenᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this ref taggedEncoder t, slice<byte> dst) => asn1_package.Encodeᴍ(ref t, dst);
}
partial class asn1ᴛint64Encoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this int64Encoder i) => asn1_package.Lenᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this int64Encoder i, slice<byte> dst) => asn1_package.Encodeᴍ(i, dst);
}
partial class asn1ᴛbitStringEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this bitStringEncoder b) => asn1_package.Lenᴍ(b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this bitStringEncoder b, slice<byte> dst) => asn1_package.Encodeᴍ(b, dst);
}
partial class asn1ᴛoidEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this oidEncoder oid) => asn1_package.Lenᴍ(oid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Encode(this oidEncoder oid, slice<byte> dst) => asn1_package.Encodeᴍ(oid, dst);
}
// </GlyphForwarders>
