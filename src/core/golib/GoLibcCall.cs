// GoLibcCall.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace go;

/// <summary>
/// How a libc call reports failure — the three conventions Go's darwin keystone assembly implements,
/// read off <c>sys_darwin_amd64.s</c> (2026-09-03): <c>syscall</c>/<c>syscall6</c>/<c>syscall9</c>
/// compare the LOW 32 BITS of the result against -1 (<c>CMPL</c>), <c>syscallX</c>/<c>syscall6X</c>
/// compare all 64 (<c>CMPQ</c>), and <c>syscallPtr</c> treats a NULL result as the error
/// (<c>TESTQ</c>). Only on failure is <c>__error()</c> consulted.
/// </summary>
public enum GoLibcErrnoRule
{
    /// <summary>The call cannot fail in the errno sense; errno is never read.</summary>
    None,
    /// <summary><c>CMPL AX, $-1</c>: failure when the low 32 bits of the result are -1.</summary>
    Int32MinusOne,
    /// <summary><c>CMPQ AX, $-1</c>: failure when the whole result is -1.</summary>
    Int64MinusOne,
    /// <summary><c>TESTQ AX, AX</c>: failure when the result is NULL.</summary>
    NullPointer,
}

/// <summary>
/// The darwin keystone's DISPATCH, platform-neutral so the fleet can measure it: invokes an already
/// resolved libc entry point with integer arguments under the C calling convention and applies Go's
/// own errno conventions to the result.
/// </summary>
/// <remarks>
/// <para>
/// Design record: docs/phase4/DESIGN-darwin-run-layer-2.md §7. Go reaches libc on darwin through
/// per-call-site assembly trampolines; the managed model has no assembly, so the address the class-B
/// resolver hands back (<see cref="GoCgoDynamicImports"/>) is called from here instead. The struct
/// marshalling those trampolines perform is an artifact of Go's ABI — the arguments arrive as
/// ordinary parameters on the <c>syscall</c> package's own signatures, or as the fields of a lifted
/// args struct on <c>runtime.libcCall</c>'s, and both are handled below.
/// </para>
/// <para>
/// Nothing here is darwin-specific. The same code calls glibc on Linux, which is how it is guarded
/// (<c>GolibTests/LibcCallDispatchTests.cs</c>): the mechanism — arity, result width, the errno
/// read through a resolved reader — is measured on every fleet host, and only the libSystem
/// resolution and the trampolines' reachability remain a mac's to confirm.
/// </para>
/// <para>
/// What is deliberately NOT modelled, stated so the gap is a fact rather than a surprise: the second
/// return register (<c>r2</c>, <c>DX</c>) is reported as 0 — no darwin syscall wrapper in the corpus
/// reads it (censused over <c>zsyscall_darwin_amd64.cs</c>: 0 readers); floating-point arguments are
/// refused (one lifted struct carries one, <c>crypto_x509_syscall_args</c>, and it sits behind a
/// class-C entry); and a variadic callee on Apple silicon, whose variadic tail travels on the stack
/// rather than in registers, is called with the fixed-register convention this signature family
/// implies — correct on amd64, where the corpus's darwin flavour lives, and recorded beside the
/// amd64-only debt for arm64.
/// </para>
/// </remarks>
public static unsafe class GoLibcCall
{
    /// <summary>The widest keystone entry: <c>Syscall9</c>.</summary>
    public const int MaxArgs = 9;

