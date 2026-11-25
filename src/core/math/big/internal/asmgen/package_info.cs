// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.math.big.@internal.asmgen_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Reg, RegPtr>]
[assembly: GoImplicitConv<RegPtr, Reg>]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Arch))]
[assembly: GoFieldAccess(typeof(Asm))]
[assembly: GoFieldAccess(typeof(Pipe))]
[assembly: GoFieldAccess(typeof(go.math.big.@internal.asmgen_package.Func))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("math/big/internal/asmgen/386.go", "386.cs", "AAkSAC5eog==")]
[assembly: GoPositionMap("math/big/internal/asmgen/add.go", "add.cs", "AA0U0pSCgoKCpoKCgoKCpoKSgoCAkoCAyJSigoKUlKaClIKUgoI=", "31-31:1;32-32:2;38-44:3")]
[assembly: GoPositionMap("math/big/internal/asmgen/amd64.go", "amd64.cs", "/gAvYIIACAai3IKClJSUlIKUgqaqtqailKSClKSClJSmlIKUpqKUgoKUlIKmgoKUlIKUgpQ=")]
[assembly: GoPositionMap("math/big/internal/asmgen/arch.go", "arch.cs", "AIcBvAKiABUugJaAloCWgAAIEICWgJaA")]
[assembly: GoPositionMap("math/big/internal/asmgen/arm.go", "arm.cs", "/gAqVoKmooK4ooK4ooK4ooI=")]
[assembly: GoPositionMap("math/big/internal/asmgen/arm64.go", "arm64.cs", "/gAtXKKCgpSCgoKUgriigoKUgoKClIK4ooKClIKCgpSCuKKCgpSCgoKUgg==")]
[assembly: GoPositionMap("math/big/internal/asmgen/asm.go", "asm.cs", "AB08woKCgpSCABgu4oKCgriygpSClIKUqqIAAhgACQKokqiSqJKClKiSqLKCgpSCqLKCgoKUgpSClKrCgoKUgpQAAhDygoKUgpQABxSirLKokqjCgoKUqLKowoKClKrCgoKUgqqiqqKs0oKUlIKUgpSCprqygrzCpoKCgoKUqsKqwqrCqsKCgoKUgoKUlIKUupKqwoKUqsKClIKClKrCgpSClKrCgpSqwoKUgoKUqsKClIKUqLKUtLQAFAqylPi0tLS0gpSUgpQADRqCgoKClJSCgpSUxpSCgpSCgoK2lIKUgpS02IKCgoKUgoKCtqaCgoKCmrKUtKSCgoKCuIKCgqaCgoKUggASDLKU+LS0tOqClJQADBiCgpSCgpSUxpSCgpSCgoK2lIKmgoKCtpTugoKCgoKClIKCyoKCgoKCgoKCggAKENKClLS0pOwACAiCgpSUlOrCgoKUgoLclAAICsKClJSClJS0ggAIDMKUtIKCgvqCrNKUtLS0tIK0gsqkqsKqwqiSqJI=")]
[assembly: GoPositionMap("math/big/internal/asmgen/func.go", "func.cs", "AB02soKClJKClMq4goKmgpKCgoKUlJS4goKClLSCtIKUlIKCgsiCgoKClKqiqJKCgpSCgpSCgqiSqJKCgpSCgoKCgoKClA==")]
[assembly: GoPositionMap("math/big/internal/asmgen/loong64.go", "loong64.cs", "/g==")]
[assembly: GoPositionMap("math/big/internal/asmgen/main.go", "main.cs", "ABIiABYcwoKCgoKCgoKC")]
[assembly: GoPositionMap("math/big/internal/asmgen/mips.go", "mips.cs", "/gAmToI=")]
[assembly: GoPositionMap("math/big/internal/asmgen/mips64.go", "mips64.cs", "/gAmToI=")]
[assembly: GoPositionMap("math/big/internal/asmgen/mul.go", "mul.cs", "AAsQspSCgpQACArCpoKCuIKCgoKCgoKCuAAIjAIAhQECgoKClIKClIKClIKClLS02oLCgoKCgoKCgoKClIKUkoKCgoKClKSmggAIBqKCgoKmgoKCtoKCgoKCqqKCgpKCgoKUsoKClLKCgoKClIKCgoKUpoLmooKCgriCgsKCgoKCgoKkgpKUlII=", "204-230:1;262-267:1;268-272:2;273-287:3;303-317:1")]
[assembly: GoPositionMap("math/big/internal/asmgen/pipe.go", "pipe.cs", "AC5esoLKgpSqooKUrsKClKqiqqIAAhLigpSCgpSClIKUAAoWAAgCgoK4goKkgsjcgoIAChSCgoKClIKUABAggoKUggAJFrKCgrKCpoKkgpTsgoKClIKClJSCgpSClIKC2oKokoKCgpSCsqaClOyCgoKUgoKUlMissoLsgpSUgpSClIIABhKygoKUgoKCkoIAAzAAFgKCgpSClIKCgpTugpSUlKaCuOrCgoKUgoKC3IKUlMiCooKUgpSCgoKCgpTIgoKUgpSCgoKUypKClIKClIIAAi4AFAKClIKCgoKmqqKqogAICJKCpoKClJSmgpSUgpSCgoKCgpSCgoKClIKUgpSmlLaCtrak")]
[assembly: GoPositionMap("math/big/internal/asmgen/ppc64.go", "ppc64.cs", "/g==")]
[assembly: GoPositionMap("math/big/internal/asmgen/riscv64.go", "riscv64.cs", "/g==")]
[assembly: GoPositionMap("math/big/internal/asmgen/s390x.go", "s390x.cs", "/gAtXKKCyoKCgpSmgoKUAAgGgpSklKaCgpSClIKUgoKU")]
[assembly: GoPositionMap("math/big/internal/asmgen/shift.go", "shift.cs", "ABESAA8YpoKmgoKClIKCgpSCgriCgoKCgoKCgpTKgpKCgoKC+IKCgoKUgpSC7oKSgoKCgoKCgoLy3IKCkoKClKaCgoKCgoKClJSUgoKCpoKC", "65-77:1;96-123:2")]
// </GoSourcePositionMaps>

