// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file provides an internal debug logging facility. The debug
// log is a lightweight, in-memory, per-M ring buffer. By default, the
// runtime prints the debug log on panic.
//
// To print something to the debug log, call dlog to obtain a dlogger
// and use the methods on that to add values. The values will be
// space-separated in the output (much like println).
//
// This facility can be enabled by passing -tags debuglog when
// building. Without this tag, dlog calls compile to nothing.
//
// Implementation notes
//
// There are two implementations of the dlog interface: dloggerImpl and
// dloggerFake. dloggerFake is a no-op implementation. dlogger is type-aliased
// to one or the other depending on the debuglog build tag. However, both types
// always exist and are always built. This helps ensure we compile as much of
// the implementation as possible in the default build configuration, while also
// enabling us to achieve good test coverage of the real debuglog implementation
// even when the debuglog build tag is not set.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// debugLogBytes is the size of each per-M ring buffer. This is
// allocated off-heap to avoid blowing up the M and hence the GC'd
// heap size.
internal static UntypedInt debugLogBytes => /* 16 << 10 */ 16384;

// debugLogStringLimit is the maximum number of bytes in a string.
// Above this, the string will be truncated with "..(n more bytes).."
internal static UntypedInt debugLogStringLimit => /* debugLogBytes / 8 */ 2048;

// dlog returns a debug logger. The caller can use methods on the
// returned logger to add values, which will be space-separated in the
// final output, much like println. The caller must call end() to
// finish the message.
//
// dlog can be used from highly-constrained corners of the runtime: it
// is safe to use in the signal handler, from within the write
// barrier, from within the stack implementation, and in places that
// must be recursively nosplit.
//
// This will be compiled away if built without the debuglog build tag.
// However, argument construction may not be. If any of the arguments
// are not literals or trivial expressions, consider protecting the
// call with "if dlogEnabled".
//
//go:nosplit
//go:nowritebarrierrec
internal static dlogger dlog() {
    // dlog1 is defined to either dlogImpl or dlogFake.
    return dlog1();
}

//go:nosplit
//go:nowritebarrierrec
internal static dloggerFake dlogFake() {
    return new dloggerFake(nil);
}

// go2cs generated this placeholder — func dlogImpl is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// A dloggerImpl writes to the debug log.
//
// To obtain a dloggerImpl, call dlog(). When done with the dloggerImpl, call
// end().
[GoType] partial struct dloggerImpl {
    internal sys.NotInHeap _;
    internal debugLogWriter w;
    // allLink is the next dlogger in the allDloggers list.
    internal ж<dloggerImpl> allLink;
    // owned indicates that this dlogger is owned by an M. This is
    // accessed atomically.
    internal atomic.Uint32 owned;
}

// allDloggers is a list of all dloggers, linked through
// dlogger.allLink. This is accessed atomically. This is prepend only,
// so it doesn't need to protect against ABA races.
internal static ж<ж<dloggerImpl>> ᏑallDloggers = new StandardBox<ж<dloggerImpl>>(default(ж<dloggerImpl>));
internal static ref ж<dloggerImpl> allDloggers => ref ᏑallDloggers.ValueSlot;

// A dloggerFake is a no-op implementation of dlogger.
[GoType] partial struct dloggerFake {
}

//go:nosplit
internal static void end(this dloggerFake l) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recordTooLargeˢ = "record too large"u8;

//go:nosplit
internal static void end(this ж<dloggerImpl> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    // Fill in framing header.
    var size = l.w.write - l.w.r.end;
    if (!l.w.writeFrameAt(l.w.r.end, size)) {
        @throw(recordTooLargeˢ);
    }
    // Commit the record.
    l.w.r.end = l.w.write;
    // Attempt to return this logger to the cache.
    if (putCachedDlogger(ref (Ꮡl).DerefOrNull())) {
        return;
    }
    // Return the logger to the global pool.
    Ꮡl.of(dloggerImpl.Ꮡowned).Store(0);
}

internal static UntypedInt debugLogUnknown => /* 1 + iota */ 1;
internal static UntypedInt debugLogBoolTrue => 2;
internal static UntypedInt debugLogBoolFalse => 3;
internal static UntypedInt debugLogInt => 4;
internal static UntypedInt debugLogUint => 5;
internal static UntypedInt debugLogHex => 6;
internal static UntypedInt debugLogPtr => 7;
internal static UntypedInt debugLogString => 8;
internal static UntypedInt debugLogConstString => 9;
internal static UntypedInt debugLogStringOverflow => 10;
internal static UntypedInt debugLogPC => 11;
internal static UntypedInt debugLogTraceback => 12;

