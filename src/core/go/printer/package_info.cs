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
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.printer_package;

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
[assembly: GoImplement<go.text.tabwriter_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<sizeCounter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<trimmer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: global::go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: global::go.GoFieldAccess(typeof(CommentedNode))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.AssignStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.BasicLit))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.BinaryExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.BlockStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.BranchStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.CaseClause))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ChanType))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.CommClause))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.Comment))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.CommentGroup))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.CompositeLit))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.DeclStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.DeferStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.Ellipsis))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ExprStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ForStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.FuncLit))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.GoStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.IfStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.IncDecStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.IndexExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.IndexListExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.InterfaceType))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.KeyValueExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.LabeledStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.MapType))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.RangeStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ReturnStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.SelectStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.SendStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.SliceExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.StarExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.StructType))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.SwitchStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.TypeAssertExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.TypeSwitchStmt))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: global::go.GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: global::go.GoFieldAccess(typeof(printer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/printer/comment.go", "comment.cs", "AA0exKKCgoKCAAkUlIKCgoKCgoKmgoKUgpSmpoKokpSCpoKCuKiSgoKCgoKClJTKgriS3Pz6gsqCgpSCgpSCgqaqooKCgoKUgrg=")]
[assembly: global::go.GoPositionMap("go/printer/gobuild.go", "gobuild.cs", "AA0aooIACRSClIKCpoKCgriClJSC3IKCgqaCpoKCgoKUgpTqtoK4gpSCpoKCgoKCgoKUgoKCuKaClpKCgoKUgpSCgIK2goIAAhDSlJSmgoKClIKUpoKClIKClKaC", "", "", "147=isNL/2/3/1,isNL/3/3/1")]
[assembly: global::go.GoPositionMap("go/printer/nodes.go", "nodes.cs", "ABVcABsCgoKCgoKClIKCpt7ygpSUgriClJSCyoIACBq0goKUgoKUAAQaAAoCgoKCgoKUgqamgoKClJSCpoKUlIKUAAcQgoLKgoKCuNyCpoKC7tyCgoKClIK4AAcQgoKClIKCgriCuIKUgoK4goKCgtyCgqaCuNyCgoKUpoKCpqaUgoKClJSUgqaCgqaUAAkYspKClIKCgoKCuIKUgriClKaUgoKm7oKmgsqAgoKCysiCuIIAAhDSpqSklKqi9KSkpJSmooKUgpSUgoKUgpSClLiigoKUgoKmpoKClIKClpKCgpSCpoLWsoKCgoKUlJSCgoKCgqaCgoKCgpSUlIKUlIKCgqamgoKCyoKCgoKmhIKClIKCgpSCgoKUgoKCpoKUgoKUgoKUgoKUpoKClIKqgoKCgoKUuIKClJSCgpSCgqaClJSCgpSCqIKCrOKUtLaUppSCgoK2lKaUgoKCtoLIpLTGpoKCgpSCgpSUgpSmgoKClKaCgoKUAAJOACcCgriCgoKmlIKCgpSCgoKCpoKCpoKUgoK4goLmspSUtraCgpS2goKCtoKUgoKCpoLIgpSCgqaClJTIgpS2gpSCgraAppSCgoLYtoKCgoKClJSCyIKCgoKC2oKCgoKCyIKCgoKClpKCgoKCgoKCuIKmgoKClIKCpoKmgraCyoKUtKSClIKCpoKCgoKCgoKmlIKCgtqClIKCgrimgriCgoK2goLIgoKUgraCtoK2graCgoK2lLS0goKkgsYABBgACgKClILcgraAgoK2goKC2JSAguiCgpS0tKS0pqaCgIKkgqrSgoKAgoKCgoKUpIKCpoKmgoIAAhIACAKClIKClICmppSCuIKCgpSClLaCusKCgoKCgqaClKSUpoKAuIKmpIKUppSCtqaCgIKkpoKCgpSCgsqClIKCgpSCgoKCgriCvuiCgoKUgoKCgoKmlJSUlKbWopSUtgAKEoKCgoKAgoKCgoKmpLaCtoKCgoKCtoKCgraCgpSCgoKCtoK2graCgtyCpoKU2oKCgsi2goKCgoKk6oKC6oKClJSCgraCgraCgoKClIKCgraCgpSUgoK2goKUgoKClMiCgraCgoKmgoKUgoKUgoKCxgAEPgAbApSSgoLKgoKCgoKUgqaUgqaCppSmpqKCgoKCgpSClIKCgpSCgpToAAoSgpSSggAKFoKUgoKCyoKClNzSlIKCgpSCgraClIKCgoKUgoKUtoKCgpSClJSClILGyLKCgpSUgoKAgoKmgoKCgpSCpoKCgpSCpqSChJQAChaCgoKCgriCrgAKCoCCtoLKgoKAgqSUgpSokoCCgILGqMKCgpSUlKaCgoKUgpSUAAIQAAoCgriClJSCgoKCgoKCgoKUlJSCgoKmgpSiqrKClKaygoK4goKClIKC1qKUgrS0xM7CgpS0pKaigoKCAAcQpoKCuJS4soKCgoKC", "1251-1264:1;1578-1584:1;1878-1880:1")]
[assembly: global::go.GoPositionMap("go/printer/printer.go", "printer.cs", "AFrGAaKCgoK+xIKClJSAgraCpoKCgoKAgoKCguyssqr0gqaCgoKUlN7irsKmlKaCgoKUqJKCgoKUpoKClIK6toKCuIKCqqLKlLSCyJSmgriCgoKCgoKUggACGgAKAoKClKbKpsqmgpSmgoKUgJSCuLaCgoKCgoKUgqaCpgACEgAIApSmlIKmpoKUgoKmgtiCxqSClKaCgriUzIKCgqaCxvyCAAcQgsaCpIKUpoKCgoIABxCCyoKmuM6igoKmqJKCgpSokgACENKCABYugoKCgoKUgoKUzIKC3IKAlIIACBKC3IKClIKUpoKCgoKUlKaCuP6CgoKUgpTKuIKC+tKClIKmgJK4goKCgpSCygAHEIKCuLiCgoKUgsYAAhAACQKCpgAJDIKClIKUyKaCgqaokoKCpgAFEAAJAoKCgoK6gpSmlIKUgqaCgoK4goKClKYAChaCioKUyoaUyoLYpIKA6KSCgoIACRLcgoKUpNqCrsKmopS0tLS0tKSmgoIABhwADQKEkoKm2Ma2poK2uJSCuIKUgoLKlIK2goK2goKCtoLugpSClJSWpMiCgoLGgsiCyoKUgpSCgoKUgsqCgqaCAAMQAAgClKaUqJKUpKSkpKSklKaClKSkpKSCxoK2pqSSgIKCtpSCgpSCyoCCpICCgILqgoKUgoKUgpSAlMimlKaU2ICCpLS02IKAgra0tMS2lIIAFjKCggAHEpQACAyCgrKClJSUtIK0goK0goLWgoLGlIKCtIKCgsaCgtakgqaUlIK2ACJOAAoYgoIACRSmtIKmqPSCkoCCtoK43KaClIKCpoKCgqa4gILIgIKmAAwcwq7C", "154-156:1;637-637:1", "", "796=Pos/1/1/1,posFor/1/2/1,End/1/1/2,posFor/2/2/2;842=Pos/1/1/1,lineFor/1/1/1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("printer")]
public static partial class printer_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct commentInfo {}
    internal partial struct exprListMode {}
    internal partial struct paramMode {}
    internal partial struct pmode {}
    internal partial struct printer {}
    internal partial struct sizeCounter {}
    internal partial struct trimmer {}
    internal partial struct whiteSpace {}
    public partial struct CommentedNode {}
    public partial struct Config {}
    public partial struct Mode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdocꓸcomment() => builtin.initPackage(typeof(global::go.go.doc.comment_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtabwriter() => builtin.initPackage(typeof(text.tabwriter_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
