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
using static go.text.template_package;

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
[assembly: GoImplement<ExecError, error>]
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
[assembly: GoFieldAccess(typeof(Template))]
[assembly: GoFieldAccess(typeof(common))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.ActionNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.BoolNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.IfNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.ListNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.NumberNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.PipeNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.StringNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.TextNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.VariableNode))]
[assembly: GoFieldAccess(typeof(go.text.template.parse_package.WithNode))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("text/template/exec.go", "exec.cs", "ABkslIKClAAVKpKokqiSqqKCgoKmqJKokoKCpoIACQ6UlILIksqiABAegqaCyLKCgpSClAARHILu4oKClLS0xNYABBIACQKCgpQAAhoACgKm4oKCgpTKgpSCAAkOAAkCgpSCgoKCgpSClJSUAAgKgrrCgriCgsbmtLSCxrS0gsbEzOKCgoKClIKClJSCtMqyprKUlJS0tLS0tLS0tJTG0oKCgqaClIKigqaClMqmgoK4poKUgqaikraCgpSCgpSUgpTEgpSClMSClIKClMSClIKClIKCgoKUlIKUxMSCgoKUgoKmlIKUlIKCgoKCyqaClJS0lIK0pKKCgoKUgqaCgoKUggACFgALAoKUgoKClIKmgoKUpqaCgriigpSktraCpJSCgpSkpLSklILuAAkIgpSqpoKClKakxoKmgsaigqaigoKUgqaCprSCgoKClKyygoKmpqKCgoKClKyygoKUlIKCpoLKgoKUgpSUlIKCgoKUgqaClNiCgoKUgoLYtLbGgoKmpoK2gsiCgryygpSCgoKUgoKCgpSClIKmgoKUuIKCgoKCpqbulLiUgoKmgoKCuIKCgqa4psqCgpSUlIKmpoKClMiSlKSUyJKClJSUlJSClIKCgoIABxCUgoLGtLamgoKUpIKUtKSkpKSUlKSkpKSCxoLGpJSCpoKCgoKClIKmgoKCgoKUgqaCgoKCgpSCpoKCgoKClIKmgoKCgoKUgqaCgoKClIKmgoKUpKSktrSkpKSUgsyygoKmrsKClIKUyqKCgoKUgoIACAyigpSCpoKClJS4", "354-358:1;363-396:2;389-394:2.1;794-799:1", "", "162=Name/1/1/1,Errorf/1/1/2", "261:21", "480:21,481:13,517:29,519:21")]
[assembly: GoPositionMap("text/template/funcs.go", "funcs.cs", "ACVOwgAaNIKCgoKokoKClIKClIKUvKKCuqSkpKSkupKClILGpKaoAAgCgoKCgqaAgqTqooKClJSClIKClKaClKSUqJKClLS0pJSClAACEPKCgpSigoKClJSCgpS0goKUgIKU6LSmAAISAAgCgoKUgoKUgpSClIKUtLSmgoKCgpSmgpSCpoKUrLKCgpSUlKqyqsKCgpSCgqaAgqSCgoKClJSCpoKilIKCpoKCpqrigoKClLiCgpTqkoKqwqrCqJIABBCCAAsegpSkpKSkpJSokoKUlJSqooKCgqaosoKClIKigoKClJS0tILIlLS0tLS0tIKUgpSClMiCpqikgqiSgoKClIKCgpSClJS0tKaUpLS0tLS2qKSCgpSopIKClKikgoKUvIKCgoKCmJKCgoKUtLS0tLS0pIKClKikgpSCgqrCvIKUgoKCgoKCmJKCgpSUlJSmlLS0tLS0tLSCgoLIgoKUlJSUqKSClIKCpoKUlKrCqsIAAhTygpSClIKCgoKmlA==", "357-365:1", "", "", "", "416:21,417:13")]
[assembly: GoPositionMap("text/template/helper.go", "helper.cs", "ABIw8oKUAAIYAAsCAAQcAA4CgsrSlJSCgoKUjuKClIKUlIKCpgACGAAJAgAEFgAKAoLIkoKClIKUruLO8oLGgoKCgoKUgpSUpsKCgqaCsoKC", "173-177:1")]
[assembly: GoPositionMap("text/template/option.go", "option.cs", "ABlUABUCgoKUpoKCpoKUlIKkgqSCyA==")]
[assembly: GoPositionMap("text/template/template.go", "template.cs", "ACZOkqaCyJIAAhTygtyokoKCgoKCAAMUAAwCgoKClIKCgoKCgqaClIKCgoLYkgAIGgAJAoKCgoKCpoKU2OKCpoKCgoKUAAUQ8oKCggACEgALAoKCgoKC2vKClIKCAAUaAAwCgoKCgoKmgoKmrOKClKaUgg==")]
// </GoSourcePositionMaps>

