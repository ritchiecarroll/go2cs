// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using io = io_package;

partial class debug_package {

// exported from runtime.
internal static partial @string modinfo();

// ReadBuildInfo returns the build information embedded
// in the running binary. The information is available only
// in binaries built with module support.
public static (ж<BuildInfo> info, bool ok) ReadBuildInfo() {
    @string data = modinfo();
    if (len(data) < 32) {
        return (default!, false);
    }
    data = data.slice(16, len(data) - 16);
    var (bi, err) = ParseBuildInfo(data);
    if (err != default!) {
        return (default!, false);
    }
    // The go version is stored separately from other build info, mostly for
    // historical reasons. It is not part of the modinfo() string, and
    // ParseBuildInfo does not recognize it. We inject it here to hide this
    // awkwardness from the user.
    bi.Value.GoVersion = runtime.Version();
    return (bi, true);
}

// BuildInfo represents the build information read from a Go binary.
[GoType] partial struct BuildInfo {
    // GoVersion is the version of the Go toolchain that built the binary
    // (for example, "go1.19.2").
    public @string GoVersion;
    // Path is the package path of the main package for the binary
    // (for example, "golang.org/x/tools/cmd/stringer").
    public @string Path;
    // Main describes the module that contains the main package for the binary.
    public Module Main;
    // Deps describes all the dependency modules, both direct and indirect,
    // that contributed packages to the build of this binary.
    public slice<ж<Module>> Deps;
    // Settings describes the build settings used to build the binary.
    public slice<BuildSetting> Settings;
}

// A Module describes a single module included in a build.
[GoType] partial struct Module {
    public @string Path; // module path
    public @string Version; // module version
    public @string Sum; // checksum
    public ж<Module> Replace; // replaced by this module
}

// A BuildSetting is a key-value pair describing one setting that influenced a build.
//
// Defined keys include:
//
//   - -buildmode: the buildmode flag used (typically "exe")
//   - -compiler: the compiler toolchain flag used (typically "gc")
//   - CGO_ENABLED: the effective CGO_ENABLED environment variable
//   - CGO_CFLAGS: the effective CGO_CFLAGS environment variable
//   - CGO_CPPFLAGS: the effective CGO_CPPFLAGS environment variable
//   - CGO_CXXFLAGS:  the effective CGO_CXXFLAGS environment variable
//   - CGO_LDFLAGS: the effective CGO_LDFLAGS environment variable
//   - DefaultGODEBUG: the effective GODEBUG settings
//   - GOARCH: the architecture target
//   - GOAMD64/GOARM/GO386/etc: the architecture feature level for GOARCH
//   - GOOS: the operating system target
//   - GOFIPS140: the frozen FIPS 140-3 module version, if any
//   - vcs: the version control system for the source tree where the build ran
//   - vcs.revision: the revision identifier for the current commit or checkout
//   - vcs.time: the modification time associated with vcs.revision, in RFC3339 format
//   - vcs.modified: true or false indicating whether the source tree had local modifications
[GoType] partial struct BuildSetting {
    // Key and Value describe the build setting.
    // Key must not contain an equals sign, space, tab, or newline.
    // Value must not contain newlines ('\n').
    public @string Key, Value;
}

// quoteKey reports whether key is required to be quoted.
internal static bool quoteKey(@string key) {
    return len(key) == 0 || strings.ContainsAny(key, "= \t\r\n\"`"u8);
}

// quoteValue reports whether value is required to be quoted.
internal static bool quoteValue(@string value) {
    return strings.ContainsAny(value, " \t\r\n\"`"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string modˢ = "mod"u8;
private static readonly @string depˢ = "dep"u8;

// String returns a string representation of a [BuildInfo].
[GoRecv] public static @string String(this ref BuildInfo bi) {
    var buf = @new<strings.Builder>();
    if (bi.GoVersion != ""u8) {
        fmt.Fprintf(new strings_BuilderжWriter(buf), "go\t%s\n"u8, bi.GoVersion);
    }
    if (bi.Path != ""u8) {
        fmt.Fprintf(new strings_BuilderжWriter(buf), "path\t%s\n"u8, bi.Path);
    }
    ref var formatMod = ref heap<Action<@string, Module>>(out var ᏑformatMod);
    var bufʗ1 = buf;
    formatMod = (@string word, Module m) => {
        bufʗ1.WriteString(word);
        bufʗ1.WriteByte((rune)'\t');
        bufʗ1.WriteString(m.Path);
        bufʗ1.WriteByte((rune)'\t');
        bufʗ1.WriteString(m.Version);
        if (m.Replace == nil){
            bufʗ1.WriteByte((rune)'\t');
            bufʗ1.WriteString(m.Sum);
        } else {
            bufʗ1.WriteByte((rune)'\n');
            ᏑformatMod.ValueSlot("=>"u8, m.Replace.Value);
        }
        bufʗ1.WriteByte((rune)'\n');
    };
    if (bi.Main != (new Module(nil))) {
        formatMod(modˢ, bi.Main);
    }
    foreach (var (_, dep) in bi.Deps) {
        formatMod(depˢ, dep.Value);
    }
    foreach (var (_, s) in bi.Settings) {
        @string key = s.Key;
        if (quoteKey(key)) {
            key = strconv.Quote(key);
        }
        @string value = s.Value;
        if (quoteValue(value)) {
            value = strconv.Quote(value);
        }
        fmt.Fprintf(new strings_BuilderжWriter(buf), "build\t%s=%s\n"u8, key, value);
    }
    return buf.String();
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string pathLineᶜ = "path\t"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string modLineᶜ = "mod\t"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string depLineᶜ = "dep\t"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string repLineᶜ = "=>\t"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string buildLineᶜ = "build\t"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string newlineᶜ = "\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string tabᶜ = "\t"u8;

// ParseBuildInfo parses the string returned by [*BuildInfo.String],
// restoring the original BuildInfo,
// except that the GoVersion field is not set.
// Programs should normally not call this function,
// but instead call [ReadBuildInfo], [debug/buildinfo.ReadFile],
// or [debug/buildinfo.Read].
public static (ж<BuildInfo> bi, error err) ParseBuildInfo(@string data) {
    ж<BuildInfo> bi = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        nint lineNum = 1;
        defer(() => {
            if (err != default!) {
                err = fmt.Errorf("could not parse Go build info: line %d: %w"u8, lineNum, err);
            }
        }, ref ᒐ);
        @string pathLine = pathLineᶜ;
        @string modLine = modLineᶜ;
        @string depLine = depLineᶜ;
        @string repLine = repLineᶜ;
        @string buildLine = buildLineᶜ;
        @string newline = newlineᶜ;
        @string tab = tabᶜ;
        (Module, error) readModuleLine(slice<@string> elem) {
            if (len(elem) != 2 && len(elem) != 3) {
                return (new Module(nil), fmt.Errorf("expected 2 or 3 columns; got %d"u8, len(elem)));
            }
            @string version = elem[1];
            @string sum = ""u8;
            if (len(elem) == 3) {
                sum = elem[2];
            }
            return (new Module(
                Path: elem[0],
                Version: version,
                Sum: sum
            ), default!);
        }
        bi = @new<BuildInfo>();
        ж<Module> last = default!;
        @string line = default!;
        bool ok = default!;
        // Reverse of BuildInfo.String(), except for go version.
        while (len(data) > 0) {
            (line, data, ok) = strings.Cut(data, newline);
            if (!ok) {
                break;
            }
            switch (ᐧ) {
            case {} when strings.HasPrefix(line, pathLine): {
                @string elem = line[(int)(len(pathLine))..];
                bi.Value.Path = elem;
                break;
            }
            case {} when strings.HasPrefix(line, modLine): {
                var elem = strings.Split(line[(int)(len(modLine))..], tab);
                last = bi.of(BuildInfo.ᏑMain);
                (last.Value, err) = readModuleLine(elem);
                if (err != default!) {
                    (bi, err) = (default!, err); goto ᒐdone;
                }
                break;
            }
            case {} when strings.HasPrefix(line, depLine): {
                var elem = strings.Split(line[(int)(len(depLine))..], tab);
                last = @new<Module>();
                bi.Value.Deps = append((~bi).Deps, last);
                (last.Value, err) = readModuleLine(elem);
                if (err != default!) {
                    (bi, err) = (default!, err); goto ᒐdone;
                }
                break;
            }
            case {} when strings.HasPrefix(line, repLine): {
                var elem = strings.Split(line[(int)(len(repLine))..], tab);
                if (len(elem) != 3) {
                    (bi, err) = (default!, fmt.Errorf("expected 3 columns for replacement; got %d"u8, len(elem))); goto ᒐdone;
                }
                if (last == nil) {
                    (bi, err) = (default!, fmt.Errorf("replacement with no module on previous line"u8)); goto ᒐdone;
                }
                last.Value.Replace = Ꮡ(new Module(
                    Path: elem[0],
                    Version: elem[1],
                    Sum: elem[2]
                ));
                last = default!;
                break;
            }
            case {} when strings.HasPrefix(line, buildLine): {
                @string kv = line[(int)(len(buildLine))..];
                if (len(kv) < 1) {
                    (bi, err) = (default!, fmt.Errorf("build line missing '='"u8)); goto ᒐdone;
                }
                @string key = default!;
                @string rawValue = default!;
                switch (kv[0]) {
                case (rune)'=': {
                    (bi, err) = (default!, fmt.Errorf("build line with missing key"u8)); goto ᒐdone;
                }
                case (rune)'`' or (rune)'"': {
                    var (rawKey, errΔ6) = strconv.QuotedPrefix(kv);
                    if (errΔ6 != default!) {
                        (bi, err) = (default!, fmt.Errorf("invalid quoted key in build line"u8)); goto ᒐdone;
                    }
                    if (len(kv) == len(rawKey)) {
                        (bi, err) = (default!, fmt.Errorf("build line missing '=' after quoted key"u8)); goto ᒐdone;
                    }
                    {
                        var c = kv[len(rawKey)]; if (c != (rune)'=') {
                            (bi, err) = (default!, fmt.Errorf("unexpected character after quoted key: %q"u8, c)); goto ᒐdone;
                        }
                    }
                    (key, _) = strconv.Unquote(rawKey);
                    rawValue = kv.slice(len(rawKey) + 1);
                    break;
                }
                default: {
                    bool okΔ4 = default!;
                    (key, rawValue, okΔ4) = strings.Cut(kv, "="u8);
                    if (!okΔ4) {
                        (bi, err) = (default!, fmt.Errorf("build line missing '=' after key"u8)); goto ᒐdone;
                    }
                    if (quoteKey(key)) {
                        (bi, err) = (default!, fmt.Errorf("unquoted key %q must be quoted"u8, key)); goto ᒐdone;
                    }
                    break;
                }}

                @string value = default!;
                if (len(rawValue) > 0) {
                    switch (rawValue[0]) {
                    case (rune)'`' or (rune)'"': {
                        error errΔ8 = default!;
                        (value, errΔ8) = strconv.Unquote(rawValue);
                        if (errΔ8 != default!) {
                            (bi, err) = (default!, fmt.Errorf("invalid quoted value in build line"u8)); goto ᒐdone;
                        }
                        break;
                    }
                    default: {
                        value = rawValue;
                        if (quoteValue(value)) {
                            (bi, err) = (default!, fmt.Errorf("unquoted value %q must be quoted"u8, value)); goto ᒐdone;
                        }
                        break;
                    }}

                }
                bi.Value.Settings = append((~bi).Settings, new BuildSetting(Key: key, Value: value));
                break;
            }}

            lineNum++;
        }
        (bi, err) = (bi, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (bi, err);
}

} // end debug_package
