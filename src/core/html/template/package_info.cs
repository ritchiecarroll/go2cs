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
using fs = go.io.fs_package;
using io = go.io_package;
using parse = go.text.template.parse_package;
using template = go.text.template_package;
// </ImportedTypeAliases>

using go;
using static go.html.template_package;
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
[assembly: GoTypeAlias("FuncMap", "go.text.template_package.FuncMap")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Error, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Error))]
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
[assembly: GoPositionMap("html/template/css.go", "css.cs", "AA8iooKUlIKClAAHEKjIAAQewoKCyoKCgoKUgoK4poKClIKClLimgqaokqiSgoKClLS0tLaokoKmlNqClJSokpSUqLKCgqKUgoKUtKSClIKCgoKmgpSCpgAXMIKeAAgCgoKUAAwagpTIgsaCyIKClA==")]
[assembly: GoPositionMap("html/template/delim_string.go", "delim_string.cs", "/oaigoKC2pSCgpQ=")]
[assembly: GoPositionMap("html/template/element_string.go", "element_string.cs", "/oaigoKCgoLalIKClA==")]
[assembly: GoPositionMap("html/template/error.go", "error.cs", "AOgB1AOClIKkpJSqwg==")]
[assembly: GoPositionMap("html/template/escape.go", "escape.cs", "ABUw8oKCgoKClJSCgoKUlIKAgoKkqtSCgqaClLgAMWaSABkokpSkgoKkpIKCpKSkpKSklKaUlrKUlJSCggAHEJSCgoTugpSkpIKklLTGpNoACAqC3MaUtLS0tLS0+IK0tIKUtti0pILs0pTKgoKCgpTcgoK4goKCgqam3IKCgoKCgqaCyqbK7gASKJKqooKUygAPMMKCgoKCpqiSAAUwABICpsbGpKyygpSClIKUgpSCpoKClIKmgoKUggAHEIKCuN6ygpSCgoKUgoLcgoKCgriCgpSCgoKmgqa4goKCgoKmgoKCgoKmqLKClIKCgqauwoKUgoKClIKCgoKUgpSCpqiygoKUqsaCgpSUgqaCysqmgoKCgpSUqtSClIK4gsqs0pKUlKam3IK4ABMqgpaCpoKmlrKygoKCgoKCgoKCuIKCgoKmggAIEoKUxqSUlIKUgpSUgpSCgoKUgpSmgoKUlKqigoKmpsqCgpQABxCC3MqCgpSmpoKmlLiokoKUqJKClKiSgpSqooK4goKCpoKUgpSCuIKCgqi2goKUqqKClK7CqJKqwqiSqJKqwqrC", "701-713:1", "", "221=errorf/1/1/2;255=errorf/1/1/2;498=NewIdentifier/1/1/2,SetTree/1/1/2,SetPos/1/1/2;589=errorf/1/1/2;732=errorf/1/1/2;737=errorf/1/1/2;771=Name/1/1/2,errorf/1/1/2;954=errorf/1/1/2")]
[assembly: GoPositionMap("html/template/html.go", "html.cs", "AA8esoKClIKUqLKCgpSosoKClKiygoKUygARJAAZNAAWLgATLKKSkriCgoKClIKClKaCgpSCpoKUgqqigtaCgpSClIKClIKCgoKCuJSUgpSCgpSUlJSCgoKUqsKCgpTclIKUlIL4pgACEgAIAg==")]
[assembly: GoPositionMap("html/template/js.go", "js.cs", "ABhEAA0EgoK42JSClKaUtoKUyMjIyAANGsiCgriE7NwAESSY6oKmgoKUpgALCMKCgoKUtui2gpS4ggAUKoKCgoIABxCmlIKCpoKClKaCgoKCgoKUgoKClJSCgoKUlKzSgoKUpqKCruKCgpSUAAIQ0oKilIKClLS0tLSkgpSCgpSClIKmAA0cABUsABgyABEkAB9IwpSkpKSklKwACQ6CgoIAASyk", "", "", "113=TrimRight/1/1/1,HasSuffix/1/1/1")]
[assembly: GoPositionMap("html/template/jsctx_string.go", "jsctx_string.cs", "/oaigoLalIKClA==")]
[assembly: GoPositionMap("html/template/state_string.go", "state_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC2pSCgpQ=")]
[assembly: GoPositionMap("html/template/template.go", "template.cs", "ACdEAAkY4oKCpIKClAAFKgAVAoKq4oKUgoKClNjSgoKCgoKUgpSClAAFFAAJAoKUAAIU8oKClKwACQKCgoKCgpSClIKUgpSClNyyAAIcAA8CgqaCgtyCgoKCgoKUgpTeAAgCgqaCgoKClNyCAAUWAAwCgoKClIKClIKC3IKCgoKClIIABxDYkoKC3IIAAhQACwKCgtiS3IKClIKokgACFPKCAAIQ8oKq4oKCAAUQ0oKUAAIYAAsCAAIWAAoCqtKAgraUlIKCgpSO4oKUgpSUgoKmAAIYAAkCAAIaAAoCqJKClIKClIKUrLKu4q7ipoKCgoKClIKUlKbCgoKmgrKCgg==", "525-529:1", "", "350=New/1/1/2;382=New/1/1/2")]
[assembly: GoPositionMap("html/template/transition.go", "transition.cs", "AA8gACBCgpaSgoKCgoKClIKCgoKUlIKCgqaUuAAIFKSCgpSmgpTKgoKUgoLcgoKCgpSUtLS0yIKUlKiSgoKCgpSopIKCgpSClJSmAAkWkoKCpoKUtKSCqJKClMoABxKCmqKmgpSCpqiSgoKUgoKUlIKUgpSUlKiSqJKCgqaUqJKClIKUgpS0tLSUtLS0tAAOGoLGgtiCgriClNiCgpSCtIKC3IKCgpSCtKSmgoKCgoKUlIKC/IKCgtiClKaqooKUtLaSgoKClJSCgvy06oKCgoLYgoK2pqbcppaSgoKUlLS0pKiSgoKUtAAKEraCgpTcqAAcOIKCgoKUpoKCgpS0tKTGgpSCpILYgqSClLqSgpS02LS2goKCgoKUgoKC3IKUgrqSqJKCgoKCuKiSgoKCpq7CgpTa6qYABxKSqJKokoKUgoKCgoKmgoKUlKiSgtim", "", "", "115=errorf/1/1/2;332=errorf/1/1/2;419=errorf/1/1/2;470=errorf/1/1/2;511=errorf/1/1/2;699=errorf/1/1/2")]
[assembly: GoPositionMap("html/template/url.go", "url.cs", "AAxEABgCgoKUgpQACgqigoKmqsIAAhDyqsKCgpSCgpSqooLugoIABxCCAAkQxoLYgpSClIK2goKUgqrCgpSoooKmpoKCgoKCgqaCAAYQoqaCpoKCgpSCgoKCpqaCgoKCpoKCgoKmgg==")]
[assembly: GoPositionMap("html/template/urlpart_string.go", "urlpart_string.cs", "/oaigoKC2pSCgpQ=")]
// </GoSourcePositionMaps>

