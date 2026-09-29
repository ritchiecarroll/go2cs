// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && (386 || amd64 || arm || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x)
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Look up symbols in the Linux vDSO.
// This code was originally based on the sample Linux vDSO parser at
// https://git.kernel.org/cgit/linux/kernel/git/torvalds/linux.git/tree/tools/testing/selftests/vDSO/parse_vdso.c
// This implements the ELF dynamic linking spec at
// http://sco.com/developers/gabi/latest/ch5.dynamic.html
// The version section is documented at
// https://refspecs.linuxfoundation.org/LSB_3.2.0/LSB-Core-generic/LSB-Core-generic/symversion.html
internal static UntypedInt _AT_SYSINFO_EHDR => 33;
internal static UntypedInt _PT_LOAD => 1; /* Loadable program segment */
internal static UntypedInt _PT_DYNAMIC => 2; /* Dynamic linking information */
internal static UntypedInt _DT_NULL => 0; /* Marks end of dynamic section */
internal static UntypedInt _DT_HASH => 4; /* Dynamic symbol hash table */
internal static UntypedInt _DT_STRTAB => 5; /* Address of string table */
internal static UntypedInt _DT_SYMTAB => 6; /* Address of symbol table */
internal static UntypedInt _DT_GNU_HASH => 0x6ffffef5; /* GNU-style dynamic symbol hash table */
internal static UntypedInt _DT_VERSYM => 0x6ffffff0;
internal static UntypedInt _DT_VERDEF => 0x6ffffffc;
internal static UntypedInt _VER_FLG_BASE => 0x1; /* Version definition of file itself */
internal static UntypedInt _SHN_UNDEF => 0; /* Undefined section */
internal static UntypedInt _SHT_DYNSYM => 11; /* Dynamic linker symbol table */
internal static UntypedInt _STT_FUNC => 2; /* Symbol is a code object */
internal static UntypedInt _STT_NOTYPE => 0; /* Symbol type is not specified */
internal static UntypedInt _STB_GLOBAL => 1; /* Global symbol */
internal static UntypedInt _STB_WEAK => 2; /* Weak symbol */
internal static UntypedInt _EI_NIDENT => 16;
internal static uintptr vdsoSymTabSize => /* vdsoArrayMax / unsafe.Sizeof(elfSym{}) */ unchecked((uintptr)46912496118442);
internal static uintptr vdsoDynSize => /* vdsoArrayMax / unsafe.Sizeof(elfDyn{}) */ unchecked((uintptr)70368744177663);
internal static UntypedInt vdsoSymStringsSize => /* vdsoArrayMax */ 1125899906842623; // byte
internal static UntypedInt vdsoVerSymSize => /* vdsoArrayMax / 2 */ 562949953421311; // uint16
internal static UntypedInt vdsoHashSize => /* vdsoArrayMax / 4 */ 281474976710655; // uint32
internal static uintptr vdsoBloomSizeScale => /* unsafe.Sizeof(uintptr(0)) / 4 */ 2; // uint32

/* How to extract and insert information held in the st_info field.  */
internal static byte _ELF_ST_BIND(byte val) {
    return (byte)((val >> (int)(4)));
}

internal static byte _ELF_ST_TYPE(byte val) {
    return (byte)(val & 0xf);
}

[GoType] partial struct vdsoSymbolKey {
    internal @string name;
    internal uint32 symHash;
    internal uint32 gnuHash;
    internal ж<uintptr> ptr;
}

[GoType] partial struct vdsoVersionKey {
    internal @string version;
    internal uint32 verHash;
}

[GoType] partial struct vdsoInfo {
    internal bool valid;
    /* Load information */
    internal uintptr loadAddr;
    internal uintptr loadOffset; /* loadAddr - recorded vaddr */
    /* Symbol table */
    [GoArrayDims(46912496118442)]
    internal ж<array<elfSym>> symtab;
    [GoArrayDims(1125899906842623)]
    internal ж<array<byte>> symstrings;
    internal slice<uint32> chain;
    internal slice<uint32> bucket;
    internal uint32 symOff;
    internal bool isGNUHash;
    /* Version table */
    [GoArrayDims(562949953421311)]
    internal ж<array<uint16>> versym;
    internal ж<elfVerdef> verdef;
}

internal static uintptr vdsoLoadStart;
internal static uintptr vdsoLoadEnd;

