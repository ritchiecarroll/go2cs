// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains data types that all implementations of the trace format
// parser need to provide to the rest of the package.
namespace go.@internal;

using fmt = fmt_package;
using math = math_package;
using strings = strings_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;
using go.@internal.trace.@event;
using io = io_package;

partial class trace_package {

// maxArgs is the maximum number of arguments for "plain" events,
// i.e. anything that could reasonably be represented as a baseEvent.
internal static UntypedInt maxArgs => 5;

/* [maxArgs - 1]uint64 */
partial struct timedEventArgs /*[4]uint64*/;

// baseEvent is the basic unprocessed event. This serves as a common
// fundamental data structure across.
partial struct baseEvent {
    internal @event.Type typ;
    internal ΔTime time;
    internal timedEventArgs args;
}

// extra returns a slice representing extra available space in args
// that the parser can use to pass data up into Event.
internal static slice<uint64> extra(this ref baseEvent e, version.Version v) {
    var exprᴛ1 = v;
    if (exprᴛ1 == version.Go122) {
        return e.args.slice(len(go122.Specs()[e.typ].Args) - 1);
    }

    throw panic(fmt.Sprintf("unsupported version: go 1.%d"u8, v));
}

// evTable contains the per-generation data necessary to
// interpret an individual event.
partial struct evTable {
    internal frequency freq;
    internal dataTable<stringID, @string> strings;
    internal dataTable<stackID, stack> stacks;
    internal map<uint64, frame> pcs;
    // extraStrings are strings that get generated during
    // parsing but haven't come directly from the trace, so
    // they don't appear in strings.
    internal slice<@string> extraStrings;
    internal map<@string, extraStringID> extraStringIDs;
    internal extraStringID nextExtra;
    // expData contains extra unparsed data that is accessible
    // only to ExperimentEvent via an EventExperimental event.
    internal map<@event.Experiment, ж<ExperimentalData>> expData;
}

// addExtraString adds an extra string to the evTable and returns
// a unique ID for the string in the table.
internal static extraStringID addExtraString(this ref evTable t, @string s) {
    if (s == ""u8) {
        return 0;
    }
    if (t.extraStringIDs == default!) {
        t.extraStringIDs = new map<@string, extraStringID>();
    }
    {
        var (idΔ1, ok) = t.extraStringIDs[s, ꟷ]; if (ok) {
            return idΔ1;
        }
    }
    t.nextExtra++;
    var id = t.nextExtra;
    t.extraStrings = append(t.extraStrings, s);
    t.extraStringIDs[s] = id;
    return id;
}

// getExtraString returns the extra string for the provided ID.
// The ID must have been produced by addExtraString for this evTable.
internal static @string getExtraString(this ref evTable t, extraStringID id) {
    if (id == 0) {
        return ""u8;
    }
    return t.extraStrings[id - 1];
}

// dataTable is a mapping from EIs to Es.
partial struct dataTable<EI, E>
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    internal slice<uint8> present;
    internal slice<E> dense;
    internal map<EI, E> sparse;
}

// insert tries to add a mapping from id to s.
//
// Returns an error if a mapping for id already exists, regardless
// of whether or not s is the same in content. This should be used
// for validation during parsing.
internal static error insert<EI, E>(this ref dataTable<EI, E> d, EI id, E data)
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    if (d.sparse == default!) {
        d.sparse = new map<EI, E>();
    }
    {
        var (existing, ok) = d.get(id); if (ok) {
            return fmt.Errorf("multiple %Ts with the same ID: id=%d, new=%v, existing=%v"u8, data, id, data, existing);
        }
    }
    d.sparse[id] = data;
    return default!;
}

// compactify attempts to compact sparse into dense.
//
// This is intended to be called only once after insertions are done.
internal static void compactify<EI, E>(this ref dataTable<EI, E> d)
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    if (d.sparse == default! || len(d.dense) != 0) {
        // Already compactified.
        return;
    }
    // Find the range of IDs.
    var maxID = ConvertToType<EI>(0);
    var minID = ~ConvertToType<EI>(0);
    foreach (var (id, _) in d.sparse) {
        if (id > maxID) {
            maxID = id;
        }
        if (id < minID) {
            minID = id;
        }
    }
    if (maxID >= ConvertToType<EI>(math.MaxInt)) {
        // We can't create a slice big enough to hold maxID elements
        return;
    }
    // We're willing to waste at most 2x memory.
    if ((nint)ConvertToUInt64<EI>(maxID - minID) > max(len(d.sparse), 2 * len(d.sparse))) {
        return;
    }
    if ((nint)ConvertToUInt64<EI>(minID) > len(d.sparse)) {
        return;
    }
    nint size = (nint)ConvertToUInt64<EI>(maxID) + 1;
    d.present = new slice<uint8>((size + 7) / 8);
    d.dense = new slice<E>(size);
    foreach (var (id, data) in d.sparse) {
        d.dense[(nint)(ConvertToUInt64<EI>(id))] = data;
        d.present[(nint)(ConvertToUInt64<EI>(id / ConvertToType<EI>(8)))] |= (uint8)((uint8)((uint8)1 << (int)(ConvertToUInt64<EI>((id % ConvertToType<EI>(8))))));
    }
    d.sparse = default!;
}

