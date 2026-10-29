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
[assembly: go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: go.GoFieldAccess(typeof(Template))]
[assembly: go.GoFieldAccess(typeof(escaper))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.ActionNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.BreakNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.CommandNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.ContinueNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.IdentifierNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.PipeNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.TemplateNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.TextNode))]
[assembly: go.GoFieldAccess(typeof(go.text.template.parse_package.Tree))]
[assembly: go.GoFieldAccess(typeof(go.text.template_package.Template))]
[assembly: go.GoFieldAccess(typeof(nameSpace))]
[assembly: go.GoFieldAccess(typeof(rangeContext))]
[assembly: go.GoFieldAccess(typeof(ΔError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("html/template/attr.go", "attr.cs", "ABEmAHzyAaK4goCCgqakgIK2ggAKFoaU", "", "", "178=Contains/2/3/1,Contains/3/3/2")]
[assembly: go.GoPositionMap("html/template/attr_string.go", "attr_string.cs", "/oaigoKCgoKCypSCgpQ=")]
[assembly: go.GoPositionMap("html/template/content.go", "content.cs", "AGzmAaKClICUpIKClKiCnLKClIKClKrCgpSkpKSkpKSkpoK4gqaClA==")]
[assembly: go.GoPositionMap("html/template/context.go", "context.cs", "ACNIgoKClKiSAAIYtIKUgoKUgpSClIKUgpSClKiSgoIAV7gBopSUqJKUlKzolJQ=", "", "", "46=Equal/1/1/4")]
[assembly: go.GoPositionMap("html/template/css.go", "css.cs", "AA8iooKUlIKClAAHEKjIAAQewoKCyoKCgoKUgoK4poKClIKClLimgqaokqiSgoKClLS0tLaokoKmlNqClJSokpSUqLKCgqKUgoKUtKSClIKCgoKmgpSCpgAXMJKeAAgCgoKUAAwagpTIgsaCyIKClA==")]
[assembly: go.GoPositionMap("html/template/delim_string.go", "delim_string.cs", "/oaigoKCypSCgpQ=")]
[assembly: go.GoPositionMap("html/template/element_string.go", "element_string.cs", "/oaigoKCgoLKlIKClA==")]
[assembly: go.GoPositionMap("html/template/error.go", "error.cs", "AOgB1AOClIKkpJSqwg==")]
[assembly: go.GoPositionMap("html/template/escape.go", "escape.cs", "ABUw8oKCgoKClJSAgoKCpJSCgIKCpKrUgoCCtoKUuAAxZpIAFiiSlKSCgqSkgoKkpKSkpKSUppSWspSUlIKCAAcQlIKAgoT+gpSkpIKklLTGpNoACAqC3MaUtLS0tLS0+IK0tIKUtti0pILc0pTKgoKCgIKAlNyCgriCgoKCpqb8goKCgoKAgraCyqbK7gASKJKqooCCpMoADzDCgIKCgoK2qJIABTAAEgKmxsakrLKClIKUgpSClIKmgoKUgqaCgpSCAAcQgIKAgtjesoKUgoKClIKC3IKCgoK4goKUgoKCpoKmuIKCgoKCpoKCgoKCpqiygpSCgoKmrsKClIKCgpSCgoKClIKUgqaosoKClKrGgoKAlKSCpoLKyqaCgoKClJSq1IKUgILIgsqs0pKUlKam3IK4ABMqgpaCpoKmlrKygoKCgoKCgoKCuIKCgoKmggAIEoKUxqSUlIKUgpSUgpSCgoKUgpSmgoKUlKqigoKmpsqCgpQABxCAguzKgoKUpqaCppS4qJKAgqSokoCCpKiSgIKkqqKCuIKCgIK2gpSClIK4goKCqLaCgpSqooKUrsKokqrCqJKokqrCqsI=", "701-713:1", "", "223=errorf/1/1/2;258=errorf/1/1/2;510=NewIdentifier/1/1/2,SetTree/1/1/2,SetPos/1/1/2;605=errorf/1/1/2;750=errorf/1/1/2;755=errorf/1/1/2;791=Name/1/1/2,errorf/1/1/2;975=errorf/1/1/2")]
[assembly: go.GoPositionMap("html/template/html.go", "html.cs", "AA8esoKClIKUqLKCgpSosoKClKiygoKUygARJAAZNAAWLgATLKKSkriCgoCCgpSCgqSmgoKUgqaClIKqooLWgoKUgpSCgpSCgoKCgriUlIKUgoKUlJSUgoKClKrCgoKU3JSCgJSkgvimAAISAAgC")]
[assembly: go.GoPositionMap("html/template/js.go", "js.cs", "ABdEAA0EgoK4gNiUgpSmlLaClMjIyMgADRrIgoK4hPzcABEkmOqCpoKClKb4woKCgpS26LaClLiCABQqgoKCggAHEKaUgoKmgoKUpoKCgoKCgpSCgoKUlIKCgpSUrNKCgpSmooKu4oKClJQAAhDSgqKUgoKUtLS0tKSClIKClIKUgqYADRwAFSwAGDIAEiQAH0jClKSkpKSUrAAJDoKCggACLKQ=", "", "", "113=TrimRight/1/1/1,HasSuffix/1/1/1")]
[assembly: go.GoPositionMap("html/template/jsctx_string.go", "jsctx_string.cs", "/oaigoLKlIKClA==")]
[assembly: go.GoPositionMap("html/template/state_string.go", "state_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCypSCgpQ=")]
[assembly: go.GoPositionMap("html/template/template.go", "template.cs", "ACdEAAkY4oKCpIKClAAFKgAVAoKq4oKUgoKClNjSgoKCgoKUgIKkgpQABRQACQKAgqQAAhTygoKUrAAJAoKCgoKClIKUgpSClIKU3LIAAhwADwKAgraCgtyCgoKCgoKUgpTeAAgCgIK2goKCgpTcggAFFgAMAoKCgpSCgpSCgtyCgoKCgpSCAAcQ2JKCgtyCAAIUAAsCgoLYktyAgoKkgqiSAAIU8oIAAhDygqrigoIABRDSgpQAAhgACwIAAhYACgKq0oCCtpSUgoKClI7igpSClJSCgqYAAhgACQIAAhoACgKokoCCpIKClIKUrLKu4q7ipoKCgoKClIKUlKbCgoKmgrKCgg==", "525-529:1", "", "358=New/1/1/2;390=New/1/1/2")]
[assembly: go.GoPositionMap("html/template/transition.go", "transition.cs", "AA8gACBCkpaSgoKCgoKClIKCgoKUlIKCgqaUuAAIFKSCgpSmgpTKgoKUgoLcgoKCgpSUtLS0yIKUlKiSgoKCgpSopIKCgpSClJSmAAkWkoKCpoKUtKSCqJKAgqTKAAcSgpqipoKUgIK2qJKCgpSCgpSUgpSClJSUqJKokoKCppSokoKUgpSClLS0tJS0tLS0AA4agsaC2IKCuIKU2IKClIK0goLcgoKClIK0pKaCgoKCgpSUgoL8goKC2IKUpqqigpS0tpKCgoKUlIKC/LTqgoKCgtiCgramptymlpKCgpSUtLSkqJKCgpS0AAoStoKClNyoABw4goKCgpSmgoKClLS0pMaClIKkgtiCpIKUupKClLTYtLaCgoKCgpSCgoLcgpSCupKokoKCgIKCyKiSgoKCpq7CgpTa6qYABxKSqJKokoKUgoKCgoKmgoKUlKiSgtim", "", "", "116=errorf/1/1/2;337=errorf/1/1/2;424=errorf/1/1/2;475=errorf/1/1/2;516=errorf/1/1/2;704=errorf/1/1/2")]
[assembly: go.GoPositionMap("html/template/url.go", "url.cs", "AAxEABgCgoKUgpT6ooCCgraqwgACEPKqwoKClIKClKqigu6CggAHEIIACRDGgtiClIKUgraCgpSCqsKClKiigqamgoKCgoKCpoIABRCipoKmgoKClIKCgoKmgKaCgoKCpoKCgoK2gg==")]
[assembly: go.GoPositionMap("html/template/urlpart_string.go", "urlpart_string.cs", "/oaigoKCypSCgpQ=")]
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
