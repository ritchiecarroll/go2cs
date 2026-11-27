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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/message.go", "message.cs", "ADl4ABMqkoKUqJKClAAOIgAHEpKClKiSgpTekgANIAAIFJKClKiSgpSmgoLugoKClIKUpqKClIKCgoKU2qKCgoKIgqaCggAGEKaCppSCgoKClIKUAAgGgoKU3riUgoKCgoKCgoKCgoKCgoKCgoKCggAbQpIAESKygoKClIKUgpSClIKUgpSClKiSAB1UABEkgpSkpKSUqJKCgoKCgqaCgoKClIKUgpSClIKUgpSmggATKIIADSiSgpSCgoKClIKCgpSClAAcPpKClIKCgpSCpoKClIKUgpKCgpSmgoKCooKUgoKClIKmgoKmgpSCgoKUgoKCgoKmgoKCgpSCgoKUgpSCgoKUgqiSgpSCgoKUgoKUgoKUgoKo/oKCgoKUgpS6koKUgoKUgpSClIKCqJKCgoKCzJKokqjsgoKUgoKCgpSClAADEMKokoKCgoLMkqiSqOyCgpSCgoKClIKUAAMQwqiSgoKCgsySqJKo7IKClIKCgoKUgpQAAxDCqJKCgoKCAAQSwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgqiSgoKClIKUgpSClIKUqJKq2IKUgpSClIKmgpSCgoKUggAJFJSCgoKmgoKCpoKCgqaCgoK4qJKEgoKipoKCgqKmgoKCoqaCgoKipgAcVgAKAoKUgoKCgoIAAhwACwKmgoKUgpSokoKUgqiSgpSCqJKClIKokoKUgqaigoKUgrSCtIK0gqSClIKowoKUgpSCgpSClIKmgoKUgpSo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjCgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKowoKUgoKClIKClIKUgpSCqMKClIKCgpSCgpSClIKUgqiSgpSUggAYPJIAAhbygoKUgoKCgoKmgoKCgpSClIKUgpSClAACEuKCgriClAAJHrKCgoKCgpSokqyygpSmgoKClIKUgpSClIKClIKUqJKmgoKUpoKClKiSpoKCpoKokqaCgqaCqJIABxKCgpSCpoKClKiSgoKUgpSmgoKUgoKClKiSpoKCgpSCAAsakoKClIKokoKClKzSqJIAAhTylIK4griCpqaUuIK4gqaUgqaC3IKmyoKCppS4qMTKlraUgoKClIKClJSUgoLKgoK4goK0gpSCgoKmgpTGxoKUgpSCgpSmuJSCgoKUgoKUlKaCgvymxtgACRaSgoKUgqiSABgMoqqUgpKCtIKSgrSCkoK0gpKCtIKSgrSCkoK0goKCtIKSgrSCkoK0gpKCtIKigrSCgoK0goKCpIKUAAcQgqiSqJKmgoKClAAIEoKokoKCgoKUqJKqgoKClIKClAAHEIKokqiSpoKCgpQABxCCqJKokqaCgoKUABAkgqiSgoKClIKClIKCgoKokgACFIKCgoKUgoKUgoKUgoKUgoKUgoKUgoKUAAcQgqiSgoKCgoKm6JKCgpSCgpSmgoKCgoKCpoKUgpQAChaCqJKCgoKCgoKUqJKugoKClIKClIKClIKClAAHEIKokqiSqIKCgpQABxCCqJKqkqaCgoKUABMqkqqCpoKCgoKClOiSgoKUgqKUpoKCgoKCgoKUgoKClIKClIKUAAgSgqiSqJKqgriClA==", "", "", "374=printUint16/1/1/1,printBool/1/7/2,GoString/1/2/3,printBool/2/7/4,printBool/3/7/5,printBool/4/7/6,printBool/5/7/7,printBool/6/7/8,printBool/7/7/9,GoString/2/2/10;495=GoString/1/2/1,GoString/2/2/2;1732=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3,printUint32/1/1/4,printUint16/1/1/5;2236=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3;2426=printUint16/1/1/1,GoString/1/1/2;2533=GoString/1/2/1,GoString/2/2/2,printUint32/1/5/3,printUint32/2/5/4,printUint32/3/5/5,printUint32/4/5/6,printUint32/5/5/7;2653=printUint16/1/3/1,printUint16/2/3/2,printUint16/3/3/3,GoString/1/1/4;2692=printByteSlice/1/1/1;2714=printByteSlice/1/1/1;2749=printUint16/1/1/1,printByteSlice/1/1/2;2826=GoString/1/1/1,printByteSlice/1/1/2")]
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/svcb.go", "svcb.cs", "ABAkgqiSiIKCgqaCAAkUgqiSqJKCgpSCpqqigoKClKaCgqaqooKCgpSCpgAIFJIAESwADBySgpSokoKUpoKCyoKClIKCgpSClIKClKaUgoKUgoKmgsqCgoKCgpKClKaUgpSClIKClIK4poKCgoKSgpSCgpSClIKUgoKmqJKClIKClIKCgq7CrsKCgpSo0oKUgoKUgoKUgpSClIKowoKowoI=", "", "", "23=printUint16/1/1/1,GoString/1/1/2;105=GoString/1/1/1,printByteSlice/1/1/2")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.dns;

