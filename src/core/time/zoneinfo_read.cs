// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;

partial class time_package {

// registerLoadFromEmbeddedTZData is called by the time/tzdata package,
// if it is imported.
//
//go:linkname registerLoadFromEmbeddedTZData
public static void registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> f) {
    loadFromEmbeddedTZData = f;
}

// loadFromEmbeddedTZData is used to load a specific tzdata file
// from tzdata information embedded in the binary itself.
// This is set when the time/tzdata package is imported,
// via registerLoadFromEmbeddedTzdata.
internal static Func<@string, (@string, error)> loadFromEmbeddedTZData;

// maxFileSize is the max permitted size of files read by readFile.
// As reference, the zoneinfo.zip distributed by Go is ~350 KB,
// so 10MB is overkill.
internal static UntypedInt maxFileSize => /* 10 << 20 */ 10485760;

[GoType("@string")] partial struct fileSizeError;

internal static @string Error(this fileSizeError f) {
    return "time: file "u8 + ((@string)f) + " is too large"u8;
}

// Copies of io.Seek* constants to avoid importing "io":
internal static UntypedInt seekStart => 0;

internal static UntypedInt seekCurrent => 1;

internal static UntypedInt seekEnd => 2;

// Simple I/O interface to binary blob of data.
[GoType] partial struct dataIO {
    internal slice<byte> p;
    internal bool error;
}

[GoRecv] internal static slice<byte> read(this ref dataIO d, nint n) {
    if (len(d.p) < n) {
        d.p = default!;
        d.error = true;
        return default!;
    }
    var p = d.p.slice(0, n);
    d.p = d.p.slice(n);
    return p;
}

[GoRecv] internal static (uint32 n, bool ok) big4(this ref dataIO d) {
    var p = d.read(4);
    if (len(p) < 4) {
        d.error = true;
        return (0, false);
    }
    return ((uint32)((uint32)((uint32)((uint32)p[3] | ((uint32)p[2] << (int)(8))) | ((uint32)p[1] << (int)(16))) | ((uint32)p[0] << (int)(24))), true);
}

[GoRecv] internal static (uint64 n, bool ok) big8(this ref dataIO d) {
    var (n1, ok1) = d.big4();
    var (n2, ok2) = d.big4();
    if (!ok1 || !ok2) {
        d.error = true;
        return (0, false);
    }
    return ((uint64)((((uint64)n1 << (int)(32))) | (uint64)n2), true);
}

[GoRecv] internal static (byte n, bool ok) @byte(this ref dataIO d) {
    var p = d.read(1);
    if (len(p) < 1) {
        d.error = true;
        return (0, false);
    }
    return (p[0], true);
}

// rest returns the rest of the data in the buffer.
[GoRecv] internal static slice<byte> rest(this ref dataIO d) {
    var r = d.p;
    d.p = default!;
    return r;
}

// Make a string by stopping at the first NUL
internal static @string byteString(slice<byte> p) {
    {
        nint i = bytealg.IndexByte(p, 0); if (i != -1) {
            p = p.slice(0, i);
        }
    }
    return ((@string)p);
}

internal static error errBadData = errors.New("malformed time zone information"u8);

