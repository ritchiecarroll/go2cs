// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using @unsafe = unsafe_package;

partial class runtime_package {

// inlinedCall is the encoding of entries in the FUNCDATA_InlTree table.
partial struct inlinedCall {
    internal abi.FuncID funcID; // type of the called function
    internal array<byte> _ = new(3);
    internal int32 nameOff; // offset into pclntab for name of called function
    internal int32 parentPc; // position of an instruction whose source position is the call site (offset from entry)
    internal int32 startLine; // line number of start of function (func keyword/TEXT directive)
}

// An inlineUnwinder iterates over the stack of inlined calls at a PC by
// decoding the inline table. The last step of iteration is always the frame of
// the physical function, so there's always at least one frame.
//
// This is typically used as:
//
//	for u, uf := newInlineUnwinder(...); uf.valid(); uf = u.next(uf) { ... }
//
// Implementation note: This is used in contexts that disallow write barriers.
// Hence, the constructor returns this by value and pointer receiver methods
// must not mutate pointer fields. Also, we keep the mutable state in a separate
// struct mostly to keep both structs SSA-able, which generates much better
// code.
partial struct inlineUnwinder {
    internal ΔfuncInfo f;
    internal /*[1048576]*/ ж<array<inlinedCall>> inlTree;
}

// An inlineFrame is a position in an inlineUnwinder.
partial struct inlineFrame {
    // pc is the PC giving the file/line metadata of the current frame. This is
    // always a "call PC" (not a "return PC"). This is 0 when the iterator is
    // exhausted.
    internal uintptr pc;
    // index is the index of the current record in inlTree, or -1 if we are in
    // the outermost function.
    internal int32 index;
}

// newInlineUnwinder creates an inlineUnwinder initially set to the inner-most
// inlined frame at PC. PC should be a "call PC" (not a "return PC").
//
// This unwinder uses non-strict handling of PC because it's assumed this is
// only ever used for symbolic debugging. If things go really wrong, it'll just
// fall back to the outermost frame.
//
// newInlineUnwinder should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname newInlineUnwinder
internal static (inlineUnwinder, inlineFrame) newInlineUnwinder(ΔfuncInfo f, uintptr pc) {
    @unsafe.Pointer inldata = (uintptr)funcdata(f, abi.FUNCDATA_InlTree);
    if (inldata == nil) {
        return (new inlineUnwinder(f: f), new inlineFrame(pc: pc, index: -1));
    }
    var inlTree = (ж<array<inlinedCall>>)(uintptr)(inldata);
    var u = new inlineUnwinder(f: f, inlTree: inlTree);
    var ᴛ1 = u.resolveInternal(pc);
    return (u, ᴛ1);
}

internal static inlineFrame resolveInternal(this ref inlineUnwinder u, uintptr pc) {
    return new inlineFrame(
        pc: pc, // Conveniently, this returns -1 if there's an error, which is the same
 // value we use for the outermost frame.

        index: pcdatavalue1(u.f, abi.PCDATA_InlTreeIndex, pc, false)
    );
}

internal static bool valid(this inlineFrame uf) {
    return uf.pc != 0;
}

// next returns the frame representing uf's logical caller.
internal static inlineFrame next(this ref inlineUnwinder u, inlineFrame uf) {
    if (uf.index < 0) {
        uf.pc = 0;
        return uf;
    }
    var parentPc = u.inlTree.Value[uf.index].parentPc;
    return u.resolveInternal(u.f.entry() + (uintptr)parentPc);
}

// isInlined returns whether uf is an inlined frame.
internal static bool isInlined(this ref inlineUnwinder u, inlineFrame uf) {
    return uf.index >= 0;
}

// srcFunc returns the srcFunc representing the given frame.
//
// srcFunc should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
// The go:linkname is below.
internal static ΔsrcFunc srcFunc(this ref inlineUnwinder u, inlineFrame uf) {
    if (uf.index < 0) {
        return u.f.srcFunc();
    }
    var t = u.inlTree.at<inlinedCall>((nint)(uf.index));
    return new ΔsrcFunc(
        u.f.datap,
        (~t).nameOff,
        (~t).startLine,
        (~t).funcID
    );
}

//go:linkname badSrcFunc runtime.(*inlineUnwinder).srcFunc
internal static partial ΔsrcFunc badSrcFunc(ж<inlineUnwinder> _Δp0, inlineFrame _Δp1);

// fileLine returns the file name and line number of the call within the given
// frame. As a convenience, for the innermost frame, it returns the file and
// line of the PC this unwinder was started at (often this is a call to another
// physical function).
//
// It returns "?", 0 if something goes wrong.
internal static (@string @file, nint line) fileLine(this ref inlineUnwinder u, inlineFrame uf) {
    @string @file = default!;

    (@file, var line32) = funcline1(u.f, uf.pc, false);
    return (@file, (nint)line32);
}

} // end runtime_package
