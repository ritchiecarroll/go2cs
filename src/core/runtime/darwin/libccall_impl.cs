// libccall_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.libcCall — darwin's dispatch bottom, realized on golib's platform-neutral dispatcher.
//
// Every libc trampoline in sys_darwin.cs is reached as libcCall(FuncPCABI0(x_trampoline), &args).
// Go's body switches to the g0 stack, records the caller's g/pc/sp in m.libcall* for the profiler's
// traceback, and jumps through asmcgocall. Its converted form opens with getg() and ends in
// asmcgocall — four bodyless intrinsics (getg, getcallerpc, getcallersp, asmcgocall) that the
// generator stubs with a throw and that no hand-own implements — so a REAL function pointer handed to
// it dies one line in, before the call. That is why the class-B resolver alone could not have
// unblocked darwin, and why this file exists (docs/phase4/DESIGN-darwin-run-layer-2.md §2.2).
//
// WHAT IS DROPPED, AND WHY THAT IS THE FAITHFUL ANSWER. The g0 switch and the libcall* bookkeeping
// exist so a profiler signal landing mid-call can unwind the Go stack; the managed host has no
// signal-driven profiler walking Go frames and no g0 — the same judgment syscall_linux_impl.cs
// records for entersyscall/exitsyscall one platform over. It is WEAKER here than there, and the
// design says so (§7.5): linux had a running flavour to confirm it, darwin has none yet, so this is
// an assumption recorded, not a measurement, and the first mac dispatch is what moves it.
//
// WHAT IS DONE. `fn` is the exported symbol's real address (FuncPCABI0 resolved the trampoline
// through the package's `//go:cgo_import_dynamic` records — the class-B pass binds runtime's spelling,
// `libc_fcntl` <-> `fcntl_trampoline`, as of the same cut). `arg` is `unsafe.Pointer(&args)` over a
// lifted per-call-site struct; its number resolves back to the box through the pointer-provenance
// record (ж's uintptr operator pins and registers reference-free storage), and the box's type gives
// the layout Go's trampoline unpacks by offset: leading fields are the inputs, `ret`/`errno` the
// outputs. GoLibcCall.DispatchArgsStruct performs exactly that and writes the outcome back through
// the box, so the converted caller reads `args.ret` as it always did.
//
// MEASURED CORRECTION (increment 6, 2026-09-05, Q41). The lifted per-call-site struct above is the
// shape of 12 of sys_darwin.go's 50 libcCall sites at the pinned go1.23.12; 35 pass
// `&<first parameter>` under //go:cgo_unsafe_args, where Go's trampoline reads the WHOLE parameter
// block from consecutive stack slots (sigaction_trampoline: 8(DI) new, 16(DI) old, 0(DI) sig).
// The converted form boxes the first parameter ALONE, so DispatchArgsStruct places one register and
// every parameter behind it travels as whatever the caller-saved registers held. Three shapes:
// SILENT for a plain-integer first parameter with more behind it (sigaction, setitimer, kevent,
// pthread_kill, syscall_syscall9 at master; read, write1 and sigprocmask already displaced);
// SILENT for a first parameter whose trampoline writes the RESULT through the pointer (walltime,
// pthread_self -- dispatched, never written back); LOUD by type for a pointer first parameter
// (the pthread_*/mmap/madvise/mlock/open/sigaltstack/sysctl family, refused above as a non-struct
// pointee). Q41's arm64 death was the sigaction member: libc wrote a 16-byte read-back through a
// stale third register into the managed stack, and the CLR's next stack walk died on it.
// sigaction_impl.cs is the first remedy (the seam marshals both pointers natively); the class is
// a separate ruling, recorded on the board.
//
// THE RULING LANDED AS THE BLOCK LIFT (Q56, 2026-09-05, DESIGN-cgo-unsafe-args-block-lift.md): the
// converter synthesizes the WHOLE parameter block for every `&first` site under //go:cgo_unsafe_args
// (31 in sys_darwin.cs; read/write1/sigprocmask/sigaction stay displaced), so shape (a)'s stale
// registers and shape (c)'s refusals end at the dispatcher, which places the block's fields; shape
// (b)'s two members take the per-symbol form table in the body below. What stays this file's
// refusal is narrower than before: a pointer field whose pointee is reference-bearing arrives as an
// order token (Q44) and libc answers EFAULT -- an improved failure, whose remedy per site is
// increment 6's native mirror (usigactiont done; itimerval, keventt, stackt, pthreadattr pending).
//
// SHAPE (d), THE DISCARDED RESULT (increment 7, 2026-09-05, the Q56 census). Go's asmcgocall hands
// the trampoline's AX back as libcCall's int32, and TWENTY converted sites read it -- `var ret =
// libcCall(...)` / `return libcCall(...)`: pthread_attr_init, pthread_attr_getstacksize,
// pthread_attr_setdetachstate, pthread_create, closefd, open, sysctl, sysctlbyname, kqueue, kevent,
// pthread_mutex_init/lock/unlock, pthread_cond_init/wait/timedwait_relative_np/signal, issetugid,
// mach_vm_region, proc_regionfilename. This file returned 0 for all of them on the belief, stated
// in its own old comment, that no caller read the result. What that cost TODAY, read against the
// shapes above: closefd reported success unconditionally and kevent answered 0 events (the two
// reading sites that dispatch at master); the two sites that pass a NIL args pointer -- kqueue and
// issetugid, the zero-argument trampolines -- never reached their call at all, refused here as
// "does not resolve to a managed args box"; the other sixteen are refused by type (shape (c)) or
// misplaced (shape (a)) before the result matters, and become real readers only under the lift.
// Both fixed below: the dispatcher returns the register and takes a null box as the bare call;
// libcCall returns the low 32 bits. Guarded on glibc by LibcCallDispatchTests -- getpid
// discriminates by construction.
//
// WHAT IS REFUSED, LOUDLY AND BY NAME, NEVER WITH A DEFAULT. An argument whose number resolves to
// no box (a reference-bearing args struct — mmap_args, mach_vm_region_args, proc_regionfilename_args
// carry managed pointers and have no pinnable storage, so they never register), a field the
// dispatcher cannot place in a register, or a null function pointer. A wrong layout would hand libc
// garbage in registers; a throw that names the symbol is the honest answer.
//
// Hand-owned (no libccall_impl.go exists, so a reconvert never regenerates this file). Declared
// NON-partial: the registry displacement leaves a comment placeholder in sys_libc.cs, not a bodyless
// partial, so this file owns the whole declaration — the nanotime_impl.cs shape beside it.
[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// The library Go's own pragma names for __error (sys_darwin.cs: `//go:cgo_import_dynamic libc_error
// __error "/usr/lib/libSystem.B.dylib"`). __error has no trampoline — Go's assembly calls it
// directly — so it is resolved by symbol rather than through a record.
private const string libSystemPath = "/usr/lib/libSystem.B.dylib";

private static nint s_libcCallErrnoReader;

internal static int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg) {
    nint entryPoint = unchecked((nint)(nuint)(uintptr)fn);

    if (entryPoint == 0) {
        throw new InvalidOperationException("go2cs: libcCall: the function pointer is null — FuncPCABI0 did not resolve the trampoline");
    }

    string symbol = GoCgoDynamicImports.SymbolOf(entryPoint) ?? $"0x{entryPoint:x}";

    // A nil args pointer is Go's zero-argument trampoline (kqueue, issetugid): nothing to resolve;
    // the dispatcher makes the bare call and the register is the whole outcome (increment 7).
    object? argsBox = arg == nil ? null : ManagedPointerTokens.Resolve((nuint)(uintptr)arg);

    if (arg != nil && argsBox is null) {
        throw new InvalidOperationException(
            $"go2cs: libcCall({symbol}): the argument pointer does not resolve to a managed args box — " +
            "a reference-bearing args struct has no pinnable storage and cannot be dispatched by layout; " +
            "the per-symbol layout record is the remedy");
    }

    if (s_libcCallErrnoReader == 0) {
        s_libcCallErrnoReader = GoCgoDynamicImports.Resolve("__error", libSystemPath);
    }

    // THE PER-SYMBOL FORM TABLE (Q56, DESIGN-cgo-unsafe-args-block-lift.md section 3.3): the two
    // `&local` sites whose trampolines use the pointer in a way no fields-to-registers layout can
    // express, read BEFORE the default dispatch. Both are unreached at master (walltime_trampoline
    // carries no import record to resolve through; pthread_self's callers are hand-owned), so the
    // table is correctness for the day they are, at the cost of two arms.
    switch (symbol) {
    case "clock_gettime":
        // walltime_trampoline: MOVQ DI, SI; MOVL $CLOCK_REALTIME, DI; CALL libc_clock_gettime -- the
        // block (a timespec: reference-free, pinned) is the SECOND argument, by address; libc fills it.
        return unchecked((int32)(uint32)GoLibcCall.CallWithBlockAddress(entryPoint, (nuint)(uintptr)arg, [0], s_libcCallErrnoReader, symbol));
    case "pthread_self":
        // pthread_self_trampoline: CALL libc_pthread_self; MOVQ AX, 0(BX) -- the result lands in the
        // block's first word (the pthread wrapper's value).
        return unchecked((int32)(uint32)GoLibcCall.CallStoringResultIntoBlock(entryPoint, argsBox, s_libcCallErrnoReader, symbol));
    case "mach_vm_region":
        return machVMRegionDispatch(entryPoint, argsBox, symbol);
    case "proc_regionfilename":
        return procRegionfilenameDispatch(entryPoint, argsBox, symbol);
    }

    nuint r = GoLibcCall.DispatchArgsStruct(entryPoint, argsBox, s_libcCallErrnoReader, symbol);

    // Go's asmcgocall returns the trampoline's AX as libcCall's int32 -- its low 32 bits, the MOVL
    // rule -- and twenty callers in sys_darwin.cs read it (the header's shape (d)); the lifted
    // family reads args.ret as well, which the dispatcher has already filled. Until increment 7 this
    // returned 0 on the belief that no caller read it.
    return unchecked((int32)(uint32)r);
}