// LoadLocationFromTZData returns a Location with the given name
// initialized from the IANA Time Zone database-formatted data.
// The data should be in the format of a standard IANA time zone file
// (for example, the content of /etc/localtime on Unix systems).
public static (ж<ΔLocation>, error) LoadLocationFromTZData(@string name, slice<byte> data) {
    var d = new dataIO(data, false);
    // 4-byte magic "TZif"
    {
        var magic = d.read(4); if (((sstring)magic) != "TZif"u8) {
            return (default!, errBadData);
        }
    }
    // 1-byte version, then 15 bytes of padding
    nint version = default!;
    slice<byte> p = default!;
    {
        p = d.read(16); if (len(p) != 16){
            return (default!, errBadData);
        } else {
            switch (p[0]) {
            case 0: {
                version = 1;
                break;
            }
            case (rune)'2': {
                version = 2;
                break;
            }
            case (rune)'3': {
                version = 3;
                break;
            }
            default: {
                return (default!, errBadData);
            }}

        }
    }
    // six big-endian 32-bit integers:
    //	number of UTC/local indicators
    //	number of standard/wall indicators
    //	number of leap seconds
    //	number of transition times
    //	number of local time zones
    //	number of characters of time zone abbrev strings
    const nint NUTCLocal = iota;
    
    const nint NStdWall = 1;
    
    const nint NLeap = 2;
    
    const nint NTime = 3;
    
    const nint NZone = 4;
    
    const nint NChar = 5;
    array<nint> n = new(6);
    for (nint i = 0; i < 6; i++) {
        var (nn, ok) = d.big4();
        if (!ok) {
            return (default!, errBadData);
        }
        if ((uint32)(nint)nn != nn) {
            return (default!, errBadData);
        }
        n[i] = (nint)nn;
    }
    // If we have version 2 or 3, then the data is first written out
    // in a 32-bit format, then written out again in a 64-bit format.
    // Skip the 32-bit format and read the 64-bit one, as it can
    // describe a broader range of dates.
    var is64 = false;
    if (version > 1) {
        // Skip the 32-bit data.
        nint skip = n[NTime] * 4 + n[NTime] + n[NZone] * 6 + n[NChar] + n[NLeap] * 8 + n[NStdWall] + n[NUTCLocal];
        // Skip the version 2 header that we just read.
        skip += 4 + 16;
        d.read(skip);
        is64 = true;
        // Read the counts again, they can differ.
        for (nint i = 0; i < 6; i++) {
            var (nn, ok) = d.big4();
            if (!ok) {
                return (default!, errBadData);
            }
            if ((uint32)(nint)nn != nn) {
                return (default!, errBadData);
            }
            n[i] = (nint)nn;
        }
    }
    nint size = 4;
    if (is64) {
        size = 8;
    }
    // Transition times.
    var txtimes = new dataIO(d.read(n[NTime] * size), false);
    // Time zone indices for transition times.
    var txzones = d.read(n[NTime]);
    // Zone info structures
    var zonedata = new dataIO(d.read(n[NZone] * 6), false);
    // Time zone abbreviations.
    var abbrev = d.read(n[NChar]);
    // Leap-second time pairs
    d.read(n[NLeap] * (size + 4));
    // Whether tx times associated with local time types
    // are specified as standard time or wall time.
    var isstd = d.read(n[NStdWall]);
    // Whether tx times associated with local time types
    // are specified as UTC or local time.
    var isutc = d.read(n[NUTCLocal]);
    if (d.error) {
        // ran out of data
        return (default!, errBadData);
    }
    ref var extend = ref heap(new @string(), out var Ꮡextend);
    var rest = d.rest();
    if (len(rest) > 2 && rest[0] == (rune)'\n' && rest[len(rest) - 1] == (rune)'\n') {
        extend = ((@string)(rest.slice(1, len(rest) - 1)));
    }
    // Now we can build up a useful data structure.
    // First the zone information.
    //	utcoff[4] isdst[1] nameindex[1]
    nint nzone = n[NZone];
    if (nzone == 0) {
        // Reject tzdata files with no zones. There's nothing useful in them.
        // This also avoids a panic later when we add and then use a fake transition (golang.org/issue/29437).
        return (default!, errBadData);
    }
    var zones = new slice<zone>(nzone);
    foreach (var (i, _) in zones) {
        bool ok = default!;
        uint32 nΔ1 = default!;
        {
            (nΔ1, ok) = zonedata.big4(); if (!ok) {
                return (default!, errBadData);
            }
        }
        if ((uint32)(nint)nΔ1 != nΔ1) {
            return (default!, errBadData);
        }
        zones[i].offset = (nint)(int32)nΔ1;
        byte b = default!;
        {
            (b, ok) = zonedata.@byte(); if (!ok) {
                return (default!, errBadData);
            }
        }
        zones[i].isDST = b != 0;
        {
            (b, ok) = zonedata.@byte(); if (!ok || (nint)b >= len(abbrev)) {
                return (default!, errBadData);
            }
        }
        zones[i].name = byteString(abbrev.slice(b));
        if (runtime.GOOS == "aix"u8 && len(name) > 8 && (name[..8] == "Etc/GMT+" || name[..8] == "Etc/GMT-")) {
            // There is a bug with AIX 7.2 TL 0 with files in Etc,
            // GMT+1 will return GMT-1 instead of GMT+1 or -01.
            if (name != "Etc/GMT+0"u8) {
                // GMT+0 is OK
                zones[i].name = name[4..];
            }
        }
    }
    // Now the transition time info.
    var tx = new slice<zoneTrans>(n[NTime]);
    foreach (var (i, _) in tx) {
        int64 nΔ2 = default!;
        if (!is64){
            {
                var (n4, ok) = txtimes.big4(); if (!ok){
                    return (default!, errBadData);
                } else {
                    nΔ2 = (int64)(int32)n4;
                }
            }
        } else {
            {
                var (n8, ok) = txtimes.big8(); if (!ok){
                    return (default!, errBadData);
                } else {
                    nΔ2 = (int64)n8;
                }
            }
        }
        tx[i].when = nΔ2;
        if ((nint)txzones[i] >= len(zones)) {
            return (default!, errBadData);
        }
        tx[i].index = txzones[i];
        if (i < len(isstd)) {
            tx[i].isstd = isstd[i] != 0;
        }
        if (i < len(isutc)) {
            tx[i].isutc = isutc[i] != 0;
        }
    }
    if (len(tx) == 0) {
        // Build fake transition to cover all time.
        // This happens in fixed locations like "Etc/GMT0".
        tx = append(tx, new zoneTrans(when: alpha, index: 0));
    }
    // Committed to succeed.
    var l = Ꮡ(new ΔLocation(zone: zones, tx: tx, name: name, extend: extend));
    // Fill in the cache with information about right now,
    // since that will be the most common lookup.
    var (sec, _, _) = runtimeNow();
    foreach (var (i, _) in tx) {
        if (tx[i].when <= sec && (i + 1 == len(tx) || sec < tx[i + 1].when)) {
            l.Value.cacheStart = tx[i].when;
            l.Value.cacheEnd = omega;
            l.Value.cacheZone = Ꮡ((~l).zone, tx[i].index);
            if (i + 1 < len(tx)){
                l.Value.cacheEnd = tx[i + 1].when;
            } else 
            if ((~l).extend != ""u8) {
                // If we're at the end of the known zone transitions,
                // try the extend string.
                {
                    ref var nameΔ1 = ref heap<@string>(out var ᏑnameΔ1);
                    ref var offset = ref heap<nint>(out var Ꮡoffset);
                    ref var isDST = ref heap<bool>(out var ᏑisDST);
                    (nameΔ1, offset, var estart, var eend, isDST, var ok) = tzset((~l).extend, (~l).cacheStart, sec); if (ok) {
                        l.Value.cacheStart = estart;
                        l.Value.cacheEnd = eend;
                        // Find the zone that is returned by tzset to avoid allocation if possible.
                        {
                            nint zoneIdx = findZone((~l).zone, nameΔ1, offset, isDST); if (zoneIdx != -1){
                                l.Value.cacheZone = Ꮡ((~l).zone, zoneIdx);
                            } else {
                                l.Value.cacheZone = Ꮡ(new zone(
                                    name: nameΔ1,
                                    offset: offset,
                                    isDST: isDST
                                ));
                            }
                        }
                    }
                }
            }
            break;
        }
    }
    return (l, default!);
}

