// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package godebug makes the settings in the $GODEBUG environment variable
// available to other packages. These settings are often used for compatibility
// tweaks, when we need to change a default behavior but want to let users
// opt back in to the original. For example GODEBUG=http2server=0 disables
// HTTP/2 support in the net/http server.
//
// In typical usage, code should declare a Setting as a global
// and then call Value each time the current setting value is needed:
//
//	var http2server = godebug.New("http2server")
//
//	func ServeConn(c net.Conn) {
//		if http2server.Value() == "0" {
//			disallow HTTP/2
//			...
//		}
//		...
//	}

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted godebug.go output). The Go
// implementation maintains a cache of per-setting atomic pointers kept current by runtime update
// hooks (re-notified when os.Setenv changes $GODEBUG), reached through a *setting EMBEDDED in
// Setting and resolved under a sync.Once. The converted runtime has no Setenv notification, and
// the literal conversion of the embedded-pointer machinery faults at runtime (the generated
// promoted-field box treats its held nil *setting as a nil POINTER dereference, so even the
// `s.setting = lookup(...)` assignment panics). This hand-owned implementation parses $GODEBUG
// (comma-separated key=value, later entries overriding earlier ones, matching Go's backward parse)
// and serves every lookup from that snapshot, REPARSING whenever the variable's text changes —
// which is what Go's update hooks accomplish, and what `Value`'s own contract below promises. An
// unset or unlisted key yields "", Go's unset default. Both protocol arms Go layers on top of the
// snapshot are carried over faithfully: a `value#pattern` bisect suffix becomes a real
// bisect.Matcher consulted per call stack (the converted internal/bisect over the runtime's
// managed traceback surface — cmd/bisect drives it end to end), and IncNonDefault maintains real
// per-name counters registered with the runtime's metric table through the godebugRegisterMetric
// shim (managed_impl.cs, the registerPoolCleanup pattern), so runtime/metrics.Read reports
// /godebug/non-default-behavior/<name>:events exactly as Go's linkname plumbing does.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using bisect = go.@internal.bisect_package;
using godebugs = go.@internal.godebugs_package;

// Hand-owned native replacement of the converted godebug.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go.@internal;

