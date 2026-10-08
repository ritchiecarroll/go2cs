// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implements methods to remove frames from profiles.
namespace go.@internal;

using fmt = fmt_package;
using regexp = regexp_package;

partial class profile_package {

// Prune removes all nodes beneath a node matching dropRx, and not
// matching keepRx. If the root node of a Sample matches, the sample
// will have an empty stack.
public static void Prune(this ref Profile p, ж<regexp.Regexp> ᏑdropRx, ж<regexp.Regexp> ᏑkeepRx) {
    var prune = new map<uint64, bool>();
    var pruneBeneath = new map<uint64, bool>();
    foreach (var (_, loc) in p.Location) {
        nint i = default!;
        for (i = len((~loc).Line) - 1; i >= 0; i--) {
            {
                var fn = (~loc).Line[i].Function; if (fn != nil && (~fn).Name != ""u8) {
                    @string funcName = fn.Value.Name;
                    // Account for leading '.' on the PPC ELF v1 ABI.
                    if (funcName[0] == (rune)'.') {
                        funcName = funcName[1..];
                    }
                    if (ᏑdropRx.MatchString(funcName)) {
                        if (ᏑkeepRx == nil || !ᏑkeepRx.MatchString(funcName)) {
                            break;
                        }
                    }
                }
            }
        }
        if (i >= 0) {
            // Found matching entry to prune.
            pruneBeneath[(~loc).ID] = true;
            // Remove the matching location.
            if (i == len((~loc).Line) - 1){
                // Matched the top entry: prune the whole location.
                prune[(~loc).ID] = true;
            } else {
                loc.Value.Line = (~loc).Line.slice(i + 1);
            }
        }
    }
    // Prune locs from each Sample
    foreach (var (_, sample) in p.Sample) {
        // Scan from the root to the leaves to find the prune location.
        // Do not prune frames before the first user frame, to avoid
        // pruning everything.
        var foundUser = false;
        for (nint i = len((~sample).Location) - 1; i >= 0; i--) {
            var id = (~sample).Location[i].Value.ID;
            if (!prune[id] && !pruneBeneath[id]) {
                foundUser = true;
                continue;
            }
            if (!foundUser) {
                continue;
            }
            if (prune[id]) {
                sample.Value.Location = (~sample).Location.slice(i + 1);
                break;
            }
            if (pruneBeneath[id]) {
                sample.Value.Location = (~sample).Location.slice(i);
                break;
            }
        }
    }
}

// RemoveUninteresting prunes and elides profiles using built-in
// tables of uninteresting function names.
public static error RemoveUninteresting(this ref Profile p) {
    ж<regexp.Regexp> keep = default!;
    ж<regexp.Regexp> drop = default!;
    error err = default!;
    if (p.DropFrames != ""u8) {
        {
            (drop, err) = regexp.Compile("^("u8 + p.DropFrames + ")$"u8); if (err != default!) {
                return fmt.Errorf("failed to compile regexp %s: %v"u8, p.DropFrames, err);
            }
        }
        if (p.KeepFrames != ""u8) {
            {
                (keep, err) = regexp.Compile("^("u8 + p.KeepFrames + ")$"u8); if (err != default!) {
                    return fmt.Errorf("failed to compile regexp %s: %v"u8, p.KeepFrames, err);
                }
            }
        }
        p.Prune(drop, keep);
    }
    return default!;
}

} // end profile_package
