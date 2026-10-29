// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

using fmt = fmt_package;
using bits = go.math.bits_package;
using slices = slices_package;
using go.math;
using ꓸꓸꓸnint = Span<nint>;

partial class asmgen_package {

// Note: Exported fields and methods are expected to be used
// by function generators (like the ones in add.go and so on).
// Unexported fields and methods should not be.

// A Pipe manages the input and output data pipelines for a function's
// memory operations.
//
// The input is one or more equal-length slices of words, so collectively
// it can be viewed as a matrix, in which each slice is a row and each column
// is a set of corresponding words from the different slices.
// The output can be viewed the same way, although it is often just one row.
partial struct ΔPipe {
    internal ж<ΔFunc> f;           // function being generated
    internal @string label;        // prefix for loop labels (default "loop")
    internal bool backward;        // processing columns in reverse
    internal bool started;         // Start has been called
    internal bool loaded;          // LoadPtrs has been called
    internal slice<RegPtr> inPtr;  // input slice pointers
    internal slice<Hint> hints;    // for each inPtr, a register hint to use for its data
    internal slice<RegPtr> outPtr; // output slice pointers
    internal ΔReg index;           // index register, if in use
    internal bool useIndexCounter; // index counter requested
    internal nint indexCounter;    // index is also counter (386); 0 no, -1 negative counter, +1 positive counter
    internal nint readOff;         // read offset not yet added to index
    internal nint writeOff;        // write offset not yet added to index
    internal slice<nint> factors;  // unrolling factors
    internal slice<ΔReg> counts;   // iterations for each factor
    internal bool needWrite;       // need a write call during Loop1/LoopN
    internal nint maxColumns;      // maximum columns during unrolled loop
    internal Action unrollStart;   // emit code at start of unrolled body
    internal Action unrollEnd;     // emit code end of unrolled body
}

// Pipe creates and returns a new pipe for use in the function f.
public static ж<ΔPipe> Pipe(this ж<ΔFunc> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    var p = Ꮡ(new ΔPipe(
        f: Ꮡf,
        label: "loop"u8,
        maxColumns: 10000000
    ));
    {
        nint m = a.Arch.maxColumns; if (m != 0) {
            p.maxColumns = m;
        }
    }
    return p;
}

// SetBackward sets the pipe to process the input and output columns in reverse order.
// This is needed for left shifts, which might otherwise overwrite data they will read later.
public static void SetBackward(this ref ΔPipe p) {
    if (p.loaded) {
        p.f.Asm.Fatalf("SetBackward after Start/LoadPtrs"u8);
    }
    p.backward = true;
}

// SetUseIndexCounter sets the pipe to use an index counter if possible,
// meaning the loop counter is also used as an index for accessing the slice data.
// This clever trick is slower on modern processors, but it is still necessary on 386.
// On non-386 systems, SetUseIndexCounter is a no-op.
public static void SetUseIndexCounter(this ref ΔPipe p) {
    if (p.f.Asm.Arch.memIndex == nil) { // need memIndex (only 386 provides it)
        return;
    }
    p.useIndexCounter = true;
}

// SetLabel sets the label prefix for the loops emitted by the pipe.
// The default prefix is "loop".
public static void SetLabel(this ref ΔPipe p, @string label) {
    p.label = label;
}

// SetMaxColumns sets the maximum number of
// columns processed in a single loop body call.
public static void SetMaxColumns(this ref ΔPipe p, nint m) {
    p.maxColumns = m;
}

// SetHint records that the inputs from the named vector
// should be allocated with the given register hint.
//
// If the hint indicates a single register on the target architecture,
// then SetHint calls SetMaxColumns(1), since the hinted register
// can only be used for one value at a time.
public static void SetHint(this ref ΔPipe p, @string name, Hint hint) {
    if (hint == HintMemOK && !p.f.Asm.Arch.memOK) {
        return;
    }
    nint i = slices.Index(p.f.inputs, name);
    if (i < 0) {
        p.f.Asm.Fatalf("unknown input name %s"u8, name);
    }
    if (p.f.Asm.hint(hint) != ""u8) {
        p.SetMaxColumns(1);
    }
    while (len(p.hints) <= i) {
        p.hints = append(p.hints, HintNone);
    }
    p.hints[i] = hint;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runLoopBackwardUsingˢ = "run loop backward, using counter as positive index"u8;
internal static readonly @string useCounterAsNegativeˢ = "use counter as negative index"u8;
internal static readonly @string runLoopBackwardˢ = "run loop backward"u8;

// LoadPtrs loads the slice pointer arguments into registers,
// assuming that the slice length n has already been loaded
// into the register n.
//
// Start will call LoadPtrs if it has not been called already.
// LoadPtrs only needs to be called explicitly when code needs
// to use LoadN before Start, like when the shift.go generators
// read an initial word before the loop.
public static void LoadPtrs(this ref ΔPipe p, ΔReg n) {
    var a = p.f.Asm;
    if (p.loaded) {
        a.Fatalf("pointers already loaded"u8);
    }

    // Load the actual pointers.
    p.loaded = true;
    foreach (var (_, name) in p.f.inputs) {
        p.inPtr = append(p.inPtr, (RegPtr)p.f.Arg(name + "_base"u8));
    }
    foreach (var (_, name) in p.f.outputs) {
        p.outPtr = append(p.outPtr, (RegPtr)p.f.Arg(name + "_base"u8));
    }

    // Decide the memory access strategy for LoadN and StoreN.
    switch (ᐧ) {
    case {} when p.backward && p.useIndexCounter: {
        // Generator wants an index counter, meaning when the iteration counter
        // is AX, we will access the slice with pointer BX using (BX)(AX*WordBytes).
        // The loop is moving backward through the slice, but the counter
        // is also moving backward, so not much to do.
        a.Comment(runLoopBackwardUsingˢ);
        p.indexCounter = +1;
        p.index = n;
        break;
    }
    case {} when !p.backward && p.useIndexCounter: {
        // Generator wants an index counter, but the loop is moving forward.
        // To make the counter move in the direction of data access,
        // we negate the counter, counting up from -len(z) to -1.
        // To make the index access the right words, we add len(z)*WordBytes
        // to each of the pointers.
        // See comment below about the garbage collector (non-)implications
        // of pointing beyond the slice bounds.
        a.Comment(useCounterAsNegativeˢ);
        p.indexCounter = -1;
        p.index = n;
        foreach (var (_, ptr) in p.inPtr) {
            a.AddWords(n, ptr, ptr);
        }
        foreach (var (_, ptr) in p.outPtr) {
            a.AddWords(n, ptr, ptr);
        }
        a.Neg(n, n);
        break;
    }
    case {} when p.backward: {
        // Generator wants to run the loop backward.
        // We'll decrement the pointers before using them,
        // so position them at the very end of the slices.
        // If we had precise pointer information for assembly,
        // these pointers would cause problems with the garbage collector,
        // since they no longer point into the allocated slice,
        // but the garbage collector ignores unexpected values in assembly stacks,
        // and the actual slice pointers are still in the argument stack slots,
        // so the slices won't be collected early.
        // If we switched to the register ABI, we might have to rethink this.
        // (The same thing happens by the end of forward loops,
        // but it's less important since once the pointers go off the slice
        // in a forward loop, the loop is over and the slice won't be accessed anymore.)
        a.Comment(runLoopBackwardˢ);
        foreach (var (_, ptr) in p.inPtr) {
            a.AddWords(n, ptr, ptr);
        }
        foreach (var (_, ptr) in p.outPtr) {
            a.AddWords(n, ptr, ptr);
        }
        break;
    }
    case {} when !p.backward: {
        // Nothing to do!
        break;
    }}
}

// LoadN returns the next n columns of input words as a slice of rows.
// Regs for inputs that have been marked using p.SetMemOK will be direct memory references.
// Regs for other inputs will be newly allocated registers and must be freed.
public static slice<slice<ΔReg>> LoadN(this ref ΔPipe p, nint n) {
    var a = p.f.Asm;
    var regs = new slice<slice<ΔReg>>(len(p.inPtr));
    foreach (var (i, vᴛ1) in p.inPtr) {
        ref var ptr = ref heap(new RegPtr(), out var Ꮡptr);
        ptr = vᴛ1;

        regs[i] = new slice<ΔReg>(n);
        switch (ᐧ) {
        case {} when a.Arch.loadIncN != nil: {
            // Load from memory and advance pointers at the same time.
            foreach (var j in range(regs[i])) {
                regs[i][j] = p.f.Asm.Reg();
            }
            if (p.backward) {
                a.Arch.loadDecN(a, ptr, regs[i]);
            } else {
                a.Arch.loadIncN(a, ptr, regs[i]);
            }
            break;
        }
        default: {
            // Load from memory using offsets.
            // We'll advance the pointers or the index counter later.
            foreach (var j in range(n)) {
                nint off = p.readOff + j;
                if (p.backward) {
                    off = -(off + 1);
                }
                ΔReg mem = default;
                if (p.indexCounter != 0) {
                    mem = a.Arch.memIndex(a, off * a.Arch.WordBytes, p.index, ptr);
                } else {
                    mem = Ꮡptr.mem(off * a.Arch.WordBytes);
                }
                Hint h = HintNone;
                if (i < len(p.hints)) {
                    h = p.hints[i];
                }
                if (h == HintMemOK) {
                    regs[i][j] = mem;
                } else {
                    var r = p.f.Asm.RegHint(h);
                    a.Mov(mem, r);
                    regs[i][j] = r;
                }
            }
            break;
        }}
    }
    p.readOff += n;
    return regs;
}

// StoreN writes regs (a slice of rows) to the next n columns of output, where n = len(regs[0]).
public static void StoreN(this ref ΔPipe p, slice<slice<ΔReg>> regs) {
    p.needWrite = false;
    var a = p.f.Asm;
    if (len(regs) != len(p.outPtr)) {
        p.f.Asm.Fatalf("wrong number of output rows"u8);
    }
    nint n = len(regs[0]);
    foreach (var (i, vᴛ1) in p.outPtr) {
        ref var ptr = ref heap(new RegPtr(), out var Ꮡptr);
        ptr = vᴛ1;

        switch (ᐧ) {
        case {} when a.Arch.storeIncN != nil: {
            // Store to memory and advance pointers at the same time.
            if (p.backward) {
                a.Arch.storeDecN(a, ptr, regs[i]);
            } else {
                a.Arch.storeIncN(a, ptr, regs[i]);
            }
            break;
        }
        default: {
            // Store to memory using offsets.
            // We'll advance the pointers or the index counter later.
            foreach (var (j, r) in regs[i]) {
                nint off = p.writeOff + j;
                if (p.backward) {
                    off = -(off + 1);
                }
                ΔReg mem = default;
                if (p.indexCounter != 0) {
                    mem = a.Arch.memIndex(a, off * a.Arch.WordBytes, p.index, ptr);
                } else {
                    mem = Ꮡptr.mem(off * a.Arch.WordBytes);
                }
                a.Mov(r, mem);
            }
            break;
        }}
    }
    p.writeOff += n;
}

// advancePtrs advances the pointers by step
// or handles bookkeeping for an imminent index advance by step
// that the caller will do.
internal static void advancePtrs(this ref ΔPipe p, nint step) {
    var a = p.f.Asm;
    switch (ᐧ) {
    case {} when a.Arch.loadIncN != nil: {
        // nothing to do
        break;
    }
    default: {
        // Adjust read/write offsets for pointer advance (or imminent index advance).
        p.readOff -= step;
        p.writeOff -= step;

        if (p.indexCounter == 0) {
            // Advance pointers.
            if (p.backward) {
                step = -step;
            }
            foreach (var (_, ptr) in p.inPtr) {
                a.Add(a.Imm(step * a.Arch.WordBytes), (ΔReg)ptr, (ΔReg)ptr, KeepCarry);
            }
            foreach (var (_, ptr) in p.outPtr) {
                a.Add(a.Imm(step * a.Arch.WordBytes), (ΔReg)ptr, (ΔReg)ptr, KeepCarry);
            }
        }
        break;
    }}
}

// DropInput deletes the named input from the pipe,
// usually because it has been exhausted.
// (This is not used yet but will be used in a future generator.)
public static void DropInput(this ref ΔPipe p, @string name) {
    nint i = slices.Index(p.f.inputs, name);
    if (i < 0) {
        p.f.Asm.Fatalf("unknown input %s"u8, name);
    }
    var ptr = p.inPtr[i];
    p.f.Asm.Free((ΔReg)ptr);
    p.inPtr = slices.Delete<slice<RegPtr>, RegPtr>(p.inPtr, i, i + 1);
    p.f.inputs = slices.Delete<slice<@string>, @string>(p.f.inputs, i, i + 1);
    if (len(p.hints) > i) {
        p.hints = slices.Delete<slice<Hint>, Hint>(p.hints, i, i + 1);
    }
}

// Start prepares to loop over n columns.
// The factors give a sequence of unrolling factors to use,
// which must be either strictly increasing or strictly decreasing
// and must include 1.
// For example, 4, 1 means to process 4 elements at a time
// and then 1 at a time for the final 0-3; specifying 1,4 instead
// handles 0-3 elements first and then 4 at a time.
// Similarly, 32, 4, 1 means to process 32 at a time,
// then 4 at a time, then 1 at a time.
//
// One benefit of using 1, 4 instead of 4, 1 is that the body
// processing 4 at a time needs more registers, and if it is
// the final body, the register holding the fragment count (0-3)
// has been freed and is available for use.
//
// Start may modify the carry flag.
//
// Start must be followed by a call to Loop1 or LoopN,
// but it is permitted to emit other instructions first,
// for example to set an initial carry flag.
public static void Start(this ref ΔPipe p, ΔReg n, params ꓸꓸꓸnint factorsʗp) {
    var factors = factorsʗp.slice();

    var a = p.f.Asm;
    if (p.started) {
        a.Fatalf("loop already started"u8);
    }
    if (p.useIndexCounter && len(factors) > 1) {
        a.Fatalf("cannot call SetUseIndexCounter and then use Start with factors != [1]; have factors = %v"u8, factors);
    }
    p.started = true;
    if (!p.loaded) {
        if (len(factors) == 1) {
            p.SetUseIndexCounter();
        }
        p.LoadPtrs(n);
    }

    // If there were calls to LoadN between LoadPtrs and Start,
    // adjust the loop not to scan those columns, assuming that
    // either the code already called an equivalent StoreN or else
    // that it will do so after the loop.
    {
        nint off = p.readOff; if (off != 0) {
            if (p.indexCounter < 0) {
                // Index is negated, so add off instead of subtracting.
                a.Add(a.Imm(off), n, n, SmashCarry);
            } else {
                a.Sub(a.Imm(off), n, n, SmashCarry);
            }
            if (p.indexCounter != 0) {
                // n is also the index we are using, so adjust readOff and writeOff
                // to continue to point at the same positions as before we changed n.
                p.readOff -= off;
                p.writeOff -= off;
            }
        }
    }

    p.Restart(n, factors.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string computeUnrolledLoopˢ = "compute unrolled loop lengths"u8;

// Restart prepares to loop over an additional n columns,
// beyond a previous loop run by p.Start/p.Loop.
public static void Restart(this ref ΔPipe p, ΔReg n, params ꓸꓸꓸnint factorsʗp) {
    var factors = factorsʗp.slice();

    var a = p.f.Asm;
    if (!p.started) {
        a.Fatalf("pipe not started"u8);
    }
    p.factors = factors;
    p.counts = new slice<ΔReg>(len(factors));
    if (len(factors) == 0) {
        factors = slice<nint>([1]);
    }

    // Compute the loop lengths for each unrolled section into separate registers.
    // We compute them all ahead of time in case the computation would smash
    // a carry flag that the loop bodies need preserved.
    if (len(factors) > 1) {
        a.Comment(computeUnrolledLoopˢ);
    }
    switch (ᐧ) {
    default: {
        a.Fatalf("invalid factors %v"u8, factors);
        break;
    }
    case {} when factors[0] is 1: {
        // increasing loop factors
        nint div = 1;
        foreach (var (i, vᴛ1) in factors[1..]) {
            var f = vᴛ1;

            if (f <= factors[i]) {
                a.Fatalf("non-increasing factors %v"u8, factors);
            }
            if ((f & (f - 1)) != 0) {
                a.Fatalf("non-power-of-two factors %v"u8, factors);
            }
            var t = p.f.Asm.Reg();
            f = quo(f, div);
            a.And(a.Imm(f - 1), n, t);
            a.Rsh(a.Imm(bits.TrailingZeros((nuint)f)), n, n);
            div *= f;
            p.counts[i] = t;
        }
        p.counts[len(p.counts) - 1] = n;
        break;
    }
    case {} when factors[len(factors) - 1] is 1: {
        // decreasing loop factors
        foreach (var (i, f) in factors.slice(0, len(factors) - 1)) {
            if (f <= factors[i + 1]) {
                a.Fatalf("non-decreasing factors %v"u8, factors);
            }
            if ((f & (f - 1)) != 0) {
                a.Fatalf("non-power-of-two factors %v"u8, factors);
            }
            var t = p.f.Asm.Reg();
            a.Rsh(a.Imm(bits.TrailingZeros((nuint)f)), n, t);
            a.And(a.Imm(f - 1), n, n);
            p.counts[i] = t;
        }
        p.counts[len(p.counts) - 1] = n;
        break;
    }}
}

// Done frees all the registers allocated by the pipe.
public static void Done(this ref ΔPipe p) {
    foreach (var (_, ptr) in p.inPtr) {
        p.f.Asm.Free((ΔReg)ptr);
    }
    p.inPtr = default;
    foreach (var (_, ptr) in p.outPtr) {
        p.f.Asm.Free((ΔReg)ptr);
    }
    p.outPtr = default;
    p.index = new ΔReg();
}

// Loop emits code for the loop, calling block repeatedly to emit code that
// handles a block of N input columns (for arbitrary N = len(in[0]) chosen by p).
// block must call p.StoreN(out) to write N output columns.
// The out slice is a pre-allocated matrix of uninitialized Reg values.
// block is expected to set each entry to the Reg that should be written
// before calling p.StoreN(out).
//
// For example, if the loop is to be unrolled 4x in blocks of 2 columns each,
// the sequence of calls to emit the unrolled loop body is:
//
//	start()  // set by pAtUnrollStart
//	... reads for 2 columns ...
//	block()
//	... writes for 2 columns ...
//	... reads for 2 columns ...
//	block()
//	... writes for 2 columns ...
//	end()  // set by p.AtUnrollEnd
//
// Any registers allocated during block are freed automatically when block returns.
public static void Loop(this ref ΔPipe p, Action<slice<slice<ΔReg>>, slice<slice<ΔReg>>> block) {
    if (p.factors == nil) {
        p.f.Asm.Fatalf("Pipe.Start not called"u8);
    }
    foreach (var (i, factor) in p.factors) {
        var n = p.counts[i];
        p.unroll(n, factor, block);
        if (i < len(p.factors) - 1) {
            p.f.Asm.Free(n);
        }
    }
    p.factors = default;
}

// AtUnrollStart sets a function to call at the start of an unrolled sequence.
// See [Pipe.Loop] for details.
public static void AtUnrollStart(this ref ΔPipe p, Action start) {
    p.unrollStart = start;
}

// AtUnrollEnd sets a function to call at the end of an unrolled sequence.
// See [Pipe.Loop] for details.
public static void AtUnrollEnd(this ref ΔPipe p, Action end) {
    p.unrollEnd = end;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unrollDXˢ = "unroll %dX"u8;
internal static readonly @string unrollDXInBatchesOfDˢ = "unroll %dX in batches of %d"u8;

// unroll emits a single unrolled loop for the given factor, iterating n times.
internal static void unroll(this ref ΔPipe p, ΔReg n, nint factor, Action<slice<slice<ΔReg>>, slice<slice<ΔReg>>> block) {
    var a = p.f.Asm;
    @string label = fmt.Sprintf("%s%d"u8, p.label, factor);

    // Top of loop control flow.
    a.Label(label);
    if (a.Arch.loopTop != ""u8) {
        a.Printf("\t"u8 + a.Arch.loopTop + "\n"u8, n, label + "done");
    } else {
        a.JmpZero(n, label + "done"u8);
    }
    a.Label(label + "cont"u8);

    // Unrolled loop body.
    if (factor < p.maxColumns) {
        a.Comment(unrollDXˢ, factor);
    } else {
        a.Comment(unrollDXInBatchesOfDˢ, factor, p.maxColumns);
    }
    if (p.unrollStart != nil) {
        p.unrollStart();
    }
    for (nint done = 0; done < factor; ) {
        nint batch = min(factor - done, p.maxColumns);
        var regs = a.RegsUsed();
        var @out = new slice<slice<ΔReg>>(len(p.outPtr));
        foreach (var i in range(@out)) {
            @out[i] = new slice<ΔReg>(batch);
        }
        var @in = p.LoadN(batch);
        p.needWrite = true;
        block(@in, @out);
        if (p.needWrite && len(p.outPtr) > 0) {
            a.Fatalf("missing p.Write1 or p.StoreN"u8);
        }
        a.SetRegsUsed(regs); // free anything block allocated
        done += batch;
    }
    if (p.unrollEnd != nil) {
        p.unrollEnd();
    }
    p.advancePtrs(factor);

    // Bottom of loop control flow.
    switch (ᐧ) {
    case {} when p.indexCounter >= 0 && a.Arch.loopBottom != ""u8: {
        a.Printf("\t"u8 + a.Arch.loopBottom + "\n"u8, n, label + "cont");
        break;
    }
    case {} when p.indexCounter is >= 0: {
        a.Sub(a.Imm(1), n, n, KeepCarry);
        a.JmpNonZero(n, label + "cont"u8);
        break;
    }
    case {} when p.indexCounter < 0 && a.Arch.loopBottomNeg != ""u8: {
        a.Printf("\t"u8 + a.Arch.loopBottomNeg + "\n"u8, n, label + "cont");
        break;
    }
    case {} when p.indexCounter is < 0: {
        a.Add(a.Imm(1), n, n, KeepCarry);
        break;
    }}
    a.Label(label + "done"u8);
}

} // end asmgen_package
