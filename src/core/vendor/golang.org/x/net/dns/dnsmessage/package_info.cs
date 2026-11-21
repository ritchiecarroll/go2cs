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
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/message.go", "message.cs", "ADt4ABUqkoKUqJKClAAQIgAJEpKClKiSgpQACQ6SAA8gAAoUkoKUqJKClMaCgu6CgoKUgpSmooKUgoKCgpTaooKCgoiCpoKCAAYQpoKmlIKCgoKUgpQACAaCgpTeuJSCgoKCgoKCgoKCgoKCgoKCgoKCAB1CkgAUIrKCgoKUgpSClIKUgpSClIKUqJIAH1QAEySClKSkpJSokoKCgoKCpoKCgoKUgpSClIKUgpSClKaCABcoggARKJKClIKCgoKUgoKClIKUACA+koKUgoKClIKmgoKUgpSCgoKClKaCgoKigpSCgoKUgqaCgqaClIKCgpSCgoKCgqaCgoKClIKCgpSClIKCgpSCqJKClIKCgpSCgpSCgpSCgqj+goKCgpSClLqSgpSCgpSClIKUgoKokoKCgoLMkqiSqOyCgpSCgoKClIKUAAMQwqiSgoKCgsySqJKo7IKClIKCgoKUgpQAAxDCqJKCgoKCzJKokqjsgoKUgoKCgpSClAADEMKokoKCgoIABBLCgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCrsKClIKClIKCgq7CgpSCgpSCgoKuwoKUgoKUgoKCyJKCgoKUgpSClIKUgpSokqrYgpSClIKUgqaClIKCgpSCAAkUlIKCgqaCgoKmgoKCpoKCgriokoSCgqKmgoKCoqaCgoKipoKCgqKmAB5WAAoCgpSCgoKCggAEHAALAqaCgpSClKiSgpSCqJKClIKokoKUgqiSgpSCpqKCgpSCtIK0grSCpIKUgqjCgpSClIKClIKUgqaCgpSClKjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqMKClIKCgpSCgpSClIKUgqjSgpSCgoKUgoKUgpSClIKo0oKUgoKClIKClIKUgpSCqNKClIKCgpSCgpSClIKUgqjCgpSCgoKUgoKUgpSClIKowoKUgoKClIKClIKUgpSCqJKClJSCABw8kgACFvKCgpSCgoKCgqaCgoKClIKUgpSClIKUAAIS4oKCuIKUAA0esoKCgoKClKiSrLKClMaCgoKUgpSClIKUgoKUgpSokqaCgpSmgoKUqJKmgoKmgqiSpoKCpoKokgAHEoKClIKmgoKUqJKCgpSClKaCgpSCgoKUqJKmgoKClIIACxqSgoKUgqiSgoKUzNKokgACFPKUgriCuIKmppS4griCppSCpoLcgqbKgoKmlLioxMqWtpSCgoKUgoKUlJSCgsqCgriCgrSClIKCgqaClMbGgpSClIKClMa4lIKCgpSCgpSUpoKC/KbG2AALFpKCgpSCqJIAGgyiqpSCkoK0gpKCtIKSgrSCkoK0gpKCtIKSgrSCgoK0gpKCtIKSgrSCkoK0gqKCtIKCgrSCgoKkgpQACRCCqJKoksaCgoKUAAoSgqiSgoKCgpSoksqCgoKUgoKUAAkQgqiSqJLGgoKClAAJEIKokqiSxoKCgpQAEiSCqJKCgoKUgoKUgoKCgqiSAAQUgoKCgpSCgpSCgpSCgpSCgpSCgpSCgpQACRCCqJKCgoKCgqbokoKClIKClMaCgoKCgoKmgpSClAAMFoKokoKCgoKCgpSoks6CgoKUgoKUgoKUgoKUAAkQgqiSqJLIgoKClAAJEIKokqqSxoKCgpQAFSqSyoKmgoKCgoKU6JKCgpSCopTGgoKCgoKCgpSCgoKUgoKUgpQAChKCqJKoksqCuIKU", "", "", "397=printUint16/1/1/1,printBool/1/7/2,GoString/1/2/3,printBool/2/7/4,printBool/3/7/5,printBool/4/7/6,printBool/5/7/7,printBool/6/7/8,printBool/7/7/9,GoString/2/2/10;526=GoString/1/2/1,GoString/2/2/2;1780=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3,printUint32/1/1/4,printUint16/1/1/5;2296=GoString/1/3/1,GoString/2/3/2,GoString/3/3/3;2494=printUint16/1/1/1,GoString/1/1/2;2613=GoString/1/2/1,GoString/2/2/2,printUint32/1/5/3,printUint32/2/5/4,printUint32/3/5/5,printUint32/4/5/6,printUint32/5/5/7;2741=printUint16/1/3/1,printUint16/2/3/2,printUint16/3/3/3,GoString/1/1/4;2784=printByteSlice/1/1/1;2810=printByteSlice/1/1/1;2849=printUint16/1/1/1,printByteSlice/1/1/2;2932=GoString/1/1/1,printByteSlice/1/1/2")]
[assembly: GoPositionMap("vendor/golang.org/x/net/dns/dnsmessage/svcb.go", "svcb.cs", "ABMkgqiSiIKCgqaCAA0UgqiSyJKCgpSCpqqigoKClKaCgqaqooKCgpSCpgAMFJIAEywADhySgpSokoKUxoKCyoKClIKCgpSClIKClMaUgoKUgoKmgsqCgoKCgpKClKaUgpSClIKClIK4poKCgoKSgpSCgpSClIKUgoKmyJKClIKClIKCgq7CrsKCgpTI0oKUgoKUgoKUgpSClIKowoKowoI=", "", "", "26=printUint16/1/1/1,GoString/1/1/2;118=GoString/1/1/1,printByteSlice/1/1/2")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.dns;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using static dnsmessage_package;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class dnsmessageᴛType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Type t) => dnsmessage_package.Stringᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Type t) => dnsmessage_package.GoStringᴍ(t);
}
partial class dnsmessageᴛClass {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Class c) => dnsmessage_package.Stringᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Class c) => dnsmessage_package.GoStringᴍ(c);
}
partial class dnsmessageᴛOpCode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this OpCode o) => dnsmessage_package.GoStringᴍ(o);
}
partial class dnsmessageᴛRCode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this RCode r) => dnsmessage_package.Stringᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this RCode r) => dnsmessage_package.GoStringᴍ(r);
}
partial class dnsmessageᴛnestedError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref nestedError e) => dnsmessage_package.Errorᴍ(ref e);
}
partial class dnsmessageᴛHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 id, uint16 bits) pack(this ref Header m) => dnsmessage_package.packᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Header m) => dnsmessage_package.GoStringᴍ(ref m);
}
partial class dnsmessageᴛheader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 count(this ref header h, section sec) => dnsmessage_package.countᴍ(ref h, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> pack(this ref header h, slice<byte> msg) => dnsmessage_package.packᴍ(ref h, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) unpack(this ref header h, slice<byte> msg, nint off) => dnsmessage_package.unpackᴍ(ref h, msg, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Header header(this ref header h) => dnsmessage_package.headerᴍ(ref h);
}
partial class dnsmessageᴛResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Resource r) => dnsmessage_package.GoStringᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Resource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
}
partial class dnsmessageᴛParser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Header, error) Start(this ref Parser p, slice<byte> msg) => dnsmessage_package.Startᴍ(ref p, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkAdvance(this ref Parser p, section sec) => dnsmessage_package.checkAdvanceᴍ(ref p, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Resource, error) resource(this ref Parser p, section sec) => dnsmessage_package.resourceᴍ(ref p, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ResourceHeader, error) resourceHeader(this ref Parser p, section sec) => dnsmessage_package.resourceHeaderᴍ(ref p, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error skipResource(this ref Parser p, section sec) => dnsmessage_package.skipResourceᴍ(ref p, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Question, error) Question(this ref Parser p) => dnsmessage_package.Questionᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Question>, error) AllQuestions(this ref Parser p) => dnsmessage_package.AllQuestionsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipQuestion(this ref Parser p) => dnsmessage_package.SkipQuestionᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAllQuestions(this ref Parser p) => dnsmessage_package.SkipAllQuestionsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ResourceHeader, error) AnswerHeader(this ref Parser p) => dnsmessage_package.AnswerHeaderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Resource, error) Answer(this ref Parser p) => dnsmessage_package.Answerᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Resource>, error) AllAnswers(this ref Parser p) => dnsmessage_package.AllAnswersᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAnswer(this ref Parser p) => dnsmessage_package.SkipAnswerᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAllAnswers(this ref Parser p) => dnsmessage_package.SkipAllAnswersᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ResourceHeader, error) AuthorityHeader(this ref Parser p) => dnsmessage_package.AuthorityHeaderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Resource, error) Authority(this ref Parser p) => dnsmessage_package.Authorityᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Resource>, error) AllAuthorities(this ref Parser p) => dnsmessage_package.AllAuthoritiesᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAuthority(this ref Parser p) => dnsmessage_package.SkipAuthorityᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAllAuthorities(this ref Parser p) => dnsmessage_package.SkipAllAuthoritiesᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ResourceHeader, error) AdditionalHeader(this ref Parser p) => dnsmessage_package.AdditionalHeaderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Resource, error) Additional(this ref Parser p) => dnsmessage_package.Additionalᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Resource>, error) AllAdditionals(this ref Parser p) => dnsmessage_package.AllAdditionalsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAdditional(this ref Parser p) => dnsmessage_package.SkipAdditionalᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SkipAllAdditionals(this ref Parser p) => dnsmessage_package.SkipAllAdditionalsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (CNAMEResource, error) CNAMEResource(this ref Parser p) => dnsmessage_package.CNAMEResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (MXResource, error) MXResource(this ref Parser p) => dnsmessage_package.MXResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (NSResource, error) NSResource(this ref Parser p) => dnsmessage_package.NSResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (PTRResource, error) PTRResource(this ref Parser p) => dnsmessage_package.PTRResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (SOAResource, error) SOAResource(this ref Parser p) => dnsmessage_package.SOAResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (TXTResource, error) TXTResource(this ref Parser p) => dnsmessage_package.TXTResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (SRVResource, error) SRVResource(this ref Parser p) => dnsmessage_package.SRVResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (AResource, error) AResource(this ref Parser p) => dnsmessage_package.AResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (AAAAResource, error) AAAAResource(this ref Parser p) => dnsmessage_package.AAAAResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (OPTResource, error) OPTResource(this ref Parser p) => dnsmessage_package.OPTResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (UnknownResource, error) UnknownResource(this ref Parser p) => dnsmessage_package.UnknownResourceᴍ(ref p);
}
partial class dnsmessageᴛMessage {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unpack(this ref Message m, slice<byte> msg) => dnsmessage_package.Unpackᴍ(ref m, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) Pack(this ref Message m) => dnsmessage_package.Packᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendPack(this ref Message m, slice<byte> b) => dnsmessage_package.AppendPackᴍ(ref m, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Message m) => dnsmessage_package.GoStringᴍ(ref m);
}
partial class dnsmessageᴛBuilder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void EnableCompression(this ref Builder b) => dnsmessage_package.EnableCompressionᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error startCheck(this ref Builder b, section s) => dnsmessage_package.startCheckᴍ(ref b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error StartQuestions(this ref Builder b) => dnsmessage_package.StartQuestionsᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error StartAnswers(this ref Builder b) => dnsmessage_package.StartAnswersᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error StartAuthorities(this ref Builder b) => dnsmessage_package.StartAuthoritiesᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error StartAdditionals(this ref Builder b) => dnsmessage_package.StartAdditionalsᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error incrementSectionCount(this ж<Builder> Ꮡb) => dnsmessage_package.incrementSectionCountᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Question(this ж<Builder> Ꮡb, Question q) => dnsmessage_package.Questionᴍ(Ꮡb, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkResourceSection(this ref Builder b) => dnsmessage_package.checkResourceSectionᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CNAMEResource(this ж<Builder> Ꮡb, ResourceHeader h, CNAMEResource r) => dnsmessage_package.CNAMEResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error MXResource(this ж<Builder> Ꮡb, ResourceHeader h, MXResource r) => dnsmessage_package.MXResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error NSResource(this ж<Builder> Ꮡb, ResourceHeader h, NSResource r) => dnsmessage_package.NSResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error PTRResource(this ж<Builder> Ꮡb, ResourceHeader h, PTRResource r) => dnsmessage_package.PTRResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SOAResource(this ж<Builder> Ꮡb, ResourceHeader h, SOAResource r) => dnsmessage_package.SOAResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error TXTResource(this ж<Builder> Ꮡb, ResourceHeader h, TXTResource r) => dnsmessage_package.TXTResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SRVResource(this ж<Builder> Ꮡb, ResourceHeader h, SRVResource r) => dnsmessage_package.SRVResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error AResource(this ж<Builder> Ꮡb, ResourceHeader h, AResource r) => dnsmessage_package.AResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error AAAAResource(this ж<Builder> Ꮡb, ResourceHeader h, AAAAResource r) => dnsmessage_package.AAAAResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error OPTResource(this ж<Builder> Ꮡb, ResourceHeader h, OPTResource r) => dnsmessage_package.OPTResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnknownResource(this ж<Builder> Ꮡb, ResourceHeader h, UnknownResource r) => dnsmessage_package.UnknownResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) Finish(this ref Builder b) => dnsmessage_package.Finishᴍ(ref b);
}
partial class dnsmessageᴛResourceHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref ResourceHeader h) => dnsmessage_package.GoStringᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte> msg, nint lenOff, error err) pack(this ref ResourceHeader h, slice<byte> oldMsg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref h, oldMsg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) unpack(this ref ResourceHeader h, slice<byte> msg, nint off) => dnsmessage_package.unpackᴍ(ref h, msg, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error fixLen(this ref ResourceHeader h, slice<byte> msg, nint lenOff, nint preLen) => dnsmessage_package.fixLenᴍ(ref h, msg, lenOff, preLen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetEDNS0(this ref ResourceHeader h, nint udpPayloadLen, RCode extRCode, bool dnssecOK) => dnsmessage_package.SetEDNS0ᴍ(ref h, udpPayloadLen, extRCode, dnssecOK);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool DNSSECAllowed(this ref ResourceHeader h) => dnsmessage_package.DNSSECAllowedᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static RCode ExtendedRCode(this ref ResourceHeader h, RCode rcode) => dnsmessage_package.ExtendedRCodeᴍ(ref h, rcode);
}
partial class dnsmessageᴛName {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Name n) => dnsmessage_package.Stringᴍ(n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Name n) => dnsmessage_package.GoStringᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Name n, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref n, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) unpack(this ref Name n, slice<byte> msg, nint off) => dnsmessage_package.unpackᴍ(ref n, msg, off);
}
partial class dnsmessageᴛQuestion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref Question q, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref q, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Question q) => dnsmessage_package.GoStringᴍ(ref q);
}
partial class dnsmessageᴛCNAMEResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref CNAMEResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref CNAMEResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref CNAMEResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛMXResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref MXResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref MXResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref MXResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛNSResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref NSResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref NSResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref NSResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛPTRResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref PTRResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref PTRResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref PTRResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛSOAResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref SOAResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SOAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref SOAResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛTXTResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref TXTResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref TXTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref TXTResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛSRVResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref SRVResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SRVResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref SRVResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛAResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref AResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref AResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref AResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛAAAAResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref AAAAResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref AAAAResource r) => dnsmessage_package.GoStringᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref AAAAResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
}
partial class dnsmessageᴛOption {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref Option o) => dnsmessage_package.GoStringᴍ(ref o);
}
partial class dnsmessageᴛOPTResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref OPTResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref OPTResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref OPTResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛUnknownResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref UnknownResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref UnknownResource r, slice<byte> msg, map<@string, uint16> compression, nint compressionOff) => dnsmessage_package.packᴍ(ref r, msg, compression, compressionOff);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref UnknownResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛSVCBResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref SVCBResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref SVCBResource r) => dnsmessage_package.GoStringᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte> value, bool ok) GetParam(this ref SVCBResource r, SVCParamKey key) => dnsmessage_package.GetParamᴍ(ref r, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetParam(this ref SVCBResource r, SVCParamKey key, slice<byte> value) => dnsmessage_package.SetParamᴍ(ref r, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool DeleteParam(this ref SVCBResource r, SVCParamKey key) => dnsmessage_package.DeleteParamᴍ(ref r, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) pack(this ref SVCBResource r, slice<byte> msg, map<@string, uint16> _Δp2, nint _Δp3) => dnsmessage_package.packᴍ(ref r, msg, _Δp2, _Δp3);
}
partial class dnsmessageᴛHTTPSResource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type realType(this ref HTTPSResource r) => dnsmessage_package.realTypeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ref HTTPSResource r) => dnsmessage_package.GoStringᴍ(ref r);
}
partial class dnsmessageᴛSVCParam {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SVCParam p) => dnsmessage_package.GoStringᴍ(p);
}
partial class dnsmessageᴛSVCParamKey {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SVCParamKey k) => dnsmessage_package.Stringᴍ(k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SVCParamKey k) => dnsmessage_package.GoStringᴍ(k);
}
partial class dnsmessageᴛParser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (SVCBResource, error) genericSVCBResource(this ref Parser p, Type svcbType) => dnsmessage_package.genericSVCBResourceᴍ(ref p, svcbType);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (SVCBResource, error) SVCBResource(this ref Parser p) => dnsmessage_package.SVCBResourceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (HTTPSResource, error) HTTPSResource(this ref Parser p) => dnsmessage_package.HTTPSResourceᴍ(ref p);
}
partial class dnsmessageᴛBuilder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error genericSVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, SVCBResource r) => dnsmessage_package.genericSVCBResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SVCBResource(this ж<Builder> Ꮡb, ResourceHeader h, SVCBResource r) => dnsmessage_package.SVCBResourceᴍ(Ꮡb, h, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error HTTPSResource(this ж<Builder> Ꮡb, ResourceHeader h, HTTPSResource r) => dnsmessage_package.HTTPSResourceᴍ(Ꮡb, h, r);
}
// </GlyphForwarders>
