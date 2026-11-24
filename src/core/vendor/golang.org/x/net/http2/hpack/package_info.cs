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
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/encode.go", "encode.cs", "AB1GotyCgqzSlIKCgpSCpoKClIKCpoKUpoKClAACFAAKAoKCpoKCpqyygpSClIKokgACFPKCgoK6kqqigoKCAAIU8oKCAAIU8oKCgpSUgoKqooKCggACENKCgpSCgoKUAAIQ0oKCgoKClIKUAAIQ0oKUgpQ=")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/hpack.go", "hpack.cs", "ABUsgu6CAAsiwuaCgoKUqAANGgAUOLLKgoKCypzCrLIAAhTgmJCaoqyyAAoWgoKmgoKCqJKCgoKUprimxIKUgpSCyoKuAAgCgoKQkoCSgIKkgIKkAAkMsoKClIKmsri4gpSCgqaCgozSgpSClIKCptwABxSC3MKCytra2tqmqJKCgoKUgoKUgqiSgoKCpoKCgoKCgpSUgoKmgoKUgoKCgqaCgqaCgpSCpoKCgqaClAAICLaCpoKCgpSClIKCpgABFAAKAoKUgpSCgpSCpoKCgoKCgoKClIKCpq7ygpSCgoKU3JSClIKCAAcQgoKUgoKCgoKUgoI=", "221-221:1;222-222:2", "", "", "", "227:61,228:13")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/huffman.go", "huffman.cs", "AAwcztKCgoKClNiygoKCgpT6mtKCuKKCgoKCgoKCgpSCgpSCgoKUuIKCgpSClIKUgoKClKaUlKYAECaCAAYQgoKmgoKUlJSClIKCgoKClJSCpIKCgs6muIKCgoKCgoKCuIKqgoKmlKSkgqSCpoKqooKClA==")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/static_table.go", "static_table.cs", "7A==")]
[assembly: GoPositionMap("vendor/golang.org/x/net/http2/hpack/tables.go", "tables.cs", "AClagoKokqiSgoKCqJKClIKCgoKUgqaCgpSCgpQAAiAADwKCgqaAgqSqwoKUgoKUpgCCAoYE")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.http2;

using static go.vendor.golang.org.x.net.http2.hpack_package;

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
[GoMethodHost(typeof(hpack_package.Decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛDecoder;
[GoMethodHost(typeof(hpack_package.DecodingError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛDecodingError;
[GoMethodHost(typeof(hpack_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛEncoder;
[GoMethodHost(typeof(hpack_package.HeaderField)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛHeaderField;
[GoMethodHost(typeof(hpack_package.InvalidIndexError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛInvalidIndexError;
[GoMethodHost(typeof(hpack_package.dynamicTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛdynamicTable;
[GoMethodHost(typeof(hpack_package.headerFieldTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛheaderFieldTable;
[GoMethodHost(typeof(hpack_package.indexType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpackᴛindexType;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class hpackᴛDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxStringLength(this ref Decoder d, nint n) => hpack_package.SetMaxStringLength⁔(ref d, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetEmitFunc(this ref Decoder d, Action<HeaderField> emitFunc) => hpack_package.SetEmitFunc⁔(ref d, emitFunc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetEmitEnabled(this ref Decoder d, bool v) => hpack_package.SetEmitEnabled⁔(ref d, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool EmitEnabled(this ref Decoder d) => hpack_package.EmitEnabled⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetMaxDynamicTableSize⁔(ref d, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetAllowedMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetAllowedMaxDynamicTableSize⁔(ref d, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxTableIndex(this ref Decoder d) => hpack_package.maxTableIndex⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (HeaderField hf, bool ok) at(this ref Decoder d, uint64 i) => hpack_package.at⁔(ref d, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<HeaderField>, error) DecodeFull(this ж<Decoder> Ꮡd, slice<byte> p) => hpack_package.DecodeFull⁔(Ꮡd, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Decoder d) => hpack_package.Close⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ref Decoder d, slice<byte> p) => hpack_package.Write⁔(ref d, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseHeaderFieldRepr(this ref Decoder d) => hpack_package.parseHeaderFieldRepr⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseFieldIndexed(this ref Decoder d) => hpack_package.parseFieldIndexed⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseFieldLiteral(this ref Decoder d, uint8 n, indexType it) => hpack_package.parseFieldLiteral⁔(ref d, n, it);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error callEmit(this ref Decoder d, HeaderField hf) => hpack_package.callEmit⁔(ref d, hf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseDynamicTableSizeUpdate(this ref Decoder d) => hpack_package.parseDynamicTableSizeUpdate⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (undecodedString u, slice<byte> remain, error err) readString(this ref Decoder d, slice<byte> p) => hpack_package.readString⁔(ref d, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) decodeString(this ref Decoder d, undecodedString u) => hpack_package.decodeString⁔(ref d, u);
}
partial class hpackᴛDecodingError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this DecodingError de) => hpack_package.Error⁔(de);
}
partial class hpackᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteField(this ж<Encoder> Ꮡe, HeaderField f) => hpack_package.WriteField⁔(Ꮡe, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 i, bool nameValueMatch) searchTable(this ж<Encoder> Ꮡe, HeaderField f) => hpack_package.searchTable⁔(Ꮡe, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxDynamicTableSize(this ref Encoder e, uint32 v) => hpack_package.SetMaxDynamicTableSize⁔(ref e, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 /*v*/ MaxDynamicTableSize(this ref Encoder e) => hpack_package.MaxDynamicTableSize⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxDynamicTableSizeLimit(this ref Encoder e, uint32 v) => hpack_package.SetMaxDynamicTableSizeLimit⁔(ref e, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldIndex(this ref Encoder e, HeaderField f) => hpack_package.shouldIndex⁔(ref e, f);
}
partial class hpackᴛHeaderField {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsPseudo(this HeaderField hf) => hpack_package.IsPseudo⁔(hf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this HeaderField hf) => hpack_package.String⁔(hf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 Size(this HeaderField hf) => hpack_package.Size⁔(hf);
}
partial class hpackᴛInvalidIndexError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this InvalidIndexError e) => hpack_package.Error⁔(e);
}
partial class hpackᴛdynamicTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMaxSize(this ref dynamicTable dt, uint32 v) => hpack_package.setMaxSize⁔(ref dt, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref dynamicTable dt, HeaderField f) => hpack_package.add⁔(ref dt, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void evict(this ref dynamicTable dt) => hpack_package.evict⁔(ref dt);
}
partial class hpackᴛheaderFieldTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref headerFieldTable t) => hpack_package.init⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this ref headerFieldTable t) => hpack_package.len⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addEntry(this ref headerFieldTable t, HeaderField f) => hpack_package.addEntry⁔(ref t, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void evictOldest(this ref headerFieldTable t, nint n) => hpack_package.evictOldest⁔(ref t, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 i, bool nameValueMatch) search(this ж<headerFieldTable> Ꮡt, HeaderField f) => hpack_package.search⁔(Ꮡt, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 idToIndex(this ж<headerFieldTable> Ꮡt, uint64 id) => hpack_package.idToIndex⁔(Ꮡt, id);
}
partial class hpackᴛindexType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool indexed(this indexType v) => hpack_package.indexed⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool sensitive(this indexType v) => hpack_package.sensitive⁔(v);
}
// </MethodForwarders>
