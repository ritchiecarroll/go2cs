// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testenv provides information about what functionality
// is available in different testing environments run by the Go team.
//
// It is an internal package because these details are specific
// to the Go team's test setup (on build.golang.org) and not
// fundamental to tests in general.
namespace go.@internal;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using cfg = go.@internal.cfg_package;
using goarch = go.@internal.goarch_package;
using platform = go.@internal.platform_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strconv = go.strconv_package;
using strings = strings_package;
using sync = go.sync_package;
using testing = testing_package;
using System.Runtime.CompilerServices;
using fs = io.fs_package;
using go;
using go.@internal;
using go.os;
using io = io_package;
using iter = iter_package;
using path;
using ꓸꓸꓸstring = Span<@string>;

partial class testenv_package {

// Save the original environment during init for use in checks. A test
// binary may modify its environment before calling HasExec to change its
// behavior (such as mimicking a command-line tool), and that modified
// environment might cause environment checks to behave erratically.
internal static slice<@string> origEnv = os.Environ();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goBuilderNameˢ = "GO_BUILDER_NAME"u8;

// Builder reports the name of the builder running this test. For example,
// "gotip-linux-amd64_avx512-test_only" or "go1.24-windows-arm64" on LUCI,
// or "linux-amd64" on our old infrastructure. Prefer using runtime.GOOS,
// runtime.GOARCH, race.Enabled, reading the OS version, checking CPU
// feature flags with internal/cpu, etc. over parsing builder names when
// possible. When matching builder names, prefer a fuzzy match instead
// of a strict comparison.
// If the test is not running on the build infrastructure,
// Builder returns the empty string.
public static @string Builder() {
    return os.Getenv(goBuilderNameˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goGcflagsˢ = "GO_GCFLAGS"u8;

// HasGoBuild reports whether the current system can build programs with “go build”
// and then run them with os.StartProcess or exec.Command.
public static bool HasGoBuild() {
    if (os.Getenv(goGcflagsˢ) != ""u8) {
        // It's too much work to require every caller of the go command
        // to pass along "-gcflags="+os.Getenv("GO_GCFLAGS").
        // For now, if $GO_GCFLAGS is set, report that we simply can't
        // run go build.
        return false;
    }

    return tryGoBuild() == default!;
}

internal static Func<error> tryGoBuild;
internal static void initᴛtryGoBuild() { tryGoBuild = sync.OnceValue(error () => {
    // To run 'go build', we need to be able to exec a 'go' command.
    // We somewhat arbitrarily choose to exec 'go tool -n compile' because that
    // also confirms that cmd/go can find the compiler. (Before CL 472096,
    // we sometimes ended up with cmd/go installed in the test environment
    // without a cmd/compile it could use to actually build things.)
    var (goToolΔ1, err) = goTool();
    if (err != default!) {
        return err;
    }
    var cmd = exec.Command(goToolΔ1, "tool"u8, "-n", "compile");
    cmd.Value.Env = origEnv;
    (var @out, err) = cmd.Output();
    if (err != default!) {
        return fmt.Errorf("%v: %w"u8, cmd.OrTypedNil(), err);
    }
    @out = bytes.TrimSpace(@out);
    if (len(@out) == 0) {
        return fmt.Errorf("%v: no tool reported"u8, cmd.OrTypedNil());
    }
    {
        var (_, errΔ1) = exec.LookPath((@string)@out); if (errΔ1 != default!) {
            return errΔ1;
        }
    }

    if (platform.MustLinkExternal(Δruntime.GOOS, Δruntime.GOARCH, false)) {
        // We can assume that we always have a complete Go toolchain available.
        // However, this platform requires a C linker to build even pure Go
        // programs, including tests. Do we have one in the test environment?
        // (On Android, for example, the device running the test might not have a
        // C toolchain installed.)
        //
        // If CC is set explicitly, assume that we do. Otherwise, use 'go env CC'
        // to determine which toolchain it would use by default.
        if (os.Getenv("CC"u8) == ""u8) {
            var cmdΔ1 = exec.Command(goToolΔ1, "env"u8, "CC");
            cmdΔ1.Value.Env = origEnv;
            var (outΔ1, errΔ2) = cmdΔ1.Output();
            if (errΔ2 != default!) {
                return fmt.Errorf("%v: %w"u8, cmdΔ1.OrTypedNil(), errΔ2);
            }
            outΔ1 = bytes.TrimSpace(outΔ1);
            if (len(outΔ1) == 0) {
                return fmt.Errorf("%v: no CC reported"u8, cmdΔ1.OrTypedNil());
            }
            (_, errΔ2) = exec.LookPath((@string)outΔ1);
            return errΔ2;
        }
    }
    return default!;
}); }

// MustHaveGoBuild checks that the current system can build programs with “go build”
// and then run them with os.StartProcess or exec.Command.
// If not, MustHaveGoBuild calls t.Skip with an explanation.
public static void MustHaveGoBuild(testing.TB t) {
    if (os.Getenv(goGcflagsˢ) != ""u8) {
        t.Helper();
        t.Skipf("skipping test: 'go build' not compatible with setting $GO_GCFLAGS"u8);
    }
    if (!HasGoBuild()) {
        t.Helper();
        t.Skipf("skipping test: 'go build' unavailable: %v"u8, tryGoBuild());
    }
}

// HasGoRun reports whether the current system can run programs with “go run”.
public static bool HasGoRun() {
    // For now, having go run and having go build are the same.
    return HasGoBuild();
}

// MustHaveGoRun checks that the current system can run programs with “go run”.
// If not, MustHaveGoRun calls t.Skip with an explanation.
public static partial void MustHaveGoRun(testing.TB t) {
    if (!HasGoRun()) {
        t.Helper();
        t.Skipf("skipping test: 'go run' not available on %s/%s"u8, Δruntime.GOOS, Δruntime.GOARCH);
    }
}

// HasParallelism reports whether the current system can execute multiple
// threads in parallel.
// There is a copy of this function in cmd/dist/test.go.
public static bool HasParallelism() {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        return false;
    }
    return true;
}

// MustHaveParallelism checks that the current system can execute multiple
// threads in parallel. If not, MustHaveParallelism calls t.Skip with an explanation.
public static partial void MustHaveParallelism(testing.TB t) {
    if (!HasParallelism()) {
        t.Helper();
        t.Skipf("skipping test: no parallelism available on %s/%s"u8, Δruntime.GOOS, Δruntime.GOARCH);
    }
}

// GoToolPath reports the path to the Go tool.
// It is a convenience wrapper around GoTool.
// If the tool is unavailable GoToolPath calls t.Skip.
// If the tool should be available and isn't, GoToolPath calls t.Fatal.
public static @string GoToolPath(testing.TB t) {
    MustHaveGoBuild(t);
    var (path, err) = GoTool();
    if (err != default!) {
        t.Fatal(err);
    }
    // Add all environment variables that affect the Go command to test metadata.
    // Cached test results will be invalidate when these variables change.
    // See golang.org/issue/32285.
    foreach (var (_, envVar) in strings.Fields(cfg.KnownEnv)) {
        os.Getenv(envVar);
    }
    return path;
}

internal static Func<(@string, error)> findGOROOT = sync.OnceValues((@string path, error err) () => {
    error err = default!;
    {
        @string pathΔ1 = Δruntime.GOROOT(); if (pathΔ1 != ""u8) {
            // If runtime.GOROOT() is non-empty, assume that it is valid.
            //
            // (It might not be: for example, the user may have explicitly set GOROOT
            // to the wrong directory. But this case is
            // rare, and if that happens the user can fix what they broke.)
            return (pathΔ1, default!);
        }
    }

    // runtime.GOROOT doesn't know where GOROOT is (perhaps because the test
    // binary was built with -trimpath).
    //
    // Since this is internal/testenv, we can cheat and assume that the caller
    // is a test of some package in a subdirectory of GOROOT/src. ('go test'
    // runs the test in the directory containing the packaged under test.) That
    // means that if we start walking up the tree, we should eventually find
    // GOROOT/src/go.mod, and we can report the parent directory of that.
    //
    // Notably, this works even if we can't run 'go env GOROOT' as a
    // subprocess.

    (var cwd, err) = os.Getwd();
    if (err != default!) {
        return ("", fmt.Errorf("finding GOROOT: %w"u8, err));
    }

    @string dir = cwd;
    while (ᐧ) {
        @string parent = filepath.Dir(dir);
        if (parent == dir) {
            // dir is either "." or only a volume name.
            return ("", fmt.Errorf("failed to locate GOROOT/src in any parent directory"u8));
        }

        {
            @string @base = filepath.Base(dir); if (@base != "src"u8) {
                dir = parent;
                continue; // dir cannot be GOROOT/src if it doesn't end in "src".
            }
        }

        var (b, errΔ1) = os.ReadFile(filepath.Join(dir, "go.mod"));
        if (errΔ1 != default!) {
            if (os.IsNotExist(errΔ1)) {
                dir = parent;
                continue;
            }
            return ("", fmt.Errorf("finding GOROOT: %w"u8, errΔ1));
        }
        @string goMod = (@string)b;

        while (goMod != ""u8) {
            @string line = default!;
            (line, goMod, _) = strings.Cut(goMod, "\n"u8);
            var fields = strings.Fields(line);
            if (len(fields) >= 2 && fields[0] == "module" && fields[1] == "std") {
                // Found "module std", which is the module declaration in GOROOT/src!
                return (parent, default!);
            }
        }
    }
});

// GOROOT reports the path to the directory containing the root of the Go
// project source tree. This is normally equivalent to runtime.GOROOT, but
// works even if the test binary was built with -trimpath and cannot exec
// 'go env GOROOT'.
//
// If GOROOT cannot be found, GOROOT skips t if t is non-nil,
// or panics otherwise.
public static @string GOROOT(testing.TB t) {
    var (path, err) = findGOROOT();
    if (err != default!) {
        if (t == default!) {
            throw panic(err);
        }
        t.Helper();
        t.Skip(err);
    }
    return path;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string platformCannotRunGoToolˢ = "platform cannot run go tool"u8;

// GoTool reports the path to the Go tool.
public static (@string, error) GoTool() {
    if (!HasGoBuild()) {
        return ("", errors.New(platformCannotRunGoToolˢ));
    }
    return goTool();
}

internal static Func<(@string, error)> goTool = sync.OnceValues((@string, error) () => exec.LookPath("go"u8));

// MustHaveSource checks that the entire source tree is available under GOROOT.
// If not, it calls t.Skip with an explanation.
public static void MustHaveSource(testing.TB t) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "ios"u8) {
        t.Helper();
        t.Skip("skipping test: no source tree on " + Δruntime.GOOS);
    }
}

// HasExternalNetwork reports whether the current system can use
// external (non-localhost) networks.
public static bool HasExternalNetwork() {
    return !testing.Short() && Δruntime.GOOS != "js"u8 && Δruntime.GOOS != "wasip1"u8;
}

// MustHaveExternalNetwork checks that the current system can use
// external (non-localhost) networks.
// If not, MustHaveExternalNetwork calls t.Skip with an explanation.
public static void MustHaveExternalNetwork(testing.TB t) {
    if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
        t.Helper();
        t.Skipf("skipping test: no external network on %s"u8, Δruntime.GOOS);
    }
    if (testing.Short()) {
        t.Helper();
        t.Skipf("skipping test: no external network in -short mode"u8);
    }
}