internal static nint findZone(slice<zone> zones, @string name, nint offset, bool isDST) {
    foreach (var (i, z) in zones) {
        if (z.name == name && z.offset == offset && z.isDST == isDST) {
            return i;
        }
    }
    return -1;
}

// loadTzinfoFromDirOrZip returns the contents of the file with the given name
// in dir. dir can either be an uncompressed zip file, or a directory.
internal static (slice<byte>, error) loadTzinfoFromDirOrZip(@string dir, @string name) {
    if (len(dir) > 4 && dir.slice(len(dir) - 4) == ".zip") {
        return loadTzinfoFromZip(dir, name);
    }
    if (dir != ""u8) {
        name = dir + "/"u8 + name;
    }
    return readFile(name);
}

// There are 500+ zoneinfo files. Rather than distribute them all
// individually, we ship them in an uncompressed zip file.
// Used this way, the zip file format serves as a commonly readable
// container for the individual small files. We choose zip over tar
// because zip files have a contiguous table of contents, making
// individual file lookups faster, and because the per-file overhead
// in a zip file is considerably less than tar's 512 bytes.

// get4 returns the little-endian 32-bit value in b.
internal static nint get4(slice<byte> b) {
    if (len(b) < 4) {
        return 0;
    }
    return (nint)((nint)((nint)((nint)b[0] | ((nint)b[1] << (int)(8))) | ((nint)b[2] << (int)(16))) | ((nint)b[3] << (int)(24)));
}

