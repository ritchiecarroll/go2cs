// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

using fmt = fmt_package;
using strings = strings_package;

partial class asmgen_package {

// Note: Exported fields and methods are expected to be used
// by function generators (like the ones in add.go and so on).
// Unexported fields and methods should not be.

// An Arch defines how to generate assembly for a specific architecture.
partial struct Arch {
    public @string Name;       // name of architecture
    public @string Build;      // build tag
    public nint WordBits;      // length of word in bits (32 or 64)
    public nint WordBytes;     // length of word in bytes (4 or 8)
    public bool CarrySafeLoop; // whether loops preserve carry flag across iterations
    // Registers.
    internal slice<@string> regs; // usable general registers, in allocation order
    internal @string reg0;        // dedicated zero register
    internal @string regCarry;    // dedicated carry register, for systems with no hardware carry bits
    internal @string regAltCarry; // dedicated secondary carry register, for systems with no hardware carry bits
    internal @string regTmp;      // dedicated temporary register
    // regShift indicates that the architecture supports
    // using REG1>>REG2 and REG1<<REG2 as the first source
    // operand in an arithmetic instruction. (32-bit ARM does this.)
    internal bool regShift;
    // setup is called to emit any per-architecture function prologue,
    // immediately after the TEXT line has been emitted.
    // If setup is nil, it is taken to be a no-op.
    internal Action<ж<ΔFunc>> setup;
    // hint returns the register to use for a given hint.
    // Returning an empty string indicates no preference.
    // If hint is nil, it is considered to return an empty string.
    internal Func<ж<Asm>, Hint, @string> hint;
    // op3 reports whether the named opcode accepts 3 operands
    // (true on most instructions on most systems, but not true of x86 instructions).
    // The assembler unconditionally turns op x,z,z into op x,z.
    // If op3 returns false, then the assembler will turn op x,y,z into mov y,z; op x,z.
    // If op3 is nil, then all opcodes are assumed to accept 3 operands.
    internal Func<@string, bool> op3;
    // memOK indicates that arithmetic instructions can use memory references (like on x86)
    internal bool memOK;
    // maxColumns is the default maximum number of vector columns
    // to process in a single [Pipe.Loop] block.
    // 0 means unlimited.
    // [Pipe.SetMaxColumns] overrides this.
    internal nint maxColumns;
    // Instruction names.
    internal @string mov;   // move (word-sized)
    internal @string add;   // add with no carry involvement
    internal @string adds;  // add, setting but not using carry
    internal @string adc;   // add, using but not setting carry
    internal @string adcs;  // add, setting and using carry
    internal @string sub;   // sub with no carry involvement
    internal @string subs;  // sub, setting but not using carry
    internal @string sbc;   // sub, using but not setting carry
    internal @string sbcs;  // sub, setting and using carry
    internal @string mul;   // multiply
    internal @string mulhi; // multiply producing high bits
    internal @string lsh;   // left shift
    internal @string lshd;  // double-width left shift
    internal @string rsh;   // right shift
    internal @string rshd;  // double-width right shift
    internal @string and;   // bitwise and
    internal @string or;    // bitwise or
    internal @string xor;   // bitwise xor
    internal @string neg;   // negate
    internal @string rsb;   // reverse subtract
    internal @string sltu;  // set less-than unsigned (dst = src2 < src1), for carry-less systems
    internal @string sgtu;  // set greater-than unsigned (dst = src2 > src1), for carry-less systems
    internal @string lea;   // load effective address
    // addF and subF implement a.Add and a.Sub
    // on systems where the situation is more complicated than
    // the six basic instructions (add, adds, adcs, sub, subs, sbcs).
    // They return a boolean indicating whether the operation was handled.
    internal Func<ж<Asm>, ΔReg, ΔReg, ΔReg, ΔCarry, bool> addF;
    internal Func<ж<Asm>, ΔReg, ΔReg, ΔReg, ΔCarry, bool> subF;
    // mulF and mulWideF implement Mul and MulWide.
    // They call Fatalf if the operation is unsupported.
    // An architecture can set the mul field instead of mulF.
    // mulWide is optional, but otherwise mulhi should be set.
    internal Action<ж<Asm>, ΔReg, ΔReg, ΔReg, ΔReg> mulWideF;
    // addWords is a printf format taking src1, src2, dst
    // and sets dst = WordBytes*src1+src2.
    // It may modify the carry flag.
    internal @string addWords;
    // subCarryIsBorrow is true when the actual processor carry bit used in subtraction
    // is really a “borrow” bit, meaning 1 means borrow and 0 means no borrow.
    // In contrast, most systems (except x86) use a carry bit with the opposite
    // meaning: 0 means a borrow happened, and 1 means it didn't.
    internal bool subCarryIsBorrow;
    // Jump instruction printf formats.
    // jmpZero and jmpNonZero are printf formats taking src, label
    // and jump to label if src is zero / non-zero.
    internal @string jmpZero;
    internal @string jmpNonZero;
    // loopTop is a printf format taking src, label that should
    // jump to label if src is zero, or else set up for a loop.
    // If loopTop is not set, jmpZero is used.
    internal @string loopTop;
    // loopBottom is a printf format taking dst, label that should
    // decrement dst and then jump to label if src is non-zero.
    // If loopBottom is not set, a subtraction is used followed by
    // use of jmpNonZero.
    internal @string loopBottom;
    // loopBottomNeg is like loopBottom but used in negative-index
    // loops, which only happen memIndex is also set (only on 386).
    // It increments dst instead of decrementing it.
    internal @string loopBottomNeg;
    // Indexed memory access.
    // If set, memIndex returns a memory reference for a mov instruction
    // addressing off(ptr)(ix*WordBytes).
    // Using memIndex costs an extra register but allows the end-of-loop
    // to do a single increment/decrement instead of advancing two or three pointers.
    // This is particularly important on 386.
    internal Func<ж<Asm>, nint, ΔReg, RegPtr, ΔReg> memIndex;
    // Incrementing/decrementing memory access.
    // loadIncN loads memory at ptr into regs, incrementing ptr by WordBytes after each reg.
    // loadDecN loads memory at ptr into regs, decrementing ptr by WordBytes before each reg.
    // storeIncN and storeDecN are the same, but storing from regs instead of loading into regs.
    // If missing, the assembler accesses memory and advances pointers using separate instructions.
    internal Action<ж<Asm>, RegPtr, slice<ΔReg>> loadIncN;
    internal Action<ж<Asm>, RegPtr, slice<ΔReg>> loadDecN;
    internal Action<ж<Asm>, RegPtr, slice<ΔReg>> storeIncN;
    internal Action<ж<Asm>, RegPtr, slice<ΔReg>> storeDecN;
    // options is a map from optional CPU features to functions that test for them.
    // The test function should jump to label if the feature is available.
    internal map<Option, Action<ж<Asm>, @string>> options;
}

// HasShiftWide reports whether the Arch has working LshWide/RshWide instructions.
// If not, calling them will panic.
public static bool HasShiftWide(this ref Arch a) {
    return a.lshd != ""u8;
}

// A Hint is a hint about what a register will be used for,
// so that an appropriate one can be selected.
partial struct Hint /*num:nuint*/;

public static Hint HintNone => /* iota */ 0;
public static Hint HintShiftCount => 1; // shift count (CX on x86)
public static Hint HintMulSrc => 2;     // mul source operand (AX on x86)
public static Hint HintMulHi => 3;      // wide mul high output (DX on x86)
public static Hint HintMemOK => 4;      // a memory reference is okay
public static Hint HintCarry => 5;      // carry flag
public static Hint HintAltCarry => 6;   // secondary carry flag

// A Reg is an allocated register or other assembly operand.
// (For example, a constant might have name "$123"
// and a memory reference might have name "0(R8)".)
partial struct ΔReg {
    internal @string name;
}

// IsImm reports whether r is an immediate value.
public static bool IsImm(this ΔReg r) {
    return strings.HasPrefix(r.name, "$"u8);
}

// IsMem reports whether r is a memory value.
public static bool IsMem(this ΔReg r) {
    return strings.HasSuffix(r.name, ")"u8);
}

// String returns the assembly syntax for r.
public static @string String(this ΔReg r) {
    return r.name;
}

// Valid reports whether is valid, meaning r is not the zero value of Reg (a register with no name).
public static bool Valid(this ΔReg r) {
    return r.name != ""u8;
}

// A RegPtr is like a Reg but expected to hold a pointer.
// The separate Go type helps keeps pointers and scalars separate and avoid mistakes;
// it is okay to convert to Reg as needed to use specific routines.
partial struct RegPtr {
    internal @string name;
}

// String returns the assembly syntax for r.
public static @string String(this RegPtr r) {
    return r.name;
}

// Valid reports whether is valid, meaning r is not the zero value of RegPtr (a register with no name).
public static bool Valid(this RegPtr r) {
    return r.name != ""u8;
}

// mem returns a memory reference to off bytes from the pointer r.
internal static ΔReg mem(this ж<RegPtr> Ꮡr, nint off) {
    return new ΔReg(fmt.Sprintf("%d(%s)"u8, off, Ꮡr.OrTypedNil()));
}

// A Carry is a flag field explaining how an instruction sets and uses the carry flags.
// Different operations expect different sets of bits.
// Add and Sub expect: UseCarry or 0, SetCarry, KeepCarry, or SmashCarry; and AltCarry or 0.
// ClearCarry, SaveCarry, and ConvertCarry expect: AddCarry or SubCarry; and AltCarry or 0.
partial struct ΔCarry /*num:nuint*/;

public static ΔCarry SetCarry => /* 1 << iota */ 1; // sets carry
public static ΔCarry UseCarry => 2;                 // uses carry
public static ΔCarry KeepCarry => 4;                // must preserve carry
public static ΔCarry SmashCarry => 8;               // can modify carry or not, whatever is easiest

public static ΔCarry ΔAltCarry => 16; // use the secondary carry flag
public static ΔCarry AddCarry => 32;  // use add carry flag semantics (for ClearCarry, ConvertCarry)
public static ΔCarry SubCarry => 64;  // use sub carry flag semantics (for ClearCarry, ConvertCarry)

// An Option denotes an optional CPU feature that can be tested at runtime.
partial struct Option /*num:nint*/;

internal static Option _ᴛ1ʗ => /* iota */ 0;

// OptionAltCarry checks whether there is an add instruction
// that uses a secondary carry flag, so that two different sums
// can be accumulated in parallel with independent carry flags.
// Some architectures (MIPS, Loong64, RISC-V) provide this
// functionality natively, indicated by asm.Carry().Valid() being true.
public static Option OptionAltCarry => 1;

} // end asmgen_package
