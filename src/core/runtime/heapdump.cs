// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implementation of runtime/debug.WriteHeapDump. Writes all
// objects in the heap plus additional info (roots, threads,
// finalizers, etc.) to a file.
// The format of the dumped file is described at
// https://golang.org/s/go15heapdump.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// go2cs generated this placeholder — func runtime_debug_WriteHeapDump is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static UntypedInt fieldKindEol => 0;
internal static UntypedInt fieldKindPtr => 1;
internal static UntypedInt fieldKindIface => 2;
internal static UntypedInt fieldKindEface => 3;
internal static UntypedInt tagEOF => 0;
internal static UntypedInt tagObject => 1;
internal static UntypedInt tagOtherRoot => 2;
internal static UntypedInt tagType => 3;
internal static UntypedInt tagGoroutine => 4;
internal static UntypedInt tagStackFrame => 5;
internal static UntypedInt tagParams => 6;
internal static UntypedInt tagFinalizer => 7;
internal static UntypedInt tagItab => 8;
internal static UntypedInt tagOSThread => 9;
internal static UntypedInt tagMemStats => 10;
internal static UntypedInt tagQueuedFinalizer => 11;
internal static UntypedInt tagData => 12;
internal static UntypedInt tagBSS => 13;
internal static UntypedInt tagDefer => 14;
internal static UntypedInt tagPanic => 15;
internal static UntypedInt tagMemProf => 16;
internal static UntypedInt tagAllocSample => 17;

internal static uintptr dumpfd; // fd to write the dump to.

internal static ж<slice<byte>> Ꮡtmpbuf = new StandardBox<slice<byte>>(default(slice<byte>));
internal static ref slice<byte> tmpbuf => ref Ꮡtmpbuf.ValueSlot;

// buffer of pending write data
internal static UntypedInt bufSize => 4096;

internal static ж<array<byte>> Ꮡbuf = new StandardBox<array<byte>>(new array<byte>(4096));
internal static ref array<byte> buf => ref Ꮡbuf.Value;

internal static uintptr nbuf;

internal static unsafe void dwrite(@unsafe.Pointer data, uintptr len) {
    if (len == 0) {
        return;
    }
    if (nbuf + len <= bufSize) {
        copy(buf.slice((nint)(nbuf)), new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(data), (int)(len))));
        nbuf += len;
        return;
    }
    write(dumpfd, @unsafe.Pointer.FromPinnedBox(Ꮡbuf), (int32)nbuf);
    if (len >= bufSize){
        write(dumpfd, data, (int32)len);
        nbuf = 0;
    } else {
        copy(buf[..], new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(data), (int)(len))));
        nbuf = len;
    }
}

internal static void dwritebyte(byte bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    dwrite(@unsafe.Pointer.FromPinnedBox(Ꮡb), 1);
}

internal static void flush() {
    write(dumpfd, @unsafe.Pointer.FromPinnedBox(Ꮡbuf), (int32)nbuf);
    nbuf = 0;
}

// Cache of types that have been serialized already.
// We use a type's hash field to pick a bucket.
// Inside a bucket, we keep a list of types that
// have been serialized so far, most recently used first.
// Note: when a bucket overflows we may end up
// serializing a type more than once. That's ok.
internal static UntypedInt typeCacheBuckets => 256;

internal static UntypedInt typeCacheAssoc => 4;

[GoType] partial struct typeCacheBucket {
    internal array<ж<_type>> t = new(typeCacheAssoc);
}

internal static ж<array<typeCacheBucket>> Ꮡtypecache = new StandardBox<array<typeCacheBucket>>(new array<typeCacheBucket>(256, () => new()));
internal static ref array<typeCacheBucket> typecache => ref Ꮡtypecache.Value;

// dump a uint64 in a varint format parseable by encoding/binary.
internal static void dumpint(uint64 v) {
    ref var buf = ref heap(new array<byte>(10), out var Ꮡbuf);
    nint n = default!;
    while (v >= 0x80) {
        buf[n] = (byte)((uint64)(v | 0x80));
        n++;
        v >>= (int)(7);
    }
    buf[n] = (byte)v;
    n++;
    dwrite(@unsafe.Pointer.FromPinnedBox(Ꮡbuf), (uintptr)n);
}

