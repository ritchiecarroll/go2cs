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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using pflagꓸParseErrorsWhitelist = go.github.com.spf13.pflag_package.ParseErrorsAllowlist;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.github.com.spf13.cobra_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2063616c6c656420626f6f6c7d", "Command_commandCalledAs")]
[assembly: GoTypeAlias("Completion", "go.@string")]
[assembly: GoTypeAlias("CompletionFunc", "System.Func<go.ж<go.github.com.spf13.cobra_package.Command>, go.slice<go.@string>, go.@string, (go.slice<go.@string>, go.github.com.spf13.cobra_package.ShellCompDirective)>")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.StringWriter>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<commandSorterByName, sort_package.Interface>]
[assembly: GoImplement<flagCompError, error>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.StringWriter>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/active_help.go", "active_help.cs", "ABpMggACEIKCgpSsgg==")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/args.go", "args.cs", "ABc4pIKogpSoooKUqrKGgoKUgoK4qIKogoKClLqCgoKUuoKCgpS6goKClLqikoKAgrYAAxCC", "75-80:1;85-90:1;95-100:1;105-110:1;115-122:1")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/bash_completions.go", "bash_completions.cs", "ACBIgoIA7wLcBYKCggAfPAAIDqaigoKClIKUpoKClJSCgpSUpJSCgpS2lIKClJT+goKCgpSCgqaCgoKClIKCgoKUpoKCgoKUgoK6ooKCioKUtAANBKKCAAEQgqaCkoKUgoKYgqaCgpSCgrimgoKCgoKUgIKCgpSClILsgoKCpoKUgriigqaUgoKClIKkgoKCgriigoKUlIKClIKUpoKCgpSCgoKCgqiigoKClIKUgqaCqKKCgpSk", "566-579:1;580-588:2;596-612:1", "", "35=activeHelpEnvVar/1/1/363")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/bash_completionsV2.go", "bash_completionsV2.cs", "ABcwgoKCgqaCgoKmALED4gbCgoKUpNqC")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/cobra.go", "cobra.cs", "AB5AACNqgqqCgryiqqIAAhCCkpSUpKSmlJSkpKasgoKUlKSkpKaCrIKClKiCgqaCpoKCksyCgoKUgoKClIKUgoKClIKClIKUuqaCgoKmqIKCgrqCgg==", "182-187:1")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/command.go", "command.cs", "AHmaBIKqgqqCroKCqoKqgqqCqIKogoKClKqCqIKogqiCgpaohKiCgoKUqIKCgpSogqqCgoKUgrqCqIKogqiCpoKClIKUpoKClIKUpoKClIKUqqKClIKUgoKCgoKUvIKCpoKUrIKqooKUgpSCgoaCgs6CgqaCpqyCgqikgpSCgpSWgpSsgoKmgpQABBCCgpTMgoKUzIKClKqCgqaClKqCgqaClKqCgqaClKqCgqaClKiCgqaClKaCgoKUpqKCpoKClKaygpSUgpSCgoKClqiogpSCtui8ooKUgpSCgoKWpKiCqIKCgoLopoKsgpSCgoKUlIKClKaCgpSmgoKUgpSCgIKCgramooKCgoKUgriGpqqygpSCmIKCtoKCtoKCtoK2goKmgIKklKiCgoKCgoKCgpSCgsqogoKCuqKClKqC5uKCpoKqgpSCgqqChoKmgqiCgoKClIKCgoKUuIKmlJSCgqaAgraCgoiWpoKCgIKkgqSCgoK4goCCtIKmgIKkgIK2goCCtpSCgIK0gpSCgoCCpIKkgoKCyuaCgriCgr6igqyCgqyigqjCgqiCqIKolJaCqJaYlIKClJSEgpSCgpSmgoKqgqaChoKCqoKqgqamooKUqIKCpoKSgoKClIK4gpSqgoSCpr6igoKCgoKUlIIAAxCigqaCgoKClJSClJS+ooKmggAIDIKCgpSElIKCgri2goKCloKmgoLcgqiCgoKCzICigKKApoSCgpSosoKClISCgpSCgpSCgpaClIK6gqiCgoKmqIKCgqaooqiygoKCgoKCprSUgoKCgoKClIKClIKCzKKooqiiqKKooqiiqIKClKqCgIKkqIKCgoKUlIKUgpTaooKUgoKUgoKClLiCgoKCpriCgoLKqIKCgoKUqIKCgqaqgoKUqoKCgpSCgoKmqIKogqiCqIKqooKmgqaCpgACEISCqIKCuqyEgoK6qoaCgryogqiCqoKCgoKUpqqilIKigqaqopSCgoKUlIKCpoSCpoKCqqKUgoKClKaCgqaSgqaqgqiCgoKClJSogoKCgoKClIKCqIKogqiCqIKqgqiCqoKqgqiylIKmqLKCpoKClKiigqaClIKWlISCpqiCqqKCgqyigoKCpoKmlIK+goKmAChSgoKCgpSClIKClIKClIKCgoKCgriCgoKCuIKCgoLcgoKUgoKUgoKCgriClIIACBCCgoKClIKCgpSClP6CgoI=", "451-459:1;491-500:1;555-557:1;760-772:1;1187-1195:1;1274-1292:1;1293-1308:2;1505-1535:1;1510-1516:1.1;1519-1527:1.2;1706-1710:1;1731-1736:1;1760-1764:1;1920-1922:1", "", "1066=DisplayName/1/1/4")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/completions.go", "completions.cs", "ACRMlgAIFoIAGIwBogACJIIAAhCCAAIQggADEqKCgpSClICCpILYooKCpoKUgtiigoKUgpSClIKUgpSClIKmgpSoogAKFoKCrIKCgIK2goKihJSErp6WrJimgoKMAAcYtoKUgoKGgp6CgqaUhJSegoKunIKCloCCtoKEloSAgsyCqoKmhICChAAEEoCChJbIgpYAARCEloKCgpqGvpKekriChqaKpoSCgoK4goaEgpKCzIiCgoKUzJiChIKCppiCgoIABRAABBSCgoKClJSGgoKmpqKAhKSAhKSmgoKmgoKEAAIggoKmpoKUgoCChAAFEJKUkqamooamgoKCmoKAhISclIKWyIKCgoiAgoScmOyEpoKEpoKIgrgAAhCygqaChLiWgoKEgrgADBaUjoKGgriCkoIAHz6CpgAfOoKUpoKmABUqgqYAER6ClKiCpqaigoaAgpSCgIKU2K6imICClIKSyIS0rIKogoKoggAFEpqEgoIAAhCCgoKU", "161-163:1;242-294:1;444-456:1;461-463:2;469-471:3;505-509:4;632-639:1;644-646:2;647-649:3;828-830:1;864-869:2;892-894:3;914-920:4", "", "168=Sprintf/1/2/1,MinimumNArgs/1/1/6,Sprintf/2/2/8;319=Type/1/3/1,Contains/1/2/1,Type/2/3/2,Contains/2/2/2,Type/3/3/3,HasPrefix/1/1/3;567=Root/1/1/5,Name/1/1/5,Sprintf/1/1/3;584=Name/2/2/1;594=Sprintf/1/2/2,Root/1/1/23,Name/1/1/23,Sprintf/2/2/3;630=Sprintf/1/2/2,Root/1/1/25,Name/1/1/25,Sprintf/2/2/3;672=Sprintf/1/2/2,Root/1/1/14,Name/1/1/14,Sprintf/2/2/3;698=Sprintf/1/2/2,Root/1/1/11,Name/1/1/11,Sprintf/2/2/3")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/fish_completions.go", "fish_completions.cs", "ABgyhIKClIKClIIA8AHgA4KCgoKowoKClKQ=", "", "", "35=activeHelpEnvVar/1/1/236")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/flag_groups.go", "flag_groups.cs", "AB5CsoKCgoKUgITesoKCgoKUgITesoKCgoKWgILegoKmmIKCgsKCgqaAgqSAgqSAgqSmooKCgqamgoKCgoKWgqaCgrjKgoKClIKCgqaCqIKmpqKCgoKCgoKmgqiClKaCgoKCgoKCpoKogpSmgoKCgoKUggACEIKCpoKCgoLCgoKqgoKEggAFEIKUooK8goLOgoKIgoKC", "93-97:1;234-238:1")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/powershell_completions.go", "powershell_completions.cs", "ABg4hIKClIKClACcAqYEgoKCgqbCgoKUpNiCqoKogqqC", "", "", "34=activeHelpEnvVar/1/1/272")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/shell_completions.go", "shell_completions.cs", "ABYwgqyCrIKqogACEoKsoqqiAAISgqqCrIKqgg==")]
[assembly: go.GoPositionMap("github.com/spf13/cobra@v1.10.2/zsh_completions.go", "zsh_completions.cs", "ABgygqqCqIKqggACGKIAAhSipsKCgpSk1oKCgoKmgoKClA==")]
// </GoSourcePositionMaps>

namespace go.github.com.spf13;

[GoPackage("cobra", ImportPath = "github.com/spf13/cobra")]
public static partial class cobra_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct commandSorterByName {}
    internal partial struct flagCompError {}
    internal partial struct tmplFunc {}
    public partial interface SliceValue {}
    public partial struct Command {}
    public partial struct Command_commandCalledAs {}
    public partial struct CompletionOptions {}
    public partial struct FParseErrWhitelist {}
    public partial struct Group {}
    public partial struct ShellCompDirective {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸspf13ꓸpflag() => builtin.initPackage(typeof(go.github.com.spf13.pflag_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
}
