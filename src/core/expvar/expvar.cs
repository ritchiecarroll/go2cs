// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package expvar provides a standardized interface to public variables, such
// as operation counters in servers. It exposes these variables via HTTP at
// /debug/vars in JSON format. As of Go 1.22, the /debug/vars request must
// use GET.
//
// Operations to set or modify these public variables are atomic.
//
// In addition to adding the HTTP handler, this package registers the
// following variables:
//
//	cmdline   os.Args
//	memstats  runtime.Memstats
//
// The package is sometimes only imported for the side effect of
// registering its HTTP handler and the above variables. To use it
// this way, link this package into your program:
//
//	import _ "expvar"
namespace go;

using json = encoding.json_package;
using godebug = @internal.godebug_package;
using Δlog = log_package;
using Δmath = math_package;
using http = net.http_package;
using os = os_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using utf8 = unicode.utf8_package;
using @internal;
using encoding;
using go.sync;
using net;
using unicode;

partial class expvar_package {

// Var is an abstract type for all exported variables.
partial interface Var {
    // String returns a valid JSON value for the variable.
    // Types with String methods that do not return valid JSON
    // (such as time.Time) must not be used as a Var.
    @string String();
}

partial interface jsonVar {
    // appendJSON appends the JSON representation of the receiver to b.
    slice<byte> appendJSON(slice<byte> b);
}

// Int is a 64-bit integer variable that satisfies the [Var] interface.
partial struct Int {
    internal atomic.Int64 i;
}

public static int64 Value(this ж<Int> Ꮡv) {
    return Ꮡv.of(Int.Ꮡi).Load();
}

public static @string String(this ж<Int> Ꮡv) {
    return ((@string)Ꮡv.appendJSON(default!));
}

internal static slice<byte> appendJSON(this ж<Int> Ꮡv, slice<byte> b) {
    return strconv.AppendInt(b, Ꮡv.of(Int.Ꮡi).Load(), 10);
}

public static void Add(this ж<Int> Ꮡv, int64 delta) {
    Ꮡv.of(Int.Ꮡi).Add(delta);
}

public static void Set(this ж<Int> Ꮡv, int64 value) {
    Ꮡv.of(Int.Ꮡi).Store(value);
}

// Float is a 64-bit float variable that satisfies the [Var] interface.
partial struct Float {
    internal atomic.Uint64 f;
}

public static float64 Value(this ж<Float> Ꮡv) {
    return Δmath.Float64frombits(Ꮡv.of(Float.Ꮡf).Load());
}

public static @string String(this ж<Float> Ꮡv) {
    return ((@string)Ꮡv.appendJSON(default!));
}

internal static slice<byte> appendJSON(this ж<Float> Ꮡv, slice<byte> b) {
    return strconv.AppendFloat(b, Δmath.Float64frombits(Ꮡv.of(Float.Ꮡf).Load()), (rune)'g', -1, 64);
}

// Add adds delta to v.
public static void Add(this ж<Float> Ꮡv, float64 delta) {
    while (ᐧ) {
        var cur = Ꮡv.of(Float.Ꮡf).Load();
        var curVal = Δmath.Float64frombits(cur);
        var nxtVal = curVal + delta;
        var nxt = Δmath.Float64bits(nxtVal);
        if (Ꮡv.of(Float.Ꮡf).CompareAndSwap(cur, nxt)) {
            return;
        }
    }
}

// Set sets v to value.
public static void Set(this ж<Float> Ꮡv, float64 value) {
    Ꮡv.of(Float.Ꮡf).Store(Δmath.Float64bits(value));
}

// Map is a string-to-Var map variable that satisfies the [Var] interface.
partial struct Map {
    internal Δsync.Map m; // map[string]Var
    internal Δsync.RWMutex keysMu;
    internal slice<@string> keys; // sorted
}

// KeyValue represents a single entry in a [Map].
partial struct KeyValue {
    public @string Key;
    public Var Value;
}

public static @string String(this ж<Map> Ꮡv) {
    return ((@string)Ꮡv.appendJSON(default!));
}

internal static slice<byte> appendJSON(this ж<Map> Ꮡv, slice<byte> b) {
    return Ꮡv.appendJSONMayExpand(b, false);
}

internal static slice<byte> appendJSONMayExpand(this ж<Map> Ꮡv, slice<byte> b, bool expand) {
    var afterCommaDelim = (byte)(rune)' ';
    var mayAppendNewline = (slice<byte> bΔ1) => bΔ1;
    if (expand) {
        afterCommaDelim = (rune)'\n';
        mayAppendNewline = (slice<byte> bΔ2) => append(bΔ2, (byte)((rune)'\n'));
    }
    b = append(b, (byte)((rune)'{'));
    b = mayAppendNewline(b);
    var first = true;
    Ꮡv.Do((KeyValue kv) => {
        if (!first) {
            b = append(b, (byte)((rune)','), afterCommaDelim);
        }
        first = false;
        b = appendJSONQuote(b, kv.Key);
        b = append(b, (byte)((rune)':'), (byte)((rune)' '));
        switch (kv.Value.type()) {
        case null: {
            b = append(b, ((@string)"null"u8).ꓸꓸꓸ);
            break;
        }
        case {} ΔvΔ1 when ΔvΔ1._<jsonVar>(out var vΔ1): {
            b = vΔ1.appendJSON(b);
            break;
        }
        default: {
            var vΔ1 = kv.Value;
            b = append(b, vΔ1.String().ꓸꓸꓸ);
            break;
        }}
    });
    b = mayAppendNewline(b);
    b = append(b, (byte)((rune)'}'));
    b = mayAppendNewline(b);
    return b;
}

// Init removes all keys from the map.
public static ж<Map> Init(this ж<Map> Ꮡv) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var v = ref Ꮡv.DerefOrNull();

