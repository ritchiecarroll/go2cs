// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using binary = encoding.binary_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using @internal;
using encoding;

partial class macho_package {

// A FatFile is a Mach-O universal binary that contains at least one architecture.
partial struct FatFile {
    public uint32 Magic;
    public slice<FatArch> Arches;
    internal io.Closer closer;
}

// A FatArchHeader represents a fat header for a specific image architecture.
partial struct FatArchHeader {
    public Cpu Cpu;
    public uint32 SubCpu;
    public uint32 Offset;
    public uint32 Size;
    public uint32 Align;
}

internal static UntypedInt fatArchHeaderSize => /* 5 * 4 */ 20;

// A FatArch is a Mach-O File inside a FatFile.
partial struct FatArch {
    public partial ref FatArchHeader FatArchHeader { get; }
    public partial ref ж<File> File { get; }
}

// ErrNotFat is returned from [NewFatFile] or [OpenFat] when the file is not a
// universal binary but may be a thin binary, based on its magic number.
public static ж<FormatError> ErrNotFat = Ꮡ(new FormatError(0, "not a fat Mach-O file"u8, default!));

// NewFatFile creates a new [FatFile] for accessing all the Mach-O images in a
// universal binary. The Mach-O binary is expected to start at position 0 in
// the ReaderAt.
public static (ж<FatFile>, error) NewFatFile(io.ReaderAt r) {
    ref var ff = ref heap(new FatFile(), out var Ꮡff);
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    // Read the fat_header struct, which is always in big endian.
    // Start with the magic number.
    var err = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡff.of(FatFile.ᏑMagic));
    if (err != default!){
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "error reading magic number"u8, default!))));
    } else 
    if (ff.Magic != MagicFat) {
        // See if this is a Mach-O file via its magic number. The magic
        // must be converted to little endian first though.
        array<byte> buf = new(4);
        binary.BigEndian.PutUint32(buf[..], ff.Magic);
        var leMagic = binary.LittleEndian.Uint32(buf[..]);
        if (leMagic == Magic32 || leMagic == Magic64){
            return (default!, new FormatErrorжerror(ErrNotFat));
        } else {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid magic number"u8, default!))));
        }
    }
    ref var offset = ref heap<int64>(out var Ꮡoffset);
    offset = (int64)4;
    // Read the number of FatArchHeaders that come after the fat_header.
    ref var narch = ref heap(new uint32(), out var Ꮡnarch);
    err = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡnarch);
    if (err != default!) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid fat_header"u8, default!))));
    }
    offset += 4;
    if (narch < 1) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "file contains no images"u8, default!))));
    }
    // Combine the Cpu and SubCpu (both uint32) into a uint64 to make sure
    // there are not duplicate architectures.
    var seenArches = new map<uint64, bool>();
    // Make sure that all images are for the same MH_ type.
    Type machoType = default!;
    // Following the fat_header comes narch fat_arch structs that index
    // Mach-O images further in the file.
    nint c = saferio.SliceCap<FatArch>((uint64)narch);
    if (c < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "too many images"u8, default!))));
    }
    ff.Arches = new slice<FatArch>(0, () => new(nil), c);
    for (var i = (uint32)0; i < narch; i++) {
        ref var fa = ref heap(new FatArch(), out var Ꮡfa);
        err = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡfa.of(FatArch.ᏑFatArchHeader));
        if (err != default!) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid fat_arch header"u8, default!))));
        }
        offset += fatArchHeaderSize;
        var fr = io.NewSectionReader(r, (int64)fa.Offset, (int64)fa.Size);
        (fa.File, err) = NewFile(new io.SectionReaderжReaderAt(fr));
        if (err != default!) {
            return (default!, err);
        }
        // Make sure the architecture for this image is not duplicate.
        var seenArch = (uint64)((((uint64)(uint32)fa.Cpu << (int)(32))) | (uint64)fa.SubCpu);
        {
            var (o, k) = seenArches[seenArch, ꟷ]; if (o || k) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, fmt.Sprintf("duplicate architecture cpu=%v, subcpu=%#x"u8, fa.Cpu, fa.SubCpu), default!))));
            }
        }
        seenArches[seenArch] = true;
        // Make sure the Mach-O type matches that of the first image.
        if (i == 0){
            machoType = fa.Type;
        } else {
            if (fa.Type != machoType) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, fmt.Sprintf("Mach-O type for architecture #%d (type=%#x) does not match first (type=%#x)"u8, i, fa.Type, machoType), default!))));
            }
        }
        ff.Arches = append(ff.Arches, fa);
    }
    return (Ꮡff, default!);
}

// OpenFat opens the named file using [os.Open] and prepares it for use as a Mach-O
// universal binary.
public static (ж<FatFile>, error) OpenFat(@string name) {
    var (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    (var ff, err) = NewFatFile(new os_FileжReaderAt(f));
    if (err != default!) {
        f.Close();
        return (default!, err);
    }
    ff.Value.closer = new os_FileжCloser(f);
    return (ff, default!);
}

public static error Close(this ref FatFile ff) {
    error err = default!;
    if (ff.closer != default!) {
        err = ff.closer.Close();
        ff.closer = default!;
    }
    return err;
}

} // end macho_package
