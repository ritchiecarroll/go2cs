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
//
// Each time a non-default setting causes a change in program behavior,
// code must call [Setting.IncNonDefault] to increment a counter that can
// be reported by [runtime/metrics.Read]. The call must only happen when
// the program executes a non-default behavior, not just when the setting
// is set to a non-default value. This is occasionally (but very rarely)
// infeasible, in which case the internal/godebugs table entry must set
// Opaque: true, and the documentation in doc/godebug.md should
// mention that metrics are unavailable.
//
// Conventionally, the global variable representing a godebug is named
// for the godebug itself, with no case changes:
//
//	var gotypesalias = godebug.New("gotypesalias") // this
//	var goTypesAlias = godebug.New("gotypesalias") // NOT THIS
//
// The test in internal/godebugs that checks for use of IncNonDefault
// requires the use of this convention.
//
// Note that counters used with IncNonDefault must be added to
// various tables in other packages. See the [Setting.IncNonDefault]
// documentation for details.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted godebug.go output). This is Go's
// godebug.go transcribed: the per-name cache of *setting values, lookup, the runtime's update hook
// and the backward parse, kept in the converter's own emitted shape (the godebug.cs.auto review
// sibling beside this file) wherever that shape runs. The runtime calls update(def, env) once when
// init registers it, and again on every os.Setenv/Unsetenv of GODEBUG (syscall_runtimeSetenv's
// godebugNotify), so Value reads a cached atomic pointer, never the environment. update is also
// where Go 1.26 locks an Immutable setting (fips140) at its first cached value.
//
// init is Go's, statement for statement, as a [ModuleInitializer]. That is the attribute the
// emission's init attribute aliases; it is spelled out so that this package's own init stays
// invisible to an artifact scrape, which importInit_test's TestPackageInitializesSeesAHandOwnedInit
// relies on. It registers through the runtime first, as Go's does. Calling the runtime loads it,
// and the runtime's own module initializer stores the startup $GODEBUG (goenvs_impl.cs) before any
// runtime code runs. So the first update, made inside setUpdate, parses the startup environment.
//
// The transcription departs from the emission at three places, each because the emitted form
// cannot run here:
//   1. Go EMBEDS *setting in Setting. The emitted promoted-field box treats its nil *setting as a
//      nil POINTER dereference, so even the `s.setting = lookup(...)` assignment panics. The field
//      is named here instead, and every promoted access spells its `.setting` hop.
//   2. setUpdate, registerMetric and setNewIncNonDefault are bodyless //go:linkname pulls. The
//      runtime functions they name are `internal`, so each one crosses through the runtime's
//      public shim in managed_impl.cs (godebugSetUpdate, godebugRegisterMetric,
//      godebugSetNewIncNonDefault), which forwards unchanged.
//   3. The bisect matcher reports through BisectStderrWriter. Go hands it &stderr, whose Write
//      is a //go:linkname to runtime.write; both write fd 2.


using System;
using System.Runtime.CompilerServices;

// Hand-owned native replacement of the converted godebug.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go.@internal;

using bisect = bisect_package;
using godebugs = godebugs_package;
using sync = global::go.sync_package;
using atomic = global::go.sync.atomic_package;
using go.sync;

