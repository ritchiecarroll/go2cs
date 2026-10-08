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
using static go.io_package;

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
[assembly: GoImplement<LimitedReader, Reader>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, Seeker>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, WriteSeeker>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, Writer>(Pointer = true)]
[assembly: GoImplement<OffsetWriter, WriterAt>(Pointer = true)]
[assembly: GoImplement<PipeReader, Closer>(Pointer = true)]
[assembly: GoImplement<PipeReader, ReadCloser>(Pointer = true)]
[assembly: GoImplement<PipeReader, Reader>(Pointer = true)]
[assembly: GoImplement<PipeWriter, Closer>(Pointer = true)]
[assembly: GoImplement<PipeWriter, WriteCloser>(Pointer = true)]
[assembly: GoImplement<PipeWriter, Writer>(Pointer = true)]
[assembly: GoImplement<SectionReader, ReadSeeker>(Pointer = true)]
[assembly: GoImplement<SectionReader, Reader>(Pointer = true)]
[assembly: GoImplement<SectionReader, ReaderAt>(Pointer = true)]
[assembly: GoImplement<SectionReader, Seeker>(Pointer = true)]
[assembly: GoImplement<discard, ReaderFrom>]
[assembly: GoImplement<discard, StringWriter>]
[assembly: GoImplement<discard, Writer>]
[assembly: GoImplement<eofReader, Reader>]
[assembly: GoImplement<multiReader, Reader>(Pointer = true)]
[assembly: GoImplement<multiReader, WriterTo>(Pointer = true)]
[assembly: GoImplement<multiWriter, StringWriter>(Pointer = true)]
[assembly: GoImplement<multiWriter, Writer>(Pointer = true)]
[assembly: GoImplement<nopCloser, ReadCloser>]
[assembly: GoImplement<nopCloser, Reader>(Promoted = true)]
[assembly: GoImplement<nopCloserWriterTo, ReadCloser>]
[assembly: GoImplement<nopCloserWriterTo, Reader>(Promoted = true)]
[assembly: GoImplement<nopCloserWriterTo, WriterTo>]
[assembly: GoImplement<teeReader, Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("io/io.go", "io.cs", "ABw6pqYACBK4ygCLAooEsoCCpAACFgALAoKUgoKClIKSgpQAAhTyAAISAAkCgoKUlJQAAh4ADAIAAhTygpSq9oCCtoCCpIKCgIKClLaUgoKCgoKCgqaCgoKUgoKmgoKUpqywAAsWsoKUgpSCgqqigoKCuJQADBqygpSAgqSCgqaSlIKYpKSrAAIQgpSCprKClIKAgoKCgpSkqJCswgAJGKKmsoKCpoKCloKmgpikp6yClIIAAhDSAAcQsoKCgIK2yuyUgqaCppSCyLKCgoKCgoKCgpQABBLCgIKk7oDsgKSCrsKCgoKCgoKUlpQ=")]
[assembly: go.GoPositionMap("io/multi.go", "multi.cs", "AA0Sgu6ylIKAgoK2gqaClIKUlKamgqaygoKAkpSkgoKClJSCppzigoLusoKCgpSCgqamlLKCgoCClIKUpIKUgoKmAAISAAgCgoKAgpS2")]
[assembly: go.GoPositionMap("io/pipe.go", "pipe.cs", "ABIq0oKCgpSiotKCgugADByipMi0goKkyIKClIKAkqbipKSCpoK0goK0puaCgpSCgJKokoKAgqSokoKAgqQABxbSqqIAAhDSAAcW0qqiAAIS4gACJAAPAuo=", "72-72:1;103-103:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("io")]
public static partial class io_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct discard {}
    internal partial struct eofReader {}
    internal partial struct multiReader {}
    internal partial struct multiWriter {}
    internal partial struct nopCloser {}
    internal partial struct nopCloserWriterTo {}
    internal partial struct onceError {}
    internal partial struct pipe {}
    internal partial struct teeReader {}
    public partial interface ByteReader {}
    public partial interface ByteScanner {}
    public partial interface ByteWriter {}
    public partial interface Closer {}
    public partial interface ReadCloser {}
    public partial interface ReadSeekCloser {}
    public partial interface ReadSeeker {}
    public partial interface ReadWriteCloser {}
    public partial interface ReadWriteSeeker {}
    public partial interface ReadWriter {}
    public partial interface Reader {}
    public partial interface ReaderAt {}
    public partial interface ReaderFrom {}
    public partial interface RuneReader {}
    public partial interface RuneScanner {}
    public partial interface Seeker {}
    public partial interface StringWriter {}
    public partial interface WriteCloser {}
    public partial interface WriteSeeker {}
    public partial interface Writer {}
    public partial interface WriterAt {}
    public partial interface WriterTo {}
    public partial struct LimitedReader {}
    public partial struct OffsetWriter {}
    public partial struct PipeReader {}
    public partial struct PipeWriter {}
    public partial struct SectionReader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}
