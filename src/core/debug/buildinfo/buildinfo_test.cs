// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using buildinfo = go.debug.buildinfo_package;
using pe = go.debug.pe_package;
using binary = encoding.binary_package;
using flag = flag_package;
using fmt = fmt_package;
using obscuretestdata = @internal.obscuretestdata_package;
using testenv = @internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using path = path_package;
using filepath = go.path.filepath_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using debug = go.runtime.debug_package;
using encoding;
using fs = io.fs_package;
using go.debug;
using go.os;
using go.path;
using go.runtime;
using io = io_package;
using static go.debug.buildinfo_internal_test_package;

partial class buildinfo_test_package {

internal static ж<bool> flagAll = flag.Bool("all", false, "test all supported GOOS/GOARCH platforms, instead of only the current platform");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testRequiresCompilingAndˢ = (@string)"test requires compiling and linking, which may be slow"u8;
internal static ref readonly @string goModˢ => ref goModˢᵇ;
private static readonly @string goModˢᵇ = "go.mod"u8;
internal static ref readonly @string helloGoˢ => ref helloGoˢᵇ;
private static readonly @string helloGoˢᵇ = "hello.go"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string srcExampleComMˢ => ref srcExampleComMˢᵇ;
private static readonly @string srcExampleComMˢᵇ = "src/example.com/m"u8;
internal static readonly object goBuildinfNotFoundˢ = (@string)"Go buildinf not found"u8;
internal static ref readonly @string mGoˢ => ref mGoˢᵇ;
private static readonly @string mGoˢᵇ = "(?m)^go\t.*\n"u8;
internal static ref readonly @string mBuildˢ => ref mBuildˢᵇ;
private static readonly @string mBuildˢᵇ = "(?m)^build\t.*\n"u8;
internal static ref readonly @string goGoversionˢ => ref goGoversionˢᵇ;
private static readonly @string goGoversionˢᵇ = "go\tGOVERSION\n"u8;
internal static ref readonly @string buildCompilerˢ => ref buildCompilerˢᵇ;
private static readonly @string buildCompilerˢᵇ = "build\t-compiler="u8;
internal static ref readonly @string doesnotexistTxtˢ => ref doesnotexistTxtˢᵇ;
private static readonly @string doesnotexistTxtˢᵇ = "doesnotexist.txt"u8;
internal static ref readonly @string emptyˢ => ref emptyˢᵇ;
private static readonly @string emptyˢᵇ = "empty"u8;

internal partial struct TestReadFile_platform /*dyn*/ {
    internal @string goos, goarch;
}

internal partial struct TestReadFile_cases /*dyn*/ {
    internal @string name;
    internal Func<ж<testing.T>, @string, @string, @string, @string> build;
    internal @string want;
    internal @string wantErr;
}

// TestReadFile confirms that ReadFile can read build information from binaries
// on supported target platforms. It builds a trivial binary on the current
// platforms (or all platforms if -all is set) in various configurations and
// checks that build information can or cannot be read.
public static void TestReadFile(ж<testing.T> t) {
    var platformsᴷ = new stackAppend<TestReadFile_platform>();

    if (testing.Short()) {
        t.Skip(testRequiresCompilingAndˢ);
    }
    testenv.MustHaveGoBuild(new buildinfo_test_package.testing_TжTB(t));
    var platforms = slice<TestReadFile_platform>([
        new("aix", "ppc64"),
        new("darwin", "amd64"),
        new("darwin", "arm64"),
        new("linux", "386"),
        new("linux", "amd64"),
        new("windows", "386"),
        new("windows", "amd64")
    ]);
    ref var runtimePlatform = ref heap<TestReadFile_platform>(new TestReadFile_platform(runtime.GOOS, runtime.GOARCH), out var ᏑruntimePlatform);
    var haveRuntimePlatform = false;
    foreach (var (_, p) in platforms) {
        if (p == runtimePlatform) {
            haveRuntimePlatform = true;
            break;
        }
    }
    if (!haveRuntimePlatform) {
        platforms = append(platformsᴷ, platforms, runtimePlatform);
    }

    var buildModes = slice<@string>(["pie", "exe"]);
    if (testenv.HasCGO()) {
        buildModes = append(buildModes, (@string)"c-shared");
    }

    // Keep in sync with src/cmd/go/internal/work/init.go:buildModeInit.
    @string badmode(@string goos, @string goarch, @string buildmode) => fmt.Sprintf("-buildmode=%s not supported on %s/%s"u8, buildmode, goos, goarch);

    var badmodeʗ1 = badmode;
    var buildWithModules = @string (ж<testing.T> tΔ1, @string goos, @string goarch, @string buildmode) => {
        @string dir = tΔ1.TempDir();
        @string gomodPath = filepath.Join(dir, goModˢ);
        var gomodData = slice<byte>("module example.com/m\ngo 1.18\n"u8);
        {
            var err = os.WriteFile(gomodPath, gomodData, 438); if (err != nil) {
                tΔ1.Fatal(err);
            }
        }
        @string helloPath = filepath.Join(dir, helloGoˢ);
        var helloData = slice<byte>("package main\nfunc main() {}\n"u8);
        {
            var err = os.WriteFile(helloPath, helloData, 438); if (err != nil) {
                tΔ1.Fatal(err);
            }
        }
        @string outPath = filepath.Join(dir, path.Base(tΔ1.Name()));
        var cmd = exec.Command(testenv.GoToolPath(new buildinfo_test_package.testing_TжTB(tΔ1)), buildˢ, "-o=" + outPath, "-buildmode=" + buildmode);
        cmd.Dir = dir;
        cmd.Env = append(os.Environ(), (@string)"GO111MODULE=on", "GOOS=" + goos, "GOARCH=" + goarch);
        var stderr = Ꮡ(new strings.Builder());
        cmd.Stderr = new buildinfo_test_package.strings_BuilderжWriter(stderr);
        {
            var err = cmd.Run(); if (err != nil) {
                {
                    @string badmodeMsg = badmodeʗ1(goos, goarch, buildmode); if (strings.Contains(stderr.String(), badmodeMsg)) {
                        tΔ1.Skip(badmodeMsg);
                    }
                }
                tΔ1.Fatalf("failed building test file: %v\n%s", err, stderr.String());
            }
        }
        return outPath;
    };

    var badmodeʗ2 = badmode;
    var buildWithGOPATH = @string (ж<testing.T> tΔ2, @string goos, @string goarch, @string buildmode) => {
        @string gopathDir = tΔ2.TempDir();
        @string pkgDir = filepath.Join(gopathDir, srcExampleComMˢ);
        {
            var err = os.MkdirAll(pkgDir, 511); if (err != nil) {
                tΔ2.Fatal(err);
            }
        }
        @string helloPath = filepath.Join(pkgDir, helloGoˢ);
        var helloData = slice<byte>("package main\nfunc main() {}\n"u8);
        {
            var err = os.WriteFile(helloPath, helloData, 438); if (err != nil) {
                tΔ2.Fatal(err);
            }
        }
        @string outPath = filepath.Join(gopathDir, path.Base(tΔ2.Name()));
        var cmd = exec.Command(testenv.GoToolPath(new buildinfo_test_package.testing_TжTB(tΔ2)), buildˢ, "-o=" + outPath, "-buildmode=" + buildmode);
        cmd.Dir = pkgDir;
        cmd.Env = append(os.Environ(), (@string)"GO111MODULE=off", "GOPATH=" + gopathDir, "GOOS=" + goos, "GOARCH=" + goarch);
        var stderr = Ꮡ(new strings.Builder());
        cmd.Stderr = new buildinfo_test_package.strings_BuilderжWriter(stderr);
        {
            var err = cmd.Run(); if (err != nil) {
                {
                    @string badmodeMsg = badmodeʗ2(goos, goarch, buildmode); if (strings.Contains(stderr.String(), badmodeMsg)) {
                        tΔ2.Skip(badmodeMsg);
                    }
                }
                tΔ2.Fatalf("failed building test file: %v\n%s", err, stderr.String());
            }
        }
        return outPath;
    };

    void damageBuildInfo(ж<testing.T> tΔ3, @string name) {
        var (data, err) = os.ReadFile(name);
        if (err != nil) {
            tΔ3.Fatal(err);
        }
        nint i = bytes.Index(data, slice<byte>(((@string)(new byte[]{0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a}))));
        if (i < 0) {
            tΔ3.Fatal(goBuildinfNotFoundˢ);
        }
        data[i + 2] = (byte)'N';
        {
            var errΔ1 = os.WriteFile(name, data, 438); if (errΔ1 != nil) {
                tΔ3.Fatal(errΔ1);
            }
        }
    }

    void damageStringLen(ж<testing.T> tΔ4, @string name) {
        var (data, err) = os.ReadFile(name);
        if (err != nil) {
            tΔ4.Fatal(err);
        }
        nint i = bytes.Index(data, slice<byte>(((@string)(new byte[]{0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a}))));
        if (i < 0) {
            tΔ4.Fatal(goBuildinfNotFoundˢ);
        }
        var verLen = data.slice(i + 32);
        binary.PutUvarint(verLen, (uint64)16 << 40); // 16TB ought to be enough for anyone.
        {
            var errΔ1 = os.WriteFile(name, data, 438); if (errΔ1 != nil) {
                tΔ4.Fatal(errΔ1);
            }
        }
    }

    var goVersionRe = regexp.MustCompile(mGoˢ);
    var buildRe = regexp.MustCompile(mBuildˢ);
    var buildReʗ1 = buildRe;
    var goVersionReʗ1 = goVersionRe;
    @string cleanOutputForComparison(@string got) {
        // Remove or replace anything that might depend on the test's environment
        // so we can check the output afterward with a string comparison.
        // We'll remove all build lines except the compiler, just to make sure
        // build lines are included.
        got = goVersionReʗ1.ReplaceAllString(got, goGoversionˢ);
        got = buildReʗ1.ReplaceAllStringFunc(got, (@string match) => {
            if (strings.HasPrefix(match, buildCompilerˢ)) {
                return match;
            }
            return ""u8;
        });
        return got;
    }



            var buildWithModulesʗ1 = buildWithModules;
            var damageBuildInfoʗ1 = damageBuildInfo;

            var buildWithModulesʗ2 = buildWithModules;
            var damageStringLenʗ1 = damageStringLen;

            var buildWithGOPATHʗ1 = buildWithGOPATH;
            var damageBuildInfoʗ2 = damageBuildInfo;
    var cases = slice<TestReadFile_cases>([
        new(
            name: "doesnotexist",
            build: (ж<testing.T> tΔ5, @string goos, @string goarch, @string buildmode) => doesnotexistTxtˢ,
            wantErr: "doesnotexist"
        ),
        new(
            name: "empty",
            build: (ж<testing.T> tΔ6, @string _Δp1, @string _Δp2, @string _Δp3) => {
                @string dir = tΔ6.TempDir();
                @string name = filepath.Join(dir, emptyˢ);
                {
                    var err = os.WriteFile(name, default, 438); if (err != nil) {
                        tΔ6.Fatal(err);
                    }
                }
                return name;
            },
            wantErr: "unrecognized file format"
        ),
        new(
            name: "valid_modules",
            build: buildWithModules,
            want: "go\tGOVERSION\n"u8 + "path\texample.com/m\n"u8 + "mod\texample.com/m\t(devel)\t\n"u8 + "build\t-compiler=gc\n"u8
        ),
        new(
            name: "invalid_modules",
            build: (ж<testing.T> tΔ7, @string goos, @string goarch, @string buildmode) => {
                @string name = buildWithModulesʗ1(tΔ7, goos, goarch, buildmode);
                damageBuildInfoʗ1(tΔ7, name);
                return name;
            },
            wantErr: "not a Go executable"
        ),
        new(
            name: "invalid_str_len",
            build: (ж<testing.T> tΔ8, @string goos, @string goarch, @string buildmode) => {
                @string name = buildWithModulesʗ2(tΔ8, goos, goarch, buildmode);
                damageStringLenʗ1(tΔ8, name);
                return name;
            },
            wantErr: "not a Go executable"
        ),
        new(
            name: "valid_gopath",
            build: buildWithGOPATH,
            want: "go\tGOVERSION\n"u8 + "path\texample.com/m\n"u8 + "build\t-compiler=gc\n"u8
        ),
        new(
            name: "invalid_gopath",
            build: (ж<testing.T> tΔ9, @string goos, @string goarch, @string buildmode) => {
                @string name = buildWithGOPATHʗ1(tΔ9, goos, goarch, buildmode);
                damageBuildInfoʗ2(tΔ9, name);
                return name;
            },
            wantErr: "not a Go executable"
        )
    ]);

    foreach (var (_, vᴛ1) in platforms) {
        ref var p = ref heap(new TestReadFile_platform(), out var Ꮡp);
        p = vᴛ1;

        var buildModesʗ1 = buildModes;
        var casesʗ1 = cases;
        var cleanOutputForComparisonʗ1 = cleanOutputForComparison;
        var pʗ1 = p;
        var runtimePlatformʗ1 = runtimePlatform;
        t.Run(p.goos + "_"u8 + p.goarch, (ж<testing.T> tΔ10) => {
            if (pʗ1 != runtimePlatformʗ1 && !flagAll.Value) {
                tΔ10.Skipf("skipping platforms other than %s_%s because -all was not set", runtimePlatformʗ1.goos, runtimePlatformʗ1.goarch);
            }
            foreach (var (_, mode) in buildModesʗ1) {
                var casesʗ2 = casesʗ1;
                var cleanOutputForComparisonʗ2 = cleanOutputForComparisonʗ1;
                var pʗ2 = pʗ1;
                tΔ10.Run(mode, (ж<testing.T> tΔ11) => {
                    foreach (var (_, vᴛ2) in casesʗ2) {
                        ref var tc = ref heap(new TestReadFile_cases(), out var Ꮡtc);
                        tc = vᴛ2;

                        var cleanOutputForComparisonʗ3 = cleanOutputForComparisonʗ2;
                        var pʗ3 = pʗ2;
                        var tcʗ1 = tc;
                        tΔ11.Run(tc.name, (ж<testing.T> tΔ12) => {
                            tΔ12.Parallel();
                            @string name = tcʗ1.build(tΔ12, pʗ3.goos, pʗ3.goarch, mode);
                            {
                                var (info, err) = buildinfo.ReadFile(name); if (err != nil) {
                                    if (tcʗ1.wantErr == ""u8) {
                                        tΔ12.Fatalf("unexpected error: %v", err);
                                    } else {
                                        @string errMsg = err.Error(); if (!strings.Contains(errMsg, tcʗ1.wantErr)) {
                                            tΔ12.Fatalf("got error %q; want error containing %q", errMsg, tcʗ1.wantErr);
                                        }
                                    }
                                } else {
                                    if (tcʗ1.wantErr != ""u8) {
                                        tΔ12.Fatalf("unexpected success; want error containing %q", tcʗ1.wantErr);
                                    }
                                    @string got = info.String();
                                    {
                                        @string clean = cleanOutputForComparisonʗ3(got); if (got != tcʗ1.want && clean != tcʗ1.want) {
                                            tΔ12.Fatalf("got:\n%s\nwant:\n%s", got, tcʗ1.want);
                                        }
                                    }
                                }
                            }
                        });
                    }
                });
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGo117Go117Base64ˢ => ref testdataGo117Go117Base64ˢᵇ;
private static readonly @string testdataGo117Go117Base64ˢᵇ = "testdata/go117/go117.base64"u8;

// Test117 verifies that parsing of the old, pre-1.18 format works.
public static void Test117(ж<testing.T> t) {
    var (b, err) = obscuretestdata.ReadFile(testdataGo117Go117Base64ˢ);
    if (err != nil) {
        t.Fatalf("ReadFile got err %v, want nil", err);
    }

    (var info, err) = buildinfo.Read(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)));
    if (err != nil) {
        t.Fatalf("Read got err %v, want nil", err);
    }

    if (info.GoVersion != "go1.17"u8) {
        t.Errorf("GoVersion got %s want go1.17", info.GoVersion);
    }
    if (info.Path != "example.com/go117"u8) {
        t.Errorf("Path got %s want example.com/go117", info.Path);
    }
    if (info.Main.Path != "example.com/go117"u8) {
        t.Errorf("Main.Path got %s want example.com/go117", info.Main.Path);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataNotgoNotgoBase64ˢ => ref testdataNotgoNotgoBase64ˢᵇ;
private static readonly @string testdataNotgoNotgoBase64ˢᵇ = "testdata/notgo/notgo.base64"u8;
internal static ref readonly @string notAGoExecutableˢ => ref notAGoExecutableˢᵇ;
private static readonly @string notAGoExecutableˢᵇ = "not a Go executable"u8;

// TestNotGo verifies that parsing of a non-Go binary returns the proper error.
public static void TestNotGo(ж<testing.T> t) {
    var (b, err) = obscuretestdata.ReadFile(testdataNotgoNotgoBase64ˢ);
    if (err != nil) {
        t.Fatalf("ReadFile got err %v, want nil", err);
    }

    (_, err) = buildinfo.Read(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)));
    if (err == nil) {
        t.Fatalf("Read got nil err, want non-nil");
    }

    // The precise error text here isn't critical, but we want something
    // like errNotGoExe rather than e.g., a file read error.
    if (!strings.Contains(err.Error(), notAGoExecutableˢ)) {
        t.Errorf("ReadFile got err %v want not a Go executable", err);
    }
}

// FuzzIssue57002 is a regression test for golang.org/issue/57002.
//
// The cause of issue 57002 is when pointerSize is not being checked,
// the read can panic with slice bounds out of range
public static void FuzzIssue57002(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    // input from issue
    f.Add(slice<byte>([0x4d, 0x5a, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x50, 0x45, 0x0, 0x0, 0x0, 0x0, 0x5, 0x0, 0x20, 0x20, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0x20, 0x3f, 0x0, 0x20, 0x0, 0x0, 0x20, 0x20, 0x20, 0x20, 0x20, 0xff, 0x20, 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xb, 0x20, 0x20, 0x20, 0xfc, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x9, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x20, 0x20, 0x20, 0x20, 0x20, 0xef, 0x20, 0xff, 0xbf, 0xff, 0xff, 0xff, 0xff, 0xff, 0xf, 0x0, 0x2, 0x0, 0x20, 0x0, 0x0, 0x9, 0x0, 0x4, 0x0, 0x20, 0xf6, 0x0, 0xd3, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x1, 0x0, 0x0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0xa, 0x20, 0xa, 0x20, 0x20, 0x20, 0xff, 0x20, 0x20, 0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a, 0xde, 0xb5, 0xdf, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x6, 0x7f, 0x7f, 0x7f, 0x20, 0xf4, 0xb2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0x5, 0x0, 0x20, 0x20, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0x20, 0x3f, 0x27, 0x20, 0x0, 0xd, 0x0, 0xa, 0x20, 0x20, 0x20, 0x20, 0x20, 0xff, 0x20, 0x20, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x0, 0x20, 0x20, 0x0, 0x0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x5c, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20]));
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> input) => {
        buildinfo.Read(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(input)));
    });
}

// TestIssue54968 is a regression test for golang.org/issue/54968.
//
// The cause of issue 54968 is when the first buildInfoMagic is invalid, it
// enters an infinite loop.
public static void TestIssue54968(ж<testing.T> t) {
    t.Parallel();

    const nint paddingSize = 200;
    const nint buildInfoAlign = 16;
    var buildInfoMagic = slice<byte>(((@string)(new byte[]{0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a})));

    // Construct a valid PE header.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    buf.Write(slice<byte>([(byte)'M', (byte)'Z']));
    buf.Write(bytes.Repeat(slice<byte>([0]), 0x3c - 2));
    // At location 0x3c, the stub has the file offset to the PE signature.
    binary.Write(new buildinfo_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, (int32)(0x3c + 4));

    buf.Write(slice<byte>([(byte)'P', (byte)'E', 0, 0]));

    binary.Write(new buildinfo_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new pe.FileHeader(NumberOfSections: 1));

    var sh = new pe.SectionHeader32(
        Name: new uint8[]{(uint8)'t', 0}.array(8),
        SizeOfRawData: (uint32)(paddingSize + len(buildInfoMagic)),
        PointerToRawData: (uint32)buf.Len()
    );
    sh.PointerToRawData = (uint32)(buf.Len() + binary.Size(sh));

    binary.Write(new buildinfo_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, sh);

    nint start = buf.Len();
    buf.Write(bytes.Repeat(slice<byte>([0]), paddingSize + len(buildInfoMagic)));
    var data = buf.Bytes();

    {
        var (_, err) = pe.NewFile(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data))); if (err != nil) {
            t.Fatalf("need a valid PE header for the misaligned buildInfoMagic test: %s", err);
        }
    }

    // Place buildInfoMagic after the header.
    for (nint iᴛ1 = 1; iᴛ1 < paddingSize - len(buildInfoMagic); iᴛ1++) {
        var i = iᴛ1;
        // Test only misaligned buildInfoMagic.
        if (i % buildInfoAlign == 0) {
            continue;
        }

        var buildInfoMagicʗ1 = buildInfoMagic;
        var dataʗ1 = data;
        t.Run(fmt.Sprintf("start_at_%d"u8, i), (ж<testing.T> tΔ1) => {
            var d = dataʗ1.slice(0, start);
            // Construct intentionally-misaligned buildInfoMagic.
            d = appendꓸꓸꓸ(d, bytes.Repeat(slice<byte>([0]), i));
            d = appendꓸꓸꓸ(d, buildInfoMagicʗ1);
            d = appendꓸꓸꓸ(d, bytes.Repeat(slice<byte>([0]), paddingSize - i));

            var (_, err) = buildinfo.Read(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(d)));

            @string wantErr = notAGoExecutableˢ;
            if (err == nil) {
                tΔ1.Errorf("got error nil; want error containing %q", wantErr);
            } else {
                @string errMsg = err.Error(); if (!strings.Contains(errMsg, wantErr)) {
                    tΔ1.Errorf("got error %q; want error containing %q", errMsg, wantErr);
                }
            }
        });
    }
}

public static void FuzzRead(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var (go117, err) = obscuretestdata.ReadFile(testdataGo117Go117Base64ˢ);
    if (err != nil) {
        Ꮡf.Errorf("Error reading go117: %v", err);
    }
    f.Add(go117);

    (var notgo, err) = obscuretestdata.ReadFile(testdataNotgoNotgoBase64ˢ);
    if (err != nil) {
        Ꮡf.Errorf("Error reading notgo: %v", err);
    }
    f.Add(notgo);

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> @in) => {
        buildinfo.Read(new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(@in)));
    });
}

} // end buildinfo_test_package