internal static void dumpbool(bool b) {
    if (b){
        dumpint(1);
    } else {
        dumpint(0);
    }
}

// dump varint uint64 length followed by memory contents.
internal static void dumpmemrange(@unsafe.Pointer data, uintptr len) {
    dumpint((uint64)len);
    dwrite(data, len);
}

internal static void dumpslice(slice<byte> b) {
    dumpint((uint64)len(b));
    if (len(b) > 0) {
        dwrite(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), (uintptr)len(b));
    }
}

internal static void dumpstr(@string s) {
    dumpmemrange(@unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)), (uintptr)len(s));
}

// dump information for a type.
internal static void dumptype(ж<_type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt == nil) {
        return;
    }
    // If we've definitely serialized the type before,
    // no need to do it again.
    var b = Ꮡtypecache.at<typeCacheBucket>((ulong)((uint32)(t.Hash & (uint32)((typeCacheBuckets - 1)))));
    if (Ꮡt == (~b).t[0]) {
        return;
    }
    for (nint i = 1; i < typeCacheAssoc; i++) {
        if (Ꮡt == (~b).t[i]) {
            // Move-to-front
            for (nint j = i; j > 0; j--) {
                b.Value.t[j] = (~b).t[j - 1];
            }
            b.Value.t[0] = Ꮡt;
            return;
        }
    }
    // Might not have been dumped yet. Dump it and
    // remember we did so.
    for (nint j = typeCacheAssoc - 1; j > 0; j--) {
        b.Value.t[j] = (~b).t[j - 1];
    }
    b.Value.t[0] = Ꮡt;
    // dump the type
    dumpint(tagType);
    dumpint((uint64)(uintptr)Ꮡt);
    dumpint((uint64)t.Size_);
    var rt = toRType(Ꮡt);
    {
        var x = Ꮡt.Uncommon(); if (x == nil || rt.nameOff((~x).PkgPath).Name() == ""u8){
            dumpstr(rt.@string());
        } else {
            @string pkgpath = rt.nameOff((~x).PkgPath).Name();
            @string name = rt.name();
            dumpint((uint64)((uintptr)len(pkgpath) + 1 + (uintptr)len(name)));
            dwrite(@unsafe.Pointer.FromPinnedBox(@unsafe.StringData(pkgpath)), (uintptr)len(pkgpath));
            dwritebyte((rune)'.');
            dwrite(@unsafe.Pointer.FromPinnedBox(@unsafe.StringData(name)), (uintptr)len(name));
        }
    }
    dumpbool((abiꓸKind)(t.Kind_ & abi.KindDirectIface) == 0 || t.Pointers());
}

// dump an object.
internal static void dumpobj(@unsafe.Pointer obj, uintptr size, bitvector bv) {
    dumpint(tagObject);
    dumpint((uint64)(uintptr)obj);
    dumpmemrange(obj, size);
    dumpfields(bv);
}

internal static void dumpotherroot(@string description, @unsafe.Pointer to) {
    dumpint(tagOtherRoot);
    dumpstr(description);
    dumpint((uint64)(uintptr)to);
}

internal static void dumpfinalizer(@unsafe.Pointer obj, ж<funcval> Ꮡfn, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fn = ref Ꮡfn.DerefOrNull();

    dumpint(tagFinalizer);
    dumpint((uint64)(uintptr)obj);
    dumpint((uint64)(uintptr)Ꮡfn);
    dumpint((uint64)(uintptr)(@unsafe.Pointer)fn.fn);
    dumpint((uint64)(uintptr)Ꮡfint);
    dumpint((uint64)(uintptr)Ꮡot);
}

