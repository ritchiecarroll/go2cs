// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rtcov = @internal.coverage.rtcov_package;
using @unsafe = unsafe_package;
using @internal.coverage;

partial class runtime_package {

//go:linkname coverage_getCovCounterList internal/coverage/cfile.getCovCounterList
internal static slice<rtcov.CovCounterBlob> coverage_getCovCounterList() {
    var resᴷ = new stackAppend<rtcov.CovCounterBlob>(moves: true);

    var res = slice<rtcov.CovCounterBlob>([]);
    var u32sz = /* unsafe.Sizeof(uint32(0)) */ (uintptr)4;
    for (var datap = Ꮡfirstmoduledata; datap != nil; datap = datap.next) {
        if (datap.covctrs == datap.ecovctrs) {
            continue;
        }
        res = append(resᴷ, res, new rtcov.CovCounterBlob(
            Counters: (ж<uint32>)(uintptr)((@unsafe.Pointer)datap.covctrs),
            Len: (uint64)((datap.ecovctrs - datap.covctrs) / u32sz)
        ));
    }
    res = move2heap(resᴷ, res);
    return res;
}

} // end runtime_package
