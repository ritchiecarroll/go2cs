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
global using reflectꓸType = go.reflect_package.ΔType;
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
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Template))]
[assembly: GoFieldAccess(typeof(escaper))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.ActionNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.BreakNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.CommandNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.ContinueNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.IdentifierNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.IfNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.PipeNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.RangeNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.TemplateNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.TextNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.Tree))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.WithNode))]
[assembly: GoFieldAccess(typeof(go.text.template_package.Template))]
[assembly: GoFieldAccess(typeof(nameSpace))]
[assembly: GoFieldAccess(typeof(rangeContext))]
[assembly: GoFieldAccess(typeof(ΔError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("html/template/attr.go", "attr.cs", "ABEmAIAB8gGiuIKCgqaUgqaCAAoWhpQ=", "", "", "178=Contains/2/3/1,Contains/3/3/2")]
[assembly: GoPositionMap("html/template/attr_string.go", "attr_string.cs", "/oaigoKCgoKC2pSCgpQ=")]
[assembly: GoPositionMap("html/template/content.go", "content.cs", "AGzmAaKClJSUgoKUqIKcsoKUgoKUqsKClKSkpKSkpKSmgriCpoKU")]
[assembly: GoPositionMap("html/template/context.go", "context.cs", "ACNIgoKClKiSAAIYtIKUgoKUgpSClIKUgpSClKiSgoIAV7gBopSUqJKUlKzolJQ=", "", "", "46=Equal/1/1/4")]
[assembly: GoPositionMap("html/template/css.go", "css.cs", "AA8iooKUlIKClAAHEKjIAAQewoKCyoKCgoKUgoK4poKClIKClLimgqaokqiSgoKClLS0tLaokoKmlNqClJSokpSUqLKCgqKUgoKUtKSClIKCgoKmgpSCpgAXMJKeAAgCgoKUAAwagpTIgsaCyIKClA==")]
[assembly: GoPositionMap("html/template/delim_string.go", "delim_string.cs", "/oaigoKC2pSCgpQ=")]
[assembly: GoPositionMap("html/template/element_string.go", "element_string.cs", "/oaigoKCgoLalIKClA==")]
[assembly: GoPositionMap("html/template/error.go", "error.cs", "AOgB1AOClIKkpJSqwg==")]
[assembly: GoPositionMap("html/template/escape.go", "escape.cs", "ABUw8oKCgoKClJSCgoKUlIKAgoKkqtSCgqaClLgAMWaSABkokpSkgoKkpIKCpKSkpKSklKaUlrKUlJSCggAHEJSCgoTugpSkpIKklLTGpNoACAqC3MaUtLS0tLS0+IK0tIKUtti0pILs0pTKgoKCgpTcgoK4goKCgqam3IKCgoKCgqaCyqbK7gASKJKqooKUygAPMMKCgoKCpqiSAAUwABICpsbGpKyygpSClIKUgpSCpoKClIKmgoKUggAHEIKCuN6ygpSCgoKUgoLcgoKCgriCgpSCgoKmgqa4goKCgoKmgoKCgoKmqLKClIKCgqauwoKUgoKClIKCgoKUgpSCpqiygoKUqsaCgpSUgqaCysqmgoKCgpSUqtSClIK4gsqs0pKUlKam3IK4ABMqgpaCpoKmlrKygoKCgoKCgoKCuIKCgoKmggAIEoKUxqSUlIKUgpSUgpSCgoKUgpSmgoKUlKqigoKmpsqCgpQABxCC3MqCgpSmpoKmlLiokoKUqJKClKiSgpSqooK4goKCpoKUgpSCuIKCgqi2goKUqqKClK7CqJKqwqiSqJKqwqrC", "701-713:1", "", "221=errorf/1/1/2;255=errorf/1/1/2;498=NewIdentifier/1/1/2,SetTree/1/1/2,SetPos/1/1/2;589=errorf/1/1/2;732=errorf/1/1/2;737=errorf/1/1/2;771=Name/1/1/2,errorf/1/1/2;954=errorf/1/1/2")]
[assembly: GoPositionMap("html/template/html.go", "html.cs", "AA8esoKClIKUqLKCgpSosoKClKiygoKUygARJAAZNAAWLgATLKKSkriCgoKClIKClKaCgpSCpoKUgqqigtaCgpSClIKClIKCgoKCuJSUgpSCgpSUlJSCgoKUqsKCgpTclIKUlIL4pgACEgAIAg==")]
[assembly: GoPositionMap("html/template/js.go", "js.cs", "ABhEAA0EgoK42JSClKaUtoKUyMjIyAANGsiCgriE7NwAESSY6oKmgoKUpgALCMKCgoKUtui2gpS4ggAUKoKCgoIABxCmlIKCpoKClKaCgoKCgoKUgoKClJSCgoKUlKzSgoKUpqKCruKCgpSUAAIQ0oKilIKClLS0tLSkgpSCgpSClIKmAA0cABUsABgyABIkAB9IwpSkpKSklKwACQ6CgoIAAiyk", "", "", "113=TrimRight/1/1/1,HasSuffix/1/1/1")]
[assembly: GoPositionMap("html/template/jsctx_string.go", "jsctx_string.cs", "/oaigoLalIKClA==")]
[assembly: GoPositionMap("html/template/state_string.go", "state_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC2pSCgpQ=")]
[assembly: GoPositionMap("html/template/template.go", "template.cs", "ACdEAAkY4oKCpIKClAAFKgAVAoKq4oKUgoKClNjSgoKCgoKUgpSClAAFFAAJAoKUAAIU8oKClKwACQKCgoKCgpSClIKUgpSClNyyAAIcAA8CgqaCgtyCgoKCgoKUgpTeAAgCgqaCgoKClNyCAAUWAAwCgoKClIKClIKC3IKCgoKClIIABxDYkoKC3IIAAhQACwKCgtiS3IKClIKokgACFPKCAAIQ8oKq4oKCAAUQ0oKUAAIYAAsCAAIWAAoCqtKAgraUlIKCgpSO4oKUgpSUgoKmAAIYAAkCAAIaAAoCqJKClIKClIKUrLKu4q7ipoKCgoKClIKUlKbCgoKmgrKCgg==", "525-529:1", "", "350=New/1/1/2;382=New/1/1/2")]
[assembly: GoPositionMap("html/template/transition.go", "transition.cs", "AA8gACBCkpaSgoKCgoKClIKCgoKUlIKCgqaUuAAIFKSCgpSmgpTKgoKUgoLcgoKCgpSUtLS0yIKUlKiSgoKCgpSopIKCgpSClJSmAAkWkoKCpoKUtKSCqJKClMoABxKCmqKmgpSCpqiSgoKUgoKUlIKUgpSUlKiSqJKCgqaUqJKClIKUgpS0tLSUtLS0tAAOGoLGgtiCgriClNiCgpSCtIKC3IKCgpSCtKSmgoKCgoKUlIKC/IKCgtiClKaqooKUtLaSgoKClJSCgvy06oKCgoLYgoK2pqbcppaSgoKUlLS0pKiSgoKUtAAKEraCgpTcqAAcOIKCgoKUpoKCgpS0tKTGgpSCpILYgqSClLqSgpS02LS2goKCgoKUgoKC3IKUgrqSqJKCgoKCuKiSgoKCpq7CgpTa6qYABxKSqJKokoKUgoKCgoKmgoKUlKiSgtim", "", "", "116=errorf/1/1/2;333=errorf/1/1/2;420=errorf/1/1/2;471=errorf/1/1/2;512=errorf/1/1/2;700=errorf/1/1/2")]
[assembly: GoPositionMap("html/template/url.go", "url.cs", "AAxEABgCgoKUgpQACgqigoKmqsIAAhDyqsKCgpSCgpSqooLugoIABxCCAAkQxoLYgpSClIK2goKUgqrCgpSoooKmpoKCgoKCgqaCAAYQoqaCpoKCgpSCgoKCpqaCgoKCpoKCgoKmgg==")]
[assembly: GoPositionMap("html/template/urlpart_string.go", "urlpart_string.cs", "/oaigoKC2pSCgpQ=")]
// </GoSourcePositionMaps>

namespace go.html;

[GoPackage("template")]
public static partial class template_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(text.template.parse_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