// HasCGO reports whether the current system can use cgo.
public static bool HasCGO() {
    return hasCgo();
}

internal static Func<bool> hasCgo = sync.OnceValue(bool () => {
    var (goToolΔ1, err) = goTool();
    if (err != default!) {
        return false;
    }
    var cmd = exec.Command(goToolΔ1, "env"u8, "CGO_ENABLED");
    cmd.Value.Env = origEnv;
    (var @out, err) = cmd.Output();
    if (err != default!) {
        throw panic(fmt.Sprintf("%v: %v"u8, cmd.OrTypedNil(), @out));
    }
    (var ok, err) = strconv.ParseBool((@string)bytes.TrimSpace(@out));
    if (err != default!) {
        throw panic(fmt.Sprintf("%v: non-boolean output %q"u8, cmd.OrTypedNil(), @out));
    }
    return ok;
});

// MustHaveCGO calls t.Skip if cgo is not available.
public static partial void MustHaveCGO(testing.TB t) {
    if (!HasCGO()) {
        t.Helper();
        t.Skipf("skipping test: no cgo"u8);
    }
}

// CanInternalLink reports whether the current system can link programs with
// internal linking.
public static bool CanInternalLink(bool withCgo) {
    return !platform.MustLinkExternal(Δruntime.GOOS, Δruntime.GOARCH, withCgo);
}

