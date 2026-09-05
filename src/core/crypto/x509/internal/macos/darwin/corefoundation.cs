// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin

// Package macOS provides cgo-less wrappers for Core Foundation and
// Security.framework, similarly to how package syscall provides access to
// libSystem.dylib.
namespace go.crypto.x509.@internal;

using bytes = bytes_package;
using errors = errors_package;
using abi = go.@internal.abi_package;
using runtime = runtime_package;
using time = time_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class macOS_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbytes() {
    builtin.initPackage(typeof(bytes_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() {
    builtin.initPackage(typeof(go.@internal.abi_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸruntime() {
    builtin.initPackage(typeof(runtime_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸtime() {
    builtin.initPackage(typeof(time_package));
}

[GoType("num:uintptr")] partial struct CFRef;

// Core Foundation linker flags for the external linker. See Issue 42459.
//
//go:cgo_ldflag "-framework"
//go:cgo_ldflag "CoreFoundation"

// CFDataToSlice returns a copy of the contents of data as a bytes slice.
public static slice<byte> CFDataToSlice(CFRef data) {
    nint length = CFDataGetLength(data);
    var ptr = CFDataGetBytePtr(data);
    var src = @unsafe.Slice((ж<byte>)(uintptr)((@unsafe.Pointer)ptr), length);
    return bytes.Clone(src);
}

// CFStringToString returns a Go string representation of the passed
// in CFString, or an empty string if it's invalid.
public static @string CFStringToString(CFRef @ref) {
    var (data, err) = CFStringCreateExternalRepresentation(@ref);
    if (err != default!) {
        return ""u8;
    }
    var b = CFDataToSlice(data);
    CFRelease(data);
    return ((@string)b);
}

// TimeToCFDateRef converts a time.Time into an apple CFDateRef.
public static CFRef TimeToCFDateRef(time.Time t) {
    var secs = t.Sub(time.Date(2001, 1, 1, 0, 0, 0, 0, time.ΔUTC)).Seconds();
    var @ref = CFDateCreate(secs);
    return @ref;
}

[GoType("num:uintptr")] partial struct CFString;

internal static UntypedInt kCFAllocatorDefault => 0;

internal static UntypedInt kCFStringEncodingUTF8 => 0x08000100;

//go:cgo_import_dynamic x509_CFDataCreate CFDataCreate "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static CFRef BytesToCFData(slice<byte> b) {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(b));
    var ᴋ0 = p;
        var ret = syscall(abi.FuncPCABI0(x509_CFDataCreate_trampoline), kCFAllocatorDefault, (uintptr)ᴋ0, (uintptr)len(b), 0, 0, 0D);
    System.GC.KeepAlive(ᴋ0);
    runtime.KeepAlive(p);
    return ((CFRef)ret);
}

internal static partial void x509_CFDataCreate_trampoline();

//go:cgo_import_dynamic x509_CFStringCreateWithBytes CFStringCreateWithBytes "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

// StringToCFString returns a copy of the UTF-8 contents of s as a new CFString.
public static CFString StringToCFString(@string s) {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s));
    var ᴋ1 = p;
        var ret = syscall(abi.FuncPCABI0(x509_CFStringCreateWithBytes_trampoline), kCFAllocatorDefault, (uintptr)ᴋ1, (uintptr)len(s), (uintptr)kCFStringEncodingUTF8, 0, 0D);
    System.GC.KeepAlive(ᴋ1);
    /* isExternalRepresentation */
    runtime.KeepAlive(p);
    return ((CFString)ret);
}

internal static partial void x509_CFStringCreateWithBytes_trampoline();

//go:cgo_import_dynamic x509_CFDictionaryGetValueIfPresent CFDictionaryGetValueIfPresent "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static (CFRef value, bool ok) CFDictionaryGetValueIfPresent(CFRef dict, CFString key) {
    ref var value = ref heap(new CFRef(), out var Ꮡvalue);

    var ᴋ2 = @unsafe.Pointer.FromBox(Ꮡvalue);
        var ret = syscall(abi.FuncPCABI0(x509_CFDictionaryGetValueIfPresent_trampoline), (uintptr)dict, (uintptr)key, (uintptr)ᴋ2, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ2);
    if (ret == 0) {
        return (0, false);
    }
    return (value, true);
}

internal static partial void x509_CFDictionaryGetValueIfPresent_trampoline();

internal static UntypedInt kCFNumberSInt32Type => 3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cfNumberGetValueCallˢ = "CFNumberGetValue call failed"u8;

//go:cgo_import_dynamic x509_CFNumberGetValue CFNumberGetValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static (int32, error) CFNumberGetValue(CFRef num) {
    ref var value = ref heap(new int32(), out var Ꮡvalue);
    var ᴋ3 = Ꮡvalue;
        var ret = syscall(abi.FuncPCABI0(x509_CFNumberGetValue_trampoline), (uintptr)num, (uintptr)kCFNumberSInt32Type, (uintptr)ᴋ3, 0, 0, 0D);
    System.GC.KeepAlive(ᴋ3);
    if (ret == 0) {
        return (0, errors.New(cfNumberGetValueCallˢ));
    }
    return (value, default!);
}

internal static partial void x509_CFNumberGetValue_trampoline();

//go:cgo_import_dynamic x509_CFDataGetLength CFDataGetLength "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static nint CFDataGetLength(CFRef data) {
    var ret = syscall(abi.FuncPCABI0(x509_CFDataGetLength_trampoline), (uintptr)data, 0, 0, 0, 0, 0D);
    return (nint)ret;
}

internal static partial void x509_CFDataGetLength_trampoline();

//go:cgo_import_dynamic x509_CFDataGetBytePtr CFDataGetBytePtr "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static uintptr CFDataGetBytePtr(CFRef data) {
    var ret = syscall(abi.FuncPCABI0(x509_CFDataGetBytePtr_trampoline), (uintptr)data, 0, 0, 0, 0, 0D);
    return ret;
}

internal static partial void x509_CFDataGetBytePtr_trampoline();

//go:cgo_import_dynamic x509_CFArrayGetCount CFArrayGetCount "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static nint CFArrayGetCount(CFRef Δarray) {
    var ret = syscall(abi.FuncPCABI0(x509_CFArrayGetCount_trampoline), (uintptr)Δarray, 0, 0, 0, 0, 0D);
    return (nint)ret;
}

internal static partial void x509_CFArrayGetCount_trampoline();

//go:cgo_import_dynamic x509_CFArrayGetValueAtIndex CFArrayGetValueAtIndex "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static CFRef CFArrayGetValueAtIndex(CFRef Δarray, nint index) {
    var ret = syscall(abi.FuncPCABI0(x509_CFArrayGetValueAtIndex_trampoline), (uintptr)Δarray, (uintptr)index, 0, 0, 0, 0D);
    return ((CFRef)ret);
}

