// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using iter = iter_package;

partial class asmgen_package {

// Note: Exported fields and methods are expected to be used
// by function generators (like the ones in add.go and so on).
// Unexported fields and methods should not be.

// A Func represents a single assembly function.
partial struct ΔFunc {
    public @string Name;
    public ж<Asm> Asm;
    internal slice<@string> inputs;   // name of input slices (not beginning with z)
    internal slice<@string> outputs;  // names of output slices (beginning with z)
    internal map<@string, nint> args; // offsets of args, results on stack
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcˢ = "func "u8;

// Func starts a new function in the assembly output.
public static ж<ΔFunc> Func(this ж<Asm> Ꮡa, @string decl) {
    ref var a = ref Ꮡa.DerefOrNull();

    var (d, ok) = strings.CutPrefix(decl, funcˢ);
    if (!ok) {
        Ꮡa.Fatalf("func decl does not begin with 'func '"u8);
    }
    ref var name = ref heap<@string>(out var Ꮡname);
    (name, d, ok) = strings.Cut(d, "("u8);
    if (!ok) {
        Ꮡa.Fatalf("func decl does not have func arg list"u8);
    }
    var f = Ꮡ(new ΔFunc(
        Name: name,
        Asm: Ꮡa,
        args: new map<@string, nint>()
    ));
    a.FreeAll();

    // Parse argument names and types. Quick and dirty.
    // Convert (args) (results) into args, results.
    d = strings.ReplaceAll(d, ") ("u8, ", "u8);
    d = strings.TrimSuffix(d, ")"u8);
    var args = strings.Split(d, ","u8);

    // Assign implicit types to all arguments (x, y int -> x int, y int).
    @string typ = ""u8;
    foreach (var (i, vᴛ1) in range<nint, @string>(slices.Backward<slice<@string>, @string>(args).Invoke)) {
        var arg = vᴛ1;
        arg = strings.TrimSpace(arg);
        if (!strings.Contains(arg, " "u8)) {
            if (typ == ""u8) {
                Ꮡa.Fatalf("missing argument type"u8);
            }
            arg += " "u8 + typ;
        } else {
            (_, typ, _) = strings.Cut(arg, " "u8);
        }
        args[i] = arg;
    }

    // Record mapping from names to offsets.
    nint off = 0;
    foreach (var (_, arg) in args) {
        var (nameΔ1, typΔ1, _) = strings.Cut(arg, " "u8);
        var exprᴛ1 = typΔ1;
        switch (ᐧ) {
        default: {
            Ꮡa.Fatalf("unknown type %s"u8, typΔ1);
            break;
        }
        case {} when exprᴛ1 == "Word"u8 || exprᴛ1 == "uint"u8 || exprᴛ1 == "int"u8: {
            f.args[nameΔ1] = off;
            off += a.Arch.WordBytes;
            break;
        }
        case {} when exprᴛ1 == "[]Word"u8: {
            if (strings.HasPrefix(nameΔ1, "z"u8)) {
                f.outputs = append(f.outputs, nameΔ1);
            } else {
                f.inputs = append(f.inputs, nameΔ1);
            }
            f.args[nameΔ1 + "_base"u8] = off;
            f.args[nameΔ1 + "_len"u8] = off + a.Arch.WordBytes;
            f.args[nameΔ1 + "_cap"u8] = off + 2 * a.Arch.WordBytes;
            off += 3 * a.Arch.WordBytes;
            break;
        }}
    }

    Ꮡa.Printf("\n"u8);
    Ꮡa.Printf("// %s\n"u8, decl);
    Ꮡa.Printf("TEXT ·%s(SB), NOSPLIT, $0\n"u8, name);
    if (a.Arch.setup != nil) {
        a.Arch.setup(f);
    }
    return f;
}

// Arg allocates a new register, copies the named argument (or result) into it,
// and returns that register.
public static ΔReg Arg(this ref ΔFunc f, @string name) {
    return f.ArgHint(name, HintNone);
}

// ArgHint is like Arg but uses a register allocation hint.
public static ΔReg ArgHint(this ref ΔFunc f, @string name, Hint hint) {
    var (off, ok) = f.args[name, ꟷ];
    if (!ok) {
        f.Asm.Fatalf("unknown argument %s"u8, name);
    }
    var mem = new ΔReg(fmt.Sprintf("%s+%d(FP)"u8, name, off));
    if (hint == HintMemOK && f.Asm.Arch.memOK) {
        return mem;
    }
    var r = f.Asm.RegHint(hint);
    f.Asm.Mov(mem, r);
    return r;
}

// ArgPtr is like Arg but returns a RegPtr.
public static RegPtr ArgPtr(this ref ΔFunc f, @string name) {
    return (RegPtr)f.Arg(name);
}

// StoreArg stores src into the named argument (or result).
public static void StoreArg(this ref ΔFunc f, ΔReg src, @string name) {
    var (off, ok) = f.args[name, ꟷ];
    if (!ok) {
        f.Asm.Fatalf("unknown argument %s"u8, name);
    }
    var a = f.Asm;
    var mem = new ΔReg(fmt.Sprintf("%s+%d(FP)"u8, name, off));
    if (src.IsImm() && !a.Arch.memOK) {
        var r = a.Reg();
        a.Mov(src, r);
        a.Mov(r, mem);
        a.Free(r);
        return;
    }
    a.Mov(src, mem);
}

} // end asmgen_package
