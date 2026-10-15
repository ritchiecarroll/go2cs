// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.archive;

using fs = go.io.fs_package;
using user = os.user_package;
using runtime = runtime_package;
using strconv = strconv_package;
using sync = sync_package;
using syscall = syscall_package;
using go.io;
using os;
using time = time_package;

partial class tar_package {

[GoInit] internal static void init() {
    sysStat = statUnix;
}

// userMap and groupMap cache UID and GID lookups for performance reasons.
// The downside is that renaming uname or gname by the OS never takes effect.
internal static ж<sync.Map> ᏑuserMap = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map userMap => ref ᏑuserMap.Value;       // map[int]string
internal static ж<sync.Map> ᏑgroupMap = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map groupMap => ref ᏑgroupMap.Value;

internal static error statUnix(fs.FileInfo fi, ж<Header> Ꮡh, bool doNameLookups) {
    ref var h = ref Ꮡh.DerefOrNull();

    var (sys, ok) = fi.Sys()._<ж<syscall.Stat_t>>(ᐧ);
    if (!ok) {
        return default!;
    }
    h.Uid = (nint)(~sys).Uid;
    h.Gid = (nint)(~sys).Gid;
    if (doNameLookups) {
        // Best effort at populating Uname and Gname.
        // The os/user functions may fail for any number of reasons
        // (not implemented on that platform, cgo not enabled, etc).
        {
            var (u, okΔ1) = ᏑuserMap.Load(h.Uid); if (okΔ1){
                h.Uname = u._<@string>();
            } else 
            {
                var (uΔ1, err) = user.LookupId(strconv.Itoa(h.Uid)); if (err == default!) {
                    h.Uname = uΔ1.Value.Username;
                    ᏑuserMap.Store(h.Uid, h.Uname);
                }
            }
        }
        {
            var (g, okΔ2) = ᏑgroupMap.Load(h.Gid); if (okΔ2){
                h.Gname = g._<@string>();
            } else 
            {
                var (gΔ1, err) = user.LookupGroupId(strconv.Itoa(h.Gid)); if (err == default!) {
                    h.Gname = gΔ1.Value.Name;
                    ᏑgroupMap.Store(h.Gid, h.Gname);
                }
            }
        }
    }
    h.AccessTime = statAtime(sys);
    h.ChangeTime = statCtime(sys);

    // Best effort at populating Devmajor and Devminor.
    if (h.Typeflag == TypeChar || h.Typeflag == TypeBlock) {
        var dev = (uint64)(~sys).Rdev; // May be int32 or uint32
        var exprᴛ1 = runtime.GOOS;
        if (exprᴛ1 == "aix"u8) {
            uint32 major = default!;
            uint32 minor = default!;
            major = (uint32)((((uint64)(dev & 0x3fffffff00000000UL)) >> (int)(32)));
            minor = (uint32)((((uint64)(dev & 0x00000000ffffffffU)) >> (int)(0)));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "linux"u8) {
            // Copied from golang.org/x/sys/unix/dev_linux.go.
            var major = (uint32)((((uint64)(dev & 0x00000000000fff00)) >> (int)(8)));
            major |= (uint32)((uint32)((((uint64)(dev & 0xfffff00000000000UL)) >> (int)(32))));
            var minor = (uint32)((((uint64)(dev & 0x00000000000000ff)) >> (int)(0)));
            minor |= (uint32)((uint32)((((uint64)(dev & 0x00000ffffff00000UL)) >> (int)(12))));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
            // Copied from golang.org/x/sys/unix/dev_darwin.go.
            var major = (uint32)((uint64)(((dev >> (int)(24))) & 0xff));
            var minor = (uint32)((uint64)(dev & 0xffffff));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "dragonfly"u8) {
            // Copied from golang.org/x/sys/unix/dev_dragonfly.go.
            var major = (uint32)((uint64)(((dev >> (int)(8))) & 0xff));
            var minor = (uint32)((uint64)(dev & 0xffff00ffU));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "freebsd"u8) {
            // Copied from golang.org/x/sys/unix/dev_freebsd.go.
            var major = (uint32)((uint64)(((dev >> (int)(8))) & 0xff));
            var minor = (uint32)((uint64)(dev & 0xffff00ffU));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "netbsd"u8) {
            // Copied from golang.org/x/sys/unix/dev_netbsd.go.
            var major = (uint32)((((uint64)(dev & 0x000fff00)) >> (int)(8)));
            var minor = (uint32)((((uint64)(dev & 0x000000ff)) >> (int)(0)));
            minor |= (uint32)((uint32)((((uint64)(dev & 0xfff00000U)) >> (int)(12))));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else if (exprᴛ1 == "openbsd"u8) {
            // Copied from golang.org/x/sys/unix/dev_openbsd.go.
            var major = (uint32)((((uint64)(dev & 0x0000ff00)) >> (int)(8)));
            var minor = (uint32)((((uint64)(dev & 0x000000ff)) >> (int)(0)));
            minor |= (uint32)((uint32)((((uint64)(dev & 0xffff0000U)) >> (int)(8))));
            (h.Devmajor, h.Devminor) = ((int64)major, (int64)minor);
        }
        else { /* default: */
            // TODO: Implement solaris (see https://golang.org/issue/8106)
        }
    }
    return default!;
}

} // end tar_package