[GoType] partial struct childInfo {
    // Information passed up from the callee frame about
    // the layout of the outargs region.
    internal uintptr argoff;   // where the arguments start in the frame
    internal uintptr arglen;   // size of args region
    internal bitvector args; // if args.n >= 0, pointer map of args region
    internal ж<uint8> sp; // callee sp
    internal uintptr depth;   // depth in call stack (0 == most recent)
}

// dump kinds & offsets of interesting fields in bv.
internal static void dumpbv(ж<bitvector> Ꮡcbv, uintptr offset) {
    ref var cbv = ref Ꮡcbv.DerefOrNull();

    for (var i = (uintptr)0; i < (uintptr)cbv.n; i++) {
        if (cbv.ptrbit(i) == 1) {
            dumpint(fieldKindPtr);
            dumpint((uint64)(offset + i * (uintptr)goarch.PtrSize));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownFunctionˢ = "unknown function"u8;

internal static void dumpframe(ж<stkframe> Ꮡs, ж<childInfo> Ꮡchild) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var child = ref Ꮡchild.DerefOrNull();

    var f = s.fn;
    // Figure out what we can about our stack map
    var pc = s.pc;
    var pcdata = (int32)(-1); // Use the entry map at function entry
    if (pc != f.entry()) {
        pc--;
        pcdata = pcdatavalue(f, abi.PCDATA_StackMapIndex, pc);
    }
    if (pcdata == -1) {
        // We do not have a valid pcdata value but there might be a
        // stackmap for this function. It is likely that we are looking
        // at the function prologue, assume so and hope for the best.
        pcdata = 0;
    }
    var stkmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_LocalsPointerMaps));
    ref var bv = ref heap(new bitvector(), out var Ꮡbv);
    if (stkmap != nil && (~stkmap).n > 0){
        bv = stackmapdata(stkmap, pcdata);
    } else {
        bv.n = -1;
    }
    // Dump main body of stack frame.
    dumpint(tagStackFrame);
    dumpint((uint64)s.sp); // lowest address in frame
    dumpint((uint64)child.depth); // # of frames deep on the stack
    dumpint((uint64)(uintptr)child.sp); // sp of child, or 0 if bottom of stack
    dumpmemrange((@unsafe.Pointer)s.sp, s.fp - s.sp); // frame contents
    dumpint((uint64)f.entry());
    dumpint((uint64)s.pc);
    dumpint((uint64)s.continpc);
    @string name = funcname(f);
    if (name == ""u8) {
        name = unknownFunctionˢ;
    }
    dumpstr(name);
    // Dump fields in the outargs section
    if (child.args.n >= 0){
        dumpbv(Ꮡchild.of(childInfo.Ꮡargs), child.argoff);
    } else {
        // conservative - everything might be a pointer
        for (var off = child.argoff; off < child.argoff + child.arglen; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint((uint64)off);
        }
    }
    // Dump fields in the local vars section
    if (stkmap == nil){
        // No locals information, dump everything.
        for (var off = child.arglen; off < s.varp - s.sp; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint((uint64)off);
        }
    } else 
    if ((~stkmap).n < 0){
        // Locals size information, dump just the locals.
        var size = (uintptr)(-(~stkmap).n);
        for (var off = s.varp - size - s.sp; off < s.varp - s.sp; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint((uint64)off);
        }
    } else 
    if ((~stkmap).n > 0) {
        // Locals bitmap information, scan just the pointers in
        // locals.
        dumpbv(Ꮡbv, s.varp - (uintptr)bv.n * (uintptr)goarch.PtrSize - s.sp);
    }
    dumpint(fieldKindEol);
    // Record arg info for parent.
    child.argoff = s.argp - s.fp;
    child.arglen = s.argBytes();
    child.sp = (ж<uint8>)(uintptr)((@unsafe.Pointer)s.sp);
    child.depth++;
    stkmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_ArgsPointerMaps));
    if (stkmap != nil){
        child.args = stackmapdata(stkmap, pcdata);
    } else {
        child.args.n = -1;
    }
    return;
}

