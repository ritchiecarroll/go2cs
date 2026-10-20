// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;

partial class profile_package {

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Profile p) {
    return profileDecoder;
}

// preEncode populates the unexported fields to be used by encode
// (with suffix X) from the corresponding exported fields. The
// exported fields are cleared up to facilitate testing.
internal static void preEncode(this ref Profile p) {
    var keysᴷ = new stackAppend<@string>();
    var numKeysᴷ = new stackAppend<@string>();

    var strings = new map<@string, nint>();
    addString(strings, ""u8);

    foreach (var (_, st) in p.SampleType) {
        st.Value.typeX = addString(strings, (~st).Type);
        st.Value.unitX = addString(strings, (~st).Unit);
    }

    foreach (var (_, s) in p.Sample) {
        s.Value.labelX = default!;
        slice<@string> keys = default!;
        foreach (var (k, _) in (~s).Label) {
            keys = append(keysᴷ, keys, k);
        }
        sort.Strings(keys);
        foreach (var (_, k) in keys) {
            var vs = (~s).Label[k];
            foreach (var (_, v) in vs) {
                s.Value.labelX = append((~s).labelX,
                    new Label(
                        keyX: addString(strings, k),
                        strX: addString(strings, v)
                    ));
            }
        }
        slice<@string> numKeys = default!;
        foreach (var (k, _) in (~s).NumLabel) {
            numKeys = append(numKeysᴷ, numKeys, k);
        }
        sort.Strings(numKeys);
        foreach (var (_, k) in numKeys) {
            var vs = (~s).NumLabel[k];
            foreach (var (_, v) in vs) {
                s.Value.labelX = append((~s).labelX,
                    new Label(
                        keyX: addString(strings, k),
                        numX: v
                    ));
            }
        }
        s.Value.locationIDX = default!;
        foreach (var (_, l) in (~s).Location) {
            s.Value.locationIDX = append((~s).locationIDX, (~l).ID);
        }
    }

    foreach (var (_, m) in p.Mapping) {
        m.Value.fileX = addString(strings, (~m).File);
        m.Value.buildIDX = addString(strings, (~m).BuildID);
    }

    foreach (var (_, l) in p.Location) {
        foreach (var (i, ln) in (~l).Line) {
            if (ln.Function != nil){
                (~l).Line[i].functionIDX = ln.Function.Value.ID;
            } else {
                (~l).Line[i].functionIDX = 0;
            }
        }
        if ((~l).Mapping != nil){
            l.Value.mappingIDX = l.Value.Mapping.Value.ID;
        } else {
            l.Value.mappingIDX = 0;
        }
    }
    foreach (var (_, f) in p.Function) {
        f.Value.nameX = addString(strings, (~f).Name);
        f.Value.systemNameX = addString(strings, (~f).SystemName);
        f.Value.filenameX = addString(strings, (~f).Filename);
    }

    p.dropFramesX = addString(strings, p.DropFrames);
    p.keepFramesX = addString(strings, p.KeepFrames);

    {
        var pt = p.PeriodType; if (pt != nil) {
            pt.Value.typeX = addString(strings, (~pt).Type);
            pt.Value.unitX = addString(strings, (~pt).Unit);
        }
    }

    p.stringTable = new slice<@string>(len(strings));
    foreach (var (s, i) in strings) {
        p.stringTable[i] = s;
    }
}

internal static void encode(this ref Profile p, ж<buffer> Ꮡb) {
    foreach (var (_, x) in p.SampleType) {
        encodeMessage(Ꮡb, 1, new ValueTypeжmessage(x));
    }
    foreach (var (_, x) in p.Sample) {
        encodeMessage(Ꮡb, 2, new Sampleжmessage(x));
    }
    foreach (var (_, x) in p.Mapping) {
        encodeMessage(Ꮡb, 3, new Mappingжmessage(x));
    }
    foreach (var (_, x) in p.Location) {
        encodeMessage(Ꮡb, 4, new Locationжmessage(x));
    }
    foreach (var (_, x) in p.Function) {
        encodeMessage(Ꮡb, 5, new Functionжmessage(x));
    }
    encodeStrings(ref (Ꮡb).DerefOrNull(), 6, p.stringTable);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 7, p.dropFramesX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 8, p.keepFramesX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 9, p.TimeNanos);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 10, p.DurationNanos);
    {
        var pt = p.PeriodType; if (pt != nil && ((~pt).typeX != 0 || (~pt).unitX != 0)) {
            encodeMessage(Ꮡb, 11, new ValueTypeжmessage(p.PeriodType));
        }
    }
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 12, p.Period);
}

