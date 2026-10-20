// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using bytealg = go.@internal.bytealg_package;
using strconv = go.@internal.strconv_package;
using go.@internal;

partial class cgroup_package {

public static error ErrNoCgroup = ((stringError)(@string)"not in a cgroup"u8);

internal static error errMalformedFile = ((stringError)(@string)"malformed file"u8);

internal static UntypedInt _PATH_MAX => 4096;

// Required amount of scratch space for CPULimit.
//
// TODO(prattmic): This is shockingly large (~70KiB) due to the (very
// unlikely) combination of extremely long paths consisting mostly
// escaped characters. The scratch buffer ends up in .bss in package
// runtime, so it doesn't contribute to binary size and generally won't
// be faulted in, but it would still be nice to shrink this. A more
// complex parser that did not need to keep entire lines in memory
// could get away with much less. Alternatively, we could do a one-off
// mmap allocation for this buffer, which is only mapped larger if we
// actually need the extra space.
public static UntypedInt ScratchSize => /* PathSize + ParseSize */ 69632;

// Required space to store a path of the cgroup in the filesystem.
public static UntypedInt PathSize => /* _PATH_MAX */ 4096;

// /proc/self/mountinfo path escape sequences are 4 characters long, so
// a path consisting entirely of escaped characters could be 4 times
// larger.
internal static UntypedInt escapedPathMax => /* 4 * _PATH_MAX */ 16384;

// Required space to parse /proc/self/mountinfo and /proc/self/cgroup.
// See findCPUMount and findCPURelativePath.
public static UntypedInt ParseSize => /* 4 * escapedPathMax */ 65536;

// Version indicates the cgroup version.
partial struct Version /*num:nint*/;

public static Version VersionUnknown => /* iota */ 0;
public static Version V1 => 1;
public static Version V2 => 2;

internal static (int64, error) parseV1Number(slice<byte> buf) {
    // Ignore trailing newline.
    nint i = bytealg.IndexByte(buf, (rune)'\n');
    if (i < 0) {
        return (0, errMalformedFile);
    }
    buf = buf.slice(0, i);

    var (val, err) = strconv.ParseInt((@string)buf, 10, 64);
    if (err != default!) {
        return (0, errMalformedFile);
    }

    return (val, default!);
}

internal static (float64, bool, error) parseV2Limit(slice<byte> buf) {
    nint i = bytealg.IndexByte(buf, (rune)' ');
    if (i < 0) {
        return (0D, false, errMalformedFile);
    }

    var quotaStr = buf.slice(0, i);
    if (bytealg.Compare(quotaStr, slice<byte>("max"u8)) == 0) {
        // No limit.
        return (0D, false, default!);
    }

    var periodStr = buf.slice(i + 1);
    // Ignore trailing newline, if any.
    i = bytealg.IndexByte(periodStr, (rune)'\n');
    if (i < 0) {
        return (0D, false, errMalformedFile);
    }
    periodStr = periodStr.slice(0, i);

    var (quota, err) = strconv.ParseInt((@string)quotaStr, 10, 64);
    if (err != default!) {
        return (0D, false, errMalformedFile);
    }

    (var period, err) = strconv.ParseInt((@string)periodStr, 10, 64);
    if (err != default!) {
        return (0D, false, errMalformedFile);
    }

    return ((float64)quota / (float64)period, true, default!);
}

// Finds the path of the current process's CPU cgroup and writes it to out.
//
// fd is a file descriptor for /proc/self/cgroup.
// Returns the number of bytes written and the cgroup version (1 or 2).
internal static (nint, Version, error) parseCPUCgroup(nint fd, Func<nint, slice<byte>, (nint, uintptr)> read, slice<byte> @out, slice<byte> scratch) {
    // The format of each line is
    //
    //   hierarchy-ID:controller-list:cgroup-path
    //
    // controller-list is comma-separated.
    //
    // cgroup v2 has hierarchy-ID 0. If a v1 hierarchy contains "cpu", that
    // is the CPU controller. Otherwise the v2 hierarchy (if any) is the
    // CPU controller. It is not possible to mount the same controller
    // simultaneously under both the v1 and the v2 hierarchies.
    //
    // See man 7 cgroups for more details.
    //
    // hierarchy-ID and controller-list have relatively small maximum
    // sizes, and the path can be up to _PATH_MAX, so we need a bit more
    // than 1 _PATH_MAX of scratch space.
    var l = newLineReader(fd, scratch, read);

    // Bytes written to out.
    nint n = 0;

    while (ᐧ) {
        var err = l.next();
        if (AreEqual(err, errIncompleteLine)){
            // Don't allow incomplete lines. While in theory the
            // incomplete line may be for a controller we don't
            // care about, in practice all lines should be of
            // similar length, so we should just have a buffer big
            // enough for any.
            return (0, 0, err);
        } else 
        if (AreEqual(err, errEOF)){
            break;
        } else 
        if (err != default!) {
            return (0, 0, err);
        }

        var line = l.line();

        // The format of each line is
        //
        //   hierarchy-ID:controller-list:cgroup-path
        //
        // controller-list is comma-separated.
        // See man 7 cgroups for more details.
        nint i = bytealg.IndexByte(line, (rune)':');
        if (i < 0) {
            return (0, 0, errMalformedFile);
        }

        var hierarchy = line.slice(0, i);
        line = line.slice(i + 1);

        i = bytealg.IndexByte(line, (rune)':');
        if (i < 0) {
            return (0, 0, errMalformedFile);
        }

        var controllers = line.slice(0, i);
        line = line.slice(i + 1);

        var path = line;
        if (len(path) == 0 || path[0] != (rune)'/') {
            // We rely on this when composing the full path.
            return (0, 0, errMalformedFile);
        }
        if (len(path) > len(@out)) {
            // Should not be possible. If we really get a very long cgroup path,
            // read /proc/self/cgroup will fail with ENAMETOOLONG.
            return (0, 0, errPathTooLong);
        }

        if (((sstring)hierarchy) == "0"u8){
            // v2 hierarchy.
            n = copy(@out, path);
            // Keep searching, we might find a v1 hierarchy with a
            // CPU controller, which takes precedence.
        } else {
            // v1 hierarchy
            if (containsCPU(controllers)) {
                // Found a v1 CPU controller. This must be the
                // only one, so we're done.
                return (copy(@out, path), V1, default!);
            }
        }
    }

    if (n == 0) {
        // Found nothing.
        return (0, 0, ErrNoCgroup);
    }

    // Must be v2, v1 returns above.
    return (n, V2, default!);
}

// Returns true if comma-separated list b contains "cpu".
internal static bool containsCPU(slice<byte> b) {
    while (len(b) > 0) {
        nint i = bytealg.IndexByte(b, (rune)',');
        if (i < 0) {
            // Neither cmd/compile nor gccgo allocates for these string conversions.
            return ((sstring)b) == "cpu"u8;
        }

        var curr = b.slice(0, i);
        var rest = b.slice(i + 1);

        if (((sstring)curr) == "cpu"u8) {
            return true;
        }

        b = rest;
    }

    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string impossibleCgroupVersionˢ = "impossible cgroup version"u8;
internal static readonly @string wrongUnescapedLenˢ = "wrong unescaped len"u8;

// Returns the path to the specified cgroup and version with cpu controller
//
// fd is a file descriptor for /proc/self/mountinfo.
// Returns the number of bytes written.
internal static (nint, error) parseCPUMount(nint fd, Func<nint, slice<byte>, (nint, uintptr)> read, slice<byte> @out, slice<byte> cgroup, Version version, slice<byte> scratch) {
    // The format of each line is:
    //
    // 36 35 98:0 /mnt1 /mnt2 rw,noatime master:1 - ext3 /dev/root rw,errors=continue
    // (1)(2)(3)   (4)   (5)      (6)      (7)   (8) (9)   (10)         (11)
    //
    // (1) mount ID:  unique identifier of the mount (may be reused after umount)
    // (2) parent ID:  ID of parent (or of self for the top of the mount tree)
    // (3) major:minor:  value of st_dev for files on filesystem
    // (4) root:  root of the mount within the filesystem
    // (5) mount point:  mount point relative to the process's root
    // (6) mount options:  per mount options
    // (7) optional fields:  zero or more fields of the form "tag[:value]"
    // (8) separator:  marks the end of the optional fields
    // (9) filesystem type:  name of filesystem of the form "type[.subtype]"
    // (10) mount source:  filesystem specific information or "none"
    // (11) super options:  per super block options
    //
    // See man 5 proc_pid_mountinfo for more details.
    //
    // Note that emitted paths will not contain space, tab, newline, or
    // carriage return. Those are escaped. See Linux show_mountinfo ->
    // show_path. We must unescape before returning.
    //
    // A mount point matches if the filesystem type (9) is cgroup2,
    // or cgroup with "cpu" in the super options (11),
    // and the cgroup is in the root (4). If there are multiple matches,
    // the first one is selected.
    //
    // We return full cgroup path, which is the mount point (5) +
    // cgroup parameter without the root (4) prefix.
    //
    // (4), (5), and (10) are up to _PATH_MAX. The remaining fields have a
    // small fixed maximum size, so 4*_PATH_MAX is plenty of scratch space.
    // Note that non-cgroup mounts may have arbitrarily long (11), but we
    // can skip those when parsing.
    var l = newLineReader(fd, scratch, read);

    while (ᐧ) {
        var err = l.next();
        if (AreEqual(err, errIncompleteLine)){
            // An incomplete line is fine as long as it doesn't
            // impede parsing the fields we need. It shouldn't be
            // possible for any mount to use more than 3*PATH_MAX
            // before (9) because there are two paths and all other
            // earlier fields have bounded options. Only (11) has
            // unbounded options.
        } else 
        if (AreEqual(err, errEOF)){
            break;
        } else 
        if (err != default!) {
            return (0, err);
        }

        var line = l.line();

        // Skip first three fields.
        foreach (var _ᴛ1 in range(3)) {
            nint iΔ1 = bytealg.IndexByte(line, (rune)' ');
            if (iΔ1 < 0) {
                return (0, errMalformedFile);
            }
            line = line.slice(iΔ1 + 1);
        }

        // (4) root:  root of the mount within the filesystem
        nint i = bytealg.IndexByte(line, (rune)' ');
        if (i < 0) {
            return (0, errMalformedFile);
        }
        var root = line.slice(0, i);
        if (len(root) == 0 || root[0] != (rune)'/') {
            // We rely on this in hasPathPrefix.
            return (0, errMalformedFile);
        }
        line = line.slice(i + 1);

        // (5) mount point:  mount point relative to the process's root
        i = bytealg.IndexByte(line, (rune)' ');
        if (i < 0) {
            return (0, errMalformedFile);
        }
        var mnt = line.slice(0, i);
        line = line.slice(i + 1);

        // Skip ahead past optional fields, delimited by " - ".
        while (ᐧ) {
            i = bytealg.IndexByte(line, (rune)' ');
            if (i < 0) {
                return (0, errMalformedFile);
            }
            if (i + 3 >= len(line)) {
                return (0, errMalformedFile);
            }
            var delim = line.slice(i, i + 3);
            if (((sstring)delim) == " - "u8) {
                line = line.slice(i + 3);
                break;
            }
            line = line.slice(i + 1);
        }

        // (9) filesystem type:  name of filesystem of the form "type[.subtype]"
        i = bytealg.IndexByte(line, (rune)' ');
        if (i < 0) {
            return (0, errMalformedFile);
        }
        var ftype = line.slice(0, i);
        line = line.slice(i + 1);

        var exprᴛ1 = version;
        if (exprᴛ1 == V1) {
            if (((sstring)ftype) != "cgroup"u8) {
                continue;
            }
            // (10) mount source:  filesystem specific information or "none"
            i = bytealg.IndexByte(line, (rune)' ');
            if (i < 0) {
                return (0, errMalformedFile);
            }
            // Don't care about mount source.
            line = line.slice(i + 1);

            // (11) super options:  per super block options
            if (!containsCPU(line)) {
                continue;
            }
        }
        else if (exprᴛ1 == V2) {
            if (((sstring)ftype) != "cgroup2"u8) {
                continue;
            }
        }
        else { /* default: */
            @throw(impossibleCgroupVersionˢ);
            throw panic("unreachable");
        }

        // Check cgroup is in the root.
        // If the cgroup is /sandbox/container, the matching mount point root could be
        // /sandbox/container, /sandbox, or /
        (var rootLen, err) = unescapePath(root, root);
        if (err != default!) {
            return (0, err);
        }
        root = root.slice(0, rootLen);
        if (!hasPathPrefix(cgroup, root)) {
            continue; // not matched, this is not the mount point we're looking for
        }

        // Cutoff the root from cgroup, ensure rel starts with '/' or is empty.
        var rel = cgroup.slice(rootLen);
        if (rootLen == 1 && len(cgroup) > 1) {
            // root is "/", but cgroup is not. Keep full cgroup path.
            rel = cgroup;
        }
        if (hasPathPrefix(rel, slice<byte>("/.."u8))) {
            // the cgroup is out of current cgroup namespace, and this mount point
            // cannot reach that cgroup.
            //
            // e.g. If the process is in cgroup /init, but in a cgroup namespace
            // rooted at /sandbox/container, /proc/self/cgroup will show /../../init.
            // we can reach it if the mount point root is
            // /../.. or /../../init, but not if it is /.. or /
            // While mount point with root /../../.. should able to reach the cgroup,
            // we don't know the path to the cgroup within that mount point.
            continue;
        }

        // All conditions met, compose the full path.
        // Copy rel to the correct place first, it may overlap with out.
        nint n = unescapedLen(mnt);
        if (n + len(rel) > len(@out)) {
            return (0, errPathTooLong);
        }
        copy(@out.slice(n), rel);
        (var n2, err) = unescapePath(@out.slice(0, n), mnt);
        if (err != default!) {
            return (0, err);
        }
        if (n2 != n) {
            @throw(wrongUnescapedLenˢ);
        }
        return (n + len(rel), default!);
    }

    // Found nothing.
    return (0, ErrNoCgroup);
}

internal static bool hasPathPrefix(slice<byte> p, slice<byte> prefix) {
    nint i = len(prefix);
    if (i == 1) {
        return true; // root contains everything
    }
    if (len(p) < i || !bytealg.Equal(prefix, p.slice(0, i))) {
        return false;
    }
    return len(p) == i || p[i] == (rune)'/'; // must match at path boundary
}

internal static error errInvalidEscape = ((stringError)(@string)"invalid path escape sequence"u8);
internal static error errPathTooLong = ((stringError)(@string)"path too long"u8);

internal static nint unescapedLen(slice<byte> @in) {
    return len(@in) - bytealg.Count(@in, (byte)(rune)'\\') * 3;
}

// unescapePath copies in to out, unescaping escape sequences generated by
// Linux's show_path.
//
// That is, '\', ' ', '\t', and '\n' are converted to octal escape sequences,
// like '\040' for space.
//
// Caller must ensure that out at least has unescapedLen(in) bytes.
// in and out may alias; in-place unescaping is supported.
//
// Returns the number of bytes written to out.
//
// Also see escapePath in cgroup_linux_test.go.
internal static (nint, error) unescapePath(slice<byte> @out, slice<byte> @in) {
    nint outi = default!;
    nint ini = default!;
    while (ini < len(@in)) {
        if (outi >= len(@out)) {
            // given that caller already ensured out is long enough, this
            // is only possible if there are malformed escape sequences
            // we have not parsed yet.
            return (outi, errInvalidEscape);
        }
        var c = @in[ini];
        if (c != (rune)'\\') {
            @out[outi] = c;
            outi++;
            ini++;
            continue;
        }

        // Start of escape sequence.

        // Escape sequence is always 4 characters: one slash and three
        // digits.
        if (ini + 3 >= len(@in)) {
            return (outi, errInvalidEscape);
        }

        nint outc = default!;
        foreach (var i in range(3)) {
            var cΔ1 = @in[ini + 1 + i];
            if (cΔ1 < (rune)'0' || cΔ1 > (rune)'7') {
                return (outi, errInvalidEscape);
            }

            outc *= 8;
            outc += (nint)((byte)(cΔ1 - (rune)'0'));
        }

        if (outc > 0xFF) {
            return (outi, errInvalidEscape);
        }
        @out[outi] = (byte)outc;
        outi++;

        ini += 4;
    }

    return (outi, default!);
}

} // end cgroup_package
