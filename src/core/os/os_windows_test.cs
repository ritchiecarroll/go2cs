// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// For TestRawConnReadWrite.
global using syscallDescriptor = go.syscall_package.ΔHandle;

namespace go;

using bytes = bytes_package;
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
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using time = time_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using @internal.syscall.windows;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using context = context_package;
using go.io;
using go.os;
using go.path;
using go.sync;
using static go.os_internal_test_package;
using unicode;

[GoFieldAccess(typeof(windows.MountPointReparseBuffer), typeof(windows.SymbolicLinkReparseBuffer))]
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
    (var ia3, err) = Δos.Stat(p);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!Δos.SameFile(ia1, ia3)) {
        Ꮡt.Errorf("files should be same"u8);
    }
}

partial struct dirLinkTest {
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
partial struct reparseData {
    internal namePosition substituteName;
    internal namePosition printName;
    internal slice<uint16> pathBuf;
}

partial struct namePosition {
    internal uint16 offset;
    internal uint16 length;
}

internal static uint16 /*offset*/ addUTF16s(this ref reparseData rd, slice<uint16> s) {
    nint off = len(rd.pathBuf) * 2;
    rd.pathBuf = appendꓸꓸꓸ(rd.pathBuf, s);
    return (uint16)off;
}

internal static (uint16 offset, uint16 length) addString(this ref reparseData rd, @string s) {
    var p = syscall.StringToUTF16(s);
    return (rd.addUTF16s(p), (uint16)((uint16)(len(p) - 1) * 2)); // do not include terminating NUL in the length (as per PrintNameLength and SubstituteNameLength documentation)
}

internal static void addSubstituteName(this ref reparseData rd, @string name) {
    (rd.substituteName.offset, rd.substituteName.length) = rd.addString(name);
}

internal static void addPrintName(this ref reparseData rd, @string name) {
    (rd.printName.offset, rd.printName.length) = rd.addString(name);
}

internal static (uint16 offset, uint16 length) addStringNoNUL(this ref reparseData rd, @string s) {
    var p = syscall.StringToUTF16(s);
    p = p.slice(0, len(p) - 1);
    return (rd.addUTF16s(p), (uint16)((uint16)len(p) * 2));
}

internal static void addSubstituteNameNoNUL(this ref reparseData rd, @string name) {
    (rd.substituteName.offset, rd.substituteName.length) = rd.addStringNoNUL(name);
}

internal static void addPrintNameNoNUL(this ref reparseData rd, @string name) {
    (rd.printName.offset, rd.printName.length) = rd.addStringNoNUL(name);
}

// pathBuffeLen returns length of rd pathBuf in bytes.
internal static uint16 pathBuffeLen(this ref reparseData rd) {
    return (uint16)((uint16)len(rd.pathBuf) * 2);
}

// Windows REPARSE_DATA_BUFFER contains union member, and cannot be
// translated into Go directly. _REPARSE_DATA_BUFFER type is to help
// construct alternative versions of Windows REPARSE_DATA_BUFFER with
// union part of SymbolicLinkReparseBuffer or MountPointReparseBuffer type.
partial struct _REPARSE_DATA_BUFFER {
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
    buf.SubstituteNameOffset = target.substituteName.offset;
    buf.SubstituteNameLength = target.substituteName.length;
    buf.PrintNameOffset = target.printName.offset;
    buf.PrintNameLength = target.printName.length;
    nint pbuflen = len(target.pathBuf);
    copy(array<uint16>.AliasPointer(buf.at(windows.MountPointReparseBuffer.ᏑPathBuffer, 0), 2048).slice(0, pbuflen, pbuflen), target.pathBuf);

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
    var testsᴷ = new stackAppend<dirLinkTest>();

