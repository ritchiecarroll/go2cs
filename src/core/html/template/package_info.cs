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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
using parse = go.text.template.parse_package;
using template = go.text.template_package;
// </ImportedTypeAliases>

using go;
using static go.html.template_package;

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
[assembly: GoTypeAlias("FuncMap", "go.text.template_package.FuncMap")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<ΔError, error>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<parse.ActionNode, ж<parse.ActionNode>>]
[assembly: GoImplicitConv<parse.BranchNode, ж<parse.BranchNode>>]
[assembly: GoImplicitConv<parse.ListNode, ж<parse.ListNode>>(Indirect = true)]
[assembly: GoImplicitConv<parse.ListNode, ж<parse.ListNode>>]
[assembly: GoImplicitConv<parse.TemplateNode, ж<parse.TemplateNode>>]
[assembly: GoImplicitConv<parse.TextNode, ж<parse.TextNode>>]
[assembly: GoImplicitConv<rangeContext, ж<rangeContext>>]
[assembly: GoImplicitConv<template.Template, ж<template.Template>>]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("html/template/attr.go", "attr.cs", "ABQmAHnyAaK4ooKCpqSAgraCAAkWhpQ=")]
[assembly: go.GoPositionMap("html/template/attr_string.go", "attr_string.cs", "/oaigoKCgoLKlIKClA==")]
[assembly: go.GoPositionMap("html/template/content.go", "content.cs", "ACXmAaKClICUpIKClKiCnLKClIKClKrCgpSkpKSkpKSkpoK4gpaClA==")]
[assembly: go.GoPositionMap("html/template/context.go", "context.cs", "ACJGgoKClKiSAAIWtIKUgoKUgpSClIKUgpQAI7ABopSkqJKUpKzolKQ=")]
[assembly: go.GoPositionMap("html/template/css.go", "css.cs", "AA8iooKUlIKClAAHEKjIAAIe4oKCyoKCgoKUgoK4poKClIKClLimgqaokqiSgoKClLS0tMaokoKmlKqylKSokpSkqLKCgqKUgoKUtLSClIKCgoKmgpSCxgAVMJKe8oKClAALGoKUqKLGgtiCgpQ=")]
[assembly: go.GoPositionMap("html/template/delim_string.go", "delim_string.cs", "/oaigoKCypSCgpQ=")]
[assembly: go.GoPositionMap("html/template/element_string.go", "element_string.cs", "/oaigoKCgsqUgoKU")]
[assembly: go.GoPositionMap("html/template/error.go", "error.cs", "AEfOA4KUgqSkpKrC")]
[assembly: go.GoPositionMap("html/template/escape.go", "escape.cs", "ABUw8oKCgqSUlICCgoKklIKAgoKkqtSCgIK2gpTIADFmkgAWKJKUpIKCpKSCgqSkpKSkpJSmlrKUlJSCggAHEJSCgIKE/oKUpKSCpJSkxqTaxoSktKSkpKTIgrSkgpS2uLSkgtzSlMqCgoKAgoCU3IKCuIKCgoKmpvyCgoKCgoCCtoLKpsoAEQ4AByiSqqKAgqTKAA8wwoCCgoKCtqiSAAUwABQClqam1KyygpSClIKUgpSCloKClIKWgoKUggAGEICCgILI3rKClIKCgpSCgsyCgoKCuIKClIKCgqaCpriCgoKCgqaCgoKCgqaosoKUgoKCpq7CkpSCgoKUgoKCgpSClIKmqLKCgpSqxpKCgJSkgqaCysqmgoKCgpSUqtSClICCyILKrNKSlJSmptyCAAoIAAUqgpaCpoKmlrKygoKCgoKCgoKCuIKCgoLIAAES8pS2pJSUgpS2lIKUgoKClIKUloKClJSqooKCpqaqgoKUAAcQgILsyoKClJaWgpaUuKiSgIKkqJKAgqSokoCCpKqigriCgoCCtoKUgpSCuIKCgqi2goKUqqKClK7CqJKqwqiSqJKqwqrC", "687-699:1")]
[assembly: go.GoPositionMap("html/template/html.go", "html.cs", "AA8esoKClIKUqLKCgpSosoKClKiygoKUAAoKAAskABw0ABYuABAsopKSuIKCgIKClIKCAAgMgpSCpoKUgqqigtaCgpSClIKClIKCgoKCuJSUgpSCgpSUlJSCpJSqwoKClNyUgoCUpIL4tgACEgAIAg==")]
[assembly: go.GoPositionMap("html/template/js.go", "js.cs", "ABdEAA0EgoKogLiEspSmlKaSlKioqKgAAhqoggAVApSCAAgMygAQIpjqgpaCgpSm+MKCgoKUptjWgpS4ggAUKoKCgoIABhCmlIKCpoKClKaCgoKCpJSCgoKUlIKCgpSUrNKCgpSmooKu4oKClJQAAhDSgqKUgoKUtLS0tLSClIKClIKUgrYAEBwAFiwAGTIAEyQAHUjClKSkpKSkrAAJDoKCggABKqQ=")]
[assembly: go.GoPositionMap("html/template/jsctx_string.go", "jsctx_string.cs", "/oaigoLKlIKClA==")]
[assembly: go.GoPositionMap("html/template/state_string.go", "state_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgsqUgoKU")]
[assembly: go.GoPositionMap("html/template/template.go", "template.cs", "ACdEAAkY4oKCpIKClAAFKgAVAoKq4oKUgoKClNjSgoKCgoKUgILGlAAFFAAJAoCCpAACFPKCgpSsAAkCgoKCgoKUgpSClIKUgpTcsgACHAAPAoCCpoKCzIKCgoKCgpSClN4ACAKAgqaCgoKClNyCAAUWAAwCgoKClIKClIKC3IKCgoKClIIABxDYkoKC3IIAAhQACwKCgtiS3ICCgqSCqJIAAhTyggACEPKCquKCggAFENKClAACGAALAgACFgAKAqrSgIKmlJSCgoKUjuKClIKUlIKCpgACGAAJAgACGgAKAqiSgIKkgoKUgpSssq7iruKmgoKCgoKUgpSUpsKCgqaCsoKC", "525-529:1")]
[assembly: go.GoPositionMap("html/template/transition.go", "transition.cs", "ABAgAB4+kpaSgoKCgqSUgoKCgpSUgoKCppS4AAcSpIKClILKgoKUgoLMgoKUlKSkpLiClJSokoKCpJSopIKCtoKUlKYACBSSgoKmgpS0tIKokoCCpMoABhCCmqKmgpSAgraokoKClIKClJSClIKUlJSokqiSgsiUqJKClIKUgpS0tLSUtLS0tAAIGvLGgsiSyqKUtIKcgsKUgrS0poKCgoKClJSCgvyCgoLIgrSWqqKClKSmkoKCgpSUgoL8tLqypILIkoLGlqbMppaSgoKUlKSkpKiSgoKUpAACEgAJBoKClNyoABs4goKCgpSWkoKClLS0tMaClIKkguiCpIKkupKClKSopMaCgoKCgpSCgoLcgpSCupKuwoKUqgAKCqYABhCSqJKokoKUgoKCgoKmgoKUlKiSgsjG")]
[assembly: go.GoPositionMap("html/template/url.go", "url.cs", "AAxEABgCgoKUgpT6ooCCgraqwgACEPKqwoKClIKClKqigu6CggABEOIABBC24siSlIKUgsaCgpSCqsKClKiigpa2goKCgoKCpoIABRCipoKmgoKClIKCgoKmgKaCgoKCpoKCgoK2gg==")]
[assembly: go.GoPositionMap("html/template/urlpart_string.go", "urlpart_string.cs", "/oaigoKCypSCgpQ=")]
// </GoSourcePositionMaps>

namespace go.html;

[GoPackage("template")]
public static partial class template_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct attr {}
    internal partial struct contentType {}
    internal partial struct context {}
    internal partial struct delim {}
    internal partial struct element {}
    internal partial struct escaper {}
    internal partial struct jsCtx {}
    internal partial struct nameSpace {}
    internal partial struct rangeContext {}
    internal partial struct state {}
    internal partial struct urlPart {}
    public partial struct CSS {}
    public partial struct ErrorCode {}
    public partial struct HTML {}
    public partial struct HTMLAttr {}
    public partial struct JS {}
    public partial struct JSStr {}
    public partial struct Srcset {}
    public partial struct Template {}
    public partial struct URL {}
    public partial struct ΔError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhtml() => builtin.initPackage(typeof(html_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(text.template.parse_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
