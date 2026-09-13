// symbol_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of debug/pe's COFF symbol reader pair — the two declarations whose
// Go bodies re-VIEW one 18-byte symbol record as two struct shapes through unsafe.Pointer:
//
//	aux := (*COFFSymbolAuxFormat5)(unsafe.Pointer(&sym))    // readCOFFSymbols' aux arm
//	rv = (*COFFSymbolAuxFormat5)(unsafe.Pointer(pesymn))    // COFFSymbolReadSectionDefAux
//
// In Go the cast is a free re-typing: COFFSymbol ([8]uint8 + uint32 + int16 + uint16 + uint8 +
// uint8) and COFFSymbolAuxFormat5 (uint32 + uint16 + uint16 + uint32 + uint16 + uint8 + [3]uint8)
// are both exactly 18 bytes with no padding, tiling the same little-endian record. The managed
// surrogates are NOT those bytes: COFFSymbol.Name is an `array<uint8>` MANAGED REFERENCE where Go
// has 8 inline octets, so the two shapes share no Go-compatible managed layout, golib's
// Reinterpret alias arm correctly refuses the pair, and the fallback view puns the C# layouts —
// the scalars land bijectively (the write in readCOFFSymbols and the read in the aux accessor
// cross the same view, so values round-trip), but the aux shape's blank `_ [3]uint8` SLOT overlays
// the Name reference and answers with the 8-element Name array itself. Measured by Go's own
// TestReadCOFFSymbolAuxInfo: `_:[0 0 0 0 0 0 0 0]` where Go prints `_:[0 0 0]`. Same family as
// the zero-size/layout-emission arc — the C# struct is not the Go struct's bytes — and the board's
// debug/pe entry records this one-file hand-own of the symbol reader as the package's remedy.
//
// The managed form transcribes the GO layout explicitly instead of punning the managed one:
//
//   - readCOFFSymbols reads EVERY 18-byte record — primary and aux alike — through the COFFSymbol
//     shape. For an aux record that is byte-identical to Go's aux-view read (Name[0:8] carries
//     Size+NumRelocs+NumLineNumbers, Value carries Checksum, SectionNumber carries SecNum, Type
//     carries Selection | _[0]<<8, StorageClass carries _[1], NumberOfAuxSymbols carries _[2]),
//     except that Go's decoder SKIPS the blank `_` field — the stream bytes are consumed but the
//     destination stays zero — so the three pad positions are zeroed after the read.
//     File.COFFSymbols therefore holds exactly the field values Go's memory holds.
//
//   - COFFSymbolReadSectionDefAux decodes the successor record's Go-layout image back into a real
//     COFFSymbolAuxFormat5 box, whose `_` is a genuine [3]uint8. One deliberate difference from
//     Go, in the safe direction: the returned pointer is a fresh box, not an alias into
//     f.COFFSymbols, so a WRITE through it does not mutate the symbol table and two calls do not
//     compare pointer-equal. Nothing in the package, its tests, or its stdlib consumers does
//     either — Go's own doc frames the result as "a blob of auxiliary information" to read.

// Hand-owned (no symbol_impl.go exists, so a reconvert never regenerates this file). The two
// declarations it replaces are registered in the converter's manualConversionFuncs
// (src/go2cs/manualTypeOperations.go), which is what turns their generated bodies in symbol.cs
// into placeholders.
[module: go.GoManualConversion]

namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using saferio = @internal.saferio_package;
using encoding;

