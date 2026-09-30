// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

[GoType("num:uint64")] partial struct Δhex;

// go2cs generated this placeholder — func bytes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

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

internal static ж<mutex> Ꮡdebuglock = new StandardBox<mutex>(new mutex(nil));
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string naNˢ = "NaN"u8;
internal static readonly @string infˢ = "+Inf"u8;
internal static readonly @string infˢ2 = "-Inf"u8;

internal static void printfloat(float64 v) {
    switch (ᐧ) {
    case {} when v != v: {
        printstring(naNˢ);
        return;
    }
    case {} when v + v == v && v > 0D: {
        printstring(infˢ);
        return;
    }
    case {} when v + v == v && v < 0D: {
        printstring(infˢ2);
        return;
    }}

    UntypedInt n = 7; // digits printed
    array<byte> buf = new(14); /* n + 7 */
    buf[0] = (rune)'+';
    nint e = 0; // exp
    if (v == 0D){
        if (1D / v < 0D) {
            buf[0] = (rune)'-';
        }
    } else {
        if (v < 0D) {
            v = -v;
            buf[0] = (rune)'-';
        }
        // normalize
        while (v >= 10D) {
            e++;
            v /= 10D;
        }
        while (v < 1D) {
            e--;
            v *= 10D;
        }
        // round
        var h = 5.0D;
        for (nint i = 0; i < n; i++) {
            h /= 10D;
        }
        v += h;
        if (v >= 10D) {
            e++;
            v /= 10D;
        }
    }
    // format +d.dddd+edd
    for (nint i = 0; i < n; i++) {
        nint s = (nint)v;
        buf[i + 2] = (byte)(s + (rune)'0');
        v -= (float64)s;
        v *= 10D;
    }
    buf[1] = buf[2];
    buf[2] = (rune)'.';
    buf[n + 2] = (rune)'e';
    buf[n + 3] = (rune)'+';
    if (e < 0) {
        e = -e;
        buf[n + 3] = (rune)'-';
    }
    buf[n + 4] = (byte)((byte)(e / 100) + (rune)'0');
    buf[n + 5] = (byte)((byte)(e / 10) % 10 + (rune)'0');
    buf[n + 6] = (byte)((byte)(e % 10) + (rune)'0');
    gwrite(buf[..]);
}

internal static void printcomplex(complex128 c) {
    print((@string)"("u8, real(c), imag(c), (@string)"i)"u8);
}

internal static void printuint(uint64 v) {
    array<byte> buf = new(100);
    nint i = len(buf);
    for (i--; i > 0; i--) {
        buf[i] = (byte)(v % 10 + (rune)'0');
        if (v < 10) {
            break;
        }
        v /= 10;
    }
    gwrite(buf.slice(i));
}

internal static void printint(int64 v) {
    if (v < 0) {
        printstring("-"u8);
        v = -v;
    }
    printuint((uint64)v);
}

internal static nint minhexdigits = 0; // protected by printlock

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string digᶜ = "0123456789abcdef"u8;

internal static void printhex(uint64 v) {
    @string dig = digᶜ;
    array<byte> buf = new(100);
    nint i = len(buf);
    for (i--; i > 0; i--) {
        buf[i] = dig[v % 16];
        if (v < 16 && len(buf) - i >= minhexdigits) {
            break;
        }
        v /= 16;
    }
    i--;
    buf[i] = (rune)'x';
    i--;
    buf[i] = (rune)'0';
    gwrite(buf.slice(i));
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
    printpointer((~sp).Δarray);
}

internal static void printeface(eface e) {
    print((@string)"("u8, e._type.OrTypedNil(), (@string)","u8, @unsafe.Pointer.OrTypedNil(e.data), (@string)")"u8);
}

internal static void printiface(iface i) {
    print((@string)"("u8, i.tab.OrTypedNil(), (@string)","u8, @unsafe.Pointer.OrTypedNil(i.data), (@string)")"u8);
}

// hexdumpWords prints a word-oriented hex dump of [p, end).
//
// If mark != nil, it will be called with each printed word's address
// and should return a character mark to appear just before that
// word's value. It can return 0 to indicate no mark.
internal static void hexdumpWords(uintptr Δp, uintptr end, Func<uintptr, byte> mark) {
    printlock();
    array<byte> markbuf = new(1);
    markbuf[0] = (rune)' ';
    minhexdigits = (nint)(/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 * 2);
    for (var i = (uintptr)0; Δp + i < end; i += goarch.PtrSize) {
        if (i % 16 == 0) {
            if (i != 0) {
                println();
            }
            print(((Δhex)(uint64)(Δp + i)), (@string)": "u8);
        }
        if (mark != default!) {
            markbuf[0] = mark(Δp + i);
            if (markbuf[0] == 0) {
                markbuf[0] = (rune)' ';
            }
        }
        gwrite(markbuf[..]);
        var val = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(Δp + i));
        print(((Δhex)(uint64)val));
        print((@string)" "u8);
        // Can we symbolize val?
        var fn = findfunc(val);
        if (fn.valid()) {
            print((@string)"<"u8, funcname(fn), (@string)"+"u8, ((Δhex)(uint64)(val - fn.entry())), (@string)"> "u8);
        }
    }
    minhexdigits = 0;
    println();
    printunlock();
}

} // end runtime_package
