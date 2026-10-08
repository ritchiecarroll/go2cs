// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;

partial class profile_package {

// Merge merges all the profiles in profs into a single Profile.
// Returns a new profile independent of the input profiles. The merged
// profile is compacted to eliminate unused samples, locations,
// functions and mappings. Profiles must have identical profile sample
// and period types or the merge will fail. profile.Period of the
// resulting profile will be the maximum of all profiles, and
// profile.TimeNanos will be the earliest nonzero one.
public static (ж<Profile>, error) Merge(slice<ж<Profile>> srcs) {
    if (len(srcs) == 0) {
        return (default!, fmt.Errorf("no profiles to merge"u8));
    }
    var (p, err) = combineHeaders(srcs);
    if (err != default!) {
        return (default!, err);
    }
    var pm = Ꮡ(new profileMerger(
        p: p,
        samples: new map<sampleKey, ж<Sample>>(len((~srcs[0]).Sample)),
        locations: new map<locationKey, ж<Location>>(len((~srcs[0]).Location)),
        functions: new map<functionKey, ж<Function>>(len((~srcs[0]).Function)),
        mappings: new map<mappingKey, ж<Mapping>>(len((~srcs[0]).Mapping))
    ));
    foreach (var (_, src) in srcs) {
        // Clear the profile-specific hash tables
        pm.Value.locationsByID = new map<uint64, ж<Location>>(len((~src).Location));
        pm.Value.functionsByID = new map<uint64, ж<Function>>(len((~src).Function));
        pm.Value.mappingsByID = new map<uint64, mapInfo>(len((~src).Mapping));
        if (len((~pm).mappings) == 0 && len((~src).Mapping) > 0) {
            // The Mapping list has the property that the first mapping
            // represents the main binary. Take the first Mapping we see,
            // otherwise the operations below will add mappings in an
            // arbitrary order.
            pm.mapMapping((~src).Mapping[0]);
        }
        foreach (var (_, s) in (~src).Sample) {
            if (!isZeroSample(ref (s).DerefOrNull())) {
                pm.mapSample(s);
            }
        }
    }
    foreach (var (_, s) in (~p).Sample) {
        if (isZeroSample(ref (s).DerefOrNull())) {
            // If there are any zero samples, re-merge the profile to GC
            // them.
            return Merge(new ж<Profile>[]{p}.slice());
        }
    }
    return (p, default!);
}

// Normalize normalizes the source profile by multiplying each value in profile by the
// ratio of the sum of the base profile's values of that sample type to the sum of the
// source profile's value of that sample type.
public static error Normalize(this ref Profile p, ж<Profile> Ꮡpb) {
    ref var pb = ref Ꮡpb.DerefOrNull();

    {
        var err = p.compatible(Ꮡpb); if (err != default!) {
            return err;
        }
    }
    var baseVals = new slice<int64>(len(p.SampleType));
    foreach (var (_, s) in pb.Sample) {
        foreach (var (i, v) in (~s).Value) {
            baseVals[i] += v;
        }
    }
    var srcVals = new slice<int64>(len(p.SampleType));
    foreach (var (_, s) in p.Sample) {
        foreach (var (i, v) in (~s).Value) {
            srcVals[i] += v;
        }
    }
    var normScale = new slice<float64>(len(baseVals));
    foreach (var (i, _) in baseVals) {
        if (srcVals[i] == 0){
            normScale[i] = 0.0D;
        } else {
            normScale[i] = (float64)baseVals[i] / (float64)srcVals[i];
        }
    }
    p.ScaleN(normScale);
    return default!;
}

internal static bool isZeroSample(ref Sample s) {
    foreach (var (_, v) in s.Value) {
        if (v != 0) {
            return false;
        }
    }
    return true;
}

partial struct profileMerger {
    internal ж<Profile> p;
    // Memoization tables within a profile.
    internal map<uint64, ж<Location>> locationsByID;
    internal map<uint64, ж<Function>> functionsByID;
    internal map<uint64, mapInfo> mappingsByID;
    // Memoization tables for profile entities.
    internal map<sampleKey, ж<Sample>> samples;
    internal map<locationKey, ж<Location>> locations;
    internal map<functionKey, ж<Function>> functions;
    internal map<mappingKey, ж<Mapping>> mappings;
}

partial struct mapInfo {
    internal ж<Mapping> m;
    internal int64 offset;
}

internal static ж<Sample> mapSample(this ref profileMerger pm, ж<Sample> Ꮡsrc) {
    ref var src = ref Ꮡsrc.DerefOrNull();

    var s = Ꮡ(new Sample(
        Location: new slice<ж<Location>>(len(src.Location)),
        Value: new slice<int64>(len(src.Value)),
        Label: new map<@string, slice<@string>>(len(src.Label)),
        NumLabel: new map<@string, slice<int64>>(len(src.NumLabel)),
        NumUnit: new map<@string, slice<@string>>(len(src.NumLabel))
    ));
    foreach (var (i, l) in src.Location) {
        s.Value.Location[i] = pm.mapLocation(l);
    }
    foreach (var (kΔ1, v) in src.Label) {
        var vv = new slice<@string>(len(v));
        copy(vv, v);
        s.Value.Label[kΔ1] = vv;
    }
    foreach (var (kΔ2, v) in src.NumLabel) {
        var u = src.NumUnit[kΔ2];
        var vv = new slice<int64>(len(v));
        var uu = new slice<@string>(len(u));
        copy(vv, v);
        copy(uu, u);
        s.Value.NumLabel[kΔ2] = vv;
        s.Value.NumUnit[kΔ2] = uu;
    }
    // Check memoization table. Must be done on the remapped location to
    // account for the remapped mapping. Add current values to the
    // existing sample.
    var k = s.key();
    {
        var (ss, ok) = pm.samples[k, ꟷ]; if (ok) {
            foreach (var (i, v) in src.Value) {
                ss.Value.Value[i] += v;
            }
            return ss;
        }
    }
    copy((~s).Value, src.Value);
    pm.samples[k] = s;
    pm.p.Value.Sample = append((~pm.p).Sample, s);
    return s;
}

// key generates sampleKey to be used as a key for maps.
internal static sampleKey key(this ref Sample sample) {
    var ids = new slice<@string>(len(sample.Location));
    foreach (var (i, l) in sample.Location) {
        ids[i] = strconv.FormatUint((~l).ID, 16);
    }
    var labels = new slice<@string>(0, len(sample.Label));
    foreach (var (k, v) in sample.Label) {
        labels = append(labels, fmt.Sprintf("%q%q"u8, k, v));
    }
    sort.Strings(labels);
    var numlabels = new slice<@string>(0, len(sample.NumLabel));
    foreach (var (k, v) in sample.NumLabel) {
        numlabels = append(numlabels, fmt.Sprintf("%q%x%x"u8, k, v, sample.NumUnit[k]));
    }
    sort.Strings(numlabels);
    return new sampleKey(
        strings.Join(ids, "|"u8),
        strings.Join(labels, ""u8),
        strings.Join(numlabels, ""u8)
    );
}

partial struct sampleKey {
    internal @string locations;
    internal @string labels;
    internal @string numlabels;
}

internal static ж<Location> mapLocation(this ref profileMerger pm, ж<Location> Ꮡsrc) {
    ref var src = ref Ꮡsrc.DerefOrNull();

    if (Ꮡsrc == nil) {
        return default!;
    }
    {
        var (lΔ1, ok) = pm.locationsByID[src.ID, ꟷ]; if (ok) {
            pm.locationsByID[src.ID] = lΔ1;
            return lΔ1;
        }
    }
    var mi = pm.mapMapping(src.Mapping);
    var l = Ꮡ(new Location(
        ID: (uint64)(len((~pm.p).Location) + 1),
        Mapping: mi.m,
        Address: (uint64)((int64)src.Address + mi.offset),
        Line: new slice<Line>(len(src.Line)),
        IsFolded: src.IsFolded
    ));
    foreach (var (i, ln) in src.Line) {
        l.Value.Line[i] = pm.mapLine(ln);
    }
    // Check memoization table. Must be done on the remapped location to
    // account for the remapped mapping ID.
    var k = l.key();
    {
        var (ll, ok) = pm.locations[k, ꟷ]; if (ok) {
            pm.locationsByID[src.ID] = ll;
            return ll;
        }
    }
    pm.locationsByID[src.ID] = l;
    pm.locations[k] = l;
    pm.p.Value.Location = append((~pm.p).Location, l);
    return l;
}

// key generates locationKey to be used as a key for maps.
internal static locationKey key(this ref Location l) {
    var key = new locationKey(
        addr: l.Address,
        isFolded: l.IsFolded
    );
    if (l.Mapping != nil) {
        // Normalizes address to handle address space randomization.
        key.addr -= l.Mapping.Value.Start;
        key.mappingID = l.Mapping.Value.ID;
    }
    var lines = new slice<@string>(len(l.Line) * 2);
    foreach (var (i, line) in l.Line) {
        if (line.Function != nil) {
            lines[i * 2] = strconv.FormatUint((~line.Function).ID, 16);
        }
        lines[i * 2 + 1] = strconv.FormatInt(line.ΔLine, 16);
    }
    key.lines = strings.Join(lines, "|"u8);
    return key;
}

partial struct locationKey {
    internal uint64 addr, mappingID;
    internal @string lines;
    internal bool isFolded;
}

internal static mapInfo mapMapping(this ref profileMerger pm, ж<Mapping> Ꮡsrc) {
    ref var src = ref Ꮡsrc.DerefOrNull();

    if (Ꮡsrc == nil) {
        return new mapInfo(nil);
    }
    {
        var (miΔ1, ok) = pm.mappingsByID[src.ID, ꟷ]; if (ok) {
            return miΔ1;
        }
    }
    // Check memoization tables.
    var mk = src.key();
    {
        var (mΔ1, ok) = pm.mappings[mk, ꟷ]; if (ok) {
            var miΔ2 = new mapInfo(mΔ1, (int64)(~mΔ1).Start - (int64)src.Start);
            pm.mappingsByID[src.ID] = miΔ2;
            return miΔ2;
        }
    }
    var m = Ꮡ(new Mapping(
        ID: (uint64)(len((~pm.p).Mapping) + 1),
        Start: src.Start,
        Limit: src.Limit,
        Offset: src.Offset,
        File: src.File,
        BuildID: src.BuildID,
        HasFunctions: src.HasFunctions,
        HasFilenames: src.HasFilenames,
        HasLineNumbers: src.HasLineNumbers,
        HasInlineFrames: src.HasInlineFrames
    ));
    pm.p.Value.Mapping = append((~pm.p).Mapping, m);
    // Update memoization tables.
    pm.mappings[mk] = m;
    var mi = new mapInfo(m, 0);
    pm.mappingsByID[src.ID] = mi;
    return mi;
}

// key generates encoded strings of Mapping to be used as a key for
// maps.
internal static mappingKey key(this ref Mapping m) {
    // Normalize addresses to handle address space randomization.
    // Round up to next 4K boundary to avoid minor discrepancies.
    const uint64 mapsizeRounding = 0x1000;
    var size = m.Limit - m.Start;
    size = size + mapsizeRounding - 1;
    size = size - (size % mapsizeRounding);
    var key = new mappingKey(
        size: size,
        offset: m.Offset
    );
    switch (ᐧ) {
    case {} when m.BuildID != ""u8: {
        key.buildIDOrFile = m.BuildID;
        break;
    }
    case {} when m.File != ""u8: {
        key.buildIDOrFile = m.File;
        break;
    }
    default: {
        break;
    }}

    // A mapping containing neither build ID nor file name is a fake mapping. A
    // key with empty buildIDOrFile is used for fake mappings so that they are
    // treated as the same mapping during merging.
    return key;
}

partial struct mappingKey {
    internal uint64 size, offset;
    internal @string buildIDOrFile;
}

internal static Line mapLine(this ref profileMerger pm, Line src) {
    var ln = new Line(
        Function: pm.mapFunction(src.Function),
        ΔLine: src.ΔLine
    );
    return ln;
}

internal static ж<Function> mapFunction(this ref profileMerger pm, ж<Function> Ꮡsrc) {
    ref var src = ref Ꮡsrc.DerefOrNull();

    if (Ꮡsrc == nil) {
        return default!;
    }
    {
        var (fΔ1, ok) = pm.functionsByID[src.ID, ꟷ]; if (ok) {
            return fΔ1;
        }
    }
    var k = src.key();
    {
        var (fΔ2, ok) = pm.functions[k, ꟷ]; if (ok) {
            pm.functionsByID[src.ID] = fΔ2;
            return fΔ2;
        }
    }
    var f = Ꮡ(new Function(
        ID: (uint64)(len((~pm.p).Function) + 1),
        Name: src.Name,
        SystemName: src.SystemName,
        Filename: src.Filename,
        StartLine: src.StartLine
    ));
    pm.functions[k] = f;
    pm.functionsByID[src.ID] = f;
    pm.p.Value.Function = append((~pm.p).Function, f);
    return f;
}

// key generates a struct to be used as a key for maps.
internal static functionKey key(this ref Function f) {
    return new functionKey(
        f.StartLine,
        f.Name,
        f.SystemName,
        f.Filename
    );
}

partial struct functionKey {
    internal int64 startLine;
    internal @string name, systemName, fileName;
}

// combineHeaders checks that all profiles can be merged and returns
// their combined profile.
internal static (ж<Profile>, error) combineHeaders(slice<ж<Profile>> srcs) {
    foreach (var (_, s) in srcs[1..]) {
        {
            var err = srcs[0].compatible(s); if (err != default!) {
                return (default!, err);
            }
        }
    }
    ref var timeNanos = ref heap(new int64(), out var ᏑtimeNanos);
    ref var durationNanos = ref heap(new int64(), out var ᏑdurationNanos);
    ref var period = ref heap(new int64(), out var Ꮡperiod);
    slice<@string> comments = default!;
    var seenComments = new map<@string, bool>{};
    ref var defaultSampleType = ref heap(new @string(), out var ᏑdefaultSampleType);
    foreach (var (_, s) in srcs) {
        if (timeNanos == 0 || (~s).TimeNanos < timeNanos) {
            timeNanos = s.Value.TimeNanos;
        }
        durationNanos += s.Value.DurationNanos;
        if (period == 0 || period < (~s).Period) {
            period = s.Value.Period;
        }
        foreach (var (_, c) in (~s).Comments) {
            {
                var seen = seenComments[c]; if (!seen) {
                    comments = append(comments, c);
                    seenComments[c] = true;
                }
            }
        }
        if (defaultSampleType == ""u8) {
            defaultSampleType = s.Value.DefaultSampleType;
        }
    }
    var p = Ꮡ(new Profile(
        SampleType: new slice<ж<ValueType>>(len((~srcs[0]).SampleType)),
        DropFrames: (~srcs[0]).DropFrames,
        KeepFrames: (~srcs[0]).KeepFrames,
        TimeNanos: timeNanos,
        DurationNanos: durationNanos,
        PeriodType: (~srcs[0]).PeriodType,
        Period: period,
        Comments: comments,
        DefaultSampleType: defaultSampleType
    ));
    copy((~p).SampleType, (~srcs[0]).SampleType);
    return (p, default!);
}

// compatible determines if two profiles can be compared/merged.
// returns nil if the profiles are compatible; otherwise an error with
// details on the incompatibility.
internal static error compatible(this ref Profile p, ж<Profile> Ꮡpb) {
    ref var pb = ref Ꮡpb.DerefOrNull();

    if (!equalValueType(ref (p.PeriodType).DerefOrNull(), ref (pb.PeriodType).DerefOrNull())) {
        return fmt.Errorf("incompatible period types %v and %v"u8, p.PeriodType.OrTypedNil(), pb.PeriodType.OrTypedNil());
    }
    if (len(p.SampleType) != len(pb.SampleType)) {
        return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
    }
    foreach (var (i, _) in p.SampleType) {
        if (!equalValueType(ref (p.SampleType[i]).DerefOrNull(), ref (pb.SampleType[i]).DerefOrNull())) {
            return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
        }
    }
    return default!;
}

// equalValueType returns true if the two value types are semantically
// equal. It ignores the internal fields used during encode/decode.
internal static bool equalValueType(ref ValueType st1, ref ValueType st2) {
    return st1.Type == st2.Type && st1.Unit == st2.Unit;
}

} // end profile_package