internal static partial void x509_CFArrayGetValueAtIndex_trampoline();

//go:cgo_import_dynamic x509_CFEqual CFEqual "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static bool CFEqual(CFRef a, CFRef b) {
    var ret = syscall(abi.FuncPCABI0(x509_CFEqual_trampoline), (uintptr)a, (uintptr)b, 0, 0, 0, 0D);
    return ret == 1;
}

internal static partial void x509_CFEqual_trampoline();

//go:cgo_import_dynamic x509_CFRelease CFRelease "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static void CFRelease(CFRef @ref) {
    syscall(abi.FuncPCABI0(x509_CFRelease_trampoline), (uintptr)@ref, 0, 0, 0, 0, 0D);
}

internal static partial void x509_CFRelease_trampoline();

//go:cgo_import_dynamic x509_CFArrayCreateMutable CFArrayCreateMutable "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static CFRef CFArrayCreateMutable() {
    var ret = syscall(abi.FuncPCABI0(x509_CFArrayCreateMutable_trampoline), kCFAllocatorDefault, 0, 0, 0, 0, 0D);
    /* kCFTypeArrayCallBacks */
    return ((CFRef)ret);
}

internal static partial void x509_CFArrayCreateMutable_trampoline();

//go:cgo_import_dynamic x509_CFArrayAppendValue CFArrayAppendValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static void CFArrayAppendValue(CFRef Δarray, CFRef val) {
    syscall(abi.FuncPCABI0(x509_CFArrayAppendValue_trampoline), (uintptr)Δarray, (uintptr)val, 0, 0, 0, 0D);
}

internal static partial void x509_CFArrayAppendValue_trampoline();

//go:cgo_import_dynamic x509_CFDateCreate CFDateCreate "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static CFRef CFDateCreate(float64 seconds) {
    var ret = syscall(abi.FuncPCABI0(x509_CFDateCreate_trampoline), kCFAllocatorDefault, 0, 0, 0, 0, seconds);
    return ((CFRef)ret);
}

internal static partial void x509_CFDateCreate_trampoline();

//go:cgo_import_dynamic x509_CFErrorCopyDescription CFErrorCopyDescription "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static CFRef CFErrorCopyDescription(CFRef errRef) {
    var ret = syscall(abi.FuncPCABI0(x509_CFErrorCopyDescription_trampoline), (uintptr)errRef, 0, 0, 0, 0, 0D);
    return ((CFRef)ret);
}

internal static partial void x509_CFErrorCopyDescription_trampoline();

//go:cgo_import_dynamic x509_CFErrorGetCode CFErrorGetCode "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static nint CFErrorGetCode(CFRef errRef) {
    return (nint)syscall(abi.FuncPCABI0(x509_CFErrorGetCode_trampoline), (uintptr)errRef, 0, 0, 0, 0, 0D);
}

internal static partial void x509_CFErrorGetCode_trampoline();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string stringCanTBeRepresentedˢ = "string can't be represented as UTF-8"u8;

//go:cgo_import_dynamic x509_CFStringCreateExternalRepresentation CFStringCreateExternalRepresentation "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
public static (CFRef, error) CFStringCreateExternalRepresentation(CFRef strRef) {
    var ret = syscall(abi.FuncPCABI0(x509_CFStringCreateExternalRepresentation_trampoline), kCFAllocatorDefault, (uintptr)strRef, kCFStringEncodingUTF8, 0, 0, 0D);
    if (ret == 0) {
        return (0, errors.New(stringCanTBeRepresentedˢ));
    }
    return (((CFRef)ret), default!);
}

internal static partial void x509_CFStringCreateExternalRepresentation_trampoline();

// syscall is implemented in the runtime package (runtime/sys_darwin.go)
internal static partial uintptr syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, float64 f1);

// ReleaseCFArray iterates through an array, releasing its contents, and then
// releases the array itself. This is necessary because we cannot, easily, set the
// CFArrayCallBacks argument when creating CFArrays.
public static void ReleaseCFArray(CFRef Δarray) {
    for (nint i = 0; i < CFArrayGetCount(Δarray); i++) {
        var @ref = CFArrayGetValueAtIndex(Δarray, i);
        CFRelease(@ref);
    }
    CFRelease(Δarray);
}

} // end macOS_package
