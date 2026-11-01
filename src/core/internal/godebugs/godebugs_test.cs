// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using godebugs = go.@internal.godebugs_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using fs = io.fs_package;
using go.@internal;
using go.os;
using io;
using path;

partial class godebugs_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string docGodebugMdˢ => ref docGodebugMdˢᵇ;
private static readonly @string docGodebugMdˢᵇ = "../../../doc/godebug.md"u8;

public static void TestAll(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    var (data, err) = os.ReadFile(docGodebugMdˢ);
    if (err != nil) {
        if (os.IsNotExist(err) && (testenv.Builder() == ""u8 || Δruntime.GOOS != "linux"u8)) {
            t.Skip(err);
        }
        t.Fatal(err);
    }
    @string doc = (@string)data;

    var incs = incNonDefaults(t);

    @string last = ""u8;
    foreach (var (_, info) in godebugs.All) {
        if (info.Name <= last) {
            t.Errorf("All not sorted: %s then %s", last, info.Name);
        }
        last = info.Name;

        if (info.Package == ""u8) {
            t.Errorf("Name=%s missing Package", info.Name);
        }
        if (info.Changed != 0 && info.Old == ""u8) {
            t.Errorf("Name=%s has Changed, missing Old", info.Name);
        }
        if (info.Old != ""u8 && info.Changed == 0) {
            t.Errorf("Name=%s has Old, missing Changed", info.Name);
        }
        if (!strings.Contains(doc, "`"u8 + info.Name + "`"u8) && !strings.Contains(doc, "`"u8 + info.Name + "="u8)) {
            t.Errorf("Name=%s not documented in doc/godebug.md", info.Name);
        }
        if (!info.Opaque && !incs[info.Name]) {
            t.Errorf("Name=%s missing IncNonDefault calls; see 'go doc internal/godebug'", info.Name);
        }
    }
}

internal static ж<regexp.Regexp> incNonDefaultRE = regexp.MustCompile(@"([\pL\p{Nd}_]+)\.IncNonDefault\(\)");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
private static ref readonly @string fDirˢ => ref fDirˢᵇ;
private static readonly @string fDirˢᵇ = "-f={{.Dir}}"u8;
private static ref readonly @string stdˢ => ref stdˢᵇ;
private static readonly @string stdˢᵇ = "std"u8;
private static ref readonly @string cmdˢ => ref cmdˢᵇ;
private static readonly @string cmdˢᵇ = "cmd"u8;
private static ref readonly @string testGoˢ => ref testGoˢᵇ;
private static readonly @string testGoˢᵇ = "_test.go"u8;

internal static map<@string, bool> incNonDefaults(ж<testing.T> t) {
    // Build list of all files importing internal/godebug.
    // Tried a more sophisticated search in go list looking for
    // imports containing "internal/godebug", but that turned
    // up a bug in go list instead. #66218
    var (@out, err) = exec.Command("go", listˢ, fDirˢ, stdˢ, cmdˢ).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go list: %v\n%s", err, @out);
    }

    var seen = new map<@string, bool>{};
    foreach (var (_, dir) in strings.Split((@string)@out, "\n")) {
        if (dir == ""u8) {
            continue;
        }
        var (files, errΔ1) = os.ReadDir(dir);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        foreach (var (_, @file) in files) {
            @string name = @file.Name();
            if (!strings.HasSuffix(name, ".go") || strings.HasSuffix(name, testGoˢ)) {
                continue;
            }
            var (data, errΔ2) = os.ReadFile(filepath.Join(dir, name));
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            foreach (var (_, m) in incNonDefaultRE.FindAllSubmatch(data, -1)) {
                seen[((@string)m[1])] = true;
            }
        }
    }
    return seen;
}

public static void TestRemoved(ж<testing.T> t) {
    foreach (var (_, info) in godebugs.Removed) {
        if (godebugs.Lookup(info.Name) != nil) {
            t.Fatalf("GODEBUG: %v exists in both Removed and All", info.Name);
        }
    }
}

} // end godebugs_test_package
