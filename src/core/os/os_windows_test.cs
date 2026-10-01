// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using syscallDescriptor = go.syscall_package.ΔHandle;

namespace go;

using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using poll = @internal.poll_package;
using windows = @internal.syscall.windows_package;
using registry = @internal.syscall.windows.registry_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using @internal.syscall.windows;
using go.io;
using go.os;
using go.path;
using static go.os_internal_test_package;
using unicode;

partial class os_test_package {

internal static ж<godebug.Setting> winsymlink = godebug.New("winsymlink"u8);

internal static ж<godebug.Setting> winreadlinkvolume = godebug.New("winreadlinkvolume"u8);

public static void TestSameWindowsFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    var (f, err) = Δos.Create("a"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();
    (var ia1, err) = Δos.Stat("a"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var path, err) = filepath.Abs("a"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var ia2, err) = Δos.Stat(path);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!Δos.SameFile(ia1, ia2)) {
        Ꮡt.Errorf("files should be same"u8);
    }
    @string p = filepath.VolumeName(path) + filepath.Base(path);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var ia3, err) = Δos.Stat(p);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!Δos.SameFile(ia1, ia3)) {
        Ꮡt.Errorf("files should be same"u8);
    }
}

[GoType] partial struct dirLinkTest {
    internal @string name;
    internal Func<@string, @string, error> mklink;
    internal bool isMountPoint;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;

internal static void testDirLinks(ж<Δtesting.T> Ꮡt, slice<dirLinkTest> tests) {
    @string tmpdir = Ꮡt.TempDir();
    Ꮡt.Chdir(tmpdir);
    @string dir = filepath.Join(tmpdir, dirˢ);
    var err = Δos.Mkdir(dir, 511);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var fi, err) = Δos.Stat(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = Δos.WriteFile(filepath.Join(dir, abcˢ), slice<byte>("abc"u8), 420);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, test) in tests) {
        @string link = filepath.Join(tmpdir, test.name + "_link");
        var errΔ1 = test.mklink(link, dir);
        if (errΔ1 != default!) {
            Ꮡt.Errorf("creating link for %q test failed: %v"u8, test.name, errΔ1);
            continue;
        }
        (var data, errΔ1) = Δos.ReadFile(filepath.Join(link, abcˢ));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("failed to read abc file: %v"u8, errΔ1);
            continue;
        }
        if (((sstring)data) != "abc"u8) {
            Ꮡt.Errorf(@"abc file is expected to have ""abc"" in it, but has %v"u8, data);
            continue;
        }
        (var fi1, errΔ1) = Δos.Stat(link);
        if (errΔ1 != default!) {
            Ꮡt.Errorf("failed to stat link %v: %v"u8, link, errΔ1);
            continue;
        }
        {
            var tp = fi1.Mode().Type(); if (tp != fs.ModeDir) {
                Ꮡt.Errorf("Stat(%q) is type %v; want %v"u8, link, tp, fs.ModeDir);
                continue;
            }
        }
        if (fi1.Name() != filepath.Base(link)) {
            Ꮡt.Errorf("Stat(%q).Name() = %q, want %q"u8, link, fi1.Name(), filepath.Base(link));
            continue;
        }
        if (!Δos.SameFile(fi, fi1)) {
            Ꮡt.Errorf("%q should point to %q"u8, link, dir);
            continue;
        }
        (var fi2, errΔ1) = Δos.Lstat(link);
        if (errΔ1 != default!) {
            Ꮡt.Errorf("failed to lstat link %v: %v"u8, link, errΔ1);
            continue;
        }
        fs.FileMode wantType = default!;
        if (test.isMountPoint && winsymlink.Value() != "0"u8){
            // Mount points are reparse points, and we no longer treat them as symlinks.
            wantType = fs.ModeIrregular;
        } else {
            // This is either a real symlink, or a mount point treated as a symlink.
            wantType = fs.ModeSymlink;
        }
        {
            var tp = fi2.Mode().Type(); if (tp != wantType) {
                Ꮡt.Errorf("Lstat(%q) is type %v; want %v"u8, link, tp, wantType);
            }
        }
    }
}

// reparseData is used to build reparse buffer data required for tests.
[GoType] partial struct reparseData {
    internal namePosition substituteName;
    internal namePosition printName;
    internal slice<uint16> pathBuf;
}

[GoType] partial struct namePosition {
    internal uint16 offset;
    internal uint16 length;
}

[GoRecv] internal static uint16 /*offset*/ addUTF16s(this ref reparseData rd, slice<uint16> s) {
    nint off = len(rd.pathBuf) * 2;
    rd.pathBuf = appendꓸꓸꓸ(rd.pathBuf, s);
    return (uint16)off;
}

[GoRecv] internal static (uint16 offset, uint16 length) addString(this ref reparseData rd, @string s) {
    var p = syscall.StringToUTF16(s);
    return (rd.addUTF16s(p), (uint16)((uint16)(len(p) - 1) * 2)); // do not include terminating NUL in the length (as per PrintNameLength and SubstituteNameLength documentation)
}

[GoRecv] internal static void addSubstituteName(this ref reparseData rd, @string name) {
    (rd.substituteName.offset, rd.substituteName.length) = rd.addString(name);
}

[GoRecv] internal static void addPrintName(this ref reparseData rd, @string name) {
    (rd.printName.offset, rd.printName.length) = rd.addString(name);
}

[GoRecv] internal static (uint16 offset, uint16 length) addStringNoNUL(this ref reparseData rd, @string s) {
    var p = syscall.StringToUTF16(s);
    p = p.slice(0, len(p) - 1);
    return (rd.addUTF16s(p), (uint16)((uint16)len(p) * 2));
}

[GoRecv] internal static void addSubstituteNameNoNUL(this ref reparseData rd, @string name) {
    (rd.substituteName.offset, rd.substituteName.length) = rd.addStringNoNUL(name);
}

[GoRecv] internal static void addPrintNameNoNUL(this ref reparseData rd, @string name) {
    (rd.printName.offset, rd.printName.length) = rd.addStringNoNUL(name);
}

// pathBuffeLen returns length of rd pathBuf in bytes.
[GoRecv] internal static uint16 pathBuffeLen(this ref reparseData rd) {
    return (uint16)((uint16)len(rd.pathBuf) * 2);
}