partial class pe_package {

// readCOFFSymbols reads in the symbol table for a PE file, returning a slice of COFFSymbol
// objects — see symbol.cs for the full Go doc comment. Hand-owned: the Go body reads auxiliary
// records through a (*COFFSymbolAuxFormat5) re-view of the primary struct's storage; the managed
// form reads every record through the COFFSymbol shape, which tiles the same 18 little-endian
// bytes (file header above).
internal static (slice<COFFSymbol>, error) readCOFFSymbols(ref FileHeader fh, io.ReadSeeker r) {
    if (fh.PointerToSymbolTable == 0) {
        return (default!, default!);
    }
    if (fh.NumberOfSymbols <= 0) {
        return (default!, default!);
    }
    var (_, err) = r.Seek((int64)fh.PointerToSymbolTable, io.SeekStart);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to seek to symbol table: %v"u8, err));
    }
    nint c = saferio.SliceCap<COFFSymbol>((uint64)fh.NumberOfSymbols);
    if (c < 0) {
        return (default!, errors.New("too many symbols; file may be corrupt"u8));
    }
    var syms = new slice<COFFSymbol>(0, () => new(), c);
    nint naux = 0;
    for (var k = (uint32)0; k < fh.NumberOfSymbols; k++) {
        ref var sym = ref heap(new COFFSymbol(), out var Ꮡsym);
        // Primary and aux records are both 18 bytes and tile the same little-endian layout, so
        // one COFFSymbol-shaped read decodes either flavor.
        err = binary.Read(r, binary.LittleEndian, Ꮡsym);
        if (err != default!) {
            return (default!, fmt.Errorf("fail to read symbol table: %v"u8, err));
        }
        if (naux == 0){
            // A primary symbol: record how many auxiliary symbols it has.
            naux = (nint)sym.NumberOfAuxSymbols;
        } else {
            // An aux symbol. Go decodes it as a COFFSymbolAuxFormat5, whose trailing blank
            // `_ [3]uint8` is SKIPPED by encoding/binary — the destination bytes stay zero. In
            // the COFFSymbol shape those three positions are Type's high byte, StorageClass and
            // NumberOfAuxSymbols; zero them to hold exactly what Go's memory holds.
            naux--;
            sym.Type &= 0xFF;
            sym.StorageClass = 0;
            sym.NumberOfAuxSymbols = 0;
        }
        syms = append(syms, sym.ΔClone());
    }
    if (naux != 0) {
        return (default!, fmt.Errorf("fail to read symbol table: %d aux symbols unread"u8, naux));
    }
    return (syms, default!);
}

// COFFSymbolReadSectionDefAux returns a blob of auxiliary information (including COMDAT info) for
// a section definition symbol — see symbol.cs for the full Go doc comment. Hand-owned: Go
// re-views 18 bytes of the successor COFFSymbol in place; the managed form transcribes that
// Go-layout image into a real COFFSymbolAuxFormat5 box (file header above).
[GoRecv] public static (ж<COFFSymbolAuxFormat5>, error) COFFSymbolReadSectionDefAux(this ref File f, nint idx) {
    ж<COFFSymbolAuxFormat5> rv = default!;
    if (idx < 0 || idx >= len(f.COFFSymbols)) {
        return (rv, fmt.Errorf("invalid symbol index"u8));
    }
    var pesym = Ꮡ(f.COFFSymbols, idx);
    UntypedInt IMAGE_SYM_CLASS_STATIC = 3;
    if ((~pesym).StorageClass != (uint8)IMAGE_SYM_CLASS_STATIC) {
        return (rv, fmt.Errorf("incorrect symbol storage class"u8));
    }
    if ((~pesym).NumberOfAuxSymbols == 0 || idx + 1 >= len(f.COFFSymbols)) {
        return (rv, fmt.Errorf("aux symbol unavailable"u8));
    }
    // Locate the successor aux symbol and decode its Go-layout image (the inverse of the
    // COFFSymbol-shaped read in readCOFFSymbols).
    var pesymn = Ꮡ(f.COFFSymbols, idx + 1);
    ref var aux = ref heap(new COFFSymbolAuxFormat5(), out var Ꮡaux);
    var name = (~pesymn).Name[..];
    aux.Size = binary.LittleEndian.Uint32(name);
    aux.NumRelocs = binary.LittleEndian.Uint16(name[4..]);
    aux.NumLineNumbers = binary.LittleEndian.Uint16(name[6..]);
    aux.Checksum = (~pesymn).Value;
    aux.SecNum = (uint16)(~pesymn).SectionNumber;
    aux.Selection = (uint8)((~pesymn).Type & 0xFF);
    aux._[0] = (uint8)((~pesymn).Type >> 8);
    aux._[1] = (~pesymn).StorageClass;
    aux._[2] = (~pesymn).NumberOfAuxSymbols;
    rv = Ꮡaux;
    return (rv, default!);
}

} // end pe_package
