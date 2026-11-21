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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(LimitedReader))]
[assembly: GoFieldAccess(typeof(PipeReader))]
[assembly: GoFieldAccess(typeof(PipeWriter))]
[assembly: GoFieldAccess(typeof(multiReader))]
[assembly: GoFieldAccess(typeof(multiWriter))]
[assembly: GoFieldAccess(typeof(pipe))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("io/io.go", "io.cs", "ABo6pqYACBK4ygCLAooEsoKUAAIWAAsCgpSCgoKUgoKClAACFPIAAhIACQKCgpSUlAACHgAMAgACFPKClKr2gqaClIKCgoKUppSCgoKCgoKCpoKCgpSCgqaCgpSmrKAAChaygpSClIKCqqKCgoK4lAAMGrKClIKUgoKmgpSClKS0tKSClIKmsoKUgoKCgoKUlKiAnMIACRiiprKCgqaCgqaCpoKUpLSkgpSCAAIQ0gAHELKCgoKmyuyUgqaCppSCyLKCgoKCgoKCgpQABBLCgpTu3JSCrgAIBsqChJKCgoKCgpSCuIKCgpSCppSCgoI=")]
[assembly: GoPositionMap("io/multi.go", "multi.cs", "AA0Sgu6ylIKCgqaCpoKUgpSUpqaCprKCgoKUlIKCgpSUgqac4oKC7rKCgoKUgoKmppSygoKClIKUlIKUgoKmAAISAAgCgoKClKY=")]
[assembly: GoPositionMap("io/pipe.go", "pipe.cs", "ABIq0oKCgpSiktKCgugADByipNi0goKkyIKClIKAkqbipKSCtoK0goK0puaCgpSCgJKokoKClKiSgoKUAAcW0qqiAAIQ0gAHFtKqogACEuIAAiQADwLq", "72-72:1;103-103:1")]
// </GoSourcePositionMaps>

namespace go;

using static go.io_package;

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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(io_package.LimitedReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛLimitedReader;
[GoMethodHost(typeof(io_package.OffsetWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛOffsetWriter;
[GoMethodHost(typeof(io_package.PipeReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛPipeReader;
[GoMethodHost(typeof(io_package.PipeWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛPipeWriter;
[GoMethodHost(typeof(io_package.SectionReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛSectionReader;
[GoMethodHost(typeof(io_package.discard)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛdiscard;
[GoMethodHost(typeof(io_package.eofReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛeofReader;
[GoMethodHost(typeof(io_package.multiReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛmultiReader;
[GoMethodHost(typeof(io_package.multiWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛmultiWriter;
[GoMethodHost(typeof(io_package.nopCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛnopCloser;
[GoMethodHost(typeof(io_package.nopCloserWriterTo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛnopCloserWriterTo;
[GoMethodHost(typeof(io_package.onceError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛonceError;
[GoMethodHost(typeof(io_package.pipe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛpipe;
[GoMethodHost(typeof(io_package.teeReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ioᴛteeReader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class ioᴛLimitedReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref LimitedReader l, slice<byte> p) => io_package.Read⁔(ref l, p);
}
partial class ioᴛOffsetWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ref OffsetWriter o, slice<byte> p) => io_package.Write⁔(ref o, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) WriteAt(this ref OffsetWriter o, slice<byte> p, int64 off) => io_package.WriteAt⁔(ref o, p, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ref OffsetWriter o, int64 offset, nint whence) => io_package.Seek⁔(ref o, offset, whence);
}
partial class ioᴛPipeReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ж<PipeReader> Ꮡr, slice<byte> data) => io_package.Read⁔(Ꮡr, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<PipeReader> Ꮡr) => io_package.Close⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseWithError(this ж<PipeReader> Ꮡr, error err) => io_package.CloseWithError⁔(Ꮡr, err);
}
partial class ioᴛPipeWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ж<PipeWriter> Ꮡw, slice<byte> data) => io_package.Write⁔(Ꮡw, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<PipeWriter> Ꮡw) => io_package.Close⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CloseWithError(this ж<PipeWriter> Ꮡw, error err) => io_package.CloseWithError⁔(Ꮡw, err);
}
partial class ioᴛSectionReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref SectionReader s, slice<byte> p) => io_package.Read⁔(ref s, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ref SectionReader s, int64 offset, nint whence) => io_package.Seek⁔(ref s, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) ReadAt(this ref SectionReader s, slice<byte> p, int64 off) => io_package.ReadAt⁔(ref s, p, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref SectionReader s) => io_package.Size⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ReaderAt r, int64 off, int64 n) Outer(this ref SectionReader s) => io_package.Outer⁔(ref s);
}
partial class ioᴛdiscard {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this discard _, slice<byte> p) => io_package.Write⁔(_, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) WriteString(this discard _, @string s) => io_package.WriteString⁔(_, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this discard _, Reader r) => io_package.ReadFrom⁔(_, r);
}
partial class ioᴛeofReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this eofReader _Δp0, slice<byte> _Δp1) => io_package.Read⁔(_Δp0, _Δp1);
}
partial class ioᴛmultiReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref multiReader mr, slice<byte> p) => io_package.Read⁔(ref mr, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 sum, error err) WriteTo(this ref multiReader mr, Writer w) => io_package.WriteTo⁔(ref mr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 sum, error err) writeToWithBuffer(this ref multiReader mr, Writer w, slice<byte> buf) => io_package.writeToWithBuffer⁔(ref mr, w, buf);
}
partial class ioᴛmultiWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref multiWriter t, slice<byte> p) => io_package.Write⁔(ref t, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref multiWriter t, @string s) => io_package.WriteString⁔(ref t, s);
}
partial class ioᴛnopCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this nopCloser _) => io_package.Close⁔(_);
}
partial class ioᴛnopCloserWriterTo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this nopCloserWriterTo _) => io_package.Close⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) WriteTo(this nopCloserWriterTo c, Writer w) => io_package.WriteTo⁔(c, w);
}
partial class ioᴛonceError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Store(this ж<onceError> Ꮡa, error err) => io_package.Store⁔(Ꮡa, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Load(this ж<onceError> Ꮡa) => io_package.Load⁔(Ꮡa);
}
partial class ioᴛpipe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) => io_package.read⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeRead(this ж<pipe> Ꮡp, error err) => io_package.closeRead⁔(Ꮡp, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) => io_package.write⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeWrite(this ж<pipe> Ꮡp, error err) => io_package.closeWrite⁔(Ꮡp, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readCloseError(this ж<pipe> Ꮡp) => io_package.readCloseError⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeCloseError(this ж<pipe> Ꮡp) => io_package.writeCloseError⁔(Ꮡp);
}
partial class ioᴛteeReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref teeReader t, slice<byte> p) => io_package.Read⁔(ref t, p);
}
// </MethodForwarders>
