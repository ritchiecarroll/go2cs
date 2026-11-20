// Copyright 2022 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using bytes = bytes_package;
using fmt = fmt_package;
using math = math_package;
using debug = go.runtime.debug_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using humanize = github.com.dustin.go_humanize.humanize_package;
using github.com.dustin.go_humanize;
using go.runtime;
using go.sync;
using io = io_package;
using static libc_package;

partial class libc_package {

partial struct PerfCounter {
    internal slice<int32> a;
    internal slice<@string> labels;
    internal bool enabled;
}

public static ж<PerfCounter> NewPerfCounter(slice<@string> labels) {
    return Ꮡ(new PerfCounter(
        a: new slice<int32>(len(labels)),
        labels: labels,
        enabled: true
    ));
}

} partial class libcᴛPerfCounter {

public static void Disable(this ref PerfCounter c) { c.enabled = false; }
public static void Enable(this ref PerfCounter c) { c.enabled = true; }

public static void Clear(this ref PerfCounter c) {
    foreach (var i in range(len(c.a))) {
        c.a[i] = 0;
    }
}

public static void Inc(this ref PerfCounter c, nint n) {
    if (c.enabled) {
        atomic.AddInt32(Ꮡ(c.a, n), 1);
    }
}

public static void IncN(this ref PerfCounter c, nint n, nint m) {
    if (c.enabled) {
        atomic.AddInt32(Ꮡ(c.a, n), (int32)m);
    }
}

public static @string String(this ж<PerfCounter> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var sv = c.enabled;

        defer(() => {
            Ꮡc.Value.enabled = sv;
        }, ref ᒐ);

        c.enabled = false;

        slice<@string> a = default;
        foreach (var (i, v) in range(c.a)) {
            if (v != 0) {
                a = append(a, fmt.Sprintf("%q: %s"u8, c.labels[i], h(v)));
            }
        }
        return fmt.Sprint(a);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} partial class libc_package {

internal static @string h(any v) {
    switch (v.type()) {
    case nint x: {
        return humanize.Comma((int64)x);
    }
    case int32 x: {
        return humanize.Comma((int64)x);
    }
    case int64 x: {
        return humanize.Comma(x);
    }
    case uint32 x: {
        return humanize.Comma((int64)x);
    }
    case uint64 x: {
        if (x <= math.MaxInt64) {
            return humanize.Comma((int64)x);
        }

        return "-"u8 + humanize.Comma(-(int64)x);
    }}
    return fmt.Sprint(v);
}

partial struct StackCapture {
    /*embed*/ public sync_package.Mutex Mutex;
    internal map<@string, nint> m;
    internal nint depth;
    internal bool enabled;
}

public static ж<StackCapture> NewStackCapture(nint depth) {
    return Ꮡ(new StackCapture(
        m: new map<@string, nint>{},
        depth: depth,
        enabled: true
    ));
}

} partial class libcᴛStackCapture {

public static void Disable(this ref StackCapture c) { c.enabled = false; }
public static void Enable(this ref StackCapture c) { c.enabled = true; }

public static void Clear(this ж<StackCapture> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(StackCapture.ᏑMutex).Lock();

        ᒐd1 = true;
        c.m = new map<@string, nint>{};
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡc.of(StackCapture.ᏑMutex).Unlock(); ᒐ.Run(); }
}

} partial class libc_package {

internal static slice<byte> stackCapturePrefix = slice<byte>("\n\t"u8);

} partial class libcᴛStackCapture {

public static partial void Record(this ж<StackCapture> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (!c.enabled) {
            return;
        }

        var b = debug.Stack();
        ref var s = ref heap(new strings.Builder(), out var Ꮡs);
@out:
        for (nint i = 0; len(b) > 0 && i < c.depth + 2; i++) {
            nint l = bytes.Index(b, stackCapturePrefix);
            if (l < 0) {
                goto break_out;
            }

            b = b.slice(l + len(stackCapturePrefix));
            nint h = bytes.IndexByte(b, (byte)'\n');
            if (h < 0) {
                h = len(b);
            }
            if (i > 1) {
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡs), "\n\t%s"u8, b.slice(0, h));
            }
            b = b.slice(h);
continue_out:;
        }
break_out:;
        Ꮡc.of(StackCapture.ᏑMutex).Lock();

        defer(Ꮡc.of(StackCapture.ᏑMutex).Unlock, ref ᒐ);

        c.m[s.String()]++;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static @string String(this ж<StackCapture> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(StackCapture.ᏑMutex).Lock();

        ᒐd1 = true;

        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        slice<@string> a = default;
        foreach (var (k, _) in c.m) {
            a = append(a, k);
        }
        var aʗ1 = a;
        sort.Slice(a, (nint i, nint j) => Ꮡc.Value.m[aʗ1[i]] < Ꮡc.Value.m[aʗ1[j]]);
        foreach (var (_, k) in range(a)) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "%d%s\n"u8, c.m[k], k);
        }
        return b.String();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.of(StackCapture.ᏑMutex).Unlock(); ᒐ.Run(); }
}

} // end libcᴛStackCapture
