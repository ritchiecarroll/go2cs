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
global using bigꓸInt = go.math.big_package.ΔInt;
global using gojqꓸModuleLoader = object;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static go.github.com.itchyny.gojq.cli_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b45786974436f6465282920696e747d", "run_typeᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b466428292075696e747074727d", "runInternal_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b4a534f4e50617273654572726f7228292028737472696e672c20737472696e672c206572726f72297d", "runInternal_typeᴛ2")]
[assembly: GoDynamicTypeLift("696e746572666163657b517565727950617273654572726f7228292028737472696e672c20737472696e672c206572726f72297d", "runInternal_typeᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b6973456d7074794572726f7228297d", "run_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<compileError, error>(Pointer = true)]
[assembly: GoImplement<emptyError, error>(Pointer = true)]
[assembly: GoImplement<encoder, marshaler>(Pointer = true)]
[assembly: GoImplement<exitCodeError, error>(Pointer = true)]
[assembly: GoImplement<filesInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<flagParseError, error>(Pointer = true)]
[assembly: GoImplement<go.github.com.itchyny.gojq_package.HaltError, error>(Pointer = true)]
[assembly: GoImplement<inputReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<inputReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<jsonInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<jsonParseError, error>(Pointer = true)]
[assembly: GoImplement<nullInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<queryParseError, error>(Pointer = true)]
[assembly: GoImplement<rawInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<rawMarshaler, marshaler>(Pointer = true)]
[assembly: GoImplement<readAllIter, inputIter>(Pointer = true)]
[assembly: GoImplement<slurpInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<slurpRawInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<yamlInputIter, inputIter>(Pointer = true)]
[assembly: GoImplement<yamlMarshaler, marshaler>(Pointer = true)]
[assembly: GoImplement<yamlParseError, error>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.encoding.json_package.SyntaxError))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.go_yaml.yaml_package.ParserError))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.go_yaml.yaml_package.TypeError))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.go_yaml.yaml_package.UnmarshalError))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.gojq_package.ParseError))]
[assembly: GoFieldAccess(typeof(inputReader))]
[assembly: GoFieldAccess(typeof(jsonInputIter))]
[assembly: GoFieldAccess(typeof(stringScanner))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/cli.go", "cli.cs", "AFesAaKCgpSClJQADgbigoKClIIADRiClIKClIiAkoKCgpSClIKCgriAgoKCgraClISCgpSCgoKUgpSCgoKUgpSCgoKUgpSCgpSCgoKCgpSClLiCuJKCgpSCgpSCgpSUgoKCgriCgpSCgpSCkgAIEJKClLyChqKUhqKUlIKU5qKWkoKClNbSgpSClMa0tLSCgoKUuIKU5oKCgoKClICCgoKkgIKCgoKUgoKmgpSCtoKUpoKCgoKClICCpIKUlICCpIKClKaCgoKCuKaCgpSCgoKCgoKUgoKU1oKAhKSAgqSmgoSU", "129-129:1;225-229:2;248-255:3;249-254:3.1;309-315:1", "", "128=Version/1/1/11;313=newFilesInputIter/1/1/1;479=marshal/1/1/1;492=marshal/1/1/1", "266:21,316:21", "150:27,151:13,250:25,251:17,357:25,358:17")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/color.go", "color.cs", "AAoWgqaigrqCkpKSkpKSkqaCgoKCgoKUpqaCgriCgoKUlKY=")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/encoder.go", "encoder.cs", "ABsyhKaCgoKmgoKmgpS0gpTGtLS0tLSAgtaAguakgpSogoKClIKCgpSChIKCpqiCgpSCgoKCgoKUgpSUtLS0tLS0tIKCgrSCgpSCgoKUgoKClJSClIKCuIKCgoKClIKUgqaCgpSC9oKCgoqCgoKUloKClIKUgoKClIKmgoKUgqaCgoKClMqCgpSCgoKUyoKClIKCuIKClIKC", "197-199:1")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/error.go", "error.cs", "ABgsgqaUgoKU7oKmlILugqaC7oKmggAHEIKCgoKUgoKmuIIACBKCgoKCgpSCgqYACBKCgoKCgoKCgoKCgoK4grjCgoKCgoKUgoKCgqaCgoKClIKClJSCpoKCgoKCgpSmpoKCAAcQwoKUgoKCgoKUgoKUgqiCqqKClIKU", "", "", "89=formatLineInfo/1/1/1;115=formatLineInfo/1/1/1;144=formatLineInfo/1/1/1;231=IndexByte/2/2/1")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/flags.go", "flags.cs", "AAkUgoKCgoKCgoKAgoKCtoCCtoKCgoKCqsiCtIKCgoKUgoKmgtiCgoKCgpSCgqaCgsaCgpSUlIKUtIKUtIKClIKClILGgoKCppSClMaClIKUgoKC1oKCgoKUgoKUlIKUlIKmpoKCgoKCgoKClIKCgIKCgpSkgoCCgpSUgqSAgoKkgoKU")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/inputs.go", "inputs.cs", "ABYsgoKCpoKm4oKUgIKkgoKCgpSCgoKCpoKClJSCABUkgoKCgrCAkqaigpSCgoKClIKCgoKCgpKmgpSCgoKUpoKCpoKmgoKCgu6CpoKClIKmgoKmggALGIamgoKUgoKCgoKClJSCgoKUgoKUlIKUlICCpICCpLiCgoKUgpSmgoKUAAgSgqaCgpSCgoKClIKmpoKCpoIACRSCgoKmgoKUgoKCgpSClKaCgqaCAAcQgqaCgpSCgoKCgoKClIKUuIKCgoKUpoIACBKCpoKClIKCgpSmgoKmggAHEIKmooKUgoKCgoKCgpSClLiCgoKClKaC", "81-81:1", "", "52=LimitReader/1/1/1")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/marshaler.go", "marshaler.cs", "ABMogoKClIKUpoLugoKClJSClA==")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/run.go", "run.cs", "7II=", "", "", "8=run/1/1/4")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/cli/stream.go", "stream.cs", "AA4cggANIIKkgqSkgpS0xoKCgoKUlIKUlLS0goKUxoKCgpSCpIKClIK2pIKkpIKkgqSC7IKCgg==")]
// </GoSourcePositionMaps>

