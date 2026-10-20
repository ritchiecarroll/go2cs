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

partial class macos_package {

// Security.framework linker flags for the external linker. See Issue 42459.
//
//go:cgo_ldflag "-framework"
//go:cgo_ldflag "Security"
// Based on https://opensource.apple.com/source/Security/Security-59306.41.2/base/Security.h

// various macOS error codes that can be returned from
// SecTrustEvaluateWithError that we can map to Go cert
// verification error types.
public static UntypedInt ErrSecCertificateExpired => -67818;
public static UntypedInt ErrSecHostNameMismatch => -67602;
public static UntypedInt ErrSecNotTrusted => -67843;

partial struct OSStatus {
    internal @string call;
    internal int32 status;
}

public static @string Error(this OSStatus s) {
    return s.call + " error: "u8 + strconv.Itoa((nint)s.status);
}

//go:cgo_import_dynamic x509_SecTrustCreateWithCertificates SecTrustCreateWithCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecTrustCreateWithCertificates(CFRef certs, CFRef policies) {
    ref var trustObj = ref heap(new CFRef(), out var ᏑtrustObj);
    var ᴋ0 = @unsafe.Pointer.FromBox(ᏑtrustObj);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustCreateWithCertificates_trampoline), (uintptr)certs, (uintptr)policies, (uintptr)ᴋ0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ0);
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
        return ((CFRef)ret, default!);
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
            defer(CFRelease, (CFRef)(uintptr)hostname, ref ᒐ);
        }
        var ret = syscall(abi.FuncPCABI0(x509_SecPolicyCreateSSL_trampoline), 1, (uintptr)hostname, 0, 0, 0, 0D);
        /* true */
        if (ret == 0) {
            return (0, new OSStatus("SecPolicyCreateSSL"u8, (int32)ret));
        }
        return ((CFRef)ret, default!);
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
    var ᴋ1 = @unsafe.Pointer.FromBox(Ꮡresult);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustEvaluate_trampoline), (uintptr)trustObj, (uintptr)ᴋ1, 0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ1);
    if ((int32)ret != 0) {
        return (0, new OSStatus("SecTrustEvaluate"u8, (int32)ret));
    }
    return (result, default!);
}

internal static partial void x509_SecTrustEvaluate_trampoline();

//go:cgo_import_dynamic x509_SecTrustEvaluateWithError SecTrustEvaluateWithError "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (nint, error) SecTrustEvaluateWithError(CFRef trustObj) {
    ref var errRef = ref heap(new CFRef(), out var ᏑerrRef);
    var ᴋ2 = @unsafe.Pointer.FromBox(ᏑerrRef);
        var ret = syscall(abi.FuncPCABI0(x509_SecTrustEvaluateWithError_trampoline), (uintptr)trustObj, (uintptr)ᴋ2, 0, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ2);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string x509InvalidCertificateˢ = "x509: invalid certificate object"u8;

//go:cgo_import_dynamic x509_SecCertificateCopyData SecCertificateCopyData "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (slice<byte>, error) SecCertificateCopyData(CFRef cert) {
    var ret = syscall(abi.FuncPCABI0(x509_SecCertificateCopyData_trampoline), (uintptr)cert, 0, 0, 0, 0, 0D);
    if (ret == 0) {
        return (default!, errors.New(x509InvalidCertificateˢ));
    }
    var b = CFDataToSlice((CFRef)ret);
    CFRelease((CFRef)ret);
    return (b, default!);
}

internal static partial void x509_SecCertificateCopyData_trampoline();

//go:cgo_import_dynamic x509_SecTrustCopyCertificateChain SecTrustCopyCertificateChain "/System/Library/Frameworks/Security.framework/Versions/A/Security"
public static (CFRef, error) SecTrustCopyCertificateChain(CFRef trustObj) {
    var ret = syscall(abi.FuncPCABI0(x509_SecTrustCopyCertificateChain_trampoline), (uintptr)trustObj, 0, 0, 0, 0, 0D);
    if (ret == 0) {
        return (0, new OSStatus("SecTrustCopyCertificateChain"u8, (int32)ret));
    }
    return ((CFRef)ret, default!);
}

internal static partial void x509_SecTrustCopyCertificateChain_trampoline();

} // end macos_package