// see vdso_linux_*.go for vdsoSymbolKeys[] and vdso*Sym vars
internal static void vdsoInitFromSysinfoEhdr(ref vdsoInfo info, ж<elfEhdr> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.DerefOrNull();

    info.valid = false;
    info.loadAddr = (uintptr)Ꮡhdr;
    @unsafe.Pointer pt = (@unsafe.Pointer)(info.loadAddr + (uintptr)hdr.e_phoff);
    // We need two things from the segment table: the load offset
    // and the dynamic table.
    bool foundVaddr = default!;
    ж<array<elfDyn>> dyn = default!;
    for (var i = (uint16)0; i < hdr.e_phnum; i++) {
        var ptΔ1 = (ж<elfPhdr>)(uintptr)(add(pt, (uintptr)i * /* unsafe.Sizeof(elfPhdr{}) */ (uintptr)56));
        var exprᴛ1 = (~ptΔ1).p_type;
        if (exprᴛ1 == _PT_LOAD) {
            if (!foundVaddr) {
                foundVaddr = true;
                info.loadOffset = info.loadAddr + (uintptr)((~ptΔ1).p_offset - (~ptΔ1).p_vaddr);
                vdsoLoadStart = info.loadOffset;
                vdsoLoadEnd = info.loadOffset + (uintptr)(~ptΔ1).p_memsz;
            }
        }
        else if (exprᴛ1 == _PT_DYNAMIC) {
            dyn = (ж<array<elfDyn>>)(uintptr)((@unsafe.Pointer)(info.loadAddr + (uintptr)(~ptΔ1).p_offset));
        }

    }
    if (!foundVaddr || dyn == nil) {
        return; // Failed
    }
    // Fish out the useful bits of the dynamic table.
    ж<array<uint32>> hash = default!;
    ж<array<uint32>> gnuhash = default!;
    info.symstrings = ж<array<byte>>.NilBoxOfDims(1125899906842623L);
    info.symtab = ж<array<elfSym>>.NilBoxOfDims(46912496118442L);
    info.versym = ж<array<uint16>>.NilBoxOfDims(562949953421311L);
    info.verdef = default!;
    for (nint i = 0; dyn.Value[i].d_tag != _DT_NULL; i++) {
        var dt = dyn.at<elfDyn>(i);
        var Δp = info.loadOffset + (uintptr)(~dt).d_val;
        var exprᴛ2 = (~dt).d_tag;
        if (exprᴛ2 == _DT_STRTAB) {
            info.symstrings = (ж<array<byte>>)(uintptr)((@unsafe.Pointer)Δp);
        }
        else if (exprᴛ2 == _DT_SYMTAB) {
            info.symtab = (ж<array<elfSym>>)(uintptr)((@unsafe.Pointer)Δp);
        }
        else if (exprᴛ2 == _DT_HASH) {
            hash = (ж<array<uint32>>)(uintptr)((@unsafe.Pointer)Δp);
        }
        else if (exprᴛ2 == _DT_GNU_HASH) {
            gnuhash = (ж<array<uint32>>)(uintptr)((@unsafe.Pointer)Δp);
        }
        else if (exprᴛ2 == _DT_VERSYM) {
            info.versym = (ж<array<uint16>>)(uintptr)((@unsafe.Pointer)Δp);
        }
        else if (exprᴛ2 == _DT_VERDEF) {
            info.verdef = (ж<elfVerdef>)(uintptr)((@unsafe.Pointer)Δp);
        }

    }
    if (info.symstrings == nil || info.symtab == nil || (hash == nil && gnuhash == nil)) {
        return; // Failed
    }
    if (info.verdef == nil) {
        info.versym = ж<array<uint16>>.NilBoxOfDims(562949953421311L);
    }
    if (gnuhash != nil){
        // Parse the GNU hash table header.
        var nbucket = gnuhash.Value[0];
        info.symOff = gnuhash.Value[1];
        var bloomSize = gnuhash.Value[2];
        info.bucket = (~gnuhash).slice((nint)(4 + bloomSize * (uint32)vdsoBloomSizeScale)).slice(0, (nint)(nbucket));
        info.chain = (~gnuhash).slice((nint)(4 + bloomSize * (uint32)vdsoBloomSizeScale + nbucket));
        info.isGNUHash = true;
    } else {
        // Parse the hash table header.
        var nbucket = hash.Value[0];
        var nchain = hash.Value[1];
        info.bucket = (~hash).slice(2, (nint)(2 + nbucket));
        info.chain = (~hash).slice((nint)(2 + nbucket), (nint)(2 + nbucket + nchain));
    }
    // That's all we need.
    info.valid = true;
}

