// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && arm64
namespace go.vendor.golang.org.x.sys;

using errors = errors_package;
using io = io_package;
using os = os_package;
using strings = strings_package;

partial class cpu_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string procCpuinfoˢ => ref procCpuinfoˢᵇ;
private static readonly @string procCpuinfoˢᵇ = "/proc/cpuinfo"u8;
private static ref readonly @string noCpuFeaturesFoundˢ => ref noCpuFeaturesFoundˢᵇ;
private static readonly @string noCpuFeaturesFoundˢᵇ = "no CPU features found"u8;
private static ref readonly @string evtstrmˢ => ref evtstrmˢᵇ;
private static readonly @string evtstrmˢᵇ = "evtstrm"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string featuresᶜ => ref featuresᶜᵇ;
private static readonly @string featuresᶜᵇ = "\nFeatures	: "u8;

internal static error readLinuxProcCPUInfo() {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(procCpuinfoˢ);
        if (err != nil) {
            return err;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        array<byte> buf = new(1024); /* (1 << 10) */ // enough for first CPU
        (var n, err) = io.ReadFull(new os_FileжReader(f), buf[..]);
        if (err != nil && !AreEqual(err, io.ErrUnexpectedEOF)) {
            return err;
        }
        @string @in = (@string)(buf.slice(0, n));
        @string features = featuresᶜ;
        nint i = strings.Index(@in, features);
        if (i == -1) {
            return errors.New(noCpuFeaturesFoundˢ);
        }
        @in = @in.slice(i + len(features));
        nint iΔ1 = strings.Index(@in, "\n"); if (iΔ1 != -1) {
            @in = @in.slice(0, iΔ1);
        }
        var m = new map<@string, ж<bool>>{};

        initOptions(); // need it early here; it's harmless to call twice
        foreach (var (_, o) in range(options)) {
            m[o.Name] = o.Feature;
        }
        // The EVTSTRM field has alias "evstrm" in Go, but Linux calls it "evtstrm".
        m[evtstrmˢ] = ᏑARM64.of(ARM64ᴛ1.ᏑHasEVTSTRM);

        foreach (var (_, fΔ1) in range(strings.Fields(@in))) {
            var (p, ok) = m[fΔ1, ꟷ]; if (ok) {
                p.Value = true;
            }
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end cpu_package
