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
using static go.vendor.golang.org.x.net.dns.dnsmessage_package;

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
[assembly: GoTypeAlias("AAAAResource", "ΔAAAAResource")]
[assembly: GoTypeAlias("AResource", "ΔAResource")]
[assembly: GoTypeAlias("CNAMEResource", "ΔCNAMEResource")]
[assembly: GoTypeAlias("MXResource", "ΔMXResource")]
[assembly: GoTypeAlias("NSResource", "ΔNSResource")]
[assembly: GoTypeAlias("OPTResource", "ΔOPTResource")]
[assembly: GoTypeAlias("PTRResource", "ΔPTRResource")]
[assembly: GoTypeAlias("Question", "ΔQuestion")]
[assembly: GoTypeAlias("SOAResource", "ΔSOAResource")]
[assembly: GoTypeAlias("SRVResource", "ΔSRVResource")]
[assembly: GoTypeAlias("TXTResource", "ΔTXTResource")]
[assembly: GoTypeAlias("UnknownResource", "ΔUnknownResource")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<nestedError, error>(Pointer = true)]
[assembly: GoImplement<ΔAAAAResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔAResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔCNAMEResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔMXResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔNSResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔOPTResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔPTRResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔSOAResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔSRVResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔTXTResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<ΔUnknownResource, ResourceBody>(Pointer = true)]
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
[assembly: go.GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/message.go", "message.cs", "ACVaABEmkoCCpKiSgIKkAAoiAAcSkoCCpKiSgIKkzpIAESAACBSSgIKkqJKAgqSmgoLugoKClIKUpoKClIKCgoKUyoKCgoKIgpaCggAGEKaCppSCgoKClIKU5oKClK6IhIKCgoKCgoKCgoKCgoKCgoKCABFCkgAQIrKCgoKUgpSClIKUgpSClIKUqJIAHFQAESSClKSkpKSokoKCgoKCpoKCgoCCpICCpICCpICCpICCpICCpKaCABMoggANKJKClIKCgoKUgoKClICCpAAcPpKClIKCgIKkgqaCgpSClIKCgoKUpoKCgoKClIKCgpSCpoKCloCCpIKCgpSCgoKCgqaCgoKClIKCgpSAgqSCgoKUgqiSgIKkgoKClIKClIKClIKCqP6CgoKClIKUupKAgqSCgpSAgqSAgqSCgqiSgoCCkoLckqiSqOyCgpSCgoKClIKUAAMQwqiSgoCCkoLckqiSqOyCgpSCgoKClIKUAAMQwqiSgoCCkoLckqiSqOyCgpSCgoKClIKUAAMQwqiSgoCCkoIABRLCgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCqJKCgoCCpICCpICCpICCpICCpKiSqtiClIKUgpSCloKEgoKChIIACBSEgoKAgraCgoCCtoKCgIK2goKAgriokoSCgqKmgoKCoqaCgoKipoKCgqKmABxWAAoCgpSCgoKCggACHAALAqaCgpSClKiSgIKkgqiSgIKkgqiSgIKkgqiSgIKkgqaigoKUgqSCpIKkgqSClIKowoKUgpSCgpSAgqSCpoKClIKUqNKAgqSCgoKUgoCCpICCpICCpIKo0oCCpIKCgpSCgIKkgIKkgIKkgqjSgIKkgoKClIKAgqSAgqSAgqSCqNKAgqSCgoKUgoCCpICCpICCpIKo0oCCpIKCgpSCgIKkgIKkgIKkgqjCgIKkgoKClIKAgqSAgqSAgqSCqNKAgqSCgoKUgoCCpICCpICCpIKo0oCCpIKCgpSCgIKkgIKkgIKkgqjSgIKkgoKClIKAgqSAgqSAgqSCqMKAgqSCgoKUgoCCpICCpICCpIKowoCCpIKCgpSCgIKkgIKkgIKkgqiSgpSUggAYPJIAAhbygoCCpIKCgoKCpoKCgoCCpICCpICCpICCpICCpAACEuKCgqiChAALHrKCgoKCgpSokqyykpSmgoKClICCpICCpICCpIKClICCpKiSpoKClKaCgpSokqaCgqaCqJKmgoKmgqiSAAcSgoKUgqaCgpSokoKClIKEpoKClIKCgpSokqaCgoKUggALGpKCgpSCqJKCgpSs0qiSAAIU8oSCqIKogpaWlLiCqIKWhIKWgsyCgKbKgoKmlLiopLqGpoSCgoKUgoKUpJSCgpqigqiCgrSSlIKCgpaAkqS25oKUgpSCgpSmuISCgoKUgoKUlJaCksyGtgAJCAAJFpKCgpSCqJIADAyiqpSCgoKkgoKCpIKCgqSCgoKkgoKCpIKCgqSCgoKkgoKCpIKCgqSCgoKkgoKCpIKUAAcQgqiSqJKmgoKAgqQACBKCqJKCgoKClKiSqoKCgpSCgIKkAAcQgqiSqJKmgoKAgqQABxCCqJKokqaCgoCCpAAQJIKokoKCgpSCgpSCgoKCqJIAAhSCgoKClIKAgqSCgpSCgpSCgpSCgpSCgpQABxCCqJKCgoKCgqbYkoKClIKClKaCgoKCgoCCtoKUgpQAChaCqJKCgoKCgoKUqJKugoKClIKClIKClIKAgqQABxCCqJKokqiCgoCCpAAHEIKokqqSpoKCgIKkABMqkqqCpoKCgoKClNiSgoKUgqKUpoKCgoKCgoKUgoKClIKClIKUAAgSgqiSqJKqgriAgqQ=", "", "", "335=printUint16/1/1/1,printBool/1/7/2,GoString/1/2/3,printBool/2/7/4,printBool/3/7/5,printBool/4/7/6,printBool/5/7/7,printBool/6/7/8,printBool/7/7/9,GoString/2/2/10;468=GoString/1/2/1,GoString/2/2/2;1836=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3,printUint32/1/1/4,printUint16/1/1/5;2345=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3;2490=printUint16/1/1/1,GoString/1/1/2;2603=GoString/1/2/1,GoString/2/2/2,printUint32/1/5/3,printUint32/2/5/4,printUint32/3/5/5,printUint32/4/5/6,printUint32/5/5/7;2726=printUint16/1/3/1,printUint16/2/3/2,printUint16/3/3/3,GoString/1/1/4;2767=printByteSlice/1/1/1;2791=printByteSlice/1/1/1;2828=printUint16/1/1/1,printByteSlice/1/1/2;2904=GoString/1/1/1,printByteSlice/1/1/2")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.dns;

[GoPackage("dnsmessage", ImportPath = "vendor/golang.org/x/net/dns/dnsmessage")]
public static partial class dnsmessage_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct nestedError {}
    internal partial struct section {}
    internal partial struct Δheader {}
    public partial interface ResourceBody {}
    public partial struct Builder {}
    public partial struct Class {}
    public partial struct Header {}
    public partial struct Message {}
    [GoValueClone("Data")] public partial struct Name {}
    public partial struct OpCode {}
    public partial struct Option {}
    public partial struct Parser {}
    public partial struct RCode {}
    [GoValueClone("Header")] public partial struct Resource {}
    [GoValueClone("Name")] public partial struct ResourceHeader {}
    public partial struct Type {}
    [GoValueClone("AAAA")] public partial struct ΔAAAAResource {}
    [GoValueClone("A")] public partial struct ΔAResource {}
    [GoValueClone("CNAME")] public partial struct ΔCNAMEResource {}
    [GoValueClone("MX")] public partial struct ΔMXResource {}
    [GoValueClone("NS")] public partial struct ΔNSResource {}
    public partial struct ΔOPTResource {}
    [GoValueClone("PTR")] public partial struct ΔPTRResource {}
    [GoValueClone("Name")] public partial struct ΔQuestion {}
    [GoValueClone("NS", "MBox")] public partial struct ΔSOAResource {}
    [GoValueClone("Target")] public partial struct ΔSRVResource {}
    public partial struct ΔTXTResource {}
    public partial struct ΔUnknownResource {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    // </ImportInitializers>
}
