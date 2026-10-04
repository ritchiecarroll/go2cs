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
global using commentꓸText = go.go.doc.comment_package.ΔText;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using token = go.go.token_package;
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
[assembly: GoImplicitConv<token.FileSet, ж<token.FileSet>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/printer/comment.go", "comment.cs", "AA0epKKCgoKCAAkUlJKCgoKCgoKmgoKUgpSmloKYkoSClpKCuJiSgoKCgpKClJTKgriC3Pz6grqCgpSCgpSCgqaqooKCgoKUgrg=")]
[assembly: global::go.GoPositionMap("go/printer/gobuild.go", "gobuild.cs", "AA0agoIACBSClIKCpoKCgriClISCzIKSgpaClpKCgoKUgpTaxoK4gpSCpoKCgoKCgoKUgoKCuJaChpKCgoKUgpSCgIKmgoIAAhDSpJSmgoKClIKUpoKClIKClKaC")]
[assembly: global::go.GoPositionMap("go/printer/nodes.go", "nodes.cs", "ABVcABsCgoKCgoKClIKCpt7ygpSUkriClJSCyoIACBq0goKUgoKUAAQaAAoCgoKCgoKUgqaWgoKChJSCpoKUlIKUAAUQgoK6goKCqMyCloKC3syCgoKClIK4AAYQgoKClIKCgqiCuIKUgoK4goKCgtyCgqaCqNyCgoKUloKClpaUgoKClJSUgpaCgpaUAAkYspKClIKCgoKCuIKUgriClKaUkoKm7oKmgrqAgoKSyriCqIIAAhDSlqSkpKqi9KSkpJSmooKUgpSUgoKUgpSClLiigoKUgoKmpoKClIKClpKCgpSCpoLWsoKCgoKElJSCgoKCkqaCgoKCgpSUlIKUpIKCgqamgoKCuoKCgoKWhIKClIKCgpSCgoKUgoKCpoKUgoKUgoKUgoKUpoKClIK6goKCgoKUuIKClJSCgpSCgqaClJSCgpSCqIKCrOKUtLaUppSCgoKmlKaUgoKCtpLIlKTGpoKCgpSCgpSUgpSmgoKClKaCgoKUAAJOACcCgriCgoKWhIKCgpSCgoKCpoKCpoKUgoK4goLmooSUtraCgpS2goKCtoKUgoKCpoLIgpSCgqaClJTIgpS2goSCgsaAppSCgoLYtoKCgoKClJSCuJKCgoKCuqKCgoKCuJKCgoKClpKCgoKCgoKCuIKmgoKClIKCpoKmgraCmoKUtKSilIKCloKCgoKCgoKmlIKCgsqSlIKCgoiG0piigoK2goLIgoKUgraCtoK2graCgoK2lKSkgoKkgsYABBgACgKClILMkgABHoSAssiSgpSkpKSkAAM9gIKCpoKCggAFNKaCgIKkgqrSgoKAgoKCgoKUpIKCpoKmgoIAAhIACAKClIKClICmppSCuIKCgpSClLaCusKCgoKCgqaClKSUpoKAuIKWpJKUppSCtqaCgIKkpoKCgpSCgsqClIKCgpSCgoKCgriCvuiCgoKUgoKCgoKmlJSUlKbWooSUtgAGEoKCgsKAgoKCgoKmpLaCtoKCgoKCtoKCgraCgpSCgoKCtoK2graCgtyCpoKU2oKCgsi2goKCgoKkurKC+oKClJSCgraCgraCgoKClIKCgraCgpSUgoK2goKUgoKClMiCgraCgoKmgoKUgoKUgoKCxgAEPgAbAoSSgoK6goKCgoKUgqaUgqaCppSWpqKCgoKCgpSClIKCgpSCgpToAAkSgpSSggAJFoKUgoKCuoKClNzSlIKCgpSCgraClIKCgoKUgoKUtoKCgpSClJSClILGyLKCgoSUgoKAgoKmgoKCgpSCpoKCgpSCpqSClJQAChaCgoKCgriCrgAKCoCCpoK6koKAgqSUgpSokoCCgILGqMKCgpSUlKaCgoKUgpSUAAIQAAoCgqiClISCgoKCgoKCgoKUlJSCgoKWgpSiqrKClKaygoK4goKClIKC1qKUgrS0xM7CgpS0pKaigoKCAAcQpoKCuJS4soKCgoKC", "1251-1264:1;1578-1584:1;1878-1880:1")]
[assembly: global::go.GoPositionMap("go/printer/printer.go", "printer.cs", "AFnGAaKCgoK+xIKClJSAgraCpoKCgoKAgoKCguyssqr0gpaCgoKUlN7irsKmlKaSgoKkqJKCgoKUloKClIK6toKCqIKCqqLKlLSCyJSWgqiCgoKCgoKUggACGgAKAoKClJbKlsqWgpSWgoKUgJSCuLaCgoKCgoKUgpaClgACEgAIApSWlIKWpoKUgoKWgqiCpuSClKaCgriUzIKCgpaCpqziAAEQ4raCpIKUloKCgpIABhCCuoKWuM6igoKmqJKCgpSokgACENKCABQugoKCgoKUgoKUzIKCzIKAlIIACBKC3IKClIKUpoKCgoKUlKaCuO6CgoKUgpTKqIKC+tKChIKmgJKogoKSgpSCugAGEIKCqKiCgoKUgsYAAhAACQKClszCgpSClMiWgoKWqJKCgqYABRAACQKCgoKCuoKEppSClIKmgoKCuIKCgpSWAAoWgoqClMqGlLqC2KSCgNikgpKCAAMSAAsMgoKUpNqCrsKmopSkpKSkpKSmgoIABhwADQKEkoKWuKbGloK2yJSCuIKUgoLKlIK2goK2goKCtoLugpSClISWpLiCgoLGoriCuoKUgpSCgoKUgrqCgpaCAAMQAAgClKaUqJKUpKSkpKSklKaClKSkpKSCxoK2pqSSgIKCppSCgpSCyoCCpICCgILqgoKUgoKUgqSAlLiWhJaUuICipLS0uKKAgra0tMSmhIIAGDKCggAHEpQACAyCgrKClJSUpIKkgoKkgoLGgoK2lIKCpIKCgraCgsakgqaElIKmABxOAAcYgoIACRSmtIKWqPSCkoCCtoKozJaChIKCloKCgpaogIK4gIKmAAwcwq7C", "154-156:1;637-637:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("printer")]
public static partial class printer_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
