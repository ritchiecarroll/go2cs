// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using @unsafe = unsafe_package;
using @internal;
using encoding;

partial class pe_package {

public static UntypedInt COFFSymbolSize => 18;

// COFFSymbol represents single COFF symbol table record.
partial struct COFFSymbol {
    public array<uint8> Name = new(8);
    public uint32 Value;
    public int16 SectionNumber;
    public uint16 Type;
    public uint8 StorageClass;
    public uint8 NumberOfAuxSymbols;
}

// go2cs generated this placeholder — func readCOFFSymbols is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// isSymNameOffset checks symbol name if it is encoded as offset into string table.
internal static (bool, uint32) isSymNameOffset(/*[8]*/ array<byte> name) {
    name = name.Clone();

    if (name[0] == 0 && name[1] == 0 && name[2] == 0 && name[3] == 0) {
        var offset = binary.LittleEndian.Uint32(name[4..]);
        if (offset == 0) {
            // symbol has no name
            return (false, 0);
        }
        return (true, offset);
    }
    return (false, 0);
}

// FullName finds real name of symbol sym. Normally name is stored
// in sym.Name, but if it is longer then 8 characters, it is stored
// in COFF string table st instead.
public static (@string, error) FullName(this ref COFFSymbol sym, StringTable st) {
    {
        var (ok, offset) = isSymNameOffset(sym.Name); if (ok) {
            return st.String(offset);
        }
    }
    return (cstring(sym.Name[..]), default!);
}

internal static (slice<ж<Symbol>>, error) removeAuxSymbols(slice<COFFSymbol> allsyms, StringTable st) {
    if (len(allsyms) == 0) {
        return (default!, default!);
    }
    var syms = new slice<ж<Symbol>>(0);
    var aux = (uint8)0;
    foreach (var (_, vᴛ1) in allsyms) {
        var sym = vᴛ1.ΔClone();

        if (aux > 0) {
            aux--;
            continue;
        }
        ref var name = ref heap<@string>(out var Ꮡname);
        (name, var err) = sym.FullName(st);
        if (err != default!) {
            return (default!, err);
        }
        aux = sym.NumberOfAuxSymbols;
        var s = Ꮡ(new Symbol(
            Name: name,
            Value: sym.Value,
            SectionNumber: sym.SectionNumber,
            Type: sym.Type,
            StorageClass: sym.StorageClass
        ));
        syms = append(syms, s);
    }
    return (syms, default!);
}

// Symbol is similar to [COFFSymbol] with Name field replaced
// by Go string. Symbol also does not have NumberOfAuxSymbols.
partial struct Symbol {
    public @string Name;
    public uint32 Value;
    public int16 SectionNumber;
    public uint16 Type;
    public uint8 StorageClass;
}

// COFFSymbolAuxFormat5 describes the expected form of an aux symbol
// attached to a section definition symbol. The PE format defines a
// number of different aux symbol formats: format 1 for function
// definitions, format 2 for .be and .ef symbols, and so on. Format 5
// holds extra info associated with a section definition, including
// number of relocations + line numbers, as well as COMDAT info. See
// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#auxiliary-format-5-section-definitions
// for more on what's going on here.
partial struct COFFSymbolAuxFormat5 {
    public uint32 Size;
    public uint16 NumRelocs;
    public uint16 NumLineNumbers;
    public uint32 Checksum;
    public uint16 SecNum;
    public uint8 Selection;
    internal array<uint8> _ = new(3); // padding
}

// These constants make up the possible values for the 'Selection'
// field in an AuxFormat5.
public static UntypedInt IMAGE_COMDAT_SELECT_NODUPLICATES => 1;

public static UntypedInt IMAGE_COMDAT_SELECT_ANY => 2;

public static UntypedInt IMAGE_COMDAT_SELECT_SAME_SIZE => 3;

public static UntypedInt IMAGE_COMDAT_SELECT_EXACT_MATCH => 4;

public static UntypedInt IMAGE_COMDAT_SELECT_ASSOCIATIVE => 5;

public static UntypedInt IMAGE_COMDAT_SELECT_LARGEST => 6;

// go2cs generated this placeholder — func COFFSymbolReadSectionDefAux is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end pe_package
