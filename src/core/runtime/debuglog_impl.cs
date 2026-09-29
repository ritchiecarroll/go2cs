// debuglog_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-finished conversion of debuglog.go's dlogImpl, printDebugLogImpl and printDebugLogPC: the
// three regions the runtime row's six TestDebugLog* rows reach (the stop-the-world seat, owner
// ruling 2026-09-28 00:40). Everything else in debuglog.go stays auto-converted: the writer's
// framing, the reader's decoding, every record method.
//
// WHAT CHANGED, and only that (printDebugLogImpl's two lifted local types are declared here, as its
// displaced body took them with it; dloggerImpl.s is below its own comment):
//   - dlogImpl allocated a logger with sysAllocOS and reinterpreted the native block as a
//     *dloggerImpl, a struct holding managed references (allLink, the reader's buffer pointer).
//     Here the logger is a managed allocation, and it is never freed, as Go's never is.
//   - allDloggers is a prepend-only list Go reads and CASes through a *uintptr reinterpret of the
//     variable's address. The variable is a managed reference slot here, and the same prepend-only
//     CAS runs on the slot itself (Interlocked.CompareExchange), with the same no-ABA argument Go
//     gives: nothing is ever unlinked.
//   - printDebugLogImpl allocated its per-log read state with sysAllocOS for the same reason (a
//     fatal-panic path must not touch the heap). Its read state holds a managed reference (the
//     reader's buffer), so it is a managed slice here.
//   - printDebugLogImpl writes a record's timestamp bytes with gwrite, not through a temporary
//     string over them (slicebytetostringtmp reinterprets a string header).
//   - Go's `print` in the three reader bodies (printDebugLogImpl, printDebugLogPC and
//     debugLogReader.printVal) is the runtime's own printer, which DumpDebugLog captures through the
//     goroutine's writebuf; they call the runtime's print primitives here (see printVal).
//   - printDebugLogPC symbolized through findfunc, and there is no pclntab: a pc here is an opaque
//     per-call-site token (managed_impl.cs, the caller records), so it resolves through the same
//     records runtime.Caller and FuncForPC read. A token names its call site, not an address after
//     the call, so a return pc needs no back-up.

[module: go.GoManualConversion]

namespace go;

using System.Threading;
using atomic = @internal.runtime.atomic_package;
using @internal;
// The plain namespace using brings internal/runtime/atomic's [GoRecv] extension methods (Load,
// CompareAndSwap, Store on the owned flag) into scope; an alias alone does not.
using @internal.runtime;

partial class runtime_package
{
    // printDebugLogImpl's two local types, lifted to package level as the converter lifts them.
    // Displacing the converted body displaces its lifted types with it, so they are declared here.
    [GoType("dyn")] [GoLocalName("readState")] internal partial struct printDebugLogImpl_readState {
        internal partial ref debugLogReader debugLogReader { get; }
        internal bool first;
        internal uint64 lost;
        internal uint64 nextTick;
    }

    [GoType("dyn")] internal partial struct printDebugLogImpl_best {
        internal uint64 tick;
        internal nint i;
    }

    internal static ж<dloggerImpl> dlogImpl() {
        // Get the time.
        var (tick, nano) = ((uint64)cputicks(), (uint64)nanotime());
        // Try to get a cached logger.
        var l = getCachedDlogger();
        // If we couldn't get a cached logger, try to get one from the
        // global pool.
        if (l == nil) {
            var all = Volatile.Read(ref allDloggers);
            for (var l1 = all; l1 != nil; l1 = l1.Value.allLink) {
                if (l1.of(dloggerImpl.Ꮡowned).Load() == 0 && l1.of(dloggerImpl.Ꮡowned).CompareAndSwap(0, 1)) {
                    l = l1;
                    break;
                }
            }
        }
        // If that failed, allocate a new logger: managed, and never freed, as Go's never is.
        if (l == nil) {
            l = @new<dloggerImpl>();
            l.Value.w.r.data = l.of(dloggerImpl.Ꮡw).of(debugLogWriter.Ꮡdata);
            l.of(dloggerImpl.Ꮡowned).Store(1);
            // Prepend to allDloggers list.
            while (true) {
                var head = Volatile.Read(ref allDloggers);
                l.Value.allLink = head;
                if (ReferenceEquals(Interlocked.CompareExchange(ref allDloggers, l, head), head)) {
                    break;
                }
            }
        }
        // If the time delta is getting too high, write a new sync
        // packet. We set the limit so we don't write more than 6
        // bytes of delta in the record header.
        const uint64 deltaLimit = /* 1<<(3*7) - 1 */ 2097151; // ~2ms between sync packets
        if (tick - (~l).w.tick > deltaLimit || nano - (~l).w.nano > deltaLimit) {
            l.of(dloggerImpl.Ꮡw).writeSync(tick, nano);
        }
        // Reserve space for framing header.
        l.of(dloggerImpl.Ꮡw).ensure(debugLogHeaderSize);
        l.Value.w.write += debugLogHeaderSize;
        // Write record header.
        l.of(dloggerImpl.Ꮡw).uvarint(tick - (~l).w.tick);
        l.of(dloggerImpl.Ꮡw).uvarint(nano - (~l).w.nano);
        var gp = getg();
        if (gp != nil && (~gp).m != nil && (~(~gp).m).p != 0){
            l.of(dloggerImpl.Ꮡw).varint((int64)(~(~(~gp).m).p.ptr()).id);
        } else {
            l.of(dloggerImpl.Ꮡw).varint(-1);
        }
        return l;
    }

