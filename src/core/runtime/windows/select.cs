// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// This file contains the implementation of Go select statements.
using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal const bool debugSelect = false;

// Select case descriptor.
// Known to compiler.
// Changes here must also be made in src/cmd/compile/internal/walk/select.go's scasetype.
[GoType] partial struct scase {
    internal ж<Δhchan> c;      // chan
    internal @unsafe.Pointer elem; // data element
}

internal static uintptr chansendpc;
internal static void initᴛchansendpc() { chansendpc = abi.FuncPCABIInternal(chansend); }
internal static uintptr chanrecvpc;
internal static void initᴛchanrecvpc() { chanrecvpc = abi.FuncPCABIInternal(chanrecv); }

internal static void selectsetpc(ref uintptr pc) {
    pc = getcallerpc();
}

internal static void sellock(slice<scase> scases, slice<uint16> lockorder) {
    ж<Δhchan> c = default!;
    foreach (var (_, o) in lockorder) {
        var c0 = scases[o].c;
        if (c0 != c) {
            c = c0;
            @lock(c.of(runtime_package.Δhchan.Ꮡlock));
        }
    }
}

internal static void selunlock(slice<scase> scases, slice<uint16> lockorder) {
    // We must be very careful here to not touch sel after we have unlocked
    // the last lock, because sel can be freed right after the last unlock.
    // Consider the following situation.
    // First M calls runtime·park() in runtime·selectgo() passing the sel.
    // Once runtime·park() has unlocked the last lock, another M makes
    // the G that calls select runnable again and schedules it for execution.
    // When the G runs on another M, it locks all the locks and frees sel.
    // Now if the first M touches sel, it will access freed memory.
    for (nint i = len(lockorder) - 1; i >= 0; i--) {
        var c = scases[lockorder[i]].c;
        if (i > 0 && c == scases[lockorder[i - 1]].c) {
            continue; // will unlock it on the next iteration
        }
        unlock(c.of(runtime_package.Δhchan.Ꮡlock));
    }
}

internal static bool selparkcommit(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // There are unlocked sudogs that point into gp's stack. Stack
    // copying must lock the channels of those sudogs.
    // Set activeStackChans here instead of before we try parking
    // because we could self-deadlock in stack growth on a
    // channel lock.
    gp.activeStackChans = true;
    // Mark that it's safe for stack shrinking to occur now,
    // because any thread acquiring this G's stack for shrinking
    // is guaranteed to observe activeStackChans after this store.
    Ꮡgp.of(g.ᏑparkingOnChan).Store(false);
    // Make sure we unlock after setting activeStackChans and
    // unsetting parkingOnChan. The moment we unlock any of the
    // channel locks we risk gp getting readied by a channel operation
    // and so gp could continue running before everything before the
    // unlock is visible (even to gp itself).
    // This must not access gp's stack (see gopark). In
    // particular, it must not access the *hselect. That's okay,
    // because by the time this is called, gp.waiting has all
    // channels in lock order.
    ж<Δhchan> lastc = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.Value.waitlink) {
        if ((~sg).c != lastc && lastc != nil) {
            // As soon as we unlock the channel, fields in
            // any sudog with that channel may change,
            // including c and waitlink. Since multiple
            // sudogs may have the same channel, we unlock
            // only after we've passed the last instance
            // of a channel.
            unlock(lastc.of(runtime_package.Δhchan.Ꮡlock));
        }
        lastc = sg.Value.c;
    }
    if (lastc != nil) {
        unlock(lastc.of(runtime_package.Δhchan.Ꮡlock));
    }
    return true;
}