//go:nosplit
internal static dloggerFake b(this dloggerFake l, bool x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> b(this ж<dloggerImpl> Ꮡl, bool x) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (x){
        l.w.@byte(debugLogBoolTrue);
    } else {
        l.w.@byte(debugLogBoolFalse);
    }
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake i(this dloggerFake l, nint x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> i(this ж<dloggerImpl> Ꮡl, nint x) {
    return Ꮡl.i64((int64)x);
}

//go:nosplit
internal static dloggerFake i8(this dloggerFake l, int8 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> i8(this ж<dloggerImpl> Ꮡl, int8 x) {
    return Ꮡl.i64((int64)x);
}

//go:nosplit
internal static dloggerFake i16(this dloggerFake l, int16 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> i16(this ж<dloggerImpl> Ꮡl, int16 x) {
    return Ꮡl.i64((int64)x);
}

//go:nosplit
internal static dloggerFake i32(this dloggerFake l, int32 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> i32(this ж<dloggerImpl> Ꮡl, int32 x) {
    return Ꮡl.i64((int64)x);
}

//go:nosplit
internal static dloggerFake i64(this dloggerFake l, int64 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> i64(this ж<dloggerImpl> Ꮡl, int64 x) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.w.@byte(debugLogInt);
    l.w.varint(x);
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake u(this dloggerFake l, nuint x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> u(this ж<dloggerImpl> Ꮡl, nuint x) {
    return Ꮡl.u64((uint64)x);
}

//go:nosplit
internal static dloggerFake uptr(this dloggerFake l, uintptr x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> uptr(this ж<dloggerImpl> Ꮡl, uintptr x) {
    return Ꮡl.u64((uint64)x);
}

//go:nosplit
internal static dloggerFake u8(this dloggerFake l, uint8 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> u8(this ж<dloggerImpl> Ꮡl, uint8 x) {
    return Ꮡl.u64((uint64)x);
}

//go:nosplit
internal static dloggerFake u16(this dloggerFake l, uint16 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> u16(this ж<dloggerImpl> Ꮡl, uint16 x) {
    return Ꮡl.u64((uint64)x);
}

//go:nosplit
internal static dloggerFake u32(this dloggerFake l, uint32 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> u32(this ж<dloggerImpl> Ꮡl, uint32 x) {
    return Ꮡl.u64((uint64)x);
}

//go:nosplit
internal static dloggerFake u64(this dloggerFake l, uint64 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> u64(this ж<dloggerImpl> Ꮡl, uint64 x) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.w.@byte(debugLogUint);
    l.w.uvarint(x);
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake hex(this dloggerFake l, uint64 x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> hex(this ж<dloggerImpl> Ꮡl, uint64 x) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.w.@byte(debugLogHex);
    l.w.uvarint(x);
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake p(this dloggerFake l, any x) {
    return l;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notAPointerTypeˢ = "not a pointer type"u8;

//go:nosplit
internal static ж<dloggerImpl> p(this ж<dloggerImpl> Ꮡl, any xʗp) {
    ref var l = ref Ꮡl.DerefOrNull();

    ref var x = ref heap(xʗp, out var Ꮡx);
    l.w.@byte(debugLogPtr);
    if (x == default!){
        l.w.uvarint(0);
    } else {
        var v = efaceOf(Ꮡx);
        var exprᴛ1 = (abiꓸKind)((~(~v)._type).Kind_ & abi.KindMask);
        if (exprᴛ1 == abi.Chan || exprᴛ1 == abi.Func || exprᴛ1 == abi.Map || exprᴛ1 == abi.Pointer || exprᴛ1 == abi.UnsafePointer) {
            l.w.uvarint((uint64)(uintptr)(~v).data);
        }
        else { /* default: */
            @throw(notAPointerTypeˢ);
        }

    }
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake s(this dloggerFake l, @string x) {
    return l;
}

// go2cs generated this placeholder — func s is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:nosplit
internal static dloggerFake pc(this dloggerFake l, uintptr x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> pc(this ж<dloggerImpl> Ꮡl, uintptr x) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.w.@byte(debugLogPC);
    l.w.uvarint((uint64)x);
    return Ꮡl;
}

//go:nosplit
internal static dloggerFake traceback(this dloggerFake l, slice<uintptr> x) {
    return l;
}

//go:nosplit
internal static ж<dloggerImpl> traceback(this ж<dloggerImpl> Ꮡl, slice<uintptr> x) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.w.@byte(debugLogTraceback);
    l.w.uvarint((uint64)len(x));
    foreach (var (_, pc) in x) {
        l.w.uvarint((uint64)pc);
    }
    return Ꮡl;
}

// A debugLogWriter is a ring buffer of binary debug log records.
//
// A log record consists of a 2-byte framing header and a sequence of
// fields. The framing header gives the size of the record as a little
// endian 16-bit value. Each field starts with a byte indicating its
// type, followed by type-specific data. If the size in the framing
// header is 0, it's a sync record consisting of two little endian
// 64-bit values giving a new time base.
//
// Because this is a ring buffer, new records will eventually
// overwrite old records. Hence, it maintains a reader that consumes
// the log as it gets overwritten. That reader state is where an
// actual log reader would start.
[GoType] partial struct debugLogWriter {
    internal sys.NotInHeap _;
    internal uint64 write;
    internal debugLogBuf data;
    // tick and nano are the time bases from the most recently
    // written sync record.
    internal uint64 tick, nano;
    // r is a reader that consumes records as they get overwritten
    // by the writer. It also acts as the initial reader state
    // when printing the log.
    internal debugLogReader r;
    // buf is a scratch buffer for encoding. This is here to
    // reduce stack usage.
    internal array<byte> buf = new(10);
}

[GoType] partial struct debugLogBuf {
    internal sys.NotInHeap _;
    internal array<byte> b = new(debugLogBytes);
}

internal static UntypedInt debugLogHeaderSize => 2;
internal static UntypedInt debugLogSyncSize => /* debugLogHeaderSize + 2*8 */ 18;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recordWrappedAroundˢ = "record wrapped around"u8;

//go:nosplit
[GoRecv] internal static void ensure(this ref debugLogWriter l, uint64 n) {
    while (l.write + n >= l.r.begin + (uint64)len(l.data.b)) {
        // Consume record at begin.
        if (l.r.skip() == ~(uint64)0) {
            // Wrapped around within a record.
            //
            // TODO(austin): It would be better to just
            // eat the whole buffer at this point, but we
            // have to communicate that to the reader
            // somehow.
            @throw(recordWrappedAroundˢ);
        }
    }
}

//go:nosplit
[GoRecv] internal static bool writeFrameAt(this ref debugLogWriter l, uint64 pos, uint64 size) {
    l.data.b[pos % (uint64)len(l.data.b)] = (uint8)size;
    l.data.b[(pos + 1) % (uint64)len(l.data.b)] = (uint8)((size >> (int)(8)));
    return size <= 0xFFFF;
}

//go:nosplit
[GoRecv] internal static void writeSync(this ref debugLogWriter l, uint64 tick, uint64 nano) {
    (l.tick, l.nano) = (tick, nano);
    l.ensure(debugLogHeaderSize);
    l.writeFrameAt(l.write, 0);
    l.write += debugLogHeaderSize;
    l.writeUint64LE(tick);
    l.writeUint64LE(nano);
    l.r.end = l.write;
}

//go:nosplit
[GoRecv] internal static void writeUint64LE(this ref debugLogWriter l, uint64 x) {
    array<byte> b = new(8);
    b[0] = (byte)x;
    b[1] = (byte)((x >> (int)(8)));
    b[2] = (byte)((x >> (int)(16)));
    b[3] = (byte)((x >> (int)(24)));
    b[4] = (byte)((x >> (int)(32)));
    b[5] = (byte)((x >> (int)(40)));
    b[6] = (byte)((x >> (int)(48)));
    b[7] = (byte)((x >> (int)(56)));
    l.bytes(b[..]);
}

//go:nosplit
[GoRecv] internal static void @byte(this ref debugLogWriter l, byte x) {
    l.ensure(1);
    var pos = l.write;
    l.write++;
    l.data.b[pos % (uint64)len(l.data.b)] = x;
}

//go:nosplit
[GoRecv] internal static void bytes(this ref debugLogWriter l, slice<byte> x) {
    l.ensure((uint64)len(x));
    var pos = l.write;
    l.write += (uint64)len(x);
    while (len(x) > 0) {
        nint n = copy(l.data.b.slice((nint)(pos % (uint64)len(l.data.b))), x);
        pos += (uint64)n;
        x = x.slice(n);
    }
}

//go:nosplit
[GoRecv] internal static void varint(this ref debugLogWriter l, int64 x) {
    uint64 u = default!;
    if (x < 0){
        u = (uint64)(((~(uint64)x << (int)(1))) | 1); // complement i, bit 0 is 1
    } else {
        u = (((uint64)x << (int)(1))); // do not complement i, bit 0 is 0
    }
    l.uvarint(u);
}

//go:nosplit
[GoRecv] internal static void uvarint(this ref debugLogWriter l, uint64 u) {
    nint i = 0;
    while (u >= 0x80) {
        l.buf[i] = (byte)((byte)u | 0x80);
        u >>= (int)(7);
        i++;
    }
    l.buf[i] = (byte)u;
    i++;
    l.bytes(l.buf.slice(0, i));
}

[GoType] partial struct debugLogReader {
    internal ж<debugLogBuf> data;
    // begin and end are the positions in the log of the beginning
    // and end of the log data, modulo len(data).
    internal uint64 begin, end;
    // tick and nano are the current time base at begin.
    internal uint64 tick, nano;
}

//go:nosplit
[GoRecv] internal static uint64 skip(this ref debugLogReader r) {
    // Read size at pos.
    if (r.begin + (uint64)debugLogHeaderSize > r.end) {
        return ~(uint64)0;
    }
    var size = (uint64)r.readUint16LEAt(r.begin);
    if (size == 0) {
        // Sync packet.
        r.tick = r.readUint64LEAt(r.begin + (uint64)debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + (uint64)debugLogHeaderSize + 8);
        size = debugLogSyncSize;
    }
    if (r.begin + size > r.end) {
        return ~(uint64)0;
    }
    r.begin += size;
    return size;
}

//go:nosplit
[GoRecv] internal static uint16 readUint16LEAt(this ref debugLogReader r, uint64 pos) {
    return (uint16)((uint16)(~r.data).b[pos % (uint64)len((~r.data).b)] | (uint16)((uint16)(~r.data).b[(pos + 1) % (uint64)len((~r.data).b)] << (int)(8)));
}

//go:nosplit
[GoRecv] internal static uint64 readUint64LEAt(this ref debugLogReader r, uint64 pos) {
    array<byte> b = new(8);
    foreach (var (i, _) in b) {
        b[i] = (~r.data).b[pos % (uint64)len((~r.data).b)];
        pos++;
    }
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[0] | ((uint64)b[1] << (int)(8))) | ((uint64)b[2] << (int)(16))) | ((uint64)b[3] << (int)(24))) | ((uint64)b[4] << (int)(32))) | ((uint64)b[5] << (int)(40))) | ((uint64)b[6] << (int)(48))) | ((uint64)b[7] << (int)(56)));
}

[GoRecv] internal static uint64 /*tick*/ peek(this ref debugLogReader r) {
    // Consume any sync records.
    var size = (uint64)0;
    while (size == 0) {
        if (r.begin + (uint64)debugLogHeaderSize > r.end) {
            return ~(uint64)0;
        }
        size = (uint64)r.readUint16LEAt(r.begin);
        if (size != 0) {
            break;
        }
        if (r.begin + (uint64)debugLogSyncSize > r.end) {
            return ~(uint64)0;
        }
        // Sync packet.
        r.tick = r.readUint64LEAt(r.begin + (uint64)debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + (uint64)debugLogHeaderSize + 8);
        r.begin += debugLogSyncSize;
    }
    // Peek tick delta.
    if (r.begin + size > r.end) {
        return ~(uint64)0;
    }
    var pos = r.begin + (uint64)debugLogHeaderSize;
    uint64 u = default!;
    for (nuint i = (nuint)0; ᐧ ; i += 7) {
        var b = (~r.data).b[pos % (uint64)len((~r.data).b)];
        pos++;
        u |= (uint64)(((uint64)((byte)(b & ~0x80))).Lsh(i));
        if ((byte)(b & 0x80) == 0) {
            break;
        }
    }
    if (pos > r.begin + size) {
        return ~(uint64)0;
    }
    return r.tick + u;
}

[GoRecv] internal static (uint64 end, uint64 tick, uint64 nano, nint Δp) header(this ref debugLogReader r) {
    uint64 end = default!;
    uint64 tick = default!;
    uint64 nano = default!;
    nint Δp = default!;

    // Read size. We've already skipped sync packets and checked
    // bounds in peek.
    var size = (uint64)r.readUint16LEAt(r.begin);
    end = r.begin + size;
    r.begin += debugLogHeaderSize;
    // Read tick, nano, and p.
    tick = r.uvarint() + r.tick;
    nano = r.uvarint() + r.nano;
    Δp = (nint)r.varint();
    return (end, tick, nano, Δp);
}

[GoRecv] internal static uint64 uvarint(this ref debugLogReader r) {
    uint64 u = default!;
    for (nuint i = (nuint)0; ᐧ ; i += 7) {
        var b = (~r.data).b[r.begin % (uint64)len((~r.data).b)];
        r.begin++;
        u |= (uint64)(((uint64)((byte)(b & ~0x80))).Lsh(i));
        if ((byte)(b & 0x80) == 0) {
            break;
        }
    }
    return u;
}

[GoRecv] internal static int64 varint(this ref debugLogReader r) {
    var u = r.uvarint();
    int64 v = default!;
    if ((uint64)(u & 1) == 0){
        v = (int64)((u >> (int)(1)));
    } else {
        v = ~(int64)((u >> (int)(1)));
    }
    return v;
}

// go2cs generated this placeholder — func printVal is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// printDebugLog prints the debug log.
internal static void printDebugLog() {
    if (dlogEnabled) {
        printDebugLogImpl();
    }
}

// go2cs generated this placeholder — func printDebugLogImpl is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func printDebugLogPC is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end runtime_package
