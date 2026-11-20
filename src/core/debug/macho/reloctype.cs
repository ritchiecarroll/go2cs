// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using static macho_package;

partial class macho_package {

partial struct RelocTypeGeneric /*num:nint*/;

//go:generate stringer -type=RelocTypeGeneric,RelocTypeX86_64,RelocTypeARM,RelocTypeARM64 -output reloctype_string.go
public static RelocTypeGeneric GENERIC_RELOC_VANILLA => 0;
public static RelocTypeGeneric GENERIC_RELOC_PAIR => 1;
public static RelocTypeGeneric GENERIC_RELOC_SECTDIFF => 2;
public static RelocTypeGeneric GENERIC_RELOC_PB_LA_PTR => 3;
public static RelocTypeGeneric GENERIC_RELOC_LOCAL_SECTDIFF => 4;
public static RelocTypeGeneric GENERIC_RELOC_TLV => 5;

} partial class macho_package {

internal static @string GoStringᴍ(RelocTypeGeneric r) { return "macho."u8 + r.String(); }

} partial class macho_package {

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

} partial class macho_package {

internal static @string GoStringᴍ(RelocTypeX86_64 r) { return "macho."u8 + r.String(); }

} partial class macho_package {

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

} partial class macho_package {

internal static @string GoStringᴍ(RelocTypeARM r) { return "macho."u8 + r.String(); }

} partial class macho_package {

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

} partial class macho_package {

internal static @string GoStringᴍ(RelocTypeARM64 r) { return "macho."u8 + r.String(); }

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class machoᴛRelocTypeGeneric {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this RelocTypeGeneric r) => macho_package.GoStringᴍ(r);
}
partial class machoᴛRelocTypeX86_64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this RelocTypeX86_64 r) => macho_package.GoStringᴍ(r);
}
partial class machoᴛRelocTypeARM {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this RelocTypeARM r) => macho_package.GoStringᴍ(r);
}
partial class machoᴛRelocTypeARM64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this RelocTypeARM64 r) => macho_package.GoStringᴍ(r);
}
// </GlyphForwarders>