internal static void dumpgoroutine(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    uintptr sp = default!;
    uintptr pc = default!;
    uintptr lr = default!;
    if (gp.syscallsp != 0){
        sp = gp.syscallsp;
        pc = gp.syscallpc;
        lr = 0;
    } else {
        sp = gp.sched.sp;
        pc = gp.sched.pc;
        lr = gp.sched.lr;
    }
    dumpint(tagGoroutine);
    dumpint((uint64)(uintptr)Ꮡgp);
    dumpint((uint64)sp);
    dumpint(gp.goid);
    dumpint((uint64)gp.gopc);
    dumpint((uint64)readgstatus(Ꮡgp));
    dumpbool(isSystemGoroutine(ref (Ꮡgp).DerefOrNull(), false));
    dumpbool(false); // isbackground
    dumpint((uint64)gp.waitsince);
    dumpstr(gp.waitreason.String());
    dumpint((uint64)(uintptr)gp.sched.ctxt);
    dumpint((uint64)(uintptr)gp.m);
    dumpint((uint64)(uintptr)gp._defer);
    dumpint((uint64)(uintptr)gp._panic);
    // dump stack
    ref var child = ref heap(new childInfo(), out var Ꮡchild);
    child.args.n = -1;
    child.arglen = 0;
    child.sp = default!;
    child.depth = 0;
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    for (Ꮡu.initAt(pc, sp, lr, Ꮡgp, 0); u.valid(); Ꮡu.next()) {
        dumpframe(Ꮡu.of(unwinder.Ꮡframe), Ꮡchild);
    }
    // dump defer & panic records
    for (var d = gp._defer; d != nil; d = d.Value.link) {
        dumpint(tagDefer);
        dumpint((uint64)(uintptr)d);
        dumpint((uint64)(uintptr)Ꮡgp);
        dumpint((uint64)(~d).sp);
        dumpint((uint64)(~d).pc);
        var fn = ~d.of(_defer.Ꮡfn).Reinterpret<Action, ж<funcval>>();
        dumpint((uint64)(uintptr)fn);
        if ((~d).fn == default!){
            // d.fn can be nil for open-coded defers
            dumpint((uint64)0);
        } else {
            dumpint((uint64)(uintptr)(@unsafe.Pointer)(~fn).fn);
        }
        dumpint((uint64)(uintptr)(~d).link);
    }
    for (var Δp = gp._panic; Δp != nil; Δp = Δp.Value.link) {
        dumpint(tagPanic);
        dumpint((uint64)(uintptr)Δp);
        dumpint((uint64)(uintptr)Ꮡgp);
        var eface = efaceOf(Δp.of(_panic.Ꮡarg));
        dumpint((uint64)(uintptr)(~eface)._type);
        dumpint((uint64)(uintptr)(~eface).data);
        dumpint(0); // was p->defer, no longer recorded
        dumpint((uint64)(uintptr)(~Δp).link);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dumpgsInStwBadStatusˢ = "dumpgs in STW - bad status"u8;

internal static void dumpgs() {
    assertWorldStopped();
    // goroutines & stacks
    forEachG((ж<g> gp) => {
        var status = readgstatus(gp); // The world is stopped so gp will not be in a scan state.
        var exprᴛ1 = status;
        if (exprᴛ1 == _Gdead) {
        }
        else if (exprᴛ1 == _Grunnable || exprᴛ1 == _Gsyscall || exprᴛ1 == _Gwaiting) {
            dumpgoroutine(gp);
        }
        else { /* default: */
            print((@string)"runtime: unexpected G.status "u8, ((Δhex)(uint64)status), (@string)"\n"u8);
            @throw(dumpgsInStwBadStatusˢ);
        }

    });
}

// ok
internal static void finq_callback(ж<funcval> Ꮡfn, @unsafe.Pointer obj, uintptr nret, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fn = ref Ꮡfn.DerefOrNull();

    dumpint(tagQueuedFinalizer);
    dumpint((uint64)(uintptr)obj);
    dumpint((uint64)(uintptr)Ꮡfn);
    dumpint((uint64)(uintptr)(@unsafe.Pointer)fn.fn);
    dumpint((uint64)(uintptr)Ꮡfint);
    dumpint((uint64)(uintptr)Ꮡot);
}

internal static void dumproots() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    // TODO(mwhudson): dump datamask etc from all objects
    // data segment
    dumpint(tagData);
    dumpint((uint64)firstmoduledata.data);
    dumpmemrange((@unsafe.Pointer)firstmoduledata.data, firstmoduledata.edata - firstmoduledata.data);
    dumpfields(firstmoduledata.gcdatamask);
    // bss segment
    dumpint(tagBSS);
    dumpint((uint64)firstmoduledata.bss);
    dumpmemrange((@unsafe.Pointer)firstmoduledata.bss, firstmoduledata.ebss - firstmoduledata.bss);
    dumpfields(firstmoduledata.gcbssmask);
    // mspan.types
    foreach (var (_, s) in mheap_.allspans) {
        if (s.of(mspan.Ꮡstate).get() == mSpanInUse) {
            // Finalizers
            for (var sp = s.Value.specials; sp != nil; sp = sp.Value.next) {
                if ((~sp).kind != _KindSpecialFinalizer) {
                    continue;
                }
                var spf = sp.Reinterpret<special, specialfinalizer>();
                @unsafe.Pointer Δp = (@unsafe.Pointer)(s.@base() + (uintptr)(~spf).special.offset);
                dumpfinalizer(Δp, (~spf).fn, (~spf).fint, (~spf).ot);
            }
        }
    }
    // Finalizer queue
    iterate_finq(finq_callback);
}

// Bit vector of free marks.
// Needs to be as big as the largest number of objects per span.
internal static array<bool> freemark = new(1024);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string freemarkArrayDoesnTHaveˢ = "freemark array doesn't have enough entries"u8;

internal static void dumpobjs() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    foreach (var (_, s) in mheap_.allspans) {
        if (s.of(mspan.Ꮡstate).get() != mSpanInUse) {
            continue;
        }
        var Δp = s.@base();
        var size = s.Value.elemsize;
        var n = (((~s).npages << (int)(_PageShift))) / size;
        if (n > (uintptr)len(freemark)) {
            @throw(freemarkArrayDoesnTHaveˢ);
        }
        for (var freeIndex = (uint16)0; freeIndex < (~s).nelems; freeIndex++) {
            if (s.isFree((uintptr)freeIndex)) {
                freemark[freeIndex] = true;
            }
        }
        for (var j = (uintptr)0; j < n; (j, Δp) = (j + 1, Δp + size)) {
            if (freemark[j]) {
                freemark[j] = false;
                continue;
            }
            dumpobj((@unsafe.Pointer)Δp, size, makeheapobjbv(Δp, size));
        }
    }
}