// get2 returns the little-endian 16-bit value in b.
internal static nint get2(slice<byte> b) {
    if (len(b) < 2) {
        return 0;
    }
    return (nint)((nint)b[0] | ((nint)b[1] << (int)(8)));
}

// loadTzinfoFromZip returns the contents of the file with the given name
// in the given uncompressed zip file.
internal static (slice<byte>, error) loadTzinfoFromZip(@string zipfile, @string name) {
    GoFrame ᒐ = default;
    try {
        var (fd, err) = open(zipfile);
        if (err != default!) {
            return (default!, err);
        }
        defer(closefd, fd, ref ᒐ);
        const nint zecheader = 0x06054b50;
        const nint zcheader = 0x02014b50;
        UntypedInt ztailsize = 22;
        const nint zheadersize = 30;
        const nint zheader = 0x04034b50;
        var buf = new slice<byte>(ztailsize);
        {
            var errΔ1 = preadn(fd, buf, -ztailsize); if (errΔ1 != default! || get4(buf) != zecheader) {
                return (default!, errors.New("corrupt zip file "u8 + zipfile));
            }
        }
        nint n = get2(buf[10..]);
        nint size = get4(buf[12..]);
        nint off = get4(buf[16..]);
        buf = new slice<byte>(size);
        {
            var errΔ2 = preadn(fd, buf, off); if (errΔ2 != default!) {
                return (default!, errors.New("corrupt zip file "u8 + zipfile));
            }
        }
        for (nint i = 0; i < n; i++) {
            // zip entry layout:
            //	0	magic[4]
            //	4	madevers[1]
            //	5	madeos[1]
            //	6	extvers[1]
            //	7	extos[1]
            //	8	flags[2]
            //	10	meth[2]
            //	12	modtime[2]
            //	14	moddate[2]
            //	16	crc[4]
            //	20	csize[4]
            //	24	uncsize[4]
            //	28	namelen[2]
            //	30	xlen[2]
            //	32	fclen[2]
            //	34	disknum[2]
            //	36	iattr[2]
            //	38	eattr[4]
            //	42	off[4]
            //	46	name[namelen]
            //	46+namelen+xlen+fclen - next header
            //
            if (get4(buf) != zcheader) {
                break;
            }
            nint meth = get2(buf[10..]);
            nint sizeΔ1 = get4(buf[24..]);
            nint namelen = get2(buf[28..]);
            nint xlen = get2(buf[30..]);
            nint fclen = get2(buf[32..]);
            nint offΔ1 = get4(buf[42..]);
            var zname = buf.slice(46, 46 + namelen);
            buf = buf.slice(46 + namelen + xlen + fclen);
            if (((sstring)zname) != name) {
                continue;
            }
            if (meth != 0) {
                return (default!, errors.New("unsupported compression for "u8 + name + " in "u8 + zipfile));
            }
            // zip per-file header layout:
            //	0	magic[4]
            //	4	extvers[1]
            //	5	extos[1]
            //	6	flags[2]
            //	8	meth[2]
            //	10	modtime[2]
            //	12	moddate[2]
            //	14	crc[4]
            //	18	csize[4]
            //	22	uncsize[4]
            //	26	namelen[2]
            //	28	xlen[2]
            //	30	name[namelen]
            //	30+namelen+xlen - file data
            //
            buf = new slice<byte>(zheadersize + namelen);
            {
                var errΔ3 = preadn(fd, buf, offΔ1); if (errΔ3 != default! || get4(buf) != zheader || get2(buf[8..]) != meth || get2(buf[26..]) != namelen || ((sstring)(buf.slice(30, 30 + namelen))) != name) {
                    return (default!, errors.New("corrupt zip file "u8 + zipfile));
                }
            }
            xlen = get2(buf[28..]);
            buf = new slice<byte>(sizeΔ1);
            {
                var errΔ4 = preadn(fd, buf, offΔ1 + 30 + namelen + xlen); if (errΔ4 != default!) {
                    return (default!, errors.New("corrupt zip file "u8 + zipfile));
                }
            }
            return (buf, default!);
        }
        return (default!, syscall.ENOENT);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// loadTzinfoFromTzdata returns the time zone information of the time zone
// with the given name, from a tzdata database file as they are typically
// found on android.
internal static Func<@string, @string, (slice<byte>, error)> loadTzinfoFromTzdata;

// loadTzinfo returns the time zone information of the time zone
// with the given name, from a given source. A source may be a
// timezone database directory, tzdata database file or an uncompressed
// zip file, containing the contents of such a directory.
internal static (slice<byte>, error) loadTzinfo(@string name, @string source) {
    if (len(source) >= 6 && source.slice(len(source) - 6) == "tzdata") {
        return loadTzinfoFromTzdata(source, name);
    }
    return loadTzinfoFromDirOrZip(source, name);
}

// loadLocation returns the Location with the given name from one of
// the specified sources. See loadTzinfo for a list of supported sources.
// The first timezone data matching the given name that is successfully loaded
// and parsed is returned as a Location.
internal static (ж<ΔLocation> z, error firstErr) loadLocation(@string name, slice<@string> sources) {
    ж<ΔLocation> z = default!;
    error firstErr = default!;

    foreach (var (_, source) in sources) {
        var (zoneData, err) = loadTzinfo(name, source);
        if (err == default!) {
            {
                (z, err) = LoadLocationFromTZData(name, zoneData); if (err == default!) {
                    return (z, default!);
                }
            }
        }
        if (firstErr == default! && !AreEqual(err, syscall.ENOENT)) {
            firstErr = err;
        }
    }
    if (loadFromEmbeddedTZData != default!) {
        var (zoneData, err) = loadFromEmbeddedTZData(name);
        if (err == default!) {
            {
                (z, err) = LoadLocationFromTZData(name, slice<byte>(zoneData)); if (err == default!) {
                    return (z, default!);
                }
            }
        }
        if (firstErr == default! && !AreEqual(err, syscall.ENOENT)) {
            firstErr = err;
        }
    }
    {
        var (source, ok) = gorootZoneSource(runtime.GOROOT()); if (ok) {
            var (zoneData, err) = loadTzinfo(name, source);
            if (err == default!) {
                {
                    (z, err) = LoadLocationFromTZData(name, zoneData); if (err == default!) {
                        return (z, default!);
                    }
                }
            }
            if (firstErr == default! && !AreEqual(err, syscall.ENOENT)) {
                firstErr = err;
            }
        }
    }
    if (firstErr != default!) {
        return (default!, firstErr);
    }
    return (default!, errors.New("unknown time zone "u8 + name));
}

// readFile reads and returns the content of the named file.
// It is a trivial implementation of os.ReadFile, reimplemented
// here to avoid depending on io/ioutil or os.
// It returns an error if name exceeds maxFileSize bytes.
internal static (slice<byte>, error) readFile(@string name) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = open(name);
        if (err != default!) {
            return (default!, err);
        }
        defer(closefd, f, ref ᒐ);
        array<byte> buf = new(4096);
        slice<byte> ret = default!;
        nint n = default!;
        while (ᐧ) {
            (n, err) = read(f, buf[..]);
            if (n > 0) {
                ret = appendꓸꓸꓸ(ret, buf.slice(0, n));
            }
            if (n == 0 || err != default!) {
                break;
            }
            if (len(ret) > maxFileSize) {
                return (default!, ((fileSizeError)name));
            }
        }
        return (ret, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end time_package
