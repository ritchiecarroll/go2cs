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
using static go.vendor.golang.org.x.net.http2.hpack_package;

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
[assembly: GoImplement<DecodingError, error>]
[assembly: GoImplement<InvalidIndexError, error>]
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
[assembly: GoFieldAccess(typeof(Decoder))]
[assembly: GoFieldAccess(typeof(Encoder))]
[assembly: GoFieldAccess(typeof(dynamicTable))]
[assembly: GoFieldAccess(typeof(headerFieldTable))]
[assembly: GoFieldAccess(typeof(node))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/encode.go", "encode.cs", "AB5GotyCgszSlIKCgpSCpoKClIKCpoKUpoKClAACFAAKAoKCpoKCpqyygpSClIKokgACFPKCgoK6ksqigoKCAAIU8oKCAAIU8oKCgpSUgoKqooKCggACENKCgpSCgoKUAAIQ0oKCgoKClIKUAAIQ0oKUgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/hpack.go", "hpack.cs", "ABgsggAKDoIADyLC5oKCgpSoAA0aABY4ssqCgoLKvMKssgACFOCYkJqirLIADhaCgqaCgoKokoKCgpTGuKbEgpSClILKgq4ACAKCgpCSgJKAgqSAgqQACQyygoKUgqayuLiClIKCpoKCjNKClIKUgoKm/AAJFIL8woLK2tra2qaokoKCgpSCgpSCqJKCgoKmgoKCgoKClJSCgqaCgpSCgoKCpoKCpoKClIKmgoKCpoKUAAgItoKmgoKClIKUgoLGAAEUAAoCgpSClIKClIKmgoKCgoKCgoKUgoKmzvKClIKCgpTclIKUgoIACxCCgpSCgoKCgpSCgg==", "221-221:1;222-222:2", "", "", "", "248:21,249:13")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/huffman.go", "huffman.cs", "AAwcztKCgoKClNiygoKCgpT6mtKCuKKCgoKCgoKCgpSCgpSCgoKUuIKCgpSClIKUgoKClKaUlKYAECaCAAYQgoKmgoKUlJSClIKCgoKClJSCpIKCgs6muIKCgoKCgoKCuIKqgoKmlKSkgqSCpoKqooKClA==")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/static_table.go", "static_table.cs", "7A==")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/tables.go", "tables.cs", "ACxagoKokqiSgoKCqJKClIKCgoKUgqaCgpSCgpQAAiAADwKCgqaAgqSqwoKUgoKUxgCCAoYE")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.http2;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using io = io_package;
using static hpack_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
// </GlyphForwarderUsings>

