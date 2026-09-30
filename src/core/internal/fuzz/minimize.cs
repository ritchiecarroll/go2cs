// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using reflect = reflect_package;

partial class fuzz_package {

internal static bool isMinimizable(reflectꓸType t) {
    return AreEqual(t, reflect.TypeOf((@string)""u8)) || AreEqual(t, reflect.TypeOf(slice<byte>(default!)));
}

internal static void minimizeBytes(slice<byte> v, Func<slice<byte>, bool> @try, Func<bool> shouldStop) {
    GoFrame ᒐ = default;
    try {
        var tmp = new slice<byte>(len(v));
        // If minimization was successful at any point during minimizeBytes,
        // then the vals slice in (*workerServer).minimizeInput will point to
        // tmp. Since tmp is altered while making new candidates, we need to
        // make sure that it is equal to the correct value, v, before exiting
        // this function.
        defer((ᴛ1, ᴛ2) => copy(ᴛ1, ᴛ2), tmp, v, ref ᒐ);
        // First, try to cut the tail.
        for (nint n = 1024; n != 0; n /= 2) {
            while (len(v) > n) {
                if (shouldStop()) {
                    return;
                }
                var candidate = v.slice(0, len(v) - n);
                if (!@try(candidate)) {
                    break;
                }
                // Set v to the new value to continue iterating.
                v = candidate;
            }
        }
        // Then, try to remove each individual byte.
        for (nint i = 0; i < len(v) - 1; i++) {
            if (shouldStop()) {
                return;
            }
            var candidate = tmp.slice(0, len(v) - 1);
            copy(candidate.slice(0, i), v.slice(0, i));
            copy(candidate.slice(i), v.slice(i + 1));
            if (!@try(candidate)) {
                continue;
            }
            // Update v to delete the value at index i.
            copy(v.slice(i), v.slice(i + 1));
            v = v.slice(0, len(candidate));
            // v[i] is now different, so decrement i to redo this iteration
            // of the loop with the new value.
            i--;
        }
        // Then, try to remove each possible subset of bytes.
        for (nint i = 0; i < len(v) - 1; i++) {
            copy(tmp, v.slice(0, i));
            for (nint j = len(v); j > i + 1; j--) {
                if (shouldStop()) {
                    return;
                }
                var candidate = tmp.slice(0, len(v) - j + i);
                copy(candidate.slice(i), v.slice(j));
                if (!@try(candidate)) {
                    continue;
                }
                // Update v and reset the loop with the new length.
                copy(v.slice(i), v.slice(j));
                v = v.slice(0, len(candidate));
                j = len(v);
            }
        }
        // Then, try to make it more simplified and human-readable by trying to replace each
        // byte with a printable character.
        var printableChars = slice<byte>("012789ABCXYZabcxyz !\"#$%&'()*+,."u8);
        foreach (var (i, b) in v) {
            if (shouldStop()) {
                return;
            }
            foreach (var (_, pc) in printableChars) {
                v[i] = pc;
                if (@try(v)) {
                    // Successful. Move on to the next byte in v.
                    break;
                }
                // Unsuccessful. Revert v[i] back to original value.
                v[i] = b;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fuzz_package