internal static slice<Func<ж<buffer>, message, error>> profileDecoder;
internal static void initᴛprofileDecoder() { profileDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // repeated ValueType sample_type = 1
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.SampleType = append((~pp).SampleType, x);
        return decodeMessage(b, new ValueTypeжmessage(x));
    },
    // repeated Sample sample = 2
    (ж<buffer> b, message m) => {
        var x = @new<Sample>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.Sample = append((~pp).Sample, x);
        return decodeMessage(b, new Sampleжmessage(x));
    },
    // repeated Mapping mapping = 3
    (ж<buffer> b, message m) => {
        var x = @new<Mapping>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.Mapping = append((~pp).Mapping, x);
        return decodeMessage(b, new Mappingжmessage(x));
    },
    // repeated Location location = 4
    (ж<buffer> b, message m) => {
        var x = @new<Location>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.Location = append((~pp).Location, x);
        return decodeMessage(b, new Locationжmessage(x));
    },
    // repeated Function function = 5
    (ж<buffer> b, message m) => {
        var x = @new<Function>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.Function = append((~pp).Function, x);
        return decodeMessage(b, new Functionжmessage(x));
    },
    // repeated string string_table = 6
    error (ж<buffer> b, message m) => {
        var err = decodeStrings(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑstringTable));
        if (err != default!) {
            return err;
        }
        if ((~m._<message, ж<Profile>>()).stringTable[0] != "") {
            return errors.New("string_table[0] must be ''"u8);
        }
        return default!;
    },
    // repeated int64 drop_frames = 7
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑdropFramesX)),
    // repeated int64 keep_frames = 8
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑkeepFramesX)),
    // repeated int64 time_nanos = 9
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑTimeNanos)),
    // repeated int64 duration_nanos = 10
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑDurationNanos)),
    // optional string period_type = 11
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<message, ж<Profile>>();
        pp.Value.PeriodType = x;
        return decodeMessage(b, new ValueTypeжmessage(x));
    },
    // repeated int64 period = 12
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑPeriod)),
    // repeated int64 comment = 13
    (ж<buffer> b, message m) => decodeInt64s(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑcommentX)),
    // int64 defaultSampleType = 14
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Profile>>().of(Profile.ᏑdefaultSampleTypeX))
}.slice(); }

// postDecode takes the unexported fields populated by decode (with
// suffix X) and populates the corresponding exported fields.
// The unexported fields are cleared up to facilitate testing.
internal static error postDecode(this ж<Profile> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    error err = default!;

    var mappings = new map<uint64, ж<Mapping>>();
    foreach (var (_, m) in p.Mapping) {
        (m.Value.File, err) = getString(p.stringTable, ref (m.of(Mapping.ᏑfileX)).DerefOrNull(), err);
        (m.Value.BuildID, err) = getString(p.stringTable, ref (m.of(Mapping.ᏑbuildIDX)).DerefOrNull(), err);
        mappings[(~m).ID] = m;
    }

    var functions = new map<uint64, ж<Function>>();
    foreach (var (_, f) in p.Function) {
        (f.Value.Name, err) = getString(p.stringTable, ref (f.of(Function.ᏑnameX)).DerefOrNull(), err);
        (f.Value.SystemName, err) = getString(p.stringTable, ref (f.of(Function.ᏑsystemNameX)).DerefOrNull(), err);
        (f.Value.Filename, err) = getString(p.stringTable, ref (f.of(Function.ᏑfilenameX)).DerefOrNull(), err);
        functions[(~f).ID] = f;
    }

    var locations = new map<uint64, ж<Location>>();
    foreach (var (_, l) in p.Location) {
        l.Value.Mapping = mappings[(~l).mappingIDX];
        l.Value.mappingIDX = 0;
        foreach (var (i, ln) in (~l).Line) {
            {
                var id = ln.functionIDX; if (id != 0) {
                    (~l).Line[i].Function = functions[id];
                    if ((~l).Line[i].Function == nil) {
                        return fmt.Errorf("Function ID %d not found"u8, id);
                    }
                    (~l).Line[i].functionIDX = 0;
                }
            }
        }
        locations[(~l).ID] = l;
    }

    foreach (var (_, st) in p.SampleType) {
        (st.Value.Type, err) = getString(p.stringTable, ref (st.of(ValueType.ᏑtypeX)).DerefOrNull(), err);
        (st.Value.Unit, err) = getString(p.stringTable, ref (st.of(ValueType.ᏑunitX)).DerefOrNull(), err);
    }

    foreach (var (_, s) in p.Sample) {
        var labels = new map<@string, slice<@string>>();
        var numLabels = new map<@string, slice<int64>>();
        foreach (var (_, vᴛ1) in (~s).labelX) {
            var l = vᴛ1;

            @string key = default!;
            @string value = default!;
            (key, err) = getString(p.stringTable, ref l.keyX, err);
            if (l.strX != 0){
                (value, err) = getString(p.stringTable, ref l.strX, err);
                labels[key] = append(labels[key], value);
            } else {
                numLabels[key] = append(numLabels[key], l.numX);
            }
        }
        if (len(labels) > 0) {
            s.Value.Label = labels;
        }
        if (len(numLabels) > 0) {
            s.Value.NumLabel = numLabels;
        }
        s.Value.Location = default!;
        foreach (var (_, lid) in (~s).locationIDX) {
            s.Value.Location = append((~s).Location, locations[lid]);
        }
        s.Value.locationIDX = default!;
    }

    (p.DropFrames, err) = getString(p.stringTable, ref nonnil(ref p).dropFramesX, err);
    (p.KeepFrames, err) = getString(p.stringTable, ref nonnil(ref p).keepFramesX, err);

    {
        var pt = p.PeriodType; if (pt == nil) {
            p.PeriodType = Ꮡ(new ValueType(nil));
        }
    }

    {
        var pt = p.PeriodType; if (pt != nil) {
            (pt.Value.Type, err) = getString(p.stringTable, ref (pt.of(ValueType.ᏑtypeX)).DerefOrNull(), err);
            (pt.Value.Unit, err) = getString(p.stringTable, ref (pt.of(ValueType.ᏑunitX)).DerefOrNull(), err);
        }
    }
    foreach (var (_, vᴛ2) in p.commentX) {
        var i = vᴛ2;

        @string c = default!;
        (c, err) = getString(p.stringTable, ref i, err);
        p.Comments = append(p.Comments, c);
    }

    p.commentX = default!;
    (p.DefaultSampleType, err) = getString(p.stringTable, ref nonnil(ref p).defaultSampleTypeX, err);
    p.stringTable = default!;
    return err;
}

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref ValueType p) {
    return valueTypeDecoder;
}

