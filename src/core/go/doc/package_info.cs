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
using ast = go.go.ast_package;
using comment = go.go.doc.comment_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.doc_package;
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
[assembly: GoFieldAccess(typeof(Example))]
[assembly: GoFieldAccess(typeof(Note))]
[assembly: GoFieldAccess(typeof(Package))]
[assembly: GoFieldAccess(typeof(Value))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BasicLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BinaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Comment))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommentGroup))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexListExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.KeyValueExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.MapType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StarExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StructType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: GoFieldAccess(typeof(go.go.doc.comment_package.Doc))]
[assembly: GoFieldAccess(typeof(go.go.doc.comment_package.Parser))]
[assembly: GoFieldAccess(typeof(go.go.doc.comment_package.Printer))]
[assembly: GoFieldAccess(typeof(go.go.doc_package.Func))]
[assembly: GoFieldAccess(typeof(go.go.doc_package.Type))]
[assembly: GoFieldAccess(typeof(namedType))]
[assembly: GoFieldAccess(typeof(reader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/doc/comment.go", "comment.cs", "AAtAABQCgoKCggACNAAXAoLK")]
[assembly: GoPositionMap("go/doc/doc.go", "doc.cs", "AHDyAeKCgoKCABAigoKClKaCgoLKgoKUlIKCgoKCgriCgoKCgpSUAAQcAAsCgoKClIKClIKC3KKCgoKUgoKUgoIACTgAGgSClILogoKUtLi6goKClKS0grQACRSCgoIAAhTygpQAAhTygoKUlIKUrsIABRTmAAIQ8gACEPIAAhQACQI=", "", "", "124=sortedKeys/1/1/4,noteBodies/1/1/7,sortedValues/1/2/8,sortedTypes/1/1/9,sortedValues/2/2/10,sortedFuncs/1/1/11")]
[assembly: GoPositionMap("go/doc/example.go", "example.cs", "AC9kABICgoKCgoKCgoKUgoKUgoKCgpSClIKUgpSClIKClKIACxi4gqSmhqaWspSCgoKUlIKClKassoKUgpSCAAgKwpSmuIKUgpSClIKCgtiClLSCAAkQmrLKgpKCgqamgoKClKaUgoKClILGppSCgoKSgsqCgtyCqJKCgsqCgtymgpSCxoLsyqbcgoKClIaqAAYcAAwMgoKClILClIKChIKCgqbagtqClKaCgoLcpoKmlIKmgsaClIKCtIKUggAIHgAIAoKUtISSgpSCxoLugoKUgoKCgoKCgriCyJSCpoKClJTapoKmgqaqooKCgpSoxIKUiqKCgoKUlIKm+tSCgqiSopSCgoKUuIKCkoKq1IKCuMqCgoKo0oKUkoKClIKUlAACHAANAoKmgoKCgpSUgoKUgoKClJSCgpTK7oKCgpSCgpSCgsqCAAMW0oKClIKUlAACEgAJAoKUgpSCpoKCrNKCgoI=", "110-112:1;207-214:1;315-317:2;318-320:3;349-377:1;379-385:2;523-525:1;680-682:1", "", "106=playExample/1/1/4;344=NewIdent/1/1/1;359=NewIdent/1/1/1;648=Pos/1/1/3")]
[assembly: GoPositionMap("go/doc/exports.go", "exports.cs", "AAwgooKCgoKmppSCgoKCuIKCgpS0gpSCtoKUqsKCgpSmqJKCgqaokoKCgoKUgqaCgqaClK7ygpSCgoKClIKCgoK4ggAHEKaCgpSCpoKCgqaClIKosoKCAAQQsti0tILGgoLGtILGgoK0gsaCtMiCpqSC7oKCpoKCgvyCgoKCgpS2rLKUpJTspoKGooKClJSUlMqCgoKCpqaClILslKiygoKCgqY=", "", "", "295=NewIdent/1/1/1")]
[assembly: GoPositionMap("go/doc/filter.go", "filter.cs", "AAwWooKCgoLKpoKClIKC2IK4lILGguqmgoKCgoKmpoKCgoKCpqaCgoKCgqaCgoKClIKCpqqigoKCgg==")]
[assembly: GoPositionMap("go/doc/reader.go", "reader.cs", "ACNAopSktraCgoKCgoKClIK25oKClK7igtymgoKUgpSU7oK+0oKCgpSUAAYY0pSkpKSmxqSUADJygq7CgpSCptyCrvKCgpSCgoKUpqaCgoKUpoKClKaCgpSCpqgACAqCgoKCgoKClIKmguqklKaClIKUgriCuIKClIK43ILcqMKClIK0pIKUqMKCgsqmgpSUgoKUmrKCgoLMkqjEgriUppSCppSC7sqCgoKCgoCmpIKmlIKCgoLugoLKqsKClIKCgri6gprCgoKCgoKUgoK4gpSokoLKgoKCAAke0oKCgoKCgpSmgszUgoKCyoKUpoKCgoKCgoKCpoKClIKCgoIACRDG3IKUlILKAAoWAAkQgoK4xIKCgoKCgriCgoKUpoKCgpSmgoLKgoKCAAUSwoK4goKCgoKCgpSmgqaCpoKClJSosoLcgpSCpoKmqJKUlAAIEoKCAAQSwoKClO6UpqaClILcggAEEKKCgoKClIKokoKCpqaCgoKCgoKmlIKCgpSmpoKCgoIACBKmmKaCgpSmgoKClPyCtoKGqqKCgpSuwqYAGDIAFCoABhSynoKCgoKCuIKClA==", "852-858:1;879-881:1;909-911:1;990-995:1", "", "106=Text/1/1/1;357=Text/1/1/1,specNames/1/1/2;570=Pos/1/1/1,End/1/1/2;674=Pos/1/1/7;923=sortedValues/1/2/4,sortedValues/2/2/5,sortedFuncs/1/2/6,sortedFuncs/2/2/7")]
[assembly: GoPositionMap("go/doc/synopsis.go", "synopsis.cs", "AAwgsqKigpSClIKUlK7CggAHEAAGGAAIAoKCgoKmgoKCgpSClII=")]
// </GoSourcePositionMaps>

namespace go.go;

using static global::go.go.doc_package;
using ast = global::go.go.ast_package;
using comment = global::go.go.doc.comment_package;
using token = global::go.go.token_package;

[GoPackage("doc")]
public static partial class doc_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct embeddedSet {}
    internal partial struct methodSet {}
    internal partial struct namedType {}
    internal partial struct reader {}
    public partial struct Example {}
    public partial struct Func {}
    public partial struct Mode {}
    public partial struct Note {}
    public partial struct Package {}
    public partial struct Type {}
    public partial struct Value {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdocꓸcomment() => builtin.initPackage(typeof(global::go.go.doc.comment_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸlazyregexp() => builtin.initPackage(typeof(global::go.@internal.lazyregexp_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(doc_package.Package)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class docᴛPackage;
[GoMethodHost(typeof(doc_package.methodSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class docᴛmethodSet;
[GoMethodHost(typeof(doc_package.reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class docᴛreader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class docᴛPackage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectValues(this ref Package p, slice<ж<Value>> values) => doc_package.collectValues⁔(ref p, values);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectTypes(this ref Package p, slice<ж<Type>> types) => doc_package.collectTypes⁔(ref p, types);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectFuncs(this ref Package p, slice<ж<Func>> funcs) => doc_package.collectFuncs⁔(ref p, funcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectInterfaceMethods(this ref Package p, ж<Type> Ꮡt) => doc_package.collectInterfaceMethods⁔(ref p, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectStructFields(this ref Package p, ж<Type> Ꮡt) => doc_package.collectStructFields⁔(ref p, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool lookupSym(this ref Package p, @string recv, @string name) => doc_package.lookupSym⁔(ref p, recv, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string importPath, bool ok) lookupPackage(this ref Package p, @string name) => doc_package.lookupPackage⁔(ref p, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<comment.Parser> Parser(this ж<Package> Ꮡp) => doc_package.Parser⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<comment.Printer> Printer(this ref Package p) => doc_package.Printer⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> HTML(this ж<Package> Ꮡp, @string text) => doc_package.HTML⁔(Ꮡp, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Markdown(this ж<Package> Ꮡp, @string text) => doc_package.Markdown⁔(Ꮡp, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Text(this ж<Package> Ꮡp, @string text) => doc_package.Text⁔(Ꮡp, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Filter(this ref Package p, Filter f) => doc_package.Filter⁔(ref p, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Synopsis(this ж<Package> Ꮡp, @string text) => doc_package.Synopsis⁔(Ꮡp, text);
}
partial class docᴛmethodSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this methodSet mset, ж<ast.FuncDecl> Ꮡf, bool preserveAST) => doc_package.set⁔(mset, Ꮡf, preserveAST);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this methodSet mset, ж<Func> Ꮡm) => doc_package.add⁔(mset, Ꮡm);
}
partial class docᴛreader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*removedFields*/ filterFieldList(this ref reader r, ж<namedType> Ꮡparent, ж<ast.FieldList> Ꮡfields, ж<ast.InterfaceType> Ꮡityp) => doc_package.filterFieldList⁔(ref r, Ꮡparent, Ꮡfields, Ꮡityp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void filterParamList(this ref reader r, ж<ast.FieldList> Ꮡfields) => doc_package.filterParamList⁔(ref r, Ꮡfields);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void filterType(this ref reader r, ж<namedType> Ꮡparent, ast.Expr typ) => doc_package.filterType⁔(ref r, Ꮡparent, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool filterSpec(this ref reader r, ast.Spec spec) => doc_package.filterSpec⁔(ref r, spec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ast.Spec> filterSpecList(this ref reader r, slice<ast.Spec> list, token.Token tok) => doc_package.filterSpecList⁔(ref r, list, tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool filterDecl(this ref reader r, ast.Decl decl) => doc_package.filterDecl⁔(ref r, decl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fileExports(this ref reader r, ж<ast.File> Ꮡsrc) => doc_package.fileExports⁔(ref r, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isVisible(this ref reader r, @string name) => doc_package.isVisible⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<namedType> lookupType(this ref reader r, @string name) => doc_package.lookupType⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string /*fname*/ recordAnonymousField(this ref reader r, ж<namedType> Ꮡparent, ast.Expr fieldType) => doc_package.recordAnonymousField⁔(ref r, Ꮡparent, fieldType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readDoc(this ref reader r, ж<ast.CommentGroup> Ꮡcomment) => doc_package.readDoc⁔(ref r, Ꮡcomment);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remember(this ref reader r, @string predecl, ж<ast.InterfaceType> Ꮡtyp) => doc_package.remember⁔(ref r, predecl, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readValue(this ж<reader> Ꮡr, ж<ast.GenDecl> Ꮡdecl) => doc_package.readValue⁔(Ꮡr, Ꮡdecl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readType(this ref reader r, ж<ast.GenDecl> Ꮡdecl, ж<ast.TypeSpec> Ꮡspec) => doc_package.readType⁔(ref r, Ꮡdecl, Ꮡspec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isPredeclared(this ref reader r, @string n) => doc_package.isPredeclared⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readFunc(this ref reader r, ж<ast.FuncDecl> Ꮡfun) => doc_package.readFunc⁔(ref r, Ꮡfun);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readNote(this ref reader r, slice<ж<ast.Comment>> list) => doc_package.readNote⁔(ref r, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readNotes(this ref reader r, slice<ж<ast.CommentGroup>> comments) => doc_package.readNotes⁔(ref r, comments);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readFile(this ж<reader> Ꮡr, ж<ast.File> Ꮡsrc) => doc_package.readFile⁔(Ꮡr, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readPackage(this ж<reader> Ꮡr, ж<ast.Package> Ꮡpkg, Mode mode) => doc_package.readPackage⁔(Ꮡr, Ꮡpkg, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectEmbeddedMethods(this ref reader r, methodSet mset, ж<namedType> Ꮡtyp, @string recvTypeName, bool embeddedIsPtr, nint level, embeddedSet visited) => doc_package.collectEmbeddedMethods⁔(ref r, mset, Ꮡtyp, recvTypeName, embeddedIsPtr, level, visited);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void computeMethodSets(this ref reader r) => doc_package.computeMethodSets⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanupTypes(this ref reader r) => doc_package.cleanupTypes⁔(ref r);
}
// </MethodForwarders>