// SpecialBuildTypes are interesting build types that may affect linking.
partial struct SpecialBuildTypes {
    public bool Cgo;
    public bool Asan;
    public bool Msan;
    public bool Race;
}

// NoSpecialBuildTypes indicates a standard, no cgo go build.
public static readonly ж<SpecialBuildTypes> ᏑNoSpecialBuildTypes = new StaticBox<SpecialBuildTypes>(default(SpecialBuildTypes));
public static ref SpecialBuildTypes NoSpecialBuildTypes => ref ((StaticBox<SpecialBuildTypes>)ᏑNoSpecialBuildTypes).ValueSlot;

// MustInternalLink checks that the current system can link programs with internal
// linking.
// If not, MustInternalLink calls t.Skip with an explanation.
public static void MustInternalLink(testing.TB t, SpecialBuildTypes with) {
    if (with.Asan || with.Msan || with.Race) {
        t.Skipf("skipping test: internal linking with sanitizers is not supported"u8);
    }
    if (!CanInternalLink(with.Cgo)) {
        t.Helper();
        if (with.Cgo && CanInternalLink(false)) {
            t.Skipf("skipping test: internal linking on %s/%s is not supported with cgo"u8, Δruntime.GOOS, Δruntime.GOARCH);
        }
        t.Skipf("skipping test: internal linking on %s/%s is not supported"u8, Δruntime.GOOS, Δruntime.GOARCH);
    }
}

