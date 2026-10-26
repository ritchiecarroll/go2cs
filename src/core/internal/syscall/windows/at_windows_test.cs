// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using Δwindows = go.@internal.syscall.windows_package;
using os = os_package;
using filepath = path.filepath_package;
using syscall = syscall_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = io.fs_package;
using go.@internal.syscall;
using path;

partial class windows_test_package {

internal partial struct TestOpen_tests /*dyn*/ {
    internal @string path;
    internal nint flag;
    internal error err;
}

public static void TestOpen(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();

    @string dir = Ꮡt.TempDir();
    @string @file = filepath.Join(dir, "a");
    var (f, err) = os.Create(@file);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();

    var tests = new TestOpen_tests[]{
        new(dir, syscall.O_RDONLY, default!),
        new(dir, syscall.O_CREAT, default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_CREAT), default!),
        new(@file, (nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | os.O_CREATE), default!),
        new(@file, (nint)((nint)(nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | os.O_CREATE) | os.O_TRUNC), default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_TRUNC), syscall.ERROR_ACCESS_DENIED),
        new(dir, (nint)((nint)syscall.O_WRONLY | (nint)syscall.O_RDWR), default!), // TODO: syscall.Open returns EISDIR here, we should reconcile this
        new(dir, syscall.O_WRONLY, syscall.EISDIR),
        new(dir, syscall.O_RDWR, syscall.EISDIR)
    }.slice();
    foreach (var (i, tt) in tests) {
        @string dirΔ1 = filepath.Dir(tt.path);
        var (dirfd, errΔ1) = syscall.Open(dirΔ1, syscall.O_RDONLY, 0);
        if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
            continue;
        }
        @string @base = filepath.Base(tt.path);
        (var h, errΔ1) = Δwindows.Openat(dirfd, @base, (uint64)tt.flag, 432);
        syscall.CloseHandle(dirfd);
        if (errΔ1 == default!) {
            syscall.CloseHandle(h);
        }
        if (!AreEqual(errΔ1, tt.err)) {
            Ꮡt.Errorf("%d: Open got %q, want %q"u8, i, errΔ1, tt.err);
        }
    }
}

internal partial struct TestDeleteAt_testCases /*dyn*/ {
    internal @string name;
    internal Action<ж<testing.T>, @string> modifier;
}

public static void TestDeleteAt(ж<testing.T> Ꮡt) {
    var testCases = new TestDeleteAt_testCases[]{
        new("DeleteAt removes normal file"u8, (ж<testing.T> tΔ1, @string name) => {
        }),
        new("DeleteAt removes file with no read permission"u8, makeFileNotReadable),
        new("DeleteAt removes readonly file"u8, makeFileReadonly)
    }.slice();

    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestDeleteAt_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ2) => {
            tΔ2.Parallel();

            @string dir = tΔ2.TempDir();
            @string @file = filepath.Join(dir, "a");
            var (f, err) = os.Create(@file);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            f.Close();

            // Remove all permissions from the file.
            // Do not use os.Chmod it sets only readonly attribute on Windows.
            tcʗ1.modifier(tΔ2, @file);

            // delete file using DeleteAt
            (var dirfd, err) = syscall.Open(dir, syscall.O_RDONLY, 0);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            @string @base = filepath.Base(@file);
            err = Δwindows.Deleteat(dirfd, @base, 0);
            syscall.CloseHandle(dirfd);
            if (err != default!) {
                tΔ2.Fatalf("Deleteat failed: %v"u8, err);
            }

            // Verify the file has been deleted.
            {
                var (_, errΔ1) = os.Stat(@file); if (!os.IsNotExist(errΔ1)) {
                    tΔ2.Fatalf("file still exists after DeleteAt"u8);
                }
            }
        });
    }
}

internal static partial void makeFileReadonly(ж<testing.T> Ꮡt, @string name) {
    {
        var err = os.Chmod(name, 0); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string s130ˢ = "S-1-3-0"u8;
private static readonly @string s131ˢ = "S-1-3-1"u8;
private static readonly @string s110ˢ = "S-1-1-0"u8;

internal static void makeFileNotReadable(ж<testing.T> Ꮡt, @string name) {
    GoFrame ᒐ = default;
    try {
        var (creatorOwnerSID, err) = syscall.StringToSid(s130ˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var creatorGroupSID, err) = syscall.StringToSid(s131ˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var everyoneSID, err) = syscall.StringToSid(s110ˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }

        Δwindows.EXPLICIT_ACCESS entryForSid(ж<syscall.SID> sid) => new Δwindows.EXPLICIT_ACCESS(
                AccessPermissions: 0,
                AccessMode: Δwindows.GRANT_ACCESS,
                Inheritance: Δwindows.SUB_CONTAINERS_AND_OBJECTS_INHERIT,
                Trustee: new Δwindows.TRUSTEE(
                    TrusteeForm: Δwindows.TRUSTEE_IS_SID,
                    Name: (uintptr)sid
                )
            );

        var entries = new Δwindows.EXPLICIT_ACCESS[]{
            entryForSid(creatorOwnerSID),
            entryForSid(creatorGroupSID),
            entryForSid(everyoneSID)
        }.slice();

        ж<Δwindows.ACL> oldAcl = default!;
        ref var newAcl = ref heap<ж<Δwindows.ACL>>(out var ᏑnewAcl);
        {
            var errΔ1 = Δwindows.SetEntriesInAcl(
                (uint32)len(entries),
                Ꮡ(entries, 0),
                oldAcl,
                ᏑnewAcl); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }

        defer(syscall.LocalFree, (syscallꓸHandle)(uintptr)newAcl, ref ᒐ);
        {
            var errΔ2 = Δwindows.SetNamedSecurityInfo(
                name,
                Δwindows.SE_FILE_OBJECT,
                (uint32)((uint32)Δwindows.DACL_SECURITY_INFORMATION | (uint32)Δwindows.PROTECTED_DACL_SECURITY_INFORMATION),
                nil,
                nil,
                newAcl,
                nil); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end windows_test_package
