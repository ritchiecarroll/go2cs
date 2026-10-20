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
private static readonly @string docGodebugMdˢ = "../../../doc/godebug.md"u8;

public static void TestAll(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));

    var (data, err) = os.ReadFile(docGodebugMdˢ);
    if (err != default!) {
        if (os.IsNotExist(err) && (testenv.Builder() == ""u8 || Δruntime.GOOS != "linux"u8)) {
            Ꮡt.Skip(err);
        }
        Ꮡt.Fatal(err);
    }
    @string doc = (@string)data;

    var incs = incNonDefaults(Ꮡt);

    @string last = ""u8;
    foreach (var (_, info) in godebugs.All) {
        if (info.Name <= last) {
            Ꮡt.Errorf("All not sorted: %s then %s"u8, last, info.Name);
        }
        last = info.Name;

        if (info.Package == ""u8) {
            Ꮡt.Errorf("Name=%s missing Package"u8, info.Name);
        }
        if (info.Changed != 0 && info.Old == ""u8) {
            Ꮡt.Errorf("Name=%s has Changed, missing Old"u8, info.Name);
        }
        if (info.Old != ""u8 && info.Changed == 0) {
            Ꮡt.Errorf("Name=%s has Old, missing Changed"u8, info.Name);
        }
        if (!strings.Contains(doc, "`"u8 + info.Name + "`"u8) && !strings.Contains(doc, "`"u8 + info.Name + "="u8)) {
            Ꮡt.Errorf("Name=%s not documented in doc/godebug.md"u8, info.Name);
        }
        if (!info.Opaque && !incs[info.Name]) {
            Ꮡt.Errorf("Name=%s missing IncNonDefault calls; see 'go doc internal/godebug'"u8, info.Name);
        }
    }
}

internal static ж<regexp.Regexp> incNonDefaultRE = regexp.MustCompile(@"([\pL\p{Nd}_]+)\.IncNonDefault\(\)"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string listˢ = "list"u8;
private static readonly @string fDirˢ = "-f={{.Dir}}"u8;
private static readonly @string stdˢ = "std"u8;
private static readonly @string cmdˢ = "cmd"u8;
private static readonly @string testGoˢ = "_test.go"u8;

internal static map<@string, bool> incNonDefaults(ж<testing.T> Ꮡt) {
    // Build list of all files importing internal/godebug.
    // Tried a more sophisticated search in go list looking for
    // imports containing "internal/godebug", but that turned
    // up a bug in go list instead. #66218
    var (@out, err) = exec.Command("go"u8, listˢ, fDirˢ, stdˢ, cmdˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go list: %v\n%s"u8, err, @out);
    }

    var seen = new map<@string, bool>{};
    foreach (var (_, dir) in strings.Split((@string)@out, "\n"u8)) {
        if (dir == ""u8) {
            continue;
        }
        var (files, errΔ1) = os.ReadDir(dir);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        foreach (var (_, @file) in files) {
            @string name = @file.Name();
            if (!strings.HasSuffix(name, ".go"u8) || strings.HasSuffix(name, testGoˢ)) {
                continue;
            }
            var (data, errΔ2) = os.ReadFile(filepath.Join(dir, name));
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            foreach (var (_, m) in incNonDefaultRE.FindAllSubmatch(data, -1)) {
                seen[((@string)m[1])] = true;
            }
        }
    }
    return seen;
}

} // end godebugs_test_package
