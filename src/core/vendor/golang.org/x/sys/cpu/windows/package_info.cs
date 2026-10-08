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
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.sys.cpu_package;

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
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b2048617341455320626f6f6c3b2048617341445820626f6f6c3b2048617341565820626f6f6c3b204861734156583220626f6f6c3b2048617341565835313220626f6f6c3b204861734156583531324620626f6f6c3b20486173415658353132434420626f6f6c3b20486173415658353132455220626f6f6c3b20486173415658353132504620626f6f6c3b20486173415658353132564c20626f6f6c3b20486173415658353132425720626f6f6c3b20486173415658353132445120626f6f6c3b2048617341565835313249464d4120626f6f6c3b2048617341565835313256424d4920626f6f6c3b2048617341565835313234564e4e495720626f6f6c3b2048617341565835313234464d41505320626f6f6c3b2048617341565835313256504f50434e54445120626f6f6c3b204861734156583531325650434c4d554c51445120626f6f6c3b20486173415658353132564e4e4920626f6f6c3b2048617341565835313247464e4920626f6f6c3b204861734156583531325641455320626f6f6c3b2048617341565835313256424d493220626f6f6c3b20486173415658353132424954414c4720626f6f6c3b204861734156583531324246313620626f6f6c3b20486173414d5854696c6520626f6f6c3b20486173414d58496e743820626f6f6c3b20486173414d584246313620626f6f6c3b20486173424d493120626f6f6c3b20486173424d493220626f6f6c3b204861734358313620626f6f6c3b2048617345524d5320626f6f6c3b20486173464d4120626f6f6c3b204861734f53585341564520626f6f6c3b2048617350434c4d554c51445120626f6f6c3b20486173504f50434e5420626f6f6c3b20486173524452414e4420626f6f6c3b2048617352445345454420626f6f6c3b204861735353453220626f6f6c3b204861735353453320626f6f6c3b20486173535353453320626f6f6c3b20486173535345343120626f6f6c3b20486173535345343220626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "X86ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b204861734441524e20626f6f6c3b2048617353435620626f6f6c3b204973504f5745523820626f6f6c3b204973504f5745523920626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "PPC64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b20486173465020626f6f6c3b204861734153494d4420626f6f6c3b204861734556545354524d20626f6f6c3b2048617341455320626f6f6c3b20486173504d554c4c20626f6f6c3b204861735348413120626f6f6c3b204861735348413220626f6f6c3b20486173435243333220626f6f6c3b2048617341544f4d49435320626f6f6c3b204861734650485020626f6f6c3b204861734153494d44485020626f6f6c3b20486173435055494420626f6f6c3b204861734153494d4452444d20626f6f6c3b204861734a5343565420626f6f6c3b2048617346434d4120626f6f6c3b204861734c5243504320626f6f6c3b204861734443504f5020626f6f6c3b204861735348413320626f6f6c3b20486173534d3320626f6f6c3b20486173534d3420626f6f6c3b204861734153494d44445020626f6f6c3b2048617353484135313220626f6f6c3b2048617353564520626f6f6c3b204861735356453220626f6f6c3b204861734153494d4446484d20626f6f6c3b2048617344495420626f6f6c3b2048617349384d4d20626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "ARM64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b20486173466173744d6973616c69676e656420626f6f6c3b204861734320626f6f6c3b204861735620626f6f6c3b204861735a626120626f6f6c3b204861735a626220626f6f6c3b204861735a627320626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "RISCV64ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b204861734d534120626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "MIPS64Xᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b2048617353575020626f6f6c3b2048617348414c4620626f6f6c3b204861735448554d4220626f6f6c3b20486173323642495420626f6f6c3b20486173464153544d554c20626f6f6c3b2048617346504120626f6f6c3b2048617356465020626f6f6c3b204861734544535020626f6f6c3b204861734a41564120626f6f6c3b2048617349574d4d585420626f6f6c3b204861734352554e434820626f6f6c3b204861735448554d42454520626f6f6c3b204861734e454f4e20626f6f6c3b20486173564650763320626f6f6c3b20486173564650763344313620626f6f6c3b20486173544c5320626f6f6c3b20486173564650763420626f6f6c3b20486173494449564120626f6f6c3b20486173494449565420626f6f6c3b2048617356465044333220626f6f6c3b204861734c50414520626f6f6c3b204861734556545354524d20626f6f6c3b2048617341455320626f6f6c3b20486173504d554c4c20626f6f6c3b204861735348413120626f6f6c3b204861735348413220626f6f6c3b20486173435243333220626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "ARMᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061643b204861735a4152434820626f6f6c3b204861735354464c4520626f6f6c3b204861734c4449535020626f6f6c3b2048617345494d4d20626f6f6c3b2048617344465020626f6f6c3b2048617345544633454820626f6f6c3b204861734d534120626f6f6c3b2048617341455320626f6f6c3b2048617341455343424320626f6f6c3b2048617341455343545220626f6f6c3b2048617341455347434d20626f6f6c3b20486173474841534820626f6f6c3b204861735348413120626f6f6c3b2048617353484132353620626f6f6c3b2048617353484135313220626f6f6c3b204861735348413320626f6f6c3b20486173565820626f6f6c3b2048617356584520626f6f6c3b205f20676f6c616e672e6f72672f782f7379732f6370752e43616368654c696e655061647d", "S390Xᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bigEndian, byteOrder>]
[assembly: GoImplement<littleEndian, byteOrder>]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/byteorder.go", "byteorder.cs", "ABUogoKmgoKogoKmgoKsogABFAACEKQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/cpu.go", "cpu.cs", "AO8BvgOCgoIAFCSCgoKCgoKClJSClIKCgpSUgpSkpIKmgoKClJaCgoKCqLaCgpaCgpaCgpY=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/cpu_gc_x86.go", "cpu_gc_x86.cs", "AAgWuA==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/cpu_other_x86.go", "cpu_other_x86.cs", "AAgSgg==")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/cpu_x86.go", "cpu_x86.cs", "AAwaggAuYISEhIKWgoSCgoKCgoKCgoKChKSClISUpqiEgpaCgoKCgoKEgoKCgoKCgoKCgoKCgoKCgoKCgoSCloKCpoI=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/parse.go", "parse.cs", "AAkcAAsEgoKCqIKCgoKCpoKClICCpICCpII=", "23-34:1")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/runtime_auxv.go", "runtime_auxv.cs", "AAsWgoKU")]
[assembly: go.GoPositionMap("vendor/golang.org/x/sys/cpu/runtime_auxv_go121.go", "runtime_auxv_go121.cs", "AAocpII=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.sys;

[GoPackage("cpu", ImportPath = "vendor/golang.org/x/sys/cpu")]
public static partial class cpu_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface byteOrder {}
    internal partial struct bigEndian {}
    internal partial struct littleEndian {}
    internal partial struct option {}
    public partial struct ARM64ᴛ1 {}
    public partial struct ARMᴛ1 {}
    public partial struct CacheLinePad {}
    public partial struct MIPS64Xᴛ1 {}
    public partial struct PPC64ᴛ1 {}
    public partial struct RISCV64ᴛ1 {}
    public partial struct S390Xᴛ1 {}
    public partial struct X86ᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}
