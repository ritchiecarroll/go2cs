// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin
namespace go.crypto.x509.@internal;

using errors = errors_package;
using abi = go.@internal.abi_package;
using strconv = strconv_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class macOS_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸstrconv() {
    builtin.initPackage(typeof(strconv_package));
}

[GoType("num:int32")] partial struct SecTrustSettingsResult;

// Security.framework linker flags for the external linker. See Issue 42459.
//
//go:cgo_ldflag "-framework"
//go:cgo_ldflag "Security"
// Based on https://opensource.apple.com/source/Security/Security-59306.41.2/base/Security.h
public static SecTrustSettingsResult SecTrustSettingsResultInvalid => /* iota */ 0;
public static SecTrustSettingsResult SecTrustSettingsResultTrustRoot => 1;
public static SecTrustSettingsResult SecTrustSettingsResultTrustAsRoot => 2;
public static SecTrustSettingsResult SecTrustSettingsResultDeny => 3;
public static SecTrustSettingsResult SecTrustSettingsResultUnspecified => 4;

[GoType("num:int32")] partial struct SecTrustResultType;

public static SecTrustResultType SecTrustResultInvalid => /* iota */ 0;
public static SecTrustResultType SecTrustResultProceed => 1;
public static SecTrustResultType SecTrustResultConfirm => 2; // deprecated
public static SecTrustResultType SecTrustResultDeny => 3;
public static SecTrustResultType SecTrustResultUnspecified => 4;
public static SecTrustResultType SecTrustResultRecoverableTrustFailure => 5;
public static SecTrustResultType SecTrustResultFatalTrustFailure => 6;
public static SecTrustResultType SecTrustResultOtherError => 7;

[GoType("num:int32")] partial struct SecTrustSettingsDomain;

public static SecTrustSettingsDomain SecTrustSettingsDomainUser => /* iota */ 0;
public static SecTrustSettingsDomain SecTrustSettingsDomainAdmin => 1;
public static SecTrustSettingsDomain SecTrustSettingsDomainSystem => 2;

public static UntypedInt ErrSecCertificateExpired => -67818;
public static UntypedInt ErrSecHostNameMismatch => -67602;
public static UntypedInt ErrSecNotTrusted => -67843;

[GoType] partial struct OSStatus {
    internal @string call;
    internal int32 status;
}

public static @string Error(this OSStatus s) {
    return s.call + " error: "u8 + strconv.Itoa((nint)s.status);
}

// Dictionary keys are defined as build-time strings with CFSTR, but the Go
// linker's internal linking mode can't handle CFSTR relocations. Create our
// own dynamic strings instead and just never release them.
//
// Note that this might be the only thing that can break over time if
// these values change, as the ABI arguably requires using the strings
// pointed to by the symbols, not values that happen to be equal to them.
public static CFString SecTrustSettingsResultKey = StringToCFString("kSecTrustSettingsResult"u8);

public static CFString SecTrustSettingsPolicy = StringToCFString("kSecTrustSettingsPolicy"u8);

public static CFString SecTrustSettingsPolicyString = StringToCFString("kSecTrustSettingsPolicyString"u8);

public static CFString SecPolicyOid = StringToCFString("SecPolicyOid"u8);

public static CFString SecPolicyAppleSSL = StringToCFString("1.2.840.113635.100.1.3"u8); // defined by POLICYMACRO

public static error ErrNoTrustSettings = errors.New("no trust settings found"u8);

internal static UntypedInt errSecNoTrustSettings => -25263;

//go:cgo_import_dynamic x509_SecTrustSettingsCopyCertificates SecTrustSettingsCopyCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef certArray, error err) SecTrustSettingsCopyCertificates(SecTrustSettingsDomain domain) {
    ref var certArray = ref heap(new CFRef(), out var ᏑcertArray);

    var ᴋ0 = @unsafe.Pointer.FromBox(ᏑcertArray);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustSettingsCopyCertificates_trampoline), (uintptr)(int32)domain, (uintptr)ᴋ0, 0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ0);
    if ((int32)ret == errSecNoTrustSettings){
        return (0, ErrNoTrustSettings);
    } else 
    if (ret != 0) {
        return (0, new OSStatus("SecTrustSettingsCopyCertificates"u8, (int32)ret));
    }
    return (certArray, default!);
}