internal static void dumpparams() {
    dumpint(tagParams);
    ref var x = ref heap<uintptr>(out var Ꮡx);
    x = (uintptr)1;
    if (~Ꮡx.Reinterpret<uintptr, byte>() == 1){
        dumpbool(false); // little-endian ptrs
    } else {
        dumpbool(true); // big-endian ptrs
    }
    dumpint(goarch.PtrSize);
    uintptr arenaStart = default!;
    uintptr arenaEnd = default!;
    foreach (var (i1, _) in mheap_.arenas) {
        if (mheap_.arenas[i1] == nil) {
            continue;
        }
        foreach (var (i, ha) in mheap_.arenas[i1].Value) {
            if (ha == nil) {
                continue;
            }
            var @base = arenaBase((arenaIdx)((((arenaIdx)(nuint)i1) << (int)(arenaL1Shift)) | ((arenaIdx)(nuint)i)));
            if (arenaStart == 0 || @base < arenaStart) {
                arenaStart = @base;
            }
            if (@base + (uintptr)heapArenaBytes > arenaEnd) {
                arenaEnd = @base + (uintptr)heapArenaBytes;
            }
        }
    }
    dumpint((uint64)arenaStart);
    dumpint((uint64)arenaEnd);
    dumpstr(goarch.GOARCH);
    dumpstr(buildVersion);
    dumpint((uint64)ncpu);
}

