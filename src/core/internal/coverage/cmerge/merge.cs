// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// package cmerge provides a few small utility APIs for helping
// with merging of counter data for a given function.
using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using math = math_package;
using go.@internal;

partial class cmerge_package {

partial struct ModeMergePolicy /*num:uint8*/;

public static ModeMergePolicy ModeMergeStrict => /* iota */ 0;
public static ModeMergePolicy ModeMergeRelaxed => 1;

// Merger provides state and methods to help manage the process of
// merging together coverage counter data for a given function, for
// tools that need to implicitly merge counter as they read multiple
// coverage counter data files.
partial struct Merger {
    internal coverage.CounterMode cmode;
    internal coverage.CounterGranularity cgran;
    internal ModeMergePolicy policy;
    internal bool overflow;
}

public static void SetModeMergePolicy(this ref Merger cm, ModeMergePolicy policy) {
    cm.policy = policy;
}

// MergeCounters takes the counter values in 'src' and merges them
// into 'dst' according to the correct counter mode.
public static (error, bool) MergeCounters(this ref Merger m, slice<uint32> dst, slice<uint32> src) {
    if (len(src) != len(dst)) {
        return (fmt.Errorf("merging counters: len(dst)=%d len(src)=%d"u8, len(dst), len(src)), false);
    }
    if (m.cmode == coverage.CtrModeSet){
        for (nint i = 0; i < len(src); i++) {
            if (src[i] != 0) {
                dst[i] = 1;
            }
        }
    } else {
        for (nint i = 0; i < len(src); i++) {
            dst[i] = m.SaturatingAdd(dst[i], src[i]);
        }
    }
    var ovf = m.overflow;
    m.overflow = false;
    return (default!, ovf);
}

// Saturating add does a saturating addition of 'dst' and 'src',
// returning added value or math.MaxUint32 if there is an overflow.
// Overflows are recorded in case the client needs to track them.
public static uint32 SaturatingAdd(this ref Merger m, uint32 dst, uint32 src) {
    var (result, overflow) = SaturatingAdd(dst, src);
    if (overflow) {
        m.overflow = true;
    }
    return result;
}

// Saturating add does a saturating addition of 'dst' and 'src',
// returning added value or math.MaxUint32 plus an overflow flag.
public static (uint32, bool) SaturatingAdd(uint32 dst, uint32 src) {
    var (d, s) = ((uint64)dst, (uint64)src);
    var sum = d + s;
    var overflow = false;
    if ((uint64)(uint32)sum != sum) {
        overflow = true;
        sum = math.MaxUint32;
    }
    return ((uint32)sum, overflow);
}

// SetModeAndGranularity records the counter mode and granularity for
// the current merge. In the specific case of merging across coverage
// data files from different binaries, where we're combining data from
// more than one meta-data file, we need to check for and resolve
// mode/granularity clashes.
public static error SetModeAndGranularity(this ref Merger cm, @string mdf, coverage.CounterMode cmode, coverage.CounterGranularity cgran) {
    if (cm.cmode == coverage.CtrModeInvalid){
        // Set merger mode based on what we're seeing here.
        cm.cmode = cmode;
        cm.cgran = cgran;
    } else {
        // Granularity clashes are always errors.
        if (cm.cgran != cgran) {
            return fmt.Errorf("counter granularity clash while reading meta-data file %s: previous file had %s, new file has %s"u8, mdf, cm.cgran.String(), cgran.String());
        }
        // Mode clashes are treated as errors if we're using the
        // default strict policy.
        if (cm.cmode != cmode) {
            if (cm.policy == ModeMergeStrict) {
                return fmt.Errorf("counter mode clash while reading meta-data file %s: previous file had %s, new file has %s"u8, mdf, cm.cmode.String(), cmode.String());
            }
            // In the case of a relaxed mode merge policy, upgrade
            // mode if needed.
            if (cm.cmode < cmode) {
                cm.cmode = cmode;
            }
        }
    }
    return default!;
}

public static void ResetModeAndGranularity(this ref Merger cm) {
    cm.cmode = coverage.CtrModeInvalid;
    cm.cgran = coverage.CtrGranularityInvalid;
    cm.overflow = false;
}

public static coverage.CounterMode Mode(this ref Merger cm) {
    return cm.cmode;
}

public static coverage.CounterGranularity Granularity(this ref Merger cm) {
    return cm.cgran;
}

} // end cmerge_package