// MustInternalLinkPIE checks whether the current system can link PIE binary using
// internal linking.
// If not, MustInternalLinkPIE calls t.Skip with an explanation.
public static partial void MustInternalLinkPIE(testing.TB t) {
    if (!platform.InternalLinkPIESupported(Δruntime.GOOS, Δruntime.GOARCH)) {
        t.Helper();
        t.Skipf("skipping test: internal linking for buildmode=pie on %s/%s is not supported"u8, Δruntime.GOOS, Δruntime.GOARCH);
    }
}

// MustHaveBuildMode reports whether the current system can build programs in
// the given build mode.
// If not, MustHaveBuildMode calls t.Skip with an explanation.
public static partial void MustHaveBuildMode(testing.TB t, @string buildmode) {
    if (!platform.BuildModeSupported(Δruntime.Compiler, buildmode, Δruntime.GOOS, Δruntime.GOARCH)) {
        t.Helper();
        t.Skipf("skipping test: build mode %s on %s/%s is not supported by the %s compiler"u8, buildmode, Δruntime.GOOS, Δruntime.GOARCH, Δruntime.Compiler);
    }
}

// HasSymlink reports whether the current system can use os.Symlink.
public static bool HasSymlink() {
    var (ok, _) = hasSymlink();
    return ok;
}

// MustHaveSymlink reports whether the current system can use os.Symlink.
// If not, MustHaveSymlink calls t.Skip with an explanation.
public static void MustHaveSymlink(testing.TB t) {
    var (ok, reason) = hasSymlink();
    if (!ok) {
        t.Helper();
        t.Skipf("skipping test: cannot make symlinks on %s/%s: %s"u8, Δruntime.GOOS, Δruntime.GOARCH, reason);
    }
}

// HasLink reports whether the current system can use os.Link.
public static bool HasLink() {
    // From Android release M (Marshmallow), hard linking files is blocked
    // and an attempt to call link() on a file will return EACCES.
    // - https://code.google.com/p/android-developer-preview/issues/detail?id=3150
    return Δruntime.GOOS != "plan9"u8 && Δruntime.GOOS != "android"u8;
}

// MustHaveLink reports whether the current system can use os.Link.
// If not, MustHaveLink calls t.Skip with an explanation.
public static partial void MustHaveLink(testing.TB t) {
    if (!HasLink()) {
        t.Helper();
        t.Skipf("skipping test: hardlinks are not supported on %s/%s"u8, Δruntime.GOOS, Δruntime.GOARCH);
    }
}

internal static ж<bool> flaky = flag.Bool("flaky"u8, false, "run known-flaky tests too"u8);

