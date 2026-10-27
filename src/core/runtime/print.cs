// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strconv = @internal.strconv_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

[GoFieldAccess(typeof(Δsliceᴛ))]
partial class runtime_package {

// The compiler knows that a print of a value of this type
// should use printhex instead of printuint (decimal).
partial struct Δhex /*num:uint64*/;

// The compiler knows that a print of a value of this type should use
// printquoted instead of printstring.
partial struct quoted /*@string*/;

// go2cs generated this placeholder — func bytes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// printBacklog is a circular buffer of messages written with the builtin
// print* functions, for use in postmortem analysis of core dumps.
internal static array<byte> printBacklog = new(512);
internal static nint printBacklogIndex;

// recordForPanic maintains a circular buffer of messages written by the
// runtime leading up to a process crash, allowing the messages to be
// extracted from a core dump.
//
// The text written during a process crash (following "panic" or "fatal
// error") is not saved, since the goroutine stacks will generally be readable
// from the runtime data structures in the core file.
internal static void recordForPanic(slice<byte> b) {
    printlock();

    if (Ꮡpanicking.Load() == 0) {
        // Not actively crashing: maintain circular buffer of print output.
        for (nint i = 0; i < len(b); ) {
            nint n = copy(printBacklog.slice(printBacklogIndex), b.slice(i));
            i += n;
            printBacklogIndex += n;
            printBacklogIndex %= len(printBacklog);
        }
    }

    printunlock();
}

internal static ж<mutex> Ꮡdebuglock = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex debuglock => ref Ꮡdebuglock.Value;

// The compiler emits calls to printlock and printunlock around
// the multiple calls that implement a single Go print or println
// statement. Some of the print helpers (printslice, for example)
// call print recursively. There is also the problem of a crash
// happening during the print routines and needing to acquire
// the print lock to print information about the crash.
// For both these reasons, let a thread acquire the printlock 'recursively'.
internal static void printlock() {
    var mp = getg().Value.m;
    mp.Value.locks++; // do not reschedule between printlock++ and lock(&debuglock).
    mp.Value.printlock++;
    if ((~mp).printlock == 1) {
        @lock(Ꮡdebuglock);
    }
    mp.Value.locks--; // now we know debuglock is held and holding up mp.locks for us.
}

internal static void printunlock() {
    var mp = getg().Value.m;
    mp.Value.printlock--;
    if ((~mp).printlock == 0) {
        unlock(Ꮡdebuglock);
    }
}

// write to goroutine-local buffer if diverting output,
// or else standard error.
internal static void gwrite(slice<byte> b) {
    if (len(b) == 0) {
        return;
    }
    recordForPanic(b);
    var gp = getg();
    // Don't use the writebuf if gp.m is dying. We want anything
    // written through gwrite to appear in the terminal rather
    // than be written to in some buffer, if we're in a panicking state.
    // Note that we can't just clear writebuf in the gp.m.dying case
    // because a panic isn't allowed to have any write barriers.
    if (gp == nil || (~gp).writebuf == default! || (~(~gp).m).dying > 0) {
        writeErr(b);
        return;
    }

    nint n = copy((~gp).writebuf.slice(len((~gp).writebuf), cap((~gp).writebuf)), b);
    gp.Value.writebuf = (~gp).writebuf.slice(0, len((~gp).writebuf) + n);
}

internal static void printsp() {
    printstring(" "u8);
}

internal static void printnl() {
    printstring("\n"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

internal static void printbool(bool v) {
    if (v){
        printstring(trueˢ);
    } else {
        printstring(falseˢ);
    }
}

// float64 requires 1+17+1+1+1+3 = 24 bytes max (sign+digits+decimal point+e+sign+exponent digits).
internal static UntypedInt float64Bytes => 24;

internal static void printfloat64(float64 v) {
    array<byte> buf = new(24); /* float64Bytes */
    gwrite(strconv.AppendFloat(buf[..0], v, (rune)'g', -1, 64));
}

// float32 requires 1+9+1+1+1+2 = 15 bytes max (sign+digits+decimal point+e+sign+exponent digits).
internal static UntypedInt float32Bytes => 15;

internal static void printfloat32(float32 v) {
    array<byte> buf = new(15); /* float32Bytes */
    gwrite(strconv.AppendFloat(buf[..0], (float64)v, (rune)'g', -1, 32));
}

// complex128 requires 24+24+1+1+1 = 51 bytes max (paren+float64+float64+i+paren).
internal static UntypedInt complex128Bytes => /* 2*float64Bytes + 3 */ 51;

internal static void printcomplex128(complex128 c) {
    array<byte> buf = new(51); /* complex128Bytes */
    gwrite(strconv.AppendComplex(buf[..0], c, (rune)'g', -1, 128));
}

// complex64 requires 15+15+1+1+1 = 33 bytes max (paren+float32+float32+i+paren).
internal static UntypedInt complex64Bytes => /* 2*float32Bytes + 3 */ 33;

internal static void printcomplex64(complex64 c) {
    array<byte> buf = new(33); /* complex64Bytes */
    gwrite(strconv.AppendComplex(buf[..0], (complex128)c, (rune)'g', -1, 64));
}

internal static void printuint(uint64 v) {
    // Note: Avoiding strconv.AppendUint so that it's clearer
    // that there are no allocations in this routine.
    // cmd/link/internal/ld.TestAbstractOriginSanity
    // sees the append and doesn't realize it doesn't allocate.
    array<byte> buf = new(20);
    nint i = strconv.RuntimeFormatBase10(buf[..], v);
    gwrite(buf.slice(i));
}

internal static void printint(int64 v) {
    // Note: Avoiding strconv.AppendUint so that it's clearer
    // that there are no allocations in this routine.
    // cmd/link/internal/ld.TestAbstractOriginSanity
    // sees the append and doesn't realize it doesn't allocate.
    var neg = v < 0;
    var u = (uint64)v;
    if (neg) {
        u = ((uint64)0 - u);
    }
    array<byte> buf = new(20);
    nint i = strconv.RuntimeFormatBase10(buf[..], u);
    if (neg) {
        i--;
        buf[i] = (rune)'-';
    }
    gwrite(buf.slice(i));
}

internal static nint minhexdigits = 0; // protected by printlock

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string digᶜ1 = "0123456789abcdef"u8;

internal static void printhexopts(bool include0x, nint mindigits, uint64 v) {
    @string dig = digᶜ1;
    array<byte> buf = new(100);
    nint i = len(buf);
    for (i--; i > 0; i--) {
        buf[i] = dig[v % 16];
        if (v < 16 && len(buf) - i >= mindigits) {
            break;
        }
        v /= 16;
    }
    if (include0x) {
        i--;
        buf[i] = (rune)'x';
        i--;
        buf[i] = (rune)'0';
    }
    gwrite(buf.slice(i));
}

internal static void printhex(uint64 v) {
    printhexopts(true, minhexdigits, v);
}

internal static void printquoted(@string s) {
    printlock();
    gwrite(slice<byte>(@""""u8));
    foreach (var (_, r) in s) {
        switch (r) {
        case (rune)'\n': {
            gwrite(slice<byte>(@"\n"u8));
            continue;
            break;
        }
        case (rune)'\r': {
            gwrite(slice<byte>(@"\r"u8));
            continue;
            break;
        }
        case (rune)'\t': {
            gwrite(slice<byte>(@"\t"u8));
            print();
            continue;
            break;
        }
        case (rune)'\\' or (rune)'"': {
            gwrite(new byte[]{(byte)(rune)'\\', (byte)r}.slice());
            continue;
            break;
        }}
        // For now, only allow basic printable ascii through unescaped
        if (r >= (rune)' ' && r <= (rune)'~'){
            gwrite(new byte[]{(byte)r}.slice());
        } else 
        if (r < 127){
            gwrite(bytes(@"\x"u8));
            printhexopts(false, 2, (uint64)r);
        } else 
        if (r < 0x1_0000){
            gwrite(bytes(@"\u"u8));
            printhexopts(false, 4, (uint64)r);
        } else {
            gwrite(bytes(@"\U"u8));
            printhexopts(false, 8, (uint64)r);
        }
    }
    gwrite(new byte[]{(byte)(rune)'"'}.slice());
    printunlock();
}

internal static void printpointer(@unsafe.Pointer Δp) {
    printhex((uint64)(uintptr)Δp);
}

internal static void printuintptr(uintptr Δp) {
    printhex((uint64)Δp);
}

internal static void printstring(@string s) {
    gwrite(bytes(s));
}

internal static void printslice(slice<byte> sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    var sp = Ꮡs.Reinterpret<slice<byte>, Δsliceᴛ>();
    print((@string)"["u8, len(s), (@string)"/"u8, cap(s), (@string)"]"u8);
    printpointer(sp.Δarray);
}

internal static void printeface(eface e) {
    print((@string)"("u8, e._type.OrTypedNil(), (@string)","u8, @unsafe.Pointer.OrTypedNil(e.data), (@string)")"u8);
}

internal static void printiface(iface i) {
    print((@string)"("u8, i.tab.OrTypedNil(), (@string)","u8, @unsafe.Pointer.OrTypedNil(i.data), (@string)")"u8);
}

} // end runtime_package
