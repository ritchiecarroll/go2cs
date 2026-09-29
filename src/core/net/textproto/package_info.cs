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
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.net.textproto_package;

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
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ProtocolError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dotReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<dotWriter, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Reader>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Writer>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<ΔError, error>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/textproto/header.go", "header.cs", "AAkaooKssgACENKClIKClAACENKClKiS")]
[assembly: go.GoPositionMap("net/textproto/pipeline.go", "pipeline.cs", "ACFIsoKCgoKqoqqiqqKqogAMINKCgoKUgoKUgoKs0oKCgpSCgoKUgoKUgoI=")]
[assembly: go.GoPositionMap("net/textproto/reader.go", "reader.cs", "ABQsAAke0qqigqiSgoKUrLKCgoKCgpSCpoKUgoKmAAIqABICgqqigoKUgoKUqqKCgpQAAhDSgqiCgpSSloCC7oKChMyEgpSWgoKClIKClJSokoKCgpSUgoKUlKbSgoKUptKCgpSCgoKClIKGlAACKAAVAoKClAACOgAeAoKCgoKCgoKWgoKCgoKCgpSClIKUlAACJgASAoKCAAcS2AANEIKCgoKCgpSUlIKClIKClKaCgpSCgpSmgoKYgoK2ooKWgoK2koKUgraClIKUgpSqooKUgqa+sq7IsoKCgoKCgpSogoKUlJSmAAEqABMCAAIQ+LKCgoKUlrqCloCCgoKClKaCgoKogoKUgoKUgoKogoKohIKCgpSCgpTKgoKUloLOoKqygpSmmNSCgoKUgoKCgpSUlJSUAAIWAAkEgoKCgpSCgpSCgpSUAAcaAAgKABcmAAUgAAwMAAsIAAIcAAoCgqiCgoKmuIKUlIKWguqCpJSClLiAgqQACBCCggAoUg==")]
[assembly: go.GoPositionMap("net/textproto/textproto.go", "textproto.cs", "AChQgs6CAA4gkgAGEJKqooKClAACNgAcAoKCgoKClKiSgpSClKiSgpSClKaCpoKC")]
[assembly: go.GoPositionMap("net/textproto/writer.go", "writer.cs", "ABMqkqaSlrKCgoIAAhQACQKCgqaCggANILKCgoKkgpSUpoKUgoK4goK2gIKklKaigpSCtIKkgqSk")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("textproto")]
public static partial class textproto_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dotReader {}
    internal partial struct dotWriter {}
    internal partial struct sequencer {}
    public partial struct Conn {}
    public partial struct MIMEHeader {}
    public partial struct Pipeline {}
    public partial struct ProtocolError {}
    public partial struct Reader {}
    public partial struct Writer {}
    public partial struct ΔError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}