// THE REGION WALK'S TWO RECORDS (2026-10-14). runtime/pprof's machVMInfo builds every darwin CPU and
// heap profile's mapping table from these two calls, and both blocks carry managed pointers --
// `address`, `size`, `count` and `object_name` are ж<T> boxes, `buf` is an interior ж<byte> -- so
// the default dispatch above refuses them by field: "field 'address' of mach_vm_region_args is a ж`1,
// which this dispatcher cannot place in an integer register" (net/http/pprof's heap handler,
// osx-arm64, run 38521951876). This is the per-symbol layout record that refusal names. Each scalar
// the trampoline passes BY ADDRESS goes through a native cell, filled from its box before the call
// and written back after it, success or failure, because libc may write a partial result; the
// mapping name goes through a native buffer copied into the caller's slice.

// mach_vm_region_trampoline (sys_darwin_amd64.s:810, sys_darwin_arm64.s:778) unpacks the block by
// offset -- address 0, size 8, flavor 16, info 24, count 32, object_name 40 -- and calls
// mach_vm_region(mach_task_self_, address, size, flavor, info, count, object_name). This passes all
// seven arguments. Go's amd64 trampoline loads object_name into R10, which is not a SysV argument
// register, so libc reads its seventh argument from the stack instead; on arm64 the seventh is X6
// and arrives as passed. Here both arches get the cell.
//
// `info` is the caller's machVMRegionBasicInfoData box (runtime/pprof's type, which this package
// cannot name). Its [8]byte Offset makes it reference-bearing, so its word is the box's ORDER TOKEN,
// and GoLibcCall.Call hands it to NativeStructMarshal: encoded at Go's offsets into a native buffer,
// decoded back after the call. Go's layout of that record IS the kernel's packed 36-byte
// vm_region_basic_info_data_64_t -- that is why Go spells Offset as [8]byte -- so the marshal's
// image is the native one. VM_REGION_BASIC_INFO_COUNT_64 (9) is the count the caller passes.
private const int MachVMRegionAddressCell = 0;
private const int MachVMRegionSizeCell = 8;
private const int MachVMRegionCountCell = 16;
private const int MachVMRegionObjectNameCell = 20;
private const int MachVMRegionCellBytes = 24;

