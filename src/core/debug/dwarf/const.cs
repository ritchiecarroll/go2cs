// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Constants
namespace go.debug;

partial class dwarf_package {

partial struct Attr /*num:uint32*/;

//go:generate stringer -type Attr -trimprefix=Attr
public static Attr AttrSibling => 0x01;
public static Attr AttrLocation => 0x02;
public static Attr AttrName => 0x03;
public static Attr AttrOrdering => 0x09;
public static Attr AttrByteSize => 0x0B;
public static Attr AttrBitOffset => 0x0C;
public static Attr AttrBitSize => 0x0D;
public static Attr AttrStmtList => 0x10;
public static Attr AttrLowpc => 0x11;
public static Attr AttrHighpc => 0x12;
public static Attr AttrLanguage => 0x13;
public static Attr AttrDiscr => 0x15;
public static Attr AttrDiscrValue => 0x16;
public static Attr AttrVisibility => 0x17;
public static Attr AttrImport => 0x18;
public static Attr AttrStringLength => 0x19;
public static Attr AttrCommonRef => 0x1A;
public static Attr AttrCompDir => 0x1B;
public static Attr AttrConstValue => 0x1C;
public static Attr AttrContainingType => 0x1D;
public static Attr AttrDefaultValue => 0x1E;
public static Attr AttrInline => 0x20;
public static Attr AttrIsOptional => 0x21;
public static Attr AttrLowerBound => 0x22;
public static Attr AttrProducer => 0x25;
public static Attr AttrPrototyped => 0x27;
public static Attr AttrReturnAddr => 0x2A;
public static Attr AttrStartScope => 0x2C;
public static Attr AttrStrideSize => 0x2E;
public static Attr AttrUpperBound => 0x2F;
public static Attr AttrAbstractOrigin => 0x31;
public static Attr AttrAccessibility => 0x32;
public static Attr AttrAddrClass => 0x33;
public static Attr AttrArtificial => 0x34;
public static Attr AttrBaseTypes => 0x35;
public static Attr AttrCalling => 0x36;
public static Attr AttrCount => 0x37;
public static Attr AttrDataMemberLoc => 0x38;
public static Attr AttrDeclColumn => 0x39;
public static Attr AttrDeclFile => 0x3A;
public static Attr AttrDeclLine => 0x3B;
public static Attr AttrDeclaration => 0x3C;
public static Attr AttrDiscrList => 0x3D;
public static Attr AttrEncoding => 0x3E;
public static Attr AttrExternal => 0x3F;
public static Attr AttrFrameBase => 0x40;
public static Attr AttrFriend => 0x41;
public static Attr AttrIdentifierCase => 0x42;
public static Attr AttrMacroInfo => 0x43;
public static Attr AttrNamelistItem => 0x44;
public static Attr AttrPriority => 0x45;
public static Attr AttrSegment => 0x46;
public static Attr AttrSpecification => 0x47;
public static Attr AttrStaticLink => 0x48;
public static Attr AttrType => 0x49;
public static Attr AttrUseLocation => 0x4A;
public static Attr AttrVarParam => 0x4B;
public static Attr AttrVirtuality => 0x4C;
public static Attr AttrVtableElemLoc => 0x4D;
public static Attr AttrAllocated => 0x4E;
public static Attr AttrAssociated => 0x4F;
public static Attr AttrDataLocation => 0x50;
public static Attr AttrStride => 0x51;
public static Attr AttrEntrypc => 0x52;
public static Attr AttrUseUTF8 => 0x53;
public static Attr AttrExtension => 0x54;
public static Attr AttrRanges => 0x55;
public static Attr AttrTrampoline => 0x56;
public static Attr AttrCallColumn => 0x57;
public static Attr AttrCallFile => 0x58;
public static Attr AttrCallLine => 0x59;
public static Attr AttrDescription => 0x5A;
public static Attr AttrBinaryScale => 0x5B;
public static Attr AttrDecimalScale => 0x5C;
public static Attr AttrSmall => 0x5D;
public static Attr AttrDecimalSign => 0x5E;
public static Attr AttrDigitCount => 0x5F;
public static Attr AttrPictureString => 0x60;
public static Attr AttrMutable => 0x61;
public static Attr AttrThreadsScaled => 0x62;
public static Attr AttrExplicit => 0x63;
public static Attr AttrObjectPointer => 0x64;
public static Attr AttrEndianity => 0x65;
public static Attr AttrElemental => 0x66;
public static Attr AttrPure => 0x67;
public static Attr AttrRecursive => 0x68;
public static Attr AttrSignature => 0x69;
public static Attr AttrMainSubprogram => 0x6A;
public static Attr AttrDataBitOffset => 0x6B;
public static Attr AttrConstExpr => 0x6C;
public static Attr AttrEnumClass => 0x6D;
public static Attr AttrLinkageName => 0x6E;
public static Attr AttrStringLengthBitSize => 0x6F;
public static Attr AttrStringLengthByteSize => 0x70;
public static Attr AttrRank => 0x71;
public static Attr AttrStrOffsetsBase => 0x72;
public static Attr AttrAddrBase => 0x73;
public static Attr AttrRnglistsBase => 0x74;
public static Attr AttrDwoName => 0x76;
public static Attr AttrReference => 0x77;
public static Attr AttrRvalueReference => 0x78;
public static Attr AttrMacros => 0x79;
public static Attr AttrCallAllCalls => 0x7A;
public static Attr AttrCallAllSourceCalls => 0x7B;
public static Attr AttrCallAllTailCalls => 0x7C;
public static Attr AttrCallReturnPC => 0x7D;
public static Attr AttrCallValue => 0x7E;
public static Attr AttrCallOrigin => 0x7F;
public static Attr AttrCallParameter => 0x80;
public static Attr AttrCallPC => 0x81;
public static Attr AttrCallTailCall => 0x82;
public static Attr AttrCallTarget => 0x83;
public static Attr AttrCallTargetClobbered => 0x84;
public static Attr AttrCallDataLocation => 0x85;
public static Attr AttrCallDataValue => 0x86;
public static Attr AttrNoreturn => 0x87;
public static Attr AttrAlignment => 0x88;
public static Attr AttrExportSymbols => 0x89;
public static Attr AttrDeleted => 0x8A;
public static Attr AttrDefaulted => 0x8B;
public static Attr AttrLoclistsBase => 0x8C;

public static @string GoString(this Attr a) {
    {
        var (str, ok) = _Attr_map[a, ꟷ]; if (ok) {
            return "dwarf.Attr"u8 + str;
        }
    }
    return "dwarf."u8 + a.String();
}

partial struct format /*num:uint32*/;

internal static format formAddr => 0x01;
internal static format formDwarfBlock2 => 0x03;
internal static format formDwarfBlock4 => 0x04;
internal static format formData2 => 0x05;
internal static format formData4 => 0x06;
internal static format formData8 => 0x07;
internal static format formString => 0x08;
internal static format formDwarfBlock => 0x09;
internal static format formDwarfBlock1 => 0x0A;
internal static format formData1 => 0x0B;
internal static format formFlag => 0x0C;
internal static format formSdata => 0x0D;
internal static format formStrp => 0x0E;
internal static format formUdata => 0x0F;
internal static format formRefAddr => 0x10;
internal static format formRef1 => 0x11;
internal static format formRef2 => 0x12;
internal static format formRef4 => 0x13;
internal static format formRef8 => 0x14;
internal static format formRefUdata => 0x15;
internal static format formIndirect => 0x16;
internal static format formSecOffset => 0x17;
internal static format formExprloc => 0x18;
internal static format formFlagPresent => 0x19;
internal static format formRefSig8 => 0x20;
internal static format formStrx => 0x1A;
internal static format formAddrx => 0x1B;
internal static format formRefSup4 => 0x1C;
internal static format formStrpSup => 0x1D;
internal static format formData16 => 0x1E;
internal static format formLineStrp => 0x1F;
internal static format formImplicitConst => 0x21;
internal static format formLoclistx => 0x22;
internal static format formRnglistx => 0x23;
internal static format formRefSup8 => 0x24;
internal static format formStrx1 => 0x25;
internal static format formStrx2 => 0x26;
internal static format formStrx3 => 0x27;
internal static format formStrx4 => 0x28;
internal static format formAddrx1 => 0x29;
internal static format formAddrx2 => 0x2A;
internal static format formAddrx3 => 0x2B;
internal static format formAddrx4 => 0x2C;
internal static format formGnuRefAlt => 0x1f20;
internal static format formGnuStrpAlt => 0x1f21;

partial struct Tag /*num:uint32*/;

//go:generate stringer -type Tag -trimprefix=Tag
public static Tag TagArrayType => 0x01;
public static Tag TagClassType => 0x02;
public static Tag TagEntryPoint => 0x03;
public static Tag TagEnumerationType => 0x04;
public static Tag TagFormalParameter => 0x05;
public static Tag TagImportedDeclaration => 0x08;
public static Tag TagLabel => 0x0A;
public static Tag TagLexDwarfBlock => 0x0B;
public static Tag TagMember => 0x0D;
public static Tag TagPointerType => 0x0F;
public static Tag TagReferenceType => 0x10;
public static Tag TagCompileUnit => 0x11;
public static Tag TagStringType => 0x12;
public static Tag TagStructType => 0x13;
public static Tag TagSubroutineType => 0x15;
public static Tag TagTypedef => 0x16;
public static Tag TagUnionType => 0x17;
public static Tag TagUnspecifiedParameters => 0x18;
public static Tag TagVariant => 0x19;
public static Tag TagCommonDwarfBlock => 0x1A;
public static Tag TagCommonInclusion => 0x1B;
public static Tag TagInheritance => 0x1C;
public static Tag TagInlinedSubroutine => 0x1D;
public static Tag TagModule => 0x1E;
public static Tag TagPtrToMemberType => 0x1F;
public static Tag TagSetType => 0x20;
public static Tag TagSubrangeType => 0x21;
public static Tag TagWithStmt => 0x22;
public static Tag TagAccessDeclaration => 0x23;
public static Tag TagBaseType => 0x24;
public static Tag TagCatchDwarfBlock => 0x25;
public static Tag TagConstType => 0x26;
public static Tag TagConstant => 0x27;
public static Tag TagEnumerator => 0x28;
public static Tag TagFileType => 0x29;
public static Tag TagFriend => 0x2A;
public static Tag TagNamelist => 0x2B;
public static Tag TagNamelistItem => 0x2C;
public static Tag TagPackedType => 0x2D;
public static Tag TagSubprogram => 0x2E;
public static Tag TagTemplateTypeParameter => 0x2F;
public static Tag TagTemplateValueParameter => 0x30;
public static Tag TagThrownType => 0x31;
public static Tag TagTryDwarfBlock => 0x32;
public static Tag TagVariantPart => 0x33;
public static Tag TagVariable => 0x34;
public static Tag TagVolatileType => 0x35;
public static Tag TagDwarfProcedure => 0x36;
public static Tag TagRestrictType => 0x37;
public static Tag TagInterfaceType => 0x38;
public static Tag TagNamespace => 0x39;
public static Tag TagImportedModule => 0x3A;
public static Tag TagUnspecifiedType => 0x3B;
public static Tag TagPartialUnit => 0x3C;
public static Tag TagImportedUnit => 0x3D;
public static Tag TagMutableType => 0x3E;      // Later removed from DWARF.
public static Tag TagCondition => 0x3F;
public static Tag TagSharedType => 0x40;
public static Tag TagTypeUnit => 0x41;
public static Tag TagRvalueReferenceType => 0x42;
public static Tag TagTemplateAlias => 0x43;
public static Tag TagCoarrayType => 0x44;
public static Tag TagGenericSubrange => 0x45;
public static Tag TagDynamicType => 0x46;
public static Tag TagAtomicType => 0x47;
public static Tag TagCallSite => 0x48;
public static Tag TagCallSiteParameter => 0x49;
public static Tag TagSkeletonUnit => 0x4A;
public static Tag TagImmutableType => 0x4B;

public static @string GoString(this Tag t) {
    if (t <= TagTemplateAlias) {
        return "dwarf.Tag"u8 + t.String();
    }
    return "dwarf."u8 + t.String();
}

// Location expression operators.
// The debug info encodes value locations like 8(R3)
// as a sequence of these op codes.
// This package does not implement full expressions;
// the opPlusUconst operator is expected by the type parser.
internal static UntypedInt opAddr => 0x03; /* 1 op, const addr */

internal static UntypedInt opDeref => 0x06;

internal static UntypedInt opConst1u => 0x08; /* 1 op, 1 byte const */

internal static UntypedInt opConst1s => 0x09; /*	" signed */

internal static UntypedInt opConst2u => 0x0A; /* 1 op, 2 byte const  */

internal static UntypedInt opConst2s => 0x0B; /*	" signed */

internal static UntypedInt opConst4u => 0x0C; /* 1 op, 4 byte const */

internal static UntypedInt opConst4s => 0x0D; /*	" signed */

internal static UntypedInt opConst8u => 0x0E; /* 1 op, 8 byte const */

internal static UntypedInt opConst8s => 0x0F; /*	" signed */

internal static UntypedInt opConstu => 0x10; /* 1 op, LEB128 const */

internal static UntypedInt opConsts => 0x11; /*	" signed */

internal static UntypedInt opDup => 0x12;

internal static UntypedInt opDrop => 0x13;

internal static UntypedInt opOver => 0x14;

internal static UntypedInt opPick => 0x15; /* 1 op, 1 byte stack index */

internal static UntypedInt opSwap => 0x16;

internal static UntypedInt opRot => 0x17;

internal static UntypedInt opXderef => 0x18;

internal static UntypedInt opAbs => 0x19;

internal static UntypedInt opAnd => 0x1A;

internal static UntypedInt opDiv => 0x1B;

internal static UntypedInt opMinus => 0x1C;

internal static UntypedInt opMod => 0x1D;

internal static UntypedInt opMul => 0x1E;

internal static UntypedInt opNeg => 0x1F;

internal static UntypedInt opNot => 0x20;

internal static UntypedInt opOr => 0x21;

internal static UntypedInt opPlus => 0x22;

internal static UntypedInt opPlusUconst => 0x23; /* 1 op, ULEB128 addend */

internal static UntypedInt opShl => 0x24;

internal static UntypedInt opShr => 0x25;

internal static UntypedInt opShra => 0x26;

internal static UntypedInt opXor => 0x27;

internal static UntypedInt opSkip => 0x2F; /* 1 op, signed 2-byte constant */

internal static UntypedInt opBra => 0x28; /* 1 op, signed 2-byte constant */

internal static UntypedInt opEq => 0x29;

internal static UntypedInt opGe => 0x2A;

internal static UntypedInt opGt => 0x2B;

internal static UntypedInt opLe => 0x2C;

internal static UntypedInt opLt => 0x2D;

internal static UntypedInt opNe => 0x2E;

internal static UntypedInt opLit0 => 0x30;

internal static UntypedInt opReg0 => 0x50;

internal static UntypedInt opBreg0 => 0x70; /* 1 op, signed LEB128 constant */

internal static UntypedInt opRegx => 0x90; /* 1 op, ULEB128 register */

internal static UntypedInt opFbreg => 0x91; /* 1 op, SLEB128 offset */

internal static UntypedInt opBregx => 0x92; /* 2 op, ULEB128 reg; SLEB128 off */

internal static UntypedInt opPiece => 0x93; /* 1 op, ULEB128 size of piece */

internal static UntypedInt opDerefSize => 0x94; /* 1-byte size of data retrieved */

internal static UntypedInt opXderefSize => 0x95; /* 1-byte size of data retrieved */

internal static UntypedInt opNop => 0x96;

internal static UntypedInt opPushObjAddr => 0x97;

internal static UntypedInt opCall2 => 0x98; /* 2-byte offset of DIE */

internal static UntypedInt opCall4 => 0x99; /* 4-byte offset of DIE */

internal static UntypedInt opCallRef => 0x9A; /* 4- or 8- byte offset of DIE */

internal static UntypedInt opFormTLSAddress => 0x9B;

internal static UntypedInt opCallFrameCFA => 0x9C;

internal static UntypedInt opBitPiece => 0x9D;

internal static UntypedInt opImplicitValue => 0x9E;

internal static UntypedInt opStackValue => 0x9F;

internal static UntypedInt opImplicitPointer => 0xA0;

internal static UntypedInt opAddrx => 0xA1;

internal static UntypedInt opConstx => 0xA2;

internal static UntypedInt opEntryValue => 0xA3;

internal static UntypedInt opConstType => 0xA4;

internal static UntypedInt opRegvalType => 0xA5;

internal static UntypedInt opDerefType => 0xA6;

internal static UntypedInt opXderefType => 0xA7;

internal static UntypedInt opConvert => 0xA8;

internal static UntypedInt opReinterpret => 0xA9;

/* 0xE0-0xFF reserved for user-specific */

// Basic type encodings -- the value for AttrEncoding in a TagBaseType Entry.
internal static UntypedInt encAddress => 0x01;

internal static UntypedInt encBoolean => 0x02;

internal static UntypedInt encComplexFloat => 0x03;

internal static UntypedInt encFloat => 0x04;

internal static UntypedInt encSigned => 0x05;

internal static UntypedInt encSignedChar => 0x06;

internal static UntypedInt encUnsigned => 0x07;

internal static UntypedInt encUnsignedChar => 0x08;

internal static UntypedInt encImaginaryFloat => 0x09;

internal static UntypedInt encPackedDecimal => 0x0A;

internal static UntypedInt encNumericString => 0x0B;

internal static UntypedInt encEdited => 0x0C;

internal static UntypedInt encSignedFixed => 0x0D;

internal static UntypedInt encUnsignedFixed => 0x0E;

internal static UntypedInt encDecimalFloat => 0x0F;

internal static UntypedInt encUTF => 0x10;

internal static UntypedInt encUCS => 0x11;

internal static UntypedInt encASCII => 0x12;

// Statement program standard opcode encodings.
internal static UntypedInt lnsCopy => 1;

internal static UntypedInt lnsAdvancePC => 2;

internal static UntypedInt lnsAdvanceLine => 3;

internal static UntypedInt lnsSetFile => 4;

internal static UntypedInt lnsSetColumn => 5;

internal static UntypedInt lnsNegateStmt => 6;

internal static UntypedInt lnsSetBasicBlock => 7;

internal static UntypedInt lnsConstAddPC => 8;

internal static UntypedInt lnsFixedAdvancePC => 9;

internal static UntypedInt lnsSetPrologueEnd => 10;

internal static UntypedInt lnsSetEpilogueBegin => 11;

internal static UntypedInt lnsSetISA => 12;

// Statement program extended opcode encodings.
internal static UntypedInt lneEndSequence => 1;

internal static UntypedInt lneSetAddress => 2;

internal static UntypedInt lneDefineFile => 3;

internal static UntypedInt lneSetDiscriminator => 4;

// Line table directory and file name entry formats.
// These are new in DWARF 5.
internal static UntypedInt lnctPath => 0x01;

internal static UntypedInt lnctDirectoryIndex => 0x02;

internal static UntypedInt lnctTimestamp => 0x03;

internal static UntypedInt lnctSize => 0x04;

internal static UntypedInt lnctMD5 => 0x05;

// Location list entry codes.
// These are new in DWARF 5.
internal static UntypedInt lleEndOfList => 0x00;

internal static UntypedInt lleBaseAddressx => 0x01;

internal static UntypedInt lleStartxEndx => 0x02;

internal static UntypedInt lleStartxLength => 0x03;

internal static UntypedInt lleOffsetPair => 0x04;

internal static UntypedInt lleDefaultLocation => 0x05;

internal static UntypedInt lleBaseAddress => 0x06;

internal static UntypedInt lleStartEnd => 0x07;

internal static UntypedInt lleStartLength => 0x08;

// Unit header unit type encodings.
// These are new in DWARF 5.
internal static UntypedInt utCompile => 0x01;

internal static UntypedInt utType => 0x02;

internal static UntypedInt utPartial => 0x03;

internal static UntypedInt utSkeleton => 0x04;

internal static UntypedInt utSplitCompile => 0x05;

internal static UntypedInt utSplitType => 0x06;

// Opcodes for DWARFv5 debug_rnglists section.
internal static UntypedInt rleEndOfList => 0x0;

internal static UntypedInt rleBaseAddressx => 0x1;

internal static UntypedInt rleStartxEndx => 0x2;

internal static UntypedInt rleStartxLength => 0x3;

internal static UntypedInt rleOffsetPair => 0x4;

internal static UntypedInt rleBaseAddress => 0x5;

internal static UntypedInt rleStartEnd => 0x6;

internal static UntypedInt rleStartLength => 0x7;

} // end dwarf_package