partial class godebug_package {

// A Setting is a single setting in the $GODEBUG environment variable.
[GoType] partial struct Setting {
    internal @string name;
    internal sync.Once once;
    internal ж<setting> setting; // Go embeds *setting (departure 1 in the header)
}

[GoType] partial struct setting {
    internal atomic.Pointer<value> value;
    internal sync.Once nonDefaultOnce;
    internal atomic.Uint64 nonDefault;
    internal ж<godebugs.Info> info;
}

[GoType] partial struct value {
    internal @string text;
    internal ж<bisect.Matcher> bisect;
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
//
// Note that almost all settings should arrange to call [IncNonDefault] precisely
// when program behavior is changing from the default due to the setting
// (not just when the setting is different, but when program behavior changes).
// See the [internal/godebug] package comment for more.
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
// Note that Value must be called at least once before IncNonDefault.
public static void IncNonDefault(this ж<Setting> Ꮡs) {
    Ꮡs.Value.setting.of(setting.ᏑnonDefaultOnce).Do(Ꮡs.register);
    Ꮡs.Value.setting.of(setting.ᏑnonDefault).Add(1);
}

internal static void register(this ж<Setting> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    if (s.setting.Value.info == nil || s.setting.Value.info.Value.Opaque) {
        throw panic("godebug: unexpected IncNonDefault of " + s.name);
    }
    registerMetric("/godebug/non-default-behavior/"u8 + s.Name() + ":events"u8, s.setting.of(setting.ᏑnonDefault).Load);
}

// cache is a cache of all the GODEBUG settings,
// a locked map[string]*atomic.Pointer[string].
//
// All Settings with the same name share a single
// *atomic.Pointer[string], so that when GODEBUG
// changes only that single atomic string pointer
// needs to be updated.
//
// A name appears in the values map either if it is the
// name of a Setting for which Value has been called
// at least once, or if the name has ever appeared in
// a name=value pair in the $GODEBUG environment variable.
// Once entered into the map, the name is never removed.
internal static ж<sync.Map> Ꮡcache = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map cache => ref Ꮡcache.Value; // name string -> value *atomic.Pointer[string]

internal static ж<value> Ꮡempty = new StandardBox<value>(default(value));
internal static ref value empty => ref Ꮡempty.Value;

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
    Ꮡs.of(Setting.Ꮡonce).Do(() => {
        Ꮡs.Value.setting = lookup(Ꮡs.Value.Name());
        if (Ꮡs.Value.setting.Value.info == nil && !Ꮡs.Value.Undocumented()) {
            throw panic("godebug: Value of name not listed in godebugs.All: " + Ꮡs.Value.name);
        }
    });
    var v = Ꮡs.Value.setting.of(setting.Ꮡvalue).Load().Value;
    if (v.bisect != nil && !v.bisect.Stack(s_bisectStderr)) {
        return ""u8;
    }
    return v.text;
}

// lookup returns the unique *setting value for the given name.
internal static ж<setting> lookup(@string name) {
    {
        var (v, ok) = Ꮡcache.Load(name); if (ok) {
            return v._<ж<setting>>();
        }
    }
    var s = @new<setting>();
    s.Value.info = godebugs.Lookup(name);
    s.of(setting.Ꮡvalue).Store(Ꮡempty);
    {
        var (v, loaded) = Ꮡcache.LoadOrStore(name, s); if (loaded) {
            // Lost race: someone else created it. Use theirs.
            return v._<ж<setting>>();
        }
    }

    return s;
}

// setUpdate is provided by package runtime.
// It calls update(def, env), where def is the default GODEBUG setting
// and env is the current value of the $GODEBUG environment variable.
// After that first call, the runtime calls update(def, env)
// again each time the environment variable changes
// (due to use of os.Setenv, for example).
internal static void setUpdate(Action<@string, @string> update) => global::go.runtime_package.godebugSetUpdate(update);

// registerMetric is provided by package runtime.
// It forwards registrations to runtime/metrics.
internal static void registerMetric(@string name, Func<uint64> read) => global::go.runtime_package.godebugRegisterMetric(name, read);

// setNewIncNonDefault is provided by package runtime.
// The runtime can do
//
//	inc := newNonDefaultInc(name)
//
// instead of
//
//	inc := godebug.New(name).IncNonDefault
//
// since it cannot import godebug.
internal static void setNewIncNonDefault(Func<@string, Action> newIncNonDefault) => global::go.runtime_package.godebugSetNewIncNonDefault(newIncNonDefault);

[ModuleInitializer]
internal static void init() {
    setUpdate(update);
    setNewIncNonDefault(newIncNonDefault);
}

internal static Action newIncNonDefault(@string name) {
    var s = New(name);
    s.Value();
    return s.IncNonDefault;
}

internal static ж<sync.Mutex> ᏑupdateMu = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex updateMu => ref ᏑupdateMu.Value;

// update records an updated GODEBUG setting.
// def is the default GODEBUG setting for the running binary,
// and env is the current value of the $GODEBUG environment variable.
internal static void update(@string def, @string env) {
    GoFrame ᒐ = default;
    try {
        ᏑupdateMu.Lock();
        defer(ᏑupdateMu.Unlock, ref ᒐ);

        // Update all the cached values, creating new ones as needed.
        // We parse the environment variable first, so that any settings it has
        // are already locked in place (did[name] = true) before we consider
        // the defaults. Existing immutable settings are always locked.
        var did = new map<@string, bool>();
        Ꮡcache.Range((any name, any s) => {
            {
                var info = s._<ж<setting>>().Value.info; if (info != nil && info.Value.Immutable) {
                    did[name._<@string>()] = true;
                }
            }
            return true;
        });
        parse(did, env);
        parse(did, def);

        // Clear any cached values that are no longer present.
        Ꮡcache.Range((any name, any s) => {
            if (!did[name._<@string>()]) {
                s._<ж<setting>>().of(setting.Ꮡvalue).Store(Ꮡempty);
            }
            return true;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// parse parses the GODEBUG setting string s,
// which has the form k=v,k2=v2,k3=v3.
// Later settings override earlier ones.
// Parse only updates settings k=v for which did[k] = false.
// It also sets did[k] = true for settings that it updates.
// Each value v can also have the form v#pattern,
// in which case the GODEBUG is only enabled for call stacks
// matching pattern, for use with golang.org/x/tools/cmd/bisect.
internal static void parse(map<@string, bool> did, @string s) {
    // Scan the string backward so that later settings are used
    // and earlier settings are ignored.
    // Note that a forward scan would cause cached values
    // to temporarily use the ignored value before being
    // updated to the "correct" one.
    nint end = len(s);
    nint eq = -1;
    for (nint i = end - 1; i >= -1; i--) {
        if (i == -1 || s[i] == (rune)',') {
            if (eq >= 0) {
                @string name = s.slice(i + 1, eq);
                @string arg = s.slice(eq + 1, end);
                if (!did[name]) {
                    did[name] = true;
                    var v = Ꮡ(new value(text: arg));
                    for (nint j = 0; j < len(arg); j++) {
                        if (arg[j] == (rune)'#') {
                            v.Value.text = arg.slice(0, j);
                            (v.Value.bisect, _) = bisect.New(arg.slice(j + 1));
                            break;
                        }
                    }
                    lookup(name).of(setting.Ꮡvalue).Store(v);
                }
            }
            eq = -1;
            end = i;
        } else if (s[i] == (rune)'=') {
            eq = i;
        }
    }
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
