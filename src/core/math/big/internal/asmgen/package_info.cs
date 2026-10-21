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
using static go.math.big.@internal.asmgen_package;

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
[assembly: GoTypeAlias("AltCarry", "const:ΔAltCarry")]
[assembly: GoTypeAlias("Carry", "ΔCarry")]
[assembly: GoTypeAlias("Func", "ΔFunc")]
[assembly: GoTypeAlias("Pipe", "ΔPipe")]
[assembly: GoTypeAlias("Reg", "ΔReg")]
[assembly: GoTypeAlias("RegsUsed", "ΔRegsUsed")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<RegPtr, ΔReg>]
[assembly: GoImplicitConv<ΔReg, RegPtr>]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("math/big/internal/asmgen/386.go", "386.cs", "AAoSAC5eog==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/add.go", "add.cs", "AAsU0pSCgoKCpoKCgoKCpoKCgoCAkoCAyJSigoKUlKaClIKUgoI=", "31-31:1;32-32:2;38-44:3")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/amd64.go", "amd64.cs", "AAgOAC9gguai3IKClJSUlIKUgqaqtqailKSClKSClJSmlIKUpqKUgoKUlIKmgoKUlIKUgpQ=")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/arch.go", "arch.cs", "AIcBvAKiABUukKaQppCmkAAJEJCmkKaQ")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/arm.go", "arm.cs", "AAgOACpWgqaigriigriigriigg==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/arm64.go", "arm64.cs", "AAgOAC1cooKClIKCgpSCuKKCgpSCgoKUgriigoKUgoKClIK4ooKClIKCgpSC")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/asm.go", "asm.cs", "AB08woKCgpSCABcu4oKCgqiygpSClIKUqqIAAhgACQKokqiSqJKClKiSqLKCgpSCqLKAgoKClIKUgqSqwoKClIKUAAIQ8oKClIKUAAcUoqyyqJKowoKClKiyqMKCgpSqwoKClIKqoqqirNKClJSSlIKUgqa6soK8wqaCgoKClKrCqsKqwqrCgoKClIKClJSClLqSqsKClKrCgIKkgoKUqsKClICCpKrCgpSqwoCCpIKClKrCgpSAgqSospS0tAANCrKU+LS0tLSClJSClAANGoKCgoKUlIKClLqUgoKUgoKCxpSClIKUtNiCgoKClIKCgsamgoKCgpqylLSkgoKCgriCgoKmgoKClIIAA58BAAysAbKU+LS0tOqClJQADBiCgpSCgpS6lIKClIKCgsaUgqaCgoLGlO6CgoKCgoKUgoLKgoKCgoKCgoKCAAN3AAiIAdKCmKSnnNwACAiAgoKUlKTawoCCgpSSgtyk6sKClJSClJSkguzCmIKCgukAARSCrNKUtLS0tIK0gsqkqsKqwqiSqJI=")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/func.go", "func.cs", "ABw2soKClJKClMq4goKmgpKCgoKUlJS4goKCmIKkgpSUgoKCAAIZAAMigoKCgpSqoqiSgoKUgoKUgoKokqiSgoKUgoKCgoKCgpQ=")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/loong64.go", "loong64.cs", "/g==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/main.go", "main.cs", "ABMiABIcwoKCgoKCgoKC")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/mips.go", "mips.cs", "AAgOACZOgg==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/mips64.go", "mips64.cs", "AAgOACZOgg==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/mul.go", "mul.cs", "AAoQspSCgpTqwqaCgriCgoKCgoKCgrgABowCAIUBAoKCgpSSkpSCgpSCgpSkpMqCwoKCgoKCgoKCgpSClIKCgoKCgpSUpoLmooKSkqaSgoKmgoKCgoKqopKCkoKCgpSygoKUsoKCgoKUgoKCgpSmgtaigpKSuIKCwoKCgoKCgpSCgpSUgg==", "204-230:1;262-267:1;268-272:2;273-287:3;303-317:1")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/pipe.go", "pipe.cs", "AC5esoLKgIKkqqKClK7CgpSqoqqiAAIS4oKUgoKUgpSClAAHFgAIAoKCuIKClIK43IKCAAoUgoKCgpSClAAQIIKClIIACRaygoKygqaClIKU7IKCgpSCgpSUgoKUgpSCgtqCqJKCgoKUgrKmgpTsgoKClIKClJTIrLKC7IKUlIKUgpSCAAYSsoKClIKCgoKCAAMwABYCgoKUgpSCgoKU7oCClJSUpoLI2sKCgpSCgoLcgpSUyIKigpSClIKCgoKClMiCgpSClIKCgpTKkoKUgoKUggACLgAUAoKUgoKCgqaqoqqi6JKCpoKClJSmgpSUgpSCgoKCgpSCgoKClIKUgpSmlLaCtrak")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/ppc64.go", "ppc64.cs", "/g==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/riscv64.go", "riscv64.cs", "/g==")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/s390x.go", "s390x.cs", "AAgOAC1cooLKgoKClKaCgpTmgpSklKaCgpSClIKUgoKU")]
[assembly: go.GoPositionMap("math/big/internal/asmgen/shift.go", "shift.cs", "AA0SAA8YpoKmkoKClIKCgpSCgsiCgoKCgoKCgpTKgoKCgoKC+IKCgoKUgpSC7pKCgoKCgoKCgoLy3IKCgoKClKaCgoKCgoKClJSUgoKCpoKC", "65-77:1;96-123:2")]
// </GoSourcePositionMaps>

namespace go.math.big.@internal;

[GoPackage("asmgen")]
public static partial class asmgen_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Arch {}
    public partial struct Asm {}
    public partial struct Hint {}
    public partial struct Option {}
    public partial struct RegPtr {}
    public partial struct ΔCarry {}
    public partial struct ΔFunc {}
    public partial struct ΔPipe {}
    public partial struct ΔReg {}
    public partial struct ΔRegsUsed {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}
