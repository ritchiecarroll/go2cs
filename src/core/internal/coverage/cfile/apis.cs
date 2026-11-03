// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using rtcov = go.@internal.coverage.rtcov_package;
using io = io_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.coverage;
using sync;

partial class cfile_package {

// WriteMetaDir implements [runtime/coverage.WriteMetaDir].
public static error WriteMetaDir(@string dir) {
    if (!finalHashComputed) {
        return fmt.Errorf("error: no meta-data available (binary not built with -cover?)"u8);
    }
    return emitMetaDataToDirectory(dir, rtcov.Meta.List);
}

// WriteMeta implements [runtime/coverage.WriteMeta].
public static error WriteMeta(io.Writer w) {
    if (w == nil) {
        return fmt.Errorf("error: nil writer in WriteMeta"u8);
    }
    if (!finalHashComputed) {
        return fmt.Errorf("error: no meta-data available (binary not built with -cover?)"u8);
    }
    var ml = rtcov.Meta.List;
    return writeMetaData(w, ml, cmode, cgran, finalHash);
}

// WriteCountersDir implements [runtime/coverage.WriteCountersDir].
public static error WriteCountersDir(@string dir) {
    if (cmode != coverage.CtrModeAtomic) {
        return fmt.Errorf("WriteCountersDir invoked for program built with -covermode=%s (please use -covermode=atomic)"u8, cmode.String());
    }
    return emitCounterDataToDirectory(dir);
}

// WriteCounters implements [runtime/coverage.WriteCounters].
public static error WriteCounters(io.Writer w) {
    if (w == nil) {
        return fmt.Errorf("error: nil writer in WriteCounters"u8);
    }
    if (cmode != coverage.CtrModeAtomic) {
        return fmt.Errorf("WriteCounters invoked for program built with -covermode=%s (please use -covermode=atomic)"u8, cmode.String());
    }
    // Ask the runtime for the list of coverage counter symbols.
    var cl = getCovCounterList();
    if (len(cl) == 0) {
        return fmt.Errorf("program not built with -cover"u8);
    }
    if (!finalHashComputed) {
        return fmt.Errorf("meta-data not written yet, unable to write counter data"u8);
    }

    var pm = rtcov.Meta.PkgMap;
    var s = Ꮡ(new emitState(
        counterlist: cl,
        pkgmap: pm
    ));
    return s.emitCounterDataToWriter(w);
}

// ClearCounters implements [runtime/coverage.ClearCounters].
public static error ClearCounters() {
    var cl = getCovCounterList();
    if (len(cl) == 0) {
        return fmt.Errorf("program not built with -cover"u8);
    }
    if (cmode != coverage.CtrModeAtomic) {
        return fmt.Errorf("ClearCounters invoked for program built with -covermode=%s (please use -covermode=atomic)"u8, cmode.String());
    }

    // Implementation note: this function would be faster and simpler
    // if we could just zero out the entire counter array, but for the
    // moment we go through and zero out just the slots in the array
    // corresponding to the counter values. We do this to avoid the
    // following bad scenario: suppose that a user builds their Go
    // program with "-cover", and that program has a function (call it
    // main.XYZ) that invokes ClearCounters:
    //
    //     func XYZ() {
    //       ... do some stuff ...
    //       coverage.ClearCounters()
    //       if someCondition {   <<--- HERE
    //         ...
    //       }
    //     }
    //
    // At the point where ClearCounters executes, main.XYZ has not yet
    // finished running, thus as soon as the call returns the line
    // marked "HERE" above will trigger the writing of a non-zero
    // value into main.XYZ's counter slab. However since we've just
    // finished clearing the entire counter segment, we will have lost
    // the values in the prolog portion of main.XYZ's counter slab
    // (nctrs, pkgid, funcid). This means that later on at the end of
    // program execution as we walk through the entire counter array
    // for the program looking for executed functions, we'll zoom past
    // main.XYZ's prolog (which was zero'd) and hit the non-zero
    // counter value corresponding to the "HERE" block, which will
    // then be interpreted as the start of another live function.
    // Things will go downhill from there.
    //
    // This same scenario is also a potential risk if the program is
    // running on an architecture that permits reordering of
    // writes/stores, since the inconsistency described above could
    // arise here. Example scenario:
    //
    //     func ABC() {
    //       ...                    // prolog
    //       if alwaysTrue() {
    //         XYZ()                // counter update here
    //       }
    //     }
    //
    // In the instrumented version of ABC, the prolog of the function
    // will contain a series of stores to the initial portion of the
    // counter array to write number-of-counters, pkgid, funcid. Later
    // in the function there is also a store to increment a counter
    // for the block containing the call to XYZ(). If the CPU is
    // allowed to reorder stores and decides to issue the XYZ store
    // before the prolog stores, this could be observable as an
    // inconsistency similar to the one above. Hence the requirement
    // for atomic counter mode: according to package atomic docs,
    // "...operations that happen in a specific order on one thread,
    // will always be observed to happen in exactly that order by
    // another thread". Thus we can be sure that there will be no
    // inconsistency when reading the counter array from the thread
    // running ClearCounters.

    foreach (var (_, c) in cl) {
        var sd = @unsafe.Slice(c.Counters.Reinterpret<uint32, atomic.Uint32>(), (nint)c.Len);
        for (nint i = 0; i < len(sd); i++) {
            // Skip ahead until the next non-zero value.
            var sdi = Ꮡ(sd, i).Load();
            if (sdi == 0) {
                continue;
            }
            // We found a function that was executed; clear its counters.
            var nCtrs = sdi;
            for (nint j = 0; j < (nint)nCtrs; j++) {
                Ꮡ(sd, i + (nint)coverage.FirstCtrOffset + j).Store(0);
            }
            // Move to next function.
            i += (nint)coverage.FirstCtrOffset + (nint)nCtrs - 1;
        }
    }
    return default;
}

} // end cfile_package
