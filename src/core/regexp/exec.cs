// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using io = io_package;
using syntax = regexp.syntax_package;
using Δsync = sync_package;
using regexp;

partial class regexp_package {

// A queue is a 'sparse array' holding pending threads of execution.
// See https://research.swtch.com/2008/03/using-uninitialized-memory-for-fun-and.html
[GoType] partial struct queue {
    internal slice<uint32> sparse;
    internal slice<entry> dense;
}

// An entry is an entry on a queue.
// It holds both the instruction pc and the actual thread.
// Some queue entries are just place holders so that the machine
// knows it has considered that pc. Such entries have t == nil.
[GoType] partial struct entry {
    internal uint32 pc;
    internal ж<thread> t;
}

// A thread is the state of a single path through the machine:
// an instruction and a corresponding capture array.
// See https://swtch.com/~rsc/regexp/regexp2.html
[GoType] partial struct thread {
    internal ж<syntax.Inst> inst;
    internal slice<nint> cap;
}

// A machine holds all the state during an NFA simulation for p.
[GoType] partial struct machine {
    internal ж<Regexp> re;   // corresponding Regexp
    internal ж<syntax.Prog> p; // compiled program
    internal queue q0, q1;        // two queues for runq, nextq
    internal slice<ж<thread>> pool; // pool of available threads
    internal bool matched;         // whether a match was found
    internal slice<nint> matchcap;  // capture information for the match
    internal inputs inputs;
}

[GoType] partial struct inputs {
    // cached inputs, to avoid allocation
    internal inputBytes bytes;
    internal inputString @string;
    internal inputReader reader;
}

internal static input newBytes(this ж<inputs> Ꮡi, slice<byte> b) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.bytes.str = b;
    return new inputBytesжinput(Ꮡi.of(inputs.Ꮡbytes));
}

internal static input newString(this ж<inputs> Ꮡi, @string s) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.@string.str = s;
    return new inputStringжinput(Ꮡi.of(inputs.Ꮡstring));
}

internal static input newReader(this ж<inputs> Ꮡi, io.RuneReader r) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.reader.r = r;
    i.reader.atEOT = false;
    i.reader.pos = 0;
    return new inputReaderжinput(Ꮡi.of(inputs.Ꮡreader));
}

[GoRecv] internal static void clear(this ref inputs i) {
    // We need to clear 1 of these.
    // Avoid the expense of clearing the others (pointer write barrier).
    if (i.bytes.str != default!){
        i.bytes.str = default!;
    } else 
    if (i.reader.r != default!){
        i.reader.r = default!;
    } else {
        i.@string.str = ""u8;
    }
}

internal static (input, nint) init(this ж<inputs> Ꮡi, io.RuneReader r, slice<byte> b, @string s) {
    if (r != default!) {
        return (Ꮡi.newReader(r), 0);
    }
    if (b != default!) {
        return (Ꮡi.newBytes(b), len(b));
    }
    return (Ꮡi.newString(s), len(s));
}

[GoRecv] internal static void init(this ref machine m, nint ncap) {
    foreach (var (_, t) in m.pool) {
        t.Value.cap = (~t).cap.slice(0, ncap);
    }
    m.matchcap = m.matchcap.slice(0, ncap);
}

// alloc allocates a new thread with the given instruction.
// It uses the free pool if possible.
[GoRecv] internal static ж<thread> alloc(this ref machine m, ж<syntax.Inst> Ꮡi) {
    ж<thread> t = default!;
    {
        nint n = len(m.pool); if (n > 0){
            t = m.pool[n - 1];
            m.pool = m.pool.slice(0, n - 1);
        } else {
            t = @new<thread>();
            t.Value.cap = new slice<nint>(len(m.matchcap), cap(m.matchcap));
        }
    }
    t.Value.inst = Ꮡi;
    return t;
}

[GoType("num:uint64")] partial struct lazyFlag;

internal static lazyFlag newLazyFlag(rune r1, rune r2) {
    return ((lazyFlag)((uint64)(((uint64)r1 << (int)(32)) | (uint64)(uint32)r2)));
}