internal static void itab_callback(ж<itab> Ꮡtab) {
    ref var tab = ref Ꮡtab.DerefOrNull();

    var t = tab.Type;
    dumptype(t);
    dumpint(tagItab);
    dumpint((uint64)(uintptr)Ꮡtab);
    dumpint((uint64)(uintptr)t);
}

internal static void dumpitabs() {
    iterate_itabs(itab_callback);
}

internal static void dumpms() {
    for (var mp = allm; mp != nil; mp = mp.Value.alllink) {
        dumpint(tagOSThread);
        dumpint((uint64)(uintptr)mp);
        dumpint((uint64)(~mp).id);
        dumpint((~mp).procid);
    }
}

//go:systemstack
internal static void dumpmemstats(ref MemStats m) {
    assertWorldStopped();
    // These ints should be identical to the exported
    // MemStats structure and should be ordered the same
    // way too.
    dumpint(tagMemStats);
    dumpint(m.Alloc);
    dumpint(m.TotalAlloc);
    dumpint(m.Sys);
    dumpint(m.Lookups);
    dumpint(m.Mallocs);
    dumpint(m.Frees);
    dumpint(m.HeapAlloc);
    dumpint(m.HeapSys);
    dumpint(m.HeapIdle);
    dumpint(m.HeapInuse);
    dumpint(m.HeapReleased);
    dumpint(m.HeapObjects);
    dumpint(m.StackInuse);
    dumpint(m.StackSys);
    dumpint(m.MSpanInuse);
    dumpint(m.MSpanSys);
    dumpint(m.MCacheInuse);
    dumpint(m.MCacheSys);
    dumpint(m.BuckHashSys);
    dumpint(m.GCSys);
    dumpint(m.OtherSys);
    dumpint(m.NextGC);
    dumpint(m.LastGC);
    dumpint(m.PauseTotalNs);
    for (nint i = 0; i < 256; i++) {
        dumpint(m.PauseNs[i]);
    }
    dumpint((uint64)m.NumGC);
}

internal static void dumpmemprof_callback(ж<bucket> Ꮡb, uintptr nstk, ж<uintptr> Ꮡpstk, uintptr size, uintptr allocs, uintptr frees) {
    var stk = array<uintptr>.AliasPointer(Ꮡpstk, 100000);
    dumpint(tagMemProf);
    dumpint((uint64)(uintptr)Ꮡb);
    dumpint((uint64)size);
    dumpint((uint64)nstk);
    for (var i = (uintptr)0; i < nstk; i++) {
        var pc = stk.Value[i];
        var f = findfunc(pc);
        if (!f.valid()){
            array<byte> buf = new(64);
            nint n = len(buf);
            n--;
            buf[n] = (rune)')';
            if (pc == 0){
                n--;
                buf[n] = (rune)'0';
            } else {
                while (pc > 0) {
                    n--;
                    buf[n] = LiteralByteAt("0123456789abcdef"u8, (uintptr)(pc & 15));
                    pc >>= (int)(4);
                }
            }
            n--;
            buf[n] = (rune)'x';
            n--;
            buf[n] = (rune)'0';
            n--;
            buf[n] = (rune)'(';
            dumpslice(buf.slice(n));
            dumpstr("?"u8);
            dumpint(0);
        } else {
            dumpstr(funcname(f));
            if (i > 0 && pc > f.entry()) {
                pc--;
            }
            var (@file, line) = funcline(f, pc);
            dumpstr(@file);
            dumpint((uint64)line);
        }
    }
    dumpint((uint64)allocs);
    dumpint((uint64)frees);
}

internal static void dumpmemprof() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    iterate_memprof(dumpmemprof_callback);
    foreach (var (_, s) in mheap_.allspans) {
        if (s.of(mspan.Ꮡstate).get() != mSpanInUse) {
            continue;
        }
        for (var sp = s.Value.specials; sp != nil; sp = sp.Value.next) {
            if ((~sp).kind != _KindSpecialProfile) {
                continue;
            }
            var spp = sp.Reinterpret<special, specialprofile>();
            var Δp = s.@base() + (uintptr)(~spp).special.offset;
            dumpint(tagAllocSample);
            dumpint((uint64)Δp);
            dumpint((uint64)(uintptr)(~spp).b);
        }
    }
}