    slice<dirLinkTest> tests = new dirLinkTest[]{
        new(
            // Create link similar to what mklink does, by inserting \??\ at the front of absolute target.
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
            // Do as junction utility https://learn.microsoft.com/en-us/sysinternals/downloads/junction does - set PrintNameLength to 0.
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
    var mklinkSupportsJunctionLinks = strings.Contains((@string)output, " /J "u8);
    if (mklinkSupportsJunctionLinks){
        tests = append(testsᴷ, tests,
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
        defer(syscall.CloseHandle, (syscallꓸHandle)(uintptr)t, ref ᒐ);

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
    buf.SubstituteNameOffset = target.substituteName.offset;
    buf.SubstituteNameLength = target.substituteName.length;
    buf.PrintNameOffset = target.printName.offset;
    buf.PrintNameLength = target.printName.length;
    if (isrelative) {
        buf.Flags = windows.SYMLINK_FLAG_RELATIVE;
    }
    nint pbuflen = len(target.pathBuf);
    copy(array<uint16>.AliasPointer(buf.at(windows.SymbolicLinkReparseBuffer.ᏑPathBuffer, 0), 2048).slice(0, pbuflen, pbuflen), target.pathBuf);

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
        var testsᴷ = new stackAppend<dirLinkTest>();

        slice<dirLinkTest> tests = default!;
        var (output, _) = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
        var mklinkSupportsDirectorySymbolicLinks = strings.Contains((@string)output, " /D "u8);
        if (mklinkSupportsDirectorySymbolicLinks){
            tests = append(testsᴷ, tests,
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
            new dirLinkTest(
                // Create link similar to what mklink does, by inserting \??\ at the front of absolute target.
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

public static partial void TestBadNetPathError(ж<Δtesting.T> Ꮡt) {
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

        if (!slices.Equal<slice<@string>, @string>(want, have)) {
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
                            n = copy(array<uint16>.AliasPointer(bufΔ1, 10000).slice(0, n, n), Ꮡs16.ValueSlot);
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
                                all = append(all, (@string)buf);
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
        var argsᴷ = new stackAppend<@string>(moves: true);

        ref var argc = ref heap(new int32(), out var Ꮡargc);
        var (argv, err) = syscall.CommandLineToArgv(Ꮡ(syscall.StringToUTF16(cmd), 0), Ꮡargc);
        if (err != default!) {
            return (default!, err);
        }
        defer(syscall.LocalFree, (syscallꓸHandle)(uintptr)argv, ref ᒐ);

        slice<@string> args = default!;
        foreach (var (_, v) in (argv.Value).slice(0, argc)) {
            args = append(argsᴷ, args, syscall.UTF16ToString((v.Value)[..]));
        }
        args = move2heap(argsᴷ, args);
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

"""u8.Raw();

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
        // examples from https://learn.microsoft.com/en-us/cpp/cpp/main-function-command-line-args
        @" ""abc"" d e"u8,
        @" a\\b d""e f""g h"u8,
        @" a\\\""b c d"u8,
        @" a\\\\""b c"" d e"u8,
        // http://daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
        // from 5.4  Examples
        @" CallMeIshmael"u8,
        @" ""Call Me Ishmael"""u8,
        @" Cal""l Me I""shmael"u8,
        @" CallMe\""Ishmael"u8,
        @" ""CallMe\""Ishmael"""u8,
        @" ""Call Me Ishmael\\"""u8,
        @" ""CallMe\\\""Ishmael"""u8,
        @" a\\\b"u8,
        @" ""a\\\b"""u8,
        // from 5.5  Some Common Tasks
        @" ""\""Call Me Ishmael\"""""u8,
        @" ""C:\TEST A\\"""u8,
        @" ""\""C:\TEST A\\\"""""u8,
        // from 5.6  The Microsoft Examples Explained
        @" ""a b c""  d  e"u8,
        @" ""ab\""c""  ""\\""  d"u8,
        @" a\\\b d""e f""g h"u8,
        @" a\\\""b c d"u8,
        @" a\\\\""b c"" d e"u8,
        // from 5.7  Double Double Quote Examples (pre 2008)
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
            Ꮡt.Fatalf("running %q failed: %v\n%v"u8, args, errΔ2, (@string)outΔ1);
        }
        {
            @string want = fmt.Sprintf("%q"u8, args);
            @string have = (@string)outΔ1; if (want != have) {
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
        Ꮡt.Fatalf("Failed to spawn child process: %v %q"u8, err, (@string)output);
    }
    {
        @string want = newtmp;
        @string have = (@string)output; if (have != want) {
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
    @string vol = strings.Trim((@string)@out, " \n\r"u8);
    return filepath.Join(vol, path.slice(len(filepath.VolumeName(path))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string junctionˢ = "junction"u8;

internal partial struct TestReadlink_tests /*dyn*/ {
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

public static partial void TestUTF16Alloc(ж<Δtesting.T> Ꮡt) {
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

public static void TestReadWriteFileOverlapped(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // See https://go.dev/issue/15388.
        Ꮡt.Parallel();

        @string name = filepath.Join(Ꮡt.TempDir(), testTxtˢ);
        var (f, err) = Δos.OpenFile(name, (nint)((nint)(nint)(Δos.O_RDWR | Δos.O_CREATE) | (nint)windows.O_FILE_FLAG_OVERLAPPED), 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var data = slice<byte>("test"u8);
        (var n, err) = f.Write(data);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (n != len(data)) {
            Ꮡt.Fatalf("Write = %d; want %d"u8, n, len(data));
        }

        {
            var (_, errΔ1) = f.Seek(0, Δio.SeekStart); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }

        (var got, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!bytes.Equal(got, data)) {
            Ꮡt.Fatalf("Read = %q; want %q"u8, got, data);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testVˢ = "-test.v"u8;

public static void TestStdinOverlappedPipe(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that we can read from a named pipe open with FILE_FLAG_OVERLAPPED.
        // See https://go.dev/issue/15388.
        if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
            ref var buf = ref heap(new @string(), out var Ꮡbuf);
            var (_, errΔ1) = fmt.Scanln(Ꮡbuf);
            if (errΔ1 != default!) {
                fmt.Print(errΔ1);
                Δos.Exit(1);
            }
            fmt.Println(buf);
            Δos.Exit(0);
        }

        Ꮡt.Parallel();
        @string name = pipeName();

        // Create the read handle inherited by the child process.
        var r = newPipe(new os_test_package.testing_TжTB(Ꮡt), name, false, true);
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);

        // Create a write handle.
        var (w, err) = Δos.OpenFile(name, Δos.O_WRONLY, 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);

        // Write some data to the pipe. The child process will read it.
        var want = slice<byte>("test\n"u8);
        {
            var (_, errΔ2) = w.Write(want); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }

        // Create a child process that will read from the pipe
        // and write the data to stdout.
        var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), testenv.Executable(new os_test_package.testing_TжTB(Ꮡt)), fmt.Sprintf("-test.run=^%s$"u8, Ꮡt.Name()), testVˢ);
        cmd = testenv.CleanCmdEnv(cmd);
        cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
        cmd.Value.Stdin = new os_test_package.os_FileжReader(r);
        (var got, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("running %q failed: %v\n%s"u8, cmd.OrTypedNil(), err, got);
        }

        if (!bytes.Contains(got, want)) {
            Ꮡt.Fatalf("output %q does not contain %q"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<Δos.File> newFileOverlapped(Δtesting.TB t, @string name, bool overlapped) {
    nint flags = (nint)(Δos.O_RDWR | Δos.O_CREATE);
    if (overlapped) {
        flags |= (nint)(windows.O_FILE_FLAG_OVERLAPPED);
    }
    var (f, err) = Δos.OpenFile(name, flags, 438);
    if (err != default!) {
        t.Fatal(err);
    }
    var fʗ1 = f;
    t.Cleanup(() => {
        {
            var errΔ1 = fʗ1.Close(); if (errΔ1 != default! && !errors.Is(errΔ1, Δos.ErrClosed)) {
                t.Fatal(errΔ1);
            }
        }
    });
    return f;
}

internal static Func<@string> currentProcess = Δsync.OnceValue(@string () => {
    // Convert the process ID to a string.
    return strconv.FormatUint((uint64)Δos.Getpid(), 10);
});

internal static readonly ж<atomic.Uint64> ᏑpipeCounter = new StaticBox<atomic.Uint64>(default(atomic.Uint64));
internal static ref atomic.Uint64 pipeCounter => ref ((StaticBox<atomic.Uint64>)ᏑpipeCounter).ValueSlot;

internal static ж<Δos.File> newBytePipe(Δtesting.TB t, @string name, bool overlapped) {
    return newPipe(t, name, false, overlapped);
}

internal static ж<Δos.File> newMessagePipe(Δtesting.TB t, @string name, bool overlapped) {
    return newPipe(t, name, true, overlapped);
}

internal static @string pipeName() {
    return @"\\.\pipe\go-os-test-"u8 + currentProcess() + @"-"u8 + strconv.FormatUint(ᏑpipeCounter.Add(1), 10);
}

internal static ж<Δos.File> newPipe(Δtesting.TB t, @string name, bool message, bool overlapped) {
    var (wname, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        t.Fatal(err);
    }
    // Create the read handle.
    nint flags = windows.PIPE_ACCESS_DUPLEX;
    if (overlapped) {
        flags |= (nint)(windows.O_FILE_FLAG_OVERLAPPED);
    }
    nint typ = (nint)((nint)windows.PIPE_TYPE_BYTE | (nint)windows.PIPE_READMODE_BYTE);
    if (message) {
        typ = (nint)((nint)windows.PIPE_TYPE_MESSAGE | (nint)windows.PIPE_READMODE_MESSAGE);
    }
    (var h, err) = windows.CreateNamedPipe(wname, (uint32)flags, (uint32)typ, 1, 4096, 4096, 0, nil);
    if (err != default!) {
        t.Fatal(err);
    }
    var f = Δos.NewFile((uintptr)h, name);
    var fʗ1 = f;
    t.Cleanup(() => {
        {
            var errΔ1 = fʗ1.Close(); if (errΔ1 != default! && !errors.Is(errΔ1, Δos.ErrClosed)) {
                t.Fatal(errΔ1);
            }
        }
    });
    return f;
}

internal static partial void testReadWrite(ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡfdr, ж<Δos.File> Ꮡfdw) {
    var write = new channel<@string>(1);
    var read = new channel<EmptyStruct>(1);
    var readʗ1 = read;
    var writeʗ1 = write;
    goǃ(() => {
        foreach (var s in writeʗ1) {
            var (n, err) = Ꮡfdw.Write(slice<byte>(s));
            readʗ1.ᐸꟷ(new EmptyStruct());
            if (err != default!) {
                Ꮡt.Error(err);
            }
            if (n != len(s)) {
                Ꮡt.Errorf("expected to write %d bytes, got %d"u8, len(s), n);
            }
        }
    });
    foreach (var i in range(10)) {
        @string s = strconv.Itoa(i);
        write.ᐸꟷ(s);
        ᐸꟷ(read);
        var buf = new slice<byte>(len(s));
        var (_, err) = Δio.ReadFull(new os_test_package.os_FileжReader(Ꮡfdr), buf);
        if (err != default!) {
            Ꮡt.Fatalf("read failed: %v"u8, err);
        }
        if (!bytes.Equal(buf, slice<byte>(s))) {
            Ꮡt.Fatalf("expected %q, got %q"u8, s, buf);
        }
    }
    close(read);
    close(write);
}

internal partial struct testPreadPwrite_op /*dyn*/ {
    internal @string s;
    internal int64 off;
}

internal static partial void testPreadPwrite(ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡfdr, ж<Δos.File> Ꮡfdw) {
    var write = new channel<testPreadPwrite_op>(1);
    var read = new channel<EmptyStruct>(1);
    var readʗ1 = read;
    var writeʗ1 = write;
    goǃ(() => {
        foreach (var iᴛ1 in writeʗ1) {
            ref var o = ref heap(new testPreadPwrite_op(), out var Ꮡo);
            o = iᴛ1;

            var (n, err) = Ꮡfdw.WriteAt(slice<byte>(o.s), o.off);
            readʗ1.ᐸꟷ(new EmptyStruct());
            if (err != default!) {
                Ꮡt.Error(err);
            }
            if (n != len(o.s)) {
                Ꮡt.Errorf("expected to write %d bytes, got %d"u8, len(o.s), n);
            }
        }
    });
    foreach (var i in range(10)) {
        var off = (int64)(i % 3); // exercise some back and forth
        @string s = strconv.Itoa(i);
        write.ᐸꟷ(new testPreadPwrite_op(s, off));
        ᐸꟷ(read);
        var buf = new slice<byte>(len(s));
        var (n, err) = Ꮡfdr.ReadAt(buf, off);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (n != len(s)) {
            Ꮡt.Fatalf("expected to read %d bytes, got %d"u8, len(s), n);
        }
        if (!bytes.Equal(buf, slice<byte>(s))) {
            Ꮡt.Fatalf("expected %q, got %q"u8, s, buf);
        }
    }
    close(read);
    close(write);
}

internal static void testFileReadEOF(ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡf) {
    var (end, err) = Ꮡf.Seek(0, Δio.SeekEnd);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    array<byte> buf = new(1);
    (var n, err) = Ꮡf.Read(buf[..]);
    if (err != default! && !AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF, got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected 0 bytes, got %d"u8, n);
    }

    (n, err) = Ꮡf.ReadAt(buf[..], end);
    if (err != default! && !AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF, got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected 0 bytes, got %d"u8, n);
    }
}

internal partial struct TestFile_tests /*dyn*/ {
    internal @string name;
    internal bool overlappedRead;
    internal bool overlappedWrite;
}

public static void TestFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var tests = new TestFile_tests[]{
        new("overlapped"u8, true, true),
        new("overlapped-read"u8, true, false),
        new("overlapped-write"u8, false, true),
        new("sync"u8, false, false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestFile_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();
            @string name = filepath.Join(tΔ1.TempDir(), fooˢ);
            var rh = newFileOverlapped(new os_test_package.testing_TжTB(tΔ1), name, ttʗ1.overlappedRead);
            var wh = newFileOverlapped(new os_test_package.testing_TжTB(tΔ1), name, ttʗ1.overlappedWrite);
            testReadWrite(tΔ1, rh, wh);
            testPreadPwrite(tΔ1, rh, wh);
            testFileReadEOF(tΔ1, rh);
        });
    }
}

public static void TestFileOverlappedSeek(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = filepath.Join(Ꮡt.TempDir(), fooˢ);
    var f = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var content = slice<byte>("foo"u8);
    {
        var (_, errΔ1) = f.Write(content); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    // Check that the file pointer is at the expected offset.
    var (n, err) = f.Seek(0, Δio.SeekCurrent);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n != (int64)len(content)) {
        Ꮡt.Errorf("expected file pointer to be at offset %d, got %d"u8, len(content), n);
    }
    // Set the file pointer to the start of the file.
    {
        var (_, errΔ2) = f.Seek(0, Δio.SeekStart); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Read the first byte.
    array<byte> buf = new(1);
    {
        var (_, errΔ3) = f.Read(buf[..]); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    if (!bytes.Equal(buf[..], content[..(int)(len(buf))])) {
        Ꮡt.Errorf("expected %q, got %q"u8, content[..(int)(len(buf))], buf[..]);
    }
    // Check that the file pointer is at the expected offset.
    (n, err) = f.Seek(0, Δio.SeekCurrent);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n != (int64)len(buf)) {
        Ꮡt.Errorf("expected file pointer to be at offset %d, got %d"u8, len(buf), n);
    }
    {
        (n, err) = f.Seek(1, Δio.SeekStart); if (err != default!){
            Ꮡt.Fatal(err);
        } else 
        if (n != 1) {
            Ꮡt.Errorf("expected file pointer to be at offset %d, got %d"u8, (nint)1, n);
        }
    }
    {
        (n, err) = f.Seek(-1, Δio.SeekEnd); if (err != default!){
            Ꮡt.Fatal(err);
        } else 
        if (n != (int64)(len(content) - 1)) {
            Ꮡt.Errorf("expected file pointer to be at offset %d, got %d"u8, len(content) - 1, n);
        }
    }
    {
        var (_, errΔ4) = f.Seek(-1, Δio.SeekStart); if (!errors.Is(errΔ4, windows.ERROR_NEGATIVE_SEEK)) {
            Ꮡt.Errorf("expected ERROR_NEGATIVE_SEEK, got %v"u8, errΔ4);
        }
    }
    {
        var (_, errΔ5) = f.Seek(0, -1); if (!errors.Is(errΔ5, windows.ERROR_INVALID_PARAMETER)) {
            Ꮡt.Errorf("expected ERROR_INVALID_PARAMETER, got %v"u8, errΔ5);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestAccessDeniedˢ = (@string)"skipping test: access denied"u8;

public static void TestFileOverlappedReadAtSeekVolume(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that we can use File.ReadAt and File.Seek with an overlapped volume handle.
        // See https://go.dev/issues/74951.
        Ꮡt.Parallel();
        @string name = @"\\.\"u8 + filepath.VolumeName(Ꮡt.TempDir());
        var (f, err) = Δos.OpenFile(name, (nint)((nint)(nint)(Δos.O_RDWR | Δos.O_CREATE) | (nint)windows.O_FILE_FLAG_OVERLAPPED), 438);
        if (err != default!) {
            if (errors.Is(err, syscall.ERROR_ACCESS_DENIED)) {
                Ꮡt.Skip(skippingTestAccessDeniedˢ);
            }
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        array<byte> buf = new(0);
        {
            var (_, errΔ1) = f.ReadAt(buf[..], 0); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var (_, errΔ2) = f.Seek(0, Δio.SeekCurrent); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPipe(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        var wʗ1 = w;
        defer(() => {
            {
                var errΔ1 = rʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                var errΔ2 = wʗ1.Close(); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
        }, ref ᒐ);
        testReadWrite(Ꮡt, r, w);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestNamedPipe_tests /*dyn*/ {
    internal @string name;
    internal bool overlappedRead;
    internal bool overlappedWrite;
}

public static void TestNamedPipe(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var tests = new TestNamedPipe_tests[]{
        new("overlapped"u8, true, true),
        new("overlapped-write"u8, false, true),
        new("overlapped-read"u8, true, false),
        new("sync"u8, false, false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestNamedPipe_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();
            @string name = pipeName();
            var pipe = newBytePipe(new os_test_package.testing_TжTB(tΔ1), name, ttʗ1.overlappedWrite);
            var @file = newFileOverlapped(new os_test_package.testing_TжTB(tΔ1), name, ttʗ1.overlappedRead);
            testReadWrite(tΔ1, pipe, @file);
        });
    }
}

public static void TestPipeMessageReadEOF(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = pipeName();
    var pipe = newMessagePipe(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true);

    var (_, err) = pipe.Write(default!);
    if (err != default!) {
        Ꮡt.Error(err);
    }

    array<byte> buf = new(10);
    (var n, err) = @file.Read(buf[..]);
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF, got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected 0 bytes, got %d"u8, n);
    }
}

public static void TestPipeClosedEOF(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = pipeName();
    var pipe = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true);

    pipe.Close();

    array<byte> buf = new(10);
    var (n, err) = @file.Read(buf[..]);
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF, got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected 0 bytes, got %d"u8, n);
    }
}

public static void TestPipeReadTimeout(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = pipeName();
    _ = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true);

    var err = @file.SetReadDeadline(time.Now().Add(time.Millisecond));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    array<byte> buf = new(10);
    (_, err) = @file.Read(buf[..]);
    if (!errors.Is(err, Δos.ErrDeadlineExceeded)) {
        Ꮡt.Errorf("expected deadline exceeded, got %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tookTooLongToCancelˢ = (@string)"took too long to cancel"u8;

public static partial void TestPipeCanceled(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = pipeName();
    _ = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true);
    var ch = new channel<EmptyStruct>(1);
    var chʗ1 = ch;
    var fileʗ1 = @file;
    goǃ(() => {
        while (ᐧ) {
            var selᴛ5 = chʗ1;
            switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
            case 0 when selᴛ5.ꟷᐳ(out _): {
                return;
            }
            default: {
                var (sc, errΔ1) = fileʗ1.SyscallConn();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                {
                    var errΔ2 = sc.Control((uintptr fd) => {
                        syscall.CancelIoEx((syscallꓸHandle)fd, nil);
                    }); if (errΔ2 != default!) {
                        Ꮡt.Error(errΔ2);
                        return;
                    }
                }
                time.Sleep(100 * time.Millisecond);
                break;
            }}
        }
    });
    array<byte> tmp = new(1);
    // Read will block until the cancel is complete.
    var (_, err) = @file.Read(tmp[..]);
    ch.ᐸꟷ(new EmptyStruct());
    if (errors.Is(err, Δos.ErrDeadlineExceeded)) {
        Ꮡt.Skip(tookTooLongToCancelˢ);
    }
    if (!errors.Is(err, syscall.ERROR_OPERATION_ABORTED)) {
        Ꮡt.Errorf("expected ERROR_OPERATION_ABORTED, got %v"u8, err);
    }
}

internal static error iocpAssociateFile(ж<Δos.File> Ꮡf, syscallꓸHandle iocp) {
    var (_, err) = windows.CreateIoCompletionPort((syscallꓸHandle)Ꮡf.Fd(), iocp, 0, 0);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedQueuedˢ = (@string)"unexpected queued completion"u8;

public static partial void TestFileAssociatedWithExternalIOCP(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that a caller can associate an overlapped handle to an external IOCP
        // after the handle has been passed to os.NewFile.
        // Also test that the File can perform I/O after it is associated with the
        // external IOCP and that those operations do not post to the external IOCP.
        Ꮡt.Parallel();
        @string name = pipeName();
        var pipe = newMessagePipe(new os_test_package.testing_TжTB(Ꮡt), name, true);
        _ = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, true); // just open a pipe client

        // Use a file to exercise WriteAt.
        var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), filepath.Join(Ꮡt.TempDir(), "a"), true);

        var (iocp, err) = windows.CreateIoCompletionPort(syscall.InvalidHandle, 0, 0, 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(() => {
            if (iocp == syscall.InvalidHandle) {
                // Already closed at the end of the test.
                return;
            }
            {
                var errΔ1 = syscall.CloseHandle(iocp); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }, ref ᒐ);

        var ch = new channel<error>(1);
        var chʗ1 = ch;
        goǃ(() => {
            ref var bytes = ref heap(new uint32(), out var Ꮡbytes);
            ref var key = ref heap(new uint32(), out var Ꮡkey);
            ref var overlapped = ref heap<ж<syscall.Overlapped>>(out var Ꮡoverlapped);
            var errΔ2 = syscall.GetQueuedCompletionStatus(iocp, Ꮡbytes, Ꮡkey, Ꮡoverlapped, syscall.INFINITE);
            chʗ1.ᐸꟷ(errΔ2);
        });

        {
            var errΔ3 = iocpAssociateFile(pipe, iocp); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            var errΔ4 = iocpAssociateFile(@file, iocp); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }

        {
            var (_, errΔ5) = pipe.Write(slice<byte>("hello"u8)); if (errΔ5 != default!) {
                Ꮡt.Fatal(errΔ5);
            }
        }
        {
            var (_, errΔ6) = @file.Write(slice<byte>("hello"u8)); if (errΔ6 != default!) {
                Ꮡt.Fatal(errΔ6);
            }
        }
        {
            var (_, errΔ7) = @file.WriteAt(slice<byte>("hello"u8), 0); if (errΔ7 != default!) {
                Ꮡt.Fatal(errΔ7);
            }
        }

        // Wait fot he goroutine to call GetQueuedCompletionStatus.
        time.Sleep(100 * time.Millisecond);

        // Trigger ERROR_ABANDONED_WAIT_0.
        {
            var errΔ8 = syscall.CloseHandle(iocp); if (errΔ8 != default!) {
                Ꮡt.Fatal(errΔ8);
            }
        }

        // Wait for the completion to be posted to the IOCP.
        err = ᐸꟷ(ch);
        iocp = syscall.InvalidHandle;
        syscall.Errno ERROR_ABANDONED_WAIT_0 = /* syscall.Errno(735) */ 735;
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, ERROR_ABANDONED_WAIT_0)) {
            // This is what we expect.
        }
        else if (AreEqual(exprᴛ1, default!)) {
            Ꮡt.Error(unexpectedQueuedˢ);
        }
        else { /* default: */
            Ꮡt.Error(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestFileWriteFdRace(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();

        var f = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), filepath.Join(Ꮡt.TempDir(), "a"), true);

        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);

        var fʗ1 = f;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (n, errΔ1) = fʗ1.Write(slice<byte>("hi"u8));
                if (errΔ1 != default!) {
                    // We look at error strings as the
                    // expected errors are OS-specific.
                    switch (ᐧ) {
                    case {} when errors.Is(errΔ1, windows.ERROR_INVALID_HANDLE): {
                        // Ignore an expected error.
                        break;
                    }
                    default: {
                        // Unexpected error.
                        Ꮡt.Error(errΔ1);
                        break;
                    }}
                    return;
                }
                if (n != 2) {
                    Ꮡt.Errorf("wrote %d bytes, expected 2"u8, n);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var fʗ2 = f;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                fʗ2.Fd();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();

        var (iocp, err) = windows.CreateIoCompletionPort(syscall.InvalidHandle, 0, 0, 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(syscall.CloseHandle, iocp, ref ᒐ);
        {
            var errΔ2 = iocpAssociateFile(f, iocp); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }

        {
            var (_, errΔ3) = f.Write(slice<byte>("hi"u8)); if (errΔ3 != default!) {
                Ꮡt.Error(errΔ3);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestSplitPath_type /*dyn*/ {
    internal @string path, wantDir, wantBase;
}

public static void TestSplitPath(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in new TestSplitPath_type[]{
        new(@"a"u8, @"."u8, @"a"u8),
        new(@"a\"u8, @"."u8, @"a"u8),
        new(@"a\\"u8, @"."u8, @"a"u8),
        new(@"a\b"u8, @"a"u8, @"b"u8),
        new(@"a\\b"u8, @"a"u8, @"b"u8),
        new(@"a\b\"u8, @"a"u8, @"b"u8),
        new(@"a\b\c"u8, @"a\b"u8, @"c"u8),
        new(@"\a"u8, @"\"u8, @"a"u8),
        new(@"\a\"u8, @"\"u8, @"a"u8),
        new(@"\a\b"u8, @"\a"u8, @"b"u8),
        new(@"\a\b\"u8, @"\a"u8, @"b"u8),
        new(@"\a\b\c"u8, @"\a\b"u8, @"c"u8),
        new(@"\\a"u8, @"\\a"u8, @"."u8),
        new(@"\\a\"u8, @"\\a\"u8, @"."u8),
        new(@"\\\a"u8, @"\\\a"u8, @"."u8),
        new(@"\\\a\"u8, @"\\\a"u8, @"."u8),
        new(@"\\a\b\c"u8, @"\\a\b"u8, @"c"u8),
        new(@"c:"u8, @"c:"u8, @"."u8),
        new(@"c:\"u8, @"c:\"u8, @"."u8),
        new(@"c:\a"u8, @"c:\"u8, @"a"u8),
        new(@"c:a"u8, @"c:"u8, @"a"u8),
        new(@"c:a\b\"u8, @"c:a"u8, @"b"u8),
        new(@"c:base"u8, @"c:"u8, @"base"u8),
        new(@"a/b/c"u8, @"a/b"u8, @"c"u8),
        new(@"a/b/c/"u8, @"a/b"u8, @"c"u8),
        new(@"\\?\c:\a"u8, @"\\?\c:\"u8, @"a"u8)
    }.slice()) {
        {
            var (dir, @base) = os_internal_test_package.SplitPath(tt.path); if (dir != tt.wantDir || @base != tt.wantBase) {
                Ꮡt.Errorf("splitPath(%q) = %q, %q, want %q, %q"u8, tt.path, dir, @base, tt.wantDir, tt.wantBase);
            }
        }
    }
}

internal partial struct TestOpenFileFlags_tests /*dyn*/ {
    internal uint32 flag;
    internal uint32 wantMode;
}

public static void TestOpenFileFlags(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();

    // The only way to retrieve some of the flags passed in CreateFile
    // is using NtQueryInformationFile, which returns the file flags
    // NT equivalent. Note that FILE_SYNCHRONOUS_IO_NONALERT is always
    // set when FILE_FLAG_OVERLAPPED is not passed.
    // The flags that can't be retrieved using NtQueryInformationFile won't
    // be tested in here, but we at least know that the logic to handle them is correct.
    var tests = new TestOpenFileFlags_tests[]{
        new(0, windows.FILE_SYNCHRONOUS_IO_NONALERT),
        new(windows.O_FILE_FLAG_OVERLAPPED, 0),
        new(windows.O_FILE_FLAG_NO_BUFFERING, (uint32)((uint32)windows.FILE_NO_INTERMEDIATE_BUFFERING | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT)),
        new((uint32)((uint32)windows.O_FILE_FLAG_NO_BUFFERING | (uint32)windows.O_FILE_FLAG_OVERLAPPED), windows.FILE_NO_INTERMEDIATE_BUFFERING),
        new(windows.O_FILE_FLAG_SEQUENTIAL_SCAN, (uint32)((uint32)windows.FILE_SEQUENTIAL_ONLY | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT)),
        new(windows.O_FILE_FLAG_WRITE_THROUGH, (uint32)((uint32)windows.FILE_WRITE_THROUGH | (uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT))
    }.slice();
    foreach (var (i, vᴛ1) in tests) {
        ref var tt = ref heap(new TestOpenFileFlags_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(strconv.Itoa(i), (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();
                var (f, err) = Δos.OpenFile(filepath.Join(tΔ1.TempDir(), testTxtˢ), (nint)((nint)(nint)((nint)syscall.O_RDWR | (nint)syscall.O_CREAT) | (nint)ttʗ1.flag), 438);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                ref var info = ref heap(new windows.FILE_MODE_INFORMATION(), out var Ꮡinfo);
                {
                    var errΔ1 = windows.NtQueryInformationFile((syscallꓸHandle)f.Fd(), Ꮡ(new windows.IO_STATUS_BLOCK(nil)),
                        @unsafe.Pointer.FromPinnedBox(Ꮡinfo), (uint32)/* unsafe.Sizeof(info) */ (uintptr)4, windows.FileModeInformation); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
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

public static void TestOpenFileDeleteOnClose(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string name = filepath.Join(Ꮡt.TempDir(), testTxtˢ);
    var (f, err) = Δos.OpenFile(name, (nint)((nint)(UntypedInt)(syscall.O_RDWR | syscall.O_CREAT) | (nint)windows.O_FILE_FLAG_DELETE_ON_CLOSE), 438);
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
        var (_, errΔ2) = Δos.Stat(name); if (!errors.Is(errΔ2, Δos.ErrNotExist)) {
            Ꮡt.Errorf("expected file to be deleted, got %v"u8, errΔ2);
        }
    }
}

public static void TestOpenFileFlagInvalid(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // invalidFileFlag is the only value in the file flag range that is not supported,
    // as it is not defined in the Windows API.
    UntypedInt invalidFileFlag = 0x00400000;
    var (f, err) = Δos.OpenFile(filepath.Join(Ꮡt.TempDir(), testTxtˢ), (nint)((nint)(UntypedInt)(syscall.O_RDWR | syscall.O_CREAT) | (nint)invalidFileFlag), 438);
    if (!errors.Is(err, Δos.ErrInvalid)) {
        Ꮡt.Fatalf("expected os.ErrInvalid, got %v"u8, err);
    }
    f.Close();
}

public static void TestOpenFileTruncateNamedPipe(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string name = pipeName();
        var pipe = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, false);
        var pipeʗ1 = pipe;
        defer(() => pipeʗ1.Close(), ref ᒐ);

        var (f, err) = Δos.OpenFile(name, (nint)((nint)(nint)(Δos.O_TRUNC | Δos.O_RDWR) | Δos.O_CREATE), 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        f.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newFileReturnedNilˢ = (@string)"NewFile returned nil"u8;

internal partial struct TestFileKeepsCompletionNotificationModes_type /*dyn*/ {
    internal @string name;
    internal uint8 modes;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "hello"u8;

public static void TestFileKeepsCompletionNotificationModes(ж<Δtesting.T> Ꮡt) {
    // NewFile must preserve completion notification modes and perform I/O
    // correctly with each combination. See go.dev/issue/80979.
    Ꮡt.Parallel();
    foreach (var (_, vᴛ1) in new TestFileKeepsCompletionNotificationModes_type[]{
        new("none"u8, 0),
        new("skipSuccess"u8, syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS),
        new("skipEvent"u8, syscall.FILE_SKIP_SET_EVENT_ON_HANDLE),
        new("both"u8, (uint8)((uint8)syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS | (uint8)syscall.FILE_SKIP_SET_EVENT_ON_HANDLE))
    }.slice()) {
        ref var tt = ref heap(new TestFileKeepsCompletionNotificationModes_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();
                @string name = filepath.Join(tΔ1.TempDir(), fileˢ2);
                var (namep, err) = syscall.UTF16PtrFromString(name);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                (var h, err) = syscall.CreateFile(namep, (uint32)((uint32)syscall.GENERIC_READ | (uint32)syscall.GENERIC_WRITE),
                    0, nil, syscall.CREATE_ALWAYS, syscall.FILE_FLAG_OVERLAPPED, 0);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                {
                    var errΔ1 = syscall.SetFileCompletionNotificationModes(h, ttʗ1.modes); if (errΔ1 != default!) {
                        syscall.CloseHandle(h);
                        tΔ1.Fatal(errΔ1);
                    }
                }
                var f = Δos.NewFile((uintptr)h, name);
                if (f == nil) {
                    syscall.CloseHandle(h);
                    tΔ1.Fatal(newFileReturnedNilˢ);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);

                // Query h directly: calling f.Fd would disassociate it from the poller.
                ref var info = ref heap(new windows.FILE_IO_COMPLETION_NOTIFICATION_INFORMATION(), out var Ꮡinfo);
                {
                    var errΔ2 = windows.NtQueryInformationFile(h, Ꮡ(new windows.IO_STATUS_BLOCK(nil)),
                        @unsafe.Pointer.FromPinnedBox(Ꮡinfo), (uint32)/* unsafe.Sizeof(info) */ (uintptr)4, windows.FileIoCompletionNotificationInformation); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                if (info.Flags != (uint32)ttʗ1.modes) {
                    tΔ1.Fatalf("completion modes = %#x; want %#x"u8, info.Flags, ttʗ1.modes);
                }
                // Check that NewFile has initialized the runtime poller.
                {
                    var errΔ3 = f.SetDeadline(new time.Time(nil)); if (errΔ3 != default!) {
                        tΔ1.Fatal(errΔ3);
                    }
                }
                @string want = wantᶜ;
                {
                    var (n, errΔ4) = f.Write(slice<byte>(want)); if (errΔ4 != default! || n != len(want)) {
                        tΔ1.Fatalf("Write = %d, %v; want %d, nil"u8, n, errΔ4, len(want));
                    }
                }
                var buf = new slice<byte>(len(want));
                {
                    var (n, errΔ5) = f.ReadAt(buf, 0); if (errΔ5 != default! || n != len(want)) {
                        tΔ1.Fatalf("ReadAt = %d, %v; want %d, nil"u8, n, errΔ5, len(want));
                    }
                }
                if (((sstring)buf) != want) {
                    tΔ1.Fatalf("ReadAt returned %q; want %q"u8, buf, want);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestNewFileStdinBlocked(ж<Δtesting.T> Ꮡt) {
    // See https://go.dev/issue/75949.
    Ꮡt.Parallel();

    // Use a subprocess to test that os.NewFile on a blocked stdin works.
    // Can't do it in the same process because os.NewFile would close
    // stdin for the whole test process once the test ends.
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        // In the child process, just exit.
        // If we get here, the os package successfully initialized.
        Δos.Exit(0);
    }
    @string name = pipeName();
    var stdin = newBytePipe(new os_test_package.testing_TжTB(Ꮡt), name, false);
    var @file = newFileOverlapped(new os_test_package.testing_TжTB(Ꮡt), name, false);

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var stdinʗ1 = stdin;
    Ꮡwg.Go(() => {
        // Block stdin on a read.
        {
            var (_, err) = stdinʗ1.Read(new slice<byte>(1)); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
    });

    time.Sleep(100 * time.Millisecond); // Give time for the read to start.
    var cmd = testenv.CommandContext(new os_test_package.testing_TжTB(Ꮡt), Ꮡt.Context(), testenv.Executable(new os_test_package.testing_TжTB(Ꮡt)), fmt.Sprintf("-test.run=^%s$"u8, Ꮡt.Name()));
    cmd.Value.Env = cmd.Environ();
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    cmd.Value.Stdin = new os_test_package.os_FileжReader(stdin);
    {
        var err = cmd.Run(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    // Unblock the read to let the goroutine exit.
    {
        var (_, err) = @file.Write(new slice<byte>(1)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    Ꮡwg.Wait(); // Don't leave goroutines behind.
}

} // end os_test_package
