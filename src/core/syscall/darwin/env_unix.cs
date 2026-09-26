// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || plan9 || wasip1
// Unix environment variables.
namespace go;

using runtime = runtime_package;
using Δsync = sync_package;

partial class syscall_package {

internal static ж<Δsync.Once> ᏑenvOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once envOnce => ref ᏑenvOnce.Value;
internal static ж<Δsync.RWMutex> ᏑenvLock = new StandardBox<Δsync.RWMutex>(default(Δsync.RWMutex));
internal static ref Δsync.RWMutex envLock => ref ᏑenvLock.Value;
internal static map<@string, nint> env;
internal static slice<@string> envs = runtime_envs();

[global::System.Diagnostics.StackTraceHidden] internal static slice<@string> runtime_envs() {
    return runtime.syscall_runtime_envs();
}

// in package runtime
internal static void copyenv() {
    env = new map<@string, nint>();
    foreach (var (i, s) in envs) {
        for (nint j = 0; j < len(s); j++) {
            if (s[j] == (rune)'=') {
                @string key = s[..(int)(j)];
                {
                    var (_, ok) = env[key, ꟷ]; if (!ok){
                        env[key] = i; // first mention of key
                    } else {
                        // Clear duplicate keys. This permits Unsetenv to
                        // safely delete only the first item without
                        // worrying about unshadowing a later one,
                        // which might be a security problem.
                        envs[i] = ""u8;
                    }
                }
                break;
            }
        }
    }
}

public static error Unsetenv(@string key) {
    GoFrame ᒐ = default;
    try {
        ᏑenvOnce.Do(copyenv);
        ᏑenvLock.Lock();
        defer(ᏑenvLock.Unlock, ref ᒐ);
        {
            var (i, ok) = env[key, ꟷ]; if (ok) {
                envs[i] = ""u8;
                delete(env, key);
            }
        }
        runtimeUnsetenv(key);
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static (@string value, bool found) Getenv(@string key) {
    @string value = default!;
    bool found = default!;
    GoFrame ᒐ = default;
    try {
        ᏑenvOnce.Do(copyenv);
        if (len(key) == 0) {
            (value, found) = ("", false); goto ᒐdone;
        }
        ᏑenvLock.RLock();
        defer(ᏑenvLock.RUnlock, ref ᒐ);
        var (i, ok) = env[key, ꟷ];
        if (!ok) {
            (value, found) = ("", false); goto ᒐdone;
        }
        @string s = envs[i];
        for (nint iΔ1 = 0; iΔ1 < len(s); iΔ1++) {
            if (s[iΔ1] == (rune)'=') {
                (value, found) = (s[(int)(iΔ1 + 1)..], true); goto ᒐdone;
            }
        }
        (value, found) = ("", false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (value, found);
}

public static error Setenv(@string key, @string value) {
    GoFrame ᒐ = default;
    try {
        ᏑenvOnce.Do(copyenv);
        if (len(key) == 0) {
            return EINVAL;
        }
        for (nint iΔ1 = 0; iΔ1 < len(key); iΔ1++) {
            if (key[iΔ1] == (rune)'=' || key[iΔ1] == 0) {
                return EINVAL;
            }
        }
        // On Plan 9, null is used as a separator, eg in $path.
        if (runtime.GOOS != "plan9"u8) {
            for (nint iΔ2 = 0; iΔ2 < len(value); iΔ2++) {
                if (value[iΔ2] == 0) {
                    return EINVAL;
                }
            }
        }
        ᏑenvLock.Lock();
        defer(ᏑenvLock.Unlock, ref ᒐ);
        var (i, ok) = env[key, ꟷ];
        @string kv = key + "="u8 + value;
        if (ok){
            envs[i] = kv;
        } else {
            i = len(envs);
            envs = append(envs, kv);
        }
        env[key] = i;
        runtimeSetenv(key, value);
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static void Clearenv() {
    GoFrame ᒐ = default;
    try {
        ᏑenvOnce.Do(copyenv);
        ᏑenvLock.Lock();
        defer(ᏑenvLock.Unlock, ref ᒐ);
        foreach (var (k, _) in env) {
            runtimeUnsetenv(k);
        }
        env = new map<@string, nint>();
        envs = new @string[]{}.slice();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static slice<@string> Environ() {
    GoFrame ᒐ = default;
    try {
        ᏑenvOnce.Do(copyenv);
        ᏑenvLock.RLock();
        defer(ᏑenvLock.RUnlock, ref ᒐ);
        var a = new slice<@string>(0, len(envs));
        foreach (var (_, env) in envs) {
            if (env != ""u8) {
                a = append(a, env);
            }
        }
        return a;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end syscall_package
