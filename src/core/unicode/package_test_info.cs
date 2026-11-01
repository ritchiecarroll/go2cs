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
using static go.unicode_package;
using static go.unicode_test_package;

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
[assembly: GoFieldAccess(typeof(CaseRange))]
[assembly: GoFieldAccess(typeof(Range16))]
[assembly: GoFieldAccess(typeof(Range32))]
[assembly: GoFieldAccess(typeof(unicode_package.Range16))]
[assembly: GoFieldAccess(typeof(unicode_package.RangeTable))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("unicode/casetables.go", "casetables.cs", "AAsYou4=")]
[assembly: GoPositionMap("unicode/digit.go", "digit.cs", "AAcQkoKU")]
[assembly: GoPositionMap("unicode/digit_test.go", "digit_test.cs", "AAsYADp2ACJGgoKCpoKCzJKCgg==")]
[assembly: GoPositionMap("unicode/graphic.go", "graphic.cs", "ABcu7M7WgpQAAhDSgpSqooKCpqiygoKmrLKCpqiSgpSopKiSgpSqooKUAAIWAAkEgpSUlKiSgpQ=")]
[assembly: GoPositionMap("unicode/graphic_test.go", "graphic_test.cs", "AAseooKCgpS0pILKgoKCgoLKgoKCgoLKgoKCgoLKgoKCgoLKgoKCgoKUgsqCgoKCgsqCgoKCgsqCgoKCgsqCgoKCgg==")]
[assembly: GoPositionMap("unicode/letter.go", "letter.cs", "AFa2AZKCgoKClIKmuIKCgoKCgpSClKaokoKCgoKUgqa4goKCgoKClIKUpqiylIKUgoKUpoKUgIKkgoKUqKSClKikgpSokoKUqrSCgoKCgoKUgpSmqJKCAAoWlKqigpSAgqSokoKokoKClJSokoKClJSokoKClJSokoKClKiSgoKUqJKCgpQACzwAEgKCpoK4goKCgoKUpoLcgIKAgqSk")]
[assembly: GoPositionMap("unicode/letter_test.go", "letter_test.cs", "ABAiABgyAA0cACJGAAwaABEkAHPoAYKCgqaCgqaCgsqCgoKmgoKmgoIADgqClKSklKaCgoKCyoKCgpSCgsqCgoKUgoLKgoKClIKCyoKCgqaCgs6igoKUgpSClIKUgpSClIKUgsqCgoKCgoKUgpSClIKUgpSCygAXMIKCgoKCgIKkuICCAA0clIKCpoLugoKSgoKCgriSgoKCgriCgoKUpoKCgpSmgoKCgpSCpqaUgoKCgoKClIKUpqaC7oKCgoKUggALDIaigoKCuO4AESSCgoLKgoKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgsqigriiggANCIKCgoK4goKC", "467-491:1;469-477:1.1;478-486:1.2;659-665:1;660-664:1.1")]
[assembly: GoPositionMap("unicode/script_test.go", "script_test.cs", "ABAiACtYAClMgoKClIKAgqSClJSC+IKCgpSCgIKkgpSUgg==")]
[assembly: GoPositionMap("unicode/tables.go", "tables.cs", "AAoSACpUAC1aAOgE0gkABxAAGDIA4gTGCQAJFNwA9gTuCQCOAZ4CAKoB1gIAPHoA6wPYBwALGACcAboCALEC5AQApAHKAgAJFAC6AvYEAI4BngIAR5ABABEkAE6eAQCuAd4CAAkUABImABw6AAkUAAoWAKsB2AIAH0AA0AGiAwAWLgAhRAA4cgCtAdwCAAsY3NwACxqCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKoAKYBzAIACBIACBLuADdwAAgSAAcQ7gAIEgAHEO4AEiYACRQABxAACBLc7twACRTuAAcQAAcQAAgSAAcQ7gCrAdgCAAcQAAkUAAoW7gAPIO4ACxgADRzuAAoW7u7uACpWAAwaAAwa7gATKAAmTgASJgALGAAUKgAaNgASJgAHENwACBIADRwADBoABxAAIUQABxAABxAABxAABxAAESQAFCoACBLuAA0cAAcQAAgSAAcQAAcQAA8gACtYAAcQAAkUAAgSAAwaAAgS7gAHEO7uAAsY7gAHEAAIEgAMGu4ABxAABxAACBLuAAgSAAcQAAwaAAgSAAoWAAcQAAcQ7gAIEgAIEgAHEO4ABxAACRTc3AAIEgAHEO7uAAcQ7u7u7gASJgAHEAAHEAAKFu7u3AAHEAAIEu7u7u7u7gAHEAAIEgATKO4ABxDu7twACBLuAAcQ7gAJFO4ABxAAFzAABxAACRQAESTc7gALGAAHEAAHEO4ABxDcAA0cAAcQAAcQAAgS7gAHEoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgqgAJkwACBIACBIAFCoADBoAqgHWAgAdPAALGAALGO7cABo23AAKFgAYMgDsAdoDAA8gABYuAAcQAAcQACBCAHz6AQALGAAdPAAJFAALGAAPIAAHEO4ASJIBACBCAGPIAQAVLAAKFgANHoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoK6ogDKApQFAIIChgQAggGGAgBevgEACRLcAH38AQANHAB+/gHuAAwW3Nzc")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("unicode")]
public static partial class unicode_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct foldPair {}
    [GoValueClone("Delta")] public partial struct CaseRange {}
    public partial struct Range16 {}
    public partial struct Range32 {}
    public partial struct RangeTable {}
    public partial struct SpecialCase {}
    public partial struct d {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

[GoPackage("unicode_test")]
public static partial class unicode_test_package
{
}
