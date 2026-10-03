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
using static go.vendor.golang.org.x.text.unicode.bidi_package;

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
[assembly: GoTypeAlias("Class", "ΔClass")]
[assembly: GoTypeAlias("Direction", "ΔDirection")]
[assembly: GoTypeAlias("Run", "ΔRun")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bracketPairs, sort_package.Interface>]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bidi.go", "bidi.cs", "ACaKAQALAoIADzIACQKClIKChIKCgoKClIKCgpKmgpSCpgACEPKCggACEPKCgqyyrLIAAhDSgoKCgoKmpoKEgoSmgoKUlIKCgoKUgqaCgoKosoKWgpSCgpSCgpaEgqqigoKClIKCAAsesqiSqJLKABAkkqiSAAIS4qqirLKCgoSCgoKogpSErLKCgoKCgoKUpg==", "70-72:1")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/bidi/bracket.go", "bracket.cs", "AC5wgsyAooCigAACFAAIAtyCgpSCABxKogAEFPaWlJSWkoKWqIKigoKUlIKUpgAFfAA5ApakpAAEFgAJAoKCgoKUgpSmqqKCgILIqKS8gpaUgpQABhCmgoKEgoKClJaCgoKUupKC")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/bidi/core.go", "core.cs", "ADd6soKCpgAXSPKCgIKkgIKkgIKkgIKmAAYSgqaAqMLMgqiCqAAHFKaoqKiWzAACJAAPAoKEgpaChICCgoKAgpKCgIKCgtiCAAkW0paCgIKCkoKCgtqWpKQAEh6AooCigKSCgoKCpoKmgqaCqJKCtoSUlIKGkpSCgoKogpSmloKC3JSkpLaCyoKkgt6SkqaCgpSClKqElLKSggADEIKCgoKmgsIADyKApIKClKrCgoKCqIKClIKCloKCgqaCloKCppIACRzWqIKCgsrMgoKCgIKClO6CggAOIoKCgoKCgpKCzKKUgpaCgpSCgoKmgqa6goK6opSCgoKSgqaC3qaEopaClqiilIKCpoKUgoKogpTKlobakoK4goK6poSChJKUlJKkyJS2AAUQooKCvtKCgoKCgqa0qsKCgoKCpgAFFOKCgoSCgpSSgqaUuIKUqLKWgoKCqISEooKClJSEgoKClKamrsKCgoLegpSCggAEIgAWGIS6gpSWgpKU3oKCgpKUppYAAiQADwKEqqKEgoKChIKUlKyylILMgoKCgpSCqIKClIKCgpaCprqqopSkqJKUpKiSgpSmgoKUgoKmpoKGlKaCgoKClJSClKaCgpSCtramgoKUgpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/bidi/prop.go", "prop.cs", "ABAeAAEaAAsCgoKUqJCooKaSAAsGAAsakoKCAAIqABICgpS0pJKUgoKClKSSlIKCgpSCgoKClKSSlIKCgpSCgoKClIKCgoKUtqyygpS0pJKUgoKClKSSlIKCgpSCgoKClKSSlIKCgpSCgoKClIKCgoKUtg==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/bidi/tables15.0.0.go", "tables15.0.0.cs", "AAwYvrKClKSkopSCgoKUpJKUgoKClIKCgoKUpJKUgoKClIKCgoKUgoKCgpS2qqKCkpSCkpSCkpSCkpSssoKUpKSilIKCgpSkkpSCgoKUgoKCgpSkkpSCgoKUgoKCgpSCgoKClLaqooKSlIKSlIKSlIKSlOyCqJKUAIkCDADHC+Ia")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

[GoPackage("bidi", ImportPath = "vendor/golang.org/x/text/unicode/bidi")]
public static partial class bidi_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bidiTrie {}
    internal partial struct bracketPair {}
    internal partial struct bracketPairer {}
    internal partial struct bracketPairs {}
    internal partial struct bracketType {}
    [GoValueClone("embeddingLevelStack", "overrideStatusStack", "isolateStatusStack")] internal partial struct directionalStatusStack {}
    internal partial struct level {}
    internal partial struct paragraph {}
    internal partial struct ΔisolatingRunSequence {}
    public partial struct Ordering {}
    public partial struct Paragraph {}
    public partial struct Properties {}
    public partial struct options {}
    public partial struct ΔClass {}
    public partial struct ΔDirection {}
    public partial struct ΔRun {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
