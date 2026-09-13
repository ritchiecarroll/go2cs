// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using fmt = fmt_package;
using abi = go.@internal.abi_package;
using io = io_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using @unsafe = unsafe_package;
using compress;
using go.@internal;

partial class pprof_package {

// lostProfileEvent is the function to which lost profiling
// events are attributed.
// (The name shows up in the pprof graphs.)
internal static void lostProfileEvent() {
    lostProfileEvent();
}

// A profileBuilder writes a profile incrementally from a
// stream of profile samples delivered by the runtime.
[GoType] partial struct profileBuilder {
    internal time.Time start;
    internal time.Time end;
    internal bool havePeriod;
    internal int64 period;
    internal profMap m;
    // encoding state
    internal io.Writer w;
    internal ж<gzip.Writer> zw;
    internal protobuf pb;
    internal slice<@string> strings;
    internal map<@string, nint> stringMap;
    internal map<uintptr, locInfo> locs; // list of locInfo starting with the given PC.
    internal map<@string, nint> funcs;  // Package path-qualified function name to Function.ID
    internal slice<memMap> mem;
    internal pcDeck deck;
}

[GoType] partial struct memMap {
    // initialized as reading mapping
    internal uintptr start; // Address at which the binary (or DLL) is loaded into memory.
    internal uintptr end; // The limit of the address range occupied by this mapping.
    internal uint64 offset;  // Offset in the binary that corresponds to the first mapped address.
    internal @string @file; // The object this entry is loaded from.
    internal @string buildID; // A string that uniquely identifies a particular program version with high probability.
    internal symbolizeFlag funcs;
    internal bool fake; // map entry was faked; /proc/self/maps wasn't available
}

[GoType("num:uint8")] partial struct symbolizeFlag;

internal static symbolizeFlag lookupTried => /* 1 << iota */ 1;
internal static symbolizeFlag lookupFailed => /* 1 << iota */ 2;

internal static UntypedInt tagProfile_SampleType => 1; // repeated ValueType
internal static UntypedInt tagProfile_Sample => 2; // repeated Sample
internal static UntypedInt tagProfile_Mapping => 3; // repeated Mapping
internal static UntypedInt tagProfile_Location => 4; // repeated Location
internal static UntypedInt tagProfile_Function => 5; // repeated Function
internal static UntypedInt tagProfile_StringTable => 6; // repeated string
internal static UntypedInt tagProfile_DropFrames => 7; // int64 (string table index)
internal static UntypedInt tagProfile_KeepFrames => 8; // int64 (string table index)
internal static UntypedInt tagProfile_TimeNanos => 9; // int64
internal static UntypedInt tagProfile_DurationNanos => 10; // int64
internal static UntypedInt tagProfile_PeriodType => 11; // ValueType (really optional string???)
internal static UntypedInt tagProfile_Period => 12; // int64
internal static UntypedInt tagProfile_Comment => 13; // repeated int64
internal static UntypedInt tagProfile_DefaultSampleType => 14; // int64
internal static UntypedInt tagValueType_Type => 1; // int64 (string table index)
internal static UntypedInt tagValueType_Unit => 2; // int64 (string table index)
internal static UntypedInt tagSample_Location => 1; // repeated uint64
internal static UntypedInt tagSample_Value => 2; // repeated int64
internal static UntypedInt tagSample_Label => 3; // repeated Label
internal static UntypedInt tagLabel_Key => 1; // int64 (string table index)
internal static UntypedInt tagLabel_Str => 2; // int64 (string table index)
internal static UntypedInt tagLabel_Num => 3; // int64
internal static UntypedInt tagMapping_ID => 1; // uint64
internal static UntypedInt tagMapping_Start => 2; // uint64
internal static UntypedInt tagMapping_Limit => 3; // uint64
internal static UntypedInt tagMapping_Offset => 4; // uint64
internal static UntypedInt tagMapping_Filename => 5; // int64 (string table index)
internal static UntypedInt tagMapping_BuildID => 6; // int64 (string table index)
internal static UntypedInt tagMapping_HasFunctions => 7; // bool
internal static UntypedInt tagMapping_HasFilenames => 8; // bool
internal static UntypedInt tagMapping_HasLineNumbers => 9; // bool
internal static UntypedInt tagMapping_HasInlineFrames => 10; // bool
internal static UntypedInt tagLocation_ID => 1; // uint64
internal static UntypedInt tagLocation_MappingID => 2; // uint64
internal static UntypedInt tagLocation_Address => 3; // uint64
internal static UntypedInt tagLocation_Line => 4; // repeated Line
internal static UntypedInt tagLine_FunctionID => 1; // uint64
internal static UntypedInt tagLine_Line => 2; // int64
internal static UntypedInt tagFunction_ID => 1; // uint64
internal static UntypedInt tagFunction_Name => 2; // int64 (string table index)
internal static UntypedInt tagFunction_SystemName => 3; // int64 (string table index)
internal static UntypedInt tagFunction_Filename => 4; // int64 (string table index)
internal static UntypedInt tagFunction_StartLine => 5; // int64

// stringIndex adds s to the string table if not already present
// and returns the index of s in the string table.
[GoRecv] internal static int64 stringIndex(this ref profileBuilder b, @string s) {
    var (id, ok) = b.stringMap[s, ꟷ];
    if (!ok) {
        id = len(b.strings);
        b.strings = append(b.strings, s);
        b.stringMap[s] = id;
    }
    return (int64)id;
}

[GoRecv] internal static void flush(this ref profileBuilder b) {
    const nint dataFlush = 4096;
    if (b.pb.nest == 0 && len(b.pb.data) > dataFlush) {
        b.zw.Write(b.pb.data);
        b.pb.data = b.pb.data[..0];
    }
}

// pbValueType encodes a ValueType message to b.pb.
[GoRecv] internal static void pbValueType(this ref profileBuilder b, nint tag, @string typ, @string unit) {
    msgOffset start = b.pb.startMessage();
    b.pb.int64(tagValueType_Type, b.stringIndex(typ));
    b.pb.int64(tagValueType_Unit, b.stringIndex(unit));
    b.pb.endMessage(tag, start);
}

// pbSample encodes a Sample message to b.pb.
[GoRecv] internal static void pbSample(this ref profileBuilder b, slice<int64> values, slice<uint64> locs, Action labels) {
    msgOffset start = b.pb.startMessage();
    b.pb.int64s(tagSample_Value, values);
    b.pb.uint64s(tagSample_Location, locs);
    if (labels != default!) {
        labels();
    }
    b.pb.endMessage(tagProfile_Sample, start);
    b.flush();
}

// pbLabel encodes a Label message to b.pb.
[GoRecv] internal static void pbLabel(this ref profileBuilder b, nint tag, @string key, @string str, int64 num) {
    msgOffset start = b.pb.startMessage();
    b.pb.int64Opt(tagLabel_Key, b.stringIndex(key));
    b.pb.int64Opt(tagLabel_Str, b.stringIndex(str));
    b.pb.int64Opt(tagLabel_Num, num);
    b.pb.endMessage(tag, start);
}

// pbLine encodes a Line message to b.pb.
[GoRecv] internal static void pbLine(this ref profileBuilder b, nint tag, uint64 funcID, int64 line) {
    msgOffset start = b.pb.startMessage();
    b.pb.uint64Opt(tagLine_FunctionID, funcID);
    b.pb.int64Opt(tagLine_Line, line);
    b.pb.endMessage(tag, start);
}

// pbMapping encodes a Mapping message to b.pb.
[GoRecv] internal static void pbMapping(this ref profileBuilder b, nint tag, uint64 id, uint64 @base, uint64 limit, uint64 offset, @string @file, @string buildID, bool hasFuncs) {
    msgOffset start = b.pb.startMessage();
    b.pb.uint64Opt(tagMapping_ID, id);
    b.pb.uint64Opt(tagMapping_Start, @base);
    b.pb.uint64Opt(tagMapping_Limit, limit);
    b.pb.uint64Opt(tagMapping_Offset, offset);
    b.pb.int64Opt(tagMapping_Filename, b.stringIndex(@file));
    b.pb.int64Opt(tagMapping_BuildID, b.stringIndex(buildID));
    // TODO: we set HasFunctions if all symbols from samples were symbolized (hasFuncs).
    // Decide what to do about HasInlineFrames and HasLineNumbers.
    // Also, another approach to handle the mapping entry with
    // incomplete symbolization results is to duplicate the mapping
    // entry (but with different Has* fields values) and use
    // different entries for symbolized locations and unsymbolized locations.
    if (hasFuncs) {
        b.pb.@bool(tagMapping_HasFunctions, true);
    }
    b.pb.endMessage(tag, start);
}

internal static (slice<runtime.Frame>, symbolizeFlag) allFrames(uintptr addr) {
    // Expand this one address using CallersFrames so we can cache
    // each expansion. In general, CallersFrames takes a whole
    // stack, but in this case we know there will be no skips in
    // the stack and we have return PCs anyway.
    var frames = runtime.CallersFrames(new uintptr[]{addr}.slice());
    var (frame, more) = frames.Next();
    if (frame.Function == "runtime.goexit"u8) {
        // Short-circuit if we see runtime.goexit so the loop
        // below doesn't allocate a useless empty location.
        return (default!, 0);
    }
    var symbolizeResult = lookupTried;
    if (frame.PC == 0 || frame.Function == ""u8 || frame.File == ""u8 || frame.Line == 0) {
        symbolizeResult |= (symbolizeFlag)(lookupFailed);
    }
    if (frame.PC == 0) {
        // If we failed to resolve the frame, at least make up
        // a reasonable call PC. This mostly happens in tests.
        frame.PC = addr - 1;
    }
    var ret = new runtime.Frame[]{frame}.slice();
    while (frame.Function != "runtime.goexit"u8 && more) {
        (frame, more) = frames.Next();
        ret = append(ret, frame);
    }
    return (ret, symbolizeResult);
}

[GoType] partial struct locInfo {
    // location id assigned by the profileBuilder
    internal uint64 id;
    // sequence of PCs, including the fake PCs returned by the traceback
    // to represent inlined functions
    // https://github.com/golang/go/blob/d6f2f833c93a41ec1c68e49804b8387a06b131c5/src/runtime/traceback.go#L347-L368
    internal slice<uintptr> pcs;
    // firstPCFrames and firstPCSymbolizeResult hold the results of the
    // allFrames call for the first (leaf-most) PC this locInfo represents
    internal slice<runtime.Frame> firstPCFrames;
    internal symbolizeFlag firstPCSymbolizeResult;
}

// newProfileBuilder returns a new profileBuilder.
// CPU profiling data obtained from the runtime can be added
// by calling b.addCPUData, and then the eventual profile
// can be obtained by calling b.finish.
internal static ж<profileBuilder> newProfileBuilder(io.Writer w) {
    var (zw, _) = gzip.NewWriterLevel(w, gzip.BestSpeed);
    var b = Ꮡ(new profileBuilder(
        w: w,
        zw: zw,
        start: time.Now(),
        strings: new @string[]{""u8}.slice(),
        stringMap: new map<@string, nint>{[""u8] = 0},
        locs: new map<uintptr, locInfo>{},
        funcs: new map<@string, nint>{}
    ));
    b.readMapping();
    return b;
}

// addCPUData adds the CPU profiling data to the profile.
//
// The data must be a whole number of records, as delivered by the runtime.
// len(tags) must be equal to the number of records in data.
[GoRecv] internal static error addCPUData(this ref profileBuilder b, slice<uint64> data, slice<@unsafe.Pointer> tags) {
    if (!b.havePeriod) {
        // first record is period
        if (len(data) < 3) {
            return fmt.Errorf("truncated profile"u8);
        }
        if (data[0] != 3 || data[2] == 0) {
            return fmt.Errorf("malformed profile"u8);
        }
        // data[2] is sampling rate in Hz. Convert to sampling
        // period in nanoseconds.
        b.period = 1000000000 / (int64)data[2];
        b.havePeriod = true;
        data = data[3..];
        // Consume tag slot. Note that there isn't a meaningful tag
        // value for this record.
        tags = tags[1..];
    }
    // Parse CPU samples from the profile.
    // Each sample is 3+n uint64s:
    //	data[0] = 3+n
    //	data[1] = time stamp (ignored)
    //	data[2] = count
    //	data[3:3+n] = stack
    // If the count is 0 and the stack has length 1,
    // that's an overflow record inserted by the runtime
    // to indicate that stack[0] samples were lost.
    // Otherwise the count is usually 1,
    // but in a few special cases like lost non-Go samples
    // there can be larger counts.
    // Because many samples with the same stack arrive,
    // we want to deduplicate immediately, which we do
    // using the b.m profMap.
    while (len(data) > 0) {
        if (len(data) < 3 || data[0] > (uint64)len(data)) {
            return fmt.Errorf("truncated profile"u8);
        }
        if (data[0] < 3 || tags != default! && len(tags) < 1) {
            return fmt.Errorf("malformed profile"u8);
        }
        if (len(tags) < 1) {
            return fmt.Errorf("mismatched profile records and tags"u8);
        }
        var count = data[2];
        var stk = data[3..(int)(data[0])];
        data = data[(int)(data[0])..];
        @unsafe.Pointer tag = tags[0];
        tags = tags[1..];
        if (count == 0 && len(stk) == 1) {
            // overflow record
            count = (uint64)stk[0];
            stk = new uint64[]{ // gentraceback guarantees that PCs in the
 // stack can be unconditionally decremented and
 // still be valid, so we must do the same.

                (uint64)(abi.FuncPCABIInternal(lostProfileEvent) + 1)
            }.slice();
        }
        b.m.lookup(stk, tag).Value.count += (int64)count;
    }
    if (len(tags) != 0) {
        return fmt.Errorf("mismatched profile records and tags"u8);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string samplesˢ = "samples"u8;
internal static readonly @string cpuˢ = "cpu"u8;

// build completes and returns the constructed profile.
internal static void build(this ж<profileBuilder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.end = time.Now();
    b.pb.int64Opt(tagProfile_TimeNanos, b.start.UnixNano());
    if (b.havePeriod) {
        // must be CPU profile
        b.pbValueType(tagProfile_SampleType, samplesˢ, countˢ);
        b.pbValueType(tagProfile_SampleType, cpuˢ, nanosecondsˢ);
        b.pb.int64Opt(tagProfile_DurationNanos, b.end.Sub(b.start).Nanoseconds());
        b.pbValueType(tagProfile_PeriodType, cpuˢ, nanosecondsˢ);
        b.pb.int64Opt(tagProfile_Period, b.period);
    }
    var values = new int64[]{0, 0}.slice();
    slice<uint64> locs = default!;
    for (var eᴛ1 = b.m.all; eᴛ1 != nil; eᴛ1 = eᴛ1.Value.nextAll) {
        var e = eᴛ1;
        values[0] = e.Value.count;
        values[1] = (~e).count * b.period;
        Action labels = default!;
        if ((~e).tag != nil) {
            var eʗ1 = e;
            labels = () => {
                foreach (var (k, v) in ~(ж<labelMap>)(uintptr)((~eʗ1).tag)) {
                    Ꮡb.Value.pbLabel(tagSample_Label, k, v, 0);
                }
            };
        }
        locs = Ꮡb.appendLocsForStack(locs[..0], (~e).stk);
        b.pbSample(values, locs, labels);
    }
    foreach (var (i, m) in b.mem) {
        var hasFunctions = m.funcs == lookupTried; // lookupTried but not lookupFailed
        b.pbMapping(tagProfile_Mapping, (uint64)(i + 1), (uint64)m.start, (uint64)m.end, m.offset, m.@file, m.buildID, hasFunctions);
    }
    // TODO: Anything for tagProfile_DropFrames?
    // TODO: Anything for tagProfile_KeepFrames?
    b.pb.strings(tagProfile_StringTable, b.strings);
    b.zw.Write(b.pb.data);
    b.zw.Close();
}

// appendLocsForStack appends the location IDs for the given stack trace to the given
// location ID slice, locs. The addresses in the stack are return PCs or 1 + the PC of
// an inline marker as the runtime traceback function returns.
//
// It may return an empty slice even if locs is non-empty, for example if locs consists
// solely of runtime.goexit. We still count these empty stacks in profiles in order to
// get the right cumulative sample count.
//
// It may emit to b.pb, so there must be no message encoding in progress.
internal static slice<uint64> /*newLocs*/ appendLocsForStack(this ж<profileBuilder> Ꮡb, slice<uint64> locs, slice<uintptr> stk) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.deck.reset();
    // The last frame might be truncated. Recover lost inline frames.
    stk = runtime_expandFinalInlineFrame(stk);
    while (len(stk) > 0) {
        var addr = stk[0];
        {
            var (l, ok) = b.locs[addr, ꟷ]; if (ok) {
                // When generating code for an inlined function, the compiler adds
                // NOP instructions to the outermost function as a placeholder for
                // each layer of inlining. When the runtime generates tracebacks for
                // stacks that include inlined functions, it uses the addresses of
                // those NOPs as "fake" PCs on the stack as if they were regular
                // function call sites. But if a profiling signal arrives while the
                // CPU is executing one of those NOPs, its PC will show up as a leaf
                // in the profile with its own Location entry. So, always check
                // whether addr is a "fake" PC in the context of the current call
                // stack by trying to add it to the inlining deck before assuming
                // that the deck is complete.
                if (len(b.deck.pcs) > 0) {
                    {
                        var added = b.deck.tryAdd(addr, l.firstPCFrames, l.firstPCSymbolizeResult); if (added) {
                            stk = stk[1..];
                            continue;
                        }
                    }
                }
                // first record the location if there is any pending accumulated info.
                {
                    var id = Ꮡb.emitLocation(); if (id > 0) {
                        locs = append(locs, id);
                    }
                }
                // then, record the cached location.
                locs = append(locs, l.id);
                // Skip the matching pcs.
                //
                // Even if stk was truncated due to the stack depth
                // limit, expandFinalInlineFrame above has already
                // fixed the truncation, ensuring it is long enough.
                stk = stk[(int)(len(l.pcs))..];
                continue;
            }
        }
        var (frames, symbolizeResult) = allFrames(addr);
        if (len(frames) == 0) {
            // runtime.goexit.
            {
                var id = Ꮡb.emitLocation(); if (id > 0) {
                    locs = append(locs, id);
                }
            }
            stk = stk[1..];
            continue;
        }
        {
            var added = b.deck.tryAdd(addr, frames, symbolizeResult); if (added) {
                stk = stk[1..];
                continue;
            }
        }
        // add failed because this addr is not inlined with the
        // existing PCs in the deck. Flush the deck and retry handling
        // this pc.
        {
            var id = Ꮡb.emitLocation(); if (id > 0) {
                locs = append(locs, id);
            }
        }
        // check cache again - previous emitLocation added a new entry
        {
            var (l, ok) = b.locs[addr, ꟷ]; if (ok){
                locs = append(locs, l.id);
                stk = stk[(int)(len(l.pcs))..]; // skip the matching pcs.
            } else {
                b.deck.tryAdd(addr, frames, symbolizeResult); // must succeed.
                stk = stk[1..];
            }
        }
    }
    {
        var id = Ꮡb.emitLocation(); if (id > 0) {
            // emit remaining location.
            locs = append(locs, id);
        }
    }
    return locs;
}

// Here's an example of how Go 1.17 writes out inlined functions, compiled for
// linux/amd64. The disassembly of main.main shows two levels of inlining: main
// calls b, b calls a, a does some work.
//
//   inline.go:9   0x4553ec  90              NOPL                 // func main()    { b(v) }
//   inline.go:6   0x4553ed  90              NOPL                 // func b(v *int) { a(v) }
//   inline.go:5   0x4553ee  48c7002a000000  MOVQ $0x2a, 0(AX)    // func a(v *int) { *v = 42 }
//
// If a profiling signal arrives while executing the MOVQ at 0x4553ee (for line
// 5), the runtime will report the stack as the MOVQ frame being called by the
// NOPL at 0x4553ed (for line 6) being called by the NOPL at 0x4553ec (for line
// 9).
//
// The role of pcDeck is to collapse those three frames back into a single
// location at 0x4553ee, with file/line/function symbolization info representing
// the three layers of calls. It does that via sequential calls to pcDeck.tryAdd
// starting with the leaf-most address. The fourth call to pcDeck.tryAdd will be
// for the caller of main.main. Because main.main was not inlined in its caller,
// the deck will reject the addition, and the fourth PC on the stack will get
// its own location.

// pcDeck is a helper to detect a sequence of inlined functions from
// a stack trace returned by the runtime.
//
// The stack traces returned by runtime's trackback functions are fully
// expanded (at least for Go functions) and include the fake pcs representing
// inlined functions. The profile proto expects the inlined functions to be
// encoded in one Location message.
// https://github.com/google/pprof/blob/5e965273ee43930341d897407202dd5e10e952cb/proto/profile.proto#L177-L184
//
// Runtime does not directly expose whether a frame is for an inlined function
// and looking up debug info is not ideal, so we use a heuristic to filter
// the fake pcs and restore the inlined and entry functions. Inlined functions
// have the following properties:
//
//	Frame's Func is nil (note: also true for non-Go functions), and
//	Frame's Entry matches its entry function frame's Entry (note: could also be true for recursive calls and non-Go functions), and
//	Frame's Name does not match its entry function frame's name (note: inlined functions cannot be directly recursive).
//
// As reading and processing the pcs in a stack trace one by one (from leaf to the root),
// we use pcDeck to temporarily hold the observed pcs and their expanded frames
// until we observe the entry function frame.
[GoType] partial struct pcDeck {
    internal slice<uintptr> pcs;
    internal slice<runtime.Frame> frames;
    internal symbolizeFlag symbolizeResult;
    // firstPCFrames indicates the number of frames associated with the first
    // (leaf-most) PC in the deck
    internal nint firstPCFrames;
    // firstPCSymbolizeResult holds the results of the allFrames call for the
    // first (leaf-most) PC in the deck
    internal symbolizeFlag firstPCSymbolizeResult;
}

[GoRecv] internal static void reset(this ref pcDeck d) {
    d.pcs = d.pcs[..0];
    d.frames = d.frames[..0];
    d.symbolizeResult = 0;
    d.firstPCFrames = 0;
    d.firstPCSymbolizeResult = 0;
}

// tryAdd tries to add the pc and Frames expanded from it (most likely one,
// since the stack trace is already fully expanded) and the symbolizeResult
// to the deck. If it fails the caller needs to flush the deck and retry.
[GoRecv] internal static bool /*success*/ tryAdd(this ref pcDeck d, uintptr pc, slice<runtime.Frame> frames, symbolizeFlag symbolizeResult) {
    {
        nint existing = len(d.frames); if (existing > 0) {
            // 'd.frames' are all expanded from one 'pc' and represent all
            // inlined functions so we check only the last one.
            ref var newFrame = ref heap<runtime.Frame>(out var ᏑnewFrame);
            newFrame = frames[0];
            ref var last = ref heap<runtime.Frame>(out var Ꮡlast);
            last = d.frames[existing - 1];
            if (last.Func != nil) {
                // the last frame can't be inlined. Flush.
                return false;
            }
            if (last.Entry == 0 || newFrame.Entry == 0) {
                // Possibly not a Go function. Don't try to merge.
                return false;
            }
            if (last.Entry != newFrame.Entry) {
                // newFrame is for a different function.
                return false;
            }
            if (runtime_FrameSymbolName(Ꮡlast) == runtime_FrameSymbolName(ᏑnewFrame)) {
                // maybe recursion.
                return false;
            }
        }
    }
    d.pcs = append(d.pcs, pc);
    d.frames = appendꓸꓸꓸ(d.frames, frames);
    d.symbolizeResult |= (symbolizeFlag)(symbolizeResult);
    if (len(d.pcs) == 1) {
        d.firstPCFrames = len(d.frames);
        d.firstPCSymbolizeResult = symbolizeResult;
    }
    return true;
}

// We can't write out functions while in the middle of the
// Location message, so record new functions we encounter and
// write them out after the Location.
[GoType("dyn")] internal partial struct emitLocation_newFunc {
    internal uint64 id;
    internal @string name, @file;
    internal int64 startLine;
}

// emitLocation emits the new location and function information recorded in the deck
// and returns the location ID encoded in the profile protobuf.
// It emits to b.pb, so there must be no message encoding in progress.
// It resets the deck.
internal static uint64 emitLocation(this ж<profileBuilder> Ꮡb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        if (len(b.deck.pcs) == 0) {
            return 0;
        }
        ᒐd1 = true;
        var addr = b.deck.pcs[0];
        var firstFrame = b.deck.frames[0];
        var newFuncs = new slice<emitLocation_newFunc>(0, 8);
        var id = (uint64)len(b.locs) + 1;
        b.locs[addr] = new locInfo(
            id: id,
            pcs: appendꓸꓸꓸ(new uintptr[]{}.slice(), b.deck.pcs),
            firstPCSymbolizeResult: b.deck.firstPCSymbolizeResult,
            firstPCFrames: appendꓸꓸꓸ(new runtime.Frame[]{}.slice(), b.deck.frames[..(int)(b.deck.firstPCFrames)])
        );
        msgOffset start = b.pb.startMessage();
        b.pb.uint64Opt(tagLocation_ID, id);
        b.pb.uint64Opt(tagLocation_Address, (uint64)firstFrame.PC);
        foreach (var (_, vᴛ1) in b.deck.frames) {
            ref var frame = ref heap(new runtime.Frame(), out var Ꮡframe);
            frame = vᴛ1;

            // Write out each line in frame expansion.
            @string funcName = runtime_FrameSymbolName(Ꮡframe);
            var funcID = (uint64)b.funcs[funcName];
            if (funcID == 0) {
                funcID = (uint64)len(b.funcs) + 1;
                b.funcs[funcName] = (nint)funcID;
                newFuncs = append(newFuncs, new emitLocation_newFunc(
                    id: funcID,
                    name: funcName,
                    @file: frame.File,
                    startLine: (int64)runtime_FrameStartLine(Ꮡframe)
                ));
            }
            b.pbLine(tagLocation_Line, funcID, (int64)frame.Line);
        }
        foreach (var (i, _) in b.mem) {
            if (b.mem[i].start <= addr && addr < b.mem[i].end || b.mem[i].fake) {
                b.pb.uint64Opt(tagLocation_MappingID, (uint64)(i + 1));
                var m = b.mem[i];
                m.funcs |= (symbolizeFlag)(b.deck.symbolizeResult);
                b.mem[i] = m;
                break;
            }
        }
        b.pb.endMessage(tagProfile_Location, start);
        // Write out functions we found during frame expansion.
        foreach (var (_, fn) in newFuncs) {
            msgOffset startΔ1 = b.pb.startMessage();
            b.pb.uint64Opt(tagFunction_ID, fn.id);
            b.pb.int64Opt(tagFunction_Name, b.stringIndex(fn.name));
            b.pb.int64Opt(tagFunction_SystemName, b.stringIndex(fn.name));
            b.pb.int64Opt(tagFunction_Filename, b.stringIndex(fn.@file));
            b.pb.int64Opt(tagFunction_StartLine, fn.startLine);
            b.pb.endMessage(tagProfile_Function, startΔ1);
        }
        b.flush();
        return id;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡb.DerefOrNull().deck.reset(); ᒐ.Run(); }
}

internal static slice<byte> space = slice<byte>(" "u8);

internal static slice<byte> newline = slice<byte>("\n"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string deletedˢ = " (deleted)"u8;

internal static void parseProcSelfMaps(slice<byte> data, Action<uint64, uint64, uint64, @string, @string> addMapping) {
    // $ cat /proc/self/maps
    // 00400000-0040b000 r-xp 00000000 fc:01 787766                             /bin/cat
    // 0060a000-0060b000 r--p 0000a000 fc:01 787766                             /bin/cat
    // 0060b000-0060c000 rw-p 0000b000 fc:01 787766                             /bin/cat
    // 014ab000-014cc000 rw-p 00000000 00:00 0                                  [heap]
    // 7f7d76af8000-7f7d7797c000 r--p 00000000 fc:01 1318064                    /usr/lib/locale/locale-archive
    // 7f7d7797c000-7f7d77b36000 r-xp 00000000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77b36000-7f7d77d36000 ---p 001ba000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d36000-7f7d77d3a000 r--p 001ba000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d3a000-7f7d77d3c000 rw-p 001be000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d3c000-7f7d77d41000 rw-p 00000000 00:00 0
    // 7f7d77d41000-7f7d77d64000 r-xp 00000000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f3f000-7f7d77f42000 rw-p 00000000 00:00 0
    // 7f7d77f61000-7f7d77f63000 rw-p 00000000 00:00 0
    // 7f7d77f63000-7f7d77f64000 r--p 00022000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f64000-7f7d77f65000 rw-p 00023000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f65000-7f7d77f66000 rw-p 00000000 00:00 0
    // 7ffc342a2000-7ffc342c3000 rw-p 00000000 00:00 0                          [stack]
    // 7ffc34343000-7ffc34345000 r-xp 00000000 00:00 0                          [vdso]
    // ffffffffff600000-ffffffffff601000 r-xp 00000000 00:00 0                  [vsyscall]
    ref var line = ref heap<slice<byte>>(out var Ꮡline);
    // next removes and returns the next field in the line.
    // It also removes from line any spaces following the field.
    slice<byte> next() {
        slice<byte> f = default!;
        (f, Ꮡline.ValueSlot, _) = bytes.Cut(Ꮡline.ValueSlot, space);
        Ꮡline.ValueSlot = bytes.TrimLeft(Ꮡline.ValueSlot, " "u8);
        return f;
    }
    while (len(data) > 0) {
        (line, data, _) = bytes.Cut(data, newline);
        var addr = next();
        var (loStr, hiStr, ok) = strings_package.Cut(((@string)addr), "-"u8);
        if (!ok) {
            continue;
        }
        var (lo, err) = strconv.ParseUint(loStr, 16, 64);
        if (err != default!) {
            continue;
        }
        (var hi, err) = strconv.ParseUint(hiStr, 16, 64);
        if (err != default!) {
            continue;
        }
        var perm = next();
        if (len(perm) < 4 || perm[2] != (rune)'x') {
            // Only interested in executable mappings.
            continue;
        }
        (var offset, err) = strconv.ParseUint(((@string)next()), 16, 64);
        if (err != default!) {
            continue;
        }
        next(); // dev
        var inode = next(); // inode
        if (line == default!) {
            continue;
        }
        @string @file = ((@string)line);
        // Trim deleted file marker.
        @string deletedStr = deletedˢ;
        nint deletedLen = len(deletedStr);
        if (len(@file) >= deletedLen && @file[(int)(len(@file) - deletedLen)..] == deletedStr) {
            @file = @file[..(int)(len(@file) - deletedLen)];
        }
        if (len(inode) == 1 && inode[0] == (rune)'0' && @file == ""u8) {
            // Huge-page text mappings list the initial fragment of
            // mapped but unpopulated memory as being inode 0.
            // Don't report that part.
            // But [vdso] and [vsyscall] are inode 0, so let non-empty file names through.
            continue;
        }
        // TODO: pprof's remapMappingIDs makes one adjustment:
        // 1. If there is an /anon_hugepage mapping first and it is
        // consecutive to a next mapping, drop the /anon_hugepage.
        // There's no indication why this is needed.
        // Let's try not doing this and see what breaks.
        // If we do need it, it would go here, before we
        // enter the mappings into b.mem in the first place.
        var (buildID, _) = elfBuildID(@file);
        addMapping(lo, hi, offset, @file, buildID);
    }
}

[GoRecv] internal static void addMapping(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID) {
    b.addMappingEntry(lo, hi, offset, @file, buildID, false);
}

[GoRecv] internal static void addMappingEntry(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID, bool fake) {
    b.mem = append(b.mem, new memMap(
        start: (uintptr)lo,
        end: (uintptr)hi,
        offset: offset,
        @file: @file,
        buildID: buildID,
        fake: fake
    ));
}

} // end pprof_package