internal static bool match(this lazyFlag f, syntax.EmptyOp op) {
    if (op == 0) {
        return true;
    }
    var r1 = (rune)(uint64)((f >> (int)(32)));
    if ((syntax.EmptyOp)(op & syntax.EmptyBeginLine) != 0) {
        if (r1 != (rune)'\n' && r1 >= 0) {
            return false;
        }
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyBeginLine));
    }
    if ((syntax.EmptyOp)(op & syntax.EmptyBeginText) != 0) {
        if (r1 >= 0) {
            return false;
        }
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyBeginText));
    }
    if (op == 0) {
        return true;
    }
    var r2 = (rune)(uint64)f;
    if ((syntax.EmptyOp)(op & syntax.EmptyEndLine) != 0) {
        if (r2 != (rune)'\n' && r2 >= 0) {
            return false;
        }
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyEndLine));
    }
    if ((syntax.EmptyOp)(op & syntax.EmptyEndText) != 0) {
        if (r2 >= 0) {
            return false;
        }
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyEndText));
    }
    if (op == 0) {
        return true;
    }
    if (syntax.IsWordChar(r1) != syntax.IsWordChar(r2)){
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyWordBoundary));
    } else {
        op &= unchecked((syntax.EmptyOp)~(syntax.EmptyOp)(syntax.EmptyNoWordBoundary));
    }
    return op == 0;
}

// match runs the machine over the input starting at pos.
// It reports whether a match was found.
// If so, m.matchcap holds the submatch information.
internal static bool match(this ж<machine> Ꮡm, input i, nint pos) {
    ref var m = ref Ꮡm.DerefOrNull();

    var startCond = m.re.Value.cond;
    if (startCond == ~((syntax.EmptyOp)((syntax.EmptyOp)0))) {
        // impossible
        return false;
    }
    m.matched = false;
    foreach (var (iΔ1, _) in m.matchcap) {
        m.matchcap[iΔ1] = -1;
    }
    var (runq, nextq) = (Ꮡm.of(machine.Ꮡq0), Ꮡm.of(machine.Ꮡq1));
    var (r, r1) = (endOfText, endOfText);
    nint width = 0;
    nint width1 = 0;
    (r, width) = i.step(pos);
    if (r != endOfText) {
        (r1, width1) = i.step(pos + width);
    }
    ref var flag = ref heap(new lazyFlag(), out var Ꮡflag);
    if (pos == 0){
        flag = newLazyFlag(-1, r);
    } else {
        flag = i.context(pos);
    }
    while (ᐧ) {
        if (len((~runq).dense) == 0) {
            if ((syntax.EmptyOp)(startCond & syntax.EmptyBeginText) != 0 && pos != 0) {
                // Anchored match, past beginning of text.
                break;
            }
            if (m.matched) {
                // Have match; finished exploring alternatives.
                break;
            }
            if (len((~m.re).prefix) > 0 && r1 != (~m.re).prefixRune && i.canCheckPrefix()) {
                // Match requires literal prefix; fast search for it.
                nint advance = i.index(m.re, pos);
                if (advance < 0) {
                    break;
                }
                pos += advance;
                (r, width) = i.step(pos);
                (r1, width1) = i.step(pos + width);
            }
        }
        if (!m.matched) {
            if (len(m.matchcap) > 0) {
                m.matchcap[0] = pos;
            }
            m.add(runq, (uint32)(~m.p).Start, pos, m.matchcap, Ꮡflag, nil);
        }
        flag = newLazyFlag(r, r1);
        m.step(runq, nextq, pos, pos + width, r, Ꮡflag);
        if (width == 0) {
            break;
        }
        if (len(m.matchcap) == 0 && m.matched) {
            // Found a match and not paying attention
            // to where it is, so any match will do.
            break;
        }
        pos += width;
        (r, width) = (r1, width1);
        if (r != endOfText) {
            (r1, width1) = i.step(pos + width);
        }
        (runq, nextq) = (nextq, runq);
    }
    m.clear(nextq);
    return m.matched;
}