internal static partial void x509_SecTrustSettingsCopyCertificates_trampoline();

internal static UntypedInt errSecItemNotFound => -25300;

//go:cgo_import_dynamic x509_SecTrustSettingsCopyTrustSettings SecTrustSettingsCopyTrustSettings "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef trustSettings, error err) SecTrustSettingsCopyTrustSettings(CFRef cert, SecTrustSettingsDomain domain) {
    ref var trustSettings = ref heap(new CFRef(), out var ᏑtrustSettings);

    var ᴋ1 = @unsafe.Pointer.FromBox(ᏑtrustSettings);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustSettingsCopyTrustSettings_trampoline), (uintptr)cert, (uintptr)(int32)domain, (uintptr)ᴋ1, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ1);
    if ((int32)ret == errSecItemNotFound){
        return (0, ErrNoTrustSettings);
    } else 
    if (ret != 0) {
        return (0, new OSStatus("SecTrustSettingsCopyTrustSettings"u8, (int32)ret));
    }
    return (trustSettings, default!);
}

internal static partial void x509_SecTrustSettingsCopyTrustSettings_trampoline();

//go:cgo_import_dynamic x509_SecTrustCreateWithCertificates SecTrustCreateWithCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecTrustCreateWithCertificates(CFRef certs, CFRef policies) {
    ref var trustObj = ref heap(new CFRef(), out var ᏑtrustObj);
    var ᴋ2 = @unsafe.Pointer.FromBox(ᏑtrustObj);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustCreateWithCertificates_trampoline), (uintptr)certs, (uintptr)policies, (uintptr)ᴋ2, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ2);
    if ((int32)ret != 0) {
        return (0, new OSStatus("SecTrustCreateWithCertificates"u8, (int32)ret));
    }
    return (trustObj, default!);
}

internal static partial void x509_SecTrustCreateWithCertificates_trampoline();

//go:cgo_import_dynamic x509_SecCertificateCreateWithData SecCertificateCreateWithData "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecCertificateCreateWithData(slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        var data = BytesToCFData(b);
        defer(CFRelease, data, ref ᒐ);
        var ret = syscall(abi.FuncPCABI0(x509_SecCertificateCreateWithData_trampoline), kCFAllocatorDefault, (uintptr)data, 0, 0, 0, 0D);
        // Returns NULL if the data passed in the data parameter is not a valid
        // DER-encoded X.509 certificate.
        if (ret == 0) {
            return (0, errors.New("SecCertificateCreateWithData: invalid certificate"u8));
        }
        return (((CFRef)ret), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static partial void x509_SecCertificateCreateWithData_trampoline();

//go:cgo_import_dynamic x509_SecPolicyCreateSSL SecPolicyCreateSSL "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecPolicyCreateSSL(@string name) {
    GoFrame ᒐ = default;
    try {
        CFString hostname = default!;
        if (name != ""u8) {
            hostname = StringToCFString(name);
            defer(CFRelease, ((CFRef)(uintptr)hostname), ref ᒐ);
        }
        var ret = syscall(abi.FuncPCABI0(x509_SecPolicyCreateSSL_trampoline), 1, (uintptr)hostname, 0, 0, 0, 0D);
        /* true */
        if (ret == 0) {
            return (0, new OSStatus("SecPolicyCreateSSL"u8, (int32)ret));
        }
        return (((CFRef)ret), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static partial void x509_SecPolicyCreateSSL_trampoline();

//go:cgo_import_dynamic x509_SecTrustSetVerifyDate SecTrustSetVerifyDate "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static error SecTrustSetVerifyDate(CFRef trustObj, CFRef dateRef) {
    var ret = syscall(abi.FuncPCABI0(x509_SecTrustSetVerifyDate_trampoline), (uintptr)trustObj, (uintptr)dateRef, 0, 0, 0, 0D);
    if ((int32)ret != 0) {
        return new OSStatus("SecTrustSetVerifyDate"u8, (int32)ret);
    }
    return default!;
}

internal static partial void x509_SecTrustSetVerifyDate_trampoline();

//go:cgo_import_dynamic x509_SecTrustEvaluate SecTrustEvaluate "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecTrustEvaluate(CFRef trustObj) {
    ref var result = ref heap(new CFRef(), out var Ꮡresult);
    var ᴋ3 = @unsafe.Pointer.FromBox(Ꮡresult);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustEvaluate_trampoline), (uintptr)trustObj, (uintptr)ᴋ3, 0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ3);
    if ((int32)ret != 0) {
        return (0, new OSStatus("SecTrustEvaluate"u8, (int32)ret));
    }
    return (result, default!);
}

