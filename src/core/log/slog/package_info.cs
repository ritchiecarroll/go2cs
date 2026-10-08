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
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("Level", "ΔLevel")]
[assembly: GoTypeAlias("LogValuer", "ΔLogValuer")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<JSONHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]
[assembly: GoImplement<TextHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<defaultHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<discardHandler, ΔHandler>]
[assembly: GoImplement<handlerWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<ΔLevel, Leveler>(Pointer = true)]
[assembly: GoImplement<ΔLevel, Leveler>]
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
[assembly: go.GoPositionMap("log/slog/attr.go", "attr.cs", "ABAkkqiSqqKokqiSqJKqoqiSAAIUAAkCpoKYgoKUqqKokqaCqqI=")]
[assembly: go.GoPositionMap("log/slog/handler.go", "handler.cs", "AFu+AYLcgqwACAKCgoKCkoKC1oKmggBCmAGUAAscwoKClKbmgpSUkoKCgIKCgsiCgoKmppTWgoKCqvKSgoKmgoKUgoKCgoKUuIKCgoKUpoKUgoKCgpSUgoKEgoKC1sSAgoKCgoLsgoLKgoKCgoKClJSCgqaUgqa6koKUAA4egoLGou6CgpSmgoKUgIKCpKaCggAGEqKCgoKUgqaCupKClKSCgryigoKCpqzSgoCCgoKmlLaCpoCCgIKClNiClMqUgpSCgpSCuIKUpoKmgoKUlJSClJSmgoKCgqaClPrCkoDcgJKCuLiCgpSUgrSkooKUuIrCgoKCgsrIgKKAooCigA==", "341-346:1;557-572:1", "", "200=Clip/1/2/3,Clip/2/2/5;414=New/1/1/5")]
[assembly: go.GoPositionMap("log/slog/json_handler.go", "json_handler.cs", "ABw80oKUAAkYoqqipoIAAkIAIALYsoCmpIKCpqKUpKSqgLLGprSkgoKAgpTGpKakkoKCgIKkgoIABxDSgJKAlIKCgIKCgpSClIKUtLS0toKCxIKCpIKCgpSCgoIACBKCgpSCgoKClJSClAAMGg==", "161-161:1;162-162:2")]
[assembly: go.GoPositionMap("log/slog/level.go", "level.cs", "ADZ2AAkCgoKUlpSkpKTM2AACENKCgpSqoqqiAAIQ0tbSgoKogoKAgoKCgraUpKSkpKSC6qAACxiSqJKmgqqiqqKqooKAgqSC", "60-65:1;123-127:1")]
[assembly: go.GoPositionMap("log/slog/logger.go", "logger.cs", "ABxYABgCgoKmgqiQrvLugIKSggAMGIKCgpSChJKCqIKCggAMGoKSqJCq4oKUgoIAAhIACAKClIKCqJKClKiyqJKClKyyAAIaAAwCqLKosqiyqLKosqiyqLKosqiyrNKClIKClIKUgoKClKiygpSCgpSClIKCgpSosqiyqLKosqiyqLKosqiyqLKosg==")]
[assembly: go.GoPositionMap("log/slog/record.go", "record.cs", "AC900gAHFtKCqLKqwoKCpoKCAAgOwoKCgoKUgriCgpSCpoKCgoIABBDSgoKCgpSCgpSClMySgoKCgIK2AAQW4pSClKa2ABQowoKClIKUgpSu4oKC")]
[assembly: go.GoPositionMap("log/slog/text_handler.go", "text_handler.cs", "ABs40oKUAAkYoqqipoIAAk4AJgKmopSkpICCgoKmgqSAlIKkpKSssoCCtoKClKaCgpSCgqaClIKUgoKUlA==")]
[assembly: go.GoPositionMap("log/slog/value.go", "value.cs", "ADp8AA8agoKUAAQQkpSkxKSkpLS+wqiSqJKokqiSqJKCgpQABhyiypSCgpS4qJKq+ICCgoKCpqSokoKCgqYAAiQADwKUpKSkpKSkpKSkpKSkpKSkpKSkpLS+spSAgqSkpKSkpKSkpKSkzrKAgqSCpoKqooCSpKqigJKkqqKAkqSmgqqigJKmpoKqooCSpqaCqqKAkqSokpSClKS0zKKqooCCpKaCrLKCgoKUlKSkpKSkpMqSgsqqopSkpKSkpKSkpKQADioACgKCgoCCuIKClJSCAAkGgoKCgpSCgoKCgoKClIKCgqY=", "502-506:1")]
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
    internal partial struct kind {}
    internal partial struct timeTime {}
    public partial interface Leveler {}
    public partial interface ΔHandler {}
    public partial interface ΔLogValuer {}
    public partial struct Attr {}
    public partial struct HandlerOptions {}
    public partial struct JSONHandler {}
    public partial struct LevelVar {}
    public partial struct Logger {}
    [GoValueClone("front")] public partial struct Record {}
    public partial struct Source {}
    public partial struct TextHandler {}
    public partial struct Value {}
    public partial struct ΔKind {}
    public partial struct ΔLevel {}
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