// clear frees all threads on the thread queue.
[GoRecv] internal static void clear(this ref machine m, ж<queue> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    foreach (var (_, d) in q.dense) {
        if (d.t != nil) {
            m.pool = append(m.pool, d.t);
        }
    }
    q.dense = q.dense[..0];
}

// step executes one step of the machine, running each of the threads
// on runq and appending new threads to nextq.
// The step processes the rune c (which may be endOfText),
// which starts at position pos and ends at nextPos.
// nextCond gives the setting for the empty-width flags after c.
[GoRecv] internal static void step(this ref machine m, ж<queue> Ꮡrunq, ж<queue> Ꮡnextq, nint pos, nint nextPos, rune c, ж<lazyFlag> ᏑnextCond) {
    ref var runq = ref Ꮡrunq.DerefOrNull();

    var longest = m.re.Value.longest;
    for (nint j = 0; j < len(runq.dense); j++) {
        var d = Ꮡ(runq.dense, j);
        var t = d.Value.t;
        if (t == nil) {
            continue;
        }
        if (longest && m.matched && len((~t).cap) > 0 && m.matchcap[0] < (~t).cap[0]) {
            m.pool = append(m.pool, t);
            continue;
        }
        var i = t.Value.inst;
        var add = false;
        var exprᴛ1 = (~i).Op;
        if (exprᴛ1 == syntax.InstMatch) {
            if (len((~t).cap) > 0 && (!longest || !m.matched || m.matchcap[1] < pos)) {
                t.Value.cap[1] = pos;
                copy(m.matchcap, (~t).cap);
            }
            if (!longest) {
                // First-match mode: cut off all lower-priority threads.
                foreach (var (_, dΔ2) in runq.dense.slice(j + 1)) {
                    if (dΔ2.t != nil) {
                        m.pool = append(m.pool, dΔ2.t);
                    }
                }
                runq.dense = runq.dense[..0];
            }
            m.matched = true;
        }
        else if (exprᴛ1 == syntax.InstRune) {
            add = i.MatchRune(c);
        }
        else if (exprᴛ1 == syntax.InstRune1) {
            add = c == (~i).Rune[0];
        }
        else if (exprᴛ1 == syntax.InstRuneAny) {
            add = true;
        }
        else if (exprᴛ1 == syntax.InstRuneAnyNotNL) {
            add = c != (rune)'\n';
        }
        else { /* default: */
            throw panic("bad inst");
        }

        if (add) {
            t = m.add(Ꮡnextq, (~i).Out, nextPos, (~t).cap, ᏑnextCond, t);
        }
        if (t != nil) {
            m.pool = append(m.pool, t);
        }
    }
    runq.dense = runq.dense[..0];
}