// Windows REPARSE_DATA_BUFFER contains union member, and cannot be
// translated into Go directly. _REPARSE_DATA_BUFFER type is to help
// construct alternative versions of Windows REPARSE_DATA_BUFFER with
// union part of SymbolicLinkReparseBuffer or MountPointReparseBuffer type.
[GoType] partial struct _REPARSE_DATA_BUFFER {
    internal windows.REPARSE_DATA_BUFFER_HEADER header;
    internal array<byte> detail = new(syscall.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
}

internal static error createDirLink(@string link, ж<_REPARSE_DATA_BUFFER> Ꮡrdb) {
    GoFrame ᒐ = default;
    try {
        ref var rdb = ref Ꮡrdb.DerefOrNull();

        var err = Δos.Mkdir(link, 511);
        if (err != default!) {
            return err;
        }
        var linkp = syscall.StringToUTF16(link);
        (var fd, err) = syscall.CreateFile(Ꮡ(linkp, 0), syscall.GENERIC_WRITE, 0, nil, syscall.OPEN_EXISTING,
            (uint32)((uint32)syscall.FILE_FLAG_OPEN_REPARSE_POINT | (uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS), 0);
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, fd, ref ᒐ);
        var buflen = (uint32)rdb.header.ReparseDataLength + (uint32)/* unsafe.Sizeof(rdb.header) */ (uintptr)8;
        ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
        return syscall.DeviceIoControl(fd, windows.FSCTL_SET_REPARSE_POINT,
            Ꮡrdb.of(_REPARSE_DATA_BUFFER.Ꮡheader).Reinterpret<windows.REPARSE_DATA_BUFFER_HEADER, byte>(), buflen, nil, 0, ᏑbytesReturned, nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static error createMountPoint(@string link, ж<reparseData> Ꮡtarget) {
    ref var target = ref Ꮡtarget.DerefOrNull();

    ж<windows.MountPointReparseBuffer> buf = default!;
    var buflen = (uint16)((uint16)/* unsafe.Offsetof(buf.PathBuffer) */ (uintptr)8 + target.pathBuffeLen()); // see ReparseDataLength documentation
    var byteblob = new slice<byte>(buflen);
    buf = Ꮡ(byteblob, 0).Reinterpret<byte, windows.MountPointReparseBuffer>();
    buf.Value.SubstituteNameOffset = target.substituteName.offset;
    buf.Value.SubstituteNameLength = target.substituteName.length;
    buf.Value.PrintNameOffset = target.printName.offset;
    buf.Value.PrintNameLength = target.printName.length;
    nint pbuflen = len(target.pathBuf);
    copy((~array<uint16>.AliasPointer(buf.at(windows.MountPointReparseBuffer.ᏑPathBuffer, 0), 2048)).slice(0, pbuflen, pbuflen), target.pathBuf);
    ref var rdb = ref heap(new _REPARSE_DATA_BUFFER(), out var Ꮡrdb);
    rdb.header.ReparseTag = windows.IO_REPARSE_TAG_MOUNT_POINT;
    rdb.header.ReparseDataLength = buflen;
    copy(rdb.detail[..], byteblob);
    return createDirLink(link, Ꮡrdb);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmdˢ = "cmd"u8;
internal static readonly @string mklinkˢ = "mklink"u8;
internal static readonly object skippingUseMklinkCmdTestˢ = (@string)@"skipping ""use_mklink_cmd"" test, mklink does not supports directory junctions"u8;

public static void TestDirectoryJunction(ж<Δtesting.T> Ꮡt) {
// Create link similar to what mklink does, by inserting \??\ at the front of absolute target.
// Do as junction utility https://learn.microsoft.com/en-us/sysinternals/downloads/junction does - set PrintNameLength to 0.
    slice<dirLinkTest> tests = new dirLinkTest[]{
        new(
            name: "standard"u8,
            isMountPoint: true,
            mklink: (@string link, @string target) => {
                ref var tΔ1 = ref heap(new reparseData(), out var ᏑtΔ1);
                tΔ1.addSubstituteName(@"\??\"u8 + target);
                tΔ1.addPrintName(target);
                return createMountPoint(link, ᏑtΔ1);
            }
        ),
        new(
            name: "have_blank_print_name"u8,
            isMountPoint: true,
            mklink: (@string link, @string target) => {
                ref var tΔ2 = ref heap(new reparseData(), out var ᏑtΔ2);
                tΔ2.addSubstituteName(@"\??\"u8 + target);
                tΔ2.addPrintName(""u8);
                return createMountPoint(link, ᏑtΔ2);
            }
        )
    }.slice();
    var (output, _) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
    var mklinkSupportsJunctionLinks = strings.Contains(((@string)output), " /J "u8);
    if (mklinkSupportsJunctionLinks){
        tests = append(tests,
            new dirLinkTest(
                name: "use_mklink_cmd"u8,
                isMountPoint: true,
                mklink: (@string link, @string target) => {
                    var (outputΔ1, err) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/J", link, target).CombinedOutput();
                    if (err != default!) {
                        Ꮡt.Errorf("failed to run mklink %v %v: %v %q"u8, link, target, err, outputΔ1);
                    }
                    return default!;
                }
            ));
    } else {
        Ꮡt.Log(skippingUseMklinkCmdTestˢ);
    }
    testDirLinks(Ꮡt, tests);
}

internal static error enableCurrentThreadPrivilege(@string privilegeName) {
    GoFrame ᒐ = default;
    try {
        var (ct, err) = windows.GetCurrentThread();
        if (err != default!) {
            return err;
        }
        ref var t = ref heap(new syscall.Token(), out var Ꮡt);
        err = windows.OpenThreadToken(ct, (uint32)((uint32)syscall.TOKEN_QUERY | (uint32)windows.TOKEN_ADJUST_PRIVILEGES), false, Ꮡt);
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, ((syscallꓸHandle)(uintptr)t), ref ᒐ);
        ref var tp = ref heap(new windows.TOKEN_PRIVILEGES(), out var Ꮡtp);
        (var privStr, err) = syscall.UTF16PtrFromString(privilegeName);
        if (err != default!) {
            return err;
        }
        err = windows.LookupPrivilegeValue(nil, privStr, Ꮡtp.at(windows.TOKEN_PRIVILEGES.ᏑPrivileges, 0).of(windows.LUID_AND_ATTRIBUTES.ᏑLuid));
        if (err != default!) {
            return err;
        }
        tp.PrivilegeCount = 1;
        tp.Privileges[0].Attributes = windows.SE_PRIVILEGE_ENABLED;
        return windows.AdjustTokenPrivileges(t, false, Ꮡtp, 0, nil, nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static error createSymbolicLink(@string link, ж<reparseData> Ꮡtarget, bool isrelative) {
    ref var target = ref Ꮡtarget.DerefOrNull();

    ж<windows.SymbolicLinkReparseBuffer> buf = default!;
    var buflen = (uint16)((uint16)/* unsafe.Offsetof(buf.PathBuffer) */ (uintptr)12 + target.pathBuffeLen()); // see ReparseDataLength documentation
    var byteblob = new slice<byte>(buflen);
    buf = Ꮡ(byteblob, 0).Reinterpret<byte, windows.SymbolicLinkReparseBuffer>();
    buf.Value.SubstituteNameOffset = target.substituteName.offset;
    buf.Value.SubstituteNameLength = target.substituteName.length;
    buf.Value.PrintNameOffset = target.printName.offset;
    buf.Value.PrintNameLength = target.printName.length;
    if (isrelative) {
        buf.Value.Flags = windows.SYMLINK_FLAG_RELATIVE;
    }
    nint pbuflen = len(target.pathBuf);
    copy((~array<uint16>.AliasPointer(buf.at(windows.SymbolicLinkReparseBuffer.ᏑPathBuffer, 0), 2048)).slice(0, pbuflen, pbuflen), target.pathBuf);
    ref var rdb = ref heap(new _REPARSE_DATA_BUFFER(), out var Ꮡrdb);
    rdb.header.ReparseTag = syscall.IO_REPARSE_TAG_SYMLINK;
    rdb.header.ReparseDataLength = buflen;
    copy(rdb.detail[..], byteblob);
    return createDirLink(link, Ꮡrdb);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingUseMklinkCmdTestˢ2 = (@string)@"skipping ""use_mklink_cmd"" test, mklink does not supports directory symbolic links"u8;

public static void TestDirectorySymbolicLink(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        slice<dirLinkTest> tests = default!;
        var (output, _) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
        var mklinkSupportsDirectorySymbolicLinks = strings.Contains(((@string)output), " /D "u8);
        if (mklinkSupportsDirectorySymbolicLinks){
            tests = append(tests,
                new dirLinkTest(
                    name: "use_mklink_cmd"u8,
                    mklink: (@string link, @string target) => {
                        var (outputΔ1, errΔ1) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/D", link, target).CombinedOutput();
                        if (errΔ1 != default!) {
                            Ꮡt.Errorf("failed to run mklink %v %v: %v %q"u8, link, target, errΔ1, outputΔ1);
                        }
                        return default!;
                    }
                ));
        } else {
            Ꮡt.Log(skippingUseMklinkCmdTestˢ2);
        }
        // The rest of these test requires SeCreateSymbolicLinkPrivilege to be held.
        Δruntime.LockOSThread();
        defer(Δruntime.UnlockOSThread, ref ᒐ);
        var err = windows.ImpersonateSelf(windows.SecurityImpersonation);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(() => windows.RevertToSelf(), ref ᒐ);
        err = enableCurrentThreadPrivilege("SeCreateSymbolicLinkPrivilege"u8);
        if (err != default!) {
            Ꮡt.Skipf(@"skipping some tests, could not enable ""SeCreateSymbolicLinkPrivilege"": %v"u8, err);
        }
        tests = append(tests,
            new dirLinkTest(
                name: "use_os_pkg"u8,
                mklink: (@string link, @string target) => Δos.Symlink(target, link)
            ),
            new dirLinkTest( // Create link similar to what mklink does, by inserting \??\ at the front of absolute target.

                name: "standard"u8,
                mklink: (@string link, @string target) => {
                    ref var tΔ1 = ref heap(new reparseData(), out var ᏑtΔ1);
                    tΔ1.addPrintName(target);
                    tΔ1.addSubstituteName(@"\??\"u8 + target);
                    return createSymbolicLink(link, ᏑtΔ1, false);
                }
            ),
            new dirLinkTest(
                name: "relative"u8,
                mklink: (@string link, @string target) => {
                    ref var tΔ2 = ref heap(new reparseData(), out var ᏑtΔ2);
                    tΔ2.addSubstituteNameNoNUL(filepath.Base(target));
                    tΔ2.addPrintNameNoNUL(filepath.Base(target));
                    return createSymbolicLink(link, ᏑtΔ2, true);
                }
            ));
        testDirLinks(Ꮡt, tests);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lanmanWorkstationˢ = "LanmanWorkstation"u8;
internal static readonly object requiresTheWindowsˢ = (@string)"Requires the Windows service Workstation, but it is detected that it is not enabled."u8;

internal static void mustHaveWorkstation(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (mar, err) = windows.OpenSCManager(nil, nil, windows.SERVICE_QUERY_STATUS);
        if (err != default!) {
            return;
        }
        defer(syscall.CloseHandle, mar, ref ᒐ);
        //LanmanWorkstation is the service name, and Workstation is the display name.
        (var srv, err) = windows.OpenService(mar, syscall.StringToUTF16Ptr(lanmanWorkstationˢ), windows.SERVICE_QUERY_STATUS);
        if (err != default!) {
            return;
        }
        defer(syscall.CloseHandle, srv, ref ᒐ);
        ref var state = ref heap(new windows.SERVICE_STATUS(), out var Ꮡstate);
        err = windows.QueryServiceStatus(srv, Ꮡstate);
        if (err != default!) {
            return;
        }
        if (state.CurrentState != windows.SERVICE_RUNNING) {
            Ꮡt.Skip(requiresTheWindowsˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testDirˢ = "TestDir"u8;

public static void TestNetworkSymbolicLink(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
        syscall.Errno _NERR_ServerNotStarted = /* syscall.Errno(2114) */ 2114;
        @string dir = Ꮡt.TempDir();
        Ꮡt.Chdir(dir);
        nint pid = Δos.Getpid();
        @string shareName = fmt.Sprintf("GoSymbolicLinkTestShare%d"u8, pid);
        @string sharePath = filepath.Join(dir, shareName);
        @string testDir = testDirˢ;
        var err = Δos.MkdirAll(filepath.Join(sharePath, testDir), 511);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var wShareName, err) = syscall.UTF16PtrFromString(shareName);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var wSharePath, err) = syscall.UTF16PtrFromString(sharePath);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Per https://learn.microsoft.com/en-us/windows/win32/api/lmshare/ns-lmshare-share_info_2:
        //
        // “[The shi2_permissions field] indicates the shared resource's permissions
        // for servers running with share-level security. A server running user-level
        // security ignores this member.
        // …
        // Note that Windows does not support share-level security.”
        //
        // So it shouldn't matter what permissions we set here.
        const uint32 permissions = 0;
        ref var p = ref heap<windows.SHARE_INFO_2>(out var Ꮡp);
        p = new windows.SHARE_INFO_2(
            Netname: wShareName,
            Type: (uint32)((uint32)windows.STYPE_DISKTREE | (uint32)windows.STYPE_TEMPORARY),
            Remark: nil,
            Permissions: permissions,
            MaxUses: 1,
            CurrentUses: 0,
            Path: wSharePath,
            Passwd: nil
        );
        err = windows.NetShareAdd(nil, 2, Ꮡp.Reinterpret<windows.SHARE_INFO_2, byte>(), nil);
        if (err != default!) {
            if (AreEqual(err, syscall.ERROR_ACCESS_DENIED) || AreEqual(err, _NERR_ServerNotStarted)) {
                Ꮡt.Skipf("skipping: NetShareAdd: %v"u8, err);
            }
            Ꮡt.Fatal(err);
        }
        var wShareNameʗ1 = wShareName;
        defer(() => {
            var errΔ1 = windows.NetShareDel(nil, wShareNameʗ1, 0);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }, ref ᒐ);
        @string UNCPath = @"\\localhost\"u8 + shareName + @"\"u8;
        (var fi1, err) = Δos.Stat(sharePath);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var fi2, err) = Δos.Stat(UNCPath);
        if (err != default!) {
            mustHaveWorkstation(Ꮡt);
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(fi1, fi2)) {
            Ꮡt.Fatalf("%q and %q should be the same directory, but not"u8, sharePath, UNCPath);
        }
        @string target = filepath.Join(UNCPath, testDir);
        @string link = linkˢ;
        err = Δos.Symlink(target, link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Δos.Remove, link, ref ᒐ);
        (var got, err) = Δos.Readlink(link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (got != target) {
            Ꮡt.Errorf(@"os.Readlink(%#q): got %v, want %v"u8, link, got, target);
        }
        (got, err) = filepath.EvalSymlinks(link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (got != target) {
            Ꮡt.Errorf(@"filepath.EvalSymlinks(%#q): got %v, want %v"u8, link, got, target);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wslˢ = "wsl"u8;
internal static readonly object skippingWslNotDetectedˢ = (@string)"skipping: WSL not detected"u8;
internal static readonly @string binMkdirˢ = "/bin/mkdir"u8;
internal static readonly @string binLnˢ = "/bin/ln"u8;
internal static readonly object skippingWslCreatedˢ = (@string)"skipping: WSL created reparse tag IO_REPARSE_TAG_SYMLINK instead of an IO_REPARSE_TAG_LX_SYMLINK"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string targetᶜ = "target"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string linkᶜ1 = "link"u8;

public static void TestStatLxSymLink(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var (_, errΔ1) = exec.LookPath(wslˢ); if (errΔ1 != default!) {
            Ꮡt.Skip(skippingWslNotDetectedˢ);
        }
    }
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string target = targetᶜ;
    @string link = linkᶜ1;
    var (_, err) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), wslˢ, binMkdirˢ, target).Output();
    if (err != default!) {
        // This normally happens when WSL still doesn't have a distro installed to run on.
        Ꮡt.Skipf("skipping: WSL is not correctly installed: %v"u8, err);
    }
    (_, err) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), wslˢ, binLnˢ, "-s", target, link).Output();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var fi, err) = Δos.Lstat(link);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var m = fi.Mode(); if ((fs.FileMode)(m & fs.ModeSymlink) != 0) {
            // This can happen depending on newer WSL versions when running as admin or in developer mode.
            Ꮡt.Skip(skippingWslCreatedˢ);
        }
    }
    // Stat'ing a IO_REPARSE_TAG_LX_SYMLINK from outside WSL always return ERROR_CANT_ACCESS_FILE.
    // We check this condition to validate that os.Stat has tried to follow the link.
    (_, err) = Δos.Stat(link);
    syscall.Errno ERROR_CANT_ACCESS_FILE = /* syscall.Errno(1920) */ 1920;
    if (err == default! || !errors.Is(err, ERROR_CANT_ACCESS_FILE)) {
        Ꮡt.Fatalf("os.Stat(%q): got %v, want ERROR_CANT_ACCESS_FILE"u8, link, err);
    }
}

public static void TestStartProcessAttr(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (p, err) = Δos.StartProcess(Δos.Getenv(comspecˢ), new @string[]{"/c"u8, "cd"u8}.slice(), @new<Δos.ProcAttr>());
        if (err != default!) {
            return;
        }
        var pʗ1 = p;
        defer(() => pʗ1.Wait(), ref ᒐ);
        Ꮡt.Fatalf("StartProcess expected to fail, but succeeded."u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object slowTestThatUsesNetworkˢ = (@string)"slow test that uses network; skipping"u8;
internal static readonly @string noSuchServerNoSuchShareˢ = @"\\no_such_server\no_such_share\no_such_file"u8;
internal static readonly object statSucceededButExpectedˢ = (@string)"stat succeeded, but expected to fail"u8;

public static void TestShareNotExistError(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(slowTestThatUsesNetworkˢ);
    }
    Ꮡt.Parallel();
    var (_, err) = Δos.Stat(noSuchServerNoSuchShareˢ);
    if (err == default!) {
        Ꮡt.Fatal(statSucceededButExpectedˢ);
    }
    if (!Δos.IsNotExist(err)) {
        Ꮡt.Fatalf("os.Stat failed with %q, but os.IsNotExist(err) is false"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object osIsNotExistSyscallErrnoˢ = (@string)"os.IsNotExist(syscall.Errno(53)) is false, but want true"u8;

public static void TestBadNetPathError(ж<Δtesting.T> Ꮡt) {
    syscall.Errno ERROR_BAD_NETPATH = /* syscall.Errno(53) */ 53;
    if (!Δos.IsNotExist(ERROR_BAD_NETPATH)) {
        Ꮡt.Fatal(osIsNotExistSyscallErrnoˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object raceConditionOccurredˢ = (@string)"race condition occurred"u8;

public static void TestStatDir(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Chdir(Ꮡt.TempDir());
        var (f, err) = Δos.Open("."u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = Δos.Chdir(".."u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var fi2, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(fi, fi2)) {
            Ꮡt.Fatal(raceConditionOccurredˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOpenVolumeName(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string tmpdir = Ꮡt.TempDir();
        Ꮡt.Chdir(tmpdir);
        var want = new @string[]{"file1"u8, "file2"u8, "file3"u8, "gopher.txt"u8}.slice();
        slices.Sort<slice<@string>, @string>(want);
        foreach (var (_, name) in want) {
            var errΔ1 = Δos.WriteFile(filepath.Join(tmpdir, name), default!, 511);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (f, err) = Δos.Open(filepath.VolumeName(tmpdir));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var have, err) = f.Readdirnames(-1);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        slices.Sort<slice<@string>, @string>(have);
        if (strings.Join(want, "/"u8) != strings.Join(have, "/"u8)) {
            Ꮡt.Fatalf("unexpected file list %q, want %q"u8, have, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDeleteReadOnly(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string p = filepath.Join(tmpdir, "a");
    // This sets FILE_ATTRIBUTE_READONLY.
    var (f, err) = Δos.OpenFile(p, Δos.O_CREATE, 256);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();
    {
        err = Δos.Chmod(p, 256); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        err = Δos.Remove(p); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestReadStdin(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var old = poll.ReadConsole;
        var oldʗ1 = old;
        defer(() => {
            poll.ReadConsole = oldʗ1;
        }, ref ᒐ);
        var (p, err) = syscall.GetCurrentProcess();
        if (err != default!) {
            Ꮡt.Fatalf("Unable to get handle to current process: %v"u8, err);
        }
        ref var stdinDuplicate = ref heap(new syscallꓸHandle(), out var ᏑstdinDuplicate);
        err = syscall.DuplicateHandle(p, syscall.Stdin, p, ᏑstdinDuplicate, 0, false, syscall.DUPLICATE_SAME_ACCESS);
        if (err != default!) {
            Ꮡt.Fatalf("Unable to duplicate stdin: %v"u8, err);
        }
        var testConsole = os_internal_test_package.NewConsoleFile(stdinDuplicate, testˢ);
        slice<@string> tests = new @string[]{
            "abc"u8,
            "äöü"u8,
            "\u3042"u8,
            "“hi”™"u8,
            "hello\x1aworld"u8,
            "\U0001F648\U0001F649\U0001F64A"u8
        }.slice();
        foreach (var (_, consoleSize) in new nint[]{1, 2, 3, 10, 16, 100, 1000}.slice()) {
            foreach (var (_, readSize) in new nint[]{1, 2, 3, 4, 5, 8, 10, 16, 20, 50, 100}.slice()) {
                foreach (var (_, s) in tests) {
                    var testConsoleʗ1 = testConsole;
                    Ꮡt.Run(fmt.Sprintf("c%d/r%d/%s"u8, consoleSize, readSize, s), (ж<Δtesting.T> tΔ1) => {
                        ref var s16 = ref heap<slice<uint16>>(out var Ꮡs16);
                        Ꮡs16.ValueSlot = utf16.Encode(slice<rune>(s));
                        poll.ReadConsole = error (syscallꓸHandle h, ж<uint16> bufΔ1, uint32 toread, ж<uint32> read, ж<byte> inputControl) => {
                            if (inputControl != nil) {
                                tΔ1.Fatalf("inputControl not nil"u8);
                            }
                            nint n = (nint)toread;
                            if (n > consoleSize) {
                                n = consoleSize;
                            }
                            n = copy((~array<uint16>.AliasPointer(bufΔ1, 10000)).slice(0, n, n), Ꮡs16.ValueSlot);
                            Ꮡs16.ValueSlot = Ꮡs16.ValueSlot.slice(n);
                            read.Value = (uint32)n;
                            tΔ1.Logf("read %d -> %d"u8, toread, read.Value);
                            return default!;
                        };
                        slice<@string> all = default!;
                        slice<byte> buf = default!;
                        var chunk = new slice<byte>(readSize);
                        while (ᐧ) {
                            var (n, errΔ1) = testConsoleʗ1.Read(chunk);
                            buf = appendꓸꓸꓸ(buf, chunk.slice(0, n));
                            if (AreEqual(errΔ1, Δio.EOF)){
                                all = append(all, ((@string)buf));
                                if (len(all) >= 5) {
                                    break;
                                }
                                buf = buf[..0];
                            } else 
                            if (errΔ1 != default!) {
                                tΔ1.Fatalf("reading %q: error: %v"u8, s, errΔ1);
                            }
                            if (len(buf) >= 2000) {
                                tΔ1.Fatalf("reading %q: stuck in loop: %q"u8, s, buf);
                            }
                        }
                        var want = strings.Split(s, "\x1a"u8);
                        while (len(want) < 5) {
                            want = append(want, ""u8);
                        }
                        if (!slices.Equal<slice<@string>, @string>(all, want)) {
                            tΔ1.Errorf("reading %q:\nhave %x\nwant %x"u8, s, all, want);
                        }
                    });
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingBecauseCPagefileˢ = (@string)@"skipping because c:\pagefile.sys is not found"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string pathᶜ = @"c:\pagefile.sys"u8;

public static void TestStatPagefile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string path = pathᶜ;
    var (fi, err) = Δos.Stat(path);
    if (err == default!) {
        if (fi.Name() == ""u8) {
            Ꮡt.Fatalf("Stat(%q).Name() is empty"u8, path);
        }
        Ꮡt.Logf("Stat(%q).Size() = %v"u8, path, fi.Size());
        return;
    }
    if (Δos.IsNotExist(err)) {
        Ꮡt.Skip(skippingBecauseCPagefileˢ);
    }
    Ꮡt.Fatal(err);
}

// syscallCommandLineToArgv calls syscall.CommandLineToArgv
// and converts returned result into []string.
internal static (slice<@string>, error) syscallCommandLineToArgv(@string cmd) {
    GoFrame ᒐ = default;
    try {
        ref var argc = ref heap(new int32(), out var Ꮡargc);
        var (argv, err) = syscall.CommandLineToArgv(Ꮡ(syscall.StringToUTF16(cmd), 0), Ꮡargc);
        if (err != default!) {
            return (default!, err);
        }
        defer(syscall.LocalFree, ((syscallꓸHandle)(uintptr)argv), ref ᒐ);
        slice<@string> args = default!;
        foreach (var (_, v) in (argv.Value).slice(0, argc)) {
            args = append(args, syscall.UTF16ToString((v.Value)[..]));
        }
        return (args, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// compareCommandLineToArgvWithSyscall ensures that
// os.CommandLineToArgv(cmd) and syscall.CommandLineToArgv(cmd)
// return the same result.
internal static void compareCommandLineToArgvWithSyscall(ж<Δtesting.T> Ꮡt, @string cmd) {
    var (syscallArgs, err) = syscallCommandLineToArgv(cmd);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var args = os_internal_test_package.CommandLineToArgv(cmd);
    {
        @string want = fmt.Sprintf("%q"u8, syscallArgs);
        @string have = fmt.Sprintf("%q"u8, args); if (want != have) {
            Ꮡt.Errorf("testing os.commandLineToArgv(%q) failed: have %q want %q"u8, cmd, args, syscallArgs);
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainGoˢ = "main.go"u8;
internal static readonly @string mainExeˢ = "main.exe"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string progᶜ = """

package main

import (
	"fmt"
	"os"
)

func main() {
	fmt.Printf("%q", os.Args)
}

"""u8;

public static void TestCmdArgs(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δtesting.Short()) {
        Ꮡt.Skipf("in short mode; skipping test that builds a binary"u8);
    }
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string prog = progᶜ;
    @string src = filepath.Join(tmpdir, mainGoˢ);
    {
        var errΔ1 = Δos.WriteFile(src, slice<byte>(prog), 438); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string exe = filepath.Join(tmpdir, mainExeˢ);
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new os_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, src);
    cmd.Value.Dir = tmpdir;
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building main.exe failed: %v\n%s"u8, err, @out);
    }
// examples from https://learn.microsoft.com/en-us/cpp/cpp/main-function-command-line-args
// http://daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
// from 5.4  Examples
// from 5.5  Some Common Tasks
// from 5.6  The Microsoft Examples Explained
// from 5.7  Double Double Quote Examples (pre 2008)
    slice<@string> cmds = new @string[]{
        @""u8,
        @" a b c"u8,
        @" """u8,
        @" """""u8,
        @" """""""u8,
        @" """" a"u8,
        @" ""123"""u8,
        @" \""123\"""u8,
        @" \""123 456\"""u8,
        @" \\"""u8,
        @" \\\"""u8,
        @" \\\\\"""u8,
        @" \\\""x"u8,
        @" """"""""\""""\\\"""u8,
        @" abc"u8,
        @" \\\\\""""x""""""y z"u8,
        "\tb\t\"x\ty\""u8,
        @" ""Брад"" d e"u8,
        @" ""abc"" d e"u8,
        @" a\\b d""e f""g h"u8,
        @" a\\\""b c d"u8,
        @" a\\\\""b c"" d e"u8,
        @" CallMeIshmael"u8,
        @" ""Call Me Ishmael"""u8,
        @" Cal""l Me I""shmael"u8,
        @" CallMe\""Ishmael"u8,
        @" ""CallMe\""Ishmael"""u8,
        @" ""Call Me Ishmael\\"""u8,
        @" ""CallMe\\\""Ishmael"""u8,
        @" a\\\b"u8,
        @" ""a\\\b"""u8,
        @" ""\""Call Me Ishmael\"""""u8,
        @" ""C:\TEST A\\"""u8,
        @" ""\""C:\TEST A\\\"""""u8,
        @" ""a b c""  d  e"u8,
        @" ""ab\""c""  ""\\""  d"u8,
        @" a\\\b d""e f""g h"u8,
        @" a\\\""b c d"u8,
        @" a\\\\""b c"" d e"u8,
        @" ""a b c"""""u8,
        @" """"""CallMeIshmael""""""  b  c"u8,
        @" """"""Call Me Ishmael"""""""u8,
        @" """"""""Call Me Ishmael"""" b c"u8
    }.slice();
    foreach (var (_, cmdΔ1) in cmds) {
        compareCommandLineToArgvWithSyscall(Ꮡt, "test"u8 + cmdΔ1);
        compareCommandLineToArgvWithSyscall(Ꮡt, @"""cmd line"""u8 + cmdΔ1);
        compareCommandLineToArgvWithSyscall(Ꮡt, exe + cmdΔ1);
        // test both syscall.EscapeArg and os.commandLineToArgv
        var args = os_internal_test_package.CommandLineToArgv(exe + cmdΔ1);
        var (outΔ1, errΔ2) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), args[0], args[1..].ꓸꓸꓸ).CombinedOutput();
        if (errΔ2 != default!) {
            Ꮡt.Fatalf("running %q failed: %v\n%v"u8, args, errΔ2, ((@string)outΔ1));
        }
        {
            @string want = fmt.Sprintf("%q"u8, args);
            @string have = ((@string)outΔ1); if (want != have) {
                Ꮡt.Errorf("wrong output of executing %q: have %q want %q"u8, args, have, want);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userFolderˢ = "UserFolder"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string onedrivekeyᶜ = @"SOFTWARE\Microsoft\OneDrive"u8;

internal static (@string, error) findOneDriveDir() {
    GoFrame ᒐ = default;
    try {
        // as per https://stackoverflow.com/questions/42519624/how-to-determine-location-of-onedrive-on-windows-7-and-8-in-c
        @string onedrivekey = onedrivekeyᶜ;
        var (k, err) = registry.OpenKey(registry.CURRENT_USER, onedrivekey, registry.READ);
        if (err != default!) {
            return ("", fmt.Errorf("OpenKey(%q) failed: %v"u8, onedrivekey, err));
        }
        defer(() => k.Close(), ref ᒐ);
        (var path, var valtype, err) = k.GetStringValue(userFolderˢ);
        if (err != default!) {
            return ("", fmt.Errorf("reading UserFolder failed: %v"u8, err));
        }
        // REG_SZ values may also contain environment variables that need to be expanded.
        // It's recommended but not required to use REG_EXPAND_SZ for paths that contain environment variables.
        if (valtype == registry.EXPAND_SZ || valtype == registry.SZ) {
            var (expanded, errΔ1) = registry.ExpandString(path);
            if (errΔ1 != default!) {
                return ("", fmt.Errorf("expanding UserFolder failed: %v"u8, errΔ1));
            }
            path = expanded;
        }
        return (path, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// TestOneDrive verifies that OneDrive folder is a directory and not a symlink.
public static void TestOneDrive(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (dir, err) = findOneDriveDir();
    if (err != default!) {
        Ꮡt.Skipf("Skipping, because we did not find OneDrive directory: %v"u8, err);
    }
    testDirStats(Ꮡt, dir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nulˢ4 = "NUL"u8;
internal static readonly @string nulˢ5 = "nul"u8;

public static void TestWindowsDevNullFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (f1, err) = Δos.Open(nulˢ4);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var f1ʗ1 = f1;
        defer(() => f1ʗ1.Close(), ref ᒐ);
        (var fi1, err) = f1.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var f2, err) = Δos.Open(nulˢ5);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var f2ʗ1 = f2;
        defer(() => f2ʗ1.Close(), ref ᒐ);
        (var fi2, err) = f2.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(fi1, fi2)) {
            Ꮡt.Errorf(@"""NUL"" and ""nul"" are not the same file"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileStatNUL(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (f, err) = Δos.Open(nulˢ4);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var (got, want) = (fi.Mode(), (fs.FileMode)((fs.FileMode)(Δos.ModeDevice | Δos.ModeCharDevice) | 438)); if (got != want) {
                Ꮡt.Errorf("Open(%q).Stat().Mode() = %v, want %v"u8, nulˢ4, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStatNUL(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (fi, err) = Δos.Stat(nulˢ4);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (got, want) = (fi.Mode(), (fs.FileMode)((fs.FileMode)(Δos.ModeDevice | Δos.ModeCharDevice) | 438)); if (got != want) {
            Ꮡt.Errorf("Stat(%q).Mode() = %v, want %v"u8, nulˢ4, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestNoSymlinkˢ = (@string)"skipping test; no symlink support"u8;

// TestSymlinkCreation verifies that creating a symbolic link
// works on Windows when developer mode is active.
// This is supported starting Windows 10 (1703, v10.0.14972).
public static void TestSymlinkCreation(ж<Δtesting.T> Ꮡt) {
    if (!testenv.HasSymlink()) {
        Ꮡt.Skip(skippingTestNoSymlinkˢ);
    }
    Ꮡt.Parallel();
    @string temp = Ꮡt.TempDir();
    @string dummyFile = filepath.Join(temp, fileˢ2);
    {
        var err = Δos.WriteFile(dummyFile, slice<byte>(""u8), 420); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string linkFile = filepath.Join(temp, linkˢ);
    {
        var err = Δos.Symlink(dummyFile, linkFile); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// TestRootRelativeDirSymlink verifies that symlinks to paths relative to the
// drive root (beginning with "\" but no volume name) are created with the
// correct symlink type.
// (See https://golang.org/issue/39183#issuecomment-632175728.)
public static void TestRootRelativeDirSymlink(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Parallel();
        @string temp = Ꮡt.TempDir();
        @string dir = filepath.Join(temp, dirˢ);
        {
            var errΔ1 = Δos.Mkdir(dir, 493); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        @string volumeRelDir = strings.TrimPrefix(dir, filepath.VolumeName(dir)); // leaves leading backslash
        @string link = filepath.Join(temp, linkˢ);
        var err = Δos.Symlink(volumeRelDir, link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        Ꮡt.Logf("Symlink(%#q, %#q)"u8, volumeRelDir, link);
        (var f, err) = Δos.Open(link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var (fi, errΔ2) = f.Stat(); if (errΔ2 != default!){
                Ꮡt.Fatal(errΔ2);
            } else 
            if (!fi.IsDir()) {
                Ꮡt.Errorf("Open(%#q).Stat().IsDir() = false; want true"u8, f.Name());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirSubˢ = @"dir\sub"u8;

// TestWorkingDirectoryRelativeSymlink verifies that symlinks to paths relative
// to the current working directory for the drive, such as "C:File.txt", are
// correctly converted to absolute links of the correct symlink type (per
// https://docs.microsoft.com/en-us/windows/win32/fileio/creating-symbolic-links).
public static void TestWorkingDirectoryRelativeSymlink(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
        // Construct a directory to be symlinked.
        @string temp = Ꮡt.TempDir();
        {
            @string v = filepath.VolumeName(temp); if (len(v) < 2 || v[1] != (rune)':') {
                Ꮡt.Skipf("Can't test relative symlinks: t.TempDir() (%#q) does not begin with a drive letter."u8, temp);
            }
        }
        @string absDir = filepath.Join(temp, dirSubˢ);
        {
            var errΔ1 = Δos.MkdirAll(absDir, 493); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        // Change to the temporary directory and construct a
        // working-directory-relative symlink.
        var (oldwd, err) = Δos.Getwd();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        Ꮡt.Chdir(temp);
        Ꮡt.Logf("Chdir(%#q)"u8, temp);
        @string wdRelDir = filepath.VolumeName(temp) + @"dir\sub"u8; // no backslash after volume.
        @string absLink = filepath.Join(temp, linkˢ);
        err = Δos.Symlink(wdRelDir, absLink);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        Ꮡt.Logf("Symlink(%#q, %#q)"u8, wdRelDir, absLink);
        // Now change back to the original working directory and verify that the
        // symlink still refers to its original path and is correctly marked as a
        // directory.
        {
            var errΔ2 = Δos.Chdir(oldwd); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        Ꮡt.Logf("Chdir(%#q)"u8, oldwd);
        (var resolved, err) = Δos.Readlink(absLink);
        if (err != default!){
            Ꮡt.Errorf("Readlink(%#q): %v"u8, absLink, err);
        } else 
        if (resolved != absDir) {
            Ꮡt.Errorf("Readlink(%#q) = %#q; want %#q"u8, absLink, resolved, absDir);
        }
        (var linkFile, err) = Δos.Open(absLink);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var linkFileʗ1 = linkFile;
        defer(() => linkFileʗ1.Close(), ref ᒐ);
        (var linkInfo, err) = linkFile.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!linkInfo.IsDir()) {
            Ꮡt.Errorf("Open(%#q).Stat().IsDir() = false; want true"u8, absLink);
        }
        (var absInfo, err) = Δos.Stat(absDir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(absInfo, linkInfo)) {
            Ꮡt.Errorf("SameFile(Stat(%#q), Open(%#q).Stat()) = false; want true"u8, absDir, absLink);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object osStatGoUnexpectedlyˢ = (@string)@"os.Stat(""*.go"") unexpectedly succeeded"u8;

// TestStatOfInvalidName is regression test for issue #24999.
public static void TestStatOfInvalidName(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (_, err) = Δos.Stat("*.go"u8);
    if (err == default!) {
        Ꮡt.Fatal(osStatGoUnexpectedlyˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string couldNotFindUnusedDriveˢ = "Could not find unused drive letter."u8;

// findUnusedDriveLetter searches mounted drive list on the system
// (starting from Z: and ending at D:) for unused drive letter.
// It returns path to the found drive root directory (like Z:\) or error.
internal static (@string, error) findUnusedDriveLetter() {
    // Do not use A: and B:, because they are reserved for floppy drive.
    // Do not use C:, because it is normally used for main drive.
    for (var l = (rune)'Z'; l >= (rune)'D'; l--) {
        @string p = ((@string)l) + @":\"u8;
        var (_, err) = Δos.Stat(p);
        if (Δos.IsNotExist(err)) {
            return (p, default!);
        }
    }
    return ("", errors.New(couldNotFindUnusedDriveˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunTestRootDirAsTempˢ = "-test.run=^TestRootDirAsTemp$"u8;

public static void TestRootDirAsTemp(ж<Δtesting.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        fmt.Print(Δos.TempDir());
        Δos.Exit(0);
    }
    testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string exe = testenv.Executable(new os_test_package.testing_TжTB(Ꮡt));
    var (newtmp, err) = findUnusedDriveLetter();
    if (err != default!) {
        Ꮡt.Skip(err);
    }
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), exe, testRunTestRootDirAsTempˢ);
    cmd.Value.Env = cmd.Environ();
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    cmd.Value.Env = append((~cmd).Env, "TMP="u8 + newtmp);
    cmd.Value.Env = append((~cmd).Env, "TEMP="u8 + newtmp);
    (var output, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("Failed to spawn child process: %v %q"u8, err, ((@string)output));
    }
    {
        @string want = newtmp;
        @string have = ((@string)output); if (have != want) {
            Ꮡt.Fatalf("unexpected child process output %q, want %q"u8, have, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mountvolˢ = "mountvol"u8;

// replaceDriveWithVolumeID returns path with its volume name replaced with
// the mounted volume ID. E.g. C:\foo -> \\?\Volume{GUID}\foo.
internal static @string replaceDriveWithVolumeID(ж<Δtesting.T> Ꮡt, @string path) {
    Ꮡt.Helper();
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mountvolˢ, filepath.VolumeName(path), "/L");
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, @out);
    }
    @string vol = strings.Trim(((@string)@out), " \n\r"u8);
    return filepath.Join(vol, path.slice(len(filepath.VolumeName(path))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string junctionˢ = "junction"u8;

[GoType("dyn")] internal partial struct TestReadlink_tests {
    internal bool junction;
    internal bool dir;
    internal bool drive;
    internal bool relative;
}

public static void TestReadlink(ж<Δtesting.T> Ꮡt) {
    var tests = new TestReadlink_tests[]{
        new(junction: true, dir: true, drive: true, relative: false),
        new(junction: true, dir: true, drive: false, relative: false),
        new(junction: true, dir: true, drive: false, relative: true),
        new(junction: false, dir: true, drive: true, relative: false),
        new(junction: false, dir: true, drive: false, relative: false),
        new(junction: false, dir: true, drive: false, relative: true),
        new(junction: false, dir: false, drive: true, relative: false),
        new(junction: false, dir: false, drive: false, relative: false),
        new(junction: false, dir: false, drive: false, relative: true)
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var ttΔ1 = ref heap<TestReadlink_tests>(out var ᏑttΔ1);
        ttΔ1 = tt;
        @string name = default!;
        if (ttΔ1.junction){
            name = junctionˢ;
        } else {
            name = symlinkˢ;
        }
        if (ttΔ1.dir){
            name += "_dir"u8;
        } else {
            name += "_file"u8;
        }
        if (ttΔ1.drive){
            name += "_drive"u8;
        } else {
            name += "_volume"u8;
        }
        if (ttΔ1.relative){
            name += "_relative"u8;
        } else {
            name += "_absolute"u8;
        }
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
            if (!ttʗ1.junction) {
                testenv.MustHaveSymlink(new os_test_package.testing_TжTB(tΔ1));
            }
            if (!ttʗ1.relative) {
                tΔ1.Parallel();
            }
            // Make sure tmpdir is not a symlink, otherwise tests will fail.
            var (tmpdir, err) = filepath.EvalSymlinks(tΔ1.TempDir());
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            @string link = filepath.Join(tmpdir, linkˢ);
            @string target = filepath.Join(tmpdir, targetˢ);
            if (ttʗ1.dir){
                {
                    var errΔ1 = Δos.MkdirAll(target, 511); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            } else {
                {
                    var errΔ2 = Δos.WriteFile(target, default!, 438); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
            }
            @string want = default!;
            if (ttʗ1.relative){
                @string relTarget = filepath.Base(target);
                if (ttʗ1.junction){
                    want = target; // relative directory junction resolves to absolute path
                } else {
                    want = relTarget;
                }
                tΔ1.Chdir(tmpdir);
                link = filepath.Base(link);
                target = relTarget;
            } else {
                if (ttʗ1.drive){
                    want = target;
                } else {
                    @string volTarget = replaceDriveWithVolumeID(tΔ1, target);
                    if (winreadlinkvolume.Value() == "0"u8){
                        want = target;
                    } else {
                        want = volTarget;
                    }
                    target = volTarget;
                }
            }
            if (ttʗ1.junction){
                var cmd = testenv.Command(new os_test_package.testing_TжTB(tΔ1), cmdˢ, "/c"u8, mklinkˢ, "/J", link, target);
                {
                    var (@out, errΔ3) = cmd.CombinedOutput(); if (errΔ3 != default!) {
                        tΔ1.Fatalf("%v: %v\n%s"u8, cmd.OrTypedNil(), errΔ3, @out);
                    }
                }
            } else {
                {
                    var errΔ4 = Δos.Symlink(target, link); if (errΔ4 != default!) {
                        tΔ1.Fatalf("Symlink(%#q, %#q): %v"u8, target, link, errΔ4);
                    }
                }
            }
            (var got, err) = Δos.Readlink(link);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (got != want) {
                tΔ1.Fatalf("Readlink(%#q) = %#q; want %#q"u8, target, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dir1ˢ = "dir1"u8;

public static void TestOpenDirTOCTOU(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // Check opened directories can't be renamed until the handle is closed.
    // See issue 52747.
    @string tmpdir = Ꮡt.TempDir();
    @string dir = filepath.Join(tmpdir, dirˢ);
    {
        var errΔ1 = Δos.Mkdir(dir, 511); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var (f, err) = Δos.Open(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string newpath = filepath.Join(tmpdir, dir1ˢ);
    err = Δos.Rename(dir, newpath);
    if (err == default! || !errors.Is(err, windows.ERROR_SHARING_VIOLATION)) {
        f.Close();
        Ꮡt.Fatalf("Rename(%q, %q) = %v; want windows.ERROR_SHARING_VIOLATION"u8, dir, newpath, err);
    }
    f.Close();
    err = Δos.Rename(dir, newpath);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localappdataˢ = "LOCALAPPDATA"u8;
internal static readonly @string python3Exeˢ = "python3.exe"u8;
internal static readonly @string microsoftWindowsAppsˢ = @"Microsoft\WindowsApps"u8;
internal static readonly object skippingTestBecauseˢ = (@string)"skipping test, because Python 3 is not installed via the Windows App Store on this system; see https://golang.org/issue/42919"u8;

public static void TestAppExecLinkStat(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We expect executables installed to %LOCALAPPDATA%\Microsoft\WindowsApps to
    // be reparse points with tag IO_REPARSE_TAG_APPEXECLINK. Here we check that
    // such reparse points are treated as irregular (but executable) files, not
    // broken symlinks.
    @string appdata = Δos.Getenv(localappdataˢ);
    if (appdata == ""u8) {
        Ꮡt.Skipf("skipping: LOCALAPPDATA not set"u8);
    }
    @string pythonExeName = python3Exeˢ;
    @string pythonPath = filepath.Join(appdata, microsoftWindowsAppsˢ, pythonExeName);
    var (lfi, err) = Δos.Lstat(pythonPath);
    if (err != default!) {
        Ꮡt.Skip(skippingTestBecauseˢ);
    }
    // An APPEXECLINK reparse point is not a symlink, so os.Readlink should return
    // a non-nil error for it, and Stat should return results identical to Lstat.
    (var linkName, err) = Δos.Readlink(pythonPath);
    if (err == default!) {
        Ꮡt.Errorf("os.Readlink(%q) = %q, but expected an error\n(should be an APPEXECLINK reparse point, not a symlink)"u8, pythonPath, linkName);
    }
    (var sfi, err) = Δos.Stat(pythonPath);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %s: %v"u8, pythonPath, err);
    }
    if (lfi.Name() != sfi.Name()) {
        Ꮡt.Logf("os.Lstat(%q) = %+v"u8, pythonPath, lfi);
        Ꮡt.Logf("os.Stat(%q)  = %+v"u8, pythonPath, sfi);
        Ꮡt.Errorf("files should be same"u8);
    }
    if (lfi.Name() != pythonExeName) {
        Ꮡt.Errorf("Stat %s: got %q, but wanted %q"u8, pythonPath, lfi.Name(), pythonExeName);
    }
    {
        var tp = lfi.Mode().Type(); if (tp != fs.ModeIrregular) {
            // A reparse point is not a regular file, but we don't have a more appropriate
            // ModeType bit for it, so it should be marked as irregular.
            Ꮡt.Errorf("%q should not be a an irregular file (mode=0x%x)"u8, pythonPath, (uint32)tp);
        }
    }
    if (sfi.Name() != pythonExeName) {
        Ꮡt.Errorf("Stat %s: got %q, but wanted %q"u8, pythonPath, sfi.Name(), pythonExeName);
    }
    {
        var m = sfi.Mode(); if ((fs.FileMode)(m & fs.ModeSymlink) != 0) {
            Ꮡt.Errorf("%q should be a file, not a link (mode=0x%x)"u8, pythonPath, (uint32)m);
        }
    }
    {
        var m = sfi.Mode(); if ((fs.FileMode)(m & fs.ModeDir) != 0) {
            Ꮡt.Errorf("%q should be a file, not a directory (mode=0x%x)"u8, pythonPath, (uint32)m);
        }
    }
    {
        var m = sfi.Mode(); if ((fs.FileMode)(m & fs.ModeIrregular) == 0) {
            // A reparse point is not a regular file, but we don't have a more appropriate
            // ModeType bit for it, so it should be marked as irregular.
            Ꮡt.Errorf("%q should not be a regular file (mode=0x%x)"u8, pythonPath, (uint32)m);
        }
    }
    (var p, err) = exec.LookPath(pythonPath);
    if (err != default!) {
        Ꮡt.Errorf("exec.LookPath(%q): %v"u8, pythonPath, err);
    }
    if (p != pythonPath) {
        Ꮡt.Errorf("exec.LookPath(%q) = %q; want %q"u8, pythonPath, p, pythonPath);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fileNotListedˢ = (@string)"file not listed"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sepᶜ = "\\";

public static void TestIllformedUTF16FileName(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string dir = Ꮡt.TempDir();
    @string sep = sepᶜ;
    if (!strings.HasSuffix(dir, sep)) {
        dir += sep;
    }
    // This UTF-16 file name is ill-formed as it contains low surrogates that are not preceded by high surrogates ([1:5]).
    var namew = new uint16[]{0x2e, 0xdc6d, 0xdc73, 0xdc79, 0xdc73, 0x30, 0x30, 0x30, 0x31, 0}.slice();
    // Create a file whose name contains unpaired surrogates.
    // Use syscall.CreateFile instead of os.Create to simulate a file that is created by
    // a non-Go program so the file name hasn't gone through syscall.UTF16FromString.
    var dirw = utf16.Encode(slice<rune>(dir));
    var pathw = appendꓸꓸꓸ(dirw, namew);
    var (fd, err) = syscall.CreateFile(Ꮡ(pathw, 0), syscall.GENERIC_ALL, 0, nil, syscall.CREATE_NEW, 0, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    syscall.CloseHandle(fd);
    @string name = syscall.UTF16ToString(namew);
    @string path = filepath.Join(dir, name);
    // Verify that os.Lstat can query the file.
    (var fi, err) = Δos.Lstat(path);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = fi.Name(); if (got != name) {
            Ꮡt.Errorf("got %q, want %q"u8, got, name);
        }
    }
    // Verify that File.Readdirnames lists the file.
    (var f, err) = Δos.Open(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var files, err) = f.Readdirnames(0);
    f.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!slices.Contains(files, name)) {
        Ꮡt.Error(fileNotListedˢ);
    }
    // Verify that os.RemoveAll can remove the directory
    // and that it doesn't hang.
    err = Δos.RemoveAll(dir);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

public static void TestUTF16Alloc(ж<Δtesting.T> Ꮡt) {
    void allowsPerRun(nint want, Action f) {
        Ꮡt.Helper();
        nint got = (nint)Δtesting.AllocsPerRun(5, f);
        if (got != want) {
            Ꮡt.Errorf("got %d allocs, want %d"u8, got, want);
        }
    }
    allowsPerRun(1, () => {
        syscall.UTF16ToString(new uint16[]{(rune)'a', (rune)'b', (rune)'c'}.slice());
    });
    allowsPerRun(1, () => {
        syscall.UTF16FromString(abcˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidˢ = "invalid"u8;

public static void TestNewFileInvalid(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    {
        var f = Δos.NewFile((uintptr)syscall.InvalidHandle, invalidˢ); if (f != nil) {
            Ꮡt.Errorf("NewFile(InvalidHandle) got %v want nil"u8, f.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipeˢ = @"\\.\pipe\"u8;

public static void TestReadDirPipe(ж<Δtesting.T> Ꮡt) {
    @string dir = pipeˢ;
    var (fi, err) = Δos.Stat(dir);
    if (err != default! || !fi.IsDir()) {
        Ꮡt.Skipf("%s is not a directory"u8, dir);
    }
    (_, err) = Δos.ReadDir(dir);
    if (err != default!) {
        Ꮡt.Errorf("ReadDir(%q) = %v"u8, dir, err);
    }
}

public static void TestReadDirNoFileID(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        os_internal_test_package.AllowReadDirFileID.Value = false;
        defer(() => {
            os_internal_test_package.AllowReadDirFileID.Value = true;
        }, ref ᒐ);
        @string dir = Ꮡt.TempDir();
        @string pathA = filepath.Join(dir, "a");
        @string pathB = filepath.Join(dir, "b");
        {
            var errΔ1 = Δos.WriteFile(pathA, default!, 438); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var errΔ2 = Δos.WriteFile(pathB, default!, 438); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        var (files, err) = Δos.ReadDir(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(files) != 2) {
            Ꮡt.Fatalf("ReadDir(%q) = %v; want 2 files"u8, dir, files);
        }
        // Check that os.SameFile works with files returned by os.ReadDir.
        (var f1, err) = files[0].Info();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var f2, err) = files[1].Info();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(f1, f1)) {
            Ꮡt.Errorf("SameFile(%v, %v) = false; want true"u8, f1, f1);
        }
        if (!Δos.SameFile(f2, f2)) {
            Ꮡt.Errorf("SameFile(%v, %v) = false; want true"u8, f2, f2);
        }
        if (Δos.SameFile(f1, f2)) {
            Ꮡt.Errorf("SameFile(%v, %v) = true; want false"u8, f1, f2);
        }
        // Check that os.SameFile works with a mix of os.ReadDir and os.Stat files.
        (var f1s, err) = Δos.Stat(pathA);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var f2s, err) = Δos.Stat(pathB);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!Δos.SameFile(f1, f1s)) {
            Ꮡt.Errorf("SameFile(%v, %v) = false; want true"u8, f1, f1s);
        }
        if (!Δos.SameFile(f2, f2s)) {
            Ꮡt.Errorf("SameFile(%v, %v) = false; want true"u8, f2, f2s);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package
