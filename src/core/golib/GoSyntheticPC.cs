// GoSyntheticPC.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace go;

// ---------------------------------------------------------------------------------------------
// SYNTHETIC PROGRAM COUNTERS — a token that stands for a function in a table, and never anything
// more. Design record: docs/phase4/DESIGN-synthetic-pc-registry.md.
//
// WHY THERE IS A FILE HERE AT ALL
//   The converted corpus has no program-counter → name mapping. `internal/abi`'s FuncPCABI0 and
//   FuncPCABIInternal answered `default` — every function's PC was 0 — and the read-back side
//   (`runtime/symtab.cs`, `len(datap.pclntable) == 0 → continue`) can never resolve one, so the
//   hole was invisible: plausible, silent, and wrong. A token makes the mapping exist for the
//   functions that HAVE a managed body, which is the only population it can honestly serve.
//
// WHAT A TOKEN PROMISES, AND THE ONE THING IT IS NOT
//   Unique per function, stable for the life of the process, and NEVER DEREFERENCED. It is not a
//   code address and it addresses nothing. That last property is enforced rather than asserted:
//   tokens are minted from the canonical HIGH half of the address space on 64-bit, which is the
//   kernel half on x86-64 and TTBR1 on arm64 — unmapped in user mode on both — so a caller that
//   dereferences one faults immediately and loudly instead of reading a stranger's memory. This
//   is deliberate: the failure mode that produced this file was a plausible value, and the range
//   is chosen so that misuse can never again be plausible.
//
// WHY A SPAN PER FUNCTION RATHER THAN A SINGLE VALUE
//   Callers do arithmetic on a PC and expect the result to stay inside the same function. The
//   corpus writes `abi.FuncPCABI0(goexit) + sys.PCQuantum` (runtime/proc.cs) and
//   `abi.FuncPCABIInternal(lostProfileEvent) + 1` (runtime/pprof/proto.cs). A one-token-per-
//   function map resolves neither, so each function owns a Stride-sized span and resolution masks
//   the offset away — the same thing Go's own pclntab does for a PC in the middle of a function.
//
// WHY A LOCK AND NOT ConcurrentDictionary.GetOrAdd
//   GoDelegateSynthesis's reason exactly: GetOrAdd runs its factory CONCURRENTLY on racing threads
//   and discards the losers' work. Here the factory allocates the next index, so a discarded loser
//   does not merely waste work — it would publish two tokens for one function on the losing thread
//   and break the uniqueness the whole mechanism rests on.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Mints and resolves synthetic program counters for converted Go functions.
/// </summary>
public static class GoSyntheticPC
{
    // 4 KiB per function. Large enough that no caller's PC arithmetic (`+1`, `+PCQuantum`) leaves
    // the function's own span; small enough that the 64-bit space holds 2^35 of them.
    private const int StrideShift = 12;

    private static readonly nuint s_stride = (nuint)1 << StrideShift;

    // The canonical high half — unmapped in user mode on x86-64 (kernel half) and arm64 (TTBR1).
    //
    // 64-BIT ONLY, AND THE 32-BIT ARM THROWS RATHER THAN NARROWING. An earlier draft used the top
    // 256 MiB (0xF000_0000) on a 32-bit runtime. That is WRONG and the census of the corpus's token
    // spaces found it: ManagedPointerTokens then minted `(nuint)(uint)RuntimeHelpers.GetHashCode(o)`
    // (ж.Contracts.cs; identity tokens in 0xC000_0000_0000_0000 since the hash-token cut, which needs
    // 64 bits too), an unconstrained 32-bit value that can and does exceed 0xF000_0000, so on a
    // 32-bit runtime the two spaces OVERLAP and a resolver consulting both would answer a pointer
    // token as a function. The corpus is 64-bit, so the collision is unreachable — which is exactly
    // why it is refused loudly at the mint instead of left latent for whoever first builds 32-bit.
    // A range-independent discriminator was the alternative and was declined: machinery for a
    // platform nobody builds (COORD ruling, 2026-09-03). See DESIGN-pc-readback.md §2.1.
    private static readonly nuint s_base = unchecked((nuint)0xFFFF_8000_0000_0000UL);

    private static readonly nuint s_capacity = (nuint.MaxValue - s_base) >> StrideShift;

    private static readonly object s_gate = new();
    private static readonly Dictionary<RuntimeMethodHandle, nuint> s_tokens = [];
    private static readonly List<MethodBase> s_functions = [];

    /// <summary>
    /// Returns the synthetic PC of the function <paramref name="target"/> was formed over, minting
    /// one on first request. Two delegates over one method — a bare method group and a closure
    /// alike — answer the same PC, because a function PC identifies the function and not the value.
    /// </summary>
    public static nuint Of(Delegate target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return Of(target.Method);
    }

