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
using static go.log.slog_package;

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
[assembly: GoImplement<JSONHandler, Handler>(Pointer = true)]
[assembly: GoImplement<Level, Leveler>(Pointer = true)]
[assembly: GoImplement<Level, Leveler>]
[assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]
[assembly: GoImplement<MultiHandler, Handler>(Pointer = true)]
[assembly: GoImplement<TextHandler, Handler>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<defaultHandler, Handler>(Pointer = true)]
[assembly: GoImplement<discardHandler, Handler>]
[assembly: GoImplement<handlerWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(LevelVar))]
[assembly: GoFieldAccess(typeof(Logger))]
[assembly: GoFieldAccess(typeof(Source))]
[assembly: GoFieldAccess(typeof(commonHandler))]
[assembly: GoFieldAccess(typeof(jsonEncoder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("log/slog/attr.go", "attr.cs", "ABIkkqiSqqKokqiSqJKqoqiSAAIUAAkCAAIQ8qaCmIKClKqiyJKmgqqi")]
[assembly: GoPositionMap("log/slog/handler.go", "handler.cs", "AGHIAYL8gqwACAKCgoKCgoKC1oKmggBPmAGUAAscooKClKbmgpSUgoKCgoKCuIKCgqamlNaCgoKq8oKCgqaCgpSCgoKCgpS4goKCgpSmgoKClJSCgoKClJSCgpSCgoL2xIKCgoKC3IKCyoKCgoKCgpSUgoKmlIKm2pKClAAQHoKC5qLugoKUxoKClIKClKaCggAKEqKCgoKUgqaCupKClKSCgryigoKCpqzSgoKCgqaUpoKmgoKClIKUuIKUypSClIKClIK4gpSmgqaCgoKUlIKUlKiSgpSCgoK0grSkpoKCgoKmgpQACArCktyCgri4goKUlIK0pKKClNiKwoKCgoLK6IKCgg==", "350-355:1;586-601:1", "", "220=Clip/1/2/3,Clip/2/2/5;439=New/1/1/5", "", "651:21,652:13")]
[assembly: GoPositionMap("log/slog/json_handler.go", "json_handler.cs", "AB080oKUAAsYoqqipoIAAj4AHgIACAiyppSCgqailLS06oCC1sa0tIKCgIKU1qQACBKUpoKCuMKClJKClIKmgqaCggANENKAkoCkgoKCgoKUgpSClLS0tMaCgqSCgpSCgoKUgoKCAAgSgoKUgoKCgpSUgpQADRo=", "159-167:1;184-184:1;185-185:2", "", "", "", "204:21,206:13")]
[assembly: GoPositionMap("log/slog/level.go", "level.cs", "ADx2AAkCgoKUgoKUppSkpKS82AACENKCgpSqoqqiAAIQ0ubSgoK4goKCgoKCpqS0tLS0lILqkAAOGJKokqaCqqKqoqqigoKUgg==", "60-69:1;127-131:1", "", "", "", "150:21,151:13")]
[assembly: GoPositionMap("log/slog/logger.go", "logger.cs", "ABxYABgCgoKmgqiAnvLugoKCAA0YgoKClIKEkoK4goKCABAagoKogJrigpSCggACEgAIAoKUgoLIkoKUqLLIkoKUzLIABBoADAKosqiyqLKosqiyqLKosqiyqLKs0oKUgpSCgpSClIKCqLKClIKUgoKUgpSCgsiyqLKosqiyqLKosqiyqLKosqiy")]
[assembly: GoPositionMap("log/slog/multi_handler.go", "multi_handler.cs", "AAwasoKCAAsUgoKCpqaygoKCgrimgoKClKaCgoKU")]
[assembly: GoPositionMap("log/slog/record.go", "record.cs", "ADB00gAJFtKCqLKqwoKCpoKCAAoOwoKCgoKUgriCgpSCpoKCgoIABBDSgoKCgpSCgpSClOySgoKCgqYABRbilIKUprYAGSjCgoKUgpSClKiwytKCpoKC")]
[assembly: GoPositionMap("log/slog/text_handler.go", "text_handler.cs", "ABs40oKUAAsYoqqipoIAAkoAJALGopS0tIKCgqaClJSClLSkrLKCpoKClKaCgpSCgqaClIKUgoKUlA==")]
[assembly: GoPositionMap("log/slog/value.go", "value.cs", "ADt8ABIagoKUAAoQkpSkxKSkpLTesqiSqJKokqiSqJKCgpQACRyiypSCgpS4qJKq+IKCgoKmlKiSgoKCpgACJAAPApSkpKSkpKSkpKSkpKSkpKSkpKSktN6ylIKUpKSkpKSkpKSkpM6ygpSCpoKqopKUqqKSlKqikpSmgqqikqamgqqikqamgqqikpSokpSClKS0zKKqooKUpoKssoKCgpSUpKSkpKSkypKCyqqilKSkpKSkpKSkpAASKgAKAoKCgriCgpSUggAMBoKCgoKUgoKCgoKCgpSCgoKm", "502-506:1", "", "", "", "589:21,590:13")]
// </GoSourcePositionMaps>

namespace go.log;

[GoPackage("slog")]
public static partial class slog_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial class groupptr {}
    internal partial class stringptr {}
    internal partial class timeLocation {}
    internal partial struct commonHandler {}
    internal partial struct defaultHandler {}
    internal partial struct discardHandler {}
    internal partial struct handleState {}
    internal partial struct handlerWriter {}
    internal partial struct jsonEncoder {}
    internal partial struct kind {}
    internal partial struct timeTime {}
    public partial interface Handler {}
    public partial interface Leveler {}
    public partial interface LogValuer {}
    public partial struct Attr {}
    public partial struct HandlerOptions {}
    public partial struct JSONHandler {}
    public partial struct Kind {}
    public partial struct Level {}
    public partial struct LevelVar {}
    public partial struct Logger {}
    public partial struct MultiHandler {}
    [GoValueClone("front")] public partial struct Record {}
    public partial struct Source {}
    public partial struct TextHandler {}
    public partial struct Value {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸbuffer() => builtin.initPackage(typeof(go.log.slog.@internal.buffer_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
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
[GoMethodHost(typeof(slog_package.Attr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛAttr;
[GoMethodHost(typeof(slog_package.JSONHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛJSONHandler;
[GoMethodHost(typeof(slog_package.Kind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛKind;
[GoMethodHost(typeof(slog_package.Level)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLevel;
[GoMethodHost(typeof(slog_package.LevelVar)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLevelVar;
[GoMethodHost(typeof(slog_package.Logger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLogger;
[GoMethodHost(typeof(slog_package.MultiHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛMultiHandler;
[GoMethodHost(typeof(slog_package.Record)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛRecord;
[GoMethodHost(typeof(slog_package.Source)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛSource;
[GoMethodHost(typeof(slog_package.TextHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛTextHandler;
[GoMethodHost(typeof(slog_package.Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛValue;
[GoMethodHost(typeof(slog_package.commonHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛcommonHandler;
[GoMethodHost(typeof(slog_package.defaultHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛdefaultHandler;
[GoMethodHost(typeof(slog_package.discardHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛdiscardHandler;
[GoMethodHost(typeof(slog_package.handleState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛhandleState;
[GoMethodHost(typeof(slog_package.handlerWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛhandlerWriter;
// </MethodHolders>