partial class godebug_package {

// A Setting is a single setting in the $GODEBUG environment variable.
[GoType] partial struct Setting {
    internal @string name;
}

// New returns a new Setting for the $GODEBUG setting with the given name.
//
// GODEBUGs meant for use by end users must be listed in ../godebugs/table.go,
// which is used for generating and checking various documentation.
// If the name is not listed in that table, New will succeed but calling Value
// on the returned Setting will panic.
// To disable that panic for access to an undocumented setting,
// prefix the name with a #, as in godebug.New("#gofsystrace").
// The # is a signal to New but not part of the key used in $GODEBUG.
public static ж<Setting> New(@string name) {
    return Ꮡ(new Setting(name: name));
}

// Name returns the name of the setting.
[GoRecv] public static @string Name(this ref Setting s) {
    if (s.name != ""u8 && s.name[0] == (rune)'#') {
        return s.name[1..];
    }
    return s.name;
}

// Undocumented reports whether this is an undocumented setting.
[GoRecv] public static bool Undocumented(this ref Setting s) {
    return s.name != ""u8 && s.name[0] == (rune)'#';
}

// String returns a printable form for the setting: name=value.
public static @string String(this ж<Setting> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    return s.Name() + "="u8 + Ꮡs.Value();
}

// IncNonDefault increments the non-default behavior counter
// associated with the given setting.
// This counter is exposed in the runtime/metrics value
// /godebug/non-default-behavior/<name>:events.
//
// go2cs: Go keeps the counter on the per-name *setting (shared through its cache) and registers
// it with the runtime on first increment via the linkname-pulled registerMetric. The managed form
// keys the counters by name directly — the same sharing — and crosses into the runtime through
// the public godebugRegisterMetric shim, which swaps the metric's placeholder compute for this
// counter's read. Go's register() panic for an unregistrable setting is preserved verbatim.
public static void IncNonDefault(this ж<Setting> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    string name = s.Name();
    var info = godebugs.Lookup(name);

    if (info == nil || info.Value.Opaque) {
        throw panic("godebug: unexpected IncNonDefault of " + s.name);
    }

    NonDefaultCounter counter = s_nonDefault.GetOrAdd(name, static _ => new NonDefaultCounter());

    if (!counter.Registered) {
        lock (counter) {
            if (!counter.Registered) {
                NonDefaultCounter captured = counter;

                global::go.runtime_package.godebugRegisterMetric(
                    "/godebug/non-default-behavior/" + name + ":events",
                    () => Volatile.Read(ref captured.Count));

                counter.Registered = true;
            }
        }
    }

    Interlocked.Increment(ref counter.Count);
}

// The non-default behavior counters, shared per NAME exactly as Go shares them through the cached
// per-name *setting. Registered guards the once-per-name runtime registration (Go's
// nonDefaultOnce), double-checked under the counter's own lock.
private sealed class NonDefaultCounter {
    internal ulong Count;
    internal bool Registered;
}

private static readonly ConcurrentDictionary<string, NonDefaultCounter> s_nonDefault =
    new(StringComparer.Ordinal);

// Go's `func init() { setNewIncNonDefault(newIncNonDefault) }`, in effect verbatim: the runtime's own
// GODEBUG settings (panicnil, asynctimerchan, ...) count their non-default events through this factory
// (runtime.godebugInc.IncNonDefault), and until it is registered the runtime drops every increment.
// Nothing registered it before, so runtime's TestPanicNil/GODEBUG=panicnil=1 read an unmoved
// /godebug/non-default-behavior/panicnil:events. The linkname target is `internal` in the runtime, so
// the call crosses through its public shim (managed_impl.cs). A module initializer is init()'s slot: it
// stores one delegate and touches no static of this package.
[ModuleInitializer]
internal static void ᴛRegisterNewIncNonDefault() {
    global::go.runtime_package.godebugSetNewIncNonDefault(newIncNonDefault);
}

// newIncNonDefault is Go's, line for line: s.Value() first (it panics for a name not in godebugs.All,
// exactly as Go's does), then the setting's own IncNonDefault as the counter.
private static Action newIncNonDefault(@string name) {
    var s = New(name);
    s.Value();
    return () => s.IncNonDefault();
}

// Value returns the current value for the GODEBUG setting s.
//
// Value maintains an internal cache that is synchronized
// with changes to the $GODEBUG environment variable,
// making Value efficient to call as frequently as needed.
// Clients should therefore typically not attempt their own
// caching of Value's result.
//
// NoInlining: this frame is part of the call-stack surface bisect.Matcher.Stack hashes — the
// runtime's captureCallers walks the MANAGED stack, and an inlined frame silently vanishes from
// it (the same reason every entry point in the runtime's traceback funnel is pinned NoInlining).
// A mid-run JIT-tier flip that removed this frame would re-hash every already-reported bisect
// call site, and cmd/bisect requires a site's hash to be stable for a whole session.
[MethodImpl(MethodImplOptions.NoInlining)]
public static @string Value(this ж<Setting> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    string name = s.Name();

    if (godebugs.Lookup(name) == nil && !s.Undocumented()) {
        throw panic("godebug: Value of name not listed in godebugs.All: " + s.name);
    }

    if (!settings().TryGetValue(name, out SettingValue? v)) {
        return "";
    }

    // A `value#pattern` setting is enabled per CALL STACK: the matcher decides (and reports
    // matches through the stderr writer, cmd/bisect's protocol), exactly Go's
    // `v.bisect != nil && !v.bisect.Stack(&stderr)`.
    if (v.Matcher is not null && !v.Matcher.Stack(s_bisectStderr)) {
        return "";
    }

    return v.Text;
}

// The current $GODEBUG snapshot, paired with the raw text it was parsed from. Go keeps its cache
// current through runtime update hooks that fire when os.Setenv rewrites $GODEBUG; with no such
// hook here, the raw text IS the invalidation token — cheap to fetch, and a change to it is
// exactly the event the hooks signal. Reading the variable and comparing is far cheaper than
// re-parsing it, so a process whose $GODEBUG never changes (the overwhelming majority) parses
// once, as the previous one-shot form did.
//
// A one-shot snapshot instead made `Value` unable to honor the contract stated above it, and
// silently: `t.Setenv("GODEBUG", …)` — how Go's own suites exercise a setting, and the only way a
// converted test can reach one — could not be observed once ANY setting had been read, which in a
// test binary is almost always before the test that sets it runs.
private static volatile Tuple<string, Dictionary<string, SettingValue>> s_settings =
    new("", new Dictionary<string, SettingValue>(StringComparer.Ordinal));

private static Dictionary<string, SettingValue> settings() {
    string raw = Environment.GetEnvironmentVariable("GODEBUG") ?? "";
    Tuple<string, Dictionary<string, SettingValue>> current = s_settings;

    if (string.Equals(current.Item1, raw, StringComparison.Ordinal))
        return current.Item2;

    Tuple<string, Dictionary<string, SettingValue>> parsed = new(raw, parseGodebugEnv(raw));

    // Last writer wins, and every writer parsed the same text it observed — so whichever snapshot
    // is published is self-consistent (the field is volatile, so the dictionary's contents are
    // visible to whoever reads the reference). A concurrent Setenv can leave a stale one behind,
    // exactly as it can race Go's update hook; the next lookup notices and reparses.
    s_settings = parsed;

    return parsed.Item2;
}

// One parsed setting: the text before any `#`, plus the bisect matcher compiled from the pattern
// after it (nil when there is no pattern, or when the pattern fails to compile — Go's parse
// discards bisect.New's error the same way, leaving v.bisect nil).
private sealed class SettingValue {
    internal readonly string Text;
    internal readonly ж<bisect.Matcher>? Matcher;

