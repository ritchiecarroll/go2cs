// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace stack table and acquisition.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using atomic = @internal.runtime.atomic_package;

partial class runtime_package {

internal static UntypedInt traceStackSize => 128;
internal static uintptr logicalStackSentinel => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptedToTraceStackOfAˢ = "attempted to trace stack of a goroutine this thread does not own"u8;

// traceStack captures a stack trace from a goroutine and registers it in the trace
// stack table. It then returns its unique ID. If gp == nil, then traceStack will
// attempt to use the current execution context.
//
// skip controls the number of leaf frames to omit in order to hide tracer internals
// from stack traces, see CL 5523.
//
// Avoid calling this function directly. gen needs to be the current generation
// that this stack trace is being written out for, which needs to be synchronized with
// generations moving forward. Prefer traceEventWriter.stack.
internal static uint64 traceStack(nint skip, ж<g> Ꮡgp, uintptr gen) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    array<uintptr> pcBuf = new(128); /* traceStackSize */
    // Figure out gp and mp for the backtrace.
    ж<m> mp = default!;
    if (Ꮡgp == nil) {
        mp = getg().Value.m;
        Ꮡgp = mp.Value.curg; gp = ref Ꮡgp.DerefOrNull();
    }
    // Double-check that we own the stack we're about to trace.
    if (debug.traceCheckStackOwnership != 0 && Ꮡgp != nil) {
        var status = readgstatus(Ꮡgp);
        // If the scan bit is set, assume we're the ones that acquired it.
        if ((uint32)(status & (uint32)_Gscan) == 0) {
            // Use the trace status to check this. There are a number of cases
            // where a running goroutine might be in _Gwaiting, and these cases
            // are totally fine for taking a stack trace. They're captured
            // correctly in goStatusToTraceGoStatus.
            var exprᴛ1 = goStatusToTraceGoStatus(status, gp.waitreason);
            var matchᴛ1 = false;
            if (exprᴛ1 == traceGoRunning || exprᴛ1 == traceGoSyscall) { matchᴛ1 = true;
                do {
                    if (getg() == Ꮡgp || (~mp).curg == Ꮡgp) {
                        break;
                    }
                    fallthrough = true;
                } while (false);
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                print((@string)"runtime: gp="u8, @unsafe.Pointer.FromPinnedBox(Ꮡgp), (@string)" gp.goid="u8, gp.goid, (@string)" status="u8, gStatusStrings[(nint)(status)], (@string)"\n"u8);
                @throw(attemptedToTraceStackOfAˢ);
            }

        }
    }
    if (Ꮡgp != nil && mp == nil) {
        // We're getting the backtrace for a G that's not currently executing.
        // It may still have an M, if it's locked to some M.
        mp = gp.lockedm.ptr();
    }
    nint nstk = 1;
    if (tracefpunwindoff() || (mp != nil && mp.hasCgoOnStack())){
        // Slow path: Unwind using default unwinder. Used when frame pointer
        // unwinding is unavailable or disabled (tracefpunwindoff), or might
        // produce incomplete results or crashes (hasCgoOnStack). Note that no
        // cgo callback related crashes have been observed yet. The main
        // motivation is to take advantage of a potentially registered cgo
        // symbolizer.
        pcBuf[0] = logicalStackSentinel;
        if (getg() == Ꮡgp){
            nstk += callers(skip + 1, pcBuf[1..]);
        } else 
        if (Ꮡgp != nil) {
            nstk += gcallers(Ꮡgp, skip, pcBuf[1..]);
        }
    } else {
        // Fast path: Unwind using frame pointers.
        pcBuf[0] = (uintptr)skip;
        if (getg() == Ꮡgp){
            nstk += fpTracebackPCs((@unsafe.Pointer)getfp(), pcBuf[1..]);
        } else 
        if (Ꮡgp != nil) {
            // Three cases:
            //
            // (1) We're called on the g0 stack through mcall(fn) or systemstack(fn). To
            // behave like gcallers above, we start unwinding from sched.bp, which
            // points to the caller frame of the leaf frame on g's stack. The return
            // address of the leaf frame is stored in sched.pc, which we manually
            // capture here.
            //
            // (2) We're called against a gp that we're not currently executing on, but that isn't
            // in a syscall, in which case it's currently not executing. gp.sched contains the most
            // up-to-date information about where it stopped, and like case (1), we match gcallers
            // here.
            //
            // (3) We're called against a gp that we're not currently executing on, but that is in
            // a syscall, in which case gp.syscallsp != 0. gp.syscall* contains the most up-to-date
            // information about where it stopped, and like case (1), we match gcallers here.
            if (gp.syscallsp != 0){
                pcBuf[1] = gp.syscallpc;
                nstk += 1 + fpTracebackPCs((@unsafe.Pointer)gp.syscallbp, pcBuf[2..]);
            } else {
                pcBuf[1] = gp.sched.pc;
                nstk += 1 + fpTracebackPCs((@unsafe.Pointer)gp.sched.bp, pcBuf[2..]);
            }
        }
    }
    if (nstk > 0) {
        nstk--; // skip runtime.goexit
    }
    if (nstk > 0 && gp.goid == 1) {
        nstk--; // skip runtime.main
    }
    var id = ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (nint)(gen % 2)).put(pcBuf[..(int)(nstk)]);
    return id;
}