private static int32 machVMRegionDispatch(nint entryPoint, object? argsBox, string symbol) {
    if (argsBox is not ж<mach_vm_region_args> box) {
        throw new InvalidOperationException($"go2cs: libcCall({symbol}): the args box is {argsBox?.GetType().Name ?? "null"}, not mach_vm_region_args");
    }

    ref mach_vm_region_args args = ref box.Value;

    // mach_task_self() is a macro over this global; Go's trampoline reads it the same way
    // (MOVQ $libc_mach_task_self_(SB), DI; MOVL 0(DI), DI).
    nint taskSelf = GoCgoDynamicImports.Resolve("mach_task_self_", libSystemPath);
    nint cells = System.Runtime.InteropServices.Marshal.AllocHGlobal(MachVMRegionCellBytes);

    try {
        System.Runtime.InteropServices.Marshal.WriteInt64(cells, MachVMRegionAddressCell, unchecked((long)(uint64)args.address.Value));
        System.Runtime.InteropServices.Marshal.WriteInt64(cells, MachVMRegionSizeCell, unchecked((long)(uint64)args.size.Value));
        System.Runtime.InteropServices.Marshal.WriteInt32(cells, MachVMRegionCountCell, unchecked((int)(uint32)args.count.Value));
        System.Runtime.InteropServices.Marshal.WriteInt32(cells, MachVMRegionObjectNameCell, unchecked((int)(uint32)args.object_name.Value));

        nuint r = GoLibcCall.Call(entryPoint, [
            unchecked((nuint)(uint)System.Runtime.InteropServices.Marshal.ReadInt32(taskSelf)),
            (nuint)(cells + MachVMRegionAddressCell),
            (nuint)(cells + MachVMRegionSizeCell),
            unchecked((nuint)(uint)(int32)args.flavor),
            (nuint)(uintptr)args.info,
            (nuint)(cells + MachVMRegionCountCell),
            (nuint)(cells + MachVMRegionObjectNameCell)
        ], GoLibcErrnoRule.None, s_libcCallErrnoReader, out _);

        args.address.Value = unchecked((uint64)System.Runtime.InteropServices.Marshal.ReadInt64(cells, MachVMRegionAddressCell));
        args.size.Value = unchecked((uint64)System.Runtime.InteropServices.Marshal.ReadInt64(cells, MachVMRegionSizeCell));
        args.count.Value = (machMsgTypeNumber)unchecked((uint32)System.Runtime.InteropServices.Marshal.ReadInt32(cells, MachVMRegionCountCell));
        args.object_name.Value = (machPort)unchecked((uint32)System.Runtime.InteropServices.Marshal.ReadInt32(cells, MachVMRegionObjectNameCell));

        // kern_return_t, an int: the low 32 bits, as libcCall's MOVL rule reads every result.
        return unchecked((int32)(uint32)r);
    }
    finally {
        System.Runtime.InteropServices.Marshal.FreeHGlobal(cells);
    }
}