        Ꮡv.of(Map.ᏑkeysMu).Lock();
        ᒐd1 = true;
        v.keys = v.keys[..0];
        Ꮡv.of(Map.Ꮡm).Clear();
        return Ꮡv;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡv.of(Map.ᏑkeysMu).Unlock(); ᒐ.Run(); }
}

// addKey updates the sorted list of keys in v.keys.
internal static void addKey(this ж<Map> Ꮡv, @string key) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var v = ref Ꮡv.DerefOrNull();

        Ꮡv.of(Map.ᏑkeysMu).Lock();
        ᒐd1 = true;
        // Using insertion sort to place key into the already-sorted v.keys.
        var (i, found) = slices.BinarySearch(v.keys, key);
        if (found) {
            return;
        }
        v.keys = slices.Insert(v.keys, i, key);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡv.of(Map.ᏑkeysMu).Unlock(); ᒐ.Run(); }
}

public static Var Get(this ж<Map> Ꮡv, @string key) {
    var (i, _) = Ꮡv.of(Map.Ꮡm).Load(key);
    var (av, _) = i._<Var>(ᐧ);
    return av;
}

public static void Set(this ж<Map> Ꮡv, @string key, Var av) {
    // Before we store the value, check to see whether the key is new. Try a Load
    // before LoadOrStore: LoadOrStore causes the key interface to escape even on
    // the Load path.
    {
        var (_, ok) = Ꮡv.of(Map.Ꮡm).Load(key); if (!ok) {
            {
                var (_, dup) = Ꮡv.of(Map.Ꮡm).LoadOrStore(key, av); if (!dup) {
                    Ꮡv.addKey(key);
                    return;
                }
            }
        }
    }
    Ꮡv.of(Map.Ꮡm).Store(key, av);
}

// Add adds delta to the *[Int] value stored under the given map key.
public static void Add(this ж<Map> Ꮡv, @string key, int64 delta) {
    var (i, ok) = Ꮡv.of(Map.Ꮡm).Load(key);
    if (!ok) {
        bool dup = default!;
        (i, dup) = Ꮡv.of(Map.Ꮡm).LoadOrStore(key, @new<Int>());
        if (!dup) {
            Ꮡv.addKey(key);
        }
    }
    // Add to Int; ignore otherwise.
    {
        var (iv, okΔ1) = i._<ж<Int>>(ᐧ); if (okΔ1) {
            iv.Add(delta);
        }
    }
}