// add adds an entry to q for pc, unless the q already has such an entry.
// It also recursively adds an entry for all instructions reachable from pc by following
// empty-width conditions satisfied by cond.  pos gives the current position
// in the input.
[GoRecv] internal static ж<thread> add(this ref machine m, ж<queue> Ꮡq, uint32 pc, nint pos, slice<nint> cap, ж<lazyFlag> Ꮡcond, ж<thread> Ꮡt) {
    ref var q = ref Ꮡq.DerefOrNull();
    ref var cond = ref Ꮡcond.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

Again:
    if (pc == 0) {
        return Ꮡt;
    }
    {
        var jΔ1 = q.sparse[pc]; if (jΔ1 < (uint32)len(q.dense) && q.dense[jΔ1].pc == pc) {
            return Ꮡt;
        }
    }
    nint j = len(q.dense);
    q.dense = q.dense.slice(0, j + 1);
    var d = Ꮡ(q.dense, j);
    d.Value.t = default!;
    d.Value.pc = pc;
    q.sparse[pc] = (uint32)j;
    var i = Ꮡ((~m.p).Inst, pc);
    var exprᴛ1 = (~i).Op;
    if (exprᴛ1 == syntax.InstFail) {
    }
    else if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch) {
        Ꮡt = m.add(Ꮡq, // nothing
 (~i).Out, pos, cap, Ꮡcond, Ꮡt); t = ref Ꮡt.DerefOrNull();
        pc = i.Value.Arg;
        goto Again;
    }
    else if (exprᴛ1 == syntax.InstEmptyWidth) {
        if (cond.match(((syntax.EmptyOp)(uint8)(~i).Arg))) {
            pc = i.Value.Out;
            goto Again;
        }
    }
    else if (exprᴛ1 == syntax.InstNop) {
        pc = i.Value.Out;
        goto Again;
    }
    else if (exprᴛ1 == syntax.InstCapture) {
        if ((nint)(~i).Arg < len(cap)){
            nint opos = cap[(~i).Arg];
            cap[(~i).Arg] = pos;
            m.add(Ꮡq, (~i).Out, pos, cap, Ꮡcond, nil);
            cap[(~i).Arg] = opos;
        } else {
            pc = i.Value.Out;
            goto Again;
        }
    }
    else if (exprᴛ1 == syntax.InstMatch || exprᴛ1 == syntax.InstRune || exprᴛ1 == syntax.InstRune1 || exprᴛ1 == syntax.InstRuneAny || exprᴛ1 == syntax.InstRuneAnyNotNL) {
        if (Ꮡt == nil){
            Ꮡt = m.alloc(i); t = ref Ꮡt.DerefOrNull();
        } else {
            t.inst = i;
        }
        if (len(cap) > 0 && Ꮡ(t.cap, 0) != Ꮡ(cap, 0)) {
            copy(t.cap, cap);
        }
        d.Value.t = Ꮡt;
        Ꮡt = default!; t = ref Ꮡt.DerefOrNull();
    }
    else { /* default: */
        throw panic("unhandled");
    }

    return Ꮡt;
}

[GoType] partial struct onePassMachine {
    internal inputs inputs;
    internal slice<nint> matchcap;
}

internal static ж<Δsync.Pool> ᏑonePassPool = new StandardBox<Δsync.Pool>(default(Δsync.Pool));
internal static ref Δsync.Pool onePassPool => ref ᏑonePassPool.Value;

internal static ж<onePassMachine> newOnePassMachine() {
    var (m, ok) = ᏑonePassPool.Get()._<ж<onePassMachine>>(ᐧ);
    if (!ok) {
        m = @new<onePassMachine>();
    }
    return m;
}

internal static void freeOnePassMachine(ж<onePassMachine> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.inputs.clear();
    ᏑonePassPool.Put(Ꮡm.OrTypedNil());
}