using static go.vendor.golang.org.x.net.dns.dnsmessage_package;

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
    [GoValueClone("SVCBResource")] public partial struct HTTPSResource {}
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
[GoMethodHost(typeof(dnsmessage_package.AAAAResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛAAAAResource;
[GoMethodHost(typeof(dnsmessage_package.AResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛAResource;
[GoMethodHost(typeof(dnsmessage_package.Builder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛBuilder;
[GoMethodHost(typeof(dnsmessage_package.CNAMEResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛCNAMEResource;
[GoMethodHost(typeof(dnsmessage_package.Class)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛClass;
[GoMethodHost(typeof(dnsmessage_package.HTTPSResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛHTTPSResource;
[GoMethodHost(typeof(dnsmessage_package.Header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛHeader;
[GoMethodHost(typeof(dnsmessage_package.MXResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛMXResource;
[GoMethodHost(typeof(dnsmessage_package.Message)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛMessage;
[GoMethodHost(typeof(dnsmessage_package.NSResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛNSResource;
[GoMethodHost(typeof(dnsmessage_package.Name)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛName;
[GoMethodHost(typeof(dnsmessage_package.OPTResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛOPTResource;
[GoMethodHost(typeof(dnsmessage_package.OpCode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛOpCode;
[GoMethodHost(typeof(dnsmessage_package.Option)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛOption;
[GoMethodHost(typeof(dnsmessage_package.PTRResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛPTRResource;
[GoMethodHost(typeof(dnsmessage_package.Parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛParser;
[GoMethodHost(typeof(dnsmessage_package.Question)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛQuestion;
[GoMethodHost(typeof(dnsmessage_package.RCode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛRCode;
[GoMethodHost(typeof(dnsmessage_package.Resource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛResource;
[GoMethodHost(typeof(dnsmessage_package.ResourceHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛResourceHeader;
[GoMethodHost(typeof(dnsmessage_package.SOAResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛSOAResource;
[GoMethodHost(typeof(dnsmessage_package.SRVResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛSRVResource;
[GoMethodHost(typeof(dnsmessage_package.SVCBResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCBResource;
[GoMethodHost(typeof(dnsmessage_package.SVCParam)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCParam;
[GoMethodHost(typeof(dnsmessage_package.SVCParamKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛSVCParamKey;
[GoMethodHost(typeof(dnsmessage_package.TXTResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛTXTResource;
[GoMethodHost(typeof(dnsmessage_package.Type)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛType;
[GoMethodHost(typeof(dnsmessage_package.UnknownResource)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛUnknownResource;
[GoMethodHost(typeof(dnsmessage_package.header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛheader;
[GoMethodHost(typeof(dnsmessage_package.nestedError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class dnsmessageᴛnestedError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class dnsmessageᴛAAAAResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref AAAAResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref AAAAResource r) => dnsmessage_package.GoString⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref AAAAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
}
partial class dnsmessageᴛAResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref AResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref AResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref AResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛBuilder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void EnableCompression(this ref Builder b) => dnsmessage_package.EnableCompression⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error startCheck(this ref Builder b, section s) => dnsmessage_package.startCheck⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartQuestions(this ref Builder b) => dnsmessage_package.StartQuestions⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartAnswers(this ref Builder b) => dnsmessage_package.StartAnswers⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartAuthorities(this ref Builder b) => dnsmessage_package.StartAuthorities⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartAdditionals(this ref Builder b) => dnsmessage_package.StartAdditionals⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error incrementSectionCount(this ж<Builder> Ꮡb) => dnsmessage_package.incrementSectionCount⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Question(this ж<Builder> Ꮡb, Question q) => dnsmessage_package.Question⁔(Ꮡb, q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkResourceSection(this ref Builder b) => dnsmessage_package.checkResourceSection⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CNAMEResource(this ж<Builder> Ꮡb, ResourceHeader h, CNAMEResource r) => dnsmessage_package.CNAMEResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error MXResource(this ж<Builder> Ꮡb, ResourceHeader h, MXResource r) => dnsmessage_package.MXResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error NSResource(this ж<Builder> Ꮡb, ResourceHeader h, NSResource r) => dnsmessage_package.NSResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error PTRResource(this ж<Builder> Ꮡb, ResourceHeader h, PTRResource r) => dnsmessage_package.PTRResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SOAResource(this ж<Builder> Ꮡb, ResourceHeader h, SOAResource r) => dnsmessage_package.SOAResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error TXTResource(this ж<Builder> Ꮡb, ResourceHeader h, TXTResource r) => dnsmessage_package.TXTResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SRVResource(this ж<Builder> Ꮡb, ResourceHeader h, SRVResource r) => dnsmessage_package.SRVResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AResource(this ж<Builder> Ꮡb, ResourceHeader h, AResource r) => dnsmessage_package.AResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AAAAResource(this ж<Builder> Ꮡb, ResourceHeader h, AAAAResource r) => dnsmessage_package.AAAAResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error OPTResource(this ж<Builder> Ꮡb, ResourceHeader h, OPTResource r) => dnsmessage_package.OPTResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnknownResource(this ж<Builder> Ꮡb, ResourceHeader h, UnknownResource r) => dnsmessage_package.UnknownResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Finish(this ref Builder b) => dnsmessage_package.Finish⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error genericSVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, SVCBResource r) => dnsmessage_package.genericSVCBResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, SVCBResource r) => dnsmessage_package.SVCBResource⁔(Ꮡb, h, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error HTTPSResource(this ж<Builder> Ꮡb, ResourceHeader h, HTTPSResource r) => dnsmessage_package.HTTPSResource⁔(Ꮡb, h, r);
}
partial class dnsmessageᴛCNAMEResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref CNAMEResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref CNAMEResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref CNAMEResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛClass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Class c) => dnsmessage_package.String⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Class c) => dnsmessage_package.GoString⁔(c);
}
partial class dnsmessageᴛHTTPSResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref HTTPSResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref HTTPSResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint16 id, uint16 bits) pack(this ref Header m) => dnsmessage_package.pack⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Header m) => dnsmessage_package.GoString⁔(ref m);
}
partial class dnsmessageᴛMXResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref MXResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref MXResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref MXResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛMessage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unpack(this ref Message m, slice<byte> msg) => dnsmessage_package.Unpack⁔(ref m, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Pack(this ref Message m) => dnsmessage_package.Pack⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendPack(this ref Message m, slice<byte> b) => dnsmessage_package.AppendPack⁔(ref m, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Message m) => dnsmessage_package.GoString⁔(ref m);
}
partial class dnsmessageᴛNSResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref NSResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref NSResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref NSResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Name n) => dnsmessage_package.String⁔(n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Name n) => dnsmessage_package.GoString⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Name n, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref n, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) unpack(this ref Name n, slice<byte> msg, nint off) => dnsmessage_package.unpack⁔(ref n, msg, off);
}
partial class dnsmessageᴛOPTResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref OPTResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref OPTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref OPTResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛOpCode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this OpCode o) => dnsmessage_package.GoString⁔(o);
}
partial class dnsmessageᴛOption {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Option o) => dnsmessage_package.GoString⁔(ref o);
}
partial class dnsmessageᴛPTRResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref PTRResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref PTRResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref PTRResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛParser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Header, error) Start(this ref Parser p, slice<byte> msg) => dnsmessage_package.Start⁔(ref p, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkAdvance(this ref Parser p, section sec) => dnsmessage_package.checkAdvance⁔(ref p, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Resource, error) resource(this ref Parser p, section sec) => dnsmessage_package.resource⁔(ref p, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ResourceHeader, error) resourceHeader(this ref Parser p, section sec) => dnsmessage_package.resourceHeader⁔(ref p, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error skipResource(this ref Parser p, section sec) => dnsmessage_package.skipResource⁔(ref p, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Question, error) Question(this ref Parser p) => dnsmessage_package.Question⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Question>, error) AllQuestions(this ref Parser p) => dnsmessage_package.AllQuestions⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipQuestion(this ref Parser p) => dnsmessage_package.SkipQuestion⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAllQuestions(this ref Parser p) => dnsmessage_package.SkipAllQuestions⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ResourceHeader, error) AnswerHeader(this ref Parser p) => dnsmessage_package.AnswerHeader⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Resource, error) Answer(this ref Parser p) => dnsmessage_package.Answer⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Resource>, error) AllAnswers(this ref Parser p) => dnsmessage_package.AllAnswers⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAnswer(this ref Parser p) => dnsmessage_package.SkipAnswer⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAllAnswers(this ref Parser p) => dnsmessage_package.SkipAllAnswers⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ResourceHeader, error) AuthorityHeader(this ref Parser p) => dnsmessage_package.AuthorityHeader⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Resource, error) Authority(this ref Parser p) => dnsmessage_package.Authority⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Resource>, error) AllAuthorities(this ref Parser p) => dnsmessage_package.AllAuthorities⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAuthority(this ref Parser p) => dnsmessage_package.SkipAuthority⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAllAuthorities(this ref Parser p) => dnsmessage_package.SkipAllAuthorities⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ResourceHeader, error) AdditionalHeader(this ref Parser p) => dnsmessage_package.AdditionalHeader⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Resource, error) Additional(this ref Parser p) => dnsmessage_package.Additional⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<Resource>, error) AllAdditionals(this ref Parser p) => dnsmessage_package.AllAdditionals⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAdditional(this ref Parser p) => dnsmessage_package.SkipAdditional⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SkipAllAdditionals(this ref Parser p) => dnsmessage_package.SkipAllAdditionals⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (CNAMEResource, error) CNAMEResource(this ref Parser p) => dnsmessage_package.CNAMEResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (MXResource, error) MXResource(this ref Parser p) => dnsmessage_package.MXResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (NSResource, error) NSResource(this ref Parser p) => dnsmessage_package.NSResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (PTRResource, error) PTRResource(this ref Parser p) => dnsmessage_package.PTRResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (SOAResource, error) SOAResource(this ref Parser p) => dnsmessage_package.SOAResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (TXTResource, error) TXTResource(this ref Parser p) => dnsmessage_package.TXTResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (SRVResource, error) SRVResource(this ref Parser p) => dnsmessage_package.SRVResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (AResource, error) AResource(this ref Parser p) => dnsmessage_package.AResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (AAAAResource, error) AAAAResource(this ref Parser p) => dnsmessage_package.AAAAResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (OPTResource, error) OPTResource(this ref Parser p) => dnsmessage_package.OPTResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (UnknownResource, error) UnknownResource(this ref Parser p) => dnsmessage_package.UnknownResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (SVCBResource, error) genericSVCBResource(this ref Parser p, Type svcbType) => dnsmessage_package.genericSVCBResource⁔(ref p, svcbType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (SVCBResource, error) SVCBResource(this ref Parser p) => dnsmessage_package.SVCBResource⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (HTTPSResource, error) HTTPSResource(this ref Parser p) => dnsmessage_package.HTTPSResource⁔(ref p);
}
partial class dnsmessageᴛQuestion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Question q, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref q, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Question q) => dnsmessage_package.GoString⁔(ref q);
}
partial class dnsmessageᴛRCode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this RCode r) => dnsmessage_package.String⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this RCode r) => dnsmessage_package.GoString⁔(r);
}
partial class dnsmessageᴛResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref Resource r) => dnsmessage_package.GoString⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Resource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
}
partial class dnsmessageᴛResourceHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref ResourceHeader h) => dnsmessage_package.GoString⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> msg, nint lenOff, error err) pack(this ref ResourceHeader h, slice<byte> oldMsg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref h, oldMsg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) unpack(this ref ResourceHeader h, slice<byte> msg, nint off) => dnsmessage_package.unpack⁔(ref h, msg, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error fixLen(this ref ResourceHeader h, slice<byte> msg, nint lenOff, nint preLen) => dnsmessage_package.fixLen⁔(ref h, msg, lenOff, preLen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetEDNS0(this ref ResourceHeader h, nint udpPayloadLen, RCode extRCode, bool dnssecOK) => dnsmessage_package.SetEDNS0⁔(ref h, udpPayloadLen, extRCode, dnssecOK);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool DNSSECAllowed(this ref ResourceHeader h) => dnsmessage_package.DNSSECAllowed⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RCode ExtendedRCode(this ref ResourceHeader h, RCode rcode) => dnsmessage_package.ExtendedRCode⁔(ref h, rcode);
}
partial class dnsmessageᴛSOAResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref SOAResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SOAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref SOAResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛSRVResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref SRVResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SRVResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref SRVResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛSVCBResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref SVCBResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref SVCBResource r) => dnsmessage_package.GoString⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> value, bool ok) GetParam(this ref SVCBResource r, SVCParamKey key) => dnsmessage_package.GetParam⁔(ref r, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetParam(this ref SVCBResource r, SVCParamKey key, slice<byte> value) => dnsmessage_package.SetParam⁔(ref r, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool DeleteParam(this ref SVCBResource r, SVCParamKey key) => dnsmessage_package.DeleteParam⁔(ref r, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SVCBResource r, slice<byte> msg, map<@string, uint16> _Δp2, nint _Δp3) => dnsmessage_package.pack⁔(ref r, msg, _Δp2, _Δp3);
}
partial class dnsmessageᴛSVCParam {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this SVCParam p) => dnsmessage_package.GoString⁔(p);
}
partial class dnsmessageᴛSVCParamKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this SVCParamKey k) => dnsmessage_package.String⁔(k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this SVCParamKey k) => dnsmessage_package.GoString⁔(k);
}
partial class dnsmessageᴛTXTResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref TXTResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref TXTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref TXTResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Type t) => dnsmessage_package.String⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Type t) => dnsmessage_package.GoString⁔(t);
}
partial class dnsmessageᴛUnknownResource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type realType(this ref UnknownResource r) => dnsmessage_package.realType⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref UnknownResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.pack⁔(ref r, msg, compression, compressionOff);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this ref UnknownResource r) => dnsmessage_package.GoString⁔(ref r);
}
partial class dnsmessageᴛheader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 count(this ref header h, section sec) => dnsmessage_package.count⁔(ref h, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> pack(this ref header h, slice<byte> msg) => dnsmessage_package.pack⁔(ref h, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) unpack(this ref header h, slice<byte> msg, nint off) => dnsmessage_package.unpack⁔(ref h, msg, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header header(this ref header h) => dnsmessage_package.header⁔(ref h);
}
partial class dnsmessageᴛnestedError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref nestedError e) => dnsmessage_package.Error⁔(ref e);
}
// </MethodForwarders>