// AddFloat adds delta to the *[Float] value stored under the given map key.
public static void AddFloat(this ж<Map> Ꮡv, @string key, float64 delta) {
    var (i, ok) = Ꮡv.of(Map.Ꮡm).Load(key);
    if (!ok) {
        bool dup = default!;
        (i, dup) = Ꮡv.of(Map.Ꮡm).LoadOrStore(key, @new<Float>());
        if (!dup) {
            Ꮡv.addKey(key);
        }
    }
    // Add to Float; ignore otherwise.
    {
        var (iv, okΔ1) = i._<ж<Float>>(ᐧ); if (okΔ1) {
            iv.Add(delta);
        }
    }
}

// Delete deletes the given key from the map.
public static void Delete(this ж<Map> Ꮡv, @string key) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var v = ref Ꮡv.DerefOrNull();

        Ꮡv.of(Map.ᏑkeysMu).Lock();
        ᒐd1 = true;
        var (i, found) = slices.BinarySearch(v.keys, key);
        if (found) {
            v.keys = slices.Delete<slice<@string>, @string>(v.keys, i, i + 1);
            Ꮡv.of(Map.Ꮡm).Delete(key);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡv.of(Map.ᏑkeysMu).Unlock(); ᒐ.Run(); }
}

// Do calls f for each entry in the map.
// The map is locked during the iteration,
// but existing entries may be concurrently updated.
public static void Do(this ж<Map> Ꮡv, Action<KeyValue> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var v = ref Ꮡv.DerefOrNull();

        Ꮡv.of(Map.ᏑkeysMu).RLock();
        ᒐd1 = true;
        foreach (var (_, k) in v.keys) {
            var (i, _) = Ꮡv.of(Map.Ꮡm).Load(k);
            var (val, _) = i._<Var>(ᐧ);
            f(new KeyValue(k, val));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡv.of(Map.ᏑkeysMu).RUnlock(); ᒐ.Run(); }
}

// String is a string variable, and satisfies the [Var] interface.
partial struct ΔString {
    internal atomic.Value s; // string
}

public static @string Value(this ж<ΔString> Ꮡv) {
    var (p, _) = Ꮡv.of(expvar_package.ΔString.Ꮡs).Load()._<@string>(ᐧ);
    return p;
}

// String implements the [Var] interface. To get the unquoted string
// use [String.Value].
public static @string String(this ж<ΔString> Ꮡv) {
    return ((@string)Ꮡv.appendJSON(default!));
}

internal static slice<byte> appendJSON(this ж<ΔString> Ꮡv, slice<byte> b) {
    return appendJSONQuote(b, Ꮡv.Value());
}

public static void Set(this ж<ΔString> Ꮡv, @string value) {
    Ꮡv.of(expvar_package.ΔString.Ꮡs).Store(value);
}

public delegate any Func();

public static any Value(this Func f) {
    return f();
}

public static @string String(this Func f) {
    var (v, _) = json.Marshal(f());
    return ((@string)v);
}

// All published variables.
internal static ж<Map> Ꮡvars = new StandardBox<Map>(default(Map));
internal static ref Map vars => ref Ꮡvars.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object reuseOfExportedVarNameˢ = (@string)"Reuse of exported var name:"u8;