// traceStackTable maps stack traces (arrays of PC's) to unique uint32 ids.
// It is lock-free for reading.
[GoType] partial struct traceStackTable {
    internal traceMap tab;
}

// put returns a unique id for the stack trace pcs and caches it in the table,
// if it sees the trace for the first time.
internal static uint64 put(this ж<traceStackTable> Ꮡt, slice<uintptr> pcs) {
    if (len(pcs) == 0) {
        return 0;
    }
    var (id, _) = Ꮡt.of(traceStackTable.Ꮡtab).put((uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡ(pcs, 0))), (uintptr)len(pcs) * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    return id;
}

// dump writes all previously cached stacks to trace buffers,
// releases all memory and resets state. It must only be called once the caller
// can guarantee that there are no more writers to the table.
internal static void dump(this ж<traceStackTable> Ꮡt, uintptr gen) {
    var stackBuf = new slice<uintptr>(traceStackSize);
    var w = unsafeTraceWriter(gen, nil);
    {
        var root = (ж<traceMapNode>)(uintptr)(Ꮡt.of(traceStackTable.Ꮡtab).of(traceMap.Ꮡroot).Load()); if (root != nil) {
            w = dumpStacksRec(root, w, stackBuf);
        }
    }
    w.flush().end();
    Ꮡt.of(traceStackTable.Ꮡtab).reset();
}

internal static traceWriter dumpStacksRec(ж<traceMapNode> Ꮡnode, traceWriter w, slice<uintptr> stackBuf) {
    ref var node = ref Ꮡnode.DerefOrNull();

    var Δstack = @unsafe.Slice(Ꮡ(node.data, 0).Reinterpret<byte, uintptr>(), (uintptr)len(node.data) / /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    // N.B. This might allocate, but that's OK because we're not writing to the M's buffer,
    // but one we're about to create (with ensure).
    nint n = fpunwindExpand(stackBuf, Δstack);
    var frames = makeTraceFrames(w.gen, stackBuf[..(int)(n)]);
    // The maximum number of bytes required to hold the encoded stack, given that
    // it contains N frames.
    nint maxBytes = 1 + (2 + 4 * len(frames)) * (nint)traceBytesPerNumber;
    // Estimate the size of this record. This
    // bound is pretty loose, but avoids counting
    // lots of varint sizes.
    //
    // Add 1 because we might also write traceEvStacks.
    bool flushed = default!;
    (w, flushed) = w.ensure(1 + maxBytes);
    if (flushed) {
        w.@byte((byte)traceEvStacks);
    }
    // Emit stack event.
    w.@byte((byte)traceEvStack);
    w.varint((uint64)node.id);
    w.varint((uint64)len(frames));
    foreach (var (_, frame) in frames) {
        w.varint((uint64)frame.PC);
        w.varint(frame.funcID);
        w.varint(frame.fileID);
        w.varint(frame.line);
    }
    // Recursively walk all child nodes.
    foreach (var (i, _) in node.children) {
        @unsafe.Pointer child = (uintptr)Ꮡnode.at(traceMapNode.Ꮡchildren, i).Load();
        if (child == nil) {
            continue;
        }
        w = dumpStacksRec((ж<traceMapNode>)(uintptr)(child), w, stackBuf);
    }
    return w;
}

// makeTraceFrames returns the frames corresponding to pcs. It may
// allocate and may emit trace events.
internal static slice<traceFrame> makeTraceFrames(uintptr gen, slice<uintptr> pcs) {
    var frames = new slice<traceFrame>(0, len(pcs));
    var ci = CallersFrames(pcs);
    while (ᐧ) {
        var (f, more) = ci.Next();
        frames = append(frames, makeTraceFrame(gen, f));
        if (!more) {
            return frames;
        }
    }
}

[GoType] partial struct traceFrame {
    public uintptr PC;
    internal uint64 funcID;
    internal uint64 fileID;
    internal uint64 line;
}

// makeTraceFrame sets up a traceFrame for a frame.
internal static traceFrame makeTraceFrame(uintptr gen, Frame f) {
    traceFrame frame = default!;
    frame.PC = f.PC;
    @string fn = f.Function;
    const nint maxLen = /* 1 << 10 */ 1024;
    if (len(fn) > maxLen) {
        fn = fn[(int)(len(fn) - maxLen)..];
    }
    frame.funcID = ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (nint)(gen % 2)).put(gen, fn);
    frame.line = (uint64)f.Line;
    @string @file = f.File;
    if (len(@file) > maxLen) {
        @file = @file[(int)(len(@file) - maxLen)..];
    }
    frame.fileID = ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (nint)(gen % 2)).put(gen, @file);
    return frame;
}

