// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = io.fs_package;
using go.os;
using path;
using static go.syscall_internal_test_package;

partial class syscall_test_package {

internal partial struct TestOpen_tests /*dyn*/ {
    internal @string path;
    internal nint flag;
    internal error err;
}

public static void TestOpen(ж<testing.T> t) {
    t.Parallel();

    @string dir = t.TempDir();
    @string @file = filepath.Join(dir, "a");
    var (f, err) = Δos.Create(@file);
    if (err != nil) {
        t.Fatal(err);
    }
    f.Close();

    var tests = slice<TestOpen_tests>([
        new(dir, syscall.O_RDONLY, default),
        new(dir, syscall.O_CREAT, default),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_CREAT), default),
        new(@file, (nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | Δos.O_CREATE), default),
        new(@file, (nint)((nint)(nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | Δos.O_CREATE) | Δos.O_TRUNC), default),
        new(@file, (nint)((nint)syscall.O_WRONLY | (nint)syscall.O_RDWR), default),
        new(dir, (nint)((nint)syscall.O_WRONLY | (nint)syscall.O_RDWR), default),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_TRUNC), syscall.ERROR_ACCESS_DENIED),
        new(dir, syscall.O_WRONLY, syscall.EISDIR),
        new(dir, syscall.O_RDWR, syscall.EISDIR)
    ]);
    foreach (var (i, tt) in tests) {
        var (h, errΔ1) = syscall.Open(tt.path, tt.flag, 432);
        if (errΔ1 == nil) {
            syscall.CloseHandle(h);
        }
        if (!AreEqual(errΔ1, tt.err)) {
            t.Errorf("%d: Open got %q, want %q", i, errΔ1, tt.err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object computerNameReturnedˢ = (@string)"ComputerName returned empty string"u8;

public static void TestComputerName(ж<testing.T> t) {
    var (name, err) = syscall.ComputerName();
    if (err != nil) {
        t.Fatalf("ComputerName failed: %v", err);
    }
    if (len(name) == 0) {
        t.Error(computerNameReturnedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string longNameAndExtensionˢ => ref longNameAndExtensionˢᵇ;
private static readonly @string longNameAndExtensionˢᵇ = "long_name.and_extension"u8;

internal partial struct TestWin32finddata_X /*dyn*/ {
    internal syscall.Win32finddata fd;
    internal byte got;
    internal array<byte> pad = new(10); // to protect ourselves
}

public static void TestWin32finddata(ж<testing.T> t) {
    @string dir = t.TempDir();

    @string path = filepath.Join(dir, longNameAndExtensionˢ);
    var (f, err) = Δos.Create(path);
    if (err != nil) {
        t.Fatalf("failed to create %v: %v", path, err);
    }
    f.Close();
    byte want = 2; // it is unlikely to have this character in the filename
    ref var x = ref heap<TestWin32finddata_X>(new TestWin32finddata_X(got: want), out var Ꮡx);

    var (pathp, _) = syscall.UTF16PtrFromString(path);
    (var h, err) = syscall.FindFirstFile(pathp, Ꮡ((x.fd)));
    if (err != nil) {
        t.Fatalf("FindFirstFile failed: %v", err);
    }
    err = syscall.FindClose(h);
    if (err != nil) {
        t.Fatalf("FindClose failed: %v", err);
    }

    if (x.got != want) {
        t.Fatalf("memory corruption: want=%d got=%d", want, x.got);
    }
}

internal static void abort(@string funcname, error err) {
    throw panic(funcname + " failed: " + err.Error());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string kernel32Dllˢ => ref kernel32Dllˢᵇ;
private static readonly @string kernel32Dllˢᵇ = "kernel32.dll"u8;
internal static ref readonly @string loadLibraryˢ => ref loadLibraryˢᵇ;
private static readonly @string loadLibraryˢᵇ = "LoadLibrary"u8;
internal static ref readonly @string getVersionˢ => ref getVersionˢᵇ;
private static readonly @string getVersionˢᵇ = "GetVersion"u8;
internal static ref readonly @string getProcAddressˢ => ref getProcAddressˢᵇ;
private static readonly @string getProcAddressˢᵇ = "GetProcAddress"u8;

public static void ExampleLoadLibrary() {
    GoFrame ᒐ = default;
    try {
        var (h, err) = syscall.LoadLibrary(kernel32Dllˢ);
        if (err != nil) {
            abort(loadLibraryˢ, err);
        }
        defer(syscall.FreeLibrary, h, ref ᒐ);
        (var proc, err) = syscall.GetProcAddress(h, getVersionˢ);
        if (err != nil) {
            abort(getProcAddressˢ, err);
        }
        var (r, _, _) = syscall.Syscall((uintptr)proc, 0, 0, 0, 0);
        var major = (byte)r;
        var minor = (uint8)(r >> 8);
        var build = (uint16)(r >> 16);
        print((@string)"windows version ", major, (@string)".", minor, (@string)" (Build ", build, (@string)")\n");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTOKEN_ALL_ACCESS(ж<testing.T> t) {
    if (syscall.TOKEN_ALL_ACCESS != 0xF01FF) {
        t.Errorf("TOKEN_ALL_ACCESS = %x, want 0xF01FF", (nint)(syscall.TOKEN_ALL_ACCESS));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gccˢ => ref gccˢᵇ;
private static readonly @string gccˢᵇ = "gcc"u8;
internal static ref readonly @string helloworldGoˢ => ref helloworldGoˢᵇ;
private static readonly @string helloworldGoˢᵇ = "helloworld.go"u8;
internal static ref readonly @string helloworldDllˢ => ref helloworldDllˢᵇ;
private static readonly @string helloworldDllˢᵇ = "helloworld.dll"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string buildmodeˢ => ref buildmodeˢᵇ;
private static readonly @string buildmodeˢᵇ = "-buildmode"u8;
internal static ref readonly @string cSharedˢ => ref cSharedˢᵇ;
private static readonly @string cSharedˢᵇ = "c-shared"u8;
internal static ref readonly @string helloworldExeˢ => ref helloworldExeˢᵇ;
private static readonly @string helloworldExeˢᵇ = "helloworld.exe"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dlltextᶜ => ref dlltextᶜᵇ;
private static readonly @string dlltextᶜᵇ = """

package main

import "C"
import (
	"fmt"
)

//export HelloWorld
func HelloWorld() {
	fmt.Println("Hello World")
}

func main() {}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string exetextᶜ => ref exetextᶜᵇ;
private static readonly @string exetextᶜᵇ = """

#include <stdlib.h>
#include <windows.h>
int main(int argc, char *argv[])
{
	system("hostname");
	((void(*)(void))GetProcAddress(LoadLibraryA(%q), "HelloWorld"))();
	system("hostname");
	return 0;
}

"""u8.Raw();

public static void TestStdioAreInheritable(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new syscall_test_package.testing_TжTB(t));
    testenv.MustHaveCGO(new syscall_test_package.testing_TжTB(t));
    testenv.MustHaveExecPath(new syscall_test_package.testing_TжTB(t), gccˢ);

    @string tmpdir = t.TempDir();

    // build go dll
    @string dlltext = dlltextᶜ;
    @string dllsrc = filepath.Join(tmpdir, helloworldGoˢ);
    var err = Δos.WriteFile(dllsrc, slice<byte>(dlltext), 420);
    if (err != nil) {
        t.Fatal(err);
    }
    @string dll = filepath.Join(tmpdir, helloworldDllˢ);
    var cmd = exec.Command(testenv.GoToolPath(new syscall_test_package.testing_TжTB(t)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, dllsrc);
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to build go library: %s\n%s", err, @out);
    }

    // build c exe
    @string exetext = exetextᶜ;
    @string exe = filepath.Join(tmpdir, helloworldExeˢ);
    cmd = exec.Command(gccˢ, "-o", exe, "-xc", "-");
    cmd.Stdin = new syscall_test_package.strings_ReaderжReader(strings.NewReader(fmt.Sprintf(exetext, dll)));
    (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to build c executable: %s\n%s", err, @out);
    }
    (@out, err) = exec.Command(exe).Output();
    if (err != nil) {
        t.Fatalf("c program execution failed: %v: %v", err, (@string)@out);
    }

    (var hostname, err) = Δos.Hostname();
    if (err != nil) {
        t.Fatal(err);
    }

    @string have = strings.ReplaceAll((@string)@out, "\n", ""u8);
    have = strings.ReplaceAll(have, "\r", ""u8);
    @string want = fmt.Sprintf("%sHello World%s"u8, hostname, hostname);
    if (have != want) {
        t.Fatalf("c program output is wrong: got %q, want %q", have, want);
    }
}

public static void TestGetwd_DoesNotPanicWhenPathIsLong(ж<testing.T> t) {
    // Regression test for https://github.com/golang/go/issues/60051.
    @string tmp = t.TempDir();
    t.Chdir(tmp);

    // The length of a filename is also limited, so we can't reproduce the
    // crash by creating a single directory with a very long name; we need two
    // layers.
    @string a200 = strings.Repeat("a", 200);
    @string dirname = filepath.Join(tmp, a200, a200);

    var err = Δos.MkdirAll(dirname, 448);
    if (err != nil) {
        t.Skipf("MkdirAll failed: %v", err);
    }
    err = Δos.Chdir(dirname);
    if (err != nil) {
        t.Skipf("Chdir failed: %v", err);
    }

    syscall.Getwd();
}

public static void TestGetStartupInfo(ж<testing.T> t) {
    ref var si = ref heap(new syscall.StartupInfo(), out var Ꮡsi);
    var err = syscall.GetStartupInfo(Ꮡsi);
    if (err != nil) {
        // see https://go.dev/issue/31316
        t.Fatalf("GetStartupInfo: got error %v, want nil", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallNˢ => ref syscallNˢᵇ;
private static readonly @string syscallNˢᵇ = "SyscallN"u8;
internal static ref readonly @string syscallˢ => ref syscallˢᵇ;
private static readonly @string syscallˢᵇ = "Syscall"u8;

public static void TestSyscallAllocations(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.SkipIfOptimizationOff(new syscall_test_package.testing_TжTB(t));

        // Test that syscall.SyscallN arguments do not escape.
        // The function used (in this case GetVersion) doesn't matter
        // as long as it is always available and doesn't panic.
        var (h, err) = syscall.LoadLibrary(kernel32Dllˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(syscall.FreeLibrary, h, ref ᒐ);
        (var proc, err) = syscall.GetProcAddress(h, getVersionˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        void testAllocs(ж<testing.T> tΔ1, @string name, Func<error> fn) {
            tΔ1.Run(name, (ж<testing.T> tΔ2) => {
                nint n = (nint)testing.AllocsPerRun(10, () => {
                    {
                        var errΔ1 = fn(); if (errΔ1 != nil) {
                            tΔ2.Fatalf("%s: %v", name, errΔ1);
                        }
                    }
                });
                if (n > 0) {
                    tΔ2.Errorf("allocs = %d, want 0", n);
                }
            });
        }

        testAllocs(t, syscallNˢ, error () => {
            var (r0, _, e1) = syscall.SyscallN(proc, 0, 0, 0);
            if (r0 == 0) {
                return e1;
            }
            return default;
        });
        testAllocs(t, syscallˢ, error () => {
            var (r0, _, e1) = syscall.Syscall(proc, 3, 0, 0, 0);
            if (r0 == 0) {
                return e1;
            }
            return default;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void FuzzUTF16FromString(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add("hi");                                      // ASCII
    f.Add("â");                                       // latin1
    f.Add("ねこ");                                      // plane 0
    f.Add("😃");                                       // extra Plane 0
    f.Add(((@string)(new byte[]{0x90})));             // invalid byte
    f.Add(((@string)(new byte[]{0xe3, 0x81})));       // truncated
    f.Add(((@string)(new byte[]{0xe3, 0xc1, 0x81}))); // invalid middle byte

    Ꮡf.Fuzz((ж<testing.T> t, @string tst) => {
        var (res, err) = syscall.UTF16FromString(tst);
        if (err != nil) {
            if (strings.Contains(tst, "\x00")) {
                t.Skipf("input %q contains a NUL byte", tst);
            }
            t.Fatalf("UTF16FromString(%q): %v", tst, err);
        }
        t.Logf("UTF16FromString(%q) = %04x", tst, res);

        if (len(res) < 1 || res[len(res) - 1] != 0) {
            t.Fatalf("missing NUL terminator");
        }
        if (len(res) > len(tst) + 1) {
            t.Fatalf("len(%04x) > len(%q)+1", res, tst);
        }
    });
}

public static void BenchmarkErrnoString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    while (Ꮡb.Loop()) {
        _ = ((syscall.Errno)2).Error();
    }
}

} // end syscall_test_package