    // dloggerImpl.s logs a string. Go logs a string constant by its offset into rodata
    // (debugLogConstString) and any other string by value, building the []byte over the string's
    // data by writing a slice header through a reinterpret, which the managed model refuses. A
    // managed string has no rodata address and no header to rewrite, so every string is logged by
    // value, from a copy of its bytes; the reader prints both kinds identically.
    internal static ж<dloggerImpl> s(this ж<dloggerImpl> Ꮡl, @string x) {
        ref var l = ref Ꮡl.DerefOrNull();

        l.w.@byte(debugLogString);
        slice<byte> b = x;
        if (len(b) > debugLogStringLimit) {
            b = b[..(int)(debugLogStringLimit)];
        }
        l.w.uvarint((uint64)len(b));
        l.w.bytes(b);
        if (len(b) != len(x)) {
            l.w.@byte(debugLogStringOverflow);
            l.w.uvarint((uint64)(len(x) - len(b)));
        }
        return Ꮡl;
    }

    internal static void printDebugLogImpl() {
        // This function should not panic or throw since it is used in
        // the fatal panic path and this may deadlock.
        printlock();
        // Get the list of all debug logs.
        var all = Volatile.Read(ref allDloggers);
        // Count the logs.
        nint n = 0;
        for (var l = all; l != nil; l = l.Value.allLink) {
            n++;
        }
        if (n == 0) {
            printunlock();
            return;
        }
        var state = new slice<printDebugLogImpl_readState>(n);
        {
            var l = all;
            foreach (var (i, _) in state) {
                var s = Ꮡ(state, i);
                s.Value.debugLogReader = l.Value.w.r;
                s.Value.first = true;
                s.Value.lost = l.Value.w.r.begin;
                s.Value.nextTick = s.of(printDebugLogImpl_readState.ᏑdebugLogReader).peek();
                l = l.Value.allLink;
            }
        }
        // Print records.
        while (true) {
            // Find the next record.
            printDebugLogImpl_best best = default!;
            best.tick = ~(uint64)0;
            foreach (var (i, _) in state) {
                if (state[i].nextTick < best.tick) {
                    best.tick = state[i].nextTick;
                    best.i = i;
                }
            }
            if (best.tick == ~(uint64)0) {
                break;
            }
            // Print record.
            var s = Ꮡ(state, best.i);
            if ((~s).first) {
                dlogWrite(">> begin log "u8);
                dlogInt(best.i);
                if ((~s).lost != 0) {
                    dlogWrite("; lost first "u8);
                    printuint((~s).lost >> (int)(10));
                    dlogWrite("KB"u8);
                }
                dlogWrite(" <<\n"u8);
                s.Value.first = false;
            }
            var (end, _, nano, Δp) = s.of(printDebugLogImpl_readState.ᏑdebugLogReader).header();
            var oldEnd = s.Value.end;
            s.Value.end = end;
            dlogWrite("["u8);
            array<byte> tmpbuf = new(21);
            var pnano = (int64)nano - runtimeInitTime;
            if (pnano < 0) {
                // Logged before runtimeInitTime was set.
                pnano = 0;
            }
            var pnanoBytes = itoaDiv(tmpbuf[..], (uint64)pnano, 9);
            // Go builds a temporary string over the bytes (slicebytetostringtmp, a string-header
            // reinterpret the managed model refuses); printstring would only hand them to gwrite.
            gwrite(pnanoBytes);
            dlogWrite(" P "u8);
            dlogInt(Δp);
            dlogWrite("] "u8);
            for (nint i = 0; (~s).begin < (~s).end; i++) {
                if (i > 0) {
                    dlogWrite(" "u8);
                }
                if (!s.of(printDebugLogImpl_readState.ᏑdebugLogReader).printVal()) {
                    // Abort this P log.
                    dlogWrite("<aborting P log>"u8);
                    end = oldEnd;
                    break;
                }
            }
            dlogWrite("\n"u8);
            // Move on to the next record.
            s.Value.begin = end;
            s.Value.end = oldEnd;
            s.Value.nextTick = s.of(printDebugLogImpl_readState.ᏑdebugLogReader).peek();
        }
        printunlock();
    }