internal static void encode(this ref ValueType p, ж<buffer> Ꮡb) {
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 1, p.typeX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 2, p.unitX);
}

internal static slice<Func<ж<buffer>, message, error>> valueTypeDecoder;
internal static void initᴛvalueTypeDecoder() { valueTypeDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // optional int64 type = 1
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<ValueType>>().of(ValueType.ᏑtypeX)),
    // optional int64 unit = 2
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<ValueType>>().of(ValueType.ᏑunitX))
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Sample p) {
    return sampleDecoder;
}

internal static void encode(this ref Sample p, ж<buffer> Ꮡb) {
    encodeUint64s(ref (Ꮡb).DerefOrNull(), 1, p.locationIDX);
    foreach (var (_, x) in p.Value) {
        encodeInt64(ref (Ꮡb).DerefOrNull(), 2, x);
    }
    foreach (var (_, x) in p.labelX) {
        encodeMessage(Ꮡb, 3, x);
    }
}

internal static slice<Func<ж<buffer>, message, error>> sampleDecoder;
internal static void initᴛsampleDecoder() { sampleDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // repeated uint64 location = 1
    (ж<buffer> b, message m) => decodeUint64s(ref (b).DerefOrNull(), m._<message, ж<Sample>>().of(Sample.ᏑlocationIDX)),
    // repeated int64 value = 2
    (ж<buffer> b, message m) => decodeInt64s(ref (b).DerefOrNull(), m._<message, ж<Sample>>().of(Sample.ᏑValue)),
    // repeated Label label = 3
    (ж<buffer> b, message m) => {
        var s = m._<message, ж<Sample>>();
        nint n = len((~s).labelX);
        s.Value.labelX = append((~s).labelX, new Label(nil));
        return decodeMessage(b, new Labelжmessage(Ꮡ((~s).labelX, n)));
    }
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this Label p) {
    return labelDecoder;
}

internal static void encode(this Label p, ж<buffer> Ꮡb) {
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 1, p.keyX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 2, p.strX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 3, p.numX);
}

internal static slice<Func<ж<buffer>, message, error>> labelDecoder;
internal static void initᴛlabelDecoder() { labelDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // optional int64 key = 1
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Label>>().of(Label.ᏑkeyX)),
    // optional int64 str = 2
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Label>>().of(Label.ᏑstrX)),
    // optional int64 num = 3
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Label>>().of(Label.ᏑnumX))
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Mapping p) {
    return mappingDecoder;
}

internal static void encode(this ref Mapping p, ж<buffer> Ꮡb) {
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 1, p.ID);
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 2, p.Start);
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 3, p.Limit);
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 4, p.Offset);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 5, p.fileX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 6, p.buildIDX);
    encodeBoolOpt(ref (Ꮡb).DerefOrNull(), 7, p.HasFunctions);
    encodeBoolOpt(ref (Ꮡb).DerefOrNull(), 8, p.HasFilenames);
    encodeBoolOpt(ref (Ꮡb).DerefOrNull(), 9, p.HasLineNumbers);
    encodeBoolOpt(ref (Ꮡb).DerefOrNull(), 10, p.HasInlineFrames);
}

