// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Proc unit tests. In runtime package so can use runtime guts.
namespace go;

using static global::go.runtime_package;

partial class runtime_internal_test_package {

public static void RunStealOrderTest() {
    global::go.runtime_package.randomOrder ord = default!;
    for (nint procs = 1; procs <= 64; procs++) {
        ord.reset((uint32)procs);
        if (procs >= 3 && len(ord.coprimes) < 2) {
            throw panic("too few coprimes");
        }
        for (nint co = 0; co < len(ord.coprimes); co++) {
            var @enum = ord.start((uint32)co);
            var @checked = new slice<bool>(procs);
            for (nint Δp = 0; Δp < procs; Δp++) {
                var x = @enum.position();
                if (@checked[x]) {
                    println((@string)"procs:"u8, procs, (@string)"inc:"u8, @enum.inc);
                    throw panic("duplicate during enumeration");
                }
                @checked[x] = true;
                @enum.next();
            }
            if (!@enum.done()) {
                throw panic("not done");
            }
        }
    }
    // Make sure that different arguments to ord.start don't generate the
    // same pos+inc twice.
    for (nint procs = 2; procs <= 64; procs++) {
        ord.reset((uint32)procs);
        var @checked = new slice<bool>(procs * procs);
        // We want at least procs*len(ord.coprimes) different pos+inc values
        // before we start repeating.
        for (nint i = 0; i < procs * len(ord.coprimes); i++) {
            var @enum = ord.start((uint32)i);
            var j = @enum.pos * (uint32)procs + @enum.inc;
            if (@checked[j]) {
                println((@string)"procs:"u8, procs, (@string)"pos:"u8, @enum.pos, (@string)"inc:"u8, @enum.inc);
                throw panic("duplicate pos+inc during enumeration");
            }
            @checked[j] = true;
        }
    }
}

} // end runtime_internal_test_package