// get returns the E for id or false if it doesn't
// exist. This should be used for validation during parsing.
internal static (E, bool) get<EI, E>(this ref dataTable<EI, E> d, EI id)
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    if (AreEqual(id, ConvertToType<EI>(0))) {
        return (@new<E>().ValueSlot, true);
    }
    if (ConvertToUInt64<EI>(id) < (uint64)len(d.dense)){
        if ((uint8)(d.present[(nint)(ConvertToUInt64<EI>(id / ConvertToType<EI>(8)))] & ((uint8)((uint8)1 << (int)(ConvertToUInt64<EI>((id % ConvertToType<EI>(8))))))) != 0) {
            return (d.dense[(nint)(ConvertToUInt64<EI>(id))], true);
        }
    } else 
    if (d.sparse != default!) {
        {
            var (data, ok) = d.sparse[id, ꟷ]; if (ok) {
                return (data, true);
            }
        }
    }
    return (@new<E>().ValueSlot, false);
}

// forEach iterates over all ID/value pairs in the data table.
internal static bool forEach<EI, E>(this ref dataTable<EI, E> d, Func<EI, E, bool> yield)
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    foreach (var (id, value) in d.dense) {
        if ((uint8)(d.present[id / 8] & (((uint8)1).Lsh((int64)((id % 8))))) == 0) {
            continue;
        }
        if (!yield(ConvertToType<EI>(id), value)) {
            return false;
        }
    }
    if (d.sparse == default!) {
        return true;
    }
    foreach (var (id, value) in d.sparse) {
        if (!yield(id, value)) {
            return false;
        }
    }
    return true;
}

// mustGet returns the E for id or panics if it fails.
//
// This should only be used if id has already been validated.
internal static E mustGet<EI, E>(this ref dataTable<EI, E> d, EI id)
    where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new()
{
    var (data, ok) = d.get(id);
    if (!ok) {
        throw panic(fmt.Sprintf("expected id %d in %T table"u8, id, data));
    }
    return data;
}

partial struct frequency /*num:float64*/;

// mul multiplies an unprocessed to produce a time in nanoseconds.
internal static ΔTime mul(this frequency f, timestamp t) {
    return ((ΔTime)(int64)((float64)(uint64)t * (float64)f));
}

partial struct stringID /*num:uint64*/;

partial struct extraStringID /*num:uint64*/;

partial struct stackID /*num:uint64*/;

// cpuSample represents a CPU profiling sample captured by the trace.
partial struct cpuSample {
    internal partial ref schedCtx schedCtx { get; }
    internal ΔTime time;
    internal stackID stack;
}

// asEvent produces a complete Event from a cpuSample. It needs
// the evTable from the generation that created it.
//
// We don't just store it as an Event in generation to minimize
// the amount of pointer data floating around.
internal static ΔEvent asEvent(this cpuSample s, ж<evTable> Ꮡtable) {
    // TODO(mknyszek): This is go122-specific, but shouldn't be.
    // Generalize this in the future.
    var e = new ΔEvent(
        table: Ꮡtable,
        ctx: s.schedCtx,
        @base: new baseEvent(
            typ: go122.EvCPUSample,
            time: s.time
        )
    );
    e.@base.args[0] = (uint64)s.stack;
    return e.ΔClone();
}

// stack represents a goroutine stack sample.
partial struct stack {
    internal slice<uint64> pcs;
}

internal static @string String(this stack s) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (_, frame) in s.pcs) {
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "\t%#v\n"u8, frame);
    }
    return sb.String();
}

// frame represents a single stack frame.
partial struct frame {
    internal uint64 pc;
    internal stringID funcID;
    internal stringID fileID;
    internal uint64 line;
}

} // end trace_package
