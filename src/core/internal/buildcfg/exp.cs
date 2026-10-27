// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using goexperiment = go.@internal.goexperiment_package;
using go.@internal;
using iter = iter_package;

partial class buildcfg_package {

// ExperimentFlags represents a set of GOEXPERIMENT flags relative to a baseline
// (platform-default) experiment configuration.
partial struct ExperimentFlags {
    /*embed*/ public go.@internal.goexperiment_package.Flags Flags;
    internal goexperiment.Flags baseline;
}

// Experiment contains the toolchain experiments enabled for the
// current build.
//
// (This is not necessarily the set of experiments the compiler itself
// was built with.)
//
// Experiment.baseline specifies the experiment flags that are enabled by
// default in the current toolchain. This is, in effect, the "control"
// configuration and any variation from this is an experiment.
public static ж<ExperimentFlags> ᏑExperiment = new StaticBox<ExperimentFlags>(default(ExperimentFlags));
public static ref ExperimentFlags Experiment => ref ᏑExperiment.Value;
internal static void initᴛExperiment() { Experiment = ((Func<ExperimentFlags>)(() => {
    var (flags, err) = ParseGOEXPERIMENT(GOOS, GOARCH, envOr("GOEXPERIMENT"u8, defaultGOEXPERIMENT));
    if (err != default!) {
        Error = err;
        return new ExperimentFlags(nil);
    }
    return flags.Value;
}))(); }

// DefaultGOEXPERIMENT is the embedded default GOEXPERIMENT string.
// It is not guaranteed to be canonical.
public static readonly @string DefaultGOEXPERIMENT = "";

// FramePointerEnabled enables the use of platform conventions for
// saving frame pointers.
//
// This used to be an experiment, but now it's always enabled on
// platforms that support it.
//
// Note: must agree with runtime.framepointer_enabled.
public static bool FramePointerEnabled;
internal static void initᴛFramePointerEnabled() { FramePointerEnabled = GOARCH == "amd64"u8 || GOARCH == "arm64"u8; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string regabiˢ = "regabi"u8;

// ParseGOEXPERIMENT parses a (GOOS, GOARCH, GOEXPERIMENT)
// configuration tuple and returns the enabled and baseline experiment
// flag sets.
//
// TODO(mdempsky): Move to [internal/goexperiment].
public static (ж<ExperimentFlags>, error) ParseGOEXPERIMENT(@string goos, @string goarch, @string goexp) {
    // regabiSupported is set to true on platforms where register ABI is
    // supported and enabled by default.
    // regabiAlwaysOn is set to true on platforms where register ABI is
    // always on.
    bool regabiSupported = default!;
    bool regabiAlwaysOn = default!;
    var exprᴛ1 = goarch;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "riscv64"u8) {
        regabiAlwaysOn = true;
        regabiSupported = true;
    }
    else if (exprᴛ1 == "s390x"u8) {
        regabiSupported = true;
    }

    // Older versions (anything before V16) of dsymutil don't handle
    // the .debug_rnglists section in DWARF5. See
    // https://github.com/golang/go/issues/26379#issuecomment-2677068742
    // for more context. This disables all DWARF5 on mac, which is not
    // ideal (would be better to disable just for cases where we know
    // the build will use external linking). In the GOOS=aix case, the
    // XCOFF format (as far as can be determined) doesn't seem to
    // support the necessary section subtypes for DWARF-specific
    // things like .debug_addr (needed for DWARF 5).
    var dwarf5Supported = (goos != "darwin"u8 && goos != "ios"u8 && goos != "aix"u8);

    ref var baseline = ref heap<goexperiment.Flags>(out var Ꮡbaseline);
    baseline = new goexperiment.Flags(
        RegabiWrappers: regabiSupported,
        RegabiArgs: regabiSupported,
        Dwarf5: dwarf5Supported,
        RandomizedHeapBase64: true,
        GreenTeaGC: true
    );
    var flags = Ꮡ(new ExperimentFlags(
        Flags: baseline,
        baseline: baseline
    ));

    // Pick up any changes to the baseline configuration from the
    // GOEXPERIMENT environment. This can be set at make.bash time
    // and overridden at build time.
    if (goexp != ""u8) {
        // Create a map of known experiment names.
        var names = new map<@string, Action<bool>>();
        var rv = reflect.ValueOf(flags.of(ExperimentFlags.ᏑFlags)).Elem();
        var rt = rv.Type();
        for (nint i = 0; i < rt.NumField(); i++) {
            var field = rv.Field(i);
            var fieldʗ1 = field;
                        names[strings.ToLower(rt.Field(i).Name)] = (bool p1) => fieldʗ1.SetBool(p1);
        }

        // "regabi" is an alias for all working regabi
        // subexperiments, and not an experiment itself. Doing
        // this as an alias make both "regabi" and "noregabi"
        // do the right thing.
        var flagsʗ1 = flags;
        names[regabiˢ] = (bool v) => {
            flagsʗ1.Value.RegabiWrappers = v;
            flagsʗ1.Value.RegabiArgs = v;
        };

        // Parse names.
        foreach (var iᴛ1 in range<@string>(strings.SplitSeq(goexp, ","u8).Invoke)) {
            var f = iᴛ1;
            if (f == ""u8) {
                continue;
            }
            if (f == "none"u8) {
                // GOEXPERIMENT=none disables all experiment flags.
                // This is used by cmd/dist, which doesn't know how
                // to build with any experiment flags.
                flags.Value.Flags = new goexperiment.Flags(nil);
                continue;
            }
            var val = true;
            if (strings.HasPrefix(f, "no"u8)) {
                (f, val) = (f[2..], false);
            }
            var (set, ok) = names[f, ꟷ];
            if (!ok) {
                return (default!, fmt.Errorf("unknown GOEXPERIMENT %s"u8, f));
            }
            set(val);
        }
    }

    if (regabiAlwaysOn) {
        flags.Value.RegabiWrappers = true;
        flags.Value.RegabiArgs = true;
    }
    // regabi is only supported on amd64, arm64, loong64, riscv64, s390x, ppc64 and ppc64le.
    if (!regabiSupported) {
        flags.Value.RegabiWrappers = false;
        flags.Value.RegabiArgs = false;
    }
    // Check regabi dependencies.
    if ((~flags).RegabiArgs && !(~flags).RegabiWrappers) {
        return (default!, fmt.Errorf("GOEXPERIMENT regabiargs requires regabiwrappers"u8));
    }
    return (flags, default!);
}

// String returns the canonical GOEXPERIMENT string to enable this experiment
// configuration. (Experiments in the same state as in the baseline are elided.)
public static @string String(this ж<ExperimentFlags> Ꮡexp) {
    return strings.Join(expList(Ꮡexp.of(ExperimentFlags.ᏑFlags), Ꮡexp.of(ExperimentFlags.Ꮡbaseline), false), ","u8);
}

// expList returns the list of lower-cased experiment names for
// experiments that differ from base. base may be nil to indicate no
// experiments. If all is true, then include all experiment flags,
// regardless of base.
internal static slice<@string> expList(ж<goexperiment.Flags> Ꮡexp, ж<goexperiment.Flags> Ꮡbase, bool all) {
    slice<@string> list = default!;
    var rv = reflect.ValueOf(Ꮡexp.OrTypedNil()).Elem();
    reflectꓸValue rBase = new(nil);
    if (Ꮡbase != nil) {
        rBase = reflect.ValueOf(Ꮡbase.OrTypedNil()).Elem();
    }
    var rt = rv.Type();
    for (nint i = 0; i < rt.NumField(); i++) {
        @string name = strings.ToLower(rt.Field(i).Name);
        var val = rv.Field(i).Bool();
        var baseVal = false;
        if (Ꮡbase != nil) {
            baseVal = rBase.Field(i).Bool();
        }
        if (all || val != baseVal) {
            if (val){
                list = append(list, name);
            } else {
                list = append(list, "no"u8 + name);
            }
        }
    }
    return list;
}

// Enabled returns a list of enabled experiments, as
// lower-cased experiment names.
public static slice<@string> Enabled(this ж<ExperimentFlags> Ꮡexp) {
    return expList(Ꮡexp.of(ExperimentFlags.ᏑFlags), nil, false);
}

// All returns a list of all experiment settings.
// Disabled experiments appear in the list prefixed by "no".
public static slice<@string> All(this ж<ExperimentFlags> Ꮡexp) {
    return expList(Ꮡexp.of(ExperimentFlags.ᏑFlags), nil, true);
}

} // end buildcfg_package