[GoPackage("hpack", ImportPath = "vendor/golang.org/x/net/http2/hpack")]
public static partial class hpack_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dynamicTable {}
    internal partial struct headerFieldTable {}
    internal partial struct incomparable {}
    internal partial struct indexType {}
    internal partial struct node {}
    internal partial struct pairNameValue {}
    internal partial struct undecodedString {}
    public partial struct Decoder {}
    public partial struct DecodingError {}
    public partial struct Encoder {}
    public partial struct HeaderField {}
    public partial struct InvalidIndexError {}
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(hpack_package.Decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛDecoder;
[GoMethodHost(typeof(hpack_package.DecodingError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛDecodingError;
[GoMethodHost(typeof(hpack_package.Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛEncoder;
[GoMethodHost(typeof(hpack_package.HeaderField)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛHeaderField;
[GoMethodHost(typeof(hpack_package.InvalidIndexError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛInvalidIndexError;
[GoMethodHost(typeof(hpack_package.dynamicTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛdynamicTable;
[GoMethodHost(typeof(hpack_package.headerFieldTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛheaderFieldTable;
[GoMethodHost(typeof(hpack_package.indexType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class hpackᴛindexType;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class hpackᴛEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteField(this ж<Encoder> Ꮡe, HeaderField f) => hpack_package.WriteFieldᴍ(Ꮡe, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64 i, bool nameValueMatch) searchTable(this ж<Encoder> Ꮡe, HeaderField f) => hpack_package.searchTableᴍ(Ꮡe, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxDynamicTableSize(this ref Encoder e, uint32 v) => hpack_package.SetMaxDynamicTableSizeᴍ(ref e, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint32 /*v*/ MaxDynamicTableSize(this ref Encoder e) => hpack_package.MaxDynamicTableSizeᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxDynamicTableSizeLimit(this ref Encoder e, uint32 v) => hpack_package.SetMaxDynamicTableSizeLimitᴍ(ref e, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldIndex(this ref Encoder e, HeaderField f) => hpack_package.shouldIndexᴍ(ref e, f);
}
partial class hpackᴛDecodingError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this DecodingError de) => hpack_package.Errorᴍ(de);
}
partial class hpackᴛInvalidIndexError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this InvalidIndexError e) => hpack_package.Errorᴍ(e);
}
partial class hpackᴛHeaderField {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsPseudo(this HeaderField hf) => hpack_package.IsPseudoᴍ(hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this HeaderField hf) => hpack_package.Stringᴍ(hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint32 Size(this HeaderField hf) => hpack_package.Sizeᴍ(hf);
}
partial class hpackᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxStringLength(this ref Decoder d, nint n) => hpack_package.SetMaxStringLengthᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetEmitFunc(this ref Decoder d, Action<HeaderField> emitFunc) => hpack_package.SetEmitFuncᴍ(ref d, emitFunc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetEmitEnabled(this ref Decoder d, bool v) => hpack_package.SetEmitEnabledᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool EmitEnabled(this ref Decoder d) => hpack_package.EmitEnabledᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetMaxDynamicTableSizeᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetAllowedMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetAllowedMaxDynamicTableSizeᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxTableIndex(this ref Decoder d) => hpack_package.maxTableIndexᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (HeaderField hf, bool ok) at(this ref Decoder d, uint64 i) => hpack_package.atᴍ(ref d, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<HeaderField>, error) DecodeFull(this ж<Decoder> Ꮡd, slice<byte> p) => hpack_package.DecodeFullᴍ(Ꮡd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Decoder d) => hpack_package.Closeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Write(this ref Decoder d, slice<byte> p) => hpack_package.Writeᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseHeaderFieldRepr(this ref Decoder d) => hpack_package.parseHeaderFieldReprᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseFieldIndexed(this ref Decoder d) => hpack_package.parseFieldIndexedᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseFieldLiteral(this ref Decoder d, uint8 n, indexType it) => hpack_package.parseFieldLiteralᴍ(ref d, n, it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error callEmit(this ref Decoder d, HeaderField hf) => hpack_package.callEmitᴍ(ref d, hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseDynamicTableSizeUpdate(this ref Decoder d) => hpack_package.parseDynamicTableSizeUpdateᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (undecodedString u, slice<byte> remain, error err) readString(this ref Decoder d, slice<byte> p) => hpack_package.readStringᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) decodeString(this ref Decoder d, undecodedString u) => hpack_package.decodeStringᴍ(ref d, u);
}
partial class hpackᴛdynamicTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMaxSize(this ref dynamicTable dt, uint32 v) => hpack_package.setMaxSizeᴍ(ref dt, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref dynamicTable dt, HeaderField f) => hpack_package.addᴍ(ref dt, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void evict(this ref dynamicTable dt) => hpack_package.evictᴍ(ref dt);
}
partial class hpackᴛindexType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool indexed(this indexType v) => hpack_package.indexedᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sensitive(this indexType v) => hpack_package.sensitiveᴍ(v);
}
partial class hpackᴛheaderFieldTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref headerFieldTable t) => hpack_package.initᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this ref headerFieldTable t) => hpack_package.lenᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addEntry(this ref headerFieldTable t, HeaderField f) => hpack_package.addEntryᴍ(ref t, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void evictOldest(this ref headerFieldTable t, nint n) => hpack_package.evictOldestᴍ(ref t, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64 i, bool nameValueMatch) search(this ж<headerFieldTable> Ꮡt, HeaderField f) => hpack_package.searchᴍ(Ꮡt, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 idToIndex(this ж<headerFieldTable> Ꮡt, uint64 id) => hpack_package.idToIndexᴍ(Ꮡt, id);
}
// </GlyphForwarders>