// doOnePass implements r.doExecute using the one-pass execution engine.
internal static slice<nint> doOnePass(this ж<Regexp> Ꮡre, io.RuneReader ir, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) {
    ref var re = ref Ꮡre.DerefOrNull();

    var startCond = re.cond;
    if (startCond == ~((syntax.EmptyOp)((syntax.EmptyOp)0))) {
        // impossible
        return default!;
    }
    var m = newOnePassMachine();
    if (cap((~m).matchcap) < ncap){
        m.Value.matchcap = new slice<nint>(ncap);
    } else {
        m.Value.matchcap = (~m).matchcap.slice(0, ncap);
    }
    var matched = false;
    foreach (var (iΔ1, _) in (~m).matchcap) {
        m.Value.matchcap[iΔ1] = -1;
    }
    var (i, _) = m.of(onePassMachine.Ꮡinputs).init(ir, ib, @is);
    var (r, r1) = (endOfText, endOfText);
    nint width = 0;
    nint width1 = 0;
    (r, width) = i.step(pos);
    if (r != endOfText) {
        (r1, width1) = i.step(pos + width);
    }
    lazyFlag flag = default!;
    if (pos == 0){
        flag = newLazyFlag(-1, r);
    } else {
        flag = i.context(pos);
    }
    nint pc = re.onepass.Value.Start;
    var inst = Ꮡ((~re.onepass).Inst, pc);
    // If there is a simple literal prefix, skip over it.
    if (pos == 0 && flag.match(((syntax.EmptyOp)(uint8)(~inst).Arg)) && len(re.prefix) > 0 && i.canCheckPrefix()) {
        // Match requires literal prefix; fast search for it.
        if (!i.hasPrefix(Ꮡre)) {
            goto Return;
        }
        pos += len(re.prefix);
        (r, width) = i.step(pos);
        (r1, width1) = i.step(pos + width);
        flag = i.context(pos);
        pc = (nint)re.prefixEnd;
    }
    while (ᐧ) {
        inst = Ꮡ((~re.onepass).Inst, pc);
        pc = (nint)(~inst).Out;
        var exprᴛ1 = (~inst).Op;
        if (exprᴛ1 == syntax.InstMatch) {
            matched = true;
            if (len((~m).matchcap) > 0) {
                m.Value.matchcap[0] = 0;
                m.Value.matchcap[1] = pos;
            }
            goto Return;
        }
        else if (exprᴛ1 == syntax.InstRune) {
            if (!inst.of(onePassInst.ᏑInst).MatchRune(r)) {
                goto Return;
            }
        }
        else if (exprᴛ1 == syntax.InstRune1) {
            if (r != (~inst).Rune[0]) {
                goto Return;
            }
        }
        else if (exprᴛ1 == syntax.InstRuneAny) {
        }
        else if (exprᴛ1 == syntax.InstRuneAnyNotNL) {
            if (r == (rune)'\n') {
                // Nothing
                goto Return;
            }
        }
        else if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch) {
            pc = (nint)onePassNext(inst, // peek at the input rune to see which branch of the Alt to take
 r);
            continue;
        }
        else if (exprᴛ1 == syntax.InstFail) {
            goto Return;
        }
        else if (exprᴛ1 == syntax.InstNop) {
            continue;
        }
        else if (exprᴛ1 == syntax.InstEmptyWidth) {
            if (!flag.match(((syntax.EmptyOp)(uint8)(~inst).Arg))) {
                goto Return;
            }
            continue;
        }
        else if (exprᴛ1 == syntax.InstCapture) {
            if ((nint)(~inst).Arg < len((~m).matchcap)) {
                m.Value.matchcap[(~inst).Arg] = pos;
            }
            continue;
        }
        else { /* default: */
            throw panic("bad inst");
        }

        if (width == 0) {
            break;
        }
        flag = newLazyFlag(r, r1);
        pos += width;
        (r, width) = (r1, width1);
        if (r != endOfText) {
            (r1, width1) = i.step(pos + width);
        }
    }
Return:
    if (!matched) {
        freeOnePassMachine(m);
        return default!;
    }
    dstCap = appendꓸꓸꓸ(dstCap, (~m).matchcap);
    freeOnePassMachine(m);
    return dstCap;
}

// doMatch reports whether either r, b or s match the regexp.
internal static bool doMatch(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s) {
    return Ꮡre.doExecute(r, b, s, 0, 0, default!) != default!;
}

// doExecute finds the leftmost match in the input, appends the position
// of its subexpressions to dstCap and returns dstCap.
//
// nil is returned if no matches are found and non-nil if matches are found.
internal static slice<nint> doExecute(this ж<Regexp> Ꮡre, io.RuneReader r, slice<byte> b, @string s, nint pos, nint ncap, slice<nint> dstCap) {
    ref var re = ref Ꮡre.DerefOrNull();

    if (dstCap == default!) {
        // Make sure 'return dstCap' is non-nil.
        dstCap = arrayNoInts.slice(0, 0, 0);
    }
    if (r == default! && len(b) + len(s) < re.minInputLen) {
        return default!;
    }
    if (re.onepass != nil) {
        return Ꮡre.doOnePass(r, b, s, pos, ncap, dstCap);
    }
    if (r == default! && len(b) + len(s) < re.maxBitStateLen) {
        return Ꮡre.backtrack(b, s, pos, ncap, dstCap);
    }
    var m = Ꮡre.get();
    var (i, _) = m.of(machine.Ꮡinputs).init(r, b, s);
    m.init(ncap);
    if (!m.match(i, pos)) {
        re.put(m);
        return default!;
    }
    dstCap = appendꓸꓸꓸ(dstCap, (~m).matchcap);
    re.put(m);
    return dstCap;
}

// arrayNoInts is returned by doExecute match if nil dstCap is passed
// to it with ncap=0.
internal static array<nint> arrayNoInts = new(0);

} // end regexp_package
