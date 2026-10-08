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
using static go.@internal.cpu_package;

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
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b2048617341455320626f6f6c3b2048617341445820626f6f6c3b2048617341565820626f6f6c3b204861734156583220626f6f6c3b204861734156583531324620626f6f6c3b20486173415658353132425720626f6f6c3b20486173415658353132564c20626f6f6c3b20486173424d493120626f6f6c3b20486173424d493220626f6f6c3b2048617345524d5320626f6f6c3b204861734653524d20626f6f6c3b20486173464d4120626f6f6c3b204861734f53585341564520626f6f6c3b2048617350434c4d554c51445120626f6f6c3b20486173504f50434e5420626f6f6c3b2048617352445453435020626f6f6c3b2048617353484120626f6f6c3b204861735353453320626f6f6c3b20486173535353453320626f6f6c3b20486173535345343120626f6f6c3b20486173535345343220626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "X86ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b2048617341455320626f6f6c3b20486173504d554c4c20626f6f6c3b204861735348413120626f6f6c3b204861735348413220626f6f6c3b2048617353484135313220626f6f6c3b20486173435243333220626f6f6c3b2048617341544f4d49435320626f6f6c3b20486173435055494420626f6f6c3b2048617344495420626f6f6c3b2049734e656f766572736520626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "ARM64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b204861734441524e20626f6f6c3b2048617353435620626f6f6c3b204973504f5745523820626f6f6c3b204973504f5745523920626f6f6c3b204973504f574552313020626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "PPC64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b204861734c535820626f6f6c3b20486173435243333220626f6f6c3b204861734c414d43415320626f6f6c3b204861734c414d5f424820626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "Loong64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b204861734d534120626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "MIPS64Xᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b20486173564650763420626f6f6c3b20486173494449564120626f6f6c3b20486173563741746f6d69637320626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "ARMᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7465726e616c2f6370752e43616368654c696e655061643b204861735a4152434820626f6f6c3b204861735354464c4520626f6f6c3b204861734c4449535020626f6f6c3b2048617345494d4d20626f6f6c3b2048617344465020626f6f6c3b2048617345544633454820626f6f6c3b204861734d534120626f6f6c3b2048617341455320626f6f6c3b2048617341455343424320626f6f6c3b2048617341455343545220626f6f6c3b2048617341455347434d20626f6f6c3b20486173474841534820626f6f6c3b204861735348413120626f6f6c3b2048617353484132353620626f6f6c3b2048617353484135313220626f6f6c3b204861735348413320626f6f6c3b20486173565820626f6f6c3b2048617356584520626f6f6c3b204861734b44534120626f6f6c3b20486173454344534120626f6f6c3b20486173454444534120626f6f6c3b205f20696e7465726e616c2f6370752e43616368654c696e655061647d", "S390Xᴛ1")]
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
[assembly: go.GoPositionMap("internal/cpu/cpu.go", "cpu.cs", "AJ0BsAIADQKCABAu4oKCgoKClJSClIKCgpSUgpSkpIKmgoKClJaCgoKCqLaCgpaCgpYAAxDCgoKm")]
[assembly: go.GoPositionMap("internal/cpu/cpu_x86.go", "cpu_x86.cs", "AAwYpgAcSIIACBKCpu6m7qbOhIKWhISCgoKCgoK6zISClIKUzJaEgpaCgoKCgoKEgoKCloSChIKWgqaCrLKCloSygoKCgoKWgqiCgoKopqKC3A==")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("cpu")]
public static partial class cpu_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct option {}
    public partial struct ARM64ᴛ1 {}
    public partial struct ARMᴛ1 {}
    public partial struct CacheLinePad {}
    public partial struct Loong64ᴛ1 {}
    public partial struct MIPS64Xᴛ1 {}
    public partial struct PPC64ᴛ1 {}
    public partial struct S390Xᴛ1 {}
    public partial struct X86ᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    // </ImportInitializers>
}
