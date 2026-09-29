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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.crypto.x509.@internal.macOS_package;

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
[assembly: GoImplement<OSStatus, error>]
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
[assembly: go.GoPositionMap("crypto/x509/internal/macos/corefoundation.go", "corefoundation.cs", "ABY+4oKCgqqigoKUgoKokoKCAAgUkoKigqSasoK0gqSYsqSClKTskoKkgpSkmJKCpJiSgqSYkoKkmJKCpJiSgqSYkqSYkpKkmJKkmJKCpJiSgqSYkqTIkoKClKSWqrKCgpQ=")]
[assembly: go.GoPositionMap("crypto/x509/internal/macos/security.go", "security.cs", "ADWIAYIACRaSkpKSlLyypIKklKS8sqSCpJSkmJKCpIKUpJiygoKmgpTUmLKCgoKUkoKU1JiSgoKUpJiSgqKClKSYspLkgpSkmJKCooKCgoKCgpSkmJKCpJiSgoKUpMiSgoKUgoKk")]
// </GoSourcePositionMaps>

// Dynamically imported C entry points are recorded here, one `GoCgoImportDynamic` attribute
// per `//go:cgo_import_dynamic` pragma this package binds to a trampoline declaration, so
// that `abi.FuncPCABI0` of that trampoline resolves to the REAL address of the exported
// symbol rather than to a token. The value is dereferenced by design - the trampoline's
// caller jumps to it - which is why a stub carrying no record here is left a loud throw
// instead: an address that is merely plausible is fatal at the first call.

// <CgoDynamicImports>
[assembly: go.GoCgoImportDynamic("x509_CFArrayAppendValue_trampoline", "CFArrayAppendValue", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFArrayCreateMutable_trampoline", "CFArrayCreateMutable", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFArrayGetCount_trampoline", "CFArrayGetCount", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFArrayGetValueAtIndex_trampoline", "CFArrayGetValueAtIndex", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFDataCreate_trampoline", "CFDataCreate", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFDataGetBytePtr_trampoline", "CFDataGetBytePtr", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFDataGetLength_trampoline", "CFDataGetLength", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFDateCreate_trampoline", "CFDateCreate", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFDictionaryGetValueIfPresent_trampoline", "CFDictionaryGetValueIfPresent", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFEqual_trampoline", "CFEqual", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFErrorCopyDescription_trampoline", "CFErrorCopyDescription", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFErrorGetCode_trampoline", "CFErrorGetCode", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFNumberGetValue_trampoline", "CFNumberGetValue", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFRelease_trampoline", "CFRelease", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFStringCreateExternalRepresentation_trampoline", "CFStringCreateExternalRepresentation", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_CFStringCreateWithBytes_trampoline", "CFStringCreateWithBytes", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
[assembly: go.GoCgoImportDynamic("x509_SecCertificateCopyData_trampoline", "SecCertificateCopyData", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecCertificateCreateWithData_trampoline", "SecCertificateCreateWithData", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecPolicyCreateSSL_trampoline", "SecPolicyCreateSSL", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustCreateWithCertificates_trampoline", "SecTrustCreateWithCertificates", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustEvaluateWithError_trampoline", "SecTrustEvaluateWithError", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustEvaluate_trampoline", "SecTrustEvaluate", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustGetCertificateAtIndex_trampoline", "SecTrustGetCertificateAtIndex", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustGetCertificateCount_trampoline", "SecTrustGetCertificateCount", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustGetResult_trampoline", "SecTrustGetResult", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustSetVerifyDate_trampoline", "SecTrustSetVerifyDate", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustSettingsCopyCertificates_trampoline", "SecTrustSettingsCopyCertificates", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
[assembly: go.GoCgoImportDynamic("x509_SecTrustSettingsCopyTrustSettings_trampoline", "SecTrustSettingsCopyTrustSettings", "/System/Library/Frameworks/Security.framework/Versions/A/Security")]
// </CgoDynamicImports>

namespace go.crypto.x509.@internal;

[GoPackage("macOS", ImportPath = "crypto/x509/internal/macos")]
public static partial class macOS_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct CFRef {}
    public partial struct CFString {}
    public partial struct OSStatus {}
    public partial struct SecTrustResultType {}
    public partial struct SecTrustSettingsDomain {}
    public partial struct SecTrustSettingsResult {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
