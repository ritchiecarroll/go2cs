// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using io = io_package;
using os = os_package;
using strings = strings_package;

partial class sysinfo_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procCpuinfoˢ = "/proc/cpuinfo"u8;

internal static error readLinuxProcCPUInfo(slice<byte> buf) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(procCpuinfoˢ);
        if (err != default!) {
            return err;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (_, err) = io.ReadFull(new os_FileжReader(f), buf);
        if (err != default! && !AreEqual(err, io.ErrUnexpectedEOF)) {
            return err;
        }

        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static @string osCPUInfoName() {
    @string modelName = ""u8;
    @string cpuMHz = ""u8;

    // The 512-byte buffer is enough to hold the contents of CPU0
    var buf = new slice<byte>(512);
    var err = readLinuxProcCPUInfo(buf);
    if (err != default!) {
        return ""u8;
    }

    var scanner = bufio.NewScanner(new bytes_ReaderжReader(bytes.NewReader(buf)));
    while (scanner.Scan()) {
        var (key, value, found) = strings.Cut(scanner.Text(), ": "u8);
        if (!found) {
            continue;
        }
        var exprᴛ1 = strings.TrimSpace(key);
        if (exprᴛ1 == "Model Name"u8 || exprᴛ1 == "model name"u8) {
            modelName = value;
        }
        else if (exprᴛ1 == "CPU MHz"u8 || exprᴛ1 == "cpu MHz"u8) {
            cpuMHz = value;
        }
    }

    if (modelName == ""u8) {
        return ""u8;
    }

    if (cpuMHz == ""u8) {
        return modelName;
    }

    // The modelName field already contains the frequency information,
    // so the cpuMHz field information is not needed.
    // modelName filed example:
    //	Intel(R) Core(TM) i7-10700 CPU @ 2.90GHz
    var f = new @string[]{"GHz"u8, "MHz"u8}.array();
    foreach (var (_, v) in f.ΔRangeSnapshot()) {
        if (strings.Contains(modelName, v)) {
            return modelName;
        }
    }

    return modelName + " @ "u8 + cpuMHz + "MHz"u8;
}

} // end sysinfo_package
