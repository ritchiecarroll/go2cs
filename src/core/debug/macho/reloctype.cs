// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

partial class macho_package {

partial struct RelocTypeGeneric /*num:nint*/;

//go:generate stringer -type=RelocTypeGeneric,RelocTypeX86_64,RelocTypeARM,RelocTypeARM64 -output reloctype_string.go
public static RelocTypeGeneric GENERIC_RELOC_VANILLA => 0;
public static RelocTypeGeneric GENERIC_RELOC_PAIR => 1;
public static RelocTypeGeneric GENERIC_RELOC_SECTDIFF => 2;
public static RelocTypeGeneric GENERIC_RELOC_PB_LA_PTR => 3;
public static RelocTypeGeneric GENERIC_RELOC_LOCAL_SECTDIFF => 4;
public static RelocTypeGeneric GENERIC_RELOC_TLV => 5;

public static @string GoString(this RelocTypeGeneric r) {
    return "macho."u8 + r.String();
}

partial struct RelocTypeX86_64 /*num:nint*/;

public static RelocTypeX86_64 X86_64_RELOC_UNSIGNED => 0;
public static RelocTypeX86_64 X86_64_RELOC_SIGNED => 1;
public static RelocTypeX86_64 X86_64_RELOC_BRANCH => 2;
public static RelocTypeX86_64 X86_64_RELOC_GOT_LOAD => 3;
public static RelocTypeX86_64 X86_64_RELOC_GOT => 4;
public static RelocTypeX86_64 X86_64_RELOC_SUBTRACTOR => 5;
public static RelocTypeX86_64 X86_64_RELOC_SIGNED_1 => 6;
public static RelocTypeX86_64 X86_64_RELOC_SIGNED_2 => 7;
public static RelocTypeX86_64 X86_64_RELOC_SIGNED_4 => 8;
public static RelocTypeX86_64 X86_64_RELOC_TLV => 9;

public static @string GoString(this RelocTypeX86_64 r) {
    return "macho."u8 + r.String();
}

partial struct RelocTypeARM /*num:nint*/;

public static RelocTypeARM ARM_RELOC_VANILLA => 0;
public static RelocTypeARM ARM_RELOC_PAIR => 1;
public static RelocTypeARM ARM_RELOC_SECTDIFF => 2;
public static RelocTypeARM ARM_RELOC_LOCAL_SECTDIFF => 3;
public static RelocTypeARM ARM_RELOC_PB_LA_PTR => 4;
public static RelocTypeARM ARM_RELOC_BR24 => 5;
public static RelocTypeARM ARM_THUMB_RELOC_BR22 => 6;
public static RelocTypeARM ARM_THUMB_32BIT_BRANCH => 7;
public static RelocTypeARM ARM_RELOC_HALF => 8;
public static RelocTypeARM ARM_RELOC_HALF_SECTDIFF => 9;

public static @string GoString(this RelocTypeARM r) {
    return "macho."u8 + r.String();
}

partial struct RelocTypeARM64 /*num:nint*/;

public static RelocTypeARM64 ARM64_RELOC_UNSIGNED => 0;
public static RelocTypeARM64 ARM64_RELOC_SUBTRACTOR => 1;
public static RelocTypeARM64 ARM64_RELOC_BRANCH26 => 2;
public static RelocTypeARM64 ARM64_RELOC_PAGE21 => 3;
public static RelocTypeARM64 ARM64_RELOC_PAGEOFF12 => 4;
public static RelocTypeARM64 ARM64_RELOC_GOT_LOAD_PAGE21 => 5;
public static RelocTypeARM64 ARM64_RELOC_GOT_LOAD_PAGEOFF12 => 6;
public static RelocTypeARM64 ARM64_RELOC_POINTER_TO_GOT => 7;
public static RelocTypeARM64 ARM64_RELOC_TLVP_LOAD_PAGE21 => 8;
public static RelocTypeARM64 ARM64_RELOC_TLVP_LOAD_PAGEOFF12 => 9;
public static RelocTypeARM64 ARM64_RELOC_ADDEND => 10;

public static @string GoString(this RelocTypeARM64 r) {
    return "macho."u8 + r.String();
}

} // end macho_package