internal static slice<Func<ж<buffer>, message, error>> mappingDecoder;
internal static void initᴛmappingDecoder() { mappingDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑID)), // optional uint64 id = 1
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑStart)), // optional uint64 memory_offset = 2
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑLimit)), // optional uint64 memory_limit = 3
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑOffset)), // optional uint64 file_offset = 4
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑfileX)), // optional int64 filename = 5
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑbuildIDX)), // optional int64 build_id = 6
    (ж<buffer> b, message m) => decodeBool(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑHasFunctions)), // optional bool has_functions = 7
    (ж<buffer> b, message m) => decodeBool(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑHasFilenames)), // optional bool has_filenames = 8
    (ж<buffer> b, message m) => decodeBool(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑHasLineNumbers)), // optional bool has_line_numbers = 9
    (ж<buffer> b, message m) => decodeBool(ref (b).DerefOrNull(), m._<message, ж<Mapping>>().of(Mapping.ᏑHasInlineFrames)) // optional bool has_inline_frames = 10
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Location p) {
    return locationDecoder;
}

internal static void encode(this ref Location p, ж<buffer> Ꮡb) {
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 1, p.ID);
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 2, p.mappingIDX);
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 3, p.Address);
    foreach (var (i, _) in p.Line) {
        encodeMessage(Ꮡb, 4, new Lineжmessage(Ꮡ(p.Line, i)));
    }
}

internal static slice<Func<ж<buffer>, message, error>> locationDecoder;
internal static void initᴛlocationDecoder() { locationDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Location>>().of(Location.ᏑID)), // optional uint64 id = 1;
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Location>>().of(Location.ᏑmappingIDX)), // optional uint64 mapping_id = 2;
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Location>>().of(Location.ᏑAddress)), // optional uint64 address = 3;
    (ж<buffer> b, message m) => { // repeated Line line = 4
        var pp = m._<message, ж<Location>>();
        nint n = len((~pp).Line);
        pp.Value.Line = append((~pp).Line, new Line(nil));
        return decodeMessage(b, new Lineжmessage(Ꮡ((~pp).Line, n)));
    }
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Line p) {
    return lineDecoder;
}

internal static void encode(this ref Line p, ж<buffer> Ꮡb) {
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 1, p.functionIDX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 2, p.ΔLine);
}

internal static slice<Func<ж<buffer>, message, error>> lineDecoder;
internal static void initᴛlineDecoder() { lineDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // optional uint64 function_id = 1
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Line>>().of(Line.ᏑfunctionIDX)),
    // optional int64 line = 2
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Line>>().of(Line.ᏑΔLine))
}.slice(); }

internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Function p) {
    return functionDecoder;
}

internal static void encode(this ref Function p, ж<buffer> Ꮡb) {
    encodeUint64Opt(ref (Ꮡb).DerefOrNull(), 1, p.ID);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 2, p.nameX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 3, p.systemNameX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 4, p.filenameX);
    encodeInt64Opt(ref (Ꮡb).DerefOrNull(), 5, p.StartLine);
}

internal static slice<Func<ж<buffer>, message, error>> functionDecoder;
internal static void initᴛfunctionDecoder() { functionDecoder = new Func<ж<buffer>, message, error>[]{
    default!, // 0
    // optional uint64 id = 1
    (ж<buffer> b, message m) => decodeUint64(ref (b).DerefOrNull(), m._<message, ж<Function>>().of(Function.ᏑID)),
    // optional int64 function_name = 2
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Function>>().of(Function.ᏑnameX)),
    // optional int64 function_system_name = 3
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Function>>().of(Function.ᏑsystemNameX)),
    // repeated int64 filename = 4
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Function>>().of(Function.ᏑfilenameX)),
    // optional int64 start_line = 5
    (ж<buffer> b, message m) => decodeInt64(ref (b).DerefOrNull(), m._<message, ж<Function>>().of(Function.ᏑStartLine))
}.slice(); }

internal static int64 addString(map<@string, nint> strings, @string s) {
    var (i, ok) = strings[s, ꟷ];
    if (!ok) {
        i = len(strings);
        strings[s] = i;
    }
    return (int64)i;
}

internal static (@string, error) getString(slice<@string> strings, ref int64 strng, error err) {
    if (err != default!) {
        return ("", err);
    }
    nint s = (nint)(strng);
    if (s < 0 || s >= len(strings)) {
        return ("", errMalformed);
    }
    strng = 0;
    return (strings[s], default!);
}

} // end profile_package
