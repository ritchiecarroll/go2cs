// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

using heap = container.heap_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using maphash = hash.maphash_package;
using io = io_package;
using log = log_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using container;
using encoding;
using hash;
using ꓸꓸꓸValue = Span<gen_package.Value>;
using ꓸꓸꓸany = Span<any>;

partial class gen_package {

internal const bool logCompile = true;

internal static void fatalf(@string f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    throw panic(fmt.Sprintf(f, args.ꓸꓸꓸ));
}

partial struct File {
    internal io.Writer w;
    internal slice<ж<Func>> funcs;
    internal slice<fileConst> consts;
}

public static ж<File> NewFile(io.Writer w) {
    return Ꮡ(new File(w: w));
}

public static void AddFunc(this ref File f, ж<Func> Ꮡfn) {
    f.funcs = append(f.funcs, Ꮡfn);
}

partial struct fileConst {
    internal @string name;
    internal any data;
}

public static void AddConst(this ref File f, @string name, any data) {
    // TODO: It would be nice if this were unified with "const" ops, but the
    // reason I added this was for []*Func consts, which would take an overhaul
    // to represent in "const" ops.
    f.consts = append(f.consts, new fileConst(name, data));
}

partial struct Func {
    internal @string name;
    internal nint nArgs;
    internal nint idGen;
    internal slice<ж<op>> ops;
}

public static ж<Func> NewFunc(@string name) {
    var fn = Ꮡ(new Func(name: name));
    return fn;
}

// attach adds x to fn's op list. If x has any unattached arguments, this adds
// those first (recursively).
internal static void attach(this ж<Func> Ꮡfn, ж<op> Ꮡx) {
    ref var fn = ref Ꮡfn.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    // Make sure the arguments are attached to the function.
    foreach (var (_, arg) in x.args) {
        var argFn = arg.Value.fn;
        if (argFn == nil){
            Ꮡfn.attach(arg);
        } else 
        if (argFn != Ꮡfn) {
            throw panic("ops from different functions");
        }
    }

    x.fn = Ꮡfn;
    x.id = fn.idGen;
    fn.idGen++;
    fn.ops = append(fn.ops, Ꮡx);
}

public static T Arg<W, T>(ж<Func> Ꮡfn)
    where W : Δwrap<T>
    where T : Word
{
    ref var fn = ref Ꮡfn.DerefOrNull();

    ref var loc = ref heap<locReg>(out var Ꮡloc);
    loc = new locReg(cls: regClassGP, reg: fn.nArgs);
    fn.nArgs++;
    W x = GoZero<W>();
    var o = Ꮡ(new op(Δop: "arg"u8, kind: x.kind(), c: loc));
    Ꮡfn.attach(o);
    return x.wrap(o);
}

public static void Return(params ꓸꓸꓸValue resultsʗp) {
    var results = resultsʗp.sslice();

    var args = new slice<ж<op>>(len(results));
    foreach (var (i, res) in results) {
        args[i] = res.getOp();
    }
    @void x = new(nil);
    x.valAny.initOp(Ꮡ(new op(Δop: "return"u8, kind: voidKind, args: args)));
}

public partial struct op {
    internal @string Δop;
    internal ж<Δkind> kind;
    internal slice<ж<op>> args;
    internal nint id;
    internal ж<Func> fn;
    // c depends on "op".
    //
    // arg locReg - The register containing the argument value
    // const any  - The constant value
    // deref int  - Byte offset from args[0]
    internal any c;
    internal @string name;
}

public static @string String(this ref op o) {
    return fmt.Sprintf("v%02d"u8, o.id);
}

internal static ж<op> imm(any val) {
    return Ꮡ(new op(Δop: "imm"u8, c: val));
}

internal static bool equalNoName(this ref op o, ж<op> Ꮡo2) {
    ref var o2 = ref Ꮡo2.DerefOrNull();

    if (o.Δop != o2.Δop || !AreEqual(o.c, o2.c) || len(o.args) != len(o2.args)) {
        return false;
    }
    foreach (var (i, arg) in o.args) {
        if (o2.args[i] != arg) {
            return false;
        }
    }
    return true;
}

internal static void write(this ref op o, io.Writer w) {
    fmt.Fprintf(w, "v%02d = %s"u8, o.id, o.Δop);
    foreach (var (_, arg) in o.args) {
        fmt.Fprintf(w, " v%02d"u8, (~arg).id);
    }
    if (o.c != default!) {
        fmt.Fprintf(w, " %v"u8, o.c);
    }
    if (o.name != ""u8) {
        fmt.Fprintf(w, " %q"u8, o.name);
    }
    if (o.kind != nil) {
        fmt.Fprintf(w, " [%s]"u8, (~o.kind).typ);
    }
    fmt.Fprintf(w, "\n"u8);
}

internal static void write(this ref Func fn, io.Writer w) {
    fmt.Fprintf(w, "FUNC %s\n"u8, fn.name);
    foreach (var (_, op) in fn.ops) {
        op.write(w);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string initialˢ = "initial"u8;
private static readonly @string postCseˢ = "post cse"u8;
private static readonly @string postDeadcodeˢ = "post deadcode"u8;
private static readonly @string postAddLoadsˢ = "post addLoads"u8;
private static readonly @string postScheduleˢ = "post schedule"u8;

public static void Compile(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    // TODO: CSE constants across the whole file

    fmt.Fprintf(f.w, """
#include "go_asm.h"
#include "textflag.h"


"""u8.Raw());

    foreach (var (_, c) in f.consts) {
        f.emitConst(c.name, c.data);
    }

    void trace(ж<Func> fn, @string step) {
        if (!logCompile) {
            return;
        }
        log.Printf("## Compiling %s: %s"u8, (~fn).name, step);
        fn.write(new os.FileжWriter(os.Stderr));
    }

    foreach (var (_, fn) in f.funcs) {
        trace(fn, initialˢ);

        while (ᐧ) {
            if (fn.cse()) {
                trace(fn, postCseˢ);
                continue;
            }
            if (fn.deadcode()) {
                trace(fn, postDeadcodeˢ);
                continue;
            }
            break;
        }
        fn.addLoads();
        trace(fn, postAddLoadsˢ);

        // Assigning locations requires ops to be in dependency order.
        fn.schedule();
        trace(fn, postScheduleˢ);

        var locs = fn.assignLocs();

        fn.emit(Ꮡf, locs);
    }
}

// cse performs common subexpression elimination.
internal static bool cse(this ref Func fn) {
    // Compute structural hashes
    var hashes = new map<ж<op>, uint64>();
    maphash.Hash h = new();
    array<byte> bbuf = new(8);
    foreach (var (_, op) in fn.ops) {
        // We ignore the name for canonicalization.
        h.Reset();
        h.WriteString((~op).Δop);
        // TODO: Ideally we would hash o1.c, but we don't have a good way to do that.
        foreach (var (_, arg) in (~op).args) {
            {
                var (_, ok) = hashes[arg, ꟷ]; if (!ok) {
                    throw panic("ops not in dependency order");
                }
            }
            binary.NativeEndian.PutUint64(bbuf[..], hashes[arg]);
            h.Write(bbuf[..]);
        }
        hashes[op] = h.Sum64();
    }

    var canon = new map<uint64, slice<ж<op>>>();
    var canonʗ1 = canon;
    var hashesʗ1 = hashes;
    ж<op> lookup(ж<op> o) {
        var hash = hashesʗ1[o];
        foreach (var (_, o2) in canonʗ1[hash]) {
            if (o.equalNoName(o2)) {
                return o2;
            }
        }
        canonʗ1[hash] = append(canonʗ1[hash], o);
        return o;
    }

    // Canonicalize ops.
    var dirty = false;
    foreach (var (_, op) in fn.ops) {
        foreach (var (i, arg) in (~op).args) {
            var newArg = lookup(arg);
            if (arg != newArg) {
                dirty = true;
                op.Value.args[i] = newArg;
            }
        }
    }
    return dirty;
}

// deadcode eliminates unused ops.
internal static bool deadcode(this ref Func fn) {
    var marks = new map<ж<op>, bool>();
    ref var mark = ref heap<Action<ж<op>>>(out var Ꮡmark);
    var marksʗ1 = marks;
    mark = (ж<op> o) => {
        if (marksʗ1[o]) {
            return;
        }
        marksʗ1[o] = true;
        foreach (var (_, arg) in (~o).args) {
            Ꮡmark.ValueSlot(arg);
        }
    };
    // Mark operations that have a side-effect.
    foreach (var (_, op) in fn.ops) {
        var exprᴛ1 = (~op).Δop;
        if (exprᴛ1 == "return"u8) {
            mark(op);
        }
    }
    // Discard unmarked operations
    if (len(marks) == len(fn.ops)) {
        return false;
    }
    var newOps = new slice<ж<op>>(0, len(marks));
    foreach (var (_, op) in fn.ops) {
        if (marks[op]) {
            newOps = append(newOps, op);
        }
    }
    fn.ops = newOps;
    return true;
}

// canMem is a map from operation to a bitmap of which arguments can use a
// direct memory reference.
[GoLazy] partial class lazyᴛcanMem { internal static map<@string, uint64> canMem = new map<@string, uint64>{
    ["VPERMB"u8] = ((uint64)1 << 0),
    ["VPERMI2B"u8] = ((uint64)1 << 0),
    ["VPERMT2B"u8] = ((uint64)1 << 0),
    ["VGF2P8AFFINEQB"u8] = ((uint64)1 << 0),
    ["VPORQ"u8] = ((uint64)1 << 0),
    ["VPSUBQ"u8] = ((uint64)1 << 0),
    ["VPSHUFBITQMB"u8] = ((uint64)1 << 0)
}; }

// addLoads inserts load ops for ops that can't take memory inputs directly.
internal static void addLoads(this ж<Func> Ꮡfn) {
    ref var fn = ref Ꮡfn.DerefOrNull();

    // A lot of operations can directly take memory locations. If there's only a
    // single reference to a deref operation, and the operation can do the deref
    // itself, eliminate the deref. If there's more than one reference, then we
    // leave the load so we can share the value in the register.
    var nRefs = fn.opRefs();
    var loads = new map<ж<op>, ж<op>>(); // deref -> load
    foreach (var (_, o) in fn.ops) {
        var canMask = canMem[(~o).Δop];
        foreach (var (i, arg) in (~o).args) {
            // TODO: Many AVX-512 operations that support memory operands also
            // support a ".BCST" suffix that performs a broadcasting memory
            // load. If the const can be broadcast and all uses support
            // broadcast load, it would be nice to use .BCST. I'm not sure if
            // that belongs in this pass or a different one.
            if ((~arg).Δop == "deref"u8 || (~arg).Δop == "const"u8) {
                // These produce memory locations.
                if ((canMask & (((uint64)1).Lsh((int64)(i)))) == 0 || nRefs[arg] > 1) {
                    // This argument needs to be loaded into a register.
                    var (load, ok) = loads[arg, ꟷ];
                    if (!ok) {
                        load = makeLoad(arg);
                        Ꮡfn.attach(load);
                        loads[arg] = load;
                    }
                    o.Value.args[i] = load;
                }
            }
        }
    }
}

internal static map<ж<op>, nint> opRefs(this ref Func fn) {
    var refs = new map<ж<op>, nint>();
    foreach (var (_, o1) in fn.ops) {
        foreach (var (_, arg) in (~o1).args) {
            refs[arg]++;
        }
    }
    return refs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string movqˢ = "MOVQ"u8;
private static readonly @string vmovdqu64ˢ = "VMOVDQU64"u8;

internal static ж<op> makeLoad(ж<op> Ꮡderef) {
    ref var deref = ref Ꮡderef.DerefOrNull();

    ref var inst = ref heap(new @string(), out var Ꮡinst);
    var exprᴛ1 = (~deref.kind).reg;
    if (exprᴛ1 == regClassGP) {
        inst = movqˢ;
    }
    else if (exprᴛ1 == regClassZ) {
        inst = vmovdqu64ˢ;
    }
    else { /* default: */
        fatalf("don't know how to load %v"u8, (~deref.kind).reg);
    }
    // The load references deref rather than deref.args[0] because when we
    // assign locations, the deref op gets the memory location to load from,
    // while its argument has some other location (like a register). Also, the
    // offset to deref is attached to the deref op.
    return Ꮡ(new op(Δop: inst, kind: deref.kind, args: new ж<op>[]{Ꮡderef}.slice()));
}

partial struct opHeap /*[]ж<op>*/;

internal static nint Len(this opHeap h) {
    return len(h);
}

internal static void Swap(this opHeap h, nint i, nint j) {
    (h[i], h[j]) = (h[j], h[i]);
}

internal static bool Less(this opHeap h, nint i, nint j) {
    nint priority(ж<op> o) {
        if ((~o).Δop == "deref"u8 || (~o).Δop == "const"u8) {
            // Input to memory load
            return 1;
        }
        if (len((~o).args) > 0 && ((~(~o).args[0]).Δop == "deref"u8 || (~(~o).args[0]).Δop == "const"u8)) {
            // Memory load
            return 2;
        }
        return 100;
    }
    {
        nint p1 = priority(h[i]);
        nint p2 = priority(h[j]); if (p1 != p2) {
            return p1 < p2;
        }
    }
    return (~h[i]).id < (~h[j]).id;
}

internal static void Push(this ref opHeap h, any x) {
    h = append(h, x._<ж<op>>());
}

internal static any Pop(this ref opHeap h) {
    var old = h;
    nint n = len(old);
    var x = old[n - 1];
    h = old.slice(0, n - 1);
    return x.OrTypedNil();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object scheduleDidnTScheduleAllˢ = (@string)"schedule didn't schedule all ops"u8;
private static readonly object beforeˢ = (@string)"before:"u8;
private static readonly object afterˢ = (@string)"after:"u8;
private static readonly object badScheduleˢ = (@string)"bad schedule"u8;

// schedule ensures fn's ops are in dependency order.
internal static void schedule(this ref Func fn) {
    // TODO: This tends to generate a huge amount of register pressure, mostly
    // because it floats loads as early as possible and partly because it has no
    // concept of rematerialization and CSE can make rematerializable values
    // live for a very long time. It some sense it doesn't matter because we
    // don't run out of registers for anything we need.

    var missing = new map<ж<op>, nint>();
    var uses = new map<ж<op>, slice<ж<op>>>();
    ref var h = ref heap<opHeap>(out var Ꮡh);
    foreach (var (_, op) in fn.ops) {
        if (len((~op).args) == 0){
            h = append(h, op);
        } else {
            missing[op] = len((~op).args);
        }
        foreach (var (_, arg) in (~op).args) {
            uses[arg] = append(uses[arg], op);
        }
    }
    heap.Init(new opHeapжInterface(Ꮡh));

    var newOps = new slice<ж<op>>(0, len(fn.ops));
    while (len(h) > 0) {
        if (false) {
            log.Printf("schedule: %s"u8, h);
        }
        var top = h[0];
        newOps = append(newOps, top);
        heap.Pop(new opHeapжInterface(Ꮡh));
        foreach (var (_, o) in uses[top]) {
            missing[o]--;
            if (missing[o] == 0) {
                heap.Push(new opHeapжInterface(Ꮡh), o.OrTypedNil());
            }
        }
    }
    if (len(newOps) != len(fn.ops)) {
        log.Print(scheduleDidnTScheduleAllˢ);
        log.Print(beforeˢ);
        fn.write(new os.FileжWriter(os.Stderr));
        fn.ops = newOps;
        log.Print(afterˢ);
        fn.write(new os.FileжWriter(os.Stderr));
        log.Fatal(badScheduleˢ);
    }

    fn.ops = newOps;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string maskˢ = ".mask"u8;

internal static void emit(this ref Func fn, ж<File> Ꮡf, map<ж<op>, loc> locs) {
    ref var f = ref Ꮡf.DerefOrNull();

    var w = f.w;

    // Emit constants first
    foreach (var (_, o) in fn.ops) {
        if ((~o).Δop == "const"u8) {
            @string name = locs[o]._<loc, locMem>().name;
            f.emitConst(name, (~o).c);
        }
    }

    fmt.Fprintf(w, "TEXT %s(SB), NOSPLIT, $0-0\n"u8, fn.name);

    // Emit body
    foreach (var (_, o) in fn.ops) {
        var exprᴛ1 = (~o).Δop;
        if (exprᴛ1 == "const"u8 || exprᴛ1 == "arg"u8 || exprᴛ1 == "return"u8 || exprᴛ1 == "deref"u8 || exprᴛ1 == "imm"u8) {
            // Does not produce code
            continue;
        }
        var exprᴛ2 = (~o).Δop;
        if (exprᴛ2 == "addConst"u8) {
            fatalf("addConst not lowered"u8);
        }

        @string opName = o.Value.Δop;
        // A ".mask" suffix is used to distinguish AVX-512 ops that use the same
        // mnemonic for regular and masked mode.
        opName = strings.TrimSuffix(opName, maskˢ);

        fmt.Fprintf(w, "\t%s"u8, opName);
        if ((~o).Δop == "VGF2P8AFFINEQB"u8) {
            // Hidden immediate, but always 0
            //
            // TODO: Replace this with an imm input.
            fmt.Fprintf(w, " $0,"u8);
        }
        foreach (var (i, arg) in (~o).args) {
            if (i == 0){
                fmt.Fprintf(w, " "u8);
            } else {
                fmt.Fprintf(w, ", "u8);
            }
            if ((~arg).Δop == "imm"u8){
                fmt.Fprintf(w, "$0x%x"u8, (~arg).c);
            } else {
                fmt.Fprint(w, locs[arg].LocString());
            }
        }
        {
            var (_, ok) = opRMW[(~o).Δop, ꟷ]; if (ok){
                // Read-modify-write instructions, so the output is already in the
                // arguments above.
            } else {
                fmt.Fprintf(w, ", %s"u8, locs[o].LocString());
            }
        }
        fmt.Fprintf(w, "\n"u8);
    }
    fmt.Fprintf(w, "\tRET\n"u8);
    fmt.Fprintf(w, "\n"u8);
}

internal static void emitConst(this ref File f, @string name, any data) {
    switch (data.type()) {
    case slice<ж<Func>> dataΔ1: {
        fmt.Fprintf(f.w, "GLOBL %s(SB), RODATA, $%#x\n"u8, name, len(dataΔ1) * 8);
        foreach (var (i, fn) in dataΔ1) {
            fmt.Fprintf(f.w, "DATA  %s+%#02x(SB)/8, "u8, name, 8 * i);
            if (fn == nil){
                fmt.Fprintf(f.w, "$0\n"u8);
            } else {
                fmt.Fprintf(f.w, "$%s(SB)\n"u8, (~fn).name);
            }
        }
        fmt.Fprintf(f.w, "\n"u8);
        return;
    }}

    // Assume it's a numeric slice or array
    var rv = reflect.ValueOf(data);
    nint sz = (nint)rv.Type().Elem().Size();
    fmt.Fprintf(f.w, "GLOBL %s(SB), RODATA, $%#x\n"u8, name, rv.Len() * sz);
    for (nint wi = 0; wi < sz * rv.Len() / 8; wi++) { // Iterate over words
        uint64 word = default!;
        for (nint j = 0; j < 8 / sz; j++) { // Iterate over elements in this word
            var d = rv.Index(quo(wi * 8, sz) + j).Uint();
            word |= (uint64)(d.Lsh((int64)(j * sz * 8)));
        }
        fmt.Fprintf(f.w, "DATA  %s+%#02x(SB)/8, $%#016x\n"u8, name, 8 * wi, word);
    }

    fmt.Fprintf(f.w, "\n"u8);
}

} // end gen_package