    /// <summary>
    /// Returns the synthetic PC of <paramref name="method"/>, minting one on first request.
    /// </summary>
    public static nuint Of(MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (IntPtr.Size != 8)
        {
            throw new PlatformNotSupportedException(
                "go2cs: synthetic program counters require a 64-bit runtime — on 32 bits the token " +
                "range overlaps ManagedPointerTokens and a resolver could answer a pointer token as " +
                "a function");
        }

        RuntimeMethodHandle handle = method.MethodHandle;

        lock (s_gate)
        {
            if (s_tokens.TryGetValue(handle, out nuint existing))
                return existing;

            if ((nuint)s_functions.Count >= s_capacity)
                throw new InvalidOperationException($"go2cs: the synthetic PC space is exhausted at {s_functions.Count} functions");

            nuint token = s_base + ((nuint)s_functions.Count << StrideShift);

            s_functions.Add(method);
            s_tokens.Add(handle, token);

            return token;
        }
    }

    /// <summary>
    /// Reports whether <paramref name="pc"/> lies in the synthetic range and names a function this
    /// registry has minted.
    /// </summary>
    public static bool IsSynthetic(nuint pc) => Resolve(pc) is not null;

    /// <summary>
    /// Returns the function <paramref name="pc"/> falls within, or <c>null</c> if it is not a
    /// synthetic PC. A PC anywhere inside a function's span resolves to that function, so the
    /// arithmetic callers do on a PC does not lose it.
    /// </summary>
    public static MethodBase? Resolve(nuint pc)
    {
        if (pc < s_base)
            return null;

        nuint index = (pc - s_base) >> StrideShift;

        lock (s_gate)
        {
            return index < (nuint)s_functions.Count ? s_functions[(int)index] : null;
        }
    }

    /// <summary>
    /// Returns the Go name of the function <paramref name="pc"/> falls within — import-path
    /// qualified, as <c>runtime.funcname</c> spells it (<c>internal/abi.FuncPCTestFn</c>) — or
    /// <c>null</c> if <paramref name="pc"/> is not synthetic.
    /// </summary>
    /// <remarks>
    /// File and line are deliberately NOT composed here. They come from the <c>[GoPositionMap]</c>
    /// records the corpus already carries, which is why this registry needs no pclntab and proposes
    /// none — a synthetic PC answers "which function", never "which instruction".
    /// </remarks>
    public static string? NameOf(nuint pc) => Resolve(pc) is { } method ? GoNameOf(method) : null;

    /// <summary>
    /// Composes the import-path-qualified Go name of a converted function.
    /// </summary>
    public static string GoNameOf(MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);

        Type? owner = method.DeclaringType;

        if (owner is null)
            return method.Name;

        string name = method.Name;

        // An sstring twin's canonical value delegate (`Sprintfᶠ`) is a lambda. Its method is a
        // compiler-generated `<.cctor>b__X_Y` on the nested `<>c` class, so the function's Go name is
        // RECORDED on it, and its package is the enclosing package class, not the closure class
        // (docs/phase4/DESIGN-sstring-twin-pilot.md §3.4).
        if (method.GetCustomAttribute<GoTwinForwarderAttribute>(inherit: false) is { GoName: { } goName })
        {
            name = goName;

            while (owner.DeclaringType is { } outer)
                owner = outer;
        }

        // A converted package is the class `<pkg>_package` in namespace `go[.<path segments>]`, so
        // the import path is the namespace below `go` with the package's own name appended:
        // `go.internal` + `abi_package` → `internal/abi`. (`go.@internal` in source is only an
        // escape; the runtime namespace string carries no `@`.)
        //
        // The two TEST variants are spelled the way Go spells them, which is not the same rule for
        // both: an INTERNAL test file compiles into the package itself, so `abi_internal_test_package`
        // is `internal/abi`; an EXTERNAL one is its own package, so `abi_test_package` keeps the
        // `_test` and is `internal/abi_test`. Getting this wrong would not fail loudly — it would
        // quietly name a frame something Go never prints.
        //
        // A stamped VERBATIM import path is final and is read first: the namespace cannot carry a '.'
        // inside a segment, a major-version directory, or a name that differs from its directory
        // (GoPackageAttribute.ImportPath). The converter stamps each test variant with the path Go
        // gives it, so neither suffix rule below applies to a stamped class.
        if (GoReflect.GoPackageImportPathOf(owner) is { } verbatim)
            return $"{verbatim}.{name}";

        string package = owner.Name;

        if (package.EndsWith(PackageSuffix, StringComparison.Ordinal))
            package = package[..^PackageSuffix.Length];

        if (package.EndsWith(InternalTestSuffix, StringComparison.Ordinal))
            package = package[..^InternalTestSuffix.Length];

        string? space = owner.Namespace;

        if (space is null || space.Length == 0)
            return $"{package}.{name}";

        string prefix = space == GoRoot
            ? ""
            : space.StartsWith(GoRootDot, StringComparison.Ordinal)
                ? space[GoRootDot.Length..].Replace('.', '/') + "/"
                : space.Replace('.', '/') + "/";

        return $"{prefix}{package}.{name}";
    }

    private const string PackageSuffix = "_package";
    private const string InternalTestSuffix = "_internal_test";
    private const string GoRoot = "go";
    private const string GoRootDot = "go.";
}