internal static void block() {
    gopark(default!, nil, waitReasonSelectNoCases, traceBlockForever, 1); // forever
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectBrokenSortˢ = "select: broken sort"u8;
internal static readonly @string gpWaitingNilˢ = "gp.waiting != nil"u8;
internal static readonly @string selectgoBadWakeupˢ = "selectgo: bad wakeup"u8;

// selectgo implements the select statement.
//
// cas0 points to an array of type [ncases]scase, and order0 points to
// an array of type [2*ncases]uint16 where ncases must be <= 65536.
// Both reside on the goroutine's stack (regardless of any escaping in
// selectgo).
//
// For race detector builds, pc0 points to an array of type
// [ncases]uintptr (also on the stack); for other builds, it's set to
// nil.
//
// selectgo returns the index of the chosen scase, which matches the
// ordinal position of its respective select{recv,send,default} call.
// Also, if the chosen scase was a receive operation, it reports whether
// a value was received.
internal static (nint, bool) selectgo(ж<scase> Ꮡcas0, ж<uint16> Ꮡorder0, ж<uintptr> Ꮡpc0, nint nsends, nint nrecvs, bool block) {
    ref var cas0 = ref Ꮡcas0.DerefOrNull();

    if (debugSelect) {
        print((@string)"select: cas0="u8, Ꮡcas0.OrTypedNil(), (@string)"\n"u8);
    }
    // NOTE: In order to maintain a lean stack size, the number of scases
    // is capped at 65536.
    var cas1 = array<scase>.AliasPointer(Ꮡcas0, 65536);
    var order1 = array<uint16>.AliasPointer(Ꮡorder0, 131072);
    nint ncases = nsends + nrecvs;
    var scases = (~cas1).slice(-1, ncases, ncases);
    var pollorder = (~order1).slice(-1, ncases, ncases);
    var lockorder = (~order1)[(int)(ncases)..].slice(-1, ncases, ncases);
    // NOTE: pollorder/lockorder's underlying array was not zero-initialized by compiler.
    // Even when raceenabled is true, there might be select
    // statements in packages compiled without -race (e.g.,
    // ensureSigM in runtime/signal_unix.go).
    slice<uintptr> pcs = default!;
    if (raceenabled && Ꮡpc0 != nil) {
        var pc1 = array<uintptr>.AliasPointer(Ꮡpc0, 65536);
        pcs = (~pc1).slice(-1, ncases, ncases);
    }
    var pcsʗ1 = pcs;
    uintptr casePC(nint casiΔ1) {
        if (pcsʗ1 == default!) {
            return (uintptr)(0);
        }
        return pcsʗ1[casiΔ1];
    }
    int64 t0 = default!;
    if (blockprofilerate > 0) {
        t0 = cputicks();
    }
    // The compiler rewrites selects that statically have
    // only 0 or 1 cases plus default into simpler constructs.
    // The only way we can end up with such small sel.ncase
    // values here is for a larger select in which most channels
    // have been nilled out. The general code handles those
    // cases correctly, and they are rare enough not to bother
    // optimizing (and needing to test).
    // generate permuted order
    nint norder = 0;
    foreach (var (i, _) in scases) {
        var casΔ1 = Ꮡ(scases, i);
        // Omit cases without channels from the poll and lock orders.
        if ((~casΔ1).c == nil) {
            casΔ1.Value.elem = default!; // allow GC
            continue;
        }
        if ((~(~casΔ1).c).timer != nil) {
            (~(~casΔ1).c).timer.maybeRunChan();
        }
        var j = cheaprandn((uint32)(norder + 1));
        pollorder[norder] = pollorder[(nint)(j)];
        pollorder[(nint)(j)] = (uint16)i;
        norder++;
    }
    pollorder = pollorder[..(int)(norder)];
    lockorder = lockorder[..(int)(norder)];
    // sort the cases by Hchan address to get the locking order.
    // simple heap sort, to guarantee n log n time and constant stack footprint.
    foreach (var (i, _) in lockorder) {
        nint j = i;
        // Start with the pollorder to permute cases on the same channel.
        var cΔ1 = scases[pollorder[i]].c;
        while (j > 0 && scases[lockorder[(j - 1) / 2]].c.sortkey() < cΔ1.sortkey()) {
            nint kΔ1 = (j - 1) / 2;
            lockorder[j] = lockorder[kΔ1];
            j = kΔ1;
        }
        lockorder[j] = pollorder[i];
    }
    for (nint i = len(lockorder) - 1; i >= 0; i--) {
        var o = lockorder[i];
        var cΔ2 = scases[o].c;
        lockorder[i] = lockorder[0];
        nint j = 0;
        while (ᐧ) {
            nint kΔ2 = j * 2 + 1;
            if (kΔ2 >= i) {
                break;
            }
            if (kΔ2 + 1 < i && scases[lockorder[kΔ2]].c.sortkey() < scases[lockorder[kΔ2 + 1]].c.sortkey()) {
                kΔ2++;
            }
            if (cΔ2.sortkey() < scases[lockorder[kΔ2]].c.sortkey()) {
                lockorder[j] = lockorder[kΔ2];
                j = kΔ2;
                continue;
            }
            break;
        }
        lockorder[j] = o;
    }
    if (debugSelect) {
        for (nint i = 0; i + 1 < len(lockorder); i++) {
            if (scases[lockorder[i]].c.sortkey() > scases[lockorder[i + 1]].c.sortkey()) {
                print((@string)"i="u8, i, (@string)" x="u8, lockorder[i], (@string)" y="u8, lockorder[i + 1], (@string)"\n"u8);
                @throw(selectBrokenSortˢ);
            }
        }
    }
    // lock all the channels involved in the select
    sellock(scases, lockorder);
    ж<g> gp = default!;
    ж<sudog> sg = default!;
    ж<Δhchan> c = default!;
    ж<scase> k = default!;
    ж<sudog> sglist = default!;
    ж<sudog> sgnext = default!;
    @unsafe.Pointer qp = default!;
    ж<ж<sudog>> nextp = default!;
    // pass 1 - look for something already waiting
    nint casi = default!;
    ж<scase> cas = default!;
    bool caseSuccess = default!;
    int64 caseReleaseTime = -1;
    bool recvOK = default!;
    foreach (var (_, casei) in pollorder) {
        casi = (nint)casei;
        cas = Ꮡ(scases, casi);
        c = cas.Value.c;
        if (casi >= nsends){
            sg = c.of(runtime_package.Δhchan.Ꮡsendq).dequeue();
            if (sg != nil) {
                goto recv;
            }
            if ((~c).qcount > 0) {
                goto bufrecv;
            }
            if ((~c).closed != 0) {
                goto rclose;
            }
        } else {
            if (raceenabled) {
                racereadpc((uintptr)c.raceaddr(), casePC(casi), chansendpc);
            }
            if ((~c).closed != 0) {
                goto sclose;
            }
            sg = c.of(runtime_package.Δhchan.Ꮡrecvq).dequeue();
            if (sg != nil) {
                goto send;
            }
            if ((~c).qcount < (~c).dataqsiz) {
                goto bufsend;
            }
        }
    }
    if (!block) {
        selunlock(scases, lockorder);
        casi = -1;
        goto retc;
    }
    // pass 2 - enqueue on all chans
    gp = getg();
    if ((~gp).waiting != nil) {
        @throw(gpWaitingNilˢ);
    }
    nextp = gp.of(g.Ꮡwaiting);
    foreach (var (_, casei) in lockorder) {
        casi = (nint)casei;
        cas = Ꮡ(scases, casi);
        c = cas.Value.c;
        var sgΔ1 = acquireSudog();
        sgΔ1.Value.g = gp;
        sgΔ1.Value.isSelect = true;
        // No stack splits between assigning elem and enqueuing
        // sg on gp.waiting where copystack can find it.
        sgΔ1.Value.elem = cas.Value.elem;
        sgΔ1.Value.releasetime = 0;
        if (t0 != 0) {
            sgΔ1.Value.releasetime = -1;
        }
        sgΔ1.Value.c = c;
        // Construct waiting list in lock order.
        nextp.ValueSlot = sgΔ1;
        nextp = sgΔ1.of(sudog.Ꮡwaitlink);
        if (casi < nsends){
            c.of(runtime_package.Δhchan.Ꮡsendq).enqueue(sgΔ1);
        } else {
            c.of(runtime_package.Δhchan.Ꮡrecvq).enqueue(sgΔ1);
        }
        if ((~c).timer != nil) {
            blockTimerChan(ref (c).DerefOrNull());
        }
    }
    // wait for someone to wake us up
    gp.Value.param = default!;
    // Signal to anyone trying to shrink our stack that we're about
    // to park on a channel. The window between when this G's status
    // changes and when we set gp.activeStackChans is not safe for
    // stack shrinking.
    gp.of(g.ᏑparkingOnChan).Store(true);
    gopark(selparkcommit, nil, waitReasonSelect, traceBlockSelect, 1);
    gp.Value.activeStackChans = false;
    sellock(scases, lockorder);
    gp.of(g.ᏑselectDone).Store(0);
    sg = (ж<sudog>)(uintptr)((~gp).param);
    gp.Value.param = default!;
    // pass 3 - dequeue from unsuccessful chans
    // otherwise they stack up on quiet channels
    // record the successful case, if any.
    // We singly-linked up the SudoGs in lock order.
    casi = -1;
    cas = default!;
    caseSuccess = false;
    sglist = gp.Value.waiting;
    // Clear all elem before unlinking from gp.waiting.
    for (var sg1 = gp.Value.waiting; sg1 != nil; sg1 = sg1.Value.waitlink) {
        sg1.Value.isSelect = false;
        sg1.Value.elem = default!;
        sg1.Value.c = default!;
    }
    gp.Value.waiting = default!;
    foreach (var (_, casei) in lockorder) {
        k = Ꮡ(scases, casei);
        if ((~(~k).c).timer != nil) {
            unblockTimerChan(ref ((~k).c).DerefOrNull());
        }
        if (sg == sglist){
            // sg has already been dequeued by the G that woke us up.
            casi = (nint)casei;
            cas = k;
            caseSuccess = sglist.Value.success;
            if ((~sglist).releasetime > 0) {
                caseReleaseTime = sglist.Value.releasetime;
            }
        } else {
            c = k.Value.c;
            if ((nint)casei < nsends){
                c.of(runtime_package.Δhchan.Ꮡsendq).dequeueSudoG(sglist);
            } else {
                c.of(runtime_package.Δhchan.Ꮡrecvq).dequeueSudoG(sglist);
            }
        }
        sgnext = sglist.Value.waitlink;
        sglist.Value.waitlink = default!;
        releaseSudog(sglist);
        sglist = sgnext;
    }
    if (cas == nil) {
        @throw(selectgoBadWakeupˢ);
    }
    c = cas.Value.c;
    if (debugSelect) {
        print((@string)"wait-return: cas0="u8, Ꮡcas0.OrTypedNil(), (@string)" c="u8, c.OrTypedNil(), (@string)" cas="u8, cas.OrTypedNil(), (@string)" send="u8, casi < nsends, (@string)"\n"u8);
    }
    if (casi < nsends){
        if (!caseSuccess) {
            goto sclose;
        }
    } else {
        recvOK = caseSuccess;
    }
    if (raceenabled) {
        if (casi < nsends){
            raceReadObjectPC(ref ((~c).elemtype).DerefOrNull(), (~cas).elem, casePC(casi), chansendpc);
        } else 
        if ((~cas).elem != nil) {
            raceWriteObjectPC(ref ((~c).elemtype).DerefOrNull(), (~cas).elem, casePC(casi), chanrecvpc);
        }
    }
    if (msanenabled) {
        if (casi < nsends){
            msanread((~cas).elem, (~(~c).elemtype).Size_);
        } else 
        if ((~cas).elem != nil) {
            msanwrite((~cas).elem, (~(~c).elemtype).Size_);
        }
    }
    if (asanenabled) {
        if (casi < nsends){
            asanread((~cas).elem, (~(~c).elemtype).Size_);
        } else 
        if ((~cas).elem != nil) {
            asanwrite((~cas).elem, (~(~c).elemtype).Size_);
        }
    }
    selunlock(scases, lockorder);
    goto retc;
bufrecv:
    if (raceenabled) {
        // can receive from buffer
        if ((~cas).elem != nil) {
            raceWriteObjectPC(ref ((~c).elemtype).DerefOrNull(), (~cas).elem, casePC(casi), chanrecvpc);
        }
        racenotify(c, (~c).recvx, nil);
    }
    if (msanenabled && (~cas).elem != nil) {
        msanwrite((~cas).elem, (~(~c).elemtype).Size_);
    }
    if (asanenabled && (~cas).elem != nil) {
        asanwrite((~cas).elem, (~(~c).elemtype).Size_);
    }
    recvOK = true;
    qp = (uintptr)chanbuf(c, (~c).recvx);
    if ((~cas).elem != nil) {
        typedmemmove((~c).elemtype, (~cas).elem, qp);
    }
    typedmemclr((~c).elemtype, qp);
    c.Value.recvx++;
    if ((~c).recvx == (~c).dataqsiz) {
        c.Value.recvx = 0;
    }
    c.Value.qcount--;
    selunlock(scases, lockorder);
    goto retc;
bufsend:
    if (raceenabled) {
        // can send to buffer
        racenotify(c, (~c).sendx, nil);
        raceReadObjectPC(ref ((~c).elemtype).DerefOrNull(), (~cas).elem, casePC(casi), chansendpc);
    }
    if (msanenabled) {
        msanread((~cas).elem, (~(~c).elemtype).Size_);
    }
    if (asanenabled) {
        asanread((~cas).elem, (~(~c).elemtype).Size_);
    }
    typedmemmove((~c).elemtype, (uintptr)chanbuf(c, (~c).sendx), (~cas).elem);
    c.Value.sendx++;
    if ((~c).sendx == (~c).dataqsiz) {
        c.Value.sendx = 0;
    }
    c.Value.qcount++;
    selunlock(scases, lockorder);
    goto retc;
recv:
    var lockorderʗ1 = lockorder;
    var scasesʗ1 = scases;
    recv(c, // can receive from sleeping sender (sg)
 sg, (~cas).elem, () => {
        selunlock(scasesʗ1, lockorderʗ1);
    }, 2);
    if (debugSelect) {
        print((@string)"syncrecv: cas0="u8, Ꮡcas0.OrTypedNil(), (@string)" c="u8, c.OrTypedNil(), (@string)"\n"u8);
    }
    recvOK = true;
    goto retc;
rclose:
    selunlock(scases, // read at end of closed channel
 lockorder);
    recvOK = false;
    if ((~cas).elem != nil) {
        typedmemclr((~c).elemtype, (~cas).elem);
    }
    if (raceenabled) {
        raceacquire((uintptr)c.raceaddr());
    }
    goto retc;
send:
    if (raceenabled) {
        // can send to a sleeping receiver (sg)
        raceReadObjectPC(ref ((~c).elemtype).DerefOrNull(), (~cas).elem, casePC(casi), chansendpc);
    }
    if (msanenabled) {
        msanread((~cas).elem, (~(~c).elemtype).Size_);
    }
    if (asanenabled) {
        asanread((~cas).elem, (~(~c).elemtype).Size_);
    }
    var lockorderʗ2 = lockorder;
    var scasesʗ2 = scases;
    send(c, sg, (~cas).elem, () => {
        selunlock(scasesʗ2, lockorderʗ2);
    }, 2);
    if (debugSelect) {
        print((@string)"syncsend: cas0="u8, Ꮡcas0.OrTypedNil(), (@string)" c="u8, c.OrTypedNil(), (@string)"\n"u8);
    }
    goto retc;
retc:
    if (caseReleaseTime > 0) {
        blockevent(caseReleaseTime - t0, 1);
    }
    return (casi, recvOK);
sclose:
    selunlock(scases, // send on closed channel
 lockorder);
    throw panic(((plainError)(@string)sendOnClosedChannelˢ));
}

internal static uintptr sortkey(this ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    return (uintptr)(uintptr)@unsafe.Pointer.FromRef(ref c);
}

// A runtimeSelect is a single case passed to rselect.
// This must match ../reflect/value.go:/runtimeSelect
[GoType] partial struct runtimeSelect {
    internal selectDir dir;
    internal @unsafe.Pointer typ; // channel type (not used here)
    internal ж<Δhchan> ch;      // channel
    internal @unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
}

[GoType("num:nint")] partial struct selectDir;

internal static selectDir _ᴛ3ʗ => /* iota */ 0;
internal static selectDir selectSend => 1; // case Chan <- Send
internal static selectDir selectRecv => 2; // case <-Chan:
internal static selectDir selectDefault => 3; // default

//go:linkname reflect_rselect reflect.rselect
internal static (nint, bool) reflect_rselect(slice<runtimeSelect> cases) {
    if (len(cases) == 0) {
        block();
    }
    var sel = new slice<scase>(len(cases));
    var orig = new slice<nint>(len(cases));
    nint nsends = 0;
    nint nrecvs = 0;
    nint dflt = -1;
    foreach (var (i, rc) in cases) {
        nint j = default!;
        var exprᴛ1 = rc.dir;
        if (exprᴛ1 == selectDefault) {
            dflt = i;
            continue;
        }
        else if (exprᴛ1 == selectSend) {
            j = nsends;
            nsends++;
        }
        else if (exprᴛ1 == selectRecv) {
            nrecvs++;
            j = len(cases) - nrecvs;
        }

        sel[j] = new scase(c: rc.ch, elem: rc.val);
        orig[j] = i;
    }
    // Only a default case.
    if (nsends + nrecvs == 0) {
        return (dflt, false);
    }
    // Compact sel and orig if necessary.
    if (nsends + nrecvs < len(cases)) {
        copy(sel[(int)(nsends)..], sel[(int)(len(cases) - nrecvs)..]);
        copy(orig[(int)(nsends)..], orig[(int)(len(cases) - nrecvs)..]);
    }
    var order = new slice<uint16>(2 * (nsends + nrecvs));
    ж<uintptr> pc0 = default!;
    if (raceenabled) {
        var pcs = new slice<uintptr>(nsends + nrecvs);
        foreach (var (i, _) in pcs) {
            selectsetpc(ref pcs[i]);
        }
        pc0 = Ꮡ(pcs, 0);
    }
    var (chosen, recvOK) = selectgo(Ꮡ(sel, 0), Ꮡ(order, 0), pc0, nsends, nrecvs, dflt == -1);
    // Translate chosen back to caller's ordering.
    if (chosen < 0){
        chosen = dflt;
    } else {
        chosen = orig[chosen];
    }
    return (chosen, recvOK);
}

[GoRecv] internal static void dequeueSudoG(this ref waitq q, ж<sudog> Ꮡsgp) {
    ref var sgp = ref Ꮡsgp.DerefOrNull();

    var x = sgp.prev;
    var y = sgp.next;
    if (x != nil) {
        if (y != nil) {
            // middle of queue
            x.Value.next = y;
            y.Value.prev = x;
            sgp.next = default!;
            sgp.prev = default!;
            return;
        }
        // end of queue
        x.Value.next = default!;
        q.last = x;
        sgp.prev = default!;
        return;
    }
    if (y != nil) {
        // start of queue
        y.Value.prev = default!;
        q.first = y;
        sgp.next = default!;
        return;
    }
    // x==y==nil. Either sgp is the only element in the queue,
    // or it has already been removed. Use q.first to disambiguate.
    if (q.first == Ꮡsgp) {
        q.first = default!;
        q.last = default!;
    }
}

} // end runtime_package
