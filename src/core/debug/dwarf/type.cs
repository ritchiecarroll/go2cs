// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// DWARF type information structures.
// The format is heavily biased toward C, but for simplicity
// the String methods use a pseudo-Go syntax.
namespace go.debug;

using strconv = strconv_package;
using fmt = fmt_package;

partial class dwarf_package {

// A Type conventionally represents a pointer to any of the
// specific Type structures ([CharType], [StructType], etc.).
[GoType] partial interface ΔType :
    fmt.Stringer
{
    ж<CommonType> Common();
    int64 Size();
}

// A CommonType holds fields common to multiple types.
// If a field is not known or not applicable for a given type,
// the zero value is used.
[GoType] partial struct CommonType {
    public int64 ByteSize;  // size of value of this type, in bytes
    public @string Name; // name that can be used to refer to type
}

public static ж<CommonType> Common(this ж<CommonType> Ꮡc) {
    return Ꮡc;
}

[GoRecv] public static int64 Size(this ref CommonType c) {
    return c.ByteSize;
}

// Basic types

// A BasicType holds fields common to all basic types.
//
// See the documentation for [StructField] for more info on the interpretation of
// the BitSize/BitOffset/DataBitOffset fields.
[GoType] partial struct BasicType {
    public partial ref CommonType CommonType { get; }
    public int64 BitSize;
    public int64 BitOffset;
    public int64 DataBitOffset;
}

public static ж<BasicType> Basic(this ж<BasicType> Ꮡb) {
    return Ꮡb;
}

[GoRecv] public static @string String(this ref BasicType t) {
    if (t.Name != ""u8) {
        return t.Name;
    }
    return "?"u8;
}

// A CharType represents a signed character type.
[GoType] partial struct CharType {
    public partial ref BasicType BasicType { get; }
}

// A UcharType represents an unsigned character type.
[GoType] partial struct UcharType {
    public partial ref BasicType BasicType { get; }
}

// An IntType represents a signed integer type.
[GoType] partial struct IntType {
    public partial ref BasicType BasicType { get; }
}

// A UintType represents an unsigned integer type.
[GoType] partial struct UintType {
    public partial ref BasicType BasicType { get; }
}

// A FloatType represents a floating point type.
[GoType] partial struct FloatType {
    public partial ref BasicType BasicType { get; }
}

// A ComplexType represents a complex floating point type.
[GoType] partial struct ComplexType {
    public partial ref BasicType BasicType { get; }
}

// A BoolType represents a boolean type.
[GoType] partial struct BoolType {
    public partial ref BasicType BasicType { get; }
}

// An AddrType represents a machine address type.
[GoType] partial struct AddrType {
    public partial ref BasicType BasicType { get; }
}

// An UnspecifiedType represents an implicit, unknown, ambiguous or nonexistent type.
[GoType] partial struct UnspecifiedType {
    public partial ref BasicType BasicType { get; }
}

// qualifiers

// A QualType represents a type that has the C/C++ "const", "restrict", or "volatile" qualifier.
[GoType] partial struct QualType {
    public partial ref CommonType CommonType { get; }
    public @string Qual;
    public ΔType Type;
}

[GoRecv] public static @string String(this ref QualType t) {
    return t.Qual + " "u8 + t.Type.String();
}

[GoRecv] public static int64 Size(this ref QualType t) {
    return t.Type.Size();
}

// An ArrayType represents a fixed size array type.
[GoType] partial struct ArrayType {
    public partial ref CommonType CommonType { get; }
    public ΔType Type;
    public int64 StrideBitSize; // if > 0, number of bits to hold each element
    public int64 Count; // if == -1, an incomplete array, like char x[].
}

[GoRecv] public static @string String(this ref ArrayType t) {
    return "["u8 + strconv.FormatInt(t.Count, 10) + "]"u8 + t.Type.String();
}

[GoRecv] public static int64 Size(this ref ArrayType t) {
    if (t.Count == -1) {
        return 0;
    }
    return t.Count * t.Type.Size();
}

// A VoidType represents the C void type.
[GoType] partial struct VoidType {
    public partial ref CommonType CommonType { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string voidˢ = "void"u8;

[GoRecv] public static @string String(this ref VoidType t) {
    return voidˢ;
}

// A PtrType represents a pointer type.
[GoType] partial struct PtrType {
    public partial ref CommonType CommonType { get; }
    public ΔType Type;
}

[GoRecv] public static @string String(this ref PtrType t) {
    return "*"u8 + t.Type.String();
}

// A StructType represents a struct, union, or C++ class type.
[GoType] partial struct StructType {
    public partial ref CommonType CommonType { get; }
    public @string StructName;
    public @string Kind; // "struct", "union", or "class".
    public slice<ж<StructField>> Field;
    public bool Incomplete; // if true, struct, union, class is declared but not defined
}

// A StructField represents a field in a struct, union, or C++ class type.
//
// # Bit Fields
//
// The BitSize, BitOffset, and DataBitOffset fields describe the bit
// size and offset of data members declared as bit fields in C/C++
// struct/union/class types.
//
// BitSize is the number of bits in the bit field.
//
// DataBitOffset, if non-zero, is the number of bits from the start of
// the enclosing entity (e.g. containing struct/class/union) to the
// start of the bit field. This corresponds to the DW_AT_data_bit_offset
// DWARF attribute that was introduced in DWARF 4.
//
// BitOffset, if non-zero, is the number of bits between the most
// significant bit of the storage unit holding the bit field to the
// most significant bit of the bit field. Here "storage unit" is the
// type name before the bit field (for a field "unsigned x:17", the
// storage unit is "unsigned"). BitOffset values can vary depending on
// the endianness of the system. BitOffset corresponds to the
// DW_AT_bit_offset DWARF attribute that was deprecated in DWARF 4 and
// removed in DWARF 5.
//
// At most one of DataBitOffset and BitOffset will be non-zero;
// DataBitOffset/BitOffset will only be non-zero if BitSize is
// non-zero. Whether a C compiler uses one or the other
// will depend on compiler vintage and command line options.
//
// Here is an example of C/C++ bit field use, along with what to
// expect in terms of DWARF bit offset info. Consider this code:
//
//	struct S {
//		int q;
//		int j:5;
//		int k:6;
//		int m:5;
//		int n:8;
//	} s;
//
// For the code above, one would expect to see the following for
// DW_AT_bit_offset values (using GCC 8):
//
//	       Little   |     Big
//	       Endian   |    Endian
//	                |
//	"j":     27     |     0
//	"k":     21     |     5
//	"m":     16     |     11
//	"n":     8      |     16
//
// Note that in the above the offsets are purely with respect to the
// containing storage unit for j/k/m/n -- these values won't vary based
// on the size of prior data members in the containing struct.
//
// If the compiler emits DW_AT_data_bit_offset, the expected values
// would be:
//
//	"j":     32
//	"k":     37
//	"m":     43
//	"n":     48
//
// Here the value 32 for "j" reflects the fact that the bit field is
// preceded by other data members (recall that DW_AT_data_bit_offset
// values are relative to the start of the containing struct). Hence
// DW_AT_data_bit_offset values can be quite large for structs with
// many fields.
//
// DWARF also allow for the possibility of base types that have
// non-zero bit size and bit offset, so this information is also
// captured for base types, but it is worth noting that it is not
// possible to trigger this behavior using mainstream languages.
[GoType] partial struct StructField {
    public @string Name;
    public ΔType Type;
    public int64 ByteOffset;
    public int64 ByteSize; // usually zero; use Type.Size() for normal fields
    public int64 BitOffset;
    public int64 DataBitOffset;
    public int64 BitSize; // zero if not a bit field
}

[GoRecv] public static @string String(this ref StructType t) {
    if (t.StructName != ""u8) {
        return t.Kind + " "u8 + t.StructName;
    }
    return t.Defn();
}

[GoRecv] internal static int64 bitOffset(this ref StructField f) {
    if (f.BitOffset != 0) {
        return f.BitOffset;
    }
    return f.DataBitOffset;
}

[GoRecv] public static @string Defn(this ref StructType t) {
    @string s = t.Kind;
    if (t.StructName != ""u8) {
        s += " "u8 + t.StructName;
    }
    if (t.Incomplete) {
        s += " /*incomplete*/"u8;
        return s;
    }
    s += " {"u8;
    foreach (var (i, f) in t.Field) {
        if (i > 0) {
            s += "; "u8;
        }
        s += (~f).Name + " "u8 + (~f).Type.String();
        s += "@"u8 + strconv.FormatInt((~f).ByteOffset, 10);
        if ((~f).BitSize > 0) {
            s += " : "u8 + strconv.FormatInt((~f).BitSize, 10);
            s += "@"u8 + strconv.FormatInt(f.bitOffset(), 10);
        }
    }
    s += "}"u8;
    return s;
}

// An EnumType represents an enumerated type.
// The only indication of its native integer type is its ByteSize
// (inside [CommonType]).
[GoType] partial struct EnumType {
    public partial ref CommonType CommonType { get; }
    public @string EnumName;
    public slice<ж<EnumValue>> Val;
}

// An EnumValue represents a single enumeration value.
[GoType] partial struct EnumValue {
    public @string Name;
    public int64 Val;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string enumˢ = "enum"u8;

[GoRecv] public static @string String(this ref EnumType t) {
    @string s = enumˢ;
    if (t.EnumName != ""u8) {
        s += " "u8 + t.EnumName;
    }
    s += " {"u8;
    foreach (var (i, v) in t.Val) {
        if (i > 0) {
            s += "; "u8;
        }
        s += (~v).Name + "="u8 + strconv.FormatInt((~v).Val, 10);
    }
    s += "}"u8;
    return s;
}

// A FuncType represents a function type.
[GoType] partial struct FuncType {
    public partial ref CommonType CommonType { get; }
    public ΔType ReturnType;
    public slice<ΔType> ParamType;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcˢ = "func("u8;

[GoRecv] public static @string String(this ref FuncType t) {
    @string s = funcˢ;
    foreach (var (i, tΔ1) in t.ParamType) {
        if (i > 0) {
            s += ", "u8;
        }
        s += tΔ1.String();
    }
    s += ")"u8;
    if (t.ReturnType != default!) {
        s += " "u8 + t.ReturnType.String();
    }
    return s;
}

// A DotDotDotType represents the variadic ... function parameter.
[GoType] partial struct DotDotDotType {
    public partial ref CommonType CommonType { get; }
}

[GoRecv] public static @string String(this ref DotDotDotType t) {
    return "..."u8;
}

// A TypedefType represents a named type.
[GoType] partial struct TypedefType {
    public partial ref CommonType CommonType { get; }
    public ΔType Type;
}

[GoRecv] public static @string String(this ref TypedefType t) {
    return t.Name;
}

[GoRecv] public static int64 Size(this ref TypedefType t) {
    return t.Type.Size();
}

// An UnsupportedType is a placeholder returned in situations where we
// encounter a type that isn't supported.
[GoType] partial struct UnsupportedType {
    public partial ref CommonType CommonType { get; }
    public Tag Tag;
}

[GoRecv] public static @string String(this ref UnsupportedType t) {
    if (t.Name != ""u8) {
        return t.Name;
    }
    return t.Name + "(unsupported type "u8 + t.Tag.String() + ")"u8;
}

// typeReader is used to read from either the info section or the
// types section.
[GoType] partial interface typeReader {
    void Seek(Offset _);
    (ж<Entry>, error) Next();
    typeReader clone();
    Offset offset();
    // AddressSize returns the size in bytes of addresses in the current
    // compilation unit.
    nint AddressSize();
}

// Type reads the type at off in the DWARF “info” section.
public static (ΔType, error) Type(this ж<Data> Ꮡd, Offset off) {
    ref var d = ref Ꮡd.DerefOrNull();

    return Ꮡd.readType(infoˢ, new ΔReaderжtypeReader(Ꮡd.Reader()), off, d.typeCache, nil);
}

[GoType] partial struct typeFixer {
    internal slice<ж<TypedefType>> typedefs;
    internal slice<ж<ΔType>> arraytypes;
}

[GoRecv] internal static void recordArrayType(this ref typeFixer tf, ж<ΔType> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt == nil) {
        return;
    }
    var (_, ok) = (t)._<ж<ArrayType>>(ᐧ);
    if (ok) {
        tf.arraytypes = append(tf.arraytypes, Ꮡt);
    }
}

[GoRecv] internal static void apply(this ref typeFixer tf) {
    foreach (var (_, t) in tf.typedefs) {
        t.of(TypedefType.ᏑCommonType).Common().Value.ByteSize = (~t).Type.Size();
    }
    foreach (var (_, t) in tf.arraytypes) {
        zeroArray(ref (t).DerefOrNull());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string complexFloatˢ = "complex float"u8;
internal static readonly @string complexDoubleˢ = "complex double"u8;
internal static readonly @string classˢ = "class"u8;
internal static readonly @string structˢ = "struct"u8;
internal static readonly @string unionˢ = "union"u8;
internal static readonly @string locationˢ = "location"u8;
internal static readonly @string constˢ = "const"u8;
internal static readonly @string restrictˢ = "restrict"u8;
internal static readonly @string volatileˢ = "volatile"u8;

[GoType("dyn")] internal partial interface readType_type {
    ж<BasicType> Basic();
}

// readType reads a type from r at off of name. It adds types to the
// type cache, appends new typedef types to typedefs, and computes the
// sizes of types. Callers should pass nil for typedefs; this is used
// for internal recursion.
internal static (ΔType, error) readType(this ж<Data> Ꮡd, @string name, typeReader r, Offset off, map<Offset, ΔType> typeCache, ж<typeFixer> Ꮡfixups) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();
        ref var fixups = ref Ꮡfixups.DerefOrNull();

        {
            var (t, ok) = typeCache[off, ꟷ]; if (ok) {
                return (t, default!);
            }
        }
        r.Seek(off);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var e, err) = r.Next();
        if (err != default!) {
            return (default!, err);
        }
        nint addressSize = r.AddressSize();
        if (e == nil || (~e).Offset != off) {
            return (default!, new DecodeError(name, off, "no type at offset"u8));
        }
        // If this is the root of the recursion, prepare to resolve
        // typedef sizes and perform other fixups once the recursion is
        // done. This must be done after the type graph is constructed
        // because it may need to resolve cycles in a different order than
        // readType encounters them.
        if (Ꮡfixups == nil) {
            ref var fixer = ref heap(new typeFixer(), out var Ꮡfixer);
            defer(() => {
                Ꮡfixer.Value.apply();
            }, ref ᒐ);
            Ꮡfixups = Ꮡfixer; fixups = ref Ꮡfixups.DerefOrNull();
        }
        // Parse type from Entry.
        // Must always set typeCache[off] before calling
        // d.readType recursively, to handle circular types correctly.
        ΔType typ = default!;
        nint nextDepth = 0;
        // Get next child; set err if error happens.
        var eʗ1 = e;
        ж<Entry> next() {
            if (!(~eʗ1).Children) {
                return default!;
            }
            // Only return direct children.
            // Skip over composite entries that happen to be nested
            // inside this one. Most DWARF generators wouldn't generate
            // such a thing, but clang does.
            // See golang.org/issue/6472.
            while (ᐧ) {
                var (kid, err1) = r.Next();
                if (err1 != default!) {
                    Ꮡerr.ValueSlot = err1;
                    return default!;
                }
                if (kid == nil) {
                    Ꮡerr.ValueSlot = new DecodeError(name, r.offset(), "unexpected end of DWARF entries"u8);
                    return default!;
                }
                if ((~kid).Tag == 0) {
                    if (nextDepth > 0) {
                        nextDepth--;
                        continue;
                    }
                    return default!;
                }
                if ((~kid).Children) {
                    nextDepth++;
                }
                if (nextDepth > 0) {
                    continue;
                }
                return kid;
            }
        }
        // Get Type referred to by Entry's AttrType field.
        // Set err if error happens. Not having a type is an error.
        var typeCacheʗ1 = typeCache;
        ΔType typeOf(ж<Entry> eΔ1) {
            var tval = eΔ1.Val(AttrType);
            ΔType t = default!;
            switch (tval.type()) {
            case Offset toff: {
                {
                    (t, Ꮡerr.ValueSlot) = Ꮡd.readType(name, r.clone(), toff, typeCacheʗ1, Ꮡfixups); if (Ꮡerr.ValueSlot != default!) {
                        return default!;
                    }
                }
                break;
            }
            case uint64 toff: {
                {
                    (t, Ꮡerr.ValueSlot) = Ꮡd.sigToType(toff); if (Ꮡerr.ValueSlot != default!) {
                        return default!;
                    }
                }
                break;
            }
            default: {
                var toff = tval;
                return new VoidTypeжΔType(@new<VoidType>());
            }}
            // It appears that no Type means "void".
            return t;
        }
        var exprᴛ1 = (~e).Tag;
        if (exprᴛ1 == TagArrayType) {
            var t = @new<ArrayType>();
            typ = new ArrayTypeжΔType(t);
            typeCache[off] = new ArrayTypeжΔType(t);
            {
                t.Value.Type = typeOf(e); if (err != default!) {
                    // Multi-dimensional array.  (DWARF v2 §5.4)
                    // Attributes:
                    //	AttrType:subtype [required]
                    //	AttrStrideSize: size in bits of each element of the array
                    //	AttrByteSize: size of entire array
                    // Children:
                    //	TagSubrangeType or TagEnumerationType giving one dimension.
                    //	dimensions are in left to right order.
                    goto Error;
                }
            }
            (t.Value.StrideBitSize, _) = e.Val(AttrStrideSize)._<int64>(ᐧ);
            // Accumulate dimensions,
            slice<int64> dims = default!;
            for (var kid = next(); kid != nil; kid = next()) {
                // TODO(rsc): Can also be TagEnumerationType
                // but haven't seen that in the wild yet.
                var exprᴛ2 = (~kid).Tag;
                if (exprᴛ2 == TagSubrangeType) {
                    var (count, ok) = kid.Val(AttrCount)._<int64>(ᐧ);
                    if (!ok) {
                        // Old binaries may have an upper bound instead.
                        (count, ok) = kid.Val(AttrUpperBound)._<int64>(ᐧ);
                        if (ok){
                            count++; // Length is one more than upper bound.
                        } else 
                        if (len(dims) == 0) {
                            count = -1; // As in x[].
                        }
                    }
                    dims = append(dims, count);
                }
                else if (exprᴛ2 == TagEnumerationType) {
                    err = new DecodeError(name, (~kid).Offset, "cannot handle enumeration type as array bound"u8);
                    goto Error;
                }

            }
            if (len(dims) == 0) {
                // LLVM generates this for x[].
                dims = new int64[]{-1}.slice();
            }
            t.Value.Count = dims[0];
            for (nint i = len(dims) - 1; i >= 1; i--) {
                t.Value.Type = new ArrayTypeжΔType(Ꮡ(new ArrayType(Type: (~t).Type, Count: dims[i])));
            }
        }
        else if (exprᴛ1 == TagBaseType) {
            var (nameΔ2, _) = e.Val(AttrName)._<@string>(ᐧ);
            var (enc, ok) = e.Val(AttrEncoding)._<int64>(ᐧ);
            if (!ok) {
                // Basic type.  (DWARF v2 §5.1)
                // Attributes:
                //	AttrName: name of base type in programming language of the compilation unit [required]
                //	AttrEncoding: encoding value for type (encFloat etc) [required]
                //	AttrByteSize: size of type in bytes [required]
                //	AttrBitOffset: bit offset of value within containing storage unit
                //	AttrDataBitOffset: bit offset of value within containing storage unit
                //	AttrBitSize: size in bits
                //
                // For most languages BitOffset/DataBitOffset/BitSize will not be present
                // for base types.
                err = new DecodeError(nameΔ2, (~e).Offset, "missing encoding attribute for "u8 + nameΔ2);
                goto Error;
            }
            var exprᴛ3 = enc;
            if (exprᴛ3 == encAddress) {
                typ = new AddrTypeжΔType(@new<AddrType>());
            }
            else if (exprᴛ3 == encBoolean) {
                typ = new BoolTypeжΔType(@new<BoolType>());
            }
            else if (exprᴛ3 == encComplexFloat) {
                typ = new ComplexTypeжΔType(@new<ComplexType>());
                if (nameΔ2 == "complex"u8) {
                    // clang writes out 'complex' instead of 'complex float' or 'complex double'.
                    // clang also writes out a byte size that we can use to distinguish.
                    // See issue 8694.
                    {
                        var (byteSize, _) = e.Val(AttrByteSize)._<int64>(ᐧ);
                        switch (byteSize) {
                        case 8: {
                            nameΔ2 = complexFloatˢ;
                            break;
                        }
                        case 16: {
                            nameΔ2 = complexDoubleˢ;
                            break;
                        }}
                    }

                }
            }
            else if (exprᴛ3 == encFloat) {
                typ = new FloatTypeжΔType(@new<FloatType>());
            }
            else if (exprᴛ3 == encSigned) {
                typ = new IntTypeжΔType(@new<IntType>());
            }
            else if (exprᴛ3 == encUnsigned) {
                typ = new UintTypeжΔType(@new<UintType>());
            }
            else if (exprᴛ3 == encSignedChar) {
                typ = new CharTypeжΔType(@new<CharType>());
            }
            else if (exprᴛ3 == encUnsignedChar) {
                typ = new UcharTypeжΔType(@new<UcharType>());
            }
            else { /* default: */
                err = new DecodeError(nameΔ2, (~e).Offset, "unrecognized encoding attribute value"u8);
                goto Error;
            }

            typeCache[off] = typ;
            var t = typ._<readType_type>().Basic();
            t.Value.Name = nameΔ2;
            (t.Value.BitSize, _) = e.Val(AttrBitSize)._<int64>(ᐧ);
            var haveBitOffset = false;
            var haveDataBitOffset = false;
            (t.Value.BitOffset, haveBitOffset) = e.Val(AttrBitOffset)._<int64>(ᐧ);
            (t.Value.DataBitOffset, haveDataBitOffset) = e.Val(AttrDataBitOffset)._<int64>(ᐧ);
            if (haveBitOffset && haveDataBitOffset) {
                err = new DecodeError(nameΔ2, (~e).Offset, "duplicate bit offset attributes"u8);
                goto Error;
            }
        }
        else if (exprᴛ1 == TagClassType || exprᴛ1 == TagStructType || exprᴛ1 == TagUnionType) {
            var t = @new<StructType>();
            typ = new StructTypeжΔType(t);
            typeCache[off] = new StructTypeжΔType(t);
            var exprᴛ4 = (~e).Tag;
            if (exprᴛ4 == TagClassType) {
                t.Value.Kind = classˢ;
            }
            else if (exprᴛ4 == TagStructType) {
                t.Value.Kind = structˢ;
            }
            else if (exprᴛ4 == TagUnionType) {
                t.Value.Kind = unionˢ;
            }

            (t.Value.StructName, _) = e.Val(AttrName)._<@string>(ᐧ);
            t.Value.Incomplete = e.Val(AttrDeclaration) != default!;
            t.Value.Field = new slice<ж<StructField>>(0, 8);
            ж<ΔType> lastFieldType = default!;
            int64 lastFieldBitSize = default!;
            int64 lastFieldByteOffset = default!;
            for (var kid = next(); kid != nil; kid = next()) {
                if ((~kid).Tag != TagMember) {
                    continue;
                }
                var f = @new<StructField>();
                {
                    f.Value.Type = typeOf(kid); if (err != default!) {
                        goto Error;
                    }
                }
                switch (kid.Val(AttrDataMemberLoc).type()) {
                case slice<byte> loc: {
                    var b = makeBuf(Ꮡd, // TODO: Should have original compilation
 // unit here, not unknownFormat.
 new unknownFormat(nil), locationˢ, 0, loc);
                    if (b.uint8() != opPlusUconst) {
                        err = new DecodeError(name, (~kid).Offset, "unexpected opcode"u8);
                        goto Error;
                    }
                    f.Value.ByteOffset = (int64)b.@uint();
                    if (b.err != default!) {
                        err = b.err;
                        goto Error;
                    }
                    break;
                }
                case int64 loc: {
                    f.Value.ByteOffset = loc;
                    break;
                }}
                (f.Value.Name, _) = kid.Val(AttrName)._<@string>(ᐧ);
                (f.Value.ByteSize, _) = kid.Val(AttrByteSize)._<int64>(ᐧ);
                var haveBitOffset = false;
                var haveDataBitOffset = false;
                (f.Value.BitOffset, haveBitOffset) = kid.Val(AttrBitOffset)._<int64>(ᐧ);
                (f.Value.DataBitOffset, haveDataBitOffset) = kid.Val(AttrDataBitOffset)._<int64>(ᐧ);
                if (haveBitOffset && haveDataBitOffset) {
                    err = new DecodeError(name, (~e).Offset, "duplicate bit offset attributes"u8);
                    goto Error;
                }
                (f.Value.BitSize, _) = kid.Val(AttrBitSize)._<int64>(ᐧ);
                t.Value.Field = append((~t).Field, f);
                if (lastFieldBitSize == 0 && lastFieldByteOffset == (~f).ByteOffset && (~t).Kind != "union"u8) {
                    // Last field was zero width. Fix array length.
                    // (DWARF writes out 0-length arrays as if they were 1-length arrays.)
                    fixups.recordArrayType(lastFieldType);
                }
                lastFieldType = f.of(StructField.ᏑType);
                lastFieldByteOffset = f.Value.ByteOffset;
                lastFieldBitSize = f.Value.BitSize;
            }
            if ((~t).Kind != "union"u8) {
                var (b, ok) = e.Val(AttrByteSize)._<int64>(ᐧ);
                if (ok && b == lastFieldByteOffset) {
                    // Final field must be zero width. Fix array length.
                    fixups.recordArrayType(lastFieldType);
                }
            }
        }
        else if (exprᴛ1 == TagConstType || exprᴛ1 == TagVolatileType || exprᴛ1 == TagRestrictType) {
            var t = @new<QualType>();
            typ = new QualTypeжΔType(t);
            typeCache[off] = new QualTypeжΔType(t);
            {
                t.Value.Type = typeOf(e); if (err != default!) {
                    // Type modifier (DWARF v2 §5.2)
                    // Attributes:
                    //	AttrType: subtype
                    goto Error;
                }
            }
            var exprᴛ5 = (~e).Tag;
            if (exprᴛ5 == TagConstType) {
                t.Value.Qual = constˢ;
            }
            else if (exprᴛ5 == TagRestrictType) {
                t.Value.Qual = restrictˢ;
            }
            else if (exprᴛ5 == TagVolatileType) {
                t.Value.Qual = volatileˢ;
            }

        }
        else if (exprᴛ1 == TagEnumerationType) {
            var t = @new<EnumType>();
            typ = new EnumTypeжΔType(t);
            typeCache[off] = new EnumTypeжΔType(t);
            (t.Value.EnumName, _) = e.Val(AttrName)._<@string>(ᐧ);
            t.Value.Val = new slice<ж<EnumValue>>(0, 8);
            for (var kid = next(); kid != nil; kid = next()) {
                if ((~kid).Tag == TagEnumerator) {
                    var f = @new<EnumValue>();
                    (f.Value.Name, _) = kid.Val(AttrName)._<@string>(ᐧ);
                    (f.Value.Val, _) = kid.Val(AttrConstValue)._<int64>(ᐧ);
                    nint n = len((~t).Val);
                    if (n >= cap((~t).Val)) {
                        var val = new slice<ж<EnumValue>>(n, n * 2);
                        copy(val, (~t).Val);
                        t.Value.Val = val;
                    }
                    t.Value.Val = (~t).Val.slice(0, n + 1);
                    t.Value.Val[n] = f;
                }
            }
        }
        else if (exprᴛ1 == TagPointerType) {
            do {
                var t = @new<PtrType>();
                typ = new PtrTypeжΔType(t);
                typeCache[off] = new PtrTypeжΔType(t);
                if (e.Val(AttrType) == default!) {
                    // Type modifier (DWARF v2 §5.2)
                    // Attributes:
                    //	AttrType: subtype [not required!  void* has no AttrType]
                    //	AttrAddrClass: address class [ignored]
                    t.Value.Type = new VoidTypeжΔType(Ꮡ(new VoidType(nil)));
                    break;
                }
                t.Value.Type = typeOf(e);
            } while (false);
        }
        else if (exprᴛ1 == TagSubroutineType) {
            var t = @new<FuncType>();
            typ = new FuncTypeжΔType(t);
            typeCache[off] = new FuncTypeжΔType(t);
            {
                t.Value.ReturnType = typeOf(e); if (err != default!) {
                    // Subroutine type.  (DWARF v2 §5.7)
                    // Attributes:
                    //	AttrType: type of return value if any
                    //	AttrName: possible name of type [ignored]
                    //	AttrPrototyped: whether used ANSI C prototype [ignored]
                    // Children:
                    //	TagFormalParameter: typed parameter
                    //		AttrType: type of parameter
                    //	TagUnspecifiedParameter: final ...
                    goto Error;
                }
            }
            t.Value.ParamType = new slice<ΔType>(0, 8);
            for (var kid = next(); kid != nil; kid = next()) {
                ΔType tkidΔ1 = default!;
                var exprᴛ6 = (~kid).Tag;
                if (exprᴛ6 == TagFormalParameter) {
                    {
                        tkidΔ1 = typeOf(kid); if (err != default!) {
                            goto Error;
                        }
                    }
                }
                else if (exprᴛ6 == TagUnspecifiedParameters) {
                    tkidΔ1 = new DotDotDotTypeжΔType(Ꮡ(new DotDotDotType(nil)));
                }
                else { /* default: */
                    continue;
                }

                t.Value.ParamType = append((~t).ParamType, tkidΔ1);
            }
        }
        else if (exprᴛ1 == TagTypedef) {
            var t = @new<TypedefType>();
            typ = new TypedefTypeжΔType(t);
            typeCache[off] = new TypedefTypeжΔType(t);
            (t.Value.Name, _) = e.Val(AttrName)._<@string>(ᐧ);
            t.Value.Type = typeOf(e);
        }
        else if (exprᴛ1 == TagUnspecifiedType) {
            var t = @new<UnspecifiedType>();
            typ = new UnspecifiedTypeжΔType(t);
            typeCache[off] = new UnspecifiedTypeжΔType(t);
            (t.Value.Name, _) = e.Val(AttrName)._<@string>(ᐧ);
        }
        else { /* default: */
            var t = @new<UnsupportedType>();
            typ = new UnsupportedTypeжΔType(t);
            typeCache[off] = new UnsupportedTypeжΔType(t);
            t.Value.Tag = e.Value.Tag;
            (t.Value.Name, _) = e.Val(AttrName)._<@string>(ᐧ);
        }

        // Typedef (DWARF v2 §5.3)
        // Attributes:
        //	AttrName: name [required]
        //	AttrType: type definition [required]
        // Unspecified type (DWARF v3 §5.2)
        // Attributes:
        //	AttrName: name
        // This is some other type DIE that we're currently not
        // equipped to handle. Return an abstract "unsupported type"
        // object in such cases.
        if (err != default!) {
            goto Error;
        }
        {
            var (b, ok) = e.Val(AttrByteSize)._<int64>(ᐧ);
            if (!ok) {
                b = -1;
                switch (typ.type()) {
                case ж<TypedefType> t: {
                    fixups.typedefs = append(fixups.typedefs, // Record that we need to resolve this
 // type's size once the type graph is
 // constructed.
 t);
                    break;
                }
                case ж<PtrType> t: {
                    b = (int64)addressSize;
                    break;
                }}
            }
            typ.Common().Value.ByteSize = b;
        }
        return (typ, default!);
Error:
        delete(typeCache, // If the parse fails, take the type out of the cache
 // so that the next call with this offset doesn't hit
 // the cache and return success.
 off);
        return (default!, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void zeroArray(ref ΔType t) {
    var at = (t)._<ж<ArrayType>>();
    if ((~at).Type.Size() == 0) {
        return;
    }
    // Make a copy to avoid invalidating typeCache.
    ref var tt = ref heap<ArrayType>(out var Ꮡtt);
    tt = at.Value;
    tt.Count = 0;
    t = new ArrayTypeжΔType(Ꮡtt);
}

} // end dwarf_package