namespace go.text;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using io = io_package;
using reflect = reflect_package;
using parse = go.text.template.parse_package;
using @internal;
using go.text.template;
using ꓸꓸꓸany = Span<any>;
using static template_package;
using fs = go.io.fs_package;
using go.io;
using go.path;
using ꓸꓸꓸstring = Span<@string>;
// </GlyphForwarderUsings>

[GoPackage("template")]
public static partial class template_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct common {}
    internal partial struct kind {}
    internal partial struct missingKeyAction {}
    internal partial struct missingValType {}
    internal partial struct option {}
    internal partial struct state {}
    internal partial struct variable {}
    internal partial struct writeError {}
    public partial struct ExecError {}
    public partial struct FuncMap {}
    public partial struct Template {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfmtsort() => builtin.initPackage(typeof(@internal.fmtsort_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(go.text.template.parse_package));
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
[GoMethodHost(typeof(template_package.ExecError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class templateᴛExecError;
[GoMethodHost(typeof(template_package.Template)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class templateᴛTemplate;
[GoMethodHost(typeof(template_package.state)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class templateᴛstate;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class templateᴛstate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref state s, @string name, reflect.Value value) => template_package.pushᴍ(ref s, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint mark(this ref state s) => template_package.markᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pop(this ref state s, nint mark) => template_package.popᴍ(ref s, mark);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setVar(this ref state s, @string name, reflect.Value value) => template_package.setVarᴍ(ref s, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setTopVar(this ref state s, nint n, reflect.Value value) => template_package.setTopVarᴍ(ref s, n, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value varValue(this ref state s, @string name) => template_package.varValueᴍ(ref s, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void at(this ref state s, parse.Node node) => template_package.atᴍ(ref s, node);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ref state s, @string format, params ꓸꓸꓸany argsʗp) => template_package.errorfᴍ(ref s, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeError(this ref state s, error err) => template_package.writeErrorᴍ(ref s, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walk(this ж<state> Ꮡs, reflect.Value dot, parse.Node node) => template_package.walkᴍ(Ꮡs, dot, node);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkIfOrWith(this ж<state> Ꮡs, parse.NodeType typ, reflect.Value dot, ж<parse.PipeNode> pipe, ж<parse.ListNode> list, ж<parse.ListNode> elseList) => template_package.walkIfOrWithᴍ(Ꮡs, typ, dot, pipe, list, elseList);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkRange(this ж<state> Ꮡs, reflect.Value dot, ж<parse.RangeNode> Ꮡr) => template_package.walkRangeᴍ(Ꮡs, dot, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkTemplate(this ref state s, reflect.Value dot, ж<parse.TemplateNode> Ꮡt) => template_package.walkTemplateᴍ(ref s, dot, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value /*value*/ evalPipeline(this ref state s, reflect.Value dot, ж<parse.PipeNode> Ꮡpipe) => template_package.evalPipelineᴍ(ref s, dot, Ꮡpipe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void notAFunction(this ref state s, slice<parse.Node> args, reflect.Value final) => template_package.notAFunctionᴍ(ref s, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalCommand(this ref state s, reflect.Value dot, ж<parse.CommandNode> Ꮡcmd, reflect.Value final) => template_package.evalCommandᴍ(ref s, dot, Ꮡcmd, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value idealConstant(this ref state s, ж<parse.NumberNode> Ꮡconstant) => template_package.idealConstantᴍ(ref s, Ꮡconstant);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalFieldNode(this ref state s, reflect.Value dot, ж<parse.FieldNode> Ꮡfield, slice<parse.Node> args, reflect.Value final) => template_package.evalFieldNodeᴍ(ref s, dot, Ꮡfield, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalChainNode(this ref state s, reflect.Value dot, ж<parse.ChainNode> Ꮡchain, slice<parse.Node> args, reflect.Value final) => template_package.evalChainNodeᴍ(ref s, dot, Ꮡchain, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalVariableNode(this ref state s, reflect.Value dot, ж<parse.VariableNode> Ꮡvariable, slice<parse.Node> args, reflect.Value final) => template_package.evalVariableNodeᴍ(ref s, dot, Ꮡvariable, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalFieldChain(this ref state s, reflect.Value dot, reflect.Value receiver, parse.Node node, slice<@string> ident, slice<parse.Node> args, reflect.Value final) => template_package.evalFieldChainᴍ(ref s, dot, receiver, node, ident, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalFunction(this ref state s, reflect.Value dot, ж<parse.IdentifierNode> Ꮡnode, parse.Node cmd, slice<parse.Node> args, reflect.Value final) => template_package.evalFunctionᴍ(ref s, dot, Ꮡnode, cmd, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalField(this ref state s, reflect.Value dot, @string fieldName, parse.Node node, slice<parse.Node> args, reflect.Value final, reflect.Value receiver) => template_package.evalFieldᴍ(ref s, dot, fieldName, node, args, final, receiver);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalCall(this ref state s, reflect.Value dot, reflect.Value fun, bool isBuiltin, parse.Node node, @string name, slice<parse.Node> args, reflect.Value final) => template_package.evalCallᴍ(ref s, dot, fun, isBuiltin, node, name, args, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value validateType(this ref state s, reflect.Value value, reflect.Type typ) => template_package.validateTypeᴍ(ref s, value, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalArg(this ref state s, reflect.Value dot, reflect.Type typ, parse.Node n) => template_package.evalArgᴍ(ref s, dot, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalBool(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalBoolᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalString(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalStringᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalInteger(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalIntegerᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalUnsignedInteger(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalUnsignedIntegerᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalFloat(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalFloatᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalComplex(this ref state s, reflect.Type typ, parse.Node n) => template_package.evalComplexᴍ(ref s, typ, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static reflect.Value evalEmptyInterface(this ref state s, reflect.Value dot, parse.Node n) => template_package.evalEmptyInterfaceᴍ(ref s, dot, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void printValue(this ref state s, parse.Node n, reflect.Value v) => template_package.printValueᴍ(ref s, n, v);
}
partial class templateᴛExecError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ExecError e) => template_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ExecError e) => template_package.Unwrapᴍ(e);
}
partial class templateᴛTemplate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error ExecuteTemplate(this ж<Template> Ꮡt, io.Writer wr, @string name, any data) => template_package.ExecuteTemplateᴍ(Ꮡt, wr, name, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Execute(this ж<Template> Ꮡt, io.Writer wr, any data) => template_package.Executeᴍ(Ꮡt, wr, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ execute(this ж<Template> Ꮡt, io.Writer wr, any data) => template_package.executeᴍ(Ꮡt, wr, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string DefinedTemplates(this ж<Template> Ꮡt) => template_package.DefinedTemplatesᴍ(Ꮡt);
}
partial class templateᴛTemplate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) ParseFiles(this ж<Template> Ꮡt, params ꓸꓸꓸstring filenamesʗp) => template_package.ParseFilesᴍ(Ꮡt, filenamesʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) ParseGlob(this ж<Template> Ꮡt, @string pattern) => template_package.ParseGlobᴍ(Ꮡt, pattern);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) ParseFS(this ж<Template> Ꮡt, fs.FS fsys, params ꓸꓸꓸstring patternsʗp) => template_package.ParseFSᴍ(Ꮡt, fsys, patternsʗp);
}
partial class templateᴛTemplate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Template> Option(this ж<Template> Ꮡt, params ꓸꓸꓸstring optʗp) => template_package.Optionᴍ(Ꮡt, optʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setOption(this ref Template t, @string opt) => template_package.setOptionᴍ(ref t, opt);
}
partial class templateᴛTemplate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref Template t) => template_package.Nameᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Template> New(this ref Template t, @string name) => template_package.Newᴍ(ref t, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref Template t) => template_package.initᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) Clone(this ж<Template> Ꮡt) => template_package.Cloneᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Template> copy(this ref Template t, ж<common> c) => template_package.copyᴍ(ref t, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) AddParseTree(this ж<Template> Ꮡt, @string name, ж<parse.Tree> tree) => template_package.AddParseTreeᴍ(Ꮡt, name, tree);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Template>> Templates(this ж<Template> Ꮡt) => template_package.Templatesᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Template> Delims(this ж<Template> Ꮡt, @string left, @string right) => template_package.Delimsᴍ(Ꮡt, left, right);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Template> Funcs(this ж<Template> Ꮡt, FuncMap funcMap) => template_package.Funcsᴍ(Ꮡt, funcMap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Template> Lookup(this ж<Template> Ꮡt, @string name) => template_package.Lookupᴍ(Ꮡt, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Template>, error) Parse(this ж<Template> Ꮡt, @string text) => template_package.Parseᴍ(Ꮡt, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool associate(this ref Template t, ж<Template> Ꮡnew, ж<parse.Tree> Ꮡtree) => template_package.associateᴍ(ref t, Ꮡnew, Ꮡtree);
}
// </GlyphForwarders>
