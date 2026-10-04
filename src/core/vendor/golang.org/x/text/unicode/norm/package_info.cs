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
using static go.vendor.golang.org.x.text.unicode.norm_package;

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
[assembly: GoTypeAlias("Properties", "ΔProperties")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<normReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<normWriter, io_package.WriteCloser>(Pointer = true)]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/composition.go", "composition.cs", "ABhMoqqigpSCgIKCAAkSgoKUrsKClIKClIKClKaCABg2goKCgqaCgoKCpoKCqJKCpqKClIKCqLKCgoKUqJKCgoKUgqqigoKClIKssoKCgpSCgpSmgoKCggAIIuKAgoKkgpSCruKAgqSUlL7SuIKCgpSClKqigqiSqJKCgoKCqJKCgqiSgoKqooIALUCCgpSCgpSClKSkpKSmgoKUgoKUgpSkpKSkqKSmgoKCqqKCgoKCgoKCgpSssoKCgoKCgryigoKCgoKClJSClIKCmMzEgtis1t6CgpSCgoKmgpTKgoKCgoKUlIKCgoK4gpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/forminfo.go", "forminfo.cs", "ADp6ABZEAAgCgsqqtAAEHICigKSAooCigKSCpoKmgqaCqrSClIKCgqiSqJKClKqiqqKmgoKCgoKCggADHgALAoKClKaCgqaCgqiSgqS4koKkvLKCkoLcgpSmgoKCgoKCgoKCgpSCgpSm")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/input.go", "input.cs", "AA8cgqaCpoKCpoKCpoKClKaCgqampoKCpqamgoKUgpSmgoKUpoKClKaCgpSmooKCgpSUgpSUgpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/iter.go", "iter.cs", "AB1AkoKCgoKUgoKCgoKokoKCgoKUgoKCgoKssoKUtLS0pIKUgoKUgoKCgoKuwoKUqJKmgoKokgACEPKmooKCgoKUgoKClIKCpqKCgoKClIKCgpSCgqaigoKCkoKCgoKUgqaCqsKClJSCgoKClKaCqsKCgoKCgoKCgoKUgpSCgqiygpKCgIKCgoKCgpKCgpSSgMqCgqaCpKaCgoKmgoKUgoCCgpSkpIKkgoKUpIKCgoKUkoCCgoKCgoKSgoKmgoKUgsSCgpSCgoCCkoKCpIKmgpKClIKGooKmooKCgIKCpIKClICCgsimooKCgoKCqLKSgoKClIKCgpSCgpSCgoKCkoKCgpSCgIKSgoKkgqaChJKCgoKCgoKCgpSCgqa0goCCgqSCgIKSgoKklIKCpqKCgriCgg==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/normalize.go", "normalize.cs", "ABpakoKCgoKUgoKSqJKCgoKClIKCkqiSgoKCgpSSgoKCgIKklKaigoKCgpSCgoKClKaokoKCgoKUkoKCgoKUgoKCgpSmlIKAgqSUrNKCgpSCgqaCgoKCgoKUgoKCgIKCkoKCgqSCpqKClIKCqsKmgoKUlIKCgoKUkpSSpqKCkoKAlIKCpIKCgoKCgoKUpoKUlIKmgqaigoKUqqKqooKq0oKCgpSmqtKCgoKUpq7igoKCgoCCgoKCgqSCgpSUuJSkpIK2goKmgqaClIKClJSqooKqoqaCgoKUgriCgoKUgIKkgoKClM6irLKssqaCgoKUlIKCgoKUlIKEgoKCgpS4gIK2gpSqoqaCgoKClJKSlIKCkqaSlIKUgoKCgoCCpIKSlKas5IKClICUgqSCgqSAgqSCgoKClJSCgoKUlICCkoKCpICCtoKClKqigpSClKrCgoKUlIKUgoKCgoKCgqaUgoKClIKCpoSSgoKCgoI=", "120-136:1")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/readwriter.go", "readwriter.cs", "AA8k9JSUgoKUgoKCgqiCgpSCgIKkgqaokoKCgqauwoKCAAwcsoKCgoKClJSClIKChIKCgoKUgpSCggAFEKKCgoKC")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/tables15.0.0.go", "tables15.0.0.cs", "AAsoAMMCLAD/EuYqsoKUpKSilIKCgpSkkpSCgoKUgoKCgpSkkpSCgoKUgoKCgpSCgoKClLaqooKSlIKSlIKSlIKSlKyygpSkpKKUgoKClKSSlIKCgpSCgoKClKSSlIKCgpSCgoKClIKCgoKUtqqigpKUgpKUgpKUgpKU7IKokpSkggA2DADsA44IAFTWAQClAQYA3AWGDrKClKSkopSCgoKUpJKUgoKClIKCgoKUpJKUgoKClIKCgoKUgoKCgpS2qqKCkpSCkpSCkpSCkpSssoKUpKSilIKCgpSkkpSCgoKUgoKCgpSkkpSCgoKUgoKCgpSCgoKClLaqooKSlIKSlIKSlIKSlOyCqJKUpIIAZwwA2QfKEABc5gEAsgEG")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/transform.go", "transform.cs", "AAwcvAAJBIKCgIKCgqSCgoKCloKUprSClIKmmPSSgpSCgoKUgpaCgoCCgoKkgoKCgoKClA==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/text/unicode/norm/trie.go", "trie.cs", "ABMi6syCnsKCgoKCgoKCgpSClKY=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.text.unicode;

[GoPackage("norm", ImportPath = "vendor/golang.org/x/text/unicode/norm")]
public static partial class norm_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct formInfo {}
    internal partial struct input {}
    internal partial struct insertErr {}
    internal partial struct nfcTrie {}
    internal partial struct nfkcTrie {}
    [GoValueClone("rb")] internal partial struct normReader {}
    [GoValueClone("rb")] internal partial struct normWriter {}
    internal partial struct qcInfo {}
    [GoValueClone("rune", "@byte")] internal partial struct reorderBuffer {}
    internal partial struct sparseBlocks {}
    internal partial struct ssState {}
    internal partial struct streamSafe {}
    internal partial struct valueRange {}
    public partial struct Form {}
    [GoValueClone("rb", "buf")] public partial struct Iter {}
    public partial struct ΔProperties {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸtransform() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.transform_package));
    // </ImportInitializers>
}
