// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fstest implements support for testing implementations and users of file systems.
namespace go.testing;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using fs = go.io.fs_package;
using maps = maps_package;
using path = path_package;
using slices = slices_package;
using strings = strings_package;
using iotest = go.testing.iotest_package;
using go.io;
using go.testing;
using iter = iter_package;
using time = time_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class fstest_package {

// TestFS tests a file system implementation.
// It walks the entire tree of files in fsys,
// opening and checking that each file behaves correctly.
// It also checks that the file system contains at least the expected files.
// As a special case, if no expected files are listed, fsys must be empty.
// Otherwise, fsys must contain at least the listed files; it can also contain others.
// The contents of fsys must not change concurrently with TestFS.
//
// If TestFS finds any misbehaviors, it returns either the first error or a
// list of errors. Use [errors.Is] or [errors.As] to inspect.
//
// Typical usage inside a test is:
//
//	if err := fstest.TestFS(myFS, "file/that/should/be/present"); err != nil {
//		t.Fatal(err)
//	}
public static error TestFS(fs.FS fsys, params ꓸꓸꓸstring expectedʗp) {
    var expected = expectedʗp.sslice();

    {
        var err = testFS(fsys, expected.ꓸꓸꓸ); if (err != default!) {
            return err;
        }
    }
    foreach (var (_, name) in expected) {
        {
            nint i = strings.Index(name, "/"u8); if (i >= 0) {
                @string dir = name.slice(0, i);
                @string dirSlash = name.slice(0, i + 1);
                slice<@string> subExpected = default!;
                foreach (var (_, nameΔ1) in expected) {
                    if (strings.HasPrefix(nameΔ1, dirSlash)) {
                        subExpected = append(subExpected, nameΔ1.slice(len(dirSlash)));
                    }
                }
                var (sub, err) = fs.Sub(fsys, dir);
                if (err != default!) {
                    return err;
                }
                {
                    var errΔ1 = testFS(sub, subExpected.ꓸꓸꓸ); if (errΔ1 != default!) {
                        return fmt.Errorf("testing fs.Sub(fsys, %s): %w"u8, dir, errΔ1);
                    }
                }
                break; // one sub-test is enough
            }
        }
    }
    return default!;
}

internal static error testFS(fs.FS fsys, params ꓸꓸꓸstring expectedʗp) {
    var expected = expectedʗp.sslice();

    ref var t = ref heap<fsTester>(out var Ꮡt);
    t = new fsTester(fsys: fsys);
    Ꮡt.checkDir("."u8);
    Ꮡt.checkOpen("."u8);
    var found = new map<@string, bool>();
    foreach (var (_, dir) in t.dirs) {
        found[dir] = true;
    }
    foreach (var (_, @file) in t.files) {
        found[@file] = true;
    }
    delete(found, "."u8);
    if (len(expected) == 0 && len(found) > 0) {
        var list = slices.Sorted(maps.Keys<map<@string, bool>, @string, bool>(found));
        if (len(list) > 15) {
            list = append(list[..10], "..."u8);
        }
        t.errorf("expected empty file system but found files:\n%s"u8, strings.Join(list, "\n"u8));
    }
    foreach (var (_, name) in expected) {
        if (!found[name]) {
            t.errorf("expected but not found: %s"u8, name);
        }
    }
    if (len(t.errors) == 0) {
        return default!;
    }
    return fmt.Errorf("TestFS found errors:\n%w"u8, errors.Join(t.errors.ꓸꓸꓸ));
}

// An fsTester holds state for running the test.
[GoType] partial struct fsTester {
    internal fs.FS fsys;
    internal slice<error> errors;
    internal slice<@string> dirs;
    internal slice<@string> files;
}

// errorf adds an error to the list of errors.
[GoRecv] internal static void errorf(this ref fsTester t, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.errors = append(t.errors, fmt.Errorf(format, args.ꓸꓸꓸ));
}

[GoRecv] internal static fs.ReadDirFile openDir(this ref fsTester t, @string dir) {
    var (f, err) = t.fsys.Open(dir);
    if (err != default!) {
        t.errorf("%s: Open: %w"u8, dir, err);
        return default!;
    }
    var (d, ok) = f._<fs.ReadDirFile>(ᐧ);
    if (!ok) {
        f.Close();
        t.errorf("%s: Open returned File type %T, not a fs.ReadDirFile"u8, dir, f);
        return default!;
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string firstOpenReadDir1Vsˢ = "first Open+ReadDir(-1) vs second Open+ReadDir(-1)"u8;
internal static readonly @string firstOpenReadDir1VsThirdˢ = "first Open+ReadDir(-1) vs third Open+ReadDir(1,2) loop"u8;
internal static readonly @string firstOpenReadDir1VsFsysˢ = "first Open+ReadDir(-1) vs fsys.ReadDir"u8;
internal static readonly @string firstOpenReadDir1VsFsˢ = "first Open+ReadDir(-1) vs fs.ReadDir"u8;

// checkDir checks the directory dir, which is expected to exist
// (it is either the root or was found in a directory listing with IsDir true).
internal static void checkDir(this ж<fsTester> Ꮡt, @string dir) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Read entire directory.
        t.dirs = append(t.dirs, dir);
        var d = t.openDir(dir);
        if (d == default!) {
            return;
        }
        var (list, err) = d.ReadDir(-1);
        if (err != default!) {
            d.Close();
            t.errorf("%s: ReadDir(-1): %w"u8, dir, err);
            return;
        }
        // Check all children.
        @string prefix = default!;
        if (dir == "."u8){
            prefix = ""u8;
        } else {
            prefix = dir + "/"u8;
        }
        foreach (var (_, info) in list) {
            @string name = info.Name();
            switch (ᐧ) {
            case {} when (name == "."u8) || (name == ".."u8) || (name == ""u8): {
                t.errorf("%s: ReadDir: child has invalid name: %#q"u8, dir, name);
                continue;
                break;
            }
            case {} when strings.Contains(name, "/"u8): {
                t.errorf("%s: ReadDir: child name contains slash: %#q"u8, dir, name);
                continue;
                break;
            }
            case {} when strings.Contains(name, @"\"u8): {
                t.errorf("%s: ReadDir: child name contains backslash: %#q"u8, dir, name);
                continue;
                break;
            }}

            @string path = prefix + name;
            t.checkStat(path, info);
            Ꮡt.checkOpen(path);
            if (info.IsDir()){
                Ꮡt.checkDir(path);
            } else {
                Ꮡt.checkFile(path);
            }
        }
        // Check ReadDir(-1) at EOF.
        (var list2, err) = d.ReadDir(-1);
        if (len(list2) > 0 || err != default!) {
            d.Close();
            t.errorf("%s: ReadDir(-1) at EOF = %d entries, %w, wanted 0 entries, nil"u8, dir, len(list2), err);
            return;
        }
        // Check ReadDir(1) at EOF (different results).
        (list2, err) = d.ReadDir(1);
        if (len(list2) > 0 || !AreEqual(err, io.EOF)) {
            d.Close();
            t.errorf("%s: ReadDir(1) at EOF = %d entries, %w, wanted 0 entries, EOF"u8, dir, len(list2), err);
            return;
        }
        // Check that close does not report an error.
        {
            var errΔ1 = d.Close(); if (errΔ1 != default!) {
                t.errorf("%s: Close: %w"u8, dir, errΔ1);
            }
        }
        // Check that closing twice doesn't crash.
        // The return value doesn't matter.
        d.Close();
        // Reopen directory, read a second time, make sure contents match.
        {
            d = t.openDir(dir); if (d == default!) {
                return;
            }
        }
        var dʗ1 = d;
        defer(() => dʗ1.Close(), ref ᒐ);
        (list2, err) = d.ReadDir(-1);
        if (err != default!) {
            t.errorf("%s: second Open+ReadDir(-1): %w"u8, dir, err);
            return;
        }
        Ꮡt.checkDirList(dir, firstOpenReadDir1Vsˢ, list, list2);
        // Reopen directory, read a third time in pieces, make sure contents match.
        {
            d = t.openDir(dir); if (d == default!) {
                return;
            }
        }
        var dʗ2 = d;
        defer(() => dʗ2.Close(), ref ᒐ);
        list2 = default!;
        while (ᐧ) {
            nint n = 1;
            if (len(list2) > 0) {
                n = 2;
            }
            var (frag, errΔ2) = d.ReadDir(n);
            if (len(frag) > n) {
                t.errorf("%s: third Open: ReadDir(%d) after %d: %d entries (too many)"u8, dir, n, len(list2), len(frag));
                return;
            }
            list2 = appendꓸꓸꓸ(list2, frag);
            if (AreEqual(errΔ2, io.EOF)) {
                break;
            }
            if (errΔ2 != default!) {
                t.errorf("%s: third Open: ReadDir(%d) after %d: %w"u8, dir, n, len(list2), errΔ2);
                return;
            }
            if (n == 0) {
                t.errorf("%s: third Open: ReadDir(%d) after %d: 0 entries but nil error"u8, dir, n, len(list2));
                return;
            }
        }
        Ꮡt.checkDirList(dir, firstOpenReadDir1VsThirdˢ, list, list2);
        // If fsys has ReadDir, check that it matches and is sorted.
        {
            var (fsys, ok) = t.fsys._<fs.ReadDirFS>(ᐧ); if (ok) {
                var (list2Δ1, errΔ3) = fsys.ReadDir(dir);
                if (errΔ3 != default!) {
                    t.errorf("%s: fsys.ReadDir: %w"u8, dir, errΔ3);
                    return;
                }
                Ꮡt.checkDirList(dir, firstOpenReadDir1VsFsysˢ, list, list2Δ1);
                for (nint i = 0; i + 1 < len(list2Δ1); i++) {
                    if (list2Δ1[i].Name() >= list2Δ1[i + 1].Name()) {
                        t.errorf("%s: fsys.ReadDir: list not sorted: %s before %s"u8, dir, list2Δ1[i].Name(), list2Δ1[i + 1].Name());
                    }
                }
            }
        }
        // Check fs.ReadDir as well.
        (list2, err) = fs.ReadDir(t.fsys, dir);
        if (err != default!) {
            t.errorf("%s: fs.ReadDir: %w"u8, dir, err);
            return;
        }
        Ꮡt.checkDirList(dir, firstOpenReadDir1VsFsˢ, list, list2);
        for (nint i = 0; i + 1 < len(list2); i++) {
            if (list2[i].Name() >= list2[i + 1].Name()) {
                t.errorf("%s: fs.ReadDir: list not sorted: %s before %s"u8, dir, list2[i].Name(), list2[i + 1].Name());
            }
        }
        t.checkGlob(dir, list2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// formatEntry formats an fs.DirEntry into a string for error messages and comparison.
internal static @string formatEntry(fs.DirEntry entry) {
    return fmt.Sprintf("%s IsDir=%v Type=%v"u8, entry.Name(), entry.IsDir(), entry.Type());
}

// formatInfoEntry formats an fs.FileInfo into a string like the result of formatEntry, for error messages and comparison.
internal static @string formatInfoEntry(fs.FileInfo info) {
    return fmt.Sprintf("%s IsDir=%v Type=%v"u8, info.Name(), info.IsDir(), info.Mode().Type());
}

// formatInfo formats an fs.FileInfo into a string for error messages and comparison.
internal static @string formatInfo(fs.FileInfo info) {
    return fmt.Sprintf("%s IsDir=%v Mode=%v Size=%d ModTime=%v"u8, info.Name(), info.IsDir(), info.Mode(), info.Size(), info.ModTime());
}

// checkGlob checks that various glob patterns work if the file system implements GlobFS.
[GoRecv] internal static void checkGlob(this ref fsTester t, @string dir, slice<fs.DirEntry> list) {
    {
        var (_, ok) = t.fsys._<fs.GlobFS>(ᐧ); if (!ok) {
            return;
        }
    }
    // Make a complex glob pattern prefix that only matches dir.
    @string glob = default!;
    if (dir != "."u8) {
        var elem = strings.Split(dir, "/"u8);
        foreach (var (i, e) in elem) {
            slice<rune> pattern = default!;
            foreach (var (j, r) in e) {
                if (r == (rune)'*' || r == (rune)'?' || r == (rune)'\\' || r == (rune)'[' || r == (rune)'-') {
                    pattern = append(pattern, (rune)((rune)'\\'), r);
                    continue;
                }
                switch ((i + j) % 5) {
                case 0: {
                    pattern = append(pattern, r);
                    break;
                }
                case 1: {
                    pattern = append(pattern, (rune)((rune)'['), r, (rune)((rune)']'));
                    break;
                }
                case 2: {
                    pattern = append(pattern, (rune)((rune)'['), r, (rune)((rune)'-'), r, (rune)((rune)']'));
                    break;
                }
                case 3: {
                    pattern = append(pattern, (rune)((rune)'['), (rune)((rune)'\\'), r, (rune)((rune)']'));
                    break;
                }
                case 4: {
                    pattern = append(pattern, (rune)((rune)'['), (rune)((rune)'\\'), r, (rune)((rune)'-'), (rune)((rune)'\\'), r, (rune)((rune)']'));
                    break;
                }}

            }
            elem[i] = ((@string)pattern);
        }
        glob = strings.Join(elem, "/"u8) + "/"u8;
    }
    // Test that malformed patterns are detected.
    // The error is likely path.ErrBadPattern but need not be.
    {
        var (_, errΔ1) = t.fsys._<fs.GlobFS>().Glob(glob + "nonexist/[]"u8); if (errΔ1 == default!) {
            t.errorf("%s: Glob(%#q): bad pattern not detected"u8, dir, glob + "nonexist/[]");
        }
    }
    // Try to find a letter that appears in only some of the final names.
    var c = (rune)(rune)'a';
    for (; c <= (rune)'z'; c++) {
        var (have, haveNot) = (false, false);
        foreach (var (_, d) in list) {
            if (strings.ContainsRune(d.Name(), c)){
                have = true;
            } else {
                haveNot = true;
            }
        }
        if (have && haveNot) {
            break;
        }
    }
    if (c > (rune)'z') {
        c = (rune)'a';
    }
    glob += "*"u8 + ((@string)c) + "*"u8;
    slice<@string> want = default!;
    foreach (var (_, d) in list) {
        if (strings.ContainsRune(d.Name(), c)) {
            want = append(want, path.Join(dir, d.Name()));
        }
    }
    var (names, err) = t.fsys._<fs.GlobFS>().Glob(glob);
    if (err != default!) {
        t.errorf("%s: Glob(%#q): %w"u8, dir, glob, err);
        return;
    }
    if (slices.Equal<slice<@string>, @string>(want, names)) {
        return;
    }
    if (!slices.IsSorted<slice<@string>, @string>(names)) {
        t.errorf("%s: Glob(%#q): unsorted output:\n%s"u8, dir, glob, strings.Join(names, "\n"u8));
        slices.Sort<slice<@string>, @string>(names);
    }
    slice<@string> problems = default!;
    while (len(want) > 0 || len(names) > 0) {
        switch (ᐧ) {
        case {} when len(want) > 0 && len(names) > 0 && want[0] == names[0]: {
            (want, names) = (want[1..], names[1..]);
            break;
        }
        case {} when len(want) > 0 && (len(names) == 0 || want[0] < names[0]): {
            problems = append(problems, "missing: " + want[0]);
            want = want[1..];
            break;
        }
        default: {
            problems = append(problems, "extra: " + names[0]);
            names = names[1..];
            break;
        }}

    }
    t.errorf("%s: Glob(%#q): wrong output:\n%s"u8, dir, glob, strings.Join(problems, "\n"u8));
}

// checkStat checks that a direct stat of path matches entry,
// which was found in the parent's directory listing.
[GoRecv] internal static void checkStat(this ref fsTester t, @string path, fs.DirEntry entry) {
    var (@file, err) = t.fsys.Open(path);
    if (err != default!) {
        t.errorf("%s: Open: %w"u8, path, err);
        return;
    }
    (var info, err) = @file.Stat();
    @file.Close();
    if (err != default!) {
        t.errorf("%s: Stat: %w"u8, path, err);
        return;
    }
    @string fentry = formatEntry(entry);
    @string fientry = formatInfoEntry(info);
    // Note: mismatch here is OK for symlink, because Open dereferences symlink.
    if (fentry != fientry && (fs.FileMode)(entry.Type() & fs.ModeSymlink) == 0) {
        t.errorf("%s: mismatch:\n\tentry = %s\n\tfile.Stat() = %s"u8, path, fentry, fientry);
    }
    (var einfo, err) = entry.Info();
    if (err != default!) {
        t.errorf("%s: entry.Info: %w"u8, path, err);
        return;
    }
    @string finfo = formatInfo(info);
    if ((fs.FileMode)(entry.Type() & fs.ModeSymlink) != 0){
        // For symlink, just check that entry.Info matches entry on common fields.
        // Open deferences symlink, so info itself may differ.
        @string feentry = formatInfoEntry(einfo);
        if (fentry != feentry) {
            t.errorf("%s: mismatch\n\tentry = %s\n\tentry.Info() = %s\n"u8, path, fentry, feentry);
        }
    } else {
        @string feinfo = formatInfo(einfo);
        if (feinfo != finfo) {
            t.errorf("%s: mismatch:\n\tentry.Info() = %s\n\tfile.Stat() = %s\n"u8, path, feinfo, finfo);
        }
    }
    // Stat should be the same as Open+Stat, even for symlinks.
    (var info2, err) = fs.Stat(t.fsys, path);
    if (err != default!) {
        t.errorf("%s: fs.Stat: %w"u8, path, err);
        return;
    }
    @string finfo2 = formatInfo(info2);
    if (finfo2 != finfo) {
        t.errorf("%s: fs.Stat(...) = %s\n\twant %s"u8, path, finfo2, finfo);
    }
    {
        var (fsys, ok) = t.fsys._<fs.StatFS>(ᐧ); if (ok) {
            var (info2Δ1, errΔ1) = fsys.Stat(path);
            if (errΔ1 != default!) {
                t.errorf("%s: fsys.Stat: %w"u8, path, errΔ1);
                return;
            }
            @string finfo2Δ1 = formatInfo(info2Δ1);
            if (finfo2Δ1 != finfo) {
                t.errorf("%s: fsys.Stat(...) = %s\n\twant %s"u8, path, finfo2Δ1, finfo);
            }
        }
    }
}

// checkDirList checks that two directory lists contain the same files and file info.
// The order of the lists need not match.
internal static void checkDirList(this ж<fsTester> Ꮡt, @string dir, @string desc, slice<fs.DirEntry> list1, slice<fs.DirEntry> list2) {
    ref var t = ref Ꮡt.DerefOrNull();

    var old = new map<@string, fs.DirEntry>();
    void checkMode(fs.DirEntry entry) {
        if (entry.IsDir() != ((fs.FileMode)(entry.Type() & fs.ModeDir) != 0)) {
            if (entry.IsDir()){
                Ꮡt.Value.errorf("%s: ReadDir returned %s with IsDir() = true, Type() & ModeDir = 0"u8, dir, entry.Name());
            } else {
                Ꮡt.Value.errorf("%s: ReadDir returned %s with IsDir() = false, Type() & ModeDir = ModeDir"u8, dir, entry.Name());
            }
        }
    }
    foreach (var (_, entry1) in list1) {
        old[entry1.Name()] = entry1;
        checkMode(entry1);
    }
    slice<@string> diffs = default!;
    foreach (var (_, entry2) in list2) {
        var entry1 = old[entry2.Name()];
        if (entry1 == default!) {
            checkMode(entry2);
            diffs = append(diffs, "+ "u8 + formatEntry(entry2));
            continue;
        }
        if (formatEntry(entry1) != formatEntry(entry2)) {
            diffs = append(diffs, "- "u8 + formatEntry(entry1), "+ " + formatEntry(entry2));
        }
        delete(old, entry2.Name());
    }
    foreach (var (_, entry1) in old) {
        diffs = append(diffs, "- "u8 + formatEntry(entry1));
    }
    if (len(diffs) == 0) {
        return;
    }
    slices.SortFunc(diffs, (@string a, @string b) => {
        var fa = strings.Fields(a);
        var fb = strings.Fields(b);
        // sort by name (i < j) and then +/- (j < i, because + < -)
        return strings.Compare(fa[1] + " " + fb[0], fb[1] + " " + fa[0]);
    });
    t.errorf("%s: diff %s:\n\t%s"u8, dir, desc, strings.Join(diffs, "\n\t"u8));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readAllVsFsysReadFileˢ = "ReadAll vs fsys.ReadFile"u8;
internal static readonly @string readallVsSecondFsysˢ = "Readall vs second fsys.ReadFile"u8;
internal static readonly @string readFileˢ = "ReadFile"u8;
internal static readonly @string readAllVsFsReadFileˢ = "ReadAll vs fs.ReadFile"u8;

// checkFile checks that basic file reading works correctly.
internal static void checkFile(this ж<fsTester> Ꮡt, @string @file) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.files = append(t.files, @file);
        // Read entire file.
        var (f, err) = t.fsys.Open(@file);
        if (err != default!) {
            t.errorf("%s: Open: %w"u8, @file, err);
            return;
        }
        (var data, err) = io.ReadAll(new fs_FileᴠReader(f));
        if (err != default!) {
            f.Close();
            t.errorf("%s: Open+ReadAll: %w"u8, @file, err);
            return;
        }
        {
            var errΔ1 = f.Close(); if (errΔ1 != default!) {
                t.errorf("%s: Close: %w"u8, @file, errΔ1);
            }
        }
        // Check that closing twice doesn't crash.
        // The return value doesn't matter.
        f.Close();
        // Check that ReadFile works if present.
        {
            var (fsys, ok) = t.fsys._<fs.ReadFileFS>(ᐧ); if (ok) {
                var (data2Δ1, errΔ2) = fsys.ReadFile(@file);
                if (errΔ2 != default!) {
                    t.errorf("%s: fsys.ReadFile: %w"u8, @file, errΔ2);
                    return;
                }
                t.checkFileRead(@file, readAllVsFsysReadFileˢ, data, data2Δ1);
                // Modify the data and check it again. Modifying the
                // returned byte slice should not affect the next call.
                foreach (var (i, _) in data2Δ1) {
                    data2Δ1[i]++;
                }
                (data2Δ1, errΔ2) = fsys.ReadFile(@file);
                if (errΔ2 != default!) {
                    t.errorf("%s: second call to fsys.ReadFile: %w"u8, @file, errΔ2);
                    return;
                }
                t.checkFileRead(@file, readallVsSecondFsysˢ, data, data2Δ1);
                    var fsysʗ1 = fsys;
                t.checkBadPath(@file, readFileˢ,
                    (@string name) => {
                        var (_, errΔ3) = fsysʗ1.ReadFile(name);
                        return errΔ3;
                    });
            }
        }
        // Check that fs.ReadFile works with t.fsys.
        (var data2, err) = fs.ReadFile(t.fsys, @file);
        if (err != default!) {
            t.errorf("%s: fs.ReadFile: %w"u8, @file, err);
            return;
        }
        t.checkFileRead(@file, readAllVsFsReadFileˢ, data, data2);
        // Use iotest.TestReader to check small reads, Seek, ReadAt.
        (f, err) = t.fsys.Open(@file);
        if (err != default!) {
            t.errorf("%s: second Open: %w"u8, @file, err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var errΔ4 = iotest.TestReader(new fs_FileᴠReader(f), data); if (errΔ4 != default!) {
                t.errorf("%s: failed TestReader:\n\t%s"u8, @file, strings.ReplaceAll(errΔ4.Error(), "\n"u8, "\n\t"u8));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoRecv] internal static void checkFileRead(this ref fsTester t, @string @file, @string desc, slice<byte> data1, slice<byte> data2) {
    if (((sstring)data1) != ((sstring)data2)) {
        t.errorf("%s: %s: different data returned\n\t%q\n\t%q"u8, @file, desc, data1, data2);
        return;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openˢ = "Open"u8;

// checkOpen validates file opening behavior by attempting to open and then close the given file path.
internal static void checkOpen(this ж<fsTester> Ꮡt, @string @file) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.checkBadPath(@file, openˢ, (@string fileΔ1) => {
        var (f, err) = Ꮡt.Value.fsys.Open(fileΔ1);
        if (err == default!) {
            f.Close();
        }
        return err;
    });
}

// checkBadPath checks that various invalid forms of file's name cannot be opened using open.
[GoRecv] internal static void checkBadPath(this ref fsTester t, @string @file, @string desc, Func<@string, error> open) {
    var bad = new @string[]{
        "/"u8 + @file,
        @file + "/."u8
    }.slice();
    if (@file == "."u8) {
        bad = append(bad, "/"u8);
    }
    {
        nint i = strings.Index(@file, "/"u8); if (i >= 0) {
            bad = append(bad,
                @file.slice(0, i) + "//" + @file.slice(i + 1),
                @file.slice(0, i) + "/./" + @file.slice(i + 1),
                @file.slice(0, i) + @"\" + @file.slice(i + 1),
                @file.slice(0, i) + "/../" + @file);
        }
    }
    {
        nint i = strings.LastIndex(@file, "/"u8); if (i >= 0) {
            bad = append(bad,
                @file.slice(0, i) + "//" + @file.slice(i + 1),
                @file.slice(0, i) + "/./" + @file.slice(i + 1),
                @file.slice(0, i) + @"\" + @file.slice(i + 1),
                @file + "/../" + @file.slice(i + 1));
        }
    }
    foreach (var (_, b) in bad) {
        {
            var err = open(b); if (err == default!) {
                t.errorf("%s: %s(%s) succeeded, want error"u8, @file, desc, b);
            }
        }
    }
}

} // end fstest_package