internal static partial void x509_SecTrustEvaluate_trampoline();

//go:cgo_import_dynamic x509_SecTrustGetResult SecTrustGetResult "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, CFRef, error) SecTrustGetResult(CFRef trustObj, CFRef resultʗp) {
    ref var result = ref heap(resultʗp, out var Ꮡresult);

    ref var chain = ref heap(new CFRef(), out var Ꮡchain);
    ref var info = ref heap(new CFRef(), out var Ꮡinfo);
    var ᴋ4 = @unsafe.Pointer.FromBox(Ꮡresult);
    var ᴋ5 = @unsafe.Pointer.FromBox(Ꮡchain);
    var ᴋ6 = @unsafe.Pointer.FromBox(Ꮡinfo);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustGetResult_trampoline), (uintptr)trustObj, (uintptr)ᴋ4, (uintptr)ᴋ5, (uintptr)ᴋ6, 0, 0D);
    System.GC.KeepAlive(ᴋ4);
    System.GC.KeepAlive(ᴋ5);
    System.GC.KeepAlive(ᴋ6);
    if ((int32)ret != 0) {
        return (0, 0, new OSStatus("SecTrustGetResult"u8, (int32)ret));
    }
    return (chain, info, default!);
}

internal static partial void x509_SecTrustGetResult_trampoline();

//go:cgo_import_dynamic x509_SecTrustEvaluateWithError SecTrustEvaluateWithError "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (nint, error) SecTrustEvaluateWithError(CFRef trustObj) {
    ref var errRef = ref heap(new CFRef(), out var ᏑerrRef);
    var ᴋ7 = @unsafe.Pointer.FromBox(ᏑerrRef);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustEvaluateWithError_trampoline), (uintptr)trustObj, (uintptr)ᴋ7, 0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ7);
    if ((int32)ret != 1) {
        var errStr = CFErrorCopyDescription(errRef);
        var err = errors.New(CFStringToString(errStr));
        nint errCode = CFErrorGetCode(errRef);
        CFRelease(errRef);
        CFRelease(errStr);
        return (errCode, err);
    }
    return (0, default!);
}

internal static partial void x509_SecTrustEvaluateWithError_trampoline();

//go:cgo_import_dynamic x509_SecTrustGetCertificateCount SecTrustGetCertificateCount "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static nint SecTrustGetCertificateCount(CFRef trustObj) {
    var ret = syscall(abi.FuncPCABI0(x509_SecTrustGetCertificateCount_trampoline), (uintptr)trustObj, 0, 0, 0, 0, 0D);
    return (nint)ret;
}

internal static partial void x509_SecTrustGetCertificateCount_trampoline();

//go:cgo_import_dynamic x509_SecTrustGetCertificateAtIndex SecTrustGetCertificateAtIndex "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecTrustGetCertificateAtIndex(CFRef trustObj, nint i) {
    var ret = syscall(abi.FuncPCABI0(x509_SecTrustGetCertificateAtIndex_trampoline), (uintptr)trustObj, (uintptr)i, 0, 0, 0, 0D);
    if (ret == 0) {
        return (0, new OSStatus("SecTrustGetCertificateAtIndex"u8, (int32)ret));
    }
    return (((CFRef)ret), default!);
}

internal static partial void x509_SecTrustGetCertificateAtIndex_trampoline();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string x509InvalidCertificateˢ = "x509: invalid certificate object"u8;

//go:cgo_import_dynamic x509_SecCertificateCopyData SecCertificateCopyData "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (slice<byte>, error) SecCertificateCopyData(CFRef cert) {
    var ret = syscall(abi.FuncPCABI0(x509_SecCertificateCopyData_trampoline), (uintptr)cert, 0, 0, 0, 0, 0D);
    if (ret == 0) {
        return (default!, errors.New(x509InvalidCertificateˢ));
    }
    var b = CFDataToSlice(((CFRef)ret));
    CFRelease(((CFRef)ret));
    return (b, default!);
}

internal static partial void x509_SecCertificateCopyData_trampoline();

} // end macOS_package