namespace go.math.big.@internal;

using static go.math.big.@internal.asmgen_package;
using ꓸꓸꓸany = variadic<any>;
using ꓸꓸꓸnint = variadic<nint>;

[GoPackage("asmgen")]
public static partial class asmgen_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Arch {}
    public partial struct Asm {}
    public partial struct Carry {}
    public partial struct Func {}
    public partial struct Hint {}
    public partial struct Option {}
    public partial struct Pipe {}
    public partial struct Reg {}
    public partial struct RegPtr {}
    public partial struct RegsUsed {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(asmgen_package.Arch)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛArch;
[GoMethodHost(typeof(asmgen_package.Asm)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛAsm;
[GoMethodHost(typeof(asmgen_package.Func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛFunc;
[GoMethodHost(typeof(asmgen_package.Pipe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛPipe;
[GoMethodHost(typeof(asmgen_package.Reg)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛReg;
[GoMethodHost(typeof(asmgen_package.RegPtr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class asmgenᴛRegPtr;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class asmgenᴛArch {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasShiftWide(this ref Arch a) => asmgen_package.HasShiftWide⁔(ref a);
}
partial class asmgenᴛAsm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Fatalf(this ж<Asm> Ꮡa, @string format, params ꓸꓸꓸany argsʗp) => asmgen_package.Fatalf⁔(Ꮡa, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string hint(this ж<Asm> Ꮡa, Hint h) => asmgen_package.hint⁔(Ꮡa, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg ZR(this ref Asm a) => asmgen_package.ZR⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Reg tmp(this ref Asm a) => asmgen_package.tmp⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg Carry(this ref Asm a) => asmgen_package.Carry⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg AltCarry(this ref Asm a) => asmgen_package.AltCarry⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg Imm(this ref Asm a, nint x) => asmgen_package.Imm⁔(ref a, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsZero(this ref Asm a, Reg r) => asmgen_package.IsZero⁔(ref a, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg Reg(this ж<Asm> Ꮡa) => asmgen_package.Reg⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg RegHint(this ж<Asm> Ꮡa, Hint hint) => asmgen_package.RegHint⁔(Ꮡa, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Free(this ж<Asm> Ꮡa, Reg r) => asmgen_package.Free⁔(Ꮡa, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Unfree(this ж<Asm> Ꮡa, Reg r) => asmgen_package.Unfree⁔(Ꮡa, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RegsUsed RegsUsed(this ref Asm a) => asmgen_package.RegsUsed⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetRegsUsed(this ref Asm a, RegsUsed used) => asmgen_package.SetRegsUsed⁔(ref a, used);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void FreeAll(this ref Asm a) => asmgen_package.FreeAll⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Printf(this ж<Asm> Ꮡa, @string format, params ꓸꓸꓸany argsʗp) => asmgen_package.Printf⁔(Ꮡa, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Comment(this ж<Asm> Ꮡa, @string format, params ꓸꓸꓸany argsʗp) => asmgen_package.Comment⁔(Ꮡa, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void EOL(this ж<Asm> Ꮡa, @string format, params ꓸꓸꓸany argsʗp) => asmgen_package.EOL⁔(Ꮡa, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool JmpEnable(this ж<Asm> Ꮡa, Option option, @string label) => asmgen_package.JmpEnable⁔(Ꮡa, option, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref Asm a, Option option) => asmgen_package.Enabled⁔(ref a, option);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetOption(this ref Asm a, Option option, bool on) => asmgen_package.SetOption⁔(ref a, option, on);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void op3(this ж<Asm> Ꮡa, @string op, Reg src1, Reg src2, Reg dst) => asmgen_package.op3⁔(Ꮡa, op, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Mov(this ж<Asm> Ꮡa, Reg src, Reg dst) => asmgen_package.Mov⁔(Ꮡa, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddWords(this ж<Asm> Ꮡa, Reg src1, RegPtr src2, RegPtr dst) => asmgen_package.AddWords⁔(Ꮡa, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void And(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst) => asmgen_package.And⁔(Ꮡa, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Or(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst) => asmgen_package.Or⁔(Ꮡa, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Xor(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst) => asmgen_package.Xor⁔(Ꮡa, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Neg(this ж<Asm> Ꮡa, Reg src, Reg dst) => asmgen_package.Neg⁔(Ꮡa, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasRegShift(this ref Asm a) => asmgen_package.HasRegShift⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg LshReg(this ж<Asm> Ꮡa, Reg shift, Reg src) => asmgen_package.LshReg⁔(Ꮡa, shift, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Lsh(this ж<Asm> Ꮡa, Reg shift, Reg src, Reg dst) => asmgen_package.Lsh⁔(Ꮡa, shift, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void LshWide(this ж<Asm> Ꮡa, Reg shift, Reg adj, Reg src, Reg dst) => asmgen_package.LshWide⁔(Ꮡa, shift, adj, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg RshReg(this ж<Asm> Ꮡa, Reg shift, Reg src) => asmgen_package.RshReg⁔(Ꮡa, shift, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Rsh(this ж<Asm> Ꮡa, Reg shift, Reg src, Reg dst) => asmgen_package.Rsh⁔(Ꮡa, shift, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RshWide(this ж<Asm> Ꮡa, Reg shift, Reg adj, Reg src, Reg dst) => asmgen_package.RshWide⁔(Ꮡa, shift, adj, src, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SLTU(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst) => asmgen_package.SLTU⁔(Ꮡa, src1, src2, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst, Carry carry) => asmgen_package.Add⁔(Ꮡa, src1, src2, dst, carry);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Sub(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dst, Carry carry) => asmgen_package.Sub⁔(Ꮡa, src1, src2, dst, carry);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ClearCarry(this ж<Asm> Ꮡa, Carry which) => asmgen_package.ClearCarry⁔(Ꮡa, which);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SaveCarry(this ж<Asm> Ꮡa, Reg dst) => asmgen_package.SaveCarry⁔(Ꮡa, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RestoreCarry(this ж<Asm> Ꮡa, Reg src) => asmgen_package.RestoreCarry⁔(Ꮡa, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ConvertCarry(this ж<Asm> Ꮡa, Carry which, Reg dst) => asmgen_package.ConvertCarry⁔(Ꮡa, which, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SaveConvertCarry(this ж<Asm> Ꮡa, Carry which, Reg dst) => asmgen_package.SaveConvertCarry⁔(Ꮡa, which, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MulWide(this ж<Asm> Ꮡa, Reg src1, Reg src2, Reg dstlo, Reg dsthi) => asmgen_package.MulWide⁔(Ꮡa, src1, src2, dstlo, dsthi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Jmp(this ж<Asm> Ꮡa, @string label) => asmgen_package.Jmp⁔(Ꮡa, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void JmpZero(this ж<Asm> Ꮡa, Reg src, @string label) => asmgen_package.JmpZero⁔(Ꮡa, src, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void JmpNonZero(this ж<Asm> Ꮡa, Reg src, @string label) => asmgen_package.JmpNonZero⁔(Ꮡa, src, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Label(this ж<Asm> Ꮡa, @string name) => asmgen_package.Label⁔(Ꮡa, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Ret(this ж<Asm> Ꮡa) => asmgen_package.Ret⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> Func(this ж<Asm> Ꮡa, @string decl) => asmgen_package.Func⁔(Ꮡa, decl);
}
partial class asmgenᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg Arg(this ref Func f, @string name) => asmgen_package.Arg⁔(ref f, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Reg ArgHint(this ref Func f, @string name, Hint hint) => asmgen_package.ArgHint⁔(ref f, name, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RegPtr ArgPtr(this ref Func f, @string name) => asmgen_package.ArgPtr⁔(ref f, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreArg(this ref Func f, Reg src, @string name) => asmgen_package.StoreArg⁔(ref f, src, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Pipe> Pipe(this ж<Func> Ꮡf) => asmgen_package.Pipe⁔(Ꮡf);
}
partial class asmgenᴛPipe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetBackward(this ref Pipe p) => asmgen_package.SetBackward⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetUseIndexCounter(this ref Pipe p) => asmgen_package.SetUseIndexCounter⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetLabel(this ref Pipe p, @string label) => asmgen_package.SetLabel⁔(ref p, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxColumns(this ref Pipe p, nint m) => asmgen_package.SetMaxColumns⁔(ref p, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetHint(this ref Pipe p, @string name, Hint hint) => asmgen_package.SetHint⁔(ref p, name, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void LoadPtrs(this ref Pipe p, Reg n) => asmgen_package.LoadPtrs⁔(ref p, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slice<Reg>> LoadN(this ref Pipe p, nint n) => asmgen_package.LoadN⁔(ref p, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StoreN(this ref Pipe p, slice<slice<Reg>> regs) => asmgen_package.StoreN⁔(ref p, regs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void advancePtrs(this ref Pipe p, nint step) => asmgen_package.advancePtrs⁔(ref p, step);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void DropInput(this ref Pipe p, @string name) => asmgen_package.DropInput⁔(ref p, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Start(this ref Pipe p, Reg n, params ꓸꓸꓸnint factorsʗp) => asmgen_package.Start⁔(ref p, n, factorsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Restart(this ref Pipe p, Reg n, params ꓸꓸꓸnint factorsʗp) => asmgen_package.Restart⁔(ref p, n, factorsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Done(this ref Pipe p) => asmgen_package.Done⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Loop(this ref Pipe p, Action<slice<slice<Reg>>, slice<slice<Reg>>> block) => asmgen_package.Loop⁔(ref p, block);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AtUnrollStart(this ref Pipe p, Action start) => asmgen_package.AtUnrollStart⁔(ref p, start);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AtUnrollEnd(this ref Pipe p, Action end) => asmgen_package.AtUnrollEnd⁔(ref p, end);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unroll(this ref Pipe p, Reg n, nint factor, Action<slice<slice<Reg>>, slice<slice<Reg>>> block) => asmgen_package.unroll⁔(ref p, n, factor, block);
}
partial class asmgenᴛReg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsImm(this Reg r) => asmgen_package.IsImm⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsMem(this Reg r) => asmgen_package.IsMem⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Reg r) => asmgen_package.String⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Valid(this Reg r) => asmgen_package.Valid⁔(r);
}
partial class asmgenᴛRegPtr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this RegPtr r) => asmgen_package.String⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Valid(this RegPtr r) => asmgen_package.Valid⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Reg mem(this ж<RegPtr> Ꮡr, nint off) => asmgen_package.mem⁔(Ꮡr, off);
}
// </MethodForwarders>