// proc_regionfilename_trampoline (sys_darwin_amd64.s:823, sys_darwin_arm64.s:792) unpacks
// {pid 0, address 8, buf 16, bufSize 24} and calls proc_regionfilename(pid, address, buf, bufSize),
// which answers the number of bytes it copied (0 when the region has no file). `buf` is
// unsafe.SliceData of the caller's []byte, an interior reference into the slice's backing store,
// so the bytes libc writes into the native buffer are copied back through unsafe.Slice over it.
private static int32 procRegionfilenameDispatch(nint entryPoint, object? argsBox, string symbol) {
    if (argsBox is not ж<proc_regionfilename_args> box) {
        throw new InvalidOperationException($"go2cs: libcCall({symbol}): the args box is {argsBox?.GetType().Name ?? "null"}, not proc_regionfilename_args");
    }

    ref proc_regionfilename_args args = ref box.Value;
    int64 bufSize = args.bufSize;

    if (bufSize < 0 || bufSize > int.MaxValue) {
        throw new InvalidOperationException($"go2cs: libcCall({symbol}): buffer size {bufSize} is out of range");
    }

    nint buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal((nint)Math.Max(bufSize, 1L));

    try {
        nuint r = GoLibcCall.Call(entryPoint, [
            unchecked((nuint)args.pid),
            (nuint)args.address,
            (nuint)buffer,
            unchecked((nuint)bufSize)
        ], GoLibcErrnoRule.None, s_libcCallErrnoReader, out _);

        int32 copied = unchecked((int32)(uint32)r);

        if (copied > 0) {
            int count = (int)Math.Min(copied, bufSize);
            slice<byte> window = @unsafe.Slice(args.buf, (uintptr)(nuint)count);

            for (int i = 0; i < count; i++) {
                window[i] = System.Runtime.InteropServices.Marshal.ReadByte(buffer, i);
            }
        }

        return copied;
    }
    finally {
        System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
    }
}

} // end runtime_package
