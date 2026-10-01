// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using abi = @internal.abi_package;
using sysdll = @internal.syscall.windows.sysdll_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δmath = math_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall.windows;
using fs = global::go.io.fs_package;
using global::go.os;
using path;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

[GoType] partial struct DLL {
    public partial ref ж<syscall_package.DLL> ΔDLL { get; }
    internal ж<testing.T> t;
}

public static ж<DLL> GetDLL(ж<testing.T> Ꮡt, @string name) {
    var (d, e) = syscall.LoadDLL(name);
    if (e != default!) {
        Ꮡt.Fatal(e);
    }
    return Ꮡ(new DLL(ΔDLL: d, t: Ꮡt));
}

public static ж<syscall.Proc> Proc(this ж<DLL> Ꮡd, @string name) {
    ref var d = ref Ꮡd.DerefOrNull();

    var (p, e) = d.ΔDLL.FindProc(name);
    if (e != default!) {
        d.t.Fatal(e);
    }
    return p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string user32Dllˢ = "user32.dll"u8;
internal static readonly @string unionRectˢ = "UnionRect"u8;
internal static readonly object stdcallUser32UnionRectˢ = (@string)"stdcall USER32.UnionRect returns"u8;
internal static readonly object resˢ = (@string)"res="u8;

[GoType("dyn")] internal partial struct TestStdCall_Rect {
    internal int32 left, top, right, bottom;
}

public static void TestStdCall(ж<testing.T> Ꮡt) {
    ref var res = ref heap<TestStdCall_Rect>(out var Ꮡres);
    res = new TestStdCall_Rect(nil);
    var expected = new TestStdCall_Rect(1, 1, 40, 60);
    var (a, _, _) = GetDLL(Ꮡt, user32Dllˢ).Proc(unionRectˢ).Call(
        (uintptr)Ꮡres,
        (uintptr)Ꮡ(new TestStdCall_Rect(10, 1, 14, 60)),
        (uintptr)Ꮡ(new TestStdCall_Rect(1, 2, 40, 50)));
    if (a != 1 || res.left != expected.left || res.top != expected.top || res.right != expected.right || res.bottom != expected.bottom) {
        Ꮡt.Error(stdcallUser32UnionRectˢ, a, resˢ, res);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string verSetConditionMaskˢ = "VerSetConditionMask"u8;
internal static readonly @string verifyVersionInfoWˢ = "VerifyVersionInfoW"u8;

[GoType("dyn")] internal partial struct Test64BitReturnStdCall_OSVersionInfoEx {
    public uint32 OSVersionInfoSize;
    public uint32 MajorVersion;
    public uint32 MinorVersion;
    public uint32 BuildNumber;
    public uint32 PlatformId;
    public array<uint16> CSDVersion = new(128);
    public uint16 ServicePackMajor;
    public uint16 ServicePackMinor;
    public uint16 SuiteMask;
    public byte ProductType;
    public byte Reserve;
}

public static void Test64BitReturnStdCall(ж<testing.T> Ꮡt) {
    UntypedInt VER_BUILDNUMBER = 0x0000004;
    UntypedInt VER_MAJORVERSION = 0x0000002;
    UntypedInt VER_MINORVERSION = 0x0000001;
    UntypedInt VER_PLATFORMID = 0x0000008;
    UntypedInt VER_PRODUCT_TYPE = 0x0000080;
    UntypedInt VER_SERVICEPACKMAJOR = 0x0000020;
    UntypedInt VER_SERVICEPACKMINOR = 0x0000010;
    UntypedInt VER_SUITENAME = 0x0000040;
    UntypedInt VER_EQUAL = 1;
    UntypedInt VER_GREATER = 2;
    uintptr VER_GREATER_EQUAL = 3;
    UntypedInt VER_LESS = 4;
    UntypedInt VER_LESS_EQUAL = 5;
    syscall.Errno ERROR_OLD_WIN_VERSION = 1150;
    var d = GetDLL(Ꮡt, kernel32Dllˢ);
    uintptr m1 = default!;
    uintptr m2 = default!;
    var VerSetConditionMask = d.Proc(verSetConditionMaskˢ);
    (m1, m2, _) = VerSetConditionMask.Call(m1, m2, VER_MAJORVERSION, VER_GREATER_EQUAL);
    (m1, m2, _) = VerSetConditionMask.Call(m1, m2, VER_MINORVERSION, VER_GREATER_EQUAL);
    (m1, m2, _) = VerSetConditionMask.Call(m1, m2, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
    (m1, m2, _) = VerSetConditionMask.Call(m1, m2, VER_SERVICEPACKMINOR, VER_GREATER_EQUAL);
    ref var vi = ref heap<Test64BitReturnStdCall_OSVersionInfoEx>(out var Ꮡvi);
    vi = new Test64BitReturnStdCall_OSVersionInfoEx(
        MajorVersion: 5,
        MinorVersion: 1,
        ServicePackMajor: 2,
        ServicePackMinor: 0
    );
    vi.OSVersionInfoSize = (uint32)/* unsafe.Sizeof(vi) */ (uintptr)284;
    var (r, _, e2) = d.Proc(verifyVersionInfoWˢ).Call(
        (uintptr)Ꮡvi,
        (uintptr)((uintptr)(UntypedInt)((UntypedInt)(VER_MAJORVERSION | VER_MINORVERSION) | VER_SERVICEPACKMAJOR) | (uintptr)VER_SERVICEPACKMINOR),
        m1, m2);
    if (r == 0 && !AreEqual(e2, ERROR_OLD_WIN_VERSION)) {
        Ꮡt.Errorf("VerifyVersionInfo failed: %s"u8, e2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dDDˢ = "%d %d %d"u8;
internal static readonly @string wsprintfAˢ = "wsprintfA"u8;
internal static readonly object cdeclUser32WsprintfAˢ = (@string)"cdecl USER32.wsprintfA returns"u8;
internal static readonly object bufˢ = (@string)"buf="u8;

public static void TestCDecl(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new array<byte>(50), out var Ꮡbuf);
    var (fmtp, _) = syscall.BytePtrFromString(dDDˢ);
    var (a, _, _) = GetDLL(Ꮡt, user32Dllˢ).Proc(wsprintfAˢ).Call(
        (uintptr)Ꮡbuf.at<byte>(0),
        (uintptr)fmtp,
        1000, 2000, 3000);
    if (((sstring)(buf.slice(0, (nint)(a)))) != "1000 2000 3000"u8) {
        Ꮡt.Error(cdeclUser32WsprintfAˢ, a, bufˢ, buf.slice(0, (nint)(a)));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string isWindowˢ = "IsWindow"u8;
internal static readonly object lparamWasNotPassedToˢ = (@string)"lparam was not passed to callback"u8;
internal static readonly object user32IsWindowReturnsˢ = (@string)"USER32.IsWindow returns FALSE"u8;
internal static readonly @string enumWindowsˢ = "EnumWindows"u8;
internal static readonly object user32EnumWindowsReturnsˢ = (@string)"USER32.EnumWindows returns FALSE"u8;
internal static readonly object callbackHasBeenNeverˢ = (@string)"Callback has been never called or your have no windows"u8;

public static void TestEnumWindows(ж<testing.T> Ꮡt) {
    var d = GetDLL(Ꮡt, user32Dllˢ);
    var isWindows = d.Proc(isWindowˢ);
    nint counter = 0;
    var isWindowsʗ1 = isWindows;
    var cb = syscall.NewCallback(uintptr (syscallꓸHandle hwnd, uintptr lparam) => {
        if (lparam != 888) {
            Ꮡt.Error(lparamWasNotPassedToˢ);
        }
        var (b, _, _) = isWindowsʗ1.Call((uintptr)hwnd);
        if (b == 0) {
            Ꮡt.Error(user32IsWindowReturnsˢ);
        }
        counter++;
        return (uintptr)(1); // continue enumeration
    });
    var (a, _, _) = d.Proc(enumWindowsˢ).Call(cb, 888);
    if (a == 0) {
        Ꮡt.Error(user32EnumWindowsReturnsˢ);
    }
    if (counter == 0) {
        Ꮡt.Error(callbackHasBeenNeverˢ);
    }
}

internal static uintptr callback(@unsafe.Pointer timeFormatString, uintptr lparamʗp) {
    ref var lparam = ref heap(lparamʗp, out var Ꮡlparam);

    (Ꮡlparam.Reinterpret<uintptr, Action>()).ValueSlot();
    return 0; // stop enumeration
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string enumTimeFormatsExˢ = "EnumTimeFormatsEx"u8;

// nestedCall calls into Windows, back into Go, and finally to f.
internal static void nestedCall(ж<testing.T> Ꮡt, Action fʗp) {
    GoFrame ᒐ = default;
    try {
        ref var f = ref heap(fʗp, out var Ꮡf);

        var c = syscall.NewCallback(callback);
        var d = GetDLL(Ꮡt, kernel32Dllˢ);
        var dʗ1 = d;
        defer(() => dʗ1.Value.ΔDLL.Value.Release(), ref ᒐ);
        uintptr LOCALE_NAME_USER_DEFAULT = 0;
        d.Proc(enumTimeFormatsExˢ).Call(c, LOCALE_NAME_USER_DEFAULT, 0, (uintptr)(~Ꮡ(@unsafe.Pointer.OfFunc(f))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nestedCallDidNotCallFuncˢ = (@string)"nestedCall did not call func"u8;

public static void TestCallback(ж<testing.T> Ꮡt) {
    bool x = false;
    nestedCall(Ꮡt, () => {
        x = true;
    });
    if (!x) {
        Ꮡt.Fatal(nestedCallDidNotCallFuncˢ);
    }
}

public static void TestCallbackGC(ж<testing.T> Ꮡt) {
    nestedCall(Ꮡt, Δruntime.GC);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object runtimeLockOSThreadDidnTˢ = (@string)"runtime.LockOSThread didn't"u8;
internal static readonly object wrongPanicˢ = (@string)"wrong panic:"u8;
internal static readonly object lostLockOnOsThreadAfterˢ = (@string)"lost lock on OS thread after panic"u8;

public static void TestCallbackPanicLocked(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Δruntime.LockOSThread();
        defer(Δruntime.UnlockOSThread, ref ᒐ);
        if (!runtime_internal_test_package.LockedOSThread()) {
            Ꮡt.Fatal(runtimeLockOSThreadDidnTˢ);
        }
        defer(() => {
            var s = recover();
            if (s == default!) {
                Ꮡt.Fatal(didNotPanicˢ);
            }
            if (s._<@string>() != "callback panic"u8) {
                Ꮡt.Fatal(wrongPanicˢ, s);
            }
            if (!runtime_internal_test_package.LockedOSThread()) {
                Ꮡt.Fatal(lostLockOnOsThreadAfterˢ);
            }
        }, ref ᒐ);
        nestedCall(Ꮡt, () => {
            throw panic("callback panic");
        });
        throw panic("nestedCall returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lockedOsThreadOnEntryToˢ = (@string)"locked OS thread on entry to TestCallbackPanic"u8;
internal static readonly object lockedOsThreadOnExitFromˢ = (@string)"locked OS thread on exit from TestCallbackPanic"u8;

public static void TestCallbackPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Make sure panic during callback unwinds properly.
        if (runtime_internal_test_package.LockedOSThread()) {
            Ꮡt.Fatal(lockedOsThreadOnEntryToˢ);
        }
        defer(() => {
            var s = recover();
            if (s == default!) {
                Ꮡt.Fatal(didNotPanicˢ);
            }
            if (s._<@string>() != "callback panic"u8) {
                Ꮡt.Fatal(wrongPanicˢ, s);
            }
            if (runtime_internal_test_package.LockedOSThread()) {
                Ꮡt.Fatal(lockedOsThreadOnExitFromˢ);
            }
        }, ref ᒐ);
        nestedCall(Ꮡt, () => {
            throw panic("callback panic");
        });
        throw panic("nestedCall returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCallbackPanicLoop(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Make sure we don't blow out m->g0 stack.
    for (nint i = 0; i < 100000; i++) {
        TestCallbackPanic(Ꮡt);
    }
}

public static void TestBlockingCallback(ж<testing.T> Ꮡt) {
    var c = new channel<nint>(0);
    var cʗ1 = c;
    goǃ(() => {
        for (nint i = 0; i < 10; i++) {
            cʗ1.ᐸꟷ(ᐸꟷ(cʗ1));
        }
    });
    var cʗ2 = c;
    nestedCall(Ꮡt, () => {
        for (nint i = 0; i < 10; i++) {
            cʗ2.ᐸꟷ(i);
            {
                nint j = ᐸꟷ(cʗ2); if (j != i) {
                    Ꮡt.Errorf("out of sync %d != %d"u8, j, i);
                }
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createThreadˢ = "CreateThread"u8;
internal static readonly @string getExitCodeThreadˢ = "GetExitCodeThread"u8;

public static void TestCallbackInAnotherThread(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var d = GetDLL(Ꮡt, kernel32Dllˢ);
        var f = (uintptr p) => p;
        var (r, _, err) = d.Proc(createThreadˢ).Call(0, 0, syscall.NewCallback((f).OrTypedNilFunc()), 123, 0, 0);
        if (r == 0) {
            Ꮡt.Fatalf("CreateThread failed: %v"u8, err);
        }
        var h = ((syscallꓸHandle)r);
        defer(syscall.CloseHandle, h, ref ᒐ);
        {
            var (s, errΔ1) = syscall.WaitForSingleObject(h, syscall.INFINITE);
            var exprᴛ1 = s;
            if (exprᴛ1 == syscall.WAIT_OBJECT_0) {
                do {
                    break;
                } while (false);
            }
            else if (exprᴛ1 == syscall.WAIT_FAILED) {
                Ꮡt.Fatalf("WaitForSingleObject failed: %v"u8, errΔ1);
            }
            else { /* default: */
                Ꮡt.Fatalf("WaitForSingleObject returns unexpected value %v"u8, s);
            }
        }

        ref var ec = ref heap(new uint32(), out var Ꮡec);
        (r, _, err) = d.Proc(getExitCodeThreadˢ).Call((uintptr)h, (uintptr)Ꮡec);
        if (r == 0) {
            Ꮡt.Fatalf("GetExitCodeThread failed: %v"u8, err);
        }
        if (ec != 123) {
            Ꮡt.Fatalf("expected 123, but got %d"u8, ec);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct cbFunc {
    internal any goFunc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stdcallˢ = "stdcall"u8;
internal static readonly @string cdeclˢ = "cdecl"u8;

internal static @string cName(this cbFunc f, bool cdecl) {
    @string name = stdcallˢ;
    if (cdecl) {
        name = cdeclˢ;
    }
    var t = reflect.TypeOf(f.goFunc);
    for (nint i = 0; i < t.NumIn(); i++) {
        name += "_"u8 + t.In(i).Name();
    }
    return name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stdcallˢ2 = "__stdcall"u8;
internal static readonly @string cdeclˢ2 = "__cdecl"u8;

internal static void cSrc(this cbFunc f, Δio.Writer w, bool cdecl) {
    // Construct a C function that takes a callback with
    // f.goFunc's signature, and calls it with integers 1..N.
    @string funcname = f.cName(cdecl);
    @string attr = stdcallˢ2;
    if (cdecl) {
        attr = cdeclˢ2;
    }
    @string typename = "t"u8 + funcname;
    var t = reflect.TypeOf(f.goFunc);
    var cTypes = new slice<@string>(t.NumIn());
    var cArgs = new slice<@string>(t.NumIn());
    foreach (var (i, _) in cTypes) {
        // We included stdint.h, so this works for all sized
        // integer types, and uint8Pair_t.
        cTypes[i] = t.In(i).Name() + "_t"u8;
        if (t.In(i).Name() == "uint8Pair"u8){
            cArgs[i] = fmt.Sprintf("(uint8Pair_t){%d,1}"u8, i);
        } else {
            cArgs[i] = fmt.Sprintf("%d"u8, i + 1);
        }
    }
    fmt.Fprintf(w, """

typedef uintptr_t %s (*%s)(%s);
uintptr_t %s(%s f) {
	return f(%s);
}
	
"""u8, attr, typename, strings.Join(cTypes, ","u8), funcname, typename, strings.Join(cArgs, ","u8));
}

internal static void testOne(this cbFunc f, ж<testing.T> Ꮡt, ж<syscall.DLL> Ꮡdll, bool cdecl, uintptr cb) {
    var (r1, _, _) = Ꮡdll.MustFindProc(f.cName(cdecl)).Call(cb);
    nint want = 0;
    for (nint i = 0; i < reflect.TypeOf(f.goFunc).NumIn(); i++) {
        want += i + 1;
    }
    if ((nint)r1 != want) {
        Ꮡt.Errorf("wanted result %d; got %d"u8, want, r1);
    }
}

[GoType] partial struct uint8Pair {
    internal uint8 x, y;
}

// Non-uintptr parameters.
internal static slice<cbFunc> cbFuncs = new cbFunc[]{
    new((uintptr i1, uintptr i2) => i1 + i2),
    new((uintptr i1, uintptr i2, uintptr i3) => i1 + i2 + i3),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4) => i1 + i2 + i3 + i4),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5) => i1 + i2 + i3 + i4 + i5),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6) => i1 + i2 + i3 + i4 + i5 + i6),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7) => i1 + i2 + i3 + i4 + i5 + i6 + i7),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7, uintptr i8) => i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8),
    new((uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7, uintptr i8, uintptr i9) => i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9),
    new((uint8 i1, uint8 i2, uint8 i3, uint8 i4, uint8 i5, uint8 i6, uint8 i7, uint8 i8, uint8 i9) => (uintptr)((uint8)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9))),
    new((uint16 i1, uint16 i2, uint16 i3, uint16 i4, uint16 i5, uint16 i6, uint16 i7, uint16 i8, uint16 i9) => (uintptr)((uint16)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9))),
    new((int8 i1, int8 i2, int8 i3, int8 i4, int8 i5, int8 i6, int8 i7, int8 i8, int8 i9) => (uintptr)((int8)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9))),
    new((int8 i1, int16 i2, int32 i3, uintptr i4, uintptr i5) => (uintptr)i1 + (uintptr)i2 + (uintptr)i3 + i4 + i5),
    new((uint8Pair i1, uint8Pair i2, uint8Pair i3, uint8Pair i4, uint8Pair i5) => (uintptr)((uint8)(i1.x + i1.y + i2.x + i2.y + i3.x + i3.y + i4.x + i4.y + i5.x + i5.y))),
    new((uint32 i1, uint32 i2, uint32 i3, uint32 i4, uint32 i5, uint32 i6, uint32 i7, uint32 i8, uint32 i9) => {
        Δruntime.GC();
        return (uintptr)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9);
    })
}.slice();

//go:registerparams
internal static uintptr sum2(uintptr i1, uintptr i2) {
    return i1 + i2;
}

//go:registerparams
internal static uintptr sum3(uintptr i1, uintptr i2, uintptr i3) {
    return i1 + i2 + i3;
}

//go:registerparams
internal static uintptr sum4(uintptr i1, uintptr i2, uintptr i3, uintptr i4) {
    return i1 + i2 + i3 + i4;
}

//go:registerparams
internal static uintptr sum5(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5) {
    return i1 + i2 + i3 + i4 + i5;
}

//go:registerparams
internal static uintptr sum6(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6) {
    return i1 + i2 + i3 + i4 + i5 + i6;
}

//go:registerparams
internal static uintptr sum7(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7) {
    return i1 + i2 + i3 + i4 + i5 + i6 + i7;
}

//go:registerparams
internal static uintptr sum8(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7, uintptr i8) {
    return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8;
}

//go:registerparams
internal static uintptr sum9(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7, uintptr i8, uintptr i9) {
    return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9;
}

//go:registerparams
internal static uintptr sum10(uintptr i1, uintptr i2, uintptr i3, uintptr i4, uintptr i5, uintptr i6, uintptr i7, uintptr i8, uintptr i9, uintptr i10) {
    return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9 + i10;
}

//go:registerparams
internal static uintptr sum9uint8(uint8 i1, uint8 i2, uint8 i3, uint8 i4, uint8 i5, uint8 i6, uint8 i7, uint8 i8, uint8 i9) {
    return (uintptr)((uint8)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9));
}

//go:registerparams
internal static uintptr sum9uint16(uint16 i1, uint16 i2, uint16 i3, uint16 i4, uint16 i5, uint16 i6, uint16 i7, uint16 i8, uint16 i9) {
    return (uintptr)((uint16)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9));
}

//go:registerparams
internal static uintptr sum9int8(int8 i1, int8 i2, int8 i3, int8 i4, int8 i5, int8 i6, int8 i7, int8 i8, int8 i9) {
    return (uintptr)((int8)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9));
}

//go:registerparams
internal static uintptr sum5mix(int8 i1, int16 i2, int32 i3, uintptr i4, uintptr i5) {
    return (uintptr)i1 + (uintptr)i2 + (uintptr)i3 + i4 + i5;
}

//go:registerparams
internal static uintptr sum5andPair(uint8Pair i1, uint8Pair i2, uint8Pair i3, uint8Pair i4, uint8Pair i5) {
    return (uintptr)((uint8)(i1.x + i1.y + i2.x + i2.y + i3.x + i3.y + i4.x + i4.y + i5.x + i5.y));
}

// This test forces a GC. The idea is to have enough arguments
// that insufficient spill slots allocated (according to the ABI)
// may cause compiler-generated spills to clobber the return PC.
// Then, the GC stack scanning will catch that.
//
//go:registerparams
internal static uintptr sum9andGC(uint32 i1, uint32 i2, uint32 i3, uint32 i4, uint32 i5, uint32 i6, uint32 i7, uint32 i8, uint32 i9) {
    Δruntime.GC();
    return (uintptr)(i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9);
}

// TODO(register args): Remove this once we switch to using the register
// calling convention by default, since this is redundant with the existing
// tests.
internal static slice<cbFunc> cbFuncsRegABI = new cbFunc[]{
    new(sum2),
    new(sum3),
    new(sum4),
    new(sum5),
    new(sum6),
    new(sum7),
    new(sum8),
    new(sum9),
    new(sum10),
    new(sum9uint8),
    new(sum9uint16),
    new(sum9int8),
    new(sum5mix),
    new(sum5andPair),
    new(sum9andGC)
}.slice();

internal static slice<cbFunc> getCallbackTestFuncs() {
    {
        nint regs = runtime_internal_test_package.SetIntArgRegs(-1); if (regs > 0) {
            return cbFuncsRegABI;
        }
    }
    return cbFuncs;
}

[GoType] partial struct cbDLL {
    internal @string name;
    internal Func<@string, @string, slice<@string>> buildArgs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object includeStdintHTypedefˢ = (@string)"""

#include <stdint.h>
typedef struct { uint8_t x, y; } uint8Pair_t;

"""u8;

[GoRecv] internal static void makeSrc(this ref cbDLL d, ж<testing.T> Ꮡt, @string path) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Δos.Create(path);
        if (err != default!) {
            Ꮡt.Fatalf("failed to create source file: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        fmt.Fprint(new Δos.FileжWriter(f), includeStdintHTypedefˢ);
        foreach (var (_, cbf) in getCallbackTestFuncs()) {
            cbf.cSrc(new Δos.FileжWriter(f), false);
            cbf.cSrc(new Δos.FileжWriter(f), true);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoRecv] internal static @string build(this ref cbDLL d, ж<testing.T> Ꮡt, @string dir) {
    @string srcname = d.name + ".c"u8;
    d.makeSrc(Ꮡt, filepath.Join(dir, srcname));
    @string outname = d.name + ".dll"u8;
    var args = d.buildArgs(outname, srcname);
    var cmd = exec.Command(args[0], args[1..].ꓸꓸꓸ);
    cmd.Value.Dir = dir;
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
    }
    return filepath.Join(dir, outname);
}

internal static slice<cbDLL> cbDLLs = new cbDLL[]{
    new(
        "test"u8,
        (@string @out, @string src) => new @string[]{"gcc"u8, "-shared"u8, "-s"u8, "-Werror"u8, "-o"u8, @out, src}.slice()
    ),
    new(
        "testO2"u8,
        (@string @out, @string src) => new @string[]{"gcc"u8, "-shared"u8, "-s"u8, "-Werror"u8, "-o"u8, @out, "-O2"u8, src}.slice()
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestGccIsMissingˢ = (@string)"skipping test: gcc is missing"u8;

public static void TestStdcallAndCDeclCallbacks(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, err) = exec.LookPath(gccˢ); if (err != default!) {
                Ꮡt.Skip(skippingTestGccIsMissingˢ);
            }
        }
        @string tmp = Ꮡt.TempDir();
        nint oldRegs = runtime_internal_test_package.SetIntArgRegs(abi.IntArgRegs);
        defer(runtime_internal_test_package.SetIntArgRegs, oldRegs, ref ᒐ);
        foreach (var (_, vᴛ1) in cbDLLs) {
            ref var dll = ref heap(new cbDLL(), out var Ꮡdll);
            dll = vᴛ1;

            Ꮡt.Run(dll.name, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    @string dllPath = Ꮡdll.Value.build(tΔ1, tmp);
                    var dllΔ1 = syscall.MustLoadDLL(dllPath);
                    var dllʗ1 = dllΔ1;
                    defer(() => dllʗ1.Release(), ref ᒐ);
                    foreach (var (_, vᴛ2) in getCallbackTestFuncs()) {
                        ref var cbf = ref heap(new cbFunc(), out var Ꮡcbf);
                        cbf = vᴛ2;

                        var cbfʗ1 = cbf;
                        var dllʗ2 = dllΔ1;
                        tΔ1.Run(cbf.cName(false), (ж<testing.T> tΔ2) => {
                            var stdcall = syscall.NewCallback(cbfʗ1.goFunc);
                            cbfʗ1.testOne(tΔ2, dllʗ2, false, stdcall);
                        });
                        var cbfʗ2 = cbf;
                        var dllʗ3 = dllΔ1;
                        tΔ1.Run(cbf.cName(true), (ж<testing.T> tΔ3) => {
                            var cdecl = syscall.NewCallbackCDecl(cbfʗ2.goFunc);
                            cbfʗ2.testOne(tΔ3, dllʗ3, true, cdecl);
                        });
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getModuleHandleWˢ = "GetModuleHandleW"u8;
internal static readonly object callbackShouldNeverGetˢ = (@string)"callback should never get called"u8;
internal static readonly @string testWindowˢ = "test_window"u8;
internal static readonly @string registerClassExWˢ = "RegisterClassExW"u8;
internal static readonly @string unregisterClassWˢ = "UnregisterClassW"u8;

[GoType("dyn")] internal partial struct TestRegisterClass_Wndclassex {
    public uint32 Size;
    public uint32 Style;
    public uintptr WndProc;
    public int32 ClsExtra;
    public int32 WndExtra;
    public syscallꓸHandle Instance;
    public syscallꓸHandle Icon;
    public syscallꓸHandle Cursor;
    public syscallꓸHandle Background;
    public ж<uint16> MenuName;
    public ж<uint16> ClassName;
    public syscallꓸHandle IconSm;
}

public static void TestRegisterClass(ж<testing.T> Ꮡt) {
    var kernel32 = GetDLL(Ꮡt, kernel32Dllˢ);
    var user32 = GetDLL(Ꮡt, user32Dllˢ);
    var (mh, _, _) = kernel32.Proc(getModuleHandleWˢ).Call(0);
    var cb = syscall.NewCallback(uintptr (syscallꓸHandle hwnd, uint32 msg, uintptr wparam, uintptr lparam) => {
        Ꮡt.Fatal(callbackShouldNeverGetˢ);
        return (uintptr)(0);
    });
    var name = syscall.StringToUTF16Ptr(testWindowˢ);
    ref var wc = ref heap<TestRegisterClass_Wndclassex>(out var Ꮡwc);
    wc = new TestRegisterClass_Wndclassex(
        WndProc: cb,
        Instance: ((syscallꓸHandle)mh),
        ClassName: name
    );
    wc.Size = (uint32)/* unsafe.Sizeof(wc) */ (uintptr)80;
    var (a, _, err) = user32.Proc(registerClassExWˢ).Call((uintptr)Ꮡwc);
    if (a == 0) {
        Ꮡt.Fatalf("RegisterClassEx failed: %v"u8, err);
    }
    (var r, _, err) = user32.Proc(unregisterClassWˢ).Call((uintptr)name, 0);
    if (r == 0) {
        Ꮡt.Fatalf("UnregisterClass failed: %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testingOutputDebugStringˢ = "testing OutputDebugString"u8;
internal static readonly @string outputDebugStringWˢ = "OutputDebugStringW"u8;

public static void TestOutputDebugString(ж<testing.T> Ꮡt) {
    var d = GetDLL(Ꮡt, kernel32Dllˢ);
    var p = syscall.StringToUTF16Ptr(testingOutputDebugStringˢ);
    d.Proc(outputDebugStringWˢ).Call((uintptr)p);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string raiseExceptionˢ = "RaiseException"u8;
internal static readonly @string raiseExceptionShouldNotˢ = "RaiseException should not return"u8;
internal static readonly @string exception0xbadˢ = "Exception 0xbad"u8;

public static void TestRaiseException(ж<testing.T> Ꮡt) {
    if (strings.HasPrefix(testenv.Builder(), windowsAmd642012ˢ)) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 49681);
    }
    @string o = runTestProg(Ꮡt, testprogˢ, raiseExceptionˢ);
    if (strings.Contains(o, raiseExceptionShouldNotˢ)) {
        Ꮡt.Fatalf("RaiseException did not crash program: %v"u8, o);
    }
    if (!strings.Contains(o, exception0xbadˢ)) {
        Ꮡt.Fatalf("No stack trace: %v"u8, o);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroDivisionExceptionˢ = "ZeroDivisionException"u8;
internal static readonly @string panicRuntimeErrorIntegerˢ = "panic: runtime error: integer divide by zero"u8;

public static void TestZeroDivisionException(ж<testing.T> Ꮡt) {
    @string o = runTestProg(Ꮡt, testprogˢ, zeroDivisionExceptionˢ);
    if (!strings.Contains(o, panicRuntimeErrorIntegerˢ)) {
        Ꮡt.Fatalf("No stack trace: %v"u8, o);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testWerDialogueˢ = "TEST_WER_DIALOGUE"u8;
internal static readonly @string testRunTestWERDialogueˢ = "-test.run=TestWERDialogue"u8;
internal static readonly object testProgramSucceededˢ = (@string)"test program succeeded unexpectedly"u8;

public static void TestWERDialogue(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δos.Getenv(testWerDialogueˢ) == "1"u8) {
        uintptr EXCEPTION_NONCONTINUABLE = 1;
        var mod = syscall.MustLoadDLL(kernel32Dllˢ);
        var proc = mod.MustFindProc(raiseExceptionˢ);
        proc.Call(0xbad, EXCEPTION_NONCONTINUABLE, 0, 0);
        Ꮡt.Fatal(raiseExceptionShouldNotˢ);
    }
    var (exe, err) = Δos.Executable();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), exe, testRunTestWERDialogueˢ));
    cmd.Value.Env = append((~cmd).Env, "TEST_WER_DIALOGUE=1"u8, "GOTRACEBACK=wer");
    // Child process should not open WER dialogue, but return immediately instead.
    // The exit code can't be reliably tested here because Windows can change it.
    (_, err) = cmd.CombinedOutput();
    if (err == default!) {
        Ꮡt.Error(testProgramSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackMemoryˢ = "StackMemory"u8;

public static void TestWindowsStackMemory(ж<testing.T> Ꮡt) {
    @string o = runTestProg(Ꮡt, testprogˢ, stackMemoryˢ);
    var (stackUsage, err) = strconv.Atoi(o);
    if (err != default!) {
        Ꮡt.Fatalf("Failed to read stack usage: %v"u8, err);
    }
    {
        nint expected = (128 << (int)(10));
        nint got = stackUsage; if (got > expected) {
            Ꮡt.Fatalf("expected < %d bytes of memory per thread, got %d"u8, expected, got);
        }
    }
}

internal static byte used;

internal static void use(slice<byte> buf) {
    foreach (var (_, c) in buf) {
        used += c;
    }
}

internal static nint /*r*/ forceStackCopy() {
    nint r = default!;

    ref var f = ref heap<Func<nint, nint>>(out var Ꮡf);
    f = (nint i) => {
        array<byte> buf = new(256);
        use(buf[..]);
        if (i == 0) {
            return 0;
        }
        return i + Ꮡf.ValueSlot(i - 1);
    };
    r = f(128);
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mydllCˢ = "mydll.c"u8;
internal static readonly @string mydllDllˢ = "mydll.dll"u8;
internal static readonly @string werrorˢ = "-Werror"u8;
internal static readonly @string cfuncˢ = "cfunc"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = """

#include <stdint.h>
#include <windows.h>

typedef uintptr_t __stdcall (*callback)(uintptr_t);

uintptr_t cfunc(callback f, uintptr_t n) {
   uintptr_t r;
   r = f(n);
   SetLastError(333);
   return r;
}

"""u8;

// Use a new goroutine so that we get a small stack.
[GoType("dyn")] internal partial struct TestReturnAfterStackGrowInCallback_result {
    internal uintptr r;
    internal syscall.Errno err;
}

public static void TestReturnAfterStackGrowInCallback(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, errΔ1) = exec.LookPath(gccˢ); if (errΔ1 != default!) {
                Ꮡt.Skip(skippingTestGccIsMissingˢ);
            }
        }
        @string src = srcᶜ;
        @string tmpdir = Ꮡt.TempDir();
        @string srcname = mydllCˢ;
        var err = Δos.WriteFile(filepath.Join(tmpdir, srcname), slice<byte>(src), 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string outname = mydllDllˢ;
        var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", outname, srcname);
        cmd.Value.Dir = tmpdir;
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
        }
        @string dllpath = filepath.Join(tmpdir, outname);
        var dll = syscall.MustLoadDLL(dllpath);
        var dllʗ1 = dll;
        defer(() => dllʗ1.Release(), ref ᒐ);
        var proc = dll.MustFindProc(cfuncˢ);
        var cb = syscall.NewCallback(uintptr (uintptr n) => {
            forceStackCopy();
            return n;
        });
        ref var want = ref heap<TestReturnAfterStackGrowInCallback_result>(out var Ꮡwant);
        want = new TestReturnAfterStackGrowInCallback_result( // Make it large enough to test issue #29331.

            r: ((~(uintptr)0) >> (int)(24)),
            err: 333
        );
        var c = new channel<TestReturnAfterStackGrowInCallback_result>(0);
        var cʗ1 = c;
        var procʗ1 = proc;
        var wantʗ1 = want;
        goǃ(() => {
            var (r, _, errΔ2) = procʗ1.Call(cb, wantʗ1.r);
            cʗ1.ᐸꟷ(new TestReturnAfterStackGrowInCallback_result(r, errΔ2._<syscall.Errno>()));
        });
        {
            var got = ᐸꟷ(c); if (got != want) {
                Ꮡt.Errorf("got %d want %d"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSyscallN(ж<testing.T> Ꮡt) {
    {
        var (_, err) = exec.LookPath(gccˢ); if (err != default!) {
            Ꮡt.Skip(skippingTestGccIsMissingˢ);
        }
    }
    if (Δruntime.GOARCH != "amd64"u8) {
        Ꮡt.Skipf("skipping test: GOARCH=%s"u8, Δruntime.GOARCH);
    }
    for (nint arglen = 0; arglen <= runtime_internal_test_package.MaxArgs; arglen++) {
        nint arglenΔ1 = arglen;
        Ꮡt.Run(fmt.Sprintf("arg-%d"u8, arglenΔ1), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();
                var args = new slice<@string>(arglenΔ1);
                var rets = new slice<@string>(arglenΔ1 + 1);
                var @params = new slice<uintptr>(arglenΔ1);
                foreach (var (i, _) in args) {
                    args[i] = fmt.Sprintf("int a%d"u8, i);
                    rets[i] = fmt.Sprintf("(a%d == %d)"u8, i, i);
                    @params[i] = (uintptr)i;
                }
                rets[arglenΔ1] = "1"u8; // for arglen == 0
                @string src = fmt.Sprintf("""

		#include <stdint.h>
		#include <windows.h>
		int cfunc(%s) { return %s; }
"""u8, strings.Join(args, ", "u8), strings.Join(rets, " && "u8));
                @string tmpdir = tΔ1.TempDir();
                @string srcname = mydllCˢ;
                var err = Δos.WriteFile(filepath.Join(tmpdir, srcname), slice<byte>(src), 0);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                @string outname = mydllDllˢ;
                var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", outname, srcname);
                cmd.Value.Dir = tmpdir;
                (var @out, err) = cmd.CombinedOutput();
                if (err != default!) {
                    tΔ1.Fatalf("failed to build dll: %v\n%s"u8, err, @out);
                }
                @string dllpath = filepath.Join(tmpdir, outname);
                var dll = syscall.MustLoadDLL(dllpath);
                var dllʗ1 = dll;
                defer(() => dllʗ1.Release(), ref ᒐ);
                var proc = dll.MustFindProc(cfuncˢ);
                // proc.Call() will call SyscallN() internally.
                (var r, _, err) = proc.Call(@params.ꓸꓸꓸ);
                if (r != 1) {
                    tΔ1.Errorf("got %d want 1 (err=%v)"u8, r, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = """

#include <stdint.h>
#include <windows.h>

uintptr_t cfunc(uintptr_t a, double b, float c, double d) {
	if (a == 1 && b == 2.2 && c == 3.3f && d == 4.4e44) {
		return 1;
	}
	return 0;
}

"""u8;

public static void TestFloatArgs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, errΔ1) = exec.LookPath(gccˢ); if (errΔ1 != default!) {
                Ꮡt.Skip(skippingTestGccIsMissingˢ);
            }
        }
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skipf("skipping test: GOARCH=%s"u8, Δruntime.GOARCH);
        }
        @string src = srcᶜ1;
        @string tmpdir = Ꮡt.TempDir();
        @string srcname = mydllCˢ;
        var err = Δos.WriteFile(filepath.Join(tmpdir, srcname), slice<byte>(src), 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string outname = mydllDllˢ;
        var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", outname, srcname);
        cmd.Value.Dir = tmpdir;
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
        }
        @string dllpath = filepath.Join(tmpdir, outname);
        var dll = syscall.MustLoadDLL(dllpath);
        var dllʗ1 = dll;
        defer(() => dllʗ1.Release(), ref ᒐ);
        var proc = dll.MustFindProc(cfuncˢ);
        (var r, _, err) = proc.Call(
            1,
            (uintptr)Δmath.Float64bits(2.2D),
            (uintptr)Δmath.Float32bits(3.3F),
            (uintptr)Δmath.Float64bits(4.4e44D));
        if (r != 1) {
            Ꮡt.Errorf("got %d want 1 (err=%v)"u8, r, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cfuncFloatˢ = "cfuncFloat"u8;
internal static readonly @string cfuncDoubleˢ = "cfuncDouble"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ2 = """

#include <stdint.h>
#include <windows.h>

float cfuncFloat(uintptr_t a, double b, float c, double d) {
	if (a == 1 && b == 2.2 && c == 3.3f && d == 4.4e44) {
		return 1.5f;
	}
	return 0;
}

double cfuncDouble(uintptr_t a, double b, float c, double d) {
	if (a == 1 && b == 2.2 && c == 3.3f && d == 4.4e44) {
		return 2.5;
	}
	return 0;
}

"""u8;

public static void TestFloatReturn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, errΔ1) = exec.LookPath(gccˢ); if (errΔ1 != default!) {
                Ꮡt.Skip(skippingTestGccIsMissingˢ);
            }
        }
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skipf("skipping test: GOARCH=%s"u8, Δruntime.GOARCH);
        }
        @string src = srcᶜ2;
        @string tmpdir = Ꮡt.TempDir();
        @string srcname = mydllCˢ;
        var err = Δos.WriteFile(filepath.Join(tmpdir, srcname), slice<byte>(src), 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string outname = mydllDllˢ;
        var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", outname, srcname);
        cmd.Value.Dir = tmpdir;
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
        }
        @string dllpath = filepath.Join(tmpdir, outname);
        var dll = syscall.MustLoadDLL(dllpath);
        var dllʗ1 = dll;
        defer(() => dllʗ1.Release(), ref ᒐ);
        var proc = dll.MustFindProc(cfuncFloatˢ);
        (_, var r, err) = proc.Call(
            1,
            (uintptr)Δmath.Float64bits(2.2D),
            (uintptr)Δmath.Float32bits(3.3F),
            (uintptr)Δmath.Float64bits(4.4e44D));
        var fr = Δmath.Float32frombits((uint32)r);
        if (fr != 1.5F) {
            Ꮡt.Errorf("got %f want 1.5 (err=%v)"u8, fr, err);
        }
        proc = dll.MustFindProc(cfuncDoubleˢ);
        (_, r, err) = proc.Call(
            1,
            (uintptr)Δmath.Float64bits(2.2D),
            (uintptr)Δmath.Float32bits(3.3F),
            (uintptr)Δmath.Float64bits(4.4e44D));
        var dr = Δmath.Float64frombits((uint64)r);
        if (dr != 2.5D) {
            Ꮡt.Errorf("got %f want 2.5 (err=%v)"u8, dr, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTimeBeginPeriod(ж<testing.T> Ꮡt) {
    const uint32 TIMERR_NOERROR = 0;
    if (runtime_internal_test_package.TimeBeginPeriodRetValue.Value != TIMERR_NOERROR) {
        Ꮡt.Fatalf("timeBeginPeriod failed: it returned %d"u8, runtime_internal_test_package.TimeBeginPeriodRetValue.Value);
    }
}

// removeOneCPU removes one (any) cpu from affinity mask.
// It returns new affinity mask.
internal static (uintptr, error) removeOneCPU(uintptr mask) {
    if (mask == 0) {
        return (0, fmt.Errorf("cpu affinity mask is empty"u8));
    }
    nint maskbits = (nint)(/* unsafe.Sizeof(mask) */ (uintptr)8 * 8);
    for (nint i = 0; i < maskbits; i++) {
        var newmask = (uintptr)(mask & ~(((uintptr)1).Lsh((nuint)i)));
        if (newmask != mask) {
            return (newmask, default!);
        }
    }
    throw panic("not reached");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openThreadˢ = "OpenThread"u8;
internal static readonly @string resumeThreadˢ = "ResumeThread"u8;
internal static readonly @string thread32Firstˢ = "Thread32First"u8;
internal static readonly @string thread32Nextˢ = "Thread32Next"u8;

[GoType("dyn")] internal partial struct resumeChildThread_ThreadEntry32 {
    public uint32 Size;
    internal uint32 tUsage;
    public uint32 ThreadID;
    public uint32 OwnerProcessID;
    public int32 BasePri;
    public int32 DeltaPri;
    public uint32 Flags;
}

internal static error resumeChildThread(ж<syscall.DLL> Ꮡkernel32, nint childpid) {
    GoFrame ᒐ = default;
    try {
        var _OpenThread = Ꮡkernel32.MustFindProc(openThreadˢ);
        var _ResumeThread = Ꮡkernel32.MustFindProc(resumeThreadˢ);
        var _Thread32First = Ꮡkernel32.MustFindProc(thread32Firstˢ);
        var _Thread32Next = Ꮡkernel32.MustFindProc(thread32Nextˢ);
        var (snapshot, err) = syscall.CreateToolhelp32Snapshot(syscall.TH32CS_SNAPTHREAD, 0);
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, snapshot, ref ᒐ);
        uintptr _THREAD_SUSPEND_RESUME = 0x0002;
        ref var te = ref heap(new resumeChildThread_ThreadEntry32(), out var Ꮡte);
        te.Size = (uint32)/* unsafe.Sizeof(te) */ (uintptr)28;
        (var ret, _, err) = _Thread32First.Call((uintptr)snapshot, (uintptr)Ꮡte);
        if (ret == 0) {
            return err;
        }
        while (te.OwnerProcessID != (uint32)childpid) {
            (ret, _, err) = _Thread32Next.Call((uintptr)snapshot, (uintptr)Ꮡte);
            if (ret == 0) {
                return err;
            }
        }
        (var h, _, err) = _OpenThread.Call(_THREAD_SUSPEND_RESUME, 1, (uintptr)te.ThreadID);
        if (h == 0) {
            return err;
        }
        defer(syscall.Close, ((syscallꓸHandle)h), ref ᒐ);
        (ret, _, err) = _ResumeThread.Call(h);
        if (ret == 0xffffffffU) {
            return err;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string getProcessAffinityMaskˢ = "GetProcessAffinityMask"u8;
internal static readonly @string setProcessAffinityMaskˢ = "SetProcessAffinityMask"u8;
internal static readonly @string testRunTestNumCPUˢ = "-test.run=TestNumCPU"u8;

public static void TestNumCPU(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
            // in child process
            fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "%d"u8, Δruntime.NumCPU());
            Δos.Exit(0);
        }
        {
            var n = runtime_internal_test_package.NumberOfProcessors();
            switch (ᐧ) {
            case {} when n is < 1: {
                Ꮡt.Fatalf("system cannot have %d cpu(s)"u8, n);
                break;
            }
            case {} when n is 1: {
                if (Δruntime.NumCPU() != 1) {
                    Ꮡt.Fatalf("runtime.NumCPU() returns %d on single cpu system"u8, Δruntime.NumCPU());
                }
                return;
            }}
        }

        const uint32 _CREATE_SUSPENDED = 0x00000004;
        const uint32 _PROCESS_ALL_ACCESS = /* syscall.STANDARD_RIGHTS_REQUIRED | syscall.SYNCHRONIZE | 0xfff */ 2035711;
        var kernel32 = syscall.MustLoadDLL(kernel32Dllˢ);
        var _GetProcessAffinityMask = kernel32.MustFindProc(getProcessAffinityMaskˢ);
        var _SetProcessAffinityMask = kernel32.MustFindProc(setProcessAffinityMaskˢ);
        var cmd = exec.Command(Δos.Args[0], testRunTestNumCPUˢ);
        cmd.Value.Env = append(Δos.Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        cmd.Value.Stdout = new runtime_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.Value.Stderr = new runtime_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(CreationFlags: _CREATE_SUSPENDED));
        ref var err = ref heap<error>(out var Ꮡerr);
        err = cmd.Start();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cmdʗ1 = cmd;
        defer(() => {
            Ꮡerr.ValueSlot = cmdʗ1.Wait();
            @string childOutput = Ꮡbuf.Value.String();
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡt.Fatalf("child failed: %v: %v"u8, Ꮡerr.ValueSlot, childOutput);
            }
            // removeOneCPU should have decreased child cpu count by 1
            @string want = fmt.Sprintf("%d"u8, Δruntime.NumCPU() - 1);
            if (childOutput != want) {
                Ꮡt.Fatalf("child output: want %q, got %q"u8, want, childOutput);
            }
        }, ref ᒐ);
        var cmdʗ2 = cmd;
        var kernel32ʗ1 = kernel32;
        defer(() => {
            Ꮡerr.ValueSlot = resumeChildThread(kernel32ʗ1, (~(~cmdʗ2).Process).Pid);
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡt.Fatal(Ꮡerr.ValueSlot);
            }
        }, ref ᒐ);
        (var ph, err) = syscall.OpenProcess(_PROCESS_ALL_ACCESS, false, (uint32)(~(~cmd).Process).Pid);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(syscall.CloseHandle, ph, ref ᒐ);
        ref var mask = ref heap(new uintptr(), out var Ꮡmask);
        ref var sysmask = ref heap(new uintptr(), out var Ꮡsysmask);
        (var ret, _, err) = _GetProcessAffinityMask.Call((uintptr)ph, (uintptr)@unsafe.Pointer.FromBox(Ꮡmask), (uintptr)@unsafe.Pointer.FromBox(Ꮡsysmask));
        if (ret == 0) {
            Ꮡt.Fatal(err);
        }
        (var newmask, err) = removeOneCPU(mask);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (ret, _, err) = _SetProcessAffinityMask.Call((uintptr)ph, newmask);
        if (ret == 0) {
            Ꮡt.Fatal(err);
        }
        (ret, _, err) = _GetProcessAffinityMask.Call((uintptr)ph, (uintptr)@unsafe.Pointer.FromBox(Ꮡmask), (uintptr)@unsafe.Pointer.FromBox(Ꮡsysmask));
        if (ret == 0) {
            Ꮡt.Fatal(err);
        }
        if (newmask != mask) {
            Ꮡt.Fatalf("SetProcessAffinityMask didn't set newmask of 0x%x. Current mask is 0x%x."u8, newmask, mask);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nojackCˢ = "nojack.c"u8;
internal static readonly @string nojackDllˢ = "nojack.dll"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ3 = """

#include <stdint.h>
#include <windows.h>

uintptr_t cfunc(void) {
   SetLastError(123);
   return 0;
}

"""u8;

// See Issue 14959
public static void TestDLLPreloadMitigation(ж<testing.T> Ꮡt) {
    {
        var (_, errΔ1) = exec.LookPath(gccˢ); if (errΔ1 != default!) {
            Ꮡt.Skip(skippingTestGccIsMissingˢ);
        }
    }
    @string tmpdir = Ꮡt.TempDir();
    @string src = srcᶜ3;
    @string srcname = nojackCˢ;
    var err = Δos.WriteFile(filepath.Join(tmpdir, srcname), slice<byte>(src), 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string name = nojackDllˢ;
    var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", name, srcname);
    cmd.Value.Dir = tmpdir;
    (var @out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
    }
    @string dllpath = filepath.Join(tmpdir, name);
    var dll = syscall.MustLoadDLL(dllpath);
    dll.MustFindProc(cfuncˢ);
    dll.Release();
    // Get into the directory with the DLL we'll load by base name
    // ("nojack.dll") Think of this as the user double-clicking an
    // installer from their Downloads directory where a browser
    // silently downloaded some malicious DLLs.
    Ꮡt.Chdir(tmpdir);
    // First before we can load a DLL from the current directory,
    // loading it only as "nojack.dll", without an absolute path.
    delete(sysdll.IsSystemDLL, name); // in case test was run repeatedly
    (dll, err) = syscall.LoadDLL(name);
    if (err != default!) {
        Ꮡt.Fatalf("failed to load %s by base name before sysdll registration: %v"u8, name, err);
    }
    dll.Release();
    // And now verify that if we register it as a system32-only
    // DLL, the implicit loading from the current directory no
    // longer works.
    sysdll.IsSystemDLL[name] = true;
    (dll, err) = syscall.LoadDLL(name);
    if (err == default!) {
        dll.Release();
        Ꮡt.Fatalf("Bad: insecure load of DLL by base name %q before sysdll registration: %v"u8, name, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestprogcgoˢ = "testdata/testprogcgo/bigstack_windows.c"u8;
internal static readonly object absFailedˢ = (@string)"Abs failed: "u8;
internal static readonly @string bigStackˢ = "bigStack"u8;

// Test that C code called via a DLL can use large Windows thread
// stacks and call back in to Go without crashing. See issue #20975.
//
// See also TestBigStackCallbackCgo.
public static void TestBigStackCallbackSyscall(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, errΔ1) = exec.LookPath(gccˢ); if (errΔ1 != default!) {
                Ꮡt.Skip(skippingTestGccIsMissingˢ);
            }
        }
        var (srcname, err) = filepath.Abs(testdataTestprogcgoˢ);
        if (err != default!) {
            Ꮡt.Fatal(absFailedˢ, err);
        }
        @string tmpdir = Ꮡt.TempDir();
        @string outname = mydllDllˢ;
        var cmd = exec.Command(gccˢ, sharedˢ, "-s", werrorˢ, "-o", outname, srcname);
        cmd.Value.Dir = tmpdir;
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build dll: %v - %v"u8, err, ((@string)@out));
        }
        @string dllpath = filepath.Join(tmpdir, outname);
        var dll = syscall.MustLoadDLL(dllpath);
        var dllʗ1 = dll;
        defer(() => dllʗ1.Release(), ref ᒐ);
        bool ok = default!;
        var proc = dll.MustFindProc(bigStackˢ);
        var cb = syscall.NewCallback(uintptr () => {
            // Do something interesting to force stack checks.
            forceStackCopy();
            ok = true;
            return (uintptr)(0);
        });
        proc.Call(cb);
        if (!ok) {
            Ꮡt.Fatalf("callback not called"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSyscallStackUsage(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that the stack usage of a syscall doesn't exceed the limit.
    // See https://go.dev/issue/69813.
    syscall.Syscall15(procSetEvent.Addr(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    syscall.Syscall18(procSetEvent.Addr(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}

internal static ж<syscall.LazyDLL> modwinmm = syscall.NewLazyDLL("winmm.dll"u8);
internal static ж<syscall.LazyDLL> modkernel32 = syscall.NewLazyDLL("kernel32.dll"u8);
internal static ж<syscall.LazyProc> procCreateEvent = modkernel32.NewProc("CreateEventW"u8);
internal static ж<syscall.LazyProc> procSetEvent = modkernel32.NewProc("SetEvent"u8);

internal static (syscallꓸHandle, error) createEvent() {
    var (r0, _, e0) = syscall.Syscall6(procCreateEvent.Addr(), 4, 0, 0, 0, 0, 0, 0);
    if (r0 == 0) {
        return (0, e0);
    }
    return (((syscallꓸHandle)r0), default!);
}

internal static error setEvent(syscallꓸHandle h) {
    var (r0, _, e0) = syscall.Syscall(procSetEvent.Addr(), 1, (uintptr)h, 0, 0);
    if (r0 == 0) {
        return e0;
    }
    return default!;
}

public static void BenchmarkChanToSyscallPing(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = b.N;
    var ch = new channel<nint>(0);
    var (@event, err) = createEvent();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var chʗ1 = ch;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            syscall.WaitForSingleObject(@event, syscall.INFINITE);
            chʗ1.ᐸꟷ(1);
        }
    });
    for (nint i = 0; i < n; i++) {
        var errΔ1 = setEvent(@event);
        if (errΔ1 != default!) {
            Ꮡb.Fatal(errΔ1);
        }
        ᐸꟷ(ch);
    }
}

public static void BenchmarkSyscallToSyscallPing(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = b.N;
    var (event1, err) = createEvent();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    (var event2, err) = createEvent();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            syscall.WaitForSingleObject(event1, syscall.INFINITE);
            {
                var errΔ1 = setEvent(event2); if (errΔ1 != default!) {
                    Ꮡb.Errorf("Set event failed: %v"u8, errΔ1);
                    return;
                }
            }
        }
    });
    for (nint i = 0; i < n; i++) {
        {
            var errΔ2 = setEvent(event1); if (errΔ2 != default!) {
                Ꮡb.Fatal(errΔ2);
            }
        }
        if (Ꮡb.Failed()) {
            break;
        }
        syscall.WaitForSingleObject(event2, syscall.INFINITE);
    }
}

public static void BenchmarkChanToChanPing(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = b.N;
    var ch1 = new channel<nint>(0);
    var ch2 = new channel<nint>(0);
    var ch1ʗ1 = ch1;
    var ch2ʗ1 = ch2;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            ᐸꟷ(ch1ʗ1);
            ch2ʗ1.ᐸꟷ(1);
        }
    });
    for (nint i = 0; i < n; i++) {
        ch1.ᐸꟷ(1);
        ᐸꟷ(ch2);
    }
}

public static void BenchmarkOsYield(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        runtime_internal_test_package.OsYield();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainExeˢ = "main.exe"u8;

public static void BenchmarkRunningGoProgram(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string tmpdir = Ꮡb.TempDir();
    @string src = filepath.Join(tmpdir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(benchmarkRunningGoProgram), 438);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    @string exe = filepath.Join(tmpdir, mainExeˢ);
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_BжTB(Ꮡb)), buildˢ, "-o", exe, src);
    cmd.Value.Dir = tmpdir;
    (var @out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡb.Fatalf("building main.exe failed: %v\n%s"u8, err, @out);
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var cmdΔ1 = exec.Command(exe);
        var (outΔ1, errΔ1) = cmdΔ1.CombinedOutput();
        if (errΔ1 != default!) {
            Ꮡb.Fatalf("running main.exe failed: %v\n%s"u8, errΔ1, outΔ1);
        }
    }
}

internal static readonly @string benchmarkRunningGoProgram = """

package main

import _ "os" // average Go program will use "os" package, do the same here

func main() {
}

"""u8;

} // end runtime_test_package
