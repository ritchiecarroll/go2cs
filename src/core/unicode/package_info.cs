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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("unicode/casetables.go", "casetables.cs", "AAwYku4=")]
[assembly: go.GoPositionMap("unicode/digit.go", "digit.cs", "AAcQkoKU")]
[assembly: go.GoPositionMap("unicode/graphic.go", "graphic.cs", "ACIu7L7WgpQAAhDSgpSqooKCpqiygoKmrLKCpqiSgpSopKiSgpSqooKUAAIWAAkEgpSklKiSgpQ=")]
[assembly: go.GoPositionMap("unicode/letter.go", "letter.cs", "AFe2AZKCgoKClIKmqIKCgoKCgpSClKaokoKCgoKUgqaogoKCgoKClIKUpqiylIKUgoKUpoKUgIKkgoKUqKSClKikgpSokoKUqrSCgoKCgoKUgpSmqJKCAAoWlKqigpSAgqSokoKokoKClJSokoKClJSokoKSlJSokoKClKiSgoKUqJKCgpQACzwAEgKCloKogoKCgoKUpoLMgIKAgqSk")]
[assembly: go.GoPositionMap("unicode/tables.go", "tables.cs", "AAoSACZOAB4+AAcQABgyAAkU3AD2BO4JAKoB1gIAPHoA6wPYBwALGACcAboCALEC5AQApAHKAgAJFAC6AvYEAI4BngIAR5ABABEkAE6eAQCuAd4CAAkUABImABw6AAkUAAoWAKsB2AIAH0AA0AGiAwAWLgAhRAA4cgCtAdwCAAsY3NwACxqSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSuAClAcwCAAgSAAgS7gA3cAAIEgAHEO4ACBIABxDuABImAAkUAAcQAAgS3O7cAAkU7gAHEAAHEAAIEgAHEO4AqwHYAgAHEAAJFAAKFu4ADyDuAAsYAA0c7gAKFu7u7gAqVgAMGgAMGu4AEygAJk4AEiYACxgAFCoAGjYAEiYABxDcAAgSAA0cAAwaAAcQACFEAAcQAAcQAAcQAAcQABEkABQqAAgS7gANHAAHEAAIEgAHEAAHEAAPIAArWAAHEAAJFAAIEgAMGgAIEu4ABxDu7gALGO4ABxAACBIADBruAAcQAAcQAAgS7gAIEgAHEAAMGgAIEgAKFgAHEAAHEO4ACBIACBIABxDuAAcQAAkU3NwACBIABxDu7gAHEO7u7u4AEiYABxAABxAAChbu7twABxAACBLu7u7u7u4ABxAACBIAEyjuAAcQ7u7cAAgS7gAHEO4ACRTuAAcQABcwAAcQAAkUABEk3O4ACxgABxAABxDuAAcQ3AANHAAHEAAHEAAIEu4ABxKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpK4ACVMAAgSAAgSABQqAAwaAKoB1gIAHTwACxgACxju3AAaNtwAChYAGDIA7AHaAwAPIAAWLgAHEAAHEAAgQgB8+gEACxgAHTwACRQACxgADyAABxDuAEiSAQAgQgBjyAEAFSwAChYADR6SkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSypIAygSUBQCCAoYEAIIBhgIAX74BAAgS3AB9/AEADRwAfv4B7gALFtzc3A==")]
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
    // </ImportInitializers>
}
