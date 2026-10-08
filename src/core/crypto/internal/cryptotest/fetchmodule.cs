// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using json = encoding.json_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using testing = testing_package;
using encoding;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.os;

partial class cryptotest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string envˢ = "env"u8;
private static readonly @string gomodcacheˢ = "GOMODCACHE"u8;
private static readonly @string goflagsˢ = "GOFLAGS"u8;
private static readonly @string modˢ = "mod"u8;
private static readonly @string downloadˢ = "download"u8;
private static readonly @string jsonˢ = "-json"u8;

internal partial struct FetchModule_j /*dyn*/ {
    public @string Dir;
}

// FetchModule fetches the module at the given version and returns the directory
// containing its source tree. It skips the test if fetching modules is not
// possible in this environment.
public static @string FetchModule(ж<testing.T> Ꮡt, @string module, @string version) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExternalNetwork(new testing_TжTB(Ꮡt));
    @string goTool = testenv.GoToolPath(new testing_TжTB(Ꮡt));
    // If the default GOMODCACHE doesn't exist, use a temporary directory
    // instead. (For example, run.bash sets GOPATH=/nonexist-gopath.)
    var (@out, err) = testenv.Command(new testing_TжTB(Ꮡt), goTool, envˢ, gomodcacheˢ).Output();
    if (err != default!) {
        Ꮡt.Fatalf("%s env GOMODCACHE: %v\n%s"u8, goTool, err, @out);
    }
    var modcacheOk = false;
    {
        @string gomodcache = ((@string)bytes.TrimSpace(@out)); if (gomodcache != ""u8) {
            {
                var (_, errΔ1) = os.Stat(gomodcache); if (errΔ1 == default!) {
                    modcacheOk = true;
                }
            }
        }
    }
    if (!modcacheOk) {
        Ꮡt.Setenv(gomodcacheˢ, Ꮡt.TempDir());
        // Allow t.TempDir() to clean up subdirectories.
        Ꮡt.Setenv(goflagsˢ, os.Getenv(goflagsˢ) + " -modcacherw"u8);
    }
    Ꮡt.Logf("fetching %s@%s\n"u8, module, version);
    (var output, err) = testenv.Command(new testing_TжTB(Ꮡt), goTool, modˢ, downloadˢ, jsonˢ, module + "@" + version).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to download %s@%s: %s\n%s\n"u8, module, version, err, output);
    }
    ref var j = ref heap(new FetchModule_j(), out var Ꮡj);
    {
        var errΔ2 = json.Unmarshal(output, Ꮡj); if (errΔ2 != default!) {
            Ꮡt.Fatalf("failed to parse 'go mod download': %s\n%s\n"u8, errΔ2, output);
        }
    }
    return j.Dir;
}

} // end cryptotest_package