namespace go.github.com.itchyny.gojq;

[GoPackage("cli", ImportPath = "github.com/itchyny/gojq/cli")]
public static partial class cli_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface inputIter {}
    internal partial interface marshaler {}
    internal partial interface runInternal_type {}
    internal partial interface runInternal_typeᴛ1 {}
    internal partial interface runInternal_typeᴛ2 {}
    internal partial interface run_type {}
    internal partial interface run_typeᴛ1 {}
    internal partial struct cli {}
    internal partial struct compileError {}
    internal partial struct emptyError {}
    [GoLocalName("keyVal")] internal partial struct encodeObject_keyVal {}
    [GoValueClone("buf")] internal partial struct encoder {}
    internal partial struct exitCodeError {}
    internal partial struct filesInputIter {}
    internal partial struct flagParseError {}
    internal partial struct flagopts {}
    internal partial struct inputReader {}
    internal partial struct jsonInputIter {}
    internal partial struct jsonParseError {}
    internal partial struct jsonStream {}
    internal partial struct nullInputIter {}
    internal partial struct queryParseError {}
    internal partial struct rawInputIter {}
    internal partial struct rawMarshaler {}
    internal partial struct readAllIter {}
    internal partial struct slurpInputIter {}
    internal partial struct slurpRawInputIter {}
    internal partial struct stringScanner {}
    internal partial struct yamlInputIter {}
    internal partial struct yamlMarshaler {}
    internal partial struct yamlParseError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸitchynyꓸgo_yaml() => builtin.initPackage(typeof(go.github.com.itchyny.go_yaml.yaml_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸitchynyꓸgojq() => builtin.initPackage(typeof(go.github.com.itchyny.gojq_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸmattnꓸgo_isatty() => builtin.initPackage(typeof(go.github.com.mattn.go_isatty.isatty_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸmattnꓸgo_runewidth() => builtin.initPackage(typeof(go.github.com.mattn.go_runewidth.runewidth_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}