    internal SettingValue(string text, ж<bisect.Matcher>? matcher) {
        Text = text;
        Matcher = matcher;
    }
}

// Parses comma-separated key=value pairs, later entries overriding earlier ones (Go parses
// backward and pins the first hit — same winner). A `value#pattern` suffix keeps the value's text
// and compiles the pattern into the entry's bisect matcher, matching Go's parse exactly.
private static Dictionary<string, SettingValue> parseGodebugEnv(string raw) {
    Dictionary<string, SettingValue> settings = new(StringComparer.Ordinal);

    foreach (string pair in raw.Split(',')) {
        int eq = pair.IndexOf('=');

        if (eq < 0)
            continue;

        string value = pair[(eq + 1)..];
        int hash = value.IndexOf('#');

        if (hash < 0) {
            settings[pair[..eq]] = new SettingValue(value, null);
        }
        else {
            var (matcher, _) = bisect.New(value[(hash + 1)..]);
            settings[pair[..eq]] = new SettingValue(value[..hash], matcher);
        }
    }

    return settings;
}

// The stderr sink bisect matcher reports through — the same fd-2 write Go's runtimeStderr makes,
// implemented on the converted bisect.Writer interface directly so no adapter is needed here.
private sealed class BisectStderrWriter : bisect.Writer {
    public (nint, error) Write(slice<byte> b) {
        if (len(b) > 0) {
            System.Console.OpenStandardError().Write(b.ToSpan());
        }
        return (len(b), default!);
    }
}

private static readonly bisect.Writer s_bisectStderr = new BisectStderrWriter();

// go2cs: retained from the converted output because the package's GoImplement registration
// (package_info.cs) binds runtimeStderr to bisect.Writer; writes go to standard error directly
// (Go routes through the runtime's fd-2 write since it cannot import os).
[GoType] partial struct runtimeStderr {
}

internal static ж<runtimeStderr> Ꮡstderr = new StandardBox<runtimeStderr>(default(runtimeStderr));
internal static ref runtimeStderr stderr => ref Ꮡstderr.Value;

[GoRecv] internal static (nint, error) Write(this ref runtimeStderr _, slice<byte> b) {
    if (len(b) > 0) {
        System.Console.OpenStandardError().Write(b.ToSpan());
    }
    return (len(b), default!);
}

} // end godebug_package