public static partial void SkipFlaky(testing.TB t, nint issue) {
    if (!flaky.Value) {
        t.Helper();
        t.Skipf("skipping known flaky test without the -flaky flag; see golang.org/issue/%d"u8, issue);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goBuilderFlakyNetˢ = "GO_BUILDER_FLAKY_NET"u8;
private static readonly object skippingTestOnBuilderˢ = (@string)"skipping test on builder known to have frequent network failures"u8;

public static void SkipFlakyNet(testing.TB t) {
    {
        var (v, _) = strconv.ParseBool(os.Getenv(goBuilderFlakyNetˢ)); if (v) {
            t.Helper();
            t.Skip(skippingTestOnBuilderˢ);
        }
    }
}

// CPUIsSlow reports whether the CPU running the test is suspected to be slow.
public static bool CPUIsSlow() {
    var exprᴛ1 = Δruntime.GOARCH;
    if (exprᴛ1 == "arm"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "wasm"u8) {
        return true;
    }
    return false;
}

// SkipIfShortAndSlow skips t if -short is set and the CPU running the test is
// suspected to be slow.
//
// (This is useful for CPU-intensive tests that otherwise complete quickly.)
public static partial void SkipIfShortAndSlow(testing.TB t) {
    if (testing.Short() && CPUIsSlow()) {
        t.Helper();
        t.Skipf("skipping test in -short mode on %s"u8, Δruntime.GOARCH);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingTestWithˢ = (@string)"skipping test with optimization disabled"u8;

// SkipIfOptimizationOff skips t if optimization is disabled.
public static partial void SkipIfOptimizationOff(testing.TB t) {
    if (OptimizationOff()) {
        t.Helper();
        t.Skip(skippingTestWithˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string importConfigˢ = "# import config\n"u8;
private static readonly @string listˢ = "list"u8;
private static readonly @string exportˢ = "-export"u8;
private static readonly @string depsˢ = "-deps"u8;
private static readonly @string ifNeImportPathCommandˢ = @"{{if ne .ImportPath ""command-line-arguments""}}{{if .Export}}{{.ImportPath}}={{.Export}}{{end}}{{end}}"u8;

// WriteImportcfg writes an importcfg file used by the compiler or linker to
// dstPath containing entries for the file mappings in packageFiles, as well
// as for the packages transitively imported by the package(s) in pkgs.
//
// pkgs may include any package pattern that is valid to pass to 'go list',
// so it may also be a list of Go source files all in the same directory.
public static void WriteImportcfg(testing.TB t, @string dstPath, map<@string, @string> packageFiles, params ꓸꓸꓸstring pkgsʗp) {
    var pkgs = pkgsʗp.sslice();

    t.Helper();

    var icfg = @new<bytes.Buffer>();
    icfg.WriteString(importConfigˢ);
    foreach (var (k, v) in packageFiles) {
        fmt.Fprintf(new bytes_BufferжWriter(icfg), "packagefile %s=%s\n"u8, k, v);
    }

    if (len(pkgs) > 0) {
        // Use 'go list' to resolve any missing packages and rewrite the import map.
        var cmd = Command(t, GoToolPath(t), listˢ, exportˢ, depsˢ, "-f", ifNeImportPathCommandˢ);
        cmd.Value.Args = appendꓸꓸꓸ((~cmd).Args, pkgs);
        cmd.Value.Stderr = new strings_BuilderжWriter(@new<strings.Builder>());
        var (@out, err) = cmd.Output();
        if (err != default!) {
            t.Fatalf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, (~cmd).Stderr);
        }

        foreach (var line in range<@string>(strings.SplitSeq((@string)@out, "\n"u8).Invoke)) {
            if (line == ""u8) {
                continue;
            }
            var (importPath, export, ok) = strings.Cut(line, "="u8);
            if (!ok) {
                t.Fatalf("invalid line in output from %v:\n%s"u8, cmd.OrTypedNil(), line);
            }
            if (packageFiles[importPath] == "") {
                fmt.Fprintf(new bytes_BufferжWriter(icfg), "packagefile %s=%s\n"u8, importPath, export);
            }
        }
    }

    {
        var err = os.WriteFile(dstPath, icfg.Bytes(), 438); if (err != default!) {
            t.Fatal(err);
        }
    }
}

// SyscallIsNotSupported reports whether err may indicate that a system call is
// not supported by the current platform or execution environment.
public static bool SyscallIsNotSupported(error err) {
    return syscallIsNotSupported(err);
}

// ParallelOn64Bit calls t.Parallel() unless there is a case that cannot be parallel.
// This function should be used when it is necessary to avoid t.Parallel on
// 32-bit machines, typically because the test uses lots of memory.
public static void ParallelOn64Bit(ж<testing.T> Ꮡt) {
    if (goarch.PtrSize == 4) {
        return;
    }
    Ꮡt.Parallel();
}

// go2cs generated this placeholder — func CPUProfilingBroken is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end testenv_package
