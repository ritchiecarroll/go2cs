// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

using fmt = fmt_package;
using log = log_package;
using bits = math.bits_package;
using strings = strings_package;
using math;

partial class gen_package {

internal const bool traceRegAlloc = true;

partial struct regClass /*num:uint8*/;

internal static regClass regClassFixed => /* iota */ 0;
internal static regClass regClassGP => 1;
internal static regClass regClassZ => 2;
internal static regClass regClassK => 3;

internal static regClass numRegClasses => 4;

internal static regClass regClassNone => /* ^regClass(0) */ 255;

partial struct locReg {
    internal regClass cls;
    internal nint reg;
}

internal static @string LocString(this locReg l) {
    var exprᴛ1 = l.cls;
    if (exprᴛ1 == regClassFixed) {
        return fixedRegs[l.reg];
    }
    if (exprᴛ1 == regClassGP) {
        return gpRegs[l.reg];
    }
    if (exprᴛ1 == regClassZ) {
        return fmt.Sprintf("Z%d"u8, l.reg);
    }
    if (exprᴛ1 == regClassK) {
        return fmt.Sprintf("K%d"u8, l.reg);
    }
    throw panic("bad register class");
}

internal static (loc, error) Deref(this locReg l, nint off) {
    return (new locMem(l, off, ""u8), default!);
}

internal static (locReg, bool) Reg(this locReg l) {
    return (l, true);
}

partial struct locMem {
    internal locReg @base;
    internal nint off;
    internal @string name;
}

internal static @string LocString(this locMem l) {
    if (l.@base.cls == regClassFixed && l.@base.reg == regSB && l.off == 0) {
        return l.name + "(SB)"u8;
    }
    if (l.name != ""u8) {
        return fmt.Sprintf("%s+%d(%s)"u8, l.name, l.off, l.@base.LocString());
    }
    if (l.off != 0) {
        return fmt.Sprintf("%d(%s)"u8, l.off, l.@base.LocString());
    }
    return "("u8 + l.@base.LocString() + ")"u8;
}

internal static (loc, error) Deref(this locMem l, nint off) {
    return (default!, fmt.Errorf("cannot dereference already memory address %s"u8, l.LocString()));
}

internal static (locReg, bool) Reg(this locMem l) {
    if (l.@base.cls == regClassFixed) {
        return (new locReg(nil), false);
    }
    return (l.@base, true);
}

partial interface loc {
    @string LocString();         // Return the assembly syntax for this location
    (loc, error) Deref(nint off); // Treat this location as an address and return a location with the contents of memory at that address
    (locReg, bool) Reg();        // Register used by this location
}

internal static map<@string, nint> opRMW = new map<@string, nint>{
    ["VPERMI2B"u8] = 2, // Overwrites third argument
    ["VPERMI2B.Z"u8] = 3, // Overwrites fourth argument
    ["VPERMI2B.mask"u8] = 3, // Overwrites fourth argument
    ["VPERMT2B"u8] = 1, // Overwrites second argument TODO: Check this. Unused for now.
    ["VPBROADCASTQ.mask"u8] = 2 // Overwrites last argument
};

// TODO: Should we have a general rule that all ".mask" instructions overwrite
// their last argument?
internal static UntypedInt regSB => iota;
internal static UntypedInt regFP => 1;

internal static slice<@string> fixedRegs = new golib.SparseArray<@string>{[regSB] = "SB"u8, [regFP] = "FP"u8}.slice();

internal static slice<@string> gpRegs = new @string[]{"AX"u8, "BX"u8, "CX"u8, "DI"u8, "SI"u8, "R8"u8, "R9"u8, "R10"u8, "R11"u8}.slice(); // ABI argument order

partial struct regSet {
    internal array<uint32> inUse = new(numRegClasses);
}

internal static void used(this ref regSet s, ж<op> Ꮡo, loc l) {
    ref var o = ref Ꮡo.DerefOrNull();

    if (l == default!) {
        return;
    }
    var (reg, ok) = l.Reg();
    if (!ok) {
        return;
    }
    if (traceRegAlloc) {
        log.Printf("  alloc %s @ v%02d"u8, reg.LocString(), o.id);
    }
    if ((uint32)(s.inUse[reg.cls] & (((uint32)1).Lsh((int64)(reg.reg)))) != 0) {
        fatalf("register %s already used"u8, reg.LocString());
    }
    s.inUse[reg.cls] |= (uint32)(((uint32)1).Lsh((int64)(reg.reg)));
}

internal static void free(this ref regSet s, loc l) {
    if (l == default!) {
        return;
    }
    var (reg, ok) = l.Reg();
    if (!ok) {
        return;
    }
    if (traceRegAlloc) {
        log.Printf("  free %s"u8, reg.LocString());
    }
    if ((uint32)(s.inUse[reg.cls] & (((uint32)1).Lsh((int64)(reg.reg)))) == 0) {
        fatalf("register %s is not in use"u8, reg.LocString());
    }
    s.inUse[reg.cls] &= unchecked((uint32)~(uint32)(((uint32)1).Lsh((int64)(reg.reg))));
}

internal static map<ж<op>, loc> assignLocs(this ж<Func> Ꮡfn) {
    ref var fn = ref Ꮡfn.DerefOrNull();

    // Remove static indicator on name, if any. We'll add it back.
    @string nameBase = strings.TrimSuffix(fn.name, "<>"u8);

    // Create map from op -> fn.ops index
    var opIndexes = new map<ж<op>, nint>(len(fn.ops));
    foreach (var (i, o) in fn.ops) {
        opIndexes[o] = i;
    }

    // Read-modify-write operations share a location with one of their inputs.
    // Likewise, deref ops extend the lifetime of their input (but in a shared
    // way, unlike RMW ops).
    //
    // Compute a map from each op to the earliest "canonical" op whose live
    // range we'll use.
    var canon = new map<ж<op>, ж<op>>();
    var overwritten = new map<ж<op>, bool>();
    foreach (var (_, o) in fn.ops) {
        // Check that this op doesn't use any overwritten inputs.
        foreach (var (_, arg) in (~o).args) {
            if (overwritten[arg]) {
                // TODO: The solution to this is to insert copy ops.
                fatalf("op %+v uses overwritten input %+v"u8, o.OrTypedNil(), arg.OrTypedNil());
            }
        }

        // Record canonical op.
        var (rmw, ok) = opRMW[(~o).Δop, ꟷ];
        if (ok){
            canon[o] = canon[(~o).args[rmw]];
            // Record that the input is dead now and must not be referenced.
            overwritten[(~o).args[rmw]] = true;
        } else 
        if ((~o).Δop == "deref"u8){
            canon[o] = canon[(~o).args[0]];
        } else {
            canon[o] = o;
        }
    }

    // Compute live ranges of each canonical op.
    //
    // First, find the last use of each op.
    var lastUses = new map<ж<op>, ж<op>>(); // Canonical creation op -> last use op
    foreach (var (_, op) in fn.ops) {
        foreach (var (_, arg) in (~op).args) {
            lastUses[canon[arg]] = op;
        }
    }
    // Invert the last uses map to get a map from op to the (canonical) values
    // that die at that op.
    var lastUseMap = new map<ж<op>, slice<ж<op>>>(); // op of last use -> (canonical) creation ops
    foreach (var (def, lastUse) in lastUses) {
        lastUseMap[lastUse] = append(lastUseMap[lastUse], def);
    }

    // Prepare for assignments
    var regUsed = new slice<regSet>(len(fn.ops), () => new()); // In-use registers at each op
    foreach (var (i, _) in regUsed) {
        // X15/Y15/Z15 is reserved by the Go ABI
        regUsed[i].inUse[regClassZ] |= (uint32)(((uint32)1 << (int)(15)));
        // K0 is contextual (if used as an opmask, it means no mask). Too
        // complicated, so just ignore it.
        regUsed[i].inUse[regClassK] |= (uint32)(((uint32)1 << (int)(0)));
    }
    var locs = new map<ж<op>, loc>();
    var canonʗ1 = canon;
    var lastUsesʗ1 = lastUses;
    var locsʗ1 = locs;
    var opIndexesʗ1 = opIndexes;
    var regUsedʗ1 = regUsed;
    void assign(ж<op> o, loc l) {
        {
            var (have, ok) = locsʗ1[o, ꟷ]; if (ok) {
                fatalf("op %+v already assigned location %v (new %v)"u8, o.OrTypedNil(), have, l);
                return;
            }
        }
        if (o == canonʗ1[o]) {
            // Mark this location used over o's live range
            for (nint i = opIndexesʗ1[o]; i < opIndexesʗ1[lastUsesʗ1[o]]; i++) {
                regUsedʗ1[i].used(Ꮡfn.Value.ops[i], l);
            }
        }
        locsʗ1[o] = l;
    }

    // Assign fixed locations
    nint id = 0;
    foreach (var (_, o) in fn.ops) {
        var exprᴛ1 = (~o).Δop;
        if (exprᴛ1 == "arg"u8) {
            if (traceRegAlloc) {
                log.Printf("fixed op %+v"u8, o.OrTypedNil());
            }
            assign(o, (~o).c._<locReg>());
        }
        else if (exprᴛ1 == "const"u8) {
            if (traceRegAlloc) {
                log.Printf("fixed op %+v"u8, o.OrTypedNil());
            }
            @string name = o.Value.name;
            if (name == ""u8){
                name = fmt.Sprintf("%s_%d<>"u8, nameBase, id);
                id++;
            } else 
            if (name[0] == (rune)'*') {
                name = nameBase + name[1..];
            }
            assign(o, new locMem(new locReg(cls: regClassFixed, reg: regSB), 0, name));
        }
        else if (exprᴛ1 == "return"u8) {
            if (traceRegAlloc) {
                log.Printf("fixed op %+v"u8, o.OrTypedNil());
            }
            assign(o, default!); // no location
            // TODO: argZ should start at 0.
            nint argGP = 0;
            nint argZ = 1;
            foreach (var (_, arg) in (~o).args) {
                var exprᴛ2 = (~(~arg).kind).reg;
                if (exprᴛ2 == regClassGP) {
                    assign(canon[arg], new locReg(regClassGP, argGP));
                    argGP++;
                }
                else if (exprᴛ2 == regClassZ) {
                    assign(canon[arg], new locReg(regClassZ, argZ));
                    argZ++;
                }
                else { /* default: */
                    fatalf("bad register class for return value"u8);
                }
            }
        }
        else if (exprᴛ1 == "imm"u8) {
            assign(o, default!); // no location
        }
    }

    // Assign locations.
    foreach (var (_, o) in fn.ops) {
        if (traceRegAlloc) {
            log.Printf("assign %+v"u8, o.OrTypedNil());
        }

        {
            var (_, ok) = locs[o, ꟷ]; if (ok) {
                // Already assigned a fixed location above.
                continue;
            }
        }

        if ((~o).Δop == "deref"u8) {
            var (locΔ1, err) = locs[(~o).args[0]].Deref((~o).c._<nint>());
            if (err != default!) {
                fatalf("%v"u8, err);
            }
            // We don't "assign" here because we've already processed the
            // canonical op, which marked loc's register as in-use for the whole
            // live range.
            locs[o] = locΔ1;
            continue;
        }

        if (canon[o] != o) {
            // Copy the canonical op's location.
            locs[o] = locs[canon[o]];
            continue;
        }
        // Below here we know that o is already a canonical op.

        {
            var (_, ok) = opRMW[(~o).Δop, ꟷ]; if (ok) {
                fatalf("read-modify-write op not canonicalized"u8);
            }
        }

        // Find a free register of the right class.
        var cls = o.Value.kind.Value.reg;
        uint32 used = default!;
        for (nint i = opIndexes[o]; i < opIndexes[lastUses[o]]; i++) {
            used |= (uint32)(regUsed[i].inUse[cls]);
        }

        // Assign result location.
        nint num = bits.TrailingZeros32(~used);
        var exprᴛ3 = cls;
        if (exprᴛ3 == regClassGP) {
            if (num >= len(gpRegs)) {
                throw panic("out of GP regs");
            }
        }
        else if (exprᴛ3 == regClassZ) {
            if (num >= 32) {
                throw panic("out of Z regs");
            }
        }
        else if (exprᴛ3 == regClassK) {
            if (num >= 8) {
                throw panic("out of K regs");
            }
        }
        else { /* default: */
            fatalf("unknown reg class %v"u8, cls);
        }
        var loc = new locReg(cls, num);
        assign(o, loc);
    }

    return locs;
}

} // end gen_package