// Publish declares a named exported variable. This should be called from a
// package's init function when it creates its Vars. If the name is already
// registered then this will log.Panic.
public static void Publish(@string name, Var v) {
    GoFrame ᒐ = default;
    try {
        {
            var (_, dup) = Ꮡvars.of(Map.Ꮡm).LoadOrStore(name, v); if (dup) {
                Δlog.Panicln(reuseOfExportedVarNameˢ, name);
            }
        }
        Ꮡvars.of(Map.ᏑkeysMu).Lock();
        defer(Ꮡvars.of(Map.ᏑkeysMu).Unlock, ref ᒐ);
        vars.keys = append(vars.keys, name);
        slices.Sort<slice<@string>, @string>(vars.keys);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Get retrieves a named exported variable. It returns nil if the name has
// not been registered.
public static Var Get(@string name) {
    return Ꮡvars.Get(name);
}

// Convenience functions for creating new exported variables.
public static ж<Int> NewInt(@string name) {
    var v = @new<Int>();
    Publish(name, new IntжVar(v));
    return v;
}

public static ж<Float> NewFloat(@string name) {
    var v = @new<Float>();
    Publish(name, new FloatжVar(v));
    return v;
}

public static ж<Map> NewMap(@string name) {
    var v = @new<Map>().Init();
    Publish(name, new MapжVar(v));
    return v;
}

public static ж<ΔString> NewString(@string name) {
    var v = @new<ΔString>();
    Publish(name, new ΔStringжVar(v));
    return v;
}

// Do calls f for each exported variable.
// The global variable map is locked during the iteration,
// but existing entries may be concurrently updated.
public static void Do(Action<KeyValue> f) {
    Ꮡvars.Do(f);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeˢ = "Content-Type"u8;
internal static readonly @string applicationJsonCharsetˢ = "application/json; charset=utf-8"u8;

internal static void expvarHandler(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    w.Header().Set(contentTypeˢ, applicationJsonCharsetˢ);
    w.Write(Ꮡvars.appendJSONMayExpand(default!, true));
}

// Handler returns the expvar HTTP Handler.
//
// This is only needed to install the handler in a non-standard location.
public static httpꓸHandler Handler() {
    return new http_HandlerFuncᴠΔHandler(new http.HandlerFunc(expvarHandler));
}

internal static any cmdline() {
    return os.Args;
}

internal static any memstats() {
    var stats = @new<runtime.MemStats>();
    runtime.ReadMemStats(stats);
    return stats.Value.ΔClone();
}

[GoInit] internal static void init() {
    if (godebug.New("httpmuxgo121"u8).Value() == "1"u8){
        http.HandleFunc("/debug/vars"u8, expvarHandler);
    } else {
        http.HandleFunc("GET /debug/vars"u8, expvarHandler);
    }
    Publish("cmdline"u8, new FuncᴠVar(new Func(cmdline)));
    Publish("memstats"u8, new FuncᴠVar(new Func(memstats)));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hexᶜ = "0123456789abcdef"u8;

// TODO: Use json.appendString instead.
internal static slice<byte> appendJSONQuote(slice<byte> b, @string s) {
    @string hex = hexᶜ;
    b = append(b, (byte)((rune)'"'));
    foreach (var (_, r) in s) {
        switch (ᐧ) {
        case {} when r < (rune)' ' || r == (rune)'\\' || r == (rune)'"' || r == (rune)'<' || r == (rune)'>' || r == (rune)'&' || r == (rune)'\u2028' || r == (rune)'\u2029': {
            switch (r) {
            case (rune)'\\' or (rune)'"': {
                b = append(b, (byte)((rune)'\\'), (byte)r);
                break;
            }
            case (rune)'\n': {
                b = append(b, (byte)((rune)'\\'), (byte)((rune)'n'));
                break;
            }
            case (rune)'\r': {
                b = append(b, (byte)((rune)'\\'), (byte)((rune)'r'));
                break;
            }
            case (rune)'\t': {
                b = append(b, (byte)((rune)'\\'), (byte)((rune)'t'));
                break;
            }
            default: {
                b = append(b, (byte)((rune)'\\'), (byte)((rune)'u'), hex[(rune)(((r >> (int)(12))) & 0xf)], hex[(rune)(((r >> (int)(8))) & 0xf)], hex[(rune)(((r >> (int)(4))) & 0xf)], hex[(rune)(((r >> (int)(0))) & 0xf)]);
                break;
            }}

            break;
        }
        case {} when r < utf8.RuneSelf: {
            b = append(b, (byte)r);
            break;
        }
        default: {
            b = utf8.AppendRune(b, r);
            break;
        }}

    }
    b = append(b, (byte)((rune)'"'));
    return b;
}

} // end expvar_package