internal static ж<slice<byte>> Ꮡdumphdr = new StandardBox<slice<byte>>(slice<byte>("go1.7 heap dump\n"u8));
internal static ref slice<byte> dumphdr => ref Ꮡdumphdr.ValueSlot;

internal static void mdump(ref MemStats m) {
    assertWorldStopped();
    // make sure we're done sweeping
    foreach (var (_, s) in mheap_.allspans) {
        if (s.of(mspan.Ꮡstate).get() == mSpanInUse) {
            s.ensureSwept();
        }
    }
    memclrNoHeapPointers(@unsafe.Pointer.FromPinnedBox(Ꮡtypecache), /* unsafe.Sizeof(typecache) */ (uintptr)8192);
    dwrite(@unsafe.Pointer.FromPinnedBox(Ꮡ(dumphdr, 0)), (uintptr)len(dumphdr));
    dumpparams();
    dumpitabs();
    dumpobjs();
    dumpgs();
    dumpms();
    dumproots();
    dumpmemstats(ref m);
    dumpmemprof();
    dumpint(tagEOF);
    flush();
}

internal static void writeheapdump_m(uintptr fd, ref MemStats m) {
    assertWorldStopped();
    var gp = getg();
    casGToWaiting((~(~gp).m).curg, _Grunning, waitReasonDumpingHeap);
    // Set dump file.
    dumpfd = fd;
    // Call dump routine.
    mdump(ref m);
    // Reset dump file.
    dumpfd = 0;
    if (tmpbuf != default!) {
        sysFree(@unsafe.Pointer.FromPinnedBox(Ꮡ(tmpbuf, 0)), (uintptr)len(tmpbuf), Ꮡmemstats.of(mstats.Ꮡother_sys));
        tmpbuf = default!;
    }
    casgstatus((~(~gp).m).curg, _Gwaiting, _Grunning);
}

// dumpint() the kind & offset of each field in an object.
internal static void dumpfields(bitvector bvʗp) {
    ref var bv = ref heap(bvʗp, out var Ꮡbv);

    dumpbv(Ꮡbv, 0);
    dumpint(fieldKindEol);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapdumpOutOfMemoryˢ = "heapdump: out of memory"u8;

internal static unsafe bitvector makeheapobjbv(uintptr Δp, uintptr size) {
    // Extend the temp buffer if necessary.
    var nptr = size / (uintptr)goarch.PtrSize;
    if ((uintptr)len(tmpbuf) < nptr / 8 + 1) {
        if (tmpbuf != default!) {
            sysFree(@unsafe.Pointer.FromPinnedBox(Ꮡ(tmpbuf, 0)), (uintptr)len(tmpbuf), Ꮡmemstats.of(mstats.Ꮡother_sys));
        }
        var n = nptr / 8 + 1;
        @unsafe.Pointer pΔ1 = (uintptr)sysAlloc(n, Ꮡmemstats.of(mstats.Ꮡother_sys));
        if (pΔ1 == nil) {
            @throw(heapdumpOutOfMemoryˢ);
        }
        tmpbuf = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(pΔ1), (int)(n)));
    }
    // Convert heap bitmap to pointer bitmap.
    builtin.clear(tmpbuf.slice(0, (nint)(nptr / 8 + 1)));
    var s = spanOf(Δp);
    var tp = s.typePointersOf(Δp, size);
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(Δp + size); if (addr == 0) {
                break;
            }
        }
        var i = (addr - Δp) / (uintptr)goarch.PtrSize;
        tmpbuf[i / 8] |= (byte)((byte)(1 << (int)((i % 8))));
    }
    return new bitvector((int32)nptr, Ꮡ(tmpbuf, 0));
}

} // end runtime_package
