// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using strconv = @internal.strconv_package;
using windows = @internal.syscall.windows_package;
using Δos = os_package;
using filepath = go.path.filepath_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;
using go.path;
using static go.os_internal_test_package;

[GoFieldAccess(typeof(windows.SymbolicLinkReparseBuffer))]
partial class os_test_package {

[GoInit] internal static void initΔ1() {
    createJunction = (ж<Δtesting.T> t, @string link, @string target) => {
        if (!filepath.IsAbs(target)) {
            target = filepath.Dir(link) + @"\"u8 + target;
        }
        (target, var err) = syscall.FullPath(target);
        if (err != default!) {
            t.Fatal(err);
        }
        ref var rd = ref heap(new reparseData(), out var Ꮡrd);
        rd.addSubstituteName(@"\??\"u8 + target);
        rd.addPrintName(target);
        {
            var errΔ1 = createMountPoint(link, Ꮡrd); if (errΔ1 != default!) {
                t.Fatal(errΔ1);
            }
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openDirectoryˢ = "OpenDirectory"u8;
internal static readonly @string createˢ = "Create"u8;
internal static readonly @string lstatˢ = "Lstat"u8;
internal static readonly @string removeAllˢ = "RemoveAll"u8;
internal static readonly @string removeAllNoRootˢ = "RemoveAllNoRoot"u8;
internal static readonly @string deleteOnCloseˢ = "DeleteOnClose"u8;
internal static readonly @string legacyDeleteˢ = "LegacyDelete"u8;

public static void TestRootOpenatFallback(ж<Δtesting.T> Ꮡt) {
    windows.TestOpenatFallback = true;
    Ꮡt.Cleanup(() => {
        windows.TestOpenatFallback = false;
    });

    // Exercise the existing path traversal and symlink confinement cases
    // with OBJ_DONT_REPARSE unavailable, as on Windows 10 build 10240.
    Ꮡt.Run(openFileˢ, TestRootOpen_File);
    Ꮡt.Run(openDirectoryˢ, TestRootOpen_Directory);
    Ꮡt.Run(createˢ, TestRootCreate);
    Ꮡt.Run(statˢ, TestRootStat);
    Ꮡt.Run(lstatˢ, TestRootLstat);
    Ꮡt.Run(removeAllˢ, TestRootRemoveAll);
    Ꮡt.Run(removeAllNoRootˢ, TestRemoveAll);
    Ꮡt.Run(deleteOnCloseˢ, testRootOpenFileDeleteOnClose);
    Ꮡt.Run(legacyDeleteˢ, (ж<Δtesting.T> tΔ1) => {
        windows.TestDeleteatFallback = true;
        tΔ1.Cleanup(() => {
            windows.TestDeleteatFallback = false;
        });
        tΔ1.Run(removeAllˢ, TestRootRemoveAll);
        tΔ1.Run(removeAllNoRootˢ, TestRemoveAll);
    });
}

// Verify that Root.Open rejects Windows reserved names.
public static void TestRootWindowsDeviceNames(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (r, err) = Δos.OpenRoot(Ꮡt.TempDir());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        {
            var (f, errΔ1) = r.Open(nulˢ4); if (errΔ1 == default!) {
                Ꮡt.Errorf(@"r.Open(""NUL"") succeeded; want error"""u8);
                f.Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ3 = "FILE"u8;

// Verify that Root.Open is case-insensitive.
// (The wrong options to NtOpenFile could make operations case-sensitive,
// so this is worth checking.)
public static void TestRootWindowsCaseInsensitivity(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        {
            var errΔ1 = Δos.WriteFile(filepath.Join(dir, fileˢ2), default!, 438); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (r, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        (var f, err) = r.Open(fileˢ3);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        f.Close();
        {
            var errΔ2 = r.Remove(fileˢ3); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var (_, errΔ3) = Δos.Stat(filepath.Join(dir, fileˢ2)); if (!errors.Is(errΔ3, Δos.ErrNotExist)) {
                Ꮡt.Fatalf("os.Stat(file) after deletion: %v, want ErrNotFound"u8, errΔ3);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestRootSymlinkRelativity_type /*dyn*/ {
    internal @string name;
    internal @string target;
}

// TestRootSymlinkRelativity tests that symlinks created using Root.Symlink have the
// same SYMLINK_FLAG_RELATIVE value as ones creates using os.Symlink.
public static void TestRootSymlinkRelativity(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        foreach (var (i, vᴛ1) in new TestRootSymlinkRelativity_type[]{new(
            name: "relative"u8,
            target: @"foo"u8
        ), new(
            name: "absolute"u8,
            target: @"C:\foo"u8
        ), new(
            name: "current working directory-relative"u8,
            target: @"C:foo"u8
        ), new(
            name: "root-relative"u8,
            target: @"\foo"u8
        ), new(
            name: "question prefix"u8,
            target: @"\\?\foo"u8
        ), new(
            name: "relative with dot dot"u8,
            target: @"a\..\b"u8 // could be cleaned (but isn't)

        )
        }.slice()) {
            ref var test = ref heap(new TestRootSymlinkRelativity_type(), out var Ꮡtest);
            test = vᴛ1;

            var rootʗ2 = root;
            var testʗ1 = test;
            Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
                @string name = fmt.Sprintf("symlink_%v"u8, i);
                {
                    var errΔ1 = Δos.Symlink(testʗ1.target, filepath.Join(dir, name)); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                {
                    var errΔ2 = rootʗ2.Symlink(testʗ1.target, name + "_at"u8); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }

                var (osRDB, errΔ3) = readSymlinkReparseData(filepath.Join(dir, name));
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                (var rootRDB, errΔ3) = readSymlinkReparseData(filepath.Join(dir, name + "_at"));
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                if (osRDB.Flags != rootRDB.Flags) {
                    tΔ1.Errorf("symlink target %q: Symlink flags = %x, Root.Symlink flags = %x"u8, testʗ1.target, osRDB.Flags, rootRDB.Flags);
                }

                // Compare the link target.
                // os.Symlink converts current working directory-relative links
                // such as c:foo into absolute links.
                (var osTarget, errΔ3) = Δos.Readlink(filepath.Join(dir, name));
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                (var rootTarget, errΔ3) = Δos.Readlink(filepath.Join(dir, name + "_at"));
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                if (osTarget != rootTarget) {
                    tΔ1.Errorf("symlink created with target %q: Symlink target = %q, Root.Symlink target = %q"u8, testʗ1.target, osTarget, rootTarget);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<windows.SymbolicLinkReparseBuffer>, error) readSymlinkReparseData(@string name) {
    GoFrame ᒐ = default;
    try {
        var (nameu16, err) = syscall.UTF16FromString(name);
        if (err != default!) {
            return (default!, err);
        }
        (var h, err) = syscall.CreateFile(Ꮡ(nameu16, 0), syscall.GENERIC_READ, 0, nil, syscall.OPEN_EXISTING,
            (uint32)((uint32)syscall.FILE_FLAG_OPEN_REPARSE_POINT | (uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS), 0);
        if (err != default!) {
            return (default!, err);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        ref var rdbbuf = ref heap(new array<byte>(16384), out var Ꮡrdbbuf);
        ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
        err = syscall.DeviceIoControl(h, syscall.FSCTL_GET_REPARSE_POINT, nil, 0, Ꮡrdbbuf.at<byte>(0), (uint32)len(rdbbuf), ᏑbytesReturned, nil);
        if (err != default!) {
            return (default!, err);
        }

        var rdb = Ꮡrdbbuf.at<byte>(0).Reinterpret<byte, windows.REPARSE_DATA_BUFFER>();
        if ((~rdb).ReparseTag != syscall.IO_REPARSE_TAG_SYMLINK) {
            return (default!, fmt.Errorf("%q: not a symlink"u8, name));
        }

        var bufoff = /* unsafe.Offsetof(rdb.DUMMYUNIONNAME) */ (uintptr)8;
        var symlinkBuf = Ꮡrdbbuf.at<byte>((ulong)(bufoff)).Reinterpret<byte, windows.SymbolicLinkReparseBuffer>();

        return (symlinkBuf, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal partial struct TestRootSymlinkToDirectory_type /*dyn*/ {
    internal @string name;
    internal @string target;
    internal bool wantDir;
}

// TestRootSymlinkToDirectory tests that Root.Symlink creates directory links
// when the target is a directory contained within the root.
public static void TestRootSymlinkToDirectory(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        {
            var errΔ1 = Δos.Mkdir(filepath.Join(dir, dirˢ), 511); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var errΔ2 = Δos.WriteFile(filepath.Join(dir, fileˢ2), default!, 438); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }

        @string dir2 = Ꮡt.TempDir();

        foreach (var (i, vᴛ1) in new TestRootSymlinkToDirectory_type[]{new(
            name: "directory outside root"u8,
            target: dir2,
            wantDir: false
        ), new(
            name: "directory inside root"u8,
            target: "dir"u8,
            wantDir: true
        ), new(
            name: "file inside root"u8,
            target: "file"u8,
            wantDir: false
        ), new(
            name: "nonexistent inside root"u8,
            target: "nonexistent"u8,
            wantDir: false
        )
        }.slice()) {
            ref var test = ref heap(new TestRootSymlinkToDirectory_type(), out var Ꮡtest);
            test = vᴛ1;

            var rootʗ2 = root;
            var testʗ1 = test;
            Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    @string name = fmt.Sprintf("symlink_%v"u8, i);
                    {
                        var errΔ3 = rootʗ2.Symlink(testʗ1.target, name); if (errΔ3 != default!) {
                            tΔ1.Fatal(errΔ3);
                        }
                    }

                    // Lstat strips the directory mode bit from reparse points,
                    // so we need to use GetFileInformationByHandle directly to
                    // determine if this is a directory link.
                    var (nameu16, errΔ4) = syscall.UTF16PtrFromString(filepath.Join(dir, name));
                    if (errΔ4 != default!) {
                        tΔ1.Fatal(errΔ4);
                    }
                    (var h, errΔ4) = syscall.CreateFile(nameu16, 0, 0, nil, syscall.OPEN_EXISTING,
                        (uint32)((uint32)syscall.FILE_FLAG_OPEN_REPARSE_POINT | (uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS), 0);
                    if (errΔ4 != default!) {
                        tΔ1.Fatal(errΔ4);
                    }
                    defer(syscall.CloseHandle, h, ref ᒐ);
                    ref var fi = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡfi);
                    {
                        var errΔ5 = syscall.GetFileInformationByHandle(h, Ꮡfi); if (errΔ5 != default!) {
                            tΔ1.Fatal(errΔ5);
                        }
                    }
                    var gotDir = (fi.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0;

                    {
                        var (got, want) = (gotDir, testʗ1.wantDir); if (got != want) {
                            tΔ1.Errorf("link target %q: isDir = %v, want %v"u8, testʗ1.target, got, want);
                        }
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

public static void TestRootOpenFileTruncateNamedPipe(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string name = pipeName();
        var pipe = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, false);
        var pipeʗ1 = pipe;
        defer(() => pipeʗ1.Close(), ref ᒐ);

        var (root, err) = Δos.OpenRoot(filepath.Dir(name));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        (var f, err) = root.OpenFile(filepath.Base(name), (nint)((nint)(nint)(Δos.O_TRUNC | Δos.O_RDWR) | Δos.O_CREATE), 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        f.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestRootOpenFileFlags_tests /*dyn*/ {
    internal uint32 flag;
    internal uint32 wantMode;
}

public static void TestRootOpenFileFlags(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();

        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        // The only way to retrieve some of the flags passed in CreateFile
        // is using NtQueryInformationFile, which returns the file flags
        // NT equivalent. Note that FILE_SYNCHRONOUS_IO_NONALERT is always
        // set when FILE_FLAG_OVERLAPPED is not passed.
        // The flags that can't be retrieved using NtQueryInformationFile won't
        // be tested in here, but we at least know that the logic to handle them is correct.
        var tests = new TestRootOpenFileFlags_tests[]{
            new(0, windows.FILE_SYNCHRONOUS_IO_NONALERT),
            new(windows.O_FILE_FLAG_OVERLAPPED, 0),
            new(windows.O_FILE_FLAG_NO_BUFFERING, (uint32)((uint32)windows.FILE_NO_INTERMEDIATE_BUFFERING | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT)),
            new((uint32)((uint32)windows.O_FILE_FLAG_NO_BUFFERING | (uint32)windows.O_FILE_FLAG_OVERLAPPED), windows.FILE_NO_INTERMEDIATE_BUFFERING),
            new(windows.O_FILE_FLAG_SEQUENTIAL_SCAN, (uint32)((uint32)windows.FILE_SEQUENTIAL_ONLY | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT)),
            new(windows.O_FILE_FLAG_WRITE_THROUGH, (uint32)((uint32)windows.FILE_WRITE_THROUGH | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT))
        }.slice();
        foreach (var (i, vᴛ1) in tests) {
            ref var tt = ref heap(new TestRootOpenFileFlags_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var rootʗ2 = root;
            var ttʗ1 = tt;
            Ꮡt.Run(strconv.Itoa(i), (ж<Δtesting.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (f, errΔ1) = rootʗ2.OpenFile(strconv.Itoa(i) + ".txt"u8, (nint)((nint)(nint)((nint)syscall.O_RDWR | (nint)syscall.O_CREAT) | (nint)ttʗ1.flag), 438);
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    ref var info = ref heap(new windows.FILE_MODE_INFORMATION(), out var Ꮡinfo);
                    {
                        var errΔ2 = windows.NtQueryInformationFile((syscallꓸHandle)f.Fd(), Ꮡ(new windows.IO_STATUS_BLOCK(nil)),
                            @unsafe.Pointer.FromPinnedBox(Ꮡinfo), (uint32)/* unsafe.Sizeof(info) */ (uintptr)4, windows.FileModeInformation); if (errΔ2 != default!) {
                            tΔ1.Fatal(errΔ2);
                        }
                    }
                    if (info.Mode != ttʗ1.wantMode) {
                        tΔ1.Errorf("file mode = 0x%x; want 0x%x"u8, info.Mode, ttʗ1.wantMode);
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

public static void TestRootOpenFileDeleteOnClose(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    testRootOpenFileDeleteOnClose(Ꮡt);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string nameᶜ = "test.txt"u8;

internal static void testRootOpenFileDeleteOnClose(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        @string name = nameᶜ;
        (var f, err) = root.OpenFile(name, (nint)((nint)(UntypedInt)(syscall.O_RDWR | syscall.O_CREAT) | (nint)windows.O_FILE_FLAG_DELETE_ON_CLOSE), 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = f.Close(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        // The file should be deleted after closing.
        {
            var (_, errΔ2) = Δos.Stat(filepath.Join(dir, name)); if (!errors.Is(errΔ2, Δos.ErrNotExist)) {
                Ꮡt.Errorf("expected file to be deleted, got %v"u8, errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRootOpenFileFlagInvalid(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        // invalidFileFlag is the only value in the file flag range that is not supported,
        // as it is not defined in the Windows API.
        UntypedInt invalidFileFlag = 0x00400000;
        (var f, err) = root.OpenFile(testTxtˢ, (nint)((nint)(UntypedInt)(syscall.O_RDWR | syscall.O_CREAT) | (nint)invalidFileFlag), 438);
        if (!errors.Is(err, Δos.ErrInvalid)) {
            Ꮡt.Fatalf("expected os.ErrInvalid, got %v"u8, err);
        }
        f.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package
