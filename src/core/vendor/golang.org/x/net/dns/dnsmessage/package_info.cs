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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<AAAAResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<AResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<CNAMEResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<HTTPSResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<MXResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<NSResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<OPTResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<PTRResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<SOAResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<SRVResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<SVCBResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<TXTResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<UnknownResource, ResourceBody>(Pointer = true)]
[assembly: GoImplement<nestedError, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Builder))]
[assembly: GoFieldAccess(typeof(SVCParam))]
[assembly: GoFieldAccess(typeof(header))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/message.go", "message.cs", "ADt4ABUqkoKUqJKClAAQIgAJEpKClKiSgpQACQ6SAA8gAAoUkoKUqJKClMaCgu6CgoKUgpSmooKUgoKCgpTaooKCgoiCpoKCAAYQpoKmlIKCgoKUgpQACAaCgpTeuJSCgoKCgoKCgoKCgoKCgoKCgoKCAB1CkgAUIrKCgoKUgpSClIKUgpSClIKUqJIAH1QAEySClKSkpJSokoKCgoKCpoKCgoKUgpSClIKUgpSClKaCABcoggARKJKClIKCgoKUgoKClIKUACA+koKUgoKClIKmgoKUgpSCgoKClKaCgoKCgpSCgoKUgqaCgqaClIKCgpSCgoKCgqaCgoKClIKCgpSClIKCgpSCqJKClIKCgpSCgpSCgpSCgqj+goKCgpSClLqSgpSCgpSClIKUgoKokoKCgoLMkqiSqOyCgpSCgoKClIKUAAMQwqiSgoKCgsySqJKo7IKClIKCgoKUgpQAAxDCqJKCgoKCzJKokqjsgoKUgoKCgpSClAADEMKokoKCgoIABBLCgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCyJKCgoKUgpSClIKUgpSokqrYgpSClIKUgqaClIKCgpSCAAkUlIKCgqaCgoKmgoKCpoKCgriokoSCgqKmgoKCoqaCgoKipoKCgqKmAB5WAAoCgpSCgoKCggAEHAALAqaCgpSClKiSgpSCqJKClIKokoKUgqiSgpSCpqKCgpSCtIK0grSCpIKUgqjCgpSClIKClIKUgqaCgpSClKjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqMKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjCgpSCgoKUgoKUgpSClIKowoKUgoKClIKClIKUgpSCqJKClJSCABw8kgACFvKCgpSCgoKCgqaCgoKClIKUgpSClIKUAAIS4oKCuIKUAA0esoKCgoKClKiSrLKClMaCgoKUgpSClIKUgoKUgpSokqaCgpSmgoKUqJKmgoKmgqiSpoKCpoKokgAHEoKClIKmgoKUqJKCgpSClKaCgpSCgoKUqJKmgoKClIIACxqSgoKUgqiSgoKUzNKokgACFPKUgriCuIKmppS4griCppSCpoLcgqbKgoKmlLioxMqWtpSCgoKUgoKUlJSCgsqCgriCgrSClIKCgqaClMbGgpSClIKClMa4lIKCgpSCgpSUpoKC/KbG2AALFpKCgpSCqJIAGgyiqpSCgoK0goKCtIKCgrSCgoK0goKCtIKCgrSCgoK0goKCtIKCgrSCgoK0goKCtIKCgrSCgoKkgpQACRCCqJKoksaCgoKUAAoSgqiSgoKCgpSoksqCgoKUgoKUAAkQgqiSqJLGgoKClAAJEIKokqiSxoKCgpQAEiSCqJKCgoKUgoKUgoKCgqiSAAQUgoKCgpSCgpSCgpSCgpSCgpSCgpSCgpQACRCCqJKCgoKCgqbokoKClIKClMaCgoKCgoKmgpSClAAMFoKokoKCgoKCgpSoks6CgoKUgoKUgoKUgoKUAAkQgqiSqJLIgoKClAAJEIKokqqSxoKCgpQAFSqSyoKmgoKCgoKU6JKCgpSCopTGgoKCgoKCgpSCgoKUgoKUgpQAChKCqJKoksqCuIKU", "", "", "397=printUint16/1/1/1,printBool/1/7/2,GoString/1/2/3,printBool/2/7/4,printBool/3/7/5,printBool/4/7/6,printBool/5/7/7,printBool/6/7/8,printBool/7/7/9,GoString/2/2/10;526=GoString/1/2/1,GoString/2/2/2;1778=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3,printUint32/1/1/4,printUint16/1/1/5;2294=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3;2481=printUint16/1/1/1,GoString/1/1/2;2600=GoString/1/2/1,GoString/2/2/2,printUint32/1/5/3,printUint32/2/5/4,printUint32/3/5/5,printUint32/4/5/6,printUint32/5/5/7;2728=printUint16/1/3/1,printUint16/2/3/2,printUint16/3/3/3,GoString/1/1/4;2771=printByteSlice/1/1/1;2797=printByteSlice/1/1/1;2836=printUint16/1/1/1,printByteSlice/1/1/2;2919=GoString/1/1/1,printByteSlice/1/1/2")]
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/svcb.go", "svcb.cs", "ABMkgqiSiIKCgqaCAA0UgqiSyJKCgpSCpqqigoKClKaCgqaqooKCgpSCpgAMFJIAEywADhySgpSokoKUxoKCyoKClIKCgpSClIKClMaUgoKUgoKmgsqCgoKCgpKClKaUgpSClIKClIK4poKCgoKSgpSCgpSClIKUgoKmyJKClIKClIKCgq7CrsKCgpTI0oKUgoKUgoKUgpSClIKowoKosoI=", "", "", "26=printUint16/1/1/1,GoString/1/1/2;118=GoString/1/1/1,printByteSlice/1/1/2")]
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
    internal partial struct header {}
    internal partial struct nestedError {}
    internal partial struct section {}
    public partial interface ResourceBody {}
    [GoValueClone("AAAA")] public partial struct AAAAResource {}
    [GoValueClone("A")] public partial struct AResource {}
    public partial struct Builder {}
    [GoValueClone("CNAME")] public partial struct CNAMEResource {}
    public partial struct Class {}
    public partial struct HTTPSResource {}
    public partial struct Header {}
    [GoValueClone("MX")] public partial struct MXResource {}
    public partial struct Message {}
    [GoValueClone("NS")] public partial struct NSResource {}
    [GoValueClone("Data")] public partial struct Name {}
    public partial struct OPTResource {}
    public partial struct OpCode {}
    public partial struct Option {}
    [GoValueClone("PTR")] public partial struct PTRResource {}
    public partial struct Parser {}
    [GoValueClone("Name")] public partial struct Question {}
    public partial struct RCode {}
    [GoValueClone("Header")] public partial struct Resource {}
    [GoValueClone("Name")] public partial struct ResourceHeader {}
    [GoValueClone("NS", "MBox")] public partial struct SOAResource {}
    [GoValueClone("Target")] public partial struct SRVResource {}
    [GoValueClone("Target")] public partial struct SVCBResource {}
    public partial struct SVCParam {}
    public partial struct SVCParamKey {}
    public partial struct TXTResource {}
    public partial struct Type {}
    public partial struct UnknownResource {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(dnsmessage_package.AAAAResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛAAAAResource;
[GoMethodHost(typeof(dnsmessage_package.AResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛAResource;
[GoMethodHost(typeof(dnsmessage_package.Builder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛBuilder;
[GoMethodHost(typeof(dnsmessage_package.CNAMEResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛCNAMEResource;
[GoMethodHost(typeof(dnsmessage_package.Class)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛClass;
[GoMethodHost(typeof(dnsmessage_package.HTTPSResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛHTTPSResource;
[GoMethodHost(typeof(dnsmessage_package.Header)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛHeader;
[GoMethodHost(typeof(dnsmessage_package.MXResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛMXResource;
[GoMethodHost(typeof(dnsmessage_package.Message)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛMessage;
[GoMethodHost(typeof(dnsmessage_package.NSResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛNSResource;
[GoMethodHost(typeof(dnsmessage_package.Name)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛName;
[GoMethodHost(typeof(dnsmessage_package.OPTResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛOPTResource;
[GoMethodHost(typeof(dnsmessage_package.OpCode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛOpCode;
[GoMethodHost(typeof(dnsmessage_package.Option)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛOption;
[GoMethodHost(typeof(dnsmessage_package.PTRResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛPTRResource;
[GoMethodHost(typeof(dnsmessage_package.Parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛParser;
[GoMethodHost(typeof(dnsmessage_package.Question)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛQuestion;
[GoMethodHost(typeof(dnsmessage_package.RCode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛRCode;
[GoMethodHost(typeof(dnsmessage_package.Resource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛResource;
[GoMethodHost(typeof(dnsmessage_package.ResourceHeader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛResourceHeader;
[GoMethodHost(typeof(dnsmessage_package.SOAResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛSOAResource;
[GoMethodHost(typeof(dnsmessage_package.SRVResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛSRVResource;
[GoMethodHost(typeof(dnsmessage_package.SVCBResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCBResource;
[GoMethodHost(typeof(dnsmessage_package.SVCParam)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCParam;
[GoMethodHost(typeof(dnsmessage_package.SVCParamKey)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCParamKey;
[GoMethodHost(typeof(dnsmessage_package.TXTResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛTXTResource;
[GoMethodHost(typeof(dnsmessage_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛType;
[GoMethodHost(typeof(dnsmessage_package.UnknownResource)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛUnknownResource;
[GoMethodHost(typeof(dnsmessage_package.header)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛheader;
[GoMethodHost(typeof(dnsmessage_package.nestedError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class dnsmessageᴛnestedError;
// </MethodHolders>