// tracefpunwindoff returns true if frame pointer unwinding for the tracer is
// disabled via GODEBUG or not supported by the architecture.
internal static bool tracefpunwindoff() {
    return debug.tracefpunwindoff != 0 || (goarch.ArchFamily != goarch.AMD64 && goarch.ArchFamily != goarch.ARM64);
}

// fpTracebackPCs populates pcBuf with the return addresses for each frame and
// returns the number of PCs written to pcBuf. The returned PCs correspond to
// "physical frames" rather than "logical frames"; that is if A is inlined into
// B, this will return a PC for only B.
internal static nint /*i*/ fpTracebackPCs(@unsafe.Pointer fp, slice<uintptr> pcBuf) {
    nint i = default!;

    for (i = 0; i < len(pcBuf) && fp != nil; i++) {
        // return addr sits one word above the frame pointer
        pcBuf[i] = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)((uintptr)fp + (uintptr)goarch.PtrSize));
        // follow the frame pointer to the next one
        fp.Value = (@unsafe.Pointer)(~(ж<uintptr>)(uintptr)(fp));
    }
    return i;
}

//go:linkname pprof_fpunwindExpand
public static nint pprof_fpunwindExpand(slice<uintptr> dst, slice<uintptr> src) {
    return fpunwindExpand(dst, src);
}

// fpunwindExpand expands a call stack from pcBuf into dst,
// returning the number of PCs written to dst.
// pcBuf and dst should not overlap.
//
// fpunwindExpand checks if pcBuf contains logical frames (which include inlined
// frames) or physical frames (produced by frame pointer unwinding) using a
// sentinel value in pcBuf[0]. Logical frames are simply returned without the
// sentinel. Physical frames are turned into logical frames via inline unwinding
// and by applying the skip value that's stored in pcBuf[0].
internal static nint fpunwindExpand(slice<uintptr> dst, slice<uintptr> pcBuf) {
    if (len(pcBuf) == 0){
        return 0;
    } else 
    if (len(pcBuf) > 0 && pcBuf[0] == logicalStackSentinel) {
        // pcBuf contains logical rather than inlined frames, skip has already been
        // applied, just return it without the sentinel value in pcBuf[0].
        return copy(dst, pcBuf[1..]);
    }
    nint n = default!;
    abi.FuncID lastFuncID = abi.FuncIDNormal;
    uintptr skip = pcBuf[0];

    var dstʗ1 = dst;
    Func<uintptr, bool> skipOrAdd = (uintptr retPC) => {
        if (skip > 0){
            skip--;
        } else 
        if (n < len(dstʗ1)) {
            dstʗ1[n] = retPC;
            n++;
        }
        return n < len(dstʗ1);
    };
outer:
    foreach (var (_, retPC) in pcBuf[1..]) {
        var callPC = retPC - 1;
        var fi = findfunc(callPC);
        if (!fi.valid()) {
            // There is no funcInfo if callPC belongs to a C function. In this case
            // we still keep the pc, but don't attempt to expand inlined frames.
            {
                var more = skipOrAdd(retPC); if (!more) {
                    goto break_outer;
                }
            }
            continue;
        }
        var (u, uf) = newInlineUnwinder(fi, callPC);
        for (; uf.valid(); uf = u.next(uf)) {
            var sf = u.srcFunc(uf);
            if (sf.funcID == abi.FuncIDWrapper && elideWrapperCalling(lastFuncID)){
            } else 
            {
                var more = skipOrAdd(uf.pc + 1); if (!more) {
                    // ignore wrappers
                    goto break_outer;
                }
            }
            lastFuncID = sf.funcID;
        }
continue_outer:;
    }
break_outer:;
    return n;
}

// startPCForTrace returns the start PC of a goroutine for tracing purposes.
// If pc is a wrapper, it returns the PC of the wrapped function. Otherwise it
// returns pc.
internal static uintptr startPCForTrace(uintptr pc) {
    var f = findfunc(pc);
    if (!f.valid()) {
        return pc; // may happen for locked g in extra M since its pc is 0.
    }
    @unsafe.Pointer w = (uintptr)funcdata(f, abi.FUNCDATA_WrapInfo);
    if (w == nil) {
        return pc; // not a wrapper
    }
    return f.datap.textAddr(~(ж<uint32>)(uintptr)(w));
}

} // end runtime_package
