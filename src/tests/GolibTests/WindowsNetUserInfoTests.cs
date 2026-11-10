using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using syscall = go.syscall_package;
using winint = go.@internal.syscall.windows_package;

namespace GolibTests;

// The value-level guard for syscall.NetUserGetInfo, the sixth member of the ptrout class
// (a Go `**T` OUT-PARAMETER the kernel writes a raw address into).
//
// WHY THIS EXISTS SEPARATELY FROM os/user. This member's consumer is os/user, and the class's
// standing bar is that a VALUE-LEVEL guard proves each member -- "it no longer returns nil" being
// exactly the evidence this class's history says not to trust. os/user cannot be that guard: it is
// roster-excluded E2 because Go's own TestGroupIds fails, and on a domain-joined host that cannot
// reach its DC that failure is structural, not a flap ("The specified domain either does not exist
// or could not be contacted"). A proof that needs a reachable domain controller is a proof that
// flaps with the network, so the guard is built to need neither.
//
// HOW IT AVOIDS THE DOMAIN. NetUserGetInfo with a NULL servername queries the LOCAL machine, which
// works offline and unprivileged, and the well-known built-in accounts carry values identical on
// every Windows host -- the same trick PointerOutParameter uses with well-known SIDs:
//
//     the built-in administrator (RID 500)   Priv = USER_PRIV_ADMIN (2),  PrimaryGroupID = 513
//     the built-in guest         (RID 501)   Priv = USER_PRIV_GUEST (0),  PrimaryGroupID = 513
//
// and the account NAME read back out of the record must equal the name queried -- a ROUND TRIP, so
// a wrapper that hands back the wrong address is caught by the VALUE and not merely by a crash.
//
// The accounts are found by RID, never by name (see LocalAccountNameByRid): a host may rename
// either one, and the values above belong to the account, not to its name.
//
// WHAT WOULD FAIL WITHOUT THE FIX. Before the wrapper was taken, `Ꮡbuf`'s ж<ж<byte>> slot could
// lend the kernel no eight-byte cell, so the operator answered 0, Windows read that as "no output
// wanted", the call SUCCEEDED, and the caller read back the nil it started with. That is invisible
// to a liveness check and is precisely why every assertion here is a value.
//
// Windows-only by nature (netapi32), and gated on the HOST rather than on which declaration was
// compiled. That distinction is load-bearing: a linux-flavor guard run on a Windows host sails past
// a declaration-only check and then dies in a module initializer, which reads like a regression in
// whatever is under test. A non-Windows run must read NOT MEASURED instead.
[TestClass]
public class WindowsNetUserInfoTests
{
    // Field-for-field USER_INFO_4 / USER_INFO_10 from lmaccess.h, matching the mirrors in
    // os/user's lookup_windows_impl.cs. Measured against Go on this host: sizeof(USER_INFO_4) is
    // 192 bytes and PrimaryGroupID sits at offset 160.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUserInfo4
    {
        public ushort* Name;
        public ushort* Password;
        public uint PasswordAge;
        public uint Priv;
        public ushort* HomeDir;
        public ushort* Comment;
        public uint Flags;
        public ushort* ScriptPath;
        public uint AuthFlags;
        public ushort* FullName;
        public ushort* UsrComment;
        public ushort* Parms;
        public ushort* Workstations;
        public uint LastLogon;
        public uint LastLogoff;
        public uint AcctExpires;
        public uint MaxStorage;
        public uint UnitsPerWeek;
        public byte* LogonHours;
        public uint BadPwCount;
        public uint NumLogons;
        public ushort* LogonServer;
        public uint CountryCode;
        public uint CodePage;
        public void* UserSid;
        public uint PrimaryGroupID;
        public ushort* Profile;
        public ushort* HomeDirDrive;
        public uint PasswordExpired;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUserInfo10
    {
        public ushort* Name;
        public ushort* Comment;
        public ushort* UsrComment;
        public ushort* FullName;
    }

    // The INDEPENDENT oracle: netapi32 reached directly, owing nothing to the converted surface.
    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "NetUserGetInfo")]
    private static extern uint NetUserGetInfoDirect(string? servername, string username, uint level, out IntPtr buf);

    [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
    private static extern uint NetApiBufferFreeDirect(IntPtr buf);

    private static unsafe string NativeString(ushort* p)
    {
        return p == null ? "" : new string((char*)p);
    }

    private static bool OnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    // ---- the built-in accounts, found by RID ---------------------------------------------------
    //
    // These tests first looked the accounts up by the names "Administrator" and "Guest". The hosted
    // windows runner image of 2026-11 has no local account by either name: the oracle's own
    // NetUserGetInfo answered NERR_UserNotFound (2221) for both, at master and at batch 281 alike.
    // What a rename cannot move is the relative ID in the machine's account domain: 500 is the
    // built-in administrator and 501 the built-in guest. NetUserEnum at level 20 returns each local
    // account's name and RID in one record, so the name is read from the RID. The lookup is netapi32
    // reached directly, like the oracle, so it owes nothing to the converted surface under test.
    //
    // A disabled account is still measured: NetUserGetInfo and NetUserGetLocalGroups read a disabled
    // account like any other (the built-in guest is disabled by default, and these tests measured it).

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUserInfo20
    {
        public ushort* Name;
        public ushort* FullName;
        public ushort* Comment;
        public uint Flags;
        public uint UserId;
    }

    [DllImport("netapi32.dll", EntryPoint = "NetUserEnum")]
    private static extern uint NetUserEnumDirect(IntPtr servername, uint level, uint filter, out IntPtr buf,
        uint prefMaxLen, out uint entriesRead, out uint totalEntries, ref uint resumeHandle);

    private const uint FILTER_NORMAL_ACCOUNT = 0x2;
    private const uint ERROR_MORE_DATA = 234;
    private const uint DOMAIN_USER_RID_ADMIN = 500;
    private const uint DOMAIN_USER_RID_GUEST = 501;

    // The local name of the account whose RID is rid, or null when this host has no such account.
    private static unsafe string? LocalAccountNameByRid(uint rid)
    {
        Assert.AreEqual(32, sizeof(NativeUserInfo20), "USER_INFO_20 is three pointers and two DWORDs");

        uint resume = 0;
        uint rc;

        do
        {
            rc = NetUserEnumDirect(IntPtr.Zero, 20, FILTER_NORMAL_ACCOUNT, out IntPtr buf, MAX_PREFERRED_LENGTH,
                out uint entriesRead, out _, ref resume);

            try
            {
                if (rc != 0 && rc != ERROR_MORE_DATA)
                {
                    Assert.Fail($"NetUserEnum(level 20) must succeed, got {rc}");
                }

                NativeUserInfo20* users = (NativeUserInfo20*)buf;

                for (uint i = 0; i < entriesRead; i++)
                {
                    if (users[i].UserId == rid)
                    {
                        return NativeString(users[i].Name);
                    }
                }
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    NetApiBufferFreeDirect(buf);
                }
            }
        }
        while (rc == ERROR_MORE_DATA);

        return null;
    }

    private static string BuiltInLabel(uint rid) =>
        rid == DOMAIN_USER_RID_ADMIN ? "the built-in administrator (RID 500)" : "the built-in guest (RID 501)";

    // The built-in accounts this host has, by their local names, each with its expected value. An
    // account the host lacks is left out and named in `missing`; the caller measures the rest and then
    // reads Inconclusive, never red, for the absent one.
    private static (string Account, T Expected)[] BuiltInAccounts<T>(T admin, T guest, out string missing)
    {
        var present = new System.Collections.Generic.List<(string, T)>();
        var absent = new System.Collections.Generic.List<string>();

        foreach (var (rid, expected) in new[] { (DOMAIN_USER_RID_ADMIN, admin), (DOMAIN_USER_RID_GUEST, guest) })
        {
            string? name = LocalAccountNameByRid(rid);

            if (name is null)
            {
                absent.Add(BuiltInLabel(rid));
            }
            else
            {
                Console.WriteLine($"{BuiltInLabel(rid)} is named \"{name}\" on this host");
                present.Add((name, expected));
            }
        }

        missing = string.Join(" and ", absent);
        return present.ToArray();
    }

    // The mirrors are only as good as their LAYOUT, and a silently wrong offset would read a
    // neighbouring field rather than fault. These are the numbers Go reports for the same record.
    [TestMethod]
    public unsafe void NativeMirrorsMatchTheDocumentedRecordLayout()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("netapi32 is the windows flavor");
            return;
        }

        Assert.AreEqual(192, sizeof(NativeUserInfo4), "USER_INFO_4 is 192 bytes on x64");
        Assert.AreEqual(160, (int)Marshal.OffsetOf<NativeUserInfo4>(nameof(NativeUserInfo4.PrimaryGroupID)),
            "usri4_primary_group_id sits at offset 160");
        Assert.AreEqual(32, sizeof(NativeUserInfo10), "USER_INFO_10 is four pointers");
    }

    // The wrapper must publish the kernel's buffer into the caller's pointer -- the defect this
    // member was taken for is that it published nothing while reporting success.
    [TestMethod]
    public unsafe void WrapperPublishesTheKernelsBuffer()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("netapi32 is the windows flavor");
            return;
        }

        string? admin = LocalAccountNameByRid(DOMAIN_USER_RID_ADMIN);

        if (admin is null)
        {
            Assert.Inconclusive($"this host has no {BuiltInLabel(DOMAIN_USER_RID_ADMIN)}");
            return;
        }

        var (u, uerr) = syscall.UTF16PtrFromString(admin);
        Assert.IsNull(uerr, "UTF16PtrFromString must succeed");

        ref var p = ref heap<ж<byte>>(out var Ꮡp);
        var e = syscall.NetUserGetInfo(nil, u, 10, Ꮡp);
        Assert.IsNull(e, $"NetUserGetInfo(level 10) must succeed, got: {e?.Error().ToString()}");

        try
        {
            nuint published = (nuint)(uintptr)p;
            Assert.AreNotEqual((nuint)0, published,
                "the wrapper published NOTHING -- this is the exact defect: success reported, caller's pointer left nil");

            NativeUserInfo10* info = (NativeUserInfo10*)published;
            Assert.AreEqual(admin, NativeString(info->Name),
                "the account name read back through the published buffer must round-trip");
        }
        finally
        {
            syscall.NetApiBufferFree(p);
        }
    }

    // Level 4 is the record os/user's lookupUserPrimaryGroup reads, and PrimaryGroupID is the one
    // field it takes. Both are compared against the independent P/Invoke, so this fails if the
    // converted path and netapi32 ever disagree -- including on the field's OFFSET.
    [TestMethod]
    public unsafe void ConvertedPathAgreesWithNetapi32Directly()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("netapi32 is the windows flavor");
            return;
        }

        var accounts = BuiltInAccounts(2u, 0u, out string missing);

        foreach (var (account, expectedPriv) in accounts)
        {
            // ---- the ORACLE: netapi32, reached directly ----
            uint rc = NetUserGetInfoDirect(null, account, 4, out IntPtr direct);
            Assert.AreEqual(0u, rc, $"the oracle itself must succeed for {account}");

            string oracleName;
            uint oraclePriv, oracleGid;

            try
            {
                NativeUserInfo4* o = (NativeUserInfo4*)direct;
                oracleName = NativeString(o->Name);
                oraclePriv = o->Priv;
                oracleGid = o->PrimaryGroupID;
            }
            finally
            {
                NetApiBufferFreeDirect(direct);
            }

            // Well-known and identical on every Windows host, so a wrong offset is caught by the
            // VALUE rather than only by disagreement between two equally-wrong reads.
            Assert.AreEqual(account, oracleName, $"oracle name round-trip for {account}");
            Assert.AreEqual(expectedPriv, oraclePriv, $"{account} carries the well-known privilege level");
            Assert.AreEqual(513u, oracleGid, $"{account} carries the well-known primary group RID");

            // ---- the CONVERTED path ----
            var (u, uerr) = syscall.UTF16PtrFromString(account);
            Assert.IsNull(uerr, "UTF16PtrFromString must succeed");

            ref var p = ref heap<ж<byte>>(out var Ꮡp);
            var e = syscall.NetUserGetInfo(nil, u, 4, Ꮡp);
            Assert.IsNull(e, $"NetUserGetInfo(level 4) must succeed for {account}, got: {e?.Error().ToString()}");

            try
            {
                nuint published = (nuint)(uintptr)p;
                Assert.AreNotEqual((nuint)0, published, $"the wrapper published nothing for {account}");

                NativeUserInfo4* c = (NativeUserInfo4*)published;

                Assert.AreEqual(oracleName, NativeString(c->Name), $"name must agree with netapi32 for {account}");
                Assert.AreEqual(oraclePriv, c->Priv, $"Priv must agree with netapi32 for {account}");
                Assert.AreEqual(oracleGid, c->PrimaryGroupID, $"PrimaryGroupID must agree with netapi32 for {account}");
            }
            finally
            {
                syscall.NetApiBufferFree(p);
            }
        }

        if (missing.Length > 0)
        {
            Assert.Inconclusive($"this host has no {missing}; the built-in accounts it has were measured");
        }
    }

    // ---- NetUserGetLocalGroups: the same class, a DIFFERENT fabrication route -------------------
    //
    // The buffer here is an ARRAY of LOCALGROUP_USERS_INFO_0, and os/user's converted body did not
    // Reinterpret it -- it laid a `ReadOnlySpan<LocalGroupUserInfo0>` straight over the native
    // memory. That record's single field is a `ж<uint16>`, so the span fabricates one managed
    // reference PER ELEMENT, and the loop's own `entry.Name == nil` test then reads a reference the
    // collector never handed out. Guarded the same way: walk the native stride, compare every name
    // against an independent direct P/Invoke.

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeLocalGroupUserInfo0
    {
        public ushort* Name;
    }

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "NetUserGetLocalGroups")]
    private static extern uint NetUserGetLocalGroupsDirect(string? servername, string username, uint level,
        uint flags, out IntPtr buf, uint prefMaxLen, out uint entriesRead, out uint totalEntries);

    private const uint LG_INCLUDE_INDIRECT = 0x1;
    private const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;

    [TestMethod]
    public unsafe void LocalGroupsAgreeWithNetapi32Directly()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("netapi32 is the windows flavor");
            return;
        }

        Assert.AreEqual(8, sizeof(NativeLocalGroupUserInfo0),
            "LOCALGROUP_USERS_INFO_0 is a single LPWSTR -- and that size IS the array stride");

        // Every Windows host puts these built-ins in these local groups, so the expectation is a
        // VALUE rather than "whatever both sides happened to read".
        var accounts = BuiltInAccounts("Administrators", "Guests", out string missing);

        foreach (var (account, mustContain) in accounts)
        {
            // ---- the ORACLE ----
            uint rc = NetUserGetLocalGroupsDirect(null, account, 0, LG_INCLUDE_INDIRECT,
                out IntPtr direct, MAX_PREFERRED_LENGTH, out uint oracleCount, out _);
            Assert.AreEqual(0u, rc, $"the oracle itself must succeed for {account}");

            var oracleGroups = new System.Collections.Generic.List<string>();

            try
            {
                NativeLocalGroupUserInfo0* o = (NativeLocalGroupUserInfo0*)direct;
                for (uint i = 0; i < oracleCount; i++)
                {
                    oracleGroups.Add(NativeString(o[i].Name));
                }
            }
            finally
            {
                NetApiBufferFreeDirect(direct);
            }

            Assert.IsTrue(oracleGroups.Contains(mustContain),
                $"{account} must be a member of {mustContain}; oracle saw [{string.Join(", ", oracleGroups)}]");

            // ---- the CONVERTED path ----
            var (q, qerr) = syscall.UTF16PtrFromString(account);
            Assert.IsNull(qerr, "UTF16PtrFromString must succeed");

            ref var p0 = ref heap<ж<byte>>(out var Ꮡp0);
            ref var entriesRead = ref heap(new uint32(), out var ᏑentriesRead);
            ref var totalEntries = ref heap(new uint32(), out var ᏑtotalEntries);

            var e = winint.NetUserGetLocalGroups(nil, q, 0, LG_INCLUDE_INDIRECT, Ꮡp0,
                MAX_PREFERRED_LENGTH, ᏑentriesRead, ᏑtotalEntries);
            Assert.IsNull(e, $"NetUserGetLocalGroups must succeed for {account}, got: {e?.Error().ToString()}");

            try
            {
                nuint published = (nuint)(uintptr)p0;
                Assert.AreNotEqual((nuint)0, published,
                    $"the wrapper published nothing for {account} -- success reported, caller's pointer left nil");

                Assert.AreEqual(oracleCount, (uint)entriesRead, $"entriesRead must agree with netapi32 for {account}");

                NativeLocalGroupUserInfo0* c = (NativeLocalGroupUserInfo0*)published;
                var converted = new System.Collections.Generic.List<string>();

                for (uint i = 0; i < (uint)entriesRead; i++)
                {
                    converted.Add(NativeString(c[i].Name));
                }

                CollectionAssert.AreEqual(oracleGroups, converted,
                    $"the group list must agree with netapi32 element-for-element for {account}");
            }
            finally
            {
                syscall.NetApiBufferFree(p0);
            }
        }

        if (missing.Length > 0)
        {
            Assert.Inconclusive($"this host has no {missing}; the built-in accounts it has were measured");
        }
    }
}