    /// <summary>
    /// Calls <paramref name="fn"/> with <paramref name="args"/> as integer-register arguments and
    /// returns the integer result, reading errno through <paramref name="errnoReader"/> when
    /// <paramref name="rule"/> says the call failed.
    /// </summary>
    /// <param name="fn">The resolved entry point — never a synthetic PC, never zero.</param>
    /// <param name="args">Up to <see cref="MaxArgs"/> integer arguments, each already widened to a register.</param>
    /// <param name="rule">Which of Go's three failure tests applies.</param>
    /// <param name="errnoReader">The resolved <c>__error</c> (darwin) or <c>__errno_location</c> (glibc): a function returning <c>int*</c>.</param>
    /// <param name="errno">The errno value on failure, 0 otherwise — sign-extended from the int the reader points at, as Go's <c>MOVLQSX</c> does.</param>
    /// <returns>The call's integer result register.</returns>
    public static nuint Call(nint fn, ReadOnlySpan<nuint> args, GoLibcErrnoRule rule, nint errnoReader, out nuint errno)
    {
        if (fn == 0)
            throw new ArgumentException("go2cs: libc call through a null function pointer — the address was never resolved", nameof(fn));

        if (args.Length > MaxArgs)
            throw new ArgumentException($"go2cs: libc call with {args.Length} arguments; the keystone family tops out at {MaxArgs}", nameof(args));

        nuint r = args.Length switch
        {
            0 => ((delegate* unmanaged[Cdecl]<nuint>)fn)(),
            1 => ((delegate* unmanaged[Cdecl]<nuint, nuint>)fn)(args[0]),
            2 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint>)fn)(args[0], args[1]),
            3 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2]),
            4 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3]),
            5 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3], args[4]),
            6 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3], args[4], args[5]),
            7 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
            8 => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]),
            _ => ((delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]),
        };

        bool failed = rule switch
        {
            GoLibcErrnoRule.Int32MinusOne => unchecked((int)(uint)r) == -1,
            GoLibcErrnoRule.Int64MinusOne => unchecked((long)(ulong)r) == -1L,
            GoLibcErrnoRule.NullPointer => r == 0,
            _ => false,
        };

        errno = failed ? ReadErrno(errnoReader) : 0;
        return r;
    }

    /// <summary>
    /// Reads the current thread's errno through a resolved reader — <c>__error</c> on darwin,
    /// <c>__errno_location</c> on glibc — both of which return <c>int*</c>. Sign-extended, as Go's
    /// <c>MOVLQSX (AX), AX</c> is.
    /// </summary>
    public static nuint ReadErrno(nint errnoReader)
    {
        if (errnoReader == 0)
            throw new InvalidOperationException("go2cs: a libc call failed but no errno reader (__error / __errno_location) was resolved to report why");

        int* location = ((delegate* unmanaged[Cdecl]<int*>)errnoReader)();
        return unchecked((nuint)(nint)(*location));
    }

    // Field names Go's own sys_darwin.go uses for a trampoline's outputs; everything before the first
    // of these, in declaration order, is an input. Preserved verbatim by the converter, so they are
    // Go's names, not a guess: `ret`/`r1`/`ret1` receive the result register, `errno`/`err`/`ret2`
    // receive errno on failure, `r2` the second return register.
    private static readonly string[] s_resultFields = { "ret", "r1", "ret1" };
    private static readonly string[] s_errnoFields = { "errno", "err", "ret2" };
    private static readonly string[] s_secondResultFields = { "r2" };

    /// <summary>
    /// The <c>runtime.libcCall</c> protocol: <paramref name="argsBox"/> is the managed box of a
    /// reference-free lifted args struct whose LEADING fields are the call's integer arguments and
    /// whose trailing <c>ret</c>/<c>errno</c>-family fields receive the outcome — the layout Go's
    /// trampoline assembly unpacks by offset, read here off the recovered type by reflection.
    /// </summary>
    /// <param name="fn">The resolved libc entry point the trampoline stands for.</param>
    /// <param name="argsBox">The <c>ж&lt;T&gt;</c> box <c>ManagedPointerTokens.Resolve</c> recovered for the call's argument pointer.</param>
    /// <param name="errnoReader">The resolved <c>__error</c>.</param>
    /// <param name="symbol">The symbol's name, for the refusal messages only.</param>
    /// <returns>
    /// The result register <see cref="Call"/> handed back — what Go's <c>asmcgocall</c> returns as
    /// <c>libcCall</c>'s <c>int32</c> (its low 32 bits), which twenty <c>sys_darwin.cs</c> callers read
    /// (<c>kqueue</c>'s descriptor, <c>closefd</c>'s verdict, <c>kevent</c>'s count, the pthread
    /// family's rc) beside the <c>ret</c> field the lifted family reads. Reported as 0 until darwin
    /// increment 7 (2026-09-05, shape (d) of the Q56 census).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The box is not a Go pointer, its struct carries a field this dispatcher cannot place in a
    /// register (a managed reference, a floating-point value, an unknown width) or a result field of
    /// an unsupported kind. Every refusal names the symbol; none returns a value.
    /// </exception>
    public static nuint DispatchArgsStruct(nint fn, object? argsBox, nint errnoReader, string symbol)
    {
        // A null box is Go's ZERO-ARGUMENT trampoline — `libcCall(fn, nil)`: kqueue, issetugid —
        // nothing to place, nothing to write back; the register is the whole outcome.
        if (argsBox is null)
            return Call(fn, ReadOnlySpan<nuint>.Empty, GoLibcErrnoRule.None, errnoReader, out _);

        if (argsBox is not INilPointer || argsBox is not IUntypedSlotAccess slot)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the argument does not resolve to a managed Go pointer box (got {argsBox?.GetType().FullName ?? "null"})");

        Type? argsType = PointeeTypeOf(argsBox.GetType());

        if (argsType is null || !argsType.IsValueType)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the argument box does not point at a struct ({argsBox.GetType().FullName})");

        if (!slot.TryLoadThrough(out object? value) || value is null)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the argument box is nil");

        FieldInfo[] fields = argsType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Array.Sort(fields, static (x, y) => x.MetadataToken.CompareTo(y.MetadataToken));

        Span<nuint> args = stackalloc nuint[MaxArgs];
        int argCount = 0;
        FieldInfo? result = null, errnoField = null, secondResult = null;

        foreach (FieldInfo field in fields)
        {
            if (Array.IndexOf(s_resultFields, field.Name) >= 0)
            {
                result = field;
                continue;
            }

            if (Array.IndexOf(s_errnoFields, field.Name) >= 0)
            {
                errnoField = field;
                continue;
            }

            if (Array.IndexOf(s_secondResultFields, field.Name) >= 0)
            {
                secondResult = field;
                continue;
            }

            if (result is not null || errnoField is not null)
                throw new InvalidOperationException($"go2cs: libcCall({symbol}): field '{field.Name}' of {argsType.Name} follows a result field; the trampoline protocol is inputs first, outputs last");

            if (argCount == MaxArgs)
                throw new InvalidOperationException($"go2cs: libcCall({symbol}): {argsType.Name} has more than {MaxArgs} input fields");

            args[argCount++] = ToRegister(field.GetValue(value), field, argsType, symbol);
        }

        GoLibcErrnoRule rule = result is null ? GoLibcErrnoRule.None : RuleFor(result.FieldType, argsType, symbol);

        nuint r = Call(fn, args[..argCount], rule, errnoReader, out nuint errno);

        result?.SetValue(value, FromRegister(r, result.FieldType, result, argsType, symbol));
        errnoField?.SetValue(value, FromRegister(errno, errnoField.FieldType, errnoField, argsType, symbol));
        secondResult?.SetValue(value, FromRegister(0, secondResult.FieldType, secondResult, argsType, symbol));

        if (!slot.TryStoreThrough(value))
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the outcome could not be stored back through the argument box");

        return r;
    }

    /// <summary>
    /// The BLOCK-ADDRESS-AS-ARGUMENT form of the per-symbol table (docs/phase4/DESIGN-cgo-unsafe-args-block-lift.md
    /// §3.3, <c>walltime</c>): the trampoline passes the block ITSELF by address, behind
    /// <paramref name="leading"/> constant registers — <c>clock_gettime(CLOCK_REALTIME, &amp;t)</c> — and libc
    /// writes the result into that storage, so nothing is read back here.
    /// </summary>
    /// <param name="blockAddress">
    /// The block's number: the pinned address of reference-free storage. A number that is a box's ORDER
    /// TOKEN (reference-bearing storage, Q44) is refused by name — libc would write into an address that
    /// names nothing; a native mirror at the seam is that pointee's remedy.
    /// </param>
    public static nuint CallWithBlockAddress(nint fn, nuint blockAddress, ReadOnlySpan<nuint> leading, nint errnoReader, string symbol)
    {
        if (blockAddress == 0)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the block passed by address is nil");

        if (ManagedPointerTokens.Resolve(blockAddress) is INilPointer box && box.PointerOrderToken == blockAddress)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the block passed by address is reference-bearing managed storage — its number is an order token, not an address, and libc cannot write into it; a native mirror at the seam is the remedy");

        if (leading.Length >= MaxArgs)
            throw new ArgumentException($"go2cs: libcCall({symbol}): {leading.Length} leading registers leave no room for the block address", nameof(leading));

        Span<nuint> args = stackalloc nuint[leading.Length + 1];
        leading.CopyTo(args);
        args[leading.Length] = blockAddress;

        return Call(fn, args, GoLibcErrnoRule.None, errnoReader, out _);
    }

    /// <summary>
    /// The RESULT-STORED-INTO-BLOCK form (§3.3, <c>pthread_self</c>): the trampoline calls with no
    /// arguments and stores the result register into the block's FIRST WORD — <c>MOVQ AX, 0(BX)</c>. The
    /// block is a named-integer wrapper (<c>pthread</c>: one field) or a scalar box; the first field, or
    /// the scalar itself, receives the register at its own width and is stored back through the box.
    /// </summary>
    public static nuint CallStoringResultIntoBlock(nint fn, object? argsBox, nint errnoReader, string symbol)
    {
        if (argsBox is not INilPointer || argsBox is not IUntypedSlotAccess slot)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the result block does not resolve to a managed Go pointer box (got {argsBox?.GetType().FullName ?? "null"})");

        Type? blockType = PointeeTypeOf(argsBox.GetType());

        if (blockType is null)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the result block's box has no pointee type ({argsBox.GetType().FullName})");

        if (!slot.TryLoadThrough(out object? value) || value is null)
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the result block box is nil");

        nuint r = Call(fn, ReadOnlySpan<nuint>.Empty, GoLibcErrnoRule.None, errnoReader, out _);
        object? stored = RegisterAs(r, blockType);

        if (stored is null)
        {
            FieldInfo[] fields = blockType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fields.Length == 0)
                throw new InvalidOperationException($"go2cs: libcCall({symbol}): the result block {blockType.Name} has no first word to store into");

            Array.Sort(fields, static (x, y) => x.MetadataToken.CompareTo(y.MetadataToken));
            FieldInfo first = fields[0];

            first.SetValue(value, RegisterAs(r, first.FieldType) ?? throw new InvalidOperationException(
                $"go2cs: libcCall({symbol}): the result block's first word '{first.Name}' of {blockType.Name} is a {first.FieldType.Name}, which this dispatcher cannot fill from an integer register"));

            stored = value;
        }

        if (!slot.TryStoreThrough(stored))
            throw new InvalidOperationException($"go2cs: libcCall({symbol}): the result could not be stored back through the block's box");

        return r;
    }

    // A register read at a scalar type's own width, or null when the type is not a register type.
    private static object? RegisterAs(nuint r, Type type)
    {
        if (type == typeof(int)) return unchecked((int)(uint)r);
        if (type == typeof(uint)) return unchecked((uint)r);
        if (type == typeof(long)) return unchecked((long)(ulong)r);
        if (type == typeof(ulong)) return unchecked((ulong)r);
        if (type == typeof(nint)) return unchecked((nint)r);
        if (type == typeof(nuint)) return r;
        if (type == typeof(uintptr)) return (uintptr)r;

        return null;
    }

    // The T of a ж<T> subclass (StandardBox<T>, NativeBox<T>, ...): walk the base chain to ж<>.
    private static Type? PointeeTypeOf(Type boxType)
    {
        for (Type? t = boxType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ж<>))
                return t.GetGenericArguments()[0];
        }

        return null;
    }

    // Go's trampolines load a 32-bit field with MOVL (zero-extending into the register) and a
    // 64-bit one with MOVQ; the callee reads only its own width. Reproduced exactly: an int32 -1
    // becomes 0x00000000FFFFFFFF, which a callee expecting an int reads as -1.
    private static nuint ToRegister(object? fieldValue, FieldInfo field, Type argsType, string symbol)
    {
        return fieldValue switch
        {
            int i => unchecked((nuint)(uint)i),
            uint u => u,
            long l => unchecked((nuint)(ulong)l),
            ulong ul => unchecked((nuint)ul),
            nint n => unchecked((nuint)n),
            nuint nu => nu,
            uintptr up => (nuint)up,
            short s => unchecked((nuint)(ushort)s),
            ushort us => us,
            sbyte sb => unchecked((nuint)(byte)sb),
            byte b => b,
            bool flag => flag ? 1u : 0u,
            _ => throw new InvalidOperationException(
                $"go2cs: libcCall({symbol}): field '{field.Name}' of {argsType.Name} is a {field.FieldType.Name}, which this dispatcher cannot place in an integer register (a managed reference, a float, or an unknown width) — the per-symbol layout record is the remedy"),
        };
    }

    private static object FromRegister(nuint r, Type fieldType, FieldInfo field, Type argsType, string symbol)
    {
        if (fieldType == typeof(int)) return unchecked((int)(uint)r);
        if (fieldType == typeof(uint)) return unchecked((uint)r);
        if (fieldType == typeof(long)) return unchecked((long)(ulong)r);
        if (fieldType == typeof(ulong)) return unchecked((ulong)r);
        if (fieldType == typeof(nint)) return unchecked((nint)r);
        if (fieldType == typeof(nuint)) return r;
        if (fieldType == typeof(uintptr)) return (uintptr)r;

        throw new InvalidOperationException(
            $"go2cs: libcCall({symbol}): result field '{field.Name}' of {argsType.Name} is a {fieldType.Name}, which this dispatcher cannot fill from an integer register");
    }

    // The failure test is decided by the result field's width, as it is in Go's assembly: a 32-bit
    // result is compared with CMPL (fcntl_args.ret is int32, and fcntl_trampoline compares... CMPQ —
    // but a 32-bit field can only ever hold -1 as 0xFFFFFFFF, so both tests agree on it), a 64-bit
    // or pointer-sized one with CMPQ.
    private static GoLibcErrnoRule RuleFor(Type resultType, Type argsType, string symbol)
    {
        if (resultType == typeof(int) || resultType == typeof(uint))
            return GoLibcErrnoRule.Int32MinusOne;

        if (resultType == typeof(long) || resultType == typeof(ulong) || resultType == typeof(nint) || resultType == typeof(nuint) || resultType == typeof(uintptr))
            return GoLibcErrnoRule.Int64MinusOne;

        throw new InvalidOperationException($"go2cs: libcCall({symbol}): result field of {argsType.Name} is a {resultType.Name}; no errno rule applies");
    }
}