namespace go.html;

using static go.html.template_package;
using fs = go.io.fs_package;
using io = go.io_package;
using parse = go.text.template.parse_package;
using template = go.text.template_package;
using ꓸꓸꓸstring = variadic<@string>;

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
    public partial struct Error {}
    public partial struct ErrorCode {}
    public partial struct HTML {}
    public partial struct HTMLAttr {}
    public partial struct JS {}
    public partial struct JSStr {}
    public partial struct Srcset {}
    public partial struct Template {}
    public partial struct URL {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(template_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛError;
[GoMethodHost(typeof(template_package.Template)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛTemplate;
[GoMethodHost(typeof(template_package.attr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛattr;
[GoMethodHost(typeof(template_package.context)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛcontext;
[GoMethodHost(typeof(template_package.delim)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛdelim;
[GoMethodHost(typeof(template_package.element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛelement;
[GoMethodHost(typeof(template_package.escaper)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛescaper;
[GoMethodHost(typeof(template_package.jsCtx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛjsCtx;
[GoMethodHost(typeof(template_package.state)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛstate;
[GoMethodHost(typeof(template_package.urlPart)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class templateᴛurlPart;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class templateᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref Error e) => template_package.Error⁔(ref e);
}
partial class templateᴛTemplate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Template>> Templates(this ж<Template> Ꮡt) => template_package.Templates⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Template> Option(this ж<Template> Ꮡt, params ꓸꓸꓸstring optʗp) => template_package.Option⁔(Ꮡt, optʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkCanParse(this ж<Template> Ꮡt) => template_package.checkCanParse⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error escape(this ж<Template> Ꮡt) => template_package.escape⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Execute(this ж<Template> Ꮡt, io.Writer wr, any data) => template_package.Execute⁔(Ꮡt, wr, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ExecuteTemplate(this ж<Template> Ꮡt, io.Writer wr, @string name, any data) => template_package.ExecuteTemplate⁔(Ꮡt, wr, name, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Template> tmpl, error err) lookupAndEscapeTemplate(this ж<Template> Ꮡt, @string name) => template_package.lookupAndEscapeTemplate⁔(Ꮡt, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string DefinedTemplates(this ref Template t) => template_package.DefinedTemplates⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) Parse(this ж<Template> Ꮡt, @string text) => template_package.Parse⁔(Ꮡt, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) AddParseTree(this ж<Template> Ꮡt, @string name, ж<parse.Tree> Ꮡtree) => template_package.AddParseTree⁔(Ꮡt, name, Ꮡtree);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) Clone(this ж<Template> Ꮡt) => template_package.Clone⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Template> New(this ж<Template> Ꮡt, @string name) => template_package.New⁔(Ꮡt, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Template> @new(this ref Template t, @string name) => template_package.new⁔(ref t, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref Template t) => template_package.Name⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Template> Funcs(this ж<Template> Ꮡt, FuncMap funcMap) => template_package.Funcs⁔(Ꮡt, funcMap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Template> Delims(this ж<Template> Ꮡt, @string left, @string right) => template_package.Delims⁔(Ꮡt, left, right);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Template> Lookup(this ж<Template> Ꮡt, @string name) => template_package.Lookup⁔(Ꮡt, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) ParseFiles(this ж<Template> Ꮡt, params ꓸꓸꓸstring filenamesʗp) => template_package.ParseFiles⁔(Ꮡt, filenamesʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) ParseGlob(this ж<Template> Ꮡt, @string pattern) => template_package.ParseGlob⁔(Ꮡt, pattern);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Template>, error) ParseFS(this ж<Template> Ꮡt, fs.FS fsΔ1, params ꓸꓸꓸstring patternsʗp) => template_package.ParseFS⁔(Ꮡt, fsΔ1, patternsʗp);
}
partial class templateᴛattr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this attr i) => template_package.String⁔(i);
}
partial class templateᴛcontext {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this context c) => template_package.String⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool eq(this context c, context d) => template_package.eq⁔(c, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string mangle(this context c, @string templateName) => template_package.mangle⁔(c, templateName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context clone(this context c) => template_package.clone⁔(c);
}
partial class templateᴛdelim {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this delim i) => template_package.String⁔(i);
}
partial class templateᴛelement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this element i) => template_package.String⁔(i);
}
partial class templateᴛescaper {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escape(this ref escaper e, context c, parse.Node n) => template_package.escape⁔(ref e, c, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escapeAction(this ref escaper e, context c, ж<parse.ActionNode> Ꮡn) => template_package.escapeAction⁔(ref e, c, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escapeBranch(this ref escaper e, context c, ж<parse.BranchNode> Ꮡn, @string nodeName) => template_package.escapeBranch⁔(ref e, c, Ꮡn, nodeName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escapeList(this ref escaper e, context c, ж<parse.ListNode> Ꮡn) => template_package.escapeList⁔(ref e, c, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (context, bool) escapeListConditionally(this ref escaper e, context c, ж<parse.ListNode> Ꮡn, Func<ж<escaper>, context, bool> filter) => template_package.escapeListConditionally⁔(ref e, c, Ꮡn, filter);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escapeTemplate(this ref escaper e, context c, ж<parse.TemplateNode> Ꮡn) => template_package.escapeTemplate⁔(ref e, c, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (context, @string) escapeTree(this ref escaper e, context c, parse.Node node, @string name, nint line) => template_package.escapeTree⁔(ref e, c, node, name, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context computeOutCtx(this ref escaper e, context c, ж<template.Template> Ꮡt) => template_package.computeOutCtx⁔(ref e, c, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (context, bool) escapeTemplateBody(this ref escaper e, context c, ж<template.Template> Ꮡt) => template_package.escapeTemplateBody⁔(ref e, c, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context escapeText(this ref escaper e, context c, ж<parse.TextNode> Ꮡn) => template_package.escapeText⁔(ref e, c, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void editActionNode(this ref escaper e, ж<parse.ActionNode> Ꮡn, slice<@string> cmds) => template_package.editActionNode⁔(ref e, Ꮡn, cmds);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void editTemplateNode(this ref escaper e, ж<parse.TemplateNode> Ꮡn, @string callee) => template_package.editTemplateNode⁔(ref e, Ꮡn, callee);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void editTextNode(this ref escaper e, ж<parse.TextNode> Ꮡn, slice<byte> text) => template_package.editTextNode⁔(ref e, Ꮡn, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void commit(this ref escaper e) => template_package.commit⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<template.Template> template(this ref escaper e, @string name) => template_package.template⁔(ref e, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Template> arbitraryTemplate(this ref escaper e) => template_package.arbitraryTemplate⁔(ref e);
}
partial class templateᴛjsCtx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this jsCtx i) => template_package.String⁔(i);
}
partial class templateᴛstate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this state i) => template_package.String⁔(i);
}
partial class templateᴛurlPart {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this urlPart i) => template_package.String⁔(i);
}
// </MethodForwarders>