internal static int32 vdsoFindVersion(ж<vdsoInfo> Ꮡinfo, ref vdsoVersionKey ver) {
    ref var info = ref Ꮡinfo.DerefOrNull();

    if (!info.valid) {
        return 0;
    }
    var def = info.verdef;
    while (ᐧ) {
        if ((uint16)((~def).vd_flags & (uint16)_VER_FLG_BASE) == 0) {
            var aux = (ж<elfVerdaux>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(def), (uintptr)(~def).vd_aux));
            if ((~def).vd_hash == ver.verHash && ver.version == gostringnocopy(info.symstrings.at<byte>((ulong)((~aux).vda_name)))) {
                return (int32)((uint16)((~def).vd_ndx & 0x7fff));
            }
        }
        if ((~def).vd_next == 0) {
            break;
        }
        def = (ж<elfVerdef>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(def), (uintptr)(~def).vd_next));
    }
    return -1; // cannot match any version
}

internal static void vdsoParseSymbols(ж<vdsoInfo> Ꮡinfo, int32 version) {
    ref var info = ref Ꮡinfo.DerefOrNull();

    if (!info.valid) {
        return;
    }
    bool apply(uint32 symIndex, vdsoSymbolKey k) {
        var sym = Ꮡinfo.Value.symtab.at<elfSym>((ulong)(symIndex));
        var typ = _ELF_ST_TYPE((~sym).st_info);
        var bind = _ELF_ST_BIND((~sym).st_info);
        // On ppc64x, VDSO functions are of type _STT_NOTYPE.
        if (typ != _STT_FUNC && typ != _STT_NOTYPE || bind != _STB_GLOBAL && bind != _STB_WEAK || (~sym).st_shndx == _SHN_UNDEF) {
            return false;
        }
        if (k.name != gostringnocopy(Ꮡinfo.Value.symstrings.at<byte>((ulong)((~sym).st_name)))) {
            return false;
        }
        // Check symbol version.
        if (Ꮡinfo.Value.versym != nil && version != 0 && (int32)((uint16)(Ꮡinfo.Value.versym.Value[symIndex] & 0x7fff)) != version) {
            return false;
        }
        k.ptr.Value = Ꮡinfo.Value.loadOffset + (uintptr)(~sym).st_value;
        return true;
    }
    if (!info.isGNUHash) {
        // Old-style DT_HASH table.
        foreach (var (_, k) in vdsoSymbolKeys) {
            if (len(info.bucket) > 0) {
                for (var chain = info.bucket[k.symHash % (uint32)len(info.bucket)]; chain != 0; chain = info.chain[chain]) {
                    if (apply(chain, k)) {
                        break;
                    }
                }
            }
        }
        return;
    }
    // New-style DT_GNU_HASH table.
    foreach (var (_, k) in vdsoSymbolKeys) {
        var symIndex = info.bucket[k.gnuHash % (uint32)len(info.bucket)];
        if (symIndex < info.symOff) {
            continue;
        }
        for (; ᐧ ; symIndex++) {
            var hash = info.chain[symIndex - info.symOff];
            if ((uint32)(hash | 1) == (uint32)(k.gnuHash | 1)) {
                // Found a hash match.
                if (apply(symIndex, k)) {
                    break;
                }
            }
            if ((uint32)(hash & 1) != 0) {
                // End of chain.
                break;
            }
        }
    }
}

internal static void vdsoauxv(uintptr tag, uintptr val) {
    var exprᴛ1 = tag;
    if (exprᴛ1 == _AT_SYSINFO_EHDR) {
        if (val == 0) {
            // Something went wrong
            return;
        }
        ref var info = ref heap(new vdsoInfo(), out var Ꮡinfo);
        var info1 = Ꮡinfo;
        vdsoInitFromSysinfoEhdr(ref (info1).DerefOrNull(), // TODO(rsc): I don't understand why the compiler thinks info escapes
 // when passed to the three functions below.
 (ж<elfEhdr>)(uintptr)((@unsafe.Pointer)val));
        vdsoParseSymbols(info1, vdsoFindVersion(info1, ref vdsoLinuxVersion));
    }

}

// vdsoMarker reports whether PC is on the VDSO page.
//
//go:nosplit
internal static bool inVDSOPage(uintptr pc) {
    return pc >= vdsoLoadStart && pc < vdsoLoadEnd;
}

} // end runtime_package