    // printDebugLogPC prints a single symbolized PC: through the caller records (see the header),
    // in Go's format, "0x<pc> [<func>+0x<off> <file>:<line>]", or " [unknown PC]".
    internal static void printDebugLogPC(uintptr pc, bool returnPC) {
        printhex((uint64)pc);
        string? name = managedFuncName(pc);
        CallerFrameRecord? record = callerFrameRecord(pc);
        if (string.IsNullOrEmpty(name) || record is null) {
            dlogWrite(" [unknown PC]"u8);
            return;
        }
        dlogWrite(" ["u8);
        dlogWrite(name);
        dlogWrite("+"u8);
        printhex((uint64)(pc - frameEntry(pc)));
        dlogWrite(" "u8);
        dlogWrite(record.File);
        dlogWrite(":"u8);
        dlogInt(record.Line);
        dlogWrite("]"u8);
    }

    // The runtime's printstring hands a string to gwrite through bytes(s), a string-header
    // reinterpret the managed model refuses (and printbool, printnl and printint's sign are printstring). The
    // reader bodies here write a string's bytes to gwrite directly: a copy, which gwrite only reads.
    private static void dlogWrite(@string s) => gwrite(s);

    // printint, whose sign goes through printstring too.
    private static void dlogInt(int64 v) {
        if (v < 0) {
            dlogWrite("-"u8);
            v = -v;
        }
        printuint((uint64)v);
    }

    // debugLogReader.printVal prints one logged value. Go's `print` is the runtime's own printer
    // (printlock/printstring/printint..., through gwrite), which DumpDebugLog captures by setting the
    // goroutine's writebuf; a converted `print(...)` binds golib's builtin print instead, which
    // writes to stderr and bypasses writebuf, so the three reader bodies here call the runtime's
    // print primitives directly, as Go's compiler lowers `print` (strings through dlogWrite above). A const-string record is never
    // written on this host (dloggerImpl.s above logs every string by value), so its arm reads the
    // record's two words and prints nothing it cannot resolve.
    [GoRecv] internal static bool printVal(this ref debugLogReader r) {
        var typ = (~r.data).b[(nint)(r.begin % (uint64)len((~r.data).b))];
        r.begin++;
        if (typ == debugLogUnknown) {
            dlogWrite("<unknown kind>"u8);
        } else if (typ == debugLogBoolTrue) {
            dlogWrite("true"u8);
        } else if (typ == debugLogBoolFalse) {
            dlogWrite("false"u8);
        } else if (typ == debugLogInt) {
            dlogInt(r.varint());
        } else if (typ == debugLogUint) {
            printuint(r.uvarint());
        } else if (typ == debugLogHex || typ == debugLogPtr) {
            printhex(r.uvarint());
        } else if (typ == debugLogString) {
            var sl = r.uvarint();
            if (r.begin + sl > r.end) {
                r.begin = r.end;
                dlogWrite("<string length corrupted>"u8);
            } else {
                while (sl > 0) {
                    var b = (~r.data).b[(int)(r.begin % (uint64)len((~r.data).b))..];
                    if ((uint64)len(b) > sl) {
                        b = b[..(int)(sl)];
                    }
                    r.begin += (uint64)len(b);
                    sl -= (uint64)len(b);
                    gwrite(b);
                }
            }
        } else if (typ == debugLogConstString) {
            r.uvarint();
            r.uvarint();
            dlogWrite("<const string>"u8);
        } else if (typ == debugLogStringOverflow) {
            dlogWrite("..("u8);
            printuint(r.uvarint());
            dlogWrite(" more bytes).."u8);
        } else if (typ == debugLogPC) {
            printDebugLogPC((uintptr)r.uvarint(), false);
        } else if (typ == debugLogTraceback) {
            nint n = (nint)r.uvarint();
            for (nint i = 0; i < n; i++) {
                dlogWrite("\n\t"u8);
                // gentraceback PCs are always return PCs.
                printDebugLogPC((uintptr)r.uvarint(), true);
            }
        } else {
            dlogWrite("<unknown field type "u8);
            printhex((uint64)typ);
            dlogWrite(" pos "u8);
            printuint(r.begin - 1);
            dlogWrite(" end "u8);
            printuint(r.end);
            dlogWrite(">\n"u8);
            return false;
        }
        return true;
    }
}
