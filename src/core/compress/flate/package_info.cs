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
using static go.compress.flate_package;

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
[assembly: GoImplement<CorruptInputError, error>]
[assembly: GoImplement<InternalError, error>]
[assembly: GoImplement<bufio_package.Reader, Reader>(Pointer = true)]
[assembly: GoImplement<byFreq, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<byLiteral, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<decompressor, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<dictWriter, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("compress/flate/deflate.go", "deflate.cs", "ACuCAQAvdoKUgoKCgpSUgoKCgqiCgpSmgoKUyoKCpoKCgoKUgoKUrtSClIKogqaWgoKCgoKUgoSCloKCgoKmlLiCquKCgpaWgoKogoKCloKChIKChIKCgoKUlKaUlIKCpqaCgIKkggAEELKqooKUgoKCgoK+soKCgoKmrMSCgqiClKS0grSCgrqWgpSUgqaCgoKCgoKCgoKmgoKWhIKCgpSCgoKUgpSUlIKUgoCCpJSmlIKCgoKUgoKCgoKCloaAgoK2qIKU3IKCgpSUgoKCpoKUpoSCgsqUlICCpKaCgoKUgoKAgqSmgoL8goKCpoKCgr6ygpSCgqaygpSCgoKCgqamooKUgoKCgoKUgqaChJSCgqSCgqSCgoKCgqSCpIKCgqSkpoKCgoKUpIKCpIKCgoKCgoKCgsiigpSClIKCgpSAgqSCgpSCAAIeAAwCgoCCpAACEuKCgoKUgoLugqYACBaiAAIYAAsGqJKssoCUgoKm")]
[assembly: go.GoPositionMap("compress/flate/deflatefast.go", "deflatefast.cs", "ABE0goKmgoKoggAXMoKqtIK6goKCzJaCgoKEAA8ghIKCgoKCgoKClIKCgoSClIKUzAAIFMyCgpaCgoKCAAcSgoKCgoKChIKCgoKCuoKClIKCgqaCgpSssoKCqIKCgpSCgqaogoKogoKClIKCgsyCgqiCgoKCpqqippaCAAMQwpSCgqiCgsqUlA==")]
[assembly: go.GoPositionMap("compress/flate/dict_decoder.go", "dict_decoder.cs", "ACNOsoSClISClIKCgpSokoKUqJKokqyyrLKssoIAAhDSgoKCgoIACBSCgoIADyKCloIAAhLigoKClIKWgpaCrLKCgoKClA==")]
[assembly: go.GoPositionMap("compress/flate/huffman_bit_writer.go", "huffman_bit_writer.cs", "ABZMAAcQ7twACRQAFzKCAAoWgoKmgoKClIKCgoKSlJSUgoKmgoKUpoKClIKCgoKCgoKCgoKCgoKCgoKClOiCgpSCgoKUgoKCgpSClIIAAh4ADwLKlIKCloKClISCgoKmgoKCpoKCgoKCgoKClIKCgoKCpoKCgpSCgoKCgpSUgoKCgoKmgoKCgqaCpqjigoKUioqokgACErKClIKUpoKClIKCgoKCgoKCgoKCgoKCgoKClAADEtKClIKClIKCgoSCgpaCgoKCgpSElIK0grSC6oKClIKClIKCgqaCgpaSgpQAAhDSgpaChIKCypSUlKyigoa4goKEgoKCqIKCgqiClKgAAhDSgpaCqIKCloCCgoK4lq7ygoSCgoKUgoKCqIKCpoKClKaClIKCqqKClIKCgqaCgoKCgoKmgoKCgoKCAAgSgoKCgqyygqiWhISCgoSIyIKCloCCgoK4goKClIKCgoKmgoKCgoKCgoKCgoKClIKClJSCrLKCgg==")]
[assembly: go.GoPositionMap("compress/flate/huffman_code.go", "huffman_code.cs", "AC5okoKmgKSCqJKCgoKCgoKWgraCtoK2gvSUpoKCgoKUppKUgoKCgqYABCYADgKClIKCqIKeysSm3IKCuoSCgoLKgoKCloKUgpSCypSCloDKlJSCpoLugpaCgoKmgpSqwoKCgoLchIKCgpQAAxDiuJSUlIKCgpSogqaUlJSWlMqigqaApIKmgMiigqaApIKClKaApII=")]
[assembly: go.GoPositionMap("compress/flate/inflate.go", "inflate.cs", "ACBGgsyAAAoUggAKFoIAKWbYtIKaopKCgpSClIKUAAgUgpaCgoKCggAGEIKWgoKCloKCgoKCgoKUgqiCgpSCgoKCgoLcgpSmgqaUgoKCgoKUuriCuIKUpoKCgswAK16igoCCtoKCgoKClLaCgraAoqSCgrbYsoKCgoKClJSClIKCyoKClMzEtIKAgraCgpSCgoKUgpSCloKCgIK2goKUgpSCuoKCgpSUgoKWkoKClKSCgoKUtIKCtIKCtIKAgraCgoKClIKCqILegpauwpqUpKaEkoKCgpSCgpSCgoKCgpS0gqaCtIK0grSCtIK0grSCtIK0goKAgoK2goKWgoKCgIKCtoKClICCgriUtISCkoCCgraCgoK0griCgpaCloSSgoKUhIKCgoKUuraCloKCgoKUgoKCgpaCgoKWgqqigoKWgoKCgoKCloKCgpSmgoKClJSokoKUpoKCgpSCgoKo+riSgoKCgoKClIKClIKCgoKUgoKCgoKUgoLKgoCCgoK2gqaUpoKEkoKUgpSClIKUuILugoIAAhYACAKEgoKCgoKCAAIU8oSCgoKCgoI=", "767-783:1")]
[assembly: go.GoPositionMap("compress/flate/token.go", "token.cs", "AA8oABw6ABQskKaSqJCmkKSApICmkoKUgpQ=")]
// </GoSourcePositionMaps>

namespace go.compress;

[GoPackage("flate")]
public static partial class flate_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct byFreq {}
    internal partial struct byLiteral {}
    internal partial struct compressionLevel {}
    [GoValueClone("hashHead", "hashPrev", "hashMatch")] internal partial struct compressor {}
    [GoValueClone("h1", "h2", "buf")] internal partial struct decompressor {}
    [GoValueClone("table")] internal partial struct deflateFast {}
    internal partial struct dictDecoder {}
    internal partial struct dictWriter {}
    internal partial struct hcode {}
    [GoValueClone("bytes", "codegenFreq")] internal partial struct huffmanBitWriter {}
    [GoValueClone("chunks")] internal partial struct huffmanDecoder {}
    [GoValueClone("bitCount")] internal partial struct huffmanEncoder {}
    internal partial struct levelInfo {}
    internal partial struct literalNode {}
    internal partial struct tableEntry {}
    internal partial struct token {}
    public partial interface Reader {}
    public partial interface Resetter {}
    public partial struct CorruptInputError {}
    public partial struct InternalError {}
    public partial struct ReadError {}
    public partial struct WriteError {}
    [GoValueClone("d")] public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}
