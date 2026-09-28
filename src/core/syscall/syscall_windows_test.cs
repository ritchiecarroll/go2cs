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
using fs = io.fs_package;
using go.os;
using path;
using static go.syscall_internal_test_package;

partial class syscall_test_package {

[GoType("dyn")] internal partial struct TestOpen_tests {
    internal @string path;
    internal nint flag;
    internal error err;
}

public static void TestOpen(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string dir = Ꮡt.TempDir();
    @string @file = filepath.Join(dir, "a");
    var (f, err) = Δos.Create(@file);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();
    var tests = new TestOpen_tests[]{
        new(dir, syscall.O_RDONLY, default!),
        new(dir, syscall.O_CREAT, default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_CREAT), default!),
        new(@file, (nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | Δos.O_CREATE), default!),
        new(@file, (nint)((nint)(nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | Δos.O_CREATE) | Δos.O_TRUNC), default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_TRUNC), syscall.ERROR_ACCESS_DENIED),
        new(dir, (nint)((nint)syscall.O_WRONLY | (nint)syscall.O_RDWR), syscall.EISDIR),
        new(dir, syscall.O_WRONLY, syscall.EISDIR),
        new(dir, syscall.O_RDWR, syscall.EISDIR)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (h, errΔ1) = syscall.Open(tt.path, tt.flag, 432);
        if (errΔ1 == default!) {
            syscall.CloseHandle(h);
        }
        if (!AreEqual(errΔ1, tt.err)) {
            Ꮡt.Errorf("%d: Open got %q, want %q"u8, i, errΔ1, tt.err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object computerNameReturnedˢ = (@string)"ComputerName returned empty string"u8;

public static void TestComputerName(ж<testing.T> Ꮡt) {
    var (name, err) = syscall.ComputerName();
    if (err != default!) {
        Ꮡt.Fatalf("ComputerName failed: %v"u8, err);
    }
    if (len(name) == 0) {
        Ꮡt.Error(computerNameReturnedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string longNameAndExtensionˢ = "long_name.and_extension"u8;

[GoType("dyn")] internal partial struct TestWin32finddata_X {
    internal syscall.Win32finddata fd;
    internal byte got;
    internal array<byte> pad = new(10); // to protect ourselves
}

public static void TestWin32finddata(ж<testing.T> Ꮡt) {
    @string dir = Ꮡt.TempDir();
    @string path = filepath.Join(dir, longNameAndExtensionˢ);
    var (f, err) = Δos.Create(path);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create %v: %v"u8, path, err);
    }
    f.Close();
    byte want = 2;     // it is unlikely to have this character in the filename
    ref var x = ref heap<TestWin32finddata_X>(out var Ꮡx);
    x = new TestWin32finddata_X(got: want);
    var (pathp, _) = syscall.UTF16PtrFromString(path);
    (var h, err) = syscall.FindFirstFile(pathp, Ꮡ((x.fd)));
    if (err != default!) {
        Ꮡt.Fatalf("FindFirstFile failed: %v"u8, err);
    }
    err = syscall.FindClose(h);
    if (err != default!) {
        Ꮡt.Fatalf("FindClose failed: %v"u8, err);
    }
    if (x.got != want) {
        Ꮡt.Fatalf("memory corruption: want=%d got=%d"u8, want, x.got);
    }
}

internal static void abort(@string funcname, error err) {
    throw panic(funcname + " failed: " + err.Error());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kernel32Dllˢ = "kernel32.dll"u8;
internal static readonly @string loadLibraryˢ = "LoadLibrary"u8;
internal static readonly @string getVersionˢ = "GetVersion"u8;
internal static readonly @string getProcAddressˢ = "GetProcAddress"u8;

public static void ExampleLoadLibrary() {
    GoFrame ᒐ = default;
    try {
        var (h, err) = syscall.LoadLibrary(kernel32Dllˢ);
        if (err != default!) {
            abort(loadLibraryˢ, err);
        }
        defer(syscall.FreeLibrary, h, ref ᒐ);
        (var proc, err) = syscall.GetProcAddress(h, getVersionˢ);
        if (err != default!) {
            abort(getProcAddressˢ, err);
        }
        var (r, _, _) = syscall.Syscall((uintptr)proc, 0, 0, 0, 0);
        var major = (byte)r;
        var minor = (uint8)((r >> (int)(8)));
        var build = (uint16)((r >> (int)(16)));
        print((@string)"windows version "u8, major, (@string)"."u8, minor, (@string)" (Build "u8, build, (@string)")\n"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTOKEN_ALL_ACCESS(ж<testing.T> Ꮡt) {
    if (syscall.TOKEN_ALL_ACCESS != 0xF01FF) {
        Ꮡt.Errorf("TOKEN_ALL_ACCESS = %x, want 0xF01FF"u8, (nint)(syscall.TOKEN_ALL_ACCESS));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gccˢ = "gcc"u8;
internal static readonly @string helloworldGoˢ = "helloworld.go"u8;
internal static readonly @string helloworldDllˢ = "helloworld.dll"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string buildmodeˢ = "-buildmode"u8;
internal static readonly @string cSharedˢ = "c-shared"u8;
internal static readonly @string helloworldExeˢ = "helloworld.exe"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dlltextᶜ = """

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

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string exetextᶜ = """

#include <stdlib.h>
#include <windows.h>
int main(int argc, char *argv[])
{
	system("hostname");
	((void(*)(void))GetProcAddress(LoadLibraryA(%q), "HelloWorld"))();
	system("hostname");
	return 0;
}

"""u8;

public static void TestStdioAreInheritable(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new syscall_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveCGO(new syscall_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveExecPath(new syscall_test_package.testing_TжTB(Ꮡt), gccˢ);
    @string tmpdir = Ꮡt.TempDir();
    // build go dll
    @string dlltext = dlltextᶜ;
    @string dllsrc = filepath.Join(tmpdir, helloworldGoˢ);
    var err = Δos.WriteFile(dllsrc, slice<byte>(dlltext), 420);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string dll = filepath.Join(tmpdir, helloworldDllˢ);
    var cmd = exec.Command(testenv.GoToolPath(new syscall_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, dllsrc);
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build go library: %s\n%s"u8, err, @out);
    }
    // build c exe
    @string exetext = exetextᶜ;
    @string exe = filepath.Join(tmpdir, helloworldExeˢ);
    cmd = exec.Command(gccˢ, "-o"u8, exe, "-xc", "-");
    cmd.Value.Stdin = new syscall_test_package.strings_ReaderжReader(strings.NewReader(fmt.Sprintf(exetext, dll)));
    (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build c executable: %s\n%s"u8, err, @out);
    }
    (@out, err) = exec.Command(exe).Output();
    if (err != default!) {
        Ꮡt.Fatalf("c program execution failed: %v: %v"u8, err, ((@string)@out));
    }
    (var hostname, err) = Δos.Hostname();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string have = strings.ReplaceAll(((@string)@out), "\n"u8, ""u8);
    have = strings.ReplaceAll(have, "\r"u8, ""u8);
    @string want = fmt.Sprintf("%sHello World%s"u8, hostname, hostname);
    if (have != want) {
        Ꮡt.Fatalf("c program output is wrong: got %q, want %q"u8, have, want);
    }
}

public static void TestGetwd_DoesNotPanicWhenPathIsLong(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Regression test for https://github.com/golang/go/issues/60051.
    @string tmp = Ꮡt.TempDir();
    Ꮡt.Chdir(tmp);
    // The length of a filename is also limited, so we can't reproduce the
    // crash by creating a single directory with a very long name; we need two
    // layers.
    @string a200 = strings.Repeat("a"u8, 200);
    @string dirname = filepath.Join(tmp, a200, a200);
    var err = Δos.MkdirAll(dirname, 448);
    if (err != default!) {
        Ꮡt.Skipf("MkdirAll failed: %v"u8, err);
    }
    err = Δos.Chdir(dirname);
    if (err != default!) {
        Ꮡt.Skipf("Chdir failed: %v"u8, err);
    }
    syscall.Getwd();
}

public static void TestGetStartupInfo(ж<testing.T> Ꮡt) {
    ref var si = ref heap(new syscall.StartupInfo(), out var Ꮡsi);
    var err = syscall.GetStartupInfo(Ꮡsi);
    if (err != default!) {
        // see https://go.dev/issue/31316
        Ꮡt.Fatalf("GetStartupInfo: got error %v, want nil"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syscallNˢ = "SyscallN"u8;
internal static readonly @string syscallˢ = "Syscall"u8;

public static void TestSyscallAllocations(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.SkipIfOptimizationOff(new syscall_test_package.testing_TжTB(Ꮡt));
        // Test that syscall.SyscallN arguments do not escape.
        // The function used (in this case GetVersion) doesn't matter
        // as long as it is always available and doesn't panic.
        var (h, err) = syscall.LoadLibrary(kernel32Dllˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(syscall.FreeLibrary, h, ref ᒐ);
        (var proc, err) = syscall.GetProcAddress(h, getVersionˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        void testAllocs(ж<testing.T> tΔ1, @string name, Func<error> fn) {
            tΔ1.Run(name, (ж<testing.T> tΔ2) => {
                nint n = (nint)testing.AllocsPerRun(10, () => {
                    {
                        var errΔ1 = fn(); if (errΔ1 != default!) {
                            tΔ2.Fatalf("%s: %v"u8, name, errΔ1);
                        }
                    }
                });
                if (n > 0) {
                    tΔ2.Errorf("allocs = %d, want 0"u8, n);
                }
            });
        }
        testAllocs(Ꮡt, syscallNˢ, error () => {
            var (r0, _, e1) = syscall.SyscallN(proc, 0, 0, 0);
            if (r0 == 0) {
                return e1;
            }
            return default!;
        });
        testAllocs(Ꮡt, syscallˢ, error () => {
            var (r0, _, e1) = syscall.Syscall(proc, 3, 0, 0, 0);
            if (r0 == 0) {
                return e1;
            }
            return default!;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void FuzzUTF16FromString(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((@string)"hi"u8); // ASCII
    f.Add((@string)"â"u8); // latin1
    f.Add((@string)"ねこ"u8); // plane 0
    f.Add((@string)"😃"u8); // extra Plane 0
    f.Add(((@string)(new byte[]{0x90}))); // invalid byte
    f.Add(((@string)(new byte[]{0xe3, 0x81}))); // truncated
    f.Add(((@string)(new byte[]{0xe3, 0xc1, 0x81}))); // invalid middle byte
    Ꮡf.Fuzz((ж<testing.T> t, @string tst) => {
        var (res, err) = syscall.UTF16FromString(tst);
        if (err != default!) {
            if (strings.Contains(tst, "\x00"u8)) {
                t.Skipf("input %q contains a NUL byte"u8, tst);
            }
            t.Fatalf("UTF16FromString(%q): %v"u8, tst, err);
        }
        t.Logf("UTF16FromString(%q) = %04x"u8, tst, res);
        if (len(res) < 1 || res[len(res) - 1] != 0) {
            t.Fatalf("missing NUL terminator"u8);
        }
        if (len(res) > len(tst) + 1) {
            t.Fatalf("len(%04x) > len(%q)+1"u8, res, tst);
        }
    });
}

} // end syscall_test_package
